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
    using AvePoint.Media.StorageApi;
    using AvePoint.RA.Common;
    using System.Diagnostics;

    #endregion using directives

    public class IndexDatabaseSynchronizer : IIndexDatabaseSynchronizer
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String indexVolume;
        IXSystem cacheSystem;
        IXSystem storageSystem;
        CacheSettingDto cacheSetting;

        public IIndexCacheManager IndexCacheManager => PlatformWindsorManager.GetService<IIndexCacheManager>();


        public IndexDatabaseDownLoadResult Download(IndexDatabaseInfo dbInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IndexDatabaseDownLoadResult indexDownLoadInfo = default(IndexDatabaseDownLoadResult);
            if (storageSystem.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                StorageInfo logicalStorageInfo = XConvert.FromNames(indexVolume, dbInfo.DbFileName, dbInfo.StorageInfo);
                logger.Info(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultIndexInfo, indexVolume, dbInfo.DbFileName, storageSystem.SystemLocation);
                if (storageSystem.FileExists(logicalStorageInfo))
                {
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, PathUtil.CombinePath(storageSystem.SystemLocation, PathUtil.CombinePath(indexVolume, dbInfo.DbFileName)));
                }
                else
                {
                    if (dbInfo.IsNeedCreateNewIndex)
                        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, PathUtil.CombinePath(storageSystem.SystemLocation, PathUtil.CombinePath(indexVolume, dbInfo.DbFileName)));
                    else
                        throw new FileNotFoundException(String.Format(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultFileNotFoundException, PathUtil.CombinePath(storageSystem.SystemLocation, PathUtil.CombinePath(indexVolume, dbInfo.DbFileName))));
                }
                storageSystem.OpenDirectory(XConvert.FromNames(indexVolume, string.Empty), FileMode.OpenOrCreate);
            }
            else
            {
                logger.Info(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultBegin, dbInfo.DbFileName);
                var param = new IndexCacheManagerParameter()
                {
                    StorageInfo = dbInfo.StorageInfo,
                    IndexName = dbInfo.DbFileName,
                    IndexVolume = this.indexVolume,
                    CacheSetting = this.cacheSetting,
                    CacheSystem = this.cacheSystem,
                    StorageSystem = this.storageSystem,
                    NeedDownLoad = dbInfo.NeedDownLoad,
                    DataMode = dbInfo.DataMode,
                    EncryptionInfo = dbInfo.EncryptionInfo,
                };
                indexDownLoadInfo =  IndexCacheManager.DownLoadIndexAsync(param).Result;
                var lastAccessTime = new Dictionary<IndexDatabaseProperties, String>();
                lastAccessTime[IndexDatabaseProperties.LastAccessTime] = DateTime.UtcNow.Ticks.ToString();
                IndexDatabasePropertiesManager.SaveDBProperties(indexVolume, dbInfo.DbFileName + ".properties", cacheSystem, lastAccessTime);
            }
            stopwatch.Stop();
            logger.Info($"IndexDatabaseSynchronizer Download finish.UseTime:{stopwatch.Elapsed}.");
            return indexDownLoadInfo;
        }

        public void DeleteFile(IndexDatabaseInfo dbInfo)
        {
            var param = new IndexCacheManagerParameter()
            {
                StorageInfo = dbInfo.StorageInfo,
                IndexName = dbInfo.DbFileName,
                IndexVolume = this.indexVolume,
                CacheSetting = this.cacheSetting,
                CacheSystem = this.cacheSystem,
                StorageSystem = this.storageSystem,
                NeedDownLoad = dbInfo.NeedDownLoad,
                DataMode = dbInfo.DataMode,
                EncryptionInfo = dbInfo.EncryptionInfo,
            };
            param.CacheSystem.DeleteFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
        }

        public IndexDatabaseDownLoadResult DownloadOtherIndex(IndexDatabaseInfo dbInfo)
        {
            IndexDatabaseDownLoadResult indexDownLoadInfo = default(IndexDatabaseDownLoadResult);
            logger.Info(MediaCoreIndexResource.IndexDatabaseSynchronizerIndexDatabaseDownLoadResultBegin, Path.Combine(dbInfo.IndexVolume, dbInfo.DbFileName));
            var param = new IndexCacheManagerParameter()
            {
                StorageInfo = dbInfo.StorageInfo,
                IndexName = dbInfo.DbFileName,
                IndexVolume = dbInfo.IndexVolume,
                CacheSetting = this.cacheSetting,
                CacheSystem = this.cacheSystem,
                StorageSystem = this.storageSystem,
                NeedDownLoad = dbInfo.NeedDownLoad,
                DataMode = dbInfo.DataMode,
                EncryptionInfo = dbInfo.EncryptionInfo
            };
            indexDownLoadInfo = IndexCacheManager.DownLoadIndexAsync(param, true).Result;
            return indexDownLoadInfo;
        }

        public IndexDatabaseUpLoadResult Upload(IndexDatabaseInfo dbInfo)
        {
            IndexDatabaseUpLoadResult uploadResult = new IndexDatabaseUpLoadResult();
            if (storageSystem != null)
            {
                if (!storageSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    logger.Info(MediaCoreIndexResource.IndexDataBaseSyncListenerUploadIndexBegin, dbInfo.DbFileName, storageSystem.SystemLocation, indexVolume);
                    var param = new IndexCacheManagerParameter()
                    {
                        IndexName = dbInfo.DbFileName,
                        IndexVolume = this.indexVolume,
                        CacheSetting = this.cacheSetting,
                        CacheSystem = this.cacheSystem,
                        StorageSystem = this.storageSystem,
                        NeedRenameIndexName = dbInfo.NeedRenameIndexName,
                        DataMode = dbInfo.DataMode,
                        EncryptionInfo = dbInfo.EncryptionInfo,
                    };
                    if (dbInfo.StorageInfo != null)
                    {
                        param.StorageInfo = dbInfo.StorageInfo;
                    }
                    var storageResult = IndexCacheManager.UploadIndex(param);
                    uploadResult.StorageInfo = param.StorageInfo;
                }
            }
            return uploadResult;
        }

        public IndexDatabaseUpLoadResult Upload(IndexDatabaseInfo dbInfo, bool isFailedIndex)
        {
            IndexDatabaseUpLoadResult uploadResult = new IndexDatabaseUpLoadResult();
            if (storageSystem != null)
            {
                if (!storageSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    logger.Info(MediaCoreIndexResource.IndexDataBaseSyncListenerUploadIndexBegin, dbInfo.DbFileName, storageSystem.SystemLocation, indexVolume);
                    var param = new IndexCacheManagerParameter()
                    {
                        IndexName = dbInfo.DbFileName,
                        IndexVolume = this.indexVolume,
                        CacheSetting = this.cacheSetting,
                        CacheSystem = this.cacheSystem,
                        StorageSystem = this.storageSystem,
                        NeedRenameIndexName = dbInfo.NeedRenameIndexName,
                        DataMode = dbInfo.DataMode,
                        EncryptionInfo = dbInfo.EncryptionInfo,
                    };
                    if (dbInfo.StorageInfo != null)
                    {
                        param.StorageInfo = dbInfo.StorageInfo;
                    }
                    StorageResult storageResult = null;
                    if (isFailedIndex)
                    {
                        storageResult = IndexCacheManager.UploadIndex(param, true);
                        uploadResult.IsCommit = storageResult.IsCommited;
                    }
                    else
                    {
                        storageResult = IndexCacheManager.UploadIndex(param);
                    }
                    uploadResult.StorageInfo = param.StorageInfo;
                }
            }
            return uploadResult;
        }

        public IndexDatabaseUpLoadResult UploadOtherIndex(IndexDatabaseInfo dbInfo)
        {
            IndexDatabaseUpLoadResult uploadResult = new IndexDatabaseUpLoadResult();
            if (storageSystem != null)
            {
                if (!storageSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    logger.Info(MediaCoreIndexResource.IndexDataBaseSyncListenerUploadIndexBegin, dbInfo.DbFileName, storageSystem.SystemLocation, dbInfo.IndexVolume);
                    var param = new IndexCacheManagerParameter()
                    {
                        IndexName = dbInfo.DbFileName,
                        IndexVolume = dbInfo.IndexVolume,
                        CacheSetting = this.cacheSetting,
                        CacheSystem = this.cacheSystem,
                        StorageSystem = this.storageSystem,
                        NeedRenameIndexName = dbInfo.NeedRenameIndexName,
                        DataMode = dbInfo.DataMode,
                        EncryptionInfo = dbInfo.EncryptionInfo
                    };
                    if (dbInfo.StorageInfo != null)
                    {
                        param.StorageInfo = dbInfo.StorageInfo;
                    }
                    var storageResult = IndexCacheManager.UploadIndex(param,true);
                    uploadResult.StorageInfo = param.StorageInfo;
                    uploadResult.IsCommit = storageResult.IsCommited;
                }
            }
            return uploadResult;
        }

        public void Initialize(IndexServiceOpenParameter param)
        {
            this.cacheSystem = param.IndexCacheDeviceSystem;
            this.storageSystem = param.IndexLogicalDeviceSystem;
            this.indexVolume = param.IndexVolume;
            this.cacheSetting = param.CacheSetting;
            this.cacheSystem.Open();
        }

        public StorageCopyResult Copy(IndexDatabaseInfo dbInfo)
        {
            var indexInfo = new StorageInfo(dbInfo.IndexVolume, dbInfo.DbFileName);
            var sourceIndexInfo = XConvert.FromNames(dbInfo.SourceIndexVolume, dbInfo.DbFileName, dbInfo.StorageInfo);
            sourceIndexInfo.Length = dbInfo.SourceIndexLogicalDevice.OpenFile(sourceIndexInfo).FileSize;
            indexInfo.Length = sourceIndexInfo.Length;
            return dbInfo.SourceIndexLogicalDevice.CopyFile(sourceIndexInfo, dbInfo.DestinationIndexLogicalDevice, indexInfo, true);
        }
    }
}