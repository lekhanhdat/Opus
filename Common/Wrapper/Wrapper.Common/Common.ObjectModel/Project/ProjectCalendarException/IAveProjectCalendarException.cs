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
    public interface IAveProjectCalendarException
    {
        //Calendar
        DateTime Finish { get; }
        int Id { get; set; }
        string Name { get; }
        //public CalendarRecurrenceDays RecurrenceDays
        int RecurrenceFrequency { get; }
        int RecurrenceMonth { get; }
        int RecurrenceMonthDay { get; }
        int RecurrenceType { get; }
        AveCalendarRecurrenceWeek RecurrenceWeek { get; }
        int Shift1Finish { get; }
        int Shift1Start { get; }
        int Shift2Finish { get; }
        int Shift2Start { get; }
        int Shift3Finish { get; }
        int Shift3Start { get; }
        int Shift4Finish { get; }
        int Shift4Start { get; }
        int Shift5Finish { get; }
        int Shift5Start { get; }
        DateTime Start { get; }
    }
}
