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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class AgentStatusTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(AgentStatusTaskExecutor));
        private const int IntervalInSeconds = 600;

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly static object lobkObj = new object();
        /// <summary>
        /// 待优化
        /// </summary>
        /// <param name="context"></param>
        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {

                lock (lobkObj)
                {
                    var tInfos = TenantService.GetAllAvailableTenantInfo();
                    foreach (var tInfo in tInfos)
                    {
                        TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, CheckAsync).Wait();

                    }
                }

            }
            catch (Exception e)
            {
                mLogger.Error("An error occurred while checking and updating agent status. ERROR:{0}",e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task CheckAsync()
        {
            try
            {
                
                var AgentMgmtService = (IAgentMgmtService)PlatformWindsorManager.GetService(typeof(IAgentMgmtService));
                await AgentMgmtService.CheckAndUpdateStatusAsync(IntervalInSeconds, Hybrid.Contract.Object.ServiceStatus.InActive);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while checking agent status,ERROR:{0}", ex.ToString());
            }
        }
    }
}
