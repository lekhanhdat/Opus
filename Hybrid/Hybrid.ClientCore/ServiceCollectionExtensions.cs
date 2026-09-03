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
namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using AvePoint.Hybrid.ClientCore;
    using AvePoint.Hybrid.ClientCore.Logging;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtensions
    {
        public static CloudSdkBuilder AddHybridCloudSdk(this IServiceCollection services,
            string product,
            X509Certificate2 communicationCert)
        {
            return AddHybridCloudSdk(services, product, null, communicationCert);
        }

        public static CloudSdkBuilder AddHybridCloudSdk(this IServiceCollection services,
            string product,
            string vCloudProduct,
            X509Certificate2 communicationCert)
        {
            //remove activity trace temporaly and add back later.
            //Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            var builder = new CloudSdkBuilder(services);

            services.AddSingleton<ApiMemoryCache>();
            services.AddSingleton<ISdkLogger>(p => new DefaultSdkLogger());
            services.TryAdd(ServiceDescriptor.Singleton<ICloudSdkHttpClientFactory, CloudSdkHttpClientFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<ICloudSdkIdentityServerTokenService, CloudSdkIdentityServerTokenService>());

            services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.Product = product;
                opt.VCloudProduct = vCloudProduct;
                opt.CommunicationCertificate = communicationCert;
            });

            // 配置默认cloud sdk的http client
            builder.ConfigureDefaultHttpClient(clientName: "CloudSdkDefault");

            return builder;
        }

        public static CloudSdkBuilder AddPublicApiCloudSdk(this IServiceCollection services,
            string product,
            X509Certificate2 communicationCert)
        {
            return AddPublicApiCloudSdk(services, product, null, communicationCert);
        }

        public static CloudSdkBuilder AddPublicApiCloudSdk(this IServiceCollection services,
            string product,
            string vCloudProduct,
            X509Certificate2 communicationCert)
        {
            //remove activity trace temporaly and add back later.
            //Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            var builder = new CloudSdkBuilder(services);

            services.AddSingleton<ApiMemoryCache>();
            services.AddSingleton<ISdkLogger>(p => new DefaultSdkLogger());
            services.TryAdd(ServiceDescriptor.Singleton<ICloudSdkHttpClientFactory, CloudSdkHttpClientFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<ICloudSdkIdentityServerTokenService, CloudSdkIdentityServerTokenService>());

            services.Configure<CloudSdkCoreOptions>(opt =>
            {
                opt.Product = product;
                opt.VCloudProduct = vCloudProduct;
                opt.CommunicationCertificate = communicationCert;
            });

            // 配置默认cloud sdk的http client
            builder.ConfigureDefaultHttpClient(clientName: "CloudSdkDefault");

            return builder;
        }
    }
}
