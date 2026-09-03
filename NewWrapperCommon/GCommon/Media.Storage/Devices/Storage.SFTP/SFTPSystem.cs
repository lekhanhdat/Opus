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
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPSystem.#Validate()", MessageId = "sftp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPSystem.#SetSystemDescription()", MessageId = "sftp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPSystem.#Open()", MessageId = "sftp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPSystem.#Close()", MessageId = "sftp")]
namespace AvePoint.Media.Storage.SFTP
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using Renci.SshNet.Common;
    using AvePoint.Media.Storage.Resources.SFTPI18N;
    using System.Threading;

    #endregion using directives

    class SFTPSystem : AbstractXSystem
    {
        private SFTPNodeInfo nodeInfo;
        private SFTPClient sftpClientNOTUsedDirectly;
        private StorageLogger logger = new StorageLogger(typeof(SFTPSystem));
        private string rootFolder;
        private bool isOpenConnection = false;
        private object checkLocker = new object();

        private SFTPClient SFTPClient
        {
            get
            {
                if (!isOpenConnection)
                {
                    lock (checkLocker)
                    {
                        if (!isOpenConnection)
                        {
                            if (sftpClientNOTUsedDirectly == null)
                                sftpClientNOTUsedDirectly = new SFTPClient();
                            sftpClientNOTUsedDirectly.Open(nodeInfo, rootFolder);
                            isOpenConnection = true;
                        }
                    }
                }
                return this.sftpClientNOTUsedDirectly;
            }
        }

        public SFTPSystem(string xri, AbstractXSystem parentSystem)
            : base(xri, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            SystemHealth = XSystemHealth.Unknown;
        }

        public override StorageOpenValidResult Open()
        {
            StorageOpenValidResult result = new StorageOpenValidResult();
            try
            {
                nodeInfo = new SFTPNodeInfo();
                sftpClientNOTUsedDirectly = new SFTPClient();
                isOpenConnection = false;
                base.Open();
                var parms = XriObject.Params;
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_HOST))
                {
                    nodeInfo.HostName = parms[SFTPXRIParameterKeys.SFTP_HOST];
                }
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_RootFolder))
                {
                    rootFolder = parms[SFTPXRIParameterKeys.SFTP_RootFolder];
                }
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_PORT))
                {
                    nodeInfo.Port = Convert.ToInt32(parms[SFTPXRIParameterKeys.SFTP_PORT]);
                }
                if (parms.ContainsKey(XRIParameterKeys.USERNAME_KEY))
                {
                    nodeInfo.UserName = parms[XRIParameterKeys.USERNAME_KEY];
                }
                if (parms.ContainsKey(XRIParameterKeys.PASSWORD_KEY))
                {
                    nodeInfo.Password = SecretUtil.DescryptPassword(parms[XRIParameterKeys.PASSWORD_KEY]);
                }
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_PRIVATE_KEY))
                {
                    nodeInfo.PrivateKey = SecretUtil.DescryptPassword(parms[SFTPXRIParameterKeys.SFTP_PRIVATE_KEY]);
                }
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_PRIVATE_KEY_PASSWORD))
                {
                    nodeInfo.PrivateKeyPassword = SecretUtil.DescryptPassword(parms[SFTPXRIParameterKeys.SFTP_PRIVATE_KEY_PASSWORD]);
                }
                if (parms.ContainsKey(SFTPXRIParameterKeys.SFTP_BufferSize))
                {
                    nodeInfo.PrivateKeyPassword = SecretUtil.DescryptPassword(parms[SFTPXRIParameterKeys.SFTP_BufferSize]);
                }
                this.IsRetry = true;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                this.SystemLocation = String.Format("sftp://{0}:{1}/{2}", this.nodeInfo.HostName, this.nodeInfo.Port, this.rootFolder);
                Type = "SFTPSystem";
                //this.TypeValue = 12;
                logger.Info("open a sftp system, host:{0}, port:{1}, user name:{2}", nodeInfo.HostName, nodeInfo.Port, nodeInfo.UserName);
            }
            catch (Exception ex)
            {
                logger.Error("open sftp system failed:{0}", ex.Message, ex);
                this.SystemHealth = XSystemHealth.Unaccessable;
                result.Message = ex.Message;
                throw;
            }
            this.SetSystemDescription();
            return result;
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "SFTP, Location : sftp://" + nodeInfo.HostName + ":" + nodeInfo.Port + ", Username: " + nodeInfo.UserName;
            var keys = new List<String>();
            keys.Add(this.nodeInfo.HostName);
            keys.Add(this.nodeInfo.Port.ToString());
            keys.Add(this.nodeInfo.UserName);
            var securityKeys = new List<string>();
            securityKeys.Add(this.nodeInfo.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageOpenValidResult Validate()
        {
            this.CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            var result = new StorageOpenValidResult();
            try
            {
                logger.Info("begin validate sftp system");
                if (!string.IsNullOrEmpty(rootFolder))
                {
                    if (!SFTPClient.CheckDirectory(""))
                    {
                        SFTPClient.MakeRootFolder(rootFolder);
                        if (!SFTPClient.CheckDirectory(""))
                        {
                            throw new FileNotFoundException("the root folder don't exist");
                        }
                    }
                }
                string tempFileForValidate = System.Guid.NewGuid().ToString() + "_DocAve.tmp";
                MemoryStream localStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tempFileForValidate));
                if (SFTPClient.StoreFile(tempFileForValidate, localStream))
                {
                    result.IsWriteAble = true;
                }
                if (!string.IsNullOrEmpty(tempFileForValidate) && SFTPClient.CheckFileExist(tempFileForValidate))
                {
                    if (SFTPClient.DeleteFile(tempFileForValidate))
                    {
                        result.IsReadAble = true;
                        result.IsDeleteAble = true;
                    }
                }
                result.TotalSpace = long.MaxValue;
                result.TotalFreeSpace = long.MaxValue;
                result.TotalUsedSpace = 0;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                result.SystemHealth = XSystemHealth.AvailableAndNotFull;
                logger.Info("validate sftp system succeed");
            }
            catch (Exception e)
            {
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage("sftp://" + nodeInfo.HostName, ContextValues.Storage.StorageType.SFTP, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.SFTP, verifyFailedEventMessage);
                var errorMessage = String.Empty;
                if (e is SocketException)
                {
                    //unplug network reason
                    errorMessage = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture);
                }
                if (e is SshAuthenticationException)
                {
                    errorMessage = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Authentication_failed", AbstractXSystem.Culture);
                }
                logger.Error(e.Message, e);
                result.SystemHealth = XSystemHealth.ConnectedFailed;
                if (errorMessage.Equals(string.Empty))
                {
                    errorMessage = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Test_failed", AbstractXSystem.Culture);
                }
                result.Message = errorMessage;
            }
            this.SystemHealth = result.SystemHealth;
            return result;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
            }
            this.CheckState();
            SFTPStream stream = null;
            try
            {
                nodeInfo.HighName = info.HighName;
                nodeInfo.FileName = info.LowName;
                nodeInfo.Offset = info.Offset;
                stream = new SFTPStream(SFTPClient, info, this);
                stream.InitStream(fileMode);
            }
            catch (Exception e)
            {
                if (fileMode.Equals(FileMode.Open))
                {
                    this.logger.Error("Opened the data failed, path: {0}.", PathUtil.CombinePath(info.HighName, info.LowName));
                }
                logger.Error(e.Message, e);
                throw;
            }
            return stream;
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            if (!SFTPClient.CheckDirectory(directoryPath))
            {
                if (mode == FileMode.Create || mode == FileMode.OpenOrCreate)
                {
                    SFTPClient.MakeDirectory(directoryPath);
                }
                else
                {
                    return null;
                }
            }
            XDirectoryInfo dInfo = new SFTPDirectory(directoryPath, dirInfo.LowName);

            ((SFTPDirectory)dInfo).IsExist = true;
            return dInfo;
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            this.CheckState();
            logger.Info("check directory, path:" + info.HighPlusLowName);
            var result = false;
            var directoryPath = info.HighName;
            if (!String.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            result = SFTPClient.CheckDirectory(directoryPath);
            return result;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            this.CheckState();
            logger.Info("list files, path:" + dirInfo.HighPlusLowName);
            var fileInfos = new List<XFileInfo>();
            var directoryPath = dirInfo.HighName;
            if (!String.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            var files = SFTPClient.ListDirectoryAndFiles(directoryPath);
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (!file.IsDirectory)
                    {
                        long size = 0;//SFTPClient.GetFileSize(PathUtil.CombinePath(directoryPath, file.Name));
                        fileInfos.Add(new SFTPFileInfo(directoryPath, file.Name, size));
                    }
                }
            }
            return fileInfos;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            this.CheckState();
            var result = new StorageListResult();
            logger.Info("list files and directory, path:" + dirInfo.HighPlusLowName);
            var directoryPath = dirInfo.HighName;
            if (!String.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            result.Files = new List<XFileInfo>();
            result.SubDirs = new List<XDirectoryInfo>();
            var files = SFTPClient.ListDirectoryAndFiles(directoryPath);
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (file.IsDirectory)
                    {
                        result.SubDirs.Add(new SFTPDirectory(directoryPath, file.Name));
                    }
                    else
                    {
                        var size = SFTPClient.GetFileSize(PathUtil.CombinePath(directoryPath, file.Name));
                        result.Files.Add(new SFTPFileInfo(directoryPath, file.Name, size));
                    }
                }
            }
            return result;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            this.CheckState();
            logger.Info("list directory, path:" + dirInfo.HighPlusLowName);
            var directoryPath = dirInfo.HighName;
            if (!String.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            var directoryInfos = new List<XDirectoryInfo>();
            var dirNames = SFTPClient.ListDirectoryAndFiles(directoryPath);
            if (dirNames != null && dirNames.Length > 0)
            {
                foreach (var dir in dirNames)
                {
                    if (dir.IsDirectory)
                    {
                        directoryInfos.Add(new SFTPDirectory(directoryPath, dir.Name));
                    }
                }
            }
            return directoryInfos;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            this.CheckState();
            logger.Info("delete file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
            var result = new StorageDeleteResult();
            var filePath = string.Empty;
            if (!String.IsNullOrEmpty(info.HighName) || !String.IsNullOrEmpty(info.LowName))
            {
                filePath = PathUtil.CombinePath(info.HighName, info.LowName);
                if (!SFTPClient.CheckFileExist(filePath))
                {
                    result.IsDeleted = true;
                    return result;
                }
                else
                {
                    result.DeletedFileSize = SFTPClient.GetFileSize(filePath);
                    if (SFTPClient.DeleteFile(filePath))
                    {
                        result.IsDeleted = true;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("file high name or low name is null or empty");
            }
            //标记执行过删除
            Deletion = true;
            return result;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            this.CheckState();
            var result = new StorageDeleteResult();
            var directoryPath = info.HighName;
            if (!String.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            logger.Info("delete directory, path:" + directoryPath);
            result = SFTPClient.DeleteDiectory(directoryPath, null);
            logger.Info("delete directory done");
            //标记执行过删除
            Deletion = true;
            result.IsDeleted = true;
            return result;
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            this.CheckState();
            XFileInfo result = null;
            if (SFTPClient.CheckFileExist(fileInfo.HighPlusLowName))
            {
                var length = SFTPClient.GetFileSize(PathUtil.CombinePath(fileInfo.HighName, fileInfo.LowName));
                var lastModifyTime = SFTPClient.GetLastModifiedTime(PathUtil.CombinePath(fileInfo.HighName, fileInfo.LowName));
                result = new SFTPFileInfo(fileInfo.HighName, fileInfo.LowName, length, lastModifyTime);
                logger.Info("open file, path:" + PathUtil.CombinePath(fileInfo.HighName, fileInfo.LowName) + ",length:" + length);
            }
            else
            {
                logger.Debug("can't find file :" + fileInfo.HighPlusLowName + ", return null");
            }
            return result;
        }

        public override bool FileExists(StorageInfo info)
        {
            this.CheckState();
            var result = SFTPClient.CheckFileExist(PathUtil.CombinePath(info.HighName, info.LowName));
            logger.Info("check file:" + PathUtil.CombinePath(info.HighName, info.LowName) + ",result:" + result);
            return result;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            this.CheckState();
            var rs = new StorageCopyResult();
            XStream sourceStream = null;
            XStream destStream = null;
            //同名则返回
            try
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
                    sourceStream = this.OpenStream(sourceFileInfo, FileMode.Open);
                    targetFileInfo.Length = sourceStream.Length;
                    destStream = this.OpenStream(targetFileInfo, FileMode.Create);
                    var buffer = new byte[64 * 1024];
                    while (true)
                    {
                        var readLength = sourceStream.Read(buffer, 0, buffer.Length);
                        if (readLength <= 0)
                            break;
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
            var moveRS = new StorageMoveResult();
            try
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
            }
            catch (System.Exception ex)
            {
                moveRS.IsMoved = false;
                moveRS.Message = ex.Message;
            }
            return moveRS;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            var moveRS = new StorageMoveResult();
            try
            {
                var copyRS = this.CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
                if (copyRS.IsCopyed)
                {
                    this.DeleteDirectory(sourceDirInfo);
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

        private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, bool isOverWrite)
        {
            var result = new StorageCopyResult();
            try
            {
                //if (client.CheckObject(SystemLocation, PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName)))
                if (this.DirectoryExists(sourceFolderInfo))
                {
                    //先copy子文件
                    var listRS = this.ListSubDirectoriesAndFiles(sourceFolderInfo);
                    foreach (var file in listRS.Files)
                    {
                        var sourceFileInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                        var targetFileInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
                        if (!this.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
                        {
                            result.IsCopyed = false;
                            return result;
                        }
                    }
                    //遍历文件夹的子文件夹
                    foreach (var directory in listRS.SubDirs)
                    {
                        var sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
                        var targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
                        if (!this.CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                        {
                            result.IsCopyed = false;
                            return result;
                        }
                    }
                    result.IsCopyed = true;
                }
            }
            catch (System.Exception ex)
            {
                result.IsCopyed = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public override void Close()
        {
            logger.Info("close sftp connection");
            if (sftpClientNOTUsedDirectly != null)
            {
                try
                {
                    sftpClientNOTUsedDirectly.Close();
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                }
                finally
                {
                    sftpClientNOTUsedDirectly = null;
                }
            }
        }

        public DateTime GetDirectoryLastModifiedTime(string path)
        {
            return SFTPClient.GetLastModifiedTime(path);
        }

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            CheckState();
            while (true)
            {
                var storageResult = new StorageResult();
                var tempInfo = info.Clone();
                try
                {
                    commitStream.Position = 0;
                    if (info.NeedRenameIndexName)
                    {
                        tempInfo.LowName = "temp" + tempInfo.LowName;
                    }
                    this.logger.Debug("commit file:{0}", info.HighPlusLowName);
                    var buffer = new byte[64 * 1024];
                    using (var stream = OpenStream(tempInfo, FileMode.Create))
                    {
                        stream.IsCommitStream = true;
                        int readLen;
                        while ((readLen = commitStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, readLen);
                        }
                        storageResult = stream.Commit(tempInfo.IsClosing);
                        storageResult.URI = stream.GetURI();
                        storageResult.IsCommited = true;
                    }
                    if (info.NeedRenameIndexName)
                    {
                        var fileInfo = OpenFile(tempInfo.Clone());
                        if (fileInfo != null && fileInfo.FileSize == commitStream.Length)
                        {
                            SFTPClient.RenameFileName(info, tempInfo, true);
                        }
                        else
                        {
                            this.logger.Error(String.Format("An error occurred while commit stream,commit file failed."));
                            throw new Exception(String.Format("An error occurred while commit stream,commit file failed."));
                        }
                    }
                    this.Written = true;
                    return storageResult;
                }
                catch (Exception e)
                {
                    if (e is SshOperationTimeoutException)
                    {
                        this.SFTPClient.ReopenConnection();
                    }
                    var fileInfo = OpenFile(info.Clone());
                    if (fileInfo != null && fileInfo.FileSize == commitStream.Length)
                    {
                        this.logger.Warn("An error occurred while commit stream, but the file upload successfully, so that will go on.");
                        if (info.NeedRenameIndexName)
                        {
                            SFTPClient.RenameFileName(info, tempInfo, true);
                        }
                        this.Written = true;
                        return storageResult;
                    }
                    else
                    {
                        this.logger.Error("commit file {0} failed:{1}", info.HighPlusLowName, e.Message);
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
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}