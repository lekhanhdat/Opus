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
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.SharePoint.Client;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.FileSystem.Restore.Common;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.SharePoint;

namespace RAFileSystem.FileSystem.FileSystem.Restore
{
    public class FSRestoreWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService = new ArchiverIndexService();
        private IProgressService ProgressService { get; set; }
        public IArchiverBackupIndexService BackupIndexService = new ArchiverBackupIndexService();
        public CacheSettingDto CacheSetting { get; set; }
        public CacheSettingDto RestoredPathSetting { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }
        public LogicalDeviceDto LogicalDeviceDto { get; set; }
        public IXSystem indexLogicalDevice;
        public IXSystem dataLogicalDevice;
        public String ConnectionName { get; set; }
        public String ConnectionId { get; set; }
        public String IndexVolume { get; set; }
        public String DataVolume { get; set; }
        public RestoreInfo restoreInfo { get; set; }
        public List<RestoreSecurityInfoWrapper> restoreSecurityInfos { get; set; }
        public IStorageDeviceManager StorageDeviceManager = new StorageDeviceManager();
        public ICacheService CacheManager = new CacheService();
        public ICacheService RestoreLocationManager = new CacheService();
        public IDataReader<ArchiverRestoreJob> DataReader = new ArchiverRestoreDataReader();
        public IEncryptionInfoManager EncryptionInfoManager = new EncryptionInfoManager();
        public ArchiverIndexService _ArchiverIndexService = new ArchiverIndexService();
        private AgentActionStatistics _RestoreStatistics = new AgentActionStatistics { ActionTab = (int)ActionTab.Restore };
        public List<ArchiverRestoreSerchResult> NeedToRestoredItems { get; set; }
        private List<FSMasterIndexContract> fsMasterIndex;
        private Dictionary<string, BlobContainerClient> blobClientDic = new Dictionary<string, BlobContainerClient>();
        public List<string> NeedToRestoredItemsPathMD5 { get; set; }
        public SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();
        public FSRestoreWorker(RestoreInfo info)
        {

            restoreInfo = info;
            NeedToRestoredItems = restoreInfo.NodeObjects;
            logger.Info($"fs restore items count is:{NeedToRestoredItems?.Count}");
            NeedToRestoredItemsPathMD5 = NeedToRestoredItems.Select(x => x.PathMd5).ToList();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            DataLogicalDeviceList = new List<LogicalDeviceDto>();
            LogicalDeviceDto = new LogicalDeviceDto() { PhysicalDrives = new List<PhysicalDeviceDto>()};
        }
        public void RunRestoreJob()
        {
            try
            {
                logger.Info("Start run fs restore job");
                InitInfo();
                Open();
                Restore();
                logger.Info("FS restore job finished");
            }
            catch (Exception ex)
            {
                logger.Error("FS restore job error", ex);
            }
            finally
            {
                AddSummaryReport();
                Close();
            }
        }
        private HashSet<string> GetAllStorageLogicalDevices(List<FSMasterIndexContract> indexes)
        {
            HashSet<string> logicalDeviceIdList = new HashSet<string>();
            foreach (var index in indexes)
            {
                foreach (var subInfo in index.SubInfo)
                {
                    logicalDeviceIdList.Add(string.IsNullOrEmpty(subInfo.CurrentStorageId) ? subInfo.StorageInfo : subInfo.CurrentStorageId);
                }
            }
            return logicalDeviceIdList;
        }
        void InitInfo()
        {
            using (var pc1 = new AgentPerformanceScope("FSRestore.InitInfo", addToStatistics: true))
            {
                logger.Info("Init restore info");
                this.ConnectionId = restoreInfo.NodeObjects[0].SitePath;
                this.ConnectionName = restoreInfo.NodeObjects[0].SitePath;
                FSJobCache.RestoreInstance.FSRestoreLocation = restoreInfo.NodeObjects[0].TreeNode;
                FSJobCache.RestoreInstance.FSRestoreOption = restoreInfo.RestoreOption;
                FSJobCache.RestoreInstance.FSRestoreCachePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSRestoreCacheLocation"+DateTime.UtcNow.Ticks.ToString());
                logger.Info($"the fs restore option is:{restoreInfo.RestoreOption}");
                if (!Directory.Exists(FSJobCache.RestoreInstance.FSRestoreCachePath))
                {
                    Directory.CreateDirectory(FSJobCache.RestoreInstance.FSRestoreCachePath);
                }
                logger.Info("ConnectionId:{0}, ConnectionName:{1}", this.ConnectionId, this.ConnectionName.LogBase64());
                var indexDevice = HybridApiClient.Instance.GetIndexDevice();
                IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);
                fsMasterIndex = HybridApiClient.Instance.GetConnectionMasterWithSubInfosList(ConnectionId);
                var deviceIdList = GetAllStorageLogicalDevices(fsMasterIndex);
                foreach (var deviceId in deviceIdList)
                {
                    var dataDevice = HybridApiClient.Instance.GetStorageDeviceById(deviceId);
                    DataLogicalDeviceList.Add(ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(dataDevice));
                }
                DiskInfoDto disk = new DiskInfoDto()
                {
                    Path = FSJobCache.RestoreInstance.FSRestoreCachePath,
                    Type = DeviceType.LocalPath,
                    Password = null,
                    UserName = string.Empty,
                    Usage = null
                };
                CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
                CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
                CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
                var volumeGenerator = new ArchiverVolumeGenerator();
                var volumeParam = new VolumeParameter() { ConnectionId = this.ConnectionId, ConnectionName = this.ConnectionName };
                this.IndexVolume = volumeGenerator.GenerateIndexVolume(volumeParam);
                this.DataVolume = volumeGenerator.GenerateDataVolume(volumeParam);
            }
        }
        void Open()
        {
            using (var pc1 = new AgentPerformanceScope("FSRestore.openDevice", addToStatistics: true))
            {
                indexLogicalDevice = StorageDeviceManager.Open(IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.ReadWrite);
                DataLogicalDeviceList.ForEach(logicalDevice =>
                {
                    logicalDevice.PhysicalDrives.ForEach(physicalDevice =>
                    {
                        logger.Debug(physicalDevice.Name.LogBase64());
                        this.LogicalDeviceDto.PhysicalDrives.Add(physicalDevice);
                    });
                });
                dataLogicalDevice = StorageDeviceManager.Open(LogicalDeviceDto.ToXRIS());
                CacheManager.Open(CacheSetting, FSJobCache.RestoreInstance.FSRestoreCachePath, this.dataLogicalDevice.IsDirectSystem);
                ArchiverRestoreJob archiverRestoreJob = new ArchiverRestoreJob(this);
                this.DataReader.Open(archiverRestoreJob);
                var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(restoreSecurityInfos);
                if (encryptionInfoDic != null)
                {
                    this.logger.Info($"Restore security infoes: {string.Join(",", encryptionInfoDic.Keys).LogBase64()}.");
                }
                DataReader.SetEncryptionInfos(encryptionInfoDic);
                OpenMainIndex();
                OpenRestoredLocation();
            }
        }
        private void OpenMainIndex()
        {
            ArchiverIndexServiceOpenParameter indexOpenParam = new ArchiverIndexServiceOpenParameter();
            indexOpenParam.TreeMode = TreeMode.SiteCollectionMode;
            indexOpenParam.IndexVolume = IndexVolume;
            indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
            indexOpenParam.IndexCacheDeviceSystem = XFactory.InstanceLibrary(CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            indexOpenParam.CacheSetting = CacheSetting;
            indexOpenParam.DBPassWord = HybridApiClient.Instance.GetDBSEEMasterKey();
            _ArchiverIndexService.Open(indexOpenParam);
            this.BackupIndexService.InitIndexProcesser(_ArchiverIndexService);
        }
        private void OpenRestoredLocation()
        {
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = "",
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            RestoredPathSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            RestoredPathSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            RestoredPathSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            RestoreLocationManager.Open(RestoredPathSetting, FSJobCache.RestoreInstance.FSRestoreLocation, this.dataLogicalDevice.IsDirectSystem);

        }
        void Close()
        {
            CacheManager.Close();
            RestoreLocationManager.Close();
            DataReader.Close();
            StorageDeviceManager.Close(indexLogicalDevice);
            StorageDeviceManager.Close(dataLogicalDevice);
            _ArchiverIndexService.Close();
        }
        void Restore()
        {
            logger.Info("real started fs restore");
            try
            {
                using (var pc1 = new AgentPerformanceScope("FSRestore.RestoreAllFiles", addToStatistics: true))
                {
                    ProgressService.SetTotal(NeedToRestoredItemsPathMD5.Count + ProgressService.Total);
                    foreach (string md5 in NeedToRestoredItemsPathMD5)
                    {
                        using (var pc2 = new AgentPerformanceScope("FSRestore.RestoreOneFile", addToStatistics: true))
                        {
                            logger.Info($"start find md5:{md5}");
                            var indexInfo = this.BackupIndexService.GetBodyIndexByMD5(md5);
                            logger.Info($"finish find md5:{md5}");
                            try
                            {
                                logger.Info($"start restore file WriteDataToFile,path:{indexInfo.Url.LogBase64()}");
                                StorageInfo info = new StorageInfo() { HighName = indexInfo.ExtraInfo, LowName = indexInfo.Name };
                                WriteDataToFile(info, indexInfo);
                                logger.Info($"finish restored file WriteDataToFile,path:{indexInfo.Url.LogBase64()}");
                                FSJobCache.RestoreInstance.SuccessCount++;
                            }
                            catch (Exception e)
                            {
                                bool hasArchiveTierError = false;
                                try
                                {
                                    hasArchiveTierError = e.InnerException.InnerException.Message.Equals("The remote server returned an error: (409) Conflict.");
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"restore file error,{ex}");
                                    hasArchiveTierError = false;
                                }
                                if (hasArchiveTierError)
                                {
                                    throw new Exception("This operation is not permitted on an archived blob.");
                                }
                                else
                                {
                                    logger.Error($"restore file error,md5:{md5}", e);
                                    FSJobCache.RestoreInstance.FailedCount++;
                                    AddReport(indexInfo.Attributes, indexInfo.ExtraInfo, indexInfo.Name, indexInfo.ContentLength, JobDetailsStatus.Failed, e.Message);
                                }
                            }
                        }
                        ProgressService.Increase();
                        //FSJobCache.RestoreInstance.AnalyzerThreadMonitor.Decrement();
                    }
                }
            }
            catch (Exception e)
            {
                if(e.Message.Contains("This operation is not permitted on an archived blob."))
                {
                    logger.Warn("Storage blob file has been archived,try Rehydration Data.");
                    RehydrationData();
                }
                logger.Error("restore error", e);
            }
        }
        private void RehydrationData()
        {
            foreach (string md5 in NeedToRestoredItemsPathMD5)
            {
                var indexInfo = this.BackupIndexService.GetBodyIndexByMD5(md5);
                try
                {
                    VerifyDataTier(indexInfo);
                    logger.Info($"finish restored file,path:{indexInfo.Url.LogBase64()}");
                    FSJobCache.RestoreInstance.SuccessCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"restore file error,md5:{md5}", e);
                    FSJobCache.RestoreInstance.FailedCount++;
                    AddReport(indexInfo.Attributes, indexInfo.ExtraInfo, indexInfo.Name, indexInfo.ContentLength, JobDetailsStatus.Failed, e.Message);
                }
            }
            WaitingRehydration();
            foreach (string md5 in NeedToRestoredItemsPathMD5)
            {
                var indexInfo = this.BackupIndexService.GetBodyIndexByMD5(md5);
                try
                {
                    StorageInfo info = new StorageInfo() { HighName = indexInfo.ExtraInfo, LowName = indexInfo.Name };
                    WriteDataToFile(info, indexInfo);
                    logger.Info($"finish restored file,path:{indexInfo.Url.LogBase64()}");
                    FSJobCache.RestoreInstance.SuccessCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"restore file error,md5:{md5}", e);
                    FSJobCache.RestoreInstance.FailedCount++;
                    AddReport(indexInfo.Attributes, indexInfo.ExtraInfo, indexInfo.Name, indexInfo.ContentLength, JobDetailsStatus.Failed, e.Message);
                }
            }
            ResetToArchiveTier();
            BLOBMappings.Clear();
        }
        private void ResetToArchiveTier()
        {
            try
            {
                foreach (var r in BLOBMappings)
                {
                    var blobClient = GetBlobAccessTier(r.Value.MappedBlobInfo.HighName + "\\" + r.Value.MappedBlobInfo.LowName, r.Value.backupjobId);
                    if (!blobClient.Exists() || blobClient.GetProperties().Value.AccessTier != AccessTier.Archive)
                    {
                        logger.Info($"The {r.Key.LogBase64()} need to rehydration, " +
                            $"mapping data: {r.Value.MappedBlobInfo.ToString().LogBase64()}, " +
                            $"Exists:{blobClient.Exists()} , " +
                            $"start time : {r.Value.StartTime.ToString()}");
                        blobClient.SetAccessTier(AccessTier.Archive);
                    }
                    else
                    {
                        logger.Info($"The {r.Key.LogBase64()} already rehydration, " +
                            $"mapping data: {r.Value.MappedBlobInfo.ToString().LogBase64()}, " +
                            $"Exists:{blobClient.Exists()} , " +
                            $"start time : {r.Value.StartTime.ToString()}");
                    }
                }

            }
            catch (Exception e)
            {
                logger.Warn($"some thing went wrong when WaitingRehydration e:{e}");
                throw;
            }
        }
        private void WaitingRehydration()
        {
            DateTime time = DateTime.Now;
            try
            {
                while (true)
                {
                    bool needContinueSleep = false;
                    foreach (var r in BLOBMappings)
                    {
                        if (!r.Value.AlreadyRehydration)
                        {
                            var blobClient = GetBlobAccessTier(r.Value.MappedBlobInfo.HighName + "\\" + r.Value.MappedBlobInfo.LowName, r.Value.backupjobId);
                            if (!blobClient.Exists() || blobClient.GetProperties().Value.AccessTier == AccessTier.Archive)
                            {
                                logger.Info($"The {r.Key.LogBase64()} need to rehydration, " +
                                    $"mapping data: {r.Value.MappedBlobInfo.ToString().LogBase64()}, " +
                                    $"Exists:{blobClient.Exists()} , " +
                                    $"start time : {r.Value.StartTime.ToString()}");
                                needContinueSleep = true;
                                break;
                            }
                            else
                            {
                                logger.Info($"The {r.Key.LogBase64()} already rehydration, " +
                                    $"mapping data: {r.Value.MappedBlobInfo.ToString().LogBase64()}, " +
                                    $"Exists:{blobClient.Exists()} , " +
                                    $"start time : {r.Value.StartTime.ToString()}");
                                r.Value.AlreadyRehydration = true;
                            }
                        }
                    }
                    if (needContinueSleep && DateTime.Now - time < TimeSpan.FromDays(5))
                    {
                        logger.Info("Will sleep 15 min to wait blob rehydration.");
                        Thread.Sleep(15 * 60 * 1000);
                        //Thread.Sleep(2* 60 * 1000);
                    }
                    else
                    {
                        logger.Info($"Exit waiting blob rehydration, all the datas rehydration : {!needContinueSleep} .");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"some thing went wrong when WaitingRehydration e:{e}");
                throw;
            }
        }
        private void VerifyDataTier(ArchiverBasicIndex index)
        {
            if (index == null) { return; }
            var nextIndex = this.BackupIndexService.LoadNextIndex(index);
            //can get from index
            if (nextIndex != null)
            {

                if (index.ContentLength > 0)
                {
                    var nextBodyIndex = nextIndex;
                    if (nextBodyIndex.ContentLength == 0)
                    {
                        nextBodyIndex = this.BackupIndexService.LoadNextBodyIndex(index);
                    }
                    #region process data files
                    if (nextBodyIndex != null)
                    {
                        if (nextBodyIndex.CurrentItemContentDataStartFileNumber > index.CurrentItemContentDataStartFileNumber)
                        {
                            logger.Info($"nextBodyIndex CurrentItemContentDataStartFileNumber {nextBodyIndex.CurrentItemContentDataStartFileNumber}, index CurrentItemContentDataStartFileNumber {index.CurrentItemContentDataStartFileNumber}");
                            for (long i = index.CurrentItemContentDataStartFileNumber; i <= nextBodyIndex.CurrentItemContentDataStartFileNumber; i++)
                            {
                                VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemContentDataFilePrefixNumber, i, FileType.Content);
                            }
                        }
                        else
                        {
                            VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemContentDataFilePrefixNumber, index.CurrentItemContentDataStartFileNumber, FileType.Content);
                        }
                    }
                    else
                    {
                        VerifyAllData(index);
                    }
                    #endregion
                }

                #region process meta files
                if (nextIndex.CurrentItemMetaDataStartFileNumber > index.CurrentItemMetaDataStartFileNumber)
                {
                    for (long i = index.CurrentItemMetaDataStartFileNumber; i <= nextIndex.CurrentItemMetaDataStartFileNumber; i++)
                    {
                        VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemMetaDataFilePrefixNumber, i, FileType.MetaData);
                    }
                }
                else
                {
                    VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemMetaDataFilePrefixNumber, index.CurrentItemMetaDataStartFileNumber, FileType.MetaData);
                }
                #endregion
            }
            else
            {
                VerifyAllData(index);
            }
        }
        private void VerifyAllData(ArchiverBasicIndex index)
        {
            StorageInfo info = new StorageInfo() { HighName = DataVolume };
            try
            {
                logger.Info("Start to verify all data.");
                var list = this.dataLogicalDevice.ListFiles(info);
                foreach (var f in list)
                {
                    if (f.LowName.Contains(index.BackupJobId))
                    {
                        ProcessDataFile(index, f.LowName);
                    }
                }
                logger.Info("Finish to verify all data.");
            }
            catch (Exception e)
            {
                logger.Warn("some device are unavailable.error message:{0}", e.ToString());
            }
        }
        private Int64 ProcessDataFile(ArchiverBasicIndex index, string fileName)
        {
            string contentStr = "_content_";
            string metaStr = "_meta_";
            string suffixStr = ".dat";
            long fileNum = -1;
            if (fileName.Contains(contentStr))
            {
                int strIndex = fileName.IndexOf(contentStr);
                string temp = fileName.Substring(strIndex + contentStr.Length);
                temp = temp.Substring(0, temp.Length - suffixStr.Length);
                fileNum = long.Parse(temp);
                if (fileNum >= index.CurrentItemContentDataStartFileNumber)
                {
                    VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemContentDataFilePrefixNumber, fileNum, FileType.Content);
                }
            }
            else if (fileName.Contains(metaStr))
            {
                int strIndex = fileName.IndexOf(metaStr);
                string temp = fileName.Substring(strIndex + metaStr.Length);
                temp = temp.Substring(0, temp.Length - suffixStr.Length);
                fileNum = long.Parse(temp);
                if (fileNum >= index.CurrentItemMetaDataStartFileNumber)
                {
                    VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemMetaDataFilePrefixNumber, fileNum, FileType.MetaData);
                }
            }

            return fileNum;
        }
        public String Generate(FileNameParameter param)
        {
            var fileName = default(String);
            switch (param.FileType)
            {
                case FileType.MetaData:
                    fileName = GenerateMetaFileName(param);
                    break;
                case FileType.Content:
                    fileName = GenerateContentFileName(param);
                    break;
                default:
                    throw new Exception(string.Format("Unknown file type {0}", param.FileType.ToString()));
            }
            return fileName;
        }
        public string GenerateContentFileName(FileNameParameter param)
        {
            return param.JobID + "_content_" + param.FileNumber + ".dat";
        }

        public string GenerateMetaFileName(FileNameParameter param)
        {
            return param.JobID + "_meta_" + param.FileNumber + ".dat";
        }
        private void VerifyAndCopyArchiverToHot(string backupJobId, Int64 prefixNumber, Int64 fileNumber, FileType fileType)
        {
            BlobContainerClient blobClient = blobClientDic.ContainsKey(backupJobId)?blobClientDic[backupJobId]:null;
            if (blobClient == null)
            {
                var msIndex = fsMasterIndex.Where(i => backupJobId.StartsWith(i.JobId)).ToList();
                if (msIndex != null && msIndex.Count > 0)
                {
                    string storageId = msIndex[0].SubInfo.Where(s => s.JobId == backupJobId).Select(s => s.CurrentStorageId).FirstOrDefault();
                    var tempXri = DataLogicalDeviceList.Where(s => s.Id.Equals(storageId, StringComparison.OrdinalIgnoreCase)).ToList().FirstOrDefault().GetXRIS(PhysicalDeviceUsage.Data)[0];
                    blobClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(tempXri);
                }
                else
                {
                    logger.Warn($"The master index of job {backupJobId} is not found.");
                    return;
                }
                blobClientDic.Add(backupJobId, blobClient);
            }

            var contentDataparam = new DataBlockOpenParam
            {
                FileType = fileType,
                JobId = backupJobId,
                PrefixNumber = prefixNumber,
                FileNumber = fileNumber,
            };
            var contentName = Generate(new FileNameParameter(contentDataparam));

            StorageInfo info = new StorageInfo { HighName = DataVolume, LowName = contentName };
            //var file = this.dataLogicalDevice.OpenFile(info);
            var blob = GetBlobAccessTier(info.HighName + "\\" + info.LowName, backupJobId);
            if (blob.GetProperties().Value.AccessTier == AccessTier.Archive)
            {
                if (!BLOBMappings.ContainsKey(info.HighPlusLowName))
                {
                    SetSelectToHotTier(info, backupJobId);
                    BLOBRehydrationMapping mapping = new BLOBRehydrationMapping()
                    {
                        AlreadyRehydration = false,
                        MappedBlobInfo = info,
                        StartTime = DateTime.Now,
                        backupjobId = backupJobId
                    };
                    BLOBMappings.Add(info.HighPlusLowName, mapping);
                }
            }
        }
        private BlobClient GetBlobAccessTier(string blobName,string backupJobId)
        {

            if (blobClientDic[backupJobId] != null)
            {
                var blobClient = blobClientDic[backupJobId].GetBlobClient(blobName);
                return blobClient;
            }

            return null;
        }
        private void SetSelectToHotTier(StorageInfo info,string backupJobId)
        {
            logger.Info($"SetSelectToTier Begin,info.LowName:{info.LowName.LogBase64()}");

                if (blobClientDic[backupJobId] != null)
                {
                    var blobClient = blobClientDic[backupJobId].GetBlobClient(info.HighName + "\\" + info.LowName);
                    if (blobClient.Exists())
                    {
                        blobClient.SetAccessTierAsync(AccessTier.Hot).GetAwaiter().GetResult();
                    }
                    else
                    {
                        logger.Warn($"The blob {info.LowName.LogBase64()} does not exist.");
                    }
                }

            logger.Info($"SetSelectToTier End,info.LowName:{info.LowName.LogBase64()}");
        }
        private void AddReport(string attributes,string extraInfo,string name,long size,JobDetailsStatus status, string comment = null)
        {
            string tempExtraInfo = string.IsNullOrEmpty(extraInfo) ? "" : extraInfo + "\\";
            string path = attributes + "\\" + tempExtraInfo + name;
            JMFSRestoreJobDetails detail = new JMFSRestoreJobDetails()
            {
                SourceLocation = path,
                Size = size.ToString(),
                FinishTime = DateTime.UtcNow.Ticks,
                Status = status,
                Comment = comment,
            };
            JobDetailService.Commit(detail);
            StatisticRestoreSummary(detail);
        }

        private void AddSummaryReport()
        {
            try
            {
                JMAgentFSJMRestoreSummaryDetails restoreSummaryDetails = new JMAgentFSJMRestoreSummaryDetails();
                restoreSummaryDetails.ActionStatistics = new List<AgentActionStatistics> { _RestoreStatistics };
                JobDetailService.Commit(restoreSummaryDetails);
            }
            catch (Exception ex) 
            {
                logger.Error($@"Fail add summary report, ex:{ex}");
            }
        }

        private void StatisticRestoreSummary(JMFSRestoreJobDetails detail)
        {
            switch (detail.Status) 
            {
                case JobDetailsStatus.Successful:
                    _RestoreStatistics.SuccessfulObj.ItemCount++;
                    _RestoreStatistics.Size += long.Parse(detail.Size);
                    break;
                case JobDetailsStatus.Skipped:
                    _RestoreStatistics.SkippedObj.ItemCount++;
                    break;
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Exception:
                default:
                    _RestoreStatistics.FailedObj.ItemCount++;
                    break;
            }
        }

        private void WriteDataToFile(StorageInfo info, ArchiverBasicIndex index)
        {
            Boolean isFileExists = this.RestoreLocationManager.CacheSystem.FileExists(info);
            byte[] buffer = new byte[64 * 1024];
            try
            {
                DataReader.GetNextItem(index);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("This operation is not permitted on an archived blob."))
                {
                    logger.Warn("Storage blob file has been archived,try Rehydration Data.");
                    throw;
                }
                throw;
            }
            if (!DataReader.Input.HasContent)
            {
                //this.itemDetailMessage.Status = 0;
            }
            //else if ((isFileExists && this.archiverRestoreJob.ArchiveRestoreOption != RestoreOption.OverWrite))
            //{
            //    //this.itemDetailMessage.Status = 2;
            //}
            logger.Info($"Archiver Restore ToFS Service Write Data To File Write,index.PathMD5:{index.PathMD5},info.HighName:{info.HighName.LogBase64()}");
            if (!isFileExists || FSJobCache.RestoreInstance.FSRestoreOption == RestoreOption.Append || FSJobCache.RestoreInstance.FSRestoreOption == RestoreOption.OverWrite)
            {
                FileMode mode = FileMode.CreateNew;
                if (isFileExists)
                {
                    switch (FSJobCache.RestoreInstance.FSRestoreOption)
                    {
                        case RestoreOption.Append:
                            bool tempExist = false;
                            int i = 1;
                            while (true)
                            {
                                string fileExtention = info.LowName.Contains(".") ? info.LowName.Substring(info.LowName.LastIndexOf(".")):"";
                                string fileName = info.LowName.Contains(".")? info.LowName.Substring(0,info.LowName.LastIndexOf("."))+"_"+i: info.LowName+"_"+i;
                                StorageInfo tempInfo = new StorageInfo() { HighName = info.HighName, LowName = fileName+ fileExtention };
                                tempExist = this.RestoreLocationManager.CacheSystem.FileExists(tempInfo);
                                if (tempExist)
                                {
                                    logger.Info($"file exist,need to create new file name,i:{i}");
                                    i++;
                                }
                                else
                                {
                                    info.LowName = fileName+ fileExtention;
                                    break;
                                }
                            }
                            mode = FileMode.CreateNew;
                            break;
                        case RestoreOption.OverWrite:
                            mode = FileMode.Truncate;
                            break;
                        case RestoreOption.NotOverWrite:
                            mode = FileMode.CreateNew;
                            break;
                    }
                }
                else
                {
                    mode = FileMode.CreateNew;
                }
                using (XStream stream = this.RestoreLocationManager.CacheSystem.OpenStream(info, mode))
                {
                    if (index.ContentLength != 0L)
                    {
                        DataReader.Input.BeginRead(FileType.Content);
                        while (true)
                        {
                            int len = DataReader.Input.ReadContent(buffer, 0, buffer.Length);
                            if (len <= 0) break;
                            stream.Write(buffer, 0, len);
                        }
                        DataReader.Input.EndRead(FileType.Content);
                        index.FileRealSize = stream.Length;
                        stream.Flush();
                        //this.itemDetailMessage.Status = 0;
                    }
                }
                AddReport(index.Attributes, index.ExtraInfo, index.Name, index.ContentLength, JobDetailsStatus.Successful);
            }
            else
            {
                logger.Info($"skip current file ,because the conflict option is skip and the file exist:{index.Url.LogBase64()}");
                AddReport(index.Attributes, index.ExtraInfo, index.Name, index.ContentLength, JobDetailsStatus.Skipped);
            }
        }
    }
}
