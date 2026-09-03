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




namespace AvePoint.Hybrid.AgentService
{
    #region using directives
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon;
    using Microsoft.Win32;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Hybrid.Utility;
    #endregion


    public class AveEnv
    {
        static AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static FileSystemWatcher configurationFileWatcher = null;

        static AveEnv()
        {
            ReInit();
        }

        #region -- public Static Properties --
    
        private static int agentLogLevel;
        public static int AgentLogLevel { get { return (int)logger.CurrentLogLevel; } set { agentLogLevel = value; } }
        public static int AgentLogMaxFileSize { get; set; }
        public static int AgentLogMaxRollBackups { get; set; }
        public static string AgentType { get; set; }
        public static string AgentRootFolder { get; set; }
        public static string AgentBinFolder { get; set; }
        public static string AgentJobFolder { get; set; }
        public static string AgentDataFolder { get; set; }
        public static string AgentLogFolder { get; set; }
        public static int AgentLogRetentionDays { get; set; }
        public static long AgentLogRetentionTriggerSize { get; set; }
        public static long AgentLogRetentionKeepSize { get; set; }
        public static int AgentArchivedLogRetetionDays { get; set; }
        public static int AgentJobsRetentionDays { get; set; }
        public static int AgentServiceUpdataInterval { get; set; }
        public static int AgentTempFileRetentionDays { get; set; }
        public static string AgentLazyStartProcess { get; set; }
        public static string AgentTempFolder { get; set; }
        public static string AgentVersion { get; set; }
        public static string AgentDisplayVersion { get; set; }
        public static int AgentLogCollectInterval { get; set; }

        #endregion

        #region -- Static Methods --

