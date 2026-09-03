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
using System.Text;
using System.Net;
using AvePoint.Media.ClassicStorage.Cloud.Common;
using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
using AvePoint.Media.ClassicStorage.Cloud.Azure.REST;

namespace AvePoint.Media.ClassicStorage.Cloud.Azure.BigDBContext
{
    public abstract class AbstractStreamOption
    {
        public AbstractHttpClient HttpClient { set; get; }
        public AzureClient Azureclient { set; get; }
        public Dictionary<string, string> Headers { set; get; }
        public string FullURL { set; get; }

        public AbstractStreamOption(AbstractHttpClient HttpClient, AzureClient azureclient, Dictionary<string, string> headers, string fullURL)
        {
            this.Azureclient = azureclient;
            this.HttpClient = HttpClient;
            this.Headers = headers;
            this.FullURL = fullURL;
        }

        public virtual HttpWebRequest CreateRequestPut(string fullURL, Dictionary<string, string> queryParams)
        {
            return HttpClient.CreateRequestPut(fullURL, queryParams);
        }

        public virtual void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            HttpClient.CombiningRequestWithHeaders(request, headers);
        }

        public abstract HttpUploadStream GetHttpUploadStream(HttpWebRequest request);

    }
}
