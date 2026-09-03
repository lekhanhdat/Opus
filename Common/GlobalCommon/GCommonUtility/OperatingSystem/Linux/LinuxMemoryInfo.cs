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
    /// Get memory info of Linux, if the current OS is not Linux, the properties will not be initialized, size unit is B
    /// </summary>
    internal static class LinuxMemoryInfo
    {
        public static Int64 TotalMemory { get; private set; }
        public static UInt64 LeftMemory
        {
            get
            {
                return GetLeftMemory(); //left memory is a real-time value
            }
        }

        static LinuxMemoryInfo()
        {
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                var lines = File.ReadAllLines(@"/proc/meminfo");
                var matches = new List<ClassPropertyMatch>()
                {
                    new ClassPropertyMatch(@"^MemTotal:\s+(.+)\s+(?i)(kb)", value => TotalMemory = Convert.ToInt64(value) * 1024),
                };
                LinuxUtility.GetValues(lines, matches);
            }
        }

        private static UInt64 GetLeftMemory()
        {
            var lines = File.ReadAllLines(@"/proc/meminfo");
            var valueStr = LinuxUtility.GetValue(lines, @"^MemAvailable:\s+(.+)\s+(?i)(kb)");
            return Convert.ToUInt64(valueStr) * 1024;
        }
    }
}
