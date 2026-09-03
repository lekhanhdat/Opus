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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Media.Common;
using Media.Service.ArchiverBackup.Index;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using Storage.Cloud.Azure;
using System.Reflection;
using System.Xml;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule;

namespace AvePoint.Media.Service.ArchiverBackup;

public class GDriveArchiverBackupRetentionService
       : RetentionServiceBase<ArchiverRetentionInfo, ArchiverRetentionResult>
        , IRetentionService
    {
        private readonly AveLogger _logger = new (MethodBase.GetCurrentMethod().DeclaringType);
        private ArchiverRetentionInfo _archiverRetentionInfo = new ();
        private readonly JobStatusInfo _jobStatusInfo = new ();
        private IXSystem _indexLogicalDevice;
        private IXSystem _dataLogicalDevice;
        private IXSystem _destinationLogicalDevice;
        private long _deleteDataSize;
        private string _dataVolume;
        private string _indexVolume;
        private AccessTierType _accessTierType;

        private readonly SafeDictionary<string, BLOBRehydrationMapping> _blobMappings = new();
        private string _errorMessage = ServiceConstants.ArchvierRetentionFailedMessage;

        private string rehydrationTemp;
        private readonly Object _rehydrationLock = new Object();
        private bool _destinationStoreInArchiverTier;
        private static readonly IRMArchiveGDriveInfoDao ArchiveGDriveInfoDao = PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private bool _isArchiveTierToColdTier;

        private bool _needCommitSubIndex = false;
        

        private readonly IGDriveArchiverRetentionIndexService _retentionIndexService = PlatformWindsorManager.GetService<IGDriveArchiverRetentionIndexService>();
        
        public IIndexProcessor<GDriveArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }

        public IIndexService<GDriveIndexServiceOpenParameter> IndexService { get; set; }
        
        public IIndexProcessor<GDriveArchiverIndexProcessorParameter> IndexSubProcessor = null;
        
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public IStorageDeviceManager DeviceManager { get; set; }

        private BlobContainerClient _sourceContainerClient;
        
        private bool isLorealSoftDelete;


        public override void Open(ArchiverRetentionInfo retentionInfo)
        {
            this._archiverRetentionInfo = retentionInfo;
            this._logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceOpenStart, this._archiverRetentionInfo.JobId);
            this._jobStatusInfo.State = 2;
            this._dataVolume = retentionInfo.DataVolume;
            this._indexVolume = retentionInfo.IndexVolume;
            this._accessTierType = retentionInfo.AccessTierType;
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            this._indexLogicalDevice = this.DeviceManager.Open(this._archiverRetentionInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this._dataLogicalDevice = XFactory.InstanceSystem(this._archiverRetentionInfo.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data)[0]);
            _dataLogicalDevice.Open();
            this.CacheManager.Open(retentionInfo.CacheSetting, false, true);
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);
            if (this._archiverRetentionInfo.RetentionRule == RetentionRule.MoveArchiverJobData)
            {
                this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDevice);
                this._destinationLogicalDevice = this.DeviceManager.Open(retentionInfo.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Data));
                this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDeviceFinished);
            }

            _logger.Info("Open index service.");
            this.OpenMainIndex(_archiverRetentionInfo, _indexVolume);
            if (retentionInfo.RetentionRule == RetentionRule.MarkArchiverJobDataTier ||
                (retentionInfo.IsSoftDelete && !retentionInfo.IsFitSoftDelete))
            {
                this.OpenSubIndex(_archiverRetentionInfo, _indexVolume);
            }
            this._destinationStoreInArchiverTier = retentionInfo.DestinationStoreInArchiverTier;
            this.rehydrationTemp = "data_google_archive\\Temp\\" + Guid.NewGuid();
            isLorealSoftDelete = IsEnabledRealDelete();
        }

        private void OpenSubIndex(ArchiverRetentionInfo archiverRetentionInfo, string indexVolume)
        {
            _logger.Info("Begin opening sub index");
            var indexServiceOpenParameter = new GDriveIndexServiceOpenParameter()
            {
                IndexDatabaseName = archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName,
                BackupJobId = archiverRetentionInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this._indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRetentionInfo.CacheSetting,
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo
            };
            this.InitIndexProcessor(indexServiceOpenParameter);
        }

        private void OpenMainIndex(ArchiverRetentionInfo archiverRetentionInfo, string indexVolume)
        {
            this._logger.Info("Begin opening main index.");
            var indexOpenParam = new GDriveIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                BackupJobId = archiverRetentionInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this._indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRetentionInfo.CacheSetting,
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo
            };
            IndexSynchronizer.Initialize(indexOpenParam);
            this.InitIndexProcessor(indexOpenParam);
        }
        
        private void InitIndexProcessor(GDriveIndexServiceOpenParameter openParam)
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
            GDriveArchiverIndexProcessorParameter param = new (IdentityManager.IdentityContent)
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
                    this.IndexSubProcessor = new IndexProcessor<GDriveArchiverIndexProcessorParameter>();
                }
                param.IsNeedCheckIntegrity = true;
                this.IndexSubProcessor.Open(param);
            }
            this._logger.Info("Open MainIndex Finished.");
        }

        public override ArchiverRetentionResult Retain(ArchiverRetentionInfo retentionInfo)
        {
            var retentionResult = new ArchiverRetentionResult();
            switch (this._archiverRetentionInfo.RetentionRule)
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
                    throw new UnknownFileTypeException(String.Format(MediaServiceArchiverBackupResource.RetentionServiceRetainUnknownFileTypeException, this._archiverRetentionInfo.RetentionRule.ToString()));
            }
            return retentionResult;
        }

        public override void GenerateJobReport(Int32 jobState)
        {
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportBegin, this._archiverRetentionInfo.JobId);
        }

        public override void UpdateJobStatusAndControlTable(Int32 jobState)
        {
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceUpdateJobStatusAndControlTableBegin);

            this._logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceUpdateJobStatusAndControlTableEnd, this._archiverRetentionInfo.JobId);
        }

        public override void ProcessException(Exception e, ArchiverRetentionResult result)
        {
            e = e.InnerException ?? e;
            switch (this._archiverRetentionInfo.RetentionRule)
            {
                case RetentionRule.RetainArchiverJobData:
                    this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataDeleteDataError, this._archiverRetentionInfo.JobId, e.ToString());
                    break;
                case RetentionRule.MoveArchiverJobData:
                    this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataError, this._archiverRetentionInfo.JobId, e.ToString());
                    break;
            }
        }

        public override void Dispose()
        {
            this.UploadIndexToRealSystem();
            if (this.IndexService != null && _archiverRetentionInfo.RetentionRule.Equals(RetentionRule.RetainArchiverJobData))
            {
                this.IndexService.Close();
            }
            try
            {
                if (rehydrationTemp.Contains("Temp"))
                {
                    StorageInfo rehydrationTempInfo = new StorageInfo() { HighName = rehydrationTemp };
                    this._dataLogicalDevice.DeleteDirectory(rehydrationTempInfo);
                }
            }
            catch (Exception e)
            {
                _logger.Warn("An error occurred while deleting rehydration temp folder. error:{0}", e.ToString());
            }
            if (this.DeviceManager != null)
            {
                this.DeviceManager.Close(this._indexLogicalDevice);
                this.DeviceManager.Close(this._dataLogicalDevice);
                this.DeviceManager.Close(this._destinationLogicalDevice);
            }
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
                var subdbInfo = new IndexDatabaseInfo(_archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName, null);
                this.IndexSynchronizer.Upload(subdbInfo);
            }
            var storageInfo = XConvert.FromNames(_archiverRetentionInfo.IndexVolume, ServiceConstants.IndexDBName, _archiverRetentionInfo.MainIndexStorageInfo);
            var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
            this.IndexSynchronizer.Upload(dbInfo);
        }

        private ArchiverRetentionResult MoveJobData()
        {
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataBegin, this._archiverRetentionInfo.JobId);
            this._deleteDataSize = this.MoveDataFromDevice(this._dataVolume, this._indexVolume);
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataFinished, this._archiverRetentionInfo.JobId, this._deleteDataSize.ToString());
            var result = this.ConvertInfoToResult(this._archiverRetentionInfo);
            result.Size = _deleteDataSize;
            result.State = 2;
            return result;
        }

        private ArchiverRetentionResult RetainJobData()
        {
            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataBegin, this._archiverRetentionInfo.JobId);
            _logger.Info($"retention type and soft delete info is:retentionType:{this._archiverRetentionInfo.RetentionDataTimeType},isFitSoftDelete:{this._archiverRetentionInfo.IsFitSoftDelete},isSoftDelete:{this._archiverRetentionInfo.IsSoftDelete}");
            if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime && this._archiverRetentionInfo.IsSoftDelete && !this._archiverRetentionInfo.IsSystemStorage)
            {
                _logger.Info("this action is retain by modified time delete");
                _deleteDataSize = this.DeleteDataFromDevice(this._dataVolume, this._indexVolume, true, true);
                SoftDelete(this._dataVolume, this._indexVolume);
            }
            else if (!this._archiverRetentionInfo.IsFitSoftDelete && this._archiverRetentionInfo.IsSoftDelete && !this._archiverRetentionInfo.IsSystemStorage)
            {
                _logger.Info("this action is soft delete");
                _deleteDataSize = this.SoftDelete(this._dataVolume, this._indexVolume);
            }
            else
            {
                _logger.Info("this action is real delete");
                this._deleteDataSize = this.DeleteDataFromDevice(this._dataVolume, this._indexVolume, true);
            }
            //this.deleteDataSize += this.DeleteIndexFromDevice();
            this._logger.Info(MediaServiceArchiverBackupResource.RetentionServiceRetainJobDataDeleteDataFinished, this._archiverRetentionInfo.JobId, this._deleteDataSize.ToString());
            var result = this.ConvertInfoToResult(this._archiverRetentionInfo);
            result.Size = _deleteDataSize;
            result.State = 2;
            //result.HasIndexRelatedToBackupJob = IsExistsIndexRelatedToJob(this._archiverRetentionInfo.JobId);
            return result;
        }

        
        private ArchiverRetentionResult MarkJobDataTier()
        {
            this._logger.Info($"start mark job data tier,{this._archiverRetentionInfo.JobId}");
            this.MarkDataTierFromDevice(this._archiverRetentionInfo, this._dataVolume);
            this._logger.Info($"finish mark job data tier,{this._archiverRetentionInfo.JobId}");
            var result = this.ConvertInfoToResult(this._archiverRetentionInfo);
            if (_isArchiveTierToColdTier)
            {
                result.IsArchiveTierToColdTier = true;
            }
            result.State = 2;
            return result;
        }
        private void MarkDataTierFromDevice(ArchiverRetentionInfo retentionInfo, String dataVolume)
        {
            var isMarkSucceedAtLeastOnce = false;
            var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
            var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(retentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
            _logger.Info($"Need mark blobs count : {fileList.Count}");
            fileList.ForEach(item =>
            {
                var info = XConvert.FromNames(item.HighName, item.Name);
                var file = this._dataLogicalDevice.OpenFile(info);
                try
                {
                    SetFileTierArchiveAsync(_dataLogicalDevice, info, file).GetAwaiter().GetResult();
                    isMarkSucceedAtLeastOnce = true;
                }
                catch (Exception ex)
                {
                    if (!isMarkSucceedAtLeastOnce)
                    {
                        _errorMessage = ex.Message;
                        _jobStatusInfo.State = 3;
                        _logger.Error($"mark data tier failed,{info.LowName} error:{ex.ToString()}");
                        throw;
                    }
                    else
                    {
                        _jobStatusInfo.State = 7;
                        _logger.Error($"mark data tier all failed,{info.LowName} error:{ex.ToString()}");
                    }
                }
            });
            var indexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
            if (indexes is { Count: > 0 })
            {
                foreach (var index in indexes)
                {
                    AddMarkTierRetentionToReport(index, "", 0, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_MarkDataTier", _archiverRetentionInfo.DataLogicalDevice.Name);
                }
            }
            UpdateSubIndexTier(this._accessTierType == AccessTierType.Archive, retentionInfo.JobId);
        }
        
        private void UpdateSubIndexTier(bool isArchiveTier, String jobId)
        {
            var parameters = new Dictionary<String, Object>
            {
                ["@jobId"] = jobId,
                ["@tier"] = isArchiveTier ? (int)Storage.AccessTierType.Archive : 0
            };
            var deleteBodyTable = "update " + IndexConstants.TableNameGDriveItem + " set COL_STORAG_ACCESSTIERTYPE = @tier where COL_JOB_ID = @jobId";
            this.IndexSubProcessor.Execute(deleteBodyTable, parameters);
            this.IndexMainProcessor.Execute(deleteBodyTable, parameters);
        }
        
        private void AddMarkTierRetentionToReport(GoogleBasicIndex fileIndex, string fileName, long fileSize, JobDetailsStatus status, string action, string storageName = "")
        {
            try
            {
                var realReport = GenerateFileLevelRetentionDetail("", fileIndex.Path, fileSize, status, action, "", storageName);
                realReport.Size = string.Empty;
                this.AddToReport(realReport);
            }
            catch (Exception e)
            {
                _logger.Error($"Add retention to report failed,itemname:{fileName}.error:{e}");
            }
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
                        if (tempFile?.FileTierType == AccessTierType.Archive && this._accessTierType == AccessTierType.Cold)
                        {
                            _isArchiveTierToColdTier = true;
                        }
                        if (this._accessTierType != tempFile.FileTierType)
                        {
                            var device = destinationDevice as IAzureSystem;
                            AzureCloudInfo info = new AzureCloudInfo();
                            info.HighName = storageInfo.HighName;
                            info.LowName = storageInfo.LowName;
                            info.FileTierType = this._accessTierType;
                            var result = await device.ChangeFileTierAsync(info);
                            if (!result.IsChanged)
                                _logger.Warn("An error occurred while setting file tier. FileName: {0}", storageInfo.LowName);
                        }
                        else
                        {
                            _logger.Info($"will not mark tier,tempFile.tier:{tempFile?.FileTierType.ToString()},accessTierType:{this._accessTierType}. FileName: {storageInfo.LowName}");
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
                _logger.Warn("job is stopped when SetFileTierArchive");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warn("An error occurred while setting file tier. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
                throw;
            }
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
                    result = this._dataLogicalDevice.DeleteFile(info);
                    //ChangeLorealBlobFromPreviousVersionToDelete(info);
                    isDeleteSucceed = true;
                    this._logger.Debug(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoInfo, storageInfo);
                }
                catch (Exception ex)
                {
                    if (!isDeleteSucceed)
                    {
                        this._errorMessage = ex.Message;
                        this._jobStatusInfo.State = 3;
                        this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                        throw;
                    }
                    else
                    {
                        this._jobStatusInfo.State = 7;
                        this._logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoWarn, storageInfo, ex.ToString());
                    }
                }
                deleteResult.DeletedFileSize += Math.Max(result.DeletedFileSize, 0);
            }
            return deleteResult.DeletedFileSize;
        }

        private Int64 MoveDataFromDevice(string dataVolume, string indexVolume)
        {
            var tempDeleteDataSize = default(Int64);
            var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
            var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this._archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
            _logger.Info($"Need move blobs count : {fileList.Count}");
            tempDeleteDataSize = this.MoveAndDeleteFileFromDevice(this._dataLogicalDevice, this._destinationLogicalDevice, fileList);
            return tempDeleteDataSize;
        }

        private Int64 MoveAndDeleteFileFromDevice(IXSystem sourceDevice, IXSystem destinationDevice,
            List<XFileInfo> fileList)
        {
            Int32 moveTime = 1;
            Int32 totalMoveTimes = fileList.Count * 2;
            fileList.ForEach(item =>
            {
                StorageInfo info = new StorageInfo
                {
                    HighName = item.HighName,
                    LowName = item.LowName
                };
                VerifyAndCopyArchiverToHot(info);
            });

            try
            {
                if (_blobMappings.Count > 0)
                {
                    //Waiting Rehydration
                    WaitingRehydration();
                }
            }
            catch (JobStopException e)
            {
                _logger.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }

            var retentionInfo = this._archiverRetentionInfo;
            fileList.ForEach(item =>
            {
                var info = XConvert.FromNames(item.HighName, item.Name);
                info.MetaInfos.Add("Archive-KeepTime", retentionInfo.RetentionTimeSpanSeconds.ToString());
                info.MetaInfos["Platform"] = ServiceConstants.DocAve;
                info.MetaInfos["Component"] = "ArchiverBackup";
                info.MetaInfos["Archive-FarmName"] = retentionInfo.FarmName;
                info.MetaInfos["Archive-WebAppName"] = retentionInfo.WebApp;
                info.MetaInfos["Archive-SiteCollectionName"] = retentionInfo.SiteUrl;
                //info.MetaInfos["Archive-PlanId"] = retentionInfo.PlanId;
                info.MetaInfos["Archive-JobId"] = retentionInfo.JobId;
                Int64 dataMode = this._retentionIndexService.GetJobDataMode(retentionInfo.JobId);
                info.MetaInfos["Archive-DataMode"] = Convert.ToString(dataMode);
                this._logger.Info(
                    MediaServiceArchiverBackupResource
                        .ArchiverBackupRetentionServiceMoveAndDeleteFileFromDeviceDataMode, dataMode);
                info.Length = sourceDevice.OpenFile(info).FileSize; //for cloud
                StorageResult storageResult = null;

                if (_blobMappings.ContainsKey(info.HighPlusLowName))
                {
                    StorageInfo sourceInfo = _blobMappings[info.HighPlusLowName].MappedBlobInfo;
                    storageResult = RealMove(sourceInfo, sourceDevice, info, destinationDevice);
                }
                else
                {
                    storageResult = RealMove(info, sourceDevice, info, destinationDevice);
                }

                if (destinationDevice.StorageType == XStorageType.Azure && _destinationStoreInArchiverTier)
                {
                    SetFileTierArchive(destinationDevice, info);
                }
            });

            return this.DeleteDataFromDevice(this._dataVolume, this._indexVolume, false, false);
        }

        private ArchiverRetentionResult ConvertInfoToResult(ArchiverRetentionInfo info)
        {
            var result = new ArchiverRetentionResult
            {
                FarmName = info.FarmName,
                JobId = info.JobId,
                SiteUrl = info.SiteUrl,
                ArchiverBackupTime = info.ArchiverBackupTime,
                StoragePolicyId = info.StoragePolicyId,
                MediaService = info.MediaService,
                RetentionAction = info.RetentionAction,
                RetentionJob = info.RetentionJob,
                DestinationPhysicalDeviceId = info.DestinationPhysicalDeviceId,
                DataLogicalDevice = info.DataLogicalDevice,
                IndexLogicalDevice = info.IndexLogicalDevice,
                IsDeleteJob = info.IsDeleteJob
            };
            return result;
        }


        
        private JMArchiverRententionJobDetails GenerateFileLevelRetentionDetail(string extraInfo, string url, long fileSize, JobDetailsStatus status,string action, string comment = "",string storageName = "")
        {
            var report = new JMArchiverRententionJobDetails
            {
                SiteUrl = extraInfo.IsNotNullOrEmpty() ? url+ ":" + extraInfo : url,
                Size = fileSize.ToString(),
                Status = status,
                JobId = _archiverRetentionInfo.JobId,
                Comment = comment,
                Action = action,
                SrcStorageName = storageName
            };
            return report;
        }
        private void AddRetentionToReport(GoogleBasicIndex fileIndex, string fileName, long fileSize, JobDetailsStatus status,string action,string storageName = "")
        {
            try
            {
                var realReport = GenerateFileLevelRetentionDetail(fileIndex.VersionNumber, fileIndex.Path, fileSize, status, action, "",storageName);
                this.AddToReport(realReport);
            }
            catch (Exception e)
            {
                _logger.Error($"Add retention to report failed,itemname:{fileName}.error:{e}");
            }
        }
        
        private void AddMoveRetentionReport(GoogleBasicIndex fileIndex,JobDetailsStatus status)
        {
            try
            {
                var versionNumber = fileIndex.VersionNumber;
                var url = fileIndex.Path;
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = versionNumber.IsNotNullOrEmpty() ? url+ ":" + versionNumber : url,
                    Size = "0",
                    Status = status,
                    SrcStorageName = _archiverRetentionInfo.DataLogicalDevice.Name,
                    DesStorageName = _archiverRetentionInfo.DestinationDevice?.Name,
                    JobId = _archiverRetentionInfo.JobId,
                    Action = "RM_PRM_PRE_Move"
                };
                this.AddToReport(report);
            }
            catch (Exception e)
            {
                _logger.Error($"Add move retention to report failed,itemname:error:{e}");
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
                    foreach (var r in _blobMappings)
                    {
                        using (new CheckJobStopScope()) { }
                        if (!r.Value.AlreadyRehydration)
                        {
                            var file = this._dataLogicalDevice.OpenFile(r.Value.MappedBlobInfo);
                            if (file is AzureCloudInfo)
                            {
                                var azureFile = file as AzureCloudInfo;
                                if (!azureFile.Exists || azureFile.FileTierType == AccessTierType.Archive)
                                {
                                    _logger.Info($"The {r.Key} need to rehydration, " +
                                        $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                        $"Exists:{azureFile.Exists} , " +
                                        $"start time : {r.Value.StartTime.ToString()}");
                                    needContinueSleep = true;
                                    break;
                                }
                                else
                                {
                                    _logger.Info($"The {r.Key} already rehydration, " +
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
                        _logger.Info("Will sleep 15 min to wait blob rehydration.");
                        Thread.Sleep(15 * 60 * 1000);
                    }
                    else
                    {
                        _logger.Info($"Exit waiting blob rehydration, all the datas rehydration : {!needContinueSleep} .");
                        break;
                    }
                }
            }
            catch (JobStopException e)
            {
                _logger.Warn("Job will stop,stop Rehydration.");
                throw;
            }
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
                        _logger.Warn("An error occurred while setting file Archive. FileName: {0}", storageInfo.LowName);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("An error occurred while setting file Archive. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
            }
        }
        
        private void VerifyAndCopyArchiverToHot(StorageInfo info)
        {
            var file = this._dataLogicalDevice.OpenFile(info);

            if (file is AzureCloudInfo)
            {
                var azureFile = file as AzureCloudInfo;
                if (file != null && azureFile.FileTierType == AccessTierType.Archive)
                {
                    string temp = Path.Combine(rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + 11));
                    lock (_rehydrationLock)
                    {
                        if (!_blobMappings.ContainsKey(info.HighPlusLowName))
                        {
                            azureFile.FileTierType = AccessTierType.Archive;
                            AzureCloudInfo info2 = new AzureCloudInfo { HighName = temp, LowName = info.LowName, FileTierType = AccessTierType.Hot };
                            StorageCopyResult res = new StorageCopyResult();
                            if (this._dataLogicalDevice is XLibrary)
                            {
                                try
                                {
                                    if ((this._dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID.EqualsIgnoreCase(DEFAULTSTORAGEID))
                                    {
                                    string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.DefaultStorage);
                                    var client = Util.MSAzure.StorageUtil.GetContainerClient(defaultConnectionString, TenantLocalValue.LogonGroupId);
                                        BlobCopyFromUriOptions opt = new BlobCopyFromUriOptions
                                        {
                                            AccessTier = AccessTier.Hot
                                        };
                                        res.IsCopyed = true;
                                    }
                                    else
                                    {
                                        res = this._dataLogicalDevice.CopyFile(azureFile, info2, true);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error($"some thing went wrong when copy file,storage id:{(this._dataLogicalDevice as XLibrary).GetWorkingSystem().SystemID}");
                                    res = this._dataLogicalDevice.CopyFile(azureFile, info2, true);
                                }
                            }
                            else
                            {
                                res = this._dataLogicalDevice.CopyFile(azureFile, info2, true);
                            }
                            if (res.IsCopyed)
                            {
                                BLOBRehydrationMapping mapping = new()
                                {
                                    AlreadyRehydration = false,
                                    MappedBlobInfo = info2,
                                    StartTime = DateTime.Now
                                };
                                _blobMappings.Add(info.HighPlusLowName, mapping);
                            }
                        }
                    }
                }
            }
        }

        

        private Int64 DeleteDataFromDevice(String dataVolume, String indexVolume, Boolean NeedDeleteSubIndex,bool isFitSoftDeleteAndRetainByModifedTime = false,bool isDeleteAction = true)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            String stubType = string.Empty;
            var tempDeleteDataSize = default(Int64);
            StorageDeleteResult deleteDataResult = new();
            StorageDeleteResult deleteIndexResult = new();
            // *** 对于StorageInterfaceType.Object 类型的Device不支持Dedup，如果支持还需要进一步完善 ***
            if (this._dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object))
            {
                _logger.Warn("this device storage InterfaceType is Object,it may wrong");
                var storageInfoList = this._retentionIndexService.GetStorageInfosByJobId(this._archiverRetentionInfo.JobId);
                tempDeleteDataSize = this.DeleteFileByStorageInfo(storageInfoList);
                if (this._archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                {
                    var retentionInfoList = this._retentionIndexService.GetDeleteDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, this._archiverRetentionInfo.SiteUrl);
                    this._retentionIndexService.DeleteDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                UpdateArchivedInfo(_archiverRetentionInfo.FarmName, _archiverRetentionInfo.SiteUrl);
                UpdateRetentionInfo(retentionInfoList);
                }
            }
            else
            {
                if (this._archiverRetentionInfo.IsFileLevelBlockBackup)
                {
                    List<GoogleBasicIndex> deletingIndexes = null;
                    if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        NeedDeleteSubIndex = false;
                        deletingIndexes = this._retentionIndexService.GetDeletingIndexesByModifiedTime(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, SoftTimeConvert(this._archiverRetentionInfo.DateTimeNow, this._archiverRetentionInfo.SoftDeleteKeepValue, this._archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                    }
                    else if(this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ArchiveTime) 
                    {
                        deletingIndexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                        DeleteMetaBlocks(this._archiverRetentionInfo.JobId, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                    }
                    else
                    {
                        throw new Exception($"Unsupported retain data type: {this._archiverRetentionInfo.RetentionDataTimeType}");
                    }
                    long contentSize = 0;
                    if (deletingIndexes is { Count: > 0 })
                    {
                        foreach (var deletingIdx in deletingIndexes)
                        {
                            var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                            _logger.Info($"Start to delete device content: {info.HighPlusLowName}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                            try
                            {
                                deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                                //ChangeLorealBlobFromPreviousVersionToDelete(info);
                                var delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                tempDeleteDataSize += delSize;
                                if (delSize == 0)
                                {
                                    delSize = deletingIdx.ContentLength;
                                }
                                if (isDeleteAction)
                                {
                                    AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", _archiverRetentionInfo.DataLogicalDevice.Name);
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
                                        AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_JS_Common_Delete", _archiverRetentionInfo.DataLogicalDevice.Name);
                                    }
                                    else
                                    {
                                        AddMoveRetentionReport(deletingIdx, JobDetailsStatus.Successful);
                                    }
                                    _errorMessage = ex.Message;
                                    _jobStatusInfo.State = 3;
                                    _logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                    throw;
                                }
                                else
                                {
                                    _jobStatusInfo.State = 7;
                                    _logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                                }
                                _logger.Info($"Update media size success,job id:{_archiverRetentionInfo.JobId},size:{tempDeleteDataSize}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Info($"No file need to delete, job id:{this._archiverRetentionInfo.JobId}");
                    }

                    if (tempDeleteDataSize > 0)
                    {
                        ArchiverIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeAsync(this._archiverRetentionInfo.JobId, tempDeleteDataSize);
                    }
                    else
                    {
                        tempDeleteDataSize = contentSize;
                    }
                }
                else
                {
                    var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                    var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this._archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
                    _logger.Info($"Need delete blobs count : {fileList.Count}");
                    fileList.ForEach(item =>
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        try
                        {
                            deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                            //ChangeLorealBlobFromPreviousVersionToDelete(info);
                            isDeleteSucceedAtLeastOnce = true;
                            tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                        }
                        catch (Exception ex)
                        {
                            if (!isDeleteSucceedAtLeastOnce)
                            {
                                this._errorMessage = ex.Message;
                                this._jobStatusInfo.State = 3;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                throw;
                            }
                            else
                            {
                                this._jobStatusInfo.State = 7;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            }
                        }
                    });
                }

                if (this._archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
                {
                    _logger.Info($"Current job id is {this._archiverRetentionInfo.RetentionJob.Id}");
                    var retentionInfoList = this._retentionIndexService.GetDeleteDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, this._archiverRetentionInfo.SiteUrl);
                    if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                    {
                        this._retentionIndexService.DeleteDataFromMainIndexByDateTime(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, SoftTimeConvert(this._archiverRetentionInfo.DateTimeNow, this._archiverRetentionInfo.SoftDeleteKeepValue, this._archiverRetentionInfo.SoftDeleteDateUnit, isFitSoftDeleteAndRetainByModifedTime), isFitSoftDeleteAndRetainByModifedTime);
                    }
                    else
                    {
                        if (!this._archiverRetentionInfo.IsFileLevelBlockBackup)
                        {
                            var deletingIndexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                            if (deletingIndexes is { Count: > 0 })
                            {
                                foreach (var deletingIdx in deletingIndexes)
                                {
                                    var delSize = deletingIdx.ContentLength;
                                    AddRetentionToReport(deletingIdx, "", delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", _archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                            }
                        }
                        this._retentionIndexService.DeleteDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                    }

                    UpdateRetentionInfo(retentionInfoList);
                    UpdateArchivedInfo(_archiverRetentionInfo.FarmName,_archiverRetentionInfo.SiteUrl);
                }
                if (this._archiverRetentionInfo.RetentionRule == RetentionRule.MoveArchiverJobData)
                {
                    var deletingIndexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
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
                    indexVolume, this._archiverRetentionInfo.JobId + "_" + ServiceConstants.IndexDBName);
                storageInfo.ExtraStorageInfo = _archiverRetentionInfo.SubIndexStorageInfo;
                try
                {
                    deleteIndexResult = this._indexLogicalDevice.DeleteFile(storageInfo);

                    if (deleteIndexResult.DeletedFileSize > 0)
                    {
                        tempDeleteDataSize += deleteIndexResult.DeletedFileSize;
                    }
                }
                catch (Exception ex)
                {
                    this._jobStatusInfo.State = 7;
                    this._logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, storageInfo.LowName, ex.ToString());
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
                _logger.Info($"ValidatesoftTime.RetentionTime2 {retentionTime.Ticks}");
                return retentionTime.Ticks;
            }
            else
            {
                return retentionTimeTicks;
            }
        }
        private void UpdateArchivedInfo(string driveName, string driveId)
        {
            long fileCount = this._retentionIndexService.GetFileNumber();
            long fileVersionCount = this._retentionIndexService.GetFileVersionNumber();
            var googleTenantId = RemoteNodeDao.GetTenantIdByObjectId(driveId);
            this._logger.Info($"file count is:{fileCount},version count is:{fileVersionCount}");
            ArchiveGDriveInfoDao.UpdateGoogleArchiverInfo(driveName, fileCount, fileVersionCount, googleTenantId, driveId);
        }
        
        private Int64 SoftDelete(String dataVolume, String indexVolume)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            bool isRealDeleteDatas = IsEnabledRealDelete(); 
            var tempDeleteDataSize = default(Int64);
            StorageDeleteResult deleteDataResult = new StorageDeleteResult();

            if (this._archiverRetentionInfo.IsFileLevelBlockBackup)
            {
                List<GoogleBasicIndex> deletingIndexes = null;
                if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    deletingIndexes = _retentionIndexService.GetDeletingIndexesByModifiedTime(_archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, this._archiverRetentionInfo.DateTimeNow, false);
                }
                else if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ArchiveTime)
                {
                    deletingIndexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                    if (isRealDeleteDatas)
                    {
                        _logger.Info("this job is soft and is real delete,delete meta blocks");
                        DeleteMetaBlocks(this._archiverRetentionInfo.JobId, ref tempDeleteDataSize, ref isDeleteSucceedAtLeastOnce);
                    }
                }
                else
                {
                    throw new Exception($"Unsupported retain data type: {this._archiverRetentionInfo.RetentionDataTimeType}");
                }

                if (deletingIndexes is { Count: > 0 })
                {
                    foreach (var deletingIdx in deletingIndexes)
                    {
                        var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                        _logger.Info($"Start to delete device content: {info.HighPlusLowName}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                        try
                        {
                            long delSize = 0;
                            if (isRealDeleteDatas)
                            {
                                deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                                //ChangeLorealBlobFromPreviousVersionToDelete(info);
                                if (deletingIdx.RetentionStatus == (int)AvePoint.GCommon.Contract.CommonFilter.FilterDeletedType.Soft && deleteDataResult.IsDeleted && deleteDataResult.DeletedFileSize < 0)
                                {
                                    _logger.Info($"this data has soft deleted,no need to add report again,name:{info.LowName}");
                                }
                                else
                                {
                                    delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                    if (delSize == 0)
                                    {
                                        delSize = deletingIdx.ContentLength;
                                    }
                                    AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_SoftDelete", _archiverRetentionInfo.DataLogicalDevice.Name);
                                }
                                isDeleteSucceedAtLeastOnce = true;

                            }
                            else
                            {
                                if (deletingIdx.RetentionStatus == (int)AvePoint.GCommon.Contract.CommonFilter.FilterDeletedType.Soft)
                                {
                                    _logger.Info($"1this data has soft deleted,no need to add report again,name:{info.LowName}");
                                }
                                else
                                {
                                    AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_SoftDelete", _archiverRetentionInfo.DataLogicalDevice.Name);
                                    isDeleteSucceedAtLeastOnce = true;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            if (!isDeleteSucceedAtLeastOnce)
                            {
                                AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_AR_CP_GSS_Retention_SoftDelete");
                                this._errorMessage = ex.Message;
                                this._jobStatusInfo.State = 3;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                throw;
                            }
                            else
                            {
                                this._jobStatusInfo.State = 7;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            }
                            _logger.Info($"Update media size success,job id:{this._archiverRetentionInfo.JobId},size:{tempDeleteDataSize}");
                        }
                    }
                }
                else
                {
                    _logger.Info($"No file need to delete, job id:{this._archiverRetentionInfo.JobId}");
                }
            }
            else
            {
                var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(this._archiverRetentionInfo.JobId, StringComparison.OrdinalIgnoreCase));
                _logger.Info($"Need delete blobs count : {fileList.Count}");
                if (isRealDeleteDatas)
                {
                    fileList.ForEach(item =>
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        //info.Length = this.dataLogicalDevice.OpenFile(info).FileSize;//for cloud
                        try
                        {
                            deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                            //ChangeLorealBlobFromPreviousVersionToDelete(info);
                            tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                            isDeleteSucceedAtLeastOnce = true;
                        }
                        catch (Exception ex)
                        {
                            if (!isDeleteSucceedAtLeastOnce)
                            {
                                this._errorMessage = ex.Message;
                                this._jobStatusInfo.State = 3;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                                throw;
                            }
                            else
                            {
                                this._jobStatusInfo.State = 7;
                                this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            }
                        }
                    });
                }
                else
                {
                    _logger.Info("not real delete datas,will just mark as soft delete");
                }
            }

            if (this._archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
            {
                _logger.Info($"Current job id is {this._archiverRetentionInfo.RetentionJob.Id}");
                var retentionInfoList = this._retentionIndexService.GetDeleteDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, this._archiverRetentionInfo.SiteUrl);
                if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    this._retentionIndexService.UpdateAsSoftDeleteByDateTime(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId, this._archiverRetentionInfo.DateTimeNow);
                }
                else
                {
                    if (!_archiverRetentionInfo.IsFileLevelBlockBackup)
                    {
                        var deletingIndexes = this._retentionIndexService.GetDeletingDataFromMainIndex(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                        if (deletingIndexes != null && deletingIndexes.Count > 0)
                        {
                            foreach (var deletingIdx in deletingIndexes)
                            {
                                var delSize = deletingIdx.ContentLength;
                                AddRetentionToReport(deletingIdx, "", delSize, JobDetailsStatus.Successful, "RM_AR_CP_GSS_Retention_SoftDelete", _archiverRetentionInfo.DataLogicalDevice.Name);
                            }
                        }
                    }
                    this._retentionIndexService.UpdateAsSoftDelete(this._archiverRetentionInfo.StoragePolicyId, this._archiverRetentionInfo.JobId);
                }
                UpdateArchivedInfo(this._archiverRetentionInfo.FarmName,this._archiverRetentionInfo.SiteUrl);
                UpdateRetentionInfo(retentionInfoList);
            }
            try
            {
                if (this._archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    var parameters = new Dictionary<String, Object>();
                    parameters["@storagePolicyId"] = this._archiverRetentionInfo.StoragePolicyId;
                    parameters["@jobId"] = this._archiverRetentionInfo.JobId;
                    parameters["@dateTime"] = this._archiverRetentionInfo.DateTimeNow;
                    parameters["@timeNow"] = DateTime.UtcNow.Ticks.ToString();
                    var deleteBodyTable = "update " + IndexConstants.TableNameGDriveItem + " set COL_RETENTION_STATUS = 1,COL_SOFT_DELETE_TIME = @timeNow where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_MODIFY_TIME<@dateTime and COL_RETENTION_STATUS = 0";
                    IndexSubProcessor.Execute(deleteBodyTable, parameters);
                }
                else
                {
                    var parameters = new Dictionary<String, Object>();
                    parameters["@storagePolicyId"] = this._archiverRetentionInfo.StoragePolicyId;
                    parameters["@jobId"] = this._archiverRetentionInfo.JobId;
                    var deleteBodyTable = "update " + IndexConstants.TableNameGDriveItem + " set COL_RETENTION_STATUS = 1 where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
                    this.IndexSubProcessor.Execute(deleteBodyTable, parameters);
                }
            }
            catch (Exception ex)
            {
                this._jobStatusInfo.State = 7;
                this._logger.Warn($"soft delete failed when mark sub index,error:{ex}");
            }

            return tempDeleteDataSize;
        }

        private void DeleteMetaBlocks(string jobId, ref long tempDeleteDataSize, ref bool isDeleteSucceedAtLeastOnce)
        {
            try
            {
                var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(_dataVolume, null));
                string metaFilePrefix = $"{jobId}_meta_";
                var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(metaFilePrefix, StringComparison.OrdinalIgnoreCase));
                _logger.Info($"Need delete meta blocks count : {fileList.Count}");
                StorageDeleteResult deleteDataResult;
                foreach(var item in fileList)
                {
                    var info = XConvert.FromNames(item.HighName, item.Name);
                    try
                    {
                        deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                        //ChangeLorealBlobFromPreviousVersionToDelete(info);

                        isDeleteSucceedAtLeastOnce = true;
                        tempDeleteDataSize += Math.Max(deleteDataResult.DeletedFileSize, 0);
                    }
                    catch (Exception ex)
                    {
                        if (!isDeleteSucceedAtLeastOnce)
                        {
                            this._errorMessage = ex.Message;
                            this._jobStatusInfo.State = 3;
                            this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                            throw;
                        }
                        else
                        {
                            this._jobStatusInfo.State = 7;
                            this._logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._jobStatusInfo.State = 7;
                this._logger.Error($"Error occurred while deleting meta blocks: {jobId}. {ex}");
            }
        }

        private void UpdateRetentionInfo(List<KeyValuePair<string, long>> retentionInfoList)
        {
            foreach (var info in retentionInfoList)
            {
                _logger.Info($"Retention file info:{info.Value}, site URL: {this._archiverRetentionInfo.SiteUrl}, list URL:{info.Key}, archiver job: {this._archiverRetentionInfo.JobId}");
                var retentionDriveInfo = new RMRetentionGDriveInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    ContainerId = info.Key,
                    DriveId = this._archiverRetentionInfo.SiteUrl,
                    RetentionJobID = this._archiverRetentionInfo.RetentionJob.Id,
                    FileNumber = info.Value
                };
                ArchiveGDriveInfoDao.SaveRetentionDriveInfo(retentionDriveInfo);
            }
        }
        
        private void ChangeLorealBlobFromPreviousVersionToDelete(StorageInfo info)
        {
            if (isLorealSoftDelete)
            {
                var source = _dataLogicalDevice as AbstractXSystem;
                if (source != null && source.StorageType == XStorageType.Azure)
                {
                    if (_sourceContainerClient == null)
                    {
                        _sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                    }
                    string blobName = info.HighPlusLowName.Replace(@"\", @"/");
                    _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {info.HighPlusLowName}.blobName:{blobName}.");
                    var blobClient = _sourceContainerClient.GetBlobClient(blobName);
                    // List all versions of the blob
                    List<string> blobVersions = new List<string>();
                    foreach (BlobItem blobItem in _sourceContainerClient.GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blobName, default))
                    {
                        _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {blobItem.Name}, Version ID: {blobItem.VersionId}.Version Delete:{blobItem.Deleted}.");
                        blobVersions.Add(blobItem.VersionId);
                    }
                    foreach (var blobVersion in blobVersions)
                    {
                        blobClient.WithVersion(blobVersion).DeleteIfExistsAsync();
                        _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Success delete blob version.Version ID: {blobVersion}.");
                    }
                }
                else
                {
                    throw new FileNotFoundException(String.Format("3An error occurred in getting file {0} size in {1}.", info.HighPlusLowName, _dataVolume));
                }
            }
        }
        
        private bool IsEnabledRealDelete()
        {
            return false;
            var realDeleteRetentionDatas = RMKeyValueDao.GetValueByKey("RealDeleteAzureRetentionDatas");
            if (realDeleteRetentionDatas != null)
            {
                bool result;
                if (bool.TryParse(realDeleteRetentionDatas.Value, out result) && result)
                {
                    string storageId = string.IsNullOrEmpty(_archiverRetentionInfo.CurrentStorageId) ? _archiverRetentionInfo.StoragePolicyId : _archiverRetentionInfo.CurrentStorageId;
                    if (string.Equals(storageId, RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warn("this storage is avepoint storage that can not delete datas when the action is soft delete");
                        return false;
                    }
                    else
                    {
                        var storageInfo = StorageDeviceDao.GetStorageDevicesById(new Guid(storageId));
                        if (storageInfo != null && storageInfo.Type == (int)StorageDeviceType.CloudAzure)
                        {
                            _logger.Info($"this storage is azure storage and soft delete,will real delete datas");
                            return true;
                        }
                        else
                        {
                            _logger.Info($"this storage is not azure storage,so skip delete datas when soft delete,storage id:{storageId},type:{storageInfo?.Type}");
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
    }
