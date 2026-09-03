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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Cloud.Sdk.Aos;
using Microsoft.Extensions.DependencyInjection;
using Cloud.Sdk;
using Cloud.Sdk.Core;
using Cloud.Sdk.Token;
using System.Net.Http;
using AvePoint.RA.Common.Configurations;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.Portal.Logger;
using Cloud.Sdk.CloudInsights;
using Cloud.Sdk.Dao;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Cop;
using Cloud.Sdk.Data.Core;
using Cloud.Sdk.Amls.Ics;
using Cloud.Sdk.Data.Amls.Ics.Category;

namespace AvePoint.RA.Common.Aos
{

    public class RMCloudSdk
    {
        public const string ProductName = "AvePointRecords";
        private static bool initialized;
        private static ICloudSdkAosClientFactory clientFactory;
        private static ICloudSdkTokenClientFactory tokenClientFactory;
        private static ICloudSdkAosModernApiClientFactory modernClientFactory;
        private static ICloudSdkCopClientFactory copClientFactory;
        private static ICloudSdkAmlsIcsClientFactory icsClientFactory;

        private static ICloudSdkTokenClientFactory _CloudSdkTokenClientFactory;
        public static ICloudSdkTokenClientFactory CloudSdkTokenClientFactory
        {
            get
            {
                return _CloudSdkTokenClientFactory;
            }
        }

        private static ICloudSdkCloudInsightsClientFactory _CloudinsightsClientFactory;
        public static ICloudSdkCloudInsightsClientFactory CloudInsightsClientFactory
        {
            get
            {
                return _CloudinsightsClientFactory;
            }
        }

        private static ICloudSdkDocAveOnlineClientFactory _CloudSdkDaoClientFactory;
        public static ICloudSdkDocAveOnlineClientFactory CloudSdkDaoClientFactory
        {
            get
            {
                return _CloudSdkDaoClientFactory;
            }
        }
        public static void InitForUnitTest(X509Certificate2 certificate, string identityServiceUrl, string clientId, string portalApiUrl, string modernPortalApiUrl)
        {
            if (!initialized)
            {

                var tokenUrl = GCommonRoleConfiguration.AosTokenApiURL;
                var services = new ServiceCollection();
                var sdkBuilder = services.AddCloudSdk(CallerType.CloudRecords, certificate);
                sdkBuilder.ConfigureDefaultHttpClient("CloudSdkHttpClient", client =>
                {
                    client.ConfigureHttpClient(c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(6);
                    });
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
                            MaxConnectionsPerServer = 64,
#if DEBUG
                            ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
#endif
                        };
                    });
                }, true).UseCustomizedLoggerInstance(new SDKLogger());
                if (!string.IsNullOrEmpty(identityServiceUrl) && !string.IsNullOrEmpty(clientId))
                {
                    //logger.Info($"use identity service to access aos api.");
                    sdkBuilder.ConfigureIdentityServer(identityServiceUrl, clientId);
                }

                sdkBuilder.AddCloudSdkAosApi(portalApiUrl)
                    .AddCloudSdkAosModernApi(modernPortalApiUrl)
                    .AddCloudSdkCloudInsightsApi(GCommonRoleConfiguration.PortalCloudInsightsApiURL)
                    .AddCloudSdkDaoApi(GCommonRoleConfiguration.ControlServiceAddress);

                if (!string.IsNullOrEmpty(tokenUrl))
                {
                    sdkBuilder.AddCloudSdkTokenApi(tokenUrl);
                }

                sdkBuilder.AddCloudSdkAmlsIcsApi("https://10.1.53.39:8886");

                var serviceProvider = services.BuildServiceProvider();
                clientFactory = serviceProvider.GetService<ICloudSdkAosClientFactory>();
                modernClientFactory = serviceProvider.GetService<ICloudSdkAosModernApiClientFactory>();
                copClientFactory = serviceProvider.GetService<ICloudSdkCopClientFactory>();
                icsClientFactory = serviceProvider.GetService<ICloudSdkAmlsIcsClientFactory>();

                if (_CloudSdkTokenClientFactory == null)
                {
                    try
                    {
                        _CloudSdkTokenClientFactory = serviceProvider.GetService<ICloudSdkTokenClientFactory>();
                        //logger.Info($"Init Cloud Aos Token API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        //logger.Info($"Init Cloud Aos Token API error: {ex}");
                    }
                }
                if (_CloudinsightsClientFactory == null)
                {
                    try
                    {
                        _CloudinsightsClientFactory = serviceProvider.GetService<ICloudSdkCloudInsightsClientFactory>();
                        //logger.Info($"Init Cloudinsights API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        //logger.Info($"Init Cloudinsights API Factory error: {ex}");
                    }
                }
                if (_CloudSdkDaoClientFactory == null)
                {
                    try
                    {
                        _CloudSdkDaoClientFactory = serviceProvider.GetService<ICloudSdkDocAveOnlineClientFactory>();
                        //logger.Info($"Init Daoave Online API Factory success!");
                    }
                    catch (Exception ex)
                    {
                        //logger.Info($"Init  Daoave Online API Factory error: {ex}");
                    }
                }
                //logger.Info($"Init Cloud Aos SDK success!");
                initialized = true;
            }
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
                return copClientFactory.CreateCopApiClient();
            }
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
    }
}
