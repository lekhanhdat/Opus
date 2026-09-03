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
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object.Compare;
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.Adonis.ReportCenter.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    [XmlRootAttribute("NodeExtension")]
    public class NodeExtensionDto : IExtensibleDataObject
    {
        public bool IsEnable { get; set; }

        /// <summary>
        /// Be used for Site Collection, Sub Site levels.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Be used for Site Collection, Sub Site levels.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("templateTitle")]
        public string TemplateTitle { get; set; }

        /// <summary>
        /// Be used for Web Application level.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Languages")]
        public Languages Languages { get; set; }

        /// <summary>
        /// Be used for Site Collection, Sub Site levels.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public uint LCID { get; set; }

        /// <summary>
        /// Be used for Site Collection level
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ContentDB")]
        public ContentDB ContentDB { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ContentTypeHub")]
        public ContentTypeHubList ContentTypeHub { get; set; }

        /// <summary>
        /// For SP13, use CompatibilityLevel to distinguish 2010Mode/2013Mode
        /// CompatibilityLevel=0 means no need use it(SP07&SP10), CompatibilityLevel = 14 means 2010Mode, CompatibilityLevel=15 means 2013Mode
        /// </summary>
        [DataMember]
        [XmlElement("CompatibilityLevel")]
        public CompatibilityLevelType CompatibilityLevel { get; set; }

        /// <summary>
        /// Be used for Web Application level
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<ContentDB> ContentDBList { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BposInfo BposInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> AvailableAgentIds { get; set; }

        /// <summary>
        /// Be used for SO extender scheduled
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string scheduleId { get; set; }

        /// <summary>
        /// Be used for SO extender scheduled
        /// The propertie is not in use.
        /// </summary>
        //[XmlIgnore]
        //[DataMember(EmitDefaultValue = false)]
        //public RuleCollection RuleCollection { get; set; }

        /// <summary>
        /// Be used for Preview tree source node.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SPTreeNodeDto SourceNode { get; set; }

        /// <summary>
        /// Be used for Granular Restore Preview.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<SPTreeNodeDto> SourceNodes { get; set; }

        /// <summary>
        /// Be used for Granular Restore Preview.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SPTreeNodeDto DestNode { get; set; }

        /// <summary>
        /// Be used for SO.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool RealTimeActive { get; set; }

        /// <summary>
        /// Be used for Auditor.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int AuditActions { get; set; }

        /// <summary>
        /// Be used for Auditor.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime AuditDataStartTime { get; set; }

        /// <summary>
        /// Be used for Auditor.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime AuditIISLogStartTime { get; set; }

        /// <summary>
        /// Be used for Report Center.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime PageTrafficStartTime { get; set; }

        /// <summary>
        /// Be used for Auditor.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime LastIndexUpdateTime { get; set; }

        /// <summary>
        /// Be used for Auditor report tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool HaveAuditData { get; set; }

        /// <summary>
        /// Be used for Report Center
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RCNodeType RCNodeType { get; set; }

        /// <summary>
        /// Be used for Auditor document auditing tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool DocumentAuditingFeatureActived { get; set; }

        /// <summary>
        /// Be used for Auditor document auditing tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool DocumentAuditingSolutionDeployed { get; set; }

        /// <summary>
        /// Be used for Auditor document auditing tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DocumentAuditingMessage { get; set; }

        /// <summary>
        /// Be used for Manage Feature tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ManageFeatureFeatureActived { get; set; }

        /// <summary>
        /// Be used for Manage Feature tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ManageFeatureSolutionDeployed { get; set; }

        /// <summary>
        /// Be used for Manage Feature tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ManageFeatureMessage { get; set; }

        /// <summary>
        /// Be used for Manage Feature tree
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ManageFeatureType ManageFeatureType { get; set; }

        /// <summary>
        /// Be used for RC caching service
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool HasUniqueSettings { get; set; }

        /// <summary>
        /// Be used for RC caching service
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsChanged { get; set; }

        /// <summary>
        /// Be used for RC caching service
        /// 0:name
        /// 1:type,
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ColumnInfo { get; set; }

        /// <summary>
        /// Hide selector for Granular Restore Search.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool SelectorHidden { get; set; }

        /// <summary>
        /// Control subsite node selector enable for Granular Restore tree & Granular Object Base Search tree node是否有Jobs节点.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool SelectorEnable { get; set; }

        /// <summary>
        /// Indicate if the node is advance search result node.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsAdvancedSearchResult { get; set; }

        /// <summary>
        /// Be used for restore advance search & granular backup data upgrade import tree.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long BackupTime { get; set; }

        /// <summary>
        /// Be used for Front-End Deployment
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Location { get; set; }

        /// <summary>
        /// Be used for BPOS Web Application
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AgentGroupId { get; set; }

        /// <summary>
        /// 用来存储SiteColumnGroup以及ContentTypeGroup是否选中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("isChecked")]
        public bool IsChecked { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("isVNode")]
        public bool IsVNode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("agentType")]
        public string AgentType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("VirtualNodes")]
        public List<VirtualTreeNode> VNodes { get; set; }
        /// <summary>
        /// DM WFE IISSiteSetting中的信息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IISConfigInfo")]
        public IISConfigInfo IISInfo { get; set; }
        /// <summary>
        /// 记录WFE default site 的SPObjectId。 记录GranularObjectSearcheTree的WebAppId。
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("SPobjectid")]
        public string SPobjectid { get; set; }
        /// <summary>
        /// DM WFESetting中是否含有Web.config的判定
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("HasWFEWebConfig")]
        public bool HasWFEWebConfig { get; set; }
        /// <summary>
        /// 标示SharePoint App是否可以升级
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IsAppUpdateAvailable")]
        public bool IsAppUpdateAvailable { get; set; }
        /// <summary>
        /// DM SiteCollection UserSolution Count
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("UserSolution")]
        public List<SolutionDetailDTO> UserSolution { get; set; }
        /// <summary>
        /// DM中solution center的brower显示信息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("SolutionDetail")]
        public SolutionDetailDTO SolutionDetail { get; set; }
        /// <summary>
        /// DM中design manager的compare使用的比对信息。
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DMCompareInfo")]
        public DMCompareInfoDTO DMCompareInfo { get; set; }

        /// <summary>
        /// 用于判断是否隐藏Compare操作的节点。
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("isHidCompareNode")]
        public bool IsHidCompareNode { get; set; }

        /// <summary>
        /// Is used to indicate source and destination are the same.
        /// True: source node is the same as destination node.
        /// False: source node and destination node are different.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("MatchedNode")]
        public bool IsMatchedNode { get; set; }


        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IsSystemFileOrFolder")]
        public Boolean IsSystemFileOrFolder { get; set; }
        /// <summary>
        /// 用于判断当前节点相同，深层次子节点不同的情况
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IsMatchedChildNode")]
        public bool IsMatchedChildNode { get; set; }

        /// <summary>
        /// 这个节点用来判断Design Manager中Content Type和Site Column是属于哪个组的节点。
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DMPropertyGroup")]
        public string DMPropertyGroup { get; set; }


        /// <summary>
        /// 这个节点用来标示DM中的column是哪个组下的，这个组是自己分配的
        /// 不是share point中的column分组，做dependency使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DMColumnGroup")]
        public string DMColumnGroup { get; set; }
        /// <summary>
        /// 标示一个分组内有多少个column
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DMColumnGroupCount")]
        public string DMColumnGroupCount { get; set; }

        /// <summary>
        /// DM做Job之前需要Browse节点
        /// Agent根据这个属性判断目的端的Tree需要Browse到那个级别
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("batchProcessingType")]
        public BatchProcessingType BatchProcessingType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("NodeSelectType")]
        public NodeSelectType NodeSelectType { get; set; }

        /// <summary>
        /// 由于DPM在Compare WFE IIS的节点时，需要将Template的节点删除，
        /// 因此需要将这个节点的一些属性存入到这个对象当中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IISTemplateNode")]
        public IISTemplateNode IISTemplateNode { get; set; }

        /// <summary>
        /// 由于DPM需要在broswer中进行目的tree的filter
        /// 为了不影响Common的Broswer所以在扩展节点中
        /// 增加这个属性用于控制所属的TreeNodeDTO是否可以选中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ShouldChecked")]
        public int ShouldChecked { get; set; }

        /// <summary>
        /// DPM的MMS DB地址信息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("MMSDBInfo")]
        public ServiceDB MMSDBInfo { get; set; }

        /// <summary>
        /// DPM MMS Used. MMS是否存在分区
        /// </summary>
        [DataMember]
        public Boolean IsMetadataPartition { get; set; }

        /// <summary>
        /// DPM MMS Used. Sharepoint MMS 分区 id
        /// </summary>
        [DataMember]
        public Guid PartitionId { get; set; }

        /// <summary>
        /// DPM MMS Used. Tency admin管理的sitecollection url
        /// </summary>
        [DataMember]
        public List<string> ManagedSitesUrl { get; set; }

        /// <summary>
        /// Be used for storing agent type for each farm
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<string> AgentTypes { get; set; }


        /// <summary>
        /// Be used for Download to distinguish ListItem .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ListItemID { get; set; }


        /// <summary>
        /// Be used for Download to distinguish Is SrcTree or Is DestTree .
        /// True 为SrcNode   False为DestNode
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsSrcNode { get; set; }


        /// <summary>
        /// For Storage History Version
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<HistoryVersion> HistoryVersions { get; set; }

        /// <summary>
        /// The size of the node
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long Size { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Boolean IsFba { get; set; }

        /// <summary>
        /// Connector Feature Management: See AvePoint.Adonis.StorageOptimization.Connector.Object.FeatureStatus
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Int1 { get; set; }

        /// <summary>
        /// DPM用此节点标识pattern获取Site Collection Id集合得新逻辑
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Int2 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ManagedPathDto> ManagedPathList { get; set; }

        /// <summary>
        /// 用于判断tree的状态，前面已经添加了一个因此重新命名为HoldSyncEnable
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool HoldSyncEnable { get; set; }

        /// <summary>
        /// Used by CA
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int SiteLockStatus { get; set; }

        public uint SiteLockStatusValue
        {
            get { return (uint)this.SiteLockStatus; }
            set { this.SiteLockStatus = (int)value; }
        }

        [DataMember(EmitDefaultValue = false)]
        public bool UseCrossDomain { get; set; }
        /// <summary>
        /// Used by CA
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SecurityScopeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FilterProperty> FilterProperties { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DestFilterId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FarmBuildVersion { get; set; }

        #region ===== for Vault =====
        [DataMember(EmitDefaultValue = false)]
        public bool IsVaultActive { set; get; }
        #endregion

        [DataMember]
        public TreeType TreeType { get; set; }

        private bool isAccessible = true;
        [DataMember]
        public bool IsAccessible
        {
            get { return isAccessible; }
            set { isAccessible = value; }
        }


        /// <summary>
        /// 判断是否是DocAve Online import数据
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IsOnlineData")]
        public bool IsOnlineData { get; set; }


        /// <summary>
        /// 判断是否是Cloud Backup数据
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("IsCloudBackupData")]
        public bool IsCloudBackupData { get; set; }

        //用于Security Trimming，仅Client端用于获取用户权限信息，不用于存储
        public List<SPTreePermissionMappingDto> PermissionList { get; set; }

        public NodeExtensionDto()
        {
            AgentTypes = new List<string>();
        }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            IsAccessible = true;//反序列化时 先将属性设为默认值 以解决旧数据升级问题
        }
    }

    [DataContract]
    [XmlRootAttribute("Languages")]
    public class Languages : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("default")]
        public string Default { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Language")]
        public List<Language> Language { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    [XmlRootAttribute("ServiceDB")]
    public class ServiceDB : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("Address")]
        public string Address { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ConnectionString")]
        public string ConnectionString { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    [XmlRootAttribute("Language")]
    public class Language : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("lcid")]
        public int LCID { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    [XmlRootAttribute("ContentDB")]
    public class ContentDB : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("id")]
        public string ID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("name")]
        public string Name { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    [XmlRootAttribute("ContentTypeHubList")]
    public class ContentTypeHubList : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SiteId")]
        public Guid SiteId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("ListID")]
        public Guid ListID { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("ListName")]
        public string ListName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("ConnectionString")]
        public string ConnectionString { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("PackageId")]
        public string PackageId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("Unpublished")]
        public string Unpublished { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("ServiceName")]
        public string ServiceName { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    [XmlRootAttribute("VirtualNode")]
    public class VirtualTreeNode : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("nodeId")]
        public string NodeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("nodeType")]
        public NodeLevel NodeType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("includeNew")]
        public int IncludeNew { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("childBeLoaded")]
        public bool ChildBeLoaded { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("expanded")]
        public bool Expanded { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("selectAll")]
        public SelectAllState SelectAll { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("checkNumber")]
        public int CheckNumber { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }


    [DataContract]
    [XmlRootAttribute("HistoryVersion")]
    public class HistoryVersion : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("id")]
        public string ID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("isChecked")]
        public bool IsChecked { get; set; }

        //后续都更改后要删除掉
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("version")]
        public string Version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("createTime")]
        public long CreateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("description")]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("path")]
        public string Path { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("type")]
        public SolutionType Type { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }

    }

    [DataContract]
    [XmlRootAttribute("ManageFeatureType")]
    public enum ManageFeatureType
    {
        [EnumMember]
        BlogActivity = 0,

        [EnumMember]
        DocumentAuditing = 1,

        [EnumMember]
        SocialActivityAnalysisWebPart = 2,

        [EnumMember]
        RCAPI = 20140801,
    }

    [DataContract]
    [XmlRootAttribute("SolutionType")]
    public enum SolutionType : int
    {
        [EnumMember]
        FarmSolution = 0,

        [EnumMember]
        UserSolution = 1,
    }

    [DataContract]
    public enum ManagedPathType
    {
        [EnumMember]
        Explicit,

        [EnumMember]
        ExplicitInclusion,

        [EnumMember]
        Wildcard,

        [EnumMember]
        WildcardInclusion,

        [EnumMember]
        Exclusion,
    }

    [DataContract]
    public class ManagedPathDto : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ManagedPathType Type { get; set; }

        public override string ToString()
        {
            return Name;
        }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    public enum CompatibilityLevelType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SP2010Mode = 14,
        [EnumMember]
        SP2013Mode = 15,
        [EnumMember]
        SP2016Mode = 16,
    }

    [DataContract]
    [XmlRootAttribute("FilterProperty")]
    public class FilterProperty : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("RuleType")]
        public FilterPolicyRuleType RuleType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Property")]
        public List<PropertyKeyValuePair> Property { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }
    [DataContract]
    [XmlRootAttribute("PropertyKeyValuePair")]
    public class PropertyKeyValuePair : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Key")]
        public string Key { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("PropertyValues")]
        public List<string> Values { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }

    [DataContract]
    public enum FilterPolicyRuleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ResultLevel,
        [EnumMember]
        Url,
        [EnumMember]
        Title,
        [EnumMember]
        Name,
        [EnumMember]
        Template,
        [EnumMember]
        CreatedBy,
        [EnumMember]
        CreatedTime,
        [EnumMember]
        ModifiedBy,
        [EnumMember]
        ModifiedTime,
        [EnumMember]
        Owner,
        [EnumMember]
        Inheritance,
        [EnumMember]
        Permission,
        [EnumMember]
        Attribute,
        [EnumMember]
        FullTextIndex,
        //for auditor
        [EnumMember]
        Country,

        //Add for CA
        [EnumMember]
        UserAndGroup,
        [EnumMember]
        ContentType,
        [EnumMember]
        Versions,
        [EnumMember]
        Auditing,
        [EnumMember]
        Versioning,
        [EnumMember]
        CustomPropertyText,
        [EnumMember]
        CustomPropertyBoolean,
        [EnumMember]
        CustomPropertyDateTime,
        [EnumMember]
        CustomPropertyNumber,
        [EnumMember]
        AnonymousAccess,
        [EnumMember]
        LockStatus,
        [EnumMember]
        Size,
        [EnumMember]
        LastAccessedTime,
        [EnumMember]
        Columns,
        [EnumMember]
        ContentTypes,
        [EnumMember]
        ColumnDateTime,
        [EnumMember]
        ColumnBoolean,
        [EnumMember]
        ColumnNumber,
        [EnumMember]
        ColumnText,
        [EnumMember]
        ColumnChoice,
    }
}
