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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Monitor.Rule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Monitor
{
    public class JobMonitorExecutor : IMonitorExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(JobMonitorExecutor));
        public IJobMonitorDao JobMonitorDao => (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));

        public async System.Threading.Tasks.Task ExecutorAsync(MonitorBase monitor)
        {
            try
            {
                mLogger.Debug("begin to monitor job.");

                using (new PerformanceScope("Monitor Job"))
                {
                    var rule = (MonitorJobRule)MonitorGlobalConfig.GetRuleByType(MonitorType.JobMonitor);

                    var failedJobs = await MonitorFailedJobAsync(rule);

                    await MonitorSpecifyExceptionJobAsync(failedJobs);

                    await MonitorLongRunningJobAsync(rule);
                }

                mLogger.Debug("end to monitor job.");
            }
            catch (Exception ex)
            {
                mLogger.Error($"error occurred while monitor job, ERROR:{ex.ToString()}");
            }
           

        }
        private async Task<List<RMJobMonitor>> MonitorFailedJobAsync(MonitorJobRule rule) 
        {
            List<RMJobMonitor> jobs = new List<RMJobMonitor>();
            try
            {
                jobs = JobMonitorDao.GetFailedJobInfoByTimeRange(rule.QueryScope);
                mLogger.Debug($"monitor failed job count: {jobs?.Count}.");
                ArgumentCheck.NotNull(jobs, nameof(jobs));
                foreach (var job in jobs)
                {
                    mLogger.Warn($"Monitor Failed Job TenantId:{TenantLocalValue.LogonGroupId}, Job ID:{job.Id}.");
                    TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.MonitorFailedJob, new List<object> { job });
                }

                await TelemetryContext.FlushAsync();
            }
            catch (Exception ex)
            {
                mLogger.Error($"Error occurred while monitor failed job, Error:{ex.ToString()}");
            }
            return jobs;
        }
        private async Task MonitorLongRunningJobAsync(MonitorJobRule rule)
        {
            try
            {
                var jobs = JobMonitorDao.GetLongRunningJobInfoByTimeRange(rule.QueryScope, rule.LongRunningDate);
                mLogger.Debug($"monitor long running job count: {jobs?.Count}.");
                ArgumentCheck.NotNull(jobs, nameof(jobs));
                foreach (var job in jobs)
                {
                    mLogger.Warn($"Monitor Long Running Job TenantId:{TenantLocalValue.LogonGroupId}, Job ID:{job.Id}.");
                    TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.MonitorLongRunningJob, new List<object> { job });
                }

                await TelemetryContext.FlushAsync();
            }
            catch (Exception ex)
            {
                mLogger.Error($"Error occurred while monitor long running job, Error:{ex.ToString()}");
            }
        }
        private async Task MonitorSpecifyExceptionJobAsync(List<RMJobMonitor> faildJobs)
        {
            try
            {
                if (faildJobs != null && faildJobs.Count > 0) 
                {
                    var jobs = faildJobs.Where(j => j.ExceptionType != MonitorExceptionType.None).ToList();
                    mLogger.Debug($"monitor specific job count: {jobs?.Count}.");
                    ArgumentCheck.NotNull(jobs, nameof(jobs));
                    foreach (var job in jobs)
                    {
                        mLogger.Warn($"Monitor Specify Exception Job TenantId: {TenantLocalValue.LogonGroupId}, Job ID:{job.Id}, ExceptionType: {job.ExceptionType}.");
                        TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.MonitorSpecificExceptionJob, new List<object> { job });
                    }

                    await TelemetryContext.FlushAsync();
                }
                
            }
            catch (Exception ex)
            {
                mLogger.Error($"Error occurred while monitor specify exception job, Error:{ex.ToString()}");
            }
        }
    }
}
