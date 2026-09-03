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

namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    #endregion

    #region Code Review
    [AveCodeReview(
    "2012/3/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_HC_1 },
    null,
    true)]
    #endregion

    class HCPDownloadStream : HttpDownloadStream
    {
        private HCPOpenParameter openParameter;
        long totalReadLength = 0;
        public override long Length
        {
            get
            {
                return InnerLength;
            }
        }

        public HCPDownloadStream(HttpWebResponse response, HCPOpenParameter openParameter)
            : base(response)
        {
            this.openParameter = openParameter;
            this.MaxRetryCount = openParameter.MaxRetryCount;
            this.Response = response;
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
                        throw new Exception(string.Format("The result of InnerStream.read is 0, either server read exception or info.Length is inexactly, info.Length: {0}, info.offset: {1}", Info.Length, Info.Offset));
                    }
                }
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception ex)
            {
                Logger.Warn("An error occurred while reading content, retry count : {0}. Error details: {1}.", Info.CurrentRetryCount, ex);
                if (Info.CurrentRetryCount < 2 * MaxRetryCount)
                {
                    if (Info.CurrentRetryCount >= MaxRetryCount)
                    {
                        if (!string.IsNullOrEmpty(openParameter.SecondaryHost))
                        {
                            Logger.Info("Begin downloading with second host: {0}", openParameter.SecondaryHost);
                            openParameter.PrimaryHost = openParameter.SecondaryHost;
                        }
                    }
                    if (openParameter.FlushDNS)
                    {
                        Logger.Debug("Begin flushing DNS");
                        DnsUtil.FlushMyCache();
                        Logger.Debug("Finished flushing DNS");
                    }
                    Logger.Debug("Begin sleeping for delay retry");
                    Thread.Sleep(openParameter.RetryInterval);
                    Logger.Debug("Finished sleeping for delay retry");
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                    }
                    InnerStream = null;
                    tempInfo.Offset += totalReadLength;
                    tempInfo.Length -= totalReadLength;
                    InnerStream = (System.OpenStream(tempInfo, FileMode.Open) as HttpDownloadStream).InnerStream;
                    return Read(buffer, offset + readLen, count - readLen);
                }
                else
                {
                    //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(this.Response.ResponseUri.AbsolutePath, ContextValues.Storage.StorageType.HCP, ex);
                    //this.Logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.HDS_HCP, readFailedEventMessage);
                    Logger.Error("Read file {0} failed, error message: {1}", this.Response.ResponseUri.AbsoluteUri.ToString(), ex);
                    throw;
                }
            }
            return readLen;
        }

    }

    #region
    [AveCodeReview(
    "2012/3/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_CO_10 },
    "ADO-28237",
    true)]
    #endregion
    class HCPHttpUploadStream : HttpUploadStream
    {
        private HttpWebRequest secRequest;
        private HCPOpenParameter OpenParam;
        readonly string Temp_file_Location = @"..\TEMP\HCP_TEMP".ToLower(CultureInfo.InvariantCulture);
        string tmpFilePath;
        const long MaxCacheStreamLength = 1024 * 1024 * 8;

        public override bool IsCommitStream
        {
            get
            {
                return base.IsCommitStream;
            }
            set
            {
                if (InnerStream == null)
                {
                    InnerStream = new BufferedStream(this.HttpWebRequest.GetRequestStream());
                }
                base.IsCommitStream = value;
            }
        }

        public HCPHttpUploadStream(HttpWebRequest request, HCPOpenParameter OpenParamter)
            : base(null)
        {
            this.HttpWebRequest = request;
            this.HttpWebRequest.AllowWriteStreamBuffering = false;
            this.HttpWebRequest.AllowAutoRedirect = false;
            this.HttpWebRequest.Timeout = 0x7ffffffe; //never timeout
            this.HttpWebRequest.SendChunked = true;
            this.OpenParam = OpenParamter;
            if (OpenParamter.IsRetry)
            {
                if (request.ContentLength > MaxCacheStreamLength)
                {
                    if (!Directory.Exists(Temp_file_Location))
                    {
                        Directory.CreateDirectory(Temp_file_Location);
                    }
                    tmpFilePath = Path.Combine(Temp_file_Location, Guid.NewGuid().ToString() + ".dat");
                    InnerStream = new BufferedStream(new FileStream(tmpFilePath, FileMode.OpenOrCreate), 64 * 1024);
                }
                else
                {
                    InnerStream = new BufferedStream(new MemoryStream(), 64 * 1024);
                }
            }
            else
            {
                IsCommitStream = true;
            }
        }

        public HCPHttpUploadStream(HttpWebRequest request, HttpWebRequest secRequest, HCPOpenParameter OpenParamter)
            : base(null)
        {
            this.HttpWebRequest = request;
            this.secRequest = secRequest;
            this.OpenParam = OpenParamter;
            this.HttpWebRequest.AllowWriteStreamBuffering = false;
            this.HttpWebRequest.AllowAutoRedirect = false;
            this.HttpWebRequest.Timeout = 0x7ffffffe; //never timeout
            this.HttpWebRequest.SendChunked = true;
            this.secRequest.AllowWriteStreamBuffering = false;
            this.secRequest.AllowAutoRedirect = false;
            this.secRequest.Timeout = 0x7ffffffe; //never timeout
            this.secRequest.SendChunked = true;
            if (OpenParamter.IsRetry)
            {
                if (request.ContentLength > MaxCacheStreamLength)
                {
                    if (!Directory.Exists(Temp_file_Location))
                    {
                        Directory.CreateDirectory(Temp_file_Location);
                    }
                    tmpFilePath = Path.Combine(Temp_file_Location, Guid.NewGuid().ToString() + ".dat");
                    InnerStream = new BufferedStream(new FileStream(tmpFilePath, FileMode.OpenOrCreate), 64 * 1024);
                }
                else
                {
                    InnerStream = new BufferedStream(new MemoryStream(), 64 * 1024);
                }
            }
            else
            {
                IsCommitStream = true;
            }

        }

        private void CommitWithPrimaryHost()
        {
            try
            {
                DoExcute(HttpWebRequest);
            }
            catch (Exception e)
            {
                Logger.Error("CommitWithPrimaryHost error: {0}.", e);
                //DeleteDirtyData();
                throw;
            }
            finally
            {
                if (InnerStream != null)
                {
                    try
                    {
                        InnerStream.Close();
                        InnerStream = null;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString());
                    }
                }
                if (File.Exists(tmpFilePath))
                {
                    try
                    {
                        File.Delete(tmpFilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString());
                    }
                }
            }

        }

        private void CommitWithPrimaryTwoHosts()
        {
            try
            {
                try
                {
                    DoExcute(HttpWebRequest);
                }
                catch (Exception ex)
                {
                    Logger.Error("upload file error: {0}", ex);
                    //DeleteDirtyData();
                    try
                    {
                        if (this.OpenParam.FlushDNS)
                        {
                            Logger.Debug("Begin flushing DNS");
                            DnsUtil.FlushMyCache();
                            Logger.Debug("Finished flushing DNS");
                        }

                        Logger.Debug("Begin sleeping for delay retry");
                        Thread.Sleep(OpenParam.RetryInterval);
                        Logger.Debug("Finished sleeping for delay retry");

                        HCPClient hcpClient = ((HCPClient)((HCPSystem)System).Client);
                        if (!string.IsNullOrEmpty((hcpClient.OpenParam.SecondaryHost)))
                        {
                            hcpClient.OpenParam.PrimaryHost = hcpClient.OpenParam.SecondaryHost;
                            hcpClient.OpenParam.SecondaryHost = string.Empty;
                        }
                        Logger.Info("Begin uploading with second host: {0}", secRequest.RequestUri);
                        DoExcute(secRequest);
                    }
                    catch (Exception exce)
                    {
                        Logger.Error("Retry with second name space failed: {0}.", exce);
                        //DeleteDirtyData();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
            finally
            {
                if (InnerStream != null)
                {
                    try
                    {
                        InnerStream.Close();
                        InnerStream = null;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString());
                    }
                }
                if (File.Exists(tmpFilePath))
                {
                    try
                    {
                        File.Delete(tmpFilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString());
                    }
                }
            }
        }


        private StorageResult HCPCommit()
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
                            throw new WebException("Create object failed. object : " + HttpWebRequest.RequestUri);
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
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (HttpWebResponse response = we.Response as HttpWebResponse)
                        {
                            if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                ((HCPClient)((HCPSystem)System).Client).HandleFailOverMode(null, null, null, we);
                            }
                        }
                    }
                    throw new RetryableException(we.Message, we);
                }
            }
            return null;
        }

        public override StorageResult Commit()
        {
            if (IsCommitStream)
            {
                if (((HCPClient)(((HCPSystem)this.System).Client)).OpenParam.IsHaveSecondaryHost)
                {
                    return HCPCommit();
                }
                else
                {
                    return base.Commit();
                }
            }
            else
            {
                StorageResult rs = new StorageResult();
                if (IsCommited)
                {
                    throw new Exception("this stream can not be submit more than once");
                }
                IsCommited = true;
                rs.PdId = System.SystemID;
                IsCommited = true;
                if (secRequest == null)
                {
                    CommitWithPrimaryHost();
                }
                else
                {
                    CommitWithPrimaryTwoHosts();
                }
                return rs;
            }

        }

        private void DeleteDirtyData()
        {
            StorageInfo info = ((HCPSystem)System).PreproccessStorageInfo(Info);
            int maxSleepTime = 60 * 15 * 1000;
            try
            {
                Logger.Info("Begin deleting dirty data, high name: {0}, low name: {1}.", info.HighName, info.LowName);
                HCPSystem hcpSystem = (HCPSystem)System;
                Dictionary<string, string> writerHeaders = hcpSystem.Client.OpenStreamWriteModeHeaders;
                bool delete = false;
                while (!delete && maxSleepTime > 0)
                {
                    try
                    {
                        int sleepTime = 30000;
                        Thread.Sleep(sleepTime);
                        maxSleepTime -= sleepTime;
                        bool isExsit = (bool)((HCPClient)hcpSystem.Client).CheckObject(info.HighName, info.LowName);
                        if (isExsit)
                        {
                            delete = (bool)((HCPClient)hcpSystem.Client).DeleteObject(info.HighName, info.LowName, new Dictionary<string, string>(), writerHeaders, true);
                        }
                        else
                        {
                            delete = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("Failed to delete dirty data. We will retry it. Error details: {0}", ex.Message);
                        continue;
                    }
                }
                if (delete)
                {
                    Logger.Info("Delete dirty data succeed. High name: {0}. Low name: {1}.", info.HighName, info.LowName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to delete unused file, high name: {0}, low name: {1}. Error details: {2}.",
                    info.HighName, info.LowName, ex);
            }
            if (maxSleepTime <= 0)
            {
                throw new Exception("Can't delete dirty data URL:" + info.HighName + "\\" + info.LowName);
            }
        }

        private bool WriteDateToServer(ref HttpWebRequest uploadRequest)
        {
            int retryCount = 0;
            while (retryCount < OpenParam.MaxRetryCount)
            {
                try
                {
                    byte[] buffer = new byte[64 * 1024];
                    using (Stream upStream = uploadRequest.GetRequestStream())
                    {
                        InnerStream.Position = 0;
                        int realLen = 0;
                        while (true)
                        {
                            realLen = InnerStream.Read(buffer, 0, buffer.Length);
                            if (realLen > 0)
                            {
                                upStream.Write(buffer, 0, realLen);
                                //while (File.Exists("C:\\sleep_write.sleep"))
                                //{
                                //    logger.Info("sleep_write*******************************");
                                //    Thread.Sleep(3000);
                                //}
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occurred while writing data to server. We will retry it, retry count: {0}. Error details: {1}.",
                        retryCount, ex);
                    retryCount++;
                    try
                    {
                        uploadRequest.Abort();
                    }
                    catch (Exception t)
                    {
                        Logger.Warn(t.ToString());
                    }
                    DeleteDirtyData();
                    uploadRequest = ReCreateRequest();
                    continue;
                }
            }
            throw new Exception("too many retries when write data");
        }

        private HttpWebRequest ReCreateRequest()
        {
            if (this.OpenParam.FlushDNS)
            {
                Logger.Debug("Begin flushing DNS");
                //while (File.Exists("C:\\sleep_before_flush_dns.sleep"))
                //{
                //    logger.Info("sleep_before_flush_dns*******************************");
                //    Thread.Sleep(3000);
                //}
                DnsUtil.FlushMyCache();
                //while (File.Exists("C:\\sleep_after_flush_dns.sleep"))
                //{
                //    logger.Info("sleep_after_flush_dns*******************************");
                //    Thread.Sleep(3000);
                //}
                Logger.Debug("Finished flushing DNS");
            }
            Logger.Debug("Begin sleeping for delay retry");
            Thread.Sleep(OpenParam.RetryInterval);
            Logger.Debug("Finished sleeping for delay retry");
            HCPClient client = (HCPClient)((HCPSystem)System).Client;
            StorageInfo info = ((HCPSystem)System).PreproccessStorageInfo(Info);
            string fullURL = client.BuildObjectAbsoluteURL(info.HighName, info.LowName);
            HttpWebRequest request = client.HttpClient.CreateRequestPut(fullURL, null);
            client.HttpClient.CombiningRequestWithHeaders(request, OpenParam.WriteHeaders);
            return request;
        }

        private bool DoExcute(HttpWebRequest commitRequest)
        {
            WriteDateToServer(ref commitRequest);
            int retryCount = 0;
            while (retryCount < OpenParam.MaxRetryCount)
            {
                try
                {
                    using (HttpWebResponse resp = commitRequest.GetResponse() as HttpWebResponse)
                    {
                        if (resp != null)
                        {
                            Logger.Debug("HttpStatusCode=" + Convert.ToInt32(resp.StatusCode) + " "
                                            + resp.StatusCode.ToString() + "; RequestUri=" + commitRequest.RequestUri);
                        }
                        if (resp == null || (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK))
                        {
                            throw new Exception("Create object failed. object : " + commitRequest.RequestUri);
                        }
                        else
                        {
                            if (Info != null)
                            {
                                System.AddMetadata(Info);
                            }
                            return true;
                        }
                    }

                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                    {
                        Logger.Error("An connection error occurred while uploading file. We will retry uploading, retry count {0}. Error details: {1}.", retryCount, we);
                        retryCount++;
                        if (this.OpenParam.FlushDNS)
                        {
                            Logger.Debug("Begin flushing DNS");
                            DnsUtil.FlushMyCache();
                            Logger.Debug("Finished flushing DNS");
                        }
                        Logger.Debug("Begin sleeping for delay retry");
                        Thread.Sleep(OpenParam.RetryInterval);
                        Logger.Debug("Finished sleeping for delay retry");
                        continue;
                    }
                    else if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (HttpWebResponse response = we.Response as HttpWebResponse)
                        {
                            if (((HCPClient)((HCPSystem)System).Client).IsServerIntertalError(response.StatusCode))
                            {
                                Logger.Error("An server internal error occurred while uploading file. We will retry uploading, retry count {0}. Error details: {1}.", retryCount, we);
                                retryCount++;
                                if (this.OpenParam.FlushDNS)
                                {
                                    Logger.Debug("Begin flushing DNS");
                                    DnsUtil.FlushMyCache();
                                    Logger.Debug("Finished flushing DNS");
                                }
                                Logger.Debug("Begin sleeping for delay retry");
                                Thread.Sleep(OpenParam.RetryInterval);
                                Logger.Debug("Finished sleeping for delay retry");
                                continue;
                            }
                            else
                            {
                                Logger.Error("An error occurred while uploading file. An this error is a ProtocolError not a retry able error, throw it out. Error details: {0}.", we);
                                throw new UnknownException(commitRequest.RequestUri.ToString(), we);
                            }
                        }
                    }
                    else
                    {
                        Logger.Error("An error occurred while uploading file. An this error is a ProtocolError not a retry able error, throw it out. Error details: {0}.", we);
                        throw new UnknownException(commitRequest.RequestUri.ToString(), we);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occurred while uploading file. It is not a webException error. We will retry uploading, retry count {0}. Error details: {1}.",
                        retryCount, ex);
                    retryCount++;
                    if (this.OpenParam.FlushDNS)
                    {
                        Logger.Debug("Begin flushing DNS");
                        DnsUtil.FlushMyCache();
                        Logger.Debug("Finished flushing DNS");
                    }
                    Logger.Debug("Begin sleeping for delay retry");
                    Thread.Sleep(OpenParam.RetryInterval);
                    Logger.Debug("Finished sleeping for delay retry");
                    continue;
                }
            }
            throw new Exception("too many retries when commit");

        }

    }
}
