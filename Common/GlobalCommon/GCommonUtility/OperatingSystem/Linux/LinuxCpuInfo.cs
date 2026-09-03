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
namespace AvePoint.GCommon.Utility
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    /// <summary>
    /// Get CPU info of Linux, if the current OS is not Linux, the properties will not be initialized
    /// </summary>
    internal static class LinuxCpuInfo
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(LinuxCpuInfo));
        public static string VendorId { get; private set; }
        public static string CpuName { get; private set; }
        public static UInt32 CpuFrequency { get; private set; }

        /// <summary>
        /// value should be within 0-100
        /// </summary>
        /// <returns></returns>
        public static double GetCPUUsage()
        {
            var content = File.ReadAllLines("/proc/stat");
            string cpuLine = "";
            foreach (var line in content)
            {
                if (line.StartsWith("cpu  "))
                {
                    cpuLine = line;
                    break;
                }
            }
            logger.Info($"Current CPU Line {cpuLine}");
            var data = cpuLine.Substring("cpu  ".Length).Split(' ');
            long total = 0;
            long idle = 0;
            for (int k = 0; k <= 8; k++)
            {
                var value = Convert.ToInt64(data[k]);
                if (k == 3)
                {
                    idle = value;
                }
                total += value;
            }
            var usage = (Convert.ToDouble(total - idle) / total)*100;
            logger.Info($"Current CPU idle {idle},Total:{total},Usage Perstange:{usage}%");
            return usage;
        }

        static LinuxCpuInfo()
        {
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                var cpuInfoLines = File.ReadAllLines(@"/proc/cpuinfo");
                var cpuInfoMatches = new List<ClassPropertyMatch>()
                {
                    new ClassPropertyMatch(@"^vendor_id\s+:\s+(.+)", value => VendorId = value),
                    new ClassPropertyMatch(@"^model name\s+:\s+(.+)", value => CpuName = value),
                    new ClassPropertyMatch(@"^cpu MHz\s+:\s+(.+)", value => CpuFrequency = Convert.ToUInt32(Convert.ToDouble(value))),
                };
                LinuxUtility.GetValues(cpuInfoLines, cpuInfoMatches);
            }
        }
    }
}
