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
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.AzureCosmosDB.Query.Linq;
using AvePoint.RA.DB.AzureCosmosDB.Query.SQL;
using AvePoint.RA.DB.AzureCosmosDB.WriteMode;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB
{
    public class RMAzureCosmosDBService
    {
        private readonly RMAzureCosmosDBContainer _container;
        private RMAzureCosmosDBDelayConcurrentAction _delayConcurrent;
        private RMAzureCosmosDBImmediatelyConcurrentAction _immediatelyConcurrent;

        public RMAzureCosmosDBService(bool initIfNotExists = false)
        {
            _container = RMAzureCosmosDBContext.GetContainerAsync(initIfNotExists).GetAwaiter().GetResult();
        }

        public RMAzureCosmosDBService(RMAzureCosmosDBContainer container)
        {
            _container = container;
        }

        #region Querying
        public RMAzureCosmosDBLinqQuerier UseLinqQuery() => _container.UseLinqQuery();

        public RMAzureCosmosDBSqlQuerier UseSqlQuery() => _container.UseSqlQuery();
        #endregion

        #region Immediate concurrent actions
        public RMAzureCosmosDBImmediatelyConcurrentAction ConfigureImmediatelyAction()
        {
            InitImmediatelyAction();
            return _immediatelyConcurrent;
        }

        private void InitImmediatelyAction()
        {
            _immediatelyConcurrent = _container.UseConcurrentAction().ToImmediately();
        }

        private RMAzureCosmosDBImmediatelyConcurrentAction EnsureImmediately()
        {
            if (_immediatelyConcurrent == null)
                throw new InvalidOperationException("Immediate concurrent mode is not initialized. Call ConfigureImmediatelyAction() before using immediate actions.");
            return _immediatelyConcurrent;
        }

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyAddAsync(IEnumerable<Record> records)
            => EnsureImmediately().AddAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyAddAsync(Record record)
            => EnsureImmediately().AddAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyUpsertAsync(IEnumerable<Record> records)
            => EnsureImmediately().UpsertAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyUpsertAsync(Record record)
            => EnsureImmediately().UpsertAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyUpsertWithOptimisticLockAsync(IEnumerable<Record> records)
            => EnsureImmediately().UpsertWithOptimisticLockAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyUpsertWithOptimisticLockAsync(Record record)
            => EnsureImmediately().UpsertWithOptimisticLockAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyReplaceAsync(IEnumerable<Record> records)
            => EnsureImmediately().ReplaceAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyReplaceAsync(Record record)
            => EnsureImmediately().ReplaceAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyReplaceWithOptimisticLockAsync(IEnumerable<Record> records)
            => EnsureImmediately().ReplaceWithOptimisticLockAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyReplaceWithOptimisticLockAsync(Record record)
            => EnsureImmediately().ReplaceWithOptimisticLockAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyDeleteAsync(IEnumerable<Record> records)
            => EnsureImmediately().DeleteAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyDeleteAsync(Record record)
            => EnsureImmediately().DeleteAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyDeleteWithOptimisticLockAsync(IEnumerable<Record> records)
            => EnsureImmediately().DeleteWithOptimisticLockAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyDeleteWithOptimisticLockAsync(Record record)
            => EnsureImmediately().DeleteWithOptimisticLockAsync(new List<Record> { record });

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyPatchAsync(IEnumerable<(Record, List<PatchOperation>)> records)
            => EnsureImmediately().PatchAsync(records);

        public Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ImmediatelyPatchAsync(Record record, List<PatchOperation> patchOperations)
            => EnsureImmediately().PatchAsync(new List<(Record, List<PatchOperation>)> { (record, patchOperations) });
        #endregion

        #region Delay concurrent actions
        private RMAzureCosmosDBDelayConcurrentAction InitDelayConcurrent() => _container.UseConcurrentAction().ToDelay();

        private RMAzureCosmosDBDelayConcurrentAction EnsureDelay()
        {
            if (_delayConcurrent == null)
                throw new InvalidOperationException("Delay concurrent mode is not initialized. Call ConfigureDelayAction() before using delay actions.");
            return _delayConcurrent;
        }

        public async Task ConfigureDelayAction(Func<RMAzureCosmosDBDelayConcurrentActionResult, Task> callback, CancellationToken cts = default)
        {
            _delayConcurrent = InitDelayConcurrent();
            await _delayConcurrent.StartAsync(callback, cts);
        }

        public Task WaitDelayQueueCompletedAsync() => EnsureDelay().WaitCompletedAsync();

        public void CompleteAddingDelayQueue() => EnsureDelay().SetCompleteAdding();

        public void DelayAction(RMAzureCosmosDBActionType actionType, Record record) => EnsureDelay().Action(actionType, record);
        public void DelayAction(RMAzureCosmosDBActionType actionType, List<PatchOperation> patchOps, Record record) => EnsureDelay().Action(actionType, record, patchOps);
        public void DelayAction(RMAzureCosmosDBActionType actionType, IEnumerable<Record> records) => EnsureDelay().Action(actionType, records);
        public void DelayAction(RMAzureCosmosDBActionType actionType, IEnumerable<(Record, List<PatchOperation>)> records) => EnsureDelay().Action(actionType, records);

        public void DelayAdd(Record record) => EnsureDelay().Add(record);
        public void DelayAdd(IEnumerable<Record> records) => EnsureDelay().Add(records);

        public void DelayUpsert(Record record) => EnsureDelay().Upsert(record);
        public void DelayUpsert(IEnumerable<Record> records) => EnsureDelay().Upsert(records);
        public void DelayUpsertWithOptimisticLock(Record record) => EnsureDelay().UpsertWithOptimisticLock(record);
        public void DelayUpsertRangeWithOptimisticLock(IEnumerable<Record> records) => EnsureDelay().UpsertWithOptimisticLock(records);

        public void DelayReplace(Record record) => EnsureDelay().Replace(record);
        public void DelayReplaceRange(IEnumerable<Record> records) => EnsureDelay().Replace(records);
        public void DelayReplaceWithOptimisticLock(Record record) => EnsureDelay().ReplaceWithOptimisticLock(record);
        public void DelayReplaceRangeWithOptimisticLock(IEnumerable<Record> records) => EnsureDelay().ReplaceWithOptimisticLock(records);
        
        public void DelayDelete(Record record) => EnsureDelay().Delete(record);
        public void DelayDeleteRange(IEnumerable<Record> records) => EnsureDelay().Delete(records);
        public void DelayDeleteWithOptimisticLock(Record record) => EnsureDelay().DeleteWithOptimisticLock(record);
        public void DelayDeleteRangeWithOptimisticLock(IEnumerable<Record> records) => EnsureDelay().DeleteWithOptimisticLock(records);

        public void DelayPatch(Record record, List<PatchOperation> patchOps) => EnsureDelay().Patch(record, patchOps);
        public void DelayPatchRange(IEnumerable<(Record, List<PatchOperation>)> records) => EnsureDelay().Patch(records);
        #endregion
    }
}

