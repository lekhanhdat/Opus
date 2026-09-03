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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.StorageTable
{
    public class TableStorageService
    {
        private static AveLogger logger = new AveLogger(typeof(TableStorageService));
        private CloudTableClient tableClient;

        public TableStorageService(string accountname, string accountKey, string endPoint)
        {
            string connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={2}", accountname, accountKey, endPoint);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            tableClient = storageAccount.CreateCloudTableClient();
        }

        public bool CheckTable(string tableName)
        {
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                logger.Error("Check azure table failed: {0}.", ex.ToString());
                return false;
            }
            return true;
        }
        private void BatchInsert<TEntity>(CloudTable table, int startIndex, int endIndex, IList<TEntity> entities) where TEntity : ITableEntity, new()
        {
            TableBatchOperation batchOperations = new TableBatchOperation();
            for (int index = startIndex; index < endIndex; index++)
            {
                batchOperations.InsertOrMerge(entities[index]);
            }
            IList<TableResult> results = table.ExecuteBatch(batchOperations);
        }

        public IEnumerable<TEntity> AddAzureTableEntities<TEntity>(string tableName, IList<TEntity> entities) where TEntity : ITableEntity, new()
        {
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                int index, startIndex, endIndex;
                for (index = 0; index < entities.Count / 100; index++)
                {
                    startIndex = index * 100;
                    endIndex = index * 100 + 100;
                    BatchInsert(table, startIndex, endIndex, entities);
                }
                startIndex = index * 100;
                endIndex = index * 100 + entities.Count % 100;
                BatchInsert(table, startIndex, endIndex, entities);
            }
            catch (Exception ex)
            {
                logger.Error("Batch add table entity failed: {0}.", ex.ToString());
            }
            return entities;
        }

        public IEnumerable<TEntity> RetrieveTableEntity<TEntity>(string tableName, TableQuery<TEntity> query) where TEntity : ITableEntity, new()
        {
            IEnumerable<TEntity> entities = null;
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                entities = table.ExecuteQuery<TEntity>(query);
            }
            catch (Exception ex)
            {
                logger.Warn("Retrieve query entity failed: {0}.", ex.ToString());
            }
            return entities;
        }

        public IEnumerable<TEntity> RetrieveTableEntitiesInCondition<TEntity>(string tableName, string conditions) where TEntity : ITableEntity, new()
        {
            IEnumerable<TEntity> entities = null;
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                if (table == null)
                {
                    logger.Warn("table {0} is null", tableName);
                    return entities;
                }
                if (!table.Exists())
                {
                    logger.Warn("table {0} not exists", tableName);
                    return entities;
                }
                TableQuery<TEntity> query = new TableQuery<TEntity>().Where(conditions);
                entities = table.ExecuteQuery<TEntity>(query);
            }
            catch (Exception ex)
            {
                logger.Warn("Retrieve condition entity failed: {0}.", ex.ToString());
            }
            return entities;
        }

        public void UpdateTableEnities<TEntity>(string tableName, IEnumerable<TEntity> entities) where TEntity : ITableEntity, new()
        {
            IEnumerable<TableResult> results = null;
            try
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                TableBatchOperation operationCollection = new TableBatchOperation();
                foreach (var entity in entities)
                {
                    operationCollection.InsertOrMerge(entity);
                }
                results = table.ExecuteBatch(operationCollection);
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Batch update table entities failed: {0}", ex.ToString()));
            }
        }

        public string CombineFilters(string filterA, string operatorString, string filterB)
        {
            return TableQuery.CombineFilters(filterA, operatorString, filterB);
        }

        public string GenerateFilterCondition(string propertyName, string operation, string givenValue)
        {
            return TableQuery.GenerateFilterCondition(propertyName, operation, givenValue);
        }

        public string GenerateFilterConditionforInt(string propertyName, string operation, int givenValue)
        {
            return TableQuery.GenerateFilterConditionForInt(propertyName, operation, givenValue);
        }

        public string GenerateFilterConditionForLong(string propertyName, string operation, long givenValue)
        {
            return TableQuery.GenerateFilterConditionForLong(propertyName, operation, givenValue);
        }
    }

    public static class QueryComparisons
    {
        public const string Equal = "eq";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "ge";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "le";
        public const string NotEqual = "ne";
    }

    public static class TableOperators
    {
        public const string And = "and";
        public const string Not = "not";
        public const string Or = "or";
    }
}
