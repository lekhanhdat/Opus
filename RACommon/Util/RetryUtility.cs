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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class RetryUtility
    {
        private static RALogger mlogger = RALogger.GetInstance(typeof(RetryUtility));

        public RetryUtility()
        {
        }

        public static T RetryAlways<T>(Func<T> action, int retryTimes, int sleepTime = 5000)
        {
            return RetryWhen<T>(action, (e) => true, retryTimes, sleepTime);
        }

        public static T RetryWhen<T>(Func<T> action, Func<Exception, bool> condition, int retryTimes, int sleepTime = 5000)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    mlogger.Warn(e.ToString());
                    if (condition(e))
                    {
                        System.Threading.Thread.Sleep(sleepTime);
                        continue;
                    }
                    throw;
                }
            }
            return default(T);
        }

        public static async Task<T> RetryWhenAsync<T>(Func<Task<T>> action, Func<Exception, bool> condition, int retryTimes, int sleepTime = 5000)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    mlogger.Warn(e.ToString());
                    if (condition(e))
                    {
                        await Task.Delay(sleepTime);
                        continue;
                    }
                    throw;
                }
            }
            return default(T);
        }

        public static async Task RetryWhenAsync(Func<Task> action, Func<Exception, bool> condition, int retryTimes, int sleepTime = 5000)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    mlogger.Warn(e.ToString());
                    if (condition(e))
                    {
                        await Task.Delay(sleepTime);
                        continue;
                    }
                    throw;
                }
            }
        }
    }
}
