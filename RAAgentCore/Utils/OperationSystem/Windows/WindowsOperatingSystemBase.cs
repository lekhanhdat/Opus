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





namespace  AvePoint.Hybrid.Utility.OperationSystem
{
    using AvePoint.Hybrid.Utility.Native;
    #region using directives
    using System;

    #endregion

    internal abstract class WindowsOperatingSystemBase
    {
        public  Boolean Is64BitOperatingSystem { get { return IntPtr.Size == 8 ? 1 < 2 : IsWow64Process; } }

        /// <summary>
        /// To judge if the current process is a wow64 process.
        /// </summary>
        /// <returns>the check result</returns>
        public  Boolean IsWow64Process
        {
            get
            {
                // 32-bit programs run on both 32-bit and 64-bit Windows
                // Detect whether the current process is a 32-bit process
                // running on a 64-bit system.
                var result = default(Boolean);
                var currentProcessPtr = Win32Native.GetCurrentProcess();
                result = (DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && Win32Native.IsWow64Process(currentProcessPtr, out result)) && result;
                Win32Native.CloseHandle(currentProcessPtr);
                return result;
            }
        }

        Boolean DoesWin32MethodExist(String moduleName, String methodName)
        {
            var result = default(Boolean);
            var moduleHandle = Win32Native.GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
                result = false;
            else
            {
                var processHandle = Win32Native.GetProcAddress(moduleHandle, methodName);
                result = processHandle != IntPtr.Zero;
                Win32Native.CloseHandle(moduleHandle);
                Win32Native.CloseHandle(processHandle);
            }
            return result;
        }
    }
}
