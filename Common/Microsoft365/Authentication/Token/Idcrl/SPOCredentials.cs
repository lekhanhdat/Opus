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
    using Microsoft365.Authentication.Configuration;
    using Microsoft365.Authentication.Extension;
    using Microsoft365.Authentication.Token.BearToken;
    using Microsoft365.Authentication.Token.Modern;
    using Microsoft365.Common.Exception;
    using Microsoft365.Common.Extension;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security;

    internal sealed class SPOCredentials : ICredentials
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(SPOCredentials));
        internal class CookieCacheEntry
        {
            /// <summary>
            /// Valid the pwd hash.
            /// </summary>
            public int PasswordHash;

            public string Cookie;

            public DateTime Expires;

            public bool IsValid
            {
                get
                {
                    return DateTime.UtcNow < Expires;
                }
            }
        }


        private readonly string userName;

        private readonly SecureString password;
        private readonly int passwordHashcode;

        private readonly AveAzureEnvironment environment;

        private static CookieCacheEntryCache cache = new CookieCacheEntryCache(Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.MaxCacheInstance);

        public event EventHandler<SPOCredentialsWebRequestEventArgs> ExecutingWebRequest;

        public string UserName { get { return userName; } }

        public SPOModernAuthenticationProvider SPOModernAuthenticationProvider { get; set; }

        private bool UseBasic { get; set; }

        public SPOCredentials(string username, SecureString password, AveAzureEnvironment env)
        {
            username.ArgumentNullValidation("username");
            password.ArgumentNullValidation("password");
            int num = username.IndexOf('@');
            if (num < 0 || num == username.Length - 1)
            {
                throw new ArgumentException(Mirosoft365ApiErrorMessage.InvalidEmailFormat(username));
            }
            //SPOAuthenticationModule.EnsureRegistered();
            userName = username;
            this.password = password;
            passwordHashcode = password.GetHashCodeV1();
            this.environment = env;
            UseBasic = false;
            SPOModernAuthenticationProvider = new SPOModernAuthenticationProvider(new DelegateUserTokenProvider(username, password, this.environment), DefaultTokenTypeConverter.Instance);

        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        //public string GetAuthenticationCookie(Uri url)
        //{
        //	return this.GetAuthenticationCookie(url, true, false);
        //}

        //public string GetAuthenticationCookie(Uri url, bool alwaysThrowOnFailure)
        //{
        //	return this.GetAuthenticationCookie(url, true, alwaysThrowOnFailure);
        //}

        internal string GetAuthenticationCookie(Uri url, bool refresh, bool alwaysThrowOnFailure)
        {
            var originalUrl = url;
            url.ArgumentNullValidation("url");
            if (!url.IsAbsoluteUri)
            {
                throw new ArgumentException(Mirosoft365ApiErrorMessage.NotAbsoluteUrlFormat(url));
            }
            Uri uri = new Uri(url, "/");
            string text = null;

            var key = string.Concat(uri, "-", userName);

            var cookieCacheEntry = cache.Get(key);
            if (!refresh && cookieCacheEntry != null && cookieCacheEntry.IsValid && cookieCacheEntry.PasswordHash == passwordHashcode)
            {
                //ClientULS.SendTraceTag(3454916u, ClientTraceCategory.Authentication, ClientTraceLevel.Verbose, "Get cookie from cache for URL {0}", new object[]
                //{
                //	uri
                //});
                text = cookieCacheEntry.Cookie;
            }
            else //if (refresh)
            {
                RequireTokenNotification(userName, TokenType.IDCLR.ToString());
                if (UseBasic)
                {
                    text = new SPOAuthenticationProvider().GetAuthenticationCookie(uri, userName, password, environment, alwaysThrowOnFailure, ExecutingWebRequest);
                }
                else
                {
                    try
                    {
                        text = SPOModernAuthenticationProvider.GetAuthenticationCookie(originalUrl, alwaysThrowOnFailure, ExecutingWebRequest);
                        if (string.IsNullOrEmpty(text))
                        {
                            UseBasic = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get SPO Modern token failed, change to get IDCLR token, Error:{0}", ex);
                        UseBasic = true;
                    }
                    if (UseBasic)
                    {
                        // need to do:if get basic cookie failed, return the token and change the TokenType to Bearer
                        text = new SPOAuthenticationProvider().GetAuthenticationCookie(uri, userName, password, environment, alwaysThrowOnFailure, ExecutingWebRequest);
                    }
                }
                if (!string.IsNullOrEmpty(text))
                {
                    cookieCacheEntry = new CookieCacheEntry
                    {
                        Cookie = text,
                        Expires = DateTime.UtcNow.AddHours(1.0),
                        PasswordHash = passwordHashcode
                    };
                    cache.AddOrUpdate(key, cookieCacheEntry);
                }
            }
            return text;
        }

        private void RequireTokenNotification(string identity, string identityType)
        {
            Microsoft365Configuration.AuthenticationConfiguration.BeforeRequestTokenEvent?.Invoke(new BeforeGetTokenArg
            {
                Identity = identity,
                IdentityType = identityType
            });
        }

        internal class CookieCacheEntryCache
        {
            private Dictionary<string, CookieCacheEntry> caches = new Dictionary<string, CookieCacheEntry>(StringComparer.OrdinalIgnoreCase);

            public int Capacity { get; set; }

            public CookieCacheEntryCache(int capacity)
            {
                Capacity = capacity;
            }

            public CookieCacheEntry Get(string key)
            {
                CookieCacheEntry entry = null;
                lock (caches)
                {
                    caches.TryGetValue(key, out entry);
                }

                return entry;
            }

            public void AddOrUpdate(string key, CookieCacheEntry entry)
            {
                lock (caches)
                {
                    if (caches.ContainsKey(key))
                    {
                        caches[key] = entry;
                    }
                    else
                    {
                        var capacity = Capacity;

                        if (caches.Count > capacity)
                        {
                            var items = caches.OrderBy(k => k.Value.Expires).Take(caches.Count - capacity);
                            foreach (var item in items)
                            {
                                logger.Info("Clean the cache:{0} with expire:{1}", item.Key, item.Value.Expires);
                                caches.Remove(item.Key);
                            }
                        }

                        caches[key] = entry;
                    }
                }
            }
        }
    }
}