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


using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AvePoint.Media.Storage.FS
{
    class AlphaFSClient : AbstractFSClient
    {
        StorageLogger logger = new StorageLogger(typeof(AlphaFSClient));
        FSClientOpenParam openParam;

        public AlphaFSClient(FSClientOpenParam param)
        {
            this.openParam = param;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            XStream stream = null;
            IIdentity tmpIdentity = null;
            //using (this.openParam.StorageIdentity.Impersonate())
            //{
            try
            {
                tmpIdentity = this.openParam.StorageIdentity.Impersonate();
                try
                {
                    stream = new AlphaFSStream(info, this.openParam.StorageSystem, openParam, fileMode);
                }
                catch (CatchedToDoMoreExcetion ex)
                {
                    Trace.TraceWarning(ex.Message);
                    string directoryPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName).TrimEnd('\\').TrimEnd('/') + "\\";
                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPath));
                    stream = new AlphaFSStream(info, this.openParam.StorageSystem, openParam, fileMode);
                }
                if (stream == null)
                {
                    throw new XIOException("OpenStream failed from " + PathUtil.CombinePath(PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName), info.LowName));
                }
                if (info.Offset > 0)
                {
                    stream.Seek(info.Offset, SeekOrigin.Begin);
                }
                return stream;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error("open stream failed for file: {0}, message: {1}", PathUtil.CombinePath(this.openParam.SystemLocation, info.HighPlusLowName), ex);
                throw new AuthenticationFailedException(ex.Message, ex);
            }
            catch (Exception e)
            {
                logger.Error("open stream failed for file: {0}, message: {1}", PathUtil.CombinePath(this.openParam.SystemLocation, info.HighPlusLowName), e);
                throw;
            }
            finally
            {
                if (tmpIdentity != null)
                {
                    tmpIdentity.Dispose();
                }
            }
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            try
            {
                bool isExist = false;
                string directoryPath = info.HighName;
                if (!string.IsNullOrEmpty(info.LowName))
                {
                    directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
                }
                string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    isExist = Alphaleonis.Win32.Filesystem.Directory.Exists(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                }
                return isExist;
            }
            catch (Exception e)
            {
                logger.Error("check directory [{0}] failed: {1}", info.HighPlusLowName, e);
                throw;
            }
        }

        public override bool FileExists(StorageInfo info)
        {
            bool rs = false;
            string fullPath = string.Empty;
            try
            {
                fullPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName);
                fullPath = PathUtil.CombinePath(fullPath, info.LowName);

                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (Alphaleonis.Win32.Filesystem.Directory.Exists(this.openParam.SystemLocation))
                    {
                        rs = Alphaleonis.Win32.Filesystem.File.Exists(fullPath);
                    }
                    else
                    {
                        logger.Error("The long path {0} is not available", this.openParam.SystemLocation);
                        throw new DeviceNotAvailableException(string.Format("The location [{0}] is not available.", this.openParam.OriginalSystemLocation));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("check file [{0}] failed: {1}:", fullPath, ex);
                throw;
            }
            return rs;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            string directoryPath = info.HighName;
            if (!string.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            directoryPath = directoryPath.TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (Alphaleonis.Win32.Filesystem.Directory.Exists(PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath)))
                {
                    rs.DeletedFileSize = this.DeleteDirectorFileAndGetLength(this.openParam.SystemLocation, directoryPath);
                    DeleteDirectory(directoryPath, this.openParam.SystemLocation, info.IsDeleteParentFolder);
                    rs.IsDeleted = true;
                }
                else
                {
                    if (Alphaleonis.Win32.Filesystem.Directory.Exists(this.openParam.SystemLocation))
                    {
                        logger.Debug("The folder [{0}] you want to delete is no longer exist.", directoryPath);
                        rs.IsDeleted = true;
                    }
                    else
                    {
                        logger.Warn("Can't access to the parent folder for folder [{0}], delete failed.", directoryPath);
                        rs.IsDeleted = false;
                    }
                }
            }
            return rs;
        }

        private long GetDirectoryLength(string dirPath)
        {
            long len = 0;
            Alphaleonis.Win32.Filesystem.DirectoryInfo dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(dirPath));
            if (ShouldSkipDirectory(dir))
            {
                logger.Warn("Skip directory length calculation for special directory: {0}", dirPath);
                return 0;
            }
            foreach (Alphaleonis.Win32.Filesystem.FileInfo file in dir.GetFiles())
            {
                try
                {
                    len += file.Length;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
            Alphaleonis.Win32.Filesystem.DirectoryInfo[] dis = dir.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName + "\\");
                }
            }
            return len;
        }
        private bool ShouldSkipDirectory(Alphaleonis.Win32.Filesystem.DirectoryInfo dir)
        {
            if (dir == null)
            {
                return true;
            }
            //Skip specific folder in DFS
            var name = dir.Name;
            if (name.Equals(".DFSFolderLink", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("DfsrPrivate", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //Backup logic：Skip specific directory as Hidden/System/ReparsePoint （去除特殊目录：隐藏/系统级/软链接）
            //var attrs = dir.Attributes;
            //if ((attrs & FileAttributes.Hidden) == FileAttributes.Hidden ||
            //    (attrs & FileAttributes.System) == FileAttributes.System ||
            //    (attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            //{
            //    return true;
            //}

            return false;
        }
        private long DeleteDirectorFileAndGetLength(string systemLocation, string directoryPath)
        {
            Int64 len = 0;
            using (this.openParam.StorageIdentity.Impersonate())
            {
                var dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(PathUtil.CombinePath(systemLocation, directoryPath));
                if (dir.Attributes < FileAttributes.ReparsePoint)
                {
                    var dirsList = dir.GetDirectories();
                    var filesList = dir.GetFiles();
                    foreach (var file in filesList)
                    {
                        try
                        {
                            len += DeleteFile(new StorageInfo() { HighName = directoryPath, LowName = file.Name }).DeletedFileSize;
                        }
                        catch (FileNotFoundException e)
                        {
                            Trace.TraceWarning(e.ToString());
                        }
                    }
                    Alphaleonis.Win32.Filesystem.DirectoryInfo[] dis = dirsList;
                    if (dis.Length > 0)
                    {
                        for (int i = 0; i < dis.Length; i++)
                        {
                            len += DeleteDirectorFileAndGetLength(systemLocation, PathUtil.CombinePath(directoryPath, dis[i].Name));
                        }
                    }
                }
            }
            return len;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName);
            string fileFullName = PathUtil.CombinePath(fileFullPath, info.LowName);
            if (openParam.securelyDelete)
            {
                WipeFile(info);
            }
            using (this.openParam.StorageIdentity.Impersonate())
            {
                Alphaleonis.Win32.Filesystem.FileInfo fileInfo = new Alphaleonis.Win32.Filesystem.FileInfo(fileFullName);
                if (fileInfo.Exists)
                {
                    rs.DeletedFileSize = fileInfo.Length;
                    Alphaleonis.Win32.Filesystem.File.Delete(fileFullName);
                    rs.IsDeleted = true;
                }
                else
                {
                    if (Alphaleonis.Win32.Filesystem.Directory.Exists(this.openParam.SystemLocation))
                    {
                        logger.Debug("The file [{0}] you want to delete is no longer exist", fileFullName);
                        rs.IsDeleted = true;
                    }
                    else
                    {
                        logger.Warn("Can't access to parent folder for file [{0}], delete failed", fileFullName);
                        rs.IsDeleted = false;
                    }
                }
            }
            return rs;
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
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            AlphaFSDirectoryInfo irectoryInfo = null;
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!Alphaleonis.Win32.Filesystem.Directory.Exists(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath)))
                {
                    if (mode != FileMode.Open)
                    {
                        if (!openParam.IsReadonly)
                        {
                            Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                            irectoryInfo = new AlphaFSDirectoryInfo(new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath)), directoryPath);
                            irectoryInfo.System = this.openParam.StorageSystem;
                        }
                        else
                        {
                            throw new MethodNotSupportForReadOnlyDeviceException("The device is set to read-only");
                        }
                    }
                }
                else
                {
                    irectoryInfo = new AlphaFSDirectoryInfo(new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath)), directoryPath);
                    irectoryInfo.System = this.openParam.StorageSystem;
                }
                if (irectoryInfo != null)
                {
                    Alphaleonis.Win32.Filesystem.DirectoryInfo dInfo = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));

                    AssembleDirAttribute(irectoryInfo, dInfo, dirInfo);
                }
            }
            return irectoryInfo;
        }

        private void AssembleDirAttribute(XDirectoryInfo irectoryInfo, Alphaleonis.Win32.Filesystem.DirectoryInfo dirInfo, StorageInfo info)
        {
            try
            {
                irectoryInfo.UserName = this.openParam.SystemUserName;
                irectoryInfo.Password = this.openParam.SystemPassword;
                irectoryInfo.Domain = this.openParam.SystemDomain;
                irectoryInfo.OriginalDirFullPath = PathUtil.CombinePath(this.openParam.OriginalSystemLocation, info.HighPlusLowName);
                //irectoryInfo.LastAccessTime = dirInfo.LastAccessTime;
                //irectoryInfo.LastAccessTimeUtc = dirInfo.LastAccessTimeUtc;
                //irectoryInfo.LastWriteTime = dirInfo.LastWriteTime;
                //irectoryInfo.LastWriteTimeUtc = dirInfo.LastWriteTimeUtc;
                //irectoryInfo.CreationTime = dirInfo.CreationTime;
                //irectoryInfo.CreationTimeUtc = dirInfo.CreationTimeUtc;
                //irectoryInfo.ParentFullName = (dirInfo.Parent == null) ? "" : AlphaFSUtil.ConvertPathToCommonUNC(dirInfo.Parent.FullName);
                //irectoryInfo.AccessControl = dirInfo.GetAccessControl();
                //irectoryInfo.DirFullPath = AlphaFSUtil.ConvertPathToCommonUNC(dirInfo.FullName);
                //irectoryInfo.UNCFullPath = AlphaFSUtil.ConvertPathToCommonUNC(dirInfo.FullName);
            }
            catch (Exception e)
            {
                this.logger.Error("AssembleDirAttribute : {0}", e);
            }
        }

        private void AssembleFileAttribute(XFileInfo xfileInfo, Alphaleonis.Win32.Filesystem.FileInfo info)
        {
            try
            {
                xfileInfo.UserName = this.openParam.SystemUserName;
                xfileInfo.Password = this.openParam.SystemPassword;
                xfileInfo.Domain = this.openParam.SystemDomain;
                xfileInfo.OriginalFileFullPath = PathUtil.CombinePath(this.openParam.OriginalSystemLocation, xfileInfo.HighPlusLowName);
                //xfileInfo.LastAccessTime = info.LastAccessTime;
                //xfileInfo.LastAccessTimeUtc = info.LastAccessTimeUtc;
                //xfileInfo.LastWriteTime = info.LastWriteTime;
                //xfileInfo.LastWriteTimeUtc = info.LastWriteTimeUtc;
                //xfileInfo.CreationTime = info.CreationTime;
                //xfileInfo.CreationTimeUtc = info.CreationTimeUtc;
                //xfileInfo.ParentFullName = AlphaFSUtil.ConvertPathToCommonUNC(info.Directory.FullName);
                //xfileInfo.AccessControl = info.GetAccessControl();
                //xfileInfo.AccessControl.GetOwner(
                //xfileInfo.FileFullPath = AlphaFSUtil.ConvertPathToCommonUNC(info.FullName);
            }
            catch (Exception e)
            {
                this.logger.Error("AssembleFileAttribute :{0} ", e);
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            string filePath = PathUtil.CombinePath(this.openParam.SystemLocation, fileInfo.HighName);
            filePath = PathUtil.CombinePath(filePath, fileInfo.LowName);
            // XFileInfo xfileInfo = null;
            AlphaFSFileInfo fsfileInfo = null;
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (Alphaleonis.Win32.Filesystem.File.Exists(filePath))
                {
                    Alphaleonis.Win32.Filesystem.FileInfo info = new Alphaleonis.Win32.Filesystem.FileInfo(filePath);
                    fsfileInfo = new AlphaFSFileInfo(info, fileInfo.HighName, fileInfo.LowName);
                    AssembleFileAttribute(fsfileInfo, info);
                    fsfileInfo.System = this.openParam.StorageSystem;
                }
            }

            return fsfileInfo;
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            List<XDirectoryInfo> xfs = new List<XDirectoryInfo>();
            if (dirInfo.IsLoadFirstLevel)
            {
                dirInfo.HighName = string.Empty;
                dirInfo.LowName = string.Empty;
            }
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo d = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                Alphaleonis.Win32.Filesystem.DirectoryInfo[] ds = d.GetDirectories();

                AlphaFSDirectoryInfo fsf;
                foreach (Alphaleonis.Win32.Filesystem.DirectoryInfo dd in ds)
                {
                    try
                    {
                        fsf = new AlphaFSDirectoryInfo(dd, PathUtil.CombinePath(directoryPath, dd.Name));
                        AssembleDirAttribute(fsf, dd, dirInfo);
                        fsf.System = this.openParam.StorageSystem;
                        xfs.Add(fsf);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("ListDirectories : {0}", e);
                    }

                }
            }
            return xfs;
        }

        public override IEnumerable<List<XFileInfo>> ListFilesInBatches(StorageInfo dirInfo, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }

            string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                var dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                var batch = new List<XFileInfo>(batchSize);

                foreach (var file in dir.EnumerateFiles())
                {
                    var fsf = new AlphaFSFileInfo(file, directoryPath, file.Name);
                    AssembleFileAttribute(fsf, file);
                    fsf.System = this.openParam.StorageSystem;
                    batch.Add(fsf);

                    if (batch.Count >= batchSize)
                    {
                        yield return batch;
                        batch = new List<XFileInfo>(batchSize);
                    }
                }

                if (batch.Count > 0)
                {
                    yield return batch;
                }
            }
        }

        public override IEnumerable<List<XDirectoryInfo>> ListDirectoriesInBatches(StorageInfo dirInfo, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            if (dirInfo.IsLoadFirstLevel)
            {
                dirInfo.HighName = string.Empty;
                dirInfo.LowName = string.Empty;
            }

            var directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }

            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";

            using (this.openParam.StorageIdentity.Impersonate())
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                var batch = new List<XDirectoryInfo>(batchSize);

                foreach (Alphaleonis.Win32.Filesystem.DirectoryInfo subDir in dir.EnumerateDirectories())
                {
                    if (subDir.Name.Equals(".DFSFolderLink", StringComparison.OrdinalIgnoreCase)
                        || subDir.Name.Equals("DfsrPrivate", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (PathUtil.CombinePath(fileFullPath, subDir.Name).Length >= 248)
                    {
                        throw new PathTooLongException("Directory path length is too long under the folder.");
                    }

                    var fsf = new AlphaFSDirectoryInfo(subDir, PathUtil.CombinePath(directoryPath, subDir.Name));
                    AssembleDirAttribute(fsf, subDir, dirInfo);
                    fsf.System = this.openParam.StorageSystem;
                    batch.Add(fsf);

                    if (batch.Count >= batchSize)
                    {
                        yield return batch;
                        batch = new List<XDirectoryInfo>(batchSize);
                    }
                }

                if (batch.Count > 0)
                {
                    yield return batch;
                }
            }
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            List<XFileInfo> xfs = new List<XFileInfo>();
            string directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            string fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileFullPath));
                Alphaleonis.Win32.Filesystem.FileInfo[] fs = dir.GetFiles();
                AlphaFSFileInfo fsf = new AlphaFSFileInfo();

                foreach (Alphaleonis.Win32.Filesystem.FileInfo file in fs)
                {
                    try
                    {
                        fsf = new AlphaFSFileInfo(file, directoryPath, file.Name);

                        AssembleFileAttribute(fsf, file);
                        fsf.System = this.openParam.StorageSystem;

                        xfs.Add(fsf);
                    }

                    catch (Exception e)
                    {
                        this.logger.Error("ListFiles : {0}", e);
                    }
                }
            }
            return xfs;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            StorageListResult result = new StorageListResult();
            result.Files = ListFiles(dirInfo);
            result.SubDirs = ListDirectories(dirInfo);
            return result;
        }

        //public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        //{
        //    if (this.openParam.ModuleType == ModuleType.Connector)
        //    {
        //        return MoveFileFastly(sourceFileInfo, targetFileInfo, isOverWrite);
        //    }
        //    else
        //    {
        //        return MoveFileSafely(sourceFileInfo, targetFileInfo, isOverWrite);
        //    }
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "fsdl")]
        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            StorageMoveResult rs = new StorageMoveResult();
            string sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName)).TrimEnd('\\').TrimEnd('/');
            string targetFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName)).TrimEnd('\\').TrimEnd('/');
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!Alphaleonis.Win32.Filesystem.File.Exists(sourceFilePath))
                {
                    rs.IsMoved = false;
                    rs.Message = "File can not be found. Path: " + sourceFilePath;
                }
                else if (targetFilePath.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    rs.IsMoved = false;
                    rs.Message = "The source file has the same path with the target file.";
                }
                else if (!Alphaleonis.Win32.Filesystem.File.Exists(targetFilePath))
                {
                    try
                    {
                        string parentFolerPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetFilePath);
                        if (!Alphaleonis.Win32.Filesystem.Directory.Exists(parentFolerPath))
                        {
                            Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(parentFolerPath);
                        }
                        Alphaleonis.Win32.Filesystem.File.Move(sourceFilePath, targetFilePath);
                    }
                    catch (System.Exception ex)
                    {
                        rs.IsMoved = false;
                        rs.Message = "The file can't be moved.Error: " + ex.Message;
                    }
                }
                else if (isOverWrite)
                {
                    string tempFilePath = sourceFilePath + "_fsdl_temp";
                    try
                    {
                        if (Alphaleonis.Win32.Filesystem.File.Exists(tempFilePath))
                        {
                            Alphaleonis.Win32.Filesystem.File.Delete(tempFilePath);
                        }
                        Alphaleonis.Win32.Filesystem.File.Move(targetFilePath, tempFilePath);
                        try
                        {
                            Alphaleonis.Win32.Filesystem.File.Move(sourceFilePath, targetFilePath);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            Alphaleonis.Win32.Filesystem.File.Move(tempFilePath, targetFilePath);
                            throw;
                        }
                        try
                        {
                            Alphaleonis.Win32.Filesystem.FileInfo tmpFile = new Alphaleonis.Win32.Filesystem.FileInfo(tempFilePath);
                            if (tmpFile.IsReadOnly == true)
                            {
                                tmpFile.IsReadOnly = false;
                            }
                            Alphaleonis.Win32.Filesystem.File.Delete(tempFilePath);
                            Alphaleonis.Win32.Filesystem.FileInfo fileinforead = new Alphaleonis.Win32.Filesystem.FileInfo(targetFilePath);
                            if (fileinforead.IsReadOnly == true)
                            {
                                fileinforead.IsReadOnly = false;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.Warn("Change read-only to false has been failed, File:{0}, Error:{1} ", targetFilePath, ex);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        rs.IsMoved = false;
                        rs.Message = "The file can't be moved. " + ex.Message;
                    }
                }
            }
            return rs;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "fsdl")]
        public override StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite = true)
        {
            StorageMoveResult rs = new StorageMoveResult();
            string sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName)).TrimEnd('\\').TrimEnd('/');
            string targetFilePath = PathUtil.CombinePath(destSystem.SystemLocation, PathUtil.CombinePath(destFile.HighName, destFile.LowName)).TrimEnd('\\').TrimEnd('/');
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!Alphaleonis.Win32.Filesystem.File.Exists(sourceFilePath))
                {
                    rs.IsMoved = false;
                    rs.Message = "File can not be found. Path: " + sourceFilePath;
                }
                else if (targetFilePath.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    rs.IsMoved = false;
                    rs.Message = "The source file has the same path with the target file.";
                }
                else if (!Alphaleonis.Win32.Filesystem.File.Exists(targetFilePath))
                {
                    try
                    {
                        string parentFolerPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetFilePath);
                        if (!Alphaleonis.Win32.Filesystem.Directory.Exists(parentFolerPath))
                        {
                            Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(parentFolerPath);
                        }
                        Alphaleonis.Win32.Filesystem.File.Move(sourceFilePath, targetFilePath);
                    }
                    catch (System.Exception ex)
                    {
                        rs.IsMoved = false;
                        rs.Message = "The file can't be moved. " + ex.Message;
                    }
                }
                else if (isOverWrite)
                {
                    string tempFilePath = sourceFilePath + "_fsdl_temp";
                    try
                    {
                        if (Alphaleonis.Win32.Filesystem.File.Exists(tempFilePath))
                        {
                            Alphaleonis.Win32.Filesystem.File.Delete(tempFilePath);
                        }
                        Alphaleonis.Win32.Filesystem.File.Move(targetFilePath, tempFilePath);
                        try
                        {
                            Alphaleonis.Win32.Filesystem.File.Move(sourceFilePath, targetFilePath);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            Alphaleonis.Win32.Filesystem.File.Move(tempFilePath, targetFilePath);
                            throw;
                        }
                        try
                        {
                            Alphaleonis.Win32.Filesystem.FileInfo tmpFile = new Alphaleonis.Win32.Filesystem.FileInfo(tempFilePath);
                            if (tmpFile.IsReadOnly == true)
                            {
                                tmpFile.IsReadOnly = false;
                            }
                            Alphaleonis.Win32.Filesystem.File.Delete(tempFilePath);
                            Alphaleonis.Win32.Filesystem.FileInfo fileinforead = new Alphaleonis.Win32.Filesystem.FileInfo(targetFilePath);
                            if (fileinforead.IsReadOnly == true)
                            {
                                fileinforead.IsReadOnly = false;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.Warn("Change read-only to false has been failed, File:{0}, Error:{1} ", targetFilePath, ex);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        rs.IsMoved = false;
                        rs.Message = "The file can't be moved. " + ex.Message;
                    }
                }
            }
            rs.URI = new XURIResult()
            {
                SInfo = destFile.Clone(),
                SysId = destSystem.SystemID,
                SdType = 0
            };
            return rs;
        }

        //private StorageMoveResult MoveFileSafely(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        //{
        //    StorageMoveResult moveRS = new StorageMoveResult();
        //    try
        //    {
        //        StorageCopyResult copyRS = CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
        //        if (copyRS.IsCopyed)
        //        {
        //            using (this.openParam.StorageIdentity.Impersonate())
        //            {
        //                string sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName));
        //                Alphaleonis.Win32.Filesystem.FileInfo sourceFile = new Alphaleonis.Win32.Filesystem.FileInfo(sourceFilePath);
        //                sourceFile.Delete();
        //            }
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

        //public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        //{
        //    if (this.openParam.ModuleType == ModuleType.Connector)
        //    {
        //        return MoveDirectoryFastly(sourceDirInfo, targetDirInfo, isOverWrite);
        //    }
        //    else
        //    {
        //        return MoveDirectorySafely(sourceDirInfo, targetDirInfo, isOverWrite);
        //    }
        //}

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            StorageMoveResult rs = new StorageMoveResult();
            string sourceDirPath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceDirInfo.HighName, sourceDirInfo.LowName)).TrimEnd('\\').TrimEnd('/') + "\\";
            string targetDirPath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetDirInfo.HighName, targetDirInfo.LowName)).TrimEnd('\\').TrimEnd('/') + "\\";
            Alphaleonis.Win32.Filesystem.DirectoryInfo sourceDir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(sourceDirPath));
            Alphaleonis.Win32.Filesystem.DirectoryInfo targetDir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetDirPath));
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!sourceDir.Exists)
                {
                    rs.IsMoved = false;
                    rs.Message = "Folder can not be found. Path: " + sourceDirPath;
                }
                if (sourceDirPath.Equals(targetDirPath, StringComparison.OrdinalIgnoreCase))
                {
                    rs.IsMoved = false;
                    rs.Message = "The source folder has the same path with the target folder.";
                    return rs;
                }
                if (!targetDir.Exists)
                {
                    try
                    {
                        if (targetDir.Parent != null && !targetDir.Parent.Exists)
                        {
                            Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetDir.Parent.FullName.TrimEnd('\\').TrimEnd('/') + "\\"));
                        }
                        Alphaleonis.Win32.Filesystem.Directory.Move(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(sourceDirPath), Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetDirPath));
                        return rs;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetDir.FullName.TrimEnd('\\').TrimEnd('/') + "\\"));
                    }
                }
                //目的端存在或者原来方法有异常，则使用递归的方法
                targetDir.Attributes = sourceDir.Attributes;
                //遍历当前文件夹的文件，并写入目的文件夹中
                foreach (XFileInfo fileInfo in ListFiles(sourceDirInfo))
                {
                    try
                    {
                        StorageInfo sourceFile = XConvert.FromNames(fileInfo.HighName, fileInfo.LowName);
                        StorageInfo targetFile = XConvert.FromNames(PathUtil.CombinePath(targetDirInfo.HighName, targetDirInfo.LowName), fileInfo.LowName);
                        rs = MoveFile(sourceFile, targetFile, isOverWrite);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        rs.IsMoved = false;
                    }
                }
                //遍历文件夹的子文件夹，进行递归处理
                foreach (XDirectoryInfo dirInfo in ListDirectories(sourceDirInfo))
                {
                    try
                    {
                        StorageInfo sourceFolder = XConvert.FromNames(dirInfo.HighName, dirInfo.LowName);
                        StorageInfo targetFolder = XConvert.FromNames(PathUtil.CombinePath(targetDirInfo.HighName, targetDirInfo.LowName), dirInfo.Name);
                        rs = MoveDirectory(sourceFolder, targetFolder, isOverWrite);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        rs.IsMoved = false;
                    }
                }
                if (rs.IsMoved == true && ListFiles(sourceDirInfo).Count == 0 && ListDirectories(sourceDirInfo).Count == 0)
                {
                    DeleteDirectory(sourceDirInfo);
                }
            }
            return rs;
        }

        //private StorageMoveResult MoveDirectorySafely(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        //{
        //    StorageMoveResult moveRS = new StorageMoveResult();
        //    try
        //    {
        //        StorageCopyResult copyRS = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
        //        if (copyRS.IsCopyed)
        //        {
        //            using (this.openParam.StorageIdentity.Impersonate())
        //            {
        //                string sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceDirInfo.HighName, sourceDirInfo.LowName));
        //                Alphaleonis.Win32.Filesystem.DirectoryInfo sourceFile = new Alphaleonis.Win32.Filesystem.DirectoryInfo(sourceFilePath);
        //                sourceFile.Delete(true);
        //            }
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

        private StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, bool isOverWrite)
        {
            StorageCopyResult rs = new StorageCopyResult();
            string folderPath = string.IsNullOrEmpty(sourceFolderInfo.LowName) ? sourceFolderInfo.HighName : PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName);
            string destPath = string.IsNullOrEmpty(targetFolderInfo.LowName) ? targetFolderInfo.HighName : PathUtil.CombinePath(targetFolderInfo.HighName, sourceFolderInfo.LowName);
            string sourceFolderFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, folderPath).TrimEnd('\\').TrimEnd('/') + "\\";
            string targetFolderFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, destPath).TrimEnd('\\').TrimEnd('/') + "\\";
            Alphaleonis.Win32.Filesystem.DirectoryInfo sourceFolder = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(sourceFolderFullPath));
            Alphaleonis.Win32.Filesystem.DirectoryInfo targetFolder = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(targetFolderFullPath));
            try
            {
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (sourceFolder.Exists)
                    {
                        if ((sourceFolder.GetFiles() != null && sourceFolder.GetFiles().Length > 0) || (sourceFolder.GetDirectories() != null && sourceFolder.GetDirectories().Length > 0))
                        {
                            //先copy子文件
                            foreach (Alphaleonis.Win32.Filesystem.FileInfo file in sourceFolder.GetFiles())
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
                            using (this.openParam.StorageIdentity.Impersonate())
                            {
                                foreach (Alphaleonis.Win32.Filesystem.DirectoryInfo directory in sourceFolder.GetDirectories())
                                {
                                    StorageInfo sourceSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name);
                                    StorageInfo targetSubFolderInfo = XConvert.FromNames(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name);
                                    if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                                    {
                                        rs.IsCopyed = false;
                                        return rs;
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new FileNotFoundException("The source directory do not exist,path:" + sourceFolderFullPath);
                            //using (identity.Impersonate())
                            //{

                            //    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(sourceFolder.Name);
                            //}
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                rs.IsCopyed = false;
                rs.Message = ex.Message;
            }
            return rs;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            StorageCopyResult rs = new StorageCopyResult();

            string sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName));
            string targetFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName));

            //同名则返回
            try
            {
                Alphaleonis.Win32.Filesystem.FileInfo sourceFile = new Alphaleonis.Win32.Filesystem.FileInfo(sourceFilePath);
                Alphaleonis.Win32.Filesystem.FileInfo targetFile = new Alphaleonis.Win32.Filesystem.FileInfo(targetFilePath);

                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (Alphaleonis.Win32.Filesystem.File.Exists(sourceFilePath))
                    {
                        if (!targetFile.Directory.Exists)
                        {
                            Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(targetFile.Directory.FullName);
                        }
                        if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (targetFile.Exists && !isOverWrite)
                            {
                                rs.IsCopyed = true;
                                return rs;
                            }
                        }
                        sourceFile.CopyTo(targetFilePath, isOverWrite);
                        rs.IsCopyed = true;
                    }
                    else
                    {
                        rs.Message = "Source file is not exist";
                        rs.IsCopyed = false;
                    }
                }
            }
            catch (Exception e)
            {
                rs.Message = e.Message;
                rs.IsCopyed = false;
                logger.Error("Copy file failed:{0}", e);
            }
            return rs;
        }

        private void DeleteDirectory(string volume, string location, bool deleteParentIfNoSubNode)
        {
            string[] directoryNames = volume.Split(new char[] { FSSystemConst.SEPARATER_CHAR }, StringSplitOptions.RemoveEmptyEntries);
            List<string> directoryPaths = new List<string>();
            using (this.openParam.StorageIdentity.Impersonate())
            {
                for (int i = 0; i < directoryNames.Length; i++)
                {
                    directoryNames[i] = directoryNames[i].TrimEnd('\\').TrimEnd('/') + "\\";
                    if (i == 0)
                    {

                        directoryPaths.Add(PathUtil.CombinePath(location, directoryNames[i]));
                    }
                    else
                    {
                        directoryPaths.Add(PathUtil.CombinePath(directoryPaths[i - 1], directoryNames[i]));
                    }
                }

                for (int i = directoryPaths.Count - 1; i >= 0; i--)
                {
                    if (i == directoryPaths.Count - 1)
                    {
                        if (Alphaleonis.Win32.Filesystem.Directory.Exists(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPaths[i])))
                        {
                            Alphaleonis.Win32.Filesystem.Directory.Delete(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPaths[i]), true);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (deleteParentIfNoSubNode)
                        {
                            if (i < directoryPaths.Count - 1)
                            {
                                if (Alphaleonis.Win32.Filesystem.Directory.GetDirectories(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPaths[i])).Length == 0 && Alphaleonis.Win32.Filesystem.Directory.GetFiles(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPaths[i])).Length == 0)
                                {
                                    Alphaleonis.Win32.Filesystem.Directory.Delete(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryPaths[i]), true);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public override string CombinePath(string firstPath, string secondPath)
        {
            if (string.IsNullOrEmpty(firstPath))
            {
                return secondPath;
            }
            if (string.IsNullOrEmpty(secondPath) || secondPath.Equals("\\", StringComparison.OrdinalIgnoreCase))
            {
                return firstPath;
            }
            if (secondPath.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                secondPath = secondPath.TrimStart(new char[] { '\\' });
            }
            return Path.Combine(firstPath, secondPath);
        }


        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem)
        {
            throw new NotSupportedException();
        }
    }
}

