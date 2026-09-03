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

namespace AvePoint.RA.Common.TransientFault
{
    public class FixedIntervalRetryStrategy : AveRetryStrategy
    {
        private readonly int retryCount;
        private readonly TimeSpan retryInterval;
        public FixedIntervalRetryStrategy()
            : this(AveRetryStrategy.DefaultClientRetryCount)
        {
        }
        public FixedIntervalRetryStrategy(int retryCount)
            : this(retryCount, AveRetryStrategy.DefaultRetryInterval)
        {
        }
        public FixedIntervalRetryStrategy(int retryCount, TimeSpan retryInterval)
            : this(null, retryCount, retryInterval, AveRetryStrategy.DefaultFirstFastRetry)
        {
        }
        public FixedIntervalRetryStrategy(string name, int retryCount, TimeSpan retryInterval)
            : this(name, retryCount, retryInterval, AveRetryStrategy.DefaultFirstFastRetry)
        {
        }
        public FixedIntervalRetryStrategy(string name, int retryCount, TimeSpan retryInterval, bool firstFastRetry)
            : base(name, firstFastRetry)
        {
            this.retryCount = retryCount;
            this.retryInterval = retryInterval;
        }
        public override ShouldRetry GetShouldRetry()
        {
            if (this.retryCount == 0)
            {
                return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    interval = TimeSpan.Zero;
                    return false;
                };
            }
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
            {
                if (currentRetryCount < this.retryCount)
                {
                    interval = this.retryInterval;
                    return true;
                }
                interval = TimeSpan.Zero;
                return false;
            };
        }
    }
}