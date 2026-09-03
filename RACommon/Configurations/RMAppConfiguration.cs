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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Contract.Configurations;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Util.MSAzure;

namespace AvePoint.RA.Common.Configurations
{
    public class RMAppConfiguration : RMBaseConfiguration<RMAppSettingKey>
    {
        private Dictionary<RMAppSettingKey, RMEncryptType> encryptItems = new Dictionary<RMAppSettingKey, RMEncryptType>()
        {
            { RMAppSettingKey.JOB_CONFIG_FOR_CUSTOMERS, RMEncryptType.Base64 },
            { RMAppSettingKey.AOS_CUSTOM_APP_CONFIG, RMEncryptType.Base64 },
        };
        private HashSet<RMAppSettingKey> envVarItems = new HashSet<RMAppSettingKey>()
        { 
            RMAppSettingKey.ENABLE_SECURITY_TRIMMING 
        };

        public RMAppConfiguration() : base()
        {
            InitDAOConfiguration();
        }

        protected override Dictionary<RMAppSettingKey, RMEncryptType> EncryptedItems => encryptItems;
        protected override HashSet<RMAppSettingKey> EnvVirableItems => RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? null : envVarItems;

        public string DatabasePrimaryServerName { get; set; }
        public string SubscriptionId
        {

            get
            {
                var subscriptionId = string.Empty;
                if (!string.IsNullOrEmpty(RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_PRIMARY_SERVER]))
                {
                    var parts = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_PRIMARY_SERVER].Split('/');
                    subscriptionId = parts[0];
                }
                return subscriptionId;
            }
        }
        public int NodeCountInSubJob
        {
            get
            {
                var nodeCnt = 5;
                int.TryParse(this[RMAppSettingKey.NODE_COUNT_IN_SUB_JOB], out nodeCnt);
                return nodeCnt;
            }
        }

        public CustomAppConfigs CustomAppConfig
        {
            get
            {
                try
                {
                    var customAppConfigXml = this[RMAppSettingKey.AOS_CUSTOM_APP_CONFIG];
                    //logger.Info($"CustomAppConfig:{customAppConfigXml}");
                    if (string.IsNullOrEmpty(customAppConfigXml))
                    {
                        return null;
                    }
                    return SerializerHelper.DeserializeFromXmlString<CustomAppConfigs>(customAppConfigXml);
                }
                catch (Exception e)
                {
                    logger.Error($"Get CustomAppConfig error:{e.ToString()}");
                    return null;
                }
            }
        }

        private void InitDAOConfiguration()
        {
            try
            {
                var gCommonConfig = GCommonRoleConfiguration.Instance;
                GCommonRoleConfiguration.RECO_Certificate = RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords);
                //if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                //{
                //    // GCommonRoleConfiguration.DAOAppSettings ?
                //}
                //else
                //{
                //    //var keyVaultUrl = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.DAO_KEY_VAULT_URL];
                //    //GCommonRoleConfiguration.DAOAppSettings = KeyVaultUtil.GetSecretAsync(GCommonRoleConfiguration.SECRET_NAME_DAO_APPSETTING, keyVaultUrl).Result;
                //    //GCommonRoleConfiguration.DAO_Certificate = RMCertificateHelper.GetCertificateByManagedIdentity(keyVaultUrl, GCommonRoleConfiguration.CERTIFICATE_IDENTIFIER_DAO);
                //    //GCommonRoleConfiguration.WCF_Certificate = RMCertificateHelper.GetCertificateByManagedIdentity(keyVaultUrl, GCommonRoleConfiguration.CERTIFICATE_IDENTIFIER_WCF);
                //    //GCommonRoleConfiguration.Encrypt_Certificate = RMCertificateHelper.GetCertificateByManagedIdentity(keyVaultUrl, GCommonRoleConfiguration.CERTIFICATE_IDENTIFIER_AVE);
                //}

                gCommonConfig["AOS_API_URL"] = this[RMAppSettingKey.AOS_API_URL];
                gCommonConfig["AOS_MODERN_API_URL"] = this[RMAppSettingKey.AOS_MODERN_API_URL];
                //gCommonConfig["KeyVaultUrl"] = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.DAO_KEY_VAULT_URL];
                //gCommonConfig["KeyVaultClientId"] = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.DAO_KEY_VAULT_CLIENT_ID];
                //gCommonConfig["KeyVaultCertThumbprint"] = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.DAO_KEY_VAULT_CERTIFICATE_THUMBPRINT];
                gCommonConfig["AosTokenApiURL"] = this[RMAppSettingKey.TOKEN_API_URL];
                gCommonConfig["UseIdentityServer"] = (string.IsNullOrEmpty(this[RMAppSettingKey.IDENTITY_SERVICE_URL]) ? false : true).ToString();
                gCommonConfig["EnableTokenService"] = (string.IsNullOrEmpty(this[RMAppSettingKey.TOKEN_API_URL]) ? false : true).ToString();
                gCommonConfig["IdentityServerAddress"] = this[RMAppSettingKey.IDENTITY_SERVICE_URL];
                gCommonConfig["IdentityServerClientId"] = this[RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
                gCommonConfig["PortalCloudInsightsApiURL"] = this[RMAppSettingKey.CLOUD_INSIGHTS_API_URL];
                gCommonConfig["ControlServiceAddress"] = this[RMAppSettingKey.DAO_CONTROL_SERVICE_ADDRESS];
                gCommonConfig["COP_API_URL"] = this[RMAppSettingKey.COP_API_URL];
                gCommonConfig["MYHUB_API_URL"] = this[RMAppSettingKey.MYHUB_API_URL];
                gCommonConfig["ICS_API_URL"] = this[RMAppSettingKey.ICS_API_URL];
                gCommonConfig["DAL_GATEWAY_API_URL"] = this[RMAppSettingKey.DAL_GATEWAY_API_URL];
                gCommonConfig["INSIGHTS_ENGINE_API_URL"] = this[RMAppSettingKey.INSIGHTS_ENGINE_API_URL];
                gCommonConfig["EDISCOVERY_API_URL"] = this[RMAppSettingKey.EDISCOVERY_API_URL];
                gCommonConfig["AOSP_API_URL"] = this[RMAppSettingKey.AOSP_API_URL];
                gCommonConfig["NEXUS_FOUNDATION_API_URL"] = this[RMAppSettingKey.NEXUS_FOUNDATION_API_URL];
                gCommonConfig["NEXUS_GOVERNANCE_API_URL"] = this[RMAppSettingKey.NEXUS_GOVERNANCE_API_URL];
                gCommonConfig["GCONTROL_MYHUB_TASK_URL"] = this[RMAppSettingKey.GCONTROL_MYHUB_TASK_URL];
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while intit dao config:{ex.ToString()}");
            }

        }

        public bool IsGovStaging() 
        {
            return RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME] == "US Gov Virginia Staging";
        }
            
        public string GetChatBotAPIUrl()
        {
            var DCCenter = this[RMAppSettingKey.AOS_DATA_CENTER];
            var chatBotUrls = GetSectionValueFromCongfigFile("CHAT_BOT_API_URL");
            if(chatBotUrls == null || !chatBotUrls.Any())
            {
                return string.Empty;
            }
            return chatBotUrls.FirstOrDefault(_ => _.Key.Equals(DCCenter, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        }

        public Dictionary<string, string> GetMultiGeoDCResourceApiUrl()
        {
            return GetMultiGeoConfigDictionary("MULTI_GEO_DC_RESOURCE_API_URL");
        }

        public Dictionary<string, string> GetMultiGeoDomainUrl()
        {
            return GetMultiGeoConfigDictionary("MULTI_GEO_DOMAIN_URL");
        }

        public Dictionary<string, string> GetMultiGeoPublicRecoApiUrl()
        {
            return GetMultiGeoConfigDictionary("MULTI_GEO_PUBLIC_RECO_API_URL");
        }

        public Dictionary<string, string> GetMultiGeoPublicSignalRServerUrl()
        {
            return GetMultiGeoConfigDictionary("MULTI_GEO_PUBLIC_SIGNALR_SERVER_URL");
        }

        private Dictionary<string, string> GetMultiGeoConfigDictionary(string sectionName)
        {
            var configDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var configSections = GetSectionValueFromCongfigFile(sectionName);
            if (configSections == null || !configSections.Any())
            {
                return configDictionary;
            }

            foreach (var configSection in configSections)
            {
                if (string.IsNullOrWhiteSpace(configSection?.Key) || string.IsNullOrWhiteSpace(configSection.Value))
                {
                    continue;
                }

                configDictionary[configSection.Key] = configSection.Value;
            }

            return configDictionary;
        }
    }
}
