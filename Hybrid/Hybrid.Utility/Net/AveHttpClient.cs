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
using HybridCommonModel.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AvePoint.Hybrid.Utility.Net
{
    internal class AveHttpClient
    {
        private static int _timeoutSeconds = 30; //30 seconds
        private readonly static object lockObj = new object();

        private static HttpClient _httpClient;
        private static HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    lock (lockObj)
                    {
                        if (_httpClient == null)
                        {
                            _httpClient = new HttpClient(new HttpClientHandler() { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
                        }
                    }
                }
                return _httpClient;
            }
        }


        private static Dictionary<string, HttpClient> _proxyHttpClientsDic = new Dictionary<string, HttpClient>();


        /// <summary>
        /// Create a intance of HttpClient object with proxy.
        /// as long as the proxy isn't modified, the intance returned will be the same.
        /// Please do not dispose the instance returned in your code.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpClient Create(AveWebProxyOptions options)
        {
            var key = JsonConvert.SerializeObject(options);
            lock (lockObj)
            {
                if (!_proxyHttpClientsDic.ContainsKey(key))
                {
                    _proxyHttpClientsDic.Clear(); //always keep the latest, remove the old ones.

                    HttpClientHandler handler = new HttpClientHandler();
                    handler.ConfigProxy(options);
                    
                    _proxyHttpClientsDic[key] = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
                }

                return _proxyHttpClientsDic[key];
            }

            //var proxy = new AveWebProxy(options);

            //HttpClientHandler handler = new HttpClientHandler()
            //{
            //    Proxy = proxy.Create(),
            //    UseProxy = true,
            //};

            //return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
        }

        /// <summary>
        /// Create a single instance of HttpClient object.
        /// Please do not dispose the instance returned in your code.
        /// </summary>
        /// <returns></returns>
        public static HttpClient Create()
        {
            return Client;
        }

    }
}
