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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using Storage;
    #endregion

    public class ArchiverRetentionInfo
    {
        public String FarmName { get; set; }
        public String ConnectionId { get; set; }
        public String WebApp { get; set; }
        public String JobId { get; set; }
        public String IndexVolume { get; set; }
        public String DataVolume { get; set; }
        public String StoragePolicyId { get; set; }
        public Int64 ArchiverBackupTime { get; set; }
        public bool RemoveOrphanedStub { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public LogicalDeviceDto DataLogicalDevice { get; set; }
        public LogicalDeviceDto DestinationDevice { get; set; }
        public MediaArchiverRetentionAction RetentionAction { get; set; }
        public String DestinationPhysicalDeviceId { get; set; }
        public ServiceDto MediaService { get; set; }
        public RetentionRule RetentionRule { get; set; }
        public Boolean hasStorageInfo { get; set; }
        public Int64 RetentionTimeSpanSeconds { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public String MainIndexStorageInfo { get; set; }
        public String SubIndexStorageInfo { get; set; }
        public AccessTierType AccessTierType { get; set; }
        /// <summary>
        /// 表示删除数据成功后是否删除job记录
        /// </summary>
        public bool IsDeleteJob { get; set; }
        /// <summary>
        /// 表示retention成功还是失败,2是成功,3是失败
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// media用于更新retention job进度等信息
        /// </summary>
        public SOJob RetentionJob { get; set; }

        public bool DestinationStoreInArchiverTier { get; set; }

        public string TenantGroupId { get; set; }
        #region for retention by modified time
        public int KeepValue { get; set; }
        public DateUnit ArchiveDateUnit { get; set; }
        public KeepDateType RetentionDataTimeType { get; set; }
        public long DateTimeNow { get; set; }
        #endregion

        public bool IsFileLevelBlockBackup { get; set; }
        public bool IsSoftDelete { get; set; }
        public bool IsFitSoftDelete { get; set; }
        public string CurrentStorageId { get; set; }
        public int SoftDeleteKeepValue { get; set; }
        public DateUnit SoftDeleteDateUnit { get; set; }
        public Int64 SoftDeleteTime { get; set; }

        public bool IsSimulateJob { get; set; }
        public string RetentionSourceName { get; set; }
        public int SourceFlag { get; set; }

        public ArchiverRetentionInfo() { }

        public ArchiverRetentionInfo(ArchiverPruningJob info)
        {
            this.FarmName = info.FarmName;
            this.JobId = info.JobId;
            this.ConnectionId = info.SiteId;
            this.WebApp = info.WebApp;
            this.ArchiverBackupTime = info.ArchiverBackupTime;
            this.RemoveOrphanedStub = info.RemoveOrphanedStub;
            this.StoragePolicyId = info.StoragePolicyId;
            this.MediaService = info.MediaService;
            this.RetentionAction = info.RetentionAction;
            this.RetentionJob = info.RetentionJob;
            this.DestinationPhysicalDeviceId = info.DestinationPhysicalDeviceId;
            this.DataLogicalDevice = info.DataLogicalDevice;
            this.IndexLogicalDevice = info.IndexLogicalDevice;
            this.IsDeleteJob = info.IsDeleteJob;
            this.MainIndexStorageInfo = info.MainIndexStorageInfo;
            this.SubIndexStorageInfo = info.SubIndexStorageInfo;
            this.DestinationDevice = info.DestinationDevice;
            var volumeGenerator = new ArchiverVolumeGenerator();
            var volumeParam = new VolumeParameter() { ConnectionId = this.ConnectionId, ConnectionName = this.ConnectionId };
            this.RetentionTimeSpanSeconds = info.RetentionTimeSpanSeconds;
            this.DataVolume = volumeGenerator.GenerateDataVolume(volumeParam);
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(volumeParam);
            this.CacheSetting = info.CacheSettings;
            this.DestinationStoreInArchiverTier = info.NeedStoreInArchiverTier;
            this.TenantGroupId = info.TenantGroupId;
            this.AccessTierType = (AccessTierType)info.AccessTierType;
            this.KeepValue = info.KeepValue;
            this.ArchiveDateUnit = info.ArchiveDateUnit;
            this.RetentionDataTimeType = info.RetentionDataTimeType;
            this.DateTimeNow = info.DateTimeNowTicks;
            this.IsSoftDelete = info.IsSoftDelete;
            this.IsFitSoftDelete = info.IsFitSoftDelete;
            this.CurrentStorageId = info.CurrentStoragePolicyId;
            this.SoftDeleteTime = info.SoftDeleteTime;
            this.SoftDeleteKeepValue = info.SoftDeleteKeepValue;
            this.SoftDeleteDateUnit = info.SoftDeleteDateUnit;
            this.IsSimulateJob = info.IsSimulateJob;
            this.RetentionSourceName = info.RetentionSourceName;
            this.SourceFlag = info.SourceFlag;
        }

        public override String ToString()
        {
            return String.Format("FarmName: {0}, SiteUrl: {1}, JobId: {2} StoragePolicyId: {3}.",
                    this.FarmName,
                    this.ConnectionId,
                    this.JobId,
                    this.StoragePolicyId);
        }

    }
    public enum AccessTierType
    {
        Other,
        Hot,
        Cool,
        Archive,
        Cold
    }
}