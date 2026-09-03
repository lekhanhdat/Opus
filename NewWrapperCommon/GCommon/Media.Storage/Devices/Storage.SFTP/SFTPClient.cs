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

[module: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "AvePoint.Media.Storage.SFTP.SFTPClient")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPClient.#DoConnection()", MessageId = "sftp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPClient.#StoreFile(System.String,System.IO.Stream)", MessageId = "sftp")]
namespace AvePoint.Media.Storage.SFTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Util;
    using System.Net;
    using System.Reflection;
    using AvePoint.GCommon;
    using System.IO;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Diagnostics;
    using Renci.SshNet;
    using Renci.SshNet.Sftp;
    using System.Net.Sockets;
    using Renci.SshNet.Common;
    using System.Threading;
    #endregion

    #region CodeReview
    #endregion

    //TODO 重构
    class SFTPClient
    {
        int timeOut;
        int bufferSize = 1024 * 64;
        StorageLogger logger = new StorageLogger(typeof(SFTPClient));
        public delegate T RetryDelegate<T>();
        string rootFolder;
        SftpClient renciSFTPClientNOTUsedDirectly;
        private bool isOpenConnection = false;
        private object checkLocker = new object();
        ConnectionInfo connectionInfo = null;
        bool isReopening = false;
        private int sftpBufferSize = 16 * 1024;

        string pathSeperator = "/";

        public void Open(SFTPNodeInfo nodeInfo, string rootFolder)
        {
            this.rootFolder = rootFolder;
            this.timeOut = 120000;

            PrivateKeyFile keyFile = null;
            if (IsValiadParameter(nodeInfo.PrivateKey))
            {
                if (IsValiadParameter(nodeInfo.PrivateKeyPassword))
                {
                    keyFile = new PrivateKeyFile(nodeInfo.PrivateKey, nodeInfo.PrivateKeyPassword);
                }
                else
                {
                    keyFile = new PrivateKeyFile(nodeInfo.PrivateKey);
                }
            }
            
            if (!IsValiadParameter(nodeInfo.Password) && IsValiadParameter(nodeInfo.PrivateKey))
            {
                logger.Info("with password and privateKey");
                connectionInfo = new PrivateKeyConnectionInfo(nodeInfo.HostName, nodeInfo.Port, nodeInfo.UserName, keyFile);
            }
            else if (IsValiadParameter(nodeInfo.Password) && !IsValiadParameter(nodeInfo.PrivateKey))
            {
                logger.Info("with password");
                connectionInfo = new PasswordConnectionInfo(nodeInfo.HostName, nodeInfo.Port, nodeInfo.UserName, nodeInfo.Password);
            }
            else
            {
            }
            if (nodeInfo.BufferSize > 0)
            {
                sftpBufferSize = nodeInfo.BufferSize;
            }


            OpenConnection();
        }

        private SftpClient RenciSFTPClient
        {
            get
            {
                if (!isOpenConnection)
                {
                    OpenConnection();
                }
                return this.renciSFTPClientNOTUsedDirectly;
            }
        }

        private void OpenConnection()
        {
            lock (checkLocker)
            {
                if (!isOpenConnection)
                {
                    if (renciSFTPClientNOTUsedDirectly == null)
                        renciSFTPClientNOTUsedDirectly = new SftpClient(connectionInfo);
                    renciSFTPClientNOTUsedDirectly.KeepAliveInterval = new TimeSpan(0, 0, 30);
                    renciSFTPClientNOTUsedDirectly.OperationTimeout = new TimeSpan(1, 0, 0);
                    DoConnection();
                    isOpenConnection = true;
                }
            }
        }

        private void DoConnection()
        {
            int retryIndex = 0;
            Exception error = null;
            while (retryIndex < 3)
            {
                retryIndex++;
                if (retryIndex > 1)
                {
                    logger.Info("retry count " + retryIndex);
                }
                try
                {
                    renciSFTPClientNOTUsedDirectly.Connect();
                    break;
                }
                catch (SshAuthenticationException sshAuthenticationException)
                {
                    throw sshAuthenticationException;
                }
                catch (Exception e)
                {
                    error = e;
                    logger.Error(e.Message, e);
                    try
                    {
                        renciSFTPClientNOTUsedDirectly.Disconnect();
                    }
                    catch (Exception disException)
                    {
                        logger.Error(disException.Message, disException);
                    }
                }
                Thread.Sleep(10 * 1000);
            }
            if (retryIndex >= 3)
            {
                logger.Error("cannot connect to sftp server");
                throw error;
            }
        }

        public void ReopenConnection()
        {
            lock (checkLocker)
            {
                isReopening = true;
                isOpenConnection = false;
                Close();
                OpenConnection();
                isReopening = false;
            }
        }

        private Boolean IsValiadParameter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            else
                return true;
        }

        public void MakeRootFolder(string pathName)
        {
            pathName = FormatSFTPPath(pathName);
            string[] dirs = pathName.Split(pathSeperator[0]);
            StringBuilder dir = new StringBuilder();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (i != 0)
                {
                    dir.Append(pathSeperator + dirs[i]);
                }
                else
                {
                    dir.Append(dirs[i]);
                }
                if (!string.IsNullOrEmpty(dir.ToString().Trim()))
                {
                    SftpFile file = (SftpFile)RenciSFTPClient.Get(dir.ToString());
                    if (file == null || !file.IsDirectory)
                    {
                        RenciSFTPClient.CreateDirectory(dir.ToString());
                        logger.Debug(string.Format("make the directory {0} successful", dir.ToString()));
                    }
                    else
                    {
                        logger.Debug(string.Format("the directory {0} exist", dir.ToString()));
                    }

                }
            }
        }

        public void MakeDirectory(string pathName)
        {
            //pathName = buildRootFolder(pathName);
            pathName = FormatSFTPPath(pathName);
            string[] dirs = pathName.Split(pathSeperator[0]);
            StringBuilder dir = new StringBuilder();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (i != 0)
                {
                    dir.Append(pathSeperator + dirs[i]);
                }
                else
                {
                    dir.Append(dirs[i]);
                }
                if (!string.IsNullOrEmpty(dir.ToString().Trim()))
                {
                    if (!CheckDirectory(dir.ToString()))
                    {
                        if (CreateDirectory(dir.ToString()))
                        {
                            logger.Debug(string.Format("make the directory {0} successful", dir.ToString()));
                        }
                    }
                }
            }
        }

        public bool CreateDirectory(string pathName)
        {
            pathName = buildFormatPath(pathName);
            try
            {
                //RenciSFTPClient.CreateDirectory(pathname);
                //return true;
                return Retry<bool>(delegate ()
                {
                    RenciSFTPClient.CreateDirectory(pathName);
                    return true;
                });
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Create Directory Error : {0}", e.Message));
                return false;
            }
        }

        /// <summary>
        /// upload a test file;use for FtpSystem.Validate()
        /// </summary>
        /// <param name="fileName">test file</param>
        /// <param name="localStream">test file's MemoryStream</param>
        /// <returns></returns>
        public bool StoreFile(string fileName, Stream localStream)
        {
            var result = default(bool);
            fileName = buildFormatPath(fileName);
            try
            {
                using (Stream requestStream = RenciSFTPClient.OpenWrite(fileName))
                {
                    int readLength = 0;
                    byte[] buffer = new byte[bufferSize];
                    readLength = localStream.Read(buffer, 0, buffer.Length);
                    while (readLength != 0)
                    {
                        requestStream.Write(buffer, 0, readLength);
                        readLength = localStream.Read(buffer, 0, buffer.Length);
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error(string.Format("can not create file {0} on this sftp server, please check the user's authority {1}", fileName, e.Message), e);
                throw;
            }
            return result;
        }

        public bool DeleteFile(string pathName)
        {
            //RenciSFTPClient.DeleteFile(pathname);
            //return true;
            pathName = buildFormatPath(pathName);
            return Retry<bool>(delegate ()
            {
                RenciSFTPClient.DeleteFile(pathName);
                return true;
            });
        }

        public StorageDeleteResult DeleteDiectory(string pathName, StorageDeleteResult sr)
        {
            if (sr == null)
            {
                sr = new StorageDeleteResult();
            }
            pathName = FormatSFTPPath(pathName);
            if (CheckDirectory(pathName))
            {
                var files = this.ListDirectoryAndFiles(pathName);
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var tempFileName = file.Name;
                        if (!tempFileName.Contains(pathName))
                        {
                            tempFileName = PathUtil.CombinePath(pathName, file.Name);
                        }
                        if (file.IsDirectory)
                        {
                            DeleteDiectory(tempFileName, sr);
                        }
                        else
                        {
                            Int64 fileSize = GetFileSize(tempFileName);
                            this.DeleteFile(tempFileName);
                            sr.DeletedFileSize += fileSize;
                        }
                    }
                    if (RemoveDirectory(pathName))
                    {
                        sr.IsDeleted = true;
                    }
                }
                else
                {
                    this.logger.Info("The directory you want to delete is not exist.");
                    sr.IsDeleted = true;
                }
            }
            return sr;
        }

        public bool RemoveDirectory(string pathName)
        {
            //RenciSFTPClient.DeleteDirectory(pathname);
            //return true;
            pathName = buildFormatPath(pathName);
            return Retry<bool>(delegate ()
            {
                RenciSFTPClient.DeleteDirectory(pathName);
                return true;
            });
        }

        public Stream GetUploadStream(string pathName)
        {
            //SftpFileStream uploadStream = RenciSFTPClient.OpenWrite(pathname);
            //return uploadStream;
            pathName = buildFormatPath(pathName);
            return Retry<Stream>(delegate ()
            {
                SftpFileStream uploadStream = RenciSFTPClient.OpenWrite(pathName);
                return uploadStream;
            });
        }

        public Stream GetUploadStream(string pathName, FileMode mode)
        {
            //SftpFileStream uploadStream = RenciSFTPClient.OpenWrite(pathname, mode);
            //return uploadStream;
            pathName = buildFormatPath(pathName);
            return Retry<Stream>(delegate ()
            {
                SftpFileStream uploadStream = RenciSFTPClient.OpenWrite(pathName);
                return uploadStream;
            });
        }

        public Stream GetDownloadStream(string pathName)
        {
            //SftpFileStream downloadStream = RenciSFTPClient.OpenRead(pathname, offset);
            //return downloadStream;
            pathName = buildFormatPath(pathName);
            return Retry<Stream>(delegate ()
            {
                SftpFileStream downloadStream = RenciSFTPClient.OpenRead(pathName);
                return downloadStream;
            });
        }

        public long GetFileSize(string pathName)
        {
            //return RenciSFTPClient.GetAttributes(pathname).Size;
            pathName = buildFormatPath(pathName);
            return RenciSFTPClient.GetAttributes(pathName).Size;

        }

        public DateTime GetLastModifiedTime(string pathName)
        {
            pathName = buildFormatPath(pathName);
            return Retry<DateTime>(delegate ()
            {
                return RenciSFTPClient.GetAttributes(pathName).LastWriteTime;
            });
        }

        public FileStruct[] ListDirectoryAndFiles(string pathName)
        {
            pathName = buildFormatPath(pathName);
            //IEnumerable<SftpFile> sftpFiles = RenciSFTPClient.ListDirectory(pathName);

            IEnumerable<ISftpFile> sftpFiles = RenciSFTPClient.ListDirectory(pathName);

            List<FileStruct> fileStructs = new List<FileStruct>();
            foreach (SftpFile file in sftpFiles)
            {
                FileStruct fileStruct = new FileStruct();
                fileStruct.Name = file.Name;
                fileStruct.IsDirectory = file.IsDirectory;
                if (!(fileStruct.Name == "." || fileStruct.Name == ".."))
                {
                    fileStructs.Add(fileStruct);
                }
            }
            return fileStructs.ToArray();
        }
        
        public ulong GetFreeSpace()
        {
            //renciSFTPClient.
            return long.MaxValue;
        }

        /// <summary>
        /// because of the special of FileZilla; so don't use .net api
        /// use socket connect to the ftp server and send rnfr
        /// </summary>
        public bool CheckFile(string pathName)
        {
            //return renciSFTPClient.Exists(pathname);
            //是否需要根据sftp区分？
            pathName = buildFormatPath(pathName);
            SftpFile file = Retry<SftpFile>(delegate ()
            {
                return (SftpFile)RenciSFTPClient.Get(pathName);
            });
            if (file != null && !file.IsDirectory)
                return true;
            else
                return false;
        }
        public bool CheckFileExist(string pathName)
        {
            //return renciSFTPClient.Exists(pathname);
            //是否需要根据sftp区分？
            pathName = buildFormatPath(pathName);

            return RenciSFTPClient.Exists(pathName);

        }
        private bool InternalCheckFileExist(string pathName)
        {
            bool exist = RenciSFTPClient.Exists(pathName);
            return exist;
        }
        public bool CheckDirectory(string pathName)
        {
            //return renciSFTPClient.Exists(pathname);
            pathName = buildFormatPath(pathName);
            if (InternalCheckFileExist(pathName))
            {
                SftpFile file = Retry<SftpFile>(delegate ()
                {
                    return (SftpFile)RenciSFTPClient.Get(pathName);
                });
                if (file != null && file.IsDirectory)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }

        }
        private string buildFormatPath(string path)
        {
            string tempPath = path;
            if (rootFolder != null && rootFolder.Length > 0)
            {
                tempPath = PathUtil.CombinePath(rootFolder, path);
            }
            tempPath = FormatSFTPPath(tempPath);
            return tempPath;
        }

        public void Close()
        {
            try
            {
                if (renciSFTPClientNOTUsedDirectly != null)
                {
                    renciSFTPClientNOTUsedDirectly.Disconnect();
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            try
            {
                if (renciSFTPClientNOTUsedDirectly != null)
                {
                    renciSFTPClientNOTUsedDirectly.Dispose();
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            renciSFTPClientNOTUsedDirectly = null;
        }

        public void MakeSFtpRootFolder()
        {
            string rootFolderPath = FormatSFTPPath(rootFolder);
            string[] dirs = rootFolderPath.Split('\\');
            StringBuilder dir = new StringBuilder();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (i != 0)
                {
                    dir.Append(pathSeperator + dirs[i]);
                }
                else
                {
                    dir.Append(dirs[i]);
                }
                String folder = dir.ToString().Trim();
                if (!string.IsNullOrEmpty(folder))
                {
                    if (!CheckRootFolder(folder))
                    {
                        if (CreateRootFolder(folder))
                        {
                            logger.Debug(string.Format("make the directory {0} successful", dir.ToString()));
                        }
                    }
                }
            }
        }

        public bool CheckRootFolder(string pathName)
        {
            SftpFile file = Retry<SftpFile>(delegate ()
            {
                return (SftpFile)RenciSFTPClient.Get(pathName);
            });
            if (file != null && file.IsDirectory)
                return true;
            else
                return false;

        }

        public bool CreateRootFolder(string pathName)
        {
            try
            {
                //RenciSFTPClient.CreateDirectory(pathname);
                //return true;
                pathName = FormatSFTPPath(pathName);
                return Retry<bool>(delegate ()
                {
                    RenciSFTPClient.CreateDirectory(pathName);
                    return true;
                });
            }
            catch (Exception e)
            {
                logger.Error("Create Root Folder Error : {0}", e.Message);
                return false;
            }
        }

        public Boolean RenameFileName(StorageInfo oldInfo, StorageInfo newInfo, Boolean isOVerride = default(Boolean))
        {
            this.logger.Info(String.Format("Rename file name begin"));
            var oldPath = buildFormatPath(PathUtil.CombinePath(oldInfo.HighName, oldInfo.LowName));
            var newPath = buildFormatPath(PathUtil.CombinePath(newInfo.HighName, newInfo.LowName));
            //this.logger.Info(String.Format("Rename file name begin,old path:{0},new path:{1},need override{2}."), oldPath, newPath, isOVerride.ToString());
            try
            {
                if (isOVerride && CheckFileExist(PathUtil.CombinePath(oldInfo.HighName, oldInfo.LowName)))
                {
                    DeleteFile(PathUtil.CombinePath(oldInfo.HighName, oldInfo.LowName));
                }
                RenciSFTPClient.RenameFile(newPath, oldPath, false);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while rename file name,details{0}", e.ToString());
                throw;
            }
            return true;
        }

        private string FormatSFTPPath(string path)
        {
            if (this.pathSeperator.Equals("/"))
            {
                return path.Replace("\\", "/");
            }
            else
            {
                return path.Replace("/", "\\");
            }
        }

        public T Retry<T>(RetryDelegate<T> del)
        {
            int counter = 0;
            SftpClient tempClient = renciSFTPClientNOTUsedDirectly;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (SocketException socketException)
                {
                    if (counter > 10)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, socketException.Message, socketException);
                        throw;
                    }
                    if (counter > 3)
                    {
                        Thread.Sleep(30 * 1000);
                    }
                    logger.Warn("this exception is a connection fail exception : " + socketException.Message, socketException);
                    lock (checkLocker)
                    {
                        if (tempClient == renciSFTPClientNOTUsedDirectly)
                        {
                            ReopenConnection();
                        }
                    }
                }
                catch (SshException sshConnectionException)
                {
                    if (counter > 10)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, sshConnectionException.Message, sshConnectionException);
                        throw;
                    }
                    if (counter > 3)
                    {
                        Thread.Sleep(30 * 1000);
                    }
                    logger.Warn("this exception is a connection fail exception : " + sshConnectionException.Message, sshConnectionException);
                    lock (checkLocker)
                    {
                        if (tempClient == renciSFTPClientNOTUsedDirectly)
                        {
                            ReopenConnection();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    bool needRetry = false;
                    while (isReopening)
                    {
                        needRetry = true;
                        Thread.Sleep(1000);
                    }
                    if (tempClient != renciSFTPClientNOTUsedDirectly)
                    {
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }

}
