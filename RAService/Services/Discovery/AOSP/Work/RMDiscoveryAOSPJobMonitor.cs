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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using Cloud.Sdk.Data.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work
{
    public class RMDiscoveryAOSPJobMonitor : RMDiscoveryAOSPWorker
    {
        private readonly IRMDiscoveryAOSPConfigurationService _configurationService;

        private readonly IJobMonitorService _jobMonitorService;

        private readonly IJobQueueService _jobQueueService;

        public RMDiscoveryAOSPJobMonitor() : base()
        {
            _configurationService = new RMDiscoveryAOSPConfigurationService();
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        }

        public async Task MonitorAsync()
        {
            try
            {
                var (has, mainJobs) = await _jobDao.TryGetMainJobsAsync(RMDiscoveryJobStatus.Running);
                if (!has)
                {
                    return;
                }

                if (HasRunningAnalysisJob())
                {
                    _logger.Info("There are running discovery jobs, skipping monitoring.");
                    return;
                }

                foreach (var mainJob in mainJobs)
                {
                    try
                    {
                        _logger.Info($"Start check job [{mainJob.Id}].");

                        if (mainJob.Status == RMDiscoveryJobStatus.Running)
                        {
                            await MonitorDiscoveryJobsAsync(mainJob);
                        }

                        if (!await _jobDao.HasDiscoveryJobAsync(mainJob.Id,
                                RMDiscoveryJobStatus.Preparing,
                                RMDiscoveryJobStatus.Waiting,
                                RMDiscoveryJobStatus.Pending,
                                RMDiscoveryJobStatus.Running))
                        {
                            _configurationService.SendDiscoveryAnalysisJob(mainJob.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while monitor main job [{mainJob.Id}]. Error: {e}");
                    }
                }
    
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while monitor discovery job. Error: {e}");
            }
        }

        private async Task MonitorDiscoveryJobsAsync(RMDiscoveryAOSPMainJob mainJob)
        {
            try
            {
                await foreach (var discoveryJob in GetNeedMonitorDiscoveryJobsAsync(mainJob))
                {
                    try
                    {
                        var lastCheckedTime = new DateTime(discoveryJob.LastCheckedTime).AddMinutes(-5).Ticks;
                        var checkTime = DateTime.UtcNow.Ticks;
                        var completedSubJobs = new List<SubJobModel>();

                        for (var i = 1; ; i++)
                        {
                            completedSubJobs = await _ieApiClient.JobService.GetSubJobsAsync(discoveryJob.RealId, new GetSubJobListRequest
                            {
                                FinishedTimeBegin = discoveryJob.Status == RMDiscoveryJobStatus.Completing ? DateTime.MinValue : new DateTime(lastCheckedTime, DateTimeKind.Utc),
                                FinishedTimeEnd = discoveryJob.Status == RMDiscoveryJobStatus.Completing ? DateTime.MaxValue : new DateTime(checkTime, DateTimeKind.Utc),
                                Page = i,
                                Size = 100,
                            });
                            foreach (var completeSubJob in completedSubJobs)
                            {
                                var (has, analysisJobInfo) = await _jobDao.TryGetAnalysisJobAsync(discoveryJob.Id, new Guid(completeSubJob.ObjectId), RMDiscoveryJobStatus.Preparing);
                                if (!has)
                                {
                                    continue;
                                }

                                switch (completeSubJob.Status.ToOpusDiscoveryJobStatus())
                                {
                                    case RMDiscoveryJobStatus.Finished:
                                        analysisJobInfo.Status = RMDiscoveryJobStatus.Pending;
                                        analysisJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
                                        analysisJobInfo.Comment = string.Empty;
                                        break;
                                    case RMDiscoveryJobStatus.Failed:
                                    case RMDiscoveryJobStatus.Exception:
                                        analysisJobInfo.Status = RMDiscoveryJobStatus.Failed;
                                        analysisJobInfo.FailedCause = RMDiscoveryJobFailedCause.DiscoveryFailed;
                                        analysisJobInfo.EndTime = DateTime.UtcNow.Ticks;
                                        analysisJobInfo.Comment = completeSubJob.StatusDetail ?? string.Empty;
                                        break;
                                }

                                _logger.Info($"Change analysis job [{analysisJobInfo.Id}] status to [{analysisJobInfo.Status}] in the discovery job [{discoveryJob.Id}].");
                                await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJobInfo);
                            }

                            discoveryJob.LastCheckedTime = checkTime;
                            await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);

                            if (!completedSubJobs.Any() || completedSubJobs.Count < 100)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to monitor discovery job [{discoveryJob.Id}]. Error: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while monitor discovery jobs. Error: {e}");
            }
        }

        private async IAsyncEnumerable<RMDiscoveryAOSPDiscoveryJob> GetNeedMonitorDiscoveryJobsAsync(RMDiscoveryAOSPMainJob mainJob)
        {
            var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Pending, RMDiscoveryJobStatus.Running);
            _logger.Info($"The number of jobs for pending and running is [{discoveryJobs.Count}] in the main job [{mainJob.Id}].");

            foreach (var discoveryJob in discoveryJobs)
            {
                var ieJobInfo = await _ieApiClient.JobService.GetJobHistoryAsync(discoveryJob.RealId);
                var ieJobStatus = ieJobInfo.Status.ToOpusDiscoveryJobStatus();
                if (ieJobStatus == RMDiscoveryJobStatus.None)
                {
                    _logger.Info($"The ie job [{ieJobInfo.Id}] status is none. Skipped check it.");
                    continue;
                }

                if (discoveryJob.Status != ieJobStatus)
                {
                    _logger.Info($"The ie job [{ieJobInfo.Id}] change job status [{discoveryJob.Status}] to [{ieJobStatus}].");
                }

                switch (ieJobStatus)
                {
                    case RMDiscoveryJobStatus.Pending:
                        continue;
                    case RMDiscoveryJobStatus.Running:
                        if (discoveryJob.Status == RMDiscoveryJobStatus.Pending)
                        {
                            discoveryJob.Status = RMDiscoveryJobStatus.Running;
                            discoveryJob.Comment = string.Empty;
                            await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        }
                        yield return discoveryJob;
                        continue;
                    case RMDiscoveryJobStatus.Exception:
                    case RMDiscoveryJobStatus.Finished:
                        discoveryJob.Status = RMDiscoveryJobStatus.Completing;
                        discoveryJob.Comment = string.Empty;
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        yield return discoveryJob;
                        continue;
                    case RMDiscoveryJobStatus.Failed:
                        await _jobDao.ChangeAnalysisJobsStatusAsync(discoveryJob.Id, RMDiscoveryJobStatus.Failed, true, RMDiscoveryJobFailedCause.DiscoveryFailed, RMDiscoveryJobStatus.Preparing);
                        if (await _jobDao.HasProcessingAnalysisJobAsync(discoveryJob.Id))
                        {
                            discoveryJob.Status = RMDiscoveryJobStatus.Completing;
                            discoveryJob.Comment = ieJobInfo.JobMessage ?? string.Empty;
                        }
                        else
                        {
                            discoveryJob.Status = RMDiscoveryJobStatus.Failed;
                            discoveryJob.EndTime = DateTime.UtcNow.Ticks;
                            discoveryJob.Comment = ieJobInfo.JobMessage ?? string.Empty;
                        }
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        continue;
                }
            }
        }

        private bool HasRunningAnalysisJob()
        {
            var jobType = JobType.DiscoveryAOSPJob;
            var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, jobType);
            var jobCount = _jobMonitorService.GetRunningJobsCount(jobType);
            return queueCount + jobCount > 0;
        }
    }
}
