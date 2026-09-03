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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.RA.Contract.Object.RealTime
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RecordsReturnMessage : ResultBase
    {
        [DataMember(EmitDefaultValue = false)]
        public ResultType ResultType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public ErrorType ErrorType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> FailedIds { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ResultType
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Skipped = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorType
    {
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// 找不到ProcessingPool
        /// </summary>
        [EnumMember]
        NoProcessingPool = 1,
        [EnumMember]
        NoSiteCollection = 2,
        [EnumMember]
        NoReportLocation = 3,
        [EnumMember]
        NoAvailableAgent = 4,
    }
}
