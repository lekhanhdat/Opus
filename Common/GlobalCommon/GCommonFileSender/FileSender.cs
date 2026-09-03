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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.I18N;
namespace AvePoint.GCommon.FileTransfer
{
    public class FileSender : IFileSender
    {
        private AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private int dataBlockInputQueueSize = 100;
        private bool isCalculateCRC;
        private bool supportReader;
        private bool isTestRun;
        private long itemBackupDataLength;
        private AveCRC32 crcCaculator = new AveCRC32();
        private byte compressionEncryptionFlag;
        private DataEncryptionInfo encryptionInfo;
        private CompressionMethods compressionMethod = CompressionMethods.ZLIB_COMPRESSION;
        private CompressionTypes compressionLevel = CompressionTypes.Normal;

        private IAveNetwork network;
        private AveDataBlockQueue inputQueue;
        private AveDataBlock writingBlock;
        private Processor processor;
        private BlockSender dataBlockSender;
        private IFileSenderResponseWorker responseWorker;
        private ResponseReader responseReader;
        private bool? hasWriteTail;
        private bool enableSSL;
        private string sslThumbprint;
        private string mJobId = string.Empty;

        public void EnableSSL(bool enabled, string sslThumbprint = null)
        {
            this.enableSSL = enabled;
            this.sslThumbprint = sslThumbprint;
        }

        public FileSender()
        {

        }

        public FileSender(string jobId)
        {
            AveLogger.SetThreadJobId(jobId);
            mJobId = jobId;
        }

        public void SetCompressionType(CompressionTypes compressLevel)
        {
            this.compressionLevel = compressLevel;
        }

        /// <summary>
        /// Set the compression and encryption flag 
        /// </summary>
        /// <param name="flag">data mode</param>
        public void SetServerFlag(long flag)
        {
            compressionEncryptionFlag = (byte)flag;
        }

        public void SetEncryptionInfo(DataEncryptionInfo info)
        {
            encryptionInfo = info;
        }

        /// <summary>
        /// Set the input queue block count
        /// </summary>
        /// <param name="size"></param>
        public void SetQueueBufferSize(int blockCount)
        {
            dataBlockInputQueueSize = blockCount > 1 ? blockCount : dataBlockInputQueueSize;
        }

        /// <summary>
        /// Set the CRC32 flag for CRC32 verification
        /// </summary>
        /// <param name="useCRC">1 means verify CRC </param>
        public void SetCertificationFlag(int useCRC)
        {
            isCalculateCRC = useCRC == 1;
        }

        /// <summary>
        /// Set the test run flag
        /// </summary>
        /// <param name="isTestRun">true means test run</param>
        public void SetTestRunFlag(bool isTestRun)
        {
            this.isTestRun = isTestRun;
        }

        public void SetReadMessageWorker(IFileSenderResponseWorker worker)
        {
            supportReader = true;
            responseWorker = worker;
        }

        public void ReceiveDataBlock(ref AveDataBlock dataBlock)
        {
            dataBlockSender.ReceiveDataBlock(dataBlock);
        }

        /// <summary>
        /// open connection with media
        /// </summary>
        [Obsolete]
        public string Open(string host, int port, string connectInfo, string reconnectInfo)
        {
            return Open(host, port, connectInfo);
        }

        public string Open(string host, int port, string connectInfo, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            //logger.Debug("FileSender is trying to open target. host:{0} port:{1}", host, port);
            logger.Info(CommonResources.FileSenderOpenStarting, host, port);
            network = AveNetwork.Connect(host, port, enableSSL, sslThumbprint, reconnectTimeOut, reconnectInterval);
            network.SendMessage(connectInfo);
            //logger.Debug("FileSender is trying to receive response. host:{0} port:{1}", host, port);
            logger.Info(CommonResources.FileSenderOpenReceivingResponse, host, port);
            string openResult = network.ReceiveMessage();
            //logger.Debug("FileSender opened successfully.");
            logger.Info(CommonResources.FileReceiverOpenSucceed);

            this.Wrap(network);

            return openResult;
        }

        public string Open(Dictionary<string, int> mediaHosts, string connectInfo, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            DateTime deadLine = DateTime.Now.AddMilliseconds(reconnectTimeOut);
            while (true)
            {
                foreach (string host in mediaHosts.Keys)
                {
                    int port = mediaHosts[host];
                    try
                    {
                        string openResult = Open(host, port, connectInfo, 1, 1);
                        network.ReconnectRetryInterval = reconnectInterval;
                        network.ReconnectTimeout = reconnectTimeOut;
                        return openResult;
                    }
                    catch (Exception e)
                    {
                        logger.Debug("open host failed. host:{0} port:{1} Details:{2}", host, port, e.ToString());
                        if (DateTime.Now > deadLine) throw;
                        Thread.Sleep(reconnectInterval);
                    }
                }
            }
        }

