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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using HybridCommonModel.DataModel;
using HybridCommonModel.Utils;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Utility.Net
{
    public class AveHttpConnectionUtil
    {
        private const string NetworkHost = @"https://www.avepointonlineservices.com";
        private static readonly IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Check if the proxy setting is work.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<bool> TestWebProxyAsync(AveWebProxyOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            var client = AveHttpClient.Create(options);
            return await TestConnectionAsync(client);
        }

        private static async Task<bool> TestConnectionAsync(HttpClient httpClientWithProxy)
        {
            var result = false;
            try
            {
                HttpResponseMessage response = await httpClientWithProxy.GetAsync(NetworkHost);
                result = response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while test proxy connection. error : {e.ToString()}");
            }

            logger.Info($"Test proxy connection result : {result}");
            return result;
        }

        /// <summary>
        /// Return a single instance of HttpClient object if not using proxy, otherwise, Create a intance of HttpClient object with proxy.
        /// as long as the proxy isn't modified, the single intance will be returned.
        /// Please do not dispose the returned instance in your code.
        /// Please note that, it will return null if an incorrect proxy is used.
        /// </summary>
        /// <param name="needCheckProxy">indicate whether to check the proxy, default is true</param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(bool needCheckProxy = true)
        {
            if (!needCheckProxy) return AveHttpClient.Create(); // no need to check proxy

            var proxySetting = AveWebProxyUtil.ReadProxySetting();
            if (proxySetting == null) return AveHttpClient.Create(); //no proxy

            var httpClientWithProxy = AveHttpClient.Create(proxySetting);
            var testResult = TestConnectionAsync(httpClientWithProxy).Result;
            return testResult ? httpClientWithProxy : null;
        }

        /// <summary>
        /// create a proxy http client
        /// </summary>
        /// <param name="proxySetting"></param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(AveWebProxyOptions proxySetting)
        {
            var httpClientWithProxy = AveHttpClient.Create(proxySetting);
            var testResult = TestConnectionAsync(httpClientWithProxy).Result;
            return testResult ? httpClientWithProxy : null;
        }
    }
}
