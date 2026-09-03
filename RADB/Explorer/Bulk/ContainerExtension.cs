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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Bulk
{
    public static class ContainerExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ContainerExtension));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        /// <summary>
        /// bulk upsert records
        /// </summary>
        /// <param name="container"></param>
        /// <param name="itemsToInsert"></param>
        /// <returns></returns>
        public static async Task<BulkOperationResponse> UpsertRecordsConcurrentlyAsync(this
            Container container,
            IReadOnlyList<Record> itemsToInsert)
        {
            var result = new BulkOperationResponse 
            { 
                FailedIds = new SynchronizedCollection<Guid>(),
                Failures = new ConcurrentDictionary<Guid, Exception>()
            };

            foreach(var itemId in itemsToInsert.Select(o => o.Id))
            {
                result.FailedIds.Add(itemId);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                var AmountToInsert = itemsToInsert.Count;
                // Prepare items for insertion
                //Console.WriteLine($"Preparing {AmountToInsert} items to insert...");

                // Create the list of Tasks
                //Console.WriteLine($"Starting...");
                //Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                List<Task> tasks = new List<Task>(AmountToInsert);
                foreach (var item in itemsToInsert)
                {
                    tasks.Add(UpsertItemAsync(container, item)
                        .ContinueWith(itemResponse =>
                        {
                            ProcessOneResponse(item, itemResponse, result);
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                //Console.WriteLine($"Total RU: {totalRU}, Failed item count: {failedItemIds.Count}");
                // </ConcurrentTasks>
                //stopwatch.Stop();
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred in UpsertRecordsConcurrentlyAsync: {ex.ToString()}");
            }
            stopwatch.Stop();

            return result;
        }

        private static void ProcessOneResponse(Record item, Task<ItemResponse<Record>> itemResponse, BulkOperationResponse result)
        {
            //result.TotalRequestUnitsConsumed += itemResponse.Result.RequestCharge;
            if (itemResponse.Status != TaskStatus.RanToCompletion)
            {
                try
                {
                    AggregateException innerExceptions = itemResponse.Exception.Flatten();
                    ArgumentCheck.NotNull(item, nameof(item));
                    logger.Error($"Failed to upsert '{item?.Id}' to Cosmos DB, error: {innerExceptions.InnerExceptions.FirstOrDefault()?.ToString()}");
                    result.Failures[item.Id] = innerExceptions.InnerExceptions.FirstOrDefault();
                    if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                    {
                        result.CanRetryWhenFailure = true;
                    }
                }
                catch (Exception ex)
                {
                    result.Failures[item.Id] = ex;
                    logger.Warn($"Failed to get response item exception for '{item?.Id}', error : {ex.ToString()}");
                }
            }
            else
            {
                result.FailedIds.Remove(item.Id);
            }
        }

        /// <summary>
        /// 批量upsert数据，如果中间遇到错误，会重试maxRetryTimes次， 每次时间间隔interval的倍数
        /// 毫秒
        /// </summary>
        /// <param name="itemsToInsert"></param>
        /// <param name="maxRetryTimes"></param>
        /// <param name="interval">mili seconds</param>
        /// <returns></returns>
        public static async Task<List<(Guid, Exception)>> RetryUpsertRecordsConcurrentlyAsync(this
            Container container, List<Record> itemsToInsert, int maxRetryTimes = 3, int interval = 500)
        {
            var result = new List<(Guid, Exception)>();
            var tempRecords = itemsToInsert;
            int retryTime = 0;
            BulkOperationResponse tmp = null;
            while (retryTime++ < maxRetryTimes)
            {
                tmp = await container.UpsertRecordsConcurrentlyAsync(tempRecords);
                if (!tmp.HasFailedItems) break;
                tempRecords = itemsToInsert.Where(r => tmp.FailedIds.Contains(r.Id)).ToList();
                Thread.Sleep(interval * retryTime);
            }

            if (tmp != null)
            {
                foreach (var id in tmp.FailedIds)
                {
                    if (tmp.Failures.ContainsKey(id))
                    {
                        result.Add((id, tmp.Failures[id]));
                    }
                    else
                    {
                        result.Add((id, null));
                    }
                }
            }
            return result;
        }

        private static async Task<ItemResponse<Record>> UpsertItemAsync(Container container, Record item)
        {
            item.SetPartitionKeys();
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return await UpsertIndependentItemAsync(container, item);
            }
            
            return await UpsertNormalItemAsync(container, item);
        }

        private static async Task<ItemResponse<Record>> UpsertIndependentItemAsync(Container container, Record item)
        {
            // Add retry logic using RMRetryer for the entire upsert operation
            return await Retryer.RetryAsync(async () =>
            {
                try
                {
                    return await InnerUpsertItemAsync(container, item, item.BuildPartitionKey());
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        throw new Exception($"ETag mismatch, failed to upsert item with id: {item.Id}", e);
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        throw new Exception($"Partition is being migrated, failed to upsert item with id: {item.Id}", e);
                    }
                    else if (e.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(e.RetryAfter.Value);
                        throw new Exception($"Request rate is too high, failed to upsert item with id: {item.Id}", e);
                    }
                    throw;
                }
                catch
                {
                    throw;
                }
            });
        }

        private static async Task<ItemResponse<Record>> UpsertNormalItemAsync(Container container, Record item)
        {
            var partitionKeyInfo = await ExplorerDBCommon.GetCosmosPartitionKeyInfo(item.CreateDate, item.Id, container);
            var originalPartitionKey = partitionKeyInfo.OriginalPartitionKey;
            var itemPartitionKey = partitionKeyInfo.ItemPartitionKey;
            var partitionKeyList = partitionKeyInfo.PartitionKeyList;
            var cacheKey = partitionKeyInfo.CacheKey;
            var dbItem = partitionKeyInfo.DBItem;

            // Add retry logic using RMRetryer for the entire upsert operation
            return await Retryer.RetryAsync(async () =>
            {
                try
                {
                    item.CreateDate = itemPartitionKey;
                    return await InnerUpsertItemAsync(container, item, itemPartitionKey);
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        return await Retryer.RetryAsync(async () =>
                        {
                            var newItem = container.GetItemLinqQueryable<Record>(true).Where((record) => record.Id == item.Id).Take(1).AsEnumerable().First();
                            item.ETag = newItem?.ETag;
                            item.MergeRecords(newItem);
                            ArgumentNullException.ThrowIfNull(newItem);
                            return await container.UpsertItemAsync(newItem, new PartitionKey(newItem.CreateDate), new ItemRequestOptions
                            {
                                IfMatchEtag = newItem?.ETag
                            });
                        });
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        return await ExplorerDBCommon.ProcessRenewPartition(container, item, dbItem, cacheKey, itemPartitionKey, partitionKeyList, originalPartitionKey);
                    }
                    else if (e.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(e.RetryAfter.Value);
                        return await Retryer.RetryAsync(async () =>
                        {
                            var newItem = container.GetItemLinqQueryable<Record>(true).Where((record) => record.Id == item.Id).Take(1).AsEnumerable().FirstOrDefault();
                            item.ETag = newItem?.ETag;
                            item.MergeRecords(newItem);
                            ArgumentNullException.ThrowIfNull(newItem);
                            return await container.UpsertItemAsync(newItem, new PartitionKey(newItem.CreateDate), new ItemRequestOptions
                            {
                                IfMatchEtag = newItem?.ETag
                            });
                        });
                    }
                    throw;
                }
                catch
                {
                    throw;
                }
            });
        }

        private static async Task<ItemResponse<Record>> InnerUpsertItemAsync(Container container, Record item, int itemPartitionKey)
        {

            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                return await container.UpsertItemAsync(item, new PartitionKey(itemPartitionKey));
            }
            return await container.UpsertItemAsync(item, new PartitionKey(itemPartitionKey), new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            });
        }

        private static async Task<ItemResponse<Record>> InnerUpsertItemAsync(Container container, Record item, PartitionKey partitionKey)
        {

            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                return await container.UpsertItemAsync(item, partitionKey);
            }
            return await container.UpsertItemAsync(item, partitionKey, new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            });
        }
    }
}
