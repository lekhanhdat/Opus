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
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.Server.ControlPanel;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography;
    using System.Xml;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
    using AvePoint.GCommon.MicroKernel.MicroKernelIntentionImpl;
    using AvePoint.GCommon.Utility.Cloud;
    using AvePoint.GCommon.JobManagement;

    #endregion

    public class AveStaticEnv
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Setup()
        {
            Tuple<byte[], int> encryptionKeys = AgentCacheManager.GetCachedRegisterResult();
            if (encryptionKeys != null)
            {
                CspCommunicationWrapper.CommunicationEncryptionKey = encryptionKeys.Item1;
                DefaultAuthInterseption.AuthorizationToken = CspCommunicationWrapper.AuthToken;
                CryptographyManagement.CryptoMode = (CryptoMode)encryptionKeys.Item2;
            }

            if (GCommonRoleConfiguration.Instance is DictRoleConfiguration)
            {
                SetupRoleConfiguration();
            }
            if (GCommonRoleConfiguration.IsContainerEnvironment && encryptionKeys == null)
            {
                logger.Info("Get encryption info from JobManagement.");
                var jobManagement = new JobManagement();
                var encryptionKey = jobManagement.GetCommunicationEncryptionKey();
                CspCommunicationWrapper.CommunicationEncryptionKey = encryptionKey;
                DefaultAuthInterseption.AuthorizationToken = CspCommunicationWrapper.AuthToken;
                CryptographyManagement.CryptoMode = CryptoMode.NoneFIPS;
            }
        }

        public static void SetupRoleConfiguration()
        {
            string configFilePath = "";
            if (GCommonRoleConfiguration.IsContainerEnvironment)
            {
                configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonEnv.config");
            }
            else
            {
                configFilePath = Path.Combine(AveEnv.AgentBinFolder, "CommonEnv.config");
            }
            GCommonRoleConfiguration.Instance = new ConfigFileRoleConfiguration(configFilePath);

            // Init certificates in RMAppConfiguration
            //if (string.IsNullOrEmpty(GCommonRoleConfiguration.KeyVaultCertThumbprint))
            //{
            //    logger.Info("current enviroment is development. dont need init certificate.");
            //    return;
            //}
            //GCommonRoleConfiguration.InitCertificate();
        }

        public static void SetupAgentServiceConfiguration()
        {
            if (!GCommonRoleConfiguration.IsContainerEnvironment)
            {
                return;
            }
            var containerConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppConfig");
            containerConfigFile = Path.Combine(containerConfigFile, "CommonEnv.config");
            if (File.Exists(containerConfigFile))
            {
                logger.Info("Container configuration file exist.Path:{0}", containerConfigFile);
                GCommonRoleConfiguration.Instance = new ConfigFileRoleConfiguration(containerConfigFile);
                var appsetting = GCommonRoleConfiguration.GetConfigurationFromKeyVault(true);
                GCommonRoleConfiguration.UpdateInstance(GCommonRoleConfiguration.ConvertToRoleConfiguration(appsetting));
                ConfigFileRoleConfiguration config = new ConfigFileRoleConfiguration(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonEnv.config"));
                GCommonRoleConfiguration.CopyTo(config);
                config.Update();
            }
            else
            {
                logger.Error("Container configuration file not exist.Path:{0}", containerConfigFile);
            }

        }

        public static void SetupAgentServiceForLocal()
        {
            var configFilePath = Path.Combine(AveEnv.AgentBinFolder, "CommonEnv.config");
            if (File.Exists(configFilePath))
            {
                logger.Info("Container configuration file exist.Path:{0}", configFilePath);
                GCommonRoleConfiguration.Instance = new ConfigFileRoleConfiguration(configFilePath);
                var appsetting = GCommonRoleConfiguration.GetConfigurationFromKeyVault(true);
                GCommonRoleConfiguration.UpdateInstance(GCommonRoleConfiguration.ConvertToRoleConfiguration(appsetting));

                ConfigFileRoleConfiguration config = new ConfigFileRoleConfiguration(configFilePath);
                GCommonRoleConfiguration.CopyTo(config);
                config.Update();
            }
            else
            {
                logger.Error("Container configuration file not exist.Path:{0}", configFilePath);
            }

        }



        public static void MonitorAgentBinFolder()
        {
            AveFileSystemWatcher fsWatcher = new AveFileSystemWatcher(AveEnv.AgentHotfixFolder, "bin.txt");
            fsWatcher.Changed += SyncConfiguration;
            fsWatcher.Deleted += SyncConfiguration;
        }

        private static void SyncConfiguration(object sender, FileSystemEventArgs e)
        {
            var lastBinFolder = AveEnv.AgentBinFolder;

            var timeout = DateTime.Now.AddSeconds(60);
            while (timeout > DateTime.Now)
            {
                AveEnv.SetAgentBinFolder();
                if (string.Compare(AveEnv.AgentBinFolder, lastBinFolder, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    logger.Info("The hotfix was applied to {0}, the last bin was:{1}", AveEnv.AgentBinFolder, lastBinFolder);
                    break;
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }
    }

    //public class AgentEnvironment
    //{
    //    public static EnvMode EnvMode
    //    {
    //        get
    //        {
    //            return GCommonRoleConfiguration.IsRoleEnvironment ? EnvMode.Azure : EnvMode.Dev;
    //        }
    //    }

    //    public static bool IsRealtimeService { get; set; }

    //    AgentEnvironment()
    //    {
    //        IsRealtimeService = false;
    //    }
    //}

    //public enum EnvMode
    //{
    //    Azure, 
    //    Dev
    //}
}

