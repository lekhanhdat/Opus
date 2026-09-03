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
namespace AvePoint.GCommon.Utility.Cloud
{
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Utility.Config;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon.Utility.TransientFault;
    using k8s;
    // using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using Microsoft.Data.SqlClient;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// 请不要引用Azure的server runtime的类！！！
    /// </summary>
    public class GCommonRoleConfiguration
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(GCommonRoleConfiguration));



        private static IRoleConfiguration TemporaryInstance = new DictRoleConfiguration();

        public static IRoleConfiguration Instance = new DictRoleConfiguration();

        #region Config

        #region Job

        public static string JobQueueName { get { return Instance.GetValue(ConfigKey.JobQueueName, false); } }

        public static Dictionary<string, string> JobQueueNameMapping
        {
            get
            {
                if (string.IsNullOrEmpty(Instance[ConfigKey.JobQueueNameMapping]))
                {
                    return new Dictionary<string, string>();
                }
                else
                {
                    return SerializerHelper.DeserializeByJsonConvert<Dictionary<string, string>>(Instance[ConfigKey.JobQueueNameMapping]);
                }
            }
        }

        public static string JobQueueConnectionString { get { return Instance[ConfigKey.JobQueueConnectionString]; } }

        public static string JobContextStorageXri { get { return GetXriString(ConfigKey.JobContextStorageXri); } }

        public static string JobReportStorageXri { get { return GetXriString(ConfigKey.JobReportStorageXri); } }
        #endregion

        #region Config Database
        public static string ConfigDatabaseInstance { get { return GetConfigDBInfo(ConfigKey.ConfigDatabaseInstance); } }

        public static string ConfigDatabaseName { get { return GetConfigDBInfo(ConfigKey.ConfigDatabaseName); } }

        public static string ConfigDatabaseUserName { get { return GetConfigDBInfo(ConfigKey.ConfigDatabaseUsername); } }

        public static string ConfigDatabasePassword { get { return GetConfigDBInfo(ConfigKey.ConfigDatabasePassword); } }

        public static string DBManagerUsername { get { return Instance[ConfigKey.DBManagerUsername]; } }

        public static string DBManagerPassword { get { return AnalysePwd(Instance[ConfigKey.DBManagerPassword]); } }


        public static string ElasticPoolName { get { return Instance[ConfigKey.ElasticPoolName]; } }

        public static string ConfigIMCacheDBName { get { return Instance[ConfigKey.IMCacheDBName]; } }

        /// <summary>
        /// 当前DocAve所在的Azure Region Name
        /// </summary>
        public static string AzureRegion { get { return Instance[ConfigKey.AzureRegion]; } }

        public static string WcfAgentHost { get { return Instance.GetValue(ConfigKey.WcfAgentHost, false); } }

        public static string TelemetryStorageSAS
        {
            get
            {
                var sas = Instance.GetValue(ConfigKey.TelemetryStorageSAS, false);
                if (!string.IsNullOrEmpty(sas))
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(sas));
                }
                return null;
            }
        }

        #endregion

        public static string HotfixStorageUrl { get { return GetXriString(ConfigKey.HotfixStorageUrl); } }

        public static string JobLogStorageXri { get { return ParseAgentStorageXri(ConfigKey.JobLogStorageXri); } }
        public static string ReportStorageXri { get { return ParseAgentStorageXri(ConfigKey.ReportStorageXri); } }

        public static string AgentStorageXri { get { return GetXriString(ConfigKey.AgentStorageXri); } }
        private const string containerName = "containerName";


        public static string SbConnectionInfo { get { return Instance[ConfigKey.SbConnectionInfo]; } }
        public static string IsStaging { get { return Instance[ConfigKey.IsStaging]; } }
        public static string PageViewStorageConnString { get { return Instance[ConfigKey.PageViewStorageConnString]; } }

        /*
         * xx&containerName=c1;c2&xx=xxx
         */
        private static string ParseAgentStorageXri(string key)
        {
            string result = null;
            string plainTxt = AgentStorageXri;
            string[] tmpArray = plainTxt.Split('&');

            string[] containers = null;
            string allContainer = null;
            for (int i = 0; i < tmpArray.Length; i++)
            {
                if (tmpArray[i].StartsWith(containerName))
                {
                    allContainer = tmpArray[i].Split('=')[1];
                    containers = allContainer.Split(';');
                    break;
                }
            }

            if ((allContainer == null) || (containers == null))
            {
                throw new Exception("container is null.");
            }

            for (int i = 0; i < containers.Length; i++)
            {
                result = plainTxt.Replace(allContainer, containers[i]);
                if (containers[i].ToLower().Contains("log") && key.ToLower().Contains("log"))
                {
                    break;
                }
                else if (containers[i].ToLower().Contains("report") && key.ToLower().Contains("report"))
                {
                    break;
                }
            }

            return result;
        }

        #region Environment

        public static int LocalResourceMaximumSize { get { return int.Parse(Instance[ConfigKey.RoleLocalResourceMaximumSize]); } }

        #endregion

        #region App

        public static string AppCertFile { get { return Instance[ConfigKey.AppCertFile]; } }
        public static string AppCertSecret { get { return Instance[ConfigKey.AppCertSecret]; } }
        public static string AppClientId { get { return Instance[ConfigKey.Office365ClientId]; } }
        public static string PortalApiURL { get { return Instance[ConfigKey.PortalApiURL]; } }
        public static string ModernPortalApiURL { get { return Instance[ConfigKey.ModernPortalApiURL]; } }
        public static string COP_API_URL { get { return Instance[ConfigKey.COP_API_URL]; } }
        public static string INSIGHTS_ENGINE_API_URL { get { return Instance[ConfigKey.INSIGHTS_ENGINE_API_URL]; } }
        public static string EDISCOVERY_API_URL { get { return Instance[ConfigKey.EDISCOVERY_API_URL]; } }
        public static string MYHUB_API_URL { get { return Instance[ConfigKey.MYHUB_API_URL]; } }
        public static string AOSP_API_URL { get { return Instance[ConfigKey.AOSP_API_URL]; } }
        public static string NEXUS_FOUNDATION_API_URL { get { return Instance[ConfigKey.GCONTROL_PLATFORM_API]; } }
        public static string NEXUS_GOVERNANCE_API_URL { get { return Instance[ConfigKey.NEXUS_GOVERNANCE_API]; } }
        public static string GCONTROL_MYHUB_TASK_URL { get { return Instance[ConfigKey.GCONTROL_MYHUB_TASK]; } }
        public static string DAL_GATEWAY_API_URL { get { return Instance[ConfigKey.DAL_GATEWAY_API_URL]; } }
        public static string ICS_API_URL
        {
            get
            {
                Instance.TryGetVaule(ConfigKey.ICS_API_URL, out string value);
                return value;
            }
        }

        public static string GetClientId(AppType appType)
        {
            switch (appType)
            {
                case AppType.SharePoint:
                    return Instance[ConfigKey.SharePointClientId];
                case AppType.Exchange:
                    return Instance[ConfigKey.ExchangeClientId];
                case AppType.Office365:
                default:
                    return Instance[ConfigKey.Office365ClientId];
            }
        }
        #endregion


