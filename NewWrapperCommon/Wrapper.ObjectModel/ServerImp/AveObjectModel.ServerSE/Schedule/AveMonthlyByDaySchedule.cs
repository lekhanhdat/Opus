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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveMonthlyByDaySchedule : AveSchedule, IAveMonthlyByDaySchedule
    {
        private SPMonthlyByDaySchedule mMonthlyByDaySchedule;

        public AveMonthlyByDaySchedule(SPMonthlyByDaySchedule monthlyByDaySchedule)
            : base(monthlyByDaySchedule)
        {
            mMonthlyByDaySchedule = monthlyByDaySchedule;
        }

        public AveMonthlyByDaySchedule()
            : this(new SPMonthlyByDaySchedule())
        { }

        public DayOfWeek BeginDay
        {
            get
            {
                return mMonthlyByDaySchedule.BeginDay;
            }
            set
            {
                mMonthlyByDaySchedule.BeginDay = value;
            }
        }

        public int BeginHour
        {
            get
            {
                return mMonthlyByDaySchedule.BeginHour;
            }
            set
            {
                mMonthlyByDaySchedule.BeginHour = value;
            }
        }

        public int BeginMinute
        {
            get
            {
                return mMonthlyByDaySchedule.BeginMinute;
            }
            set
            {
                mMonthlyByDaySchedule.BeginMinute = value;
            }
        }

        public int BeginSecond
        {
            get
            {
                return mMonthlyByDaySchedule.BeginSecond;
            }
            set
            {
                mMonthlyByDaySchedule.BeginSecond = value;
            }
        }

        public AveWeekOfMonth BeginWeek
        {
            get
            {
                return (AveWeekOfMonth)mMonthlyByDaySchedule.BeginWeek;
            }
            set
            {
                mMonthlyByDaySchedule.BeginWeek = (WeekOfMonth)value;
            }
        }

        public override string ToString()
        {
            return mMonthlyByDaySchedule.ToString();
        }
    }
}
