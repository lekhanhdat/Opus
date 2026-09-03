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
namespace AvePoint.Office365.Api.AIR
{
    using GCommon;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    static class MSIPCRuntime
    {
        private static object lockObj = new object();
        private static AveLogger logger = AveLogger.GetInstance(typeof(MSIPCRuntime));

        private static bool? shareCredential;
        private static bool? runtimeStatus;
        private static Exception runtimeException;
        private static IPC.APIMode currentAPIMode;
        private static Mutex globalMutex;

        internal static void EnsureRuntime()
        {
            EnsureRuntime(IPC.APIMode.Server);
            InitializeShareMode();
        }

        internal static void EnsureRuntime(IPC.APIMode mode)
        {
            EnsureRuntimeInternal(mode);
            InitializeShareMode();
        }

        private static void EnsureRuntimeInternal(IPC.APIMode mode)
        {
            if (runtimeStatus == null)
            {
                lock (lockObj)
                {
                    if (runtimeStatus == null)
                    {
                        try
                        {
                            currentAPIMode = IPC.SafeNativeMethods.IpcInitialize();
                            runtimeStatus = true;
                        }
                        catch (Exception ex)
                        {
                            runtimeStatus = false;
                            runtimeException = ex;
                            throw;
                        }
                    }
                }
            }

            if (runtimeStatus == true)
            {
                if (currentAPIMode != mode)
                {
                    lock (lockObj)
                    {
                        if (currentAPIMode != mode)
                        {
                            IPC.SafeNativeMethods.IpcSetAPIMode(mode);
                            currentAPIMode = mode;
                        }
                    }
                }
            }
            else
            {
                throw runtimeException;
            }
        }

        internal static void InitializeShareMode()
        {
            if (shareCredential == null)
            {
                lock (lockObj)
                {
                    if (shareCredential == null)
                    {
                        try
                        {
                            var shareCredentialValue = ReadFromConfiguration("Office365Api.MSIPCShareCredential", true);
                            if (shareCredentialValue)
                            {
                                var storeName = GetAvailableStoreName();

                                //C:\ProgramData\Microsoft\MSIPC\Server
                                var location = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\msipc\Server\"), storeName);

                                logger.Info("start to use location:{0}", location);

                                if (Directory.Exists(location))
                                {
                                    try
                                    {
                                        //Delete SID folder
                                        //https://stackoverflow.com/questions/16099956/how-to-determine-if-a-string-is-a-user-sid
                                        //https://msdn.microsoft.com/en-us/library/cc246018.aspx
                                        // @"^S-\d-(\d+-){1,14}\d+$"
                                        DeleteDirectory(location);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Warn( "Clean directory:{0} has exception:{1}", location, ex);
                                    }
                                }

                                IPC.SafeNativeMethods.IpcSetStoreName(storeName);

                                shareCredential = true;
                            }
                            else
                            {
                                shareCredential = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            shareCredential = false;
                            logger.Error("Initialize Share Mode failed:{0}", ex);
                        }
                    }
                }
            }
        }

        static string GetAvailableStoreName()
        {
            int index = 1;
            while (true)
            {
                var name = string.Concat("Global\\Office365Api-MSIPC-", index);
                try
                {
                    bool creadedNew;
                    globalMutex = new Mutex(true, name, out creadedNew);
                    if (creadedNew)
                    {
                        return string.Concat("O", index);
                    }
                    else
                    {
                        globalMutex.Close();
                    }
                }
                catch (AbandonedMutexException e)
                {
                    logger.Warn("require the mutex:{0} has exception:{1}", name, e);
                    return string.Concat("O", index);
                }
                catch (Exception ex)
                {
                    logger.Warn("require the mutex:{0} has exception:{1}", name, ex);
                }

                index++;
            }
        }

        static bool ReadFromConfiguration(string key, bool defaultValue)
        {
            string keyValue = ConfigurationManager.AppSettings[key];
            bool keyBoolValue = false;
            if (!bool.TryParse(keyValue, out keyBoolValue))
            {
                keyBoolValue = defaultValue;
            }
            return keyBoolValue;
        }

        static void DeleteDirectory(string directoryName)
        {
            try
            {
                Directory.Delete(directoryName, true);
            }
            catch (Exception)
            {
                var files = Directory.GetFiles(directoryName);

                foreach (var file in files)
                {
                    try
                    {
                        if (file.Length > 260)
                        {
                            var info = new LongPathFileInfo(file);
                            if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                info.Attributes = info.Attributes & (~FileAttributes.ReadOnly);
                            }
                            info.Delete();
                        }
                        else
                        {
                            var info = new FileInfo(file);
                            if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                info.Attributes = info.Attributes & (~FileAttributes.ReadOnly);
                            }
                            info.Delete();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("delete file:{0} has exception:{1}", file, e);
                    }
                }

                var subDirectories = Directory.GetDirectories(directoryName);

                foreach (var subDirectory in subDirectories)
                {
                    DeleteDirectory(subDirectory);
                }

                try
                {
                    Directory.Delete(directoryName, true);
                }
                catch (Exception e)
                {
                    logger.Warn("delete folder:{0} has exception:{1}", directoryName, e);
                }
            }
        }

        class LongPathFileInfo : FileSystemInfo
        {
            private readonly string name;

            public new FileAttributes Attributes
            {
                [SecuritySafeCritical]
                get
                {
                    return base.Attributes;
                }
                [SecuritySafeCritical]
                set
                {
                    if (!SetFileAttributes(this.FullPath, (int)value))
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error == 87)
                        {
                            throw new ArgumentException("Invalid file attributes");
                        }
                        if (lastWin32Error == 5)
                        {
                            throw new ArgumentException("Access Denied");
                        }
                        WinIOError(lastWin32Error, this.FullPath);
                    }
                }
            }

