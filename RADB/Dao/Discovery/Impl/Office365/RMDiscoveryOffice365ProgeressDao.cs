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
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using DocumentFormat.OpenXml.Office.CustomUI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365ProgeressDao : IRMDiscoveryOffice365ProgressDao
    {
        public async Task AddOrUpdateSiteOptimizedInfoAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteOptimizedInfo[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365SiteOptimizedInfoes.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateContainerOptimizedInfoAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerOptimizedInfo[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365ContainerOptimizedInfoes.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<RMDiscoveryOffice365SiteOptimizedInfo> GetSiteOptimizedInfoAsync(Guid o365TenantId, long siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteOptimizedInfoes.FirstOrDefaultAsync(item => item.SiteId == siteId);
        }

        public async Task<RMDiscoveryOffice365ContainerOptimizedInfo> GetContainerOptimizedInfoAsync(Guid o365TenantId, int containerId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerOptimizedInfoes.FirstOrDefaultAsync(item => item.ContainerId == containerId);
        }

        public async Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfoAsync(Guid o365TenantId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var optimizedSql = $@"SELECT 
SUM(NextOptimizableFileTotalSize) AS NextOptimizableFileTotalSize,
SUM(NextOptimizableVersionTotalSize) AS NextOptimizableVersionTotalSize,
SUM(Archived) AS Archived,
SUM(Deleted) AS Deleted
FROM [{schemaName}].[RMSiteOptimizedInfoes]";
            var dataCollection = await context.ExecuteQueryAsync(optimizedSql);
            var res = dataCollection.ToList<RMDiscoveryProgressSummaryOptimizedInfo>().FirstOrDefault();
            res ??= new RMDiscoveryProgressSummaryOptimizedInfo();

            var aggregateSql = $"SELECT FileTotalSize, FileSumCount FROM [{schemaName}].[RMAggregateTotalData]";
            var totalData = (await context.ExecuteQueryAsync(aggregateSql)).ToList<RMDiscoveryOffice365AggregateTotalData>();
            res.FileTotalSize = totalData.Sum(item => item.FileTotalSize);
            res.FileSumCount = totalData.Sum(item => item.FileSumCount);

            return res;
        }

        public async Task<List<RMDiscoveryProgressContainerOptimizedInfo>> GetContainerOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(paginateInfo.O365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $@"SELECT 
container.Name AS Name, 
container.FileTotalSize - optimized.Archived - optimized.Deleted AS Remaining,
optimized.Archived AS Archived,
optimized.Deleted AS Deleted
FROM [{schemaName}].[RMContainerOptimizedInfoes] AS optimized
JOIN [{schemaName}].[RMContainerInfoes] AS container
ON optimized.ContainerId = container.Id
ORDER BY container.Name
OFFSET @offset ROWS
FETCH NEXT @pageSize ROWS ONLY";
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@offset", paginateInfo.PageIndex * paginateInfo.PageSize),
                new SqlParameter("@pageSize", paginateInfo.PageSize));
            return dataCollection.ToList<RMDiscoveryProgressContainerOptimizedInfo>();
        }

        public async Task<int> CountContainerOptimizedAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerOptimizedInfoes.CountAsync();
        }

        public async Task<List<RMDiscoveryProgressSiteOptimizedInfo>> GetSiteOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(paginateInfo.O365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $@"SELECT 
site.Url AS Url,
site.ContentSource AS ContentSource,
optimized.NextOptimizationTime AS NextOptimizationTime,
site.FileTotalSize AS FileTotalSize,
site.FileSumCount AS FileSumCount,
optimized.NextOptimizableFileTotalSize AS NextOptimizableFileTotalSize,
optimized.NextOptimizableVersionTotalSize AS NextOptimizableVersionTotalSize,
optimized.Archived AS Archived,
optimized.Deleted AS Deleted
FROM [{schemaName}].[RMSiteOptimizedInfoes] AS optimized
JOIN [{schemaName}].[RMSiteInfoes] AS site
ON optimized.SiteId = site.Id
ORDER BY optimized.NextOptimizationTime, site.Url
OFFSET @offset ROWS
FETCH NEXT @pageSize ROWS ONLY";
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@offset", paginateInfo.PageIndex * paginateInfo.PageSize),
                new SqlParameter("@pageSize", paginateInfo.PageSize));
            return dataCollection.ToList<RMDiscoveryProgressSiteOptimizedInfo>();
        }

        public async Task<int> CountSiteOptimizedAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteOptimizedInfoes.CountAsync();
        }
    }
}
