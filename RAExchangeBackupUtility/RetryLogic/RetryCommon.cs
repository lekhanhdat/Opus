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


namespace ExchangeUtility
{
    using AvePoint.GCommon;
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Reflection;
    using System.Threading;

    public class RetryCommon
    {
        protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int RetryTime = 5;
        private readonly int WaitTime = 2;//min
        private readonly int MINUTES = 60 * 1000;
        private readonly int HALFMINUTES = 30 * 1000;
        private readonly WaitMode WaitMode = WaitMode.Normal;
        private readonly DelayMode DelayMode = DelayMode.None;

        public RetryCommon() { }

        public RetryCommon(int maxRetryTime, int waitTime, WaitMode waitMode)
        {
            this.RetryTime = maxRetryTime;
            this.WaitTime = waitTime;
            this.WaitMode = waitMode;
        }

        public RetryCommon(int maxRetryTime, int waitTime, WaitMode waitMode, DelayMode delayMode)
        {
            this.RetryTime = maxRetryTime;
            this.WaitTime = waitTime;
            this.WaitMode = waitMode;
            this.DelayMode = delayMode;
        }

        public TReturn Retry<TReturn>(Func<TReturn> func)
        {
            TReturn returnValue = default(TReturn);
            Retry(() => returnValue = func(), this.RetryTime, this.WaitTime, this.WaitMode, this.DelayMode);
            return returnValue;
        }

        public TReturn Retry<T, TReturn>(T arg, Func<T, TReturn> func)
        {
            TReturn returnValue = default(TReturn);
            Retry(() => returnValue = func(arg), this.RetryTime, this.WaitTime, this.WaitMode, this.DelayMode);
            return returnValue;
        }

        private void Retry(Func<object> func, int maxRetryTime, int waitTime, WaitMode waitMode, DelayMode delayMode)
        {
            int retryTime = 1;
            do
            {
                try
                {
                    if (retryTime == 1) DelayCalculator(delayMode);
                    func();
                    break;
                }
                catch (PSRemotingTransportException ex)
                {
                    logger.Warn("Try block throw a PSRemotingTransportException, retry time: {0}", retryTime);
                    if (retryTime > maxRetryTime) throw ex;
                }
                catch (RemoteException ex)
                {
                    logger.Warn("Try block throw a RemoteException, retry time: {0}", retryTime);
                    if (retryTime > maxRetryTime) throw ex;
                }
                catch (Exception ex)
                {
                    logger.Warn("Try block throw a Exception, retry time: {0}", retryTime);
                    if (retryTime > maxRetryTime) throw ex;
                }
                switch (waitMode)
                {
                    case WaitMode.Increase:
                        Thread.Sleep(retryTime * waitTime * MINUTES);
                        break;
                    case WaitMode.Normal:
                        Thread.Sleep(waitTime * MINUTES);
                        break;
                    case WaitMode.Random:
                        var tick = DateTime.Now.Ticks;
                        var ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                        /* Fortify Issue Type: Insecure Randomness 
                        * Sink Details:  this position
                        * Ignore Reason: random用于ThreadSleep 
                        */
                        Thread.Sleep(ran.Next(1, 9) * 1 * HALFMINUTES);
                        break;
                    case WaitMode.None:
                        Thread.Sleep(0 * waitTime * MINUTES);
                        break;
                    default: break;
                }
                retryTime++;
            } while (true);
        }

        private void DelayCalculator(DelayMode delayMode)
        {
            switch (delayMode)
            {
                case DelayMode.Normal:
                    Thread.Sleep(2 * MINUTES);
                    break;
                case DelayMode.Random:
                    var tick = DateTime.Now.Ticks;
                    var ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                    /* Fortify Issue Type: Insecure Randomness 
                        * Sink Details:  this position
                        * Ignore Reason: random用于ThreadSleep 
                        */
                    Thread.Sleep(ran.Next(0, 9) * 1 * HALFMINUTES);
                    break;
                case DelayMode.None:
                default:
                    break;
            }
        }
    }

    public enum WaitMode
    {
        None = 0,
        Normal = 1,
        Increase = 2,
        Random = 3,
    }

    public enum DelayMode
    {
        None = 0,
        Normal = 1,
        Random = 2
    }
}
