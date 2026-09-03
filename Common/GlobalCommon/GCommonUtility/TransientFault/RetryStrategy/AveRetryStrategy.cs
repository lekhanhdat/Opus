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
    public abstract class AveRetryStrategy
    {
        public static readonly int DefaultClientRetryCount = 5;
        public static readonly TimeSpan DefaultClientBackoff = TimeSpan.FromSeconds(10.0);
        public static readonly TimeSpan DefaultMaxBackoff = TimeSpan.FromSeconds(30.0);
        public static readonly TimeSpan DefaultMinBackoff = TimeSpan.FromSeconds(1.0);
        public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(1.0);
        public static readonly TimeSpan DefaultRetryIncrement = TimeSpan.FromSeconds(1.0);
        public static readonly bool DefaultFirstFastRetry = true;

        private static AveRetryStrategy noRetry = new FixedIntervalRetryStrategy(0, AveRetryStrategy.DefaultRetryInterval);
        private static AveRetryStrategy defaultFixed = new FixedIntervalRetryStrategy(AveRetryStrategy.DefaultClientRetryCount, AveRetryStrategy.DefaultRetryInterval);
        private static AveRetryStrategy defaultProgressive = new IncrementalRetryStrategy(AveRetryStrategy.DefaultClientRetryCount, AveRetryStrategy.DefaultRetryInterval, AveRetryStrategy.DefaultRetryIncrement);
        private static AveRetryStrategy defaultExponential = new ExponentialBackoffRetryStrategy(AveRetryStrategy.DefaultClientRetryCount, AveRetryStrategy.DefaultMinBackoff, AveRetryStrategy.DefaultMaxBackoff, AveRetryStrategy.DefaultClientBackoff);
        public static AveRetryStrategy NoRetry
        {
            get
            {
                return AveRetryStrategy.noRetry;
            }
        }
        public static AveRetryStrategy DefaultFixed
        {
            get
            {
                return AveRetryStrategy.defaultFixed;
            }
        }
        public static AveRetryStrategy DefaultProgressive
        {
            get
            {
                return AveRetryStrategy.defaultProgressive;
            }
        }
        public static AveRetryStrategy DefaultExponential
        {
            get
            {
                return AveRetryStrategy.defaultExponential;
            }
        }
        public bool FastFirstRetry
        {
            get;
            set;
        }
        public string Name
        {
            get;
            private set;
        }
        protected AveRetryStrategy(string name, bool firstFastRetry)
        {
            this.Name = name;
            this.FastFirstRetry = firstFastRetry;
        }
        public abstract ShouldRetry GetShouldRetry();
    }

    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);
}
