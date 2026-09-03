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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPProgressDao : IRMDiscoveryAOSPProgressDao
    {
        public async Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfoAsync(Guid o365TenantId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);

            var optimizedSql = $@"
SELECT
    SUM(NextOptimizableFileTotalSize) AS NextOptimizableFileTotalSize,
    SUM(NextOptimizableVersionTotalSize) AS NextOptimizableVersionTotalSize,
    SUM(CASE WHEN ContentSource = 1 THEN Archived ELSE 0 END) AS SPArchived,
    SUM(CASE WHEN ContentSource = 1 THEN ArchivedCount ELSE 0 END) AS SPArchivedCount,
    SUM(CASE WHEN ContentSource = 1 THEN Deleted ELSE 0 END) AS SPDeleted,
    SUM(CASE WHEN ContentSource = 1 THEN DeletedCount ELSE 0 END) AS SPDeletedCount,
    SUM(CASE WHEN ContentSource = 6 THEN Archived ELSE 0 END) AS OneArchived,
    SUM(CASE WHEN ContentSource = 6 THEN ArchivedCount ELSE 0 END) AS OneArchivedCount,
    SUM(CASE WHEN ContentSource = 6 THEN Deleted ELSE 0 END) AS OneDeleted,
    SUM(CASE WHEN ContentSource = 6 THEN DeletedCount ELSE 0 END) AS OneDeletedCount 
FROM [{schemaName}].[RMAOSPSiteOptimizedInfoes] 
WHERE ContentSource IN (1, 6);
";
            var dataCollection = await context.ExecuteQueryAsync(optimizedSql);
            var res = dataCollection.ToList<RMDiscoveryProgressSummaryOptimizedInfo>().FirstOrDefault();
            res ??= new RMDiscoveryProgressSummaryOptimizedInfo();

            var aggregateSql = $"SELECT FileTotalSize, FileSumCount FROM [{schemaName}].[RMAOSPBasicInactiveData]";
            var totalData = (await context.ExecuteQueryAsync(aggregateSql)).ToList<RMDiscoveryAOSPAggregateTotalData>();
            res.FileTotalSize = totalData.Sum(item => item.FileTotalSize);
            res.FileSumCount = totalData.Sum(item => item.FileSumCount);
            res.Archived = res.SPArchived + res.OneArchived;
            res.Deleted = res.SPDeleted + res.OneDeleted;
            res.ArchivedCount = res.SPArchivedCount + res.OneArchivedCount;
            res.DeletedCount = res.SPDeletedCount + res.OneDeletedCount;

            return res;
        }

        public async Task<RMDiscoveryAOSPSiteOptimizedInfo> GetSiteOptimizedInfoAsync(Guid o365TenantId, long siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPSiteOptimizedInfoes.FirstOrDefaultAsync(item => item.SiteId == siteId);
        }

        public async Task AddOrUpdateSiteOptimizedInfoAsync(Guid o365TenantId, params RMDiscoveryAOSPSiteOptimizedInfo[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            efContext.AOSPSiteOptimizedInfoes.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        //public async Task<RMDiscoveryAOSPContainerOptimizedInfo> GetContainerOptimizedInfoAsync(Guid o365TenantId, int containerId)
        //{
        //    using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
        //    return await efContext.AOSPContainerOptimizedInfoes.FirstOrDefaultAsync(item => item.ContainerId == containerId);
        //}
    }
}
