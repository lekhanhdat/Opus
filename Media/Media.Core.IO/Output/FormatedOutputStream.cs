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




namespace AvePoint.Media.Core.IO.Output
{
    #region using directives

    using System;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Network;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.IO;
    using Merged18NResources.MediaCoreIO;
    using AvePoint.Media.Service.DomainModel;
    using Storage;

    #endregion using directives

    public class FormatedOutputStream : IGeneralOutputStream
    {
        #region -- Private Fields --
        private static AveLogger Logger = new AveLogger(typeof(FormatedOutputStream));

        DataEncryptionInfo encryptionInfo;
        Byte defaultDataMode;
        Int32 contentDataPageSize;
        Int32 maxBlockSize;
        Int32 sharePointVersion;
        Int64 productVersion;
        IOutputDataListener dataListener;
        StreamOpenType openType;

        String metaDataFileName;
        Stream metaDataStream;
        String contentDataFileName;
        Stream contentDataStream;

        Int32 currentMetaDataFileNumber;//当前正在操作的meta data数据块号
        Int32 currentContentDataFileNumber;//当前正在操作的content数据块号

        String currentItemMetaDataHeaderXml;//当前item的header xml
        Int32 currentItemMetaDataHeaderXmlLength;//当前item的header xml字节长度
        String currentItemMetaDataTailXml;//当前item的tail xml
        Int32 currentItemMetaDataTailXmlLength;//当前item的tail xml字节长度
        Byte currentItemDataMode;

        String currentItemMetaDataFilePrefixNumber;//当前item的meta data所在的meta data数据块的prefix number，会记录到index db中
        Int32 currentItemMetaDataStartFileNumber;//当前item的meta data开始数据块号，会记录到index db中
        Int64 currentItemMetaDataDataHeaderStartOffset;//这个值代表当前item的meta data在开始数据块中Data header的偏移，会记录到index db中
        Int64 currentItemMetaDataStartOffset;//当前item的meta data在开始数据块中的偏移，会记录到index db中
        Int32 currentItemMetaDataInnerOffset;//这个值代表在写content前所写的meta data的长度，会记录到data header和index db中
        Int64 currentItemMetaDataAndContentDataTotalLength;//这个值代表当前item的meta data加上content data总长度，会记录到index db中

        Int64 currentItemMetaDataDataHeaderOffset;//这个值代表Data header在当前数据块内的偏移，回写meta data的data header时从这个偏移开写
        Int32 currentItemMetaDataLength; //这个值代表在当前数据块内meta data的长度，会记录到data header中
        Int32 currentItemMetaDataBlockSequenceNO;//这个值代表如果当前item的meta data跨多个数据块，那么当前这个数据块的顺序号，会记录到data header中

        String currentItemContentDataFilePrefixNumber;//当前item的content所在的content数据块的prefix number，会记录到index db中
        Int32 currentItemContentDataStartFileNumber;//当前item的content开始数据块号，会记录到index db中
        Int64 currentItemContentDataDataHeaderStartOffset;//这个值代表当前item的content在开始数据块中Data header的偏移，会记录到index db中
        Int64 currentItemContentDataStartOffset;//当前item的content在开始数据块中的偏移，会记录到index db中
        Int64 currentItemContentDataTotalLength;//当前item的content的总长度，会记录到index db中

        Int64 currentItemContentDataDataHeaderOffset;//这个值代表Data header在当前数据块内的偏移，回写content的data header时从这个偏移开写
        Int32 currentItemContentDataLength;//这个值代表在当前数据块内content的长度，会记录到data header中
        Int32 currentItemContentDataTailAlignPageAppendLength;//如果当前的content结束没有按照页对齐，会补0对齐，这个值代表补了多少个0，由于content中不写tail xml,这只被存放到content的data header的tail length里面
        Int32 currentItemContentDataBlockSequenceNO;//这个值代表如果当前item的content跨多个数据块，那么当前这个数据块的顺序号，会记录到data header中

        AveCRC32 currentItemCrc = new AveCRC32();

        Boolean isWriteMetaDataOnce;//当前item是否已经写过一次meta data
        Boolean isWriteContentDataOnce;//当前item是否已经写过一次content

