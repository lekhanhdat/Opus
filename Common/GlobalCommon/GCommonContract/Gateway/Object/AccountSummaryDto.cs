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


using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountSummaryDto
    {
        [DataMember]
        public int TotalAccountCount { get; set; }

        /// <summary>
        /// 状态为Enabled的账户数量
        /// </summary>
        [DataMember]
        public int ActiveAccountCount { get; set; }

        /// <summary>
        /// 状态不为Enabled的账户数量
        /// </summary>
        [DataMember]
        public int InactiveAccountCount { get; set; }

        /// <summary>
        ///当前时间 大于 过期时间 小于 当前时间 + 15d的账户
        /// </summary>
        [DataMember]
        public int ExpiringIn15DaysAccountCount { get; set; }

        /// <summary>
        /// LicenseType为Enterprise的账户数量
        /// </summary>
        [DataMember]
        public int PurchasedAccountCount { get; set; }

        public override string ToString()
        {
            return string.Format("AccountSummaryDto[Total {0}, Active {1}, Inactive {2}, Expiring {3}, Purchased{4}]",
                TotalAccountCount, ActiveAccountCount, InactiveAccountCount, ExpiringIn15DaysAccountCount, PurchasedAccountCount);
        }
    }
}
