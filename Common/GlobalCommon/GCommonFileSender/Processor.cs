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
using System.Reflection;
using System.Threading;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.I18N;

namespace AvePoint.GCommon.FileTransfer
{
    internal class Processor : IGeneralOutputStream
    {
        private AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IGeneralOutputStream topStream;

        public byte CompressionEncryptionFlag { get; set; }
        public DataEncryptionInfo EncryptionInfo { get; set; }
        public CompressionMethods CompressionMethod { get; set; }
        public CompressionTypes CompressionLevel { get; set; }

        private AveDataBlockQueue dataBlockInputQueue;
        private BlockSender dataBlockSender;
        private AveDataBlock sendingBlock = null;
        private Thread processThread;


        public Processor(AveDataBlockQueue inputQueue, BlockSender blockSender)
        {
            dataBlockInputQueue = inputQueue;
            dataBlockSender = blockSender;
        }

        #region 服务线程

        public void Start(string jobId = "")
        {
            //logger.Debug("Processor compression encryption flag is {0}", Convert.ToString(CompressionEncryptionFlag, 2));
            logger.Info(CommonResources.ProcessorStartLogFlag, Convert.ToString(CompressionEncryptionFlag, 2));

            topStream = this;
            if ((CompressionEncryptionFlag & GConstants.TransferFlag.AGENT_ENCRYPTED) != 0)
            {
                topStream = new SecurityOutputStream(EncryptionInfo, topStream);
            }
            if ((CompressionEncryptionFlag & GConstants.TransferFlag.AGENT_COMPRESSED) != 0)
            {
                topStream = new SecurityOutputStream(CompressionMethod, CompressionLevel, topStream);
            }

            string currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            processThread = new Thread(new ParameterizedThreadStart(Process));
            processThread.IsBackground = true;
            processThread.Name = currentThreadId + "_Processor";
            processThread.Start(string.IsNullOrEmpty(jobId) ? null : jobId);
        }

        public void WaitForProcessCompleted(int timeout)
        {
            processThread.Join(timeout);
        }

        private void Process(object o = null)
        {
            try
            {
                if (o != null)
                {
                    AveLogger.SetThreadJobId(o as string);
                }
                Boolean isNeedWrite = true;
                while (isNeedWrite)
                {
                    AveDataBlock workBlock = dataBlockInputQueue.TakeWorkingBlock();
                    switch (workBlock.Type)
                    {
                        case AveDataBlockType.HEADER_TYPE:
                            topStream.WriteHeaderXml(workBlock.RetrieveString());
                            break;
                        case AveDataBlockType.DATA_TYPE:
                            topStream.WriteMetaData(workBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, workBlock.DataSize);
                            break;
                        case AveDataBlockType.CONTENTDATA_TYPE:
                            topStream.WriteContentData(workBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, workBlock.DataSize);
                            break;
                        case AveDataBlockType.TAIL_TYPE:
                            topStream.WriteTailXml(workBlock.RetrieveString());
                            break;
                        case
                        AveDataBlockType.CLOSE_CONNECTION_TYPE:
                            logger.Info(CommonResources.ProcessorProcessGotCloseBLK);
                            topStream.Close(workBlock.RetrieveString());
                            logger.Info(CommonResources.ProcessorProcessStreamClosed);
                            isNeedWrite = false;
                            logger.Info(CommonResources.ProcessorProcessExitByCloseBLK);
                            break;

                    }
                    dataBlockInputQueue.PutFreeBlock(workBlock);
                    //while (true)
                    //{
                    //    AveDataBlock workBlock = dataBlockInputQueue.TakeWorkingBlock();
                    //    if (workBlock.Type == AveDataBlockType.HEADER_TYPE)
                    //    {
                    //        topStream.WriteHeaderXml(workBlock.RetrieveString());
                    //    }
                    //    else if (workBlock.Type == AveDataBlockType.DATA_TYPE)
                    //    {
                    //        topStream.WriteMetaData(workBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, workBlock.DataSize);
                    //    }
                    //    else if (workBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                    //    {
                    //        topStream.WriteContentData(workBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, workBlock.DataSize);
                    //    }
                    //    else if (workBlock.Type == AveDataBlockType.TAIL_TYPE)
                    //    {
                    //        topStream.WriteTailXml(workBlock.RetrieveString());
                    //    }
                    //    else if (workBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    //    {
                    //        //logger.Debug("Processor got close connection block from input queue. stream is closing.");
                    //        logger.Info(CommonResources.ProcessorProcessGotCloseBLK);
                    //        topStream.Close(workBlock.RetrieveString());
                    //        //logger.Debug("Processor stream is closed.");
                    //        logger.Info(CommonResources.ProcessorProcessStreamClosed);
                    //    }
                    //    AveDataBlockType currentBlockType = workBlock.Type;
                    //    dataBlockInputQueue.PutFreeBlock(workBlock);
                    //    if (currentBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    //    {
                    //        //logger.Debug("Processor exit by close connection block.");
                    //        logger.Info(CommonResources.ProcessorProcessExitByCloseBLK);
                    //        break;
                    //    }
                }
            }
            catch (Exception ex)
            {
                //logger.Debug("An error occurred while Processor doing process. Exception details: {0}", ex.ToString());
                logger.Info(CommonResources.ProcessorProcessErrorOccurred, ex.ToString());
                dataBlockInputQueue.SetException(ex.Message);
            }
        }

