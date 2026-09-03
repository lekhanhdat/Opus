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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work
{
    public class RMDiscoveryFSJobMonitor : RMDiscoveryFSWorker
    {
        private readonly IRMDiscoveryFSConfigurationService _discoveryFSConfigurationService;
        private readonly IJobMonitorDao _jobMonitorDao;

        public RMDiscoveryFSJobMonitor() : base()
        {
            _discoveryFSConfigurationService = new RMDiscoveryFSConfigurationService();
            _jobMonitorDao = new JobMonitorDao();
        }

        public async Task MonitorAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Running);
                if (!has)
                {
                    _logger.Info("No running main job found for file system discovery.");
                    return;
                }
                _logger.Info($"Start check job [{mainJob.Id}].");

                await MonitorDiscoveryJobsAsync(mainJob);

                if (!await _jobDao.HasDiscoveryJobAsync(mainJob.Id,
                        RMDiscoveryJobStatus.Preparing,
                        RMDiscoveryJobStatus.Waiting,
                        RMDiscoveryJobStatus.Pending,
                        RMDiscoveryJobStatus.Running))
                {
                    _discoveryFSConfigurationService.SendDiscoveryAnalysisJob(mainJob.Id);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while monitor fs discovery job. Error: {ex}");
            }
        }

        private async Task MonitorDiscoveryJobsAsync(RMDiscoveryFSMainJob mainJob)
        {
            try
            {
                await foreach (var discoveryJob in GetNeedMonitorDiscoveryJobsAsync(mainJob))
                {
                    if (discoveryJob.Status != RMDiscoveryJobStatus.Completing && discoveryJob.Status != RMDiscoveryJobStatus.Failed
                            && discoveryJob.Status != RMDiscoveryJobStatus.Exception)
                        continue;
                    try
                    {
                        var analysisJobInfoes = await _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, int.MaxValue, RMDiscoveryJobStatus.Preparing, RMDiscoveryJobStatus.Pending).ToListAsync();

                        analysisJobInfoes.ForEach(analysisJobInfo =>
                        {
                            if (discoveryJob.Status == RMDiscoveryJobStatus.Completing)
                            {
                                analysisJobInfo.Status = RMDiscoveryJobStatus.Pending;
                            }
                            else if (discoveryJob.Status == RMDiscoveryJobStatus.Failed || discoveryJob.Status == RMDiscoveryJobStatus.Exception)
                            {
                                analysisJobInfo.Status = RMDiscoveryJobStatus.Failed;
                            }

                            analysisJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;

                            _logger.Info($"Change analysis job [{analysisJobInfo.Id}] status to [{analysisJobInfo.Status}] in the discovery scan job [{discoveryJob.Id}].");
                        });

                        await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJobInfoes.ToArray());

                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to monitor discovery scan job [{discoveryJob.Id}]. Error: {e}");
                    }

                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while monitor discovery jobs. Error: {e}");
            }
        }

        private async IAsyncEnumerable<RMDiscoveryFSDiscoveryJob> GetNeedMonitorDiscoveryJobsAsync(RMDiscoveryFSMainJob mainJob)
        {
            var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Pending, RMDiscoveryJobStatus.Running);
            _logger.Info($"The number of jobs for pending and running is [{discoveryJobs.Count}] in the main job [{mainJob.Id}].");

            foreach (var discoveryJob in discoveryJobs)
            {
                var scanJobInfo = _jobMonitorDao.GetJobById(discoveryJob.RealId);
                if (scanJobInfo == null) continue;
                var scanJobStatus = ((Contract.RMWeb.JobMonitor.JobStatus)scanJobInfo.Status).ToOpusDiscoveryJobStatus();
                if (scanJobStatus == RMDiscoveryJobStatus.None)
                {
                    _logger.Info($"The discovery scan job [{scanJobInfo.Id}] status is none. Skipped check it.");
                    continue;
                }

                if (discoveryJob.Status != scanJobStatus)
                {
                    _logger.Info($"The discovery scan job [{scanJobInfo.Id}] change job status [{discoveryJob.Status}] to [{scanJobStatus}].");
                }

                switch (scanJobStatus)
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
                        discoveryJob.Status = RMDiscoveryJobStatus.Completing;
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        yield return discoveryJob;
                        continue;
                    case RMDiscoveryJobStatus.Failed:
                        await _jobDao.ChangeAnalysisJobsStatusAsync(discoveryJob.Id, RMDiscoveryJobStatus.Failed, true, mainJob.Version == RMDiscoveryJobVersion.V1 ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.DiscoveryFailed, RMDiscoveryJobStatus.Preparing);
                        if (await _jobDao.HasProcessingAnalysisJobAsync(discoveryJob.Id))
                        {
                            discoveryJob.Status = RMDiscoveryJobStatus.Completing;
                        }
                        else
                        {
                            discoveryJob.Status = RMDiscoveryJobStatus.Failed;
                            discoveryJob.EndTime = DateTime.UtcNow.Ticks;
                        }
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                        continue;
                }
            }

            var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Pending, RMDiscoveryJobStatus.Completing);
            foreach (var completingDiscoveryJob in completingDiscoveryJobs)
            {
                yield return completingDiscoveryJob;
            }
        }

    }
}
