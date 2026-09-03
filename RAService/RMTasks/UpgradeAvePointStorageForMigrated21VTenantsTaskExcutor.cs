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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System;

namespace AvePoint.RA.Service.RMTasks
{
    public class UpgradeAvePointStorageForMigrated21VTenantsTaskExcutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(UpgradeAvePointStorageForMigrated21VTenantsTaskExcutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                if(!DataCenterUtil.Is21V())
                {
                    logger.Info("[UpgradeAvePointStorage] Current deployment is not 21V, skip the task execution.");
                    return;
                }

                var oldConfigAveStorageConnStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.OLD_AVE_STORAGE_CONNECTION_STRING_IN_21V];
                if(string.IsNullOrEmpty(oldConfigAveStorageConnStr))
                {
                    logger.Error("[UpgradeAvePointStorage] Old config avepoint storage connection string is empty, skip the task execution.");
                    return;
                }

                (var oldConfigStorageAccountName, var oldConfigStorageAccessPoint) = AzureUtil.ParseConfigConnectionString(oldConfigAveStorageConnStr);
                logger.Debug($"[UpgradeAvePointStorage] Old config avepoint storage account name: [{oldConfigStorageAccountName}], access point: [{oldConfigStorageAccessPoint}]");
                if(string.IsNullOrEmpty(oldConfigStorageAccessPoint) || string.IsNullOrEmpty(oldConfigStorageAccountName))
                {
                    logger.Error("[UpgradeAvePointStorage] Old config avepoint storage is incorrectly, skip the task execution.");
                    return;
                }

                var tInfos = TenantService.GetAllAvailableTenantInfo();
                logger.Debug($"[UpgradeAvePointStorage] Tenant infos count: [{tInfos.Count}]");
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, async () =>
                    {
                        try
                        {
                            if (KeyValueDao.CompletedAvePointStorageUpgradeFor21V())
                            {
                                return;
                            }
                            logger.Debug($"[UpgradeAvePointStorage] Start upgrade avepoint storage for tenant [{tInfo.TenantId}]");

                            var avePointStorages = await StorageDeviceService.GetAllAvePointStorageAsync();
                            logger.Info($"[UpgradeAvePointStorage] There are [{avePointStorages.Count}] avepoint storages");

                            foreach (var oldAveStorage in avePointStorages)
                            {
                                var (oldAccountName, oldAccessPoint) = AzureUtil.ParseSavedConnectionString(oldAveStorage.ConnectionString);
                                logger.Debug($"[UpgradeAvePointStorage] Old saved avepoint storage account name: [{oldAccountName}], access point: [{oldAccessPoint}]");
                                if (oldConfigStorageAccountName.Equals(oldAccountName, StringComparison.OrdinalIgnoreCase) && oldConfigStorageAccessPoint.Equals(oldAccessPoint, StringComparison.OrdinalIgnoreCase))
                                {
                                    logger.Info($"[UpgradeAvePointStorage] Start upgrade avepoint storage for tenant [{tInfo.TenantId}|{oldAveStorage.Id}]");
                                    var newAveStorageString = StorageDeviceService.GetDefaultStorageConnectionString();
                                    //logger.Info($"Upgrade avepoint storage for tenant [{tInfo.TenantId}|{oldAveStorage.Id}] to {newAveStorageString}");
                                    await StorageDeviceService.UpdateAveStorageFor21VAsync(oldAveStorage.Id, newAveStorageString);
                                    logger.Info($"[UpgradeAvePointStorage] Success upgrade avepoint storage for tenant [{tInfo.TenantId}|{oldAveStorage.Id}]");
                                }
                                else
                                {
                                    logger.Warn($"[UpgradeAvePointStorage] The saved old avepoint storage isn't equals to config old avepoint storage [{tInfo.TenantId}], accountName: [{oldAccountName}], accessPoint: [{oldAccessPoint}]");
                                }
                            }

                            KeyValueDao.SetAvePointStorageUpgradeFor21VCompletedFlag();

                            logger.Debug($"[UpgradeAvePointStorage] Finish upgrade avepoint storage for tenant [{tInfo.TenantId}]");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"[UpgradeAvePointStorage] Error occurred while upgrading avepoint storage for tenant [{tInfo.TenantId}], ERROR:{ex}");
                        }

                    });
                }

                logger.Info("[UpgradeAvePointStorage] Completed upgrade avepoint storage for migrated 21V tenants.");
            }
            catch (Exception ex)
            {
                logger.Error("[UpgradeAvePointStorage] Error occurred while upgrading avepoint storage, ERROR:{0}", ex.ToString());
            }
        }

    }
}
