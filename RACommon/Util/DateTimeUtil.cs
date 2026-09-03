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

using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Utility.TimeZoneConvert;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Data.Aos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvePoint.RA.Common.Util
{
    public static class DateTimeUtil
    {
        public const long ALMOST_AN_HOUR = TimeSpan.TicksPerHour - 1;
        private const string DEFAULT_TIME_FORMAT = "MM/dd/yyyy HH:mm";
        public const string TIMESTAMP_YYYYMMDDHHMMSS = "yyyyMMddHHmmss";
        public const string DATETYPEForRuleFilter = "yyyy/MM/dd HH:mm:ss";

        public static System.Globalization.CultureInfo Culture;
        public static string DATETYPEForAPI003
        {
            get
            {
                return "yyyy-MM-dd HH:mm";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="utcTicks">Utc</param>
        /// <param name="timeZone">源时区</param>
        /// <param name="useDst">使用夏令时取反值:使用为:false</param>
        /// <returns></returns>
        public static DateTime ConvertTimeFromUtc(DateTime utcTime, TimeZoneInfo timeZone, bool useDst)// = true)
        {
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            // 时间为夏令时时间 且指定不使用夏令时 减一小时
            if (useDst && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(time))
            {
                time = time.AddHours(-1);
            }
            return time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="utcTicks">Utc</param>
        /// <param name="timeZone">源时区</param>
        /// <param name="useDst">使用夏令时取反值:使用为:false</param>
        /// <returns></returns>
        public static DateTime ConvertTimeFromUtc(long utcTicks, TimeZoneInfo timeZone, bool useDst)// = true)
        {
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(utcTicks, DateTimeKind.Utc), timeZone);
            // 时间为夏令时时间 且指定不使用夏令时 减一小时
            if (useDst && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(time))
            {
                time = time.AddHours(-1);
            }
            return time;
        }

        public static DateTime ConvertTimeFromUtc(long utcTicks, string timeZoneId, bool useDst)// = true)
        {
            return ConvertTimeFromUtc(utcTicks, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), useDst);
        }
        
        /// <summary>
        /// 修改了此方法,适用于将指定时区时间转换为UTC时间
        /// </summary>
        /// <param name="datetime">源时区的时间</param>
        /// <param name="sourceTimezone">源时区</param>
        /// <param name="useDst">使用夏令时取反值:使用为:false, 当前台不勾选夏令时的时候增加一小时</param>
        /// <returns></returns>
        public static DateTime ConvertTimeToUtcDate(DateTime datetime, TimeZoneInfo sourceTimezone, bool useDst)// = true)
        {
            datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);
            // 时间为夏令时时间 且指定不使用夏令时 加一小时
            if (useDst && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(datetime))
            {
                datetime = datetime.AddHours(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(datetime, sourceTimezone);
        }

        public static DateTime ConvertTimeToUtcDate(DateTime datetime, string sourceTimezoneId, bool useDst)// = true)
        {
            return ConvertTimeToUtcDate(datetime, GeneralSettingConfig.FindSystemTimeZoneById(sourceTimezoneId), useDst);
        }

        public static long ConvertTimeToUtc(long ticks, TimeZoneInfo sourceTimezone, bool useDst)// = true)
        {
            DateTime newTime = new DateTime(ticks, DateTimeKind.Unspecified);
            return ConvertTimeToUtcDate(newTime, sourceTimezone, useDst).Ticks;
        }

        public static long ConvertTimeToUtc(DateTime datetime, string sourceTimezoneId, bool useDst)// = true)
        {
            return ConvertTimeToUtcDate(datetime, GeneralSettingConfig.FindSystemTimeZoneById(sourceTimezoneId), useDst).Ticks;
        }

        public static TimeSpan Duration(long utcTicks0, long utcTicks1)
        {
            return TimeSpan.FromTicks(utcTicks0).Subtract(TimeSpan.FromTicks(utcTicks1)).Duration();
        }

        public static DateTime ConvertStringToDateTime(string source, string format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                return DateTime.ParseExact(source, DEFAULT_TIME_FORMAT, null);
            }
            else
            {
                return DateTime.ParseExact(source, format, null);
            }
        }

        public static string ConvertDateTimeToString(DateTime source, string format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                return source.ToString(DEFAULT_TIME_FORMAT);
            }
            else
            {
                return source.ToString(format);
            }
        }

        public static string GetFormattedTimeStamp(DateTime datetime = default(DateTime), string timeformat = null)
        {
            if (datetime.Equals(DateTime.MinValue))
            {
                datetime = DateTime.Now;
            }

            if (string.IsNullOrEmpty(timeformat))
            {
                timeformat = TIMESTAMP_YYYYMMDDHHMMSS;
            }
            return datetime.ToString(timeformat);
        }

        public static long GetTicks(string timeStr, string timeZoneId, bool isDayLight)
        {
            //DateTime dt = DateTime.Parse(timeStr);
            //DateTime result = DateTimeUtil.ConvertTimeToUtcDate(dt, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId), !isDayLight);
            //return result.Ticks;
            return ConvertTimeToUtcDate(timeStr, timeZoneId, isDayLight).Ticks;
        }

        public static DateTime ConvertTimeToUtcDate(string timeStr, string timeZoneId, bool isDayLight)
        {
            DateTime dt = DateTime.Parse(timeStr);
            DateTime result = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
            return result;
        }

        public static string GetSimplifyZoneInfo(string timeZoneId)
        {
            return GetAllStaticTimeZones().Where(x => x.Id == timeZoneId).FirstOrDefault()?.Zone;
        }

        public static DateTime ConvertTimeToUtcDate(DateTime datetime, GeneralSettingModel gls)
        {
            var sourceTimezone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            var useDst = !gls.DayLight;

            datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);
            // 时间为夏令时时间 且指定不使用夏令时 加一小时
            if (useDst && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(datetime))
            {
                datetime = datetime.AddHours(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(datetime, sourceTimezone);
        }

        public static DateTime ConvertTimeFromUtc(long utcTicks, GeneralSettingModel gls)
        {
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            var useDst = !gls.DayLight;

            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(utcTicks, DateTimeKind.Utc), timeZone);
            // 时间为夏令时时间 且指定不使用夏令时 减一小时
            if (useDst && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(time))
            {
                time = time.AddHours(-1);
            }
            return time;
        }
        public static string ConvertTimeZone(string dateTimeStr, string sourceTimeZoneId, string targetTimeZoneId)
        {
            var sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneId);
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZoneId);
            var localTime = DateTime.ParseExact(dateTimeStr, "yyyy/M/d H:m", CultureInfo.InvariantCulture);

            var sourceDateTimeOffset = new DateTimeOffset(
                localTime,
                sourceTimeZone.GetUtcOffset(localTime));

            var targetDateTimeOffset = TimeZoneInfo.ConvertTime(
                sourceDateTimeOffset,
                targetTimeZone);

            return targetDateTimeOffset.ToString("yyyy/M/d H:m");
        }

        public static DateTime ConvertTimeFromUtc(DateTime utcTime, GeneralSettingModel gls)
        {
            if (gls.TimeZoneId == "UTC")
            {
                //前台$$.date.format()方法，如果Kind是UTC， 会自动转换到浏览器时区， 目的时区是UTC时要特殊处理 RECO-9783 
                return new DateTime(utcTime.Ticks, DateTimeKind.Unspecified);
            }
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            var useDst = !gls.DayLight;

            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            // 时间为夏令时时间 且指定不使用夏令时 减一小时
            if (useDst && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(time))
            {
                time = time.AddHours(-1);
            }
            return time;
        }

        //Move From GeneralSettingService
        public static string ConvertFromUTCDateTime(string startTime, GeneralSettingModel gls, string format = null)
        {
            DateTime dt = DateTime.Parse(startTime);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            var returnFormat = JSDateTimeFormat.DEFAULT_TIME_FORMAT;
            if (!string.IsNullOrEmpty(format))
            {
                returnFormat = format;
            }
            return DateTimeUtil.ConvertTimeFromUtc(dt, gls).ToString(returnFormat);
        }

        public static string GetFormattedTimeBetweenTimezones(string sourceDateTimeString, string sourceTimeZoneId, string destinationTimeZoneId, string timeFormat = null, bool needUtcSuffix = false)
        {
            string resultFormat = "{0}";
            if (needUtcSuffix) resultFormat = "{0} {1}";
            DateTime sourceDateTimeUnspecified = DateTime.Parse(RemoveTimeUtcSuffix(sourceDateTimeString));
            TimeZoneInfo sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneId);
            TimeZoneInfo destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(destinationTimeZoneId);
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(sourceDateTimeUnspecified, sourceTimeZone);
            DateTime destinationDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, destinationTimeZone);
            var temp = destinationDateTime.ToString(timeFormat ?? JSDateTimeFormat.DEFAULT_TIME_FORMAT);
            return string.Format(resultFormat, temp, needUtcSuffix ? GetTimeZoneNameById(destinationTimeZoneId) : "");
        }

        public static string GetFormattedTimeFromUtc(string sourceDateTimeString, string destinationTimeZoneId, string timeFormat = null)
        {
            DateTime sourceDateTimeUnspecified = DateTime.Parse(RemoveTimeUtcSuffix(sourceDateTimeString));
            TimeZoneInfo destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(destinationTimeZoneId);
            DateTime destinationDateTime = TimeZoneInfo.ConvertTimeFromUtc(sourceDateTimeUnspecified, destinationTimeZone);
            return destinationDateTime.ToString(timeFormat ?? JSDateTimeFormat.DEFAULT_TIME_FORMAT);
        }

        private static string GetTimeZoneNameById(string timeZoneId)
        {
            string timeZoneName = string.Empty;
            if (!string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneName = TimeZoneConvertHelper.FindSystemTimeZoneById(timeZoneId).DisplayName;
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(timeZoneName);
                timeZoneName = matchResult.Value;
            }
            return timeZoneName;
        }
        private static List<AveTimeZone> rv;
        static DateTimeUtil()
        {
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            BuildTimeZoneInfo();
        }
        public static List<AveTimeZone> GetAllStaticTimeZones()
        {
            if (Culture != null && System.Threading.Thread.CurrentThread.CurrentCulture.LCID != Culture.LCID)
            {
                BuildTimeZoneInfo();
                Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            }
            return rv;
        }

        public static void BuildTimeZoneInfo()
        {
            rv = new List<AveTimeZone>();
            // ============================== 000 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Dateline Standard Time",
                //(UTC-12:00) International Date Line West
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_InternationalDateLineWest"),
                Zone = "(UTC-12:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 12),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "UTC-11",
                //(UTC-11:00) Coordinated Universal Time-11
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime11"),
                Zone = "(UTC-11:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Aleutian Standard Time",
                //(UTC-10:00) Aleutian Islands
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AleutianIslands"),
                Zone = "(UTC-10:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Hawaiian Standard Time",
                //(UTC-10:00) Hawaii
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Hawaii"),
                Zone = "(UTC-10:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Marquesas Standard Time",
                //(UTC-09:30) Marquesas Islands 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_MarquesasIslands"),
                Zone = "(UTC-09:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-(TimeSpan.TicksPerHour * 9 + TimeSpan.TicksPerMinute * 30)),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Alaskan Standard Time",
                //(UTC-09:00) Alaska
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Alaska"),
                Zone = "(UTC-09:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "UTC-09",
                //(UTC-09:00) Coordinated Universal Time-09
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime09"),
                Zone = "(UTC-09:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Pacific Standard Time (Mexico)",
                //(UTC-08:00) Baja California 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BajaCalifornia"),
                Zone = "(UTC-08:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "UTC-08",
                //(UTC-08:00) Coordinated Universal Time-08 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime08"),
                Zone = "(UTC-08:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Pacific Standard Time",
                //(UTC-08:00) Pacific Time (US & Canada)
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_PacificTimeUSCanada"),
                Zone = "(UTC-08:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "US Mountain Standard Time",
                //(UTC-07:00) Arizona
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Arizona"),
                Zone = "(UTC-07:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Mountain Standard Time (Mexico)",
                //(UTC-07:00) Chihuahua, La Paz, Mazatlan
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_ChihuahuaLaPazMazatlan"),
                Zone = "(UTC-07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Mountain Standard Time",
                //(UTC-07:00) Mountain Time (US & Canada)
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_MountainTimeUSCanada"),
                Zone = "(UTC-07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 7),
            });
            // ============================== 010 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Central America Standard Time",
                //(UTC-06:00) Central America
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CentralAmerica"),
                Zone = "(UTC-06:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Central Standard Time",
                //(UTC-06:00) Central Time (US & Canada)
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CentralTimeUSCanada"),
                Zone = "(UTC-06:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Easter Island Standard Time",
                //(UTC-06:00) Easter Island 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_EasterIsland"),
                Zone = "(UTC-06:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Central Standard Time (Mexico)",
                //(UTC-06:00) Guadalajara, Mexico City, Monterrey
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_GuadalajaraMexicoCityMonterrey"),
                Zone = "(UTC-06:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Canada Central Standard Time",
                //(UTC-06:00) Saskatchewan
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Saskatchewan"),
                Zone = "(UTC-06:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "SA Pacific Standard Time",
                //(UTC-05:00) Bogota, Lima, Quito, Rio Branco 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BogotaLimaQuito"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Eastern Standard Time (Mexico)",
                //(UTC - 05:00) Chetumal
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Chetumal"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Eastern Standard Time",
                //(UTC-05:00) Eastern Time (US & Canada) 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_EasternTimeUSCanada"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Haiti Standard Time",
                //(UTC-05:00) Haiti
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Haiti"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Cuba Standard Time",
                //(UTC - 05:00) Havana
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Havana"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "US Eastern Standard Time",
                //(UTC-05:00) Indiana (East) 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_IndianaEast"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Turks And Caicos Standard Time",
                //(UTC-05:00) Turks and Caicos 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_TurksandCaicos"),
                Zone = "(UTC-05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Paraguay Standard Time",
                //(UTC-04:00) Asuncion
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Asuncion"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Atlantic Standard Time",
                //(UTC-04:00) Atlantic Time (Canada)
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AtlanticTimeCanada"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Venezuela Standard Time",
                //(UTC-04:00) Caracas 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Caracas"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            // ============================== 020 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Central Brazilian Standard Time",
                //(UTC-04:00) Cuiaba
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Cuiaba"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "SA Western Standard Time",
                //(UTC-04:00) Georgetown, La Paz, Manaus, San Juan 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_GeorgetownLaPazManausSanJuan"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Pacific SA Standard Time",
                //(UTC-04:00) Santiago 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Santiago"),
                Zone = "(UTC-04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Newfoundland Standard Time",
                //(UTC-03:30) Newfoundland
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Newfoundland"),
                Zone = "(UTC-03:30)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-(TimeSpan.TicksPerHour * 3 + TimeSpan.TicksPerMinute * 30)),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Tocantins Standard Time",
                //(UTC-03:00) Araguaina 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Araguaina"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "E. South America Standard Time",
                //(UTC-03:00) Brasilia
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Brasilia"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "SA Eastern Standard Time",
                //(UTC-03:00) Cayenne, Fortaleza 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CayenneFortaleza"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Argentina Standard Time",
                //(UTC-03:00) City of Buenos Aires
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CityofBuenosAires"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Greenland Standard Time",
                //(UTC-03:00) Greenland 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Greenland"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Montevideo Standard Time",
                //(UTC-03:00) Montevideo
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Montevideo"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "Magallanes Standard Time",
                //(UTC-03:00) Punta Arenas 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_PuntaArenas"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Saint Pierre Standard Time",
                //(UTC-03:00) Saint Pierre and Miquelon
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_SaintPierreandMiquelon"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Bahia Standard Time",
                //(UTC-03:00) Salvador
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Salvador"),
                Zone = "(UTC-03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "UTC-02",
                //(UTC-02:00) Coordinated Universal Time-02
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime02"),
                Zone = "(UTC-02:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 2),
            });
            // ============================== 030 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Azores Standard Time",
                //(UTC-01:00) Azores
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Azores"),
                Zone = "(UTC-01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 1),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Cape Verde Standard Time",
                //(UTC-01:00) Cape Verde Is.
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CapeVerdeIs"),
                Zone = "(UTC-01:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(-TimeSpan.TicksPerHour * 1),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "UTC",  // StandardName和ID不同
                //(UTC) Coordinated Universal Time
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime"),
                Zone = "(UTC)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(0),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "GMT Standard Time",
                //(UTC) Dublin, Edinburgh, Lisbon, London
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_DublinEdinburghLisbonLondon"),
                Zone = "(UTC+00:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(0),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Greenwich Standard Time",
                //(UTC) Monrovia, Reykjavik
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_MonroviaReykjavik"),
                Zone = "(UTC+00:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(0),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Sao Tome Standard Time",
                //(UTC+00:00) Sao Tome
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_SaoTome"),
                Zone = "(UTC+00:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(0),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Morocco Standard Time",
                //(UTC+01:00) Casablanca 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Casablanca"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(0),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "W. Europe Standard Time",
                //(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AmsterdamBerlinBernRomeStockholmVienna"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 1),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Central Europe Standard Time",
                //(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BelgradeBratislavaBudapestLjubljanaPrague"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 1),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Romance Standard Time",
                //(UTC+01:00) Brussels, Copenhagen, Madrid, Paris
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BrusselsCopenhagenMadridParis"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 1),
            });
            // ============================== 040 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Central European Standard Time",
                //(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_SarajevoSkopjeWarsawZagreb"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 1),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "W. Central Africa Standard Time",
                //(UTC+01:00) West Central Africa
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_WestCentralAfrica"),
                Zone = "(UTC+01:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 1),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Jordan Standard Time",
                //(UTC+02:00) Amman
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Amman"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "GTB Standard Time",
                //(UTC+02:00) Athens, Bucharest"
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AthensBucharest"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Middle East Standard Time",
                //(UTC+02:00) Beirut
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Beirut"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Egypt Standard Time",
                //(UTC+02:00) Cairo
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Cairo"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "E. Europe Standard Time",
                //(UTC+02:00) Chisinau
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Chisinau"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Syria Standard Time",
                //(UTC+02:00) Damascus
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Damascus"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "West Bank Standard Time",
                //(UTC+02:00) Gaza, Hebron 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_GazaHebron"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "South Africa Standard Time",
                //(UTC+02:00) Harare, Pretoria
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_HararePretoria"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "FLE Standard Time",
                //(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_HelsinkiKyivRigaSofiaTallinnVilnius"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Israel Standard Time",
                //(UTC+02:00) Jerusalem
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Jerusalem"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            // ============================== 050 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Kaliningrad Standard Time",
                //(UTC+02:00) Kaliningrad 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Kaliningrad"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Sudan Standard Time",
                //(UTC+02:00) Khartoum
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Khartoum"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Libya Standard Time",
                //(UTC+02:00) Tripoli
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Tripoli"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Namibia Standard Time",
                //(UTC+02:00) (UTC+02:00) Windhoek 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Windhoek"),
                Zone = "(UTC+02:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 2),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Arabic Standard Time",
                //(UTC+03:00) Baghdad
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Baghdad"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Turkey Standard Time",
                //(UTC+03:00) Istanbul
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Istanbul"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Arab Standard Time",
                //(UTC+03:00) Kuwait, Riyadh
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_KuwaitRiyadh"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Belarus Standard Time",
                //(UTC+03:00) Minsk
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Minsk"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Russian Standard Time",
                //(UTC+03:00) Moscow, St. Petersburg 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_MoscowStPetersburg"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "E. Africa Standard Time",
                //(UTC+03:00) Nairobi 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Nairobi"),
                Zone = "(UTC+03:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Iran Standard Time",
                //(UTC+03:30) Tehran
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Tehran"),
                Zone = "(UTC+03:30)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 3 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Arabian Standard Time",
                //(UTC+04:00) Abu Dhabi, Muscat
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AbuDhabiMuscat"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Astrakhan Standard Time",
                //(UTC+04:00) Astrakhan, Ulyanovsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AstrakhanUlyanovsk"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Azerbaijan Standard Time",
                //(UTC+04:00) Baku
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Baku"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Russia Time Zone 3",
                //(UTC+04:00) Izhevsk, Samara
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_IzhevskSamara"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Mauritius Standard Time",
                //(UTC+04:00) Port Louis
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_PortLouis"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            // ============================== 060 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Saratov Standard Time",
                //(UTC+04:00) Saratov 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Saratov"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Georgian Standard Time",
                //(UTC+04:00) Tbilisi 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Tbilisi"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Volgograd Standard Time",
                //(UTC+04:00) Volgograd 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Volgograd"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Caucasus Standard Time",
                //(UTC+04:00) Yerevan 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Yerevan"),
                Zone = "(UTC+04:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Afghanistan Standard Time",
                //(UTC+04:30) Kabul
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Kabul"),
                Zone = "(UTC+04:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 4 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "West Asia Standard Time",
                //(UTC+05:00) Ashgabat, Tashkent
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AshgabatTashkent"),
                Zone = "(UTC+05:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Ekaterinburg Standard Time",
                //(UTC+05:00) Ekaterinburg 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Ekaterinburg"),
                Zone = "(UTC+05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Pakistan Standard Time",
                //(UTC+05:00) Islamabad, Karachi 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_IslamabadKarachi"),
                Zone = "(UTC+05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Qyzylorda Standard Time",
                //(UTC+05:00) Qyzylorda 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Qyzylorda"),
                Zone = "(UTC+05:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "India Standard Time",
                //(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_ChennaiKolkataMumbaiNewDelhi"),
                Zone = "(UTC+05:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Sri Lanka Standard Time",
                //(UTC+05:30) Sri Jayawardenepura
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_SriJayawardenepura"),
                Zone = "(UTC+05:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Nepal Standard Time",
                //(UTC+05:45) Kathmandu
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Kathmandu"),
                Zone = "(UTC+05:45)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 5 + TimeSpan.TicksPerMinute * 45),
            });

            // ============================== 070 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Central Asia Standard Time",
                //(UTC+06:00) Astana
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Astana"),
                Zone = "(UTC+06:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Bangladesh Standard Time",
                //(UTC+06:00) Dhaka
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Dhaka"),
                Zone = "(UTC+06:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Omsk Standard Time",
                //(UTC+06:00) Omsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Omsk"),
                Zone = "(UTC+06:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 6),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Myanmar Standard Time",
                //(UTC+06:30) Yangon (Rangoon)
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Yangon"),
                Zone = "(UTC+06:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 6 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "SE Asia Standard Time",
                //(UTC+07:00) Bangkok, Hanoi, Jakarta
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BangkokHanoiJakarta"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Altai Standard Time",
                //(UTC+07:00) Barnaul, Gorno-Altaysk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BarnaulGornoAltaysk"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "W. Mongolia Standard Time",
                //(UTC + 07:00) Hovd
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Hovd"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "North Asia Standard Time",
                //(UTC+07:00) Krasnoyarsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Krasnoyarsk"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "N. Central Asia Standard Time",
                //(UTC+07:00) Novosibirsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Novosibirsk"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "Tomsk Standard Time",
                //(UTC+07:00) Tomsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Tomsk"),
                Zone = "(UTC+07:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 7),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "China Standard Time",
                //(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BeijingChongqingHongKongUrumqi"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "North Asia East Standard Time",
                //(UTC+08:00) Irkutsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Irkutsk"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Singapore Standard Time",   // StandardName和ID不同
                //(UTC+08:00) Kuala Lumpur, Singapore
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_KualaLumpurSingapore"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "W. Australia Standard Time",
                //(UTC + 08:00) Perth
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Perth"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            // ============================== 080 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Taipei Standard Time",
                //(UTC+08:00) Taipei 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Taipei"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Ulaanbaatar Standard Time",
                //(UTC+08:00) Ulaanbaatar
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Ulaanbaatar"),
                Zone = "(UTC+08:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Aus Central W. Standard Time",
                //(UTC+08:45) Eucla
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Eucla"),
                Zone = "(UTC+08:45)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 8 + TimeSpan.TicksPerMinute * 45),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Transbaikal Standard Time",
                //(UTC+09:00) Chita
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Chita"),
                Zone = "(UTC+09:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Tokyo Standard Time",
                //(UTC+09:00) Osaka, Sapporo, Tokyo
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_OsakaSapporoTokyo"),
                Zone = "(UTC+09:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "North Korea Standard Time",
                //(UTC+09:00) Pyongyang 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Pyongyang"),
                Zone = "(UTC+09:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Korea Standard Time",
                //(UTC+09:00) Seoul
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Seoul"),
                Zone = "(UTC+09:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Yakutsk Standard Time",
                //(UTC+09:00) Yakutsk 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Yakutsk"),
                Zone = "(UTC+09:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Cen. Australia Standard Time",
                //(UTC+09:30) Adelaide
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Adelaide"),
                Zone = "(UTC+09:30)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9 + TimeSpan.TicksPerMinute * 30),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "AUS Central Standard Time",
                //(UTC+09:30) Darwin
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Darwin"),
                Zone = "(UTC+09:30)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 9 + TimeSpan.TicksPerMinute * 30),
            });


            rv.Add(new AveTimeZone()
            {
                Id = "E. Australia Standard Time",
                //(UTC+10:00) Brisbane
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Brisbane"),
                Zone = "(UTC+10:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "AUS Eastern Standard Time",
                //(UTC+10:00) Canberra, Melbourne, Sydney
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CanberraMelbourneSydney"),
                Zone = "(UTC+10:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "West Pacific Standard Time",
                //(UTC+10:00) Guam, Port Moresby
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_GuamPortMoresby"),
                Zone = "(UTC+10:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10),
            });
            // ============================== 090 ==============================
            rv.Add(new AveTimeZone()
            {
                Id = "Tasmania Standard Time",
                //(UTC+10:00) Hobart
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Hobart"),
                Zone = "(UTC+10:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Vladivostok Standard Time",
                //(UTC+10:00) Vladivostok
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Vladivostok"),
                Zone = "(UTC+10:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Lord Howe Standard Time",
                //(UTC+10:30) Lord Howe Island
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_LordHoweIsland"),
                Zone = "(UTC+10:30)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 10 + TimeSpan.TicksPerMinute * 30),
            });


            rv.Add(new AveTimeZone()
            {
                Id = "Bougainville Standard Time",
                //(UTC+11:00) Bougainville Island
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_BougainvilleIsland"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Russia Time Zone 10",
                //(UTC+11:00) Chokurdakh 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Chokurdakh"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Magadan Standard Time",
                //(UTC+11:00) Magadan 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Magadan"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Norfolk Standard Time",
                //(UTC+11:00) Norfolk Island 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_NorfolkIsland"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Sakhalin Standard Time",
                //(UTC+11:00) Sakhalin
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Sakhalin"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Central Pacific Standard Time",
                //(UTC+11:00) Solomon Is., New Caledonia
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_SolomonIsNewCaledonia"),
                Zone = "(UTC+11:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 11),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Russia Time Zone 11",
                //(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AnadyrPetropavlovskKamchatsky"),
                Zone = "(UTC+12:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 12),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "New Zealand Standard Time",
                //(UTC+12:00) Auckland, Wellington
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_AucklandWellington"),
                Zone = "(UTC+12:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 12),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "UTC+12",
                //(UTC+12:00) Coordinated Universal Time+12
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime12"),
                Zone = "(UTC+12:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 12),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Fiji Standard Time",
                //(UTC+12:00) Fiji
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Fiji"),
                Zone = "(UTC+12:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 12),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Chatham Islands Standard Time",
                //(UTC+12:45) Chatham Islands 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_ChathamIslands"),
                Zone = "(UTC+12:45)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 12 + TimeSpan.TicksPerMinute * 45),
            });

            rv.Add(new AveTimeZone()
            {
                Id = "UTC+13",
                //(UTC+13:00) Coordinated Universal Time+13 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_CoordinatedUniversalTime13"),
                Zone = "(UTC+13:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 13),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Tonga Standard Time",
                //(UTC+13:00) Nuku'alofa
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Nukualofa"),
                Zone = "(UTC+13:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 13),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Samoa Standard Time",
                //(UTC+13:00) Samoa 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_Samoa"),
                Zone = "(UTC+13:00)",
                SupportsDaylightSavingTime = true,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 13),
            });
            rv.Add(new AveTimeZone()
            {
                Id = "Line Islands Standard Time",
                //(UTC+14:00) Kiritimati Island 
                DisplayName = I18NEntity.GetString("RM_JS_JM_TimeZone_KiritimatiIsland"),
                Zone = "(UTC+14:00)",
                SupportsDaylightSavingTime = false,
                BaseUtcOffset = new TimeSpan(TimeSpan.TicksPerHour * 14),
            });
        }

        public static List<string> AllTimeZones =
        [
            "Dateline Standard Time",
            "UTC-11",
            "Samoa Standard Time",
            "Hawaiian Standard Time",
            "Alaskan Standard Time",
            "Pacific Standard Time (Mexico)",
            "Pacific Standard Time",
            "US Mountain Standard Time",
            "Mountain Standard Time (Mexico)",
            "Mountain Standard Time",
            "Central America Standard Time",
            "Central Standard Time",
            "Central Standard Time (Mexico)",
            "Canada Central Standard Time",
            "SA Pacific Standard Time",
            "Eastern Standard Time",
            "US Eastern Standard Time",
            "Venezuela Standard Time",
            "Paraguay Standard Time",
            "Atlantic Standard Time",
            "Central Brazilian Standard Time",
            "SA Western Standard Time",
            "Pacific SA Standard Time",
            "Newfoundland Standard Time",
            "E. South America Standard Time",
            "Argentina Standard Time",
            "SA Eastern Standard Time",
            "Greenland Standard Time",
            "Montevideo Standard Time",
            "UTC-02",
            "Mid-Atlantic Standard Time",
            "Azores Standard Time",
            "Cape Verde Standard Time",
            "Morocco Standard Time",
            "UTC",
            "GMT Standard Time",
            "Greenwich Standard Time",
            "W. Europe Standard Time",
            "Central Europe Standard Time",
            "Romance Standard Time",
            "Central European Standard Time",
            "W. Central Africa Standard Time",
            "Namibia Standard Time",
            "Jordan Standard Time",
            "GTB Standard Time",
            "Middle East Standard Time",
            "Egypt Standard Time",
            "Syria Standard Time",
            "South Africa Standard Time",
            "FLE Standard Time",
            "Israel Standard Time",
            "E. Europe Standard Time",
            "Arabic Standard Time",
            "Arab Standard Time",
            "Russian Standard Time",
            "E. Africa Standard Time",
            "Iran Standard Time",
            "Arabian Standard Time",
            "Azerbaijan Standard Time",
            "Mauritius Standard Time",
            "Georgian Standard Time",
            "Caucasus Standard Time",
            "Afghanistan Standard Time",
            "Ekaterinburg Standard Time",
            "Pakistan Standard Time",
            "West Asia Standard Time",
            "India Standard Time",
            "Sri Lanka Standard Time",
            "Nepal Standard Time",
            "Central Asia Standard Time",
            "Bangladesh Standard Time",
            "N. Central Asia Standard Time",
            "Myanmar Standard Time",
            "SE Asia Standard Time",
            "North Asia Standard Time",
            "China Standard Time",
            "North Asia East Standard Time",
            "Singapore Standard Time",
            "W. Australia Standard Time",
            "Taipei Standard Time",
            "Ulaanbaatar Standard Time",
            "Tokyo Standard Time",
            "Korea Standard Time",
            "Yakutsk Standard Time",
            "Cen. Australia Standard Time",
            "AUS Central Standard Time",
            "E. Australia Standard Time",
            "AUS Eastern Standard Time",
            "West Pacific Standard Time",
            "Tasmania Standard Time",
            "Vladivostok Standard Time",
            "Magadan Standard Time",
            "Central Pacific Standard Time",
            "New Zealand Standard Time",
            "UTC+12",
            "Fiji Standard Time",
            "Aleutian Standard Time",
            "Marquesas Standard Time",
            "Eastern Standard Time (Mexico)",
            "Easter Island Standard Time",
            "Haiti Standard Time",
            "Cuba Standard Time",
            "Turks And Caicos Standard Time",
            "Magallanes Standard Time",
            "Tocantins Standard Time",
            "Saint Pierre Standard Time",
            "Libya Standard Time",
            "E. Europe Standard Time",
            "West Bank Standard Time",
            "Sudan Standard Time",
            "Astrakhan Standard Time",
            "Russia Time Zone 3",
            "Saratov Standard Time",
            "Qyzylorda Standard Time",
            "Omsk Standard Time",
            "Altai Standard Time",
            "Tomsk Standard Time",
            "Transbaikal Standard Time",
            "North Korea Standard Time",
            "Aus Central W. Standard Time",
            "Bougainville Standard Time",
            "Sakhalin Standard Time",
            "Russia Time Zone 10",
            "Norfolk Standard Time",
            "Russia Time Zone 11",
            "Chatham Islands Standard Time",
            "UTC+13",
            "Tonga Standard Time",
            "Kaliningrad Standard Time",
            "Kamchatka Standard Time",
            "Turkey Standard Time",
            "Volgograd Standard Time",
            "UTC-09",
            "UTC-08",
            "Yukon Standard Time",
            "W. Mongolia Standard Time",
            "Lord Howe Standard Time",
            "Line Islands Standard Time",
            "Bahia Standard Time"
        ];

   
        public static string RemoveTimeUtcSuffix(string time, string format = null)
        {
            if (string.IsNullOrWhiteSpace(time)) return time;

            var idx = time.IndexOf(" (UTC", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return time;

            var splitedTime = time[..idx].Trim();

            return DateTime.TryParseExact(
                splitedTime,
                format ?? JSDateTimeFormat.DEFAULT_TIME_FORMAT,  
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            ) ? splitedTime : time;
        }
    }
}
