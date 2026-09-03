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
using System;

namespace AvePoint.ObjectModel.Server16
{
    class AveSchedule : IAveSchedule
    {
        private SPSchedule mSchedule;

        public AveSchedule()
        { }

        public AveSchedule(SPSchedule schedule)
        {
            mSchedule = schedule;
        }

        internal SPSchedule Schedule
        {
            get { return mSchedule; }
        }

        internal static AveSchedule InitSchedule(SPSchedule schedule)
        {
            return (AveSchedule)AveServerAssemblyInit.CreateElement(typeof(IAveSchedule), schedule);
        }

        public IAveSchedule FromString(string recurrenceValue)
        {
            SPSchedule schedule = SPSchedule.FromString(recurrenceValue);
            if (schedule == null)
            {
                return null;
            }
            return InitSchedule(schedule);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
