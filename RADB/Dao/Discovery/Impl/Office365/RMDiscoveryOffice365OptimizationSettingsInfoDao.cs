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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365OptimizationSettingsInfoDao : IRMDiscoveryOffice365OptimizationSettingsInfoDao
    {
        public async Task<int> AddOrUpdateAsync(RMDiscoveryOffice365OptimizationSettingsInfo settingInfo, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            context.Office365OptimizationSettingsInfos.AddOrUpdate(settingInfo);
            return await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365OptimizationSettingsInfo>> GetNeedRunJobSettingAsync(long time, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365OptimizationSettingsInfos.Where(item => item.NextTime < time && item.Status == (int)DiscoverOptimizationScheduleStatus.Ready).OrderBy(item => item.NextTime).ToListAsync();
        }

        public async Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoByIdAsync(Guid id, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365OptimizationSettingsInfos.FirstAsync(item => item.SettingId == id);
        }
        public async Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoBySettingAsync(string setting, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var result = await context.Office365OptimizationSettingsInfos.Where(item => item.Setting == setting && item.Status == (int)DiscoverOptimizationScheduleStatus.Ready).ToListAsync();
            if (result == null)
            {
                return null;
            }
            else
            {
                return result.FirstOrDefault();
            }
        }
        public async Task<int> UpdateStatusAsync(Guid settingId, DiscoverOptimizationScheduleStatus status, Guid O365TenantId)
        {
            var parameters = new List<SqlParameter>();
            string schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(O365TenantId);
            var sql = $"UPDATE [{schemaName}].[RMOptimizationSettingsInfo] SET Status = @Status WHERE SettingId = @SettingId";
            parameters.Add(new SqlParameter("@Status", (int)status));
            parameters.Add(new SqlParameter("@SettingId", settingId));
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var effectCount = await context.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
            return effectCount;
        }

        public async Task<List<RMDiscoveryOffice365OptimizationSettingsInfo>> GetPlanSettingInfoAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(paginateInfo.O365TenantId);
            return await efContext.Office365OptimizationSettingsInfos.Where(item => item.Status == (int)DiscoverOptimizationScheduleStatus.Ready && item.IsHandle == false)
                .OrderBy(item => item.NextTime)
                .Skip(paginateInfo.PageIndex * paginateInfo.PageSize).Take(paginateInfo.PageSize).ToListAsync();
        }

        public async Task<int> CountPlanSettingInfoAsync(Guid o365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await context.Office365OptimizationSettingsInfos.Where(item => item.Status == (int)DiscoverOptimizationScheduleStatus.Ready).CountAsync();
        }

        public async Task<List<string>> GetSettingRelateSitesAsync(Guid o365TenantId, Guid uniqueId, int skip, int take)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var query = context.Office365SiteOptimizationMappingInfos
                .Where(mapping => mapping.SettingId == uniqueId)
                .Join(context.Office365SiteInfoes,
                    mapping => mapping.NodeId,
                    site => site.Id,
                    (mapping, site) => new
                    {
                        site.Id,
                        site.Url
                    })
                .OrderBy(item => item.Id)
                .AsQueryable();

            if (skip > 0)
            {
                query = query.Skip(skip);
            }

            if (take > 0)
            {
                query = query.Take(take);
            }

            return await query.Select(item => item.Url).ToListAsync();
        }

        public async Task<int> CountSettingRelateSiteAsync(Guid o365TenantId, Guid uniqueId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await context.Office365SiteOptimizationMappingInfos.Where(item => item.SettingId == uniqueId).CountAsync();
        }

        public async Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoBySettingInfoIdAsync(int id, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365OptimizationSettingsInfos.Where(item => id == (int)item.Id).FirstOrDefaultAsync();
        }

        public async Task<int> removePlanSettingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId)
        {
            var setting = context.Office365OptimizationSettingsInfos.Where(item => item.SettingId == settingId && item.Status == (int)DiscoverOptimizationScheduleStatus.Ready).FirstOrDefault();
            context.Office365OptimizationSettingsInfos.Remove(setting);
            return await context.SaveChangesAsync();
        }

        public async Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetLatestSettingAsync(Guid o365TenantId, Guid siteId, long beforeScheduleTicks)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schema = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schema);
            var sql = $@"SELECT 
TOP 1 setting.Id AS Id, setting.SettingId AS SettingId, setting.Type AS Type, setting.NextTime AS NextTime, setting.Setting AS Setting, setting.Status AS Status
FROM [{schema}].[RMSiteInfoes] AS site
JOIN [{schema}].[RMSiteOptimizationMappingInfo] AS mapping
ON site.Id = mapping.NodeId
JOIN [{schema}].[RMOptimizationSettingsInfo] AS setting
ON mapping.SettingId = setting.SettingId
WHERE site.SiteId = @siteId AND setting.NextTime > @beforeScheduleTicks
ORDER BY NextTime";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@siteId", siteId), new SqlParameter("@beforeScheduleTicks", beforeScheduleTicks));
            return dataCollection.ToList<RMDiscoveryOffice365OptimizationSettingsInfo>().FirstOrDefault();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetSettingRelatedSitesAsync(Guid o365TenantId, Guid id)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schema = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schema);
            const int pageSize = 100;
            for (var i = 0; ; i += pageSize)
            {
                var sql = $@"SELECT 
Site.Id AS Id, Site.Url AS Url, Site.SiteId AS SiteId, Site.ContentSource AS ContentSource
FROM [{schema}].[RMSiteOptimizationMappingInfo] AS setting
JOIN [{schema}].[RMSiteInfoes] AS site
ON setting.NodeId = site.Id
WHERE setting.SettingId = @settingId
ORDER BY site.Id
OFFSET @offset ROWS
FETCH NEXT @pageSize ROWS ONLY";
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@offset", i * pageSize),
                    new SqlParameter("@pageSize", pageSize),
                    new SqlParameter("@settingId", id));
                var items = dataCollection.ToList<RMDiscoveryOffice365SiteInfo>();
                foreach (var item in items)
                {
                    yield return item;
                }

                if (items.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async Task<int> UpdateIsHandleAsync(Guid settingId, bool isHandle, Guid O365TenantId)
        {
            var parameters = new List<SqlParameter>();
            string schemaName = SecurityUtils.SanitizeSQLSchemaName(RMDiscoveryDBManager.GetOffice365SchemaName(O365TenantId));
            var sql = $"UPDATE {schemaName}.[RMOptimizationSettingsInfo] SET IsHandle = @IsHandle WHERE SettingId = @SettingId";
            parameters.Add(new SqlParameter("@IsHandle", isHandle ? 1 : 0));
            parameters.Add(new SqlParameter("@SettingId", settingId));
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var effectCount = await context.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
            return effectCount;
        }
    }
}
