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
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    using GCommon.Contract.Media.TCPRequest;
    using Merged18NResources.MediaServiceApplicationModel;
    using Restore;
    using System;
    using System.Reflection;
    using System.Threading;
    using System.IO;
    using System.Collections.Generic;
    using Storage;
    using global::Media.Common;
    using Storage.Cloud.Azure;
    using global::Media.Common.ClassicStorageApi;
    using AvePoint.GCommon.Utility;

    #endregion using directives
    public class EndUserArchiverRestoreService : IEndUserArchiverRestoreService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Int64 maxItemNum;
        Int64 sendItemNum = 0;
        String siteCollectionNewestUrl;
        IXSystem dataLogicalDevice;
        IXSystem indexLogicalDevice;
        JobProgressInfo jobProgressInfo;
        RestoreJobPolicy restoreJobPolicy;
        ArchiverFileSender fileSender = new ArchiverFileSender();
        ArchiverRestoreJob restoreJob;
        String errorDetailMessage = String.Empty;


        public ICacheService CacheManager { get; set; }

        public IVerifyDataReader VerifyDataReader { get; set; }

        public IJobProgressUpdater JobProgressUpdater { get; set; }

        public IRestoreServiceTreeHandler TreeHandler { get; set; }

        public IStorageDeviceManager StorageDeviceManager { get; set; }

        public IDataReader<ArchiverRestoreJob> DataReader { get; set; }

        public IEncryptionInfoManager EncryptionInfoManager { get; set; }

        public IStreamOpenTypeGenerator StreamOpenTypeGenerator { get; set; }

        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IRestoreJobRunningPolicyChecker RestoreJobRunningPolicyChecker { get; set; }

        public IFileNameGeneratorFactory FileNameGeneratorFactory { get; set; }

        public Action<long> UpdateProgress { get; set; }
        private IFileNameGenerator fileNameGenerator;
        public bool HasBlobInArchiverTier
        {
            get { return BLOBMappings.Count > 0; }
        }

        public bool BlockedRestoreArchiveTierData = false;
        private SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();
        private List<string> AllScanedBLOBs = new List<string>();
        private string rehydrationTemp;
        private readonly object rehydrationLock = new object();

        public void HandleRequest(MediaTCPRequest request, ArchiverRestoreDataBlockManger restoreDataBlockManager, Action<long> updateProgress)
        {
            try
            {
                this.restoreJob = Activator.CreateInstance(typeof(ArchiverRestoreJob), request) as ArchiverRestoreJob;
                this.UpdateProgress = updateProgress;
                this.fileSender.RestoreDataBlockManger = (ArchiverRestoreDataBlockManger)restoreDataBlockManager;
                this.Open();
                //this.fileSender.RestoreDataBlockManger = (GranularRestoreDataBlockManger)restoreDataBlockManager;
                Thread restoreThread = new Thread(Restore) { IsBackground = true };
                restoreThread.Start();
            }
            catch (Exception e)
            {
                //errorMessage = CatchHelper.ProcessException(e);
                this.errorDetailMessage = e.Message;
                this.logger.Error(MediaServiceApplicationModelResource.RestoreServiceBaseHandleRequestRestoreError, e.ToString());
                throw;
            }
            finally
            {
                //this.Close(this.errorDetailMessage);
            }
        }
        void Open()
        {
            this.restoreJobPolicy = new RestoreJobPolicy(this.restoreJob);
            this.RestoreJobRunningPolicyChecker.SetPolicy(restoreJobPolicy);
            this.restoreJobPolicy.JobStatus = JobStatus.Stopping;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceOpenBegin);
            this.fileNameGenerator = FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, DataVersion.Data6000);
            this.rehydrationTemp = SecurityUtils.SafeCombinePath("data_archive", "Temp" + Guid.NewGuid());
            this.restoreJob.LogicalDevice = new LogicalDeviceDto();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceOpenAfterCut, Environment.NewLine, this.restoreJob.TreeRoot);

            this.restoreJob.DataLogicalDeviceList.ForEach(logicalDevice =>
            {
                logicalDevice.PhysicalDrives.ForEach(physicalDevice =>
                {
                    logger.Debug(physicalDevice.Name);
                    this.restoreJob.LogicalDevice.PhysicalDrives.Add(physicalDevice);
                });
            });
            this.dataLogicalDevice = this.StorageDeviceManager.Open(this.restoreJob.LogicalDevice.ToXRIS());
            this.indexLogicalDevice = this.StorageDeviceManager.Open(this.restoreJob.IndexLogicalDevice.ToXRIS());
            this.CacheManager.Open(this.restoreJob.CacheSetting, this.dataLogicalDevice.IsDirectSystem);

            this.DataReader.Open(this.restoreJob);
            var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(this.restoreJob.RestoreSecurityInfos);
            DataReader.SetEncryptionInfos(encryptionInfoDic);

            this.jobProgressInfo = new JobProgressInfo();
            if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
            {
                this.jobProgressInfo.IsFinal = true;
                this.logger.Info("Current restore job is stopped.");
            }
            else
            {
                jobProgressInfo.Id = this.restoreJob.JobId;
                jobProgressInfo.IsSubJob = this.restoreJob.JobId.IndexOf("_", StringComparison.OrdinalIgnoreCase) == -1 ? false : true;
                this.UpdateProgress(1);
            }
        }

        void Restore()
        {
            if (!this.jobProgressInfo.IsFinal)
            {
                bool needSendCloseBlock = true;
                try
                {
                    ArchiverIndexServiceOpenParameter indexOpenParam = new ArchiverIndexServiceOpenParameter();
                    indexOpenParam.TreeMode = this.restoreJob.TreeMode;
                    indexOpenParam.IndexVolume = this.restoreJob.IndexVolume;
                    indexOpenParam.BackupJobId = this.restoreJob.BackupJobId;
                    indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
                    indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.restoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
                    indexOpenParam.CacheSetting = this.restoreJob.CacheSetting;
                    indexOpenParam.CheckAccessTier = this.restoreJob.CheckAccessTier;
                    if (this.restoreJob.EndUserRequestItems.Count == 1 && !string.IsNullOrEmpty(this.restoreJob.EndUserRequestItems[0].BackUpJobId))
                    {
                        indexOpenParam.TreeMode = TreeMode.JobMode;
                        indexOpenParam.BackupJobId = this.restoreJob.EndUserRequestItems[0].BackUpJobId;
                    }
                    this.IndexService.Open(indexOpenParam);
                    var siteCollectionNode = this.GetSiteCollectionTreeNode(this.restoreJob.TreeRoot);
                    this.siteCollectionNewestUrl = siteCollectionNode.Name;
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCalculateTotalItemNumBegin);
                    var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = siteCollectionNode, RestoreJob = this.restoreJob, IsJustCalculateCount = true };
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCalculateTotalItemNumEnd, maxItemNum);

                    this.logger.Info("Start statistics restore data in ArchiverTier.");
                    restoreTreeHandlerParam.IsJustCalculateCount = false;
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
                    this.logger.Info("The restored data count in ArchiverTier is {0}.AllScanedBLOBs:{1}.", BLOBMappings.Count, AllScanedBLOBs.Count);
                    if (BLOBMappings.Count > 0)
                    {
                        if (restoreJob.IsEndUserRestore && !restoreJob.IsEndUserRestoreAccessTier)
                        {
                            BlockedRestoreArchiveTierData = true;
                            throw new DataInArchiveTierException($"IsEndUserRestoreAccessTier {restoreJob.IsEndUserRestoreAccessTier},BLOBMappingsCount:{BLOBMappings.Count}");
                        }
                        //Add message to job summary comment

                        //Waiting Rehydration
                        WaitingRehydration();
                        //Add Blob mappings to ArchiverRestoreDataReader
                        if (this.DataReader is ArchiverRestoreDataReader)
                        {
                            (this.DataReader as ArchiverRestoreDataReader).SettingMappings(BLOBMappings);
                        }
                    }

                    restoreTreeHandlerParam.IsJustCalculateCount = false;

                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);

                }
                catch (JobNeedStopException)
                {
                    needSendCloseBlock = false;
                    this.jobProgressInfo.IsFinal = true;
                    this.UpdateProgress((Int32)((this.sendItemNum * 1.0 / this.maxItemNum) * 100));
                    //this.JobProgressUpdater.UpdateJobProgress(jobProgressInfo, maxItemNum, sendItemNum);
                    this.logger.Info("Current restore job is stopped.");
                }
                catch (DataInArchiveTierException archivetierexception)
                {
                    needSendCloseBlock = false;
                    this.logger.Error("restore failed:{0}", archivetierexception.ToString());
                    this.fileSender.Close(String.Empty);
                }
                catch (Exception ex)
                {
                    needSendCloseBlock = false;
                    errorDetailMessage = ex.Message;
                    this.logger.Error("restore failed:{0}", ex);
                    this.fileSender.Close(errorDetailMessage);
                }
                if (needSendCloseBlock)
                    this.fileSender.Close(String.Empty);
            }
        }

        private void WaitingRehydration()
        {
            DateTime time = DateTime.Now;
            while (true)
            {
                if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
                {
                    throw new JobNeedStopException();
                }
                bool needContinueSleep = false;
                foreach (var r in BLOBMappings)
                {
                    if (!r.Value.AlreadyRehydration)
                    {
                        var file = this.dataLogicalDevice.OpenFile(r.Value.MappedBlobInfo);
                        if (file is AzureCloudInfo)
                        {
                            var azureFile = file as AzureCloudInfo;
                            if (!file.Exists || azureFile.FileTierType == Storage.AccessTierType.Archive)
                            {
                                logger.Info($"The {r.Key} need to rehydration, " +
                                    $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                    $"Exists:{file.Exists} , " +
                                    $"start time : {r.Value.StartTime.ToString()}");
                                needContinueSleep = true;
                                break;
                            }
                            else
                            {
                                logger.Info($"The {r.Key} already rehydration, " +
                                    $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                    $"Exists:{file.Exists} , " +
                                    $"start time : {r.Value.StartTime.ToString()}");
                                r.Value.AlreadyRehydration = true;
                            }
                        }
                    }
                }

                if (needContinueSleep && DateTime.Now - time < TimeSpan.FromDays(5))
                {
                    logger.Info("Will sleep 15 min to wait blob rehydration.");
                    Thread.Sleep(15 * 60 * 1000);
                }
                else
                {
                    logger.Info($"Exit waiting blob rehydration, all the datas rehydration : {!needContinueSleep} .");
                    break;
                }
            }
        }

        void Close(String errorMessage)
        {
            if (!this.jobProgressInfo.IsFinal)
            {
                this.jobProgressInfo.IsFinal = true;
                this.UpdateProgress(100);
                //this.JobProgressUpdater.UpdateJobProgress(this.jobProgressInfo, 100, 100, true);
            }
            try
            {
                if (rehydrationTemp.Contains("Temp"))
                {
                    StorageInfo rehydrationTempInfo = new StorageInfo() { HighName = rehydrationTemp };
                    this.dataLogicalDevice.DeleteDirectory(rehydrationTempInfo);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting rehydration temp folder. error:{0}", e.ToString());
            }
            this.DataReader.Close();
            this.IndexService.Close();
            this.fileSender.Close(errorMessage);
            this.StorageDeviceManager.Close(this.dataLogicalDevice);
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.CacheManager.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCloseEnd);
        }

        private void SendIndexItemData(Object sender, IndexItemProceedEventArgs args)
        {
            this.SendItemData(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
        }

        private void CalculateIndexItemCount(Object sender, IndexItemProceedEventArgs args)
        {
            this.maxItemNum += args.IndexCount;
        }

        private void StatisticDataInArchiverTier(Object sender, IndexItemProceedEventArgs args)
        {
            VerifyDataTier(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
        }

        private SPTreeNodeDto GetSiteCollectionTreeNode(SPTreeNodeDto treeNode)
        {
            return treeNode.Children[0].Children[0].Children[0];
        }

        private void SendItemData(ArchiverBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
            {
                throw new JobNeedStopException();
            }
            var errorMessage = String.Empty;
            var encryptionInfo = String.Empty;
            if (index == null) { return; }
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceSendItemDataSendData, index.PathMD5, markMessage.VersionFlag);
            index.OpenType = this.StreamOpenTypeGenerator.GetStreamOpenType(index.CurrentItemVersion);
            try
            {
                encryptionInfo = this.DataReader.GetNextItem(index);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreServiceSendItemDataError, e.ToString());
            }
            //if (MediaConfigInfo.CommonConfigInfo.VerifyDataInRestore)
            //{
            //    VerifyDataReader.VerifyDataWithStorageCrc32(DataReader.Input);
            //    return;
            //}
            FileHeader fileHeader = new FileHeader();
            if (index.Type.EqualsIgnoreCase("P"))
            {
                fileHeader.Type = AveSharePointType.TYPE_WEB;
            }
            else
            {
                fileHeader.Type = (AveSharePointType)index.Type.ToUpperInvariant()[0];
            }
            fileHeader.ListBaseType = Convert.ToInt32(index.ListBaseType);
            fileHeader.IsFailed = Convert.ToBoolean(index.IsFailed);
            fileHeader.Path = fileHeader.Type.Equals(AveSharePointType.TYPE_SITE) ? this.siteCollectionNewestUrl : index.Name;
            if (markMessage != null)
            {
                fileHeader.Property = markMessage.Property;
                fileHeader.Security = markMessage.Security;
                fileHeader.VersionFlag = markMessage.VersionFlag;
            }
            fileHeader.ListType = index.ListType;
            fileHeader.EncryptionInfo = encryptionInfo;
            fileHeader.HeaderExtraAttribute = index.ExtraInfo;
            fileHeader.IsAppData = index.IsAppData != null && index.IsAppData.EqualsIgnoreCase("True") ? true : false;
            string headerXml = FileHeaderUtil.ToString(fileHeader);
            //logger.Info("Current item type : {0} , path : {1}", fileHeader.Type, fileHeader.Path);
            this.fileSender.WriteHead(headerXml, (byte)index.Flag, index.Crc);
            try
            {
                this.DataReader.SendData(this.fileSender);
            }
            catch (Exception e)
            {
                if (errorMessage.Equals(String.Empty))
                    errorMessage = e.Message;
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreServiceSendItemDataException, e.ToString());
            }
            this.fileSender.WriteTail(errorMessage);
            this.sendItemNum++;
            this.UpdateProgress((Int32)((this.sendItemNum * 1.0 / this.maxItemNum) * 100));
        }

        private void VerifyDataTier(ArchiverBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
            {
                throw new JobNeedStopException();
            }
            if (index == null) { return; }
            var nextIndex = RestoreIndexService.LoadNextIndex(index);
            //can get from index
            if (nextIndex != null)
            {

                #region process data files
                if (nextIndex.CurrentItemContentDataStartFileNumber > index.CurrentItemContentDataStartFileNumber)
                {

                    for (long i = index.CurrentItemContentDataStartFileNumber; i <= nextIndex.CurrentItemContentDataStartFileNumber; i++)
                    {
                        VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemContentDataFilePrefixNumber, i, FileType.Content);
                    }
                }
                else
                {
                    VerifyAndCopyArchiverToHot(index.BackupJobId, index.CurrentItemContentDataFilePrefixNumber, index.CurrentItemContentDataStartFileNumber, FileType.Content);
                }
                #endregion

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
                //cannot get file count from index, need to get from device.
                StorageInfo info = new StorageInfo() { HighName = restoreJob.DataVolume };
                try
                {
                    var list = this.dataLogicalDevice.ListFiles(info);
                    foreach (var f in list)
                    {
                        if (f.LowName.Contains(index.BackupJobId))
                        {
                            ProcessDataFile(index, f.LowName);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("some device are unavailable.error message:{0}", e.ToString());
                }
            }
        }

        private Int64 ProcessDataFile(ArchiverBasicIndex index, string fileName)
        {
            //"_content_"
            //"_meta_"
            //".dat"
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

        private void VerifyAndCopyArchiverToHot(string backupJobId, Int64 prefixNumber, Int64 fileNumber, FileType fileType)
        {
            var contentDataparam = new DataBlockOpenParam
            {
                FileType = fileType,
                JobId = backupJobId,
                PrefixNumber = prefixNumber,
                FileNumber = fileNumber,
                DataVersion = DataVersion.Data6000,
                OpenFromCache = MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache,
            };
            var contentName = fileNameGenerator.Generate(new FileNameParameter(contentDataparam));
            if (!AllScanedBLOBs.Contains(SecurityUtils.SafeCombinePath(restoreJob.DataVolume, contentName)))
            {
                StorageInfo info = new StorageInfo { HighName = restoreJob.DataVolume, LowName = contentName };
                var file = this.dataLogicalDevice.OpenFile(info);
                if (file is AzureCloudInfo)
                {
                    var azureFile = file as AzureCloudInfo;
                    if (file != null && azureFile.FileTierType == AccessTierType.Archive)
                    {
                        string temp = SecurityUtils.SafeCombinePath(rehydrationTemp, restoreJob.DataVolume.Substring(restoreJob.DataVolume.IndexOf("DataVolume") + 11));
                        lock (rehydrationLock)
                        {
                            if (!BLOBMappings.ContainsKey(SecurityUtils.SafeCombinePath(restoreJob.DataVolume, contentName)))
                            {
                                azureFile.FileTierType = AccessTierType.Archive;
                                AzureCloudInfo info2 = new AzureCloudInfo { HighName = temp, LowName = contentName, FileTierType = AccessTierType.Hot };
                                var res = this.dataLogicalDevice.CopyFile(azureFile, info2, true);
                                if (res.IsCopyed)
                                {
                                    BLOBRehydrationMapping mapping = new BLOBRehydrationMapping()
                                    {
                                        AlreadyRehydration = false,
                                        MappedBlobInfo = info2,
                                        StartTime = DateTime.Now
                                    };
                                    BLOBMappings.Add(SecurityUtils.SafeCombinePath(restoreJob.DataVolume, contentName), mapping);
                                }
                            }
                        }
                    }
                }
                logger.Info($"VerifyAndCopyArchiverToHot AllScanedBLOBs: {SecurityUtils.SafeCombinePath(restoreJob.DataVolume, contentName)}.");
                AllScanedBLOBs.Add(SecurityUtils.SafeCombinePath(restoreJob.DataVolume, contentName));

            }
        }

        public void Dispose()
        {
            this.Close(this.errorDetailMessage);
        }
    }
}
