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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace AvePoint.RA.Common.SystemSetting
{
    public static class GeneralSettingConfig
    {
        public static  Dictionary<TimeFormat, string> TimeFormats = new Dictionary<TimeFormat, string>();
        public static  Dictionary<DateFormat, string> DateFormats = new Dictionary<DateFormat, string>();
        //public static ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();
        public static List<GCommon.Contract.Server.Common.TimeZone.AveTimeZone> TimeZones;
        public static  List<string> TT = new List<string>();
        public static List<TimeZoneMsg> TimeZoneInfoes { get; private set; }


        public static System.Globalization.CultureInfo Culture;
        static GeneralSettingConfig()
        {
            InitTimeFormats();
            InitDataFormats();
            TimeZones = DateTimeUtil.GetAllStaticTimeZones();
            TimeZoneInfoes = GetFormateTimeZoneInfoes();
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        }

        public static void Reset()
        {
            if (Culture != null && System.Threading.Thread.CurrentThread.CurrentCulture.LCID != Culture.LCID)
            {
                TimeZones = DateTimeUtil.GetAllStaticTimeZones();
                TimeZoneInfoes = GetFormateTimeZoneInfoes();
            }
        }

        public static TimeZoneInfo FindSystemTimeZoneById(string windowsId)
        {
            return TimeZoneConvertHelper.FindSystemTimeZoneById(windowsId);
        }

        private static void InitTimeFormats()
        {
            TimeFormats.Add(TimeFormat.h_mm_ss_tt, "h:mm:ss tt");
            TimeFormats.Add(TimeFormat.h_mm_ss, "HH:mm:ss");
        }
        private static void InitDataFormats()
        {
            DateFormats.Add(DateFormat.yyyy_MM_dd, "yyyy-MM-dd");
            DateFormats.Add(DateFormat.M_d_yyyy, "M-d-yyyy");
            DateFormats.Add(DateFormat.M_d_yy, "M-d-yy");
            DateFormats.Add(DateFormat.MM_dd_yy, "MM-dd-yy");
            DateFormats.Add(DateFormat.d_MMMM_yy, "d-MMMM-yy");
            DateFormats.Add(DateFormat.MMMM_d_yyyy, "MMMM d,yyyy");
            DateFormats.Add(DateFormat.d_MMM_yyyy, "d-MMM-yyyy");
            DateFormats.Add(DateFormat.dd_MM_yyyy, "dd-MM-yyyy");
        }
        private static List<TimeZoneMsg> GetFormateTimeZoneInfoes()
        {
            List<TimeZoneMsg> TimeZones = new List<TimeZoneMsg>();
           var timeZoneInfo = GeneralSettingConfig.TimeZones;
            for (int i = 0; i < timeZoneInfo.Count; i++)
            {
                var timezone = timeZoneInfo[i];
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(timezone.DisplayName);
                var simplifyDisplayName = matchResult.Value;

                TimeZoneMsg msg = new TimeZoneMsg()
                {
                    id = timezone.Id,
                    displayName = timezone.DisplayName,
                    simplifyDisplayName = simplifyDisplayName,
                    zone = timezone.DisplayName,
                    offsetHours = timezone.BaseUtcOffset.Hours,
                    offsetMinutes = timezone.BaseUtcOffset.Minutes,
                    supportsDaylightSavingTime = timezone.SupportsDaylightSavingTime,
                    autoAdjustClock = timezone.SupportsDaylightSavingTime,
                };
                TimeZones.Add(msg);
            }
            return TimeZones;
        }

        public static AveTimeZone GetTimeZoneInforById(string timeZoneId)
        {
            AveTimeZone timeZoneInfo = null;
            AveTimeZone tempZoneInfo = null;
            for (int i = 0; i < TimeZones.Count; i++)
            {
                tempZoneInfo = TimeZones[i];
                if (tempZoneInfo.Id == timeZoneId)
                {
                    timeZoneInfo = tempZoneInfo;
                }
            }
            if(timeZoneInfo == null)
            {
                timeZoneInfo = TimeZones.FirstOrDefault(t => t.Id == "UTC");
            }
            return timeZoneInfo;
        }
    }
}
