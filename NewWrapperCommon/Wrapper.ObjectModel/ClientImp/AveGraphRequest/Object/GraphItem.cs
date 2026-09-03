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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Office365.Api;
using Newtonsoft.Json.Linq;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    class GraphItem : GraphBase
    {
        private string webId;
        private Guid listId;
        private int itemId;
        public GraphItem(ITokenProvider tokenProvider, IWebProxy proxy, string webId, Guid listId, int itemId)
            : base(tokenProvider, proxy)
        {
            this.webId = webId;
            this.listId = listId;
            this.itemId = itemId;
        }
        /// <summary>
        /// 数量比通过API获取的要少
        /// </summary>
        /// <returns></returns>
        public JObject GetFieldValues()
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items/{3}/fields", GraphApiUrl.V1, webId, listId, itemId);
            return GetObjectInfo(requestUri);
        }
        public JObject UpdateItemFieldValues(string fieldValueJson)
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items/{3}/fields", GraphApiUrl.V1, webId, listId, itemId);
            var parameter = GenerateRequestsParameters(requestUri);
            parameter.Content = new StringContentRequest(fieldValueJson, "application/json");
            return request.PatchJsonAsync<JObject>(parameter).Result;
        }


        public JObject GetVersions()
        {
            string requestUri = string.Format("{0}/sites/{1}/lists/{2}/items/{3}/versions", GraphApiUrl.V1, webId, listId, itemId);
            return GetObjectInfo(requestUri);
        }
        public JObject GetSpecifiedFieldValues(string[] fields)
        {
            string requestUri = string.Format("{0}/sites/{1},{2},{3}/lists/{4}/items/{5}", GraphApiUrl.V1, webId, listId, itemId);
            requestUri = string.Format("{0}{1}", requestUri, CreateQueryParamFormQueryListItemField(fields));
            return GetObjectInfo(requestUri);
        }

        private string CreateQueryParamFormQueryListItemField(string[] fields)
        {
            string queryParam = "?expand=fields(select=";
            fields.ToList().ForEach(item => { queryParam = queryParam + item + ","; });

            return queryParam.TrimEnd(',') + ")";
        }
    }
}
