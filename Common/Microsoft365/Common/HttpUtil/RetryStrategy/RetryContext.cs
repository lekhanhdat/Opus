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
namespace Microsoft365.Common.HttpUtil
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;

    /// <summary>
    /// context should be binding on handler, and policy will build condition based on it, then handler should follow the condition to perform action.
    /// </summary>
    public class RetryContext
    {
        public HttpResponseMessage Response { get; set; }
        public Exception Exception { get; set; }
        public DateTimeOffset RetryStartTime { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public ConcurrentDictionary<string, int> TypedRetryTimes { get; set; } = new ConcurrentDictionary<string, int>();

        public RetryContext()
        {
            
        }

        public void SetContextInfo(HttpResponseMessage message, Exception exception)
        {
            Response = message;
            Exception= exception;
            RetryCount++;
        }

    }
}
