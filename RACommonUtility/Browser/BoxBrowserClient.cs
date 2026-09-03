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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Browser
{
    public class BoxBrowserClient
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(BoxBrowserClient));

        public static async Task<RABoxBrowserContract> GetRootNode()
        {
            try
            {
                var hybridAgentApiClient = GetClient();
                return await hybridAgentApiClient.BoxBrowserService.GetRootNode();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree node. Error: {ex}");
                return null;
            }
        }

        public static async Task<IEnumerable<RABoxBrowserContract>> GetChildrenWithSettingIcon(RABoxBrowserContract contract)
        {
            try
            {
                var hybridAgentApiClient = GetClient();
                return await hybridAgentApiClient.BoxBrowserService.GetChildrenWithSettingIcon(contract);
            }
            catch(Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree node. Error: {ex}");
                return null;
            }
        }
        public static async Task<RABoxBrowserContract> BBrowserTreeByPager(RABoxBrowserContract contract)
        {
            try
            {
                var hybridAgentApiClient = GetClient();
                return await hybridAgentApiClient.BoxBrowserService.BBrowserTreeByPager(contract);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree node. Error: {ex}");
                return null;
            }
        }

        private static HybridAgentApiClient GetClient()
        {
            var identityServer = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.IDENTITY_SERVICE_URL];
            var indentityClientId = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
            var apiUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.RECO_API_URL];
            var certificate = RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords);

            var services = new ServiceCollection();
            services.AddHybridCloudSdk(RecordsConstants.RECORDS_APPLICATION_NAME, certificate)
                .ConfigureIdentityServer(identityServer, indentityClientId, HBContractConstants.HybridInernalScope, true)
                .ConfigureDefaultHttpClient("RABrowserClient", client =>
                {
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
#if DEBUG
                            ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
#endif
                        };

                    });
                })
                .AddHybridAgentApi(apiUrl);

            var serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();

            return factory.CreateHybridAgentClient(TenantLocalValue.LogonGroupId, HBContractConstants.HybridInernalScope);
        }
    }
}
