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

namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Resources.DropboxI18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;
    using static AvePoint.Media.Storage.Cloud.Dropbox.DropboxObject;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/9/28",
    "yanxin.fu@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion

    class DropboxSystem : AbstractXSystem
    {
        private String accessToken;
        private Boolean isValidate;
        private DropboxRetry retry;
        private StorageLogger logger = new StorageLogger(typeof(DropboxSystem));

        public override String Type
        {
            get
            {
                return DropboxConstants.StorageType;
            }
        }
        protected override void SetSystemDescription()
        {
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = "DropBox Storage, Root folder: " + this.SystemLocation;
            var keys = new List<String>();
            keys.Add(this.SystemLocation);
            keys.Add(this.accessToken);
            this.SystemKey = GenerateSystemKey(keys, new List<String>());
        }

        static DropboxSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;
            ServicePointManager.ServerCertificateValidationCallback =
                        new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
        }

        private static Boolean CheckValidationResult(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public DropboxSystem(String xri, AbstractXSystem parentSystem)
            : base(xri, parentSystem)
        {
            base.SystemHealth = XSystemHealth.Unknown;
            this.Open();
            this.retry = new DropboxRetry(this.isValidate, this.MaxRetryCount, this.RetryInterval);
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.ACCESS_TOKEN))
            {
                this.accessToken = SecretUtil.DescryptPassword(this.XriObject.Params[XRIParameterKeys.ACCESS_TOKEN]);
            }
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.DROPBOX_VALIDATE_KEY))
            {
                this.isValidate = Boolean.Parse(this.XriObject.Params[XRIParameterKeys.DROPBOX_VALIDATE_KEY]);
            }
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_INFO))
            {
                this.Proxy = new WebProxy(this.XriObject.Params[XRIParameterKeys.PROXY_INFO]);//兼容老数据
            }
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.ContainerKey))
            {
                this.SystemLocation = this.XriObject.Params[XRIParameterKeys.ContainerKey];
            }
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                this.createIfNotExist = Boolean.Parse(this.XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.DROPBOX_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.DROPBOX_PROXY_SETTING]))
            {
                if (this.XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_IP) && this.XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_PORT))
                {
                    var proxyIp = this.XriObject.Params[XRIParameterKeys.PROXY_IP];
                    var proxyPort = int.Parse(this.XriObject.Params[XRIParameterKeys.PROXY_PORT]);
                    this.Proxy = new WebProxy(proxyIp, proxyPort);
                    if (this.XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_USERNAME) && this.XriObject.Params.ContainsKey(XRIParameterKeys.PROXYPASSWORD))
                    {
                        var userName = this.XriObject.Params[XRIParameterKeys.PROXY_USERNAME];
                        var password = SecretUtil.DescryptPassword(this.XriObject.Params[XRIParameterKeys.PROXYPASSWORD]);
                        var nameAndDomain = userName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        if (nameAndDomain.Length > 1)
                            this.Proxy.Credentials = new NetworkCredential(nameAndDomain[1], password, nameAndDomain[0]);
                        else
                            this.Proxy.Credentials = new NetworkCredential(userName, password);
                    }
                }
            }
            SetSystemDescription();
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            return new StorageOpenValidResult();
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            CheckState();
            return this.retry.Retry<XDirectoryInfo>(delegate()
            {
                var dir = default(DropboxDirectoryInfo);
                switch (mode)
                {
                    case System.IO.FileMode.Create:
                    case System.IO.FileMode.CreateNew:
                    case System.IO.FileMode.OpenOrCreate:
                        if (!String.IsNullOrEmpty(dirInfo.HighPlusLowName))
                        {
                            if (!this.DirectoryExists(dirInfo))
                            {
                                CreateDirectory(dirInfo);
                            }
                        }
                        dir = new DropboxDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
                        dir.IsExists = true;
                        break;
                    case System.IO.FileMode.Open:
                        var exists = DirectoryExists(dirInfo);
                        if (!exists)
                        {
                            return null;
                        }
                        dir = new DropboxDirectoryInfo(dirInfo.HighName, dirInfo.LowName, GetObjectInfo(dirInfo).Server_modified);
                        dir.IsExists = exists;
                        break;
                    case System.IO.FileMode.Append:
                    case System.IO.FileMode.Truncate:
                    default:
                        throw new UnsupportedXException("Unsupported File Mode : " + mode.ToString());
                }
                return dir;
            });
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState();
            var rs = new StorageCopyResult();
            var sourcePath = Path.Combine(this.SystemLocation, sourceFileInfo.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            sourcePath = sourcePath.StartsWith("/") ? sourcePath : "/" + sourcePath;
            sourcePath = sourcePath.EndsWith("/") ? sourcePath.TrimEnd('/') : sourcePath;
            var targetPath = Path.Combine(this.SystemLocation, targetFileInfo.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            targetPath = targetPath.StartsWith("/") ? targetPath : "/" + targetPath;
            targetPath = targetPath.EndsWith("/") ? targetPath.TrimEnd('/') : targetPath;
            try
            {
                return this.retry.Retry<StorageCopyResult>(delegate()
                {
                    if (this.FileExists(sourceFileInfo))
                    {
                        if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (this.FileExists(targetFileInfo) && !isOverWrite)
                            {
                                rs.IsCopyed = true;
                                return rs;
                            }
                        }
                        var url = StorageUrl.DropboxCopy;
                        var param = "{\"from_path\": \"" + sourcePath + "\",\"to_path\":\"" + targetPath + "\",\"autorename\":false}";
                        var bytes = Encoding.UTF8.GetBytes(param);
                        var request = CreateRequestWithToken(url, "POST");
                        request.ContentType = @"application/json";
                        request.ContentLength = bytes.Length;
                        using (var requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(bytes, 0, bytes.Length);
                        }
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                throw new WebException(String.Format("Copy file failed, StatusCode={0} source url is {1}, dest url is {2}",
                                    response.StatusCode, sourcePath, targetPath));
                            }
                        }
                        rs.IsCopyed = true;
                    }
                    else
                    {
                        rs.Message = "Source file is not exist";
                        rs.IsCopyed = false;
                    }
                    return rs;
                });
            }
            catch (Exception e)
            {
                rs.Message = e.ToString();
                rs.IsCopyed = false;
                logger.Error("Copy file failed: {0}", e);
            }
            return rs;
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, Boolean isOverWrite)
        {
            return this.retry.Retry<StorageCopyResult>(delegate()
            {
                if (destSystem is DropboxSystem
                    && this.XriObject.Params[XRIParameterKeys.ACCESS_TOKEN].Equals(destSystem.XriObject.Params[XRIParameterKeys.ACCESS_TOKEN], StringComparison.OrdinalIgnoreCase))
                    return this.CopyFile(srcFile, destFile, isOverWrite);
                else
                    return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
            });
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return this.ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return this.ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        //TODO 处理大数据 默认：10000，最大值：25000
        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            CheckState();
            var responseJson = String.Empty;
            var results = new StorageListResult();
            var has_more = false;
            var cursor = String.Empty;
            results = this.retry.Retry<StorageListResult>(delegate()
            {
                var tempResult = new StorageListResult();
                var dirs = new List<XDirectoryInfo>();
                var files = new List<XFileInfo>();
                var uriStr = StorageUrl.DropboxList;
                var request = CreateRequestWithToken(uriStr, "POST");
                var path = Path.Combine(this.SystemLocation, dirInfo.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
                path = path.StartsWith("/") ? path : "/" + path;
                path = path.EndsWith("/") ? path.TrimEnd('/') : path;
                var param = "{\"path\": \"" + path + "\",\"recursive\": false,\"include_media_info\": false,\"include_deleted\": false}";
                this.logger.Info("Param is " + param);
                var bytes = Encoding.UTF8.GetBytes(param);
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            responseJson = streamReader.ReadToEnd();
                        }
                    }
                }
                ConvertJsonToList(dirs, files, responseJson, out has_more, out cursor);
                tempResult.SubDirs = dirs;
                tempResult.Files = files;
                return tempResult;
            });
            this.logger.Info("If has more : " + has_more + ", cursor : " + cursor);
            //while has_more , continue
            while (has_more)
            {
                this.logger.Info("There are too many files or folders, continue list items.");
                var moreResult = this.retry.Retry<StorageListResult>(delegate()
                {
                    var tempResult = new StorageListResult();
                    var dirs = new List<XDirectoryInfo>();
                    var files = new List<XFileInfo>();
                    var uriStr = StorageUrl.DropboxListContinue;
                    var request = CreateRequestWithToken(uriStr, "POST");
                    var param = "{\"cursor\": \"" + cursor + "\"}";
                    this.logger.Info("Param is " + param);
                    var bytes = Encoding.UTF8.GetBytes(param);
                    request.ContentType = "application/json";
                    request.ContentLength = bytes.Length;
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                responseJson = streamReader.ReadToEnd();
                            }
                        }
                    }
                    ConvertJsonToList(dirs, files, responseJson, out has_more, out cursor);
                    tempResult.SubDirs = dirs;
                    tempResult.Files = files;
                    return tempResult;
                });
                results.SubDirs.AddRange(moreResult.SubDirs);
                results.Files.AddRange(moreResult.Files);
            }
            return results;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            if (info.LowName == null)
                info.LowName = String.Empty;
            var rs = new StorageDeleteResult();
            return this.retry.Retry<StorageDeleteResult>(delegate()
            {
                if (!DirectoryExists(info))
                {
                    logger.Warn("The directory you want to delete is not exist, path:{0}", info.HighPlusLowName);
                    rs.IsDeleted = true;
                }
                else
                {
                    var deletedSize = GetFolderSize(info, 0);
                    rs.IsDeleted = this.DeleteDirectory(this.SystemLocation, info.HighPlusLowName);
                    rs.DeletedFileSize = deletedSize;
                    if (info.IsDeleteParentFolder)
                    {
                        var directoryNames = info.HighPlusLowName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        var directoryPaths = new List<String>();
                        for (var i = 0; i < directoryNames.Length - 1; i++)
                        {
                            directoryNames[i] = directoryNames[i].TrimEnd('\\').TrimEnd('/') + "\\";
                            if (i == 0)
                                directoryPaths.Add(directoryNames[i]);
                            else
                                directoryPaths.Add(PathUtil.CombinePath(directoryPaths[i - 1], directoryNames[i]));
                        }
                        for (var index = directoryPaths.Count - 1; index >= 0; index--)
                        {
                            var highAndLowName = new StorageInfo(directoryPaths[index], "");
                            var subObjects = ListSubDirectoriesAndFiles(highAndLowName);
                            if (subObjects.Files.Count == 0 && subObjects.SubDirs.Count == 0)
                                this.DeleteDirectory(this.SystemLocation, highAndLowName.HighPlusLowName);
                            else
                                break;
                        }
                    }
                }
                this.Deletion = true;
                return rs;
            });
        }

        public override Boolean FileExists(StorageInfo info)
        {
            var result = false;
            try
            {
                return this.retry.Retry<Boolean>(delegate()
                {
                    var objectInfo = GetObjectInfo(info);
                    if (!objectInfo.Is_deleted)
                    {
                        result = true;
                    }
                    return result;
                });
            }
            catch (PathNotFoundException)
            {
                logger.Warn("The object not fount , path : {0}", info.HighPlusLowName);
            }
            return result;
        }

        public override Boolean DirectoryExists(StorageInfo info)
        {
            return this.FileExists(info);
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            var stream = new DropboxStream(this, info, fileMode);
            stream.System = this;
            stream.Info = info;
            if (fileMode != FileMode.Open)
            {
                this.Written = true;
            }
            return stream;
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            DropboxFileInfo result = null;
            try
            {
                result = this.retry.Retry<DropboxFileInfo>(delegate()
                {

                    var fileObject = this.GetObjectInfo(fileInfo);
                    var info = new DropboxFileInfo(fileInfo.HighName, fileInfo.LowName, fileObject.Size, fileObject.Server_modified);
                    info.FileSize = fileObject.Size;
                    if (!fileObject.Is_deleted)
                    {
                        info.IsExists = true;
                    }
                    return info;
                });
            }
            catch (PathNotFoundException)
            {
                logger.Debug("File not exist.");
            }
            return result;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {
            var moveRS = new StorageMoveResult();
            try
            {
                return this.retry.Retry<StorageMoveResult>(delegate()
                {
                    var copyRS = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
                    if (copyRS.IsCopyed)
                    {
                        DeleteDirectory(sourceDirInfo);
                    }
                    else
                    {
                        moveRS.IsMoved = false;
                        moveRS.Message = copyRS.Message;
                    }
                    return moveRS;
                });
            }
            catch (Exception ex)
            {
                moveRS.IsMoved = false;
                moveRS.Message = ex.Message;
            }
            return moveRS;
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            var moveRS = new StorageMoveResult();
            try
            {
                return this.retry.Retry<StorageMoveResult>(delegate()
                {
                    var copyRS = this.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                    if (copyRS.IsCopyed)
                    {
                        this.DeleteFile(sourceFileInfo);
                    }
                    else
                    {
                        moveRS.IsMoved = false;
                        moveRS.Message = copyRS.Message;
                    }
                    return moveRS;
                });
            }
            catch (Exception ex)
            {
                moveRS.IsMoved = false;
                moveRS.Message = ex.Message;
            }
            return moveRS;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            try
            {
                return this.retry.Retry<StorageDeleteResult>(delegate()
                {
                    if (!DirectoryExists(info))
                    {
                        logger.Warn("The directory you want to delete is not exist, path:{0}", info.HighPlusLowName);
                        result.IsDeleted = true;
                    }
                    else
                    {
                        result.DeletedFileSize = GetObjectInfo(info).Size;
                        var url = StorageUrl.DropboxDelete;
                        this.logger.Info("Url is {0}.  Dropbox API v2", url);
                        var path = Path.Combine(this.SystemLocation, info.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
                        path = path.StartsWith("/") ? path : "/" + path;
                        path = path.EndsWith("/") ? path.TrimEnd('/') : path;
                        var request = this.CreateRequestWithToken(url, "POST");
                        request.ContentType = @"application/json";
                        var param = "{\"path\":\"" + path + "\"}";
                        logger.Info("The param is: {0}", param);
                        Byte[] metadataData = Encoding.UTF8.GetBytes(param);
                        request.ContentLength = metadataData.Length;
                        using (var requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(metadataData, 0, metadataData.Length);
                        }
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                result.IsDeleted = true;
                                this.Deletion = true;
                            }
                        }
                    }
                    return result;
                });
            }
            catch (PathNotFoundException)
            {
                logger.Warn("The file {0} was not exist", info.HighPlusLowName);
                result.IsDeleted = true;
                result.DeletedFileSize = 0;
            }
            return result;
        }

        public override void Close() { }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            this.logger.Debug("Start validate,Dropbox API V2");
            if (this.IsForcePassValidation)
                return base.Validate();
            var openValidResult = new StorageOpenValidResult();
            try
            {
                var spaceInfo = this.CheckFreeSpace();
                openValidResult.TotalSpace = this.innerTotalSpace = spaceInfo.TotalSpace;
                openValidResult.TotalFreeSpace = this.innerTotalFreeSpace = spaceInfo.TotalFreeSpace;
                openValidResult.TotalUsedSpace = this.innerTotalUsedSpace = spaceInfo.TotalUsedSpace;
                openValidResult.IsHasPermission = true;
                if (!this.SystemLocation.Equals("/") && !String.IsNullOrEmpty(this.SystemLocation))
                {
                    if (this.DirectoryExists(new StorageInfo("/", "/")))
                    {
                        openValidResult.IsReadAble = true;
                    }
                    else
                    {
                        if (this.CreateIfNotExists)
                        {
                            this.OpenDirectory(new StorageInfo("/", "/"), FileMode.Create);
                        }
                        else
                        {
                            logger.Info("the root folder don't exist:" + this.SystemLocation);
                            openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_RootFolderNotExist", AbstractXSystem.Culture);
                            openValidResult.SystemHealth = XSystemHealth.Unaccessable;
                            openValidResult.IsDeleteAble = false;
                            openValidResult.IsWriteAble = false;
                            openValidResult.IsReadAble = false;
                            return openValidResult;
                        }
                    }
                }
                if (ValidateIsFull())
                {
                    openValidResult.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    var info = new StorageInfo();
                    var b = new Byte[1];
                    info.Length = b.Length;
                    info.LowName = System.Guid.NewGuid().ToString();
                    using (var stream = OpenStream(info, FileMode.CreateNew))
                    {
                        b[0] = 0x00;
                        stream.Write(b, 0, b.Length);
                        var commitResult = stream.Commit();
                    }
                    openValidResult.IsWriteAble = true;
                    try
                    {
                        DeleteFile(info);
                        openValidResult.IsDeleteAble = true;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Cannot delete the temp file , ID : {0} and error : {1}", info.ObjectId, e);
                    }
                    openValidResult.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }
            }
            catch (WebException we)
            {
                if (we.Response != null)
                {
                    if ((we.Response as HttpWebResponse).StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                    {
                        logger.Error("Validate Error:{0}", we);
                        openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy_Authentication_Filed", AbstractXSystem.Culture);
                        openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                    }
                    else if ((we.Response as HttpWebResponse).StatusCode == HttpStatusCode.Forbidden || (we.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        logger.Error("Validate Error:{0}", we);
                        openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_AccessTokenNotAvailable", AbstractXSystem.Culture);
                        openValidResult.SystemHealth = XSystemHealth.Unaccessable;
                    }
                    else
                    {
                        logger.Error("Validate Error:{0}", we);
                        openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Test_failed", AbstractXSystem.Culture);
                        openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                    }
                }
                else
                {
                    logger.Error("Validate Error:{0}", we);
                    openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Test_failed", AbstractXSystem.Culture);
                    openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                }
            }
            catch (DeviceNotAvailableException de)
            {
                logger.Error("Validate Error:{0}", de);
                openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_RootFolderNotExist", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
            }
            catch (AuthenticationFailedException ae)
            {
                logger.Error("Validate Error:{0}", ae);
                openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_AccessTokenNotAvailable", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.Unaccessable;
            }
            catch (Exception ex)
            {
                logger.Error("Validate Error:{0}", ex);
                openValidResult.Message = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Test_failed", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = openValidResult.SystemHealth;
            }
            return openValidResult;
        }

        private SpaceInfo CheckFreeSpace()
        {
            var info = new SpaceInfo();
            var request = this.CreateRequestWithToken(StorageUrl.DropboxSpaceUsage, "POST");
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        var jsonText = stream.ReadToEnd();
                        var usageInfo = new JavaScriptSerializer().Deserialize<DropboxUsageInfo>(jsonText);
                        info.TotalSpace = usageInfo.Allocation.Allocated;
                        info.TotalUsedSpace = usageInfo.Used;
                        info.TotalFreeSpace = info.TotalSpace - info.TotalUsedSpace;
                    }
                }
            }
            return info;
        }

        private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, Boolean isOverWrite)
        {
            var rs = new StorageCopyResult();
            try
            {
                if (this.DirectoryExists(sourceFolderInfo))
                {
                    //先copy子文件
                    var listRS = ListSubDirectoriesAndFiles(sourceFolderInfo);
                    foreach (var file in listRS.Files)
                    {
                        var sourceFileInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                        var targetFileInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
                        if (!CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
                        {
                            rs.IsCopyed = false;
                            return rs;
                        }
                    }
                    //遍历文件夹的子文件夹
                    foreach (var directory in listRS.SubDirs)
                    {
                        var sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
                        var targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
                        if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                        {
                            rs.IsCopyed = false;
                            return rs;
                        }
                    }
                    rs.IsCopyed = true;
                }
            }
            catch (Exception ex)
            {
                rs.IsCopyed = false;
                rs.Message = ex.Message;
            }
            return rs;
        }

        private Boolean DeleteDirectory(String systemLocation, String highAndLowName)
        {
            var result = false;
            var url = StorageUrl.DropboxDelete;
            var path = Path.Combine(this.SystemLocation, highAndLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            path = path.StartsWith("/") ? path : "/" + path;
            path = path.EndsWith("/") ? path.TrimEnd('/') : path;
            var param = "{\"path\": \"" + path + "\"}";
            var bytes = Encoding.UTF8.GetBytes(param);
            var request = CreateRequestWithToken(url, "POST");
            request.ContentType = @"application/json";
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        var responseBody = stream.ReadToEnd();
                        result = true;
                    }
                }
            }
            return result;
        }

        private Int64 GetFolderSize(StorageInfo storageInfo, Int64 size)
        {
            var deletedSize = size;
            var result = ListSubDirectoriesAndFiles(storageInfo);
            foreach (var fileInfo in result.Files)
            {
                deletedSize += fileInfo.FileSize;
            }
            foreach (var dirInfo in result.SubDirs)
            {
                dirInfo.HighName = dirInfo.HighName.Trim('/');
                deletedSize = GetFolderSize(dirInfo, deletedSize);
            }
            return deletedSize;
        }

        private void ConvertJsonToList(List<XDirectoryInfo> dirs,
                            List<XFileInfo> files,
                            String jsonText,
                            out Boolean has_more,
                            out String cursor)
        {
            var obj = DropboxUtil.ParseJsonString(jsonText);
            has_more = obj.Has_more;
            cursor = obj.Cursor;
            foreach (var result in obj.Entries)
            {
                var highName = String.Empty;
                var lowName = String.Empty;
                if (result.Path_display != null && result.Path_display.StartsWith("/", StringComparison.OrdinalIgnoreCase) && result.Path_display.IndexOf("/", StringComparison.OrdinalIgnoreCase) == 1)
                {
                    highName = String.Empty;
                    lowName = result.Path_display.Replace("/", String.Empty);
                }
                else
                {
                    if (!"/".Equals(this.SystemLocation, StringComparison.OrdinalIgnoreCase))
                    {
                        var locationPosition = result.Path_display.IndexOf(this.SystemLocation, StringComparison.OrdinalIgnoreCase);
                        result.Path_display = result.Path_display.Substring(0, locationPosition) + result.Path_display.Substring(locationPosition + this.SystemLocation.Length);
                        result.Path_display = result.Path_display.StartsWith("/", StringComparison.OrdinalIgnoreCase) ? result.Path_display : "/" + result.Path_display;
                    }
                    var moveLen = result.Path_display.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) - result.Path_display.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                    if (moveLen == 0)
                    {
                        lowName = result.Path_display.Replace("/", String.Empty);
                    }
                    else
                    {
                        highName = result.Path_display.Substring(result.Path_display.IndexOf("/", StringComparison.OrdinalIgnoreCase), moveLen).Trim('/');
                        lowName = result.Path_display.Substring(result.Path_display.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).Trim('/');
                    }
                }
                if (result.Is_dir)
                    dirs.Add(new DropboxDirectoryInfo(highName, lowName, result.Server_modified));
                else
                    files.Add(new DropboxFileInfo(highName, lowName, (Int64)result.Size, result.Server_modified));
            }
        }

        private DropboxObject GetObjectInfo(StorageInfo dirInfo)
        {
            CheckState();
            var result = String.Empty;
            logger.Info("The HighPlusLowName is: {0}", dirInfo.HighPlusLowName);
            var path = Path.Combine(this.SystemLocation, dirInfo.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            path = path.StartsWith("/") ? path : "/" + path;
            path = path.EndsWith("/") ? path.TrimEnd('/') : path;
            var uriStr = StorageUrl.DropboxMeta;
            var param = "{\"path\":\"" + path + "\",\"include_deleted\":true}";
            logger.Info("The param is: {0}", param);
            Byte[] metadataData = Encoding.UTF8.GetBytes(param);
            try
            {
                var request = CreateRequestWithToken(uriStr, "POST");
                request.ContentType = @"application/json";
                request.ContentLength = metadataData.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(metadataData, 0, metadataData.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new WebException(String.Format("Get directory info failed, StatusCode={0} URL={1}",
                            response.StatusCode, path));
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException e)
            {
                var body = String.Empty;
                var errorSummary = String.Empty;
                var resp = e.Response as HttpWebResponse;
                using (var respStream = resp.GetResponseStream())
                {
                    using (var sr = new StreamReader(respStream))
                    {
                        body = sr.ReadToEnd();
                    }
                    var regexId = new Regex(DropboxConstants.ErrorSummary);
                    var mc = regexId.Matches(body);
                    foreach (var m in mc)
                    {
                        var temp = m.ToString().Split(':');
                        errorSummary = temp[1].Trim(new Char[] { '\\', '\"' }).Substring(2);
                    }
                }
                logger.Error("Get object info failed, msg:{0}, response body:{1}:", e, body);
                if (errorSummary.Contains("path/not_found"))
                    throw new PathNotFoundException(e.Message, e);
                throw;
            }
            return DropboxUtil.ParseJsonString(result);
        }

        private void CreateDirectory(StorageInfo info)
        {
            var url = StorageUrl.DropboxCreateFolder;
            logger.Info("The HighPlusLowName is: {0}", info.HighPlusLowName);
            var path = Path.Combine(this.SystemLocation, info.HighPlusLowName.TrimStart(new char[] { '\\', '/' })).Replace("\\", "/");
            path = path.StartsWith("/") ? path : "/" + path;
            path = path.EndsWith("/") ? path.TrimEnd('/') : path;
            var param = "{\"path\":\"" + path + "\"}";
            logger.Info("The param is: {0}", param);
            var bytes = Encoding.UTF8.GetBytes(param);
            var request = CreateRequestWithToken(url, "POST");
            request.ContentLength = bytes.Length;
            request.ContentType = @"application/json";
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new WebException(String.Format("Create directory failed, StatusCode={0} URL={1}",
                        response.StatusCode, url));
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }
            }
        }

        internal HttpWebRequest CreateRequestWithToken(String url, String method, String param = null, StorageInfo storage = null)
        {
            url = url.Replace("\\", "/");
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            request.Headers.Add("Authorization", String.Format("Bearer {0}", this.accessToken));
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;
                if (request.Proxy.Credentials != null)
                {
                    request.PreAuthenticate = true;
                }
            }
            if (storage != null && storage.Offset > 0)
            {
                var methodinfo = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                   BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                   new Type[] { typeof(String), typeof(String) }, null);
                methodinfo.Invoke(request.Headers, new Object[] { "Range", "bytes=" + storage.Offset + "-" + (storage.Length + storage.Offset) });
            }
            if (param != null)
            {
                var methodinfo = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                   BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                   new Type[] { typeof(String), typeof(String) }, null);
                methodinfo.Invoke(request.Headers, new Object[] { "Dropbox-API-Arg", param });
            }
            return request;
        }
    }
}
