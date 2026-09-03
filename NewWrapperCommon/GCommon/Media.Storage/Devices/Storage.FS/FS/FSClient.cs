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


using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AvePoint.Media.Storage.FS
{


    #region CodeReview
    [AveCodeReview(
   "2012/8/9",
   "rongbiao.sun@avepoint.com",
   "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
    null,
    true)]
    #endregion
    class FSClient : AbstractFSClient
    {
        StorageLogger logger = new StorageLogger(typeof(FSClient));
        FSClientOpenParam openParam;
        public FSClient(FSClientOpenParam param)
        {
            this.openParam = param;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            try
            {
                XStream stream = null;
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    try
                    {
                        stream = new FSStream(info, this.openParam.StorageSystem, fileMode);
                    }
                    catch (CatchedToDoMoreExcetion ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        String directoryPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName).TrimEnd('\\').TrimEnd('/') + "\\";
                        Directory.CreateDirectory(directoryPath);
                        stream = new FSStream(info, this.openParam.StorageSystem, fileMode);
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
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error("Open stream failed for file {0}, message {1}", info.HighPlusLowName, ex);
                throw new AuthenticationFailedException(String.Format("Open stream failed for file {0}", info.HighPlusLowName), ex);
            }
            catch (Exception e)
            {
                logger.Error("Open stream failed for file {0}, message {1}", info.HighPlusLowName, e);
                throw;
            }
        }
        public override bool DirectoryExists(StorageInfo info)
        {
            try
            {
                Boolean isExist = false;
                String directoryPath = info.HighName;
                if (!String.IsNullOrEmpty(info.LowName))
                {
                    directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
                }
                String fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    isExist = Directory.Exists(fileFullPath);
                }
                return isExist;
            }
            catch (Exception e)
            {
                logger.Error("Check directory {0} failed: {1}", info.HighPlusLowName, e);
                throw;
            }
        }
        public override bool FileExists(StorageInfo info)
        {
            Boolean isExist = false;
            String fullPath = String.Empty;
            try
            {
                fullPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName);
                fullPath = PathUtil.CombinePath(fullPath, info.LowName);
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (Directory.Exists(this.openParam.SystemLocation))
                    {
                        isExist = File.Exists(fullPath);
                    }
                    else
                    {
                        throw new DeviceNotAvailableException(String.Format("The location [{0}] is not available.", this.openParam.OriginalSystemLocation));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Check file {0} failed: {1}:", fullPath, ex);
                throw;
            }
            return isExist;
        }
        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            var rs = new StorageDeleteResult();
            var directoryPath = info.HighName;
            if (!String.IsNullOrEmpty(info.LowName))
            {
                directoryPath = PathUtil.CombinePath(info.HighName, info.LowName);
            }
            directoryPath = directoryPath.TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (Directory.Exists(PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath)))
                {
                    try
                    {
                        this.DeleteDirectorFileAndGetLength(rs, this.openParam.SystemLocation, directoryPath);
                        logger.Debug("Get directory size finished, try to delete directory.");
                        this.DeleteDirectory(directoryPath, this.openParam.SystemLocation, info.IsDeleteParentFolder);
                    }
                    catch (IOException e)
                    {
                        this.logger.Warn("Delete the file failed, maybe the path is too long, try to delete with alphaFS. Error: {0}", e);
                        rs.DeleteExceptionType = DeleteExceptionType.IOException;
                        return rs;
                    }
                    rs.IsDeleted = true;
                }
                else
                {
                    if (Directory.Exists(this.openParam.SystemLocation))
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
        private void DeleteDirectory(string directoryRelativePath, string originalSystemLocation, bool deleteParentIfNoSubNode)
        {
            String[] directoryNames = directoryRelativePath.Split(new Char[] { FSSystemConst.SEPARATER_CHAR }, StringSplitOptions.RemoveEmptyEntries);
            List<String> directoryPaths = new List<String>();
            using (this.openParam.StorageIdentity.Impersonate())
            {
                for (int i = 0; i < directoryNames.Length; i++)
                {
                    directoryNames[i] = directoryNames[i].TrimEnd('\\').TrimEnd('/') + "\\";
                    if (i == 0)
                        directoryPaths.Add(PathUtil.CombinePath(originalSystemLocation, directoryNames[i]));
                    else
                        directoryPaths.Add(PathUtil.CombinePath(directoryPaths[i - 1], directoryNames[i]));
                }
                for (int i = directoryPaths.Count - 1; i >= 0; i--)
                {
                    if (i == directoryPaths.Count - 1)
                    {
                        if (Directory.Exists(directoryPaths[i]))
                            Directory.Delete(directoryPaths[i], true);
                        else
                            break;
                    }
                    else
                    {
                        if (deleteParentIfNoSubNode)
                        {
                            if (Directory.GetDirectories(directoryPaths[i]).Length == 0 && Directory.GetFiles(directoryPaths[i]).Length == 0)
                                Directory.Delete(directoryPaths[i], true);
                        }
                        else
                            break;
                    }
                }
            }
        }
        public long GetDirectoryLength(string dirPath)
        {
            Int64 len = 0;
            var dir = new DirectoryInfo(dirPath);
            if (ShouldSkipDirectory(dir))
            {
                logger.Warn("Skip directory length calculation for special directory: {0}", dirPath);
                return 0;
            }
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    len += file.Length;
                }
                catch (FileNotFoundException e)
                {
                    Trace.TraceWarning(e.ToString());
                }
            }
            DirectoryInfo[] dis = dir.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }
        private bool ShouldSkipDirectory(DirectoryInfo dir)
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
        private void DeleteDirectorFileAndGetLength(StorageDeleteResult rs, string systemLocation, string directoryPath)
        {
            using (this.openParam.StorageIdentity.Impersonate())
            {
                var dir = new DirectoryInfo(PathUtil.CombinePath(systemLocation, directoryPath));
                if (dir.Attributes < FileAttributes.ReparsePoint)
                {
                    var dirsList = dir.GetDirectories();
                    var filesList = dir.GetFiles();
                    foreach (FileInfo file in filesList)
                    {
                        try
                        {
                            rs.DeletedFileSize += this.DeleteFile(new StorageInfo() { HighName = directoryPath, LowName = file.Name }).DeletedFileSize;
                        }
                        catch (FileNotFoundException e)
                        {
                            Trace.TraceWarning(e.ToString());
                        }

                    }
                    DirectoryInfo[] dis = dirsList;
                    if (dis.Length > 0)
                    {
                        for (int i = 0; i < dis.Length; i++)
                        {
                            this.DeleteDirectorFileAndGetLength(rs, systemLocation, PathUtil.CombinePath(directoryPath, dis[i].Name));
                        }
                    }
                }
            }
        }
        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var rs = new StorageDeleteResult();
            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, info.HighName);
            var fileFullName = PathUtil.CombinePath(fileFullPath, info.LowName);
            if (this.openParam.securelyDelete)
            {
                WipeFile(info);
            }
            using (this.openParam.StorageIdentity.Impersonate())
            {
                var fileInfo = new FileInfo(fileFullName);
                try
                {
                    rs.DeletedFileSize = fileInfo.Length;
                    File.Delete(fileFullName);
                    rs.IsDeleted = true;
                }
                catch (UnauthorizedAccessException ue)
                {
                    logger.Warn("File [{0}] can't be accessed.", fileFullName);
                    rs.Message = ue.Message;
                    rs.IsUnauthorizedAccessException = true;
                    return rs;
                }
                catch (FileNotFoundException fe)//通过异常处理文件不存在的问题。由于AlphaFS异常返回特殊，暂不处理长路径情况
                {
                    if (Directory.Exists(this.openParam.SystemLocation))
                    {
                        logger.Debug("The file [{0}] you want to delete is no longer exist.", fileFullName);
                        rs.IsDeleted = true;
                    }
                    else
                    {
                        logger.Warn("Can't access to parent folder for file [{0}], delete failed.", fileFullName);
                        rs.IsDeleted = false;
                        rs.Message = fe.Message;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Occurred an error when delete the file [{0}], error {1}", fileFullName, e);
                    throw;
                }
            }
            return rs;
        }
        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            FSDirectoryInfo irectoryInfo = null;
            var directoryPath = dirInfo.HighName;
            if (!String.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!Directory.Exists(fileFullPath))
                {
                    if (mode != FileMode.Open)
                    {
                        if (!this.openParam.IsReadonly)
                        {
                            Directory.CreateDirectory(fileFullPath);
                            irectoryInfo = new FSDirectoryInfo(new DirectoryInfo(fileFullPath), directoryPath);
                            irectoryInfo.System = this.openParam.StorageSystem;
                        }
                        else
                        {
                            throw new MethodNotSupportForReadOnlyDeviceException("The current device is read-only");
                        }
                    }
                }
                else
                {
                    irectoryInfo = new FSDirectoryInfo(new DirectoryInfo(fileFullPath), directoryPath);
                    irectoryInfo.System = this.openParam.StorageSystem;
                }
                if (irectoryInfo != null)
                {
                    DirectoryInfo dInfo = new DirectoryInfo(fileFullPath);
                    AssembleDirAttribute(irectoryInfo, dInfo, dirInfo);
                }
            }
            return irectoryInfo;
        }
        private void AssembleDirAttribute(XDirectoryInfo irectoryInfo, DirectoryInfo dirInfo, StorageInfo info)
        {
            try
            {
                irectoryInfo.UserName = this.openParam.SystemUserName;
                irectoryInfo.Password = this.openParam.SystemPassword;
                irectoryInfo.Domain = this.openParam.SystemDomain;
                irectoryInfo.UNCFullPath = PathUtil.CombinePath(openParam.SystemLocation, info.HighPlusLowName);
                irectoryInfo.OriginalDirFullPath = PathUtil.CombinePath(this.openParam.OriginalSystemLocation, info.HighPlusLowName);
            }
            //TODO
            catch (PathTooLongException ex)
            {
                logger.Warn("AssembleDirAttribute :{0} ", ex);
                throw;
            }
            catch (Exception e)
            {
                this.logger.Error("AssembleDirAttribute :{0} ", e);
            }
        }
        private void AssembleFileAttribute(XFileInfo xfileInfo, FileInfo info)
        {
            try
            {
                xfileInfo.UserName = this.openParam.SystemUserName;
                xfileInfo.Password = this.openParam.SystemPassword;
                xfileInfo.Domain = this.openParam.SystemDomain;
                xfileInfo.OriginalFileFullPath = PathUtil.CombinePath(this.openParam.OriginalSystemLocation, xfileInfo.HighPlusLowName);
            }
            catch (PathTooLongException ex)
            {
                this.logger.Warn("AssembleFileAttribute :{0}", ex);
                throw;
            }
            catch (Exception e)
            {
                this.logger.Error("AssembleFileAttribute :{0}", e);
            }
        }
        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            var filePath = PathUtil.CombinePath(this.openParam.SystemLocation, fileInfo.HighName);
            filePath = PathUtil.CombinePath(filePath, fileInfo.LowName);
            FSFileInfo fsfileInfo = null;
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (File.Exists(filePath))
                {
                    var info = new FileInfo(filePath);
                    fsfileInfo = new FSFileInfo(info, fileInfo.HighName, fileInfo.LowName);
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
            var directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                DirectoryInfo d = new DirectoryInfo(fileFullPath);
                DirectoryInfo[] ds = d.GetDirectories();
                FSDirectoryInfo fsf;
                foreach (DirectoryInfo dir in ds)
                {
                    if (PathUtil.CombinePath(fileFullPath, dir.Name).Length >= 248)
                    {
                        throw new PathTooLongException("Directory path length is too long under the folder.");
                    }
                    fsf = new FSDirectoryInfo(dir, PathUtil.CombinePath(directoryPath, dir.Name));
                    AssembleDirAttribute(fsf, dir, dirInfo);
                    fsf.System = this.openParam.StorageSystem;
                    xfs.Add(fsf);
                }
            }
            return xfs;
        }
        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            List<XFileInfo> xfs = new List<XFileInfo>();
            var directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }
            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";
            using (this.openParam.StorageIdentity.Impersonate())
            {
                var dir = new DirectoryInfo(fileFullPath);
                FileInfo[] fs = dir.GetFiles();
                FSFileInfo fsf = new FSFileInfo();
                foreach (FileInfo file in fs)
                {
                    if (PathUtil.CombinePath(fileFullPath, file.Name).Length >= 260)
                    {
                        throw new PathTooLongException("File path length is too long under the folder.");
                    }
                    fsf = new FSFileInfo(file, directoryPath, file.Name);
                    AssembleFileAttribute(fsf, file);
                    fsf.Length = file.Length;
                    fsf.System = this.openParam.StorageSystem;
                    xfs.Add(fsf);
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

            var directoryPath = dirInfo.HighName;
            if (!string.IsNullOrEmpty(dirInfo.LowName))
            {
                directoryPath = PathUtil.CombinePath(dirInfo.HighName, dirInfo.LowName);
            }

            var fileFullPath = PathUtil.CombinePath(this.openParam.SystemLocation, directoryPath).TrimEnd('\\').TrimEnd('/') + "\\";

            using (this.openParam.StorageIdentity.Impersonate())
            {
                var dir = new DirectoryInfo(fileFullPath);
                var batch = new List<XFileInfo>(batchSize);

                foreach (var file in dir.EnumerateFiles())
                {
                    if (PathUtil.CombinePath(fileFullPath, file.Name).Length >= 260)
                    {
                        throw new PathTooLongException("File path length is too long under the folder.");
                    }

                    var fsf = new FSFileInfo(file, directoryPath, file.Name);
                    AssembleFileAttribute(fsf, file);
                    fsf.System = this.openParam.StorageSystem;
                    fsf.Length = file.Length;
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
                var dir = new DirectoryInfo(fileFullPath);
                var batch = new List<XDirectoryInfo>(batchSize);

                foreach (var subDir in dir.EnumerateDirectories())
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

                    var fsf = new FSDirectoryInfo(subDir, PathUtil.CombinePath(directoryPath, subDir.Name));
                    //var files = subDir.GetFiles("*.*", SearchOption.AllDirectories);
                    //long size = files.Sum(f => f.Length);
                    //int fileCount = files.Length;
                    //fsf.TotalFileCount = fileCount;
                    //fsf.Length = size;
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

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            var result = new StorageListResult();
            result.Files = ListFiles(dirInfo);
            result.SubDirs = ListDirectories(dirInfo);
            return result;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "fsdl")]
        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            var rs = new StorageMoveResult();
            var sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName));
            var targetFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName));
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!File.Exists(sourceFilePath))
                {
                    rs.IsMoved = false;
                    rs.Message = "File can not be found. Path: " + sourceFilePath;
                }
                else if (targetFilePath.TrimEnd(new char[] { ' ', '\\' }).Equals(sourceFilePath.TrimEnd(new char[] { ' ', '\\' })))
                {
                    rs.IsMoved = false;
                    rs.Message = "The source file has the same path with the target file.";
                }
                else if (!File.Exists(targetFilePath))
                {
                    try
                    {
                        var parentFolerPath = Path.GetDirectoryName(targetFilePath).TrimEnd('\\').TrimEnd('/') + "\\";
                        if (!Directory.Exists(parentFolerPath))
                        {
                            Directory.CreateDirectory(parentFolerPath);
                        }
                        File.Move(sourceFilePath, targetFilePath);
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
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                        File.Move(targetFilePath, tempFilePath);
                        try
                        {
                            File.Move(sourceFilePath, targetFilePath);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            File.Move(tempFilePath, targetFilePath);
                            throw;
                        }
                        try
                        {
                            var tmpFile = new FileInfo(tempFilePath);
                            if (tmpFile.IsReadOnly == true)
                            {
                                tmpFile.IsReadOnly = false;
                            }
                            File.Delete(tempFilePath);
                            var fileinforead = new FileInfo(targetFilePath);
                            if (fileinforead.IsReadOnly == true)
                            {
                                fileinforead.IsReadOnly = false;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.Warn("Change read-only to false failed,File:[{0}], Error: {1}", targetFilePath, ex);
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
            var rs = new StorageMoveResult();
            var sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName));
            var targetFilePath = PathUtil.CombinePath(destSystem.SystemLocation, PathUtil.CombinePath(destFile.HighName, destFile.LowName));
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!File.Exists(sourceFilePath))
                {
                    rs.IsMoved = false;
                    rs.Message = "File can not be found. Path: " + sourceFilePath;
                }
                else if (targetFilePath.TrimEnd(new char[] { ' ', '\\' }).Equals(sourceFilePath.TrimEnd(new char[] { ' ', '\\' })))
                {
                    rs.IsMoved = false;
                    rs.Message = "The source file has the same path with the target file.";
                }
                else if (!File.Exists(targetFilePath))
                {
                    try
                    {
                        string parentFolerPath = Path.GetDirectoryName(targetFilePath);
                        if (!Directory.Exists(parentFolerPath))
                        {
                            Directory.CreateDirectory(parentFolerPath);
                        }
                        File.Move(sourceFilePath, targetFilePath);
                    }
                    catch (System.Exception ex)
                    {
                        rs.IsMoved = false;
                        rs.Message = "The file can't be moved. " + ex.Message;
                    }
                }
                else
                {
                    var tempFilePath = sourceFilePath + "_fsdl_temp";
                    try
                    {
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                        File.Move(targetFilePath, tempFilePath);
                        try
                        {
                            File.Move(sourceFilePath, targetFilePath);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            File.Move(tempFilePath, targetFilePath);
                            throw;
                        }
                        try
                        {
                            FileInfo tmpFile = new FileInfo(tempFilePath);
                            if (tmpFile.IsReadOnly == true)
                            {
                                tmpFile.IsReadOnly = false;
                            }
                            File.Delete(tempFilePath);
                            FileInfo fileinforead = new FileInfo(targetFilePath);
                            if (fileinforead.IsReadOnly == true)
                            {
                                fileinforead.IsReadOnly = false;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.Warn("Change read-only to false failed,File:[{0}], Error: {1}", targetFilePath, ex);
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
        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            var rs = new StorageMoveResult() { IsMoved = true };
            var sourceDirPath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceDirInfo.HighName, sourceDirInfo.LowName)).TrimEnd('\\').TrimEnd('/') + "\\";
            var targetDirPath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetDirInfo.HighName, targetDirInfo.LowName)).TrimEnd('\\').TrimEnd('/') + "\\";
            var sourceDir = new DirectoryInfo(sourceDirPath);
            var targetDir = new DirectoryInfo(targetDirPath);
            using (this.openParam.StorageIdentity.Impersonate())
            {
                if (!sourceDir.Exists)
                {
                    rs.IsMoved = false;
                    rs.Message = "Folder can not be found. Path: " + sourceDirPath;
                    return rs;
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
                            targetDir.Parent.Create();
                        }
                        Directory.Move(sourceDirPath, targetDirPath);
                        return rs;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        targetDir.Create();
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
                        MoveFile(sourceFile, targetFile, isOverWrite);
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
        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            var rs = new StorageCopyResult();
            var sourceFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName));
            var targetFilePath = PathUtil.CombinePath(this.openParam.SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName));
            var sourceFile = new FileInfo(sourceFilePath);
            var targetFile = new FileInfo(targetFilePath);
            try
            {
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (sourceFile.Exists)
                    {
                        if (!targetFile.Directory.Exists)
                        {
                            targetFile.Directory.Create();
                        }
                        if (!isOverWrite && targetFile.Exists)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                        sourceFile.CopyTo(targetFilePath, isOverWrite);
                        rs.IsCopyed = true;
                    }
                    else
                    {
                        rs.Message = "source file is not exist";
                        rs.IsCopyed = false;
                    }
                }
            }
            catch (Exception e)
            {
                rs.Message = e.Message;
                rs.IsCopyed = false;
                logger.Error("Copy file failed. Error{0}", e);
            }
            return rs;
        }

        public override Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            var result = true;
            try
            {
                using (this.openParam.StorageIdentity.Impersonate())
                {
                    if (!Directory.Exists(symlinkPath))
                        result = FSUtil.ConvertLongPathToSymlink(symlinkPath, targetPath);
                    if (!result)
                        logger.Error("Convert long path to symbolic link failed ,error code:[{0}]", FSUtil.GetLastError());
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Convert long path to symbolic link failed,{0}", ex);
            }
            return result;
        }

        public override SpeedResults GetNetshareSpeed(IOType type, int writeRatio, string blokeSize, string fileUNCPath)
        {
            var IoType = type == IOType.Sequential ? "-s" : "-r";
            SpeedResults resultObject = null;
            string command = string.Format("\"" + AppDomain.CurrentDomain.BaseDirectory + "diskspd.exe\" -C50M {0} -d10 -w{1} -t1 -b{2} -h -o16 -D -Rxml {3}", IoType, writeRatio, blokeSize, "\"" + fileUNCPath + "\"");
            using (this.openParam.StorageIdentity.Impersonate())
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                logger.Info("Begin to input a command: {0}", command);
                p.StandardInput.WriteLine(command + "&exit");
                p.StandardInput.AutoFlush = true;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
                var xmlIndex = output.IndexOf("<Results>");
                if (xmlIndex < 0)
                {
                    throw new Exception("The xml did not have a valid format : " + output);
                }
                var xml = output.Substring(output.IndexOf("<Results>"));
                logger.Debug("The result Xml is : {0}", xml);//是否要打出来?
                resultObject = SpeedUtil.DeserializeXml(xml);
            }

            return resultObject;
        }



        public override void Close()
        {
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
