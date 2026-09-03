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
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class AzureFileShareDashboardCollector : DashboardCollector
    {

        private static readonly IRMAzureFileShareConnectionDao AzureFileShareConnection = PlatformWindsorManager.GetService<IRMAzureFileShareConnectionDao>();

        public override SourceFlag Flag => SourceFlag.AzureFileShare;

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            var result = new List<RMDashboardDataUsage>();

            var azureFileShareConnections = AzureFileShareConnection.GetAll();

            foreach(var connection in azureFileShareConnections)
            {
                var dirPath = connection.AccessEndPoint.EndsWith("/") ?
                                connection.AccessEndPoint + connection.FileShareName + "/" :
                                connection.AccessEndPoint + "/" + connection.FileShareName + "/";

                var sharePath = connection.AccessEndPoint.EndsWith("/") ?
                                connection.AccessEndPoint + connection.FileShareName:
                                connection.AccessEndPoint + "/" + connection.FileShareName;

                string createdSql = @"SELECT VALUE COUNT(1) 
                        FROM c 
                        WHERE c.sourceFlag = @sourceFlag 
                          AND c.recordStatus = @recordStatus 
                          AND (c.nodeType = @directoryNodeType OR c.nodeType = @fileNodeType)
                          AND (STARTSWITH(c.dirPath, @dirPath) OR STRINGEQUALS(c.dirPath, @sharePath, true))";

                var createdParameters = new Dictionary<string, object>
                {
                    { "@sourceFlag", (int)Flag },         
                    { "@recordStatus", (int)RMRecordStatus.Active }, 
                    { "@directoryNodeType", (int)NodeLevel.AzureFileShareDirectory }, 
                    { "@fileNodeType", (int)NodeLevel.AzureFileShareFile },  
                    { "@dirPath", dirPath },             
                    { "@sharePath", sharePath }            
                };

                var createdCount = ExplorerDao.QueryCount(createdSql, createdParameters);

                string destroyedQuery = @"SELECT VALUE COUNT(1) 
                                        FROM c 
                                        WHERE c.sourceFlag = @sourceFlag 
                                          AND c.recordStatus = @recordStatus 
                                          AND (c.nodeType = @directoryNodeType OR c.nodeType = @fileNodeType)
                                          AND (STARTSWITH(c.dirPath, @dirPath) OR STRINGEQUALS(c.dirPath, @sharePath, true))";

                var destroyedParameters = new Dictionary<string, object>
                {
                    { "@sourceFlag", (int)Flag },
                    { "@recordStatus", (int)RMRecordStatus.Destroyed },
                    { "@directoryNodeType", (int)NodeLevel.AzureFileShareDirectory },
                    { "@fileNodeType", (int)NodeLevel.AzureFileShareFile },
                    { "@dirPath", dirPath },
                    { "@sharePath", sharePath }
                };

                var destoryedCount = ExplorerDao.QueryCount(destroyedQuery, destroyedParameters);

                var dataUsage = new RMDashboardDataUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)Flag,
                    ContainerId = connection.ConnectionGroupId.ToString(),
                    ScopeId = connection.Id.ToString(),
                    Title = connection.Name,
                    Path = sharePath,
                    Active = createdCount,
                    Destroyed = destoryedCount,
                };
                result.Add(dataUsage);
            }
            return result;
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
where c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Active} 
and c.termId != {Guid.Empty} and c.nodeType = {(int)NodeLevel.AzureFileShareFile} GROUP BY c.termId";
            var queryCount = ExplorerDao.QueryRelatedTermCount(sql); 
            return queryCount;
        }

        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {
            var activeSql = $@"
SELECT LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.timeCreated >= {startTicks}
GROUP BY LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10)
";

            var destroyedSql = $@"
SELECT LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
AND c.destroyedTime >= {startTicks}
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
