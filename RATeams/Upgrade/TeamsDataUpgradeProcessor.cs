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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RATeams.Upgrade.CosmosDB;

namespace RATeams.Upgrade
{
    public class TeamsDataUpgradeProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(TeamsDataUpgradeProcessor));

        private static readonly TeamsUpgradeJobManager reportManager = new();

        private static readonly IRMRemoteNodeDao remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static readonly IRMKeyValueDao keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();


        public TeamsDataUpgradeProcessor(string jobId)
        {
            reportManager.Init(jobId, JobType.TeamsDataUpgrade);
        }

        public async Task RunAsync()
        {
            await ProcessMigrateSharePointToTeamsData();
            var status = reportManager.SetJobFinished();
            if (status == JobStatus.Finished)
            {
                var keyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeamsData, Value = "True" };
                await keyValueDao.SaveOrUpdateAsync(keyValueEntity);
            }
        }

        public async Task ProcessMigrateSharePointToTeamsData()
        {
            using(var perform = new PerformanceScope("TeamsDataUpgradeProcessor.ProcessMigrateSharePointToTeamsData","",true))
            {
                await foreach (var sites in remoteNodeDao.GetAllTeamsSiteAsync())
                {
                    foreach (var site in sites)
                    {
                        if (site == null) continue;
                        try
                        {
                            var cosmosDBProcessor = new CosmosDBProcessor();
                            if (await cosmosDBProcessor.PrepareAsync())
                            {
                                await cosmosDBProcessor.ChangeBaseRemoteNode(site, ProcessChangeSourceFlag);
                            }
                            await cosmosDBProcessor.WaitFinishAsync();
                            reportManager.AddRecordReport(new JMConvertStubJobDetails
                            {
                                Action = (int)TeamsUpgradeAction.DataUpgrade,
                                FullPath = site.Url,
                                FinishTime = DateTime.UtcNow.Ticks,
                                Status = JobDetailsStatus.Successful,
                            });
                            reportManager.HasSucceedDetail = true;
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while process change SP to teams data [{site?.Url}]. Error: {e}");
                            reportManager.AddRecordReport(new JMConvertStubJobDetails
                            {
                                Action = (int)TeamsUpgradeAction.DataUpgrade,
                                FullPath = site.Url,
                                FinishTime = DateTime.UtcNow.Ticks,
                                Status = JobDetailsStatus.Failed,
                            });
                            reportManager.HasFailedDetail = true;
                        }
                    }
                }
            }
        }

        private Record ProcessChangeSourceFlag(Record item, RMRemoteNode changeInfo)
        {
            if (item.SourceFlag != (int)SourceFlag.Teams)
            {
                item.SourceFlag = (int)SourceFlag.Teams;
                Guid TeamsId = Guid.Empty;
                if (Guid.TryParse(changeInfo.TeamId, out TeamsId))
                {
                    item.TeamsId = TeamsId;
                }
            }
            return item;
        }
    }
}
