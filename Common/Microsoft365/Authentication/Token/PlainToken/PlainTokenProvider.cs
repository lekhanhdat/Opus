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
namespace Microsoft365.Authentication.Token.PlainToken
{
    using Microsoft365.Authentication;
    using Microsoft365.Common.Extension;
    using System;
    using System.Net;
    using System.Security;

    public class PlainTokenProvider : ITokenProvider
    {
        private readonly string username;
        private readonly string password;
        private readonly SecureString securePassword;

        public PlainTokenProvider(string username, string password)
        {
            username.ArgumentNullValidation("username");
            password.ArgumentNullValidation("password");
            this.username = username;
            this.password = password;
        }

        public PlainTokenProvider(string username, SecureString password)
        {
            username.ArgumentNullValidation("username");
            password.ArgumentNullValidation("password");
            this.username = username;
            securePassword = password;
        }

        public TokenType TokenType
        {
            get
            {
                return TokenType.Plain;
            }
        }

        public string Identifier
        {
            get
            {
                return username;
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            var credential = new NetworkCredential();

            var index = username.IndexOf('\\');

            if (index > 0)
            {
                var domain = username.Substring(0, index);
                var name = username.Substring(index + 1);
                credential.Domain = domain;
                credential.UserName = name;
            }
            else
            {
                credential.UserName = username;
            }

            if (password == null)
            {
                credential.SecurePassword = securePassword;
            }
            else
            {
                credential.Password = password;
            }

            return credential;
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            throw new NotSupportedException();
        }
    }
}