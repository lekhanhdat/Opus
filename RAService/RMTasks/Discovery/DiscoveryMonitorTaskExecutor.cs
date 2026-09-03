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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using AvePoint.RA.Service.Services.Discovery.Google.Work;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work;

namespace AvePoint.RA.Service.RMTasks.Discovery
{
    public class DiscoveryMonitorTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DiscoveryMonitorTaskExecutor));

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
                                var office365Monitor = new RMDiscoveryOffice365JobMonitor();
                                await office365Monitor.MonitorAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckGoogleTablesExistsAsync())
                            {
                                var googleMonitor = new RMDiscoveryGoogleJobMonitor();
                                await googleMonitor.MonitorAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckAOSPTablesExistsAsync())
                            {
                                var aosp365Monitor = new RMDiscoveryAOSPJobMonitor();
                                await aosp365Monitor.MonitorAsync();
                            }

                            if (await RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync())
                            {
                                var fileSystemMonitor = new RMDiscoveryFSJobMonitor();
                                await fileSystemMonitor.MonitorAsync();
                            }
                        }
                        catch(Exception e)
                        {
                            s_logger.Error($"An error occurred while execute monitor task for tenant. Error: {e}");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while execute monitor task. Error: {e}");
            }
        }
    }
}
