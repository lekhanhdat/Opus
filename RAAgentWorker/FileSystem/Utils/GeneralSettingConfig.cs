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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RAFileSystem.Utils
{
    public static class GeneralSettingConfig
    {
        public static readonly Dictionary<TimeFormat, string> TimeFormats = new Dictionary<TimeFormat, string>();
        public static readonly Dictionary<DateFormat, string> DateFormats = new Dictionary<DateFormat, string>();
        public static ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();
        public static readonly List<string> TT = new List<string>();
        static GeneralSettingConfig()
        {
            InitTimeFormats();
            InitDataFormats();
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

        public static TimeZoneInfo GetTimeZoneInforById(string timeZoneId)
        {
            TimeZoneInfo timeZoneInfo = null;
            for (int i = 0; i < TimeZones.Count; i++)
			{
			 if(TimeZones[i].Id == timeZoneId){
                 timeZoneInfo = TimeZones[i];
                }
                
			}
            return timeZoneInfo;
        }
    }
}
