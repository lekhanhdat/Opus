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

using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#SetContainerKeyName()", MessageId = "containername")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#OpenFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "occured")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#FileExists(AvePoint.Media.Storage.StorageInfo)", MessageId = "occured")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#DeleteFile(AvePoint.Media.Storage.StorageInfo)", MessageId = "occured")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#DeleteDirectory(AvePoint.Media.Storage.StorageInfo)", MessageId = "occured")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Common.CloudSystem.#.ctor(System.String,AvePoint.Media.Storage.AbstractXSystem)", MessageId = "containername")]
namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Resources.CloudCommonI18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/4/11",
    "rongbiao.sun@avepoint.com",
    "yanxin.fu@avepoint.com",
     new String[] { CodeReviewConstants.CHECK_LIST_ID_BL_1,
                    CodeReviewConstants.CHECK_LIST_ID_FA_4},
    "ADO-28237",
     true)]
    #endregion
    abstract class CloudSystem : AbstractXSystem
    {
        private Hashtable containers = new Hashtable();
        private String systemLocationKeyName = "containername";
        private StorageLogger logger = StorageLogger.GetInstance(typeof(CloudSystem));
        public AbstractRESTOprationExecutor Client { get; set; }
        protected Hashtable Containers { get { return this.containers; } set { this.containers = value; } }
        protected String SystemLocationKeyName { get { return systemLocationKeyName; } set { this.systemLocationKeyName = value; } }

        protected virtual void SetContainerKeyName()
        {
            SystemLocationKeyName = "containername";
        }

        private static Boolean CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        static CloudSystem()
        {
            try
            {
                try
                {
                    ServicePointManager.DefaultConnectionLimit = 1024;
                    ServicePointManager.ServerCertificateValidationCallback =
                        new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.Message);
                    try
                    {
                        ServicePointManager.DefaultConnectionLimit = 254;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning(e.Message);
                        ServicePointManager.DefaultConnectionLimit = 64;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        public CloudSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            SetContainerKeyName();
            this.IsRetry = true;
            this.createIfNotExist = false;
        }

        protected void SetSecurityProtocol(String url)
        {
            var endpoint = new Uri(url);
            if (endpoint.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                var protocolType = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                var donetMajorVersion = Environment.Version.Major;
                //Because of the default protocol is TLS1.0 for .net framework4.5, so in order to use TLS1.2, we need to set securityprotocol explicitly.
                if (donetMajorVersion == 4)
                {
                    protocolType = (SecurityProtocolType)4080;
                }
                ServicePointManager.SecurityProtocol = protocolType;
                logger.Info("The major version of the dotnet framework is {0}, the security protocols of the media service is {1}.", donetMajorVersion, protocolType.ToString());
            }
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            logger.Debug("Cloud system type: " + this.GetType().Name + " Enter into Open()");
            SetSystemDescription();
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            return new StorageOpenValidResult();
        }

        public StorageInfo PreproccessStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            Client.Data_Version = storageInfo.DataVersion;
            if (storageInfo.DataVersion == Data_Version.DocAve5)
            {
                info = D5PreproccessStorageInfo(storageInfo);
            }
            else
            {
                if (String.IsNullOrEmpty(info.LowName))
                {
                    info.LowName = String.Empty;
                }
                if (!String.IsNullOrEmpty(SystemLocation))
                {
                    info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                    info.HighName = SystemLocation;
                }
                if (info.LowName.Equals("\\"))
                {
                    info.LowName = "";
                }
                info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');
            }
            return info;
        }

        public abstract StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo);

        public abstract StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo);

        public StorageInfo Preproccess2DirectoryStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            if (storageInfo.DataVersion == Data_Version.DocAve5)
            {
                info = D5Preproccess2DirectoryStorageInfo(storageInfo);
            }
            else
            {
                if (String.IsNullOrEmpty(info.LowName))
                {
                    info.LowName = "\\";
                }
                if (!String.IsNullOrEmpty(SystemLocation))
                {
                    info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
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
            String tempStr = info.HighName;
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            CheckState(infoTemp.HighName);
            String fullURL = Client.BuildObjectAbsoluteURL(infoTemp.HighName, infoTemp.LowName);
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    Dictionary<String, String> writerHeaders = Client.OpenStreamWriteModeHeaders;
                    writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
                    writerHeaders["Content-Length"] = infoTemp.Length.ToString();
                    AddMetadata(info, writerHeaders);
                    XStream writeStream = (HttpUploadStream)Client.Invoke("OpenObjectForWrite", new object[] { fullURL, writerHeaders });
                    writeStream.System = this;
                    writeStream.Info = info;
                    writeStream.MaxRetryCount = Client.CloudOpenParam.MaxRetryCount;
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
                        var readerHeaders = Client.OpenStreamReadModeHeaders;
                        if (infoTemp.Length > 0)
                        {
                            readerHeaders["Range"] = "bytes=" + infoTemp.Offset + "-" + (infoTemp.Length + infoTemp.Offset);
                        }
                        readStream = (XStream)Client.Invoke("OpenObjectForRead", new object[] { fullURL, readerHeaders });
                        readStream.System = this;
                        if (infoTemp.Length <= 0)
                        {
                            info.Length = ((HttpDownloadStream)readStream).InnerLength;
                        }
                        readStream.Info = info;
                        readStream.MaxRetryCount = Client.CloudOpenParam.MaxRetryCount;
                        if (info.DataVersion == Data_Version.DocAve5)
                        {
                            info.HighName = tempStr;
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Failed to open the file. File Path: {0}. Details {1}.", fullURL, e);
                        throw;
                    }
                    return readStream;
                default:
                    throw new NotSupportedException("Unsupported file mode " + fileMode);
            }
        }

        public virtual Boolean AddMetadata(StorageInfo storageInfo, Dictionary<String, String> writerHeaders)
        {
            switch (Client.CloudOpenParam.CustomizedMetaMode)
            {
                case CustomizedMode.Close:
                    return true;
                case CustomizedMode.SupportAll:
                    foreach (KeyValuePair<String, String> entry in Client.CloudOpenParam.CustomizedMetaData)
                    {
                        if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                        {
                            writerHeaders[entry.Key] = entry.Value;
                        }
                    }
                    foreach (KeyValuePair<String, String> entry in storageInfo.MetaInfos)
                    {
                        if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                        {
                            writerHeaders[entry.Key] = entry.Value;
                        }
                    }
                    break;
                case CustomizedMode.DocAveOnly:
                    foreach (KeyValuePair<String, String> entry in storageInfo.MetaInfos)
                    {
                        if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                        {
                            writerHeaders[entry.Key] = entry.Value;
                        }
                    }
                    break;
                case CustomizedMode.CustomizedOnly:
                    foreach (KeyValuePair<String, String> entry in Client.CloudOpenParam.CustomizedMetaData)
                    {
                        if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                        {
                            writerHeaders[entry.Key] = entry.Value;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException("Unknown Customized Mode " + Client.CloudOpenParam.CustomizedMetaMode);
            }
            return base.AddMetadata(storageInfo);
        }

        public override Boolean DirectoryExists(StorageInfo info)
        {
            if (info.DataVersion == Data_Version.DocAve5)
            {
                //由于D5的cloud中没有containername的概念，所以当highname=String.Empty时，return true；
                if (String.IsNullOrEmpty(info.HighName))
                {
                    return true;
                }
                StorageInfo d5Info = D5Preproccess2DirectoryStorageInfo(info);
                return (Boolean)Client.Invoke("CheckObject", new object[] { d5Info.HighName, d5Info.LowName });
            }
            else
            {
                StorageInfo d6Info = Preproccess2DirectoryStorageInfo(info);
                return (Boolean)Client.Invoke("CheckObject", new object[] { d6Info.HighName, d6Info.LowName });
            }
        }

        public override Boolean FileExists(StorageInfo info)
        {
            String tempIndex = info.HighName;
            StorageInfo storageInfo = PreproccessStorageInfo(info);
            Boolean result = false;
            try
            {
                CheckState(storageInfo.HighName);
                result = (Boolean)Client.Invoke("CheckObject", new object[] { storageInfo.HighName, storageInfo.LowName });
            }
            catch (Exception e)
            {
                logger.Error("An error occured while check object : {0}, object name : {1}. Details : {2}.", info.HighName, info.LowName, e);
                throw;
            }
            if (info.DataVersion == Data_Version.DocAve5)
            {
                info.HighName = tempIndex;
            }
            return result;
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            String tempHighName = fileInfo.HighName;
            CloudFileInfo xfileInfo = null;
            try
            {
                xfileInfo = new CloudFileInfo(fileInfo.HighName, fileInfo.LowName);
                StorageInfo info = PreproccessStorageInfo(fileInfo);
                CheckState(info.HighName);
                Client.Data_Version = fileInfo.DataVersion;
                xfileInfo = (CloudFileInfo)Client.Invoke("GetObjectInfo", new object[] { info.HighName, info.LowName });
                xfileInfo.HighName = fileInfo.HighName;
                xfileInfo.LowName = fileInfo.LowName;
            }
            catch (Exception e)
            {
                if (e is PathNotFoundException || e.InnerException is PathNotFoundException)
                {
                    logger.Debug("The object not exist, container name : {0}, object name : {1}.", fileInfo.HighName, fileInfo.LowName);
                    return null;
                }
                else
                {
                    logger.Error("An error occured while check object container name : {0}, object name : {1}. Details : {2}.", fileInfo.HighName, fileInfo.LowName, e);
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
            var result = new StorageDeleteResult();
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            CheckState(infoTemp.HighName);
            try
            {
                Dictionary<String, String> queryObjectsParams = Client.ListObjectQueryParams;
                if (!String.IsNullOrEmpty(infoTemp.LowName) && !"/".Equals(infoTemp.LowName, StringComparison.CurrentCultureIgnoreCase))
                {
                    queryObjectsParams.Add("prefix", infoTemp.LowName);
                }
                Dictionary<String, String> queryObjectsHeaders = Client.ListObjectHeaders;
                String baseURL = Client.BuildURLWithOutQueryParams(infoTemp.HighName);
                ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { baseURL, queryObjectsParams, queryObjectsParams });
                String responseString = responseInfo.ResponseXml;
                String[] subObjects = String.IsNullOrEmpty(responseString) ? null : responseString.Split(new char[] { '\r', '\n' });
                if (subObjects != null)
                {
                    var deleteHeaders = Client.Headers;
                    foreach (String subObject in subObjects)
                    {
                        String fullURL = Client.BuildObjectAbsoluteURL(infoTemp.HighName, subObject);
                        Client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<String, String>(), deleteHeaders });
                    }
                }
                result.IsDeleted = true;
                result.DeletedFileSize = 0;
            }
            catch (Exception e)
            {
                logger.Error("An error occured while deleting container, container name : {0}. Details : {1}.", infoTemp.HighName, e);
                throw;
            }
            Deletion = true;
            return result;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            CheckState();
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            try
            {
                result.DeletedFileSize = ((CloudFileInfo)Client.Invoke("GetObjectInfo", new object[] { infoTemp.HighName, infoTemp.LowName })).FileSize;
                result.IsDeleted = (Boolean)Client.Invoke("DeleteObject", new object[] { infoTemp.HighName, infoTemp.LowName, false });
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                result.DeletedFileSize = -1;
                result.IsDeleted = true;
            }
            catch (Exception e)
            {
                logger.Error("An error occured while deleting object, container name : {0}, object name : {1}. Details : {2}.", infoTemp.HighName, infoTemp.LowName, e);
                throw;
            }
            Deletion = true;
            return result;
        }

        protected virtual Boolean CheckContainer(String containerName)
        {
            logger.Debug("Begin check container : {0}.", containerName);
            Boolean isOK = (Boolean)Client.Invoke("CheckContainer", new object[] { containerName });
            if (!containers.ContainsKey(containerName) && isOK)
            {
                containers.Add(containerName, true);
            }
            return isOK;
        }

        private void ContainerCheckorCreate(String containerName)
        {
            if (!containers.ContainsKey(containerName))
            {
                Boolean isOK = CheckContainer(containerName);
                if (!isOK)
                {
                    Client.Invoke("CreateContainer", new object[] { containerName });
                }
            }
        }

        public override StorageOpenValidResult Validate()
        {
            var result = new StorageOpenValidResult();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            try
            {
                result = Client.GetPermissions();
                if (!CheckContainer(this.SystemLocation))
                {
                    if (CreateIfNotExists)
                    {
                        Client.Invoke("CreateContainer", new object[] { SystemLocation });
                    }
                    else
                    {
                        logger.Info("The root folder {0} don't exist.", SystemLocation);
                        result.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_RootFolderNotExist", AbstractXSystem.Culture);
                        result.SystemHealth = XSystemHealth.Unaccessable;
                        result.IsDeleteAble = false;
                        result.IsWriteAble = false;
                        result.IsReadAble = false;
                        return result;
                    }
                }
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(XriObject.VIM, this.XriString, new CheckFreeSpace(Client.GetUserAccountInfo));
                result.SystemHealth = XSystemHealth.AvailableAndNotFull;
                result.TotalUsedSpace = spaceInfo.TotalUsedSpace;
                result.TotalSpace = spaceInfo.TotalSpace;
                result.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
                result.IsDeleteAble = true;
                result.IsReadAble = true;
                result.IsWriteAble = true;
                innerTotalFreeSpace = result.TotalFreeSpace;
                if (ValidateIsFull())
                {
                    result.SystemHealth = XSystemHealth.Available;
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when validate cloud system. Details : {0}.", ex);
                if (ex is AuthenticationFailedException)
                {
                    result.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Authentication_failed", AbstractXSystem.Culture);
                }
                else
                {
                    result.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Test_failed", AbstractXSystem.Culture);
                    PrintEventLog(ex);
                }
                result.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = result.SystemHealth;
            }
            SuperValidate();
            return result;
        }

        private void PrintEventLog(Exception ex)
        {
            UInt16 eventTaskCategory = 0;
            ContextValues.Storage.StorageType eventTaskMessage = ContextValues.Storage.StorageType.Cloud;
            switch (this.GetType().Name)
            {
                case "AmazonSystem":
                    eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Amazon_S3;
                    eventTaskMessage = ContextValues.Storage.StorageType.Amazon;
                    break;
                case "AtmosSystem":
                    if (XriObject.VIM.Equals("atmos_vim"))
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
            EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, eventTaskMessage, ex);
            this.logger.Log(EventSources.DocAveStorageAPIService, eventTaskCategory, verifyFailedEventMessage);
        }

        #region SuperValidate
        private void SuperValidate()
        {
            try
            {
                var fileFullPath = Path.Combine(ExecutorContext.BinDirectory, @"EnableSuperValidate.config");
                if (File.Exists(fileFullPath))
                {
                    var testInfor = new StorageInfo("TestDirectory", "File.test") { DataVersion = Data_Version.DocAve6 };
                    logger.Info("the basic validate test finished, start full test. first writing methods(OpenStream,CommitStream).");
                    TestWrite(testInfor);
                    logger.Info("the writing test finished, start test reading methods(OpenStream, Read).");
                    TestRead(testInfor);
                    logger.Info("the reading test finished, start test listing methods(ListDirectories, ListFiles).");
                    TestList(testInfor);
                    logger.Info("the listing test finished, start test deleting methods(DeleteDirectory, DeleteFile).");
                    TestDelete(testInfor);
                    logger.Info("if there is no error above, all test successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Super validate meet an error, the detail is {0}.", ex.ToString());
            }
        }

        private void TestDelete(StorageInfo testInfor)
        {
            try
            {
                if (this.FileExists(testInfor))
                {
                    this.DeleteFile(testInfor);
                }
                testInfor.LowName = String.Empty;
                if (this.DirectoryExists(testInfor))
                {
                    this.DeleteDirectory(testInfor);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("delete test failed, reason {0}.", ex.ToString());
            }
        }

        private void TestList(StorageInfo testInfor)
        {
            try
            {
                var directories = this.ListDirectories(new StorageInfo());
                testInfor.LowName = String.Empty;
                var files = this.ListFiles(testInfor);
                if (directories.Count != 1 || files.Count != 1)
                {
                    throw new Exception("list count is not correct");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("list test failed, reason {0}.", ex.ToString());
            }
        }

        private void TestRead(StorageInfo testInfor)
        {
            try
            {
                byte[] buffer = new byte[4];
                using (var readSteam = this.OpenStream(testInfor, FileMode.Open))
                {
                    readSteam.Read(buffer, 0, buffer.Length);
                }
                String result = Encoding.ASCII.GetString(buffer);
                if (!result.Equals("Test", StringComparison.Ordinal))
                {
                    throw new Exception("read result is not correct");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("read test failed, reason {0}.", ex.ToString());
            }
        }

        private void TestWrite(StorageInfo testInfor)
        {
            try
            {
                byte[] buffer = new byte[4];
                int readLen = 0;
                byte[] tempbuffer = Encoding.ASCII.GetBytes("Test");
                testInfor.Length = tempbuffer.Length;
                using (MemoryStream stream = new MemoryStream(tempbuffer))
                {
                    using (var writeSteam = this.OpenStream(testInfor, FileMode.Create))
                    {
                        while ((readLen = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writeSteam.Write(buffer, 0, readLen);
                        }
                        writeSteam.Commit(true);
                    }
                }
                if (!this.FileExists(testInfor))
                {
                    throw new Exception("test file didn't write successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Write test failed, reason {0}.", ex.ToString());
            }
        }

        #endregion

        public override void Close()
        {
            if (Client != null)
            {
                Client.Close();
            }
        }

        public void CheckState(String containerName)
        {
            CheckState();
            ContainerCheckorCreate(containerName);
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            CloudDirectoryInfo directoryInfo = null;
            String name = "/".Equals(dirInfoTemp.LowName) ? "" : dirInfoTemp.LowName;
            String fullURL = Client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, name);
            var headers = Client.OpenDirectoryWriteModeHeaders;
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                    headers["Content-Type"] = "DOCAVE/directory".ToLower(CultureInfo.InvariantCulture);
                    headers["Content-Length"] = "0";
                    Client.Invoke("CreateObjectWithNoContent", new object[] { fullURL, headers });
                    directoryInfo = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    directoryInfo.IsExists = true;
                    break;
                case FileMode.Open:
                    var exists = this.DirectoryExists(dirInfo);
                    if (!exists)
                    {
                        return null;
                    }
                    directoryInfo = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    directoryInfo.IsExists = exists;
                    break;
                case FileMode.Append:
                case FileMode.Truncate:
                default:
                    throw new UnsupportedXException("Unsupported File Mode : " + mode.ToString());
            }
            return directoryInfo;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            var directories = new List<XDirectoryInfo>();
            var objects = Client.ListObject(dirInfoTemp.HighName, dirInfoTemp.LowName);
            foreach (String obj in objects)
            {
                var lowName = String.IsNullOrEmpty(obj) ? obj : obj.TrimEnd('/');
                var info = new CloudDirectoryInfo(dirInfo.HighName, lowName);
                info.IsExists = true;
                directories.Add(info);
            }
            return directories;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            var result = new StorageListResult();
            var dirInfoTemp = PreproccessStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            var children = Client.ListObject(dirInfoTemp.HighName, dirInfoTemp.LowName);
            var dirNames = new List<String>();
            var fileNames = new List<String>();
            foreach (String child in children)
            {
                if (child.Contains("/"))
                {
                    String folder = child.Substring(0, child.IndexOf("/", StringComparison.OrdinalIgnoreCase));
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
            foreach (String dir in dirNames)
            {
                result.SubDirs.Add(new CloudDirectoryInfo(dir));
            }
            foreach (String file in fileNames)
            {
                result.Files.Add(new CloudFileInfo(dirInfoTemp.HighName, file));
            }
            return result;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState();
            var result = new StorageCopyResult();
            XStream sourceStream = null;
            XStream destStream = null;
            try
            {
                if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName) }))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName) }) && !isOverWrite)
                        {
                            result.IsCopyed = true;
                            return result;
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

                    result.IsCopyed = true;
                }
                else
                {
                    result.Message = "source file is not exist";
                    result.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.IsCopyed = false;
                logger.Error("Copy file failed, details : " + e);
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
            return result;
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            var moveResult = new StorageMoveResult();
            try
            {
                StorageCopyResult copyResult = CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                if (copyResult.IsCopyed)
                {
                    Client.DeleteObject(SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName), false);
                }
                else
                {
                    moveResult.IsMoved = false;
                    moveResult.Message = copyResult.Message;
                }
            }
            catch (Exception ex)
            {
                moveResult.IsMoved = false;
                moveResult.Message = ex.Message;
            }
            return moveResult;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {

            StorageMoveResult moveResult = new StorageMoveResult();
            try
            {
                StorageCopyResult copyResult = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
                if (copyResult.IsCopyed)
                {
                    DeleteDirectory(sourceDirInfo);
                }
                else
                {
                    moveResult.IsMoved = false;
                    moveResult.Message = copyResult.Message;
                }
            }
            catch (Exception ex)
            {
                moveResult.IsMoved = false;
                moveResult.Message = ex.Message;
            }
            return moveResult;
        }

        private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, Boolean isOverWrite)
        {
            StorageCopyResult result = new StorageCopyResult();
            try
            {
                if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName) }))
                {
                    //先copy子文件
                    StorageListResult listRS = ListSubDirectoriesAndFiles(sourceFolderInfo);
                    foreach (XFileInfo file in listRS.Files)
                    {
                        StorageInfo sourceFileInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                        StorageInfo targetFileInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
                        if (!CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
                        {
                            result.IsCopyed = false;
                            return result;
                        }
                    }
                    //遍历文件夹的子文件夹
                    foreach (XDirectoryInfo directory in listRS.SubDirs)
                    {
                        StorageInfo sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
                        StorageInfo targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
                        if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                        {
                            result.IsCopyed = false;
                            return result;
                        }
                    }
                    result.IsCopyed = true;
                }
            }
            catch (Exception ex)
            {
                result.IsCopyed = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private bool IsEnableProxy()
        {
            bool result = false;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Proxy_Setting) && Boolean.Parse(XriObject.Params[XRIParameterKeys.Proxy_Setting]))
            {
                result = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.AMAZON_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.AMAZON_PROXY_SETTING]))
            {
                result = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ATMOS_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.ATMOS_PROXY_SETTING]))
            {
                result = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ATT_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.ATT_PROXY_SETTING]))
            {
                result = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.AZURE_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.AZURE_PROXY_SETTING]))
            {
                result = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.RACKSPACE_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.RACKSPACE_PROXY_SETTING]))
            {
                result = true;
            }
            return result;
        }

        protected void ConstructCloudOpenParameter(String xriStr, CloudOpenParameter openParam)
        {
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
            {
                String customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                openParam.CustomizedMetaMode = (CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedMetaKey))
            {
                openParam.CustomizedMetaData = ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
            }
            openParam.RetryInterval = this.RetryInterval;
            openParam.MaxRetryCount = this.MaxRetryCount;
            openParam.NeedRetry = this.IsRetry;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_USERNAME_KEY))
            {
                openParam.UserName = XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_PASSWORD_KEY))
            {
                openParam.Password = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.MODULE_TYPE_KEY))
            {
                openParam.ModuleType = int.Parse(XriObject.Params[XRIParameterKeys.MODULE_TYPE_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY))
            {
                openParam.NeedRetry = Boolean.Parse(XriObject.Params[XRIParameterKeys.IS_RETRY]);
            }
            else
            {
                openParam.NeedRetry = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                createIfNotExist = Boolean.Parse(XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            if (IsEnableProxy())
            {
                if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_IP) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_PORT))
                {
                    openParam.ProxyIp = XriObject.Params[XRIParameterKeys.PROXY_IP];
                    openParam.ProxyPort = int.Parse(XriObject.Params[XRIParameterKeys.PROXY_PORT]);
                    if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_USERNAME) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXYPASSWORD))
                    {
                        openParam.ProxyUsername = XriObject.Params[XRIParameterKeys.PROXY_USERNAME];
                        openParam.ProxyPassword = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.PROXYPASSWORD]);
                    }
                }
            }
            this.SystemLocation = this.XriObject[SystemLocationKeyName];
            if (String.IsNullOrEmpty(SystemLocation))
            {
                SystemLocation = Client.GetDocAveDefaultContainer();
            }
            openParam.SystemLocation = SystemLocation;
        }
    }
}
