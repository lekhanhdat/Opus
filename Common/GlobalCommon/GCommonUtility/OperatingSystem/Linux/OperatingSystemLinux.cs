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
    using System.Linq;

    internal sealed class OperatingSystemLinux : OperatingSystemBase, IOperatingSystem
    {
        public override uint GetCpuFrequency()
        {
            return LinuxCpuInfo.CpuFrequency;
        }

        public override string GetCpuName()
        {
            return LinuxCpuInfo.CpuName;
        }

        public override ulong GetLeftMemory()
        {
            return LinuxMemoryInfo.LeftMemory;
        }

        public override long GetTotalMemory()
        {
            return LinuxMemoryInfo.TotalMemory;
        }

        public override int GetCPUUsage()
        {
            return Convert.ToInt32(LinuxCpuInfo.GetCPUUsage());
        }

        public override string GetProcessCmdLine(int processId)
        {
            string file = $"/proc/{processId}/cmdline";
            if (File.Exists(file))
            {
                return File.ReadAllText(file);
            }
            return default;
        }

        /// <summary>
        /// NET6_0_OR_GREATER_linux,ARG: dotnet{\0}ProcessLoader.dll{\0}arg1{\0}arg2{\0}arg3{\0}
        /// </summary>
        /// <param name="processId"></param>
        /// <returns>dotnet ProcessLoader.dll arg1 arg2 arg3</returns>
        public override string[] GetProcessCommandLine(int processId)
        {
            var cmdLine = GetProcessCmdLine(processId);
            if (!string.IsNullOrEmpty(cmdLine))
            {
                var parameters = cmdLine.Split('\0').ToList();
                if (parameters.Any())
                {
                    return parameters.ToArray();
                }
            }
            return default;
        }
    }
}
