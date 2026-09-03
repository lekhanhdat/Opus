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
using System.ServiceModel.Configuration;
using System.Xml;
using System.IO;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using System.Configuration;
using AvePoint.GCommon.Security.AccessControl;

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// DataTransferSection
    /// </summary>
    [Serializable]
    public class DataTransferSection : ConfigurationSection
    {
        /// <summary>
        /// DataTransfer Config
        /// </summary>
        [ConfigurationProperty("data")]
        public DataConfigElement DataConfig
        {
            get { return (DataConfigElement)this["data"]; }
        }

        /// <summary>
        /// MQ Config
        /// </summary>
        [ConfigurationProperty("mq")]
        public MqConfigElement MqConfig
        {
            get { return (MqConfigElement)this["mq"]; }
        }

        /// <summary>
        /// default data uri schema
        /// </summary>
        [ConfigurationProperty("defaultDataUriSchema", DefaultValue = "https")]
        public string DefaultDataUriSchema
        {
            get { return (string)this["defaultDataUriSchema"]; }
            set { this["defaultDataUriSchema"] = value; }
        }

        /// <summary>
        /// mq uri schema
        /// </summary>
        [ConfigurationProperty("mqUriSchema", DefaultValue = "net.tcp")]
        public string MqUriSchema
        {
            get { return (string)this["mqUriSchema"]; }
            set { this["mqUriSchema"] = value; }
        }

        /// <summary>
        /// relay service url schema
        /// </summary>
        [ConfigurationProperty("relayServiceUriSchema", DefaultValue = "net.tcp")]
        public string RelayServiceUriSchema
        {
            get { return (string)this["relayServiceUriSchema"]; }
            set { this["relayServiceUriSchema"] = value; }
        }

        /// <summary>
        /// file transfer service url schema
        /// </summary>
        [ConfigurationProperty("fileTransferServiceUriSchema", DefaultValue = "net.tcp")]
        public string FileTransferServiceUriSchema
        {
            get { return (string)this["fileTransferServiceUriSchema"]; }
            set { this["fileTransferServiceUriSchema"] = value; }
        }

        /// <summary>
        /// stream mode service url schema
        /// </summary>
        [ConfigurationProperty("streamModeServiceUriSchema", DefaultValue = "https")]
        public string StreamModeServiceUriSchema
        {
            get { return (string)this["streamModeServiceUriSchema"]; }
            set { this["streamModeServiceUriSchema"] = value; }
        }

        /// <summary>
        /// 是否使用SSL
        /// </summary>
        [ConfigurationProperty("enableSsl", DefaultValue = true)]
        public bool EnableSsl
        {
            get { return (bool)this["enableSsl"]; }
            set { this["enableSsl"] = value; }
        }

        /// <summary>
        /// 是否使用Stream Mode
        /// </summary>
        [ConfigurationProperty("enableStreamMode", DefaultValue = true)]
        public bool EnableStreamMode
        {
            get { return (bool)this["enableStreamMode"]; }
            set { this["enableStreamMode"] = value; }
        }

        [ConfigurationProperty("httpPort", DefaultValue = 14008)]
        public int HttpModePort
        {
            get { return (int)this["httpPort"]; }
            set { this["httpPort"] = value; }
        }

        [ConfigurationProperty("httpCacheDataNumber", DefaultValue = 200)]
        public int HttpCacheDataNumber
        {
            get { return (int)this["httpCacheDataNumber"]; }
            set { this["httpCacheDataNumber"] = value; }
        }
    }

    /// <summary>
    /// DataTransfer Configuration Element
    /// </summary>
    public class DataConfigElement : ConfigurationElement
    {
        /// <summary>
        /// 单位是minute
        /// </summary>
        [ConfigurationProperty("minReconnectTimeout", DefaultValue=5)]
        public int MinReconnectTimeout
        {
            get { return (int)this["minReconnectTimeout"]; }
            set { this["minReconnectTimeout"] = value; }
        }
        /// <summary>
        /// 单位是minute
        /// </summary>
        [ConfigurationProperty("defaultReconnectTimeout", DefaultValue = 30)]
        public int DefaultReconnectTimeout
        {
            get { return (int)this["defaultReconnectTimeout"]; }
            set { this["defaultReconnectTimeout"] = value; }
        }
        /// <summary>
        /// Send buffer size used in DataTransfer
        /// </summary>
        [ConfigurationProperty("sendBufferSize", DefaultValue = 64 * 1024)]
        public int SendBufferSize
        {
            get { return (int)this["sendBufferSize"]; }
            set { this["sendBufferSize"] = value; }
        }

        /// <summary>
        /// 用于Service存储的Buffer大小
        /// </summary>
        [ConfigurationProperty("cycleStreamSize", DefaultValue = 5 * 1024 * 1024)]
        public int CycleStreamSize
        {
            get { return (int)this["cycleStreamSize"]; }
            set { this["cycleStreamSize"] = value; }
        }

        /// <summary>
        /// 用于中间处理加密和压缩的缓存使用。
        /// </summary>
        [ConfigurationProperty("dataBlockProcessorCycleStreamSize", DefaultValue = 1024 * 1024)]
        public int DataBlockProcessorCycleStreamSize
        {
            get { return (int)this["dataBlockProcessorCycleStreamSize"]; }
            set { this["dataBlockProcessorCycleStreamSize"] = value; }
        }

        /// <summary>
        /// DataBlockProcessorBufferSize
        /// </summary>
        [ConfigurationProperty("dataBlockProcessorBufferSize", DefaultValue = 64 * 1024)]
        public int DataBlockProcessorBufferSize
        {
            get { return (int)this["dataBlockProcessorBufferSize"]; }
            set { this["dataBlockProcessorBufferSize"] = value; }
        }

        /// <summary>
        /// default encryption method
        /// </summary>
        [ConfigurationProperty("dataBlockEncryptionMethod", DefaultValue = EncryptionAlgorithm.AES_ENCRYPTION)]
        public EncryptionAlgorithm DataBlockEncryptionMethod
        {
            get { return (EncryptionAlgorithm)this["dataBlockEncryptionMethod"]; }
            set { this["dataBlockEncryptionMethod"] = value; }
        }

        /// <summary>
        /// default compression method
        /// </summary>
        [ConfigurationProperty("dataBlockCompressionMethod", DefaultValue = CompressionMethods.ZLIB_COMPRESSION)]
        public CompressionMethods DataBlockCompressionMethod
        {
            get { return (CompressionMethods)this["dataBlockCompressionMethod"]; }
            set { this["dataBlockCompressionMethod"] = value; }
        }

        /// <summary>
        /// max cache buffer size
        /// </summary>
        [ConfigurationProperty("maxCacheBuffer", DefaultValue = 200)]
        public int MaxCacheBuffer
        {
            get { return (int)this["maxCacheBuffer"]; }
            set { this["maxCacheBuffer"] = value; }
        }

        /// <summary>
        /// For WaitHandShakeToClose used, Default 1h
        /// </summary>
        [ConfigurationProperty("closeTimeout", DefaultValue = 60 * 60 * 1000)]
        public int CloseTimeout
        {
            get { return (int)this["closeTimeout"]; }
            set { this["closeTimeout"] = value; }
        }

        /// <summary>
        /// 获取DataBlock的Timeout时间
        /// </summary>
        [ConfigurationProperty("takeDataBlockTimeout", DefaultValue = 10000)]
        public int TakeDataBlockTimeOut
        {
            get { return (int)this["takeDataBlockTimeout"]; }
            set { this["takeDataBlockTimeout"] = value; }
        }

        /// <summary>
        /// disable performance logger
        /// </summary>
        [ConfigurationProperty("disablePerformanceLogger", DefaultValue = true)]
        public bool DisablePerformanceLogger
        {
            get { return (bool)this["disablePerformanceLogger"]; }
            set { this["disablePerformanceLogger"] = value; }
        }
        /// <summary>
        /// enable performance counter
        /// </summary>
        [ConfigurationProperty("enablePerformanceCounter", DefaultValue = true)]
        public bool EnablePerformanceCounter
        {
            get { return (bool)this["enablePerformanceCounter"]; }
            set { this["enablePerformanceCounter"] = value; }
        }
        /// <summary>
        /// File Transfer Service临时目录
        /// </summary>
        [ConfigurationProperty("fileTransferServiceTempFolder", DefaultValue="")]
        public string FileTransferServiceTempFolder
        {
            get { return (string)this["fileTransferServiceTempFolder"]; }
            set { this["fileTransferServiceTempFolder"] = value; }
        }

        /// <summary>
        /// Strean Mode max send size
        /// </summary>
        [ConfigurationProperty("streamModeMaxSendSize", DefaultValue = "1500000000")]
        public long StreamModeMaxSendSize
        {
            get { return (long)this["streamModeMaxSendSize"]; }
            set { this["streamModeMaxSendSize"] = value; }
        }

        [ConfigurationProperty("processDataWithNewThread", DefaultValue = true)]
        public bool ProcessDataWithNewThread
        {
            get { return (bool)this["processDataWithNewThread"]; }
            set { this["processDataWithNewThread"] = value; }
        }

        [ConfigurationProperty("fileCycleStreamSize", DefaultValue = 50)]
        public int FileCycleStreamSize
        {
            get { return (int)this["fileCycleStreamSize"]; }
            set { this["fileCycleStreamSize"] = value; }
        }

        [ConfigurationProperty("connectionTimeout", DefaultValue = 10)]
        public int ConnectionTimeout
        {
            get { return (int)this["connectionTimeout"]; }
            set { this["connectionTimeout"] = value; }
        }

        [ConfigurationProperty("deleteTransferFileInService", DefaultValue = true)]
        public bool DeleteTransferFileInService
        {
            get { return (bool)this["deleteTransferFileInService"]; }
            set { this["deleteTransferFileInService"] = value; }
        }
    }

    /// <summary>
    /// MQ Configuration Element
    /// </summary>
    public class MqConfigElement : ConfigurationElement
    {
        /// <summary>
        /// Temp folder to output diagnostic log
        /// </summary>
        [ConfigurationProperty("tempFolder", DefaultValue = "")]
        public string TempFolder
        {
            get { return (string)this["tempFolder"]; }
            set { this["tempFolder"] = value; }
        }
        /// <summary>
        /// message timeout interval
        /// </summary>
        [ConfigurationProperty("maxMessageTimeout", DefaultValue = 48 * (60 * 60 * 1000))]
        public int MaxMessageTimeout
        {
            get { return (int)this["maxMessageTimeout"]; }
            set { this["maxMessageTimeout"] = value; }
        }
        /// <summary>
        /// MQChannel Mode
        /// </summary>
        [ConfigurationProperty("channelMode", DefaultValue = AveChannelMode.WCF)]
        public AveChannelMode ChannelMode
        {
            get { return (AveChannelMode)this["channelMode"]; }
            set { this["channelMode"] = value; }
        }

        [ConfigurationProperty("isOneWayConnection", DefaultValue=false)]
        public bool IsOneWayConnection
        {
            get { return (bool)this["isOneWayConnection"]; }
            set { this["isOneWayConnection"] = value; }
        }
        /// <summary>
        /// max retry times
        /// </summary>
        [ConfigurationProperty("maxRetryTimes", DefaultValue=3)]
        public int MaxRetryTimes
        {
            get { return (int)this["maxRetryTimes"]; }
            set { this["maxRetryTimes"] = value; }
        }
        /// <summary>
        /// max reconnection timeout //断网重连 timeout时间 单位为秒
        /// </summary>
        [ConfigurationProperty("maxReconnectionTimeOut", DefaultValue = 1800)]
        public int MaxReconnectionTimeOut
        {
            get { return (int)this["maxReconnectionTimeOut"]; }
            set { this["maxReconnectionTimeOut"] = value; }
        }
        /// <summary>
        /// //断网重连间隔 单位为秒
        /// </summary>
        [ConfigurationProperty("reconnectionTime", DefaultValue = 2)]
        public int ReconnectionTime
        {
            get { return (int)this["reconnectionTime"]; }
            set { this["reconnectionTime"] = value; }
        }
        /// <summary>
        /// //上次重连结束之后在该段时间内出现的重连请求不处理
        /// </summary>
        [ConfigurationProperty("noReconnectTimeOut", DefaultValue = 3)]
        public int NoReconnectTimeOut
        {
            get { return (int)this["noReconnectTimeOut"]; }
            set { this["noReconnectTimeOut"] = value; }
        }
        /// <summary>
        /// MQ相关的Performance Counter信息
        /// </summary>
        [ConfigurationProperty("enablePerformanceCounter", DefaultValue = true)]
        public bool EnablePerformanceCounter
        {
            get { return (bool)this["enablePerformanceCounter"]; }
            set { this["enablePerformanceCounter"] = value; }
        }
    }

    /// <summary>
    /// DataTransfer Global Config 
    /// </summary>
    [Serializable]
    public class DataTransferGlobalConfig : MarshalByRefObject
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(DataTransferGlobalConfig), false);

        private const string ConfigurationFileName = "CommonDataTransfer.config";

        //static DataTransferGlobalConfig()
        //{
        //    try
        //    {
        //        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        //        AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("add type resolve event handler:{0}", ex.ToString());
        //        //throw;
        //    }
        //}

        //static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    if (args.Name.Equals("CommonDataTransfer", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return typeof(DataTransferGlobalConfig).Assembly;
        //    }
        //    if(args.Name.Equals("CommonUtility", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return typeof(DataTransferGlobalConfig).Assembly;
        //    }
        //    return null;
        //}

        //static System.Reflection.Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        //{
        //    if(args.Name.StartsWith("AvePoint.GCommon.Transfer", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return typeof(DataTransferGlobalConfig).Assembly;
        //    }
        //    if(args.Name.StartsWith("AvePoint.GCommon.Utility.SslStreamSecurity", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return typeof(DataTransferGlobalConfig).Assembly;
        //    }

        //    return null;
        //}

        private static string configurationFile = string.Empty;
        private static DataTransferSection dataTransferConfiguration;
        private static Configuration builtInConfiguration;
        private static string wcfThumbprint;

        /// <summary>
        /// configuration file
        /// </summary>
        public static string ConfigurationFile
        {
            get { return configurationFile; }
        }

        /// <summary>
        /// DataTransfer Config
        /// </summary>
        public static DataTransferSection DataTransferConfiguration
        {
            get { EnsureConfiguration(); return dataTransferConfiguration; }
        }

        /// <summary>
        /// Built in configuration
        /// </summary>
        internal static Configuration BuiltInConfiguration
        {
            get { EnsureConfiguration(); return builtInConfiguration; }
        }

        /// <summary>
        /// Wcf Thumbprint
        /// </summary>
        public static string WcfThumbprint
        {
            get { EnsureConfiguration(); return wcfThumbprint; }
        }

        /// <summary>
        /// 给ILMerge产品使用，可以指定configuration
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void LoadSpecialConfiguration(string configurationFile)
        {
            Load(configurationFile);
        }

        private static void LoadDefaultConfiguration()
        {
            string configuration = string.Empty;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!baseDirectory.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
            {
                baseDirectory = Path.Combine(baseDirectory, "bin");
            }
            if (Directory.Exists(baseDirectory))
            {
                configuration = Path.Combine(baseDirectory, ConfigurationFileName);
            }
            else
            {
                configuration = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
            }

            if (!File.Exists(configuration))
            {
                configuration = Path.Combine(Path.GetDirectoryName(typeof(DataTransferGlobalConfig).Assembly.Location), ConfigurationFileName);
            }

            Load(configuration);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Load(string configuration)
        {
            if (!File.Exists(configuration))
            {
                throw new FileNotFoundException(string.Format("Configuration file:{0} does not exist.", configuration));
            }

            logger.Info("The default configuration file is :{0}", configuration);

            var fileMap = new System.Configuration.ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configuration;
            builtInConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
            DataTransferUpgradeFactory.ProcessUpgrade(ref builtInConfiguration, () => System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None));
            dataTransferConfiguration = (DataTransferSection)builtInConfiguration.GetSection("dataTransfer");
            wcfThumbprint = LoadWcfThumbprint(builtInConfiguration);
            configurationFile = configuration;
        }

        /// <summary>
        /// Ensure Configuration
        /// </summary>
        private static void EnsureConfiguration()
        {
            if(string.IsNullOrEmpty(configurationFile))
            {
                lock(logger)
                {
                    if(string.IsNullOrEmpty(configurationFile))
                    {
                        LoadDefaultConfiguration();
                    }
                }
            }
        }

        /// <summary>
        /// reload configuration
        /// </summary>
        internal static void ReloadConfiguration()
        {
            builtInConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = configurationFile }, System.Configuration.ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Load Wcf Thumbprint from configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string LoadWcfThumbprint(Configuration configuration)
        {
            var thumbprint = string.Empty;

            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(configuration);
            if (serviceModel != null)
            {
                foreach (EndpointBehaviorElement item in serviceModel.Behaviors.EndpointBehaviors)
                {
                    foreach (BehaviorExtensionElement extenstion in item)
                    {
                        if (extenstion is ClientCredentialsElement)
                        {
                            var credentials = extenstion as ClientCredentialsElement;
                            thumbprint = credentials.ClientCertificate.FindValue;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(thumbprint)) { break; }
                }

                if (string.IsNullOrEmpty(thumbprint))
                {
                    foreach (ServiceBehaviorElement item in serviceModel.Behaviors.ServiceBehaviors)
                    {
                        foreach (BehaviorExtensionElement extenstion in item)
                        {
                            if (extenstion is ServiceCredentialsElement)
                            {
                                var credentials = extenstion as ServiceCredentialsElement;
                                thumbprint = credentials.ServiceCertificate.FindValue;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(thumbprint)) { break; }
                    }
                }
            }

            return thumbprint;
        }

        /// <summary>
        /// Update Wcf Thumbprint
        /// 
        /// Note: 请不要修改此名
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <returns></returns>
        public static bool UpdateWcfThumbprint(string thumbprint)
        {
            var changed = false;
            EnsureConfiguration();
            if (string.Compare(wcfThumbprint, thumbprint, StringComparison.Ordinal) != 0 && (!string.IsNullOrEmpty(thumbprint)))
            {
                DataTransferLogger.Logger(AveLogLevel.INFO, "start to update thumbprint from {0} to {1}", wcfThumbprint, thumbprint);

                var fileMap = new System.Configuration.ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configurationFile;
                var configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
                
                var serviceModel = ServiceModelSectionGroup.GetSectionGroup(configuration);
                if (serviceModel != null)
                {
                    foreach (EndpointBehaviorElement item in serviceModel.Behaviors.EndpointBehaviors)
                    {
                        foreach (BehaviorExtensionElement extenstion in item)
                        {
                            if (extenstion is ClientCredentialsElement)
                            {
                                var credentials = extenstion as ClientCredentialsElement;
                                credentials.ClientCertificate.FindValue = thumbprint;
                                changed = true;
                            }
                        }
                    }

                    foreach (ServiceBehaviorElement item in serviceModel.Behaviors.ServiceBehaviors)
                    {
                        foreach (BehaviorExtensionElement extenstion in item)
                        {
                            if (extenstion is ServiceCredentialsElement)
                            {
                                var credentials = extenstion as ServiceCredentialsElement;
                                credentials.ServiceCertificate.FindValue = thumbprint;
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                    {
                        configuration.Save(ConfigurationSaveMode.Modified);
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// Add Http Url Acl
        /// 
        /// Note: 请不要修改此名
        /// </summary>
        /// <param name="sidType"></param>
        /// <param name="overwrite"></param>
        public static void AddHttpUrlAcl(WELL_KNOWN_SID_TYPE sidType, bool overwrite)
        {
            HttpApi.AddHttpsAclUrl(AvePoint.GCommon.Transfer.Common.DataTransferGlobalConfig.DataTransferConfiguration.HttpModePort, sidType);
            HttpApi.BindCertificate("0.0.0.0", AvePoint.GCommon.Transfer.Common.DataTransferGlobalConfig.DataTransferConfiguration.HttpModePort, wcfThumbprint, overwrite);
        }

        ///// <summary>
        ///// Update Wcf Thumbprint
        ///// </summary>
        ///// <param name="thumbprint"></param>
        ///// <returns></returns>
        //public bool UpdateWcfThumbprint_(string thumbprint)
        //{
        //    return UpdateWcfThumbprint(thumbprint);
        //}

        ///// <summary>
        ///// Add Http Url Acl
        ///// </summary>
        ///// <param name="sidType"></param>
        ///// <param name="overwrite"></param>
        //public void AddHttpUrlAcl_(WELL_KNOWN_SID_TYPE sidType, bool overwrite)
        //{
        //    AddHttpUrlAcl(sidType, overwrite);
        //}
    }

    /// <summary>
    /// Upgrade interface
    /// </summary>
    internal interface IUpgrade
    {
        /// <summary>
        /// The current version for the upgrade
        /// </summary>
        Version CurrentVersion
        {
            get;
        }
        /// <summary>
        /// check if it's available to upgrade
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        bool IsAvailable(Version version);
        /// <summary>
        /// upgrade if it's available.
        /// 
        /// result: 
        /// true  ==> need to reload configuration
        /// false ==> no need to reload configuration
        /// </summary>
        /// <returns></returns>
        bool Upgrade(Configuration configuration);
    }

    /// <summary>
    /// Version 0.0.0.0 -> 1.0.0.0
    /// </summary>
    internal class DataTransferUpgradeV1 : IUpgrade //: IComparable<IUpgrade>
    {
        /// <summary>
        /// Current version is 1.0.0.0
        /// </summary>
        public Version CurrentVersion
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// Is available to update
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool IsAvailable(Version version)
        {
            if (CurrentVersion > version)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Upgrade 0.0.0.0 --> 1.0.0.0
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool Upgrade(Configuration configuration)
        {
            var names = typeof(DataTransferUpgradeV1).Assembly.GetManifestResourceNames();

            var name = "AvePoint.GCommon.Transfer.CommonDataTransfer.config";

            foreach(var item in names)
            {
                if(item.Contains("CommonDataTransfer.config"))
                {
                    name = item;
                    break;
                }
            }

            using (var configurationResourceStream = typeof(DataTransferUpgradeV1).Assembly.GetManifestResourceStream(name))
            {
                var defaultConfiguration = new XmlDocument();
                defaultConfiguration.Load(configurationResourceStream);
                var defaultDataTransferElement = defaultConfiguration.SelectSingleNode("configuration/dataTransfer") as XmlElement;

                var document = new XmlDocument();
                document.Load(configuration.FilePath);

                var element = document.SelectSingleNode("configuration/dataTransfer") as XmlElement;

                var changed = UpgradeDataTransfer(element, defaultDataTransferElement);
                changed |= UpdateSectionType(document.DocumentElement);
                changed |= OverwriteSystemServiceModel(document.DocumentElement, defaultConfiguration.DocumentElement);
                changed |= UpdateVersion(document.DocumentElement,"1.0.0.0");

                if (changed)
                {
                    document.Save(configuration.FilePath);
                }
            }

            return true;
        }

        /// <summary>
        /// Update Version
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool UpdateVersion(XmlElement element, string version)
        {
            var appSettings = element.SelectSingleNode("appSettings");

            var find = false;

            if (appSettings == null)
            {
                appSettings = element.OwnerDocument.CreateElement("appSettings");
                element.InsertAfter(appSettings, element.FirstChild);
            }
            else
            {
                foreach (XmlNode item in appSettings.ChildNodes)
                {
                    if (item.Name.Equals("add", StringComparison.Ordinal) && item is XmlElement)
                    {
                        var addElement = item as XmlElement;

                        var versionString = addElement.GetAttribute("key");
                        if (!string.IsNullOrEmpty(versionString) && versionString.Equals("version", StringComparison.OrdinalIgnoreCase))
                        {
                            addElement.SetAttribute("value", version);
                            find = true;
                            break;
                        }
                    }
                }
            }


            if (!find)
            {
                var versionElement = element.OwnerDocument.CreateElement("add");
                versionElement.SetAttribute("key", "version");
                versionElement.SetAttribute("value", version);
                appSettings.AppendChild(versionElement);
            }

            return true;
        }

        /// <summary>
        /// Update section Type
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool UpdateSectionType(XmlElement element)
        {
            var changed = false;
            var sections = element.SelectSingleNode("configSections");

            if (sections != null && sections.HasChildNodes)
            {
                foreach (XmlNode item in sections.ChildNodes)
                {
                    if (item.Name.Equals("section", StringComparison.Ordinal) && item.NodeType == XmlNodeType.Element)
                    {
                        var itemElement = item as XmlElement;
                        var name = itemElement.GetAttribute("name");

                        if (string.Compare(name, "dataTransfer", StringComparison.Ordinal) == 0)
                        {
                            itemElement.SetAttribute("type", "AvePoint.GCommon.Transfer.Common.DataTransferSection, CommonDataTransfer");
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// upgrade data transfer
        /// </summary>
        /// <param name="element"></param>
        /// <param name="defaultDataTransferConfig"></param>
        /// <returns></returns>
        private static bool UpgradeDataTransfer(XmlElement element, XmlElement defaultDataTransferConfig)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            else if (defaultDataTransferConfig == null)
            {
                throw new ArgumentNullException("defaultDataTransferConfig");
            }

            var changed = false;

            changed |= CompareAndMergeAttributes(defaultDataTransferConfig.Attributes, element.Attributes, element.OwnerDocument);

            var dataElement = element.SelectSingleNode("data") as XmlElement;

            var defaultDataElement = defaultDataTransferConfig.SelectSingleNode("data") as XmlElement;

            if (defaultDataElement == null)
            {
                throw new ArgumentNullException("defaultDataElement");
            }
            else if (dataElement == null)
            {
                element.AppendChild(element.OwnerDocument.ImportNode(defaultDataElement, true));
                changed = true;
            }
            else
            {
                changed |= CompareAndMergeAttributes(defaultDataElement.Attributes, dataElement.Attributes, element.OwnerDocument);
            }

            var mqElement = element.SelectSingleNode("mq") as XmlElement;

            var defaultMqElement = defaultDataTransferConfig.SelectSingleNode("mq") as XmlElement;

            if (defaultMqElement == null)
            {
                throw new ArgumentNullException("defaultMqElement");
            }
            else if (mqElement == null)
            {
                element.AppendChild(element.OwnerDocument.ImportNode(defaultMqElement, true));
                changed = true;
            }
            else
            {
                changed |= CompareAndMergeAttributes(defaultMqElement.Attributes, mqElement.Attributes, element.OwnerDocument);
            }

            return changed;
        }

        /// <summary>
        /// source = default configuration
        /// dest   = history version configuration
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        private static bool CompareAndMergeAttributes(XmlAttributeCollection source, XmlAttributeCollection dest, XmlDocument destDocument)
        {
            var changed = false;

            var sourceCollection = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (XmlAttribute item in source)
            {
                sourceCollection[item.Name] = item.Value;
            }



            for (var index = dest.Count - 1; index >= 0; index--)
            {
                var item = dest[index];

                var newName = item.Name;
                var changeCurrentName = false;
                if(!Char.IsLower(item.Name[0]))
                {
                    newName = Char.ToLowerInvariant(item.Name[0]) + item.Name.Substring(1);
                    changeCurrentName = true;
                }

                if (sourceCollection.ContainsKey(newName))
                {
                    sourceCollection.Remove(newName);

                    if(changeCurrentName)
                    {
                        dest.RemoveAt(index);
                        var newItem = destDocument.CreateAttribute(newName);
                        newItem.Value = item.Value;
                        if(index == 0)
                        {
                            dest.Prepend(newItem);
                        }
                        else
                        {
                            dest.InsertAfter(newItem, dest[index - 1]);
                        }
                        changed = true;
                    }
                }
                else
                {
                    dest.RemoveAt(index);
                    changed = true;
                }
            }

            foreach (var item in sourceCollection)
            {
                var attribute = destDocument.CreateAttribute(item.Key);
                attribute.Value = item.Value;
                dest.Append(attribute);
                changed = true;
            }

            return changed;
        }

        /// <summary>
        /// Overwrite System Service Model
        /// </summary>
        /// <param name="currentConfiguration"></param>
        /// <param name="defaultConfiugration"></param>
        /// <returns></returns>
        private static bool OverwriteSystemServiceModel(XmlElement currentConfiguration, XmlElement defaultConfiugration)
        {
            var node = currentConfiguration.SelectSingleNode("system.serviceModel");

            var defaultNode = defaultConfiugration.SelectSingleNode("system.serviceModel");

            if (defaultNode == null)
            {
                throw new NullReferenceException("defaultNode");
            }
            else if (node == null)
            {
                currentConfiguration.AppendChild(currentConfiguration.OwnerDocument.ImportNode(defaultNode, true));
            }
            else
            {
                currentConfiguration.RemoveChild(node);
                currentConfiguration.AppendChild(currentConfiguration.OwnerDocument.ImportNode(defaultNode, true));
            }

            return true;
        }
    }

    /// <summary>
    /// Version 1.0.0.0 -> 1.0.0.1
    /// </summary>
    internal class DataTransferUpgradeV1001 : IUpgrade //: IComparable<IUpgrade>
    {
        /// <summary>
        /// Current version is 1.0.0.0
        /// </summary>
        public Version CurrentVersion
        {
            get { return new Version(1, 0, 0, 1); }
        }

        /// <summary>
        /// Is available to update
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool IsAvailable(Version version)
        {
            if (CurrentVersion > version)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Upgrade 1.0.0.0 --> 1.0.0.1
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public bool Upgrade(Configuration configuration)
        {
            try
            {
                XmlDocument configXml = new XmlDocument();
                configXml.Load(configuration.FilePath);
                XmlElement dataTransferEle = configXml.DocumentElement.SelectSingleNode("dataTransfer") as XmlElement;
                dataTransferEle.SetAttribute("defaultDataUriSchema", "net.tcp");
                dataTransferEle.SetAttribute("enableStreamMode", "false");
                DataTransferUpgradeV1.UpdateVersion(configXml.DocumentElement, "1.0.0.1");
                configXml.Save(configuration.FilePath);
            }
            catch (Exception e)
            {
                DataTransferLogger.Logger(AveLogLevel.ERROR, e.ToString());
            }
            return true;
        }
    }

    internal class DataTransferUpgradeV1002 : IUpgrade
    {
        public Version CurrentVersion
        {
            get { return new Version(1, 0, 0, 2); }
        }

        public bool IsAvailable(Version version)
        {
            if (CurrentVersion > version)
            {
                return true;
            }
            return false;
        }

        public bool Upgrade(Configuration configuration)
        {
            var succeed = false;
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(configuration.FilePath);
                var serviceModelRoot = xmlDocument.DocumentElement.SelectSingleNode("system.serviceModel");
                var bindingNodeList = serviceModelRoot.SelectNodes("bindings/customBinding/binding");

                foreach (XmlNode xmlNode in bindingNodeList)
                {
                    var oldNode = xmlNode.SelectSingleNode("sslStreamSecurity");
                    if (oldNode != null)
                    {
                        var newNode = xmlDocument.CreateElement("XsslStreamSecurity");
                        newNode.SetAttribute("requireClientCertificate", "true");

                        xmlNode.ReplaceChild(newNode, oldNode);
                    }
                }

                var extensionsNode = serviceModelRoot.SelectSingleNode("extensions");
                if (extensionsNode == null)
                {
                    extensionsNode = xmlDocument.CreateElement("extensions");
                    serviceModelRoot.AppendChild(extensionsNode);
                }
                var bindingElementExtensionsNode = extensionsNode.SelectSingleNode("bindingElementExtensions");
                if (bindingElementExtensionsNode == null)
                {
                    bindingElementExtensionsNode = xmlDocument.CreateElement("bindingElementExtensions");
                    extensionsNode.AppendChild(bindingElementExtensionsNode);
                }
                var xsslStreamSecurityNodeFound = default(bool);
                foreach (XmlElement childNode in bindingElementExtensionsNode.ChildNodes)
                {
                    if (childNode.HasAttribute("name") && "XsslStreamSecurity".Equals(childNode.GetAttribute("name"), StringComparison.OrdinalIgnoreCase))
                    {
                        xsslStreamSecurityNodeFound = true;
                        break;
                    }
                }
                if (!xsslStreamSecurityNodeFound)
                {
                    var xsslStreamSecurityNode = xmlDocument.CreateElement("add");
                    xsslStreamSecurityNode.SetAttribute("name", "XsslStreamSecurity");
                    xsslStreamSecurityNode.SetAttribute("type", "AvePoint.GCommon.Utility.SslStreamSecurity.XSslStreamSecurityElement, CommonUtility, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fffb45e56dd478e3");
                    bindingElementExtensionsNode.AppendChild(xsslStreamSecurityNode);
                }

                DataTransferUpgradeV1.UpdateVersion(xmlDocument.DocumentElement, "1.0.0.2");
                xmlDocument.Save(configuration.FilePath);
                succeed = true;
            }
            catch (Exception ex)
            {
                DataTransferLogger.Logger(AveLogLevel.ERROR, "An error occurred while upgrading data transfer configuration file, error: {0}", ex.ToString());
            }
            return succeed;
        }
    }

    /// <summary>
    /// DataTransferVersionUtility
    /// </summary>
    internal static class DataTransferVersionUtility
    {
        /// <summary>
        /// DataTransferVersion Const string
        /// </summary>
        private const string DataTransferVersion = "version";

        /// <summary>
        /// Get version from configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static Version GetVersionFromConfiguration(Configuration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            var versionConfig = configuration.AppSettings.Settings[DataTransferVersion];
            Version currentVersion;
            if (versionConfig != null)
            {
                currentVersion = new Version(versionConfig.Value);
            }
            else
            {
                currentVersion = new Version(0, 0);
            }

            return currentVersion;
        }

        /// <summary>
        /// Set version for configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static void SetVersionFromConfiguration(Configuration configuration, Version version)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            var versionConfig = configuration.AppSettings.Settings[DataTransferVersion];

            if (versionConfig == null)
            {
                configuration.AppSettings.Settings.Add(DataTransferVersion, version.ToString());
            }
            else
            {
                configuration.AppSettings.Settings[DataTransferVersion].Value = version.ToString();
            }
        }
    }

    /// <summary>
    /// DataTransfer Upgrade Factory
    /// </summary>
    internal class DataTransferUpgradeFactory
    {
        /// <summary>
        /// Upgrade collection
        /// </summary>
        private static readonly List<IUpgrade> upgrades = new List<IUpgrade>();

        /// <summary>
        /// the latest upgrade
        /// </summary>
        public static IUpgrade HighUpgrade
        {
            get 
            {
                if (upgrades != null && upgrades.Count > 0)
                {
                    return upgrades[0];
                }
                return null;
            }
        }

        /// <summary>
        /// Static Method
        /// </summary>
        static DataTransferUpgradeFactory()
        {
            upgrades.Add(new DataTransferUpgradeV1());
            upgrades.Add(new DataTransferUpgradeV1001());
            upgrades.Add(new DataTransferUpgradeV1002());
            upgrades.Sort(UpgradeComparison);
        }

        /// <summary>
        /// Process Upgrade
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="reloadConfiguration"></param>
        internal static void ProcessUpgrade(ref Configuration configuration, Func<Configuration> reloadConfiguration)
        {
            var availableUpgrades = new List<IUpgrade>();
            if (upgrades != null && upgrades.Count > 0)
            {
                var currentVersion = DataTransferVersionUtility.GetVersionFromConfiguration(configuration);
                foreach (var upgrade in upgrades)
                {
                    if (upgrade.IsAvailable(currentVersion))
                    {
                        availableUpgrades.Add(upgrade);
                    }
                    else
                    {
                        break;
                    }
                }

                for (var i = availableUpgrades.Count - 1; i >= 0; i--)
                {
                    DataTransferLogger.Logger(AveLogLevel.INFO, "start to upgrade to {0}", availableUpgrades[i].CurrentVersion);
                    if (availableUpgrades[i].Upgrade(configuration))
                    {
                        if (reloadConfiguration != null)
                        {
                            configuration = reloadConfiguration();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// IUpgrade Comparison
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int UpgradeComparison(IUpgrade x, IUpgrade y)
        {
            int compareResult = 0;

            if (x == null)
            {
                if (y != null)
                {
                    compareResult = 1;
                }
            }
            else
            {
                if (y == null)
                {
                    compareResult = -1;
                }
                else if (x.CurrentVersion > y.CurrentVersion)
                {
                    compareResult = -1;
                }
                else if (x.CurrentVersion < y.CurrentVersion)
                {
                    compareResult = 1;
                }
            }

            return compareResult;
        }
    }


//    class XmlConfiguration
//    {
//        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(XmlConfiguration), false);

//        private static object syncObj = new object();
//        private static string configurationFile = string.Empty;
//        private static Configuration configuration = null;

//        static XmlConfiguration()
//        {
//            InitiateConfiguration();
//        }

//        public static string ConfigurationFile
//        {
//            get { return configurationFile; }
//        }

//        public static Configuration BuiltInConfiguration
//        {
//            get 
//            {
//                if(configuration == null)
//                {
//                    var fileMap = new System.Configuration.ExeConfigurationFileMap();
//                    fileMap.ExeConfigFilename = configurationFile;
//                    configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
//                }

//                return configuration;
//            }
//        }

//        private static void InitiateConfiguration()
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(configurationFile))
//                {
//                    lock (syncObj)
//                    {
//                        if (string.IsNullOrEmpty(configurationFile))
//                        {
//                            XmlElement element = null;

//                            var document = new XmlDocument();

//                            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
//                            if (!baseDirectory.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
//                            {
//                                baseDirectory = Path.Combine(baseDirectory, "bin");
//                            }
//                            if (Directory.Exists(baseDirectory))
//                            {
//                                configurationFile = Path.Combine(baseDirectory, "CommonDataTransfer.config");
//                            }
//                            else
//                            {
//                                configurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonDataTransfer.config");
//                            }

//                            document.Load(configurationFile);

//                            var node = document.SelectSingleNode("configuration/dataTransfer");
//                            if (node != null)
//                            {
//                                element = node as XmlElement;
//                            }

//                            logger.Debug("The configuration file is {0}.", configurationFile);

                            

//                            using (var configurationResourceStream = typeof(XmlConfiguration).Assembly.GetManifestResourceStream("CommonDataTransfer.config"))
//                            {
//                                var defaultConfiguration = new XmlDocument();
//                                defaultConfiguration.Load(configurationResourceStream);
//                                var defaultDataTransferElement = defaultConfiguration.SelectSingleNode("configuration/dataTransfer") as XmlElement;

//                                var versionNumber = element.GetAttribute("version");

//                                if (string.IsNullOrEmpty(versionNumber))
//                                {
//                                    var changed = UpgradeDataTransfer(element, defaultDataTransferElement);
//                                    changed |= OverwriteSystemServiceModel(document.DocumentElement, defaultConfiguration.DocumentElement);

//                                    if (changed)
//                                    {
//                                        document.Save(configurationFile);
//                                    }
//                                }
//                                //TODO if has version upgrade
//                            }
                            
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Initialize the configuration failed:{0}.", ex.ToString());
//                throw;
//            }
//        }

//        private static bool UpgradeDataTransfer(XmlElement element, XmlElement defaultDataTransferConfig)
//        {
//            if (element == null)
//            {
//                throw new ArgumentNullException("element");
//            }
//            else if(defaultDataTransferConfig == null)
//            {
//                throw new ArgumentNullException("defaultDataTransferConfig");
//            }

//            var changed = false;

//            changed |= CompareAndMergeAttributes(defaultDataTransferConfig.Attributes, element.Attributes, element.OwnerDocument);

//            var dataElement = element.SelectSingleNode("configuration/dataTransfer/data") as XmlElement;

//            var defaultDataElement = defaultDataTransferConfig.SelectSingleNode("configuration/dataTransfer/data") as XmlElement;

//            if(defaultDataElement == null)
//            {
//                throw new ArgumentNullException("defaultDataElement");
//            }
//            else if(dataElement == null)
//            {
//                element.AppendChild(element.OwnerDocument.ImportNode(defaultDataElement, true));
//                changed = true;
//            }
//            else
//            {
//                changed |= CompareAndMergeAttributes(defaultDataElement.Attributes, dataElement.Attributes, element.OwnerDocument);
//            }

//            var mqElement = element.SelectSingleNode("configuration/dataTransfer/mq") as XmlElement;

//            var defaultMqElement = defaultDataTransferConfig.SelectSingleNode("configuration/dataTransfer/mq") as XmlElement;

//            if (defaultMqElement == null)
//            {
//                throw new ArgumentNullException("defaultMqElement");
//            }
//            else if (mqElement == null)
//            {
//                element.AppendChild(element.OwnerDocument.ImportNode(defaultMqElement, true));
//                changed = true;
//            }
//            else
//            {
//                changed |= CompareAndMergeAttributes(defaultMqElement.Attributes, mqElement.Attributes, element.OwnerDocument);
//            }

//            return changed;
//        }

//        /// <summary>
//        /// source = default configuration
//        /// dest   = history version configuration
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="dest"></param>
//        /// <returns></returns>
//        private static bool CompareAndMergeAttributes(XmlAttributeCollection source, XmlAttributeCollection dest, XmlDocument destDocument)
//        {
//            var changed = false;

//            var sourceCollection = new Dictionary<string, string>(StringComparer.Ordinal);

//            foreach(XmlAttribute item in source)
//            {
//                sourceCollection[item.Name] = item.Value;
//            }

//            for(var index = dest.Count - 1; index >= 0; index--)
//            {
//                var item = dest[index];

//                if (sourceCollection.ContainsKey(item.Name))
//                {
//                    sourceCollection.Remove(item.Name);
//                }
//                else
//                {
//                    dest.RemoveAt(index);
//                    changed = true;
//                }
//            }

//            foreach (var item in sourceCollection)
//            {
//                var attribute = destDocument.CreateAttribute(item.Key);
//                attribute.Value = item.Value;
//                dest.Append(attribute);
//                changed = true;
//            }


//            return changed;
//        }

//        /// <summary>
//        /// Overwrite System Service Model
//        /// </summary>
//        /// <param name="currentConfiguration"></param>
//        /// <param name="defaultConfiugration"></param>
//        /// <returns></returns>
//        private static bool OverwriteSystemServiceModel(XmlElement currentConfiguration, XmlElement defaultConfiugration)
//        {
//            var node = currentConfiguration.SelectSingleNode("configuration/system.serviceModel");

//            var defaultNode = defaultConfiugration.SelectSingleNode("configuration/system.serviceModel");

//            if(defaultNode == null)
//            {
//                throw new NullReferenceException("defaultNode");
//            }
//            else if(node == null)
//            {
//                currentConfiguration.AppendChild(currentConfiguration.OwnerDocument.ImportNode(defaultNode, true));
//            }
//            else
//            {
//                currentConfiguration.RemoveChild(node);
//                currentConfiguration.AppendChild(currentConfiguration.OwnerDocument.ImportNode(defaultNode, true));
//            }

//            return true;
//        }

//        //private static bool UpgradeDataTransferAndMQ(XmlElement element)
//        //{
//        //    if (element == null)
//        //    {
//        //        throw new ArgumentNullException("element");
//        //    }

//        //    bool changed = false;

//        //    XmlElement dataTransferEle = null;
//        //    XmlElement mqEle = null;
//        //    string nodeName = "MQ";

//        //    foreach (XmlNode subNode in element.ChildNodes)
//        //    {
//        //        if (subNode.Name.Equals("data", StringComparison.OrdinalIgnoreCase))
//        //        {
//        //            dataTransferEle = subNode as XmlElement;
//        //        }
//        //        else if (subNode.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
//        //        {
//        //            mqEle = subNode as XmlElement;
//        //        }
//        //    }

//        //    if (dataTransferEle == null)
//        //    {
//        //        dataTransferEle = element.OwnerDocument.CreateElement("data");
//        //        element.AppendChild(dataTransferEle);
//        //        changed = true;
//        //    }
//        //    if (mqEle == null)
//        //    {
//        //        mqEle = element.OwnerDocument.CreateElement(nodeName.ToLowerInvariant());
//        //        element.AppendChild(mqEle);
//        //        changed = true;
//        //    }

//        //    DataTransferConfiguration.Binding = GetAttributeFromXmlElement(xmlElement, "Binding", DataTransferConfiguration.Binding, ref changed);
            
//        //    DataTransferConfiguration.UriSchema = GetAttributeFromXmlElement(xmlElement, "UriSchema", DataTransferConfiguration.UriSchema, ref changed);
//        //    DataTransferConfiguration.FileTransferServiceUriSchema = GetAttributeFromXmlElement(xmlElement, "FileTransferServiceUriSchema", DataTransferConfiguration.FileTransferServiceUriSchema, ref changed);
//        //    DataTransferConfiguration.MqUriSchema = GetAttributeFromXmlElement(xmlElement, "MqUriSchema", DataTransferConfiguration.MqUriSchema, ref changed);
//        //    DataTransferConfiguration.RelayServiceUriSchema = GetAttributeFromXmlElement(xmlElement, "RelayServiceUriSchema", DataTransferConfiguration.RelayServiceUriSchema, ref changed);
//        //    DataTransferConfiguration.StreamModeServiceUriSchema = GetAttributeFromXmlElement(xmlElement, "StreamModeServiceUriSchema", DataTransferConfiguration.StreamModeServiceUriSchema, ref changed);

//        //    DataTransferConfiguration.EnableSsl = bool.Parse(GetAttributeFromXmlElement(xmlElement, "EnableSsl", DataTransferConfiguration.EnableSsl.ToString(), ref changed));

//        //    DataTransferConfiguration.DefaultDataBindingName = GetAttributeFromXmlElement(xmlElement, "DefaultDataBindingName", DataTransferConfiguration.DefaultDataBindingName.ToString(), ref changed);
//        //    DataTransferConfiguration.HttpModePort = int.Parse(GetAttributeFromXmlElement(xmlElement, "HttpPort", DataTransferConfiguration.HttpModePort.ToString(), ref changed));
//        //    DataTransferConfiguration.CacheDataNumber = int.Parse(GetAttributeFromXmlElement(xmlElement, "CacheDataNumber", DataTransferConfiguration.CacheDataNumber.ToString(), ref changed));
//        //    DataTransferConfiguration.CycleStreamSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "CycleStreamSize", DataTransferConfiguration.CycleStreamSize.ToString(), ref changed));
//        //    DataTransferConfiguration.DataBlockCompressionMethod = (CompressionMethods)Enum.Parse(typeof(CompressionMethods),
//        //        GetAttributeFromXmlElement(dataTransferEle, "DataBlockCompressionMethod", DataTransferConfiguration.DataBlockCompressionMethod.ToString(), ref changed));
//        //    DataTransferConfiguration.DataBlockEncryptionMethod = (EncryptionAlgorithm)Enum.Parse(typeof(EncryptionAlgorithm),
//        //        GetAttributeFromXmlElement(dataTransferEle, "DataBlockEncryptionMethod", DataTransferConfiguration.DataBlockEncryptionMethod.ToString(), ref changed));
//        //    DataTransferConfiguration.DataBlockProcessorBufferSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DataBlockProcessorBufferSize", DataTransferConfiguration.DataBlockProcessorBufferSize.ToString(), ref changed));
//        //    DataTransferConfiguration.DataBlockProcessorCycleStreamSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DataBlockProcessorCycleStreamSize", DataTransferConfiguration.DataBlockProcessorCycleStreamSize.ToString(), ref changed));
//        //    DataTransferConfiguration.DefaultReconnectTimeout = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DefaultReconnectTimeout", DataTransferConfiguration.DefaultReconnectTimeout.ToString(), ref changed));
//        //    DataTransferConfiguration.MaxCacheBuffer = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "MaxCacheBuffer", DataTransferConfiguration.MaxCacheBuffer.ToString(), ref changed));
//        //    DataTransferConfiguration.MinReconnectTimeout = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "MinReconnectTimeout", DataTransferConfiguration.MinReconnectTimeout.ToString(), ref changed));
//        //    DataTransferConfiguration.SendBufferSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "SendBufferSize", DataTransferConfiguration.SendBufferSize.ToString(), ref changed));
//        //    DataTransferConfiguration.TakeDataBlockTimeOut = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "TakeDataBlockTimeOut", DataTransferConfiguration.TakeDataBlockTimeOut.ToString(), ref changed));
//        //    DataTransferConfiguration.DisablePerformanceLogger = bool.Parse(GetAttributeFromXmlElement(dataTransferEle, "DisablePerformanceLogger", DataTransferConfiguration.DisablePerformanceLogger.ToString(), ref changed));
//        //    DataTransferConfiguration.EnablePerformanceCounter = bool.Parse(GetAttributeFromXmlElement(dataTransferEle, "EnablePerformanceCounter", DataTransferConfiguration.EnablePerformanceCounter.ToString(), ref changed));

//        //    /*
//        //     * Remove unused attribute from 6.0 configuration file
//        //     */
//        //    RemoveAttributeFromXmlElement(dataTransferEle, "UriSchema", ref changed);
//        //    RemoveAttributeFromXmlElement(dataTransferEle, "DefaultDataBindingName", ref changed);

//        //    AveMQConfigure.IsOneWayConnection = bool.Parse(GetAttributeFromXmlElement(mqEle, "IsOneWayConnection", AveMQConfigure.IsOneWayConnection.ToString(), ref changed));
//        //    AveMQConfigure.MaxMessageTimeout = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxMessageTimeout", AveMQConfigure.MaxMessageTimeout.ToString(), ref changed));
//        //    AveMQConfigure.MaxReconnectionTimeOut = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxReconnectionTimeOut", AveMQConfigure.MaxReconnectionTimeOut.ToString(), ref changed));
//        //    AveMQConfigure.MaxRetryTimes = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxRetryTimes", AveMQConfigure.MaxRetryTimes.ToString(), ref changed));
//        //    AveMQConfigure.MQChannelMode = (AveChannelMode)Enum.Parse(typeof(AveChannelMode),
//        //        GetAttributeFromXmlElement(mqEle, "MQChannelMode", AveMQConfigure.MQChannelMode.ToString(), ref changed));
//        //    AveMQConfigure.NoReconnectTimeOut = int.Parse(GetAttributeFromXmlElement(mqEle, "NoReconnectTimeOut", AveMQConfigure.NoReconnectTimeOut.ToString(), ref changed));
//        //    AveMQConfigure.ReconnectionTime = int.Parse(GetAttributeFromXmlElement(mqEle, "ReconnectionTime", AveMQConfigure.ReconnectionTime.ToString(), ref changed));
//        //    AveMQConfigure.TempFolder = GetAttributeFromXmlElement(mqEle, "TempFolder", AveMQConfigure.TempFolder.ToString(), ref changed);
//        //    AveMQConfigure.EnablePerformanceCounter = bool.Parse(GetAttributeFromXmlElement(mqEle, "EnablePerformanceCounter", AveMQConfigure.EnablePerformanceCounter.ToString(), ref changed));
//        //    //set MQ relative information
//        //    return changed;
//        //}

////        /// <summary>
////        /// upgrade ServiceModel中的节点
////        /// </summary>
////        /// <param name="currentConfiguration"></param>
////        /// <returns></returns>
////        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "WCF configuration file")]
////        private static bool UpgradeSystemServiceModel(XmlElement currentConfiguration, XmlElement defaultConfiugration)
////        {
////            bool changed = false;

////            Tuple<XmlElement, bool> serviceModel = FindAndCreateSubElement(currentConfiguration, "system.serviceModel");
////            changed |= serviceModel.Item2;
////            Tuple<XmlElement, bool> bindings = FindAndCreateSubElement(serviceModel.Item1, "bindings");
////            changed |= bindings.Item2;
////            Tuple<Dictionary<string, XmlElement>, bool> subBinding = FindAndCreateSubElements(bindings.Item1, "basicHttpBinding",
////                "netTcpBinding", "wsDualHttpBinding", "customBinding");
////            changed |= subBinding.Item2;

////            Tuple<XmlElement, bool> basicHttpBinding = EnsureSubBinding(subBinding.Item1["basicHttpBinding"], "DataTransferDefaultDataBinding",
////@"
////        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
////                 maxBufferPoolSize=""524288"" maxBufferSize=""536870912"" transferMode=""Buffered"" maxReceivedMessageSize=""536870912""
////                 hostNameComparisonMode=""StrongWildcard"">
////          <security mode=""None""></security>
////          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////        </binding>");
////            changed |= basicHttpBinding.Item2;

////            Tuple<XmlElement, bool> basicHttpBindingForStreamMode = EnsureSubBinding(subBinding.Item1["basicHttpBinding"], "DataTransferHttpStreamModeDataBinding",
////@"
////        <binding name=""DataTransferHttpStreamModeDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""00:10:00"" sendTimeout=""00:10:00"" 
////                 maxBufferPoolSize=""2147483647"" maxBufferSize=""2147483647"" transferMode=""Streamed"" maxReceivedMessageSize=""2147483647""
////                 hostNameComparisonMode=""StrongWildcard"">
////          <security mode=""None""></security>
////          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////        </binding>");
////            changed |= basicHttpBindingForStreamMode.Item2;

////            Tuple<XmlElement, bool> netTcpBinding = EnsureSubBinding(subBinding.Item1["netTcpBinding"], "DataTransferDefaultDataBinding",
////@"
////        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
////                 maxBufferSize=""536870912"" maxBufferPoolSize=""524288"" transferMode=""Buffered"" listenBacklog=""10"" maxReceivedMessageSize=""536870912""
////                 hostNameComparisonMode=""StrongWildcard"" portSharingEnabled=""True"">
////          <security mode=""None""></security>
////          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////        </binding>");
////            changed |= netTcpBinding.Item2;

////            Tuple<XmlElement, bool> netTCPBindingForStreamMode = EnsureSubBinding(subBinding.Item1["netTcpBinding"], "DataTransferNetTCPStreamModeDataBinding",
////@"
////        <binding name=""DataTransferNetTCPStreamModeDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""00:10:00"" sendTimeout=""01:10:00"" maxBufferSize=""2147483647"" maxBufferPoolSize=""2147483647"" transferMode=""Streamed"" listenBacklog=""10"" maxReceivedMessageSize=""2147483647"" hostNameComparisonMode=""StrongWildcard"" portSharingEnabled=""True"">
////          <security mode=""None"">
////          </security>
////          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////        </binding>");
////            changed |= netTCPBindingForStreamMode.Item2;

////            Tuple<XmlElement, bool> wsDualHttpBinding = EnsureSubBinding(subBinding.Item1["wsDualHttpBinding"], "DataTransferDefaultDataBinding",
////@"
////        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
////                 maxBufferPoolSize=""524288"" maxReceivedMessageSize=""536870912"" hostNameComparisonMode=""StrongWildcard"">
////          <security mode=""None""></security>
////          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////        </binding>");
////            changed |= wsDualHttpBinding.Item2;

////            Tuple<XmlElement, bool> customBinding = EnsureSubBinding(subBinding.Item1["customBinding"], "DataTransferDefaultDataBinding",
////@"
////        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"">
////          <transactionFlow transactionProtocol=""OleTransactions"" />
////          <binaryMessageEncoding maxReadPoolSize=""64"" maxWritePoolSize=""16"" maxSessionSize=""2048"">
////            <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
////          </binaryMessageEncoding>
////          <!--<sslStreamSecurity requireClientCertificate=""true"" />-->
////          <tcpTransport manualAddressing=""false"" maxBufferPoolSize=""524288""
////            maxReceivedMessageSize=""536870912"" connectionBufferSize=""102400""
////            hostNameComparisonMode=""StrongWildcard"" channelInitializationTimeout=""00:00:05""
////            maxBufferSize=""536870912"" maxPendingConnections=""10"" maxOutputDelay=""00:00:00.2000000""
////            maxPendingAccepts=""1"" transferMode=""Buffered"" listenBacklog=""10""
////            portSharingEnabled=""true"" teredoEnabled=""false"">
////            <connectionPoolSettings groupName=""default"" leaseTimeout=""00:05:00""
////              idleTimeout=""00:02:00"" maxOutboundConnectionsPerEndpoint=""10"" />
////          </tcpTransport>
////        </binding>");
////            changed |= customBinding.Item2;

////            return changed;
////        }

//        ///// <summary>
//        ///// 查找并且创建
//        ///// </summary>
//        ///// <param name="parentElement"></param>
//        ///// <param name="name"></param>
//        ///// <returns></returns>
//        //private static Tuple<XmlElement, bool> FindAndCreateSubElement(XmlElement parentElement, string name)
//        //{
//        //    if (parentElement == null)
//        //    {
//        //        throw new ArgumentNullException("parentElement");
//        //    }

//        //    XmlElement element = null;
//        //    bool changed = false;

//        //    foreach (XmlNode subNode in parentElement.ChildNodes)
//        //    {
//        //        if (subNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && subNode.NodeType == XmlNodeType.Element)
//        //        {
//        //            element = (XmlElement)subNode;
//        //            break;
//        //        }
//        //    }

//        //    if (element == null)
//        //    {
//        //        element = parentElement.OwnerDocument.CreateElement(name);
//        //        parentElement.AppendChild(element);
//        //        changed = true;
//        //    }

//        //    return new Tuple<XmlElement, bool>(element, changed);
//        //}

//        ///// <summary>
//        ///// 批量创建sub element
//        ///// </summary>
//        ///// <param name="parentElement"></param>
//        ///// <param name="names"></param>
//        ///// <returns></returns>
//        //private static Tuple<Dictionary<string, XmlElement>, bool> FindAndCreateSubElements(XmlElement parentElement, params string[] names)
//        //{
//        //    Dictionary<string, XmlElement> subEles = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
//        //    bool changed = false;

//        //    if (parentElement == null)
//        //    {
//        //        throw new ArgumentNullException("parentElement");
//        //    }
//        //    if (names == null || names.Length == 0)
//        //    {
//        //        throw new ArgumentNullException("names");
//        //    }

//        //    foreach (string name in names)
//        //    {
//        //        subEles[name] = null;
//        //    }

//        //    foreach (XmlNode subNode in parentElement.ChildNodes)
//        //    {
//        //        if (subNode.NodeType == XmlNodeType.Element && subEles.ContainsKey(subNode.Name))
//        //        {
//        //            subEles[subNode.Name] = (XmlElement)subNode;
//        //        }
//        //    }

//        //    foreach (string name in names)
//        //    {
//        //        if (subEles[name] == null)
//        //        {
//        //            XmlElement tempNode = parentElement.OwnerDocument.CreateElement(name);
//        //            parentElement.AppendChild(tempNode);
//        //            subEles[name] = tempNode;
//        //            changed = true;
//        //        }
//        //    }

//        //    return new Tuple<Dictionary<string, XmlElement>, bool>(subEles, changed);
//        //}

//        //private static Tuple<XmlElement, bool> EnsureSubBinding(XmlElement parentElement, string bindingName, string bindingSchema)
//        //{
//        //    XmlElement subBinding = null;
//        //    bool changed = false;

//        //    if (parentElement == null)
//        //    {
//        //        throw new ArgumentNullException("parentElement");
//        //    }

//        //    foreach (XmlNode subNode in parentElement.ChildNodes)
//        //    {
//        //        if (subNode.NodeType == XmlNodeType.Element && subNode.Name.Equals("binding", StringComparison.Ordinal))
//        //        {
//        //            XmlElement subEle = subNode as XmlElement;
//        //            string subName = subEle.GetAttribute("name");
//        //            if ((!string.IsNullOrEmpty(subName)) && subName.Equals(bindingName, StringComparison.Ordinal))
//        //            {
//        //                subBinding = subEle;
//        //                break;
//        //            }
//        //        }
//        //    }

//        //    if (subBinding == null)
//        //    {
//        //        //subBinding = parentElement.OwnerDocument.CreateElement("binding");
//        //        parentElement.InnerXml += bindingSchema;
//        //        changed = true;
//        //    }

//        //    return new Tuple<XmlElement, bool>(subBinding, changed);
//        //}

//        ///// <summary>
//        ///// 获取默认的Binding
//        ///// </summary>
//        ///// <param name="configurationFile"></param>
//        ///// <param name="bindingName"></param>
//        ///// <returns></returns>
//        //private static Binding GetBindingFromConfigurationFile(string configurationFile, string bindingType, string bindingName, string uriSchema)
//        //{
//        //    Binding binding = null;

//        //    try
//        //    {
//        //        Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
//        //            new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = configurationFile },
//        //            System.Configuration.ConfigurationUserLevel.None);

//        //        BindingsSection bindingSection = configuration.GetSection("system.serviceModel/bindings") as BindingsSection;

//        //        if (bindingSection != null)
//        //        {
//        //            binding = ApplyConfiguration(bindingType, bindingName, bindingSection);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        logger.Error("Cannot get the binding from configuration file:{0} with configuration name:{1}, because there is an exception:{2}.", configurationFile, bindingName, ex.ToString());
//        //        binding = GetDefaultBinding(uriSchema);
//        //    }


//        //    return binding;
//        //}

//        //internal static Binding ApplyConfiguration(string bindingType, string bindingName, BindingsSection section)
//        //{
//        //    if (string.IsNullOrEmpty(bindingType))
//        //    {
//        //        throw new ArgumentNullException("bindingName null Exception");
//        //    }

//        //    Binding binding = null;
//        //    switch (bindingType)
//        //    {
//        //        case "netTcpBinding":
//        //            {
//        //                binding = new NetTcpBinding();
//        //                section.NetTcpBinding.Bindings[bindingName].ApplyConfiguration(binding);
//        //            } break;
//        //        case "basicHttpBinding":
//        //            {
//        //                binding = new BasicHttpBinding();
//        //                section.BasicHttpBinding.Bindings[bindingName].ApplyConfiguration(binding);
//        //            } break;
//        //        case "wsDualHttpBinding":
//        //            {
//        //                binding = new WSDualHttpBinding();
//        //                section.WSDualHttpBinding.Bindings[bindingName].ApplyConfiguration(binding);
//        //            } break;
//        //        case "customBinding":
//        //            {
//        //                binding = new CustomBinding();
//        //                section.CustomBinding.Bindings[bindingName].ApplyConfiguration(binding);
//        //            } break;
//        //        default:
//        //            {
//        //                throw new Exception(string.Format("Not Supported. {0}", bindingType));
//        //            }
//        //    }

//        //    return binding;
//        //}

//        //internal static Binding GetDefaultBinding(string uriSchema)
//        //{
//        //    Binding binding = null;
//        //    if ((!string.IsNullOrEmpty(uriSchema)) && uriSchema.Equals("http", StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        BasicHttpBinding basicHttpBind = new BasicHttpBinding();
//        //        basicHttpBind.Security.Mode = BasicHttpSecurityMode.None;
//        //        basicHttpBind.TransferMode = TransferMode.Buffered;
//        //        basicHttpBind.MaxBufferSize = 536870912;
//        //        basicHttpBind.MaxReceivedMessageSize = 536870912;
//        //        basicHttpBind.ReaderQuotas.MaxStringContentLength = 536870912;
//        //        basicHttpBind.ReaderQuotas.MaxArrayLength = 536870912;

//        //        binding = basicHttpBind;
//        //    }
//        //    else// default is NetTcp
//        //    {
//        //        NetTcpBinding tempBinding = new NetTcpBinding();
//        //        tempBinding.Security.Mode = SecurityMode.None;
//        //        tempBinding.PortSharingEnabled = true;
//        //        tempBinding.TransferMode = TransferMode.Buffered;
//        //        tempBinding.MaxBufferSize = 536870912;
//        //        tempBinding.MaxReceivedMessageSize = 536870912;
//        //        tempBinding.ReaderQuotas.MaxStringContentLength = 536870912;
//        //        tempBinding.ReaderQuotas.MaxArrayLength = 536870912;
//        //        binding = tempBinding;
//        //    }

//        //    return binding;
//        //}

//        ///// <summary>
//        ///// Get attribute and auto created the attribute if it does not exist.
//        ///// </summary>
//        ///// <param name="element"></param>
//        ///// <param name="name"></param>
//        ///// <param name="defaultValue"></param>
//        ///// <param name="configurationFileChanged"></param>
//        ///// <returns></returns>
//        //private static string GetAttributeFromXmlElement(XmlElement element, string name, string defaultValue, ref bool configurationFileChanged)
//        //{
//        //    string result = defaultValue;

//        //    if (element.HasAttribute(name))
//        //    {
//        //        result = element.GetAttribute(name);
//        //    }
//        //    else
//        //    {
//        //        element.SetAttribute(name, defaultValue);
//        //        configurationFileChanged = true;
//        //    }

//        //    return result;
//        //}

//        ///// <summary>
//        ///// remove special attribute
//        ///// </summary>
//        ///// <param name="element"></param>
//        ///// <param name="name"></param>
//        ///// <param name="configurationFileChanged"></param>
//        ///// <returns></returns>
//        //private static void RemoveAttributeFromXmlElement(XmlElement element, string name, ref bool configurationFileChanged)
//        //{
//        //    if (element.HasAttribute(name))
//        //    {
//        //        element.RemoveAttribute(name);
//        //        configurationFileChanged = true;
//        //    }
//        //}

//        //internal static Binding GetBindingByParameter(string bindingType, string bindingName, string uriSchema)
//        //{
//        //    return GetBindingFromConfigurationFile(configurationFile, bindingType,
//        //                       bindingName, uriSchema);
//        //}
//    }
}
