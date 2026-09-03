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
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Trigger;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Trigger;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Trigger;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Trigger;

namespace AvePoint.RA.Service.RMTasks.Discovery
{
    public class DiscoveryTriggerTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DiscoveryTriggerTaskExecutor));

        private static readonly IRMTenantDiscoveryDBInfoDao s_tenantDiscoveryDBDao = new RMTenantDiscoveryDBInfoDao();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenantInfoes = await s_tenantDiscoveryDBDao.GetAllAvaliableAsync();
                foreach(var tenantInfo in tenantInfoes)
                {
                    await TenantUtil.RunUnderTenantAsync(tenantInfo.Id, "", async () =>
                    {
                        try
                        {
                            if (await RMDiscoveryDBManager.CheckOffice365TablesExistsAsync())
                            {
                                var office365Trigger = new RMDiscoveryOffice365JobTrigger();
                                await office365Trigger.TriggerAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckSalesforceTablesExistsAsync())
                            {
                                var salesforceTrigger = new RMDiscoverySalesforceJobTrigger();
                                await salesforceTrigger.TriggerAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckGoogleTablesExistsAsync())
                            {
                                var googleTrigger = new RMDiscoveryGoogleJobTrigger();
                                await googleTrigger.TriggerAsync();
                            }

                            if(await RMDiscoveryDBManager.CheckAOSPTablesExistsAsync())
                            {
                                var aospTrigger = new RMDiscoveryAOSPJobTrigger();
                                await aospTrigger.TriggerAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync())
                            {
                                var fileSystemTrigger = new RMDiscoveryFSJobTrigger();
                                await fileSystemTrigger.TriggerAsync();
                            }
                        }
                        catch(Exception e)
                        {
                            s_logger.Error($"An error occurred while execute trigger task for tenant. Error: {e}");
                        }
                    });
                }
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while execute trigger task. Error: {e}");
            }
        }
    }
}
