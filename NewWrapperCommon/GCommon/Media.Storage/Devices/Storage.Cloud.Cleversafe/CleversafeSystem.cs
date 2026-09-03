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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using Common;
    using Resources.CloudCommonI18N;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Util;
    #endregion

    class CleversafeSystem : AbstractXSystem
    {
        CleversafeOpenParameter openParameter;
        StorageLogger logger = new StorageLogger(typeof(CleversafeSystem));
        CleversafeClient client;

        public override String Type
        {
            get
            {
                return "CleversafeSystem";
            }
        }

        private static Boolean CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        static CleversafeSystem()
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

        public CleversafeSystem(String xriStr, String initMode, AbstractXSystem parentSystem)
            : base(xriStr, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.IsRetry = true;
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            logger.Info("Cleversafe system open.");
            openParameter = new CleversafeOpenParameter();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
            {
                try
                {
                    String customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                    openParameter.CustomizedMetaMode = (CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
                }
                catch (Exception e)
                {
                    logger.Error("Unknown custom metadata mode value.Details : {0}, the value is {1}", e, XriObject.Params[XRIParameterKeys.CustomizedModeKey]);
                    throw new Exception("Unknown custom metadata mode value.Details : {0}", e);
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedMetaKey))
            {
                try
                {
                    openParameter.CustomizedMetaData = ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
                }
                catch (Exception e)
                {
                    logger.Error("Unknown custom metadata format. {0}", XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);
                    throw new Exception("Unknown custom metadata format.", e);
                }
            }
            openParameter.RetryInterval = this.RetryInterval;
            openParameter.MaxRetryCount = this.MaxRetryCount;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_USERNAME_KEY))
            {
                openParameter.UserName = XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Cloud_PASSWORD_KEY))
            {
                openParameter.Password = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.MODULE_TYPE_KEY))
            {
                openParameter.ModuleType = int.Parse(XriObject.Params[XRIParameterKeys.MODULE_TYPE_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY))
            {
                openParameter.NeedRetry = Boolean.Parse(XriObject.Params[XRIParameterKeys.IS_RETRY]);
            }
            else
            {
                openParameter.NeedRetry = true;
            }
            //AccesserIPs,用;分割
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ACCESSER_IP))
            {
                openParameter.AccesserIPs = new List<String>(XriObject.Params[XRIParameterKeys.ACCESSER_IP].Split(';'));
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.VAULT_NAME))
            {
                openParameter.VaultName = XriObject.Params[XRIParameterKeys.VAULT_NAME].Trim();
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Enable_SSL))
            {
                openParameter.Protocol = Boolean.Parse(XriObject.Params[XRIParameterKeys.Enable_SSL]) ? "https" : "http";
            }
            else
            {
                openParameter.Protocol = "http";
            }
            this.openParameter.system = this;
            client = new CleversafeClient();
            client.InitConfig(this.openParameter);
            SetSystemDescription();
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            return new StorageOpenValidResult();
        }

        public StorageInfo PreproccessToDirectoryStorageInfo(StorageInfo dirInfo)
        {
            var info = PreproccessToFileStorageInfo(dirInfo);
            info.LowName = info.LowName.TrimEnd('/') + "/";
            return info;
        }

        public StorageInfo PreproccessToFileStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo info = dirInfo.Clone();
            if (String.IsNullOrEmpty(info.LowName))
            {
                info.LowName = "\\";
            }
            if (!String.IsNullOrEmpty(openParameter.VaultName))
            {
                info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                info.HighName = openParameter.VaultName;
            }
            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');
            return info;
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
                result = client.GetPermissions();
                if (!CheckVault(openParameter.VaultName))
                {
                    logger.Info("The vaultName {0} doesn't exist.", openParameter.VaultName);
                    result.SystemHealth = XSystemHealth.Unaccessable;
                    result.IsDeleteAble = false;
                    result.IsWriteAble = false;
                    result.IsReadAble = false;
                    return result;
                }
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(XriObject.VIM, this.XriString, new CheckFreeSpace(client.GetUserAccountInfo));
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
                    logger.Error("Authentication failed. Please verify the entered information and try again.Details : {0}", ex);
                    result.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
                else
                {
                    result.Message = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Test_failed", AbstractXSystem.Culture);
                    logger.Error("Validation test failed. Please verify the entered information and try again.Details : {0}", ex);
                    result.SystemHealth = XSystemHealth.ConnectedFailed;
                }
            }
            finally
            {
                this.SystemHealth = result.SystemHealth;
            }
            return result;
        }

        protected Boolean CheckVault(String vaultName)
        {
            logger.Debug("Begin check vault : {0}.", vaultName);
            Boolean result = client.CheckVault(vaultName);
            return result;
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult deleteResult = new StorageDeleteResult();
            StorageInfo infoTemp = PreproccessToFileStorageInfo(info);
            CheckState();
            try
            {
                deleteResult.DeletedFileSize = client.GetObjectInfo(infoTemp.HighName, infoTemp.LowName).FileSize;
                deleteResult.IsDeleted = client.DeleteObject(infoTemp.HighName, infoTemp.LowName, false);
            }
            catch (PathNotFoundException ex)
            {
                logger.Error("Path not found .Details : {0}.", ex);
                Deletion = true;
                deleteResult.DeletedFileSize = 0;
                deleteResult.IsDeleted = true;
                return deleteResult;
            }
            catch (Exception e)
            {
                logger.Error(String.Format("An error occurred when deleting object, vauleName : {0}, objectName : {1}.", infoTemp.HighName, infoTemp.LowName), e);
                throw;
            }
            //标记执行过删除
            Deletion = true;
            return deleteResult;
        }
        public override Boolean DirectoryExists(StorageInfo info)
        {
            StorageInfo storageInfo = PreproccessToDirectoryStorageInfo(info);
            return client.CheckObject(storageInfo.HighName, storageInfo.LowName, ObjectType.Directory);
        }

        private Dictionary<String, String> ProccessParams(StorageInfo dirInfo)
        {
            var queryParams = client.QueryParams;
            if (!String.IsNullOrEmpty(dirInfo.LowName) && !"/".Equals(dirInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", dirInfo.LowName);
            }
            return queryParams;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            if (!DirectoryExists(info))
            {
                logger.Warn("The directory you want to delete don't exist, path : {0}", info.HighPlusLowName);
                Deletion = true;
                return new StorageDeleteResult { IsDeleted = true };
            }
            StorageInfo dirInfoTemp = PreproccessToDirectoryStorageInfo(info);
            Boolean loop = true;
            int size = 0;
            do
            {
                ResponseInfo responseInfo = client.ListObjects(client.BuildURL(dirInfoTemp.HighName), this.ProccessParams(dirInfoTemp));
                var xmlObjs = XElement.Parse(responseInfo.ResponseXml);
                XNamespace xmlns = xmlObjs.GetDefaultNamespace();
                XName contentsName = xmlns + "Contents";
                XName keyName = xmlns + "Key";
                var deleteContent = new XElement("Delete", from xmlObj in xmlObjs.Elements(contentsName)
                                                           select new XElement("Object", new XElement("Key", xmlObj.Element(keyName).Value)));
                client.DeleteObjects(client.BuildURL(dirInfoTemp.HighName) + "/?delete", null, client.Headers, deleteContent.ToString());
                size += xmlObjs.Elements(xmlns + "Contents").Sum(obj => int.Parse(obj.Element(xmlns + "Size") == null ? "0" : obj.Element(xmlns + "Size").Value));
                if (!Boolean.Parse(xmlObjs.Element(xmlns + "IsTruncated").Value))
                {
                    loop = false;
                }
            } while (loop);
            if (info.IsDeleteParentFolder)
            {
                DeleteParentFolder(dirInfoTemp);
            }
            Deletion = true;
            return new StorageDeleteResult { IsDeleted = true, DeletedFileSize = size };
        }

        private void DeleteParentFolder(StorageInfo dirInfo)
        {
            var directoryPaths = new List<String>();
            var directoryNames = dirInfo.LowName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                    var deleteContent = new XElement("Delete", new XElement("Object", new XElement("Key", directoryPaths[index])));
                    client.DeleteObjects(client.BuildURL(dirInfo.HighName) + "/?delete", null, client.Headers, deleteContent.ToString());
                }
                else
                {
                    break;
                }
            }
        }

        public override Boolean FileExists(StorageInfo info)
        {
            CheckState();
            StorageInfo storageInfo = PreproccessToFileStorageInfo(info);
            Boolean result = false;
            try
            {
                result = client.CheckObject(storageInfo.HighName, storageInfo.LowName, ObjectType.File);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when check object, vaultName : {0}, objectName : {1}, Error : {2}.", info.HighName, info.LowName, e);
                throw;
            }
            return result;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState();
            StorageCopyResult storageCopyResult = new StorageCopyResult();
            try
            {
                if (client.CheckObject(openParameter.VaultName, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName).TrimStart(new char[] { '\\', '/' }), ObjectType.File))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (client.CheckObject(openParameter.VaultName, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }), ObjectType.File) && !isOverWrite)
                        {
                            storageCopyResult.IsCopyed = true;
                            return storageCopyResult;
                        }
                    }
                    StorageInfo sourceStorageInfo = PreproccessToFileStorageInfo(sourceFileInfo);
                    StorageInfo targetStorageInfo = PreproccessToFileStorageInfo(targetFileInfo);
                    Dictionary<String, String> queryParams = client.CopyFileQueryParams;
                    storageCopyResult.IsCopyed = client.CopyFile(sourceStorageInfo, targetStorageInfo, queryParams);
                }
                else
                {
                    storageCopyResult.Message = "Source file don't exist";
                    storageCopyResult.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                storageCopyResult.Message = e.ToString();
                storageCopyResult.IsCopyed = false;
                logger.Error("Copy file failed.Details:{0}", e);
            }
            return storageCopyResult;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourcFile, IXSystem destSystem, StorageInfo destFile, Boolean isOverWrite)
        {
            if (destSystem is CleversafeSystem
               && XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY], StringComparison.OrdinalIgnoreCase)
               && XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY], StringComparison.OrdinalIgnoreCase))
            {
                return this.CopyFile(sourcFile, destFile, isOverWrite);
            }
            else
            {
                return base.CopyFile(sourcFile, destSystem, destFile, isOverWrite);
            }
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            StorageMoveResult moveResult = new StorageMoveResult();
            try
            {
                StorageCopyResult copyResult = this.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                if (copyResult.IsCopyed)
                {
                    client.DeleteObject(openParameter.VaultName, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName), false);
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
                StorageCopyResult copyRS = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
                if (copyRS.IsCopyed)
                {
                    DeleteDirectory(sourceDirInfo);
                }
                else
                {
                    moveResult.IsMoved = false;
                    moveResult.Message = copyRS.Message;
                }
            }
            catch (Exception ex)
            {
                moveResult.IsMoved = false;
                moveResult.Message = ex.Message;
                logger.Warn("Move directory failed. Details : {0}", ex.Message);
            }
            return moveResult;
        }

        public override StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        {
            StorageMoveResult moveResult = new StorageMoveResult();
            StorageCopyResult copyResult = CopyFile(srcFile, destSystem, destFile, true);
            if (copyResult.IsCopyed)
            {
                moveResult.URI = copyResult.URI;
                DeleteFile(srcFile);
                moveResult.IsMoved = true;
            }
            else
            {
                moveResult.IsMoved = false;
                moveResult.Message = copyResult.Message;
                logger.Info("Move file failed. Details : {0}", copyResult.Message);
            }
            return moveResult;
        }

        private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, Boolean isOverWrite)
        {
            StorageCopyResult result = new StorageCopyResult();
            try
            {
                StorageInfo sourceDirectoryInfo = PreproccessToDirectoryStorageInfo(sourceFolderInfo);
                if (client.CheckObject(sourceDirectoryInfo.HighName, sourceDirectoryInfo.LowName, ObjectType.Directory))
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
                logger.Error("Copy directory failed.Details : {0}", ex);
            }
            return result;
        }

        public override void Close() { }

        //在执行OpenStream时，首先先看info中是否填写了MetaInfos，如果有，放入"X-AMZ-META-"属性中
        public StorageInfo PreprocessOpenStreamWithMeta(StorageInfo info)
        {
            Dictionary<String, String> preparedMetaInfos = new Dictionary<String, String>();
            foreach (KeyValuePair<String, String> entry in info.MetaInfos)
            {
                preparedMetaInfos["X-AMZ-META-".ToLower(CultureInfo.InvariantCulture) + entry.Key] = entry.Value != null ? client.HttpClient.Encode(entry.Value) : entry.Value;
            }
            info.MetaInfos = preparedMetaInfos;
            return info;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            info = PreprocessOpenStreamWithMeta(info);
            StorageInfo infoTemp = PreproccessToFileStorageInfo(info);
            CheckState();
            return client.DoExcuteWithMultiAccesserIPRetry<XStream>(delegate()
            {
                string fullURL = client.BuildURL(infoTemp.HighName, infoTemp.LowName);
                switch (fileMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                    case FileMode.Append:
                        Dictionary<string, string> writerHeaders = client.Headers;
                        writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
                        writerHeaders["Content-Length"] = infoTemp.Length.ToString();
                        AddMetadata(info, writerHeaders);
                        XStream writeStream = client.OpenObjectForWrite(fullURL, writerHeaders);
                        writeStream.System = this;
                        writeStream.Info = info;
                        writeStream.MaxRetryCount = client.CloudOpenParam.MaxRetryCount;
                        this.Written = true;
                        return writeStream;
                    case FileMode.Open:
                        XStream readStream = null;
                        try
                        {
                            var readerHeaders = client.Headers;
                            if (infoTemp.Length > 0)
                            {
                                readerHeaders["Range"] = "bytes=" + infoTemp.Offset + "-" + (infoTemp.Length + infoTemp.Offset);
                            }
                            readStream = client.OpenObjectForRead(fullURL, readerHeaders);
                            readStream.System = this;
                            if (infoTemp.Length <= 0)
                            {
                                info.Length = ((HttpDownloadStream)readStream).InnerLength;
                            }
                            readStream.Info = info;
                            readStream.MaxRetryCount = client.CloudOpenParam.MaxRetryCount;
                        }
                        catch (Exception e)
                        {
                            logger.Error("Failed to open the file. File Path: {0}.Details :{1}", fullURL, e.Message);
                            throw;
                        }
                        return readStream;
                    default:
                        logger.Error("Unsupported file mode :" + fileMode);
                        throw new NotSupportedException("Unsupported file mode :" + fileMode);
                }
            });
        }

        public virtual Boolean AddMetadata(StorageInfo storageInfo, Dictionary<string, string> writerHeaders)
        {
            if (client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.Close))
            {
                return true;
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.CustomizedOnly))
            {
                foreach (KeyValuePair<string, string> entry in client.CloudOpenParam.CustomizedMetaData)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.DocAveOnly))
            {
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.SupportAll))
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
                throw new Exception("unKnown Customized Mode:" + client.CloudOpenParam.CustomizedMetaMode.ToString());
            }
            return base.AddMetadata(storageInfo);
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, System.IO.FileMode mode)
        {
            CheckState();
            CloudDirectoryInfo directoryInfo = null;
            Dictionary<String, String> headers = client.Headers;
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Append:
                case FileMode.Truncate:
                    StorageInfo dirInfoTemp = PreproccessToDirectoryStorageInfo(dirInfo);
                    dirInfoTemp.LowName = "/".Equals(dirInfoTemp.LowName) ? "" : dirInfoTemp.LowName;//不知道干什么的
                    headers["Content-Type"] = "DOCAVE/directory".ToLower(CultureInfo.InvariantCulture);
                    headers["Content-Length"] = "0";
                    client.CreateObjectWithNoContent(dirInfoTemp.HighName, dirInfoTemp.LowName, headers);
                    directoryInfo = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    directoryInfo.IsExists = true;
                    break;
                case FileMode.Open:
                    Boolean exists = this.DirectoryExists(dirInfo);
                    if (!exists)
                    {
                        return null;
                    }
                    directoryInfo = new CloudDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                    directoryInfo.IsExists = exists;
                    break;
                default:
                    //break;
                    throw new UnsupportedXException("Unsupported File Mode : " + mode.ToString());
            }
            return directoryInfo;
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
            StorageInfo storageInfo = PreproccessToDirectoryStorageInfo(dirInfo);
            CheckState();
            var queryParams = this.ProccessParams(storageInfo);
            queryParams.Add("delimiter", "/");
            queryParams.Add("format", "xml");
            ResponseInfo responseInfo = client.ListObjects(client.BuildURL(storageInfo.HighName), queryParams);
            String responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            ConvertXmlToList(dirs, files, responseXmlString, storageInfo, dirInfo);
            while (true)
            {
                Regex defaultRegex = new Regex("<NextMarker>(.+)</NextMarker>");
                MatchCollection matches = defaultRegex.Matches(responseInfo.ResponseXml);
                String markerValue = "";
                if (matches.Count == 1 && !markerValue.Equals(matches[0].Groups[1].Value))
                {
                    markerValue = matches[0].Groups[1].Value;
                    if (queryParams.ContainsKey("marker"))
                    {
                        queryParams["marker"] = markerValue;
                    }
                    else
                    {
                        queryParams.Add("marker", markerValue);
                    }
                    responseInfo = client.ListObjects(client.BuildURL(storageInfo.HighName), queryParams);
                    ConvertXmlToList(dirs, files, responseInfo.ResponseXml, storageInfo, dirInfo);
                }
                else
                {
                    break;
                }
            }
            StorageListResult results = new StorageListResult();
            results.SubDirs = dirs;
            results.Files = files;
            return results;
        }

        public void ConvertXmlToList(List<XDirectoryInfo> dirs, List<XFileInfo> files, String responseXmlString, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            CloudDirectoryInfo dir;
            CloudFileInfo file;
            responseXmlString = responseXmlString.Replace(" xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"", "");
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/Contents");
            XPathNavigator singleNav;
            String name;
            Int64 size;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("Key");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    singleNav = nav.SelectSingleNode("Size");
                    if (singleNav != null)
                    {
                        size = singleNav.ValueAsLong;
                    }
                    if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                        if (String.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                        dirs.Add(dir);
                    }
                    else
                    {
                        file = new CloudFileInfo(storageInfo.HighPlusLowName, name.RemoveFirst(dirInfo.LowName), size);
                        files.Add(file);
                    }
                }
            }
            navs = client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/CommonPrefixes");
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                singleNav = nav.SelectSingleNode("Prefix");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                    if (String.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                    dirs.Add(dir);
                }
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            CloudFileInfo xfileInfo = null;
            try
            {
                xfileInfo = new CloudFileInfo(fileInfo.HighName, fileInfo.LowName);
                StorageInfo info = PreproccessToFileStorageInfo(fileInfo);
                xfileInfo = (CloudFileInfo)client.GetObjectInfo(info.HighName, info.LowName);
                xfileInfo.HighName = fileInfo.HighName;
                xfileInfo.LowName = fileInfo.LowName;
            }
            catch (Exception e)
            {
                if (e is PathNotFoundException || e.InnerException is PathNotFoundException)
                {
                    logger.Warn(String.Format("Object not exist vaultName : {0}, objectName : {1}.", fileInfo.HighName, fileInfo.LowName));
                    return null;
                }
                else
                {
                    logger.Error(String.Format("An error occurred when check object vault name : {0}, object name : {1}.", fileInfo.HighName, fileInfo.LowName), e);
                    throw;
                }
            }
            return xfileInfo;
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Cleversafe, Access Key ID: " + this.openParameter.UserName + ", vaultName: " + this.openParameter.VaultName;
            List<String> keys = new List<String>();
            keys.Add(this.openParameter.VaultName);
            keys.Add(this.openParameter.UserName);
            List<String> securityKeys = new List<String>();
            securityKeys.Add(this.openParameter.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }
    }
}
