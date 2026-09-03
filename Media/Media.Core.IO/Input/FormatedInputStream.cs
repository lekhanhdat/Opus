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




namespace AvePoint.Media.Core.IO.Input
{
    #region using directives

    using System;
    using System.IO;
    using System.Reflection;
    using AngleSharp.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using Merged18NResources.MediaCoreIO;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Utility;

    #endregion using directives

    public class FormatedInputStream : IMediaGeneralInputStream, IXConverter
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        IInputDataListener dataListener;
        IndexBase currentItemIndex;
        String lastBackupJobIdForMetaData;
        String lastBackupJobIdForContentData;
        String currentBackupJobIdForMetaData;
        String currentBackupJobIdForContentData;

        Int64 currentMetaDataFilePrefixNumber;
        Int64 lastMetaDataFilePrefixNumber = -1;
        Int64 lastMetaDataFileNumber = -1;
        Int64 currentMetaDataFileNumber;
        String currentMetaDataFileName;
        XStream currentMetaDataStream;

        Int64 currentContentDataFilePrefixNumber;
        Int64 lastContentDataFilePrefixNumber = -1;
        Int64 lastContentDataFileNumber = -1;
        Int64 currentContentDataFileNumber;
        String currentContentDataFileName;
        XStream currentContentDataStream;

        Int64 totalReadedMetaData;
        Int64 totalReadedContentData;

        StreamOpenType openType;

        Int64 metaDataBeginReadOffset;
        Int64 metaDataBeginReadLength;
        Int64 contentDataBeginReadOffset;
        Int64 contentDataBeginReadLength;

        AveCRC32 crc = new AveCRC32();

        public FormatedInputStream(OpenInputStreamParameter openParam)
        {
            this.dataListener = openParam.DataListener;
            if (this.dataListener == null) throw new NullReferenceException(MediaCoreIOResource.FormatedInputStreamFormatedInputStreamException);
        }

        #region GeneralInputStream Members

        public void Open()
        {
        }

        public String NextItem(IndexBase itemIndex)
        {
            ResetParameters();//必须先为变量清零，否则ChangeMetaDataFile和ChangeContentDataFile用到这两个值会出错
            this.currentItemIndex = itemIndex;
            this.openType = itemIndex.OpenType;

            var archiverIndex = itemIndex as ArchiverBasicIndex;
            this.currentBackupJobIdForMetaData = (archiverIndex != null && archiverIndex.IsDeduplicateData) ? archiverIndex.JobId : itemIndex.BackupJobId;

            if (this.HasMetaDataPart1)
            {
                this.currentMetaDataFilePrefixNumber = this.currentItemIndex.CurrentItemMetaDataFilePrefixNumber;
                this.currentMetaDataFileNumber = this.currentItemIndex.CurrentItemMetaDataStartFileNumber - 1;
                ChangeMetaDataFile();
            }

            this.currentBackupJobIdForContentData = itemIndex.BackupJobId;
            if (this.HasContent)
            {
                this.currentContentDataFilePrefixNumber = this.currentItemIndex.CurrentItemContentDataFilePrefixNumber;
                this.currentContentDataFileNumber = this.currentItemIndex.CurrentItemContentDataStartFileNumber - 1;
                ChangeContentDataFile();
            }
            return default(String);
        }

