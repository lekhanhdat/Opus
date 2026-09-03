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
    using System;
    using System.Net;

    internal class SharePointOnlineAuthenticationModule : IAuthenticationModule
    {
        private const string EmptyAuthorization = " ";
        private static SharePointOnlineAuthenticationModule s_instance;
        private static object s_lock = new object();

        private SharePointOnlineAuthenticationModule()
        {
        }

        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            SharePointOnlineCredentials spoCredentials = credentials as SharePointOnlineCredentials;
            if (spoCredentials != null)
            {
                bool preAuthentication = false;
                if (this.GetSpoAuthCookieAndUpdateRequest(request, spoCredentials, preAuthentication))
                {
                    return new Authorization(" ");
                }
            }
            return null;
        }

        internal static void EnsureRegistered()
        {
            if (s_instance == null)
            {
                lock (s_lock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new SharePointOnlineAuthenticationModule();
                        AuthenticationManager.Register(s_instance);
                    }
                }
            }
        }

        private bool GetSpoAuthCookieAndUpdateRequest(WebRequest request, SharePointOnlineCredentials spoCredentials, bool preAuthentication)
        {
            string authenticationCookie;
            string uriString = request.RequestUri.ToString();
            int index = uriString.IndexOf('?');
            if (index > 0)
            {
                uriString = uriString.Substring(0, index);
            }
            index = uriString.IndexOf('#');
            if (index > 0)
            {
                uriString = uriString.Substring(0, index);
            }
            index = uriString.IndexOf("/_vti_bin", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                uriString = uriString.Substring(0, index);
            }
            index = uriString.IndexOf("/_api", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                uriString = uriString.Substring(0, index);
            }
            Uri url = new Uri(uriString);
            if (preAuthentication)
            {
                bool refresh = false;
                authenticationCookie = spoCredentials.GetAuthenticationCookie(url, refresh);
                if (string.IsNullOrEmpty(authenticationCookie))
                {
                    bool flag2 = true;
                    authenticationCookie = spoCredentials.GetAuthenticationCookie(url, flag2);
                }
            }
            else
            {
                bool flag3 = true;
                authenticationCookie = spoCredentials.GetAuthenticationCookie(url, flag3);
            }
            if (!string.IsNullOrEmpty(authenticationCookie))
            {
                request.Headers[HttpRequestHeader.Cookie] = authenticationCookie;
                return true;
            }
            return false;
        }

        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            SharePointOnlineCredentials spoCredentials = credentials as SharePointOnlineCredentials;
            if (spoCredentials != null)
            {
                bool preAuthentication = true;
                this.GetSpoAuthCookieAndUpdateRequest(request, spoCredentials, preAuthentication);
            }
            return null;
        }

        public string AuthenticationType
        {
            get
            {
                return "SPOIDCRL";
            }
        }

        public bool CanPreAuthenticate
        {
            get
            {
                return true;
            }
        }
    }
}

