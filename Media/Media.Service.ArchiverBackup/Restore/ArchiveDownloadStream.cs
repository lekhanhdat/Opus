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

using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.FilteringBox;
using AvePoint.Media.Core.IO;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve60x;
using AvePoint.RA.CommonUtil;
using Media.Common.ClassicStorageApi;
using Storage;


namespace Media.Service.ArchiverBackup
{

    public class ArchiverDataFilterStream : Stream
    {
        Stream innerStream;
        IDataFilteringBox filteringBox;
        ReadingState readingState;

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set { }
        }
        public override void Flush()
        {
            innerStream.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }
        public override int Read(byte[] data, int offset, int count)
        {
            int outputLen = filteringBox.ReceiveOutput(data, offset, count);
            if (outputLen != 0) return outputLen;
            if (this.readingState == ReadingState.Open)
            {
                filteringBox.InputBegin();
                this.readingState = ReadingState.ContentData;
            }
            while (true)
            {
                byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                int readLen = 0;

                readLen = innerStream.Read(buffer, 0, buffer.Length);

                if (readLen == -1)
                {
                    filteringBox.InputEnd();
                }
                else
                {
                    filteringBox.Input(buffer, 0, readLen);
                }
                outputLen = filteringBox.ReceiveOutput(data, offset, count);
                if (outputLen == 0) continue;
                return outputLen;
            }
        }

