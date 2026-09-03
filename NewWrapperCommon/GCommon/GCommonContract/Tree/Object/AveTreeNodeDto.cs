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
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public interface IAveTreeNodeDto
    {
        string ID { get; set; }

        string Name { get; set; }

        string DisplayName { get; set; }

        string Title { get; set; }

        string FullPath { get; set; }

        NodeLevel Level { get; set; }

        NodeType Type { get; set; }

        string FarmID { get; set; }

        string ParentId { get; set; }

        IAveTreeNodeDto Parent { get; set; }

        IList Children { get; set; }

        int CheckNumber { get; set; }

        List<NodeFilterPolicy> FilterPolicy { get; set; }

        IncludeNewState IncludeNew { get; set; }

        SelectAllState SelectAll { get; set; }

        int CheckState { get; set; }

        bool ChildrenLoaded { get; set; }

        int ChildrenCount { get; set; }

        int FilteredChildrenCount { get; set; }

        bool CanChildrenBeLoaded { get; set; }

        int CurrentPage { get; set; }

        int StartIndex { get; set; }

        bool Expanded { get; set; }

        string PageInfo { get; set; }

        bool HasNextPage { get; set; }

        int Offset { get; set; }

        int FilteredOffset { get; set; }
        //int Depth { get; set; }

        long ObjectSize { get; set; }

        //int FolderDepth { get; set; }

        PageNodeType PageNodeType { get; set; }

        List<NodeExtraOption> ExtraOptions { get; set; }

        NodeExtensionDto NodeExtension { get; set; }
    }

    [KnownType("GetKnownTypes")]
    [DataContract(IsReference = true)]
    [XmlRootAttribute("AveTreeNode")]
    public abstract class AveTreeNodeDto<T> : IAveTreeNodeDto, IExtensibleDataObject where T : AveTreeNodeDto<T>
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

        //public override int GetHashCode()
        //{
        //    return (Name == null ? 0 : Name.GetHashCode()) ^ (FullPath == null ? 0 : FullPath.GetHashCode());
        //}

        public override bool Equals(object obj)
        {
            if (!(obj is AveTreeNodeDto<T>))
            {

                return false;
            }

            AveTreeNodeDto<T> node = obj as AveTreeNodeDto<T>;

            //return ID == node.ID && Name == node.Name && FullPath == node.FullPath;
            return Name == node.Name && FullPath == node.FullPath;

        }

        public AveTreeNodeDto()
        {
            this.Children = new List<T>();
            this.ExtraOptions = new List<NodeExtraOption>();
            this.SelectAll = SelectAllState.Undefined;
            this.IncludeNew = IncludeNewState.Undefined;
            this.NodeExtension = new NodeExtensionDto();
        }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// 用于界面显示的名字
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 节点的Destination name，用于OOP restore
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("destinationName")]
        public string DestinationName { get; set; }

        /// <summary>
        /// 暂时没有使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 节点的Title，用于Top Level Site的 Root Site{Title}
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        ///对于Site Collection和Site，此值是从http开始的全路径，对于List,Folder和Item，此值是相对路径(ServerRelativeUrl)。
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("fullPath")]
        public string FullPath { get; set; }

        /// <summary>
        /// 节点的级别，WebApp, Site Collection, Site....
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("level")]
        public NodeLevel Level { get; set; }

        /// <summary>
        /// 节点的类型 List, Library...
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("type")]
        public NodeType Type { get; set; }


        /// <summary>
        /// Farm ID
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("farmID")]
        public string FarmID { get; set; }

        /// <summary>
        /// 父节点ID
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("parentId")]
        public string ParentId { get; set; }

        IAveTreeNodeDto IAveTreeNodeDto.Parent
        {
            get
            {
                return this.Parent;
            }
            set
            {
                this.Parent = (T)value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [XmlIgnore]
        public T Parent { get; set; }


        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("offset")]
        public int Offset { get; set; }


        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("filteredoffset")]
        public int FilteredOffset { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("objectsize")]
        public long ObjectSize { get; set; }

        IList IAveTreeNodeDto.Children
        {
            get
            {
                return this.Children;
            }
            set
            {
                this.Children = (List<T>)value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("AveTreeNode")]
        public List<T> Children { get; set; }

        /// <summary>
        /// 标识节点的选中状态：
        /// GConstants.TreeCheckNumber.CHECKED
        /// GConstants.TreeCheckNumber.UNCHECKED
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("checkNumber")]
        public int CheckNumber { get; set; }

        /// <summary>
        /// IncludeNew节点是否选中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("includeNew")]
        public IncludeNewState IncludeNew { get; set; }

        /// <summary>
        /// SelectAll节点是否选中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("selectAll")]
        public SelectAllState SelectAll { get; set; }

        /// <summary>
        /// 没有使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("checkState")]
        public int CheckState { get; set; }

        /// <summary>
        /// 子节点是否已经被载入
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("childrenLoaded")]
        public bool ChildrenLoaded { get; set; }

        /// <summary>
        /// 子节点数量
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("childrenCount")]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// 过滤后的子节点数量
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("filteredChildrenCount")]
        public int FilteredChildrenCount { get; set; }

        /// <summary>
        /// 子节点是否可以继续Load
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("canChildrenBeLoaded")]
        public bool CanChildrenBeLoaded { get; set; }

        /// <summary>
        /// 子节点显示第几页
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("currentPage")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// 当前的页第一个元素在所有子节点的位置
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// 节点是否展开
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("expanded")]
        public bool Expanded { get; set; }

        /// <summary>
        /// 原用于在删除虚节点之后保存虚节点上的属性，现已不推荐使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ExtraOption")]
        public List<NodeExtraOption> ExtraOptions { get; set; }

        /// <summary>
        /// 节点的扩展字段
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("NodeExtension")]
        public NodeExtensionDto NodeExtension { get; set; }

        /// <summary>
        /// 用于Item分页的PageInfo
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PageInfo { get; set; }

        /// <summary>
        /// 标识Item是否有下一页
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 节点所处的深度
        /// </summary>
        //[DataMember]
        //public int Depth { get; set; }

        /// <summary>
        /// Folder节点相对于List的深度
        /// </summary>
        //[DataMember]
        //public int FolderDepth { get; set; }

        /// <summary>
        /// 分页节点的类型
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PageNodeType PageNodeType { get; set; }

        public override string ToString()
        {
            return TextNode("", true);
        }

        /// <summary>
        /// 转换Tree型结构Text
        /// </summary>
        /// <param name="prefix">用于此节点之前的\t和连线</param>
        /// <param name="isLastChild">此节点是否是该层最后一个节点</param>
        /// <returns></returns>
        public string TextNode(string prefix, bool isLastChild)
        {
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.Append(prefix + (isLastChild ? "└" : "├") + (CheckNumber == 1 ? "√" : " ") + Name + " " + (SelectAll == SelectAllState.Checked ? "SelectAll" : "") + " " + (IncludeNew == IncludeNewState.Checked ? "IncludeNew" : "") + " " + this.Offset + " " + this.StartIndex + " " + this.ChildrenCount + " " + (this.NodeExtension != null ? this.NodeExtension.IsAccessible + "" : "") + "\r\n");
            if (Children != null)
            {
                for (int i = 0; i < Children.Count - 1; i++)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "" + "\t", false));
                    }
                    else
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "│" + "\t", false));
                    }
                }
                if (Children.Count > 0)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "" + "\t", true));
                    }
                    else
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "│" + "\t", true));
                    }
                }
            }
            return textBuilder.ToString();
        }

        /// <summary>
        /// FilterPolicy
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("FilterPolicy")]
        public List<NodeFilterPolicy> FilterPolicy
        {
            get;
            set;
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
    public enum NodeLevel
    {
        [EnumMember]
        [Description("Undefined")]
        Undefined = 0,

        [EnumMember]
        Root = -2,

        [EnumMember]
        Farm = -1,

        [EnumMember]
        [Description("Web Application")]
        WebApplication = 2,

        [EnumMember]
        ContentDBs = 4,

        [EnumMember]
        SiteCollections = 6,

        [EnumMember]
        ContentDB = 30,

        [EnumMember]
        [Description("Site Collection")]
        SiteCollection = 100,

        [EnumMember]
        [Description("Site")]
        Site = 200,

        [EnumMember]
        Lists = 201,

        [EnumMember]
        Sites = 202,

        [EnumMember]
        Apps = 280,

        [EnumMember]
        App = 281,

        [EnumMember]
        AppData = 282,

        [EnumMember]
        RecycleBin = 283,

        #region Deployment Manager中DesignManager部分的虚节点
        [EnumMember]
        VDesignLists = 203, //对应site实节点下面design Lists节点

        [EnumMember]
        VSiteAdmin = 204, //对应site实节点下面SiteAdmin节点

        [EnumMember]
        VSiteColumns = 205,//对应site admin虚节点下面site column。

        [EnumMember]
        VContentTypes = 206,//对应site admin虚节点下面site contentType。

        [EnumMember]
        VLookAndFeels = 207,//对应site实节点下面,暂时隐藏

        [EnumMember]
        VUsersAndPerms = 208,//对应site实节点下面,暂时隐藏
        [EnumMember]
        ContentTypeGroup = 209,//对应contenttype虚节点下面contentType group。
        [EnumMember]
        SiteContentType = 210,//对应contentType group下面的Site ContentType。
        [EnumMember]
        SiteColumnGroup = 211,//对应site admin虚节点下面site column Group。
        [EnumMember]
        SiteColumn = 212,//对应site column Group下面的Site Column。
        [EnumMember]
        SiteCTWorkflow = 213,
        [EnumMember]
        VSiteCTWorkflow = 214,
        [EnumMember]
        SiteWorkflow = 215,
        [EnumMember]
        VSiteWorkflow = 216,
        [EnumMember]
        SiteSetting = 255,

        #endregion

        [EnumMember]
        [Description("List")]
        List = 300,

        [EnumMember]
        Folder = 400,

        [EnumMember]
        DesignFolder = 404,//Design Folder，一般是Root folder下面的hidden folder

        [EnumMember]
        Item = 500,

        [EnumMember]
        ItemVersion = 550,

        [EnumMember]
        DesignItem = 502,//Design Folder下面的item，一般是system file.

        //[Obsolete("You should use NodeType to distinguish List and Library.")]
        [EnumMember]
        Library = 301,

        [EnumMember]
        RootFolder = 402, //list rootfolder & web rootfolder
        [EnumMember]
        DesignObjRootFolder = 403, //和rootfolder同级别的，但是rootfolder

        [EnumMember]
        Folders = 401,

        [EnumMember]
        Items = 501,

        [EnumMember]
        DesignItems = 503,

        [EnumMember]
        DesignFolders = 504,

        [EnumMember]
        ListSetting = 600,

        [EnumMember]
        VListAdmin = 601, //对应list实节点下面ListAdmin节点

        [EnumMember]
        VListColumns = 602,//对应list admin虚节点下面list columns

        [EnumMember]
        VListContentTypes = 603,//对应list admin虚节点下面list contentTypes

        [EnumMember]
        ListContentTypeGroup = 604,//对应List contenttype虚节点下面list contentType group

        [EnumMember]
        ListContentType = 605,//对应List contentType group下面的List ContentType

        [EnumMember]
        ListColumnGroup = 606,//对应List admin虚节点下面List column Group

        [EnumMember]
        ListColumn = 607,//对应List column Group下面的List Column

        [EnumMember]
        ListWorkflow = 608,

        [EnumMember]
        VListWorkflow = 609,

        [EnumMember]
        ListCTWorkflow = 610,

        [EnumMember]
        VListCTWorkflow = 611,

        /*for CA Security Search start*/
        [EnumMember]
        Groups = 1000,

        [EnumMember]
        SharePointGroup = 1001,

        [EnumMember]
        DomainGroup = 1002,

        [EnumMember]
        SharePointUser = 1003,

        [EnumMember]
        Users = 1100,

        [EnumMember]
        User = 1101,
        /*for CA Security Search end*/
        #region 将用ContentManager和Design Manager discover时候使用老方法，之后会修改掉。
        //[Obsolete("This value was used by DocAve5, you should use Lists to instead of this value in DocAve6.")]
        [EnumMember]
        Deprecated_Lists = 252,

        //[Obsolete("This value was used by DocAve5, you should use Sites to instead of this value in DocAve6.")]
        [EnumMember]
        Deprecated_Sites = 251,

        //[Obsolete("This value was used by DocAve5, you should use Item to instead of this value in DocAve6.")]
        [EnumMember]
        Deprecated_Item = 453,

        //[Obsolete("This value was used by DocAve5, you should use Folder to instead of this value in DocAve6.")]
        [EnumMember]
        Deprecated_Folder = 454,
        #endregion

        [EnumMember]
        Device = 2002,

        [EnumMember]
        AgentGroup = 2000,

        [EnumMember]
        FSFolder = 2100,

        [EnumMember]
        FSFile = 2200,
        [EnumMember]
        FSConnectionGroups = 2201,
        [EnumMember]
        FSConnectionGroup = 2202,

        #region Deployment Manager中使用的特殊虚节点
        [EnumMember]
        DMVirtualNode = 2300,//对应WebApplications

        [EnumMember]
        FEWVirtualNode = 2301,//对应Front-end 虚节点

        [EnumMember]
        SLCVirtualNode = 2302,//对应solutions虚节点

        [EnumMember]
        SharedServices = 2303,//对应Services虚节点

        [EnumMember]
        FEWAgentNode = 2400, //Front-end下面的Agent虚节点

        [EnumMember]
        IISettingsVirtualNode = 2401,//

        [EnumMember]
        GACVirtualNode = 2402,

        [EnumMember]
        CustomFeatureVirtualNode = 2403,//对应

        [EnumMember]
        SiteDefinitionVirtualNode = 2404,

        [EnumMember]
        FileSystemVirtualNode = 2405,

        [EnumMember]
        IISPNode = 2406,

        [EnumMember]
        IISDefaultSiteNode = 2407,

        [EnumMember]
        IISNonIISiteNode = 2408,

        [EnumMember]
        GACFirstVirtualNode = 2409,

        [EnumMember]
        GACSecondVirtualNode = 2410,
        [EnumMember]
        GACThirdVirtualNode = 2425,
        [EnumMember]
        GACNode = 2411,

        [EnumMember]
        CustomFeatureNode = 2412,

        [EnumMember]
        SiteDefinitionNode = 2413,

        [EnumMember]
        FileSystemDiskNode = 2414,

        [EnumMember]
        FileSystemFolderNode = 2415,

        [EnumMember]
        FileSystemFileNode = 2416,

        [EnumMember]
        FileSystemFoldersNode = 2417, //virtual node

        [EnumMember]
        FileSystemFilesNode = 2418,  //virtual node

        [EnumMember]
        IISTemplatesNode = 2419,

        [EnumMember]
        IISiteNode = 2420,

        [EnumMember]
        IISFolderNode = 2421,

        [EnumMember]
        IISWebConfigNode = 2422,

        [EnumMember]
        SolutionNode = 2423,

        [EnumMember]
        IISFileNode = 2424,

        [EnumMember]
        VSiteSolutionNode = 2500,

        [EnumMember]
        StorageLevel = 2550, //  Storage Level特殊节点

        [EnumMember]
        ManagedMetadataService = 2600, // DeploymentManager中的Managed Metadata Service节点

        [EnumMember]
        MMS = 2610, // DeploymentManager中的MMS节点

        [EnumMember]
        Traditional = 2611,

        [EnumMember]
        Multitenant = 2612,

        [EnumMember]
        MutiTenantModeTermStore = 2613,

        [EnumMember]
        TermStore = 2620, // DeploymentManager中的Term Store节点

        [EnumMember]
        GlobalTermGroup = 2630, // DeploymentManager中的 Global Term Group节点

        [EnumMember]
        LocalTermGroup = 2631, // DeploymentManager中的Local Term Group节点

        [EnumMember]
        TermGroup = 2640, // DeploymentManager中的Term Group节点

        [EnumMember]
        TermSet = 2650, // DeploymentManager中的Term Set节点

        [EnumMember]
        Term = 2670, // DeploymentManager中的Term节点

        [EnumMember]
        ContentTypeHub = 2680, // DeploymentManager中的Content Type hub节点

        [EnumMember]
        PublishingContentType = 2690, // DeploymentManager中Content Type hub下的Content Type节点

        [EnumMember]
        PatternVersions = 2700,

        [EnumMember]
        Pattern = 2710,

        [EnumMember]
        PatternQueue = 2720,
        #endregion

        #region migration node level

        [EnumMember]
        Agent = 3000,

        //connection level
        [EnumMember]
        FileConnection = 3010,

        [EnumMember]
        LivelinkConnection = 3011,

        [EnumMember]
        NotesConnection = 3012,

        [EnumMember]
        ExchangeConnection = 3013,

        [EnumMember]
        ExchangeFolder = 3014,

        [EnumMember]
        ExchangeItem = 3015,

        [EnumMember]
        QuickPlaceConnection = 3016,

        [EnumMember]
        DocumentumConnection = 3017,

        //virtual items level
        [EnumMember]
        FileItems = 3020,

        [EnumMember]
        eRoomItems = 3021,

        [EnumMember]
        LivelinkItems = 3022,

        [EnumMember]
        NotesItems = 3023,

        //[EnumMember]//don't know
        //QuickPlaceItems = 3024,

        [EnumMember]
        DocumentumItems = 3025,
        #endregion

        #region eRoom Migration

        [EnumMember]
        eRoomCommunity = 3100,
        [EnumMember]
        eRoomFacility = 3101,
        [EnumMember]
        eRoomRoom = 3102,
        [EnumMember]
        eRoomList = 3103,
        [EnumMember]
        eRoomFolder = 3104,
        [EnumMember]
        eRoomItem = 3105,
        [EnumMember]
        ERMRoot = 3106,
        [EnumMember]
        ERMAgent = 3107,
        [EnumMember]
        ERMConnection = 3108,
        [EnumMember]
        ERMFacility = 3109,
        [EnumMember]
        ERMeRoom = 3110,
        [EnumMember]
        ERMList = 3111,
        [EnumMember]
        ERMFolder = 3112,
        [EnumMember]
        ERMItem = 3113,
        [EnumMember]
        ERMItems = 3114,

        #endregion

        #region Livelink Migration

        [EnumMember]
        LivelinkWorkspace = 3150,
        [EnumMember]
        LivelinkProject = 3151,
        [EnumMember]
        LivelinkList = 3152,
        [EnumMember]
        LivelinkItem = 3153,

        #endregion

        #region Lotus Notes Migration

        [EnumMember]
        LotusNotesDominoServer = 3160,
        [EnumMember]
        LotusNotesDatabase = 3161,
        [EnumMember]
        LotusNotesView = 3162,
        [EnumMember]
        LotusNotesDocument = 3163,

        #endregion

        #region Quick Place Migration
        [EnumMember]
        QuickPlaceDominoServer = 3170,

        [EnumMember]
        QuickPlacePlace = 3171,

        [EnumMember]
        QuickPlaceRoom = 3172,


        #endregion

        #region  Documentum Migration
        [EnumMember]
        DocumentumCabinet = 3180,

        [EnumMember]
        DocumentumObject = 3181,

        [EnumMember]
        DocumentumFolder = 3182,

        [EnumMember]
        DocumentumVirtualDocument = 3183,

        [EnumMember]
        DocumentumSnapShort = 3185,

        [EnumMember]
        DocumentumSnapShot = 3184,
        #endregion

        [EnumMember]
        CustomDatabase = 4000, // PR Custom Database Root Node

        #region Upgrade Data
        [EnumMember]
        Plan = 4010,
        [EnumMember]
        Cycle = 4012,
        [EnumMember]
        Job = 4013,
        #endregion

        #region SQL Server Data Manager
        [EnumMember]
        SRMFilePath = 4020,
        [EnumMember]
        SRMFolder = 4021,
        [EnumMember]
        SRMFile = 4022,
        [EnumMember]
        SRMBAKFilePath = 4023,
        [EnumMember]
        SRMDatabase = 4024,
        [EnumMember]
        SDMInstance = 4025,
        #endregion

        #region High Availability
        [EnumMember]
        HAGroup = 4030,
        #endregion

        #region CA Policy Enforcer

        [EnumMember]
        PERule = 4040,
        [EnumMember]
        PEDetail = 4041,

        #endregion

        #region analyze VHD Backup
        [EnumMember]
        SSDMVHDFile = 4050,
        [EnumMember]
        VHDFolders = 4051,
        [EnumMember]
        VHDItems = 4052,
        [EnumMember]
        VHDItem = 4053,
        [EnumMember]
        VHDFilePath = 4054,
        [EnumMember]
        LDFFile = 4055,
        [EnumMember]
        NDFFile = 4056,
        [EnumMember]
        MDFFile = 4057,
        #endregion
    }

    public static class NodeLevelExtensions
    {
        public static bool IsIn(this NodeLevel value, params NodeLevel[] values)
        {
            return values.Any(v => v == value);
        }

        public static bool IsNotIn(this NodeLevel value, params NodeLevel[] values)
        {
            return !IsIn(value, values);
        }
    }

    [DataContract]
    public enum NodeType
    {
        [EnumMember]
        UnspecifiedBaseType = -1,

        [EnumMember]
        GenericList = 0,

        [EnumMember]
        DocumentLibrary = 1,

        [EnumMember]
        Unused = 2,

        [EnumMember]
        DiscussionBoard = 3,

        [EnumMember]
        Survey = 4,

        [EnumMember]
        Issue = 5,

        [EnumMember]
        ManualInput = 100,

        [EnumMember]
        CAWebapp = 201,

        [EnumMember]
        Document = 6,

        [EnumMember]
        ListItem = 7,

        #region PlatformRecovery模块中lists下的节点type定义
        [EnumMember]
        Announcements = 104,
        [EnumMember]
        Contacts = 105,
        [EnumMember]
        Calendar = 106,
        [EnumMember]
        CustomList = 10100,//由于和ManualInput冲突，加了10000
        [EnumMember]
        CustomListInDB = 120,
        //[EnumMember]
        //DiscussionBoard = 108,
        [EnumMember]
        IssueTracking = 1100,
        [EnumMember]
        Links = 130,
        [EnumMember]
        projectTask = 150,
        [EnumMember]
        StatusList = 432,
        //[EnumMember]
        //Survey = 102,
        [EnumMember]
        Tasks = 107,
        [EnumMember]
        ExternalList = 600,
        [EnumMember]
        ImportSpreadsheet = 10001,// 为对应数据库，自定义
        #endregion

        #region DesignManager里面的Design List定义
        [EnumMember]
        ListTemplate = 300,
        [EnumMember]
        MasterPageGallery = 301,
        [EnumMember]
        Images = 302,
        [EnumMember]
        WebPartGallery = 303,
        [EnumMember]
        StyleLibrary = 304,
        [EnumMember]
        SiteCollectionImages = 305,
        [EnumMember]
        ThemeGallery = 306,
        [EnumMember]
        UserInformationList = 307,
        [EnumMember]
        wfpub = 308,
        [EnumMember]
        TaxomonyHiddenList = 310,
        [EnumMember]
        SitePages = 311,
        [EnumMember]
        SiteAssets = 312,
        [EnumMember]
        ReportingTemplates = 313,
        [EnumMember]
        ReportingMetadata = 314,
        [EnumMember]
        FormTemplates = 315,
        [EnumMember]
        ConvertedForms = 316,
        [EnumMember]
        ContenttypePublishingErrorLog = 317,
        [EnumMember]
        Solutions = 318,
        #endregion

        #region WFEDeployManager里面的Node Type定义
        [EnumMember]
        GACFirstNode = 400,

        [EnumMember]
        GACSecondNode = 401,
        [EnumMember]
        GACThirdNode = 402,
        #endregion

        #region MetaDataService 里面SystemTermGroup和SystemTermSet的定义
        [EnumMember]
        SystemTermGroup = 500,

        [EnumMember]
        UserTermGroup = 501,

        #endregion

        #region eRoom Migration

        #region eRoom List
        [EnumMember]
        eRoomHomeFolder = 600,
        [EnumMember]
        eRoomFolder = 601,
        [EnumMember]
        eRoomInbox = 602,
        [EnumMember]
        eRoomDiscussionPage = 603,
        [EnumMember]
        eRoomPollPage = 604,
        [EnumMember]
        eRoomCalendarPage = 605,
        [EnumMember]
        eRoomProjectSchedulePage = 606,
        [EnumMember]
        eRoomDBPage = 607,
        [EnumMember]
        eRoomDBProcess = 608,
        [EnumMember]
        eRoomDashboardPage = 609,
        [EnumMember]
        eRoomFolderPage = 610,
        [EnumMember]
        eRoomAllLinks = 611,
        [EnumMember]
        eRoomAllNotes = 612,
        [EnumMember]
        eRoomLinkedFolder = 613,
        #endregion

        #region eRoom Item

        #endregion

        #endregion

        #region Livelink Migration

        #region Livelink List

        [EnumMember]
        LivelinkAppearance = 700,
        [EnumMember]
        LivelinkCategory = 701,
        [EnumMember]
        LivelinkChannel = 702,
        [EnumMember]
        LivelinkCollection = 703,
        [EnumMember]
        LivelinkCompoundDocument = 704,
        [EnumMember]
        LivelinkCustomView = 705,
        [EnumMember]
        LivelinkDiscussion = 706,
        [EnumMember]
        LivelinkFolder = 707,
        [EnumMember]
        LivelinkLiveReport = 708,
        [EnumMember]
        LivelinkPoll = 709,
        [EnumMember]
        LivelinkProspector = 710,
        [EnumMember]
        LivelinkTaskList = 711,
        [EnumMember]
        LivelinkWorkflowMap = 712,
        [EnumMember]
        LivelinkEnterpriseWS = 713,
        [EnumMember]
        LivelinkPersonalWS = 714,
        [EnumMember]
        LivelinkOtherAccessWS = 715,
        [EnumMember]
        LivelinkProject = 716,

        [EnumMember]
        LivelinkContractFolder = 717,
        [EnumMember]
        LivelinkBusinessLeads = 718,
        #endregion

        #region Livelink Item

        [EnumMember]
        LivelinkDocument = 750,
        [EnumMember]
        LivelinkShortcut = 751,
        [EnumMember]
        LivelinkTextDocument = 752,
        [EnumMember]
        LivelinkURL = 753,
        [EnumMember]
        LivelinkWorkflowStatus = 754,
        [EnumMember]
        LivelinkXmlDtd = 755,
        [EnumMember]
        LivelinkProjectTemplate = 756,
        [EnumMember]
        LivelinkTaskGroup = 757,
        [EnumMember]
        LivelinkAppearanceWorkspaceFolder = 758,
        [EnumMember]
        LivelinkProspectorSnapshot = 759,
        [EnumMember]
        LivelinkTopic = 760,
        [EnumMember]
        LivelinkTask = 761,
        [EnumMember]
        LivelinkNews = 762,
        [EnumMember]
        LivelinkMilestone = 763,
        [EnumMember]
        LivelinkGeneration = 764,
        [EnumMember]
        LivelinkItem = 765,
        [EnumMember]
        LivelinkCADDocument = 766,

        #endregion

        #endregion
        #region PublicFolder Migration

        [EnumMember]
        PFPublicFolder = 800,//PublicFolder
        [EnumMember]
        PFUrnContentClassesFolder = 810,//urn:content-classes:folder
        [EnumMember]
        PFUrnContentClassesMailfolder = 811,//urn:content-classes:mailfolder
        [EnumMember]
        PFUrnContentClassesCalendarfolder = 812,//urn:content-classes:calendarfolder
        [EnumMember]
        PFUrnContentClassesContactfolder = 813,//urn:content-classes:contactfolder
        [EnumMember]
        PFUrnContentClassesTaskfolder = 814,//urn:content-classes:taskfolder
        [EnumMember]
        PFUrnContentClassesJournalfolder = 815,//urn:content-classes:journalfolder
        [EnumMember]
        PFUrnContentClassesNotefolder = 816,//urn:content-classes:notefolder
        [EnumMember]
        PFIPF = 820,//IPF
        [EnumMember]
        PFIPFNote = 821,//IPF.Note
        [EnumMember]
        PFIPFAppointment = 822,//IPF.Appointment
        [EnumMember]
        PFIPFContact = 823,//IPF.Contact
        [EnumMember]
        PFIPFTask = 824,//IPF.Task
        [EnumMember]
        PFIPFJournal = 825,//IPF.Journal
        [EnumMember]
        PFIPFStickyNote = 826,//IPF.StickyNote
        [EnumMember]
        PFIPFNoteInfoPathForm = 827,//IPF.Note.InfoPathForm

        #endregion

        [EnumMember]//ADO-82850 for GA+
        AdminCenter = 830,

        /// <summary>
        /// ADO-103189 for Archiver Newsfeed
        /// </summary>
        [EnumMember]
        NewsfeedPost = 831,
        [EnumMember]
        NewsfeedReply = 832,

        #region webapplication
        [EnumMember]
        SharePointSitesGroup = 833,
        [EnumMember]
        OneDriveSitesGroup = 834,
        [EnumMember]
        TeamSitesGroup = 838,
        #endregion
        #region sitecollection
        [EnumMember]
        SharePointSites = 835,
        [EnumMember]
        OneDriveSites = 836,
        [EnumMember]
        TeamSites = 839,
        #endregion
        #region DPM SharedService
        [EnumMember]
        DPMSharedService = 837,
        #endregion
    }

    [DataContract]
    public enum IncludeNewState
    {
        [EnumMember]
        Undefined = -1,

        [EnumMember]
        Checked = 1,

        [EnumMember]
        Unchecked = 0
    }

    [DataContract]
    public enum SelectAllState
    {
        [EnumMember]
        Undefined = -1,

        [EnumMember]
        Checked = 1,

        [EnumMember]
        Unchecked = 0
    }

    [DataContract]
    public enum PageNodeType
    {
        [EnumMember]
        Normal = 0,

        [EnumMember]
        PreNext = 1
    }
}
