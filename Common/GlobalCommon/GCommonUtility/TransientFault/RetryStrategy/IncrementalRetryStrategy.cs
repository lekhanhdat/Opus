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

namespace AvePoint.GCommon.Utility.TransientFault
{
    public class IncrementalRetryStrategy : AveRetryStrategy
    {
        private readonly int retryCount;
        private readonly TimeSpan initialInterval;
        private readonly TimeSpan increment;
        public IncrementalRetryStrategy()
            : this(AveRetryStrategy.DefaultClientRetryCount, AveRetryStrategy.DefaultRetryInterval, AveRetryStrategy.DefaultRetryIncrement)
        {
        }
        public IncrementalRetryStrategy(int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(null, retryCount, initialInterval, increment)
        {
        }
        public IncrementalRetryStrategy(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(name, retryCount, initialInterval, increment, AveRetryStrategy.DefaultFirstFastRetry)
        {
        }
        public IncrementalRetryStrategy(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment, bool firstFastRetry)
            : base(name, firstFastRetry)
        {
            this.retryCount = retryCount;
            this.initialInterval = initialInterval;
            this.increment = increment;
        }
        public override ShouldRetry GetShouldRetry()
        {
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                if (currentRetryCount < this.retryCount)
                {
                    retryInterval = TimeSpan.FromMilliseconds(this.initialInterval.TotalMilliseconds + this.increment.TotalMilliseconds * (double)currentRetryCount);
                    return true;
                }
                retryInterval = TimeSpan.Zero;
                return false;
            };
        }
    }
}
