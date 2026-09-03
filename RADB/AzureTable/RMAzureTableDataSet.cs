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
using AvePoint.RA.Contract.Tenant;
using Azure.Data.Tables;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureTable
{
    public class RMAzureTableDataSet<T> where T : class, ITableEntity, new()
    {
        public RMAzureTableContext Context { get; private set; }

        public string TableName { get; private set; }

        public bool EnableMultipleTenant { get; private set; }

        public RMAzureTableDataSet(RMAzureTableContext context, string tableName)
            : this(context, tableName, false)
        { }

        public RMAzureTableDataSet(RMAzureTableContext context, string tableName, bool enableMultipleTenant)
        {
            Context = context;
            TableName = tableName;
            EnableMultipleTenant = enableMultipleTenant;
        }

        public async Task Add(T entity)
        {
            var client = await GetTableClient();
            var response = await client.AddEntityAsync(entity);
            if(response.IsError)
            {
                throw new RMAzureTableException(response.ReasonPhrase);
            }
        }

        public async Task AddRange(IEnumerable<T> entities)
        {
            var client = await GetTableClient();
            var addEntitiesBatch = entities.Select(item => new TableTransactionAction(TableTransactionActionType.Add, item));
            var response = await client.SubmitTransactionAsync(addEntitiesBatch).ConfigureAwait(false);
            var failedResponse = response.Value.FirstOrDefault(item => item.IsError);
            if(failedResponse != null)
            {
                throw new RMAzureTableException(failedResponse.ReasonPhrase);
            }
        }

        public async Task UpsertMerge(T entity)
        {
            var client = await GetTableClient();
            var response = await client.UpsertEntityAsync(entity, TableUpdateMode.Merge);
            if(response.IsError)
            {
                throw new RMAzureTableException(response.ReasonPhrase);
            }
        }

        public async Task UpsertMergeRange(IEnumerable<T> entities)
        {
            var client = await GetTableClient();
            var upsertEntitiesBatch = entities.Select(item => new TableTransactionAction(TableTransactionActionType.UpsertMerge, item));
            var response = await client.SubmitTransactionAsync(upsertEntitiesBatch).ConfigureAwait(false);
            var failedResponse = response.Value.FirstOrDefault(item => item.IsError);
            if (failedResponse != null)
            {
                throw new RMAzureTableException(failedResponse.ReasonPhrase);
            }
        }

        public async Task UpsertReplace(T entity)
        {
            var client = await GetTableClient();
            var response = await client.UpsertEntityAsync(entity, TableUpdateMode.Replace);
            if (response.IsError)
            {
                throw new RMAzureTableException(response.ReasonPhrase);
            }
        }

        public async Task UpsertReplaceRange(IEnumerable<T> entities)
        {
            var client = await GetTableClient();
            var upsertEntitiesBatch = entities.Select(item => new TableTransactionAction(TableTransactionActionType.UpsertReplace, item));
            var response = await client.SubmitTransactionAsync(upsertEntitiesBatch).ConfigureAwait(false);
            var failedResponse = response.Value.FirstOrDefault(item => item.IsError);
            if (failedResponse != null)
            {
                throw new RMAzureTableException(failedResponse.ReasonPhrase);
            }
        }

        public async Task Delete(string partitionKey, string rowKey)
        {
            var client = await GetTableClient();
            var response = await client.DeleteEntityAsync(partitionKey, rowKey);
            if(response.IsError)
            {
                throw new RMAzureTableException(response.ReasonPhrase);
            }
        }

        public Task Delete(T entity)
        {
            return Delete(entity.PartitionKey, entity.RowKey);
        }

        public async Task<int> Delete(Expression<Func<T, bool>> filter)
        {
            var needDeletedItems = await Query(filter).ToListAsync();
            if(needDeletedItems.Count > 0)
            {
                await DeleteRange(needDeletedItems);
            }

            return needDeletedItems.Count;
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            var client = await GetTableClient();
            var deleteEntitiesBatch = entities.Select(item => new TableTransactionAction(TableTransactionActionType.Delete, item));
            var response = await client.SubmitTransactionAsync(deleteEntitiesBatch).ConfigureAwait(false);
            var failedResponse = response.Value.FirstOrDefault(item => item.IsError);
            if (failedResponse != null)
            {
                throw new RMAzureTableException(failedResponse.ReasonPhrase);
            }
        }
        
        public async Task<bool> Exists(Expression<Func<T, bool>> filter)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter, maxPerPage: 1);
            return (await asyncPageable.FirstOrDefaultAsync()) != null;
        }

        public async Task<T> FirstOrDefault(Expression<Func<T, bool>> filter)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter, maxPerPage: 1);
            return await asyncPageable.FirstOrDefaultAsync();
        }

        public async Task<int> Count()
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(select: new List<string> { "PartitionKey" });
            return await asyncPageable.CountAsync();
        }

        public async Task<int> Count(Expression<Func<T, bool>> filter)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(filter, select: new List<string> { "PartitionKey" });
            return await asyncPageable.CountAsync();
        }

        public async Task<long> LongCount()
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(select: new List<string> { "PartitionKey" });
            return await asyncPageable.LongCountAsync();
        }

        public async Task<long> LongCount(Expression<Func<T, bool>> filter)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(filter, select: new List<string> { "PartitionKey" });
            return await asyncPageable.LongCountAsync();
        }

        public async IAsyncEnumerable<T> Query()
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>();
            await foreach(var res in asyncPageable)
            {
                yield return res;
            }
        }

        public async IAsyncEnumerable<T> Query(IEnumerable<string> selectProperties)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(select: selectProperties);
            await foreach (var res in asyncPageable)
            {
                yield return res;
            }
        }

        public async IAsyncEnumerable<T> Query(Expression<Func<T, bool>> filter)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter);
            await foreach (var res in asyncPageable)
            {
                yield return res;
            }
        }

        public async IAsyncEnumerable<T> Query(Expression<Func<T, bool>> filter, IEnumerable<string> selectProperties)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter, select: selectProperties);
            await foreach (var res in asyncPageable)
            {
                yield return res;
            }
        }

        public async Task<(string ContinuatioinToken, IEnumerable<T> Values)> QueryWithPagination(int pageSize, string continuationToken)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(maxPerPage: pageSize);
            var page = await asyncPageable.AsPages(continuationToken).FirstOrDefaultAsync();
            return (page.ContinuationToken, page.Values);
        }

        public async Task<(string ContinuatioinToken, IEnumerable<T> Values)> QueryWithPagination(Expression<Func<T, bool>> filter, int pageSize, string continuationToken)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter, maxPerPage: pageSize);
            var page = await asyncPageable.AsPages(continuationToken).FirstOrDefaultAsync();
            return (page.ContinuationToken, page.Values);
        }

        public async Task<(string ContinuatioinToken, IEnumerable<T> Values)> QueryWithPagination(Expression<Func<T, bool>> filter, IEnumerable<string> selectProperties, int pageSize, string continuationToken)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync(filter, maxPerPage: pageSize, select: selectProperties);
            var page = await asyncPageable.AsPages(continuationToken).FirstOrDefaultAsync();
            return (page.ContinuationToken, page.Values);
        }

        public async Task<(string ContinuatioinToken, IEnumerable<T> Values)> QueryWithPagination(IEnumerable<string> selectProperties, int pageSize, string continuationToken)
        {
            var client = await GetTableClient();
            var asyncPageable = client.QueryAsync<T>(maxPerPage: pageSize, select: selectProperties);
            var page = await asyncPageable.AsPages(continuationToken).FirstOrDefaultAsync();
            return (page.ContinuationToken, page.Values);
        }

        private async Task<TableClient> GetTableClient()
        {
            var tableName = TableName;
            if (EnableMultipleTenant)
            {
                tableName = TableName + TenantLocalValue.LogonGroupId.Replace("-", "");
            }

            return await Context.GetTableClientAsync(tableName, true);
        }
    }
}
