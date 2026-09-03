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
namespace ExchangeUtility.Graph
{
    using AvePoint.RA.CommonUtil;

    using System;
    using System.Linq;

    public class YammerAPIRetry : IYammerRetryable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(YammerAPIRetry));
        private readonly int RetryTime = 5;
        public object Retry<T1, T2>(Func<T1, T2, object> Execute, T1 a, T2 b)
        {
            var maxRetryTime = this.RetryTime;
            var retryTime = 0;
            do
            {
                try
                {
                    return Execute(a, b);
                }
                catch (YammerAPIException yaex)
                {
                    logger.Warn("Try block throw a YammerAPIException, retry time: {0}, error: {1}", retryTime, yaex);
                    if (retryTime >= maxRetryTime) throw;
                    if (!yaex.WaitForNextRequest()) throw;
                }
                catch (TimeoutException toe)
                {
                    logger.Warn($"TimeoutException, no need to retry, error: {toe}");
                    throw;
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException && ex.IsConnectonForciblyClosedExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsTaskCanceledExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsErrorRequestExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsTimeOutExceptioin())
                    {
                        logger.Warn($"TimeoutException, no need to retry, error: {ex.InnerException}");
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
                retryTime++;
            } while (true);
        }

        public object Retry<T1, T2, T3>(Func<T1, T2, T3, object> Execute, T1 a, T2 b, T3 c)
        {
            var maxRetryTime = this.RetryTime;
            var retryTime = 0;
            do
            {
                try
                {
                    return Execute(a, b, c);
                }
                catch (YammerAPIException yaex)
                {
                    logger.Warn("Try block throw a YammerAPIException, retry time: {0}, error: {1}", retryTime, yaex);
                    if (retryTime >= maxRetryTime) throw;
                    if (!yaex.WaitForNextRequest()) throw;
                }
                catch (TimeoutException toe)
                {
                    logger.Warn($"TimeoutException, no need to retry, error: {toe}");
                    throw;
                }
                catch (AggregateException ex) when (ex.Flatten().InnerExceptions.FirstOrDefault(x => x is YammerAPIException) is YammerAPIException ymex)
                {
                    logger.Warn("Try block throw a YammerAPIException inner AggregateException, retry time: {0}, error: {1}", retryTime, ex);
                    if (retryTime >= maxRetryTime) throw;
                    if (!ymex.WaitForNextRequest()) throw;
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException && ex.IsConnectonForciblyClosedExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsTaskCanceledExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsErrorRequestExceptioin())
                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else if (ex is AggregateException && ex.IsTimeOutExceptioin())
                    {
                        logger.Warn($"TimeoutException, no need to retry, error: {ex.InnerException}");
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
                retryTime++;
            } while (true);
        }
    }
}