        public int ReadMetaDataPart1(byte[] data, int offset, int count)
        {
            int readLen;
            Int64 remainderData = GetAvailableMetaDataPart1Length();
            if (remainderData == 0)
            {
                return -1;
            }
            else if (remainderData <= count)
            {
                readLen = this.currentMetaDataStream.Read(data, offset, (int)remainderData);
            }
            else
            {
                readLen = this.currentMetaDataStream.Read(data, offset, count);
            }
            if (readLen > 0)
            {
                this.totalReadedMetaData += readLen;
            }
            else
            {
                if (remainderData > 0)
                {
                    EndRead(FileType.MetaData);
                    ChangeMetaDataFile();
                    BeginRead(FileType.MetaData);
                    readLen = ReadMetaDataPart1(data, offset, count);
                }
            }
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                crc.Update(data, offset, readLen);
            }
            return readLen;
        }

        public void BeginRead(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.MetaData:
                    //currentMetaDataStream.BeginRead(currentMetaDataStream.Info);
                    break;
                case FileType.Content:
                    //currentContentDataStream.BeginRead(currentContentDataStream.Info);
                    break;
                default:
                    throw new UnknownFileTypeException(string.Format(MediaCoreIOResource.FormatedInputStreamBeginReadException, fileType));
            }
        }

        public void EndRead(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.MetaData:
                    //currentMetaDataStream.EndRead();
                    break;
                case FileType.Content:
                    //currentContentDataStream.EndRead();
                    break;
                default:
                    throw new UnknownFileTypeException(string.Format(MediaCoreIOResource.FormatedInputStreamBeginReadException, fileType));
            }
        }

        public int ReadContent(byte[] data, int offset, int count)
        {
            int readLen = 0;
            long remainderData = GetAvailableContentDataLength();
            if (remainderData == 0)
            {
                return -1;
            }
            else if (remainderData <= count)
            {
                readLen = this.currentContentDataStream.Read(data, offset, (int)remainderData);
            }
            else
            {
                readLen = this.currentContentDataStream.Read(data, offset, count);
            }

            if (readLen > 0)
            {
                this.totalReadedContentData += readLen;
            }
            else
            {
                if (remainderData > 0)
                {
                    EndRead(FileType.Content);
                    ChangeContentDataFile();
                    BeginRead(FileType.Content);
                    readLen = ReadContent(data, offset, count);
                }
            }
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                crc.Update(data, offset, readLen);
            }
            return readLen;
        }

        public int ReadMetaDataPart2(byte[] data, int offset, int count)
        {
            int readLen;
            long remainderData = GetAvailableMetaDataPart2Length();
            if (remainderData == 0)
            {
                return -1;
            }
            else if (remainderData <= count)
            {
                readLen = this.currentMetaDataStream.Read(data, offset, (int)remainderData);
            }
            else
            {
                readLen = this.currentMetaDataStream.Read(data, offset, count);
            }
            if (readLen > 0)
            {
                this.totalReadedMetaData += readLen;
            }
            else
            {
                if (remainderData > 0)
                {
                    EndRead(FileType.MetaData);
                    ChangeMetaDataFile();
                    BeginRead(FileType.MetaData);
                    readLen = ReadMetaDataPart2(data, offset, count);
                }
            }
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                crc.Update(data, offset, readLen);
            }
            return readLen;
        }

        public void EndItem()
        {
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore && !crc.Value.ToString().EqualsIgnoreCase(this.currentItemIndex.CurrentItemStorageCrc))
            {
                logger.Warn(MediaCoreIOResource.FormatedInputStreamEndItemWarn, this.currentItemIndex.ToString());
            }
        }

        public void Close()
        {
            if (this.currentMetaDataStream != null)
            {
                this.dataListener.CloseDataBlock(FileType.MetaData, this.currentMetaDataFileName, this.currentMetaDataStream);
                this.currentMetaDataStream = null;
            }
            if (this.currentContentDataStream != null)
            {
                this.dataListener.CloseDataBlock(FileType.Content, this.currentContentDataFileName, this.currentContentDataStream);
                this.currentContentDataStream = null;
            }
        }

        public IndexBase CurrentItemIndex
        {
            get { return this.currentItemIndex; }
        }

        public bool HasMetaDataPart1
        {
            get
            {
                if (IsOpenModeSet(StreamOpenType.LengthInContent))
                {
                    return true;
                }
                //4.5和5.0的数据都按照MetaDataPart1读取，不认为存在Content和MetaDataPart2读取
                if (IsOpenModeSet(StreamOpenType.NoContent))
                {
                    return currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength > 0;
                }
                return currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - currentItemIndex.CurrentItemContentDataTotalLength > 0;
            }
        }

        public bool HasContent
        {
            get
            {
                if (IsOpenModeSet(StreamOpenType.NoContent))
                    return false;
                return currentItemIndex.CurrentItemContentDataTotalLength > 0;
            }
        }

        public bool HasMetaDataPart2
        {
            get
            {
                if (IsOpenModeSet(StreamOpenType.NoContent))
                    return false;
                if (currentItemIndex.CurrentItemMetaDataInnerOffset > 0)
                {
                    return (currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - currentItemIndex.CurrentItemMetaDataInnerOffset - currentItemIndex.CurrentItemContentDataTotalLength) > 0;
                }
                else { return false; }
            }
        }

        #endregion GeneralInputStream Members

        #region Private Methods

        private bool IsOpenModeSet(StreamOpenType destType)
        {
            return (openType & destType) == destType;
        }

        private void ResetParameters()
        {
            crc.Reset();
            this.totalReadedMetaData = 0;
            this.totalReadedContentData = 0;
            metaDataBeginReadOffset = 0;
            metaDataBeginReadLength = 0;
            contentDataBeginReadOffset = 0;
            contentDataBeginReadLength = 0;
        }

        /// <summary>
        /// 从文件中读取四个字节的数据总长度
        /// </summary>
        /// <returns></returns>
        private long ReadItemMetaDataAndContentDataTotalLengthInMetaDataFile(long offset)
        {
            long size = 0;

            //有的存储介质要求传入读取的offset和length，这个函数需要从offset开始读取4个字节
            metaDataBeginReadOffset = offset;
            metaDataBeginReadLength = 4;
            DataBlockOpenOutParam outParam;
            using (Stream tempStream = this.dataListener.OpenDataBlock(new DataBlockOpenParam
            {
                FileType = FileType.MetaData,
                JobId = this.currentBackupJobIdForMetaData,
                PrefixNumber = currentMetaDataFilePrefixNumber,
                FileNumber = currentMetaDataFileNumber,
                DataVersion = this.GetDataVersion(),
                IsReadLength = true,
                Index = currentItemIndex
            }, out outParam))
            {
                tempStream.Position = offset;
                size = DataReaderUtility.ReadBigInt32(tempStream);
            }
            currentMetaDataFileName = outParam.FileName;
            return size == -1 ? long.MaxValue : size;
        }

        /// <summary>
        /// 一定要在mTotalReadedMetaData变量修改后调用这个方法，这个方法内部依靠mTotalReadedMetaData变量的值
        /// </summary>
        private void ChangeMetaDataFile()
        {
            bool needResetOffset = false;
            if (this.currentMetaDataStream != null)
            {
                this.dataListener.CloseDataBlock(FileType.MetaData, this.currentMetaDataFileName, this.currentMetaDataStream);
                this.currentMetaDataStream = null;
            }
            this.currentMetaDataFileNumber++;
            bool isRealChange = this.currentMetaDataFileNumber != this.lastMetaDataFileNumber || this.currentMetaDataFilePrefixNumber != lastMetaDataFilePrefixNumber
                || !this.currentBackupJobIdForMetaData.EqualsIgnoreCase(this.lastBackupJobIdForMetaData);
            if (isRealChange)
            {
                this.lastMetaDataFilePrefixNumber = this.currentMetaDataFilePrefixNumber;
                this.lastBackupJobIdForMetaData = this.currentBackupJobIdForMetaData;
                this.lastMetaDataFileNumber = this.currentMetaDataFileNumber;
            }

            bool isFirstBlock = this.currentMetaDataFileNumber == this.currentItemIndex.CurrentItemMetaDataStartFileNumber;

            if (isFirstBlock && IsOpenModeSet(StreamOpenType.LengthInContent))//只在第一次并且length存储在文件中，才读取长度
            {
                currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength = ReadItemMetaDataAndContentDataTotalLengthInMetaDataFile(this.currentItemIndex.CurrentItemMetaDataStartOffset);
            }
            else if (IsOpenModeSet(StreamOpenType.LengthInContent))
            {
                var sizeInContent = ReadItemMetaDataAndContentDataTotalLengthInMetaDataFile(0);
                needResetOffset = true;
                if (sizeInContent == long.MaxValue)
                {
                    currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength = long.MaxValue;
                }
                else
                {
                    currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength = sizeInContent + totalReadedMetaData;
                }
            }

            DataBlockOpenOutParam outParam;
            this.currentMetaDataStream = this.dataListener.OpenDataBlock(new DataBlockOpenParam
            {
                FileType = FileType.MetaData,
                JobId = currentBackupJobIdForMetaData,
                PrefixNumber = currentMetaDataFilePrefixNumber,
                FileNumber = currentMetaDataFileNumber,
                DataVersion = this.GetDataVersion(),
                OpenFromCache = MediaConfigInfo.CommonConfigInfo.ReadMetaDataViaCache,
                ShouldDownloadData = isRealChange,
                Index = currentItemIndex
            }, out outParam);
            currentMetaDataFileName = outParam.FileName;
            if (needResetOffset)
            {
                this.currentMetaDataStream.Position = 4;
            }
            //if (isFirstBlock)
            //{
            //    if (currentMetaDataStream.CanSeek)
            //    {
            //        this.currentMetaDataStream.Position = this.currentItemIndex.CurrentItemMetaDataStartOffset;//第一个数据块，直接定位到偏移
            //    }
            //}
            //else
            //{
            //    if (currentMetaDataStream.CanSeek)
            //    {
            //        this.currentMetaDataStream.Position = metaDataBeginReadOffset;
            //    }
            //}
        }

        /// <summary>
        /// 一定要在mTotalReadedContentData变量修改后调用这个方法，这个方法内部依靠mTotalReadedContentData变量的值
        /// </summary>
        private void ChangeContentDataFile()
        {
            if (this.currentContentDataStream != null)
            {
                this.dataListener.CloseDataBlock(FileType.Content, this.currentContentDataFileName, this.currentContentDataStream);
                this.currentContentDataStream = null;
            }

            this.currentContentDataFileNumber++;
            bool isRealChange = this.currentContentDataFileNumber != this.lastContentDataFileNumber || this.currentContentDataFilePrefixNumber != lastContentDataFilePrefixNumber
                || !this.currentBackupJobIdForContentData.EqualsIgnoreCase(this.lastBackupJobIdForContentData);
            if (isRealChange)
            {
                this.lastContentDataFilePrefixNumber = this.currentContentDataFilePrefixNumber;
                this.lastBackupJobIdForContentData = this.currentBackupJobIdForContentData;
                this.lastContentDataFileNumber = this.currentContentDataFileNumber;
            }

            bool isFirstBlock = this.currentContentDataFileNumber == this.currentItemIndex.CurrentItemContentDataStartFileNumber;

            DataBlockOpenOutParam outParam;
            this.currentContentDataStream = this.dataListener.OpenDataBlock(new DataBlockOpenParam
            {
                FileType = FileType.Content,
                JobId = currentBackupJobIdForContentData,
                PrefixNumber = currentContentDataFilePrefixNumber,
                FileNumber = currentContentDataFileNumber,
                DataVersion = this.GetDataVersion(),
                OpenFromCache = MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache,
                ShouldDownloadData = isRealChange,
                Index = currentItemIndex
            }, out outParam);
            currentContentDataFileName = outParam.FileName;
            //if (isFirstBlock)
            //{
            //    if (this.currentContentDataStream.CanSeek)
            //    {
            //        this.currentContentDataStream.Position = this.currentItemIndex.CurrentItemContentDataStartOffset;//第一个数据块，直接定位到偏移
            //    }
            //}
            //else
            //{
            //    if (this.currentContentDataStream.CanSeek)
            //    {
            //        this.currentContentDataStream.Position = IOConstants.BlockHeaderSize + this.currentItemIndex.CurrentItemPageSize;//非第一个数据块，直接跳过两个header定位到Meta Data开始位置
            //    }
            //}
        }

        private Int64 GetAvailableMetaDataPart1Length()
        {
            if (IsOpenModeSet(StreamOpenType.NoContent))
            {
                return (currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - this.totalReadedMetaData);
            }
            if (this.currentItemIndex.CurrentItemMetaDataInnerOffset > 0)
            {
                return (this.currentItemIndex.CurrentItemMetaDataInnerOffset - this.totalReadedMetaData);
            }
            else
            {
                return (this.currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - this.totalReadedMetaData);
            }
        }

        private DataVersion GetDataVersion()
        {
            DataVersion result = new DataVersion();
            if (currentItemIndex.CurrentItemVersion >= 10000)
            {
                result = (DataVersion)(Convert.ToInt32(currentItemIndex.CurrentItemVersion.ToString().Substring(0, 4)));
            }
            else
            {
                result = (DataVersion)currentItemIndex.CurrentItemVersion;
            }
            return result;
        }

        private long GetAvailableMetaDataPart2Length()
        {
            return this.currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - this.totalReadedMetaData - this.currentItemIndex.CurrentItemContentDataTotalLength;
        }

        private long GetAvailableContentDataLength()
        {
            return GetCurrentItemContentDataTotalLength() - this.totalReadedContentData;
        }

        #endregion Private Methods

        #region IXConverter Members

        public StorageInfo FormNames(FileType fileType, String highName, String lowName)
        {
            var info = new StorageInfo { HighName = highName, LowName = lowName, };
            switch (fileType)
            {
                case FileType.Content:
                    info.Offset = contentDataBeginReadOffset;
                    info.Length = contentDataBeginReadLength;
                    info.DataType = DataBlockType.ContentData;
                    break;
                case FileType.MetaData:
                    info.Offset = metaDataBeginReadOffset;
                    info.Length = metaDataBeginReadLength;
                    info.DataType = DataBlockType.MetaData;
                    break;
                default:
                    throw new UnknownFileTypeException(String.Format(MediaCoreIOResource.FormatedInputStreamBeginReadException, fileType));
            }
            info.ExtraStorageInfo = CurrentItemIndex.StorageInformation;
            return info;
        }

        public void SetFileSize(FileType fileType, Int64 blockFileSize, bool isSupportAutoChangeDataBlock, Boolean isReadLength = default(Boolean))
        {
            if (fileType == FileType.Content)
            {
                bool isFirstBlock = IsFirstContentDataBlock();
                SetContentDataOffsetAndLength(isFirstBlock, blockFileSize, isSupportAutoChangeDataBlock);
            }
            else if (fileType == FileType.MetaData)
            {
                bool isFirstBlock = IsFirstMetaDataBlock();
                SetMetaDataOffsetAndLength(isFirstBlock, blockFileSize, isSupportAutoChangeDataBlock, isReadLength);
            }
        }

        public bool IsFirstMetaDataBlock()
        {
            return this.currentMetaDataFileNumber == this.currentItemIndex.CurrentItemMetaDataStartFileNumber;
        }

        public bool IsFirstContentDataBlock()
        {
            return this.currentContentDataFileNumber == this.currentItemIndex.CurrentItemContentDataStartFileNumber;
        }

        public long GetCurrentItemMetaDataStartOffsetFromItemIndex()
        {
            return this.currentItemIndex.CurrentItemMetaDataStartOffset;
        }

        public long GetCurrentItemContentDataStartOffsetFromItemIndex()
        {
            return this.currentItemIndex.CurrentItemContentDataStartOffset;
        }

        private void SetMetaDataOffsetAndLength(bool isFirstBlock, long blocksize, bool isSupportAutoChangeDataBlock, Boolean isReadLength)
        {
            int extraLength = IsOpenModeSet(StreamOpenType.Skip4Bytes) ? 4 : 0;//在老数据中，有4个字节的额外空间，因此在读取时将offset后移4(byte)
            if (isFirstBlock)
            {
                if (isReadLength)
                    metaDataBeginReadOffset = this.currentItemIndex.CurrentItemMetaDataStartOffset;
                else
                    metaDataBeginReadOffset = this.currentItemIndex.CurrentItemMetaDataStartOffset + extraLength;

                if (blocksize == this.currentItemIndex.CurrentItemMetaDataStartOffset)
                {
                    metaDataBeginReadOffset = IOConstants.BlockHeaderSize + IOConstants.DataHeaderSize;
                }
            }
            else
            {
                metaDataBeginReadOffset = IOConstants.BlockHeaderSize + IOConstants.DataHeaderSize;
            }

            if (IsOpenModeSet(StreamOpenType.NoContent))
            {
                if (isSupportAutoChangeDataBlock)
                {
                    metaDataBeginReadLength = currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - totalReadedMetaData;
                }
                else
                {
                    metaDataBeginReadLength = Math.Min(currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - totalReadedMetaData, blocksize - metaDataBeginReadOffset);
                }
            }
            else
            {
                long metaTotleLength = currentItemIndex.CurrentItemMetaDataAndContentDataTotalLength - currentItemIndex.CurrentItemContentDataTotalLength;
                if (isSupportAutoChangeDataBlock)
                {
                    metaDataBeginReadLength = metaTotleLength - totalReadedMetaData;
                }
                else
                {
                    metaDataBeginReadLength = Math.Min(metaTotleLength - totalReadedMetaData, blocksize - metaDataBeginReadOffset);
                }
            }
        }

        private void SetContentDataOffsetAndLength(bool isFirstBlock, long blocksize, bool isSupportAutoChangeDataBlock)
        {
            if (IsOpenModeSet(StreamOpenType.NoContent))
                throw new Exception(MediaCoreIOResource.FormatedInputStreamSetContentDataOffsetAndLengthException);

            var contentDataTotalLength = GetCurrentItemContentDataTotalLength();
            if (isFirstBlock)
            {
                contentDataBeginReadOffset = currentItemIndex.CurrentItemContentDataStartOffset;
                if (blocksize == this.currentItemIndex.CurrentItemContentDataStartOffset)
                {
                    contentDataBeginReadOffset = IOConstants.BlockHeaderSize + this.currentItemIndex.CurrentItemPageSize;
                }
            }
            else
            {
                contentDataBeginReadOffset = IOConstants.BlockHeaderSize + this.currentItemIndex.CurrentItemPageSize;
            }

            if (isSupportAutoChangeDataBlock)
            {
                contentDataBeginReadLength = contentDataTotalLength;
            }
            else
            {
                contentDataBeginReadLength = Math.Min(contentDataTotalLength - totalReadedContentData, blocksize - contentDataBeginReadOffset);
            }
        }

        private long GetCurrentItemContentDataTotalLength(bool forReadContent = true)
        {
            if (forReadContent)
            {
                var archiverIndex = this.CurrentItemIndex as ArchiverBasicIndex;
                if (archiverIndex != null && archiverIndex.IsDeduplicateData && !string.IsNullOrEmpty(archiverIndex.DedupExtension))
                {
                    try
                    {
                        var dedupExtInfo = SerializerHelper.DeserializeByDataContractJsonSerializer<DedupExtensionInfo>(archiverIndex.DedupExtension);
                        return dedupExtInfo.SourceFileContentLength;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Deserialize dedup extension fail. ExtStr: {archiverIndex.DedupExtension}. Error: {ex}");
                        throw;
                    }
                }
            }

            return this.CurrentItemIndex.CurrentItemContentDataTotalLength;
        }

        #endregion IXConverter Members

        public void SetEncryptionInfos(Dictionary<string, DataEncryptionInfo> encryptionInfos, Func<string, DataEncryptionInfo>? dataEncryptInfoGetter = null)
        {
            throw new NotImplementedException();
        }
    }
}