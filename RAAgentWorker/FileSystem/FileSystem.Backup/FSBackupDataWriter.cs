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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.IO;
using AvePoint.Media.Core.IO.Output;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.Wrapper.Common;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.FileSystem.Backup
{
    public class FSBackupDataWriter 
        : ApplicationModelServiceBase
        , IFSBackupDataWriter
        , IOutputDataHandler<ArchiverBasicIndex>
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        FSArchiverBackupJob archiverBackupJob;
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        Boolean isOnlyHeader;
        Boolean isIndexOpen;
        Int64 sequence = 1;
        Int64 totalDataSize;
        String currentFolderName;
        String currentSiteCollection;
        String currentListBaseType;
        bool internalShouldInit = true;
        ArchiverBasicIndex currentIndex;
        List<String> fileHeaders;
        List<ArchiverBasicIndex> indexes;
        List<List<ArchiverBasicIndex>> cacheIndexList;
        IGeneralOutputStream outputStream;
        Dictionary<String, String> scopeIdDictionary = new Dictionary<String, String>();
        public static readonly String IndexDBName = "index.db";
        //private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        //private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();
        //private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IEncryptionInfoManager EncryptionInfoManager = new EncryptionInfoManager();

        public IArchiverBackupIndexService BackupIndexService = new ArchiverBackupIndexService();

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService = new ArchiverIndexService();

        public ArchiverIndexService ArchiverIndexService = new ArchiverIndexService();
        public void Close()
        {
            this.ReleaseResourceAsync().Wait();
            try
            {
                this.CacheManager.Clear(this.archiverBackupJob.DataVolume, this.archiverBackupJob.JobId, -1);
            }
            catch (System.Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                this.logger.Warn("CacheManager.Clear() has error");
                //this.logger.Warn(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderCloseClearCacheError, ex.ToString());
                //info.ErrorMessage += ex.ToString();
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
                //info.ErrorMessage += ex.ToString();
                throw;
            }
            try
            {
                //IndexService.Close();
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while closing index device. Error:{e.ToString()}");
            }
            this.StorageDeviceManager.Close(dataLogicalDevice);
            this.StorageDeviceManager.Close(indexLogicalDevice);
            this.indexes.Clear();
            this.cacheIndexList.Clear();
            this.logger.Info("ArchiverBackupDataWriter Close End");
        }

        public ArchiverBasicIndex GetArchiverIndex(string md5)
        {
            throw new NotImplementedException();
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
                throw new Exception("ArchiverBackupDataWriterException");
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
                throw new Exception("ArchiverBackupDataWriterException");
            }
        }

        public void HandleHeader(string headerXml)
        {
            try
            {
                var header = new MediaArchiverFileHeader(headerXml);
                this.isOnlyHeader = false;
                this.currentIndex = this.AssembleIndexHeader(header);
                if (internalShouldInit)
                {
                    this.InitResource();
                    internalShouldInit = false;
                }
                this.outputStream.BeforeItem(this.currentIndex);
                this.outputStream.WriteHeaderXml(headerXml);
                this.indexes.Add(currentIndex);

            }
            catch (Exception ex)
            {
                //WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Header. Error:{ex.ToString()}");
                internalShouldInit = false;
                throw;
            }
        }
        private async System.Threading.Tasks.Task ReleaseResourceAsync()
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
                //info.BackupStatus = BackupStatus.ExceptionOccurred;
                //info.ErrorMessage += ex.ToString();
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
                    HybridApiClient.Instance.UpdateSiteMasterMediaDataSize(this.archiverBackupJob.JobId, this.totalDataSize);
                    if (this.totalDataSize > 0)
                    {
                        HybridApiClient.Instance.UpdateLastArchivedTime(this.archiverBackupJob.StoragePolicyId);
                    }
                    this.isIndexOpen = false;
                }
            }
            catch (Exception ex)
            {
                //info.BackupStatus = BackupStatus.ExceptionOccurred;
                //info.ErrorMessage += ex.ToString();
                logger.Error("An error occourred while uploading subindex to real device, details:{0}", ex);
                throw;
            }
            this.currentSiteCollection = null;
        }
        private String GetFirstOnlinePhysicalDriveId()
        {
            string physicalDeviceId = (this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID;
            this.logger.Info($"ArchiverBackupDataWriter Get First Online Physical DriveId:{physicalDeviceId}");
            return physicalDeviceId;
        }
        private ArchiverBasicIndex AssembleIndexHeader(MediaArchiverFileHeader header)
        {
            ArchiverBasicIndex basicIndex;
            string fullPath = string.Empty;
            string parentPath = string.Empty;
            this.currentSiteCollection = this.archiverBackupJob.ConnectionName;
            switch (header.Type)
            {
                case AveSharePointType.TYPE_SITE:
                    basicIndex = new ArchiverHeadIndex();
                    //if mapping(SharePoint) ,use site url before mapping

                    parentPath = string.Empty;
                    fullPath = this.currentSiteCollection;
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

                    fullPath = header.Url;
                    basicIndex.CreateTime = header.CreateTime;
                    basicIndex.ModifyTime = header.ModifyTime;
                    basicIndex.Editor = header.Editor;
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
                    throw new NotSupportedException(String.Format("not surpport type:", header.Type));
            }
            if (fullPath == null || parentPath == null)
            {
                throw new ArgumentNullException("fullPath or parentPath is null");
            }
            basicIndex.Id = Guid.NewGuid().ToString();
            basicIndex.Flag = this.archiverBackupJob.DataMode;
            basicIndex.Type = Convert.ToChar(header.Type).ToString();
            basicIndex.Name = header.Type.Equals(AveSharePointType.TYPE_SITE) ? this.currentSiteCollection : header.Path;
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

            if (header.IsAppData && this.currentIndex.Type.Equals("W", StringComparison.OrdinalIgnoreCase))
            {
                this.currentIndex.Type = "P";
            }
            //basicIndex.SitePath = this.currentSiteCollection;
            basicIndex.ArchiveTime = this.archiverBackupJob.ArchiveTime;
            basicIndex.ListType = header.IsMyProfileList;
            basicIndex.PlanId = this.archiverBackupJob.PlanId;
            //basicIndex.DedupSourceFileJobId = this.archiverBackupJob.JobId;
            basicIndex.JobId = this.archiverBackupJob.JobId;
            basicIndex.Sequence = sequence++;
            basicIndex.StoragePolicyId = this.archiverBackupJob.StoragePolicyId;
            //basicIndex.Url = header.Url;
            basicIndex.ExtraInfo = header.HeaderExtraAttribute;
            //if (archiverBackupJob.UseArchiveTier)
            //    SetAccessTierToFlagExtend(basicIndex, AccessTierType.Archive);
            //basicIndex.Attributes = FSJobCache.Instance.RootPath;
            basicIndex.ExtraInfo = header.Extra;
            //basicIndex.Retention = archiverBackupJob.RuleId;
            string tempExtraInfo = string.IsNullOrEmpty(basicIndex.ExtraInfo) ? "" : basicIndex.ExtraInfo + "\\";
            string tempLocation = FSJobCache.Instance.RootPath + "\\" + tempExtraInfo + basicIndex.Name;
            basicIndex.PathMD5 = tempLocation.ToMD5HashCode();
            return basicIndex;
        }
        private void InitResource()
        {

            if (internalShouldInit)
            {

                //var serializedEncryptionInfo = this.EncryptionInfoManager.PutEncryptionInfo(this.archiverBackupJob.DataEncryptionInfoWrapper);
                ArchiverSiteMasterIndex siteMasterIndex = this.AssembleMasterIndex();
                this.InsertControlSiteMasterIndex(siteMasterIndex);
            }
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter(this.archiverBackupJob, this.CacheManager.CacheSystem, this.indexLogicalDevice, HybridApiClient.Instance.GetDBSEEMasterKey());
            this.ArchiverIndexService.Open(indexServiceOpenParameter);
            this.isIndexOpen = true;
            this.BackupIndexService.InitIndexProcesser(ArchiverIndexService);
            if (internalShouldInit)
            {
                ArchiveIndexInfo fsMasterIndexInfo = this.AssembleFSMasterIndexInfo();
                this.BackupIndexService.InsertSiteMaster(fsMasterIndexInfo);
            }
            OpenOutputStreamParameter openStreamParam = this.AssembleOutputStreamParameter();
            this.outputStream = OutputStreamFactory.GetOutputStream(openStreamParam);
            this.outputStream.Open();
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
                //siteMasterIndex.MaxDataBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize;
            }
            return siteMasterIndex;
        }
        private ArchiveIndexInfo AssembleFSMasterIndexInfo()
        {
            ArchiveIndexInfo siteMasterIndex = new ArchiveIndexInfo();
            if (this.currentSiteCollection != null)
            {
                siteMasterIndex.ArchiveTime = this.archiverBackupJob.ArchiveTime;
                siteMasterIndex.Guid = Guid.NewGuid().ToString();
                siteMasterIndex.JobId = this.archiverBackupJob.JobId;
                siteMasterIndex.UNCPath = FSJobCache.Instance.RootPath;
                siteMasterIndex.ConnectionPath = FSJobCache.Instance.ConnectionPath;
                siteMasterIndex.ConnectionId = this.currentSiteCollection;
                //siteMasterIndex.MaxDataBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize;
            }
            return siteMasterIndex;
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
                //MaxFileSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize,
            };
            //if (this.archiverBackupJob.UseArchiveTier)
            //{
            //    outputDataListenerOpenParameter.AccessTier = AccessTierType.Archive;
            //}
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
            //openStreamParam.MaxBlockSize = MediaConfigInfo.ArchiverConfigInfo.MaxDataFileSize * IOConstants.MB;
            if (this.archiverBackupJob.OutFileLevelBlock)
            {
                logger.Info("Backup job set OutputLevel to FileLevel");
                openStreamParam.OutputLevel = OutputStreamLevel.FileLevel;
            }
            return openStreamParam;
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
            string connectionName = HybridApiClient.Instance.GetConnectionNameById(this.archiverBackupJob.ConnectionId);
            logger.Info($"this fs archive job connection name is:{connectionName.LogBase64()}");
            var archiverSiteMasterIndexContract = new FSMasterIndexContract
            {
                JobId = siteMasterIndex.JobId.Substring(0, siteMasterIndex.JobId.LastIndexOf('_')),
                Id = Guid.NewGuid().ToString(),
                ArchiverTime = siteMasterIndex.BackupTime,
                FarmName = siteMasterIndex.FarmName,
                FarmId = this.archiverBackupJob.FarmId,
                IndexDeviceId = this.archiverBackupJob.IndexLogicalDevice.Id,
                ConnectionName = string.IsNullOrEmpty(connectionName)? this.archiverBackupJob.ConnectionId: connectionName,
                ConnectionId = this.archiverBackupJob.ConnectionId,
                JobState = 0,
                StoragePolicyId = this.archiverBackupJob.StoragePolicyId,
                SPVersion = this.archiverBackupJob.SpVersion,
                SubInfo = archiverIndexSubInfoList,
                RuleId = archiverBackupJob.RuleId,
                SourceFlag = archiverBackupJob.SourceFlag,
                AgentId = TenantAgentInfo.AgentId,
                //VersionDetails = new VersionDetails()
                //{
                //    PlatformType = AvePoint.GCommon.Contract.Media.Object.PlatformType.DocAve,
                //    ProductVersion = ProductVersion.Product6X,
                //    LastImportedTime = 0
                //},
                Extension = new ArchiverSiteMasterIndexExtension()
                {
                    UpdateTime = DateTime.UtcNow.Ticks,
                },
                //BackupFileType = GetBackupFileType(),
            };
            //this.ControlStubs.ArchiverBackupService.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract, IdentityManager.IdentityContent);
            //this.ControlStubs.ArchiverGeneralServiceSiteMasterIndexService.InsertIntoArchiverSiteMasterIndex(archiverSiteMasterIndexContract);
            HybridApiClient.Instance.InsertIntoFSMasterIndex(archiverSiteMasterIndexContract);
        }
        public void HandleTail(string tailXml)
        {
            try
            {
                if (!this.isOnlyHeader)
                {
                    logger.Info($"MediaService ArchiverBackupResource.ArchiverBackupDataWriterHandleTailHandle:{tailXml}");
                    //var tail = FileHeaderUtil.ToFileTail(tailXml);
                    //this.AssembleIndexTail(this.currentIndex, tail);
                    this.outputStream.EndItem(this.currentIndex);
                    this.outputStream.WriteTailXml(tailXml);
                    //currentIndex.HadHandleTail = !tail.IsFailed;
                }
            }
            catch (Exception ex)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while Handle Tail. Error:{ex.ToString()}");
                throw new Exception("ArchiverBackupDataWriterException");
            }
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
            currentIndex.IsFailed = tail.IsFailed.ToString();
        }
        public void Open(FSArchiverBackupJob backupJob)
        {
            try
            {
                //this.logger.Info(LOGRESOURCE.MediaServiceArchiverBackupResource.ArchiverBackupDataWriterOpenBegin);
                this.archiverBackupJob = backupJob;
                this.dataLogicalDevice = this.StorageDeviceManager.OpenDataSystemForWrite(backupJob.DataLogicalDevice, archiverBackupJob.UseSnapLock);
                this.indexLogicalDevice = this.StorageDeviceManager.Open(backupJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.ReadWrite);
                this.ValidateManager.Validate(new IndexDeviceValidateParameter(backupJob.JobId, backupJob.IndexVolume, IndexDBName, this.indexLogicalDevice, backupJob.IndexLogicalDevice));
                this.CacheManager.Open(this.archiverBackupJob.CacheSetting, BackgroundSettings.GetInstance().ArchiveCache, this.dataLogicalDevice.IsDirectSystem, true);
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
                    indexOpenParam.DBPassWord = HybridApiClient.Instance.GetDBSEEMasterKey();
                    //IndexService.Open(indexOpenParam);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while open IndexService. Error:{ex}");
                }
            }
            catch (Exception e)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                logger.Error($"Error occurred while open index device. Error:{e.ToString()}");
                throw;
            }
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

        public void AfterDataBlockCommit(FileType fileType, StorageResult storageResult, bool closing, long backupDataSize)
        {
            this.totalDataSize += backupDataSize;
            if (fileType == FileType.Content)
            {
                this.indexes.ForEach(index => index.HasContentIdMerged = index.HasWrittenContentData);
            }
            if (fileType == FileType.MetaData)// && storageResult.NeedCommit)
            {
                this.CommitArchiveIndexes();
            }
        }
        private void CommitArchiveIndexes()
        {
            //List<ArchiverBasicIndex> indexesNeedToCommit = this.indexes.FindAll(index => index.HadHandleTail && index.HasContentIdMerged);
            //if (indexesNeedToCommit.Count > 0)
            //{
                this.BackupIndexService.InsertArchiveIndexes(this.indexes);
                this.indexes.Clear();
            //}
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
                throw new Exception(String.Format($"MediaService ArchiverBackup Resource.ArchiverBackupDataWriter GetDataFileName UnknownFileTypeException:{fileType.ToString()}"));
            }
            return fileName;
        }

        public void IncreaseMediaDataSize(long size)
        {
            this.totalDataSize += size;
        }

        public string GetJobId()
        {
            return this.archiverBackupJob.JobId;
        }

        public string GetConnectionId()
        {
            return this.archiverBackupJob.ConnectionId;
        }

        public string GetConnectionName()
        {
            return this.archiverBackupJob.ConnectionName;
        }
    }
}
