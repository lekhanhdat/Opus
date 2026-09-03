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
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.ArchiverBackup.Restore;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.DomainModel.DocAve60x;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.JobMonitor;
    using AvePoint.Wrapper.Common;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using Media.Service.ArchiverBackup.Index;
    using Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntentionImpl;
    using Merged18NResources.MediaServiceApplicationModel;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Storage;
    using Storage.Cloud.Azure;
    using Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using FileHeader = RAGoogle.Restore.Content.FileHeader;
    #endregion using directives

    public class GDriveArchiverRestoreService : IDisposable
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Int64 maxItemNum;
        Int64 sendItemNum = 0;
        bool restoreIsStopping = false;
        String siteCollectionNewestUrl;
        IXSystem dataLogicalDevice;
        IXSystem indexLogicalDevice;
        JobProgressInfo jobProgressInfo;
        RestoreJobPolicy restoreJobPolicy;
        ArchiverFileSender fileSender = new ArchiverFileSender();
        GDriveRestoreJob restoreJob;
        String errorDetailMessage = String.Empty;
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

        public ICacheService CacheManager => PlatformWindsorManager.GetService<ICacheService>();

        public IVerifyDataReader VerifyDataReader { get; set; }

        public IJobProgressUpdater JobProgressUpdater { get; set; }

        public IGDriveRestoreServiceTreeHandler TreeHandler { get; set; } = new GDriveRestoreTreeHandler();

        public IStorageDeviceManager StorageDeviceManager { get; set; } = PlatformWindsorManager.GetService<IStorageDeviceManager>();

        public IDataReader<GDriveRestoreJob> DataReader { get; set; } = new GDriveRestoreDataReader();

        public IEncryptionInfoManager EncryptionInfoManager { get; set; } = PlatformWindsorManager.GetService<IEncryptionInfoManager>();

        public IStreamOpenTypeGenerator StreamOpenTypeGenerator { get { return new StreamOpenTypeGenerator(); } }

        public GDriveArchiverIndexService IndexService { get; set; }

        public IGDriveArchiverRestoreIndexService RestoreIndexService { get; set; }

        public IRestoreJobRunningPolicyChecker RestoreJobRunningPolicyChecker
        {
            get
            {
                return new RestoreJobRunningPolicyChecker();
            }
        }

        public IFileNameGenerator FileNameGenerator { get { return new ArchiverFileNameGenerator(); } }

        public Action<long> UpdateProgress { get; set; }

        public bool HasBlobInArchiverTier
        {
            get { return BLOBMappings.Count > 0; }
        }

        private SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();

        private List<string> AllScanedBLOBs = new List<string>();

        private IFileNameGenerator fileNameGenerator;
        private string rehydrationTemp;
        private readonly object rehydrationLock = new object();

        private bool hasRehydrationData = false;
        private List<string> hasRestoredItems = new List<string>();
        private List<string> backupJobIds = new List<string>();
        public void HandleRequest(MediaTCPRequest request, ArchiverRestoreDataBlockManger restoreDataBlockManager, Action<long> updateProgress)
        {
            try
            {
                var restoreRequest = request as GDriveRestoreRequest;
                this.restoreJob = new GDriveRestoreJob(restoreRequest);

                this.restoreJob.KeepVersionsNumber = restoreRequest.KeepVersionsNumber;
                this.restoreJob.RestoreVersionOption = restoreRequest.RestoreVersionsOption;
                this.UpdateProgress = updateProgress;
                this.fileSender.RestoreDataBlockManger = (ArchiverRestoreDataBlockManger)restoreDataBlockManager;
                this.Open();
                Thread restoreThread = new Thread(Restore) { IsBackground = true };
                restoreThread.Start();
            }
            catch (Exception e)
            {
                this.errorDetailMessage = e.Message;
                this.logger.Error(MediaServiceApplicationModelResource.RestoreServiceBaseHandleRequestRestoreError, e.ToString());
                //throw;
            }
            finally
            {
                if (restoreIsStopping)
                {
                    logger.Warn("restore is stopping");
                    throw new JobStopException();
                }
            }
        }

        CancellationToken cancellationToken;

        public SimulateResotreResult HandleSimulateRequest(MediaTCPRequest request, CancellationToken cancellationToken)
        {
            return simulateResotreResult;
        }

        void Open()
        {
            this.restoreJobPolicy = new RestoreJobPolicy(this.restoreJob);
            this.RestoreJobRunningPolicyChecker.SetPolicy(restoreJobPolicy);
            this.restoreJobPolicy.JobStatus = JobStatus.Stopping;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceOpenBegin);
            this.restoreJob.LogicalDevice = new LogicalDeviceDto();
            this.fileNameGenerator = FileNameGenerator;
            this.rehydrationTemp = Path.Combine("data_archive", "Temp" + Guid.NewGuid());
            //this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceOpenBeforeCut, Environment.NewLine, this.restoreJob.TreeRoot);
            //this.TreeHandler.CutTree(this.restoreJob.TreeRoot);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceOpenAfterCut, Environment.NewLine, this.restoreJob.TreeRoot);
            //this.Network.SendMessage(ServiceConstants.StringSendToAgent);
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
            if (encryptionInfoDic != null)
            {
                this.logger.Info($"Restore security infoes: {string.Join(",", encryptionInfoDic.Keys)}.");
            }
            this.DataReader.SetEncryptionInfos(encryptionInfoDic);
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

            }

        }

        void Restore()
        {
            if (!this.jobProgressInfo.IsFinal)
            {
                bool needRehydrationData = false;
                bool needSendCloseBlock = true;
                try
                {
                    IndexService = new GDriveArchiverIndexService();
                    var indexOpenParam = new GDriveIndexServiceOpenParameter();
                    indexOpenParam.TreeMode = this.restoreJob.TreeMode;
                    indexOpenParam.IndexVolume = this.restoreJob.IndexVolume;
                    indexOpenParam.BackupJobId = this.restoreJob.BackupJobId;
                    indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
                    indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.restoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
                    indexOpenParam.CacheSetting = this.restoreJob.CacheSetting;
                    indexOpenParam.CheckAccessTier = this.restoreJob.CheckAccessTier;
                    IndexService.Open(indexOpenParam);

                    RestoreIndexService = new GDriveArchiverRestoreIndexService()
                    {
                        HeadAndBodyService = new GDriveArchiverHeadAndBodyIndexService() { IndexProcessor = IndexService.IndexProcessor },
                        SiteMasterService = new GDrvieMasterIndexService() { IndexProcessor = IndexService.IndexProcessor }
                    };
                    this.TreeHandler.RestoreIndexService = RestoreIndexService;
                    this.TreeHandler.restoreJob = this.restoreJob;
                    var siteCollectionNode = this.restoreJob.GDriveTreeRoot;

                    this.siteCollectionNewestUrl = siteCollectionNode.FullPath;
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCalculateTotalItemNumBegin);
                    var restoreTreeHandlerParam = new TreeNodeParameter { GoogleDriveTree = siteCollectionNode, RestoreJob = this.restoreJob, IsJustCalculateCount = true };
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.UpdateProgress(this.maxItemNum);
                    logger.Info($"this retore job should restore count is:{this.maxItemNum}");
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCalculateTotalItemNumEnd, maxItemNum);

                    //RehydrationData();

                    restoreTreeHandlerParam.IsJustCalculateCount = false;
                    try
                    {
                        this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                        this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                        this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                    }
                    catch (BlobArchivedException e)
                    {
                        logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                        this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                        needRehydrationData = true;
                    }
                    catch (SkipRetryException e)
                    {
                        if (e.Message.Contains("This operation is not permitted on an archived blob."))
                        {
                            logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                            needRehydrationData = true;
                        }
                    }
                    if (needRehydrationData)
                    {
                        hasRehydrationData = true;
                        RehydrationData();
                        restoreTreeHandlerParam.IsJustCalculateCount = false;
                        this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                        this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                        this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                    }
                }
                catch (JobStopException e)
                {
                    logger.Warn("Job will stop,stop Rehydration and delete Temp folder");
                }
                catch (JobNeedStopException)
                {
                    needSendCloseBlock = false;
                    this.jobProgressInfo.IsFinal = true;
                    //this.JobProgressUpdater.UpdateJobProgress(jobProgressInfo, maxItemNum, sendItemNum);
                    this.logger.Info("Current restore job is stopped.");
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
            WrapperConfiguration.NeedToUploadIndex = false;//ArchiverIndexSubInfoDao.CheckExistSoftInfoAndUpdateThem(backupJobIds);
        }

        void Close(String errorMessage)
        {
            if (!this.jobProgressInfo.IsFinal)
            {
                this.jobProgressInfo.IsFinal = true;
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
            if (!CheckJobStatusUtility.isStopping)
            {
                this.fileSender.Close(errorMessage);
            }
            this.StorageDeviceManager.Close(this.dataLogicalDevice);
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.CacheManager.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCloseEnd);
        }

        private void RehydrationData()
        {
            //if (needCheckArchiverTier)
            //{
            AllScanedBLOBs.Clear();
            this.logger.Info("Start statistics restore data in ArchiverTier.");
            var restoreTreeHandlerParam = new TreeNodeParameter { GoogleDriveTree = this.restoreJob.GDriveTreeRoot, RestoreJob = this.restoreJob, IsJustCalculateCount = false };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
            this.logger.Info("The restored data count in ArchiverTier is {0}.AllScanedBLOBs:{1}.", BLOBMappings.Count, AllScanedBLOBs.Count);
            //}
            try
            {
                if (BLOBMappings.Count > 0)
                {
                    //Add message to job summary comment

                    //Waiting Rehydration
                    WaitingRehydration();
                    //Add Blob mappings to ArchiverRestoreDataReader
                    if (this.DataReader is GDriveRestoreDataReader)
                    {
                        (this.DataReader as GDriveRestoreDataReader).SettingMappings(BLOBMappings);
                    }
                }
            }
            catch (JobStopException e)
            {
                logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }
        }

        private void SendIndexItemData(Object sender, IndexItemProceedEventArgs args)
        {
            this.SendItemData(args.IndexItem as GoogleBasicIndex, args.MarkMessage);
        }

        private SimulateResotreResult simulateResotreResult = new SimulateResotreResult()
        {
            LevelCountMap = new Dictionary<int, long>()
            {
                {(int)PolicyLevel.SiteCollection, 0 },
                {(int)PolicyLevel.Site, 0 },
                {(int)PolicyLevel.List, 0 },
                {(int)PolicyLevel.Folder, 0 },
                {(int)PolicyLevel.Document, 0 },
                {(int)PolicyLevel.DocumentVersion, 0 },
                {(int)PolicyLevel.Item, 0 },
                {(int)PolicyLevel.ItemVersion, 0 },
                {(int)PolicyLevel.Attachment, 0 },
                {(int)PolicyLevel.None, 0 }
            }
        };


        private void CalculateIndexItemCount(Object sender, IndexItemProceedEventArgs args)
        {
            this.maxItemNum += args.IndexCount;
        }

        private void StatisticDataInArchiverTier(Object sender, IndexItemProceedEventArgs args)
        {
            VerifyDataTier(args.IndexItem as GoogleBasicIndex, args.MarkMessage);
        }

        private void SendItemData(GoogleBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (!backupJobIds.Contains(index.JobId))
            {
                backupJobIds.Add(index.JobId);
            }
            if (hasRestoredItems.Contains(index.PathMD5))
            {
                logger.Info($"this item has restored,pathMd5:{index.PathMD5}");
                return;
            }
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
            catch (BlobArchivedException e)
            {
                if (!hasRehydrationData)
                {
                    logger.Warn($"Storage blob file has been archived,try Rehydration Data.");
                    throw;
                }
                else
                {
                    logger.Warn($"Storage blob file has been archived.");
                }
            }
            catch (SkipRetryException e)
            {
                if (e.Message.Contains("This operation is not permitted on an archived blob."))
                {
                    if (!hasRehydrationData)
                    {
                        logger.Warn($"Storage blob file has been archived,try Rehydration Data.");
                        throw;
                    }
                    else
                    {
                        logger.Warn($"Storage blob file has been archived.");
                    }
                }
                else
                {
                    errorMessage = e.Message;
                    this.logger.Error($"SkipRetryException process data failed,error:{e.ToString()}");
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreServiceSendItemDataError, e.ToString());
            }

            FileHeader fileHeader = new FileHeader();
            fileHeader.Type = index.Type;
            fileHeader.Path = fileHeader.Type.Equals((int)GDriveDataType.MyDrive) || fileHeader.Type.Equals((int)GDriveDataType.SharedDrive) ? this.siteCollectionNewestUrl : index.Path;
            fileHeader.Name = index.Name;
            if (markMessage != null)
            {
                fileHeader.Property = markMessage.Property;
                fileHeader.Security = markMessage.Security;
                fileHeader.VersionFlag = markMessage.VersionFlag;
                fileHeader.IsSelect = markMessage.IsSelected;
                fileHeader.ParentIsSelect = markMessage.ParentIsSelected;
            }
            fileHeader.EncryptionInfo = encryptionInfo;
            //fileHeader.HeaderExtraAttribute = index.ExtraInfo;
            fileHeader.ArchiveTime = index.ArchiveTime;
            fileHeader.IsAppData = false;
            fileHeader.Id = index.ItemId;
            fileHeader.StorageId = index.StoragePolicyId ?? string.Empty;
            fileHeader.BackUpJobId = index.JobId;
            fileHeader.ItemPathMD5 = index.PathMD5;
            fileHeader.DriveId = index.DriveId;
            fileHeader.DriveName = index.DriveName;
            fileHeader.ParentId = index.ParentId;
            fileHeader.VersionNumber = index.VersionNumber;
            string headerXml = FileHeader.ToXmlString(fileHeader);
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
            if (index.Type == (int)GDriveDataType.File || index.Type == (int)GDriveDataType.FileVersion)
            {
                hasRestoredItems.Add(index.PathMD5);
            }

        }

        private void VerifyDataTier(GoogleBasicIndex index, RestoreMarkMessage markMessage)
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

                if (index.ContentLength > 0)
                {
                    var nextBodyIndex = nextIndex;
                    if (nextBodyIndex.ContentLength == 0)
                    {
                        nextBodyIndex = RestoreIndexService.LoadNextBodyIndex(index);
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
        private void VerifyAllData(GoogleBasicIndex index)
        {
            StorageInfo info = new StorageInfo() { HighName = restoreJob.DataVolume };
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
        private Int64 ProcessDataFile(GoogleBasicIndex index, string fileName)
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
                                StorageCopyResult res = new StorageCopyResult();
                                if (this.dataLogicalDevice is XLibrary)
                                {
                                    try
                                    {
                                        //if ((this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID.EqualsIgnoreCase(DEFAULTSTORAGEID))
                                        //{
                                        //    var client = Util.MSAzure.StorageUtil.GetContainerClient(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.DEFAULT_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId);
                                        //    var scrBlobClient = client.GetBlobClient(info.HighPlusLowName);
                                        //    var desBlobClient = client.GetBlobClient(info2.HighPlusLowName);
                                        //    BlobCopyFromUriOptions opt = new BlobCopyFromUriOptions();
                                        //    opt.AccessTier = AccessTier.Hot;
                                        //    var APIRes = desBlobClient.StartCopyFromUri(scrBlobClient.Uri, opt);
                                        //    res.IsCopyed = true;
                                        //}
                                        //else
                                        {
                                            res = this.dataLogicalDevice.CopyFile(azureFile, info2, true);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"some thing went wrong when copy file,storage id:{(this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID}");
                                        res = this.dataLogicalDevice.CopyFile(azureFile, info2, true);
                                    }
                                    //if (lib.GetWorkingSystem().Properties)
                                    //    var client = Util.MSAzure.StorageUtil.GetContainerClient(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.DEFAULT_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId);
                                    //var blobclient = client.GetBlobClient("temp");
                                    //blobclient.StartCopyFromUri()
                                }
                                else
                                {
                                    res = this.dataLogicalDevice.CopyFile(azureFile, info2, true);
                                }
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

        private void WaitingRehydration()
        {
            DateTime time = DateTime.Now;
            try
            {
                while (true)
                {
                    if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
                    {
                        throw new JobNeedStopException();
                    }
                    bool needContinueSleep = false;
                    foreach (var r in BLOBMappings)
                    {
                        using (new CheckJobStopScope()) { }
                        if (!r.Value.AlreadyRehydration)
                        {
                            var file = this.dataLogicalDevice.OpenFile(r.Value.MappedBlobInfo);
                            if (file is AzureCloudInfo)
                            {
                                var azureFile = file as AzureCloudInfo;
                                if (!file.Exists || azureFile.FileTierType == AccessTierType.Archive)
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
            catch (JobStopException e)
            {
                logger.Warn("Job will stop,stop Rehydration.");
                restoreIsStopping = true;
                throw;
            }
        }

        public void Dispose()
        {
            this.Close(this.errorDetailMessage);
        }
    }

    public class BLOBRehydrationMapping
    {
        public bool AlreadyRehydration;
        public StorageInfo MappedBlobInfo;
        public DateTime StartTime;
    }
}