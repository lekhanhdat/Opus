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
    using System.Net;
    using AvePoint.Office365.Api;
    using Newtonsoft.Json.Linq;

    public class GraphList : GraphBase
    {
        private string webId;
        private Guid listId;
        public GraphList(ITokenProvider tokenProvider, IWebProxy proxy, string webId, Guid listId)
            : base(tokenProvider, proxy)
        {
            this.webId = webId;
            this.listId = listId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public void DeleteItem(int itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items/{3}", GraphApiUrl.V1, webId, listId, itemId);
            DeleteObject(requestUri);
        }

        public JObject GetItem(int itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items/{3}", GraphApiUrl.V1, webId, listId, itemId);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        /// 测试发现该方法获取不到list下 folder的信息
        /// </summary>
        /// <returns></returns>
        public JObject GetItems()
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items", GraphApiUrl.V1, webId, listId);
            return GetObjectInfo(requestUri);
        }

        public JObject GetContentTypes()
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/contenttypes", GraphApiUrl.V1, webId, listId);
            return GetObjectInfo(requestUri);
        }

        public JObject GetContentTypeById(string contentTypeId)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/contenttypes/{3}", GraphApiUrl.V1, webId, listId, contentTypeId);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        /// 暂时没有找到如何在folder下创建 list item的方法
        /// </summary>
        /// <param name="listItemInfo"></param>
        /// <returns></returns>
        public JObject CreateListItem(string listItemInfo)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items", GraphApiUrl.V1, webId, listId);
            var parameter =GenerateStringContentRequestParameters(requestUri, listItemInfo, "application/json");
            return request.PostAsync<JObject>(parameter).Result;
        }

        public JObject GetFields()
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/columns", GraphApiUrl.V1, webId, listId);
            return GetObjectInfo(requestUri);
        }

        public JObject GetFieldById(string fieldId)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/columns/{3}", GraphApiUrl.V1, webId, listId, fieldId);
            return GetObjectInfo(requestUri);
        }

    }
}
