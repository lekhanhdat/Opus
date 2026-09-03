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
using System.Linq;
using System.Text;
using System.IO;
using AvePoint.GCommon.Contract.CodeReview;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using AvePoint.GCommon;
using AvePoint.Media.Storage.Util;
using System.Collections.Specialized;

namespace AvePoint.Media.Storage.GoogleDrive
{
    delegate int ReadDelegate(byte[] buffer, int offset, int count);

    class GoogleDriveStream : XStream
    {
        AveLogger logger = AveLogger.GetInstance(typeof(GoogleDriveStream));
        private Stream respStream;
        public ReadDelegate ReadDelegate { get; set; }
        private long readTotalLength;

        FileMode streamMode;
        string objectId = string.Empty;
        public string NextMetaId { get; set; }
        Stream innerStream;
        GoogleDriveSystem googleDriveSystem;
        HttpWebRequest req;

        public GoogleDriveStream(GoogleDriveSystem sys, StorageInfo storageInfo, Stream respStream, ReadDelegate readDelegate)
            : base(sys)
        {
            this.googleDriveSystem = sys;
            this.Info = storageInfo;
            this.respStream = respStream;
            this.ReadDelegate = readDelegate;
        }

        public GoogleDriveStream(StorageInfo info, GoogleDriveSystem sys)
            : base(sys)
        {
            this.Info = info;
            this.URI.SdType = 410;
            this.URI.SysId = sys.SystemID;
            this.URI.SInfo = info.Clone();
            this.streamMode = FileMode.Create;
            this.googleDriveSystem = sys;
        }

        public void InitWriteStream(StorageInfo info)
        {
            CloseStream();
            if (this.Info.Length > this.googleDriveSystem.OpenParameter.BlockLength)
            {
                this.innerStream = new MemoryStream();
            }
            else
            {
                ResetRequstStream();

            }
            if (this.innerStream == null)
            {
                throw new Exception("InitWriteStream(), innerStream is null.");
            }
        }

        private long timeTotalWrite;
        private long sizeTotalWrite;
        private long blockWriteSize;
        private long blockWriteCount = 0;
        static readonly string boundary = "-------314159265358979323846";
        static readonly string delimiter = "\r\n--" + boundary + "\r\n";
        static readonly byte[] closeDelimiter = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--");

        private void ResetRequstStream()
        {
            this.googleDriveSystem.Retry<bool>(delegate()
            {
                byte[] metadataData = Encoding.UTF8.GetBytes(GenerateMetaJasonString(this.Info));
                string url = StorageUrl.GoogleDriveUpload;
                //req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "POST";
                req = this.googleDriveSystem.GenerateRequest(url, "POST");
                req.AllowWriteStreamBuffering = false;//不缓存,直接上传到device
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout//默认是100s
                req.Headers.Add("Authorization", "Bearer " + this.googleDriveSystem.GetAccessToken());
                req.ContentType = "multipart/mixed; boundary=\"" + boundary + "\"";
                req.ContentLength = metadataData.Length + this.Info.Length + closeDelimiter.Length;
                innerStream = req.GetRequestStream();
                innerStream.Write(metadataData, 0, metadataData.Length);
                return true;
            });
        }

