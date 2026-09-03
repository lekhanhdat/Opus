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

namespace AvePoint.Media.Storage.OneDrive
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    #endregion

    delegate int ReadDelegate(byte[] buffer, int offset, int count);

    #region CodeReview
    [AveCodeReview(
    "2012/9/13",
    "rongbiao.sun@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion
    class OneDriveStream : XStream
    {
        AveLogger logger = AveLogger.GetInstance(typeof(OneDriveSystem));
        private Stream respStream;
        public ReadDelegate ReadDelegate { get; set; }
        private long readTotalLength;

        FileMode streamMode;
        string objectId = string.Empty;
        public string NextMetaId { get; set; }
        Stream innerStream;
        OneDriveSystem skyDriveSystem;
        HttpWebRequest req;

        public OneDriveStream(OneDriveSystem sys, StorageInfo storageInfo, Stream respStream, ReadDelegate readDelegate)
            : base(sys)
        {
            this.skyDriveSystem = sys;
            this.Info = storageInfo;
            this.respStream = respStream;
            this.ReadDelegate = readDelegate;
        }

        public OneDriveStream(StorageInfo info, OneDriveSystem sys)
            : base(sys)
        {
            this.Info = info;
            this.URI.SdType = 407;
            this.URI.SysId = sys.SystemID;
            this.URI.SInfo = info.Clone();
            this.streamMode = FileMode.Create;
            this.skyDriveSystem = sys;
        }

        public void InitWriteStream(StorageInfo info)
        {
            CloseStream();
            if (!string.IsNullOrEmpty(info.ExtraStorageInfo))
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();
                properties.Add("name", Guid.NewGuid().ToString());
                UpdateFileProperties(info.ObjectId, GetPropertyStr(null, properties));
            }
            if (this.Info.Length > this.skyDriveSystem.OpenParameter.BlockLength)
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

        private void ResetRequstStream()
        {
            this.skyDriveSystem.Retry<bool>(delegate()
            {
                string url = String.Format(StorageUrl.OneDriveUpload, this.skyDriveSystem.OpenParameter.RootFolderId, Guid.NewGuid(), this.skyDriveSystem.GetAccessToken());
                //req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "PUT";
                var req = this.skyDriveSystem.GenerateRequest(url, "PUT");
                req.AllowWriteStreamBuffering = false;//不缓存,直接上传到device
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout//默认是100s
                req.ContentLength = this.Info.Length;
                innerStream = req.GetRequestStream();
                this.req = req;
                return true;
            });
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                if (this.Info.Length > this.skyDriveSystem.OpenParameter.BlockLength && blockWriteSize >= this.skyDriveSystem.OpenParameter.EachBlockLength)
                {
                    StorageInfo tmpInfo = this.Info.Clone();
                    tmpInfo.LowName = tmpInfo.LowName + blockWriteCount;
                    this.objectId += (UploadFile(innerStream, tmpInfo) + OneDriveConstant.FILE_ID_SEPARATOR);
                    blockWriteSize = 0;
                    blockWriteCount++;
                    CloseStream();
                    innerStream = new MemoryStream();
                }
                this.innerStream.Write(buffer, offset, count);
                timeTotalWrite += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalWrite += count;
                blockWriteSize += count;
                this.skyDriveSystem.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                this.skyDriveSystem.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.HighPlusLowName, ContextValues.Storage.StorageType.SkyDrive, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.SkyDrive, writeFailedEventMessage);
                logger.Error("Error when write : {0}", e);
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
                    string nextMetaID = skyDriveSystem.GetNextMetaID(this.Info.ObjectId);
                    if (nextMetaID == null)
                    {
                        throw new IDNullException("NextMetaID is null, fileID=" + this.Info.ObjectId);
                    }
                    newInfo.ObjectId = nextMetaID;
                }
                else if (this.Info.DataType == DataBlockType.ContentData)
                {
                    newInfo.Offset = 4096 + 4096;
                    string nextContentId = skyDriveSystem.GetNextMetaID(this.Info.ObjectId);
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
                OneDriveStream skyDriveStream = (OneDriveStream)skyDriveSystem.OpenStream(newInfo, FileMode.Open);
                this.ReadDelegate = skyDriveStream.ReadDelegate;
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
                        if (this.Info.Length > this.skyDriveSystem.OpenParameter.BlockLength)
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
                            if (this.skyDriveSystem.LastMetaId != null)
                            {
                                UpdateFileProperties(this.skyDriveSystem.LastMetaId, GetPropertyStr(new Dictionary<string, string>() { { OneDriveConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.skyDriveSystem.LastMetaId = this.objectId;
                        }
                        else if (this.Info.DataType == DataBlockType.ContentData)
                        {
                            if (this.skyDriveSystem.LastContentId != null)
                            {
                                UpdateFileProperties(this.skyDriveSystem.LastContentId, GetPropertyStr(new Dictionary<string, string>() { { OneDriveConstant.META_ID_HEADER, this.objectId } }, null));
                            }
                            this.skyDriveSystem.LastContentId = this.objectId;
                        }
                        if (!string.IsNullOrEmpty(this.Info.ExtraStorageInfo) && this.Info.IsDeleteOldVersion)
                        {
                            this.skyDriveSystem.DeleteFile(new StorageInfo()
                            {
                                ObjectId = this.Info.ObjectId,
                            });
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
                result.URI.SdType = 407;
                result.URI.SysId = this.skyDriveSystem.SystemID;
                result.UriId = this.objectId;
                result.PdId = this.skyDriveSystem.SystemID;
                result.IsCommited = true;
                this.logger.Debug("lowName:{0}, ExtraStorageInfo:{1}", this.objectId, result.StorageInfo);
                return result;
            }
            catch (Exception e)
            {
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.HighPlusLowName, ContextValues.Storage.StorageType.SkyDrive, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.SkyDrive, writeFailedEventMessage);
                logger.Error("Error when commit : {0}", e);
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

        //private string UploadStream()
        //{
        //    string fileID = null;
        //    if (innerStream.Length >= this.skyDriveSystem.OpenParameter.BlockLength)
        //    {
        //        fileID = UploadBlockFile();
        //        this.Info.MetaInfos[BLOCK_FILE_SIZE] = Convert.ToString(innerStream.Length);
        //    }
        //    else
        //    {
        //        fileID = UploadFile(innerStream, Info);
        //    }
        //    logger.Debug("UploadFile succeed, fileID=" + fileID);
        //    return fileID;
        //}

        //private string UploadBlockFile()
        //{
        //    string fileID = null;
        //    using (MemoryStream mStream = new MemoryStream(this.skyDriveSystem.OpenParameter.EachBlockLength))
        //    {
        //        byte[] buffer = new byte[this.skyDriveSystem.OpenParameter.EachBlockLength];
        //        int freeCapacity = this.skyDriveSystem.OpenParameter.EachBlockLength;
        //        int dataBlockNumber = 0;
        //        while (true)
        //        {
        //            int readLen = innerStream.Read(buffer, 0, freeCapacity);
        //            if (readLen <= 0)
        //            {
        //                break;
        //            }
        //            mStream.Write(buffer, 0, readLen);
        //            if ((freeCapacity = mStream.Capacity - (int)mStream.Position) == 0)
        //            {
        //                StorageInfo uploadInfo = Info.Clone();
        //                uploadInfo.LowName = Info.LowName + "_" + dataBlockNumber;
        //                fileID += UploadFile(mStream, uploadInfo) + FILE_ID_SEPARATOR;
        //                mStream.SetLength(0);
        //                mStream.Position = 0;
        //                freeCapacity = this.skyDriveSystem.OpenParameter.EachBlockLength;
        //            }
        //            dataBlockNumber++;
        //        }
        //        if (mStream.Position != 0)
        //        {
        //            StorageInfo uploadInfo = Info.Clone();
        //            uploadInfo.LowName = Info.LowName + "_" + dataBlockNumber;
        //            fileID += UploadFile(mStream, uploadInfo) + FILE_ID_SEPARATOR;
        //        }
        //        fileID = fileID.Substring(0, fileID.Length - FILE_ID_SEPARATOR.Length);
        //    }
        //    return fileID;
        //}

        /// <summary>
        /// Updates the file properties.
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <param name="body">The body, example: "{ description: "'host': 'earth', 'port': '80'" }"</param>
        /// <returns></returns>
        private bool UpdateFileProperties(string fileID, string body)
        {
            //fileID = this.skyDriveSystem.SplitFileID(fileID)[0];
            this.logger.Debug("Update file properties,details :fileID:{0},body:{1}.", fileID, body);
            return this.skyDriveSystem.Retry<bool>(delegate()
            {
                string url = String.Format(StorageUrl.OneDriveUpdate, fileID);
                //HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "PUT";
                var req = this.skyDriveSystem.GenerateRequest(url, "PUT");
                req.Headers.Add("Authorization", "Bearer " + this.skyDriveSystem.GetAccessToken());
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
                    sb.Append("'").Append(meta.Key)
                      .Append("': '").Append(meta.Value).Append("', ");
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
                    sb.Append("\"").Append(meta.Key).Append("\"")
                      .Append(":").Append("\"").Append(meta.Value).Append("\"");
                }
            }
            sb.Append("\r").Append(" }");
            return sb.ToString();
        }

        private string UploadFile(Stream fileStream, StorageInfo info)
        {
            return this.skyDriveSystem.Retry<string>(delegate()
            {
                string url = String.Format(StorageUrl.OneDriveUpload, this.skyDriveSystem.OpenParameter.RootFolderId, Guid.NewGuid(), this.skyDriveSystem.GetAccessToken());
                //HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
                //req.Method = "PUT";
                var req = this.skyDriveSystem.GenerateRequest(url, "PUT");
                req.AllowWriteStreamBuffering = false;
                req.AllowAutoRedirect = false;
                req.Timeout = 0x7ffffffe; //never timeout
                req.ContentLength = fileStream.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    byte[] buffer = new byte[65536];
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
                }
                string locationStr;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    locationStr = resp.Headers["Location"];
                }
                req.Abort();

                Regex r = new Regex(StorageUrl.OneDriveRegex);
                Match m = r.Match(locationStr);
                if (!m.Success)
                {
                    throw new Exception("Match location failed, location = " + locationStr);
                }
                string fileID = m.Groups[1].Value;
                Dictionary<string, string> properties = new Dictionary<string, string>();
                properties.Add("name", info.HighPlusLowName.Replace('/', '\\'));
                UpdateFileProperties(fileID, GetPropertyStr(null, properties));
                return fileID;
            });
        }

        private string ExcuteRequest(HttpWebRequest req)
        {
            try
            {
                string locationStr;
                innerStream.Flush();
                innerStream.Close();
                string fileID = this.skyDriveSystem.Retry<string>(delegate()
                {
                    using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("UploadFile failed, StatusCode={0} URL={1}", resp.StatusCode, req.RequestUri));
                        }
                        locationStr = resp.Headers["Location"];
                    }
                    req.Abort();
                    Regex r = new Regex(StorageUrl.OneDriveRegex);
                    Match m = r.Match(locationStr);
                    if (!m.Success)
                    {
                        throw new Exception("Match location failed, location = " + locationStr);
                    }
                    return m.Groups[1].Value;
                });
                Dictionary<string, string> properties = new Dictionary<string, string>();
                String[] pattern = { "\\", "/", ":", ";", "*", "<", ">", "|", "?" };
                String[] replacePattern = { "%5c", "%2f", "%3a", "%3b", "%2A", "%3c", "%3e", "%7c", "%3f" };
                String info = Info.HighPlusLowName;
                for (int i = 0; i < pattern.Length; i++)
                {
                    info = info.Replace(pattern[i], replacePattern[i]);
                }
                properties.Add("name", info);
                UpdateFileProperties(fileID, GetPropertyStr(null, properties));
                return fileID;
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
