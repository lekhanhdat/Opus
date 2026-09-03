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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.ManualApproval;

namespace AvePoint.RA.Service.RMTasks
{
    internal class ManualFileSystemUpgradeExecutor : ITaskExecutor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(ManualFileSystemUpgradeExecutor));

        private const string S_NEED_RENEW_PARTITION_KEY_FOR_FS = "NEED_RENEW_PARTITION_KEY_FOR_FS";

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenantService = PlatformWindsorManager.GetService<ITenantService>();
                var tenants = tenantService.GetAllAvailableTenantInfo();
                foreach (var tenant in tenants)
                {
                    try
                    {
                        TenantUtil.RunUnderTenant(tenant.TenantId, tenant.RegisterEmail, () => {

                            var keyValuesDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

                            var setting = keyValuesDao.GetValueByKey(S_NEED_RENEW_PARTITION_KEY_FOR_FS);
                            if (setting != null && Convert.ToBoolean(setting.Value))
                            {
                                var manualApprovalService = PlatformWindsorManager.GetService<IRMManualApprovalService>();
                                manualApprovalService.SendFileSystemManualDataUpgradeJobMessage();
                            }
                        });
                    }
                    catch(Exception ex)
                    {
                        s_logger.Error($"An error occurred while execute manual file system upgrade task by tenant: [{tenant.TenantId}]. Error: {ex}");
                    }
                }
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while execute manual file system upgrade task. Error: {e}");
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
