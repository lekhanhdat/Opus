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
using AvePoint.GCommon.Utility.PerformanceScope;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.DataIngestion;
using Azure.Storage.Blobs;
using ProtoBuf;
using RAFileSystem.FileSystem.DataSync.V2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataIngestion
{
    public class RMDataIngestionExecutionResultCollector
    {
        private const int ITEM_RESULT_CAPACITY = 10_000;
        private const int POLL_INTERVAL_ACTIVE_MS = 1000;
        private const int POLL_INTERVAL_IDLE_MS = 5000;

        private readonly AveLogger _logger = new AveLogger(typeof(RMDataIngestionExecutionResultCollector));

        private readonly string _jobId;
        public string JobId => _jobId;

        private readonly RMDataIngestionOperationType _operationType;
        public RMDataIngestionOperationType OperationType => _operationType;

        private readonly ConcurrentDictionary<string, RMDataIngestionMessageSendReceipt> _messageSendReceipts;

        private readonly Channel<RMDataIngestionAgentWorkItemExecutionResult> _itemsChannel;

        private RMDataIngestionPersistor _ingestionPersistor;

        private bool _messageAddCompleted = false;

        public RMDataIngestionExecutionResultCollector(string jobId, RMDataIngestionOperationType operationType, RMDataIngestionPersistor ingestionPersistor)
        {
            _jobId = jobId;
            _operationType = operationType;
            _ingestionPersistor = ingestionPersistor;
            _messageSendReceipts = new ConcurrentDictionary<string, RMDataIngestionMessageSendReceipt>();
            _itemsChannel = Channel.CreateBounded<RMDataIngestionAgentWorkItemExecutionResult>(new BoundedChannelOptions(ITEM_RESULT_CAPACITY)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = true,
            });
            _ = Task.Run(() => MonitorMessageExecutionResultAsync());
            ProtobufRuntimeHelper.EnsureTypeRegistered<RMDataIngestionAgentWorkItemExecutionResult>();
        }

        public void AddMessageSendReceipt(RMDataIngestionMessageSendReceipt receipt)
        {
            if (!_messageSendReceipts.ContainsKey(receipt.MessageId))
            {
                _messageSendReceipts.TryAdd(receipt.MessageId, receipt);
            }
        }

        private async Task MonitorMessageExecutionResultAsync()
        {
            while (!_messageAddCompleted || !_messageSendReceipts.IsEmpty)
            {
                foreach (var kvp in _messageSendReceipts)
                {
                    try
                    {
                        var result = HybridApiClient.Instance.DataIngestionGetExecutionResult(_jobId, kvp.Key);
                        if (result == null || (result.Status != RMDataIngestionStatus.Succeed && result.Status != RMDataIngestionStatus.Failed))
                            continue;
                        await ProcessMessageExecutionResult(result);
                        _messageSendReceipts.TryRemove(kvp.Key, out _);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to process message {0}. Will retry next cycle. Exception: {1}", kvp.Key, ex);
                    }
                }
                await Task.Delay(!_messageSendReceipts.IsEmpty ? POLL_INTERVAL_ACTIVE_MS : POLL_INTERVAL_IDLE_MS);
            }
            _itemsChannel.Writer.Complete();
        }

        private async Task ProcessMessageExecutionResult(RMDataIngestionExecutionResult executionResult)
        {
            using (new AgentPerformanceScope("ExecutionResultCollector.ProcessMessageExecutionResult", addToStatistics: true))
            {
                if (executionResult.Status == RMDataIngestionStatus.Failed)
                {
                    await ProcessFailedMessage(executionResult.MessageId);
                    return;
                }
                await ProcessSucceedMessage(executionResult.ResultBlobName, executionResult.MessageId);
                _logger.Info($"Successfully processed execution result for message: {executionResult.MessageId}.");
            }
        }

        private async Task ProcessFailedMessage(string messageId)
        {
            using (new AgentPerformanceScope("ExecutionResultCollector.ProcessFailedMessage", addToStatistics: true))
            {
                try
                {
                    await _ingestionPersistor.ReadAsync<RMDataIngestionAgentWorkItemExecutionResult>(messageId, async res =>
                    {
                        await _itemsChannel.Writer.WriteAsync(res).AsTask();
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to process succeed message execution result. Exception: {ex}");
                }
            }
        }

        private async Task ProcessSucceedMessage(string blobName, string messageId)
        {
            using (new AgentPerformanceScope("ExecutionResultCollector.ProcessSucceedMessage", addToStatistics: true))
            {
                try
                {
                    var failedIdsTask = DownloadFailedIdsAsync(blobName);
                    var failedIds = await failedIdsTask;
                    await _ingestionPersistor.ReadAsync<RMDataIngestionAgentWorkItemExecutionResult>(messageId, res =>
                    {
                        if (failedIds.TryGetValue(res.Id, out var message))
                        {
                            res.Message = message;
                        }
                        else
                        {
                            res.Succeed = true;
                        }
                        return _itemsChannel.Writer.WriteAsync(res).AsTask();
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to process succeed message execution result. Exception: {ex}");
                }
                finally
                {
                    DeleteResultBlob(blobName);
                }
            }
        }

        private async Task<Dictionary<Guid, string>> DownloadFailedIdsAsync(string blobName)
        {
            var failedIds = new Dictionary<Guid, string>();
            var sasUri = HybridApiClient.Instance.DataIngestionGenerateBlobSasUri(RMDataIngestionType.AgentWork, blobName);
            var blobClient = new BlobClient(new Uri(sasUri));
            using (var streamReader = await blobClient.OpenReadAsync())
            {
                var items = Serializer.DeserializeItems<RMDataIngestionAgentWorkItemExecutionResult>(streamReader, PrefixStyle.Base128, 1);
                foreach (var item in items)
                {
                    if (!failedIds.ContainsKey(item.Id))
                    {
                        failedIds.Add(item.Id, item.Message);
                    }
                    else
                    {
                        _logger.Warn($"Duplicate execution result id: {item.Id} in blob: {blobName}.");
                    }
                }
            }
            return failedIds;
        }

        public void SetMessageAddCompleted()
        {
            _messageAddCompleted = true;
        }

        public async Task EnqueueNonIngestionDataAsync(IEnumerable<RMDataIngestionAgentWorkItemExecutionResult> res)
        {
            try
            {
                foreach (var item in res)
                {
                    await _itemsChannel.Writer.WriteAsync(item).AsTask();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to write non-ingestion execution result to channel. Exception: {ex}");
            }
        }

        public IEnumerable<RMDataIngestionAgentWorkItemExecutionResult> ReadItemExecutionResults()
        {
            while (Task.Run(() => _itemsChannel.Reader.WaitToReadAsync().AsTask()).GetAwaiter().GetResult())
            {
                while (_itemsChannel.Reader.TryRead(out var item))
                {
                    yield return item;
                }
            }
        }

        public Task ReadItemExecutionResultsAsync(Action<RMDataIngestionAgentWorkItemExecutionResult> callback, CancellationToken token)
        {
            return _itemsChannel.Reader.DrainChannelAsync(res =>
            {
                callback(res);
                return Task.CompletedTask;
            }, token);
        }

        private void DeleteResultBlob(string blobName)
        {
            try
            {
                _logger.Info($"Start to delete result blob: {blobName} in job: {_jobId} after processing.");
                var deleteBlobResult = HybridApiClient.Instance.DeleteBlobByName(new RMDataIngestionBlobDto
                {
                    BlobName = blobName,
                    IngestionType = RMDataIngestionType.AgentWork
                });
                if (!deleteBlobResult)
                {
                    _logger.Warn($"Failed to delete result blob: {blobName} in job: {_jobId} after processing.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete result blob: {blobName}. Exception: {ex}");
            }
        }
    }
}
