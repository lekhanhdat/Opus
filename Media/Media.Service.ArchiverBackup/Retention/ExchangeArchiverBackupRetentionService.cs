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
    using AvePoint.Common;
    using AvePoint.Common.Portal;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Archiver;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.DisposalStubDao;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.DB.Model;
    using AvePoint.RA.I18N.Core;
    using AvePoint.RA.RACommonUtility;
    using AvePoint.RA.RACommonUtility.Browser;
    using AvePoint.RA.RACommonUtility.Common;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Office;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using DocumentFormat.OpenXml.Office2010.Excel;
    using DocumentFormat.OpenXml.Presentation;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Storage;
    using Storage.Cloud.Azure;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using Util;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
    using BposConnectionType = GCommon.Contract.CentralAdmin.Object.BposConnectionType;
    using RetentionRule = DomainModel.RetentionRule;


    //using AvePoint.Common.RemoteNode.Impl.RemoteO365AccountService;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/3/7",
    "dwxue@avepoint.com",
    "xiaofeiwang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_8 },
    "ADO-26066",
    false)]

    #endregion CodeReview

    public class ExchangeArchiverBackupRetentionService
        : RetentionServiceBase<ArchiverRetentionInfo, ArchiverRetentionResult>
        , IRetentionService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ArchiverRetentionInfo archiverRetentionInfo = new ArchiverRetentionInfo();
        JobStatusInfo jobStatusInfo = new JobStatusInfo();
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        IXSystem destinationLogicalDevice;
        Int64 deleteDataSize = default(Int64);
        String dataVolume;
        String indexVolume;
        AccessTierType accessTierType;
        Boolean isObjectType;
        String ErrorMessage = ServiceConstants.ArchvierRetentionFailedMessage;
        public SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();
        private String rehydrationTemp;
        private readonly Object rehydrationLock = new Object();
        private Boolean destinationStoreInArchiverTier;
        //List<MediaArchiverRetentionInfo> retentionInfo = new List<MediaArchiverRetentionInfo>();
        private static readonly IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private bool isArchiveTierToColdTier;
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IArchiverRetentionIndexService RetentionIndexService { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }
        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexSubProcessor = null;

        private int CurrentProgress { get; set; }

        public IMArchiverJobManagementService ArchiverJobManagementService { get; set; }

        public IStorageDeviceManager DeviceManager { get; set; }
        private IAveORecords Record
        {
            get
            {
                IAveORecords records = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.Auto).CreateRecords();
                return records;
            }
        }
        private bool IsLorealSoftDelete;
        private BlobContainerClient sourceContainerClient;
        private static IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();

        public override void Open(ArchiverRetentionInfo retentionInfo)
        {
            this.archiverRetentionInfo = retentionInfo;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceOpenStart, this.archiverRetentionInfo.JobId);
            this.jobStatusInfo.State = 2;
            this.dataVolume = retentionInfo.DataVolume;
            this.indexVolume = retentionInfo.IndexVolume;
            this.accessTierType = retentionInfo.AccessTierType;
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            this.indexLogicalDevice = this.DeviceManager.Open(this.archiverRetentionInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.dataLogicalDevice = XFactory.InstanceSystem(this.archiverRetentionInfo.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data)[0]);
            dataLogicalDevice.Open();
            this.CacheManager.Open(retentionInfo.CacheSetting, false, true);
            this.isObjectType = this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object) || this.indexLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object);
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);
            if (this.archiverRetentionInfo.RetentionRule == RetentionRule.MoveArchiverJobData)
            {
                this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDevice);
                this.destinationLogicalDevice = this.DeviceManager.Open(retentionInfo.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Data));
                this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDeviceFinished);
            }
            this.OpenMainIndex(this.archiverRetentionInfo, this.indexVolume);
            if (retentionInfo.RetentionRule == RetentionRule.MarkArchiverJobDataTier || (retentionInfo.IsSoftDelete && !retentionInfo.IsFitSoftDelete))
            {
                this.OpenSubIndex(this.archiverRetentionInfo, this.indexVolume);
            }
            this.destinationStoreInArchiverTier = retentionInfo.DestinationStoreInArchiverTier;
            this.rehydrationTemp = "data_archive\\Temp\\" + Guid.NewGuid();
            IsLorealSoftDelete = IsEnabledRealDelete();
        }

        public override ArchiverRetentionResult Retain(ArchiverRetentionInfo retentionInfo)
        {
            var retentionResult = new ArchiverRetentionResult();
            switch (this.archiverRetentionInfo.RetentionRule)
            {
                case RetentionRule.RetainArchiverJobData:
                    retentionResult = this.RetainJobData();
                    break;
                case RetentionRule.MoveArchiverJobData:
                    retentionResult = this.MoveJobData();
                    break;
                case RetentionRule.MarkArchiverJobDataTier:
                    retentionResult = this.MarkJobDataTier();
                    break;
                default:
                    throw new UnknownFileTypeException(String.Format(MediaServiceArchiverBackupResource.RetentionServiceRetainUnknownFileTypeException, this.archiverRetentionInfo.RetentionRule.ToString()));
            }
            return retentionResult;
        }

        public override void GenerateJobReport(Int32 jobState)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportBegin, this.archiverRetentionInfo.JobId);
            //var jobDetailList = new List<JobDetail>();
            //var jobSummaryList = new List<JobSummary>();
            //try
            //{
            //    var jobDetail = new JobDetail();

            //    var physicalDevicePath = new StringBuilder();
            //    jobDetail.Size = this.deleteDataSize;
            //    jobDetail.Status = this.jobStatusInfo.State.Equals(jobState) ?
            //        Convert.ToInt32(JobReportDetailStatus.Success) : Convert.ToInt32(JobReportDetailStatus.Failed);
            //    jobDetail.Message = this.jobStatusInfo.State.Equals(jobState) ?
            //        ServiceConstants.ArchiverRetentionSuccessfulMessage : ServiceConstants.ArchvierRetentionFailedMessage;
            //    jobDetail.Remark7 = this.archiverRetentionInfo.DataLogicalDevice.Name;
            //    jobDetail.Remark8 = this.archiverRetentionInfo.RetentionAction == MediaArchiverRetentionAction.MoveData ? this.archiverRetentionInfo.DestinationDevice.Name : String.Empty;
            //    jobDetail.Remark9 = this.archiverRetentionInfo.RetentionAction == MediaArchiverRetentionAction.MoveData ? "Move the Data to Logical Device" : "Delete the Data";
            //    jobDetail.Remark11 = this.archiverRetentionInfo.DataLogicalDevice.Id;
            //    jobDetail.Remark12 = this.archiverRetentionInfo.RetentionAction == MediaArchiverRetentionAction.MoveData ? this.archiverRetentionInfo.DestinationDevice.Id : String.Empty;
            //    jobDetailList.Add(jobDetail);
            //    jobSummaryList.Add(new JobSummary()
            //    {
            //        EntityType = Convert.ToInt32(JobReportDetailEntityType.NormalInfo),
            //        Key = "DataSize",
            //        Value = (this.deleteDataSize / 1024).ToString(),
            //        SubJobId = this.archiverRetentionInfo.JobId,
            //    });
            //    String summaryComments = string.Empty;

            //    if (BLOBMappings.Count > 0)
            //    {
            //        String str = jobState == 2 ? ServiceConstants.ArchiverRetentionSuccessfulMessage : this.ErrorMessage;
            //        List<PropertyItem> propertyItems = new List<PropertyItem>();
            //        propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = str, DefaultValue = str });
            //        propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "Gui_NewLine" });
            //        propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ArchiverRehydrationAzureBlobComments", DefaultValue = "The current job contains data in the Azure archive tier, so it takes time for Blob rehydration from the Archive tier." });

            //        summaryComments = SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
            //    }
            //    else
            //    {
            //        summaryComments = jobState == 2 ? ServiceConstants.ArchiverRetentionSuccessfulMessage : this.ErrorMessage;
            //    }
            //    jobSummaryList.Add(new JobSummary()
            //    {
            //        Key = "Comments",
            //        Value = summaryComments.ToString()
            //    });
            //    this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportComplete, this.archiverRetentionInfo.JobId);
            //    SubJobDto subJobInfo = new SubJobDto() { Id = this.archiverRetentionInfo.RetentionJob.Id, ParentId = this.archiverRetentionInfo.RetentionJob.Id.Split('_')[0] };
            //    JobDetailService.UpdateSubJobDetails(jobDetailList, subJobInfo);
            //    JobDetailService.UpdateSubJobSummary(jobSummaryList, subJobInfo);
            //    //this.ControlStubs.JobDetailService.UpdateJobDetails(jobDetailList, this.archiverRetentionInfo.RetentionJob);
            //    //this.ControlStubs.JobDetailService.UpdateJobSummary(jobSummaryList, this.archiverRetentionInfo.RetentionJob);
            //}
            //catch (Exception ex)
            //{
            //    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportError, this.archiverRetentionInfo.JobId, ex.ToString());
            //}
        }

        public override void UpdateJobStatusAndControlTable(Int32 jobState)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceUpdateJobStatusAndControlTableBegin);

            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceUpdateJobStatusAndControlTableEnd, this.archiverRetentionInfo.JobId);
        }

        public override void ProcessException(Exception e, ArchiverRetentionResult result)
        {
            e = e.InnerException ?? e;
            switch (this.archiverRetentionInfo.RetentionRule)
            {
                case RetentionRule.RetainArchiverJobData:
                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataDeleteDataError, this.archiverRetentionInfo.JobId, e.ToString());
                    break;
                case RetentionRule.MoveArchiverJobData:
                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataError, this.archiverRetentionInfo.JobId, e.ToString());
                    break;
            }
        }

        public override void Dispose()
        {
            this.UploadIndexToRealSystem();
            if (this.IndexService != null && this.archiverRetentionInfo.RetentionRule.Equals(RetentionRule.RetainArchiverJobData))
            {
                this.IndexService.Close();
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
            if (this.DeviceManager != null)
            {
                this.DeviceManager.Close(this.indexLogicalDevice);
                this.DeviceManager.Close(this.dataLogicalDevice);
                this.DeviceManager.Close(this.destinationLogicalDevice);
            }
        }

        private ArchiverRetentionResult MoveJobData()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataBegin, this.archiverRetentionInfo.JobId);
            this.deleteDataSize = this.MoveDataFromDevice(this.dataVolume, this.indexVolume);
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataFinished, this.archiverRetentionInfo.JobId, this.deleteDataSize.ToString());
            var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            result.Size = deleteDataSize;
            result.State = 2;
            return result;
        }

        private ArchiverRetentionResult RetainJobData()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataBegin, this.archiverRetentionInfo.JobId);
            logger.Info($"retention type and soft delete info is:retentionType:{this.archiverRetentionInfo.RetentionDataTimeType},isFitSoftDelete:{this.archiverRetentionInfo.IsFitSoftDelete},isSoftDelete:{this.archiverRetentionInfo.IsSoftDelete}");
            if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime && this.archiverRetentionInfo.IsSoftDelete)
            {
                logger.Info("this action is retain by modified time delete");
                this.deleteDataSize = this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, true, true);
                this.SoftDelete(this.dataVolume, this.indexVolume);
            }
            else if (!this.archiverRetentionInfo.IsFitSoftDelete && this.archiverRetentionInfo.IsSoftDelete)
            {
                logger.Info("this action is soft delete");
                this.deleteDataSize = this.SoftDelete(this.dataVolume, this.indexVolume);
            }
            else
            {
                logger.Info("this action is real delete");
                this.deleteDataSize = this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, true);
            }
            //this.deleteDataSize += this.DeleteIndexFromDevice();
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataDeleteDataFinished, this.archiverRetentionInfo.JobId, this.deleteDataSize.ToString());
            var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            result.Size = deleteDataSize;
            result.State = 2;
            result.HasIndexRelatedToBackupJob = IsExistsIndexRelatedToJob(this.archiverRetentionInfo.JobId);
            return result;
        }

        
        private ArchiverRetentionResult MarkJobDataTier()
        {
            this.logger.Info($"start mark job data tier,{this.archiverRetentionInfo.JobId}");
            this.MarkDataTierFromDevice(this.archiverRetentionInfo, this.dataVolume);
            this.logger.Info($"finish mark job data tier,{this.archiverRetentionInfo.JobId}");
            var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            if (isArchiveTierToColdTier)
            {
                result.IsArchiveTierToColdTier = true;
            }
            result.State = 2;
            return result;
        }
        private void MarkDataTierFromDevice(ArchiverRetentionInfo retentionInfo, String dataVolume)
        {
            Boolean isMarkSucceedAtLeastOnce = false;
            var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
            var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(retentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
            logger.Info($"Need mark blobs count : {fileList.Count}");
            fileList.ForEach(item =>
            {
                var info = XConvert.FromNames(item.HighName, item.Name);
                var file = this.dataLogicalDevice.OpenFile(info);
                try
                {
                    SetFileTierArchiveAsync(dataLogicalDevice, info, file).GetAwaiter().GetResult();
                    isMarkSucceedAtLeastOnce = true;
                }
                catch (Exception ex)
                {
                    if (!isMarkSucceedAtLeastOnce)
                    {
                        this.ErrorMessage = ex.Message;
                        this.jobStatusInfo.State = 3;
                        this.logger.Error($"mark data tier failed,{info.LowName} error:{ex.ToString()}");
                        throw;
                    }
                    else
                    {
                        this.jobStatusInfo.State = 7;
                        this.logger.Error($"mark data tier all failed,{info.LowName} error:{ex.ToString()}");
                    }
                }
            });
            //this.RetentionIndexService.UpdateDataFromMainIndex(this.accessTierType == AccessTierType.Archive, retentionInfo.JobId);
            UpdateSubIndexTier(this.accessTierType == AccessTierType.Archive, retentionInfo.JobId);
        }
        private void UpdateSubIndexTier(bool isArchiveTier, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = jobId;
            parameters["@tier"] = isArchiveTier ? (int)Storage.AccessTierType.Archive : 0;
            var deleteBodyTable = "update " + IndexConstants.TableNameArchiveBody + " set COL_EXTENSION_2 = @tier where COL_JOBID = @jobId";
            this.IndexSubProcessor.Execute(deleteBodyTable, parameters);
            this.IndexMainProcessor.Execute(deleteBodyTable, parameters);
        }
        private async Task SetFileTierArchiveAsync(IXSystem destinationDevice, StorageInfo storageInfo, XFileInfo file)
        {
            try
            {
                using (new CheckJobStopScope()) { }
                if (destinationDevice.StorageType == XStorageType.Azure)
                {
                    if (file is AzureCloudInfo)
                    {
                        var tempFile = file as AzureCloudInfo;
                        if (tempFile?.FileTierType == AccessTierType.Archive && this.accessTierType == AccessTierType.Cold)
                        {
                            isArchiveTierToColdTier = true;
                        }
                        if (this.accessTierType != tempFile.FileTierType)
                        {
                            var device = destinationDevice as IAzureSystem;
                            AzureCloudInfo info = new AzureCloudInfo();
                            info.HighName = storageInfo.HighName;
                            info.LowName = storageInfo.LowName;
                            info.FileTierType = this.accessTierType;
                            var result = await device.ChangeFileTierAsync(info);
                            if (!result.IsChanged)
                                logger.Warn("An error occurred while setting file tier. FileName: {0}", storageInfo.LowName);
                        }
                        else
                        {
                            logger.Info($"will not mark tier,tempFile.tier:{tempFile?.FileTierType.ToString()},accessTierType:{this.accessTierType}. FileName: {storageInfo.LowName}");
                        }
                    }
                }
                else
                {
                    throw new Exception("RM_MR_MarkTier_ErrorMessage");
                }
            }
            catch (JobStopException ex)
            {
                logger.Warn("job is stopped when SetFileTierArchive");
                throw;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while setting file tier. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
                throw;
            }
        }

        private bool IsExistsIndexRelatedToJob(string jobId)
        {
            //var parameters = new Dictionary<String, Object>();
            //parameters["@JobId"] = $"%{jobId}%";
            //var deleteBodyTable = $"SELECT COL_ID FROM {IndexConstants.TableNameArchiveBody} WHERE COL_POOL_GUID LIKE @JobId LIMIT 1;";
            //var result = this.IndexMainProcessor.ExecuteScalar(deleteBodyTable, parameters);
            //return result != null;
            return false; // Teams & Mailbox not support deduplication
        }

        private Int64 DeleteFileByStorageInfo(List<String> storageInfoList)
        {
            //如果有一个storage info删除失败，则认为整个retention失败
            var deleteResult = new StorageDeleteResult();
            var result = new StorageDeleteResult();
            Boolean isDeleteSucceed = false;
            Int32 deleteTime = 1;
            Int32 totalRetentionTimes = storageInfoList.Count;
            foreach (String storageInfo in storageInfoList)
            {
                var info = new StorageInfo
                {
                    ExtraStorageInfo = storageInfo,
                };
                try
                {
                    result = this.dataLogicalDevice.DeleteFile(info);
                    ChangeLorealBlobFromPreviousVersionToDelete(info);
                    isDeleteSucceed = true;
                    this.logger.Debug(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoInfo, storageInfo);
                }
                catch (Exception ex)
                {
                    if (!isDeleteSucceed)
                    {
                        this.ErrorMessage = ex.Message;
                        this.jobStatusInfo.State = 3;
                        this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                        throw;
                    }
                    else
                    {
                        this.jobStatusInfo.State = 7;
                        this.logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoWarn, storageInfo, ex.ToString());
                    }
                }
                deleteResult.DeletedFileSize += Math.Max(result.DeletedFileSize, 0);
            }
            return deleteResult.DeletedFileSize;
        }

        private Int64 MoveDataFromDevice(string dataVolume, string indexVolume)
        {
            var tempDeleteDataSize = default(Int64);
            var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
            var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
            logger.Info($"Need move blobs count : {fileList.Count}");
            tempDeleteDataSize = this.MoveAndDeleteFileFromDevice(this.dataLogicalDevice, this.destinationLogicalDevice, fileList);
            return tempDeleteDataSize;
        }

        private void VerifyAndCopyArchiverToHot(StorageInfo info)
        {
            var file = this.dataLogicalDevice.OpenFile(info);

            if (file is AzureCloudInfo)
            {
                var azureFile = file as AzureCloudInfo;
                if (file != null && azureFile.FileTierType == AccessTierType.Archive)
                {
                    string temp = Path.Combine(rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + 11));
                    lock (rehydrationLock)
                    {
                        if (!BLOBMappings.ContainsKey(info.HighPlusLowName))
                        {
                            azureFile.FileTierType = AccessTierType.Archive;
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
                }
            }
        }


        private Int64 MoveAndDeleteFileFromDevice(IXSystem sourceDevice, IXSystem destinationDevice, List<XFileInfo> fileList)
        {
            Int32 moveTime = 1;
            Int32 totalMoveTimes = fileList.Count * 2;
            fileList.ForEach(item =>
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
            catch (JobStopException e)
            {
                logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }
            var retentionInfo = this.archiverRetentionInfo;
            fileList.ForEach(item =>
            {
                var info = XConvert.FromNames(item.HighName, item.Name);
                info.MetaInfos.Add("Archive-KeepTime", retentionInfo.RetentionTimeSpanSeconds.ToString());
                info.MetaInfos["Platform"] = ServiceConstants.DocAve;
                info.MetaInfos["Component"] = "ArchiverBackup";
                info.MetaInfos["Archive-FarmName"] = retentionInfo.FarmName;
                info.MetaInfos["Archive-WebAppName"] = retentionInfo.WebApp;
                //if ((sourceDevice as ClassicStorage.AbstractXSystem)?.SupportedFileType == FileBlockType.SingleInstanceLevel_File
                //   && (destinationDevice as ClassicStorage.AbstractXSystem)?.SupportedFileType == FileBlockType.SingleInstanceLevel_File && !item.Name.Contains("meta"))
                //{
                //    Int64 contentFileNumber = this.GetContentFileNumber(item.Name);
                //    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceMoveAndDeleteFileFromDeviceInfo, retentionInfo.ToString(), contentFileNumber);
                //    info.MetaInfos["OriginalFileName"] = this.RetentionIndexService.GetItemName(contentFileNumber, retentionInfo.JobId);
                //}
                info.MetaInfos["Archive-SiteCollectionName"] = retentionInfo.SiteUrl;
                //info.MetaInfos["Archive-PlanId"] = retentionInfo.PlanId;
                info.MetaInfos["Archive-JobId"] = retentionInfo.JobId;
                Int64 dataMode = this.RetentionIndexService.GetJobDataMode(retentionInfo.JobId);
                info.MetaInfos["Archive-DataMode"] = Convert.ToString(dataMode);
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceMoveAndDeleteFileFromDeviceDataMode, dataMode);
                info.Length = sourceDevice.OpenFile(info).FileSize;//for cloud
                StorageResult storageResult = null;

                if (BLOBMappings.ContainsKey(info.HighPlusLowName))
                {
                    StorageInfo sourceInfo = BLOBMappings[info.HighPlusLowName].MappedBlobInfo;
                    storageResult = RealMove(sourceInfo, sourceDevice, info, destinationDevice);
                }
                else
                {
                    storageResult = RealMove(info, sourceDevice, destinationDevice);
                }
                if (destinationDevice.StorageType == XStorageType.Azure && destinationStoreInArchiverTier)
                {
                    SetFileTierArchive(destinationDevice, info);
                }
            });

            return this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, false,false);
        }

        private void SetFileTierArchive(IXSystem destinationDevice, StorageInfo storageInfo)
        {
            try
            {
                if (destinationDevice.StorageType == XStorageType.Azure)
                {
                    var device = destinationDevice as IAzureSystem;
                    AzureCloudInfo info = (AzureCloudInfo)storageInfo;
                    info.FileTierType = AccessTierType.Archive;
                    var result = device.ChangeFileTierAsync(info).GetAwaiter().GetResult();
                    if (!result.IsChanged)
                        logger.Warn("An error occurred while setting file Archive. FileName: {0}", storageInfo.LowName);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while setting file Archive. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
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

        private long GetContentFileNumber(String name)
        {
            return Convert.ToInt64(name.Substring(name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1, name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) - name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 1));
        }

        private StorageResult RealMove(StorageInfo info, IXSystem sourceDevice, IXSystem destinationDevice)
        {
            StorageResult storageResult = null;
            byte[] buffer = new byte[64 * 1024];
            try
            {

                using (var sourceStream = sourceDevice.OpenStream(info, FileMode.Open))
                {
                    //sourceStream.BeginRead(info);
                    using (var commitStream = destinationDevice.OpenStream(info, FileMode.CreateNew))
                    {
                        while (true)
                        {
                            int readLen = sourceStream.Read(buffer, 0, buffer.Length);
                            if (readLen <= 0) break;
                            commitStream.Write(buffer, 0, readLen);
                        }
                        storageResult = commitStream.Commit();
                        if (storageResult == null)
                        {
                            storageResult = new StorageResult();
                        }
                    }
                    //sourceStream.EndRead();
                }
            }
            catch (Exception ex)
            {
                storageResult = null;
                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRealMoveInvalidDevice, ex.ToString());
                throw;
            }
            return storageResult;
        }

        private StorageResult RealMove(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice)
        {
            StorageResult storageResult = null;
            byte[] buffer = new byte[64 * 1024];
            try
            {

                using (var sourceStream = sourceDevice.OpenStream(sourceInfo, FileMode.Open))
                {
                    //sourceStream.BeginRead(sourceInfo);
                    using (var commitStream = destinationDevice.OpenStream(destinationInfo, FileMode.CreateNew))
                    {
                        while (true)
                        {
                            int readLen = sourceStream.Read(buffer, 0, buffer.Length);
                            if (readLen <= 0) break;
                            commitStream.Write(buffer, 0, readLen);
                        }
                        storageResult = commitStream.Commit();
                        if (storageResult == null)
                        {
                            storageResult = new StorageResult();
                        }
                    }
                    //sourceStream.EndRead();
                }
            }
            catch (Exception ex)
            {
                storageResult = null;
                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRealMoveInvalidDevice, ex.ToString());
                throw;
            }
            return storageResult;
        }

        private ArchiverRetentionResult ConvertInfoToResult(ArchiverRetentionInfo info)
        {
            var result = new ArchiverRetentionResult();
            result.FarmName = info.FarmName;
            result.JobId = info.JobId;
            result.SiteUrl = info.SiteUrl;
            result.ArchiverBackupTime = info.ArchiverBackupTime;
            result.StoragePolicyId = info.StoragePolicyId;
            result.MediaService = info.MediaService;
            result.RetentionAction = info.RetentionAction;
            result.RetentionJob = info.RetentionJob;
            result.DestinationPhysicalDeviceId = info.DestinationPhysicalDeviceId;
            result.DataLogicalDevice = info.DataLogicalDevice;
            result.IndexLogicalDevice = info.IndexLogicalDevice;
            result.IsDeleteJob = info.IsDeleteJob;
            return result;
        }



        //private MediaArchiverRetentionInfo ConverToMediaArchiverRetentionInfo(ArchiverRetentionInfo retentionInfo)
        //{
        //    var info = new MediaArchiverRetentionInfo();
        //    info.FarmName = retentionInfo.FarmName;
        //    info.JobId = retentionInfo.JobId;
        //    info.ArchiverBackupTime = retentionInfo.ArchiverBackupTime;
        //    info.DataLogicalDevice = retentionInfo.DataLogicalDevice;
        //    info.DestinationDevice = retentionInfo.DestinationDevice;
        //    info.IndexLogicalDevice = retentionInfo.IndexLogicalDevice;
        //    info.DestinationPhysicalDeviceId = retentionInfo.DestinationPhysicalDeviceId;
        //    info.IsDeleteJob = retentionInfo.IsDeleteJob;
        //    info.RetentionAction = retentionInfo.RetentionAction;
        //    info.RetentionTimeSpanSeconds = retentionInfo.RetentionTimeSpanSeconds;
        //    return info;
        //}



        private static string GetWebServerServerRelativeUrl(string webUrl, IAveSite site)
        {
            if (webUrl.TrimEnd('/').Length == site.Url.TrimEnd('/').Length)
            {
                return string.Empty;
            }
            else
            {
                int hostLength = site.Url.Length - site.ServerRelativeUrl.Length;
                var result = webUrl.Substring(hostLength, webUrl.Length - hostLength);
                return result.Substring(result.IndexOf('/'));
            }
        }
        private void RemoveStubFromSharePoint(Dictionary<string, List<(string, string)>> docFullUrls, string tenantGroupId, string siteUrl, string jobId, string stubType)
        {
            try
            {
                if (docFullUrls != null && docFullUrls.Count > 0)
                {
                    RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    if (remoteSiteCollection == null)
                    {
                        logger.Info($"Cannot find {siteUrl} in the RemoteSiteCollection table. so skip remove stub.");
                        return;
                    }
                    if (stubType == "null")
                    {
                        logger.Info("this job is not create stub job");
                        return;
                    }

                    var defaultSuffix = EnsureStubType(stubType);
                    AveBPOSAccountInfo bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                    var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
                    using (IAveSite mSite = aveObjectModelFactory.CreateSite(siteUrl))
                    {
                        foreach (var webUrl in docFullUrls.Keys)
                        {
                            if (docFullUrls[webUrl].Count <= 0)
                            {
                                continue;
                            }
                            var webServerRelatedUrl = GetWebServerServerRelativeUrl(webUrl, mSite);
                            using (IAveWeb web = mSite.OpenWeb(webServerRelatedUrl))
                            {
                                foreach (var (docUrl, nodeGuid) in docFullUrls[webUrl])
                                {
                                    try
                                    {
                                        var possiblyStubSuffixes = GetPossiblyStubSuffixes(defaultSuffix);
                                        foreach (var stub in possiblyStubSuffixes)
                                        {
                                            var stubRelativeUrl = GetWebServerServerRelativeUrl(string.Format("{0}{1}", docUrl, stub), mSite);
                                            var stubFile = web.GetFile(stubRelativeUrl);
                                            bool isStubMatch = false;
                                            if (stubFile.Exists)
                                            {
                                                PossiblyStubSuffix = stub;
                                                try
                                                {
                                                    if (stubFile.Item != null)
                                                    {
                                                        var archiverLinkFileType = stubFile.Item.FieldValues["ArchiverLinkFileType"];
                                                        isStubMatch = archiverLinkFileType.ToString().StartsWith(jobId.Substring(0, jobId.LastIndexOf('_')));
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"stub file item is null ,url:{stubRelativeUrl}");
                                                        continue;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Warn("file not a stub,because it's fieldValues does not contain ArchiverLinkFileType,error:{0}", e.ToString());
                                                    continue;
                                                }
                                                if (isStubMatch)
                                                {
                                                    try
                                                    {
                                                        stubFile.Delete();
                                                        StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, mSite.ID.ToString(), nodeGuid);
                                                        GenerateFileLevelStubRetentionDetail(stub, docUrl, JobDetailsStatus.Successful);
                                                        logger.Info($"stub file has been deleted ,url:{stubRelativeUrl}");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Info($"delete file exception: {e.Message}. retry action.,stub url:{stubRelativeUrl}");
                                                        Record.UndeclareItemAsRecord(stubFile.Item);
                                                        stubFile.Delete();
                                                        StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, mSite.ID.ToString(), nodeGuid);
                                                        GenerateFileLevelStubRetentionDetail(stub, docUrl, JobDetailsStatus.Successful);
                                                        logger.Info($"delete file exception: {e.Message}. retry action success.,stub url:{stubRelativeUrl}");
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    logger.Info(string.Format("stub type: {0} does not exist in library.", System.IO.Path.GetExtension(stub)));
                                                }
                                            }
                                            else
                                            {
                                                logger.Info("current stub type:{0} not exsit.", stub);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"delete stubfile failed reson:{e.ToString()}");
                                        GenerateFileLevelStubRetentionDetail(defaultSuffix, docUrl, JobDetailsStatus.Failed, "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.Info($"the job:{jobId} has no stub need to delete.");
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Error in remove archive stub.reason : {0}.", e.ToString()));
            }
        }
        private void GenerateFileLevelStubRetentionDetail(string stubSuffix, string docUrl, JobDetailsStatus status, string comment = "")
        {
            var report = new JMArchiverRententionJobDetails();
            if (!string.IsNullOrEmpty(stubSuffix))
            {
                report.SiteUrl = docUrl + stubSuffix;
            }
            else
            {
                report.SiteUrl = docUrl;
            }
            report.Size = null;
            report.Status = status;
            report.JobId = archiverRetentionInfo.JobId;
            report.Comment = comment;
            report.Action = "RM_JS_Common_Delete";
            AddToReport(report);
        }
        private JMArchiverRententionJobDetails GenerateFileLevelRetentionDetail(string extraInfo, string url, long fileSize, JobDetailsStatus status,string action, string comment = "",string storageName = "")
        {
            var report = new JMArchiverRententionJobDetails();
            report.SiteUrl = GetFullPath(extraInfo, url);
            report.Size = fileSize.ToString();
            report.Status = status;
            report.JobId = archiverRetentionInfo.JobId;
            report.Comment = comment;
            report.Action = action;
            report.SrcStorageName = storageName;
            return report;
        }
        private string GetFullPath(string extraInfo, string url)
        {
            var document = new XmlDocument();
            document.LoadXml(extraInfo);
            var apUrlElements = document.GetElementsByTagName("HeaderExtraAttribute");
            if (apUrlElements != null && apUrlElements.Count > 0)
            {
                var apUrl = apUrlElements[0]?.Attributes["APUrl"]?.Value ?? url;
                return apUrl.Contains("\\") ? apUrl?.Replace("\\", "/") : apUrl;
            }
            return url;
        }
        private void AddRetentionToReport(ArchiverBasicIndex fileIndex, string fileName, long fileSize, JobDetailsStatus status,string action,string storageName = "")
        {
            try
            {
                var realReport = GenerateFileLevelRetentionDetail(fileIndex.ExtraInfo, fileIndex.Url, fileSize, status, action, "",storageName);
                this.AddToReport(realReport);
            }
            catch (Exception e)
            {
                logger.Error($"Add retention to report failed,itemname:{fileName}.error:{e}");
            }
        }

        private Dictionary<string, HashSet<string>> deletingDuplicatedFiles = new Dictionary<string, HashSet<string>>();
        private HashSet<string> RecordDeletingDuplicatedFiles(ArchiverBasicIndex item)
        {
            if (item.DuplicateStatus > 0)
            {
                if (!deletingDuplicatedFiles.TryGetValue(item.StorageCrc64, out var duplicatedFileIDs))
                {
                    duplicatedFileIDs = new HashSet<string>();
                    deletingDuplicatedFiles[item.StorageCrc64] = duplicatedFileIDs;
                }
                duplicatedFileIDs.Add(item.Id);
                return duplicatedFileIDs;
            }

            return new HashSet<string>();
        }

        private bool CheckIsLastDuplicatedFileWithSameCRC(ArchiverBasicIndex item, HashSet<string> deletingFileIDs)
        {
            var sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_8 = @CRC;";
            var dupFiles = this.IndexMainProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, new Dictionary<string, object>() { { "@CRC", item.StorageCrc64 } });
            var refsCount = dupFiles.Count(f => !deletingFileIDs.Contains(f.Id) && f.DuplicateStatus > 0);
            return refsCount == 0;
        }


        private void RealDeleteDeduplicateFileDataFromDevice(ArchiverBasicIndex item, ref long tempDeleteDataSize, ref bool isDeleteSucceedAtLeastOnce)
        {
            logger.Info($"This is dedup file data: {item.Id}. Source File: {item.DedupSourceFileId}");
            var deletingFileIDs = RecordDeletingDuplicatedFiles(item);
            string deleteAction = "RM_JS_Common_Delete";
            if (!CheckIsLastDuplicatedFileWithSameCRC(item, deletingFileIDs))
            {
                logger.Info($"Don't need remove source file data. Exists other duplicate file refs.");
                this.AddToReport(GenerateFileLevelRetentionDetail(item.ExtraInfo, item.Url, 0, JobDetailsStatus.Successful, deleteAction));
                isDeleteSucceedAtLeastOnce = true;
            }
            else
            {
                logger.Info($"Need remove source file data. Not exists duplicate file refs.");
                var subJobIdOfSouceFile = item.DedupSourceFileJobId;
                string highName = this.dataVolume;
                string lowName = subJobIdOfSouceFile + "_content_" + item.ContentDataFileNumber + ".dat";
                try
                {
                    var dedupDevice = GetDataLogicalDeviceByJobId(subJobIdOfSouceFile);
                    if (dedupDevice == null)
                    {
                        logger.Error($"Data logical device not found. {subJobIdOfSouceFile}.");
                        this.AddToReport(GenerateFileLevelRetentionDetail(item.ExtraInfo, item.Url, 0, JobDetailsStatus.Failed, deleteAction));
                        return;
                    }

                    var sourceFileInfo = XConvert.FromNames(highName, lowName);
                    StorageDeleteResult deleteDataResult = dedupDevice.DeleteFile(sourceFileInfo);
                    //ChangeLorealBlobFromPreviousVersionToDelete(sourceFileInfo);
                    if (deleteDataResult?.IsDeleted == true)
                    {
                        logger.Info($"Finished to delete source file content:{item.Id}.ContentDeviceName:{highName}\\{lowName}.");
                        var delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                        this.AddToReport(GenerateFileLevelRetentionDetail(item.ExtraInfo, item.Url, delSize, JobDetailsStatus.Successful, deleteAction));
                        isDeleteSucceedAtLeastOnce = true;
                        tempDeleteDataSize += delSize;
                    }
                    else
                    {
                        logger.Error($"Failed to delete source file content:{item.Id}.FilePath:{item.Url},message:{deleteDataResult?.Message}.");
                        this.AddToReport(GenerateFileLevelRetentionDetail(item.ExtraInfo, item.Url, 0, JobDetailsStatus.Failed, deleteAction));
                    }
                }
                catch (Exception ex)
                {
                    if (!isDeleteSucceedAtLeastOnce)
                    {
                        this.AddToReport(GenerateFileLevelRetentionDetail(item.ExtraInfo, item.Url, 0, JobDetailsStatus.Failed, deleteAction));
                        this.ErrorMessage = ex.Message;
                        this.jobStatusInfo.State = 3;
                        this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, lowName, ex.ToString());
                    }
                    else
                    {
                        this.jobStatusInfo.State = 7;
                        this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, lowName, ex.ToString());
                    }
                }
            }
        }

        private Int64 DeleteDataFromDevice(String dataVolume, String indexVolume, Boolean NeedDeleteSubIndex,bool isFitSoftDeleteAndRetainByModifedTime = false,bool needToAddJobDetail = true)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            String stubType = string.Empty;
            var tempDeleteDataSize = default(Int64);
            StorageDeleteResult deleteDataResult = new StorageDeleteResult();
            StorageDeleteResult deleteIndexResult = new StorageDeleteResult();
            // *** 对于StorageInterfaceType.Object 类型的Device不支持Dedup，如果支持还需要进一步完善 ***
            if (this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object))
            {
                logger.Warn("this device storage InterfaceType is Object,it may wrong");
                var storageInfoList = this.RetentionIndexService.GetStorageInfosByJobId(this.archiverRetentionInfo.JobId);
                tempDeleteDataSize = this.DeleteFileByStorageInfo(storageInfoList);
                if (this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                {
                    if (this.archiverRetentionInfo.RemoveOrphanedStub)
                    {
                        string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        var stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType);
                        RemoveStubFromSharePoint(stubUrlList, this.archiverRetentionInfo.TenantGroupId, siteUrl, this.archiverRetentionInfo.JobId, stubType);
                    }
                    var retentionInfoList = this.RetentionIndexService.GetDeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.SiteUrl);
                    this.RetentionIndexService.DeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    UpdateArchivedInfo(this.archiverRetentionInfo.SiteUrl);
                    UpdateRetentionInfo(retentionInfoList);
                }
            }
            else
            {
                if (this.archiverRetentionInfo.IsFileLevelBlockBackup)
                {
                    List<ArchiverBasicIndex> deletingIndexes = null;
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        NeedDeleteSubIndex = false;
                        deletingIndexes = this.RetentionIndexService.GetDeletingIndexesByModifiedTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, SoftTimeConvert(this.archiverRetentionInfo.DateTimeNow, this.archiverRetentionInfo.SoftDeleteKeepValue, this.archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                    }
                    else if(this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ArchiveTime) 
                    {
                        deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        DeleteMetaBlocks(this.archiverRetentionInfo.JobId, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                    }
                    else
                    {
                        throw new Exception($"Unsupported retain data type: {this.archiverRetentionInfo.RetentionDataTimeType}");
                    }
                    long contentSize = 0;
                    if (deletingIndexes != null && deletingIndexes.Count > 0)
                    {
                        HashSet<string> needDeletedFileContentName = new HashSet<string>();

                        foreach (var deletingIdx in deletingIndexes)
                        {
                            if (deletingIdx.DuplicateStatus > 0)
                            {
                                RealDeleteDeduplicateFileDataFromDevice(deletingIdx, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                                continue;
                            }

                            var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                            logger.Info($"Start to delete device content: {info.HighPlusLowName}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                            try
                            {
                                deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                                ChangeLorealBlobFromPreviousVersionToDelete(info);
                                var delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                tempDeleteDataSize += delSize;
                                if (needToAddJobDetail)
                                {
                                    if (delSize == 0)
                                    {
                                        delSize = deletingIdx.ContentLength;
                                    }
                                    AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                                isDeleteSucceedAtLeastOnce = true;
                                contentSize+= delSize;
                            }
                            catch (Exception ex)
                            {
                                if (!isDeleteSucceedAtLeastOnce)
                                {
                                    if (needToAddJobDetail)
                                    {
                                        AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                    }
                                    this.ErrorMessage = ex.Message;
                                    this.jobStatusInfo.State = 3;
                                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                    throw;
                                }
                                else
                                {
                                    this.jobStatusInfo.State = 7;
                                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                                }
                                logger.Info($"Update media size success,job id:{this.archiverRetentionInfo.JobId},size:{tempDeleteDataSize}");
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"No file need to delete, job id:{this.archiverRetentionInfo.JobId}");
                    }

                    if (tempDeleteDataSize > 0)
                    {
                        ArchiverIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeAsync(this.archiverRetentionInfo.JobId, tempDeleteDataSize);
                    }
                    else
                    {
                        tempDeleteDataSize = contentSize;
                    }
                }
                else
                {
                    var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                    var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
                    logger.Info($"Need delete blobs count : {fileList.Count}");
                    Int32 deleteTime = this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData ? 1 : fileList.Count + 1;
                    Int32 totalRetentionTimes = this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData ? fileList.Count : fileList.Count * 2;
                    fileList.ForEach(item =>
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        //info.Length = this.dataLogicalDevice.OpenFile(info).FileSize;//for cloud
                        try
                        {
                            deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                            ChangeLorealBlobFromPreviousVersionToDelete(info);
                            isDeleteSucceedAtLeastOnce = true;
                            tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                        }
                        catch (Exception ex)
                        {
                            if (!isDeleteSucceedAtLeastOnce)
                            {
                                this.ErrorMessage = ex.Message;
                                this.jobStatusInfo.State = 3;
                                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                throw;
                            }
                            else
                            {
                                this.jobStatusInfo.State = 7;
                                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            }
                        }
                    });
                }

                if (this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                {
                    if (this.archiverRetentionInfo.RemoveOrphanedStub)
                    {
                        string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        Dictionary<string, List<(string, string)>> stubUrlList = [];
                        if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                        {
                            stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndexByModifiedTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType, SoftTimeConvert(this.archiverRetentionInfo.DateTimeNow, this.archiverRetentionInfo.SoftDeleteKeepValue, this.archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                        }
                        else
                        {
                            stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType);
                        }
                        
                        RemoveStubFromSharePoint(stubUrlList, this.archiverRetentionInfo.TenantGroupId, siteUrl, this.archiverRetentionInfo.JobId, stubType);
                    }
                    logger.Info($"Current job id is {this.archiverRetentionInfo.RetentionJob.Id}");
                    var retentionInfoList = this.RetentionIndexService.GetDeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.SiteUrl);
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        this.RetentionIndexService.DeleteDataFromMainIndexByDateTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, SoftTimeConvert(this.archiverRetentionInfo.DateTimeNow, this.archiverRetentionInfo.SoftDeleteKeepValue, this.archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                    }
                    else
                    {
                        this.RetentionIndexService.DeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    }

                    UpdateArchivedInfo(this.archiverRetentionInfo.SiteUrl);
                    UpdateRetentionInfo(retentionInfoList);
                }
            }

            if (NeedDeleteSubIndex)
            {
                StorageInfo storageInfo = XConvert.FromNames(
                    indexVolume, this.archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName);
                storageInfo.ExtraStorageInfo = archiverRetentionInfo.SubIndexStorageInfo;
                try
                {
                    deleteIndexResult = this.indexLogicalDevice.DeleteFile(storageInfo);

                    if (deleteIndexResult.DeletedFileSize > 0)
                    {
                        tempDeleteDataSize += deleteIndexResult.DeletedFileSize;
                    }
                }
                catch (Exception ex)
                {
                    this.jobStatusInfo.State = 7;
                    this.logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, storageInfo.LowName, ex.ToString());
                }
            }

            return tempDeleteDataSize;
        }
        private long SoftTimeConvert(long retentionTimeTicks, int keepValue, DateUnit dateUnit,bool isSoftDelete)
        {
            if (isSoftDelete)
            {
                DateTime retentionTime = DateTime.UtcNow;
                switch (dateUnit)
                {
                    case DateUnit.Year:
                        retentionTime = retentionTime.AddYears(-keepValue);
                        break;
                    case DateUnit.Month:
                        retentionTime = retentionTime.AddMonths(-keepValue);
                        break;
                    case DateUnit.Week:
                        retentionTime = retentionTime.AddDays(-keepValue * 7);
                        break;
                    case DateUnit.Day:
                        retentionTime = retentionTime.AddDays(-keepValue);
                        break;
                }
                logger.Info($"ValidatesoftTime.RetentionTime2 {retentionTime.Ticks}");
                return retentionTime.Ticks;
            }
            else
            {
                return retentionTimeTicks;
            }
        }
        private Int64 SoftDelete(String dataVolume, String indexVolume)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            bool isRealDeleteDatas = IsEnabledRealDelete();
            String stubType = string.Empty;
            var tempDeleteDataSize = default(Int64);
            StorageDeleteResult deleteDataResult = new StorageDeleteResult();
            StorageDeleteResult deleteIndexResult = new StorageDeleteResult();

                if (this.archiverRetentionInfo.IsFileLevelBlockBackup)
                {
                    List<ArchiverBasicIndex> deletingIndexes = null;
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        deletingIndexes = this.RetentionIndexService.GetDeletingIndexesByModifiedTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.DateTimeNow,false);
                    }
                    else if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ArchiveTime)
                    {
                        deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        if (isRealDeleteDatas)
                        {
                            logger.Info("this job is soft and is real delete,delete meta blocks");
                            DeleteMetaBlocks(this.archiverRetentionInfo.JobId, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                        }
                    }
                    else
                    {
                        throw new Exception($"Unsupported retain data type: {this.archiverRetentionInfo.RetentionDataTimeType}");
                    }

                    if (deletingIndexes != null && deletingIndexes.Count > 0)
                    {
                        HashSet<string> needDeletedFileContentName = new HashSet<string>();
                        foreach (var deletingIdx in deletingIndexes)
                        {
                            if (deletingIdx.DuplicateStatus > 0 && isRealDeleteDatas)
                            {
                                RealDeleteDeduplicateFileDataFromDevice(deletingIdx, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                                continue;
                            }

                            var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                            logger.Info($"Start to delete device content: {info.HighPlusLowName}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                            try
                            {
                            long delSize = 0;
                            if (isRealDeleteDatas)
                            {
                                deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                                ChangeLorealBlobFromPreviousVersionToDelete(info);
                                if (deletingIdx.RetentionStatus == (int)GCommon.Contract.CommonFilter.FilterDeletedType.Soft && deleteDataResult.IsDeleted && deleteDataResult.DeletedFileSize < 0)
                                {
                                    logger.Info($"this data has soft deleted,no need to add report again,name:{info.LowName}");
                                }
                                else
                                {
                                    delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                    if (delSize == 0)
                                    {
                                        delSize = deletingIdx.ContentLength;
                                    }
                                    AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_SoftDelete", archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                                isDeleteSucceedAtLeastOnce = true;

                            }
                            else
                            {
                                if (deletingIdx.RetentionStatus == (int)GCommon.Contract.CommonFilter.FilterDeletedType.Soft)
                                {
                                    logger.Info($"1this data has soft deleted,no need to add report again,name:{info.LowName}");
                                }
                                else
                                {
                                    var result = this.dataLogicalDevice.OpenFile(info);
                                    AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Successful,"RM_AR_CP_GSS_Retention_SoftDelete", archiverRetentionInfo.DataLogicalDevice.Name);
                                    isDeleteSucceedAtLeastOnce = true;
                                }
                            }

                        }
                            catch (Exception ex)
                            {
                                if (!isDeleteSucceedAtLeastOnce)
                                {
                                    AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed,"RM_AR_CP_GSS_Retention_SoftDelete");
                                    this.ErrorMessage = ex.Message;
                                    this.jobStatusInfo.State = 3;
                                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                    throw;
                                }
                                else
                                {
                                    this.jobStatusInfo.State = 7;
                                    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                                }
                                logger.Info($"Update media size success,job id:{this.archiverRetentionInfo.JobId},size:{tempDeleteDataSize}");
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"No file need to delete, job id:{this.archiverRetentionInfo.JobId}");
                    }
                }
                else
                {
                    var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                    var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
                    logger.Info($"Need delete blobs count : {fileList.Count}");
                    Int32 deleteTime = this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData ? 1 : fileList.Count + 1;
                    Int32 totalRetentionTimes = this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData ? fileList.Count : fileList.Count * 2;
                if (isRealDeleteDatas)
                {
                    fileList.ForEach(item =>
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        //info.Length = this.dataLogicalDevice.OpenFile(info).FileSize;//for cloud
                        try
                        {
                            deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                            ChangeLorealBlobFromPreviousVersionToDelete(info);
                            tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                            isDeleteSucceedAtLeastOnce = true;
                        }
                        catch (Exception ex)
                        {
                            if (!isDeleteSucceedAtLeastOnce)
                            {
                                this.ErrorMessage = ex.Message;
                                this.jobStatusInfo.State = 3;
                                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                throw;
                            }
                            else
                            {
                                this.jobStatusInfo.State = 7;
                                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            }
                        }
                    });
                }
                else
                {
                    logger.Info("not real delete datas,will just mark as soft delete");
                }
                }

                if (this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                {
                    if (this.archiverRetentionInfo.RemoveOrphanedStub)
                    {
                        string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        Dictionary<string, List<(string, string)>> stubUrlList = [];
                        if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                        {
                            stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndexByModifiedTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType, this.archiverRetentionInfo.DateTimeNow,false);
                        }
                        else
                        {
                            stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType);
                        }

                        RemoveStubFromSharePoint(stubUrlList, this.archiverRetentionInfo.TenantGroupId, siteUrl, this.archiverRetentionInfo.JobId, stubType);
                    }
                    logger.Info($"Current job id is {this.archiverRetentionInfo.RetentionJob.Id}");
                    var retentionInfoList = this.RetentionIndexService.GetDeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.SiteUrl);
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        this.RetentionIndexService.UpdateAsSoftDeleteByDateTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.DateTimeNow);
                    }
                    else
                    {
                        this.RetentionIndexService.UpdateAsSoftDelete(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    }

                    UpdateArchivedInfo(this.archiverRetentionInfo.SiteUrl);
                    UpdateRetentionInfo(retentionInfoList);
                }
                try
                {
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        var parameters = new Dictionary<String, Object>();
                        parameters["@storagePolicyId"] = this.archiverRetentionInfo.StoragePolicyId;
                        parameters["@jobId"] = this.archiverRetentionInfo.JobId;
                        parameters["@dateTime"] = this.archiverRetentionInfo.DateTimeNow;
                        parameters["@timeNow"] = DateTime.UtcNow.Ticks.ToString();
                        var deleteBodyTable = "update " + IndexConstants.TableNameArchiveBody + " set COL_RETENTION_STATUS = 1,COL_META_TAIL_LENGTH = @timeNow where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_MODIFY_TIME<@dateTime and COL_RETENTION_STATUS = 0";
                        this.IndexSubProcessor.Execute(deleteBodyTable, parameters);
                    }
                    else
                    {
                        var parameters = new Dictionary<String, Object>();
                        parameters["@storagePolicyId"] = this.archiverRetentionInfo.StoragePolicyId;
                        parameters["@jobId"] = this.archiverRetentionInfo.JobId;
                        var deleteBodyTable = "update " + IndexConstants.TableNameArchiveBody + " set COL_RETENTION_STATUS = 1 where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
                        this.IndexSubProcessor.Execute(deleteBodyTable, parameters);
                    }
                }
                catch (Exception ex)
                {
                    this.jobStatusInfo.State = 7;
                    this.logger.Warn($"soft delete failed when mark sub index,error:{ex}");
                }

            return tempDeleteDataSize;
        }

        private void DeleteMetaBlocks(string jobId, ref long tempDeleteDataSize, ref bool isDeleteSucceedAtLeastOnce)
        {
            try
            {
                var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                string metaFilePrefix = $"{jobId}_meta_";
                var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(metaFilePrefix, StringComparison.OrdinalIgnoreCase));
                logger.Info($"Need delete meta blocks count : {fileList.Count}");
                StorageDeleteResult deleteDataResult;
                foreach(var item in fileList)
                {
                    var info = XConvert.FromNames(item.HighName, item.Name);
                    try
                    {
                        deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                        ChangeLorealBlobFromPreviousVersionToDelete(info);
                        isDeleteSucceedAtLeastOnce = true;
                        tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                    }
                    catch (Exception ex)
                    {
                        if (!isDeleteSucceedAtLeastOnce)
                        {
                            this.ErrorMessage = ex.Message;
                            this.jobStatusInfo.State = 3;
                            this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                            throw;
                        }
                        else
                        {
                            this.jobStatusInfo.State = 7;
                            this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.jobStatusInfo.State = 7;
                this.logger.Error($"Error occurred while deleting meta blocks: {jobId}. {ex}");
            }
        }

        private void UpdateRetentionInfo(List<KeyValuePair<string, long>> retentionInfoList)
        {
            foreach (var info in retentionInfoList)
            {
                logger.Info($"Retention file info:{info.Value}, site URL: {this.archiverRetentionInfo.SiteUrl}, list URL:{info.Key}, archiver job: {this.archiverRetentionInfo.JobId}");
                var retentionSiteInfo = new RMRetentionSiteInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    ListUrl = info.Key,
                    SiteUrl = this.archiverRetentionInfo.SiteUrl,
                    RetentionJobID = this.archiverRetentionInfo.RetentionJob.Id,
                    FileNumber = info.Value
                };
                ArchiveSiteInfoDao.SaveRetentionSiteInfo(retentionSiteInfo);
            }
        }

        private void UpdateArchivedInfo(string siteUrl)
        {
            var syncArchivedSiteInfo = RMKeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
            if (syncArchivedSiteInfo != null)
            {
                bool result;
                if (bool.TryParse(syncArchivedSiteInfo.Value, out result) && result)
                {
                    long fileCount = this.RetentionIndexService.GetFileNumber();
                    long fileVersionCount = this.RetentionIndexService.GetFileVersionNumber();
                    var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl)?.TenantId;
                    this.logger.Info($"file count is:{fileCount},version count is:{fileVersionCount}");
                    ArchiveSiteInfoDao.UpdateArchiverInfo(siteUrl, fileCount, fileVersionCount, o365TenantId);
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
        private bool IsEnabledRealDelete()
        {
            var realDeleteRetentionDatas = RMKeyValueDao.GetValueByKey("RealDeleteAzureRetentionDatas");
            if (realDeleteRetentionDatas != null)
            {
                bool result;
                if (bool.TryParse(realDeleteRetentionDatas.Value, out result) && result)
                {
                    string storageId = string.IsNullOrEmpty(archiverRetentionInfo.CurrentStorageId) ? archiverRetentionInfo.StoragePolicyId : archiverRetentionInfo.CurrentStorageId;
                    if (string.Equals(storageId, RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Warn("this storage is avepoint storage that can not delete datas when the action is soft delete");
                        return false;
                    }
                    else
                    {
                        var storageInfo = StorageDeviceDao.GetStorageDevicesById(new Guid(storageId));
                        if (storageInfo != null && storageInfo.Type == (int)StorageDeviceType.CloudAzure)
                        {
                            logger.Info($"this storage is azure storage and soft delete,will real delete datas");
                            return true;
                        }
                        else
                        {
                            logger.Info($"this storage is not azure storage,so skip delete datas when soft delete,storage id:{storageId},type:{storageInfo?.Type}");
                            return false;
                        }
                    }
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
        private void ChangeLorealBlobFromPreviousVersionToDelete(StorageInfo info)
        {
            if (IsLorealSoftDelete)
            {
                var source = dataLogicalDevice as AbstractXSystem;
                if (source != null && source.StorageType == XStorageType.Azure)
                {
                    if (sourceContainerClient == null)
                    {
                        sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                    }
                    string blobName = info.HighPlusLowName.Replace(@"\", @"/");
                    logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {info.HighPlusLowName}.blobName:{blobName}.");
                    var blobClient = sourceContainerClient.GetBlobClient(blobName);
                    // List all versions of the blob
                    List<string> blobVersions = new List<string>();
                    foreach (BlobItem blobItem in sourceContainerClient.GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blobName, default))
                    {
                        logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {blobItem.Name}, Version ID: {blobItem.VersionId}.Version Delete:{blobItem.Deleted}.");
                        blobVersions.Add(blobItem.VersionId);
                    }
                    foreach (var blobVersion in blobVersions)
                    {
                        blobClient.WithVersion(blobVersion).DeleteIfExistsAsync();
                        logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Success delete blob version.Version ID: {blobVersion}.");
                    }
                }
                else
                {
                    throw new FileNotFoundException(String.Format("3An error occurred in getting file {0} size in {1}.", info.HighPlusLowName, dataVolume));
                }
            }
        }

        private void OpenSubIndex(ArchiverRetentionInfo archiverRetentionInfo, String indexVolume)
        {
            this.logger.Info("Begin opening mainindex.");
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName,
                BackupJobId = archiverRetentionInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRetentionInfo.CacheSetting,
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo
            };
            this.InitIndexProcessor(indexServiceOpenParameter);
        }
        private void OpenMainIndex(ArchiverRetentionInfo archiverRetentionInfo, String indexVolume)
        {
            this.logger.Info("Begin opening mainindex.");
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                BackupJobId = archiverRetentionInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRetentionInfo.CacheSetting,
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo
            };
            IndexSynchronizer.Initialize(indexServiceOpenParameter);
            this.InitIndexProcessor(indexServiceOpenParameter);
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
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
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
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
            else
            {
                if (this.IndexSubProcessor == null)
                {
                    this.IndexSubProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
                }
                param.IsNeedCheckIntegrity = true;
                this.IndexSubProcessor.Open(param);
            }
            this.logger.Info("Open MainIndex Finished.");
        }

        

        private void UploadIndexToRealSystem()
        {
            if (this.IndexMainProcessor != null)
            {
                this.IndexMainProcessor.Close();
            }
            if (this.IndexSubProcessor != null)
            {
                this.IndexSubProcessor.Close();
                var subdbInfo = new IndexDatabaseInfo(archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName, null);
                this.IndexSynchronizer.Upload(subdbInfo);
            }
            var storageInfo = XConvert.FromNames(archiverRetentionInfo.IndexVolume, ServiceConstants.IndexDBName, archiverRetentionInfo.MainIndexStorageInfo);
            var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
            this.IndexSynchronizer.Upload(dbInfo);
        }
    }
}