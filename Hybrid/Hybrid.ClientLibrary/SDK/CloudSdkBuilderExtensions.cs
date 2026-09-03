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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AvePoint.Hybrid.ClientLibrary.SDK
{
    public static class CloudSdkBuilderExtensions
    {
        public static CloudSdkBuilder AddHybridAgentApi(this CloudSdkBuilder builder,
            string defaultApiUrl = null)
        {

            builder.AddCloudSdkHttpClient<HybridAgentApiOption>(true, new CloudSdkClientConfiguration
            {
                DefaultApiUrl = defaultApiUrl,
                UseIdentityServer = true
            });

            return builder.AddServices();

        }


        public static CloudSdkBuilder AddHybridAgentApi(this CloudSdkBuilder builder, CloudSdkClientConfiguration configuration)
        {
            if (configuration.HttpClientOption == null)
            {
                configuration.HttpClientOption = new CloudSdkHttpClientOption("HybridAgent");
            }

            builder.AddCloudSdkHttpClient<HybridAgentApiOption>(false, configuration);
            return builder.AddServices();
        }

        private static CloudSdkBuilder AddServices(this CloudSdkBuilder builder)
        {
            builder.Services.TryAdd(ServiceDescriptor.Singleton<ICloudSdkHybridAgentClientFactory, CloudSdkHybridAgentClientFactory>());
            return builder;
        }
    }
}
