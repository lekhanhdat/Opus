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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Dao;
using Cloud.Sdk.Token;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RADataBroker
{
    public static class DAOClientCache
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(DAOAPIClientV1));
        private static readonly ConcurrentDictionary<String, DocAveOnlineApiClient> clientDic = new ConcurrentDictionary<String, DocAveOnlineApiClient>();

        public static DocAveOnlineApiClient GetDAOApiClient(string aosCustomerId = "")
        {
            return clientDic.GetOrAdd(aosCustomerId, t => GetClient(aosCustomerId));
        }

        private static DocAveOnlineApiClient GetClient(string aosCustomerId)
        {
            try
            {
                var client = AosApiUtility.CloudSdkDaoClientFactory.CreateDaoApiClient(aosCustomerId);
                InitializeTenant(client);
                return client;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while init dao client. Error:{e.ToString()}");
                throw new Exception(I18NEntity.GetString("RM_JS_DAM_CheckDAConFailed"));
            }
        }

        private static void InitializeTenant(DocAveOnlineApiClient onlineApiClient, int retryCount = 0)
        {
            try
            {
                var task = Task.Run(() => { return onlineApiClient.AccountManagerService.Initialize(); });
                var result = task.GetAwaiter().GetResult();
                if (result.InitializeStatus == Cloud.Sdk.Data.Dao.InitializeStatus.Sucessful
                    || result.InitializeStatus == Cloud.Sdk.Data.Dao.InitializeStatus.Exist)
                {
                    //tenant group has been initialized in DAO
                }
                else if (result.InitializeStatus == Cloud.Sdk.Data.Dao.InitializeStatus.Initializing)
                {
                    //tenant group is being initialized by other thread, need to wait and retry
                    if (retryCount < 6)
                    {
                        logger.Warn($"Tenant group:{TenantLocalValue.LogonGroupId} is initializing in DAO, need to wait and retry. Retry count:{retryCount}");
                        Thread.Sleep(10 * 1000);
                        InitializeTenant(onlineApiClient, ++retryCount);
                    }
                    else
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_JM_TenangGroupInitializedFailedDAO"));
                    }
                }
                else if (result.InitializeStatus == Cloud.Sdk.Data.Dao.InitializeStatus.SoftDelete)
                {
                    //soft deleted in DAO
                    throw new Exception(I18NEntity.GetString("RM_JS_JM_TenangGroupSoftDeletedDAO"));
                }
                else
                {
                    //initialized failed, need to retry
                    if (retryCount < 3)
                    {
                        logger.Warn($"Tenant group:{TenantLocalValue.LogonGroupId} initializeStatus is {result.InitializeStatus.ToString()}, need to retry. Retry count:{retryCount}");
                        InitializeTenant(onlineApiClient, ++retryCount);
                    }
                    else
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_JM_TenangGroupInitializedFailedDAO"));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while InitializeTenant in DAO, error:{e.ToString()} Retry count:{retryCount}");
                throw e;
            }
        }
    }
}
