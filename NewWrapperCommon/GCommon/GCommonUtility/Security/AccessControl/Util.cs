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
using System.Text;
using System.Runtime.InteropServices;

namespace AvePoint.GCommon.Security.AccessControl
{
    internal static class Util
    {
        internal static string GetErrorMessage(UInt32 errorCode)
        {
            UInt32 FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
            UInt32 FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
            UInt32 FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

            UInt32 dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;

            IntPtr source = new IntPtr();

            string msgBuffer = "";

            UInt32 retVal = FormatMessage(dwFlags, source, errorCode, 0, ref msgBuffer, 512, null);

            return msgBuffer.ToString();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 FormatMessage(UInt32 dwFlags, IntPtr lpSource, UInt32 dwMessageId, UInt32 dwLanguageId, [MarshalAs(UnmanagedType.LPTStr)] ref string lpBuffer, int nSize, IntPtr[] Arguments);

        /*
         * DWORD GetLastError(void);
         */
        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        /*
         * HLOCAL LocalAlloc(
         *     UINT uFlags,
         *     SIZE_T uBytes
         * );
         */
        [DllImport("Kernel32.dll")]
        internal static extern IntPtr LocalAlloc(LocalAllocFlags uFlags, uint uBytes);

        /*
         * HLOCAL LocalFree(
         *     HLOCAL hMem
         * );
         */
        [DllImport("Kernel32.dll")]
        internal static extern IntPtr LocalFree(IntPtr hMem);
    }

    [Flags]
    internal enum LocalAllocFlags
    {
        Fixed = 0x00,
        Moveable = 0x20,
        ZeroInit = 0x40
    }
}
