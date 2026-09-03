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
    using I18N;
    using Network;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    #endregion

    internal class BlockProcessor : IDisposable
    {
        readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IAveNetwork network;
        AveDataBlockQueue outputQueue;

        public IBlockReader BlockReader { get; private set; }

        public BlockProcessor(IAveNetwork aveNetwork, AveDataBlockQueue processorOutputQueue)
        {
            network = aveNetwork;
            outputQueue = processorOutputQueue;
        }

        public void Start(int receiverQueueSize)
        {
            this.BlockReader = new DataBlockReceiver(network, receiverQueueSize);
            this.BlockReader = new SecurityInputStream(BlockReader, true);
            this.BlockReader = new SecurityInputStream(BlockReader, false);

            var currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
            }
            var thread = new Thread(Process) { IsBackground = true, Name = currentThreadId + "_BlockProcessor" };
            thread.Start();
        }

        public void Close(string errorMessage)
        {
            BlockReader.Close(errorMessage);
        }

        private void Process()
        {
            try
            {
                while (true)
                {
                    var block = outputQueue.TakeFreeBlock();
                    BlockReader.ReadDataBlock(block);
                    var currentBlockType = block.Type;
                    outputQueue.PutWorkingBlock(block);
                    if (currentBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        //logger.Debug("BlockProcessor exit by close connection block.");
                        logger.Info(CommonResources.BlockProcessorProcessExitByCloseBLK);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Debug("An error occurred while Processor doing process. Exception details: {0}", ex.ToString());
                logger.Info(CommonResources.BlockProcessorProcessExceptionOccurred, ex.ToString());
                outputQueue.SetException(ex.Message);
            }
        }

        public void Dispose()
        {
            if (outputQueue != null)
            {
                outputQueue.Dispose();
                outputQueue = null;
            }
        }
    }
}