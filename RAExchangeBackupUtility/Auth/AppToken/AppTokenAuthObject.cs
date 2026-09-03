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
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Services;
    using Microsoft.Exchange.WebServices.Data;
    using Microsoft.Identity.Client;
    // using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Security.Cryptography.X509Certificates;

    public class AppTokenAuthObject : AuthObject, IAppTokenAuthObject
    {
        #region Input

        internal string tenantId;
        internal string authority;
        internal string clientId;
        internal string appId;
        internal AvePoint.GCommon.Contract.CentralAdmin.Object.AppType appType;
        //internal string siteUrl;
        internal string tenantGroupId;

        private X509Certificate2 appOnlyCertificate;
        public string ResourceUrl { get; private set; }
        #endregion

        #region Result
        public string AccessTokenPayload
        {
            get
            {
                return JsonWebToken.Decode(this.accessToken, null, false);
            }
        }

        protected string accessToken;

        //private string refreshToken;

        private string accessTokenType;

        private DateTimeOffset expiresOn;

        //private UserInfo userInfo;
        #endregion

        private AppTokenManager manager;

        protected virtual string AuthorizationHeaderValue
        {
            get { return string.Format("{0} {1}", this.accessTokenType, this.accessToken); }
        }
        internal AppTokenAuthObject(AuthenticationInfo authenticationInfo, AppInfo appInfo, string userName, string ewsServiceUrl)//, string refreshToken)
          : base(userName, ewsServiceUrl)
        {
            if (string.IsNullOrEmpty(authenticationInfo?.TenantId)) throw new ArgumentNullException("authenticationInfo.TenantId");
            if (string.IsNullOrEmpty(authenticationInfo?.Resource)) throw new ArgumentNullException("authenticationInfo.Resource");
            if (string.IsNullOrEmpty(authenticationInfo?.Authority)) throw new ArgumentNullException("authenticationInfo.Authority");
           // if (string.IsNullOrEmpty(authenticationInfo?.SiteUrl)) throw new ArgumentNullException("authenticationInfo.SiteUrl");
            if (string.IsNullOrEmpty(authenticationInfo?.TenantGroupId)) throw new ArgumentNullException("authenticationInfo.TenantGroupId");
            if (string.IsNullOrEmpty(appInfo?.ClientId)) throw new ArgumentNullException("appInfo.ClientId");
            //if (appInfo?.Certificate == null) throw new ArgumentNullException("appInfo.Certificate");
            //if (appInfo?.AppType == null) throw new ArgumentNullException("appInfo.AppType");
            this.tenantId = authenticationInfo?.TenantId;
            this.ResourceUrl = authenticationInfo?.Resource;
            this.authority = authenticationInfo?.Authority;
            this.clientId = appInfo?.ClientId;
            this.appOnlyCertificate = appInfo?.Certificate;
            this.appType = appInfo.AppType;
            this.appId = appInfo.AppId;
            //this.siteUrl = authenticationInfo?.SiteUrl;
            this.tenantGroupId = authenticationInfo?.TenantGroupId;
            this.manager = new AppTokenManager();
        }

        internal AppTokenAuthObject(AuthenticationInfo authenticationInfo, string clientId, string userName, string ewsServiceUrl)
            : base(userName, ewsServiceUrl)
        {
            if (string.IsNullOrEmpty(authenticationInfo?.TenantId)) throw new ArgumentNullException("authenticationInfo.TenantId");
            if (string.IsNullOrEmpty(authenticationInfo?.Resource)) throw new ArgumentNullException("authenticationInfo.Resource");
            if (string.IsNullOrEmpty(authenticationInfo?.Authority)) throw new ArgumentNullException("authenticationInfo.Authority");
            //if (string.IsNullOrEmpty(authenticationInfo?.SiteUrl)) throw new ArgumentNullException("authenticationInfo.SiteUrl");
            if (string.IsNullOrEmpty(authenticationInfo?.TenantGroupId)) throw new ArgumentNullException("authenticationInfo.TenantGroupId");
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException("appInfo.ClientId");
            this.tenantId = authenticationInfo?.TenantId;
            this.ResourceUrl = authenticationInfo?.Resource;
            this.authority = authenticationInfo?.Authority;
            this.clientId = clientId;
            //this.siteUrl = authenticationInfo?.SiteUrl;
            this.tenantGroupId = authenticationInfo?.TenantGroupId;
        }

        public virtual string GetAccessToken()
        {
            Refresh();
            return this.accessToken;
        }

        public bool Refresh()
        {
            lock (this.manager)
            {
                return this.manager.RefreshAccessToken(this, false);
            }
        }

        public override AuthObjectType AuthType
        {
            get { return AuthObjectType.AccessToken; }
        }

        public virtual TokenPermissionType PermissionType { get { return TokenPermissionType.Application; } }

        public override void BindToExchangeService(ExchangeService service)
        {
            Refresh();
            service.Credentials = new OAuthCredentials(this.accessToken);
            //service.HttpHeaders.Add(AUTHORIAZATION_HEADER_NAME, this.AuthorizationHeaderValue);
            //service.PreAuthenticate = true;
            //service.UseDefaultCredentials = true;
        }

        //public override void BindToExchangeServiceBinding(ExchangeServiceBinding serviceBinding, string xAnchorMailbox = null)
        //{
        //    var bindingV2 = serviceBinding as ExchangeServiceBindingV2;
        //    if (bindingV2 == null) throw new ArgumentException();

        //    Refresh();
        //    bindingV2.AddHeader(AUTHORIAZATION_HEADER_NAME, this.AuthorizationHeaderValue);
        //    bindingV2.AddHeader(ExchangeConstants.IMPERSONATION_HEADER_NAME, xAnchorMailbox);
        //    bindingV2.PreAuthenticate = true;
        //    bindingV2.UseDefaultCredentials = true;
        //}

        public override void AddImpersonationHeader(ExchangeService service, string mailbox)
        {
            service.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME] = mailbox;
        }

        public override void SetImpersonatedUserId(ExchangeService service, string impersonatedUserAddress)
        {
            base.SetImpersonatedUserId(service, impersonatedUserAddress);
        }

        public override void RemoveImpersonatedUserId(ExchangeService service)
        {
            base.RemoveImpersonatedUserId(service);
        }

        public class AppTokenManager
        {
            private static IRALogger logger = RALogger.GetInstance(typeof(AppTokenManager));
            private static readonly TimeSpan Token_EXPIRES_EDGE = new TimeSpan(0, 5, 0);

            public AppTokenManager()//string tenantId)
            {
                //this.context = new AuthenticationContext(string.Format("https://login.windows.net/{0}", tenantId), false);// new AuthenticationContext(authority);
            }

            public bool RefreshAccessToken(AppTokenAuthObject authObj, bool force)
            {
                #region token expires
                //http://www.cloudidentity.com/blog/2015/03/20/azure-ad-token-lifetime/
                //•Access tokens last 1 hour
                //•Refresh tokens last for 14 days, but
                //•If you use a refresh token within those 14 days, you will receive a new one with a new validity window shifted forward of another 14 days. You can repeat this trick for up to 90 days of total validity, then you’ll have to reauthenticate
                //•Refresh tokens can be invalidated at ANY time, for reasons independent from your app. Hence you should NOT take a dependency on the above in your code – your logic should always assume that the refresh token can fail at any time
                //•Refresh tokens issues for guest MSA accounts last only 12 hours
                #endregion

                CheckArgs(authObj);

                if (authObj.expiresOn != null && (authObj.expiresOn - DateTimeOffset.UtcNow > Token_EXPIRES_EDGE)) return false;
                var result = AcquireToken(authObj);

                //var result = AvePoint.GCommon.Utility.Cloud.AppTokenHelper.GetTokenFromCert(authObj.tenantId, authObj.clientId, authObj.ResourceUrl, authObj.appOnlyCertificate);
                //var result = this.context.AcquireTokenByRefreshToken(authObj.refreshToken, authObj.clientId, authObj.Resource);
                RefreshAuthObj(result, authObj);
                //LogAccessToken(authObj);
                return true;

            }

            public virtual EXOTokenItem AcquireToken(AppTokenAuthObject authObj)
            {
                //var authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(string.Format("{0}/{1}", authObj.authority, authObj.tenantId), false);
                //var cac = new ClientAssertionCertificate(authObj.clientId, authObj.appOnlyCertificate);
                //var result = authenticationContext.AcquireTokenAsync(authObj.ResourceUrl, cac).Result;
                EXOTokenItem result = null;
                try
                {
                    result = AcquireTokenFromAOS(authObj);
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while get token from aos, error:{e.ToString()}");
                    throw;
                }
                //no longer get token locally
                //if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
                //{
                //    result = AcquireTokenFromLocal(authObj);
                //}
                return result;
            }



            private EXOTokenItem AcquireTokenFromAOS(AppTokenAuthObject authObj)
            {
                logger.Info("Start to get token with MSAL from AOS for app profile.");
                TokenParam tokenParam = new TokenParam()
                {
                    CustomerId = authObj.tenantGroupId,
                    SpTokenType = SharePointTokenType.Bearer,
                    TenantId = authObj.tenantId,
                    AppType = authObj.appType,
                    TokenMethod = TokenMethod.MSAL,
                    Identity = authObj.appId,
                    Resource = authObj.ResourceUrl,
                    ClientId = authObj.clientId
                    //SiteUrl = authObj.siteUrl
                };
                AvePoint.GCommon.Utility.AosTokenResult aosToken = AvePoint.Common.Portal.PortalUtil.GetTokenByAOSNewSDKForEXO(tokenParam);

                return new EXOTokenItem(aosToken.AccessToken, "Bearer", aosToken.ExpiresOn);
            }





            private void CheckArgs(AppTokenAuthObject authObj)
            {
                if (authObj == null) throw new ArgumentNullException("authObj");
                //if (string.IsNullOrEmpty(authObj.refreshToken)) throw new ArgumentNullException("authObj.RefreshToken");
                if (string.IsNullOrEmpty(authObj.tenantId)) throw new ArgumentNullException("authObj.TenantId");
                if (string.IsNullOrEmpty(authObj.clientId)) throw new ArgumentNullException("authObj.ClientId");
                if (string.IsNullOrEmpty(authObj.ResourceUrl)) throw new ArgumentNullException("authObj.Resource");
            }

            private void RefreshAuthObj(EXOTokenItem result, AppTokenAuthObject authObj)
            {
                authObj.accessToken = result.AccessToken;
                authObj.accessTokenType = result.AccessTokenType;
                //authObj.refreshToken = result.RefreshToken;
                authObj.expiresOn = result.ExpiresOn;
                //authObj.userInfo = result.UserInfo;
                //todo: save back to database
            }
        }

        public class AppInfo
        {
            public string AppId { get; set; }
            public string ClientId { get; set; }
            public X509Certificate2 Certificate { get; set; }
            public AvePoint.GCommon.Contract.CentralAdmin.Object.AppType AppType { get; set; }
        }

        public class AuthenticationInfo
        {
            public string TenantId { get; set; }
            public string Authority { get; set; }
            public string Resource { get; set; }
            public string SiteUrl { get; set; }
            public string TenantGroupId { get; set; }
        }

        public class EXOTokenItem
        {
            public string AccessToken { get; private set; }

            //private string refreshToken;

            public string AccessTokenType { get; private set; }

            public DateTimeOffset ExpiresOn { get; private set; }

            //public UserInfo UserInfo { get; private set; }

            public EXOTokenItem(string accessToken, string accessTokenType, DateTimeOffset expiresOn)
            {
                AccessToken = accessToken;
                AccessTokenType = accessTokenType;
                ExpiresOn = expiresOn;
            }
        }

    }
}
