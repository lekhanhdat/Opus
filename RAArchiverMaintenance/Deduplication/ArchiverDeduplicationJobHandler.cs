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
using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Index;
using RAArchiverCommon;
using RAArchiverCommon.TeamsController;
using Storage;
using System.IO;
using System.Xml;
using static AvePoint.RA.DB.DBLocker.SampleDBLocker;
using RMJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace RAArchiverMaintenance.Deduplication
{
    public class ArchiverDeduplicationJobHandler
    {
        private IRALogger logger = new RALogger(typeof(ArchiverDeduplicationJobHandler));
        private static readonly int indexLimit = ServiceConstants.MergeIndexLimit;

        private IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private ICacheService CacheManager = PlatformWindsorManager.GetService<ICacheService>();
        private IStorageDeviceManager StorageDeviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private string subJobId;
        private DeduplicationSiteData jobData;
        private RMJobStatus jobStatus = RMJobStatus.InProgress;
        private string summaryComment = string.Empty;
        private bool hasUnexpectedException = false;
        private bool hasNewDupFiles = false;
        private bool hasErrorDedupFile = false;
        private bool hasDedupSuccessedFile = false;
        private bool disablePerformanceMonitor = false;
        private bool hasPendingDeletedDuplicatedFiles = false;
        private bool isDisableRetentionPeriodLimitation = false;
        private long retentionPeriodLimitTicks = DateTime.UtcNow.AddDays(-90).Ticks;
        private long minDedupTime = -1;
        private long maxDedupTime = -1;
        private IXSystem indexLogicalDevice;
        private IVolumeGenerator volumeGenerator = new VolumeGeneratorFactory().GetVolumeGenerator(ProductModule.ArchiverBackup);
        private string dataVolume;
        private string indexVolume;
        private CacheSettingDto cacheSetting;
        private Dictionary<string, string> storageDeviceIDs = new Dictionary<string, string>(); // key is archiver sub job id
        private Dictionary<string, StorageDeviceDto> storageDeviceInfoes = new Dictionary<string, StorageDeviceDto>();
        private Dictionary<string, IXSystem> dataLogicalDevices = new Dictionary<string, IXSystem>();
        private HashSet<string> changedSubIndexes = new HashSet<string>();
        private ActionStatistics reportStatistics = new ActionStatistics();

        private ArchiverDeduplicationService DeduplicationService = new ArchiverDeduplicationService();

        private IIndexDatabaseSynchronizer IndexSynchronizer = PlatformWindsorManager.GetService<IIndexDatabaseSynchronizer>();
        //主Index IIndexProcessor,用来操作本地Download的Index
        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
        //临时Index，用来存储要De-dup的数据,IIndexProcessor,用来操作本地Download的Index
        public IIndexProcessor<ArchiverDedupIndexProcessorParameter> DedupIndexProcessor = new IndexProcessor<ArchiverDedupIndexProcessorParameter>();

        private Dictionary<string, IIndexProcessor<ArchiverIndexProcessorParameter>> SubIndexProcessors = new Dictionary<string, IIndexProcessor<ArchiverIndexProcessorParameter>>(); // key is archiver sub job id

        private class SqliteIndexDBQuery
        {
            // Dudup Job Query main/sub index db
            public static readonly string SelectAllDuplicateCRC = "SELECT COL_EXTENSION_8 FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_FILE_HEADER_TYPE > 0 GROUP BY COL_EXTENSION_8 HAVING COUNT(*)>1";
            public static readonly string SelectDuplicateFilesByCRCs = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_8 IN ";
            public static readonly string SelectArchiverBodyByCRC = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_8 = @CRC;";
            public static readonly string UpdateFileIndexDedupInfoById = $"UPDATE {IndexConstants.TableNameArchiveBody} SET COL_CONTENT_DATA_FILE_NUMBER = @ContentDataFileNumber, COL_STORAGEINFO = @StorageInfo, COL_BLOB_INFO = @DedupSourceFileId, COL_POOL_GUID = @DedupExtension, COL_EXTENSION_3 = @DuplicateStatus, COL_DEL_STATUS = @DelStatus WHERE COL_ID = @COL_ID ";

            // Query Dedup Index DB
            public static readonly string SelectAllDeletingFilesCount = $"SELECT COUNT(*) FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 0;";
            public static readonly string SelectAllDeletingFiles = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 0 LIMIT @OFFSET, @LENGTH;";
            public static readonly string SelectAllDedupFilesCount = $"SELECT COUNT(*) FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 1 AND COL_RECYCLE_TIME > @DedupFrom AND COL_RECYCLE_TIME <= @DedupTo;";
            public static readonly string SelectAllDedupFiles = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 1 AND COL_RECYCLE_TIME > @DedupFrom AND COL_RECYCLE_TIME <= @DedupTo LIMIT @OFFSET, @LENGTH;";
            public static readonly string SelectOneDedupFileId = $"SELECT COL_ID FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 1;";
            public static readonly string SelectDedupFilesBySourceFileId = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_BLOB_INFO = @SourceFileId;";
            public static readonly string DeleteExistsFilesByIDs = $"DELETE FROM {IndexConstants.TableNameArchiveBody} WHERE COL_ID IN ";
            public static readonly string UpdateDeletedDedupFileById = $"UPDATE {IndexConstants.TableNameArchiveBody} SET COL_DEL_STATUS = 1, COL_RECYCLE_TIME = @DedupTime WHERE COL_ID = @COL_ID ";
        }

        public ArchiverDeduplicationJobHandler(string jobId, JobType jobType)
        {
            this.subJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            ReportManager.StartUpdateJobProgress();

            try
            {
                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
                this.jobData = GetJobData();
            }
            catch (Exception e)
            {
                logger.Error($"Create Archiver Dedup Service Failed. {e}");
                throw;
            }
        }

        public async Task RunAsync()
        {
            try
            {
                var siteId = GetSiteId();
                using var indexDbLocker = await SampleDBLocker.Get4IndexDBUpdater(
                     this.jobData.SiteCollectionURL, siteId, this.subJobId, TimeSpan.FromHours(1)
                );

                Init();
                if (!(await CheckJobIsStoppingAsync()))
                {
                    RealDeuplicate();
                    SetSiteCollectionDedupInfo();
                }
            }
            catch (SampleDBLockerTimeoutException toEx)
            {
                this.summaryComment = $"The sub job conflicts with other running jobs on the site: {this.jobData.SiteCollectionURL}. ";
                logger.Error($"An timeout error occurred while deduplicating, {toEx}");
            }
            catch (Exception e)
            {
                this.hasUnexpectedException = true;
                logger.Error($"An error occurred while deduplicating, {e}");
            }

            this.MoveToNextJobStage(90, 95);

            try
            {
                this.PreSetJobCompletedStatus();
                this.GenerateJobReport();
            }
            catch (Exception e)
            {
                this.hasUnexpectedException = true;
                logger.Error($"Generate job report or update job status fails. {e}");
                this.PreSetJobCompletedStatus();
            }

            try
            {
                this.UpdateJobCompletedStatus();
            }
            catch (Exception ex)
            {
                logger.Error($"Update job completed status fails. {ex}");
            }

            this.DisposeObj();
        }

        private DeduplicationSiteData GetJobData()
        {
            IRMSubJobDao SubJobDao = new RMSubJobDao();
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
            logger.Info($"Dedup job data : {subJobWithContext.JobContext?.Settings}");
            return SerializerHelper.DeserializeByJsonConvert<DeduplicationSiteData>(subJobWithContext.JobContext?.Settings);
        }

        private void Init()
        {
            AvePerformanceTimerPool.SetDisable(disablePerformanceMonitor);
            AvePerformanceMonitor.SetDisable(disablePerformanceMonitor);
            using (AvePerformanceScope pc = new AvePerformanceScope("Deduplication.Open"))
            {
                logger.Info($"Begin deduplicating file level data, site url is {this.jobData.SiteCollectionURL}");

                this.MoveToNextJobStage(1, 5);

                this.isDisableRetentionPeriodLimitation = StorageDeviceService.IsDisableRetentionPeriodLimitation();
                logger.Info($"Is DisableRetentionPeriodLimitation: {isDisableRetentionPeriodLimitation}");

                this.dataVolume = this.volumeGenerator.GenerateDataVolume(new VolumeParameter() { SiteCollectionUrl = this.jobData.SiteCollectionURL, FarmName = "" });
                this.indexVolume = this.volumeGenerator.GenerateIndexVolume(new VolumeParameter() { SiteCollectionUrl = this.jobData.SiteCollectionURL, FarmName = "" });
                logger.Info($"Begin opening IndexLogicalDevice.");
                var indexStroage = StorageDeviceService.GetIndexDevice();
                if (indexStroage == null)
                {
                    throw new Exception("Cannot find index Storage Device.");
                }
                this.cacheSetting = GetCacheSetting();
                var indexLogicalDeviceDto = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexStroage);
                this.indexLogicalDevice = this.StorageDeviceManager.Open(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
                this.CacheManager.Open(this.cacheSetting, false, true);
                logger.Info($"Opened indexLogicalDevice successfully.");
                this.MoveToNextJobStage(5, 10);

                this.OpenMainIndex();
                this.MoveToNextJobStage(10, 15);

                this.OpenDedupFileIndex();
                this.MoveToNextJobStage(15, 20);
            }
        }

        private string GetSiteId()
        {
            foreach (var masterIndexId in this.jobData.ArchiverSiteMasterIndexIds)
            {
                var siteId = ArchiverSiteMasterIndexDao.GetSiteId(masterIndexId);
                if (siteId != null)
                {
                    return siteId;
                }
            }
            throw new Exception($"Could not get site id by ArchiverSiteMasterIndexIds");
        }


        private void RealDeuplicate()
        {
            var duplicateCRCs = QueryAllDuplicateCRC();
            logger.Info($"Deuplicate CRC Total: {duplicateCRCs.Count}");
            this.MoveToNextJobStage(20, 25, 20);
            int processedCount = 0;
            DatabaseUtility.BatchOperation(duplicateCRCs, (batchCRCs) =>
            {
                try
                {
                    ProcessDuplicateData(batchCRCs);
                    processedCount += batchCRCs.Count();
                    logger.Info($"Deuplicate CRCs progress: {processedCount}/{duplicateCRCs.Count}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while deduping for CRCs: {string.Join(",", batchCRCs)}. {ex}");
                }

                this.UpdateCurrentStageProgress(duplicateCRCs.Count, processedCount);
            }, 200);

            if (hasNewDupFiles)
            {
                UploadChangedSubIndexToAzure();
                this.MoveToNextJobStage(45, 50);

                UploadMainIndexToAzure();
                this.MoveToNextJobStage(50, 55);

                UploadDedupIndexToAzure();
            }
            this.MoveToNextJobStage(55, 60, 20);

            DeleteDuplicateFiles();

            if (this.hasDedupSuccessedFile)
            {
                UploadDedupIndexToAzure();
            }
            this.MoveToNextJobStage(85, 90);
        }

        private void ProcessDuplicateData(IEnumerable<string> dupCRCs)
        {
            dupCRCs = dupCRCs.Where(crc =>
            {
                if (string.IsNullOrEmpty(crc))
                {
                    logger.Warn($"Skip process empty CRC.");
                    return false;
                }
                return true;
            });
            if (dupCRCs.Count() == 0)
            {
                logger.Warn($"No duplicate data.");
                return;
            }
            var fileIndexes = QueryDuplicateFileIndexesByCRCs(dupCRCs);
            var newDupFileIndexes = new List<ArchiverBodyIndex>();
            foreach (var indexesWithSameCrc in fileIndexes.GroupBy(i => i.StorageCrc64))
            {
                string oldSourceFileId = null;
                var dedupTime = DateTime.UtcNow.Ticks;
                var dupStatusFileIndexes = new List<ArchiverBodyIndex>();
                var notDupStatusFileIndexes = new List<ArchiverBodyIndex>();

                foreach (var fileIndex in indexesWithSameCrc)
                {
                    if (fileIndex.DuplicateStatus > (int)IndexDeduplicateFileStatus.None)
                    {
                        oldSourceFileId = fileIndex.DedupSourceFileId;
                        dupStatusFileIndexes.Add(fileIndex);
                    }
                    else
                    {
                        notDupStatusFileIndexes.Add(fileIndex);
                    }
                }

                if (notDupStatusFileIndexes.Any())
                {
                    logger.Info($"There are {indexesWithSameCrc.Count()} with same CRC: {indexesWithSameCrc.Key}. Need dedup file count: {notDupStatusFileIndexes.Count}");
                    var dupSourceFileIndex = indexesWithSameCrc.OrderByDescending(i => i.ArchiveTime).First();
                    if (dupSourceFileIndex.DuplicateStatus > 0)
                    {
                        logger.Info($"Still use source file: {dupSourceFileIndex.Id}");
                    }
                    else
                    {
                        logger.Info($"Set as {(dupStatusFileIndexes.Any() ? "new " : "")}source file: {dupSourceFileIndex.Id}");
                    }

                    foreach (var fileIndex in notDupStatusFileIndexes)
                    {
                        var isSourceFile = dupSourceFileIndex.Id == fileIndex.Id;
                        fileIndex.DelStatus = isSourceFile ? (int)IndexDedupFileDeleteStatus.NotNeedDelete : (int)IndexDedupFileDeleteStatus.WaitToDelete;
                        fileIndex.DuplicateStatus = isSourceFile ? (int)IndexDeduplicateFileStatus.SourceFile : (int)IndexDeduplicateFileStatus.DuplicateFile;
                        fileIndex.DedupSourceFileId = dupSourceFileIndex.Id;
                        fileIndex.DedupExtension = GetNewDedupExtensionInfo(fileIndex, dupSourceFileIndex);

                        fileIndex.ContentDataFileNumber = dupSourceFileIndex.ContentDataFileNumber;
                        fileIndex.StorageInfo = dupSourceFileIndex.StorageInfo;
                    }
                    // 将新增的Duplicate的file插入到 Dedup Index DB里
                    InsertToDedupIndexFile(notDupStatusFileIndexes);

                    // 当新增的Duplicate File 的 Archive Time 更靠后时，会切换它为新的Source File
                    // 需要把 Dedup Index DB里将oldSourceFileId作为Source File的 File都做Update
                    if (!string.IsNullOrEmpty(oldSourceFileId))
                    {
                        // 如果 当前CRC对应的File都是未做过Dedup的File，则表示之前从未有过同样CRC的file，又或者之前有过，但是被Dedup后，又被Retention掉了；
                        // 后者的情况下，Dedup Index里会存在同样的CRC，对应多个Source File的情况。但这个并不影响Dedup的正确性。
                        var filesRefOldSourceFile = GetDedupFileIndexesBySourceFileId(oldSourceFileId);
                        foreach (var item in filesRefOldSourceFile)
                        {
                            if (item.Id == oldSourceFileId)
                            {
                                // 旧的Source File需要更新为 DuplicateFile，且待删除状态
                                item.DelStatus = (int)IndexDedupFileDeleteStatus.WaitToDelete;
                                item.DuplicateStatus = (int)IndexDeduplicateFileStatus.DuplicateFile;
                            }
                            item.DedupSourceFileId = dupSourceFileIndex.Id;
                            item.DedupExtension = GetNewDedupExtensionInfo(item, dupSourceFileIndex);
                            item.ContentDataFileNumber = dupSourceFileIndex.ContentDataFileNumber;
                            item.StorageInfo = dupSourceFileIndex.StorageInfo;

                            if (dupStatusFileIndexes.Any(i => i.Id == item.Id))
                            {
                                notDupStatusFileIndexes.Add(item);
                            }

                            UpdateFileInDedupIndexDB(item);
                        }
                    }

                    UpdateDuplicateFilesInSubIndexDB(notDupStatusFileIndexes);
                    UpdateFileDedupInfoToMainIndexDB(notDupStatusFileIndexes);

                    this.hasNewDupFiles = true;
                }
            }
        }

        private string GetNewDedupExtensionInfo(ArchiverBodyIndex fileInfo, ArchiverBodyIndex sourceFileInfo)
        {
            DedupExtensionInfo dedupExtInfo = null;
            if (string.IsNullOrEmpty(fileInfo.DedupExtension))
            {
                dedupExtInfo = new DedupExtensionInfo()
                {
                    DedupJobId = this.subJobId,
                    SourceFileContentLength = sourceFileInfo.ContentLength,
                    SourceFileStoragePolicyId = sourceFileInfo.StoragePolicyId,
                    DedupSourceFileJobId = sourceFileInfo.JobId,
                    DedupSourceFileFlag = sourceFileInfo.Flag,
                    DuplicateFileNumber = fileInfo.ContentDataFileNumber,
                    DuplicateFileStorageInfo = fileInfo.StorageInfo,
                };
            }
            else
            {
                try
                {
                    dedupExtInfo = SerializerHelper.DeserializeByDataContractJsonSerializer<DedupExtensionInfo>(fileInfo.DedupExtension);
                    dedupExtInfo.DedupJobId = $"{dedupExtInfo.DedupJobId},{this.subJobId}";
                    dedupExtInfo.SourceFileContentLength = sourceFileInfo.ContentLength;
                    dedupExtInfo.SourceFileStoragePolicyId = sourceFileInfo.StoragePolicyId;
                    dedupExtInfo.DedupSourceFileJobId = sourceFileInfo.JobId;
                    dedupExtInfo.DedupSourceFileFlag = sourceFileInfo.Flag;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Deserialize dedup extension fail. ExtStr: {fileInfo.DedupExtension}. Error: {ex}");
                    dedupExtInfo = new DedupExtensionInfo()
                    {
                        DedupJobId = this.subJobId,
                        SourceFileContentLength = sourceFileInfo.ContentLength,
                        SourceFileStoragePolicyId = sourceFileInfo.StoragePolicyId,
                        DedupSourceFileJobId = sourceFileInfo.JobId,
                        DedupSourceFileFlag = sourceFileInfo.Flag,
                        DuplicateFileNumber = fileInfo.ContentDataFileNumber,
                        DuplicateFileStorageInfo = fileInfo.StorageInfo,
                    };
                }
            }
            return SerializerHelper.SerializeByDataContractJsonSerializer(dedupExtInfo);
        }

        private void UpdateDuplicateFilesInSubIndexDB(List<ArchiverBodyIndex> indexes)
        {
            foreach (var group in indexes.GroupBy(i => i.JobId))
            {
                var archiverSubJobId = group.Key;
                this.changedSubIndexes.Add(archiverSubJobId);

                logger.Info($"Update duplicate files in sub index db. ArchiverSubJobId: {archiverSubJobId}. IDs: {string.Join(",", indexes.Select(i => i.Id))}");
                var subIndexProcessor = GetSubIndexProcessor(archiverSubJobId);
                foreach (var dupFileInfo in group)
                {
                    UpdateFileDedupInfoToSubIndexDB(subIndexProcessor, dupFileInfo);
                }
            }
        }

        private void DeleteDuplicateFiles()
        {
            logger.Info($"Start delete duplicate files");
            var total = GetDeletingDedupFileIndexesCount();
            logger.Info($"There are deleting duplicate files: {total}");
            if (total == 0)
            {
                this.MoveToNextJobStage(75, 80);
                return;
            }

            int completedCount = 0;
            int failedCount = 0;
            for (int offset = 0; offset < total; offset += indexLimit)
            {
                var deletingFiles = GetDeletingDedupFileIndexes(offset, indexLimit);

                DatabaseUtility.BatchOperation(deletingFiles, (batchDelFiles) =>
                {
                    if (CheckJobIsStoppingAsync().GetAwaiter().GetResult())
                    {
                        this.MoveToNextJobStage(75, 80);
                        return;
                    }

                    Dictionary<string, long> deletedSizeMapping = new Dictionary<string, long>();
                    foreach (var fileInfo in batchDelFiles)
                    {
                        completedCount++;
                        this.UpdateCurrentStageProgress(total, completedCount);

                        try
                        {
                            var storageId = GetStorageDeviceIdByArchiverJobId(fileInfo.JobId);
                            if(!string.IsNullOrEmpty(storageId))
                            {
                                if (!IsAllowDeleteFromAveStorage(fileInfo, storageId))
                                {
                                    this.hasPendingDeletedDuplicatedFiles = true;
                                    continue;
                                }
                            }

                            var deletedSize = RealDeleteDataFromDevice(fileInfo);
                            AccumulateDeleteSize(deletedSizeMapping, fileInfo.JobId, deletedSize);
                            UpdateDedupFileWithDeletedStatus(fileInfo);
                            SetArchiverDedupInfo(fileInfo.DedupTime);
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            this.hasErrorDedupFile = true;
                            string errorMsg = ex.Message;
                            if (ex is Storage.Util.DeviceNotAvailableException)
                            {
                                errorMsg = "RM_JM_Detail_Dedup_DeviceNotAvailable";
                            }
                            AddDedupJobReportForDeleteStorage(fileInfo, JobDetailsStatus.Failed, fileInfo.ContentLength, errorMsg);
                            logger.Error($"Real del data from device fails. Id:{fileInfo.Id}. {ex}");
                        }
                    }

                    UpdateBackupMediaDataSize(deletedSizeMapping);

                }, 200);
            }

        }

        private bool IsAllowDeleteFromAveStorage(ArchiverBodyIndex fileInfo, string storageId)
        {
            if(!this.isDisableRetentionPeriodLimitation && IsSavedBySystemStorage(storageId))
            {
                if(fileInfo.ArchiveTime > retentionPeriodLimitTicks)
                {
                    logger.Warn($"The dedup file could not deleted from Ave Storage. Id:{fileInfo.Id}, ArchivedTime: {fileInfo.ArchiveTime}");
                    return false;
                }
            }

            return true;
        }

        private void AccumulateDeleteSize(Dictionary<string, long> deletedSizeMapping, string subJobId, long deletedSize)
        {
            if (deletedSize <= 0)
            {
                return;
            }

            if (!deletedSizeMapping.TryGetValue(subJobId, out var totalSize))
            {
                totalSize = 0;
            }
            totalSize += deletedSize;

            deletedSizeMapping[subJobId] = totalSize;
        }

        private void UpdateBackupMediaDataSize(Dictionary<string, long> deletedSizeMapping)
        {
            foreach (var item in deletedSizeMapping)
            {
                DeduplicationService.DecreaseMediaDataSize(item.Key, item.Value);
            }
        }

        private void SetArchiverDedupInfo(long dedupTime)
        {
            if (this.hasDedupSuccessedFile)
            {
                this.minDedupTime = Math.Min(dedupTime, this.minDedupTime);
                this.maxDedupTime = Math.Max(dedupTime, this.maxDedupTime);
            }
            else
            {
                this.minDedupTime = dedupTime;
                this.maxDedupTime = dedupTime;
            }
            this.hasDedupSuccessedFile = true;
        }


        #region Data Device Operations

        private StorageInfo GetStorageInfo(IXSystem dataDevice, ArchiverBodyIndex fileInfo)
        {
            if (dataDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object))
            {
                return new StorageInfo
                {
                    ExtraStorageInfo = fileInfo.DuplicateFileStorageInfo,
                };
            }
            else
            {
                string name = fileInfo.JobId + "_content_" + fileInfo.DuplicateFileNumber + ".dat";
                string highName = this.dataVolume;
                return XConvert.FromNames(highName, name);
            }
        }

        private long RealDeleteDataFromDevice(ArchiverBodyIndex fileInfo)
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.RealDeleteDataFromDevice"))
            {
                var dataLogicalDevice = GetDataLogicalDevice(fileInfo);
                if (dataLogicalDevice == null)
                {
                    AddDedupJobReportForDeleteStorage(fileInfo, JobDetailsStatus.Failed, fileInfo.ContentLength, "RM_JM_Detail_Dedup_DeviceNotAvailable");
                    return 0;
                }

                var info = GetStorageInfo(dataLogicalDevice, fileInfo);
                var delSuccess = false;
                string message = string.Empty;
                long fileSize = 0;
                logger.Info($"Start to delete device content. ContentDeviceName:{info.HighName}\\{info.LowName}.");
                try
                {
                    StorageDeleteResult deleteDataResult = dataLogicalDevice.DeleteFile(info);
                    if (deleteDataResult.IsDeleted)
                    {
                        logger.Info($"Finished to delete device content. ContentDeviceName:{info.HighName}\\{info.LowName}.");
                        fileSize = deleteDataResult.DeletedFileSize;
                        delSuccess = true;
                    }
                    else
                    {
                        message = deleteDataResult.Message;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete device content:{info.LowName}. {ex}");
                    message = ex.Message;
                }

                if (!delSuccess)
                {
                    if (!dataLogicalDevice.FileExists(info))
                    {
                        fileSize = fileInfo.ContentLength;
                        logger.Info($"Device content not exists. ContentDeviceName:{info.HighName}\\{info.LowName}.");
                        message = string.Empty;
                    }
                }

                AddDedupJobReportForDeleteStorage(fileInfo, delSuccess ? JobDetailsStatus.Successful : JobDetailsStatus.Failed, fileInfo.ContentLength, message);

                return fileSize;
            }
        }

        private string GetStorageDeviceIdByArchiverJobId(string jobId)
        {
            if (!this.storageDeviceIDs.TryGetValue(jobId, out var storageId))
            {
                storageId = DeduplicationService.GetArchiverIndexStorageId(jobId);
                if (string.IsNullOrEmpty(storageId))
                {
                    logger.Error($"Can't find physical device id by jobId: {jobId}");
                }

                this.storageDeviceIDs[jobId] = storageId;
            }

            return storageId;
        }

        private IXSystem GetDataLogicalDevice(ArchiverBodyIndex fileInfo)
        {
            var storageId = GetStorageDeviceIdByArchiverJobId(fileInfo.JobId);
            if (string.IsNullOrEmpty(storageId))
            {
                return null;
            }
            else
            {
                return GetDataLogicalDevice(storageId);
            }
        }

        private bool IsSavedBySystemStorage(string storageId)
        {
            var storageDeviceInfo = GetStorageDeviceInfo(storageId);
            if(storageDeviceInfo != null)
            {
                return storageDeviceInfo.IsSystemStorage;
            }
            return false;
        }

        private StorageDeviceDto GetStorageDeviceInfo(string storageDeviceId)
        {
            if (string.IsNullOrEmpty(storageDeviceId))
            {
                return null;
            }
            if(!this.storageDeviceInfoes.TryGetValue(storageDeviceId, out var storageDeviceInfo))
            {
                storageDeviceInfo = StorageDeviceService.GetStorageDeviceById(storageDeviceId, needDecryptSecert: true);
                this.storageDeviceInfoes[storageDeviceId] = storageDeviceInfo;
            }
            return storageDeviceInfo;
        }

        private IXSystem GetDataLogicalDevice(string storageDeviceId)
        {
            if (!this.dataLogicalDevices.TryGetValue(storageDeviceId, out var dataLogicalDevice))
            {
                var storageDevice = GetStorageDeviceInfo(storageDeviceId);
                if (storageDevice != null)
                {
                    try
                    {
                        dataLogicalDevice = this.StorageDeviceManager.Open(new List<string>() { storageDevice.BuildXRI() });
                    }
                    catch (Storage.Util.DeviceNotAvailableException ex)
                    {
                        this.dataLogicalDevices[storageDeviceId] = null;
                        throw;
                    }
                }
                else
                {
                    logger.Error($"Can't find storage device by id: {storageDeviceId}");
                }

                this.dataLogicalDevices[storageDeviceId] = dataLogicalDevice;
            }

            return dataLogicalDevice;
        }

        #endregion

        #region Job Report and Progress

        private void GenerateJobReport()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.GenerateJobReport"))
            {
                this.logger.Info($"Start generating dedup job report.Job: {this.subJobId}.");
                try
                {
                    if (!disablePerformanceMonitor)
                    {
                        AvePerformanceMonitor.WritePerformanceResult();
                    }

                    JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails()
                    {
                        ActionStatistics = new List<ActionStatistics>() { reportStatistics }
                    };
                    ReportManager.SendJobDetail(summaryDetails);
                    AvePerformanceTimerPool.Clear();
                }
                catch (Exception ex)
                {
                    this.logger.Error($"An error occurred while generating dedup job report, details: {ex}. Job: {this.subJobId}.");
                }
            }
        }

        public void AddDedupJobReportForDeleteStorage(ArchiverBasicIndex index, JobDetailsStatus currentFileStatus, long deleteStorageSize, string comment = "")
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("FileDedup.AddDedupJobReportForDeleteStorage"))
            {
                var deleteStorageDetail = new JMArchiverDedupJobDetails();
                deleteStorageDetail.DedupTime = DateTime.UtcNow.Ticks;
                deleteStorageDetail.Name = index.Name;
                deleteStorageDetail.Size = index.ContentLength;
                deleteStorageDetail.SrcURL = GetFileUrl(index.ExtraInfo);
                deleteStorageDetail.SubJobId = this.subJobId;//retention job id
                deleteStorageDetail.ModifyTime = index.ModifyTime;
                deleteStorageDetail.BackupSubJobId = index.JobId;//backup subsubjobid
                deleteStorageDetail.NewFileStoragePath = string.IsNullOrEmpty(index.StorageInfo) ? index.DedupSourceFileJobId + "_content_" + index.ContentDataFileNumber + ".dat" : index.StorageInfo;
                deleteStorageDetail.OldFileStoragePath = string.IsNullOrEmpty(index.DuplicateFileStorageInfo) ? index.JobId + "_content_" + index.DuplicateFileNumber + ".dat" : index.DuplicateFileStorageInfo;
                deleteStorageDetail.Comment = comment;
                deleteStorageDetail.Status = currentFileStatus;
                ReportManager.SendJobDetail(deleteStorageDetail);
                reportStatistics.Size += index.ContentLength;
                switch (currentFileStatus)
                {
                    case JobDetailsStatus.Successful:
                        reportStatistics.SuccessfulObj.ItemCount++;
                        break;
                    case JobDetailsStatus.Failed:
                        reportStatistics.FailedObj.ItemCount++;
                        break;
                    case JobDetailsStatus.Skipped:
                        reportStatistics.SkippedObj.ItemCount++;
                        break;
                    default:
                        break;
                }
            }
        }

        public void AddDedupJobReportForSite(JobDetailsStatus currentFileStatus, string comment = "")
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("FileDedup.AddDedupJobReportForSite"))
            {
                var deleteStorageDetail = new JMArchiverDedupJobDetails();
                deleteStorageDetail.DedupTime = DateTime.UtcNow.Ticks;
                deleteStorageDetail.Name = this.jobData.SiteCollectionURL.Split('/').LastOrDefault();
                deleteStorageDetail.Size = reportStatistics.SuccessfulObj.ItemCount;
                deleteStorageDetail.SrcURL = this.jobData.SiteCollectionURL;
                deleteStorageDetail.SubJobId = this.subJobId;
                deleteStorageDetail.ModifyTime = 0;
                deleteStorageDetail.BackupSubJobId = string.Empty;
                deleteStorageDetail.NewFileStoragePath = string.Empty;
                deleteStorageDetail.OldFileStoragePath = string.Empty;
                deleteStorageDetail.Comment = comment;
                deleteStorageDetail.Status = currentFileStatus;
                ReportManager.SendJobDetail(deleteStorageDetail);
            }
        }

        private string GetFileUrl(string extraInfo)
        {
            string fileUrl = string.Empty;
            if (!string.IsNullOrEmpty(extraInfo))
            {
                var docment = new XmlDocument();
                docment.LoadXml(extraInfo);
                var rootElement = docment.DocumentElement;
                if (rootElement.HasAttribute("APUrl"))
                {
                    fileUrl = rootElement.GetAttribute("APUrl").Replace(@"\", @"/");
                }
            }
            return fileUrl;
        }

        private void PreSetJobCompletedStatus()
        {
            if(this.jobStatus == RMJobStatus.Skipped || this.jobStatus == RMJobStatus.Stopped)
            {
                this.logger.Info($"Already Completed Status: {this.jobStatus}");
                return;
            }
            else if (this.jobStatus == RMJobStatus.Stopping)
            {
                this.jobStatus = RMJobStatus.Stopped;
            }
            else if (this.hasErrorDedupFile || this.hasUnexpectedException)
            {
                this.jobStatus = hasDedupSuccessedFile ? RMJobStatus.FinishWithException : RMJobStatus.Failed;
            }
            else
            {
                this.jobStatus = RMJobStatus.Finished;
            }
            this.logger.Info($"Job Completed Status: {this.jobStatus}");
        }

        private void UpdateJobCompletedStatus()
        {
            var jobState = this.jobStatus;
            if (this.hasErrorDedupFile || this.hasUnexpectedException || jobState == RMJobStatus.FinishWithException || jobState == RMJobStatus.Failed)
            {
                if (string.IsNullOrEmpty(summaryComment))
                {
                    summaryComment = "RM_JM_Summary_DedupCommentWhenError";
                    AddDedupJobReportForSite(jobState == RMJobStatus.Failed ? JobDetailsStatus.Failed : JobDetailsStatus.Exception, "RM_JM_Summary_DedupCommentWhenError");
                }
            }
            else if (!this.hasDedupSuccessedFile && jobState == RMJobStatus.Finished)
            {
                AddDedupJobReportForSite(JobDetailsStatus.Successful, "RM_JM_Summary_DedupCommentWhenNoDuplicatedFiles");
            }
            ReportManager.SetJobFinished(jobState, summaryComment);
            this.logger.Info($"Finished UpdateJobCompletedStatus.jobID:{this.subJobId}.");
        }

        private void SetSiteCollectionDedupInfo()
        {
            if (this.hasErrorDedupFile || this.hasUnexpectedException)
            {
                this.logger.Info($"Skip Update deduplicated status for ArchiverSiteMasterIndex. HasErrorDedupFile: {this.hasErrorDedupFile}, HasException: {this.hasUnexpectedException}");
            }
            else if (this.jobStatus != RMJobStatus.FinishWithException && this.jobStatus != RMJobStatus.Failed
                && this.jobStatus != RMJobStatus.Stopping && this.jobStatus != RMJobStatus.Stopped)
            {
                this.logger.Info($"Update deduplicated status for ArchiverSiteMasterIndex");
                IEnumerable<string> siteMasterIndexIDs = this.jobData.ArchiverSiteMasterIndexIds;
                if (this.hasPendingDeletedDuplicatedFiles)
                {
                    siteMasterIndexIDs = siteMasterIndexIDs.Skip(1);
                }
                DeduplicationService.UpdateArchiverMasterIndexDeduplicatedState(siteMasterIndexIDs);
            }

            if (hasDedupSuccessedFile)
            {
                this.logger.Info($"Upsert dedup info for: {this.jobData.SiteCollectionURL}");
                DeduplicationService.UpsertArchiverDedupInfo(this.jobData.SiteCollectionURL, this.minDedupTime, this.maxDedupTime);

                UpdateArchivedInfo();
                UpdateTeamsGroupArchivedInfo();
            }
        }

        private void UpdateArchivedInfo()
        {
            try
            {
                var syncArchivedSiteInfo = KeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
                if (syncArchivedSiteInfo != null)
                {
                    this.logger.Info("start update archiver size for archiver info");

                    bool result;
                    if (bool.TryParse(syncArchivedSiteInfo.Value, out result) && result)
                    {
                        var siteUrlAndJobIdMapping = ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(new List<string>() { this.jobData.SiteCollectionURL });
                        var siteUrlAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
                        //long fileCount = GetFileCount();
                        //long fileVersionCount = GetFileVersionCount();
                        //this.logger.Info($"file count is:{fileCount},version count is:{fileVersionCount}");
                        //ArchiveSiteInfoDao.UpdateArchiverInfo(
                        //    this.jobData.SiteCollectionURL, 
                        //    fileCount, 
                        //    fileVersionCount, 
                        //    siteUrlAndSizeMapping.Any() ? siteUrlAndSizeMapping[this.jobData.SiteCollectionURL] : 0);
                        ArchiveSiteInfoDao.UpdateArchiverSize(this.jobData.SiteCollectionURL, siteUrlAndSizeMapping.Any() ? siteUrlAndSizeMapping[this.jobData.SiteCollectionURL] : 0);
                    }
                    else
                    {
                        logger.Warn($"syncArchivedSiteInfo value is false or syncArchivedSiteInfo value convert failed,syncArchivedSiteInfo Value is:{syncArchivedSiteInfo.Value}");
                    }
                }
                else
                {
                    logger.Warn("syncArchivedSiteInfo is null,please check it in db");
                }
            }
            catch (Exception ex)
            {
                this.hasUnexpectedException = true;
                this.logger.Warn($"Error occurred while updating archiver size. {ex}");
            }
        }

        private void UpdateTeamsGroupArchivedInfo()
        {
            try
            {
                var worker = new TeamsSODashboardWorker();
                worker.UpdateTeamsGroupRelatedSiteArchivedInfo(this.jobData.SiteCollectionURL).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                this.hasUnexpectedException = true;
            }
        }


        private int currentProgress = 0;    // 当前进度
        private int currentStageTotalProgress = 0;  // 当前阶段的Job进度的最大占比（最多增长多少进度）
        private int currentStageProgressStartPoint = 1;    // 当前阶段的Job进度的起点
        private void MoveToNextJobStage(int minProgress, int maxProgress, int stageTotalProgress = 0)
        {
            this.currentStageTotalProgress = stageTotalProgress;
            if (minProgress >= maxProgress)
            {
                maxProgress = minProgress;
            }

            if (maxProgress > currentStageProgressStartPoint)
            {
                currentStageProgressStartPoint = new Random().Next(minProgress, maxProgress);
            }

            currentProgress = currentStageProgressStartPoint;
            ReportManager.SetProgress(currentProgress);
        }
        private void UpdateCurrentStageProgress(int totalSize, int completedSize)
        {
            try
            {
                if (totalSize <= 0)
                {
                    return;
                }

                int increaseProgress = completedSize * this.currentStageTotalProgress / totalSize;
                if (increaseProgress + currentStageProgressStartPoint > currentProgress)
                {
                    currentProgress = increaseProgress + currentStageProgressStartPoint;

                    ReportManager.SetProgress(currentProgress);
                }
            }
            catch (Exception e)
            {
                logger.Error($"UpdateCurrentStageProgress failed.Message:{e}.");
            }
        }

        private async Task<bool> CheckJobIsStoppingAsync()
        {
            var jobState = await SubJobDao.GetSubJobStatusAsync(this.subJobId);
            if (jobState == RMJobStatus.Stopping || jobState == RMJobStatus.Stopped)
            {
                this.jobStatus = jobState;
                logger.Warn($"Current dedup sub job is stopping.");
                return true;
            }

            return false;
        }

        #endregion

        #region Main Index Operations

        private CacheSettingDto GetCacheSetting()
        {
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!System.IO.Directory.Exists(archiveTemp))
            {
                System.IO.Directory.CreateDirectory(archiveTemp);
            }

            CacheSettingDto cache = new CacheSettingDto()
            {
                Extension = new CacheSettingExtension()
                {
                    Path = new List<PathMap>() {
                        new PathMap() {
                            DiskInfo = new DiskInfoDto() {
                                Path = archiveTemp
                            }
                        }
                    }
                }
            };
            return cache;
        }

        private void OpenMainIndex()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.OpenMainIndex"))
            {
                logger.Info("Begin opening mainindex.");
                var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
                {
                    IndexDatabaseName = ServiceConstants.IndexDBName,
                    //BackupJobId = ,
                    IndexVolume = indexVolume,
                    TreeMode = TreeMode.SiteCollectionMode,
                    IndexLogicalDeviceSystem = this.indexLogicalDevice,
                    IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                    CacheSetting = this.cacheSetting,
                    //StorageInfo = 
                };
                IndexSynchronizer.Initialize(indexServiceOpenParameter);
                this.InitMainIndexProcessor(indexServiceOpenParameter);
            }
        }

        private void InitMainIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.IndexMainProcessor.Open(param);
            }

            this.logger.Info("Open MainIndex Finished.");
        }

        private void UploadMainIndexToAzure()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadMainIndexToAzure"))
            {
                logger.Info($"Begin UploadMainIndexToAzure.");
                var mainIndexDBInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                {
                    this.IndexSynchronizer.Upload(mainIndexDBInfo);
                });
                logger.Info($"End UploadMainIndexIndexToAzure.");
            }
        }

        private List<string> QueryAllDuplicateCRC()
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.QueryAllDuplicateCRC"))
            {
                return this.IndexMainProcessor.ExecuteQueryForOneColume<string>(SqliteIndexDBQuery.SelectAllDuplicateCRC, null);
            }
        }

        private List<ArchiverBodyIndex> QueryDuplicateFileIndexesByCRCs(IEnumerable<string> crcList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.QueryFileIndexByCRCs"))
            {
                return this.IndexMainProcessor.ExecuteQuery<ArchiverBodyIndex>(
                    $"{SqliteIndexDBQuery.SelectDuplicateFilesByCRCs} ('{string.Join("','", crcList)}')",
                    new Dictionary<string, object>());
            }
        }

        private void UpdateFileIndexDedupInfo<T>(IIndexProcessor<T> indexDbProcessor, ArchiverBodyIndex fileIndex)
            where T : IndexProcessorParameter
        {
            var param = new Dictionary<string, object>()
            {
                { "@ContentDataFileNumber", fileIndex.ContentDataFileNumber },
                { "@StorageInfo", fileIndex.StorageInfo },
                { "@DedupSourceFileId", fileIndex.DedupSourceFileId },
                { "@DuplicateStatus", fileIndex.DuplicateStatus },
                { "@DedupExtension", fileIndex.DedupExtension },
                { "@DelStatus", fileIndex.DelStatus },
                { "@COL_ID", fileIndex.Id },
            };
            indexDbProcessor.Execute(
                SqliteIndexDBQuery.UpdateFileIndexDedupInfoById,
                param);
        }

        private void UpdateFileDedupInfoToMainIndexDB<T>(IIndexProcessor<T> indexDbProcessor, ArchiverBodyIndex fileIndex)
            where T : IndexProcessorParameter
        {
            logger.Info($"Index is updating to dedup status. ID: {fileIndex.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileDedupInfoToMainIndexDB"))
            {
                UpdateFileIndexDedupInfo(indexDbProcessor, fileIndex);
            }
        }

        private void UpdateFileDedupInfoToMainIndexDB(IEnumerable<ArchiverBodyIndex> indexes)
        {
            logger.Info($"Those file indexes is updating to dedup status: {indexes.Count()}");
            foreach (var item in indexes)
            {
                UpdateFileDedupInfoToMainIndexDB(this.IndexMainProcessor, item);
            }
        }

        //private long GetFileCount()
        //{
        //    var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
        //    return Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(sql, null));
        //}

        //private long GetFileVersionCount()
        //{
        //    var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
        //    return Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(sql, null));
        //}

        #endregion

        #region Sub Index Operations

        private IIndexProcessor<ArchiverIndexProcessorParameter> GetSubIndexProcessor(string archiverSubJobId)
        {
            if (!SubIndexProcessors.TryGetValue(archiverSubJobId, out var subIndexProcessor))
            {
                subIndexProcessor = OpenSubIndex(archiverSubJobId);
                SubIndexProcessors[archiverSubJobId] = subIndexProcessor;
            }

            return subIndexProcessor;
        }

        private IIndexProcessor<ArchiverIndexProcessorParameter> OpenSubIndex(string archiverSubJobId)
        {
            IIndexProcessor<ArchiverIndexProcessorParameter> subIndexProcessor = null;
            this.logger.Info($"Begin opening SubIndex: {archiverSubJobId}");
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.OpenSubIndex"))
                {
                    var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
                    {
                        IndexDatabaseName = $"{archiverSubJobId}_{ServiceConstants.IndexDBName}",
                        //BackupJobId = ,
                        IndexVolume = indexVolume,
                        TreeMode = TreeMode.SiteCollectionMode,
                        IndexLogicalDeviceSystem = this.indexLogicalDevice,
                        IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                        CacheSetting = this.cacheSetting,
                        //StorageInfo = 
                    };
                    IndexSynchronizer.Initialize(indexServiceOpenParameter);

                    subIndexProcessor = this.InitSubIndexProcessor(indexServiceOpenParameter);
                }

                logger.Info($"Open SubIndex Finished: {archiverSubJobId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Open SubIndex Failed: {archiverSubJobId}. {ex}");
            }
            return subIndexProcessor;
        }

        private IIndexProcessor<ArchiverIndexProcessorParameter> InitSubIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var subIndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            //param.IsNeedCheckIntegrity = true;
            subIndexProcessor.Open(param);

            return subIndexProcessor;
        }

        private void UploadChangedSubIndexToAzure()
        {
            foreach (var archiverSubJobId in this.changedSubIndexes)
            {
                UploadSubIndexToAzure(archiverSubJobId);
            }
        }

        private void UploadSubIndexToAzure(string archiverSubJobId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadSubIndexToAzure"))
            {
                var subIdxDbName = $"{archiverSubJobId}_{ServiceConstants.IndexDBName}";
                logger.Info($"Begin UploadSubIndexToAzure: {subIdxDbName}");
                var subIndexDBInfo = new IndexDatabaseInfo(subIdxDbName, null);
                try
                {
                    DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                    {
                        this.IndexSynchronizer.Upload(subIndexDBInfo);
                    });
                    logger.Info($"End UploadSubIndexToAzure: {subIdxDbName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while UploadSubIndexToAzure: {subIdxDbName}. {ex}");
                }
            }
        }

        private void UpdateFileDedupInfoToSubIndexDB(IIndexProcessor<ArchiverIndexProcessorParameter> subIndexProcessor, ArchiverBodyIndex index)
        {
            logger.Info($"Update dedup status to sub index db. Id: {index.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileDedupStatusForSubIndexDB"))
            {
                UpdateFileIndexDedupInfo(subIndexProcessor, index);
            }
        }

        #endregion

        #region Dedup Index Operations

        private void OpenDedupFileIndex()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.OpenDedupFileIndex"))
            {
                this.logger.Info("Begin opening Dedup File Index.");
                var indexServiceOpenParameter = new ArchiverDedupIndexServiceOpenParameter()
                {
                    IndexDatabaseName = ServiceConstants.DedupIndexDBName,
                    //BackupJobId = ,
                    IndexVolume = indexVolume,
                    TreeMode = TreeMode.SiteCollectionMode,
                    IndexLogicalDeviceSystem = this.indexLogicalDevice,
                    IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                    CacheSetting = this.cacheSetting,
                    //StorageInfo = 
                };
                //RetentionIndexSynchronizer.Initialize(indexServiceOpenParameter);
                this.InitDedupFileIndexProcessor(indexServiceOpenParameter);
            }
        }

        private void InitDedupFileIndexProcessor(ArchiverDedupIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);

            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                {
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
                }
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));

                //azure不存在 dedup index，本地新创建，如果存在缓存的dedup index，此处会抛错，因此azure不存在时先删除本地cache的dedup index.
                FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                if (finfo.Exists)
                {
                    this.logger.Info($"The dedup index file exist in media cache and delete it.Path:{indexDownLoadInfo.IndexFullPath}.");
                    try
                    {
                        finfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Delete dedup index file failed.Path:{indexDownLoadInfo.IndexFullPath}.Error:{ex}.");
                    }
                }
            }

            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            var param = new ArchiverDedupIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            //param.IsNeedCheckIntegrity = true;
            this.DedupIndexProcessor.Open(param);
            this.logger.Info("Open DedupFileIndex Finished.");
        }

        private void UploadDedupIndexToAzure()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadDedupIndexToAzure"))
            {
                logger.Info($"Begin UploadDedupIndexToAzure.");
                var dedupIndexDBInfo = new IndexDatabaseInfo(ServiceConstants.DedupIndexDBName, null);
                DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                {
                    IndexSynchronizer.Upload(dedupIndexDBInfo);
                });
                logger.Info($"End UploadDedupIndexToAzure.");
            }
        }

        private void InsertToDedupIndexFile(List<ArchiverBodyIndex> indexes)
        {
            var idList = indexes.Select(i => i.Id);

            logger.Info($"Insert dedup files to dedup index db. IDs: {string.Join(",", idList)}");
            try
            {
                using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.InsertToDedupIndexFile"))
                {
                    this.DedupIndexProcessor.Insert(indexes);
                }
            }
            catch (Exception ex)
            {
                logger.Info($"Insert dedup files failed. try to delete exists inserted files. {ex}");
                using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.DeleteExistsDedupItems"))
                {
                    this.DedupIndexProcessor.Execute(
                        $"{SqliteIndexDBQuery.DeleteExistsFilesByIDs} ('{string.Join("', '", idList)}')",
                        null as Dictionary<string, object>);
                }

                logger.Info($"Retry to insert dedup files to dedup index db. IDs: {string.Join(",", idList)}");
                using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.InsertToDedupIndexFileRetry"))
                {
                    this.DedupIndexProcessor.Insert(indexes);
                }
            }
        }

        private void UpdateFileInDedupIndexDB(ArchiverBodyIndex fileIndex)
        {
            logger.Info($"Updating file in Dedup Index DB. ID: {fileIndex.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileInDedupIndexDB"))
            {
                UpdateFileIndexDedupInfo(this.DedupIndexProcessor, fileIndex);
            }
        }

        private void UpdateDedupFileWithDeletedStatus(ArchiverBodyIndex fileInfo)
        {
            fileInfo.DedupTime = DateTime.UtcNow.Ticks;
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateDeletedDedupFile"))
            {
                this.DedupIndexProcessor.Execute(
                    SqliteIndexDBQuery.UpdateDeletedDedupFileById,
                    new Dictionary<string, object>
                    {
                        { "@COL_ID", fileInfo.Id },
                        { "@DedupTime", fileInfo.DedupTime },
                    });
            }
        }

        private List<ArchiverBodyIndex> GetDedupFileIndexesBySourceFileId(string sourceFileId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.GetDedupFileIndexesBySourceFileId"))
            {
                return this.DedupIndexProcessor.ExecuteQuery<ArchiverBodyIndex>(
                    SqliteIndexDBQuery.SelectDedupFilesBySourceFileId,
                    new Dictionary<string, object>
                    {
                        { "@SourceFileId", sourceFileId }
                    });
            }
        }

        private int GetDeletingDedupFileIndexesCount()
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("Dedup.GetDeletingDedupFileIndexesCount"))
            {
                return Convert.ToInt32(this.DedupIndexProcessor.ExecuteScalar(SqliteIndexDBQuery.SelectAllDeletingFilesCount, null));
            }
        }

        private List<ArchiverBodyIndex> GetDeletingDedupFileIndexes(int offset, int pageSize)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.GetDeletingDedupFileIndexes"))
            {
                return this.DedupIndexProcessor.ExecuteQuery<ArchiverBodyIndex>(
                    SqliteIndexDBQuery.SelectAllDeletingFiles,
                    new Dictionary<string, object>
                    {
                        { "@OFFSET", offset },
                        { "@LENGTH", pageSize },
                    });
            }
        }

        #endregion

        private void DisposeObj()
        {
            if (IndexMainProcessor != null)
            {
                try
                {
                    IndexMainProcessor.Close();
                }
                catch (Exception ex)
                {
                    logger.Error($"Close mian index fails. {ex}");
                }
            }
            if (DedupIndexProcessor != null)
            {
                try
                {
                    DedupIndexProcessor.Close();
                }
                catch (Exception ex)
                {
                    logger.Error($"Close dedup index fails. {ex}");
                }
            }
            if (StorageDeviceManager != null)
            {
                try
                {
                    StorageDeviceManager.Close(this.indexLogicalDevice);
                }
                catch (Exception ex)
                {
                    logger.Error($"Close index device fails. {ex}");
                }

                foreach (var item in this.dataLogicalDevices)
                {
                    try
                    {
                        StorageDeviceManager.Close(item.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Close data device fails: {item.Key}. {ex}");
                    }
                }
            }
        }

        public void Dispose()
        {
            this.DisposeObj();
        }
    }
}
