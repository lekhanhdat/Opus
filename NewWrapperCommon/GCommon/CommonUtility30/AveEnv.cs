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




namespace AvePoint.Common
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
    #endregion


    public class AveEnv
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static FileSystemWatcher configurationFileWatcher = null;

        static AveEnv()
        {
            ReInit();
        }

        #region -- public Static Properties --
        public static string LocalSPServerName { get; set; }
        public static string AgentSchema { get; set; }
        public static string AgentName { get; set; }
        public static string AgentAddress { get; set; }
        public static int AgentPort { get; set; }
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
        public static string AgentFarmName { get; set; }
        public static string AgentFarmId { get; set; }
        public static string AgentWcfThumbprint { get; set; }
        public static bool AgentEnableSSL { get; set; }
        public static string AgentSSLThumbprint { get; set; }
        public static string AgentCIID { get; set; }
        public static bool AgentSkipRemoveAgentType { get; set; }
        public static int AgentRegisterRetryInterval { get; set; }
        public static int AgentRegisterMaxRetries { get; set; }
        public static bool AgentRegisterSkipFailed { get; set; }
        public static int AgentCheckingRoleInFarmTimeout { get; set; }
        public static string AgentVersion { get; set; }
        public static string AgentDisplayVersion { get; set; }
        public static OEMProductType AgentProductType { get; set; }
        public static bool IsDocAve { get { return (AgentProductType & OEMProductType.DocAve) == OEMProductType.DocAve; } }
        public static bool IsNetApp { get { return (AgentProductType & OEMProductType.NetApp) == OEMProductType.NetApp; } }
        public static string ManagerSchema { get; set; }
        public static string ManagerAddress { get; set; }
        public static int ManagerPort { get; set; }
        public static byte[] PassphraseHash { get; set; }

        public static int SPVersion { get { return (int)AveSPEnv.SPVersion; } }
        public static int MossOrWss { get { return (int)AveSPEnv.SPMOSSOrWSS; } }
        public static string SharePointDisplayVersion { get { return AveSPEnv.DisplayVersion; } }
        public static string SharePointDLLVersion { get; set; }
        public static bool IsPublishing
        {
            get
            {
                if (AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2007 
                    || AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2010 
                    || AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2013
                    || AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2016
                    || AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2019)
                {
                    if (AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.MOSS)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static bool IsSharePoint2003 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2003; } }
        public static bool IsSharePoint2007 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2007; } }
        public static bool IsSharePoint2010 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2010; } }
        public static bool IsSharePoint2013 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2013; } }
        public static bool IsSharePoint2016 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2016; } }
        public static bool IsSharePoint2019 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2019; } }
        public static bool IsSharePointSE { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePointSE; } }
        public static bool IsSharePoint2013OrAbove { get { return AveSPEnv.SPVersion >= AveSPEnv.AveSPVersionInternal.SharePoint2013; } }
        public static bool IsNonSPInstalled { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.None; } }
        public static bool IsMoss { get { return AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.MOSS; } }
        public static bool IsWss { get { return AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.WSS; } }

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
                //try
                //{
                //    using (AveAppPoolExecuter appPool = new AveAppPoolExecuter())
                //    {
                //        RegistryKey productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\AvePoint\DocAve6");
                //        if (productKey != null && productKey.GetValue("Path") != null)
                //        {
                //            AgentRootFolder = productKey.GetValue("Path").ToString();
                //            logger.Info(@"Got agent root folder from HKEY_LOCAL_MACHINE\SOFTWARE\AvePoint\DocAve6->Path");
                //        }
                //        else
                //        {
                //            productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Network Appliance\SnapManager for SharePoint 8");
                //            if (productKey != null && productKey.GetValue("Path") != null)
                //            {
                //                AgentRootFolder = productKey.GetValue("Path").ToString();
                //                logger.Info(@"Got agent root folder from HKEY_LOCAL_MACHINE\SOFTWARE\Network Appliance\SnapManager for SharePoint 8->Path");
                //            }
                //            else
                //            {
                //                productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\IBM\SnapManager for SharePoint 8");
                //                if (productKey != null && productKey.GetValue("Path") != null)
                //                {
                //                    AgentRootFolder = productKey.GetValue("Path").ToString();
                //                    logger.Info(@"Got agent root folder from HKEY_LOCAL_MACHINE\SOFTWARE\IBM\SnapManager for SharePoint 8->Path");
                //                }
                //            }
                //        }

                //    }
                //}
                //catch (Exception e) { logger.Warn("Read agent root folder exception: {0}", e.ToString()); }
                //2. read DocAve6 Communication Service Executable location
                //if (string.IsNullOrEmpty(AgentRootFolder))
                //{
                //    try
                //    {
                //        using (AveAppPoolExecuter appPool = new AveAppPoolExecuter())
                //        {
                //            var serviceSubKey = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\" + AgentConstants.AgentBinaryName.SERVICE_NAME);
                //            if (serviceSubKey != null)
                //            {
                //                var servicePathObj = serviceSubKey.GetValue("ImagePath");
                //                if (servicePathObj != null)
                //                {
                //                    var servicePath = servicePathObj.ToString().Trim('"');
                //                    int index = servicePath.LastIndexOf('\\');
                //                    if (index > 0)
                //                    {
                //                        servicePath = servicePath.Substring(0, index);
                //                        execuableUnderBinFolder = servicePath.EndsWith("debug", StringComparison.OrdinalIgnoreCase) ? false : true;
                //                        index = servicePath.LastIndexOf('\\');
                //                        if (index > 0)
                //                        {
                //                            AgentRootFolder = servicePath.Substring(0, index);
                //                            logger.Info(@"Got agent root folder from service image path.");
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    catch (Exception e) { logger.Warn("Read agent root folder exception: {0}", e.ToString()); }
                //}
                //3. read AgentCommonService process location
                //if (string.IsNullOrEmpty(AgentRootFolder))
                //{
                //    try
                //    {
                //        using (AveAppPoolExecuter appPool = new AveAppPoolExecuter())
                //        {
                //            int index = AgentConstants.AgentBinaryName.SERVICE_EXE_NAME.LastIndexOf('.');
                //            if (index > 0)
                //            {
                //                string processName = AgentConstants.AgentBinaryName.SERVICE_EXE_NAME.Substring(0, index);
                //                Process[] serviceProcesses = Process.GetProcessesByName(processName);
                //                if (serviceProcesses != null && serviceProcesses.Length > 0)
                //                {
                //                    var servicePath = serviceProcesses[0].MainModule.FileName;
                //                    index = servicePath.LastIndexOf('\\');
                //                    if (index > 0)
                //                    {
                //                        servicePath = servicePath.Substring(0, index);
                //                        execuableUnderBinFolder = servicePath.EndsWith("debug", StringComparison.OrdinalIgnoreCase) ? false : true;
                //                        index = servicePath.LastIndexOf('\\');
                //                        if (index > 0)
                //                        {
                //                            AgentRootFolder = servicePath.Substring(0, index);
                //                            logger.Info(@"Got agent root folder from service process path.");
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    catch (Exception e) { logger.Warn("Read agent root folder exception: {0}", e.ToString()); }
                //}
                //4. finally, reading current process location
                if (string.IsNullOrEmpty(AgentRootFolder))
                {
                    const string binPath = "\\bin\\";
                    const string debugPath = "\\debug\\";
                    const string unitTestPath = "\\out\\";
                    string currentExecuteLocation = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\";
                    if (currentExecuteLocation.EndsWith(binPath, StringComparison.OrdinalIgnoreCase))
                    {
                        AgentRootFolder = currentExecuteLocation.Substring(0, currentExecuteLocation.Length - binPath.Length);
                        logger.Info(@"Got agent root folder from execute bin location.");
                    }
                    else if (currentExecuteLocation.EndsWith(debugPath, StringComparison.OrdinalIgnoreCase))
                    {
                        AgentRootFolder = currentExecuteLocation.Substring(0, currentExecuteLocation.Length - debugPath.Length);
                        execuableUnderBinFolder = false;
                        logger.Info(@"Got agent root folder from execute debug location.");
                    }
                    else if (currentExecuteLocation.EndsWith(unitTestPath, StringComparison.OrdinalIgnoreCase))
                    {
                        AgentRootFolder = currentExecuteLocation.Substring(0, currentExecuteLocation.Length - unitTestPath.Length);
                        execuableUnderBinFolder = false;
                        logger.Info(@"Got agent root folder from unit test output location.");
                    }
                    else
                    {
                        logger.Warn("Executable file doesn't under bin/debug folder. set agent root folder to: {0}", currentExecuteLocation);
                    }
                }
                //logger.Info("Agent root folder path is :{0}", AgentRootFolder);
                if (string.IsNullOrEmpty(AgentBinFolder) && !string.IsNullOrEmpty(AgentRootFolder))
                {
                    AgentBinFolder = execuableUnderBinFolder ? CombinePath(AgentRootFolder, "bin") : CombinePath(AgentRootFolder, "debug");
                    AgentDataFolder = CombinePath(AgentRootFolder, "data");
                    AgentJobFolder = CombinePath(AgentRootFolder, "jobs");
                    AgentTempFolder = CombinePath(AgentRootFolder, "temp");
                    AgentLogFolder = CombinePath(AgentRootFolder, "logs");
                }

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
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
                Configuration envConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
                new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = vcEnvConfigurationFile }
                , System.Configuration.ConfigurationUserLevel.None);

                bool configurationFileNeedToChanged = false;
                ManagerSchema = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "managerSchema", "https", ref configurationFileNeedToChanged);
                ManagerAddress = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "managerAddress", "localhost", ref configurationFileNeedToChanged);
                ManagerPort = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "managerPort", 6001, ref configurationFileNeedToChanged));
                AgentSchema = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentSchema", "net.tcp", ref configurationFileNeedToChanged);
                AgentName = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentName", "localhost", ref configurationFileNeedToChanged);
                LocalSPServerName = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "localSPServerName", AgentName, ref configurationFileNeedToChanged);
                AgentAddress = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentAddress", "localhost", ref configurationFileNeedToChanged);
                AgentPort = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentPort", 10103, ref configurationFileNeedToChanged));
                AgentType = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentType", "0000", ref configurationFileNeedToChanged);
                AgentFarmName = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentFarmName", string.Empty, ref configurationFileNeedToChanged);
                AgentFarmId = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentFarmId", string.Empty, ref configurationFileNeedToChanged);
                AgentEnableSSL = bool.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentEnableSSL", false, ref configurationFileNeedToChanged));
                AgentSSLThumbprint = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentSSLThumbprint", string.Empty, ref configurationFileNeedToChanged);
                AgentLogRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentArchivedLogRetetionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentArchivedLogRetentionDays", 90, ref configurationFileNeedToChanged));
                AgentLogRetentionTriggerSize = long.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionTriggerSize", 2147483648, ref configurationFileNeedToChanged));
                AgentLogRetentionKeepSize = long.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLogRetentionKeepSize", 1879048192, ref configurationFileNeedToChanged));
                AgentJobsRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentJobsRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentServiceUpdataInterval = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentUpdateInterval", 1, ref configurationFileNeedToChanged));
                AgentTempFileRetentionDays = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentTempFileRetentionDays", 7, ref configurationFileNeedToChanged));
                AgentLazyStartProcess = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentLazyStartProcess", "00000", ref configurationFileNeedToChanged);
                AgentCIID = GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentCIID", string.Empty, ref configurationFileNeedToChanged);
                AgentSkipRemoveAgentType = bool.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentSkipRemoveAgentType", false, ref configurationFileNeedToChanged));
                AgentRegisterRetryInterval = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentRegisterRetryInterval", 60, ref configurationFileNeedToChanged));
                AgentRegisterMaxRetries = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentRegisterMaxRetries", 2147483647, ref configurationFileNeedToChanged));
                AgentRegisterSkipFailed = bool.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentRegisterSkipFailed", false, ref configurationFileNeedToChanged));
                AgentCheckingRoleInFarmTimeout = int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentCheckingRoleInFarmTimeout", 5, ref configurationFileNeedToChanged));
                AgentProductType = (OEMProductType)int.Parse(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "agentProductType", 1, ref configurationFileNeedToChanged));
                PassphraseHash = Convert.FromBase64String(GetAttributeFromSetting(envConfiguration.AppSettings.Settings, "passphraseHash", "", ref configurationFileNeedToChanged));

                if (configurationFileNeedToChanged)
                {
                    envConfiguration.Save();
                }

                string serviceVersionConfigurationFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_ServiceVersionConfig);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(serviceVersionConfigurationFile);
                AgentVersion = xmlDoc.SelectSingleNode("configuration/properties/ProductVersion").InnerText;
                AgentDisplayVersion = xmlDoc.SelectSingleNode("configuration/properties/DisplayVersion").InnerText;
            }
            catch (Exception ex)
            {
                logger.Error("Load Agent Environment Failed:{0}", ex.ToString());
            }
        }

        private static void LoadLogConfig()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentBinFolder))
                {
                    string logFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_Log4netConfig);
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

        private static void EnsureFarmName()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentFarmName)) return;

                AgentFarmName = GetFarmName();
                if (!string.IsNullOrEmpty(AgentFarmName))
                {
                    AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentFarmName = true });
                }
            }
            catch (Exception ex)
            {
                logger.Error("Persist configuration failed when ensure farm name. Error: {0}", ex.ToString());
            }
        }

        public static bool ClearFarmNameAndFarmIdInConfiguration()
        {
            bool clearSuccess = false;
            try
            {
                logger.Info("Clear farm name and farm id in configuration,original farm name:{0},farm id:{1}", AgentFarmName, AgentFarmId);
                AgentFarmName = string.Empty;
                AgentFarmId = string.Empty;
                AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentFarmName = true, PersistAgentFarmId = true });
                clearSuccess = true;
            }
            catch (Exception e)
            {
                logger.Error("Clear farm name and farm id failed,error:{0}", e.ToString());
                clearSuccess = false;
            }
            return clearSuccess;
        }

        public static void EnsureFarmId()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentFarmId)) return;

                AgentFarmId = GetFarmId();
                if (!string.IsNullOrEmpty(AgentFarmId))
                {
                    AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentFarmId = true });
                }
            }
            catch (Exception ex)
            {
                logger.Error("Persist configuration failed when ensure farm id. Error Information: {0} ", ex.ToString());
            }
        }

        private static void EnsureSharePointDLLVersion()
        {
            if (!string.IsNullOrEmpty(SharePointDLLVersion)) return;

            SharePointDLLVersion = GetSharePointDLLFileVersion();
        }

        private static void EnsureWcfThumbprint()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentWcfThumbprint)) return;

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
                XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
                XmlNode serviceCertificateNode = xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
                string wcfThumbprint1 = clientCertificateNode.Attributes["findValue"].Value;
                string wcfThumbprint2 = serviceCertificateNode.Attributes["findValue"].Value;
                AgentWcfThumbprint = wcfThumbprint1;
            }
            catch (Exception ex)
            {
                logger.Error("Load WCF thumbprint failed:{0}", ex.ToString());
            }
        }

        private static void PersistWcfThumbprint()
        {
            if (string.IsNullOrEmpty(AgentWcfThumbprint)) return;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
            XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
            XmlNode serviceCertificateNode = xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
            clientCertificateNode.Attributes["findValue"].Value = AgentWcfThumbprint;
            serviceCertificateNode.Attributes["findValue"].Value = AgentWcfThumbprint;
            xDoc.Save(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
        }

        private static void PersistAgentLogLevel(PersistOptions option)
        {
            if (option.PersistAgentLogLevel || option.PersistAgentLogMaxFileSize || option.PersistAgentLogMaxRollBackups)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_Log4netConfig));
                if (option.PersistAgentLogLevel)
                {
                    XmlNode levelNode = xDoc.SelectSingleNode(@"/log4net/root/level");
                    levelNode.Attributes["value"].Value = ((AveLogLevel)agentLogLevel).ToString();
                }
                if (option.PersistAgentLogMaxFileSize)
                {
                    xDoc.GetElementsByTagName("maximumFileSize")[0].Attributes["value"].Value = AgentLogMaxFileSize.ToString() + "MB";
                }
                if (option.PersistAgentLogMaxRollBackups)
                {
                    xDoc.GetElementsByTagName("maxSizeRollBackups")[0].Attributes["value"].Value = AgentLogMaxRollBackups.ToString();
                }
                xDoc.Save(Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_Log4netConfig));
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
                logger.Warn("Create Directory:{0} failed:{1}", path, ex.ToString());
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

        private static string GetSharePointDLLFileVersion()
        {
            string fileVersion = string.Empty;
            try
            {
                logger.Info("Begin to get SharePoint file version.");
                string hiveLocation = string.Empty;
                if (IsSharePoint2019)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2016)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2013)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2010)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2007)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0").GetValue("Location").ToString();
                }
                string spDLL = Path.Combine(Path.Combine(hiveLocation, "ISAPI"), "Microsoft.SharePoint.dll");
                fileVersion = FileVersionInfo.GetVersionInfo(spDLL).FileVersion;
                logger.Info("SharePoint file version: " + fileVersion);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting SharePoint file version. Exception: {0}", ex.ToString());
            }
            if (string.IsNullOrEmpty(fileVersion))
            {
                logger.Info("Cannot get SharePoint file version. it's non-SharePoint box.");
                return string.Empty;
            }
            return fileVersion;
        }

        private static string GetFarmId()
        {
            string farmId = string.Empty;

            try
            {
                logger.Info("Begin to get farm id.");
                if (IsSharePoint2019)
                {
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                }
                else if (IsSharePoint2016)
                {
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                }
                else if (IsSharePoint2013)
                {
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                }
                else if (IsSharePoint2010)
                {
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                }
                else if (IsSharePoint2007)
                {
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                }
                logger.Info("SharePoint Configuration id: {0}", farmId);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting SharePoint configuration id. Exception: {0}", ex.ToString());
            }
            if (string.IsNullOrEmpty(farmId))
            {
                logger.Info("Cannot get SharePoint configuration id. it's non-SharePoint box.");
                return string.Empty;
            }
            return farmId;
        }



        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
        private static string GetFarmName()
        {
            string farmName = string.Empty;
            string dsn = string.Empty;

            try
            {
                logger.Info("Begin to get farm name.");
                try
                {
                    if (IsSharePoint2019)
                    {
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    }
                    else if (IsSharePoint2016)
                    {
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    }
                    else if (IsSharePoint2013)
                    {
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    }
                    else if (IsSharePoint2010)
                    {
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    }
                    else if (IsSharePoint2007)
                    {
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0").OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while getting SharePoint configuration dsn. Exception: {0}", ex.ToString());
                }
                if (string.IsNullOrEmpty(dsn))
                {
                    logger.Info("Cannot get SharePoint configuration dsn. it's non-SharePoint box.");
                    return string.Empty;
                }

                logger.Info("SharePoint Configuration DSN: {0}", dsn);
                string[] contents = dsn.Split(';');
                string dbName = string.Empty;
                string instance = string.Empty;
                foreach (string temp in contents)
                {
                    if (temp.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                    {
                        instance = temp.Substring(12).ToUpperInvariant();
                        continue;
                    }
                    if (temp.StartsWith("Initial Catalog", StringComparison.OrdinalIgnoreCase))
                    {
                        dbName = temp.Substring(16);
                        continue;
                    }
                }
                farmName = "Farm(" + instance.ToUpper() + ":" + dbName.ToUpper() + ")";
                string[] specialChars = new string[] { "/", "*", "?", "\"", "<", ">", "|" };
                foreach (string s in specialChars)
                {
                    farmName = farmName.Replace(s, "");
                }
                logger.Info("Got Farm Name: {0}", farmName);
            }
            catch (Exception ex)
            {
                logger.Error("Get Farm Name Failed:{0}", ex.ToString());
            }
            return farmName;
        }


        public static void PersistConfiguration(PersistOptions options)
        {
            logger.Info("persist configuration: " + options.ToString());

            string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
            Configuration envConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(
            new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = vcEnvConfigurationFile }
            , System.Configuration.ConfigurationUserLevel.None);
            bool shouldPersist = false;
            if (options.PersistAgentFarmName)
            {
                envConfiguration.AppSettings.Settings.Remove("agentFarmName");
                envConfiguration.AppSettings.Settings.Add("agentFarmName", AgentFarmName);
                shouldPersist = true;
            }
            if (options.PersistAgentFarmId)
            {
                envConfiguration.AppSettings.Settings.Remove("agentFarmId");
                envConfiguration.AppSettings.Settings.Add("agentFarmId", AgentFarmId);
                shouldPersist = true;
            }
            if (options.PersistAgentName)
            {
                envConfiguration.AppSettings.Settings.Remove("agentName");
                envConfiguration.AppSettings.Settings.Add("agentName", AgentName);
                shouldPersist = true;
            }
            if (options.PersistAgentAddress)
            {
                envConfiguration.AppSettings.Settings.Remove("agentAddress");
                envConfiguration.AppSettings.Settings.Add("agentAddress", AgentAddress);
                shouldPersist = true;
            }
            if (options.PersistAgentPort)
            {
                envConfiguration.AppSettings.Settings.Remove("agentPort");
                envConfiguration.AppSettings.Settings.Add("agentPort", AgentPort.ToString());
                shouldPersist = true;
            }
            if (options.PersistAgentType)
            {
                envConfiguration.AppSettings.Settings.Remove("agentType");
                envConfiguration.AppSettings.Settings.Add("agentType", AgentType);
                shouldPersist = true;
            }
            if (options.PersistAgentCIID)
            {
                envConfiguration.AppSettings.Settings.Remove("agentCIID");
                envConfiguration.AppSettings.Settings.Add("agentCIID", AgentCIID);
                shouldPersist = true;
            }
            if (options.PersistAgentLazyStartProcess)
            {
                envConfiguration.AppSettings.Settings.Remove("agentLazyStartProcess");
                envConfiguration.AppSettings.Settings.Add("agentLazyStartProcess", AgentLazyStartProcess);
                shouldPersist = true;
            }
            if (options.PersistAgentProductType)
            {
                envConfiguration.AppSettings.Settings.Remove("agentProductType");
                envConfiguration.AppSettings.Settings.Add("agentProductType", ((int)AgentProductType).ToString());
                shouldPersist = true;
            }
            if (options.PersistManagerAddress)
            {
                envConfiguration.AppSettings.Settings.Remove("managerAddress");
                envConfiguration.AppSettings.Settings.Add("managerAddress", ManagerAddress);
                shouldPersist = true;
            }
            if (options.PersistManagerPort)
            {
                envConfiguration.AppSettings.Settings.Remove("managerPort");
                envConfiguration.AppSettings.Settings.Add("managerPort", ManagerPort.ToString());
                shouldPersist = true;
            }
            if (options.PersistPassphraseHash)
            {
                envConfiguration.AppSettings.Settings.Remove("passphraseHash");
                envConfiguration.AppSettings.Settings.Add("passphraseHash", Convert.ToBase64String(PassphraseHash));
                shouldPersist = true;
            }
            if (options.PersistAgentSslThumbprint)
            {
                envConfiguration.AppSettings.Settings.Remove("agentSSLThumbprint");
                envConfiguration.AppSettings.Settings.Add("agentSSLThumbprint", AgentSSLThumbprint);
                shouldPersist = true;
            }
            if(options.PersistRegisterMaxRetries)
            {
                envConfiguration.AppSettings.Settings.Remove("agentRegisterMaxRetries");
                envConfiguration.AppSettings.Settings.Add("agentRegisterMaxRetries", AgentRegisterMaxRetries.ToString());
                shouldPersist = true;
            }
            if (shouldPersist)
            {
                envConfiguration.Save();
            }

            if (options.PersistAgentWcfThumbprint)
            {
                PersistWcfThumbprint();
            }
            PersistAgentLogLevel(options);
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
            EnsureFarmName(); //load farm name from registry
            EnsureFarmId(); //load farm id(configDB id) from registry
            //EnsureSharePointDLLVersion(); //load SharePoint DLL file version from HIVE
            EnsureWcfThumbprint(); //load WCF certificate thumbprint from AgentCommonWCFBehaviors.config

            try
            {
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
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

                AveLogger.SetCustomizedLogPostfix("V:" + AgentVersion);
            }
            catch (Exception ex)
            {
                logger.Error("Initializing file {0} watcher failed. Error: {1}.", AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig, ex.ToString());
            }
        }
        #endregion

        private static void sConfigurationFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                configurationFileWatcher.WaitForChanged(WatcherChangeTypes.Changed, 2000);
                string vcEnvConfigurationFile = Path.Combine(AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_VCEnvConfig);
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
            public bool PersistAgentFarmName { get; set; }
            public bool PersistAgentFarmId { get; set; }
            public bool PersistAgentName { get; set; }
            public bool PersistAgentAddress { get; set; }
            public bool PersistAgentPort { get; set; }
            public bool PersistAgentLogLevel { get; set; }
            public bool PersistAgentLogMaxFileSize { get; set; }
            public bool PersistAgentLogMaxRollBackups { get; set; }
            public bool PersistAgentType { get; set; }
            public bool PersistAgentWcfThumbprint { get; set; }
            public bool PersistAgentSslThumbprint { get; set; }
            public bool PersistAgentCIID { get; set; }
            public bool PersistAgentLazyStartProcess { get; set; }
            public bool PersistAgentProductType { get; set; }
            public bool PersistManagerAddress { get; set; }
            public bool PersistManagerPort { get; set; }
            public bool PersistPassphraseHash { get; set; }
            public bool PersistRegisterMaxRetries { get; set; }

            public string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("PersistAgentFarmName:" + PersistAgentFarmName);
                sb.Append(" PersistAgentFarmId:" + PersistAgentFarmId);
                sb.Append(" PersistAgentName:" + PersistAgentName);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentPort:" + PersistAgentPort);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentLogLevel:" + PersistAgentLogLevel);
                sb.Append(" PersistAgentType:" + PersistAgentType);
                sb.Append(" PersistAgentWcfThumbprint:" + PersistAgentWcfThumbprint);
                sb.Append(" PersistAgentSslThumbprint:" + PersistAgentSslThumbprint);
                sb.Append(" PersistAgentCIID:" + PersistAgentCIID);
                sb.Append(" PersistAgentProductType:" + PersistAgentProductType);
                sb.Append(" PersistManagerAddress:" + PersistManagerAddress);
                sb.Append(" PersistManagerPort:" + PersistManagerPort);
                sb.Append(" PersistPassphraseHash:" + PersistPassphraseHash);
                sb.Append(" PersistRegisterMaxRetries:" + PersistRegisterMaxRetries);

                return sb.ToString();
            }
        }

    }

    internal class AveSPEnv
    {/// <summary>
        /// 这个枚举只表示SharePoint版本，但是不区分MOSS Or WSS
        /// </summary>
        internal enum AveSPVersionInternal : int
        {
            None = 0,
            SharePoint2003 = 1,
            SharePoint2007 = 2,
            SharePoint2010 = 4,
            SharePoint2013 = 8,
            SharePoint2016 = 16,
            SharePoint2019 = 32,
            SharePointSE = 64,
        }

        /// <summary>
        /// 这个区分是WSS还是MOSS
        /// </summary>
        internal enum AveSPMOSSOrWSSInternal : int
        {
            None = 0,
            WSS = 1,
            MOSS = 2,
        }

        static AveLogger logger = AveLogger.GetInstance(typeof(AveSPEnv));

        static AveSPVersionInternal spVersion;
        static AveSPMOSSOrWSSInternal spMoss;

        internal static AveSPVersionInternal SPVersion { get { return spVersion; } }
        internal static AveSPMOSSOrWSSInternal SPMOSSOrWSS { get { return spMoss; } }
        internal static string DisplayVersion { get; set; }

        static AveSPEnv()
        {
            try
            {
                GetMossOrWssVersion();
            }
            catch (Exception ex)
            {
                logger.Error("Get SPVersion Failed:{0}", ex.ToString());
            }
        }

        static void GetMossOrWssVersion()
        {
            String wss30 = "Microsoft Windows SharePoint Services 3.0";
            String wss30ID = "{90120000-1014-0000-0000-0000000FF1CE}";
            String wss30IDx64 = "{90120000-1014-0000-1000-0000000FF1CE}";
            String mossDisplay = "Microsoft Office SharePoint Server 2007";
            String moss2007ID = "{90120000-110D-0000-0000-0000000FF1CE}";
            String moss2007IDx64 = "{90120000-110D-0000-1000-0000000FF1CE}";
            String sps2003 = "Microsoft Office SharePoint Portal Server 2003";
            String sps2003ID = "{610F491D-BE5F-4ED1-A0F7-759D40C7622E}";

            String wss20 = "Microsoft Windows SharePoint Services 2.0";
            String wss20ID = "{91140409-7000-11D3-8CFE-0150048383C9}";
            String moss2010 = "Microsoft SharePoint Server 2010";
            String moss2010ID = "{20140000-110D-0000-1000-0000000FF1CE}";
            String moss2010IDNew = "{90140000-110D-0000-1000-0000000FF1CE}";
            String wss2010 = "Microsoft SharePoint Foundation 2010";
            String wss2010ID = "{90140000-1110-0000-1000-0000000FF1CE}";
            String wss2010New = "Microsoft SharePoint Foundation 2010 Core";
            String wss2010IDNew = "{90140000-1014-0000-1000-0000000FF1CE}";

            String wss2013 = "Microsoft SharePoint Foundation 2013 Core";
            String wss2013ID = "{20150000-1014-0000-1000-0000000FF1CE}";
            String wss2013IDNew = "{90150000-1014-0000-1000-0000000FF1CE}";
            String moss2013 = "Microsoft SharePoint Server 2013";
            String moss2013ID = "{20150000-110D-0000-1000-0000000FF1CE}";
            String moss2013IDNew = "{90150000-110D-0000-1000-0000000FF1CE}";

            String wss2016 = "Microsoft SharePoint Foundation 2016 Core";
            String wss2016ID = "{20160000-1014-0000-1000-0000000FF1CE}";
            String wss2016IDNew = "{90160000-1014-0000-1000-0000000FF1CE}";
            String moss2016 = "Microsoft SharePoint Server 2016";
            String moss2016ID = "{20160000-110D-0000-1000-0000000FF1CE}";
            String moss2016IDNew = "{90160000-110D-0000-1000-0000000FF1CE}";

            String moss2019 = "Microsoft SharePoint Server 2019";
            String moss2019Id = "{10160000-110D-0000-1000-0000000FF1CE}";
            String moss2019IdNew = "{90160000-1167-0000-1000-0000000FF1CE}";

            String wssSE = "Microsoft SharePoint Server Subscription Edition Core";
            //String wssSEId = "{}";
            String wssSEIdNew = "{90160000-1010-0000-1000-0000000FF1CE}";
            String mossSE = "Microsoft SharePoint Server Subscription Edition";
            //String mossSEId = "{}";
            String mossSEIdNew = "{90160000-1169-0000-1000-0000000FF1CE}";

            if (KeyNameExists(mossSEIdNew, mossSE))
            {
                spVersion = AveSPVersionInternal.SharePointSE;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(mossSEIdNew);
            }
            else if (KeyNameExists(wssSEIdNew, wssSE))
            {
                spVersion = AveSPVersionInternal.SharePointSE;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wssSEIdNew);
            }
            else if (KeyNameExists(moss2019IdNew, moss2019))
            {
                spVersion = AveSPVersionInternal.SharePoint2019;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2019IdNew);
            }
            else if (KeyNameExists(moss2019Id, moss2019))
            {
                spVersion = AveSPVersionInternal.SharePoint2019;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2019Id);
            }
            else if (KeyNameExists(moss2016ID, moss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2016ID);
            }
            else if (KeyNameExists(moss2016IDNew, moss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2016IDNew);
            }
            else if (KeyNameExists(wss2016ID, wss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2016ID);
            }
            else if (KeyNameExists(wss2016IDNew, wss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2016IDNew);
            }
            else if (KeyNameExists(moss2013ID, moss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2013ID);
            }
            else if (KeyNameExists(moss2013IDNew, moss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2013IDNew);
            }
            else if (KeyNameExists(wss2013ID, wss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2013ID);
            }
            else if (KeyNameExists(wss2013IDNew, wss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2013IDNew);
            }
            else if (KeyNameExists(moss2010ID, moss2010) || KeyNameExists(moss2010IDNew, moss2010))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2010ID, moss2010))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2010ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2010IDNew);
                }
            }
            else if (KeyNameExists(wss2010ID, wss2010))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2010ID);
            }
            else if (KeyNameExists(wss2010IDNew, wss2010New))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2010New);
            }
            else if (KeyNameExists(moss2007ID, mossDisplay) || KeyNameExists(moss2007IDx64, mossDisplay))
            {
                spVersion = AveSPVersionInternal.SharePoint2007;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2007ID, mossDisplay))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2007ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2007IDx64);
                }
            }
            else if (KeyNameExists(wss30ID, wss30) || KeyNameExists(wss30IDx64, wss30))
            {
                spVersion = AveSPVersionInternal.SharePoint2007;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                if (KeyNameExists(wss30ID, wss30))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(wss30ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(wss30IDx64);
                }
            }
            else if (KeyNameExists(sps2003ID, sps2003))
            {
                spVersion = AveSPVersionInternal.SharePoint2003;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
            }
            else if (KeyNameExists(wss20ID, wss20))
            {
                spVersion = AveSPVersionInternal.SharePoint2003;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
            }
            else
            {
                spVersion = AveSPVersionInternal.None;
                spMoss = AveSPMOSSOrWSSInternal.None;
            }

            //temporary method
            //if (spVersion == AveSPVersionInternal.SharePoint2016)
            //{
            //    var sysRoot = Path.GetPathRoot(Environment.SystemDirectory);
            //    if (File.Exists(Path.Combine(sysRoot, "IAmSP2019.debug")))
            //    {
            //        spVersion = AveSPVersionInternal.SharePoint2019;
            //    }
            //}
            //end
        }

        static string GetDisplayVersionUnderKey(string winKeyPath)
        {
            string win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string win32KeyPath = win32UninstallKeyPath + winKeyPath;
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    object displayVersionValue = rk.GetValue("DisplayVersion");
                    if (displayVersionValue != null)
                    {
                        return displayVersionValue.ToString();
                    }
                }
                else
                {
                    string win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    string win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        object displayVersionValue = rk.GetValue("DisplayVersion");
                        if (displayVersionValue != null)
                        {
                            return displayVersionValue.ToString();
                        }
                    }
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
            return string.Empty;
        }

        static bool KeyNameExists(string winKeyPath, string displayName)
        {
            string win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string win32KeyPath = win32UninstallKeyPath + winKeyPath;
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    object displayNameValue = rk.GetValue("DisplayName");
                    if (displayNameValue != null && displayNameValue.ToString().StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    string win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    string win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        object displayNameValue = rk.GetValue("DisplayName");
                        if (displayNameValue != null && displayNameValue.ToString().StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
            return false;
        }
    }

    public enum OEMProductType
    {
        DocAve = 1,
        NetApp = 2,
        NetAppToIBM = 6,
    }

}