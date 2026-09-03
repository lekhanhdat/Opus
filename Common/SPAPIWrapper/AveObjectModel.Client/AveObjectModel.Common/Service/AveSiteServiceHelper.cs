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
using Microsoft365.Authentication;
using Microsoft365.SharePoint.WebService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    public class AveSiteServiceHelper : IAveSiteServiceHelper
    {
        private AveLogger mLog = AveLogger.GetInstance(typeof(AveSiteServiceHelper));

        private Func<string> CookieProvider { get; set; }
        public string TryToRectifySiteUrl(string url, IAveSite site)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<string> urls = site.AllWebs.Select(a => a.Url).OrderByDescending(a=>a.Length).ToList();
            mLog.Info($"linkRestoreReport load all webs by site cost time:{sw.ElapsedMilliseconds}.urls count:{urls.Count}.");
            sw.Stop();
            foreach (var webUrl in urls)
            {
                string tempWebUrl = string.Empty;
                if (!webUrl.EndsWith("/"))
                {
                    tempWebUrl = webUrl+"/";
                }
                if (url.StartsWith(tempWebUrl))
                {
                    return webUrl;
                }
            }
            return site.Url;
        }

        public string TryToRectifySiteUrl(string url, AveBPOSAccountInfo accountInfo)
        {
            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            int index = url.LastIndexOf('/');
            string urlHeader = url.StartsWith("https") ? "https://" : "http://";
            string leafname = url.Substring(index + 1);
            if (index > urlHeader.Length && leafname.Contains("."))
            {
                string tempUrl = url.Substring(0, index);
                if (tempUrl.IndexOf('/', urlHeader.Length) != tempUrl.LastIndexOf('/'))   //Subsite的url中允许存在英文句号，因此需要在这里判断，避免因为Subsite中的英文句号导致判断出错
                {
                    url = url.Substring(0, index);
                }
            }
            if (url.Contains("%"))
            {
                url = System.Web.HttpUtility.UrlDecode(url);
            }
            //some page url in sp2013 contains the following pattern, have to replace it before found out the right sitecollection url
            string sp15DeltaPrefix = "/_layouts/15/start.aspx#/";
            if (url.Contains(sp15DeltaPrefix))
            {
                url = url.Replace(sp15DeltaPrefix, "/");
            }
            sw3.Stop();
            mLog.Info($"linkRestoreReport generate url in TryToRectifySiteUrl cost time:{sw3.ElapsedMilliseconds}");
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            CookieProvider = () => { return accountInfo.Convert2TokenProvider().GetToken(new Uri(new Uri(url).GetLeftPart(UriPartial.Authority))); };
            sw1.Stop();
            mLog.Info($"linkRestoreReport TryToRectifySiteUrl cost time:{sw1.ElapsedMilliseconds}");
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            while (true)
            {
                try
                {
                    //AveSiteService siteService = CreateSiteService(url, accountInfo);
                    //string siteInfo = siteService.GetSite(url);
                    //if url contains % or #, this method will not work, and the web url cannot contains % or #, so we can start from the upper structure of these two characters.
                    string delimiter = ((Char)0x12).ToString();
                    if (url.Contains("%") || url.Contains("#") || url.Contains(delimiter, StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info($"Rectify url for illegal character: {url}");
                        index = url.LastIndexOf('/');
                        if (index > urlHeader.Length)
                        {
                            url = url.Substring(0, index);
                            continue;
                        }
                    }
                    using (var service = new SiteService(url, CookieProvider))
                    {
                        service.GetSite(url);
                    }
                }
                catch (HttpRequestException we)
                {
                    //using (HttpWebResponse response = we. as HttpWebResponse)
                    {
                        index = url.LastIndexOf('/');
                        if (we.StatusCode == System.Net.HttpStatusCode.NotFound && index > urlHeader.Length)
                        {
                            url = url.Substring(0, index);
                            continue;
                        }
                        else
                        {
                            mLog.Warn("rectify url {0} failed with webexception, Error Message:{1}", url, we.ToString());
                        }
                    }
                }
                catch (InvalidOperationException se)
                {
                    if (string.Equals(se.GetExceptionDetail(), "Operation is not valid due to the current state of the object.", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn("The site collection {0} is not available.", url);
                        throw new FileNotFoundException(string.Format("The site collection {0} is not available", url));
                    }
                    mLog.Warn("rectify url {0} failed with soapexception, Error Message:{1}", url, se.GetExceptionDetail());
                }
                catch (Exception e)
                {
                    mLog.Warn("rectify url {0} failed, Error Message:{1}", url, e.ToString());
                }
                sw2.Stop();
                mLog.Info($"linkRestoreReport TryToRectifySiteUrl CreateSite cost time:{sw2.ElapsedMilliseconds}");
                return url;
            }
        }
    }
}
