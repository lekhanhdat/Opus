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
namespace Microsoft365.Authentication.Token.BearToken
{
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft365.Authentication.Configuration;
    using Microsoft365.Common.Extension;
    using Microsoft365.Configuration;
    using Microsoft.Identity.Client;
    using System.Collections.Generic;
    using Microsoft365.Common.Utility;
    using Microsoft365.Common.Logger;
    using System.Threading.Tasks;
    using Microsoft365.Authentication.Extension;

    public class AppOnlyBearerTokenProvider : NativeNestedTokenProviderBase,ITokenProvider
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(AppOnlyBearerTokenProvider));
        IConfidentialClientApplication application;
        protected AuthenticationResult CachedResult { get; set; }

        public string Identifier { get; private set; }

        public TokenType TokenType { get { return TokenType.Bearer; } }

        public AveAzureEnvironment AveAzureEnvironment { get; set; }
        protected string TenantId { get; set; }
        protected string ClientId { get; set; }
        protected X509Certificate2 Certificate { get; set; }

        public AppOnlyBearerTokenProvider(string tenantId, string clientId, X509Certificate2 certificate, AveAzureEnvironment environment)
        {
            tenantId.ArgumentNullValidation("tenantId");
            clientId.ArgumentNullValidation("clientId");
            certificate.ArgumentNullValidation("certificate");
            AveAzureEnvironment = environment;
            Identifier = clientId;
            application = ConfidentialClientApplicationBuilder.Create(clientId)
              .WithAuthority(environment.ToMSALCloudInstance(), tenantId, false)
              .WithCertificate(certificate)
              .WithDefaultLogging()
              .Build();
            application.AppTokenCache.SetBeforeWrite(RequireTokenNotification);

        }

        public void RequireTokenNotification(TokenCacheNotificationArgs args)
        {
            if (!string.IsNullOrEmpty(args?.ClientId))
            {
                Microsoft365Configuration.AuthenticationConfiguration.BeforeRequestTokenEvent?.Invoke(new BeforeGetTokenArg
                {
                    Identity = args.ClientId,
                    IdentityType = TokenType.Bearer.ToString(),
                    ResourceUrl =args.SuggestedCacheKey?? TokenType.Bearer.ToString()
                });
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }
        public string GetToken(Uri url, bool refresh = false)
        {
            return GetAuthenticationResult(url, refresh).CreateAuthorizationHeader();
        }

        public async Task<AuthenticationResult> GetAuthenticationResultAsync(Uri url, bool refresh = false)
        {
            var scopes = new List<string> { url.GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" };
            return await application.AcquireTokenForClient(scopes)
                .WithForceRefresh(refresh)
                .ExecuteAsync();
        }

        public AuthenticationResult GetAuthenticationResult(Uri url, bool refresh = false)
        {
           var result = GetAuthenticationResultAsync(url, refresh)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            if (!string.Equals(CachedResult?.AccessToken, result?.AccessToken, StringComparison.OrdinalIgnoreCase))
            {
                logger.Info($"{Environment.NewLine}AccessToken: ExpiresOn:{result?.ExpiresOn.UtcDateTime},ExtendedExpiresOn:{result?.ExtendedExpiresOn.UtcDateTime},TokenType:{result?.TokenType},Header:{result?.CreateAuthorizationHeader().Substring(0, 15)},Scopes:{string.Join(";", result?.Scopes)}");
                logger.Info($"{Environment.NewLine}{JwtUtil.GetPayload(result?.AccessToken)}");
            }
            CachedResult = result;
            return result;
        }

        public override string ToString()
        {
            return $"{TenantId}|{ClientId}|{TokenType}";
        }
    }
}