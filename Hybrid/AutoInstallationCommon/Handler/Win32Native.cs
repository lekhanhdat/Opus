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
using System.Text;

namespace AutoInstallationCommon.Utility
{
    #region using directives

    #endregion

    /// <summary>
    ///     This class is a win32 API wrapper class for P/I
    /// </summary>
    internal static class Win32Native
    {
        #region --USER32.DLL

        [DllImport("user32.dll", EntryPoint = "GetGuiResources")]
        public static extern uint GetGuiResources([In] IntPtr hProcess, uint uiFlags);

        #endregion

        #region -- Constants --

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_NETWORK = 3;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_SERVICE = 5;
        public const int LOGON32_LOGON_UNLOCK = 7;
        public const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public const int LOGON32_PROVIDER_DEFAULT = 0;
        public const int LOGON32_PROVIDER_WINNT35 = 1;
        public const int LOGON32_PROVIDER_WINNT40 = 2;
        public const int LOGON32_PROVIDER_WINNT50 = 3;

        public const int SECURITY_IMPERSONATION_LEVEL_ANONYMOUS = 0;
        public const int SECURITY_IMPERSONATION_LEVEL_IDENTIFICATION = 1;
        public const int SECURITY_IMPERSONATION_LEVEL_IMPERSONATION = 2;
        public const int SECURITY_IMPERSONATION_LEVEL_DELEGATION = 3;

        #endregion

        #region -- ADVAPI32.DLL --

        public enum SID_NAME_USE
        {
            SidTypeAlias = 4,
            SidTypeComputer = 9,
            SidTypeDeletedAccount = 6,
            SidTypeDomain = 3,
            SidTypeGroup = 2,
            SidTypeInvalid = 7,
            SidTypeUnknown = 8,
            SidTypeUser = 1,
            SidTypeWellKnownGroup = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        public enum LSA_AccessPolicy : long
        {
            POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
            POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
            POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
            POLICY_TRUST_ADMIN = 0x00000008L,
            POLICY_CREATE_ACCOUNT = 0x00000010L,
            POLICY_CREATE_SECRET = 0x00000020L,
            POLICY_CREATE_PRIVILEGE = 0x00000040L,
            POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
            POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
            POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
            POLICY_SERVER_ADMIN = 0x00000400L,
            POLICY_LOOKUP_NAMES = 0x00000800L,
            POLICY_NOTIFICATION = 0x00001000L
        }

        [DllImport("advapi32.dll")]
        public static extern uint LsaNtStatusToWinError(uint status);

        [DllImport("advapi32.dll")]
        public static extern IntPtr FreeSid(IntPtr pSid);

