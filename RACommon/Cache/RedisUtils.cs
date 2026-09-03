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
using AvePoint.RA.Contract.Services;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Cache
{
    internal class RedisUtils
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(RedisUtils));

        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            var retry = Policy.Handle<TimeoutRejectedException>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) });
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(3), TimeoutStrategy.Pessimistic);
            var wrap = retry.WrapAsync(timeout);
            try
            {
                return await wrap.ExecuteAsync(func);
            }
            catch (RedisConnectionException e)
            {
                logger.Error($"Redis connection exception occcurred, {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred, {0} {1}", e.Message, e);
                throw;
            }
        }

        public static async Task ExecuteAsync(Func<Task> func)
        {
            var retry = Policy.Handle<TimeoutRejectedException>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) });
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(3), TimeoutStrategy.Pessimistic);
            var wrap = retry.WrapAsync(timeout);
            try
            {
                await wrap.ExecuteAsync(func);
            }
            catch (RedisConnectionException e)
            {
                logger.Error($"Redis connection exception occcurred, {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred, {0} {1}", e.Message, e);
                throw;
            }
        }
    }
}
