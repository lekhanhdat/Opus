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
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using AvePoint.GCommon;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility.ConfigurationFile;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Newtonsoft.Json;

namespace AvePoint.Hybrid.Utility
{
    public enum HybridAppSettingKey
    {
        SignalRServer,
        PublicIdentityServiceURL,
        PublicClientIdInIdentityService,
        CustomerAgentId,
        CustomerAuthCode,
        CustomerTenantId,
        RecordAPIServer,
        PersistThreadCount,
        Version,
        AnalyzerThreadCount,
        DiscoveryThreadCount,
        DiscoveryCacheThrottling,
        AnalyzerCacheThrottling,
        PersistCacheThrottling,
        TransferDataCount,
        FSAStubNameFormat,
        DisposalDiscoveryThreadCount,
        DisposalAnalyzerThreadCount,
        DisposalPersistThreadCount,
        //IsMultiGeoMainDC,
        //CurrentDC,
        FailedItemThrottling,
        #region For JPMC only
        DiscoveryAndAnalyzeWorkerCount,
        PersistAndReportWorkerCount,
        WorkerTransferDataCount,
        #endregion
    }

    public static class CommonConfiguration
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(CommonConfiguration));

        private static Dictionary<HybridAppSettingKey, string> _appSettings = new Dictionary<HybridAppSettingKey, string>();

        private static X509Certificate2 appCert { set; get; }

        public static AveLogger Logger => logger;

        private static bool _inUpgradingProcess = false;

        public static bool InUpgradingProcess => _inUpgradingProcess;

        public static X509Certificate2 getAppCert()
        {
            return appCert;
        }

        public static void SetAppCert(X509Certificate2 cert)
        {
            appCert = cert;
        }

        public static string getConfig(HybridAppSettingKey key)
        {
            return _appSettings[key];
        }

        public static void InitAppSetting()
        {

            try
            {
                //installation envrioment, read configuration from registry.
                if (!string.IsNullOrEmpty(AgentConfigurationFileHelper.ReadConfigBase64String()))
                {
                    Logger.Info("Init configuration from registry.");
                    AgentConfigurtion config = AgentConfigurationFileHelper.ReadFromRegistry();

                    Logger.Info($"Init configuration for [{config.CurrentDC}]");
                    _appSettings.AddOrReplace(HybridAppSettingKey.SignalRServer, config.SiginalRServiceUrl);
                    _appSettings.AddOrReplace(HybridAppSettingKey.PublicIdentityServiceURL, config.IdentityServiceUrl);
                    _appSettings.AddOrReplace(HybridAppSettingKey.PublicClientIdInIdentityService, config.ClientId);
                    _appSettings.AddOrReplace(HybridAppSettingKey.CustomerAgentId, config.Id);
                    _appSettings.AddOrReplace(HybridAppSettingKey.CustomerAuthCode, config.AuthCode);
                    _appSettings.AddOrReplace(HybridAppSettingKey.CustomerTenantId, config.CustomerId);
                    _appSettings.AddOrReplace(HybridAppSettingKey.RecordAPIServer, config.RecordsApiUrl);
                    _appSettings.AddOrReplace(HybridAppSettingKey.Version, config.Version);

                    appCert = new X509Certificate2(Convert.FromBase64String(config.CertificateContent), config.CertificatePWD);
                    InitLocalAppSetting();
                    foreach (HybridAppSettingKey key in Enum.GetValues(typeof(HybridAppSettingKey)))
                    {
                        if (key != HybridAppSettingKey.CustomerAuthCode)
                            Logger.Info($"update key:{key}, value:{getConfig(key)}");
                    }

                    Logger.Info("Finish to init configuration from registry.");

                    return;
                }
                AgentConfigurtion tempconfig = AgentConfigurationFileHelper.ReadFromLocalPath(@"", "");
                // init from develoment enviroment
                _appSettings.AddOrReplace(HybridAppSettingKey.SignalRServer, "");
                _appSettings.AddOrReplace(HybridAppSettingKey.PublicIdentityServiceURL, tempconfig.IdentityServiceUrl);
                _appSettings.AddOrReplace(HybridAppSettingKey.PublicClientIdInIdentityService, tempconfig.ClientId);
                _appSettings.AddOrReplace(HybridAppSettingKey.CustomerAgentId, "");
                _appSettings.AddOrReplace(HybridAppSettingKey.CustomerAuthCode, tempconfig.AuthCode);  //should be the value of field 'AuthCode' of table RMAgents
                _appSettings.AddOrReplace(HybridAppSettingKey.CustomerTenantId, "");
                _appSettings.AddOrReplace(HybridAppSettingKey.RecordAPIServer, "");
                InitLocalAppSetting();
                _appSettings.AddOrReplace(HybridAppSettingKey.Version, "15.1.0.10");
                //appCert = new X509Certificate2(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\config\recotest.pfx", "1qaz2wsxE");
                appCert = new X509Certificate2(Convert.FromBase64String(tempconfig.CertificateContent), tempconfig.CertificatePWD);
                Logger.Info($"Finish to init cert for development env.");

            }
            catch (Exception ex)
            {
                Logger.Error($"init app config error:{ex.ToString()}");
            }

        }

        private static void InitLocalAppSetting()
        {
            _appSettings.AddOrReplace(HybridAppSettingKey.DiscoveryThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.DiscoveryThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.PersistThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.PersistThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.AnalyzerThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.AnalyzerThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.DisposalDiscoveryThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.DisposalDiscoveryThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.DisposalPersistThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.DisposalPersistThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.DisposalAnalyzerThreadCount, ConfigurationManager.AppSettings[HybridAppSettingKey.DisposalAnalyzerThreadCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.DiscoveryCacheThrottling, ConfigurationManager.AppSettings[HybridAppSettingKey.DiscoveryCacheThrottling.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.PersistCacheThrottling, ConfigurationManager.AppSettings[HybridAppSettingKey.PersistCacheThrottling.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.AnalyzerCacheThrottling, ConfigurationManager.AppSettings[HybridAppSettingKey.AnalyzerCacheThrottling.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.TransferDataCount, ConfigurationManager.AppSettings[HybridAppSettingKey.TransferDataCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.FSAStubNameFormat, ConfigurationManager.AppSettings[HybridAppSettingKey.FSAStubNameFormat.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.FailedItemThrottling, ConfigurationManager.AppSettings[HybridAppSettingKey.FailedItemThrottling.ToString()]);
            #region JPMC only
            _appSettings.AddOrReplace(HybridAppSettingKey.DiscoveryAndAnalyzeWorkerCount, ConfigurationManager.AppSettings[HybridAppSettingKey.DiscoveryAndAnalyzeWorkerCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.PersistAndReportWorkerCount, ConfigurationManager.AppSettings[HybridAppSettingKey.PersistAndReportWorkerCount.ToString()]);
            _appSettings.AddOrReplace(HybridAppSettingKey.WorkerTransferDataCount, ConfigurationManager.AppSettings[HybridAppSettingKey.WorkerTransferDataCount.ToString()]);
            #endregion
        }

        public static void SetInUpgradingProcess(bool inUpgrading)
        {
            _inUpgradingProcess = inUpgrading;
        }
    }
}
