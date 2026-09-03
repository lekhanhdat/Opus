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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Bulk;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class MLManualApprovalRecordRepository
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(MLManualApprovalRecordRepository));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static CosmosClient Client => new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;

        private Container CosmosContainer { get; }

        public MLManualApprovalRecordRepository() :
            this(RMDBContextManager.GetCosmosDBConnectionAsync().Result)
        { }

        public MLManualApprovalRecordRepository(CosmosConnectionInfo cosmosConnectionInfo)
        {
            CodeContract.NullThrowing(cosmosConnectionInfo, "cosmosConnectionInfo");
            CodeContract.NullOrEmptyStringThrowing(cosmosConnectionInfo.DatabaseId, "cosmosConnectionInfo[DatabaseId]");
            CodeContract.NullOrEmptyStringThrowing(cosmosConnectionInfo.CollectionId, "cosmosConnectionInfo[CollectionId]");

            try
            {
                var database = Client.GetDatabase(cosmosConnectionInfo.DatabaseId);
                CosmosContainer = database.GetContainer(cosmosConnectionInfo.CollectionId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get cosmos db container. Error: {e}");
                throw;
            }
        }

        public async Task<List<ManualApprovalRecord>> QueryItemsAsync(Expression<Func<ManualApprovalRecord, bool>> predicate)
        {
            var result = new List<ManualApprovalRecord>();

            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true).Where(predicate);
            using (var linqFeed = queryable.ToFeedIterator())
            {
                while (linqFeed.HasMoreResults)
                {
                    var response = await linqFeed.ReadNextAsync().ConfigureAwait(false);
                    LogQueryMetrics(queryable, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                    foreach (var item in response)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        //public async Task<(IEnumerable<T> Result, string Continuation)> QueryItemsWithPaginationAsync<T>(Expression<Func<ManualApprovalRecord, bool>> predicate,
        //    Expression<Func<ManualApprovalRecord, T>> selectLambda, string continuation, int limit = 5000)
        //{

        //    var requestOptions = GetPaginateRequestOptions(limit);
        //    var result = new List<T>();
        //    string innerContinuation = null;

        //    var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions)
        //        .Where(predicate)
        //        .Select(selectLambda);

        //    using (var linqFeed = queryable.ToFeedIterator())
        //    {
        //        if (linqFeed.HasMoreResults)
        //        {
        //            var response = await linqFeed.ReadNextAsync().ConfigureAwait(false);
        //            innerContinuation = response.ContinuationToken;

        //            foreach (var item in response)
        //            {
        //                result.Add(item);
        //            }
        //        }
        //    }

        //    return (result, innerContinuation);
        //}

        public async Task<PaginateQueryManualApprovalExplorerResult> QueryItemsWithPaginationAsync(ManualApprovalExplorerQueryDefinition queryDefinition)
        {
            var result = new PaginateQueryManualApprovalExplorerResult
            {
                Items = new List<ManualApprovalRecord>(),
            };
            var needQueryItemsCount = queryDefinition.PageSize;
            string continuationToken = queryDefinition.Continuation;

            do
            {
                var queryResult = await QueryItemsWithPaginationAsync(queryDefinition.Predicates, queryDefinition.OrderDefinitions, needQueryItemsCount, continuationToken).ConfigureAwait(false);
                result.Items.AddRange(queryResult.Items);
                result.Continuation = continuationToken = queryResult.Continuation;
                needQueryItemsCount -= queryResult.Items.Count;

            } while (needQueryItemsCount > 0 && !string.IsNullOrEmpty(continuationToken));

            return result;
        }

        public async Task<PaginateQueryManualApprovalExplorerResult> QueryItemsWithPaginationAsync(
            List<Expression<Func<ManualApprovalRecord, bool>>> predicates,
            List<ManualApprovalExplorerOrderDefinition> orderDefinitions,
            int pageSize,
            string continuation
            )
        {
            continuation = string.IsNullOrEmpty(continuation) ? null : continuation;
            var requestOptions = GetPaginateRequestOptions(pageSize);
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions).AsQueryable();
            queryable = BuildQueryablePredicates(queryable, predicates);
            queryable = BuildQueryableOrders(queryable, orderDefinitions);
            Logger.Debug($"[QueryItemsWithPaginationAsync]Query Text: {queryable.ToString()}");
            var paginateItems = new List<ManualApprovalRecord>();
            string continuationToken = null;

            using (var linqFeed = queryable.ToFeedIterator())
            {
                if (linqFeed.HasMoreResults)
                {
                    var response = await linqFeed.ReadNextAsync().ConfigureAwait(false);
                    continuationToken = response.ContinuationToken;
                    LogQueryMetrics(queryable, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                    foreach (var item in response)
                    {
                        paginateItems.Add(item);
                    }
                }
            }

            return new PaginateQueryManualApprovalExplorerResult
            {
                Items = paginateItems,
                Continuation = continuationToken
            };
        }

        public async Task<int> CountAsync(ManualApprovalExplorerQueryDefinition queryDefinition)
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true).AsQueryable();
            queryable = BuildQueryablePredicates(queryable, queryDefinition);
            return await queryable.CountAsync().ConfigureAwait(false);
        }

        public async Task UpsertItemAsync(ManualApprovalRecord item)
        {
            item.SetPartitionKeys();
            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                await CosmosContainer.UpsertItemAsync(item, item.BuildPartitionKey());
            }

            try
            {
                await CosmosContainer.UpsertItemAsync(item, item.BuildPartitionKey(), new ItemRequestOptions { IfMatchEtag = item.ETag });
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    await Retryer.RetryAsync(async () =>
                    {
                        var newItem = CosmosContainer.GetItemLinqQueryable<Record>(true).Where((record) => record.Id == item.Id).Take(1).AsEnumerable().First();
                        item.ETag = newItem?.ETag;
                        item.MergeRecords(newItem);
                        ArgumentNullException.ThrowIfNull(newItem);
                        return await CosmosContainer.UpsertItemAsync(newItem, item.BuildPartitionKey(), new ItemRequestOptions
                        {
                            IfMatchEtag = newItem?.ETag
                        });
                    });

                    return;
                }

                throw;
            }
            catch
            {
                throw;
            }
        }

        public async Task UpsertItems(List<ManualApprovalRecord> items)
        {
            foreach (var item in items)
            {
                await UpsertItemAsync(item).ConfigureAwait(false);
            }
        }

        private static IQueryable<ManualApprovalRecord> BuildQueryablePredicates(IQueryable<ManualApprovalRecord> queryable, ManualApprovalExplorerQueryDefinition queryDefinition)
        {
            var isManual = (int)RMMLUnderReview.IsManual;
            queryable = queryable.Where(item => item.MLUnderReview == isManual && (item.RecordStatus == (int)RMRecordStatus.Active || item.RecordStatus == (int)RMRecordStatus.TrainingManualSync || item.RecordStatus == (int)RMRecordStatus.ManualPreSync));
            foreach (var predicate in queryDefinition.Predicates)
            {
                queryable = queryable.Where(predicate);
            }
            return queryable;
        }

        private static IQueryable<ManualApprovalRecord> BuildQueryablePredicates(IQueryable<ManualApprovalRecord> queryable, List<Expression<Func<ManualApprovalRecord, bool>>> predicates)
        {
            var isManual = (int)RMMLUnderReview.IsManual;
            queryable = queryable.Where(item => item.MLUnderReview == isManual && (item.RecordStatus == (int)RMRecordStatus.Active || item.RecordStatus == (int)RMRecordStatus.TrainingManualSync || item.RecordStatus == (int)RMRecordStatus.ManualPreSync));
            foreach (var predicate in predicates)
            {
                queryable = queryable.Where(predicate);
            }
            return queryable;
        }

        private static IQueryable<ManualApprovalRecord> BuildQueryableOrders(IQueryable<ManualApprovalRecord> queryable, List<ManualApprovalExplorerOrderDefinition> orderDefinitions)
        {
            if (orderDefinitions.Count == 0)
            {
                return queryable;
            }

            IOrderedQueryable<ManualApprovalRecord> orderedQueryable = null;
            for (var i = 0; i < orderDefinitions.Count; i++)
            {
                var orderDefinition = orderDefinitions[i];
                if (i == 0)
                {
                    if (!orderDefinition.IsDesc)
                    {
                        orderedQueryable = queryable.OrderBy(orderDefinition.OrderKeySelector);
                    }
                    else
                    {
                        orderedQueryable = queryable.OrderByDescending(orderDefinition.OrderKeySelector);
                    }

                    continue;
                }

                if (!orderDefinition.IsDesc)
                {
                    orderedQueryable = orderedQueryable.ThenBy(orderDefinition.OrderKeySelector);
                }
                else
                {
                    orderedQueryable = orderedQueryable.ThenByDescending(orderDefinition.OrderKeySelector);
                }
            }

            return orderedQueryable;
        }

        private static QueryRequestOptions GetPaginateRequestOptions(int pageSize)
        {
            return new QueryRequestOptions()
            {
                MaxConcurrency = 0,
                MaxBufferedItemCount = -1,
                MaxItemCount = pageSize,
            };
        }

        private static void LogQueryMetrics<T>(IQueryable<T> queryable, double ru, string indexMetrics, TimeSpan timeCost)
        {
            var enableLog = false;
            if (!bool.TryParse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.LOG_COSMOS_QUERY_METRICS], out enableLog)) return;
            if (!enableLog) return;
            Expression expression = queryable.Expression;
            Logger.Warn($"Cosmos diagnose log. Sql:{expression.ToString()} Time Cost:{timeCost.Milliseconds} Request Units:{ru.ToString()} Index Metrics:{indexMetrics}");
        }
    }
}
