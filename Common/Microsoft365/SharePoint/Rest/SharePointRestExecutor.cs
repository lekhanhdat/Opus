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

using Microsoft365.Authentication.TokenProvider;
using Microsoft365.Common.Logger;
using Microsoft365.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.SharePoint.Rest
{
    internal class SharePointRestExecutor
    {
        private readonly string siteUrl;
        protected IATokenProvider TokenProvider { get; set; }
        public bool RequireAdmin { get; private set; }
        private static HttpClient client;
        private static IMicrosoft365Logger Logger => Microsoft365LoggerManager.CreateLogger(typeof(SharePointRestExecutor));

        public string MaxDataServiceVersion { get; set; }

        static SharePointRestExecutor()
        {
            client = new HttpClient(new HttpMessageHandlerWithRetry());
            client.Timeout = TimeSpan.FromMinutes(10);
            InitDefaultHeaders(client.DefaultRequestHeaders);
        }

        private static void InitDefaultHeaders(HttpRequestHeaders headers)
        {
            headers.ConnectionClose = false;
            headers.Connection.Add("Keep-Alive");
            headers.UserAgent.TryParseAdd(Microsoft365Configuration.CommonConfiguration.UserAgent);

            headers.Accept.Clear();
            //Do not use verbose json for now, SharePoint online Odata version is 3.0, prefer the new JSON format
            //Do not change default headers to verbose json, the data format will change, which could cause compatibility issue.
            headers.Accept.TryParseAdd("application/json");//;odata=verbose
        }

        public SharePointRestExecutor(string siteUrl, IATokenProvider tokenProvider, bool requireAdmin = false)
        {
            this.siteUrl = siteUrl;
            this.TokenProvider = tokenProvider;
            this.RequireAdmin = requireAdmin;
        }

        public static bool EnableLogging { get; set; } = true;

        private static void Log(string message)
        {
            if (EnableLogging)
            {
                Logger.Info(message);
            }
        }
        private static void LogError(string message)
        {
            Logger.Error(message);
        }
        public static void AuthenticateRequest(HttpRequestMessage request, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public HttpResponseMessage Execute(Uri uri, HttpMethod method, HttpContent content, Dictionary<string, string> headers)
        {
            HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(uri, method, content, headers);
            return client.SendAsync(httpRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private HttpRequestMessage GetHttpRequestMessage(Uri uri, HttpMethod method, HttpContent content, Dictionary<string, string> headers)
        {
            var token = TokenProvider.GetSharePointToken(new Uri(this.siteUrl).GetLeftPart(UriPartial.Authority), SPTokenType.ApplicationBear, SPUserType.Adaptation) ??
                            TokenProvider.GetSharePointToken(this.siteUrl, SPTokenType.DelegateBear, this.RequireAdmin ? SPUserType.ServiceAccount : SPUserType.Adaptation);
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = content
            };
            headers?.ToList().ForEach(t => httpRequestMessage.Headers.TryAddWithoutValidation(t.Key, t.Value));
            AddPreDefinedHeaders(httpRequestMessage);
            AuthenticateRequest(httpRequestMessage, token.AccessToken);
            return httpRequestMessage;
        }

        private void AddPreDefinedHeaders(HttpRequestMessage httpRequestMessage)
        {
            if (this.MaxDataServiceVersion != null)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("MaxDataServiceVersion", this.MaxDataServiceVersion);
            }
        }

        public T Execute<T>(Uri uri, HttpMethod method, HttpContent content, Dictionary<string, string> headers = null)
        {
            //https://github.com/aspnet/Security/issues/886
            var response = Execute(uri, method, content, headers);
            Log($@"Call SharePoint rest api, {method} {uri}
{response}");
            var result = response.Content.ReadAsStringAsync().GetResultEx();
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent) return default(T);
                return JsonConvert.DeserializeObject<T>(result);
            }
            //unreachable code for HttpMessageHandlerWithRetry, does not remove in case default httpclient is used
            LogError($@"Call SharePoint rest api, {method} {uri}
{result}");
            throw new SPRestException($"{method } {uri}", response.StatusCode, response.ReasonPhrase, result, $"{response.Headers}{response.Content.Headers}");
        }

        public T Execute<T>(Uri request, HttpMethod method, object obj, Dictionary<string, string> headers = null)
        {
            return Execute<T>(request, method, ToHttpContent(obj), headers);
        }

        public T Get<T>(Uri endpoint, Dictionary<string, string> headers = null)
        {
            return Execute<T>(endpoint, HttpMethod.Get, null, headers);
        }

        public T Post<T>(Uri endpoint, object obj, Dictionary<string, string> headers = null)
        {
            return Execute<T>(endpoint, HttpMethod.Post, obj, headers);
        }

        /// <summary>
        /// File is not buffered, instead is copied to target stream.
        /// Not performance issue for large file, and no size limit.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="target"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task DownloadStreamRequestAsync(Uri uri, Stream target, Dictionary<string, string> headers = null, CancellationToken token = default)
        {
            using (var request = GetHttpRequestMessage(uri, HttpMethod.Get, null, headers))
            {
                //return when response headers read
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    Log($@"Call SharePoint rest api, GET {uri}");
                    if (response.IsSuccessStatusCode)
                    {
                        //response.Content.Headers.ContentLength
                        var stream = await response.Content.ReadAsStreamAsync(token);
                        await stream.CopyToAsync(target, token);
                    }
                    else//unreachable code for HttpMessageHandlerWithRetry, does not remove in case default httpclient is used
                    {
                        var error = await response.Content.ReadAsStringAsync(token);
                        LogError($@"Call SharePoint rest api, GET {uri}
{error}");
                        throw new SPRestException($"{HttpMethod.Get} {uri}", response.StatusCode, response.ReasonPhrase, error, $"{response.Headers}{response.Content.Headers}");
                    }
                }
            }
        }

        /// <summary>
        /// File is buffered in memory, by default max buffered length is 2GB, HttpClient.MaxResponseContentBufferSize
        /// There will be performance issue for large file, and failed when file is larger than 2GB
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<Stream> GetStreamRequestAsync(Uri uri, Dictionary<string, string> headers = null)
        {
            var request = GetHttpRequestMessage(uri, HttpMethod.Get, null, headers);
            //return when response stream read, and buffer in memorystream
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        private static HttpContent ToHttpContent(object obj)
        {
            if (obj == null) return null;
            return new StringContent(
                ConvertToJson(obj),
                Encoding.UTF8,
                "application/json");
        }

        private static string ConvertToJson(object obj)
        {
            var json =  JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Log($"SharePoint rest content:{obj.GetType().FullName}, {json}");
            return json;
        }
    }
}