        private static void InitEnv()
        {
            try
            {
                try
                {
                    string hostName = System.Net.Dns.GetHostName();
                    string machineName = Environment.MachineName;
                    if (string.Compare(hostName, machineName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        logger.Warn("Hostname: {0}", hostName);
                        logger.Warn("MachineName:{0}", machineName);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Check host name and machine name exception: {0}", e.ToString());
                }

                bool execuableUnderBinFolder = true;
                //1. read agent root folder from registry
                try
                {
                    string currentExecuteLocation = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\";

                    AgentRootFolder = currentExecuteLocation;
                    AgentBinFolder = currentExecuteLocation;
                    //AgentDataFolder = CombinePath(AgentRootFolder, "data");
                    //AgentJobFolder = CombinePath(AgentRootFolder, "jobs");
                    //AgentTempFolder = CombinePath(AgentRootFolder, "temp");
                    string tempPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                    tempPath = tempPath.Substring(0, tempPath.LastIndexOf('\\'));
                    AgentLogFolder = CombinePath(tempPath, "logs");

                    logger.Info(@"Agent AgentRootFolder folder : " + AgentRootFolder);
                    logger.Info(@"Agent AgentBinFolder folder : " + AgentBinFolder);
                    //logger.Info(@"Agent AgentDataFolder folder : " + AgentDataFolder);
                    //logger.Info(@"Agent AgentJobFolder folder : " + AgentJobFolder);
                    //logger.Info(@"Agent AgentTempFolder folder : " + AgentTempFolder);
                    logger.Info(@"Agent AgentLogFolder folder : " + AgentLogFolder);
                    

                }
                catch (Exception e) { logger.Warn("Read agent root folder exception: {0}", e.ToString()); }

                logger.Debug("Running on CLR version: [{0}].", Environment.Version.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("Init AveEnv failed:{0}", ex.ToString());
            }
        }

        private static void LoadEnv()
        {
            try
            {
                if (string.IsNullOrEmpty(AgentBinFolder))
                {
                    return;
                }
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder + @"\config", Constants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
                Configuration envConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
                new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = vcEnvConfigurationFile }
                , System.Configuration.ConfigurationUserLevel.None);

                bool configurationFileNeedToChanged = false;
                AgentLogRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentArchivedLogRetetionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentArchivedLogRetentionDays", 90, ref configurationFileNeedToChanged));
                AgentLogRetentionTriggerSize = long.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionTriggerSize", 2147483648, ref configurationFileNeedToChanged));
                AgentLogRetentionKeepSize = long.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionKeepSize", 1879048192, ref configurationFileNeedToChanged));
                AgentJobsRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentJobsRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentServiceUpdataInterval = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentUpdateInterval", 1, ref configurationFileNeedToChanged));
                AgentTempFileRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentTempFileRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentLazyStartProcess = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLazyStartProcess", "00000", ref configurationFileNeedToChanged);
                AgentLogCollectInterval = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogCollectInterval", 720, ref configurationFileNeedToChanged));

                if (configurationFileNeedToChanged)
                {
                    envConfiguration.Save();
                }

                string serviceVersionConfigurationFile = Path.Combine(AgentRootFolder + @"\config", Constants.AgentConfigurationFileName.AgentConfigFile_ServiceVersionConfig);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(serviceVersionConfigurationFile);
                var versionInRegistry = GetVersionFromRegistry();
                AgentVersion = versionInRegistry??xmlDoc.SelectSingleNode("configuration/properties/ProductVersion").InnerText;
                AgentDisplayVersion = versionInRegistry??xmlDoc.SelectSingleNode("configuration/properties/DisplayVersion").InnerText;
            }
            catch (Exception ex)
            {
                logger.Error("Load Agent Environment Failed:{0}", ex.ToString());
            }
        }

        private static string GetVersionFromRegistry()
        {
            foreach(var packageId in Constants.PackageIds)
            {
                var uninstall = Registry.LocalMachine.OpenSubKey($@"{Constants.RegistryUninstall}\{packageId}");
                if (uninstall != null)
                {
                    return uninstall.GetValue(Constants.RegistryDisplayVersion)?.ToString();
                }
            }
            return null;
            //var uninstall = Registry.LocalMachine.OpenSubKey($@"{Constants.RegistryUninstall}\{Constants.PackageId}");
            //if (uninstall == null)
            //{
            //    uninstall = Registry.LocalMachine.OpenSubKey($@"{Constants.RegistryUninstall}\{Constants.OldPackageId}");
            //}
            //return uninstall?.GetValue(Constants.RegistryDisplayVersion)?.ToString();
        }

        private static void LoadLogConfig()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentBinFolder))
                {
                    string logFile = Path.Combine(AgentBinFolder, Constants.AgentConfigurationFileName.AgentConfigFile_Log4netConfig);
                    if (File.Exists(logFile))
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.Load(logFile);

                        string fileSize = xDoc.GetElementsByTagName("maximumFileSize")[0].Attributes["value"].Value;
                        string rollBackups = xDoc.GetElementsByTagName("maxSizeRollBackups")[0].Attributes["value"].Value;
                        fileSize = fileSize.Substring(0, fileSize.Length - 2);
                        AgentLogMaxFileSize = int.Parse(fileSize);
                        AgentLogMaxRollBackups = int.Parse(rollBackups);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Load log configuration  Failed:{0}", ex.ToString());
            }
        }

        private static string CombinePath(string parentFolder, string currentFolderName)
        {
            var path = Path.Combine(parentFolder, currentFolderName);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Create Directory failed:{0}", ex.ToString());
            }
            return path;
        }

        private static string GetAttributeFromSetting(KeyValueConfigurationCollection settings, string name, object defaultValue, ref bool configurationFileChanged)
        {
            var keyValue = settings[name];
            if (keyValue != null)
            {
                return keyValue.Value;
            }
            else
            {
                settings.Add(name, defaultValue.ToString());
                configurationFileChanged = true;
            }

            return defaultValue.ToString();
        }


        public static void PersistConfiguration(PersistOptions options)
        {
            logger.Info("persist configuration: " + options.ToString());

            string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, Constants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
            Configuration envConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
            new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = vcEnvConfigurationFile }
            , System.Configuration.ConfigurationUserLevel.None);
            bool shouldPersist = false;

            if (options.PersistAgentLazyStartProcess)
            {
                envConfiguration.AppSettings.Settings.Remove("agentLazyStartProcess");
                envConfiguration.AppSettings.Settings.Add("agentLazyStartProcess", AgentLazyStartProcess);
                shouldPersist = true;
            }
            if (shouldPersist)
            {
                envConfiguration.Save();
            }

        }


        public static void ReInit()
        {
#if DEBUG
            while (File.Exists("C:\\debugAveEnv"))
            {
                Thread.Sleep(2000);
            }
#endif
            InitEnv();  //init all dir path
            LoadEnv(); //load configuration from AgentCommonVCEnv.config
            LoadLogConfig(); //load log configuration from AgentLog4net.config

            try
            {
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, Constants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
                FileInfo configurationInfo = new FileInfo(vcEnvConfigurationFile);
                if (configurationFileWatcher != null)
                {
                    configurationFileWatcher.EnableRaisingEvents = false;
                }
                else
                {
                    configurationFileWatcher = new FileSystemWatcher();
                    configurationFileWatcher.Changed += new FileSystemEventHandler(sConfigurationFileWatcher_Changed);
                    configurationFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                }
                configurationFileWatcher.Path = configurationInfo.DirectoryName;
                configurationFileWatcher.Filter = configurationInfo.Name;
                configurationFileWatcher.EnableRaisingEvents = true;

                AvePoint.GCommon.AveLogger.SetCustomizedLogPostfix("V:" + AgentVersion);
            }
            catch (Exception ex)
            {
                logger.Error("Initializing file {0} watcher failed. Error: {1}.", Constants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig, ex.ToString());
            }
        }
        #endregion

        private static void sConfigurationFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                configurationFileWatcher.WaitForChanged(WatcherChangeTypes.Changed, 2000);
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, Constants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
                if (File.Exists(vcEnvConfigurationFile))
                {
                    LoadEnv();
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.ToString());
            }
        }


        public class PersistOptions
        {
          
            public bool PersistAgentName { get; set; }
            public bool PersistAgentAddress { get; set; }
        
            public bool PersistAgentLazyStartProcess { get; set; }
            public bool PersistAgentProductType { get; set; }
            public bool PersistManagerAddress { get; set; }
            public bool PersistManagerPort { get; set; }
            public bool PersistPassphraseHash { get; set; }
            public bool PersistRegisterMaxRetries { get; set; }

            public string ToString()
            {
                StringBuilder sb = new StringBuilder();
          
                sb.Append(" PersistAgentName:" + PersistAgentName);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentProductType:" + PersistAgentProductType);
                sb.Append(" PersistManagerAddress:" + PersistManagerAddress);
                sb.Append(" PersistManagerPort:" + PersistManagerPort);
                sb.Append(" PersistPassphraseHash:" + PersistPassphraseHash);
                sb.Append(" PersistRegisterMaxRetries:" + PersistRegisterMaxRetries);

                return sb.ToString();
            }
        }

    }

}