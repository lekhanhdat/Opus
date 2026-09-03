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
    #region directives

    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility.AzureBlobStorage;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Common.Report;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.Contract;
    using AvePoint.RA.Contract.Common;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.RMWeb;
    using AvePoint.RA.Contract.RMWeb.CP;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.ReportCenter;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.I18N.Core;
    using AvePoint.RA.RACommonUtility.Common;
    using AvePoint.RA.Service.Services.Archiver;
    using AvePoint.Wrapper.Common;
    using Azure.Storage.Blobs.Models;
    using GCommon.Utility;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using global::Media.Service.ArchiverBackup.Restore;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Microsoft.Graph;
    using Microsoft.IdentityModel.Protocols.WsTrust;
    using Microsoft.SharePoint.Client;
    using Newtonsoft.Json;
    using RAArchiverCommon;
    using Storage;
    using Storage.Cloud.Azure;
    using Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Util;
    using ZXing;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
    using static Google.Apis.Storage.v1.ObjectsResource;
    using AccessTierType = Storage.AccessTierType;
    using JobStatus = DomainModel.JobStatus;

    #endregion directives

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_8 },
    "ADO-34389",
    true)]

    #endregion CodeReview
    [Serializable]
    public class ExportOutLimitException : Exception
    { }
    public class ArchiverRestoreToStorageService
        : ApplicationModelServiceBase
        , IArchiverRestoreToStorageService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Int64 totalSize;
        Int64 currentFolderSize;
        Int64 maxItemNum;
        Int64 sendItemNum;
        Int32 currentJobState;
        Int64 zipSizeLimit = 20L * 1024 * 1024 * 1024;


        IXSystem destinationPhysicalDevice;
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        ArchiverRestoreJob archiverRestoreJob;
        JobStatusInfo jobStatusInfo;
        //IExportService exportService;
        StringBuilder lastSite;
        StringBuilder lastList;
        Boolean hadHandledAttachment = default(Boolean);
        RestoreJobPolicy restoreJobPolicy;
        ItemDetailMessage itemDetailMessage;
        int folderIndex = 1;
        string tempRestoreFolder;
        Boolean isUpdateProgressFinished;
        Boolean isUpdateExportLimitSize;
        Boolean exportOutLimit;
        public bool HasCompleteNode { get; set; }
        public bool HasErrorNode { get; set; }
        String TempRestoreFolder => folderIndex == 1 ? tempRestoreFolder : tempRestoreFolder + "(" + folderIndex + ")";
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        public ArchiverIndexService _IndexService { get; set; }
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService
        {
            get
            {
                if (_IndexService == null)
                {
                    _IndexService = new ArchiverIndexService()
                    {
                        IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                        IndexSynchronizer = new IndexDatabaseSynchronizer()
                    };
                    return _IndexService;
                }
                else
                {
                    return _IndexService;
                }
            }
            set { }
        }
        public IRMReportManager ReportManager { get; set; }
        public IRestoreServiceTreeHandler TreeHandler { get; set; }// { get { return new ArchiverRestoreTreeHandler(); } set { } }
        public IDataReader<ArchiverRestoreJob> DataReader { get; set; }
        public IEncryptionInfoManager EncryptionInfoManager { get; set; }//{ get { return new EncryptionInfoManager(); } set { } }
        public IJobProgressUpdater JobProgressUpdater { get; set; }
        public IAJobStatusUpdater JobStatusUpdater { get; set; }
        public IStreamOpenTypeGenerator StreamOpenTypeGenerator { get { return new StreamOpenTypeGenerator(); } set { } }
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public IRestoreJobRunningPolicyChecker RestoreJobRunningPolicyChecker { get { return new RestoreJobRunningPolicyChecker(); } set { } }
        public IArchiverRestoreIndexService RestoreIndexService { get; set; }//{ get { return new ArchiverRestoreIndexService(); } set { } }
        //public IRestoreToFSReportService RestoreToFSReportService { get; set; }
        public IFileNameGeneratorFactory FileNameGeneratorFactory { get { return new FileNameGeneratorFactory(); } set { } }

        public SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();
        private List<string> AllScanedBLOBs = new List<string>();
        private IFileNameGenerator fileNameGenerator;
        private string rehydrationTemp;
        private readonly object rehydrationLock = new object();
        private string mDefaultPhysicalDeviceId = string.Empty;
        private bool needRehydrationData = false;
        private bool hasRehydrationData = false;
        private List<string> hasRestoredItems = new List<string>();
        private ArchiverRestoreRequest mRestoreRequest;
        public List<string> backupJobIds = new List<string>();
        private ActionStatistics restoreActionStatistics;
        private readonly object lockObject = new object();
        private IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IKeyValueService keyValueService = PlatformWindsorManager.GetService<IKeyValueService>();
        public bool HasBlobInArchiverTier
        {
            get { return BLOBMappings.Count > 0; }
        }
        // Max file name length (characters) to avoid PathTooLongException on common filesystems (ext4: 255 byte segment limit). We reserve some space for potential duplicate suffixes so set to 240.
        private const int MAX_FILE_NAME_LENGTH = 240; // simple character limit for a single file name segment

        public async Task HandleRestoreRequestAsync(ArchiverRestoreRequest restoreRequest, IRMReportManager reportManager)
        {
            this.ReportManager = reportManager;
            this.mRestoreRequest = restoreRequest;
            bool outRestoreStopping = false;
            string summeryComment = string.Empty;
            var archiverRestoreJob = new ArchiverRestoreJob(restoreRequest);
            try
            {
                this.SetZipSizeLimit();
                this.Open(archiverRestoreJob);
                this.Restore(archiverRestoreJob.TenantGroupId);
                this.ZipAndUploadFile(archiverRestoreJob.SiteUrl, restoreRequest.IsRecenterExport);

                if (!restoreRequest.IsRecenterExport)
                {
                    ExportSendEmail sendEmail = new ExportSendEmail();
                    ParameterDto para = new ParameterDto() { ExportLocation= destinationPhysicalDevice.SystemPath, ZipPassword= archiverRestoreJob.ZipFilePassword, RestoreJobid=archiverRestoreJob.ParentJobId };
                    await sendEmail.SendEmailAsync(restoreRequest.NotificationUsers, para);
                }
            }
            catch (JobStopException e)
            {
                logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                outRestoreStopping = true;
                throw;
            }
            catch (ExportOutLimitException ex)
            {
                logger.Error("Export Out Limit Exception");
                exportOutLimit = true;
                this.currentJobState = 3;
            }
            catch (Storage.Util.AuthenticationFailedException ex)
            {
                logger.Error("An error occurred while restoring to storage,upload failed,detail{0}:", ex);
                this.currentJobState = 7;
                HasErrorNode = true;
                summeryComment = "RM_JM_OutOfRestore_Upload_Failed";
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while restoring to storage,detail{0}:", ex);
                this.currentJobState = 7;
                HasErrorNode = true;
                throw;
            }
            finally
            {
                this.isUpdateProgressFinished = true;
                AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus jobStatus;
                if (outRestoreStopping)
                {
                    jobStatus = RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                }
                else
                {
                    jobStatus = GetJobStatus();
                }
                AddRestoreJobSummaryDetails();
                this.ReportManager.SetJobFinished(jobStatus, summeryComment);
                //this.UpdateJobSummary();
                this.Dispose();
            }
        }

        private void SetZipSizeLimit()
        {
            long size = keyValueService.GetOOPRestoreJobZipSizeLimit();
            if (size > 0)
            {
                zipSizeLimit = size;
            } 
        }

        private async Task<Stream> GetStubStreamAsync(ArchiverRestoreRequest restoreRequest)
        {
            var archiverRestoreJob = new ArchiverRestoreJob(restoreRequest);
            this.OpenStubPreview(archiverRestoreJob);
            this.RestoreIndexService = new ArchiverRestoreIndexService() { HeadAndBodyService = new ArchiverHeadAndBodyIndexService() { IndexProcessor = _IndexService.IndexProcessor } };
            var index = this.RestoreIndexService.LoadByPathMd5(restoreRequest.PreviewParam.PathMd5, restoreRequest.ArchiveTime);
            if (index == null)
            {
                throw new NullReferenceException(string.Format($"Cannot find the index with the path:{restoreRequest.PreviewParam.FullPath}"));
            }
            var resultStream = this.GetDataStream(index);
            return resultStream;
        }
        public AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus GetJobStatus()
        {
            AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            if (HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (HasCompleteNode && HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }
            else if (!HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (!HasCompleteNode && HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;
            }
            return mJobStatus;
        }
        public async Task HandleEndUserRestoreRequestAsync(ArchiverRestoreRequest restoreRequest, IRMReportManager reportManager)
        {
            try
            {
                this.SetZipSizeLimit();
                this.ReportManager = reportManager;
                var archiverRestoreJob = new ArchiverRestoreJob(restoreRequest);
                this.Open(archiverRestoreJob);
                this.RestoreForDownloadArchiverContent();
                //this.ZipAndUploadFile();
                var file = await GetFileInfo();
                (string blobName, bool needSasUri) = await UploadFileToStorageAsync(file);
                if(needSasUri)
                {
                    var sasUri = await GenerateSasUri(blobName);
                    DownloadDataInfoDao.UpdateBlobSasUriByJobId(this.TempRestoreFolder, sasUri);
                }
                DownloadDataInfoDao.UpdateDownloadFileSizeByJobId(this.TempRestoreFolder, file.FileSize);

                this.DeleteCachenfo();
            }
            catch (JobStopException e)
            {
                logger.Warn("Job will stop");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while restoring to storage,detail{0}:", ex);
                HasErrorNode = true;
                this.currentJobState = 7;
            }
            finally
            {
                this.isUpdateProgressFinished = true;
                //this.UpdateJobSummary();
                AddRestoreJobSummaryDetails();
                this.ReportManager.SetJobFinished(GetJobStatus());
                this.Dispose();
            }
        }

        private async Task<XFileInfo> GetFileInfo()
        {
            var restoreFolderPath = Path.Combine(this.CacheManager.CacheSystem.SystemLocation, this.TempRestoreFolder);
            var storageInfo = new StorageInfo { HighName = this.TempRestoreFolder };
            var files = this.CacheManager.CacheSystem.ListFiles(storageInfo);
            if (!files.Any())
            {
                throw new Exception("Failed to upload file to Azure.");
            }
            return files[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>BlobName</returns>
        /// <exception cref="Exception"></exception>
        private async Task<(string,bool)> UploadFileToStorageAsync(XFileInfo file)
        {
            string blobName;
            blobName = Path.Combine(TenantLocalValue.LogonGroupId, this.TempRestoreFolder, file.Name);

            var fileStorageInfo = new StorageInfo { HighName = file.HighName, LowName = file.LowName };
            bool needSasUri;

            using (Stream content = this.CacheManager.CacheSystem.OpenStream(fileStorageInfo, FileMode.Open))
            {
                string containerName = "archivedcontent";
                string specialSharedConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
                (blobName, needSasUri) = AzureUtil.UploadStorageBlobForDownloadCenter(containerName, blobName, content, sharedConnectionString: specialSharedConnectionString);
            }
            return (blobName,needSasUri);
        }

        private async Task<string> GenerateSasUri(string blobName)
        {
            string connectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
            var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(connectionString, containerName);
            if (await azureBlobStorage.CheckBlobExistAsync(blobName))
            {
                var sasUri = Util.MSAzure.StorageUtil.GenerateSasUriForRead(connectionString, containerName, blobName,TimeSpan.FromDays(7));
                //var Expired = DateTime.UtcNow.AddHours(6).AddMinutes(-10);
                logger.Info("Finish Create File SAS");
                return sasUri;
            }
            else
            {
                throw new Exception($"Can not find blob, blobName:{blobName}.");
            }
        }
        void OpenStubPreview(ArchiverRestoreJob restoreJob)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenBegin);
            this.archiverRestoreJob = restoreJob;
            this.jobStatusInfo = new JobStatusInfo();
            this.archiverRestoreJob.LogicalDevice = new LogicalDeviceDto();
            this.archiverRestoreJob.DataLogicalDeviceList.ForEach(logicalDevice =>
            {
                logicalDevice.PhysicalDrives.ForEach(physicalDevice =>
                {
                    this.archiverRestoreJob.LogicalDevice.PhysicalDrives.Add(physicalDevice);
                });
            });
            foreach (var pd in this.archiverRestoreJob.LogicalDevice.PhysicalDrives)
            {
                if (pd.Name == "Default Physical Device")
                {
                    mDefaultPhysicalDeviceId = pd.Id;
                    break;
                }
            }
            if (StorageDeviceManager == null)
            {
                StorageDeviceManager = new StorageDeviceManager();
            }
            this.dataLogicalDevice = this.StorageDeviceManager.Open(this.archiverRestoreJob.LogicalDevice.ToXRIS());

            this.fileNameGenerator = FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, DataVersion.Data6000);
            this.rehydrationTemp = Path.Combine("data_archive", "Temp" + Guid.NewGuid());
            if (this.CacheManager == null)
            {
                this.CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            }
            //this.RestoreToFSReportService.PrepareForReport(this.archiverRestoreJob);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(this.archiverRestoreJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(this.archiverRestoreJob.CacheSetting, indexLogicalDevice.IsDirectSystem);
            var restoreTempInfo = new StorageInfo { HighName = this.TempRestoreFolder, LowName = String.Empty };
            this.CacheManager.CacheSystem.Open();
            this.CacheManager.CacheSystem.OpenDirectory(restoreTempInfo, FileMode.OpenOrCreate);
            var indexOpenParam = new ArchiverIndexServiceOpenParameter(this.archiverRestoreJob, indexLogicalDevice);
            indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.archiverRestoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            this.IndexService.Open(indexOpenParam);
            this.DataReader.Open(this.archiverRestoreJob);
            var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(this.archiverRestoreJob.RestoreSecurityInfos);
            DataReader.SetEncryptionInfos(encryptionInfoDic);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenBeforeCut, Environment.NewLine, restoreJob.TreeRoot);
        }
        void Open(ArchiverRestoreJob restoreJob)
        {
            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenBegin);
            this.archiverRestoreJob = restoreJob;
            this.jobStatusInfo = new JobStatusInfo();
            this.archiverRestoreJob.LogicalDevice = new LogicalDeviceDto();
            this.archiverRestoreJob.DataLogicalDeviceList.ForEach(logicalDevice =>
            {
                logicalDevice.PhysicalDrives.ForEach(physicalDevice =>
                {
                    this.archiverRestoreJob.LogicalDevice.PhysicalDrives.Add(physicalDevice);
                });
            });
            foreach (var pd in this.archiverRestoreJob.LogicalDevice.PhysicalDrives)
            {
                if (pd.Name == "Default Physical Device")
                {
                    mDefaultPhysicalDeviceId = pd.Id;
                    break;
                }
            }
            if (StorageDeviceManager == null)
            {
                StorageDeviceManager = new StorageDeviceManager();
            }
            this.dataLogicalDevice = this.StorageDeviceManager.Open(this.archiverRestoreJob.LogicalDevice.ToXRIS());

            this.fileNameGenerator = FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, DataVersion.Data6000);
            this.rehydrationTemp = Path.Combine("data_archive", "Temp" + Guid.NewGuid());
            if (this.CacheManager == null)
            {
                this.CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            }
            //this.RestoreToFSReportService.PrepareForReport(this.archiverRestoreJob);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(this.archiverRestoreJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(this.archiverRestoreJob.CacheSetting, indexLogicalDevice.IsDirectSystem);
            this.tempRestoreFolder = this.archiverRestoreJob.JobId.Contains("_") ? this.archiverRestoreJob.JobId.Remove(this.archiverRestoreJob.JobId.IndexOfIgnoreCase("_")) :
                this.archiverRestoreJob.JobId;
            var restoreTempInfo = new StorageInfo { HighName = this.TempRestoreFolder, LowName = String.Empty };
            this.destinationPhysicalDevice = XFactoryCommon.InstanceSystem(this.archiverRestoreJob.DestinationFSDevice.BuildXRI());
            this.CacheManager.CacheSystem.Open();
            this.CacheManager.CacheSystem.OpenDirectory(restoreTempInfo, FileMode.OpenOrCreate);
            sw3.Stop();
            this.logger.Info($"linkRestoreReport init open function cost time:{sw3.ElapsedMilliseconds}");
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            var indexOpenParam = new ArchiverIndexServiceOpenParameter(this.archiverRestoreJob, indexLogicalDevice);
            indexOpenParam.IndexCacheDeviceSystem = XFactoryCommon.InstanceLibrary(this.archiverRestoreJob.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            this.IndexService.Open(indexOpenParam);
            sw1.Stop();
            this.logger.Info($"linkRestoreReport restore open index cost:{sw1.ElapsedMilliseconds}");
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            this.DataReader.Open(this.archiverRestoreJob);
            var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(this.archiverRestoreJob.RestoreSecurityInfos);
            DataReader.SetEncryptionInfos(encryptionInfoDic);
            sw2.Stop();
            this.logger.Info($"linkRestoreReport restore open data device cost:{sw2.ElapsedMilliseconds}");
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenBeforeCut, Environment.NewLine, restoreJob.TreeRoot);
            //this.TreeHandler.CutTree(this.archiverRestoreJob.TreeRoot);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenAfterCut, Environment.NewLine, restoreJob.TreeRoot);
            //if (this.archiverRestoreJob.RestoreFSOption == RestoreFSOption.CONCORDANCE)
            //{
            //    var exportProvider = new ExportServiceProvider();
            //    this.exportService = exportProvider.Create(ExportFormat.Concordance);
            //    var exportServiceInfo = new ExportServiceInfo { ExportDevice = this.archiverRestoreJob.DestinationFSDevice, JobId = this.archiverRestoreJob.ParentJobId };
            //    this.exportService.Open(exportServiceInfo);
            //}
            //this.JobStatusUpdater = JobReportServiceFactory.CreateJobStatusUpdater();
            this.jobStatusInfo.State = 1;
            this.jobStatusInfo.Type = 28;
            this.jobStatusInfo.IsSubJob = this.archiverRestoreJob.BackupJobId.IndexOf("_", StringComparison.OrdinalIgnoreCase) == -1 ? false : true;
            this.jobStatusInfo.Id = this.archiverRestoreJob.JobId;
            this.lastSite = new StringBuilder();
            this.lastList = new StringBuilder();
            this.restoreJobPolicy = new RestoreJobPolicy(restoreJob);
            this.RestoreJobRunningPolicyChecker.SetPolicy(restoreJobPolicy);
            this.restoreJobPolicy.JobStatus = JobStatus.Stopping;
            ThreadPool.QueueUserWorkItem(state =>
            {
                while (!this.isUpdateProgressFinished)
                {
                    Thread.Sleep(5 * 60 * 1000);
                    //JobProcessUtility.CheckIfJobCancelled(this.JobStatusUpdater.UpdateJobProgress(this.jobStatusInfo));
                }
            });
        }
        private void AddRestoreReport(string url, long nodeSize, int status, string cacheNodeType, long finishTime,string pathMd5, string message = "")
        {
            if (status == 10)
                return;
            AnalyzeStatus((JobDetailsStatus)status);
            string apUrl = string.Empty;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(url);
                XmlNode root = doc.DocumentElement;
                apUrl = root.Attributes["APUrl"].Value;
            }
            catch (Exception e)
            {
                apUrl = string.Empty;
                logger.Error($"some thing went wrong when get apUrl for add report,error:{e.ToString()}");
            }
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            JMRestoreActionJobDetailes mArchiverActionJobDetails = new JMRestoreActionJobDetailes();
            mArchiverActionJobDetails.SourceLocation = string.IsNullOrEmpty(apUrl)?url:apUrl.Replace('\\','/');
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Status = (JobDetailsStatus)status;
            mArchiverActionJobDetails.Level = ConverTypeToLevel(cacheNodeType);
            mArchiverActionJobDetails.Comment = message;
            mArchiverActionJobDetails.Path = string.IsNullOrEmpty(apUrl) ? url : apUrl.Replace('\\', '/');
            mArchiverActionJobDetails.PolicyLevel = cacheNodeType;
            mArchiverActionJobDetails.PathMd5 = pathMd5;
            ReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeRestoreDetailsForSummary(nodeSize, (int)JobReportUtility.ConverTypeToNodeLevel(cacheNodeType), (JobDetailsStatus)status);
        }
        private void AnalyzeRestoreDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (restoreActionStatistics == null)
            {
                lock (lockObject)
                {
                    if (restoreActionStatistics == null)
                    {
                        restoreActionStatistics = new ActionStatistics();
                        restoreActionStatistics.ActionTab = (int)ActionTab.Restore;
                    }
                }
            }
            if (status == JobDetailsStatus.Successful)
            {
                restoreActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(restoreActionStatistics, cacheNodeType, status);
        }

        private void AnalyzeStatusForSummary(ActionStatistics sta, int cacheNodeType, JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    AnalyzeObjCount(sta.SuccessfulObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Skipped:
                    AnalyzeObjCount(sta.SkippedObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Failed:
                    AnalyzeObjCount(sta.FailedObj, cacheNodeType);
                    break;
                default:
                    break;
            }
        }

        private void AddRestoreJobSummaryDetails()
        {
            JMRestoreSummaryDetails summaryDetails = new JMRestoreSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (restoreActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(restoreActionStatistics);
            }
            if (summaryDetails.ActionStatistics.Count > 0)
            {
                ReportManager.SendJobDetail(summaryDetails);
            }
        }

        private void AnalyzeObjCount(ObjectStatistic objSta, int cacheNodeType)
        {
            if (cacheNodeType == (int)CacheNodeType.Exception)
            {
                objSta.ExceptionCount++;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Item)
            {
                objSta.ItemCount++;
            }
            else if (cacheNodeType > (int)CacheNodeType.List)
            {
                objSta.FolderCount++;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                objSta.ListCount++;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web)
            {
                objSta.SiteCount++;
            }
            else if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                objSta.SiteCollectionCount++;
            }
        }

        private string ConverTypeToLevel(string type)
        {
            switch (type)
            {
                case "E":
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case "W":
                    return "RM_JS_Rule_ObjectLevel_Site";
                case "L":
                    return "RM_JS_Rule_ObjectLevel_List";
                case "F":
                    return "RM_JS_Rule_ObjectLevel_Folder";
                case "D":
                case "I":
                    return "RM_JS_Rule_ObjectLevel_Item";
                case "A":
                    return "RM_JS_Rule_ObjectLevel_Attachment";
                case "Y":
                    return "RM_JS_Rule_ObjectLevel_App";
                default:
                    return type;
            }
        }
        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped)
            {
                HasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                HasErrorNode = true;
            }
        }
        private void Restore(string tenantGroupId)
        {
            try
            {
                var siteCollectionNode = this.GetSiteCollectionTreeNode(this.archiverRestoreJob.TreeRoot);
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCalculateTotalItemNumStart);
                var restoreTreeHandlerParam = new TreeNodeParameter
                {
                    CurrentTree = siteCollectionNode,
                    RestoreJob = this.archiverRestoreJob,
                    IsJustCalculateCount = true
                };
                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                this.ReportManager.SetTotal(this.maxItemNum);
                logger.Info($"this retore job should restore count is:{this.maxItemNum}");
                this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                sw1.Stop();
                logger.Info($"linkRestoreReport restore CalculateIndexItemCount cost:{sw1.ElapsedMilliseconds}");
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCalculateTotalItemNumResult,
                            maxItemNum);
                //RehydrationData();

                if (isUpdateExportLimitSize)
                {
                    //>100G
                    if (this.destinationPhysicalDevice.StorageType == XStorageType.Azure)
                    {
                        //var mArchiverJobManagementService = JobReportServiceFactory.CreateArchiverJobManagementService();
                        //if (!mArchiverJobManagementService.UpdadeAndCheckDataSize(tenantGroupId))
                        //{
                        //    logger.Warn("out limit Storage Space");
                        //    throw new ExportOutLimitException();
                        //}
                    }
                }
                restoreTreeHandlerParam.IsJustCalculateCount = false;
                try
                {
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemData);
                }
                catch (Storage.Util.BlobArchivedException e)
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
                    else
                    {
                        logger.Error($"there exist something wrong when archive tier:{e}");
                        throw;
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
                logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }
            finally
            {
                this.isUpdateProgressFinished = true;
                WrapperConfiguration.NeedToUploadIndex = ArchiverIndexSubInfoDao.CheckExistSoftInfoAndUpdateThem(backupJobIds);
            }
        }

        private void VerifyAllData(ArchiverBasicIndex index)
        {
            StorageInfo info = new StorageInfo() { HighName = this.archiverRestoreJob.DataVolume };
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
        private void RehydrationData()
        {
            this.logger.Info("Start statistics restore data in ArchiverTier.");
            var restoreTreeHandlerParam = new TreeNodeParameter { CurrentTree = this.archiverRestoreJob.TreeRoot, RestoreJob = this.archiverRestoreJob, IsJustCalculateCount = false };
            this.logger.Info("Start statistics restore data in ArchiverTier.");
            this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
            this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
            this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(StatisticDataInArchiverTier);
            this.logger.Info("The restored data count in ArchiverTier is {0}.AllScanedBLOBs:{1}.", BLOBMappings.Count, AllScanedBLOBs.Count);
            try
            {
                if (BLOBMappings.Count > 0)
                {
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
                logger.Warn("Job will stop,stop Rehydration and delete Temp folder");
                throw;
            }
        }
        private void RestoreForDownloadArchiverContent()
        {
            try
            {
                var siteCollectionNode = this.GetSiteCollectionTreeNode(this.archiverRestoreJob.TreeRoot);
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCalculateTotalItemNumStart);
                var restoreTreeHandlerParam = new TreeNodeParameter
                {
                    CurrentTree = siteCollectionNode,
                    RestoreJob = this.archiverRestoreJob,
                    IsJustCalculateCount = true
                };
                this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(CalculateIndexItemCount);
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCalculateTotalItemNumResult,
                            maxItemNum);
                //RehydrationData();
                restoreTreeHandlerParam.IsJustCalculateCount = false;
                try
                {
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                }
                catch (Storage.Util.BlobArchivedException e)
                {
                    logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                    needRehydrationData = true;
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                }
                catch (SkipRetryException e)
                {
                    if (e.Message.Contains("This operation is not permitted on an archived blob."))
                    {
                        logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                        needRehydrationData = true;
                        this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                    }
                    else
                    {
                        logger.Error($"there is archived tier data can not restore,1will retry,error:{e.ToString()}");
                        throw;
                    }
                }
                if (needRehydrationData)
                {
                    RehydrationData();
                    hasRehydrationData = true;
                    restoreTreeHandlerParam.IsJustCalculateCount = false;
                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                    this.TreeHandler.ProcessTreeNode(restoreTreeHandlerParam);
                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(SendIndexItemDataForEnduserDownload);
                }
            }
            catch (JobStopException e)
            {
                logger.Warn("Job will stop");
                throw;
            }
            finally
            {
                this.isUpdateProgressFinished = true;
            }
        }

        private void SendIndexItemData(Object sender, IndexItemProceedEventArgs args)
        {
            //if (this.archiverRestoreJob.RestoreFSOption == RestoreFSOption.CONCORDANCE)
            //{
            //    this.SendWithConcordanceFormat(args.IndexItem as ArchiverBasicIndex);
            //}
            //else
            using (new CheckJobStopScope()) { }
            //{
            this.itemDetailMessage = new ItemDetailMessage();
            this.SendItemData(args.IndexItem as ArchiverBasicIndex);
            if (currentFolderSize > zipSizeLimit)
            {
                this.ZipAndUploadFile(archiverRestoreJob.SiteUrl, mRestoreRequest.IsRecenterExport);
            }
            //}
        }

        private void SendIndexItemDataForEnduserDownload(Object sender, IndexItemProceedEventArgs args)
        {
            //if (this.archiverRestoreJob.RestoreFSOption == RestoreFSOption.CONCORDANCE)
            //{
            //    this.SendWithConcordanceFormat(args.IndexItem as ArchiverBasicIndex);
            //}
            //else
            //{
            this.itemDetailMessage = new ItemDetailMessage();
            this.SendItemDataForEnduserDownload(args.IndexItem as ArchiverBasicIndex);
            //}
        }

        private void CalculateIndexItemCount(Object sender, IndexItemProceedEventArgs args)
        {
            this.maxItemNum += args.IndexCount;
        }

        private SPTreeNodeDto GetSiteCollectionTreeNode(SPTreeNodeDto treeNode)
        {
            return treeNode;
        }

        private void StatisticDataInArchiverTier(Object sender, IndexItemProceedEventArgs args)
        {
            VerifyDataTier(args.IndexItem as ArchiverBasicIndex, args.MarkMessage);
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
            if (!AllScanedBLOBs.Contains(SecurityUtils.SafeCombinePath(archiverRestoreJob.DataVolume, contentName)))
            {
                StorageInfo info = new StorageInfo { HighName = archiverRestoreJob.DataVolume, LowName = contentName };
                var file = this.dataLogicalDevice.OpenFile(info);
                if (!isUpdateExportLimitSize)
                {
                    if (this.dataLogicalDevice is XLibrary)
                    {
                        var id = (this.dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID;
                        if (mDefaultPhysicalDeviceId == id)
                        {
                            isUpdateExportLimitSize = true;
                        }
                    }
                }

                if (file is AzureCloudInfo)
                {
                    var azureFile = file as AzureCloudInfo;
                    if (azureFile != null && azureFile.FileTierType == AccessTierType.Archive)
                    {
                        string temp = Path.Combine(rehydrationTemp, archiverRestoreJob.DataVolume.Substring(archiverRestoreJob.DataVolume.IndexOf("DataVolume") + 11));
                        lock (rehydrationLock)
                        {
                            if (!BLOBMappings.ContainsKey(SecurityUtils.SafeCombinePath(archiverRestoreJob.DataVolume, contentName)))
                            {
                                azureFile.FileTierType = AccessTierType.Archive;
                                StorageInfo info2 = new AzureCloudInfo { HighName = temp, LowName = contentName, FileTierType = AccessTierType.Hot };
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
                                    BLOBMappings.Add(SecurityUtils.SafeCombinePath(archiverRestoreJob.DataVolume, contentName), mapping);
                                }
                            }
                        }
                    }
                }
                logger.Info($"VerifyAndCopyArchiverToHot AllScanedBLOBs: {SecurityUtils.SafeCombinePath(archiverRestoreJob.DataVolume, contentName)}.");
                AllScanedBLOBs.Add(SecurityUtils.SafeCombinePath(archiverRestoreJob.DataVolume, contentName));
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
                logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }
        }

        //public override void ProcessException(Exception e)
        //{
        //    this.currentJobState = 7;
        //    e = e.InnerException ?? e;
        //    this.logger.Error(MediaServiceArchiverBackupResource.ArchvierRestoreToFSServiceProcessExceptionError, e.ToString());
        //}

        public void Dispose()
        {
            try
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
                this.dataLogicalDevice.Close();
                this.DataReader.Close();
                this.IndexService.Close();
                this.StorageDeviceManager.Close(indexLogicalDevice);
                this.CacheManager.CacheSystem.Close();
                this.destinationPhysicalDevice.Close();
                this.CacheManager.Close();
                //if (this.exportService != null)
                //    this.exportService.Close();
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCloseSucceed);
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCloseError, e.ToString());
            }
        }

        /*private void UpdateJobSummary()
        {
            this.jobStatusInfo.Progress = 100;
            if (this.currentJobState != 3)
            {
                this.jobStatusInfo.State = this.currentJobState == 7 ? 7 : 2;
            }
            else
            {
                this.jobStatusInfo.State = 3;
            }
            try
            {
                List<PropertyItem> propertyItems = new List<PropertyItem>();
                if (this.jobStatusInfo.State == 7 || this.jobStatusInfo.State == 3)
                {
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ArchiverRestoreToFSServiceErrorMessage", DefaultValue = MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceErrorMessage });
                }
                else
                {
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ArchiverRestoreToFSSuccessfulMessage", DefaultValue = MediaServiceArchiverBackupResource.ArchiverRestoreToFSSuccessfulMessage });
                }
                if (BLOBMappings.Count > 0)
                {
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "Gui_NewLine" });
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ArchiverRehydrationAzureBlobComments", DefaultValue = "The current job contains data in the Azure archive tier, so it takes time for Blob rehydration from the Archive tier." });
                }
                if (exportOutLimit)
                {
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "Gui_NewLine" });
                    propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ExportOutLimit", DefaultValue = "The total size of exported and/or restored data has exceeded the allowed storage limit." });
                }
                string summaryComments = SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
                JobSummaryMessage summary = new JobSummaryMessage
                {
                    JobStatus = this.jobStatusInfo.State,
                    TotalSize = this.totalSize / 1024,
                    ErrorMessage = summaryComments
                };
                //this.RestoreToFSReportService.SendJobSummary(summary);
            }
            catch (Exception e)
            {
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCloseSendJobSummaryError, e.ToString());
                this.jobStatusInfo.State = 7;
            }
            this.JobProgressUpdater.UpdateJobProgress(jobStatusInfo, maxItemNum, sendItemNum, true);
            this.JobStatusUpdater.UpdateJobStatus(jobStatusInfo);
        }*/
        private void SendItemData(ArchiverBasicIndex index)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!backupJobIds.Contains(index.JobId))
            {
                backupJobIds.Add(index.JobId);
            }
            if (hasRestoredItems.Contains(index.PathMD5))
            {
                logger.Info($"this item has restored,pathmd5:{index.PathMD5}");
                return;
            }
            if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
            {
                throw new JobNeedStopException();
            }
            logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoRestore, index.PathMD5);
            index.OpenType = this.StreamOpenTypeGenerator.GetStreamOpenType(index.CurrentItemVersion);
            index.IsRestoreToFS = true;
            try
            {
                string resultPath = string.Empty;
                if (mRestoreRequest.IsRecenterExport)
                {
                    resultPath = this.TempRestoreFolder;
                }
                else
                {
                    this.itemDetailMessage.DirPath = CombineDirectoryName(index);
                    var tempDirSplit = itemDetailMessage.DirPath.Split('\\');

                    if (tempDirSplit.Length > 0)
                    {
                        foreach (var temp in tempDirSplit)
                        {
                            resultPath = Path.Combine(resultPath, temp);
                        }
                    }
                }
                sw.Stop();
                logger.Info($"linkRestoreReport SendItemData before CreateFolderOrFile cost:{sw.ElapsedMilliseconds}");
                CreateFolderOrFile(index, resultPath);
                this.HasCompleteNode = true;
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
                    this.itemDetailMessage.Message = e.Message;
                    this.itemDetailMessage.Status = 1;
                    this.currentJobState = 7;
                    this.HasErrorNode = true;
                    this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, index.Name);
                    this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, e.ToString());
                }
            }
            catch (Exception ex)
            {
                this.itemDetailMessage.Message = ex.Message;
                this.itemDetailMessage.Status = 1;
                this.currentJobState = 7;
                this.HasErrorNode = true;
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, index.Name);
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, ex.ToString());
            }
            this.JobProgressUpdater.UpdateJobProgress(jobStatusInfo, maxItemNum, ++sendItemNum);
            this.totalSize += index.ContentLength;
            this.currentFolderSize += index.FileRealSize;
            this.UpdateJobReport(index);
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            if (index.Type == "I")
            {
                SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += ContractConstants.ITEMSIZEFORLICENSE;
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, index.Url);
            }
            else
            {
                SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += index.ContentLength;
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(index.FileRealSize, index.Url);
            }
            sw1.Stop();
            logger.Info($"linkRestoreReport AccumulationItemsSize cost:{sw1.ElapsedMilliseconds} ");
            if (index.Type == "D" || index.Type == "I" || index.Type == "A")
            {
                SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                hasRestoredItems.Add(index.PathMD5);
            }
            if ((index.Type == "D" || index.Type == "I" || index.Type == "U" || index.Type == "V")  && this.itemDetailMessage.Status == 0)
            {
                SOArchiverJobInfoStatistics.Instance.ItemAndVersionCountFotTelemetry++;
                SOArchiverJobInfoStatistics.Instance.ItemAndVersionExpireSumTime += SOArchiverJobInfoStatistics.Instance.MainJobStartTime - index.ArchiveTime;
            }
        }

        private void SendItemDataForEnduserDownload(ArchiverBasicIndex index)
        {
            AveSharePointType aveType = (AveSharePointType)index.Type[0];
            if (aveType == AveSharePointType.TYPE_FOLDER || aveType == AveSharePointType.TYPE_LIST ||
                aveType == AveSharePointType.TYPE_SITE || aveType == AveSharePointType.TYPE_WEB|| aveType == AveSharePointType.TYPE_APP)
            {
                return;
            }
            if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
            {
                throw new JobNeedStopException();
            }
            logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoRestore, index.PathMD5);
            index.OpenType = this.StreamOpenTypeGenerator.GetStreamOpenType(index.CurrentItemVersion);
            index.IsRestoreToFS = true;
            try
            {
                string resultPath = this.TempRestoreFolder;
                CreateFolderOrFile(index, resultPath);
            }
            catch (Storage.Util.BlobArchivedException e)
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
                    this.itemDetailMessage.Message = e.Message;
                    this.itemDetailMessage.Status = 1;
                    this.currentJobState = 7;
                    this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, index.Name);
                    this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, e.ToString());
                }
            }
            catch (Exception ex)
            {
                this.itemDetailMessage.Message = ex.Message;
                this.itemDetailMessage.Status = 1;
                this.currentJobState = 7;
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, index.Name);
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceSendItemDataDtoException, ex.ToString());
            }
            this.JobProgressUpdater.UpdateJobProgress(jobStatusInfo, maxItemNum, ++sendItemNum);
            this.totalSize += index.ContentLength;
            this.currentFolderSize += index.FileRealSize;
            this.UpdateJobReport(index);
        }

        //void SendWithConcordanceFormat(ArchiverBasicIndex index)
        //{
        //    var folderName = this.GenerateFolderName();
        //    if (RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
        //    {
        //        throw new JobNeedStopException();
        //    }
        //    var fileName = this.GenerateFileName(index);
        //    index.OpenType = this.StreamOpenTypeGenerator.GetStreamOpenType(index.CurrentItemVersion);
        //    index.IsRestoreToFS = true;
        //    if (index.Type.EqualsIgnoreCase("A") ||
        //        index.Type.EqualsIgnoreCase("D") ||
        //        index.Type.EqualsIgnoreCase("I") ||
        //        index.Type.EqualsIgnoreCase("V") ||
        //        index.Type.EqualsIgnoreCase("U"))
        //    {
        //        var metaData = this.GenerateMetaData(index);
        //        this.DataReader.GetNextItem(index);
        //        var exportInfo = new ExportInfo { FolderName = folderName, FileName = fileName };
        //        try
        //        {
        //            if (index.Type.EqualsIgnoreCase("I") || index.Type.EqualsIgnoreCase("U"))
        //                this.exportService.Export(metaData, exportInfo);
        //            else
        //            {
        //                metaData.FileName = fileName;
        //                this.exportService.Export(DataReader.Input.ReadContent, metaData, exportInfo);
        //            }
        //            this.logger.Info(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestoreFinished, fileName);
        //        }
        //        catch (Exception e)
        //        {
        //            this.logger.Error(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestoreError, fileName, e.ToString());
        //        }
        //    }
        //    this.totalSize += index.ContentLength;
        //    var detail = new ItemDetailMessage { Name = fileName, Type = index.Type, ContentLength = index.ContentLength };
        //    this.RestoreToFSReportService.SendDetailReport(detail);
        //    this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, this.maxItemNum, ++this.sendItemNum);
        //}
        private void UpdateJobReport(ArchiverBasicIndex index)
        {
            try
            {
                this.ReportManager.Increase();
                AddRestoreReport(index.ExtraInfo,index.ContentLength, itemDetailMessage.Status, index.Type, 0, index.PathMD5, itemDetailMessage.Message);
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceUpdateJobReportException, e.Message);
            }
        }

        private String CombineDirectoryName(ArchiverBasicIndex index)
        {
            string name = index.Name;
            switch (index.Type)
            {
                case "E":
                    if (name.StartsWith("http://"))
                    {
                        name = name.Remove(0, "http://".Length);
                    }
                    else if (name.StartsWith("https://"))
                    {
                        name = name.Remove(0, "https://".Length);
                    }
                    name = name.Replace("/", "_");
                    lastSite = new StringBuilder().Append(GeneratorFarmName(this.archiverRestoreJob.FarmName)).Append("\\").Append(name).Append("\\");
                    lastList = lastSite;
                    break;
                case "W":
                case "L":
                case "F":
                    hadHandledAttachment = false;
                    if (name.Equals(".", StringComparison.OrdinalIgnoreCase))
                    {
                        lastList = new StringBuilder(lastSite.ToString());
                        break;
                    }
                    if (name.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.Remove(0, ".\\".Length);
                    }
                    lastList = new StringBuilder(lastSite.ToString()).Append(name).Append("\\");
                    break;
                case "A":
                    //attachment类型的处理:创建一个item name +"Attachment"的文件夹，然后将attachment写入到这个文件夹中
                    if (hadHandledAttachment != true)
                    {
                        int colonPosition = name.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                        lastList = new StringBuilder(lastList.ToString()).Append(colonPosition > 0 ? name.Substring(0, colonPosition) : name).Append("Attachment\\");
                        hadHandledAttachment = true;
                    }
                    else
                    {
                        int colonPosition = name.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                        // Fix issues when item ID is larger or equal to 10
                        var pathSegments = new List<string> { string.Empty };
                        pathSegments.AddRange(lastList.ToString().Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries));
                        pathSegments.RemoveAt(pathSegments.Count - 1);
                        lastList = new StringBuilder(string.Join("\\", pathSegments))
                            .Append('\\')
                            .Append(colonPosition > 0 ? name.Substring(0, colonPosition) : name)
                            .Append("Attachment\\");
                    }
                    break;
            }
            string fullPath = this.TempRestoreFolder + lastList.ToString();
            string[] segments = fullPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = segments[i].Replace(":", "_").Replace(".", "_");
            }
            return string.Join("\\", segments);
        }

        private String GeneratorFarmName(string farmName)
        {
            var realFarmName = farmName.Contains("\\") && farmName.Contains(":") ?
                farmName.Replace(":", "#").Replace("\\", "#")
                : ConvertFarmNameToUpper(farmName.Replace(":", "#"));
            return realFarmName;
        }

        private String ConvertFarmNameToUpper(String farmName)
        {
            return farmName.Contains("(") ? "Farm" + farmName.ToUpper().Substring(farmName.IndexOf("(", StringComparison.OrdinalIgnoreCase)) : farmName;
        }

        private void CreateFolderOrFile(ArchiverBasicIndex index, String dirPath)
        {
            string fileName = index.Name;
            int colonPosition = fileName.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            AveSharePointType aveType = (AveSharePointType)index.Type[0];
            if (aveType == AveSharePointType.TYPE_FOLDER || aveType == AveSharePointType.TYPE_LIST ||
                aveType == AveSharePointType.TYPE_SITE || aveType == AveSharePointType.TYPE_WEB)
            {
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileOpen, dirPath);
                StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), string.Empty);
                if (!this.CacheManager.CacheSystem.DirectoryExists(info))
                {
                    this.CacheManager.CacheSystem.OpenDirectory(info, FileMode.OpenOrCreate);
                    this.itemDetailMessage.Status = 0;
                }
                else
                {
                    this.itemDetailMessage.Status = 10;//if status is 10,not report
                }
            }
            else if (aveType == AveSharePointType.TYPE_ATTACHMENTS)
            {
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileOpen, dirPath);
                // create new folder for Attachment
                StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), string.Empty);
                this.CacheManager.CacheSystem.OpenDirectory(info, FileMode.OpenOrCreate);
                fileName = fileName.Substring(colonPosition + 1);
                info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), ReplaceInvalidChar(fileName, true));
                WriteDataToFile(info, index);
            }
            else if (aveType == AveSharePointType.TYPE_DOCUMENT || aveType == AveSharePointType.TYPE_VERSION)
            {
                //documment类型含version的处理:(举例说明a.txt:1.0 ---> a_1.0.txt)
                var name = index.ItemName.LastIndexOfIgnoreCase(".") >= 0 ? index.ItemName.Remove(index.ItemName.LastIndexOfIgnoreCase(".")) : index.ItemName;
                var extension = index.ItemName.LastIndexOfIgnoreCase(".") >= 0 ? index.ItemName.Substring(index.ItemName.LastIndexOfIgnoreCase(".")) : null;
                var newFileName = colonPosition > 0 ? name + '_' + index.Name.Substring(colonPosition + 1) + extension : index.Name;
                StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), ReplaceInvalidChar(newFileName, true));
                WriteDataToFile(info, index, hasFileVersion: colonPosition > 0);
            }
            else
            {
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileNotWrite, fileName, aveType);
                this.itemDetailMessage.Status = 2;
            }
        }

        private void WriteDataToFile(StorageInfo info, ArchiverBasicIndex index, bool hasFileVersion = false)
        {
            if (!Directory.Exists(info.HighName))
            {
                Directory.CreateDirectory(info.HighName);
            }

            // Truncate overlong file names before any further processing (keep extension when possible).
            if (!string.IsNullOrEmpty(info.LowName))
            {
                string originalName = info.LowName;
                info.LowName = TruncateFileName(info.LowName, hasFileVersion);
                if (!originalName.Equals(info.LowName, StringComparison.Ordinal))
                {
                    string originalFullPath = System.IO.Path.Combine(info.HighName ?? string.Empty, originalName);
                    string truncatedFullPath = System.IO.Path.Combine(info.HighName ?? string.Empty, info.LowName);
                    // 统一输出：原始文件名 / 截取后文件名 / 原始完整路径 / 截取后完整路径
                    logger.Warn($"[RestoreFileNameTruncate] OriginalName='{originalName}' TruncatedName='{info.LowName}' MaxLimit={MAX_FILE_NAME_LENGTH}chars JobId={archiverRestoreJob?.ParentJobId} IndexPathMD5={index?.PathMD5}");
                    logger.Warn($"[RestoreFileNameTruncatePath] OriginalFullPath='{originalFullPath}' => NewFullPath='{truncatedFullPath}'");
                }
            }

            bool isFileExists = this.CacheManager.CacheSystem.FileExists(info);
            if (isFileExists && mRestoreRequest.IsRecenterExport)
            {
                string fileName = Path.GetFileNameWithoutExtension(info.LowName);
                string fileExtend = Path.GetExtension(info.LowName);
                int fileNameIndex = 1;
                while (isFileExists)
                {
                    info.LowName = $@"{fileName}({fileNameIndex++}){fileExtend}";
                    // Ensure still within limit after adding
                    info.LowName = TruncateFileName(info.LowName, hasFileVersion);
                    isFileExists = this.CacheManager.CacheSystem.FileExists(info);
                }
            }
            
            byte[] buffer = new byte[64 * 1024];
            try
            {
                DataReader.GetNextItem(index);
            }
            catch (Storage.Util.BlobArchivedException e)
            {
                logger.Warn("Storage blob file has been archived,try Rehydration Data.");
                throw;
            }
            catch (SkipRetryException e)
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
                this.itemDetailMessage.Status = 0;
            }
            else if ((isFileExists && this.archiverRestoreJob.ArchiveRestoreOption != RestoreOption.OverWrite))
            {
                this.itemDetailMessage.Status = 2;
            }
            //if (!Convert.ToBoolean(index.IsSystemFile))
            {
                logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceWriteDataToFileWrite, index.PathMD5, info.HighName);
                //if (!isFileExists || this.archiverRestoreJob.ArchiveRestoreOption == RestoreOption.OverWrite)
                if (!isFileExists) // Currently, the out-of-place restore job don't have ArchiveRestoreOption, so we will skip file if it already exists.
                {
                    using (XStream stream = this.CacheManager.CacheSystem.OpenStream(info, isFileExists ? FileMode.Truncate : FileMode.CreateNew))
                    {
                        if (index.ContentLength != 0L)
                        {
                            Stopwatch sw1 = new Stopwatch();
                            sw1.Start();
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
                            this.itemDetailMessage.Status = 0;
                            sw1.Stop();
                            logger.Info($"linkRestoreReport WriteDataToFile read and write cost time:{sw1.ElapsedMilliseconds}");
                        }
                    }
                }
            }
        }
        private string TruncateFileName(string fileName, bool hasFileVersion = false)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            if (fileName.Length <= MAX_FILE_NAME_LENGTH) return fileName;
            string extension = Path.GetExtension(fileName);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            int allowed = MAX_FILE_NAME_LENGTH - (extension?.Length ?? 0);
            string version = string.Empty;
            if (hasFileVersion)
            {
                version = nameWithoutExt.Substring(nameWithoutExt.LastIndexOf('_'));
                allowed -= version.Length;
            }
            if (allowed <= 0)
            {
                return fileName.Substring(0, MAX_FILE_NAME_LENGTH);
            }
            if (nameWithoutExt.Length > allowed)
            {
                nameWithoutExt = nameWithoutExt.Substring(0, allowed);
            }
            return nameWithoutExt + "..." + version + extension;
        }
        private Stream GetDataStream(ArchiverBasicIndex index)
        {
            byte[] buffer = new byte[64 * 1024];
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                DataReader.GetNextItem(index);
                DataReader.Input.CurrentItemIndex.IsRestoreToFS = true;
            }
            catch (Storage.Util.BlobArchivedException e)
            {
                logger.Warn("Storage blob file has been archived,try Rehydration Data.");
                throw;
            }
            catch (SkipRetryException e)
            {
                if (e.Message.Contains("This operation is not permitted on an archived blob."))
                {
                    logger.Warn("Storage blob file has been archived,try Rehydration Data.");
                    throw;
                }
                throw;
            }
            DataReader.Input.BeginRead(FileType.Content);
            while (true)
            {
                int len = DataReader.Input.ReadContent(buffer, 0, buffer.Length);
                if (len <= 0) break;
                memoryStream.Write(buffer, 0, len);
            }
            DataReader.Input.EndRead(FileType.Content);
            return memoryStream;
        }
        String ReplaceInvalidChar(String srcStr, bool isFile)
        {
            Char[] invalidCS = Path.GetInvalidFileNameChars();
            var sep = Path.DirectorySeparatorChar;
            foreach (char c in invalidCS)
            {
                srcStr = !(!isFile && c == sep) ? srcStr.Replace(c, '_') : srcStr;
            }
            return srcStr;
        }

        //String GenerateFileName(ArchiverBasicIndex index)
        //{
        //    var fileName = index.Name;
        //    var flag = index.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
        //    if (index.Type.EqualsIgnoreCase("A"))
        //        fileName = index.Name.Substring(flag + 1);
        //    else if (index.Type.EqualsIgnoreCase("D") || index.Type.EqualsIgnoreCase("V"))
        //    {
        //        var name = index.ItemName.Remove(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
        //        var extension = index.ItemName.Substring(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
        //        fileName = flag > 0 ? name + '_' + index.Name.Substring(flag + 1) + extension : index.Name;
        //    }
        //    return fileName;
        //}

        //ConcordanceMetaData GenerateMetaData(ArchiverBasicIndex index)
        //{
        //    var metaDataInfo = new HashSet<MetaDataItemInfo>();
        //    var userDefined = index.Attributes.Split(ServiceConstants.ExtraChar);
        //    var metaData = new ConcordanceMetaData() { ContentSize = index.ContentLength };
        //    foreach (String column in userDefined)
        //    {
        //        var seperatorIndex = column.IndexOf(ServiceConstants.Delimiter);
        //        if (seperatorIndex > 0 && seperatorIndex + 1 != column.Length)
        //        {
        //            var columnName = column.Remove(seperatorIndex);
        //            var columnValue = column.Substring(seperatorIndex + 1);
        //            if (columnName.EqualsIgnoreCase("Title"))
        //                metaData.Title = columnValue;
        //            else
        //                metaDataInfo.Add(new MetaDataItemInfo(columnName, columnValue, typeof(String)));
        //        }
        //    }
        //    metaDataInfo.Add(new MetaDataItemInfo("Type", index.Type.ToNodeLevelByMediaDataTypeString().ToString(), typeof(String)));
        //    metaData.MetadataInfo = metaDataInfo;
        //    return metaData;
        //}

        //String GenerateFolderName()
        //{
        //    var temp = this.archiverRestoreJob.SiteUrl.Replace("://", "#");
        //    var siteUrl = temp.Replace("/", "#").Replace(":", "#");
        //    var folderName = Path.Combine(this.archiverRestoreJob.ParentJobId, siteUrl);
        //    return folderName;
        //}
        private string GetRestoreCenterExportZipFileName(string siteUrl)
        {
            if(folderIndex == 1)
            {
                return this.tempRestoreFolder + "_" + ConvertSiteUrlToPath(siteUrl)+".zip";
            }
            else
            {
                return this.tempRestoreFolder + "_" + ConvertSiteUrlToPath(siteUrl) + "(" + folderIndex + ")" +".zip";
            }
        }

        private void ZipAndUploadFile(string siteUrl,bool isRecenterExport)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var restoreFolderPath = Path.Combine(this.CacheManager.CacheSystem.SystemLocation, this.TempRestoreFolder);
            if (!Directory.Exists(restoreFolderPath))
            {
                logger.Info($@"Exist zip and upload file, because : {restoreFolderPath} not exist");
                return;
            }
            string zipFileName = string.Empty;
            StorageInfo storageInfo = new StorageInfo(); ;
            if (isRecenterExport)
            {
                zipFileName = GetRestoreCenterExportZipFileName(siteUrl);
                storageInfo = new StorageInfo { HighName = "ArchivedExportContent" + "\\" + TenantLocalValue.LogonGroupId+"\\"+this.TempRestoreFolder, LowName = zipFileName };
            }
            else
            {
                zipFileName = this.TempRestoreFolder + ".zip";
            }
            StorageInfo tempStorageInfo = new StorageInfo { HighName = string.Empty, LowName = zipFileName };
            var zipFilePath = Path.Combine(this.CacheManager.CacheSystem.SystemLocation, zipFileName);
            try
            {
                ZipUtil.ZipFolder(restoreFolderPath, zipFilePath, this.archiverRestoreJob.ZipFilePassword, Encoding.UTF8);
                //ZipUtil.ZipFolder(restoreFolderPath, zipFilePath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                this.logger.Warn($"zip the directory {restoreFolderPath} failed, maybe the path is too long, try to zip with alphaFS. {e.ToString()}");
                ZipUtil.ZipFolderForLongPath(restoreFolderPath, zipFilePath, this.archiverRestoreJob.ZipFilePassword, Encoding.UTF8);
            }
            this.logger.Info("Restore job summary total size is:{0}", this.totalSize);
            var lenth = this.CacheManager.CacheSystem.OpenFile(tempStorageInfo).FileSize;
            storageInfo.Length=lenth;
            tempStorageInfo.Length= lenth;
            this.logger.Info("The restore zip file size is:{0}", lenth);
            using (XStream cacheStream = this.CacheManager.CacheSystem.OpenStream(tempStorageInfo, FileMode.Open))
            {
                if (isUpdateExportLimitSize)
                {
                    if (this.destinationPhysicalDevice.StorageType == XStorageType.Azure)
                    {
                        //var mArchiverJobManagementService = JobReportServiceFactory.CreateArchiverJobManagementService();
                        //mArchiverJobManagementService.UpdadeAndCheckDataSize(tenantGroupId, storageInfo.Length);
                    }
                }
                sw.Stop();
                logger.Info($"linkRestoreReport zip current export data cost:{sw.ElapsedMilliseconds}");
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();
                if (isRecenterExport)
                {
                    this.destinationPhysicalDevice.CommitStream(cacheStream, storageInfo);
                }
                else
                {
                    this.destinationPhysicalDevice.CommitStream(cacheStream, tempStorageInfo);
                }
                sw2.Stop();
                logger.Info($"linkRestoreReport upload this zip cost:{sw2.ElapsedMilliseconds}");
            }
            this.DeleteCachenfo();
            folderIndex++;
            currentFolderSize = 0;
        }
        private string ConvertSiteUrlToPath(string siteUrl)
        {
            if (siteUrl.StartsWith("http://"))
            {
                siteUrl = siteUrl.Remove(0, "http://".Length);
            }
            else if (siteUrl.StartsWith("https://"))
            {
                siteUrl = siteUrl.Remove(0, "https://".Length);
            }
            return PathValidation.ConverSpecialChar(siteUrl, "_");
        }
        private void DeleteCachenfo()
        {
            try
            {
                var tempFolderInfo = new StorageInfo { HighName = this.TempRestoreFolder, LowName = String.Empty };
                var cacheFileInfo = new StorageInfo { HighName = String.Empty, LowName = this.TempRestoreFolder + ".zip" };
                try
                {
                    if (this.CacheManager.CacheSystem.DirectoryExists(tempFolderInfo))
                        this.CacheManager.CacheSystem.DeleteDirectory(tempFolderInfo);
                }
                catch (Exception e)
                {
                    logger.Warn("delete cache directory data warn", this.archiverRestoreJob.JobId, e.ToString());
                }

                try
                {
                    if (this.CacheManager.CacheSystem.FileExists(cacheFileInfo))
                        this.CacheManager.CacheSystem.DeleteFile(cacheFileInfo);
                }
                catch (Exception e)
                {
                    logger.Warn("delete cache file data warn", this.archiverRestoreJob.JobId, e.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Warn("delete cache data warn", this.archiverRestoreJob.JobId, e.ToString());
                //this.DeleteCachenfoForLongPath();
            }
        }

        public async Task<Stream> GetStubStreamForPreviewAsync(ArchiverRestoreRequest message)
        {
            return await GetStubStreamAsync(message);
        }
    }
    internal static class PathValidation
    {
        private static Char[] CustomerChars = new Char[] { '!' };
        private static string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + new string(CustomerChars);
        private static Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

        public static string ConverSpecialChar(string originalString, string ConvertIllegalCharacterTo)
        {
            return r.Replace(originalString, ConvertIllegalCharacterTo);
        }
    }
}