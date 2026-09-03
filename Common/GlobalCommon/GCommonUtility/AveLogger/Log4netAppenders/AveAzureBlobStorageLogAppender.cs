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
namespace AvePoint.GCommon
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Util;

    public class AveAzureBlobStorageLogAppender : FileAppender
    {
        private int count;
        private long m_maxFileSize = 10485760L;
        private string baseFileName;
        private DateTime nextCheckPoint = DateTime.MaxValue;
        private int m_UploadInterval = 0;
        /// <summary>
        /// the interval of uploading job log to storage, in minutes.
        /// </summary>
        public int UploadInterval
        {
            get { return this.m_UploadInterval; }
            set { this.m_UploadInterval = value; }
        }

        public long MaxFileSize
        {
            get
            {
                return this.m_maxFileSize;
            }
            set
            {
                this.m_maxFileSize = value;
            }
        }

        public string MaximumFileSize
        {
            get
            {
                return this.m_maxFileSize.ToString(NumberFormatInfo.InvariantInfo);
            }
            set
            {
                this.m_maxFileSize = OptionConverter.ToFileSize(value, this.m_maxFileSize + 1L);
            }
        }

        public override void ActivateOptions()
        {
            if (baseFileName == null)
            {
                baseFileName = File;
            }

            if (SecurityContext == null)
            {
                SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }
            try
            {
                using (SecurityContext.Impersonate(this))
                {
                    var fullName = FileAppender.ConvertToFullPath(File.Trim());
                    if (System.IO.File.Exists(fullName))
                    {
                        System.IO.File.Move(fullName, string.Concat(fullName, DateTime.UtcNow.ToString("yyyyMMddHHmmss")));
                    }
                }
            }
            catch(Exception ex)
            {
                LogLog.Error(typeof(AveAzureBlobStorageLogAppender),"move the previous file:" + File, ex);
            }
            this.nextCheckPoint = GetNextCheckPoint();
            base.ActivateOptions();
        }

        private DateTime GetNextCheckPoint()
        {
            if (m_UploadInterval > 0)
            {
                return DateTime.Now.AddMinutes(m_UploadInterval);
            }
            else
            {
                //at the begining of next hour by default.
                var current = DateTime.Now;
                current = current.AddMilliseconds(-current.Millisecond);
                current = current.AddSeconds(-current.Second);
                current = current.AddMinutes(-current.Minute);
                current = current.AddHours(1);
                return current;
            }
        }

        protected override void SetQWForFiles(TextWriter writer)
        {
            base.QuietWriter = new CountingQuietTextWriter(writer, ErrorHandler);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            this.AdjustFileBeforeAppend();
            base.Append(loggingEvent);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            this.AdjustFileBeforeAppend();
            base.Append(loggingEvents);
        }

        protected void AdjustFileBeforeAppend()
        {
            if (File != null && ((CountingQuietTextWriter)base.QuietWriter).Count >= this.m_maxFileSize)
            {
                base.CloseFile();
                MoveAndUploadLog(File);
                LogLog.Debug(typeof(AveAzureBlobStorageLogAppender), "RollingFileAppender: rolling over count [" + ((CountingQuietTextWriter)base.QuietWriter).Count + "]");
                count++;

                File = Path.Combine(Path.GetDirectoryName(baseFileName), string.Format("{0}.{1}{2}", Path.GetFileNameWithoutExtension(baseFileName), count, Path.GetExtension(baseFileName)));
                this.SafeOpenFile(File, AppendToFile);
                return;
            }
            if (File != null && this.nextCheckPoint < DateTime.Now)
            {
                this.nextCheckPoint = GetNextCheckPoint();
                CopyAndUploadLog(File);
                LogLog.Debug(typeof(AveAzureBlobStorageLogAppender), "RollingFileAppender: rolling over at [" + DateTime.Now + "]");

                return;
            }
        }

        private void MoveAndUploadLog(string fileName)
        {
            string targetFileName = null;
            try
            {
                var logsFolder = Path.GetDirectoryName(fileName);

                if (logsFolder.EndsWith("common", StringComparison.OrdinalIgnoreCase))
                {
                    targetFileName = Path.Combine(logsFolder, string.Concat(Path.GetFileNameWithoutExtension(fileName), "_", Dns.GetHostName().Replace("-", "").Replace(".", ""), "_", Path.GetExtension(fileName)));
                }
                else
                {
                    var name = Path.GetFileName(fileName);
                    var extension = name.Substring(name.IndexOf('.'));
                    if (extension.StartsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        extension = extension.Substring(".exe".Length);
                    }

                    string jobId = log4net.GlobalContext.Properties["TenantJobId"] as string;
                    if (string.IsNullOrEmpty(jobId))
                    {
                        jobId = string.Empty;
                    }
                    else
                    {
                        jobId = "_" + jobId;
                    }

                    string tenantIdentity = log4net.GlobalContext.Properties["TenantName"] as string;

                    if (string.IsNullOrEmpty(tenantIdentity))
                    {
                        tenantIdentity = "common";
                    }
                    var targetDirectoryName = Path.Combine(Path.GetDirectoryName(fileName), tenantIdentity);
                    CreateDirIfNotExist(targetDirectoryName);

                    var currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";

                    targetFileName = Path.Combine(targetDirectoryName, currentProcessName + jobId + extension);



                    var index = 0;

                    while (System.IO.File.Exists(targetFileName))
                    {
                        targetFileName = Path.Combine(targetDirectoryName, currentProcessName + jobId + "." + index + extension);
                        index++;
                    }
                }

                LogLog.Debug(typeof(AveAzureBlobStorageLogAppender), "Move file from [" + fileName + "] to [" + targetFileName + "]");

                System.IO.File.Move(fileName, targetFileName);

                LogUploadService.GetInstance().UploadLog(targetFileName);
            }
            catch(Exception ex)
            {
                LogLog.Error(typeof(AveAzureBlobStorageLogAppender), "Mofe file from [" + fileName + "] to [" + targetFileName + "] failed.", ex);
            }
        }

        private void CopyAndUploadLog(string fileName)
        {
            string targetFileName = null;
            try
            {
                var logsFolder = Path.GetDirectoryName(fileName);

                if (logsFolder.EndsWith("common", StringComparison.OrdinalIgnoreCase))
                {
                    targetFileName = Path.Combine(logsFolder, string.Concat(Path.GetFileNameWithoutExtension(fileName), "_", Dns.GetHostName().Replace("-", "").Replace(".", ""), "_", Path.GetExtension(fileName)));
                }
                else
                {
                    var name = Path.GetFileName(fileName);
                    var extension = name.Substring(name.IndexOf('.'));
                    if (extension.StartsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        extension = extension.Substring(".exe".Length);
                    }

                    string jobId = log4net.GlobalContext.Properties["TenantJobId"] as string;
                    if (string.IsNullOrEmpty(jobId))
                    {
                        jobId = string.Empty;
                    }
                    else
                    {
                        jobId = "_" + jobId;
                    }

                    targetFileName = GetTargeFileName(fileName, extension, jobId);
                }

                LogLog.Debug(typeof(AveAzureBlobStorageLogAppender), "Copy file from [" + fileName + "] to [" + targetFileName + "]");

                System.IO.File.Copy(fileName, targetFileName, true);

                LogUploadService.GetInstance().UploadLog(targetFileName);
            }
            catch (Exception ex)
            {
                LogLog.Error(typeof(AveAzureBlobStorageLogAppender), "Copy file from [" + fileName + "] to [" + targetFileName + "] failed.", ex);
            }
        }

        private static string GetTargeFileName(string fileName, string extension, string jobId)
        {
            string targetFileName;
            string tenantIdentity = log4net.GlobalContext.Properties["TenantName"] as string;

            if (string.IsNullOrEmpty(tenantIdentity))
            {
                tenantIdentity = "common";
            }
            //c:\C:\DocAveOnlineTenant\Logs\InProgress
            var baseDir = Path.Combine(Path.GetDirectoryName(fileName), "InProgress");
            CreateDirIfNotExist(baseDir);
            //c:\C:\DocAveOnlineTenant\Logs\InProgress\qlluo@avepoint.com
            var targetDirectoryName = Path.Combine(baseDir, tenantIdentity);
            CreateDirIfNotExist(targetDirectoryName);

            var currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";

            targetFileName = Path.Combine(targetDirectoryName, currentProcessName + jobId + extension);
            return targetFileName;
        }

        private static void CreateDirIfNotExist(string targetDirectoryName)
        {
            try
            {
                if (!Directory.Exists(targetDirectoryName))
                {
                    Directory.CreateDirectory(targetDirectoryName);
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(typeof(AveAzureBlobStorageLogAppender), "Ensure directory [" + targetDirectoryName + "] failed.", ex);
            }
        }

        public void FinishAndSwitch2CommonLog()
        {
            base.CloseFile();
            MoveAndUploadLog(File);
            LogLog.Debug(typeof(AveAzureBlobStorageLogAppender), "RollingFileAppender: rolling over count [" + ((CountingQuietTextWriter)base.QuietWriter).Count + "]");
            count++;

            File = Path.Combine(Path.GetDirectoryName(baseFileName), "Common", string.Concat(System.Diagnostics.Process.GetCurrentProcess().ProcessName, ".exe", Path.GetExtension(baseFileName)));
            baseFileName = File;
            LockingModel = new FileAppender.MinimalLock();// to avoid the write conflict issue.
            base.ActivateOptions();
        }
    }
}
