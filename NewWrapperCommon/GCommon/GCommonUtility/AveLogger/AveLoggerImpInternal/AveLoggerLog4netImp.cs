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
    #region using directives

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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    #endregion using directives

    /// <summary>
    /// 日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveLoggerLog4netImp is unmodifiable as the cause of being referenced.")]
    internal class AveLoggerLog4netImp : AveLoggerAbstractImp
    {
        private ILog log4NetLog;

        #region --GlobalInitialize--

        private static string defaultConfigurationFile = string.Empty;
        private static bool globalInitialized = false;
        private static Object locker = new Object();
        private static object _lockObj = new object();
        private static FieldInfo mThreadNameField;
        private static FieldInfo ThreadNameField
        {
            get
            {
                if (mThreadNameField == null)
                {
                    Thread t1 = Thread.CurrentThread;
                    mThreadNameField = t1.GetType().GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return mThreadNameField;
            }
        }
        //[MethodImpl(MethodImplOptions.Synchronized)]
        private static void GlobalInitialize()
        {
            lock (locker)
            {
                if (!globalInitialized)
                {
                    if (log4net.GlobalContext.Properties["RelatedPath"] == null)
                    {
                        log4net.GlobalContext.Properties["RelatedPath"] = string.Empty;
                    }
                    if (log4net.GlobalContext.Properties["ProcessName"] == null)
                    {
                        log4net.GlobalContext.Properties["ProcessName"] = Process.GetCurrentProcess().ProcessName + ".exe";
                    }
                    if (log4net.GlobalContext.Properties["LogFilePostfix"] == null)
                    {
                        SetLogFilePostfix();
                    }
                    defaultConfigurationFile = GetConfigFileName();
                    ApplyConfigFile();
                    globalInitialized = true;
                }
            }
        }

        private static string GetCurrentDomainBaseDirectoryParent()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string parentDir = new DirectoryInfo(baseDir).Parent.FullName;
            return parentDir;
        }

        private static string GetConfigFileName()
        {
            var process = Process.GetCurrentProcess();
            if (process.ProcessName.Equals("CloudAgentService"))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\AgentLog4net.config");
                if (File.Exists(path))
                {
                    return path;
                }
            }
            if (process.ProcessName.Equals("RecordsAgentWorker"))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSLog4net.config");
                if (File.Exists(path))
                {
                    return path;
                }
            }
            if (process.ProcessName.Equals("RecordsAgentBrowser"))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\BrowserLog4net.config");
                if (File.Exists(path))
                {
                    return path;
                }
            }
            var log4netConfigurationFile = string.Empty;
            var agentConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgentLog4net.config");
            var mediaConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaLog4Net.config");
            var reportingConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportCenterLog4Net.config");
            var controlWebConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\ControlLog4net.config");
            var controlTimerConfig = Path.Combine(GetCurrentDomainBaseDirectoryParent(), @"Config\ControlTimerLog4net.config");
            var deploymentConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\CommonDeploymentLog4net.config");
            var deploymentToolConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\CommonDeploymentToolLog4net.config");
            var governanceAutomationConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\GovernanceAutomationLog4net.config");
            var patchInstallerConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Etc\InstallLog4net.config");
            if (File.Exists(Log4NetGlobalConfig.ConfigFile))
            {
                log4netConfigurationFile = Log4NetGlobalConfig.ConfigFile;
            }
            else if (File.Exists(agentConfig))
            {
                log4netConfigurationFile = agentConfig;
            }
            else if (File.Exists(mediaConfig))
            {
                log4netConfigurationFile = mediaConfig;
            }
            else if (File.Exists(reportingConfig))
            {
                log4netConfigurationFile = reportingConfig;
            }
            else if (File.Exists(controlWebConfig))
            {
                log4netConfigurationFile = controlWebConfig;
            }
            else if (File.Exists(controlTimerConfig))
            {
                log4netConfigurationFile = controlTimerConfig;
            }
            else if (File.Exists(deploymentConfig))
            {
                log4netConfigurationFile = deploymentConfig;
            }
            else if (File.Exists(deploymentToolConfig))
            {
                log4netConfigurationFile = deploymentToolConfig;
            }
            else if (File.Exists(governanceAutomationConfig))
            {
                log4netConfigurationFile = governanceAutomationConfig;
            }
            else if (File.Exists(patchInstallerConfig))
            {
                log4netConfigurationFile = patchInstallerConfig;
            }
            return log4netConfigurationFile;
        }

        private static void ApplyConfigFile()
        {
            if (!string.IsNullOrEmpty(defaultConfigurationFile))
            {
                ChangeConfigurationFile(defaultConfigurationFile);
                XmlConfigurator.ConfigureAndWatch(new FileInfo(defaultConfigurationFile));
            }
            else
            {
                if (System.Configuration.ConfigurationManager.GetSection("log4net") != null)
                {
                    XmlConfigurator.Configure();
                }
            }
        }

        /// <summary>
        /// Change Configuration File
        /// </summary>
        /// <param name="defaultConfigurationFile"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ChangeConfigurationFile is unmodifiable as the cause of being referenced.")]
        private static void ChangeConfigurationFile(string defaultConfigurationFile)
        {
            try
            {
                if (!string.IsNullOrEmpty(defaultConfigurationFile))
                {
                    if (File.Exists(defaultConfigurationFile))
                    {
                        XmlDocument document = new XmlDocument();
                        document.Load(defaultConfigurationFile);

                        var nodes = document.SelectNodes("/log4net/appender");
                        bool changed = false;

                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (XmlNode node in nodes)
                            {
                                if (node is XmlElement)
                                {
                                    var xe = node as XmlElement;
                                    var name = xe.GetAttribute("name");
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        if (name.Equals("LogFileAppender", StringComparison.Ordinal) || name.Equals("HighLevelFileLogAppender", StringComparison.Ordinal))
                                        {
                                            XmlElement encodingNode = null;
                                            foreach (XmlNode subNode in xe.ChildNodes)
                                            {
                                                if (subNode.Name.Equals("encoding", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    encodingNode = subNode as XmlElement;
                                                    break;
                                                }
                                            }
                                            if (encodingNode == null)
                                            {
                                                encodingNode = document.CreateElement("encoding");
                                                encodingNode.SetAttribute("value", "UTF-8");
                                                xe.PrependChild(encodingNode);
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (changed)
                        {
                            document.Save(defaultConfigurationFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Change Configuration File {0} failed:{1}", defaultConfigurationFile, ex);
            }
        }

        private static void SetLogFilePostfix()
        {
            Process process = Process.GetCurrentProcess();
            if (string.Equals(process.ProcessName, "AgentService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonRestartService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonPostInstall", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013AgentCommonBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016AgentCommonBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSEAgentCommonBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonPRBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010ReplicatorListener", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010ReplicatorAnalyzer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010StorageOptimizationService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013StorageOptimizationService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016StorageOptimizationService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019StorageOptimizationService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSEStorageOptimizationService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010ConnectorProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013ConnectorProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010PreviewService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013PreviewService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2007AgentCommonRoleChecker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010AgentCommonRoleChecker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonSPRoleChecker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010StorageProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013StorageProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016StorageProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019StorageProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSEStorageProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonPRLiveModeBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonVDBFileServer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonVDBDriverInstaller", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010ReportCenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013ReportCenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016ReportCenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019ReportCenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSEReportCenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSERCAuditor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019RCAuditor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016RCAuditor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013RCAuditor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010RCAuditor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010CentralAdminWorker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013CentralAdminWorker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016CentralAdminWorker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019CentralAdminWorker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SPSECentralAdminWorker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010GovernanceAutomation", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "MediaService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonMigrationBrowser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonAPIUtility", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013AgentCommonAPIUtility", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010HealthAnalyzer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonHealthAnalyzer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "AgentCommonReplicatorService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "MediaPlatformBackupExecuter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010ReportCenterUsagePatternListener", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013ReportCenterUsagePatternListener", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2016ReportCenterUsagePatternListener", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2019ReportCenterUsagePatternListener", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2007HSExportProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2010HSExportProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "SP2013HSExportProcessor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "CloudAgentService", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "RecordsAgentBrowser", StringComparison.OrdinalIgnoreCase))
            {
                log4net.GlobalContext.Properties["LogFilePostfix"] = string.Empty;
            }
            else
            {
                string format = new StringBuilder().Append("MM").Append('d').Append('d').Append("HH").Append('m').Append('m').Append('s').Append('s').ToString();
                string timestamp = DateTime.Now.ToString(format);
                string pid = Process.GetCurrentProcess().Id.ToString();
                log4net.GlobalContext.Properties["LogFilePostfix"] = "_" + timestamp + "_" + pid;
            }
        }

        #endregion --GlobalInitialize--

        #region --Enable single logging thread started--

        private static object startLoggingFlushThreadLock = new object();
        private static bool loggingFlushThreadStarted = false;
        private static volatile bool enableCacheMode = true;
        private static volatile bool hasOutput = false;
        private static List<string> threadNames = new List<string>();
        private static List<ILog> loggerEntries = new List<ILog>();
        private static List<LoggingEvent> loggingEntries = new List<LoggingEvent>();

        private static void EnsureLoggingFlushThreadStarted()
        {
            string disableLogCacheMode = ConfigurationManager.AppSettings["disableLogCacheMode"];
            if (!string.IsNullOrEmpty(disableLogCacheMode))
            {
                if (bool.Parse(disableLogCacheMode)) return;
            }

            lock (startLoggingFlushThreadLock)
            {
                if (loggingFlushThreadStarted) return;
                loggingFlushThreadStarted = true;
                enableCacheMode = true;
                Thread t = new Thread(FlushLogEntries);
                t.IsBackground = true;
                t.Name = "FlushLogEntries";
                t.Start();

                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            new AveLoggerLog4netImp().WaitForAllLogsFlush();
        }

        private static void FlushLogEntries()
        {
            try
            {
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
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        for (int i = 0; i < tempLoggerEntries.Count; i++)
                        {
                            Thread t1 = Thread.CurrentThread;
                            ThreadNameField?.SetValue(t1, null);
                            t1.Name = tempThreadNames[i];
                            //t1.GetType().GetMethod("InformThreadNameChangeEx", BindingFlags.NonPublic | BindingFlags.Static).Invoke(t1, new object[] { t1, t1.Name });
                            //MethodInfo mi = t1.GetType().GetMethod("InformThreadNameChangeEx", BindingFlags.NonPublic | BindingFlags.Static);
                            //if (mi != null)
                            //{
                            //    mi.Invoke(t1, new object[] { t1, t1.Name });
                            //}
                            //else
                            //{
                            //    mi = t1.GetType().GetMethod("InformThreadNameChange", BindingFlags.NonPublic | BindingFlags.Static);
                            //    MethodInfo nativeHandle = t1.GetType().GetMethod("GetNativeHandle", BindingFlags.NonPublic | BindingFlags.Instance);
                            //    object threadHandle = nativeHandle.Invoke(t1, new object[] { });
                            //    string threadName = t1.Name;
                            //    int len = (t1.Name != null) ? t1.Name.Length : 0;
                            //    mi.Invoke(t1, new object[] { threadHandle, threadName, len });
                            //}
                            tempLoggerEntries[i].Logger.Log(tempLoggingEntries[i]);
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

        #endregion --Enable single logging thread started--

        public override bool IsAgentContext()
        {
            return defaultConfigurationFile.EndsWith("AgentLog4net.config", StringComparison.OrdinalIgnoreCase);
        }

        public override bool IsMediaContext()
        {
            return defaultConfigurationFile.EndsWith("MediaLog4Net.config", StringComparison.OrdinalIgnoreCase);
        }

        public override AveLogLevel CurrentLogLevel { get { return (AveLogLevel)((Hierarchy)LogManager.GetRepository()).Root.Level.Value; } }

        public override bool IsErrorEnabled { get { return log4NetLog.IsErrorEnabled; } }

        public override bool IsWarnEnabled { get { return log4NetLog.IsWarnEnabled; } }

        public override bool IsInfoEnabled { get { return log4NetLog.IsInfoEnabled; } }

        public override bool IsDebugEnabled { get { return log4NetLog.IsDebugEnabled; } }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SetJobId is unmodifiable as the cause of being referenced.")]
        public override void SetJobId(string jobId, bool mergeOldFile)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }
            var log4netConfigurationFile = GetConfigFileName();
            var previousLogFilePostfix = log4net.GlobalContext.Properties["LogFilePostfix"] as string;
            var currentLogFilePostfix = "_" + jobId;

            //先更改文件路径再处理之前的log文件
            log4net.GlobalContext.Properties["LogFilePostfix"] = currentLogFilePostfix;
            ApplyConfigFile();

            if (File.Exists(defaultConfigurationFile))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(defaultConfigurationFile);
                    foreach (XmlElement appender in doc.SelectNodes("log4net/appender"))
                    {
                        if (string.Equals(appender.GetAttribute("type"), "log4net.Appender.RollingFileAppender", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(appender.GetAttribute("type"), "AvePoint.GCommon.AveHighLevelFileLogAppender", StringComparison.OrdinalIgnoreCase))
                        {
                            var fileNode = appender.SelectSingleNode("file") as XmlElement;
                            if (fileNode != null)
                            {
                                var logFilePath = fileNode.GetAttribute("value");

                                //用log4net的方法按照log4net的内部实现进行替换。
                                logFilePath = logFilePath.Replace("%property{RelatedPath}", log4net.GlobalContext.Properties["RelatedPath"].ToString());
                                logFilePath = logFilePath.Replace("%property{ProcessName}", log4net.GlobalContext.Properties["ProcessName"].ToString());
                                var previousLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", previousLogFilePostfix));
                                var currentLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", currentLogFilePostfix));
                                if (File.Exists(previousLogFilePath))
                                {
                                    if (mergeOldFile)
                                    {
                                        if (!File.Exists(currentLogFilePath))
                                        {
                                            File.Move(previousLogFilePath, currentLogFilePath);
                                        }
                                        else
                                        {
                                            File.AppendAllText(currentLogFilePath, File.ReadAllText(previousLogFilePath, Encoding.UTF8), Encoding.UTF8);
                                            File.Delete(previousLogFilePath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
        }


        /// <summary>
        /// 根据Tenant Account Id区分用户Log(区分非Job的Log)
        /// </summary>
        /// <param name="tenantAccountId">指定的Tenant Account Id</param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SeparateLogByTenant is unmodifiable as the cause of being referenced.")]
        public override void SeparateLogByTenant(string tenantAccountId, string tenantAccountName)
        {
            string tenantFolderName = tenantAccountName;
            if (string.IsNullOrEmpty(tenantFolderName))
            {
                return;
            }

            lock (_lockObj)
            {
                var log4netConfigurationFile = GetConfigFileName();
                if (File.Exists(log4netConfigurationFile))
                {
                    try
                    {
                        var doc = new XmlDocument();
                        doc.Load(log4netConfigurationFile);
                        var currentLogFilePath = string.Empty;
                        string previousRelatedPath = log4net.GlobalContext.Properties["RelatedPath"].ToString();
                        foreach (XmlElement appender in doc.SelectNodes("log4net/appender"))
                        {
                            if (string.Equals(appender.GetAttribute("type"), "AvePoint.GCommon.AveSeparativeLogAppender"))
                            {
                                var fileNode = appender.SelectSingleNode("file") as XmlElement;
                                if (fileNode != null)
                                {
                                    var logFilePath = fileNode.GetAttribute("value");
                                    var tempFilePath = logFilePath;
                                    logFilePath = logFilePath.Replace("%property{TenantFolderName}", tenantFolderName);
                                    logFilePath = logFilePath.Replace("%property{RelatedPath}", previousRelatedPath);
                                    string baseLocation = logFilePath.Substring(0, logFilePath.LastIndexOf('\\'));
                                    if (!Directory.Exists(baseLocation))
                                    {
                                        try
                                        {
                                            Directory.CreateDirectory(baseLocation);
                                        }
                                        catch (ArgumentException argExp)
                                        {
                                            if (argExp.Message.Equals("Illegal characters in path."))   //if email contains illegal characters, create a tenant log folder using tenant id
                                            {
                                                logFilePath = tempFilePath;
                                                tenantFolderName = tenantAccountId;
                                                logFilePath = logFilePath.Replace("%property{TenantFolderName}", tenantFolderName);
                                                baseLocation = logFilePath.Substring(0, logFilePath.LastIndexOf('\\'));
                                                if (!Directory.Exists(baseLocation))
                                                {
                                                    Directory.CreateDirectory(baseLocation);
                                                }
                                            }
                                        }
                                    }

                                    logFilePath = logFilePath.Replace("%property{ProcessName}", log4net.GlobalContext.Properties["ProcessName"].ToString());
                                    currentLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", ""));
                                }
                                break;
                            }
                        }
                        if (CheckRepositoryExists(tenantFolderName))
                        {
                            return;
                        }
                        var h = LogManager.GetRepository() as Hierarchy;
                        if (h == null) return;

                        foreach (var appender in h.GetAppenders())
                        {
                            if (appender is RollingFileAppender)
                            {
                                CloneSeparativeLogAppenderNode(appender as RollingFileAppender, tenantFolderName, currentLogFilePath);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 根据Tenant Account Id和Job Id区分用户Log
        /// </summary>
        /// <param name="jobId">指定Job的Job Id</param>
        /// <param name="tenantAccountId">指定的Tenant Account Id</param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SeparateLogByTenant is unmodifiable as the cause of being referenced.")]
        public override void SeparateLogByTenant(string jobId, string tenantAccountId, string tenantAccountName)
        {
            string tenantFolderName = tenantAccountName;
            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(tenantFolderName))
            {
                return;
            }
            var log4netConfigurationFile = GetConfigFileName();
            var previousLogFilePostfix = log4net.GlobalContext.Properties["LogFilePostfix"] as string;
            var currentLogFilePostfix = "_" + jobId;

            if (File.Exists(log4netConfigurationFile))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(log4netConfigurationFile);
                    var previousLogFilePath = string.Empty;
                    var currentLogFilePath = string.Empty;
                    var currentHighLogFilePath = string.Empty;
                    var previousHighLogFilePath = string.Empty;
                    string previousRelatedPath = log4net.GlobalContext.Properties["RelatedPath"].ToString();
                    string previousProcessName = log4net.GlobalContext.Properties["ProcessName"].ToString();
                    foreach (XmlElement appender in doc.SelectNodes("log4net/appender"))
                    {
                        if (string.Equals(appender.GetAttribute("type"), "log4net.Appender.RollingFileAppender"))
                        {
                            var fileNode = appender.SelectSingleNode("file") as XmlElement;
                            if (fileNode != null)
                            {
                                var logFilePath = fileNode.GetAttribute("value");
                                logFilePath = logFilePath.Replace("%property{RelatedPath}", previousRelatedPath);
                                var currentLogTemp = logFilePath.Replace("%property{ProcessName}", Path.Combine(tenantFolderName, previousProcessName));
                                currentLogTemp = currentLogTemp.Replace("%property{ProcessName}", previousProcessName);
                                currentLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(currentLogTemp.Replace("%property{LogFilePostfix}", currentLogFilePostfix));
                                string baseLocation = currentLogFilePath.Substring(0, currentLogFilePath.LastIndexOf('\\'));
                                if (!Directory.Exists(baseLocation))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(baseLocation);
                                    }
                                    catch (ArgumentException argExp)
                                    {
                                        if (argExp.Message.Equals("Illegal characters in path."))   //if email contains illegal characters, create a tenant log folder using tenant id
                                        {
                                            tenantFolderName = tenantAccountId;
                                            currentLogTemp = logFilePath.Replace("%property{ProcessName}", Path.Combine(tenantFolderName, previousProcessName));
                                            baseLocation = logFilePath.Substring(0, logFilePath.LastIndexOf('\\'));
                                            if (!Directory.Exists(baseLocation))
                                            {
                                                Directory.CreateDirectory(baseLocation);
                                            }
                                        }
                                    }
                                }
                                logFilePath = logFilePath.Replace("%property{ProcessName}", previousProcessName);
                                previousLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", previousLogFilePostfix));
                                currentHighLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(currentLogTemp.Replace("%property{LogFilePostfix}", currentLogFilePostfix + "_High"));
                                //先更改文件路径再处理之前的log文件
                                log4net.GlobalContext.Properties["RelatedPath"] = logFilePath.Substring(0, logFilePath.IndexOf("\\Logs", StringComparison.OrdinalIgnoreCase));
                                log4net.GlobalContext.Properties["ProcessName"] = Path.Combine(tenantFolderName, log4net.GlobalContext.Properties["ProcessName"].ToString());
                                log4net.GlobalContext.Properties["LogFilePostfix"] = currentLogFilePostfix;
                                defaultConfigurationFile = log4netConfigurationFile;
                                ApplyConfigFile();
                            }
                        }
                        else if (string.Equals(appender.GetAttribute("type"), "AvePoint.GCommon.AveHighLevelFileLogAppender"))
                        {
                            var fileNode = appender.SelectSingleNode("file") as XmlElement;
                            if (fileNode != null)
                            {
                                var logFilePath = fileNode.GetAttribute("value");
                                logFilePath = logFilePath.Replace("%property{RelatedPath}", previousRelatedPath);
                                logFilePath = logFilePath.Replace("%property{ProcessName}", previousProcessName);
                                previousHighLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", previousLogFilePostfix));
                            }
                        }
                    }
                    if (File.Exists(previousLogFilePath))
                    {
                        if (!File.Exists(currentLogFilePath))
                        {
                            File.Move(previousLogFilePath, currentLogFilePath);
                        }
                        else
                        {
                            File.AppendAllText(currentLogFilePath, File.ReadAllText(previousLogFilePath, Encoding.Default), Encoding.Default);
                            File.Delete(previousLogFilePath);
                        }
                    }
                    if (File.Exists(previousHighLogFilePath))
                    {
                        if (!File.Exists(currentHighLogFilePath))
                        {
                            File.Move(previousHighLogFilePath, currentHighLogFilePath);
                        }
                        else
                        {
                            File.AppendAllText(currentHighLogFilePath, File.ReadAllText(previousHighLogFilePath, Encoding.Default), Encoding.Default);
                            File.Delete(previousHighLogFilePath);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
        }

        public override void SetLogLevel(AveLogLevel logLevel)
        {
            string level = "INFO";
            switch (logLevel)
            {
                case AveLogLevel.ERROR:
                    level = "ERROR";
                    break;
                case AveLogLevel.WARN:
                    level = "WARN";
                    break;
                case AveLogLevel.INFO:
                    level = "INFO";
                    break;
                case AveLogLevel.DEBUG:
                    level = "DEBUG";
                    break;
                default:
                    level = "INFO";
                    break;
            }
            SetLog4netLevel(level);
        }

        private void SetLog4netLevel(string level)
        {
            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();
            //Configure all loggers to be at the debug level.
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                repository.Threshold = repository.LevelMap[level];
                log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                foreach (log4net.Core.ILogger logger in loggers)
                {
                    ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];
                }
            }
            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];

        }

        #region -- SetThhreadJobId--

        /// <summary>
        /// 根据Job Id创建一个以Job Id为名字的Logger对象，并将Job Id设置到ThreadContext中，以便写Log的时候找到对应的Logger
        /// </summary>
        /// <param name="jobId">指定Job的Job Id</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void SetThreadJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return;
            }
            if (CheckRepositoryExists(jobId))
            {
                return;
            }

            var previousLogFilePostfix = GlobalContext.Properties["LogFilePostfix"] as string;
            var currentLogFilePostfix = "_" + jobId;

            //先更改文件路径再处理之前的log文件
            ApplyConfigFile();

            if (!string.IsNullOrEmpty(previousLogFilePostfix) && File.Exists(defaultConfigurationFile))
            {
                try
                {
                    var h = LogManager.GetRepository() as Hierarchy;

                    if (h == null) return;

                    foreach (var appender in h.GetAppenders())
                    {
                        if (appender is AveHighLevelFileLogAppender)
                        {
                            CloneHighLevelFileLogAppenderNode(appender as AveHighLevelFileLogAppender, jobId, currentLogFilePostfix, true);
                        }
                        else if (appender is RollingFileAppender)
                        {
                            CloneRollingFileAppenderNode(appender as RollingFileAppender, jobId, currentLogFilePostfix, true);
                        }
                        else if (appender is AveHighLevelEventLogAppender)
                        {
                            CloneHighLevelEventLogAppenderNode(appender as AveHighLevelEventLogAppender, jobId, currentLogFilePostfix);
                        }

                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SetTreadLogTenantAndJobId is unmodifiable as the cause of being referenced.")]
        public override void SetTreadLogTenantAndJobId(string jobId, string tenantAccountId, string tenantAccountName)
        {
            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(tenantAccountName))
            {
                return;
            }
            if (!string.IsNullOrEmpty(jobId) && CheckRepositoryExists(jobId))
            {
                return;
            }
            string previousProcessName = log4net.GlobalContext.Properties["ProcessName"].ToString();
            var previousLogFilePostfix = log4net.GlobalContext.Properties["LogFilePostfix"] as string;
            var previousLogFilePath = string.Empty;
            var previousHighLogFilePath = string.Empty;
            #region check and set location
            if (!previousProcessName.StartsWith(tenantAccountName, StringComparison.OrdinalIgnoreCase) && !previousProcessName.StartsWith(tenantAccountId, StringComparison.OrdinalIgnoreCase))
            {
                string previousRelatedPath = log4net.GlobalContext.Properties["RelatedPath"].ToString();
                var log4netConfigurationFile = GetConfigFileName();
                if (File.Exists(log4netConfigurationFile))
                {
                    #region load configuration file and check log folder exist
                    var doc = new XmlDocument();
                    doc.Load(log4netConfigurationFile);
                    foreach (XmlElement appender in doc.SelectNodes("log4net/appender"))
                    {
                        if (string.Equals(appender.GetAttribute("type"), "log4net.Appender.RollingFileAppender"))
                        {
                            var fileNode = appender.SelectSingleNode("file") as XmlElement;
                            if (fileNode != null)
                            {
                                var logFilePath = fileNode.GetAttribute("value");
                                logFilePath = logFilePath.Replace("%property{RelatedPath}", previousRelatedPath);
                                var currentLogTemp = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{ProcessName}", Path.Combine(tenantAccountName, previousProcessName)));
                                string baseLocation = currentLogTemp.Substring(0, currentLogTemp.LastIndexOf('\\'));
                                bool noSet = false;
                                if (!Directory.Exists(baseLocation))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(baseLocation);
                                        log4net.GlobalContext.Properties["ProcessName"] = Path.Combine(tenantAccountName, log4net.GlobalContext.Properties["ProcessName"].ToString());
                                        noSet = true;
                                    }
                                    catch (ArgumentException argExp)
                                    {
                                        if (argExp.Message.Equals("Illegal characters in path."))   //if email contains illegal characters, create a tenant log folder using tenant id
                                        {
                                            currentLogTemp = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{ProcessName}", Path.Combine(tenantAccountId, previousProcessName)));
                                            baseLocation = logFilePath.Substring(0, logFilePath.LastIndexOf('\\'));
                                            if (!Directory.Exists(baseLocation))
                                            {
                                                Directory.CreateDirectory(baseLocation);
                                                log4net.GlobalContext.Properties["ProcessName"] = Path.Combine(tenantAccountId, log4net.GlobalContext.Properties["ProcessName"].ToString());
                                                noSet = true;
                                            }
                                        }
                                    }
                                }
                                if (!noSet)
                                {
                                    log4net.GlobalContext.Properties["ProcessName"] = Path.Combine(tenantAccountName, log4net.GlobalContext.Properties["ProcessName"].ToString());
                                }
                                previousLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{ProcessName}", previousProcessName).Replace("%property{LogFilePostfix}", previousLogFilePostfix));
                            }

                        }
                        else if (string.Equals(appender.GetAttribute("type"), "AvePoint.GCommon.AveHighLevelFileLogAppender"))
                        {
                            var fileNode = appender.SelectSingleNode("file") as XmlElement;
                            if (fileNode != null)
                            {
                                var logFilePath = fileNode.GetAttribute("value");
                                logFilePath = logFilePath.Replace("%property{RelatedPath}", previousRelatedPath);
                                logFilePath = logFilePath.Replace("%property{ProcessName}", previousProcessName);
                                previousHighLogFilePath = log4net.Util.SystemInfo.ConvertToFullPath(logFilePath.Replace("%property{LogFilePostfix}", previousLogFilePostfix));
                            }
                        }

                    }
                    #endregion
                }
            }
            #endregion
            var currentLogFilePostfix = "_" + jobId;

            //先更改文件路径再处理之前的log文件
            ApplyConfigFile();
            if (!string.IsNullOrEmpty(previousLogFilePath) && File.Exists(previousLogFilePath))
            {
                File.Delete(previousLogFilePath);
            }
            if (!string.IsNullOrEmpty(previousHighLogFilePath) && File.Exists(previousHighLogFilePath))
            {
                File.Delete(previousHighLogFilePath);
            }

            if (File.Exists(defaultConfigurationFile))
            {
                try
                {
                    var h = LogManager.GetRepository() as Hierarchy;

                    if (h == null) return;

                    foreach (var appender in h.GetAppenders())
                    {
                        if (appender is AveHighLevelFileLogAppender)
                        {
                            CloneHighLevelFileLogAppenderNode(appender as AveHighLevelFileLogAppender, jobId, currentLogFilePostfix, true);
                        }
                        else if (appender is RollingFileAppender)
                        {
                            CloneRollingFileAppenderNode(appender as RollingFileAppender, jobId, currentLogFilePostfix, true);
                        }
                        else if (appender is AveHighLevelEventLogAppender)
                        {
                            CloneHighLevelEventLogAppenderNode(appender as AveHighLevelEventLogAppender, jobId, currentLogFilePostfix);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
            log4net.GlobalContext.Properties["ProcessName"] = previousProcessName;
        }
        /// <summary>
        /// 检查当前的环境中是否已经存在指定的Repository，避免多次创建出错
        /// </summary>
        /// <param name="repository">Repository名字，一般为Job Id</param>
        /// <returns>true：存在；false：不存在</returns>
        private bool CheckRepositoryExists(string repository)
        {
            try
            {
                log4net.LogicalThreadContext.Properties["ThreadJobId"] = repository;

                foreach (var r in LogManager.GetAllRepositories())
                {
                    if (r.Name == repository)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }

            return false;
        }

        /// <summary>
        /// 根据目前配置文件中的RollingFileAppender信息创建一个同样属性的RollingFileAppender信息
        /// </summary>
        /// <param name="appender">配置文件中已有的Appender</param>
        /// <param name="jobId">Job Id作为新Appender的名字</param>
        /// <param name="cPostfix">当前的前缀</param>
        /// <returns>true：Clone成功；false：由于无File节点导致Clone失败</returns>
        private bool CloneRollingFileAppenderNode(RollingFileAppender appender, string jobId, string cPostfix, bool removeOldFile = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(appender.File))
                {
                    if (removeOldFile && File.Exists(appender.File))
                    {
                        File.Delete(appender.File);
                    }
                    const string ext = ".log";

                    var logFilePath = appender.File.Substring(0, appender.File.LastIndexOf("\\", System.StringComparison.OrdinalIgnoreCase) + 1) + log4net.Util.SystemInfo.ApplicationFriendlyName;

                    var rfa = new RollingFileAppender
                    {
                        Name = jobId,
                        AppendToFile = appender.AppendToFile,
                        LockingModel = appender.LockingModel,
                        RollingStyle = appender.RollingStyle,
                        MaxFileSize = appender.MaxFileSize,
                        MaxSizeRollBackups = appender.MaxSizeRollBackups,
                        File = logFilePath + cPostfix + ext,
                        Layout = appender.Layout,
                        Encoding = Encoding.UTF8,
                        PreserveLogFileNameExtension = appender.PreserveLogFileNameExtension
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
        }

        /// <summary>
        /// 根据目前配置文件中的RollingFileAppender信息创建一个同样属性的RollingFileAppender信息
        /// </summary>
        /// <param name="appender">配置文件中已有的Appender</param>
        /// <param name="jobId">Job Id作为新Appender的名字</param>
        /// <param name="cPostfix">当前的前缀</param>
        /// <returns>true：Clone成功；false：由于无File节点导致Clone失败</returns>
        private bool CloneHighLevelFileLogAppenderNode(AveHighLevelFileLogAppender appender, string jobId, string cPostfix, bool removeOldFile = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(appender.File))
                {
                    if (removeOldFile && File.Exists(appender.File))
                    {
                        File.Delete(appender.File);
                    }
                    const string ext = ".log";

                    var logFilePath = appender.File.Substring(0, appender.File.LastIndexOf("\\", System.StringComparison.OrdinalIgnoreCase) + 1) + log4net.Util.SystemInfo.ApplicationFriendlyName;

                    var rfa = new AveHighLevelFileLogAppender
                    {
                        Name = jobId,
                        AppendToFile = appender.AppendToFile,
                        LockingModel = appender.LockingModel,
                        RollingStyle = appender.RollingStyle,
                        MaxFileSize = appender.MaxFileSize,
                        MaxSizeRollBackups = appender.MaxSizeRollBackups,
                        File = logFilePath + cPostfix + "_High" + ext,
                        Layout = appender.Layout,
                        Encoding = Encoding.UTF8,
                        PreserveLogFileNameExtension = appender.PreserveLogFileNameExtension
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
        }

        /// <summary>
        /// 根据目前配置文件中的AveHighLevelEventLogAppender信息创建一个同样属性的AveHighLevelEventLogAppender信息
        /// </summary>
        /// <param name="appender">配置文件中已有的Appender</param>
        /// <param name="jobId">Job Id作为新Appender的名字</param>
        /// <param name="cPostfix">当前的前缀</param>
        /// <returns>true：Clone成功；false：由于无File节点导致Clone失败</returns>
        private bool CloneHighLevelEventLogAppenderNode(AveHighLevelEventLogAppender appender, string jobId, string cPostfix)
        {
            try
            {
                var rfa = new AveHighLevelEventLogAppender
                {
                    Name = jobId,
                    Layout = appender.Layout
                };

                rfa.ActivateOptions();

                ILoggerRepository repository = LogManager.GetRepository(jobId);
                BasicConfigurator.Configure(repository, rfa);

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }

            return false;
        }

        private static bool CloneSeparativeLogAppenderNode(RollingFileAppender appender, string tenantId, string path)
        {
            try
            {
                var sla = new AveSeparativeLogAppender(path)
                {
                    Layout = appender.Layout
                };
                sla.ActivateOptions();

                ILoggerRepository repository = LogManager.CreateRepository(tenantId);
                BasicConfigurator.Configure(repository, sla);

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }

            return false;
        }

        #endregion -- SetThhreadJobId--

        public override void InitializeInstance()
        {
            GlobalInitialize();
            if (string.IsNullOrEmpty(this.loggerName))
            {
                this.log4NetLog = LogManager.GetLogger(this.loggingType);
            }
            else
            {
                this.log4NetLog = LogManager.GetLogger(this.loggerName);
            }

            //try
            //{
            //    foreach (var appender in this.log4NetLog.Logger.Repository.GetAppenders())
            //    {
            //        if (appender is FileAppender)
            //        {
            //            (appender as FileAppender).Encoding = Encoding.UTF8;
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Trace.TraceError("Ensure file appender utf8 exception. {0}", e.ToString());
            //}
            EnsureLoggingFlushThreadStarted();
        }

        public override void WaitForAllLogsFlush()
        {
            while (true)
            {
                lock (loggingEntries)
                {
                    if (!enableCacheMode) return;
                    if (loggingEntries.Count == 0 && !hasOutput) return;
                }
                Thread.Sleep(300);
            }
        }

        public override void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource, Exception e)
        {
            try
            {
                if (LogicalThreadContext.Properties["ThreadJobId"] != null)
                {
                    log4NetLog = LogManager.GetLogger(LogicalThreadContext.Properties["ThreadJobId"] as string, loggingType);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
                log4NetLog = LogManager.GetLogger(loggingType);
            }
            //Utility.I18N.EventSourcesUtil.CreateEventSources();
            ILoggerRepository repository = log4NetLog.Logger.Repository;
            string loggerName = log4NetLog.Logger.Name;
            if (IsAgentContext() || IsMediaContext())
            {
                if (loggerName != null && loggerName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    loggerName = loggerName.Substring(loggerName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) + 1);
                }
            }
            foreach (object obj in this.loggingType.GetCustomAttributes(false))
            {
                if (obj is AveVersionAttribute)
                {
                    loggerName = loggerName + "," + obj.ToString();
                    break;
                }
            }

            Level log4netLevel = new Level((int)level, Convert.ToString(level));
            string loggingMsg = msg;
            if (e != null)
            {
                loggingMsg = string.Format("{0}\n{1}", msg, e.ToString());
            }
            LoggingEvent loggingEntry = new LoggingEvent(loggingType, repository, loggerName, log4netLevel, loggingMsg, null);
            loggingEntry.Properties["EventID"] = eventId;
            loggingEntry.Properties["EventMessage"] = msg;
            loggingEntry.Properties["TaskCategory"] = taskCategory;
            loggingEntry.Properties["EventSource"] = eventSource;
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "appender is valid")]
        public override void SetDeployId(string deployId)
        {
            try
            {
                if (!String.IsNullOrEmpty(deployId))
                {
                    var doc = new XmlDocument();
                    doc.Load(defaultConfigurationFile);
                    var nodes = doc.SelectNodes("/log4net/appender/layout/header");
                    if (nodes != null)
                    {
                        foreach (var ele in nodes.Cast<XmlElement>())
                        {
                            ele.SetAttribute("value", "Level DateTime Thread Class EventID- Message Deployment ID: " + deployId + "\r\n");
                        }
                    }
                    doc.Save(defaultConfigurationFile);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }
    }

    public class Log4NetGlobalConfig
    {
        public static string ConfigFile;

        public static string LogPath { set { log4net.GlobalContext.Properties["RelatedPath"] = value; } }

        public static string ProcessName { set { log4net.GlobalContext.Properties["ProcessName"] = value; } }

        public static string LogFilePostfix { set { log4net.GlobalContext.Properties["LogFilePostfix"] = value; } }
    };
}