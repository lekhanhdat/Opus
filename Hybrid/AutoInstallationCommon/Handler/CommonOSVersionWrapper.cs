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
using System.Runtime.InteropServices;

namespace AutoInstallationCommon.Utility
{
    public class CommonOSVersionWrapper
    {
        private readonly CommonRegistryWrapper registryWrapper = CommonRegistryWrapper.GetInstance();
        private OSVERSIONINFO _versionInfo = new OSVERSIONINFOEX();

        /// <summary>
        ///     获取操作系统版本
        /// </summary>
        /// <returns></returns>
        public OSVersion GetOSVersionHandler()
        {
            var windowsVersion = OSVersion.Unknown;

            _versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(_versionInfo);
            var bVersionInfoEx = Win32Wrapper.GetVersionEx(_versionInfo);

            var systemInfo = new SYSTEM_INFO();
            Win32Wrapper.GetSystemInfo(ref systemInfo);

            if (!bVersionInfoEx)
            {
                _versionInfo = new OSVERSIONINFO();
                _versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(_versionInfo);
                var success = Win32Wrapper.GetVersionEx(_versionInfo);

                if (!success) return OSVersion.Unknown;
            }

            windowsVersion = ScanOSVersion(_versionInfo, systemInfo);


            return windowsVersion;
        }

        private OSVersion ScanOSVersion(OSVERSIONINFO versionInfo, SYSTEM_INFO systemInfo)
        {
            var windowsVersion = OSVersion.Unknown;
            //versionInfo.dwMinorVersion = 2;
            //if (IsWindows8OrAbove(versionInfo))
            //{
            //    windowsVersion = OSVersion.Windows8OrAbove;
            //}
            if (IsWindows8OrWindows2012(versionInfo))
                windowsVersion = GetWindowsServer2012OrWindows8Version(versionInfo);
            else if (IsWindows7OrWindowsServer2008R2(versionInfo))
                windowsVersion = GetWindows7OrWindowsServer2008R2Version(versionInfo);
            else if (IsWindows2008OrWindowsVista(versionInfo))
                windowsVersion = GetWindowsServer2008OrVistaVersion(versionInfo);
            else if (IsWindows2003OrHomeServerOrWindowsXP64Bit(versionInfo))
                windowsVersion = GetWindow2003OrHomeServerOrWindowsXPVersion(versionInfo, systemInfo);
            else if (IsWindowsXP(versionInfo))
                windowsVersion = OSVersion.WindowsXP;
            else if (IsWindows2000(versionInfo)) windowsVersion = OSVersion.Windows2000;

            return windowsVersion;
        }

        private OSVersion GetWindow2003OrHomeServerOrWindowsXPVersion(OSVERSIONINFO versionInfo, SYSTEM_INFO systemInfo)
        {
            var windowsVersion = OSVersion.Unknown;
            if (Win32Wrapper.GetSystemMetrics(Win32Wrapper.SM_SERVERR2) != 0)
                windowsVersion = OSVersion.WindowsServer2003R2;
            else if (Win32Wrapper.GetSystemMetrics(Win32Wrapper.SM_SERVERR2) == 0)
                windowsVersion = OSVersion.WindowsServer2003;
            if (Convert.ToBoolean(((OSVERSIONINFOEX) versionInfo).wSuiteMask & Win32Wrapper.VER_SUITE_WH_SERVER))
                windowsVersion = OSVersion.WindowsHomeServer;
            if (((OSVERSIONINFOEX) versionInfo).wProductType == Win32Wrapper.VER_NT_WORKSTATION &&
                systemInfo.dwOemId == Win32Wrapper.PROCESSOR_ARCHITECTURE_AMD64)
                windowsVersion = OSVersion.WindowsXPProfessionalX64Edition;
            return windowsVersion;
        }

        private OSVersion GetWindowsServer2008OrVistaVersion(OSVERSIONINFO versionInfo)
        {
            var windowsVersion = OSVersion.Unknown;
            if (((OSVERSIONINFOEX) versionInfo).wProductType != Win32Wrapper.VER_NT_WORKSTATION)
                windowsVersion = OSVersion.WindowsServer2008;
            else
                windowsVersion = OSVersion.WindowsVista;
            return windowsVersion;
        }

        private OSVersion GetWindows7OrWindowsServer2008R2Version(OSVERSIONINFO versionInfo)
        {
            var windowsVersion = OSVersion.Unknown;
            if (((OSVERSIONINFOEX) versionInfo).wProductType == Win32Wrapper.VER_NT_WORKSTATION)
            {
                windowsVersion = OSVersion.Windows7;
            }
            else
            {
                var installationType =
                    registryWrapper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        "InstallationType");
                if ("Server Core".Equals(installationType, StringComparison.OrdinalIgnoreCase))
                    windowsVersion = OSVersion.WindowsServer2008R2ServerCore;
                else
                    windowsVersion = OSVersion.WindowsServer2008R2;
            }

            return windowsVersion;
        }

        private OSVersion GetWindowsServer2012OrWindows8Version(OSVERSIONINFO versionInfo)
        {
            var windowsVersion = OSVersion.Unknown;
            if (((OSVERSIONINFOEX) versionInfo).wProductType != Win32Wrapper.VER_NT_WORKSTATION)
            {
                var installationType =
                    registryWrapper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        "InstallationType");
                if ("Server Core".Equals(installationType, StringComparison.OrdinalIgnoreCase))
                    windowsVersion = OSVersion.WindowsServer2008R2ServerCore;
                else
                    windowsVersion = OSVersion.WindowsServer2012;
            }
            else
            {
                windowsVersion = OSVersion.Windows8;
            }

            return windowsVersion;
        }

        private static bool IsWindows2000(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 0;
        }

        private static bool IsWindowsXP(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 1;
        }

        private static bool IsWindows2003OrHomeServerOrWindowsXP64Bit(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 2;
        }

        private static bool IsWindows2008OrWindowsVista(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 0;
        }

        private bool IsWindows7OrWindowsServer2008R2(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 1;
        }

        private bool IsWindows8OrAbove(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion >= 6 && versionInfo.dwMinorVersion > 1;
        }

        private bool IsWindows8OrWindows2012(OSVERSIONINFO versionInfo)
        {
            return versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 2;
        }
    }

    public enum OSVersion
    {
        Windows8OrAbove,
        Windows8,
        WindowsServer2012,
        Windows7,
        WindowsServer2008R2,
        WindowsServer2008R2ServerCore,
        WindowsServer2008,
        WindowsVista,
        WindowsServer2003R2,
        WindowsHomeServer,
        WindowsServer2003,
        WindowsXPProfessionalX64Edition,
        WindowsXP,
        Windows2000,
        Unknown
    }
}