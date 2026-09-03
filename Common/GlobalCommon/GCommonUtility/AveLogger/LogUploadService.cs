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
using System.Threading;
using AvePoint.GCommon.Utility.Storage;
using log4net;
using log4net.Repository.Hierarchy;

namespace AvePoint.GCommon
{
    using ICSharpCode.SharpZipLib.Zip;
    using Microsoft.IdentityModel.Protocols.WsTrust;
    using System.Net;

    public class LogUploadService
    {
        private LogUploadThread LogUploadThread { get; set; }

        private static LogUploadService _instance;

        private AveLogger logger = AveLogger.GetInstance(typeof(LogUploadService));

        private static readonly object locker = new object();

        public bool HasLogNeedUpload
        {
            get
            {
                return LogUploadThread.HasLogNeedUpload;
            }
        }

        private LogUploadService()
        {
            Start();
        }

        public static LogUploadService GetInstance()
        {
            lock (locker)
            {
                if (_instance == null)
                {
                    _instance = new LogUploadService();
                }
            }
            return _instance;
        }

        public static void SetLogType(LogType mLogType)
        {
            lock (locker)
            {
                if (_instance == null)
                {
                    _instance = new LogUploadService();
                }
                _instance.LogUploadThread.UploadLogTye = mLogType;
            }
        }

        public void ApplyXriString(string xriString)
        {
            LogUploadThread.XriString = xriString;
        }

        public void ApplyVersion(string version)
        {
            //this.Version = version;
            LogUploadThread.Vesrion = version;
        }

        public void ApplyIdentifier(string identifier)
        {
            LogUploadThread.RoleIdentifier = identifier;
        }

        public void UploadLog(string fileName)
        {
            try
            {
                LogUploadThread.UploadLog(fileName);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while uploading log file.", ex);
            }
        }

        private void Start()
        {
            LogUploadThread = new LogUploadThread();
            LogUploadThread.UploadLogTye = LogType.ServieLog;
            Thread thread = new Thread(LogUploadThread.DoUpload);
            thread.IsBackground = true;
            thread.Start();
        }


    }

    public class LogUploadThread
    {
        internal LogType UploadLogTye;

        private Queue<string> logFileQueue = new Queue<string>();

        private bool Interrupted { get; set; }

        private string mRoleIdentifier;
        public string RoleIdentifier
        {
            set
            {
                lock (configurationLock)
                {
                    mRoleIdentifier = value;
                    Monitor.PulseAll(configurationLock);
                }
            }
        }

        private string xriString;

        public string XriString
        {
            set
            {
                lock (configurationLock)
                {
                    xriString = value;
                    Monitor.PulseAll(configurationLock);
                }
            }
        }

        private string version;
        public string Vesrion
        {
            set
            {
                lock (configurationLock)
                {
                    version = value;
                    Monitor.PulseAll(configurationLock);
                }
            }
        }

        private readonly object configurationLock = new object();

        private AveLogger logger = AveLogger.GetInstance(typeof(LogUploadThread));

        public bool HasLogNeedUpload = true;

        public void UploadLog(string fileName)
        {
            lock (logFileQueue)
            {
                logFileQueue.Enqueue(fileName);
                this.HasLogNeedUpload = true;
                Monitor.PulseAll(logFileQueue);
            }
        }

        public void Stop()
        {
            Interrupted = true;
        }

