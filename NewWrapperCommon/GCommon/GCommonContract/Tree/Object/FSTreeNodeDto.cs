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
using AvePoint.GCommon.Contract.SharePointBrowser;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    [XmlRootAttribute("FSTreeNode")]
    public class FSTreeNodeDto : AveTreeNodeDto<FSTreeNodeDto>
    {
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("agentGroupId")]
        public string AgentGroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("path")]
        public string Path { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("FSConectionPath")]
        public string FSConectionPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("domain")]
        public string Domain { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("username")]
        public string Username { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("encryptedPassword")]
        public string EncryptedPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("pathType")]
        public PathType PathType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("profileType")]
        public int ProfileType { get; set; }

        /// <summary>
        /// DPM用于存储Export Tree内容的属性
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("treeContent")]
        public string ExportTreeContent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SPType")]
        public SPType SPType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SPVersion")]
        public Int32 SPVersion { get; set; }
    }
}
