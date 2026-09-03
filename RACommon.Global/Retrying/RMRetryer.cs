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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Retrying
{
    public class RMRetryer
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMRetryer));

        public RMRetryPredicate<RMExceptionPredicate, RMRetryAttemptInfo> ExceptionRetryPredicates { get; private set; }

        public IRMRetryWaitStrategy WaitStrategy { get; private set; }

        public IRMRetryStopStrategy StopStrategy { get; private set; }

        public IRMRetryBlockStrategy BlockStrategy { get; private set; }

        public RMRetryer(
            RMRetryPredicate<RMExceptionPredicate, RMRetryAttemptInfo> exceptionPredicates,
            IRMRetryWaitStrategy waitStrategy,
            IRMRetryStopStrategy stopStrategy,
            IRMRetryBlockStrategy blockStrategy
            )
        {
            ExceptionRetryPredicates = exceptionPredicates;
            WaitStrategy = waitStrategy;
            StopStrategy = stopStrategy;
            BlockStrategy = blockStrategy;
        }

        public void Retry(Action action)
        {
            var startTime = DateTime.UtcNow;
            var exceptions = new List<Exception>();
            for(var retryTimes = 0; ;retryTimes++)
            {
                try
                {
                    action();
                    return;
                }
                catch(Exception e)
                {
                    exceptions.Add(e);
                    var attemptInfo = new RMRetryAttemptInfo(retryTimes, startTime, e);

                    if (!ExceptionRetryPredicates.Predicate(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    if (StopStrategy.ShouldStop(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    var waitTime = WaitStrategy.ComputeSleepTime(attemptInfo);
                    BlockStrategy.Block(waitTime);
                }
            }
        }

        public T Retry<T>(Func<T> action)
        {
            var startTime = DateTime.UtcNow;
            var exceptions = new List<Exception>();
            for (var retryTimes = 0; ; retryTimes++)
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);

                    var attemptInfo = new RMRetryAttemptInfo(retryTimes, startTime, e);

                    if (!ExceptionRetryPredicates.Predicate(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    if (StopStrategy.ShouldStop(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    var waitTime = WaitStrategy.ComputeSleepTime(attemptInfo);
                    BlockStrategy.Block(waitTime);
                }
            }
        }

        public async Task RetryAsync(Func<Task> action)
        {
            var startTime = DateTime.UtcNow;
            var exceptions = new List<Exception>();
            for (var retryTimes = 0; ; retryTimes++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    var attemptInfo = new RMRetryAttemptInfo(retryTimes, startTime, e);

                    if (!ExceptionRetryPredicates.Predicate(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    if (StopStrategy.ShouldStop(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    var waitTime = WaitStrategy.ComputeSleepTime(attemptInfo);
                    BlockStrategy.Block(waitTime);
                }
            }
        }

        public async Task<T> RetryAsync<T>(Func<Task<T>> action)
        {
            var startTime = DateTime.UtcNow;
            var exceptions = new List<Exception>();
            for (var retryTimes = 0; ; retryTimes++)
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    Logger.Error($"Retry [{retryTimes}] times. Error: {e}");
                    exceptions.Add(e);

                    var attemptInfo = new RMRetryAttemptInfo(retryTimes, startTime, e);

                    if (!ExceptionRetryPredicates.Predicate(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    if (StopStrategy.ShouldStop(attemptInfo))
                    {
                        throw new RMRetryException(retryTimes, exceptions);
                    }

                    var waitTime = WaitStrategy.ComputeSleepTime(attemptInfo);
                    BlockStrategy.Block(waitTime);
                }
            }
        }
    }
}
