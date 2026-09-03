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
using AvePoint.Media.ClassicStorage.Cloud.Azure.REST;
using AvePoint.Media.ClassicStorage.Cloud.Common;
using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
using AvePoint.Media.ClassicStorage.Security;
using AvePoint.Media.ClassicStorage.Util;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace AvePoint.Media.ClassicStorage.Cloud.Azure
{
    class AzureUploadStream : HttpUploadStream
    {
        private long length;
        public override long Length { get { return length; } }
        //private MD5 md5 = MD5.Create();
        private HashAlgorithm hashAlgorithm;
        public AzureUploadStream(HttpWebRequest request) : base(request)
        {
            hashAlgorithm = new AveCrc64();
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

                hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
            }
            catch (Exception e)
            {
                SetEventTaskInfo(System);
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.HttpWebRequest.RequestUri.AbsoluteUri, EventTaskMessage, e);
                //this.Logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, writeFailedEventMessage);
                Logger.Error("Write file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                throw new RetryableException(e.Message, e);
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
                    rs.PdId = System.SystemID;
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                        InnerStream = null;
                    }

                    hashAlgorithm.TransformFinalBlock(new byte[1], 0, 0);
                    //string calContentMD5 = Convert.ToBase64String(hashAlgorithm.Hash);
                    //string returnMD5 = "";
                    string calContentHash = Convert.ToBase64String(hashAlgorithm.Hash);
                    string returnHash = "";             
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
                        returnHash = resp.Headers[MSAzureConstants.ContentCRC64];
                    }
                    if (!calContentHash.Equals(returnHash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException($"{hashAlgorithm.GetType().Name} not match {calContentHash}, {returnHash}");
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
                        }
                    }
                    SetEventTaskInfo(System);
                    this.Logger.Warn(we.ToString());
                    throw;
                }
            }
            return null;
        }

        public override void Close()
        {
            base.Close();
            if (hashAlgorithm != null)
            {
                hashAlgorithm.Dispose();
                hashAlgorithm = null;
            }
        }
    }
}
