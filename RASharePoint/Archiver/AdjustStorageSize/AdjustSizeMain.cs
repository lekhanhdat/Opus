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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.MoveDataTier;
using Media.Service.DomainModel.MoveDataTier;
using Storage.Cloud.Azure;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.SharePoint.Archiver.AdjustStorageSize
{
    public class AdjustSizeMain
    {
        private static IRALogger mLog = new RALogger(typeof(AdjustSizeMain));
        IXSystem dataLogicalDevice;
        string dataVolume;
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public IVolumeGeneratorFactory VolumeGeneratorFactory { get { return new VolumeGeneratorFactory(); } }
        public IStorageDeviceManager DeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private long TimeOlderTicks { get
            {
                var key = RMKeyValueDao.GetValueByKey("TimeOlderTicks");
                if (key == null)
                {
                    return 0;
                }
                else
                {
                    if (long.TryParse(key?.Value, out long result))
                    {
                        return result;
                    }
                }
                return 0;
            }}
        private string JobId = string.Empty;
        private JobContext jobContext = null;
        public JobReportImps mJobreport;
        public AdjustSizeMain(string jobId)
        {
            JobId = jobId;
            jobContext = JobContext.GetInstance(jobId, JobType.AdjustStorageSize);
            jobContext.ReportManager.StartUpdateJobProgress();
        }
        public async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                mLog.Info("start adjust storage size");
                List<string> errorStorageId = new List<string>();
                mJobreport = new JobReportImps(jobContext.ReportManager);
                List<AdjustStorageSizeContent> adjustStorageSizeList = new List<AdjustStorageSizeContent>();
                adjustStorageSizeList = GetNeedAdjustJob();
                mLog.Info($"adjustStorageSizeList count is {adjustStorageSizeList?.Count}");
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(adjustStorageSizeList);
                foreach (var adjustContent in adjustStorageSizeList)
                {
                    mLog.Info($"jobIdWithStorageId count is {adjustContent.jobIdWithStorageId?.Count}");
                    foreach (var jobKeyValue in adjustContent.jobIdWithStorageId)
                    {
                        string storageId = jobKeyValue.Value;
                        if (errorStorageId.Contains(storageId))
                        {
                            mLog.Warn($"this storage can not connect ,storage id:{storageId} has error,skip retry it,jobKeyValue.Key:{jobKeyValue.Key}");
                            continue;
                        }
                        var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId, needDecryptSecert: true);
                        storageDevice ??= (await StorageDeviceService.GetSystemStorageAsync()).FirstOrDefault();//Migration to Opus, AvePoint Storage for Opus may not exist.                                                                             //var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                        dataLogicalDevice = XFactory.InstanceSystem(storageDevice?.ConnectionString);
                        try
                        {
                            using (new CheckJobStopScope())
                            {
                                var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
                                var volumeParam = new VolumeParameter(new MoveDataTierJob()
                                {
                                    FarmName = string.Empty,
                                    SiteUrl = adjustContent.SiteUrl
                                });
                                dataVolume = volumeGenerator.GenerateDataVolume(volumeParam);
                                var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                                var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(jobKeyValue.Key, StringComparison.OrdinalIgnoreCase));
                                if (fileList != null && fileList.Count > 0)
                                {
                                    mLog.Info($"Need adjust file count : {fileList.Count}");
                                    long size = ProcessDataFile(fileList, dataLogicalDevice);
                                    await ArchiverIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeForAdjustAsync(jobKeyValue.Key, size);
                                    mLog.Info("finish adjust file");
                                }
                                else
                                {
                                    await ArchiverIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeForAdjustAsync(jobKeyValue.Key, -1);
                                    mLog.Info($"current job id has no file to adjust storage size,job id:{jobKeyValue.Key}");
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
                            if (!errorStorageId.Contains(storageId))
                            {
                                mLog.Warn($"something went wrong when adjust storage id:{storageId}");
                                errorStorageId.Add(storageId);
                            }
                            mLog.Warn($"something went wrong when adjust ,job id:{jobKeyValue.Key},error:{e}");
                            mJobreport.HasErrorNode = true;
                        }
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
        private List<AdjustStorageSizeContent> GetNeedAdjustJob()
        {
            List<AdjustStorageSizeContent> result = new List<AdjustStorageSizeContent>();
            var siteMasterInfo = ArchiverSiteMasterIndexDao.GetAllDisposalJobNodsInfo(TimeOlderTicks);
            var subInfos = ArchiverIndexSubInfoDao.GetAllDisposalArchiverIndexSubInfo(TimeOlderTicks);
            foreach (var siteInfo in siteMasterInfo) 
            {
                AdjustStorageSizeContent info = new AdjustStorageSizeContent();
                info.SiteUrl = siteInfo.SiteURL;
                info.jobIdWithStorageId = new List<KeyValuePair<string, string>>();
                foreach (var subInfo in subInfos) 
                {
                    if (subInfo.SubSubJobId.StartsWith(siteInfo.JobId))
                    {
                        string tempKey = subInfo.SubSubJobId;
                        string tempValue = string.IsNullOrEmpty(subInfo.CurrentStorageId) ? subInfo.StorageInfo : subInfo.CurrentStorageId;
                        KeyValuePair<string, string> tempPair = new KeyValuePair<string, string>(tempKey, tempValue);
                        info.jobIdWithStorageId.Add(tempPair);
                    }
                }
                result.Add(info);
            }
            return result;
        }
        private long ProcessDataFile(List<XFileInfo> fileList, IXSystem dataDevice)
        {
            long size = 0;
            fileList.ForEach(item =>
            {
                var azureFile = dataDevice.OpenFile(item);
                size += azureFile.FileSize;
            });
            return size;
        }
    }
}
