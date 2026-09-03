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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.SecurityTrimming;
using DocumentFormat.OpenXml.Math;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RecordReturnLoanDataHistoryTableDao : IRecordReturnLoanDataHistoryTableDao
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(RecordReturnLoanDataHistoryTableDao));

        private const string TablePrefix = "RECORDReturnLoanDataHistory";

        private string ConnectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private IRMLocationDao _locationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private IRMSecurityTrimmingHelper _securityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }

        public IEnumerable<RecordReturnLoanDataHistory> AddRecordReturnLoanDataHistory(string tenantGroupId, List<RecordReturnLoanDataHistory> entities)
        {
            string tableName = GetTableName(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<RecordReturnLoanDataHistory>(ConnectionString, tableName, entities);
            return mEntities;
        }

        public async Task<(IEnumerable<RecordReturnLoanDataHistory>,int)> GetRecordReturnLoanDataHistoryPaginationWithLimit(string tenantGroupId, ReturnLoanHistoryParam filter, int limit)
        {
            var (locationScopeIds, isAdmin) = await _securityTrimmingHelper.GetPhysicalLocationPermissionAsync();
            if (((locationScopeIds == null || locationScopeIds.Count == 0) && !isAdmin) || filter.PageSize * filter.PageIndex > limit) return (new List<RecordReturnLoanDataHistory>(), limit);
            string tableName = GetTableName(tenantGroupId);
            var condition = new AzureTableQueryConditionBuilder();
            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MaxValue;
            if (filter?.FilterOptions != null && DateTime.TryParse(filter.FilterOptions.StartTime, out startTime) && DateTime.TryParse(filter.FilterOptions.EndTime, out endTime))
            {
                condition.AppendAndQuery("ReturnTime", AzureQueryComparisons.LessThanOrEqual, endTime.Ticks);
                condition.AppendAndQuery("ReturnTime", AzureQueryComparisons.GreaterThanOrEqual, startTime.Ticks);
            }
            IEnumerable<RecordReturnLoanDataHistory> result;
            if (isAdmin)
            {
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordReturnLoanDataHistory>(ConnectionString, tableName, condition.ToString())
                               .Where(e => filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                               .OrderByDescending(e => e.ReturnTime).Take(limit);
            }
            else
            {
                var locationPaths = _locationDao.LoadLocationPathByLocationIds(locationScopeIds);
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordReturnLoanDataHistory>(ConnectionString, tableName, condition.ToString())
                               .Where(e => (filter == null || filter.SearchText.IsNullOrEmpty() || e.ItemName.ToLowerInvariant().Contains(filter.SearchText.ToLowerInvariant()))
                               && locationPaths.Any(path => e.HomeLocation.Contains(path)))
                               .OrderByDescending(e => e.ReturnTime).Take(limit);
            }
            var totalCount = result.Count();
            return (result.Skip(filter.PageIndex * filter.PageSize).Take(filter.PageSize), totalCount);
        }

        public async Task<(IEnumerable<RecordReturnLoanDataHistory>, int)> GetRecordReturnLoanDataHistoryPagination(string tenantGroupId, int pageIndex, int pageSize)
        {
            string tableName = GetTableName(tenantGroupId);
            var (locationScopeIds, isAdmin) = await _securityTrimmingHelper.GetPhysicalLocationPermissionAsync();
            var condition = new AzureTableQueryConditionBuilder();
            IEnumerable<RecordReturnLoanDataHistory> result;
            if (isAdmin)
            {
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordReturnLoanDataHistory>(ConnectionString, tableName, condition.ToString())
                   .OrderByDescending(e => e.ReturnTime)
                   .Skip(pageIndex * pageSize)
                   .Take(pageSize);
            }
            else
            {
                var locationPaths = _locationDao.LoadLocationPathByLocationIds(locationScopeIds);
                result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordReturnLoanDataHistory>(ConnectionString, tableName, condition.ToString())
                   .Where(e => locationPaths.Any(path => e.HomeLocation.Contains(path)))
                   .OrderByDescending(e => e.ReturnTime)
                   .Skip(pageIndex * pageSize)
                   .Take(pageSize);
            }
            
            return (result, result.Count());
        }
    }
}
