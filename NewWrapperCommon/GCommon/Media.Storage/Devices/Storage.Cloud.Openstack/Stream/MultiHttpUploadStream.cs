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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    //TODO 分段retry
    class MultiHttpUploadStream : OpenStackStream
    {
        private StorageLogger Logger = StorageLogger.GetInstance(typeof(MultiHttpUploadStream));

        private OpenStackBaseRestClient restClient;
        private OpenStackOpenParameter openParameter;
        private HttpWebRequest webRequest ;
        private string containerName;
        private string objectName;
        private Dictionary<string, string> headerParameters;
        private Dictionary<string, string> urlParameters;
        private long segmentMinSize;
        private bool checkMD5 = true;
        private MD5 currentMD5;
        private MD5 totalMD5;
        private int currentSegmentNumber = 1;
        private long totalLength;
        private long currentLength;
        private long needWriteLength;
        private string currentSegmentName;
        private List<SegmentInfo> segmentsList = new List<SegmentInfo>();

        private long totalWriteLength;
        private long totalWriteTime;
        private long totalCommitTime;

        public MultiHttpUploadStream(OpenStackBaseRestClient restClient, StorageInfo info, OpenStackOpenParameter openParameter, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null)
        {
            this.Info = info;
            this.restClient = restClient;
            this.openParameter = openParameter;
            this.containerName = info.HighName;
            this.objectName = info.LowName;
            this.needWriteLength = info.Length;
            segmentMinSize = openParameter.SegmentMinSize;
            checkMD5 = openParameter.UploadCheckMD5;

            if (headerParameters != null)
            {
                this.headerParameters = headerParameters;
            }
            else
            {
                this.headerParameters = new Dictionary<string, string>();
            }
            if (urlParameters != null)
            {
                this.urlParameters = urlParameters;
            }
            else
            {
                this.urlParameters = new Dictionary<string, string>();
            }

            totalMD5 = new MD5CryptoServiceProvider();
            currentMD5 = new MD5CryptoServiceProvider();

            InitRequest();
        }

        private void InitRequest()
        {
            currentSegmentName = objectName + "/" + currentSegmentNumber;
            currentSegmentNumber++;
            webRequest = restClient.UploadObjectRequest(containerName, currentSegmentName, headerParameters, urlParameters);
            webRequest.AllowWriteStreamBuffering = false;
            webRequest.AllowAutoRedirect = false;
            webRequest.Timeout = 2 * 3600 * 1000; //1小时超时
            webRequest.SendChunked = true;
            Stream reqStream = webRequest.GetRequestStream();
            InnerStream = new BufferedStream(reqStream, 64 * 1024);
            currentMD5.Initialize();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                long startTicks = DateTime.UtcNow.Ticks;
                InnerStream.Write(buffer, offset, count);
                totalWriteLength = totalWriteLength + count;
                totalWriteTime = totalWriteTime + DateTime.UtcNow.Ticks - startTicks;

                totalLength += count - offset;
                currentLength += count - offset;
                if (checkMD5)
                {
                    currentMD5.TransformBlock(buffer, offset, count, null, 0);
                    totalMD5.TransformBlock(buffer, offset, count, null, 0);
                }
                if (currentLength > segmentMinSize)
                {
                    CommitSegment();
                    InitRequest();
                    currentLength = 0;
                }
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

        public void CommitSegment()
        {
            try
            {
                StorageResult rs = new StorageResult();

                long startTicks = DateTime.UtcNow.Ticks;
                if (InnerStream != null)
                {
                    InnerStream.Close();
                    InnerStream = null;
                }

                using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        string eTag = response.Headers["ETag"];
                        currentMD5.TransformFinalBlock(new byte[1], 0, 0);
                        byte[] resultHash = currentMD5.Hash;
                        StringBuilder sBuilder = new StringBuilder();
                        for (int i = 0; i < resultHash.Length; i++)
                        {
                            sBuilder.Append(resultHash[i].ToString("x2"));
                        }

                        SegmentInfo segmentInfo = new SegmentInfo();
                        segmentInfo.path = containerName + "/" + currentSegmentName;
                        segmentInfo.size_bytes = currentLength;
                        segmentInfo.etag = eTag;
                        segmentsList.Add(segmentInfo);
                    }
                    else
                    {
                        throw new Exception("Create object failed. object : " + webRequest.RequestUri);
                    }
                }
                totalCommitTime = totalCommitTime + DateTime.UtcNow.Ticks - startTicks;

            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    Logger.Info("this exception is a connection fail exception:" + we.Message);
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
                SetEventTaskInfo(System);
                throw;
            }
        }

        public override StorageResult Commit()
        {
            if (!IsCommited)
            {
                try
                {
                    IsCommited = true;
                    StorageResult rs = new StorageResult();
                    if (InnerStream != null)
                    {
                        CommitSegment();
                    }
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string result = js.Serialize(segmentsList.ToArray());
                    byte[] buffer = Encoding.UTF8.GetBytes(result);

                    urlParameters = new Dictionary<string, string>();
                    urlParameters.Add("multipart-manifest", "put");
                    webRequest = restClient.UploadObjectRequest(containerName, objectName, headerParameters, urlParameters);

                    webRequest.GetRequestStream().Write(buffer, 0, buffer.Length);

                    long startTicks = DateTime.UtcNow.Ticks;
                    using (HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response != null && (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK))
                        {

                        }
                        else
                        {
                            throw new Exception("Create object failed. object : " + webRequest.RequestUri);
                        }
                    }
                    totalCommitTime = totalCommitTime + DateTime.UtcNow.Ticks - startTicks;
                    return rs;
                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                    {
                        Logger.Info("this exception is a connection fail exception:" + we.Message);
                        throw new RetryableException(we.Message, we);
                    }
                    else if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (HttpWebResponse response = we.Response as HttpWebResponse)
                        {
                            HttpStatusCode code = response.StatusCode;
                            if (code == HttpStatusCode.Unauthorized || (int)code == 420)
                            {
                                restClient.Authentication();
                                throw new RetryableException(we.Message, we);
                            }
                            if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
                            {
                                throw new RetryableException(we.Message, we);
                            }
                        }
                    }
                    SetEventTaskInfo(System);
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
            Logger.Debug("multi upload close, length " + totalWriteLength + " , commit time " + totalCommitTime + " , write time " + totalWriteTime);
        }

        public override void Abort()
        {
            if (webRequest != null)
            {
                webRequest.Abort();
            }
        }
    }

    class SegmentInfo
    {
        public string path { get; set; }
        public string etag { get; set; }
        public long size_bytes { get; set; }
    }
}
