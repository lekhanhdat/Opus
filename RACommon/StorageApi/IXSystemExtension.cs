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

namespace AvePoint.Application.StorageApiModern
{
    using AvePoint.RA.CommonUtil;
    using global::Storage;
    using global::Storage.Cloud.Azure;

    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;

    public static class IXSystemExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(IXSystemExtension));

        public static StorageCopyResult CopyFileExt(this IXSystem system, StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            var sourceSystem = system.ToXSystem();
            var targetSystem = destSystem.ToXSystem();
            return sourceSystem.CopyFile(srcFile, targetSystem, destFile, isOverWrite);
        }

        public static StorageCopyResult CopyFileExt(this IXSystem system, StorageInfo srcFile, StorageInfo destFile, bool isOverWrite)
        {
            return system.CopyFile(srcFile, destFile, isOverWrite);
        }

        public static async Task<StorageCopyResult> CopyFileAsyncExt(this IXSystem system, StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            var sourceSystem = system.ToXSystem();
            var targetSystem = destSystem.ToXSystem();
            return await sourceSystem.CopyFileAsync(srcFile, targetSystem, destFile, isOverWrite);
        }

        public static async Task<StorageCopyResult> CopyFileAsyncExt(this IXSystem system, StorageInfo srcFile, StorageInfo destFile, bool isOverWrite)
        {
            return await system.CopyFileAsync(srcFile, destFile, isOverWrite);
        }

        public static IXSystem ToXSystem(this IXSystem system) => (system is XLibrary xLibrary) ? xLibrary.GetWorkingSystem() : system;

        public static List<IXSystem> GetAllSubSystems(this IXSystem system) => (system is XLibrary xLibrary) ? xLibrary.SubSystems : new List<IXSystem> { system };

        public static async Task<StorageResult> UploadAsyncExt(this IXSystem system, Stream stream, StorageInfo info, bool overWrite, CancellationToken cancellationToken)
        {
            var realSystem = system.ToXSystem();
            if (realSystem is IAzureSystem azureSystem)
            {
                return await azureSystem.UploadAsync(stream, info, new ContentUploadOption { Overwrite = overWrite }, cancellationToken);
            }
            else
            {
                if ((!overWrite) && realSystem.StorageInterfaceType == StorageInterfaceType.Namespace && await realSystem.ExistsAsync(info, false))
                {
                    throw new global::Storage.Util.FileAlreadyExistException(info.FullName);
                }
                return await realSystem.UploadAsync(stream, info, cancellationToken);
            }
        }

        public static XFileInfo SafeOpenFile(this IXSystem device, StorageInfo info)
        {
            try
            {
                return PollyRetry.Execute<XFileInfo>(delegate ()
                {
                    var result = device.OpenFile(info);
                    return result;
                });
            }
            catch (Exception ex)
            {
                logger.Error("open data or index with exception: {0}, {1}", info.HighPlusLowName, ex);
                return new XFileInfo() { FileSize = 0 };
            }
        }

        public static (XFileInfo? FileInfo, IXSystem WorkingSystem) OpenFileExt(this IXSystem xSystem, StorageInfo storageInfo)
        {
            if (xSystem is XLibrary library)
            {
                var defaultSystem = library.GetWorkingSystem();
                var fileInfo = defaultSystem.OpenFile(storageInfo);
                if (fileInfo == null || !fileInfo.Exists)
                {
                    foreach (var subSystem in library.SubSystems)
                    {
                        if (subSystem != defaultSystem)
                        {
                            var subFileInfo = subSystem.OpenFile(storageInfo);
                            if (subFileInfo != null && subFileInfo.Exists)
                            {
                                defaultSystem = subSystem;
                                fileInfo = subFileInfo;
                                break;
                            }
                        }
                    }
                }
                return (fileInfo, defaultSystem);
            }
            return (xSystem.OpenFile(storageInfo), xSystem);
        }

        private static HashSet<XStorageType> CloudStorageTypes = new () { XStorageType.Amazon, XStorageType.Azure, XStorageType.Dropbox, XStorageType.S3Compatible };

        public static bool IsCloudStorage(this IXSystem xSystem)
        {
            if (xSystem is null)
            {
                return false;
            }
            return CloudStorageTypes.Contains(xSystem.StorageType);
        }

        public static StorageDeleteResult DeleteFileExt(this IXSystem system, StorageInfo info)
        {
            if (system.StorageType == XStorageType.GoogleCloud)
            {
                var fileInfo = system.OpenFile(info);
                if (fileInfo != null)
                {
                    var result = system.DeleteFile(info);
                    result.DeletedFileSize = fileInfo.FileSize;
                    return result;
                }

                return new StorageDeleteResult()
                {
                    IsDeleted = true,
                };
            }
            return system.DeleteFile(info);
        }
    }
}