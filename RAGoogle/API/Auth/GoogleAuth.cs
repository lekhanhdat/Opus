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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using Cloud.Sdk.Data.AosModern;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Newtonsoft.Json;
using System.Net;
using Util;

namespace RAGoogle.API
{
    public static class GoogleAuth
    {

        private static RALogger s_logger = RALogger.GetInstance(typeof(GoogleAuth));

        public static ServiceCredential CreateServiceAccountCredential(RMAosGoogleAppProfile app, string impersonateUser = null, GoogleScopeType scopeType = GoogleScopeType.Unknown)
        {
            return CreateServiceAccountCredential(app, GetScopes(scopeType), impersonateUser);
        }

        public static ServiceCredential CreateServiceAccountCredential(RMAosGoogleAppProfile app, IEnumerable<string> scopes, string impersonateUser = null)
        {
            return CreateServiceAccountCredential(
                app.AOSAppId,
                app.TenantId,
                app.ServiceAccount,
                app.TokenServerUrl,//TokenServerUrl,
                app.PrivateKey,
                impersonateUser.IsNotNullOrEmpty() ? impersonateUser : app.UserName,
                app.AuthenticationType == GoogleAuthenticationType.ImpersonationUser,
                scopes);
        }

        internal static ServiceCredential CreateServiceAccountCredential(string appProfileId, string tenantId, string serviceAccount, string tokenServerUrl, string privateKey, string delegatedUser, bool enableImpersonationUser, IEnumerable<string> scopes)
        {
            if(!enableImpersonationUser)
            {
                var zer = new ServiceAccountCredential.Initializer(serviceAccount, tokenServerUrl);
                zer = zer.FromPrivateKey(privateKey);
                zer.User = delegatedUser;
                zer.Scopes = scopes;
                return new ServiceAccountCredential(zer);
            }

            var refreshFunc = new Func<string, string, IdentityProviderType, List<string>, string, Task<(string AccessToken, DateTimeOffset ExpiryUtc)>>(
                async (p1, p2, idpType, scopes, p5) =>
                {
                    var token = await RMAosApiClient.GetGoogleTokenByAppProfileAsync(p1, p2, idpType, scopes, p5);
                    return (token.AccessToken, token.ExpiresOn);
                });

            return new AutoRefreshServiceCredential(() => refreshFunc(appProfileId, tenantId, IdentityProviderType.CustomGoogleApp, scopes.ToList(), delegatedUser));
        }

        internal static IEnumerable<string> GetScopes(GoogleScopeType scopeType)
        {
            return scopeType switch
            {
                GoogleScopeType.Drive => GoogleAuthScopes.DriveScopes,
                GoogleScopeType.DriveWithLabel => GoogleAuthScopes.DriveWithLabelScopes,
                GoogleScopeType.GoogleReport => GoogleAuthScopes.ReportScope,
                GoogleScopeType.Admin => GoogleAuthScopes.AdminScopes,
                _ => null,
            };
        }
    }

    public class CustomHttpClientFactory : Google.Apis.Http.HttpClientFactory
    {
        // You need proxy file to create handler. You can comment it if you don't need config proxy to access google
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            string developmentJson = SecurePath.Combine(AppDomain.CurrentDomain.BaseDirectory, "config1", "AA.json");
            if (!File.Exists(developmentJson))
            {
                throw new ArgumentNullException("Proxy not found.");
            }
            LocalProxy proxyConfig = null;
            using (StreamReader stream = new StreamReader(developmentJson))
            {
                string proxyJson = stream.ReadToEnd();
                if (proxyJson.IsNotNullOrEmpty())
                {
                    proxyConfig = JsonConvert.DeserializeObject<LocalProxy>(proxyJson);
                }
                if (proxyJson.IsNullOrEmpty() || proxyConfig is null)
                {
                    throw new ArgumentNullException("Proxy not found.");
                }
            }
            // 配置代理
            WebProxy proxy = new WebProxy(proxyConfig.Host, true)
            {
                Credentials = new NetworkCredential(proxyConfig.Account, proxyConfig.Password)
            };
            HttpClient.DefaultProxy = proxy;
            return base.CreateHandler(args);
        }

        public class LocalProxy
        {
            public string Host
            {
                get; set;
            }
            public string Account
            {
                get; set;
            }
            public string Password
            {
                get; set;
            }
        }
    }

    public enum GoogleScopeType
    {
        Unknown = 0,
        GoogleReport = 1,
        Drive = 2,
        Admin = 4,
        DriveWithLabel = 8,
    }
}
