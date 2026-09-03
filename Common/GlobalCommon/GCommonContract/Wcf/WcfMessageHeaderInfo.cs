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
using AvePoint.GCommon.Contract.AccountManager.Object;

namespace AvePoint.GCommon.Contract.Wcf
{
    [DataContract]
    public class WcfMessageHeaderInfo
    {
        [DataMember]
        public SecurityInfo SecurityInfo { get; set; }

        [DataMember]
        public SecurityTrimmingType SecurityTrimmingType { get; set; }

        [DataMember]
        public string AccountId { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        /// <summary>
        /// Account所在的Group
        /// </summary>
        [DataMember]
        public string GroupId { get; set; }
    }

    [DataContract(Name = "Auth")]
    public class WebApiMessageHeader
    {
        [DataMember(Name = "s")]
        public SecurityTrimmingType SecurityTrimmingType { get; set; }

        [DataMember(Name = "i")]
        public string AccountId { get; set; }

        [DataMember(Name = "n")]
        public string AccountName { get; set; }

        /// <summary>
        /// Account所在的Group
        /// </summary>
        [DataMember(Name = "g")]
        public string GroupId { get; set; }

        /// <summary>
        /// 上次请求的时间戳
        /// </summary>
        [DataMember(Name = "t")]
        public long TimeStamp { get; set; }
        [DataMember(Name= "d")]
        public long SessionTimeoutDuration { get; set; }
        /// <summary>
        /// 是否单用户登录
        /// </summary>
        [DataMember(Name = "f")]
        public bool ForceLogined { get; set; }
    }

    [DataContract]
    public class SecurityInfo
    {
        [DataMember]
        public string SecurityToken { get; set; }
    }
}
