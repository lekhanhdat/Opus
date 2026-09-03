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
    using Microsoft365.Authentication.Extension;
    using Microsoft365.Authentication.Token.Idclr;
    using System;
    using System.Net;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using Token.Idclr;

    class CertAuthSPOTokenProvider : ITokenProvider
    {
        private readonly X509CertificateCollection certificates;
        private readonly SPOCredentials onlineCredentials;

        public CertAuthSPOTokenProvider(string username, string password, X509CertificateCollection certificates, AveAzureEnvironment environment)
            : this(username, password.ToSecureString(), certificates, environment)
        { }

        public CertAuthSPOTokenProvider(string username, SecureString password, X509CertificateCollection certificates, AveAzureEnvironment environment)
        {
            this.certificates = certificates;
            if (environment == AveAzureEnvironment.None && !string.IsNullOrEmpty(username))
            {
                environment = Office365Discover.GetEnvironment(username);
            }
            onlineCredentials = new SPOCredentials(username, password, environment);
            //onlineCredentials.ExecutingWebRequest += EnsureCertificates;
        }



        public TokenType TokenType
        {
            get { return TokenType.IDCLR; }
        }

        public string Identifier
        {
            get
            {
                return onlineCredentials.UserName;
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            return onlineCredentials.GetAuthenticationCookie(url, refresh, true);
        }
    }
}