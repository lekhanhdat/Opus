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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class EnforceRuleActionJobTaskExecutor : ITaskExecutor
    {
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IRALogger mLogger = RALogger.GetInstance(typeof(EnforceRuleActionJobTaskExecutor));



        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, ExcuteTaskAsync);
                }
                
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while update disposal job status,ERROR:{0}", ex.ToString());
            }
        }
        private async System.Threading.Tasks.Task ExcuteTaskAsync()
        {
            try
            {
                var runningJobScopeIds = RMJobService.GetRunningJobsScopeId(Contract.JobMonitor.JobType.DisposalActivityManagement);
                //mLogger.Debug("get running disposal job count:{0}", runningJobScopeIds.Count);

                if (runningJobScopeIds.Count > 0)
                {
                    mLogger.Info("disposal job keys is :{0}", string.Join(",", runningJobScopeIds));
                    IEnumerable<IGrouping<string, SOJob>> groups = null;
                    try
                    {
                        groups = RMJobService.ValidateJobs(runningJobScopeIds).GroupBy(j => j.ProfileId);
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("get disposal error:{0}", e.ToString());
                    }
                    if (groups == null)
                    {
                        mLogger.Warn("disposal group error group is null.");
                        return;
                    }
                    foreach (var group in groups)
                    {
                        string jobId = string.Empty;
                        mLogger.Info("begin to scan jobkey:{0}", group.Key);
                        jobId = RMJobService.GetJobFakeidByKey(group.Key);
                        mLogger.Info("begin to sync job,ID:{0}", jobId);
                        SOJob scanJob = null;
                        SOJob backupJob = null;
                        SOJob physicalJob = null; 
                        scanJob = group.Where(j => j.Type == (int)JobTypes.ArchiverScan || j.Type == (int)JobTypes.ExchangeArchiverScan).FirstOrDefault();
                        backupJob = group.Where(j => j.Type == (int)JobTypes.ArchiverBackup || j.Type == (int)JobTypes.ExchangeArchiverBackup).FirstOrDefault();
                        physicalJob = group.Where(j => j.Type == (int)JobTypes.PhysicalRecords).FirstOrDefault();
                        if (physicalJob != null)
                        {
                            JobStatus finallyStatus = JobMonitorService.ConvertToRAStatus(physicalJob.State);
                            if (new List<JobStatus>() { JobStatus.Failed, JobStatus.FinishWithException }.Contains(finallyStatus))
                            {
                                var summary = await RMJobService.GetDisposalJobSummaryAsync(physicalJob);
                                RMJobService.UpdateJob(
                                   jobId,
                                   (int)physicalJob.Progress,
                                   (int)finallyStatus,
                                   physicalJob.FinishTime,
                                   summary.Comment);
                            }
                            else
                            {
                                RMJobService.UpdateJob(
                                jobId,
                                (int)physicalJob.Progress,
                                (int)finallyStatus,
                                physicalJob.FinishTime);
                            }
                            RMJobService.UpdateArchiverJob(physicalJob, jobId);
                        }
                        else if (scanJob == null)
                        {
                            //TODO　
                            mLogger.Warn("scan job is null.");
                        }
                        else if (backupJob == null)
                        {
                            if (new int[] { (int)AvePoint.Common.JobState.Failed, (int)AvePoint.Common.JobState.Skiped, (int)AvePoint.Common.JobState.Stopped }.Contains(scanJob.State))
                            { //只有scan job 且为终止状态(不起archiver job),更新FinishTime
                                var summary = await RMJobService.GetDisposalJobSummaryAsync(scanJob);
                                RMJobService.UpdateJob(jobId,
                                    CalcProgress(scanJob.Progress),
                                    (int)JobMonitorService.ConvertToRAStatus(scanJob.State),
                                    scanJob.FinishTime,
                                    summary.Comment);
                            }
                            else
                            {
                                RMJobService.UpdateJob(jobId,
                                   CalcProgress(scanJob.Progress),
                                   (int)JobMonitorService.ConvertToRAStatus((int)AvePoint.Common.JobState.InProgress),
                                   0);
                            }
                            RMJobService.UpdateArchiverJob(scanJob, jobId);
                        }
                        else if (backupJob != null)
                        {
                            JobStatus finallyStatus = JobMonitorService.ConvertToRAStatus(backupJob.State);
                            if (JobMonitorService.ConvertToRAStatus(backupJob.State) == JobStatus.Finished &&
                                JobMonitorService.ConvertToRAStatus(scanJob.State) == JobStatus.FinishWithException)
                            {
                                finallyStatus = JobStatus.FinishWithException;
                            }
                            //TODO 目前除了scan FinishWithException时，其他都按照backup设置最终状态

                            if (new List<JobStatus>() { JobStatus.Failed, JobStatus.FinishWithException }.Contains(finallyStatus))
                            {
                                var summary = await RMJobService.GetDisposalJobSummaryAsync(backupJob);
                                RMJobService.UpdateJob(
                                   jobId,
                                   CalcProgress(scanJob.Progress + backupJob.Progress),
                                   (int)finallyStatus,
                                   backupJob.FinishTime,
                                   summary.Comment);
                            }
                            else
                            {
                                RMJobService.UpdateJob(
                                jobId,
                                CalcProgress(scanJob.Progress + backupJob.Progress),
                                (int)finallyStatus,
                                backupJob.FinishTime);
                            }
                            RMJobService.UpdateArchiverJob(scanJob, jobId);
                            RMJobService.UpdateArchiverJob(backupJob, jobId);
                        }
                        var jobStatus = RMJobService.GetJobStatus(jobId);
                        if(JobServiceUtility.IsFinalState((int)jobStatus))
                        {
                            TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { jobId });
                        }
                        mLogger.Info("finish to sync job,ID:{0}", jobId);
                    }

                    await TelemetryContext.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while excute disposal job task,ERROR:{0}", ex.ToString());
            }
            
        }

        private int CalcProgress(double progress)
        {
            double dProgress = progress / 2;
            if (dProgress > 0 && dProgress < 1)
            {
                return (int)Math.Ceiling(dProgress);//避免scan job 1% inprogress时，主job进度为0%;
            }
            return (int)(dProgress);
        }
    }
}
