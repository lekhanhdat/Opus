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

namespace AvePoint.Media.Service.ExchangeBackup
{
    #region using directives

    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.ArchiverBackup;
    
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using AvePoint.Metadata;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.RACommonUtility.Common;
    using AvePoint.RA.RedisCache;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using ExchangeCommonWrapper;
    //using AvePoint.Wrapper.Common;
    using ExchangeUtility.Graph;
    //using AvePoint.Media.Service.ArchiverBackup;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using global::Media.Service.ArchiverBackup.Restore;
    using MediaContract;
    using MediaDataIO;
    using Merged18NResources.MediaServiceExchangeBackUp;
    
    using Microsoft365Backup.CommonUtil;
    using Office365GroupRestore;
    using Org.BouncyCastle.Asn1.Tsp;
    using RAArchiverCommon;
    using RATeams.Restore.Common;
    using Storage;
    using Storage.Cloud.Azure;
    using Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Provider;
    using System.Xml;
    using Util;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
    using static Org.BouncyCastle.Math.EC.ECCurve;
    //using FileType = MediaContract.FileType;
    using AvePerformanceScope = AvePoint.Wrapper.Common.AvePerformanceScope;
    using FileType = MediaDataIO.FileType;

    //using AvePoint.Media.Service.ArchiverBackup;

    #endregion using directives

