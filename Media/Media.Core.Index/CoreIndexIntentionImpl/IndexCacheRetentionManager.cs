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



namespace AvePoint.Media.Core.Index
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaCoreIndex;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using DocumentFormat.OpenXml.Bibliography;
    using System.Diagnostics;
    using System.Reflection.PortableExecutable;

    #endregion using directives

    public class IndexCacheRetentionManager : IIndexCacheRetentionManager
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 在下载index前,保证cache有足够的空间，否则做retention
        /// </summary>
        /// <param name="indexVolume"></param>
        /// <param name="indexFileName"></param>
        /// <param name="storageSystem"></param>
        /// <param name="cacheSystem"></param>
        public void MakeSureCacheHaveEnoughSpaceBeforeDownload(IndexCacheManagerParameter param)
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
            Dictionary<StorageInfo, long> lastAccessTimeDictionary = null;
            bool findAvailableCache = default(bool);
            foreach (var subSystem in (param.CacheSystem as XLibrary).SubSystems)
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

        /// <summary>
        /// 在做backup前,保证cache有足够的空间，否则做retention
        /// </summary>
        /// <param name="indexVolume"></param>
        /// <param name="indexFileName"></param>
        /// <param name="storageSystem"></param>
        /// <param name="cacheSystem"></param>
        public void MakeSureCacheHaveEnoughSpaceBeforeBackup(IXSystem cacheSystem, CacheSettingDto cacheSetting)
        {
            ulong minimumFreeSpace = cacheSetting.LimitFreeSpace;
            Dictionary<StorageInfo, long> lastAccessTimeDictionary = null;
            bool findAvailableCache = default(bool);
            foreach (var subSystem in (cacheSystem as XLibrary).SubSystems)
            {
                if (!(subSystem is IXSpaceInfo spaceInfo))
                {
                    logger.Warn("The system does not support space related properties");
                    continue;
                }
                ulong totalFreeSpace = spaceInfo.TotalFreeSpace;
                try
                {
                    cacheSystem.FindCondition = new Predicate<IXSystem>(xSystem => { return spaceInfo.TotalFreeSpace > minimumFreeSpace; });
                    logger.Info(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadSucceed, cacheSystem.SystemLocation);
                    findAvailableCache = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (totalFreeSpace < minimumFreeSpace)
                    {
                        lastAccessTimeDictionary = GetLastAccessTimeDictionary(subSystem);
                    }
                    if (DoIndexRetentionByLastAccessTime(lastAccessTimeDictionary, cacheSystem, totalFreeSpace, minimumFreeSpace))
                    {
                        findAvailableCache = true;
                    }
                    else
                    {
                        logger.Warn(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadWarn, cacheSystem.SystemLocation, ex.ToString());
                    }
                }
            }
            if (!findAvailableCache)
                throw new SpaceNotEnoughException(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadNoEnoughSpaceException);
        }

        /// <summary>
        /// 在上传index后，保证cache有足够的空间，否则做retention
        /// </summary>
        /// <param name="cacheSystem"></param>
        /// <param name="fileInfo"></param>
        public void MakeSureCacheHaveEnoughSpaceAfterUpload(IXSystem cacheSystem, CacheSettingDto cacheSetting)
        {
            ulong minimumFreeSpace = cacheSetting.LimitFreeSpace;
            Dictionary<StorageInfo, long> lastAccessTimeDictionary = null;
            if (!(cacheSystem is IXSpaceInfo spaceInfo))
            {
                logger.Warn("The system does not support space related properties");
                return;
            }
            ulong totalFreeSpace = spaceInfo.TotalFreeSpace;
            if (totalFreeSpace < minimumFreeSpace)
            {
                lastAccessTimeDictionary = GetLastAccessTimeDictionary(cacheSystem);
                if (!DoIndexRetentionByLastAccessTime(lastAccessTimeDictionary, cacheSystem, totalFreeSpace, minimumFreeSpace))
                {
                    logger.Warn(MediaCoreIndexResource.IndexCacheManagerMakeSureCacheHaveEnoughSpaceBeforeDownloadWarn, cacheSystem.SystemLocation, string.Empty);
                }
            }
        }

        /// <summary>
        /// 删除最不常访问的index
        /// </summary>
        /// <param name="lastAccessTimeDictionary"></param>
        /// <param name="cacheSystem"></param>
        /// <returns></returns>
        private bool DoIndexRetentionByLastAccessTime(Dictionary<StorageInfo, long> lastAccessTimeDictionary, IXSystem cacheSystem, ulong totalFreeSpace, ulong minimumFreeSpace)
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

        /// <summary>
        /// 递归获得一个键为index文件的StorageInfo对象，值为上次访问时间的Dictionary
        /// </summary>
        /// <param name="cacheSystem"></param>
        /// <param name="info"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private Dictionary<StorageInfo, long> GetLastAccessTimeDictionary(IXSystem cacheSystem)
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

        /// <summary>
        /// 删除指定的cache中的index db文件,和property文件
        /// </summary>
        /// <param name="indexVolume"></param>
        /// <param name="indexFileName"></param>
        /// <param name="cacheSystem"></param>
        private UInt64 DeleteCachedIndex(string indexVolume, string indexFileName, IXSystem cacheSystem)
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
    }
}