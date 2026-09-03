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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystemCore.Utils
{
    public static class DateTimeUtil
    {
        public const string DATETYPEForRuleFilter = "yyyy/MM/dd HH:mm:ss";

        //Move From GeneralSettingService
        public static string ConvertFromUTCDateTime(string startTime, GeneralSettingModel gls, string format = null)
        {
            DateTime dt = DateTime.Parse(startTime);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            var returnFormat = ContractConstants.DEFAULT_TIME_FORMAT;
            if (!string.IsNullOrEmpty(format))
            {
                returnFormat = format;
            }
            return DateTimeUtil.ConvertTimeFromUtc(dt, gls).ToString(returnFormat);
        }

        public static DateTime ConvertTimeFromUtc(DateTime utcTime, GeneralSettingModel gls)
        {
            if (gls.TimeZoneId == "UTC")
            {
                //前台$$.date.format()方法，如果Kind是UTC， 会自动转换到浏览器时区， 目的时区是UTC时要特殊处理 RECO-9783 
                return new DateTime(utcTime.Ticks, DateTimeKind.Unspecified);
            }
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(gls.TimeZoneId);
            var useDst = !gls.DayLight;

            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            // 时间为夏令时时间 且指定不使用夏令时 减一小时
            if (useDst && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(time))
            {
                time = time.AddHours(-1);
            }
            return time;
        }
    }
}
