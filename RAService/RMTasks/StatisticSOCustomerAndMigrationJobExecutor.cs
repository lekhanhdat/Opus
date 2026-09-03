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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class StatisticSOCustomerAndMigrationJobExecutor : ITaskExecutor
    {
        private RALogger logger => RALogger.GetInstance(typeof(AddDiscoveryDBToFailoverGroupExecutor));

        private static ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to executor statistic SO customer and migration job.");
                var availableTenants = TenantInfoDao.GetAllAvailableTenantInfo();
                foreach (var tenantInfo in availableTenants)
                {
                    try
                    {
                        logger.Info($"Statistic [{tenantInfo.TenantId}] SO customer and migration job.");
                        TenantUtil.RunUnderTenant(tenantInfo.TenantId, tenantInfo.RegisterEmail, (Action)(() =>
                        {
                            var isSOCustomer = TenantService.IsNewOpusTenant();
                            logger.Info($"[{tenantInfo.TenantId}] is so customer? {isSOCustomer}.");
                            if (isSOCustomer)
                            {
                                TelemetryContext.SendToQueue(TelemetryModule.Account, TelemetryEventType.SOCustomer);
                                var migrationJob = JobMonitorDao.GetLastFinishedJob(JobType.CloudArchiverMigration);
                                logger.Info($"[{tenantInfo.TenantId}] if has migration job? {migrationJob != null}.");
                                if (migrationJob != null)
                                {
                                    logger.Info($"[{tenantInfo.TenantId}] migration job id : {migrationJob.Id}.");
                                    TelemetryContext.SendToQueue(TelemetryModule.Magration, TelemetryEventType.MigrationJob, (IList<object>)new List<object> { migrationJob.EndTime.ToString() });
                                }
                            }
                        }));
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Statistic [{tenantInfo.TenantId}] SO customer and migration job failed.Error : {e}");
                    }
                }

                await TelemetryContext.FlushAsync();
            }
            catch(Exception e)
            {
                logger.Error("Statistic SO customer and migration job failed.");
            }
        }
    }
}
