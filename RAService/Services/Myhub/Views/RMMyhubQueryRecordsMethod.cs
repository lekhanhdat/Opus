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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MyHub.NewMethods
{
    public class RMMyhubQueryRecordsMethod
    {
        RALogger logger = RALogger.GetInstance(typeof(RMMyhubQueryRecordsMethod));
        private readonly Lazy<Task<Container>> _containerFactory;
        public RMMyhubQueryRecordsMethod()
        {
            _containerFactory = new Lazy<Task<Container>>(CreateContainerAsync);
        }
        private static async Task<Container> CreateContainerAsync()
        {
            var connectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
            var client = new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;
            return client.GetDatabase(connectionInfo.DatabaseId).GetContainer(connectionInfo.CollectionId);
        }

        private async Task<Container> GetContainerAsync()
        {
            return await _containerFactory.Value;
        }
        public async Task<T> QuerySingleAsync<T>(string sql, List<SqlParameter> parameters)
        {
            var queryDefinition = BuildQueryDefinition(sql, parameters ?? new List<SqlParameter>());
            var container = await GetContainerAsync();
            using FeedIterator<T> feedIterator = container.GetItemQueryIterator<T>(queryDefinition);

            if (!feedIterator.HasMoreResults)
            {
                return default(T);
            }

            var response = await feedIterator.ReadNextAsync();
            return response.FirstOrDefault();
        }
        public async Task<(List<T> Items, string ContinuationToken)> QueryAsync<T>(
            string sql,
            List<SqlParameter> parameters = null,
            string continuationToken = null,
            int maxItemCount = 30,
            string partitionKey = null)
        {
            var queryDefinition = BuildQueryDefinition(sql, parameters);
            var container = await GetContainerAsync();
            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = maxItemCount,
                PartitionKey = string.IsNullOrWhiteSpace(partitionKey)
                    ? null
                    : new PartitionKey(partitionKey)
            };

            using var queryIterator = container.GetItemQueryIterator<T>(
                queryDefinition,
                continuationToken,
                queryOptions);

            if (!queryIterator.HasMoreResults)
                return (new List<T>(), null);

            var response = await queryIterator.ReadNextAsync();

            string token = null;
            try
            {
                token = response.ContinuationToken;
            }
            catch (ArgumentException)
            {
                // GROUP BY 查询不支持 ContinuationToken
                token = null;
            }

            return (response.ToList(), token);
        }
        public async Task<List<T>> QueryAllAsync<T>(string sql, List<SqlParameter> parameters = null, string partitionKey = null)
        {
            var allItems = new List<T>();
            string continuationToken = null;

            do
            {
                var (batch, token) = await QueryAsync<T>( sql, parameters, continuationToken, partitionKey: partitionKey);
                allItems.AddRange(batch);
                continuationToken = token;
            }
            while (continuationToken != null);

            return allItems;
        }
        private static QueryDefinition BuildQueryDefinition(string sql, List<SqlParameter> parameters)
        {
            var queryDefinition = new QueryDefinition(sql);
            foreach (var parameter in parameters)
            {
                queryDefinition = queryDefinition.WithParameter(parameter.ParameterName, parameter.Value);
            }

            return queryDefinition;
        }
    }
}
