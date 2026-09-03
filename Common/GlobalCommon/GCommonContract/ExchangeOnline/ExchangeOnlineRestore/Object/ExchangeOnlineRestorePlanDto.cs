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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineRestorePlanDto : PlanDto
    {
        [DataMember]
        public EORestoreType RestoreType { get; set; }

        [DataMember]
        public bool IncludeDetailedJobReport { get; set; }

        /// <summary> if True, means Attach, else Merge。 </summary>
        [DataMember]
        public bool RestoreContentsToSub { get; set; }

        //[DataMember]
        //public EORestoreVersionSetting RestoreVersionSetting { get; set; }

        //[DataMember]
        //public int VersionCount { get; set; }

        #region ==  Conflict resolution setting ==
        [DataMember]
        public EOConflictResolutionType ContainerConflictResolution { get; set; }

        [DataMember]
        public EOConflictResolutionType ContentConflictResolution { get; set; }

        [DataMember]
        public EODependencyOption EODependencyType { get; set; }
        #endregion ==

        //[DataMember]
        //public bool IncludeRecycleBinData { get; set; }

        [DataMember]
        public bool OnlyOneJob { get; set; }

        /// <summary> 与SiteMasterIndex数据表的JobId相关联。 </summary>
        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public EOBackupLevel BackupLevel { get; set; }

        [DataMember]
        public EORestorePlanType Type { get; set; }

        #region == 以下属性为export to file system 所使用==
        [DataMember]
        public EORestoreDestFileType DestFileType { get; set; }

        [DataMember]
        public string Prefix { get; set; }

        [DataMember]
        public string DestStoragePolicyId { get; set; }

        [DataMember]
        public Int32 PostFileFolderCount { get; set; }
        #endregion ==

        //#region == Mapping setting ==
        ///// <summary> 用户选择的Language Mapping setting Id </summary>
        //[DataMember]
        //public string LanguageMappingSettingId { get; set; }

        ///// <summary> 用户选择的User Mapping setting Id </summary>
        //[DataMember]
        //public string UserMappingSettingId { get; set; }

        ///// <summary>用户选择的Domain Mapping setting Id</summary>
        //[DataMember]
        //public string DomainMappingSettingId { get; set; }
        //#endregion ==

        ///// <summary> Restore user profile setting. </summary>
        //[DataMember]
        //public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool ExcludeGroupWithoutPermissions { get; set; }

        [DataMember]
        public EOGlobalRestoreOption GlobalRestoreOption { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public EORestoreSettingsForMisc SettingsForMisc { get; set; }

        [DataMember]
        public bool IsSearchTree { get; set; }

        [DataMember]
        public string BackupSrcAgentGroupId { get; set; }

        #region == plan settings ==
        [DataMember]
        public string BackupCycleId { get; set; }

        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        [DataMember]
        public string LogicalDeviceId { get; set; }
        #endregion == plan settings ==
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOGlobalRestoreOption
    {
        [DataMember]
        public EOContainerSetting ContainerSetting { set; get; }

        [DataMember]
        public EOContentSetting ContentSetting { set; get; }
    }

    /// <summary> 不需要存储Db里,为了做CEIP使用. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreSettingsForMisc
    {
        [DataMember]
        public bool IsConfigureSchedule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOContainerSetting
    {
        [EnumMember]
        None = 0,
        /// <summary> Check Restore Container </summary>
        [EnumMember]
        RestoreContainer = 1,
        /// <summary> Check Restore Container + Security </summary>
        [EnumMember]
        Security = 3,
        /// <summary> Check Restore Container + Property </summary>
        [EnumMember]
        Property = 5,
        /// <summary> Check Restore Container + Security + Property </summary>
        [EnumMember]
        SecurityAndProperty = 7,

        //RestoreSecurityOnly=16
        /// <summary> Check Only Restore Security + Merge </summary>
        [EnumMember]
        SecurityOnlyMerge = 48,
        /// <summary> Check Only Restore Security + Replace </summary>
        [EnumMember]
        SecurityOnlyOverWrite = 80,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOContentSetting
    {
        [EnumMember]
        None = 0,
        /// <summary>Check Restore Content </summary>
        [EnumMember]
        RestoreContent = 1,
        /// <summary> Check Restore Content + Security </summary>
        [EnumMember]
        Security = 3,

        //RestoreSecurityOnly=16
        /// <summary>Check Only Restore Security + Merge </summary>
        [EnumMember]
        SecurityOnlyMerge = 48,
        /// <summary>Check Only Restore Security + Replace </summary>
        [EnumMember]
        SecurityOnlyOverWrite = 80,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum EORestorePlanType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MailBox = 1,
        [EnumMember]
        Folder = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EORestoreType
    {
        [EnumMember]
        InPlace = 0,
        [EnumMember]
        OutOfPlace = 1,
        [EnumMember]
        ToStorage = 2
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum EORestoreVersionSetting
    //{
    //    [EnumMember]
    //    All = 0,
    //    [EnumMember]
    //    MajorAndMinor = 1,
    //    [EnumMember]
    //    MajorOnly = 2
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOConflictResolutionType
    {
        [EnumMember]
        None = -1,
        /// <summary>  container and content level conflict resolution共有设置 </summary>
        [EnumMember]
        Skip = 0,
        /// <summary>container and content level conflict resolution共有设置 </summary>
        [EnumMember]
        Overwrite = 1,
        ///// <summary> 仅content level有 </summary>
        //[EnumMember]
        //OverwriteByModifiedTime = 2,
        ///// <summary> 仅content level有 </summary>
        //[EnumMember]
        //AppendItemOrDocumentByReNamed = 3,
        ///// <summary> 仅container level有 </summary>
        //[EnumMember]
        //Replace = 4,
        ///// <summary> 仅content level有 </summary>
        //[EnumMember]
        //AppendANewVersion = 5,
        ///// <summary> 仅container level有 </summary>
        [EnumMember]
        Merge = 3,
        [EnumMember]
        Append = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EODependencyOption
    {
        [EnumMember]
        None = 0,
        /// <summary>
        /// Not restore item dependent columns and content types.
        /// </summary>
        [EnumMember]
        NotRestore = 1,
        /// <summary>
        /// Not migrate the columns and content types,and not migrate corresponding item.
        /// </summary>
        [EnumMember]
        SkipConfilctItem = 2,
        /// <summary>
        /// Overwrite the columns and content types.
        /// </summary>
        [EnumMember]
        Overwrite = 3,
        /// <summary>
        /// Append the columns and content types to destination.
        /// </summary>
        [EnumMember]
        Append = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EORestoreDestFileType
    {
        [EnumMember]
        PST = 0,
    }
}
