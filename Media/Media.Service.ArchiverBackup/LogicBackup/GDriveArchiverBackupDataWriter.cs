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
using AvePoint.Media.Core.IO.Output;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using LOGRESOURCE = Merged18NResources.MediaServiceArchiverBackup;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.Media.Core.IO;
using AvePoint.GCommon.Utility;
using Cloud.Sdk.EDiscovery.Services;
using Media.Service.ArchiverBackup.Index;
using Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntention;
using Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntentionImpl;
using Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntentionImpl;
using AvePoint.Media.Core.Index;
using System.Xml;
using Media.Service.DomainModel.Index.ExchangeIndexes;
using AvePoint.RA.DB.Dao;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Archiver;
using Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntention;
using Microsoft.Azure.Cosmos.Linq;
namespace Media.Service.ArchiverBackup.LogicBackup
{
    public class GDriveArchiverBackupDataWriter : ApplicationModelServiceBase
        , IArchiverBackupDataWriter
        , IOutputDataHandler<GoogleBasicIndex>
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        GDriveBackupJob archiverBackupJob;
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        Boolean isIndexOpen;
        Int64 sequence = 1;
        Int64 totalDataSize;
        GoogleBasicIndex currentIndex;
        List<GoogleBasicIndex> indexes;
        IGeneralOutputStream outputStream;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IEncryptionInfoManager EncryptionInfoManager => PlatformWindsorManager.GetService<IEncryptionInfoManager>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();
        public IGDriveArchiverBackupIndexService BackupIndexService { get; set; }

        public GDriveArchiverIndexService ArchiverIndexService { get; set; }
        public void AfterDataBlockCommit(FileType fileType, StorageResult storageResult, bool closing, long backupDataSize)
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
            
