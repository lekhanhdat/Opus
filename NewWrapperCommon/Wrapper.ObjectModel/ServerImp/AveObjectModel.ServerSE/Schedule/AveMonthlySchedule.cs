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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveMonthlySchedule : AveDailySchedule, IAveMonthlySchedule
    {
        private SPMonthlySchedule mMonthlySchedule;

        public AveMonthlySchedule(SPMonthlySchedule monthly)
            : base(monthly)
        {
            mMonthlySchedule = monthly;
        }

        public AveMonthlySchedule()
            : this(new SPMonthlySchedule())
        { }

        #region IAveMonthlySchedule Members

        public int BeginDay
        {
            get
            {
                return mMonthlySchedule.BeginDay;
            }
            set
            {
                mMonthlySchedule.BeginDay = value;
            }
        }

        public int BeginHour
        {
            get
            {
                return mMonthlySchedule.BeginHour;
            }
            set
            {
                mMonthlySchedule.BeginHour = value;
            }
        }

        public int BeginMinute
        {
            get
            {
                return mMonthlySchedule.BeginMinute;
            }
            set
            {
                mMonthlySchedule.BeginMinute = value;
            }
        }

        public int EndDay
        {
            get
            {
                return mMonthlySchedule.EndDay;
            }
            set
            {
                mMonthlySchedule.EndDay = value;
            }
        }

        public int EndMinute
        {
            get
            {
                return mMonthlySchedule.EndMinute;
            }
            set
            {
                mMonthlySchedule.EndMinute = value;
            }
        }

        public int EndHour
        {
            get
            {
                return mMonthlySchedule.EndHour;
            }
            set
            {
                mMonthlySchedule.EndHour = value;
            }
        }

        public override string ToString()
        {
            return mMonthlySchedule.ToString();
        }

        #endregion
    }
}
