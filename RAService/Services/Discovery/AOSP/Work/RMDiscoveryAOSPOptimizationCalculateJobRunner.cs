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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Optimization;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work
{
    public class RMDiscoveryAOSPOptimizationCalculateJobRunner
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPOptimizationCalculateJobRunner));

        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _settingInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();

        private readonly IRMDiscoveryAOSPProgressDao _optimizationDao = new RMDiscoveryAOSPProgressDao();

        private readonly Guid _settingId;

        private readonly Guid _o365TenantId;

        private readonly IRMReportManager _reportManager;

        public RMDiscoveryAOSPOptimizationCalculateJobRunner(string jobId, Guid settingId, Guid o365TenantId)
        {
            _settingId = settingId;
            _o365TenantId = o365TenantId;
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            ReportMangerFactory.Instance.Init(jobId, Contract.JobMonitor.JobType.DiscoveryAOSPOptimizationCalculate);
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

                    var settingInfo = await _settingInfoDao.GetSettingInfoByIdAsync(_settingId, _o365TenantId);
                    var nextRunTime = settingInfo.NextTime == 0 ? DateTime.UtcNow.Ticks : settingInfo.NextTime;

                    var siteCollection = _settingInfoDao.GetSettingRelatedSitesAsync(_o365TenantId, _settingId);
                    await foreach (var site in siteCollection)
                    {
                        var optimizationInfo = await _optimizationDao.GetSiteOptimizedInfoAsync(_o365TenantId, site.Id);
                        if (optimizationInfo != null && optimizationInfo.NextOptimizationTime > DateTime.UtcNow.Ticks && optimizationInfo.NextOptimizationTime < nextRunTime && optimizationInfo.NextOptimizationTime != 0L)
                        {
                            continue;
                        }

                        var calculator = new RMDiscoveryAOSPOptimizationCalculator(_o365TenantId, site, settingInfo);
                        await calculator.CalculateAsync();
                    }

                    _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                    cts.Cancel(false);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run discovery analysis job. Error: {e}");
                _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Failed);
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
