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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Cloud.Amazon;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    #endregion
    class CleversafeUploadStream : HttpUploadStream
    {
        StorageLogger logger = StorageLogger.GetInstance(typeof(CleversafeUploadStream));
        public AbstractHttpClient HttpClient { set; get; }
        public CleversafeOpenParameter openParameter;
        MemoryStream mStream = null;
        private MySHA256 sha256 = new MySHA256();
        private Int64 length;

        public CleversafeUploadStream(HttpWebRequest request, CleversafeOpenParameter openParameter)
            : base(null)
        {
            this.openParameter = openParameter;
            if (request == null)
            {
                return;
            }
            this.HttpWebRequest = request;
            mStream = new MemoryStream(5 * 1024 * 1024);
        }
        public override void Close()
        {
            if (!IsCommited)
            {
                Commit();
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
                    HttpWebRequest.AllowWriteStreamBuffering = false;
                    HttpWebRequest.AllowAutoRedirect = true;
                    HttpWebRequest.Timeout = 0x7ffffffe; //never timeout
                    StorageResult result = new StorageResult();
                    result.PdId = System.SystemID;

                    CleversafeUtils.AddAuthorization(HttpWebRequest, openParameter.UserName, openParameter.Password);
                    using (Stream reqStream = HttpWebRequest.GetRequestStream())
                    {
                        byte[] tempBuffer = new byte[65536];
                        mStream.Position = 0;
                        while (true)
                        {
                            int len = mStream.Read(tempBuffer, 0, tempBuffer.Length);
                            if (len <= 0)
                                break;
                            reqStream.Write(tempBuffer, 0, len);
                        }
                        mStream.Close();
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
                    return result;
                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                    {
                        Logger.Debug("this exception is a connection fail exception:" + we.Message);
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
                    EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.HttpWebRequest.RequestUri.AbsoluteUri, EventTaskMessage, we);
                    logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, writeFailedEventMessage);
                    throw;
                }
            }
            return null;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                long startTicks = DateTime.UtcNow.Ticks;
                mStream.Write(buffer, offset, count);
                length += count - offset;
                System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalWriteBytes(count);
                sha256.AddByte(buffer, offset, count);
            }
            catch (Exception e)
            {
                SetEventTaskInfo(System);
                Logger.Error("Write file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                throw new RetryableException(e.Message, e);
            }
        }

    }
}