    public class ExchangeRestoreService
           : Office365GroupRestore.IRestoreService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeRestoreService));

        ExchangeRestoreJob RestoreJob { get; set; }

        private Int64 maxItemNum;
        private Int64 fileSize;
        private String errorMessage;
        private IXSystem indexLogicalDevice;
        private IXSystem dataLogicalDevice;
        private IXSystem destinationPhysicalDevice;
        private JobProgressInfo jobProgressInfo;
        private Byte[] buffer = new Byte[1048576];
        private List<ExchangeOnlineTreeNodeDto> mailBoxNodes;
        private RestoreConfig Config;
        private RestoreDataHandlerBase restoreDataHandler;
        private Dictionary<string, byte[]> EncryptionKeyCache;
        private SafeDictionary<string, ArchiverBackup.BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, ArchiverBackup.BLOBRehydrationMapping>();
        private List<string> AllScanedBLOBs = new List<string>();
        public event Office365GroupRestore.RestoreDataHandler.ProcessException ProcessExceptionHandler;
        private String rehydrationTemp;
        private string tempPath = "Temp"+Guid.NewGuid().ToString();
        private readonly Object rehydrationLock = new Object();
        private bool NeedToWeakup = false;
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        private List<string> restoredItemId = new List<string>();
        private readonly HashSet<AveSharePointType> ContainerRestoreDataTypes = [AveSharePointType.TYPE_SITE, AveSharePointType.TYPE_WEB, AveSharePointType.TYPE_FOLDER, AveSharePointType.TYPE_LIST];
        public Int64 MaxItemNum { get { return maxItemNum; } }

        public ICacheService CacheManager => RService.CacheService;

        public IStorageDeviceManager StorageDeviceManager => RService.StorageDeviceManager;

        public IExchangeRestoreTreeHandler TreeHandler => RContainer.ExchangeRestoreTreeHandler;

        public ExchangeIndexService IndexService => RContainer.ExchangeIndexService;

        public IExchangeRestoreIndexService RestoreIndexService => RContainer.ExchangeRestoreIndexService;

        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao = RContainer.ArchiverSiteMasterIndexDao;

        public void Open(RestoreConfig config)
        {
            logger.Info("Exchange restore service open.");
            this.Config = config;
            this.RestoreJob = config.exchangeRestoreJob;
            this.RestoreJob.RestoreVersionOption = config.RestoreVersionOption;
            this.RestoreJob.KeepVersionsNumber = config.KeepVersionsNumber;
            logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceOpenJobInfo, this.RestoreJob.ToString());
            logger.Info("Open index logical device.");
            this.indexLogicalDevice = this.StorageDeviceManager.Open(this.RestoreJob.IndexDBLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.Read);
            logger.Info("Open data logical device.");
            this.dataLogicalDevice = this.StorageDeviceManager.Open(this.RestoreJob.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Data), DeviceAccess.Read);
            logger.Info("Open cache manager.");
            this.CacheManager.Open(this.RestoreJob.CacheSetting, false, false);//stodo
            logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceOpenPreRestoreInitTree, Environment.NewLine, this.RestoreJob.ExchangeTreeRoot);
            //this.mailBoxNodes = RestoreJob.ExchangeTreeRoot.Children[0].Children.SelectMany(group => group.Children).ToList();
            mailBoxNodes = new List<ExchangeOnlineTreeNodeDto> { RestoreJob.ExchangeTreeRoot };
            SetPlannerRestoreConfig();
            logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceOpenCalculateTotleItemNumStartCalculate);
            this.jobProgressInfo = new JobProgressInfo() { Id = this.RestoreJob.JobId };
            this.EncryptionKeyCache = RestoreJob.RestoreSecurityInfos.GroupBy(t => t.BackupJobId).ToDictionary(t => t.Key, t => AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(t.First().SecurityInfo.DynamicKey));

            using (new AvePoint.Wrapper.Common.AvePerformanceScope("ExchangeRestoreService.Open.PraseThree"))
            {

                foreach (var mailBox in this.mailBoxNodes)
                {
                    RestoreConfig.CurrentMailbox = string.Format("{0}(GroupInfo)", mailBox.Name);
                    RestoreConfig.CurrentMailboxAddress = mailBox.Name;
                    RestoreConfig.CurrentMailboxType = mailBox.MailboxType;

                    if (config.RestoreType == GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.EORestoreType.InPlace)
                    {
                        var mediaDatePathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, config.exchangeRestoreJob.BackupJobId, mailBox.Name);
                        this.RestoreJob.IndexVolume = mediaDatePathGenerator.GenerateIndexVolume();
                        this.RestoreJob.DataVolume = mediaDatePathGenerator.GenerateDataVolume();
                        var indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
                        logger.Info("Open index service.");
                        this.IndexService.Open(indexOpenParam);
                        var restoreTreeHandlerParam = new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = this.RestoreJob, IsJustCalculateCount = true };
                        this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(this.CalculateIndexItemCount);
                        this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                        this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(this.CalculateIndexItemCount);
                        logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceOpenCalculateTotleItemNumDataCount, maxItemNum);
                        SOArchiverJobInfoStatistics.Instance.InitInstance(config.RestoreJobId, mailBox.Name, RA.Contract.JobMonitor.JobType.TeamsArchiverRestore, mailBox.ID);
                        SOArchiverJobInfoStatistics.Instance.KeepDataOption = -2;
                        SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
                    }
                    else if(config.JobType == (int)RA.Contract.JobMonitor.JobType.TeamsOutPlaceRestore)
                    {
                        var mediaDatePathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, config.exchangeRestoreJob.BackupJobId, mailBox.Name);
                        this.RestoreJob.IndexVolume = mediaDatePathGenerator.GenerateIndexVolume();
                        this.RestoreJob.DataVolume = mediaDatePathGenerator.GenerateDataVolume();
                        destinationPhysicalDevice = XFactoryCommon.InstanceSystem(config.DestinationFSDevice.BuildXRI());
                        destinationPhysicalDevice.Open();
                        config.DestinationPhysicalDevice = destinationPhysicalDevice;
                        config.DestinationDeviceSystemPath = destinationPhysicalDevice.SystemPath;
                        SOArchiverJobInfoStatistics.Instance.InitInstance(config.RestoreJobId, mailBox.Name, RA.Contract.JobMonitor.JobType.TeamsOutPlaceRestore, mailBox.ID);
                        SOArchiverJobInfoStatistics.Instance.KeepDataOption = -2;
                        SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
                    }
                    else //export exchange data
                    {
                        destinationPhysicalDevice = XFactoryCommon.InstanceSystem(config.DestinationFSDevice.BuildXRI());
                        destinationPhysicalDevice.Open();
                        config.DestinationPhysicalDevice = destinationPhysicalDevice;
                        config.DestinationDeviceSystemPath = destinationPhysicalDevice.SystemPath;
                        SOArchiverJobInfoStatistics.Instance.InitInstance(config.RestoreJobId, mailBox.Name, RA.Contract.JobMonitor.JobType.MailBoxArchiverRestore, mailBox.ID);
                        SOArchiverJobInfoStatistics.Instance.KeepDataOption = -2;
                        SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
                    }

                }
            }
        }
        private void SetPlannerRestoreConfig()
        {
            try
            {
                if (Config.JobId.StartsWith("EP") || this.mailBoxNodes.Count > 1) return;
                var temp = this.mailBoxNodes.First();
                while (temp.Children.Count > 0)
                {
                    temp = temp.Children.First();
                }
                switch (temp.Level)
                {
                    case NodeLevel.ExchangeOnlineMailbox:
                        RestoreConfig.EntirePlannerPlan = true;
                        break;
                    case NodeLevel.Office365PlannerPlan:
                    case NodeLevel.Office365PlannerTask:
                        RestoreConfig.NeedRecordTaskAttachmentsLink = true;
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to Judgment level. Reason: {0}", ex.ToString());
            }
        }

        private bool IsEnabledRealDelete()
        {
            var realDeleteRetentionDatas = RService.RMKeyValueDao.GetValueByKey("RealDeleteAzureRetentionDatas");
            if (realDeleteRetentionDatas != null)
            {
                bool result;
                if (bool.TryParse(realDeleteRetentionDatas.Value, out result) && result)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private AbstractXSystem ValidStorage(AbstractXSystem storage)
        {
            if (string.IsNullOrEmpty(storage.ConnectionString))
            {
                var storageLibrary = storage as XLibrary;
                if (storageLibrary == null)
                {
                    logger.Error($"Index storage system not a XLibrary.");
                }
                storage = storageLibrary?.SubSystems?.FirstOrDefault() as AbstractXSystem;
                if (storage == null)
                {
                    logger.Error($"Index storage system's connection string is null.");
                }
            }
            return storage;
        }


        void UnDeleteDataBlock(string fileName)
        {
            try
            {
                logger.Info($"Begin to ExchangeRestoreService.UnDeleteDataBlock fileName:");
                var blobClient = softDeleteContainerClient.GetBlobClient(fileName);
                blobClient.Undelete();
                SetBlockStatusToCurrentVersion(blobClient, fileName);

                void SetBlockStatusToCurrentVersion(BlobClient blobClient, string highPlusLowName)
                {
                    string blobName = highPlusLowName.Replace(@"\", @"/");
                    logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {highPlusLowName}.blobName:{blobName}.");
                    blobClient = softDeleteContainerClient.GetBlobClient(blobName);
                    // List all versions of the blob
                    List<string> blobVersions = new List<string>();
                    foreach (BlobItem blobItem in softDeleteContainerClient.GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blobName, default))
                    {
                        logger.Info($"SetBlockStatusToCurrentVersion.Blob name: {blobItem.Name}, Version ID: {blobItem.VersionId}.Version Delete:{blobItem.Deleted}.");
                        blobVersions.Add(blobItem.VersionId);
                    }
                    BlobClient versionedBlobClient = softDeleteContainerClient.GetBlobClient(blobName).WithVersion(blobVersions.FirstOrDefault());

                    // 开始复制操作
                    CopyFromUriOperation copyFromUriOperation = blobClient.StartCopyFromUri(versionedBlobClient.Uri);
                    // 检查复制状态
                    // 等待复制操作完成
                    while (!copyFromUriOperation.HasCompleted)
                    {
                        Thread.Sleep(100);
                        copyFromUriOperation.UpdateStatus();
                    }
                    logger.Info($"SetBlockStatusToCurrentVersion.Blob copy completed successfully.VersionId:{blobVersions.FirstOrDefault()}.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to undelete soft deleted data. error:{e}");
            }
        }

        BlobContainerClient softDeleteContainerClient;
        public void Restore(Object parameter)
        {
            logger.Info("Exchange restore service restore.");
            PrepareRestoreDataHandler(parameter);
            if (jobProgressInfo.IsFinal)
            {
                return;
            }
            bool needRehydrationData = false;

            if (Config.IsSoftDeleted && IsEnabledRealDelete())
            {
                var source = dataLogicalDevice as AbstractXSystem;

                if (source != null && source.StorageType == XStorageType.Azure)
                {
                    source = ValidStorage(source);
                    softDeleteContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                }
            }

            try
            {
                InternalRestore();
            }
            catch (JobNeedStopException)
            {
                this.jobProgressInfo.IsFinal = true;
                logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceRestoreNeedStop);
            }
            catch (BlobArchivedException e)
            {
                logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                if (Config.RestoreType == GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.EORestoreType.InPlace)
                {
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                }
                else if(Config.JobType == (int)RA.Contract.JobMonitor.JobType.TeamsOutPlaceRestore)
                {
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForTeamsOutPlace);
                }
                else
                {
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendExchangeIndexItemData);
                }
                needRehydrationData = true;
            }
            catch (Exception ex)
            {
                logger.Error($"RestoreService restore with {ex?.GetType().FullName} : {ex}");
                ProcessExceptionHandler(GetExceptionMessage(ex));
            }
            if (needRehydrationData)
            {
                //hasRehydrationData = true;
                var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(RestoreJob.DataVolume, null));
                logger.Info($"Need move blobs count : {tempFileList.Count}");
                tempFileList.ForEach(item =>
                {
                    StorageInfo info = new StorageInfo();
                    info.HighName = item.HighName;
                    info.LowName = item.LowName;
                    VerifyAndCopyArchiverToHot(info);
                });
                try
                {
                    if (BLOBMappings.Count > 0)
                    {
                        //Waiting Rehydration
                        WaitingRehydration();
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"WaitingRehydration failed, error: {e}");
                }
                //restoreTreeHandlerParam.IsJustCalculateCount = false;
                NeedToWeakup = true;
                InternalRestore(NeedToWeakup);
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
                        using (new CheckJobStopScope()) { }
                        if (!r.Value.AlreadyRehydration)
                        {
                            var file = this.dataLogicalDevice.OpenFile(r.Value.MappedBlobInfo);
                            if (file is AzureCloudInfo)
                            {
                                var azureFile = file as AzureCloudInfo;
                                if (!azureFile.Exists || azureFile.FileTierType == AccessTierType.Archive)
                                {
                                    logger.Info($"The {r.Key} need to rehydration, " +
                                        $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                        $"Exists:{azureFile.Exists} , " +
                                        $"start time : {r.Value.StartTime.ToString()}");
                                    needContinueSleep = true;
                                    break;
                                }
                                else
                                {
                                    logger.Info($"The {r.Key} already rehydration, " +
                                        $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                        $"Exists:{azureFile.Exists} , " +
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
                throw;
            }
        }
        private void VerifyAndCopyArchiverToHot(StorageInfo info)
        {
            var file = this.dataLogicalDevice.OpenFile(info);

            if (file is AzureCloudInfo)
            {
                var azureFile = file as AzureCloudInfo;
                //if (file != null && azureFile.FileTierType == AccessTierType.Archive)
                //{
                    string temp = SecurityUtils.SafeCombinePath(rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + 11));
                    lock (rehydrationLock)
                    {
                        if (!BLOBMappings.ContainsKey(info.HighPlusLowName))
                        {
                            //azureFile.FileTierType = AccessTierType.Archive;
                            AzureCloudInfo info2 = new AzureCloudInfo { HighName = temp, LowName = info.LowName, FileTierType = AccessTierType.Hot };
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
                                    StartTime = DateTime.Now
                                };
                                BLOBMappings.Add(info.HighPlusLowName, mapping);
                            }
                        }
                    }
                //}
            }
        }
        private void RestoreTeams(ExchangeOnlineTreeNodeDto mailBox)
        {
            RestoreConfig.CurrentMailboxAddress = mailBox.Name;
            RestoreConfig.CurrentMailbox = string.Format("{0}(GroupInfo)", mailBox.Name);
            RestoreConfig.CurrentMailboxType = mailBox.MailboxType;
            this.rehydrationTemp = SecurityUtils.SafeCombinePath(ServiceConstants.TeamsArchiverPath + "\\" + tempPath);
            var indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
            logger.Info("Open index service");
            this.IndexService.Open(indexOpenParam);

            RestoreIndexService.CreateIndex("COL_BACKUP_TIME");
            var restoreTreeHandlerParam = new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = this.RestoreJob };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
        }
        private void RestoreExchange(ExchangeOnlineTreeNodeDto mailBox,bool needToWeakup)
        {
            this.rehydrationTemp = SecurityUtils.SafeCombinePath(ServiceConstants.EXOArchiverPath + "\\" + tempPath);
            var volumeParam = new VolumeParameter()
            {
                EmailAddress = mailBox.Name,
                TempFolder = tempPath,
            };
            IVolumeGenerator generator = new ExchangeVolumeGenerator();
            if (needToWeakup)
            {
                this.RestoreJob.DataVolume = generator.GenerateTempDataVolume(volumeParam);
            }
            else
            {
                this.RestoreJob.DataVolume = generator.GenerateDataVolume(volumeParam);
            }
            this.RestoreJob.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            var indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
            logger.Info("Open index service.");
            this.IndexService.Open(indexOpenParam);
            var mapping = RestoreIndexService.LoadEXONameAndMd5Mapping();
            var restoreTreeHandlerParam = new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = this.RestoreJob };
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendExchangeIndexItemData);
            this.TreeHandler.ProcessExchangeNode(restoreTreeHandlerParam, mapping);
            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendExchangeIndexItemData);
        }
        private void InternalRestore(bool needToWeakup = false)
        {
            foreach (var mailBox in this.mailBoxNodes)
            {
                if (Config.RestoreType == GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.EORestoreType.InPlace)
                {
                    if (needToWeakup)
                    {
                        var mediaDatePathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, "", mailBox.Name);
                        this.RestoreJob.DataVolume = mediaDatePathGenerator.GenerateDataVolume(tempPath);
                    }
                    RestoreTeams(mailBox);
                }
                else if(Config.JobType == (int)RA.Contract.JobMonitor.JobType.TeamsOutPlaceRestore)
                {
                    using(var performance = new PerformanceScope("RestoreOutPlaceTeamsGroup", "", true))
                    {
                         RestoreOutPlaceTeamsGroup(mailBox, needToWeakup);
                    }
                }
                else
                {
                    using (var performance = new PerformanceScope("RestoreMailboxIndexTotal", "", true))
                    {
                        RestoreExchange(mailBox, needToWeakup);
                    }
                }
                this.restoreDataHandler.Add(new ExchangeDataBlock() { IsTimeOut = true, FileTail = new RestoreFileTail() });
                ProcessExceptionHandler -= this.restoreDataHandler.ProcessEx;
            }
        }

        private void RestoreOutPlaceTeamsGroup(ExchangeOnlineTreeNodeDto mailBox, bool needToWeakup)
        {
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForTeamsOutPlace);
            if (needToWeakup)
            {
                var mediaDatePathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, "", mailBox.Name);
                this.RestoreJob.DataVolume = mediaDatePathGenerator.GenerateDataVolume(tempPath);
            }
            RestoreTeamsData(mailBox);
            RestoreMailBoxData(mailBox, needToWeakup);
            RestoreSiteData(mailBox);

            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForTeamsOutPlace);
        }

        private void RestoreSiteData(ExchangeOnlineTreeNodeDto mailBox)
        {
            var siteUnderUrlsUnderTeams = LoadRestoreSiteUnderCurrentTeams(mailBox.Name);
            foreach(var siteUrl in siteUnderUrlsUnderTeams)
            {
                IVolumeGenerator generator = new ArchiverVolumeGenerator();
                var volumeParam = new VolumeParameter()
                {
                    FarmName = string.Empty,
                    SiteCollectionUrl = siteUrl
                };
                this.RestoreJob.DataVolume = generator.GenerateDataVolume(volumeParam);
                this.RestoreJob.IndexVolume = generator.GenerateIndexVolume(volumeParam);
                ExchangeIndexServiceOpenParameter indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
                this.IndexService.Open(indexOpenParam, DataModule.SitePlatform);
                this.TreeHandler.ProcessSiteCollectionNode(siteUrl, this.RestoreJob);
            }
        }

        private List<string> LoadRestoreSiteUnderCurrentTeams(string teamsAddress)
        {
            return ArchiverSiteMasterIndexDao.LoadSiteCollectionUrlsByJobIdOrTeamsGroup(this.RestoreJob.BackupJobId, teamsAddress);
        }

        private void RestoreMailBoxData(ExchangeOnlineTreeNodeDto mailBox, bool needToWeakup)
        {
            this.rehydrationTemp = SecurityUtils.SafeCombinePath(ServiceConstants.EXOArchiverPath + "\\" + tempPath);
            var volumeParam = new VolumeParameter()
            {
                EmailAddress = mailBox.Name,
                TempFolder = tempPath,
            };
            IVolumeGenerator generator = new ExchangeVolumeGenerator();
            if (needToWeakup)
            {
                this.RestoreJob.DataVolume = generator.GenerateTempDataVolume(volumeParam);
            }
            else
            {
                this.RestoreJob.DataVolume = generator.GenerateDataVolume(volumeParam);
            }
            this.RestoreJob.IndexVolume = generator.GenerateIndexVolume(volumeParam);
            ExchangeIndexServiceOpenParameter indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
            logger.Info("Open index service.");
            this.IndexService.Open(indexOpenParam);
            var mapping = RestoreIndexService.LoadEXONameAndMd5Mapping();
            TreeNodeParameter restoreTreeHandlerParam = new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = this.RestoreJob };
            this.TreeHandler.ProcessExchangeNode(restoreTreeHandlerParam, mapping);
        }

        private void RestoreTeamsData(ExchangeOnlineTreeNodeDto mailBox)
        {
            RestoreConfig.CurrentMailboxAddress = mailBox.Name;
            RestoreConfig.CurrentMailbox = string.Format("{0}(GroupInfo)", mailBox.Name);
            RestoreConfig.CurrentMailboxType = mailBox.MailboxType;
            this.rehydrationTemp = SecurityUtils.SafeCombinePath(ServiceConstants.TeamsArchiverPath + "\\" + tempPath);
            ExchangeIndexServiceOpenParameter indexOpenParam = new ExchangeIndexServiceOpenParameter(this.RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, RestoreConfig.CurrentMailbox, mailBox.MailboxType);
            logger.Info("Open index service");
            this.IndexService.Open(indexOpenParam);

            RestoreIndexService.CreateIndex("COL_BACKUP_TIME");
            TreeNodeParameter restoreTreeHandlerParam = new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = this.RestoreJob };
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
        }

        private String GetExceptionMessage(Exception ex)
        {
            if (ex is DeviceNotAvailableException)
            {
                return ExchangeReportMessage.CreateReportMessage("Wrapper_DeviceNotAvailable");
            }
            if (ex is Storage.Util.AuthenticationFailedException)
            {
                return ExchangeReportMessage.CreateReportMessage("Wrapper_DeviceAuthenticationFailed");
            }

            if (ex is BlobArchivedException)
            {
                return RestoreConstants.DATA_ARCHIVED_EXCEPTION;
            }

            if (ex is IndexCanNotFoundException)
            {
                return "The object you selected cannot be found in the backup index, please check if the object was backed up successfully in the backup job.";
            }

            if (ex.Message?.Contains("Value cannot be null.Parameter name: source") ?? false)
            {
                return ExchangeReportMessage.CreateReportMessage("Agent.Teams.CannotGetBackupData_E919719C-31E4-482F-B459-F4C9D420E1EE", this.RestoreJob.BackupJobId);
            }

            return ex.Message;

        }

        private void PrepareRestoreDataHandler(object parameter)
        {
            if (parameter is RestoreDataHandlerBatch)
            {
                restoreDataHandler = parameter as RestoreDataHandlerBatch;
                ProcessExceptionHandler += restoreDataHandler.ProcessEx;
            }
            else
            {
                restoreDataHandler = parameter as RestoreDataHandler;
                ProcessExceptionHandler += restoreDataHandler.ProcessEx;
            }
            ArgumentNullException.ThrowIfNull(restoreDataHandler, $"restoreDataHandler {parameter?.GetType().FullName}");
        }

        public void Close(String errorMessage)
        {
            logger.Info("Exchange restore service close.");
            this.IndexService.Close();
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.StorageDeviceManager.Close(this.dataLogicalDevice);
            destinationPhysicalDevice?.Close();
            this.CacheManager.Close();
            logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceCloseFinish);
        }


        private void SendIndexItemData(Object sender, IndexItemProceedEventArgs args)
        {
            this.ExchangeIndexHandled(args.IndexItem as GroupBasicIndex, args.MarkMessage);
        }
        private void SendExchangeIndexItemData(Object sender, IndexItemProceedEventArgs args)
        {
            this.ExchangeExportIndexHandled(args.IndexItem as ExchangeBasicIndex, args.MarkMessage);
        }

        private void SendIndexItemDataForTeamsOutPlace(Object sender, IndexItemProceedEventArgs args)
        {
            if (args.IndexItem is GroupBasicIndex)
            {
                this.ExchangeIndexHandled(args.IndexItem as GroupBasicIndex, args.MarkMessage);
            }
            else if (args.IndexItem is ExchangeBasicIndex)
            {
                this.ExchangeExportIndexHandled(args.IndexItem as ExchangeBasicIndex, args.MarkMessage);
            }
            else if(args.IndexItem is ArchiverBasicIndex)
            {
                this.ExchangeIndexHandled(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
            }
        }

        private void CalculateIndexItemCount(Object sender, IndexItemProceedEventArgs args)
        {
            this.maxItemNum += args.IndexCount;
        }
        private void ExchangeExportIndexHandled(ExchangeBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (index != null)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ExchangeRestoreService.ExchangeExportIndexHandled"))
                {
                    if (restoredItemId.Contains(index.Id))
                    {
                        logger.Info($"this item has restored,id:{index.Id}");
                        return;
                    }
                    logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceExchangeIndexHandledSendData, index.Path);
                    ExchangeDataBlock dataBlock = new ExchangeDataBlock();
                    this.fileSize = 0;
                    try
                    {
                        dataBlock.FileHeader = this.HandleEXOHeader(index, markMessage.ParentName, markMessage.ChildCount);
                        dataBlock.RestoreData = this.HandleEXOData(index);
                        dataBlock.FileTail = this.HandleTail();
                    }
                    catch (PathNotFoundException ex)
                    {
                        logger.Error("An error occurred while handle datablock info. exception:{0}", ex.ToString());
                        dataBlock.FileHeader = new ExchangeFileHeader();
                        dataBlock.RestoreData = new ExchangeRestoreData();
                        dataBlock.FileTail = new RestoreFileTail();
                    }
                    this.restoreDataHandler.AddForEXO(dataBlock);
                    //stodo//RestoreDataMonitor.Instance?.RecordDataDistribution(index.BackupTime, index.DataFileLength, index.JobId);
                }
            }
        }

        private void ExchangeIndexHandled(ArchiverBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (index != null)
            {
                if (ContainerRestoreDataTypes.Contains((AveSharePointType)index.Type[0]))
                {
                    ExchangeDataBlock containerDataBlock = new ExchangeDataBlock();
                    try
                    {
                        containerDataBlock.FileHeader = this.HandleSiteItemHeader(index, markMessage.ParentPath, markMessage.RealPath);
                    }
                    catch(Exception e)
                    {
                        logger.Error("An error occurred while handle datablock info. exception:{0}", e.ToString());
                        containerDataBlock.FileHeader = new ExchangeFileHeader();
                    }
                    this.restoreDataHandler.AddForSite(containerDataBlock);
                    return;
                }
                using(AvePerformanceScope pc = new AvePerformanceScope("ExchangeRestoreService.ExchangeIndexSiteItemHandled")) 
                {
                    ExchangeDataBlock dataBlock = new ExchangeDataBlock();
                    try
                    {
                        logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceExchangeIndexHandledSendData, index.PathMD5);
                        this.fileSize = 0;
                        dataBlock.FileHeader = this.HandleSiteItemHeader(index, markMessage.ParentPath, markMessage.RealPath);
                        dataBlock.RestoreData = this.HandleSiteItemData(index, markMessage.SiteCollectionPath);
                        dataBlock.FileTail = this.HandleTail();
                    }
                    catch (PathNotFoundException ex)
                    {
                        logger.Error("An error occurred while handle datablock info. exception:{0}", ex.ToString());
                        dataBlock.FileHeader = new ExchangeFileHeader();
                        dataBlock.RestoreData = new ExchangeRestoreData();
                        dataBlock.FileTail = new RestoreFileTail();
                    }
                    this.restoreDataHandler.AddForSite(dataBlock);
                }
            }
        }

        private ExchangeFileHeader HandleSiteItemHeader(ArchiverBasicIndex index, string parentPath, string realPath)
        {
            index.OpenType = StreamOpenType.Default;
            index.IsRestoreToFS = true;
            var fileHeader = new ExchangeFileHeader()
            {
                DataType = ConvertArchiverBasicTypeToExchangeDataType((AveSharePointType)index.Type[0]),
                Name = index.Name,
                ParentFullPath = parentPath,
                ItemName = index.ItemName,
                Path = realPath
            };
            return fileHeader;
        }

        private ExchangeDataType ConvertArchiverBasicTypeToExchangeDataType(AveSharePointType type) => type switch
        {
            AveSharePointType.TYPE_DOCUMENT => ExchangeDataType.SiteDocumentItem,
            AveSharePointType.TYPE_ATTACHMENTS => ExchangeDataType.SiteAttachmentItem,
            AveSharePointType.TYPE_VERSION => ExchangeDataType.SiteVersionItem,
            AveSharePointType.TYPE_SITE => ExchangeDataType.SiteCollection,
            AveSharePointType.TYPE_LIST => ExchangeDataType.SiteList,
            AveSharePointType.TYPE_FOLDER => ExchangeDataType.SiteFolder,
            AveSharePointType.TYPE_WEB => ExchangeDataType.Web,
            _ => ExchangeDataType.None
        };

        private void ExchangeIndexHandled(GroupBasicIndex index, RestoreMarkMessage markMessage)
        {
            if (index != null)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ExchangeRestoreService.ExchangeIndexHandled"))
                {
                    RestoreConfig.CurrentFileExtension = index.FileExtensionName;
                    logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceExchangeIndexHandledSendData, index.Path);
                    ExchangeDataBlock dataBlock = new ExchangeDataBlock();
                    this.fileSize = 0;
                    try
                    {
                        dataBlock.FileHeader = this.HandleHeader(index);
                        dataBlock.RestoreData = this.HandleData(index);
                        dataBlock.FileTail = this.HandleTail();
                    }
                    catch (PathNotFoundException ex)
                    {
                        logger.Error("An error occurred while handle datablock info. exception:{0}", ex.ToString());
                        dataBlock.FileHeader = new ExchangeFileHeader();
                        dataBlock.RestoreData = new ExchangeRestoreData();
                        dataBlock.FileTail = new RestoreFileTail();
                    }
                    this.restoreDataHandler.Add(dataBlock);
                    //stodo//RestoreDataMonitor.Instance?.RecordDataDistribution(index.BackupTime, index.DataFileLength, index.JobId);
                }
            }
        }

        private ExchangeFileHeader HandleHeader(GroupBasicIndex index)
        {
            index.OpenType = StreamOpenType.Default;
            index.IsRestoreToFS = false;
            var fileHeader = new ExchangeFileHeader()
            {
                DataType = (ExchangeDataType)index.Type,
                Name = index.Name,
                NodeType = index.NodeType,
            };
            var tempPath = index.Path.Contains(ServiceConstants.Delimiter) ?
                index.Path.Substring(0, index.Path.LastIndexOf(ServiceConstants.Delimiter)) : index.Path;
            if (index.Type == (Int32)ExchangeDataType.Item)
            {
                fileHeader.ParentFullPath = tempPath.Contains(ServiceConstants.Delimiter) ?
                   tempPath.Substring(0, tempPath.LastIndexOf(ServiceConstants.Delimiter)) : tempPath;
            }
            else
                fileHeader.ParentFullPath = tempPath;
            return fileHeader;
        }
        private ExchangeFileHeader HandleEXOHeader(ExchangeBasicIndex index,string parentName,int childCount)
        {
            index.OpenType = StreamOpenType.Default;
            index.IsRestoreToFS = true;
            var fileHeader = new ExchangeFileHeader()
            {
                DataType = (ExchangeDataType)index.Type,
                Name = index.Name,
                NodeType = index.NodeType,
                ParentName = parentName,
                ChildCount = childCount
            };
            return fileHeader;
        }
        private ExchangeRestoreData HandleData(GroupBasicIndex index)
        {
            ExchangeRestoreData restoreData = new ExchangeRestoreData();
            try
            {
                var tempFolder = "";//stodo AveEnv.GetAgentTempFolder(ContextLevel.Process);
                restoreData.RestoreStream = new RestoreStream(GenerateReader(index), tempFolder);
                HandleMetadata(restoreData);
                this.fileSize = restoreData.RestoreStream.Size;
                restoredItemId.Add(index.Id);
            }
            catch(SkipRetryException ex) when (ex.Message.Contains("Response status code does not indicate success: 409 (This operation is not permitted on an archived blob.)"))
            {
                logger.Error($"Error occurred while HandleData index. Blob is currently in Archived status: {ex}");
                throw new BlobArchivedException(ex.Message, "");
            }
            catch (BlobArchivedException e)
            {
                logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                throw;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                logger.Error("An error occurred while to handle data. Reason: {0}. ", e.ToString());
            }
            return restoreData;
        }

        private ExchangeRestoreData HandleSiteItemData(ArchiverBasicIndex index, string siteCollectionPath)
        {
            ExchangeRestoreData restoreData = new ExchangeRestoreData();
            try
            {
                restoreData.RestoreStream = new RestoreStream(GenerateSiteItemReader(index, siteCollectionPath), string.Empty);
                HandleContent(restoreData);
                this.fileSize = restoreData.RestoreStream.Size;
                restoredItemId.Add(index.Id);
            }
            catch (SkipRetryException ex) when (ex.Message.Contains("Response status code does not indicate success: 409 (This operation is not permitted on an archived blob.)"))
            {
                logger.Error($"Error occurred while HandleData index. Blob is currently in Archived status: {ex}");
                throw new BlobArchivedException(ex.Message, "");
            }
            catch (BlobArchivedException e)
            {
                logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                throw;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                logger.Error("An error occurred while to handle data. Reason: {0}. ", e.ToString());
            }
            return restoreData;
        }

        private ExchangeRestoreData HandleEXOData(ExchangeBasicIndex index)
        {
            ExchangeRestoreData restoreData = new ExchangeRestoreData();
            try
            {
                var tempFolder = "";//stodo AveEnv.GetAgentTempFolder(ContextLevel.Process);
                restoreData.RestoreStream = new RestoreStream(GenerateEXOReader(index), tempFolder);
                HandleMetadata(restoreData);
                HandleContent(restoreData);
                this.fileSize = restoreData.RestoreStream.Size;
                restoredItemId.Add(index.Id);
            }
            catch (SkipRetryException ex) when (ex.Message.Contains("Response status code does not indicate success: 409 (This operation is not permitted on an archived blob.)"))
            {
                logger.Error($"Error occurred while HandleData index. Blob is currently in Archived status: {ex}");
                throw new BlobArchivedException(ex.Message, "");
            }
            catch (BlobArchivedException e)
            {
                logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                throw;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                logger.Error("An error occurred while to handle data. Reason: {0}. ", e.ToString());
            }
            return restoreData;
        }
        private IItemDataReader GenerateReader(GroupBasicIndex index)
        {
            var context = new DataContextBase
            {
                ContentDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemContentDataStartFileNumber,
                    StartOffset = index.CurrentItemContentDataStartOffset,
                    PrefixNumber = index.CurrentItemContentDataFilePrefixNumber,
                    ContentLength = index.CurrentItemContentDataTotalLength,
                    FileType = FileType.Content,
                    ItemPageSize = index.CurrentItemPageSize
                },
                MetaDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemMetaDataStartFileNumber,
                    StartOffset = index.CurrentItemMetaDataStartOffset,
                    PrefixNumber = index.CurrentItemMetaDataFilePrefixNumber,
                    ContentLength = index.CurrentItemMetaDataAndContentDataTotalLength - index.CurrentItemContentDataTotalLength,
                    FileType = FileType.MetaData
                },
                DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, index.BackupJobId, RestoreConfig.CurrentMailboxAddress, NeedToWeakup, tempPath),
                EncryptionKey = EncryptionKeyCache.ContainsKey(index.JobId) ? EncryptionKeyCache[index.JobId] : null,
                ItemDataMode = (byte)index.CurrentItemDataMode
            };

            if (softDeleteContainerClient != null)
            {
                context.UnDeleteSoftDeletedDataBlock = UnDeleteDataBlock;
            }

            return new ItemDataReader(context, dataLogicalDevice);
        }
        private IItemDataReader GenerateEXOReader(ExchangeBasicIndex index)
        {
            var context = new DataContextBase
            {
                ContentDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemContentDataStartFileNumber,
                    StartOffset = index.CurrentItemContentDataStartOffset,
                    PrefixNumber = index.CurrentItemContentDataFilePrefixNumber,
                    ContentLength = index.CurrentItemContentDataTotalLength,
                    FileType = FileType.Content,
                    ItemPageSize = index.CurrentItemPageSize
                },
                MetaDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemMetaDataStartFileNumber,
                    StartOffset = index.CurrentItemMetaDataStartOffset,
                    PrefixNumber = index.CurrentItemMetaDataFilePrefixNumber,
                    ContentLength = index.CurrentItemMetaDataAndContentDataTotalLength - index.CurrentItemContentDataTotalLength,
                    FileType = FileType.MetaData
                },
                DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.EXOPlatform, index.BackupJobId, RestoreConfig.CurrentMailboxAddress, NeedToWeakup, tempPath),
                //EncryptionKey = Convert.FromBase64String(EncryptionKeyCache[index.JobId]),
                ItemDataMode = (byte)index.CurrentItemDataMode
            };

            if (softDeleteContainerClient != null)
            {
                context.UnDeleteSoftDeletedDataBlock = UnDeleteDataBlock;
            }

            return new ItemDataReader(context, dataLogicalDevice);
        }

        private IItemDataReader GenerateSiteItemReader(ArchiverBasicIndex index, string siteCollectionPath)
        {
            var context = new DataContextBase
            {
                ContentDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemContentDataStartFileNumber,
                    StartOffset = index.CurrentItemContentDataStartOffset,
                    PrefixNumber = index.CurrentItemContentDataFilePrefixNumber,
                    ContentLength = index.CurrentItemContentDataTotalLength,
                    FileType = FileType.Content,
                    ItemPageSize = index.CurrentItemPageSize
                },
                MetaDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemMetaDataStartFileNumber,
                    StartOffset = index.CurrentItemMetaDataStartOffset,
                    PrefixNumber = index.CurrentItemMetaDataFilePrefixNumber,
                    ContentLength = index.CurrentItemMetaDataAndContentDataTotalLength - index.CurrentItemContentDataTotalLength,
                    FileType = FileType.MetaData
                },
                DataPathGenerator = new SiteMediaDataPathGenerator(DataModule.SitePlatform, index.BackupJobId, index.SitePath, NeedToWeakup, tempPath),
                EncryptionKey = EncryptionKeyCache.ContainsKey(index.JobId) ? EncryptionKeyCache[index.JobId] : null,
                ItemDataMode = (byte)index.CurrentItemDataMode
            };

            if (softDeleteContainerClient != null)
            {
                context.UnDeleteSoftDeletedDataBlock = UnDeleteDataBlock;
            }

            return new ItemDataReader(context, dataLogicalDevice);
        }

        private void HandleMetadata(ExchangeRestoreData restoreData)
        {
            var metaLists = new List<AveMetadata>();
            AveMetadata meta;
            while ((meta = restoreData.RestoreStream.ReadMetadata()) != null)
            {
                metaLists.Add(meta);
            }
            restoreData.MetadataLists = metaLists;
        }
        private void HandleContent(ExchangeRestoreData restoreData)
        {
            restoreData.ContentStream = restoreData.RestoreStream.OpenContentStream();
        }
        private RestoreFileTail HandleTail()
        {
            var tail = new RestoreFileTail()
            {
                FileSize = fileSize,
                HasException = !string.IsNullOrEmpty(errorMessage),
                ErrorMessage = errorMessage
            };

            return tail;
        }

        public void DeleteTempFile()
        {
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
        }
    }
}