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
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.FileSystem.Collect;
using RAFileSystem.FileSystem.DataIngestion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSPersistWorker : IFSDataSyncWorker
    {
        private static int _workerCounter;
        private readonly AveLogger _logger;
        private FSDataSyncChannelProvider _channelProvider;
        private RMDataIngestionDataCollector _ingestionDataCollector;
        private RMDataIngestionExecutionResultCollector _ingestionExecutionResultCollector;
        private readonly CancellationToken _token;
        private readonly FSUniqueIdAssigner _uniqueIdAssigner;
        private int DEFAULT_BATCH_SIZE => ConfigUtils.WORKER_TRANSFER_DATA_COUNT;

        public FSPersistWorker(FSDataSyncChannelProvider channelProvider, RMDataIngestionDataCollector ingestionDataCollector, RMDataIngestionExecutionResultCollector ingestionExecutionResultCollector, CancellationToken token)
        {
            var workerId = Interlocked.Increment(ref _workerCounter);
            _logger = AveLogger.GetInstance(typeof(FSPersistWorker), $"FSPersistWorker-{workerId}");
            _channelProvider = channelProvider;
            _ingestionDataCollector = ingestionDataCollector;
            _ingestionExecutionResultCollector = ingestionExecutionResultCollector;
            _uniqueIdAssigner = new FSUniqueIdAssigner(FSDataCollectorV2.UniqueIdSetting, FSDataCollectorV2.ClassificationLevel);
            _token = token;
            ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info("FSPersistWorker started.");
                using (new AgentPerformanceScope("PersistWorker.Run", addToStatistics: true))
                {
                    await RunBufferedPersistAsync();
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.Warn("FSPersistWorker canceled.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred in FSPersistWorker. Error:{0}", ex.ToString());
            }
            _logger.Info("FSPersistWorker completed successfully.");
        }

        private async Task RunBufferedPersistAsync()
        {
            using (new AgentPerformanceScope("PersistWorker.RunBufferedPersistAsync", addToStatistics: true))
            {
                const int BufferSize = 100;
                const int MaxDegreeOfParallelism = 10;

                using (var bufferCollection = new BlockingCollection<FileSystemRecordDto>(BufferSize))
                {
                    var producerTask = ProducePersistRecordsAsync(bufferCollection);
                    var consumerTask = Enumerable.Range(0, MaxDegreeOfParallelism).Select(_ => ConsumePersistRecordAsync(bufferCollection));

                    await Task.WhenAll(new[] { producerTask }.Concat(consumerTask));
                    _logger.Info("Producer and consumer tasks completed");
                }
            }
        }

        private async Task ProducePersistRecordsAsync(BlockingCollection<FileSystemRecordDto> bufferCollection)
        {
            try
            {
                _logger.Info("Producer started - draining PersistChannel to buffer");
                var buffer = new List<FileSystemRecordDto>(DEFAULT_BATCH_SIZE);
                await _channelProvider.PersistChannel.Reader.DrainChannelAsync(
                   record =>
                   {
                       bufferCollection.Add(record, _token);
                       return Task.CompletedTask;
                   },
                   _token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warn("Producer canceled");
            }
            catch (Exception ex)
            {
                _logger.Error($"Producer error: {ex}");
            }
            finally
            {
                bufferCollection.CompleteAdding();
                _logger.Info("Producer marked collection as complete");
            }
        }

        private async Task ConsumePersistRecordAsync(BlockingCollection<FileSystemRecordDto> bufferCollection)
        {
            _logger.Info("Consumer started - processing records from buffer");
            var batch = new List<FileSystemRecordDto>(DEFAULT_BATCH_SIZE);
            foreach (var item in bufferCollection.GetConsumingEnumerable(_token))
            {
                batch.Add(item);
                if (batch.Count >= DEFAULT_BATCH_SIZE)
                {
                    await FlushBatchAsync(batch);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                await FlushBatchAsync(batch);
            }
            _logger.Info("Consumer completed - all records processed");
        }

        private async Task FlushBatchAsync(List<FileSystemRecordDto> records)
        {
            using (new AgentPerformanceScope("PersistWorker.FlushBatch", addToStatistics: true))
            {
                if (records == null || records.Count == 0) return;

                _logger.Debug("Start to sync data to explorer. Batch size:{0}", records.Count);
                var failedRecords = new List<FileSystemRecordDto>();
                var needSyncRecords = new List<FileSystemRecordDto>();
                try
                {
                    using (new AgentPerformanceScope("PersistWorker.SyncRecords", addToStatistics: true))
                    {
                        var assignResult = _uniqueIdAssigner.AssignUniqueIds(records);
                        var failedIdSet = assignResult.FailedNodeIds != null && assignResult.FailedNodeIds.Count > 0 ? new HashSet<Guid>(assignResult.FailedNodeIds) : null;
                        if (failedIdSet != null)
                        {
                            foreach (var id in failedIdSet)
                            {
                                FSJobCache.Instance.CurrentJobFailedItemIds.Add(id);
                            }
                        }

                        _logger.Info($"Unique ID assignment complete. Success: [{assignResult.SuccessCount}], Failed: [{assignResult.FailureCount}]");

                        if (failedIdSet == null)
                        {
                            needSyncRecords.AddRange(records);
                        }
                        else
                        {
                            foreach (var r in records)
                            {
                                if (failedIdSet.Contains(r.NodeId))
                                {
                                    failedRecords.Add(r);
                                    continue;
                                }
                                needSyncRecords.Add(r);
                            }
                        }

                        if (needSyncRecords.Count > 0)
                        {
                            await WriteDataToCollectorAsync(needSyncRecords);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Flush batch records failed. Error:{0}", ex);
                    foreach (var r in records)
                    {
                        FSJobCache.Instance.CurrentJobFailedItemIds.Add(r.NodeId);
                    }
                    failedRecords = records;
                }
                finally
                {
                    if (failedRecords.Count > 0)
                    {
                        await ProcessNonIngestionDataAsync(failedRecords);
                        _logger.Warn($"Processed {failedRecords.Count} failed records separately.");
                    }
                }
                _logger.Debug("Finished syncing data to explorer.");
            }
        }

        private async Task WriteDataToCollectorAsync(List<FileSystemRecordDto> records)
        {
            using (new AgentPerformanceScope("PersistWorker.WriteDataToCollector", addToStatistics: true))
            {
                if (records.Count == 0) return;

                var blobDataList = new ConcurrentBag<FSDataIngestion<FileSystemRecordDto>>();
                var localDataList = new ConcurrentBag<RMDataIngestionAgentWorkItemExecutionResult>();

                Parallel.ForEach(records, new ParallelOptions { MaxDegreeOfParallelism = 5 }, record =>
                {
                    blobDataList.Add(new FSDataIngestion<FileSystemRecordDto> { Item = record });
                    localDataList.Add(ConvertToExecutionResult(record));
                });

                await _ingestionDataCollector.WriteBatchAsync(blobDataList.ToList(), localDataList.ToList());
                _logger.Info($"Persisted {records.Count} records to ingestion collector.");
            }
        }

        private async Task ProcessNonIngestionDataAsync(List<FileSystemRecordDto> records)
        {
            var localDataList = records.Select(ConvertToExecutionResult).ToList();
            await _ingestionExecutionResultCollector.EnqueueNonIngestionDataAsync(localDataList);
        }

        private RMDataIngestionAgentWorkItemExecutionResult ConvertToExecutionResult(FileSystemRecordDto record)
        {
            return new RMDataIngestionAgentWorkItemExecutionResult
            {
                Id = record.NodeId,
                NodeType = record.NodeType,
                LeafName = record.LeafName,
                DirPath = record.DirPath,
                Depth = record.Depth,
                HasRuleChanged = record.HasRuleChanged,
                HasTermChanged = record.HasTermChanged,
            };
        }
    }
}