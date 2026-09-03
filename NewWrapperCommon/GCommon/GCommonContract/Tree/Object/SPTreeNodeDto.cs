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



using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [KnownType("GetKnownTypes")]
    [DataContract]
    [XmlRootAttribute("SPTreeNode")]
    public class SPTreeNodeDto : AveTreeNodeDto<SPTreeNodeDto>
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute]
        public String SPObjectId { get; set; }

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

        public UInt32 SiteLockStatusValue
        {
            get { return (UInt32)this.SiteLockStatus; }
            set { this.SiteLockStatus = (Int32)value; }
        }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("spVersion")]
        public Int32 SPVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SPType SPType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IsOnlineSite IsOnlineSite { get; set; }

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

        [DataMember(EmitDefaultValue = false)]
        public String Url { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]//仅用于Advanced Search功能 在SPTreeService中传递数据
        public FilterPolicyInfo PolicyInfo { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]//仅用于Advanced Search功能 在SPTreeService中传递数据
        public bool IsAdvancedSearchEnable { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("isInAdvancedSearchMode")]//是否是advanced search模式展开子节点
        public bool IsInAdvancedSearchMode { get; set; }

        #region recyclebin
        /// <summary>
        /// Item type of recycle bin item
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("RecycleBinItemType")]
        public RecycleBinItemType RecycleBinItemType { get; set; }

        /// <summary>
        /// Delete date of recycle bin item 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DeleteDate")]
        public DateTime DeleteDate { get; set; }

        private string deleteDateStr = string.Empty;
        /// <summary>
        /// Delete date str of recycle bin item 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DeleteDateStr")]
        public string DeleteDateStr
        {
            get
            {
                if (DeleteDate != null)
                {
                    return DeleteDate.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt");
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.deleteDateStr = value;
            }
        }

        /// <summary>
        /// Original location of recycle bin item 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("OriginalLocation")]
        public string OriginalLocation { get; set; }

        /// <summary>
        /// Delete transaction id of recycle bin item 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("DeleteTransactionId")]
        public byte[] DeleteTransactionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("ListId")]
        public Guid ListId { get; set; }

        #endregion
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);//或者typeof(MiscInfoBase)
        }

        //public override int GetHashCode()
        //{
        //    return (Name == null ? 0 : Name.GetHashCode()) ^ (FullPath == null ? 0 : FullPath.GetHashCode()) ^ (SPObjectId == null ? 0 : SPObjectId.GetHashCode());
        //}

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is SPTreeNodeDto))
        //    {
        //        return false;
        //    }
        //    SPTreeNodeDto node = obj as SPTreeNodeDto;
        //    return Name == node.Name && FullPath == node.FullPath && SPObjectId == node.SPObjectId;
        //}
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
    [DataContract]
    public enum RecycleBinItemType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        File = 1,
        [EnumMember]
        FileVersion = 2,
        [EnumMember]
        ListItem = 3,
        [EnumMember]
        List = 4,
        [EnumMember]
        Folder = 5,
        [EnumMember]
        FolderWithLists = 6,
        [EnumMember]
        Attachment = 7,
        [EnumMember]
        ListItemVersion = 8,
        [EnumMember]
        CascadeParent = 9,
        [EnumMember]
        Web = 10,
        [EnumMember]
        App = 11,
    }
}
