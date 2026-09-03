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
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.RMTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Timer.Task
{
    public class UpgradeDBModeTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(UpgradeDBModeTaskExecutor));

        private readonly static object thisLock = new object();
        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                ICommonService CommonService = (ICommonService)PlatformWindsorManager.GetService(typeof(ICommonService));
                try
                {
                    CommonService.UpgradeControlDB();
                }
                catch (Exception ex)
                {
                    logger.Error("errror occurred while upgrade control db mode:{0}", ex.ToString());
                }
                ITenantService TenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                logger.Info($"finish to init tenant service, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                lock (thisLock)
                {
                    var tInfos = TenantService.GetAllTenantInfo();
                    foreach (var tInfo in tInfos)
                    {
                        TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, CommonService.UpgradeTenantDBAsync).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade DB Mode,ERROR:{0}", ex.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
