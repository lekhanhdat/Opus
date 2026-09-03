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

namespace AvePoint.Wrapper.Common
{
    using GCommon;
    using System;
    using System.Collections;
    using System.Net;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    public sealed class SharePointOnlineCredentials : ICredentials
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        // Fields
        private const int CacheHours = 1;
        private EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest;
        private Hashtable m_cachedCookies = new Hashtable();
        private object m_lock = new object();
        private SecureString m_password;
        private string m_userName;

        // Events
        public event EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> ExecutingWebRequest
        {
            add
            {
                EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> handler2;
                EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest = this.executingWebRequest;
                do
                {
                    handler2 = executingWebRequest;
                    EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> handler3 = (EventHandler<SharePointOnlineCredentialsWebRequestEventArgs>)Delegate.Combine(handler2, value);
                    executingWebRequest = Interlocked.CompareExchange<EventHandler<SharePointOnlineCredentialsWebRequestEventArgs>>(ref this.executingWebRequest, handler3, handler2);
                }
                while (executingWebRequest != handler2);
            }
            remove
            {
                EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> handler2;
                EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest = this.executingWebRequest;
                do
                {
                    handler2 = executingWebRequest;
                    EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> handler3 = (EventHandler<SharePointOnlineCredentialsWebRequestEventArgs>)Delegate.Remove(handler2, value);
                    executingWebRequest = Interlocked.CompareExchange<EventHandler<SharePointOnlineCredentialsWebRequestEventArgs>>(ref this.executingWebRequest, handler3, handler2);
                }
                while (executingWebRequest != handler2);
            }
        }

        // Methods
        public SharePointOnlineCredentials(string username, SecureString password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            int index = username.IndexOf('@');
            if (index < 0 || index == username.Length - 1) throw new ArgumentNullException("username");
            if (password == null) throw new ArgumentNullException("password");
            SharePointOnlineAuthenticationModule.EnsureRegistered();
            this.m_userName = username;
            this.m_password = password;
        }

        public string GetAuthenticationCookie(Uri url)
        {
            return this.GetAuthenticationCookie(url, true, false);
        }

        public string GetAuthenticationCookie(Uri url, bool alwaysThrowOnFailure)
        {
            return this.GetAuthenticationCookie(url, true, alwaysThrowOnFailure);
        }

        internal string GetAuthenticationCookie(Uri url, bool refresh, bool alwaysThrowOnFailure)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (!url.IsAbsoluteUri) throw new ArgumentNullException("url");
            Uri uri = new Uri(url, "/");
            string str = null;
            CookieCacheEntry entry = (CookieCacheEntry)this.m_cachedCookies[uri];
            if (!refresh && entry != null && entry.IsValid)
            {
                log.Debug("Get cookie from cache for URL {0}", uri);
                return entry.Cookie;
            }
            if (refresh)
            {
                str = SharePointOnlineAuthenticationProviderHelper.CreateDefaultProvider().GetAuthenticationCookie(uri, this.m_userName, this.m_password, alwaysThrowOnFailure, this.executingWebRequest);
                if (string.IsNullOrEmpty(str)) return str;
                log.Debug("Put cookie in cache for URL {0}", uri);
                lock (this.m_lock)
                {
                    CookieCacheEntry entry2 = new CookieCacheEntry
                    {
                        Cookie = str,
                        Expires = DateTime.UtcNow.AddHours(1.0)
                    };
                    this.m_cachedCookies[uri] = entry2;
                }
            }
            return str;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        // Properties
        internal SecureString Password
        {
            get
            {
                return this.m_password;
            }
        }

        public string UserName
        {
            get
            {
                return this.m_userName;
            }
        }

        // Nested Types
        private class CookieCacheEntry
        {
            // Fields
            public string Cookie;
            public DateTime Expires;

            // Properties
            public bool IsValid
            {
                get
                {
                    return (DateTime.UtcNow < this.Expires);
                }
            }
        }
    }
}

