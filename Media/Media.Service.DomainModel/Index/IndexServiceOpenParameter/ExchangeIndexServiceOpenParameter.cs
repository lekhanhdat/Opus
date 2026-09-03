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


namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using AvePoint.Media.Common;
    using Storage;
    using GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using System;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Media.Object;

    #endregion using directives

    public class ExchangeIndexServiceOpenParameter : IndexServiceOpenParameter
    {
        public string UserAddress { get; set; }
        public string ObjectId { get; set; }

        public MailboxType MailboxType { get; set; }

        public string SourceIndexVolume { get; set; }

        public IXSystem SourceIndexLogicalDeviceSystem { get; set; }
        public bool IsOversizeIndexDB { get; set; }
        public string OversizeDBConnectionPassword { get; set; }
        /// <summary>
        /// Whether the current node is allowed to use objectId.
        /// </summary>
        public bool SupportObjectId { get; set; }

        public long Timestamp { get; set; }

        public bool UseReserveIndex { get; set; }

        public bool? IsForceUpgrade { get; set; }
        public int WaitIndexLockerTimeOutInMs { get; set; } = 30 * 60 * 1000;

        public bool NeedCheckNewDB { get; set; } = true;
        public TreeMode TreeMode { set; get; }
        public string TenantGroupId { get { return IdentityManager.IdentityContent; } }

        public ExchangeIndexServiceOpenParameter()
        {
        }
        public ExchangeIndexServiceOpenParameter(string subSubJobId, IXSystem indexLogicalDevice, IXSystem indexCacheDevice, String indexVolume)
        {
            IndexVolume = indexVolume;
            BackupJobId = subSubJobId;
            IndexDatabaseName = subSubJobId + "_" + ServiceConstants.IndexDBName;
            IndexLogicalDeviceSystem = indexLogicalDevice;
            IndexCacheDeviceSystem = indexCacheDevice;
        }
        private ExchangeIndexServiceOpenParameter(ExchangeIndexServiceOpenParameter param)
        {
            IndexVolume = param.IndexVolume;
            IndexCacheDeviceSystem = param.IndexCacheDeviceSystem;
            BackupJobId = param.BackupJobId;
            IndexLogicalDeviceSystem = param.IndexLogicalDeviceSystem;
            StorageInfo = param.StorageInfo;
            IsNeedCreateNewIndex = param.IsNeedCreateNewIndex;
            CacheSetting = param.CacheSetting;
            UserAddress = param.UserAddress;
            MailboxType = param.MailboxType;
            ObjectId = param.ObjectId;
            EncryptionInfo = param.EncryptionInfo;
            SupportObjectId = param.SupportObjectId;
            IsNeedBackupIndex = param.IsNeedBackupIndex;
            DataMode = param.DataMode;
            Timestamp = param.Timestamp;
            IndexDatabaseName = param.IndexDatabaseName;
        }

        public ExchangeIndexServiceOpenParameter(ExchangeBrowseInfo browserInfo, IXSystem cacheSystem, IXSystem indexDevice)
        {
            IndexVolume = browserInfo.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            BackupJobId = browserInfo.BackupJobId;
            IndexLogicalDeviceSystem = indexDevice;
            StorageInfo = browserInfo.StorageInfo;
            CacheSetting = browserInfo.CacheSetting;
            UserAddress = browserInfo.UserAddress;
            MailboxType = browserInfo.MailboxType;
            ObjectId = browserInfo.ObjectId;
            EncryptionInfo = browserInfo.IndexEncryptionInfoWrapper?.EncryptionInfo;
            SupportObjectId = browserInfo.SupportObjectId;
            Timestamp = browserInfo.EndTime;
            //NeedCheckNewDB = browserInfo.Level != TreeNodeLevel.PowerAutomate && browserInfo.Level != TreeNodeLevel.PowerApps;
        }

        public ExchangeIndexServiceOpenParameter(ExchangeBackupJob backupJob, string currentUserAddress, IXSystem cacheSystem, IXSystem indexDevice, bool isNeedBackupIndex = false, bool isNeedCheckIntegrity = false)
        {
            IndexCacheDeviceSystem = cacheSystem;
            IndexVolume = backupJob.IndexVolume;
            CacheSetting = backupJob.CacheSetting;
            IndexLogicalDeviceSystem = indexDevice;
            StorageInfo = backupJob.StorageInfo;
            IsNeedCreateNewIndex = true;
            BackupJobId = backupJob.JobId;
            UserAddress = currentUserAddress;
            DataMode = backupJob.DataMode;
            EncryptionInfo = backupJob.IndexEncryptionInfoWrapper?.EncryptionInfo;
            IsNeedBackupIndex = isNeedBackupIndex;
            IsNeedCheckIntegrity = isNeedCheckIntegrity;
        }

        public ExchangeIndexServiceOpenParameter(ExchangeBackupJob backupJob, IXSystem cacheSystem, IXSystem indexDevice)
        {
            IndexVolume = backupJob.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            CacheSetting = backupJob.CacheSetting;
            BackupJobId = backupJob.JobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = TreeMode.JobMode;
            IsNeedCreateNewIndex = true;
        }

        /// <summary>
        /// 支持 Exchagne module
        /// </summary>
        public ExchangeIndexServiceOpenParameter(ExchangeBackupJob backupJob, string currentUserAddress, string currentObjectId, IXSystem cacheSystem, IXSystem indexDevice, bool supportObjectId) : this(backupJob, currentUserAddress, cacheSystem, indexDevice)
        {
            ObjectId = currentObjectId;
            SupportObjectId = supportObjectId;
        }
        /// <summary>
        /// 支持 Exchagne module
        /// </summary>
        public ExchangeIndexServiceOpenParameter(ExchangeRestoreJob restoreJob, IXSystem cacheSystem, IXSystem indexDevice, string currentUserAddress, MailboxType currentMailboxType, string currentObjectId, bool supportObjectId) : this(restoreJob, cacheSystem, indexDevice, currentUserAddress, currentMailboxType)
        {
            ObjectId = currentObjectId;
            SupportObjectId = supportObjectId;
            Timestamp = restoreJob.BackupTime;
        }
        public ExchangeIndexServiceOpenParameter(ExchangeRestoreJob restoreJob, IXSystem cacheSystem, IXSystem indexDevice, string currentUserAddress, MailboxType currentMailboxType)
        {
            IndexCacheDeviceSystem = cacheSystem;
            IndexVolume = restoreJob.IndexVolume;
            CacheSetting = restoreJob.CacheSetting;
            IndexLogicalDeviceSystem = indexDevice;
            StorageInfo = restoreJob.IndexStorageInfoMap.ContainsKey(currentUserAddress) ? restoreJob.IndexStorageInfoMap[currentUserAddress] : string.Empty;
            BackupJobId = restoreJob.JobId;
            UserAddress = currentUserAddress;
            MailboxType = currentMailboxType;
            EncryptionInfo = restoreJob.IndexEncryptionInfoWrapper?.EncryptionInfo;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override ExchangeIndexServiceOpenParameter DeepClone()
        {
            return new ExchangeIndexServiceOpenParameter(this);
        }
    }
}