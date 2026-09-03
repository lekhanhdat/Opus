/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */



namespace AvePoint.GCommon.FileTransfer
{
    #region using directives
    using System;
    using System.Globalization;
    using System.Text;
    using GCommon;
    using Network;
    using System.Reflection;
    using System.Threading;
    using I18N;

    #endregion

    public class BlockSender : IDisposable
    {
        readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        IAveNetwork network;
        readonly bool supportReader;
        readonly AveConnectionOptions connectionOptions;
        long totalSentSinceLastSync;
        DateTime lastSyncSuccessfulTime = DateTime.MinValue;

        readonly AveDataBlockQueue sendingQueue;
        Thread sendThread;
        volatile bool sendingThreadExitByCloseConnection;

        Thread keepAliveThread;
        volatile bool isKeepAliveStopped;
        readonly object stopKeepAliveSyncRoot = new object();
        readonly object pausedSyncRoot = new object();

        bool isPaused;

        readonly string loggingSessionId = Guid.NewGuid().ToString();

        public int Available { get { return network.Available; } }

        [Obsolete]
        public BlockSender(IAveNetwork aveNetwork, bool supportReader = false, int sendingQueueSize = 100)
        {
            this.network = aveNetwork;
            this.supportReader = supportReader;
            this.connectionOptions = new AveConnectionOptions { DataBlockQueueSize = sendingQueueSize };
            sendingQueue = new AveDataBlockQueue(sendingQueueSize) { Name = "Block Sender Queue" };
        }

        public BlockSender(IAveNetwork aveNetwork, AveConnectionOptions connOptions, bool supportReader = false)
        {
            this.network = aveNetwork;
            this.supportReader = supportReader;
            this.connectionOptions = connOptions;
            sendingQueue = new AveDataBlockQueue(this.connectionOptions.DataBlockQueueSize) { Name = "Block Sender Queue" };
        }

        public void ReceiveDataBlock(AveDataBlock dataBlock)
        {
            network.ReceiveDataBlock(dataBlock);
        }

        public string ReceiveMessage()
        {
            return network.ReceiveMessage();
        }

        public void SendMessage(string msg)
        {
            network.SendMessage(msg);
        }

        public void Close()
        {
            if (lastSyncSuccessfulTime != DateTime.MinValue)
                logger.Info("Blocker sender last synchronization time: {0}", lastSyncSuccessfulTime.ToString(CultureInfo.InvariantCulture));
            if (sendingThreadExitByCloseConnection)
            {
                //logger.Debug("BlockSender is waiting for close connection data block. session ID: {0}", loggingSessionId);
                logger.Info(CommonResources.BlockSenderCloseWaitingCloseBLK, loggingSessionId);
                var exitBlock = new AveDataBlock();
                while (true)
                {
                    network.ReceiveDataBlock(exitBlock);
                    if (exitBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE) break;
                    //logger.Debug("BlockSender discard one data block while waiting for close connection data block. block type: {0} session ID: {1}", exitBlock.Type, loggingSessionId);
                    logger.Info(CommonResources.BlockSenderCloseDiscardBLK, exitBlock.Type, loggingSessionId);
                }
                //logger.Debug("BlockSender got close connection data block.  session ID: {0}", loggingSessionId);
                logger.Info(CommonResources.BlockSenderCloseGotCloseBLK, loggingSessionId);
                var closeMessage = exitBlock.RetrieveString();
                if (!string.IsNullOrEmpty(closeMessage))
                {
                    throw new ClosedWithErrorException(closeMessage);
                }
            }
            else
            {
                this.sendingQueue.CheckException();
            }

            if (network != null)
            {
                network.Close();
                network = null;
            }
        }

        /// <summary>
        /// Start the sending process. The sender takes a data block
        /// from sending queue and send over network, then put the
        /// data block into sent queue.
        /// </summary>
        public void Start(string jobId = "")
        {
            string currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
            }
            sendThread = new Thread(SendProcess) { Name = currentThreadId + "_SendBlockThread", IsBackground = true };
            sendThread.Start(string.IsNullOrEmpty(jobId) ? null : jobId);