            public override string Name { get { return name; } }

            public override bool Exists
            {
                [SecuritySafeCritical]
                get
                {
                    bool result;
                    try
                    {
                        result = (base.Attributes & FileAttributes.Directory) != FileAttributes.Directory;
                    }
                    catch
                    {
                        result = false;
                    }
                    return result;
                }
            }

            public LongPathFileInfo(string fileName)
            {
                OriginalPath = fileName;

                if (!fileName.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = @"\\?\" + fileName;
                }

                FullPath = fileName;
                name = Path.GetFileName(fileName);
            }

            [SecuritySafeCritical]
            public override void Delete()
            {
                if (!DeleteFile(this.FullPath))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 2)
                    {
                        return;
                    }
                    WinIOError(lastWin32Error, FullPath);
                }
            }

            [SecurityCritical]
            internal static void WinIOError(int errorCode, string maybeFullPath)
            {
                bool isInvalidPath = errorCode == 123 || errorCode == 161;
                string displayablePath = maybeFullPath;

                string errorMessage = string.Format("Error code:{0}, file name:{1}", errorCode, maybeFullPath);

                if (errorCode <= 80)
                {
                    if (errorCode <= 15)
                    {
                        switch (errorCode)
                        {
                            case 2:
                                throw new FileNotFoundException(errorMessage);
                            case 3:
                                throw new DirectoryNotFoundException(errorMessage);
                            case 4:
                                break;
                            case 5:
                                throw new UnauthorizedAccessException(errorMessage);
                            default:
                                if (errorCode == 15)
                                {
                                    throw new DriveNotFoundException(errorMessage);
                                }
                                break;
                        }
                    }
                    else if (errorCode != 32)
                    {
                        if (errorCode == 80)
                        {
                            if (displayablePath.Length != 0)
                            {
                                throw new IOException(errorMessage);
                            }
                        }
                    }
                    else
                    {
                        throw new IOException(errorMessage);
                    }
                }
                else if (errorCode <= 183)
                {
                    if (errorCode == 87)
                    {
                        throw new IOException(errorMessage);
                    }
                    if (errorCode == 183)
                    {
                        throw new IOException(errorMessage);
                    }
                }
                else
                {
                    if (errorCode == 206)
                    {
                        throw new PathTooLongException(errorMessage);
                    }
                    if (errorCode == 995)
                    {
                        throw new OperationCanceledException();
                    }
                }
                throw new IOException(errorMessage);
            }

            [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool SetFileAttributes(string name, int attr);

            [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool DeleteFile(string path);
        }
    }
}