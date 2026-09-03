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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.SharePoint.Upgrade;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.RMTasks
{
    public class CosmosDBDirtyDataDeleteUpgradeExecutor : ITaskExecutor
    {
        private const string S_DIRTY_DATA_KEY = "COSMOS_DIRTY_DATA_WILL_DELETE_DEFINITION";

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            var tenantService = PlatformWindsorManager.GetService<ITenantService>();
            var tenants = tenantService.GetAllAvailableTenantInfo();
            foreach (var tenant in tenants)
            {
                
                TenantUtil.RunUnderTenant(tenant.TenantId, tenant.RegisterEmail, () => {

                    var keyValuesDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

                    var setting = keyValuesDao.GetValueByKey(S_DIRTY_DATA_KEY);
                    if (setting != null)
                    {
                        var definition = JsonConvert.DeserializeObject<RMCosmosDBDirtyDataNeedProcessedDefinition>(setting.Value);
                        if(definition != null && definition.NeedProcess)
                        {
                            var sharepointSettingService = PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
                            sharepointSettingService.SendDirtyDataDeleteJobMessage();
                        }
                    }
                });
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
