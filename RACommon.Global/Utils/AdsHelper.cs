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
using AvePoint.RA.Contract.Explorer;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AvePoint.RA.Common.Utils
{
    public class AdsHelper
    {
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;
        private const uint CREATE_ALWAYS = 2;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_ACCESS_DENIED = 5;
        private const uint FILE_ATTRIBUTE_READONLY = 0x00000001;

        const int maxRetries = 3;
        const int delayMs = 100;

        private const string UniqueIdStreamName = "uniqueid";

        private const string TermIdStreamName = "termid";

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetFileTime(
            SafeFileHandle hFile,
            ref System.Runtime.InteropServices.ComTypes.FILETIME lpCreationTime,
            ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastAccessTime,
            ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastWriteTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

        public static void WriteUniqueIdAds(string filePath, FileSystemADSUniqueInfo uniqueInfo, bool isFolder = false)
        {
            var retryCount = 0;
            var lastError = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    var fullPath = $"{filePath}:{UniqueIdStreamName}";
                    var uniqueInfoStr = JsonConvert.SerializeObject(uniqueInfo);

                    ActionKeepTime(filePath, isFolder, () =>
                    {
                        using (SafeFileHandle handle = CreateFile(
                            fullPath,
                            GENERIC_WRITE,
                            FILE_SHARE_READ | FILE_SHARE_WRITE,  // 允许共享读写
                            IntPtr.Zero,
                            OPEN_EXISTING,
                            FILE_ATTRIBUTE_NORMAL,
                            IntPtr.Zero))
                        {
                            if (handle.IsInvalid && Marshal.GetLastWin32Error() != 2)
                            {
                                lastError = Marshal.GetLastWin32Error();
                                throw new IOException("Failed to open ADS", lastError);
                            }

                            using (var stream = new FileStream(handle, FileAccess.Write))
                            using (var writer = new StreamWriter(stream, Encoding.UTF8))
                            {
                                writer.Write(uniqueInfoStr);
                            }
                        }
                    });
                    return;
                }
                catch (IOException ex)
                {
                    bool isSharingViolation =
                        ex.Message.Contains("32") ||
                        ex.Message.Contains("sharing violation") ||
                        lastError == 32;

                    if (!isSharingViolation)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new IOException($"Failed after {maxRetries} attempts. Last error: {lastError}", ex);
                    }
                    Thread.Sleep(delayMs * retryCount);
                }
            }
        }

        public static void WriteUniqueIdAdsAndRevertTime(string filePath, FileSystemADSUniqueInfo uniqueInfo, bool isFolder = false)
        {
            var retryCount = 0;
            var lastError = 0;
            var fullPath = $"{filePath}:{UniqueIdStreamName}";

            DateTime? lastAccessTime = null;
            DateTime? lastWriteTime = null;

            try
            {
                if (isFolder)
                {
                    var dirInfo = new DirectoryInfo(filePath);
                    lastAccessTime = dirInfo.LastAccessTime;
                    lastWriteTime = dirInfo.LastWriteTime;
                }
                else
                {
                    var fileInfo = new FileInfo(filePath);
                    lastAccessTime = fileInfo.LastAccessTime;
                    lastWriteTime = fileInfo.LastWriteTime;
                }
            }
            catch
            {
            }

            var fileTimeWrite = ToFileTime(lastWriteTime.Value);
            var fileTimeAccess = ToFileTime(lastAccessTime.Value);
            var fileTimeCreate = new System.Runtime.InteropServices.ComTypes.FILETIME { dwLowDateTime = 0, dwHighDateTime = 0 };

            while (retryCount < maxRetries)
            {
                SafeFileHandle handle = null;
                FileStream stream = null;
                var clearedReadOnly = false;

                try
                {
                    var uniqueInfoStr = JsonConvert.SerializeObject(uniqueInfo);
                    uint flags = FILE_ATTRIBUTE_NORMAL | (isFolder ? FILE_FLAG_BACKUP_SEMANTICS : 0);

                    handle = CreateFile(
                        fullPath,
                        GENERIC_WRITE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        FILE_ATTRIBUTE_NORMAL,
                        IntPtr.Zero);

                    if (handle.IsInvalid)
                    {
                        lastError = Marshal.GetLastWin32Error();

                        if (lastError == 2)
                        {
                            // Create new ADS: may require removing READONLY on base file.
                            uint baseAttr = GetFileAttributes(filePath);
                            if (baseAttr != 0xFFFFFFFF && (baseAttr & FILE_ATTRIBUTE_READONLY) != 0)
                            {
                                if (SetFileAttributes(filePath, baseAttr & ~FILE_ATTRIBUTE_READONLY))
                                    clearedReadOnly = true;
                            }

                            handle = CreateFile(
                                fullPath,
                                GENERIC_WRITE,
                                FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                                IntPtr.Zero,
                                CREATE_ALWAYS,
                                flags,
                                IntPtr.Zero);

                            if (handle.IsInvalid)
                            {
                                lastError = Marshal.GetLastWin32Error();
                                throw new IOException($"Failed to create ADS. Win32Error={lastError}");
                            }
                        }
                        else if (lastError == ERROR_ACCESS_DENIED)
                        {
                            throw new IOException($"Access denied opening ADS. Win32Error={lastError}");
                        }
                        else if (lastError != 0)
                        {
                            throw new IOException($"Failed to open ADS. Win32Error={lastError}");
                        }
                    }

                    stream = new FileStream(handle, FileAccess.Write);
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        stream = null;
                        writer.Write(uniqueInfoStr);

                        if (!SetFileTime(handle, ref fileTimeCreate, ref fileTimeAccess, ref fileTimeWrite))
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            //throw new IOException($"无法设置时间属性 (错误码: {errorCode})");
                        }
                    }

                    if (clearedReadOnly)
                    {
                        // Restore READONLY bit.
                        uint baseAttr = GetFileAttributes(filePath);
                        if (baseAttr != 0xFFFFFFFF)
                            SetFileAttributes(filePath, baseAttr | FILE_ATTRIBUTE_READONLY);
                    }

                    return;
                }
                catch (IOException ex)
                {
                    bool isSharingViolation =
                        ex.HResult == -2147024864 ||  // ERROR_SHARING_VIOLATION
                        lastError == 32;

                    if (!isSharingViolation)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new IOException($"Failed after {maxRetries} attempts. Last error: {lastError}", ex);
                    }

                    Thread.Sleep(delayMs * retryCount);
                }
                finally
                {
                    stream?.Dispose();
                    handle?.Dispose();
                }
            }
        }

        public static void WriteTermIdAds(string filePath, FileSystemADSTermInfo termInfo, bool isFolder = false)
        {
            var retryCount = 0;
            var lastError = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var fullPath = $"{filePath}:{TermIdStreamName}";
                    var termInfoStr = JsonConvert.SerializeObject(termInfo);

                    ActionKeepTime(filePath, isFolder, () =>
                    {
                        using (SafeFileHandle handle = CreateFile(
                       fullPath,
                       GENERIC_WRITE,
                       FILE_SHARE_READ | FILE_SHARE_WRITE,
                       IntPtr.Zero,
                       OPEN_EXISTING,
                       FILE_ATTRIBUTE_NORMAL,
                       IntPtr.Zero))
                        {
                            if (handle.IsInvalid && Marshal.GetLastWin32Error() != 2)
                                throw new IOException("Failed to open ADS", Marshal.GetLastWin32Error());

                            using (var stream = new FileStream(handle, FileAccess.Write))
                            using (var writer = new StreamWriter(stream, Encoding.UTF8))
                            {
                                writer.Write(termInfoStr);
                            }
                        }
                    });
                    return;
                }
                catch (IOException ex)
                {
                    bool isSharingViolation =
                        ex.Message.Contains("32") ||
                        ex.Message.Contains("sharing violation") ||
                        lastError == 32;

                    if (!isSharingViolation)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new IOException($"Failed after {maxRetries} attempts. Last error: {lastError}", ex);
                    }
                    Thread.Sleep(delayMs * retryCount);
                }
            }
        }

        public static void WriteTermIdAdsAndRevertTime(string filePath, FileSystemADSTermInfo termInfo, bool isFolder = false)
        {
            var retryCount = 0;
            var lastError = 0;
            var fullPath = $"{filePath}:{TermIdStreamName}";

            DateTime? creationTime = null;
            DateTime? lastAccessTime = null;
            DateTime? lastWriteTime = null;

            try
            {
                if (isFolder)
                {
                    var dirInfo = new DirectoryInfo(filePath);
                    creationTime = dirInfo.CreationTime;
                    lastAccessTime = dirInfo.LastAccessTime;
                    lastWriteTime = dirInfo.LastWriteTime;
                }
                else
                {
                    var fileInfo = new FileInfo(filePath);
                    creationTime = fileInfo.CreationTime;
                    lastAccessTime = fileInfo.LastAccessTime;
                    lastWriteTime = fileInfo.LastWriteTime;
                }
            }
            catch
            {
            }

            var fileTimeWrite = ToFileTime(lastWriteTime.Value);
            var fileTimeAccess = ToFileTime(lastAccessTime.Value);
            var fileTimeCreate = new System.Runtime.InteropServices.ComTypes.FILETIME { dwLowDateTime = 0, dwHighDateTime = 0 };

            while (retryCount < maxRetries)
            {
                SafeFileHandle handle = null;
                FileStream stream = null;
                var clearedReadOnly = false;

                try
                {
                    var uniqueInfoStr = JsonConvert.SerializeObject(termInfo);
                    uint flags = FILE_ATTRIBUTE_NORMAL | (isFolder ? FILE_FLAG_BACKUP_SEMANTICS : 0);

                    handle = CreateFile(
                        fullPath,
                        GENERIC_WRITE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        FILE_ATTRIBUTE_NORMAL,
                        IntPtr.Zero);

                    if (handle.IsInvalid)
                    {
                        lastError = Marshal.GetLastWin32Error();

                        if (lastError == 2)
                        {
                            // Create new ADS: may require removing READONLY on base file.
                            uint baseAttr = GetFileAttributes(filePath);
                            if (baseAttr != 0xFFFFFFFF && (baseAttr & FILE_ATTRIBUTE_READONLY) != 0)
                            {
                                if (SetFileAttributes(filePath, baseAttr & ~FILE_ATTRIBUTE_READONLY))
                                    clearedReadOnly = true;
                            }

                            handle = CreateFile(
                                fullPath,
                                GENERIC_WRITE,
                                FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                                IntPtr.Zero,
                                CREATE_ALWAYS,
                                flags,
                                IntPtr.Zero);

                            if (handle.IsInvalid)
                            {
                                lastError = Marshal.GetLastWin32Error();
                                throw new IOException($"Failed to create ADS. Win32Error={lastError}");
                            }
                        }
                        else if (lastError == ERROR_ACCESS_DENIED)
                        {
                            throw new IOException($"Access denied opening ADS. Win32Error={lastError}");
                        }
                        else if (lastError != 0)
                        {
                            throw new IOException($"Failed to open ADS. Win32Error={lastError}");
                        }
                    }

                    stream = new FileStream(handle, FileAccess.Write);
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        stream = null;
                        writer.Write(uniqueInfoStr);

                        if (!SetFileTime(handle, ref fileTimeCreate, ref fileTimeAccess, ref fileTimeWrite))
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            //throw new IOException($"无法设置时间属性 (错误码: {errorCode})");
                        }
                    }

                    if (clearedReadOnly)
                    {
                        // Restore READONLY bit.
                        uint baseAttr = GetFileAttributes(filePath);
                        if (baseAttr != 0xFFFFFFFF)
                            SetFileAttributes(filePath, baseAttr | FILE_ATTRIBUTE_READONLY);
                    }

                    return;
                }
                catch (IOException ex)
                {
                    bool isSharingViolation =
                        ex.HResult == -2147024864 ||  // ERROR_SHARING_VIOLATION
                        lastError == 32;

                    if (!isSharingViolation)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new IOException($"Failed after {maxRetries} attempts. Last error: {lastError}", ex);
                    }

                    Thread.Sleep(delayMs * retryCount);
                }
                finally
                {
                    stream?.Dispose();
                    handle?.Dispose();
                }
            }
        }


        public static string ReadUniqueIdAds(string filePath)
        {
            string fullPath = $"{filePath}:{UniqueIdStreamName}";

            using (SafeFileHandle handle = CreateFile(
                fullPath,
                GENERIC_READ,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 2) // ADS not found
                        return string.Empty;
                    throw new IOException($"Failed to open ADS. Win32Error={error}");
                }

                using (var stream = new FileStream(handle, FileAccess.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string ReadTermIdAds(string filePath)
        {
            string fullPath = $"{filePath}:{TermIdStreamName}";

            using (SafeFileHandle handle = CreateFile(
                fullPath,
                GENERIC_READ,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 2) // ADS not found
                        return string.Empty;
                    throw new IOException($"Failed to open ADS. Win32Error={error}");
                }

                using (var stream = new FileStream(handle, FileAccess.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static void DeleteUniqueId(string filePath)
        {
            string fullPath = $"{filePath}:{UniqueIdStreamName}";

            using (SafeFileHandle handle = CreateFile(
               fullPath,
               GENERIC_READ,
               FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
               IntPtr.Zero,
               OPEN_EXISTING,
               FILE_ATTRIBUTE_NORMAL,
               IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    throw new IOException("Failed to open ADS", Marshal.GetLastWin32Error());

                using (var stream = new FileStream(handle, FileAccess.Write))
                {
                    stream.Close();
                }
            }
        }

        public static void DeleteTermId(string filePath)
        {
            string fullPath = $"{filePath}:{TermIdStreamName}";

            using (SafeFileHandle handle = CreateFile(
               fullPath,
               GENERIC_READ,
               FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
               IntPtr.Zero,
               OPEN_EXISTING,
               FILE_ATTRIBUTE_NORMAL,
               IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    throw new IOException("Failed to open ADS", Marshal.GetLastWin32Error());

                using (var stream = new FileStream(handle, FileAccess.Write))
                {
                    stream.Close();
                }
            }
        }

        public static void ActionKeepTime(string filePath, bool isFolder, Action action)
        {
            FileSystemInfo fileSystemInfo = null;
            try
            {
                if (isFolder)
                {
                    fileSystemInfo = new DirectoryInfo(filePath);
                }
                else
                {
                    fileSystemInfo = new FileInfo(filePath);
                }

                var originalLastModifidTime = fileSystemInfo.LastWriteTime;
                var originalLastModifidTimeUtc = fileSystemInfo.LastWriteTimeUtc;
                var originalLastAccessedTime = fileSystemInfo.LastAccessTime;
                var originalLastAccessedTimeUtc = fileSystemInfo.LastAccessTimeUtc;

                action();

                try
                {
                    if (isFolder)
                    {
                        Directory.SetLastWriteTime(filePath, originalLastModifidTime);
                        Directory.SetLastWriteTimeUtc(filePath, originalLastModifidTimeUtc);
                        Directory.SetLastAccessTime(filePath, originalLastAccessedTime);
                        Directory.SetLastAccessTimeUtc(filePath, originalLastAccessedTimeUtc);
                    }
                    else
                    {
                        File.SetLastWriteTime(filePath, originalLastModifidTime);
                        File.SetLastWriteTimeUtc(filePath, originalLastModifidTimeUtc);
                        File.SetLastAccessTime(filePath, originalLastAccessedTime);
                        File.SetLastAccessTimeUtc(filePath, originalLastAccessedTimeUtc);
                    }
                }
                catch (IOException ex) when (Marshal.GetLastWin32Error() == 32)
                {

                }
            }
            finally
            {
                fileSystemInfo?.Refresh();
            }
        }

        private static System.Runtime.InteropServices.ComTypes.FILETIME ToFileTime(DateTime dateTime)
        {
            long fileTime = dateTime.ToFileTime();
            return new System.Runtime.InteropServices.ComTypes.FILETIME
            {
                dwLowDateTime = (int)(fileTime & 0xFFFFFFFF),
                dwHighDateTime = (int)(fileTime >> 32)
            };
        }
    }
}
