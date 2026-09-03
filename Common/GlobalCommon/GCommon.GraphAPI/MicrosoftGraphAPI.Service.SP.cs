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

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.GraphAPI
{
    public partial class MicrosoftGraphAPIService
    {
        public Stream GetFileContent(string siteId, string listId, string itemId,string tempFilePath)
        {
            return new GetFileContent(this.resourceUrl, this.refreshAccessToken, siteId, listId, itemId, tempFilePath, this.RetryController).GetApiResult();
        }

        public Stream GetFileVersionContent(string siteId, string listId, string itemId, string versionId, string tempFilePath)
        {
            return new GetFileVersionContent(this.resourceUrl, this.refreshAccessToken, siteId, listId, itemId, versionId, tempFilePath, this.RetryController).GetApiResult();
        }

    }

    public class GetFileContent : GetRequest<Stream>
    {
        private string siteId;
        private string listId;
        private string itemId;
        private string tempFilePath;

        public GetFileContent(string baseUrl, Func<string> getToken, IRetryable retryable,string tempFilePath) : base(baseUrl, getToken, retryable)
        {
            this.tempFilePath = tempFilePath;
        }

        public GetFileContent(string baseUrl, Func<string> getToken, string siteId, string listId, string itemId, string tempFilePath, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.siteId = siteId;
            this.listId = listId;
            this.itemId = itemId;
            this.tempFilePath = tempFilePath;
        }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{siteId}/lists/{listId}/items/{itemId}/driveItem/content";
            }
        }

        public override Stream GetApiResult()
        {
            Stream content = System.IO.File.OpenWrite(tempFilePath);
            var returnBlock = new byte[BlockSize];
            var i = 0;
            var requestHeaders = new Dictionary<string, string>();
            requestHeaders.Add("Range", "");
            do
            {
                try
                {
                    var rangeValue = $"bytes={i * BlockSize}-{BlockSize * (i + 1) - 1}";
                    requestHeaders["Range"] = rangeValue;
                    this.RequestHeader = requestHeaders;
                    this.httpMethod = HttpMethod.Get;
                    returnBlock = this.ExecuteV1(null, this.FullUrl);
                    if (returnBlock != null && returnBlock.Length > 0)
                    {
                        content.Write(returnBlock, 0, returnBlock.Length);
                    }
                    i++;
                    // for test 
                    //if (i % 10 == 0)
                    //{
                    //    Console.WriteLine($"Requests has executed {i} times, current download size:{content.Position}");
                    //}
                }
                catch (Exception ex)
                {
                    if (content != null)
                    {
                        content.Dispose();
                    }
                    throw;
                    //for test -need refresh token  
                    //Console.WriteLine($"need refresh token,current:{this.apptoken}");
                }
            }
            while (returnBlock?.Length == BlockSize);
            return content;
        }
    }

    public class GetFileVersionContent : GetFileContent
    {
        private string siteId;
        private string listId;
        private string itemId;
        private string versionId;
        private string tempFilePath;
        public GetFileVersionContent(string baseUrl, Func<string> getToken, string siteId, string listId, string itemId, string versionId, string tempFilePath, IRetryable retryable) : base(baseUrl, getToken, retryable, tempFilePath)
        {
            this.siteId = siteId;
            this.listId = listId;
            this.itemId = itemId;
            this.versionId = versionId;
            this.tempFilePath = tempFilePath;
        }

        protected override string RequestUrl
        {
            get
            {
                return $"{this.apiUrlV1}/sites/{siteId}/lists/{listId}/items/{itemId}/driveItem/versions/{versionId}/content";
            }
        }

    }
}