        public void DoUpload()
        {
            while (true && !Interrupted)
            {
                try
                {
                    string logFileName = string.Empty;
                    lock (logFileQueue)
                    {
                        while (logFileQueue.Count == 0)
                        {
                            Monitor.Wait(logFileQueue);
                        }
                        logFileName = logFileQueue.Dequeue();
                    }
                    //WaitingForConfiguration();
                    //upload log file

                    if (!string.IsNullOrEmpty(xriString))
                    {
                        logger.Info("Upload log file to blob stroage. File Name: {0}", logFileName);
                        var zipFile = ZipLogFile(logFileName);
                        IXSystem xSystem = XFactory.InstanceSystem(xriString);
                        using (FileStream fs = new FileStream(zipFile, FileMode.Open))
                        {
                            string tenantFolderName = GetTenantFolderName(logFileName);
                            if (UploadLogTye == LogType.JobLog)
                            {
                                //上传Log的时候前面添加一个parentJobId的folder,防止subjob log太多导致storage explorer tool展开超时
                                string logName = GetLogFileName(logFileName, tenantFolderName, LogType.JobLog);
                                string[] names = logName.Split(new char[] { '_', '.' });
                                if (names.Length > 2)
                                {
                                    tenantFolderName = tenantFolderName + "/" + names[2]; //Path.GetFileNameWithoutExtension(names[1]);
                                }
                            }
                            if (UploadLogTye == LogType.ServieLog)
                            {
                                tenantFolderName = tenantFolderName + "/" + this.mRoleIdentifier;
                            }
                            
                            StorageInfo storageInfo = new StorageInfo()
                            {
                                HighName = tenantFolderName,
                                LowName = zipFile,
                                Length = fs.Length
                            };
                            StorageResult sr = xSystem.CommitStream(fs, storageInfo);
                            if (sr.IsCommitted)
                            {
                                logger.Info("Log file uploaded successfully, log path:{0}.", tenantFolderName + "/" + zipFile);
                            }
                            else
                            {
                                logger.Error("The log file is not be committed completely.");
                            }
                        }
                        //delete file
                        DeleteFile(logFileName);
                        DeleteFileDirectly(zipFile);
                    }
                    else
                    {
                        logger.Warn("No physical device or service version has been configured for uploading file. File Name: {0}. Version: {1}", logFileName, version);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while uploading log file. file name.", ex);
                }
                finally
                {
                    if (logFileQueue.Count == 0)
                    {
                        HasLogNeedUpload = false;
                    }
                }
            }
        }

        private string ZipLogFile(string logFile)
        {
            var file = new FileInfo(logFile);
            string target;
            switch (UploadLogTye)
            {
                case LogType.JobLog:
                    target = string.Format("{0}_{1}_{2}", GetLogFileName(logFile, GetTenantFolderName(logFile), UploadLogTye), Dns.GetHostName(), System.Diagnostics.Process.GetCurrentProcess().Id);
                    break;
                case LogType.SeparativeLog:
                    target = string.Format("{0}_{1}_{2}({3}-{4})", Dns.GetHostName(), GetLogFileName(logFile, GetTenantFolderName(logFile), UploadLogTye), GetSeparativeLogFileName(logFile), file.LastAccessTime.ToString("MM-dd HHmm"), file.LastWriteTime.ToString("MM-dd HHmm"));
                    break;
                default:
                    target = string.Format("{0}_{1}({2}-{3})", Dns.GetHostName(), GetLogFileName(logFile, GetTenantFolderName(logFile), UploadLogTye), file.LastAccessTime.ToString("MM-dd HHmm"), file.LastWriteTime.ToString("MM-dd HHmm"));
                    break;

            }
            var result = target + ".zip";
            using (ZipOutputStream OutputStream = new ZipOutputStream(File.Create(result)))
            {
                byte[] buffer = new byte[4096];
                ZipEntry entry = new ZipEntry(target + ".log");
                OutputStream.PutNextEntry(entry);

                using (FileStream fs = File.OpenRead(target + ".log"))
                {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        OutputStream.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                }
            }
            return result;
        }

        private string GetSeparativeLogFileName(string filePath)
        {
            try
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                int index = fileName.ToLower().IndexOf(".log");
                return index != -1 ? fileName.Substring(0, index) : fileName;
            }
            catch(Exception e)
            {
                logger.Info($"Get seprative log filename failed {e.ToString()}");
            }
            return string.Empty;
        }

        private string GetLogFileName(string filePath, string tenantFoldername,LogType logtype)
        {
            if (string.IsNullOrEmpty(tenantFoldername))
            {
                return System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            }
            else
            {
                filePath = filePath.Substring(filePath.IndexOf(tenantFoldername) + tenantFoldername.Length + 1);//trim parent folder path 
                switch (logtype)
                {
                    case LogType.JobLog:
                        return filePath;
                    default:
                        int index = filePath.ToLower().IndexOf(".log");
                        var value = index != -1 ? filePath.Substring(0, index) : filePath;
                        if (value.Contains("\\"))
                        {
                            value = value.Replace('\\', '_');
                        }
                        return value;
                }//trim log file suffix
            }
        }

        private string GetTenantFolderName(string filePath)
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (filePath.Contains(processName))
            {
                filePath = filePath.Substring(0, filePath.IndexOf(processName)).TrimEnd('\\');
                return filePath.Substring(filePath.LastIndexOf('\\') + 1);
            }
            else
            {
                return string.Empty;
            }
        }

        //private void WaitingForConfiguration()
        //{
        //    lock (configurationLock)
        //    {
        //        while (physicalDeviceDto == null ||
        //                string.IsNullOrEmpty(version)
        //              )
        //        {
        //            Monitor.Wait(configurationLock);
        //        }
        //    }
        //}

        private void DeleteFile(string fileName)
        {
            bool deleteFile = true;
#if DEBUG
            deleteFile = false;
#endif
            if (deleteFile)
            {
                AveRollingFileAppender appender = GetCurrentAppender();
                if (appender != null)
                {
                    appender.DeleteFileAfterUploading(fileName);
                }
                else
                {
                    DeleteFileDirectly(fileName);
                }
            }
        }

        private void DeleteFileDirectly(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                logger.Error("delete file:{0} failed:{1}", file, ex);
            }
        }

        private AveRollingFileAppender GetCurrentAppender()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = hierarchy.Root;
            AveRollingFileAppender appender = rootLogger.GetAppender("LogFileAppender") as AveRollingFileAppender;
            if (appender == null)
            {
                appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveRollingFileAppender;
            }
            return appender;
        }
    }

    public enum LogType
    {
        None = 0,
        JobLog = 1,
        ServieLog = 2,
        SeparativeLog = 3,
    }
}
