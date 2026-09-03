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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using Azure.Storage.Blobs;
using DataExportCore.Utils;
using Merged18NResources.MediaCoreIndex;
using Storage;
using System.Diagnostics;
using System.Text;
using Util.MSAzure;

namespace DataExportCore.Content
{
    public static class IndexDownloader
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(IndexDownloader));

        public static IndexDatabaseDownLoadResult Download(IndexDatabaseInfo dbInfo, ArchiverIndexServiceOpenParameter openParam)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            if (openParam.IndexLogicalDeviceSystem.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                StorageInfo logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, dbInfo.DbFileName, dbInfo.StorageInfo);
                logger.Info(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultIndexInfo, openParam.IndexVolume, dbInfo.DbFileName, openParam.IndexLogicalDeviceSystem.SystemLocation);
                if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
                {
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, PathUtil.CombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, PathUtil.CombinePath(openParam.IndexVolume, dbInfo.DbFileName)));
                }
                else
                {
                    if (dbInfo.IsNeedCreateNewIndex)
                        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, PathUtil.CombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, PathUtil.CombinePath(openParam.IndexVolume, dbInfo.DbFileName)));
                    else
                        throw new FileNotFoundException(String.Format(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultFileNotFoundException, PathUtil.CombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, PathUtil.CombinePath(openParam.IndexVolume, dbInfo.DbFileName))));
                }
                openParam.IndexLogicalDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.OpenOrCreate);
            }
            else
            {
                logger.Info(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultBegin, dbInfo.DbFileName);
                var param = new IndexCacheManagerParameter()
                {
                    StorageInfo = dbInfo.StorageInfo,
                    IndexName = dbInfo.DbFileName,
                    IndexVolume = openParam.IndexVolume,
                    CacheSetting = openParam.CacheSetting,
                    CacheSystem = openParam.IndexCacheDeviceSystem,
                    StorageSystem = openParam.IndexLogicalDeviceSystem,
                    NeedDownLoad = dbInfo.NeedDownLoad,
                    DataMode = dbInfo.DataMode,
                    EncryptionInfo = dbInfo.EncryptionInfo,
                };
                indexDownLoadInfo = DownLoadIndex(param);
                var lastAccessTime = new Dictionary<IndexDatabaseProperties, String>
                {
                    [IndexDatabaseProperties.LastAccessTime] = DateTime.UtcNow.Ticks.ToString()
                };
                IndexDatabasePropertiesManager.SaveDBProperties(openParam.IndexVolume, dbInfo.DbFileName + ".properties", openParam.IndexCacheDeviceSystem, lastAccessTime);
            }
            stopwatch.Stop();
            logger.Info($"IndexDatabaseSynchronizer Download finish.UseTime:{stopwatch.Elapsed}.");
            return indexDownLoadInfo;
        }

        private static IndexDatabaseDownLoadResult DownLoadIndex(IndexCacheManagerParameter param)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            StorageInfo cahceStorageInfo = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);
            String indexFileFullPath = PathUtil.CombinePath(PathUtil.CombinePath(param.StorageSystem.LocalTempPath, param.IndexVolume), param.IndexName);
            String connectionString = String.Format("Data Source = {0}", indexFileFullPath);
            Guid connectionID = Guid.NewGuid();
            IndexDatabaseStatus result;
            if (ConnectionLockManager.GetConnectionLock(connectionString, connectionID, ConnectionLockType.Download))
            {
                try
                {
                    param.CacheSystem.DeleteFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                    MakeSureCacheHaveEnoughSpaceBeforeDownload(param);
                    DownloadFromRealXSystem(param);
                    XFileInfo fileInfo = param.CacheSystem.OpenFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                    result = IndexDatabaseStatus.Downloaded;
                }
                finally
                {
                    ConnectionLockManager.RemoveConnectionLock(connectionString, connectionID);
                }
            }
            bool fileExist = param.CacheSystem.FileExists(cahceStorageInfo);
            result = fileExist ? IndexDatabaseStatus.Cached : IndexDatabaseStatus.Nonexistent;
            IndexDatabaseDownLoadResult indexInfo = new IndexDatabaseDownLoadResult(result, PathUtil.CombinePath(param.CacheSystem.SystemLocation, PathUtil.CombinePath(param.IndexVolume, param.IndexName)));
            param.CacheSystem.OpenDirectory(XConvert.FromNames(param.IndexVolume, string.Empty), FileMode.Create);
            logger.Info(MediaCoreIndexResource.IndexCacheManagerDownLoadIndexResult, indexInfo.IndexFullPath, result.ToString());
            stopwatch.Stop();
            logger.Info($"Successful DownLoadIndexAsync.UseTime:{stopwatch.Elapsed}.");
            return indexInfo;
        }

        private static void DownloadFromRealXSystem(IndexCacheManagerParameter param)
        {
            //logger.Info(MediaCoreIndexResource.IndexCacheManagerDownloadFromRealXSystemBegin, param.IndexName, param.IndexVolume);
            logger.Info($"Start downloading index database [{param.IndexName}] in [{param.IndexVolume}]");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var cacheBuffer = new Byte[1024 * 64];
            var info = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);

            info.Length = param.StorageSystem.OpenFile(info).FileSize;//针对cloud介质
            logger.Info($"Begin open stream for download index, the info is {info.ToString()}, file size {info.Length}");
            var storageSystem = param.StorageSystem as AbstractXSystem;
            if (storageSystem != null && storageSystem.StorageType == XStorageType.Azure)
            {
                if (string.IsNullOrEmpty(storageSystem.ConnectionString))
                {
                    var storageLibrary = storageSystem as XLibrary;
                    if (storageLibrary == null)
                    {
                        logger.Error($"Index storage system not a XLibrary.");
                    }
                    storageSystem = storageLibrary?.SubSystems?.FirstOrDefault() as AbstractXSystem;
                    if (storageSystem == null)
                    {
                        logger.Error($"Index storage system's connection string is null.");
                    }
                }

                var containerClient = GlobalCache.IndexDeviceId.Equals(ExportUtility.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase) ? CustomGetBlobContainerClientByStorageXRI(storageSystem?.ConnectionString)
                    : RAStorageUtil.GetBlobContainerClientByStorageXRI(storageSystem?.ConnectionString);
                var cachelocation = ((XLibrary)param.CacheSystem).SubSystems[0].SystemLocation;
                AzureUtil.DownloadBlobToAsync(containerClient, info.HighPlusLowName, SecurityUtils.SafeCombinePath(cachelocation, info.HighPlusLowName)).GetAwaiter().GetResult();
            }
            else
            {
                using (var downloader = param.StorageSystem.OpenStream(info, FileMode.Open))
                {
                    //TODO
                    //downloader.BeginRead(info);
                    using (var encryptedStream = new IndexDecryptedStream(downloader, param.EncryptionInfo))
                    {
                        try
                        {
                            using (var cacheStream = param.CacheSystem.OpenStream(info, FileMode.CreateNew))
                            {
                                Int32 readLen = 0;
                                while ((readLen = encryptedStream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                                {
                                    cacheStream.Write(cacheBuffer, 0, readLen);
                                }
                                //TODO
                                //downloader.EndRead();
                                cacheStream.Flush();
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"something went wrong when CacheSystem.OpenStream message:{e.ToString()}");
                            throw;
                        }
                    }
                }
            }

            stopwatch.Stop();
            logger.Info($"Successful DownloadFromRealXSystem.UseTime:{stopwatch.Elapsed}.");
        }

        private static void MakeSureCacheHaveEnoughSpaceBeforeDownload(IndexCacheManagerParameter param)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (param.CacheSetting == null)
            {
                param.CacheSetting = new CacheSettingDto();
                param.CacheSetting.LimitFreeSpace = 1024L * 1024L * 1024L;
            }
            ulong minimumFreeSpace = param.CacheSetting.LimitFreeSpace;
            StorageInfo info = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);
            var indexInfo = param.StorageSystem.OpenFile(info);
            stopwatch.Stop();
            logger.Info($"MakeSureCacheHaveEnoughSpaceBeforeDownload OpenFile UseTime:{stopwatch.Elapsed}.");
            if (indexInfo == null)
            {
                throw new FileNotFoundException(string.Format(MediaCoreIndexResource.IndexCacheRetentionManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadException, info.ToString()));
            }
            //SAAS-37634 Some blob files were archived by DAO manager, so media service will throw before download

            //TODO
            //if (indexInfo.FileTierType == AccessTierType.Archive)
            //{
            //    string errorMsg = $"{MediaCoreIndexResource.CannotProcessArchivedBlobException}, details:{info.ToString()}";
            //    logger.Error(errorMsg);
            //    throw new InvalidDataException(MediaCoreIndexResource.CannotProcessArchivedBlobException);
            //}
            var fileSize = indexInfo.FileSize;
            Dictionary<StorageInfo, long> lastAccessTimeDictionary = [];
            bool findAvailableCache = false;
            foreach (var subSystem in ((XLibrary)param.CacheSystem).SubSystems)
            {
                if (!(subSystem is IXSpaceInfo spaceInfo))
                {
                    logger.Warn("The system does not support space related properties");
                    continue;
                }
                ulong totalFreeSpace = spaceInfo.TotalFreeSpace;
                try
                {
                    param.CacheSystem.FindCondition = new Predicate<IXSystem>(xSystem => { return spaceInfo.TotalFreeSpace > minimumFreeSpace + (ulong)fileSize; });
                    logger.Info(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadSucceed, param.CacheSystem.SystemLocation);
                    findAvailableCache = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (totalFreeSpace < minimumFreeSpace + (ulong)fileSize)
                    {
                        lastAccessTimeDictionary = GetLastAccessTimeDictionary(subSystem);
                    }
                    if (DoIndexRetentionByLastAccessTime(lastAccessTimeDictionary, param.CacheSystem, totalFreeSpace, minimumFreeSpace))
                    {
                        findAvailableCache = true;
                    }
                    else
                    {
                        logger.Warn(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadWarn, param.CacheSystem.SystemLocation, ex.ToString());
                    }
                }
            }
            if (!findAvailableCache)
                throw new SpaceNotEnoughException(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadNoEnoughSpaceException);
        }

        private static Dictionary<StorageInfo, long> GetLastAccessTimeDictionary(IXSystem cacheSystem)
        {
            Dictionary<StorageInfo, long> fileList = new Dictionary<StorageInfo, long>();
            StorageInfo info = XConvert.FromNames(string.Empty, string.Empty);
            foreach (var product in cacheSystem.ListDirectories(info))
            {
                if (!product.Name.Equals("data_archive", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                StorageInfo productInfo = XConvert.FromNames(Path.Combine(info.HighName, product.Name), string.Empty);

                foreach (var volume in cacheSystem.ListDirectories(productInfo))
                {
                    if (!volume.Name.Equals("IndexVolume", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    StorageInfo IndexVolumeInfo = XConvert.FromNames(Path.Combine(productInfo.HighName, volume.Name), string.Empty);
                    foreach (var farm in cacheSystem.ListDirectories(IndexVolumeInfo))
                    {
                        StorageInfo farmInfo = XConvert.FromNames(Path.Combine(IndexVolumeInfo.HighName, farm.Name), string.Empty);
                        foreach (var site in cacheSystem.ListDirectories(farmInfo))
                        {
                            StorageInfo siteInfo = XConvert.FromNames(Path.Combine(farmInfo.HighName, site.Name), string.Empty);

                            foreach (var file in cacheSystem.ListFiles(siteInfo))
                            {
                                if (file.Name.Contains(ServiceConstants.DBPropertiesName))
                                {
                                    StorageInfo propertyFileInfo = new StorageInfo();
                                    propertyFileInfo.Length = file.FileSize;
                                    propertyFileInfo.HighName = siteInfo.HighName;
                                    propertyFileInfo.LowName = file.Name.Remove(file.Name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                                    long lastAccess = 0;
                                    try
                                    {
                                        lastAccess = file.LastAccessTimeUtc.Ticks;
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"Get index file LastAccessTimeUtc failed {e}");
                                    }
                                    fileList[propertyFileInfo] = lastAccess;
                                }
                            }
                        }
                    }
                }
                foreach (var solutionFile in cacheSystem.ListFiles(productInfo))
                {
                    if (solutionFile.Name.Contains(ServiceConstants.DBPropertiesName))
                    {
                        StorageInfo propertyFileInfo = new StorageInfo();
                        propertyFileInfo.Length = solutionFile.FileSize;
                        propertyFileInfo.HighName = productInfo.HighName;
                        propertyFileInfo.LowName = solutionFile.Name.Remove(solutionFile.Name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                        fileList[propertyFileInfo] = 0;
                    }
                }
            }
            return fileList;
        }

        private static bool DoIndexRetentionByLastAccessTime(Dictionary<StorageInfo, long> lastAccessTimeDictionary, IXSystem cacheSystem, ulong totalFreeSpace, ulong minimumFreeSpace)
        {
            foreach (KeyValuePair<StorageInfo, long> pair in lastAccessTimeDictionary)
            {
                if (pair.Value < DateTime.UtcNow.AddDays(-1).Ticks)
                {
                    var deleteFileLength = DeleteCachedIndex(pair.Key.HighName, pair.Key.LowName, cacheSystem);
                    totalFreeSpace += deleteFileLength;
                }
            }
            return totalFreeSpace > minimumFreeSpace;
        }

        private static UInt64 DeleteCachedIndex(string indexVolume, string indexFileName, IXSystem cacheSystem)
        {
            StorageInfo info = XConvert.FromNames(indexVolume, indexFileName);
            string propertiesFileName = indexFileName + ServiceConstants.DBPropertiesName;
            UInt64 deleteFileSize = 0;
            try
            {
                bool fileExist = cacheSystem.FileExists(info);
                if (fileExist)
                {
                    var indexResult = cacheSystem.DeleteFile(info);
                    logger.Info(MediaCoreIndexResource.IndexCacheManagerDeleteCachedIndexResult, indexFileName, indexVolume);
                    deleteFileSize += (UInt64)indexResult.DeletedFileSize;
                }
                info.LowName = propertiesFileName;
                fileExist = cacheSystem.FileExists(info);
                if (fileExist)
                {
                    var propertyResult = cacheSystem.DeleteFile(info);
                    deleteFileSize += (UInt64)propertyResult.DeletedFileSize;
                }
            }
            catch (Exception e)
            {
                logger.Warn(MediaCoreIndexResource.IndexCacheRetentionManagerDeleteCachedIndexWarn, Path.Combine(indexVolume, indexFileName), e);
            }
            return deleteFileSize;
        }

        public static BlobContainerClient CustomGetBlobContainerClientByStorageXRI(string xri)
        {
            ConnectionBuilder xriObj = ConnectionBuilder.ValueOf(xri);
            string accessPoint = string.Empty;
            string containerName = string.Empty;
            string accountName = string.Empty;
            string accountKey = string.Empty;

            if (xriObj.Params.ContainsKey("accesspoint"))
            {
                accessPoint = xriObj.Params["accesspoint"];
            }
            if (xriObj.Params.ContainsKey("containername"))
            {
                containerName = xriObj.Params["containername"];
            }
            if (xriObj.Params.ContainsKey("name"))
            {
                accountName = xriObj.Params["name"];
            }
            if (xriObj.Params.ContainsKey("secret"))
            {
                accountKey = xriObj.Params["secret"];
            }
            logger.Info("Blob url: {0}, container name: {1}, account name: {2}", accessPoint, containerName, accountName);

            StringBuilder connString = new StringBuilder();
            var accessPointUri = new Uri(accessPoint);
            if (string.IsNullOrEmpty(accountKey))
            {
                connString.Append($"{accountName}.{accessPointUri.Authority}");
            }
            else
            {
                var blobPrefix = "blob.";
                var endpointSuffix = accessPoint.Substring(accessPoint.LastIndexOf(blobPrefix) + blobPrefix.Length);
                if (endpointSuffix.IndexOf('/') > 0)
                {
                    endpointSuffix = endpointSuffix.Split('/')[0];
                }

                connString.Append(BuildConnectionStringPart("DefaultEndpointsProtocol", accessPointUri.Scheme));
                connString.Append(BuildConnectionStringPart("AccountName", accountName));
                connString.Append(BuildConnectionStringPart("AccountKey", accountKey));
                connString.Append(BuildConnectionStringPart("EndpointSuffix", endpointSuffix, true));
            }
            return StorageUtil.GetContainerClient(connString.ToString(), containerName);
        }
        private static string BuildConnectionStringPart(string key, string value, bool isEnd = false)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return $"{key}={value}" + (isEnd ? "" : ";");
            }
            return string.Empty;
        }
    }
}