        private void Wrap(IAveNetwork network)
        {
            inputQueue = new AveDataBlockQueue(dataBlockInputQueueSize);

            dataBlockSender = new BlockSender(network);
            dataBlockSender.Start(mJobId);

            processor = new Processor(inputQueue, dataBlockSender);
            processor.CompressionEncryptionFlag = compressionEncryptionFlag;
            processor.EncryptionInfo = encryptionInfo;
            processor.CompressionMethod = compressionMethod;
            processor.CompressionLevel = compressionLevel;
            processor.Start(mJobId);

            if (supportReader)
            {
                responseReader = new ResponseReader(dataBlockSender, responseWorker);
                responseReader.Start(mJobId);
            }
        }

        /// <summary>
        /// empty message means successful.
        /// </summary>
        public void Close(string message)
        {
            try
            {
                if (hasWriteTail.HasValue && !hasWriteTail.Value)
                {
                    //logger.Debug("FileSender.Close() is sending tail data block");
                    logger.Info(CommonResources.FileSenderCloseSendingTailBLK);
                    WriteTail(string.Empty);
                }
                //logger.Debug("FileSender is sending close connection data block");
                logger.Info(CommonResources.FileSenderCloseSendingCloseBLK, message);
                writingBlock = GetWritingBlock(AveDataBlockType.CLOSE_CONNECTION_TYPE);
                writingBlock.PutString(message);
                inputQueue.PutWorkingBlock(writingBlock);
                writingBlock = null;
            }
            catch (BlockQueueSyncException e)
            {
                logger.Warn("Exception occurred while closing. {0}", e.ToString());
                //logger.Debug("FileSender skipped sending close connection data block");
                logger.Info(CommonResources.FileSenderCloseSkippedSendingCloseBLK, message);
            }

            //logger.Debug("FileSender is waiting for processor shut down.");
            logger.Info(CommonResources.FileSenderCloseWaitProcessorShutdown);
            processor.WaitForProcessCompleted(int.MaxValue);
            //logger.Debug("FileSender is waiting for block sender shut down.");
            logger.Info(CommonResources.FileSenderCloseWaitBlockSenderShutdown);
            dataBlockSender.WaitForSendCompleted(int.MaxValue);
            if (supportReader)
            {
                //logger.Debug("FileSender is waiting for response reader shut down.");
                logger.Debug(CommonResources.FileSenderCloseWaitResponseReaderShutdown);
                responseReader.WaitingForReaderCompleted(int.MaxValue);
            }
            try
            {
                dataBlockSender.Close();
            }
            catch (Exception e)
            {
                logger.Debug(e.Message);
                if (string.IsNullOrEmpty(message)) throw;
            }
            //logger.Debug("FileSender closed successfully.");
            logger.Info(CommonResources.FileSenderCloseSucceed);

        }

