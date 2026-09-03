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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class BoxDashboardCollector : DashboardCollector
    {

        private static readonly IRMBoxConnectionDao BoxConnectionDao = PlatformWindsorManager.GetService<IRMBoxConnectionDao>();

        public override SourceFlag Flag => SourceFlag.Box;

        private readonly long unixEpochTicks = 621355968000000000;

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            var result = new List<RMDashboardDataUsage>();

            var boxConnections = BoxConnectionDao.GetAll();

            var dirPath = string.Empty;

            foreach (var connection in boxConnections)
            {
                var activeRecords = GetRecordsByStatus(RMRecordStatus.Active, connection.Id);
                var activeRecordCount = activeRecords.Count();

                var destoryedRecords = GetRecordsByStatus(RMRecordStatus.Destroyed, connection.Id);
                var destoryedRecordCount = destoryedRecords.Count();

                if(activeRecordCount > 0 || destoryedRecordCount > 0)
                {
                    var existingDirpath = activeRecordCount > 0 ? activeRecords[0].DirPath : destoryedRecords[0].DirPath;
                    var targetIndex = existingDirpath.IndexOf('\\');
                    var userLoginName = existingDirpath.Substring(0, targetIndex);
                    dirPath = $"{connection.Name}/{userLoginName}";
                }

                var dataUsage = new RMDashboardDataUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)Flag,
                    ContainerId = connection.ConnectionGroupId.ToString(),
                    ScopeId = connection.Id.ToString(),
                    Title = connection.Name,
                    Path = dirPath,
                    Active = activeRecordCount,
                    Destroyed = destoryedRecordCount,
                };
                result.Add(dataUsage);
            }
            return result;
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
where c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Active} 
and c.termId != {Guid.Empty} and c.nodeType = {(int)NodeLevel.BoxFile} GROUP BY c.termId";
            var queryCount = ExplorerDao.QueryRelatedTermCount(sql);
            return queryCount;
        }

        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {
            var activeSql = $@"
SELECT LEFT(TicksToDateTime(c.timeCreated - {unixEpochTicks}), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
And c.nodeType = {(int)NodeLevel.BoxFile}
AND c.timeCreated >= {startTicks}
GROUP BY LEFT(TicksToDateTime(c.timeCreated - {unixEpochTicks}), 10)
";

            var destroyedSql = $@"
SELECT LEFT(TicksToDateTime(c.destroyedTime - {unixEpochTicks}), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
And c.nodeType = {(int)NodeLevel.BoxFile}
AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
AND c.destroyedTime >= {startTicks}
GROUP BY LEFT(TicksToDateTime(c.destroyedTime - {unixEpochTicks}), 10)
";

            var waitingSql = $@"
SELECT LEFT(TicksToDateTime(c.manual_collectionTime - {unixEpochTicks}), 10) AS date , COUNT(1) AS count FROM c WHERE
c.manual_isManualSynced
AND c.sourceFlag = {(int)Flag}
And c.nodeType = {(int)NodeLevel.BoxFile}
AND c.manual_collectionTime >= 637865381843477781
GROUP BY LEFT(TicksToDateTime(c.manual_collectionTime - {unixEpochTicks}), 10)
";

            return new Dictionary<DataUsageStatus, string>
            {
                { DataUsageStatus.Active, activeSql },
                { DataUsageStatus.Destroyed, destroyedSql },
                { DataUsageStatus.WaitingForApproval, waitingSql }
            };
        }

        private List<Record> GetRecordsByStatus(RMRecordStatus status, Guid connectionId)
        {
            switch (status)
            {
                case RMRecordStatus.Active:
                    return ExplorerDao.QueryAll(record =>
                record.RecordStatus == (int)status
                && record.NodeType == (int)NodeLevel.BoxFile
                && record.ContainerId == connectionId.ToString()).ToList();

                case RMRecordStatus.Destroyed:
                    return ExplorerDao.QueryAll(record =>
              record.RecordStatus == (int)status
              && record.NodeType == (int)NodeLevel.BoxFile
              && record.ContainerId == connectionId.ToString()).ToList();

                default:
                    return new List<Record>();
            }
        }
    }
}
