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
using AvePoint.GCommon.Contract.Replicator.Object.Settings;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using Media.Service.DomainModel.MoveDataTier;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using Storage.Cloud.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.MoveDataTier
{
    public class MoveDataTierMain
    {
        private static IRALogger mLog = new RALogger(typeof(MoveDataTierMain));
        IXSystem dataLogicalDevice;
        string dataVolume;
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public IVolumeGeneratorFactory VolumeGeneratorFactory { get { return new VolumeGeneratorFactory(); } }
        public IStorageDeviceManager DeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private string JobId = string.Empty;
        private JobContext jobContext = null;
        public JobReportImps mJobreport;
        public MoveDataTierMain(string jobId)
        {
            JobId = jobId;
            jobContext = JobContext.GetInstance(jobId, JobType.MoveDataTier);
            jobContext.ReportManager.StartUpdateJobProgress();
        }
        public async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                mLog.Info("start move data tier");
                mJobreport = new JobReportImps(jobContext.ReportManager);
                MoveDataTierContent siteUrlWithJobId = SerializerHelper.DeserializeByDataContractSerializer<MoveDataTierContent>(jobContext.JobContextSetting);
                string siteUrl = siteUrlWithJobId.SiteUrl;
                string storageId = RecordsConstants.AVEPOINT_DEFAULT_STORAGEID;
                var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
                storageDevice ??= (await StorageDeviceService.GetSystemStorageAsync()).FirstOrDefault();//Migration to Opus, AvePoint Storage for Opus may not exist.
                //var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                dataLogicalDevice = XFactory.InstanceSystem(storageDevice?.ConnectionString);
                foreach (string jobId in siteUrlWithJobId.JobIds)
                {
                    try
                    {
                        using (new CheckJobStopScope())
                        {
                            List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(jobId);
                            ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
                            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
                            var volumeParam = new VolumeParameter(new MoveDataTierJob()
                            {
                                FarmName = string.Empty,
                                SiteUrl = siteInfo.SiteURL,
                                WebAppUrl = siteInfo.WebURL
                            });
                            dataVolume = volumeGenerator.GenerateDataVolume(volumeParam);
                            var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                            var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(jobId, StringComparison.OrdinalIgnoreCase));
                            if (fileList != null && fileList.Count > 0)
                            {
                                mLog.Info($"Need move data tier count : {fileList.Count}");
                                ProcessDataFile(fileList, dataLogicalDevice);
                                ArchiverSiteMasterIndexDao.SetMoveDateTierFlag(jobId);
                                mLog.Info("finish move data tier");
                            }
                            else
                            {
                                mLog.Info($"current job id has no file to move data tier,job id:{jobId}");
                            }
                        }
                    }
                    catch (JobStopException ex)
                    {
                        mLog.Warn("job is stopped");
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Error($"something went wrong when move data tier,job id:{jobId}");
                        throw;
                    }
                }
            }
            catch (JobStopException ex)
            {
                mLog.Warn("job is stopped");
                mJobreport.HasStop = true;
                throw;
            }
            catch (Exception e)
            {
                mLog.Error($"move date tier error ,reson:{e.ToString()}");
                mJobreport.HasErrorNode = true;
            }
            finally
            {
                mJobreport.FinishReport();
                dataLogicalDevice.Close();
            }
        }
        private void ProcessDataFile(List<XFileInfo> fileList, IXSystem dataDevice)
        {
            fileList.ForEach(item =>
            {
                var azureFile = dataDevice.OpenFile(item);
                if (azureFile is AzureCloudInfo)
                {
                    var tempFile = azureFile as AzureCloudInfo;
                    
                    mLog.Info($"azureFile FileTierType is {tempFile.FileTierType.ToString()}");
                    if (tempFile.FileTierType != AccessTierType.Archive)
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        SetFileTierArchiveAsync(dataDevice, info).GetAwaiter().GetResult();
                    }
                }
            });
        }
        private async Task SetFileTierArchiveAsync(IXSystem destinationDevice, StorageInfo storageInfo)
        {
            try
            {
                using (new CheckJobStopScope()){ }
                if (destinationDevice.StorageType == XStorageType.Azure)
                {
                    var device = destinationDevice as IAzureSystem;
                    AzureCloudInfo info = new AzureCloudInfo();
                    info.HighName = storageInfo.HighName;
                    info.LowName = storageInfo.LowName;
                    info.FileTierType = AccessTierType.Archive;
                    var result = await device.ChangeFileTierAsync(info);
                    if (!result.IsChanged)
                        mLog.Warn("An error occurred while setting file Archive. FileName: {0}", storageInfo.LowName);
                }
            }
            catch (JobStopException ex)
            {
                mLog.Warn("job is stopped when SetFileTierArchive");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while setting file tier. Reason: {0}, FileName: {1}", ex.ToString(), storageInfo.LowName);
                throw;
            }
        }
    }
}
