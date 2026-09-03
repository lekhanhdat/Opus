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

using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft365.Authentication.Token;

/// <summary>
/// site collection url request Url rs,AppSiteUrlList.Any(t=>rs.startwith(t+"/")||rs==t)
/// key:WebUrl,Value SC Url
/// </summary>
public class AppSiteAuthenticationConvertUrlCache
{
    private static RALogger logger = RALogger.GetInstance(typeof(AppSiteAuthenticationConvertUrlCache));
    private static readonly object locker = new object();
    private static Dictionary<string, string> WebUrlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public static string GetMappedUrl(string url)
    {
        lock (locker)
        {
            string newUrl = WebUrlMap.Keys.FirstOrDefault(t => url.StartsWith(t + "/") || string.Equals(t, url, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(newUrl))
            {
                logger.Info($"[AppWebUrlMapping] Mapped - convert token url from {url} to {WebUrlMap[newUrl]}");
                return WebUrlMap[newUrl];
            }
            else
            {
                Uri newUri = new Uri(url);
                return newUri.GetLeftPart(UriPartial.Authority).ToString();
            }
        }
    }

    public static bool AddWebUrlMapping(string siteUrl, string webUrl)
    {
        lock (locker)
        {
            if (!WebUrlMap.ContainsKey(webUrl))
            {
                var appWebHost = new Uri(webUrl).GetLeftPart(UriPartial.Authority);
                var siteUrlHost = new Uri(siteUrl).GetLeftPart(UriPartial.Authority);
                var appWebSiteUrl = siteUrl.Replace(siteUrlHost, appWebHost);
                logger.Info($"[AppWebUrlMapping] Added - Url:{siteUrl},Replace {siteUrlHost} to {appWebHost},NewUrl:{appWebSiteUrl}");
                WebUrlMap.Add(webUrl, appWebSiteUrl);
                return true;
            }
            return false;
        }
    }
}