        public ArchiverDataFilterStream(Stream stream, IDataFilteringBox dataFilteringBox)
        {
            innerStream = stream;
            this.filteringBox = dataFilteringBox;
            this.readingState = ReadingState.Open;
        }
    }

    public class ArchiveDownloadStream : Stream
    {
        Stream innerStream;

        public ArchiveDownloadStream(ArchiverBasicIndex fileIndex, ArchiverRestoreJob restoreJob, Dictionary<string, DataEncryptionInfo> encryptionInfos)
        {
            innerStream = new ArchiverContentStream(fileIndex, restoreJob);

            var isMediaEncryptedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED;
            var isAgentEncryptedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.AGENT_ENCRYPTED) == GConstants.TransferFlag.AGENT_ENCRYPTED;

            if (isMediaEncryptedData || isAgentEncryptedData)
            {
                var encryptionInfo = DataEncryptionInfoManager.StaticBlowfishEncryptionInfo;
                if (encryptionInfos.ContainsKey(fileIndex.BackupJobId))
                {
                    encryptionInfo = encryptionInfos[fileIndex.BackupJobId];
                }
                DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                var decryptionFilteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);

                innerStream = new ArchiverDataFilterStream(innerStream, decryptionFilteringBox);
            }

            var isMediaCompressedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.MEDIA_COMPRESSED) == GConstants.TransferFlag.MEDIA_COMPRESSED;
            var isAgentCompressedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.AGENT_COMPRESSED) == GConstants.TransferFlag.AGENT_COMPRESSED;

            if (isMediaCompressedData || isAgentCompressedData)
            {
                var compressFilteringBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(fileIndex.CurrentItemCompressionMethod);
                innerStream = new ArchiverDataFilterStream(innerStream, compressFilteringBox);
            }
        }
            
        public ArchiveDownloadStream(ArchiverBasicIndex fileIndex, string dataVolume, DataEncryptionInfo? dataEncryptionInfo, IXSystem logicalDevice)
        {
            innerStream = new ArchiverContentStream(fileIndex, dataVolume, logicalDevice);

            var isMediaEncryptedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED;
            var isAgentEncryptedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.AGENT_ENCRYPTED) == GConstants.TransferFlag.AGENT_ENCRYPTED;

            if (isMediaEncryptedData || isAgentEncryptedData)
            {
                var encryptionInfo = DataEncryptionInfoManager.StaticBlowfishEncryptionInfo;
                if (dataEncryptionInfo != null)
                {
                    encryptionInfo = dataEncryptionInfo;
                }
                DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                var decryptionFilteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                innerStream = new ArchiverDataFilterStream(innerStream, decryptionFilteringBox);
            }

            var isMediaCompressedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.MEDIA_COMPRESSED) == GConstants.TransferFlag.MEDIA_COMPRESSED;
            var isAgentCompressedData = (fileIndex.CurrentItemDataMode & GConstants.TransferFlag.AGENT_COMPRESSED) == GConstants.TransferFlag.AGENT_COMPRESSED;

            if (isMediaCompressedData || isAgentCompressedData)
            {
                var compressFilteringBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(fileIndex.CurrentItemCompressionMethod);
                innerStream = new ArchiverDataFilterStream(innerStream, compressFilteringBox);
            }
        }

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set { }
        }
        public override void Flush()
        {
            innerStream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }
    }

    public class ArchiverContentStream : Stream
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ArchiverContentStream));
        string _dataVolume;
        IndexBase _indexer;

        private IXSystem logicalDevice;
        private ArchiverFileNameGenerator _fileNameGenerator = new ArchiverFileNameGenerator();
        private Dictionary<string, long> blockSizeMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        Int64 dataBlockFileNumber;
        String dataBlockFileName;
        XStream innerContentStream;
        AveCRC32 crc = new AveCRC32();

        Int64 totalReadedContentData;
        Int64 contentDataBeginReadOffset;
        Int64 contentDataBeginReadLength;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length
        {
            get
            {
                return _indexer.CurrentItemContentDataTotalLength;
            }
        }
        public override long Position
        {
            get
            {
                return totalReadedContentData;
            }
            set { }
        }

        public ArchiverContentStream(ArchiverBasicIndex fileIndex, ArchiverRestoreJob restoreJob)
        {
            _indexer = fileIndex;
            _indexer.IsRestoreToFS = true;

            //Open device
            this._dataVolume = restoreJob.DataVolume;
            this.logicalDevice = XFactoryCommon.InstanceLibrary(restoreJob.LogicalDevice.ToXRIS());
            this.logicalDevice.Open();

            this.totalReadedContentData = 0;
            this.contentDataBeginReadOffset = 0;
            this.contentDataBeginReadLength = 0;

            this.dataBlockFileNumber = this._indexer.CurrentItemContentDataStartFileNumber - 1;
            OpenNextStream();
        }

        public ArchiverContentStream(ArchiverBasicIndex fileIndex, string dataVolume, IXSystem logicalDevice)
        {
            _indexer = fileIndex;
            _indexer.IsRestoreToFS = true;

            //Open device
            logger.Info($"Data volume info: {dataVolume}");
            this._dataVolume = dataVolume;
            this.logicalDevice = logicalDevice;

            this.totalReadedContentData = 0;
            this.contentDataBeginReadOffset = 0;
            this.contentDataBeginReadLength = 0;

            this.dataBlockFileNumber = this._indexer.CurrentItemContentDataStartFileNumber - 1;
            OpenNextStream();
        }

        private void OpenNextStream()
        {
            if (this.innerContentStream != null)
            {
                this.innerContentStream.Close();
                this.innerContentStream = null;
            }

            this.dataBlockFileNumber++;

            this.dataBlockFileName = _fileNameGenerator.GenerateContentFileName(new FileNameParameter() { FileNumber = dataBlockFileNumber, JobID = _indexer.BackupJobId });
            //prepare for next stream
            var currentContentDataInfo = new StorageInfo { HighName = _dataVolume, LowName = dataBlockFileName, Offset = contentDataBeginReadOffset, Length = contentDataBeginReadLength, DataType = DataBlockType.ContentData };
            var blockFileSize = this.DataBlockGetSize(currentContentDataInfo);
            var isFirstBlock = this.dataBlockFileNumber == this._indexer.CurrentItemContentDataStartFileNumber;
            if (isFirstBlock)
            {
                contentDataBeginReadOffset = _indexer.CurrentItemContentDataStartOffset;
            }
            else
            {
                contentDataBeginReadOffset = IOConstants.BlockHeaderSize + this._indexer.CurrentItemPageSize;
            }

            if ((this.logicalDevice as AvePoint.Media.ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true)
            {
                contentDataBeginReadLength = _indexer.CurrentItemContentDataTotalLength;
            }
            else
            {
                contentDataBeginReadLength = Math.Min(_indexer.CurrentItemContentDataTotalLength - totalReadedContentData, blockFileSize - contentDataBeginReadOffset);
            }


            logger.Info($"Open stream {currentContentDataInfo.HighPlusLowName}");
            currentContentDataInfo.Length = contentDataBeginReadLength;
            currentContentDataInfo.Offset = contentDataBeginReadOffset;
            this.innerContentStream = this.logicalDevice.OpenStream(currentContentDataInfo, FileMode.Open);
        }

        public override void Flush()
        {
            // No-op for read-only stream
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readLen = 0;
            long remainingCount = this.Length - this.totalReadedContentData;
            if (remainingCount == 0)
            {
                return -1;
            }
            else if (remainingCount <= count)
            {
                readLen = this.innerContentStream.Read(buffer, offset, (int)remainingCount);
            }
            else
            {
                readLen = this.innerContentStream.Read(buffer, offset, count);
            }

            if (readLen > 0)
            {
                this.totalReadedContentData += readLen;
            }
            else
            {
                if (remainingCount > 0)
                {
                    OpenNextStream();
                    readLen = Read(buffer, offset, count);
                }
            }
            if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            {
                crc.Update(buffer, offset, readLen);
            }
            return readLen;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private long DataBlockGetSize(StorageInfo blockInfo)
        {
            if (this.blockSizeMap.ContainsKey(blockInfo.HighPlusLowName))
            {
                return this.blockSizeMap[blockInfo.HighPlusLowName];
            }
            else
            {
                if (this.logicalDevice.FileExists(blockInfo))
                {
                    long fileSize = 0;
                    var fileInfo = this.logicalDevice.OpenFile(blockInfo);
                    if (fileInfo != null)
                    {
                        fileSize = fileInfo.FileSize;
                        this.blockSizeMap[blockInfo.HighPlusLowName] = fileSize;
                        return fileInfo.FileSize;
                    }
                    return fileSize;
                }
                else
                {
                    throw new FileNotFoundException($"An error occurred while getting file size in {blockInfo.HighPlusLowName}.", blockInfo.LowName, null);
                }
            }
        }
    }
}
