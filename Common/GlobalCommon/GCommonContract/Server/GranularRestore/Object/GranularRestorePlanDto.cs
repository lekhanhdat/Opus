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




namespace AvePoint.GCommon.Contract.Server.GranularRestore.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularRestorePlanDto : PlanDto
    {
        [DataMember]
        public RestoreType RestoreType { get; set; }

        [DataMember]
        public bool IncludeDetailedJobReport { get; set; }

        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        [DataMember]
        public bool IncludeCustomPropertyBags { get; set; }

        [DataMember]
        public bool IsReadDataViaCache { get; set; }

        /// <summary> if True, means Attach, else Merge。 </summary>
        [DataMember]
        public bool RestoreContentsToSub { get; set; }

        [DataMember]
        public RestoreVersionSetting RestoreVersionSetting { get; set; }

        [DataMember]
        public int VersionCount { get; set; }

        [DataMember]
        public bool IncludeProjectsData { get; set; }

        [DataMember]
        public bool IsIncludeSharedLinks { get; set; }

        #region ==  Conflict resolution setting ==
        [DataMember]
        public ConflictResolutionType ContainerConflictResolution { get; set; }

        [DataMember]
        public ConflictResolutionType ContentConflictResolution { get; set; }

        [DataMember]
        public ConflictResolutionType AppsConflictResolution { get; set; }

        [DataMember]
        public ItemDependencyOption ItemDependencyType { get; set; }

        [DataMember]
        public RestoreThreadType RestoreThreadType { get; set; }
        #endregion ==

        [DataMember]
        public bool IncludeRecycleBinData { get; set; }

        [DataMember]
        public bool OnlyOneJob { get; set; }

        /// <summary> 与SiteMasterIndex数据表的JobId相关联。 </summary>
        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public GranularRestorePlanType Type { get; set; }

        #region == 以下属性为export to file system 所使用==
        [DataMember]
        public string DestStoragePolicyId { get; set; }

        [DataMember]
        public List<string> EmailUsers { get; set; }

        [DataMember]
        public string ZipFilePassword { get; set; }
        #endregion ==

        #region == Mapping setting ==
        /// <summary> 用户选择的Language Mapping setting Id </summary>
        [DataMember]
        public string LanguageMappingSettingId { get; set; }

        /// <summary> 用户选择的User Mapping setting Id </summary>
        [DataMember]
        public string UserMappingSettingId { get; set; }

        /// <summary>用户选择的Domain Mapping setting Id</summary>
        [DataMember]
        public string DomainMappingSettingId { get; set; }
        #endregion ==

        /// <summary> Restore user profile setting. </summary>
        [DataMember]
        public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool ExcludeGroupWithoutPermissions { get; set; }

        [DataMember]
        public GlobalRestoreOption GlobalRestoreOption { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public RestoreSettingsForMisc SettingsForMisc { get; set; }

        [DataMember]
        public bool IsSearchTree { get; set; }

        /// <summary>Roll Backup job的catogory  cm/rp </summary>
        [DataMember]
        public int JobCategory { get; set; }

        #region  == PlanSettings ==
        [DataMember]
        public string BackupCycleId { get; set; }

        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string LogicalDeviceId { get; set; }

        /// <summary> if True, means Skip Special Lists Under PersonalSite。 </summary>
        [DataMember]
        public bool SkipHiddenList { get; set; }

        #endregion == PlanSettings ==

        #region == Tree ==
        [DataMember]
        public string tempSrcTreeMessage { get; set; }
        private bool srcTreeSerilized = false;
        private bool srcTreeChanged = false;
        private List<ContentDto> treeContents;
        public List<ContentDto> TreeContents
        {
            get
            {
                if (!srcTreeSerilized || srcTreeChanged)
                {
                    treeContents = (List<ContentDto>)Deserialize(tempSrcTreeMessage, typeof(List<ContentDto>));
                    srcTreeSerilized = true;
                    srcTreeChanged = false;
                }
                return treeContents;
            }
            set
            {
                tempSrcTreeMessage = Serialize(value, typeof(List<ContentDto>));
                srcTreeChanged = true;
            }
        }

        private string Serialize(System.Object obj, Type type)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(obj.GetType());
                dataContractSerializer.WriteObject(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }

        }

        private System.Object Deserialize(string jsonstring, Type type)
        {
            if (string.IsNullOrEmpty(jsonstring))
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] content = Convert.FromBase64String(jsonstring);
                ms.Write(content, 0, content.Length);
                ms.Position = 0;
                var dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(ms);
            }
        }
        #endregion == Tree ==
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GlobalRestoreOption
    {
        [DataMember]
        public ContainerSetting ContainerSetting { set; get; }

        [DataMember]
        public ContentSetting ContentSetting { set; get; }
    }

    /// <summary> 不需要存储Db里,为了做CEIP使用. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreSettingsForMisc
    {
        [DataMember]
        public bool IsConfigureSchedule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContainerSetting
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
    public enum ContentSetting
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
    public enum GranularRestorePlanType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ItemLevel = 1,
        [EnumMember]
        SiteLevel = 2,
        [EnumMember]
        SiteCollectionLevel = 4,
        [EnumMember]
        AdvancedSearch = 32,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreType
    {
        [EnumMember]
        InPlace = 0,
        [EnumMember]
        OutOfPlace = 1,
        [EnumMember]
        ToFileSystem = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreVersionSetting
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        MajorAndMinor = 1,
        [EnumMember]
        MajorOnly = 2,
        [EnumMember]
        None = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictResolutionType
    {
        [EnumMember]
        None = -1,
        /// <summary>  container and content level conflict resolution共有设置 </summary>
        [EnumMember]
        Skip = 0,
        /// <summary>container and content level conflict resolution共有设置 </summary>
        [EnumMember]
        Overwrite = 1,
        /// <summary> 仅content level有 </summary>
        [EnumMember]
        OverwriteByModifiedTime = 2,
        /// <summary> 仅content level有 </summary>
        [EnumMember]
        AppendItemOrDocumentByReNamed = 3,
        /// <summary> 仅container level有 </summary>
        [EnumMember]
        Replace = 4,
        /// <summary> 仅content level有 </summary>
        [EnumMember]
        AppendANewVersion = 5,
        /// <summary> 仅container level有 </summary>
        [EnumMember]
        Merge = 6
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ItemDependencyOption
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
        Append = 4,
        /// <summary>
        /// Ignore difference, and move the Items
        /// </summary>
        [EnumMember]
        IgnoreDifference = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreThreadType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Single = 1,

        [EnumMember]
        Multiple = 2
    }

}
