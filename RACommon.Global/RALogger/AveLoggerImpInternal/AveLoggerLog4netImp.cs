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




namespace AvePoint.RA.CommonUtil
{
    using AvePoint.RA.Common.Cache;
    #region using directives
    using AvePoint.RA.Contract.Services;
    using AvePoint.RA.Contract.Tenant;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    #endregion

    /// <summary>
    /// 日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    internal class AveLoggerImp : IAveLoggerImp
    {
        private static string defaultConfigurationFile = string.Empty;

        Type loggingType;
        ILog log4NetLog;

        readonly static object startLoggingFlushThreadLock = new object();
        static bool loggingFlushThreadStarted = false;
        static volatile bool enableCacheMode = true;
        static volatile bool hasOutput = false;
        static List<string> threadNames = new List<string>();
        static List<ILog> loggerEntries = new List<ILog>();
        static List<LoggingEvent> loggingEntries = new List<LoggingEvent>();
        static FieldInfo mThreadNameField = null;
        static FieldInfo ThreadNameField
        {
            get
            {
                if (mThreadNameField == null)
                {
                    Thread t1 = Thread.CurrentThread;
                    var field = t1.GetType().GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null)
                    {
                        field = t1.GetType().GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic);
                    }
                    mThreadNameField = field;
                }
                return mThreadNameField;
            }
        }

        static AveLoggerImp()
        {
            if (log4net.GlobalContext.Properties["RelatedPath"] == null)
            {
                log4net.GlobalContext.Properties["RelatedPath"] = string.Empty;
            }
            if (log4net.GlobalContext.Properties["ProcessName"] == null)
            {
                log4net.GlobalContext.Properties["ProcessName"] = Process.GetCurrentProcess().ProcessName + ".exe";
            }
            var log4netConfigurationFile = GetConfigFileName();
            ApplyConfigFile(log4netConfigurationFile);
        }

        public AveLoggerImp(Type type)
        {
            this.loggingType = type;
            this.log4NetLog = LogManager.GetLogger(type);
            EnsureLoggingFlushThreadStarted();
        }

        public AveLoggerImp(Type type, string loggerName)
        {
            this.loggingType = type;
            this.log4NetLog = LogManager.GetLogger(loggerName);
            EnsureLoggingFlushThreadStarted();
        }

        public static string DefaultConfigurationFile { get { return defaultConfigurationFile; } }
        public AveLogLevel CurrentLogLevel { get { return (AveLogLevel)((Hierarchy)LogManager.GetRepository()).Root.Level.Value; } }
        public bool IsErrorEnabled { get { return log4NetLog.IsErrorEnabled; } }
        public bool IsWarnEnabled { get { return log4NetLog.IsWarnEnabled; } }
        public bool IsInfoEnabled { get { return log4NetLog.IsInfoEnabled; } }
        public bool IsDebugEnabled { get { return log4NetLog.IsDebugEnabled; } }

        public void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource, Exception e)
        {

            log4NetLog = LogManager.GetLogger(loggingType);

            ILoggerRepository repository = log4NetLog.Logger.Repository;
            string loggerName = log4NetLog.Logger.Name;

            Level log4netLevel = new Level((int)level, level.ToString());
            string loggingMsg = msg;
            if (e != null)
            {
                loggingMsg = string.Format("{0}\n{1}", msg, e.ToString());
            }
            try
            {
                WriteAzureEntry(loggingMsg, level);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
            LoggingEvent loggingEntry = new LoggingEvent(loggingType, repository, loggerName, log4netLevel, loggingMsg, null);
            loggingEntry.Properties["EventID"] = eventId;
            loggingEntry.Properties["TenantGroup"] = !string.IsNullOrEmpty(TenantLocalValue.LogonGroupId) ? TenantLocalValue.LogonGroupId : "None Group";
            loggingEntry.Properties["TenantUser"] = !string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail) ? TenantLocalValue.LogonUserEmail : "None User";
            loggingEntry.Properties["TenantIdentity"] = CallContext.GetData("TenantIdentity");
            loggingEntry.Properties["ThreadJobId"] = CallContext.GetData("ThreadJobId");
            loggingEntry.Properties["TraceId"] = string.IsNullOrEmpty(TenantLocalValue.TraceId) ? "None TraceId" : TenantLocalValue.TraceId;


            if (!enableCacheMode)
            {
                log4NetLog.Logger.Log(loggingEntry);
            }
            else
            {
                lock (loggingEntries)
                {
                    string currentThreadName = Thread.CurrentThread.Name;
                    if (currentThreadName == null) currentThreadName = Thread.CurrentThread.ManagedThreadId.ToString();
                    threadNames.Add(currentThreadName);
                    loggerEntries.Add(log4NetLog);
                    loggingEntries.Add(loggingEntry);
                }
            }
        }

        public void WriteAzureEntry(string msg, AveLogLevel level)
        {
            string type = loggingType.ToString();
            switch (level)
            {
                case AveLogLevel.DEBUG:
                case AveLogLevel.INFO:
                    Trace.TraceInformation(string.Format("{0}, {1}", type, msg));
                    break;
                case AveLogLevel.WARN:
                    Trace.TraceWarning(string.Format("{0}, {1}", type, msg));
                    break;
                case AveLogLevel.ERROR:
                    Trace.TraceError(string.Format("{0}, {1}", type, msg));
                    break;
            }
        }

        private static void EnsureLoggingFlushThreadStarted()
        {
            //if (RALogger.DisableLogCacheMode)
            //{
            //    return;
            //}

            lock (startLoggingFlushThreadLock)
            {
                if (loggingFlushThreadStarted) return;
                loggingFlushThreadStarted = true;
                Thread t = new Thread(FlushLogEntries);
                t.IsBackground = true;
                t.Name = "FlushLogEntries";
                t.Start();

                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            WaitForAllLogsFlush();
            
        }

        internal static void WaitForAllLogsFlush()
        {
            while (true)
            {
                lock (loggingEntries)
                {
                    if (!enableCacheMode) return;
                    if (loggingEntries.Count == 0 && !hasOutput) return;
                }
                Thread.Sleep(2000);
            }
        }

        private static void FlushLogEntries()
        {
            try
            {
                enableCacheMode = true;
                List<string> tempThreadNames = new List<string>();
                List<ILog> tempLoggerEntries = new List<ILog>();
                List<LoggingEvent> tempLoggingEntries = new List<LoggingEvent>();
                while (true)
                {
                    lock (loggingEntries)
                    {
                        if (loggingEntries.Count == 0)
                        {
                            hasOutput = false;
                        }
                        else
                        {
                            tempThreadNames = threadNames.GetRange(0, threadNames.Count);
                            tempLoggerEntries = loggerEntries.GetRange(0, loggerEntries.Count);
                            tempLoggingEntries = loggingEntries.GetRange(0, loggingEntries.Count);
                            threadNames.RemoveRange(0, threadNames.Count);
                            loggerEntries.RemoveRange(0, loggerEntries.Count);
                            loggingEntries.RemoveRange(0, loggingEntries.Count);
                            hasOutput = true;
                        }
                    }
                    if (!hasOutput)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }
                    else
                    {
                        for (int i = 0; i < tempLoggerEntries.Count; i++)
                        {
                            Thread t1 = Thread.CurrentThread;
                            //setThreadName(t1, null);
                            ThreadNameField?.SetValue(t1, null);
                            t1.Name = tempThreadNames[i];
                            //t1.GetType().GetMethod("InformThreadNameChangeEx", BindingFlags.NonPublic | BindingFlags.Static).Invoke(t1, new object[] { t1, t1.Name });

                            var logEntity = tempLoggingEntries[i];
                            // why set the value here?
                            if (!string.IsNullOrEmpty(logEntity.Properties["TenantIdentity"]?.ToString()) && !string.IsNullOrEmpty(logEntity.Properties["ThreadJobId"]?.ToString())) 
                            {
                                CallContext.SetData("TenantIdentity", logEntity.Properties["TenantIdentity"]);
                                CallContext.SetData("ThreadJobId", logEntity.Properties["ThreadJobId"]);
                            }
                            
                            tempLoggerEntries[i].Logger.Log(logEntity);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                enableCacheMode = false;
                Trace.TraceError("Flush log entries thread exception. {0}", e.ToString());
            }
        }

        /// <summary>
        /// 根据目前配置文件中的RollingFileAppender信息创建一个同样属性的RollingFileAppender信息
        /// </summary>
        /// <param name="appender">配置文件中已有的Appender</param>
        /// <param name="jobId">Job Id作为新Appender的名字</param>
        /// <param name="cPostfix">当前的前缀</param>
        /// <returns>true：Clone成功；false：由于无File节点导致Clone失败</returns>
        /*private static bool CloneAppenderNode(RollingFileAppender appender, string jobId, string cPostfix)
        {
            try
            {
                if (!string.IsNullOrEmpty(appender.File))
                {
                    const string ext = ".log";

                    var logFilePath = Path.Combine(Path.GetDirectoryName(appender.File), log4net.Util.SystemInfo.ApplicationFriendlyName);

                    var rfa = new RollingFileAppender
                    {
                        Name = jobId,
                        AppendToFile = appender.AppendToFile,
                        LockingModel = appender.LockingModel,
                        RollingStyle = appender.RollingStyle,
                        MaxFileSize = appender.MaxFileSize,
                        MaxSizeRollBackups = appender.MaxSizeRollBackups,
                        File = logFilePath + cPostfix + ext,
                        Layout = appender.Layout
                    };

                    rfa.ActivateOptions();

                    ILoggerRepository repository = LogManager.CreateRepository(jobId);
                    BasicConfigurator.Configure(repository, rfa);

                    return true;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }

            return false;
        }*/

        private static string GetConfigFileName()
        {
            var configFile = RALogger.ConfigFile ?? ConfigurationManager.AppSettings["log4net"];
            if (configFile != null)
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
            }

            throw new Exception("log4net config file not set.");

            //var log4netConfigurationFile = string.Empty;
            //var timerConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TimerLog4net.config");
            //var workerConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"WorkerLog4net.config");
            //var webConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Config{Path.DirectorySeparatorChar}WebLog4net.config");
            //if (File.Exists(Log4NetGlobalConfig.ConfigFile))
            //{
            //    log4netConfigurationFile = Log4NetGlobalConfig.ConfigFile;
            //}
            //else if (File.Exists(timerConfig))
            //{
            //    log4netConfigurationFile = timerConfig;
            //}
            //else if (File.Exists(workerConfig))
            //{
            //    log4netConfigurationFile = workerConfig;
            //}
            //else if (File.Exists(webConfig))
            //{
            //    log4netConfigurationFile = webConfig;
            //}
            //return log4netConfigurationFile;
        }

        private static void ApplyConfigFile(string log4netConfigurationFile)
        {
            if (!string.IsNullOrEmpty(log4netConfigurationFile))
            {
                defaultConfigurationFile = log4netConfigurationFile;
                XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netConfigurationFile));
            }
            else
            {
                if (System.Configuration.ConfigurationManager.GetSection("log4net") != null)
                {
                    XmlConfigurator.Configure();
                }
            }
        }
    }

    public class Log4NetGlobalConfig
    {
        public static string ConfigFile;
        public static string LogPath { set { log4net.GlobalContext.Properties["RelatedPath"] = value; } }
        public static string ProcessName { set { log4net.GlobalContext.Properties["ProcessName"] = value; } }
    };
}