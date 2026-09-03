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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Cloud.Sdk.Aos;
using Cloud.Sdk.Cop;
using Cloud.Sdk.Core;
using Cloud.Sdk.Token;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.RA.Common.Aos
{

//    public class RMCloudSdk
//    {
//        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(RMCloudSdk));
//        private static bool initialized;
//        private static ICloudSdkAosClientFactory clientFactory;
//        private static ICloudSdkTokenClientFactory tokenClientFactory;
//        private static ICloudSdkCopClientFactory copClientFactory;
//        public static ICloudSdkTokenClientFactory TokenClientFactory
//        {
//            get
//            {
//                return tokenClientFactory;
//            }
//        }

//        public static void Init(X509Certificate2 certificate, bool closeConnection = false)
//        {
//            if (!initialized)
//            {
//                var identityServiceUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.IDENTITY_SERVICE_URL];
//                var clientId = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
//                var portalApiUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.AOS_API_URL];
//                var tokenUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.TOKEN_API_URL];
//                var copApiUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.COP_API_URL];
//                var services = new ServiceCollection();
//                var sdkBuilder = services.AddCloudSdk(RecordsConstants.RECORDS_APPLICATION_NAME, certificate);
//                sdkBuilder.ConfigureCloudSdkLogger(option =>
//                {
//                    option.OnLogInformation = (msg) => { logger.Info(msg); };
//                    option.OnLogWarning = (msg) => { logger.Warn(msg); };
//                    option.OnLogDebug = (msg) => { };
//                    option.OnLogError = (msg) => { logger.Error(msg); };
//                });
//                sdkBuilder.ConfigureDefaultHttpClient("CloudSdkHttpClient", client =>
//                {
//                    client.ConfigureHttpClient(c =>
//                    {
//                        c.Timeout = TimeSpan.FromMinutes(6);
//                        if (closeConnection)
//                        {
//                            c.DefaultRequestHeaders.Add("Connection", "close");
//                        }
//                    });
//                    client.ConfigurePrimaryHttpMessageHandler(() =>
//                    {
//                        return new HttpClientHandler()
//                        {
//                            MaxConnectionsPerServer = 64,
//#if DEBUG
//                            ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
//#endif
//                        };
//                    });
//                }, true);

//                if (!string.IsNullOrEmpty(identityServiceUrl) && !string.IsNullOrEmpty(clientId))
//                {
//                    logger.Info($"use identity service to access aos api.");
//                    sdkBuilder.ConfigureIdentityServer(identityServiceUrl, clientId);
//                }

//                sdkBuilder.AddCloudSdkAosApi(portalApiUrl);
//                if (!string.IsNullOrEmpty(copApiUrl))
//                {
//                    sdkBuilder.AddCloudSdkCopApi(copApiUrl);
//                }
                
//                if (!string.IsNullOrEmpty(tokenUrl))
//                {
//                    sdkBuilder.AddCloudSdkTokenApi(tokenUrl);
//                }

//                var serviceProvider = services.BuildServiceProvider();

//                clientFactory = serviceProvider.GetService<ICloudSdkAosClientFactory>();
//                tokenClientFactory = serviceProvider.GetService<ICloudSdkTokenClientFactory>();
//                copClientFactory = serviceProvider.GetService<ICloudSdkCopClientFactory>();
//                initialized = true;
//            }
//        }

//        public static AosApiClient Aos
//        {
//            get
//            {
//                return clientFactory.CreateAosClient();
//            }
//        }
//        public static CopApiClient Cop 
//        {
//            get 
//            {
//                return copClientFactory.CreateCopApiClient();
//            }
//        }
//        //public static TokenApiClient TokenClient
//        //{
//        //    get
//        //    {
//        //        return tokenClientFactory?.CreateTokenApiClient();                
//        //    }
//        //}

//    }
}