#if DEBUG
        public static string AosCustomerId { get { return Instance[ConfigKey.AosCustomerId]; } }
        /// <summary>
        /// 用于开发环境中不关联AOS情况下，开发指定TenantId
        /// </summary>
        public static string Office365TenantIdForDev { get { return Instance[ConfigKey.Office365TenantIdForDev]; } }
        public static bool IsDevelopmentEnviorment
        {
            get
            {
                return true;
            }
        }
#endif

        public static string SimpleLogin { get { return Instance[ConfigKey.SimpleLogin]; } }

        public static int AgentCoreServicePort { get { return int.Parse(Instance[ConfigKey.AgentCoreServicePort]); } }

        public static string RoleId { get { return Instance.GetValue(ConfigKey.RoleId, false); } }

        public static string DeploymentId { get { return Instance.GetValue(ConfigKey.DeploymentId, false); } }

        public static string ControlServiceAddress { get { return Instance.GetValue(ConfigKey.ControlServiceAddress, false); } }

        //public static bool InsiderEnvironment { get { return bool.Parse(Instance.GetValue(ConfigKey.InsiderEnvironment, false)); } }

        public static string KeyVaultCertThumbprint { get { return Instance.GetValue(ConfigKey.KeyVaultCertThumbprint, false); } }

        public static string KeyVaultUrl { get { return Instance.GetValue(ConfigKey.KeyVaultUrl, false); } }

        public static string KeyVaultClientId { get { return Instance.GetValue(ConfigKey.KeyVaultClientId, false); } }
        public static string AosTokenApiURL { get { return Instance.GetValue(ConfigKey.AosTokenApiURL, false); } }
        #region Identity Server
        public static bool UseIdentityServer
        {
            get
            {
                try
                {
                    return bool.Parse(Instance.GetValue(ConfigKey.UseIdentityServer, false));
                }
                catch (Exception ex)
                {
                    logger.Error("Get user identity server error, exception:{0}.", ex.ToString());
                    return false;
                }
            }
        }
        public static string IdentityServerAddress { get { return Instance.GetValue(ConfigKey.IdentityServerAddress, false); } }
        public static string IdentityServerClientId { get { return Instance.GetValue(ConfigKey.IdentityServerClientId, false); } }
        public static string IdentityServerResource { get { return Instance.GetValue(ConfigKey.IdentityServerResource, false); } }
        public static string IdentityServerIssuers { get { return Instance.GetValue(ConfigKey.IdentityServerIssuers, false); } }

        #endregion

        
        public static string PortalTokenApiInternalURL { get { return Instance.GetValue(ConfigKey.PortalTokenApiInternalURL, false); } }

        public static bool EnableTokenService
        {
            get
            {
                try
                {
                    var value = bool.Parse(Instance.GetValue(ConfigKey.EnableTokenService, false));
                    logger.Info($"Current environment's token service is enabled:{value}");
                    return value;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occured when get EnableTokenService due to {0}", ex);
                    return false;
                }
            }
        }
        public static string PortalCloudInsightsApiURL { get { return Instance.GetValue(ConfigKey.PortalCloudInsightsApiURL, false); } }

        public static bool InsiderEnvironment
        {
            get
            {
                try
                {
                    return bool.Parse(Instance.GetValue(ConfigKey.InsiderEnvironment, false));
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private static ApplicationConfiguration _AppConfig = null;
        public static string DAOAppSettings
        {
            set
            {
                try
                {
                    _AppConfig = JsonConvert.DeserializeObject<ApplicationConfiguration>(value);
                }
                catch (Exception ex)
                {
                    logger.Error($"DAO AppSettings incorrect. Content: {value}. {ex}");
                }
            }
        }

        #region 缓存证书文件
        private static X509Certificate2 _DAO_Certificate;
        public static X509Certificate2 DAO_Certificate
        {
            get
            {
                if (_DAO_Certificate != null)
                {
                    return _DAO_Certificate;
                }
                //else if (!string.IsNullOrEmpty(KeyVaultUrl)
                //    && !string.IsNullOrEmpty(KeyVaultCertThumbprint)
                //    && !string.IsNullOrEmpty(KeyVaultClientId))
                //{
                //    InitCertificate();
                //    return _DAO_Certificate;
                //}
                else
                {//for dev
                    //_DAO_Certificate = Get509cert(StoreLocation.LocalMachine, "8D34BFD00CECC8D19D0D1E3CDE7AC004B48FE010");//insider
                    _DAO_Certificate = Get509cert(StoreLocation.LocalMachine, "DC895545B9271DDA56ADF6E7AC82E6CFDB457850"); //market
                    return _DAO_Certificate;
                }
            }
            set
            {
                _DAO_Certificate = value;
            }
        }
        private static X509Certificate2 _RECO_Certificate;
        public static X509Certificate2 RECO_Certificate
        {
            get
            {
                if (_RECO_Certificate != null)
                {
                    return _RECO_Certificate;
                }
                else
                {//for dev
                    //_DAO_Certificate = Get509cert(StoreLocation.LocalMachine, "8D34BFD00CECC8D19D0D1E3CDE7AC004B48FE010");//insider
                    _RECO_Certificate = Get509cert(StoreLocation.LocalMachine, "DC895545B9271DDA56ADF6E7AC82E6CFDB457850"); //market
                    return _RECO_Certificate;
                }
            }
            set
            {
                _RECO_Certificate = value;
            }
        }
        private static X509Certificate2 _Encrypt_Certificate;
        public static X509Certificate2 Encrypt_Certificate
        {
            get
            {
                if (_Encrypt_Certificate != null)
                {
                    return _Encrypt_Certificate;
                }
                //else if (!string.IsNullOrEmpty(KeyVaultUrl)
                //    && !string.IsNullOrEmpty(KeyVaultCertThumbprint)
                //    && !string.IsNullOrEmpty(KeyVaultClientId))
                //{
                //    InitCertificate();
                //    return _Encrypt_Certificate;
                //}
                else
                {//for dev
                    //_Encrypt_Certificate = Get509cert(StoreLocation.LocalMachine, "B99F3DB462B1DF6D64724A1DA23375C82A21B789"); //insider
                    _Encrypt_Certificate = Get509cert(StoreLocation.LocalMachine, "92CEDE13F23E4A59B16BDD1C144F64A50F737DDD"); //market
                    return _Encrypt_Certificate;
                }
            }
            set
            {
                _Encrypt_Certificate = value;
            }
        }


        private enum Certificate
        {
            AOS,
            AVE,
            DAO,
            GAO,
            WCF,
            WebApp
        }
        public const string CERTIFICATE_IDENTIFIER_DAO = "docaveonline";
        public const string CERTIFICATE_IDENTIFIER_AVE = "AVEEncrypt";
        public const string CERTIFICATE_IDENTIFIER_WCF = "WCFcertificate";
        public const string CERTIFICATE_IDENTIFIER_WEBAPP = "WebApplication";
        #endregion

#endregion

        #region Update

        private static string GetConfigDBInfo(string key)
        {
            string value;

            if (!TemporaryInstance.TryGetVaule(key, out value))
            {
                UpdateConfigDB();
                value = TemporaryInstance[key];
            }

            return value;
        }

        private static string GetFromBase64(string key)
        {
            string value;
            string decoderKey = string.Concat(key, "_decoder");
            if (!TemporaryInstance.TryGetVaule(decoderKey, out value))
            {
                value = ConvertFormBase64(Instance[key]);
                TemporaryInstance[decoderKey] = value;
            }

            return value;
        }

        private static void UpdateConfigDB()
        {
            var str = Instance[ConfigKey.ConfigDatabaseConnection];
            try
            {
                var connection = new SqlConnectionStringBuilder(str);
                TemporaryInstance[ConfigKey.ConfigDatabaseInstance] = connection.DataSource;
                TemporaryInstance[ConfigKey.ConfigDatabaseName] = connection.InitialCatalog;
                TemporaryInstance[ConfigKey.ConfigDatabaseUsername] = connection.UserID;
                TemporaryInstance[ConfigKey.ConfigDatabasePassword] = AnalysePwd(connection.Password);
            }
            catch (Exception ex)
            {
                logger.Error("Analyze connection string failed. " + ex);
            }
        }

        public static void UpdateInstance(IRoleConfiguration newRoleConfiguration)
        {
            Instance = newRoleConfiguration;
            TemporaryInstance = new DictRoleConfiguration();
        }

        public static void Update(IRoleConfiguration newRoleConfiguration)
        {
            CopyTo(newRoleConfiguration, Instance);
            TemporaryInstance = new DictRoleConfiguration();
        }

        public static void CopyTo(IRoleConfiguration targetRoleConfiguration)
        {
            CopyTo(Instance, targetRoleConfiguration);
        }

        public static void CopyTo(IRoleConfiguration sourceRoleConfiguration, IRoleConfiguration targetRoleConfiguration)
        {
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.JobQueueConnectionString);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.JobContextStorageXri);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.JobReportStorageXri);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AgentStorageXri);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.HotfixStorageUrl);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.SbConnectionInfo);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.IsStaging);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.ConfigDatabaseConnection);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.ElasticPoolName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.IMCacheDBName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.DBManagerUsername);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.DBManagerPassword);

            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.JobQueueName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.JobQueueNameMapping);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PageViewStorageConnString);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AppCertFile);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AppCertSecret);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.Office365ClientId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.SharePointClientId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.ExchangeClientId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PortalApiURL);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.ModernPortalApiURL);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PortalURL);
