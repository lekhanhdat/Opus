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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using Google.Cloud.Logging.V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex
{
    [Audit]
    public class RMArchivedFullTextIndexService : IRMArchivedFullTextIndexService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexService));

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndex = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private readonly IKeyValueService _keyValueService = PlatformWindsorManager.GetService<IKeyValueService>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly IRetentionIndexSubInfoDao _retentionIndexSubInfoDao = PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();
        
        private readonly IRestoreSearchService _restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();
        public async Task<ArchiverRestoreResult> GetSearchResultByFilter(ArchiverRestoreResult searchContract)
        {
            var isNewFullTextIndexkeyValue = _keyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
            if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
            {
                var querierV1 = new RMArchivedFullTextIndexQuerierV1(searchContract.SerchContract);
                return await querierV1.QueryAsync(searchContract.ContinuationToken, searchContract.PageSize);
            }

            var querier = new RMArchivedFullTextIndexQuerier(searchContract.SerchContract);
            return await querier.QueryAsync(searchContract.ContinuationToken, searchContract.PageSize, searchContract.CategoryId);
        }
        public async Task<ArchiverRestoreResult> GetEDiscoverySimpleSearchResult(ArchiverRestoreSimpleSearchQueryParameter parameter)
        {
            var isNewFullTextIndexkeyValue = _keyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
            if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
            {
                var querierV1 = new RMArchivedFullTextIndexSimpleQuerierV1(parameter);
                return await querierV1.QueryAsync();
            }

            var querier = new RMArchivedFullTextIndexSimpleQuerier(parameter);
            return await querier.QueryAsync();
        }
        public void SendJobMessage()
        {
            try
            {
                var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.ArchiverFullTextIndex);
                var jobCount = _jobMonitorService.GetRunningJobsCount(JobType.ArchiverFullTextIndex);
                if (queueCount + jobCount > 0)
                {
                    _logger.Warn("Archive full text index job already exists. Skipped send.");
                    return;
                }

                JobQueueDto jqDto = new()
                {
                    JobType = JobType.ArchiverFullTextIndex,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = BuildAndUploadJobPayload(),
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
                _logger.Info($"Send archive full text index job message successful");

            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while send archive full text index job message. Error: {e}");
            }
        }

        private string BuildAndUploadJobPayload()
        {
            try
            {
                var payload = BuildJobPayload();
                string json = SerializerHelper.SerializeByJsonConvert(payload);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using var stream = new MemoryStream(bytes);
                string blobUrl = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.FullTextIndexJobInfoFile, Guid.NewGuid().ToString());
                RAStorageUtil.UploadReportBlob(blobUrl, new MemoryStream(bytes));
                return blobUrl;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while building and uploading archived full text index job payload. Error: {e}");
                return string.Empty;
            }
        }

        public RMArchivedFullTextIndexJobPayload BuildJobPayload()
        {
            try
            {
                var payload = new RMArchivedFullTextIndexJobPayload
                {
                    IsBlacklistMode = _keyValueService.IsSCBlackListForEdiscovery()
                };

                if (!payload.IsBlacklistMode)
                {
                    payload.SiteUrls = _restoreSiteMappingDao.GetAllWhitelist()
                        .Select(w => NormalizeSiteUrlOriginal(w.SourceSiteUrl))
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                else
                {
                    payload.SiteUrls = _restoreSiteMappingDao.GetAllBlacklist()
                        .Select(b => NormalizeSiteUrl(b.SourceSiteUrl))
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToList();
                }

                _logger.Info($"Resolved archived full text index job payload. Mode: {(payload.IsBlacklistMode ? "blacklist" : "whitelist")}. Target site count: [{payload.SiteUrls?.Count ?? 0}].");

                return payload;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while building archived full text index job payload. Error: {e}");
                return null;
            }
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunArchiverFullTextIndexJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunJob(string payload = null)
        {
            var jobId = string.Empty;
            try
            {
                var conflictJobCount = 0;
                foreach(var conflictJobType in new List<JobType> { JobType.ArchiverMoveIndex, JobType.ArchiverRetention, JobType.DeleteRestoredData, JobType.ArchiverDeduplication, JobType.DeleteOrphanDatas })
                {
                    conflictJobCount += _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, conflictJobType);
                    conflictJobCount += _jobMonitorService.GetRunningJobsCount(conflictJobType);
                }

                jobId = _jobMonitorService.CreateJob(JobType.ArchiverFullTextIndex, "RM_TS_RunSchedule");
                if (conflictJobCount > 0)
                {
                    _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_AFTI_JobSkipped");
                    _logger.Warn($"Due to move index or retention job has running, skipped full text index job.");
                    return jobId;
                }

                if (_restoreSearchService.HasReachedIndexSizeLimitation())
                {
                    _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_AFTI_JobSkippedBySizeLimitation");
                    _logger.Warn($"Due to index size has exceeded the purchased capacity, skipped full text index job.");
                    return jobId;
                }

                var jobPayload = ResolveJobPayload(payload);
                if (jobPayload == null)
                {
                    _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_AFTI_JobFailed");
                    _logger.Warn("Failed to resolve full text index job payload.");
                    return jobId;
                }

                var targetSites = ResolveTargetSiteUrls(jobPayload);
                var filteredSites = FilterTargetSitesByPendingWork(targetSites).GetAwaiter().GetResult();
                if (filteredSites.Count == 0)
                {
                    _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished, string.Empty);
                    _logger.Warn("No eligible sites found for full text index job. Skip execution.");
                    return jobId;
                }
                int subJobCountInConfigFile = _keyValueDao.GetSubJobCountFromDB((int)JobType.ArchiverFullTextIndex);
                _subJobDao.UpdateSubJobCount(jobId, filteredSites.Count);
                var currentSubjobIndex = 0;
                foreach (var siteUrl in filteredSites)
                {
                    var sendNow = currentSubjobIndex < subJobCountInConfigFile;
                    var subJobId = CreateSubJob(jobId, currentSubjobIndex, filteredSites.Count, siteUrl, sendNow);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        _jobQueueService.HandleMessage(new JobQueueMessage()
                        {  
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = JobType.ArchiverFullTextIndex,
                            CommandLine = string.Format("{0} {1}", JobType.ArchiverFullTextIndex, subJobId),
                        });
                    }
                    currentSubjobIndex++;
                }

                _logger.Info($"Successful create archive full text index sub jobs for [{jobId}]. Count: [{filteredSites.Count}].");

                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run archive full text index job. Error: {e}");
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
                return string.IsNullOrWhiteSpace(jobId) ? string.Empty : jobId;
            }
        }

        private async Task<List<string>> FilterTargetSitesByPendingWork(List<string> targetSites)
        {
            if (targetSites == null || targetSites.Count == 0)
            {
                return new List<string>();
            }

            var maxArchiverTime = await _archiverSiteMasterIndex.GetMaxArchiverTimeAsync();
            if (maxArchiverTime == 0)
            {
                return new List<string>();
            }

            var targetSet = NormalizeSiteUrls(targetSites);
            var pendingSiteSet = await GetPendingSiteSetAsync(targetSet, maxArchiverTime);
            var result = FilterTargetSitesBySet(targetSites, pendingSiteSet);

            _logger.Info($"Pending site count: {result.Count}.");

            return result;
        }

        private static HashSet<string> NormalizeSiteUrls(IEnumerable<string> siteUrls)
        {
            return siteUrls
                .Select(NormalizeSiteUrl)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> FilterTargetSitesBySet(IEnumerable<string> targetSites, HashSet<string> pendingSites)
        {
            var result = new List<string>();
            foreach (var siteUrl in targetSites)
            {
                var normalized = NormalizeSiteUrl(siteUrl);
                if (!string.IsNullOrWhiteSpace(normalized) && pendingSites.Contains(normalized))
                {
                    result.Add(siteUrl);
                }
            }

            return result;
        }

        private async Task<HashSet<string>> GetPendingSiteSetAsync(HashSet<string> targetSites, long maxArchiverTime)
        {
            var pendingSites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var siteInfos = await _archivedFullTextIndexDao.GetSiteInfoesBySiteUrlsV1Async(targetSites);
            var siteInfoByUrl = siteInfos
                .Where(info => !string.IsNullOrWhiteSpace(info.SiteUrl))
                .GroupBy(info => NormalizeSiteUrl(info.SiteUrl), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.LatestSyncTime).First(), StringComparer.OrdinalIgnoreCase);
            var siteInfoById = siteInfos.ToDictionary(info => info.Id, info => info);

            pendingSites.UnionWith(await GetRetentionSiteSetAsync(targetSites));
            pendingSites.UnionWith(await GetFailedJobSiteSetAsync(targetSites, siteInfoById));
            pendingSites.UnionWith(await GetUpdatedSiteSetAsync(targetSites, siteInfoByUrl, maxArchiverTime));

            return pendingSites;
        }

        private async Task<HashSet<string>> GetRetentionSiteSetAsync(HashSet<string> targetSites)
        {
            var retentionJobs = await _retentionIndexSubInfoDao.GetRetentionInfoesBySiteUrlsAsync(targetSites);
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var job in retentionJobs)
            {
                var siteUrl = NormalizeSiteUrl(job.SiteURL);
                if (!string.IsNullOrWhiteSpace(siteUrl) && targetSites.Contains(siteUrl))
                {
                    result.Add(siteUrl);
                }
            }

            return result;
        }

        private async Task<HashSet<string>> GetFailedJobSiteSetAsync(
            HashSet<string> targetSites,
            Dictionary<long, AvePoint.RA.DB.Model.ArchivedFullTextIndex.RMArchivedDataFullTextIndexSiteInfoesV1> siteInfoById)
        {
            var failedJobs = await _archivedFullTextIndexDao.GetJobInfoesBySiteUrlsV1Async(
                targetSites,
                JobStatus.Failed,
                JobStatus.FinishWithException);
            var siteIds = siteInfoById.Keys.ToList();
            if (siteIds.Count > 0)
            {
                var failedJobsById = await _archivedFullTextIndexDao.GetJobInfoesBySiteIdsV1Async(
                    siteIds,
                    JobStatus.Failed,
                    JobStatus.FinishWithException);
                failedJobs.AddRange(failedJobsById);
            }
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var job in failedJobs)
            {
                var siteUrl = NormalizeSiteUrl(job.SiteUrl);
                if (string.IsNullOrWhiteSpace(siteUrl) && siteInfoById.TryGetValue(job.FullTextIndexSiteId, out var siteInfo))
                {
                    siteUrl = NormalizeSiteUrl(siteInfo.SiteUrl);
                }

                if (!string.IsNullOrWhiteSpace(siteUrl) && targetSites.Contains(siteUrl))
                {
                    result.Add(siteUrl);
                }
            }

            return result;
        }

        private async Task<HashSet<string>> GetUpdatedSiteSetAsync(
            HashSet<string> targetSites,
            Dictionary<string, AvePoint.RA.DB.Model.ArchivedFullTextIndex.RMArchivedDataFullTextIndexSiteInfoesV1> siteInfoByUrl,
            long maxArchiverTime)
        {
            var minLatestSyncTime = targetSites
                .Select(siteUrl => siteInfoByUrl.TryGetValue(siteUrl, out var info) ? info.LatestSyncTime : 0)
                .DefaultIfEmpty(0)
                .Min();

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var indexEnumerable = _archiverSiteMasterIndex.GetSiteMasterIndexesBySiteUrlsAsync(
                targetSites,
                minLatestSyncTime,
                maxArchiverTime);
            await foreach (var index in indexEnumerable)
            {
                var siteUrl = NormalizeSiteUrl(index.SiteURL);
                if (string.IsNullOrWhiteSpace(siteUrl) || !targetSites.Contains(siteUrl))
                {
                    continue;
                }

                var latestSyncTime = siteInfoByUrl.TryGetValue(siteUrl, out var siteInfo)
                    ? siteInfo.LatestSyncTime
                    : 0;
                if (index.ArchiverTime > latestSyncTime)
                {
                    result.Add(siteUrl);
                }
            }

            return result;
        }

        private RMArchivedFullTextIndexJobPayload ResolveJobPayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return BuildJobPayload();
            }

            return DownloadAndParseJobPayload(payload);
        }

        private RMArchivedFullTextIndexJobPayload DownloadAndParseJobPayload(string blobUrl)
        {
            string jsonContent = string.Empty;
            try
            {
                var jobPayLoadFilePath = JobReportUtility.GetFullTextIndexJobFile(blobUrl);
                jsonContent = File.ReadAllText(jobPayLoadFilePath);
                return SerializerHelper.DeserializeByJsonConvert<RMArchivedFullTextIndexJobPayload>(jsonContent);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while download or parse job payload from [{blobUrl}]. Error: {e}");
                return BuildJobPayload();
            }
        }

        private List<string> ResolveTargetSiteUrls(RMArchivedFullTextIndexJobPayload payload)
        {
            if (payload == null)
            {
                return new List<string>();
            }

            if (!payload.IsBlacklistMode)
            {
                return payload.SiteUrls ?? new List<string>();
            }

            var blacklist = payload.SiteUrls?.Select(NormalizeSiteUrl) ?? Enumerable.Empty<string>();

            var allSites = _archiverSiteMasterIndex.GetAllBackupSiteCollectionDistinctUrl()
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allowedSites = allSites
                .Where(url => !blacklist.Contains(NormalizeSiteUrl(url)))
                .Select(NormalizeSiteUrlOriginal)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.Info($"Allowed site count: {allowedSites.Count}.");

            return allowedSites;
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, int subJobCount, string siteUrl, bool sendNow)
        {
            var subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)JobType.ArchiverFullTextIndex,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Weight = 100d / subJobCount,
                Runable = sendNow ? RecordsConstants.SubJob_Runnable_CanRun : RecordsConstants.SubJob_Runnable_Waiting,
                String1 = siteUrl
            };

            _subJobDao.CreateJob(subJob);
            return subJobId;
        }

        private static string NormalizeSiteUrl(string url)
        {
            return string.IsNullOrWhiteSpace(url)
                ? string.Empty
                : url.Trim().TrimEnd('/').ToLowerInvariant();
        }

        private static string NormalizeSiteUrlOriginal(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            return url.Trim().TrimEnd('/');
        }

        public async Task<string> GetSiteLatestArchivedTime(SiteCollectionNodesInfo searchNode)
        {
            try
            {
                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var ticks = 0L;
                var isNewFullTextIndexkeyValue = _keyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
                if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
                {
                    ticks = await _archivedFullTextIndexDao.GetSiteLatestArchivedTimeV1Async(searchNode.SiteUrl);
                }
                else
                {
                    ticks = await _archivedFullTextIndexDao.GetSiteLatestArchivedTimeAsync(searchNode.SiteUrl);
                }
                return _generalSettingService.ConvertTiksToDateTime(gls, ticks, true).DataTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get site [{searchNode.SiteUrl}] latest archived time. Error: {e}");
                return DateTime.UtcNow.Ticks.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public async Task<string> GetLatestArchivedTime()
        {
            try
            {
                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var ticks = 0L;
                var isNewFullTextIndexkeyValue = _keyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
                if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
                {
                    ticks = await _archivedFullTextIndexDao.GetLatestArchivedTimeV1Async();
                }
                else
                {
                    ticks = await _archivedFullTextIndexDao.GetLatestArchivedTimeAsync();
                }

                return _generalSettingService.ConvertTiksToDateTime(gls, ticks, true).DataTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get latest archived time. Error: {e}");
                return DateTime.UtcNow.Ticks.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
