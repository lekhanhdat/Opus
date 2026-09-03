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
    using AvePoint.Application.StorageApiModern;
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
    using AvePoint.Media.Service.ArchiverBackup.Exceptions;
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
    using AvePoint.RA.DB.Dao.Extension;
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
    using DocumentFormat.OpenXml.Spreadsheet;
    using global::Media.Common;
    using global::Media.Common.ClassicStorageApi;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Storage;
    using Storage.Cloud.Azure;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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

    public class ArchiverBackupRetentionService
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
        Int64 deleteDataNumber = default(Int64);
        String dataVolume;
        String indexVolume;
        AccessTierType accessTierType;
        Boolean isObjectType;
        String ErrorMessage = ServiceConstants.ArchvierRetentionFailedMessage;
        string I18NEntity_RM_PRM_PRE_Move = I18NEntity.GetString("RM_PRM_PRE_Move");
        string I18NEntity_RM_JS_Common_Copy = I18NEntity.GetString("RM_JS_Common_Copy");
        public SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings = new SafeDictionary<string, BLOBRehydrationMapping>();
        private String rehydrationTemp;
        private readonly Object rehydrationLock = new Object();
        private Boolean destinationStoreInArchiverTier;
        //List<MediaArchiverRetentionInfo> retentionInfo = new List<MediaArchiverRetentionInfo>();
        private static readonly IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IRMRetentionSimulateInfosDao RetentionInfosDao = PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
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
            this.dataLogicalDevice = XFactoryCommon.InstanceSystem(this.archiverRetentionInfo.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data)[0]);
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

            this.IsEnableExtendedMoveActionForRetention = RMKeyValueDao.IsEnableExtendedMoveActionForRetention();
            logger.Info($"Source storage type: {dataLogicalDevice?.StorageType}," +
                $" destination storage type: {destinationLogicalDevice?.StorageType}," +
                $" enable extended move action for retention job: {this.IsEnableExtendedMoveActionForRetention}");
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
            if (!archiverRetentionInfo.IsSimulateJob)
            {
                this.UploadIndexToRealSystem();
            }
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
            var startTime = DateTime.UtcNow;

            // Create CancellationTokenSource for async operations
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    // Validate preconditions before proceeding
                    ValidatePreConditions();
                    
                    this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataBegin, this.archiverRetentionInfo.JobId);
                    
                    // Execute the core data movement logic asynchronously
                    this.deleteDataSize = this.MoveDataFromDeviceAsync(this.dataVolume, this.indexVolume, cts.Token).GetAwaiter().GetResult();
                    
                    var elapsed = DateTime.UtcNow - startTime;
                    this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataFinished, 
                                     this.archiverRetentionInfo.JobId, 
                                     this.deleteDataSize.ToString(),
                                     elapsed.TotalSeconds);
                    
                    // Construct result object
                    var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
                    result.Size = deleteDataSize;
                    result.State = this.jobStatusInfo.State;
                    return result;
                }
                catch (TimeoutException ex)
                {
                    // Blob rehydration timeout (up to 5 days)
                    this.logger.Error("Blob rehydration timeout for job {0}: {1}", 
                                     this.archiverRetentionInfo?.JobId ?? "unknown", 
                                     ex.Message);
                    if (this.jobStatusInfo != null)
                    {
                        this.jobStatusInfo.State = 3; // Failed state
                    }
                    this.ErrorMessage = "Blob rehydration timed out after maximum wait period";
                    
                    var retentionEx = new RetentionException(this.ErrorMessage, ex)
                    {
                        JobId = this.archiverRetentionInfo?.JobId,
                        RetentionRule = "MoveArchiverJobData"
                    };
                    throw retentionEx;
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Insufficient permissions to access storage
                    this.logger.Error("Unauthorized access for job {0}: {1}", 
                                     this.archiverRetentionInfo?.JobId ?? "unknown", 
                                     ex.Message);
                    if (this.jobStatusInfo != null)
                    {
                        this.jobStatusInfo.State = 3;
                    }
                    this.ErrorMessage = "Insufficient permissions to access storage devices";
                    
                    var retentionEx = new RetentionException(this.ErrorMessage, ex)
                    {
                        JobId = this.archiverRetentionInfo?.JobId,
                        RetentionRule = "MoveArchiverJobData"
                    };
                    throw retentionEx;
                }
                catch (IOException ex)
                {
                    // Network or I/O error
                    this.logger.Error("I/O error for job {0}: {1}", 
                                     this.archiverRetentionInfo?.JobId ?? "unknown", 
                                     ex.ToString());
                    if (this.jobStatusInfo != null)
                    {
                        this.jobStatusInfo.State = 3;
                    }
                    this.ErrorMessage = "Network or I/O error occurred during data move";
                    
                    var retentionEx = new RetentionException(this.ErrorMessage, ex)
                    {
                        JobId = this.archiverRetentionInfo?.JobId,
                        RetentionRule = "MoveArchiverJobData"
                    };
                    throw retentionEx;
                }
                catch (InvalidOperationException ex)
                {
                    // Precondition validation failed
                    this.logger.Error("Invalid operation for job {0}: {1}", 
                                     this.archiverRetentionInfo?.JobId ?? "unknown", 
                                     ex.Message);
                    if (this.jobStatusInfo != null)
                    {
                        this.jobStatusInfo.State = 3;
                    }
                    this.ErrorMessage = ex.Message;
                    throw; // Re-throw as-is since it's already descriptive
                }
                catch (JobStopException)
                {
                    cts.CancelAsync().ExecuteAsyncTask();
                    throw;
                }
                catch (Exception ex)
                {
                    // Catch-all for unexpected errors
                    this.logger.Error("Unexpected error moving job data for job {0}: {1}", 
                                     this.archiverRetentionInfo?.JobId ?? "unknown", 
                                     ex.ToString());
                    if (this.jobStatusInfo != null)
                    {
                        this.jobStatusInfo.State = 3;
                    }
                    this.ErrorMessage = "An unexpected error occurred during data move operation";
                    
                    var retentionEx = new RetentionException(this.ErrorMessage, ex)
                    {
                        JobId = this.archiverRetentionInfo?.JobId,
                        RetentionRule = "MoveArchiverJobData"
                    };
                    throw retentionEx;
                }
            }
        }

        private ArchiverRetentionResult SimulateRetainJobData()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataBegin, this.archiverRetentionInfo.JobId);
            logger.Info($"retention type and soft delete info is:retentionType:{this.archiverRetentionInfo.RetentionDataTimeType},isFitSoftDelete:{this.archiverRetentionInfo.IsFitSoftDelete},isSoftDelete:{this.archiverRetentionInfo.IsSoftDelete}");
            if (this.archiverRetentionInfo.IsSoftDelete)
            {
                logger.Info("SimulateRetainJobData.Skip check soft delete data.");
                //this.deleteDataSize = this.SimulateDeleteDataFromDevice(this.dataVolume, this.indexVolume, true, true);
                //this.SoftDelete(this.dataVolume, this.indexVolume);
                return null;
            }
            else if (!this.archiverRetentionInfo.IsFitSoftDelete && this.archiverRetentionInfo.IsSoftDelete)
            {
                //logger.Info("this action is soft delete");
                //this.deleteDataSize = this.SoftDelete(this.dataVolume, this.indexVolume);
                return null;
            }
            else
            {
                logger.Info("this action is real delete");
                var simulateResult = this.SimulateDeleteDataFromDevice(this.dataVolume, this.indexVolume, true);
                this.deleteDataSize = simulateResult.Item1;
                this.deleteDataNumber = simulateResult.Item2;
                RetentionInfosDao.AccumulateUpdateRetentionInfo(archiverRetentionInfo.SourceFlag, deleteDataNumber, deleteDataSize);
            }
            //this.deleteDataSize += this.DeleteIndexFromDevice();
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataDeleteDataFinished, this.archiverRetentionInfo.JobId, this.deleteDataSize.ToString());
            var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            result.Size = deleteDataSize;
            result.State = 2;
            result.HasIndexRelatedToBackupJob = IsExistsIndexRelatedToJob(this.archiverRetentionInfo.JobId);
            return result;
        }

        private ArchiverRetentionResult RetainJobData()
        {
            if (archiverRetentionInfo.IsSimulateJob)
            {
                return SimulateRetainJobData();
            }
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataBegin, this.archiverRetentionInfo.JobId);
            logger.Info($"retention type and soft delete info is:retentionType:{this.archiverRetentionInfo.RetentionDataTimeType},isFitSoftDelete:{this.archiverRetentionInfo.IsFitSoftDelete},isSoftDelete:{this.archiverRetentionInfo.IsSoftDelete}");
            if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime && this.archiverRetentionInfo.IsSoftDelete && !this.archiverRetentionInfo.IsSystemStorage)
            {
                logger.Info("this action is retain by modified time delete");
                this.deleteDataSize = this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, true, true);
                this.SoftDelete(this.dataVolume, this.indexVolume);
            }
            else if (!this.archiverRetentionInfo.IsFitSoftDelete && this.archiverRetentionInfo.IsSoftDelete && !this.archiverRetentionInfo.IsSystemStorage)
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
            var Indexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
            if (Indexes != null && Indexes.Count > 0)
            {
                foreach (var Idx in Indexes)
                {
                    AddMarkTierRetentionToReport(Idx, "", 0, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_MarkDataTier", archiverRetentionInfo.DataLogicalDevice.Name);
                }
            }
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
            var parameters = new Dictionary<String, Object>();
            parameters["@JobId"] = $"%{jobId}%";
            var deleteBodyTable = $"SELECT COL_ID FROM {IndexConstants.TableNameArchiveBody} WHERE COL_POOL_GUID LIKE @JobId LIMIT 1;";
            var result = this.IndexMainProcessor.ExecuteScalar(deleteBodyTable, parameters);
            return result != null;
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
                    if (archiverRetentionInfo.IsSimulateJob)
                    {
                        result = new StorageDeleteResult()
                        {
                            DeletedFileSize = this.dataLogicalDevice.OpenFile(info).FileSize
                        };
                    }
                    else
                    {
                        result = this.dataLogicalDevice.DeleteFile(info);
                        ChangeLorealBlobFromPreviousVersionToDelete(info);
                    }
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

        private async Task<Int64> MoveDataFromDeviceAsync(string dataVolume, string indexVolume, CancellationToken cancellationToken)
        {
            var tempDeleteDataSize = default(Int64);
            //var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
            //var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
            //logger.Info($"Need move blobs count : {fileList.Count}");
            var fileList = this.ListFilesFromNameAsync(XConvert.FromNames(dataVolume, null), cancellationToken);
            tempDeleteDataSize = await this.MoveAndDeleteFileFromDeviceAsync(this.dataLogicalDevice, this.destinationLogicalDevice, fileList, cancellationToken);
            return tempDeleteDataSize;
        }

        private async IAsyncEnumerable<XFileInfo> ListFilesFromNameAsync(StorageInfo dirInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (this.dataLogicalDevice is IXCloudSystem cloudSystem)
            {
                await foreach (var file in cloudSystem.ListAllFilesAsync(dirInfo, cancellationToken))
                {
                    if (file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return file;
                    }
                }
            }
            else
            {
                // Currently, only IXCloudSystem supports async listing
                // For other storage types, we will fall back to synchronous listing, which may cause performance issues if there are a large number of files
                // We should consider implementing async listing for other storage types in the future, when the storage-api supports it
                logger.Warn("Data logical device does not support async listing. Falling back to synchronous listing, which may cause performance issues for large datasets.");
                foreach (var file in this.dataLogicalDevice.ListFiles(dirInfo))
                {
                    if (file.LowName.StartsWith(this.archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return file;
                    }
                }
            }
        }

        private void VerifyAndCopyArchiverToHot(XFileInfo info)
        {
            //var file = this.dataLogicalDevice.OpenFile(info);

            if (info is AzureCloudInfo azureFile)
            {
                if (azureFile != null && azureFile.FileTierType == AccessTierType.Archive)
                {
                    string temp = Path.Combine(rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + 11));
                    lock (rehydrationLock)
                    {
                        if (!BLOBMappings.ContainsKey(info.HighPlusLowName))
                        {
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

        /// <summary>
        /// Randomly throws an exception if random exception testing is enabled.
        /// Used for testing error handling and recovery scenarios.
        /// </summary>
        /// <param name="operationName">Name of the operation being tested</param>
        /// <param name="probability">Probability of throwing exception (0.0 to 1.0, default 0.3 = 30%)</param>
        private void ThrowRandomExceptionIfEnabled(string operationName, double probability = 0.3)
        {
            if (!RMKeyValueDao.IsEnableRandomExceptionForTestingAsync().ExecuteAsyncTask())
            {
                return;
            }

            var random = new Random();
            if (random.NextDouble() < probability)
            {
                var exceptionTypes = new Action[]
                {
                    () => throw new IOException($"Simulated IO exception during {operationName}"),
                    () => throw new UnauthorizedAccessException($"Simulated access denied during {operationName}"),
                    () => throw new TimeoutException($"Simulated timeout during {operationName}"),
                    () => throw new InvalidOperationException($"Simulated invalid operation during {operationName}")
                };

                var exceptionIndex = random.Next(exceptionTypes.Length);
                logger.Warn($"[TESTING] Throwing random exception for operation: {operationName}");
                exceptionTypes[exceptionIndex]();
            }
        }

        private async Task<long> MoveAndDeleteFileFromDeviceAsync(IXSystem sourceDevice, IXSystem destinationDevice, IAsyncEnumerable<XFileInfo> fileSource, CancellationToken cancellationToken)
        {
            // Read retention policy configuration for source deletion
            bool enableDelete = !RMKeyValueDao.IsEnableCopyToAnotherLocation();

            // Log configuration for troubleshooting
            logger.Info($"Retention move policy: enableDelete={enableDelete}, jobId={this.archiverRetentionInfo?.JobId ?? "unknown"}");

            #region--- Incremental Move - Query Failed Files

            // STEP 1: Query failed files table to determine processing scope (BEFORE rehydration)
            var failedFiles = GetFailedFiles();
            bool isIncrementalMode = failedFiles.Count > 0;

            #endregion

            var hasSuccessfulMove = false;
            long totalSize = 0;
            int totalErrorFiles = 0;

            var retentionInfo = this.archiverRetentionInfo;

            // Determine block length for file operations
            // Currently, only Google Cloud storage supports direct streaming for small files
            long blockLength = 100;
            blockLength = destinationDevice.StorageType is XStorageType.GoogleCloud ? 0 : ConnectionBuilder
                .ValueOf(retentionInfo!.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Data).First())
                .GetInt64(XRIParameterKeys.BLOCK_LENGTH, blockLength);
            logger.Info($"Block length for file operations: {blockLength} bytes, destination storage type: {destinationDevice.StorageType}");

            // Only archive-tier files that need rehydration are kept in memory
            // All other files are processed immediately during streaming
            var deferredFiles = new List<XFileInfo>();
            int totalFileCount = 0;

            #region--- Stream, filter, start rehydration, and process non-archive files immediately

            var deletingIndexesList = this.RetentionIndexService.GetArchivedFileIndexes(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
            Dictionary<string, ArchivedFileIndexInfo> deletingIndexesDictionary = new Dictionary<string, ArchivedFileIndexInfo>();
            if(deletingIndexesList != null && deletingIndexesList.Count > 0)
            {
                logger.Info($"Get deleting data from main index: {deletingIndexesList.Count}");
                foreach (var deletingIdx in deletingIndexesList)
                {
                    var fileName = $"{this.archiverRetentionInfo.JobId}_content_{deletingIdx.ContentDataFileNumber}.dat";
                    deletingIndexesDictionary[fileName] = deletingIdx;
                }
            }
            
            await foreach (var file in fileSource.WithCancellation(cancellationToken))
            {
                if (isIncrementalMode)
                {
                    var key = $"{file.HighName}|{file.Name}";
                    if (!failedFiles.Contains(key)) continue;
                }

                totalFileCount++;

                // Start rehydration for archive-tier blobs and defer them
                if (sourceDevice.StorageType == XStorageType.Azure)
                {
                    VerifyAndCopyArchiverToHot(file);
                    if (BLOBMappings.ContainsKey(file.HighPlusLowName))
                    {
                        deferredFiles.Add(file);
                        continue;
                    }
                }

                // Process non-archive file immediately
                var result = await MoveSingleFileAsync(file, sourceDevice, destinationDevice, enableDelete, isIncrementalMode, blockLength, deletingIndexesDictionary, cancellationToken);
                totalSize += result.Size;
                hasSuccessfulMove |= result.Success;
                if(!result.Success) { totalErrorFiles += 1; }
            }

            if (isIncrementalMode)
                logger.Info($"Incremental move mode: processing {totalFileCount} failed files for job {this.archiverRetentionInfo?.JobId}");
            else
                logger.Info($"Full move mode: processing {totalFileCount} files ({deferredFiles.Count} deferred for rehydration) for job {this.archiverRetentionInfo?.JobId}");

            #endregion

            #region--- Wait for rehydration and process deferred archive-tier files

            if (deferredFiles.Count > 0)
            {
                try
                {
                    //Waiting Rehydration
                    await WaitingRehydrationAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.Warn("Rehydration cancelled for job {0}", this.archiverRetentionInfo?.JobId ?? "unknown");
                    throw;
                }
                catch (JobStopException e)
                {
                    logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                    throw;
                }

                logger.Info($"Rehydration completed for {deferredFiles.Count} files, starting to move deferred files for job {this.archiverRetentionInfo?.JobId}");
                this.IsProcessingArchivedFile = true;
                foreach (var file in deferredFiles)
                {
                    var result = await MoveSingleFileAsync(file, sourceDevice, destinationDevice, enableDelete, isIncrementalMode, blockLength, deletingIndexesDictionary, cancellationToken);
                    totalSize += result.Size;
                    hasSuccessfulMove |= result.Success;
                    if (!result.Success) { totalErrorFiles += 1; }
                }
            }

            #endregion

            //handle the report for deleting indexes that do not have related data blocks.
            if (deletingIndexesDictionary.Count > 0)
            {
                logger.Warn($"These deleting indexes do not have related data blocks. Count: {deletingIndexesDictionary.Count}");

                var reportAction = enableDelete ? "RM_PRM_PRE_Move" : "RM_JS_Common_Copy";
                var actionString = enableDelete ? I18NEntity_RM_PRM_PRE_Move : I18NEntity_RM_JS_Common_Copy;
                var dataVolumePath = this.dataVolume.Replace('\\', '/').TrimEnd('/');

                foreach (var deletingIdx in deletingIndexesDictionary.Values)
                {
                    try
                    {
                        var comment = "The data block may have been deleted previously.";
                        var blobPath = $"{dataVolumePath}/{this.archiverRetentionInfo.JobId}_content_{deletingIdx.ContentDataFileNumber}.dat";
                        logger.Warn($"{comment} {this.archiverRetentionInfo.JobId}_content_{deletingIdx.ContentDataFileNumber}.dat");

                        //var report = new JMArchiverRententionJobDetails
                        //{
                        //    SiteUrl = deletingIdx.Url,
                        //    Size = deletingIdx.ContentLength.ToString(),
                        //    Status = JobDetailsStatus.Skipped,
                        //    SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                        //    DesStorageName = archiverRetentionInfo.DestinationDevice?.Name,
                        //    JobId = archiverRetentionInfo.JobId,
                        //    Action = reportAction,
                        //    Comment = comment,
                        //};

                        var migrationDetails = new JMArchiverRententionMigrationDetails
                        {
                            SiteUrl = this.archiverRetentionInfo.SiteUrl,
                            SharePointUrl = deletingIdx.Url,
                            SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                            DesStorageName = archiverRetentionInfo.DestinationDevice?.Name,
                            BlobPath = blobPath,
                            Status = JobDetailsStatus.Skipped,
                            Size = deletingIdx.ContentLength.ToString(),
                            Action = actionString,
                            JobId = archiverRetentionInfo.JobId,
                            Comment = comment,
                        };

                        //this.AddToReport(report);
                        AddToMigrationReport(migrationDetails);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Failed to add retention job report for move action. error:{e}");
                    }
                }
            }
            if (hasSuccessfulMove || totalFileCount == 0)
            {
                if (totalErrorFiles == 0)
                {
                    // Complete success - cleanup all failure records
                    this.jobStatusInfo.State = 2;
                    CleanupAllFailures();
                    logger.Info($"Job {this.archiverRetentionInfo?.JobId} completed successfully, all failure records cleaned up");
                }
                else
                {
                    // Partial success - keep failure records for retry
                    this.jobStatusInfo.State = 7;
                    logger.Warn($"Job {this.archiverRetentionInfo?.JobId} completed with {totalErrorFiles} failures, failure records retained for retry");
                }
            }
            else
            {
                this.jobStatusInfo.State = 3;
                //CleanupAllFailures(); // Do not clean up failures if no successful moves for incremental retry
                logger.Error($"Job {this.archiverRetentionInfo?.JobId} failed with no successful moves");
            }

            return totalSize;
        }

        private async Task<(long Size, bool Success)> MoveSingleFileAsync(
            XFileInfo item, IXSystem sourceDevice, IXSystem destinationDevice,
            bool enableDelete, bool isIncrementalMode, long blockLength,
            Dictionary<string, ArchivedFileIndexInfo> deletingIndexes,
            CancellationToken cancellationToken)
        {
            using (new CheckJobStopScope()) { }
            var srcInfo = item;
            var destInfo = item.ToCorrectTypeStorageInfo(destinationDevice);
            var originalInfo = XConvert.FromNames(item.HighName, item.Name);
            if (BLOBMappings.ContainsKey(originalInfo.HighPlusLowName))
            {
                srcInfo = BLOBMappings[originalInfo.HighPlusLowName].MappedBlobInfo as XFileInfo;
            }
            else
            {
                var retentionInfo = this.archiverRetentionInfo;
                srcInfo.MetaInfos.Add("Archive-KeepTime", retentionInfo.RetentionTimeSpanSeconds.ToString());
                srcInfo.MetaInfos["Platform"] = ServiceConstants.DocAve;
                srcInfo.MetaInfos["Component"] = "ArchiverBackup";
                srcInfo.MetaInfos["Archive-FarmName"] = retentionInfo.FarmName;
                srcInfo.MetaInfos["Archive-WebAppName"] = retentionInfo.WebApp;
                srcInfo.MetaInfos["Archive-SiteCollectionName"] = retentionInfo.SiteUrl;
                srcInfo.MetaInfos["Archive-JobId"] = retentionInfo.JobId;

                originalInfo.MetaInfos.AddRange(srcInfo.MetaInfos, true);
            }

            if (item.FileSize > 0)
            {
                srcInfo.Length = item.FileSize;
                logger.Debug("Using FileSize {0} from ListFiles for {1}", item.FileSize, item.LowName);
            }
            else
            {
                // Defensive: fallback to OpenFile only if FileSize not available
                logger.Warn("FileSize not available from ListFiles for {0}, performing OpenFile", item.Name);
                srcInfo.Length = sourceDevice.OpenFile(srcInfo)?.FileSize ?? 0;
            }

            bool needProcess = true;
            string? errorMessage = string.Empty;

            try
            {
                ThrowRandomExceptionIfEnabled("RealMove");
                var storageResult = await RealMoveAsync(srcInfo, sourceDevice, destInfo, destinationDevice, blockLength, cancellationToken);

                // Verify commit was successful before proceeding to delete
                if (storageResult == null || !storageResult.IsCommited)
                {
                    needProcess = false;
                    errorMessage = $"Move completed but commit verification failed for {srcInfo.Name}. IsCommited={storageResult?.IsCommited}";
                    logger.Error(errorMessage);
                }
            }
            catch (Exception e)
            {
                needProcess = false;
                errorMessage = $"Failed to move the content to destination storage location due to internal error, Details:{e.Message}";
                logger.Error($"Failed to move the content to destination storage location due to internal error, Details:{e}");
            }

            if (needProcess && enableDelete)
            {
                try
                {
                    ThrowRandomExceptionIfEnabled("DeleteFile");
                    // Always delete the ORIGINAL blob, not the rehydration temp copy
                    var deleteResult = sourceDevice.DeleteFileExt(originalInfo);
                }
                catch (Exception e)
                {
                    needProcess = false;
                    errorMessage = $"The content has been moved to destination storage location, however the content was not removed from the source storage location due to internal error. Details:{e.Message}";
                    logger.Error($"The content has been moved to destination storage location, however the content was not removed from the source storage location due to internal error, Details:{e}");
                }
            }

            try
            {
                var reportAction = enableDelete ? "RM_PRM_PRE_Move" : "RM_JS_Common_Copy";
                var actionString = enableDelete ? I18NEntity_RM_PRM_PRE_Move : I18NEntity_RM_JS_Common_Copy;

                var migrationDetails = new JMArchiverRententionMigrationDetails
                {
                    SiteUrl = this.archiverRetentionInfo.SiteUrl,
                    SharePointUrl = string.Empty,
                    SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                    DesStorageName = archiverRetentionInfo.DestinationDevice?.Name,
                    BlobPath = srcInfo.HighPlusLowName.Replace('\\', '/'),
                    Status = needProcess ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                    Size = srcInfo.Length.ToString(),
                    Action = actionString,
                    JobId = archiverRetentionInfo.JobId,
                    Comment = needProcess ? string.Empty : errorMessage,
                };

                if (deletingIndexes.TryGetValue(srcInfo.Name, out var deletingIdx))
                {
                    deletingIndexes.Remove(srcInfo.Name);

                    migrationDetails.SharePointUrl = deletingIdx.Url;

                    var report = new JMArchiverRententionJobDetails
                    {
                        SiteUrl = deletingIdx.Url,
                        Size = srcInfo.Length.ToString(),
                        Status = needProcess ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                        SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                        DesStorageName = archiverRetentionInfo.DestinationDevice?.Name,
                        JobId = archiverRetentionInfo.JobId,
                        Action = reportAction,
                        Comment = needProcess ? string.Empty : errorMessage,
                    };

                    this.AddToReport(report);
                }

                AddToMigrationReport(migrationDetails);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to add job report for move action. error:{e}");
            }

            // CHECKPOINT: Update failure tracking table based on operation result
            if (needProcess)
            {
                // Success - remove from failure table only in incremental mode
                if (isIncrementalMode)
                {
                    RemoveFailureRecord(item.HighName, item.Name);
                }
                return (srcInfo.Length, true);
            }
            else
            {
                // Failure - add to failure table
                RecordFailure(item.HighName, item.Name);
                return (0, false);
            }
        }

        private string ConvertToSiteUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            input = input.Replace("\\", "/");
            int start = input.IndexOf("https#", StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return input;
            var encoded = input.Substring(start);
            var parts = encoded.Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return input;
            string scheme = parts[0]; // https
            string host = parts[2];   // domain
            var url = $"{scheme}://{host}";
            if (parts.Length > 3)
            {
                var path = string.Join("/", parts.Skip(3));
                url = SecurityUtils.SafeCombinePath(url, path);
            }
            return url;
        }

        /// <summary>
        /// Asynchronously sets a blob to Archive access tier
        /// </summary>
        /// <param name="destinationDevice">The destination storage device</param>
        /// <param name="storageInfo">Storage information for the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task SetFileTierArchive2Async(IXSystem destinationDevice, StorageInfo storageInfo, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (destinationDevice.StorageType == XStorageType.Azure)
                {
                    var device = destinationDevice as IAzureSystem;
                    AzureCloudInfo info = (AzureCloudInfo)storageInfo;
                    info.FileTierType = AccessTierType.Archive;

                    // Replace sync-over-async with proper await
                    var result = await device.ChangeFileTierAsync(info);

                    if (!result.IsChanged)
                    {
                        logger.Warn("An error occurred while setting file Archive. FileName: {0}", storageInfo.LowName);
                    }
                    else
                    {
                        logger.Debug("Successfully set file tier to Archive. FileName: {0}", storageInfo.LowName);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.Warn("SetFileTierArchive cancelled for file {0}", storageInfo?.LowName ?? "unknown");
                throw;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while setting file Archive. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo?.LowName ?? "unknown");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously waits for blob rehydration from Archive tier to Hot tier
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop waiting</param>
        /// <exception cref="TimeoutException">Thrown when rehydration exceeds 5-day timeout</exception>
        private async Task WaitingRehydrationAsync(CancellationToken cancellationToken)
        {
            DateTime time = DateTime.Now;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
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
                        await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
                    }
                    else
                    {
                        if (needContinueSleep)
                        {
                            // Timeout exceeded without full rehydration
                            var elapsed = DateTime.Now - time;
                            throw new TimeoutException(
                                $"Blob rehydration timed out after {elapsed.TotalDays:F2} days (max 5 days) for job {this.archiverRetentionInfo?.JobId}");
                        }
                        logger.Info($"Exit waiting blob rehydration, all the datas rehydration : {!needContinueSleep} .");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.Warn("Blob rehydration cancelled for job {0}", this.archiverRetentionInfo?.JobId ?? "unknown");
                throw;
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



        #region Incremental Move Support - Failure Tracking Only

        /// <summary>
        /// Gets the set of failed files for the current job from the failure tracking table.
        /// Returns empty set if this is the first run or all files previously succeeded.
        /// Creates the tracking table if it doesn't exist (lazy initialization).
        /// </summary>
        /// <returns>HashSet of file keys in format "HighName|LowName"</returns>
        private HashSet<string> GetFailedFiles()
        {
            var failedFiles = new HashSet<string>();
            var jobId = this.archiverRetentionInfo?.JobId;
            var storagePolicyId = this.archiverRetentionInfo?.StoragePolicyId;

            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(storagePolicyId))
            {
                logger.Warn("JobId or StoragePolicyId is null or empty, cannot query failed files");
                return failedFiles;
            }

            try
            {
                // Ensure table exists (lazy initialization - more reliable than XML upgradeConfiguration)
                var createTableSql = @"CREATE TABLE IF NOT EXISTS TB_RETENTION_MOVE_FAILED(
                    COL_ID VARCHAR(36) NOT NULL PRIMARY KEY, 
                    COL_STORAGE_POLICY_ID CHAR(36) NOT NULL, 
                    COL_JOB_ID VARCHAR(100) NOT NULL, 
                    COL_HIGH_NAME TEXT NOT NULL, 
                    COL_LOW_NAME VARCHAR(255) NOT NULL, 
                    CONSTRAINT UQ_POLICY_JOB_FILE UNIQUE (COL_STORAGE_POLICY_ID, COL_JOB_ID, COL_HIGH_NAME, COL_LOW_NAME)
                )";
                this.IndexMainProcessor.Execute(createTableSql, new Dictionary<string, object>());

                // Create index if not exists (improves query performance)
                var createIndexSql = "CREATE INDEX IF NOT EXISTS IX_MOVE_FAILED_POLICY_JOB ON TB_RETENTION_MOVE_FAILED (COL_STORAGE_POLICY_ID, COL_JOB_ID)";
                this.IndexMainProcessor.Execute(createIndexSql, new Dictionary<string, object>());

                // Query failed files for this job
                var sql = "SELECT COL_HIGH_NAME, COL_LOW_NAME FROM TB_RETENTION_MOVE_FAILED WHERE COL_STORAGE_POLICY_ID = @policyId AND COL_JOB_ID = @jobId";
                var parameters = new Dictionary<string, object>
                {
                    { "@policyId", storagePolicyId },
                    { "@jobId", jobId }
                };

                var dataTable = this.IndexMainProcessor.ExecuteQuery(sql, parameters);

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var highName = row["COL_HIGH_NAME"]?.ToString() ?? string.Empty;
                        var lowName = row["COL_LOW_NAME"]?.ToString() ?? string.Empty;
                        var key = $"{highName}|{lowName}";
                        failedFiles.Add(key);
                    }
                }

                logger.Info($"Found {failedFiles.Count} failed files for job {jobId}");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to query failed files for job {jobId}: {e.Message}", e);
                failedFiles.Clear();
                throw new Exception($"Failed to query failed files for job");
            }

            return failedFiles;
        }

        /// <summary>
        /// Records a file failure in the tracking table.
        /// Only inserts if record doesn't exist (uses INSERT OR IGNORE for simplicity).
        /// </summary>
        /// <param name="highName">The HighName of the failed file</param>
        /// <param name="lowName">The Name/LowName of the failed file</param>
        private void RecordFailure(string highName, string lowName)
        {
            var jobId = this.archiverRetentionInfo?.JobId;
            var storagePolicyId = this.archiverRetentionInfo?.StoragePolicyId;

            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(storagePolicyId))
            {
                return;
            }

            try
            {
                var recordId = Guid.NewGuid().ToString();
                var insertSql = "INSERT OR IGNORE INTO TB_RETENTION_MOVE_FAILED (COL_ID, COL_STORAGE_POLICY_ID, COL_JOB_ID, COL_HIGH_NAME, COL_LOW_NAME) VALUES (@id, @policyId, @jobId, @highName, @lowName)";
                var insertParams = new Dictionary<string, object>
                {
                    { "@id", recordId },
                    { "@policyId", storagePolicyId },
                    { "@jobId", jobId },
                    { "@highName", highName ?? string.Empty },
                    { "@lowName", lowName ?? string.Empty }
                };
                this.IndexMainProcessor.Execute(insertSql, insertParams);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to record failure for {lowName}: {e.Message}", e);
            }
        }

        /// <summary>
        /// Removes a specific file's failure record after successful move.
        /// Called immediately when a file moves successfully.
        /// </summary>
        /// <param name="highName">The HighName of the succeeded file</param>
        /// <param name="lowName">The Name/LowName of the succeeded file</param>
        private void RemoveFailureRecord(string highName, string lowName)
        {
            var jobId = this.archiverRetentionInfo?.JobId;
            var storagePolicyId = this.archiverRetentionInfo?.StoragePolicyId;

            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(storagePolicyId))
            {
                return;
            }

            try
            {
                var sql = "DELETE FROM TB_RETENTION_MOVE_FAILED WHERE COL_STORAGE_POLICY_ID = @policyId AND COL_JOB_ID = @jobId AND COL_HIGH_NAME = @highName AND COL_LOW_NAME = @lowName";
                var parameters = new Dictionary<string, object>
                {
                    { "@policyId", storagePolicyId },
                    { "@jobId", jobId },
                    { "@highName", highName ?? string.Empty },
                    { "@lowName", lowName ?? string.Empty }
                };

                this.IndexMainProcessor.Execute(sql, parameters);
                logger.Debug($"Removed failure record for {lowName}");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to remove failure record for {lowName}: {e.Message}", e);
            }
        }

        /// <summary>
        /// Removes all failure records for the current job after complete success.
        /// Called when all files have been successfully moved.
        /// </summary>
        private void CleanupAllFailures()
        {
            var jobId = this.archiverRetentionInfo?.JobId;
            var storagePolicyId = this.archiverRetentionInfo?.StoragePolicyId;

            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(storagePolicyId))
            {
                return;
            }

            try
            {
                var sql = "DELETE FROM TB_RETENTION_MOVE_FAILED WHERE COL_STORAGE_POLICY_ID = @policyId AND COL_JOB_ID = @jobId";
                var parameters = new Dictionary<string, object>
                {
                    { "@policyId", storagePolicyId },
                    { "@jobId", jobId }
                };

                this.IndexMainProcessor.Execute(sql, parameters);
                logger.Info($"Cleaned up failure records for job {jobId}");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to cleanup failure records for job {jobId}: {e.Message}", e);
            }
        }

        #endregion

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
                                if (web == null || !web.Exists)
                                {
                                    logger.Error($"Cannot find web: {webServerRelatedUrl} in this site. so skip remove stub.");
                                    continue;
                                }
                                foreach (var (docUrl, nodeGuid) in docFullUrls[webUrl])
                                {
                                    int stubCount = 0; 
                                    try
                                    {
                                        var possiblyStubSuffixes = GetPossiblyStubSuffixes(defaultSuffix);
                                        foreach (var stub in possiblyStubSuffixes)
                                        {
                                            stubCount++;
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
                                                        stubFile.Item.SetComplianceTagOnBulkItems("");
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
                                        logger.Info($"The number of check stub suffix is [{stubCount}].");
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
                throw new Exception("Failed to delete stub file. Reason:", e);
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

        private void AddSimulateRetentionToReport(ArchiverBasicIndex fileIndex, string fileName, long fileSize, JobDetailsStatus status, string action, string storageName = "")
        {
            try
            {
                var realReport = GenerateFileLevelRetentionDetail(fileIndex.ExtraInfo, fileIndex.Url, fileSize, status, action, "", storageName);
                JMArchiverRententionDashboardDetails dashboardReport = new JMArchiverRententionDashboardDetails(realReport)
                {
                    RetentionKeepDate = archiverRetentionInfo.KeepValue,
                    RetentionKeepDateUnit = (int)archiverRetentionInfo.ArchiveDateUnit,
                    RetentionSource = archiverRetentionInfo.RetentionSourceName,
                    SourceFlag = archiverRetentionInfo.SourceFlag,
                };
                this.AddToReport(dashboardReport);
            }
            catch (Exception e)
            {
                logger.Error($"Add retention to report failed,itemname:{fileName}.error:{e}");
            }
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
        private void AddMarkTierRetentionToReport(ArchiverBasicIndex fileIndex, string fileName, long fileSize, JobDetailsStatus status, string action, string storageName = "")
        {
            try
            {
                var realReport = GenerateFileLevelRetentionDetail(fileIndex.ExtraInfo, fileIndex.Url, fileSize, status, action, "", storageName);
                realReport.Size = string.Empty;
                this.AddToReport(realReport);
            }
            catch (Exception e)
            {
                logger.Error($"Add retention to report failed,itemname:{fileName}.error:{e}");
            }
        }
        private void AddMoveRetentionReport(ArchiverBasicIndex fileIndex,JobDetailsStatus status)
        {
            try
            {
                var report = new JMArchiverRententionJobDetails();
                report.SiteUrl = GetFullPath(fileIndex.ExtraInfo, fileIndex.Url);
                report.Size = "0";
                report.Status = status;
                report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                report.DesStorageName = archiverRetentionInfo.DestinationDevice?.Name;
                report.JobId = archiverRetentionInfo.JobId;
                report.Action = "RM_PRM_PRE_Move";
                this.AddToReport(report);
            }
            catch (Exception e)
            {
                logger.Error($"Add move retention to report failed,itemname:error:{e}");
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


        private (Int64, Int64) SimulateDeleteDataFromDevice(String dataVolume, String indexVolume, Boolean NeedDeleteSubIndex, bool isFitSoftDeleteAndRetainByModifedTime = false, bool needToAddJobDetail = true)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            String stubType = string.Empty;
            var tempDeleteDataSize = default(Int64);
            var tempDeleteDataNumber = default(Int64);
            StorageDeleteResult deleteDataResult = new StorageDeleteResult();
            StorageDeleteResult deleteIndexResult = new StorageDeleteResult();
            // *** 对于StorageInterfaceType.Object 类型的Device不支持Dedup，如果支持还需要进一步完善 ***
            if (this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object))
            {
                logger.Warn("this device storage InterfaceType is Object,it may wrong");
                var storageInfoList = this.RetentionIndexService.GetStorageInfosByJobId(this.archiverRetentionInfo.JobId);
                tempDeleteDataSize = this.DeleteFileByStorageInfo(storageInfoList);
                //if (this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                //{
                //    if (this.archiverRetentionInfo.RemoveOrphanedStub)
                //    {
                //        string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                //        var stubUrlList = this.RetentionIndexService.FilterDocumentUrlFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, ref stubType);
                //        RemoveStubFromSharePoint(stubUrlList, this.archiverRetentionInfo.TenantGroupId, siteUrl, this.archiverRetentionInfo.JobId, stubType);
                //    }
                //    var retentionInfoList = this.RetentionIndexService.GetDeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.SiteUrl);
                //    this.RetentionIndexService.DeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                //    UpdateArchivedInfo(this.archiverRetentionInfo.SiteUrl);
                //    UpdateRetentionInfo(retentionInfoList);
                //}
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
                    else if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ArchiveTime)
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
                                //RealDeleteDeduplicateFileDataFromDevice(deletingIdx, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                                continue;
                            }

                            var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                            logger.Info($"Start to delete device content: {info.HighPlusLowName}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                            try
                            {
                                deleteDataResult = new StorageDeleteResult()
                                {
                                    DeletedFileSize = this.dataLogicalDevice.OpenFile(info).FileSize
                                };
                                //ChangeLorealBlobFromPreviousVersionToDelete(info);
                                var delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                tempDeleteDataSize += delSize;
                                if (needToAddJobDetail)
                                {
                                    if (delSize == 0)
                                    {
                                        delSize = deletingIdx.ContentLength;
                                    }
                                    tempDeleteDataNumber++;
                                    AddSimulateRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                                isDeleteSucceedAtLeastOnce = true;
                                contentSize += delSize;
                            }
                            catch (Exception ex)
                            {
                                if (!isDeleteSucceedAtLeastOnce)
                                {
                                    if (needToAddJobDetail)
                                    {
                                        tempDeleteDataNumber++;
                                        AddSimulateRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
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

                    if (tempDeleteDataSize <= 0)
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
                            deleteDataResult = new StorageDeleteResult()
                            {
                                DeletedFileSize = this.dataLogicalDevice.OpenFile(info).FileSize
                            };
                            //ChangeLorealBlobFromPreviousVersionToDelete(info);
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

                    var deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    if (deletingIndexes != null && deletingIndexes.Count > 0)
                    {
                        foreach (var deletingIdx in deletingIndexes)
                        {
                            var delSize = deletingIdx.ContentLength;
                            tempDeleteDataNumber++;
                            AddSimulateRetentionToReport(deletingIdx, "", delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                        }
                    }
                }
            }


            return (tempDeleteDataSize, tempDeleteDataNumber);
        }


        private Int64 DeleteDataFromDevice(String dataVolume, String indexVolume, Boolean NeedDeleteSubIndex,bool isFitSoftDeleteAndRetainByModifedTime = false,bool isDeleteAction = true)
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
                                if (delSize == 0)
                                {
                                    delSize = deletingIdx.ContentLength;
                                }
                                if (isDeleteAction)
                                {
                                    AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                                else
                                {
                                    AddMoveRetentionReport(deletingIdx, JobDetailsStatus.Successful);
                                }
                                isDeleteSucceedAtLeastOnce = true;
                                contentSize += delSize;
                            }
                            catch (Exception ex)
                            {
                                if (!isDeleteSucceedAtLeastOnce)
                                {
                                    if (isDeleteAction)
                                    {
                                        AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                    }
                                    else
                                    {
                                        AddMoveRetentionReport(deletingIdx, JobDetailsStatus.Successful);
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
                        if (this.archiverRetentionInfo.DeleteStatus != (int)DeletedStatus.Normal)
                        {
                            logger.Info($"Current job id is {this.archiverRetentionInfo.RetentionJob.Id},the delete status is:[{this.archiverRetentionInfo.DeleteStatus}]");
                        }
                        else
                        {
                            RemoveStubFromSharePoint(stubUrlList, this.archiverRetentionInfo.TenantGroupId, siteUrl, this.archiverRetentionInfo.JobId, stubType);
                        }
                    }
                    logger.Info($"Current job id is {this.archiverRetentionInfo.RetentionJob.Id}");
                    var retentionInfoList = this.RetentionIndexService.GetDeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.SiteUrl);
                    if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        this.RetentionIndexService.DeleteDataFromMainIndexByDateTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, SoftTimeConvert(this.archiverRetentionInfo.DateTimeNow, this.archiverRetentionInfo.SoftDeleteKeepValue, this.archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                    }
                    else
                    {
                        if (!this.archiverRetentionInfo.IsFileLevelBlockBackup)
                        {
                            var deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                            if (deletingIndexes != null && deletingIndexes.Count > 0)
                            {
                                foreach (var deletingIdx in deletingIndexes)
                                {
                                    var delSize = deletingIdx.ContentLength;
                                    AddRetentionToReport(deletingIdx, "", delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                            }
                        }
                        this.RetentionIndexService.DeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    }

                    UpdateArchivedInfo(this.archiverRetentionInfo.SiteUrl);
                    UpdateRetentionInfo(retentionInfoList);
                }
                if (this.archiverRetentionInfo.RetentionRule == RetentionRule.MoveArchiverJobData)
                {
                    var deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                    if (deletingIndexes != null && deletingIndexes.Count > 0)
                    {
                        foreach (var deletingIdx in deletingIndexes)
                        {
                            AddMoveRetentionReport(deletingIdx, JobDetailsStatus.Successful);
                        }
                    }
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
                    if (!this.archiverRetentionInfo.IsFileLevelBlockBackup)
                    {
                        var deletingIndexes = this.RetentionIndexService.GetDeletingDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
                        if (deletingIndexes != null && deletingIndexes.Count > 0)
                        {
                            HashSet<string> needDeletedFileContentName = new HashSet<string>();
                            foreach (var deletingIdx in deletingIndexes)
                            {
                                var delSize = deletingIdx.ContentLength;
                                AddRetentionToReport(deletingIdx, "", delSize, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_SoftDelete", archiverRetentionInfo.DataLogicalDevice.Name);
                            }
                        }
                    }
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
                        if (!archiverRetentionInfo.IsSimulateJob)
                        {
                            deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                            ChangeLorealBlobFromPreviousVersionToDelete(info);
                        }
                        else
                        {
                            deleteDataResult = new StorageDeleteResult() { DeletedFileSize = item.FileSize };
                        }
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

        /// <summary>
        /// Validates preconditions before executing retention operations
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when preconditions are not met</exception>
        private void ValidatePreConditions()
        {
            if (this.archiverRetentionInfo == null)
            {
                throw new InvalidOperationException(
                    "Retention info not initialized. Please call Open() first.");
            }

            if (string.IsNullOrEmpty(this.dataVolume))
            {
                throw new InvalidOperationException(
                    "Data volume is not set. Cannot proceed with retention operation.");
            }

            if (string.IsNullOrEmpty(this.indexVolume))
            {
                throw new InvalidOperationException(
                    "Index volume is not set. Cannot proceed with retention operation.");
            }

            if (this.dataLogicalDevice == null)
            {
                throw new InvalidOperationException(
                    "Data logical device is not opened. Device must be opened before retention.");
            }

            if (this.destinationLogicalDevice == null)
            {
                throw new InvalidOperationException(
                    "Destination logical device is not opened. Device must be opened before retention.");
            }

            this.logger.Debug("Preconditions validated successfully for job {0}",
                             this.archiverRetentionInfo?.JobId ?? "unknown");
        }
    }
}