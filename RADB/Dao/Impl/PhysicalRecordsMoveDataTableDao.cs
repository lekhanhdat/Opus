using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.SecurityTrimming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PhysicalRecordsMoveDataTableDao : IPhysicalRecordsMoveDataTableDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(PhysicalRecordsMoveDataTableDao));

        private const string TablePrefix = "RECOPhysicalRecordsMoveData";

        private string ConnectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private IRMLocationDao _locationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private IRMSecurityTrimmingHelper _securityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
        public IEnumerable<PhysicalRecordMoveData> Add(string tenantGroupId, List<PhysicalRecordMoveData> entities)
        {
            string tableName = GetTableName(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<PhysicalRecordMoveData>(ConnectionString, tableName, entities);
            return mEntities;
        }

        public async Task<(IEnumerable<PhysicalRecordMoveData>, int)> GetMoveDatasPaginationWithLimit(string tenantGroupId, PickListMoveParam filter, int limit)
        {
            var (locationScopeIds, isAdmin) = await _securityTrimmingHelper.GetPhysicalLocationPermissionAsync();
            if (((locationScopeIds == null || locationScopeIds.Count == 0) && !isAdmin) || filter.PageSize * filter.PageIndex > limit) return (new List<PhysicalRecordMoveData>(), limit);
            string tableName = GetTableName(tenantGroupId);
            var condition = new AzureTableQueryConditionBuilder();
            if (filter?.FilterOptions?.Status?.Any() == true)
            {
                foreach (var status in filter.FilterOptions.Status)
                {
                    condition.AppendOrQuery("Status", AzureQueryComparisons.Equal, (int)status);
                }
            }
            IEnumerable<PhysicalRecordMoveData> result;
            if (isAdmin)
            {
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<PhysicalRecordMoveData>(ConnectionString, tableName, condition.ToString())
                               .Where(e => filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                               .OrderByDescending(e => e.ExecuteOn).Take(limit);
            }
            else
            {
                var locationPaths = _locationDao.LoadLocationPathByLocationIds(locationScopeIds);
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<PhysicalRecordMoveData>(ConnectionString, tableName, condition.ToString())
                               .Where(e => (filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                               && locationPaths.Any(path => e.HomeLocation.Contains(path)))
                               .OrderByDescending(e => e.ExecuteOn).Take(limit);
            }
            var totalCount = result.Count();
            return (result.Skip(filter.PageIndex * filter.PageSize).Take(filter.PageSize), totalCount);
        }

        public async Task<(IEnumerable<PhysicalRecordMoveData>, int)> GetMoveDatasPagination(string tenantGroupId, PickMoveListParam filter, int pageIndex, int pageSize)
        {
            string tableName = GetTableName(tenantGroupId);
            var (locationScopeIds, isAdmin) = await _securityTrimmingHelper.GetPhysicalLocationPermissionAsync();
            var condition = new AzureTableQueryConditionBuilder();
            if (filter?.FilterOptions?.Status?.Any() == true)
            {
                foreach (var status in filter.FilterOptions.Status)
                {
                    condition.AppendOrQuery("Status", AzureQueryComparisons.Equal, (int)status);
                }
            }
            IEnumerable<PhysicalRecordMoveData> result;
            if (isAdmin)
            {
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<PhysicalRecordMoveData>(ConnectionString, tableName, condition.ToString())
                   .Where(e => filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                   .OrderByDescending(e => e.ExecuteOn)
                   .Skip(pageIndex * pageSize)
                   .Take(pageSize);
            }
            else
            {
                var locationPaths = _locationDao.LoadLocationPathByLocationIds(locationScopeIds);
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<PhysicalRecordMoveData>(ConnectionString, tableName, condition.ToString())
                   .Where(e => locationPaths.Any(path => e.HomeLocation.Contains(path)))
                   .Where(e => filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                   .OrderByDescending(e => e.ExecuteOn)
                   .Skip(pageIndex * pageSize)
                   .Take(pageSize);
            }

            return (result, result.Count());
        }
    }
}
