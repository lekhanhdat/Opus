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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Model.DisposalStub;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Azure;
using Cloud.Sdk.Data.AosModern;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.SharePoint.StubDisposal
{
    public class StubDisposalProcessor
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(StubDisposalProcessor));

        private IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();

        private IRMSiteStubSettingMappingDao SiteStubSettingMappingDao => PlatformWindsorManager.GetService<IRMSiteStubSettingMappingDao>();

        private IRMStubDisposalSiteInfoDao StubDisposalSiteInfoDao => PlatformWindsorManager.GetService<IRMStubDisposalSiteInfoDao>();

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        private bool mJobHasException = false;
        private bool mJobHasSuccess = false;
        private string mSummaryComment = string.Empty;
        private string _jobId = string.Empty;
        private JobType _jobType;
        private JobContext _jobContext = null;
        private StubDisposalSiteInfoDto _stubDisposalSiteInfoDto = null;
        private Dictionary<string, SiteStubSettingMappingDto> _siteStubTemplateMapping = null;
        private const int BATCH_SIZE = 25;
        private DateTime _cutOffTime = DateTime.MinValue;
        private AppProfileInfo _profileInfo;
        private TenantConnectionInfo _tenantConnectionInfo;
        private TokenResult _tokenResult;
        private readonly Dictionary<string, (IAveSite site, IAveORecords record)> _siteCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RMStubFileRecordTableEntity> _bufferedDeleteRecords = new();
        private readonly Dictionary<string, long> _minStubCreatedTimeMapping = new();
        private readonly HashSet<string> _processedTemplateIds = new();

        public StubDisposalProcessor(string jobId, JobType jobType)
        {
            _jobType = jobType;
            _jobId = jobId;
            _jobContext = JobContext.GetInstance(_jobId, _jobType);
            ReportManager.StartUpdateJobProgress();

        }

        public async Task Initialize()
        {
            _logger.Info($"Initializing StubDisposalProcessor");
            _siteStubTemplateMapping = [];
            _stubDisposalSiteInfoDto = SerializerHelper.DeserializeByJsonConvert<StubDisposalSiteInfoDto>(_jobContext.JobContextSetting);
            if (_stubDisposalSiteInfoDto == null)
            {
                throw new Exception($"Failed to deserialize JobContextSetting for job {_jobId}");
            }
            _logger.Info($"JobContextSetting. SiteCollectionUrl: {_stubDisposalSiteInfoDto.SiteCollectionUrl}, StartDisposalTime: {_stubDisposalSiteInfoDto.StartDisposalTime.Ticks}, MinRetentionTime: {_stubDisposalSiteInfoDto.MinRetentionTime}");

            var mappings = await SiteStubSettingMappingDao.GetAllMappingsBySiteUrlAsync(_stubDisposalSiteInfoDto.SiteCollectionUrl);
            if (mappings == null || mappings.Count == 0)
            {
                _logger.Warn($"No site stub setting mapping found for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}");
                StubDisposalSiteInfoDao.DeleteByKey(_stubDisposalSiteInfoDto.Id);
                throw new Exception($"RM_JM_StubDisposal_ErrorMessage_NoAvailableMapping");
            }
            _logger.Info($"Found {mappings.Count} site stub setting mappings for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}.");

            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(_stubDisposalSiteInfoDto.SiteCollectionUrl);
            if (remoteSiteCollection == null)
            {
                _logger.Error($"The site [{_stubDisposalSiteInfoDto.SiteCollectionUrl}] not found in opus db.");
                throw new Exception("RM_RDM_SCNotFound");
            }

            try
            {
                InitTenantConnection(remoteSiteCollection);

                var bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(_stubDisposalSiteInfoDto.SiteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
                var site = aveObjectModelFactory.CreateSite(_stubDisposalSiteInfoDto.SiteCollectionUrl);
                var record = aveObjectModelFactory.CreateRecords();
                _siteCache.Add(_stubDisposalSiteInfoDto.SiteCollectionUrl, (site, record));
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to initialize tenant connection. E: {e}");
                WebException we = e as WebException ?? e.InnerException as WebException;
                if (we != null && we.Response is HttpWebResponse response)
                {
                    _logger.Error($"HTTP Status Code: {response.StatusCode}");
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception($"RM_TS_NoRegister");
                    }
                }
                throw;
            }

            var usingTemplateId = Guid.Empty;

            foreach (var mapping in mappings)
            {
                if (!mapping.IsEnabledRetention)
                {
                    _logger.Info($"Stub template {mapping.StubTemplateId} is disable Retention. Skip it");
                    continue;
                }

                DateTime tempCutOff = CalculateStubRetentionDate(_stubDisposalSiteInfoDto.StartDisposalTime, mapping.RetentionValue, mapping.RetentionUnit, true);

                if (tempCutOff > _cutOffTime)
                {
                    _cutOffTime = tempCutOff;
                    usingTemplateId = mapping.StubTemplateId;
                }

                var stubTemplateId = mapping.StubTemplateId.ToString();

                if (!_siteStubTemplateMapping.TryGetValue(stubTemplateId, out var mappingDto))
                {
                    _siteStubTemplateMapping[stubTemplateId] = new SiteStubSettingMappingDto()
                    {
                        SiteCollectionUrl = mapping.SiteCollectionUrl,
                        StubTemplateId = mapping.StubTemplateId,
                        IsEnabledRetention = mapping.IsEnabledRetention,
                        RetentionValue = mapping.RetentionValue,
                        RetentionUnit = mapping.RetentionUnit,
                        FirstStubCreatedTime = mapping.FirstStubCreatedTime,
                    };

                    _logger.Info($"Add mapping for stub template {mapping.StubTemplateId} with retention {mapping.RetentionValue} {mapping.RetentionUnit}, first stub created time: {mapping.FirstStubCreatedTime}");
                }
                else
                {
                    _logger.Warn($"There are multiple mappings for the same stub template {mappingDto.StubTemplateId} and site {mappingDto.SiteCollectionUrl}. Please check data consistency. Mapping id: {mapping.Id}");
                }
            }

            _processedTemplateIds.Add(usingTemplateId.ToString());
            _logger.Info($"Calculated CutOff Time: {_cutOffTime:yyyyMMddHHmmss} from template: {usingTemplateId}");
        }

        private void InitTenantConnection(RemoteSiteCollection remoteSiteCollection)
        {
            _logger.Info($"Initializing tenant connection for site {remoteSiteCollection.url}, siteId: {remoteSiteCollection.ObjectId} with tenantId {remoteSiteCollection.TenantId}");
            if (remoteSiteCollection == null)
            {
                _logger.Error($"The site not found in opus db.");
                throw new Exception("RM_RDM_SCNotFound");
            }

            var o365TenantId = remoteSiteCollection.TenantId;
            _profileInfo = PoolUserUtil.GetBPOSInfoAsync(o365TenantId).GetAwaiter().GetResult();
            if (_profileInfo == null)
            {
                _logger.Error($"The site [{remoteSiteCollection.url}] no app profile found.");
                throw new Exception("RM_JM_AppProfile_NotFoundError");
            }

            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            _tenantConnectionInfo = client.TenantManagementService.GetByTenantIdAsync(o365TenantId).GetAwaiter().GetResult();
            if (_tenantConnectionInfo == null)
            {
                _logger.Error($"The site [{remoteSiteCollection.url}] no tenant info found in AOS.");
                throw new Exception("RM_JM_Archive_TenantRemoveFromAOS_ErrorMessage");
            }
        }

        public async Task RunJobAsync()
        {
            var finalStatus = JobStatus.None;
            JMStubDisposalJobDetails siteFailReport = null;
            try
            {
                await Initialize();

                if (_cutOffTime == DateTime.MinValue)
                {
                    _logger.Info($"No valid cut off time calculated for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}. No stub file needs to be processed");
                    finalStatus = JobStatus.Finished;
                    return;
                }

                _logger.Info($"Start to run StubDisposalProcessor for " +
                    $"site {_stubDisposalSiteInfoDto.SiteCollectionUrl}, " +
                    $"MinRetentionTime: {_stubDisposalSiteInfoDto.MinRetentionTime}, " +
                    $"StartDisposalTime: {_stubDisposalSiteInfoDto.StartDisposalTime:yyyyMMddHHmmss}");

                await foreach (var batch in StubFileRecordDao.QueryMainRecByRetTimeBatchesAsync(TenantLocalValue.LogonGroupId, _stubDisposalSiteInfoDto.Id.ToString(), _cutOffTime))
                {
                    _logger.Info($"Processing a batch of {batch.Count} stub file records for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}");
                    ReportManager.IncreaseBase(batch.Count * 2);
                    await ProcessSearchResultByBatch(batch, _stubDisposalSiteInfoDto.SiteCollectionUrl);
                }

                _logger.Info($"Finished processing all batches for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}");

                // 6.2
                if (_bufferedDeleteRecords.Count > 0)
                {
                    _logger.Info($"Flushing remaining {_bufferedDeleteRecords.Count} buffered delete records to table.");
                    await FlushDeletionBufferAsync();
                }

                _logger.Info($"All stub file records processed for site {_stubDisposalSiteInfoDto.SiteCollectionUrl}.");
                // 7.3 Calculate next MinRetentionTime and update to table, then job will be triggered by next MinRetentionTime
                await CalculateAndScheduleNextRunAsync();
            }
            catch (JobStopException)
            {
                _logger.Info($"JobStopException caught, job is stopping...");
                finalStatus = JobStatus.Stopped;
                mSummaryComment = string.Empty;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while running StubDisposalProcessor. E: {e}");
                siteFailReport = new()
                {
                    Url = _stubDisposalSiteInfoDto?.SiteCollectionUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message
                };
                finalStatus = JobStatus.Failed;
                mSummaryComment = e.Message; // or a default error message
            }
            finally
            {
                if (siteFailReport != null) SendReport(siteFailReport);

                if (finalStatus != JobStatus.None)
                {
                    _logger.Info($"Job is completed with status {finalStatus} and summary comment: {mSummaryComment}");
                }
                else if (mJobHasException)
                {
                    finalStatus = mJobHasSuccess ? JobStatus.FinishWithException : JobStatus.Failed;
                }
                else
                    finalStatus = JobStatus.Finished;

                ReportManager.SetJobFinished(finalStatus, mSummaryComment);
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        private async Task CalculateAndScheduleNextRunAsync()
        {
            long globalMinNextRunTicks = long.MaxValue;
            bool hasAnyActiveSchedule = false;

            foreach (var mapping in _siteStubTemplateMapping.Values)
            {
                if (!mapping.IsEnabledRetention) continue;

                _logger.Info($"Calculating next run for template {mapping.StubTemplateId} with firstStubCreateTime: {mapping.FirstStubCreatedTime}, retention {mapping.RetentionValue} {mapping.RetentionUnit}");

                long firstStubTicks = 0;
                string tplId = mapping.StubTemplateId.ToString();
                bool isTemplateUpdated = false;

                if (_minStubCreatedTimeMapping.TryGetValue(tplId, out long nextMinTicks))
                {
                    firstStubTicks = nextMinTicks;
                    isTemplateUpdated = true;
                    _logger.Info($"Template {tplId}: Found nextMinStubCreatedTime: {new DateTime(firstStubTicks)}");
                }
                else if (_processedTemplateIds.Contains(tplId))
                {
                    var nextRecord = await StubFileRecordDao.GetFirstRecordAfterTimeAsync(
                        TenantLocalValue.LogonGroupId,
                        _stubDisposalSiteInfoDto.Id.ToString(),
                        mapping.StubTemplateId,
                        _cutOffTime
                    );

                    if (nextRecord != null)
                    {
                        firstStubTicks = nextRecord.StubCreatedTime;
                        isTemplateUpdated = true;
                        _logger.Info($"Template {tplId}: Found future record from DB. FirstStubCreatedTime: {new DateTime(firstStubTicks)}");
                    }
                    else
                    {
                        firstStubTicks = 0;
                        isTemplateUpdated = true;
                        _logger.Info($"Template {tplId}: No records found. Template is empty.");
                    }
                }
                else
                {
                    firstStubTicks = mapping.FirstStubCreatedTime;
                    _logger.Info($"Template {tplId}: No processed record for this template, use the original FirstStubCreatedTime from mapping: {new DateTime(firstStubTicks)}");
                }

                
                if (isTemplateUpdated)
                {
                    if (firstStubTicks == 0)
                    {
                        _logger.Info($"Template {tplId}: No more stubs for this template. Delete the mapping.");
                        SiteStubSettingMappingDao.DeleteMappingBySiteUrlAndTemplateId(mapping.SiteCollectionUrl, mapping.StubTemplateId);
                    }
                    else
                    {
                        _logger.Info($"Template {tplId}: Update first stub created time to {new DateTime(firstStubTicks)}");
                        mapping.FirstStubCreatedTime = firstStubTicks;
                        SiteStubSettingMappingDao.UpdateFirstStubCreateTimeBySiteStlp(mapping.StubTemplateId.ToString(), mapping.SiteCollectionUrl, firstStubTicks);
                    }
                }

                if (firstStubTicks > 0)
                {
                    var createdTime = new DateTime(firstStubTicks, DateTimeKind.Utc);
                    long nextRunTicksForThisTemplate = CalculateStubRetentionDate(createdTime, mapping.RetentionValue, mapping.RetentionUnit).Ticks;

                    _logger.Info($"Template {tplId}: Next run time calculated based on first stub created time {createdTime:yyyy-MM-dd HH:mm:ss} is {new DateTime(nextRunTicksForThisTemplate):yyyy-MM-dd HH:mm:ss}");

                    if (nextRunTicksForThisTemplate < globalMinNextRunTicks)
                    {
                        globalMinNextRunTicks = nextRunTicksForThisTemplate;
                        hasAnyActiveSchedule = true;
                    }
                }
            }

            if (hasAnyActiveSchedule && globalMinNextRunTicks < long.MaxValue)
            {
                var nextRunTime = new DateTime(globalMinNextRunTicks, DateTimeKind.Utc);
                _logger.Info($"Rescheduling Job. Next MinRetentionTime: {nextRunTime:yyyy-MM-dd HH:mm:ss} (Ticks: {globalMinNextRunTicks})");
                await StubDisposalSiteInfoDao.UpdateMinRetentionTimeAsync(_stubDisposalSiteInfoDto.Id, globalMinNextRunTicks);
            }
            else
            {
                _logger.Info("No active stubs found to dispose. Job will not be scheduled (MinRetentionTime = MaxValue).");
                // hard delete or soft delete or set MinRetentionTime to MaxValue ?
                StubDisposalSiteInfoDao.DeleteByKey(_stubDisposalSiteInfoDto.Id);
            }

            _logger.Info("Finished calculating and scheduling next run.");
        }

        private async Task ProcessSearchResultByBatch(List<RMStubFileRecordTableEntity> searchResult, string scUrl)
        {
            using var _ = new PerformanceScope("StubDisposalProcessor:ProcessSearchResult", $"Process {searchResult.Count} items", true);

            foreach (var batch in searchResult.Chunk(BATCH_SIZE))
            {
                try
                {
                    using (new CheckJobStopScope()) { }

                    _logger.Info($"Processing a batch of {batch.Count()} stub file records for site {scUrl}");

                    var validDtos = await ValidateBatchResult(scUrl, batch);

                    _logger.Info($"{validDtos.Count} stub file records are valid for search in this batch.");

                    var notFoundFiles = await SearchStubsBatchAsync(validDtos);

                    _logger.Info($"{notFoundFiles.Count()} stub files not found in search by StubId for this batch.");

                    // for the files not found by stubId, try to delete by direct path
                    foreach (var file in notFoundFiles)
                    {
                        var filePath = file.ArchivedFileFullPath + LinkFileCommon.GetStubFileNameSuffixWithDot(file.StubType);
                        if (DeleteStubs(scUrl, file.WebId, filePath))
                        {
                            // process delete record in table
                            var rowKey = $"{file.RefDateTime:yyyyMMddHHmmss}_{file.ArchivedItemId:N}";
                            await BufferRecordForDeletionAsync(rowKey);
                            _processedTemplateIds.Add(file.StubTemplateId);
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error processing batch of stub file records. E: {e}");
                }
            }
            //return updatedRecordCount;
        }

        private async Task<List<StubFileRecordDto>> ValidateBatchResult(string scUrl, RMStubFileRecordTableEntity[] batch)
        {
            List<StubFileRecordDto> batchDtos = [];
            foreach (var entity in batch)
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (entity.RecordType == 1)
                    {
                        _logger.Warn($"Found index record for RowKey {entity.RowKey}, skip it");
                        continue; // skip index record
                    }

                    var createdTimeUtc = new DateTime(entity.StubCreatedTime, DateTimeKind.Utc);
                    DateTime expirationDateUtc;

                    if (_siteStubTemplateMapping.TryGetValue(entity.StubTemplateId, out var mapping))
                    {
                        expirationDateUtc = CalculateStubRetentionDate(createdTimeUtc, mapping.RetentionValue, mapping.RetentionUnit);
                    }
                    else
                    {
                        _logger.Warn($"No mapping found for stub template {entity.StubTemplateId}, site {scUrl}. RowKey: {entity.RowKey}. The stub template should be deleted. Clear orphan stub record");
                        // process delete record in table
                        await BufferRecordForDeletionAsync(entity.RowKey);
                        continue;
                    }

                    if (expirationDateUtc > _stubDisposalSiteInfoDto.StartDisposalTime)
                    {
                        _logger.Info($"The stub file RowKey {entity.RowKey} is not expired yet, skip it. CreatedTime: {createdTimeUtc:yyyyMMddHHmmss}, ExpirationDate: {expirationDateUtc:yyyyMMddHHmmss}");
                        TrackMinStubCreateTimeMapping(entity.StubTemplateId, entity.StubCreatedTime);
                        continue;
                    }

                    var tempDto = new StubFileRecordDto()
                    {
                        SiteCollectionID = Guid.Parse(entity.PartitionKey),
                        ArchivedItemId = Guid.Parse(entity.RowKey.Split('_').Last()),
                        StubTemplateId = entity.StubTemplateId,
                        ArchivedFileFullPath = entity.ArchivedFileFullPath,
                        StubType = (LeaveStubType)entity.StubType,
                        StubId = entity.StubId,
                        WebId = entity.WebId,
                        ListId = entity.ListId,
                        RefDateTime = new DateTime(entity.StubCreatedTime, DateTimeKind.Utc)
                    };

                    _logger.Info($"Validated stub file record with RowKey {entity.RowKey}. CreatedTime: {createdTimeUtc:yyyyMMddHHmmss}, ExpirationDate: {expirationDateUtc:yyyyMMddHHmmss}, StubId: {tempDto.StubId}");

                    batchDtos.Add(tempDto);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error validating stub file record with RowKey {entity.RowKey}. E: {e}");
                    // throw;
                }
            }

            return batchDtos;
        }

        private void TrackMinStubCreateTimeMapping(string templateId, long createdTicks)
        {
            if (!_minStubCreatedTimeMapping.ContainsKey(templateId))
            {
                _minStubCreatedTimeMapping[templateId] = createdTicks;
            }
            else
            {
                if (createdTicks < _minStubCreatedTimeMapping[templateId])
                {
                    _minStubCreatedTimeMapping[templateId] = createdTicks;
                }
            }
        }

        private async Task<IEnumerable<StubFileRecordDto>> SearchStubsBatchAsync(IEnumerable<StubFileRecordDto> stubFiles)
        {
            if (stubFiles == null || !stubFiles.Any()) return stubFiles;

            var foundStubIds = new Dictionary<string, string>(); // stubId, path

            try
            {
                var query = string.Join(" OR ", stubFiles.Select(s => $"\"{s.StubId}\""));
                if (string.IsNullOrEmpty(query))
                {
                    _logger.Warn("Empty query for SearchStubsBatchAsync.");
                    return stubFiles;
                }

                using var _ = new PerformanceScope("StubDisposalProcessor:SearchStubsBatchAsync", $"Batch search stubs by StubId", true);

                using var searchContext = new ClientContext(_tenantConnectionInfo.AdminUrl);
                var token = await GetTokenAsync();
                searchContext.ExecutingWebRequest += (sender, e) => e.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + token;

                var keywordQuery = new KeywordQuery(searchContext)
                {
                    QueryText = query,
                    TrimDuplicates = false, // to find copy/move files
                    RowLimit = 500,
                    StartRow = 0,
                    EnableSorting = true,
                    Culture = 1033,
                };

                keywordQuery.SelectProperties.Add("SiteName");
                keywordQuery.SelectProperties.Add("WebID");
                keywordQuery.SelectProperties.Add("ListId");
                keywordQuery.SelectProperties.Add("UniqueId");
                keywordQuery.SelectProperties.Add("FileExtension");
                keywordQuery.SelectProperties.Add("Path");

                keywordQuery.SelectProperties.Add("HitHighlightedSummary");

                var results = new SearchExecutor(searchContext).ExecuteQuery(keywordQuery);
                await searchContext.ExecuteQueryAsync();

                if (results.Value.Count > 0)
                {
                    var resultRows = results.Value[0].ResultRows.ToList();
                    _logger.Info($"Batch search found {resultRows.Count} results.");

                    foreach (var row in resultRows)
                    {
                        string summary = row["HitHighlightedSummary"] != null ? row["HitHighlightedSummary"].ToString() : string.Empty;
                        var matchedStub = stubFiles.FirstOrDefault(i => summary.Contains(i.StubId));
                        var filePath = row["Path"] != null ? row["Path"].ToString() : string.Empty;

                        if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                        {
                            await ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedStub);
                            continue;
                        }

                        // fallback to Path matching
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            var originalPath = filePath.Substring(0, filePath.LastIndexOf('.'));
                            var matchedPathIndex = stubFiles.FirstOrDefault(f => string.Equals(f.ArchivedFileFullPath, originalPath, StringComparison.OrdinalIgnoreCase));
                            if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                            {
                                await ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedPathIndex);
                                continue;
                            }
                        }

                        _logger.Warn($"Found a search result at [{filePath.LogBase64()}] but could not map back to any StubID in the batch via Summary. Summary content: {summary}...");
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured while SearchStubsBatchAsync. E: {e}");
            }

            var notFoundFiles = new List<StubFileRecordDto>();
            foreach (var kvp in stubFiles)
            {
                if (!foundStubIds.ContainsKey(kvp.StubId))
                {
                    notFoundFiles.Add(kvp);
                }
            }

            return notFoundFiles;
        }

        private async Task ValidateAndProcessMatchStub(Dictionary<string, string> foundStubIds, IDictionary<string, object> row, string filePath, StubFileRecordDto matchedStub)
        {
            bool isFirstResult = false;
            if (!foundStubIds.TryGetValue(matchedStub.StubId, out var cachePath))
            {
                foundStubIds.Add(matchedStub.StubId, filePath);
                isFirstResult = true;
            }
            else if (!string.Equals(cachePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                // this stub have multiple search results, which may be caused by copy/move
                isFirstResult = false;
            }
            else
            {
                // same stubId and same path but hit multiple times, which should not happend
                _logger.Warn($"StubId {matchedStub.StubId} with same path {filePath.LogBase64()} hit multiple times in search results");
                return;
            }

            if (!await ProcessFoundStubRowAsync(row, matchedStub, isFirstResult)) // error or not match type
            {
                foundStubIds.Remove(matchedStub.StubId);
            }
        }

        private async Task<bool> ProcessFoundStubRowAsync(IDictionary<string, object> row, StubFileRecordDto fileDto, bool isFirstResult)
        {
            try
            {
                using (new CheckJobStopScope()) { }

                var siteName = row["SiteName"].ToString();
                var webId = new Guid(row["WebID"].ToString());
                var listId = new Guid(row["ListId"].ToString());
                var uniqueId = new Guid(row["UniqueId"].ToString());
                var fileExtension = row["FileExtension"].ToString();
                var filePath = row["Path"].ToString();

                if (!webId.Equals(fileDto.WebId) || !listId.Equals(fileDto.ListId))
                {
                    _logger.Info($"Found stub file but in different web or list. webId: {webId}, listId: {listId}");
                }

                if (!string.Equals(siteName, _stubDisposalSiteInfoDto.SiteCollectionUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warn($"Found stub file but in different site. siteName:{siteName}, current FileUrl: {filePath}, original FileUrl:{fileDto.ArchivedFileFullPath}");
                }

                // Validate Type
                if (!string.Equals(fileExtension, LinkFileCommon.GetStubFileNameSuffix(fileDto.StubType), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warn($"Found stub file but not the same archived stub type. fileExtension:{fileExtension}, StubStype:{fileDto.StubType}, FileUrl:{fileDto.ArchivedFileFullPath}");
                    return false;
                }

                _logger.Info($"Found stub file for recordId:{fileDto.ArchivedItemId}, isFirstResult: {isFirstResult}");

                if (DeleteStubs(siteName, webId, filePath) && isFirstResult)
                {
                    // process delete record in table
                    var rowKey = $"{fileDto.RefDateTime:yyyyMMddHHmmss}_{fileDto.ArchivedItemId:N}";
                    await BufferRecordForDeletionAsync(rowKey);
                    _processedTemplateIds.Add(fileDto.StubTemplateId);
                }

                return true;
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing found stub row for StubID: {fileDto.StubId}, File: {fileDto.ArchivedFileFullPath}. E: {ex}");
                return false;
            }
        }

        private bool DeleteStubs(string siteUrl, Guid webId, string stubUrl)
        {
            var report = new JMStubDisposalJobDetails()
            {
                Url = stubUrl,
                //NodeType = "",
                Status = JobDetailsStatus.Successful,
            };

            try
            {
                if (!TryGetSiteRecordMapping(siteUrl, out var siteRecordPair))
                {
                    _logger.Error($"The site [{siteUrl}] not found in opus.");
                    report.Status = JobDetailsStatus.Failed;
                    report.Comment = "RM_TS_SCNotRegisterInOpus";
                    return false;
                }

                var (site, record) = siteRecordPair;
                var fileInfo = site.OpenWeb(webId).GetFile(stubUrl);
                if (!fileInfo.Exists)
                {
                    _logger.Warn($"The stub file at {stubUrl.LogBase64()} not found. It might be already deleted. Skip it and delete tracking record");
                    report.Status = JobDetailsStatus.Skipped;
                    report.Comment = "RM_MA_WF_NoElements";
                    return true;
                }

                try
                {
                    if (fileInfo.Item.IsRecord())
                    {
                        _logger.Info($"Undeclare record for item {stubUrl.LogBase64()} before deletion.");
                        record.UndeclareItemAsRecord(fileInfo.Item);
                    }
                    //item.SetComplianceTag(null, false, false, false, false);
                    if (fileInfo.Item.FieldValues.TryGetValue(SPColumnConstants.SP_ComplianceTag, out object value) 
                        && !string.IsNullOrEmpty(value?.ToString()))
                    {
                        _logger.Info($"Clear compliance tag for item {stubUrl.LogBase64()} before deletion. Current tag: {value}");
                        fileInfo.Item.SetComplianceTagOnBulkItems(string.Empty);
                    }
                    fileInfo.Delete();
                    _logger.Info($"Deleted stub file at {stubUrl.LogBase64()} successfully.");
                }
                catch (Exception e)
                {
                    _logger.Info($"delete file exception: {e.Message}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete stubs. Error: {e}");
                report.Status = JobDetailsStatus.Failed;
                report.Comment = e.Message;
                return false;
            }
            finally
            {
                SendReport(report);
            }
        }

        private void SendReport(JMStubDisposalJobDetails report)
        {
            report.FinishTime = DateTime.UtcNow.Ticks;
            switch (report.Status)
            {
                case JobDetailsStatus.Successful:
                case JobDetailsStatus.Skipped:
                    mJobHasSuccess = true;
                    break;
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Exception:
                    mJobHasException = true;
                    break;
                default:
                    break;
            }
            ReportManager.Increase();
            ReportManager.SendJobDetail(report);
        }

        private async Task BufferRecordForDeletionAsync(string rowKey, bool needDeleteIndexRecord = true)
        {
            var deleteEntity = new RMStubFileRecordTableEntity(_stubDisposalSiteInfoDto.Id.ToString(), rowKey)
            {
                RecordType = 0,
                ETag = ETag.All, // force delete
            };

            _bufferedDeleteRecords.Add(deleteEntity);

            if (needDeleteIndexRecord)
            {
                var indexEntity = new RMStubFileRecordTableEntity(_stubDisposalSiteInfoDto.Id.ToString(), rowKey.Split('_')[1])
                {
                    RecordType = 1,
                    ETag = ETag.All, // force delete
                };
                _bufferedDeleteRecords.Add(indexEntity);
            }

            if (_bufferedDeleteRecords.Count >= 100)
            {
                _logger.Info($"Deletion buffer reached {_bufferedDeleteRecords.Count} records, flushing to table.");
                await FlushDeletionBufferAsync();
            }
        }

        private async Task FlushDeletionBufferAsync()
        {
            if (_bufferedDeleteRecords.Count == 0) return;

            try
            {
                StubFileRecordDao.DeleteStubFileRecordEntities(TenantLocalValue.LogonGroupId, _bufferedDeleteRecords);

                _logger.Info($"Flushed deletion buffer. Deleted {_bufferedDeleteRecords.Count} records.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to flush deletion buffer. Records might remain in table as orphans. E: {ex.Message}");
            }
            finally
            {
                ReportManager.Increase(_bufferedDeleteRecords.Count);
                _bufferedDeleteRecords.Clear();
            }
        }

        private bool TryGetSiteRecordMapping(string siteUrl, out (IAveSite site, IAveORecords record) siteRecordPair)
        {
            siteRecordPair = (null, null);
            if (_siteCache.TryGetValue(siteUrl, out var value))
            {
                siteRecordPair = (value.site, value.record);
                return true;
            }

            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
            if (remoteSiteCollection == null)
            {
                _logger.Error($"The site [{siteUrl}] not found in opus db.");
                return false;
            }

            var bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
            var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            var site = aveObjectModelFactory.CreateSite(siteUrl);
            var record = aveObjectModelFactory.CreateRecords();
            _siteCache[siteUrl] = (site, record);
            siteRecordPair = (site, record);
            return true;
        }

        private async Task<string> GetTokenAsync()
        {
            if (_tokenResult != null && _tokenResult.ExpiresOn > DateTime.UtcNow.AddMinutes(10))
            {
                return _tokenResult.AccessToken;
            }

            var client = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
            _tokenResult = await client.ModernTokenService.GetTokenByAppProfileAsync(
                _profileInfo.Type,
                TokenResourceType.SharePoint,
                _profileInfo.TenantId,
                _profileInfo.Id,
                new Uri(_tenantConnectionInfo.AdminUrl).GetLeftPart(UriPartial.Authority),
                TokenType.ApplicationToken
            );
            return _tokenResult.AccessToken;
        }

        private DateTime CalculateStubRetentionDate(DateTime createdUtc, int retentionValue, DateUnit retentionUnit, bool isReverse = false)
        {
            try
            {
                if (isReverse)
                {
                    retentionValue = -retentionValue;
                }
                return retentionUnit switch
                {
                    DateUnit.Day => createdUtc.AddDays(retentionValue),
                    DateUnit.Week => createdUtc.AddDays(7 * retentionValue),
                    DateUnit.Month => createdUtc.AddMonths(retentionValue),
                    DateUnit.Year => createdUtc.AddYears(retentionValue),
                    _ => throw new ArgumentOutOfRangeException(nameof(retentionUnit))
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Error($"CalculateStubRetentionDate out of range. " +
                             $"CreatedUtc: {createdUtc}, RetentionValue: {retentionValue}, Unit: {retentionUnit}. Ex: {ex}");

                return !isReverse ? DateTime.MaxValue : DateTime.MinValue;
            }
            catch (Exception e)
            {
                _logger.Error($"Error calculating stub retention date. " +
                             $"CreatedUtc: {createdUtc}, RetentionValue: {retentionValue}, Unit: {retentionUnit}. E: {e}");
                return createdUtc;
            }
        }
    }
}
