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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public class AveProjectCalendarExceptionInfo
    {
        //public Calendar Calendar
        public DateTime Finish;
        public int Id;
        public string Name;
        //public CalendarRecurrenceDays RecurrenceDays
        public int RecurrenceFrequency;
        public int RecurrenceMonth;
        public int RecurrenceMonthDay;
        public int RecurrenceType;
        public AveCalendarRecurrenceWeek RecurrenceWeek;
        public int Shift1Finish;
        public int Shift1Start;
        public int Shift2Finish;
        public int Shift2Start;
        public int Shift3Finish;
        public int Shift3Start;
        public int Shift4Finish;
        public int Shift4Start;
        public int Shift5Finish;
        public int Shift5Start;
        public DateTime Start;
    }
}
