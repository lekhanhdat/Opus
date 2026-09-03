using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMMultiGeoApiChangeLogDao : IRMMultiGeoApiChangeLogDao
    {
        private const string TablePrefix = "RMMultiGeoApiChangeLog";

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMMultiGeoApiChangeLogDao));

        private string ConnectionString => RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        public void Add(string tenantGroupId, RMMultiGeoApiChangeLogEntity entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantGroupId) || entity == null)
                {
                    return;
                }

                AzureTableStorageUtility.AddAzureTableEntity(ConnectionString, GetTableName(tenantGroupId), entity);
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to add multi-geo api change log. TenantGroupId: [{tenantGroupId}], OperationType: [{entity?.OperationType}]. Exception: {ex}");
            }
        }

        public IEnumerable<string> GetAllOperationTypeNeedSync(string logonGroupId, long lastSyncTime)
        {
            try
            {
                string tableName = GetTableName(logonGroupId);
                var condition = new AzureTableQueryConditionBuilder();
                if (lastSyncTime > 0)
                {
                    var lastSyncDateTime = new DateTimeOffset(lastSyncTime, TimeSpan.Zero).UtcDateTime;
                    condition.AppendAndQuery(nameof(RMMultiGeoApiChangeLogEntity.CreatedOn), AzureQueryComparisons.GreaterThan, lastSyncDateTime);
                }
                return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RMMultiGeoApiChangeLogEntity>(ConnectionString, tableName, condition.ToString())?.Select(e => e.OperationType).Distinct() ?? new List<string>();
            }
            catch(Exception ex)
            {
                s_logger.Error($"Failed to get operation types needed to sync. TenantGroupId: [{logonGroupId}], LastSyncTime: [{lastSyncTime}]. Exception: {ex}");
                return Enumerable.Empty<string>();
            }
        }

        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
    }
}