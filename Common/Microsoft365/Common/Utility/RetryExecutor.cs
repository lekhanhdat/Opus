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
namespace Microsoft365.Common.Utility
{
    using Microsoft365.Common.Logger;
    using System;
    public static class RetryExecutor
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RetryExecutor));
        public static void Execute(Action action,string operationName, int maxRetryTimes=3,int retryInterval=1000)
        {
            int retryTimes = 0;
            do
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Try operation {operationName} failed.retryTimes:{retryTimes},maxRetryTimes:{maxRetryTimes},retryInterval:{retryInterval},Error:{ex?.Message}");

                }
            } while (retryTimes++ < maxRetryTimes);
        }

        public static T Execute<T>(Func<T> func, string operationName, int maxRetryTimes = 3, int retryInterval = 1000)
        {
            Exception exception = null;
            int retryTimes = 0;
            do
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    logger.Warn($"Try operation {operationName} failed.retryTimes:{retryTimes},maxRetryTimes:{maxRetryTimes},retryInterval:{retryInterval},Error:{ex?.Message}");
                    exception = ex;
                }
            } while (retryTimes++ < maxRetryTimes);

            if (exception != null)
            {
                throw exception;
            }
            return default(T);
        }
    }
}