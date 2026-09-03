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
using AvePoint.Media.Storage.Cloud.Common;

namespace AvePoint.Media.Storage.Cloud.Azure
{
    class DBContext
    {
        private Dictionary<string, string> headers;
        private AzureClient azureclient;
        private AbstractStreamOption streamOption;
        private AbstractHttpClient HttpClient;

        public DBContext(AzureClient azureclient, Dictionary<string, string> headers, AbstractHttpClient HttpClient, string fullURL)
        {
            streamOption = StreamOptionFactory(azureclient, headers, HttpClient, fullURL, long.Parse(headers["Content-Length"]));
        }

        public HttpWebRequest CreateRequestPut(string fullURL, Dictionary<string, string> queryParams)
        {
            return streamOption.CreateRequestPut(fullURL, queryParams);
        }

        public void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            streamOption.CombiningRequestWithHeaders(request , headers);
        }

        public HttpUploadStream GetHttpUploadStream(HttpWebRequest request)
        {
            return streamOption.GetHttpUploadStream(request);
        }


        private AbstractStreamOption StreamOptionFactory(AzureClient azureclient, Dictionary<string, string> headers, AbstractHttpClient HttpClient, string fullURL , long contentLength)
        {
            AbstractStreamOption Option = null;
            if (contentLength >= azureclient.OpenParams.BlockLength * 1024 * 1024)
            {
                if (azureclient.OpenParams.UseBlockBlob)
                {
                    Option = new BlockBlobOption(HttpClient, azureclient, headers, fullURL);
                }
                else
                {
                    Option = new BigDBOption(HttpClient, azureclient, headers, fullURL);
                }
            }
            else
            {
                Option = new SmallDBOption(HttpClient, azureclient, headers, fullURL);
            }
            return Option;
        }
        

    }


    
}
