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
using AvePoint.Archiver.Media;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Media.Service.ArchiverBackup.LogicBackup;
using RAArchiverCommon;
using RAGoogle.Common;
using RAGoogle.Helper;

namespace RAGoogle.Archive.Media
{
    class MediaServerManagementUtil
    {
        private IRALogger Logger = RALogger.GetInstance(typeof(MediaServerManagementUtil));
        private int subJobNumber = 0;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();        
        internal GDriveBackupJob GDriveBackupJob { get; set; }
        internal GDriveBackupRequest GDriveBackupRequest { get; set; }
        public MediaServerManagementUtil()
        {
            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();
            MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
            MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
        }
        public BackupInfoSender ConfigMedia(string ruleId, string subJobId, GoogleConfiguration configuration, ref string IndexJobId, SourceFlag sourceFlag, SourceFlag dataFlag)
        {
            GDriveBackupRequest = new GDriveBackupRequest();
            GDriveBackupRequest.RuleId = ruleId;
            //aRequest.SourceFlag = (int)sourceFlag;
            //aRequest.DataFlag = (int)dataFlag;
            GDriveBackupRequest.JobId = GenerageSubJobId(subJobId);//subJobId;
            IndexJobId = GDriveBackupRequest.JobId;
            var currentRule = configuration.currentRule.GoogleDriveRule;
            StoragePolicyDto storage = currentRule.StoragePolicyDto;
            GDriveBackupRequest.UseSnapLock = currentRule.UseSnapLock;
            GDriveBackupRequest.UseArchiverTier = currentRule.IsArchivedTier;
            GDriveBackupRequest.StoragePolicyId = storage.Id;
            GDriveBackupRequest.AchiverTime = configuration.ArchiverUNCTime.Ticks;
            //set RetentionTimeSpan
            if (storage.RetentionOption != null && storage.RetentionOption.StorageType == StoragePolicyType.ArchiveType && storage.RetentionOption.ArchiveRetentionRules != null && storage.RetentionOption.ArchiveRetentionRules.Count > 0)
            {
                ArchiveRetentionRule retentionRule = storage.RetentionOption.ArchiveRetentionRules[0];
                long keepValue = retentionRule.KeepValue;
                switch (retentionRule.ArchiveDateUnit)
                {
                    case DateUnit.Month:
                        {
                            TimeSpan resultTime = DateTime.Now.AddMonths((int)keepValue).Subtract(DateTime.Now);
                            keepValue = resultTime.Days;
                            break;
                        }
                    case DateUnit.Week:
                        {
                            keepValue = keepValue * 7;
                            break;
                        }
                    default: break;
                }
                GDriveBackupRequest.RetentionTimeSpanSeconds = keepValue * 24 * 3600;
            }
            else
            {
                //when no retention rule ,we give RetentionTimeSpanSeconds = -1 
                GDriveBackupRequest.RetentionTimeSpanSeconds = -1;
            }

            GDriveBackupRequest.LogicalDevice = storage.PrimaryStorage;

            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            if (indexDeviceDto == null && configuration.SelectedNode.IsNodeProcessFromGControl)
            {
                Logger.Info($"not found index device.");
                StorageDeviceService.SetUsingDeviceByIdAsync(storage.Id, SettingProfilesType.IndexDevice, storage.Name).GetAwaiter().GetResult();
                Logger.Info($"set index device by id {storage.Id} successful.");
                indexDeviceDto = StorageDeviceService.GetIndexDevice();
            }
            GDriveBackupRequest.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);

            GDriveBackupRequest.CompressionType = currentRule.ArchiverCompressionType;
            GDriveBackupRequest.EncryptionMethods = currentRule.EncryptionMethods;
            GDriveBackupRequest.DataSecurity = currentRule.ArchiverDataSecurity;
            GDriveBackupRequest.DataEncryptionInfoWrapper = currentRule.DataEncryptionInfoWrapper;

            if (currentRule.DataEncryptionInfoWrapper != null)
            {
                GDriveBackupRequest.EncryptionInfo = currentRule.DataEncryptionInfoWrapper.EncryptionInfo;
                DataEncryptionInfoManager.PutEncryptionInfo(currentRule.DataEncryptionInfoWrapper.EncryptionInfo, currentRule.DataEncryptionInfoWrapper.DynamicKey);
            }
            else
            {
                GDriveBackupRequest.EncryptionInfo = DataEncryptionInfoManager.DefaultEncryptionInfo;
            }
            Logger.Info("ArchiverBackupRequest EncryptionInfo is:{0}.", GDriveBackupRequest.EncryptionInfo == null ? string.Empty : GDriveBackupRequest.EncryptionInfo.ToString());

            var driveData = ConvertHelper.ConvertDtoNodeTreeToData(configuration.SelectedNode, configuration.AppProfile.TenantId);
            GDriveBackupRequest.DriveName = driveData.Name;
            GDriveBackupRequest.DriveId = driveData.Id;
            GDriveBackupRequest.IsSharedDrive = driveData.Shared;
            GDriveBackupRequest.TenantId = driveData.TenantId;
            if (!configuration.CachedBackupJob.ContainsKey(IndexJobId))
            {
                configuration.CachedBackupJob.Add(IndexJobId, GDriveBackupRequest);
            }
            IArchiverBackupDataWriter fileSender = new GDriveArchiverBackupDataWriter();
            ConvertBackupRequestToJob(configuration);
            fileSender.OpenGDrive(GDriveBackupJob);
            return new BackupInfoSender(fileSender);
        }
        private string GenerageSubJobId(string parentJobId)
        {
            subJobNumber++;
            if (subJobNumber >= 1000)
            {
                return string.Format("{0}_{1:D4}", parentJobId, subJobNumber);
            }
            else
            {
                return string.Format("{0}_{1:D3}", parentJobId, subJobNumber);
            }
        }
        private void ConvertBackupRequestToJob(GoogleConfiguration configuration)
        {
            GDriveBackupJob = new GDriveBackupJob(GDriveBackupRequest);
            GDriveBackupJob.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            if (configuration.BackgroundSettings.GoogleOutputStreamLevel == 0)
            {
                GDriveBackupJob.OutFileLevelBlock = true;
            }
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = string.Empty,
                UserName = string.Empty,
                Usage = null
            };
            GDriveBackupJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            GDriveBackupJob.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
        }
    }
}