        [DllImport("advapi32.dll")]
        public static extern uint LsaClose(IntPtr ObjectHandle);

        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            int DesiredAccess,
            out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaAddAccountRights(IntPtr PolicyHandle,
            IntPtr AccountSid,
            LSA_UNICODE_STRING[] UserRights,
            uint CountOfRights);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountSid(string SystemName,
            byte[] bSid,
            StringBuilder Name,
            ref int cbName,
            StringBuilder DomainName,
            ref int cbDomainName,
            ref SID_NAME_USE peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountName(string SystemName,
            StringBuilder Name,
            byte[] bSid,
            ref int cbName,
            StringBuilder DomainName,
            ref int cbDomainName,
            ref SID_NAME_USE peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountName(string SystemName,
            string Name,
            IntPtr psid,
            ref int cbName,
            StringBuilder DomainName,
            ref int cbDomainName,
            ref int peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ConvertSidToStringSid(byte[] sid, out StringBuilder stringSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LogonUser(string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LogonUserW(string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateTokenEx(IntPtr tokenHandle,
            int dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            int SECURITY_IMPERSONATION_LEVEL,
            int TOKEN_TYPE,
            ref IntPtr dupeTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcessAsUserW(IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
        public static extern int RegOpenKeyEx(IntPtr hKey,
            string subKey,
            uint options,
            int sam,
            out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(IntPtr hKey,
            string lpValueName,
            IntPtr lpReserved,
            out uint lpType,
            StringBuilder lpData,
            ref uint lpcbData);

        [DllImport("advapi32.dll", EntryPoint = "OpenEventLog")]
        public static extern IntPtr OpenEventLog(string lpUNCServerName, string lpSourceName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool BackupEventLog(IntPtr hEventLog, string backupFile);

        [DllImport("advapi32.dll", EntryPoint = "CloseEventLog")]
        public static extern bool CloseEventLog(IntPtr hEventLog);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int ImpersonateLoggedOnUser(IntPtr hToken);

        #endregion

        #region -- KERNEL32.DLL --

        [StructLayout(LayoutKind.Sequential)]
        public class OSVERSIONINFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            private string _szCSDVersion;

            public int dwOSVersionInfoSize { get; set; }

            public int dwMajorVersion { get; set; }

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
        public struct MEMORYSTATUS
        {
            public uint DwLength { get; set; }

            public uint DwMemoryLoad { get; set; }

            public uint DwTotalPhys { get; set; }

            public uint DwAvailPhys { get; set; }

            public uint DwTotalPageFile { get; set; }

            public uint DwAvailPageFile { get; set; }

            public uint DwTotalVirtual { get; set; }

            public uint DwAvailVirtual { get; set; }
        }

        [DllImport("kernel32.dll")]
        public static extern bool GetVersionEx([In] [Out] OSVERSIONINFO versionInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        [DllImport("Kernel32.dll")]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, ref uint lpExitCode);

        [DllImport("Kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORYSTATUS buf);

        #endregion

        #region --NETAPI32.DLL--

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHARE_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)] private string _shi2_netname;

            public string shi2_netname
            {
                get { return _shi2_netname; }
                set { _shi2_netname = value; }
            }

            public uint shi2_type { get; set; }

            [MarshalAs(UnmanagedType.LPWStr)] private string _shi2_remark;

            public string shi2_remark
            {
                get { return _shi2_remark; }
                set { _shi2_remark = value; }
            }

            public uint shi2_permissions { get; set; }

            public uint shi2_max_uses { get; set; }

            public uint shi2_current_uses { get; set; }

            [MarshalAs(UnmanagedType.LPWStr)] private string _shi2_path;

            public string shi2_path
            {
                get { return _shi2_path; }
                set { _shi2_path = value; }
            }

            [MarshalAs(UnmanagedType.LPWStr)] private string _shi2_passwd;

            public string shi2_passwd
            {
                get { return _shi2_passwd; }
                set { _shi2_passwd = value; }
            }
        }

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        public static extern int NetShareGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername,
            [MarshalAs(UnmanagedType.LPWStr)] string netname,
            int level,
            ref IntPtr bufptr);

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        internal static extern int NetApiBufferFree(IntPtr Buffer);

        #endregion

        #region -- MPR.DLL--

        [StructLayout(LayoutKind.Sequential)]
        public struct NETRESOURCEW
        {
            public int dwScope { get; set; }

            public int dwType { get; set; }

            public int dwDisplayType { get; set; }

            public int dwUsage { get; set; }

            [MarshalAs(UnmanagedType.LPWStr)] private string _lpLocalName;

            public string lpLocalName
            {
                get { return _lpLocalName; }
                set { _lpLocalName = value; }
            }

            [MarshalAs(UnmanagedType.LPWStr)] private string _lpRemoteName;

            public string lpRemoteName
            {
                get { return _lpRemoteName; }
                set { _lpRemoteName = value; }
            }

            [MarshalAs(UnmanagedType.LPWStr)] private string _lpComment;

            public string lpComment
            {
                get { return _lpComment; }
                set { _lpComment = value; }
            }

            [MarshalAs(UnmanagedType.LPWStr)] private string _lpProvider;

            public string lpProvider
            {
                get { return _lpProvider; }
                set { _lpProvider = value; }
            }
        }

        [DllImport("mpr.dll")]
        public static extern int WNetAddConnection2W([MarshalAs(UnmanagedType.LPArray)] NETRESOURCEW[] lpNetResource,
            [MarshalAs(UnmanagedType.LPWStr)] string lpPassword,
            [MarshalAs(UnmanagedType.LPWStr)] string UserName,
            int dwFlags);

        [DllImport("mpr.dll")]
        public static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        #endregion
    }
}