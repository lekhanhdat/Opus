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
namespace Media.Service.ArchiverBackup.LogicBackup
{
    public class EXOArchiverBackupDataWriter : ApplicationModelServiceBase
        , IArchiverBackupDataWriter
        , IOutputDataHandler<ExchangeBasicIndex>
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ExchangeBackupJob archiverBackupJob;
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        Boolean isOnlyHeader;
        Boolean isIndexOpen;
        Int64 sequence = 1;
        Int64 totalDataSize;
        String currentFolderName;
        String currentSiteCollection;
        String currentListBaseType;
        ExchangeBasicIndex currentIndex;
        List<String> fileHeaders;
        List<ExchangeBasicIndex> indexes;
        List<List<ExchangeBasicIndex>> cacheIndexList;
        IGeneralOutputStream outputStream;
        Dictionary<String, String> scopeIdDictionary = new Dictionary<String, String>();
        //private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        //private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IEncryptionInfoManager EncryptionInfoManager { get; set; }
        private IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        public IEXOArchiverBackupIndexService BackupIndexService { get; set; }

        public IIndexService<ExchangeIndexServiceOpenParameter> IndexService { get; set; }

        public EXOArchiverIndexService ArchiverIndexService { get; set; }
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
            //try
            //{
            //    IndexService.Close();
            //}
            //catch (Exception e)
            //{
            //    logger.Warn($"Error occurred while closing index device. Error:{e.ToString()}");
            //}
            this.StorageDeviceManager.Close(dataLogicalDevice);
            this.StorageDeviceManager.Close(indexLogicalDevice);
            this.indexes.Clear();
            this.cacheIndexList.Clear();
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
                    this.scopeIdDictionary.Clear();
                    this.indexes.Clear();
                    this.cacheIndexList.Clear();
                    this.ArchiverIndexService.Close();
                    this.ArchiverIndexService.UploadSubIndexToRealDevice();
                    EXOArhciverSubInfo.UpdateEXOSubInfoSizeBySubSubJobId(this.archiverBackupJob.JobId, this.totalDataSize);
                    //if (this.totalDataSize > 0)
                    //{
                    //    StorageDeviceService.UpdateLastArchivedTimeAsync(this.GetFirstOnlinePhysicalDriveId(), DateTime.UtcNow.Ticks);
                    //}
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

        public void Dispose()
        {
            throw new NotImplementedException();
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

        public List<ExchangeBasicIndex> GetIndexesNeedToCommit(FileType fileType)
        {
            var result = new List<ExchangeBasicIndex>();
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

        public void HandleData(byte[] buffer, int offset, int dataSize)
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

        public void HandleHeader(string headerXml)
        {
            try
            {
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderBegin, "");
                var header = new ExchangeFileHeader(headerXml);
                this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterHandleHeaderInfo, header.DataType, "");

                this.isOnlyHeader = false;
                this.currentIndex = this.AssembleIndex(header);
                if (header.DataType == ExchangeDataType.Mailbox)
                {
                    this.InitResource();
                }
                this.outputStream.BeforeItem(this.currentIndex);
                this.outputStream.WriteHeaderXml(headerXml);
                this.indexes.Add(currentIndex);
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Header. Error:{ex.ToString()}");
                throw new ArchiverBackupDataWriterException();
            }
        }
        private void InitResource()
        {
            if (ArchiverIndexService == null)
            {
                ArchiverIndexService = new EXOArchiverIndexService();
            }
            var indexServiceOpenParameter = new ExchangeIndexServiceOpenParameter(this.archiverBackupJob, this.CacheManager.CacheSystem, this.indexLogicalDevice);
            this.ArchiverIndexService.Open(indexServiceOpenParameter);
            BackupIndexService = new EXOArchiverBackupIndexService() { HeadAndBodyService = new EXOArchiverHeadAndBodyIndexService() { IndexProcessor = ArchiverIndexService.IndexProcessor } };
            this.isIndexOpen = true;
            //var serializedEncryptionInfo = this.EncryptionInfoManager.PutEncryptionInfo(this.archiverBackupJob.DataEncryptionInfoWrapper);
            //this.BackupIndexService.UpdateJobInfoIndex(this.archiverBackupJob.JobId, ServiceConstants.EncryptionInfoKey, serializedEncryptionInfo);

            OpenOutputStreamParameter openStreamParam = this.AssembleOutputStreamParameter();
            this.outputStream = OutputStreamFactory.GetOutputStream(openStreamParam);
            this.outputStream.Open();

            //ArchiverSiteMasterIndex siteMasterIndex = this.AssembleMasterIndex();
            //this.BackupIndexService.InsertSiteMaster(siteMasterIndex);
            //this.InsertControlSiteMasterIndex(siteMasterIndex);
        }
        private OpenOutputStreamParameter AssembleOutputStreamParameter()
        {
            var outputDataListenerOpenParameter = new OutputDataListenerOpenParameter<ExchangeBasicIndex>
            {
                CacheSystem = this.CacheManager.CacheSystem,
                DataLogicalDevice = this.dataLogicalDevice,
                DataVolume = this.archiverBackupJob.DataVolume,
                JobId = this.archiverBackupJob.JobId,
                OutputDataHandler = this,
                BackupJob = this.archiverBackupJob,
                MaxFileSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize,
            };
            //if (this.archiverBackupJob.UseArchiveTier)
            //{
            //    outputDataListenerOpenParameter.AccessTier = AccessTierType.Archive;
            //}
            var outputDataListener = new OutputDataListener<ExchangeBasicIndex>(outputDataListenerOpenParameter);
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
            //if (this.archiverBackupJob.OutFileLevelBlock)
            //{
            //    logger.Info("Backup job set OutputLevel to FileLevel");
                openStreamParam.OutputLevel = OutputStreamLevel.DataBlockLevel;
            //}
            return openStreamParam;
        }

