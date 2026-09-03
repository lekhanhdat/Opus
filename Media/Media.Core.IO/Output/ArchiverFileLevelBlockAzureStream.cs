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
using Storage;
using Storage.Cloud.Azure;
using System.Reflection;
using AvePoint.Media.Core.IO.Output;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Core.IO;

namespace Media.Core.IO.Output
{
    internal class ArchiverFileLevelBlockAzureStream : Stream
    {
        private AveLogger logger = new AveLogger(typeof(ArchiverFileLevelBlockAzureStream));
        private const int MAX_TEMP_MEMORY_STREAM_LIMIT = 1024 * 1024 * 50;
        private StorageInfo fileInfo;
        private MemoryStream tempStream;
        private IAzureSystem azureSystem;
        private long totalLength;
        private bool isMultipartUpload;
        private List<String> blockIdBase64list = new List<String>();
        private int multipartBlockIndex = 0;
        private IOutputDataHandler<ArchiverBasicIndex> datahander;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this.totalLength;

        public override long Position { get => this.totalLength; set => throw new NotImplementedException(); }

        public ArchiverFileLevelBlockAzureStream(IXSystem dataLogicalDevice, StorageInfo fileInfo, IOutputDataHandler<ArchiverBasicIndex> outputDataHandler)
        {
            this.tempStream = new MemoryStream(MAX_TEMP_MEMORY_STREAM_LIMIT);
            this.fileInfo = fileInfo;
            this.datahander = outputDataHandler;

            var preparedMetaInfos = new Dictionary<String, String>();
            foreach (KeyValuePair<String, String> entry in fileInfo.MetaInfos)
            {
                preparedMetaInfos[entry.Key] = entry.Value != null ? Storage.Util.EncodeHelper.StoragePathEncode(entry.Value) : entry.Value;
            }
            this.fileInfo.MetaInfos = preparedMetaInfos;

            var storageLibrary = dataLogicalDevice as XLibrary;
            if (storageLibrary != null)
            {
                azureSystem = storageLibrary?.SubSystems?.FirstOrDefault() as IAzureSystem;
            }
            else
            {
                azureSystem = dataLogicalDevice as IAzureSystem;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            totalLength += count - offset;
            var remainder = (int)tempStream.Length + count - MAX_TEMP_MEMORY_STREAM_LIMIT;
            if (remainder > 0)
            {
                var writeCount = MAX_TEMP_MEMORY_STREAM_LIMIT - (int)tempStream.Length;
                if (writeCount > 0)
                {
                    tempStream.Write(buffer, offset, writeCount);
                }

                RealWriteBlock();

                tempStream = new MemoryStream(MAX_TEMP_MEMORY_STREAM_LIMIT);
                tempStream.Write(buffer, offset + writeCount, remainder);
            }
            else
            {
                tempStream.Write(buffer, offset, count);
            }
        }

        private void RealWriteBlock()
        {
            using (this.tempStream)
            {
                this.isMultipartUpload = true;
                this.fileInfo.Length = this.tempStream.Length;

                var blockIdBase64 = Convert.ToBase64String(BitConverter.GetBytes(this.multipartBlockIndex));
                azureSystem.PutBlockAsync(blockIdBase64, this.tempStream, this.fileInfo).GetAwaiter().GetResult();
                logger.Debug($"Upload the partial block_{this.multipartBlockIndex} to azure storage succeed, length is {this.fileInfo.Length}.");
                blockIdBase64list.Add(blockIdBase64);
                this.multipartBlockIndex++;
            }
        }

        private void SetBlobMetadata()
        {
            var systemType = azureSystem.GetType();
            var azureClientProp = systemType.GetProperty("Client", BindingFlags.Instance | BindingFlags.Public);
            var buildObjUrlFunc = systemType.GetMethod("BuildObjectUrl", BindingFlags.Instance | BindingFlags.NonPublic);
            var buildUrlTask = (Task<string>)buildObjUrlFunc.Invoke(azureSystem, new object[] { this.fileInfo });
            var reqUrl = buildUrlTask.GetAwaiter().GetResult();
            var azureClient = azureClientProp.GetValue(azureSystem);
            var setBlobMetadataFunc = azureClient.GetType().GetMethod("SetBlobMetadataAsync", BindingFlags.Instance | BindingFlags.Public);
            var setBlobMetadataTask = (Task<bool>)setBlobMetadataFunc.Invoke(azureClient, new object[] { reqUrl, this.fileInfo.MetaInfos });
            setBlobMetadataTask.GetAwaiter().GetResult();
            logger.Info($"Complete set azure blob metadata, url='{this.fileInfo.HighPlusLowName}'");
        }

        public override void Close()
        {
            if (this.multipartBlockIndex > 0)
            {
                if (tempStream.Length > 0)
                {
                    RealWriteBlock();
                }
                azureSystem.PutBlockListAsync(blockIdBase64list, this.fileInfo).GetAwaiter().GetResult();
                logger.Info($"Complete multipart upload azure blob succeed, url='{this.fileInfo.HighPlusLowName}'");

                //SetBlobMetadata();
            }
            else
            {
                using (this.tempStream)
                {
                    if (this.tempStream.Length > 0)
                    {
                        azureSystem.UploadAsync(this.tempStream, this.fileInfo).GetAwaiter().GetResult();
                    }
                }
            }
            this.datahander.IncreaseMediaDataSize(totalLength);
            logger.Info($"Upload the azure blob file '{this.fileInfo.HighPlusLowName}' successfully, length is {totalLength}.");
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
