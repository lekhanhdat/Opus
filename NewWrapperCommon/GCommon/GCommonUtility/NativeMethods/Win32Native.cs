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



using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", Scope = "member", Target = "AvePoint.GCommon.Win32Native.#FindMimeFromData(System.UInt32,System.String,System.Byte[],System.UInt32,System.String,System.UInt32,System.UInt32&,System.UInt32)", MessageId = "0")]
[module: SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", Scope = "member", Target = "AvePoint.GCommon.Win32Native.#LsaEnumerateAccountsWithUserRight(System.IntPtr,AvePoint.GCommon.Win32Native+LSA_UNICODE_STRING,System.IntPtr&,System.UInt32&)", MessageId = "1")]
namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    #endregion

    /// <summary>
    /// This class is a win32 API wrapper class for P/I
    /// </summary>
    public static class Win32Native
    {
        #region -- Constants --

        public const int MAX_PATH = 260;
        public const ulong MAXDWORD = 0xFFFFFFFF;
        public const int INVALID_HANDLE_VALUE = -1;
        public const int FILE_ATTRIBUTE_ARCHIVE = 0x20;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        public const int FILE_ATTRIBUTE_HIDDEN = 0x2;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_ATTRIBUTE_READONLY = 0x1;
        public const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        public const int FILE_ATTRIBUTE_TEMPORARY = 0x100;
        public const int ERROR_NO_MORE_FILES = 18;
        public const int ERROR_FILE_EXISTS = 0x50;
        public const int ERROR_ALREADY_EXISTS = 0xB7;//183
        public const int ERROR_FILE_NOT_FOUND = 0x2;
        public const int ERROR_NO_ERROR = 0x0;
        public const int ERROR_ACCESS_DENIED = 0x5;
        public const int ERROR_DISK_FULL = 112;
        public const int ERROR_DIR_NOT_EMPTY = 145; //(0x91)

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

        public const int SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;
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
        public class SECURITY_ATTRIBUTES_EXTEND
        {
            [MarshalAs(UnmanagedType.U4)]
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public Boolean bInheritHandle;
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
        public struct LSA_ENUMERATION_INFORMATION
        {
            public IntPtr Sid;
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SERVICE_DELAYED_AUTO_START_INFO
        {
            public bool fDelayedAutostart;
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

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaEnumerateAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, out /* LSA_UNICODE_STRING[]*/ IntPtr EnumerationBuffer, out uint CountReturned);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        public static extern uint LsaEnumerateAccountsWithUserRight(IntPtr PolicyHandle, LSA_UNICODE_STRING UserRights, out /* LSA_ENUMERATION_INFORMATION[]*/ IntPtr EnumerationBuffer, out uint CountReturned);

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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4), Serializable]
        public struct WIN32_FIND_DATAW
        {
            public UInt32 dwFileAttributes;

            public UInt32 ftCreationTime;
            public UInt32 ftCreationTime2;

            public UInt32 ftLastAccessTime;
            public UInt32 ftLastAccessTime2;

            public UInt32 ftLastWriteTime;
            public UInt32 ftLastWriteTime2;

            public UInt32 nFileSizeHigh;

            public UInt32 nFileSizeLow;

            public UInt32 dwReserved0;

            public UInt32 dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 520)]
            public String cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 28)]
            public String cAlternateFileName;
        }

        public enum GET_FILEEX_INFO_LEVELS : uint
        {
            GetFileExInfoStandard,
            GetFileExMaxInfoLevel,
        }

        public delegate CopyProgressResult CopyProgressRoutine(
        long totalFileSize,
        long totalBytesTransferred,
        long streamSize,
        long streamBytesTransferred,
        uint dwStreamNumber,
        CopyProgressCallbackReason dwCallbackReason,
        IntPtr hSourceFile,
        IntPtr hDestinationFile,
        IntPtr lpData);

        public enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        public enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public FileTime ftCreationTime;
            public FileTime ftLastAccessTime;
            public FileTime ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix"), Flags]
        public enum FileAttributes
        {
            Invalid = -1,
            None = 0,
            ReadOnly = 1,
            Hidden = 2,
            System = 4,
            Directory = 16,
            Archive = 32,
            Device = 64,
            Normal = 128,
            Temporary = 256,
            SparseFile = 512,
            ReparsePoint = 1024,
            Compressed = 2048,
            Offline = 4096,
            NotContentIndexed = 8192,
            Encrypted = 16384,
            Virtual = 65536,
        }

        public struct FileTime
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;

            public long AsLong()
            {
                return (long)dwHighDateTime << 32 | dwLowDateTime & (long)uint.MaxValue;
            }

            public DateTime AsDateTime()
            {
                return new DateTime(AsLong(), DateTimeKind.Local);
            }
        }

        [Flags]
        public enum CopyFileFlags : uint
        {
            None = 0x0,
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_RESTARTABLE = 0x00000002,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
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

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORYSTATUS buf);

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll ", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)]     string path, [MarshalAs(UnmanagedType.LPTStr)]     StringBuilder shortPath, int shortPathLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW([MarshalAs(UnmanagedType.LPWStr)] String lpFileName, [In, Out] ref WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean FindNextFileW(IntPtr hFindFile, [In, Out] ref WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int CopyFileW([MarshalAs(UnmanagedType.LPWStr)] String lpExistingFileName, [MarshalAs(UnmanagedType.LPWStr)] String lpNewFileName, int bFailIfExists);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int FindClose(IntPtr handleFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CreateDirectoryW([MarshalAs(UnmanagedType.LPWStr)]String directoryName, [MarshalAs(UnmanagedType.LPStruct)] SECURITY_ATTRIBUTES_EXTEND securityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveDirectoryW([In] string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesExW(string lpFileName, [MarshalAs(UnmanagedType.U4), In] GET_FILEEX_INFO_LEVELS fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetFileAttributesW(string lpFileName, FileAttributes dwFileAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CopyFileExW(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFileW(string lpFileName);

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

        #region -- SHLWAPI.DLL--
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PathIsNetworkPath(string path);
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
    }
}