        private ExchangeBasicIndex AssembleIndex(ExchangeFileHeader fileHeader)
        {
            var index = CreateIndex(fileHeader.DataType);
            var fullPath = fileHeader.DataType == ExchangeDataType.Mailbox ?
                fileHeader.Name :
                fileHeader.Path;
            var parentFullPath = fileHeader.ParentFullPath;
            //if (fileHeader.DataType == ExchangeDataType.Mailbox && !fileHeader.Name.EqualsIgnoreCase(EOBackupConfig.CurrentMailbox))
            //{ //用于开发阶段校验，暴露错误。
            //    logger.Error($"Context name [{EOBackupConfig.CurrentMailbox}] does not match to mailbox name [{fileHeader.Name}].");
            //    throw new Exception("Context name does not match to mailbox name.");
            //}
            //if (EOBackupConfig.CurrentUseObjectId)
            //{
            //    fullPath = ReplaceRootNameToObjectId(fullPath);
            //    parentFullPath = ReplaceRootNameToObjectId(parentFullPath);
            //}
            index.Id = Guid.NewGuid().ToString();
            index.Type = (int)fileHeader.DataType;
            index.Name = fileHeader.Name;
            index.Path = fullPath;
            //index.PlanId = config.exchangeBackupJob.PlanId;
            index.JobId = archiverBackupJob.JobId;
            //index.CycleId = config.exchangeBackupJob.CycleId;
            //index.CurrentJobId = config.exchangeBackupJob.ParentJobId;
            //index.JobType = config.exchangeBackupJob.JobId.Substring(0, 2);
            index.PathMD5 = fileHeader.Path.ToMD5HashCode();
            index.ParentPathMD5 = parentFullPath.ToMD5HashCode();
            index.BackupType = fileHeader.BackupType;
            index.BackupTime = archiverBackupJob.BackupTime;
            index.Sequence = sequence++;
            index.NodeType = fileHeader.NodeType;
            index.DisplayTo = fileHeader.DisplayTo;
            index.Sender = fileHeader.Sender;
            index.Category = fileHeader.Category;
            index.SendDate = fileHeader.SendDate;
            index.HasAttach = fileHeader.HasAttach;
            //index.Flag = (byte)config.DataSecurity;
            //index.IsRecoverable = IsRecoverable(fullPath) ? 1 : 0;
            return index;
        }
        private ExchangeBasicIndex CreateIndex(ExchangeDataType dataType)
        {
            switch (dataType)
            {
                case ExchangeDataType.Mailbox:
                case ExchangeDataType.Folder:
                case ExchangeDataType.Calendar:
                    return new ExchangeContainerIndex();

                case ExchangeDataType.Item:
                case ExchangeDataType.Post:
                case ExchangeDataType.Attachment:
                case ExchangeDataType.CalendarEvent:
                    return new ExchangeItemIndex();

                default:
                    throw new ArgumentException("Invalid data type.", dataType.ToString());
            }
        }
        private void CommitArchiveIndexes()
        {
            List<ExchangeBasicIndex> indexesNeedToCommit = this.indexes.FindAll(index => index.HadHandleTail && index.HasContentIdMerged);
            if (indexesNeedToCommit.Count > 0)
            {
                this.BackupIndexService.InsertArchiveIndexes(indexesNeedToCommit);
                this.indexes.RemoveAll(index => index.HadHandleTail && index.HasContentIdMerged);
            }
        }
        private void AssembleIndexTail(ExchangeBasicIndex currentIndex, FileTail tail)
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
                if (!this.isOnlyHeader)
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
        public void OpenGDrive(GDriveBackupJob backupJob)
        {

        }
        public void OpenEXO(ExchangeBackupJob backupJob)
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
                this.fileHeaders = new List<String>();
                this.indexes = new List<ExchangeBasicIndex>();
                this.cacheIndexList = new List<List<ExchangeBasicIndex>>();
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
