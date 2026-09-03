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
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections;
    using System.Net;
    using System.Security;

    public sealed class SharePointOnlineCredentials : ICredentials
    {
        private const int CacheHours = 1;
        private Hashtable m_cachedCookies = new Hashtable();
        private object m_lock = new object();
        private SecureString m_password;
        private string m_userName;

        public SharePointOnlineCredentials(string username, SecureString password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw ClientUtility.CreateArgumentNullException("username");
            }
            int index = username.IndexOf('@');
            if ((index < 0) || (index == (username.Length - 1)))
            {
                throw ClientUtility.CreateArgumentException("username");
            }
            if (password == null)
            {
                throw ClientUtility.CreateArgumentNullException("password");
            }
            SharePointOnlineAuthenticationModule.EnsureRegistered();
            this.m_userName = username;
            this.m_password = password;
        }

        public string GetAuthenticationCookie(Uri url)
        {
            bool refresh = true;
            return this.GetAuthenticationCookie(url, refresh);
        }

        internal string GetAuthenticationCookie(Uri url, bool refresh)
        {
            if (url == null)
            {
                throw ClientUtility.CreateArgumentNullException("url");
            }
            if (!url.IsAbsoluteUri)
            {
                throw ClientUtility.CreateArgumentException("url");
            }
            Uri uri = new Uri(url, "/");
            string str = null;
            CookieCacheEntry entry = (CookieCacheEntry) this.m_cachedCookies[uri];
            if ((!refresh && (entry != null)) && entry.IsValid)
            {
                return entry.Cookie;
            }
            if (refresh)
            {
                str = SharePointOnlineAuthenticationProviderHelper.CreateDefaultProvider().GetAuthenticationCookie(uri, this.m_userName, this.m_password);
                if (string.IsNullOrEmpty(str))
                {
                    return str;
                }
                lock (this.m_lock)
                {
                    CookieCacheEntry entry2 = new CookieCacheEntry();
                    entry2.Cookie = str;
                    entry2.Expires = DateTime.UtcNow.AddHours(1.0);
                    this.m_cachedCookies[uri] = entry2;
                }
            }
            return str;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

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

        private class CookieCacheEntry
        {
            public string Cookie;
            public DateTime Expires;

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

