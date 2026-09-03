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

namespace Microsoft365.Authentication
{
    using Microsoft.Identity.Client;
    using Microsoft365.Authentication.Extension;
    using System;
    using Microsoft365.Authentication.TokenProvider;
    using Microsoft365.Common.Logger;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// not tested,don't use it on production at this moment.
    /// </summary>
    internal class NativeNestedDelegateMSALTokenProvider :NativeNestedTokenProviderBase, INestedTokenProvider
    {
        //private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(NativeNestedDelegateMSALTokenProvider));
        protected virtual IPublicClientApplication Application { get; set; }
        protected virtual AveAzureEnvironment AveAzureEnvironment { get; set; }
        protected virtual string UserName { get; set; }
        protected virtual SecureString Password { get; set; }

        public NativeNestedDelegateMSALTokenProvider(string userName, string password,string clientId,string tenantId, AveAzureEnvironment environment)
        {
            var tenant = string.IsNullOrEmpty(tenantId) ? Office365Discover.GetTenantId(userName): tenantId;
            Application = BuildApplication(clientId, tenant,environment);
            AveAzureEnvironment = environment;
            UserName = userName;
            Password = password.ToSecureString();
        }

        private static IPublicClientApplication BuildApplication(string clientId, string tenantId, AveAzureEnvironment environment, string redirectUrl = "")
        {
            var builder = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(environment.ToMSALCloudInstance(), tenantId)
                .WithDefaultLogging();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                builder.WithRedirectUri(redirectUrl);
            }
            else
            {
                builder.WithDefaultRedirectUri();
            }
            return builder.Build();
        }

        protected override async Task<AccessTokenResult> ProcessTokenResultAsync(string resource, AuthenticationResourceType resourceType)
        {
            var scopes = new List<string> { ResourceUtil.GenerateMsalScope(resource, resourceType, AveAzureEnvironment) };
            var result = Application.AcquireTokenByUsernamePassword(scopes, UserName, Password).ExecuteAsync();
            return new AccessTokenResult((await result).AccessToken, null, (await result).ExpiresOn, TokenType.Bearer);
        }
    }
}