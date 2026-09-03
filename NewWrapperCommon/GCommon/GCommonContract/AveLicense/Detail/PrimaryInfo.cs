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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.AveLicense.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("PrimaryInfo")]
    public class PrimaryInfo
    {
        [DataMember]
        [XmlAttribute("GUID")]
        public string GUID { get; set; }

        /// <summary>
        /// License 的版本号
        /// </summary>
        [DataMember]
        [XmlAttribute("Version")]
        public string Version { get; set; }

        /// <summary>
        /// License 的类型
        /// </summary>
        [DataMember]
        [XmlAttribute("LicenseType")]
        public LicenseType LicenseType { get; set; }

        /// <summary>
        /// License Global产品类型
        /// </summary>
        [DataMember]
        [XmlAttribute("ProductType")]
        public ProductType ProductType { get; set; }

        /// <summary>
        /// 服务器名称,服务器地址
        /// </summary>
        [DataMember]
        [XmlAttribute("HostsAndIPs")]
        public List<string> HostsAndIPs { get; set; }

        /// <summary>
        /// 客户公司的名称
        /// </summary>
        [DataMember]
        [XmlAttribute("CompanyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// 发票号
        /// </summary>
        [DataMember]
        [XmlAttribute("Invoice")]
        public string Invoice { get; set; }

        /// <summary>
        /// 客户账号
        /// </summary>
        [DataMember]
        [XmlAttribute("AccountNumber")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 是否使用Amazon VM环境
        /// </summary>
        [DataMember]
        [XmlAttribute("UseAmazonVM")]
        public bool UseAmazonVM { get; set; }
    }
}
