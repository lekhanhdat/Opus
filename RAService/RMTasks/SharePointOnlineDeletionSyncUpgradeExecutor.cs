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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.ManualApproval.Upgrade;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.RMTasks
{
    public class SharePointOnlineDeletionSyncUpgradeExecutor : ITaskExecutor
    {
        private const string NeedRunDeletionSyncUpgradeJob = "NEED_RUN_DELETION_SYNC_UPGRADE_JOB";

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            var tenantService = PlatformWindsorManager.GetService<ITenantService>();
            var tenants = tenantService.GetAllAvailableTenantInfo();
            foreach(var tenant in tenants)
            {
                TenantUtil.RunUnderTenant(tenant.TenantId, tenant.RegisterEmail, () => {

                    var keyValuesDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

                    var setting = keyValuesDao.GetValueByKey(NeedRunDeletionSyncUpgradeJob);
                    if (setting != null && Convert.ToBoolean(setting.Value))
                    {
                        var sharepointSettingService = PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
                        sharepointSettingService.SendDeletionSyncUpgradeJobMessage();
                    }
                });
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
