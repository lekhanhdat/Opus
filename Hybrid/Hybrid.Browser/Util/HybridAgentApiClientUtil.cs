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
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.Hybrid.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Hybrid.ClientCore;
using System.Net.Http;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Net;

namespace AvePoint.RA.Hybrid.Browser.Util
{
    public class HybridAgentApiClientUtil
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(HybridAgentApiClientUtil));

        public static HybridAgentApiClient Client
        {
            get => GetClient();
        }

        private static HybridAgentApiClient GetClient()
        {
            var apiUrl = CommonConfiguration.getConfig(HybridAppSettingKey.RecordAPIServer);
            var agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
            var agentInstallationCode = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAuthCode);
            var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
            var identityServer = CommonConfiguration.getConfig(HybridAppSettingKey.PublicIdentityServiceURL);
            var identityClientId = CommonConfiguration.getConfig(HybridAppSettingKey.PublicClientIdInIdentityService);

            Logger.Info($"Get hybrid agent: identity server: [{identityServer}].");

            var services = new ServiceCollection();
            services.AddHybridCloudSdk(Constants.ClientAPI_ProductName, CommonConfiguration.getAppCert())
                .ConfigureIdentityServer(identityServer, identityClientId, HBContractConstants.HybridAgentScope)
                .ConfigureDefaultHttpClient("HybridAgentClient", client =>
                {
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
                            //ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
                        }.ConfigProxy();

                    });
                })
                .AddHybridAgentApi(apiUrl).UseCustomizedLoggerInstance(new HybridSDKLogger());

            var serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();

            return factory.CreateHybridAgentClient(tenantId, HBContractConstants.HybridAgentScope, agentId, agentInstallationCode);
        }
    }
}
