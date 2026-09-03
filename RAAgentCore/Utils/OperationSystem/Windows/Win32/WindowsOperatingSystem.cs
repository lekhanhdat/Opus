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



namespace  AvePoint.Hybrid.Utility.OperationSystem.Windows.Win32
{
    #region using directives
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using AvePoint.Hybrid.Utility.Native;

    #endregion

    internal class WindowsOperatingSystem
        : WindowsOperatingSystemBase
        , IOperatingSystem
    {
        const Int32 VER_PLATFORM_WIN32s = 0;
        const Int32 VER_PLATFORM_WIN32_WINDOWS = 1;
        const Int32 VER_PLATFORM_WIN32_NT = 2;

        const Int32 VER_NT_WORKSTATION = 1;
        const Int32 VER_NT_DOMAIN_CONTROLLER = 2;
        const Int32 VER_NT_SERVER = 3;

        // Microsoft Small Business Server 
        const Int32 VER_SUITE_SMALLBUSINESS = 1;
        // Win2k Adv Server or .Net Enterprise Server 
        const Int32 VER_SUITE_ENTERPRISE = 2;
        // Terminal Services is installed.   
        const Int32 VER_SUITE_TERMINAL = 16;
        // Win2k Datacenter 
        const Int32 VER_SUITE_DATACENTER = 128;
        // Terminal server in remote admin mode 
        const Int32 VER_SUITE_SINGLEUSERTS = 256;
        const Int32 VER_SUITE_PERSONAL = 512;
        // Microsoft .Net webserver installed 
        const Int32 VER_SUITE_BLADE = 1024;

        public OperatingSystemInfo GetOSInfo()
        {
            var result = new OperatingSystemInfo();
            result.Name = this.GetWindowsDispalyName();
            result.ShortName = result.Name;
            result.CpuHz = this.GetCPUFrequency();
            result.ProcessorName = this.GetCPUName();
            result.TotalVisibleMemorySize = this.GetTotalMemory();
            return result;
        }

        Int64 GetTotalMemory()
        {
            var memSt = new Win32Native.MEMORYSTATUS();
            Win32Native.GlobalMemoryStatus(ref memSt);
            return memSt.DwTotalPhys;
        }

        public UInt64 GetLeftMemoryEx()
        {
            var memSt = new Win32Native.MEMORYSTATUSEX();
            if (Win32Native.GlobalMemoryStatusEx(memSt))
            {
                return memSt.AvailPhys;
            }
            throw new Win32Exception(Win32Native.GetErrorMessage(Marshal.GetLastWin32Error())); 
        }

        String GetCPUName()
        {
            var rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return Convert.ToString(rk.GetValue("ProcessorNameString"));
        }


        UInt32 GetCPUFrequency()
        {
            var rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return Convert.ToUInt32(rk.GetValue("~MHz"));
        }


        String GetWindowsDispalyName()
        {
            var displayName = String.Empty;
            Win32Native.OSVERSIONINFO versionInfo = new Win32Native.OSVERSIONINFOEX();
            versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
            var versionInfoEx = Win32Native.GetVersionEx(versionInfo);
            if (!versionInfoEx)
            {
                versionInfo = new Win32Native.OSVERSIONINFO();
                versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
                if (!Win32Native.GetVersionEx(versionInfo))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            switch (versionInfo.dwPlatformId)
            {
                case VER_PLATFORM_WIN32s:
                case VER_PLATFORM_WIN32_WINDOWS:
                    displayName = "Unknown System ";
                    break;
                case VER_PLATFORM_WIN32_NT:
                    displayName = this.GetWin32NtOSDisplayName(versionInfo, versionInfoEx);
                    break;
            }
            displayName += versionInfo.szCSDVersion; //service pack X?
            return displayName;
        }

        String GetWin32NtOSDisplayName(Win32Native.OSVERSIONINFO versionInfo, Boolean versionInfoEx)
        {
            var displayName = String.Empty;
            if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 1)
            {
                if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                    displayName = "Microsoft Windows 7 ";
                else if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                    displayName = "Microsoft Windows Server 2008 R2 ";
            }
            else if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 0)
            {
                if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                    displayName = "Microsoft Windows Vista ";
                else if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                    displayName = "Microsoft Windows Server 2008 ";
            }
            else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 2)
                displayName = "Microsoft Windows Server 2003 ";
            else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 1)
                displayName = "Microsoft Windows XP ";
            else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 0)
                displayName = "Microsoft Windows 2000 ";

            if (versionInfoEx)
            {
                displayName += this.GetSuiteDisplayName(versionInfo);
            }
            return displayName;
        }

        String GetSuiteDisplayName(Win32Native.OSVERSIONINFO versionInfo)
        {
            string displayName = String.Empty;
            if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
            {
                if (versionInfo.dwMajorVersion == 4)
                    displayName += "Workstation 4.0 ";
                else if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
                    displayName += "Home Edition ";
                else
                    displayName += "Professional Edition ";
            }
            else if (((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_SERVER || ((Win32Native.OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_DOMAIN_CONTROLLER)
            {
                if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 2)
                {
                    if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                        displayName += "DataCenter Edition ";
                    else if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                        displayName += "Enterprise Edition ";
                    else if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
                        displayName += "Web Edition ";
                    else
                        displayName += "Standard Edition ";
                }
                else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 0)
                {
                    if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                        displayName += "DataCenter Server ";
                    else if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                        displayName += "Advanced Server ";
                    else displayName += "Server ";
                }
                else
                {
                    if (Is64BitOperatingSystem)
                    {
                        if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                            displayName += "Enterprise x64 Edition ";
                        else displayName += "Standard x64 Edition ";
                    }
                    else
                    {
                        if ((((Win32Native.OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                            displayName += "Enterprise Edition ";
                        else displayName += "Standard Edition ";
                    }
                }
            }
            return displayName;
        }
    }
}
