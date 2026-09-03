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
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class FeatureUsageLimitTaskExcutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(FeatureUsageLimitTaskExcutor));
        private IFeatureUsageLimitService FeatureUsageLimitService => PlatformWindsorManager.GetService<IFeatureUsageLimitService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var nowLocal = DateTimeOffset.Now;
                logger.Info($"Current time (local): {nowLocal:yyyy-MM-dd HH:mm:ss.fff zzz};" +" — start clear usage");
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, ExcuteClearFeatureUsage);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while clear feature usage, ERROR:{0}", ex.ToString());
            }
        }
        private async System.Threading.Tasks.Task ExcuteClearFeatureUsage()
        {
            try
            {
                FeatureUsageLimitService.ClearUsage();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while clear feature usage, ERROR:{0}", ex.ToString());
            }
        }
    }
}
