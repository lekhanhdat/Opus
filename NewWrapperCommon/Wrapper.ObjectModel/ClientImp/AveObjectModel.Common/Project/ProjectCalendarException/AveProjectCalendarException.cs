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

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectCalendarException : AveClientObject, IAveProjectCalendarException
    {
        private IAveRequest mRequest;

        public AveProjectCalendarException(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        #region Properties
        public DateTime Finish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Finish");
            }
        }

        public int Id
        {
            get
            {
                return base.DataCache.GetProperty<int>("Id");
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public int RecurrenceFrequency
        {
            get
            {
                return base.DataCache.GetProperty<int>("RecurrenceFrequency");
            }
        }

        public int RecurrenceMonth
        {
            get
            {
                return base.DataCache.GetProperty<int>("RecurrenceMonth");
            }
        }

        public int RecurrenceMonthDay
        {
            get
            {
                return base.DataCache.GetProperty<int>("RecurrenceMonthDay");
            }
        }

        public int RecurrenceType
        {
            get
            {
                return base.DataCache.GetProperty<int>("RecurrenceType");
            }
        }

        public AveCalendarRecurrenceWeek RecurrenceWeek
        {
            get
            {
                return base.DataCache.GetProperty<AveCalendarRecurrenceWeek>("RecurrenceWeek");
            }
        }

        public int Shift1Finish
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift1Finish");
            }
        }

        public int Shift1Start
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift1Start");
            }
        }

        public int Shift2Finish
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift2Finish");
            }
        }

        public int Shift2Start
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift2Start");
            }
        }

        public int Shift3Finish
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift3Finish");
            }
        }

        public int Shift3Start
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift3Start");
            }
        }

        public int Shift4Finish
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift4Finish");
            }
        }

        public int Shift4Start
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift4Start");
            }
        }

        public int Shift5Finish
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift5Finish");
            }
        }

        public int Shift5Start
        {
            get
            {
                return base.DataCache.GetProperty<int>("Shift5Start");
            }
        }

        public DateTime Start
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Start");
            }
        }
        #endregion

    }
}
