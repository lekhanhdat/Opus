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
using System.Globalization;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore
{
    class DateTimeDataFormat : BaseDataFormat
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(DateTimeDataFormat));

        public DateTimeDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem) :
            base(xmlField, destField, mItem)
        {
        }

        public override object CheckFieldValue(object value)
        {
            if (value is DateTime)
            {
                DateTime dateTmievalue = (DateTime)(value);
                DateTime minDatetime = new DateTime(1900, 1, 2);
                // 当value是Unspecified, 在存入sharepoint的时候会当作local时区再转化一次转成UTC时间，这个转化可能会导致转后时间小于1900-1-1的这个UTC时间，导致update失败。
                // 由于这个转化过程需要依赖目的端的时区，在restore层难以获取这个信息，所以很难比较。
                // 所以这里为了保证能不会因为这个column导致整个item还原失败，这里做一个笼统的判断，如果时间数值小于1900-1-2，且是Unspecified，就直接设置为UTC 1900-1-1。
                if (minDatetime > dateTmievalue && dateTmievalue.Kind.Equals(DateTimeKind.Unspecified))
                {
                    log.Warn(string.Format("Change the date time column value from {0} to UTC time {1}", dateTmievalue.ToString(), value.ToString()));
                    value = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                }
            }
            return Convert.ToDateTime(value, DateTimeFormatInfo.InvariantInfo);
        }
    }
}
