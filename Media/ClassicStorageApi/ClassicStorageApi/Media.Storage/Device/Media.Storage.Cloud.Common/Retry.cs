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




namespace AvePoint.Media.ClassicStorage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon;
    using System.Threading;
    using System.Reflection;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
    #endregion

    public delegate T RetryDelegate<T>();
    public delegate void RetryDelegate();

    public class Retry
    {
        public int Count { get; set; }
        public int Interval { get; set; }
        public bool NeedRetry { get; set; }
        public bool FlushDns { get; set; }
        public AveLogger Logger { get; set; }

        public Retry(int count, int interval, bool needRetry, bool flushDns)
        {
            this.Count = count;
            this.Interval = interval;
            this.NeedRetry = needRetry;
            this.FlushDns = flushDns;
            this.Logger = AveLogger.GetInstance(typeof(Retry));
    }

        public T retry<T>(IRetryMethod<T> retryMethod)
        {
            if (NeedRetry)
            {
                int num = 0;
                while (num < Count)
                {
                    Logger.Info("Retry after " + Interval + " s. Retry count: " + num);

                    if (FlushDns)
                    {
                        Logger.Debug("Flush dns");
                        DnsUtil.FlushMyCache();
                        //Logger.Debug("end flush dns");
                    }
                    Thread.Sleep(Interval);

                    try
                    {
                        return retryMethod.retry();
                    }
                    catch (Exception e)
                    {
                        if (e is RetryableException)
                        {
                            num++;
                            Logger.Error("Exception when retry.", e);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            throw new Exception("Retry failed.");
        }

        public object ExcuteRetry(MethodInfo methodInfo, object obj, object[] args)
        {
            if (NeedRetry)
            {
                int num = 0;
                while (num < Count)
                {
                    Logger.Info("Retry after " + Interval + " ms. Retry count: " + num);
                    if (FlushDns)
                    {
                        Logger.Debug("begin flush dns");
                        DnsUtil.FlushMyCache();
                        Logger.Debug("end flush dns");
                    }
                    Thread.Sleep(Interval);
                    try
                    {
                        return methodInfo.Invoke(obj, args);
                    }
                    catch (Exception e)
                    {
                        num++;
                        Logger.Error("Exception when retry.", e);
                        if (!(e.InnerException is RetryableException))
                        {
                            throw ;
                        }
                        continue;
                    }
                }
            }
            throw new Exception("Retry method " + methodInfo.Name + " failed");
        }

        public T CloudRetry<T>(RetryDelegate<T> del)
        {
            return CloudRetry<T>(del, Count, Interval);
        }

        public T CloudRetry<T>(RetryDelegate<T> del, int numberOfRetries, int msPause)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    if (FlushDns)
                    {
                        Logger.Debug("begin flush dns");
                        DnsUtil.FlushMyCache();
                        Logger.Debug("end flush dns");
                    }
                    return del.Invoke();
                }
                catch (Exception ex)
                {
                    if (counter > numberOfRetries)
                    {
                        Logger.Error("CloudRetry occurs exception: " + ex.Message);
                        throw;
                    }
                    else
                    {
                        Logger.Warn(ex.Message, ex);
                        Logger.Info("Cloud Retry after " + Interval + " ms. Retry count: " + counter);
                        if (msPause > 0)
                        {
                            Thread.Sleep(msPause);
                        }
                    }
                }

            }
        }
    }
}
