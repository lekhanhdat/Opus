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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [KnownType(typeof(SecuritySearchTreeNodeDto))]
    [KnownType(typeof(SOTreeNode))]
    [KnownType(typeof(ArchiverIndexDeviceTreeNodeDto))]
    [KnownType(typeof(SOOrphanBLOBRetentionTreeNodeDto))]
    [KnownType(typeof(SOBlobProviderTreeNodeDto))]
    [DataContract]
    [XmlRootAttribute("SPTreeNode")]
    public class SPTreeNodeDto : AveTreeNodeDto<SPTreeNodeDto>
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute]
        public String SPObjectId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute]
        public Int32 ItemRowId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("farmName")]
        public String FarmName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("agentId")]
        public String AgentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("template")]
        public Int32 Template { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("hasSubFolder")]
        public Boolean HasSubFolder { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("isFba")]
        public Boolean IsFba { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("siteLockStatus")]
        public Int32 SiteLockStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("spVersion")]
        public Int32 SPVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SPType SPType { get; set; }

        //[DataMember]
        //[XmlElement("permissioinText")]
        //public String PermissionText { get; set; }

        //[DataMember]
        //[XmlElement("nodeID")]
        //public Int32 MemberID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("inheritingPermissions")]
        public Boolean InheritingPermissions { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("state")]
        public Int32 State { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("promotion")]
        public NodePromotion Promotion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("promote")]
        public String Promote { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("folderCreationg")]
        public Boolean FolderCreation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("owner")]
        public String Owner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("discription")]
        public String Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("loginName")]
        public String LoginName { get; set; }

        //Content Manager
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("cmFlag")]
        public Int32 CMFlag { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("languageID")]
        public String languageID { get; set; }
        //end Content Manager

        //[DataMember]
        //[XmlElement("icon")]
        //public String Icon { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PropertyState Property { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SecurityState Security { get; set; }

        /// <summary>
        /// 是否为Hidden List
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Boolean Hidden { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]//是不是orphanve onedrive , 未赋值时为null
        public bool? IsOrphenOneDrive { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String Url { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String FullUrl { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String SitePath { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]//仅用于Advanced Search功能 在SPTreeService中传递数据
        public FilterPolicyInfo PolicyInfo { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]//仅用于Advanced Search功能 在SPTreeService中传递数据
        public bool IsAdvancedSearchEnable { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("isInAdvancedSearchMode")]//是否是advanced search模式展开子节点
        public bool IsInAdvancedSearchMode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("teamName")]//专为API提供显示当前private channel site在哪个team下
        public string TeamName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("teamName")]//专为API提供显示当前private channel site在哪个team下
        public bool IsSOMode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("o365TenantId")]
        public string O365TenantId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("teamsId")]
        public string TeamsId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("predictionModeType")]
        public int PredictionModeType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Extension")]
        public string Extension { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("Size")]
        public long Size { get; set; }

        #region project



        #endregion


        public static void Main(String[] args)
        {
            IAveTreeNodeDto a = new SPTreeNodeDto();
        }

        public object Clone()
        {
            SPTreeNodeDto dto = new SPTreeNodeDto();

            dto.Level = this.Level;
            dto.CheckNumber = this.CheckNumber;
            dto.FullPath = this.FullPath;
            dto.Name = this.Name;
            dto.Title = this.Title;
            dto.SelectAll = this.SelectAll;
            dto.IncludeNew = this.IncludeNew;

            dto.Url = this.Url;
            dto.AgentId = this.AgentId;
            dto.CanChildrenBeLoaded = this.CanChildrenBeLoaded;
            dto.CheckState = this.CheckState;
            dto.ChildrenCount = this.ChildrenCount;
            dto.Description = this.Description;
            dto.DisplayName = this.DisplayName;
            dto.Expanded = this.Expanded;
            dto.FarmID = this.FarmID;
            dto.FarmName = this.FarmName;
            dto.Hidden = this.Hidden;
            dto.ID = this.ID;

            return dto;
        }

        public object DeepClone()
        {
            SPTreeNodeDto dto = this.Clone() as SPTreeNodeDto;
            foreach (SPTreeNodeDto subNodeDto in this.Children)
            {
                SPTreeNodeDto nodeDto = subNodeDto.DeepClone() as SPTreeNodeDto;
                nodeDto.Parent = dto;
                dto.Children.Add(nodeDto);
            }
            return dto;
        }

        public override bool Equals(object obj)
        {
            if (obj is SPTreeNodeDto)
            {
                var node = obj as SPTreeNodeDto;
                if ((this.Level == NodeLevel.Apps && node.Level == NodeLevel.Apps)
                    || (this.Level == NodeLevel.Lists && node.Level == NodeLevel.Lists)
                    || (this.Level == NodeLevel.Sites && node.Level == NodeLevel.Sites)
                    || (this.Level == NodeLevel.Folders && node.Level == NodeLevel.Folders)
                    || (this.Level == NodeLevel.WebApplication && node.Level == NodeLevel.WebApplication))
                {
                    return this.ID == node.ID && this.Name == node.Name;
                }
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract]
    [XmlRootAttribute("NodeExtraOption")]
    public class NodeExtraOption
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("key")]
        public string Key { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("value")]
        public string value { get; set; }
    }

    [DataContract]
    [XmlRootAttribute("NodeSearchInfo")]
    public class NodeSearchInfo
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("pattern")]
        public string Pattern { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("caseSensitive")]
        public bool CaseSensitive { get; set; }
    }

    [DataContract]
    public enum NodePromotion
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        Promote = 1,

        [EnumMember]
        Demote = 2
    }

    [DataContract]
    public enum PropertyState
    {
        [EnumMember]
        Unchecked = 0,

        [EnumMember]
        Checked = 1
    }

    [DataContract]
    public enum SecurityState
    {
        [EnumMember]
        Unchecked = 0,

        [EnumMember]
        Checked = 1
    }

    [DataContract]
    public enum SPType
    {
        [EnumMember]
        Moss,

        [EnumMember]
        BPOS
    }
}
