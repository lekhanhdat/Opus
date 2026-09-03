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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Server.Audit.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditorSampleDto
    {
        [DataMember]
        public AveAuditorAction ActionMultiInfo { get; set; }

        [DataMember]
        public string ObjectDetail { get; set; }

        [DataMember]
        public AveAuditStatus Status { get; set; }

        /// <summary>
        /// key:I18NId  value:I18N Params
        /// </summary>
        [DataMember]
        public Dictionary<string,string[]> Comment { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public DateTime Time { get; set; }

        [DataMember]
        public AuditorEntityExtension Extension { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot]
    public class StorageTableConfigurationSetting : ISystemSettingContent
    {
        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string AccountKey { get; set; }

        [DataMember]
        public string EndPoint { get; set; }

        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public StorageTableType StorageTableType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StorageTableType
    {
        [EnumMember]
        AccountInformation,
        [EnumMember]
        RealTimeDetails,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfigStorageTableResult
    {
        [EnumMember]
        Successful = 0,
        [EnumMember]
        Failed = 1,
    }
}