            keepAliveThread = new Thread(KeepAlive) { Name = currentThreadId + "_BlockSender keep alive", IsBackground = true };
            keepAliveThread.Start(string.IsNullOrEmpty(jobId) ? null : jobId);
        }

        public void WaitForSendCompleted(int timeout)
        {
            keepAliveThread.Join(timeout);
            sendThread.Join(timeout);
        }

        public void Pause()
        {
            if (!isPaused)
            {
                //logger.Debug("BlockSender is pausing. session ID: {0}", loggingSessionId);
                logger.Info(CommonResources.BlockSenderPauseStarting, loggingSessionId);
                isPaused = true;
            }
        }

        public void Resume()
        {
            lock (pausedSyncRoot)
            {
                if (isPaused)
                {
                    //logger.Debug("BlockSender is resuming. session ID: {0}", loggingSessionId);
                    logger.Info(CommonResources.BlockSenderResumeStarting, loggingSessionId);
                    isPaused = false;
                    Monitor.PulseAll(pausedSyncRoot);
                }
            }
        }

        /// <summary>
        /// Loop each data block in sending queue and send the data block.
        /// </summary>
        private void SendProcess(object o = null)
        {
            try
            {
                if (o != null)
                    AveLogger.SetThreadJobId(o as string);

                while (true)
                {
                    lock (pausedSyncRoot)
                    {
                        if (isPaused)
                        {
                            //logger.Debug("BlockSender is paused. session ID: {0}", loggingSessionId);
                            logger.Info(CommonResources.BlockSenderSendProcessPaused, loggingSessionId);
                            Monitor.Wait(pausedSyncRoot, 1800 * 1000);
                            //logger.Debug("BlockSender is resumed. session ID: {0}", loggingSessionId);
                            logger.Info(CommonResources.BlockSenderSendProcessResumed, loggingSessionId);
                        }
                    }

                    var sendingBlock = sendingQueue.TakeWorkingBlock();
                    if (sendingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        //logger.Debug("BlockSender is sending close connection data block. session ID: {0}", loggingSessionId);
                        logger.Info(CommonResources.BlockSenderSendProcessSendingCloseBLK, loggingSessionId);
                        StopKeepAlive();
                    }
                    if (sendingBlock.Type == AveDataBlockType.ALIVE_TYPE)
                    {
                        //logger.Debug("BlockSender is sending keep alive data block. blockID:{0} session ID: {1}", sendingBlock.RetrieveString(), loggingSessionId);
                        logger.Info(CommonResources.BlockSenderSendProcessSendingKeepAlive, sendingBlock.RetrieveString(), loggingSessionId);
                    }

                    var dataBlockValidSize = sendingBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN;
                    if (!supportReader)
                    {
                        if (totalSentSinceLastSync + dataBlockValidSize > this.connectionOptions.SentCacheConfirmSize)
                        {
                            //sync to make sure cache is always valid
                            //Trace.WriteLine(string.Format("Sync data sending process. totalSentSinceLastSync:{0} SentCacheConfirmSize:{1} CurrentSendingBlockValidSize:{2}", totalSentSinceLastSync, this.connectionOptions.SentCacheConfirmSize, dataBlockValidSize));
                            SyncSendingProcess();
                            totalSentSinceLastSync = 0;
                        }
                    }
                    network.SendDataBlock(sendingBlock);
                    if (!supportReader)
                    {
                        totalSentSinceLastSync += dataBlockValidSize;
                    }
                    if (sendingBlock.Type == AveDataBlockType.ALIVE_TYPE)
                    {
                        //logger.Debug("BlockSender send out keep alive data block. blockID:{0} session ID: {1}", sendingBlock.RetrieveString(), loggingSessionId);
                        logger.Info(CommonResources.BlockSenderSendProcessSendOutKeepAlive, sendingBlock.RetrieveString(), loggingSessionId);
                    }
                    AveDataBlockType currentBlockType = sendingBlock.Type;
                    sendingQueue.PutFreeBlock(sendingBlock);
                    if (currentBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        //logger.Debug("BlockSender send out a close connection data block. session ID: {0}", loggingSessionId);
                        logger.Info(CommonResources.BlockSenderSendProcessSendOutCloseBLK, loggingSessionId);
                        sendingThreadExitByCloseConnection = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("An error occurred in BlockSender send thread. Exception details: {0}", ex.ToString());
                logger.Error(CommonResources.BlockSenderSendProcessErrorOccurred, ex.ToString());
                sendingQueue.SetException(ex.Message);
                StopKeepAlive();
            }
        }

        private void SyncSendingProcess()
        {
            var message = "Sync:" + Guid.NewGuid().ToString();
            var syncData = new byte[Encoding.UTF8.GetByteCount(message) + AveDataBlock.DATA_BLOCK_HEADER_LEN];
            Encoding.UTF8.GetBytes(message, 0, message.Length, syncData, AveDataBlock.DATA_BLOCK_HEADER_LEN);

            var syncRequestBlock = new AveDataBlock(syncData) { DataSize = syncData.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN, Type = AveDataBlockType.SYNC_TYPE };
            network.SendDataBlock(syncRequestBlock);
            var syncResponseBlock = new AveDataBlock();
            network.ReceiveDataBlock(syncResponseBlock);
            if (syncRequestBlock.Type != syncResponseBlock.Type
                || string.Compare(syncRequestBlock.RetrieveString(), syncResponseBlock.RetrieveString(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new SyncDataException();
            }
            lastSyncSuccessfulTime = DateTime.Now;
            //logger.Info("Sync OK");
        }
        private void KeepAlive(object o = null)
        {
            try
            {
                if (o != null)
                {
                    AveLogger.SetThreadJobId(o as string);
                }
                while (true)
                {
                    lock (stopKeepAliveSyncRoot)
                    {
                        if (isKeepAliveStopped) break;
                        if (Monitor.Wait(stopKeepAliveSyncRoot, 5 * 60 * 1000))
                        {
                            //logger.Debug("BlockSender exit keep alive.");
                            logger.Info(CommonResources.BlockSenderKeepAliveExit);
                            break;
                        }
                        if (!isKeepAliveStopped)
                        {
                            string blockId = Guid.NewGuid().ToString();
                            //logger.Debug("BlockSender is trying to put a keep alive data block. block ID:{0}", blockId);
                            logger.Info(CommonResources.BlockSenderKeepAlivePuttingBLK, blockId);
                            AveDataBlock block = GetFreeBlock();
                            block.Type = AveDataBlockType.ALIVE_TYPE;
                            block.PutString(blockId);
                            SendDataBlock(block);
                            //logger.Debug("BlockSender put a keep alive data block. block ID:{0}", blockId);
                            logger.Info(CommonResources.BlockSenderKeepAlivePutBLK, blockId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error("An error occurred in BlockSender keep alive thread. Exception details: {0}", ex.ToString());
                logger.Error(CommonResources.BlockSenderKeepAliveErrorOccurred, ex.ToString());
            }
        }

        private void StopKeepAlive()
        {
            lock (stopKeepAliveSyncRoot)
            {
                isKeepAliveStopped = true;
                Monitor.PulseAll(stopKeepAliveSyncRoot);
            }
        }

        public void SendDataBlock(AveDataBlock block)
        {
            sendingQueue.PutWorkingBlock(block);
        }

        public AveDataBlock GetFreeBlock()
        {
            return sendingQueue.TakeFreeBlock();
        }

        public void Dispose()
        {
            if (sendingQueue != null)
            {
                sendingQueue.Dispose();
            }
        }
    }
}
