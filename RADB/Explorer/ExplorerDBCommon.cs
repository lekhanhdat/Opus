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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Common.Retrying;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AvePoint.RA.DB.Explorer
{
    public class ExplorerDBCommon
    {
        private static readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IRMCache Cache = PlatformWindsorManager.GetService<IRMCache>();

        private const string PARTITION_KEY = "PARTITION_KEY";

        public static async Task<GetCosmosDBItemResult> GetCosmosPartitionKeyInfo(int itemPartitionKey, Guid itemId, Container CosmosContainer)
        {
            var orignalPartitionKey = (itemPartitionKey / 100).ToString().Length >= 8 ? (itemPartitionKey / 100) : itemPartitionKey;
            var cacheKey = PARTITION_KEY + orignalPartitionKey.ToString();
            Record dbItem = null;

            // Use RMRetryer for Cosmos DB read
            await RMRetryerBuilder.CreateBuilder().Build().RetryAsync(async () =>
            {
                try
                {
                    dbItem = (await CosmosContainer.ReadItemAsync<Record>(itemId.ToString(), new PartitionKey(itemPartitionKey))).Resource;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    dbItem = null;
                }
            });

            var partitionKeyListStr = string.Empty;
            var partitionKeyList = new List<int>();
            // Use RMRetryer for cache/dao access
            await RMRetryerBuilder.CreateBuilder().Build().RetryAsync(async () =>
            {
                try
                {
                    partitionKeyListStr = await Cache.GetAsync<string>(cacheKey) ?? RMKeyValueDao.GetValueByKey(cacheKey)?.Value;
                }
                catch
                {
                    partitionKeyListStr = RMKeyValueDao.GetValueByKey(cacheKey)?.Value;
                }
            });
            partitionKeyList = string.IsNullOrEmpty(partitionKeyListStr) ? [] : SerializerHelper.DeserializeByJsonSerializer<List<int>>(partitionKeyListStr);

            if (dbItem == null && !string.IsNullOrEmpty(partitionKeyListStr))
            {
                foreach (var partitionKey in partitionKeyList)
                {
                    // Use RMRetryer for each partition key read
                    await RMRetryerBuilder.CreateBuilder().Build().RetryAsync(async () =>
                    {
                        try
                        {
                            dbItem = (await CosmosContainer.ReadItemAsync<Record>(itemId.ToString(), new PartitionKey(partitionKey))).Resource;
                            itemPartitionKey = partitionKey;
                        }
                        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            dbItem = null;
                        }
                    });
                    if (dbItem != null)
                    {
                        break;
                    }
                }
            }

            return new()
            {
                DBItem = dbItem,
                ItemPartitionKey = itemPartitionKey,
                PartitionKeyList = partitionKeyList,
                OriginalPartitionKey = orignalPartitionKey,
                CacheKey = cacheKey
            };
        }

        public static async Task<ItemResponse<Record>> ProcessRenewPartition(Container CosmosContainer, Record item, Record dbItem,string cacheKey, int itemPartitionKey, List<int> partitionKeyList, int originalPartitionKey)
        {
            if (partitionKeyList.Count > 0)
            {
                itemPartitionKey = partitionKeyList.Last();
            }
            else
            {
                itemPartitionKey = originalPartitionKey * 100 + 1;
                await RMKeyValueDao.SaveOrUpdateAsync(new()
                {
                    Key = cacheKey,
                    Value = SerializerHelper.SerializeByJsonSerializer(new List<int> { itemPartitionKey })
                });
                await Cache.SetAsync(cacheKey, SerializerHelper.SerializeByJsonSerializer(new List<int> { itemPartitionKey }));
            }
            try
            {
                item.CreateDate = itemPartitionKey;
                var result = await InnerUpsertItemAsync(CosmosContainer, item);
                if (dbItem != null && itemPartitionKey != dbItem.CreateDate)
                {
                    await CosmosContainer.DeleteItemAsync<Record>(item.Id.ToString(), new PartitionKey(dbItem.CreateDate));
                }
                return result;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden && ex.SubStatusCode == 1014)
                {
                    itemPartitionKey = partitionKeyList.Last() + 1;
                    partitionKeyList.Add(itemPartitionKey);
                    await Cache.RemoveAsync(cacheKey);
                    await Cache.SetAsync(cacheKey, SerializerHelper.SerializeByJsonSerializer(partitionKeyList));
                    await RMKeyValueDao.SaveOrUpdateAsync(new()
                    {
                        Key = cacheKey,
                        Value = SerializerHelper.SerializeByJsonSerializer(partitionKeyList)
                    });
                    item.CreateDate = itemPartitionKey;
                    var result = await InnerUpsertItemAsync(CosmosContainer, item);
                    if (dbItem != null && itemPartitionKey != dbItem.CreateDate)
                    {
                        await CosmosContainer.DeleteItemAsync<Record>(item.Id.ToString(), new PartitionKey(dbItem.CreateDate));
                    }
                    return result;
                }
                throw;
            }
        }

        private static async Task<ItemResponse<Record>> InnerUpsertItemAsync(Container cosmosContainer, Record item)
        {
            if (string.IsNullOrWhiteSpace(item.ETag))
            {
                return await cosmosContainer.UpsertItemAsync(item, new PartitionKey(item.CreateDate));
            }
            return await cosmosContainer.UpsertItemAsync(item, new PartitionKey(item.CreateDate), new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            });
        }
    }

    public class GetCosmosDBItemResult
    {
        public Record DBItem { get; set; } = null;

        public int ItemPartitionKey { get; set; }

        public int OriginalPartitionKey { get; set; }

        public List<int> PartitionKeyList { get; set; } = [];
        
        public string CacheKey { get; set; }
    }
}
