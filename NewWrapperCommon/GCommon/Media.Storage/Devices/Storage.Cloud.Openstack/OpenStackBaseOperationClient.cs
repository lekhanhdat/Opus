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
using System.Globalization;
using System.IO;
using System.Text;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackBaseOperationClient
    {
        StorageLogger logger = StorageLogger.GetInstance(typeof(OpenStackBaseOperationClient));
        OpenStackOpenParameter openParameter;
        OpenStackBaseRestClient restClient;
        String containerName;

        public OpenStackBaseOperationClient(OpenStackOpenParameter openParameter)
        {
            this.openParameter = openParameter;
            this.containerName = openParameter.SystemLocation;
            restClient = new OpenStackBaseRestClient(openParameter);
        }

        public StorageOpenValidResult Authentication()
        {
            OpenStackIdentityInfo openStackIdentityInfo = restClient.Authentication();
            if (openStackIdentityInfo.HasAuthentication)
            {
                return new StorageOpenValidResult()
                {
                    IsHasPermission = true,
                    TotalSpace = long.MaxValue - 1,
                    TotalFreeSpace = long.MaxValue - 1,
                    TotalUsedSpace = 0
                };
            }
            else
            {
                throw new AuthenticationFailedException(openStackIdentityInfo.ErrorMessage);
            }
        }

        public virtual Boolean ContainerExists(String containerName)
        {
            return restClient.CheckContainer(containerName);
        }

        public virtual Boolean CreateContainer(String containerName)
        {
            return restClient.CreateContainer(containerName);
        }

        public virtual XStream OpenStream(StorageInfo storageInfo, FileMode fileMode)
        {
            XStream cloudStream;
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    var writerHeaders = new Dictionary<String, String>();
                    this.AddMetadata(storageInfo, writerHeaders);
                    writerHeaders["Content-Type"] = "docave/data".ToLower(CultureInfo.InvariantCulture);
                    var needPartUpload = storageInfo.Length > openParameter.SingleUploadMaxSize || storageInfo.Length > openParameter.MaxFileSize;
                    if (needPartUpload && openParameter.EnableSLO)
                    {
                        cloudStream = new MultiHttpUploadStream(restClient, storageInfo, this.openParameter, writerHeaders);
                    }
                    else
                    {
                        cloudStream = new HttpUploadStream(restClient, storageInfo, this.openParameter, writerHeaders);
                    }
                    break;
                case FileMode.Open:
                    cloudStream = new HttpDownloadStream(restClient, storageInfo);
                    break;
                default:
                    throw new NotSupportedException("Unsupported File Mode: " + fileMode);
            }
            return cloudStream;
        }

        private void AddMetadata(StorageInfo storageInfo, Dictionary<String, String> writerHeaders)
        {
            if (openParameter.CustomizedMetaMode.Equals(CustomizedMode.Close))
            {
                return;
            }
            if (this.openParameter.CustomizedMetaMode.Equals(CustomizedMode.CustomizedOnly))
            {
                foreach (var entry in this.openParameter.CustomizedMetaData)
                {
                    if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders["X-Object-Meta-" + entry.Key] = entry.Value;
                    }
                }
            }
            else if (this.openParameter.CustomizedMetaMode.Equals(CustomizedMode.DocAveOnly))
            {
                foreach (var entry in storageInfo.MetaInfos)
                {
                    if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (this.openParameter.CustomizedMetaMode.Equals(CustomizedMode.SupportAll))
            {
                foreach (var entry in this.openParameter.CustomizedMetaData)
                {
                    if (!String.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders["X-Object-Meta-" + entry.Key] = entry.Value;
                    }
                }
                foreach (var entry in storageInfo.MetaInfos)
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
        }

        public virtual Boolean DirectoryExists(StorageInfo info)
        {
            var urlParameters = new Dictionary<String, String> { { "limit", "1" } };
            var files = restClient.ListAllObjects(info.HighName, info.LowName, urlParameters);
            return files != null && files.Count > 0;
        }

        public virtual XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            OpenStackDirectoryInfo dir = null;
            //string name = "/".Equals(dirInfo.LowName) ? "" : dirInfo.LowName;
            var headers = new Dictionary<string, string>();
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                    headers["Content-Type"] = "docave/directory".ToLower(CultureInfo.InvariantCulture);
                    headers["Content-Length"] = "0";
                    restClient.CreateObjectWithNoContent(dirInfo.HighName, dirInfo.LowName, headers);
                    dir = new OpenStackDirectoryInfo(dirInfo.HighName, dirInfo.LowName, true);
                    break;
                case FileMode.Open:
                    var exists = restClient.CheckObject(dirInfo.HighName, dirInfo.LowName);
                    if (exists)
                    {
                        dir = new OpenStackDirectoryInfo(dirInfo.HighName, dirInfo.LowName, true);
                    }
                    break;
                default:
                    throw new UnsupportedXException("Unsupported File Mode: " + mode.ToString());
            }
            return dir;
        }

        public virtual StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            if (openParameter.EnableBulkDelete)
            {
                return BulkDeleteDirectory(info);
            }
            var rs = new StorageDeleteResult();
            Int64 totalDeletedSize = 0;
            var objects = this.restClient.ListAllObjects(info.HighName, info.LowName);
            if (objects != null && objects.Count > 0)
            {
                foreach (var subObject in objects)
                {
                    this.restClient.DeleteObject(this.containerName, subObject.HighPlusLowName);
                    totalDeletedSize = totalDeletedSize + subObject.FileSize;
                }
            }
            rs.IsDeleted = true;
            rs.DeletedFileSize = totalDeletedSize;
            return rs;
        }

        public virtual StorageDeleteResult BulkDeleteDirectory(StorageInfo info)
        {
            var rs = new StorageDeleteResult();
            Int64 totalDeletedSize = 0;
            var objects = restClient.ListAllObjects(info.HighName, info.LowName);
            if (objects != null && objects.Count > 0)
            {
                var deleteContent = new StringBuilder();
                foreach (var subObject in objects)
                {
                    deleteContent.Append("/").Append(subObject.HighName).Append("/").Append(subObject.LowName).Append("\r\n");
                    totalDeletedSize = totalDeletedSize + subObject.FileSize;
                }
                restClient.BulkDelete(deleteContent.ToString());
            }
            rs.IsDeleted = true;
            rs.DeletedFileSize = totalDeletedSize;
            return rs;
        }

        public virtual StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            var results = restClient.ListAllDirFiles(dirInfo.HighName, dirInfo.LowName);
            return results;
        }

        public virtual StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public virtual List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public virtual List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        public virtual StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            var rs = new StorageCopyResult();
            if (FileExists(sourceFileInfo))
            {
                if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                {
                    var targetFileExists = FileExists(targetFileInfo);
                    if (targetFileExists && !isOverWrite)
                    {
                        rs.IsCopyed = true;
                        return rs;
                    }
                }
                rs.IsCopyed = restClient.CopyFile(sourceFileInfo.HighName, sourceFileInfo.LowName, targetFileInfo.HighName, targetFileInfo.LowName);
            }
            else
            {
                rs.Message = "source file is not exist";
                rs.IsCopyed = false;
            }
            return rs;
        }

        public virtual StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var deleteResult = new StorageDeleteResult();
            var fileInfo = this.restClient.GetObjectInfo(info.HighName, info.LowName);
            if (fileInfo == null)
            {
                deleteResult.IsDeleted = true;
            }
            else
            {
                deleteResult.IsDeleted = restClient.DeleteObject(info.HighName, info.LowName);
                deleteResult.DeletedFileSize = fileInfo.FileSize;
            }
            return deleteResult;
        }

        public virtual Boolean FileExists(StorageInfo info)
        {
            return restClient.CheckObject(info.HighName, info.LowName);
        }

        public virtual StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {
            var moveResult = new StorageMoveResult();
            var copyResult = CopyDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
            if (copyResult.IsCopyed)
            {
                DeleteDirectory(sourceDirInfo);//TODO 成功Move之后不会返回moveResult.IsMoved = true;
                moveResult.IsMoved = true;
            }
            else
            {
                moveResult.IsMoved = false;
                moveResult.Message = copyResult.Message;
            }
            return moveResult;
        }

        public StorageCopyResult CopyDirectory(StorageInfo sourceFolderInfo, StorageInfo targetFolderInfo, Boolean isOverWrite)
        {
            var copyResult = new StorageCopyResult();
            if (restClient.CheckObject(sourceFolderInfo.HighName, sourceFolderInfo.LowName))
            {
                //先copy子文件
                var listResult = ListSubDirectoriesAndFiles(sourceFolderInfo);
                foreach (var file in listResult.Files)
                {
                    //var sourceFileInfo = XConvert.FromNames(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                    var sourceFileInfo = new StorageInfo(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), file.Name);
                    var targetFileInfo = new StorageInfo(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), file.Name);
                    if (!CopyFile(sourceFileInfo, targetFileInfo, isOverWrite).IsCopyed)
                    {
                        copyResult.IsCopyed = false;
                        return copyResult;
                    }
                }
                //遍历文件夹的子文件夹
                foreach (var directory in listResult.SubDirs)
                {
                    var sourceSubFolderInfo = new StorageInfo(PathUtil.CombinePath(sourceFolderInfo.HighName, sourceFolderInfo.LowName), directory.Name + "/");
                    var targetSubFolderInfo = new StorageInfo(PathUtil.CombinePath(targetFolderInfo.HighName, targetFolderInfo.LowName), directory.Name + "/");
                    if (!CopyDirectory(sourceSubFolderInfo, targetSubFolderInfo, isOverWrite).IsCopyed)
                    {
                        copyResult.IsCopyed = false;
                        return copyResult;
                    }
                }
                copyResult.IsCopyed = true;
            }
            return copyResult;
        }

        public virtual StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            var moveResult = new StorageMoveResult();
            var copyResult = this.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
            if (copyResult.IsCopyed)
            {
                if (this.DeleteFile(sourceFileInfo).IsDeleted)
                {
                    moveResult.IsMoved = true;
                    var url = new XURIResult
                        {
                            SdType = 408,
                            //SInfo = new StorageInfo { ObjectId = copyResult.URI.SInfo.ObjectId } //TODO 空引用 copyResult.URI是null
                            SInfo = targetFileInfo
                        };
                    moveResult.URI = url;
                }
            }
            return moveResult;
        }

        public virtual XFileInfo OpenFile(StorageInfo fileInfo)
        {
            return restClient.GetObjectInfo(fileInfo.HighName, fileInfo.LowName);
        }

        public virtual void Close()
        {
            logger.Info("OpenStackBaseOperationClient Close.");//TODO
        }
    }
}
