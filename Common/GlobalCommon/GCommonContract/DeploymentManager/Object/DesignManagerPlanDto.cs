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




using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DesignManagerPlanDto : AbstractDMPlanDto
    {
        private DesignManagerPlanDto designManagerPlanDto;

        public DesignManagerPlanDto(DesignManagerPlanDto designManagerPlanDto)
        {
            // TODO: Complete member initialization
            this.designManagerPlanDto = designManagerPlanDto;
        }

        public DesignManagerPlanDto()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// 存储DM界面的选项
        /// </summary>
        [DataMember]
        public DesignManagerOptionForGui DMOption { get; set; }

        /// <summary>
        /// 存储JobId
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        /// 存储Schedule集合
        /// </summary>
        [DataMember]
        public List<ScheduleDto> ScheduleDtos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DesignManagerOption
    {
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("selectedTreeNodeLevel")]
        public NodeLevel SelectedTreeNodeLevel { get; set; }

        /// <summary>
        /// 存储ContainerConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("containerConflictResolutionOption")]
        public DPMConflictResolution ContainerConflictResolutionOption { get; set; }

        private bool _recursion = true;
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("recursion")]
        public bool Recursion
        {
            get
            {
                return this._recursion;
            }
            set
            {
                this._recursion = value;
            }
        }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("contentConflictResolutionOption")]
        public DPMConflictResolution ContentConflictResolutionOption { get; set; }

        /// <summary>
        /// 存储MigrateConfigurationOption Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("migrateConfigurationOption")]
        public DPMConflictResolution MigrateTheItemConflictResolution { get; set; }

        /// <summary>
        /// 存储ContentType And Column Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("contentAndColumnConfigurationOption")]
        public DPMConflictResolution ContentTypeAndSiteColumnOption { get; set; }

        /// <summary>
        /// 存储UserMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("userMappingId")]
        public string UserMappingId { get; set; }

        /// <summary>
        /// 存储DomainMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("domainMappingId")]
        public string DomainMappingId { get; set; }

        /// <summary>
        /// 存储UserMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("toSiteCollection")]
        public bool ToSiteCollection { get; set; }

        /// <summary>
        /// 存储UserMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("toSite")]
        public bool ToSite { get; set; }

        /// <summary>
        ///存储BachSetting中ToTopSite or ToSubSite的值
        /// </summary>
        [DataMember]
        [XmlAttribute("batchProcessingType")]
        public BatchProcessingType BatchProcessingType { get; set; }

        ///// <summary>
        ///// 对应Design Manager Setting界面的Include Stubs选项
        ///// </summary>
        //[DataMember]
        //[XmlAttribute("includeStubs")]
        //public bool IncludeStubs { get; set; }

        /// <summary>
        /// 对应Design Manager Setting界面的Include Workflow Definition选项
        /// </summary>
        [DataMember]
        [XmlAttribute("includeWorkflowDefinition")]
        public bool IncludeWorkflowDefinition { get; set; }

        /// <summary>
        /// 对应Design Manager Setting界面的Deploy the Content type to Relative Lists选项
        /// </summary>
        [DataMember]
        [XmlAttribute("deployToRelativeLists")]
        public bool DeployToRelativeLists { get; set; }

        //Extender/Connector Data
        [DataMember]
        [XmlAttribute("isMigrateData")]
        public bool IsMigrateData { get; set; }

        [DataMember]
        [XmlAttribute("isRealContent")]
        public bool IsRealContent { get; set; }

        [DataMember]
        [XmlAttribute("isStubOnly")]
        public bool IsStubOnly { get; set; }

        private bool _isPreserveNullColumnValues = true;
        [DataMember]
        [XmlAttribute("isPreserveNullColumnValues")]
        public bool IsPreserveNullColumnValues
        {
            get
            {
                return this._isPreserveNullColumnValues;
            }
            set
            {
                this._isPreserveNullColumnValues = value;
            }
        }
        /// <summary>
        /// 存储Export Location的id值
        /// </summary>
        [DataMember]
        [XmlAttribute("locationSetupId")]
        public string LocationSetupId { set; get; }

        /// <summary>
        /// 存储Filter Policy的Id值
        /// </summary>
        [DataMember]
        [XmlAttribute("filterId")]
        public string FilterId { set; get; }

        /// <summary>
        /// 存储Destination端Filter Policy的Id值
        /// </summary>
        [DataMember]
        [XmlAttribute("DestFilterId")]
        public string DestFilterId { set; get; }

        /// <summary>
        /// 存储Storage Policy的Id值
        /// </summary>
        [DataMember]
        [XmlAttribute("storagePolicyId")]
        public string StoragePolicyId { set; get; }

        /// <summary>
        /// 存储判断是否选择了backup undo功能
        /// </summary>
        [DataMember]
        [XmlAttribute("isBackUp")]
        public bool IsBackUp { set; get; }

        /// <summary>
        /// 存储是否选择Deploy Multilanguage选项
        /// </summary>
        [DataMember]
        [XmlAttribute("isDeployMultilanguage")]
        public bool IsDeployMultilanguage { set; get; }

        /// <summary>
        /// 存储是否选择Security选项
        /// </summary>
        [DataMember]
        [XmlAttribute("isSecurity")]
        public bool IsSecurity { set; get; }

        /// <summary>
        /// 存储Include User Profiles选项
        /// </summary>
        [DataMember]
        [XmlAttribute("isIncludeUserProfiles")]
        public bool IsIncludeUserProfiles { set; get; }

        /// <summary>
        /// 存储User Content选项
        /// </summary>
        [DataMember]
        [XmlAttribute("isUserContent")]
        public bool IsUserContent { set; get; }

        /// <summary>
        /// 存储isMigrate
        /// </summary>
        [DataMember]
        [XmlAttribute("migrateTheItem")]
        public MigrateTheItem MigrateTheItem { get; set; }

        [DataMember]
        [XmlAttribute("archivedDataType")]
        public int ArchivedDataType { set; get; }
        [DataMember]
        [XmlAttribute("fromArchive")]
        public bool FromArchive { set; get; }
        [DataMember]
        [XmlAttribute("fromExtender")]
        public bool FromExtender { set; get; }
        [DataMember]
        [XmlAttribute("fromConnector")]
        public bool FromConnector { set; get; }

        /// <summary>
        /// Save Version功能中，用户输入的version
        /// </summary>
        [DataMember]
        [XmlAttribute("version")]
        public double Version { set; get; }

        /// <summary>
        /// Save Version功能中，用户输入的Description
        /// </summary>
        [DataMember]
        [XmlAttribute("versionDescription")]
        public string VersionDescription { set; get; }
        [DataMember]
        [XmlAttribute("restoreToSite")]
        public bool RestoreToSite { set; get; }
        [DataMember]
        [XmlAttribute("restoreToWeb")]
        public bool RestoreToWeb { set; get; }

        /// <summary>
        /// 存储用户选择的Language Mapping的Id值
        /// </summary>
        [DataMember]
        [XmlAttribute("languageMappingId")]
        public string LanguageMappingId { set; get; }

        /// <summary>
        /// 存储Compare模式下，Deployment
        /// </summary>
        [DataMember]
        [XmlAttribute("compareDeployModule")]
        public int CompareDeployModule { set; get; }

        [DataMember]
        public BPOSType BPOS { get; set; }

        /// <summary>
        /// 存储DesignImport下拉菜单值
        /// </summary>
        [DataMember]
        [XmlAttribute("importType")]
        public ImportType ImportType { get; set; }
        /// <summary>
        /// 存储DM ExportLocation的Name
        /// </summary>
        [DataMember]
        public string ExportLocationDtoName { get; set; }

        /// <summary>
        /// 存储DM ExportLocationID
        /// </summary>
        [DataMember]
        public string ExportLocationDtoID { get; set; }

        /// <summary>
        /// 存储DM ExportLocation UserName
        /// </summary>
        [DataMember]
        public string NetDomain { get; set; }

        /// <summary>
        /// 存储DM ExportLocation UserName
        /// </summary>
        [DataMember]
        public string NetUserName { get; set; }

        [DataMember]
        public string NetPassWord { get; set; }

        [DataMember]
        [XmlAttribute("includeApp")]
        public bool IncludeApp { set; get; }

        [DataMember]
        [XmlAttribute("appConflictResolutionOption")]
        public DPMConflictResolution AppConflictResolutionOption { set; get; }

        [DataMember]
        [XmlAttribute("srcObjectId")]
        public string SrcObjectId { get; set; }

        [DataMember]
        [XmlAttribute("destObjectId")]
        public string DestObjectId { get; set; }


        [DataMember]
        public FBAInfo FBAInfo { get; set; }

        [DataMember]
        public bool SkipHiddenList { get; set; }

        [DataMember]
        public bool IsShareLink { get; set; }

        [DataMember]
        public bool IncludeFormPageWebPart { get; set; }


        [DataMember]
        public bool IsBackupMetadataService { get; set; }

        [DataMember]
        public BackupMetadataServiceSetting BackupMetadataServiceSetting { get; set; }

        [DataMember]
        public MappingSource MappingSource { get; set; }
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
    }

    [DataContract]
    public enum ModificationType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        No = 1,
        [EnumMember]
        Yes = 2,
    }

    [DataContract]
    public enum DeletionType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        No = 1,
        [EnumMember]
        Yes = 2,
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DesignManagerOptionForGui : DesignManagerOption
    {
    }

    [DataContract]
    public class FBAInfo
    {
        [DataMember]
        public bool IsFba { set; get; }

        [DataMember]
        public string DisplayName { set; get; }

        [DataMember]
        public string LoginName { set; get; }
    }

    [DataContract]
    public enum DMPlanType : int
    {
        /// <summary>
        /// 表示正常类型的plan
        /// </summary>
        [EnumMember]
        Deploy = 0,

        /// <summary>
        /// Save Version功能中的Export类型Plan
        /// </summary>
        [EnumMember]
        Export = 1,

        /// <summary>
        /// Save Version功能中的Import类型Plan
        /// </summary>
        [EnumMember]
        Import = 2,

        /// <summary>
        /// Compare类型的Plan
        /// </summary>
        [EnumMember]
        Compare = 3,

        [EnumMember]
        CmdLine = 4,

        [EnumMember]
        BackUp = 5
    }

    [DataContract]
    public enum DMDeployModule : int
    {
        [EnumMember]
        None = 0,

        /// <summary>
        /// Compare方式中，源端向目的端部署
        /// </summary>
        [EnumMember]
        DeployToDest = 1,

        /// <summary>
        /// Compare方式中，目的端向源端部署
        /// </summary>
        [EnumMember]
        DeployToSrc = 2,

        /// <summary>
        /// Compare方式中，双向部署
        /// </summary>
        [EnumMember]
        DeployToAll = 3
    }

    /*[DataContract]
    public enum
        DeploymentOptions : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Overwrite = 1,
        [EnumMember]
        NotOverwrite = 2,
        [EnumMember]
        Replace = 3,
        [EnumMember]
        OverWriteIfNewer = 4,
        [EnumMember]
        FullDeployment = 5,
        [EnumMember]
        IncrementalDeployment = 6
    }*/


    [DataContract]
    public enum DPMConflictResolution : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Overwrite = 1,
        [EnumMember]
        NotOverwrite = 2,
        [EnumMember]
        Replace = 3,
        [EnumMember]
        OverWriteIfNewer = 4,
        [EnumMember]
        Merge = 5,
        [EnumMember]
        Skipped = 6,
        [EnumMember]
        Upgrade = 7,
        [EnumMember]
        RetractAndRedeploy = 8,
        [EnumMember]
        OverwriteByLastModifiedTime = 9,
        [EnumMember]
        SkipAndDoNotMigrate = 10,
        [EnumMember]
        Append = 11,
        [EnumMember]
        IgnoreDifference = 12
    }

    [DataContract]
    public enum DeploymentOption : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        FullDeployment = 1,
        [EnumMember]
        IncrementalDeployment = 2,
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

        //Export, Import需要支持BPOS ？？？？
    }

    [DataContract]
    public enum BatchProcessingType : int
    {
        [EnumMember]
        None,
        [EnumMember]
        ToTopSite,
        [EnumMember]
        ToAllSubSite
    }

    [DataContract]
    public enum OperationType : int
    {
        [EnumMember]
        Export,
        [EnumMember]
        Import
    }

    [DataContract]
    public enum ImportType : int
    {
        [EnumMember]
        Undefined = -1,
        [EnumMember]
        Design = 0,
        [EnumMember]
        Solution = 1,
        [EnumMember]
        AssemblyCache = 2,
        [EnumMember]
        Feature = 3
    }

    [DataContract]
    public enum MappingSource
    {
        [EnumMember]
        DAO = 0,
        [EnumMember]
        GAO = 1,
    }
}
