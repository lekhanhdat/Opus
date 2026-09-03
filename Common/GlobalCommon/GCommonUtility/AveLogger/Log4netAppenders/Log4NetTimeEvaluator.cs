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
namespace AvePoint.GCommon
{
    using System;
    using log4net.Core;

    /// <summary>
    /// An evaluator that triggers after specified number of seconds.
    /// </summary>
    public class Log4NetTimeEvaluator : ITriggeringEventEvaluator
    {


        /// <summary>
        /// The time threshold for triggering in seconds. Zero means it won't trigger at all.
        /// </summary>
        private int m_interval;

        /// <summary>
        /// The UTC time of last check. This gets updated when the object is created and when the evaluator triggers.
        /// </summary>
        private DateTime m_lastTimeUtc;

        /// <summary>
        /// The time threshold in seconds to trigger after
        /// </summary>
        /// <value>
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </value>
        public int Interval
        {
            get
            {
                return this.m_interval;
            }
            set
            {
                this.m_interval = value;
            }
        }

        /// <summary>
        /// Create a new evaluator using the time threshold in seconds.
        /// </summary>
        /// <remarks>
        public Log4NetTimeEvaluator() : this(0)
        {
        }

        /// <summary>
        /// Create a new evaluator using the specified time threshold in seconds.
        /// </summary>
        /// <param name="interval">
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </param>
        /// <remarks>
        public Log4NetTimeEvaluator(int interval)
        {
            this.m_interval = interval;
            this.m_lastTimeUtc = DateTime.UtcNow;
        }
        private readonly object locker = new object();
        /// <summary>
        /// Is this <paramref name="loggingEvent" /> the triggering event?
        /// </summary>
        /// <param name="loggingEvent">The event to check</param>
        /// <returns>This method returns <c>true</c>, if the specified time period 
        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }
            if (this.m_interval == 0)
            {
                return false;
            }
            bool result;
            lock (locker)
            {
                if (DateTime.UtcNow.Subtract(this.m_lastTimeUtc).TotalSeconds > (double)this.m_interval)
                {
                    this.m_lastTimeUtc = DateTime.UtcNow;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
    }

}
