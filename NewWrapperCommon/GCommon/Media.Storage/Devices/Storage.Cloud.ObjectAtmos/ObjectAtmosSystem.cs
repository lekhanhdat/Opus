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
using AvePoint.GCommon;
using AvePoint.Media.Storage.Util;
using System.Net;
using System.IO;
using System.Threading;
using System.Globalization;
using AvePoint.Media.Storage.Resources.AtmosI18N;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    class ObjectAtmosSystem : AbstractXSystem
    {
        AveLogger logger = AveLogger.GetInstance(typeof(ObjectAtmosSystem));
        ObjectAtmosOpenParameter OpenParameter { get; set; }
        DateTime deadTime;
        List<String> deadHostList;
        private ulong totalFreeSpace;
        private ulong totalSpace;
        private ulong totalUsedSpace;
        delegate T RetryDelegate<T>();
        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }
        object lockObj = new object();
        List<string> AvailableHosts = new List<string>();

        static ObjectAtmosSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;
            ServicePointManager.ServerCertificateValidationCallback =
                        new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public override string Type
        {
            get
            {
                return "ObjectAtmosSystem";
            }
        }

        public override ulong TotalFreeSpace
        {
            get
            {
                return this.totalFreeSpace;
            }
        }

        public override ulong TotalSpace
        {
            get
            {
                return this.totalSpace;
            }
        }

        public override ulong TotalUsedSpace
        {
            get
            {
                return this.totalUsedSpace;
            }
        }

        public ObjectAtmosSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new ObjectAtmosOpenParameter();
            this.deadHostList = new List<string>();
            this.Open();
        }

        #region Method

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_CHECKSUM_UPLOAD))
            {
                this.OpenParameter.EnableChecksumForCreate = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_CHECKSUM_DOWNLOAD))
            {
                this.OpenParameter.VerifyChecksumAtRead = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_USERNAME_KEY))
            {
                this.OpenParameter.FullTokenId = XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY];
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_PASSWORD_KEY))
            {
                this.OpenParameter.SharedSecret = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY]);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.AccessPoinyKey))
            {
                this.OpenParameter.AccessPoints = XriObject.Params[XRIParameterKeys.AccessPoinyKey];
                this.AvailableHosts = this.OpenParameter.AccessPoints.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrEmpty(this.SystemID))
            {
                this.OpenParameter.PhysicalId = this.SystemID;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_INTERVAL))
            {
                int retryInterval = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_INTERVAL]);
                if (retryInterval >= int.MaxValue)
                {
                    throw new Exception(string.Format("unknown retryInterval value {0}.", retryInterval));
                }
                OpenParameter.RetryInterval = retryInterval;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_COUNT))
            {
                int retryCount = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_COUNT]);
                if (retryCount < 1 || retryCount >= int.MaxValue)
                {
                    throw new Exception(string.Format("unknown retryCount value {0}.", retryCount));
                }
                OpenParameter.MaxRetryCount = retryCount;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_VALIDATE_KEY))
            {
                OpenParameter.IsValidate = true;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_FAILOVER_KEY))
            {
                OpenParameter.ValidateFailoverInterval = long.Parse(XriObject.Params[XRIParameterKeys.OBJECT_ATMOS_FAILOVER_KEY]);
                logger.Debug("Add ValidateFailoverInterval , value is {0}", OpenParameter.ValidateFailoverInterval);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
            {
                try
                {
                    string customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                    OpenParameter.CustomizedMetaMode = (CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    throw new Exception("unknown custom metadata mode value");
                }
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedMetaKey))
            {
                try
                {
                    OpenParameter.CustomizedMetaData = ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    throw new Exception("unknown custom metadata format");
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.OBJECT_ATMOS_PROXY_SETTING]))
            {
                if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_IP) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_PORT))
                {
                    var ProxyIp = XriObject.Params[XRIParameterKeys.PROXY_IP];
                    var ProxyPort = int.Parse(XriObject.Params[XRIParameterKeys.PROXY_PORT]);
                    OpenParameter.Proxy = new WebProxy(ProxyIp, ProxyPort);
                    if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_USERNAME) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXYPASSWORD))
                    {
                        OpenParameter.Proxy.Credentials = new NetworkCredential(XriObject.Params[XRIParameterKeys.PROXY_USERNAME], SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.PROXYPASSWORD]));
                    }
                }
            }
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            SetSystemDescription();
            return new StorageOpenValidResult();
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Atmos Object Storage Server";
        }

        public void RemoveDeadHost(string host)
        {
            lock (lockObj)
            {
                logger.Warn("The host {0} is unavailable now", host);
                this.AvailableHosts.Remove(host);
                this.deadHostList.Add(host);
                deadTime = DateTime.Now;
            }
        }

        private void ValidateDeadClient()
        {
            if (DateTime.Now.Subtract(deadTime).Minutes > OpenParameter.ValidateFailoverInterval && this.deadHostList.Count > 0)
            {
                logger.Debug("There are {0} host unavailable now", this.deadHostList.Count);
                for (int i = deadHostList.Count - 1; i >= 0; i--)
                {
                    ObjectAtmosClient client = new ObjectAtmosClient(deadHostList[i], this.OpenParameter, this);
                    StorageInfo info = new StorageInfo();
                    Byte[] b = new Byte[1];
                    HttpWebRequest request = client.GetUploadRequest(info);
                    info.Length = b.Length;
                    try
                    {
                        using (XStream stream = new ObjectAtmosStream(client, info, request, this))
                        {
                            (stream as ObjectAtmosStream).InitStream(FileMode.Create);
                            b[0] = 0x00;
                            stream.Write(b, 0, b.Length);
                            stream.Commit(false);
                            info.ObjectId = (stream as ObjectAtmosStream).objectId;
                        }
                        client.DeleteObject(info);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("The client {0} is still unavailable , and error : {1}", deadHostList[i], e);
                        continue;
                    }
                    this.AvailableHosts.Add(deadHostList[i]);
                    this.deadHostList.RemoveAt(i);
                }
                deadTime = DateTime.Now;
            }
        }

        private ObjectAtmosClient GetAvailableClient()
        {
            ValidateDeadClient();
            if (this.AvailableHosts.Count > 0)
            {
                return new ObjectAtmosClient(this.AvailableHosts[0], this.OpenParameter, this);
            }
            else
            {
                throw new Exception("no available host in " + this.OpenParameter.AccessPoints);
            }
        }

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            if (this.OpenParameter.EnableChecksumForCreate)
            {
                using (var sha = new SHA1Util())
                {
                    info.checksum = sha.GetChecksumStringForBlob(commitStream);
                }
            }
            return Retry<StorageResult>(delegate()
            {
                CheckState();
                while (true)
                {
                    try
                    {
                        long writeLength = 0;
                        StorageResult rs = new StorageResult();
                        commitStream.Position = 0;
                        StorageInfo infoClone = info.Clone();
                        logger.Debug("commit file {0} length {1}, stream length {2}, SHA1 value is {3}", infoClone.HighPlusLowName, infoClone.Length, commitStream.Length, infoClone.checksum);
                        byte[] buffer = new byte[64 * 1024];
                        int readLen = 0;
                        using (XStream stream = OpenStream(infoClone, FileMode.Create))
                        {
                            stream.IsCommitStream = true;
                            while ((readLen = commitStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                stream.Write(buffer, 0, readLen);
                                writeLength += readLen;
                            }
                            rs = stream.Commit(infoClone.IsClosing);
                            rs.URI = stream.GetURI();
                            rs.IsCommited = true;
                        }
                        logger.Debug("write length for file {0} : {1}", info.LowName, writeLength);
                        this.Written = true;
                        return rs;
                    }
                    catch (Exception e)
                    {
                        logger.Error("commit file {0} failed:{1}", info.HighPlusLowName, e);
                        if (info.CurrentRetryCount < this.MaxRetryCount && this.IsRetry)
                        {
                            logger.Info("this is a retry able exception, retry it, retry count:{0}, max retry:{1}", info.CurrentRetryCount, this.MaxRetryCount);
                            info.CurrentRetryCount++;
                            Thread.Sleep(this.RetryInterval);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            });
        }

        private StorageOpenValidResult ValidateForFailover()
        {
            return Retry<StorageOpenValidResult>(delegate()
            {
                StorageOpenValidResult rs = new StorageOpenValidResult();
                StorageInfo info = new StorageInfo();
                Byte[] b = new Byte[1];
                rs = GetAvailableClient().HasPermissions();
                this.totalFreeSpace = rs.TotalFreeSpace;
                this.totalUsedSpace = rs.TotalUsedSpace;
                this.totalSpace = rs.TotalSpace;

                if (ValidateIsFull())
                {
                    rs.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    info.Length = b.Length;
                    using (XStream stream = OpenStream(info, FileMode.Create))
                    {
                        b[0] = 0x00;
                        stream.Write(b, 0, b.Length);
                        stream.Commit(false);
                        info.ObjectId = (stream as ObjectAtmosStream).objectId;
                    }
                    rs.IsWriteAble = true;
                    try
                    {
                        GetAvailableClient().DeleteObject(info);
                        rs.IsDeleteAble = true;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("cannot delete the temp file , ID : {0} and error : {1}", info.ObjectId, e);
                    }
                    rs.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }
                this.SystemHealth = rs.SystemHealth;
                return rs;
            });
        }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            StorageOpenValidResult rs = new StorageOpenValidResult();
            try
            {
                rs = ValidateForFailover();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred when validate Object Atmos system. {0}", ex);
                rs.SystemHealth = XSystemHealth.AuthenticationFailed;
                rs.Message = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Test_failed", AbstractXSystem.Culture);
                this.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            return rs;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            return Retry<XStream>(delegate()
            {
                CheckState();
                ObjectAtmosStream stream = null;
                var checksum = string.Empty;
                ObjectAtmosClient client = GetAvailableClient();
                if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
                {
                    this.Written = true;
                    HttpWebRequest request = client.GetUploadRequest(info);
                    stream = new ObjectAtmosStream(client, info, request, this);
                }
                else
                {
                    HttpWebResponse response = client.OpenObject(info);
                    checksum = response.Headers.Get("x-emc-wschecksum");
                    stream = new ObjectAtmosStream(client, info, response, this);
                }
                stream.InitStream(fileMode);
                if (fileMode == FileMode.Open && this.OpenParameter.VerifyChecksumAtRead && !string.IsNullOrEmpty(checksum))
                {
                    var responseChecksum = ObjectAtmosUtil.GetChecksumStringForDownload(checksum);
                    //var blobChecksum = ObjectAtmosUtil.GetChecksumStringForBlob(stream);
                    //if (responseChecksum == blobChecksum)
                    //{
                    //    stream.Position = 0;
                    //}
                    //else
                    //{
                    //    throw new RetryableException("The content SHA-1 value is mismatch from server.");
                    //}
                    info.checksum = responseChecksum;
                }
                return stream;
            });
        }

        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            return Retry<bool>(delegate()
            {
                return GetAvailableClient().CheckObject(info);
            });
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            return Retry<XFileInfo>(delegate()
            {
                return GetAvailableClient().GetObjectInfo(fileInfo);
            });
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult rs = new StorageDeleteResult();
            try
            {
                foreach (string objId in info.ObjectIds)
                {
                    if (!string.IsNullOrEmpty(objId))
                    {
                        try
                        {
                            DeleteUploadedFile(new StorageInfo() { ObjectId = objId }, rs);
                        }
                        catch (PathNotFoundException)
                        {
                            logger.Info("file already not exists, fileID=" + objId);
                        }
                    }
                }
                rs.IsDeleted = true;
            }
            catch (Exception e)
            {
                logger.Error("delete object failed, id:" + info.ObjectId + ", msg:" + e.Message);
                throw;
            }
            Deletion = true;
            return rs;
        }

        private void DeleteUploadedFile(StorageInfo info, StorageDeleteResult rs)
        {
            if (FileExists(info))
            {
                rs.DeletedFileSize += OpenFile(info).FileSize;
                GetAvailableClient().DeleteObject(info);
            }
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            return Retry<StorageCopyResult>(delegate()
            {
                ObjectAtmosClient client = GetAvailableClient();
                CheckState();
                StorageCopyResult rs = new StorageCopyResult();
                XStream sourceStream = null;
                XStream destStream = null;
                try
                {
                    if ((bool)client.CheckObject(sourceFileInfo))
                    {
                        if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                        {
                            if ((bool)client.CheckObject(targetFileInfo) && !isOverWrite)
                            {
                                rs.IsCopyed = true;
                                return rs;
                            }
                        }
                        sourceStream = OpenStream(sourceFileInfo, FileMode.Open);
                        targetFileInfo.Length = (sourceStream as ObjectAtmosStream).response.ContentLength;
                        destStream = OpenStream(targetFileInfo, FileMode.Create);
                        byte[] buffer = new byte[64 * 1024];
                        while (true)
                        {
                            int readLength = sourceStream.Read(buffer, 0, buffer.Length);
                            if (readLength <= 0)
                            {
                                break;
                            }
                            else
                            {
                                destStream.Write(buffer, 0, readLength);
                            }
                        }

                        rs.IsCopyed = true;
                    }
                    else
                    {
                        rs.Message = "source file is not exist";
                        rs.IsCopyed = false;
                    }
                }
                catch (Exception e)
                {
                    rs.Message = e.Message;
                    rs.IsCopyed = false;
                    logger.Error("copy file failed:" + e.Message);
                }
                finally
                {
                    if (destStream != null)
                    {
                        destStream.Close();
                    }
                    if (sourceStream != null)
                    {
                        sourceStream.Close();
                    }
                }
                return rs;
            });

        }

        public override void Close()
        {
        }

        private T Retry<T>(RetryDelegate<T> del)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (Exception ex)
                {
                    if ((ex is RetryableException) && this.AvailableHosts.Count > 0)
                    {
                        logger.Error("find a new host and try to use it:" + this.AvailableHosts[0]);
                        Thread.Sleep(OpenParameter.RetryInterval);
                        continue;
                    }
                    else if (ex is WebException)
                    {
                        WebException webEx = ex as WebException;
                        if (webEx.Status == WebExceptionStatus.ProtocolError)
                        {
                            HttpWebResponse resp = webEx.Response as HttpWebResponse;
                            if (resp.StatusCode == HttpStatusCode.NotFound)
                            {
                                throw new PathNotFoundException(ex.Message, ex);
                            }
                            else if (resp.StatusCode == HttpStatusCode.InternalServerError || resp.StatusCode == HttpStatusCode.RequestTimeout || resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                RemoveDeadHost(GetAvailableClient().Endpoint);
                                continue;
                            }
                            else
                            {
                                string body = string.Empty;
                                using (Stream respStream = resp.GetResponseStream())
                                {
                                    using (StreamReader sr = new StreamReader(respStream))
                                    {
                                        body = sr.ReadToEnd();
                                    }
                                }
                                logger.Error("execute request failed, msg:{0}, response body:{1}:", ex.Message, body, ex);
                                throw;
                            }
                        }
                        else if (webEx.Status == WebExceptionStatus.ConnectionClosed || webEx.Status == WebExceptionStatus.ConnectFailure || webEx.Status == WebExceptionStatus.NameResolutionFailure || webEx.Status == WebExceptionStatus.Timeout)
                        {
                            RemoveDeadHost(GetAvailableClient().Endpoint);
                            continue;
                        }
                        else
                        {
                            logger.Error("execute request failed:" + ex.Message, ex);
                            throw;
                        }
                    }
                    else
                    {
                        logger.Error("operation failed and there are no other available endpoint:" + ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        #endregion Methond
        
        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            throw new NotSupportedException();
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            throw new NotSupportedException();
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}