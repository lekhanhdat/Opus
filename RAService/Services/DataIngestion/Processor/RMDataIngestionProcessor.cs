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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.DataIngestion;
using AvePoint.RA.DB.Dao.DataIngestion.Impl;
using AvePoint.RA.DB.Model.DataIngestion;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork;
using Azure;
using Azure.Data.Tables.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor;

public class RMDataIngestionProcessor
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionProcessor));

    private readonly IRMDataIngestionJobDao _jobDao = new RMDataIngestionJobDao();

    private readonly IRMDataIngestionMessageDao _messageDao = new RMDataIngestionMessageDao();

    private readonly string _jobId;

    private readonly PeriodicTimer _refreshModifiedTimeTimer;

    private long _successCount;

    private long _failedCount;

    public RMDataIngestionProcessor(string jobId)
    {
        _jobId = jobId;
        _refreshModifiedTimeTimer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        _ = LoopRefreshModifiedTimeAsync();
    }

    public async Task ProcessAsync()
    {
        try
        {
            var fSHighPerformanceConfiguration = FSHighPerformanceUtility.LoadFSHighPerformanceConfig();
            var maxDegreeOfParallelism = fSHighPerformanceConfiguration.Setting.MaxDegreeOfParallelism;
            var jobInfo = await _jobDao.GetByIdAsync(_jobId);
            if (jobInfo == null) return;

            _logger.Info($"Start processing. JobId: {_jobId}, IngestionType: {jobInfo.IngestionType}, MaxParallelism: {maxDegreeOfParallelism}.");
            List<Task> tasksList = [];
            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                tasksList.Add(Task.Run(() => ConsumeMessagesFromSqlTable(jobInfo)));
            }
            await Task.WhenAll(tasksList);

            var finalStatus = DetermineFinalStatus();
            await _jobDao.UpdateStatusAsync(_jobId, finalStatus, DateTime.UtcNow.Ticks);
            _logger.Info($"Finished processing data ingestion. JobId: {_jobId} FinalStatus: {finalStatus}");
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while processing data ingestion. JobId: {_jobId} Error: {e}");
            throw;
        }
    }

    private JobStatus DetermineFinalStatus()
    {
        if (Interlocked.Read(ref _successCount) == 0) return JobStatus.Failed;
        if (Interlocked.Read(ref _failedCount) > 0) return JobStatus.FinishWithException;
        return JobStatus.Finished;
    }

    private async Task ConsumeMessagesFromSqlTable(RMDataIngestionJob jobInfo)
    {
        var ideRetryCount = 0;
        const int maxIdeRetry = 5;

        while (true)
        {
            var message = await _messageDao.TryClaimNextMessageAsync();
            if (message == null)
            {
                if (ideRetryCount < maxIdeRetry)
                {
                    ideRetryCount++;
                    await Task.Delay(ideRetryCount * 2000);
                    continue;
                }
                ideRetryCount = 0;

                if (await ShouldExitConsumeLoopAsync(jobInfo))
                {
                    _logger.Info("Finished all message, exit thread");
                    break;
                }
                continue;
            }
            try
            {
                ideRetryCount = 0;
                var messageProcessor = GetMessageProcessor(jobInfo.IngestionType, message);
                var succeed = await messageProcessor.ProcessAsync();
                if (succeed) Interlocked.Increment(ref _successCount);
                else Interlocked.Increment(ref _failedCount);
                _logger.Info($"Processed message. JobId: {_jobId}, MessageId: {message.Id}, Result: {succeed}.");
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCount);
                _logger.Error($"An error occurred while processing message Id: {message.Id} for JobId: {_jobId}. Error: {ex}");
                continue;
            }
            finally
            {
                await _messageDao.DeleteAsync(message.Id);
                _logger.Info($"Deleted message Id: {message.Id} for JobId: {_jobId}.");
            }
        }
    }
    
    private async Task<bool> ShouldExitConsumeLoopAsync(RMDataIngestionJob jobInfo)
    {   
        var hasWaitingMessage = _messageDao.Exist(result => result.UniqueId == jobInfo.UniqueId && result.Status == RMDataIngestionMessageStatus.Waiting);
        if (hasWaitingMessage)
        {
            await _messageDao.PrepareNextMessageAsync();
            return false;
        }
        return true;
    }

    public async Task ConsumeMessagesFromAzureTable(RMDataIngestionJob jobInfo)
    {
        while (true)
        {
            RMDataIngestionMessageTableEntity message = null;
            try
            {
                message = await RMRecordStorageAzureTableContext.DataIngestionMessageList
                    .FirstOrDefault(item => item.Status == (int)RMDataIngestionMessageStatus.Pending);

                if (message == null)    
                {
                    _logger.Info($"No more pending messages to process for JobId: {_jobId}.");
                    break;
                }

                message.Status = (int)RMDataIngestionMessageStatus.Processing;
                await RMRecordStorageAzureTableContext.DataIngestionMessageList.UpsertMerge(message);
                _logger.Info($"Leased message {message.RowKey} for processing.");
            }
            catch (RequestFailedException ex) when (ex.Status == 412 || ex.ErrorCode == TableErrorCode.UpdateConditionNotSatisfied)
            {
                _logger.Info($"Message {message?.RowKey} was picked up by another process. Retrying...");
                continue;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching/leasing message: {ex}");
                break;
            }

            try
            {
                var messageProcessor = GetMessageProcessor(jobInfo.IngestionType, ConvertUtil.ConvertDataIngestionMessageAzureEntityToModel(message));
                var succeed = await messageProcessor.ProcessAsync();
                if(succeed) Interlocked.Increment(ref _successCount);
                else Interlocked.Increment(ref _failedCount);
                await RMRecordStorageAzureTableContext.DataIngestionMessageList.Delete(message.PartitionKey, message.RowKey);
                _logger.Info($"Processed message successfully. JobId: {_jobId}, MessageId: {message.RowKey}, Result: {succeed}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Processing error: {message.RowKey}. Error: {ex}");
                Interlocked.Increment(ref _failedCount);
            }
        }
    }

    private RMDataIngestionMessageProcessor GetMessageProcessor(RMDataIngestionType ingestionType, RMDataIngestionMessage message)
    {
        return ingestionType switch
        {
            RMDataIngestionType.AgentWork => new RMDataIngestionAgentWorkMessageProcessor(message),
            _ => throw new NotSupportedException($"Ingestion type not supported: {ingestionType}"),
        };
    }

    private async Task LoopRefreshModifiedTimeAsync()
    {
        while (await _refreshModifiedTimeTimer.WaitForNextTickAsync())
        {
            try
            {
                await _jobDao.UpdateModifiedTimeAsync(_jobId, DateTime.UtcNow.Ticks);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while refreshing modified time for JobId: {_jobId}, Error: {e}");
            }
        }
    }
}
