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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using Server.ControlPanel.Cryptography.Wrapper;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreParamDto
    {
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String Path { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public Int64 StartTime { get; set; }

        [DataMember]
        public Int64 EndTime { get; set; }

        [DataMember]
        public Int32 OffSet { get; set; }

        [DataMember]
        public Int32 Length { get; set; }

        [DataMember]
        public Boolean OnlyOneJob { get; set; }

        [DataMember]
        public String BackupJobId { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String BackupPlanId { get; set; }

        [DataMember]
        public String BackupCycleID { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        /// <summary> 存储storage的一些信息，如：EMC存储介质的clip id,Dell存储介质的Object id。</summary>
        [DataMember]
        public string StorageInfo { get; set; }

        [DataMember]
        public Boolean IsMigrationBrowse { get; set; }

        [DataMember]
        public List<string> BackupJobIds { get; set; }

        public override String ToString()
        {
            return String.Format("Path: {0}, Level: {1}, Backup Job ID: {2}", this.Path, this.Level, this.BackupJobId);
        }
    }

    public class EORestoreParamDto
    {
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public long EndTime { get; set; }

        [DataMember]
        public int OffSet { get; set; }

        [DataMember]
        public int Length { get; set; }

        [DataMember]
        public bool OnlyOneJob { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public string BackupCycleID { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        //[DataMember]
        //public OversizeIndexDBCacheInfo OversizeIndexDBCacheInfo { get; set; }
        //[DataMember]
        //public HostIndexDBCacheInfo HostIndexDBCacheInfo { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public MailboxType MailboxType { get; set; }

        /// <summary> 存储storage的一些信息，如：EMC存储介质的clip id,Dell存储介质的Object id。</summary>
        [DataMember]
        public string StorageInfo { get; set; }

        [DataMember]
        public DataEncryptionInfoWrapper IndexEncryptionInfoWrapper { get; set; }

        [DataMember]
        public bool SupportObjectId { get; set; }

        [DataMember]
        public long RetentionEarliestTime { get; set; }

        [DataMember]
        public bool BackendPagination { get; set; }
        public override string ToString()
        {
            return string.Format("Path: {0}, Level: {1}, Backup Job ID: {2}", Path, Level, BackupJobId);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreParamDto : RestoreParamDto
    {
        [DataMember]
        public String GUID { get; set; }

        [DataMember]
        public String SiteId { get; set; }

        [DataMember]
        public String ParentId { get; set; }

        [DataMember]
        public String WebId { get; set; }

        [DataMember]
        public String DirName { get; set; }

        [DataMember]
        public String PathMD5 { get; set; }

        [DataMember]
        public Int32 ItemCountOfPage { get; set; }

        [DataMember]
        public Int32 RowId { get; set; }

        [DataMember]
        public String AgentHostName { get; set; }

        [DataMember]
        public String FarmId { get; set; }

        [DataMember]
        public String Location { get; set; }

        [DataMember]
        public String WfeHostName { get; set; }

        [DataMember]
        public String StorageInfoExtension { get; set; }

        [DataMember]
        public List<String> FullPaths { get; set; }

        // blob节点类型
        [DataMember]
        public PRSNBlobType BlobType { get; set; }

        // 当前主tree的blob节点id
        [DataMember]
        public String BlobNodeId { get; set; }

        public override String ToString()
        {
            return String.Format("GUID: {0}, Farm Id: {1}, Location: {2}, WFE Host Name: {3}",
                this.GUID,
                this.FarmId,
                this.Location,
                this.WfeHostName);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullPathInfo
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public Boolean IsExist { get; set; }

        public override String ToString()
        {
            return String.Format("Name: {0}, Is Exist: {1}", this.Name, this.IsExist);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverRestoreParamDto : RestoreParamDto
    {
        [DataMember]
        public String WebAppUrl { get; set; }
        [DataMember]
        public ArchiverLoadTreeOption LoadTreeOption { get; set; }
        [DataMember]
        public StorageDeviceDto IndexLogicalDevice { get; set; }

        public override String ToString()
        {
            return String.Format("Web Application Url: {0}, Load Tree Option: {1}, Index Logical Device: {2}",
                this.WebAppUrl,
                this.LoadTreeOption,
                this.IndexLogicalDevice);
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GDriveRestoreParamDto : ArchiverRestoreParamDto
    {
        [DataMember]
        public String DriveId { get; set; }

        [DataMember]
        public String TenantId { get; set; }
        [DataMember]
        public ArchiverLoadTreeOption LoadTreeOption { get; set; }
        [DataMember]
        public StorageDeviceDto IndexLogicalDevice { get; set; }

        public override String ToString()
        {
            return String.Format("DriveId: {0}, Load Tree Option: {1}, Index Logical Device: {2}",
                this.DriveId,
                this.LoadTreeOption,
                this.IndexLogicalDevice);
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ArchiverLoadTreeOption
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        SiteCollectionMode = 1,

        [EnumMember]
        JobMode = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreTreeBrowseResult
    {
        /// <summary>目前用于Item节点分页,标示节点总个数. </summary>
        [DataMember]
        public int TotalCounts { get; set; }

        /// <summary> Media端返回节点.</summary>
        [DataMember]
        public List<SPTreeNodeDto> ChildenNodes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreTreeBrowseResult
    {
        /// <summary>目前用于Item节点分页,标示节点总个数. </summary>
        [DataMember]
        public int TotalCounts { get; set; }

        /// <summary> Media端返回节点.</summary>
        [DataMember]
        public List<ExchangeOnlineTreeNodeDto> ChildenNodes { get; set; }
    }

    #region Blob
    /// <summary>PRSN blob状态</summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRSNBlobType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        ConnectorBlob = 1,

        [EnumMember]
        StorageManager = 2,
    }
    #endregion

}