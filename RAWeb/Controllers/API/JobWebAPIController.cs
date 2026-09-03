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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;


namespace AvePoint.RA.Web.Controllers.API
{
    public class JobWebAPIController : RAWebApiBase
    {
        //private static readonly RALogger logger = RALogger.GetInstance(typeof(JobWebAPIController));
        //private IJobMonitorService _JobMonitorService;
        //private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        //private ITenantService _TenantService;
        //private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);
        //private IRMRemoteNodeDao _RemoteNodeDao;
        //private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService(ref _RemoteNodeDao);
        //private IRMMailboxDao _MailboxDao;
        //private IRMMailboxDao MailboxDao => PlatformWindsorManager.GetService(ref _MailboxDao);
        //string jobId = "TS20160630153200354637";

        //[HttpGet]
        //public System.Threading.Tasks.Task<JMItemInfo> GetOneJob()
        //{
        //    return JobMonitorService.GetJobAsync(jobId);
        //}
        //[HttpPost]
        //public string PostJob([FromBody] JMItemInfo job)
        //{
        //    if (job != null)
        //    {
        //        return "OK";
        //    }
        //    return "Failed";
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public string Test()
        //{
        //    try
        //    {
        //        var dbServer = RA.Common.Configurations.RMGlobalConfiguration.DBConfig.ConfigDatabaseInstance;
        //        var dbName = RA.Common.Configurations.RMGlobalConfiguration.DBConfig.ConfigDatabaseName;
        //        using (var con = AzureUtil.GetConnection(dbServer, dbName))
        //        {
        //            var command = con.CreateCommand();
        //            command.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
        //            command.CommandText = "select 1";
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Test SqlConnection : {ex}");
        //        return ex.ToString();
        //    }
        //    return "OK";
        //}

        /*
        [HttpGet]
        [AllowAnonymous]
        public string ResetRemoteNodes(string tenantGroupId)
        {
            TenantUtil.RunUnderTenant(tenantGroupId, null, () =>
            {
                var mbKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.Mailbox);
                var pcKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.PrivateChannel);
                var rnKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.RemoteNode);

                if (RedisCacheService.Redis.HasKeyExisted(mbKey))
                {
                    RedisCacheService.Redis.KeyDelete(mbKey);
                }
                var mbFields = RedisCacheService.Redis.HashAll<SyncRemoteNodePara>(mbKey);
                RedisCacheService.Redis.HashDelete(mbKey, mbFields.Keys, false);

                if (RedisCacheService.Redis.HasKeyExisted(pcKey))
                {
                    RedisCacheService.Redis.KeyDelete(pcKey);
                }
                var pcFields = RedisCacheService.Redis.HashAll<SyncRemoteNodePara>(pcKey);
                RedisCacheService.Redis.HashDelete(pcKey, pcFields.Keys, false);

                if (RedisCacheService.Redis.HasKeyExisted(rnKey))
                {
                    RedisCacheService.Redis.KeyDelete(rnKey);
                }
                var rnFields = RedisCacheService.Redis.HashAll<SyncRemoteNodePara>(rnKey);
                RedisCacheService.Redis.HashDelete(rnKey, rnFields.Keys, false);

                RemoteNodeDao.ClearAll();
                MailboxDao.ClearAll();

                TenantService.UpdateSyncNodeState(tenantGroupId, Contract.Aos.Notification.RMInitNodeState.SyncFailed);
            });
            
            return "OK";
        }
        */
    }
}