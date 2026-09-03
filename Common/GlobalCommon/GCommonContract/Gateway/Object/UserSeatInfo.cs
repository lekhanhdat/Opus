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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserSeatInfo
    {
        [DataMember]
        public string AccountName { get; set; }

        /// <summary>
        /// key   月份，例如2013-4-1 时间部分必须是0
        /// value key所在月份的user数量
        /// </summary>
        [DataMember]
        public Dictionary<DateTime, int> UserSeatSummary { get; set; }
        [DataMember]
        public SortedDictionary<DateTime, int> SiteCount { get; set; }
        [DataMember]
        public SortedDictionary<DateTime, int> MailBox { get; set; }
        [DataMember]
        public SortedDictionary<DateTime, int> OneDrive { get; set; }

        [DataMember]
        public int? LicenseType { get; set; }
        [DataMember]
        public DateTime? ExpirationTime { get; set; }

    }

    public class MonthlyUserSeatInfo
    {
        public string AccountName { get; set; }
        public DateTime Month { get; set; }
        public int UserCount { get; set; }
        public int SiteCount { get; set; }
        public int MailBox { get; set; }
        public int OneDriveCount { get; set; }
        public Nullable<int> LicenseType { get; set; }
        public Nullable<DateTime> ExpirationTime { get; set; } 

    }
}
