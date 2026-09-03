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

using AvePoint.GCommon;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security.Principal;
using System.Web;

namespace RASPAppWeb
{
    public abstract class SharePointContext
    {
        public const string SPHostUrlKey = "SPHostUrl";
        public const string SPAppWebUrlKey = "SPAppWebUrl";
        public const string SPLanguageKey = "SPLanguage";
        public const string SPClientTagKey = "SPClientTag";
        public const string SPProductNumberKey = "SPProductNumber";

        protected static readonly long AccessTokenLifetimeTolerance = 5 * 60; // 5 minutes

        private readonly Uri spHostUrl;
        private readonly Uri spAppWebUrl;
        private readonly string spLanguage;
        private readonly string spClientTag;
        private readonly string spProductNumber;

        // <AccessTokenString, UtcExpiresOn in Epoch time>
        protected Tuple<string, long> userAccessTokenForSPHost;
        protected Tuple<string, long> userAccessTokenForSPAppWeb;
        protected Tuple<string, long> appOnlyAccessTokenForSPHost;
        protected Tuple<string, long> appOnlyAccessTokenForSPAppWeb;

        /// <summary>
        /// Gets the SharePoint host url from QueryString of the specified HTTP request.
        /// </summary>
        /// <param name="httpRequest">The specified HTTP request.</param>
        /// <returns>The SharePoint host url. Returns <c>null</c> if the HTTP request doesn't contain the SharePoint host url.</returns>
        public static Uri GetSPHostUrl(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException("httpRequest");
            }

            string spHostUrlString = TokenHelper.EnsureTrailingSlash(httpRequest.Query[SPHostUrlKey]);

            if (Uri.TryCreate(spHostUrlString, UriKind.Absolute, out Uri spHostUrl) &&
                (spHostUrl.Scheme == Uri.UriSchemeHttp || spHostUrl.Scheme == Uri.UriSchemeHttps))
            {
                return spHostUrl;
            }

            return null;
        }

        /// <summary>
        /// The SharePoint host url.
        /// </summary>
        public Uri SPHostUrl
        {
            get { return this.spHostUrl; }
        }

        /// <summary>
        /// The SharePoint app web url.
        /// </summary>
        public Uri SPAppWebUrl
        {
            get { return this.spAppWebUrl; }
        }

        /// <summary>
        /// The SharePoint language.
        /// </summary>
        public string SPLanguage
        {
            get { return this.spLanguage; }
        }

        /// <summary>
        /// The SharePoint client tag.
        /// </summary>
        public string SPClientTag
        {
            get { return this.spClientTag; }
        }

        /// <summary>
        /// The SharePoint product number.
        /// </summary>
        public string SPProductNumber
        {
            get { return this.spProductNumber; }
        }

        /// <summary>
        /// The user access token for the SharePoint host.
        /// </summary>
        public abstract string UserAccessTokenForSPHost
        {
            get;
        }

        /// <summary>
        /// The user access token for the SharePoint app web.
        /// </summary>
        public abstract string UserAccessTokenForSPAppWeb
        {
            get;
        }

        /// <summary>
        /// The app only access token for the SharePoint host.
        /// </summary>
        public abstract string AppOnlyAccessTokenForSPHost
        {
            get;
        }

        /// <summary>
        /// The app only access token for the SharePoint app web.
        /// </summary>
        public abstract string AppOnlyAccessTokenForSPAppWeb
        {
            get;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="spHostUrl">The SharePoint host url.</param>
        /// <param name="spAppWebUrl">The SharePoint app web url.</param>
        /// <param name="spLanguage">The SharePoint language.</param>
        /// <param name="spClientTag">The SharePoint client tag.</param>
        /// <param name="spProductNumber">The SharePoint product number.</param>
        protected SharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber)
        {
            if (spHostUrl == null)
            {
                throw new ArgumentNullException("spHostUrl");
            }

            if (string.IsNullOrEmpty(spLanguage))
            {
                throw new ArgumentNullException("spLanguage");
            }

            if (string.IsNullOrEmpty(spClientTag))
            {
                throw new ArgumentNullException("spClientTag");
            }

            if (string.IsNullOrEmpty(spProductNumber))
            {
                throw new ArgumentNullException("spProductNumber");
            }

            this.spHostUrl = spHostUrl;
            this.spAppWebUrl = spAppWebUrl;
            this.spLanguage = spLanguage;
            this.spClientTag = spClientTag;
            this.spProductNumber = spProductNumber;
        }