        IndexBase currentIndex;
        OutputStreamLevel outputLevel;
        Boolean isContainContent;

        #endregion -- Private Fields --

        public FormatedOutputStream(OpenOutputStreamParameter openParam)
        {
            this.currentItemMetaDataFilePrefixNumber = openParam.PrefixNumber;
            this.currentMetaDataFileNumber = openParam.InitMetaDataFileNumber - 1;
            this.currentItemContentDataFilePrefixNumber = openParam.PrefixNumber;
            this.currentContentDataFileNumber = openParam.InitContentDataFileNumber - 1;
            this.defaultDataMode = openParam.DataMode;
            this.maxBlockSize = openParam.MaxBlockSize;
            this.dataListener = openParam.DataListener;
            this.outputLevel = openParam.OutputLevel;
            string forceFileLevel = ConfigurationManager.AppSettings["forceFileLevel"];
            if (!string.IsNullOrEmpty(forceFileLevel) && bool.Parse(forceFileLevel))
            {
                this.outputLevel = OutputStreamLevel.FileLevel;
            }
            this.contentDataPageSize = (int)this.outputLevel;
            this.sharePointVersion = openParam.SPVersion;
            this.openType = openParam.OpenType;
            this.encryptionInfo = openParam.EncryptionInfo;
            this.productVersion = Convert.ToInt64(MediaEnvironment.MediaServer.MediaServerVersion.Replace(".", string.Empty));
        }

        #region -- GeneralOutputStream Methods--

        public void Open()
        {
            ChangeMetaDataFile(false, false, false);
            bool reserveBlockHeader = outputLevel == OutputStreamLevel.DataBlockLevel;
            ChangeContentDataFile(false, false, false, reserveBlockHeader);
        }

        public void BeforeItem(IndexBase basicIndex)
        {
            isContainContent = false;
            currentIndex = basicIndex;
            this.defaultDataMode = (byte)currentIndex.CurrentItemDataMode;
            currentIndex.CurrentItemPageSize = (this.outputLevel == OutputStreamLevel.FileLevel ? 1 : this.contentDataPageSize);
            currentIndex.CurrentItemVersion = this.productVersion;
        }

