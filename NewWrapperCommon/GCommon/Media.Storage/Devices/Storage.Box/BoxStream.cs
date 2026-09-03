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

using AvePoint.GCommon;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace AvePoint.Media.Storage.Box
{
    delegate int ReadDelegate(byte[] buffer, int offset, int count);

    class BoxStream : XStream
    {
        AveLogger logger = AveLogger.GetInstance(typeof(BoxStream));
        private Stream respStream;
        public ReadDelegate ReadDelegate { get; set; }
        private long readTotalLength;

        FileMode streamMode;
        string objectId = string.Empty;
        string versionId;
        public string NextMetaId { get; set; }
        Stream innerStream;
        BoxSystem boxSystem;
        HttpWebRequest req;
        string parentId;
        BoxObject boxObject;
        //XFileInfo targetFile = new XFileInfo();

        public BoxStream(BoxSystem sys, StorageInfo storageInfo, Stream respStream, ReadDelegate readDelegate)
            : base(sys)
        {
            this.boxSystem = sys;
            this.Info = storageInfo;
            this.respStream = respStream;
            this.streamMode = FileMode.Open;
            this.ReadDelegate = readDelegate;
        }

        public BoxStream(StorageInfo info, BoxSystem sys)
            : base(sys)
        {
            this.Info = info;
            this.URI.SdType = 408;
            this.URI.SysId = sys.SystemID;
            this.URI.SInfo = info.Clone();
            this.streamMode = info.FileMode;
            this.boxSystem = sys;
        }

        public void InitWriteStream()
        {
            CloseStream();

            string url = string.Format(StorageUrl.BoxUpload);
            if (this.Info.IsCreateNewVersion)
            {
                url = string.Format(StorageUrl.BoxUploadWithVersion, this.Info.ObjectId);
            }
            req = boxSystem.GenerateRequest(BoxConstants.HttpMethod_POST, url);
            req.AllowWriteStreamBuffering = false;
            req.AllowAutoRedirect = false;
            req.Timeout = 0x7ffffffe; //never timeout
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            byte[] metadataData = Encoding.UTF8.GetBytes(GetMultipartFormData(this.Info));
            req.ContentLength = metadataData.Length + this.Info.Length + closeDelimiter.Length;
            innerStream = req.GetRequestStream();
            innerStream.Write(metadataData, 0, metadataData.Length);
        }

        private long timeTotalWrite;
        private long sizeTotalWrite;

        protected string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("/", "%2F");
        }

        static readonly string boundary = "--AaB03x";
        static readonly byte[] closeDelimiter = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

        private string GetMultipartFormData(StorageInfo info)
        {
            if (!String.IsNullOrEmpty(info.ClipId) && !info.ClipId.Equals(info.HighName))
            {
                parentId = info.ClipId;
            }
            else
            {
                parentId = this.boxSystem.OpenParameter.RootFolderId;
            }
            //if (this.boxSystem.OpenParameter.RootFolderId.Equals("-1", StringComparison.OrdinalIgnoreCase))
            //{
            //    parentId = info.ClipId;
            //}
            //else
            //{
            //    parentId = this.boxSystem.OpenParameter.RootFolderId;
            //}
            //具体Multipart\FormData格式参考 rfc1867。
            //其中folder_id，filename，由box API指定要用Multipart\FormData提交的数据。
            StringBuilder sb = new StringBuilder();
            sb.Append("--" + boundary + "\r\n")
              .Append("Content-Disposition:form-data;name=\"parent_id\"\r\n\r\n")
              .Append(parentId + "\r\n")
              .Append("--" + boundary + "\r\n")
              .Append(string.Format("Content-Disposition:form-data;name=\"filename\";filename=\"{0}\"\r\n", info.LowName))
              .Append("Content-Type: application/octet-stream\r\n\r\n");
            return sb.ToString();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                this.innerStream.Write(buffer, offset, count);
                timeTotalWrite += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalWrite += count;
                this.boxSystem.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                this.boxSystem.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                logger.Error("Error when write data : {0}", e);
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
                logger.Warn("close stream error:" + ex.Message);
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
                if (this.Info.DataType == DataBlockType.MetaData)
                {
                    newInfo.Offset = 62 + 4096;
                    string nextMetaID = boxSystem.GetNextMetaID(this.Info);
                    if (nextMetaID == null)
                    {
                        throw new IDNullException("NextMetaID is null, fileID=" + this.Info.ObjectId);
                    }
                    newInfo.ObjectId = nextMetaID;
                }
                else if (this.Info.DataType == DataBlockType.ContentData)
                {
                    newInfo.Offset = 4096 + 4096;
                    string nextContentId = boxSystem.GetNextMetaID(this.Info);
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
                BoxStream boxStream = (BoxStream)boxSystem.OpenStream(newInfo, FileMode.Open);
                this.ReadDelegate = boxStream.ReadDelegate;
                this.Info.ObjectId = newInfo.ObjectId;
                //this.Info = newInfo;
                //this.readTotalLength = 0;
                return Read(buffer, offset, count);
            }
            return readLen;
        }

        public override StorageResult Commit(bool closeParent)
        {
            this.IsCommited = true;
            StorageResult result = new StorageResult();
            XFileInfo targetFile = null;
            try
            {
                switch (this.streamMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        if (!this.Info.IsCreateNewVersion)
                        {
                            if (!this.Info.ObjectId.Equals(this.Info.LowName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetFile = this.boxSystem.SetNewFileName(Info, Info.LowName + DateTime.Now.Ticks);
                            }
                            else if (!this.streamMode.Equals(FileMode.CreateNew))
                            {
                                var fileList = new List<XFileInfo>();
                                if (this.Info.ClipId.Equals(this.Info.HighName))//没有clip id
                                {
                                    this.Info.ClipId = this.boxSystem.OpenParameter.RootFolderId;
                                }
                                fileList = this.boxSystem.ListFiles(this.Info);
                                foreach (var file in fileList)
                                {
                                    if (file.LowName.Equals(this.Info.LowName))
                                    {
                                        targetFile = this.boxSystem.SetNewFileName(file, file.Name + DateTime.Now.Ticks);
                                        break;
                                    }
                                }
                            }
                        }
                        boxObject = ExcuteRequest(this.req).Entries[0];
                        this.objectId = boxObject.Id;
                        if (this.Info.DataType == DataBlockType.MetaData)
                        {
                            if (this.boxSystem.LastMetaId != null)
                            {
                                UpdateFileProperties(this.boxSystem.LastMetaId,
                                    GetPropertyStr(
                                        new Dictionary<string, string>() { { BoxConstants.META_ID_HEADER, this.objectId } },
                                        null));
                            }
                            this.boxSystem.LastMetaId = this.objectId;
                        }
                        else if (this.Info.DataType == DataBlockType.ContentData)
                        {
                            if (this.boxSystem.LastContentId != null)
                            {
                                UpdateFileProperties(this.boxSystem.LastContentId,
                                    GetPropertyStr(
                                        new Dictionary<string, string>() { { BoxConstants.META_ID_HEADER, this.objectId } },
                                        null));
                            }
                            this.boxSystem.LastContentId = this.objectId;
                        }
                        result.StorageInfo = string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", "",
                            this.objectId);
                        XURIResult xURIResult = new XURIResult();
                        xURIResult.SInfo = new StorageInfo()
                        {
                            HighName = parentId,
                            LowName = this.objectId,
                            ExtraStorageInfo = result.StorageInfo,
                            Etag = boxObject.Etag
                        };
                        if (this.Info.IsCreateNewVersion)
                        {
                            List<XFileInfo> boxList = this.boxSystem.GetFileVersion(Info.ObjectId, Info.HighName,
                                Info.LowName);
                            this.versionId = xURIResult.SInfo.VersionId = boxList[boxList.Count - 1].ObjectId;
                        }
                        else
                        {
                            if (targetFile != null)
                            {
                                this.boxSystem.DeleteFile(targetFile);
                            }
                        }
                        result.URI = xURIResult;
                        result.URI.SdType = 408;
                        result.URI.SysId = this.boxSystem.SystemID;
                        result.UriId = this.objectId;
                        result.PdId = this.boxSystem.SystemID;
                        result.IsCommited = true;
                        break;
                    case FileMode.Open:
                        break;
                    default:
                        throw new Exception("Unsupported access type.");
                }
                return result;
            }
            catch (WebException e)
            {
                logger.Error("commit stream {0} failed {1}", this.Info.HighPlusLowName, e);
                var errorResponse = e.Response as HttpWebResponse;
                if (errorResponse != null)
                {
                    var statusCode = errorResponse.StatusCode;
                    var errorMessage = String.Empty;
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        errorMessage = reader.ReadToEnd();
                        if (String.IsNullOrEmpty(errorMessage))
                        {
                            errorMessage = e.Message;
                        }
                        this.logger.Error("Error Message:{0}, details:{1}", errorMessage, e.ToString());
                    }
                    errorResponse.Close();
                    errorResponse = null;

                    if (statusCode == HttpStatusCode.Unauthorized)
                    {
                        this.boxSystem.ResetToken();
                        errorMessage = "The token has expired, and it also has been reset, please have a try.";
                    }
                    this.SetNewFileName(targetFile, this.Info.LowName);
                    throw new RetryableException(errorMessage);
                }
                this.SetNewFileName(targetFile, this.Info.LowName);
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

        private void SetNewFileName(StorageInfo targetFile, String fileName)
        {
            try
            {
                if (targetFile != null)
                {
                    this.boxSystem.SetNewFileName(targetFile, fileName);
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while setting file name,details:{0}", ex.ToString());
            }
        }

        public override XURIResult GetURI()
        {
            this.URI.SInfo = new StorageInfo() { HighName = string.Empty, LowName = this.objectId, ObjectId = this.objectId, VersionId = this.versionId, ExtraStorageInfo = string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", "", this.objectId) };
            if (this.boxObject != null)
            {
                this.URI.SInfo.Etag = this.boxObject.Etag;
            }
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
            return this.boxSystem.Retry<bool>(delegate()
            {
                string url = String.Format(StorageUrl.BoxFileInfo, fileID);
                HttpWebRequest req = this.boxSystem.GenerateRequest(BoxConstants.HttpMethod_PUT, url);
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

        private BoxObject ExcuteRequest(HttpWebRequest req)
        {
            innerStream.Write(closeDelimiter, 0, closeDelimiter.Length);
            innerStream.Flush();
            innerStream.Close();

            string jsonStr;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, req.RequestUri));
                }
                using (StreamReader sr = new StreamReader(resp.GetResponseStream() as Stream))
                {
                    jsonStr = sr.ReadToEnd();
                }
            }
            req.Abort();
            return this.boxSystem.ParseJsonString(jsonStr);
        }

        public override void Close()
        {
            //if (!this.IsCommited)
            //{
            //    this.Commit();
            //}
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
