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
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class HttpUploadStream : OpenStackStream
    {
        private static StorageLogger Logger = StorageLogger.GetInstance(typeof(HttpUploadStream));

        private long length;
        public override long Length { get { return length; } }
        private OpenStackBaseRestClient restClient;
        private HttpWebRequest webRequest;
        private OpenStackOpenParameter openParameter;
        private string containerName;
        private string objectName;
        private Dictionary<string, string> headerParameters;
        private Dictionary<string, string> urlParameters;

        private bool checkMD5 = true;

        private long totalWriteTime;
        private long totalCommitTime;

        public HttpUploadStream(OpenStackBaseRestClient restClient, StorageInfo info, OpenStackOpenParameter openParameter, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null)
        {
            this.Info = info;
            this.restClient = restClient;
            this.openParameter = openParameter;
            this.containerName = info.HighName;
            this.objectName = info.LowName;
            this.length = info.Length;
            checkMD5 = openParameter.UploadCheckMD5;
            this.headerParameters = headerParameters ?? new Dictionary<string, string>();
            this.urlParameters = urlParameters ?? new Dictionary<string, string>();
            InitRequest();
        }

        private void InitRequest()
        {
            webRequest = restClient.UploadObjectRequest(containerName, objectName, headerParameters, urlParameters);
            webRequest.AllowWriteStreamBuffering = false;
            webRequest.AllowAutoRedirect = true;
            webRequest.Timeout = 2 * 3600 * 1000; //2小时超时
            webRequest.ContentLength = Length;
            var reqStream = webRequest.GetRequestStream();
            this.InnerStream = new BufferedStream(reqStream, 64 * 1024);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                var startTicks = DateTime.UtcNow.Ticks;
                InnerStream.Write(buffer, offset, count);
                length += count - offset;
                totalWriteTime = totalWriteTime + DateTime.UtcNow.Ticks - startTicks;
            }
            catch (Exception e)
            {
                SetEventTaskInfo(System);
                Logger.Error("Write file {0} failed, error message: {1},{2}", webRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                throw new RetryableException(e.Message, e);
            }
        }

        public override StorageResult Commit(bool closeParent)
        {
            return Commit();
        }

        public override StorageResult Commit()
        {
            if (!this.IsCommited)
            {
                try
                {
                    IsCommited = true;
                    var rs = new StorageResult {PdId = this.System.SystemID};
                    var startTicks = DateTime.UtcNow.Ticks;
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                        InnerStream = null;
                    }
                    using (var webResponse = webRequest.GetResponse() as HttpWebResponse)
                    {
                        if (webResponse == null || (webResponse.StatusCode != HttpStatusCode.Created && webResponse.StatusCode != HttpStatusCode.OK))
                        {
                            throw new Exception("Create object failed. object : " + webRequest.RequestUri);
                        }
                    }
                    totalCommitTime = totalCommitTime + DateTime.UtcNow.Ticks - startTicks;
                    return rs;
                }
                catch (WebException webException)
                {
                    Abort();
                    if (webException.Status == WebExceptionStatus.ConnectionClosed || webException.Status == WebExceptionStatus.ConnectFailure || webException.Status == WebExceptionStatus.NameResolutionFailure || webException.Status == WebExceptionStatus.Timeout)
                    {
                        Logger.Info("this exception is a connection fail exception:" + webException.Message);
                        throw new RetryableException(webException.Message, webException);
                    }
                    if (webException.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (var response = webException.Response as HttpWebResponse)
                        {
                            var code = response.StatusCode;
                            if (code == HttpStatusCode.Unauthorized || (Int32)code == 420)
                            {
                                this.restClient.Authentication();
                                throw new RetryableException(webException.Message, webException);
                            }
                            if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
                            {
                                throw new RetryableException(webException.Message, webException);
                            }
                        }
                    }
                    SetEventTaskInfo(System);
                    throw;
                }
                catch (Exception e)
                {
                    Abort();
                    throw;
                }
            }
            return null;
        }

        public override void ClosedUnmoral()
        {

            if (webRequest != null)
            {
                if (InnerStream != null)
                {
                    InnerStream.Close();
                    InnerStream = null;
                }
                webRequest.Abort();
            }
        }

        public override XURIResult GetURI()
        {
            if (this.URI == null)
            {
                this.URI = new XURIResult();
            }
            this.URI.SdType = 4;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = Info.Clone();
            return this.URI;
        }

        public override void Close()
        {
            if (!IsCommited)
            {
                Commit();
            }
            Logger.Debug("upload close, length " + length + " , commit time " + totalCommitTime + " , write time " + totalWriteTime);
        }

        public override void Abort()
        {
            if (webRequest != null)
            {
                try
                {
                    webRequest.Abort();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                }
            }
        }
    }
}
