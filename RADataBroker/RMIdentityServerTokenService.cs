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
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.RA.RADataBroker
{
    public class RMIdentityServerTokenService: IDisposable
    {
        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(RMIdentityServerTokenService));
        private static readonly HttpClient httpClient;
        private readonly MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        private string IdentityServerAddress;
        private string IdentityServerClientId;
        private X509Certificate2 Cert;
        static RMIdentityServerTokenService() 
        {
            var httpClientHandler = new HttpClientHandler();
#if DEBUG
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromHours(1) };
            httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2;)");
        }
        public RMIdentityServerTokenService(string address, string clientId, X509Certificate2 cert)
        {
            this.IdentityServerAddress = address;
            this.IdentityServerClientId = clientId;
            this.Cert = cert;
        }

        public void Dispose()
        {
            if (cache?.Count > 0) 
            {
                cache.Dispose();
            }
        }

        public string GetIdentityServerToken(string tenantId)
        {
            var cacheKey = $"cloudsdk:token:identity:{IdentityServerClientId}: {RMIdentityScopes.DAO_ReadWrite_All} :{tenantId ?? "none"}";
            var cachedToken = cache.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }
            logger.Info($"get token from dao request:{tenantId}");

            var disco = httpClient.GetDiscoveryDocumentAsync(IdentityServerAddress).Result;
            if (disco.IsError)
            {
                logger.Error($"get identity discover error:{disco?.Exception?.ToString()}");
                throw disco.Exception;
            }

            var request = new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientAssertion = new ClientAssertion()
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = CreateClientAuthJwt(disco, tenantId)
                },
                Scope = RMIdentityScopes.DAO_ReadWrite_All,
                Parameters = new Parameters(new Dictionary<string, string>())
            };
            request.Parameters.Add("tenantid", tenantId);

            var tokenResponse = httpClient.RequestClientCredentialsTokenAsync(request).Result;

            if (tokenResponse.IsError)
            {
                logger.Error($"get identity token error:{tokenResponse?.Exception?.ToString()}");
                throw new Exception(tokenResponse?.Exception?.Message);
            }

            var result = tokenResponse.AccessToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var re = handler.ReadJwtToken(result);
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(50));
            return result;
        }

        private string CreateClientAuthJwt(DiscoveryDocumentResponse response, string tenantId = null)
        {
            // set exp to 5 minutes
            var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 10 };

            var subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("sub", IdentityServerClientId),
                    new Claim("jti", Guid.NewGuid().ToString()),
                });

            var securityToken = tokenHandler.CreateJwtSecurityToken(
                issuer: IdentityServerClientId,
                audience: response.TokenEndpoint,
                subject : subject,
                signingCredentials: new SigningCredentials(new X509SecurityKey(Cert), "RS256")
            );
            return tokenHandler.WriteToken(securityToken);
        }

    }

    public class RMIdentityScopes
    {
        public static readonly string DAO_ReadWrite_All = "cloudmanagement.readwrite.all";
    }
}
