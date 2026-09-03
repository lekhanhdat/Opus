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




using Storage;

namespace AvePoint.Media.ClassicStorage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.Media.ClassicStorage;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Config;
    using AvePoint.Media.ClassicStorage.Util;
    using System.IO;
    using System.Threading;
    using System.Collections;
    using AvePoint.Media.ClassicStorage.Cloud;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;
    using System.Text.RegularExpressions;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.ClassicStorage.Resources.CloudCommonI18N;
    using System.Diagnostics;
    using System.Globalization;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.StorageApi;
    using AvePoint.GCommon.Utility;
    #endregion

    public abstract class CloudSystem : AbstractXSystem
    {
        #region -- Private Members --
        private AveLogger logger = AveLogger.GetInstance(typeof(CloudSystem));
        protected AveLogger Logger
        {
            get
            {
                return this.logger;
            }
            set
            {
                this.logger = value;
            }
        }
        public AbstractRESTOprationExecutor client { get; set; }
        protected string currentXset { get; set; }
        private bool createIfNotExists;
        public override bool CreateIfNotExists { get { return this.createIfNotExists; } set { this.createIfNotExists = value; } }
        private string cloudType = "Cloud";
        protected string CloudType
        {
            get
            {
                return this.cloudType;
            }
            set
            {
                this.cloudType = value;
            }
        }
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
        private List<XStream> activedStreams = new List<XStream>();
        private object activedStreamLocker = new object();
        private string systemLocationKeyName = "containerName".ToLower(CultureInfo.InvariantCulture);
        protected string SystemLocationKeyName { get { return systemLocationKeyName; } set { this.systemLocationKeyName = value; } }
        private Hashtable containers = new Hashtable();
        public Hashtable Containers
        {
            get { return this.containers; }
            set { this.containers = value; }
        }
        #endregion

        protected void Add2ActivedStream(XStream stream)
        {
            lock (activedStreamLocker)
            {
                activedStreams.Add(stream);
            }
        }

        public void RemoveFromActivedStream(XStream stream)
        {
            lock (activedStreamLocker)
            {
                activedStreams.Remove(stream);
            }
        }

        protected virtual void SetContainerKeyName()
        {
            SystemLocationKeyName = "containerName".ToLower(CultureInfo.InvariantCulture);
        }

        static CloudSystem()
        {
            try
            {
                try
                {
                    ServicePointManager.DefaultConnectionLimit = 512;
                    //ServicePointManager.ServerCertificateValidationCallback =
                    //    new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);

                }
                catch (Exception ex)
                {
                    try
                    {
                        ServicePointManager.DefaultConnectionLimit = 254;
                    }
                    catch
                    {
                        Trace.TraceWarning(ex.Message);
                        ServicePointManager.DefaultConnectionLimit = 64;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        public CloudSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            //this.SupportedFileType = (global::Storage.FileBlockType)FileBlockType.SingleInstanceLevel_Block;
            SetContainerKeyName();
            this.IsRetry = true;
            switch (this.GetType().Name)
            {
                case "AmazonSystem":
                    cloudType = "Amazon";
                    eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Amazon_S3;
                    eventTaskMessage = ContextValues.Storage.StorageType.Amazon;
                    break;
                case "AtmosSystem":
                    if (XriObject.VIM.Equals("atmos_vim"))
                    {
                        cloudType = "Atmos";
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_EMC_Atmos;
                        eventTaskMessage = ContextValues.Storage.StorageType.Atmos;
                    }
                    else
                    {
                        cloudType = "At&t";
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_ATT_Synaptic;
                        eventTaskMessage = ContextValues.Storage.StorageType.ATT;
                    }
                    break;
                case "AzureSystem":
                    cloudType = "Azure";
                    eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Windows_Azure;
                    eventTaskMessage = ContextValues.Storage.StorageType.Azure;
                    break;
                case "RackspaceSystem":
                    cloudType = "Rackspace";
                    eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Rackspace;
                    eventTaskMessage = ContextValues.Storage.StorageType.Rackspace;
                    break;
                case "HCPSystem":
                    cloudType = "HCP";
                    eventTaskCategory = EventCategorys.DocAveStorageAPIService.HDS_HCP;
                    eventTaskMessage = ContextValues.Storage.StorageType.HCP;
                    break;
                default:
                    break;
            }
        }

        #region 常用方法

        public override StorageOpenValidResult Open()
        {
            base.Open();
            //logger.Debug("CloudType: " + this.cloudType + " Enter into Open()");
            SetSystemDescription();
            this.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.AvailableAndNotFull;
            return new StorageOpenValidResult();
        }

        public StorageInfo PreproccessStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            client.Data_Version = Data_Version.DocAve6;

            if (string.IsNullOrEmpty(info.LowName))
            {
                info.LowName = string.Empty;
            }
            if (!string.IsNullOrEmpty(SystemLocation))
            {
                info.LowName = SecurityUtils.SafeCombinePath(info.HighName, info.LowName);
                info.HighName = SystemLocation;
            }
            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');
            return info;
        }

        public virtual StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public virtual StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public StorageInfo Preproccess2DirectoryStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            if (storageInfo.DataVersion == Data_Version.DocAve5)
            {
                info = D5Preproccess2DirectoryStorageInfo(storageInfo);
            }
            else
            {
                if (string.IsNullOrEmpty(info.LowName))
                {
                    info.LowName = "\\";
                }
                if (!string.IsNullOrEmpty(SystemLocation))
                {
                    if (!string.IsNullOrEmpty(info.HighName))
                    {
                        info.LowName = SecurityUtils.SafeCombinePath(info.HighName, info.LowName);
                    }
                    info.HighName = SystemLocation;
                }
                if (info.LowName.Equals("\\"))
                {
                    info.LowName = "";
                }
                info.LowName = info.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/";
            }
            return info;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            string tempStr = info.HighName;
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            CheckState(infoTemp.HighName);
            string fullURL = client.BuildObjectAbsoluteURL(infoTemp.HighName, infoTemp.LowName);
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    Dictionary<string, string> writerHeaders = client.OpenStreamWriteModeHeaders;
                    writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
                    writerHeaders["Content-Length"] = infoTemp.Length.ToString();
                    AddMetadata(info, writerHeaders);
                    XStream writeStream = (HttpUploadStream)client.Invoke("OpenObjectForWrite", new object[] { fullURL, writerHeaders });
                    writeStream.System = this;
                    writeStream.Info = info;
                    writeStream.MaxRetryCount = client.CloudOpenParam.MaxRetryCount;
                    Add2ActivedStream(writeStream);
                    this.Written = true;
                    if (info.DataVersion == Data_Version.DocAve5)
                    {
                        info.HighName = tempStr;
                    }
                    return writeStream;
                case FileMode.Open:
                    XStream readStream = null;
                    try
                    {
                        Dictionary<string, string> readerHeaders = client.OpenStreamReadModeHeaders;
                        if (infoTemp.Length > 0)
                        {
                            string range = "bytes=" + infoTemp.Offset + "-" + (infoTemp.Length + infoTemp.Offset);
                            readerHeaders["Range"] = range;
                        }
                        readStream = (XStream)client.Invoke("OpenObjectForRead", new object[] { fullURL, readerHeaders });
                        readStream.System = this;
                        if (infoTemp.Length <= 0)
                        {
                            info.Length = ((HttpDownloadStream)readStream).InnerLength;
                        }
                        readStream.Info = info;
                        readStream.MaxRetryCount = client.CloudOpenParam.MaxRetryCount;
                        Add2ActivedStream(readStream);
                        if (info.DataVersion == Data_Version.DocAve5)
                        {
                            info.HighName = tempStr;
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Opened the data failed, path: {0}.", fullURL);
                        logger.Error(e.Message, e);
                        throw;
                    }
                    return readStream;
                default:
                    throw new NotImplementedException("U" + "NSUPPORTTED".ToLower(CultureInfo.InvariantCulture) + " File Mode " + fileMode);
            }

        }

        public bool AddMetadata(StorageInfo storageInfo, Dictionary<string, string> writerHeaders)
        {
            if (client.CloudOpenParam.CustomizedMetaMode.Equals(Storage.CustomizedMode.Close))
            {
                return true;
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(Storage.CustomizedMode.CustomizedOnly))
            {
                foreach (KeyValuePair<string, string> entry in client.CloudOpenParam.CustomizedMetaData)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(Storage.CustomizedMode.DocAveOnly))
            {
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(Storage.CustomizedMode.SupportAll))
            {
                foreach (KeyValuePair<string, string> entry in client.CloudOpenParam.CustomizedMetaData)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else
            {
                throw new Exception("unKnown Customized Mode");
            }
            return base.AddMetadata(storageInfo);
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            if (info.DataVersion == Data_Version.DocAve5)
            {
                //由于D5的cloud中没有containername的概念，所以当highname=string.Empty时，return true；
                if (string.IsNullOrEmpty(info.HighName))
                {
                    return true;
                }
                StorageInfo d5Info = D5Preproccess2DirectoryStorageInfo(info);
                return (bool)client.Invoke("CheckObject", new object[] { d5Info.HighName, d5Info.LowName });
            }
            else
            {
                StorageInfo d6Info = Preproccess2DirectoryStorageInfo(info);
                return (bool)client.Invoke("CheckObject", new object[] { d6Info.HighName, d6Info.LowName });
            }
        }

        public override bool FileExists(StorageInfo info)
        {
            while (true)
            {
                try
                {
                    string tempIndex = info.HighName;
                    StorageInfo storageInfo = PreproccessStorageInfo(info);
                    bool rs = false;
                    CheckState(storageInfo.HighName);
                    rs = (bool)client.Invoke("CheckObject", new object[] { storageInfo.HighName, storageInfo.LowName });
                    if (info.DataVersion == Data_Version.DocAve5)
                    {
                        info.HighName = tempIndex;
                    }
                    return rs;
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("error when check object : {0}, object name : {1}.", info.HighName, info.LowName), e);
                    if (info.CurrentRetryCount < this.MaxRetryCount && this.IsRetry)
                    {
                        logger.Info("this is a retry able exception, retry it, retry count:{0}, max retry:{1}", info.CurrentRetryCount, this.MaxRetryCount);
                        info.CurrentRetryCount++;
                        Thread.Sleep(this.RetryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            string tempHighName = fileInfo.HighName;
            CloudFileInfo xfileInfo = null;
            try
            {
                xfileInfo = new CloudFileInfo(fileInfo.HighName, fileInfo.LowName);
                xfileInfo.HighName = fileInfo.HighName;
                xfileInfo.LowName = fileInfo.LowName;
                StorageInfo info = PreproccessStorageInfo(fileInfo);
                CheckState(info.HighName);
                client.Data_Version = fileInfo.DataVersion;
                xfileInfo = (CloudFileInfo)client.Invoke("GetObjectInfo", new object[] { info.HighName, info.LowName });
                xfileInfo.HighName = fileInfo.HighName;
                xfileInfo.LowName = fileInfo.LowName;
            }
            catch (Exception e)
            {
                if (e is PathNotFoundException || e.InnerException is PathNotFoundException)
                {
                    logger.Debug(string.Format("object not exist container name : {0}, object name : {1}.", fileInfo.HighName, fileInfo.LowName));
                    return null;
                }
                else
                {
                    logger.Error(string.Format("error when check object container name : {0}, object name : {1}.", fileInfo.HighName, fileInfo.LowName), e);
                    throw;
                }
            }
            if (fileInfo.DataVersion == Data_Version.DocAve5)
            {
                fileInfo.HighName = tempHighName;
            }
            return xfileInfo;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            CheckState(infoTemp.HighName);
            try
            {
                Dictionary<string, string> queryObjectsParams = client.ListObjectQueryParams;
                if (!string.IsNullOrEmpty(infoTemp.LowName) && !"/".Equals(infoTemp.LowName, StringComparison.CurrentCultureIgnoreCase))
                {
                    queryObjectsParams.Add("prefix", infoTemp.LowName);
                }
                Dictionary<string, string> queryObjectsHeaders = client.ListObjectHeaders;
                string baseURL = client.BuildURLWithOutQueryParams(infoTemp.HighName);
                ResponseInfo responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { baseURL, queryObjectsParams, queryObjectsParams });
                string responseString = responseInfo.ResponseXml;
                string[] subObjects = string.IsNullOrEmpty(responseString) ? null : responseString.Split(new char[] { '\r', '\n' });
                if (subObjects != null)
                {
                    Dictionary<string, string> deleteHeaders = client.Headers;
                    string fullURL;
                    foreach (string subObject in subObjects)
                    {
                        fullURL = client.BuildObjectAbsoluteURL(infoTemp.HighName, subObject);
                        client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<string, string>(), deleteHeaders });
                    }
                }
                rs.IsDeleted = true;
                rs.DeletedFileSize = 0;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("error when delete container, container name : {0}", infoTemp.HighName), e);
                throw;
            }
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            CheckState();
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            try
            {
                var tempObjectInfo = (CloudFileInfo)client.Invoke("GetObjectInfo", new object[] { infoTemp.HighName, infoTemp.LowName });
                rs.DeletedFileSize = tempObjectInfo.FileSize;
                rs.IsDeleted = (bool)client.Invoke("DeleteObject", new object[] { infoTemp.HighName, infoTemp.LowName, false });
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                Deletion = true;
                return rs;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("error when delete object, container name : {0}, object name : {1}.", infoTemp.HighName, infoTemp.LowName), e);
                throw;
            }
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        protected virtual bool CheckContainer(string containerName)
        {
            bool isOK = (bool)client.Invoke("CheckContainer", new object[] { containerName });
            if (!containers.ContainsKey(containerName) && isOK)
            {
                containers.Add(containerName, true);
            }
            return isOK;
        }

        public virtual void ContainerCheckorCreate(string containerName)
        {
            if (!containers.ContainsKey(containerName))
            {
                bool isOK = CheckContainer(containerName);
                if (!isOK)
                {
                    client.Invoke("CreateContainer", new object[] { containerName });
                }
            }
        }

        public override StorageOpenValidResult Validate()
        {
            StorageOpenValidResult rs = new StorageOpenValidResult();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            try
            {
                rs = client.HasPermissions();
                if (!CheckContainer(this.SystemLocation))
                {
                    if (createIfNotExists)
                    {
                        client.Invoke("CreateContainer", new object[] { SystemLocation });
                    }
                    else
                    {
                        logger.Info("the root folder don't exist:" + SystemLocation);
                        rs.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.Unaccessable;
                        rs.IsDeleteAble = false;
                        rs.IsWriteAble = false;
                        rs.IsReadAble = false;
                        return rs;
                    }
                }
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(XriObject.VIM, this.XriString, CheckFreeSpace);
                rs.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.AvailableAndNotFull;
                rs.TotalUsedSpace = spaceInfo.TotalUsedSpace;
                rs.TotalSpace = spaceInfo.TotalSpace;
                rs.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
                rs.IsDeleteAble = true;
                rs.IsReadAble = true;
                rs.IsWriteAble = true;
                totalFreeSpace = rs.TotalFreeSpace;
                if (ValidateIsFull())
                {
                    rs.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.Available;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred when validate cloud system. {0}", ex);

                if (ex is AuthenticationFailedException)
                {
                    rs.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Authentication_failed", Culture);
                }
                else if (ex is BucketInOtherRegionException)
                {
                    rs.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_BucketInOtherRegion", Culture);
                }
                else
                {
                    rs.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Test_failed", Culture);
                }
                rs.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = rs.SystemHealth;
            }
            return rs;
        }

        public virtual SpaceInfo CheckFreeSpace()
        {
            return client.GetUserAccountInfo();
        }

        public override void Close()
        {
            //if (this.Streams != null && this.Streams.Count > 0)
            //{
            //    foreach (XStream stream in this.Streams)
            //    {
            //        stream.Close();
            //    }
            //    this.Streams.Clear();
            //}
            if (client != null)
            {
                client.Close();
            }
            //lock (activedStreamLocker)
            //{
            //    foreach (XStream stream in activedStreams)
            //    {
            //        stream.ClosedUnmoral();
            //    }
            //}
        }

       /* private string GetDetailErrorMsg(Stream response)
        {
            if (response != null)
            {
                using (StreamReader sr = new StreamReader(response))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                return string.Empty;
            }
        }*/

        public void CheckState(string containerName)
        {
            CheckState();
            ContainerCheckorCreate(containerName);
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, System.IO.FileMode mode)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            CloudDirectoryInfo dir = null;
            string name = "/".Equals(dirInfoTemp.LowName) ? "" : dirInfoTemp.LowName;
            string fullURL = client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, name);
            Dictionary<string, string> headers = client.OpenDirectoryWriteModeHeaders;
            switch (mode)
            {
                case System.IO.FileMode.Create:
                case System.IO.FileMode.CreateNew:
                case System.IO.FileMode.OpenOrCreate:
                    headers["Content-Type"] = "DOCAVE/directory".ToLower(CultureInfo.InvariantCulture);
                    headers["Content-Length"] = "0";
                    client.Invoke("CreateObjectWithNoContent", new object[] { fullURL, headers });
                    dir = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    dir.IsExists = true;
                    break;
                case System.IO.FileMode.Open:

                    bool exists = (bool)client.Invoke("CheckObject", new object[] { fullURL, new Dictionary<string, string>(), headers });
                    if (!exists)
                    {
                        return null;
                    }
                    dir = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    dir.IsExists = exists;
                    break;

                case System.IO.FileMode.Append:
                case System.IO.FileMode.Truncate:
                default:
                    break;
                    throw new UnsupportedXException("Unsupported File Mode : " + mode.ToString());
            }
            return dir;
        }

        private string TrimEndChar(string source, char c)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            while (source.EndsWith(c + "", StringComparison.OrdinalIgnoreCase))
            {
                source = source.TrimEnd(c);
            }
            return source;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            List<XDirectoryInfo> xfs = new List<XDirectoryInfo>();
            List<string> xstreamList = client.ListObject(dirInfoTemp.HighName, dirInfoTemp.LowName);
            foreach (string name in xstreamList)
            {
                CloudDirectoryInfo info = new CloudDirectoryInfo(dirInfo.HighName, TrimEndChar(name, '/'));
                info.IsExists = true;
                xfs.Add(info);
            }
            return xfs;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            StorageListResult rs = new StorageListResult();
            StorageInfo dirInfoTemp = PreproccessStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            List<string> children = client.ListObject(dirInfoTemp.HighName, dirInfoTemp.LowName);
            List<string> dirNames = new List<string>();
            List<string> fileNames = new List<string>();
            foreach (string child in children)
            {
                if (child.Contains("/"))
                {
                    string folder = child.Substring(0, child.IndexOf("/", StringComparison.OrdinalIgnoreCase));
                    if (!dirNames.Contains(folder))
                    {
                        dirNames.Add(folder);
                    }
                }
                else
                {
                    if (!fileNames.Contains(child))
                    {
                        fileNames.Add(child);
                    }
                }
            }

            foreach (string dir in dirNames)
            {
                rs.SubDirs.Add(new CloudDirectoryInfo(dir));
            }

            foreach (string file in fileNames)
            {
                rs.Files.Add(new CloudFileInfo(dirInfoTemp.HighName, file, 0));
            }
            return rs;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState();
            StorageCopyResult rs = new StorageCopyResult();
            XStream sourceStream = null;
            XStream destStream = null;
            try
            {
                if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName) }))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName) }) && !isOverWrite)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                    }
                    sourceStream = OpenStream(sourceFileInfo, FileMode.Open);
                    targetFileInfo.Length = (sourceStream as HttpDownloadStream).InnerLength;
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
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            StorageMoveResult moveRS = new StorageMoveResult();
            try
            {
                StorageCopyResult copyRS = CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                if (copyRS.IsCopyed)
                {
                    client.DeleteObject(SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName), false);
                }
                else
                {
                    moveRS.IsMoved = false;
                    moveRS.Message = copyRS.Message;
                }
            }
            catch (System.Exception ex)
            {
                moveRS.IsMoved = false;
                moveRS.Message = ex.Message;
            }
            return moveRS;
        }

        //public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        //{

        //    StorageMoveResult moveRS = new StorageMoveResult();
        //    try
        //    {
        //        StorageCopyResult copyRS = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
        //        if (copyRS.IsCopyed)
        //        {
        //            DeleteDirectory(sourceDirInfo);
        //        }
        //        else
        //        {
        //            moveRS.IsMoved = false;
        //            moveRS.Message = copyRS.Message;
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        moveRS.IsMoved = false;
        //        moveRS.Message = ex.Message;
        //    }
        //    return moveRS;
        //}

        //private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, bool isOverWrite)
        //{
        //    StorageCopyResult rs = new StorageCopyResult();
        //    try
        //    {
        //        //if (client.CheckObject(SystemLocation, PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName)))
        //        if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName) }))
        //        {
        //            //先copy子文件
        //            StorageListResult listRS = ListSubDirectoriesAndFiles(sourceFolderInfo);
        //            foreach (XFileInfo file in listRS.Files)
        //            {
        //                StorageInfo sourceFileInfo = XConvert.FromNames(StorageApi.PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
        //                StorageInfo targetFileInfo = XConvert.FromNames(StorageApi.PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
        //                if (!CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
        //                {
        //                    rs.IsCopyed = false;
        //                    return rs;
        //                }
        //            }
        //            //遍历文件夹的子文件夹
        //            foreach (XDirectoryInfo directory in listRS.SubDirs)
        //            {
        //                StorageInfo sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
        //                StorageInfo targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
        //                if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
        //                {
        //                    rs.IsCopyed = false;
        //                    return rs;
        //                }
        //            }
        //            rs.IsCopyed = true;
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        rs.IsCopyed = false;
        //        rs.Message = ex.Message;
        //    }
        //    return rs;
        //}

        #endregion

        #region -- Virtual Members --
        protected string RemoveFirst(string str, string patValue)
        {
            if (!str.StartsWith(patValue, StringComparison.CurrentCulture))
            {
                return str;
            }

            if (str.IndexOf(patValue, StringComparison.CurrentCulture) < 0)
            {
                return str;
            }

            return str.Remove(0, patValue.Length);
        }
        protected void ConstructCloudOpenParameter(string xriStr, CloudOpenParameter openParam)
        {
            //XriObject = XRI.ValueOf(xriStr);
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
            {
                try
                {
                    string customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                    openParam.CustomizedMetaMode = (global::Storage.CustomizedMode)(CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
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
                    openParam.CustomizedMetaData = OptionUtil.ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    throw new Exception("unknown custom metadata format");
                }
            }

            openParam.RetryInterval = this.RetryInterval;
            openParam.MaxRetryCount = this.MaxRetryCount;
            openParam.NeedRetry = this.IsRetry;

            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_USERNAME_KEY))
            {
                openParam.UserName = XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_PASSWORD_KEY_WITHOUT_ENCRYPT))
            {
                openParam.Password = XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY_WITHOUT_ENCRYPT];
            }
            else if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_PASSWORD_KEY))
            {
                openParam.Password = SecretUtil.Decrypt(XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY]);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.MODULE_TYPE_KEY))
            {
                openParam.ModuleType = int.Parse(XriObject.Params[XRIParameterKeys.MODULE_TYPE_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY))
            {
                openParam.NeedRetry = bool.Parse(XriObject.Params[XRIParameterKeys.IS_RETRY]);
            }
            else
            {
                openParam.NeedRetry = true;
            }
            //if (XriObject.Params.ContainsKey(XRIParameterKeys.IsValidate)) //Becasue too many corrupted records in the DB, cancle this logical, damn it!
            //{
            //    logger.Debug("This operation is from GUI.");
            //    openParam.NeedRetry = !bool.Parse(XriObject.Params[XRIParameterKeys.IsValidate]);//The retry logic should be false if the request from GUI
            //}
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                this.CreateIfNotExists = bool.Parse(XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            //base.Open();
            this.SystemLocation = this.XriObject[SystemLocationKeyName];
            if (string.IsNullOrEmpty(SystemLocation))
            {
                SystemLocation = client.GetDocAveDefaultContainer();
            }
            openParam.SystemLocation = SystemLocation;
        }

        protected virtual void ConstructCloudOpenParameter(string xriStr)
        {
        }

        #endregion

    }
}
