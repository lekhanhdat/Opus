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
namespace Microsoft365.Common.HttpUtil
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// this factory used to create short term HttpClient used in product.
    /// </summary>
    public class HttpClientFactory
    {
        private static IHttpMessageHandlerFactory httpMessageHandlerFactory;
        private static IServiceProvider provider;
        static HttpClientFactory()
        {
            IServiceCollection services = new ServiceCollection().AddHttpClient();
            provider = services.BuildServiceProvider();
            httpMessageHandlerFactory = provider.GetService<IHttpMessageHandlerFactory>();
        }

        private static HttpMessageHandler CreateClientHandler(string name)
        {
            return httpMessageHandlerFactory.CreateHandler(name);
        }

        /// <summary>
        /// create a HttpClient with a message handler provided by IHttpClientFactory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="retryStrategies"></param>
        /// <param name="customHandlers"></param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(string name,IList<IRetryStrategy> retryStrategies = null, DelegatingHandler[] customHandlers=null)
        {
            var messageHandler = CreateClientHandler(name);
            return CreateClientInternal(messageHandler,false, retryStrategies, customHandlers);
        }

        /// <summary>
        /// create a HttpClient with customized message handler
        /// </summary>
        /// <param name="retryStrategies">used in retry handler for created httpclient</param>
        /// <param name="customHandlers"></param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(HttpMessageHandler customMessagetHandler, bool disposeHandler, IList<IRetryStrategy> retryStrategies = null, DelegatingHandler[] customHandlers = null)
        {
            ArgumentNullException.ThrowIfNull(customMessagetHandler);
            return CreateClientInternal(customMessagetHandler, disposeHandler,retryStrategies, customHandlers);
        }

        private static HttpClient CreateClientInternal(HttpMessageHandler defaultHandler,bool disposeHandler, IList<IRetryStrategy> retryStrategies, DelegatingHandler[] handlers)
        {
            HttpMessageHandler currentHandler = defaultHandler;
            if (retryStrategies != null && retryStrategies.Any())
            {
                currentHandler = new DefaultRetryHandler(currentHandler, retryStrategies);
            }
            if (handlers != null)
            {
                for (int i = handlers.Length - 1; i >= 0; --i)
                {
                    DelegatingHandler handler = handlers[i];
                    // Non-delegating handlers are ignored since we always 
                    // have RetryDelegatingHandler as the outer-most handler
                    while (handler.InnerHandler is DelegatingHandler)
                    {
                        handler = handler.InnerHandler as DelegatingHandler;
                    }

                    handler.InnerHandler = currentHandler;
                    currentHandler = handlers[i];
                }
            }
            return new HttpClient(currentHandler, disposeHandler);
        }
    }

    public static class RestClientFactory
    {
        private const string ClientName = "SharePointRest";
        public static IList<IRetryStrategy> DefaultStrategies = new List<IRetryStrategy>
            {
                new ToomanyRequestRetryStrategy(
                    new ToomanyRequestRetryOption(
                        TimeSpan.FromMinutes(15),
                        TimeSpan.FromMinutes(60),
                        TimeSpan.FromMinutes(2),
                        int.MaxValue)
                    ),
                new FixedIntervalRetryStrategy(
                    TimeSpan.FromMilliseconds(500),
                    4) };
        /// <summary>
        /// create a short term httpclient used for sharepoint relative requests, this Client will have UserAgent on it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HttpClient CreateSharePointRestClient(string name)
        {
            name = string.IsNullOrEmpty(name) ? ClientName : name;
            return HttpClientFactory.CreateHttpClient(name, DefaultStrategies).WithUserAgent();
        }


        private static HttpClient WithUserAgent(this HttpClient client)
        {
            if (!string.IsNullOrEmpty(Microsoft365.Configuration.Microsoft365Configuration.CommonConfiguration.UserAgent))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Microsoft365.Configuration.Microsoft365Configuration.CommonConfiguration.UserAgent);
            }
            return client;
        }
    }
}
