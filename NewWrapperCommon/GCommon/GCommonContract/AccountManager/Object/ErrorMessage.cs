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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ErrorMessage
    {
        /// <summary>
        /// Account Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// 删除失败的原因
        /// </summary>
        [DataMember]
        public Result Reason { get; set; }

        /// <summary>
        /// Account Info
        /// </summary>
        [DataMember]
        public AccountMappingDto AccountDto { get; set; }

        /// <summary>
        /// AD Account 所属 Domain 的信息
        /// </summary>
        [DataMember]
        public DomainDto DomainDto { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        [DataMember]
        public string Message { get; set; }           
    }
}
