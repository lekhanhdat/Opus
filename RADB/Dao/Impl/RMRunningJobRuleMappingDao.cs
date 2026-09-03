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
using AngleSharp.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Core;
using Azure;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRunningJobRuleMappingDao : IRMRunningJobRuleMappingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRunningJobRuleMappingDao));
        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private const string DisableAddJobRuleMappingsKey = "DisableAddJobRuleMappings";

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

        private const string TablePrefix = "RMRunningJobRuleMapping";
        private string ConnectionString => RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private string GetTableName(string tenantGroupId) => string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));

        private static ConcurrentDictionary<string, bool> _jobVEORuleMapping = new ConcurrentDictionary<string, bool>();

        public void AddJobMappingsForVEOMerge(string tenantGroupId, string jobId)
        {
            var cacheKey = $"{tenantGroupId}:{jobId}";
            if (_jobVEORuleMapping.TryGetValue(cacheKey, out var hasVEORule) && hasVEORule) return;
            var updateEntity = new RMRunningJobRuleMappingTableEntity(tenantGroupId, jobId)
            {
                HasVEORule = true
            };
            try
            {
                AzureTableStorageUtility.UpdateTableEnity(ConnectionString, GetTableName(tenantGroupId), updateEntity);
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"RunningJobRuleMapping table for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Skip AddJobMappingsForVEOMerge");
                }
                else
                {
                    logger.Error($"Failed to AddJobMappingsForVEOMerge for tenantGroupId: {tenantGroupId}, jobId: {jobId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to AddJobMappingsForVEOMerge for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Exception: {e}");
            }

            // still need to add cache true to mark this job has VEO rule to create the merge VEO job even though the update failed or DisableAddJobRuleMappingsKey is true
            _jobVEORuleMapping.AddOrUpdate(cacheKey, true, (key, oldValue) => true);
            logger.Info($"AddJobMappingsForVEOMerge for tenantGroupId: {tenantGroupId}, jobId: {jobId} with cacheKey: {cacheKey}");
        }

        public void AddJobRuleMapping(string tenantGroupId, string jobId, List<Guid> ruleIds)
        {
            if (HasDisableMappingFlag())
            {
                logger.Info("[DisableAddJobRuleMappings] Enable disableAddJobRuleMappings function, so skip add job rule mappings.");
                return;
            }

            var ruleIntIds = RMRuleDao.GetRuleIntIdsByRuleGuIds(ruleIds);
            if (ruleIntIds == null || ruleIntIds.Count == 0)
            {
                return;
            }

            var insertEntity = new RMRunningJobRuleMappingTableEntity(tenantGroupId, jobId)
            {
                RuleIds = "," + string.Join(",", ruleIntIds) + ",",
            };
            AzureTableStorageUtility.AddAzureTableEntity(ConnectionString, GetTableName(tenantGroupId), insertEntity);
        }

        public RMRunningJobRuleMappingTableEntity GetJobRuleMapping(string tenantGroupId, string jobId)
        {
            if (HasDisableMappingFlag())
            {
                logger.Info("[DisableAddJobRuleMappings] Enable disableAddJobRuleMappings function, so skip GetJobRuleMapping.");
                return null;
            }

            var entity = AzureTableStorageUtility.RetrieveTableEntity<RMRunningJobRuleMappingTableEntity>(ConnectionString, GetTableName(tenantGroupId), tenantGroupId, jobId);
            return entity;
        }

        public bool HasVEORule(string tenantGroupId, string jobId)
        {
            string cacheKey = $"{tenantGroupId}:{jobId}";
            if (_jobVEORuleMapping.TryGetValue(cacheKey, out var hasVEORule))
            {
                logger.Info($"Get HasVEORule from cache for tenantGroupId: {tenantGroupId}, jobId: {jobId}, hasVEORule: {hasVEORule}");
                return hasVEORule;
            }

            logger.Warn($"Not found Cache for HasVEORule for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Querying from table storage.");
            RMRunningJobRuleMappingTableEntity entity = null;
            try
            {
                entity = GetJobRuleMapping(tenantGroupId, jobId);
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"RunningJobRuleMapping table for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Skip check HasVEORule");
                }
                else
                {
                    logger.Error($"Failed to get job rule mapping for tenantGroupId: {tenantGroupId}, jobId: {jobId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to get job rule mapping for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Exception: {e}");
            }
            var result = entity?.HasVEORule ?? false;
            return result;
        }

        public async Task<bool> IsRuleUsedByJobAsync(string tenantGroupId, Guid ruleId)
        {
            if (HasDisableMappingFlag())
            {
                logger.Info("[DisableAddJobRuleMappings] Enable disableAddJobRuleMappings function, so skip HasVEORule.");
                return false;
            }

            var ruleIntIds = RMRuleDao.GetRuleIntIdsByRuleGuIds([ruleId]);

            if (ruleIntIds == null || ruleIntIds.Count == 0)
            {
                return false;
            }

            int waitingState = (int)JobStatus.Wait;
            int runningState = (int)JobStatus.InProgress;
            string tableName = GetTableName(tenantGroupId);
            var mappingEntities = AzureTableStorageUtility
                .QueryEntitiesByPartitionKey<RMRunningJobRuleMappingTableEntity>(
                    ConnectionString,
                    tableName,
                    tenantGroupId,
                    e => !string.IsNullOrEmpty(e.RuleIds) && e.RuleIds.Contains($",{ruleIntIds.First()},"))
                .ToList();
            if (mappingEntities == null || mappingEntities.Count == 0)
            {
                return false;
            }

            var jobIdToEntityMapping = mappingEntities.ToDictionary(e => e.RowKey);
            var jobIdToRecordMapping = (await JobMonitorDao.FindListAsync(j => jobIdToEntityMapping.Keys.Contains(j.Id))).ToDictionary(e => e.Id);

            bool hasRunningJobs = false;
            var entitiesToDelete = new List<RMRunningJobRuleMappingTableEntity>();

            foreach (var (jobId, jobEntity) in jobIdToEntityMapping)
            {
                if (!jobIdToRecordMapping.TryGetValue(jobId, out var job) || job == null)
                {
                    jobEntity.ETag = ETag.All;
                    entitiesToDelete.Add(jobEntity);
                    _jobVEORuleMapping.TryRemove($"{tenantGroupId}:{jobId}", out _);
                    continue;
                }

                if (job.Status == waitingState || job.Status == runningState)
                {
                    hasRunningJobs = true;
                    continue;
                }

                jobEntity.ETag = ETag.All;
                entitiesToDelete.Add(jobEntity);
                _jobVEORuleMapping.TryRemove($"{tenantGroupId}:{jobId}", out _);
            }

            if (entitiesToDelete.Count > 0)
            {
                AzureTableStorageUtility.DeleteTableEntities(
                    ConnectionString,
                    tableName,
                    entitiesToDelete);
            }

            return hasRunningJobs;
        }

        public void RemoveJobRuleMappings(string tenantGroupId, string jobId)
        {
            string cacheKey = $"{tenantGroupId}:{jobId}";
            if ((!_jobVEORuleMapping.TryGetValue(cacheKey, out var hasVeoRule) || !hasVeoRule) && HasDisableMappingFlag())
            {
                logger.Info("[DisableAddJobRuleMappings] Enable disableAddJobRuleMappings function, so skip RemoveJobRuleMappings.");
                return;
            }

            var deleteEntity = new RMRunningJobRuleMappingTableEntity(tenantGroupId, jobId)
            {
                ETag = ETag.All
            };
            try
            {
                AzureTableStorageUtility.DeleteTableEntity(ConnectionString, GetTableName(tenantGroupId), deleteEntity);
            }
            catch (RequestFailedException re)
            {
                if (re.ErrorCode == "TableNotFound")
                {
                    logger.Info($"RunningJobRuleMapping table for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Skip check RemoveJobRuleMappings");
                }
                else
                {
                    logger.Error($"Failed to RemoveJobRuleMappings for tenantGroupId: {tenantGroupId}, jobId: {jobId}. RequestFailedException: {re}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to RemoveJobRuleMappings for tenantGroupId: {tenantGroupId}, jobId: {jobId}. Exception: {e}");
            }
        }

        private static bool HasDisableMappingFlag()
        {
            var hasDisableMappingFlag = KeyValueDao.TryGetBoolValue(DisableAddJobRuleMappingsKey, out var disableAddJobRuleMappings);
            if (hasDisableMappingFlag && disableAddJobRuleMappings) return true;
            return false;
        }
    }
}
