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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model.DisposalStub;
using Azure;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.DisposalStubDao.Impl
{
    public class RMStubFileRecordDao : IRMStubFileRecordDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMStubFileRecordDao));

        private IRMRuleDao mRMRuleDao;
        protected IRMRuleDao RMRuleDao
        {
            get
            {
                if (mRMRuleDao == null)
                {
                    mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
                }
                return mRMRuleDao;
            }
        }

        private IJobMonitorDao mJobMonitorDao;
        protected IJobMonitorDao JobMonitorDao
        {
            get
            {
                if (mJobMonitorDao == null)
                {
                    mJobMonitorDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
                }
                return mJobMonitorDao;
            }
        }

        private const string TablePrefix = "RMStubFileRecords";
        private string ConnectionString => RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private string GetTableName(string tenantGroupId) => string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));

        public void AddStubFileRecordEntity(string tenantGroupId, RMStubFileRecordTableEntity entity)
        {
            var rowKeyRefs = entity.RowKey.Split('_');
            var indexRecord = new RMStubFileRecordTableEntity(entity.PartitionKey, rowKeyRefs[1])
            {
                RefDateTimeStr = rowKeyRefs[0],
                RecordType = 1, // index record
                //StubId = entity.StubId
            };
            //AzureTableStorageUtility.AddAzureTableEntity(ConnectionString, GetTableName(tenantGroupId), entity);
            AzureTableStorageUtility.AddAzureTableEntities(ConnectionString, GetTableName(tenantGroupId), [entity, indexRecord]);
        }

        public void AddStubFileRecordEntities(string tenantGroupId, List<RMStubFileRecordTableEntity> entities)
        {
            if (entities == null || entities.Count == 0)
            {
                return;
            }
            var indexRecords = entities.Select(entity =>
            {
                var rowKeyRefs = entity.RowKey.Split('_');
                return new RMStubFileRecordTableEntity(entity.PartitionKey, rowKeyRefs[1])
                {
                    RefDateTimeStr = rowKeyRefs[0],
                    RecordType = 1, // index record
                    //StubId = entity.StubId
                };
            }).ToList();

            entities.AddRange(indexRecords);

            AzureTableStorageUtility.AddAzureTableEntities(ConnectionString, GetTableName(tenantGroupId), entities);
        }

        public void DeleteStubFileRecordEntities(string tenantGroupId, List<RMStubFileRecordTableEntity> indexEntities)
        {
            if (indexEntities == null || indexEntities.Count == 0)
            {
                return;
            }

            try
            {
                AzureTableStorageUtility.DeleteTableEntities(ConnectionString, GetTableName(tenantGroupId), indexEntities);
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                }
                else if (re.ErrorCode == "ResourceNotFound")
                {
                    logger.Warn($"Batch delete failed, Status: {re.Status}, Reason: {re.Message}. Triggering fallback to individual deletion for {indexEntities.Count} items.");
                    DeleteSingleEntity4Batch(tenantGroupId, indexEntities);
                }
                else
                {
                    logger.Error($"Failed to delete StubFileRecord entities for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to delete StubFileRecord entities for tenantGroupId: {tenantGroupId}. Exception: {e}");
            }
        }

        private void DeleteSingleEntity4Batch(string tenantGroupId, List<RMStubFileRecordTableEntity> indexEntities)
        {
            foreach (var item in indexEntities)
            {
                try
                {
                    AzureTableStorageUtility.DeleteTableEntity(ConnectionString, GetTableName(tenantGroupId), item);
                }
                //catch (RequestFailedException singleEx) when (singleEx.Status == 404)
                //{
                //    logger.Info($"[Fallback] Item not found (PK: {item.PartitionKey}, RK: {item.RowKey}). Skipped.");
                //}
                catch (Exception singleEx)
                {
                    logger.Error($"[Fallback] Failed to delete item (PK: {item.PartitionKey}, RK: {item.RowKey}). Exception: {singleEx.Message}");
                }
            }
        }

        private static ConcurrentDictionary<string, Dictionary<string, RMStubFileRecordTableEntity>> deleteEntityCache = [];

        public void DeleteStubFileRecordEntitiesInBatch(string tenantGroupId, string siteId, string archivedItemId)
        {
            var tableName = GetTableName(tenantGroupId);
            try
            {
                archivedItemId = archivedItemId.Replace("-", string.Empty);
                if (deleteEntityCache.TryGetValue(siteId, out var deleteEntities))
                {
                    lock (deleteEntities)
                    {
                        if (deleteEntities.Count > 0 && deleteEntities.ContainsKey(archivedItemId))
                        {
                            logger.Info($"Record for siteId: {siteId}, archivedItemId: {archivedItemId} already exists in delete cache. Skip add deletion.");
                            return;
                        }
                    }
                }
                
                var indexRecord = AzureTableStorageUtility.RetrieveTableEntity<RMStubFileRecordTableEntity>(ConnectionString, tableName, siteId, archivedItemId);
                if (indexRecord == null)
                {
                    logger.Warn($"Cannot find index record for PartitionKey: {siteId}, RowKey: {archivedItemId}. Skip deletion.");
                    return;
                }

                indexRecord.ETag = ETag.All;
                //AddIndexEntityCache(siteId, indexRecord);

                var mainRowKey = $"{indexRecord.RefDateTimeStr}_{indexRecord.RowKey}";
                var mainIndex = new RMStubFileRecordTableEntity(siteId, mainRowKey)
                {
                    ETag = ETag.All,
                    RecordType = 0
                };

                //AddIndexEntityCache(siteId, mainIndex);

                var currentSiteList = deleteEntityCache.GetOrAdd(siteId, _ => []);
                List<RMStubFileRecordTableEntity> batchToDelete = null;

                lock (currentSiteList)
                {
                    if (!currentSiteList.ContainsKey(archivedItemId))
                    {
                        currentSiteList[indexRecord.RowKey] = indexRecord;
                        currentSiteList[mainIndex.RowKey] = mainIndex;

                        if (currentSiteList.Count >= 100)
                        {
                            batchToDelete = currentSiteList.Values.ToList();
                            currentSiteList.Clear();
                        }
                    }
                }

                if (batchToDelete != null && batchToDelete.Count > 0)
                {
                    try
                    {
                        logger.Info($"Batch size reached 100. Deleting records for site: {siteId}");
                        AzureTableStorageUtility.DeleteTableEntities(ConnectionString, GetTableName(tenantGroupId), batchToDelete);
                        logger.Info($"Successfully deleted batch for site: {siteId}");
                    }
                    catch (RequestFailedException re)
                    {
                        if (re.ErrorCode == "TableNotFound")
                        {
                            logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                        }
                        else if (re.ErrorCode == "ResourceNotFound")
                        {
                            logger.Warn($"Batch delete failed, Status: {re.Status}, Reason: {re.Message}. Triggering fallback to individual deletion for {batchToDelete.Count} items.");
                            DeleteSingleEntity4Batch(tenantGroupId, batchToDelete);
                        }
                        else
                        {
                            logger.Error($"Failed to delete StubFileRecord entities for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to execute batch delete for site: {siteId}. Exception: {ex}");
                    }
                }
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                }
                else
                {
                    logger.Error($"Failed to DeleteStubFileRecordEntitiesInBatch for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to DeleteStubFileRecordEntitiesInBatch for tenantGroupId: {tenantGroupId}. Exception: {e}");
            }
        }

        private void AddIndexEntityCache(string siteId, RMStubFileRecordTableEntity value)
        {
            var indexEntityList = deleteEntityCache.GetOrAdd(siteId, _ => []);

            lock (indexEntityList)
            {
                indexEntityList[value.RowKey] = value;
            }
        }

        public void FlushDeleteCache(string tenantGroupId)
        {
            if (deleteEntityCache.IsEmpty) return;

            logger.Info($"Start flushing delete cache for tenantGroupId: {tenantGroupId}, total sites: {deleteEntityCache.Count}");
            var tableName = GetTableName(tenantGroupId);
            foreach (var kvp in deleteEntityCache)
            {
                var siteList = kvp.Value;
                List<RMStubFileRecordTableEntity> batchToDelete = null;

                lock (siteList)
                {
                    if (siteList == null || siteList.Count == 0) continue;
                    batchToDelete = siteList.Values.ToList();
                    siteList.Clear();
                }

                if (batchToDelete != null && batchToDelete.Count > 0)
                {
                    try
                    {
                        logger.Info($"Flushing delete cache for site: {kvp.Key}, batch size: {batchToDelete.Count}");
                        AzureTableStorageUtility.DeleteTableEntities(ConnectionString, tableName, batchToDelete);
                        logger.Info($"Successfully flushed delete cache for site: {kvp.Key}");
                    }
                    catch (RequestFailedException re)
                    {
                        if (re.ErrorCode == "TableNotFound")
                        {
                            logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                        }
                        else if (re.ErrorCode == "ResourceNotFound")
                        {
                            logger.Warn($"Batch delete failed, Status: {re.Status}, Reason: {re.Message}. Triggering fallback to individual deletion for {batchToDelete.Count} items.");
                            DeleteSingleEntity4Batch(tenantGroupId, batchToDelete);
                        }
                        else
                        {
                            logger.Error($"Failed to delete StubFileRecord entities for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Failed to flush cache for site: {kvp.Key}. E: {e}");
                    }
                }
            }
        }

        public async IAsyncEnumerable<List<RMStubFileRecordTableEntity>> QueryMainRecByRetTimeBatchesAsync(string tenantGroupId, string partitionKey, DateTime cutoffTime)
        {
            if (string.IsNullOrEmpty(tenantGroupId)) throw new ArgumentNullException(nameof(tenantGroupId));
            if (string.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));

            string tableName = GetTableName(tenantGroupId);

            string finalFilter = string.Empty;

            using (var builder = new AzureTableQueryConditionBuilder(partitionKey))
            {
                string timeRowKeyStr = $"{cutoffTime.AddSeconds(1):yyyyMMddHHmmss}"; // +1 second to make the filter exclusive, as RowKey is in format of RefTimeTicks_rowkey lead to cannot query rowkey by equal operator.
                builder.AppendAndQuery("RowKey", AzureQueryComparisons.LessThan, timeRowKeyStr);
                builder.AppendAndQuery("RecordType", AzureQueryComparisons.Equal, 0);
                finalFilter = builder.ToString();
            }

            IAsyncEnumerable<List<RMStubFileRecordTableEntity>> batchStream = null;

            try
            {
                batchStream = AzureTableStorageUtility.QueryBatchesAsync<RMStubFileRecordTableEntity>(ConnectionString, tableName, finalFilter);
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                }
                else
                {
                    logger.Error($"Failed to get StubFileRecord for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to get StubFileRecord for tenantGroupId: {tenantGroupId}. Exception: {e}");
            }

            if (batchStream == null)
            {
                yield break;
            }

            await foreach (var batch in batchStream)
            {
                yield return batch;
            }
        }

        public async Task<RMStubFileRecordTableEntity> GetFirstRecordAfterTimeAsync(string tenantGroupId, string partitionKey, Guid stubTemplateId, DateTime cutOffTime)
        {
            try
            {
                var tableName = GetTableName(tenantGroupId);
                string finalFilter = string.Empty;

                using (var builder = new AzureTableQueryConditionBuilder(partitionKey))
                {
                    string timeRowKeyStr = $"{cutOffTime:yyyyMMddHHmmss}";
                    builder.AppendAndQuery("RowKey", AzureQueryComparisons.GreaterThan, timeRowKeyStr);
                    builder.AppendAndQuery("RecordType", AzureQueryComparisons.Equal, 0);
                    builder.AppendAndQuery("StubTemplateId", AzureQueryComparisons.Equal, stubTemplateId.ToString());
                    finalFilter = builder.ToString();
                }

                var result = AzureTableStorageUtility.RetrieveTableEntityInCondition<RMStubFileRecordTableEntity>(ConnectionString, tableName, finalFilter);

                return result;
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"StubFileRecord table for tenantGroupId: {tenantGroupId} not found. Skip process");
                }
                else
                {
                    logger.Error($"Failed to get StubFileRecord for tenantGroupId: {tenantGroupId}. RequestFailedException: {re}");
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting first record after time for template {stubTemplateId}. PartitionKey: {partitionKey}, CutOff: {cutOffTime}. Error: {ex}");
                return null;
            }
        }
    }
}
