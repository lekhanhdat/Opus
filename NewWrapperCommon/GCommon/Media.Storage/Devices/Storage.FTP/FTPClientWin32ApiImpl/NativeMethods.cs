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




namespace AvePoint.Media.Storage.FTP
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Text;    

    /// <summary>
    /// Static class containing all PInvoke methods for WinInet API
    /// </summary>
    [CompilerGeneratedAttribute]
    static class NativeMethods
    {   
        private const uint FormatMessageFromSystem = 4096;
        private const uint FormatMessageIgnoreInserts = 512;
        public const int ErrorInternetExtendedError = (InternetErrorBase + 3);
        public const int ErrorNoMoreFiles = 18;
        public const int FileAttributeDirectory = 16;
        public const int FileAttributeNormal = 128;
        public const int FtpTransferTypeAscii = 0x00000001;
        public const int FtpTransferTypeBinary = 0x00000002;
        public const int FtpTransferTypeUnknown = 0x00000000;
        public const int InternetDefaultFtpPort = 21;
        public const int InternetErrorBase = 12000;
        public const int InternetFlagAsync = 0x10000000;
        public const int InternetFlagFromCache = 0x01000000;
        public const int InternetFlagHyperlink = 0x00000400;
        public const int InternetFlagNeedFile = 0x00000010;
        public const int InternetFlagNoCacheWrite = 0x04000000;
        public const int InternetFlagOffline = 0x01000000;
        public const int InternetFlagPassive = 8;
        public const int InternetFlagReload = 8;
        public const int InternetFlagResynchronize = 0x00000800;
        public const int InternetFlagSync = 0x00000004;
        public const int InternetNoCallback = 0;
        public const int InternetOpenTypeDirect = 1;
        public const int InternetOpenTypePreconfig = 0;
        public const int InternetServiceFtp = 1;
        public const int MaxPath = 260;
        public const int NoError = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public int dwHighDateTime;
            public int dwLowDateTime;

            public DateTime? ToDateTime()
            {
                if (this.dwHighDateTime == 0 && this.dwLowDateTime == 0)
                    return null;

                unchecked
                {
                    uint low = (uint)this.dwLowDateTime;
                    long ft = (((long)this.dwHighDateTime) << 32 | low);
                    return DateTime.FromFileTimeUtc(ft);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct INTERNET_BUFFERS
        {
            public int dwBufferLength;
            public int dwBufferTotal;
            public int dwHeadersLength;
            public int dwHeadersTotal;
            public int dwOffsetHigh;
            public int dwOffsetLow;
            public int dwStructSize;
            public IntPtr lpvBuffer;
            public IntPtr Next;
            public string Header;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int dfFileAttributes;
            public int dwReserved0;
            public int dwReserved1;
            public int nFileSizeHigh;
            public int nFileSizeLow;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPath)]
            public char[] fileName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] alternateFileName;
        }
        
        internal static string TranslateInternetError(uint errorCode)
        {
            IntPtr hModule = IntPtr.Zero;
            try
            {
                StringBuilder buf = new StringBuilder(255);
                hModule = LoadLibrary("wininet.dll");
                if (FormatMessage(FormatMessageFromSystem | FormatMessageIgnoreInserts, hModule, errorCode, 0U, buf, (uint) buf.Capacity + 1, IntPtr.Zero) != 0)
                {
                    return buf.ToString();
                }
                else
                {
                    System.Diagnostics.Debug.Write("Error:: " + Marshal.GetLastWin32Error());
                    return string.Empty;
                }
            }
            finally
            {
                FreeLibrary(hModule);
            }
        }

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern int FreeLibrary(IntPtr hModule);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern uint FormatMessage(uint dwFlags, System.IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)] System.Text.StringBuilder lpBuffer, uint nSize, System.IntPtr arguments);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern System.IntPtr LoadLibrary([System.Runtime.InteropServices.InAttribute] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)] string lpLibFileName);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr InternetOpen(
            [In] string agent,
            [In] int dwAccessType,
            [In] string proxyName,
            [In] string proxyBypass,
            [In] int dwFlags);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr InternetConnect(
            [In] IntPtr hInternet,
            [In] string serverName,
            [In] int serverPort,
            [In] string userName,
            [In] string password,
            [In] int dwService,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int InternetCloseHandle(
            [In] IntPtr hInternet);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpCommand(
            [In] IntPtr hConnect,
            [In] bool fExpectResponse,
            [In] int dwFlags,
            [In] string command,
            [In] IntPtr dwContext,
            [In] [Out] ref IntPtr ftpCmd);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpCreateDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpDeleteFile(
            [In] IntPtr hConnect,
            [In] string fileName);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FtpFindFirstFile(
            [In] IntPtr hConnect,
            [In] string searchFile,
            [In] [Out] ref NativeMethods.WIN32_FIND_DATA findFileData,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpGetCurrentDirectory(
            [In] IntPtr hConnect,
            [In] [Out] StringBuilder currentDirectory,
            [In] [Out] ref int dwCurrentDirectory);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpGetFile(
            [In] IntPtr hConnect,
            [In] string remoteFile,
            [In] string newFile,
            [In] bool failIfExists,
            [In] int dwFlagsAndAttributes,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int FtpGetFileSize(
            [In] IntPtr hConnect,
            [In] [Out] ref int dwFileSizeHigh);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpOpenFile(
            [In] IntPtr hConnect,
            [In] string fileName,
            [In] int dwAccess,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpPutFile(
            [In] IntPtr hConnect,
            [In] string localFile,
            [In] string newRemoteFile,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpRemoveDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpRenameFile(
            [In] IntPtr hConnect,
            [In] string existingName,
            [In] string newName);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int FtpSetCurrentDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int InternetFindNextFile(
            [In] IntPtr hInternet,
            [In] [Out] ref NativeMethods.WIN32_FIND_DATA findData);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int InternetGetLastResponseInfo(
            [In] [Out] ref int dwError,
            [MarshalAs(UnmanagedType.LPTStr)] [Out] StringBuilder buffer,
            [In] [Out] ref int bufferLength);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int InternetReadFile(
            [In] IntPtr hConnect,
            [MarshalAs(UnmanagedType.LPTStr)] [In] [Out] StringBuilder buffer,
            [In] int buffCount,
            [In] [Out] ref int bytesRead);

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int InternetReadFileEx(
            [In] IntPtr hFile,
            [In] [Out] ref NativeMethods.INTERNET_BUFFERS lpBuffersOut,
            [In] int dwFlags,
            [In] [Out] int dwContext);
    }
}