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



namespace AvePoint.Media.Storage.FTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Resources.FTPI18N;
    using AvePoint.Media.Storage.Util;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/29",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_1 },
    "ADO-26069",
    true)]
    #endregion

    class FTPSystem : AbstractXSystem
    {
        private FTPNodeInfo nodeInfo;
        private FtpClient ftpClient;
        private StorageLogger logger = new StorageLogger(typeof(FTPSystem));
        protected string innerRootFolder;
        public string RootFolder
        {
            get
            {
                return this.innerRootFolder;
            }
            set
            {
                this.innerRootFolder = value;
            }
        }

        public FTPSystem(string xri, AbstractXSystem parentSystem)
            : base(xri, parentSystem)
        {
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.ServerCertificateValidationCallback =
                        new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            SystemHealth = XSystemHealth.Unknown;
            this.Open();
        }
        public static Boolean CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            StorageOpenValidResult sr = new StorageOpenValidResult();
            try
            {
                nodeInfo = new FTPNodeInfo();
                ftpClient = new FtpClient();
                base.Open();
                Dictionary<string, string> parms = XriObject.Params;
                if (parms.ContainsKey(XRIParameterKeys.FTP_HOST))
                {
                    nodeInfo.HostName = parms[XRIParameterKeys.FTP_HOST];
                }
                if (parms.ContainsKey(XRIParameterKeys.FTP_RootFolder))
                {
                    innerRootFolder = parms[XRIParameterKeys.FTP_RootFolder];
                }
                if (parms.ContainsKey(XRIParameterKeys.FTP_PORT))
                {
                    nodeInfo.Port = Convert.ToInt32(parms[XRIParameterKeys.FTP_PORT]);
                }
                if (parms.ContainsKey(XRIParameterKeys.USERNAME_KEY))
                {
                    nodeInfo.UserName = parms[XRIParameterKeys.USERNAME_KEY];
                }
                if (parms.ContainsKey(XRIParameterKeys.PASSWORD_KEY))
                {
                    nodeInfo.Password = SecretUtil.DescryptPassword(parms[XRIParameterKeys.PASSWORD_KEY]);
                }
                if (parms.ContainsKey(XRIParameterKeys.FTP_SCHEMA))
                {
                    string schema = parms[XRIParameterKeys.FTP_SCHEMA];

                    if (!string.IsNullOrEmpty(schema) && (schema.Equals(FtpSchema.Ftp.ToString(), StringComparison.OrdinalIgnoreCase) || schema.Equals(FtpSchema.Ftps.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        nodeInfo.Schema = schema;
                    }
                    else
                    {
                        nodeInfo.Schema = FtpSchema.Ftp.ToString();
                    }
                }
                else
                {
                    nodeInfo.Schema = FtpSchema.Ftp.ToString();
                }
                if (parms.ContainsKey(XRIParameterKeys.FTPTypekey))
                {
                    nodeInfo.FtpType = parms[XRIParameterKeys.FTPTypekey];
                }
                if (parms.ContainsKey(XRIParameterKeys.RETRY_COUNT))
                {
                    nodeInfo.MaxRetryCount = int.Parse(parms[XRIParameterKeys.RETRY_COUNT]);
                }
                if (parms.ContainsKey(XRIParameterKeys.RETRY_INTERVAL))
                {
                    nodeInfo.RetryInternal = int.Parse(parms[XRIParameterKeys.RETRY_INTERVAL]);
                }
                if (parms.ContainsKey(XRIParameterKeys.IS_RETRY))
                {
                    nodeInfo.IsRetry = Boolean.Parse(parms[XRIParameterKeys.IS_RETRY]);
                }
                if (parms.ContainsKey(XRIParameterKeys.FTP_USEPASSIVE))
                {
                    nodeInfo.UsePassive = Boolean.Parse(parms[XRIParameterKeys.FTP_USEPASSIVE]);
                }
                if (parms.ContainsKey(XRIParameterKeys.FTP_USEFLUENTFTP))
                {
                    nodeInfo.UseFluentFTP = Boolean.Parse(parms[XRIParameterKeys.FTP_USEFLUENTFTP]);
                }
                //ADO-197324, poc 客户临时配置,merge到trunk需要界面配置相关选项.
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                this.SystemLocation = Uri.UnescapeDataString(new UriBuilder(nodeInfo.Schema, nodeInfo.HostName, nodeInfo.Port, innerRootFolder).ToString());
                ftpClient.Open((FtpSchema)Enum.Parse(typeof(FtpSchema), nodeInfo.Schema, true), nodeInfo.HostName, nodeInfo.Port, nodeInfo.UserName, nodeInfo.Password, innerRootFolder, nodeInfo.FtpType, nodeInfo.IsRetry, nodeInfo.MaxRetryCount, nodeInfo.RetryInternal, nodeInfo.UsePassive, nodeInfo.UseFluentFTP);
                Type = "FTPSystem";
                logger.Info("open a ftp system, host:{0}, port:{1}, user name:{2}, use passive:{3}, Schema:{4}, root folder:{5}, use Fluent FTP{6}", nodeInfo.HostName, nodeInfo.Port, nodeInfo.UserName, nodeInfo.UsePassive, nodeInfo.Schema, innerRootFolder, nodeInfo.UseFluentFTP);
            }
            catch (Exception ex)
            {
                logger.Error("open ftp system failed:{0}", ex.Message);
                this.SystemHealth = XSystemHealth.Unaccessable;
                sr.Message = ex.Message;
                throw;
            }
            SetSystemDescription();
            return sr;

        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "FTP, Location : " + nodeInfo.Schema + "://" + nodeInfo.HostName + ":" + nodeInfo.Port + ", Username: " + nodeInfo.UserName;
            List<string> keys = new List<string>();
            keys.Add(this.nodeInfo.HostName);
            keys.Add(this.nodeInfo.Port.ToString());
            keys.Add(this.nodeInfo.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.nodeInfo.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageOpenValidResult Validate()
        {
            if (nodeInfo.UseFluentFTP)
            {
                return this.SimpleValidate();
            }
            else
            {
                return this.NormalValidate();
            }
        }

        public StorageOpenValidResult SimpleValidate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult result = new StorageOpenValidResult();
            try
            {
                logger.Info("begin check ftp connection");

                ftpClient.CheckConnection();
                result.SystemHealth = XSystemHealth.AvailableAndNotFull;

                logger.Info("check ftp connection succeed");
            }
            catch (Exception e)
            {
                string errorMessage = string.Empty;
                result.SystemHealth = XSystemHealth.ConnectedFailed;
                if (e is System.TimeoutException | e is System.Net.Sockets.SocketException)
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture);
                }
                if (e is Wrapper.FtpCommandException && ((Wrapper.FtpCommandException)e).CompletionCode.Equals("530"))
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Authentication_failed", AbstractXSystem.Culture);
                    result.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
                logger.Error(e.Message, e);
                if (errorMessage.Equals(string.Empty))
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Test_failed", AbstractXSystem.Culture);
                }
                result.Message = errorMessage;
            }
            this.SystemHealth = result.SystemHealth;
            return result;
        }

        public StorageOpenValidResult NormalValidate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult result = new StorageOpenValidResult();
            try
            {
                logger.Info("begin validate ftp system");
                if (!nodeInfo.UseFluentFTP && !string.IsNullOrEmpty(innerRootFolder))
                {
                    if (!ftpClient.CheckDirectory(string.Empty))
                    {
                        ftpClient.CreateDirectory(String.Empty);
                        //throw new FileNotFoundException("the root folder don't exist");
                    }
                }
                string tempFolderForValidate = System.Guid.NewGuid().ToString() + "_DocAve";
                string tempFileForValidate = System.Guid.NewGuid().ToString() + "_DocAve.tmp";
                MemoryStream localStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tempFileForValidate));
                if(nodeInfo.UseFluentFTP)
                {
                    tempFileForValidate = "DocAve.tmp";
                    if (ftpClient.StoreFile(tempFileForValidate, localStream))
                    {
                        result.IsWriteAble = true;
                    }
                }
                else
                {
                    if (ftpClient.CreateDirectory(tempFolderForValidate))
                    {
                        if (ftpClient.StoreFile(tempFileForValidate, localStream))
                        {
                            result.IsWriteAble = true;
                        }
                        if (ftpClient.CheckDirectory(tempFolderForValidate) == false)
                        {
                            throw new WebException("The remote server returned an error: (550) File unavailable (e.g., file not found, no access).");
                        }
                    }
                }

                if(!nodeInfo.UseFluentFTP)
                {
                    if (!string.IsNullOrEmpty(tempFileForValidate) && ftpClient.CheckFile(tempFileForValidate) && !string.IsNullOrEmpty(tempFolderForValidate))
                    {
                        if (ftpClient.DeleteFile(tempFileForValidate))
                        {
                            if (ftpClient.DeleteDirectory(tempFolderForValidate).IsDeleted == true)
                            {
                                result.IsReadAble = true;
                                result.IsDeleteAble = true;
                            }
                        }
                    }
                }
                //If need validate delete file permission,use logic below.
                //if (nodeInfo.IsReplicatorDevice)
                //{
                //    if (result.IsWriteAble && ftpClient.DeleteFile(tempFileForValidate))
                //    {
                //        result.IsReadAble = true;
                //        result.IsDeleteAble = true;
                //    }
                //}
                //else if(ftpClient.CheckFile(tempFileForValidate) && ftpClient.DeleteFile(tempFileForValidate) && ftpClient.DeleteDirectory(tempFolderForValidate).IsDeleted)
                //{
                //    result.IsReadAble = true;
                //    result.IsDeleteAble = true;
                //}
                result.TotalSpace = long.MaxValue;
                result.TotalFreeSpace = long.MaxValue;
                result.TotalUsedSpace = 0;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                result.SystemHealth = XSystemHealth.AvailableAndNotFull;
                logger.Info("validate ftp system succeed");
            }
            catch (WebException e)
            {
                logger.Error("FtpSystem Validate failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error(e.Message, e);
            }
            catch (Exception e)
            {
                EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(nodeInfo.Schema + "://" + nodeInfo.HostName, ContextValues.Storage.StorageType.FTP, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.FTP, verifyFailedEventMessage);
                string errorMessage = string.Empty;
                result.SystemHealth = XSystemHealth.ConnectedFailed;
                if (e is System.Net.WebException && e.Message.Contains("Unable to connect to the remote server"))
                {
                    //unplug network reason
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture);
                }
                if (e.GetHashCode().Equals(530))
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Authentication_failed", AbstractXSystem.Culture);
                    result.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
                if (e.Message.Contains("The remote server returned an error: (530) Not logged in."))
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Authentication_failed", AbstractXSystem.Culture);
                    result.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
                logger.Error(e.Message, e);
                if (errorMessage.Equals(string.Empty))
                {
                    errorMessage = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Test_failed", AbstractXSystem.Culture);
                    if(nodeInfo.UseFluentFTP)
                    {
                        result.SystemHealth = XSystemHealth.AvailableAndNotFull;
                        errorMessage = string.Empty;
                    }
                }
                result.Message = errorMessage;
            }
            this.SystemHealth = result.SystemHealth;
            return result;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            if (fileMode == FileMode.Append || fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
            }
            CheckState();
            FTPStream stream = null;
            try
            {
                nodeInfo.HighName = info.HighName;
                nodeInfo.FileName = info.LowName;
                nodeInfo.Offset = info.Offset;
                stream = new FTPStream(ftpClient, info, this);
                stream.InitStream(fileMode);
            }
            catch (WebException e)
            {
                logger.Error("FtpSystem OpenStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                throw;
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
            if (!ftpClient.CheckDirectory(directoryPath))
            {
                if (mode == FileMode.Create || mode == FileMode.OpenOrCreate)
                {
                    ftpClient.MakeDirectory(directoryPath);
                }
                else
                {
                    return null;
                }
            }
            XDirectoryInfo dInfo = new FTPDirectory(directoryPath, dirInfo.LowName);
            //directoryPath = directoryPath.TrimEnd(new char['/']);
            //string folder = string.Empty;
            //string parentPath = string.Empty;
            //if (directoryPath.Contains("/"))
            //{
            //    folder = directoryPath.Substring(directoryPath.LastIndexOf("/") + 1);
            //    parentPath = directoryPath.Substring(0, directoryPath.LastIndexOf("/"));
            //}
            //else
            //{
            //    folder = directoryPath;
            //    parentPath = string.Empty;
            //}
            //FileStruct[] subFiles = ftpClient.ListDirectoryAndFiles(parentPath);
            //foreach (FileStruct file in subFiles)
            //{
            //    if (file.IsDirectory && file.Name.Equals(folder, StringComparison.OrdinalIgnoreCase))
            //    {
            //        dInfo.LastWriteTime = file.CreateTime;
            //        break;
            //    }
            //}
            ((FTPDirectory)dInfo).IsExist = true;
            return dInfo;
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            CheckState();
            logger.Info("check directory, path:" + info.HighPlusLowName);
            bool result = false;
            string directoryPath = info.HighName;
            if (!string.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            result = ftpClient.CheckDirectory(directoryPath);
            return result;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            CheckState();
            logger.Info("list files, path:" + dirInfo.HighPlusLowName);
            List<XFileInfo> rs = new List<XFileInfo>();
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            FileStruct[] files = ftpClient.ListDirectoryAndFiles(directoryPath);
            if (files != null && files.Length > 0)
            {
                foreach (FileStruct file in files)
                {
                    if (!file.IsDirectory)
                    {
                        long size = ftpClient.GetFileSize(PathUtil.CombinePath(directoryPath, file.Name));
                        rs.Add(new FTPFileInfo(directoryPath, file.Name, size));
                    }
                }
            }
            return rs;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            CheckState();
            StorageListResult sr = new StorageListResult();
            logger.Info("list files and directory, path:" + dirInfo.HighPlusLowName);
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }

            sr.Files = new List<XFileInfo>();
            sr.SubDirs = new List<XDirectoryInfo>();

            FileStruct[] files = ftpClient.ListDirectoryAndFiles(directoryPath);
            if (files != null && files.Length > 0)
            {
                foreach (FileStruct file in files)
                {
                    if (!file.IsDirectory)
                    {
                        long size = ftpClient.GetFileSize(PathUtil.CombinePath(directoryPath, file.Name));
                        sr.Files.Add(new FTPFileInfo(directoryPath, file.Name, size));
                    }
                    else
                    {
                        sr.SubDirs.Add(new FTPDirectory(directoryPath, file.Name));
                    }
                }
            }
            return sr;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            CheckState();
            logger.Info("list directory, path:" + dirInfo.HighPlusLowName);
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            List<XDirectoryInfo> rs = new List<XDirectoryInfo>();
            FileStruct[] dirNames = ftpClient.ListDirectoryAndFiles(directoryPath);
            if (dirNames != null && dirNames.Length > 0)
            {
                foreach (FileStruct dir in dirNames)
                {
                    if (dir.IsDirectory)
                    {
                        rs.Add(new FTPDirectory(directoryPath, dir.Name));
                    }
                }
            }
            return rs;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            logger.Info("delete file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
            StorageDeleteResult rs = new StorageDeleteResult();
            string filePath = string.Empty;
            if (!string.IsNullOrEmpty(info.HighName) || !string.IsNullOrEmpty(info.LowName))
            {
                filePath = PathUtil.CombinePath(info.HighName, info.LowName);
                if (!ftpClient.CheckFile(filePath))
                {
                    rs.IsDeleted = true;
                    return rs;
                }
                else
                {
                    rs.DeletedFileSize = ftpClient.GetFileSize(filePath);
                    if (ftpClient.DeleteFile(filePath))
                    {
                        rs.IsDeleted = true;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("file high name or low name is null or empty");
            }
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            var rs = new StorageDeleteResult();
            var directoryPath = info.HighName;
            if (!string.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            if (ftpClient.CheckDirectory(directoryPath))
            {
                logger.Info("delete directory, path:" + directoryPath);
                rs = ftpClient.DeleteDirectory(directoryPath);
                if (info.IsDeleteParentFolder)
                {
                    var directoryNames = directoryPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    var directoryPaths = new List<String>();
                    for (int i = 0; i < directoryNames.Length - 1; i++)
                    {
                        directoryNames[i] = directoryNames[i].TrimEnd('\\').TrimEnd('/') + "\\";
                        if (i == 0)
                        {
                            directoryPaths.Add(directoryNames[i]);
                        }
                        else
                        {
                            directoryPaths.Add(PathUtil.CombinePath(directoryPaths[i - 1], directoryNames[i]));
                        }
                    }
                    for (int index = directoryPaths.Count - 1; index >= 0; index--)
                    {
                        var path = directoryPaths[index];
                        if (ftpClient.ListDirectoryAndFiles(path) == null)
                        {
                            ftpClient.DeleteDirectory(path);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                logger.Warn("The directory you want to delete is not exist, path:{0}", directoryPath);
                rs.IsDeleted = true;
            }
            Deletion = true;
            return rs;
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            FTPFileInfo result = null;
            if (ftpClient.CheckFile(fileInfo.HighPlusLowName))
            {
                long length = ftpClient.GetFileSize(PathUtil.CombinePath(fileInfo.HighName, fileInfo.LowName));
                DateTime lastModifyTime = ftpClient.GetLastModifiedTime(PathUtil.CombinePath(fileInfo.HighName, fileInfo.LowName));
                result = new FTPFileInfo(fileInfo.HighName, fileInfo.LowName, length, lastModifyTime);
                result.System = this;
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
            CheckState();
            bool result = ftpClient.CheckFile(PathUtil.CombinePath(info.HighName, info.LowName));
            logger.Info("check file:" + PathUtil.CombinePath(info.HighName, info.LowName) + ",result:" + result);
            return result;
        }


        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState();
            StorageCopyResult rs = new StorageCopyResult();
            XStream sourceStream = null;
            XStream destStream = null;
            //同名则返回
            try
            {
                if (FileExists(sourceFileInfo))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (FileExists(targetFileInfo) && !isOverWrite)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                    }
                    sourceStream = OpenStream(sourceFileInfo, FileMode.Open);
                    targetFileInfo.Length = sourceStream.Length;
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
            catch (WebException e)
            {
                rs.Message = e.Message;
                rs.IsCopyed = false;
                logger.Error("FtpSystem CopyFile failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("copy file failed:" + e.Message);
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
                    DeleteFile(sourceFileInfo);
                }
                else
                {
                    moveRS.IsMoved = false;
                    moveRS.Message = copyRS.Message;
                }
            }
            catch (Exception ex)
            {
                moveRS.IsMoved = false;
                moveRS.Message = ex.Message;
            }
            return moveRS;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {

            StorageMoveResult moveRS = new StorageMoveResult();
            try
            {
                StorageCopyResult copyRS = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
                if (copyRS.IsCopyed)
                {
                    DeleteDirectory(sourceDirInfo);
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
            StorageCopyResult rs = new StorageCopyResult();
            try
            {
                //if (client.CheckObject(SystemLocation, PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName)))
                if (DirectoryExists(sourceFolderInfo))
                {
                    //先copy子文件
                    StorageListResult listRS = ListSubDirectoriesAndFiles(sourceFolderInfo);
                    foreach (XFileInfo file in listRS.Files)
                    {
                        StorageInfo sourceFileInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                        StorageInfo targetFileInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
                        if (!CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
                        {
                            rs.IsCopyed = false;
                            return rs;
                        }
                    }
                    //遍历文件夹的子文件夹
                    foreach (XDirectoryInfo directory in listRS.SubDirs)
                    {
                        StorageInfo sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
                        StorageInfo targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
                        if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                        {
                            rs.IsCopyed = false;
                            return rs;
                        }
                    }
                    rs.IsCopyed = true;
                }
            }
            catch (System.Exception ex)
            {
                rs.IsCopyed = false;
                rs.Message = ex.Message;
            }
            return rs;
        }

        public override void Close()
        {
            if (ftpClient != null)
            {
            }
        }

        public DateTime GetDirectoryLastModifiedTime(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Equals("/", StringComparison.OrdinalIgnoreCase) || path.Equals("\\", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Input path format error.");
            }

            if (path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - 1);
            }
            int index = path.LastIndexOf('/');
            string parentPath = string.Empty;
            if (index > 0)
            {
                parentPath = path.Substring(0, index);
            }
            string lastDirName = path.Substring(index + 1);

            FtpWebRequest request = ftpClient.BuildFtpWebRequest(parentPath, WebRequestMethods.Ftp.ListDirectoryDetails);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            string dirDetailsStr;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                dirDetailsStr = sr.ReadToEnd();
            }

            Regex regexDirDetailsStr = new Regex(string.Format("([^<]+)<[^>]+>\\s*{0}\r\n", lastDirName));
            Match match = regexDirDetailsStr.Match(dirDetailsStr);
            if (!match.Success)
            {
                throw new Exception(string.Format("Not found '{0}' directory details.", path));
            }
            string dateTimeStr = Regex.Replace(match.Groups[1].Value.Trim(), "\\s+", " ");

            //string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt", 
            //       "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss", 
            //       "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt", 
            //       "M/d/yyyy h:mm", "M/d/yyyy h:mm", 
            //       "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm", "MM-dd-yy hh:mmtt"};

            string[] formats = {AveDateTimeUtility.GetDateTypeForConfiguration1(), AveDateTimeUtility.GetDateTypeForConfiguration2(),
                                AveDateTimeUtility.GetDateTypeForConfiguration3(),AveDateTimeUtility.GetDateTypeForConfiguration4(),
                                AveDateTimeUtility.GetDateTypeForConfiguration5(), AveDateTimeUtility.GetDateTypeForConfiguration6(),
                                AveDateTimeUtility.GetDateTypeForConfiguration7(), AveDateTimeUtility.GetDateTypeForConfiguration8(),
                                AveDateTimeUtility.GetDateTypeForConfiguration9(), AveDateTimeUtility.GetDateTypeForConfiguration10(),
                                AveDateTimeUtility.GetDateTypeForConfiguration11()};

            DateTime dateTime;
            if (!DateTime.TryParseExact(dateTimeStr, formats, AbstractXSystem.Culture, System.Globalization.DateTimeStyles.None, out dateTime))
            {
                throw new Exception(string.Format("Parse date time string '{0}' error.", dateTimeStr));
            }
            return dateTime;
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}
