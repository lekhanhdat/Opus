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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.Portal.Logger;
using Cloud.Sdk.Aos;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.CloudInsights;
using Cloud.Sdk.Core;
using Cloud.Sdk.Dao;
using Cloud.Sdk.Token;
using Cloud.Sdk.Cop;
using Cloud.Sdk.MyHub;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Cloud.Sdk.Data.Core;

using Cloud.Sdk.Amls.Ics;
using Cloud.Sdk.Data.Amls.Ics.Category;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.IE;
using Cloud.Sdk.EDiscovery;
using Cloud.Sdk.Aosp;
using Cloud.Sdk.Nexus.Foundation;
using Cloud.Sdk.Nexus.Governance;
using Cloud.Sdk.LAL.PlatformSS;

namespace AvePoint.GCommon.Utility
{
    public class AosApiUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AosApiUtility));
        private static bool initialized;
        private static bool aospInitialized;
        public const string ProductName = "AvePointRecords";
        private static ICloudSdkAosClientFactory clientFactory;
        private static ICloudSdkAosModernApiClientFactory modernClientFactory;
        private static ICloudSdkCopClientFactory copClientFactory;
        private static ICloudSdkAmlsIcsClientFactory icsClientFactory;
        private static ICloudSdkIEClientFactory ieClientFactory;
        private static ICloudSdkEDiscoveryClientFactory eDiscoveryClientFactory;
        private static ICloudSdkMyHubClientFactory myHubClientFactory;
        private static ICloudSdkAospClientFactory aospClientFactory;
        private static ICloudSdkNexusFoundationClientFactory gControlPlatformClientFactory;
        private static ICloudSdkNexusGovernanceClientFactory nexusGovernanceClientFactory;
        private static ICloudSdkLALPlatformSSClientFactory lalPlatformSSClientFactory;

        private static ICloudSdkTokenClientFactory _CloudSdkTokenClientFactory;
        public static ICloudSdkTokenClientFactory CloudSdkTokenClientFactory
        {
            get
            {
                if (_CloudSdkTokenClientFactory == null)
                {
                    Init(GCommonRoleConfiguration.RECO_Certificate);
                }
                return _CloudSdkTokenClientFactory;
            }
        }     

        private static ICloudSdkCloudInsightsClientFactory _CloudinsightsClientFactory;
        public static ICloudSdkCloudInsightsClientFactory CloudInsightsClientFactory
        {
            get
            {
                if (_CloudinsightsClientFactory == null)
                {
                    Init(GCommonRoleConfiguration.RECO_Certificate);
                }
                return _CloudinsightsClientFactory;
            }
        }

        private static ICloudSdkDocAveOnlineClientFactory _CloudSdkDaoClientFactory;
        public static ICloudSdkDocAveOnlineClientFactory CloudSdkDaoClientFactory
        {
            get
            {
                if (_CloudSdkDaoClientFactory == null)
                {
                    Init(GCommonRoleConfiguration.RECO_Certificate);
                }
                return _CloudSdkDaoClientFactory;
            }
        }
        public static void Init(X509Certificate2 certificate, bool closeConnection = false)
        {
            if (!initialized)
            {                
                
                var identityServiceUrl = GCommonRoleConfiguration.IdentityServerAddress;
                var clientId = GCommonRoleConfiguration.IdentityServerClientId;               
                var tokenUrl = GCommonRoleConfiguration.AosTokenApiURL;

                var services = new ServiceCollection();
                var sdkBuilder = services.AddCloudSdk(CallerType.CloudRecords, certificate);
                
                sdkBuilder.ConfigureDefaultHttpClient("CloudSdkHttpClient", client =>
                {
                    client.ConfigureHttpClient(c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(6);
                        if (closeConnection)
                        {
                            c.DefaultRequestHeaders.Add("Connection", "close");
                        }
                    });
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
                            MaxConnectionsPerServer = 1024,
#if DEBUG
                            ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
#endif

                        };
                    });
                }, true).ConfigureCloudSdkLogger(loggingOptions =>
                {
                    loggingOptions.OnLogInformation = msg => logger.Info(msg);
                    loggingOptions.OnLogWarning = msg => logger.Warn(msg);
                    loggingOptions.OnLogDebug = msg => logger.Debug(msg);
                    loggingOptions.OnLogError = msg => logger.Error(msg);
                });
                if (!string.IsNullOrEmpty(identityServiceUrl) && !string.IsNullOrEmpty(clientId))
                {
                    logger.Info($"use identity service to access aos api.");
                    sdkBuilder.ConfigureIdentityServer(identityServiceUrl, clientId);
                }

                sdkBuilder.AddCloudSdkAosApi(GetPortalApiUrl())
                    .AddCloudSdkAosModernApi(GetModernPortalApiUrl())
                    .AddCloudSdkCloudInsightsApi(GCommonRoleConfiguration.PortalCloudInsightsApiURL)
                    .AddCloudSdkDaoApi(GCommonRoleConfiguration.ControlServiceAddress);
                    //.AddCloudSdkHttpClient<IEApiOption>(true, new CloudSdkClientConfiguration
                    //{
                    //    DefaultApiUrl = "https://graph.sharepointguild.com/insightsengine",
                    //});

                if (!string.IsNullOrEmpty(tokenUrl))
                {
                    sdkBuilder.AddCloudSdkTokenApi(tokenUrl);
                }
                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.COP_API_URL)) 
                {
                    sdkBuilder.AddCloudSdkCopApi(GCommonRoleConfiguration.COP_API_URL);
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.MYHUB_API_URL))
                {
                    sdkBuilder.AddCloudSdkMyHubApi(GCommonRoleConfiguration.MYHUB_API_URL);
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.ICS_API_URL))
                {
                    sdkBuilder.AddCloudSdkAmlsIcsApi(GCommonRoleConfiguration.ICS_API_URL);
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.DAL_GATEWAY_API_URL))
                {
                    sdkBuilder.AddCloudSdkLALPlatformSSApi();
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.INSIGHTS_ENGINE_API_URL))
                {
                    sdkBuilder.AddCloudSdkIEApi(GCommonRoleConfiguration.INSIGHTS_ENGINE_API_URL);
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.EDISCOVERY_API_URL))
                {
                    sdkBuilder.AddCloudSdkEDiscoveryApi(GCommonRoleConfiguration.EDISCOVERY_API_URL);
                }

                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.AOSP_API_URL))
                {
                    sdkBuilder.AddCloudSdkAospApi(GCommonRoleConfiguration.AOSP_API_URL);
                }
                
                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.NEXUS_FOUNDATION_API_URL))
                {
                    sdkBuilder.AddCloudSdkNexusFoundationApi(GCommonRoleConfiguration.NEXUS_FOUNDATION_API_URL);
                }
                
                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.NEXUS_GOVERNANCE_API_URL))
                {
                    sdkBuilder.AddCloudSdkNexusGovernanceApi(GCommonRoleConfiguration.NEXUS_GOVERNANCE_API_URL);
                }

                var serviceProvider = services.BuildServiceProvider();
                clientFactory = serviceProvider.GetService<ICloudSdkAosClientFactory>();
                modernClientFactory = serviceProvider.GetService<ICloudSdkAosModernApiClientFactory>();
                copClientFactory = serviceProvider.GetService<ICloudSdkCopClientFactory>();
                myHubClientFactory = serviceProvider.GetService<ICloudSdkMyHubClientFactory>();
                aospClientFactory = serviceProvider.GetService<ICloudSdkAospClientFactory>();
                icsClientFactory = serviceProvider.GetService<ICloudSdkAmlsIcsClientFactory>();
                ieClientFactory = serviceProvider.GetService<ICloudSdkIEClientFactory>();
                eDiscoveryClientFactory = serviceProvider.GetService<ICloudSdkEDiscoveryClientFactory>();
                gControlPlatformClientFactory = serviceProvider.GetService<ICloudSdkNexusFoundationClientFactory>();
                nexusGovernanceClientFactory = serviceProvider.GetService<ICloudSdkNexusGovernanceClientFactory>();
                lalPlatformSSClientFactory = serviceProvider.GetService<ICloudSdkLALPlatformSSClientFactory>();

                if (_CloudSdkTokenClientFactory == null)
                {
                    try
                    {
                        _CloudSdkTokenClientFactory = serviceProvider.GetService<ICloudSdkTokenClientFactory>();
                        logger.Info($"Init Cloud Aos Token API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        logger.Info($"Init Cloud Aos Token API error: {ex}");
                    }
                }
                if (_CloudinsightsClientFactory == null)
                {
                    try
                    {
                        _CloudinsightsClientFactory = serviceProvider.GetService<ICloudSdkCloudInsightsClientFactory>();
                        logger.Info($"Init Cloudinsights API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        logger.Info($"Init Cloudinsights API Factory error: {ex}");
                    }
                }
                if (_CloudSdkDaoClientFactory == null)
                {

                    try
                    {
                        _CloudSdkDaoClientFactory = serviceProvider.GetService<ICloudSdkDocAveOnlineClientFactory>();
                        logger.Info($"Init Daoave Online API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        logger.Info($"Init  Daoave Online API Factory error: {ex}");
                    }
                }
                logger.Info($"Init Cloud Aos SDK success!");
                initialized = true;
            }
        }

        public static string GetPortalApiUrl()
        {
            string apiUrl = GCommonRoleConfiguration.PortalApiURL;
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiUrl.Trim()))
            {
                logger.Error("GetPortalApiUrl error:PortalAPIURL is null or empty!");
                throw new Exception("PortalAPIURL is null or empty!");
            }
            apiUrl = apiUrl.Trim();
            while (apiUrl.EndsWith("/"))
            {
                apiUrl = apiUrl.Remove(apiUrl.Length - 1);
            }
            logger.Info("GetPortalApiUrl: {0}", apiUrl);
            return apiUrl;
        }

        public static string GetModernPortalApiUrl()
        {
            string apiUrl = GCommonRoleConfiguration.ModernPortalApiURL;
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiUrl.Trim()))
            {
                logger.Error("GetPortalApiUrl error:ModernPortalApiURL is null or empty!");
                throw new Exception("ModernPortalApiURL is null or empty!");
            }
            apiUrl = apiUrl.Trim();
            while (apiUrl.EndsWith("/"))
            {
                apiUrl = apiUrl.Remove(apiUrl.Length - 1);
            }
            logger.Info("GetModernPortalApiUrl: {0}", apiUrl);
            return apiUrl;
        }

        public static AosApiClient AosClient
        {
            get
            {
                return clientFactory.CreateAosClient();
            }
        }

        public static CopApiClient CopClient
        {
            get
            {
                return copClientFactory.CreateCopApiClient(GCommonRoleConfiguration.PortalApiURL);
            }
        }

        public static MyHubApiClient GetMyhubClient(string tenantGroupId)
        {
            return myHubClientFactory.CreateMyHubClient(tenantGroupId);
        }

        public static AospApiClient GetAospApiClient()
        {
            return aospClientFactory.CreateAospClient();
        }

        public static AosModernApiTenantClient GetAosModerClient()
        {
            return modernClientFactory.CreateAosModernApiTenantClient(TenantLocalValue.LogonGroupId);
        }

        public static AosModernApiTenantClient GetAosModernClient(string tenantGroupId)
        {
            return modernClientFactory.CreateAosModernApiTenantClient(tenantGroupId);
        }

        public static AosModernApiApplicationClient GetAosModernApplicationClient()
        {
            return modernClientFactory.CreateAosModernApiApplicationClient();
        }

        public static AmlsIcsApiClient GetIcsClient(string tenantGroupId)
        {
            return icsClientFactory.CreateAmlsIcsClient(tenantGroupId);
        }

        public static ICloudSdkLALPlatformSSClientFactory LALPlatformSSClientFactory
        {
            get
            {
                if (lalPlatformSSClientFactory == null)
                {
                    Init(GCommonRoleConfiguration.RECO_Certificate);
                }

                return lalPlatformSSClientFactory;
            }
        }

        public static CloudInsightsApiClient GetCloudInsightsClient()
        {
            var cloudInsightsApiUrl = GCommonRoleConfiguration.PortalCloudInsightsApiURL;
#if DEBUG
            cloudInsightsApiUrl = "https://graph.sharepointguild.com/cloudinsights";
#endif
            return CloudInsightsClientFactory.CreateCloudInsightsClient(cloudInsightsApiUrl, TenantLocalValue.LogonGroupId);
        }

        public static IEApiClient GetInsightsEngineApiClient()
        {
             return ieClientFactory.CreateIEClient("insightsengine.readwrite.all", TenantLocalValue.LogonGroupId);
        }

        public static EDiscoveryApiClient GetEDiscoveryApiClient()
        {
            return eDiscoveryClientFactory.CreateEDiscoveryClient(TenantLocalValue.LogonGroupId);
        }
        
        public static NexusFoundationApiClient GetGControlClient()
        {
            if (gControlPlatformClientFactory == null)
            {
                return null;
            }
            return gControlPlatformClientFactory.CreateNexusFoundationClient(TenantLocalValue.LogonGroupId);
        }
        
        public static NexusGovernanceApiClient GetNexusGovernanceClient()
        {
            if (nexusGovernanceClientFactory == null)
            {
                return null;
            }
            return nexusGovernanceClientFactory.CreateGovernanceClient(TenantLocalValue.LogonGroupId);
        }
    }
}
