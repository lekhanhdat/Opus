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
    using Microsoft365.Authentication.Token.BearToken;
    using Microsoft365.Authentication.TokenProvider;
    using System;
    using Microsoft365.Authentication.ADAL;
    using System.Threading.Tasks;
    using System.Security;

    class NativeNestedDelegateTokenProvider : DelegateUserTokenProvider, INestedTokenProvider
    {
        public NativeNestedDelegateTokenProvider(string userName, SecureString password, AveAzureEnvironment environment)
            : base(userName, password, environment)
        {
        }

        internal async Task<AuthenticationResult> GetAuthenticationResult(Uri url, string clientId)
        {
            return await context.AcquireTokenAsync(
                    url.GetLeftPart(UriPartial.Authority),
                    clientId,
                    credential);
        }

        protected override async Task<AccessTokenResult> ProcessTokenResultAsync(string resource, AuthenticationResourceType resourceType)
        {
            var resourceUrl = ResourceUtil.GenerateResourceUrl(resource, resourceType, AveAzureEnvironment);
            string clientId = GetClientId(resourceType);
            if (Uri.TryCreate(resourceUrl, UriKind.Absolute, out Uri uri))
            {
                var result = context.AcquireTokenAsync(
                    uri.GetLeftPart(UriPartial.Authority),
                    clientId,
                    credential);
                return new AccessTokenResult((await result).AccessToken, null, (await result).ExpiresOn, TokenType.Bearer);
            }
            throw new ArgumentException($"Invalid Uri {resourceUrl}");
        }

        protected virtual string GetClientId(AuthenticationResourceType resourceType)
        {
            return ResourceUtil.GetDelegateAppClientId(resourceType);
        }
    }
}