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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using StorageTable;

namespace AvePoint.GCommon
{
    public class AveAzureStorageLogAppender : AppenderSkeleton
    {
        /// <summary>
        /// The PartitionKey of each entry
        /// </summary>
        public string PartitionKey { get; set; }

        public AzureStorageCredential Credential { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// The name of table in Azure Storage
        /// </summary>
        private string TableName;

        private Thread storageThread;

        private Queue<LogEntry> logEntryQueue = new Queue<LogEntry>();

        private long previousTicks = DateTime.Now.Ticks;

        private int num = 0;

        private object pendingLock = new object();

        public bool NeedStopThread { get; set; }

        private object threadCountLock = new object();

        private int threadCount = 0;

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {

                if (Credential == null
                    || string.IsNullOrEmpty(Credential.AccountName)
                    || string.IsNullOrEmpty(Credential.AccessKey)
                    || string.IsNullOrEmpty(Version))
                {
                    return;
                }
                LogEntry logEntry = new LogEntry();
                logEntry.Level = loggingEvent.Level.ToString();
                logEntry.Thread = loggingEvent.ThreadName;
                logEntry.Time = loggingEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss,fff");
                logEntry.LoggerName = loggingEvent.LoggerName;
                logEntry.EventID = loggingEvent.Properties["EventID"].ToString();
                logEntry.Message = loggingEvent.RenderedMessage;

                logEntry.PartitionKey = PartitionKey;

                long ticks = DateTime.Now.Ticks;
                if (ticks == previousTicks)
                {
                    num++;
                }
                else
                {
                    previousTicks = ticks;
                    num = 0;
                }
                logEntry.RowKey = string.Format("{0}-{1}", ticks, num);

                lock (logEntryQueue)
                {
                    logEntryQueue.Enqueue(logEntry);
                    if (logEntryQueue.Count > 100)
                    {
                        StartStorageThread();
                    }
                    Monitor.Pulse(logEntryQueue);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.StackTrace);
            }
        }

        private void StartStorageThread()
        {
            lock (threadCountLock)
            {
                int workerNum = 0;
                int completionNum = 0;
                ThreadPool.GetAvailableThreads(out workerNum, out completionNum);
                if (workerNum > 0 && completionNum > 0 && threadCount <= 10)
                {
                    ThreadPool.QueueUserWorkItem((state) => WriteToStorage());
                    threadCount++;
                }
            }
        }

        public static void StopStorageThread()
        {
            AveAzureStorageLogAppender appender = GetCurrentAppender();
            if (appender != null)
            {
                appender.NeedStopThread = true;
            }
        }

        private void WriteToStorage()
        {
            try
            {
                WaitingForConfiguring();

                Regex nonAlphanumbericPattern = new Regex("[^a-zA-Z0-9]");
                TableName = nonAlphanumbericPattern.Replace(Dns.GetHostName() + System.Diagnostics.Process.GetCurrentProcess().ProcessName + Version, "");

                TableService tableService = InitTableService();
                if (tableService == null)
                {
                    return;
                }
                tableService.CreateTable(TableName);
                Queue<LogEntry> processingLogEntryQueue = new Queue<LogEntry>();
                int logCount = 0;
                lock (logEntryQueue)
                {
                    while (logEntryQueue.Count == 0)
                    {
                        Monitor.Wait(logEntryQueue);
                    }
                    while (logEntryQueue.Count > 0)
                    {
                        processingLogEntryQueue.Enqueue(logEntryQueue.Dequeue());
                        logCount++;
                        if (logCount >= 100)
                        {
                            break;
                        }
                    }
                }
                tableService.BatchCommitEnties(TableName, processingLogEntryQueue.ToList());
                processingLogEntryQueue.Clear();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.StackTrace);
            }
            lock (threadCountLock)
            {
                threadCount--;
            }
        }

        private void WaitingForConfiguring()
        {
            lock (pendingLock)
            {
                while (Credential == null ||
                        string.IsNullOrEmpty(Credential.AccountName) ||
                        string.IsNullOrEmpty(Credential.AccessKey) ||
                        string.IsNullOrEmpty(Version)
                      )
                {
                    Monitor.Wait(pendingLock);
                }
            }
        }

        /// <summary>
        /// Invoke this method will resume the consumer thread, and start to write data to storage.
        /// Becareful this method must be invoked after all parameter configured.
        /// </summary>
        public static void ConfigurationComplete()
        {
            AveAzureStorageLogAppender appender = GetCurrentAppender();
            if (appender != null)
            {
                lock (appender.pendingLock)
                {
                    Monitor.Pulse(appender.pendingLock);
                }
            }
        }

        private TableService InitTableService()
        {
            string AccountName = Credential.AccountName;
            string AccessKey = Credential.AccessKey;
            TableService tableService = new TableService(AccountName, AccessKey);
            return tableService;
        }

        public static void ApplyAzureStorageCredential(AzureStorageCredential credential)
        {
            AveAzureStorageLogAppender appender = GetCurrentAppender();
            if (appender != null)
            {
                appender.Credential = credential;
            }
        }

        public static void ApplyVersion(string version)
        {
            AveAzureStorageLogAppender appender = GetCurrentAppender();
            if (appender != null)
            {
                appender.Version = version;
            }
        }

        private static AveAzureStorageLogAppender GetCurrentAppender()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = hierarchy.Root;
            AveAzureStorageLogAppender appender = rootLogger.GetAppender("AzureStorageLogAppender") as AveAzureStorageLogAppender;
            return appender;
        }
    }
}
