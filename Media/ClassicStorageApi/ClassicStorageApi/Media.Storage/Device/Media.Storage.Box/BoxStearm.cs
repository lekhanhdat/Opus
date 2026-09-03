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


using System.Text;
using AvePoint.Media.ClassicStorage.Util;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using AvePoint.GCommon;

namespace AvePoint.Media.ClassicStorage.Box
{
    public delegate int ReadDelegate(byte[] buffer, int offset, int count);

    public class BoxStream : XStream
    {
        AveLogger logger = AveLogger.GetInstance(typeof(BoxStream));
        private Stream respStream;
        public ReadDelegate ReadDelegate { get; set; }
        private long readTotalLength;

        FileMode streamMode;
        string objectId = string.Empty;
        public string NextMetaId { get; set; }
        Stream innerStream;
        BoxSystem boxSystem;
        HttpWebRequest req;

        public BoxStream(BoxSystem sys, StorageInfo storageInfo, Stream respStream, ReadDelegate readDelegate)
            : base(sys)
        {
            this.boxSystem = sys;
            this.Info = storageInfo;
            this.respStream = respStream;
            this.ReadDelegate = readDelegate;
        }

        public BoxStream(StorageInfo info, BoxSystem sys)
            : base(sys)
        {
            this.Info = info;
            this.URI.SdType = 408;
            this.URI.SysId = sys.SystemID;    //此ID?
            this.URI.SInfo = info.Clone();
            this.streamMode = FileMode.Create;
            this.boxSystem = sys;
        }

        public void InitWriteStream(StorageInfo info)
        {
            CloseStream();
            if (this.Info.Length > this.boxSystem.OpenParameter.BlockLength)    //BlockLength默认值？
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

        protected string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("/", "%2F");
        }

        static readonly string boundary = "--AaB03x";
        static readonly byte[] closeDelimiter = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

        private string GetMultipartFormData(StorageInfo info)
        {
            //具体Multipart\FormData格式参考 rfc1867。
            //其中folder_id，filename，由box API指定要用Multipart\FormData提交的数据。
            StringBuilder sb = new StringBuilder();
            sb.Append("--" + boundary + "\r\n")
              .Append("Content-Disposition:form-data;name=\"parent_id\"\r\n\r\n")
              .Append(this.boxSystem.OpenParameter.RootFolderId + "\r\n")
              .Append("--" + boundary + "\r\n")
              .Append(string.Format("Content-Disposition:form-data;name=\"filename\";filename=\"{0}\"\r\n", info.HighPlusLowName.Replace("\\", "/").Replace("/", "-")))
              .Append("Content-Type: application/octet-stream\r\n\r\n");

            return sb.ToString();
        } 

