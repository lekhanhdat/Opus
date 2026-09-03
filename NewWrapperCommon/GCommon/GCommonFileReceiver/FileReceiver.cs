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
    using System.Reflection;
    using System.Xml;
    using GCommon;
    using Network;
    using Utility;
    using I18N;

    #endregion
    public class FileReceiver : IFileReceiver, IDisposable
    {
        private readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AveCRC32 crcCaculator = new AveCRC32();
        private string expectCRCValue;
        private bool isCalculateCRC;
        private int crcMatchResult = -1;//1:match;   0:not match;   -1:not need CRC;

        private IAveNetwork network;
        private AveDataBlockQueue blockProcessorOutputQueue;
        private BlockProcessor processor;
        private AveDataBlock readingBlock;
        private bool enableSSL;
        private string sslThumbprint = string.Empty;

        public IBlockReader GetRawRead()
        {
            return this.processor.BlockReader;
        }

        // ReSharper disable ParameterHidesMember
        public void EnableSSL(bool enabled, string sslThumbprint = null)
        // ReSharper restore ParameterHidesMember
        {
            this.enableSSL = enabled;
            this.sslThumbprint = sslThumbprint;
        }

        [Obsolete]
        public string Open(string host, int port, string info)
        {
            var connOptions = new AveConnectionOptions { Host = host, Port = port, EnableSSL = this.enableSSL, SSLThumbprint = this.sslThumbprint };
            return Open(connOptions, info);
        }

        public string Open(AveConnectionOptions connOptions, string info)
        {
            //logger.Debug("FileReceiver is trying to open target. host:{0} port:{1}", host, port);
            logger.Info(CommonResources.FileReceiverOpenStarting, connOptions.Host, connOptions.Port);
            network = AveNetwork.Connect(connOptions);
            network.SendMessage(info);
            //logger.Debug("FileReceiver is trying to receive response. host:{0} port:{1}", host, port);
            logger.Info(CommonResources.FileReceiverOpenReceiveingResponse, connOptions.Host, connOptions.Port);
            string openResult = network.ReceiveMessage();
            //logger.Debug("FileReceiver opened successfully.");
            logger.Info(CommonResources.FileReceiverOpenSucceed);

            Wrap(network, connOptions);
            return openResult;
        }
        /// <summary>
        /// Only For the other side file sender supports responser
        /// </summary>
        /// <param name="info"></param>
        public void SendMessage(string info)
        {
            network.SendMessage(info);
        }

        [Obsolete]
        // ReSharper disable ParameterHidesMember
        public void Wrap(IAveNetwork network)
        // ReSharper restore ParameterHidesMember
        {
            Wrap(network, new AveConnectionOptions()); //use default option
        }

        // ReSharper disable ParameterHidesMember
        public void Wrap(IAveNetwork network, AveConnectionOptions connOptions)
        // ReSharper restore ParameterHidesMember
        {
            readingBlock = new AveDataBlock();
            blockProcessorOutputQueue = new AveDataBlockQueue(connOptions.DataBlockQueueSize);

            processor = new BlockProcessor(network, blockProcessorOutputQueue);
            processor.Start(connOptions.DataBlockQueueSize);

        }

        /// <summary>
        /// Read next data block
        /// </summary>
        private void ReadNextBlock(ref AveDataBlock dataBlock)
        {
            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.FileReceiverReadCatalog);
            AveDataBlock tempBlock = dataBlock;
            dataBlock = blockProcessorOutputQueue.TakeWorkingBlock();
            switch (dataBlock.Type)
            {
                case AveDataBlockType.HEADER_TYPE:
                    string headerXml = dataBlock.RetrieveString();
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(headerXml);
                    // ReSharper disable PossibleNullReferenceException
                    isCalculateCRC = xmlDocument.DocumentElement.HasAttribute("CRC32");
                    // ReSharper restore PossibleNullReferenceException
                    expectCRCValue = xmlDocument.DocumentElement.GetAttribute("CRC32");
                    crcCaculator.Reset();
                    break;
                case AveDataBlockType.DATA_TYPE:
                case AveDataBlockType.CONTENTDATA_TYPE:
                    if (isCalculateCRC)
                    {
                        crcCaculator.Update(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    }
                    break;
                case AveDataBlockType.TAIL_TYPE:
                    if (isCalculateCRC)
                    {
                        crcMatchResult = expectCRCValue.Equals(crcCaculator.Value.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                    }
                    break;
            }
            blockProcessorOutputQueue.PutFreeBlock(tempBlock);
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.FileReceiverReadCatalog, dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
        }

        /// <summary>
        /// read next FileHeader
        /// </summary>
        /// <returns>null means got CLOSE_CONNECTION_TYPE</returns>      
        public string GetNextFileHead()
        {
            do
            {
                if (readingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {
                    var errorMessage = readingBlock.RetrieveString();
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        return null;
                    }
                    throw new ClosedWithErrorException(errorMessage);
                }
                ReadNextBlock(ref readingBlock);
            }
            while (readingBlock.Type != AveDataBlockType.HEADER_TYPE);

            var headerStr = readingBlock.RetrieveString();
            readingBlock.ClearDataBuffer();
            return headerStr;
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        /// <summary>
        /// read metadata and content bytes
        /// </summary>
        /// <param name="buffer">buffer to fill</param>
        /// <param name="offset">offset to fill</param>
        /// <param name="len">expect length to read</param>
        /// <returns>0 means reach the end of current file</returns>
        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            if (readingBlock.Type == AveDataBlockType.TAIL_TYPE)
            {
                return 0;
            }
            if (readingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                var errorMessage = readingBlock.RetrieveString();
                throw new ClosedWithErrorException(errorMessage);
            }
            var returnLen = 0;
            var dataSize = readingBlock.DataSize;
            if (dataSize > 0)
            {
                if (len <= dataSize)
                {
                    readingBlock.CopyTo(buffer, offset, len);
                    readingBlock.AdjustDataBlock(len);
                    return len;
                }
                readingBlock.CopyTo(buffer, offset, dataSize);
                len -= dataSize;
                offset += dataSize;
                returnLen = dataSize;
                // ReSharper disable RedundantAssignment
                dataSize = 0;
                // ReSharper restore RedundantAssignment
                readingBlock.ClearDataBuffer();
            }
            while (true)
            {
                ReadNextBlock(ref readingBlock);
                if (readingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE || readingBlock.Type == AveDataBlockType.TAIL_TYPE)
                {
                    break;
                }

                if (readingBlock.Type == AveDataBlockType.DATA_TYPE || readingBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                {
                    dataSize = readingBlock.DataSize;
                    if (dataSize <= len)
                    {
                        readingBlock.CopyTo(buffer, offset, dataSize);
                        offset += dataSize;
                        len -= dataSize;
                        returnLen += dataSize;
                        readingBlock.ClearDataBuffer();

                        if (len == 0) break;
                    }
                    else
                    {
                        readingBlock.CopyTo(buffer, offset, len);
                        readingBlock.AdjustDataBlock(len);
                        returnLen += len;
                        break;
                    }
                }
            }
            return returnLen;
        }

        public string GetFileTail()
        {
            if (readingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                var errorMessage = readingBlock.RetrieveString();
                throw new ClosedWithErrorException(errorMessage);
            }
            return readingBlock.RetrieveString();
        }

        public string Close(string errorMsg)
        {
            //logger.Debug("FileReceiver is waiting for processor shut down.");
            logger.Info(CommonResources.FileReceiverCloseWaitProcessorShutDown);
            processor.Close(errorMsg);
            //logger.Debug("FileReceiver closed successfully.");
            logger.Info(CommonResources.FileReceiverCloseSucceed);

            return string.Empty;
        }

        /// <summary>
        /// Get the value that if the CRC is matched.
        /// </summary>
        /// <returns>If the calculate value is right, return 1; else return 0;if the CRC is disabled, return -1</returns>
        public int CRC32Match()
        {
            return crcMatchResult;
        }

        public void Dispose()
        {
            if (blockProcessorOutputQueue != null)
            {
                blockProcessorOutputQueue.Dispose();
            }
        }
    }
}
