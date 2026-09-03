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

namespace ExchangeUtility
{
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Net;
    using System.Security;

    public class CredentialAuthObject : AuthObject
    {
        internal CredentialAuthObject(string userName, string password, string ewsServiceUrl)
            : base(userName, ewsServiceUrl)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

            this.Password = password.ToSecureString();
            this.PasswordC = password;
        }

        public SecureString Password { get; private set; }
        public String PasswordC { get; private set; }
        public override AuthObjectType AuthType
        {
            get { return AuthObjectType.UserPassword; }
        }

        public override void BindToExchangeService(ExchangeService service)
        {
            service.Credentials = new NetworkCredential(this.UserName, this.Password);
            service.UseDefaultCredentials = false;
        }

        public override void SetImpersonatedUserId(ExchangeService service, string impersonatedUserAddress)
        {
            if (string.Equals(this.UserName, impersonatedUserAddress, StringComparison.OrdinalIgnoreCase)) return;
            base.SetImpersonatedUserId(service, impersonatedUserAddress);
        }
    }
}
