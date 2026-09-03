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

    public class FixedIntervalRetryStrategy : AbstractRetryStrategy
    {
        protected TimeSpan Interval { get; set; }
        protected int MaxRetries { get; set; }
        public FixedIntervalRetryStrategy(TimeSpan interval,int maxRetries)
        {
            Interval = interval;
            MaxRetries=maxRetries;
        }
        protected override bool RetryAllowed(RetryContext context)
        {
            return context.RetryCount < MaxRetries;
        }

        protected override TimeSpan GetRetryAfterTime(RetryContext context)
        {
            return Interval;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Interval, MaxRetries);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            FixedIntervalRetryStrategy fixedIntervalRetryStrategy = obj as FixedIntervalRetryStrategy;
            return this.Interval == fixedIntervalRetryStrategy.Interval && this.MaxRetries == fixedIntervalRetryStrategy.MaxRetries;
        }
    }
}
