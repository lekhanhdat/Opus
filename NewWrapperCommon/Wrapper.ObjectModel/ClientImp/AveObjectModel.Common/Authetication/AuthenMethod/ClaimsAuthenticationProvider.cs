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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    public class ClaimsAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ClaimsAuthenticationProvider));
        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            try
            {
                string webapp = GetServerUrl(siteUrl);
                string serverRelativeUrl = GetServerRelativeUrl(siteUrl);
                if (!string.Equals(serverRelativeUrl, "/"))
                {
                    serverRelativeUrl = serverRelativeUrl.TrimEnd('/');
                }
                AveHttpValueCollection values = new AveHttpValueCollection();
                values["ReturnUrl"] = serverRelativeUrl.TrimEnd('/') + "/_layouts/Authenticate.aspx?Source=" + serverRelativeUrl;
                values["Source"] = serverRelativeUrl;
                string url = webapp.TrimEnd('/') + "/_windows/default.aspx?" + values.ToString(true);
                //string url = webapp.TrimEnd('/') + "/_windows/default.aspx?ReturnUrl=" + serverRelativeUrl.TrimEnd('/') + "/_layouts/Authenticate.aspx?Source=" + serverRelativeUrl + "&Source=" + serverRelativeUrl;

                CookieContainer myCookieContainer = new CookieContainer();
                HttpWebRequest windowsRequest = HttpWebRequest.Create(url) as HttpWebRequest;
                windowsRequest.Credentials = new NetworkCredential(userAccountInfo.UserName, userAccountInfo.Password);
                windowsRequest.CookieContainer = myCookieContainer;
                HttpWebResponse response = windowsRequest.GetResponse() as HttpWebResponse;
                var cookie = windowsRequest.CookieContainer;
                log.Debug("login site {0} successfully using claims authentication",siteUrl);
                return new AuthenticationResult(AutheStatus.Successful, AveAuthenticationMode.Claims, cookie);
            }
            catch(Exception e)
            {
                log.Warn("Login failed by Claim authentication. Url:{0}, user:{1}, Error:{2}", siteUrl, userAccountInfo.UserName,e);
                return new AuthenticationResult(AutheStatus.Failed, AveAuthenticationMode.Claims);
            }
        }

        private string GetServerUrl(string siteUrl)
        {
            if (string.IsNullOrEmpty(siteUrl) || !siteUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !siteUrl.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return siteUrl;
            }
            else
            {
                return new Uri(siteUrl).GetLeftPart(UriPartial.Authority);
            }
        }
        private string GetServerRelativeUrl(string webUrl)
        {
            if (string.IsNullOrEmpty(webUrl) || !webUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !webUrl.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return webUrl;
            }
            else
            {
                if (webUrl.Contains("+"))//ADO-110767, UrlDecode will change '+' to space.
                {
                    webUrl = webUrl.Replace("+", "%2b");
                }
                return System.Web.HttpUtility.UrlDecode(new Uri(webUrl).PathAndQuery);
            }
        }
        
    }
}
