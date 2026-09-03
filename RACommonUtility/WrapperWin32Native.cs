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
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AvePoint.RA.RACommonUtility
{
    public class WrapperWin32Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary(string dllname);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);


        internal static void LoadAssemblyForAuthentication(bool forSharePointColumn = false)
        {
           
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            Process processes = Process.GetCurrentProcess();
            string pName = processes.MainModule.FileName;
            if (!pName.Contains("RevIMScheduleJob.exe") && !forSharePointColumn)
            {
                path += "bin";
            }
            string idcrlPath = IntPtr.Size == 8 ? @"Office365\x64\MSOIDCLIL.DLL" : @"Office365\x86\MSOIDCLIL.DLL";
            LoadLibrary(Path.Combine(path, idcrlPath));
            string idresPath = IntPtr.Size == 8 ? @"Office365\x64\MSOIDRES.DLL" : @"Office365\x86\MSOIDRES.DLL";
            LoadLibrary(Path.Combine(path, idresPath));
        }
        
        //internal static void LoadAssemblyRunTime()
        //{
        //    string path = System.AppDomain.CurrentDomain.BaseDirectory + @"bin\2013";
        //    string idcrlPath = "Microsoft.SharePoint.Client.dll";
        //    string runTimePath = "Microsoft.SharePoint.Client.Runtime.dll";
        //    LoadLibrary(Path.Combine(path, idcrlPath));
        //    LoadLibrary(Path.Combine(path, runTimePath));
        //}
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
