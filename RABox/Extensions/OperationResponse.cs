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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RABox.Extensions
{
    public class OperationResponse
    {
        private RAMessageType result = RAMessageType.Successful;

        public RAMessageType Result
        {
            get
            {
                return result;
            }
            set
            {
                if (value == RAMessageType.Failed)
                    ErrorTime = DateTime.UtcNow.Ticks;

                result = value;
            }
        }

        public OperationResponse()
        { }

        public long ErrorTime { get; set; }

        public string Message { get; set; }

        public string MessageKey { get; set; }

        public string[] MessageArgs { get; set; }

        public virtual object GetData() { return null; }
    }

    public class OperationResponse<TItem> : OperationResponse
    {
        public OperationResponse(TItem item)
        {
            DataObject = item;
        }

        public TItem DataObject { get; set; }
        public override object GetData() { return DataObject; }
    }

    public class FailedOperationResponse<TItem> : OperationResponse<TItem>
    {
        public FailedOperationResponse(TItem item) : base(item)
        {
            Result = RAMessageType.Failed;
            ErrorTime = DateTime.UtcNow.Ticks;
        }
    }
}
