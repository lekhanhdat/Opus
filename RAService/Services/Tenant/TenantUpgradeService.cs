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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class TenantUpgradeService : RMServiceBase, ITenantUpgradeService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TenantUpgradeService));

        private IRMTenantUpgradeInfoDao TenantUpgradeInfoDao => PlatformWindsorManager.GetService<IRMTenantUpgradeInfoDao>();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public void SendUpgradeJobMessage()
        {
            try
            {
                var count = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.TenantUpgrade);
                if(count > 0)
                {
                    Logger.Warn($"A upgrade job meessage already exists.");
                }
                var queue = new JobQueueDto
                {
                    JobType = JobType.TenantUpgrade,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = null
                };

                JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while current tenant send upgrade job message. Error: {e}");
            }
        }

        public string RealRunUpgradeJob()
        {
            Logger.Info("Start run tenant upgrade job.");
            var jobId = string.Empty;

            try
            {
                var username = "RM_TS_RunSchedule";
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.TenantUpgrade) > 0;
                jobId = JobMonitorService.CreateJob(JobType.TenantUpgrade, username);
                if (hasRunningJob)
                {
                    Logger.Warn("A running upgrade job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                Logger.Info($"Real run upgrade job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.TenantUpgrade,
                    CommandLine = $"{JobType.TenantUpgrade} {jobId}",
                });

                TenantUpgradeInfoDao.UpdateTenantUpgradeInfoToRunning(TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run upgrade job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }
    }
}
