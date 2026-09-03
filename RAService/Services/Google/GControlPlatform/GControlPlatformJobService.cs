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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Foundation;
using OpusJobType = AvePoint.RA.Contract.JobMonitor.JobType;
using FoundationJob = Cloud.Sdk.Data.Nexus.Foundation.Job;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Common;

namespace AvePoint.RA.Service.Services.Google.GControlPlatform;

public class GControlPlatformJobService : GControlPlatformBaseService, IGControlPlatformJobService
{
    public Task<FoundationJob> GetPlatformJobHistory(Guid id)
    {
        Logger.Info($"Get GControl job id: {id}");
        return GControlPlatformApiClient.JobService.GetJobHistory(id);
    }

    public async Task<Guid> CreatePlatformJob(string opusJobId, string name, OpusJobType opusJobType, string jobRunBy)
    {
        var goJobGuid = Guid.NewGuid();

        try
        {
            Logger.Info($"Created GControl job for job type: {opusJobType}, job id: {opusJobId}, gControl job id: {goJobGuid}");

            name = GetNamePlatformJob(opusJobType, name);
            
            var targetObjectName = name switch
            {
                RMConstants.DEFAULT_GOOGLE_USER_GROUP => GetTargetObjectName("GoogleOnePlatformServer.Job.TargetObjectName.DefaultMyDriveNameForOpusJobs"),
                RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP =>GetTargetObjectName("GoogleOnePlatformServer.Job.TargetObjectName.DefaultSharedDriveNameForOpusJobs"),
                _ => name
            };
            var platformJob = new FoundationJob
            {
                Id = goJobGuid,
                Name = name,
                ActorName = jobRunBy == "RM_TS_RunSchedule" ? null : jobRunBy,
                OperatedBy = jobRunBy == "RM_TS_RunSchedule" ? Guid.Empty.ToString() : TenantLocalValue.LogonGroupId,
                CreatedTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow,
                JobCategory = JobCategory.InformationManagement,
                GCCallerType = NexusCallerType.Opus,
                TenantId = TenantLocalValue.LogonGroupId,
                JobType = opusJobType.ConvertToGControlJobType(),
                Module = GCModuleType.InformationLifecycle,
                NextRunTime = DateTime.UtcNow,
                Parameters = [],
                Priority = GeneralPriority.Normal,
                TargetObjectName = targetObjectName,
                ShowInMonitor = true,
                JobStatus = JobStatus.WaitingToRun,
                //AdditionalInfo = opusJobId still not supported in GControl
            };
            CheckIfTheJobNeedToHideInMonitor(platformJob, opusJobType);
            await GControlPlatformApiClient.JobService.CreateJob(platformJob);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error while creating platform job {ex}");
        }

        return goJobGuid;
    }

    /// <summary>
    /// From requirement GOGA-4188, need to hide Dashboard job in job monitor
    /// </summary>
    /// <param name="platformJob"></param>
    /// <param name="opusJobType"></param>
    private void CheckIfTheJobNeedToHideInMonitor(FoundationJob platformJob, OpusJobType opusJobType)
    {
        List<OpusJobType> jobsNeedToHideInMonitor = 
        [
            OpusJobType.Dashboard
        ];
        if (jobsNeedToHideInMonitor.Contains(opusJobType))
        {
            platformJob.ShowInMonitor = false;
        }
    }

    private string GetNamePlatformJob(OpusJobType opusJobType, string name)
    {
        return opusJobType switch
        {
            OpusJobType.TermSynchronization => "Sync classification job",
            OpusJobType.ManualApprovalOrRejectJob => "Records for review job",
            OpusJobType.ExplorerOfflineSearch => "Global Data Sync Job",
            OpusJobType.Dashboard => "SyncDashboardData",
            OpusJobType.SyncNodesFromAOS => "Global data sync node",
            OpusJobType.SyncSecurityContainer => "Global security data sync",
            OpusJobType.ManualApprovalEmailSchedule => "Manual approval settings",
            OpusJobType.MachineLearningReviewApprove => "Approve smart classifications",
            OpusJobType.MachineLearningReviewReclassify => "Reclassify smart classifications",
            OpusJobType.ImportTermStructure => "Import template classification job",
            OpusJobType.GoogleArchiverRetention => "Retention job",
            OpusJobType.GlobalSearchAction => "Reclassification job",
            OpusJobType.DiscoveryGoogleJobV1 => "Discovery and analysis job",
            OpusJobType.DiscoveryGoogleProfileJob => "Analyze profile job",
            _ => name
        };
    }

    private string GetTargetObjectName(string name)
    {
        return JsonConvert.SerializeObject(new TargetObjectNameI18n
        {
            Key = name
        });

    }

    public async Task<bool> UpdatePlatformJob(Guid id, JobStatus jobStatus, DateTime dateTime)
    {
        try
        {
            Logger.Info($"Update GControl job id: {id}");
            return await GControlPlatformApiClient.JobService.UpdateJob(id, new FoundationJob()
            {
                JobStatus = jobStatus,
                FinishedTime = dateTime
            });
        }catch (Exception ex)
        {
            Logger.Error($"Error while updating platform job {id} with status {jobStatus}: {ex}");
            return false;
        }
    }

    public async Task<bool> DeletePlatformJob(Guid id)
    {
        Logger.Info($"Delete GControl job id: {id}");
        return await GControlPlatformApiClient.JobService.DeleteJob(id);
    }
}