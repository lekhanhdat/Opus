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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Teams;
using AvePoint.RA.DB.Core;
using Azure;
using Azure.Data.Tables;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TeamsChannelConflictSettingDao : ITeamsChannelConflictSettingDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsChannelConflictSettingDao));

        private const string TablePrefix = "RECOConflictChannelSetting";

        private string ConnectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
        public IEnumerable<TeamsChannelConflictSetting> AddTeamsChannelConflictSettings(string tenantGroupId, List<TeamsChannelConflictSetting> entities)
        {
            string tableName = GetTableName(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<TeamsChannelConflictSetting>(ConnectionString, tableName, entities);
            return mEntities;
        }

        public IEnumerable<TeamsChannelConflictSetting> GetTeamsChannelConflictSettings(string tenantGroupId, ModuleType moduleType, int pageSize, int pageIndex)
        {
            string tableName = GetTableName(tenantGroupId);
            AzureTableQueryConditionBuilder ModuleTypeFilter = new AzureTableQueryConditionBuilder();
            ModuleTypeFilter.AppendOrQuery("ModuleType", AzureQueryComparisons.Equal, moduleType.ToString(), AzureDataType.String);
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<TeamsChannelConflictSetting>(ConnectionString, tableName, ModuleTypeFilter.ToString())
                .Skip(pageSize * pageIndex)
                .Take(pageSize);
            return result;
        }

        public IEnumerable<TeamsChannelConflictSetting> GetAllTeamsConflictChannelSettings(string tenantGroupId, ModuleType moduleType)
        {
            string tableName = GetTableName(tenantGroupId);
            AzureTableQueryConditionBuilder filter = new AzureTableQueryConditionBuilder();
            filter.AppendOrQuery("ModuleType", AzureQueryComparisons.Equal, moduleType.ToString(), AzureDataType.String);
            filter.AppendOrQuery("IsConflict", AzureQueryComparisons.Equal, true, AzureDataType.Bool);
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<TeamsChannelConflictSetting>(ConnectionString, tableName, filter.ToString());
            return result;
        }

        public TeamsChannelConflictQueryResult GetTeamsChannelConflictSettingWithTotal(string tenantGroupId, ModuleType moduleType, int pageSize, int pageIndex, string sortBy, bool isAscending)
        {
            string tableName = GetTableName(tenantGroupId);
            AzureTableQueryConditionBuilder ModuleTypeFilter = new AzureTableQueryConditionBuilder();
            ModuleTypeFilter.AppendOrQuery("ModuleType", AzureQueryComparisons.Equal, moduleType.ToString(), AzureDataType.String);
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<TeamsChannelConflictSetting>(ConnectionString, tableName, ModuleTypeFilter.ToString());
            string normalizedSort = sortBy?.Trim()?.ToLowerInvariant();
            IEnumerable<TeamsChannelConflictSetting> orderedResult;
            switch (normalizedSort)
            {
                case "isconflict":
                    orderedResult = isAscending
                        ? result.OrderBy(r => r.IsConflict)
                        : result.OrderByDescending(r => r.IsConflict);
                    break;
                case null:
                case "":
                case "fullpath":
                default:
                    orderedResult = isAscending
                        ? result.OrderBy(r => r.FullPath ?? string.Empty)
                        : result.OrderByDescending(r => r.FullPath ?? string.Empty);
                    break;
            }

            var settingReuslt = new TeamsChannelConflictQueryResult()
            {
                Settings = orderedResult.Skip(pageSize * pageIndex).Take(pageSize).ToList(),
                TotalCount = result.Count(),
            };

            return settingReuslt;
        }

        public async Task<bool> DeleteAllTeamsChannelConflictSettings(string tenantGroupId)
        {
            string tableName = GetTableName(tenantGroupId);
            TableClient tableClient = AzureUtil.GetTableClient(ConnectionString, tableName, false);

            try
            {
                bool tableExists = await TableExistsAsync(tableClient);
                if (!tableExists)
                {
                    Console.WriteLine($"Table {tableName} does not exist. No deletion needed.");
                    return true;
                }

                await DeleteEntitiesInBatchesAsync(tableClient);
                return true;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "TableNotFound")
            {
                Console.WriteLine($"Table {tableName} not found. Error: {ex.Message}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting entities from {tableName}: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> TableExistsAsync(TableClient tableClient)
        {
            try
            {
                var queryResponse = tableClient.QueryAsync<TableEntity>(
                    filter: string.Empty,
                    maxPerPage: 1,
                    select: new[] { "PartitionKey" });

                bool exist = false;
                await foreach (var entity in queryResponse)
                {
                    exist = true;
                }
                return exist;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "TableNotFound" || ex.ErrorCode == "ResourceNotFound")
            {
                logger.Debug("Table does not exist: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn("Error checking table existence, assuming table exists: {0}", ex.Message);
                return true;
            }
        }

        private async Task DeleteEntitiesInBatchesAsync(TableClient tableClient)
        {
            const int batchSize = 100; // Azure Table batch operation limit
            var cancellationToken = default(CancellationToken);

            var partitionKeys = await GetDistinctPartitionKeysAsync(tableClient);

            foreach (var partitionKey in partitionKeys)
            {
                await DeleteEntitiesByPartitionAsync(tableClient, partitionKey, batchSize, cancellationToken);
            }
        }

        private async Task<List<string>> GetDistinctPartitionKeysAsync(TableClient tableClient)
        {
            var partitionKeys = new List<string>();
            var query = tableClient.QueryAsync<TableEntity>(
                select: new[] { "PartitionKey" });

            await foreach (var entity in query)
            {
                partitionKeys.Add(entity.PartitionKey);
            }
            return partitionKeys.Distinct().ToList();
        }

        private async Task DeleteEntitiesByPartitionAsync(
            TableClient tableClient,
            string partitionKey,
            int batchSize,
            CancellationToken cancellationToken)
        {
            var batch = new List<TableTransactionAction>();
            var query = tableClient.QueryAsync<TableEntity>(
                filter: $"PartitionKey eq '{partitionKey}'",
                cancellationToken: cancellationToken);

            await foreach (var entity in query)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));

                if (batch.Count >= batchSize)
                {
                    await tableClient.SubmitTransactionAsync(batch, cancellationToken);
                    batch.Clear();
                }
            }

            // Submit remaining entities
            if (batch.Count > 0)
            {
                await tableClient.SubmitTransactionAsync(batch, cancellationToken);
            }
        }

    }
}
