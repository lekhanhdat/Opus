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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Message
{

    public class COPLoginModel
    {
        public string ProductName { get; set; }
        public string AosApiUrl { get; set; }
        public string Signature { get; set; }
        public string AccessToken { get; set; }
    }
    public class COPMessage
    {
        public string TenantId { get; set; }
        public string Product { get; set; }
        public string TenantOwner { get; set; }
        public string RecordId { get; set; }
        public string Signature { get; set; }
        /// <summary>
        /// Delete Tenant Data Tool
        /// 1. Soft Delete  2. Hard Delete
        /// </summary>
        public int Type { get; set; }
        public override string ToString()
        {
            return $"tenant:{TenantId}, prod:{Product}, rec:{RecordId}, sign:{Signature}";
        }
    }

    public enum AccountDeleteType
    {
        SoftDelete = 1, 
        HardDelete = 2
    }



    public class COPReturnMessage
    {
        public string RecordId { get; set; }
        public string Product { get; set; }
        public COPTenantStatus Status { get; set; }
        public string Message { get; set; }
        public MessageType Type { get; set; }

    }

    public class DeleteDataMessage
    {
        public string Message { get; set; }
        public MessageType Type { get; set; }
    }


    public enum COPTenantStatus
    {
        HardDeleting = 3,
        HardFailed = 4,
        HardDeleted = 6,
        SoftDeleting = 8,
        SoftFailed = 9,
        SoftDeleted = 10,
    }
    public enum MessageType
    {
        Success = 1,
        Failed = 2,
    }
}
