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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Query.SQL
{

    public record RMAzureCosmosDBQueryParameter(string Name, object Value);

    public class RMAzureCosmosDBSqlQuerier
    {
        private readonly Container CosmosContainer;

        private string QuerySql { get; set; }

        private RMAzureCosmosDBQueryParameter[] Parameters { get; set; } = Array.Empty<RMAzureCosmosDBQueryParameter>();

        internal RMAzureCosmosDBSqlQuerier(Container container)
        {
            CosmosContainer = container;
        }

        public RMAzureCosmosDBSqlQuerier WithSql(string sql)
        {
            QuerySql = sql;
            return this;
        }

        public RMAzureCosmosDBSqlQuerier WithParameter(params RMAzureCosmosDBQueryParameter[] parameters)
        {
            Parameters = parameters;
            return this;
        }

        public async IAsyncEnumerable<TResult> AllAsync<TResult>()
        {
            var queryDefinition = BuildQueryDefinition();
            using var iterator = CosmosContainer.GetItemQueryIterator<TResult>(queryDefinition, requestOptions: new QueryRequestOptions
            {
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
            });
            while(iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>()
        {
            TResult result = default;
            var queryDefinition = BuildQueryDefinition();

            string continuationToken = null;
            var pageSize = 1;
            do
            {
                using var iterator = CosmosContainer.GetItemQueryIterator<TResult>(queryDefinition, continuationToken, new QueryRequestOptions
                {
                    MaxItemCount = pageSize,
                    MaxBufferedItemCount = -1,
                    MaxConcurrency = -1,
                });

                if (!iterator.HasMoreResults) break;

                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                result = response.FirstOrDefault();

                continuationToken = response.ContinuationToken;
                pageSize -= response.Count;

            } while (pageSize > 0 && !string.IsNullOrEmpty(continuationToken));

            return result;
        }

        public async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>()
        {
            var res = new Dictionary<TKey, TValue>();

            await foreach(var item in AllAsync<KeyValuePair<TKey, TValue>>().ConfigureAwait(false))
            {
                res.Add(item.Key, item.Value);
            }

            return res;
        }

        public async Task<RMAzureCosmosDBQueryPagniationResult<TResult>> PaginateAsync<TResult>(string continuationToken, int pageSize)
        {
            var queryDefinition = BuildQueryDefinition();
            var items = new List<TResult>(pageSize);
            do
            {
                using var iterator = CosmosContainer.GetItemQueryIterator<TResult>(queryDefinition, continuationToken, new QueryRequestOptions
                {
                    MaxItemCount = pageSize,
                    MaxBufferedItemCount = -1,
                    MaxConcurrency = -1,
                });

                if (!iterator.HasMoreResults) break;

                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                continuationToken = response.ContinuationToken;

                foreach (var item in response)
                {
                    items.Add(item);
                }

                pageSize -= response.Count;

            } while (pageSize > 0 && !string.IsNullOrEmpty(continuationToken));

            return new RMAzureCosmosDBQueryPagniationResult<TResult>(continuationToken, items);
        }

        private QueryDefinition BuildQueryDefinition()
        {
            var queryDefinition = new QueryDefinition(QuerySql);
            foreach (var parameter in Parameters)
            {
                queryDefinition.WithParameter(parameter.Name, parameter.Value);
            }

            return queryDefinition;
        }
    }
}
