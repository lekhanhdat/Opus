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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPJobManagentService : IRMDiscoveryAOSPJobManagentService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPJobManagentService));

        private readonly IRMDiscoveryAOSPJobDao _jobDao = new RMDiscoveryAOSPJobDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMDiscoveryAOSPConfigurationDao _configDao = new RMDiscoveryAOSPConfigurationDao();

        public async Task<RMDiscoveryAOSPLatestJobInfo> GetJobInfoAsync(string o365TenantId)
        {
            try
            {
                if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckAOSPTablesExistsAsync())
                {
                    return new()
                    {
                        Status = RMDiscoveryJobStatus.None,
                        EnableRot = false,
                    };
                }

                var (has, jobInfo) = await _jobDao.TryGetLatestMainJobAsync(o365TenantId);
                if (!has)
                {
                    return new()
                    {
                        Status = RMDiscoveryJobStatus.None,
                        EnableRot = false,
                    };
                }

                var rotConfig = await _configDao.GetByO365TenantIdAsync<RMDiscoveryAOSPRotDefinition>(RMDiscoveryConfigurationType.AOSPROTDefinition, o365TenantId);

                var completedJobs = await _jobDao.GetAnalysisCompletedStatusByMainJobIdAsync(jobInfo.Id);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Pending, out var pendingCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Waiting, out var waitingCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Running, out var runningCount);

                return new RMDiscoveryAOSPLatestJobInfo
                {
                    HasJob = true,
                    Status = jobInfo.Status,
                    JobType = jobInfo.Type,
                    SiteProgressInfo = new RMDiscoveryAOSPJobSiteProgressInfo
                    {
                        NeedProcessCount = jobInfo.SitesCount,
                        SucceedCount = finishedCount,
                        FailedCount = failedCount + timeoutCount,
                        DiscoveredCount = pendingCount + waitingCount + runningCount + finishedCount + failedCount + timeoutCount,
                    },
                    StartTime = (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.StartTime, true)).FormaTime,
                    EndTime = jobInfo.EndTime == 0 ? "0" : (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.EndTime, true)).SimplifyFormatTime,
                    EndTimeLong = jobInfo.EndTime,
                    EnableRot = rotConfig.Enable,
                    Comment = jobInfo.Comment
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured while get job progress info. Error: {e}");
                return new()
                {
                    Status = RMDiscoveryJobStatus.None,
                    EnableRot = false,
                };
            }
        }

        public async Task<RMDiscoveryAOSPLatestJobInfo> GetJobInfoByMainJobIdAsync(string o365TenantId, string mainJobId)
        {
            try
            {
                if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckAOSPTablesExistsAsync())
                {
                    return new()
                    {
                        Status = RMDiscoveryJobStatus.None,
                        EnableRot = false,
                    };
                }

                var (has, jobInfo) = await _jobDao.TryGetMainJobAsync(new Guid(mainJobId));
                if (!has)
                {
                    return new()
                    {
                        Status = RMDiscoveryJobStatus.None,
                        EnableRot = false,
                    };
                }

                var rotConfig = await _configDao.GetByO365TenantIdAsync<RMDiscoveryAOSPRotDefinition>(RMDiscoveryConfigurationType.AOSPROTDefinition, o365TenantId);

                var completedJobs = await _jobDao.GetAnalysisCompletedStatusByMainJobIdAsync(jobInfo.Id);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Pending, out var pendingCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Waiting, out var waitingCount);
                _ = completedJobs.TryGetValue(RMDiscoveryJobStatus.Running, out var runningCount);

                return new RMDiscoveryAOSPLatestJobInfo
                {
                    HasJob = true,
                    Status = jobInfo.Status,
                    JobType = jobInfo.Type,
                    SiteProgressInfo = new RMDiscoveryAOSPJobSiteProgressInfo
                    {
                        NeedProcessCount = jobInfo.SitesCount,
                        SucceedCount = finishedCount,
                        FailedCount = failedCount + timeoutCount,
                        DiscoveredCount = pendingCount + waitingCount + runningCount + finishedCount + failedCount + timeoutCount,
                    },
                    StartTime = (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.StartTime, true)).FormaTime,
                    EndTime = jobInfo.EndTime == 0 ? "0" : (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.EndTime, true)).SimplifyFormatTime,
                    EndTimeLong = jobInfo.EndTime,
                    EnableRot = rotConfig.Enable,
                    Comment = jobInfo.Comment
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured while get job progress info. Error: {e}");
                return new()
                {
                    Status = RMDiscoveryJobStatus.None,
                    EnableRot = false,
                };
            }
        }
    }
}
