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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB
{
    public class RMAzureCosmosDBContext
    {
        private static string TenantId => TenantLocalValue.LogonGroupId;

        private static readonly Dictionary<string, Container> s_containers = new();

        private static readonly SemaphoreSlim _asyncLock = new(1);

        private static IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static CosmosClient Client  => new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;

        public static async Task<RMAzureCosmosDBContainer> GetContainerAsync(bool createIfNotExists = true)
        {
            return await GetContainerAsync(TenantId, createIfNotExists);
        }

        public static async Task<RMAzureCosmosDBContainer> GetContainerAsync(string tenantId, bool createIfNotExists = true)
        {
            if (!s_containers.ContainsKey(tenantId))
            {
                try
                {
                    await _asyncLock.WaitAsync();
                    if (!s_containers.ContainsKey(tenantId))
                    {
                        Container container = null;
                        if (createIfNotExists)
                        {
                            container = RMCosmosDBIndependentController.IsEnabledIndependent() ? await CreateIndependentContainerIfNotExistsAsync(tenantId).ConfigureAwait(false) : await CreateNormalContainerIfNotExistsAsync(tenantId).ConfigureAwait(false);
                        }
                        else
                        {
                            if(!await ExistsContainer())
                            {
                                throw new Exception($"Database for tenant [{tenantId}] not found.");
                            }

                            var connectionInfo = await RMDBContextManager.GetCosmosDBConnectionAsync();
                            var database = Client.GetDatabase(connectionInfo.DatabaseId);
                            container = database.GetContainer(tenantId);
                        }
                        s_containers.Add(tenantId, container);
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            }

            return new RMAzureCosmosDBContainer(s_containers[tenantId]);
        }

        public static async Task<bool> ExistsContainer()
        {
            var connectionInfo = await RMDBContextManager.GetCosmosDBConnectionAsync();
            if(connectionInfo == null)
            {
                return false;
            }
            var existsDatabase = await ExistsDatabase(connectionInfo.DatabaseId);
            if(!existsDatabase)
            {
                return false;
            }

            var database = Client.GetDatabase(connectionInfo.DatabaseId);
            using var iterator = database.GetContainerQueryIterator<ContainerProperties>($"SELECT * FROM c WHERE c.id = '{connectionInfo.CollectionId}'");
            if(iterator.HasMoreResults)
            {
                foreach (var properties in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    if (properties.Id.Equals(connectionInfo.CollectionId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> ExistsDatabase(string databaseId)
        {
            var databaseIds = new HashSet<string>();
            using var iterator = Client.GetDatabaseQueryIterator<DatabaseProperties>();
            while (iterator.HasMoreResults)
            {
                foreach (var properties in await iterator.ReadNextAsync().ConfigureAwait(false))
                {
                    if (properties.Id.Equals(databaseId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<Container> CreateIndependentContainerIfNotExistsAsync(string tenantId)
        {
            var connectionInfo = await RMDBContextManager.GetCosmosDBConnectionAsync();
            var properties = new ContainerProperties
            {
                Id = connectionInfo.CollectionId,
                PartitionKeyPaths = new List<string>
                {
                    "/l1PartitionKey",
                    "/l2PartitionKey",
                    "/l3PartitionKey",
                },
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent
                }
            };

            properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath() { Path = "/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/recordHistory/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/metaInfo/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecordsCount/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecords/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/extsion1/*" });

            var throughput = connectionInfo.ThroughputType == ThroughputType.Dedicated ? connectionInfo.Throughput : default(int?);
            var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
            var resource = connectionInfo.Resource;
            if (!string.IsNullOrEmpty(extensionConnectionStr))
            {
                resource = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr).Count;
            }
            var database = Client.GetDatabase(connectionInfo.DatabaseId);
            var response = await database.CreateContainerIfNotExistsAsync(properties, throughput).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var dao = new Dao.Impl.DBInfoDao();
                dao.AddIndependentExplorerDBMappingInfo(new RMDBInfoDto { DBName = connectionInfo.DatabaseId, ContainerName = connectionInfo.CollectionId, Resource = resource });
            }

            return database.GetContainer(tenantId);
        }

        public static async Task<Container> CreateNormalContainerIfNotExistsAsync(string tenantId)
        {
            var connectionInfo = await RMDBContextManager.GetCosmosDBConnectionAsync();
            var properties = new ContainerProperties
            {
                Id = connectionInfo.CollectionId,
                PartitionKeyPath = "/createDate",
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent
                }
            };

            properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath() { Path = "/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/recordHistory/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/metaInfo/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecordsCount/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/relatedRecords/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/extsion1/*" });

            var throughput = connectionInfo.ThroughputType == ThroughputType.Dedicated ? connectionInfo.Throughput : default(int?);
            var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
            var resource = connectionInfo.Resource;
            if (!string.IsNullOrEmpty(extensionConnectionStr))
            {
                resource = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr).Count;
            }
            var database = Client.GetDatabase(connectionInfo.DatabaseId);
            var response = await database.CreateContainerIfNotExistsAsync(properties, throughput).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var dao = new Dao.Impl.DBInfoDao();
                dao.AddExplorerDBMappingInfo(new RMDBInfoDto { DBName = connectionInfo.DatabaseId, ContainerName = connectionInfo.CollectionId, Resource = resource });
            }

            return database.GetContainer(tenantId);
        }
    }
}
