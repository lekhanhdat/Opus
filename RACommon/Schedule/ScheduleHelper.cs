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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Schedule;
using System;
using AvePoint.RA.Common.SystemSetting;

namespace AvePoint.RA.Common.Schedule
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public class ScheduleHelper
    {
        //private static RALogger logger = RALogger.GetInstance(typeof(ScheduleHelper));

        /// <summary>
        /// 计算下一次起job的时间
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static DateTime CalculateNextTime(ScheduleInfo schedule)
        {
            if (isLongAfterTime(schedule.NextTime))
            {
                return schedule.NextTime;
            }
            DateTime nextTime = schedule.NextTime;
            if (schedule.EndType == EndType.NoEnd)
            {
                nextTime = CalculateNextTimeNoEnd(schedule, nextTime);
            }
            else if (schedule.EndType == EndType.EndByOccurrences)
            {
                nextTime = CalculateNextTimeEndByOccurrences(schedule, nextTime);
            }
            else if (schedule.EndType == EndType.EndByTime)
            {
                nextTime = CalculateNextTimeEndByTime(schedule, nextTime);
            }
            return nextTime;
        }

        #region 此区域方法是否可以用DateTime.MaxValue取代？
        public static DateTime getLongAfterTime()
        {
            DateTime cal = new DateTime(9999, 1, 1, 1, 0, 0, 0);
            return cal;
        }

        public static bool isLongAfterTime(DateTime date)
        {
            return date.Year > 3000;
        }

        public static bool isLongAfterTime(long ticks)
        {
            return new DateTime(ticks).Year == 9999;
        }

        #endregion

        public static DateTime ConvertTimeToUtc(DateTime dateTime,string timeZoneId)
        {
            TimeZoneInfo timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
        }

        public static bool IsTimeEarlierThanNow(DateTime dateTime, string timeZoneId)
        {
            DateTime startTime = dateTime;
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                startTime = ConvertTimeToUtc(dateTime, timeZoneId);
            }
            return DateTime.Compare(startTime, DateTime.UtcNow) < 0;
        }

        public static bool FirstTimeIsAfterSecondTime(DateTime firstTime,DateTime secondTime)
        {
            return DateTime.Compare(firstTime, secondTime) > 0;
        }

        private static DateTime CalculateNextTimeNoEnd(ScheduleInfo schedule, DateTime nextTime)
        {
            DateTime currentTime = DateTime.UtcNow;
            while (DateTime.Compare(nextTime, currentTime) < 0)
            {
                nextTime = AddNextTimeByIntervalType(schedule);
                schedule.NextTime = nextTime;
            }
            schedule.NextTime = nextTime;
            return nextTime;
        }

        private static DateTime CalculateNextTimeEndByOccurrences(ScheduleInfo schedule, DateTime nextTime)
        {
            DateTime currentTime = DateTime.UtcNow;
            while (DateTime.Compare(nextTime, currentTime) < 0)
            {
                nextTime = AddNextTimeByIntervalType(schedule);
                schedule.NextTime = nextTime;
                schedule.Occurrences++;
            }
            if (schedule.Occurrences >= schedule.OccurrencesTotal)
            {
                nextTime = getLongAfterTime();
            }
            schedule.NextTime = nextTime;
            return nextTime;
        }

        private static DateTime CalculateNextTimeEndByTime(ScheduleInfo schedule, DateTime nextTime)
        {
            DateTime currentTime = DateTime.UtcNow;
            DateTime endTime = DateTime.Parse(schedule.EndTime);
            //DateTime endTime = DateTimeUtil.ConvertTimeToUtcDate(DateTime.Parse(schedule.EndTime), schedule.TimeZoneId, schedule.IsDaylightSaving);
            
            //endTime = new DateTime(schedule.EndTime.Ticks, DateTimeKind.Utc);
            while (DateTime.Compare(nextTime, currentTime) < 0)
            {
                nextTime = AddNextTimeByIntervalType(schedule);
                schedule.NextTime = nextTime;
            }
            if (DateTime.Compare(nextTime, endTime) > 0)
            {
                nextTime = getLongAfterTime();
            }
            schedule.NextTime = nextTime;
            return nextTime;
        }

        private static DateTime AddNextTimeByIntervalType(ScheduleInfo schedule)
        {
            var nextTime = new DateTime(schedule.NextTime.Ticks, DateTimeKind.Utc);
            if (schedule.Interval == 0)
            {
                return getLongAfterTime();
            }

            if (schedule.IntervalType == IntervalType.Weekly)
            {
                nextTime = nextTime.AddDays(7 * schedule.Interval);
            }
            else if (schedule.IntervalType == IntervalType.Daily)
            {
                nextTime = nextTime.AddDays(schedule.Interval);
            }
            else if (schedule.IntervalType == IntervalType.Hourly)
            {
                nextTime = nextTime.AddHours(schedule.Interval);
            }
            else if (schedule.IntervalType == IntervalType.Monthly)
            {
                var nextMonth = nextTime.AddMonths(schedule.Interval);
                if(schedule.DayOfMonth >= 100)
                {
                    var firstDayOfMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1, nextMonth.Hour, nextMonth.Minute, nextMonth.Second);
                    var daysOffset = ((int)schedule.WeekType - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
                    var firstWeekday = firstDayOfMonth.AddDays(daysOffset);
                    nextTime = firstWeekday.AddDays((schedule.DayOfMonth - 100) * 7);
                }
                else
                {
                    var maxDay = GetMaxDayOfMonth(nextMonth.Year, nextMonth.Month);
                    var day = Math.Min(schedule.DayOfMonth, maxDay);
                    nextTime = new DateTime(nextMonth.Year, nextMonth.Month, day, nextMonth.Hour, nextMonth.Minute, nextMonth.Second);
                }
            }
            return nextTime;
        }

        private static int GetMaxDayOfMonth(int year, int month)
        {
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                case 2:
                    return IsLeapYear(year) ? 29 : 28;
                default:
                    throw new ArgumentException("Invaid");
            }
        }

        private static bool IsLeapYear(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
        }
    }
}
