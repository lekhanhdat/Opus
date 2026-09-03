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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.EDiscovery;
using PnP.Framework.Modernization.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexJobRunner
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexJobRunner));

        private readonly string _jobId;

        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndex = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private readonly IArchiverIndexSubInfoDao _archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        private readonly ISettingProfileService _settingProfileService = PlatformWindsorManager.GetService<ISettingProfileService>();

        private readonly IRetentionIndexSubInfoDao _retentionIndexSubInfoDao = PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IRMArchiveSiteInfoDao _archiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly RMArchivedFullTextIndexCategoryManagement _categoryManagementService = new();

        private readonly RMArchivedFullTextIndexJobManager _jobManager;

        private readonly string _siteUrl;

        private RMArchivedFullTextIndexSiteManager _siteManager;

        public RMArchivedFullTextIndexJobRunner(string jobId)
        {
            _jobId = jobId;
            _jobManager = new(jobId);
            _siteUrl = ResolveSiteUrlFromSubJob();
            StorageApiConfiguration.Setup();
            var communicationKey = _settingProfileService.GetCommunicationEncryptionKey();
            CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
            MediaConfigInfo.CommonConfigInfo = PlatformWindsorManager.GetService<CommonConfigInfo>();
        }

        private string ResolveSiteUrlFromSubJob()
        {
            try
            {
                var subJob = _subJobDao.GetSubJob(_jobId, true);
                var siteUrl = subJob?.String1?.Trim();
                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    _logger.Warn($"Full text index sub job [{_jobId}] has empty site url.");
                    return string.Empty;
                }

                return siteUrl;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while resolving site url for sub job [{_jobId}]. Error: {e}");
                return string.Empty;
            }
        }

        public async Task RunAsync()
        {
            try
            {
                var maxArchiverTime = await _archiverSiteMasterIndex.GetMaxArchiverTimeAsync();
                _logger.Info($"The max archiver time of archiver job is [{maxArchiverTime}].");

                if (maxArchiverTime == 0)
                {
                    _jobManager.Finish();
                    return;
                }
                
                await RunTargetSiteAsync(_siteUrl, maxArchiverTime);

                await _siteManager?.FinishAsync();

                await _archivedFullTextIndexDao.AddOrUpdateLatestSyncTimeAsync(maxArchiverTime);

                if (_keyValueDao.TryGetBoolValue(KeyNameCollection.IsNewFullTextIndex, out var isNew) && isNew)
                {
                    _logger.Info($"The full text index has already in newest, no need to add flag.");
                    return;
                }

                _logger.Info($"Set full text index is new flag to true.");
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.IsNewFullTextIndex, Value = "true" });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run job. Error: {e}");
                _jobManager.Failed();
            }
            finally
            {
                _jobManager.Finish();
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        #region Site processing

        private async Task RunTargetSiteAsync(string siteUrl, long maxArchiverTime)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                _logger.Warn("No site url found for full text index sub job. Skip execution.");
                return;
            }

            var siteManager = GetSiteManager(siteUrl);
            var needRunSubJobDic = new Dictionary<string, ArchiverIndexSubInfoContract>();

            var siteInfo = _archiveSiteInfoDao.GetSiteInfoesBySiteUrls(new List<string> { siteUrl }).FirstOrDefault();
            if (siteInfo != null)
            {
                var deleteRetentionSubJobs = await DeleteRetentionDataAsync(siteInfo);
                deleteRetentionSubJobs.ForEach(item => needRunSubJobDic[item.JobId] = item);
            }

            var indexEnumerable = _archiverSiteMasterIndex.GetSiteMasterIndexesAsync(siteManager.SiteUrl, siteManager.LatestSyncTime, maxArchiverTime);
            await foreach (var index in indexEnumerable)
            {
                var subJobs = _archiverIndexSubInfoDao.GetSubInfoesBySubJobId(index.JobId);
                subJobs.ForEach(item =>
                {
                    item.SiteUniqueId = index.SiteId;
                    item.SiteUrl = index.SiteURL;
                    item.ArchiverTime = index.ArchiverTime;
                    needRunSubJobDic[item.JobId] = item;
                });
            }

            var needRerunSubJobs = await GetNeedRerunSubJobsAsync(siteManager.Id);
            needRerunSubJobs.ForEach(item => needRunSubJobDic[item.JobId] = item);

            var needRunSubJobs = needRunSubJobDic.Values.OrderBy(item => item.ArchiverTime).ToList();
            _logger.Info($"Need run job count [{needRunSubJobs.Count}].");

            _jobManager.Init(needRunSubJobs.Count());

            if (!needRunSubJobs.Any())
            {
                return;
            }

            var logicalDevice = await LoadAllStorageDevicesAsync();
            foreach (var needRunSubJob in needRunSubJobs)
            {
                var synchronizer = new RMArchivedFullTextIndexSynchronizer(_jobManager, siteManager, needRunSubJob, logicalDevice);
                await synchronizer.SyncAsync();
            }
        }


        private async Task<List<ArchiverIndexSubInfoContract>> DeleteRetentionDataAsync(RMArchiveSiteInfo siteNode)
        {
            var res = new List<ArchiverIndexSubInfoContract>();
            try
            {
                var needDeleteJobs = await _retentionIndexSubInfoDao.GetRetentionInfoesAsync(siteNode.SiteUrl);
                foreach (var needDeleteJob in needDeleteJobs)
                {
                    try
                    {
                        var siteManager = GetSiteManager(needDeleteJob.SiteURL);
                        if (siteManager.LatestSyncTime == 0)
                        {
                            _logger.Warn($"The site [{siteManager.SiteUrl}] never sync data. Skip it [{needDeleteJob.ArchiverJobId}].");
                            await _retentionIndexSubInfoDao.DeleteAsync(needDeleteJob);
                            continue;
                        }
                        var syncJobManager = new RMArchivedFullTextIndexSyncJobManager(siteManager, _jobManager, needDeleteJob.ArchiverJobId, needDeleteJob.SiteId, 0);
                        await syncJobManager.InitAsync(true);

                        // var (hasCategory, categoryInfo) = await _categoryManagementService.TryGetCategoryInfoAsync(needDeleteJob.ArchiverJobId);
                        // if (!hasCategory)
                        // {
                        //     _logger.Warn($"The archiver job [{needDeleteJob.ArchiverJobId}] no category info found. No need delete data. Skip it.");
                        //     await _retentionIndexSubInfoDao.DeleteAsync(needDeleteJob);
                        //     continue;
                        // }

                        var deletor = new RMArchivedFullTextIndexEDiscoveryDataJobDeletor(siteManager, _jobManager, syncJobManager);
                        var deleteRes = await deletor.DeleteAsync();
                        deleteRes &= await deletor.WaitAsync();
                        await _categoryManagementService.SyncCategoryDataSizeAsync();

                        _jobManager.Add(siteManager.SiteUrl, needDeleteJob.Id, deleteRes ? Contract.RMWeb.JobMonitor.JobStatus.Finished : Contract.RMWeb.JobMonitor.JobStatus.Failed);

                        if (!deleteRes)
                        {
                            continue;
                        }

                        await _retentionIndexSubInfoDao.DeleteAsync(needDeleteJob);

                        var (has, archiverJob) = await _archiverIndexSubInfoDao.TryGetSubInfoByJobIdAsync(needDeleteJob.ArchiverJobId);
                        if (has)
                        {
                            archiverJob.SiteUniqueId = needDeleteJob.SiteId;
                            archiverJob.SiteUrl = needDeleteJob.SiteURL;
                            res.Add(archiverJob);
                            _logger.Info($"The site [{siteManager.SiteUrl}] retention job [{needDeleteJob.JobId}] has related archver job [{needDeleteJob.ArchiverJobId}].");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while delete [{needDeleteJob.ArchiverJobId}] retention data. Error: {e}");
                    }
                }

                _logger.Info($"Need run delete retention relate job count [{res.Count}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete retention data. Error: {e}");
            }

            return res;
        }

        private async Task<List<ArchiverIndexSubInfoContract>> GetNeedRerunSubJobsAsync(long siteId)
        {
            var res = new List<ArchiverIndexSubInfoContract>();

            var jobs = await _archivedFullTextIndexDao.GetSiteJobInfoesV1(siteId, Contract.RMWeb.JobMonitor.JobStatus.Failed, Contract.RMWeb.JobMonitor.JobStatus.FinishWithException);
            foreach (var job in jobs)
            {
                var (has, subInfo) = await _archiverIndexSubInfoDao.TryGetSubInfoByJobIdAsync(job.ArchiverJobId);
                if (!has)
                {
                    _logger.Info($"No archiver job [{job.ArchiverJobId}] found in master index table.");
                    job.Status = Contract.RMWeb.JobMonitor.JobStatus.Finished;
                    job.StartTime = DateTime.UtcNow.Ticks;
                    job.EndTime = DateTime.UtcNow.Ticks;
                    job.FullTextIndexSyncJobId = _jobManager.JobId;
                    await _archivedFullTextIndexDao.AddOrUpdateJobInfoAsync(job);
                    continue;
                }

                subInfo.ArchiverTime = job.ArchiverTime;
                subInfo.SiteUniqueId = job.SiteId;
                subInfo.SiteUrl = job.SiteUrl;
                res.Add(subInfo);
            }

            _logger.Info($"Need re run job count [{res.Count}].");

            return res;
        }

        #endregion

        private async Task<LogicalDeviceDto> LoadAllStorageDevicesAsync()
        {
            var deviceIDs = await _archiverIndexSubInfoDao.GetAllDeviceIDsAsync();
            var dataDeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives = new()
            };
            foreach (var deviceID in deviceIDs)
            {
                var storageDeviceDto = _storageDeviceService.GetStorageDeviceById(deviceID, needDecryptSecert: true);
                if (storageDeviceDto == null)
                {
                    _logger.Warn($"No storage device found for id [{deviceID}].");
                    continue;
                }
                
                dataDeviceDto.PhysicalDrives.Add(new PhysicalDeviceDto()
                {
                    Id = storageDeviceDto.Id,
                    ConnectionString = storageDeviceDto.ConnectionString,
                    ModifyTime = storageDeviceDto.ModifyTime,
                    Type = storageDeviceDto.Type,
                });
            }
            return dataDeviceDto;
        }

        private RMArchivedFullTextIndexSiteManager GetSiteManager(string siteUrl)
        {
            if (_siteManager == null || !string.Equals(_siteManager.SiteUrl, siteUrl, StringComparison.OrdinalIgnoreCase))
            {
                _siteManager = new RMArchivedFullTextIndexSiteManager(siteUrl);
            }

            return _siteManager;
        }
    }
}
