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




namespace  AvePoint.Hybrid.Utility.Native
{
    #region using directives
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    #endregion

    /// <summary>
    /// This class is a win32 API wrapper class for P/I
    /// </summary>
    internal static class Win32Native
    {
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
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
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
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public UInt32 Attributes;
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
        public static extern UInt32 LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, Int32 DesiredAccess, out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, uint CountOfRights);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountSid(string SystemName, byte[] bSid, StringBuilder Name, ref int cbName, StringBuilder DomainName, ref int cbDomainName, ref SID_NAME_USE peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountName(string SystemName, StringBuilder Name, byte[] bSid, ref int cbName, StringBuilder DomainName, ref int cbDomainName, ref SID_NAME_USE peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountName(string SystemName, string Name, IntPtr psid, ref int cbName, StringBuilder DomainName, ref int cbDomainName, ref  int peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ConvertSidToStringSid(byte[] sid, out StringBuilder stringSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LogonUserW(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateTokenEx(IntPtr tokenHandle, int dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes, int SECURITY_IMPERSONATION_LEVEL, int TOKEN_TYPE, ref IntPtr dupeTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcessAsUserW(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern Int32 RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
        public static extern Int32 RegOpenKeyEx(IntPtr hKey, String subKey, UInt32 options, Int32 sam, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern Int32 RegQueryValueEx(IntPtr hKey, String lpValueName, IntPtr lpReserved, out UInt32 lpType, StringBuilder lpData, ref UInt32 lpcbData);

        [DllImport("advapi32.dll", EntryPoint = "OpenEventLog")]
        public static extern IntPtr OpenEventLog(string lpUNCServerName, String lpSourceName);

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
            private Int32 _dwOSVersionInfoSize;

            public Int32 dwOSVersionInfoSize
            {
                get { return _dwOSVersionInfoSize; }
                set { _dwOSVersionInfoSize = value; }
            }
            private Int32 _dwMajorVersion;

            public Int32 dwMajorVersion
            {
                get { return _dwMajorVersion; }
                set { _dwMajorVersion = value; }
            }
            private Int32 _dwMinorVersion;

            public Int32 dwMinorVersion
            {
                get { return _dwMinorVersion; }
                set { _dwMinorVersion = value; }
            }
            private Int32 _dwBuildNumber;

            public Int32 dwBuildNumber
            {
                get { return _dwBuildNumber; }
                set { _dwBuildNumber = value; }
            }
            private Int32 _dwPlatformId;

            public Int32 dwPlatformId
            {
                get { return _dwPlatformId; }
                set { _dwPlatformId = value; }
            }
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            private String _szCSDVersion;

            public String szCSDVersion
            {
                get { return _szCSDVersion; }
                set { _szCSDVersion = value; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class OSVERSIONINFOEX : OSVERSIONINFO
        {
            private Int16 _wServicePackMajor;

            public Int16 wServicePackMajor
            {
                get { return _wServicePackMajor; }
                set { _wServicePackMajor = value; }
            }
            private Int16 _wServicePackMinor;

            public Int16 wServicePackMinor
            {
                get { return _wServicePackMinor; }
                set { _wServicePackMinor = value; }
            }
            private Int16 _wSuiteMask;

            public Int16 wSuiteMask
            {
                get { return _wSuiteMask; }
                set { _wSuiteMask = value; }
            }
            private Byte _wProductType;

            public Byte wProductType
            {
                get { return _wProductType; }
                set { _wProductType = value; }
            }
            private Byte _wReserved;

            public Byte wReserved
            {
                get { return _wReserved; }
                set { _wReserved = value; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUS
        {
            private UInt32 dwLength;

            public UInt32 DwLength
            {
                get { return dwLength; }
                set { dwLength = value; }
            }
            private UInt32 dwMemoryLoad;

            public UInt32 DwMemoryLoad
            {
                get { return dwMemoryLoad; }
                set { dwMemoryLoad = value; }
            }
            private UInt32 dwTotalPhys;

            public UInt32 DwTotalPhys
            {
                get { return dwTotalPhys; }
                set { dwTotalPhys = value; }
            }
            private UInt32 dwAvailPhys;

            public UInt32 DwAvailPhys
            {
                get { return dwAvailPhys; }
                set { dwAvailPhys = value; }
            }
            private UInt32 dwTotalPageFile;

            public UInt32 DwTotalPageFile
            {
                get { return dwTotalPageFile; }
                set { dwTotalPageFile = value; }
            }
            private UInt32 dwAvailPageFile;

            public UInt32 DwAvailPageFile
            {
                get { return dwAvailPageFile; }
                set { dwAvailPageFile = value; }
            }
            private UInt32 dwTotalVirtual;

            public UInt32 DwTotalVirtual
            {
                get { return dwTotalVirtual; }
                set { dwTotalVirtual = value; }
            }
            private UInt32 dwAvailVirtual;

            public UInt32 DwAvailVirtual
            {
                get { return dwAvailVirtual; }
                set { dwAvailVirtual = value; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MEMORYSTATUSEX
        {
            private int length;
            private int memoryLoad;
            private ulong totalPhys;
            private ulong availPhys;
            private ulong totalPageFile;
            private ulong availPageFile;
            private ulong totalVirtual;
            private ulong availVirtual;
            private ulong availExtendedVirtual;

            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int MemoryLoad
            {
                get { return memoryLoad; }
                set { memoryLoad = value; }
            }

            public ulong TotalPhys
            {
                get { return totalPhys; }
                set { totalPhys = value; }
            }

            public ulong AvailPhys
            {
                get { return availPhys; }
                set { availPhys = value; }
            }

            public ulong TotalPageFile
            {
                get { return totalPageFile; }
                set { totalPageFile = value; }
            }

            public ulong AvailPageFile
            {
                get { return availPageFile; }
                set { availPageFile = value; }
            }

            public ulong TotalVirtual
            {
                get { return totalVirtual; }
                set { totalVirtual = value; }
            }

            public ulong AvailVirtual
            {
                get { return availVirtual; }
                set { availVirtual = value; }
            }

            public ulong AvailExtenedVirtual
            {
                get { return availExtendedVirtual; }
                set { availExtendedVirtual = value; }
            }

            internal MEMORYSTATUSEX()
            {
                this.length = Marshal.SizeOf(this);
            }
        }

        [DllImport("kernel32.dll")]
        public extern static Boolean GetVersionEx([In, Out] OSVERSIONINFO versionInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean IsWow64Process(IntPtr hProcess, out Boolean wow64Process);

        [DllImport("Kernel32.dll")]
        public static extern Boolean GetExitCodeProcess(System.IntPtr hProcess, ref uint lpExitCode);

        [DllImport("Kernel32.dll")]
        public static extern uint WaitForSingleObject(System.IntPtr hHandle, uint dwMilliseconds);

        [DllImport("Kernel32.dll")]
        public static extern Boolean QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern Boolean QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, [Out] StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments); 

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORYSTATUS buf);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX buffer);

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll ", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)]     string path, [MarshalAs(UnmanagedType.LPTStr)]     StringBuilder shortPath, int shortPathLength);

        #endregion

        #region --USER32.DLL

        [DllImport("user32.dll", EntryPoint = "GetGuiResources")]
        public static extern UInt32 GetGuiResources([InAttribute()] IntPtr hProcess, UInt32 uiFlags);

        #endregion

        #region --NETAPI32.DLL--

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHARE_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_netname;

            public string shi2_netname
            {
                get { return _shi2_netname; }
                set { _shi2_netname = value; }
            }
            private uint _shi2_type;

            public uint shi2_type
            {
                get { return _shi2_type; }
                set { _shi2_type = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_remark;

            public string shi2_remark
            {
                get { return _shi2_remark; }
                set { _shi2_remark = value; }
            }
            private uint _shi2_permissions;

            public uint shi2_permissions
            {
                get { return _shi2_permissions; }
                set { _shi2_permissions = value; }
            }
            private uint _shi2_max_uses;

            public uint shi2_max_uses
            {
                get { return _shi2_max_uses; }
                set { _shi2_max_uses = value; }
            }
            private uint _shi2_current_uses;

            public uint shi2_current_uses
            {
                get { return _shi2_current_uses; }
                set { _shi2_current_uses = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_path;

            public string shi2_path
            {
                get { return _shi2_path; }
                set { _shi2_path = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_passwd;

            public string shi2_passwd
            {
                get { return _shi2_passwd; }
                set { _shi2_passwd = value; }
            }
        }

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        public static extern int NetShareGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string netname, int level, ref IntPtr bufptr);

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        internal static extern int NetApiBufferFree(IntPtr Buffer);

        #endregion

        #region -- MPR.DLL--

        [StructLayout(LayoutKind.Sequential)]
        public struct NETRESOURCEW
        {
            private int _dwScope;

            public int dwScope
            {
                get { return _dwScope; }
                set { _dwScope = value; }
            }
            private int _dwType;

            public int dwType
            {
                get { return _dwType; }
                set { _dwType = value; }
            }
            private int _dwDisplayType;

            public int dwDisplayType
            {
                get { return _dwDisplayType; }
                set { _dwDisplayType = value; }
            }
            private int _dwUsage;

            public int dwUsage
            {
                get { return _dwUsage; }
                set { _dwUsage = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpLocalName;

            public string lpLocalName
            {
                get { return _lpLocalName; }
                set { _lpLocalName = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpRemoteName;

            public string lpRemoteName
            {
                get { return _lpRemoteName; }
                set { _lpRemoteName = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpComment;

            public string lpComment
            {
                get { return _lpComment; }
                set { _lpComment = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpProvider;

            public string lpProvider
            {
                get { return _lpProvider; }
                set { _lpProvider = value; }
            }
        }

        [DllImport("mpr.dll")]
        public static extern int WNetAddConnection2W([MarshalAs(UnmanagedType.LPArray)] NETRESOURCEW[] lpNetResource, [MarshalAs(UnmanagedType.LPWStr)] string lpPassword, [MarshalAs(UnmanagedType.LPWStr)] string UserName, int dwFlags);

        [DllImport("mpr.dll")]
        public static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        #endregion

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        public extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        public static string GetErrorMessage(int errorCode)
        {
            StringBuilder errorMessage = new StringBuilder(0x200);
            if (FormatMessage(0x3200, IntPtr.Zero, errorCode, 0, errorMessage, errorMessage.Capacity, IntPtr.Zero) != 0)
            {
                return errorMessage.ToString();
            }            
            return string.Format("failed to get error message due to error: {0}", Marshal.GetLastWin32Error());
        }
    }
}