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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    public class AzureTableStorageUtility
    {
        private static RALogger mLogger = new RALogger(typeof(AzureTableStorageUtility));


        public static bool CreateAzureTable(string connectionString, params string[] tableNames)
        {
            try
            {
                var serviceClient = AzureUtil.GetServiceClient(connectionString);
                foreach (var tableName in tableNames)
                {
                    //TODO: Need retry?
                    serviceClient.CreateTableIfNotExists(tableName);
                }
            }
            catch
            {
                mLogger.Error("An error occurred while creating azure table.");
                throw;
            }
            return true;
        }

        public static TEntity AddAzureTableEntity<TEntity>(string connectionString, string tableName, TEntity entity) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
                ConvertDateTimePropsToUtc(entity);
                //add entity
                tableClient.UpsertEntity(entity, TableUpdateMode.Merge);
            }
            catch
            {
                mLogger.Error("An error occurred while add entity, rowkey: {0}.", entity.RowKey);
                throw;
            }
            return entity;
        }

        public static IEnumerable<TEntity> AddAzureTableEntities<TEntity>(string connectionString, string tableName, IList<TEntity> entities) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = null;
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.AddAzureTableEntities.GetTableClient"))
                {
                    tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
                }
                //TODO: Need retry?
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.AddAzureTableEntities.ConvertDateTimePropsToUtc"))
                {
                    ConvertDateTimePropsToUtc(entities);
                }
                //Batch add entities
                var batches = new Dictionary<string, TableTransactionAction>();
                foreach (var group in entities.GroupBy(e => e.PartitionKey))
                {
                    DatabaseUtility.BatchOperation(group, (batchItems) => 
                    {
                        tableClient.SubmitTransaction(batchItems.Select(e => new TableTransactionAction(TableTransactionActionType.UpsertMerge, e)));
                    }, 100);
                }
            }
            catch(Exception ex)
            {
                mLogger.Error("An error occurred while add entities.Message:{0}.", ex.ToString());
                throw;
            }
            return entities;
        }

        public static TEntity RetrieveTableEntity<TEntity>(string connectionString, string tableName, string partitionKey, string rowKey) where TEntity : class, ITableEntity, new()
        {
            TEntity entity = default(TEntity);
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName);
                var entities = tableClient.Query<TEntity>(e => e.PartitionKey == partitionKey && e.RowKey == rowKey, 1);
                entity = entities.FirstOrDefault();
            }
            catch
            {
                mLogger.Error("Can't retrieve specified entity. rowkey: {0}.", rowKey);
                throw;
            }
            return entity;
        }

        public static IEnumerable<TEntity> RetrieveTableEntitiesInCondition<TEntity>(string connectionString, string tableName, string conditions) where TEntity : class, ITableEntity, new()
        {
            try
            {
                List<TEntity> entities = new List<TEntity>();
                TableClient tableClient = null;
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.RetrieveTableEntitiesInCondition.GetTableClient"))
                {
                    tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
                }
                var pageableResults = tableClient.Query<TEntity>(conditions);
                foreach (var page in pageableResults.AsPages())
                {
                    entities.AddRange(page.Values);
                }
                return entities;
            }
            catch
            {
                mLogger.Error("Can't retrieve specified entities. conditions: {0}.", conditions);
                throw;
            }
        }

        // Query entities by partition key with optional predicate filter
        public static IEnumerable<TEntity> QueryEntitiesByPartitionKey<TEntity>(string connectionString, string tableName, string partitionKey, Func<TEntity, bool> predicate = null) where TEntity : class, ITableEntity, new()
        {
            List<TEntity> entities = new List<TEntity>();
            TableClient tableClient = null;
            try
            {
                tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
            }
            catch (Exception e)
            {
                mLogger.Error("Can't retrieve specified entities. Exception: {0}.", e);
                throw;
            }

            var pageableResults = tableClient.Query<TEntity>(e => e.PartitionKey == partitionKey);
            foreach (var page in pageableResults.AsPages())
            {
                foreach (var entity in page.Values)
                {
                    if (predicate == null || predicate(entity))
                    {
                        yield return entity;
                    }
                }
            }
        }

        public static TEntity RetrieveTableEntityInCondition<TEntity>(string connectionString, string tableName, string conditions) where TEntity : class, ITableEntity, new()
        {
            TEntity entity = default(TEntity);
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName);
                var entities = tableClient.Query<TEntity>(conditions);
                entity = entities.FirstOrDefault();
            }
            catch
            {
                mLogger.Error("Can't retrieve specified entity. conditions: {0}.", conditions);
                throw;
            }
            return entity;
        }

        public static async IAsyncEnumerable<List<TEntity>> QueryBatchesAsync<TEntity>(string connectionString, string tableName, string filter, int maxPerPage = 1000) where TEntity : class, ITableEntity, new()
        {
            var tableClient = AzureUtil.GetTableClient(connectionString, tableName);

            AsyncPageable<TEntity> queryResults = tableClient.QueryAsync<TEntity>(filter: filter, maxPerPage: maxPerPage);

            await foreach (Page<TEntity> page in queryResults.AsPages())
            {
                if (page.Values.Count > 0)
                {
                    yield return page.Values.ToList();
                }
            }
        }

        public static Response UpdateTableEnity<TEntity>(string connectionString, string tableName, TEntity entity) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName);
                ConvertDateTimePropsToUtc(entity);
                return tableClient.UpdateEntity(entity, ETag.All, TableUpdateMode.Merge);
            }
            catch
            {
                mLogger.Error("Can't update specified entity. rowkey: {0}.", entity.RowKey);
                throw;
            }
        }

        public static async Task<List<Response>> UpdateTableEnitiesAsync<TEntity>(string connectionString, string tableName, IEnumerable<TEntity> entities) where TEntity : class, ITableEntity, new()
        {
            List<Response> results = new List<Response>();
            try
            {
                if (entities == null || entities.Count() <= 0)
                {
                    mLogger.Warn(string.Format("Can't execute empty batch update query."));
                    return results;
                }
                ConvertDateTimePropsToUtc(entities);
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName);
                await DatabaseUtility.BatchOperationAsync(entities, async batchItems => 
                {
                    var batchActions = batchItems.Select(e => new TableTransactionAction(TableTransactionActionType.UpdateMerge, e));
                    results.AddRange((await tableClient.SubmitTransactionAsync(batchActions)).Value);
                }, 100);
            }
            catch
            {
                mLogger.Error("Can't update specified entities in batch.");
                throw;
            }
            return results;
        }

        public static bool DeleteTableEntity<TEntity>(string connectionString, string tableName, TEntity entity) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName);
                tableClient.DeleteEntity(entity.PartitionKey, entity.RowKey);
            }
            catch
            {
                mLogger.Error("Can't delete specified entity.");
                throw;
            }
            return true;
        }

        public static bool DeleteTableEntities<TEntity>(string connectionString, string tableName, IList<TEntity> entities) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = null;
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.DeleteTableEntities.GetTableClient"))
                {
                    tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
                }
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.DeleteTableEntities.ConvertDateTimePropsToUtc"))
                {
                    ConvertDateTimePropsToUtc(entities);
                }
                DatabaseUtility.BatchOperation(entities, batchItems =>
                {
                    var batchActions = batchItems.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e));
                    tableClient.SubmitTransaction(batchActions);
                }, 100);
            }
            catch
            {
                mLogger.Error("An error occurred while delete specified entities.");
                throw;
            }
            return true;
        }

        public static bool DeleteTableEntitiesWithCondition<TEntity>(string connectionString, string tableName, string conditionQuery) where TEntity : class, ITableEntity, new()
        {
            try
            {
                TableClient tableClient = AzureUtil.GetTableClient(connectionString, tableName, true);
                var entities = RetrieveTableEntitiesInCondition<TEntity>(connectionString, tableName, conditionQuery);
                if (entities.Count() == 0)
                {
                    mLogger.Warn("No object to delete in azure table {0}", conditionQuery);
                    return false;
                }
                ConvertDateTimePropsToUtc(entities);
                DatabaseUtility.BatchOperation(entities, batchItems =>
                {
                    var batchActions = batchItems.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e));
                    tableClient.SubmitTransaction(batchActions);
                }, 100);
            }
            catch(Exception ex)
            {
                mLogger.Error("An error occurred while delete entities by condition: {0}.{1}", conditionQuery,ex.ToString());
                throw;
            }
            return true; ;
        }

        #region convert datetime properties to utc datetime
        private static void ConvertDateTimePropsToUtc<TEntity>(IEnumerable<TEntity> items) where TEntity : class, ITableEntity, new()
        {
            if (items.Count() == 0)
            {
                return;
            }
            var type = items.First().GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    foreach (var item in items)
                    {
                        ConvertDateTimePropToUtc(item, prop);
                    }
                }
            }
        }

        private static void ConvertDateTimePropsToUtc<TEntity>(TEntity item) where TEntity : class, ITableEntity, new()
        {
            var type = item.GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    ConvertDateTimePropToUtc(item, prop);
                }
            }
        }

        private static void ConvertDateTimePropToUtc<TEntity>(TEntity item, PropertyInfo prop) where TEntity : class, ITableEntity, new()
        {
            var dt = (DateTime)prop.GetValue(item);
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                prop.SetValue(item, dt);
            }
            else if (dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
                prop.SetValue(item, dt);
            }
        }
        #endregion
    }
}
