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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft365.Authentication;


namespace AvePoint.Wrapper.Common
{
    public class AveTokenProvider : ITokenProvider
    {
        private readonly int waitTaskTimeoutLimit = 1000 * 60 * 3; //3min
        private readonly int retryTimes = 3; //retry to execute query with 3 times if throttle
        private readonly int sleepTime = 1000 * 3; // sleep 3 sec
        private Func<string, AvePoint.GCommon.Utility.AosTokenResult> _getToken;
        private AvePoint.GCommon.Utility.TokenParam _tokenParam;
        private CookieCacheEntryCache cache = new CookieCacheEntryCache(5000);//ConfigInstance.MaxCacheInstance
        private static AvePoint.GCommon.AveLogger mLogger = AvePoint.GCommon.AveLogger.GetInstance(typeof(AveTokenProvider));
        public Action GetTokenTimeOutHandle { get; set; }

        #region implement interface

        public AveTokenProvider(AvePoint.GCommon.Utility.TokenParam tokenParam, Func<string, AvePoint.GCommon.Utility.AosTokenResult> getToken = null)
        {
            ValidateTokenParam(tokenParam);
            this._tokenParam = tokenParam;
            //mLogger.Info("Out put TokenParam: {0}", tokenParam.ToString());
            if (getToken != null)
            {
                this._getToken = getToken;
            }
            else
            {
                this._getToken = GenerateGetTokenDelegate();
            }
        }

        public string Identifier
        {
            get
            {
                if (this._tokenParam.SpTokenType == AvePoint.GCommon.Utility.SharePointTokenType.IDCRL || this._tokenParam.SpTokenType == AvePoint.GCommon.Utility.SharePointTokenType.Bearer)
                {
                    //return $"{this._tokenParam.SiteUrl}-{this._tokenParam.Identity}";//{this._tokenParam.SpTokenType}-{this._tokenParam.TenantId}-
                    return this._tokenParam.Identity;
                }
                else
                {
                    throw new NotSupportedException($"Not support this type:({this._tokenParam.SpTokenType})'s identifier property.");
                }
            }
        }

        public TokenType TokenType
        {
            get
            {
                switch (this._tokenParam.SpTokenType)
                {
                    case AvePoint.GCommon.Utility.SharePointTokenType.Bearer:
                        return TokenType.Bearer;
                    case AvePoint.GCommon.Utility.SharePointTokenType.IDCRL:
                        return TokenType.Bearer;
                }
                throw new ArgumentException(this._tokenParam.SpTokenType.ToString());
            }
        }

        public System.Net.NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            ReSetSPUrl(url);
            string result = string.Empty;
            string key = GenerateCookieCacheKey(url);
            CookieCacheEntry cookieCacheEntry;
            if (!refresh && TryGetCookieCache(key, out cookieCacheEntry) && !IsExpireCache(cookieCacheEntry))
            {
                result = cookieCacheEntry.AccessToken;
                mLogger.Info($"GetToken from cache.");
            }
            else
            {
                result = RefreshToken(url, 1);
                mLogger.Info($"RefreshToken success.");
            }
            return result;
        }
        #endregion

        #region get token

        private string RefreshToken(Uri url, int bufferMinutes = 0)
        {
            AvePoint.GCommon.Utility.AosTokenResult tokenResult = GetTokenWithRetry(url, sleepTime, retryTimes);
            ValidTokenResult(tokenResult);
            string cookie = tokenResult.AccessToken;
            string key = GenerateCookieCacheKey(url);
            //For app profile:,request header:authorization need add prefix:'Bearer ', because office365dll cannot support this format
            if (TokenType == TokenType.Bearer)
            {
                cookie = string.Format("Bearer {0}", cookie);
                mLogger.Info($"Add prefix to token for appprofile mode.");
            }
            cache.AddOrUpdate(key, new CookieCacheEntry() { AccessToken = cookie, Expires = tokenResult.ExpiresOn.AddMinutes(-bufferMinutes) });
            return cookie;
        }

