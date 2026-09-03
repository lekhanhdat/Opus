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
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.AzureCosmosDB.WriteMode;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Concurrent
{
    public class RMAzureCosmosDBImmediatelyConcurrentAction
    {
        private readonly RMAzureCosmosDBContainer Container;

        private readonly int MaxDegreeOfParallelism;

        private readonly RMAzureCosmosDBRetryer Retryer;

        internal RMAzureCosmosDBImmediatelyConcurrentAction(RMAzureCosmosDBContainer container, int retryTimes, int maxDegreeOfParallelism, int initalRetryDelayTime)
        {
            Container = container;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            Retryer = new RMAzureCosmosDBRetryer(retryTimes, initalRetryDelayTime);
        }

        private async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ConcurrentActionAsync(IEnumerable<Record> records, Func<Record, Task> action)
        {
            var result = new ConcurrentBag<RMAzureCosmosDBImmediatelyConcurrentActionResult>();

            await Parallel.ForEachAsync(records, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async (record, cancellationToken) =>
            {
                var retriedResult = await Retryer.RetryAsync(async () =>
                {
                    await action(record);
                });

                result.Add(new RMAzureCosmosDBImmediatelyConcurrentActionResult
                {
                    Item = record,
                    IsSucceed = retriedResult.IsSucceed,
                    IsOptimisticLockConflict = retriedResult.IsOptimisticLockConflict,
                    CanContinueRetry = retriedResult.CanContinueRetry,
                    Exception = retriedResult.Exception
                });
            });

            return result;
        }

        private async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ConcurrentActionAsync(IEnumerable<(Record record, List<PatchOperation> ops)> records, Func<Record, List<PatchOperation>, Task> action)
        {
            var result = new ConcurrentBag<RMAzureCosmosDBImmediatelyConcurrentActionResult>();

            await Parallel.ForEachAsync(records, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, async (item, cancellationToken) =>
            {
                var (record, ops) = item;

                var retriedResult = await Retryer.RetryAsync(async () =>
                {
                    await action(record, ops);
                });

                result.Add(new RMAzureCosmosDBImmediatelyConcurrentActionResult
                {
                    Item = record,
                    IsSucceed = retriedResult.IsSucceed,
                    IsOptimisticLockConflict = retriedResult.IsOptimisticLockConflict,
                    CanContinueRetry = retriedResult.CanContinueRetry,
                    Exception = retriedResult.Exception
                });
            });

            return result;
        }

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> AddAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.AddAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> UpsertAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.UpsertAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> UpsertWithOptimisticLockAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.UpsertWithOptimisticLockAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ReplaceAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.ReplaceAsync);
   
        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> ReplaceWithOptimisticLockAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.ReplaceWithOptimisticLockAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> DeleteAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.DeleteAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> DeleteWithOptimisticLockAsync(IEnumerable<Record> records)
            => await ConcurrentActionAsync(records, Container.DeleteWithOptimisticLockAsync);

        internal async Task<IEnumerable<RMAzureCosmosDBImmediatelyConcurrentActionResult>> PatchAsync(IEnumerable<(Record, List<PatchOperation>)> records)
            => await ConcurrentActionAsync(records, Container.PatchAsync);
    }
}
