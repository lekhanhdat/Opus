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






using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.ContentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CMPlan : PlanDto
    {
        [DataMember]
        public FarmDto DestFarm { get; set; }


        [DataMember]
        //public string tmpSrcTree;
        //public SPTreeNodeDto tmpSrcTreeObj;
        //public bool srcTreeSerilized = false;
        //public bool srcTreeChanged = false;
        public SPTreeNodeDto SrcTree { get; set; }
        //{
        //    get
        //    {
        //        if (!srcTreeSerilized || srcTreeChanged)
        //        {
        //            tmpSrcTreeObj = (SPTreeNodeDto)Deserialize(tmpSrcTree, typeof(SPTreeNodeDto));
        //            srcTreeSerilized = true;
        //            srcTreeChanged = false;
        //        }
        //        return tmpSrcTreeObj;
        //    }
        //    set
        //    {
        //        tmpSrcTree = Serialize(value, typeof(SPTreeNodeDto));
        //        srcTreeChanged = true;
        //    }
        //}

        [DataMember]
        //public string tmpDestTree;
        //public SPTreeNodeDto tmpDestTreeObj;
        //public bool destTreeSerilized = false;
        //public bool destTreeChanged = false;
        public SPTreeNodeDto DestTree { get; set; }
        //{
        //    get
        //    {
        //        if (!destTreeSerilized || destTreeChanged)
        //        {
        //            tmpDestTreeObj = (SPTreeNodeDto)Deserialize(tmpDestTree, typeof(SPTreeNodeDto));
        //            destTreeSerilized = true;
        //            destTreeChanged = false;
        //        }
        //        return tmpDestTreeObj;
        //    }
        //    set
        //    {
        //        tmpDestTree = Serialize(value, typeof(SPTreeNodeDto));
        //        destTreeChanged = true;
        //    }
        //}


        //[DataMember]
        //public string tSrcTree { get; set; }

        //[DataMember]
        //public string tDestTree { get; set; }

        /// <summary>
        /// import src file tree
        /// </summary>
        [DataMember]
        public FSTreeNodeDto DiskTree { get; set; }

        [DataMember]
        public CMSettings PlanDetail { get; set; }

        [DataMember]
        public Boolean TestRun { get; set; }

        [DataMember]
        public List<PlanGroupDtoForOtherModule> PlanGroups { get; set; }

        //[DataMember]
        //public ProfileDto EmailProfile { get; set; }

        /// <summary>
        /// 存储cm各种扩展setting，暂时只存储了和backup相关的信息
        /// </summary>
        [DataMember]
        public ExtendSetting ExtendSetting { set; get; }

        [DataMember]
        public RunningPlanDetailInfo RunningPlanDetailInfo { set; get; }





    }

    [DataContract]
    public class RunningPlanDetailInfo
    {
        [DataMember]
        public bool isRunning { set; get; }

        [DataMember]
        public string jobId { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CMSettings
    {
        #region setttings

        [DataMember]
        public OperationType Operation { get; set; }
        [DataMember]
        public Boolean IsConfiguration { get; set; }
        [DataMember]
        public MigrateTheItem MigrateTheItem { get; set; }
        [DataMember]
        public MigrateTheItemConflictResolution MigrateTheItemConflictResolution { get; set; }
        [DataMember]
        public Boolean IsSecurity { get; set; }
        [DataMember]
        public Boolean IsContent { get; set; }
        [DataMember]
        public Boolean IsBackupMetadataService { get; set; }
        [DataMember]
        public BackupMetadataServiceSetting BackupMetadataServiceSetting { get; set; }
        [DataMember]
        public Boolean IsIncludeCustomPropertyBags { get; set; }
        // 此属性已在CM中废弃
        //[DataMember]
        //public Boolean IsWorkflow { get; set; }
        [DataMember]
        public Boolean IsGenerateMetadataFile { get; set; }
        [DataMember]
        public Boolean IncludeVersions { get; set; }
        [DataMember]
        public Boolean IsIncludeWorkflowDefiniton { get; set; }
        [DataMember]
        [Obsolete("在 DAO 已经不会再使用此 setting")]
        public Boolean IsIncludeWorkflowInstance { get; set; }
        [DataMember]
        public Boolean IsIncludeListAttachments { get; set; }
        [DataMember]
        public Boolean IsIncludeStubs { get; set; }
        [DataMember]
        public Boolean IsIncludeListView { get; set; }
        [DataMember]
        public Boolean IsDisableInformationRightsManagement { get; set; }
        [DataMember]
        public Boolean EnableSuperUserDecryptsFiles { get; set; }
        [DataMember]
        public Boolean IsPreserveNullColumnValues { get; set; }
        [DataMember]
        [Obsolete("在 DAO 已经不会再使用此 setting")]
        public Boolean IsCompression { get; set; }
        [DataMember]
        public Boolean IsEncryption { get; set; }
        [DataMember]
        public Int32 Compression { get; set; }
        [DataMember]
        public EncryptionType Encryption { get; set; }
        [DataMember]
        public ActionType Action { get; set; }
        [DataMember]
        public Boolean IsBackupDest { get; set; }
        [DataMember]
        public Boolean IsBackupSource { get; set; }
        [DataMember]
        public string AlertReceiver { get; set; }
        [DataMember]
        public Boolean IsIncludeUserProfile { get; set; }
        [DataMember]
        public Boolean IsKeepUserMetaData { get; set; }
        [DataMember]
        public Boolean IsKeepModifiedByAndModifiedTime { get; set; }
        [DataMember]
        public SecuritySettings SecuritySettings { get; set; }
        [DataMember]
        public ConfigurationSettings ConfigurationSettings { get; set; }

        /// <summary>
        /// 此属性以后会不再使用.
        /// </summary>
        [DataMember]
        public ConflictSolutionType ConflictSolution { get; set; }

        [DataMember]
        public Boolean IsRecursion { get; set; }

        [DataMember]
        public ConflictSolutionType ContainerConflictSolution { get; set; }

        [DataMember]
        public Boolean IsOverWriteByLastModifyTime { get; set; }

        [DataMember]
        public ConflictSolutionType ContentConflictSolution { get; set; }

        [DataMember]
        public ConflictSolutionType APPsConflictSolution { get; set; }

        /// <summary>
        /// 6.1新加属性，对应页面keep look and feel 
        /// </summary>
        [DataMember]
        public Boolean IsPromoteSubSite { get; set; }

        /// <summary>
        /// 6.1新加属性
        /// </summary>
        [DataMember]
        public bool ExcludeWithoutPermission { get; set; }

        [DataMember]
        public BPOSType BPOS { get; set; }
        #endregion

        #region profiles
        [DataMember]
        [Obsolete("在 DAO 已经不会再使用此 setting")]
        public NameAndIdDto SecurityProfile { get; set; }
        [DataMember]
        public NameAndIdDto FilterPolicyDto { get; set; }
        [DataMember]
        public NameAndIdDto UserMappingDto { get; set; }
        [DataMember]
        public NameAndIdDto languageMappingDto { set; get; }
        [DataMember]
        [Obsolete("在 DAO 已经不会再使用此 setting")]
        public NameAndIdDto DomainMappingDto { get; set; }
        [DataMember]
        public NameAndIdDto StoragePolicyDto { get; set; }
        [DataMember]
        public NameAndIdDto ExportLocationDto { get; set; }


        [DataMember]
        public NameAndIdDto EmailProfile { get; set; }


        [DataMember]
        public NameAndIdDto TemplateMapping { get; set; }

        [DataMember]
        public NameAndIdDto ColumnMapping { get; set; }

        [DataMember]
        public Boolean ISUseMetadataFile { get; set; }

        [DataMember]
        public NameAndIdDto ContentTypeMapping { get; set; }

        #endregion

        [DataMember]
        public string Location { set; get; }
        [DataMember]
        public DeleteType DeleteType { set; get; }
        [DataMember]
        public LocationInfoDto LocationInfo { set; get; }
        [DataMember]
        public GradeReuslt GradeReuslt { set; get; }

        // 这个属性存储default setting中的notification
        [DataMember]
        public NotificationDto Notification { get; set; }

        // 此属性已由IsIncludeWorkflowDefiniton、IsIncludeWorkflowInstance替代
        //[DataMember]
        //public WorkflowType WorkflowType { get; set; }

        [DataMember]
        public SODataType SODataType { get; set; }
        [DataMember]
        public Boolean IsDeleteCheckedFiles { get; set; }

        // SAAS-12520
        [DataMember]
        public bool SkipHiddenList { get; set; }

        [DataMember]
        public bool IsIncludeShareLink { get; set; }

        [DataMember]
        public bool IsTransferWebParts { get; set; }

        [DataMember]
        public bool IsIncludeNinexForm { get; set; }

        [DataMember]
        public CopyMethod CopyMethod { get; set; }
        [DataMember]
        public Boolean IsUpdateSpecificLinks { get; set; }
    }

    [DataContract]
    public enum MigrateTheItem
    {
        [EnumMember]
        MigrateTheItem = 0,
        [EnumMember]
        DoNotMigrateTheItem = 1,
    }

    [DataContract]
    public enum MigrateTheItemConflictResolution
    {
        [EnumMember]
        DoNotMigrateTheItems = 0,
        [EnumMember]
        OverwriteIdenticalItemsInDestination = 1,
        [EnumMember]
        Append = 2,
        [EnumMember]
        IgnoreDifferenceAndMoveItems = 3,
    }

    //[DataContract]
    //public enum WorkflowType
    //{
    //    [EnumMember]
    //    DoNotMigrate = 0,
    //    [EnumMember]
    //    MigrateDefination = 1,
    //    [EnumMember]
    //    MigrateDefinationAndState = 2
    //}

    [DataContract]
    public enum SODataType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        RealContent = 1,
        [EnumMember]
        StubOnly = 2,
        [EnumMember]
        RestoreTheExtender = 3,
        [EnumMember]
        DoNotRestoreExtender = 4
    }

    [DataContract]
    public enum GradeReuslt
    {
        [EnumMember]
        None,
        [EnumMember]
        Demote,
        [EnumMember]
        Promote,
        [EnumMember]
        Lateral
    }

    [DataContract]
    public class LocationInfoDto
    {
        [DataMember]
        public string Path { set; get; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string EncryptedPassword { get; set; }

        [DataMember]
        public string MediaStorageXri { get; set; }
    }

    [DataContract]
    public enum DeleteType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Manually = 1,
        [EnumMember]
        Auto = 2
    }

    [DataContract]
    public enum DeleteCheckOutFileType
    {
        [EnumMember]
        No = 0,
        [EnumMember]
        Yes = 1
    }

    [DataContract]
    public enum DeleteState
    {
        [EnumMember]
        NoDelete = 0,
        [EnumMember]
        Deleting = 1,
        [EnumMember]
        Deleted = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CMDefaultSettings
    {
        [DataMember]
        public CMSettings Move { get; set; }
        [DataMember]
        public CMSettings Copy { get; set; }
    }

    [DataContract]
    public enum ConflictSolutionType
    {
        [EnumMember]
        Skip,
        [EnumMember]
        NotOverwrite,
        [EnumMember]
        Append,
        [EnumMember]
        Overwrite,
        [EnumMember]
        Replace,
        [EnumMember]
        Merge,
        [EnumMember]
        OverwriteByLastModifiedTime
    }
    [DataContract]
    public enum ActionType
    {
        [EnumMember]
        Attach,
        [EnumMember]
        Merge
    }

    [DataContract]
    public enum CopyMethod
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        HighSpeed = 1,
    }

    [DataContract]
    public enum BackupMetadataServiceSetting
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        TermsOnly = 1,
        [EnumMember]
        TermSets = 2,
        [EnumMember]
        ManagedMetadataService = 3,
        [EnumMember]
        Unchecked = 4,
    }

    [DataContract]
    public enum BPOSType
    {
        [EnumMember]
        NoBPOS, //源端目的端都没选BPOS
        [EnumMember]
        SrcBPOS,  //SrcBPOS 表示源端选择BPOS agent，目的端选regular agent
        [EnumMember]
        DestBPOS, //DestBPOS 表示源端选择regular agent，目的端选BPOS agent
        [EnumMember]
        BothBPOS //BothBPOS 表示源端和目的端选择的都是BPOS agent

        //Export, Import需要支持BPOS 未定
    }

    [DataContract]
    public enum EncryptionType
    {
        [EnumMember]
        MediaService,
        [EnumMember]
        SharePointAgent
    }

    [DataContract]
    public class BackUpInfo
    {
        [DataMember]
        public string BackUpSrcJobId { set; get; }

        [DataMember]
        public string BackUpSrcPlanId { set; get; }

        [DataMember]
        public string BackUpDestJobId { get; set; }

        [DataMember]
        public string BackUpDestPlanId { get; set; }
    }

    [DataContract]
    public class ExtendSetting
    {
        [DataMember]
        public BackUpInfo BackUpInfo { set; get; }
    }


    [DataContract]
    public class BackUpLicenseInfo
    {
        [DataMember]
        public bool IsExist2007BackUpLicense { set; get; }

        [DataMember]
        public bool IsExist2010BackUpLicense { set; get; }
    }

    [DataContract]
    public enum PlanValidateResult
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NameExist = 1,
        [EnumMember]
        CheckScheduleFailed = 2
    }

    [DataContract]
    public class SecuritySettings
    {
        [DataMember]
        public PermissionSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        public PermissionSiteLevel SiteLevel { get; set; }

        [DataMember]
        public PermissionListLevel ListLevel { get; set; }

        [DataMember]
        public PermissionFolderLevel FolderLevel { get; set; }

        [DataMember]
        public PermissionItemLevel ItemLevel { get; set; }
    }

    [DataContract]
    public class ConfigurationSettings
    {
        [DataMember]
        public ConfigurationSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        public ConfigurationSiteLevel SiteLevel { get; set; }

        [DataMember]
        public ConfigurationListLevel ListLevel { get; set; }
    }

    [DataContract]
    public class PermissionSiteCollectionLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }
    }

    [DataContract]
    public class PermissionSiteLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool PermissionLevels { get; set; }

        [DataMember]
        public bool SitePermissions { get; set; }
    }

    [DataContract]
    public class PermissionListLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool ListPermission { get; set; }
    }

    [DataContract]
    public class PermissionFolderLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool FolderPermission { get; set; }
    }

    [DataContract]
    public class PermissionItemLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool ItemPermission { get; set; }
    }

    [DataContract]
    public class ConfigurationSiteCollectionLevel
    {
        [DataMember]
        public bool FeaturesAndProperties { get; set; }
    }

    [DataContract]
    public class ConfigurationSiteLevel
    {
        [DataMember]
        public bool FeaturesAndProperties { get; set; }

        [DataMember]
        public bool ColumnAndContentType { get; set; }

        [DataMember]
        public bool NavigationAndQuickLaunch { get; set; }

        [DataMember]
        public bool SiteTemplateAndListTemplate { get; set; }

        [DataMember]
        public bool Others { get; set; }
    }

    [DataContract]
    public class ConfigurationListLevel
    {
        [DataMember]
        public bool ListSettings { get; set; }

        [DataMember]
        public bool PublicViews { get; set; }
    }
}