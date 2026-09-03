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

using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Util;
using AvePoint.RA.RADataBroker;
using AvePoint.Media.Service;
using Microsoft365.SharePoint.Rest;
using Cloud.Sdk.Aos;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.DB.Dao.Impl;

namespace AvePoint.RA.Service.RMTasks
{

    internal class EnsureSecurityProfileAfterMigrationExecutor : ITaskExecutor
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(EnsureSecurityProfileAfterMigrationExecutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();

        public Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to EnsureSecurityProfile task.");
                var tenants = TenantService.GetAllAvailableTenantInfo().ToDictionary(item => item.TenantId, item => item.RegisterEmail);
                foreach (var tenant in tenants)
                {
                    TenantUtil.RunUnderTenant(tenant.Key, tenant.Value, async () =>
                    {
                        try
                        {
                            if (NeedUpgrade(tenant.Key))
                            {
                                logger.Info($"Start EnsureSecurityProfile ,Tenant: {tenant.Key}");
                                await RunUpgradeAsync(tenant.Key);
                                logger.Info($"End EnsureSecurityProfile,Tenant: {tenant.Key}");
                            }
                            else
                            {
                                logger.Info($"Skip EnsureSecurityProfile, Because no need upgrade,Tenant: {tenant.Key} ");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"An error occurred while EnsureSecurityProfile in tenant: {tenant.Key}. Error:{e}");
                        }
                    });
                }
                logger.Info("Finish to EnsureSecurityProfile task.");
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while EnsureSecurityProfile. ERROR:{0}", e.ToString());
            }
            return Task.CompletedTask;
        }

        public bool NeedUpgrade(string tenantId)
        {
            if (RMTenantUpgradeHelper.IsNeedUpgrade(tenantId, RMUpgradeFeature.EnsureSecurityProfile))
            {
                var lastestMigrationJob = JobMonitorService.GetJobsByJobType(JobType.CloudArchiverMigration)
                    .OrderByDescending(j => j.StartTime).FirstOrDefault();
                if (lastestMigrationJob != null)
                {
                    if (lastestMigrationJob.Status == (int)JobStatus.InProgress)
                    {
                        logger.Info($"Tenant: {tenantId}, Migration job is running.");
                        return false;
                    }
                    else if (lastestMigrationJob.Status == (int)JobStatus.Finished)
                    {
                        if (TenantService.IsNewOpusTenant())
                        {
                            logger.Info($"Tenant: {tenantId}, Need execute upgrade.");
                            return true;
                        }
                        else
                        {
                            logger.Warn($"Tenant: {tenantId}, May be tenants have migrated again.");
                            return false;
                        }
                    }
                }
                else
                {
                    logger.Info($"Tenant: {tenantId}, There is no migration job for this tenant.");
                    RMTenantUpgradeHelper.SetToUpgrading(tenantId);
                    RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.EnsureSecurityProfile, RMUpgradeStatus.Success);
                    return false;
                }
            }
            else
            {
                logger.Info($"Tenant: {tenantId}, This tenant does not need to upgrade.");
            }
            return false;
        }


        public async Task RunUpgradeAsync(string tenantId)
        {
            try
            {
                RMTenantUpgradeHelper.SetToUpgrading(tenantId);
                var global = GlobalStorageSettingDao.FindAll().FirstOrDefault();
                Guid? securityProfileIdInGlobal = global?.SecurityProfileId;
                if (securityProfileIdInGlobal != null && securityProfileIdInGlobal != Guid.Empty)
                {
                    var securityProfileId = await GeneralSettingService.VerfiyHasMastkeySecurityProfileAsync();
                    if (string.IsNullOrEmpty(securityProfileId))
                    {
                        await GeneralSettingService.EnsureDefaultMastkeySecurityProfileAsync(securityProfileIdInGlobal.GetValueOrDefault());
                        RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.EnsureSecurityProfile, RMUpgradeStatus.Success);
                        RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.SecurityProfileCreatedInTimer, RMUpgradeStatus.Success);

                        logger.Info($"Tenant: {tenantId}, Serurity profile is created success");
                    }
                    else
                    {
                        logger.Info($"Tenant: {tenantId}, Security profile in profile setting is exist");
                    }
                }
                else
                {
                    logger.Warn($"Tenant: {tenantId}, Security profile in global setting is non-exist, So no need to upgrade");
                }

                RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.EnsureSecurityProfile, RMUpgradeStatus.Success);
            }
            catch
            {
                RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.EnsureSecurityProfile, RMUpgradeStatus.Failed);
                throw;
            }
        }
    }
}