        private AvePoint.GCommon.Utility.AosTokenResult GetTokenWithRetry(Uri url, int sleepTime, int retryTimes)
        {
            AvePoint.GCommon.Utility.AosTokenResult tokenResult = default(AvePoint.GCommon.Utility.AosTokenResult);
            TokenRequestExecutor.RetryAction(() =>
            {
                tokenResult = HandleTokenTimeout(url);
                ValidTokenResult(tokenResult);
            }
            , sleepTime
            , retryTimes
            );
            return tokenResult;
        }

        private AvePoint.GCommon.Utility.AosTokenResult HandleTokenTimeout(Uri url)
        {
            AvePoint.GCommon.Utility.AosTokenResult result = new AvePoint.GCommon.Utility.AosTokenResult();
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                result = _getToken.Invoke(url.ToString());
            });
            if (!task.Wait(waitTaskTimeoutLimit))
            {
                throw new TimeoutException($"Get token with this url:{url.ToString()} time out:{waitTaskTimeoutLimit} ms, customerId:{_tokenParam.CustomerId}, tenantId:{_tokenParam.TenantId}, task status:{task.Status}");
            }
            return result;
        }

        #endregion

        #region cookie cache
        private bool TryGetCookieCache(string key, out CookieCacheEntry cookieCacheEntry)
        {
            try
            {
                cookieCacheEntry = cache.Get(key);
                if (cookieCacheEntry != null)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                cookieCacheEntry = null;
                mLogger.Error($"An error occured when try get cookie from cache, error:{0}", e);
            }
            return false;
        }

        private string GenerateCookieCacheKey(Uri url)
        {
            Uri uri;
            if (url == null)
            {
                uri = new Uri(_tokenParam.SiteUrl);
            }
            else
            {
                uri = new Uri(url, "/");
            }
            return string.Concat(uri, "-", this._tokenParam.Identity.LogBase64());
        }

        private bool IsExpireCache(CookieCacheEntry cookieCacheEntry)
        {
            return cookieCacheEntry != null && !string.IsNullOrWhiteSpace(cookieCacheEntry.AccessToken) ? !cookieCacheEntry.IsValid : true;
        }
        #endregion

        #region common
        private void ValidateTokenParam(AvePoint.GCommon.Utility.TokenParam info)
        {
            info.CheckForAveTokenProvider();
        }

        private void ReSetSPUrl(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("Refresh token url is null");
            }
            var newUrl = url.ToString();
            if (_tokenParam != null
                && !string.IsNullOrWhiteSpace(_tokenParam.SiteUrl)
                && !string.IsNullOrWhiteSpace(newUrl)
                && !string.Equals(_tokenParam.SiteUrl, newUrl, StringComparison.OrdinalIgnoreCase))
            {
                mLogger.Info($"Reset url, old:{_tokenParam.SiteUrl}, new：{newUrl}");
                _tokenParam.SiteUrl = newUrl;
            }
        }

        private void ValidTokenResult(AvePoint.GCommon.Utility.AosTokenResult tokenResult)
        {
            if (tokenResult == null)
            {
                throw new AveNullResultException("Token Result is null .");
            }
            if (!string.IsNullOrWhiteSpace(tokenResult.Error))
            {
                throw new AveErrorException(tokenResult.Error);
            }
            if (tokenResult.ExpiresOn < DateTimeOffset.Now)
            {
                throw new AveChangeTokenExpireException("Current token is expired.");
            }
            if (string.IsNullOrWhiteSpace(tokenResult.AccessToken))
            {
                throw new AveWrapperInvalidDataException();
            }
        }

        private Func<string, AvePoint.GCommon.Utility.AosTokenResult> GenerateGetTokenDelegate()
        {
            return (url) =>
            {
                _tokenParam.SiteUrl = url;
                AvePoint.GCommon.Utility.AosTokenResult aosToken = AvePoint.Common.Portal.PortalUtil.GetTokenByAOSNewSDK(_tokenParam);
                return aosToken;
            };
        }
        #endregion
    }
}

