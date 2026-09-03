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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service;
using AvePoint.Media.Storage;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using RAFileSystem.FileSystem.FileSystem.Restore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Storage.Util;
using AvePoint.GCommon;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using RAFileSystem.FileSystem.Common;
using AvePoint.RA.Common.Hybrid;
using RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Storage;
using System.IO;
using System.Threading;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Common;
using Azure.Storage.Blobs.Models;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Configurations;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Util;
using Microsoft.Extensions.Azure;
using NVelocity.Util.Introspection;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using static AvePoint.GCommon.Utility.I18N.EventIds.SharePoint;
using AvePoint.GCommon.Utility;

namespace RAFileSystem.FileSystem.FileSystem.Retain
{
    public class FSRetainActionWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        private static string CACHE_FODER_PATH = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, "RetentionCache");
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

       public IArchiverRetentionIndexService RetentionIndexService = new ArchiverRetentionIndexService();
        private string containerName;
        public ICacheService CacheManager = new CacheService();
        public ArchiverIndexService _ArchiverIndexService = new ArchiverIndexService();
        //public IArchiverBackupIndexService RetentionIndexService = new ArchiverBackupIndexService();
        private int CurrentProgress { get; set; }
        private bool isMoveToAvepointStorage;
        private bool isArchiveTierToColdTier;
        public IStorageDeviceManager DeviceManager = new StorageDeviceManager();
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private bool dataLogicalDeviceIsAzure;
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
        public FSRetainActionWorker(IReportService<JMJobDetails> JobDetailService)
        { 
            this.JobDetailService = JobDetailService;
        }
        public ArchiverRetentionResult InternalRetain(ArchiverRetentionInfo retentionInfo)
        {
            //var jobState = 2; //2 stand for job successful
            ArchiverRetentionResult result;
            try
            {
                this.Open(retentionInfo);
                result = this.Retain(retentionInfo);
                if (result.State == 2)
                {
                    FSJobCache.RestoreInstance.SuccessCount++;
                }
            }
            catch (Exception e)
            {
                FSJobCache.RestoreInstance.FailedCount++;
                throw;
            }
            finally
            {
                this.Close();
            }
            return result;
        }
        public void Close()
        {
            if (!archiverRetentionInfo.IsSimulateJob)
            {
                this.UploadIndexToRealSystem();
            }
            //if (this.IndexService != null && this.archiverRetentionInfo.RetentionRule.Equals(RetentionRule.RetainArchiverJobData))
            //{
            //    this.IndexService.Close();
            //}
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
        private void UploadIndexToRealSystem()
        {
            if (_ArchiverIndexService.IndexProcessor != null)
            {
                _ArchiverIndexService.IndexProcessor.Close();
            }
            var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
            _ArchiverIndexService.IndexSynchronizer.Upload(dbInfo);
        }
        public ArchiverRetentionResult Retain(ArchiverRetentionInfo retentionInfo)
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
                    throw new Exception(String.Format($"RetentionServiceRetainUnknownFileTypeException:{this.archiverRetentionInfo.RetentionRule.ToString()}"));
            }
            return retentionResult;
        }
        private ArchiverRetentionResult MoveJobData()
        {
            var result = new ArchiverRetentionResult();
            if (isMoveToAvepointStorage)
            {
                result.State = 7;//no need to add job details
                return result;
            }
            this.logger.Info($"Retention Service Move Job Data Begin.this.archiverRetentionInfo.JobId:{this.archiverRetentionInfo.JobId}");
            this.deleteDataSize = this.MoveDataFromDevice(this.dataVolume, this.indexVolume);
            this.logger.Info($"Retention Service Move Job Data Finished.this.archiverRetentionInfo.JobId:{this.archiverRetentionInfo.JobId},this.deleteDataSize.ToString():{this.deleteDataSize.ToString()}");
            result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            result.Size = deleteDataSize;
            result.State = 2;
            return result;
        }
        private void VerifyAndCopyArchiverToHot(StorageInfo info)
        {
            if (dataLogicalDeviceIsAzure)
            {
                //var file = this.dataLogicalDevice.OpenFile(info);
                var blob = GetBlobAccessTier(info.HighName + "\\" +info.LowName);
                if (blob.GetProperties().Value.AccessTier == AccessTier.Archive)
                {
                    string temp = Path.Combine(rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + 11));
                    lock (rehydrationLock)
                    {
                        if (!BLOBMappings.ContainsKey(info.HighPlusLowName))
                        {
                            SetSelectToTier(AccessTierType.Hot, info);
                            BLOBRehydrationMapping mapping = new BLOBRehydrationMapping()
                            {
                                AlreadyRehydration = false,
                                MappedBlobInfo = info,
                                StartTime = DateTime.Now
                            };
                            BLOBMappings.Add(info.HighPlusLowName, mapping);
                        }
                    }
                }
            }
        }

        private long SimulateDeleteDatas(StorageInfo info)
        {
            return this.dataLogicalDevice.OpenFile(info).FileSize;
        }

        private long DeleteDatas(StorageInfo info)
        {
            if (archiverRetentionInfo.IsSimulateJob)
            {
                return SimulateDeleteDatas(info);
            }
            if (dataLogicalDeviceIsAzure)
            {
                //var file = this.dataLogicalDevice.OpenFile(info);
                var blob = GetBlobAccessTier(info.HighName + "\\" + info.LowName);
                if (blob.GetProperties().Value.AccessTier == AccessTier.Archive)
                {
                    lock (rehydrationLock)
                    {
                        long blobSize = blob.GetProperties().Value.ContentLength;
                        logger.Info($"Azure DeleteDatas Begin,info.LowName:{info.LowName.LogBase64()},blobSize:{blobSize}");
                        DeleteArchiveTierDatasInternal(info);
                        return blobSize;
                    }
                }
                else
                {
                    logger.Info($"Azure DeleteDatas Begin,info.LowName:{info.LowName.LogBase64()}");
                    var deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                    return deleteDataResult.DeletedFileSize;
                }
            }
            else
            {
                logger.Info($"not Azure storage DeleteDatas Begin,info.LowName:{info.LowName.LogBase64()}");
                var deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                return deleteDataResult.DeletedFileSize;
            }
        }
        private void InitAzureStorageInfo(string xriString)
        {
            if (dataLogicalDeviceIsAzure)
            {
                sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(xriString);
            }
        }

        private void DeleteArchiveTierDatasInternal(StorageInfo info)
        {
            logger.Info($"DeleteArchiveTierDatasInternal Begin,info.LowName:{info.LowName.LogBase64()},dataLogicalDeviceIsAzure:{dataLogicalDeviceIsAzure}");
            if (dataLogicalDeviceIsAzure)
            {
                if (sourceContainerClient != null)
                {
                    var blobClient = sourceContainerClient.GetBlobClient(info.HighName + "\\" + info.LowName);
                    if (blobClient.Exists())
                    {
                        var result = blobClient.Delete();
                        logger.Info($"delete archive tier info.LowName:{info.LowName.LogBase64()}");
                    }
                    else
                    {
                        logger.Warn($"The blob {info.LowName.LogBase64()} does not exist1.");
                    }
                }
            }
            logger.Info($"DeleteArchiveTierDatasInternal End,info.LowName:{info.LowName.LogBase64()},dataLogicalDeviceIsAzure:{dataLogicalDeviceIsAzure}");
        }
        private void SetSelectToTier(AccessTierType tierType,StorageInfo info)
        {
            logger.Info($"SetSelectToTier Begin,info.LowName:{info.LowName.LogBase64()},tierType:{tierType},dataLogicalDeviceIsAzure:{dataLogicalDeviceIsAzure}");
            if (dataLogicalDeviceIsAzure)
            {
                if (sourceContainerClient != null)
                {
                    var blobClient = sourceContainerClient.GetBlobClient(info.HighName + "\\"+info.LowName);
                    if (blobClient.Exists())
                    {
                        switch (tierType)
                        {
                            case AccessTierType.Cool:
                                blobClient.SetAccessTierAsync(AccessTier.Cool).GetAwaiter().GetResult();
                                break;
                            case AccessTierType.Hot:
                                blobClient.SetAccessTierAsync(AccessTier.Hot).GetAwaiter().GetResult();
                                break;
                            case AccessTierType.Archive:
                                blobClient.SetAccessTierAsync(AccessTier.Archive).GetAwaiter().GetResult();
                                break;
                            case AccessTierType.Cold:
                                blobClient.SetAccessTierAsync(AccessTier.Cold).GetAwaiter().GetResult();
                                break;
                        }
                    }
                    else
                    {
                        logger.Warn($"The blob {info.LowName.LogBase64()} does not exist.");
                    }
                }
            }
            logger.Info($"SetSelectToTier End,info.LowName:{info.LowName.LogBase64()},tierType:{tierType},dataLogicalDeviceIsAzure:{dataLogicalDeviceIsAzure}");
        }
        private BlobClient GetBlobAccessTier(string blobName)
        {
            if (dataLogicalDeviceIsAzure)
            {
                if (sourceContainerClient != null)
                {
                    var blobClient = sourceContainerClient.GetBlobClient(blobName);
                    return blobClient;
                }
            }
            return null;
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
                if (sourceDevice.SupportedFileType == FileBlockType.SingleInstanceLevel_File
                   && destinationDevice.SupportedFileType == FileBlockType.SingleInstanceLevel_File && !item.Name.Contains("meta"))
                {
                    Int64 contentFileNumber = this.GetContentFileNumber(item.Name);
                    this.logger.Info($"Archiver Backup Retention Service MoveAndDeleteFile From Device Info.retentionInfo.ToString():{retentionInfo.ToString().LogBase64()},contentFileNumber:{contentFileNumber}");
                    info.MetaInfos["OriginalFileName"] = this.RetentionIndexService.GetItemName(contentFileNumber, retentionInfo.JobId);
                }
                info.MetaInfos["Archive-SiteCollectionName"] = retentionInfo.ConnectionId;
                //info.MetaInfos["Archive-PlanId"] = retentionInfo.PlanId;
                info.MetaInfos["Archive-JobId"] = retentionInfo.JobId;
                Int64 dataMode = (int)DataSecurity.EncryptionMedia;
                info.MetaInfos["Archive-DataMode"] = Convert.ToString(dataMode);
                this.logger.Info($"Archiver Backup Retention Service MoveAndDelete File From Device Data Mode.dataMode:{dataMode}");
                info.Length = sourceDevice.OpenFile(info).FileSize;//for cloud
                StorageResult storageResult = null;

                if (BLOBMappings.ContainsKey(info.HighPlusLowName))
                {
                    StorageInfo sourceInfo = BLOBMappings[info.HighPlusLowName].MappedBlobInfo;
                    storageResult = RealMove(sourceInfo, sourceDevice, info, destinationDevice);
                }
                else
                {
                    storageResult = RealMove(info, sourceDevice, info, destinationDevice);
                }
                if (destinationDevice.Type == ServiceConstants.AzureSystem && destinationStoreInArchiverTier)
                {
                    //SetFileTierArchive(destinationDevice, info);
                }
            });

            return this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, false,false,false);
        }

        //private void SetFileTierArchive(IXSystem destinationDevice, StorageInfo storageInfo)
        //{
        //    try
        //    {
        //        if (destinationDevice.Type == ServiceConstants.AzureSystem)
        //        {
        //            var device = destinationDevice as IAzureSystem;
        //            AzureCloudInfo info = (AzureCloudInfo)storageInfo;
        //            info.FileTierType = AccessTierType.Archive;
        //            var result = device.ChangeFileTierAsync(info).GetAwaiter().GetResult();
        //            if (!result.IsChanged)
        //                logger.Warn("An error occurred while setting file Archive. FileName: {0}", storageInfo.LowName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("An error occurred while setting file Archive. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
        //    }
        //}

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
                            var blobClient = GetBlobAccessTier(r.Value.MappedBlobInfo.HighName + "\\" + r.Value.MappedBlobInfo.LowName);
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

        private long GetContentFileNumber(String name)
        {
            return Convert.ToInt64(name.Substring(name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1, name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) - name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 1));
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
                try
                {

                    var blob = GetBlobAccessTier(info.HighName + "\\" + info.LowName);
                    var fileTier = blob.GetProperties().Value.AccessTier;
                    if (this.accessTierType == AccessTierType.Cold && fileTier == AccessTier.Archive)
                    {
                        isArchiveTierToColdTier = true;
                    }
                    if (this.accessTierType == AccessTierType.Archive && fileTier == AccessTier.Archive)
                    {
                        logger.Info($"will not mark tier,info.LowName:{info.LowName.LogBase64()},fileTier:{fileTier.ToString().LogBase64()},accessTierType:{this.accessTierType.ToString()}");
                    }
                    else
                    {
                        SetSelectToTier(this.accessTierType, info);
                    }
                    isMarkSucceedAtLeastOnce = true;
                }
                catch (Exception ex)
                {
                    if (!isMarkSucceedAtLeastOnce)
                    {
                        this.ErrorMessage = ex.Message;
                        this.jobStatusInfo.State = 3;
                        this.logger.Error($"mark data tier failed,{info.LowName.LogBase64()} error:{ex.ToString()}");
                        throw;
                    }
                    else
                    {
                        this.jobStatusInfo.State = 7;
                        this.logger.Error($"mark data tier all failed,{info.LowName.LogBase64()} error:{ex.ToString()}");
                    }
                }
            });
        }
        //private void SetFileTierArchiveAsync(IXSystem destinationDevice, StorageInfo storageInfo, XFileInfo file)
        //{
        //    try
        //    {
        //        if (destinationDevice.Type == ServiceConstants.AzureSystem)
        //        {
        //            if (file is AzureCloudInfo)
        //            {
        //                var tempFile = file as AzureCloudInfo;

        //                if (this.accessTierType != tempFile.FileTierType)
        //                {
        //                    var device = destinationDevice as IAzureSystem;
        //                    AzureCloudInfo info = new AzureCloudInfo();
        //                    info.HighName = storageInfo.HighName;
        //                    info.LowName = storageInfo.LowName;
        //                    info.FileTierType = this.accessTierType;
        //                    var result = device.ChangeFileTierAsync(info);
        //                    if (!result.IsChanged)
        //                        logger.Warn("An error occurred while setting file tier. FileName: {0}", storageInfo.LowName);
        //                }
        //                else
        //                {
        //                    logger.Info($"will not mark tier,tempFile.tier:{tempFile?.FileTierType.ToString()},accessTierType:{this.accessTierType}. FileName: {storageInfo.LowName}");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("RM_MR_MarkTier_ErrorMessage");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("An error occurred while setting file tier. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
        //        throw;
        //    }
        //}


        private StorageResult RealMove(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice)
        {
            StorageResult storageResult = null;
            string fileName = CACHE_FODER_PATH + Path.DirectorySeparatorChar + DateTime.UtcNow.Ticks;
            byte[] buffer = new byte[64 * 1024];
            try
            {
                if (!Directory.Exists(CACHE_FODER_PATH))
                {
                    Directory.CreateDirectory(CACHE_FODER_PATH);
                }
                using (var sourceStream = sourceDevice.OpenStream(sourceInfo, FileMode.Open))
                {
                    using (var tempFile = new FileStream(
                    fileName,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    buffer.Length,
                    FileOptions.WriteThrough))
                    {
                        int bytesRead = 0;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            tempFile.Write(buffer, 0, bytesRead);
                        }
                        tempFile.Flush(true);
                    }
                }
                using (Stream cacheStream = File.OpenRead(fileName))
                {
                    storageResult = destinationDevice.CommitStream(cacheStream, destinationInfo);
                }
            }
            catch (Exception ex)
            {
                storageResult = null;
                this.logger.Error($"Retention Service Real Move Invalid Device.ex:{ex}");
                throw;
            }
            finally
            {
                FileUtility.TryDelete(fileName);
            }
            return storageResult;
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
       
        private Int64 DeleteDataFromDevice(String dataVolume, String indexVolume, Boolean NeedDeleteSubIndex, bool isFitSoftDeleteAndRetainByModifedTime = false,bool needToAddDetail = true)
        {
            Boolean isDeleteSucceedAtLeastOnce = false;
            String stubType = string.Empty;
            var tempDeleteDataSize = default(Int64);
            var tempDeleteDataNumber = default(Int64);
            StorageDeleteResult deleteDataResult = new StorageDeleteResult();
            StorageDeleteResult deleteIndexResult = new StorageDeleteResult();


            List<ArchiverBasicIndex> deletingIndexes = null;
            if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
            {
                NeedDeleteSubIndex = false;
                deletingIndexes = this.RetentionIndexService.GetDeletingIndexesByModifiedTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.DateTimeNow, isFitSoftDeleteAndRetainByModifedTime);
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

            if (deletingIndexes != null && deletingIndexes.Count > 0)
            {
                HashSet<string> needDeletedFileContentName = new HashSet<string>();
                foreach (var deletingIdx in deletingIndexes)
                {
                    var info = XConvert.FromNames(dataVolume, deletingIdx.JobId + "_content_" + deletingIdx.ContentDataFileNumber + ".dat");
                    logger.Info($"Start to delete device content: {info.HighPlusLowName.LogBase64()}.ModifiedTime:{new DateTime(deletingIdx.ModifyTime)}.SubSubJobId:{deletingIdx.JobId}.");
                    try
                    {
                        var delSize = Math.Max(DeleteDatas(info), 0);
                        if (needToAddDetail)
                        {
                            AddRetentionToReport(deletingIdx, info.LowName, delSize, JobDetailsStatus.Successful, "RM_JS_Common_Delete", "", archiverRetentionInfo.DataLogicalDevice.Name);
                        }
                        isDeleteSucceedAtLeastOnce = true;
                        tempDeleteDataSize += delSize;
                        tempDeleteDataNumber++;
                    }
                    catch (Exception ex)
                    {
                        if (!isDeleteSucceedAtLeastOnce)
                        {
                            if (needToAddDetail)
                            {
                                AddRetentionToReport(deletingIdx, info.LowName, 0, JobDetailsStatus.Failed, "RM_JS_Common_Delete", ex.Message, archiverRetentionInfo.DataLogicalDevice.Name);
                            }
                            this.ErrorMessage = ex.Message;
                            this.jobStatusInfo.State = 3;
                            this.logger.Error($"Retention Service Delete Data From Device Error.info.LowName:{info.LowName.LogBase64()},ex:{ex}");
                            throw;
                        }
                        else
                        {
                            this.jobStatusInfo.State = 7;
                            this.logger.Error($"Retention Service Delete Data From Device Warn.info.LowName:{info.LowName.LogBase64()},ex:{ex}");
                        }
                        logger.Info($"Update media size success,job id:{this.archiverRetentionInfo.JobId},size:{tempDeleteDataSize}");
                    }
                }
            }
            else
            {
                logger.Info($"No file need to delete, job id:{this.archiverRetentionInfo.JobId}");
            }

            if (tempDeleteDataSize > 0 && this.archiverRetentionInfo.RetentionRule != RetentionRule.MoveArchiverJobData)
            {
                RetainedInfo tempInfo = new RetainedInfo();
                tempInfo.SubSubJobId = this.archiverRetentionInfo.JobId;
                tempInfo.RetainSize = tempDeleteDataSize;
                tempInfo.RetainFileNumber = tempDeleteDataNumber;
                tempInfo.IsSimulateJob = this.archiverRetentionInfo.IsSimulateJob;
                HybridApiClient.Instance.UpdateRetainedSizeInfo(tempInfo);
            }

            if (archiverRetentionInfo.IsSimulateJob)
            {
                return tempDeleteDataSize;
            }

            if (this.archiverRetentionInfo.RetentionRule == RetentionRule.RetainArchiverJobData)
            {
                logger.Info($"Current job id is {this.archiverRetentionInfo.RetentionJob.Id}");
                if (this.archiverRetentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    this.RetentionIndexService.DeleteDataFromMainIndexByDateTime(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId, this.archiverRetentionInfo.DateTimeNow, isFitSoftDeleteAndRetainByModifedTime);
                }
                else
                {
                    this.RetentionIndexService.DeleteDataFromMainIndex(this.archiverRetentionInfo.StoragePolicyId, this.archiverRetentionInfo.JobId);
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
                    this.logger.Warn($"RetentionServiceDeleteDataFromDeviceWarn.storageInfo.LowName:{storageInfo.LowName.LogBase64()},ex:{ex}");
                }
            }

            return tempDeleteDataSize;
        }


        private void CommitReport(JMFSRetainJobDetails report)
        {
            if (archiverRetentionInfo.IsSimulateJob)
            {
                JMFSRetainDashboardDetails dashboardReport = new JMFSRetainDashboardDetails(report)
                {
                    RetentionKeepDate = archiverRetentionInfo.KeepValue,
                    RetentionKeepDateUnit = (int)archiverRetentionInfo.ArchiveDateUnit,
                    RetentionSource = archiverRetentionInfo.RetentionSourceName,
                    SourceFlag = archiverRetentionInfo.SourceFlag,
                };
                JobDetailService.Commit(dashboardReport);
            }
            else
            {
                JobDetailService.Commit(report);
            }
        }

        private void AddRetentionToReport(ArchiverBasicIndex deletingIdx, string fileName, long size, JobDetailsStatus status, string action,string message,string storageName)
        {
            JMFSRetainJobDetails report = new JMFSRetainJobDetails();
            string filePath = string.IsNullOrEmpty(deletingIdx.ExtraInfo)? deletingIdx.Attributes + "\\"+ deletingIdx.Name: deletingIdx.Attributes + "\\" + deletingIdx.ExtraInfo + "\\" + deletingIdx.Name;
            report.SiteUrl = filePath;
            report.Size = size.ToString();
            report.Status = status;
            report.JobId = archiverRetentionInfo.JobId;
            report.Comment = message;
            report.Action = action;//"RM_AR_CP_GSS_Retention_MarkDataTier";
            report.SrcStorageName = storageName;
            CommitReport(report);
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
                foreach (var item in fileList)
                {
                    var info = XConvert.FromNames(item.HighName, item.Name);
                    try
                    {
                        isDeleteSucceedAtLeastOnce = true;
                        tempDeleteDataSize += Math.Max(DeleteDatas(info), 0);
                    }
                    catch (Exception ex)
                    {
                        if (!isDeleteSucceedAtLeastOnce)
                        {
                            this.ErrorMessage = ex.Message;
                            this.jobStatusInfo.State = 3;
                            this.logger.Error($"RetentionServiceDeleteDataFromDeviceError.info.LowName:{info.LowName.LogBase64()},e:{ex}");
                            throw;
                        }
                        else
                        {
                            this.jobStatusInfo.State = 7;
                            this.logger.Error($"RetentionServiceDeleteDataFromDeviceWarn.info.LowName:{info.LowName.LogBase64()},e:{ex}");
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
        private ArchiverRetentionResult RetainJobData()
        {
            logger.Info("this action is real delete");
            this.deleteDataSize = this.DeleteDataFromDevice(this.dataVolume, this.indexVolume, true);
            logger.Info($"Retention Service Retain Job Data Begin.this.archiverRetentionInfo.JobId:{this.archiverRetentionInfo}");
            var result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            result.Size = deleteDataSize;
            result.State = 2;
            //result.HasIndexRelatedToBackupJob = IsExistsIndexRelatedToJob(this.archiverRetentionInfo.JobId);
            this.logger.Info($"RetentionServiceRetainJobDataDeleteDataFinished.this.archiverRetentionInfo.JobId:{this.archiverRetentionInfo},this.deleteDataSize.ToString():{this.deleteDataSize.ToString()}");
            return result;
        }
        private ArchiverRetentionResult ConvertInfoToResult(ArchiverRetentionInfo info)
        {
            var result = new ArchiverRetentionResult();
            result.FarmName = info.FarmName;
            result.JobId = info.JobId;
            result.SiteUrl = info.ConnectionId;
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
        public void Open(ArchiverRetentionInfo retentionInfo)
        {
            this.archiverRetentionInfo = retentionInfo;
            this.logger.Info($"ArchiverBackupRetentionServiceOpenStart:this.archiverRetentionInfo.JobId:{this.archiverRetentionInfo}");
            this.jobStatusInfo.State = 2;
            this.dataVolume = retentionInfo.DataVolume;
            this.indexVolume = retentionInfo.IndexVolume;
            this.accessTierType = retentionInfo.AccessTierType;
            this.logger.Info("Retention Service Open Index And DataDevice");
            if (this.archiverRetentionInfo.RetentionRule == AvePoint.Media.Service.DomainModel.RetentionRule.MoveArchiverJobData)
            {
                var desPhysical = retentionInfo.DestinationDevice.PhysicalDrives.FirstOrDefault();
                if (desPhysical.Id.Equals(DEFAULTSTORAGEID, StringComparison.OrdinalIgnoreCase) || desPhysical.IsSystemStorage || desPhysical.Type == 14 || desPhysical.Type == 407)//google device
                {
                    logger.Info("Retention Service Open Destination Data Device,the destination storage is avepoint storage,skip process it");
                    isMoveToAvepointStorage = true;
                    return;
                }
                else
                {
                    this.logger.Info("Retention Service Open Destination Data Device");
                    this.destinationLogicalDevice = this.DeviceManager.Open(retentionInfo.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Data));
                    this.logger.Info("Retention Service Open Destination Data Device Finished");
                }
            }
            this.indexLogicalDevice = this.DeviceManager.Open(this.archiverRetentionInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            var tempXri = this.archiverRetentionInfo.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data)[0];
            dataLogicalDevice = XFactory.InstanceSystem(tempXri);
            dataLogicalDevice.Open();
            dataLogicalDeviceIsAzure = dataLogicalDevice.Type == ServiceConstants.AzureSystem?true:false;
            this.CacheManager.Open(retentionInfo.CacheSetting, BackgroundSettings.GetInstance().ArchiveTemp, true);
            this.isObjectType = this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object) || this.indexLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object);
            this.logger.Info("RetentionServiceOpenIndexAndDataDeviceFinished");
            this.OpenMainIndex(this.archiverRetentionInfo, this.indexVolume);
            //if (retentionInfo.RetentionRule == RetentionRule.MarkArchiverJobDataTier || (retentionInfo.IsSoftDelete && !retentionInfo.IsFitSoftDelete))
            //{
            //    this.OpenSubIndex(this.archiverRetentionInfo, this.indexVolume);
            //}
            this.destinationStoreInArchiverTier = retentionInfo.DestinationStoreInArchiverTier;
            this.rehydrationTemp = "data_fs_archive\\Temp\\" + Guid.NewGuid();
            InitAzureStorageInfo(tempXri);
            //IsLorealSoftDelete = IsEnabledRealDelete();

            logger.Info($"Source storage type: {dataLogicalDevice?.Type}, destination storage type: {destinationLogicalDevice?.Type}");
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
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo,
                DBPassWord = HybridApiClient.Instance.GetDBSEEMasterKey()
        };
            _ArchiverIndexService.Open(indexServiceOpenParameter);
            this.RetentionIndexService.InitIndexProcesser(_ArchiverIndexService);
        }
    }
}
