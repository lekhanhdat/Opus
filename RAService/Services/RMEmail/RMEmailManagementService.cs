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
using AvePoint.RA.Contract.RMEmail;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMEmail
{
    public class RMEmailManagementService : RMServiceBase, IRMEmailManagementService
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMEmailManagementService));

        private static readonly HashSet<int> s_needSendEmailJobs = new()
        {
            (int)JobType.RecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.EXORecordsDisposal,
            (int)JobType.PhysicalRecordsDisposal,
            (int)JobType.BoxRecordsDisposal,
            (int)JobType.ApprovalProcessArchive,
            (int)JobType.GoogleRecordsDisposal,
            (int)JobType.TeamsRecordsDisposal,
        };

        private static IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private static IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private static IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();


        public bool CheckJobNeedSendEmail(string jobId)
        {
            try
            {
                var job = JobMonitorDao.GetJob(jobId);
                return s_needSendEmailJobs.Contains(job.JobType);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while check job need send email. Error: {e}");
                return false;
            }
        }

        public Task<string> RealRunSendEmailJob(string prefix)
        {
            s_logger.Info("Start run send email job.");
            var jobId = string.Empty;

            try
            {
                var username = "RM_TS_RunSchedule";
                jobId = JobMonitorService.CreateJob(JobType.SendEmailJob, username);

                s_logger.Info($"Real run send email job: [{jobId}].");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.SendEmailJob,
                    CommandLine = $"{JobType.SendEmailJob} {jobId} {prefix}",
                });
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while real run send email job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return System.Threading.Tasks.Task.FromResult(jobId);
        }

        public bool SendEmailJobMessageToQueue(string prefix)
        {
            var id = string.Empty;
            var runJobUserName = "RM_TS_RunSchedule";

            try
            {
                var queue = new JobQueueDto
                {
                    JobType = JobType.SendEmailJob,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = runJobUserName,
                    Parameters = prefix
                };

                id = JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while run email schedule job. Error: {e}");
            }
            return !string.IsNullOrEmpty(id);
        }
    }
}
