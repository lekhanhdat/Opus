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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using RAArchiverCommon.Sqlite.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    internal class ThrottlingStatisticExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(ThrottlingStatisticExecutor));

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public Task ExecutorAsync(TaskBase task)
        {
            mLogger.Info($"Start execute throttling timer worker");
            DateTime now = DateTime.UtcNow;
            if (now.TimeOfDay < new TimeSpan(0, 10, 0))
            {
                Thread.Sleep(new TimeSpan(0, 10, 10) - now.TimeOfDay);
            }

            var tenants = TenantService.GetAllAvailableTenantInfo().ToDictionary(item => item.TenantId, item => item.RegisterEmail);
            foreach (var tenant in tenants)
            {
                TenantUtil.RunUnderTenant(tenant.Key, tenant.Value, () =>
                {
                    try
                    {
                        DayThrottlingDetailWorker.MergeCacheBlobDayThrottlingDetail();
                    }
                    catch (Exception e)
                    {
                        mLogger.Error($"An error occurred while Collect Throttling Statistic in tenant,tenant id = {tenant.Key}.Error:{e.ToString()}");
                    }
                });
            }
            mLogger.Info($"end execute throttling timer worker, cose time:{(DateTime.UtcNow - now).TotalMinutes} m");

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
