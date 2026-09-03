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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.RMTasks
{
    public class ProcessAOSNotificationTaskExecutor : ITaskExecutor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ProcessAOSNotificationTaskExecutor));
        private static readonly int RunSRNJobIfNoNewNotifiedMinutes = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.RUN_SRN_JOB_IF_NO_NOTIFIED_IN_MINUTES, 5);


        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMAOSNotificationService AOSNotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();
        private IDataEncryptionService DataEncryptionService => PlatformWindsorManager.GetService<IDataEncryptionService>();
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMFunctionSettingDao _functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        /// <summary>
        /// 待优化
        /// </summary>
        /// <param name="context"></param>
        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to process AOS notifications.");
                ProcessNotifications();
                logger.Info("Finish to process AOS notifications.");
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while checking and updating agent status. ERROR:{0}", e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void ProcessNotifications()
        {
            TryCreateSyncNodesJobs();
            TryCreateSwitchSecurityProfileJobs();
            TrySyncChangedTenantOwner();
        }

        private void TryCreateSyncNodesJobs()
        {
            var period = DateTime.UtcNow.AddMinutes(-RunSRNJobIfNoNewNotifiedMinutes).Ticks;
            var pendingTenants = AOSNotificationService.GetPendingTenants(period);
            var tenants = TenantService.GetAllAvailableTenantInfo().ToDictionary(item => item.TenantId, item => item.RegisterEmail);
            foreach (var tenantId in pendingTenants)
            {
                var initState = TenantService.GetTenantInitNodeState(tenantId);
                if (initState != RMInitNodeState.Synced)
                {
                    logger.Info($"Tenant: {tenantId}, InitNodeState: {initState}, can't start incremental sync job.");
                    continue;
                }

                var count = JobQueueService.GetMessagesCount(tenantId, Contract.JobMonitor.JobType.SyncNodesFromAOS);
                if (count > 0)
                {
                    logger.Info($"Tenant: {tenantId} has sync job in the JobQueue.");
                    continue;
                }

                TenantUtil.RunUnderTenant(tenantId, tenants[tenantId], () =>
                {
                    if (_functionSettingDao.IsEnableMultiGeoFeature(_keyValueDao).Result && !_multiGeoDataCenterService.IsMainDC())
                    {
                        logger.Info($"Multi-geo feature is enabled and this is not the main DC, skipping sync node jobs.");
                        return;
                    }
                    count = JobMonitorService.GetRunningJobsCount(Contract.JobMonitor.JobType.SyncNodesFromAOS);
                    if (count > 0)
                    {
                        logger.Info($"Tenant: {tenantId} has running sync job.");
                        return;
                    }

                    var id = RemoteNodeService.CreateSyncNodesJob();
                    if (!string.IsNullOrEmpty(id))
                    {
                        logger.Info($"Create sync nodes job success. Tenant group id: {tenantId}, Job id: {id}.");
                    }
                    else
                    {
                        logger.Info($"Create sync nodes job fail. Tenant group id: {tenantId}.");
                    }
                });
            }
        }

        private void TryCreateSwitchSecurityProfileJobs()
        {
            DataEncryptionService.CreateSwitchProfileJobs();
        }

        private void TrySyncChangedTenantOwner()
        {
            var tenants = TenantService.GetAllAvailableTenantInfo().ToDictionary(item => item.TenantId, item => item.RegisterEmail);
            var changeTenantOwnerMessages = AOSNotificationService.GetChangeOwnerTenants();

            foreach(var message in changeTenantOwnerMessages)
            {
                try
                {
                    TenantUtil.RunUnderTenant(message.TenantGroupId, tenants[message.TenantGroupId], () =>
                    {
                        try
                        {
                            logger.Info($"Tenant: {message.TenantGroupId} starts to sync tenant owner.");
                            UserService.SyncAosUsersAsync().GetAwaiter().GetResult();
                            TenantService.SyncTenantOwner(message.TenantGroupId);
                            AOSNotificationService.Delete(message.QueueMessageId);
                            logger.Info($"Tenant: {message.TenantGroupId} sync tenant owner success.");
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Tenant: {message.TenantGroupId} sync tenant owner failed. Error : {e}");
                        }
                    });
                }
                catch (Exception e)
                {
                    logger.Error($"Tenant: {message.TenantGroupId} sync tenant owner failed. Error : {e}");
                }
            }
        }
    }
}
