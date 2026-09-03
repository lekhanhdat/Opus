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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings
{

    /// <summary>
    /// 用于保存数据库序列化操作和Server分别与GUI与Client通信使用
    /// </summary>
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProperitySetting : IProfileContent
    {
        [DataMember]
        [XmlArray("CPMappingItems")]
        [XmlArrayItem("MappingItem")]
        public List<PropMappingItem> ContentPropertiesMappings { get; set; }

        [DataMember]
        [XmlArray("MPMappingItems")]
        [XmlArrayItem("MappingItem")]
        public List<PropMappingItem> MediaPropertiesMappings { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PropMappingItem
    {
        [DataMember]
        [XmlAttribute("include")]
        public bool IsIncluded { get; set; }

        [DataMember]
        [XmlAttribute("fsPropertiesTxt")]
        public string FsPropertiesTxt { get; set; }

        [DataMember]
        [XmlAttribute("fsProperties")]
        public string FsProperties { get; set; }

        [DataMember]
        [XmlAttribute("spProperties")]
        public string SPProperties { get; set; }

    }
}
