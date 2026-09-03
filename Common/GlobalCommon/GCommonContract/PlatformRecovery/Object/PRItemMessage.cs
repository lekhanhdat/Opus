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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("PRItemMessage")]
    public class PRItemMessage : AveMessage
    {
        [DataMember]
        [XmlAttribute("PlanId")]
        public string PlanId { get; set; }

        [DataMember]
        [XmlAttribute("JobId")]
        public string JobId { get; set; }

        [DataMember]
        [XmlAttribute("Media")]
        public string Media { get; set; }

        [DataMember]
        [XmlAttribute("Agent")]
        public string Agent { get; set; }

        [DataMember]
        [XmlAttribute("JobLevel")]
        public string JobLevel { get; set; }

        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlElement("RestoreInfo")]
        public PRItemRestoreConfig RestoreInfo { get; set; }

        [DataMember]
        [XmlElement("SourceInfo")]
        public SourceDetails SourceInfo { get; set; } //Control.exe赋值

        [DataMember]
        [XmlElement("NintexDBInfo")]
        public SourceDetails NintexDBInfo { get; set; } //Control.exe赋值

        /// <summary>
        /// GUI 进行赋值,目的端tree
        /// </summary>
        [DataMember]
        [XmlElement("DestTree")]
        public SPTreeNodeDto DestTree { get; set; }

        /// <summary>
        /// GUI 进行赋值,源端tree
        /// </summary>
        [DataMember]
        [XmlElement("SourceTree")]
        public SPTreeNodeDto SourceTree { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("PRItemRestoreConfig")]
    public class PRItemRestoreConfig
    {
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("WorkflowDefinition")]
        public bool WorkflowDefinition { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("IncludeRecycleBinData")]
        public bool IncludeRecycleBinData { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("DetailedReport")]
        public bool DetailedReport { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("RestoreOption")]
        public PRItemRestoreOption RestoreOption { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("RestoreType")]
        public PRItemRestoreCopyTable RestoreType { get; set; }

        /// <summary>item restore security property设置</summary>
        [DataMember]
        [XmlElement("GlobalRestoreOption")]
        public GlobalRestoreOption GlobalRestoreOption { get; set; }

        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("OutOfPlace")]
        public bool OutOfPlace { get; set; }
        /// <summary>
        /// GUI 进行赋值(sub site 的url)暂时不填
        /// </summary>
        [DataMember]
        [XmlAttribute("PromoteWebUrl")]
        public string PromoteWebUrl { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlElement("ListLanguageMappings")]
        public List<LanguageMapping> ListLanguageMappings { get; set; }
        [DataMember]
        [XmlElement("ColumnLanguageMappings")]
        public List<LanguageMapping> ColumnLanguageMappings { get; set; }
        [DataMember]
        [XmlElement("LanguageMappingDto")]
        public LanguageMappingDto LanguageMappingDto { get; set; }
        /// <summary>
        /// GUI 进行赋值(目的端最低的级别)
        /// </summary>
        [DataMember]
        [XmlAttribute("DestExpandedLevel")]
        public NodeLevel DestExpandedLevel { get; set; }
        /// <summary>
        /// GUI 进行赋值(源端最低的级别)
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceExpandedLevel")]
        public NodeLevel SourceExpandedLevel { get; set; }

        /// <summary>
        /// 对应5的config界面(暂时不赋值)
        /// </summary>
        [DataMember]
        [XmlAttribute("DestLanguage")]
        public uint DestLanguage { get; set; }
        [DataMember]
        [XmlAttribute("DestContentDBId")]
        public Guid DestContentDBId { get; set; }
        [DataMember]
        [XmlAttribute("OwnerLogin")]
        public string OwnerLogin { get; set; }
        [DataMember]
        [XmlAttribute("WorkflowState")]
        public PRWorkflow WorkflowState { get; set; }
        [DataMember]
        [XmlAttribute("SOInfos")]
        public SOSourceInfos SOInfos { get; set; }


        // Exclude User /Group Without Permission
        [DataMember]
        [XmlAttribute("ExcludeGroupWithoutPermissions")]
        public bool ExcludeGroupWithoutPermissions { get; set; }
        // version setting
        [DataMember]
        [XmlAttribute("RestoreVersionSetting")]
        public PRRestoreVersionSetting RestoreVersionSetting { get; set; }
        [DataMember]
        [XmlAttribute("VersionCount")]
        public int VersionCount { get; set; }

        [DataMember]
        [XmlAttribute("UserMapping")]
        public UserAndDomainMapping UserMapping { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("LanguageMapping")]
    public class LanguageMapping
    {
        [DataMember]
        [XmlArray("LanguageMap")]
        [XmlArrayItem("LanguageMappingPair")]
        public List<LanguagePair> LanguageMap { get; set; }
        [DataMember]
        [XmlAttribute("Type")]
        public LanguageMappingType Type { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("SOSourceInfos")]
    public class SOSourceInfos
    {
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceFarmName")]
        public string SourceFarmName { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceFarmID")]
        public string SourceFarmID { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceWebAppUrl")]
        public string SourceWebAppUrl { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceWebAppID")]
        public string SourceWebAppID { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceDBName")]
        public string SourceDBName { get; set; }
        /// <summary>
        /// GUI 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("SourceDBID")]
        public string SourceDBID { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("LanguageMappingPair")]
    public class LanguagePair
    {
        [DataMember]
        [XmlAttribute("Language")]
        public uint LanguageId { get; set; }
        [DataMember]
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LanguageMappingType
    {
        [EnumMember]
        Undefined,
        [EnumMember]
        List,
        [EnumMember]
        Column
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum WorkflowState
    //{
    //    [EnumMember]
    //    NoRestore = 0,
    //    [EnumMember]
    //    RestoreDefination = 1,
    //    [EnumMember]
    //    RestoreDefinationAndState = 2,
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("SourceDetails")]
    public class SourceDetails
    {
        /// <summary>
        /// service 进行赋值
        /// </summary>
        [DataMember]
        [XmlAttribute("Level")]
        public PRBackupLevel Level { get; set; }

        [DataMember]
        [XmlAttribute("DBConnectionString")]
        public string DBConnectionString { get; set; }

        [DataMember]
        [XmlAttribute("SourceWebAppName")]
        public string SourceWebAppName { get; set; }

        [DataMember]
        [XmlAttribute("SourceWebAppUrl")]
        public string SourceWebAppUrl { get; set; }

        [DataMember]
        [XmlAttribute("NintexDBConnectionString")]
        public string NintexDBConnectionString { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRItemRestoreOption
    {
        [EnumMember]
        Undefined,
        [EnumMember]
        OverWrite,
        [EnumMember]
        NotOverWrite,
        [EnumMember]
        Append,
        [EnumMember]
        Replace,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRItemRestoreCopyTable
    {
        [EnumMember]
        CopyTable,
        [EnumMember]
        NotCopyTable,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("PRWorkflow")]
    public class PRWorkflow
    {
        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }

        [DataMember]
        public bool IncludeWorkflowInstance { get; set; }

        [DataMember]
        public WorkflowConflictResolutionType DefinitionConflictResolution { get; set; }

        [DataMember]
        public WorkflowConflictResolutionType InstanceConflictResolution { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("GlobalRestoreOption")]
    public class GlobalRestoreOption
    {
        [DataMember]
        public ContainerSetting ContainerSetting { set; get; }

        [DataMember]
        public ContentSetting ContentSetting { set; get; }
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WorkflowConflictResolutionType
    {
        [EnumMember]
        None,
        /// <summary> Include workflow definition和Include workflow instance共有设置 </summary>
        [EnumMember]
        NotOverwrite,
        /// <summary> 仅Include workflow instance有 </summary>
        [EnumMember]
        Overwrite,
        /// <summary> 仅Include workflow definition有 </summary>
        [EnumMember]
        Append,
        /// <summary> only 'Include workflow definition' hava this setting, 
        /// represent skip the definition if there is any running instance. </summary>
        [EnumMember]
        OverwriteOrSkipDefinition,
        /// <summary> 仅Include workflow definition有 </summary>
        [EnumMember]
        OverwriteDefinitionByForce
    }
}