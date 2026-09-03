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
using Google.Apis.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

#nullable enable

namespace AvePoint.RAI.Core.Auth
{
    /// <summary>
    /// Custom HTTP client factory for Google APIs with proxy support
    /// </summary>
    public class CustomHttpClientFactory : Google.Apis.Http.HttpClientFactory
    {
        /// <summary>
        /// Create HTTP message handler with proxy configuration
        /// </summary>
        /// <param name="args">HTTP client creation arguments</param>
        /// <returns>HTTP message handler</returns>
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            try
            {
                string developmentJson = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "Proxy.json");
                if (!File.Exists(developmentJson))
                {
                    // Return default handler if no proxy configuration found
                    return base.CreateHandler(args);
                }

                LocalProxy? proxyConfig = null;
                using (StreamReader stream = new(developmentJson))
                {
                    string proxyJson = stream.ReadToEnd();
                    if (!string.IsNullOrEmpty(proxyJson))
                    {
                        proxyConfig = JsonConvert.DeserializeObject<LocalProxy>(proxyJson);
                    }
                }

                if (proxyConfig == null || string.IsNullOrEmpty(proxyConfig.Host))
                {
                    // Return default handler if proxy configuration is invalid
                    return base.CreateHandler(args);
                }

                WebProxy proxy = new(proxyConfig.Host, true)
                {
                    Credentials = new NetworkCredential(proxyConfig.Account, proxyConfig.Password)
                };

                HttpClientHandler handler = new()
                {
                    UseProxy = true,
                    Proxy = proxy,
                    // Critical: Disable certificate revocation checking for proxy environment
                    CheckCertificateRevocationList = false,
                    // Configure SSL settings for proxy connections
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    UseCookies = false,
                    PreAuthenticate = false,
                    AutomaticDecompression = DecompressionMethods.None
                };

                return handler;
            }
            catch (Exception)
            {
                // Fall back to default handler if proxy setup fails
                return base.CreateHandler(args);
            }
        }

        /// <summary>
        /// Local proxy configuration model
        /// </summary>
        public class LocalProxy
        {
            /// <summary>
            /// Proxy host URL
            /// </summary>
            public string Host { get; set; } = string.Empty;

            /// <summary>
            /// Proxy account username
            /// </summary>
            public string Account { get; set; } = string.Empty;

            /// <summary>
            /// Proxy account password
            /// </summary>
            public string Password { get; set; } = string.Empty;
        }
    }
}
