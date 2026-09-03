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
using System.IO;
using System.Xml;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.FilteringBox;

namespace AvePoint.GCommon.Transfer.Data
{
    /// <summary>
    /// 不管上层是什么，提供写的方法和读的方法，内部有一个Thread来Process
    /// </summary>
    public class DataBlockProcessorV2 : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(DataBlockProcessorV2), false);

        #region #private fields#
        private string identify = string.Empty;
        private string sessionId = string.Empty;
        private AveThreadWrapper processBufferThread = null;
        private CycleStream inputCycleStream = null;
        private CycleStream outputCycleStream = null;
        private bool compression = false;
        private bool encryption = false;
        private int compressionLevel = 0;
        private bool filterByServerFlag = false;
        private DataEncryptionInfo encryptionInfo;
        private DataTransferCommonParameterizedDelegate dataProcessorExceptionCallback;
        private CommonPerformanceTimerPool performanceTimerPool;
        #endregion

        #region #Public Properties#
        public long OutputWriteCount
        {
            get 
            {
                if (this.outputCycleStream != null)
                {
                    return this.outputCycleStream.WriteLength;
                }
                return 0;
            }
        }
        public long OutputReadCount
        {
            get
            {
                if (this.outputCycleStream != null)
                {
                    return this.outputCycleStream.ReadLength;
                }
                return 0;
            }
        }
        public long InputReadCount
        {
            get
            {
                if (this.outputCycleStream != null)
                {
                    return this.inputCycleStream.ReadLength;
                }
                return 0;
            }
        }
        public long InputWriteCount
        {
            get
            {
                if (this.outputCycleStream != null)
                {
                    return this.inputCycleStream.WriteLength;
                }
                return 0;
            }
        }
        internal AveThreadWrapper ProcessBufferThread
        {
            get { return processBufferThread; }
        }
        public DataTransferCommonParameterizedDelegate DataProcessorExceptionCallback
        {
            get { return dataProcessorExceptionCallback; }
            set { dataProcessorExceptionCallback = value; }
        }
        public CommonPerformanceTimerPool PerformanceTimerPool
        {
            get { return performanceTimerPool; }
            set 
            { 
                performanceTimerPool = value;
                this.inputCycleStream.PerformanceTimerPool = value;
                this.outputCycleStream.PerformanceTimerPool = value;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compression">是否加密</param>
        /// <param name="encryption">是否压缩</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <param name="filterByServerFlag">加密受buffer的header控制还是程序控制，false一般用于receiver，true一般用于sender</param>
        public DataBlockProcessorV2(bool encryption, DataEncryptionInfo info, bool compression, int compressionLevel, bool filterByServerFlag, string identify, string sessionId)
        {
            this.compression = compression;
            this.encryption = encryption;
            this.compressionLevel = compressionLevel;
            this.filterByServerFlag = filterByServerFlag;
            this.inputCycleStream = new CycleStream(DataTransferConfiguration.DataBlockProcessorCycleStreamSize);
            this.outputCycleStream = new CycleStream(DataTransferConfiguration.DataBlockProcessorCycleStreamSize);
            this.identify = identify;
            this.sessionId = sessionId;
            this.encryptionInfo = info;
        }

        public void Run()
        {
            processBufferThread = AveThreadUtility.StartThread(ProcessDataBlockThread, "ProcessDataBlockThread" + this.sessionId + this.identify, identify);
        }

        public void Close(bool force)
        {
            DataTransferLogger.Logger("Session:{0}, identify:{1}, input write:{2}, input read:{3}, output write:{4}, output read:{5}", this.sessionId, this.identify, this.InputWriteCount, this.InputReadCount, this.OutputWriteCount, this.OutputReadCount);

            if (this.processBufferThread != null)
            {
                this.inputCycleStream.FinishWrite();
                this.outputCycleStream.FinishWrite();
                this.processBufferThread.Stop(10000, string.Empty, force);
                this.inputCycleStream.Dispose();
                this.outputCycleStream.Dispose();
                this.inputCycleStream = null;
                this.outputCycleStream = null;
                this.processBufferThread = null;
            }
        }

        public int Write(AveDataBlock dataBlock)
        {
            return Write(dataBlock.Buffer, 0, dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
        }

        public int Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        public int Write(byte[] buffer, int index, int length)
        {
            this.inputCycleStream.SafeWrite(buffer, index, length);
            return length;
        }

        public void FinishWrite()
        {
            this.inputCycleStream.FinishWrite();
        }

        public AveDataBlock Read(AveDataBlock dataBlock)
        {
            dataBlock.ClearDataBuffer();

            var readLen = Read(dataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN, false);
            if (readLen == AveDataBlock.DATA_BLOCK_HEADER_LEN)
            {
                this.AdjustDataBlockSize(dataBlock, dataBlock.DataSize);
                Read(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
            }
            else if (readLen == 0)
            {
                dataBlock = null;
            }
            else
            {
                throw new Exception(string.Format("Read data header has exception because the read length is:{0}, session:{1}, identify:{2}.", readLen, this.sessionId, this.identify));
            }

            return dataBlock;
        }

        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        public int Read(byte[] buffer, int index, int length, bool throwExceptionIfNoData=true)
        {
            var readLen = this.outputCycleStream.SafeRead(buffer, index, length, throwExceptionIfNoData);

            return readLen;
        }

        public void SetReadTimeoutDelegate(DataTransferCommonDelegate commonDelegate)
        {
            if (this.outputCycleStream != null)
            {
                this.outputCycleStream.ReadTimeoutDelegate = commonDelegate;
            }
        }

        public void SetWriteTimeoutDelegate(DataTransferCommonDelegate commonDelegate)
        {
            if (this.inputCycleStream != null)
            {
                this.inputCycleStream.WriteTimeoutDelegate = commonDelegate;
            }
        }

        private void ProcessDataBlockThread()
        {
            try
            {
                if (performanceTimerPool != null)
                {
                    performanceTimerPool.Action("Data block processor thread", true);
                }

                if (filterByServerFlag)
                {
                    EncryptAndCompress();
                }
                else
                {
                    DecryptAndDeCompress();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process dataBlock failed:{0} when the session is {1} and identify is {2}.", ex.ToString(), this.sessionId, this.identify);


                if (DataProcessorExceptionCallback != null)
                {
                    DataProcessorExceptionCallback(new Tuple<DataTransferWorkStatus, string>(DataTransferWorkStatus.DataProcessError, ex.ToString()));
                }
            }
            finally
            {
                if (performanceTimerPool != null)
                {
                    performanceTimerPool.Action("Data block processor thread", false);
                }
            }
        }

        private void EncryptAndCompress()
        {
            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Initiate before encrypt and compress", true);
            }
            var currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            AveDataBlock tempDataBlock = new AveDataBlock(DataTransferConfiguration.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            IDataFilteringBox encryptionBox = null;
            IDataFilteringBox compressionBox = null;
            byte[] buffer = new byte[DataTransferConfiguration.DataBlockProcessorBufferSize];
            MemoryStream memoryStream = new MemoryStream();

            #region DataTransfer Header Block
            AveDataBlock encryptionInfoDataBlock = new AveDataBlock(DataTransferConfiguration.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            encryptionInfoDataBlock.ClearDataBuffer();
            encryptionInfoDataBlock.Type = AveDataBlockType.ENCRYPTION_INFO_EXCHANGE_TYPE;
            XmlDocument document = new XmlDocument();
            XmlElement rootElement = document.CreateElement("DataTransfer");
            #endregion

            if (encryption)
            {
                DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                encryptionBox = DataFilteringBoxFactory.GetEncryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                
                rootElement.SetAttribute("EncryptionInfo", SerializerHelper.SerializeToBase64StringByDataContractSerializer(wrapper.EncryptionInfo));
            }
            if (compression)
            {
                compressionBox = DataFilteringBoxFactory.GetCompressionFilteringBox(DataTransferConfiguration.DataBlockCompressionMethod, compressionLevel);
            }

            #region Send the DataTransfer Header Block
            encryptionInfoDataBlock.PutString(rootElement.OuterXml);
            this.outputCycleStream.SafeWrite(encryptionInfoDataBlock.Buffer, 0, encryptionInfoDataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            #endregion

            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Initiate before encrypt and compress", false);
            }

            while (currentThreadWrapper.KeepRunning)
            {
                tempDataBlock.ClearDataBuffer();
                memoryStream.Position = 0;
                memoryStream.SetLength(0);

                buffer = new byte[DataTransferConfiguration.DataBlockProcessorBufferSize];

                var readLen = this.inputCycleStream.SafeRead(buffer, 0, buffer.Length, false);
                //DataTransferLogger.Logger("EncryptAndCompress: Read buffer:{0}", readLen);
                if (readLen > 0)
                {
                    if (compression)
                    {
                        try
                        {
                            buffer = FilterData(compressionBox, buffer, 0, readLen, memoryStream);
                        }
                        catch (Exception e)
                        {
                            DataTransferLogger.Logger(e.Message);
                            throw;
                        }
                        readLen = buffer.Length;
                        tempDataBlock.Flag |= GConstants.TransferFlag.AGENT_COMPRESSED;
                    }

                    if (encryption)
                    {
                        try
                        {
                            buffer = FilterData(encryptionBox, buffer, 0, readLen, memoryStream);
                        }
                        catch (Exception e) 
                        {
                            DataTransferLogger.Logger(e.Message);
                            throw;
                        }
                        readLen = buffer.Length;
                        tempDataBlock.Flag |= GConstants.TransferFlag.AGENT_ENCRYPTED;
                    }
                    
                    tempDataBlock.PutBinary(buffer, 0, readLen);
                    //DataTransferLogger.Logger("EncryptAndCompress: Write buffer:{0}", tempDataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    this.outputCycleStream.SafeWrite(tempDataBlock.Buffer, 0, tempDataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                }

                if (readLen < buffer.Length)
                {
                    this.outputCycleStream.FinishWrite();
                    //logger.Debug("Encrypt and compress data block end.");
                    DataTransferLogger.Logger("Encrypt and compress finish when the session is {0} and identify is {1}.", this.sessionId, this.identify);
                    break;// there is no enough buffer here now.
                }
            }

            memoryStream.Close();
        }

        private void DecryptAndDeCompress()
        {
            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Initiate before decrypt and decompress", true);
            }

            var currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            AveDataBlock tempDataBlock = new AveDataBlock(DataTransferConfiguration.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            IDataFilteringBox decryptionBox = null;
            IDataFilteringBox deCompressionBox = null;
            byte[] buffer = null;// new byte[DataTransferConfiguration.DataBlockProcessorBufferSize];
            MemoryStream memoryStream = new MemoryStream();
            //use static encryption for the decryption
            DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(DataEncryptionInfoManager.StaticEncryptionInfo);
            decryptionBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
            deCompressionBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(DataTransferConfiguration.DataBlockCompressionMethod);

            bool isFirstBlock = true;

            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Initiate before decrypt and decompress", false);
            }

            while (true)
            {
                if (currentThreadWrapper != null && (!currentThreadWrapper.KeepRunning))
                {
                    break;
                }

                tempDataBlock.ClearDataBuffer();
                memoryStream.Position = 0;

                var readLen = this.inputCycleStream.SafeRead(tempDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN, false);
                //DataTransferLogger.Logger("DecryptAndDeCompress: Read buffer:{0}", readLen);
                if (readLen == 0 || tempDataBlock.DataSize == 0)
                {
                    DataTransferLogger.Logger("Decrypt and decompress end when the session is {0} and identify is {1}.", this.sessionId, this.identify);
                    this.outputCycleStream.FinishWrite();
                    break;
                }
                else if (readLen < AveDataBlock.DATA_BLOCK_HEADER_LEN)
                {
                    throw new Exception(string.Format("Read data header has exception because the read length is:{0}, session:{1}, identify:{2}.", readLen, this.sessionId, this.identify));
                }


                this.AdjustDataBlockSize(tempDataBlock, tempDataBlock.DataSize);
                readLen = this.inputCycleStream.SafeRead(tempDataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, tempDataBlock.DataSize);

                #region get the first datablock and then get the encryption info
                if (isFirstBlock)
                {
                    isFirstBlock = false;
                    if (tempDataBlock.Type == AveDataBlockType.ENCRYPTION_INFO_EXCHANGE_TYPE)
                    {
                        if (performanceTimerPool != null)
                        {
                            performanceTimerPool.Action("Get the exchange key", true);
                        }

                        XmlDocument document = new XmlDocument();
                        document.LoadXml(tempDataBlock.RetrieveString());
                        if (document.DocumentElement.HasAttribute("EncryptionInfo"))
                        {
                            string encryptionInfoBase64 = document.DocumentElement.GetAttribute("EncryptionInfo");
                            if (!string.IsNullOrEmpty(encryptionInfoBase64))
                            {
                                DataEncryptionInfo encryptionInfo = (DataEncryptionInfo)SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(encryptionInfoBase64, typeof(DataEncryptionInfo));
                                DataEncryptionInfoWrapper infoWrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                                decryptionBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)infoWrapper.EncryptionInfo.EncryptionType, infoWrapper.DynamicKey);
                            }
                        }

                        if (performanceTimerPool != null)
                        {
                            performanceTimerPool.Action("Get the exchange key", false);
                        }

                        continue;
                    }
                }
                #endregion

                bool isCompressionInDatablock = GConstants.TransferFlag.IsModeSet(tempDataBlock.Flag, GConstants.TransferFlag.AGENT_COMPRESSED);
                bool isEncryptionInDatablock = GConstants.TransferFlag.IsModeSet(tempDataBlock.Flag, GConstants.TransferFlag.AGENT_ENCRYPTED);

                buffer = tempDataBlock.Buffer;
                var index = AveDataBlock.DATA_BLOCK_HEADER_LEN;

                if (isEncryptionInDatablock)
                {
                    try
                    {
                        buffer = FilterData(decryptionBox, buffer, index, readLen, memoryStream);
                    }
                    catch (Exception e) 
                    {
                        DataTransferLogger.Logger(e.Message);
                        throw;
                    }
                    index = 0;
                    readLen = buffer.Length;
                }

                if (isCompressionInDatablock)
                {
                    try
                    {
                        FilterData(deCompressionBox, buffer, index, readLen, outputCycleStream);
                    }
                    catch (Exception e)
                    {
                        DataTransferLogger.Logger(e.Message);
                        throw;
                    }
                }
                else
                {
                    //DataTransferLogger.Logger("DecryptAndDeCompress: Write buffer:{0}", readLen);
                    this.outputCycleStream.SafeWrite(buffer, index, readLen);
                }
            }

            memoryStream.Close();
        }

        private byte[] FilterData(IDataFilteringBox filteringBox, byte[] buffer, int index, int length, MemoryStream memoryStream)
        {
            //IDataFilteringBox filteringBox = filteringBox1;
            //if (isEncryption)
            //{
            //    EncryptionFilteringBox filteringBox = filteringBox1 as EncryptionFilteringBox;
            //}
            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("FilterData", true);
            }

            using (var stream = new MemoryStream())
            {
                //if (isEncryption)
                //{
                    filteringBox.InputBegin();
               // }
                const int inputUnit = 64 * 1024;
                var tempBuffer = new byte[inputUnit];
                int inputLen = 0;
                memoryStream.Position = 0;
                memoryStream.SetLength(0);

                while (inputLen < length)
                {
                    var shouldInputLen = inputUnit;
                    if (length - inputLen <= inputUnit)
                    {
                        shouldInputLen = length - inputLen;
                    }

                    filteringBox.Input(buffer, index, shouldInputLen);
                    index += shouldInputLen;
                    inputLen += shouldInputLen;

                    while (true)
                    {
                        var readComLen = filteringBox.ReceiveOutput(tempBuffer, 0, tempBuffer.Length);
                        if (readComLen == 0)
                        {
                            break;
                        }
                        memoryStream.Write(tempBuffer, 0, readComLen);
                    }
                }
                filteringBox.InputEnd();

                while (true)
                {
                    var readComLen = filteringBox.ReceiveOutput(tempBuffer, 0, tempBuffer.Length);
                    if (readComLen == -1)
                    {
                        break;
                    }
                    memoryStream.Write(tempBuffer, 0, readComLen);
                }

                if (performanceTimerPool != null)
                {
                    performanceTimerPool.Action("FilterData", false);
                }

                return memoryStream.ToArray();
            }
        }

        private void FilterData(IDataFilteringBox filteringBox, byte[] buffer, int index, int length, CycleStream memoryStream)
        {
            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Filter Data", true);
            }

            filteringBox.InputBegin();
            const int inputUnit = 64 * 1024;
            var tempBuffer = new byte[inputUnit];
            int inputLen = 0;

            while (inputLen < length)
            {
                var shouldInputLen = inputUnit;
                if (length - inputLen <= inputUnit)
                {
                    shouldInputLen = length - inputLen;
                }

                filteringBox.Input(buffer, index, shouldInputLen);
                index += shouldInputLen;
                inputLen += shouldInputLen;

                while (true)
                {
                    var readComLen = filteringBox.ReceiveOutput(tempBuffer, 0, tempBuffer.Length);
                    if (readComLen == 0)
                    {
                        break;
                    }
                    //DataTransferLogger.Logger("DecryptAndDeCompress: Write buffer:{0}", readComLen);
                    memoryStream.SafeWrite(tempBuffer, 0, readComLen);
                }
            }
            filteringBox.InputEnd();

            while (true)
            {
                var readComLen = filteringBox.ReceiveOutput(tempBuffer, 0, tempBuffer.Length);
                if (readComLen == -1)
                {
                    break;
                }
                //DataTransferLogger.Logger("DecryptAndDeCompress: Write buffer:{0}", readComLen);
                memoryStream.SafeWrite(tempBuffer, 0, readComLen);
            }

            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Filter Data", false);
            }
        }

        private void AdjustDataBlockSize(AveDataBlock dataBlock, int size)
        {
            if (size > dataBlock.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN)
            {
                var tempBuffer = dataBlock.Buffer;
                dataBlock.Buffer = new byte[size + AveDataBlock.DATA_BLOCK_HEADER_LEN * 5];
                Array.Copy(tempBuffer, 0, dataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //nothing
        }

        #endregion
    }
}
