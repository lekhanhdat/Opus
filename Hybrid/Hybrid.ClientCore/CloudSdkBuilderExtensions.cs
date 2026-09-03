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
namespace AvePoint.Hybrid.ClientCore
{
    using AvePoint.Hybrid.ClientCore.Clients;
    using AvePoint.Hybrid.ClientCore.Logging;
    using HybridCommonModel.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Polly;
    using Polly.Extensions.Http;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public static class CloudSdkBuilderExtensions
    {
        public static CloudSdkBuilder UseCloudSdkCore(this CloudSdkBuilder builder, Action<CloudSdkCoreOptions> configure)
        {
            builder.Services.Configure(configure);
            return builder;
        }

        public static CloudSdkBuilder UseCustomizedLoggerInstance(this CloudSdkBuilder builder, ISdkLogger logger)
        {
            builder.Services.RemoveAll<ISdkLogger>();
            builder.Services.AddSingleton<ISdkLogger>(privoder => logger);
            return builder;
        }
        
        /// <summary>
        /// Cloud SdK Default HttpClient配置，如果需自定义SDK defualt 配置可以使用
        /// </summary>
        public static CloudSdkBuilder ConfigureDefaultHttpClient(
            this CloudSdkBuilder builder, 
            string clientName = "CloudSdk", 
            Action<IHttpClientBuilder> action = null,
            bool customizeRetry = false
            )
        {
            var httpClientOption = new CloudSdkHttpClientOption(clientName);
            httpClientOption.UseCustomizedRetryPolicy = customizeRetry;
            if (action != null)
            {
                httpClientOption.ClientBuilderConfigureAction = action;
            }

            builder.AddModuleHttpClient(httpClientOption);
            builder.Services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.DefaultHttpClientName = clientName;
            });
            return builder;
        }

        [Obsolete("This method will be removed in 1.4.*, please use ConfigureDefaultHttpClient for customized default http client")]
        public static CloudSdkBuilder ConfigureHttpClient(this CloudSdkBuilder builder, Action<IHttpClientBuilder> action)
        {
            var httpClientOption = new CloudSdkHttpClientOption("CloudSdk")
            {
                ClientBuilderConfigureAction = action,
            };

            var clientName = builder.AddModuleHttpClient(httpClientOption);

            builder.Services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.DefaultHttpClientName = clientName;
            });

            return builder;
        }

        
        [Obsolete("This method will be removed in 1.4.*, please use ConfigureDefaultHttpClient for customized default http client")]
        public static CloudSdkBuilder ConfigureHttpClient(
            this CloudSdkBuilder builder,
            string httpClientName,
            Action<IHttpClientBuilder> action,
            bool customizeRetry = false
            )
        {
            var httpClientOption = new CloudSdkHttpClientOption(httpClientName)
            {
                ClientBuilderConfigureAction = action,
                UseCustomizedRetryPolicy = customizeRetry
            };

            var clientName = builder.AddModuleHttpClient(httpClientOption);

            builder.Services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.DefaultHttpClientName = clientName;
                opt.UseCustomizedRetryPolicy = customizeRetry;
            });

            return builder;
        }

        public static CloudSdkBuilder ConfigureLoggingFields(this CloudSdkBuilder builder,
            string interfaceFiled = "CloudSdkService",
            string methodField = "CloudSdkMethod",
            string durationField = "CloudSdkDuration")
        {
            CloudSdkLoggingFields.Interface = interfaceFiled;
            CloudSdkLoggingFields.Method = methodField;
            CloudSdkLoggingFields.Duration = durationField;

            return builder;
        }

        public static CloudSdkBuilder ConfigureIdentityServer(this CloudSdkBuilder builder, string serverAddress, string clientId, string identityServerScope, bool isInternalIdentityServer = false)
        {
            builder.Services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.IsIdentityServerConfigured = true;
                opt.IdentityServerAddress = serverAddress;
                opt.IdentityServerClientId = clientId;
                opt.IdentityServerScope = identityServerScope;
                opt.IsInternalIdentityServer = isInternalIdentityServer;
            });

            // Identity Server专属的http client
            builder.AddModuleHttpClient(new CloudSdkHttpClientOption("CloudSdkIdentityServer")
            {
                UseCustomizedRetryPolicy = true,
                ClientBuilderConfigureAction = clientBuilder =>
                {
                    clientBuilder.AddPolicyHandler((serverProvider, request) =>
                    {
                        return HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(5)
                        });
                    });
                }
            });

            return builder;
        }


        public static CloudSdkBuilder AddCloudSdkHttpClient<TApiOption>(
            this CloudSdkBuilder builder,
            bool useDefaultHttpClient,
            CloudSdkClientConfiguration configuration) where TApiOption : ApiOptionBase
        {
            builder.Services.Configure<TApiOption>(opt =>
            {
                opt.DefaultApiUrl = configuration.DefaultApiUrl;
                opt.UseIdentityServer = configuration.UseIdentityServer;
                opt.UseDefaultHttpClient = useDefaultHttpClient;
            });

            //通常情况下configuration.HttpClientOption不可能为null，仅用于避免当SDK外部有自定义调用时配置错误的情况
            if (!useDefaultHttpClient && configuration.HttpClientOption != null)
            {
                var httpClientOption = configuration.HttpClientOption;
                builder.AddModuleHttpClient(httpClientOption);
                builder.Services.Configure<TApiOption>(opt =>
                {
                    opt.HttpClientName = httpClientOption.ClientName;
                    opt.UseCustomizedRetryPolicy = httpClientOption.UseCustomizedRetryPolicy;
                });
            }

            return builder;
        }

        /// <summary>
        /// SDK初始化module httpclient使用
        /// </summary>
        public static string AddModuleHttpClient(
           this CloudSdkBuilder builder,
           CloudSdkHttpClientOption clientOption)
        {
            var clientBuilder = builder.Services.AddHttpClient(clientOption.ClientName, client =>
            {
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
                client.Timeout = TimeSpan.FromMinutes(10);
            });
            // 默认设置client handler为64并发 
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    MaxConnectionsPerServer = 64,
                }.ConfigProxy();
            });

            clientOption.ClientBuilderConfigureAction(clientBuilder);

            if (!clientOption.UseCustomizedRetryPolicy)
            {
                clientBuilder.ConfigureDefualtRetryPolicy();
            }
            return clientOption.ClientName;
        }

        private static void ConfigureDefualtRetryPolicy(this IHttpClientBuilder clientBuilder)
        {
            var defaultRetryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new[]
                 {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(60)
                    });

            var getTokenRetryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new[]
                {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5)
                    });

            // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
            clientBuilder.AddPolicyHandler((serverProvider, request) =>
            {
                var requestUri = request.RequestUri.ToString();
                // get token类的http request执行较短的retry方案
                if (request.Properties.ContainsKey(ApiClientBase.GET_CLOUD_IDENTITY_TOKEN_PROPERTY_KEY))
                {
                    return getTokenRetryPolicy;
                }

                return defaultRetryPolicy;
            });
        }
    }
}
