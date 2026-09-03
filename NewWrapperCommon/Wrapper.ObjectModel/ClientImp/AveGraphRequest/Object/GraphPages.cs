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
using System.Net;
using AvePoint.Office365.Api;
using Newtonsoft.Json.Linq;

namespace AvePoint.ObjectModel.AveGraphRequest
{

    /// <summary>
    /// Site Pages list
    /// </summary>
    public class GraphPages : GraphBase
    {
        private string webId;

        public GraphPages(ITokenProvider tokenProvider, IWebProxy proxy, string webId)
            : base(tokenProvider, proxy)
        {
            this.webId = webId;
        }

        public JObject GetPageById(string pageId)
        {
            string requestUri = string.Format("{0}/sites/{1}/pages/{2}", GraphApiUrl.BETA, webId, pageId);
            return GetObjectInfo(requestUri);
        }


        public void DeletePagesList(string pageId)
        {
            string requestUri = string.Format("{0}/sites/{1}/pages/{2}", GraphApiUrl.BETA, webId, pageId);
            DeleteObject(requestUri);
        }

        public JObject GetSitePagesLists()
        {
            string requestUri = string.Format("{0}/sites/{1}/pages", GraphApiUrl.BETA, webId);
            return GetObjectInfo(requestUri);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public JObject CreatePage(string content)
        {
            string requestUri = string.Format("{0}/sites/{1}/pages", GraphApiUrl.BETA, webId);
            var parameter = GenerateStringContentRequestParameters(requestUri, content, "application/json");
            return request.PostAsync<JObject>(parameter).Result;
        }


        public void PublishPage(string pageId)
        {
            string requestUri = string.Format("{0}/sites/{1}/pages/{2}/publish", GraphApiUrl.BETA, webId, pageId);
            var parameter = GenerateRequestsParameters(requestUri);
            request.PostRequest(parameter);

        }
    }
}
