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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Task;

namespace AvePoint.RA.Service.RMTasks;

public class DBPoolTaskExecutor : ITaskExecutor
{
    private RALogger logger = RALogger.GetInstance(typeof(DBPoolTaskExecutor));

    public async Task ExecutorAsync(TaskBase task)
    {
        await RunPoolJob();
    }
    public int MaxDatabaseInPool
    {
        get
        {
            if (RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.MAX_DATABASE_INPOOL].IsNullOrEmpty())
            {
                return DBPoolTaskExecutorConstants.MaxDatabaseInPool;
            }
            return Convert.ToInt32(RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.MAX_DATABASE_INPOOL]);
        }

    }

    private async Task RunPoolJob()
    {
        var sqlServers = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.SQL_SERVER_FOR_ELASTICPOOL];

        if (sqlServers.IsNullOrEmpty())
        {
            logger.Warn("No sql servers for pool job");
            return;
        }
        logger.Info("start to run pool job");
        var subscriptionId = RMGlobalConfiguration.AppConfig.SubscriptionId;
        foreach (var server in sqlServers.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                logger.Info($"Start to process server {server}");
                var temp = server.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var databases = AzureSqlManagementService.GetDatabasesByServer(subscriptionId, temp[0], temp[1], TokenProvider.GetResourceManagerToken());
                var allBasicDatabases = databases.Where(item => (item.Sku.Name.EqualsIgnoreCase("Basic") || (item.Sku.Name.EqualsIgnoreCase("Standard") && item.Sku.Capacity == 10)) && item.Name.StartsWith("reco_discovery")).ToList();
                if (allBasicDatabases.Count == 0)
                {
                    logger.Info("no need to move databases");
                    continue;
                }
                logger.Info($"There are some databases need to be moved. Start to find available elastic pools");
                List<String> needExcludePoolNames = new List<String>();
                var availablePoolName = this.FindAvailablePool(needExcludePoolNames, subscriptionId, temp[0], temp[1], databases[0].Location, out Int32 currentPoolCount);
                foreach (var item in allBasicDatabases)
                {
                    try
                    {
                        logger.Info($"move database {item.Name}");
                        if (currentPoolCount == MaxDatabaseInPool)
                        {
                            needExcludePoolNames.Add(availablePoolName);
                            availablePoolName = this.FindAvailablePool(needExcludePoolNames, subscriptionId, temp[0], temp[1], databases[0].Location, out currentPoolCount);
                        }
                        AzureSqlManagementService.MoveDatabaseToElasticPool(subscriptionId, temp[0], temp[1], item.Name, TokenProvider.GetResourceManagerToken(), new AzureResourceModels.UpdateDatabaseRequest()
                        {
                            Properties = new AzureResourceModels.DatabaseProperties()
                            {
                                ElasticPoolId = $"/subscriptions/{subscriptionId}/resourceGroups/{temp[0]}/providers/Microsoft.Sql/servers/{temp[1]}/elasticPools/{availablePoolName}",
                            }
                        });
                        logger.Info($"finish to move database {item.Name} to {availablePoolName}");
                        currentPoolCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while moving database {item.Name}. {ex}");
                    }
                }
                //sleep 10 minutes to wait for the move operation to complete
                logger.Info("All databases have been moved. Sleep 10 minutes to wait for the move operation to complete.");
                await Task.Delay(TimeSpan.FromMinutes(10));

            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while moving databases in DB server {server}. {ex}");
            }
        }
        return;
    }
    private Int32 GetElasticPoolSuffix(String poolName)
    {
        if (poolName.StartsWith(DBPoolTaskExecutorConstants.ElasticPoolNamePrefix))
        {
            var tempStr = poolName.Substring(DBPoolTaskExecutorConstants.ElasticPoolNamePrefix.Length);
            if (Int32.TryParse(tempStr, out int result))
            {
                return result;
            }
        }
        return 0;
    }

    private String FindAvailablePool(List<String> needExcludePoolNames,
        String subscriptionId,
        String resourceGroup,
        String serverName,
        String location,
        out Int32 currentPoolCount)
    {
        currentPoolCount = 0;
        var elasticpoolsInAzure = AzureSqlManagementService.GetElasticPoolsByServer(subscriptionId, resourceGroup, serverName, TokenProvider.GetResourceManagerToken());
        var elasticpools = elasticpoolsInAzure.Where(item => !needExcludePoolNames.Contains(item.Name)).ToList();
        var availablePoolName = String.Empty;

        foreach (var pool in elasticpools)
        {
            var poolDatabases = AzureSqlManagementService.GetDatabasesByPool(subscriptionId, resourceGroup, serverName, pool.Name, TokenProvider.GetResourceManagerToken());
            if (poolDatabases.Count < MaxDatabaseInPool)
            {
                availablePoolName = pool.Name;
                currentPoolCount = poolDatabases.Count;
                break;
            }
        }
        if (availablePoolName.IsNullOrEmpty())
        {
            logger.Info("can not find available pool, need to create a new one");
            var newPoolName = String.Empty;
            if (elasticpoolsInAzure.Count == 0)
            {
                newPoolName = DBPoolTaskExecutorConstants.ElasticPoolNamePrefix + 1;
            }
            else
            {
                var maxNamePrefix = elasticpoolsInAzure.Select(item => GetElasticPoolSuffix(item.Name)).Max();
                newPoolName = DBPoolTaskExecutorConstants.ElasticPoolNamePrefix + (maxNamePrefix + 1);
            }
            logger.Info($"the new pool name is {newPoolName}");
            this.CreateNewElasticPool(subscriptionId, resourceGroup, serverName, location, newPoolName);
            availablePoolName = newPoolName;
            currentPoolCount = 0;
        }
        return availablePoolName;
    }
    private void CreateNewElasticPool(String subscriptionId,
            String resourceGroup,
            String serverName,
            String location,
            String newElasticPoolName)
    {
        var request = new AzureResourceModels.CreateElasticPoolRequest()
        {
            Location = location,
            Sku = new AzureResourceModels.Sku()
            {
                Name = "StandardPool",
                Tier = "Standard",
                Capacity = 50
            },
            Properties = new AzureResourceModels.ElasticPoolProperties()
            {
                PerDatabaseSettings = new AzureResourceModels.ElasticPoolPerDatabaseSettings()
                {
                    MaxCapacity = 10
                }
            }
        };
        AzureSqlManagementService.CreateElasticPool(subscriptionId, resourceGroup, serverName, newElasticPoolName, TokenProvider.GetResourceManagerToken(), request);
        var hasCreated = false;
        do
        {
            var elasticpools = AzureSqlManagementService.GetElasticPoolsByServer(subscriptionId, resourceGroup, serverName, TokenProvider.GetResourceManagerToken());
            hasCreated = elasticpools.Find(item => item.Name.EqualsIgnoreCase(newElasticPoolName) && item.Properties.State.EqualsIgnoreCase("Ready")) != null;
            Thread.Sleep(2000);
        } while (!hasCreated);
    }

}
