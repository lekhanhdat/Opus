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

namespace ExchangeUtility
{
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Threading;

    [Serializable]
    class EWSRetryException : Exception
    {
        protected EWSRetryException() { }
        protected EWSRetryException(string message) : base(message) { }
        public EWSRetryException(string message, Exception inner) : base(message, inner)
        {
            this.BackOffMilliseconds = GetWaitTime(inner);
        }
        protected EWSRetryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }


        /// <summary>
        /// WaitTimeInMilliseconds > 0, recommend to retry in [WaitTimeInMilliseconds] ms
        /// WaitTimeInMilliseconds = 0, recommend to retry immediately
        /// WaitTimeInMilliseconds < 0, logic error, no need to retry
        /// </summary>
        public int BackOffMilliseconds { get; protected set; }

        /// <summary>
        /// 对于RetryException, 等待一段时间再执行下次Request操作
        /// </summary>
        /// <returns>True:执行下次Retry, False:不需要进行Retry</returns>
        public bool WaitForNextRequest()
        {
            return ServiceResponseExceptionExtension.WaitForNextRequest(this.BackOffMilliseconds);
        }            

        protected virtual int GetWaitTime(Exception ex)
        {
            if (ex == null) return ServiceErrorExtension.DefaultBackOffMilliseconds;
            var sbEx = ex as ServerBusyException;
            if (sbEx != null) return sbEx.BackOffMilliseconds;
            var srEx = ex as ServiceResponseException;
            if (srEx != null) return srEx.ErrorCode.BackOffMilliseconds();

            return ServiceErrorExtension.DefaultBackOffMilliseconds;
        }
    }
}
