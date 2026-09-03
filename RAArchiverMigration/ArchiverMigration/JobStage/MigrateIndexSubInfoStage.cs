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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Encryption;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateIndexSubInfoStage : AbstractArchiverMigrationStage
    {
        public override string StageType => "Migrate ArchiverIndexSubInfoes";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_IndexData";

        private IArchiverSiteMasterIndexService SiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private ILoginService LoginService => PlatformWindsorManager.GetService<ILoginService>();


        protected override async Task InnerExecuteAsync()
        {
            List<ArchiverIndexSubInfoContract> indexSubInfoes = null;
            var encryptProfileId = JobExecutor.GlobalStorageSetting?.SecurityProfileId;
            if (encryptProfileId == null || encryptProfileId == Guid.Empty)
            {
                logger.Info($"Need init security profile.");
                await LoginService.InitSecurityProfileAsync();
                encryptProfileId = (await GeneralSettingService.VerifyAndCreateDefaultSecurityProfileAsync()).Item1;
            }
            else
            {
                logger.Info($"Need create the security profile id in setting profile");
                await GeneralSettingService.EnsureDefaultMastkeySecurityProfileAsync(encryptProfileId.GetValueOrDefault());
            }

            int fetchSize = 2000;
            int offset = 0;

            do
            {
                logger.Info($"Fetch index sub infoes from {offset}");
                indexSubInfoes = await FetchIndexSubInfoesAsync(offset, fetchSize);

                var count = indexSubInfoes?.Count ?? 0;
                logger.Info($"Fetched index sub infoes: {count}");

                foreach (var indexSubInfo in indexSubInfoes)
                {
                    try
                    {
                        logger.Info($"Migrate index sub info: {indexSubInfo.JobId}");
                        indexSubInfo.SourceFlag = (int)GetSourceFlag(indexSubInfo);
                        indexSubInfo.DataFlag = indexSubInfo.SourceFlag;
                        var storageDevice = JobExecutor.StorageMigrationService.GetStorageDeviceByDAOStoragePolicyId(indexSubInfo.StoragePolicyId);
                        indexSubInfo.StoragePolicyId = storageDevice?.Id ?? Guid.Empty.ToString();

                        var (_, currentStorageDevice, fromCache) = JobExecutor.StorageMigrationService.GetStorageDeviceByDAOPhysicalDeviceId(indexSubInfo.CurrentStorageId);
                        indexSubInfo.CurrentStorageId = currentStorageDevice?.Id ?? Guid.Empty.ToString();

                        if (storageDevice != null && currentStorageDevice != null && storageDevice.DAOPhysicalDeviceId == currentStorageDevice.DAOPhysicalDeviceId)
                        {
                            indexSubInfo.CurrentStorageId = indexSubInfo.StoragePolicyId;
                        }

                        if (currentStorageDevice == null && !fromCache)
                        {
                            AddJobDetail(JobDetailsStatus.Failed, indexSubInfo.StoragePolicyId, "RM_JS_ArchiverMigration_Comment_ConvertPhysicalDeviceFailed");
                        }

                        if (indexSubInfo.ArchiverSubInfoExtension != null)
                        {
                            var (_, primaryStorageDevice, _) = await JobExecutor.StorageMigrationService
                                .GetStorageDeviceByDAOLogicalDeviceIdAsync(indexSubInfo.ArchiverSubInfoExtension.PrimaryLogicalId);
                            indexSubInfo.ArchiverSubInfoExtension.PrimaryLogicalId = primaryStorageDevice?.Id ?? Guid.Empty.ToString();
                        }

                        var encryptInfo = indexSubInfo.ArchiverSubInfoExtension?.DataEncryptionInfo;
                        if (encryptInfo != null)
                        {
                            byte[] result = indexSubInfo.DataEncryptionDynamicKey;
                            var info = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                            info.EncryptionType = (int)EncryptionAlgorithm.AES_ENCRYPTION;
                            info.ProfileGuid = encryptProfileId?.ToString() ?? Guid.Empty.ToString();
                            info.ProtectionGuid = encryptInfo.ProfileGuid;
                            info.ProfileName = "Default Encryption Profile";
                            info.EncryptedDynamicKey = JobExecutor.AesEncryptorWrapper.Encrypt(result);

                            indexSubInfo.ArchiverSubInfoExtension.DataEncryptionInfo = info;
                            indexSubInfo.DataEncryptionInfo = info;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"migrate index subInfo failed: {indexSubInfo.JobId}");
                        throw;
                    }
                }

                await SiteMasterIndexService.BulkCopyIndexSubInfoesAsync(indexSubInfoes);

                JobProgressUpdater.Increase(indexSubInfoes.Count);
                indexSubInfoes.ForEach(i => AddJobDetail(JobDetailsStatus.Successful, i.JobId));

                offset += count;
            } while (indexSubInfoes != null && indexSubInfoes.Count >= 2000);

        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return GetArchiverMigrationDataAsync<int>((service) =>
            {
                return service.GetAllIndexSubInfoesCount();
            });
        }

        private SourceFlag GetSourceFlag(ArchiverIndexSubInfoContract subInfo)
        {
            var mainJobId = subInfo.JobId.Substring(0, subInfo.JobId.LastIndexOf('_'));
            if(JobExecutor.ArchiverJobIdAndSourceFlagMappings.TryGetValue(mainJobId, out var sourceFlag))
            {
                return sourceFlag;
            }
            return SourceFlag.None;
        }


        private async Task<List<ArchiverIndexSubInfoContract>> FetchIndexSubInfoesAsync(int offset, int fetchSize)
        {
            var data = await GetArchiverMigrationDataAsync<List<ArchiverIndexSubInfoContract>>((service) =>
            {
                return service.GetIndexSubInfoes(new() { Offset = offset, FetchSize = fetchSize});
            });
            return data;
        }
    }
}
