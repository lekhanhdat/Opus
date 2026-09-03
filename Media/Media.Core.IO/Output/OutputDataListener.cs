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




namespace AvePoint.Media.Core.IO.Output
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.Wrapper.Common;
    using global::Media.Common;
    using global::Media.Core.IO.Output;
    using Merged18NResources.MediaCoreIO;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    #endregion

    public class OutputDataListener<T> : IDisposable, IOutputDataListener where T : IndexBase
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String dataVolume;
        String jobId;
        Int32 maxFileSize;
        bool outFileLevelBlock;
        IXSystem cacheSystem;
        IXSystem dataLogicalDevice;
        MemoryStream metaDataMemoryStream;
        MemoryStream contentDataMemoryStream;
        IOutputDataHandler<T> outputDataHandler;
        IStorageInfoMetaDataBuilderFactory storageInfoBuilderFactory = MediaServiceLocator.Discover<IStorageInfoMetaDataBuilderFactory>();
        AccessTierType accessTier;

        Dictionary<String, String> metaDataInfoDic;
        Boolean storeMD5;
        Boolean isDefaultStorage;
        AccessTierType DefaultStorageAccessTier
        {
            get
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var storageTier = RMGlobalConfiguration.AppConfig[RMAppSettingKey.DEFAULT_STORAGE_TIER];
                if (string.IsNullOrEmpty(envName) || storageTier == "1")
                {
                    return AccessTierType.Cool;
                }
                else
                {
                    return AccessTierType.Cold;
                }
            }
        }
        AccessTierType AccessTierForRule
        {
            get
            {
                return (AccessTierType)WrapperConfiguration.MoveToAnotherTierType;
            }
        }
        public OutputDataListener(OutputDataListenerOpenParameter<T> parameter)
        {
            this.jobId = parameter.JobId;
            this.dataVolume = parameter.DataVolume;
            this.maxFileSize = parameter.MaxFileSize;
            this.cacheSystem = parameter.CacheSystem;
            this.outFileLevelBlock = (parameter.BackupJob as ArchiverBackupJob)?.OutFileLevelBlock == true;
            this.dataLogicalDevice = parameter.DataLogicalDevice;
            this.outputDataHandler = parameter.OutputDataHandler;
            this.metaDataInfoDic = this.GetStorageInfoMetaDatas(parameter.BackupJob);
            this.storeMD5 = parameter.storeMD5;
            this.isDefaultStorage = IsDefaultStorage(parameter.BackupJob.LogicalDevice);
            this.accessTier = parameter.AccessTier;

            ulong totalUsedSpace = 0;
            if (dataLogicalDevice is SpaceInfo spaceInfo)
            {
                totalUsedSpace = spaceInfo.TotalUsedSpace;
            }
            logger.Info("Open OutputDataListener,JobId:{0},LogicalDevice:{1}({2}|Usage:{3}),IsDefaultStorage:{4},dataVolume:{5}, outFileLevelBlock: {6}",
                jobId,
                dataLogicalDevice.SystemName,
                dataLogicalDevice.StorageType,
                totalUsedSpace,
                isDefaultStorage,
                dataVolume, 
                outFileLevelBlock
                );
        }

        private bool IsDefaultStorage(LogicalDeviceDto logicalDeviceDto)
        {
            try
            {
                foreach (var physicalDrive in logicalDeviceDto.PhysicalDrives)
                {
                    if (physicalDrive.IsDefaultDevice())
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while checking default storage. Reason: {0}", ex.ToString());
                return false;
            }
        }

        //public StorageResult CommitDataBlock(string fileName, bool closing)
        //{
        //    StorageResult storageResult = null;
        //    StorageInfo storageInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
        //    storageInfo.DataType = DataBlockType.MetaData;
        //    if (dataLogicalDevice.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
        //    {
        //        storageResult = new StorageResult();
        //        storageResult.NeedCommit = true;
        //        logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedNoUpload);
        //    }
        //    else
        //    {
        //        storageInfo.Length = cacheSystem.OpenFile(storageInfo).FileSize;//针对cloud介质
        //        logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedBeforeUpload, Path.Combine(dataVolume, fileName), storageInfo.Length);
        //        storageInfo.IsClosing = closing;
        //        //storageInfo.FileTierType = this.accessTier;
        //        storageResult = UploadDataToLogicalDevice(storageInfo, storageInfo);
        //        logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedUpload, Path.Combine(dataVolume, fileName));
        //    }
        //    return storageResult;
        //}

        public void CommitDataBlock(FileType fileType, string fileName, bool closing, OutputStreamLevel outputLevel, string itemName = null)
        {
            StorageResult storageResult = null;
            int prefixNumber = GetPrefixNumber(fileName, fileType);
            string cacheName = GetCacheDataName(fileType, prefixNumber);
            if (outputLevel == OutputStreamLevel.FileLevel)
            {
                this.metaDataInfoDic["OriginalFileName"] = itemName ?? string.Empty;
            }
            StorageInfo cacheInfo = XConvert.FromNames(dataVolume, cacheName, this.metaDataInfoDic);
            StorageInfo storageInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
            //storageInfo.FileTierType = this.accessTier;
            switch (fileType)
            {
                case FileType.MetaData:
                    storageInfo.DataType = DataBlockType.MetaData;
                    break;
                case FileType.Content:
                    storageInfo.DataType = DataBlockType.ContentData;
                    break;
                default:
                    storageInfo.DataType = DataBlockType.Other;
                    break;
            }

            if (this.outFileLevelBlock && fileType == FileType.Content && this.dataLogicalDevice.StorageType == XStorageType.Azure)
            {
                storageResult = new StorageResult();
                storageResult.NeedCommit = true;
                logger.Info("File data content already uploaded.");
            }
            else if (dataLogicalDevice.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                storageResult = new StorageResult();
                storageResult.NeedCommit = true;
                logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedNoUpload);
            }
            else
            {
                if (dataLogicalDevice.StorageInterfaceType == StorageInterfaceType.Namespace)
                {
                    bool isFileExists = false;
                    try
                    {
                        isFileExists = dataLogicalDevice.FileExists(storageInfo);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("An error occurred while checking if the file exists. Reason: {0}", ex.ToString());
                        if (this.dataLogicalDevice.StorageType == XStorageType.Dropbox
                            && ex.InnerException != null && ex.InnerException is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            logger.Warn("Dropbox returned a conflict error when checking if the file exists. Treating as file doesn't exist.");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    if (isFileExists)
                    {
                        throw new FileAlreadyExistException("file already exists " + storageInfo.HighPlusLowName);
                    }
                }
                if (MediaConfigInfo.CommonConfigInfo.UseMemoryStream && this.maxFileSize < ServiceConstants.MemoryStreamLimit)
                {
                    storageInfo.Length = GetMemoryStream(fileType).Length;
                }
                else
                {
                    storageInfo.Length = cacheSystem.OpenFile(cacheInfo).FileSize;//针对cloud介质
                }
                logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedBeforeUpload, Path.Combine(dataVolume, fileName), storageInfo.Length);
                storageInfo.IsClosing = closing;
                List<T> indexes = outputDataHandler.GetIndexesNeedToCommit(fileType);
                storageResult = UploadDataToLogicalDevice(fileType, storageInfo, cacheInfo, indexes);
                if (storageResult.URI == null)
                {
                    storageResult.URI = new XURIResult();
                }
                if (storageResult.URI.SInfo == null)
                {
                    storageResult.URI.SInfo = storageInfo.Clone();
                }
                storageResult.URI.SInfo.LowName = fileName;
                logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockCommittedUpload, Path.Combine(dataVolume, fileName));
            }
            outputDataHandler.AfterDataBlockCommit(fileType, storageResult, closing, storageInfo.Length);
        }
        //TODO
        //not for memoryDataStream
        private String StoreMD5ValueInDataFile(StorageInfo cacheInfo)
        {
            //if (!cacheSystem.FileExists(cacheInfo))
            //{

            //}
            long time = DateTime.Now.Ticks;
            if (cacheSystem.OpenFile(cacheInfo).FileSize <= 4096)
            {
                logger.Info("file length is " + cacheSystem.OpenFile(cacheInfo).FileSize);
                return "";
            }

            byte[] fakeMD5Bytes = null;
            cacheInfo.FileAccess = FileAccess.ReadWrite;
            using (Stream stream = cacheSystem.OpenStream(cacheInfo, FileMode.OpenOrCreate))
            {
                using (var hashAlgorithm = new AveCrc64())
                {
                    byte[] buffer = new byte[65536];
                    stream.Position = 4096;
                    while (true)
                    {
                        int readLen = stream.Read(buffer, 0, buffer.Length);
                        if (readLen <= 0)
                            break;
                        hashAlgorithm.TransformBlock(buffer, 0, readLen, null, 0);
                    }
                    hashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
                    fakeMD5Bytes = new byte[16];
                    hashAlgorithm.Hash.CopyTo(fakeMD5Bytes, 0);
                    stream.Position = 30;
                    stream.Write(fakeMD5Bytes, 0, fakeMD5Bytes.Length);
                }
            }
            time = DateTime.Now.Ticks - time;
            logger.Info("cal md5 time is " + (time / 10000));
            return Convert.ToBase64String(fakeMD5Bytes);
        }

        public Stream ChangeDataBlock(FileType fileType, int prefixNumber, int fileNumber, out string fileName)
        {
            Stream stream;
            fileName = outputDataHandler.GetDataFileName(fileType, prefixNumber, fileNumber);
            string cacheName = GetCacheDataName(fileType, prefixNumber);
            logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockChangingChange, Path.Combine(dataVolume, fileName));
            if(this.outFileLevelBlock && fileType == FileType.Content && this.dataLogicalDevice.StorageType == XStorageType.Azure)
            {
                StorageInfo fileInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
                if (WrapperConfiguration.MoveToArchiverTierWhenArchiving || isDefaultStorage)
                {
                    var azureInfo = ConvertStorageInfoToAzureCloudInfo(fileInfo);
                    fileInfo = azureInfo;
                    if (isDefaultStorage)
                    {
                        azureInfo.FileTierType = DefaultStorageAccessTier;
                    }
                    else
                    {
                        azureInfo.FileTierType = AccessTierForRule;
                    }
                }

                stream = new ArchiverFileLevelBlockAzureStream(dataLogicalDevice, fileInfo, (IOutputDataHandler<ArchiverBasicIndex>)this.outputDataHandler);
            }
            else if (dataLogicalDevice.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                StorageInfo directoryInfo = XConvert.FromNames(dataVolume, string.Empty, this.metaDataInfoDic);
                StorageInfo fileInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
                stream = dataLogicalDevice.OpenStream(fileInfo, FileMode.OpenOrCreate);
            }
            else
            {
                if (MediaConfigInfo.CommonConfigInfo.UseMemoryStream && this.maxFileSize < ServiceConstants.MemoryStreamLimit)
                {
                    stream = InitializeMemoryStream(fileType);
                }
                else
                {
                    StorageInfo directoryInfo = XConvert.FromNames(dataVolume, string.Empty, this.metaDataInfoDic);
                    StorageInfo fileInfo = XConvert.FromNames(dataVolume, cacheName, this.metaDataInfoDic);
                    if (cacheSystem.FileExists(fileInfo))
                    {
                        stream = cacheSystem.OpenStream(fileInfo, FileMode.Truncate);
                    }
                    else
                    {
                        stream = cacheSystem.OpenStream(fileInfo, FileMode.CreateNew);
                    }
                }
            }
            return stream;
        }

        /// <summary>
        /// 使用UnformattedOutputStream需要实现这个方法，否则不用实现
        /// </summary>
        public Stream ChangeDataBlock(FileType fileType, string fileName)
        {
            XStream stream;
            logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockChangingChangeUnFormatedBegin);
            if (dataLogicalDevice.IsDirectSystem && !MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                StorageInfo directoryInfo = XConvert.FromNames(dataVolume, string.Empty, this.metaDataInfoDic);
                StorageInfo fileInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
                stream = dataLogicalDevice.OpenStream(fileInfo, FileMode.OpenOrCreate);
            }
            else
            {
                StorageInfo directoryInfo = XConvert.FromNames(dataVolume, string.Empty, this.metaDataInfoDic);
                StorageInfo fileInfo = XConvert.FromNames(dataVolume, fileName, this.metaDataInfoDic);
                if (cacheSystem.FileExists(fileInfo))
                {
                    stream = cacheSystem.OpenStream(fileInfo, FileMode.Truncate);
                }
                else
                {
                    stream = cacheSystem.OpenStream(fileInfo, FileMode.CreateNew);
                }
                logger.Info(MediaCoreIOResource.OutputDataListenerDataBlockChangingChange, Path.Combine(dataVolume, fileName));
            }
            return stream;
        }

        String GetCacheDataName(FileType fileType, int prefixNumber)
        {
            String cacheName;
            switch (fileType)
            {
                case FileType.MetaData:
                    cacheName = this.jobId + "_" + prefixNumber + "_" + ServiceConstants.MetaDataCacheName;
                    break;
                case FileType.Content:
                    cacheName = this.jobId + "_" + prefixNumber + "_" + ServiceConstants.ContentDataCacheName;
                    break;
                default:
                    throw new UnknownFileTypeException(String.Format(MediaCoreIOResource.OutputDataListenerGetCacheDataNameUnknownTypeException, fileType.ToString()));
            }
            return cacheName;
        }

        private int GetPrefixNumber(string fileName, FileType fileType)
        {
            int preFixNumber = -1;
            try
            {
                switch (fileType)
                {
                    case FileType.MetaData:
                        preFixNumber = Convert.ToInt32(fileName.Substring(4, fileName.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 4));
                        break;
                    case FileType.Content:
                        preFixNumber = Convert.ToInt32(fileName.Substring(7, fileName.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 7));
                        break;
                    default:
                        throw new UnknownFileTypeException(String.Format(MediaCoreIOResource.OutputDataListenerGetCacheDataNameUnknownTypeException, fileType.ToString()));
                }
            }
            catch (Exception e)
            {
                logger.Debug(MediaCoreIOResource.OutputDataListenerGetPrefixNumberException, fileName, e.Message);
            }
            return preFixNumber;
        }

        private Stream InitializeMemoryStream(FileType fileType)
        {
            Stream stream;
            switch (fileType)
            {
                case FileType.MetaData:
                    metaDataMemoryStream = new MemoryStream(this.maxFileSize * IOConstants.MB);
                    stream = metaDataMemoryStream;
                    break;
                case FileType.Content:
                    contentDataMemoryStream = new MemoryStream(this.maxFileSize * IOConstants.MB);
                    stream = contentDataMemoryStream;
                    break;
                default:
                    throw new UnknownFileTypeException(string.Format(MediaCoreIOResource.OutputDataListenerGetCacheDataNameUnknownTypeException, fileType.ToString()));
            }
            return stream;
        }

        private StorageResult UploadDataToLogicalDevice(FileType fileType, StorageInfo storageInfo, StorageInfo cacheInfo, List<T> indexes)
        {
            using (LogPerformance lp = new($"UploadDataToLogicalDevice {storageInfo.LowName}"))
            {
                StorageResult storageResult = null;
                try
                {
                    if (MediaConfigInfo.CommonConfigInfo.UseMemoryStream && this.maxFileSize < ServiceConstants.MemoryStreamLimit)
                    {
                        storageResult = UploadDataToLogicalDeviceByMemoryStream(fileType, storageInfo, indexes);
                    }
                    else
                    {
                        storageResult = UploadDataToLogicalDeviceByFileStream(fileType, storageInfo, cacheInfo, indexes);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(MediaCoreIOResource.OutputDataListenerUploadDataToLogicalDeviceException, cacheInfo.LowName, ex.ToString());
                    throw;
                }
                finally
                {
                    if (MediaConfigInfo.CommonConfigInfo.UseMemoryStream && this.maxFileSize < ServiceConstants.MemoryStreamLimit)
                    {
                        MemoryStream stream = GetMemoryStream(fileType);
                        stream.Close();
                        stream = null;
                    }
                }
                return storageResult;
            }
        }

        public string MetaDataKeyNamePlatform { get { return "Platform"; } }
        public string MetaDataKeyNameComponent { get { return "Component"; } }
        public string MetaDataKeyNameGranularFarmName { get { return "Granular-FarmName"; } }
        public string MetaDataKeyNameGranularWebAppName { get { return "Granular-WebAppName"; } }
        public string MetaDataKeyNameGranularPlanId { get { return "Granular-PlanId"; } }
        public string MetaDataKeyNameGranularCycleId { get { return "Granular-CycleId"; } }
        public string MetaDataKeyNameGranularJobId { get { return "Granular-JobId"; } }
        public string MetaDataKeyNameGranularDataMode { get { return "Granular-DataMode"; } }
        public string MetaDataKeyNameExchangePlanId { get { return "Exchange-PlanId"; } }
        public string MetaDataKeyNameExchangeCycleId { get { return "Exchange-CycleId"; } }
        public string MetaDataKeyNameExchangeJobId { get { return "Exchange-JobId"; } }
        public string MetaDataKeyNameExchangeDataMode { get { return "Exchange-DataMode"; } }

        Dictionary<String, String> GetStorageInfoMetaDatas(ExchangeBackupJob backupJob)
        {
            

        var metaDataDictionary = new Dictionary<string, string>();
            metaDataDictionary[MetaDataKeyNamePlatform] = ServiceConstants.DocAve;
            metaDataDictionary[MetaDataKeyNameComponent] = "ExchangeBackup";
            metaDataDictionary[MetaDataKeyNameExchangePlanId] = backupJob.PlanId;
            //metaDataDictionary[MetaDataKeyNameExchangeCycleId] = backupJob.CycleId;
            metaDataDictionary[MetaDataKeyNameExchangeJobId] = backupJob.JobId;
            metaDataDictionary[MetaDataKeyNameExchangeDataMode] = backupJob.DataMode.ToString();

            return metaDataDictionary;
        }
        Dictionary<String, String> GetGDriveStorageInfoMetaDatas(GDriveBackupJob backupJob)
        {
            var metaDataDictionary = new Dictionary<string, string>();
            metaDataDictionary[MetaDataKeyNamePlatform] = ServiceConstants.DocAve;
            metaDataDictionary[MetaDataKeyNameComponent] = "GDrvieBackup";
            metaDataDictionary[MetaDataKeyNameExchangePlanId] = backupJob.PlanId;
            metaDataDictionary[MetaDataKeyNameExchangeJobId] = backupJob.JobId;
            metaDataDictionary[MetaDataKeyNameExchangeDataMode] = backupJob.DataMode.ToString();

            return metaDataDictionary;
        }
        Dictionary<String, String> GetStorageInfoMetaDatas(BackupJobBase backupJob)
        {
            if (backupJob is ExchangeBackupJob eb)
            {
                return GetStorageInfoMetaDatas(eb);
            }
            else if(backupJob is GDriveBackupJob gb)
            {
                return GetGDriveStorageInfoMetaDatas(gb);
            }
            Dictionary<String, String> metaData = null;
            var storageInfoMetaDataBuilderAttributes = backupJob.GetType().GetCustomAttributes(typeof(StorageInfoMetaDataBuilderAttribute), true);
            var builderId = backupJob.GetType().FullName;
            if (storageInfoMetaDataBuilderAttributes.Length > 0)
                builderId = (storageInfoMetaDataBuilderAttributes[0] as StorageInfoMetaDataBuilderAttribute).Key ?? builderId;
            if (!string.IsNullOrEmpty(builderId) && builderId.Equals("AvePoint.Media.Service.DomainModel.ArchiverStorageInfoMetaDataBuilder", StringComparison.OrdinalIgnoreCase))
            {
                var storageMetaDataBuilder = new ArchiverStorageInfoMetaDataBuilder();
                metaData = storageMetaDataBuilder.BuildMetaData(backupJob);
            }
            else
            {
                var storageMetaDataBuilder = this.storageInfoBuilderFactory.GetMetaDataBuilder(builderId);
                metaData = storageMetaDataBuilder.BuildMetaData(backupJob);
                this.storageInfoBuilderFactory.ReleaseMetaDataBuilder(storageMetaDataBuilder);
            }
            return metaData;
        }

        MemoryStream GetMemoryStream(FileType fileType)
        {
            MemoryStream memoryStream;
            switch (fileType)
            {
                case FileType.MetaData:
                    memoryStream = metaDataMemoryStream;
                    break;
                case FileType.Content:
                    memoryStream = contentDataMemoryStream;
                    break;
                default:
                    memoryStream = null;
                    break;
            }
            return memoryStream;
        }

        private Storage.Cloud.Azure.AzureCloudInfo ConvertStorageInfoToAzureCloudInfo(StorageInfo storageInfo)
        {
            Storage.Cloud.Azure.AzureCloudInfo azureInfo = new Storage.Cloud.Azure.AzureCloudInfo();
            azureInfo.HighName = storageInfo.HighName;
            azureInfo.LowName = storageInfo.LowName;
            azureInfo.Length = storageInfo.Length;
            return azureInfo;
        }

        private Storage.Cloud.Google.GoogleCloudInfo ConvertStorageInfoToGoogleCloudInfoWithColdLineTier(StorageInfo storageInfo)
        {
            Storage.Cloud.Google.GoogleCloudInfo googleInfo = new Storage.Cloud.Google.GoogleCloudInfo();
            googleInfo.HighName = storageInfo.HighName;
            googleInfo.LowName = storageInfo.LowName;
            googleInfo.Length = storageInfo.Length;
            googleInfo.StorageClass = Storage.Cloud.Google.GoogleStorageClass.Coldline;
            return googleInfo;
        }

        private StorageResult UploadDataToLogicalDeviceByFileStream(FileType fileType, StorageInfo storageInfo, StorageInfo cacheInfo, List<T> indexes)
        {
            String contentMD5 = String.Empty;
            if (storeMD5)
            {
                contentMD5 = StoreMD5ValueInDataFile(cacheInfo);
            }

            StorageResult storageResult;
            byte[] buffer = new byte[64 * 1024];
            if (fileType == FileType.MetaData)
            {
                storageInfo.DataType = DataBlockType.MetaData;
            }
            using (XStream cacheStream = cacheSystem.OpenStream(cacheInfo, FileMode.Open))
            {
                if (isDefaultStorage)
                {
                    if (this.dataLogicalDevice.StorageType == XStorageType.GoogleCloud)
                    {
                        var googleInfo = ConvertStorageInfoToGoogleCloudInfoWithColdLineTier(storageInfo);
                        storageResult = dataLogicalDevice.CommitStream(cacheStream, googleInfo);
                    }
                    else
                    {
                        var azureInfo = ConvertStorageInfoToAzureCloudInfo(storageInfo);
                        azureInfo.FileTierType = DefaultStorageAccessTier;
                        storageResult = dataLogicalDevice.CommitStream(cacheStream, azureInfo);
                    }
                }
                else
                {
                    if (WrapperConfiguration.MoveToArchiverTierWhenArchiving && this.dataLogicalDevice.StorageType == XStorageType.Azure)
                    {
                        storageResult = MoveToArchiverTier(storageInfo, cacheStream);
                    }
                    else
                    {
                        storageResult = dataLogicalDevice.CommitStream(cacheStream, storageInfo);
                    }
                }
                //TODO
                //if (storageInfo.FileTierType == AccessTierType.Archive)
                //{
                //    SetFileTierArchive(storageInfo);
                //}
                //else if (isDefaultStorage)
                //{
                //    SetFileTierCool(storageInfo);
                //}
                //dataLogicalDevice.MergeStorageInfo<T>(indexes, storageResult, storageInfoProperty);
                if (fileType == FileType.MetaData)
                {
                    storageResult.NeedCommit = true;
                }
            }
            if (storageResult != null && string.IsNullOrEmpty(storageResult.ContentMD5) && !string.IsNullOrEmpty(contentMD5))
            {
                storageResult.ContentMD5 = contentMD5;
            }
            return storageResult;
        }
        private Storage.StorageResult MoveToArchiverTier(Storage.StorageInfo storageInfo, Stream cacheStream)
        {
            Storage.StorageResult storageResult;
            var azureInfo = ConvertStorageInfoToAzureCloudInfo(storageInfo);
            azureInfo.FileTierType = AccessTierForRule;
            try
            {
                storageResult = dataLogicalDevice.CommitStream(cacheStream, azureInfo);
            }
            catch(Exception e)
            {
                logger.Warn($"something went wrong when upload data,error:{e.ToString()}");
                storageResult = dataLogicalDevice.CommitStream(cacheStream, storageInfo);
            }
            return storageResult;
        }
        private StorageResult UploadDataToLogicalDeviceByMemoryStream(FileType fileType, StorageInfo storageInfo, List<T> indexes)
        {
            StorageResult storageResult;
            byte[] buffer = new byte[64 * 1024];
            if (fileType == FileType.MetaData)
            {
                storageInfo.DataType = DataBlockType.MetaData;
            }
            switch (fileType)
            {
                case FileType.MetaData:
                    {
                        if (isDefaultStorage)
                        {
                            if(this.dataLogicalDevice.StorageType == XStorageType.GoogleCloud)
                            {
                                var googleInfo = ConvertStorageInfoToGoogleCloudInfoWithColdLineTier(storageInfo);
                                storageResult = dataLogicalDevice.CommitStream(metaDataMemoryStream, googleInfo);
                                break;
                            }
                            var azureInfo = ConvertStorageInfoToAzureCloudInfo(storageInfo);
                            azureInfo.FileTierType = DefaultStorageAccessTier;
                            storageResult = dataLogicalDevice.CommitStream(metaDataMemoryStream, azureInfo);
                        }
                        else
                        {
                            if (WrapperConfiguration.MoveToArchiverTierWhenArchiving && this.dataLogicalDevice.StorageType == XStorageType.Azure)
                            {
                                storageResult = MoveToArchiverTier(storageInfo, metaDataMemoryStream);
                            }
                            else
                            {
                                storageResult = dataLogicalDevice.CommitStream(metaDataMemoryStream, storageInfo);
                            }
                        }
                        break;
                    }
                case FileType.Content:
                    {
                        if (isDefaultStorage)
                        {
                            if (this.dataLogicalDevice.StorageType == XStorageType.GoogleCloud)
                            {
                                var googleInfo = ConvertStorageInfoToGoogleCloudInfoWithColdLineTier(storageInfo);
                                storageResult = dataLogicalDevice.CommitStream(contentDataMemoryStream, googleInfo);
                                break;
                            }
                            var azureInfo = ConvertStorageInfoToAzureCloudInfo(storageInfo);
                            azureInfo.FileTierType = DefaultStorageAccessTier;
                            storageResult = dataLogicalDevice.CommitStream(contentDataMemoryStream, azureInfo);
                        }
                        else
                        {
                            if (WrapperConfiguration.MoveToArchiverTierWhenArchiving && this.dataLogicalDevice.StorageType == XStorageType.Azure)
                            {
                                storageResult = MoveToArchiverTier(storageInfo, contentDataMemoryStream);
                            }
                            else
                            {
                                storageResult = dataLogicalDevice.CommitStream(contentDataMemoryStream, storageInfo);
                            }
                        }
                        break;
                    }

                default:
                    throw new Exception(string.Format(MediaCoreIOResource.OutputDataListenerGetCacheDataNameUnknownTypeException, fileType.ToString()));
            }

            //TODO
            //if (storageInfo.FileTierType == AccessTierType.Archive)
            //{
            //    SetFileTierArchive(storageInfo);
            //}
            //else if (isDefaultStorage)
            //{
            //    SetFileTierCool(storageInfo);
            //}
            //dataLogicalDevice.MergeStorageInfo<T>(indexes, storageResult, storageInfoProperty);
            return storageResult;
        }

        public void Dispose()
        {
            if (this.metaDataMemoryStream != null)
            { this.metaDataMemoryStream.Dispose(); }
            if (this.contentDataMemoryStream != null)
            { this.contentDataMemoryStream.Dispose(); }
        }

        public void IncreaseDataSize(long size)
        {
            outputDataHandler.IncreaseMediaDataSize(size);
        }
    }
}