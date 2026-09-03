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
    // using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Security;

    public class ServiceAccout2AppTokenAuthObject : AppTokenAuthObject, IAppTokenAuthObject, ICredentialAuthObject
    {
        private SAApptokenManager saTokenManager;

        internal ServiceAccout2AppTokenAuthObject(AuthenticationInfo authenticationInfo, string clientId, string userName, string password, string ewsServiceUrl, AzureCloudType cloudType)
            : base(authenticationInfo, clientId, userName, ewsServiceUrl)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");
            this.saTokenManager = new SAApptokenManager();
            this.PasswordC = password;
            this.Password = password.ToSecureString();
            this.CloudType = cloudType;
        }
        public String PasswordC { get; private set; }

        public AzureCloudType CloudType { get; set; }


        public override AuthObjectType AuthType
        {
            get { return AuthObjectType.PasswordAccessToken; }
        }

        public override TokenPermissionType PermissionType { get { return TokenPermissionType.Delegated; } }

        public SecureString Password { get; private set; }
        public void ResetSecurePassword()
        {
            if (this.Password.Length == 0 && !string.IsNullOrEmpty(this.PasswordC))
                this.Password = this.PasswordC.ToSecureString();
        }

        public override string GetAccessToken()
        {
            RefreshToken();
            return accessToken;
        }
        public bool RefreshToken()
        {
            lock (this.saTokenManager)
            {
                return this.saTokenManager.RefreshAccessToken(this, false);
            }
        }

        public override void BindToExchangeService(ExchangeService service)
        {
            RefreshToken();
            service.Credentials = new OAuthCredentials(this.accessToken);
            //service.HttpHeaders.Add(AUTHORIAZATION_HEADER_NAME, this.AuthorizationHeaderValue);
            //service.PreAuthenticate = true;
            //service.UseDefaultCredentials = true;
        }
        //public override void BindToExchangeServiceBinding(ExchangeServiceBinding serviceBinding, string xAnchorMailbox = null)
        //{
        //    var bindingV2 = serviceBinding as ExchangeServiceBindingV2;
        //    if (bindingV2 == null) throw new ArgumentException();
        //    RefreshToken();
        //    bindingV2.AddHeader("Authorization", this.AuthorizationHeaderValue);
        //    bindingV2.AddHeader(ExchangeConstants.IMPERSONATION_HEADER_NAME, xAnchorMailbox);
        //    bindingV2.PreAuthenticate = true;
        //    bindingV2.UseDefaultCredentials = true;
        //}
        //public override void BindToPOXAutoDiscoverService(POXAutodiscoverService poxAutodiscoverService)
        //{
        //    RefreshToken();
        //    poxAutodiscoverService.Credentials = new POXCredential(this.accessToken);
        //}

        public override void AddImpersonationHeader(ExchangeService service, string mailbox)
        {
            service.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME] = mailbox;
        }

        public override void SetImpersonatedUserId(ExchangeService service, string impersonatedUserAddress)
        {
            base.SetImpersonatedUserId(service, impersonatedUserAddress);
        }
    }
}