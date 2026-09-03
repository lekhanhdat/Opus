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
using System.Text;

namespace AvePoint.RA.Common.Retrying
{
    public interface IRMRetryWaitStrategy
    {
        int ComputeSleepTime(RMRetryAttemptInfo attemptInfo);
    }
    
    public class RMRetryWaitStrategy
    {

        private static readonly IRMRetryWaitStrategy NO_WAIT_STRATEGY = new RMRetryFixedWaitStrategy(0);

        private RMRetryWaitStrategy() { }

        public static IRMRetryWaitStrategy NoWait()
        {
            return NO_WAIT_STRATEGY;
        }

        public static IRMRetryWaitStrategy FixedWait()
        {
            return new RMRetryFixedWaitStrategy();
        }

        public static IRMRetryWaitStrategy FixedWait(int waitTime)
        {
            return new RMRetryFixedWaitStrategy(waitTime);
        }

        public static IRMRetryWaitStrategy FixedWait(TimeSpan waitTime)
        {
            return new RMRetryFixedWaitStrategy((int)waitTime.TotalMilliseconds);
        }

        public static IRMRetryWaitStrategy RandomWait()
        {
            return new RMRetryFixedWaitStrategy();
        }

        public static IRMRetryWaitStrategy RandomWait(int minimum, int maximum)
        {
            return new RMRetryRandomWaitStrategy(minimum, maximum);
        }

        public static IRMRetryWaitStrategy RandomWait(TimeSpan minimum, TimeSpan maximum)
        {
            return new RMRetryRandomWaitStrategy((int)minimum.TotalMilliseconds, (int)maximum.TotalMilliseconds);
        }

        public static IRMRetryWaitStrategy IncrementWait()
        {
            return new RMRetryIncrementWaitStrategy();
        }

        public static IRMRetryWaitStrategy IncrementWait(int initialWaitTime, int increment)
        {
            return new RMRetryIncrementWaitStrategy(initialWaitTime, increment);
        }

        public static IRMRetryWaitStrategy IncrementWait(TimeSpan initialWaitTime, TimeSpan increment)
        {
            return new RMRetryIncrementWaitStrategy((int)initialWaitTime.TotalMilliseconds, (int)increment.TotalMilliseconds);
        }
    }

    public class RMRetryIncrementWaitStrategy : IRMRetryWaitStrategy
    {

        public int InitialWaitTime { get; private set; }

        public int Increment { get; private set; }

        public RMRetryIncrementWaitStrategy() :
            this(1000, 500)
        { }

        public RMRetryIncrementWaitStrategy(int initialWaitTime, int increment)
        {
            InitialWaitTime = initialWaitTime;
            Increment = increment;
        }

        public int ComputeSleepTime(RMRetryAttemptInfo attemptInfo)
        {
            return InitialWaitTime + (Increment * attemptInfo.RetryTimes);
        }
    }

    public class RMRetryRandomWaitStrategy : IRMRetryWaitStrategy
    {
        private readonly Random Rad = new Random();

        public int Minimum { get; private set; }

        public int Maximum { get; private set; }

        public RMRetryRandomWaitStrategy()
            : this(200, 2000)
        { }

        public RMRetryRandomWaitStrategy(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public int ComputeSleepTime(RMRetryAttemptInfo attemptInfo)
        {
            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details:  AvePoint.RA.Common.Retrying RMRetryer 85 117 149 181
            * Ignore Reason: random用于ThreadSleep 
            */
            return (int)Math.Abs(Rad.NextDouble()) % (Maximum - Minimum) + Minimum;
        }
    }

    public class RMRetryFixedWaitStrategy : IRMRetryWaitStrategy
    {
        public int WaitTime { get; private set; }
        
        public RMRetryFixedWaitStrategy()
            : this(1000)
        { }

        public RMRetryFixedWaitStrategy(int waitTime)
        {
            WaitTime = waitTime;
        }

        public int ComputeSleepTime(RMRetryAttemptInfo attemptInfo)
        {
            return WaitTime;
        }
    }
}
