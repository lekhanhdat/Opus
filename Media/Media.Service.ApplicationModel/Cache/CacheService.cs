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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.IO;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using global::Media.Common.ClassicStorageApi;
    using Util;
    #endregion

    public class CacheService : ICacheService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXConverter converter;
        CacheSettingDto cacheSetting;
        String lastMetaDataFileName;
        String lastMetaDataFilePath;
        String lastContentDataFileName;
        String lastContentDataFilePath;

        public IXSystem CacheSystem { get; set; }
        public IIndexCacheRetentionManager IndexCacheRetentionManager { get; set; }

        public void Open(CacheSettingDto cacheSetting, Boolean isDirectSystem, Boolean isBackup = default(Boolean))
        {
            this.cacheSetting = cacheSetting;
            if (!isDirectSystem
                || MediaConfigInfo.CommonConfigInfo.ForceUseCache
                || MediaConfigInfo.CommonConfigInfo.ReadMetaDataViaCache
                || MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache)
            {
                this.CacheSystem = XFactoryCommon.InstanceLibrary(cacheSetting.ConvertToMediaLogicalDeviceDto().ToXRIS());
                this.CacheSystem.Open();
                if (isBackup)
                {
                    IndexCacheRetentionManager.MakeSureCacheHaveEnoughSpaceBeforeBackup(this.CacheSystem, cacheSetting);
                }
                this.logger.Info(MediaServiceApplicationModelResource.CacheServiceOpenSucceed);
            }
        }

        /// <summary>
        /// 将用于还原的数据块下载到cache，以解决磁带中交叉读取数据块导致的效率损失
        /// </summary>
        /// <param name="fileType">meta or content</param>
        /// <param name="highName">file path</param>
        /// <param name="lowName">file name</param>
        public void DownloadDataFromDevice(IXConverter converter, IXSystem logicalDevice, FileType fileType, string highName, string lowName)
        {
            logger.Info(MediaServiceApplicationModelResource.CacheServiceDownloadDataFromDeviceBegin, lowName, highName);
            this.converter = converter;
            byte[] cacheBuffer = new byte[1024 * 64];
            StorageInfo deviceDataInfo = this.converter.FormNames(fileType, highName, lowName);
            deviceDataInfo.Offset = 0;
            deviceDataInfo.Length = 0;
            using (XStream downloader = logicalDevice.OpenStream(deviceDataInfo, FileMode.Open))
            {
                using (XStream cacheStream = this.CacheSystem.OpenStream(deviceDataInfo, FileMode.CreateNew))
                {
                    int readLen = 0;
                    //downloader.BeginRead(downloader.Info);
                    while ((readLen = downloader.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                    {
                        cacheStream.Write(cacheBuffer, 0, readLen);
                    }
                    //downloader.EndRead();
                    cacheStream.Flush();
                }
            }
            DeleteLastDataFile(fileType);
            RecordCurrentFilePathAndName(fileType, highName, lowName);
        }

        public void Clear(String dataVolume, String jobId, Int32 preFixNum)
        {
            if (this.CacheSystem != null)
            {
                StorageInfo cacheInfo = XConvert.FromNames(dataVolume, jobId + "_" + preFixNum + "_" + ServiceConstants.ContentDataCacheName);
                if (this.CacheSystem.FileExists(cacheInfo))
                {
                    this.CacheSystem.DeleteFile(cacheInfo);
                }
                cacheInfo = XConvert.FromNames(dataVolume, jobId + "_" + preFixNum + "_" + ServiceConstants.MetaDataCacheName);
                if (this.CacheSystem.FileExists(cacheInfo))
                {
                    this.CacheSystem.DeleteFile(cacheInfo);
                }
                this.logger.Info(MediaServiceApplicationModelResource.CacheServiceClearSucceed);
            }
        }

        public void Close()
        {
            if (this.CacheSystem != null)
            {
                //if (MediaConfigInfo.CommonConfigInfo.ReadMetaDataViaCache
                //    && !this.lastMetaDataFilePath.IsNullOrEmpty()
                //    && !this.lastMetaDataFileName.IsNullOrEmpty())
                //{
                //    this.CacheSystem.DeleteFile(this.converter.FormNames(FileType.MetaData, this.lastMetaDataFilePath, this.lastMetaDataFileName));
                //}
                //if (MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache
                //    && !this.lastContentDataFilePath.IsNullOrEmpty()
                //    && !this.lastContentDataFileName.IsNullOrEmpty())
                //{
                //    this.CacheSystem.DeleteFile(this.converter.FormNames(FileType.Content, this.lastContentDataFilePath, this.lastContentDataFileName));
                //}
                this.CacheSystem.Close();
                //this.logger.Info(MediaServiceApplicationModelResource.CacheServiceCloseSucceed);
            }
        }

        void DeleteLastDataFile(FileType fileType)
        {
            if (fileType == FileType.MetaData)
            {
                if (!this.lastMetaDataFilePath.IsNullOrEmpty() && !this.lastMetaDataFileName.IsNullOrEmpty())
                {
                    this.CacheSystem.DeleteFile(this.converter.FormNames(fileType, lastMetaDataFilePath, lastMetaDataFileName));
                }
            }
            else if (fileType == FileType.Content)
            {
                if (!this.lastContentDataFilePath.IsNullOrEmpty() && !this.lastContentDataFileName.IsNullOrEmpty())
                {
                    this.CacheSystem.DeleteFile(this.converter.FormNames(fileType, lastContentDataFilePath, lastContentDataFileName));
                }
            }
        }

        void RecordCurrentFilePathAndName(FileType fileType, String highName, String lowName)
        {
            if (fileType == FileType.MetaData)
            {
                this.lastMetaDataFilePath = highName;
                this.lastMetaDataFileName = lowName;
            }
            else if (fileType == FileType.Content)
            {
                this.lastContentDataFilePath = highName;
                this.lastContentDataFileName = lowName;
            }
        }
    }
}