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
    public class FileSystemDashboardCollector : DashboardCollector
    {

        private static readonly IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();

        private static readonly IRMFunctionSettingDao FunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        public override SourceFlag Flag => SourceFlag.FileSystem;

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            var level = GetClassificationLevel();
            
            var result = new List<RMDashboardDataUsage>();

            var connections = FSConnectionDao.GetAllConnections();

            foreach(var connection in connections)
            {
                var dirpath = connection.UNCPath;
                var parameters = new Dictionary<string, object>
                  {
                          { "@sourceFlag", (int)Flag },
                          { "@recordStatus", (int)RMRecordStatus.Active },
                          { "@nodeType", (int)level },
                          { "@dirPath", dirpath }
                  };

                var createdSql = @"SELECT VALUE COUNT(1) 
                                 FROM c 
                                 WHERE c.sourceFlag = @sourceFlag 
                                    AND c.recordStatus = @recordStatus 
                                    AND c.nodeType = @nodeType 
                                    AND STARTSWITH(c.dirPath, @dirPath )  ";
                var createdCount = ExplorerDao.QueryCount(createdSql, parameters);

                int destoryedCount = 0;
                if (level == (int)NodeLevel.FSFile)
                {
                    var destoryedSql = @"SELECT VALUE COUNT(1) 
                                         FROM c 
                                         WHERE c.sourceFlag = @sourceFlag  
                                            AND c.recordStatus = @recordStatus  
                                            AND c.nodeType = @nodeType  
                                            AND STARTSWITH(c.dirPath, @dirPath)";

                    parameters = new Dictionary<string, object>
                    {
                        { "@sourceFlag", (int)Flag },
                        { "@recordStatus", (int)RMRecordStatus.Destroyed },
                        { "@nodeType", (int)NodeLevel.FSFile },
                        { "@dirPath",  dirpath}
                    };

                    destoryedCount = ExplorerDao.QueryCount(destoryedSql, parameters);
                }
                else
                {
                    //get data from azure table, not support select count
                }

                var dataUsage = new RMDashboardDataUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)Flag,
                    ContainerId = connection.GroupId.ToString(),
                    ScopeId = connection.Id.ToString(),
                    Title = connection.Name,
                    Path = connection.UNCPath,
                    Active = createdCount,
                    Destroyed = destoryedCount,
                };
                result.Add(dataUsage);
            }
    
            return result;
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var level = GetClassificationLevel();
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
where c.sourceFlag = {(int)Flag} and c.recordStatus = {(int)RMRecordStatus.Active} 
and c.termId != {Guid.Empty} and c.nodeType = {level} GROUP BY c.termId";
            return ExplorerDao.QueryRelatedTermCount(sql);
        }


        public static int GetClassificationLevel()
        {
            FunctionSettingDao.TryGet(AvePoint.RA.Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out RMFunctionSetting setting);
            if (setting == null)
            {
                return (int)NodeLevel.FSFile;
            }
            if (Enum.TryParse(setting.SettingInfo, out NodeLevel result))
            {
                return (int)result;
            } 
            return (int)NodeLevel.FSFolder;
        }

        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {

            var level = GetClassificationLevel();

            var activeSql = $@"
SELECT LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.timeCreated >= {startTicks}
AND c.nodeType = {level}
GROUP BY LEFT(TicksToDateTime(c.timeCreated - 621355968000000000), 10)
";

            var destroyedSql = $@"
SELECT LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
c.sourceFlag = {(int)Flag}
AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
AND c.destroyedTime >= {startTicks}
AND c.nodeType = {level}
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
