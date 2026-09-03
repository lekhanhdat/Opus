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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Resources.CloudCommonI18N;
    using AvePoint.Media.Storage.Util;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/5/23",
    "shouqiang.liu@avepoint.com",
    "rongbiao.sun@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_HC_1 },
     null,
     true)]
    #endregion

    class HCPSystem : CloudSystem
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(HCPSystem));
        private HCPOpenParameter openParamter;

        private string initMode;

        public override bool IsFull
        {
            get
            {
                if (this.SpaceThresholdUnit == SpaceThresholdUnit.MB)
                {
                    if (this.innerTotalFreeSpace <= this.SpaceThreshold * 1024 * 1024)
                    {
                        return true;
                    }
                }
                if (this.SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
                {
                    if (this.innerTotalFreeSpace * 100.0 / this.innerTotalSpace <= this.SpaceThreshold)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public HCPSystem(string xri, string initMode, AvePoint.Media.Storage.AbstractXSystem parentSystem)
            : base(xri, parentSystem)
        {
            this.initMode = initMode;
            openParamter = new HCPOpenParameter();
            ConstructCloudOpenParameter(xri, openParamter);
            SystemHealth = XSystemHealth.Unknown;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.Open();
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "HDS Hitachi Content Platform, Namespace Address:" + openParamter.PrimaryHost + ", Root Folder:" + openParamter.Library;
            List<string> keys = new List<string>();
            keys.Add(this.openParamter.PrimaryHost);
            keys.Add(this.openParamter.Library);
            keys.Add(this.openParamter.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.openParamter.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        protected void ConstructCloudOpenParameter(string xriStr, HCPOpenParameter openParamter)
        {
            try
            {
                openParamter.PrimaryHost = XriObject.Params[XRIParameterKeys.KEY_HOST];
                if (XriObject.Params.ContainsKey(XRIParameterKeys.KEY_SECONDHOST))
                {
                    openParamter.SecondaryHost = XriObject.Params[XRIParameterKeys.KEY_SECONDHOST];
                    openParamter.IsHaveSecondaryHost = true;
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.KEY_USERNAME))
                {
                    openParamter.UserName = XriObject.Params[XRIParameterKeys.KEY_USERNAME];
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.KEY_PASSWORD))
                {
                    openParamter.Password = XriObject.Params[XRIParameterKeys.KEY_PASSWORD];
                }
                this.Type = "HCPSystem";
                openParamter.Library = XriObject.Params[XRIParameterKeys.KEY_LIBRARY];
                this.SystemLocation = openParamter.Library;
            }
            catch (Exception t)
            {
                logger.Error("{0}", t);
                throw new InvalidXRIException("Invalid XRI String :" + xriStr, t);
            }

        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            StorageInfo storageInfo = PreproccessStorageInfo(info);
            CheckState(storageInfo.HighName);
            string fullURL = Client.BuildObjectAbsoluteURL(storageInfo.HighName, storageInfo.LowName);
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    Dictionary<string, string> writerHeaders = Client.OpenStreamWriteModeHeaders;
                    bool isExsit = (bool)((HCPClient)Client).Invoke("CheckObject", new object[] { storageInfo.HighName, storageInfo.LowName, false });
                    if (isExsit)
                    {
                        ((HCPClient)Client).Invoke("DeleteObject", new object[] { storageInfo.HighName, storageInfo.LowName, new Dictionary<string, string>(), writerHeaders, true });
                    }
                    writerHeaders["Content-Type"] = "DOCAVE/DATA".ToLower(CultureInfo.InvariantCulture);
                    writerHeaders[HCPConsts.KEY_Object_Size] = storageInfo.Length.ToString();
                    writerHeaders["Content-Length"] = storageInfo.Length.ToString();
                    XStream writeStream = (XStream)((HCPClient)Client).Invoke("OpenObjectForWrite", new object[] { storageInfo.HighName, storageInfo.LowName, writerHeaders });
                    writeStream.System = this;
                    writeStream.Info = info;
                    this.Written = true;
                    return writeStream;
                case FileMode.Open:
                    XStream readStream = null;
                    try
                    {
                        Dictionary<string, string> readerHeaders = Client.OpenStreamReadModeHeaders;
                        if (storageInfo.Length > 0)
                        {
                            string range = "bytes=" + storageInfo.Offset + "-" + (storageInfo.Length + storageInfo.Offset);
                            readerHeaders["Range"] = range;
                        }
                        readStream = (XStream)((HCPClient)Client).Invoke("OpenObjectForRead", new object[] { storageInfo.HighName, storageInfo.LowName, readerHeaders });
                        readStream.System = this;
                        readStream.Info = info;
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to open the file. File Path: {0}.", fullURL);
                        logger.Error("OpenStream Error: {0}.", e);
                        throw;
                    }
                    return readStream;

                default:
                    throw new NotSupportedException("Unsupported File Mode " + fileMode);
            }
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            StorageInfo storageDirInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(storageDirInfo.HighName);
            string lowName = "/".Equals(storageDirInfo.LowName) ? "" : storageDirInfo.LowName;
            string urlWithoutQueryParms = Client.BuildObjectAbsoluteURL(storageDirInfo.HighName, lowName);
            Dictionary<string, string> headers = Client.ListDirectoryHeaders;
            headers[HCPConsts.KEY_X_HCP_TYPE] = HCPConsts.KEY_VAL_Directory;
            ResponseInfo responseInfo = (ResponseInfo)((HCPClient)Client).Invoke("ListObjects", new object[] { urlWithoutQueryParms, new Dictionary<string, string>(), headers });
            string responseXmlString = responseInfo.ResponseXml;

            CloudDirectoryInfo dir;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            CloudFileInfo file;
            List<XFileInfo> files = new List<XFileInfo>();

            List<XPathNavigator> navs = Client.FirstStepAnalyzeXML(responseXmlString, "directory/entry", HCPConsts.XML_Selected_Namespace);

            string xmlFileName = "UTF".ToLower(CultureInfo.InvariantCulture) + "8" + "Name";
            string xmlFileType = "type";
            string xmlFileTypeValue = "object";
            if (((HCPClient)Client).IsDefaultNamespace)
            {
                xmlFileName = "name";
                xmlFileType = "fileType";
                xmlFileTypeValue = "file";
            }

            string type;
            string name;
            long size;
            foreach (XPathNavigator nav in navs)
            {
                type = null;
                name = null;
                size = 0;
                name = nav.GetAttribute(xmlFileName, "");
                type = nav.GetAttribute(xmlFileType, "");
                if (xmlFileTypeValue.Equals(type, StringComparison.CurrentCultureIgnoreCase))
                {
                    size = int.Parse(nav.GetAttribute("size", ""));
                    file = new CloudFileInfo(dirInfo.HighPlusLowName, name, size);
                    files.Add(file);
                }
                else
                {
                    if (((HCPClient)Client).IsDefaultNamespace && name.Equals("."))
                    {
                        continue;
                    }
                    dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, name);
                    dirs.Add(dir);
                }
            }
            StorageListResult results = new StorageListResult();
            results.SubDirs = dirs;
            results.Files = files;
            return results;

        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            CloudDirectoryInfo dir = null;
            StorageInfo storageDirInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(storageDirInfo.HighName);
            //string name = "/".Equals(storageDirInfo.LowName) ? "" : storageDirInfo.LowName;
            Dictionary<string, string> headers = Client.OpenDirectoryWriteModeHeaders;
            switch (mode)
            {
                case System.IO.FileMode.Create:
                case System.IO.FileMode.CreateNew:
                case System.IO.FileMode.OpenOrCreate:
                    ((HCPClient)Client).Invoke("CreateContainer", new object[] { storageDirInfo.HighPlusLowName });
                    dir = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    dir.IsExists = true;
                    break;
                case System.IO.FileMode.Open:
                    bool exists = (bool)((HCPClient)Client).Invoke("CheckObject", new object[] { storageDirInfo.HighName, storageDirInfo.LowName });
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
                    throw new UnsupportedXException("Unsupport File Mode : " + mode.ToString());
            }
            return dir;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageDeleteResult sdr = new StorageDeleteResult();
            if (!DirectoryExists(info))
            {
                logger.Warn("The directory you want to delete is not exist, path: {0}", info.HighPlusLowName);
                sdr.IsDeleted = true;
            }
            else
            {
                StorageListResult slr = ListSubDirectoriesAndFiles(info);
                info = Preproccess2DirectoryStorageInfo(info);
                foreach (XFileInfo file in slr.Files)
                {
                    sdr.DeletedFileSize += DeleteFile(file).DeletedFileSize;
                }
                if (slr.SubDirs.Count != 0)
                {
                    foreach (XDirectoryInfo dir in slr.SubDirs)
                    {
                        sdr.DeletedFileSize += DeleteDirectory(dir).DeletedFileSize;
                    }
                }
                sdr.IsDeleted = Client.DeleteContainer(info.HighPlusLowName);
                if (info.IsDeleteParentFolder)
                {
                    var directoryPaths = new List<String>();
                    var directoryNames = info.LowName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < directoryNames.Length - 1; i++)
                    {
                        directoryNames[i] = directoryNames[i] + "/";
                        if (i == 0)
                        {
                            directoryPaths.Add(directoryNames[i]);
                        }
                        else
                        {
                            directoryPaths.Add(directoryPaths[i - 1] + directoryNames[i]);
                        }
                    }
                    for (int index = directoryPaths.Count - 1; index >= 0; index--)
                    {
                        var subBlobs = ListSubDirectoriesAndFiles(new StorageInfo(directoryPaths[index], ""));
                        if (subBlobs.Files.Count == 0 && subBlobs.SubDirs.Count == 0)
                        {
                            Client.DeleteContainer(Path.Combine(SystemLocation, directoryPaths[index]));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return sdr;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            Dictionary<string, string> objectInfo;
            StorageDeleteResult sdr = new StorageDeleteResult();
            StorageInfo storageInfo = PreproccessStorageInfo(info);
            CheckState(storageInfo.HighName);
            string name = "/".Equals(storageInfo.LowName) ? "" : storageInfo.LowName;
            //string fullURL = client.BuildObjectAbsoluteURL(info.HighName, name);
            Dictionary<string, string> headers = Client.Headers;
            try
            {
                objectInfo = (Dictionary<string, string>)((HCPClient)Client).Invoke("GetObjectInfo", new object[] { storageInfo.HighName, name, new Dictionary<string, string>(), headers });
                sdr.DeletedFileSize = long.Parse(objectInfo["Content-Length"]);
                sdr.IsDeleted = (bool)((HCPClient)Client).Invoke("DeleteObject", new object[] { storageInfo.HighName, name, new Dictionary<string, string>(), headers });
                Deletion = true;
                return sdr;
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                sdr.IsDeleted = true;
                Deletion = true;
                return sdr;
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            StorageInfo storageInfo = PreproccessStorageInfo(fileInfo);
            CheckState(storageInfo.HighName);
            string name = "/".Equals(storageInfo.LowName) ? "" : storageInfo.LowName;
            //string url = client.BuildObjectAbsoluteURL(storageInfo.HighName, name);
            Dictionary<string, string> requestHeaders = Client.Headers;
            Dictionary<string, string> responseHeaders;
            try
            {
                responseHeaders = (Dictionary<string, string>)((HCPClient)Client).Invoke("GetObjectInfo", new object[] { storageInfo.HighName, name, new Dictionary<string, string>(), requestHeaders });
            }
            catch (Exception e)
            {
                if (e is PathNotFoundException || e.InnerException is PathNotFoundException)
                {
                    logger.Debug("Object not exist container name : {0}, object name : {1}.", fileInfo.HighName, fileInfo.LowName);
                    return null;
                }
                else
                {
                    logger.Error("An error occurred when checking object, container name : {0}, object name : {1}. Error details: {2}.",
                        fileInfo.HighName, fileInfo.LowName, e);
                    throw;
                }
            }
            CloudFileInfo file = new CloudFileInfo(fileInfo.HighName, fileInfo.LowName, int.Parse(responseHeaders["Content-Length"]));
            return file;
        }

        public override bool FileExists(StorageInfo info)
        {
            //CheckState(info.HighName);
            StorageInfo storageInfo = PreproccessStorageInfo(info);
            CheckState(storageInfo.HighName);
            string name = "/".Equals(storageInfo.LowName) ? "" : storageInfo.LowName;
            //string fullURL = client.BuildObjectAbsoluteURL(storageInfo.HighName, name);
            Dictionary<string, string> headers = Client.Headers;
            return (bool)((HCPClient)Client).Invoke("CheckObject", new object[] { storageInfo.HighName, name });
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            StorageInfo dirInfo = Preproccess2DirectoryStorageInfo(info);
            CheckState(dirInfo.HighName);
            string name = "/".Equals(dirInfo.LowName) ? "" : dirInfo.LowName;
            //string fullURL = client.BuildObjectAbsoluteURL(dirInfo.HighName, name);
            Dictionary<string, string> headers = Client.Headers;
            headers[HCPConsts.KEY_X_HCP_TYPE] = HCPConsts.KEY_VAL_Directory;
            return (bool)((HCPClient)Client).Invoke("CheckContainer", new object[] { dirInfo.HighPlusLowName });
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            StorageOpenValidResult rs = base.Open();
            Dictionary<string, string> parms = XriObject.Params;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
            {
                string customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                this.openParamter.CustomizedMetaMode = (CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedMetaKey))
            {
                this.openParamter.CustomizedMetaData = ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_COUNT))
            {
                this.openParamter.MaxRetryCount = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_COUNT]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_INTERVAL))
            {
                this.openParamter.RetryInterval = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_INTERVAL]);
                int retryInterval = this.openParamter.RetryInterval;
                if (retryInterval <= 0 || retryInterval >= int.MaxValue)
                {
                    throw new Exception("unknown retryInterval value");
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.FLUSH_DNS))
            {
                this.openParamter.FlushDNS = bool.Parse(XriObject.Params[XRIParameterKeys.FLUSH_DNS]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.FAIL_OVER_MODE))
            {
                string failovermode = XriObject.Params[XRIParameterKeys.FAIL_OVER_MODE];
                this.openParamter.FailOverMode = (FailoverMode)Enum.Parse(typeof(FailoverMode), failovermode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.DSM_VALIDATE_KEY))
            {
                this.openParamter.IsValidate = bool.Parse(XriObject.Params[XRIParameterKeys.DSM_VALIDATE_KEY]);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY))
            {
                this.openParamter.IsRetry = bool.Parse(XriObject.Params[XRIParameterKeys.IS_RETRY]);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_ID_KEY))
            {
                SystemID = XriObject.Params[XRIParameterKeys.SYSTEM_ID_KEY];
                this.openParamter.PhysicalId = SystemID;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_NAME_KEY))
            {
                SystemName = XriObject.Params[XRIParameterKeys.SYSTEM_NAME_KEY];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_STATUS_KEY))
            {
                SystemStatus = (XSystemStatus)int.Parse(XriObject.Params[XRIParameterKeys.SYSTEM_STATUS_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_USAGE_KEY))
            {
                SystemUsage = (XSystemUsage)int.Parse(XriObject.Params[XRIParameterKeys.SYSTEM_USAGE_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.MODULE_TYPE_KEY))
            {
                moduleType = (ModuleType)(int.Parse(XriObject.Params[XRIParameterKeys.MODULE_TYPE_KEY]));
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SECONDARY_NAMESPACE_TIMEOUT))
            {
                this.openParamter.SecondaryTimeout = long.Parse(XriObject.Params[XRIParameterKeys.SECONDARY_NAMESPACE_TIMEOUT]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CACHE_SECONDARY_NAMESPACE))
            {
                this.openParamter.CacheSecondary = bool.Parse(XriObject.Params[XRIParameterKeys.CACHE_SECONDARY_NAMESPACE]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                createIfNotExist = bool.Parse(XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            if (parms.ContainsKey(XRIParameterKeys.DSM_MODIFY_TIME_KEY))
            {
                this.openParamter.ModifyTime = parms[XRIParameterKeys.DSM_MODIFY_TIME_KEY];
            }
            if (Client == null)
            {
                Client = new HCPClient();
                if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
                {
                    Client.InitConfig(openParamter);
                    Client.InitRetry(openParamter);
                    Client.HttpClient.CurrentSystem = this;
                }
            }
            return rs;
        }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult rs = new StorageOpenValidResult();
            try
            {
                rs = Client.GetPermissions();
                if (!CheckContainer(this.SystemLocation))
                {
                    if (CreateIfNotExists)
                    {
                        Client.Invoke("CreateContainer", new object[] { SystemLocation });
                    }
                    else
                    {
                        logger.Info("The root folder don't exist: {0}", SystemLocation);
                        rs.SystemHealth = XSystemHealth.Unaccessable;
                        this.SystemHealth = XSystemHealth.Unaccessable;
                        rs.IsDeleteAble = false;
                        rs.IsWriteAble = false;
                        rs.IsReadAble = false;
                        return rs;
                    }
                }
                this.innerTotalSpace = rs.TotalSpace;
                this.innerTotalUsedSpace = rs.TotalUsedSpace;
                this.innerTotalFreeSpace = rs.TotalFreeSpace;
                if (IsFull)
                {
                    rs.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    rs.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }

            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when opening cloud system. Details: {0}", ex);
                EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.HCP, ex);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.HDS_HCP, verifyFailedEventMessage);

                if (ex is AuthenticationFailedException)
                {
                    rs.Message = ex.Message;
                }
                else
                {
                    rs.Message = CloudCommonI18N.ResourceManager.GetString("Test_failed", AbstractXSystem.Culture);
                }
                rs.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = rs.SystemHealth;
            }
            return rs;
        }

        public override bool AddMetadata(StorageInfo storageInfo)
        {
            if (storageInfo.MetaInfos.Count == 0)
            {
                return false;
            }
            Dictionary<string, string> writerHeaders = Client.OpenStreamWriteModeHeaders;
            StorageInfo info = PreproccessStorageInfo(storageInfo);
            CheckState(info.HighName);
            // add retention.
            if (storageInfo.MetaInfos.ContainsKey("Archive-KeepTime") && storageInfo.MetaInfos.ContainsKey("Archive-BackupTime"))
            {
                long retentionTime = long.Parse(storageInfo.MetaInfos["Archive-KeepTime"]);
                long backupTime = long.Parse(storageInfo.MetaInfos["Archive-BackupTime"]);
                if (retentionTime > 0)
                {
                    DateTime backupDateTime = new DateTime(backupTime);
                    retentionTime = (long)(retentionTime - ((TimeSpan)(DateTime.UtcNow - backupDateTime)).TotalSeconds);//UTC时间减去已经过去的时间，保持各个数据块的retention时间一致
                    long ds = 86400;        //day second, 86400 = 3600*24
                    long ys = 31536000;     //year second, 31536000 = 3600*24*365
                    long y = retentionTime / ys;
                    long d = (retentionTime % ys) / ds;
                    long h = (retentionTime % ds) / 3600;
                    long s = (retentionTime % ds) % 3600;
                    string postData = "retention=A"
                                        + ((y == 0) ? "" : "+" + y + "y")
                                        + ((d == 0) ? "" : "+" + d + "d")
                                        + ((h == 0) ? "" : "+" + h + "h")
                                        + ((s == 0) ? "" : "+" + s + "s");
                    ((HCPClient)Client).Invoke("ModifySystemMetadata", new object[] { info.HighName, info.LowName, postData, Client.OpenStreamWriteModeHeaders });
                }
            }

            // add custom metadata.
            Dictionary<string, string> CustomizedAndDefaultMetadatas = new Dictionary<string, string>();
            switch (openParamter.CustomizedMetaMode)
            {
                case CustomizedMode.Close:
                    break;
                case CustomizedMode.SupportAll:
                    CustomizedAndDefaultMetadatas = HCPUtility.CombinDictionary(storageInfo.MetaInfos, openParamter.CustomizedMetaData);
                    AddExtendedParameters(info.HighName, info.LowName, CustomizedAndDefaultMetadatas);
                    break;
                case CustomizedMode.DocAveOnly:
                    AddExtendedParameters(info.HighName, info.LowName, storageInfo.MetaInfos);
                    break;
                case CustomizedMode.CustomizedOnly:
                    AddExtendedParameters(info.HighName, info.LowName, openParamter.CustomizedMetaData);
                    break;
                default:
                    AddExtendedParameters(info.HighName, info.LowName, storageInfo.MetaInfos);                      //默认仅支持默认的MetaData.
                    break;
            }
            return true;
        }

        private void AddExtendedParameters(string highName, string lowName, Dictionary<string, string> metaDatas)
        {
            string metadataXml = HCPUtility.GetMetadataXML(metaDatas);
            HCPClient hcpClient = (HCPClient)Client;
            if (hcpClient.IsDefaultNamespace)
            {
                hcpClient.AddDefaultCustomMetadata(highName, lowName, metadataXml);
            }
            else
            {
                hcpClient.AddCustomMetadata(highName, lowName, metadataXml);
            }
        }

        protected override bool CheckContainer(string containerName)
        {
            try
            {
                bool isOK = (bool)((HCPClient)Client).Invoke("CheckContainer", new object[] { containerName, false });
                if (!Containers.ContainsKey(containerName) && isOK)
                {
                    Containers.Add(containerName, true);
                }
                return isOK;
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                return false;
            }
        }

        public override StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}
