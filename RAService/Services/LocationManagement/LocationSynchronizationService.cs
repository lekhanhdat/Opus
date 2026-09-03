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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.LocationManagement
{
    [Audit]
    public class LocationSynchronizationService : RMServiceBase, ILocationSynchronizationService
    {
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private RALogger logger = RALogger.GetInstance(typeof(LocationSynchronizationService));




        public string RunSyncLocationTreeToSharePoint(JobRunBy runByType, bool fromTimerJobPage)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalFolderSynchronization,
                    JobRunType = runByType,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = fromTimerJobPage.ToString()
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunSyncLocationTreeToSharePoint,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationTermSynchronisation, Action = AuditAction.RunLocationTermSyncJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public string RealRunSyncLocationTreeToSharePoint(JobRunBy runByType, string jobRunByUser, bool fromTimerJobPage)
        {
            string id = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job
            string runBy = string.Empty;
            if (runByType == JobRunBy.Schedule)
            {
                runBy = "RM_TS_RunSchedule";
            }
            else
            {
                runBy = jobRunByUser;
            }

            //查询当前还没有结束的Term Sync Job
            //List<string> runningPTermSyncJobs = JobMonitorService.GetRunningJobs(JobType.PhysicalTermSynchronization);
            //List<string> runningPFolderSyncJobs = JobMonitorService.GetRunningJobs(JobType.PhysicalFolderSynchronization);

            //Term Sync Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            //bool isSkip = false;
            //if (runningPTermSyncJobs.Count > 0 || runningPFolderSyncJobs.Count > 0)
            //{
            //    isSkip = true;
            //}
            //if (!isSkip)
            //{
            //新起线程起Job
            //StartPTermSyncJob();
            id = StartPFolderSyncJob(runByType, runBy);
            //}
            //else
            //{
            //logger.Info(I18NEntity.GetString("Skipped this job. A location synchronisation job is already running."));
            //JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "Skipped this job. A location synchronisation job is already running.");
            //}

            return id;
        }

        //private void StartPTermSyncJob()
        //{


        //    IJobMessageSender jobMsgSender = new JobMessageSender();
        //    jobMsgSender.SendMessage(new JobQueueMessage()
        //    {
        //        JobType = JobType.PhysicalTermSynchronization,
        //        CommandLine = string.Format("{0} {1}", JobType.PhysicalTermSynchronization, ""),
        //    });

        //}

        private string StartPFolderSyncJob(JobRunBy runByType, string runBy)
        {
            var jobId = JobMonitorService.CreateJob(JobType.PhysicalFolderSynchronization, runBy);
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobType = JobType.PhysicalFolderSynchronization,
                JobId = jobId,
                RunBy = runByType,
                CommandLine = string.Format("{0} {1} {2}", JobType.PhysicalFolderSynchronization, jobId, runBy),
            });
            return jobId;
        }

    }
}
