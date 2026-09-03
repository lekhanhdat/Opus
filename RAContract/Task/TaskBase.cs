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

namespace AvePoint.RA.Contract.Task
{
    public abstract class TaskBase
    {
        public string Id { get; set; }

        public RMTaskStatus Status { get; set; }

        public TaskType Type { get; set; }
        public string ProfileId { get; set; }

        public long NextRunTime { get; set; }
        public bool DisallowConcurrentExecution { get; set; }
        /// <summary>
        /// task time out min
        /// </summary>
        public int Timeout { get; set; } = 5;

        /// <summary>
        /// Concurrency control property
        /// </summary>
        public byte[] RowVersion { get; set; }

        public TaskSchedule Schedule { get; set; }

        public bool IsTimeout
        {
            get
            {
                return new DateTime(this.NextRunTime, DateTimeKind.Utc).AddMinutes(Timeout) < DateTime.UtcNow;
            }
        }

        internal abstract TaskBase AssembleDefaultTask();
        internal string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public long CalculateNextRunTime(long nextRunTime = -1)
        {
            var time = nextRunTime > 0 ? nextRunTime : this.NextRunTime;
            var nextRunDate = new DateTime(time, DateTimeKind.Utc);
            var interval = this.Schedule.Interval;
            var intervalType = this.Schedule.IntervalType;
            switch (intervalType)
            {
                case TaskIntervalType.OnlyOnce:
                    nextRunDate = DateTime.MaxValue;
                    break;
                case TaskIntervalType.Seconds:
                    nextRunDate = nextRunDate.AddSeconds(interval);
                    break;
                case TaskIntervalType.Minutes:
                    nextRunDate = nextRunDate.AddMinutes(interval);
                    break;
                case TaskIntervalType.Hourly:
                    nextRunDate = nextRunDate.AddHours(interval);
                    break;
                case TaskIntervalType.Daily:
                    nextRunDate = nextRunDate.AddDays(interval);
                    break;
                case TaskIntervalType.Weekly:
                    nextRunDate = nextRunDate.AddDays(interval * 7);
                    break;
                case TaskIntervalType.Monthly:
                    nextRunDate = nextRunDate.AddMonths(interval);
                    break;
            }
            return nextRunDate.Ticks;
        }

    }
}
