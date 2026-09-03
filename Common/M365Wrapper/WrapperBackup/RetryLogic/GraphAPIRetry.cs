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
using System.Threading.Tasks;

using AvePoint.GCommon.GraphAPI;

using AvePoint.RA.CommonUtil;

namespace ExchangeUtility.Graph
{
    public class GraphAPIRetry : IRetryable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(GraphAPIRetry));

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
                catch (GraphAPIException gaex)
                {
                    logger.Warn("Try block throw a GraphAPIException, retry time: {0}, error: {1}", retryTime, gaex);
                    if (retryTime >= maxRetryTime) throw;
                    if (!gaex.WaitForNextRequest()) throw;
                    if (gaex.Tag.Equals("ListChannelMessages") && gaex.HttpStatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                    {
                        string query = b.ToString();
                        if (query.Contains("?$top=50&", StringComparison.OrdinalIgnoreCase))
                            query = query.Replace("$Top=50&", string.Empty, StringComparison.OrdinalIgnoreCase);
                        else if (query.Contains("?$top=50", StringComparison.OrdinalIgnoreCase))
                            query = query.Replace("?$Top=50", string.Empty, StringComparison.OrdinalIgnoreCase);
                        T2 newB = (T2)Convert.ChangeType(query, typeof(T2));
                        return Execute(a, newB);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsConnectonForciblyClosedExceptioin() || ex.IsTaskCanceledExceptioin() || ex.IsErrorRequestExceptioin()
                        || ex.Message.Contains("Request to the endpoint timed out", StringComparison.OrdinalIgnoreCase))//ex is AggregateException && 

                    {
                        logger.Warn("Try block throw a Exception, retry time: {0}, error: {1}", retryTime, ex);
                        if (retryTime >= maxRetryTime) throw;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else
                    {
                        throw;
                    }
                }
                retryTime++;
            } while (true);
        }

        public TResult Retry<TIn, TResult>(Func<Task<TResult>, Task, TResult> excuteSDKRequest, Func<TIn, Task<TResult>> doTask1 = null, Func<Task<TResult>> doTask2 = null, Func<TIn, Task> doTask3 = null, Func<Task> doTask4 = null, TIn requestBody = default(TIn))
        {
            var maxRetryTime = this.RetryTime;
            var retryTime = 0;
            do
            {
                try
                {
                    //以下item的顺序是按使用频率排列的，目的是减少总体的判断次数。
                    if (null != doTask2) return excuteSDKRequest(doTask2(), null);
                    if (null != doTask3)
                    {
                        excuteSDKRequest(null, doTask3(requestBody));
                        return default(TResult);//doTask4 请求使用频率极低，这里加 return 可以减少一次判断。
                    }
                    if (null != doTask1) return excuteSDKRequest(doTask1(requestBody), null);
                    if (null != doTask4) excuteSDKRequest(null, doTask4());
                    return default(TResult);
                }
                catch (GraphAPIException gaex)
                {
                    logger.Warn("Try block throw a GraphAPIException, retry time: {0}, error: {1}", retryTime, gaex);
                    if (retryTime >= maxRetryTime) throw;
                    if (!gaex.WaitForNextRequest()) throw;
                }
                retryTime++;
            } while (true);
        }
    }
}