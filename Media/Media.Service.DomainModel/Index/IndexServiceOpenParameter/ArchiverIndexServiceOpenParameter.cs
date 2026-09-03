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
    using AvePoint.Common;
    #region using directives

    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Common;
    using Storage;
    using System;
    using System.Collections.Generic;

    #endregion using directives

    public class ArchiverIndexServiceOpenParameter : IndexServiceOpenParameter
    {
        public TreeMode TreeMode { set; get; }
        public String SiteUrl { get; set; }
        public bool CheckAccessTier { get; set; }

        public bool IsEndUserRequest { get; set; }

        public int WaitIndexLockerTimeOutInMs { get; set; } = 30 * 60 * 1000;

        public List<EndUserRestoreItem> EndUserRequestItems { get; set; }

        public string TenantGroupId { get { return IdentityManager.IdentityContent; } }

        public ArchiverIndexServiceOpenParameter() { }

        public ArchiverIndexServiceOpenParameter(ArchiverBrowseInfo browserInfo, IXSystem cacheSystem, IXSystem indexDevice)
        {
            IndexVolume = browserInfo.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            BackupJobId = browserInfo.BackupJobId;
            IndexLogicalDeviceSystem = indexDevice;
            StorageInfo = browserInfo.StorageInfo;
            TreeMode = browserInfo.TreeMode;
        }

        public ArchiverIndexServiceOpenParameter(ArchiverBackupJob backupJob, IXSystem cacheSystem, IXSystem indexDevice)
        {
            IndexVolume = backupJob.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            CacheSetting = backupJob.CacheSetting;
            BackupJobId = backupJob.JobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = TreeMode.JobMode;
            IsNeedCreateNewIndex = true;
        }
        public ArchiverIndexServiceOpenParameter(ExchangeBackupJob backupJob, IXSystem cacheSystem, IXSystem indexDevice)
        {
            IndexVolume = backupJob.IndexVolume;
            IndexCacheDeviceSystem = cacheSystem;
            CacheSetting = backupJob.CacheSetting;
            BackupJobId = backupJob.JobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = TreeMode.JobMode;
            IsNeedCreateNewIndex = true;
        }
        public ArchiverIndexServiceOpenParameter(ArchiverRestoreJob restoreJob, IXSystem indexDevice)
        {
            IndexVolume = restoreJob.IndexVolume;
            BackupJobId = restoreJob.BackupJobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = restoreJob.TreeMode;
            CacheSetting = restoreJob.CacheSetting;
        }

        public ArchiverIndexServiceOpenParameter(ArchiverExportJob exportJob, IXSystem indexDevice)
        {
            IndexVolume = exportJob.IndexVolume;
            BackupJobId = exportJob.BackupJobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = exportJob.TreeMode;
        }

        public ArchiverIndexServiceOpenParameter(ArchiverRetentionInfo archiverRetentionInfo, IXSystem indexDevice, String indexVolume)
        {
            CacheSetting = archiverRetentionInfo.CacheSetting;
            IndexVolume = indexVolume;
            BackupJobId = archiverRetentionInfo.JobId;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = TreeMode.SiteCollectionMode;
            IsNeedCreateNewIndex = true;
        }

        public ArchiverIndexServiceOpenParameter(MergeIndexJobState mergeIndexJobsState, IXSystem indexLogicalDevice, IXSystem indexCacheDevice, String indexVolume)
        {
            IndexVolume = indexVolume;
            BackupJobId = mergeIndexJobsState.JobId;
            IndexDatabaseName = mergeIndexJobsState.JobId + "_" + ServiceConstants.IndexDBName;
            IndexLogicalDeviceSystem = indexLogicalDevice;
            IndexCacheDeviceSystem = indexCacheDevice;
        }

        public ArchiverIndexServiceOpenParameter(EndUserBrowseInfo browserInfo, IXSystem indexDevice)
        {
            IndexVolume = browserInfo.IndexVolume;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = browserInfo.TreeMode;
        }

        public ArchiverIndexServiceOpenParameter(ErrorPageCheckInfo checkInfo, IXSystem indexDevice)
        {
            IndexVolume = checkInfo.IndexVolume;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = checkInfo.TreeMode;
        }

        public ArchiverIndexServiceOpenParameter(EndUserDownloadInfo downloadInfo, IXSystem indexDevice)
        {
            IndexVolume = downloadInfo.IndexVolume;
            IndexLogicalDeviceSystem = indexDevice;
            TreeMode = downloadInfo.TreeMode;
        }

        public ArchiverIndexServiceOpenParameter(IXSystem indexLogicalDevice, String indexVolume, String IndexDBName)
        {
            IndexLogicalDeviceSystem = indexLogicalDevice;
            IndexVolume = indexVolume;
            IndexDatabaseName = IndexDBName;
            TreeMode = TreeMode.SiteCollectionMode;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}