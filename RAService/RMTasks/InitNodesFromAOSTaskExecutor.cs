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
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using System;

namespace AvePoint.RA.Service.RMTasks
{
    public class InitNodesFromAOSTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(InitNodesFromAOSTaskExecutor));

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();


        /// <summary>
        /// 待优化
        /// </summary>
        /// <param name="context"></param>
        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                CreateInitNodesJobForTenants();
                CheckStateForSyncingTenants();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while checking and updating agent status. ERROR:{0}",e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void CheckStateForSyncingTenants()
        {
            var syncingTenants = TenantService.GetSyncingNodesTenants();
            if (syncingTenants?.Count > 0)
            {
                foreach (var tInfo in syncingTenants)
                {
                    TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, CheckStateForSyncingTenant);
                }
            }
        }
        /// <summary>
        /// 正在Syncing的Tenant，如果Job Monitor里没有Running的SyncNodesFromAOS Job
        /// 则表示Job已经非正常退出了，并没有执行成功，也没有更新Tenant Sync Data的状态
        /// 需要在这里把Tenant更新成 SyncFailed 状态，以便后续重新启动Init Sync Data Job
        /// </summary>
        private void CheckStateForSyncingTenant()
        {
            var tenantGroupId = TenantLocalValue.LogonGroupId;
            try
            {
                if (JobMonitorService.GetRunningJobsCount(JobType.SyncNodesFromAOS) == 0
                    && JobQueueService.GetMessagesCount(tenantGroupId, JobType.SyncNodesFromAOS) == 0)
                {
                    logger.Warn($"Set sync nodes failed state for tenant: {tenantGroupId}");
                    TenantService.UpdateSyncNodeState(tenantGroupId, RMInitNodeState.SyncFailed);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while checking unfinished sync nodes job for tenant: {tenantGroupId}. {ex}");
            }
        }

        private void CreateInitNodesJobForTenants()
        {
            var tInfos = TenantService.GetPenddingForSyncNodesTenants();
            foreach (var tInfo in tInfos)
            {
                TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, CreateInitNodesJobForTenant);
            }
        }
        private void CreateInitNodesJobForTenant()
        {
            RemoteNodeService.CreateSyncAllNodesJob();
        }
    }
}
