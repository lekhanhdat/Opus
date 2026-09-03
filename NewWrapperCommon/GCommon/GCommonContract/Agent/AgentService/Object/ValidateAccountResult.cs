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

namespace AvePoint.GCommon.Contract.AgentService.Object
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateAccountResult
    {
        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public int ErrorCode { get; set; }
    }

    ///// <summary>
    ///// http://msdn.microsoft.com/en-us/library/18d8fbe8-a967-4f1c-ae50-99ca8e491d2d.aspx
    ///// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum ValidateAccountError
    //{
    //    [EnumMember]
    //    SUCCESS = 0x00000000,
    //    [EnumMember]
    //    ERROR_ACCOUNT_LOCKED_OUT = 0x00000775,
    //}
}
