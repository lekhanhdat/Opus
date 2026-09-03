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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class PhysicalDashboardCollector : DashboardCollector
    {

        private static readonly IRMLocationDao RMLocationDao = PlatformWindsorManager.GetService<IRMLocationDao>();

        public override SourceFlag Flag => SourceFlag.Physical;

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            var result = new List<RMDashboardDataUsage>();

            var createdSql = $@"SELECT c.locationId as aveSiteId, COUNT(1) as siteUsageCount FROM c 
where c.sourceFlag = {(int)Flag} and 
(c.recordStatus = {(int)RMRecordStatus.Active} or c.recordStatus = {(int)RMRecordStatus.Closed} or c.recordStatus = {(int)RMRecordStatus.Missing}) and 
(c.nodeType = {(int)RMNodeType.PhyBox} or c.nodeType = {(int)RMNodeType.PhyRecord} or c.nodeType = {(int)RMNodeType.PhyFile})
group by c.locationId";
            var createdDic = ExplorerDao.QuerySiteCollectionUsageCount(createdSql);

            var destoryedSql = $@"SELECT c.locationId as aveSiteId, COUNT(1) as siteUsageCount FROM c 
where c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Destroyed} and 
(c.nodeType = {(int)RMNodeType.PhyBox} or c.nodeType = {(int)RMNodeType.PhyRecord} or c.nodeType = {(int)RMNodeType.PhyFile})
group by c.locationId";
            var destoryedDic = ExplorerDao.QuerySiteCollectionUsageCount(destoryedSql);

            var locationIds = createdDic.Keys.Union(destoryedDic.Keys).ToList().ConvertAll(item => new Guid(item));
            var locationInfos = RMLocationDao.GetLocationInfos(locationIds);

            var locationIdNameMapping = RMLocationDao.GetLocationIdNameMapping();

            string GetLocationFullPath(RMLocation location)
            {
                var ids = location.DirPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var names = ids.ToList().ConvertAll(item => locationIdNameMapping[int.Parse(item)]);
                names[0] = I18NEntity.GetString(names[0]);
                return string.Join("/", names);
            }

            foreach (var locationInfo in locationInfos)
            {
                var dataUsage = new RMDashboardDataUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)Flag,
                    ContainerId = Guid.Empty.ToString(),
                    ScopeId = locationInfo.UniqueId.ToString(),
                    Title = locationInfo.Name,
                    Path = GetLocationFullPath(locationInfo),
                    Active = 0,
                    Destroyed = 0,
                };

                if(createdDic.TryGetValue(locationInfo.UniqueId.ToString(), out var count))
                {
                    dataUsage.Active = count;
                }

                if (destoryedDic.TryGetValue(locationInfo.UniqueId.ToString(), out count))
                {
                    dataUsage.Destroyed = count;
                }

                result.Add(dataUsage);
            }

            return result;
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
where c.sourceFlag = {(int)Flag} and
(c.recordStatus = {(int)RMRecordStatus.Active} or c.recordStatus = {(int)RMRecordStatus.Closed} or c.recordStatus = {(int)RMRecordStatus.Missing})
and c.termId != {Guid.Empty} 
and (c.nodeType = {(int)RMNodeType.PhyBox} or c.nodeType = {(int)RMNodeType.PhyRecord} or c.nodeType = {(int)RMNodeType.PhyFile}) 
GROUP BY c.termId";
            return ExplorerDao.QueryRelatedTermCount(sql);
        }

        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {
            var activeSql = $@"
SELECT LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.timeCreated >= {startTicks}
AND ARRAY_CONTAINS([{(int)RMNodeType.PhyBox}, {(int)RMNodeType.PhyRecord}, {(int)RMNodeType.PhyFile}], c.nodeType)
GROUP BY LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10)
";

            var destroyedSql = $@"
SELECT LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
AND c.destroyedTime >= {startTicks}
AND ARRAY_CONTAINS([{(int)RMNodeType.PhyBox}, {(int)RMNodeType.PhyRecord}, {(int)RMNodeType.PhyFile}], c.nodeType)
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
