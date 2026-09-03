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
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB
{
    public class RMAzureCosmosDBContainer
    {
        private readonly Container CosmosContainer;

        public RMAzureCosmosDBContainer(Container container)
        {
            CosmosContainer = container;
        }

        public RMAzureCosmosDBLinqQuerier UseLinqQuery()
        {
            return new RMAzureCosmosDBLinqQuerier(CosmosContainer);
        }

        public RMAzureCosmosDBSqlQuerier UseSqlQuery()
        {
            return new RMAzureCosmosDBSqlQuerier(CosmosContainer);
        }

        public RMAzureCosmosDBConcurrentActionBuilder UseConcurrentAction()
        {
            return RMAzureCosmosDBConcurrentActionBuilder.CreateBuilder(this);
        }

        public async Task AddAsync(Record record)
        {
            record.SetPartitionKeys();
            await CosmosContainer.CreateItemAsync(record, record.BuildPartitionKey());
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> AddRangeAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());
            foreach (var record in records)
            {
                try
                {
                    await AddAsync(record);
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task PatchAsync(Record record, List<PatchOperation> patchOperations)
        {
            record.SetPartitionKeys();
            await CosmosContainer.PatchItemAsync<Record>(record.Id.ToString(), record.BuildPartitionKey(), patchOperations);
        }

        public async Task UpsertAsync(Record record)
        {
            record = record.SetPartitionKeys();
            await CosmosContainer.UpsertItemAsync(record, record.BuildPartitionKey());
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> UpsertRangeAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    await UpsertAsync(record);
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task<bool> UpsertWithOptimisticLockAsync(Record record)
        {
            if (string.IsNullOrWhiteSpace(record.ETag))
            {
                await UpsertAsync(record);
                return true;
            }

            try
            {
                record.SetPartitionKeys();
                await CosmosContainer.UpsertItemAsync(record, record.BuildPartitionKey(), new ItemRequestOptions
                {
                    IfMatchEtag = record.ETag
                });
                return true;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> UpsertRangeWithOptimisticLockAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    var isSucceed = await UpsertWithOptimisticLockAsync(record);
                    if (!isSucceed)
                    {
                        res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, true, null));
                    }
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task ReplaceAsync(Record record)
        {
            record.SetPartitionKeys();
            await CosmosContainer.ReplaceItemAsync(record, record.Id.ToString(), record.BuildPartitionKey());
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> ReplaceRangeAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    await ReplaceAsync(record);
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task<bool> ReplaceWithOptimisticLockAsync(Record record)
        {
            if (string.IsNullOrWhiteSpace(record.ETag))
            {
                throw new ArgumentException("[record.ETag] Must not be empty.");
            }

            try
            {
                record.SetPartitionKeys();
                await CosmosContainer.ReplaceItemAsync(record, record.Id.ToString(), record.BuildPartitionKey(), new ItemRequestOptions
                {
                    IfMatchEtag = record.ETag
                });
                return true;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> ReplaceRangeWithOptimisticLockAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    var isSucceed = await ReplaceWithOptimisticLockAsync(record);
                    if (!isSucceed)
                    {
                        res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, true, null));
                    }
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task DeleteAsync(Guid id, PartitionKey partitionKey)
        {
            await CosmosContainer.DeleteItemAsync<Record>(id.ToString(), partitionKey);
        }

        public async Task DeleteAsync(Record record)
        {
            await CosmosContainer.DeleteItemAsync<Record>(record.Id.ToString(), record.BuildPartitionKey());
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> DeleteRangeAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    await DeleteAsync(record);
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }

        public async Task<bool> DeleteWithOptimisticLockAsync(Record record)
        {
            if (string.IsNullOrWhiteSpace(record.ETag))
            {
                throw new ArgumentException("[record.ETag] Must not be empty.");
            }

            try
            {
                await CosmosContainer.DeleteItemAsync<Record>(record.Id.ToString(), record.BuildPartitionKey(), new ItemRequestOptions
                {
                    IfMatchEtag = record.ETag
                });
                return true;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<IEnumerable<RMAzureCosmosDBRangeActionFailedResult>> DeleteRangeWithOptimisticLockAsync(IEnumerable<Record> records)
        {
            var res = new List<RMAzureCosmosDBRangeActionFailedResult>(records.Count());

            foreach (var record in records)
            {
                try
                {
                    var isSucceed = await DeleteWithOptimisticLockAsync(record);
                    if (!isSucceed)
                    {
                        res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, true, null));
                    }
                }
                catch (Exception e)
                {
                    res.Add(new RMAzureCosmosDBRangeActionFailedResult(record, false, e));
                }
            }

            return res;
        }
    }
}
