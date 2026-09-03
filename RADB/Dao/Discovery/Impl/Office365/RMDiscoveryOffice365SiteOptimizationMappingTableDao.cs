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
using AngleSharp.Html;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365SiteOptimizationMappingTableDao : IRMDiscoveryOffice365SiteOptimizationMappingTableDao
    {
        public async Task<int> CountAsync(Guid o365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await context.Office365SiteOptimizationMappingInfos.CountAsync();
        }

        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryOffice365SiteOptimizationMappingInfo> updateRuleInfos, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            context.Office365SiteOptimizationMappingInfos.AddOrUpdate(updateRuleInfos.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoAsync(Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365SiteOptimizationMappingInfos.ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoBySettingIdsAsync(Guid O365TenantId, IEnumerable<Guid> settingIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var ids = settingIds.ToHashSet();
            return await efContext.Office365SiteOptimizationMappingInfos
                .AsNoTracking()
                .Where(item => ids.Contains(item.SettingId))
                .ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoBySettingIdsAsync(Guid O365TenantId, IEnumerable<Guid> settingIds, int skip, int take)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var ids = settingIds.ToHashSet();
            return await efContext.Office365SiteOptimizationMappingInfos
                .AsNoTracking()
                .Where(item => ids.Contains(item.SettingId))
                .OrderBy(item => item.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<RMDiscoveryOffice365SiteOptimizationMappingInfo> GetMappingInfoByNodeIdAsync(long nodeId, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365SiteOptimizationMappingInfos.FirstAsync(item => item.NodeId == nodeId);
        }

        public async Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetMappingInfoBySettingIdAsync(Guid settingId, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await context.Office365SiteOptimizationMappingInfos.Where(item => item.SettingId == settingId).ToListAsync();
        }

        public async Task<int> GetInScopeSiteCount(Guid O365TenantId, int containerId)
        {
            var schema = RMDiscoveryDBManager.GetOffice365SchemaName(O365TenantId);
            var sql = @$"select count(distinct s.Id) 
                        from {schema}.RMSiteInfoes as s
                        join {schema}.RMSiteOptimizationMappingInfo as m
                        on s.Id = m.NodeId
                        where s.ContainerId = @containerId";
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return context.Database.SqlQuery<int>(sql, new SqlParameter("containerId", containerId)).FirstOrDefault();
        }

        public async Task<List<long>> GetAllInScopeSiteIds(Guid O365TenantId, IEnumerable<long> itemIds)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            var itemIdsSet = itemIds.ToHashSet();
            var result = new HashSet<long>();
            foreach (var batch in itemIdsSet.Batch(1000))
            {
                var batchSet = batch.ToHashSet();
                var batchResult = await context.Office365SiteOptimizationMappingInfos.AsNoTracking()
                    .Where(item => batchSet.Contains(item.NodeId))
                    .Select(item => item.NodeId)
                    .Distinct()
                    .ToListAsync();
                result.UnionWith(batchResult);
            }
            return result.ToList();
        }

        public async Task removeMappingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId)
        {
            var mappings = context.Office365SiteOptimizationMappingInfos.Where(item => item.SettingId == settingId);
            foreach (var mapping in mappings)
            {
                context.Office365SiteOptimizationMappingInfos.Remove(mapping);
            }
            await context.SaveChangesAsync();
        }
        //查询所有的site
        public async Task<List<long>> GetAllsites(Guid O365TenantId)
        {
            //sql
            //var sql = $"SELECT nodeId  FROM [{_schemaName}].[RMSiteOptimizationMappingInfo] GROUP BY nodeId;";
            //var dataList = await _queryDao.GetDataListAsync<long>(sql);
            //return dataList;
            //ef
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(O365TenantId);
            return await efContext.Office365SiteOptimizationMappingInfos.Select(item => item.NodeId).Distinct().ToListAsync(); ;

        }

        #region V3
        public async Task<long> CountPHLDataTotalSizeV3(Guid o365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await context.Office365ContainerInfoes.SumAsync(info => info.PHLTotalSize);
        }

        public async Task<long> GetPHLDataTotalSizeV3ByContainerId(Guid o365TenantId, int containerId)
        {
            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await context.Office365ContainerInfoes
                .Where(info => info.Id == containerId)
                .Select(info => info.PHLTotalSize)
                .FirstOrDefaultAsync();
        }
        #endregion
    }
}
