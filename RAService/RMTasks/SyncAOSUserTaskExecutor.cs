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
    public class SyncAOSUserTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(SyncAOSUserTaskExecutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                mLogger.Info($"SyncAOSUserTaskExecutor: available tenant count is [{tInfos?.Count}]");
                if (tInfos == null)
                {
                    mLogger.Error("Available tenant info is null");
                    throw new NullReferenceException("Available tenant info is null");
                }
                foreach (var tInfo in tInfos)
                {
                    mLogger.Info($"SyncAOSUserTaskExecutor Start, Current tenant: [{tInfo.TenantId}]");
					await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, SyncUserTaskAsync);
                }

            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while UpgradePoolUserTask,ERROR:{0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task SyncUserTaskAsync()
        {
            try
            {
                mLogger.Info("begin to sync user:{0}", TenantLocalValue.LogonGroupId);
                await UserService.SyncAosUsersAsync();
                TenantService.SyncTenantOwner(TenantLocalValue.LogonGroupId);
                //UserService.SyncAccountPoolUsers();
                mLogger.Info("finish to sync user:{0}", TenantLocalValue.LogonGroupId);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while Upgrade current PoolUserTask,ERROR:{0}", ex.ToString());
            }

        }


    }
}
