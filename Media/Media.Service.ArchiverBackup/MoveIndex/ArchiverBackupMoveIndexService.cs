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
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using GCommon.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Storage;
    using AvePoint.RA.Common;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.Contract.Object;
    using AvePoint.RA.Common.Report;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using Microsoft.SharePoint.Client;
    using Merged18NResources.MediaServiceApplicationModel;
    using Microsoft.Azure.Cosmos.Core;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.CommonUtil;
    using static ICSharpCode.SharpZipLib.Zip.FastZip;
    using Storage.Cloud.Google;
    using AvePoint.RA.DB.Model;
    #endregion

    public class ArchiverBackupMoveIndexService
        : MoveIndexServiceBase<ArchiverMoveIndexInfo, ArchvierMoveIndexResult>
        , IMoveIndexService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;
        IXSystem destinationLogicalDevice;
        JobStatusInfo jobStatusInfo = new JobStatusInfo();
        Int64 totalSize;
        Boolean isObjectType;

        String indexVolume;
        ArchiverMoveIndexInfo info;
        ArchvierMoveIndexResult result;
        List<XFileInfo> indexFileLists = new List<XFileInfo>();
        List<JMJobDetails> jobDetailList = new List<JMJobDetails>();
        Int64 maxItemNum;
        Int64 sendItemNum = 0;

        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);

        public IStorageDeviceManager DeviceManager { get; set; }

        public override void Open(ArchiverMoveIndexInfo info)
        {
            this.logger.Info("ArchiverBackupMoveIndexService Open Begin.");
            this.info = info;
            this.jobStatusInfo.State = ServiceConstants.JobFinished;
            //this.JobStatusUpdater = JobReportServiceFactory.CreateJobStatusUpdater();
            this.indexLogicalDevice = this.DeviceManager.Open(info.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.destinationLogicalDevice = this.DeviceManager.Open(info.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(info.CacheSetting, false, true);
            this.isObjectType = this.destinationLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object);
            this.logger.Info("ArchiverBackupMoveIndexService Open ended.");
        }

        public override async Task<ArchvierMoveIndexResult> MoveAsync(ArchiverMoveIndexInfo info)
        {
            this.result = new ArchvierMoveIndexResult() { Finished = true };
            this.indexFileLists = new List<XFileInfo>();
            this.logger.Info("ArchiverBackupMoveIndexService move index begin.");

            //两个device实际为同一个路径，导致copy api overwrite时删除source index 文件，所以创建一个随机文件并检查，来避免这个问题
            string testFileName = $"{Guid.NewGuid().ToString()}.dat";
            this.logger.Info($"Begin test source and destination {testFileName} ");
            var testStorageInfo = XConvert.FromNames("", testFileName);
            try
            {
                var testContent = Encoding.UTF8.GetBytes("Test");
                MemoryStream ms = new MemoryStream();
                ms.Write(testContent, 0, testContent.Length);
                var res = this.indexLogicalDevice.CommitStream(ms, testStorageInfo);

                if (res.IsCommited && this.destinationLogicalDevice.FileExists(testStorageInfo))
                {
                    this.logger.Warn("The test file exists in the destination storage, so will throw exception.");
                    throw new Exception("The source storage same as destination storage");
                }
            }
            catch (Exception e)
            {
                this.logger.Warn($"Test file has exception,{e}");
            }
            finally
            {
                if (this.indexLogicalDevice.FileExists(testStorageInfo))
                {
                    this.indexLogicalDevice.DeleteFile(testStorageInfo);
                }
            }

            info.SiteUrls.ForEach(tempSiteUrl =>
            {
                var volumeParam = new VolumeParameter() { FarmName = String.Empty, WebApplicationUrl = info.WebApp, SiteCollectionUrl = tempSiteUrl };
                this.indexVolume = info.VolumeGenerator.GenerateIndexVolume(volumeParam);
                List<XFileInfo> currentSiteIndexFileList = this.indexLogicalDevice.ListFiles(XConvert.FromNames(indexVolume, String.Empty));
                this.indexFileLists.AddRange(currentSiteIndexFileList);
                AddToDetailList(tempSiteUrl, currentSiteIndexFileList);
            });

            info.TeamsSiteUrls.ForEach(tempSiteUrl =>
            {
                var volumeParam = new VolumeParameter() { FarmName = String.Empty, WebApplicationUrl = info.WebApp, SiteCollectionUrl = tempSiteUrl, EmailAddress = tempSiteUrl };
                this.indexVolume = info.TeamsVolumeGenerator.GenerateIndexVolume(volumeParam);
                List<XFileInfo> currentSiteIndexFileList = this.indexLogicalDevice.ListFiles(XConvert.FromNames(indexVolume, String.Empty));
                this.indexFileLists.AddRange(currentSiteIndexFileList);
                AddToDetailList(tempSiteUrl, currentSiteIndexFileList);
            });

            info.ExchangeSiteUrls.ForEach(tempSiteUrl =>
            {
                var volumeParam = new VolumeParameter() { FarmName = String.Empty, WebApplicationUrl = info.WebApp, SiteCollectionUrl = tempSiteUrl, EmailAddress = tempSiteUrl };
                this.indexVolume = info.ExchangeVolumeGenerator.GenerateIndexVolume(volumeParam);
                List<XFileInfo> currentSiteIndexFileList = this.indexLogicalDevice.ListFiles(XConvert.FromNames(indexVolume, String.Empty));
                this.indexFileLists.AddRange(currentSiteIndexFileList);
                AddToDetailList(tempSiteUrl, currentSiteIndexFileList);
            });

            info.GDriveIndexInfos.ForEach(index =>
            {
                var volumeParam = new VolumeParameter() { TenantId = index.WebId, DriveId = index.SiteId };
                this.indexVolume = info.GDriveVolumnGenerator.GenerateIndexVolume(volumeParam);
                List<XFileInfo> currentSiteIndexFileList = this.indexLogicalDevice.ListFiles(XConvert.FromNames(indexVolume, String.Empty));
                this.indexFileLists.AddRange(currentSiteIndexFileList);
                AddToDetailList(index.SiteURL, currentSiteIndexFileList);
            });

            info.FSIndexInfos.ForEach(fSIndexInfo =>
            {
                string fsIndexFullPath = SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(ServiceConstants.FSArchiverPath, "IndexVolume"), fSIndexInfo.ConnectionId), fSIndexInfo.ConnectionId);
                this.indexVolume = fsIndexFullPath;
                List<XFileInfo> currentFsIndexFileList = this.indexLogicalDevice.ListFiles(XConvert.FromNames(fsIndexFullPath, String.Empty));
                this.indexFileLists.AddRange(currentFsIndexFileList);
                AddToDetailList(fSIndexInfo.ConnectionName, currentFsIndexFileList);
            });

            //copy + delete
            ReportMangerFactory.Instance.ReportManager.IncreaseBase(indexFileLists.Count * 2);
            if (this.indexFileLists.Count > 0)
            {
                maxItemNum = this.indexFileLists.Count;  //SAAS-13656 增加keep alive thread逻辑防止运行超过一个小时的job time out。
                this.indexFileLists.ForEach(item =>
                {
                    sendItemNum++;
                    var indexInfo = XConvert.FromNames(item.HighName, item.LowName);
                    indexInfo.Length = this.indexLogicalDevice.OpenFile(indexInfo).FileSize;
                    totalSize += item.FileSize;
                    //var result = this.indexLogicalDevice.CopyFile(indexInfo, this.destinationLogicalDevice, indexInfo, true);
                    //if (!result.IsCopyed)
                    //{
                    //    this.logger.Error($"Failed copy file to destination.Message {result.Message} {indexInfo.ToString()}");
                    //    var retryResult = RetryByDownload(indexInfo);
                    //    if (!retryResult.IsCopyed)
                    //    {
                    //        throw new Exception(result.Message);
                    //    }
                    //}
                    //else
                    //{
                        this.logger.Info($"Verify index size.");
                        //if (!VerifyIndexSize(this.destinationLogicalDevice, indexInfo, indexInfo.Length))
                        //{
                            //exception handle
                            var retryResult = RetryByDownload(indexInfo);
                            if (!retryResult.IsCopyed)
                            {
                                throw new Exception(result.Message);
                            }
                        //}
                    //}

                    ReportMangerFactory.Instance.ReportManager.Increase();
                    this.logger.Info("The database {0}has been move successful.", item.Name);
                });

                var message = await StorageDeviceService.SetUsingDeviceByIdAsync(info.DestinationDevice.PhysicalDrives.FirstOrDefault()?.Id, RA.Contract.SettingProfilesType.IndexDevice);

                if (message != null && message.MessageType == RAMessageType.Successful)
                {
                    try
                    {
                        this.logger.Info("Delete source file.");
                        this.DeleteSourceFiles();
                        this.logger.Info("Delete source file finished.");
                    }
                    catch (Exception e)
                    {
                        this.logger.Error($"Failed to delete source file, {e}");
                    }
                }
                else
                {
                    this.logger.Error($"Failed to set index device, {message?.ErrorMessage}");
                    this.jobStatusInfo.State = ServiceConstants.JobFailed;
                }

            }
            else
            {
                this.logger.Warn("There is no index files need to move,source device :{0}", this.indexLogicalDevice.SystemLocation.ToString());
                var message = await StorageDeviceService.SetUsingDeviceByIdAsync(info.DestinationDevice.PhysicalDrives.FirstOrDefault()?.Id, RA.Contract.SettingProfilesType.IndexDevice);
                if (message != null && message.MessageType == RAMessageType.Successful)
                {
                    this.logger.Info("SetUsingDeviceByIdAsync successful.");
                }
            }
            this.logger.Info("ArchiverBackupMoveIndexService move index finished.");
            return result;
        }

        private StorageCopyResult RetryByDownload(StorageInfo indexInfo)
        {
            this.logger.Info($"Will retry move index by donwload.");
            var retryResult = CopyIndexByDownload(indexLogicalDevice, indexInfo, destinationLogicalDevice, indexInfo, true);
            if (retryResult.IsCopyed)
            {
                this.logger.Info($"Verify index size again.");
                if (!VerifyIndexSize(this.destinationLogicalDevice, indexInfo, indexInfo.Length))
                {
                    throw new Exception("The index file size mismatch.");
                }
            }
            return retryResult;
        }

        private StorageCopyResult CopyIndexByDownload(IXSystem sourceSystem, StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            StorageCopyResult result = new StorageCopyResult();
            var cacheBuffer = new Byte[1024 * 64];
            if (isOverWrite)
            {
                if (destSystem.OpenFile(destFile)?.Exists ?? false)
                {
                    this.logger.Info($"Find index file {srcFile.LowName} in destination, will delete first.");
                    destSystem.DeleteFile(destFile);
                }
            }
            var source = sourceSystem as AbstractXSystem;
            var destination = destSystem as AbstractXSystem;
            string cacheLocation = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, "MoveIndexCache");
            var cacheFileLocation = SecurityUtils.SafeCombinePath(cacheLocation, srcFile.HighPlusLowName);
            if (source != null && source.StorageType == XStorageType.Azure && destination != null && destination.StorageType == XStorageType.Azure)
            {
                source = ValidStorage(source);
                destination = ValidStorage(destination);
                var sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                var desContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(destination.ConnectionString);
                logger.Info($"start copy source index:{srcFile.HighPlusLowName} to target");
                AzureUtil.DownloadBlobToAsync(sourceContainerClient, srcFile.HighPlusLowName, cacheFileLocation).GetAwaiter().GetResult();
                desContainerClient.GetBlobClient(destFile.HighPlusLowName).Upload(cacheFileLocation, true);
                logger.Info($"finish copy index,target:{destFile.HighPlusLowName}");

            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(cacheFileLocation)))
                {
                    logger.Warn($"move index cache location does not exist,create it");
                    Directory.CreateDirectory(Path.GetDirectoryName(cacheFileLocation));
                }
                using (var downloader = sourceSystem.OpenStream(srcFile, FileMode.Open))
                {
                    try
                    {
                        logger.Info($"not azure ,start copy source index:{srcFile.HighPlusLowName} to target");
                        using (var uploader = System.IO.File.Open(cacheFileLocation, FileMode.OpenOrCreate))
                        {
                            Int32 readLen = 0;
                            while ((readLen = downloader.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                            {
                                uploader.Write(cacheBuffer, 0, readLen);
                            }
                            if (destination?.StorageType == XStorageType.GoogleCloud)
                            {
                                var destGoogleFile = new GoogleCloudInfo(destFile.HighName, destFile.LowName);
                                destGoogleFile.StorageClass = GoogleStorageClass.Standard;
                                destination?.CommitStream(uploader, destGoogleFile);
                            }
                            else
                            {
                                destination?.CommitStream(uploader, destFile);
                            }
                        }
                        result.IsCopyed = true;
                        logger.Info($"finish copy index,target:{destFile.HighPlusLowName}");
                    }
                    catch (Exception e)
                    {
                        logger.Error($"something went wrong when copying index. message:{e.ToString()}");
                        result.IsCopyed = false;
                        result.Message = e.Message;
                    }
                }

            }
            System.IO.File.Delete(cacheFileLocation);
            return result;
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
        private bool VerifyIndexSize(IXSystem destSystem, StorageInfo destFile, long sourceIndexSize)
        {
            var destFileSize = destSystem.OpenFile(destFile).FileSize;
            if (sourceIndexSize == destFileSize)
            {
                this.logger.Info($"Verify index size successful, FileSize {destFileSize}.");
                return true;
            }
            else
            {
                this.logger.Error($"Failed verify index size, Source {sourceIndexSize} Destination {destFileSize}");
                return false;
            }
        }

        public override void ProcessException(Exception e, ArchvierMoveIndexResult result)
        {
            result.Finished = false;
            this.jobStatusInfo.State = ServiceConstants.JobFailed;
            result.Message = String.Format("An error occurred while moving indexes,details:{0}.", e.Message);
            this.logger.Error("An error occurred while moving indexes,details:{0}.", e.Message);
        }

        public override void Close()
        {
            if (this.DeviceManager != null)
            {
                this.DeviceManager.Close(this.indexLogicalDevice);
                this.DeviceManager.Close(this.destinationLogicalDevice);
            }
            if (this.jobStatusInfo.State.Equals(ServiceConstants.JobFinished))
                ReportMangerFactory.Instance.ReportManager.SetJobFinished(RA.Contract.RMWeb.JobMonitor.JobStatus.Finished, "RM_Job_MoveIndexServiceJobReportSuccessful");
            else if(this.jobStatusInfo.State.Equals(ServiceConstants.JobFinishedWithException))
                ReportMangerFactory.Instance.ReportManager.SetJobFinished(RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException, "RM_Job_MoveIndexServiceJobReportFailed");
            else if (this.jobStatusInfo.State.Equals(ServiceConstants.JobFailed))
                ReportMangerFactory.Instance.ReportManager.SetJobFinished(RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_Job_MoveIndexServiceJobReportFailed");
        }

        public override void GenerateJobReport()
        {
            try
            {
                if (this.jobStatusInfo.State != ServiceConstants.JobFinished)
                {
                    foreach (var detail in jobDetailList)
                    {
                        detail.Status = JobDetailsStatus.Failed;
                    }
                }
                ReportMangerFactory.Instance.ReportManager.BatchSendJobDetail(jobDetailList);
            }
            catch (Exception ex)
            {
                this.logger.Error("Generate job report is failed, due to {0}:", ex.ToString());
            }
        }
        private void AddToDetailList(string siteUrl, List<XFileInfo> currentSiteIndexFileList)
        {
            Int64 currentSiteIndexSize = 0;
            JMArchiverMoveIndexJobDetails jobDetail = new JMArchiverMoveIndexJobDetails();
            jobDetail.Status = this.jobStatusInfo.State.Equals(ServiceConstants.JobFinished) ?
                               JobDetailsStatus.Successful : JobDetailsStatus.Failed;
            jobDetail.SiteUrl= siteUrl;
            jobDetail.SrcStorageName = this.info.IndexLogicalDevice.Name;
            jobDetail.DesStorageName = this.info.DestinationDevice.Name;
            foreach (var currentSiteIndexFile in currentSiteIndexFileList)
            {
                currentSiteIndexSize += currentSiteIndexFile.FileSize;
            }
            jobDetail.Size = currentSiteIndexSize.ToString();
            jobDetailList.Add(jobDetail);
        }
        private void DeleteSourceFiles()
        {
            this.indexFileLists.ForEach(item =>
            {
                var indexInfo = XConvert.FromNames(item.HighName, item.Name);
                this.indexLogicalDevice.DeleteFile(indexInfo);
                ReportMangerFactory.Instance.ReportManager.Increase();
                this.logger.Info("Delete source index file successful,details:{0}.", item.Name);
            });
            var indexDirectorInfo = XConvert.FromNames(this.indexVolume, String.Empty);
            if (this.indexLogicalDevice.DirectoryExists(indexDirectorInfo))
            {
                var result = this.indexLogicalDevice.ListDirectories(XConvert.FromNames(this.indexVolume, String.Empty)).Count;
                if (result <= 0)
                {
                    this.logger.Info("Index logical device should be deleted,details:{0}.", result.ToString());
                    this.indexLogicalDevice.DeleteDirectory(indexDirectorInfo);
                }
            }
            this.logger.Info("Delete source index file ended.");
        }
    }
}
