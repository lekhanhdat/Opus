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
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using Microsoft.Extensions.Options;

    public interface ICloudSdkHttpClientFactory
    {
        HttpClient HttpClient { get; }
        HttpClient GetDefaultClient();
        HttpClient GetClientByName(string clientName);
    }

    internal class CloudSdkHttpClientFactory : ICloudSdkHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string defaultHttpClientName;
        private readonly ConcurrentDictionary<string, HttpClient> httpClients;

        public CloudSdkHttpClientFactory(IHttpClientFactory httpClientFactory, IOptions<CloudSdkCoreOptions> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.defaultHttpClientName = options.Value.DefaultHttpClientName;
            this.httpClients = new ConcurrentDictionary<string, HttpClient>();
        }

        public HttpClient HttpClient => GetClientByName(defaultHttpClientName);

        public HttpClient GetDefaultClient()
        {
            return GetClientByName(defaultHttpClientName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="needCheckProxy">临时的解决办法，以后应该需要重构，不用传入needCheckProxy，在外围调用的时候，就配置好httpclient的参数设置</param>
        /// <returns></returns>
        public HttpClient GetClientByName(string clientName)
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (httpClients.TryGetValue(clientName, out var client))
            {
                return client;
            }

            HttpClient clientInstance = httpClientFactory.CreateClient(clientName);

            //var clientInstance = httpClientFactory.CreateClient(clientName);

            if (httpClients.TryAdd(clientName, clientInstance))
            {
                return clientInstance;
            }
            // ConcurrentDictionary.TryAdd返回false的话，则说明字典已存在对应key-value
            // 此时应返回缓存的HttpClient instance, 并销毁新生成的httpclient instance
            //if (!needCheckProxy)
                clientInstance.Dispose();
            return httpClients[clientName];
        }
    }
}
