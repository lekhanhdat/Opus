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
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.ArchiverMigration.ArchiverMigration
{
    public class MigrationHistoryDataService
    {
        protected RALogger logger = RALogger.GetInstance(typeof(SPNodeMigrationService));

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexService SiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private ISettingProfileService ProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IArchiverDedupInfoDao ArchiverDedupInfoDao => PlatformWindsorManager.GetService<IArchiverDedupInfoDao>();


        public async Task ClearAllMigratedHistoryData()
        {
            await ResetOpusRuleStorage();

            var count = await StorageDeviceService.DeleteMigratedStorageDevicesAsync();
            logger.Info($"Delete migrated storages : {count}");

            count = await SiteMasterIndexService.DeleteMigratedSiteMasterIndexesAsync();
            logger.Info($"Delete migrated SiteMasterIndexes : {count}");

            count = await SiteMasterIndexService.DeleteMigratedIndexSubInfoesAsync();
            logger.Info($"Delete migrated IndexSubInfoes : {count}");

            count = await RMRuleDao.DeleteMigratedRuleContainerMembershipsAsync();
            logger.Info($"Delete migrated RuleContainerMemberships : {count}");

            count = await RMRuleDao.DeleteMigratedRulesAsync();
            logger.Info($"Delete migrated RMRules : {count}");

            count = await MiscProfileDao.DeleteMigratedMiscProfilesAsync();
            logger.Info($"Delete migrated RMMiscProfiles : {count}");

            count = await ArchiverSettingDao.DeleteMigratedArchiverSettings();
            logger.Info($"Delete migrated RMArchiverSettings : {count}");

            count = await ArchiverSettingDao.DeleteMigratedRuleMappings();
            logger.Info($"Delete migrated RMRuleMappings : {count}");

            count = await ArchiverSettingDao.DeleteMigratedSchedulesAsync();
            logger.Info($"Delete migrated RMSchedules : {count}");

            count = await ProfileService.DeleteMigratedSettingProfilesAsync();
            logger.Info($"Delete migrated RMSettingProfiles : {count}");

            ExportSettingService.DeleteMigratedVeoConfig();
            logger.Info($"Delete migrated VEO config");

            count = await JobMonitorService.DeleteMigratedArchiverJobsAsync();
            logger.Info($"Delete migrated Archiver Sub Jobs : {count}");

            count = await JobMonitorService.DeleteMigratedMainJobsAsync();
            logger.Info($"Delete migrated Archiver Main Jobs : {count}");

            count = await ArchiverDedupInfoDao.DeleteMigratedDataAsync();
            logger.Info($"Delete migrated dedup infoes : {count}");

        }

        private async Task ResetOpusRuleStorage()
        {
            var recordsRules = await RMRuleDao.GetRulesWithoutRemovedAsync();
            recordsRules = recordsRules.Where(r => r.ModelType == (int)RuleModel.None || r.ModelType == (int)RuleModel.Records).ToList();
            foreach (var recordsRule in recordsRules)
            {
                var rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(recordsRule.Extension);
                ResetStoragePolicy(rule);
                recordsRule.Extension = SerializerHelper.SerializeByDataContractJsonSerializer(rule);
            }
            RMRuleDao.BatchUpdate(recordsRules.ToList());
        }
        private void ResetStoragePolicy(Rule opusRule)
        {
            ResetStoragePolicyForSingleRuleAsync(opusRule);
            ResetStoragePolicyForSingleRuleAsync(opusRule.OneDriveRule);
            ResetStoragePolicyForSingleRuleAsync(opusRule.PhysicalRule);
        }
        private Dictionary<Guid, RMStorageDeviceInfo> storageDic = new();
        private void ResetStoragePolicyForSingleRuleAsync(Rule opusRule)
        {
            if (opusRule == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(opusRule.StoragePolicyId))
            {
                var storageId = new Guid(opusRule.StoragePolicyId);
                if (!storageDic.TryGetValue(storageId, out RMStorageDeviceInfo? storage))
                {
                    storage = StorageDeviceDao.GetStorageDevicesById(storageId);
                    storageDic.Add(storageId, storage);
                    logger.Info($"put storage cache, name:{opusRule.StoragePolicyName}/{opusRule.StoragePolicyId}");
                }
                else
                {
                    logger.Info($"get storage cache, name:{opusRule.StoragePolicyName}/{opusRule.StoragePolicyId}");
                }
                if (storage != null)
                {
                    logger.Info($"get rule [{opusRule.Name}] storage from opus db, name:{opusRule.StoragePolicyName}/{opusRule.StoragePolicyId}");
                    if (!string.IsNullOrEmpty(storage.DAOStoragePolicyId))
                    {
                        logger.Info($"reset rule [{opusRule.Name}] storage {opusRule.StoragePolicyId} -> {storage.DAOStoragePolicyId}");
                        opusRule.StoragePolicyId = storage.DAOStoragePolicyId;
                    }
                }
                //else
                //{
                //    var client = new DAOAPIClientV1();
                //    logger.Info($"get rule [{opusRule.Name}] storage from dao, name:{opusRule.StoragePolicyName}");
                //    var rule = client.LoadRule(opusRule.Id);
                //    if (rule != null)
                //    {
                //        opusRule.StoragePolicyId = rule.StoragePolicyId;
                //    }
                //}
            }
        }
    }
}
