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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Core;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.StorageDevice;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateSettingProfilesStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Migrate SettingProfiles";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(1, 3);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_SettingProfile";

        private List<SettingProfileDto> migrateProfiles;

        private ISettingProfileService ProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        protected override async Task PreExecuteAsync()
        {
            migrateProfiles = await GetAllSettingProfilesAsync();
            JobProgressUpdater.Increase(1);
        }

        protected override async Task InnerExecuteAsync()
        {
            logger.Info("Start migrate setting profiles.");
            List<SettingProfileDto> needRemovedMigrateProfiles = new List<SettingProfileDto>();
            foreach (var profile in migrateProfiles)
            {
                if (profile.Type == (int)SettingProfilesType.IndexDevice)
                {
                    logger.Info($"Convert DAOIndexDeviceId to opus storage device id");
                    var (_, indexStorageDevice, _) = await JobExecutor.StorageMigrationService
                        .GetStorageDeviceByDAOLogicalDeviceIdAsync(profile.Settings);
                    if (indexStorageDevice != null)
                    {
                        profile.Settings = indexStorageDevice.Id;
                    }
                    else
                    {
                        AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, profile.Name, "RM_JS_ArchiverMigration_Comment_ConvertIndexDeviceFailed");
                    }
                }
                else if (profile.Type == (int)SettingProfilesType.DBSEEMasterKey)
                {
                    //对于老客户，没有跑过备份job，此时DBSEEMasterKey为空，不migration此属性，由OPUS自己重新生成.
                    if (string.IsNullOrEmpty(profile.Settings))
                    {
                        logger.Info("InnerExecuteAsync.Migrate setting profiles DBSEEMasterKey is empty,so skip sync this setting to OPUS.");
                        needRemovedMigrateProfiles.Add(profile);
                    }
                    else
                    {
                        profile.Settings = JobExecutor.AesEncryptorWrapper.Encrypt(profile.Settings);
                    }
                }
                //对于没有leave stub的老客户，EndUserStubLinkMasterKey为空，不migration此属性，由OPUS自己重新生成.
                else if (profile.Type == (int)SettingProfilesType.EndUserStubLinkMasterKey && string.IsNullOrEmpty(profile.Settings))
                {
                    logger.Info("InnerExecuteAsync.Migrate setting profiles EndUserStubLinkMasterKey is empty,so skip sync this setting to OPUS.");
                    needRemovedMigrateProfiles.Add(profile);
                }
            }
            if (needRemovedMigrateProfiles != null && needRemovedMigrateProfiles.Count > 0)
            {
                foreach (var needRemoveProfile in needRemovedMigrateProfiles)
                {
                    migrateProfiles.Remove(needRemoveProfile);
                    logger.Info($"InnerExecuteAsync.Success remove:{needRemoveProfile.Type}.");
                }
            }

            if (!string.IsNullOrWhiteSpace(JobExecutor.JobSettings.ExportLocationId))
            {
                logger.Info($"Convert ExportLocationLogicalDeviceId to opus storage device");
                var exportLocationStorage = await GetExportLocationAsync();
                if(exportLocationStorage != null)
                {
                    JobExecutor.StorageMigrationService.TryRenameIfNameExists(exportLocationStorage);
                    await StorageDeviceService.CreateStorageDeviceAsync(exportLocationStorage, EntityObjectPermissionType.FullPermission);
                    logger.Info($"Migrate storage: {exportLocationStorage.Name} | {exportLocationStorage.Id}, StoragePolicyId: {exportLocationStorage.DAOStoragePolicyId}, LogicalDeviceId: {exportLocationStorage.DAOLogicalDeviceId}, PhysicalDeviceId: {exportLocationStorage.DAOPhysicalDeviceId}");

                    migrateProfiles.Add(new()
                    {
                        DAOMigrated = true,
                        Id = Guid.NewGuid().ToString(),
                        Name = "UsingExportLocationDevice",
                        Settings = exportLocationStorage.Id,
                        Type = (int)SettingProfilesType.ExportLocationDevice
                    });
                }
                else
                {
                    AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "UsingExportLocationDevice", "RM_JS_ArchiverMigration_Comment_ConvertExportLocationFailed");
                }
            }
            JobProgressUpdater.Increase(1);


            await ProfileService.BatchCreateAsync(migrateProfiles);

            migrateProfiles.ForEach(p => AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful, p.Name));
            JobProgressUpdater.Increase(1);
        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return Task.FromResult(4);
        }

        private async Task<List<SettingProfileDto>> GetAllSettingProfilesAsync()
        {
            return await GetArchiverMigrationDataAsync<List<SettingProfileDto>>((service) =>
            {
                return service.GetAllSettingProfiles();
            });
        }

        private async Task<StorageDeviceDto> GetExportLocationAsync()
        {
            return await GetArchiverMigrationDataAsync<StorageDeviceDto>((service) =>
            {
                return service.GetExportLocationStorage(JobExecutor.JobSettings.ExportLocationId);
            });
        }
    }
}