        /// <summary>
        /// Creates a user ClientContext for the SharePoint host.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateUserClientContextForSPHost()
        {
            return CreateClientContext(this.SPHostUrl, this.UserAccessTokenForSPHost);
        }

        /// <summary>
        /// Creates a user ClientContext for the SharePoint app web.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateUserClientContextForSPAppWeb()
        {
            return CreateClientContext(this.SPAppWebUrl, this.UserAccessTokenForSPAppWeb);
        }

        /// <summary>
        /// Creates app only ClientContext for the SharePoint host.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateAppOnlyClientContextForSPHost()
        {
            return CreateClientContext(this.SPHostUrl, this.AppOnlyAccessTokenForSPHost);
        }

        /// <summary>
        /// Creates an app only ClientContext for the SharePoint app web.
        /// </summary>
        /// <returns>A ClientContext instance.</returns>
        public ClientContext CreateAppOnlyClientContextForSPAppWeb()
        {
            return CreateClientContext(this.SPAppWebUrl, this.AppOnlyAccessTokenForSPAppWeb);
        }

        /// <summary>
        /// Gets the database connection string from SharePoint for autohosted add-in.
        /// This method is deprecated because the autohosted option is no longer available.
        /// </summary>
        [ObsoleteAttribute("This method is deprecated because the autohosted option is no longer available.", true)]
        public string GetDatabaseConnectionString()
        {
            throw new NotSupportedException("This method is deprecated because the autohosted option is no longer available.");
        }

        /// <summary>
        /// Determines if the specified access token is valid.
        /// It considers an access token as not valid if it is null, or it has expired.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <returns>True if the access token is valid.</returns>
        protected static bool IsAccessTokenValid(Tuple<string, long> accessToken)
        {
            return accessToken != null &&
                   !string.IsNullOrEmpty(accessToken.Item1) &&
                   accessToken.Item2 > TokenHelper.EpochTimeNow();
        }

