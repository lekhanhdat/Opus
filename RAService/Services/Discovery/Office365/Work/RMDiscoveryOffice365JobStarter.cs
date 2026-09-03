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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365JobStarter : RMDiscoveryOffice365Worker
    {
        public const int MAX_ANALYSIS_JOBS_ASYNC = 1;

        private readonly IRMSubJobDao _subJobDao;

        private readonly RMDiscoveryOffice365ConfigurationService _configurationService;

        public RMDiscoveryOffice365JobStarter() :base()
        {
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _configurationService = new RMDiscoveryOffice365ConfigurationService();
        }

        public async Task StartAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync();
                if (!has)
                {
                    return;
                }

                switch (mainJob.Version)
                {
                    case RMDiscoveryJobVersion.V1:
                        await StartV1Async(mainJob);
                        break;
                    case RMDiscoveryJobVersion.V5:
                        await StartV5Async(mainJob);
                        break;
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while start analysis jobs. Error: {e}");
            }
        }

        private async Task StartV1Async(RMDiscoveryOffice365MainJob mainJob)
        {
            var processingAnalysisJobsCount = await _jobDao.CountAnalysisJobsByMainJobAsync(mainJob.Id, RMDiscoveryJobStatus.Running, RMDiscoveryJobStatus.Pending);
            var runnableAnalysisJobsCount = MAX_ANALYSIS_JOBS_ASYNC - processingAnalysisJobsCount;
            if (runnableAnalysisJobsCount == 0)
            {
                return;
            }

            var analysisJobs = await _jobDao.GetAnalysisJobsAsync(mainJob.Id, runnableAnalysisJobsCount, RMDiscoveryJobStatus.Waiting);
            analysisJobs.ForEach(item =>
            {
                item.Status = RMDiscoveryJobStatus.Pending;
                item.LastModifiedTime = DateTime.UtcNow.Ticks;
            });

            var subJobs = await _subJobDao.GetDiscoveryAnalysisSubJobs(analysisJobs.Select(item => item.Id).ToList());
            subJobs.ForEach(item =>
            {
                item.Runable = RecordsConstants.SubJob_Runnable_CanRun;
                item.LastUpdateTime = DateTime.UtcNow.Ticks;
            });

            await _subJobDao.AddOrUpdateSubJobsAsync(subJobs.ToArray());

            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJobs.ToArray());
            _logger.Info($"Successful set jobs [{string.Join(", ", analysisJobs.Select(item => item.Id))}] to [{RMDiscoveryJobStatus.Pending}] status.");
        }

        private async Task StartV5Async(RMDiscoveryOffice365MainJob mainJob)
        {
            var pendingAnalysisJobsCount = await _jobDao.CountAnalysisJobsByMainJobAsync(mainJob.Id, RMDiscoveryJobStatus.Pending);
            if (pendingAnalysisJobsCount == 0)
            {
                return;
            }

            _configurationService.SendNextVersionDiscoveryAnalysisJob(mainJob.Id);
        }
    }
}
