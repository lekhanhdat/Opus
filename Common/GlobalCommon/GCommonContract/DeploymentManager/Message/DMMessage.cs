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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMMessage : AveMessage
    {
        [DataMember]
        public DMContent DMContent { get; set; }

        /// <summary>
        /// 用来区分import or export功能
        /// </summary>
        [DataMember]
        public DMPlanType DMPlanType { get; set; }

        [DataMember]
        public DPMJobType DesignJobType { get; set; }

        [DataMember]
        public ControlJobType ControlJobType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMContent
    {
        [DataMember]
        public ServiceDto SourceAgent { get; set; }
        [DataMember]
        public SPTreeNodeDto SrcTree { get; set; }
        [DataMember]
        public List<DMDestInfo> DMDestInfos { get; set; }
        [DataMember]
        public LanguageMappingDto LanguageMapping { get; set; }
        [DataMember]
        public UserAndDomainMapping UserMapping { get; set; }
        [DataMember]
        public FilterPolicyInfo FilterPolicy { get; set; }
        [DataMember]
        public string LoginName { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public int Level { get; set; }
        [DataMember]
        public bool DeployMultilanguage { get; set; }
        [DataMember]
        public int Security { get; set; }
        [DataMember]
        public int IsContent { get; set; }
        [DataMember]
        public double ExportVersion { get; set; }
        [DataMember]
        public string VersionDescription { get; set; }
        [DataMember]
        public string IsHidContentNode { get; set; }
        [DataMember]
        public bool RestoreToSite { get; set; }
        [DataMember]
        public bool RestoreToWeb { get; set; }
        [DataMember]
        public DMPlanType JobType { get; set; }
        [DataMember]
        public bool IsIncludeUserProfiles { get; set; }
        [DataMember]
        public bool IsTestRun { get; set; }
        [DataMember]
        public bool IsSendEmail { get; set; }
        /// <summary>
        /// 存储ContainerConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ContainerConflictResolutionOption { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public bool Recursion { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ContentConflictResolutionOption { get; set; }
        /// <summary>
        /// 存储MigrateConfiguration Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution MigrateTheItemConflictResolution { get; set; }
        /// <summary>
        /// 存储ContentType And SiteColumn ConflictResolutionOption Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ContentTypeAndSiteColumnOption { get; set; }
        [DataMember]
        public string JobStartTime { get; set; }
        /// <summary>
        /// Work Flow Definition
        /// </summary>
        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }
        /// <summary>
        /// Deploy To Relative Lists
        /// </summary>
        [DataMember]
        public bool DeployToRelativeLists { get; set; }
        //Extender/Connector Data
        [DataMember]
        public bool IsMigrateData { get; set; }
        [DataMember]
        public bool IsRealContent { get; set; }
        [DataMember]
        public bool IsStubOnly { get; set; }

        [DataMember]
        public MigrateTheItem MigrateTheItem { get; set; }
        [DataMember]
        public int Compress { get; set; }
        [DataMember]
        public bool IsKeepId { get; set; }
        [DataMember]
        public int DataConfiguration { get; set; }
        [DataMember]
        public bool setIsByteLevelDifferencing { get; set; }
        [DataMember]
        public bool IsMetaDataOnly { get; set; }

        [DataMember]
        public int Quota { get; set; }
        [DataMember]
        public int QuotaUnit { get; set; }
        [DataMember]
        public string Replication { get; set; }
        [DataMember]
        public int SyncDeletion { get; set; }
        [DataMember]
        public int UsingUnit { get; set; }
        /// <summary>
        /// 是否keep null值到目的端
        /// </summary>
        [DataMember]
        public Boolean IsPreserveNullColumnValues { get; set; }
        [DataMember]
        public BatchProcessingType BatchProcessingType { get; set; }
        /// <summary>
        /// 表示使用哪种情况的mapping split type。
        /// </summary>
        [DataMember]
        public DMMappingSplitType DMMappingSplitType { get; set; }

        [DataMember]
        public ExportLocationDto ExportLocationDto { get; set; }

        [DataMember]
        public string NetDomain { get; set; }

        [DataMember]
        public string NetUserName { get; set; }

        [DataMember]
        public FBAInfo FBAInfo { get; set; }

        [DataMember]
        public bool IncludeApp { set; get; }

        [DataMember]
        public DPMConflictResolution AppConflictResolutionOption { set; get; }

        [DataMember]
        public Boolean IsBackupMetadataService { get; set; }

        [DataMember]
        public BackupMetadataServiceSetting BackupMetadataServiceSetting { get; set; }

        [DataMember]
        public bool SkipHiddenList { get; set; }

        [DataMember]
        public bool IsShareLink { get; set; }

        [DataMember]
        public bool IncludeFormPageWebPart { get; set; }

        [DataMember]
        public bool OverWriteRegionalSetting { get; set; }

        [DataMember]
        public bool IsMultiThreadRestore { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMDestInfo
    {
        [DataMember]
        public ServiceDto DestAgent { get; set; }
        [DataMember]
        public SPTreeNodeDto DestTree { get; set; }
        [DataMember]
        public SPTreeNodeDto SpecialBackupDestTree { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ControlJobType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Stop = 1,
        [EnumMember]
        Pause = 2,
        [EnumMember]
        Resume = 3
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DMMappingSplitType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Normal = 1,
        [EnumMember]
        Multiple = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DPMJobType
    {
        [EnumMember]
        Deploy = 0,
        [EnumMember]
        Backup = 1,
        [EnumMember]
        Rollback = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMConfig
    {
        [DataMember]
        public DMMessage dmMessage { get; set; }
        [DataMember]
        public SCDMMessage scMessage { get; set; }
        /*[DataMember]
        public string SrcFarmId { get; set; }
        [DataMember]
        public string DestFarmId { get; set; }*/
        [DataMember]
        public string SrcObjectId { get; set; }
        [DataMember]
        public string DestObjectId { get; set; }
        [DataMember]
        public PlanOrderInfo PlanInfo { get; set; }
        [DataMember]
        public string StoragePolicyID { get; set; }
        [DataMember]
        public string BackUpJobID { get; set; }
        [DataMember]
        public string DMVJobId { get; set; }
    }
}
