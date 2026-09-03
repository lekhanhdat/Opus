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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;

    #endregion
    class HttpUploadStream : CloudStream
    {
        private StorageLogger logger = StorageLogger.GetInstance(typeof(HttpUploadStream));
        public StorageLogger Logger { get { return logger; } }
        public AbstractHttpClient HttpClient { set; get; }
        private long length;
        public override long Length { get { return length; } }
        public HttpUploadStream()
        {
        }

        public HttpUploadStream(HttpWebRequest request)
        {
            if (request == null)
            {
                return;
            }
            try
            {
                this.HttpWebRequest = request;
                if (request.Proxy.Credentials != null)
                {
                    request.PreAuthenticate = true;
                }
                request.AllowWriteStreamBuffering = false;
                request.AllowAutoRedirect = false;
                request.Timeout = 2 * 60 * 60 * 1000;
                Stream reqStream = request.GetRequestStream();
                InnerStream = new BufferedStream(reqStream, 64 * 1024);
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    Logger.Info("this exception is a connection fail exception:" + we.Message);
                    throw new RetryableException(we.Message, we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            throw new RetryableException(we.Message, we);
                        }
                    }
                }
                else
                {
                    Logger.Error("HttpUploadStream error: " + we.Message);
                    if (HttpWebRequest != null)
                    {
                        HttpWebRequest.Abort();
                    }
                    throw;
                }
            }
            catch (Exception t)
            {
                Logger.Error("New HttpUploadStream failed ", t.Message, t);
                throw;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                long startTicks = DateTime.UtcNow.Ticks;
                InnerStream.Write(buffer, offset, count);
                length += count - offset;
                System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                SetEventTaskInfo(System);
                Logger.Error("Write file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                throw new RetryableException(e.Message, e);
            }
        }

        public override StorageResult Commit(bool closeParent)
        {
            return Commit();
        }

        public override StorageResult Commit()
        {
            if (!IsCommited)
            {
                try
                {
                    IsCommited = true;
                    StorageResult rs = new StorageResult();
                    rs.PdId = System.SystemID;
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                        InnerStream = null;
                    }
                    using (HttpWebResponse resp = HttpWebRequest.GetResponse() as HttpWebResponse)
                    {
                        if (resp == null || (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK))
                        {
                            throw new Exception("Create object failed. object : " + HttpWebRequest.RequestUri);
                        }
                        if (Info != null)
                        {
                            System.AddMetadata(Info);
                        }
                        if (HttpClient != null)
                        {
                            HttpClient.CalcDataFlow(HttpWebRequest, resp);
                        }
                    }
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
                            if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
                            {
                                throw new RetryableException(we.Message, we);
                            }
                            else if ((int)code == 507)//处理dropbox空间满了的情况
                            {
                                throw new NotEnoughFreeSpaceException("The device space is full, message : " + we.Message, we);
                            }
                        }
                    }
                    SetEventTaskInfo(System);
                    EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.HttpWebRequest.RequestUri.AbsoluteUri, EventTaskMessage, we);
                    this.Logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, writeFailedEventMessage);
                    throw;
                }
            }
            return null;
        }

        public override void ClosedUnmoral()
        {

            if (HttpWebRequest != null)
            {
                if (InnerStream != null)
                {
                    InnerStream.Close();
                    InnerStream = null;
                }
                HttpWebRequest.Abort();
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
        }

        public override void Abort()
        {
            if (HttpWebRequest != null)
            {
                HttpWebRequest.Abort();
            }
        }
    }
}
