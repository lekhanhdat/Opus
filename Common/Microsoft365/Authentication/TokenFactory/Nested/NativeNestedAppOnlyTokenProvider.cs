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
    using System.Security.Cryptography.X509Certificates;
    using System;
    using Microsoft365.Authentication.Token;
    using Microsoft365.Common.Utility;
    using System.Threading.Tasks;
    using Microsoft365.Authentication.ADAL;

    class NativeNestedAppOnlyTokenProvider :AppOnlyBearerTokenProvider, INestedTokenProvider
    {
        public NativeNestedAppOnlyTokenProvider(string tenantId, string clientId, X509Certificate2 certificate,AveAzureEnvironment environment) 
            : base(tenantId, clientId, certificate, environment)
        {
        }
        protected override async Task<AccessTokenResult> ProcessTokenResultAsync(string resource, AuthenticationResourceType resourceType)
        {
            var resourceUrl = ResourceUtil.GenerateResourceUrl(resource, resourceType, AveAzureEnvironment);
            if (Uri.TryCreate(resourceUrl, UriKind.Absolute, out Uri uri))
            {
                var result =  GetAuthenticationResultAsync(uri);
                return  new AccessTokenResult((await result).AccessToken, null, (await result).ExpiresOn, TokenType.Bearer);
            }
            throw new ArgumentException($"Invalid Uri {resourceUrl}");
        }
    }
}