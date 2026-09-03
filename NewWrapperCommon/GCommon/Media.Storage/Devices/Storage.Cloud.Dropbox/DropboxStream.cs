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

namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/9/28",
    "yanxin.fu@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion

    class DropboxStream : XStream
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(DropboxStream));
        private StorageInfo info;
        private MemoryStream ms;
        private DropboxSystem system;
        private Stream innerStream;
        private Int64 uploadOffset = 0;
        private String session_id = String.Empty;
        private Byte[] partContent = new Byte[DropboxConstants.ChunkSize];
        private Boolean isLargeFile = false;
        private Boolean isFirstUpload = false;
        private HttpWebRequest uploadRequest;

        public DropboxStream(DropboxSystem system, StorageInfo info, FileMode fileMode)
            : base(null)
        {
            this.info = info;
            this.system = system;
            this.URI.SdType = 407;
            this.URI.SysId = this.system.SystemID;
            this.URI.SInfo = info.Clone();
            if (fileMode == FileMode.Open)
            {
                InitReadStream();
            }
            else
            {
                InitWriteStream();
            }
        }

        private void InitWriteStream()
        {
            if (this.info.Length > DropboxConstants.UploadLimitSize)
            {
                this.isLargeFile = true;
            }
            else
            {
                var url = StorageUrl.DropboxNormalUpload;
                var path = Path.Combine(this.system.SystemLocation, info.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
                path = path.StartsWith("/") ? path : "/" + path;
                path = path.EndsWith("/") ? path.TrimEnd('/') : path;
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"path\":\"").Append(path).Append("\",\"mode\":\"overwrite\"}");//,\"autorename\":false,\"mute\":false}");
                this.uploadRequest = this.system.CreateRequestWithToken(url, "POST", stringBuilder.ToString());
                this.uploadRequest.Timeout = Int32.MaxValue - 1;
                this.uploadRequest.ContentLength = this.info.Length;
                this.uploadRequest.ContentType = @"application/octet-stream";
                this.innerStream = this.uploadRequest.GetRequestStream();
            }
        }

        private void InitReadStream()
        {
            var url = StorageUrl.DropboxDownload;
            var path = Path.Combine(this.system.SystemLocation, info.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            path = path.StartsWith("/") ? path : "/" + path;
            path = path.EndsWith("/") ? path.TrimEnd('/') : path;
            var param = "{\"path\":\"" + path + "\"}";
            var downloadRequest = this.system.CreateRequestWithToken(url, "POST", param, this.info);
            try
            {
                var response = downloadRequest.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    this.innerStream = response.GetResponseStream();
                    this.info.Length = response.ContentLength;
                }
                else if (response.StatusCode == HttpStatusCode.PartialContent && this.info.Offset > 0)
                {
                    this.innerStream = response.GetResponseStream();
                }
                else
                {
                    throw new WebException("Get file stream failed");
                }
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                var body = String.Empty;
                using (var respStream = resp.GetResponseStream())
                {
                    using (var sr = new StreamReader(respStream))
                    {
                        body = sr.ReadToEnd();
                    }
                }
                logger.Error("Init read stream failed,msg:{0}, response body:{1}:", we, body);
                throw;
            }
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (this.isLargeFile)
            {
                if (this.ms == null)
                {
                    this.isFirstUpload = true;
                    this.ms = new MemoryStream(DropboxConstants.ChunkSize);
                }
                var freeMemoryStream = this.ms.Capacity - (Int32)this.ms.Position;
                while (count >= freeMemoryStream)
                {
                    this.ms.Write(buffer, offset, freeMemoryStream);
                    offset = offset + freeMemoryStream;
                    count = count - freeMemoryStream;
                    PreUploadPartial(this.ms);
                    freeMemoryStream = this.ms.Capacity - (Int32)this.ms.Position;
                }
                if (count > 0)
                    this.ms.Write(buffer, offset, count);
                if (this.ms.Capacity == (Int32)this.ms.Position)
                    PreUploadPartial(this.ms);
            }
            else
            {
                innerStream.Write(buffer, offset, count);
            }
        }

        public void PreUploadPartial(MemoryStream ms)
        {
            String finalUrl;
            String param;
            if (isFirstUpload)
            {
                finalUrl = StorageUrl.DropboxUploadSessionStart;
                param = "{\"close\": false}";
            }
            else
            {
                finalUrl = StorageUrl.DropboxUploadSessionAppend_v2;
                param = "{\"cursor\": {\"session_id\":\"" + session_id + "\",\"offset\":" + this.uploadOffset + "},\"close\": false }";
            }
            if (this.ms.Position > 0)
            {
                this.ms.Position = 0;
                this.session_id = UploadPart(this.ms, param, this.ms.Length, finalUrl);
                this.ms.SetLength(0);
                this.ms.Position = 0;
            }
        }

        public String UploadPart(MemoryStream ms, String param, Int64 len, String url)
        {
            var request = this.system.CreateRequestWithToken(url, "POST", param);
            request.ContentType = "application/octet-stream";
            request.ContentLength = len;
            this.uploadOffset += len;
            try
            {
                using (var upStream = request.GetRequestStream())
                {
                    var tempLen = 0;
                    var buffer = new Byte[64 * 1024];
                    while ((tempLen = this.ms.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        upStream.Write(buffer, 0, tempLen);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                request.Abort();
                throw;
            }
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        if (isFirstUpload)
                        {
                            var responseBody = stream.ReadToEnd();
                            var regexId = new Regex(DropboxConstants.SessionId);
                            var mc = regexId.Matches(responseBody);
                            foreach (var m in mc)
                            {
                                var temp = m.ToString().Split(new String[] { "\": \"", "\"}" }, StringSplitOptions.RemoveEmptyEntries);
                                session_id = temp[1];
                            }
                            isFirstUpload = false;
                        }
                        return session_id;
                    }
                }
                else
                {
                    throw new Exception("Multi-Upload Failed,object:" + url);
                }
            }
        }

        public override StorageResult Commit(Boolean closeParent)
        {
            var rs = new StorageResult();
            rs.PdId = System.SystemID;
            var response = default(HttpWebResponse);
            try
            {
                if (this.isLargeFile)
                {
                    if (!this.IsCommited)
                    {
                        var url = StorageUrl.DropboxUploadSessionFinish;
                        var path = Path.Combine(this.system.SystemLocation, info.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
                        path = path.StartsWith("/") ? path : "/" + path;
                        path = path.EndsWith("/") ? path.TrimEnd('/') : path;
                        var param = "{\"cursor\": {\"session_id\":\"" + session_id + "\",\"offset\":" + this.uploadOffset + "},\"commit\": {\"path\": \"" + path + "\",\"mode\": \"overwrite\"}}";
                        //用于清理该流在Write后部分残留在memoryStream的数据，并且关闭该流，并关闭session
                        using (this.ms)
                        {
                            this.ms.Position = 0;
                            UploadPart(this.ms, param, this.ms.Length, url);
                        }
                    }
                }
                else
                {
                    using (response = (HttpWebResponse)this.uploadRequest.GetResponse())
                    {
                        if (response == null || response.StatusCode != HttpStatusCode.OK)
                            throw new Exception("Create object failed. object : " + this.uploadRequest.RequestUri);
                    }
                }
                this.IsCommited = true;
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                using (var respStream = resp.GetResponseStream())
                {
                    using (var sr = new StreamReader(respStream))
                    {
                        var body = sr.ReadToEnd();
                        logger.Error("Execute request failed, msg:{0}, response body:{1}:", we, body);
                    }
                }
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    logger.Info("This exception is a connection fail exception: {0}", we.Message);
                    throw new RetryableException(we.Message, we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (var errorResponse = we.Response as HttpWebResponse)
                    {
                        var code = errorResponse.StatusCode;
                        if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
                            throw new RetryableException(we.Message, we);
                    }
                }
                logger.Error("Commit file failed : {0}", we);
                throw;
            }
            return rs;
        }

        public override int Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            var readLen = 0;
            try
            {
                readLen = this.innerStream.Read(buffer, offset, count);
                this.ReadLength += readLen;
            }
            catch (Exception e)
            {
                logger.Error("Read file {0} failed. Error: {1}", this.info.HighPlusLowName, e);
                throw;
            }
            return readLen;
        }

        public override void Flush() { }

        public override XURIResult GetURI()
        {
            return this.URI;
        }

        public override Boolean CanRead
        {
            get
            {
                return true;
            }
        }

        public override Boolean CanSeek
        {
            get
            {
                return false;
            }
        }

        public override Boolean CanWrite
        {
            get
            {
                return true;
            }
        }

        public override Int64 Length
        {
            get
            {
                return this.info.Length;
            }
        }

        public override Int64 Position
        {
            get
            {
                return this.ReadLength;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Close()
        {
            if (this.innerStream != null)
            {
                this.innerStream.Close();
            }
        }
    }
}
