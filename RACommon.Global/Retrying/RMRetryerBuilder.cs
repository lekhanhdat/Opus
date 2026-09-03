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
using System.Linq;
using System.Text;

namespace AvePoint.RA.Common.Retrying
{
    public class RMRetryerBuilder
    {
        public RMRetryPredicate<RMExceptionPredicate, RMRetryAttemptInfo> ExceptionRetryPredicates { get; private set; } = new RMRetryPredicate<RMExceptionPredicate, RMRetryAttemptInfo>();

        public IRMRetryWaitStrategy WaitStrategy { get; private set; }

        public IRMRetryStopStrategy StopStrategy { get; private set; }

        private RMRetryerBuilder() { }
        
        public static RMRetryerBuilder CreateBuilder()
        {
            return new RMRetryerBuilder();
        }

        public RMRetryerBuilder RetryIfExceptionOfType(Type exceptionType)
        {
            return RetryIfExceptionOfType(exceptionType, true);
        }

        public RMRetryerBuilder RetryIfExceptionOfType(Type exceptionType, bool isStrictMatch)
        {
            ExceptionRetryPredicates.Add(new RMExceptionPredicate(exceptionType, isStrictMatch));
            return this;
        }

        public RMRetryerBuilder WithWaitStrategy(IRMRetryWaitStrategy waitStrategy)
        {
            WaitStrategy = waitStrategy;
            return this;
        }

        public RMRetryerBuilder WithStopStrategy(IRMRetryStopStrategy stopStrategy)
        {
            StopStrategy = stopStrategy;
            return this;
        }

        public RMRetryer Build()
        {
            if(!ExceptionRetryPredicates.Predicates.Any())
            {
                RetryIfExceptionOfType(typeof(Exception), false);
            }

            if(WaitStrategy == null)
            {
                WaitStrategy = RMRetryWaitStrategy.IncrementWait();
            }

            if(StopStrategy == null)
            {
                StopStrategy = RMRetryStopStrategy.StopAfterAttempt();
            }

            return new RMRetryer(ExceptionRetryPredicates, WaitStrategy, StopStrategy, RMRetryBlockStrategy.ThreadSleepStrategy());
        }
    }
}
