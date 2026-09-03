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
using System.Threading.Tasks;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.RACommonUtility.Common;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using RAFileSystem.FSBatchUpload.Processor;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

namespace RAFileSystem.FSBatchUpload
{
    public interface IFSBatchHandler : IDisposable
    {
        Task ProcessBatchJobAsync(string messageId, FSBatchUploadNotification batchJobMessage);
        QueueClient GetQueueClient();
        TableClient GetTableClient();
        string GetBlobSasUriForWrite(string blobName);
    }

    public class FSBatchHandler : IFSBatchHandler
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FSBatchHandler));
        private readonly string JobId;
        private readonly JobType jobType;

        private string _sharedCS;
        public string SharedCS
        {
            get
            {
                if (!string.IsNullOrEmpty(_sharedCS))
                    return _sharedCS;

                _sharedCS = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);

                if (string.IsNullOrEmpty(_sharedCS))
                {
                    logger.Error("Cannot get the shared storage connection string, pls check the config file");
                    throw new Exception("RM_HS_Criteria_View_Msg_ValidOtherError");
                }

                return _sharedCS;
            }
        }

        private const string defaultContainerName = "sharedlocation";
        private const string defaultRootFolder = "batch_upload";

        private string _containerName;
        public string ContainerName
        {
            get
            {
                if (!string.IsNullOrEmpty(_containerName))
                    return _containerName;
                _containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                if (string.IsNullOrEmpty(_containerName))
                {
                    logger.Warn("Cannot get the shared storage container name, pls check the config file");
                    _containerName = defaultContainerName;
                }
                return _containerName;
            }
        }

        private QueueClient _queueClient;
        private TableClient _tableClient;
        public QueueClient GetQueueClient() => _queueClient;

        public TableClient GetTableClient() => _tableClient;

        public FSBatchHandler(string jobId, JobType jobType)
        {
            this.JobId = jobId;
            this.jobType = jobType;
            Initialize();
        }

        public void Initialize()
        {
            _queueClient ??= CreateQueueClient();
            _tableClient ??= CreateTableClient();
        }

        public QueueClient CreateQueueClient()
        {
            var queueName = $"{JobId.ToLowerInvariant().Replace("_", "-")}"; // Queue names must be lowercase and no special characters except hyphen
            var client = new QueueClient(SharedCS, queueName);
            client.CreateIfNotExists();
            return client;
        }

        public TableClient CreateTableClient()
        {
            string tableName = $"FSBatchReport{jobType}{JobId.Replace("_", "")}";
            var client = new TableClient(SharedCS, tableName);
            client.CreateIfNotExists();
            return client;
        }

        public string GetBlobSasUriForWrite(string blobName)
        {
            try
            {
                var blobPath = Path.Combine(defaultRootFolder, JobId, "batch_data", blobName).Replace("\\", "/");
                return Util.MSAzure.StorageUtil.GenerateSasUriForWrite(SharedCS, ContainerName, blobPath, TimeSpan.FromDays(7));
            }
            catch (Exception e)
            {
                logger.Error($"Error generating SAS URI for blob: {e.Message}");
                throw;
            }
        }

        public BlobClient CreateBlobClient(string blobName, string parentFolder = "")
        {
            var blobPath = Path.Combine(defaultRootFolder, JobId, parentFolder, blobName).Replace("\\", "/");
            var client = new BlobClient(SharedCS, ContainerName, blobPath);
            return client;
        }

        public async Task ProcessBatchJobAsync(string messageId, FSBatchUploadNotification batchJobMessage)
        {
            try
            {
                string className = $"{typeof(IFSBatchProcessorBase).Namespace}.FS{batchJobMessage.OperationType}BatchProcessor";
                Type workerType = Type.GetType(className, throwOnError: false);

                if (workerType == null)
                {
                    logger.Error($"Worker type not found: {className}");
                    return;
                }

                var worker = Activator.CreateInstance(workerType);

                if (worker is IFSBatchProcessorBase batchWorker)
                {
                    var inputClient = CreateBlobClient(batchJobMessage.BlobName, "batch_data");
                    var reportBlobName = $"r_{messageId}.fsb";
                    var reportBlobClient = CreateBlobClient(reportBlobName, "batch_reports");

                    await batchWorker.InitializeAsync(inputClient, _tableClient, reportBlobClient);
                    logger.Info($"Initialized batch worker for messageId: {messageId}");

                    await batchWorker.ExecuteBatchAsync(batchJobMessage, messageId);
                    logger.Info($"Completed processing batch job. MessageId: {messageId}");
                }
                else
                {
                    logger.Error($"Worker {className} with Operation type {batchJobMessage.OperationType} does not implement IFSBatchProcessorBase.");
                    throw new InvalidOperationException($"The worker type {className} is not valid for batch processing.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error processing batch job. MessageId: {messageId}, Error: {e}");
            }
        }

        public void Dispose()
        {
            try
            {
                _queueClient.DeleteIfExists();
                _tableClient.Delete();
                string batchReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatchTempFolder", JobId);
                if (Directory.Exists(batchReportPath))
                {
                    Directory.Delete(batchReportPath, true);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error during disposal of FSBatchHandler for JobId: {JobId}, Error: {e}");
            }
        }
    }
}
