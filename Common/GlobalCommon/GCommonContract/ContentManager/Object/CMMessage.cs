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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ContentTypeMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.TemplateMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;

namespace AvePoint.GCommon.Contract.ContentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CMMessage : AveMessage
    {
        [DataMember]
        public Content Content { get; set; }

        //[DataMember]
        //public BposInfo BposInfo { get; set; }

        [DataMember]
        public OperationType Operation { get; set; }

        [DataMember]
        public ReturnResult ReturnValue { get; set; }

        //[DataMember]
        //public ApiObjectModelType ObjectModelType { get; set; }

        [DataMember]
        public List<SPTreeNodeDto> DeleteTreeNodes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Content
    {
        [DataMember]
        public SPTreeNodeDto SrcTreeNode { get; set; }
        [DataMember]
        public SPTreeNodeDto DestTreeNode { get; set; }
        [DataMember]
        public DMOption DMOption { get; set; }
        [DataMember]
        public LanguageMappingDto LanguageMapping { get; set; }
        [DataMember]
        public UserAndDomainMapping UserMapping { get; set; }
        [DataMember]
        public FilterPolicyInfo FilterPolicy { get; set; }

        [DataMember]
        public TemplateMappingContract TemplateMapping { get; set; }
        [DataMember]
        public ColumnMappingDataContract ColumnMappingData { get; set; }
        [DataMember]
        public ContentTypeMappingDataContract ContentTypeMappingData { get; set; }

        //ArchiveRule
        [DataMember]
        public ServiceDto SourceAgent { get; set; }
        [DataMember]
        public ServiceDto DestinationAgent { get; set; }
        [DataMember]
        public Int32 DestinationCount { get; set; }
        [DataMember]
        public Int32 Levle { get; set; }
        //KeepId
        [DataMember]
        public Boolean IsKeepId { get; set; }
        [DataMember]
        public Int32 Option { get; set; }
        [DataMember]
        public String JobId { get; set; }
        [DataMember]
        public Int32 SyncDeletion { get; set; }
        [DataMember]
        public String PlanName { get; set; }
        [DataMember]
        public String PlanId { get; set; }
        [DataMember]
        public Int32 Quota { get; set; }
        [DataMember]
        public Int32 UsingUnit { get; set; }
        [DataMember]
        public Boolean IsConfiguration { get; set; }
        [DataMember]
        public MigrateTheItem MigrateTheItem { get; set; }
        [DataMember]
        public MigrateTheItemConflictResolution MigrateTheItemConflictResolution { get; set; }
        [DataMember]
        public Boolean IsContent { get; set; }
        [DataMember]
        public Boolean IsBackupMetadataService { get; set; }
        [DataMember]
        public BackupMetadataServiceSetting BackupMetadataServiceSetting { get; set; }
        [DataMember]
        public Boolean IsIncludeCustomPropertyBags { get; set; }
        [DataMember]
        public Boolean IsSecurity { get; set; }
        [DataMember]
        public Boolean IsIncludeVersions { get; set; }

        // 此属性已在CM中废弃
        //[DataMember]
        //public Boolean IsWorkflow { get; set; }

        [DataMember]
        public Boolean IsGenerateMetadataFile { get; set; }
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
        /// <summary>
        /// key is site url, value is super user configuration
        /// </summary>
        [DataMember]
        public Dictionary<string, SuperUserConfigurationDto> SuperUserConfigurationSiteUrlMappings { get; set; }
        [DataMember]
        public Boolean IsKeepModifiedByAndModifiedTime { get; set; }
        [DataMember]
        public Boolean EnableBackupAttachments { get; set; }
        [DataMember]
        public Boolean IsMetaDataOnly { get; set; }
        [DataMember]
        public Boolean IsMigrateArchiverStubs { get; set; }
        [DataMember]
        public Int32 DataConfiguration { get; set; }
        [DataMember]
        public Boolean IsPromote { get; set; }
        [DataMember]
        public Boolean IsPreserveNullColumnValues { get; set; }
        [DataMember]
        public Boolean IsEncryption { get; set; }
        [DataMember]
        [Obsolete("在 DAO 已经不会再使用此 setting")]
        public Boolean IsCompression { get; set; }
        /// <summary>
        /// 只有在IsCompression为true时，才需要设置DataEncryptionInfoWrapper
        /// </summary>
        [DataMember]
        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }
        [DataMember]
        public Int32 Compression { get; set; }
        [DataMember]
        public EncryptionType Encryption { get; set; }
        [DataMember]
        public Boolean IsSendEmail { get; set; }
        [DataMember]
        public String SrcLanguage { get; set; }
        [DataMember]
        public String DestLanguage { get; set; }
        [DataMember]
        public Boolean IsIncludeUserProfiles { get; set; }
        [DataMember]
        public Int32 MoveAction { get; set; }
        [DataMember]
        public Int32 DeleteType { get; set; }
        [DataMember]
        public String LoginName { get; set; }
        [DataMember]
        public Boolean TestRun { get; set; }
        [DataMember]
        public String AlertReceiver { get; set; }
        [DataMember]
        public String PlaceHolderAccount { get; set; }
        [DataMember]
        public String Location { get; set; }
        [DataMember]
        public String Domain { get; set; }
        [DataMember]
        public String Username { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public JobOperationType JobOperationType { set; get; }
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
        [DataMember]
        public SODataType SODataType { get; set; }
        [DataMember]
        public bool IsDeleteCheckedFiles { get; set; }
        [DataMember]
        public string MediaStorageXri { get; set; }
        [DataMember]
        public BPOSType BPOSType { get; set; }

        /// <summary>
        /// 6.1新加属性
        /// </summary>
        [DataMember]
        public CMMappingSplitType PromoteSubSite { get; set; }
        [DataMember]
        public bool ExcludeWithoutPermission { get; set; }
        [DataMember]
        public ActionType Action { get; set; }

        [DataMember]
        public SecuritySettings SecuritySettings { get; set; }
        [DataMember]
        public ConfigurationSettings ConfigurationSettings { get; set; }

        // saas-12520
        [DataMember]
        public bool SkipHiddenList { get; set; }

        [DataMember]
        public bool IsIncludeShareLink { get; set; }

        [DataMember]
        public bool IsTransforWebPart { get; set; }

        [DataMember]
        public bool IsIncludeNinexForm { get; set; }

        [DataMember]
        public CopyMethod CopyMethod { get; set; }

        [DataMember]
        public bool IsUpdateSpecificLinks { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentOption
    {
        [DataMember]
        public Int32 ReplicateArchivedDataType { get; set; }
        [DataMember]
        public Boolean IsReplicateFromArchiver { get; set; }
        [DataMember]
        public Boolean IsReplicateFromConnector { get; set; }
        [DataMember]
        public Boolean IsReplicateFromExtender { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMOption
    {
        [DataMember]
        public ContentOption ContentOption { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationType
    {
        [EnumMember]
        Export,
        [EnumMember]
        Import,
        [EnumMember]
        Copy,
        [EnumMember]
        Move,
        [EnumMember]
        DeleteContent,
        [EnumMember]
        OnlinePreviewInit,
        [EnumMember]
        OnlinePreviewBrowser,
        [EnumMember]
        OfflinePreviewInit,
        [EnumMember]
        OfflinePreviewBrowser
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CMMappingSplitType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Normal = 1,
        [EnumMember]
        PromoteSubSite = 2
    }
}
