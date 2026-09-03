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
//using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Microsoft.Identity.Client;
using System;
using System.Security;

namespace AvePoint.Wrapper.Common.Graph
{
    internal class ServiceAccountGrapahTokenProvider : GraphTokenProviderBase
    {
        public SA2AppAuthInfo AuthInfo { get; private set; }
        IPublicClientApplication app;
        public ServiceAccountGrapahTokenProvider(SA2AppAuthInfo authInfo) 
            : base(authInfo.Resource)
        {
            AuthInfo = authInfo;
        }
        protected override void RefreshToken()
        {
            //var authenticationContext = new AuthenticationContext(string.Format("{0}{1}", AuthInfo.Authority, "Common"), false);
            if (app == null)
            {
                app = PublicClientApplicationBuilder.Create(AuthInfo.ClientId)
                        .WithAuthority(string.Format("{0}/{1}", AuthInfo.Authority, "Common"))
                        .Build();
            }
            var result = app.AcquireTokenByUsernamePassword(new[] { new Uri(AuthInfo.Resource).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" },
                AuthInfo.UserName,
                ToSecureString(AuthInfo.Password)).ExecuteAsync().Result;
            CachedTokenItem = new TokenItem(result.AccessToken, result.TokenType, result.ExpiresOn);
        }

        private SecureString ToSecureString(string value)
        {
            var ss = new SecureString();
            foreach (var c in value)
            {
                ss.AppendChar(c);
            }
            return ss;
        }
    }
}
