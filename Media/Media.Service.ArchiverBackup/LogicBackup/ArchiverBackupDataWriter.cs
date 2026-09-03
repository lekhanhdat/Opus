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

namespace AvePoint.Media.Service.ArchiverBackup
{
    using AvePoint.Common;
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Core.IO.Output;
    using LOGRESOURCE = Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    using Backup;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Storage;
    using AvePoint.RA.Common;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.Contract.Archiver;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using System.Threading.Tasks;
    using global::Media.Common.ClassicStorageApi;
    using AvePoint.Wrapper.Common;
    using global::Media.Service.ArchiverBackup.LogicBackup;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.RA.Contract.Explorer;

    //using AvePoint.GCommon.MicroKernel;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/1/16",
    "yuchenyang@avepoint.com",
    "dwxue@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_3 },
    "ADO-23485",
    true)]
    [AveCodeReview(
   "2012/7/18",
   "dwxue@avepoint.com",
   "xiaofeiwang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    public class ArchiverBackupDataWriter
        : ApplicationModelServiceBase
        , IArchiverBackupDataWriter
        , IOutputDataHandler<ArchiverBasicIndex>
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ArchiverBackupJob archiverBackupJob;
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        Boolean isOnlyHeader;
        Boolean isIndexOpen;
        Int64 sequence = 1;
        Int64 totalDataSize;
        String currentFolderName;
        String currentSiteCollection;
        String currentListBaseType;
        ArchiverBasicIndex currentIndex;
        List<String> fileHeaders;
        List<ArchiverBasicIndex> indexes;
        List<List<ArchiverBasicIndex>> cacheIndexList;
        IGeneralOutputStream outputStream;
        Dictionary<String, String> scopeIdDictionary = new Dictionary<String, String>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IEncryptionInfoManager EncryptionInfoManager { get; set; }

        public IArchiverBackupIndexService BackupIndexService { get; set; }

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public ArchiverIndexService ArchiverIndexService { get; set; }

        #region DataWriterBase Members

        public void Open(ArchiverBackupJob backupJob)
        {
            try
            {
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterOpenBegin);
                this.archiverBackupJob = backupJob;
                this.dataLogicalDevice = this.StorageDeviceManager.OpenDataSystemForWrite(backupJob.DataLogicalDevice, archiverBackupJob.UseSnapLock);
                this.indexLogicalDevice = this.StorageDeviceManager.Open(backupJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.ReadWrite);
                this.ValidateManager.Validate(new IndexDeviceValidateParameter(backupJob.JobId, backupJob.IndexVolume, ServiceConstants.IndexDBName, this.indexLogicalDevice, backupJob.IndexLogicalDevice));
                this.CacheManager.Open(this.archiverBackupJob.CacheSetting, this.dataLogicalDevice.IsDirectSystem, true);
                this.fileHeaders = new List<String>();
                this.indexes = new List<ArchiverBasicIndex>();
                this.cacheIndexList = new List<List<ArchiverBasicIndex>>();
                //backupJob.Network.SendMessage(ServiceConstants.StringSendToAgent);
                //当前异常处理中的代码，是由Derek Chu 2022七月做OPUS相关功能时添加的逻辑，由于功能涉及比较复杂，且只影响OPUS，当前代码keep原有异常逻辑
                //通过分析代码，当前代码逻辑完全没有必要，但是涉及到是OPUS功能，暂且保留
                try
                {
                    ArchiverIndexServiceOpenParameter indexOpenParam = new ArchiverIndexServiceOpenParameter();
                    indexOpenParam.TreeMode = TreeMode.SiteCollectionMode;
                    indexOpenParam.IndexVolume = backupJob.IndexVolume;
                    indexOpenParam.BackupJobId = backupJob.JobId;
                    indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
                    indexOpenParam.IndexCacheDeviceSystem = XFactory.InstanceLibrary(this.archiverBackupJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
                    indexOpenParam.CacheSetting = this.archiverBackupJob.CacheSetting;
                    indexOpenParam.CheckAccessTier = false;
                    indexOpenParam.IsNeedCreateNewIndex = true;
                    IndexService.Open(indexOpenParam);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while open IndexService. Error:{ex}");
                }
            }
            catch (IndexCanNotFoundException ie)
            {
                logger.Error($"Error occurred while open index device. Error:{ie.ToString()}");
            }
            catch (Exception e)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while open index device. Error:{e.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void Close(BackupCloseInfo info)
        {
            this.ReleaseResourceAsync(info).Wait();
            try
            {
                this.CacheManager.Clear(this.archiverBackupJob.DataVolume, this.archiverBackupJob.JobId, -1);
            }
            catch (System.Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                this.logger.Warn("CacheManager.Clear() has error");
                //this.logger.Warn(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderCloseClearCacheError, ex.ToString());
                info.ErrorMessage += ex.ToString();
                throw;
            }
            try
            {
                this.CacheManager.Close();
            }
            catch (System.Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                this.logger.Warn("CacheManager.Close() has error");
                //this.logger.Warn(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderCloseCloseCacheError, ex.ToString());
                info.ErrorMessage += ex.ToString();
                throw;
            }
            try
            {
                IndexService.Close();
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while closing index device. Error:{e.ToString()}");
            }
            this.StorageDeviceManager.Close(dataLogicalDevice);
            this.StorageDeviceManager.Close(indexLogicalDevice);
            this.indexes.Clear();
            this.cacheIndexList.Clear();
            this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterCloseEnd);
        }

        public void Dispose() { }

        #endregion DataWriterBase Members

        #region Handle Methods

        public void HandleHeader(String headerXml)
        {
            try 
            { 
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderBegin, "");
                var header = new MediaArchiverFileHeader(headerXml);
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderInfo, header.Type, "");
                switch (header.FileHeaderType)
                {
                    case MediaArchiverFileHeaderType.None:
                    case MediaArchiverFileHeaderType.NeedToBackUp:
                        this.isOnlyHeader = false;
                        this.currentIndex = this.AssembleIndexHeader(header);
                        if (header.Type == AveSharePointType.TYPE_SITE)
                        {
                            this.InitResource();
                        }
                        this.outputStream.BeforeItem(this.currentIndex);
                        this.outputStream.WriteHeaderXml(headerXml);
                        this.indexes.Add(currentIndex);
                        break;
                    case MediaArchiverFileHeaderType.NeedToDelete:
                        this.isOnlyHeader = true;
                        this.fileHeaders.Add(headerXml);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Header. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void HandleData(Byte[] buffer, Int32 offset, Int32 dataSize)
        {
            try
            {
                if (!this.isOnlyHeader)
                {
                    this.outputStream.WriteMetaData(buffer, offset, dataSize);
                }
            }
            catch (Exception ex)
            {
                this.outputStream.WriteMetaData(buffer, offset, dataSize);
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Data. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void HandleContentData(Byte[] buffer, Int32 offset, Int32 dataSize)
        {
            try
            {
                if (!this.isOnlyHeader)
                {
                    this.outputStream.WriteContentData(buffer, offset, dataSize);
                }
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Content Data. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void HandleTail(String tailXml)
        {
            try
            {
                if (!this.isOnlyHeader)
                {
                    logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleTailHandle, tailXml);
                    var tail = FileHeaderUtil.ToFileTail(tailXml);
                    this.AssembleIndexTail(this.currentIndex, tail);
                    this.outputStream.EndItem(this.currentIndex);
                    this.outputStream.WriteTailXml(tailXml);
                    currentIndex.HadHandleTail = !tail.IsFailed;
                }
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Tail. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public ArchiverBasicIndex GetArchiverIndex(string md5)
        {
            try
            {
                return this.BackupIndexService.GetCurrentIndex(md5);
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while GetArchiverIndex. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        #endregion Handle Methods

        #region IOutputDataHandler Members

        //暂时不支持centera 只支持数据块换块立刻上传的介质
        public void AfterDataBlockCommit(FileType fileType, StorageResult storageResult, Boolean closing, Int64 backupDataSize) //todo
        {
            this.totalDataSize += backupDataSize;
            if (fileType == FileType.Content)
            {
                this.indexes.ForEach(index => index.HasContentIdMerged = index.HasWrittenContentData);
            }
            if (fileType == FileType.MetaData && storageResult.NeedCommit)
            {
                this.CommitArchiveIndexes();
            }
        }

        public string GetDataFileName(FileType fileType, int prefixNumber, int fileNumber)
        {
            string fileName;
            if (fileType == FileType.Content)
            {
                fileName = this.archiverBackupJob.JobId + "_content_" + fileNumber + ".dat";
            }
            else if (fileType == FileType.MetaData)
            {
                fileName = this.archiverBackupJob.JobId + "_meta_" + fileNumber + ".dat";
            }
            else
            {
                throw new UnknownFileTypeException(String.Format(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterGetDataFileNameUnknownFileTypeException, fileType.ToString()));
            }
            return fileName;
        }

        public List<ArchiverBasicIndex> GetIndexesNeedToCommit(FileType fileType)
        {
            var result = new List<ArchiverBasicIndex>();
            if (fileType == FileType.Content)
            {
                result.AddRange(this.indexes.FindAll(index => index.HasWrittenContentData));
            }
            else if (fileType == FileType.MetaData)
            {
                result.AddRange(this.indexes.FindAll(index => index.HasWrittenMetaData));
            }
            return result;
        }

        #endregion IOutputDataHandler Members

        #region private methods

        private async System.Threading.Tasks.Task ReleaseResourceAsync(BackupCloseInfo info)
        {
            try
            {
                if (this.outputStream != null)
                {
                    this.outputStream.Close();
                }
            }
            catch (Exception ex)
            {
                info.BackupStatus = BackupStatus.ExceptionOccurred;
                info.ErrorMessage += ex.ToString();
                logger.Error("An error occourred while closing outputStream, details:{0}", ex);
                throw;
            }
            finally
            {
                this.outputStream = null;
            }
            try
            {
                if (this.isIndexOpen)
                {
                    this.scopeIdDictionary.Clear();
                    this.indexes.Clear();
                    this.cacheIndexList.Clear();
                    this.ArchiverIndexService.Close();
                    this.ArchiverIndexService.UploadSubIndexToRealDevice();
                    await ArchiverManagementService.UpdateSiteMasterMediaDataSizeAsync(this.archiverBackupJob.JobId, this.totalDataSize, IdentityManager.IdentityContent);
                    if (this.totalDataSize > 0)
                    {
                        await StorageDeviceService.UpdateLastArchivedTimeAsync(this.GetFirstOnlinePhysicalDriveId(), DateTime.UtcNow.Ticks);
                    }
                    this.isIndexOpen = false;
                }
            }
            catch (Exception ex)
            {
                info.BackupStatus = BackupStatus.ExceptionOccurred;
                info.ErrorMessage += ex.ToString();
                logger.Error("An error occourred while uploading subindex to real device, details:{0}", ex);
                throw;
            }
            this.currentSiteCollection = null;
        }



        private void InitResource()
        {
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter(this.archiverBackupJob, this.CacheManager.CacheSystem, this.indexLogicalDevice);
            this.ArchiverIndexService.Open(indexServiceOpenParameter);
            this.isIndexOpen = true;
            var serializedEncryptionInfo = this.EncryptionInfoManager.PutEncryptionInfo(this.archiverBackupJob.DataEncryptionInfoWrapper);
            this.BackupIndexService.UpdateJobInfoIndex(this.archiverBackupJob.JobId, ServiceConstants.EncryptionInfoKey, serializedEncryptionInfo);

            OpenOutputStreamParameter openStreamParam = this.AssembleOutputStreamParameter();
            this.outputStream = OutputStreamFactory.GetOutputStream(openStreamParam);
            this.outputStream.Open();

            ArchiverSiteMasterIndex siteMasterIndex = this.AssembleMasterIndex();
            this.BackupIndexService.InsertSiteMaster(siteMasterIndex);
            this.InsertControlSiteMasterIndex(siteMasterIndex);
        }

        private void InsertControlSiteMasterIndex(ArchiverSiteMasterIndex siteMasterIndex)
        {
            var archiverIndexSubInfo = new ArchiverIndexSubInfoContract()
            {
                Id = Guid.NewGuid().ToString(),
                JobId = this.archiverBackupJob.JobId,
                LogicalDeviceId = this.archiverBackupJob.DataLogicalDevice.Id,
                PhysicalDeviceId = this.GetFirstOnlinePhysicalDriveId(),
                StoragePolicyId = this.archiverBackupJob.StoragePolicyId,
                RetentionTime = this.archiverBackupJob.ArchiveTime,
                RetentionTimeSpanSeconds = this.archiverBackupJob.RetentionTimeSpanSeconds
            };
            if (this.archiverBackupJob.DataEncryptionInfoWrapper != null)
                archiverIndexSubInfo.DataEncryptionInfo = this.archiverBackupJob.DataEncryptionInfoWrapper.EncryptionInfo;
            var archiverIndexSubInfoList = new List<ArchiverIndexSubInfoContract>();
            archiverIndexSubInfoList.Add(archiverIndexSubInfo);
            var archiverSiteMasterIndexContract = new ArchiverSiteMasterIndexContract
            {
                JobId = siteMasterIndex.JobId.Substring(0, siteMasterIndex.JobId.LastIndexOf('_')),
                Id = Guid.NewGuid().ToString(),
                ArchiverTime = siteMasterIndex.BackupTime,
                FarmName = siteMasterIndex.FarmName,
                FarmId = this.archiverBackupJob.FarmId,
                IndexDeviceId = this.archiverBackupJob.IndexLogicalDevice.Id,
                WebURL = this.archiverBackupJob.WebAppUrl,
                SiteURL = this.archiverBackupJob.SiteUrl,
                WebId = this.archiverBackupJob.WebAppId,
                SiteId = this.archiverBackupJob.SiteId,
                JobState = 0,
                StoragePolicyId = this.archiverBackupJob.StoragePolicyId,
                SPVersion = this.archiverBackupJob.SpVersion,
                SubInfo = archiverIndexSubInfoList,
                Module = IndexModule.Archiver,
                RuleId = archiverBackupJob.RuleId,
                SourceFlag = archiverBackupJob.SourceFlag,
                DataFlag = archiverBackupJob.DataFlag,
                O365TenantId = archiverBackupJob.O365TenantId,
                VersionDetails = new VersionDetails()
                {
                    PlatformType = AvePoint.GCommon.Contract.Media.Object.PlatformType.DocAve,
                    ProductVersion = ProductVersion.Product6X,
                    LastImportedTime = 0
                },
                Extension = new ArchiverSiteMasterIndexExtension()
                {
                    UpdateTime = DateTime.UtcNow.Ticks,
                },
                BackupFileType = GetBackupFileType(),
            };
            //this.ControlStubs.ArchiverBackupService.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract, IdentityManager.IdentityContent);
            //this.ControlStubs.ArchiverGeneralServiceSiteMasterIndexService.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract);
            ArchiverSiteMasterIndexDao.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract);
        }

        //Records用自己的File Level，走自己的Retention逻辑，不跟Archiver走同样File Level Retention
        private int GetBackupFileType()
        {
            int backupFileType = (int)BackupFileType.DataBlock;
            if (archiverBackupJob.IsRAJob)
            {
                if(archiverBackupJob.OutFileLevelBlock)
                {
                    backupFileType = (int)BackupFileType.RecordsFile;
                }
            }
            else
            {
                if (archiverBackupJob.OutFileLevelBlock)
                {
                    backupFileType = (int)BackupFileType.File;
                }
            }
            return backupFileType;
        }

        private String GetFirstOnlinePhysicalDriveId()
        {
            string physicalDeviceId = (this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID;
            this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterGetFirstOnlinePhysicalDriveId, physicalDeviceId);
            //foreach (string physicalDeviceConnectionString in this.archiverBackupJob.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data))
            //{
            //    if (physicalDeviceId == string.Empty)
            //    {
            //        IXSystem physicalDeviceSystem = XFactory.InstanceSystem(physicalDeviceConnectionString);
            //        physicalDeviceSystem.Open();
            //        if (physicalDeviceSystem.SystemStatus == XSystemStatus.Online)
            //        {
            //            physicalDeviceId = physicalDeviceSystem.SystemID;
            //        }
            //        physicalDeviceSystem.Close();
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            return physicalDeviceId;
        }

        private void CommitArchiveIndexes()
        {
            List<ArchiverBasicIndex> indexesNeedToCommit = this.indexes.FindAll(index => index.HadHandleTail && index.HasContentIdMerged);
            if (indexesNeedToCommit.Count > 0)
            {
                this.BackupIndexService.InsertArchiveIndexes(indexesNeedToCommit);
                this.indexes.RemoveAll(index => index.HadHandleTail && index.HasContentIdMerged);
            }
        }

        private OpenOutputStreamParameter AssembleOutputStreamParameter()
        {
            var outputDataListenerOpenParameter = new OutputDataListenerOpenParameter<ArchiverBasicIndex>
            {
                CacheSystem = this.CacheManager.CacheSystem,
                DataLogicalDevice = this.dataLogicalDevice,
                DataVolume = this.archiverBackupJob.DataVolume,
                JobId = this.archiverBackupJob.JobId,
                OutputDataHandler = this,
                BackupJob = this.archiverBackupJob,
                MaxFileSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize,
            };
            if (this.archiverBackupJob.UseArchiveTier)
            {
                outputDataListenerOpenParameter.AccessTier = AccessTierType.Archive;
            }
            var outputDataListener = new OutputDataListener<ArchiverBasicIndex>(outputDataListenerOpenParameter);
            var openStreamParam = new OpenOutputStreamParameter();
            openStreamParam.PrefixNumber = string.Empty;
            openStreamParam.InitMetaDataFileNumber = 0;
            openStreamParam.InitContentDataFileNumber = 0;
            openStreamParam.DataMode = this.archiverBackupJob.DataMode;
            openStreamParam.DataListener = outputDataListener;
            openStreamParam.CompressionType = this.archiverBackupJob.CompressionType;
            openStreamParam.CompressionMethod = CompressionMethods.ZLIB_COMPRESSION;
            openStreamParam.EncryptionInfo = this.archiverBackupJob.EncryptionInfo;
            openStreamParam.SPVersion = this.archiverBackupJob.SpVersion;
            openStreamParam.MaxBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize * IOConstants.MB;
            if (this.archiverBackupJob.OutFileLevelBlock)
            {
                logger.Info("Backup job set OutputLevel to FileLevel");
                openStreamParam.OutputLevel = OutputStreamLevel.FileLevel;
            }
            return openStreamParam;
        }

        private ArchiverBasicIndex AssembleIndexHeader(MediaArchiverFileHeader header)
        {
            ArchiverBasicIndex basicIndex;
            string fullPath = string.Empty;
            string parentPath = string.Empty;

            switch (header.Type)
            {
                case AveSharePointType.TYPE_SITE:
                    basicIndex = new ArchiverHeadIndex();
                    //if mapping(SharePoint) ,use site url before mapping
                    this.currentSiteCollection = this.archiverBackupJob.SiteUrl;
                    parentPath = string.Empty;
                    fullPath = this.currentSiteCollection;
                    break;
                case AveSharePointType.TYPE_WEB:
                case AveSharePointType.TYPE_LIST:
                case AveSharePointType.TYPE_APP:
                    basicIndex = new ArchiverHeadIndex();
                    currentFolderName = header.Path;
                    int index = currentFolderName.Contains("\\") ? currentFolderName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : currentFolderName.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    parentPath = index > 0 ? this.currentSiteCollection + "\\" + currentFolderName.Substring(0, index) : this.currentSiteCollection;
                    fullPath = this.currentSiteCollection + "\\" + currentFolderName;
                    if (header.Type.Equals(AveSharePointType.TYPE_LIST))
                    {
                        basicIndex.ListBaseType = header.ListBaseType;
                        this.currentListBaseType = basicIndex.ListBaseType;
                    }
                    break;
                case AveSharePointType.TYPE_FOLDER:
                    basicIndex = new ArchiverHeadIndex();
                    currentFolderName = header.Path;
                    Int32 folderIndex = currentFolderName.Contains("\\") ? currentFolderName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : currentFolderName.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    parentPath = folderIndex > 0 ? this.currentSiteCollection + "\\" + currentFolderName.Substring(0, folderIndex) : this.currentSiteCollection;
                    fullPath = this.currentSiteCollection + "\\" + currentFolderName;
                    break;
                case AveSharePointType.TYPE_LISTITEM:
                case AveSharePointType.TYPE_DOCUMENT:
                case AveSharePointType.TYPE_ATTACHMENTS:
                    basicIndex = new ArchiverBodyIndex();
                    if (this.currentSiteCollection.Equals(currentFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        parentPath = this.currentSiteCollection;
                    }
                    else
                    {
                        parentPath = this.currentSiteCollection + "\\" + currentFolderName;
                    }
                    fullPath = parentPath + "\\" + header.Path;
                    basicIndex.CreateTime = header.CreateTime;
                    basicIndex.ModifyTime = header.ModifyTime;
                    basicIndex.Author = header.Author;
                    basicIndex.Editor = header.Editor;
                    basicIndex.NodeGuid = header.NodeGuid;
                    basicIndex.stubInfo = header.StubInfo;
                    try
                    {
                        basicIndex.BackupFileType = Convert.ToInt32(header.BackupFileType);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"basicIndex.BackupFileType has warn:{ex}.");
                        basicIndex.BackupFileType = 0;
                    }
                    break;
                default:
                    throw new NotSupportedException(String.Format(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterAssembleIndexHeaderError, header.Type));
            }
            if (fullPath == null || parentPath == null)
            {
                throw new ArgumentNullException("fullPath or parentPath is null");
            }
            basicIndex.Id = Guid.NewGuid().ToString();
            basicIndex.Flag = this.archiverBackupJob.DataMode;
            basicIndex.Type = Convert.ToChar(header.Type).ToString();
            basicIndex.Name = header.Type.Equals(AveSharePointType.TYPE_SITE) ? this.currentSiteCollection : header.Path;
            if (header.Type.Equals(AveSharePointType.TYPE_LIST))
            {
                basicIndex.PathMD5 = (fullPath + this.currentListBaseType).ToMD5HashCode();
            }
            else
            {
                basicIndex.PathMD5 = fullPath.ToMD5HashCode();
            }
            if ((header.Type.Equals(AveSharePointType.TYPE_LISTITEM)
                || header.Type.Equals(AveSharePointType.TYPE_DOCUMENT)
                || header.Type.Equals(AveSharePointType.TYPE_ATTACHMENTS)))
            {
                basicIndex.ParentPathMD5 = (parentPath + this.currentListBaseType).ToMD5HashCode();
            }
            else
            {
                basicIndex.ParentPathMD5 = parentPath.ToMD5HashCode();
            }

            basicIndex.IsAppData = header.IsAppData.ToString();
            if (header.IsAppData && this.currentIndex.Type.EqualsIgnoreCase("W"))
            {
                this.currentIndex.Type = "P";
            }
            basicIndex.AppDataName = header.AppDataName;
            basicIndex.SitePath = this.currentSiteCollection;
            basicIndex.ArchiveTime = this.archiverBackupJob.ArchiveTime;
            basicIndex.ListType = header.IsMyProfileList;
            basicIndex.PlanId = this.archiverBackupJob.PlanId;
            basicIndex.CycleId = this.archiverBackupJob.JobId;
            basicIndex.JobId = this.archiverBackupJob.JobId;
            basicIndex.Sequence = sequence++;
            basicIndex.StoragePolicyId = this.archiverBackupJob.StoragePolicyId;
            basicIndex.Url = header.Url;
            basicIndex.ExtraInfo = header.HeaderExtraAttribute;
            if (archiverBackupJob.UseArchiveTier)
                SetAccessTierToFlagExtend(basicIndex, AccessTierType.Archive);

            basicIndex.Retention = archiverBackupJob.RuleId;

            return basicIndex;
        }

        private ArchiverSiteMasterIndex AssembleMasterIndex()
        {
            ArchiverSiteMasterIndex siteMasterIndex = new ArchiverSiteMasterIndex();
            if (this.currentSiteCollection != null)
            {
                siteMasterIndex.BackupTime = this.archiverBackupJob.ArchiveTime;
                siteMasterIndex.RetentionTimeSpanSeconds = this.archiverBackupJob.RetentionTimeSpanSeconds;
                siteMasterIndex.FarmId = this.archiverBackupJob.FarmId;
                siteMasterIndex.FarmName = this.archiverBackupJob.FarmName;
                siteMasterIndex.ID = Guid.NewGuid().ToString();
                siteMasterIndex.JobId = this.archiverBackupJob.JobId;
                siteMasterIndex.LogicalDrive = this.archiverBackupJob.IndexLogicalDevice.Id
                    + this.archiverBackupJob.DataLogicalDevice.Id;
                siteMasterIndex.FarmId = this.archiverBackupJob.FarmId;
                siteMasterIndex.PlanId = this.archiverBackupJob.PlanId;
                siteMasterIndex.SiteUrl = this.currentSiteCollection;
                siteMasterIndex.SPVersion = this.archiverBackupJob.SpVersion;
                siteMasterIndex.WebAppName = this.archiverBackupJob.WebAppUrl;
                siteMasterIndex.MaxDataBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize;
            }
            return siteMasterIndex;
        }

        private void AssembleIndexTail(ArchiverBasicIndex currentIndex, FileTail tail)
        {
            List<string> attributes = tail.Attributes;
            StringBuilder stringBuilder = new StringBuilder(String.Empty);
            for (int i = 0; i < attributes.Count; i++)
            {
                stringBuilder.Append(attributes[i]).Append(ServiceConstants.ExtraChar);
            }
            currentIndex.Attributes = stringBuilder.ToString();
            currentIndex.Crc = tail.Crc32 == null ? 0 : (long)tail.Crc32;
            currentIndex.StorageCrc64 = ProcessCrc64Result(tail.Crc64);
            currentIndex.IsFailed = tail.IsFailed.ToString();
            currentIndex.IsSystemFile = tail.IsSystemFile.ToString();
        }
        private string ProcessCrc64Result(string crc64)
        {
            string result = string.Empty;
            if (WrapperConfiguration.IsILMode)
            {
                if (WrapperConfiguration.RecordsOutputStreamLevel == 0)
                {
                    result = string.IsNullOrEmpty(crc64) ? string.Empty : crc64;
                }
            }
            else
            {
                if (WrapperConfiguration.ArchiverOutputStreamLevel == 0)
                {
                    result = string.IsNullOrEmpty(crc64) ? string.Empty : crc64;
                }
            }
            return result;
        }
        private void SetAccessTierToFlagExtend(ArchiverBasicIndex archiverBasicIndex, AccessTierType accessTier)
        {
            if (archiverBasicIndex.FlagExtend < 0)
            {
                archiverBasicIndex.FlagExtend = 0;
            }
            archiverBasicIndex.FlagExtend = (archiverBasicIndex.FlagExtend & ~0x3) | ((int)(accessTier));
        }

        public void IncreaseMediaDataSize(long size)
        {
            this.totalDataSize += size;
        }

        public void OpenEXO(ExchangeBackupJob backupJob)
        {
            throw new NotImplementedException();
        }
        public void OpenGDrive(GDriveBackupJob backupJob)
        {
            throw new NotImplementedException();
        }
        #endregion private methods
    }
}