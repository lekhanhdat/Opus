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
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.Hybrid.Contract.Object;

using CommonModel.MethodInfo;
using HybirdProxy.Token;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.Utility.Net;
using HybridCommonModel.DataModel;
using Polly;

namespace AvePoint.Hybrid.Utility.ConfigurationFile
{
    public class ConfigurationFileChecker
    {
        //private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static string ProductName = "HybridAgent";

        /// <summary>
        /// Check if the configuration can be used.
        /// if this configuration was already used, return false, otherwise, return true
        /// </summary>
        /// <param name="configurtion"></param>
        /// <returns></returns>
        public static bool Validate(AgentConfigurtion configurtion, AveWebProxyOptions proxyOptions)
        {
            //return true;
            var client = GetClient(configurtion, proxyOptions);
            var result = Task.Run(() => client.AgentMgmtService.Validate(new AgentConfigurtion { Id = configurtion.Id, PackageId = configurtion.PackageId })).Result;
            return result;
        }

        /// <summary>
        /// Mark this configuration is used
        /// </summary>
        /// <param name="configurtion"></param>
        /// <returns></returns>
        public static bool UpdateConfigFileStatus(AgentConfigurtion configurtion, AveWebProxyOptions proxyOptions)
        {
            //return true;
            var client = GetClient(configurtion, proxyOptions);
            var result = Task.Run(() => client.AgentMgmtService.Install(new AgentConfigurtion { Id = configurtion.Id, PackageId = configurtion.PackageId })).Result;
            return result;
        }

        private static X509Certificate2 GetCert(AgentConfigurtion configurtion)
        {

            return new X509Certificate2(Convert.FromBase64String(configurtion.CertificateContent), configurtion.CertificatePWD);
        }

        private static HybridAgentApiClient GetClient(AgentConfigurtion configurtion, AveWebProxyOptions proxyOptions)
        {
            //logger.Info("Get hybrid agent , identityServer :  " + identityServer + ", identityClientId : " + identityClientId);

            var services = new ServiceCollection();
           
            services.AddHybridCloudSdk(ProductName, GetCert(configurtion))
                .ConfigureIdentityServer(configurtion.IdentityServiceUrl, configurtion.ClientId, HBContractConstants.HybridAgentScope)
                .ConfigureDefaultHttpClient("HybridAgentClient", client =>
                {
                    client.AddResilienceHandler("HybridAgentClientPipeline", configureBuilder =>
                    {
                        configureBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage> 
                        { 
                            MaxRetryAttempts = 2,
                            DelayGenerator = args =>
                            {
                                var delay = TimeSpan.FromSeconds(5);
                                if(args.AttemptNumber == 0)
                                {
                                    delay = TimeSpan.FromSeconds(1);
                                }
                                else if (args.AttemptNumber == 1)
                                {
                                    delay = TimeSpan.FromSeconds(2);
                                }
                                return new ValueTask<TimeSpan?>(delay);
                            }
                        });
                    });
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
                            //ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
                        }.ConfigProxy(proxyOptions);

                    });
                }, customizeRetry:true)
                .AddHybridAgentApi(configurtion.RecordsApiUrl);

            var serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();

            return factory.CreateHybridAgentClient(configurtion.CustomerId, HBContractConstants.HybridAgentScope, configurtion.Id, configurtion.AuthCode);
        }

        /// <summary>
        /// check if the client has the necessary scopes
        /// </summary>
        /// <param name="configurtion"></param>
        /// <returns></returns>
        public static bool ValidateScope(AgentConfigurtion configurtion, AveWebProxyOptions proxyOptions)
        {
            var scopes = string.Join(" ", APIScope.Agent, HybridAgentPermissionScopes.ReadWrite_All, APIScope.Common);
            var httpClient = proxyOptions != null && proxyOptions.Enabled ? AveHttpConnectionUtil.CreateHttpClient(proxyOptions) : AveHttpConnectionUtil.CreateHttpClient(false);
            var task = TokenHelper.RequestToken(httpClient, configurtion.Id, configurtion.AuthCode, scopes, configurtion.ClientId, configurtion.IdentityServiceUrl, () => GetCert(configurtion), configurtion.CustomerId);
            var token = task.GetAwaiter().GetResult();

            return !string.IsNullOrEmpty(token);
        }
    }
}