            this.StorageDeviceManager.Close(dataLogicalDevice);
            this.StorageDeviceManager.Close(indexLogicalDevice);
            this.indexes.Clear();
            this.logger.Info("MediaServiceArchiverBackupResource.ArchiverBackupDataWriterCloseEnd");
        }

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
                    this.indexes.Clear();
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
        }
        private String GetFirstOnlinePhysicalDriveId()
        {
            return (this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public GoogleBasicIndex GetArchiverIndexEx(string md5)
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
        public ArchiverBasicIndex GetArchiverIndex(string md5)
        {
            return new();
        }
        private void InsertControlSiteMasterIndex(GDriveMasterIndex siteMasterIndex)
        {
            var archiverIndexSubInfo = new ArchiverIndexSubInfoContract()
            {
                Id = Guid.NewGuid().ToString(),
                JobId = this.archiverBackupJob.JobId,
                LogicalDeviceId = this.archiverBackupJob.LogicalDevice.Id,
                PhysicalDeviceId = this.GetFirstOnlinePhysicalDriveId(),
                StoragePolicyId = this.archiverBackupJob.StoragePolicyName,
                RetentionTime = this.archiverBackupJob.ArchiverTime,
                RetentionTimeSpanSeconds = 0,
                DataEncryptionInfo = archiverBackupJob.DataEncryptionInfoWrapper != null ? archiverBackupJob.DataEncryptionInfoWrapper.EncryptionInfo : null,
            };
            var archiverSiteMasterIndexContract = new ArchiverSiteMasterIndexContract
            {
                JobId = siteMasterIndex.JobId.Substring(0, siteMasterIndex.JobId.LastIndexOf('_')),
                Id = Guid.NewGuid().ToString(),
                ArchiverTime = siteMasterIndex.BackupTime,
                IndexDeviceId = this.archiverBackupJob.IndexLogicalDevice.Id,
                SiteURL = this.archiverBackupJob.DriveName,
                SiteId = this.archiverBackupJob.DriveId,
                WebId = this.archiverBackupJob.TenantId,
                JobState = 0,
                StoragePolicyId = this.archiverBackupJob.StoragePolicyName,
                SubInfo = [archiverIndexSubInfo],
                Module = IndexModule.Archiver,
                RuleId = archiverBackupJob.RuleId,
                SourceFlag = (int)SourceFlag.Google,
                DataFlag = (int)SourceFlag.Google,
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
            //also insert to subjob info
            ArchiverSiteMasterIndexDao.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract);
        }
        
        private int GetBackupFileType()
        {
            int backupFileType = (int)BackupFileType.DataBlock;
            if(archiverBackupJob.OutFileLevelBlock)
            {
                backupFileType = (int)BackupFileType.RecordsFile;
            }
            return backupFileType;
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

        public List<GoogleBasicIndex> GetIndexesNeedToCommit(FileType fileType)
        {
            var result = new List<GoogleBasicIndex>();
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

        public void HandleContentData(byte[] buffer, int offset, int dataSize)
        {
            try
            {
               this.outputStream.WriteContentData(buffer, offset, dataSize);
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Content Data. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void HandleData(byte[] buffer, int offset, int dataSize)
        {
            try
            {
               this.outputStream.WriteMetaData(buffer, offset, dataSize);
            }
            catch (Exception ex)
            {
                this.outputStream.WriteMetaData(buffer, offset, dataSize);
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Data. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void HandleHeader(string headerXml)
        {
            try
            {
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderBegin, "");
                var header = new GDriveFileHeader(headerXml);//todo
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderInfo, header.DataType, "");

                this.currentIndex = this.AssembleIndex(header);
                if (header.DataType == GDriveDataType.MyDrive || header.DataType == GDriveDataType.SharedDrive)
                {
                    this.InitResource(header);
                }
                this.outputStream.BeforeItem(this.currentIndex);
                this.outputStream.WriteHeaderXml(headerXml);
                this.indexes.Add(currentIndex);
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Header. Error:{ex.ToString()}");
                //throw new ArchiverBackupDataWriterException();
            }
        }
        private void InitResource(GDriveFileHeader header)
        {
            if (ArchiverIndexService == null)
            {
                ArchiverIndexService = new GDriveArchiverIndexService();
            }
            var indexServiceOpenParameter = new GDriveIndexServiceOpenParameter(this.archiverBackupJob, this.CacheManager.CacheSystem, this.indexLogicalDevice);
            this.ArchiverIndexService.Open(indexServiceOpenParameter);
            BackupIndexService = new GDriveArchiverBackupIndexService()
            {
                HeadAndBodyService = new GDriveArchiverHeadAndBodyIndexService() { IndexProcessor = ArchiverIndexService.IndexProcessor },
                SiteMasterService = new GDrvieMasterIndexService() { IndexProcessor = ArchiverIndexService.IndexProcessor }
            };
            this.isIndexOpen = true;
            //todo
            var serializedEncryptionInfo = this.EncryptionInfoManager.PutEncryptionInfo(this.archiverBackupJob.DataEncryptionInfoWrapper);
            this.BackupIndexService.UpdateJobInfoIndex(this.archiverBackupJob.JobId, ServiceConstants.EncryptionInfoKey, serializedEncryptionInfo);

            var openStreamParam = this.AssembleOutputStreamParameter();
            this.outputStream = OutputStreamFactory.GetOutputStream(openStreamParam);
            this.outputStream.Open();
            //todo
            var siteMasterIndex = new GDriveMasterIndex();
            
                siteMasterIndex.BackupTime = this.archiverBackupJob.ArchiverTime;
                siteMasterIndex.ID = Guid.NewGuid().ToString();
                siteMasterIndex.JobId = this.archiverBackupJob.JobId;
                siteMasterIndex.LogicalDrive = this.archiverBackupJob.IndexLogicalDevice.Id
                    + this.archiverBackupJob.LogicalDevice.Id;
                siteMasterIndex.PlanId = this.archiverBackupJob.PlanId;
                siteMasterIndex.MaxDataBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize;
                siteMasterIndex.DriveName = header.DriveName;
                siteMasterIndex.DriveId = header.DriveId;
                siteMasterIndex.TenantId = this.archiverBackupJob.TenantId;
            this.BackupIndexService.InsertSiteMaster(siteMasterIndex);
            this.InsertControlSiteMasterIndex(siteMasterIndex);
        }
        private OpenOutputStreamParameter AssembleOutputStreamParameter()
        {
            var outputDataListenerOpenParameter = new OutputDataListenerOpenParameter<GoogleBasicIndex>
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
            var outputDataListener = new OutputDataListener<GoogleBasicIndex>(outputDataListenerOpenParameter);
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

        private GoogleBasicIndex AssembleIndex(GDriveFileHeader fileHeader)
        {
            var index = CreateIndex(fileHeader.DataType);

            index.Id = Guid.NewGuid().ToString();
            index.Type = (int)fileHeader.DataType;
            index.Name = fileHeader.Name;
            index.Path = fileHeader.Path;
            index.PlanId = archiverBackupJob.PlanId;
            index.JobId = archiverBackupJob.JobId;
            index.CycleId = archiverBackupJob.JobId;
            if(fileHeader.DataType == GDriveDataType.FileVersion)
            {
                index.PathMD5 = $"{fileHeader.DriveId}/{fileHeader.ItemId}/{fileHeader.VersionNumberStr}".ToMD5HashCode();
                index.VersionNumber = fileHeader.VersionNumberStr;
            }
            else
            {
                index.PathMD5 = $"{fileHeader.DriveId}/{fileHeader.ItemId}".ToMD5HashCode();
            }
            
            index.ParentPathMD5 = $"{fileHeader.DriveId}/{fileHeader.ParentId}".ToMD5HashCode();
            index.BackupType = fileHeader.BackupType;
            index.ArchiveTime = archiverBackupJob.ArchiverTime;
            index.Sequence = sequence++;
            index.NodeType = fileHeader.NodeType;
            index.CreateTime = fileHeader.CreatedTime;
            index.ModifyTime = fileHeader.ModifiedTime;
            index.CreatedBy = fileHeader.CreatedBy;
            index.DriveId = fileHeader.DriveId;
            index.DriveName = fileHeader.DriveName;
            index.ItemId = fileHeader.ItemId;
            index.Flag = (byte)archiverBackupJob.DataMode;
            index.Retention = archiverBackupJob.RuleId;
            index.ParentId = fileHeader.ParentId;
            index.StoragePolicyId = archiverBackupJob.StoragePolicyName;
            if (archiverBackupJob.UseArchiveTier)
                SetAccessTierToFlagExtend(index, AccessTierType.Archive);
            //if (fileHeader.DataType == GDriveDataType.File || fileHeader.DataType == GDriveDataType.FileVersion)
            //{
            //    index.MemberEmail = fileHeader.MemberEmail;
            //}

            //index.IsRecoverable = IsRecoverable(fullPath) ? 1 : 0;
            return index;
        }
        private void SetAccessTierToFlagExtend(GoogleBasicIndex archiverBasicIndex, AccessTierType accessTier)
        {
            if (archiverBasicIndex.StorageAccessTierType < 0)
            {
                archiverBasicIndex.StorageAccessTierType = 0;
            }
            archiverBasicIndex.StorageAccessTierType = (archiverBasicIndex.StorageAccessTierType & ~0x3) | ((int)(accessTier));
        }
        private GoogleBasicIndex CreateIndex(GDriveDataType dataType)
        {
            switch (dataType)
            {
                case GDriveDataType.MyDrive:
                case GDriveDataType.SharedDrive:
                case GDriveDataType.Folder:
                    return new GoogleContainerIndex();

                case GDriveDataType.File:
                case GDriveDataType.FileVersion:
                    return new GoogleItemIndex();

                default:
                    throw new ArgumentException("Invalid data type.", dataType.ToString());
            }
        }
        private void CommitArchiveIndexes()
        {
            List<GoogleBasicIndex> indexesNeedToCommit = this.indexes.FindAll(index => index.HadHandleTail && index.HasContentIdMerged);
            if (indexesNeedToCommit.Count > 0)
            {
                this.BackupIndexService.InsertArchiveIndexes(indexesNeedToCommit);
                this.indexes.RemoveAll(index => index.HadHandleTail && index.HasContentIdMerged);
            }
        }
        private void AssembleIndexTail(GoogleBasicIndex currentIndex, FileTail tail)
        {
            List<string> attributes = tail.Attributes;
            StringBuilder stringBuilder = new StringBuilder(String.Empty);
            for (int i = 0; i < attributes.Count; i++)
            {
                stringBuilder.Append(attributes[i]).Append(ServiceConstants.ExtraChar);
            }
            currentIndex.Attributes = stringBuilder.ToString();
            currentIndex.Crc = tail.Crc32 == null ? 0 : (long)tail.Crc32;
        }
        public void HandleTail(string tailXml)
        {
            try
            {
                logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleTailHandle, tailXml);
                //var tail = FileHeaderUtil.ToFileTail(tailXml);
                //this.AssembleIndexTail(this.currentIndex, tail);
                StringBuilder tail = new StringBuilder();
                XmlElement xe = new XmlDocument().CreateElement("Attribute");
                tail.Append(xe.OuterXml);
                this.outputStream.EndItem(this.currentIndex);
                this.outputStream.WriteTailXml(tail.ToString());
                currentIndex.HadHandleTail = true;//!tail.IsFailed;
                currentIndex.HasWrittenContentData = true;
                currentIndex.HasWrittenMetaData = true;
                
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Tail. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }

        public void IncreaseMediaDataSize(long size)
        {
            this.totalDataSize += size;
        }

        public void Open(ArchiverBackupJob backupJob)
        {
            
        }
        public void OpenEXO(ExchangeBackupJob backupJob)
        {
        }
        public void OpenGDrive(GDriveBackupJob backupJob)
        {
            try
            {
                InitManager();
                this.logger.Info("MediaServiceArchiverBackupResource.ArchiverBackupDataWriterOpenBegin");
                this.archiverBackupJob = backupJob;
                this.dataLogicalDevice = this.StorageDeviceManager.OpenDataSystemForWrite(backupJob.LogicalDevice);
                this.indexLogicalDevice = this.StorageDeviceManager.Open(backupJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.ReadWrite);
                this.ValidateManager.Validate(new IndexDeviceValidateParameter(backupJob.JobId, backupJob.IndexVolume, ServiceConstants.IndexDBName, this.indexLogicalDevice, backupJob.IndexLogicalDevice));
                this.CacheManager.Open(this.archiverBackupJob.CacheSetting, this.dataLogicalDevice.IsDirectSystem, true);
                this.indexes = new List<GoogleBasicIndex>();
            }
            catch (IndexCanNotFoundException ie)
            {
                logger.Error($"Error occurred while open index device. Error:{ie.ToString()}");
            }
            catch (Exception e)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while open index device. Error:{e.ToString()}");
                //throw new ArchiverBackupDataWriterException();
            }
        }
        private void InitManager()
        {
            if (StorageDeviceManager == null)
            {
                StorageDeviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
            }
            if (CacheManager == null)
            {
                CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            }
            if (IndexSynchronizer == null)
            {
                IndexSynchronizer = PlatformWindsorManager.GetService<IIndexDatabaseSynchronizer>();
            }
            if (ValidateManager == null)
            {
                ValidateManager = PlatformWindsorManager.GetService<IIndexDeviceValidateManager>();
            }
        }
    }
}
