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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace AvePoint.ObjectModel.Common
{
    public class WindowsAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(WindowsAuthenticationProvider));
        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            try
            {
                HttpWebRequest webRequest = HttpWebRequest.Create(siteUrl.TrimEnd('/') + "/_layouts/Authenticate.aspx") as HttpWebRequest;
                AddCommonHeaders(webRequest, userAccountInfo);
                webRequest.AllowAutoRedirect = false;
                webRequest.KeepAlive = true;
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.Redirect && webResponse.Headers[HttpResponseHeader.Location].StartsWith("/_forms/default.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new WebException("Forms authentication is required.");
                    }
                    else if (webResponse.StatusCode == HttpStatusCode.Found && webResponse.Headers[HttpResponseHeader.Location].EndsWith("/_layouts/15/Authenticate.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpWebRequest webRequest1 = HttpWebRequest.Create(siteUrl.TrimEnd('/') + "/_layouts/15/Authenticate.aspx") as HttpWebRequest;
                        AddCommonHeaders(webRequest1, userAccountInfo);
                        webRequest1.AllowAutoRedirect = false;
                        using (HttpWebResponse webResponse1 = webRequest1.GetResponse() as HttpWebResponse)
                        {
                            if (webResponse1.StatusCode == HttpStatusCode.Redirect && webResponse1.Headers[HttpResponseHeader.Location].StartsWith("/_forms/default.aspx", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new WebException("Forms authentication is required. ");
                            }
                        }
                    }
                }
                var credentials = webRequest.Credentials;
                //EnsureFormDigest(siteUrl, credentials);
                log.Debug("login site {0} successfully using windows authentication", siteUrl);
                return new AuthenticationResult(AutheStatus.Successful, AveAuthenticationMode.Windows, credentials);
            }
            catch(Exception e)
            {
                log.Warn("Login failed by Windows authentication. Url:{0}, user:{1}, Error:{2}", siteUrl, userAccountInfo.UserName, e);
                return new AuthenticationResult(AutheStatus.Failed, AveAuthenticationMode.Windows);
            }
        }

        private void EnsureFormDigest(string siteUrl, object credential)
        {
            AveSiteService siteService = new AveSiteService(siteUrl + "/_vti_bin/Sites.asmx") { Timeout = 3 * 60 * 1000, Credentials = credential };
            try
            {
                var mDigestInfo = siteService.GetUpdatedFormDigestInformation(null);
            }
            catch (WebException e)
            {
                log.Info("EnsureFormDigest for site {0} error. Error Message: {1}", siteUrl, e);
                EnsureFormDigest(siteUrl, credential);
            }
        }
        private void AddCommonHeaders(HttpWebRequest webRequest, AveBPOSAccountInfo userAccountInfo)
        {
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E; .NET CLR 3.5.30729; .NET CLR 3.0.30729; InfoPath.2)";
            if (string.IsNullOrEmpty(userAccountInfo.Domain))
            {
                webRequest.Credentials = new NetworkCredential(userAccountInfo.UserName, userAccountInfo.Password);
            }
            else
            {
                webRequest.Credentials = new NetworkCredential(userAccountInfo.UserName, userAccountInfo.Password, userAccountInfo.Domain);
            }
        }
    }
    

}
