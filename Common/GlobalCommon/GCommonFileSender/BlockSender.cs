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




using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using AvePoint.GCommon.Network;
using AvePoint.I18N;

namespace AvePoint.GCommon.FileTransfer
{

    public class BlockSender : IDisposable
    {
        private AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveNetwork network;

        private AveDataBlockQueue sendingQueue;
        private Thread sendThread;
        private volatile bool sendingThreadExitByCloseConnection = false;

        private Thread keepAliveThread;
        private readonly object  stopKeepAliveSyncRoot = new object();
        private volatile bool isKeepAliveStopped;

        private bool isPaused = false;
        private readonly object pausedSyncRoot = new object();

        private string loggingSessionId = Guid.NewGuid().ToString();

        public int Available { get { return network.Available; } }

        public BlockSender(IAveNetwork aveNetwork)
        {
            network = aveNetwork;
            sendingQueue = new AveDataBlockQueue();
            sendingQueue.Name = "BlockSender sending queue";
            string syncQueueTimeout = ConfigurationManager.AppSettings["syncQueueTimeout"];
            if (!string.IsNullOrEmpty(syncQueueTimeout))
            {
                sendingQueue.TimeOut = int.Parse(syncQueueTimeout);
            }
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
            if (sendingThreadExitByCloseConnection)
            {
                //logger.Debug("BlockSender is waiting for close connection data block. session ID: {0}", loggingSessionId);
                logger.Info(CommonResources.BlockSenderCloseWaitingCloseBLK, loggingSessionId);
                AveDataBlock exitBlock = new AveDataBlock();
                while (true)
                {
                    network.ReceiveDataBlock(exitBlock);
                    if (exitBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE) break;
                    //logger.Debug("BlockSender discard one data block while waiting for close connection data block. block type: {0} session ID: {1}", exitBlock.Type, loggingSessionId);
                    logger.Info(CommonResources.BlockSenderCloseDiscardBLK, exitBlock.Type, loggingSessionId);
                }
                //logger.Debug("BlockSender got close connection data block.  session ID: {0}", loggingSessionId);
                logger.Info(CommonResources.BlockSenderCloseGotCloseBLK, loggingSessionId);
                string closeMessage = exitBlock.RetrieveString();
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
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            sendThread = new Thread(new ParameterizedThreadStart(SendProcess));
            sendThread.Name = currentThreadId + "_SendBlockThread";
            sendThread.IsBackground = true;
            sendThread.Start(string.IsNullOrEmpty(jobId) ? null : jobId);

            keepAliveThread = new Thread(new ParameterizedThreadStart(KeepAlive));
            keepAliveThread.Name = currentThreadId + "_BlockSender keep alive";
            keepAliveThread.IsBackground = true;
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
                {
                    AveLogger.SetThreadJobId(o as string);
                }

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

                    AveDataBlock sendingBlock = sendingQueue.TakeWorkingBlock();
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
                    network.SendDataBlock(sendingBlock);
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
                        else
                        {
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
