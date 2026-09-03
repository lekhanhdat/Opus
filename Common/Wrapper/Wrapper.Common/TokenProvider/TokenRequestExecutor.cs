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
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public class TokenRequestExecutor
    {
        private static int defaultRetryTimes = 3; //retry to execute query with 3 times if throttle
        private static int defaultSleepTime = 1000 * 3; // sleep 3 sec
        private static AvePoint.GCommon.AveLogger mLogger = AvePoint.GCommon.AveLogger.GetInstance(typeof(TokenRequestExecutor));

        public static void RetryAction(Action executeAction, int sleepTime= -1, int retryTimes = -1, Action<Exception> errorHandle = null)
        {
            sleepTime = sleepTime == -1 ? defaultSleepTime : sleepTime;
            retryTimes = retryTimes == -1 ? defaultRetryTimes : retryTimes;
            string errorMsg = "";
            System.Diagnostics.Stopwatch performanceTimer = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Stopwatch retryTimer = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                try
                {
                    retryTimer.Start();
                    executeAction();
                    performanceTimer.Stop();
                    retryTimer.Stop();
                    mLogger.Info($"Execute token request success, cost:{performanceTimer.Elapsed}");
                    return;
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                    if (errorHandle != null)
                    {
                        errorHandle(e);
                    }
                    if (!ShouldRetry(e)) { throw; }
                    
                    mLogger.Error($"An error occured when execute token request and will retry, cost:{retryTimer.Elapsed}, retryTimes:{retryTimes}, sleeptime:{sleepTime} error:{e.Message}");
                    retryTimer.Reset();
                    System.Threading.Thread.Sleep(sleepTime);
                }
            }
            while (--retryTimes > 0);
            throw new Exception(errorMsg);
        }

        private static bool ShouldRetry(Exception e)
        {
            if (e == null)
            {
                mLogger.Error("This exception is null, don't need to retry");
                return false;
            }
            if (e is TimeoutException)
            {
                return true;
            }
            else if (e is AveNullResultException)
            {
                return true;
            }
            else if (e is AveErrorException)
            {
                return true;
            }
            else if (e is AveChangeTokenExpireException)
            {
                return true;
            }
            else if (e is AveWrapperInvalidDataException)
            {
                return true;
            }
            return false;
        }
    }
}
