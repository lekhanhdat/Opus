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
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Common
{
    public class CommonPerformanceTimerPool
    {
        static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SortedList<string, AvePerformanceTimerWrapper> timers = new SortedList<string, AvePerformanceTimerWrapper>(StringComparer.OrdinalIgnoreCase);
        private bool isDisable = false;

        public CommonPerformanceTimerPool(bool isDisable)
        {
            this.isDisable = isDisable;
        }

        public void Start(string fullName)
        {
            try
            {
                if (!isDisable)
                {
                    AvePerformanceTimer timer = GetTimer(fullName, true);
                    timer.Start();
                }
            }
            catch (Exception e) { logger.Warn(e.ToString()); }
        }

        public void Stop(string fullName)
        {
            try
            {
                if (!isDisable)
                {
                    AvePerformanceTimer timer = GetTimer(fullName, true);
                    timer.Stop();
                }
            }
            catch (Exception e) { logger.Warn(e.ToString()); }
        }

        public void Action(string fullName, bool isStart)
        {
            if (isStart)
            {
                Start(fullName);
            }
            else
            {
                Stop(fullName);
            }
        }

        public override string ToString()
        {
            string result = string.Empty;
            if (!isDisable)
            {
                lock (timers)
                {
                    if (timers.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine();
                        builder.AppendFormat("{0, 40}  {1, 10}  {2, 10}  {3, 10}  {4, 10}\r\n",
                            "Name", "Count", "Duration", "MinTime", "MaxTime");
                        foreach (KeyValuePair<string, AvePerformanceTimerWrapper> keyValue in timers)
                        {
                            builder.AppendFormat("{0, 40}  {1, 10}\r\n", keyValue.Key, keyValue.Value.ToString());
                        }
                        result = builder.ToString();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取Timer
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="autoCreated"></param>
        /// <returns></returns>
        private AvePerformanceTimer GetTimer(string fullName, bool autoCreated)
        {
            AvePerformanceTimer timer = null;
            AvePerformanceTimerWrapper timerWrapper = GetTimerWrapper(fullName, autoCreated);

            if (timerWrapper != null)
            {
                timer = timerWrapper.GetTimer(autoCreated);
            }

            return timer;
        }

        /// <summary>
        /// 获取TimerWrapper
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="autoCreated"></param>
        /// <returns></returns>
        private AvePerformanceTimerWrapper GetTimerWrapper(string fullName, bool autoCreated)
        {
            AvePerformanceTimerWrapper timer = null;

            if (!timers.TryGetValue(fullName, out timer))
            {
                if (autoCreated)
                {
                    timer = new AvePerformanceTimerWrapper();
                    timers[fullName] = timer;
                }
            }

            return timer;
        }
    }

    class AvePerformanceTimerWrapper
    {
        private Dictionary<int, AvePerformanceTimer> timers = new Dictionary<int, AvePerformanceTimer>();

        public AvePerformanceTimer GetTimer(bool autoCreated)
        {
            AvePerformanceTimer timer = null;
            int id = Thread.CurrentThread.ManagedThreadId;

            lock (timers)
            {
                if (timers.ContainsKey(id))
                {
                    timer = timers[id];
                }
                else if (autoCreated)
                {
                    timer = new AvePerformanceTimer();
                    timers[id] = timer;
                }
            }

            return timer;
        }

        public override string ToString()
        {
            string result = string.Empty;
            lock (timers)
            {
                if (timers.Count > 0)
                {
                    long totalCount = 0L;
                    long duration = 0L;
                    long minTime = -1L;
                    long maxTime = 0L;

                    foreach (AvePerformanceTimer timer in timers.Values)
                    {
                        totalCount += timer.Count;
                        duration = timer.Duration;
                        if (minTime == -1)
                        {
                            minTime = timer.MinTime;
                        }
                        else if (minTime > timer.MinTime)
                        {
                            minTime = timer.MinTime;
                        }

                        if (maxTime < timer.MaxTime)
                        {
                            maxTime = timer.MaxTime;
                        }
                    }


                    result = string.Format("{0, 10}  {1, 10}  {2, 10}  {3, 10}",
                        totalCount, duration, minTime, maxTime);
                }
            }

            return result;
        }
    }
}
