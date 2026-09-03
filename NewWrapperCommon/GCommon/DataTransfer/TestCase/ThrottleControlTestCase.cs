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
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class ThrottleControlTestCase
    {
        public static bool Test()
        {
            ThrottleControlInfo info = new ThrottleControlInfo();
            info.Enable(true);
            info.SetWorkDay(WorkDays.Friday);
            info.SetWorkDay(WorkDays.Monday);
            info.SetWorkDay(WorkDays.Saturday);
            info.SetWorkDay(WorkDays.Sunday);
            info.SetWorkDay(WorkDays.Tuesday);
            info.SetWorkDay(WorkDays.Wednesday);
            info.SetWorkDay(WorkDays.Thursday);
            info.SetStartHour(1);
            info.SetEndHour(24);
            info.SetWorkHoursRate(128 * 1024);
            info.SetNonWorkHoursRate(32 * 1024);

            long total = 0;
            DateTime d = DateTime.Now;
            int i = 5;
            while (i > 0)
            {
                i--;
                long num = 64 * 1024;
                info.WriteBytesCount(num);
                total += num;
                long s = (DateTime.Now.Ticks - d.Ticks) / 10000000;
                if (s > 0)
                    Console.WriteLine(total / s);
            }

            return true;
        }
    }
}
