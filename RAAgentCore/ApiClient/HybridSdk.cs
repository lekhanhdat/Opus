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
using AvePoint.GCommon;
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Net;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace AvePoint.RA.Common.Hybrid
{
    public class HybridSdk
    {
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(HybridSdk));

        private static ICloudSdkHybridAgentClientFactory clientFactory;
        private string mCustomerId;
        private string mAgentId;
        private string mAgentAuthCode;
        public HybridSdk()
        {
            mCustomerId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
            mAgentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
            mAgentAuthCode = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAuthCode);
            ThrowUtil.ThrowIfNullOrEmpty(mCustomerId, "hybrid sdk tenant Id is null.");
            Init();
        }

        private void Init()
        {

            var identityServiceUrl = CommonConfiguration.getConfig(HybridAppSettingKey.PublicIdentityServiceURL);

            var clientId = CommonConfiguration.getConfig(HybridAppSettingKey.PublicClientIdInIdentityService);
            var portalApiUrl = CommonConfiguration.getConfig(HybridAppSettingKey.RecordAPIServer);
            logger.Info($"init hybrid agent api client:{identityServiceUrl}, {portalApiUrl}, {mCustomerId}");
            var services = new ServiceCollection();

            var sdkBuilder = services.AddHybridCloudSdk(ContractConstants.RECORDS_HYBRID_NAME, CommonConfiguration.getAppCert());
            sdkBuilder.ConfigureDefaultHttpClient("HybridAgentClient", client =>
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
                        //ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
                    }.ConfigProxy();
                });
            }, true).ConfigureIdentityServer(identityServiceUrl, clientId, HBContractConstants.HybridAgentScope).AddHybridAgentApi(portalApiUrl).UseCustomizedLoggerInstance(new HybridSDKLogger());

            var serviceProvider = services.BuildServiceProvider();
            clientFactory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();
            logger.Info($"init hybrid sdk success.");

        }

        public HybridAgentApiClient HybridClient
        {
            get
            {
                
                return clientFactory.CreateHybridAgentClient(mCustomerId, HBContractConstants.HybridAgentScope, mAgentId,mAgentAuthCode);
            }
        }

    }

}
