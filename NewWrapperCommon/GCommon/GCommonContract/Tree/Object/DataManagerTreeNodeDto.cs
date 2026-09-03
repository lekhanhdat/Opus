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
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.DataManager;
    using AvePoint.GCommon.Contract.AveLicense;
    #endregion

    /// <summary>
    /// DataManager use it to load tree
    /// </summary>
    [DataContract]
    [XmlRootAttribute("DataManagerTreeNodeDto")]
    public class DataManagerTreeNodeDto : AveTreeNodeDto<DataManagerTreeNodeDto>
    {
        [DataMember]
        [XmlAttribute("IndexDevice")]
        public LogicalDeviceDto IndexDevice { set; get; }

        [DataMember]
        [XmlAttribute("StoragePolicyIdList")]
        public List<string> StoragePolicyIdList { set; get; }

        [DataMember]
        [XmlAttribute("KeepTime")]
        public long KeepTime { set; get; }

        [DataMember]
        [XmlAttribute("StartTime")]
        public long StartTime { set; get; }

        [DataMember]
        [XmlAttribute("IsHold")]
        public bool IsHold { get; set; }

        [DataMember]
        [XmlAttribute("JobState")]
        public int JobState { get; set; }

        [DataMember]
        [XmlAttribute("DeviceCompression")]
        public string DeviceCompression { get; set; }

        [DataMember]
        [XmlAttribute("DeferCompressionTime")]
        public string DeferCompressionTime { get; set; }

        [DataMember]
        [XmlAttribute("IsIndex")]
        public string IsIndex { get; set; }

        [DataMember]
        [XmlAttribute("IsShred")]
        public string IsShred { get; set; }

        [DataMember]
        [XmlAttribute("DataSize")]
        public string DataSize { get; set; }

        [DataMember]
        [XmlAttribute("NodeType")]
        public DataManagerNodeType NodeType { get; set; }

        [DataMember]
        [XmlAttribute("ProductVersion")]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        [XmlAttribute("ProductType")]
        public ProductType ProductType { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Job Id: {0}, ", this.ID);
            return stringBuilder.ToString();
        }
    }
}
