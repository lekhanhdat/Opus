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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class ProcessUtil
    {
        public static string CountCurrentProcess()
        {
            var p = Process.GetCurrentProcess();
            return p.ProcessName;
        }

        public static string GetCurrentProcessName()
        {
            var p = Process.GetCurrentProcess();
            return p.ProcessName;
        }
        static long GetProcessMemory()
        {
            long memoryBytes = Process.GetCurrentProcess().PrivateMemorySize64;
            return memoryBytes;
        }

        public static double GetProcessMemoryMB()
        {
            long memoryBytes = GetProcessMemory();
            return ConvertBytesToMB(memoryBytes);
        }

        static double ConvertBytesToMB(long bytes)
        {
            const double bytesInMegabyte = 1024 * 1024;
            double megabytes = bytes / bytesInMegabyte;
            return Math.Round(megabytes, 2);
        }
    }
}
