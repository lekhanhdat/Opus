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

namespace AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RequestResult
    {
        /// <summary>
        /// result 为空的时候为success
        /// </summary>
        [DataMember]
        public ResultType Result { get; set; }

        [DataMember]
        public bool HasCompleted { get; set; }

        [DataMember]
        public Exception Exception { get; set; }

        [DataMember]
        public ReportLocationState State { get; set; }

        [DataMember]
        public string LocationId { get; set; }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ReportLocationState
        { 
            /// <summary>
            /// unc path不可用
            /// </summary>
            [EnumMember]
            None = 0,

            /// <summary>
            /// unc path可用，但是找不到rpt文件
            /// </summary>
            [EnumMember]
            OnlyUNC = 1,

            /// <summary>
            /// 可以找到rpt文件
            /// </summary>
            [EnumMember]
            All = 2,
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ResultType
    { 
        [EnumMember]
        Successful = 0,
        [EnumMember]
        UNCPathError = 1,
        [EnumMember]
        UserNameOrPasswordError = 2,
    }
}
