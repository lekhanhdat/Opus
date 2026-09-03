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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class CosmosClientManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(CosmosClientManager));

        private static readonly TimeSpan DefaultIdleTcpConnectionTimeout = TimeSpan.FromMinutes(20);

        private static Dictionary<string, CosmosClient> CosmosClientDic = [];

        public CosmosClient Client { get; set; }

        /// <summary>
        /// Creates CosmosClientOptions based on environment configuration
        /// </summary>
        /// <returns>Configured CosmosClientOptions for the current environment</returns>
        public static CosmosClientOptions CreateCosmosClientOptions()
        {
            // Check if running in GCP environment to set appropriate connection mode
            var envConfig = RMGlobalConfiguration.EnvSetting;
            var connectionMode = envConfig.IsGCPEnvironment ? ConnectionMode.Gateway : ConnectionMode.Direct;
            Logger.Info($"Setting Cosmos DB connection mode to {connectionMode} (GCP Environment: {envConfig.IsGCPEnvironment})");

            var clientOptions = new CosmosClientOptions
            {
                ConnectionMode = connectionMode,
                AllowBulkExecution = true,
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                RequestTimeout = TimeSpan.FromMinutes(5)
            };

            // Only set PortReuseMode when using Direct connection mode
            if (connectionMode == ConnectionMode.Direct)
            {
                clientOptions.PortReuseMode = PortReuseMode.PrivatePortPool;
                clientOptions.IdleTcpConnectionTimeout = DefaultIdleTcpConnectionTimeout;
            }

            return clientOptions;
        }

        /// <summary>
        /// Creates a CosmosClient with environment-specific configuration
        /// </summary>
        /// <param name="connectionString">Cosmos DB connection string</param>
        /// <returns>Configured CosmosClient</returns>
        public static CosmosClient CreateCosmosClient(string connectionString)
        {
            var clientOptions = CreateCosmosClientOptions();
            return new CosmosClient(connectionString, clientOptions);
        }

        public CosmosClientManager(string tenantId)
        {
            try
            {
                if (CosmosClientDic.TryGetValue(tenantId, out CosmosClient value))
                {
                    Client = value;
                    return;
                }
                var dBInfoDao = new DBInfoDao();
                var dbName = dBInfoDao.GetDBNameByTenantId(tenantId);
                var resource = dBInfoDao.GetExplorerDBResource(tenantId);
                var connection = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING];
                var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
                if (!string.IsNullOrEmpty(extensionConnectionStr) && (string.IsNullOrEmpty(dbName) || resource != 0))
                {
                    var currentAccounts = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr);
                    var currentAccount = currentAccounts.Last();
                    if (resource != 0)
                    {
                        currentAccount = currentAccounts[resource - 1];
                    }
                    connection = CipherEncryptionUtil.CipherDecrypt(currentAccount);
                }
                Logger.Info($"Current cosmos resource is {resource}.");

                var client = CreateCosmosClient(connection);
                Client = client;
                CosmosClientDic[tenantId] = client;
                Logger.Info($"Succeed connect to cosmos db.");

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initilize cosmos db client object. Error: {e}");
                throw;
            }
        }
    }
}
