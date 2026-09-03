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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Bulk;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class ManualApprovalRecordRepository
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalRecordRepository));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static CosmosClient Client => new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;

        private Container CosmosContainer { get; }

        public ManualApprovalRecordRepository() :
            this(RMDBContextManager.GetCosmosDBConnectionAsync().Result)
        { }

        public ManualApprovalRecordRepository(CosmosConnectionInfo cosmosConnectionInfo)
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

        public async Task<(IEnumerable<T> Result, string Continuation)> QueryItemsWithPaginationAsync<T>(Expression<Func<ManualApprovalRecord, bool>> predicate,
            Expression<Func<ManualApprovalRecord, T>> selectLambda, string continuation, int limit = 5000)
        {

            var requestOptions = GetPaginateRequestOptions(limit);
            var result = new List<T>();
            string innerContinuation = null;

            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions)
                .Where(predicate)
                .Select(selectLambda);

            using (var linqFeed = queryable.ToFeedIterator())
            {
                if (linqFeed.HasMoreResults)
                {
                    var response = await linqFeed.ReadNextAsync().ConfigureAwait(false);
                    innerContinuation = response.ContinuationToken;
                    LogQueryMetrics(queryable, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                    foreach (var item in response)
                    {
                        result.Add(item);
                    }
                }
            }

            return (result, innerContinuation);
        }

        public async Task<PaginateQueryManualApprovalExplorerResult> QueryFolderViewItemsWithPaginationAsync(ManualApprovalExplorerQueryDefinition queryDefinition, Expression<Func<ManualApprovalRecord, bool>> folderPredicate, List<Expression<Func<ManualApprovalRecord, bool>>> folderViewFilters)
        {
            var result = new PaginateQueryManualApprovalExplorerResult
            {
                Items = new List<ManualApprovalRecord>(),
            };
            var needQueryItemsCount = queryDefinition.PageSize;
            string continuationToken = queryDefinition.Continuation;

            do
            {
                var queryResult = await QueryFolderViewItemsWithPaginationAsync(queryDefinition.Predicates, queryDefinition.OrderDefinitions, folderPredicate, folderViewFilters, needQueryItemsCount, continuationToken).ConfigureAwait(false);
                result.Items.AddRange(queryResult.Items);
                result.Continuation = continuationToken = queryResult.Continuation;
                needQueryItemsCount -= queryResult.Items.Count;

            } while (needQueryItemsCount > 0 && !string.IsNullOrEmpty(continuationToken));

            return result;
        }

        public async Task<PaginateQueryManualApprovalExplorerResult> QueryFolderViewItemsWithPaginationAsync(
            List<Expression<Func<ManualApprovalRecord, bool>>> predicates,
            List<ManualApprovalExplorerOrderDefinition> orderDefinitions,
            Expression<Func<ManualApprovalRecord, bool>> folderPredicate,
            List<Expression<Func<ManualApprovalRecord, bool>>> folderViewFilters,
            int pageSize,
            string continuation
            )
        {
            var requestOptions = GetPaginateRequestOptions(pageSize);
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions).AsQueryable();
            var result = CombineWithAnd(predicates);
            if (folderPredicate != null)
            {
                result = CombineWithOr([result, folderPredicate]);
            }
            if (folderViewFilters.Count != 0)
            {
                result = CombineWithAnd([result, .. folderViewFilters]);
            }
            queryable = queryable.Where(result);
            queryable = BuildQueryableOrders(queryable, orderDefinitions);
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
            var requestOptions = GetPaginateRequestOptions(pageSize);
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions).AsQueryable();
            queryable = BuildQueryablePredicates(queryable, predicates);
            queryable = BuildQueryableOrders(queryable, orderDefinitions);

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

        public async Task<PaginateQueryFolderPathResult> QueryItemsWithPaginationAsyncForFolderPath(
            Expression<Func<ManualApprovalRecord, bool>> predicate,
            Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate,
        int pageSize, string continuation, Expression<Func<ManualApprovalRecord, bool>> isSelectFolderPath)
        {
            try
            {
                var requestOptions = GetPaginateRequestOptions(pageSize);

                var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true, continuation, requestOptions)
                                                .Where(predicate)
                                                .Where(notAdminpredicate)
                                                .Where(isSelectFolderPath)
                                                .Select(item => item.ManualFolderPath)
                                                .OrderBy(item => item)     //sort
                                                .Distinct();


                var folderPathResiltItems = new HashSet<string>();

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
                            folderPathResiltItems.Add(item);
                        }
                    }
                }

                return new PaginateQueryFolderPathResult
                {
                    Items = folderPathResiltItems,
                    Continuation = continuationToken
                };

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public async Task<int> CountAsyncForRecordItemDistinct(Expression<Func<ManualApprovalRecord, bool>> predicate, Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate, Expression<Func<ManualApprovalRecord, string>> selectProperty)
        {

            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true)
                        .Where(predicate)
                        .Where(notAdminpredicate)
                        .Select(selectProperty)
                        .Distinct();

            return await queryable.CountAsync().ConfigureAwait(false);
        }

        public async Task<(bool isOnlyOneLocation, string manualSiteUrl)> FindisOneLocation(Expression<Func<ManualApprovalRecord, bool>> predicate, Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate, Expression<Func<ManualApprovalRecord, string>> selectProperty)
        {

            var location = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true)
                        .Where(predicate)
                        .Where(notAdminpredicate)
                        .Select(selectProperty)
                        .Take(1).AsEnumerable().FirstOrDefault();


            if (location == null)
            {
                return (true, string.Empty);
            }

            var otherLocation = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true)
                        .Where(predicate)
                        .Where(notAdminpredicate)
                        .Where(item => item.ManualSiteUrl != location);

            var isOnlyOneLocation = (await otherLocation.CountAsync().ConfigureAwait(false)) == 0;
            return (isOnlyOneLocation, isOnlyOneLocation ? location : string.Empty);
        }


        public async Task<int> CountAsync(ManualApprovalExplorerQueryDefinition queryDefinition)
        {
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true).AsQueryable();
            queryable = BuildQueryablePredicates(queryable, queryDefinition);
            return await queryable.CountAsync().ConfigureAwait(false);
        }

        public async Task<int> CountFolderViewAsync(ManualApprovalExplorerQueryDefinition queryDefinition, Expression<Func<ManualApprovalRecord, bool>> predicate, List<Expression<Func<ManualApprovalRecord, bool>>> folderViewFilters)
        { 
            var queryable = CosmosContainer.GetItemLinqQueryable<ManualApprovalRecord>(true).AsQueryable();
            var archiveStatus = (int)ActionStatus.Archiverd;
            Expression<Func<ManualApprovalRecord, bool>> statusExpression = root => root.IsManualSynced && root.ManualArchiveStatus != archiveStatus && root.RecordStatus != (int)RMRecordStatus.Hidden && root.RecordStatus != (int)RMRecordStatus.RMDeleted;
            queryDefinition.Predicates.Add(statusExpression);
            var result = CombineWithAnd(queryDefinition.Predicates);
            if (predicate != null)
            {
                result = CombineWithOr([result, predicate]);
            }
            if (folderViewFilters.Count != 0)
            {
                result = CombineWithAnd([result, ..folderViewFilters]);
            }
            queryable = queryable.Where(result);
            return await queryable.CountAsync().ConfigureAwait(false);
        }

        public async Task<bool> UpsertItemAsync(ManualApprovalRecord item)
        {
            item.SetPartitionKeys();
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return await UpsertIndependentItemAsync(item);
            }

            return await UpsertNormalItemAsync(item);
        }

        private async Task<bool> UpsertIndependentItemAsync(ManualApprovalRecord item)
        {
            try
            {
                try
                {
                    await InnerUpsertItemAsync(item, item.BuildPartitionKey());
                    return true;
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        throw new Exception($"The item with id {item.Id} has been modified by another process. Please refresh and try again.", e);
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        throw new Exception($"The partition key for item with id {item.Id} is not found. This may be caused by the partition being expired and deleted. Please check if the partition key value is correct and if the partition is still valid.", e);
                    }

                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> UpsertNormalItemAsync(ManualApprovalRecord item)
        {
            try
            {
                var result = await ExplorerDBCommon.GetCosmosPartitionKeyInfo(item.CreateDate, item.Id, CosmosContainer);
                var originalPartitionKey = result.OriginalPartitionKey;
                var itemPartitionKey = result.ItemPartitionKey;
                var partitionKeyList = result.PartitionKeyList;
                var cacheKey = result.CacheKey;
                var dbItem = result.DBItem;

                try
                {
                    item.CreateDate = result.ItemPartitionKey;
                    await InnerUpsertItemAsync(item, new PartitionKey(item.CreateDate));
                    return true;
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        await Retryer.RetryAsync(async () =>
                        {
                            var newItem = (await CosmosContainer.ReadItemAsync<Record>(item.Id.ToString(), new PartitionKey(itemPartitionKey))).Resource;
                            item.ETag = newItem?.ETag;
                            item.MergeRecords(newItem);
                            return await CosmosContainer.UpsertItemAsync(newItem, new PartitionKey(itemPartitionKey), new ItemRequestOptions
                            {
                                IfMatchEtag = newItem?.ETag
                            });
                        });

                        return true;
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        await ExplorerDBCommon.ProcessRenewPartition(CosmosContainer, item, dbItem, cacheKey, itemPartitionKey, partitionKeyList, originalPartitionKey);
                        return true;
                    }

                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task InnerUpsertItemAsync(ManualApprovalRecord item, PartitionKey partitionKey)
        {
            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                await CosmosContainer.UpsertItemAsync(item, partitionKey);
            }
            await CosmosContainer.UpsertItemAsync(item, partitionKey, new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            });
        }

        public async Task UpsertItemsAsync(List<ManualApprovalRecord> items)
        {
            foreach (var item in items)
            {
                await UpsertItemAsync(item).ConfigureAwait(false);
            }
        }

        private static IQueryable<ManualApprovalRecord> BuildQueryablePredicates(IQueryable<ManualApprovalRecord> queryable, ManualApprovalExplorerQueryDefinition queryDefinition)
        {
            var archiveStatus = (int)ActionStatus.Archiverd;
            queryable = queryable.Where(item => item.IsManualSynced && item.ManualArchiveStatus != archiveStatus && item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted);
            foreach (var predicate in queryDefinition.Predicates)
            {
                queryable = queryable.Where(predicate);
            }
            return queryable;
        }

        private static IQueryable<ManualApprovalRecord> BuildQueryablePredicates(IQueryable<ManualApprovalRecord> queryable, List<Expression<Func<ManualApprovalRecord, bool>>> predicates)
        {
            var archiveStatus = (int)ActionStatus.Archiverd;
            queryable = queryable.Where(item => item.IsManualSynced && item.ManualArchiveStatus != archiveStatus && item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted);
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

        private static Expression<Func<T, bool>> CombineWithAnd<T>(List<Expression<Func<T, bool>>> expressions)
        {
            if (expressions == null || !expressions.Any())
                throw new ArgumentException("The expressions list cannot be null or empty.");

            var result = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

            foreach (var expression in expressions)
            {
                result.Add(expression.Body);
            }

            var body = result.AsEnumerable().Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
        private static Expression<Func<T, bool>> CombineWithOr<T>(List<Expression<Func<T, bool>>> expressions)
        {
            if (expressions == null || !expressions.Any())
                throw new ArgumentException("The expressions list cannot be null or empty.");

            var result = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

            foreach (var expression in expressions)
            {
                result.Add(expression.Body);
            }
            var body = result.AsEnumerable().Aggregate(Expression.OrElse);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}