        public void Write(AveDataBlock dataBlock)
        {
            switch (dataBlock.Type)
            {
                case AveDataBlockType.HEADER_TYPE:
                    string headXml = Encoding.UTF8.GetString(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    WriteHeaderXml(headXml);
                    break;
                case AveDataBlockType.DATA_TYPE:
                    WriteMetaData(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    break;
                case AveDataBlockType.CONTENTDATA_TYPE:
                    WriteContentData(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    break;
                case AveDataBlockType.TAIL_TYPE:
                    string tailXml = Encoding.UTF8.GetString(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    WriteTailXml(tailXml);
                    break;
                default:
                    throw new System.NotSupportedException(string.Format(MediaCoreIOResource.FormatedOutputStreamWriteException, dataBlock.Type));
            }
        }

        public void EndItem(IndexBase basicIndex)
        {
            long dataFilePrefixNumber = 0;
            if (long.TryParse(this.currentItemMetaDataFilePrefixNumber, out dataFilePrefixNumber))
            {
                basicIndex.CurrentItemMetaDataFilePrefixNumber = dataFilePrefixNumber;
            }
            basicIndex.CurrentItemMetaDataStartFileNumber = this.currentItemMetaDataStartFileNumber;
            basicIndex.CurrentItemMetaDataDataHeaderStartOffset = this.currentItemMetaDataDataHeaderStartOffset;
            basicIndex.CurrentItemMetaDataStartOffset = this.currentItemMetaDataStartOffset;
            basicIndex.CurrentItemMetaDataInnerOffset = this.currentItemMetaDataInnerOffset;
            basicIndex.CurrentItemMetaDataAndContentDataTotalLength = this.currentItemMetaDataAndContentDataTotalLength;
            if (long.TryParse(this.currentItemContentDataFilePrefixNumber, out dataFilePrefixNumber))
            {
                basicIndex.CurrentItemContentDataFilePrefixNumber = dataFilePrefixNumber;
            }
            basicIndex.CurrentItemContentDataStartFileNumber = this.currentItemContentDataStartFileNumber;
            basicIndex.CurrentItemContentDataDataHeaderStartOffset = this.currentItemContentDataDataHeaderStartOffset;
            basicIndex.CurrentItemContentDataStartOffset = this.currentItemContentDataStartOffset;
            basicIndex.CurrentItemContentDataTotalLength = this.currentItemContentDataTotalLength;

            basicIndex.CurrentItemStorageCrc = this.currentItemCrc.Value.ToString();

            basicIndex.CurrentMetaDataFileNumber = this.currentMetaDataFileNumber;
            basicIndex.CurrentContentDataFileNumber = this.currentContentDataFileNumber;
            basicIndex.HasWrittenMetaData = true;
            basicIndex.HasWrittenContentData = true;
            if (basicIndex.CurrentItemContentDataTotalLength == 0)
                basicIndex.HasContentIdMerged = true;
        }

        public StorageResult Close()
        {
            //close顺序也需要注意, 必须先close content 后cloes meta data.
            try
            {
                if (contentDataStream != null)
                {
                    try
                    {
                        if(outputLevel != OutputStreamLevel.FileLevel)
                        {
                            CommitContentDataBlockHeader();
                        }
                    }
                    finally
                    {
                        if (!(MediaConfigInfo.CommonConfigInfo.UseMemoryStream
                            && this.maxBlockSize < ServiceConstants.MemoryStreamLimit * IOConstants.MB))
                        {
                            contentDataStream.Close();
                            contentDataStream = null;
                        }
                        this.dataListener.CommitDataBlock(FileType.Content, this.contentDataFileName, false, outputLevel);
                    }
                }
            }
            finally
            {
                if (metaDataStream != null)
                {
                    try
                    {
                        CommitMetaDataBlockHeader();
                    }
                    finally
                    {
                        if (!(MediaConfigInfo.CommonConfigInfo.UseMemoryStream
                            && this.maxBlockSize < ServiceConstants.MemoryStreamLimit * IOConstants.MB))
                        {
                            metaDataStream.Close();
                            metaDataStream = null;
                        }
                        this.dataListener.CommitDataBlock(FileType.MetaData, this.metaDataFileName, true, outputLevel);
                    }
                }
            }
            return null;
        }

        #endregion -- GeneralOutputStream Methods--

        #region -- GeneralOutputStreamEx Methods--

        public void WriteHeaderXml(string headerXml)
        {
            WriteHeaderXml(headerXml, this.defaultDataMode);
        }

        private void WriteHeaderXml(string headerXml, byte dataMode)
        {
            ResetParameters(true, false, false);
            byte[] headerXmlBuffer = Encoding.UTF8.GetBytes(headerXml);
            if (metaDataStream.Position + (IOConstants.DataHeaderSize + headerXmlBuffer.Length) > maxBlockSize)
            {
                //DataHeader 和 HeaderXml不能跨多个文件
                ChangeMetaDataFile(false, false, true);
                ResetParameters(true, true, false);
            }
            this.currentItemDataMode = dataMode;

            this.currentItemMetaDataDataHeaderOffset = (int)metaDataStream.Position;
            this.currentItemMetaDataDataHeaderStartOffset = this.currentItemMetaDataDataHeaderOffset;
            metaDataStream.Seek(IOConstants.DataHeaderSize, SeekOrigin.Current); //reserve Data Header space
            metaDataStream.Write(headerXmlBuffer, 0, headerXmlBuffer.Length);

            this.currentItemMetaDataHeaderXml = headerXml;
            this.currentItemMetaDataHeaderXmlLength = headerXmlBuffer.Length;
        }

        public void WriteMetaData(byte[] data, int offset, int count)
        {
            while (count > IOConstants.WriteBufferMaxSize)
            {
                WriteMetaDataInternal(data, offset, IOConstants.WriteBufferMaxSize);
                offset += IOConstants.WriteBufferMaxSize;
                count -= IOConstants.WriteBufferMaxSize;
            }
            if (count > 0)
            {
                WriteMetaDataInternal(data, offset, count);
            }
        }

        private void WriteMetaDataInternal(byte[] data, int offset, int count)
        {
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                this.currentItemCrc.Update(data, offset, count);
            }
            if (!isWriteMetaDataOnce)
            {
                //第一次写当前item的meta data,记录下这个item的meta data所在的开始数据块和offset
                this.currentItemMetaDataStartFileNumber = this.currentMetaDataFileNumber;
                this.currentItemMetaDataStartOffset = (int)metaDataStream.Position;
                isWriteMetaDataOnce = true;
                this.currentIndex.HasWrittenMetaData = true;
            }
            if (metaDataStream.Position + count > maxBlockSize)
            {
                //当前数据块装不下所有meta data, 要首先把当前数据库写满
                int tmpLen = (int)(maxBlockSize - metaDataStream.Position);
                if (tmpLen > 0)
                {
                    metaDataStream.Write(data, offset, tmpLen);
                    this.currentItemMetaDataLength += tmpLen;
                    this.currentItemMetaDataAndContentDataTotalLength += tmpLen;
                }
                offset = offset + tmpLen;
                count = count - tmpLen;
                //切换到下一个数据块
                ChangeMetaDataFile(true, true, true);
                this.currentItemMetaDataBlockSequenceNO++;
                ResetParameters(false, true, false);
                this.currentItemMetaDataDataHeaderOffset = (int)metaDataStream.Position;
                metaDataStream.Seek(IOConstants.DataHeaderSize, SeekOrigin.Current); //reserve Data Header space
                //DataReaderUtility.AllignPageSize(this.mMetaDataStream, 0);
            }
            metaDataStream.Write(data, offset, count);
            this.currentItemMetaDataLength += count;
            this.currentItemMetaDataAndContentDataTotalLength += count;
        }

        public void WriteContentData(byte[] data, int offset, int count)
        {
            int lengh = count;
            if (outputLevel == OutputStreamLevel.FileLevel)
            {
                isContainContent = true;
                WriteFileLevelContentDataInternal(data, offset, count);
            }
            else if (outputLevel == OutputStreamLevel.DataBlockLevel)
            {
                while (count > IOConstants.WriteBufferMaxSize)
                {
                    WriteContentDataInternal(data, offset, IOConstants.WriteBufferMaxSize);
                    offset += IOConstants.WriteBufferMaxSize;
                    count -= IOConstants.WriteBufferMaxSize;
                }
                if (count > 0)
                {
                    WriteContentDataInternal(data, offset, count);
                }
            }
        }

        private void WriteFileLevelContentDataInternal(byte[] data, int offset, int count)
        {
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                this.currentItemCrc.Update(data, offset, count);
            }
            if (!isWriteContentDataOnce)
            {
                int position = 0;
                try
                {
                    position = (int)contentDataStream.Position;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Get content data stream position fails. {ex}");
                }
                this.isWriteContentDataOnce = true;
                //第一次写content的时候记录下，Content开始的数据块，以及在Content前的meta data长度即InnerOffset
                this.currentItemContentDataStartFileNumber = this.currentContentDataFileNumber;
                this.currentItemMetaDataInnerOffset = Convert.ToInt32(this.currentItemMetaDataAndContentDataTotalLength);
                this.currentItemContentDataDataHeaderOffset = position;
                this.currentItemContentDataDataHeaderStartOffset = currentItemContentDataDataHeaderOffset;
                this.currentItemContentDataStartOffset = position;
                this.currentIndex.HasWrittenContentData = true;
            }
            contentDataStream.Write(data, offset, count);
            this.currentItemContentDataLength += count;
            this.currentItemContentDataTotalLength += count;
            this.currentItemMetaDataAndContentDataTotalLength += count;
        }

        private void WriteContentDataInternal(byte[] data, int offset, int count)
        {
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                this.currentItemCrc.Update(data, offset, count);
            }
            if (!isWriteContentDataOnce)
            {
                if (contentDataStream.Position + this.contentDataPageSize >= maxBlockSize)
                {
                    //当前数据块块无法在写Content的Header Xml了, 切换到下一个数据块
                    ChangeContentDataFile(true, true, true, true);
                    this.currentItemContentDataBlockSequenceNO++;
                    ResetParameters(false, false, true);
                }

                //第一次写content的时候记录下，Content开始的数据块，以及在Content前的meta data长度即InnerOffset
                this.currentItemContentDataStartFileNumber = this.currentContentDataFileNumber;
                this.currentItemMetaDataInnerOffset = Convert.ToInt32(this.currentItemMetaDataAndContentDataTotalLength);
                this.isWriteContentDataOnce = true;

                this.currentItemContentDataDataHeaderOffset = (int)contentDataStream.Position;
                this.currentItemContentDataDataHeaderStartOffset = currentItemContentDataDataHeaderOffset;
                contentDataStream.Seek(IOConstants.DataHeaderSize, SeekOrigin.Current); //reserve Data Header space

                byte[] headerXmlBuffer = Encoding.UTF8.GetBytes(this.currentItemMetaDataHeaderXml);
                contentDataStream.Write(headerXmlBuffer, 0, headerXmlBuffer.Length);

                DataReaderUtility.AllignPageSize(this.contentDataStream, this.contentDataPageSize);
                this.currentItemContentDataStartOffset = (int)this.contentDataStream.Position;
                this.currentIndex.HasWrittenContentData = true;
            }

            if (contentDataStream.Position + count > maxBlockSize)
            {
                //当前数据块装不下所有content, 要首先把当前数据块写满
                int tmpLen = (int)(maxBlockSize - contentDataStream.Position);
                if (tmpLen > 0)
                {
                    contentDataStream.Write(data, offset, tmpLen);
                    this.currentItemContentDataLength += tmpLen;
                    this.currentItemContentDataTotalLength += tmpLen;
                    this.currentItemMetaDataAndContentDataTotalLength += tmpLen;
                }
                offset = offset + tmpLen;
                count = count - tmpLen;

                //切换到下一个数据块
                ChangeContentDataFile(true, true, true, true);
                this.currentItemContentDataBlockSequenceNO++;
                ResetParameters(false, false, true);

                this.currentItemContentDataDataHeaderOffset = (int)contentDataStream.Position;
                contentDataStream.Seek(IOConstants.DataHeaderSize, SeekOrigin.Current); //reserve Data Header space

                DataReaderUtility.AllignPageSize(this.contentDataStream, this.contentDataPageSize);
            }
            contentDataStream.Write(data, offset, count);
            this.currentItemContentDataLength += count;
            this.currentItemContentDataTotalLength += count;
            this.currentItemMetaDataAndContentDataTotalLength += count;
        }

        public void WriteTailXml(string tailXml)
        {
            byte[] tailXmlBuffer = Encoding.UTF8.GetBytes(tailXml);
            this.currentItemMetaDataTailXml = tailXml;
            this.currentItemMetaDataTailXmlLength = tailXmlBuffer.Length;

            metaDataStream.Write(tailXmlBuffer, 0, tailXmlBuffer.Length);
            long currentMetaDataOffset = metaDataStream.Position;
            CommitMetaDataHeader(false);
            metaDataStream.Position = currentMetaDataOffset;

            if (outputLevel == OutputStreamLevel.FileLevel)
            {
                if (isContainContent)
                {
                    ChangeContentDataFile(false, false, false, false);
                }
            }
            else if (outputLevel == OutputStreamLevel.DataBlockLevel)
            {
                if (this.isWriteContentDataOnce)
                {
                    this.currentItemContentDataTailAlignPageAppendLength = DataReaderUtility.AllignPageSize(contentDataStream, this.contentDataPageSize);
                    long currentContentDataOffset = contentDataStream.Position;
                    CommitContentDataHeader(false);
                    contentDataStream.Position = currentContentDataOffset;
                }
            }
        }

        #endregion -- GeneralOutputStreamEx Methods--

        #region --Private Methods--

        private void ChangeMetaDataFile(bool commitDataHeader, bool hasMoreData, bool commitBlockHeader)
        {
            if (commitDataHeader)
            {
                CommitMetaDataHeader(hasMoreData);
            }
            if (commitBlockHeader)
            {
                CommitMetaDataBlockHeader();
            }
            if (metaDataStream != null)
            {
                if (!(MediaConfigInfo.CommonConfigInfo.UseMemoryStream
                    && this.maxBlockSize < ServiceConstants.MemoryStreamLimit * IOConstants.MB))
                {
                    metaDataStream.Close();
                    metaDataStream = null;
                }
                this.dataListener.CommitDataBlock(FileType.MetaData, this.metaDataFileName, false, outputLevel);
            }

            this.currentMetaDataFileNumber++;
            int prefixNumber;
            if (!int.TryParse(currentItemMetaDataFilePrefixNumber, out prefixNumber))
            {
                prefixNumber = -1;
            }
            metaDataStream = this.dataListener.ChangeDataBlock(FileType.MetaData, prefixNumber, currentMetaDataFileNumber, out metaDataFileName);
            metaDataStream.Seek(IOConstants.BlockHeaderSize, SeekOrigin.Begin);//reserve Block Header space
        }

        private void ChangeContentDataFile(bool commitDataHeader, bool hasMoreData, bool commitBlockHeader, bool needReserveBlockHeaderSpace)
        {
            if ((openType & StreamOpenType.NoContent) == 0)
            {
                if (commitDataHeader)
                {
                    CommitContentDataHeader(hasMoreData);
                }
                if (commitBlockHeader)
                {
                    CommitContentDataBlockHeader();
                }
                if (contentDataStream != null)
                {
                    if (!(MediaConfigInfo.CommonConfigInfo.UseMemoryStream
                        && this.maxBlockSize < ServiceConstants.MemoryStreamLimit * IOConstants.MB))
                    {
                        contentDataStream.Close();
                        contentDataStream = null;
                    }
                    if (outputLevel == OutputStreamLevel.FileLevel)
                    {
                        this.dataListener.CommitDataBlock(FileType.Content, this.contentDataFileName, false, outputLevel, this.currentIndex.CurrentItemName);
                    }
                    else
                    {
                        this.dataListener.CommitDataBlock(FileType.Content, this.contentDataFileName, false, outputLevel);
                    }
                }

                this.currentContentDataFileNumber++;
                int prefixNumber;
                if (!int.TryParse(currentItemMetaDataFilePrefixNumber, out prefixNumber))
                {
                    prefixNumber = -1;
                }
                contentDataStream = this.dataListener.ChangeDataBlock(FileType.Content, prefixNumber, currentContentDataFileNumber, out contentDataFileName);
                if (needReserveBlockHeaderSpace)
                {
                    contentDataStream.Seek(IOConstants.BlockHeaderSize, SeekOrigin.Begin);//reserve BlockHeader space
                }
            }
        }

        private void CommitMetaDataBlockHeader()
        {
            BlockHeader blockHeader = new BlockHeader();
            blockHeader.Version = IOConstants.DataBlockVersion;
            blockHeader.Type = GetBlockType(BlockType.MetaData);//constant, meta data type
            blockHeader.BlockNum = currentMetaDataFileNumber;
            blockHeader.NextBlockNum = currentMetaDataFileNumber + 1;
            blockHeader.BlockSize = maxBlockSize;
            blockHeader.PageSize = 1;
            blockHeader.NextHeaderOffset = 0;//reserved
            blockHeader.SPVersion = this.sharePointVersion;

            metaDataStream.Position = 0L;
            WriteBlockHeader(metaDataStream, blockHeader);
        }

        private void CommitContentDataBlockHeader()
        {
            BlockHeader blockHeader = new BlockHeader();
            blockHeader.Version = IOConstants.DataBlockVersion;
            blockHeader.Type = GetBlockType(BlockType.ContentData); //constant, content type
            blockHeader.BlockNum = currentContentDataFileNumber;
            blockHeader.NextBlockNum = currentContentDataFileNumber + 1;
            blockHeader.BlockSize = maxBlockSize;
            blockHeader.PageSize = contentDataPageSize == (int)OutputStreamLevel.FileLevel ? 1 : contentDataPageSize;
            blockHeader.NextHeaderOffset = 0;//reserved
            blockHeader.SPVersion = this.sharePointVersion;

            contentDataStream.Position = 0L;
            WriteBlockHeader(contentDataStream, blockHeader);
        }

        private BlockType GetBlockType(BlockType defaultType)
        {
            if ((this.defaultDataMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED)
            {
                defaultType |= BlockType.Encrypted;
            }
            if ((this.defaultDataMode & GConstants.TransferFlag.MEDIA_COMPRESSED) == GConstants.TransferFlag.MEDIA_COMPRESSED)
            {
                defaultType |= BlockType.Compressed;
            }
            return defaultType;
        }

        private void CommitMetaDataHeader(bool hasMoreData)
        {
            DataHeader dataHeader = new DataHeader();
            if (this.currentItemMetaDataStartFileNumber != this.currentMetaDataFileNumber)
            {
                dataHeader.HeaderXmlLength = 0;
            }
            else
            {
                dataHeader.HeaderXmlLength = this.currentItemMetaDataHeaderXmlLength;
            }
            dataHeader.TailXmlLength = this.currentItemMetaDataTailXmlLength;
            dataHeader.DataLength = this.currentItemMetaDataLength;
            dataHeader.InnerOffset = this.currentItemMetaDataInnerOffset;
            dataHeader.Crc = this.currentItemCrc.Value;
            dataHeader.SequenceNO = this.currentItemMetaDataBlockSequenceNO;
            dataHeader.HasMoreData = hasMoreData ? (byte)1 : (byte)0;
            dataHeader.DataMode = this.currentItemDataMode;

            metaDataStream.Position = this.currentItemMetaDataDataHeaderOffset;
            WriteDataHeader(metaDataStream, dataHeader);
        }

        private void CommitContentDataHeader(bool hasMoreData)
        {
            DataHeader dataHeader = new DataHeader();
            if (this.currentItemContentDataStartFileNumber != this.currentContentDataFileNumber)
            {
                dataHeader.HeaderXmlLength = 0;
            }
            else
            {
                dataHeader.HeaderXmlLength = this.currentItemMetaDataHeaderXmlLength;
            }
            dataHeader.TailXmlLength = this.currentItemContentDataTailAlignPageAppendLength;
            dataHeader.DataLength = this.currentItemContentDataLength;
            dataHeader.InnerOffset = this.currentItemMetaDataInnerOffset;
            dataHeader.Crc = this.currentItemCrc.Value;
            dataHeader.SequenceNO = this.currentItemContentDataBlockSequenceNO;
            dataHeader.HasMoreData = hasMoreData ? (byte)1 : (byte)0;
            dataHeader.DataMode = this.currentItemDataMode;

            contentDataStream.Position = this.currentItemContentDataDataHeaderOffset;
            WriteDataHeader(contentDataStream, dataHeader);
        }

        private void ResetParameters(bool newItem, bool afterChangeMetaDataFile, bool afterChangeContentFile)
        {
            if (newItem)
            {
                this.currentItemMetaDataHeaderXml = string.Empty;
                this.currentItemMetaDataHeaderXmlLength = 0;
                this.currentItemMetaDataTailXml = string.Empty;
                this.currentItemMetaDataTailXmlLength = 0;

                this.currentItemMetaDataStartFileNumber = 0;
                this.currentItemMetaDataDataHeaderStartOffset = 0;
                this.currentItemMetaDataStartOffset = 0;
                this.currentItemMetaDataInnerOffset = 0;
                this.currentItemMetaDataAndContentDataTotalLength = 0;

                this.currentItemMetaDataDataHeaderOffset = 0;
                this.currentItemMetaDataLength = 0;
                //this.mCurrentItemMetaDataBlockSequenceNO = 0;  //正常情况下，这个值应该reset,但是5.x有bug没有reset,所以为了对比数据方便，暂时保留这个bug

                this.currentItemContentDataStartFileNumber = 0;
                this.currentItemContentDataDataHeaderStartOffset = 0;
                this.currentItemContentDataStartOffset = 0;
                this.currentItemContentDataTotalLength = 0;

                this.currentItemContentDataDataHeaderOffset = 0;
                this.currentItemContentDataLength = 0;
                this.currentItemContentDataTailAlignPageAppendLength = 0;
                //this.mCurrentItemContentDataBlockSequenceNO = 0;  //正常情况下，这个值应该reset,但是5.x有bug没有reset,所以为了对比数据方便，暂时保留这个bug

                this.currentItemCrc.Reset();

                this.isWriteMetaDataOnce = false;
                this.isWriteContentDataOnce = false;
            }
            else if (afterChangeMetaDataFile)
            {
                this.currentItemMetaDataDataHeaderOffset = 0;
                this.currentItemMetaDataLength = 0;
            }
            else if (afterChangeContentFile)
            {
                this.currentItemContentDataDataHeaderOffset = 0;
                this.currentItemContentDataLength = 0;
                this.currentItemContentDataTailAlignPageAppendLength = 0;//这个值不reset也行，因为只有content所在的最后一个数据块可能需要按页补齐，前面不会对这个变量赋值
            }
        }

        private void WriteDataHeader(Stream stream, DataHeader dataHeader)
        {
            byte[] buffer = new byte[IOConstants.DataHeaderSize];
            Encoding.UTF8.GetBytes(dataHeader.GUID, 0, dataHeader.GUID.Length, buffer, 0);
            DataReaderUtility.ToBigBytes(dataHeader.HeaderXmlLength, buffer, dataHeader.GUID.Length + 0);
            DataReaderUtility.ToBigBytes(dataHeader.TailXmlLength, buffer, dataHeader.GUID.Length + 4);
            DataReaderUtility.ToBigBytes(dataHeader.DataLength, buffer, dataHeader.GUID.Length + 8);
            DataReaderUtility.ToBigBytes(dataHeader.InnerOffset, buffer, dataHeader.GUID.Length + 12);
            DataReaderUtility.ToBigBytes(dataHeader.Crc, buffer, dataHeader.GUID.Length + 16);
            DataReaderUtility.ToBigBytes(dataHeader.SequenceNO, buffer, dataHeader.GUID.Length + 20);
            buffer[dataHeader.GUID.Length + 24] = dataHeader.HasMoreData;
            buffer[dataHeader.GUID.Length + 25] = dataHeader.DataMode;
            stream.Write(buffer, 0, buffer.Length);
        }

        private void WriteBlockHeader(Stream stream, BlockHeader blockHeader)
        {
            byte[] buffer = new byte[30];
            DataReaderUtility.ToBigBytes(blockHeader.Version, buffer, 0);
            DataReaderUtility.ToBigBytes((short)blockHeader.Type, buffer, 4);
            DataReaderUtility.ToBigBytes(blockHeader.BlockNum, buffer, 6);
            DataReaderUtility.ToBigBytes(blockHeader.NextBlockNum, buffer, 10);
            DataReaderUtility.ToBigBytes(blockHeader.BlockSize, buffer, 14);
            DataReaderUtility.ToBigBytes(blockHeader.PageSize, buffer, 18);
            DataReaderUtility.ToBigBytes(blockHeader.NextHeaderOffset, buffer, 22);
            DataReaderUtility.ToBigBytes(blockHeader.SPVersion, buffer, 26);
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(16, SeekOrigin.Current);

            //在设置SiteCollection级别的filter policy的情况下，某些SiteCollection不符合filter条件，但是仍然会起子job，此时备份时候只会走到open和close方法，current index为空
            if (this.encryptionInfo != null && this.currentIndex != null)
            {
                string serializedEncryptionInfo = SerializerHelper.SerializeToBase64StringByDataContractSerializer(this.encryptionInfo);
                string jobId = this.currentIndex.BackupJobId;
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlElement = xmlDoc.CreateElement("ExtensionXml");
                xmlElement.SetAttribute("jobID", jobId);
                xmlElement.SetAttribute("encryptionInfo", serializedEncryptionInfo);
                string xml = xmlElement.OuterXml;
                byte[] extensionBuffer = Encoding.UTF8.GetBytes(xml);
                if (stream.Position + 4 + extensionBuffer.Length < IOConstants.BlockHeaderSize - 512)
                {
                    byte[] extensionLengthBuffer = new byte[4];
                    DataReaderUtility.ToBigBytes(extensionBuffer.Length, extensionLengthBuffer, 0);
                    stream.Write(extensionLengthBuffer, 0, 4);
                    stream.Write(extensionBuffer, 0, extensionBuffer.Length);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion --Private Methods--
    }
}