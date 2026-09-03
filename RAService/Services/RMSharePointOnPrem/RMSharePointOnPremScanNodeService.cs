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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RMSharePointOnPrem.AuditHandler;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.RMSharePointOnPrem
{
    [Audit]
    public class RMSharePointOnPremScanNodeService : RMServiceBase, IRMSharePointOnPremScanNodeService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSharePointOnPremScanNodeService));

        private static readonly JobType ScanLocalNodeJobType = JobType.SPOnPremScanLocalNodes;

        private static string TenantId => TenantLocalValue.LogonGroupId;

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public string RunScheduleJob(JobRunBy runBy)
        {
            Logger.Info($"Start run sharepoint on-premise scan node job. Run by: [{runBy}].");

            var id = string.Empty;
            var runJobUserName = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";

            try
            {
                var queue = new JobQueueDto
                {
                    JobType = ScanLocalNodeJobType,
                    JobRunType = runBy,
                    TenantGroupId = TenantId,
                    JobRunByUser = runJobUserName,
                    Parameters = null
                };

                id = JobQueueService.AddToDBJobQueue(queue);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while run sharepoint on-premise scan node job. Error: {e}");
            }
            return id;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunSPOnPremScanLocalNodeJob, BeforeHandler = typeof(RMSharePointOnPremSLNBeforeAuditHandler), AfterHandler = typeof(RMSharePointOnPremSLNAfterAuditHanlder))]
        public async Task<string> RunRealTimeJobAsync(JobRunBy runBy)
        {
            Logger.Info("Start run sharepoint on-premise sync node job.");
            var jobId = string.Empty;
            try
            {
                var userName = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";

                var res = JobMonitorService.GetRunningJobsCount(ScanLocalNodeJobType) > 0;
                jobId = JobMonitorService.CreateJob(ScanLocalNodeJobType, userName);
                if (res)
                {
                    Logger.Warn("A Running scan sharepoint on-premise node already exists");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SLN_JobSkip");
                    return jobId;
                }

                var farms = (await SharePointOnPremClient.BrowseFarmsAsync())?.NodeList;

                if (farms?.Count == 0)
                {
                    Logger.Warn("The Farm to be scanned cannot be found.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SLN_NotFoundFarm");
                    return jobId;
                }

                var farmIds = farms.Select(item => item.ID).ToList();
                RunScanLocalNodeJob(jobId, userName, farmIds);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while running sharepoint on-premise scan node job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }
            return jobId;
        }

        private void RunScanLocalNodeJob(string jobId, string runByUserName, List<string> farmIds)
        {
            var subJobWeight = 100d / farmIds.Count;
            var subJobIndex = 0;
            foreach(var farmId in farmIds)
            {
                var subJobId = CreateSubJob(jobId, farmId, subJobIndex++, subJobWeight);
                var args = new Hybrid.Contract.RecordsJobArgs
                {
                    JobId = subJobId,
                    JobType = Hybrid.Contract.JobType.SPOnPremScanNode,
                    TenantId = TenantId,
                    FarmId = farmId,
                };
                HybridSharePointWorkerService.StartSPJob(args);
            }
        }

        private string CreateSubJob(string jobId, string farmId, int subJobIndex, double subJobWeight)
        {
            var subJobId = string.Format(jobId + "_{0:D3}", subJobIndex);
            var subJob = new RMSubJob
            {
                Id = subJobId,
                FarmId = farmId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)ScanLocalNodeJobType,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Weight = subJobWeight,
                Runable = 2
            };
            SubJobDao.CreateJob(subJob);
            Logger.Info($"Create sharepoint on-premise scan node sub job: [{subJob}] successful. Parent Id: [{jobId}], Weight: [{subJobWeight}]");
            return subJobId;
        }
    }
}
