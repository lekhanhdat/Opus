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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using RADashboard.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class OneDriveDashboardCollector : DashboardCollector
    {
        public override SourceFlag Flag => SourceFlag.OneDrive;

        private static readonly IRMScopeDao RMScopeDao = PlatformWindsorManager.GetService<IRMScopeDao>();

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            var result = new List<RMDashboardDataUsage>();

            var scopeInfos = RMScopeDao.GetExistScopeInfo().Distinct(new ScopeInfoComparer()).ToDictionary(item => item.FullPath, item => item.ScopeName);

            var createdSql = $"SELECT c.aveSiteId, COUNT(1) AS siteUsageCount FROM items c where c.sourceFlag = {(int)Flag} and  c.recordStatus = {(int)RMRecordStatus.Active} and (c.nodeType = {(int)NodeLevel.Item} or c.nodeType = {(int)NodeLevel.Folder}) GROUP BY c.aveSiteId";
            var createdDataUsageDic = ExplorerDao.QuerySiteCollectionUsageCount(createdSql);

            var destoryedSql = $"SELECT c.aveSiteId, COUNT(1) AS siteUsageCount FROM items c where  c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Destroyed} and (c.nodeType = {(int)NodeLevel.Item} or c.nodeType = {(int)NodeLevel.Folder}) GROUP BY c.aveSiteId";
            var destoryedDataUsageDic = ExplorerDao.QuerySiteCollectionUsageCount(destoryedSql);

            var archivedSql = $"SELECT c.aveSiteId, COUNT(1) AS siteUsageCount FROM items c where  c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Archived} and (c.nodeType = {(int)NodeLevel.Item} or c.nodeType = {(int)NodeLevel.Document} or c.nodeType = {(int)NodeLevel.Folder}) GROUP BY c.aveSiteId";
            var archivedDataUsageDic = ExplorerDao.QuerySiteCollectionUsageCount(archivedSql);

            var siteIds = createdDataUsageDic.Keys.Concat(destoryedDataUsageDic.Keys).Concat(archivedDataUsageDic.Keys).ToHashSet().ToList();
            var sites = RABrowserClient.GetRemoteSiteCollectionsByIdList(siteIds);

            foreach (var site in sites)
            {

                var siteTitle = site.url;
                if (scopeInfos.TryGetValue(site.url, out var title))
                {
                    if(!string.IsNullOrEmpty(title))
                    {
                        siteTitle = title;
                    }
                }

                var dataUsage = new RMDashboardDataUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)Flag,
                    ContainerId = site.parentId,
                    ScopeId = site.id,
                    Title = siteTitle,
                    Path = site.url,
                    Active = 0,
                    Destroyed = 0,
                };

                if (createdDataUsageDic.TryGetValue(site.id, out var createdCount))
                {
                    dataUsage.Active = createdCount;
                }

                if (destoryedDataUsageDic.TryGetValue(site.id, out var destoryedCount))
                {
                    dataUsage.Destroyed = destoryedCount;
                }

                if (archivedDataUsageDic.TryGetValue(site.id, out var archivedCount))
                {
                    dataUsage.Archived = archivedCount;
                }

                result.Add(dataUsage);
            }

            return result;
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
where c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Active} 
and c.termId != {Guid.Empty} and (c.nodeType = {(int)NodeLevel.Item} or c.nodeType = {(int)NodeLevel.Folder}) GROUP BY c.termId";
            return ExplorerDao.QueryRelatedTermCount(sql);
        }

        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {
            var activeSql = $@"
SELECT LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.timeCreated >= {startTicks}
AND ARRAY_CONTAINS([{(int)NodeLevel.Item}, {(int)NodeLevel.Folder}], c.nodeType)
GROUP BY LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10)
";

            var destroyedSql = $@"
SELECT LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
AND c.destroyedTime >= {startTicks}
AND ARRAY_CONTAINS([{(int)NodeLevel.Item}, {(int)NodeLevel.Folder}], c.nodeType)
GROUP BY LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10)
";

            var waitingSql = $@"
SELECT LEFT(TicksToDateTime(c.manual_collectionTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.manual_isManualSynced
AND c.sourceFlag = {(int)Flag}
AND c.manual_collectionTime >= 637865381843477781
GROUP BY LEFT(TicksToDateTime(c.manual_collectionTime - 621355968000000000), 10)
";

            return new Dictionary<DataUsageStatus, string>
            {
                { DataUsageStatus.Active, activeSql },
                { DataUsageStatus.Destroyed, destroyedSql },
                { DataUsageStatus.WaitingForApproval, waitingSql }
            };
        }
    }
}
