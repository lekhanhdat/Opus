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
namespace AvePoint.ObjectModel.AveGraphRequest
{
    using System;
    using Office365.Api;
    using Newtonsoft.Json.Linq;
    using System.Net.Http.Headers;
    using System.Net;

    public class GraphSite:GraphBase
    {
        private Uri siteUri;
        public GraphSite(ITokenProvider tokenProvider,IWebProxy proxy, string siteUrl)
            :base(tokenProvider,proxy)
        {
            this.siteUri = new Uri(siteUrl);
        }

        public JObject GetSiteCollectionInfo()
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}", GraphApiUrl.V1, siteUri.Host, siteUri.AbsolutePath);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        /// 暂时还想不出什么情况下需要使用Site Id获取SiteInfo
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="siteCollectionId"></param>
        /// <returns></returns>
        private JObject GetSiteCollectionInfoById(string hostName,Guid siteCollectionId)
        {
            string requestUri = string.Format("{0}/sites/{1},{2}", GraphApiUrl.V1, hostName, siteCollectionId);
            return GetObjectInfo(requestUri);
        }
    }
}
