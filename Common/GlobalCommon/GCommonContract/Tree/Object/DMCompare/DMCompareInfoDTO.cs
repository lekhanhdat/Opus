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

namespace AvePoint.GCommon.Contract.Tree.Object.Compare
{
    /// <summary>
    /// Design Manager Compare功能使用的扩展信息对象
    /// </summary>
    [DataContract]
    [XmlRootAttribute("DMCompareInfoDTO")]
    public class DMCompareInfoDTO
    {
        /// <summary>
        /// 对应SP对象的compare对应关系
        /// </summary>
        [DataMember]
        [XmlAttribute("Key")]
        public string Key { get; set; }
        /// <summary>
        /// 对应SP对象的compare时候比较的对应值
        /// </summary>
        [DataMember]
        [XmlAttribute("Value")]
        public string Value { get; set; }

        [DataMember]
        [XmlAttribute("DetailInfo")]
        [XmlIgnore]
        public DetailInfo DetailInfo { get; set; }

        /// <summary>
        /// 用于比较深层次子节点是否相同的属性
        /// </summary>
        [DataMember]
        [XmlAttribute("ChildNodeValue")]
        public string ChildNodeValue { get; set; }
    }

    [DataContract]
    [XmlRootAttribute("IISTemplateNode")]
    public class IISTemplateNode
    {
        /// <summary>
        /// 存储IIS Template节点的Id值
        /// </summary>
        [DataMember]
        [XmlAttribute("Id")]
        public string Id { get; set; }

        /// <summary>
        /// 存储IIS Template节点的
        /// </summary>
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 存储IIS Template节点的FullPath
        /// </summary>
        [DataMember]
        [XmlAttribute("FullPath")]
        public string FullPath { get; set; }
    }

    [DataContract]
    public enum NodeSelectType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MultipleSubSite = 1
    }
    [DataContract]
    public enum CompareDetailNodeType : int
    {
        [EnumMember]
        None = 0,
        /// <summary>
        /// compare值依赖于其它List或更高级别
        /// </summary>
        [EnumMember]
        RelatedList = 1,
        /// <summary>
        /// 只有兄弟节点全都一样才会比较显示
        /// </summary>
        [EnumMember]
        RelatedBrothers = 2,
        /// <summary>
        /// 标题节点
        /// </summary>
        [EnumMember]
        TitleNode = 3,
        /// <summary>
        /// 不只要显示哪些属性不一样，还需要显示出具体不一样的值（可以参照document和item的情况去理解）
        /// </summary>
        [EnumMember]
        Detail = 4,
    }
    [DataContract]
    public class DetailInfo
    {
        /// <summary>
        /// Detail的Title
        /// </summary>
        [DataMember]
        [XmlAttribute("Title")]
        public string Title { get; set; }

        /// <summary>
        /// Detail的Key
        /// </summary>
        [DataMember]
        [XmlAttribute("Key")]
        public string Key { get; set; }

        /// <summary>
        /// 真正compare的值
        /// </summary>
        [DataMember]
        [XmlAttribute("Value")]
        public string Value { get; set; }
        /// <summary>
        /// 节点的类型
        /// </summary>
        [DataMember]
        [XmlAttribute("NodeType")]
        public CompareDetailNodeType NodeType { get; set; }
        /// <summary>
        /// 父节点（不要轻易使用，一般情况并无必要）
        /// </summary>
        [DataMember]
        [XmlAttribute("Parent")]
        public DetailInfo Parent { get; set; }

        private List<DetailInfo> childrens = new List<DetailInfo>();
        /// <summary>
        /// 子节点
        /// </summary>
        [DataMember]
        [XmlAttribute("Childrens")]
        public List<DetailInfo> Childrens
        {
            get
            {
                return childrens;
            }
            set
            {
                childrens = value;
            }
        }
    }
}
