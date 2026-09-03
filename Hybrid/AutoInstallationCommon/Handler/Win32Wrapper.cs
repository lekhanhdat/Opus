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
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace AutoInstallationCommon.Utility
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public uint dwOemId { get; set; }
        public uint dwPageSize { get; set; }
        public uint lpMinimumApplicationAddress { get; set; }
        public uint lpMaximumApplicationAddress { get; set; }
        public uint dwActiveProcessorMask { get; set; }
        public uint dwNumberOfProcessors { get; set; }
        public uint dwProcessorType { get; set; }
        public uint dwAllocationGranularity { get; set; }
        public uint dwProcessorLevel { get; set; }
        public uint dwProcessorRevision { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class OSVERSIONINFO
    {
        private int _dwMajorVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string _szCSDVersion;

        public int dwOSVersionInfoSize { get; set; }

        public int dwMajorVersion
        {
            get { return _dwMajorVersion; }
            set { dwMajorVersion = value; }
        }

        public int dwMinorVersion { get; set; }

        public int dwBuildNumber { get; set; }

        public int dwPlatformId { get; set; }

        public string szCSDVersion
        {
            get { return _szCSDVersion; }
            set { _szCSDVersion = value; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class OSVERSIONINFOEX : OSVERSIONINFO
    {
        public short wServicePackMajor { get; set; }

        public short wServicePackMinor { get; set; }

        public short wSuiteMask { get; set; }

        public byte wProductType { get; set; }

        public byte wReserved { get; set; }
    }

    public class Win32Wrapper
    {
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_NETWORK = 3;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public const int LOGON32_PROVIDER_DEFAULT = 3;
        public const int LOGON32_PROVIDER_WINNT50 = 3;
        public const int VER_PLATFORM_WIN32s = 0;
        public const int VER_PLATFORM_WIN32_WINDOWS = 1;
        public const int VER_PLATFORM_WIN32_NT = 2;

        public const int VER_NT_WORKSTATION = 1;
        public const int VER_NT_DOMAIN_CONTROLLER = 2;
        public const int VER_NT_SERVER = 3;

        // Microsoft Small Business Server 
        public const int VER_SUITE_SMALLBUSINESS = 1;

        // Win2k,2003,2008 Adv Server or .Net Enterprise Server 
        public const int VER_SUITE_ENTERPRISE = 2;

        // Terminal Services is installed.   
        public const int VER_SUITE_TERMINAL = 16;

        // Win2k Datacenter 
        public const int VER_SUITE_DATACENTER = 128;

        // Terminal server in remote admin mode 
        public const int VER_SUITE_SINGLEUSERTS = 256;

        // Vista Home,Basic , XP home
        public const int VER_SUITE_PERSONAL = 512;

        // Microsoft .Net webserver installed 
        public const int VER_SUITE_BLADE = 1024;

        // The build number if the system is Windows Server 2003 R2; otherwise, 0.
        public const int SM_SERVERR2 = 89;

        // Windows Home Server is installed.
        public const int VER_SUITE_WH_SERVER = 0x00008000;

        // x64 (AMD or Intel)
        public const int PROCESSOR_ARCHITECTURE_AMD64 = 9;

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_RESTORE_PRIVILEGE = "SeRestorePrivilege";

        [DllImport("kernel32.dll")]
        public static extern bool GetVersionEx([In] [Out] OSVERSIONINFO versionInfo);

        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(ref SYSTEM_INFO systemInfo);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetSystemMetrics(int index);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool LogonUserW(string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        public static void GiveRestorePrivilege()
        {
            TOKEN_PRIVILEGES tokenPrivileges;
            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Luid = new LUID();
            tokenPrivileges.Attributes = SE_PRIVILEGE_ENABLED;

            var tokenHandle = RetrieveProcessToken();

            try
            {
                var success = LookupPrivilegeValue
                    (null, SE_RESTORE_PRIVILEGE, ref tokenPrivileges.Luid);
                if (success == false)
                {
                    var lastError = Marshal.GetLastWin32Error();
                    // throw new Exception(
                    // string.Format(LOGRESX.DPM_WFEFindPrivilegeError, SE_RESTORE_PRIVILEGE, lastError));
                }

                success = AdjustTokenPrivileges(
                    tokenHandle, false,
                    ref tokenPrivileges, 0,
                    IntPtr.Zero, IntPtr.Zero);
                if (success == false)
                {
                    var lastError = Marshal.GetLastWin32Error();
                    //throw new Exception(
                    // string.Format(LOGRESX.DPM_WFEAssignPrivilegeError, SE_RESTORE_PRIVILEGE, lastError));
                }
            }
            catch (Exception ex)
            {
                //mLog.Warn(ex.Message);
            }
            finally
            {
                CloseHandle(tokenHandle);
            }
        }

        private static IntPtr RetrieveProcessToken()
        {
            var processHandle = GetCurrentProcess();
            var tokenHandle = IntPtr.Zero;
            var success = OpenProcessToken(processHandle,
                TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                ref tokenHandle);
            if (success == false)
            {
                var lastError = Marshal.GetLastWin32Error();
                //throw new Exception(
                //string.Format(LOGRESX.DPM_WFERetrieveProcessTokeNError, lastError));
            }

            return tokenHandle;
        }

        public static void SetAccount(string path, string username)
        {
            //FileInfo fileInfo = new FileInfo(filePath);

            //FileSecurity fileSecurity = fileInfo.GetAccessControl();

            //fileSecurity.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.FullControl, AccessControlType.Allow));     //以完全控制为例

            //fileInfo.SetAccessControl(fileSecurity);
            var inherits = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            bool ret;
            var folder = new DirectoryInfo(path);

            var dSecurity = folder.GetAccessControl(AccessControlSections.All);

            var accRule = new FileSystemAccessRule(username, FileSystemRights.FullControl, inherits,
                PropagationFlags.InheritOnly, AccessControlType.Allow);
            dSecurity.ModifyAccessRule(AccessControlModification.Add, accRule, out ret);

            folder.SetAccessControl(dSecurity);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        #region DLLImport

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool CloseHandle(IntPtr hObject);

        // Use this signature if you do not want the previous state
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
            ref TOKEN_PRIVILEGES newState,
            uint bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken
            (IntPtr processHandle, int desiredAccess, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue
            (string host, string name, ref LUID lpLuid);

        #endregion
    }
}