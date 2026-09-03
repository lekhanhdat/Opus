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

namespace AvePoint.GCommon.Utility.Cryptography
{
    //[StructLayout(LayoutKind.Sequential)]
    //internal struct PROCESS_INFORMATION
    //{
    //    public IntPtr hProcess;
    //    public IntPtr hThread;
    //    public int dwProcessId;
    //    public int dwThreadId;
    //}

    public class MemoryCleaner
    {



        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }


        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          [Out] byte[] lpBuffer,
          IntPtr dwSize,
          out uint lpNumberOfBytesRead
         );
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        static Win32Native.PROCESS_INFORMATION processInfo;

        public static int GetProcessID()
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            return p.Id;
        }
        public static IntPtr Open(int processId)
        {
            processInfo = new Win32Native.PROCESS_INFORMATION();
            IntPtr hProcess = IntPtr.Zero;
            hProcess = OpenProcess(ProcessAccessFlags.All, false, processId);
            if (hProcess == IntPtr.Zero)
                throw new Exception("OpenProcessFailed");
            processInfo.hProcess = hProcess;
            processInfo.dwProcessId = (uint)processId;
            return hProcess;
        }
        public static int WriteMemory(IntPtr addressBase, byte[] writeBytes, IntPtr writeLength)
        {
            int reallyWriteLength = 0;
            if (!WriteProcessMemory(processInfo.hProcess, addressBase, writeBytes, writeLength, out reallyWriteLength))
            {
                //throw new Exception();
            }
            return reallyWriteLength;
        }

    }


}
