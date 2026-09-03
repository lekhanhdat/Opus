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
using AvePoint.Media.ClassicStorage.Cloud.Azure.REST;
using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
using System.IO;
using AvePoint.Media.ClassicStorage.Util;
using System.Globalization;

namespace AvePoint.Media.ClassicStorage.Cloud.Azure.BigDBContext
{
    public class BigDBOption : AbstractStreamOption, IChangeStream
    {
        private long totalContentLength;
        private long realContenLength;

        public BigDBOption(AbstractHttpClient HttpClient, AzureClient azureclient, Dictionary<string, string> headers, string fullURL)
            : base(HttpClient, azureclient, headers, fullURL)
        {
            realContenLength = long.Parse(headers["Content-Length"].ToString());
            totalContentLength = long.Parse(Byte512(headers["Content-Length"]).ToString());
            Execute();
        }

        public long GetTotalContentLength()
        {
            return totalContentLength;
        }

        public override HttpUploadStream GetHttpUploadStream(HttpWebRequest request)
        {
            return new BigDBHttpUploadStream(ChangeHttpUploadStream(0, 4194303, 1), Azureclient, this);
        }

        private Dictionary<string, string> GetOneceHeaders(Dictionary<string, string> headers)
        {
            Dictionary<string, string> headersTemp = Azureclient.BigDBOpenStreamWriteModeHeaders;
            headers["x-ms-blob-type"] = "PageBlob";
            headersTemp["x-ms-blob-content-length"] = Byte512(headers["Content-Length"]).ToString();
            headers["Content-Length"] = "0";
            foreach (string key in headersTemp.Keys)
            {
                headers.Add(key, headersTemp[key]);
            }
            return headers;
        }

        public HttpWebRequest ChangeHttpUploadStream(long beginLength, long endLength, int rangeType)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            Dictionary<string, string> headers = Azureclient.Headers;
            headers["Range"] = "bytes=" + beginLength.ToString() + "-" + endLength.ToString();
            headers["Content-Length"] = (endLength - beginLength + 1).ToString();
            headers["x-ms-page-write"] = "update";
            queryParams["comp"] = "page";
            headers["Content-Type"] = "DOCAVE/DATA".ToLower(CultureInfo.InvariantCulture);

            HttpWebRequest request = CreateRequestPut(FullURL, queryParams);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            request.Timeout = 0x7ffffffe; //never timeout
            CombiningRequestWithHeaders(request, headers);
            return request;
        }

        private long Byte512(string headContentLength)
        {
            long tempLength = long.Parse(headContentLength);
            if (tempLength % 512 != 0)
                tempLength = (tempLength / 512 + 1) * 512;
            else
                tempLength = (tempLength / 512) * 512;
            return tempLength;
        }

        public long GetRealContentLength()
        {
            return realContenLength;
        }


        private void Execute()
        {
            HttpWebRequest request = CreateRequestPut(FullURL, null);
            CombiningRequestWithHeaders(request, GetOneceHeaders(Headers));

            //HttpWebResponse r = request.GetResponse() as HttpWebResponse;
            //HttpClient.CalcDataFlow(request,r);
            //int code = (int)r.StatusCode;
            //request.Abort();

            try
            {
                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    if (resp == null || (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK))
                    {
                        throw new Exception("Create object failed. object : " + request.RequestUri);
                    }
                    HttpClient.CalcDataFlow(request, resp);
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    throw new RetryableException(we.Message, we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        HttpStatusCode code = response.StatusCode;
                        if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
                        {
                            throw new RetryableException(we.Message, we);
                        }
                    }
                }
                else
                {
                    if (request != null)
                    {
                        request.Abort();
                    }
                    throw;
                }
            }
        }
    }

    public class BlockBlobOption : AbstractStreamOption
    {
        public BlockBlobOption(AbstractHttpClient HttpClient, AzureClient azureclient, Dictionary<string, string> headers, string fullURL)
            : base(HttpClient, azureclient, headers, fullURL)
        {
        }

        public override HttpUploadStream GetHttpUploadStream(HttpWebRequest request)
        {
            return new BlockBlobUploadStream(this);
        }
    }
}
