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
    using System.Security;
    using Microsoft365.Authentication.ADAL;
    using Microsoft365.Authentication.ServiceEndPoint;
    using Microsoft365.Authentication;
    using Microsoft365.Authentication.Token.ModernToken;
    using Microsoft365.Authentication.Extension;
    using System.Net;

    public class DelegateUserTokenProvider : NativeNestedTokenProviderBase,IDelegateUserTokenProvider,ITokenProvider
    {
       
        private const string DefaultTokenPrefix = "Bearer ";
        private const string DefaultTenantId = "common";
        protected AuthenticationContext context { get; set; }
        protected UserCredential credential { get; set; }
        protected string TokenPrefix { get; private set; } = DefaultTokenPrefix;
        protected virtual string ClientId { get; set; }= ResourceUtil.MicrosoftApp;
        public AveAzureEnvironment AveAzureEnvironment { get; set; }

        public string Identifier
        {
            get
            {
                return credential.UserName;
            }
        }

        public TokenType TokenType
        {
            get
            {
                return TokenType.Bearer;
            }
        }
        [Obsolete]
        public DelegateUserTokenProvider(string userName, string password, AveAzureEnvironment environment)
            : this(userName, password.ToSecureString(), DefaultTenantId, environment)
        {
        }

        public DelegateUserTokenProvider(string userName, string password, string tenantId, AveAzureEnvironment environment)
             : this(userName, password.ToSecureString(), tenantId, environment)
        {
        }

        private DelegateUserTokenProvider(string userName, SecureString password, string tenantId, AveAzureEnvironment environment)
        {
            ArgumentCheck(userName, password);
            ClientId = ResourceUtil.MicrosoftApp;
            TokenPrefix = DefaultTokenPrefix;
            context = new AuthenticationContext($"{MicrosoftOnlineInstance.FromEnvironment(environment).AdalAuthorityEndpointUrl.TrimEnd('/')}/{tenantId}");
            credential = new UserCredential(userName, password);
            AveAzureEnvironment = environment;
        }

        public DelegateUserTokenProvider(string userName, SecureString password, AveAzureEnvironment environment)
             : this(userName, password, DefaultTenantId, environment)
        {
            ArgumentCheck(userName, password);
            ClientId = ResourceUtil.MicrosoftApp;
            TokenPrefix = DefaultTokenPrefix;
            context = new AuthenticationContext($"{MicrosoftOnlineInstance.FromEnvironment(environment).AdalAuthorityEndpointUrl.TrimEnd('/')}/common");
            credential = new UserCredential(userName, password);
            AveAzureEnvironment = environment;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        public virtual string GetToken(Uri url, bool refresh = false)
        {
            return GetUserToken(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">sharepoint url</param>
        /// <returns>Bearer token with Bearer prefix</returns>
        public string GetUserToken(Uri url)
        {
            return string.Concat(TokenPrefix,
                GetAuthenticationResult(url).AccessToken);
        }

        public AuthenticationResult GetAuthenticationResult(Uri url)
        {
            return context.AcquireTokenAsync(
                    url.GetLeftPart(UriPartial.Authority),
                    ClientId,
                    credential).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void ArgumentCheck(string username, SecureString password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
            if (password==null|| password.Length==0)
            {
                throw new ArgumentNullException("password");
            }
        }

        public override string ToString()
        {
            return $"{credential.UserName}|{TokenType}";
        }
    }
}