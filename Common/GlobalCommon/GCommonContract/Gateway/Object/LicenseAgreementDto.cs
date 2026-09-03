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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseAgreementDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 区分standard类型和customized类型
        /// </summary>
        [DataMember]
        public LicenseAgreementType Type { get; set; }
        /// <summary>
        /// 区分standard类型和customized类型
        /// </summary>
        [DataMember]
        public string Content { get; set; }
        /// <summary>
        /// License agreement名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// License agreement作用的国家或地区的枚举值
        /// </summary>
        [DataMember]
        public Country Country { get; set; }

        /// <summary>
        /// 方便CRM team通过该属性来统计用户信息。
        /// </summary>
        [DataMember]
        public string CRMCustomerID { get; set; }

        /// <summary>
        /// 如果是History License，则保存最新Version的License Id；如果不是History，则为NULL
        /// </summary>
        [DataMember]
        public string ParentId { get; set; }

        /// <summary>
        /// 保存License Agreement的更新时间
        /// </summary>
        [DataMember]
        public long UpdateTime { get; set; }

        public override string ToString()
        {
            return string.Format("LicenseAgreementDto[Id {0}, Name {1}, Type {2}, Country {3}, CRMCustomerID {4}, ParentId {5}, UpdateTime {6}]", Id, Name, Type.ToString(), Country.ToString(), CRMCustomerID, ParentId, UpdateTime);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseAgreementType
    {
        [Description("")]
        [EnumMember]
        None = 0,

        [Description("Standard")]
        [EnumMember]
        Standard = 1,

        [Description("Customized")]
        [EnumMember]
        Customized = 2,

        [Description("Trial")]
        [EnumMember]
        Trial = 3
    }
}
