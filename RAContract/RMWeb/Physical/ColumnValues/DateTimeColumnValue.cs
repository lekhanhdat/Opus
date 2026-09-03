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
using AvePoint.GCommon.Utility.TimeZoneConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical.ColumnValues
{
    public class DateTimeColumnValue
    {
        public DateTime Date { get; set; }
        public string TimeZoneId { get; set; }
        public bool IsSetDayLight { get; set; }

        public DateTime GetUtcDate()
        {
            //var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);  //TODO Cyrus
            var timeZone = TimeZoneConvertHelper.FindSystemTimeZoneById(this.TimeZoneId);
            var datetime = DateTime.SpecifyKind(this.Date, DateTimeKind.Unspecified);
            // 时间为夏令时时间 且指定不使用夏令时 加一小时
            if (!this.IsSetDayLight && timeZone.SupportsDaylightSavingTime && timeZone.IsDaylightSavingTime(datetime))
            {
                datetime = datetime.AddHours(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(datetime, timeZone);
        }
    }
}
