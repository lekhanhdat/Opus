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
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;

namespace AvePoint.RA.Service.RMTasks.Discovery
{
    internal class DiscoveryClearLicenseUsageTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DiscoveryClearLicenseUsageTaskExecutor));

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
                        var res = await RMDiscoveryOffice365LicenseHelper.ClearLicenseUsageAsync();
                        var salesforceResult = await RMDiscoverySalesforceLicenseHelper.ClearLicenseUsageAsync();
                        var googleROTResult = await RMDiscoveryGoogleLicenseHelper.ClearLicenseUsageAsync();
                        var fileSystemResult = await RMDiscoveryFSLicenseHelper.ClearLicenseUsageAsync();
                        s_logger.Info($"Clear discovery license usage result: {res}, salesforce result: {salesforceResult}, google ROT result: {googleROTResult}, file system result: {fileSystemResult}");
                    });
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while execute clear discovery license task. Error: {e}");
            }
        }
    }
}
