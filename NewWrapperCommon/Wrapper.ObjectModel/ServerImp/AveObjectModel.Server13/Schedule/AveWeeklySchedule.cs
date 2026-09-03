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

namespace AvePoint.ObjectModel.Server13
{
    class AveWeeklySchedule : AveDailySchedule, IAveWeeklySchedule
    {
        private SPWeeklySchedule mWeeklySchedule;

        public AveWeeklySchedule() : this(new SPWeeklySchedule())
        {            
        }

        public AveWeeklySchedule(SPWeeklySchedule weekly)
            : base(weekly)
        {
            mWeeklySchedule = weekly;
        }

        #region IAveWeeklySchedule Members

        public DayOfWeek BeginDayOfWeek
        {
            get
            {
                return mWeeklySchedule.BeginDayOfWeek;
            }
            set
            {
                mWeeklySchedule.BeginDayOfWeek = value;
            }
        }

        public int BeginHour
        {
            get
            {
                return mWeeklySchedule.BeginHour;
            }
            set
            {
                mWeeklySchedule.BeginHour = value;
            }
        }

        public int BeginMinute
        {
            get
            {
                return mWeeklySchedule.BeginMinute;
            }
            set
            {
                mWeeklySchedule.BeginMinute = value;
            }
        }

        public DayOfWeek EndDayOfWeek
        {
            get
            {
                return mWeeklySchedule.EndDayOfWeek;
            }
            set
            {
                mWeeklySchedule.EndDayOfWeek = value;
            }
        }

        public int EndHour
        {
            get
            {
                return mWeeklySchedule.EndHour;
            }
            set
            {
                mWeeklySchedule.EndHour = value;
            }
        }

        public int EndMinute
        {
            get
            {
                return mWeeklySchedule.EndMinute;
            }
            set
            {
                mWeeklySchedule.EndMinute = value;
            }
        }

        public override string ToString()
        {
            return mWeeklySchedule.ToString();
        }

        #endregion
    }
}
