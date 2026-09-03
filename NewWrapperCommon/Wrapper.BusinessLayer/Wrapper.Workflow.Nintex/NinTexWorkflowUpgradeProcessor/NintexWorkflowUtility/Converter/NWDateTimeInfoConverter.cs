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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    static class NWDateTimeInfoConverter
    {
        /// <summary>
        /// datetimeXml格式如下：
        /// <DateTimeValue><Lcid>1033</Lcid><Date>12/3/2015</Date><Hour>0</Hour><Minute>0</Minute></DateTimeValue>
        /// </summary>
        /// <param name="datetimeXml"></param>
        /// <returns></returns>
        /// 需要注意：Office365 DateTimeInfo中有Officeset这项  而local没有 因此无法转换，故而会存在时区问题
        /// 当前的逻辑是保持源端时间与目的端时间在数据上一致，而不是时间上一致，具体例子如下：
        /// 源端时间：8：00 (timezone +8) 转移到目的端时间为：8：00 (timezone -8)
        public static DateTimeInfo ConvertDateTimeInfo(short srcWebTimeZone, IAveTimeZone timeZone, string datetimeXml)
        {
            TimeZoneInfo srcTimeZoneInfo = null;
            if (srcWebTimeZone > 0)
            {
                srcTimeZoneInfo = AveTimeZoneUtility.ToTimeZoneInfo(srcWebTimeZone);
            }
            DateTimeInfo dateTimeInfo = null;
            if (!string.IsNullOrEmpty(datetimeXml))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(datetimeXml);
                var DateNode = doc.SelectSingleNode("DateTimeValue/Date");
                DateTime date;
                if (DateTime.TryParse(DateNode.InnerText, out date))
                {
                    var HourNode = doc.SelectSingleNode("DateTimeValue/Hour");
                    var MinuteNode = doc.SelectSingleNode("DateTimeValue/Minute");
                    date = date.AddHours(int.Parse(HourNode.InnerText));
                    date = date.AddMinutes(int.Parse(MinuteNode.InnerText));
                    if (srcTimeZoneInfo != null)
                    {
                        date = TimeZoneInfo.ConvertTimeToUtc(date, srcTimeZoneInfo);
                    }
                    else
                    {
                        date = timeZone.LocalTimeToUTC(date);
                    }
                    
                    dateTimeInfo = new DateTimeInfo
                    {
                        Year = date.Year,
                        Month = date.Month,
                        Day = date.Day,
                        Hour = date.Hour,
                        Minute = date.Minute,
                        Offset = 0,
                    };
                }
            }
            return dateTimeInfo;
        }
    }
}
