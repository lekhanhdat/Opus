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



namespace  AvePoint.Hybrid.Utility.OperationSystem.Windows.Wmi
{
    #region using directives
    using System;
    using System.Diagnostics;

    #endregion

    internal class WindowsOperatingSystem : WindowsOperatingSystemBase, IOperatingSystem
    {
        static String operatingSystemName;
        static String operatingSystemShortName;
        static String processorName;
        static UInt32 cpuHz;
        static Int64 totalVisibleMemorySize;

        public WindowsOperatingSystem()
        {
            if (String.IsNullOrEmpty(operatingSystemName))
            {
                var win32OperatingSystemCollection = Hybrid.Utility.OperationSystem.OperatingSystem.GetInstances(new String[] { "Caption", "CSDVersion", "SerialNumber", "TotalVisibleMemorySize" });
                foreach (Hybrid.Utility.OperationSystem.OperatingSystem operatingSystemItem in win32OperatingSystemCollection)
                {
                    using (operatingSystemItem)
                    {
                        operatingSystemShortName = operatingSystemItem.Caption;
                        operatingSystemName = String.Format("{0} {1}", operatingSystemItem.Caption ?? String.Empty, operatingSystemItem.CSDVersion ?? String.Empty);
                        totalVisibleMemorySize = Convert.ToInt64(operatingSystemItem.TotalVisibleMemorySize);
                        break;
                    }
                }

                var win32ProcessorCollection = Hybrid.Utility.OperationSystem.Process.GetInstances(new String[] { "CurrentClockSpeed", "Name" });
                foreach (Processor processItem in win32ProcessorCollection)
                {
                    using (processItem)
                    {
                        processorName = processItem.Name;
                        cpuHz = processItem.CurrentClockSpeed;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// use WMI class Win32_OperatingSystem to get free memory size
        /// </summary>
        /// <remarks>
        /// Notes, we use WMI namespace root\cimv2 here because in some Windows 2003 systems,
        /// the default WMI namespace is the root\default, not the cimv2. So be careful for 
        /// this. 
        /// </remarks>
        /// <returns>Free Physical Memory size</returns>
        Int64 GetFreePhysicalMemory()
        {
            var result = default(Int64);
            var win32OperatingSystemCollection = Hybrid.Utility.OperationSystem.OperatingSystem.GetInstances(new String[] { "FreePhysicalMemory" });
            foreach (Hybrid.Utility.OperationSystem.OperatingSystem operatingSystemItem in win32OperatingSystemCollection)
            {
                using (operatingSystemItem)
                {
                    result = Convert.ToInt64(operatingSystemItem.FreePhysicalMemory);
                    break;
                }
            }
            return result;
        }

        public OperatingSystemInfo GetOSInfo()
        {
            var result = new OperatingSystemInfo();
            result.Name = operatingSystemName;
            result.ShortName = operatingSystemShortName;
            result.TotalVisibleMemorySize = totalVisibleMemorySize;
            result.ProcessorName = processorName;
            result.CpuHz = cpuHz;
           
            return result;
        }
    }
}
