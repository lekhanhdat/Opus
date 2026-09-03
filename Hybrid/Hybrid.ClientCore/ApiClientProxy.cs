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
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AvePoint.Hybrid.ClientCore.Logging;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ApiClientProxy<TInterface> : DispatchProxyAsync
    {
        private ApiClientBase client;
        private ILogger logger;
        private ISdkLogger _logger;

        public static TInterface Create(ApiClientBase client, ILogger logger, ISdkLogger sdkLogger)
        {
            object proxy = Create<TInterface, ApiClientProxy<TInterface>>();
            ((ApiClientProxy<TInterface>)proxy).client = client;
            ((ApiClientProxy<TInterface>)proxy).logger = logger;
            ((ApiClientProxy<TInterface>)proxy)._logger = sdkLogger;
            return (TInterface)proxy;
        }


        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            throw new NotSupportedException("Only Async methods supproted");
        }

        protected override async Task InvokeAsync(MethodInfo method, object[] args)
        {
            var traceContext = new TraceContext(method);

            try
            {
                // 这里针对task cancel的问题做了一次retry，如果外部设定为customized retry的话，则直接跳过retry的步骤
                using (var response = await InternalInvokeAsyncWithRetry(traceContext, client.UseCustomizedRetryPolicy, method, args))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        throw new CloudApiException(content) { ErrorCode = (int)response.StatusCode, TraceContext = traceContext };
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

        // isInRetry为true时，代表SDK内部不进行下一次手动retry，当以下两种case时，此值为true：
        // 1. SDK外部设置customized retry policy
        // 2. 已经有一次尝试失败
        private async Task<HttpResponseMessage> InternalInvokeAsyncWithRetry(TraceContext traceContext, bool isInRetry, MethodInfo method, object[] args)
        {
            try
            {
                return await InternalInvokeAsync(traceContext, method, args);
            }
            catch (TaskCanceledException ex)
            {
                if (!isInRetry)
                {
                    var msg = $"TaskCanceledException handled.{traceContext}";
                    traceContext.IsRetry = true;
                    logger.LogWarning(ex, msg);
                    _logger?.Warn(msg);
                    return await InternalInvokeAsyncWithRetry(traceContext, true, method, args);
                }
                else
                {
                    throw;
                }
            }
        }

        protected override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            var traceContext = new TraceContext(method);
            try
            {
                // 这里针对task cancel的问题做了一次retry，如果外部设定为customized retry的话，则直接跳过retry的步骤
                using (var response = await InternalInvokeAsyncWithRetry(traceContext, client.UseCustomizedRetryPolicy, method, args))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        throw new CloudApiException(content) { ErrorCode = (int)response.StatusCode, TraceContext = traceContext };
                    }
                    return await client.ReadResponseJson<T>(traceContext, response);
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

        private async Task<HttpResponseMessage> InternalInvokeAsync(TraceContext traceContext, MethodInfo method, object[] args)
        {
            var attr = method.GetCustomAttribute<ApiAttribute>();
            if (attr == null)
            {
                throw new InvalidCastException("No ApiAttribute");
            }

            using (var request = new HttpRequestMessage())
            {
                _logger?.Info($"Sending Public API request to endpoint: {attr.Url}, traceId: {traceContext.TraceId}");
                request.Method = GetHttpMethod(attr.HttpMethod);
                await client.AssembleRequestHeaders(request);

                var parameters = AssembleGetParams(method, args);
                if (attr.HttpMethod == "POST" || attr.HttpMethod == "PUT")
                {
                    request.RequestUri = client.GetRequestUrl(true, attr.Url, parameters, out var parsedParams);
                    if (parameters != null)
                    {
                        var remaindParams = parameters.Where(i => !parsedParams.Contains(i.Key)).ToDictionary(i => i.Key, i => i.Value);
                        request.Content = GenerateHttpContent(request.RequestUri, remaindParams);
                    }
                }
                else
                {
                    request.RequestUri = client.GetRequestUrl(false, attr.Url, parameters, out var parsedParams);
                }
                return await client.SendAsync(traceContext, request);
            }
        }

        private Dictionary<string, object> AssembleGetParams(MethodInfo targetMethod, object[] args)
        {
            if (args != null && args.Any())
            {
                var methodParmas = new Dictionary<string, object>();
                var paramInfos = targetMethod.GetParameters();
                for (var i = 0; i < paramInfos.Length; i++)
                {
                    var info = paramInfos[i];
                    methodParmas[info.Name] = args[i];
                }
                return methodParmas;
            }
            return null;
        }

        private HttpMethod GetHttpMethod(string methodString)
        {
            switch (methodString)
            {
                case "GET":
                    return HttpMethod.Get;
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "DELETE":
                    return HttpMethod.Delete;
                default: return HttpMethod.Get;
            }
        }

        /// <summary>
        /// List API endpoints need to send file streams.
        /// </summary>
        private HashSet<string> ListUriWithFormRequest = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/FSDiscovery/UploadAnalyzedFileToStorage"
        };

        private bool CheckFormRequestUri(string requestUri)
        {
            return ListUriWithFormRequest.Contains(requestUri);
        }

        private HttpContent GenerateHttpContent(Uri requestUri, Dictionary<string, object> paramters)
        {
            if (!paramters.Any()) return null;

            if (CheckFormRequestUri(requestUri.AbsolutePath)
                && paramters.First().Value is MultipartFormDataContent form
                && paramters.Count == 1)
            {
                return form;
            }

            var serializedContent = paramters.Count == 1
                ? JsonConvert.SerializeObject(paramters.First().Value, client.JsonSerializerSettings)
                : JsonConvert.SerializeObject(paramters, client.JsonSerializerSettings);

            return new StringContent(serializedContent, Encoding.UTF8, "application/json");
        }
    }

}
