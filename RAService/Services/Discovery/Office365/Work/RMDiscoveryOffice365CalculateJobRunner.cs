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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator;
using RACloudFS.Report;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365CalculateJobRunner
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365CalculateJobRunner));

        private readonly IJobMonitorDao _jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private readonly IRMDiscoveryOffice365JobDao _discoveryJobDao = new RMDiscoveryOffice365JobDao();

        private readonly IRMDiscoveryOffice365TenantDao _o365TenantDao = new RMDiscoveryOffice365TenantDao();

        private readonly IRMDiscoveryOffice365DataQueryDao _dataQueryDao = new RMDiscoveryOffice365DataQueryDao();

        private readonly IRMDiscoveryExecutionInfoDao _executionInfoDao = new RMDiscoveryExecutionInfoDao();

        private readonly IRMReportManager _reportManager;

        private readonly string _jobId;

        public RMDiscoveryOffice365CalculateJobRunner(string jobId)
        {
            _jobId = jobId;
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            ReportMangerFactory.Instance.Init(jobId, Contract.JobMonitor.JobType.DiscoveryReCalculate);
            _reportManager.StartUpdateJobProgress(60);
            _reportManager.IncreaseBase(10000);
        }

        public async Task RunAsync()
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    _ = RefreshJobProgressAsync(cts.Token);

                    var jobInfo = _jobMonitorDao.GetJob(_jobId);
                    var (_, mainJob) = await _discoveryJobDao.TryGetMainJobAsync(jobInfo.DiscoveryMainJobId);

                    _logger.Info($"The calculate run job base on [{mainJob.Id} {mainJob.Type}].");

                    var succeed = true;

                    var duplicateCalculator = new RMDiscoveryOffice365DuplicateCalculator(mainJob);
                    succeed &= await duplicateCalculator.CalculateAsync();

                    var rescanCalculator = new RMDiscoveryOffice365RescanCalculator(mainJob);
                    succeed &= await rescanCalculator.CalculateAsync();

                    if (mainJob.Type != RMDiscoveryJobType.Retry)
                    {
                        var projectionCalculator = new RMDiscoveryOffice365ProjectionCalculator();
                        succeed &= await projectionCalculator.CalculateAsync();
                    }

                    _logger.Info($"Current calculate job is succeed [{succeed}].");

                    await CalculateJobStatusAsync(mainJob);

                    var o365Tenants = await _o365TenantDao.GetAllAsync();
                    foreach (var o365Tenant in o365Tenants)
                    {
                        var cacheManager = new RMDiscoveryCacheManager(o365Tenant.UniqueId, RMDiscoveryCacheDataSource.Office365);
                        await cacheManager.ClearAsync();
                        _logger.Info($"The tenant [{o365Tenant.UniqueId}] cache cleared.");
                    }

                    _reportManager.SetJobFinished(succeed ? JobStatus.Finished : JobStatus.Failed);
                    cts.Cancel(false);
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while run calculate job. Error: {e}");
                _reportManager.SetJobFinished(JobStatus.Failed);
            }
        }

        private async Task CalculateJobStatusAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            try
            {
                var discoveryJobStatusDic = await _discoveryJobDao.GetDiscoveryCompletedStatusAsync(mainJob.Id);
                _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Exception, out var exceptionCount);
                mainJob.EndTime = DateTime.UtcNow.Ticks;
                mainJob.Status = RMDiscoveryJobStatus.Finished;

                if ((finishedCount > 0 && failedCount + exceptionCount > 0) || exceptionCount > 0)
                {
                    mainJob.Status = RMDiscoveryJobStatus.Exception;
                }
                else if (failedCount + exceptionCount > 0)
                {
                    mainJob.Status = RMDiscoveryJobStatus.Failed;
                }
                await _discoveryJobDao.AddOrUpdateMainJobAsync(mainJob);

                if(mainJob.Type != RMDiscoveryJobType.Retry)
                {
                    if (mainJob.Status == RMDiscoveryJobStatus.Finished || mainJob.Status == RMDiscoveryJobStatus.Exception)
                    {
                        var fileTotalSize = 0L;
                        var o365Tenants = await _o365TenantDao.GetAllAsync();
                        foreach (var o365Tenant in o365Tenants)
                        {
                            var aggregateInfo = await _dataQueryDao.GetAggregateTotalDataListAsync(o365Tenant.UniqueId);
                            fileTotalSize += aggregateInfo.Sum(item => item.FileTotalSize);
                        }

                        await _executionInfoDao.UpdateFileSizeByMainJobAsync(mainJob.Id, fileTotalSize);
                    }
                    else if (mainJob.Status == RMDiscoveryJobStatus.Failed)
                    {
                        await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                        await RMDiscoveryOffice365LicenseHelper.DecreaseConsumedFrequencyPreMonthAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate discovery main job status. Error: {e}");
                throw;
            }
        }

        private async Task RefreshJobProgressAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _reportManager.Increase();
                await Task.Delay(1000 * 60 * 5, token);
            }
        }
    }
}
