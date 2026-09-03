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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AvePoint.GCommon.Utility
{
    public class ServiceHelper
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ChangeServiceConfig(
            SafeHandle serviceHandle,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr dwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword,
            string lpDisplayName);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool QueryServiceConfig(SafeHandle hService,
            IntPtr lpServiceConfig,
            int cbBufSize,
            out int pcbBytesNeeded);
        
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;

        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        /// <summary>
        /// Change the start type of a windows service
        /// </summary>
        /// <param name="serviceHandle">ServiceController.ServiceHandle</param>
        /// <param name="mode">(uint)ServiceStartMode.Manual</param>
        public static void ChangeStartType(SafeHandle serviceHandle, uint mode)
        {
            IntPtr nTag = IntPtr.Zero;
            if (!ChangeServiceConfig(serviceHandle, SERVICE_NO_CHANGE, mode, SERVICE_NO_CHANGE,
                                        null, null, nTag, null, null, null, null))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceHandle"></param>
        /// <returns></returns>
        public static QUERY_SERVICE_CONFIG GetServiceConfig(SafeHandle serviceHandle)
        {
            int neededBytes = 0;

            bool result = QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out neededBytes);
            int win32err = Marshal.GetLastWin32Error();
            if (win32err == ERROR_INSUFFICIENT_BUFFER) //122
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.AllocCoTaskMem(neededBytes);
                    result = QueryServiceConfig(serviceHandle, ptr, neededBytes, out neededBytes);
                    if (result)
                    {
                        QUERY_SERVICE_CONFIG config = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, typeof(QUERY_SERVICE_CONFIG));
                        return config;
                    }
                    else
                    {
                        win32err = Marshal.GetLastWin32Error();
                        throw new Win32Exception(win32err, "QueryServiceConfig failed");
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
            else
            {
                throw new Win32Exception(win32err, "QueryServiceConfig failed");
            }
        }
    }
    /// <summary>
    /// Result struct for query service configuration
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct QUERY_SERVICE_CONFIG
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwServiceType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwStartType;
        public int dwErrorControl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpBinaryPathName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpLoadOrderGroup;
        public int dwTagId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpDependencies;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpServiceStartName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpDisplayName;
    }
}
