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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using AvePoint.RA.DB.Core.Discovery.DBManager;

namespace AvePoint.RA.Service.RMTasks.Discovery
{
    public class DiscoveryStarterTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DiscoveryTriggerTaskExecutor));

        private static readonly IRMTenantDiscoveryDBInfoDao s_tenantDiscoveryDBDao = new RMTenantDiscoveryDBInfoDao();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenantInfoes = await s_tenantDiscoveryDBDao.GetAllAvaliableAsync();
                foreach (var tenantInfo in tenantInfoes)
                {
                    await TenantUtil.RunUnderTenantAsync(tenantInfo.Id, "", async () =>
                    {
                        try
                        {
                            if (await RMDiscoveryDBManager.CheckOffice365TablesExistsAsync())
                            {
                                var trigger = new RMDiscoveryOffice365JobStarter();
                                await trigger.StartAsync();
                            }
                        }
                        catch(Exception e)
                        {
                            s_logger.Error($"An error occurred while execute starter task for tenant. Error: {e}");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while execute starter task. Error: {e}");
            }
        }
    }
}
