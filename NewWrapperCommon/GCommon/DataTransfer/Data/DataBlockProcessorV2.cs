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
using System.Text;
using AvePoint.GCommon.Utility.FilteringBox;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Network;
using System.IO;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using System.Xml;

namespace AvePoint.GCommon.Transfer.Data
{
    /// <summary>
    /// 不管上层是什么，提供写的方法和读的方法，内部有一个Thread来Process
    /// </summary>
    public class DataBlockProcessorV2 : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(DataBlockProcessorV2), false);

        #region #private fields#
        protected string identify = string.Empty;
        protected string sessionId = string.Empty;
        protected AveThreadWrapper processBufferThread = null;
        protected CycleStream inputCycleStream = null;
        protected CycleStream outputCycleStream = null;
        protected bool compression = false;
        protected bool encryption = false;
        protected int compressionLevel = 0;
        protected bool filterByServerFlag = false;
        protected DataEncryptionInfo encryptionInfo;
        protected DataTransferCommonParameterizedDelegate dataProcessorInnerExceptionCallback;
        protected CommonPerformanceTimerPool performanceTimerPool;
        #endregion

        #region #Public Properties#
        public virtual long OutputWriteCount
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
        public virtual long OutputReadCount
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
                if (this.inputCycleStream != null)
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
                if (this.inputCycleStream != null)
                {
                    return this.inputCycleStream.WriteLength;
                }
                return 0;
            }
        }
        internal virtual AveThreadWrapper ProcessBufferThread
        {
            get { return processBufferThread; }
        }
        public DataTransferCommonParameterizedDelegate DataProcessorExceptionCallback
        {
            get { return dataProcessorInnerExceptionCallback; }
            set { dataProcessorInnerExceptionCallback = value; }
        }
        public CommonPerformanceTimerPool PerformanceTimerPoolProperty
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
            this.inputCycleStream = new CycleStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorCycleStreamSize);
            this.outputCycleStream = new CycleStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorCycleStreamSize);
            this.identify = identify;
            this.sessionId = sessionId;
            this.encryptionInfo = info;
        }

        public virtual void Run()
        {
            processBufferThread = AveThreadUtility.StartThread(ProcessDataBlockThread, "ProcessDataBlockThread" + this.sessionId + this.identify, identify);
        }

        public virtual void Close(bool force)
        {
            DataTransferLogger.Logger(AveLogLevel.INFO, "Session:{0}, identify:{1}, input write:{2}, input read:{3}, output write:{4}, output read:{5}", this.sessionId, this.identify, this.InputWriteCount, this.InputReadCount, this.OutputWriteCount, this.OutputReadCount);

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

        public virtual int Write(byte[] buffer, int index, int length)
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

        public virtual int Read(byte[] buffer, int index, int length, bool throwExceptionIfNoData = true)
        {
            var readLen = this.outputCycleStream.SafeRead(buffer, index, length, throwExceptionIfNoData);

            return readLen;
        }

        public virtual void SetReadTimeoutDelegate(DataTransferCommonDelegate commonDelegate)
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
            AveDataBlock tempDataBlock = new AveDataBlock(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            IDataFilteringBox encryptionBox = null;
            IDataFilteringBox compressionBox = null;
            byte[] buffer = new byte[DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize];
            MemoryStream memoryStream = new MemoryStream();

            #region DataTransfer Header Block
            AveDataBlock encryptionInfoDataBlock = new AveDataBlock(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            encryptionInfoDataBlock.ClearDataBuffer();
            encryptionInfoDataBlock.Type = AveDataBlockType.ENCRYPTION_INFO_EXCHANGE_TYPE;
            XmlDocument document = new XmlDocument();
            XmlElement rootElement = document.CreateElement("DataTransfer");
            #endregion

            if (encryption)
            {
                DataEncryptionInfoWrapper wrapper = ResolveDynamicKey(encryptionInfo);
                encryptionBox = DataFilteringBoxFactory.GetEncryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                
                rootElement.SetAttribute("EncryptionInfo", SerializerHelper.SerializeToBase64StringByDataContractSerializer(wrapper.EncryptionInfo));
            }
            if (compression)
            {
                compressionBox = DataFilteringBoxFactory.GetCompressionFilteringBox(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockCompressionMethod, compressionLevel);
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

                buffer = new byte[DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize];

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
            AveDataBlock tempDataBlock = new AveDataBlock(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            IDataFilteringBox decryptionBox = null;
            IDataFilteringBox deCompressionBox = null;
            byte[] buffer = null;// new byte[DataTransferConfiguration.DataBlockProcessorBufferSize];
            MemoryStream memoryStream = new MemoryStream();
            //use static encryption for the decryption
            DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(DataEncryptionInfoManager.StaticEncryptionInfo);
            decryptionBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
            deCompressionBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockCompressionMethod);

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
                                DataEncryptionInfoWrapper infoWrapper = ResolveDynamicKey(encryptionInfo);
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

        private DataEncryptionInfoWrapper ResolveDynamicKey(DataEncryptionInfo encryptionInfo)
        {
            DataEncryptionInfoWrapper wrapper = null;
            try
            {
                wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
            }
            catch (Exception ex)
            {
                logger.Debug("Cannot resolve dynamic key. {0}", ex);
            }

            if (wrapper == null)
            {
                try
                {
                    logger.Debug("Try to resolve dynamic key in callback method.");
                    wrapper = DataTransferDynamicKeyResolver.ResolverDynamicKey(encryptionInfo);
                    if (wrapper != null)
                    {
                        DataEncryptionInfoManager.PutEncryptionInfo(wrapper);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Resolve dynamic key in callback error. {0}", ex);
                }
            }

            if (wrapper != null && wrapper.EncryptionInfo.EncryptedDynamicKey != null)
            {
                logger.Debug("Using dynamic key encryption method.");
            }
            return wrapper;
        }

        protected byte[] FilterData(IDataFilteringBox filteringBox, byte[] buffer, int index, int length, MemoryStream memoryStream)
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

            //using (var stream = new MemoryStream())
            {
                //if (isEncryption)
                //{
                    filteringBox.InputBegin();
               // }
                const int inputUnit = 64 * 1024;
                var tempBuffer = new byte[inputUnit];
                int inputLen = 0;
                memoryStream.Position = 0;
                if (memoryStream.Length != 0)
                {
                    memoryStream.SetLength(0);
                }

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

        protected byte[] FilterData(IDataFilteringBox filteringBox, byte[] buffer, int index, int length)
        {
            using(var stream = new MemoryStream())
            {
                return FilterData(filteringBox, buffer, index, length, stream);
            }
        }

        protected void FilterData(IDataFilteringBox filteringBox, byte[] buffer, int index, int length, CycleStream memoryStream)
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

        protected void AdjustDataBlockSize(AveDataBlock dataBlock, int size)
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

    /// <summary>
    /// 没有内部Thread，只有在读取数据的时候才会加密和解密，这样可能会影响效率，但是需要测试。
    /// 
    /// 比起V2多了一次线程，CPU消耗比较大
    /// </summary>
    internal class DataBlockProcessorV3 : DataBlockProcessorV2
    {
        protected IDataFilteringBox encryptionFilteringBox;
        protected IDataFilteringBox compressionFilteringBox;
        protected MemoryStream filteredStream;
        protected byte[] tempBuffer;
        protected MemoryStream tempStream;
        protected AveDataBlock writeDataBlock;
        protected int lastWriteLength;
        protected int lastFilteredStreamLength;
        protected bool firstBlock;
        protected long originalWriteLength;
        protected long originalReadLength;

        /// <summary>
        /// construct method
        /// </summary>
        /// <param name="encryption"></param>
        /// <param name="info"></param>
        /// <param name="compression"></param>
        /// <param name="compressionLevel"></param>
        /// <param name="protectData"> 是否是加密压缩，还是解密解压缩
        /// true: write 不加密，read 加密
        /// false: write 解密，read 不解密</param>
        /// <param name="identify"></param>
        /// <param name="sessionId"></param>
        public DataBlockProcessorV3(bool encryption, DataEncryptionInfo info, bool compression, int compressionLevel, bool protectData, string identify, string sessionId)
            : base(encryption, info, compression, compressionLevel, protectData, identify, sessionId)
        {
            filteredStream = new MemoryStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            tempStream = new MemoryStream();
            tempBuffer = new byte[DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockProcessorBufferSize];
            writeDataBlock = new AveDataBlock();
            firstBlock = true;
            originalWriteLength = 0;
            originalReadLength = 0;
            if(protectData)
            {
                var preWriteDataBlock = new AveDataBlock();
                preWriteDataBlock.Type = AveDataBlockType.ENCRYPTION_INFO_EXCHANGE_TYPE;
                var document = new XmlDocument();
                var rootElement = document.CreateElement("DataTransfer");
                if(encryption)
                {
                    var wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                    encryptionFilteringBox = DataFilteringBoxFactory.GetEncryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                    rootElement.SetAttribute("EncryptionInfo", SerializerHelper.SerializeToBase64StringByDataContractSerializer(wrapper.EncryptionInfo));
                }
                if(compression)
                {
                    compressionFilteringBox = DataFilteringBoxFactory.GetCompressionFilteringBox(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockCompressionMethod, compressionLevel);
                }
                preWriteDataBlock.PutString(rootElement.OuterXml);
                filteredStream.Write(preWriteDataBlock.Buffer, 0, preWriteDataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            }
            else
            {
                var wrapper = DataEncryptionInfoManager.ResolveDynamicKey(DataEncryptionInfoManager.StaticEncryptionInfo);
                encryptionFilteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);// for old data
                compressionFilteringBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DataBlockCompressionMethod);
            }
        }

        public override void Run()
        {
            //nothing
        }

        public override int Write(byte[] buffer, int index, int length)
        {
            originalWriteLength += length;
            if(!filterByServerFlag)
            {
                var totalLength = length;
                while (length > 0)
                {
                    if (lastWriteLength < AveDataBlock.DATA_BLOCK_HEADER_LEN)
                    {
                        var remainLength = AveDataBlock.DATA_BLOCK_HEADER_LEN - lastWriteLength;
                        var writeLength = length;
                        if(length > remainLength)
                        {
                            writeLength = remainLength;
                        }

                        Array.Copy(buffer, index, writeDataBlock.Buffer, lastWriteLength, writeLength);
                        length -= writeLength;
                        index += writeLength;
                        lastWriteLength += writeLength;
                    }

                    if (length > 0 && lastWriteLength >= AveDataBlock.DATA_BLOCK_HEADER_LEN)
                    {
                        var dataSize = writeDataBlock.DataSize;

                        if(dataSize > writeDataBlock.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN)
                        {
                            var newBuffer = new byte[dataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                            Array.Copy(writeDataBlock.Buffer, 0, newBuffer, 0, writeDataBlock.Buffer.Length);
                            writeDataBlock.Buffer = newBuffer;
                        }

                        var writeLength = length;

                        if(dataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN - lastWriteLength < length)
                        {
                            writeLength = dataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN - lastWriteLength;
                        }

                        Array.Copy(buffer, index, writeDataBlock.Buffer, lastWriteLength, writeLength);
                        length -= writeLength;
                        index += writeLength;
                        lastWriteLength += writeLength;

                        if(lastWriteLength == dataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN)
                        {
                            ProcessDataBlock(writeDataBlock);
                            writeDataBlock.ClearDataBuffer();
                            lastWriteLength = 0;
                        }
                        else if(lastWriteLength > dataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN)
                        {
                            throw new Exception(string.Format("Logic issue:{0} > {1} + {2}", lastWriteLength, dataSize, AveDataBlock.DATA_BLOCK_HEADER_LEN));
                        }
                    }
                }

                return totalLength;
            }

            return base.Write(buffer, index, length);
        }

        protected void ProcessDataBlock(AveDataBlock dataBlock)
        {
            if(firstBlock)
            {
                firstBlock = false;
                if(dataBlock.Type == AveDataBlockType.ENCRYPTION_INFO_EXCHANGE_TYPE)
                {
                    if (performanceTimerPool != null)
                    {
                        performanceTimerPool.Action("Get the exchange key", true);
                    }

                    var document = new XmlDocument();
                    document.LoadXml(dataBlock.RetrieveString());
                    if (document.DocumentElement.HasAttribute("EncryptionInfo"))
                    {
                        string encryptionInfoBase64 = document.DocumentElement.GetAttribute("EncryptionInfo");
                        if (!string.IsNullOrEmpty(encryptionInfoBase64))
                        {
                            var encryptionInfo = (DataEncryptionInfo)SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(encryptionInfoBase64, typeof(DataEncryptionInfo));
                            var infoWrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                            encryptionFilteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)infoWrapper.EncryptionInfo.EncryptionType, infoWrapper.DynamicKey);
                        }
                    }

                    if (performanceTimerPool != null)
                    {
                        performanceTimerPool.Action("Get the exchange key", false);
                    }

                    return;
                }
            }

            bool isCompressionInDatablock = GConstants.TransferFlag.IsModeSet(dataBlock.Flag, GConstants.TransferFlag.AGENT_COMPRESSED);
            bool isEncryptionInDatablock = GConstants.TransferFlag.IsModeSet(dataBlock.Flag, GConstants.TransferFlag.AGENT_ENCRYPTED);

            var buffer = dataBlock.Buffer;
            var index = AveDataBlock.DATA_BLOCK_HEADER_LEN;
            var readLen = dataBlock.DataSize;

            if (isEncryptionInDatablock)
            {
                try
                {
                    buffer = FilterData(encryptionFilteringBox, buffer, index, readLen, tempStream);
                }
                catch (Exception e)
                {
                    DataTransferLogger.Logger("decrypt data failed:{0}" + e);
                    throw;
                }
                index = 0;
                readLen = buffer.Length;
            }

            if (isCompressionInDatablock)
            {
                try
                {
                    buffer = FilterData(compressionFilteringBox, buffer, index, readLen, tempStream);
                }
                catch (Exception e)
                {
                    DataTransferLogger.Logger("decompress data failed:{0}", e);
                    throw;
                }
                index = 0;
                readLen = buffer.Length;
            }

            base.Write(buffer, index, readLen);
        }

        public override int Read(byte[] buffer, int index, int length, bool throwExceptionIfNoData = true)
        {
            if (filterByServerFlag)
            {
                var totalReadLen = 0;

                while (length > 0)
                {
                    var readLen = Read(buffer, index, length);

                    if (readLen == 0)
                    {
                        break;
                    }

                    totalReadLen += readLen;
                    index += readLen;
                    length -= readLen;
                }

                if (throwExceptionIfNoData && length > 0)
                {
                    throw new Exception("There is not enough data.");
                }

                return totalReadLen;
            }

            return this.inputCycleStream.SafeRead(buffer, index, length, throwExceptionIfNoData);
        }

        private int Read(byte[] buffer, int index, int length)
        {
            var currentLength = filteredStream.Length - lastFilteredStreamLength;
            if (currentLength == 0)
            {
                if (filteredStream.Length > 0)
                {
                    filteredStream.Position = 0;
                    filteredStream.SetLength(0);
                }
                lastFilteredStreamLength = 0;

                var readLen = this.inputCycleStream.SafeRead(tempBuffer, 0, tempBuffer.Length, false);

                if (readLen > 0)
                {
                    var dataBlock = ProcessDataBlock(tempBuffer, 0, readLen, filterByServerFlag);
                    filteredStream.Write(dataBlock.Buffer, 0, dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);

                    if (readLen < tempBuffer.Length)
                    {
                        DataTransferLogger.Logger("Encrypt and compress finish when the session is {0} and identify is {1}.", this.sessionId, this.identify);
                    }
                }

                currentLength = filteredStream.Length - lastFilteredStreamLength;
            }

            if (currentLength > 0)
            {
                var readLen = 0;
                if (currentLength > length)
                {
                    filteredStream.Position = lastFilteredStreamLength;
                    readLen = filteredStream.Read(buffer, index, length);
                    lastFilteredStreamLength += readLen;
                    originalReadLength += readLen;
                    return readLen;
                }

                filteredStream.Position = lastFilteredStreamLength;
                readLen = filteredStream.Read(buffer, index, (int)currentLength);
                lastFilteredStreamLength += readLen;
                originalReadLength += readLen;
                return readLen;
            }

            return 0;
        }

        protected AveDataBlock ProcessDataBlock(byte[] buffer, int index, int length, bool usingFilter)
        {
            var dataBlock = new AveDataBlock();

            if (usingFilter)
            {
                if (compression)
                {
                    try
                    {
                        buffer = FilterData(compressionFilteringBox, buffer, index, length, tempStream);
                    }
                    catch (Exception e)
                    {
                        DataTransferLogger.Logger("compress data failed:{0}", e);
                        throw;
                    }
                    index = 0;
                    length = buffer.Length;
                    dataBlock.Flag |= GConstants.TransferFlag.AGENT_COMPRESSED;
                }

                if (encryption)
                {
                    try
                    {
                        buffer = FilterData(encryptionFilteringBox, buffer, index, length, tempStream);
                    }
                    catch (Exception e)
                    {
                        DataTransferLogger.Logger("encrypt data failed:{0}", e);
                        throw;
                    }
                    index = 0;
                    length = buffer.Length;
                    dataBlock.Flag |= GConstants.TransferFlag.AGENT_ENCRYPTED;
                }
            }

            dataBlock.PutBinary(buffer, index, length);

            return dataBlock;
        }

        public override void SetReadTimeoutDelegate(DataTransferCommonDelegate commonDelegate)
        {
            //base.SetReadTimeoutDelegate(commonDelegate);
        }

        public override void Close(bool force)
        {
            DataTransferLogger.Logger(AveLogLevel.INFO, "Session:{0}, identify:{1}, input write:{2}, input read:{3}, output write:{4}, output read:{5}", this.sessionId, this.identify, this.InputWriteCount, this.InputReadCount, this.OutputWriteCount, this.OutputReadCount);

            if(outputCycleStream != null)
            {
                outputCycleStream.FinishWrite();
            }
            if(inputCycleStream != null)
            {
                inputCycleStream.FinishWrite();
            }


            if (this.processBufferThread != null)
            {
                this.processBufferThread.Stop(10000, string.Empty, force);
                this.processBufferThread = null;
            }

            if(outputCycleStream != null)
            {
                outputCycleStream.Dispose();
                outputCycleStream = null;
            }

            if(inputCycleStream != null)
            {
                this.inputCycleStream.Dispose();
                this.inputCycleStream = null;
            }

            if(filteredStream != null)
            {
                filteredStream.Close();
                filteredStream.Dispose();
                filteredStream = null;
            }

            if(tempStream != null)
            {
                tempStream.Close();
                tempStream.Dispose();
                tempStream = null;
            }
        }

        public override long OutputReadCount { get { return originalReadLength; } }

        public override long OutputWriteCount{ get { return originalWriteLength; } }
    }
}
