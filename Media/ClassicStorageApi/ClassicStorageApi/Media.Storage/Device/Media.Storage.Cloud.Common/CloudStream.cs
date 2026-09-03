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




namespace AvePoint.Media.ClassicStorage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AvePoint.Media.ClassicStorage.Util;
    using System.Reflection;
    using System.Net;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
    using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
    using System.Globalization;
    using AvePoint.GCommon.Utility.I18N;
    using global::Storage.Util;
    using AvePoint.Media.StorageApi;
    using RetryableException = global::Storage.Util.RetryableException;
    using AvePoint.GCommon;
    #endregion

    public class CloudStream : XStream
    {
        public HttpWebRequest HttpWebRequest { set; get; }
        private ushort eventTaskCategory;

        protected ushort EventTaskCategory
        {
            get
            {
                return this.eventTaskCategory;
            }
            set
            {
                this.eventTaskCategory = value;
            }
        }

        private ContextValues.Storage.StorageType eventTaskMessage = ContextValues.Storage.StorageType.Cloud;

        protected ContextValues.Storage.StorageType EventTaskMessage
        {
            get
            {
                return this.eventTaskMessage;
            }
            set
            {
                this.eventTaskMessage = value;
            }
        }

        protected void SetEventTaskInfo(IXSystemCommon currentSystem)
        {
            if (currentSystem == null)
            {
                return;
            }
            else
            {
                switch (System.GetType().Name)
                {
                    case "AmazonSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Amazon_S3;
                        eventTaskMessage = ContextValues.Storage.StorageType.Amazon;
                        break;
                    case "AtmosSystem":
                        if (currentSystem.XriObject.VIM.Equals("atmos_vim"))
                        {
                            eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_EMC_Atmos;
                            eventTaskMessage = ContextValues.Storage.StorageType.Atmos;
                        }
                        else
                        {
                            eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_ATT_Synaptic;
                            eventTaskMessage = ContextValues.Storage.StorageType.ATT;
                        }
                        break;
                    case "AzureSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Windows_Azure;
                        eventTaskMessage = ContextValues.Storage.StorageType.Azure;
                        break;
                    case "RackspaceSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Rackspace;
                        eventTaskMessage = ContextValues.Storage.StorageType.Rackspace;
                        break;
                    case "HCPSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.HDS_HCP;
                        eventTaskMessage = ContextValues.Storage.StorageType.HCP;
                        break;
                    default:
                        break;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

    }//4194304

    public class HttpUploadStream : CloudStream
    {
        public AveLogger Logger { get { return AveLogger.GetInstance(typeof(HttpUploadStream)); } }
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
                request.AllowWriteStreamBuffering = false;
                request.AllowAutoRedirect = false;
                request.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout
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
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.HttpWebRequest.RequestUri.AbsoluteUri, EventTaskMessage, e);
                //this.Logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, writeFailedEventMessage);
                Logger.Error("Write file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                throw new Storage.Util.RetryableException(e.Message, e);
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
                        throw new Storage.Util.RetryableException(we.Message, we);
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
            if (System != null)
            {
                (System as CloudSystem).RemoveFromActivedStream(this);
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
            if (System != null)
            {
                (System as CloudSystem).RemoveFromActivedStream(this);
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

    public class HttpDownloadStream : CloudStream
    {
        public HttpWebResponse Response { get; set; }
        public long InnerLength { get; protected set; }
        long totalReadLength = 0;
        public long NowOffset { set; get; }
        public long NowLenght { set; get; }
        public AveLogger Logger { get { return AveLogger.GetInstance(typeof(HttpDownloadStream)); } }

        public override long Length
        {
            get
            {
                return this.InnerLength;
            }
        }

        public HttpDownloadStream(HttpWebResponse response)
        {

            if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.PartialContent == response.StatusCode)
            {
                Stream respStream = response.GetResponseStream();
                InnerStream = new BufferedStream(respStream, 64 * 1024);
            }
            else
            {
                throw new Exception(string.Format("Open Http Down Stream Error, Error Code : {0}", response.StatusCode));
            }
            this.Response = response;
            this.InnerLength = response.ContentLength;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            StorageInfo tempInfo = Info.Clone();
            int readLen = 0;
            try
            {
                while (readLen < count)
                {
                    int tempLen = InnerStream.Read(buffer, offset + readLen, count - readLen);
                    readLen += tempLen;
                    totalReadLength += tempLen;
                    if (readLen >= count || (tempLen <= 0 && totalReadLength >= Info.Length))
                    {
                        Info.CurrentRetryCount = 0;
                        break;
                    }
                    if (tempLen <= 0 && totalReadLength < Info.Length)
                    {
                        throw new Exception("The result of InnerStream.read is 0, either server read exception or info.Length is inexactly, info.length: " + Info.Length);
                    }
                }
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception e)
            {
                //Logger.Warn("Error Occurred during reading content (" + "R" + "ETRING".ToLower(CultureInfo.InvariantCulture) + " : " + Info.CurrentRetryCount + " ):" + e.Message, e);
                if (Info.CurrentRetryCount < MaxRetryCount)
                {
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                    }
                    InnerStream = null;
                    tempInfo.Offset += totalReadLength;
                    tempInfo.Length -= totalReadLength;
                    InnerStream = (System.OpenStream(tempInfo, FileMode.Open) as HttpDownloadStream).InnerStream;
                    return Read(buffer, offset + readLen, count - readLen) + readLen;
                }
                else
                {
                    SetEventTaskInfo(System);
                    //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(this.Response.ResponseUri.AbsolutePath, EventTaskMessage, e);
                    //this.Logger.Log(EventSources.DocAveStorageAPIService, EventTaskCategory, readFailedEventMessage);
                    Logger.Error("Read file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                    throw;
                }
            }
            return readLen;
        }

        //public virtual void Reopen()
        //{
        //    Info.CurrentRetryCount++;
        //    if (NowOffset == 0 && NowLenght == 0 && Info != null)
        //    {
        //        NowOffset = Info.Offset;
        //        NowLenght = Info.Length;
        //    }
        //    if (Info != null)
        //    {
        //        Info.Offset = NowOffset;
        //        Info.Length = NowLenght;
        //        if (System != null)
        //        {
        //            if (InnerStream != null)
        //            {
        //                InnerStream.Close();
        //            }
        //            InnerStream = null;
        //            InnerStream = (System.OpenStream(Info, FileMode.Open) as HttpDownloadStream).InnerStream;
        //        }
        //    }
        //}

        public override void ClosedUnmoral()
        {
            if (Response != null)
            {
                Response.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
            if (System != null)
            {
                (System as CloudSystem).RemoveFromActivedStream(this);
            }
        }

        public override void Close()
        {
            if (Response != null)
            {
                Response.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
            if (System != null)
            {
                (System as CloudSystem).RemoveFromActivedStream(this);
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
    }

    public class CloudStream2 : XStream
    {
        #region -- Private Members --

        FileMode fileMode;
        string ctnName;
        string objName;
        AbstractRESTOprationExecutor client;
        Stream innerStream;
        long offset;
        long length;
        long readLenth;
        long writeLenth;
        int retryCount = 0;
        int maxRetryCount = 6;
        AveLogger Logger;
        HttpWebRequest request;
        int dataBlockNumber;

        //for block used by Azure cloud
        public static readonly int maxBlobSize = 64 * 1024 * 1024;
        public static readonly int blobSize = 4 * 1024 * 1024;
        public static readonly int BUFFER_SIZE = 64 * 1024;//4096;
        private IXSystemCommon sys;

        #endregion

        #region -- Constructor --

        public CloudStream2(AbstractRESTOprationExecutor client, StorageInfo info, FileMode fileMode, AbstractXSystem sys)
            : base(sys)
        {
            this.client = client;
            this.ctnName = info.HighName;
            if (!string.IsNullOrEmpty(sys.SystemLocation))
            {
                this.ctnName = sys.SystemLocation;
            }
            this.objName = info.LowName;
            this.fileMode = fileMode;
            this.Logger = AveLogger.GetInstance(typeof(CloudStream2));
            this.readLenth = 0;
            this.writeLenth = 0;
            this.dataBlockNumber = 0;
            this.offset = info.Offset;
            this.length = info.Length;

            this.URI.SdType = 4;
            this.sys = sys;
            this.URI.SysId = sys.SystemID;
            this.URI.SInfo = info.Clone();

            string fullURL = client.BuildObjectAbsoluteURL(ctnName, objName);
            switch (fileMode)
            {
                case FileMode.Open:
                    Dictionary<string, string> readHeaders = (client as IHttpRequestPrepare).OpenStreamReadModeHeaders;
                    long rangFrom = offset;
                    long rangeTo = offset + length;
                    if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
                    {
                        string range = "bytes=" + rangFrom + "-" + rangeTo;
                        readHeaders.Add("Range", range);
                    }
                    innerStream = client.OpenObjectForRead(fullURL, readHeaders);
                    break;
                case FileMode.Append:
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    Dictionary<string, string> writeHeaders = (client as IHttpRequestPrepare).OpenStreamWriteModeHeaders;
                    writeHeaders.Add("Content-Type", "DOCAVE/data".ToLower(CultureInfo.InvariantCulture));
                    writeHeaders.Add("Content-Length", info.Length + "");
                    innerStream = client.OpenObjectForWrite(fullURL, writeHeaders);
                    break;
                default:
                    throw new Exception("Unknown File Mode : " + fileMode);
            }
        }

        #endregion

        #region -- XStream Members --

        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                return length;
            }
        }

        public override long Position
        {
            get
            {
                return readLenth;
            }
            set
            {
            }
        }

        public int ReadFromStream(byte[] buffer, int off, int len)
        {
            int read = 0;
            int length = 0;
            int remaining = len;
            int totalRead = 0;
            while (remaining > 0)
            {
                length = remaining > BUFFER_SIZE ? BUFFER_SIZE : remaining;
                read = innerStream.Read(buffer, off, length);
                if (read == -1 || read == 0)
                {
                    break;
                }
                off += read;
                remaining -= read;
                totalRead += read;
            }
            return totalRead == 0 ? -1 : totalRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = -1;
            while (retryCount < maxRetryCount)
            {
                if (innerStream == null)
                {
                    innerStream = client.OpenObject(ctnName, objName, (int)(this.offset + readLenth), (int)(this.length + offset));
                }
                try
                {
                    len = ReadFromStream(buffer, offset, count);
                }
                catch (Exception e)
                {
                    this.Logger.Error("read from cloud error, read length:" + len + ", error message:" + e.Message, e);
                    throw;
                }
                if (len >= 0)
                {
                    readLenth += len;
                }
                if (len == count)
                {
                    break;
                }
                else if (len < count)
                {
                    if (readLenth < this.length)
                    {
                        Logger.Warn("downLoadStream is aborted, we will get the xStream again, xSet :"
                                        + this.ctnName
                                        + ", xStream :"
                                        + this.objName
                                        + ",xStream position:"
                                        + this.offset);
                        Logger.Info("read length :" + len + "bytesToRead :" + count);
                        Logger.Info("reopen xStream for cloud, retry count:" + retryCount);
                        if (innerStream != null)
                        {
                            innerStream.Close();
                        }
                        innerStream = null;
                        retryCount++;
                        if (len >= 0)
                        {
                            readLenth -= len;
                        }
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return len;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                if (length > maxBlobSize)
                {
                    if (writeLenth + count - offset > blobSize)
                    {
                        int toWrite = (int)(blobSize - writeLenth);
                        innerStream.Write(buffer, offset, toWrite);
                        dataBlockNumber++;
                        request = client.GetUploadRequest(ctnName, objName, XConst.DOCAVE + "/data", request, dataBlockNumber, length);
                        innerStream = request.GetRequestStream();
                        innerStream.Write(buffer, offset + toWrite, count - toWrite);
                        writeLenth = count - offset - toWrite;
                    }
                    else
                    {
                        innerStream.Write(buffer, offset, count);
                        writeLenth = writeLenth + count - offset;
                    }
                }
                else
                {
                    innerStream.Write(buffer, offset, count);
                    writeLenth = writeLenth + count - offset;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
        }

        public override StorageResult Commit(bool closeParent)
        {
            return Commit();
        }

        private bool isCommited;

        public override StorageResult Commit()
        {
            StorageResult rs = null;
            try
            {
                switch (fileMode)
                {
                    case FileMode.Open:
                        break;
                    case FileMode.Append:
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                        client.CreateObject(ctnName, objName, request, length);
                        break;
                    default:
                        throw new Exception("Unknown File Mode : " + fileMode);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
            rs = new StorageResult();
            rs.PdId = sys.SystemID;
            isCommited = true;
            return rs;
        }

        public override void Close()
        {
            try
            {
                if (!isCommited)
                {
                    Commit();
                }
                base.Close();
                if (innerStream != null)
                {
                    innerStream.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Warn("close stream error:" + e.Message, e);
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }

        #endregion
    }
}
