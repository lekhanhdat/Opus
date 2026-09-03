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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object
{
    /// <summary>
    /// Client端使用的
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("SyncSetting")]
    public class SyncSetting
    {
        [DataMember]
        [XmlIgnore]
        public SPTreeNodeDto TopNode { get; set; }

        [DataMember]
        [XmlElement("FileMigrator")]
        public CommonSetting CmmonSetting { get; set; }

        [DataMember]
        [XmlElement("PropertiesMapping")]
        public ProperitySetting PropertiesSetting { get; set; }

        [DataMember]
        [XmlElement("PermissionMapping")]
        public SecuritySetting SecuritySetting { get; set; }

        [DataMember]
        [XmlArray("NewMossPermissions")]
        [XmlArrayItem("newPer")]
        public List<SPPermission> NewSPPermissions { get; set; }
    }
}
