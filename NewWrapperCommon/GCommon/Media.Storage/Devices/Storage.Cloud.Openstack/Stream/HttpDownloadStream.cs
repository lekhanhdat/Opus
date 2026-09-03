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
using System.Globalization;
using System.IO;
using System.Net;
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class HttpDownloadStream : OpenStackStream
    {
        private static StorageLogger Logger = StorageLogger.GetInstance(typeof(HttpDownloadStream));

        private OpenStackBaseRestClient restClient;
        private HttpWebResponse webResponse;
        private String containerName;
        private String objectName;
        private Dictionary<String, String> headerParameters;
        private Dictionary<String, String> urlParameters;
        private Int64 contentResponseLength;
        private Int64 readOffset;
        private Int64 readRequestLength;
        private Int64 totalReadedLength;

        public HttpDownloadStream(OpenStackBaseRestClient restClient, StorageInfo info, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            this.Info = info;
            this.restClient = restClient;
            this.containerName = info.HighName;
            this.objectName = info.LowName;
            this.readOffset = info.Offset;
            this.readRequestLength = info.Length;
            this.headerParameters = headerParameters ?? new Dictionary<String, String>();
            this.urlParameters = urlParameters ?? new Dictionary<String, String>();
            InitRequest();
        }

        private void InitRequest()
        {
            if (this.readOffset > 0 || this.readRequestLength > 0)
            {
                String range;
                if (readRequestLength > 0)
                    range = "bytes=" + readOffset + "-" + (readRequestLength + readOffset - 1);
                else
                    range = "bytes=" + readOffset + "-";
                headerParameters["Range"] = range;
            }
            webResponse = restClient.DownloadObjectResponse(containerName, objectName, headerParameters, urlParameters); // TODO webResponse可以在用完就释放了
            if (HttpStatusCode.OK == webResponse.StatusCode || HttpStatusCode.PartialContent == webResponse.StatusCode)
            {
                var respStream = webResponse.GetResponseStream();
                InnerStream = new BufferedStream(respStream, 64 * 1024);
            }
            else
            {
                throw new Exception(string.Format("Open Http Down Stream Error, Error Code : {0}", webResponse.StatusCode));
            }

            this.contentResponseLength = webResponse.ContentLength;
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            var readLen = default(Int32);
            try
            {
                while (readLen < count)
                {
                    Int32 tempLen = InnerStream.Read(buffer, offset + readLen, count - readLen);
                    readLen += tempLen;
                    totalReadedLength += tempLen;
                    if (readLen >= count || (tempLen <= 0 && totalReadedLength >= Info.Length))
                    {
                        Info.CurrentRetryCount = 0;
                        break;
                    }
                    if (tempLen <= 0 && totalReadedLength < Info.Length)
                    {
                        throw new Exception("The result of InnerStream.read is 0, either server read exception or info.Length is inexactly, info.length: " + Info.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Error Occurred during reading content (" + "R" + "ETRING".ToLower(CultureInfo.InvariantCulture) + " : " + Info.CurrentRetryCount + " ):" + e.Message, e);
                this.Close();
                if (Info.CurrentRetryCount < MaxRetryCount)
                {
                    this.readOffset = this.totalReadedLength + this.readOffset;
                    this.readRequestLength = this.readRequestLength - this.totalReadedLength;
                    InitRequest();
                    return Read(buffer, offset + readLen, count - readLen);
                }
                else
                {
                    SetEventTaskInfo(System);
                    //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(this.Response.ResponseUri.AbsolutePath, EventTaskMessage, e);
                    //this.Logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, readFailedEventMessage);
                    //Logger.Error("Read file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                    throw;
                }
            }
            return readLen;
        }

        public override void ClosedUnmoral()
        {
            if (webResponse != null)
            {
                webResponse.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
        }

        public override void Close()
        {
            if (webResponse != null)
            {
                webResponse.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
        }

        public override long Length
        {
            get
            {
                return this.contentResponseLength;
            }
        }

        public override bool CanRead
        {
            get
            {
                return InnerStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return InnerStream.CanWrite;
            }
        }
    }
}
