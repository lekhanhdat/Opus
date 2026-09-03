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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchLocationDto : EDiscoveryRequest
    {
        /// <summary>
        /// 需要探测的UNCPath地址
        /// </summary>
        [DataMember]
        public string UNCPath { get; set; }

        /// <summary>
        /// 域用户名.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// 用户密码.
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// 执行的请求动作.
        /// </summary>
        [DataMember]
        public LocationAction Action { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LocationAction : uint
    {
        [EnumMember]
        TestLocation = 1 //测试Location的有效性.
    }
}
