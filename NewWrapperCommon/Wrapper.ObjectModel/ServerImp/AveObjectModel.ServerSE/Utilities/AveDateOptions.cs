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



using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveDateOptions : IAveDateOptions
    {
        private DateOptions mDateOptions;

        public AveDateOptions(DateOptions dateOptions)
        {
            mDateOptions = dateOptions;
        }

        public AveDateOptions(string localeId, AveCalendarType calendar, string workWeek, string firstDayOfWeek, string hijriAdjustment, string timeZoneSpan, string selectedDate)
        {
            mDateOptions = new DateOptions(localeId, (SPCalendarType)calendar, workWeek, firstDayOfWeek, hijriAdjustment, timeZoneSpan, selectedDate);
        }

        #region IAveDateOptions Members

        public string GetShortDateString(AveSimpleDate simpleDate)
        {
            SimpleDate rSimpleDate = new SimpleDate();
            AveObjectCopy.CopyObject(rSimpleDate, simpleDate, null);
            return mDateOptions.GetShortDateString(rSimpleDate);
        }

        public string TimePattern12Hour
        {
            get { return mDateOptions.TimePattern12Hour; }
        }

        public string TimePattern24Hour
        {
            get { return mDateOptions.TimePattern24Hour; }
        }

        public string[] DayNames
        {
            get { return mDateOptions.DayNames; }
        }

        public string[] GetHoursString(bool hoursMode24, bool hasMinutes)
        {
            return mDateOptions.GetHoursString(hoursMode24, hasMinutes);
        }

        #endregion
    }
}
