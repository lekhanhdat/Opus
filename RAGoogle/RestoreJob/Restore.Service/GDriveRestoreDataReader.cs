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




namespace RAGoogle.Restore.Service
{
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Core.IO.Input;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.DB.Dao;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using global::Media.Service.ArchiverBackup.Restore;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    #endregion using directives

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]
    public class GDriveRestoreDataReader
        : DataReaderBase<GDriveRestoreJob>
        , IInputDataListener
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXConverter converter;
        IXSystem LogicalDevice;
        GDriveRestoreJob archiverRestoreJob;
        IMediaGeneralInputStream GeneralInput;
        Dictionary<string, long> blockSizeMap = new Dictionary<string, long>();
        public ICacheService CacheManager => PlatformWindsorManager.GetService<ICacheService>();
        public override IMediaGeneralInputStream Input { get { return GeneralInput; } }
        string SoftDeleteJobIds = string.Empty;
        public IVolumeGeneratorFactory VolumeGeneratorFactory { get; set; }

        public IFileNameGeneratorFactory FileNameGeneratorFactory { get { return new FileNameGeneratorFactory(); } }

        private SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings;
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceManager StorageDeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        IXSystem SoftDeleteDevice;
        BlobContainerClient sourceContainerClient;
        public override void Open(GDriveRestoreJob restoreJob)
        {
            this.archiverRestoreJob = restoreJob;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderOpenBegin);
            this.LogicalDevice = XFactoryCommon.InstanceLibrary(restoreJob.LogicalDevice.ToXRIS());
            this.LogicalDevice.Open();
            // 如果需要将meta或者content data下载到cache以便减少交叉读取数据块带来的效率损失的话，则open cache.
            this.CacheManager.Open(this.archiverRestoreJob.CacheSetting, LogicalDevice.IsDirectSystem, false);

            OpenInputStreamParameter openParam = new OpenInputStreamParameter();
            openParam.DataListener = this;
            openParam.IsSupportAutoChangeDataBlock = (this.LogicalDevice as AvePoint.Media.ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true;
            this.GeneralInput = InputStreamFactory.GetInputStream(openParam, out this.converter);
            this.GeneralInput.Open();
        }

        public override void Dispose()
        {
            this.blockSizeMap.Clear();
            this.GeneralInput.Close();
            this.LogicalDevice.Close();
            this.SoftDeleteDevice?.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderCloseEnd);
        }

        #region InputDataListener Methods

        public void CloseDataBlock(FileType fileType, string fileName, System.IO.Stream stream)
        {
            stream.Close();
        }

        public void SettingMappings(SafeDictionary<string, BLOBRehydrationMapping> blobMappings)
        {
            BLOBMappings = blobMappings;
        }

        public XStream OpenDataBlock(DataBlockOpenParam param, out DataBlockOpenOutParam outParam)
        {
            IXSystem dataDevice;
            this.logger.Debug(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderOpenDataBlockBeginDebug);
            outParam = new DataBlockOpenOutParam();
            String dataVolume = this.archiverRestoreJob.DataVolume;

            var fileNameGenerator = this.FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, param.DataVersion);
            outParam.FileName = fileNameGenerator.Generate(new FileNameParameter(param));
            outParam.FileSize = this.DataBlockGetSize(param.FileType, outParam.FileName, dataVolume);
            //var filePath = Path.Combine(archiverRestoreJob.DataVolume, param.JobId);
            if (param.OpenFromCache)
            {
                dataDevice = this.CacheManager.CacheSystem;
                int retryTime = 0;
                if (param.ShouldDownloadData)
                {
                    while (true)
                    {
                        try
                        {
                            retryTime++;
                            this.CacheManager.DownloadDataFromDevice(this.converter, this.LogicalDevice, param.FileType, archiverRestoreJob.DataVolume, outParam.FileName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryTime > 5)
                            {
                                throw;
                            }
                            this.logger.Warn($"Archiver OpenDataBlock OpenFromCache DownloadDataFromDevice failed and need retry,RetryTime:{retryTime}.Message:{ex}.");
                            this.CacheManager.CacheSystem.DeleteFile(this.converter.FormNames(param.FileType, archiverRestoreJob.DataVolume, outParam.FileName));
                        }
                    }
                }
            }
            else
            {
                dataDevice = this.LogicalDevice;
            }
            this.converter.SetFileSize(param.FileType, outParam.FileSize, (this.LogicalDevice as AvePoint.Media.ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderDataBlockOpeningBegin, Path.Combine(dataVolume, outParam.FileName), outParam.FileSize);
            StorageInfo info = this.converter.FormNames(param.FileType, dataVolume, outParam.FileName);

            if (BLOBMappings != null && BLOBMappings.ContainsKey(Path.Combine(info.HighName, info.LowName)))
            {
                var mapped = BLOBMappings[Path.Combine(info.HighName, info.LowName)].MappedBlobInfo;
                logger.Info($"Mapped from [{info.HighPlusLowName}] to [{mapped.HighPlusLowName}].");
                info.HighName = mapped.HighName;
                info.LowName = mapped.LowName;
            }
            return dataDevice.OpenStream(info, FileMode.Open);
        }
        private void UnDeleteDataBlock(FileType fileType, string fileName, string dataVolume)
        {
            try
            {
                var info = this.converter.FormNames(fileType, dataVolume, fileName);
                var temp = SoftDeleteJobIds.StartsWith(dataVolume);
                if (temp)
                {
                    var blobClient = sourceContainerClient.GetBlobClient(info.HighPlusLowName);
                    blobClient.Undelete();
                    SetBlockStatusToCurrentVersion(blobClient, info.HighPlusLowName);
                }
                else
                {
                    var subsubJobId = GetSubsubJobIdFromFileName(fileName);
                    var subInfo = ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(subsubJobId).GetAwaiter().GetResult();
                    if (!string.Equals(subInfo.CurrentStorageId, RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                    {
                        if (SoftDeleteDevice != null)
                        {
                            SoftDeleteDevice.Close();
                        }
                        logger.Info($"this subinfo is soft delete,id:{subInfo.SubSubJobId},storage id:{subInfo.CurrentStorageId}");
                        var srcStorageDevice = StorageDeviceService.GetStorageDeviceById(subInfo.CurrentStorageId);
                        var srcLogical = ConvertStorageDeviceDtoToLogicalDeviceDto(srcStorageDevice);
                        SoftDeleteDevice = this.StorageDeviceManager.Open(srcLogical.GetXRIS(PhysicalDeviceUsage.All));
                        var source = SoftDeleteDevice as AbstractXSystem;
                        if (source != null && source.StorageType == XStorageType.Azure)
                        {
                            source = ValidStorage(source);
                            sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                            var blobClient = sourceContainerClient.GetBlobClient(info.HighPlusLowName);
                            blobClient.Undelete();
                            SetBlockStatusToCurrentVersion(blobClient, info.HighPlusLowName);
                            SoftDeleteJobIds = subInfo.SubSubJobId;
                        }
                        else
                        {
                            throw new FileNotFoundException(String.Format("3An error occurred in getting file {0} size in {1}.", fileName, dataVolume));
                        }
                    }
                    else
                    {
                        logger.Info($"this subinfo is not soft delete,id:{subInfo.SubSubJobId}");
                        throw new FileNotFoundException(String.Format("1An error occurred in getting file {0} size in {1}.", fileName, dataVolume));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"undelete data block failed,name:{dataVolume},error:{e}");
                throw new FileNotFoundException(String.Format("2An error occurred in getting file {0} size in {1}.", fileName, dataVolume));
            }
        }
        /// <summary>
        /// MakeCurrentVersion
        /// </summary>
        private void SetBlockStatusToCurrentVersion(BlobClient blobClient, string highPlusLowName)
        {
            string blobName = highPlusLowName.Replace(@"\", @"/");
            logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {highPlusLowName}.blobName:{blobName}.");
            blobClient = sourceContainerClient.GetBlobClient(blobName);
            // List all versions of the blob
            List<string> blobVersions = new List<string>();
            foreach (BlobItem blobItem in sourceContainerClient.GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blobName, default))
            {
                logger.Info($"SetBlockStatusToCurrentVersion.Blob name: {blobItem.Name}, Version ID: {blobItem.VersionId}.Version Delete:{blobItem.Deleted}.");
                blobVersions.Add(blobItem.VersionId);
            }
            BlobClient versionedBlobClient = sourceContainerClient.GetBlobClient(blobName).WithVersion(blobVersions.FirstOrDefault());

            // 开始复制操作
            CopyFromUriOperation copyFromUriOperation = blobClient.StartCopyFromUri(versionedBlobClient.Uri);
            // 检查复制状态
            // 等待复制操作完成
            while (!copyFromUriOperation.HasCompleted)
            {
                Thread.Sleep(100);
                copyFromUriOperation.UpdateStatus();
            }
            logger.Info($"SetBlockStatusToCurrentVersion.Blob copy completed successfully.VersionId:{blobVersions.FirstOrDefault()}.");
        }

        private string GetSubsubJobIdFromFileName(string fileName)
        {
            int index = 0;
            if (fileName.Contains("_content_"))
            {
                index = fileName.IndexOf("_content_", StringComparison.OrdinalIgnoreCase);
            }
            else if (fileName.Contains("_meta_"))
            {
                index = fileName.IndexOf("_meta_", StringComparison.OrdinalIgnoreCase);
            }
            if (index > 0)
            {
                return fileName.Substring(0, index);
            }
            return fileName;
        }
        public static LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
                IsSystemStorage = storageDevice.Id == RecordsConstants.AVEPOINT_DEFAULT_STORAGEID || storageDevice.IsSystemStorage
            };

            var logical = new LogicalDeviceDto();
            logical.Name = storageDevice.Name;
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }
        private AbstractXSystem ValidStorage(AbstractXSystem storage)
        {
            if (string.IsNullOrEmpty(storage.ConnectionString))
            {
                var storageLibrary = storage as XLibrary;
                if (storageLibrary == null)
                {
                    logger.Error($"Index storage system not a XLibrary.");
                }
                storage = storageLibrary?.SubSystems?.FirstOrDefault() as AbstractXSystem;
                if (storage == null)
                {
                    logger.Error($"Index storage system's connection string is null.");
                }
            }
            return storage;
        }

        private long DataBlockGetSize(FileType fileType, string fileName, string dataVolume)
        {
            long blockSize = 0;
            var fileInfo = default(XFileInfo);
            if (this.blockSizeMap.ContainsKey(Path.Combine(dataVolume, fileName).ToLower()))
            {
                blockSize = this.blockSizeMap[Path.Combine(dataVolume, fileName).ToLower()];
            }
            else
            {
                var info = this.converter.FormNames(fileType, dataVolume, fileName);
                if (this.LogicalDevice.FileExists(info))
                    fileInfo = this.LogicalDevice.OpenFile(info);
                else
                {
                    if (RecordRestoredFile.IsOrealSoftDelete())
                    {
                        logger.Info($"blob has not exsit,try to undelete it:{fileName}");
                        UnDeleteDataBlock(fileType, fileName, dataVolume);
                        fileInfo = this.LogicalDevice.OpenFile(info);
                    }
                    else
                    {
                        throw new FileNotFoundException(String.Format("An error occurred in getting file {0} size in {1}.", fileName, dataVolume));
                    }
                }
                if (fileInfo != null)
                {
                    blockSize = fileInfo.FileSize;
                    this.blockSizeMap.Add(Path.Combine(dataVolume, fileName).ToLower(), blockSize);
                }
            }
            return blockSize;
        }

        #endregion InputDataListener Methods

        public XStream OpenDataBlockForGetVersion(DataBlockOpenParam param)
        {
            IXSystem dataDevice;
            StorageInfo info = new StorageInfo();
            var fileNameGenerator = this.FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, param.DataVersion);
            info.LowName = fileNameGenerator.Generate(new FileNameParameter(param));
            info.HighName = this.archiverRestoreJob.DataVolume;
            info.Offset = 0;
            info.Length = 4;
            if (param.OpenFromCache)
            {
                dataDevice = this.CacheManager.CacheSystem;
                int retryTime = 0;
                if (param.ShouldDownloadData)
                {
                    while (true)
                    {
                        try
                        {
                            retryTime++;
                            this.CacheManager.DownloadDataFromDevice(this.converter, this.LogicalDevice, param.FileType, info.HighName, info.LowName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryTime > 5)
                            {
                                throw;
                            }
                            this.logger.Warn($"Archiver OpenDataBlockForGetVersion OpenFromCache DownloadDataFromDevice failed and need retry,RetryTime:{retryTime}.Message:{ex}.");
                            this.CacheManager.CacheSystem.DeleteFile(this.converter.FormNames(param.FileType, info.HighName, info.LowName));
                        }
                    }
                }
            }
            else
            {
                dataDevice = this.LogicalDevice;
            }
            return dataDevice.OpenStream(info, FileMode.Open);
        }
    }
}