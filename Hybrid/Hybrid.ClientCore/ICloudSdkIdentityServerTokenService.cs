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
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AvePoint.Hybrid.ClientCore.Logging;
    using Duende.IdentityModel;
    using Duende.IdentityModel.Client;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public interface ICloudSdkIdentityServerTokenService
    {
        Task<string> GetIdentityServerToken(string scope, string tenantId = null, string clientId = null, string ClientAuth = null);

        Task<bool> TrySetIdentityServerTokenAsync(ApiOptionBase apiOptionBase, string scope, HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string tenantId = null,string HybridAgentId=null,string HybridAgentAuth = null);

        void SetIdentityServerToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token);

        void SetCloudIdentityToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token);
    }

    public enum AuthenticationHeaderScheme
    {
        None,
        Basic,
        Bearer
    }

    internal class CloudSdkIdentityServerTokenService : ICloudSdkIdentityServerTokenService
    {
        private const string IDENTITY_SERVER_IS_BROKEN_CACHE_KEY = "sdk:identityserver_is_broken";
        private const int IDENTITY_SERVER_BROKEN_SKIP_MINUTES = 5;

        private readonly ILogger<CloudSdkIdentityServerTokenService> logger;
        private readonly ISdkLogger _logger;
        private readonly CloudSdkCoreOptions coreOptions;
        private readonly HttpClient httpClient;
        private readonly MemoryCache cache;

        private DiscoveryDocumentResponse discoveryDocumentResponse = null;

        public CloudSdkIdentityServerTokenService(ILogger<CloudSdkIdentityServerTokenService> logger,
            ISdkLogger sdkLogger,
            ApiMemoryCache cache,
            IOptions<CloudSdkCoreOptions> coreOptions,
            ICloudSdkHttpClientFactory cloudSdkHttpClientFactory)
        {
            this.logger = logger;
            this._logger = sdkLogger;
            this.coreOptions = coreOptions.Value;
            this.httpClient = cloudSdkHttpClientFactory.GetClientByName("CloudSdkIdentityServer");
            this.cache = cache.Cache;
        }

        private void LogUseProxy(HttpClient client)
        {
            BindingFlags InstanceBindFlags = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.Instance;
            var clientHandlerProperty = client.GetType().BaseType.GetField("handler", InstanceBindFlags);
            if(clientHandlerProperty == null)
            {
                clientHandlerProperty = client.GetType().BaseType.GetField("_handler", InstanceBindFlags);
            }
            var handlerObj = clientHandlerProperty.GetValue(client);
            var clientHandlerObj = GetHttpClientHandlerForLog(handlerObj);
            logger.LogInformation($"Use Proxy: {clientHandlerObj?.UseProxy}");
            _logger?.Info($"Use Proxy: {clientHandlerObj?.UseProxy}");
        }

        private HttpClientHandler GetHttpClientHandlerForLog(object handlerObj)
        {
            HttpClientHandler clientHandlerObj = handlerObj as HttpClientHandler;
            if (clientHandlerObj != null)
            {
                return clientHandlerObj;
            }
            else
            {
                var delegatingHandlerObj = handlerObj as DelegatingHandler;
                if (delegatingHandlerObj != null && delegatingHandlerObj.InnerHandler != null)
                {
                    return GetHttpClientHandlerForLog(delegatingHandlerObj.InnerHandler);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<string> GetIdentityServerToken(string scope, string tenantId = null, string HybridAgentId = null, string HybridAgentAuth=null)
        {
            var cacheKey = $"cloudsdk:token:identity:{coreOptions.IdentityServerClientId}:{scope}:{tenantId ?? "none"}";
            var cachedToken = cache.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

            var traceContext = new TraceContext(typeof(CloudSdkIdentityServerTokenService));

            logger.LogInformation("Try to get token from identity server. scope:{0} tenant:{1}",scope, tenantId ?? "N/A");
            _logger?.Info("Try to get token from identity server. scope:{0} tenant:{1}", scope, tenantId ?? "N/A");
            LogUseProxy(httpClient);

            if (discoveryDocumentResponse == null)
            {
                if (String.IsNullOrEmpty(coreOptions.IdentityServerAddress))
                {
                    var errorMessage = "To use the Identity Server token, you must set the value of IdentityServerAddress through the ConfigureIdentityServer() method.";
                    logger.LogError(errorMessage);
                    _logger?.Error(errorMessage);
                    throw new ArgumentException(errorMessage, nameof(coreOptions.IdentityServerAddress));
                }

                using (var discoRequest = new DiscoveryDocumentRequest { Address = coreOptions.IdentityServerAddress })
                {
                    if (!string.IsNullOrEmpty(traceContext.ActivityId))
                    {
                        discoRequest.Headers.Add("traceparent", traceContext.ActivityId);
                    }
                    var disco = Task.Run(() => httpClient.GetDiscoveryDocumentAsync(discoRequest)).Result;
                    //var disco = await httpClient.GetDiscoveryDocumentAsync(discoRequest);
                    if (disco.IsError)
                    {
                        logger.LogError(disco.Exception, disco.Error);
                        _logger?.Error(disco.Exception, disco.Error);
                        throw disco.Exception;
                    }

                    discoveryDocumentResponse = disco;
                }
            }

            var request = new ClientCredentialsTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientAssertion = new ClientAssertion()
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = CreateClientAuthJwt(discoveryDocumentResponse, tenantId)
                },
                Scope = scope,
                Parameters = new Parameters(new Dictionary<string,string>())
            };

            if (!string.IsNullOrEmpty(traceContext.ActivityId))
            {
                request.Headers.Add("traceparent", traceContext.ActivityId);
            }

            using (request)
            {

                if (!string.IsNullOrEmpty(tenantId))
                {
                    request.Parameters.Add("tenantid", tenantId);
                    if (!coreOptions.IsInternalIdentityServer)
                    {
                        request.Parameters.Add("agentid", HybridAgentId);
                        request.Parameters.Add("installationcode", HybridAgentAuth);
                    }
                }

                var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(request);
                logger.LogInformation($"End of getting token from identity server.scope:{scope} tenant:{tenantId}, token is null:{string.IsNullOrEmpty(tokenResponse.AccessToken)}");
                _logger?.Info($"End of getting token from identity server.scope:{scope} tenant:{tenantId}, token is null:{string.IsNullOrEmpty(tokenResponse.AccessToken)}");

                if (tokenResponse.IsError)
                {
                    logger.LogError(tokenResponse.Exception, tokenResponse.Error);
                    _logger?.Error($"get token, address: {discoveryDocumentResponse?.TokenEndpoint} error: {tokenResponse.Exception?.ToString()}");
                    throw new CloudApiException(tokenResponse.Error, tokenResponse.Exception) { TraceContext = traceContext };
                }

                var result = tokenResponse.AccessToken;
                cache.Set(cacheKey, result, TimeSpan.FromMinutes(50));
                return result;
            }
        }

        public async Task<bool> TrySetIdentityServerTokenAsync(ApiOptionBase apiOptionBase, string scope, HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme = AuthenticationHeaderScheme.Bearer, string tenantId = null, string HybridAgentId = null, string HybridAgentAuth = null)
        {
            var isIdentityServerBroken = cache.Get<bool?>(IDENTITY_SERVER_IS_BROKEN_CACHE_KEY) ?? false;

            if (isIdentityServerBroken || !apiOptionBase.UseIdentityServer || !coreOptions.IsIdentityServerConfigured)
            {
                if (isIdentityServerBroken)
                {
                    var logText = $"Since access to Identity Server failed in the most recent request, access will be skipped this time. scope:{scope} tenant:{tenantId ?? "N/A"}";
                    logger.LogWarning(logText);
                    _logger?.Warn(logText);
                }

                return false;
            }

            return await GetIdentityServerToken(scope, tenantId,HybridAgentId, HybridAgentAuth)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        cache.Set(IDENTITY_SERVER_IS_BROKEN_CACHE_KEY, true, TimeSpan.FromMinutes(IDENTITY_SERVER_BROKEN_SKIP_MINUTES));

                        var logText = $"Get token from identity server failed: the maximum number of retrys has been reached. scope:{scope} tenant:{tenantId ?? "N/A"}";
                        logger.LogError(logText);
                        _logger?.Error(logText);
                    }
                    else
                    {
                        SetIdentityServerToken(request, tokenScheme, t.Result);
                    }

                    return !t.IsFaulted;
                });
        }

        public void SetIdentityServerToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token)
        {
            request.Headers.Add("Token-Source", "IdentityServer");
            request.Headers.Add("Is-Internal-Identity-Server", coreOptions.IsInternalIdentityServer ? "1" : "0");
            SetAuthorizationHeader(request, tokenScheme, token);
             _logger?.Info($"set identity token header info:{tokenScheme}, {coreOptions.IsInternalIdentityServer}, {string.IsNullOrEmpty(token)}");
        }

        public void SetCloudIdentityToken(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token)
        {
            request.Headers.Add("Token-Source", "CloudIdentity");
            SetAuthorizationHeader(request, tokenScheme, token);
        }

        private void SetAuthorizationHeader(HttpRequestMessage request, AuthenticationHeaderScheme tokenScheme, string token)
        {
            switch (tokenScheme)
            {
                case AuthenticationHeaderScheme.None:
                    request.Headers.Add("Authorization", token);
                    break;
                case AuthenticationHeaderScheme.Basic:
                case AuthenticationHeaderScheme.Bearer:
                    request.Headers.Authorization = new AuthenticationHeaderValue(tokenScheme.ToString(), token);
                    break;
                default:
                    throw new NotSupportedException(tokenScheme.ToString());
            }
        }

        private string CreateClientAuthJwt(DiscoveryDocumentResponse response, string tenantId = null)
        {
            // set exp to 5 minutes
            var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 10 };

            var subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("sub", coreOptions.IdentityServerClientId),
                    new Claim("jti", Guid.NewGuid().ToString()),
                });

            var securityToken = tokenHandler.CreateJwtSecurityToken(
                issuer: coreOptions.IdentityServerClientId,
                audience: response.TokenEndpoint,
                subject,
                signingCredentials: new SigningCredentials(new X509SecurityKey(coreOptions.CommunicationCertificate), "RS256")
            );
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
