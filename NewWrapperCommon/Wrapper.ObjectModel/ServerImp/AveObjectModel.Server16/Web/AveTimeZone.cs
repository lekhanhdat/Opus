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

namespace AvePoint.ObjectModel.Server16
{
    class AveTimeZone : IAveTimeZone
    {
        private SPTimeZone mTimeZone;

        internal SPTimeZone TimeZone
        {
            get { return mTimeZone; }
            set { mTimeZone = value; }
        }

        public AveTimeZone(SPTimeZone timeZone)
        {
            mTimeZone = timeZone;
        }

        #region IAveTimeZone Members

        public DateTime UTCToLocalTime(DateTime dateTime)
        {
            return mTimeZone.UTCToLocalTime(dateTime);
        }

        public string Description
        {
            get { return mTimeZone.Description; }
        }

        public ushort ID
        {
            get
            {
                return mTimeZone.ID;
            }
            set
            {
                mTimeZone.ID = value;
            }
        }

        public DateTime LocalTimeToUTC(DateTime dateTime)
        {
            return mTimeZone.LocalTimeToUTC(dateTime);
        }

        #endregion
    }
}
