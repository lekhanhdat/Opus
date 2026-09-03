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
namespace AvePoint.Hybrid.ClientCore.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using AvePoint.Hybrid.ClientCore.Logging;
    using AvePoint.RA.Contract.Tenant;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public abstract class ApiClientBase
    {
        public static string GET_CLOUD_IDENTITY_TOKEN_PROPERTY_KEY = "GET_CLOUD_IDENTITY_TOKEN_REQUEST";
        private static readonly string hostName;

        protected ILogger logger;
        protected ISdkLogger _logger;
        protected readonly string httpClientName;
        protected readonly CloudSdkCoreOptions coreOptions;
        protected readonly ApiOptionBase apiOptionBase;

        private readonly ICloudSdkIdentityServerTokenService tokenService;
        private readonly ICloudSdkHttpClientFactory cloudSdkHttpClientFactory;

        public string HybridAgentId { get; set; }

        static ApiClientBase()
        {
            try
            {
                hostName = Dns.GetHostName() ?? Environment.MachineName;
            }
            // 不留catch exception, 仅异常时获取machine name
            catch
            {
                hostName = Environment.MachineName;
            }
        }

        public ApiClientBase(
            CloudSdkCoreOptions coreOption,
            ICloudSdkHttpClientFactory cloudSdkHttpClientFactory,
            ApiOptionBase apiOptionBase,
            ICloudSdkIdentityServerTokenService tokenService)
        {
            this.coreOptions = coreOption;
            this.cloudSdkHttpClientFactory = cloudSdkHttpClientFactory;
            this.apiOptionBase = apiOptionBase;
            this.tokenService = tokenService;
            // client初始化的时候确定retry policy和http client name
            if (apiOptionBase.UseDefaultHttpClient)
            {
                httpClientName = coreOption.DefaultHttpClientName;
                UseCustomizedRetryPolicy = coreOption.UseCustomizedRetryPolicy;
            }
            else
            {
                httpClientName = apiOptionBase.HttpClientName;
                UseCustomizedRetryPolicy = apiOptionBase.UseCustomizedRetryPolicy;
            }
        }

        public HttpClient HttpClient => cloudSdkHttpClientFactory.GetClientByName(httpClientName);
        public bool UseCustomizedRetryPolicy { get; private set; }
        public virtual string IdentityServerScope { get; set; }
        public virtual JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();

        protected abstract string BaseUrl { get; }
        protected virtual Uri GetRequestUrl(string path)
        {
            var url = $"{BaseUrl}/{path.TrimEnd('/')}";
            return new Uri(url);
        }

        public TInterface CreateServiceProxy<TInterface>() => ApiClientProxy<TInterface>.Create(this, logger, _logger);

        public Task<TResponse> PostAsync<TRequest, TResponse>(
            string relativePath,
            TRequest requestBody,
            string operationName = null,
            Dictionary<string, object> routeParameters = null)
        {
            return InvokeAsync<TResponse>(HttpMethod.Post, relativePath, requestBody, routeParameters, operationName);
        }

        public Task<TResponse> PostAsync<TResponse>(
           string relativePath,
           string operationName = null,
           Dictionary<string, object> routeParameters = null)
        {
            return InvokeAsync<TResponse>(HttpMethod.Post, relativePath, null, routeParameters, operationName);
        }

        public Task<TResponse> GetAsync<TResponse>(
            string relativePath,
            Dictionary<string, object> queryParameters = null,
            string operationName = null)
        {
            return InvokeAsync<TResponse>(HttpMethod.Get, relativePath, null, queryParameters, operationName);
        }

        public async Task<TResponse> InvokeAsync<TResponse>(
            HttpMethod httpMethod,
            string relativePath,
            object requestBody = null,
            Dictionary<string, object> parameters = null,
            string operationName = null)
        {
            if (httpMethod == null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            var traceContext = new TraceContext(GetType().FullName, operationName ?? $"{httpMethod.Method}:{relativePath}");

            try
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = httpMethod;
                    await AssembleRequestHeaders(request);

                    var isRequestBodyMethod = IsRequestBodyMethod(httpMethod);
                    request.RequestUri = GetRequestUrl(isRequestBodyMethod, relativePath, parameters, out _);

                    if (isRequestBodyMethod && requestBody != null)
                    {
                        request.Content = CreateRequestContent(requestBody);
                    }

                    _logger?.Info($"Sending direct Public API request to endpoint: {request.RequestUri}, traceId: {traceContext.TraceId}");

                    using (var response = await SendAsync(traceContext, request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new CloudApiException(content)
                            {
                                ErrorCode = (int)response.StatusCode,
                                TraceContext = traceContext
                            };
                        }

                        return await ReadResponseJson<TResponse>(traceContext, response);
                    }
                }
            }
            catch (CloudApiException apiEx)
            {
                logger.LogError(apiEx, apiEx.Message);
                _logger?.Error(apiEx, apiEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed Invoke");
                _logger?.Error(ex, "Failed Invoke");
                throw;
            }
        }

        public virtual Uri GetRequestUrl(bool isPost, string path, Dictionary<string, object> parameters, out List<string> urlParsedParams)
        {
            urlParsedParams = null;
            var url = $"{BaseUrl.TrimEnd('/')}/{path.TrimEnd('/')}".ToLower();
            if (parameters != null)
            {
                var parsedParam = new List<string>();
                var keys = parameters.Keys.ToList();

                foreach (var kv in parameters)
                {
                    var holder = "{" + kv.Key.ToLower() + "}";
                    if (url.Contains(holder))
                    {
                        url = url.Replace(holder, HttpUtility.UrlEncode(kv.Value.ToString()).Replace("+", "%20"));
                        parsedParam.Add(kv.Key);
                    }
                }
                urlParsedParams = parsedParam;
                var leftParams = parameters.Where(i => !parsedParam.Contains(i.Key));
                if (!isPost && leftParams.Count() > 0)
                {
                    var tempParamsList = new List<String>();
                    foreach (var item in leftParams)
                    {
                        if (item.Value != null)
                        {
                            tempParamsList.Add($"{item.Key}={HttpUtility.UrlEncode(item.Value.ToString())}");
                        }
                    }
                    url = $"{url}?{ string.Join("&", tempParamsList.ToArray())}";
                }
            }
            return new Uri(url);
        }

        public async Task<T> ReadResponseJson<T>(TraceContext traceContext, HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (typeof(T).Name == "String")
                    {
                        object obj = content;
                        return (T)obj;
                    }
                    //else
                    //{
                    //    logger?.LogWarning("Return type is "+ typeof(T).Name);
                    //    _logger?.Warn("Return type is " + typeof(T).Name);
                    //}
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(content);
                    }
                    catch (Exception ex)
                    {
                        throw new CloudApiException(content, ex) { TraceContext = traceContext };
                    }
                }
                else
                {
                    logger?.LogWarning("Request succeed. Response Body is empty");
                    _logger?.Warn("Request succeed. Response Body is empty");
                }
            }
            else
            {
                throw new CloudApiException(content) { ErrorCode = (int)responseMessage.StatusCode };
            }

            return default;
        }

        public async Task<HttpResponseMessage> SendAsync(TraceContext traceContext, HttpRequestMessage msg)
        {
            msg.Headers.Add("Product", coreOptions.Product);
            if (!string.IsNullOrEmpty(traceContext.RequestId))
            { 
                msg.Headers.Add("CloudSDK-RequestId", traceContext.RequestId); 
            }
                
            msg.Headers.Add("CloudSDK-ClientHost", hostName);

            if (!string.IsNullOrEmpty(traceContext.ActivityId))
            {
                msg.Headers.Add("traceparent", traceContext.ActivityId);
            }

            if (!string.IsNullOrWhiteSpace(TenantAgentInfo.JobId))
            {
                msg.Headers.Add("Agent-Job-Id", TenantAgentInfo.JobId);
            }

            if (!string.IsNullOrWhiteSpace(HybridAgentId))
            {
                msg.Headers.Add("Hybrid-Agent-Id", HybridAgentId);
            }
            if (!string.IsNullOrWhiteSpace(traceContext.TraceId))
            {
                msg.Headers.Add("TraceId", traceContext.TraceId);
            }
            if (!string.IsNullOrWhiteSpace(TenantAgentInfo.TenantRegisterEmail))
            {
                msg.Headers.Add("UserName", TenantAgentInfo.TenantRegisterEmail);
            }
            else if(!string.IsNullOrWhiteSpace(TenantLocalValue.LogonUserEmail))
            {
                msg.Headers.Add("UserName", TenantLocalValue.LogonUserEmail);
            }
            if(!string.IsNullOrWhiteSpace(TenantLocalValue.LogonUserId))
            {
                msg.Headers.Add("uid", TenantLocalValue.LogonUserId);
            }
            if (!string.IsNullOrWhiteSpace(TenantLocalValue.MultiGeoIP))
            {
                msg.Headers.Add("X-MultiGeo-IP", TenantLocalValue.MultiGeoIP);
            }

            logger.LogInformation("Invoke: {0}", traceContext);
            _logger.Info("Invoke: {0}", traceContext);
            // response实现了IDsiposable接口，交给外层进行dispose，这里不做额外处理
            lock (debugTimeoutLock)
            {
                if (debugTimeout)
                {
                    HttpClient.Timeout = TimeSpan.FromHours(1);
                    debugTimeout = false;
                }
            }
            var result = await HttpClient.SendAsync(msg);
            logger.LogInformation("Invoke Finished. {0}", traceContext.ToFinalString());
            _logger.Info("Invoke Finished. {0}", traceContext.ToFinalString());
            return result;
        }
        private static bool debugTimeout = false;
        private static readonly object debugTimeoutLock = new object();

        protected virtual HttpContent CreateRequestContent(object requestBody)
        {
            if (requestBody is HttpContent httpContent)
            {
                return httpContent;
            }

            var serializedContent = JsonConvert.SerializeObject(requestBody, JsonSerializerSettings);
            return new StringContent(serializedContent, Encoding.UTF8, "application/json");
        }

        private static bool IsRequestBodyMethod(HttpMethod httpMethod)
        {
            return string.Equals(httpMethod.Method, "POST", StringComparison.OrdinalIgnoreCase)
                || string.Equals(httpMethod.Method, "PUT", StringComparison.OrdinalIgnoreCase);
        }


        protected virtual Task<string> GetIdentityServerToken(string tenantId = null, string HybridAgentId = null, string HybridAgentAuth = null) => tokenService.GetIdentityServerToken(IdentityServerScope, tenantId, HybridAgentId, HybridAgentAuth);

        protected virtual Task<bool> TrySetIdentityServerTokenAsync(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme = AuthenticationHeaderScheme.Bearer, string tenantId = null) => tokenService.TrySetIdentityServerTokenAsync(apiOptionBase, IdentityServerScope, request, tokenScheme, tenantId);

        protected virtual void SetIdentityServerToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token) => tokenService.SetIdentityServerToken(request, tokenScheme, token);

        protected virtual void SetCloudIdentityToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token) => tokenService.SetCloudIdentityToken(request, tokenScheme, token);

        public abstract Task AssembleRequestHeaders(HttpRequestMessage request);
    }
}
