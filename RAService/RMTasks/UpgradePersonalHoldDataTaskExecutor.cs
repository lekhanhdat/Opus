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
    public class UpgradePersonalHoldDataTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(UpgradePersonalHoldDataTaskExecutor));
        private ITenantService _tenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IUpgradePersonalHoldDataService _upgradePersonalHoldDataService => PlatformWindsorManager.GetService<IUpgradePersonalHoldDataService>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {

                var tInfos = _tenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, ExecuteTaskAsync);
                }

            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while UpgradePersonalHoldDataTaskExecutor,ERROR:{0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ExecuteTaskAsync()
        {
            try
            {
                mLogger.Info($"begin to upgrade personal hold data for tenant {TenantLocalValue.LogonGroupId}");
                //_upgradePersonalHoldDataService = (IUpgradePersonalHoldDataService)PlatformWindsorManager.GetService(typeof(IUpgradePersonalHoldDataService));
                if (_upgradePersonalHoldDataService.NeedUpgrade())
                {
                    await _upgradePersonalHoldDataService.UpgradeAsync();
                    mLogger.Info($"finish to upgrade personal hold data for tenant { TenantLocalValue.LogonGroupId}");
                }
                else
                {
                    mLogger.Info($"no need to upgrade personal hold data for tenant {TenantLocalValue.LogonGroupId}");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"error occurred while Upgrade personal hold data for tenant {TenantLocalValue.LogonGroupId},ERROR: {ex.ToString()}");
            }

        }


    }
}
