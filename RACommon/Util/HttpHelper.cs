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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class HttpHelper
    {
        public static HttpClient client = new HttpClient { Timeout = new TimeSpan(0, 3, 0) };
        #region app
        public static readonly TimeSpan DefaultHttpTimeout = new TimeSpan(0, 3, 0);

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(HttpHelper));

        private static readonly TimeSpan HttpRetryInterval = TimeSpan.FromSeconds(60);
        private static readonly int HttpRetryTimes = 2;
        private static readonly string UserAgentHeaderId = "User-Agent";

        private static readonly AveRetryStrategy HttpRetryStrategy = new FixedIntervalRetryStrategy(HttpRetryTimes, HttpRetryInterval);
        private static readonly AveRetryPolicy HttpRetryPolicy = new AveRetryPolicy(new HttpTransientErrorDetectionStrategy(), HttpRetryStrategy);

        #endregion
        public static string Delete(string uri, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return content;
        }

        public static string Get(string uri, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, SecurityUtils.SanitizeRequestUrl(uri));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return content;
        }

        public static async Task<string> GetAsync(string uri, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, SecurityUtils.SanitizeRequestUrl(uri));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        public static async Task<Byte[]> GetByteAsync(string uri, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, SecurityUtils.SanitizeRequestUrl(uri));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            return content;
        }
        public static string Post(string uri, string param, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(param, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return content;
        }

        public static async Task<string> PostAsync(string uri, string param, string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(param, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request);
            Logger.Warn($"Post result: [{response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase ?? ""}]");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static string Patch(string uri, string param, string accessToken)
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri)
            {
                Content = new StringContent(param, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return content;
        }
        #region
        public static string HttpGet(string accessToken, string requestUri)
        {
            HttpResponseHeaders responseHeaders = null;
            //using (var client = new HttpClient() { Timeout = DefaultHttpTimeout })
            //{
                return HttpGet(client, accessToken, requestUri, out responseHeaders);
            //}
        }

        public static string HttpGet(HttpClient client, string accessToken, string requestUri)
        {
            HttpResponseHeaders responseHeaders = null;
            return HttpGet(client, accessToken, requestUri, out responseHeaders);
        }

        public static string HttpGet(HttpClient client, string accessToken, string requestUri, out HttpResponseHeaders responseHeaders)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            if (!request.Headers.Contains(UserAgentHeaderId))
            {
                request.Headers.Add(UserAgentHeaderId, string.Format("ISV|AvePoint|CloudRecords/{0}", 1.0));
            }

            var response = HttpRetryPolicy.ExecuteAction(() => client.SendAsync(request).GetAwaiter().GetResult());
            responseHeaders = response.Headers;
            var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                return result;
            }
            else
            {
                throw new Exception(string.Format("Get {0} failed, StatusCode {1}, result {2}", requestUri, (int)response.StatusCode, result));
            }
        }

        //public static async Task<string> HttpGetAsync(string accessToken, string requestUri)
        //{
        //    //using (var client = new HttpClient() { Timeout = DefaultHttpTimeout })
        //    //{
        //        return await HttpGetAsync(client, accessToken, requestUri);
        //    //}
        //}

        //public static async Task<string> HttpGetAsync(HttpClient client, string accessToken, string requestUri)
        //{
        //    return await HttpRetryPolicy.ExecuteAsync(async () =>
        //    {
        //        return await HttpGetAsyncInternal(client, accessToken, requestUri);
        //    }, HttpRetryTimes, HttpRetryInterval);
        //}

        //public static async Task<string> HttpGetAsyncInternal(HttpClient client, string accessToken, string requestUri)
        //{
        //    if (!string.IsNullOrEmpty(accessToken))
        //    {
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    }
        //    if (!client.DefaultRequestHeaders.Contains(UserAgentHeaderId))
        //    {
        //        client.DefaultRequestHeaders.Add(UserAgentHeaderId, string.Format("ISV|AvePoint|CloudRecords/{0}", 1.0));//RMGlobalConfiguration.EnvSetting.ProductVersion));
        //    }
        //    var response = await client.GetAsync(requestUri);
        //    var result = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return result;
        //    }
        //    else
        //    {
        //        throw new Exception(string.Format("Get {0} failed, StatusCode {1}, result {2}", requestUri, (int)response.StatusCode, result));
        //    }
        //}
        #endregion
    }
}
