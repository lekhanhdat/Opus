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

namespace Microsoft365.Common.RequestMonitor
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;


    public class Microsoft365RequestMonitorService : IDisposable
    {
        public Microsoft365RequestMonitorMode Mode { get; private set; }
        public DateTime StartTime { get; private set; }
        private ConcurrentDictionary<string, int> tokenNumber = new ConcurrentDictionary<string, int>();
        private long requestNumber = 0;
        private long errorResponseNumber = 0;
        private long throttledResponseNumber = 0;
        private TimerRange ThrottlingBlockedTimeRange;
        private readonly object throttlingBlockedTimeRangeLocker = new object();
        private readonly object operationLocker = new object();
        private long PSIRequestNumber = 0;
        private TimerRange PSIRequestTimeRange;
        private readonly object PSIRequestTimeRangeLocker = new object();

        private static Microsoft365RequestMonitorService mInstance = new Microsoft365RequestMonitorService();
        public static Microsoft365RequestMonitorService Instance
        {
            get
            {
                return mInstance;
            }
        }

        private Microsoft365RequestMonitorService()
        {
            Enable();
        }

        public void Enable()
        {
            lock (operationLocker)
            {
                Mode = Microsoft365RequestMonitorMode.Enabled;
                StartTime = DateTime.UtcNow;
                requestNumber = 0;
                errorResponseNumber = 0;
                throttledResponseNumber = 0;
                ThrottlingBlockedTimeRange = new TimerRange();
                PSIRequestNumber = 0;
                PSIRequestTimeRange = new TimerRange();
            }
        }

        public void Disable()
        {
            lock (operationLocker)
            {
                Mode = Microsoft365RequestMonitorMode.Disabled;
            }
        }
        public void AddThrottlingBlockedTimeRange(DateTime startUtc, int milliseconds)
        {
            AddThrottlingBlockedTimeRange(startUtc, startUtc.AddMilliseconds(milliseconds));
        }
        public void AddThrottlingBlockedTimeRange(DateTime startUtc, DateTime endUtc)
        {
            lock (throttlingBlockedTimeRangeLocker)
            {
                ThrottlingBlockedTimeRange.AddRange(new RangeItem(startUtc, endUtc));
            }
        }

        public void AddPSIRequestTimeRange(DateTime startUtc, DateTime endUtc)
        {
            lock (PSIRequestTimeRangeLocker)
            {
                PSIRequestTimeRange.AddRange(new RangeItem(startUtc, endUtc));
            }
        }

        protected TimeSpan GetThrottlingBlockedTotalTime()
        {
            lock (throttlingBlockedTimeRangeLocker)
            {
                return ThrottlingBlockedTimeRange.GetTotalTime();
            }
        }

        /// <summary>
        /// Returns true if the current UTC time falls within any active throttling range.
        /// Used by RateLimitHandler to decide whether to delay outgoing requests.
        /// </summary>
        public bool IsCurrentlyThrottled()
        {
            lock (throttlingBlockedTimeRangeLocker)
            {
                var now = DateTime.UtcNow;
                foreach (var range in ThrottlingBlockedTimeRange.GetDetails())
                {
                    if (now >= range.Start && now < range.End)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Returns the latest throttle end time among all active ranges, or <see cref="DateTime.MinValue"/> if none.
        /// </summary>
        public DateTime GetThrottleUntil()
        {
            lock (throttlingBlockedTimeRangeLocker)
            {
                var now = DateTime.UtcNow;
                DateTime latest = DateTime.MinValue;
                foreach (var range in ThrottlingBlockedTimeRange.GetDetails())
                {
                    if (range.End > now && range.End > latest)
                    {
                        latest = range.End;
                    }
                }
                return latest;
            }
        }

        protected TimeSpan GetPSIRequestTotalTime()
        {
            lock (PSIRequestTimeRangeLocker)
            {
                return PSIRequestTimeRange.GetTotalTime();
            }
        }

        public Microsoft365RequestSummary GetSummary()
        {
            return new Microsoft365RequestSummary
            {
                RequestNumber = requestNumber,
                ErrorResponseNumber = errorResponseNumber,
                ThrottledResponseNumber = throttledResponseNumber,
                ThrottlingBlockedTime = GetThrottlingBlockedTotalTime(),
                TokenRequestNumber = tokenNumber.Values.ToList().Sum(),
                PSIRequestNumber = PSIRequestNumber,
                PSIRequestDuation = GetPSIRequestTotalTime()
            };
        }

        public void AddTokenAuditor(TokenMonitorItem item)
        {
            lock (operationLocker)
            {
                if (Mode == Microsoft365RequestMonitorMode.Disabled)
                {
                    return;
                }
                if (!tokenNumber.ContainsKey(item.IdentityType))
                {
                    tokenNumber.TryAdd(item.IdentityType, 0);
                }
                tokenNumber[item.IdentityType]++;
            }
        }

        /// <summary>
        /// todo add url and error message to track details, but not necessary for now 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="responseStateType"></param>
        public void AddRequest(ResponseStateType responseStateType)
        {
            lock (operationLocker)
            {
                if (Mode == Microsoft365RequestMonitorMode.Disabled)
                {
                    return;
                }
                switch (responseStateType)
                {
                    case ResponseStateType.OK:
                        Interlocked.Increment(ref requestNumber);
                        break;
                    case ResponseStateType.Failed:
                        Interlocked.Increment(ref requestNumber);
                        Interlocked.Increment(ref errorResponseNumber);
                        break;
                    case ResponseStateType.Throttled:
                        Interlocked.Increment(ref requestNumber);
                        Interlocked.Increment(ref throttledResponseNumber);
                        break;
                }
            }
        }

        public void AddPSIRequest(DateTime startTime, DateTime endTime)
        {
            lock (operationLocker)
            {
                if (Mode == Microsoft365RequestMonitorMode.Disabled)
                {
                    return;
                }
                Interlocked.Increment(ref PSIRequestNumber);
                AddPSIRequestTimeRange(startTime, endTime);
            }
        }

        public void Dispose()
        {
            lock (throttlingBlockedTimeRangeLocker)
            {
                if (ThrottlingBlockedTimeRange != null)
                {
                    ThrottlingBlockedTimeRange.Dispose();
                }
            }
            lock (PSIRequestTimeRangeLocker)
            {
                if (PSIRequestTimeRange != null)
                {
                    PSIRequestTimeRange.Dispose();
                }
            }
        }
    }
}