        private void ResetRequstStream()
        {
            this.boxSystem.Retry<bool>(delegate()
            {
                byte[] metadataData = Encoding.UTF8.GetBytes(GetMultipartFormData(this.Info));
                string url = string.Empty;
                if (this.boxSystem.FileExists(this.Info))
                {
                    url = string.Format("https://upload.box.com/api/2.0/files/{0}/content", this.Info.ObjectId);
                }
                else
                {
                    url = string.Format("https://upload.box.com/api/2.0/files/content");
                }
                req = this.boxSystem.GenerateRequest("POST", url);
                req.AllowWriteStreamBuffering = false;
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout
                req.ContentType = "multipart/form-data; boundary=" + boundary;
                req.ContentLength = metadataData.Length + this.Info.Length + closeDelimiter.Length;
                innerStream = req.GetRequestStream();
                innerStream.Write(metadataData, 0, metadataData.Length);
                return true;
            });
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                if (this.Info.Length > this.boxSystem.OpenParameter.BlockLength && blockWriteSize >= this.boxSystem.OpenParameter.EachBlockLength)
                {
                    StorageInfo tmpInfo = this.Info.Clone();
                    tmpInfo.LowName = tmpInfo.LowName + blockWriteCount;
                    this.objectId += (UploadFile(innerStream, tmpInfo) + BoxConstant.FILE_ID_SEPARATOR);
                    blockWriteSize = 0;
                    blockWriteCount++;
                    CloseStream();
                    innerStream = new MemoryStream();
                }
                this.innerStream.Write(buffer, offset, count);
                timeTotalWrite += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalWrite += count;
                blockWriteSize += count;
                this.boxSystem.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                this.boxSystem.IncreaseTotalWriteBytes(count);
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
                    string nextMetaID = boxSystem.GetNextMetaID(this.Info.ObjectId);
                    if (nextMetaID == null)
                    {
                        throw new IDNullException("NextMetaID is null, fileID=" + this.Info.ObjectId);
                    }
                    newInfo.ObjectId = nextMetaID;
                }
                else if (this.Info.DataType == DataBlockType.ContentData)
                {
                    newInfo.Offset = 4096 + 4096;
                    string nextContentId = boxSystem.GetNextMetaID(this.Info.ObjectId);
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
                this.logger.Info("Start close the previous stream.");
                this.Close();
                this.logger.Info("Open another stream for read.");
                BoxStream boxStream = (BoxStream)boxSystem.OpenStream(newInfo, FileMode.Open);
                this.respStream = boxStream.respStream;
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
            try
            {
                StorageResult result = new StorageResult();
                switch (this.streamMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        if (this.Info.Length > this.boxSystem.OpenParameter.BlockLength)
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
                            if (this.boxSystem.LastMetaId != null)
                            {
                                UpdateFileProperties(this.boxSystem.LastMetaId, GetPropertyStr(new Dictionary<string, string>() { { BoxConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.boxSystem.LastMetaId = this.objectId;
                        }
                        else if (this.Info.DataType == DataBlockType.ContentData)
                        {
                            if (this.boxSystem.LastContentId != null)
                            {
                                UpdateFileProperties(this.boxSystem.LastContentId, GetPropertyStr(new Dictionary<string, string>() { { BoxConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.boxSystem.LastContentId = this.objectId;
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
                result.URI.SdType = 408;
                result.URI.SysId = this.boxSystem.SystemID;
                result.UriId = this.objectId;
                result.PdId = this.boxSystem.SystemID;
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
            fileID = this.boxSystem.SplitFileID(fileID)[0];
            return this.boxSystem.Retry<bool>(delegate()
            {
                string url = "https://api.box.com/2.0/files/" + fileID;
                req = this.boxSystem.GenerateRequest("PUT", url);
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
            return this.boxSystem.Retry<string>(delegate()
            {
                byte[] metadataData = Encoding.UTF8.GetBytes(GetMultipartFormData(info));
                string url = string.Format("https://api.box.com/2.0/files/content");
                req = this.boxSystem.GenerateRequest("POST", url);
                req.AllowWriteStreamBuffering = false;
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout
                req.ContentType = "multipart/form-data; boundary=" + boundary;
                req.ContentLength = metadataData.Length + fileStream.Length + closeDelimiter.Length;
                fileStream.Position = 0;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(metadataData, 0, metadataData.Length);
                    byte[] buffer = new byte[64 * 1024];
                    fileStream.Position = 0;
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
                string jsonStr;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream() as Stream))
                        {
                            jsonStr = sr.ReadToEnd();
                        }
                        throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, jsonStr));
                    }
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream() as Stream))
                    {
                        jsonStr = sr.ReadToEnd();
                    }
                }
                req.Abort();
                //e.g. "id": "3994506542"
                Match m = Regex.Match(jsonStr, "\"id\":\"[^,]+");
                if (!m.Success)
                {
                    throw new Exception("Match jsonStr failed, jsonStr = " + jsonStr);
                }
                string[] strs = m.Groups[0].Value.Split(':');
                string fileId = strs[1].Substring(1, strs[1].Length - 2);
                //Dictionary<string, string> properties = new Dictionary<string, string>();
                //properties.Add("name", Info.HighPlusLowName.Replace('/', '\\'));
                //UpdateFileProperties(fileId, GetPropertyStr(null, properties));
                return fileId;
            });
        }

        private string ExcuteRequest(HttpWebRequest req)
        {
            try
            {
                innerStream.Write(closeDelimiter, 0, closeDelimiter.Length);
                string jsonStr;
                innerStream.Flush();
                innerStream.Close();
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, req.RequestUri));
                    }
                    using(StreamReader sr =new StreamReader(resp.GetResponseStream() as Stream))
                    {
                        jsonStr = sr.ReadToEnd();
                    }
                }
                req.Abort();
                //e.g. "id": "3994506542"
                Match m = Regex.Match(jsonStr, "\"id\":\"[^,]+");
                if (!m.Success)
                {
                    throw new Exception("Match jsonStr failed, jsonStr = " + jsonStr);
                }
                string[] strs = m.Groups[0].Value.Split(':');
                string fileId = strs[1].Substring(1, strs[1].Length-2);
                return fileId;
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

                        //TODO
                        using (StreamReader sr = new StreamReader(we.Response.GetResponseStream() as Stream))
                        {
                            string str = sr.ReadToEnd();
                        }
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
                this.logger.Info("Close the previous stream.");
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
                throw new NotImplementedException();
            }
        }
    }
}
