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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Net;
using AvePoint.GCommon;
using System.Web.Services.Protocols;
using System.IO;

namespace AvePoint.ObjectModel.Common
{
    public class AveSiteServiceHelper : IAveSiteServiceHelper
    {
        private AveLogger mLog = AveLogger.GetInstance(typeof(AveSiteServiceHelper));

        public string TryToRectifySiteUrl(string url, AveBPOSAccountInfo accountInfo)
        {
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
            while (true)
            {
                try
                {
                    AveSiteService siteService = CreateSiteService(url, accountInfo);
                    string siteInfo = siteService.GetSite(url);
                }
                catch (WebException we)
                {
                    mLog.Warn("rectify url {0} failed with WebException, Error Message:{1}", url, we.ToString());
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        index = url.LastIndexOf('/');
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound && index > urlHeader.Length)
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
                catch (SoapException se)
                {
                    mLog.Warn("rectify url {0} failed with soapexception, Error Message:{1}", url, se.Detail.InnerText);
                    if (string.Equals(se.Detail.InnerText, "Operation is not valid due to the current state of the object.", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn("The site collection {0} is not available.", url);
                        throw new FileNotFoundException(string.Format("The site collection {0} is not available", url));
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("rectify url {0} failed, Error Message:{1}", url, e.ToString());
                }
                return url;
            }
        }

        private AveSiteService CreateSiteService(string url, AveBPOSAccountInfo accountInfo)
        {
            AveSiteService siteService = new AveSiteService(url + "/_vti_bin/Sites.asmx");
            siteService.Timeout = 3 * 60 * 1000;
            if (!string.IsNullOrEmpty(accountInfo.Domain))
            {
                siteService.Credentials = new NetworkCredential(accountInfo.UserName, accountInfo.Password, accountInfo.Domain);
            }
            else
            {
                siteService.Credentials = new NetworkCredential(accountInfo.UserName, accountInfo.Password);
            }
            return siteService;
        }
    }
}