        private string GenerateMetaJasonString(StorageInfo info)
        {
            string metadata = string.Empty;
            string parentStr = "[{\"id\": \"" + this.googleDriveSystem.OpenParameter.RootFolderId + "\"}]";
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"title\": ").Append("\"").Append(info.HighPlusLowName.Replace("\\", "_").Replace("/", "_")).Append("\"");
            sb.Append(",");
            sb.Append("\"parents\":").Append(parentStr);
            sb.Append("}");
            metadata = sb.ToString();
            string multipartRequestBody =
                  delimiter +
                  "Content-Type: application/json\r\n\r\n" +
                  metadata +
                  delimiter +
                  "Content-Type: application/octet-stream\r\n\r\n";
            return multipartRequestBody;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                if (this.Info.Length > this.googleDriveSystem.OpenParameter.BlockLength && blockWriteSize >= this.googleDriveSystem.OpenParameter.EachBlockLength)
                {
                    StorageInfo tmpInfo = this.Info.Clone();
                    tmpInfo.LowName = tmpInfo.LowName + blockWriteCount;
                    this.objectId += (UploadFile(innerStream, tmpInfo) + GoogleDriveConstant.FILE_ID_SEPARATOR);
                    blockWriteSize = 0;
                    blockWriteCount++;
                    CloseStream();
                    innerStream = new MemoryStream();
                }
                this.innerStream.Write(buffer, offset, count);
                timeTotalWrite += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalWrite += count;
                blockWriteSize += count;
                this.googleDriveSystem.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                this.googleDriveSystem.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.HighPlusLowName, ContextValues.Storage.StorageType.SkyDrive, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.SkyDrive, writeFailedEventMessage);
                logger.Error(e.Message, e);
                throw new RetryableException(e.ToString());
            }
        }

        private void CloseStream()
        {
            try
            {
                if (this.innerStream != null)
                {
                    this.innerStream.Close();
                    this.innerStream = null;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("close stream error: {0}.", ex);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readLen = this.ReadDelegate(buffer, offset, count);
            readTotalLength += readLen;
            if (readLen <= 0 && readTotalLength < this.Info.Length)
            {
                StorageInfo newInfo = new StorageInfo();
                newInfo.Length = this.Info.Length - readTotalLength;
                if (this.Info.DataType == DataBlockType.MetaData || this.Info.DataType == DataBlockType.ContentData)
                {
                    newInfo.Offset = 62 + 4096;
                    string nextMetaID = googleDriveSystem.GetNextMetaID(this.Info.ObjectId);
                    if (nextMetaID == null)
                    {
                        throw new IDNullException("NextMetaID is null, fileID=" + this.Info.ObjectId);
                    }
                    newInfo.ObjectId = nextMetaID;
                }
                else if (this.Info.DataType == DataBlockType.ContentData)
                {
                    newInfo.Offset = 4096 + 4096;
                    string nextContentId = googleDriveSystem.GetNextMetaID(this.Info.ObjectId);
                    if (nextContentId == null)
                    {
                        throw new IDNullException("nextContentId is null, fileID=" + this.Info.ObjectId);
                    }
                    newInfo.ObjectId = nextContentId;
                }
                else
                {
                    logger.Info("In the process of reading returns 0 Or length is too long, info.Length=" + this.Info.Length);
                    newInfo.Offset = this.Info.Offset + readTotalLength;
                    newInfo.ObjectId = this.Info.ObjectId;
                }
                GoogleDriveStream newStream = (GoogleDriveStream)googleDriveSystem.OpenStream(newInfo, FileMode.Open);
                this.ReadDelegate = newStream.ReadDelegate;
                this.Info.ObjectId = newInfo.ObjectId;
                //this.Info = newInfo;
                //this.readTotalLength = 0;
                return Read(buffer, offset, count);
            }
            return readLen;
        }

        public override StorageResult Commit(bool closeParent)
        {
            try
            {
                StorageResult result = new StorageResult();
                switch (this.streamMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        if (this.Info.Length > this.googleDriveSystem.OpenParameter.BlockLength)
                        {
                            StorageInfo tmpInfo = Info.Clone();
                            tmpInfo.LowName += blockWriteCount;
                            this.objectId += UploadFile(this.innerStream, tmpInfo);
                        }
                        else
                        {
                            this.objectId += ExcuteRequest(this.req);
                        }
                        if (this.Info.DataType == DataBlockType.MetaData)
                        {
                            if (this.googleDriveSystem.LastMetaId != null)
                            {
                                UpdateFileProperties(this.googleDriveSystem.LastMetaId, GetPropertyStr(new Dictionary<string, string>() { { GoogleDriveConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.googleDriveSystem.LastMetaId = this.objectId;
                        }
                        else if (this.Info.DataType == DataBlockType.ContentData)
                        {
                            if (this.googleDriveSystem.LastContentId != null)
                            {
                                UpdateFileProperties(this.googleDriveSystem.LastContentId, GetPropertyStr(new Dictionary<string, string>() { { GoogleDriveConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.googleDriveSystem.LastContentId = this.objectId;
                        }
                        result.IsCommited = true;
                        break;
                    case FileMode.Open:
                    case FileMode.Append:
                    case FileMode.Truncate:
                        break;
                    default:
                        throw new Exception("Unsupported access type.");
                }
                result.StorageInfo = string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", "", this.objectId);
                XURIResult xURIResult = new XURIResult();
                xURIResult.SInfo = new StorageInfo() { HighName = string.Empty, LowName = this.objectId, ExtraStorageInfo = result.StorageInfo };
                result.URI = xURIResult;
                result.URI.SdType = 410;
                result.URI.SysId = this.googleDriveSystem.SystemID;
                result.UriId = this.objectId;
                result.PdId = this.googleDriveSystem.SystemID;
                result.IsCommited = true;
                return result;
            }
            catch (Exception e)
            {
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.HighPlusLowName, ContextValues.Storage.StorageType.SkyDrive, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.SkyDrive, writeFailedEventMessage);
                logger.Error(e.Message, e);
                throw;
            }
            finally
            {
                if (innerStream != null)
                {
                    innerStream.Close();
                    innerStream = null;
                }
            }
        }

        public override XURIResult GetURI()
        {
            this.URI.SInfo = new StorageInfo() { HighName = string.Empty, LowName = this.objectId, ExtraStorageInfo = string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", "", this.objectId) };
            return this.URI;
        }

        /// <summary>
        /// Updates the file properties.
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <param name="body">The body, example: "{ description: "'host': 'earth', 'port': '80'" }"</param>
        /// <returns></returns>
        private bool UpdateFileProperties(string fileID, string body)
        {
            fileID = this.googleDriveSystem.SplitFileID(fileID)[0];
            return this.googleDriveSystem.Retry<bool>(delegate()
            {
                string url = String.Format(StorageUrl.GoogleDriveProperty, fileID);
                //HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "PUT";
                var req = this.googleDriveSystem.GenerateRequest(url, "PUT");
                req.Headers.Add("Authorization", "Bearer " + this.googleDriveSystem.GetAccessToken());
                req.ContentType = "application/json";
                using (Stream reqStream = req.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(body);
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("UpdatingFileProperties failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                }
                req.Abort();
                return true;
            });
        }

        private string GetPropertyStr(Dictionary<string, string> metaInfos, Dictionary<string, string> headers)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ").Append("\r");
            if (metaInfos != null && metaInfos.Count > 0)
            {
                sb.Append("\"description\": \"");
                foreach (var meta in metaInfos)
                {
                    sb.Append("'").Append(HttpUtility.UrlEncode(meta.Key).Replace("+", "%20").Replace("/", "%2F"))
                      .Append("': '").Append(HttpUtility.UrlEncode(meta.Value).Replace("+", "%20").Replace("/", "%2F")).Append("', ");
                }
                sb.Append("\"");
            }
            if (headers != null && headers.Count > 0)
            {
                if (metaInfos != null && metaInfos.Count > 0)
                {
                    sb.Append(",").Append("\r");
                }
                foreach (var meta in headers)
                {
                    sb.Append("\"").Append(HttpUtility.UrlEncode(meta.Key).Replace("+", "%20").Replace("/", "%2F")).Append("\"")
                      .Append(":").Append("\"").Append(HttpUtility.UrlEncode(meta.Value).Replace("+", "%20").Replace("/", "%2F")).Append("\"");
                }
            }
            sb.Append("\r").Append(" }");
            return sb.ToString();
        }

        private string UploadFile(Stream fileStream, StorageInfo info)
        {
            return this.googleDriveSystem.Retry<string>(delegate()
            {
                byte[] metadataData = Encoding.UTF8.GetBytes(GenerateMetaJasonString(info));
                string url = StorageUrl.GoogleDriveUpload;
                //HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "POST";
                var req = this.googleDriveSystem.GenerateRequest(url, "POST");
                req.AllowWriteStreamBuffering = false;//不缓存,直接上传到device
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout//默认是100s
                req.Headers.Add("Authorization", "Bearer " + this.googleDriveSystem.GetAccessToken());
                req.ContentType = "multipart/mixed; boundary=\"" + boundary + "\"";
                req.ContentLength = metadataData.Length + fileStream.Length + closeDelimiter.Length;
                fileStream.Position = 0;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(metadataData, 0, metadataData.Length);
                    byte[] buffer = new byte[65536];
                    while (true)
                    {
                        int readLen = fileStream.Read(buffer, 0, buffer.Length);
                        if (readLen <= 0)
                        {
                            break;
                        }
                        reqStream.Write(buffer, 0, readLen);
                    }
                    reqStream.Write(closeDelimiter, 0, closeDelimiter.Length);
                }

                string jsonStr = string.Empty;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }

                    jsonStr = new StreamReader(resp.GetResponseStream()).ReadToEnd();

                }

                Regex r = new Regex("\"kind\": \"drive#file\",\n \"id\"\\s*:\\s*\"([^\"]+)\"");
                Match m = r.Match(jsonStr);
                if (!m.Success)
                {
                    throw new Exception("Match location failed, location = " + jsonStr);
                }
                string fileID = m.Groups[1].Value;
                return fileID;
            });
        }

        private string ExcuteRequest(HttpWebRequest req)
        {

            try
            {
                innerStream.Write(closeDelimiter, 0, closeDelimiter.Length);
                innerStream.Flush();
                innerStream.Close();
                return this.googleDriveSystem.Retry<string>(delegate()
                {
                    string jsonStr = string.Empty;
                    using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, this.Info.HighPlusLowName));
                        }
                        jsonStr = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    }
                    Regex r = new Regex("\"kind\": \"drive#file\",\n \"id\"\\s*:\\s*\"([^\"]+)\"");
                    Match m = r.Match(jsonStr);
                    if (!m.Success)
                    {
                        throw new Exception("Match location failed, location = " + jsonStr);
                    }
                    return m.Groups[1].Value;
                });
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    logger.Info("this exception is a connection fail exception:" + we.Message);
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
                throw;
            }
        }

        public override void Close()
        {
            if (this.respStream != null)
            {
                this.respStream.Close();
                this.respStream = null;
            }
        }

        public override void Flush()
        {
        }



        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return this.Info.Length; }
        }

        public override long Position
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
    }
}
