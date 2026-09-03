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
using System.Collections.Concurrent;
using System.Threading;

namespace AvePoint.RA.Common.Global.Throttle
{
    public class CallLimiter
    {
       // private static readonly RALogger logger = RALogger.GetInstance(typeof(CallLimiter));
        private int _callLimitPerSecond;
        private string _name;
        private ConcurrentQueue<DateTime> _timeStampQueue;
        /// <summary>
        /// Please use <see cref="CallLimiterFactory"/> to create instance.
        /// </summary>
        /// <param name="callLimitPerSecond"></param>
        public CallLimiter(string name, int callLimitPerSecond = 10)
        {
            if (callLimitPerSecond < 1)
            {
                throw new ArgumentException($"callLimitPerSecond of queue[{name}] should be greater than 0");
            }
            _name = name;
            _callLimitPerSecond = callLimitPerSecond;
            _timeStampQueue = new ConcurrentQueue<DateTime>();
        }

        public void WaitCallLimitPerSecond()
        {
            //System.Threading.Monitor.Enter(_timeStampQueue);
            try
            {
                if (_timeStampQueue.Count >= _callLimitPerSecond)
                {
                    DateTime dt;
                    if (_timeStampQueue.TryPeek(out dt))
                    {
                        TimeSpan timeInterval = DateTime.UtcNow - dt;
                        if (timeInterval < TimeSpan.FromSeconds(1))
                        {
                            var delaySpan = TimeSpan.FromSeconds(1) - timeInterval;
                            ///logger.Warn($"Exceed the queue[{_name}] limitation, will delay current thread for {delaySpan.TotalMilliseconds} ms.");
                            Thread.Sleep(delaySpan);
                        }
                        _timeStampQueue.TryDequeue(out dt);
                    }
                }
                _timeStampQueue.Enqueue(DateTime.UtcNow);
            }
            catch(Exception e)
            {
                //logger.Warn($"An error occurred while waiting queue[{_name}]. error: {e.ToString()}");
            }
            //finally
            //{
            //    System.Threading.Monitor.Exit(_timeStampQueue);
            //}
        }

    }
}
