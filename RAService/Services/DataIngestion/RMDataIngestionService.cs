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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.DataIngestion;
using AvePoint.RA.DB.Dao.DataIngestion.Impl;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.DB.Model.DataIngestion;
using AvePoint.RA.Service.Services.DataIngestion.DataStorage;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion;

public class RMDataIngestionService
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionService));

    private readonly IRMDataIngestionJobDao _jobDao = new RMDataIngestionJobDao();

    private readonly IRMKeyValueDao _keyValueDao = new RMKeyValueDao();

    private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

    private IRMDataIngestionMessageDao DataIngestionMessageDao = new RMDataIngestionMessageDao();

    public async Task<RMDataIngestionMessageSendReceipt> SendMessageAsync(RMDataIngestionMessageDto message)
    {
        try
        {
            //await RMRecordStorageAzureTableContext.DataIngestionMessageList.Add(ConvertUtil.ConvertDataIngestionMessageDtoToAzureEntity(message));
            var pendingCount = await DataIngestionMessageDao.GetExecutableMessageCount(message);
            message.Status = FSHighPerformanceUtility.GetNextMessageStatus(pendingCount);

            await DataIngestionMessageDao.AddOrUpdateAsync(ConvertUtil.ConvertDataIngestionMessageDtoToModel(message));
            _logger.Info($"Message has been recorded to system. MessageId: {message.Id.ToString()}, JobId: {message.UniqueId}, status: {message.Status}");
            return new RMDataIngestionMessageSendReceipt
            {
                MessageId = message.Id.ToString(),
                Succeed = true
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while send message. Error: {e}");
            return new RMDataIngestionMessageSendReceipt
            {
                MessageId = string.Empty,
                Succeed = false
            };
        }
    }
    
    public async Task<RMDataIngestionBlobReference> GenerateBlobReferenceAsync(RMDataIngestionBlobNamingContext blobNamingContext)
    {
        try
        {
            var blobClient = RMDataIngestionAzureStorageBlobHandlerFactory.Create(blobNamingContext.IngestionType);
            return await blobClient.GenerateBlobReferenceAsync(blobNamingContext);
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while generating blob reference. Error: {e}");
            return null;
        }
    }

    public async Task<bool> DeleteBlobAsync(RMDataIngestionType ingestionType, string blobName)
    {
        try
        {
            var blobClient = RMDataIngestionAzureStorageBlobHandlerFactory.Create(ingestionType);
            await blobClient.DeleteAsync(blobName);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while deleting blob: [{blobName}]. Error: {e}");
            return false;
        }
    }

    public async Task<string> GenerateBlobSasUri(RMDataIngestionType ingestionType, string blobName)
    {
        try
        {
            var blobClient = RMDataIngestionAzureStorageBlobHandlerFactory.Create(ingestionType);
            var sasUri = await blobClient.GetSasUriAsync(blobName);
            return sasUri;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while generating blob: [{blobName}] SAS URI. Error: {e}");
            return string.Empty;
        }
    }

    public async Task<RMDataIngestionExecutionResult> GetExecutionResultAsync(string uniqueId, string messageId)
    {
        try
        {
            var resultEntity = await RMRecordStorageAzureTableContext.DataIngestionExecuteResultList.FirstOrDefault(
                item => item.PartitionKey == uniqueId && item.RowKey == messageId);

            if (resultEntity == null)
            {
                _logger.Info($"No execution result found for JobId: {uniqueId}, MessageId: {messageId}");
                return null;
            }
            return new RMDataIngestionExecutionResult
            {
                JobId = uniqueId,
                MessageId = messageId,
                SourceBlobName = resultEntity.SourceBlobName,
                ResultBlobName = resultEntity.ResultBlobName,
                Status = (RMDataIngestionStatus)resultEntity.Status,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while retrieving execution result for JobId: {uniqueId}, MessageId: {messageId}. Error: {e}");
            return null;
        }
    }

    public bool SupportsDataIngestion()
    {
        try
        {
            var supportFlagObj = FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature();

            if (!supportFlagObj)
            {
                _logger.Info("Data Ingestion is not supported as per configuration.");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while checking data ingestion support. Error: {e}");
            return false;
        }
    }

    public bool SupportsTriggerNewJobPod()
    {
        try
        {
            var supportFlagObj = _keyValueDao.HasSupportTriggerNewJobPod();

            if (!supportFlagObj)
            {
                _logger.Info("Data Ingestion is not supported trigger new job pod.");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while checking supported trigger new job pod. Error: {e}");
            return false;
        }
    }

    public async Task<bool> HasExecutableMessagesAsync(RMDataIngestionType ingestionType)
    {
        try
        {
            var hasMessages = await DataIngestionMessageDao.HasExecutableMessagesAsync(ingestionType);
            //var hasMessages = await RMRecordStorageAzureTableContext.DataIngestionMessageList.Exists(
            //    msg => msg.IngestionType == (int)ingestionType && msg.Status == (int)RMDataIngestionMessageStatus.Pending);
            _logger.Info($"Check executable messages for IngestionType: {ingestionType}, HasMessages: {hasMessages}");
            return hasMessages;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while checking executable messages for IngestionType: {ingestionType}. Error: {e}");
            return false;
        }
    }

    public async Task ExecuteJobAsync(RMDataIngestionType ingestionType, string uniqueId = null)
    {
        try
        {
            var jobId = string.Format("{0}-{1}", ingestionType.ToString(), uniqueId.Replace("_", "-"));
            await _jobDao.AddOrUpdateAsync(new RMDataIngestionJob
            {
                Id = jobId,
                IngestionType = ingestionType,
                CreatedTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                Status = Contract.RMWeb.JobMonitor.JobStatus.Wait,
                UniqueId = uniqueId,
            });
            _jobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
            {
                JobId = jobId,
                JobType = Contract.JobMonitor.JobType.DataIngestion,
                CommandLine = $"{Contract.JobMonitor.JobType.DataIngestion} {jobId}",
            });
            _logger.Info($"Data Ingestion Job created. JobId: {jobId}, IngestionType: {ingestionType}");
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while executing job for IngestionType: {ingestionType}. Error: {e}");
        }
    }

    public async Task<RMDataIngestionJob> GetExistingDataIngesionJob(string uniqueId)
    {
        return (await _jobDao.GetExistingJobByUniqueId(uniqueId));
    }
}
