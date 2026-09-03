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
using AvePoint.Api.Contract;
using AvePoint.Api.Service.Implement;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.Web.Common.Utils;
using Cloud.sdk.Data.Opus.Migration;
using DocAveOnline.WebApi.Contracts;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Api.Web.ApiControllers
{
    [Route("api/archivermigration/[action]")]
    //[Authorize]
    [ApiController]
    public class ArchiverMigrationApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverMigrationApiController));
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();
        private ArchiverService ArchiverService { get { return new ArchiverService(); } }
        
        protected IRMCacheManager RMCacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();


        [HttpPost]
        public async Task<BooleanResult> ExistsOpusAppProfile(IEnumerable<String> o365tenantIDs)
        {
            var result = new BooleanResult();
            try
            {
                if(o365tenantIDs == null || o365tenantIDs.Count()  == 0)
                {
                    result.Value = true;
                }
                else
                {
                    logger.Info($"check if all those office365 exists app: {string.Join(',', o365tenantIDs)}");
                    var allAppProfiles = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId);
                    if(allAppProfiles != null && allAppProfiles.Count > 0)
                    {
                        var hasAppTenants = new HashSet<Guid>();
                        foreach (var app in allAppProfiles)
                        {
                            if(Guid.TryParse(app?.TenantId, out var tenantId))
                            {
                                hasAppTenants.Add(tenantId);
                            }
                        }

                        var noAppTenants = o365tenantIDs.Where(id => !hasAppTenants.Contains(new Guid(id)));
                        result.Value = noAppTenants.Count() == 0;
                        result.ErrorMessage = string.Join(',', noAppTenants);
                        logger.Info($"those office365 not exists app: {string.Join(',', noAppTenants)}");
                    }
                }
                
            }
            catch (Exception e)
            {
                var errorMessage = $"Get opus app profiles failed, {e}";
                logger.Error(errorMessage);
                result.ErrorCode = ErrorCode.UnExpectedException;
                result.ErrorMessage = errorMessage;
            }

            return result;
        }

        [HttpPost]
        public async Task<CreateJobResult> CreateJob([Bind("ExportLocationId")] ArchiverMigrationJobSettings jobSettings)
        {
            var result = new CreateJobResult();
            try
            {
                logger.Info($"start create Cloud Archiver Migration job: {TenantLocalValue.LogonGroupId}");
                if (await ArchiverService.OpusStorageOptimizationEnabled())
                {
                    result.ErrorCode = ErrorCode.UnExpectedException;
                    result.ErrorMessage = "AlreadyNewOpus";
                    return result;
                }

                var runningJobs = JobMonitorService.GetRunningJobs(JobType.CloudArchiverMigration);
                if (runningJobs.Count > 0)
                {
                    logger.Info($"exists Cloud Archiver Migration running job: {string.Join(',', runningJobs)}");
                    result.JobId = runningJobs.FirstOrDefault();
                    result.ErrorMessage = "MigrationJobIsRunning";
                    return result;
                }

                if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
                {
                    var tenantInfo = TenantService.GetTenantInfo(TenantLocalValue.LogonGroupId);
                    TenantLocalValue.LogonUserEmail = tenantInfo.RegisterEmail;
                }

                var jobId = JobMonitorService.GenerateJobId(JobType.CloudArchiverMigration);
                result.JobId = jobId;

                ArchiverMigratedJobMessage message = new()
                {
                    ArchiverMigrationJobSettings = jobSettings,
                    JobId = jobId,
                };
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.CloudArchiverMigration,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = RA.Common.Global.Utils.SerializerHelper.SerializeByJsonConvert(message),
                };
                JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                var errorMessage = $"Create archiver migration job failed, {e}";
                logger.Error(errorMessage);
                result.ErrorCode = ErrorCode.UnExpectedException;
                result.ErrorMessage = errorMessage;
            }

            return result;
        }

        [HttpGet]
        public async Task<JobStatusResult> GetJobStatus(string jobId)
        {
            return await InnerGetJobStatus(jobId, JobType.CloudArchiverMigration);
        }

        [HttpGet]
        public async Task<JobStatusResult> GetManualApproveJobStatus(string jobId)
        {
            return await InnerGetJobStatus(jobId, JobType.ManualApprovalTimer);
        }

        private async Task<JobStatusResult> InnerGetJobStatus(string jobId, JobType jobType)
        {
            var result = new JobStatusResult();
            try
            {
                var jobInfo = await JobMonitorService.GetJobAsync(jobId);
                if (jobInfo == null || jobInfo.JobTypeCode != (int)jobType)
                {
                    var messages = JobQueueService.GetDBJobMessage();
                    var jqDto = messages.FirstOrDefault(m => m.JobType == jobType);
                    if (jqDto != null)
                    {
                        result.Progress = 0;
                        result.Status = JobStatus.Pending;
                        result.Comment = "";
                    }
                    else
                    {
                        //此处逻辑主要是为了防止job monitor里面没有，但是从job queue里面取数据，恰巧queue message被取出去的case. Sleep 3s后retry，如果取不到，再给DAO返回异常.
                        Thread.Sleep(3000);
                        jobInfo = await JobMonitorService.GetJobAsync(jobId);
                        if (jobInfo != null)
                        {
                            logger.Info($"Retry GetJobAsync success.JobId:{jobId}.");
                            result.Progress = jobInfo.Progress;
                            if ((jobInfo.MigrationJobStatus != null && (jobInfo.MigrationJobStatus & (int)ArchiverMigrationJobStatus.PreparingDownloadReportBlob) == (int)ArchiverMigrationJobStatus.PreparingDownloadReportBlob))
                            {
                                logger.Info("Prepareing download report blob , Job inProgress");
                                result.Status = JobStatus.InProgress;
                            }
                            else
                            {
                                result.Status = DataContractConvertUtil.ConvertToStatus((RA.Contract.RMWeb.JobMonitor.JobStatus)jobInfo.Status);
                            }

                            if (result.Status == JobStatus.InProgress)
                            {
                                result.Progress = Math.Min(jobInfo.Progress, 99);
                            }
                            result.Comment = jobInfo.Comment;
                        }
                        else
                        {
                            var errorMessage = $"Can't find {jobType} job : {jobId}";
                            logger.Error(errorMessage);
                            result.ErrorCode = ErrorCode.UnExpectedException;
                            result.ErrorMessage = errorMessage;
                        }
                    }
                }
                else
                {
                    result.Progress = jobInfo.Progress;
                    if ((jobInfo.MigrationJobStatus != null && (jobInfo.MigrationJobStatus & (int)ArchiverMigrationJobStatus.PreparingDownloadReportBlob) == (int)ArchiverMigrationJobStatus.PreparingDownloadReportBlob))
                    {
                        logger.Info("Prepareing download report blob , Job inProgress");
                        result.Status = JobStatus.InProgress;
                    }
                    else
                    {
                        result.Status = DataContractConvertUtil.ConvertToStatus((RA.Contract.RMWeb.JobMonitor.JobStatus)jobInfo.Status);
                    }
                    
                    if(result.Status == JobStatus.InProgress)
                    {
                        result.Progress = Math.Min(jobInfo.Progress, 99);
                    }
                    result.Comment = jobInfo.Comment;
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Get {jobType} job status failed, {e}";
                logger.Error(errorMessage);
                result.ErrorCode = ErrorCode.UnExpectedException;
                result.ErrorMessage = errorMessage;
            }

            return result;
        }

        [HttpGet]
        public async Task<OpusTenantStateForMigration> CheckOpusTenantState()
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            var tenantInfo = TenantService.GetTenantInfo(tenantId);
            if (tenantInfo == null || tenantInfo.Status != TenantStatus.Normal)
            {
                logger.Warn($"Tenant status : NotInitialized  - {tenantId}");
                return OpusTenantStateForMigration.NotInitialized;
            }
            TenantLocalValue.LogonUserEmail = tenantInfo.RegisterEmail;

            bool isSyncing = false;
            if (NeedCreateSyncNodeJobBeforeMigration(tenantInfo, out isSyncing))
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.InitNodesFromAOS, $"{tenantId}", TimeSpan.FromMinutes(5)))
                {
                    if (NeedCreateSyncNodeJobBeforeMigration(tenantInfo, out isSyncing))
                    {
                        logger.Info($"create job for InitNodesFromAOS - {tenantId}");
                        var syncNodeJobId = RemoteNodeService.CreateSyncAllNodesJob();
                        logger.Info($"InitNodesFromAOS job created : {syncNodeJobId}");

                        return OpusTenantStateForMigration.NotSyncRemoteNodes;
                    }
                }
            }

            if(isSyncing)
            {
                return OpusTenantStateForMigration.NotSyncRemoteNodes;
            }

            return OpusTenantStateForMigration.Ready;
        }

        private int GetArchiverMigrationPreRunSRNJobPeriodInMinutes(int defaultValue = 60)
        {
            var preRunSRNJobPeriod = KeyValueService.Get(RMKeyValuesConstants.ArchiverMigrationPreRunSRNJobPeriodInMinutes)?.Value;
            if(preRunSRNJobPeriod == null)
            {
                logger.Info($"No ArchiverMigrationPreRunSRNJobPeriodInMinutes set. use default {defaultValue} minutes.");
            }
            else if(!int.TryParse(preRunSRNJobPeriod, out var periodVal))
            {
                logger.Error($"ArchiverMigrationPreRunSRNJobPeriodInMinutes set as an invalid value: {preRunSRNJobPeriod}");
            }
            else
            {
                logger.Info($"ArchiverMigrationPreRunSRNJobPeriodInMinutes set as: {periodVal} minutes.");
                return periodVal;
            }

            return defaultValue;
        }

        private bool NeedCreateSyncNodeJobBeforeMigration(TenantInfoDto tenantInfo, out bool isSyncing)
        {
            isSyncing = false;
            var syncNodeState = TenantService.GetTenantInitNodeState(tenantInfo.SyncNodeState);
            if ((syncNodeState & RMInitNodeState.Synced) == RMInitNodeState.Synced)
            {
                var runningJobs = JobMonitorService.GetRunningJobs(JobType.SyncNodesFromAOS);
                if (runningJobs.Count == 0)
                {
                    var (jobId, endTime) = JobMonitorService.GetLastFinishedJob(JobType.SyncNodesFromAOS);
                    if (!string.IsNullOrEmpty(jobId) && endTime > 0)
                    {
                        var period = GetArchiverMigrationPreRunSRNJobPeriodInMinutes();
                        if (endTime < DateTime.UtcNow.AddMinutes(-period).Ticks)
                        {
                            logger.Info($"No finish sync node job in 10 min. lastJob: {jobId}, endTime: {endTime}");
                            return true;
                        }
                        else
                        {
                            logger.Info($"Exists finish sync node job in 10 min. lastJob: {jobId}, endTime: {endTime}");
                            return false;
                        }
                    }
                    else
                    {
                        logger.Info($"No running sync node job. current state: {syncNodeState}");
                        return true;
                    }
                }
                else
                {
                    isSyncing = true;
                    logger.Info($"{runningJobs.Count} sync node job running. current state: {syncNodeState}");
                    return false;
                }
            }
            else if ((syncNodeState & RMInitNodeState.Syncing) == RMInitNodeState.Syncing)
            {
                isSyncing = true;
                logger.Info($"No need create sync node job. current state: {syncNodeState}");
                return false;
            }
            else
            {
                logger.Info($"Never ran sync node job. current state: {syncNodeState}");
                return true;
            }
        }

        [HttpGet]
        public async Task<CreateJobResult> CheckNeedRunManualApproveJob()
        {
            var result = new CreateJobResult();
            try
            {
                if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
                {
                    var tenantInfo = TenantService.GetTenantInfo(TenantLocalValue.LogonGroupId);
                    TenantLocalValue.LogonUserEmail = tenantInfo.RegisterEmail;
                }

                var (checkResult, jobId) = await ManualApprovalService.NeedRunManualApproveJob();
                result.JobId = jobId;
                if (checkResult)
                {
                    var groupId = TenantLocalValue.LogonGroupId;
                    JobQueueDto jqDto = new()
                    {
                        JobType = JobType.ManualApprovalTimer,
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = groupId,
                        JobRunByUser = TenantLocalValue.LogonUserEmail,
                        Parameters = jobId
                    };
                     JobQueueService.AddToDBJobQueue(jqDto);
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = ErrorCode.UnExpectedException;
                logger.Error("error occurred while RunManualApprovalJob, ERROR:{0}", ex.ToString());
            }
            return result;
        }

        [HttpGet]
        public Task<bool> InitTenant(string logonUserId)
        {
            return ArchiverService.InitTenantForMigrationJob(logonUserId);
        }

        [HttpGet]
        public Task<MigrationJobReportSASResult> GetJobReport(string jobId)
        {
            return RMCacheManager.Cache.TryGetAsync($"MIGRATION_REPORT_SAS_{jobId}", async () =>
            {
                return await ArchiverService.GetMigrationJobReportSASAsync(jobId);
            }, TimeSpan.FromHours(5));
        }
    }
}