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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.FileSystem.Collect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAFileSystem.Utils
{
    public class TimeSettingUtil
    {
        private const string DEFAULT_TIME_FORMAT = "MM/dd/yyyy HH:mm";
        public static string GetFinishTime(DateTime time)
        {
            if (FSJobCache.Instance.TimeSettingModel != null)
            {
                return ConvertTiksToDateTime(FSJobCache.Instance.TimeSettingModel, time.Ticks, FSJobCache.Instance.TimeFormat).SimplifyFormatTime;
            }
            else
            {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                DateTime currentDate = ConvertTimeFromUtc(time.Ticks, localZone, false);
                return currentDate.ToString(DEFAULT_TIME_FORMAT);
            }
        }
        public static TimeModel ConvertTiksToDateTime(GeneralSettingModel gls, long tiks, string timeFormatDisplay)
        {
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            TimeZoneInfo tiz = GeneralSettingConfig.GetTimeZoneInforById(gls.TimeZoneId);
            DateTime currentDate = ConvertTimeFromUtc(tiks, tiz, !gls.DayLight);
            string formaTime = ConvertDateTimeToString(currentDate, string.Format("{0} {1} ", dateFormat, timeFormat));
            string simplifyFormatTime = formaTime;
            if (true)
            {
                formaTime = string.Format("{0} {1}", formaTime, timeFormatDisplay);
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(tiz.DisplayName);
                simplifyFormatTime = string.Format("{0} {1}", simplifyFormatTime, matchResult.Value);
                //"(UTC hh:mm)"
            }
            TimeModel model = new TimeModel()
            {
                FormaTime = formaTime,
                DataTime = currentDate,
                SimplifyFormatTime = simplifyFormatTime
            };
            return model;
        }

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

        private static string ConvertDateTimeToString(DateTime source, string format = null)
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


    }
}