        #endregion

        #region IGeneralOutputStream members

        public void Open()
        {

        }

        public void WriteHeaderXml(string headerXml)
        {
            sendingBlock = GetSendingBlock(AveDataBlockType.HEADER_TYPE);
            sendingBlock.PutString(headerXml);
            dataBlockSender.SendDataBlock(sendingBlock);
            sendingBlock = null;
        }

        public void WriteMetaData(byte[] data, int offset, int count)
        {
            RealWrite(data, offset, count, AveDataBlockType.DATA_TYPE);
        }

        public void WriteContentData(byte[] data, int offset, int count)
        {
            RealWrite(data, offset, count, AveDataBlockType.CONTENTDATA_TYPE);
        }

        public void WriteTailXml(string tailXml)
        {
            if (sendingBlock != null)
            {
                //put cached data into queue
                dataBlockSender.SendDataBlock(sendingBlock);
                sendingBlock = null;
            }
            sendingBlock = GetSendingBlock(AveDataBlockType.TAIL_TYPE);
            sendingBlock.PutString(tailXml);
            dataBlockSender.SendDataBlock(sendingBlock);
            sendingBlock = null;
        }

        public void Close(string errorMessage)
        {
            //logger.Debug("Processor put close connection block to shut down.");
            logger.Info(CommonResources.ProcessorClosePutCloseBLK);
            sendingBlock = GetSendingBlock(AveDataBlockType.CLOSE_CONNECTION_TYPE);
            sendingBlock.PutString(errorMessage);
            dataBlockSender.SendDataBlock(sendingBlock);
            sendingBlock = null;
            //logger.Debug("Processor closed successfully.");
            logger.Info(CommonResources.ProcessorCloseSucceed);
        }

        protected void RealWrite(byte[] buf, int offset, int length, AveDataBlockType currentBlockType)
        {
            if (sendingBlock != null && sendingBlock.Type != currentBlockType)
            {
                //different type data should put into different block
                dataBlockSender.SendDataBlock(sendingBlock);
                sendingBlock = null;
            }
            if (sendingBlock == null)
            {
                sendingBlock = GetSendingBlock(currentBlockType);
            }

            int availableSpace = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN - sendingBlock.DataSize;
            while (length > availableSpace)
            {
                sendingBlock.AppendBuffer(buf, offset, availableSpace);
                dataBlockSender.SendDataBlock(sendingBlock);
                sendingBlock = null;

                offset += availableSpace;
                length -= availableSpace;
                if (length == 0) break;

                sendingBlock = GetSendingBlock(currentBlockType);
                availableSpace = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN;
            }
            if (length > 0)
            {
                ArgumentCheck.NotNull(sendingBlock, nameof(sendingBlock));
                sendingBlock.AppendBuffer(buf, offset, length);
            }
        }

        #endregion

        private AveDataBlock GetSendingBlock(AveDataBlockType blockType)
        {
            sendingBlock = dataBlockSender.GetFreeBlock();
            sendingBlock.SerialNumber = 0;
            sendingBlock.DataSize = 0;
            sendingBlock.Type = blockType;
            //sendingBlock.EncryptMethod = (byte)EncryptionMethod;
            sendingBlock.Flag = CompressionEncryptionFlag;
            return sendingBlock;
        }

    }
}