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





using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public class NodeInfo
    {
        /// <summary>
        /// 节点的级别
        /// </summary>
        public NodeLevel Level { get; set; }

        /// <summary>
        /// 节点的级别
        /// </summary>
        public PRNodeTypeId PRNodeTypeId { get; set; }

        /// <summary>
        /// 节点的类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 该级别节点是否在树上显示。
        /// </summary>
        public bool IsShowOnTree { get; set; }

        /// <summary>
        /// 节点的子节点是否可以展开。
        /// </summary>
        public bool CanChildrenBeLoaded { get; set; }

        /// <summary>
        /// 节点的Selector的配置情况。默认为没有Selector。
        /// </summary>
        public SelectorInfo Selector { get; set; }

        /// <summary>
        /// 节点的虚拟节点配置情况。默认没有任何虚拟节点。
        /// </summary>
        public Collection<VirtualNode> VirtualNodes { get; set; }

        public NodeInfo()
        {
            IsShowOnTree = true;
            CanChildrenBeLoaded = true;
            Type = DefaultValue.DEFAULT_VALUE_ASTERISK;
            VirtualNodes = new Collection<VirtualNode>(); //默认必须赋初值。
            Selector = SelectorInfo.DEFAULT;
            PRNodeTypeId = GCommon.Contract.Tree.Object.PRNodeTypeId.None;
        }
    }

    public class SelectorInfo
    {
        public static SelectorInfo DEFAULT = new SelectorInfo(SelectorType.None, false);

        public SelectorType Type { get; set; }
        public bool Disabled { get; set; }

        public SelectorInfo()
        {
            //没有需要初始化的属性。
        }

        public SelectorInfo(SelectorType type, bool disabled)
        {
            Type = type;
            Disabled = disabled;
        }
    }

    public class VirtualNode
    {
        public VirtualNode()
        {
            IconPath = string.Empty;
            Type = DefaultValue.DEFAULT_VALUE_ASTERISK;
            Level = DefaultValue.DEFAULT_VALUE_ZERO;
            Template = DefaultValue.DEFAULT_VALUE_ZERO;
            VirtualNodeType = DefaultValue.DEFAULT_VALUE_ZERO;
        }

        public VirtualNodeType VirtualNodeType { get; set; }
        public string IconPath { get; set; }
        public int Template { get; set; }
        public NodeLevel Level { get; set; }
        public string Type { get; set; }
    }

    public enum VirtualNodeType
    {
        UnSpecified = 0,
        SelectAll = 1,
        IncludeNew = 2,
        ManualInput = 3,
        SelectAllList = 4,
        SelectAllLibrary = 5
    }

    public enum SelectorType
    {
        None = 0,
        CheckBox = 1,
        Radio = 2,
    }

    public class DefaultValue
    {
        public const string DEFAULT_VALUE_ASTERISK = "*"; //表示所有的。
        public const bool DEFAULT_VALUE_FALSE = false;
        public const int DEFAULT_VALUE_ZERO = 0;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(IncludeNode))]
    [KnownType(typeof(ExcludeNode))]
    [XmlInclude(typeof(IncludeNode))]
    [XmlInclude(typeof(ExcludeNode))]
    [XmlRootAttribute("NodeFilterPolicy")]
    public class NodeFilterPolicy
    {
        [DataMember]
        [XmlAttribute("Level")]
        public NodeLevel Level { get; set; }
        [DataMember]
        [XmlAttribute("Type")]
        public string Type { get; set; }
        [DataMember]
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }
        [DataMember]
        [XmlAttribute("Hidden")]
        public string Hidden { get; set; }
        [DataMember]
        [XmlAttribute("Template")]
        public string Template { get; set; }


    }
        [DataContract(Namespace = ContractConstants.Namespace)]
        [XmlRootAttribute("IncludeNode")]
    public class IncludeNode : NodeFilterPolicy
    {
        public IncludeNode() 
        {

            Type = DefaultValue.DEFAULT_VALUE_ASTERISK;
            DisplayName = DefaultValue.DEFAULT_VALUE_ASTERISK;
            Hidden = DefaultValue.DEFAULT_VALUE_ASTERISK;
            Template = DefaultValue.DEFAULT_VALUE_ASTERISK;
        }
    }

        [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExcludeNode : NodeFilterPolicy
    {
        public ExcludeNode() 
        {

            Type = DefaultValue.DEFAULT_VALUE_ASTERISK;
            DisplayName = DefaultValue.DEFAULT_VALUE_ASTERISK;
            Hidden = DefaultValue.DEFAULT_VALUE_ASTERISK;
            Template = DefaultValue.DEFAULT_VALUE_ASTERISK;
        }
    }
}
