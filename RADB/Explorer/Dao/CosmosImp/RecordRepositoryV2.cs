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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Bulk;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model.Extension;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Concurrent;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    /// <summary>
    /// NOTE: The repository sharing unique instance of cosmos client underlying for improving performance.
    ///  https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips-dotnet-sdk-v3-sql
    /// </summary>
    public class RecordRepositoryV2 : IDisposable
    {
        private CommonUtil.RALogger logger = CommonUtil.RALogger.GetInstance(typeof(RecordRepositoryV2));

        private readonly static object locker = new object();

        private CosmosConnectionInfo CosmosConnectionInfo { get; set; }

        //singleton to boost performance
        //Note: each DC/application server maps to one cosmos db endpoint only. So there is no need to prepare a dictionary for mapping endpoint-client.
        // https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips-dotnet-sdk-v3-sql
        private static CosmosClient Client { get; set; }

        private Database Database { get; set; }
        private Container Container { get; set; }

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private QueryRequestOptions DefaultOptions { get; } = new QueryRequestOptions()
        {
            //determined by sdk automatically
            MaxConcurrency = 0,
            //determined by sdk automatically
            MaxBufferedItemCount = -1,
            //determined by sdk automatically
            MaxItemCount = -1,

            PopulateIndexMetrics = true,
        };

        public OperationLevel Level { get; private set; }

        public const int DefaultCountPerPage = 15;

        public RecordRepositoryV2(CosmosConnectionInfo connectionInfo)
        {
            CodeContract.NullThrowing(connectionInfo, "CosmosConnectionInfo");

            CosmosConnectionInfo = connectionInfo;

            InitConnection();

        }
        /// <summary>
        /// database level操作使用
        /// </summary>
        /// <param name="specifiedDatabaseId"></param>
        public RecordRepositoryV2(string specifiedDatabaseId = "")
        {
            var connection = new CosmosConnectionInfo()
            {
                DatabaseId = specifiedDatabaseId,
            };
            CosmosConnectionInfo = connection;

            InitConnection();
        }

        private void InitConnection()
        {
            lock(locker)
            {
                    //note: the endpoint is unique for each dc/application server. 
                Client = new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;
            }

            Debug.Assert(Client != null);

            this.Level = OperationLevel.OnDatabase;
            //notice: there is no _client.OpenAsync() like method to test the connection
            
            if(!string.IsNullOrEmpty(CosmosConnectionInfo.DatabaseId))
            {
                Database = Client.GetDatabase(CosmosConnectionInfo.DatabaseId);
                this.Level = OperationLevel.OnContainer;
            }

            if(Database != null && !string.IsNullOrEmpty(CosmosConnectionInfo.CollectionId))
            {
                Container = Database.GetContainer(CosmosConnectionInfo.CollectionId);
                this.Level = OperationLevel.OnItem;
            }
        }

        private void EnsureOperationLevel(OperationLevel level)
        {
            if(this.Level < level)
            {
                throw new InvalidOperationException($"Invalided operation, the operation is not supported under current operation level: {this.Level}, expected level: {level}. Please ensure use the correct constructor to init the repository");
            }
        }

        private QueryRequestOptions GetPagingRequestOptions(int itemPerPage)
        {
            return new QueryRequestOptions()
            {
                //determined by sdk automatically
                MaxConcurrency = 0,
                //determined by sdk automatically
                MaxBufferedItemCount = -1,

                MaxItemCount = itemPerPage,

                PopulateIndexMetrics = true,
            };
        }

        #region operations
        public async Task<List<string>> GetAllDatabaseIds()
        {
            return await Retryer.RetryAsync(async () =>
            {
                List<string> result = new List<string>();
                using (FeedIterator<DatabaseProperties> iterator = Client.GetDatabaseQueryIterator<DatabaseProperties>())
                {
                    while (iterator.HasMoreResults)
                    {
                        foreach (DatabaseProperties db in await iterator.ReadNextAsync().ConfigureAwait(false))
                        {
                            result.Add(db.Id);
                        }
                    }
                }
                return result;
            });
        }

        public async Task<bool> DatabaseExist(string databaseId)
        {
            return await Retryer.RetryAsync(async () =>
            {
                List<string> result = new List<string>();
                using (FeedIterator<DatabaseProperties> iterator = Client.GetDatabaseQueryIterator<DatabaseProperties>())
                {
                    while (iterator.HasMoreResults)
                    {
                        foreach (DatabaseProperties db in await iterator.ReadNextAsync().ConfigureAwait(false))
                        {
                            result.Add(db.Id);
                        }
                    }
                }
                return result.Any(r => r.Equals(databaseId));
            });
        }

        public async Task<int> QueryContainerCountAsync(string databaseId)
        {
            return await Retryer.RetryAsync(async () =>
            {
                var databaseIterator = Client.GetDatabaseQueryIterator<DatabaseProperties>();
                int result = 0;

                while (databaseIterator.HasMoreResults)
                {
                    var databaseProperties = await databaseIterator.ReadNextAsync().ConfigureAwait(false);

                    var database = databaseProperties.Where(db => db.Id.Equals(databaseId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    var containerIterator = Client.GetDatabase(database?.Id).GetContainerQueryIterator<ContainerProperties>();

                    while (containerIterator.HasMoreResults)
                    {
                        var containerProperties = await containerIterator.ReadNextAsync();
                        result = containerProperties.Count;
                        if (result != 0)
                        {
                            break;
                        }
                    }
                }

                return result;
            });
        }

        public async Task<(bool created, Record itemCreated)> AddAsync(Record record, bool returnItemCreated = false)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(record, "record");
                record.AppendCustomColumns();
                record.SetPartitionKeys();
                var response = await Container.CreateItemAsync<Record>(record).ConfigureAwait(false);

                return (response.StatusCode == HttpStatusCode.Created, response.Resource);
            });
        }

        public async Task<bool> DeleteAsync(string ID, PartitionKey PK)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(PK, "PK");

                var response = await Container.DeleteItemAsync<Record>(ID, PK).ConfigureAwait(false);

                return response.StatusCode == HttpStatusCode.NoContent;
            });
        }

        public async Task DeleteRangeAsync(IEnumerable<Record> records)
        {
            var recordList = records.ToList();
            if (!recordList.Any()) return;

            var groups = recordList.GroupBy(r => r.BuildPartitionKey());

            var exceptions = new ConcurrentBag<Exception>();

            await Parallel.ForEachAsync(
                groups,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (group, cancellationToken) =>
                {
                    try
                    {
                        var partitionKey = group.Key;
                        var batch = Container.CreateTransactionalBatch(partitionKey);

                        foreach (var record in group)
                        {
                            batch.DeleteItem(record.Id.ToString());
                        }
                        await batch.ExecuteAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });

            if (!exceptions.IsEmpty)
                throw new AggregateException(exceptions);
        }

        public async Task<Record> ReplaceItemAsync(Record item)
        {
            item.SetPartitionKeys();
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return await ReplaceIndependentItemAsync(item);
            }

            return await ReplaceNormalItemAsync(item);
        }

        private async Task<Record> ReplaceIndependentItemAsync(Record item)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullOrEmptyStringThrowing(item.Id.ToString(), "record.Id");

                try
                {
                    var response = await Container.ReplaceItemAsync(item, item.Id.ToString(), item.BuildPartitionKey(), new ItemRequestOptions
                    {
                        IfMatchEtag = item.ETag
                    });

                    return response.Resource;
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        throw new Exception($"Replace item failed with PreconditionFailed, which means the item has been modified by other process. Please retry the operation with the latest item. record id: {item.Id}, partition key: {item.BuildPartitionKey()}", e);
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        throw new InvalidOperationException($"Replace item failed with Forbidden and substatus code 1014, which means the partition is being migrated. Please retry the operation later. record id: {item.Id}, partition key: {item.BuildPartitionKey()}", e);
                    }

                    throw;
                }
            });
        }

        private async Task<Record> ReplaceNormalItemAsync(Record item)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullOrEmptyStringThrowing(item.Id.ToString(), "record.Id");

                var partitionKeyInfo = await ExplorerDBCommon.GetCosmosPartitionKeyInfo(item.CreateDate, item.Id, Container);
                var originalPartitionKey = partitionKeyInfo.OriginalPartitionKey;
                var itemPartitionKey = partitionKeyInfo.ItemPartitionKey;
                var partitionKeyList = partitionKeyInfo.PartitionKeyList;
                var cacheKey = partitionKeyInfo.CacheKey;
                var dbItem = partitionKeyInfo.DBItem;

                try
                {
                    item.CreateDate = itemPartitionKey;
                    var response = await Container.ReplaceItemAsync(item, item.Id.ToString(), new PartitionKey(item.CreateDate), new ItemRequestOptions
                    {
                        IfMatchEtag = item.ETag
                    });

                    return response.Resource;
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        var response = await Retryer.RetryAsync(async () =>
                        {
                            var newItem = Container.GetItemLinqQueryable<Record>(true).Where((record) => record.Id == item.Id).Take(1).AsEnumerable().First();
                            item.ETag = newItem?.ETag;
                            item.MergeRecords(newItem);
                            ArgumentNullException.ThrowIfNull(newItem);
                            return await Container.ReplaceItemAsync(newItem, item.Id.ToString(), new PartitionKey(newItem.CreateDate), new ItemRequestOptions
                            {
                                IfMatchEtag = newItem?.ETag
                            });
                        });
                        return response.Resource;
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        var result = await ExplorerDBCommon.ProcessRenewPartition(Container, item, dbItem, cacheKey, itemPartitionKey, partitionKeyList, originalPartitionKey);
                        return result.Resource;
                    }

                    throw;
                }
            });
        }

        public async Task<(bool isNewCreated, Record record)> UpsertItemAsync(Record item)
        {
            item.SetPartitionKeys();
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return await UpsertIndependentItemAsync(item);
            }

            return await UpsertNormalItemAsync(item);
        }

        private async Task<(bool isNewCreated, Record record)> UpsertIndependentItemAsync(Record item)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullOrEmptyStringThrowing(item.Id.ToString(), "record.Id");

                try
                {
                    var result = await InnerUpsertItemAsync(Container, item, item.BuildPartitionKey());
                    return (result.StatusCode == HttpStatusCode.Created, result.Resource);
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        throw new Exception($"Upsert item failed with PreconditionFailed, which means the item has been modified by other process. Please retry the operation with the latest item. record id: {item.Id}, partition key: {item.BuildPartitionKey()}", e);
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        throw new Exception($"Upsert item failed with Forbidden and substatus code 1014, which means the partition is being migrated. Please retry the operation later. record id: {item.Id}, partition key: {item.BuildPartitionKey()}", e);
                    }

                    throw;
                }
            });
        }

        private async Task<(bool isNewCreated, Record record)> UpsertNormalItemAsync(Record item)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullOrEmptyStringThrowing(item.Id.ToString(), "record.Id");

                var partitionKeyInfo = await ExplorerDBCommon.GetCosmosPartitionKeyInfo(item.CreateDate, item.Id, Container);
                var originalPartitionKey = partitionKeyInfo.OriginalPartitionKey;
                var itemPartitionKey = partitionKeyInfo.ItemPartitionKey;
                var partitionKeyList = partitionKeyInfo.PartitionKeyList;
                var cacheKey = partitionKeyInfo.CacheKey;
                var dbItem = partitionKeyInfo.DBItem;

                try
                {
                    item.CreateDate = itemPartitionKey;
                    var result = await InnerUpsertItemAsync(Container, item, new PartitionKey(item.CreateDate));
                    return (result.StatusCode == HttpStatusCode.Created, result.Resource);
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        var result = await Retryer.RetryAsync(async () =>
                        {
                            var newItem = Container.GetItemLinqQueryable<Record>(true).Where((record) => record.Id == item.Id).Take(1).AsEnumerable().First();
                            item.ETag = newItem?.ETag;
                            item.MergeRecords(newItem);
                            ArgumentNullException.ThrowIfNull(newItem);
                            return await Container.UpsertItemAsync(newItem, new PartitionKey(newItem.CreateDate), new ItemRequestOptions
                            {
                                IfMatchEtag = newItem?.ETag
                            });
                        });

                        return (result.StatusCode == HttpStatusCode.Created, result.Resource);
                    }
                    else if (e.StatusCode == HttpStatusCode.Forbidden && e.SubStatusCode == 1014)
                    {
                        var result = await ExplorerDBCommon.ProcessRenewPartition(Container, item, dbItem, cacheKey, itemPartitionKey, partitionKeyList, originalPartitionKey);
                        return (result.StatusCode == HttpStatusCode.Created, result.Resource);
                    }

                    throw;
                }
            });
        }

        private static async Task<ItemResponse<Record>> InnerUpsertItemAsync(Container container, Record item, PartitionKey itemPartitionKey)
        {
            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                return await container.UpsertItemAsync(item, itemPartitionKey);
            }
            return await container.UpsertItemAsync(item, itemPartitionKey, new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            });
        }
        /// <summary>
        /// 批量upsert数据，如果中间遇到错误，会重试maxRetryTimes次， 每次时间间隔interval的倍数毫秒
        /// </summary>
        /// <param name="records"></param>
        /// <param name="maxRetryTimes"></param>
        /// <param name="interval">mili seconds</param>
        /// <returns></returns>
        public async Task<List<(Guid, Exception)>> RetryUpsertRecordsConcurrentlyAsync(List<Record> records, int maxRetryTimes = 3, int interval = 500)
        {
            return await Container.RetryUpsertRecordsConcurrentlyAsync(records, maxRetryTimes, interval);
        }


        public async Task<int> CountAsync(Expression<Func<Record, bool>> predicate)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(predicate, nameof(predicate));

                return await Container.GetItemLinqQueryable<Record>(true)
                    .Where(predicate).CountAsync().ConfigureAwait(false);
            });
        }

        public Record FirstOrDefault(Expression<Func<Record, bool>> predicate)
        {
            EnsureOperationLevel(OperationLevel.OnItem);

            CodeContract.NullThrowing(predicate, nameof(predicate));

            return Container.GetItemLinqQueryable<Record>(true)
                .Where(predicate).Take(1).AsEnumerable().FirstOrDefault();
        }

        public Record FirstOrDefault(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda)
        {
            return Container.GetItemLinqQueryable<Record>(true).Where(predicate).OrderBy(orderLambda).Take(1).AsEnumerable().FirstOrDefault();
        }

        public Record FirstOrDefaultByOrderDesc(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda)
        {
            return Container.GetItemLinqQueryable<Record>(true).Where(predicate).OrderByDescending(orderLambda).Take(1).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Note: this method will return all items without paging, so please ensure narrowing down result set properly.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Record>> QueryAllAysnc(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false)
        {
            return await QueryAllAysnc<Record>(predicate, r => r, orderBy, descending).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> QueryAllAysnc<TResult>(Expression<Func<Record, bool>> predicate, Expression<Func<Record, TResult>> selection, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(predicate, nameof(predicate));
                CodeContract.NullThrowing(selection, nameof(selection));

                List<TResult> results = new List<TResult>();

                var tempQuery = Container.GetItemLinqQueryable<Record>(true, null, DefaultOptions)
                    .Where(predicate);

                if (orderBy != null)
                {
                    tempQuery = descending ? tempQuery.OrderByDescending(orderBy) : tempQuery.OrderBy(orderBy);
                }

                var query = tempQuery.Select(selection);

                using (var setIterator = query.ToFeedIterator())
                {
                    while (setIterator.HasMoreResults)
                    {
                        var response = await setIterator.ReadNextAsync().ConfigureAwait(false);
                        LogQueryMetrics(query.ToQueryDefinition().QueryText, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                        foreach (var r in response)
                        {
                            results.Add(r);
                        }
                    }
                }

                return results;
            });
        }

        public async Task<(IEnumerable<TResult> results, string continuationToken)> QueryByPageAsync<TResult>(Expression<Func<Record, bool>> predicate, Expression<Func<Record, TResult>> selection, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false, int countPerPage = DefaultCountPerPage, string continuation = null)
        {
            var continueToken = continuation;
            var items = new List<TResult>();
            var needQueryItemsCount = countPerPage;

            do
            {
                var queryResult = await QueryByPageAsyncHelper<TResult>(predicate, selection, orderBy, descending, needQueryItemsCount, continueToken);
                items.AddRange(queryResult.results);
                continueToken = queryResult.continuationToken;
                needQueryItemsCount -= queryResult.results.Count();

            } while (needQueryItemsCount > 0 && !string.IsNullOrEmpty(continueToken));

            return (items, continueToken);
        }

        public async Task<(IEnumerable<TResult> results, string continuationToken)> QueryByPageAsyncHelper<TResult>(
    Expression<Func<Record, bool>> predicate,
    Expression<Func<Record, TResult>> selection,
    Expression<Func<Record, dynamic>> orderBy = null,
    bool descending = false,
    int countPerPage = DefaultCountPerPage,
    string continuation = null)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(predicate, nameof(predicate));
                CodeContract.NullThrowing(selection, nameof(selection));
                continuation = EnsureContiuation(continuation);

                List<TResult> results = new List<TResult>();

                var tempQuery = Container.GetItemLinqQueryable<Record>(
                   true,
                   continuation,
                  GetPagingRequestOptions(countPerPage)
                  )
                 .Where(predicate);

                if (orderBy != null)
                {
                    tempQuery = descending ? tempQuery.OrderByDescending(orderBy) : tempQuery.OrderBy(orderBy);
                }

                var query = tempQuery.Select(selection);
                string ContinuationToken = null;
                using (var setIterator = query
                  .ToFeedIterator())
                {
                    if (setIterator.HasMoreResults)
                    {
                        var response = await setIterator.ReadNextAsync().ConfigureAwait(false);
                        LogQueryMetrics(query.ToQueryDefinition().QueryText, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                        ContinuationToken = response.ContinuationToken;
                        foreach (var r in response)
                        {
                            results.Add(r);
                        }
                    }
                }

                return (results, ContinuationToken);
            });
        }

        private static string EnsureContiuation(string continuation)
        {
            if (continuation == string.Empty)
            {
                continuation = null;
            }

            return continuation;
        }

        public async Task<(IEnumerable<Record> records, string continuationToken)> QueryByPageAsync(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false, int countPerPage = DefaultCountPerPage, string continuation = null)
        {
            return await QueryByPageAsync<Record>(predicate, r => r, orderBy, descending, countPerPage, continuation).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> QueryAllBySqlAsync<TResult>(QueryDefinition queryDefinition)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(queryDefinition, nameof(queryDefinition));

                List<TResult> results = new List<TResult>();
                logger.Debug($"Begin Query DB {Container.Database.Id}, Container {Container.Id}");
                using (var iterator = Container.GetItemQueryIterator<TResult>(
                   queryDefinition,
                   null,
                  DefaultOptions
                  ))
                {
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                        LogQueryMetrics(queryDefinition.QueryText, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                        foreach (var r in response)
                        {
                            results.Add(r);
                        }
                    }
                }

                return results;
            });
        }

        public async Task<IEnumerable<Record>> QueryAllBySqlAsync(QueryDefinition queryDefinition)
        {
            EnsureOperationLevel(OperationLevel.OnItem);

            CodeContract.NullThrowing(queryDefinition, nameof(queryDefinition));

            return await QueryAllBySqlAsync<Record>(queryDefinition).ConfigureAwait(false);
        }

        public async Task<(IEnumerable<TResult> results, string continuationToken)> QueryPageBySqlAsync<TResult>(QueryDefinition queryDefinition, int countPerPage = DefaultCountPerPage, string continuation = null)
        {
            var result = new List<TResult>();
            var continuationToken = continuation;
            var needQueryItemsCount = countPerPage;

            do
            {
                var queryResult = await Query<TResult>(queryDefinition, needQueryItemsCount, continuationToken).ConfigureAwait(false);
                result.AddRange(queryResult.result);
                continuationToken = queryResult.continuationToken;
                needQueryItemsCount -= queryResult.result.Count();
            } while (needQueryItemsCount > 0 && !string.IsNullOrEmpty(continuationToken));

            return (result, continuationToken);
        }

        public async Task<(IEnumerable<TResult> result, string continuationToken)> Query<TResult>(QueryDefinition queryDefinition, int countPerPage = DefaultCountPerPage, string continuation = null)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(queryDefinition, nameof(queryDefinition));
                continuation = EnsureContiuation(continuation);

                List<TResult> results = new List<TResult>();

                string ContinuationToken = null;
                using (var iterator = Container.GetItemQueryIterator<TResult>(
                   queryDefinition,
                   continuation,
                  GetPagingRequestOptions(countPerPage)
                  ))
                {
                    if (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                        LogQueryMetrics(queryDefinition.QueryText, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                        ContinuationToken = response.ContinuationToken;
                        foreach (var r in response)
                        {
                            results.Add(r);
                        }
                    }
                }

                return (results, ContinuationToken);
            });
        }

        public async Task<(IEnumerable<Record> results, string continuationToken)> QueryPageBySqlAsync(QueryDefinition queryDefinition, int countPerPage = DefaultCountPerPage, string continuation = null)
        {
            return await QueryPageBySqlAsync<Record>(queryDefinition, countPerPage, continuation).ConfigureAwait(false);
        }

        public async Task<List<string>> DistinctQueryAsync(Expression<Func<Record, string>> selectLambda, Expression<Func<Record, bool>> whereLambda)
        {
            return await Retryer.RetryAsync(async () =>
            {
                var queryable = Container.GetItemLinqQueryable<Record>(true)
                    .Where(whereLambda)
                    .Select(selectLambda)
                    .Distinct();

                using var iterator = queryable.ToFeedIterator();
                List<string> distinctValues = new List<string>();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    LogQueryMetrics(queryable.ToQueryDefinition().QueryText, response.RequestCharge, response.IndexMetrics, response.Diagnostics.GetClientElapsedTime());
                    distinctValues.AddRange(response);
                }
                return distinctValues;
            });
        }

        public async Task<bool> ExistAsync(Expression<Func<Record, bool>> predicate)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(predicate, nameof(predicate));

                using (var setIterator = Container.GetItemLinqQueryable<Record>(true)
                  .Where(predicate)
                  .Take(1)
                  .ToFeedIterator())
                {
                    if (setIterator.HasMoreResults)
                    {
                        var response = await setIterator.ReadNextAsync().ConfigureAwait(false);
                        return response.Count == 1;
                    }

                    return false;
                }
            });
        }

        public async Task<int> UpdateAllAsync(Expression<Func<Record, bool>> predicate, Action<Record> operation)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnItem);

                CodeContract.NullThrowing(predicate, nameof(predicate));
                CodeContract.NullThrowing(operation, nameof(operation));

                int count = 0;
                using (var setIterator = Container.GetItemLinqQueryable<Record>(true)
                    .Where(predicate)
                    .ToFeedIterator())
                {
                    while (setIterator.HasMoreResults)
                    {
                        foreach (var r in await setIterator.ReadNextAsync().ConfigureAwait(false))
                        {
                            operation(r);
                            r.AppendCustomColumns();
                            await ReplaceItemAsync(r);
                            count++;
                        }
                    }
                }

                return count;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>true: db is created, false: db not created, might exist already</returns>
        public async Task<(bool success, string accountEndpoint)> CreateDatabaseIfNotExistsAsync(string dbName)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnDatabase);

                CodeContract.NullOrEmptyStringThrowing(dbName, "dbName");

                var response = await Client.CreateDatabaseIfNotExistsAsync(dbName, ThroughputProperties.CreateManualThroughput(400)).ConfigureAwait(false);

                return (response.StatusCode == HttpStatusCode.Created, Client.Endpoint.ToString());
            });
        }

        public async Task<bool> CreateIndependentContainerIfNotExistsAsync(string containerId, int? throughput = null)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnContainer);

                CodeContract.NullOrEmptyStringThrowing(containerId, "containerId");

                #region Build-in container property
                ContainerProperties cp = new ContainerProperties()
                {
                    Id = containerId,
                    PartitionKeyPaths = new List<string>
                    {
                      "/l1PartitionKey",
                      "/l2PartitionKey",
                      "/l3PartitionKey",
                    },
                    IndexingPolicy = new IndexingPolicy()
                    {
                        Automatic = true,
                        IndexingMode = IndexingMode.Consistent
                    }
                };

                //include
                cp.IndexingPolicy.IncludedPaths.Add(new IncludedPath() { Path = "/*" });
                //exclude
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/recordHistory/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/metaInfo/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecordsCount/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecords/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/extsion1/*" });

                throughput = CosmosConnectionInfo.ThroughputType == ThroughputType.Dedicated ? CosmosConnectionInfo.Throughput : default(int?);
                #endregion

                var response = await Database.CreateContainerIfNotExistsAsync(cp, throughput).ConfigureAwait(false);

                return response.StatusCode == HttpStatusCode.Created;
            });
        }

        public async Task<bool> CreateNormalContainerIfNotExistsAsync(string containerId, int? throughput = null)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnContainer);

                CodeContract.NullOrEmptyStringThrowing(containerId, "containerId");

                #region Build-in container property
                ContainerProperties cp = new ContainerProperties()
                {
                    Id = containerId,
                    PartitionKeyPath = "/createDate",
                    IndexingPolicy = new IndexingPolicy()
                    {
                        Automatic = true,
                        IndexingMode = IndexingMode.Consistent
                    }
                };

                //include
                cp.IndexingPolicy.IncludedPaths.Add(new IncludedPath() { Path = "/*" });
                //exclude
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/recordHistory/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/metaInfo/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecordsCount/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecords/*" });
                cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/extsion1/*" });

                throughput = CosmosConnectionInfo.ThroughputType == ThroughputType.Dedicated ? CosmosConnectionInfo.Throughput : default(int?);
                #endregion

                var response = await Database.CreateContainerIfNotExistsAsync(cp, throughput).ConfigureAwait(false);

                return response.StatusCode == HttpStatusCode.Created;
            });
        }

        /// <summary>
        /// add the include path to index policy
        /// </summary>
        /// <param name="pathList"></param>
        /// <returns></returns>
        public async Task<bool> AddIndexPolicyIncludedPaths(List<string> pathList)
        {
            return await Retryer.RetryAsync(async () =>
            {
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    return Container.ReadContainerAsync();
                });

                var containerResponse = task.GetAwaiter().GetResult();
                var includePaths = containerResponse.Resource.IndexingPolicy.IncludedPaths;
                var existPathList = includePaths.Select(o => o.Path).ToList();
                var tobeAdded = pathList.Except(existPathList).ToList();
                if (tobeAdded.Count == 0) return false;

                foreach (var path in tobeAdded)
                {
                    includePaths.Insert(0, new IncludedPath { Path = path });
                }
                var response = await Container.ReplaceContainerAsync(containerResponse.Resource).ConfigureAwait(false);
                return response.StatusCode == HttpStatusCode.Created;
            });
        }

        /// <summary>
        /// Check if can update index policy now.
        /// </summary>
        /// <returns>Return false if the Cosmos DB is doing indexing now, otherwise return true.</returns>
        public bool CanUpdateIndexPolicy()
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                return Container.ReadContainerAsync(new ContainerRequestOptions { PopulateQuotaInfo = true });
            });
            var containerResponse = task.GetAwaiter().GetResult();
            // retrieve the index transformation progress from the result
            long indexTransformationProgress = long.Parse(containerResponse.Headers[CosmosConst.IndexTransformationProgressHeader]);
            return indexTransformationProgress == 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true: delete successful, false: container not exist</returns>
        public async Task<bool> DeleteContainerAsync(string containerId)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnContainer);

                CodeContract.NullOrEmptyStringThrowing(containerId, "containerId");

                try
                {
                    var container = Database.GetContainer(containerId);

                    var result = await container.DeleteContainerAsync().ConfigureAwait(false);

                    return result.StatusCode == HttpStatusCode.NoContent;
                }
                catch (CosmosException e) /*when (e.InnerException is CosmosException internalEx && internalEx.StatusCode == HttpStatusCode.NotFound)*/
                {
                    logger.Error($"Delete cosmos container failed, error: {e}");
                    return e.StatusCode == HttpStatusCode.NotFound;
                }
                catch (Exception e)
                {
                    logger.Error($"Delete cosmos container failed, error: {e}");
                    return false;
                }
            });
        }

        public async Task<List<string>> GetContainersInDBAsync()
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnContainer);

                var iterator = Database.GetContainerQueryIterator<ContainerProperties>();

                var containers = await iterator.ReadNextAsync().ConfigureAwait(false);

                List<string> results = new List<string>();
                foreach (var container in containers)
                {
                    results.Add(container.Id);
                }

                return results;
            });
        }

        public async Task<List<string>> GetContainersInDBAsync(string dbName)
        {
            return await Retryer.RetryAsync(async () =>
            {
                EnsureOperationLevel(OperationLevel.OnDatabase);

                Database = Client.GetDatabase(dbName);

                var iterator = Database.GetContainerQueryIterator<ContainerProperties>();

                var containers = await iterator.ReadNextAsync().ConfigureAwait(false);

                List<string> results = new List<string>();
                foreach (var container in containers)
                {
                    results.Add(container.Id);
                }

                return results;
            });
        }


        public void Dispose()
        {
            Client?.Dispose();
        }

        /// <summary>
        /// Will log the query metrics if CommonRoleConfiguration.LogCosmosQueryMetrics is true
        /// </summary>
        /// <param name="queryMetrics"></param>
        private void LogQueryMetrics(string sql, double ru, string indexMetrics, TimeSpan timeCost)
        {
            var enableLog = false;
            if (!bool.TryParse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.LOG_COSMOS_QUERY_METRICS], out enableLog)) return;
            if (!enableLog) return;

            logger.Warn($"Cosmos diagnose log. Sql:{sql} Time Cost:{timeCost.Milliseconds} Request Units:{ru.ToString()} Index Metrics:{indexMetrics}");
        }

        #endregion


    }

    public enum OperationLevel
    {
        Unkown = 0,
        OnDatabase = 1,
        OnContainer = 2,
        OnItem = 3
    }
}