        /// <summary>
        /// Creates a ClientContext with the specified SharePoint site url and the access token.
        /// </summary>
        /// <param name="spSiteUrl">The site url.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>A ClientContext instance.</returns>
        private static ClientContext CreateClientContext(Uri spSiteUrl, string accessToken)
        {
            if (spSiteUrl != null && !string.IsNullOrEmpty(accessToken))
            {
                return TokenHelper.GetClientContextWithAccessToken(spSiteUrl.AbsoluteUri, accessToken);
            }

            return null;
        }
    }

    /// <summary>
    /// Redirection status.
    /// </summary>
    public enum RedirectionStatus
    {
        Ok,
        ShouldRedirect,
        CanNotRedirect
    }

    /// <summary>
    /// Provides SharePointContext instances.
    /// </summary>
    public abstract class SharePointContextProvider
    {
        private static SharePointContextProvider current;

        /// <summary>
        /// The current SharePointContextProvider instance.
        /// </summary>
        public static SharePointContextProvider Current
        {
            get { return SharePointContextProvider.current; }
        }

        /// <summary>
        /// Initializes the default SharePointContextProvider instance.
        /// </summary>
        static SharePointContextProvider()
        {
            if (!TokenHelper.IsHighTrustApp())
            {
                SharePointContextProvider.current = new SharePointAcsContextProvider();
            }
            else
            {
                SharePointContextProvider.current = new SharePointHighTrustContextProvider();
            }
        }

        /// <summary>
        /// Creates a SharePointContext instance with the specified HTTP request.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>The SharePointContext instance. Returns <c>null</c> if errors occur.</returns>
        public SharePointContext CreateSharePointContext(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException("httpRequest");
            }

            // SPHostUrl
            Uri spHostUrl = SharePointContext.GetSPHostUrl(httpRequest);
            if (spHostUrl == null)
            {
                return null;
            }

            // SPAppWebUrl
            string spAppWebUrlString = TokenHelper.EnsureTrailingSlash(httpRequest.Query[SharePointContext.SPAppWebUrlKey]);
            Uri spAppWebUrl;
            if (!Uri.TryCreate(spAppWebUrlString, UriKind.Absolute, out spAppWebUrl) ||
                !(spAppWebUrl.Scheme == Uri.UriSchemeHttp || spAppWebUrl.Scheme == Uri.UriSchemeHttps))
            {
                spAppWebUrl = null;
            }

            // SPLanguage
            string spLanguage = httpRequest.Query[SharePointContext.SPLanguageKey];
            if (string.IsNullOrEmpty(spLanguage))
            {
                return null;
            }

            // SPClientTag
            string spClientTag = httpRequest.Query[SharePointContext.SPClientTagKey];
            if (string.IsNullOrEmpty(spClientTag))
            {
                return null;
            }

            // SPProductNumber
            string spProductNumber = httpRequest.Query[SharePointContext.SPProductNumberKey];
            if (string.IsNullOrEmpty(spProductNumber))
            {
                return null;
            }

            return CreateSharePointContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, httpRequest);
        }

        /// <summary>
        /// Creates a SharePointContext instance.
        /// </summary>
        /// <param name="spHostUrl">The SharePoint host url.</param>
        /// <param name="spAppWebUrl">The SharePoint app web url.</param>
        /// <param name="spLanguage">The SharePoint language.</param>
        /// <param name="spClientTag">The SharePoint client tag.</param>
        /// <param name="spProductNumber">The SharePoint product number.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>The SharePointContext instance. Returns <c>null</c> if errors occur.</returns>
        protected abstract SharePointContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, HttpRequest httpRequest);

        /// <summary>
        /// Validates if the given SharePointContext can be used with the specified HTTP context.
        /// </summary>
        /// <param name="spContext">The SharePointContext.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>True if the given SharePointContext can be used with the specified HTTP context.</returns>
        protected abstract bool ValidateSharePointContext(SharePointContext spContext, HttpContext httpContext);

        /// <summary>
        /// Loads the SharePointContext instance associated with the specified HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The SharePointContext instance. Returns <c>null</c> if not found.</returns>
        protected abstract SharePointContext LoadSharePointContext(HttpContext httpContext);

        /// <summary>
        /// Saves the specified SharePointContext instance associated with the specified HTTP context.
        /// <c>null</c> is accepted for clearing the SharePointContext instance associated with the HTTP context.
        /// </summary>
        /// <param name="spContext">The SharePointContext instance to be saved, or <c>null</c>.</param>
        /// <param name="httpContext">The HTTP context.</param>
        protected abstract void SaveSharePointContext(SharePointContext spContext, HttpContext httpContext);
    }

    #region ACS

    /// <summary>
    /// Encapsulates all the information from SharePoint in ACS mode.
    /// </summary>
    public class SharePointAcsContext : SharePointContext
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(SharePointAcsContext));
        private readonly string contextToken;
        private readonly SharePointContextToken contextTokenObj;

        /// <summary>
        /// The context token.
        /// </summary>
        public string ContextToken
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextToken : null; }
        }

        /// <summary>
        /// The context token's "CacheKey" claim.
        /// </summary>
        public string CacheKey
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextTokenObj.CacheKey : null; }
        }

        /// <summary>
        /// The context token's "refreshtoken" claim.
        /// </summary>
        public string RefreshToken
        {
            get { return this.contextTokenObj.ValidTo > DateTime.UtcNow ? this.contextTokenObj.RefreshToken : null; }
        }

        public override string UserAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.userAccessTokenForSPHost,
                                            () => TokenHelper.GetAccessToken(this.contextTokenObj, this.SPHostUrl.Authority));
            }
        }

        public override string UserAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.userAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetAccessToken(this.contextTokenObj, this.SPAppWebUrl.Authority));
            }
        }

        public override string AppOnlyAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPHost,
                                            () => TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, this.SPHostUrl.Authority, TokenHelper.GetRealmFromTargetUrl(this.SPHostUrl)));
            }
        }

        public override string AppOnlyAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, this.SPAppWebUrl.Authority, TokenHelper.GetRealmFromTargetUrl(this.SPAppWebUrl)));
            }
        }

        public SharePointAcsContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, string contextToken, SharePointContextToken contextTokenObj)
            : base(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber)
        {
            if (string.IsNullOrEmpty(contextToken))
            {
                throw new ArgumentNullException("contextToken");
            }

            if (contextTokenObj == null)
            {
                throw new ArgumentNullException("contextTokenObj");
            }

            this.contextToken = contextToken;
            this.contextTokenObj = contextTokenObj;
        }

        /// <summary>
        /// Ensures the access token is valid and returns it.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        /// <returns>The access token string.</returns>
        private static string GetAccessTokenString(ref Tuple<string, long> accessToken, Func<OAuthTokenResponse> tokenRenewalHandler)
        {
            RenewAccessTokenIfNeeded(ref accessToken, tokenRenewalHandler);

            return IsAccessTokenValid(accessToken) ? accessToken.Item1 : null;
        }

        /// <summary>
        /// Renews the access token if it is not valid.
        /// </summary>
        /// <param name="accessToken">The access token to renew.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        private static void RenewAccessTokenIfNeeded(ref Tuple<string, long> accessToken, Func<OAuthTokenResponse> tokenRenewalHandler)
        {
            if (IsAccessTokenValid(accessToken))
            {
                return;
            }

            try
            {
                OAuthTokenResponse oauthTokenResponse = tokenRenewalHandler();

                long expiresOn = oauthTokenResponse.ExpiresOn;

                if ((expiresOn - oauthTokenResponse.NotBefore) > AccessTokenLifetimeTolerance)
                {
                    // Make the access token get renewed a bit earlier than the time when it expires
                    // so that the calls to SharePoint with it will have enough time to complete successfully.
                    expiresOn -= AccessTokenLifetimeTolerance;
                }

                accessToken = Tuple.Create(oauthTokenResponse.AccessToken, expiresOn);
            }
            catch (WebException e)
            {
                logger.Error($"error occured when RenewAccessTokenIfNeeded,error:{e}");
            }
        }
    }

    /// <summary>
    /// Default provider for SharePointAcsContext.
    /// </summary>
    public class SharePointAcsContextProvider : SharePointContextProvider
    {
        private const string SPContextKey = "SPContext";
        private const string SPCacheKeyKey = "SPCacheKey";

        protected override SharePointContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, HttpRequest httpRequest)
        {
            string contextTokenString = TokenHelper.GetContextTokenFromRequest(httpRequest);
            if (string.IsNullOrEmpty(contextTokenString))
            {
                return null;
            }

            SharePointContextToken contextToken = null;
            try
            {
                contextToken = TokenHelper.ReadAndValidateContextToken(contextTokenString, httpRequest.GetUrl().Authority);
            }
            catch (WebException)
            {
                return null;
            }
            catch (AudienceUriValidationFailedException)
            {
                return null;
            }

            return new SharePointAcsContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, contextTokenString, contextToken);
        }

        protected override bool ValidateSharePointContext(SharePointContext spContext, HttpContext httpContext)
        {
            SharePointAcsContext spAcsContext = spContext as SharePointAcsContext;

            if (spAcsContext != null)
            {
                Uri spHostUrl = SharePointContext.GetSPHostUrl(httpContext.Request);
                string contextToken = TokenHelper.GetContextTokenFromRequest(httpContext.Request);
                var spCacheKey = httpContext.Request.Cookies[SPCacheKeyKey];

                return spHostUrl == spAcsContext.SPHostUrl &&
                       !string.IsNullOrEmpty(spAcsContext.CacheKey) &&
                       spCacheKey == spAcsContext.CacheKey &&
                       !string.IsNullOrEmpty(spAcsContext.ContextToken) &&
                       (string.IsNullOrEmpty(contextToken) || contextToken == spAcsContext.ContextToken);
            }

            return false;
        }

        protected override SharePointContext LoadSharePointContext(HttpContext httpContext)
        {
            return httpContext.Session.GetObject<SharePointAcsContext>(SPContextKey);
        }

        protected override void SaveSharePointContext(SharePointContext spContext, HttpContext httpContext)
        {
            SharePointAcsContext spAcsContext = spContext as SharePointAcsContext;

            if (spAcsContext != null)
            {
                var spCacheKeyCookieOpt = new CookieOptions()
                {
                    Secure = true,
                    HttpOnly = true,
                    Domain = spContext.SPHostUrl.Host,
                    SameSite = SameSiteMode.Lax,
                };
                httpContext.Response.Cookies.Append(SPCacheKeyKey, spAcsContext.CacheKey, spCacheKeyCookieOpt);
            }

            httpContext.Session.SetObject(SPContextKey, spAcsContext);
        }
    }

    #endregion ACS

    #region HighTrust

    /// <summary>
    /// Encapsulates all the information from SharePoint in HighTrust mode.
    /// </summary>
    public class SharePointHighTrustContext : SharePointContext
    {
        private readonly WindowsIdentity logonUserIdentity;

        /// <summary>
        /// The Windows identity for the current user.
        /// </summary>
        public WindowsIdentity LogonUserIdentity
        {
            get { return this.logonUserIdentity; }
        }

        public override string UserAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.userAccessTokenForSPHost,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPHostUrl, this.LogonUserIdentity));
            }
        }

        public override string UserAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.userAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPAppWebUrl, this.LogonUserIdentity));
            }
        }

        public override string AppOnlyAccessTokenForSPHost
        {
            get
            {
                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPHost,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPHostUrl, null));
            }
        }

        public override string AppOnlyAccessTokenForSPAppWeb
        {
            get
            {
                if (this.SPAppWebUrl == null)
                {
                    return null;
                }

                return GetAccessTokenString(ref this.appOnlyAccessTokenForSPAppWeb,
                                            () => TokenHelper.GetS2SAccessTokenWithWindowsIdentity(this.SPAppWebUrl, null));
            }
        }

        public SharePointHighTrustContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, WindowsIdentity logonUserIdentity)
            : base(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber)
        {
            if (logonUserIdentity == null)
            {
                throw new ArgumentNullException("logonUserIdentity");
            }

            this.logonUserIdentity = logonUserIdentity;
        }

        /// <summary>
        /// Ensures the access token is valid and returns it.
        /// </summary>
        /// <param name="accessToken">The access token to verify.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        /// <returns>The access token string.</returns>
        private static string GetAccessTokenString(ref Tuple<string, long> accessToken, Func<string> tokenRenewalHandler)
        {
            RenewAccessTokenIfNeeded(ref accessToken, tokenRenewalHandler);

            return IsAccessTokenValid(accessToken) ? accessToken.Item1 : null;
        }

        /// <summary>
        /// Renews the access token if it is not valid.
        /// </summary>
        /// <param name="accessToken">The access token to renew.</param>
        /// <param name="tokenRenewalHandler">The token renewal handler.</param>
        private static void RenewAccessTokenIfNeeded(ref Tuple<string, long> accessToken, Func<string> tokenRenewalHandler)
        {
            if (IsAccessTokenValid(accessToken))
            {
                return;
            }

            long expiresOn = TokenHelper.EpochTimeNow() + (long)TokenHelper.HighTrustAccessTokenLifetime.TotalSeconds;

            if (TokenHelper.HighTrustAccessTokenLifetime.TotalSeconds > AccessTokenLifetimeTolerance)
            {
                // Make the access token get renewed a bit earlier than the time when it expires
                // so that the calls to SharePoint with it will have enough time to complete successfully.
                expiresOn -= AccessTokenLifetimeTolerance;
            }

            accessToken = Tuple.Create(tokenRenewalHandler(), expiresOn);
        }
    }

    /// <summary>
    /// Default provider for SharePointHighTrustContext.
    /// </summary>
    public class SharePointHighTrustContextProvider : SharePointContextProvider
    {
        private const string SPContextKey = "SPContext";

        protected override SharePointContext CreateSharePointContext(Uri spHostUrl, Uri spAppWebUrl, string spLanguage, string spClientTag, string spProductNumber, HttpRequest httpRequest)
        {
            throw new NotImplementedException();
            //WindowsIdentity logonUserIdentity = httpRequest.LogonUserIdentity;
            //if (logonUserIdentity == null || !logonUserIdentity.IsAuthenticated || logonUserIdentity.IsGuest || logonUserIdentity.User == null)
            //{
            //    return null;
            //}

            //return new SharePointHighTrustContext(spHostUrl, spAppWebUrl, spLanguage, spClientTag, spProductNumber, logonUserIdentity);
        }

        protected override bool ValidateSharePointContext(SharePointContext spContext, HttpContext httpContext)
        {
            SharePointHighTrustContext spHighTrustContext = spContext as SharePointHighTrustContext;

            if (spHighTrustContext != null)
            {
                Uri spHostUrl = SharePointContext.GetSPHostUrl(httpContext.Request);
                throw new NotImplementedException("Validate SharePointHighTrustContext");
                //WindowsIdentity logonUserIdentity = httpContext.Request.LogonUserIdentity;

                //return spHostUrl == spHighTrustContext.SPHostUrl &&
                //       logonUserIdentity != null &&
                //       logonUserIdentity.IsAuthenticated &&
                //       !logonUserIdentity.IsGuest &&
                //       logonUserIdentity.User == spHighTrustContext.LogonUserIdentity.User;
            }

            return false;
        }

        protected override SharePointContext LoadSharePointContext(HttpContext httpContext)
        {
            return httpContext.Session.GetObject<SharePointHighTrustContext>(SPContextKey);
        }

        protected override void SaveSharePointContext(SharePointContext spContext, HttpContext httpContext)
        {
            httpContext.Session.SetObject(SPContextKey, spContext);
        }
    }

    #endregion HighTrust
}