        /// <summary>
        /// Build a data block from the given header xml and put into send queue
        /// </summary>
        /// <param name="xml">File header description xml.</param>
        public void WriteHead(string xml)
        {
            itemBackupDataLength = 0;
            crcCaculator.Reset();
            if (isTestRun) return;

            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog);
            if (hasWriteTail.HasValue && !hasWriteTail.Value)
            {
                WriteTail(string.Empty);
            }
            writingBlock = GetWritingBlock(AveDataBlockType.HEADER_TYPE);
            writingBlock.PutString(xml);
            inputQueue.PutWorkingBlock(writingBlock);
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog, writingBlock.DataSize);
            writingBlock = null;
            hasWriteTail = false;
        }

        /// <summary>
        /// Write the content using content data type block, used for single instance mode.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteContentData(byte[] buf, int offset, int length)
        {
            RealWrite(buf, offset, length, AveDataBlockType.CONTENTDATA_TYPE);
        }

        /// <summary>
        /// Write the metadata using metadata data type block.
        /// </summary>
        /// <param name="buf">An array of byte data</param>
        /// <param name="offset">offset of the array</param>
        /// <param name="length">length need to copy</param>
        public void WriteData(byte[] buf, int offset, int length)
        {
            RealWrite(buf, offset, length, AveDataBlockType.DATA_TYPE);
        }

        /// <summary>
        /// End current file item and write the tail description.
        /// </summary>
        /// <param name="xml">Tail description xml</param>
        /// <returns>File size</returns>
        public long WriteTail(string xml)
        {
            return WriteTail(xml, true);
        }

        /// <summary>
        /// End current file item and write the tail description.
        /// </summary>
        /// <param name="xml">
        ///   <example>
        ///     <Attribute>Title:TestItem_44009_03-23-2011 03.28.49</Attribute> 
        ///     <Attribute>Priority:(2) Normal</Attribute> 
        ///     <Attribute>Status:Not Started</Attribute> 
        ///     <Attribute>PercentComplete:0.35</Attribute> 
        ///     <Attribute>DueDate:5/15/2011 7:00:00 AM</Attribute> 
        ///     <Attribute>Modified:3/23/2011 10:28:49 AM</Attribute> 
        ///     <Attribute>Created:3/23/2011 10:28:49 AM</Attribute> 
        ///     <Attribute>Author:QAROOTDC\lotsuser_test_66</Attribute>
        ///     <BackupDataExtraInfo version="5.2"><KeyAndValue key="ID" value="144209" /><KeyAndValue key="Title" value="Get Started with Windows SharePoint Services!" /></BackupDataExtraInfo>
        ///   </example>
        /// </param>
        /// <param name="isOK">Success flag</param>
        /// <returns>File size</returns>
        public long WriteTail(string xml, bool isOK)
        {
            if (isTestRun) return itemBackupDataLength;

            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog);
            if (writingBlock != null)
            {
                //put cached data into queue
                inputQueue.PutWorkingBlock(writingBlock);
                writingBlock = null;
            }
            writingBlock = GetWritingBlock(AveDataBlockType.TAIL_TYPE);

            int index = xml.IndexOf("<BackupDataExtraInfo", StringComparison.OrdinalIgnoreCase);
            string attributes = string.Empty;
            string extraInfo = string.Empty;
            if (index > 0)
            {
                attributes = xml.Substring(0, index);
                extraInfo = xml.Substring(index);
            }
            else
            {
                attributes = xml;
            }
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("FileTail");
            e.SetAttribute("length", itemBackupDataLength.ToString());
            if (isCalculateCRC)
            {
                e.SetAttribute("CRC32", crcCaculator.Value.ToString());
            }
            e.SetAttribute("extraInfo", extraInfo);
            e.InnerXml = attributes;
            if (!isOK)
            {
                e.SetAttribute("failed", "true");
            }
            writingBlock.PutString(e.OuterXml);
            inputQueue.PutWorkingBlock(writingBlock);
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog, writingBlock.DataSize);
            writingBlock = null;
            hasWriteTail = true;

            return itemBackupDataLength;
        }

        private AveDataBlock GetWritingBlock(AveDataBlockType blockType)
        {
            writingBlock = inputQueue.TakeFreeBlock();
            writingBlock.SerialNumber = 0;
            writingBlock.DataSize = 0;
            writingBlock.Type = blockType;
            //writingBlock.EncryptMethod = (byte)encryptionMethod;
            writingBlock.Flag = compressionEncryptionFlag;
            return writingBlock;
        }

        protected void RealWrite(byte[] buf, int offset, int length, AveDataBlockType currentBlockType)
        {
            itemBackupDataLength += length;
            if (isTestRun) return;

            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog);
            if (writingBlock != null && writingBlock.Type != currentBlockType)
            {
                //different type data should put into different block
                inputQueue.PutWorkingBlock(writingBlock);
                writingBlock = null;
            }
            if (writingBlock == null)
            {
                AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.PRSendQueueWait);
                writingBlock = GetWritingBlock(currentBlockType);
                AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.PRSendQueueWait, 1);
            }
            if (isCalculateCRC)
            {
                crcCaculator.Update(buf, offset, length);
            }

            int availableSpace = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN - writingBlock.DataSize;
            while (length > availableSpace)
            {
                writingBlock.AppendBuffer(buf, offset, availableSpace);
                inputQueue.PutWorkingBlock(writingBlock);
                writingBlock = null;

                offset += availableSpace;
                length -= availableSpace;
                if (length == 0) break;
                AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.PRSendQueueWait);
                writingBlock = GetWritingBlock(currentBlockType);
                AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.PRSendQueueWait, 1);
                availableSpace = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN;
            }
            if (length > 0)
            {
                ArgumentCheck.NotNull(writingBlock, nameof(writingBlock));
                writingBlock.AppendBuffer(buf, offset, length);
            }
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.FileSenderWriteCatalog, length);
        }
    }
}