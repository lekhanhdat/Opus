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
using AvePoint.RA.DB.AzureCosmosDB.Exceptions;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Concurrent
{
    public class RMAzureCosmosDBDelayConcurrentAction : IAsyncDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private const int DEFAULT_CONSUME_DELAY_TIME = 200;

        private const int DEFAULT_QUEUE_BOUND = 1000;

        private const int DEFAULT_MAX_CONSUMERS_ALLOWED = 10; //Need dynamic adjust by setting

        private readonly int _maxDegreeOfParallelism;

        private readonly RMAzureCosmosDBRetryer _retryer;

        private readonly Dictionary<RMAzureCosmosDBActionType, Func<Record, Task>> _generalActions;

        private readonly Dictionary<RMAzureCosmosDBActionType, Func<Record, List<PatchOperation>, Task>> _patchActions;

        private readonly Channel<RMAzureCosmosDBDelayConcurrentActionData> _channel;

        private readonly SemaphoreSlim _semaphore = new(1);

        private Func<RMAzureCosmosDBDelayConcurrentActionResult, Task> _notificationCallback;

        private CancellationTokenSource _cts;

        private List<RMAzureCosmosDBDelayConcurrentActionTask> _consumerTasks;

        private readonly object _consumeLock = new();

        private long _processedCount, _failedCount;

        internal RMAzureCosmosDBDelayConcurrentAction(RMAzureCosmosDBContainer container, int retryTimes, int maxDegreeOfParallelism, int initalRetryDelayTime)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _retryer = new RMAzureCosmosDBRetryer(retryTimes, initalRetryDelayTime);
            _channel = Channel.CreateBounded<RMAzureCosmosDBDelayConcurrentActionData>(new BoundedChannelOptions(DEFAULT_QUEUE_BOUND)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
            _generalActions = new Dictionary<RMAzureCosmosDBActionType, Func<Record, Task>>()
            {
                { RMAzureCosmosDBActionType.Add,  container.AddAsync },
                { RMAzureCosmosDBActionType.Upsert, container.UpsertAsync },
                { RMAzureCosmosDBActionType.UpsertWithOptimisticLock, container.UpsertWithOptimisticLockAsync },
                { RMAzureCosmosDBActionType.Replace, container.ReplaceAsync },
                { RMAzureCosmosDBActionType.ReplaceWithOptimisticLock, container.ReplaceWithOptimisticLockAsync },
                { RMAzureCosmosDBActionType.Delete, container.DeleteAsync },
                { RMAzureCosmosDBActionType.DeleteWithOptimisticLock, container.DeleteWithOptimisticLockAsync },
            };
            _patchActions = new Dictionary<RMAzureCosmosDBActionType, Func<Record, List<PatchOperation>, Task>>()
            {
                { RMAzureCosmosDBActionType.Patch, container.PatchAsync },
            };
        }

        #region Lifecycle: Start / Stop
        public async Task StartAsync(Func<RMAzureCosmosDBDelayConcurrentActionResult, Task> notificationCallback, CancellationToken externalCancellation = default)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_cts != null && _consumerTasks != null) return; // already started

                RegisterCallback(notificationCallback);

                _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);

                lock (_consumeLock)
                {
                    _consumerTasks = new List<RMAzureCosmosDBDelayConcurrentActionTask>();

                    for (int i = 0; i < _maxDegreeOfParallelism; i++)
                    {
                        var workerCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                        var task = Task.Run(() => ConsumingAsync(workerCts.Token));
                        _consumerTasks.Add(new RMAzureCosmosDBDelayConcurrentActionTask
                        {
                            Task = task,
                            Cts = workerCts
                        });
                    }
                }

                //_ = Task.Run(() => MonitoringAdjustConsumersAsync(_cts.Token))
                logger.Info($"Started with {_consumerTasks.Count} workers.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task StopAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _cts?.Cancel();

                _channel.Writer.TryComplete();

                List<RMAzureCosmosDBDelayConcurrentActionTask> consumersSnapshot;
                lock (_consumeLock)
                {
                    consumersSnapshot = _consumerTasks?.ToList() ?? new();
                }

                if (consumersSnapshot.Count > 0)
                {
                    try
                    {
                        await Task.WhenAll(consumersSnapshot.Select(t => t.Task)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("One or more consumers threw when stopping.", ex);
                    }
                }
            }
            finally
            {
                lock (_consumeLock)
                {
                    _consumerTasks = null;
                }
                _semaphore.Release();
            }
            logger.Info("RMAzureCosmosDBDelayConcurrentAction stopped.");
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error("Error while disposing RMAzureCosmosDBDelayConcurrentAction.", ex);
            }
            finally
            {
                _semaphore.Dispose();
                _cts?.Dispose();
            }
        }
        #endregion

        #region Consuming logic
        private async Task ConsumingAsync(CancellationToken token)
        {
            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(token).ConfigureAwait(false))
                {
                    RMAzureCosmosDBRetryerResult actionResult = null;
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var action = GetAction(item.ActionType);
                        actionResult = await _retryer.RetryAsync(async () => await action(item.Item, item.PatchOperations));
                        if (!actionResult.IsSucceed) Interlocked.Increment(ref _failedCount);
                        await _notificationCallback(new RMAzureCosmosDBDelayConcurrentActionResult
                        {
                            Item = item.Item,
                            ActionType = item.ActionType,
                            IsSucceed = actionResult.IsSucceed,
                            IsOptimisticLockConflict = actionResult.IsOptimisticLockConflict,
                            CanContinueRetry = actionResult.CanContinueRetry,
                            Exception = actionResult.Exception
                        }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.Info($"Consuming process was cancelled. while consuming {item.Item?.RecordsId}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while consuming item NodeId: {item.Item?.NodeId}", ex);
                        await _notificationCallback(new RMAzureCosmosDBDelayConcurrentActionResult
                        {
                            Item = item.Item,
                            ActionType = item.ActionType,
                            IsSucceed = false,
                            Exception = new RMAzureCosmosDBRetryerException(0, 0, false, false, new() { ex })
                        }).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Increment(ref _processedCount);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Consuming process was cancelled. while consuming data from channel");
                return;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while consuming process.", ex);
                return;
            }
            finally
            {
                logger.Info($"Consumer exiting. Processed Count [{_processedCount}], Failed Count [{_failedCount}].");
            }
        }

        private Func<Record, List<PatchOperation>, Task> GetAction(RMAzureCosmosDBActionType actionType)
        {
            if (actionType == RMAzureCosmosDBActionType.Patch)
            {
                if (!_patchActions.TryGetValue(actionType, out var patchAction))
                    throw new InvalidOperationException($"Patch action type [{actionType}] is not supported.");
                return patchAction;
            }
            else
            {
                if (!_generalActions.TryGetValue(actionType, out var generalAction))
                    throw new InvalidOperationException($"General action type [{actionType}] is not supported.");
                return (record, _) => generalAction(record);
            }
        }
        #endregion

        #region Utilities

        private void RegisterCallback(Func<RMAzureCosmosDBDelayConcurrentActionResult, Task> notificationCallback)
        {
            _notificationCallback = notificationCallback ?? throw new ArgumentNullException(nameof(notificationCallback));
        }

        public Task WaitCompletedAsync() => _consumerTasks != null ? Task.WhenAll(_consumerTasks.Select(c => c.Task)) : Task.CompletedTask;

        public void SetCompleteAdding() => _channel.Writer.Complete();

        #endregion

        #region Action Methods
        public async Task Action(RMAzureCosmosDBActionType actionType, Record record, List<PatchOperation> patchOps = null)
            => await _channel.Writer.WriteAsync(new RMAzureCosmosDBDelayConcurrentActionData(actionType, record, patchOps));

        public async Task Action(RMAzureCosmosDBActionType actionType, IEnumerable<Record> records)
        {
            foreach (var record in records)
            {
                await Action(actionType, record);
            }
        }

        public async Task Action(RMAzureCosmosDBActionType actionType, IEnumerable<(Record, List<PatchOperation>)> recordsWithPatchOps)
        {
            foreach (var (record, patchOps) in recordsWithPatchOps)
            {
                await Action(actionType, record, patchOps);
            }
        }

        public async Task Add(Record record) => await Action(RMAzureCosmosDBActionType.Add, record);
        public async Task Add(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.Add, records);

        public async Task Upsert(Record record) => await Action(RMAzureCosmosDBActionType.Upsert, record);
        public async Task Upsert(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.Upsert, records);
        public async Task UpsertWithOptimisticLock(Record record) => await Action(RMAzureCosmosDBActionType.UpsertWithOptimisticLock, record);
        public async Task UpsertWithOptimisticLock(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.UpsertWithOptimisticLock, records);

        public async Task Replace(Record record) => await Action(RMAzureCosmosDBActionType.Replace, record);
        public async Task Replace(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.Replace, records);
        public async Task ReplaceWithOptimisticLock(Record record) => await Action(RMAzureCosmosDBActionType.ReplaceWithOptimisticLock, record);
        public async Task ReplaceWithOptimisticLock(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.ReplaceWithOptimisticLock, records);

        public async Task Delete(Record record) => await Action(RMAzureCosmosDBActionType.Delete, record);
        public async Task Delete(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.Delete, records);
        public async Task DeleteWithOptimisticLock(Record record) => await Action(RMAzureCosmosDBActionType.DeleteWithOptimisticLock, record);
        public async Task DeleteWithOptimisticLock(IEnumerable<Record> records) => await Action(RMAzureCosmosDBActionType.DeleteWithOptimisticLock, records);

        public async Task Patch(Record record, List<PatchOperation> patchOperations) => await Action(RMAzureCosmosDBActionType.Patch, record, patchOperations);
        public async Task Patch(IEnumerable<(Record, List<PatchOperation>)> recordsWithPatchOperator) => await Action(RMAzureCosmosDBActionType.Patch, recordsWithPatchOperator);
        #endregion
    }
}
