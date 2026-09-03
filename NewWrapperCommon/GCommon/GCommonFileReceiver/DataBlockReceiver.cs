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



using System.Globalization;

namespace AvePoint.GCommon.FileTransfer
{
    #region using directives
    using System;
    using System.Reflection;
    using System.Threading;
    using Network;
    using I18N;

    #endregion

    public class DataBlockReceiver : IBlockReader, IDisposable
    {
        readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        IAveNetwork network;
        readonly Thread dataBlockReadingThread;
        AveDataBlockQueue dataBlockCacheQueue;
        volatile bool readingThreadExitByCloseConnection;
        readonly String loggingSessionId = Guid.NewGuid().ToString();

        public DataBlockReceiver(IAveNetwork aveNetwork)
            : this(aveNetwork, 100)
        {
        }

        public DataBlockReceiver(IAveNetwork aveNetwork, int queueSize)
        {
            network = aveNetwork;
            dataBlockCacheQueue = new AveDataBlockQueue(queueSize) { Name = "DataBlock Receiver Queue" };

            var currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
            }
            dataBlockReadingThread = new Thread(DataBlockReadingThread) { Name = currentThreadId + "_DataBlockReadingThread", IsBackground = true };
            dataBlockReadingThread.Start();
        }

        #region IBlockReader Members

        /// <summary>
        /// fill the data block using network data
        /// </summary>
        /// <param name="dataBlock">the block will fill</param>
        public void ReadDataBlock(AveDataBlock dataBlock)
        {
            var block = dataBlockCacheQueue.TakeWorkingBlock();
            block.CopyTo(dataBlock);
            dataBlockCacheQueue.PutFreeBlock(block);
        }

        public void SendDataBlock(AveDataBlock sendBlock)
        {
            network.SendDataBlock(sendBlock);
        }

        public IBlockReader PrevReader
        {
            get
            {
                return null;
            }
            set
            {
                throw new ArgumentException("DataBlockReceiver must be the first reader.");
            }
        }

        public void Close(string errorMessage)
        {
            //logger.Debug("DataBlockReceiver is closing with message:{0}. session ID: {1}", errorMessage, loggingSessionID);
            logger.Debug(CommonResources.DataBlockReceiverCloseStarting, errorMessage, loggingSessionId);
            if (readingThreadExitByCloseConnection)
            {
                //logger.Debug("DataBlockReceiver is sending close connection data block. session ID: {0}", loggingSessionID);
                logger.Debug(CommonResources.DataBlockReceiverCloseSendingCloseBLK, loggingSessionId);
                var closeBlock = new AveDataBlock { Type = AveDataBlockType.CLOSE_CONNECTION_TYPE };
                closeBlock.PutString(errorMessage);
                network.SendDataBlock(closeBlock);
            }
            else
            {
                //logger.Debug("DataBlockReceiver abort reading thread. session ID: {0}", loggingSessionID);
                logger.Info(CommonResources.DataBlockReceiverCloseAbortReadingThread, loggingSessionId);
                dataBlockReadingThread.Abort();
            }

            //logger.Debug("DataBlockReceiver shutdown socket. session ID: {0}", loggingSessionID);
            logger.Debug(CommonResources.DataBlockReceiverCloseShutDownSocket, loggingSessionId);
            if (network != null)
            {
                network.Close();
                network = null;
            }
        }

        #endregion

        /// <summary>
        /// this thread read data block from network and put in the cache queue until got close connection block
        /// </summary>
        private void DataBlockReadingThread()
        {
            try
            {
                while (true)
                {
                    var dataBlock = dataBlockCacheQueue.TakeFreeBlock();
                    while (true)
                    {
                        network.ReceiveDataBlock(dataBlock);
                        if (dataBlock.Type == AveDataBlockType.ALIVE_TYPE)
                        {
                            var blockId = dataBlock.RetrieveString();
                            //logger.Debug("DataBlockReceiver cache thread drop a keep alive data block. blockID:{0} session ID: {1}", blockID, loggingSessionID);
                            logger.Info(CommonResources.DataBlockReceiverDataBlockReadingThreadDropKeepAlive, blockId, loggingSessionId);
                        }
                        else if (dataBlock.Type == AveDataBlockType.SYNC_TYPE)
                        {
                            //echo 
                            network.SendDataBlock(dataBlock);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (dataBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        //logger.Debug("DataBlockReceiver cache thread got close connection data block. session ID: {0}", loggingSessionID);
                        logger.Debug(CommonResources.DataBlockReceiverDataBlockReadingThreadGotCloseBLK, loggingSessionId);
                        readingThreadExitByCloseConnection = true;
                    }
                    var currentBlockType = dataBlock.Type;
                    dataBlockCacheQueue.PutWorkingBlock(dataBlock);
                    if (currentBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        //logger.Debug("DataBlockReceiver cache thread exited by close connection data block. session ID: {0}", loggingSessionID);
                        logger.Debug(CommonResources.DataBlockReceiverDataBlockReadingThreadExitByCloseBLK, loggingSessionId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Debug("DataBlockReceiver reading thread exit with exception details: {0} session ID: {1}", ex.ToString(), loggingSessionID);
                logger.Info(CommonResources.DataBlockReceiverDataBlockReadingThreadExitWithException, ex.ToString(), loggingSessionId);
                dataBlockCacheQueue.SetException(ex.ToString());
            }
        }

        public void Dispose()
        {
            if (dataBlockCacheQueue != null)
            {
                dataBlockCacheQueue.Dispose();
                dataBlockCacheQueue = null;
            }
        }
    }
}