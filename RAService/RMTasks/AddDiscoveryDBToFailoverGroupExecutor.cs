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
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.Common.Util;
using System.Threading;
using AvePoint.RA.Common.Retrying;
using AvePoint.Media.ClassicStorage.Cloud.Common;

namespace AvePoint.RA.Service.RMTasks
{
    public class AddDiscoveryDBToFailoverGroupExecutor : ITaskExecutor
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(AddDiscoveryDBToFailoverGroupExecutor));

        private static readonly IRMTenantDiscoveryDBInfoDao s_tenantDiscoveryDBDao = new RMTenantDiscoveryDBInfoDao();

        private static readonly RMRetryer retryer = RMRetryerBuilder.CreateBuilder().Build();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to run add discovery DB To failover group executor");

                var tenantDisocveryDBInfos = await s_tenantDiscoveryDBDao.GetAllAvaliableAsync();

                var needUpdateDiscoveryDBInfos = new List<RMTenantDiscoveryDBInfo>();

                foreach (var discoveryDB in tenantDisocveryDBInfos)
                {
                    logger.Info($"Current tenant id is : {discoveryDB.Id}");
                    TenantUtil.RunUnderTenant(discoveryDB.Id, "", () =>
                    {
                        try
                        {
                            retryer.Retry(() =>
                            {
                                var result = FailoverGroupService.AddDatabasesToFailoverGroup(discoveryDB.DatabaseName);
                                if (result)
                                {
                                    discoveryDB.UseFailoverGroup = result;
                                    needUpdateDiscoveryDBInfos.Add(discoveryDB);
                                }
                                logger.Info($"This {discoveryDB.Id} tenant is ues failover group? {result}");
                                Thread.Sleep(5 * 1000);
                            });
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Add discovery db to failover group failed, tenant id : {discoveryDB.Id}, error : {e}.");

                        }
                    });
                }

                var result = await s_tenantDiscoveryDBDao.TryUpdateTenantDiscoveryDBInfoAsync(needUpdateDiscoveryDBInfos);
                logger.Info($"Update tenant discovery db info result : {result}");
            }
            catch(Exception e)
            {
                logger.Error($"Run add discovery DB to failover group failed, error : {e}");
            }
        }
    }
}
