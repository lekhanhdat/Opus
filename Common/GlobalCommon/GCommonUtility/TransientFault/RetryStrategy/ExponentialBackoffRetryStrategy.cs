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
    public class ExponentialBackoffRetryStrategy : AveRetryStrategy
    {
        private readonly int retryCount;
        private readonly TimeSpan minBackoff;
        private readonly TimeSpan maxBackoff;
        private readonly TimeSpan deltaBackoff;
        public ExponentialBackoffRetryStrategy()
            : this(AveRetryStrategy.DefaultClientRetryCount, AveRetryStrategy.DefaultMinBackoff, AveRetryStrategy.DefaultMaxBackoff, AveRetryStrategy.DefaultClientBackoff)
        {
        }
        public ExponentialBackoffRetryStrategy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : this(null, retryCount, minBackoff, maxBackoff, deltaBackoff, AveRetryStrategy.DefaultFirstFastRetry)
        {
        }
        public ExponentialBackoffRetryStrategy(string name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : this(name, retryCount, minBackoff, maxBackoff, deltaBackoff, AveRetryStrategy.DefaultFirstFastRetry)
        {
        }
        public ExponentialBackoffRetryStrategy(string name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, bool firstFastRetry)
            : base(name, firstFastRetry)
        {
            this.retryCount = retryCount;
            this.minBackoff = minBackoff;
            this.maxBackoff = maxBackoff;
            this.deltaBackoff = deltaBackoff;
        }
        public override ShouldRetry GetShouldRetry()
        {
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                if (currentRetryCount < this.retryCount)
                {
                    Random random = new Random();
                    /* Fortify Issue Type: Insecure Randomness 
                    * Sink Details: this method
                    * Ignore Reason: random用于设置重试间隔 
                    */
                    int num = (int)((Math.Pow(2.0, (double)currentRetryCount) - 1.0) * (double)random.Next((int)(this.deltaBackoff.TotalMilliseconds * 0.8), (int)(this.deltaBackoff.TotalMilliseconds * 1.2)));
                    int num2 = (int)Math.Min(this.minBackoff.TotalMilliseconds + (double)num, this.maxBackoff.TotalMilliseconds);
                    retryInterval = TimeSpan.FromMilliseconds((double)num2);
                    return true;
                }
                retryInterval = TimeSpan.Zero;
                return false;
            };
        }
    }
}
