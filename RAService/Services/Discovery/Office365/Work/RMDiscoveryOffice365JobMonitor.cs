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
using AngleSharp.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.Data.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365JobMonitor : RMDiscoveryOffice365Worker
    {

        private readonly IRMDiscoveryOffice365ConfigurationService _configurationService;

        private readonly IJobMonitorService _jobMonitorService;

        private readonly IJobQueueService _jobQueueService;

        public RMDiscoveryOffice365JobMonitor() : base()
        {
            _configurationService = new RMDiscoveryOffice365ConfigurationService();
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        }

        public async Task MonitorAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Running);
                if (!has)
                {
                    return;
                }

                if (mainJob.Version != RMDiscoveryJobVersion.V5 && HasRunningAnalysisJob(mainJob))
                {
                    return;
                }

                _logger.Info($"Start check job [{mainJob.Id}].");
                if (mainJob.Version == RMDiscoveryJobVersion.V1)
                {
                    await CheckTimeoutJobsAsync(mainJob);
                    await CalculateJobsStatusAsync(mainJob);
                }

                if (mainJob.Status == RMDiscoveryJobStatus.Running)
                {
                    await MonitorDiscoveryJobsAsync(mainJob);
                }

                if (mainJob.Version != RMDiscoveryJobVersion.V1 && mainJob.Version != RMDiscoveryJobVersion.V5)
                {
                    if (!await _jobDao.HasDiscoveryJobAsync(mainJob.Id, 
                            RMDiscoveryJobStatus.Preparing,
                            RMDiscoveryJobStatus.Waiting,
                            RMDiscoveryJobStatus.Pending,
                            RMDiscoveryJobStatus.Running))
                    {
                        _configurationService.SendNextVersionDiscoveryAnalysisJob(mainJob.Id);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while monitor discovery job. Error: {e}");
            }
        }

        private async Task CheckTimeoutJobsAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            try
            {
                const int runningTimeoutHour = 1;
                const int pendingTimeoutHour = 3;
                var runningTimeoutTicks = DateTime.UtcNow.AddHours(0 - runningTimeoutHour).Ticks;
                var pendingTimeoutTicks = DateTime.UtcNow.AddHours(0 - pendingTimeoutHour).Ticks;

                var runningTimeoutJobs = await _jobDao.GetTimeoutAnalysisJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Running, runningTimeoutTicks);
                if (runningTimeoutJobs.Any())
                {
                    _logger.Info($"These jobs [{string.Join(", ", runningTimeoutJobs)}] whose status is [{RMDiscoveryJobStatus.Running}] timeout.");
                }

                var pendingTimeoutJobs = await _jobDao.GetTimeoutAnalysisJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Pending, pendingTimeoutTicks);
                if (pendingTimeoutJobs.Any())
                {
                    _logger.Info($"These jobs [{string.Join(", ", pendingTimeoutJobs)}] whose status is [{RMDiscoveryJobStatus.Pending}] timeout.");
                }

                var willUpdateJobs = runningTimeoutJobs.Concat(pendingTimeoutJobs).ToList();
                if (willUpdateJobs.Any())
                {
                    willUpdateJobs.ForEach(item =>
                    {
                        item.Status = RMDiscoveryJobStatus.Timeout;
                        item.EndTime = DateTime.UtcNow.Ticks;
                    });

                    await _jobDao.AddOrUpdateAnalysisJobAsync(willUpdateJobs.ToArray());
                    _logger.Info($"Successful set jobs [{willUpdateJobs.Count}] status to timeout.");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while check time out jobs in main job [{mainJob.Id}]. Error: {e}");
            }
        }

        private async Task CalculateJobsStatusAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            try
            {
                var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Completing);
                foreach (var completingDiscoveryJob in completingDiscoveryJobs)
                {
                    if (await _jobDao.HasProcessingAnalysisJobAsync(completingDiscoveryJob.Id))
                    {
                        continue;
                    }

                    var analysisJobStatusDic = await _jobDao.GetAnalysisCompletedStatusAsync(completingDiscoveryJob.Id);
                    _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                    _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                    _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                    completingDiscoveryJob.EndTime = DateTime.UtcNow.Ticks;
                    completingDiscoveryJob.Status = RMDiscoveryJobStatus.Finished;
                    if (finishedCount > 0 && completingDiscoveryJob.SiteCount - finishedCount > 0)
                    {
                        completingDiscoveryJob.Status = RMDiscoveryJobStatus.Exception;
                    }
                    else if (failedCount + timeoutCount > 0)
                    {
                        completingDiscoveryJob.Status = RMDiscoveryJobStatus.Failed;
                    }
                    await _jobDao.AddOrUpdateDiscoveryJobAsync(completingDiscoveryJob);
                    _logger.Info($"The discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] [{completingDiscoveryJob.Status}].");
                }

                if (!await _jobDao.HasProcessingDiscoveryJobAsync(mainJob.Id))
                {
                    _configurationService.SendCalculateJob(mainJob.Id);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while check job status in main job [{mainJob.Id}]. Error: {e}");
            }
        }

        private async Task MonitorDiscoveryJobsAsync(RMDiscoveryOffice365MainJob mainJob)
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
                                FinishedTimeBegin = discoveryJob.Status == RMDiscoveryJobStatus.Completing ? DateTimeOffset.MinValue : new DateTime(lastCheckedTime, DateTimeKind.Utc),
                                FinishedTimeEnd = discoveryJob.Status == RMDiscoveryJobStatus.Completing ? DateTimeOffset.MaxValue : new DateTime(checkTime, DateTimeKind.Utc),
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
                                        analysisJobInfo.Status = mainJob.Version == RMDiscoveryJobVersion.V1 ? RMDiscoveryJobStatus.Waiting : RMDiscoveryJobStatus.Pending;
                                        analysisJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
                                        analysisJobInfo.FailedCause = RMDiscoveryJobFailedCause.None;
                                        break;
                                    case RMDiscoveryJobStatus.Failed:
                                    case RMDiscoveryJobStatus.Exception:
                                        analysisJobInfo.Status = RMDiscoveryJobStatus.Pending;
                                        if (mainJob.Version != RMDiscoveryJobVersion.V1)
                                        {
                                            analysisJobInfo.FailedCause = RMDiscoveryJobFailedCause.DiscoveryFailed;
                                        }
                                        analysisJobInfo.EndTime = DateTime.UtcNow.Ticks;
                                        break;
                                }

                                _logger.Info($"Change analysis job [{analysisJobInfo.Id}] status to [{analysisJobInfo.Status}] in the discovery job [{discoveryJob.Id}], CompleteSubJobStatus:[{completeSubJob.Status}], FailedCause:[{analysisJobInfo.FailedCause}].");
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

        private async IAsyncEnumerable<RMDiscoveryOffice365DiscoveryJob> GetNeedMonitorDiscoveryJobsAsync(RMDiscoveryOffice365MainJob mainJob)
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
                            await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        }
                        yield return discoveryJob;
                        continue;
                    case RMDiscoveryJobStatus.Exception:
                    case RMDiscoveryJobStatus.Finished:
                    case RMDiscoveryJobStatus.Failed:
                        discoveryJob.Status = RMDiscoveryJobStatus.Completing;
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        yield return discoveryJob;
                        continue;
                }
            }

            //var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Pending, RMDiscoveryJobStatus.Completing);
            //foreach (var completingDiscoveryJob in completingDiscoveryJobs)
            //{
            //    yield return completingDiscoveryJob;
            //}
        }

        private bool HasRunningAnalysisJob(RMDiscoveryOffice365MainJob mainJob)
        {
            var jobType = mainJob.Version.ToOffice365JobType();
            var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, jobType);
            var jobCount = _jobMonitorService.GetRunningJobsCount(jobType);
            return queueCount + jobCount > 0;
        }
    }
}
