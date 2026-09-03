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
using CommonModel.MethodInfo;
using Duende.IdentityModel.Client;
using Duende.IdentityModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HybirdProxy.Token
{
    public class TokenHelper
    {
        public enum ClientType
        { Agent = 1, Manager = 2 }
        
        public static string TokenIssuer = "https://localhost:5001/";
        private static Microsoft.Extensions.Caching.Memory.MemoryCache cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());


        private static HttpClient client = new HttpClient();
        public static async Task<string> RequestToken(string clientId, ClientType type, string tenantId = null)
        {
            //var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(TokenIssuer);
            if (disco.IsError)
            {
                return null;
            }
            ClientCredentialsTokenRequest request = null;
            if(type == ClientType.Manager)
            {
                request = new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "managerClient",
                    ClientSecret = "secret",
                    Scope = "manager common"
                };
            }
            else
            {
                Dictionary<string, string> claims = new Dictionary<string, string>();
                claims.Add("AgentId", clientId);
                claims.Add("TenantId", tenantId);
                request = new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "agentClient",
                    ClientSecret = "secret",

                    Scope = "agent common",

                    Parameters = new Parameters(claims)
                };
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(request);

            if (tokenResponse.IsError)
            {
                return null;
            }

            return tokenResponse.AccessToken;
        }

        public static async Task<string> RequestToken(HttpClient httpClient, string clientId, string ClientAuth, string scope, string IdentityServerClientId, string IdentityServerAddress, Func<X509Certificate2> communicationCertificateFunc, string tenantId, ILoggerFactory logFactory = null)
        {
            
            var cacheKey = $"hybridrecord:token:identity:{IdentityServerClientId}:{scope}:{tenantId ?? "none"}";
            var cachedToken = cache.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

            //var httpClient = new HttpClient();

            var disco = await httpClient.GetDiscoveryDocumentAsync(IdentityServerAddress);
            if (disco.IsError)
            {
                throw disco.Exception;
            }

            ClientCredentialsTokenRequest request;
            Dictionary<string, string> claims = new Dictionary<string, string>();

            request = new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientAssertion = new ClientAssertion()
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = CreateClientAuthJwt(disco, communicationCertificateFunc(), IdentityServerClientId)
                },
                Scope = scope + " " + APIScope.Common,
                Parameters = new Parameters(claims)
            };

            if (!string.IsNullOrEmpty(tenantId))
            {
                request.Parameters.Add("tenantid", tenantId);
                request.Parameters.Add("agentid", clientId);
                request.Parameters.Add("installationcode", ClientAuth);
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(request);

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error, tokenResponse.Exception);
            }

            var result = tokenResponse.AccessToken;
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(50));
            return result;

        }

        private static string CreateClientAuthJwt(DiscoveryDocumentResponse response, X509Certificate2 CommunicationCertificate,string IdentityServerClientId)
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
                subject,
                signingCredentials: new SigningCredentials(new X509SecurityKey(CommunicationCertificate), "RS256")

            );
            return tokenHandler.WriteToken(securityToken);
        }

    }
}
