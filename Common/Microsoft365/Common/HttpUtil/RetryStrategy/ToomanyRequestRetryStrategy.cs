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
    using Microsoft365.Common.SoapClient;
    using System;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// this strategy will retry for too many request http status code.
    /// </summary>
    public class ToomanyRequestRetryStrategy : AbstractRetryStrategy,IRetryStrategy
    {
        protected ToomanyRequestRetryOption Policy { get; set; }
        public ToomanyRequestRetryStrategy(ToomanyRequestRetryOption? policy = default):base()
        {
            if (policy.HasValue)
            {
                Policy = policy.Value;
            }
        }


        /// <summary>
        ///retry time is controlled by max retry after and default retry after.
        //if Retry-After has value and less than max retry after, use Retry-After
        //if Retry-After has value and greater than max retry after, use max retry after
        //if Retry-After has no value ,use default retry after.
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        protected override TimeSpan GetRetryAfterTime(RetryContext context)
        {
            int timeFromHeader = 0;
            if (context.Response != null && context.Response.Headers.Contains("Retry-After") && int.TryParse(context.Response.Headers.GetValues("Retry-After").FirstOrDefault(), out timeFromHeader))
            {
                return TimeSpan.FromSeconds(Math.Min(timeFromHeader, Convert.ToInt32(Policy.MaxAfterTime.TotalSeconds)));
            }
            return Policy.DefaultRetryAfter;

        }

        protected override bool RetryAllowed(RetryContext context)
        {
            bool shouldRetry = false;
            if (context.Response.StatusCode == HttpStatusCode.TooManyRequests && (context.RetryCount <= Policy.MaxRetries || (DateTime.UtcNow - context.RetryStartTime) <= Policy.MaxRetryTime))
            {
                shouldRetry = true;
            }
            return shouldRetry;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Policy);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            ToomanyRequestRetryStrategy toomanyRequestRetryStrategy = obj as ToomanyRequestRetryStrategy;
            return this.Policy.Equals(toomanyRequestRetryStrategy.Policy);
        }
    }
}
