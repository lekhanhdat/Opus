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
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Microsoft.Data.OData.Query.SemanticAst;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SyncFailureItemDao : ISyncFailureItemDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(RecordsHistoryTableDao));
        private const string TablePrefix = "RECODataSyncFailure";
        private const string DataSourceConditionStr = "DataSource";
        private const string SortTicksConditionStr = "SortTicks";
        private string connectionString
        {
            get
            {
                return RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            }
        }
        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
        public bool Add(string tenantGroupId, List<SyncFailureItemEntity> entities)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                var mEntities = AzureTableStorageUtility.AddAzureTableEntities<SyncFailureItemEntity>(connectionString, tableName, entities);
                logger.Debug("Add failure item count {0}", mEntities?.Count());
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }
        }

        public List<SyncFailureItemEntity> GetAll(string tenantGroupId, string siteId)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                string partionCondition = new AzureTableQueryConditionBuilder(siteId).ToString();
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, partionCondition.ToString())
                   .OrderByDescending(e => e.Timestamp).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }

        public List<SyncFailureItemEntity> GetAllByDataSource(string tenantGroupId, string siteId, int dataSource)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                string partionCondition = new AzureTableQueryConditionBuilder(siteId).ToString();
                //AzureTableQueryConditionBuilder dataSourceCondition = new AzureTableQueryConditionBuilder();
                //dataSourceCondition.AppendOrQuery("DataSource", "eq", dataSource);
                string dataSourceCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(DataSourceConditionStr, AzureQueryComparisons.Equal, dataSource, AzureDataType.Int);
                string query = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition.ToString(), dataSourceCondition.ToString()).ToString();
                logger.Debug(query);
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, query)
                   .OrderByDescending(e => e.Timestamp).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }

        public List<SyncFailureItemEntity> GetAllByDataSource(string tenantGroupId, string siteId, string listId, int dataSource)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                AzureTableQueryConditionBuilder partionCondition = new AzureTableQueryConditionBuilder(siteId);
                AzureTableQueryConditionBuilder listCondition = new AzureTableQueryConditionBuilder();
                listCondition.AppendOrQuery("ListId", "eq", listId);
                string query = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition.ToString(), listCondition.ToString()).ToString();
                string dataSourceCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(DataSourceConditionStr, AzureQueryComparisons.Equal, dataSource, AzureDataType.Int);
                query = AzureTableQueryConditionBuilder.CombineAndQueries(query, dataSourceCondition.ToString()).ToString();
                logger.Debug(query);
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, query)
                   .OrderByDescending(e => e.Timestamp).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }

        public List<SyncFailureItemEntity> GetDataByPage(string tenantGroupId, string siteId, int dataSource, long sortTicks, int pageSize)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                AzureTableQueryConditionBuilder partionCondition = new AzureTableQueryConditionBuilder(siteId);
                string dataSourceCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(DataSourceConditionStr, AzureQueryComparisons.Equal, dataSource, AzureDataType.Int);
                string query = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition.ToString(), dataSourceCondition.ToString()).ToString();
                string sortTicksCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(SortTicksConditionStr, AzureQueryComparisons.GreaterThan, sortTicks, AzureDataType.Long);
                query = AzureTableQueryConditionBuilder.CombineAndQueries(query, sortTicksCondition.ToString()).ToString();
                logger.Debug(query);
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, query)
                   .OrderBy(e => e.SortTicks).Take(pageSize).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }

        public List<SyncFailureItemEntity> GetDataByPage(string tenantGroupId, string siteId, string listId,int dataSource, long sortTicks, int pageSize)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                AzureTableQueryConditionBuilder partionCondition = new AzureTableQueryConditionBuilder(siteId);
                string dataSourceCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(DataSourceConditionStr, AzureQueryComparisons.Equal, dataSource, AzureDataType.Int);
                string query = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition.ToString(), dataSourceCondition.ToString()).ToString();
                string sortTicksCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(SortTicksConditionStr, AzureQueryComparisons.GreaterThan, sortTicks, AzureDataType.Long);
                query = AzureTableQueryConditionBuilder.CombineAndQueries(query, sortTicksCondition.ToString()).ToString();
                AzureTableQueryConditionBuilder listCondition = new AzureTableQueryConditionBuilder();
                listCondition.AppendOrQuery("ListId", "eq", listId);
                query = AzureTableQueryConditionBuilder.CombineAndQueries(query, listCondition.ToString()).ToString();
                logger.Debug(query);
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, query)
                   .OrderBy(e => e.SortTicks).Take(pageSize).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }

        public List<SyncFailureItemEntity> GetAll(string tenantGroupId, string siteId, string listId)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                AzureTableQueryConditionBuilder partionCondition = new AzureTableQueryConditionBuilder(siteId);
                AzureTableQueryConditionBuilder listCondition = new AzureTableQueryConditionBuilder();
                listCondition.AppendOrQuery("ListId", "eq", listId);
                string query = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition.ToString(), listCondition.ToString()).ToString();
                logger.Debug(query);
                var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<SyncFailureItemEntity>(connectionString, tableName, query)
                   .OrderByDescending(e => e.Timestamp).ToList();
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return new List<SyncFailureItemEntity>();
            }
        }
        public bool Remove(string tenantGroupId, SyncFailureItemEntity entity)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                return AzureTableStorageUtility.DeleteTableEntity<SyncFailureItemEntity>(connectionString, tableName, entity);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }
        }
        public bool RemoveAll(string tenantGroupId, string siteId)
        {
            try
            {
                string tableName = GetTableName(tenantGroupId);
                string partionCondition = new AzureTableQueryConditionBuilder(siteId).ToString();
                return AzureTableStorageUtility.DeleteTableEntitiesWithCondition<SyncFailureItemEntity>(connectionString, tableName, partionCondition.ToString());
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }
        }

    }
}
