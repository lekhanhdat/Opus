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
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace AutoInstallationCommon.Utility
{
    #region using directives

    #endregion

    /// <summary>
    ///     日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    internal class AveLoggerImp : IAveLoggerImp
    {
        private readonly ILog log4NetLog;
        private readonly Type loggingType;

        static AveLoggerImp()
        {
            SetDefaultConfigFile();
        }

        public AveLoggerImp(Type type)
        {
            loggingType = type;
            log4NetLog = LogManager.GetLogger(type);
        }

        public AveLogLevel CurrentLogLevel => (AveLogLevel) ((Hierarchy) LogManager.GetRepository()).Root.Level.Value;
        public bool IsErrorEnabled => log4NetLog.IsErrorEnabled;
        public bool IsWarnEnabled => log4NetLog.IsWarnEnabled;
        public bool IsInfoEnabled => log4NetLog.IsInfoEnabled;
        public bool IsDebugEnabled => log4NetLog.IsDebugEnabled;

        public void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource)
        {
            var repository = log4NetLog.Logger.Repository;
            var loggerName = log4NetLog.Logger.Name;
            var aveClassVersion = GetClassAveVersion(loggingType);
            if (!string.IsNullOrEmpty(aveClassVersion)) loggerName = loggerName + "," + aveClassVersion;
            var log4netLevel = new Level((int) level, level.ToString());
            var loggingEntry = new LoggingEvent(loggingType, repository, loggerName, log4netLevel, msg, null);
            loggingEntry.Properties["EventID"] = eventId;
            loggingEntry.Properties["TaskCategory"] = taskCategory;
            loggingEntry.Properties["EventSource"] = eventSource;
            log4NetLog.Logger.Log(loggingEntry);
        }

        private string GetClassAveVersion(Type type)
        {
            var attrs = type.GetCustomAttributes(false);
            foreach (var obj in attrs)
                if (obj is AveVersionAttribute)
                    return obj.ToString();
            return string.Empty;
        }

        private static void SetDefaultConfigFile()
        {
            var log4netConfigurationFile = string.Empty;
            var deploymentConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"Config\AvePoint.CommonDeploymentLog4net.config");

            if (File.Exists(deploymentConfig)) log4netConfigurationFile = deploymentConfig;

            #region set log file pattern property

            GlobalContext.Properties["LogFilePostfix"] = string.Empty;
            var process = Process.GetCurrentProcess();
            if (string.Compare(process.ProcessName, "AgentService", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "AgentCommonRestartService",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "AgentCommonPostInstall", StringComparison.OrdinalIgnoreCase) ==
                0
                || string.Compare(process.ProcessName, "AgentCommonBrowser", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "SP2010ReplicatorListener",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "SP2010ReplicatorAnalyzer",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "SP2010StorageOptimizationService",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "SP2010ConnectorProcessor",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "AgentCommonPlatformRecoveryInstaller",
                    StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "SP2010StorageProcessor", StringComparison.OrdinalIgnoreCase) ==
                0
                || string.Compare(process.ProcessName, "SP2010VDBItemBrowser", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(process.ProcessName, "AgentCommonVDBFileServer",
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                GlobalContext.Properties["LogFilePostfix"] = string.Empty;
            }
            else
            {
                var format = new StringBuilder().Append("MM").Append('d').Append('d').Append("HH").Append('m')
                    .Append('m').Append('s').Append('s').ToString();
                var timestamp = DateTime.Now.ToString(format);
                var pid = Process.GetCurrentProcess().Id.ToString();
                GlobalContext.Properties["LogFilePostfix"] = "_" + timestamp + "_" + pid;
            }

            #endregion

            if (!string.IsNullOrEmpty(log4netConfigurationFile))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netConfigurationFile));
            }
            else
            {
                if (ConfigurationManager.GetSection("log4net") != null) XmlConfigurator.Configure();
            }
        }
    }

    public class AveVersionAttribute : DescriptionAttribute
    {
        private readonly string version;

        public AveVersionAttribute(string ver)
        {
            try
            {
                version = ver.Substring(11, ver.Length - 13);
            }
            catch
            {
                version = "0.0";
            }
        }

        public override string ToString()
        {
            return version;
        }
    }
}