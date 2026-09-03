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
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using GCommon.Utility;
    using Storage;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using AvePoint.Media.Storage.Util;
    using System.Linq;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.Common.Hybrid;
    using AvePoint.RA.Contract.Services;
    #endregion using directives

    public class IndexCacheManager : IIndexCacheManager
    {
        String indexStorageInfo;
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IIndexCacheRetentionManager indexCacheRetentionManager = new IndexCacheRetentionManager();

        #region 对外公开的方法

        /// <summary>
        /// 从XSystem下载cacheIndexVolume\indexName
        /// </summary>
        /// <param name="indexVolume"></param>
        /// <param name="indexFileName"></param>
        /// <param name="sys"></param>
        /// <returns>true表示真正到XSystem下载；否则，使用本地缓存</returns>
        public async Task<IndexDatabaseDownLoadResult> DownLoadIndexAsync(IndexCacheManagerParameter param, Boolean isFailedIndex = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IndexDatabaseStatus result = IndexDatabaseStatus.Nonexistent;
            //有些文件不是根据路径可以直接定位的, 需要传入extraStorageInfo
            StorageInfo cahceStorageInfo = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);
            String indexFileFullPath = PathUtil.CombinePath(PathUtil.CombinePath(param.StorageSystem.SystemLocation, param.IndexVolume), param.IndexName);
            String connectionString = String.Format("Data Source = {0}", indexFileFullPath);
            Guid connectionID = Guid.NewGuid();
            if (!isFailedIndex)
                indexStorageInfo = param.StorageInfo;
            if (ConnectionLockManager.GetConnectionLock(connectionString, connectionID, ConnectionLockType.Download))
            {
                try
                {
                    if (!isFailedIndex)
                    {
                        var (hasModifiedTimeByControl, lastModifyTimeByControl) = await QueryLastModifyTimeFromControlAsync(param.IndexVolume, param.IndexName);
                        if (ShouldDownloadIndex(param, hasModifiedTimeByControl, lastModifyTimeByControl))
                        {
                            if (!param.CacheSystem.FileExists(XConvert.FromNames(param.IndexVolume, param.IndexName)))
                            {
                                param.CacheSystem.DeleteFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                                //indexCacheRetentionManager.MakeSureCacheHaveEnoughSpaceBeforeDownload(param);
                                DownloadFromRealXSystem(param);
                                if (!hasModifiedTimeByControl)
                                {
                                    lastModifyTimeByControl = DateTime.UtcNow.Ticks;
                                    SaveIndexFileNameAndLastModifyTimeToControl(PathUtil.CombinePath(param.IndexVolume, param.IndexName), lastModifyTimeByControl);
                                }
                                //记录本地cache index的修改时间，用于判断是否需要上传
                                XFileInfo fileInfo = param.CacheSystem.OpenFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                                //SaveIndexFileNameAndLastModifyTimeToLocal(param.IndexVolume, param.IndexName, param.CacheSystem, lastModifyTimeByControl);
                                //SaveIndexFileNameAndLastModifyTimeToControl(PathUtil.CombinePath(param.IndexVolume, param.IndexName), fileInfo.LastWriteTimeUtc.Ticks);
                                
                            }
                            result = IndexDatabaseStatus.Downloaded;
                        }
                    }
                    else
                    {
                        param.CacheSystem.DeleteFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                        indexCacheRetentionManager.MakeSureCacheHaveEnoughSpaceBeforeDownload(param);
                        DownloadFromRealXSystem(param);
                        result = IndexDatabaseStatus.Downloaded;
                    }
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
            logger.Info($"Index Cache Manager DownLoad Index Result,indexInfo.IndexFullPath:{indexInfo.IndexFullPath.LogBase64()},result:{result.ToString()}");
            stopwatch.Stop();
            logger.Info($"Successful DownLoadIndexAsync.UseTime:{stopwatch.Elapsed}.");
            return indexInfo;
        }

        /// <summary>
        /// 将cacheIndexVolume\indexName上传到XSystem
        /// </summary>
        /// <param name="indexVolume"></param>
        /// <param name="indexFileName"></param>
        /// <param name="sys"></param>
        /// <returns>true，真正将数据上传上去；否则，仅在本地缓存</returns>
        public StorageResult UploadIndex(IndexCacheManagerParameter param, Boolean isFailedIndex = false)
        {
            StorageResult indexLoc = new StorageResult();
            string indexFileFullPath = PathUtil.CombinePath(param.IndexVolume, param.IndexName);
            Guid tempConnectionID = Guid.NewGuid();
            logger.Info($"Index Cache Manager Upload Index Begin,param.IndexVolume:{param.IndexVolume},param.IndexName:{param.IndexName},indexFileFullPath:{indexFileFullPath.LogBase64()}");
            if (ConnectionLockManager.GetConnectionLock(String.Format("Data Source = {0}", indexFileFullPath), tempConnectionID, ConnectionLockType.Upload))
            {
                try
                {
                    if (!isFailedIndex)
                    {
                        if (ShouldUploadIndex(param.IndexVolume, param.IndexName, param.CacheSystem))
                        {
                            indexLoc = UploadToRealXSystem(param, isFailedIndex);
                            XFileInfo fileInfo = param.CacheSystem.OpenFile(XConvert.FromNames(param.IndexVolume, param.IndexName));
                            logger.Info($"Index Cache Manager Upload Index FileInfo,fileInfo.LastWriteTimeUtc:{fileInfo.LastWriteTimeUtc}");
                            SaveIndexFileNameAndLastModifyTimeToLocal(param.IndexVolume, param.IndexName, param.CacheSystem, fileInfo.LastWriteTimeUtc.Ticks);
                            SaveIndexFileNameAndLastModifyTimeToControl(indexFileFullPath, fileInfo.LastWriteTimeUtc.Ticks);
                            indexCacheRetentionManager.MakeSureCacheHaveEnoughSpaceAfterUpload(param.CacheSystem, param.CacheSetting);
                        }
                    }
                    else
                    {
                        indexLoc = UploadToRealXSystem(param, isFailedIndex);
                        indexCacheRetentionManager.MakeSureCacheHaveEnoughSpaceAfterUpload(param.CacheSystem, param.CacheSetting);
                    }
                }
                finally
                {
                    ConnectionLockManager.RemoveConnectionLock(String.Format("Data Source = {0}", indexFileFullPath), tempConnectionID);
                }
            }
            return indexLoc;
        }

        #endregion 对外公开的方法

        #region 向control和local查询文件最后修改时间

        /// <summary>
        /// 从control查询lastmodifytime
        /// </summary>
        /// <param name="queryKey"></param>
        /// <returns>如果control上不存在queryKey，则返回false；否则返回true</returns>
        private async Task<(bool,long)> QueryLastModifyTimeFromControlAsync(string indexVolume, string indexFileName)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool isOK = false;
            long lastModifyTimeFromControl = long.MinValue;
            try
            {
                string key = ServiceConstants.ModifyTimeHeader + PathUtil.CombinePath(indexVolume, indexFileName);
                List<MediaDataDto> mediaDataDtoList = new List<MediaDataDto>();//= await MediaDao.GetMediaDatasAsync(key);
                if (mediaDataDtoList == null || mediaDataDtoList.Count == 0)
                {
                    //logger.Info(MediaCoreIndexResource.IndexCacheManagerQueryLastModifyTimeFromControlNotContainKey, key);
                    logger.Info($"Control does not contain the key {key.LogBase64()}");
                }
                else
                {
                    if (mediaDataDtoList.Count > 1)
                    {
                        //logger.Warn(MediaCoreIndexResource.IndexCacheManagerQueryLastModifyTimeFromControlWarn);
                        logger.Warn("Multiple modified time are queried");
                    }

                    lastModifyTimeFromControl = Convert.ToInt64(mediaDataDtoList.OrderByDescending(m => m.Value).First().Value);
                    isOK = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"IndexCache Manager QueryLastModifyTime From Control Error:{ex.ToString()}");
            }
            stopwatch.Stop();
            logger.Info($"QueryLastModifyTimeFromControlAsync finish.UseTime:{stopwatch.Elapsed}.");
            return (isOK, lastModifyTimeFromControl);
        }

        /// <summary>
        /// 从cache中的property文件中查询lastmodifytime
        /// </summary>
        /// <param name="queryKey"></param>
        /// <param name="lastModifyTimeByLocalCache"></param>
        /// <returns>如果cache上不存在index.db和其对应的property文件，则返回false；否则返回true</returns>
        private bool QueryLastModifyTimeFromCache(string indexVolume, string indexFileName, IXSystem cacheSystem, out long lastModifyTimeByLocalCache)
        {
            string propertyFile = indexFileName + ServiceConstants.DBPropertiesName;
            StorageInfo propertyFileInfo = XConvert.FromNames(indexVolume, propertyFile);
            StorageInfo dbInfo = XConvert.FromNames(indexVolume, indexFileName);
            bool propertyFileExist = cacheSystem.FileExists(propertyFileInfo);
            bool dbExist = cacheSystem.FileExists(dbInfo);

            if (propertyFileExist && dbExist)
            {
                Dictionary<IndexDatabaseProperties, string> dbProperties = IndexDatabasePropertiesManager.ParseDBProperties(indexVolume, propertyFile, cacheSystem);
                if (dbProperties.ContainsKey(IndexDatabaseProperties.LastModifyTime))
                {
                    if (long.TryParse(dbProperties[IndexDatabaseProperties.LastModifyTime], out lastModifyTimeByLocalCache))
                    {
                        return true;
                    }
                }
            }
            lastModifyTimeByLocalCache = long.MinValue;
            return false;
        }

        #endregion 向control和local查询文件最后修改时间

        #region 向control和local cache存储db最后修改时间

        /// <summary>
        /// 将lastmodifytime和indexVolume发给control
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        private void SaveIndexFileNameAndLastModifyTimeToControl(string filePath, long time)
        {
            try
            {
                string key = ServiceConstants.ModifyTimeHeader + filePath;
                key = key.Replace("\\", "/");
                logger.Info($"UpdateOrInsertMediaData key:{key.LogBase64()},time:{time.ToString()}");
                HybridApiClient.Instance.UpdateOrInsertMediaData(new KeyValuePair<string,string>(key, time.ToString()));
            }
            catch (Exception ex)
            {
                logger.Error($"IndexCacheManager Save IndexFileName And LastModifyTime To Control Error:{ex.ToString()}");
            }
        }

        /// <summary>
        /// 将cache db最后修改时间存到本地db cache文件中
        /// </summary>
        private void SaveIndexFileNameAndLastModifyTimeToLocal(string indexVolume, string indexFileName, IXSystem cacheSystem, long time)
        {
            string propertiesFile = indexFileName + ServiceConstants.DBPropertiesName;
            Dictionary<IndexDatabaseProperties, string> keyValues = new Dictionary<IndexDatabaseProperties, string>();
            keyValues[IndexDatabaseProperties.LastModifyTime] = time.ToString();
            IndexDatabasePropertiesManager.SaveDBProperties(indexVolume, propertiesFile, cacheSystem, keyValues);
        }

        #endregion 向control和local cache存储db最后修改时间

        #region 向真正的存储介质上传、下载数据

        private StorageResult UploadToRealXSystem(IndexCacheManagerParameter param, Boolean isFailedIndex)
        {
            try
            {
                logger.Info($"IndexCacheManager Upload To Real XSystem Begin,param.IndexName:{param.IndexName},param.IndexVolume:{param.IndexVolume}");
                byte[] cacheBuffer = new byte[1024 * 64];
                StorageInfo info = XConvert.FromNames(param.IndexVolume, param.IndexName);
                info.NeedRenameIndexName = param.NeedRenameIndexName;
                StorageResult storageResult = null;
                info.Length = param.CacheSystem.OpenFile(info).FileSize;
                logger.Info($"IndexCacheManager Upload To Real XSystem Open:{info.ToString()}");

                var encryptedFile = EncryptFile(param.CacheSystem, info, param.DataMode, param.EncryptionInfo);
                using (XStream cacheStream = param.CacheSystem.OpenStream(encryptedFile, FileMode.Open))
                {
                    info.Length = encryptedFile.Length;
                    info.IsClosing = true;
                    logger.Info("indexStorageInfo  " + param.StorageInfo + "    " + indexStorageInfo);
                    if (isFailedIndex)
                        info.ExtraStorageInfo = param.StorageInfo;
                    else
                    {
                        if (param.StorageInfo == null)
                        {
                            info.ExtraStorageInfo = indexStorageInfo;
                        }
                        else
                        {
                            info.ExtraStorageInfo = null;
                        }
                    }
                    //临时修改，只适用于azure，physical device 不check 剩余空间
                    var workSystem = param.StorageSystem;
                    if (param.StorageSystem.FileExists(info))
                    {
                        workSystem = ((XLibrary)param.StorageSystem).GetWorkingSystem();
                    }
                    if (workSystem.Type == ServiceConstants.AzureSystem)
                    {
                        logger.Info("this index device is azure device,set index to hot tier");
                        //Storage.Cloud.Azure.AzureCloudInfo azureInfo = ConvertStorageInfoToAzureCloudInfo(info);
                        storageResult = workSystem.CommitStream(cacheStream, info);
                    }
                    //else if(workSystem.Type == XStorageType.GoogleCloud)
                    //{
                    //    Storage.Cloud.Google.GoogleCloudInfo googleInfo = ConvertStorageInfoToGoogleCloudInfo(info);
                    //    storageResult = workSystem.CommitStream(cacheStream, googleInfo);
                    //}
                    else
                    {
                        logger.Info($"this index device is not azure device,set index to hot tier ,device type: {workSystem?.Type}");
                        storageResult = workSystem.CommitStream(cacheStream, info);
                    }
                }
                logger.Info($"IndexCacheManager Upload To Real XSystem End,param.IndexName:{param.IndexName},param.IndexVolume:{param.IndexVolume}");
                return storageResult;
            }
            catch (Exception ex)
            {
                logger.Error($"IndexCacheManager Upload To Real XSystem Error:{ex}");
                throw;
            }
        }
        //private Storage.Cloud.Azure.AzureCloudInfo ConvertStorageInfoToAzureCloudInfo(StorageInfo storageInfo)
        //{
        //    Storage.Cloud.Azure.AzureCloudInfo azureInfo = new Storage.Cloud.Azure.AzureCloudInfo();
        //    azureInfo.HighName = storageInfo.HighName;
        //    azureInfo.LowName = storageInfo.LowName;
        //    azureInfo.Length = storageInfo.Length;
        //    azureInfo.FileTierType = AccessTierType.Hot;
        //    return azureInfo;
        //}
        //private Storage.Cloud.Google.GoogleCloudInfo ConvertStorageInfoToGoogleCloudInfo(StorageInfo storageInfo)
        //{
        //    Storage.Cloud.Google.GoogleCloudInfo googleInfo = new Storage.Cloud.Google.GoogleCloudInfo();
        //    googleInfo.HighName = storageInfo.HighName;
        //    googleInfo.LowName = storageInfo.LowName;
        //    googleInfo.Length = storageInfo.Length;
        //    googleInfo.StorageClass = Storage.Cloud.Google.GoogleStorageClass.Standard;
        //    return googleInfo;
        //}
        private StorageInfo EncryptFile(IXSystem cacheSystem, StorageInfo sourceInfo, byte dataMode, GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo encryptionInfo)
        {
            try
            {
                if (!NeedEncrypt(dataMode)) { return sourceInfo; }
                logger.Info($"Start to encrypt index file: {sourceInfo}");
                var targetInfo = sourceInfo.Clone();
                targetInfo.LowName = $"{targetInfo.LowName}.encrypted";
                if (encryptionInfo != null)
                {
                    new IndexEncryptionManager(cacheSystem).EncryptFile(sourceInfo, targetInfo, encryptionInfo);
                    targetInfo.Length = cacheSystem.OpenFile(targetInfo).FileSize;
                    logger.Info($"Finish encrypt index file: {targetInfo} ");
                    return targetInfo;
                }
                else
                {
                    logger.Info("Index file is not encrypted as encryptionInfo is null");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to encrypted index file, error: {0}", ex);
            }
            return sourceInfo;
        }

        private bool NeedEncrypt(byte dataMode)
        {
            return (dataMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED;
        }

        private void DownloadFromRealXSystem(IndexCacheManagerParameter param)
        {
            //logger.Info(MediaCoreIndexResource.IndexCacheManagerDownloadFromRealXSystemBegin, param.IndexName, param.IndexVolume);
            //FIXED
            logger.Info($"Start downloading index database [{param.IndexName}] in [{param.IndexVolume}]");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var cacheBuffer = new Byte[1024 * 64];
            var info = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);

            info.Length = param.StorageSystem.OpenFile(info).FileSize;//针对cloud介质
            logger.Info($"Begin open stream for download index, the info is {info.ToString().LogBase64()}, file size {info.Length}");
            var storageSystem = param.StorageSystem as AbstractXSystem;
            if (storageSystem != null && storageSystem.Type == ServiceConstants.AzureSystem)
            {
                if (string.IsNullOrEmpty(storageSystem.XriString))
                {
                    var storageLibrary = storageSystem as XLibrary;
                    if(storageLibrary == null)
                    {
                        logger.Error($"Index storage system not a XLibrary.");
                    }
                    storageSystem = storageLibrary?.SubSystems?.FirstOrDefault() as AbstractXSystem;
                    if(storageSystem == null)
                    {
                        logger.Error($"Index storage system's connection string is null.");
                    }
                }
                
                var containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(storageSystem?.XriString);
                var cachelocation= param.CacheSystem.SystemLocation;
                AzureUtil.DownloadBlobToAsync(containerClient, info.HighPlusLowName, PathUtil.CombinePath(cachelocation, info.HighPlusLowName)).GetAwaiter().GetResult();
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

        #endregion 向真正的存储介质上传、下载数据

        #region 判断是否需要上传、下载db

        /// <summary>
        /// 根据先前记录的本地index file的lastwritetime，判断index file是否需要上传。
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <returns></returns>
        private Boolean ShouldUploadIndex(String indexVolume, String indexFileName, IXSystem cacheSystem)
        {
            Boolean shouldUpload = true;
            String indexRelatedFilePath = PathUtil.CombinePath(indexVolume, indexFileName);
            StorageInfo info = XConvert.FromNames(indexVolume, indexFileName);
            XFileInfo fileInfo = cacheSystem.OpenFile(info);
            Int64 cachedLastWriteTime;
            //判断最新修改时间与download时缓存的修改时间是否一致，如果不一致，则需要上传index文件
            if (QueryLastModifyTimeFromCache(indexVolume, indexFileName, cacheSystem, out cachedLastWriteTime))
            {
                if (fileInfo.LastWriteTimeUtc.Ticks <= cachedLastWriteTime)
                {
                    shouldUpload = false;
                }
            }
            return shouldUpload;
        }

        private bool ShouldDownloadIndex(IndexCacheManagerParameter param, bool existsModifiedTimeByControl, long lastModifiedTimeByControl)
        {
            bool shouldDownLoadIndex = false;
            var IscacheHasLastModifiedTime = QueryLastModifyTimeFromCache(param.IndexVolume, param.IndexName, param.CacheSystem, out var lastModifyTimeByLocalCache);
            var indexInfo = XConvert.FromNames(param.IndexVolume, param.IndexName, param.StorageInfo);

            if ((param.NeedDownLoad || !param.CacheSystem.FileExists(indexInfo)) && param.StorageSystem.FileExists(indexInfo))
            {
                shouldDownLoadIndex = true;
            }
            else if (existsModifiedTimeByControl)
            {
                if (IscacheHasLastModifiedTime)
                {
                    if (lastModifiedTimeByControl != lastModifyTimeByLocalCache)
                    {
                        shouldDownLoadIndex = true;
                    }
                }
                else
                {
                    shouldDownLoadIndex = true;
                }
            }
            else if (param.StorageSystem.FileExists(indexInfo) && !existsModifiedTimeByControl)
            {
                shouldDownLoadIndex = true;
            }

            return shouldDownLoadIndex;
        }

        #endregion 判断是否需要上传、下载db
    }
}