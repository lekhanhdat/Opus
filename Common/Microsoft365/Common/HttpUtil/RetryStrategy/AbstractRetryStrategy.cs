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
    using System.Threading.Tasks;

    public abstract class AbstractRetryStrategy: IRetryStrategy
    {
        public virtual async Task<RetryCondition> DetermineCondition(RetryContext context)
        {
            var shouldRetry = RetryAllowed(context);
            var retryAfterTime = shouldRetry? GetRetryAfterTime(context): TimeSpan.Zero;

            if (shouldRetry)
            {
                context.TypedRetryTimes.AddOrUpdate(GetType().FullName, (key) => { return 1; }, (key, oldValue) => { return oldValue + 1; });
            }
            var customAction = GetCustomActionAfterRetry(context);
            return await Task.FromResult(new RetryCondition
            {
                RetryAllowed = shouldRetry,
                RetryAfterTime = retryAfterTime,
                RetryAfterCustomAction = customAction
            });
        }

        protected abstract bool RetryAllowed(RetryContext context);

        protected virtual TimeSpan GetRetryAfterTime(RetryContext context) => TimeSpan.Zero;
        protected virtual Action GetCustomActionAfterRetry(RetryContext context) => null;

    }
}
