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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using Microsoft.Azure.Cosmos;
using System;
using System.Diagnostics.Metrics;
using System.Net;

namespace AvePoint.RA.VectorDataCenter.Storage
{
    public class CosmosDbVectorStore : IVectorStore
    {
        public string Name => "CosmosDB";

        private static readonly RALogger _logger = RALogger.GetInstance(typeof(CosmosDbVectorStore));
        private readonly Container _container;
        private readonly RMTenantVectorCosmosMappingDao _mappingDao;

        private readonly string _connectionString = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.VECTOR_DB_CONNECTION_STRING];
        private readonly string _databaseId;
        private readonly string _containerId;
        private const int QueryMaxRetries = 5;

        public CosmosDbVectorStore(bool Init = true)
        {
            if (!Init)
                return;
            _mappingDao = new RMTenantVectorCosmosMappingDao();

            var client = CosmosClientManager.CreateCosmosClient(_connectionString);

            // Get the current tenant's Database and Container name
            var tenantId = Guid.Parse(TenantLocalValue.LogonGroupId);
            var (dbName, containerName) = _mappingDao.GetOrCreateDatabaseAndContainerName(tenantId);
            _databaseId = dbName;
            _containerId = containerName;
            _container = client.GetContainer(_databaseId, _containerId);
        }
        /// <summary>
        /// Constructor for specifying a custom connection string (e.g., for testing)
        /// </summary>
        /// <param name="connectionString"></param>
        public CosmosDbVectorStore(string connectionString)
        {
            _connectionString = connectionString;
            _databaseId = "RECO_Vector";
            _containerId = "test1";

            var client = CosmosClientManager.CreateCosmosClient(_connectionString);
            _container = client.GetContainer(_databaseId, _containerId);
        }

        public async Task CreateDBAsync()
        {
            var client = CosmosClientManager.CreateCosmosClient(_connectionString);

            // Create database if not exists
            await client.CreateDatabaseIfNotExistsAsync(_databaseId, ThroughputProperties.CreateManualThroughput(400));
            // Define custom indexing policy to include the 'vector' field
            var containerProperties = new ContainerProperties(_containerId, "/id")
            {
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent,
                    IncludedPaths =
                    {

                        new IncludedPath { Path = "/*" }
                    },
                    ExcludedPaths =
                    {
                        new ExcludedPath { Path = "/vector/*" },
                        new ExcludedPath { Path = "/_etag/?" }
                    }
                },

                VectorEmbeddingPolicy = new VectorEmbeddingPolicy(
                    [
                        new Microsoft.Azure.Cosmos.Embedding
                        {
                            Path = "/vector",
                            DataType = VectorDataType.Float32,
                            Dimensions = 1536,
                            DistanceFunction = DistanceFunction.Cosine,
                        }
                    ]
                )
            };
            await client.GetDatabase(_databaseId).CreateContainerIfNotExistsAsync(containerProperties);
        }

        public async Task StoreVectorAsync(Guid id, string name, float[] vector, string metadata)
        {
            // Ensure DB and container exist before storing
            await CreateDBAsync();
            var doc = new VectorDocument { id = id.ToString(), name = name, vector = vector, metadata = metadata };
            await _container.UpsertItemAsync(doc);
        }

        public async Task<string> QueryMetaDataByTermId(Guid termId)
        {
            string sql = $@"SELECT c.id, c.metadata FROM c where c.id = '{termId}'";
            var queryDef = new QueryDefinition(sql);
            var results = new List<string>();
            try
            {
                var iterator = _container.GetItemQueryIterator<dynamic>(queryDef);
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        string metadata = item.metadata;
                        results.Add(metadata);
                    }
                }
                return results.Count == 0 ? string.Empty : results[0];
            }
            catch (Exception e)
            {
                _logger.Error($"Error querying by term id CosmosDB: {e.Message}", e);
                return string.Empty;
            }
        }

        public async Task<(string id, float? scoreFromStore)[]> QuerySimilarAsync(float[] vector, int topK = 5)
        {
            string vectorParam = string.Join(",", vector);
            string sql = $@"SELECT c.id, VectorDistance(c.vector, [{vectorParam}], false) AS distance FROM c ORDER BY VectorDistance(c.vector, [{vectorParam}], false) OFFSET 0 LIMIT {topK}";
            var queryDef = new QueryDefinition(sql);
            for(int attempt = 0; attempt < QueryMaxRetries; attempt++)
            {
                try
                {
                    var results = new List<(string, float?)>();
                    var iterator = _container.GetItemQueryIterator<dynamic>(queryDef);
                    while (iterator.HasMoreResults)
                    {
                        foreach (var item in await iterator.ReadNextAsync())
                        {
                            string id = item.id;
                            // CosmosDB returns vector as array of doubles, convert to float[]
                            float? distance = Convert.ToSingle(item.distance);
                            results.Add((id, distance));
                        }
                    }
                    return results.ToArray();
                }
                catch (CosmosException cosmosEx)
                {
                    bool retryable =
                        cosmosEx.StatusCode == HttpStatusCode.TooManyRequests ||
                        cosmosEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        cosmosEx.Message.Contains("Request rate is large", StringComparison.OrdinalIgnoreCase);

                    if (!retryable)
                    {
                        _logger.Error($"Non-retryable CosmosDB exception during similarity query (status {cosmosEx.StatusCode}): {cosmosEx.Message}", cosmosEx);
                        break;
                    }

                    TimeSpan delay = cosmosEx.RetryAfter.HasValue ?
                                     cosmosEx.RetryAfter.Value : TimeSpan.FromSeconds(1000 * attempt);

                    _logger.Warn($"Retry {attempt + 1}/{QueryMaxRetries} due to {cosmosEx.StatusCode}. Waiting {delay.TotalSeconds:F2}s. Message={cosmosEx.Message}");
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error querying CosmosDB: {ex.Message}", ex);
                }
            };
            return Array.Empty<(string, float?)>();
        }

        public async Task DeleteVectorAsync(Guid id)
        {
            try
            {
                await _container.DeleteItemAsync<VectorDocument>(id.ToString(), new PartitionKey(id.ToString()));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.Warn($"Item with id: {id} not found. Skipping.");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private class VectorDocument
        {
            public string id { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public float[] vector { get; set; } = Array.Empty<float>();
            public string metadata { get; set; } = string.Empty;
        }

        public async Task DeleteVectorsByIdsAsync(List<Guid> ids)
        {
            foreach (var id in ids)
            {
                try
                {
                    await _container.DeleteItemAsync<VectorDocument>(id.ToString(), new PartitionKey(id.ToString()));
                    _logger.Info($"Deleted item with id: {id}");
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Warn($"Item with id: {id} not found. Skipping.");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error deleting item with id: {id}", ex);
                }
            }
        }
        public async Task DropVectorDbIfExist(string dbName, string containerId)
        {
            var client = CosmosClientManager.CreateCosmosClient(_connectionString);
            var database = client.GetDatabase(dbName);
            try
            {
                var container = database.GetContainer(containerId);
                await container.DeleteContainerAsync();
                _logger.Info($"Dropped container '{containerId}' and related objects from database '{dbName}'.");
            }
            catch (CosmosException ex) 
            {
                _logger.Info($"Error while dropping container {containerId} in database {dbName}: {ex.Message}", ex);
            }
        }

    }
}
