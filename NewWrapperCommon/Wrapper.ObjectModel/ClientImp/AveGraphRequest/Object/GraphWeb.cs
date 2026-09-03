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
using AvePoint.Office365.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    public class GraphWeb : GraphBase
    {
        private Uri webUri;
        private string webId;
        public GraphWeb(ITokenProvider tokenProvider, IWebProxy proxy, string webUrl)
            : base(tokenProvider, proxy)
        {
            webUri = new Uri(webUrl);
        }

        public string WebId
        {
            get
            {
                if (string.IsNullOrEmpty(webId))
                {
                    webId = GetWebId();
                }
                return webId;
            }
        }

        private string GetWebId()
        {
            dynamic webInfo = GetWebInfo();
            return webInfo.id;
        }

        public JObject GetWebInfo()
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath);
            return GetObjectInfo(requestUri);
        }

        public JObject GetSubWebs()
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}:/sites", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        ///  暂时还想不出什么情况下需要使用Site Id获取SiteInfo
        /// </summary>
        /// <param name="siteCollectionId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private JObject GetSubWebsById(Guid siteCollectionId, Guid siteId)
        {
            string requestUri = string.Format("{0}/sites/{1},{2},{3}/sites", GraphApiUrl.V1, webUri.Host, siteCollectionId, siteId);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        /// 测试发现获取list数量不足 用API测试获取到了26个list, 使用该API只获取到了11个
        /// </summary>
        /// <returns></returns>
        public JObject GetLists()
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}:/lists", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath);
            return GetObjectInfo(requestUri);
        }


        public JObject GetListById(Guid listId)
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}:/lists/{3}", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath, listId);
            return GetObjectInfo(requestUri);
        }

        public void DeleteListById(Guid listId)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}", GraphApiUrl.V1, WebId, listId);
            DeleteObject(requestUri);
        }

        /// <summary>
        /// 当前没有添加field 参数 ，后期考虑添加
        /// </summary>
        /// <param name="title"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public JObject CreateList(string title, string template)
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}:/lists", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath);
            var parameter = GenerateStringContentRequestParameters(requestUri, JsonConvert.SerializeObject(new { displayName = title, columns = new object[] { }, list = new { template = template } }), "application/json");

            return request.PostAsync<JObject>(parameter).Result;
        }

        /// <summary>
        /// Drive 对应Document Library
        /// </summary>
        /// <returns></returns>
        public JObject GetDrives()
        {
            string requestUri = string.Format("{0}/sites/{1}:{2}:/Drives", GraphApiUrl.V1, webUri.Host, webUri.AbsolutePath);
            return GetObjectInfo(requestUri);
        }


        public JObject GetDriveById(string driveId)
        {
            string requestUri = string.Format("{0}/sites/{1}/Drives/{2}", GraphApiUrl.V1, WebId, driveId);
            return GetObjectInfo(requestUri);
        }
    }
}
