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
namespace Microsoft365.Authentication.Token.Idclr
{
    using Microsoft365.Authentication;
    using Microsoft365.Authentication.Extension;
    using Microsoft365.Authentication.TokenProvider;
    using System;
    using System.Net;
    using System.Security;
    using System.Threading.Tasks;

    public class SPOIDCLRTokenProvider : NativeNestedTokenProviderBase, ITokenProvider
    {
        private readonly ITokenProvider provider;
        protected AveAzureEnvironment AveAzureEnvironment { get; set; }

        public SPOIDCLRTokenProvider(string username, string password)
            : this(username, password, AveAzureEnvironment.None)
        { }

        public SPOIDCLRTokenProvider(string username, string password, AveAzureEnvironment environment)
            : this(username, password.ToSecureString(), environment)
        { }

        public SPOIDCLRTokenProvider(string username, SecureString password)
            : this(username, password, AveAzureEnvironment.None)
        { }

        public SPOIDCLRTokenProvider(string username, SecureString password, AveAzureEnvironment environment)
        {
            string domain = null;

            var index = username.IndexOf('@');
            if (index >= 0 && index + 1 < username.Length)
            {
                domain = username.Substring(index + 1);
            }
            if (environment == AveAzureEnvironment.None && !string.IsNullOrEmpty(username))
            {
                environment = Office365Discover.GetEnvironment(username);
            }
            AveAzureEnvironment = environment;
            provider = AuthenticationFramework.GetAuthProviderApi(domain).CreateTokenProvider(username, password, environment);
        }

        public string Identifier
        {
            get
            {
                return provider.Identifier;
            }
        }

        public TokenType TokenType
        {
            get { return TokenType.IDCLR; }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            return provider.GetToken(url, refresh);
        }
    }
}