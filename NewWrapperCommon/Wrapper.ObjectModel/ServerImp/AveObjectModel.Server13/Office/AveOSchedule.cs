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



namespace AvePoint.ObjectModel.Server13.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.Search.Administration;
    using AvePoint.Wrapper.Common;
    #endregion

    class AveOSchedule : IAveOSchedule
    {
        private Schedule mSchedule;

        public AveOSchedule(Schedule schedule)
        {
            mSchedule = schedule;
        }

        internal Schedule Schedule
        {
            get
            {
                return mSchedule;
            }
        }

        #region IAveOSchedule Members

        public string Description
        {
            get
            {
                return mSchedule.Description;
            }
        }

        public int StartHour
        {
            get
            {
                return mSchedule.StartHour;
            }
            set
            {
                mSchedule.StartHour = value;
            }
        }

        public int StartMinute
        {
            get
            {
                return mSchedule.StartMinute;
            }
            set
            {
                mSchedule.StartMinute = value;
            }
        }

        public int RepeatDuration
        {
            get
            {
                return mSchedule.RepeatDuration;
            }
            set
            {
                mSchedule.RepeatDuration = value;
            }
        }

        public int RepeatInterval
        {
            get
            {
                return mSchedule.RepeatInterval;
            }
            set
            {
                mSchedule.RepeatInterval = value;
            }
        }

        public DateTime NextRunTime
        {
            get
            {
                return mSchedule.NextRunTime;
            }
        }

        public int BeginDay
        {
            get
            {
                return mSchedule.BeginDay;
            }
            set
            {
                mSchedule.BeginDay = value;
            }
        }

        public int BeginMonth
        {
            get
            {
                return mSchedule.BeginMonth;
            }
            set
            {
                mSchedule.BeginMonth = value;
            }
        }

        public int BeginYear
        {
            get
            {
                return mSchedule.BeginYear;
            }
            set
            {
                mSchedule.BeginYear = value;
            }
        }

        #endregion
    }
}
