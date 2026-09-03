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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace RAFileSystem.FSBatchUpload.Processor
{
    /// <summary>
    ///  Implement class must named "FS{OperationType}BatchProcessor"
    /// </summary>
    public interface IFSBatchProcessorBase
    {
        Task InitializeAsync(BlobClient dataBlob, TableClient tableClient, BlobClient reportBlobClient);
        Task ExecuteBatchAsync(FSBatchUploadNotification batchJobMessage, string messageId);
    }

    public abstract class FSBatchProcessorBase<T> : IFSBatchProcessorBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSBatchProcessorBase<T>));
        protected abstract Task<List<FSItemReportDto>> ProcessBatchAsync(BatchPackage<T> item);

        protected BlobClient _datablobClient;
        protected TableClient _tableClient;
        protected BlobClient _reportBlobClient;

        public virtual async Task InitializeAsync(BlobClient dataBlob, TableClient tableClient, BlobClient reportBlobClient)
        {
            ProtobufRuntimeHelper.EnsureTypeRegistered<T>();
            _datablobClient = dataBlob;
            _tableClient = tableClient;
            _reportBlobClient = reportBlobClient;
        }

        public async Task ExecuteBatchAsync(FSBatchUploadNotification batchJobMessage, string messageId)
        {
            var entity = new FSBatchUploadResultTableEntity
            {
                PartitionKey = batchJobMessage.JobId,
                RowKey = messageId,
                StartTime = DateTime.UtcNow.Ticks,
                Status = JobDetailsStatus.Successful,
            };

            BatchPackage<T> batchData;
            try
            {
                batchData = await DownloadAndParseBatchAsync<T>();
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to deserialize input batch. JobId: {batchJobMessage.JobId}, BatchId: {messageId}. Error: {ex}");
                entity.Status = JobDetailsStatus.Failed;
                entity.ErrorMessage = ex.Message;
                await SaveBatchReportAsync(entity);
                throw;
            }

            var batchResponse = new FSBatchReportDto
            {
                MessageId = messageId,
                TotalItems = batchData.Items.Count,
                BatchSize = batchData.BatchSize,
                BatchStatus = JobDetailsStatus.Pending,
                Records = [],
            };

            batchResponse.Records = await ProcessBatchAsync(batchData);

            batchResponse.ProcessedItems = batchResponse.Records.Count;
            batchResponse.BatchStatus = JobDetailsStatus.Successful;

            try
            {
                entity.SASURI = await UploadResultAndGetSasAsync(batchResponse, batchJobMessage.JobId);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to upload result blob. BatchId: {messageId}. Error: {ex}");
                entity.Status = JobDetailsStatus.Failed;
                entity.ErrorMessage = ex.Message;
                await SaveBatchReportAsync(entity);
                return;
            }

            await SaveBatchReportAsync(entity);
        }

        private async Task<BatchPackage<TItem>> DownloadAndParseBatchAsync<TItem>()
        {
            if (!await _datablobClient.ExistsAsync())
            {
                logger.Error($"Blob not found: {_datablobClient.Name}");
                throw new FileNotFoundException($"Blob not found: {_datablobClient.Name}");
            }

            using (Stream blobStream = await _datablobClient.OpenReadAsync())
            {
                return FileSystemContractHelper.DeserializerProtoBuf<BatchPackage<TItem>>(blobStream);
            }
        }

        private async Task SaveBatchReportAsync(FSBatchUploadResultTableEntity batchReportEntity)
        {
            batchReportEntity.FinishTime = DateTime.UtcNow.Ticks;
            await _tableClient.UpsertEntityAsync(batchReportEntity);
        }

        private async Task<string> UploadResultAndGetSasAsync(FSBatchReportDto batchReport, string jobId)
        {
            var fileName = _reportBlobClient.Name.Split("\\").Last();
            if (batchReport.BatchSize >= 0 && batchReport.BatchSize < FileSystemContractHelper.MEMORY_STREAM_THRESHOLD)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        FileSystemContractHelper.SerializerProtoBuf(memoryStream, batchReport);
                        memoryStream.Position = 0;
                        await _reportBlobClient.UploadAsync(memoryStream, true);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"SerializeAndUploadAsync failed: {e.Message}");
                    throw;
                }
            }
            else
            {
                string batchReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatchTempFolder", jobId, "report");
                if (!Directory.Exists(batchReportPath))
                {
                    Directory.CreateDirectory(batchReportPath);
                }

                var tempFile = Path.Combine(batchReportPath, fileName);

                try
                {
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        FileSystemContractHelper.SerializerProtoBuf(fileStream, batchReport);
                    }

                    using (var readStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                    {
                        await _reportBlobClient.UploadAsync(readStream, true);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"SerializeAndUploadAsync failed: {e.Message}");
                    throw;
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }

            if (_reportBlobClient.CanGenerateSasUri)
            {
                return _reportBlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(7)).ToString();
            }
            else
            {
                logger.Error("Cannot generate SAS URI. Check connection string.");
                throw new InvalidOperationException("Cannot generate SAS URI. Check connection string.");
            }
        }
    }
}
