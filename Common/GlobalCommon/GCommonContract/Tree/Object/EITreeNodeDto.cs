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




namespace AvePoint.GCommon.Contract.Tree.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ExportAndImport;
    #endregion

    /// <summary>
    /// Upgrade import & export backup data tree node structure.
    /// </summary>
    [DataContract]
    [XmlRootAttribute("EITreeNode")]
    public class EITreeNodeDto : AveTreeNodeDto<EITreeNodeDto>
    {
        [DataMember]
        [XmlAttribute("FarmName")]
        public String FarmName { set; get; }

        [DataMember]
        [XmlAttribute("IndexDeviceId")]
        public String IndexDeviceId { set; get; }

        [DataMember]
        [XmlAttribute("PlatformType")]
        public PlatformType PlatformType { set; get; }

        [DataMember]
        [XmlAttribute("OperateType")]
        public EIOperateType OperateType { get; set; }

        [DataMember]
        [XmlAttribute("logicalDeviceIds")]
        public List<String> LogicalDeviceIds { get; set; }

        [DataMember]
        [XmlAttribute("deviceName")]
        public String DeviceName { get; set; }

        [DataMember]
        [XmlAttribute("lastImportedTime")]
        public Int64 LastImportedTime { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Index Device Id: {0}, ", this.IndexDeviceId);
            stringBuilder.AppendFormat("Operate Type: {0}, ", this.OperateType);
            stringBuilder.AppendFormat("Device Name: {0}", this.DeviceName);
            return stringBuilder.ToString();
        }
    }
}
