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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.ApproveJob
{
    public class RunApproveJob
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RunApproveJob));

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IJobQueueService jobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private static IJobMonitorService jobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public static IRMSubJobDao subJobDao  => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public RAReturnMessage RunApproveOrRejectJob(ManualApprovalJobParam param) 
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var logonUserId = TenantLocalValue.LogonUserId;
                param.UserId = logonUserId;
                var parameter = SerializerHelper.SerializeByJsonSerializer(param);
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ManualApprovalOrRejectJob,
                    Parameters = parameter,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while RunApprovedOrRejectedJob,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        public async Task<string> RealRunApproveOrRejectJobAsync(string param) 
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            string logonUserId = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalJobParam>(param).UserId;
            try
            {
                var jobType = JobType.ManualApprovalOrRejectJob;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = jobMonitorService.CreateJob(jobType, jobRunByUser, account.UserId);
                subJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);
                List<string> runningJobs = jobMonitorService.GetRunningJobs(JobType.ManualApprovalOrRejectJob);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    jobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1} {2}", jobType, subJobId, logonUserId),
                    });
                }
                else
                {
                    Logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip")); 
                    jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in RealRunApprovedOrRejectedJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks
            };
            if (jobState == JobStatus.Wait)
            {
                subJob.Runable = RecordsConstants.SubJob_Runnable_CanRun;
            }
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            subJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }
    }
}
