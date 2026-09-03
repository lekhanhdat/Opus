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
using Newtonsoft.Json;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    class GraphFolder : GraphBase
    {
        private string webId;
        private string folderId;
        private string driveId;
        public GraphFolder(ITokenProvider tokenProvider, IWebProxy proxy, string webId, string driveId, string folderId)
            : base(tokenProvider, proxy)
        {
            this.webId = webId;
            this.driveId = driveId;
            this.folderId = folderId;
        }

        public JObject CreateSubFolder(string folderName)
        {
            string webApiUrl = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/children", GraphApiUrl.V1, webId, driveId, folderId);

            var parameter = GenerateRequestsParameters(webApiUrl);
            parameter.Content = new StringContentRequest(JsonConvert.SerializeObject(new { name = folderName, folder = new { } }), "application/json");

            return request.PostAsync<JObject>(parameter).Result;
        }


    }
}
