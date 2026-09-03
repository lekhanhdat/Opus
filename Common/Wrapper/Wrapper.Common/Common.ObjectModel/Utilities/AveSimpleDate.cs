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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public struct AveSimpleDate
    {
        private int mYear;
        private int mMonth;
        private int mDay;
        private int mEra;
        private readonly int mHashValue;
        
        public AveSimpleDate(int year, int month, int day, int era)
        {
            mYear = year;
            mMonth = month;
            mDay = day;
            mEra = era;
            mHashValue = ((year + month) + day) + era;
        }

        public AveSimpleDate(int year, int month, int day)
        {
            mYear = year;
            mMonth = month;
            mDay = day;
            mEra = 1;
            mHashValue = (year + month) + day;
        }

        public int Year
        {
            get
            {
                return mYear;
            }
            set
            {
                mYear = value;
            }
        }

        public int Month
        {
            get
            {
                return mMonth;
            }
            set
            {
                mMonth = value;
            }
        }

        public int Day
        {
            get
            {
                return mDay;
            }
            set
            {
                mDay = value;
            }
        }

        public int Era
        {
            get
            {
                return mEra;
            }
            set
            {
                mEra = value;
            }
        }

        public static bool operator >(AveSimpleDate di0, AveSimpleDate di)
        {
            if (di0.Era > di.Era)
            {
                return true;
            }
            if (di0.Era != di.Era)
            {
                return false;
            }
            if (di0.Year > di.Year)
            {
                return true;
            }
            if (di0.Year != di.Year)
            {
                return false;
            }
            return ((di0.Month > di.Month) || ((di0.Month == di.Month) && (di0.Day > di.Day)));
        }

        public static bool operator <(AveSimpleDate di0, AveSimpleDate di)
        {
            if (di0.Era < di.Era)
            {
                return true;
            }
            if (di0.Era != di.Era)
            {
                return false;
            }
            if (di0.Year < di.Year)
            {
                return true;
            }
            if (di0.Year != di.Year)
            {
                return false;
            }
            return ((di0.Month < di.Month) || ((di0.Month == di.Month) && (di0.Day < di.Day)));
        }

        public static bool operator >=(AveSimpleDate di0, AveSimpleDate di)
        {
            return !(di0 < di);
        }

        public static bool operator <=(AveSimpleDate di0, AveSimpleDate di)
        {
            return !(di0 > di);
        }

        public static bool operator ==(AveSimpleDate di0, AveSimpleDate di)
        {
            return (((di0.Year == di.Year) && (di0.Month == di.Month)) && (di0.Day == di.Day));
        }

        public static bool operator !=(AveSimpleDate di0, AveSimpleDate di)
        {
            return !(di0 == di);
        }
        
        public override bool Equals(object obj)
        {
            return (this == ((AveSimpleDate)obj));
        }

        public override int GetHashCode()
        {
            return mHashValue;
        }
    }
}