#if DEBUG
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AosCustomerId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.Office365TenantIdForDev);
#endif
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.RedisCacheSettings);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.SimpleLogin);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AzureRegion);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AgentCoreServicePort);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.RoleId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.DeploymentId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.RoleLocalResourceName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.RoleLocalResourceMaximumSize);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.IsRoleEnvironment);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.WcfAgentHost);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.TelemetryStorageSAS);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.ControlServiceAddress);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.InsiderEnvironment);

            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PortalTopicConnectionString);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PortalTopicName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.PortalSubscriptionName);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.IsProductEnvironment);

            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.KeyVaultClientId);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.KeyVaultUrl);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.KeyVaultCertThumbprint);
            CopyTo(sourceRoleConfiguration, targetRoleConfiguration, ConfigKey.AosTokenApiURL);
        }

        private static bool CopyTo(IRoleConfiguration sourceRoleConfiguration, IRoleConfiguration targetRoleConfiguration, string key)
        {
            var modify = false;
            try
            {
                string sourceValue;

                if (sourceRoleConfiguration.TryGetVaule(key, out sourceValue))
                {
                    string value;
                    if ((!targetRoleConfiguration.TryGetVaule(key, out value)) || (!string.Equals(sourceValue, value, StringComparison.Ordinal)))
                    {
                        logger.Info("Copy setting: {0}, value: {1}.", key, string.Equals(key, ConfigKey.ConfigDatabasePassword, StringComparison.Ordinal) ? "***********" : sourceValue);
                        targetRoleConfiguration[key] = sourceValue;
                        modify = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Copy value failed, key: {0}, error: {1}.", key, ex);
            }
            return modify;
        }

        private static string AnalysePwd(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return password;
            }
            string plainTextPwd = null;

            Regex regex = new Regex("^(plaintext|pt){(.*)}$");
            Match match = regex.Match(password);
            if (match.Success)
            {
                plainTextPwd = match.Groups[2].Value;
            }
            else
            {
                var encryptor = new SecurityKeyEncryptor(CertificateHelper.DocAveOnlineCertificate);
                plainTextPwd = encryptor.Decrypt(password);
            }
            return plainTextPwd;
        }

        private static string ConvertFormBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return base64String;
            }
            logger.Info(base64String);
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }
        #endregion

        #region ConfigKeys
        public class ConfigKey
        {
            //job
            public const string JobQueueConnectionString = "JobQueueConnectionString";
            public const string JobContextStorageXri = "JobContextStorageXri";
            public const string JobReportStorageXri = "JobReportStorageXri";
            //config db
            public const string ConfigDatabaseInstance = "ConfigDatabaseInstance";
            public const string ConfigDatabaseName = "ConfigDatabaseName";
            public const string ConfigDatabaseUsername = "ConfigDatabaseUsername";
            public const string ConfigDatabasePassword = "ConfigDatabasePassword";
            public const string ConfigDatabaseConnection = "ConfigDatabaseConnection";
            public const string RoleLocalResourceName = "RoleLocalResource";
            public const string RoleLocalResourceMaximumSize = "RoleLocalResourceMaximumSize";
            public const string ElasticPoolName = "ElasticPoolName";
            public const string IMCacheDBName = "IMCacheDBName";
            public const string DBManagerUsername = "DBManagerUsername";
            public const string DBManagerPassword = "DBManagerPassword";
            //dashboard
            public const string JobQueueName = "JobQueueName";
            public const string JobQueueNameMapping = "JobQueueNameMapping";
            //public const string CustomStorageXri = "CustomStorageXri";
            public const string HotfixStorageUrl = "HotfixStorageXri";
            public const string JobLogStorageXri = "JobLogStorageXri";
            public const string ReportStorageXri = "ReportStorageXri";
            public const string AgentStorageXri = "AgentStorageXri";
            //Real time
            public const string SbConnectionInfo = "RealTimeConnectionString";
            public const string IsStaging = "IsStaging";
            public const string IsRoleEnvironment = "IsRoleEnvironment";
            public const string WcfAgentHost = "WcfAgentHost";
            public const string TelemetryStorageSAS = "TelemetryStorageSAS";
            public const string PageViewStorageConnString = "PageViewStorageConnString";
            public const string AppCertFile = "AppCertFile";
            public const string AppCertSecret = "AppCertSecret";
            public const string Office365ClientId = "Office365ClientId";
            public const string SharePointClientId = "SharePointClientId";
            public const string ExchangeClientId = "ExchangeClientId";
            public const string PortalApiURL = "AOS_API_URL";
            public const string ModernPortalApiURL = "AOS_MODERN_API_URL";
            public const string COP_API_URL = "COP_API_URL";
            public const string ICS_API_URL = "ICS_API_URL";
            public const string INSIGHTS_ENGINE_API_URL = "INSIGHTS_ENGINE_API_URL";
            public const string EDISCOVERY_API_URL = "EDISCOVERY_API_URL";
            public const string MYHUB_API_URL = "MYHUB_API_URL";
            public const string AOSP_API_URL = "AOSP_API_URL";
            public const string GCONTROL_PLATFORM_API = "NEXUS_FOUNDATION_API_URL";
            public const string NEXUS_GOVERNANCE_API = "NEXUS_GOVERNANCE_API_URL";
            public const string GCONTROL_MYHUB_TASK = "GCONTROL_MYHUB_TASK_URL";

#if DEBUG
            public const string AosCustomerId = "AosCustomerId";
            public const string Office365TenantIdForDev = "Office365TenantIdForDev";
#endif
            public const string RedisCacheSettings = "RedisCacheSettings";
            public const string SimpleLogin = "SimpleLogin";
            public const string AzureRegion = "AzureRegion";
            public const string AgentCoreServicePort = "AgentCoreServicePort";
            public const string RoleId = "RoleId";
            public const string DeploymentId = "DeploymentId";
            public const string ControlServiceAddress = "ControlServiceAddress";
            public const string InsiderEnvironment = "InsiderEnvironment";
            public const string IsProductEnvironment = "IsProductEnvironment";
            public const string PortalURL = "PortalURL";
            public const string StartupConfig = "StartupConfig";

            public const string PortalTopicConnectionString = "PortalTopicConnectionString";
            public const string PortalTopicName = "PortalTopicName";
            public const string PortalSubscriptionName = "PortalSubscriptionName";

            public const string KeyVaultCertThumbprint = "KeyVaultCertThumbprint";
            public const string KeyVaultUrl = "KeyVaultUrl";
            public const string KeyVaultClientId = "KeyVaultClientId";
            public const string AosTokenApiURL = "AosTokenApiURL";

            public const string UseIdentityServer = "UseIdentityServer";
            public const string IdentityServerAddress = "IdentityServerAddress";
            public const string IdentityServerClientId = "IdentityServerClientId";
            public const string IdentityServerResource = "IdentityServerResource";
            public const string IdentityServerIssuers = "IdentityServerIssuers";
            public const string PortalTokenApiURL = "PortalTokenApiURL";
            public const string EnableTokenService = "EnableTokenService";
            public const string PortalTokenApiInternalURL = "PortalTokenApiInternalURL";
            public const string PortalCloudInsightsApiURL = "PortalCloudInsightsApiURL";
            public const string RECOCertString = "RECOCertString";
            public const string DAL_GATEWAY_API_URL = "DAL_GATEWAY_API_URL";
        }
        #endregion

        #region 获取key vault上的appsettings和证书

        public const string SECRET_NAME_DAO_APPSETTING = "appsettings";

        public static IRoleConfiguration ConvertToRoleConfiguration(ApplicationConfiguration setting, bool isControlRole = false)
        {
            var result = new DictRoleConfiguration();
            result[ConfigKey.AppCertFile] = setting.AppCertFile;
            result[ConfigKey.AppCertSecret] = setting.AppCertSecret;
            result[ConfigKey.AzureRegion] = setting.AzureRegion;
            result[ConfigKey.ConfigDatabaseConnection] = setting.ConfigDatabaseConnection;
            result[ConfigKey.DBManagerPassword] = setting.DBManagerPassword;
            result[ConfigKey.DBManagerUsername] = setting.DBManagerUsername;
            result[ConfigKey.ElasticPoolName] = setting.ElasticPoolName;
            result[ConfigKey.AgentStorageXri] = GetStorageXriFromAppSetting(setting.AgentStorageXri);
            result[ConfigKey.WcfAgentHost] = setting.WcfAgentHost;
            result[ConfigKey.ExchangeClientId] = setting.ExchangeClientId;
            result[ConfigKey.IMCacheDBName] = setting.IMCacheDBName;
            result[ConfigKey.IsStaging] = setting.IsStaging;
            result[ConfigKey.JobContextStorageXri] = GetStorageXriFromAppSetting(setting.JobContextStorageXri);
            result[ConfigKey.JobQueueConnectionString] = setting.JobQueueConnectionString;
            result[ConfigKey.JobQueueNameMapping] = setting.JobQueueNameMapping;
            result[ConfigKey.JobReportStorageXri] = GetStorageXriFromAppSetting(setting.JobReportStorageXri);
            result[ConfigKey.Office365ClientId] = setting.Office365ClientId;
            result[ConfigKey.SharePointClientId] = setting.SharePointClientId;
            result[ConfigKey.SimpleLogin] = setting.SimpleLogin;
            result[ConfigKey.InsiderEnvironment] = setting.InsiderEnvironment;
            result[ConfigKey.IsProductEnvironment] = setting.IsProductEnvironment;
            result[ConfigKey.PortalURL] = setting.PortalURL;
            result[ConfigKey.TelemetryStorageSAS] = setting.TelemetryStorageSAS;
            result[ConfigKey.SbConnectionInfo] = setting.RealTimeConnectionString;
            result[ConfigKey.ControlServiceAddress] = setting.ControlServiceAddress;
            result[ConfigKey.HotfixStorageUrl] = GetStorageXriFromAppSetting(setting.HotfixStorageXri);
            result[ConfigKey.PageViewStorageConnString] = setting.PageViewStorageConnString;
            result[ConfigKey.AosTokenApiURL] = setting.AosTokenApiURL;

            //下列值都是从container或者cloudservice进行初始化到当前类中 所以转换时直接赋值
            logger.Info("current jobqueuename is {0}", JobQueueName);
            result[ConfigKey.JobQueueName] = JobQueueName;
            result[ConfigKey.KeyVaultCertThumbprint] = KeyVaultCertThumbprint;
            result[ConfigKey.KeyVaultClientId] = KeyVaultClientId;
            result[ConfigKey.KeyVaultUrl] = KeyVaultUrl;

            var isRoleEnviroment = Instance.GetValue(ConfigKey.IsRoleEnvironment, false);
            var roleId = Instance.GetValue(ConfigKey.RoleId, false);
            var roleLocalResourceMaximumSize = Instance.GetValue(ConfigKey.RoleLocalResourceMaximumSize, false);
            var roleLocalResourceName = Instance.GetValue(ConfigKey.RoleLocalResourceName, false);
            var deploymentId = Instance.GetValue(ConfigKey.DeploymentId, false);
            var agentCoreServicePort = Instance.GetValue(ConfigKey.AgentCoreServicePort, false);
            logger.Info("is role enviroment value is {0},roleId is {1},roleLocalResourceMaximumSize is {2},roleLocalResourceName is {3},deploymentId is {4},agentCoreServicePort is {5}",
                isRoleEnviroment, roleId, roleLocalResourceMaximumSize, roleLocalResourceName, deploymentId, agentCoreServicePort);
            result[ConfigKey.IsRoleEnvironment] = isRoleEnviroment;
            result[ConfigKey.RoleId] = roleId;
            result[ConfigKey.RoleLocalResourceMaximumSize] = roleLocalResourceMaximumSize;
            result[ConfigKey.RoleLocalResourceName] = roleLocalResourceName;
            result[ConfigKey.DeploymentId] = deploymentId;
            result[ConfigKey.AgentCoreServicePort] = agentCoreServicePort;
            return result;
        }

        public static string GetStorageXriFromAppSetting(StorageInfo storageInfo)
        {
            var paramArray = storageInfo.ConnectionString.Split(';');
            var paramDic = new Dictionary<string, string>();
            foreach (var param in paramArray)
            {
                if (param.StartsWith("accesspoint=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var accessPointStr = param.ToLower().Replace("accesspoint=", "");
                    paramDic.Add("AccessPoint", accessPointStr);
                }
                if (param.StartsWith("accountname=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var accountName = param.ToLower().Replace("accountname=", "");
                    paramDic.Add("AccountName", accountName);
                }
                if (param.StartsWith("AccountKey=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var accountKey = param.Replace("AccountKey=", "");
                    paramDic.Add("AccountKey", Encode(accountKey));
                }
            }
            paramDic.Add("ContainerName", storageInfo.ContainerName);
            return AssembleStorageXriString(paramDic);
        }

        private static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
        }

        private static string AssembleStorageXriString(Dictionary<string, string> paramInfoDic)
        {
            var accessPoint = GetParamValue(paramInfoDic, "AccessPoint");
            var accountName = GetParamValue(paramInfoDic, "AccountName");
            var accountKey = GetParamValue(paramInfoDic, "AccountKey");
            var containerName = GetParamValue(paramInfoDic, "ContainerName");
            return $"docave-xam://azure_vim?accessPoint={accessPoint}&containerName={containerName}&name={accountName}&secret={accountKey}";
        }

        private static string GetParamValue(Dictionary<string, string> paramInfoDic, string key)
        {
            foreach (var pair in paramInfoDic)
            {
                if (string.Equals(pair.Key, key, StringComparison.InvariantCultureIgnoreCase))
                {
                    return pair.Value;
                }
            }
            throw new Exception("Can not find the value of key :" + key);
        }

     
        #endregion

        #region 4Dev env       

        private static string GetXriString(string key)
        {
            var configString = Instance[key];
            if (string.IsNullOrEmpty(configString))
            {
                logger.Warn($"Current key {key} is not config in appsetting");
                return string.Empty;
            }
            if (configString.Contains("accessPoint") && configString.Contains("containerName") && configString.Contains("name") && configString.Contains("secret"))
            {
                return configString;
            }
            else
            {
                return GetFromBase64(key);
            }
        }

        private static X509Certificate2 Get509cert(StoreLocation location, string thumbprint)
        {
            logger.Info("start get 509 cert from location,thumbprint is {0}", thumbprint);
            X509Store x509Store = new X509Store(StoreName.My, location);
            x509Store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (x509Certificate2Collection.Count == 0)
            {
                return null;
            }
            X509Certificate2 result = x509Certificate2Collection[0];
            x509Store.Close();
            logger.Info("Get 509 cert from location finish");
            return result;
        }

        #endregion
    }

    public interface IRoleConfiguration
    {
        string this[string key] { get; set; }

        string GetValue(string key, bool throwExceptionIfNotFound);

        bool TryGetVaule(string key, out string value);

        void Update();
    }

    public class DictRoleConfiguration : IRoleConfiguration
    {
        private readonly Dictionary<string, string> configStrings;

        public DictRoleConfiguration()
        {
            configStrings = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                return GetValue(key, true);
            }

            set
            {
                lock (configStrings)
                {
                    configStrings[key] = value;
                }
            }
        }

        public string GetValue(string key, bool throwExceptionIfNotFound)
        {
            string value;
            if (!TryGetVaule(key, out value))
            {
                if (throwExceptionIfNotFound)
                {
                    throw new KeyNotFoundException("The key is not found.");
                }
            }

            return value;
        }

        public bool TryGetVaule(string key, out string value)
        {
            lock (configStrings)
            {
                return configStrings.TryGetValue(key, out value);
            }
        }

        public void Update()
        {
        }
    }

    public class ConfigFileRoleConfiguration : IRoleConfiguration
    {
        private readonly Configuration configuration;
        private KeyValueConfigurationCollection appSettings;
        private bool changed;

        public ConfigFileRoleConfiguration(string commonEnvConfig)
        {
            configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = commonEnvConfig }, ConfigurationUserLevel.None);
            appSettings = configuration.AppSettings.Settings;
        }

        public string this[string key]
        {
            get
            {
                return GetValue(key, true);
            }
            set
            {
                changed = true;
                var valueObj = appSettings[key];
                if (valueObj == null)
                {
                    appSettings.Add(key, value);
                }
                else
                {
                    valueObj.Value = value;
                }
            }
        }

        public string GetValue(string key, bool throwExceptionIfNotFound)
        {
            string value;
            if (!TryGetVaule(key, out value))
            {
                if (throwExceptionIfNotFound)
                {
                    throw new KeyNotFoundException(string.Format("The key:{0} is not found.", key));
                }
            }

            return value;
        }

        public bool TryGetVaule(string key, out string value)
        {
            value = null;
            var found = false;

            var settingValue = appSettings[key];

            if (settingValue != null)
            {
                found = true;
                value = settingValue.Value;
            }

            return found;
        }

        public void Update()
        {
            if (changed)
            {
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
                changed = false;
            }
        }
    }

    public class AppConfigRoleConfiguration : IRoleConfiguration
    {
        public string this[string key]
        {
            get
            {
                return GetValue(key, true);
            }
            set
            {
                ConfigurationManager.AppSettings[key] = value;
            }
        }

        public string GetValue(string key, bool throwExceptionIfNotFound)
        {
            string value;

            if ((!TryGetVaule(key, out value)) && throwExceptionIfNotFound)
            {
                throw new KeyNotFoundException(string.Format("The key:{0} is not found.", key));
            }

            return value;
        }

        public bool TryGetVaule(string key, out string value)
        {
            value = ConfigurationManager.AppSettings[key];
            return value != null;
        }

        public void Update()
        {
        }
    }

    //public class RoleLocalResource
    //{
    //    private static readonly AveLogger logger = AveLogger.GetInstance(typeof(RoleLocalResource));
    //    //public static readonly string localResourceWorkingDir = @"C:\WorkingPlace";
    //    public static readonly string exeFile = "LocalResourceManagement";
    //    public static void StartProcess(bool isWebRole = false, bool isApiRole = false)
    //    {
    //        try
    //        {
    //            if (!GCommonRoleConfiguration.IsRoleEnvironment && !GCommonRoleConfiguration.IsContainerEnvironment)
    //            {
    //                logger.Info("The current environment is not role, need not to start local resource process.");
    //                return;
    //            }
    //            string exeFilePath = string.Empty;
    //            string currentExecuteLocation = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\";
    //            if (isApiRole)
    //            {
    //                exeFilePath = "bin" + "\\";
    //            }
    //            exeFilePath = exeFilePath + exeFile;
    //            exeFilePath = Path.Combine(currentExecuteLocation, exeFilePath);
    //            logger.Info("The local resource exe file path: {0}.", exeFilePath);
    //            StartProcess sp = new StartProcess(GCommonRoleConfiguration.LocalResourcePath);
    //            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName(exeFile);
    //            foreach (System.Diagnostics.Process existedPro in procs)
    //            {
    //                logger.Info("Delete existed local resource process, process id: {0}.", existedPro.Id);
    //                existedPro.Kill();
    //            }
    //            string rootPath = GCommonRoleConfiguration.LocalResourcePath;
    //            logger.Info("Start local resource process, the root path: {0}.", rootPath);
    //            int localResourceMaximumSize = GCommonRoleConfiguration.LocalResourceMaximumSize;
    //            logger.Info("Start local resource process, the max size: {0}.", localResourceMaximumSize);
    //            string args = string.Format("-o {0} {1}", localResourceMaximumSize, rootPath);
    //            logger.Info("The start process args: {0}.", args);
    //            System.Diagnostics.Process process = sp.Start(exeFilePath, args);
    //            logger.Info("Local resource process id: {0}.", process.Id);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error("Start local resource process failed: {0}.", ex.ToString());
    //        }
    //    }
    //}
}
