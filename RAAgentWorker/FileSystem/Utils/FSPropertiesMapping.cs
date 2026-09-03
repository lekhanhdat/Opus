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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.RA.FileSystem.Utils
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("FilePropertiesMapping")]
    public class FSPropertiesMapping
    {
        [DataMember]
        [XmlElement("CommonMapping")]
        public CommonMapping CommonMapping { get; set; }

        [DataMember]
        [XmlElement("PropertiesMapping")]
        public PropertiesMapping PropertiesMapping { get; set; }

        [DataMember]
        [XmlElement("PermissionMapping")]
        public PermissionMapping PermissionMapping { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CommonMapping
    {
        [DataMember]
        [XmlElement("LengthMapping")]
        public LengthItem LengthItem { get; set; }

        [DataMember]
        [XmlArray("IllegalReplaceMapping")]
        [XmlArrayItem("Item")]
        public List<IllegalCharReplaceMappingItem> IllegalCharReplaceMappings { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LengthItem
    {
        [DataMember]
        [XmlAttribute("cbMaxFolderName")]
        public bool IsCheckedMaxForlderName { get; set; }

        [DataMember]
        [XmlAttribute("MaxFolderNameLen")]
        public int MaxForlderNameLength { get; set; }

        [DataMember]
        [XmlAttribute("cbMaxFileName")]
        public bool IsCheckedMaxFileName { get; set; }

        [DataMember]
        [XmlAttribute("MaxFileNameLen")]
        public int MaxFileNameLength { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IllegalCharReplaceMappingItem
    {
        [DataMember]
        [XmlAttribute("IllegalChar")]
        public string IllegalChar { get; set; }

        [DataMember]
        [XmlAttribute("ReplaceChar")]
        public string ReplaceChar { get; set; }

        [DataMember]
        [XmlAttribute("type")]
        public int Type { get; set; }

        public override string ToString()
        {
            if (IllegalChar == "{")
            {
                return "left brace";
            }
            else if (IllegalChar == "}")
            {
                return "right brace";
            }
            else if (IllegalChar == ".")
            {
                return "dot";
            }
            return IllegalChar;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PropertiesMapping
    {
        [DataMember]
        [XmlArray("MoveMappingItems")]
        [XmlArrayItem("MappingItem")]
        public List<PropMappingItem> MovePropertiesMappings { get; set; }
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

        [DataMember]
        [XmlAttribute("spColumnType")]
        public string FieldType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionMapping
    {
        [DataMember]
        [XmlArray("PermissionMatch")]
        [XmlArrayItem("MatchInfo")]
        public List<PermissionItem> PermissionMappings { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionItem
    {
        [DataMember]
        [XmlAttribute("SysPermission")]
        public string FileSystemPermission { get; set; }

        [DataMember]
        [XmlAttribute("SPPermission")]
        public string SPPermissionId { get; set; }
    }
}
