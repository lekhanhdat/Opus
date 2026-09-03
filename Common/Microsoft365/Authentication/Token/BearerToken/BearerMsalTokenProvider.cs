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
    using Microsoft.Identity.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft365.Common.Extension;
    using Microsoft365.Authentication.Extension;


    public class BearerMsalTokenProvider : ITokenProvider
    {
        private IPublicClientApplication application;
        private string username;
        SecureString password;

        public TokenType TokenType { get { return TokenType.Bearer; } }

        public string Identifier { get; private set; }

        public BearerMsalTokenProvider(string tenantId, string username, string password)
            : this(tenantId, username, password, AveAzureEnvironment.AzureCloud, AzureCommonPowerShellClientId.AzureADPowerShell)
        {

        }
        public BearerMsalTokenProvider(string tenantId, string username, string password, AveAzureEnvironment environment)
            : this(tenantId, username, password, environment, AzureCommonPowerShellClientId.AzureADPowerShell)
        {

        }
        public BearerMsalTokenProvider(string tenantId, string username, string password, AveAzureEnvironment environment, string clientId)
        {
            tenantId.ArgumentNullValidation("tenantId");
            clientId.ArgumentNullValidation("clientId");
            username.ArgumentNullValidation("username");
            password.ArgumentNullValidation("password");
            Identifier = tenantId;
            this.username = username;
            this.password = password.ToSecureString();
            if(string.IsNullOrEmpty(tenantId))
            {
                var mail = new System.Net.Mail.MailAddress(username);
                tenantId = mail.Host;
            }

            application = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(environment.ToMSALCloudInstance(), tenantId, false)
                //.WithRedirectUri("urn:ietf:wg:oauth:2.0:oob")
                .Build();
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            var scope = new List<string> { string.Format("{0}/.default", url.GetLeftPart(UriPartial.Authority)) };
            var accounts = application.GetAccountsAsync().Result;
            if (accounts.Any())
            {
                return string.Concat("Bearer ", application.AcquireTokenSilent(scope, accounts.FirstOrDefault()).ExecuteAsync().Result.AccessToken);
            }
            return string.Concat("Bearer ",
                application.AcquireTokenByUsernamePassword(scope, username, password).ExecuteAsync().Result.AccessToken);

        }
    }
}
