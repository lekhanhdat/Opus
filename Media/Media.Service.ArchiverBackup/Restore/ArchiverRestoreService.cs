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
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.DomainModel.DocAve60x;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.JobMonitor;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.RACommonUtility.Common;
    using AvePoint.Wrapper.Common;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Cloud.Sdk.EDiscovery.Services;
    using GCommon.Contract.Media.TCPRequest;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using Merged18NResources.MediaServiceApplicationModel;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Restore;
    using Storage;
    using Storage.Cloud.Azure;
    using Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using Util;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
    using static Google.Apis.Storage.v1.ObjectsResource;

    #endregion using directives

    public class ArchiverRestoreService
        : IArchiverRestoreService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Int64 maxItemNum;
        Int64 sendItemNum = 0;
        bool restoreIsStopping=false;
        String siteCollectionNewestUrl;
        IXSystem dataLogicalDevice;
        IXSystem indexLogicalDevice;
        JobProgressInfo jobProgressInfo;
        RestoreJobPolicy restoreJobPolicy;
        ArchiverFileSender fileSender = new ArchiverFileSender();
        ArchiverRestoreJob restoreJob;
        String errorDetailMessage = String.Empty;
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

        public ICacheService CacheManager { get; set; }

        public IVerifyDataReader VerifyDataReader { get; set; }

        public IJobProgressUpdater JobProgressUpdater { get; set; }

        public IRestoreServiceTreeHandler TreeHandler { get; set; }

        public IStorageDeviceManager StorageDeviceManager { get; set; }

        public IDataReader<ArchiverRestoreJob> DataReader { get; set; }

        public IEncryptionInfoManager EncryptionInfoManager { get; set; }

        public IStreamOpenTypeGenerator StreamOpenTypeGenerator { get { return new StreamOpenTypeGenerator(); } }

        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IRestoreJobRunningPolicyChecker RestoreJobRunningPolicyChecker
        {
            get {
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
                var restoreRequest = request as ArchiverRestoreRequest;
                this.restoreJob = Activator.CreateInstance(typeof(ArchiverRestoreJob), request) as ArchiverRestoreJob;
                this.restoreJob.KeepVersionsNumber = restoreRequest.KeepVersionsNumber;
                this.restoreJob.RestoreVersionOption = restoreRequest.RestoreVersionsOption;
                this.restoreJob.JobType = request.JobType;
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
                if(restoreIsStopping)
                {
                    logger.Warn("restore is stopping");
                    throw new JobStopException();
                }
                //this.Close(this.errorDetailMessage);
            }
        }

        CancellationToken cancellationToken;
        Action<int, long> onItemProcessedCallback;

        public void HandlePreviewRequest(MediaTCPRequest request, CancellationToken cancellationToken, Action<int, long> onItemProcessed)
        {
            this.cancellationToken = cancellationToken;
            this.onItemProcessedCallback = onItemProcessed;
            var restoreRequest = request as ArchiverRestoreRequest;
            this.restoreJob = Activator.CreateInstance(typeof(ArchiverRestoreJob), request) as ArchiverRestoreJob;
            //this.restoreJob.RestoreVersionOption = RestoreDocumentVersionsOption.AllVersions;
            this.restoreJob.KeepVersionsNumber = restoreRequest.KeepVersionsNumber;
            this.restoreJob.RestoreVersionOption = restoreRequest.RestoreVersionsOption;
            this.Open();
            ArchiverIndexServiceOpenParameter indexOpenParam = new ArchiverIndexServiceOpenParameter();
            indexOpenParam.TreeMode = this.restoreJob.TreeMode;
            indexOpenParam.IndexVolume = this.restoreJob.IndexVolume;
            indexOpenParam.BackupJobId = this.restoreJob.BackupJobId;
            indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
            indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.restoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            indexOpenParam.CacheSetting = this.restoreJob.CacheSetting;
            indexOpenParam.CheckAccessTier = this.restoreJob.CheckAccessTier;
            this.IndexService.Open(indexOpenParam);
            var previewSiteCollectionNode = this.restoreJob.TreeRoot;
            if (previewSiteCollectionNode.SPVersion >= (Int32)AveSPVersion.SharePoint2013)
            {
                this.logger.Info("Archiver restore service restore app data begin");
                (this.TreeHandler as ArchiverRestoreTreeHandler)?.ProcessTreeNodeForApps(previewSiteCollectionNode, this.restoreJob);
            }
            this.siteCollectionNewestUrl = previewSiteCollectionNode.SitePath;

            var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = previewSiteCollectionNode, RestoreJob = this.restoreJob, IsPreview = true };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(PreviewRestore);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
            this.logger.Info($"Preview restore finished, siteUrl:{this.siteCollectionNewestUrl}.");
        }

        public SimulateResotreResult HandleSimulateRequest(MediaTCPRequest request, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            var restoreRequest = request as ArchiverRestoreRequest;
            this.restoreJob = Activator.CreateInstance(typeof(ArchiverRestoreJob), request) as ArchiverRestoreJob;
            //this.restoreJob.RestoreVersionOption = RestoreDocumentVersionsOption.AllVersions;
            this.restoreJob.KeepVersionsNumber = restoreRequest.KeepVersionsNumber;
            this.restoreJob.RestoreVersionOption = restoreRequest.RestoreVersionsOption;
            this.Open();
            ArchiverIndexServiceOpenParameter indexOpenParam = new ArchiverIndexServiceOpenParameter();
            indexOpenParam.TreeMode = this.restoreJob.TreeMode;
            indexOpenParam.IndexVolume = this.restoreJob.IndexVolume;
            indexOpenParam.BackupJobId = this.restoreJob.BackupJobId;
            indexOpenParam.IndexLogicalDeviceSystem = this.indexLogicalDevice;
            indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.restoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            indexOpenParam.CacheSetting = this.restoreJob.CacheSetting;
            indexOpenParam.CheckAccessTier = this.restoreJob.CheckAccessTier;
            this.IndexService.Open(indexOpenParam);
            var siteCollectionNode = this.restoreJob.TreeRoot;
            if (siteCollectionNode.SPVersion >= (Int32)AveSPVersion.SharePoint2013)
            {
                this.logger.Info("Archiver restore service restore app data begin");
                (this.TreeHandler as ArchiverRestoreTreeHandler)?.ProcessTreeNodeForApps(siteCollectionNode, this.restoreJob);
            }
            this.siteCollectionNewestUrl = siteCollectionNode.SitePath;

            var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = siteCollectionNode, RestoreJob = this.restoreJob, IsPreview = true };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SimulateRestore);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
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
            DataReader.SetEncryptionInfos(encryptionInfoDic);
            //this.fileSender.Wrap(this.Network);
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
                //this.JobProgressUpdater.UpdateJobProgress(this.jobProgressInfo, 100, 1);
            }
            //foreach (var phy in this.restoreJob.LogicalDevice.PhysicalDrives)
            //{
            //    if (!phy.Name.StartsWith("Default Physical Device"))
            //    {
            //        needCheckArchiverTier = true;
            //    }
            //}
        }

        void Restore()
        {
            if (!this.jobProgressInfo.IsFinal)
            {
                bool needRehydrationData = false;
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
                    this.IndexService.Open(indexOpenParam);
                    var siteCollectionNode = this.restoreJob.TreeRoot;
                    if (siteCollectionNode.SPVersion >= (Int32)AveSPVersion.SharePoint2013)
                    {
                        this.logger.Info("Archiver restore service restore app data begin");
                        (this.TreeHandler as ArchiverRestoreTreeHandler).ProcessTreeNodeForApps(siteCollectionNode, this.restoreJob);
                    }
                    this.siteCollectionNewestUrl = siteCollectionNode.SitePath;
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCalculateTotalItemNumBegin);
                    var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = siteCollectionNode, RestoreJob = this.restoreJob, IsJustCalculateCount = true };
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
            // Simulate/preview restore jobs (job id prefixes "SRS"/"PRS") never set fileSender.RestoreDataBlockManger
            // because HandleSimulateRequest doesn't send any data back over the TCP connection, so closing the file
            // sender for those jobs is both unnecessary and would throw a NullReferenceException.
            if (!CheckJobStatusUtility.isStopping && !IsPreviewRestoreJob(this.restoreJob?.JobId))
            {
                this.fileSender.Close(errorMessage);
            }
            this.StorageDeviceManager.Close(this.dataLogicalDevice);
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.CacheManager.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreServiceCloseEnd);
        }

        // Job id prefixes generated for jobs that only call HandleSimulateRequest (JobMonitorService.GenerateJobId:
        // "PRS" for PreviewRestore. These jobs never open a real restore TCP connection.

        private static bool IsPreviewRestoreJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return false;
            }
            if (jobId.StartsWith("PRS", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private void RehydrationData()
        {
            //if (needCheckArchiverTier)
            //{
            AllScanedBLOBs.Clear();
            this.logger.Info("Start statistics restore data in ArchiverTier.");
            var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = this.restoreJob.TreeRoot, RestoreJob = this.restoreJob, IsJustCalculateCount = false };
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
                    if (this.DataReader is ArchiverRestoreDataReader)
                    {
                        (this.DataReader as ArchiverRestoreDataReader).SettingMappings(BLOBMappings);
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
            this.SendItemData(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
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

        private void SimulateRestore(Object sender, IndexItemProceedEventArgs args)
        {
            if(this.cancellationToken.IsCancellationRequested)
            {
                throw new JobStopException();
            }
            var archiverBasicIndex = (ArchiverBasicIndex)args.IndexItem;
            PolicyLevel level = GetPolicyLevel(archiverBasicIndex);
            long nodeCount = simulateResotreResult.LevelCountMap.GetValueOrDefault((int)level, 0);
            simulateResotreResult.LevelCountMap[(int)level] = ++nodeCount;
            simulateResotreResult.Size += archiverBasicIndex.ContentLength;
        }

        // Used by HandlePreviewRequest. Unlike SimulateRestore, this doesn't aggregate into simulateResotreResult
        // itself - it just forwards each processed item to the caller-supplied callback, which is expected to do
        // its own aggregation (e.g. AveItemPreviewRestoreMain tracking live size/level count for its own job).
        private void PreviewRestore(Object sender, IndexItemProceedEventArgs args)
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                throw new JobStopException();
            }
            var archiverBasicIndex = (ArchiverBasicIndex)args.IndexItem;
            PreviewRestoreLevel level = GetPreviewRestoreLevel(archiverBasicIndex);
            logger.Info($"Preview restore processing item, level:{level}, url:{archiverBasicIndex.Url}, contentLength:{archiverBasicIndex.ContentLength}.");
            this.onItemProcessedCallback?.Invoke((int)level, archiverBasicIndex.ContentLength);
        }

        private PolicyLevel GetPolicyLevel(ArchiverBasicIndex archiverBasicIndex)
        {
            switch (archiverBasicIndex.Type)
            {
                case "E":
                    return PolicyLevel.SiteCollection;
                case "W":
                    return PolicyLevel.Site;
                case "L":
                    return PolicyLevel.List;
                case "F":
                    return PolicyLevel.Folder;
                case "D":
                    if (archiverBasicIndex.Name.Contains(':'))
                    {
                        return PolicyLevel.DocumentVersion;
                    }
                    else
                    {
                        return PolicyLevel.Document;
                    }
                case "U":
                    return PolicyLevel.ItemVersion;
                case "V":
                    return PolicyLevel.DocumentVersion;
                case "I":
                    if (archiverBasicIndex.Name.Contains(':'))
                    {
                        return PolicyLevel.ItemVersion;
                    }
                    else
                    {
                        return PolicyLevel.Item;
                    }
                case "A":
                    return PolicyLevel.Attachment;
                default:
                    logger.Warn($@"Unable get node type,type:{archiverBasicIndex.Type},full path:{archiverBasicIndex.Url}");
                    return PolicyLevel.None;
            }
        }

        // Maps an archived item to its PreviewRestoreLevel (sequential 0..8 display order). Used only by the
        // preview restore data size flow (PreviewRestore -> AveItemPreviewRestoreMain.UpdateSimulateResult),
        // kept separate from GetPolicyLevel/PolicyLevel (still used by SimulateRestore) so the older
        // simulate-restore feature's bit-flag-keyed LevelCountMap consumers (e.g. ViewStatistics) are unaffected.
        private PreviewRestoreLevel GetPreviewRestoreLevel(ArchiverBasicIndex archiverBasicIndex)
        {
            switch (archiverBasicIndex.Type)
            {
                case "E":
                    return PreviewRestoreLevel.SiteCollection;
                case "W":
                    return PreviewRestoreLevel.Site;
                case "L":
                    return PreviewRestoreLevel.List;
                case "F":
                    return PreviewRestoreLevel.Folder;
                case "D":
                    if (archiverBasicIndex.Name.Contains(':'))
                    {
                        return PreviewRestoreLevel.DocumentVersion;
                    }
                    else
                    {
                        return PreviewRestoreLevel.Document;
                    }
                case "U":
                    return PreviewRestoreLevel.ItemVersion;
                case "V":
                    return PreviewRestoreLevel.DocumentVersion;
                case "I":
                    if (archiverBasicIndex.Name.Contains(':'))
                    {
                        return PreviewRestoreLevel.ItemVersion;
                    }
                    else
                    {
                        return PreviewRestoreLevel.Item;
                    }
                case "A":
                    return PreviewRestoreLevel.Attachment;
                default:
                    logger.Warn($@"Unable get node type,type:{archiverBasicIndex.Type},full path:{archiverBasicIndex.Url}");
                    return PreviewRestoreLevel.Unknown;
            }
        }

        private void CalculateIndexItemCount(Object sender, IndexItemProceedEventArgs args)
        {
            this.maxItemNum += args.IndexCount;
        }

        private void StatisticDataInArchiverTier(Object sender, IndexItemProceedEventArgs args)
        {
            VerifyDataTier(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
        }

        private void SendItemData(ArchiverBasicIndex index, RestoreMarkMessage markMessage)
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
            if (!string.IsNullOrEmpty(index.stubInfo))
            {
                string xmlString = index.stubInfo;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                XmlNode node = xmlDoc.SelectSingleNode("StubInfo");
                fileHeader.StubType = node.Attributes["StubType"].Value;
            }
            fileHeader.ListBaseType = Convert.ToInt32(index.ListBaseType);
            fileHeader.IsFailed = Convert.ToBoolean(index.IsFailed);
            fileHeader.Path = fileHeader.Type.Equals(AveSharePointType.TYPE_SITE) ? this.siteCollectionNewestUrl : index.Name;
            if (markMessage != null)
            {
                fileHeader.Property = markMessage.Property;
                fileHeader.Security = markMessage.Security;
                fileHeader.VersionFlag = markMessage.VersionFlag;
                fileHeader.IsCurrentVersion = markMessage.IsCurrentVersion;
                fileHeader.IsSelect = markMessage.IsSelected;
                fileHeader.ParentIsSelect = markMessage.ParentIsSelected;
            }
            fileHeader.ListType = index.ListType;
            fileHeader.EncryptionInfo = encryptionInfo;
            fileHeader.HeaderExtraAttribute = index.ExtraInfo;
            fileHeader.ArchiveTime = index.ArchiveTime;
            fileHeader.IsAppData = index.IsAppData != null && index.IsAppData.EqualsIgnoreCase("True") ? true : false;
            fileHeader.Id = index.Id;
            fileHeader.StorageId = index.StoragePolicyId??string.Empty;
            fileHeader.BackUpJobId = index.JobId;
            fileHeader.ItemPathMD5 = index.PathMD5;
            fileHeader.UniqueId = index.NodeGuid;
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
            if (index.Type == "D" || index.Type == "I" || index.Type == "A")
            {
                hasRestoredItems.Add(index.PathMD5);
            }

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
        private void VerifyAllData(ArchiverBasicIndex index)
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
                                StorageCopyResult res = new StorageCopyResult();
                                if (this.dataLogicalDevice is XLibrary)
                                {
                                    try
                                    {
                                        if ((this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID.EqualsIgnoreCase(DEFAULTSTORAGEID))
                                        {
                                            string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.DefaultStorage);
                                            var client = Util.MSAzure.StorageUtil.GetContainerClient(defaultConnectionString, TenantLocalValue.LogonGroupId);
                                            var scrBlobClient = client.GetBlobClient(info.HighPlusLowName);
                                            var desBlobClient = client.GetBlobClient(info2.HighPlusLowName);
                                            BlobCopyFromUriOptions opt = new BlobCopyFromUriOptions();
                                            opt.AccessTier = AccessTier.Hot;
                                            var APIRes = desBlobClient.StartCopyFromUri(scrBlobClient.Uri, opt);
                                            res.IsCopyed = true;
                                        }
                                        else
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
                                        StartTime = DateTime.Now,
                                        storageContainerName = dataLogicalDevice.SystemLocation,
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
            Dictionary<string, BlobContainerClient> sourceContainerClients = new Dictionary<string, BlobContainerClient>();
            string storageContainerName = string.Empty;
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
                            var source = dataLogicalDevice as AbstractXSystem;
                            if (source != null && source.StorageType == XStorageType.Azure)
                            {
                                if (!sourceContainerClients.ContainsKey(r.Value.storageContainerName))
                                {
                                    var sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(restoreJob.LogicalDevice.ToXRIS().Where(s=> s.Contains(r.Value.storageContainerName)).FirstOrDefault());
                                    sourceContainerClients.Add(r.Value.storageContainerName, sourceContainerClient);
                                }
                                string blobName = r.Value.MappedBlobInfo.HighPlusLowName.Replace(@"\", @"/");
                                logger.Info($"WaitingRehydration.Blob name: {r.Value.MappedBlobInfo.HighPlusLowName}.blobName:{blobName}.");
                                var blobClient = sourceContainerClients[r.Value.storageContainerName].GetBlobClient(blobName);
                                BlobProperties properties = blobClient.GetProperties();
                                //判断状态
                                if (properties.AccessTier == AccessTier.Archive)
                                {
                                    logger.Info($"The {r.Key} need to rehydration, " +
                                    $"mapping data: {blobName}, " +
                                    $"AccessTier:{properties.AccessTier} , " +
                                    $"start time : {r.Value.StartTime.ToString()}" +
                                    $"ArchiveStatus : {properties.ArchiveStatus}");
                                    needContinueSleep = true;
                                    break;
                                }
                                else if (properties.AccessTier == AccessTier.Hot || properties.AccessTier == AccessTier.Cool)
                                {
                                    logger.Info($"The {r.Key} already rehydration, " +
                                    $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                    $"AccessTier: {properties.AccessTier} , " +
                                    $"start time : {r.Value.StartTime.ToString()}");
                                    r.Value.AlreadyRehydration = true;
                                }
                            }
                            else
                            {
                                logger.Info($"The rehydration data not XStorageType.Azure.");
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
        public string storageContainerName;
    }
}