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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AvePoint.Wrapper.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace AvePoint.Wrapper.Common
{
    class WrapperWin32Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary(string dllname);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);


        internal static void LoadAssemblyForAuthentication()
        {
            string idcrlPath = IntPtr.Size == 8 ? @"Office365\x64\MSOIDCLIL.DLL" : @"Office365\x86\MSOIDCLIL.DLL";
            LoadLibrary(Path.Combine(AvePoint.Common.AveEnv.AgentBinFolder,idcrlPath));
        }
    }

    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Methods
        private SafeLibraryHandle()
            : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            return WrapperWin32Native.FreeLibrary(base.handle);
        }
    }
}
