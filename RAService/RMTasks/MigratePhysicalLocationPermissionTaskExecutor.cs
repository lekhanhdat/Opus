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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    internal class MigratePhysicalLocationPermissionTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(MigratePhysicalLocationPermissionTaskExecutor));

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

        private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, async () =>
                    {
                        await MigratePhysicalLocationPermission(tInfo.TenantId);
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to execute task {task.Id} of type {task.Type}. {ex}");
            }
        }

        private async Task MigratePhysicalLocationPermission(string tenantId)
        {
            try
            {
                if (!RMTenantUpgradeHelper.IsNeedUpgrade(tenantId, Contract.TenantUpgrade.RMUpgradeFeature.MigratePhysicalPermissions))
                    return;
                logger.Info($"Start to migrate physical location permission for tenant {tenantId}.");
                var phyTopLocationIds = await RMLocationDao.GetAllTopLocationIds();
                
                var physicalRecordManagerGroupId = RMSecurityGroupDao.LoadGroupIdHavePhysicalRecordManagerPermission();
                if (physicalRecordManagerGroupId != 0)
                {
                    logger.Info($"Migrate physical location record manager permission with group {physicalRecordManagerGroupId}");
                    ScopeRoleAssignmentDao.AddScopePermission(physicalRecordManagerGroupId, phyTopLocationIds, SourceFlag.Physical);
                }
                RMTenantUpgradeHelper.SetToFinish(tenantId, Contract.TenantUpgrade.RMUpgradeFeature.MigratePhysicalPermissions, Contract.TenantUpgrade.RMUpgradeStatus.Success);
                logger.Info($"Succeed to migrate physical location permission for tenant {tenantId}.");
            }
            catch (Exception ex)
            {
                RMTenantUpgradeHelper.SetToFinish(tenantId, Contract.TenantUpgrade.RMUpgradeFeature.MigratePhysicalPermissions, Contract.TenantUpgrade.RMUpgradeStatus.Failed);
                logger.Error($"Failed to migrate physical location permission for tenant {tenantId}.Error: {ex}");
            }
        }
    }
}
