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
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace  AvePoint.Hybrid.Utility.Util
{
    public class ProcessUtil
    {
        public static int Count(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return 0;
            
            return Process.GetProcessesByName(processName).Count();
        }

        public static int CountCurrentProcess()
        {
            var p = Process.GetCurrentProcess();
            return Count(p.ProcessName);
        }

        public static string GetCmdline(Process p)
        {
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher(
                $"SELECT * FROM Win32_Process WHERE ProcessId={p.Id}"))
            {
                foreach (ManagementObject mo in mos.Get())
                {
                    foreach (var prop in mo.Properties)
                    {
                        if (prop.Name.Equals("CommandLine", StringComparison.OrdinalIgnoreCase))
                        {
                            return prop.Value.ToString();
                        }
                    }
                }

            }

            return string.Empty;
        }
    }
}
