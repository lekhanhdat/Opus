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
using AvePoint.RA.DB.AzureCosmosDB.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.AzureCosmosDB
{
    public class RMAzureCosmosDBRetryer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMAzureCosmosDBRetryer));
        
        private static readonly HttpStatusCode OptimisticLockConflictStatusCode = HttpStatusCode.PreconditionFailed;

        private static readonly HashSet<HttpStatusCode> CanRetriedStatusCodeSet = new()
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.Gone,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.ServiceUnavailable,
            (HttpStatusCode)449 // 449 ? Transient error that only occurs on write operations.
        };

        private static readonly HashSet<HttpStatusCode> UseRandomBackoffStatusCodeSet = new()
        {
            (HttpStatusCode)449
        };

        private static readonly HashSet<HttpStatusCode> UseExponentialBackoffStatusCodeSet = new()
        {
            HttpStatusCode.TooManyRequests
        };

        private static readonly HashSet<HttpStatusCode> UseIncrementalBackoffStatusCodeSet = new()
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.Gone,
            HttpStatusCode.ServiceUnavailable,
        };

        private const int MAX_DELAY_TIME = 60 * 1000;

        private readonly int RetryTimes;

        private readonly int InitialDelayTime;

        public RMAzureCosmosDBRetryer(int retryTimes, int initialDelayTime)
        {
            RetryTimes = retryTimes;
            InitialDelayTime = initialDelayTime;
        }

        public async Task<RMAzureCosmosDBRetryerResult> RetryAsync(Func<ValueTask> func)
        {
            var result = new RMAzureCosmosDBRetryerResult
            {
                RetriedTimes = 0,
                MaxRetryTimes = RetryTimes,
                IsSucceed = true,
                IsOptimisticLockConflict = false,
                CanContinueRetry = false,
            };

            var exceptions = new List<Exception>();

            do
            {
                try
                {
                    await func();
                    break;
                }
                catch (CosmosException e)
                {
                    exceptions.Add(e);

                    if (e.StatusCode == OptimisticLockConflictStatusCode)
                    {
                        result.IsSucceed = false;
                        result.IsOptimisticLockConflict = true;
                        result.CanContinueRetry = false;
                        break;
                    }

                    if (!CanRetriedStatusCodeSet.Contains(e.StatusCode))
                    {
                        result.IsSucceed = false;
                        result.IsOptimisticLockConflict = false;
                        result.CanContinueRetry = false;
                        break;
                    }

                    var delayTime = InitialDelayTime;

                    if (UseRandomBackoffStatusCodeSet.Contains(e.StatusCode))
                    {
                        var random = new Random();
                        var minTime = Convert.ToInt32(Math.Pow(result.RetriedTimes + 1, 2) * InitialDelayTime);
                        /* Fortify Issue Type: Insecure Randomness 
                        * Sink Details:  this position
                        * Ignore Reason: random用于task Delay
                        */
                        delayTime = random.Next(minTime, MAX_DELAY_TIME);
                    }

                    if (UseExponentialBackoffStatusCodeSet.Contains(e.StatusCode))
                    {
                        delayTime = Convert.ToInt32(Math.Pow(result.RetriedTimes + 1, 2) * InitialDelayTime);
                        if (e.RetryAfter.HasValue)
                        {
                            // Add 500ms to avoid being throttled again
                            delayTime = (int)e.RetryAfter.Value.TotalMilliseconds + 500;
                        }
                    }

                    if (UseIncrementalBackoffStatusCodeSet.Contains(e.StatusCode))
                    {
                        delayTime = (result.RetriedTimes + 1) * InitialDelayTime;
                    }
                    
                    _logger.Error($"Occured problem when do action: {e.StatusCode} retry  after {delayTime} seconds, retry count {result.RetriedTimes}");
                    
                    await Task.Delay(delayTime);
                    result.RetriedTimes++;
                }
                catch (Exception e)
                {
                    result.IsSucceed = false;
                    result.CanContinueRetry = false;
                    result.IsOptimisticLockConflict = false;
                    exceptions.Add(e);
                    break;
                }

            } while (result.RetriedTimes <= RetryTimes);

            if (result.RetriedTimes > RetryTimes)
            {
                result.IsSucceed = false;
            }
            
            if (exceptions.Any())
            {
                result.Exception = new Exceptions.RMAzureCosmosDBRetryerException(result.RetriedTimes, RetryTimes, result.IsOptimisticLockConflict, !result.CanContinueRetry, exceptions);
            }
            return result;
        }

    }
}
