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

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    [XmlRoot("DMTreeSchema")]
    public class DMTreeSchema
    {
        [XmlArray(ElementName = "TreeSchemas")]
        public List<TreeSchemaObj> TreeSchemas { get; set; }
    }

    [DataContract]
    public class TreeSchemaObj
    {
        /// <summary>
        /// 当前结点的Node Level
        /// </summary>
        [XmlAttribute("noteType")]
        public NodeLevel NodeType { get; set; }

        /// <summary>
        /// 子结点的Node类型对象集合
        /// </summary>
        [XmlArray(ElementName = "ChildObjs")]
        public List<NodeObj> NodeObjs { get; set; }
    }

    [DataContract]
    public class NodeObj
    {
        /// <summary>
        /// 子结点的Node Level
        /// </summary>
        [XmlAttribute("childNodeType")]
        public NodeLevel ChildNodeType { get; set; }

        /// <summary>
        /// 是否为虚拟节点
        /// </summary>
        [XmlAttribute("isVNode")]
        public bool IsVNode { get; set; }

        /// <summary>
        /// 虚节点名称
        /// </summary>
        [XmlAttribute("nodeName")]
        public string NodeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(ElementName = "NodeTypeObjs")]
        public List<NodeTypeObj> NodeTypeObjs { get; set; }
    }

    [DataContract]
    public class NodeTypeObj
    {
        /// <summary>
        /// 主要用于区分List级别的类型。
        /// </summary>
        [XmlAttribute("nodeType")]
        public NodeType NodeType { get; set; }

        [XmlAttribute("flag")]
        public int Flag { get; set; }
    }
}
