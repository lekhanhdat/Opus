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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using Merged18NResources.MediaServiceArchiverBackup;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using RAArchiverCommon;
using RAArchiverCommon.Utility;
using RecordsHotfixMaintenanceService;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ConvertStub
{
    public class ConvertStubJobHandler : ApplicationModelServiceBase
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(ConvertStubJobHandler));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private readonly IArchiverIndexSubInfoDao _archiveIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private readonly IRMRuleDao _ruleDao = PlatformWindsorManager.GetService<IRMRuleDao>();
        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private IMCacheSettingService _cacheSettingService;
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_cacheSettingService == null)
                {
                    _cacheSettingService = new CacheSettingService();
                    return _cacheSettingService;
                }
                else
                {
                    return _cacheSettingService;
                }
            }
        }
        private HashSet<string> _remoteNodeSet = [];
        private Dictionary<string, ArchiverIndexService> _scAndIndexServiceDic = [];
        private Dictionary<string, ArchiverConvertStubIndexService> _scAndConvertStubServiceDic = [];
        private JobContext _jobContext = null;
        private string _jobId = string.Empty;
        private JobType _jobType;
        private ScheduleConfiguration _configuration;
        private IXSystem _indexLogicalDevice;
        private StorageDeviceDto _indexDeviceDto;
        private AppProfileInfo _profileInfo;
        private TenantConnectionInfo _tenantConnectionInfo;
        private TokenResult _tokenResult;
        private LeaveStubType _possiblyStubType = LeaveStubType.None;
        private readonly Dictionary<LeaveStubType, string> _stubTypeSuffixMappings = new()
        {
            { LeaveStubType.Aspx, LinkFileCommon.GetStubFileNameSuffixWithDot(LeaveStubType.Aspx) },
            { LeaveStubType.Html, LinkFileCommon.GetStubFileNameSuffixWithDot(LeaveStubType.Html) },
            { LeaveStubType.Txt, LinkFileCommon.GetStubFileNameSuffixWithDot(LeaveStubType.Txt) },
            { LeaveStubType.Link, LinkFileCommon.GetStubFileNameSuffixWithDot(LeaveStubType.Link) },
        };

        private Dictionary<string, StubSiteNode> _siteNodeCache = [];
        private string _mainJobSiteUrl = string.Empty;
        private int _fileProcessCount = 1000;
        private ConvertStubDto _convertStubDto = null;
        private const int BATCH_SIZE = 25;

        public ConvertStubJobHandler(string jobId, JobType jobType)
        {
            _jobType = jobType;
            _jobId = jobId;
            _jobContext = JobContext.GetInstance(_jobId, _jobType);
            //_jobContext.ReportManager.Increase();
            _jobContext.ReportManager.StartUpdateJobProgress(60);
            _configuration = new ScheduleConfiguration(_jobId);
            _configuration.JobReportDto = new JobReportImps(_jobContext.ReportManager);
            _configuration.ProgressDto = _configuration.JobReportDto;
            _configuration.IsConvertStubJob = true;
            SOArchiverJobInfoStatistics.Instance.MainJobStartTime = _jobContext.MainJobStartTime;
            _convertStubDto = SerializerHelper.DeserializeByDataContractSerializer<ConvertStubDto>(_jobContext.JobContextSetting);
            InitVirtualRule(_convertStubDto.StubTemplateId.ToString(), _convertStubDto.StubType);
            InitConfiguration(_convertStubDto.NodeSetting);
        }

        public void InitVirtualRule(string stubTemplateId, LeaveStubType stubType)
        {
            var stubSetting = LinkFileCommon.GetStubTemplatesByIdAsync(stubTemplateId).Result;
            var virtualRule = new Rule()
            {
                Id = Guid.NewGuid().ToString(),
                LeaveStubType = (LeaveStubType)stubSetting.StubType,
                StubTemplateId = stubTemplateId,
                StubTemplateName = stubSetting.Name,
                LeaveStubMessage = stubSetting.StubContent,
                DeclareStubOption = AccountUtility.IsSupportRecordLabel() ? (stubSetting.IsDeclareStubAsRecords ? DeclareStubType.AddRecordLabel : DeclareStubType.DeleteRecordLabel) : (stubSetting.IsDeclareStubAsRecords ? DeclareStubType.Declare : DeclareStubType.UnDeclare),
                DeclareLinkFile = stubSetting.IsDeclareStubAsRecords,
                KeepDataOption = (int)KeepDataOption.ArchiveAndLeaveStub
            };
            if (stubSetting.IsEnabledRetention)
            {
                virtualRule.LeaveStubIsEnabledRetention = true;
                virtualRule.LeaveStubRetentionValue = stubSetting.RetentionValue;
                virtualRule.LeaveStubRetentionUnit = stubSetting.RetentionUnit;
            }
            _configuration.currentRule = virtualRule;
            _configuration.NeedConvertStubType = stubType;
            _configuration.isConvertSameTypeStub = _configuration.currentRule.LeaveStubType == _configuration.NeedConvertStubType;
            s_logger.Info($"InitVirtualRule. NeedConvertStubType:{stubType}, new stub type:{virtualRule.LeaveStubType}");
        }

        private void InitConfiguration(RMSPTreeNode treeNode)
        {
            var node = RMDtoConverter.ConvertRMTree2SPTree(treeNode);
            var siteNode = SPTreeNodeManagement.GetSiteCollectionNode(node);
            //var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
            _mainJobSiteUrl = siteNode.Url;
            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteNode.Url);
            if (remoteSiteCollection == null)
            {
                s_logger.Error($"The site [{siteNode.Url}] not found in opus db.");
                throw new Exception("RM_RDM_SCNotFound");
            }
            InitTenantConnection(remoteSiteCollection);
            var bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
            var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteNode.Url, bposInfo, AveContextKind.ClientObjectModel);

            var aveSPSite = new AveSPSite(siteNode.Url, AveContextKind.ClientObjectModel, bposInfo, null);

            var stubSiteNode = new StubSiteNode()
            {
                AveSPSite = aveSPSite,
                AveObjectModelFactory = aveObjectModelFactory,
                SiteUrl = siteNode.Url,
                CurrentFoundStubFileCount = 0,
            };
            _siteNodeCache.TryAdd(siteNode.Url, stubSiteNode);

            _configuration.siteUrlSchemeAndHost = new Uri(siteNode.Url).Scheme + @"://" + new Uri(siteNode.Url).Authority;
            _configuration.user = bposInfo;
        }

        public async Task RunAsync()
        {
            try
            {
                await using var multiLocker = new SampleDBLockerFactory(_jobId);
                Open();
                await ProcessNodeSetAsync(_remoteNodeSet, multiLocker);
            }
            catch (JobStopException stop)
            {
                s_logger.Error(stop.ToString());
                _configuration.JobReportDto.HasStop = true;
                throw;
            }
            catch (ScheduleJobConfigurationError configError)
            {
                s_logger.Error(configError.ToString());
                _configuration.JobReportDto.HasErrorNode = true;
                _configuration.JobReportDto.summaryComments = configError.Message;
            }
            catch (Exception e)
            {
                s_logger.Error(e.ToString());
                _configuration.JobReportDto.HasErrorNode = true;
                if (e.Message.StartsWith("Token Result is null"))
                {
                    s_logger.Error("Token Result is null.it means that o365 is expired");
                    _configuration.JobReportDto.summaryComments = "RM_AR_TokenResult_Null";
                }
                else
                {
                    _configuration.JobReportDto.summaryComments = e.Message;
                }
            }
            finally
            {
                Close();
                if (_configuration.JobReportDto != null)
                {
                    _configuration.JobReportDto.FinishReport();
                }
                else
                {
                    //jobContext.ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                }
            }
        }

        private async Task ProcessNodeSetAsync(HashSet<string> remoteNodeSet, SampleDBLockerFactory multiLocker)
        {
            foreach (string scUrl in remoteNodeSet)
            {
                bool isMainSite = string.Equals(scUrl, _mainJobSiteUrl, StringComparison.OrdinalIgnoreCase);
                bool hasAcquiredLock = false;

                try
                {
                    if (isMainSite)
                    {
                        s_logger.Info($"Get locker for main job site collection {scUrl} to update index.");
                        hasAcquiredLock = await multiLocker.TryAcquire4IndexDBUpdaterAsync(scUrl, "", TimeSpan.FromHours(1));
                        if (!hasAcquiredLock) // This site is already locked which mean it being processed by another job.
                        {
                            s_logger.Warn($"Failed to acquire lock for site collection {scUrl}. It may be processed by another job. Skip processing this site collection.");
                            _configuration.JobReportDto.AddRecordReport(scUrl, ConvertStubAction.None, JobDetailsStatus.Skipped, "RM_JM_Retention_IndexLock");
                            continue;
                        }
                    }

                    OpenObjectSiteCollectionIndex(scUrl);
                    if (!_scAndConvertStubServiceDic.TryGetValue(scUrl, out _))
                    {
                        s_logger.Warn($"The stub index is not open for the site: {scUrl}");
                        continue;
                    }
                    await ProgressSiteCollection(scUrl);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (ScheduleJobConfigurationError)
                {
                    throw;
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occured while processing site collection {scUrl}. E: {e.Message}");
                    var errorMessage = e.Message;
                    if (e.Message.StartsWith("Token Result is null"))
                    {
                        s_logger.Error("Token Result is null.it means that o365 is expired");
                        errorMessage = "RM_AR_TokenResult_Null";
                    }
                    _configuration.JobReportDto.AddRecordReport(scUrl, ConvertStubAction.None, JobDetailsStatus.Failed, errorMessage);
                }
                finally
                {
                    CloseSiteIndex(scUrl);
                    if (isMainSite && hasAcquiredLock)
                    {
                        await multiLocker.ReleaseAsync(_mainJobSiteUrl);
                    }
                }
            }
        }

        private async Task ProgressSiteCollection(string siteCollectionUrl)
        {
            s_logger.Info($@"Start progress sc:{siteCollectionUrl}");
            ArchiverConvertStubIndexService _archiverConvertStubIndexService = _scAndConvertStubServiceDic[siteCollectionUrl];
            var pageOffset = 0;
            var pageCount = _fileProcessCount;
            var headIndexes = _archiverConvertStubIndexService.GetAllHeadIndex();
            Dictionary<string, ArchiverBasicIndex> pathLookup = headIndexes.ToDictionary(r => r.PathMD5, r => r);
            using var _ = new PerformanceScope("ConvertStubJobHandler:ProgressSiteCollection", $"Progress Site {siteCollectionUrl}", true);
            while (true)
            {
                s_logger.Info($"Search file for convert stub. Count:{pageCount}, index:{pageOffset}");
                try
                {
                    using (new CheckJobStopScope()) { }
                    var searchResult = _archiverConvertStubIndexService.SearchForConvertStub(_configuration.NeedConvertStubType.ToString(), pageCount, pageOffset);
                    if (searchResult == null || searchResult.Count == 0)
                    {
                        s_logger.Warn("No search results found after search.");
                        break;
                    }

                    s_logger.Info($"Found files with match stub type. Count:{searchResult.Count}");
                    pageOffset += searchResult.Count;
                    var webGroups = ConvertStubUtility.GroupIndexesByWebUrl(pathLookup, searchResult);

                    foreach (var webGroup in webGroups)
                    {
                        s_logger.Info($"Group found file by web. WebUrl:{webGroup.Key}, count:{webGroup.Value.Count}");
                        pageOffset -= await ProcessSearchResultByBatch(webGroup.Value, webGroup.Key, siteCollectionUrl);
                        //pageOffset -= await ProcessSearchResult(webGroup.Value, webGroup.Key, siteCollectionUrl);
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    s_logger.Error(e.ToString());
                    _configuration.JobReportDto.HasErrorNode = true;
                    _configuration.JobReportDto.summaryComments = e.Message;
                    throw;
                }
                finally
                {
                }
            }
            

            foreach (var siteNode in _siteNodeCache.Values)
            {
                if (siteNode.CurrentFoundStubFileCount > 0)
                {
                    s_logger.Info($"Start Convert Stub for Site {siteNode.SiteUrl}. FileCount: {siteNode.CurrentFoundStubFileCount}.");
                    await StartConvertStubForSite(siteNode);
                }
            }
            _siteNodeCache = _siteNodeCache.Where(group => group.Key == _mainJobSiteUrl).ToDictionary();
            s_logger.Info($@"End progress sc:{siteCollectionUrl}");
        }


        private async Task<int> ProcessSearchResult(List<ArchiverBasicIndex> searchResult, string webUrl, string scUrl)
        {
            var updatedRecordCount = 0;
            using var _ = new PerformanceScope("ConvertStubJobHandler:ProcessSearchResult", $"Process search result for web {webUrl}", true);
            foreach (var fileIndex in searchResult)
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    s_logger.Info($"TryGetStubInfo for file: {fileIndex.NodeGuid} , StubInfo: {fileIndex.stubInfo}");
                    var (hasStub, stubId, stubType) = ConvertStubUtility.TryGetStubInfo(fileIndex.stubInfo);
                    if (!hasStub)
                    {
                        s_logger.Warn($"Cannot get stub info for file {fileIndex.Url}, stub: {fileIndex.stubInfo}");
                        _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubFileNotFound"));
                        continue;
                    }

                    using var ps = new PerformanceScope("ConvertStubJobHandler:SearchStubs", $"Search stubs for archived file {fileIndex.NodeGuid}", true);
                    if (await SearchStubsByStubIdAsync(stubId, stubType, fileIndex) | TryGetStubsByStubType(stubType, webUrl, scUrl, fileIndex))
                    {
                        s_logger.Info($"Found stubs for recordId:{fileIndex.Id}, FileId: {fileIndex.NodeGuid} , StubId: {stubId}, StubType: {stubType}");
                        _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Successful);
                    }
                    else
                    {
                        s_logger.Warn($"Not found stub for recordId:{fileIndex.Id}, FileId: {fileIndex.NodeGuid} , StubId: {stubId}, StubType: {stubType}");
                        _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubFileNotFound"));
                    }

                    await GetRuleNameByJobIdAsync(fileIndex);

                    foreach (var siteNode in _siteNodeCache.Values)
                    {
                        if (siteNode.CurrentFoundStubFileCount >= _fileProcessCount)
                        {
                            s_logger.Info($"SiteNode {siteNode.SiteUrl} reach count: {siteNode.CurrentFoundStubFileCount}, process count: {_fileProcessCount}. Start processing");
                            updatedRecordCount += await StartConvertStubForSite(siteNode);
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occured while searching stub for file: {fileIndex.Url}. E: {e}");
                    _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Failed, e.Message);
                    continue;
                }
            }
            

            if (_configuration.isConvertSameTypeStub)
            {
                s_logger.Error($"Same stub stype, need full offset. updatedRecordCount: {updatedRecordCount} reset.");
                updatedRecordCount = 0;
            }
            return updatedRecordCount;
        }

        private async Task<int> ProcessSearchResultByBatch(List<ArchiverBasicIndex> searchResult, string webUrl, string scUrl)
        {
            var updatedRecordCount = 0;

            using var _ = new PerformanceScope("ConvertStubJobHandler:ProcessSearchResult", $"Process {searchResult.Count} items", true);
            var filesNeedPathCheck = new List<ArchiverBasicIndex>();

            foreach (var batch in searchResult.Chunk(BATCH_SIZE))
            {
                using (new CheckJobStopScope()) { }

                await GetRuleNameByJobIdsAsync(batch.Select(i => i.JobId));
                foreach (var fileIndex in batch)
                {
                    if (!fileIndex.HasStub)
                    {
                        s_logger.Warn($"Cannot get stub info for file {fileIndex.Url}, stub: {fileIndex.stubInfo}");
                        _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubFileNotFound"));
                        continue;
                    }

                    if (string.IsNullOrEmpty(fileIndex.StubId))
                    {
                        s_logger.Info($"File {fileIndex.NodeGuid}, StubType: {fileIndex.LeaveStubType} has no StubId, need check by path.");
                        filesNeedPathCheck.Add(fileIndex);
                        continue;
                    }
                }

                var notFoundFiles = await SearchStubsBatchAsync(batch);

                filesNeedPathCheck.AddRange(notFoundFiles);

                if (filesNeedPathCheck.Count >= BATCH_SIZE)
                {
                    await TryGetStubsByStubTypeBatchAsync(filesNeedPathCheck, webUrl, scUrl);
                    filesNeedPathCheck.Clear();
                }

                foreach (var siteNode in _siteNodeCache.Values)
                {
                    if (siteNode.CurrentFoundStubFileCount >= _fileProcessCount)
                    {
                        s_logger.Info($"SiteNode {siteNode.SiteUrl} reach count: {siteNode.CurrentFoundStubFileCount}, process count: {_fileProcessCount}. Start processing");
                        updatedRecordCount += await StartConvertStubForSite(siteNode);
                    }
                }
            }

            if (filesNeedPathCheck.Count != 0)
            {
                await TryGetStubsByStubTypeBatchAsync(filesNeedPathCheck, webUrl, scUrl);
                filesNeedPathCheck.Clear();
            }

            if (_configuration.isConvertSameTypeStub)
            {
                s_logger.Error($"Same stub stype, need full offset. updatedRecordCount: {updatedRecordCount} reset.");
                updatedRecordCount = 0;
            }
            return updatedRecordCount;
        }

        private async Task<IEnumerable<ArchiverBasicIndex>> SearchStubsBatchAsync(IEnumerable<ArchiverBasicIndex> stubFiles)
        {
            if (stubFiles == null || !stubFiles.Any()) return stubFiles;

            var foundStubIds = new Dictionary<string, string>(); // stubId, path

            try
            {
                var query = string.Join(" OR ", stubFiles.Select(id => $"\"{id.StubId}\""));
                if (string.IsNullOrEmpty(query))
                {
                    s_logger.Warn("Empty query for SearchStubsBatchAsync.");
                    return stubFiles;
                }

                using var _ = new PerformanceScope("ConvertStubJobHandler:SearchStubsBatchAsync", $"Batch search stubs by StubId", true);

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
                    s_logger.Info($"Batch search found {resultRows.Count} results.");

                    foreach (var row in resultRows)
                    {
                        string summary = row["HitHighlightedSummary"] != null ? row["HitHighlightedSummary"].ToString() : string.Empty;
                        var filePath = row["Path"] != null ? row["Path"].ToString() : string.Empty;
                        var matchedStub = stubFiles.FirstOrDefault(i => i.HasStub && summary.Contains(i.StubId));

                        if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                        {
                            ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedStub);
                            continue;
                        }

                        // fallback to Path matching
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            var originalPath = filePath.Substring(0, filePath.LastIndexOf('.'));
                            var matchedPathIndex = stubFiles.FirstOrDefault(f => string.Equals(f.Url, originalPath, StringComparison.OrdinalIgnoreCase));
                            if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                            {
                                ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedPathIndex);
                                continue;
                            }
                        }

                        // fallback to Title matching
                        if (!string.IsNullOrEmpty(row["Title"]?.ToString()))
                        {
                            var title = row["Title"].ToString();
                            var matchedTitleIndex = stubFiles.FirstOrDefault(f => string.Equals(f.Name, title, StringComparison.OrdinalIgnoreCase)); // should check if more than 1 file has same name ?
                            if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                            {
                                ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedTitleIndex);
                                continue;
                            }
                        }

                        s_logger.Warn($"Found a search result at [{filePath.LogBase64()}] but could not map back to any StubID in the batch via Summary. Summary content: {summary.Substring(0, Math.Min(summary.Length, 100))}...");
                    }
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while SearchStubsBatchAsync. E: {e}");
            }

            var notFoundFiles = new List<ArchiverBasicIndex>();
            foreach (var kvp in stubFiles)
            {
                if (!foundStubIds.ContainsKey(kvp.StubId))
                {
                    notFoundFiles.Add(kvp);
                }
            }

            return notFoundFiles;
        }

        private void ValidateAndProcessMatchStub(Dictionary<string, string> foundStubIds, IDictionary<string, object> row, string filePath, ArchiverBasicIndex matchedStub)
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
                s_logger.Warn($"StubId {matchedStub.StubId} with same path {filePath.LogBase64()} hit multiple times in search results");
                return;
            }

            ProcessFoundStubRow(row, matchedStub, matchedStub.StubId, isFirstResult);
            _configuration.JobReportDto.AddRecordReport(matchedStub.Url, ConvertStubAction.Scan, JobDetailsStatus.Successful);
        }

        private void ProcessFoundStubRow(IDictionary<string, object> row, ArchiverBasicIndex fileIndex, string stubId, bool isFirstResult)
        {
            try
            {
                var siteName = row["SiteName"].ToString();
                var webId = new Guid(row["WebID"].ToString());
                var listId = new Guid(row["ListId"].ToString());
                var uniqueId = new Guid(row["UniqueId"].ToString());
                var fileExtension = row["FileExtension"].ToString();
                var filePath = row["Path"].ToString();

                // Validate Site
                if (!string.Equals(siteName, _mainJobSiteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    s_logger.Warn($"Found stub file but in different site. siteName:{siteName}, current FileUrl: {filePath}, original FileUrl:{fileIndex.Url}");
                }

                // Validate Type
                if (!string.Equals(fileExtension, LinkFileCommon.GetStubFileNameSuffix(fileIndex.LeaveStubType), StringComparison.OrdinalIgnoreCase))
                {
                    s_logger.Warn($"Found stub file but not the same archived stub type. fileExtension:{fileExtension}, StubStype:{fileIndex.LeaveStubType}, FileUrl:{fileIndex.Url}");
                    return;
                }

                // Cache stubSiteNode
                if (!_siteNodeCache.TryGetValue(siteName, out var stubSiteNode))
                {
                    var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteName);
                    if (remoteSiteCollection == null)
                    {
                        s_logger.Warn($"The site [{siteName}] not found in opus db");
                        _siteNodeCache[siteName] = null;
                    }
                    else
                    {
                        var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteName, _configuration.user, AveContextKind.ClientObjectModel);
                        stubSiteNode = new StubSiteNode()
                        {
                            AveSPSite = new AveSPSite(siteName, AveContextKind.ClientObjectModel, _configuration.user, null),
                            SiteUrl = siteName,
                            AveObjectModelFactory = aveObjectModelFactory,
                            CurrentFoundStubFileCount = 0,
                        };
                        _siteNodeCache[siteName] = stubSiteNode;
                    }
                }

                // Re validate stubSiteNode
                if (stubSiteNode == null)
                {
                    var newPath = Path.ChangeExtension(filePath, LinkFileCommon.GetStubFileNameSuffixWithDot(_configuration));
                    _configuration.JobReportDto.AddRecordReport(newPath, ConvertStubAction.Create, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_TS_SCNotRegister"));
                    s_logger.Warn($"File [{fileIndex.Url}] found in site not synced to Opus. Skip it");
                    return;
                }

                // Cache ListNode/FileNode
                if (stubSiteNode.StubListNodeCache.TryGetValue(listId, out var listNode))
                {
                    if (!listNode.StubFileNodeCache.ContainsKey(uniqueId))
                    {
                        listNode.StubFileNodeCache.Add(uniqueId, new StubFileNode()
                        {
                            UniqueId = uniqueId,
                            FileIndex = fileIndex,
                            IsSkipUpdateIndex = !isFirstResult
                        });
                    }
                }
                else
                {
                    stubSiteNode.StubListNodeCache[listId] = new StubListNode()
                    {
                        ListId = listId,
                        WebId = webId,
                        StubFileNodeCache = new()
                        {
                            { uniqueId,
                                new StubFileNode()
                                {
                                    UniqueId = uniqueId,
                                    FileIndex = fileIndex,
                                    IsSkipUpdateIndex = false,
                                }
                            }
                        }
                    };
                }

                stubSiteNode.CurrentFoundStubFileCount++;
                s_logger.Info($"Found stub for recordId:{fileIndex.Id}, FileId: {fileIndex.NodeGuid} at {filePath}");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Error processing found stub row for StubID: {stubId}, File: {fileIndex.Url}. E: {ex}");
            }
        }

        private async Task TryGetStubsByStubTypeBatchAsync(List<ArchiverBasicIndex> files, string webUrl, string scUrl)
        {
            if (files == null || files.Count == 0) return;

            try
            {
                using var _ = new PerformanceScope("ConvertStubJobHandler:TryGetStubsByStubTypeBatch", $"Check direct path for {files.Count} files", true);

                if (!_siteNodeCache.TryGetValue(_mainJobSiteUrl, out var stubSiteNode) || stubSiteNode == null)
                {
                    s_logger.Warn($"Site not found in Cache or is null. SiteUrl: [{_mainJobSiteUrl}]");
                    return;
                }

                var webRelativeUrl = AveUrlUtility.GetServerRelativeUrl(webUrl);
                var web = webUrl.Equals(scUrl, StringComparison.OrdinalIgnoreCase)
                          ? stubSiteNode.AveSPSite.SPSite.RootWeb
                          : stubSiteNode.AveSPSite.SPSite.OpenWeb(webRelativeUrl);

                if (web == null || !web.Exists)
                {
                    s_logger.Warn($"Web not found or does not exist. WebUrl: [{webUrl}]");
                    return;
                }

                var pathsToSend = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var fileTypesMap = new Dictionary<string, List<LeaveStubType>>();

                foreach (var file in files)
                {
                    var typesToCheck = GetTypesToCheckForFile(file);
                    fileTypesMap[file.NodeGuid] = typesToCheck;

                    foreach (var type in typesToCheck)
                    {
                        // Generate Path
                        string path = file.Url.Replace(scUrl, _mainJobSiteUrl) + LinkFileCommon.GetStubFileNameSuffixWithDot(type);
                        pathsToSend.Add(path);
                    }
                }

                // [Path] -> (UniqueId, ListId)
                var foundStubsLookup = web.GetStubNodesByBatchPath(pathsToSend.ToList());

                foreach (var fileIndex in files)
                {
                    bool isFound = false;
                    var typesToCheck = fileTypesMap[fileIndex.NodeGuid];

                    foreach (var type in typesToCheck)
                    {
                        string stubPath = fileIndex.Url.Replace(scUrl, _mainJobSiteUrl) + LinkFileCommon.GetStubFileNameSuffixWithDot(type);

                        if (foundStubsLookup.TryGetValue(stubPath, out var stubInfo))
                        {
                            isFound = true;
                            //if (_possiblyStubType != type) _possiblyStubType = type;
                            AddStubToCache(stubSiteNode, fileIndex, stubInfo.UniqueId, stubInfo.ListId, web.ID);
                            _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Successful);
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        s_logger.Warn($"Not found stub for FileId: {fileIndex.NodeGuid}, StubId: {fileIndex.StubId}");
                        _configuration.JobReportDto.AddRecordReport(fileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubFileNotFound"));
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"Batch Check Type Error: {ex}");
            }
        }

        private List<LeaveStubType> GetTypesToCheckForFile(ArchiverBasicIndex fileIndex)
        {
            var typesToCheck = new List<LeaveStubType>();

            if (fileIndex.LeaveStubType != LeaveStubType.None)
            {
                ConvertStubUtility.TryAddStubTypeToList(typesToCheck, fileIndex.LeaveStubType);
            }
            else
            {
                // if (_possiblyStubType != LeaveStubType.None) typesToCheck.Add(_possiblyStubType);
                foreach (var item in _stubTypeSuffixMappings)
                {
                    ConvertStubUtility.TryAddStubTypeToList(typesToCheck, item.Key);
                }
            }
            return typesToCheck;
        }

        private void AddStubToCache(StubSiteNode stubSiteNode, ArchiverBasicIndex fileIndex, Guid stubUniqueId, Guid listId, Guid webId)
        {
            if (!stubSiteNode.StubListNodeCache.TryGetValue(listId, out var listNode))
            {
                listNode = new StubListNode
                {
                    ListId = listId,
                    WebId = webId,
                    StubFileNodeCache = []
                };
                stubSiteNode.StubListNodeCache[listId] = listNode;
            }

            if (!listNode.StubFileNodeCache.ContainsKey(stubUniqueId))
            {
                listNode.StubFileNodeCache.Add(stubUniqueId, new StubFileNode
                {
                    UniqueId = stubUniqueId,
                    FileIndex = fileIndex,
                    IsSkipUpdateIndex = false
                });
            }

            stubSiteNode.CurrentFoundStubFileCount++;
        }

        private async Task GetRuleNameByJobIdsAsync(IEnumerable<string> jobIds)
        {
            foreach (var jobId in jobIds)
            {
                if (!_configuration.RuleNameByJobIdDic.TryGetValue(jobId, out _))
                {
                    s_logger.Info($"start get rule name by job Id, JobId: {jobId}");
                    var ruleName = "";
                    var subInfo = await _archiveIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(jobId);
                    if (subInfo != null && !string.IsNullOrEmpty(subInfo.RuleId))
                    {
                        var rule = _ruleDao.GetRuleById(new Guid(subInfo.RuleId));
                        if (rule != null && !string.IsNullOrEmpty(rule.RuleName))
                        {
                            ruleName = rule.RuleName;
                        }
                    }

                    _configuration.RuleNameByJobIdDic[jobId] = ruleName;
                    s_logger.Info($"add new rule name to cache, JobId: {jobId}, rule name: {ruleName}");
                }
            }
        }

        private async Task<int> StartConvertStubForSite(StubSiteNode siteNode)
        {
            try
            {
                using var _ = new PerformanceScope("ConvertStubJobHandler:StartConvertStubForSite", $"Process convert stub for site {siteNode.SiteUrl}", true);
                s_logger.Info($"Start Convert Stub for Site {siteNode.SiteUrl}");
                var siteProcessor = new ConvertStubSiteProcessor(_configuration, siteNode, _scAndConvertStubServiceDic);
                return await siteProcessor.Run();
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while StartConvertStubForSite {siteNode.SiteUrl}. E: {e}");
            }
            finally
            {
                _configuration.StubCache.Clear();
                siteNode.CurrentFoundStubFileCount = 0;
                foreach (var stubListNode in siteNode.StubListNodeCache.Values)
                {
                    stubListNode.StubFileNodeCache.Clear();
                }
            }

            return 0;
        }

        private async Task GetRuleNameByJobIdAsync(ArchiverBasicIndex fileIndex)
        {
            s_logger.Info($"start get rule name by job Id, JobId: {fileIndex.JobId}");

            if (!_configuration.RuleNameByJobIdDic.TryGetValue(fileIndex.JobId, out _))
            {
                var ruleName = "";
                var subInfo = await _archiveIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(fileIndex.JobId);
                if (subInfo != null && !string.IsNullOrEmpty(subInfo.RuleId))
                {
                    var rule = _ruleDao.GetRuleById(new Guid(subInfo.RuleId));
                    if (rule != null && !string.IsNullOrEmpty(rule.RuleName))
                    {
                        ruleName = rule.RuleName;
                    }
                }

                _configuration.RuleNameByJobIdDic[fileIndex.JobId] = ruleName;
                s_logger.Info($"add new rule name to cache, JobId: {fileIndex.JobId}, rule name: {ruleName}");
            }
        }

        private void Open()
        {
            _indexDeviceDto = _storageDeviceService.GetIndexDevice();
            if (_indexDeviceDto == null)
            {
                throw new Exception("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
            }

            StorageDeviceManager ??= new StorageDeviceManager();

            var indexLogicalDevive = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(_indexDeviceDto);
            _indexLogicalDevice = StorageDeviceManager.Open(indexLogicalDevive.GetXRIS(PhysicalDeviceUsage.Index));
            InitAndOpenCacheManager();

            FoundNeedCheckSC();
            //foreach (string scUrl in FoundNeedCheckSC())
            //{
            //    OpenObjectSiteCollectionIndex(indexDeviceDto, scUrl);
            //}
        }

        private IEnumerable<string> FoundNeedCheckSC()
        {
            var siteMappings = _restoreSiteMappingDao.GetSiteMappingsByTargetSCUrl(_mainJobSiteUrl);
            _remoteNodeSet = siteMappings.Select(map => map.SourceSiteUrl).Append(_mainJobSiteUrl).ToHashSet();
            return _remoteNodeSet;
        }

        private void OpenObjectSiteCollectionIndex(string siteCollectionUrl)
        {
            ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo()
            {
                IndexVolume = new ArchiverVolumeGenerator().GenerateIndexVolume(new VolumeParameter() { FarmName = string.Empty, SiteCollectionUrl = siteCollectionUrl, }),
                Path = siteCollectionUrl,
                EndTime = DateTime.MaxValue.Ticks,
                SiteUrl = siteCollectionUrl,
                TreeMode = Media.Service.DomainModel.TreeMode.SiteCollectionMode,
                IndexLogicalDevice = _indexDeviceDto,
                CacheSetting = CacheSettingService.GetBrowserCacheInfo(),
            };
            var openParam = new ArchiverIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, _indexLogicalDevice)
            {
                WaitIndexLockerTimeOutInMs = 3000,
                IndexDatabaseName = ServiceConstants.IndexDBName,
                CacheSetting = browseInfo.CacheSetting
            };
            try
            {
                ArchiverIndexService _indexService = new ArchiverIndexService()
                {
                    IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                    IndexSynchronizer = new IndexDatabaseSynchronizer()
                };
                _indexService.Open(openParam);

                _scAndIndexServiceDic.Add(siteCollectionUrl, _indexService);

                var convertStubIndexService = new ArchiverConvertStubIndexService() { HeadAndBodyService = new ArchiverHeadAndBodyIndexService() { IndexProcessor = _scAndIndexServiceDic[siteCollectionUrl].IndexProcessor } };

                var processCount = convertStubIndexService.SearchCountForConvertStub(_configuration.NeedConvertStubType.ToString());
                if (processCount == 0)
                {
                    s_logger.Info($"No file need convert stub for site collection: {siteCollectionUrl}");
                    return;
                }

                s_logger.Info($"SiteCollectionUrl: {siteCollectionUrl}, ProcessCount: {processCount}");
                _jobContext.ReportManager.IncreaseBase(processCount * 3); // scan , create and delete
                //_configuration.JobReportDto.ReportManager.IncreaseBase(processCount * 3);

                _scAndConvertStubServiceDic[siteCollectionUrl] = convertStubIndexService;
            }
            catch (Exception e)
            {
                if (e is IndexCanNotFoundException || e.Message.Equals(MediaServiceArchiverBackupResource.ArchiverIndexServiceOpenIndexCanNotFoundException))
                {
                    _configuration.JobReportDto.AddRecordReport(siteCollectionUrl, ConvertStubAction.Scan, JobDetailsStatus.Skipped, I18NEntity.GetString(MediaServiceArchiverBackupResource.ArchiverIndexServiceOpenIndexCanNotFoundException));
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> SearchStubsByStubIdAsync(string stubId, string stubType, ArchiverBasicIndex fileIndex)
        {
            var stubFound = false;
            var isNotFirstResult = false;
            try
            {
                using var _ = new PerformanceScope("ConvertStubJobHandler:SearchStubsByStubIdAsync", $"Search stub by id: {stubId} for file: {fileIndex.NodeGuid}", true);
                if (string.IsNullOrEmpty(stubId))
                {
                    return stubFound;
                }
                using var searchContext = new ClientContext(_tenantConnectionInfo.AdminUrl);
                var token = await GetTokenAsync();
                searchContext.ExecutingWebRequest += (sender, e) => e.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + token;
                var keywordQuery = new KeywordQuery(searchContext);
                keywordQuery.SelectProperties.Add("SiteName");
                keywordQuery.SelectProperties.Add("WebID");
                keywordQuery.SelectProperties.Add("ListId");
                keywordQuery.SelectProperties.Add("UniqueId");
                keywordQuery.SelectProperties.Add("FileExtension");
                keywordQuery.SelectProperties.Add("Path");
                keywordQuery.TrimDuplicates = false;
                keywordQuery.RowLimit = 1000;
                keywordQuery.StartRow = 0;
                keywordQuery.EnableSorting = true;
                keywordQuery.Culture = 1033;
                keywordQuery.QueryText = stubId;
                var searchExecutor = new SearchExecutor(searchContext);
                var results = searchExecutor.ExecuteQuery(keywordQuery);
                await searchContext.ExecuteQueryAsync();
                var result = results.Value[0].ResultRows.ToList();
                foreach (var row in result)
                {
                    var siteName = row["SiteName"].ToString();
                    var webId = new Guid(row["WebID"].ToString());
                    var listId = new Guid(row["ListId"].ToString());
                    var uniqueId = new Guid(row["UniqueId"].ToString());
                    var fileExtension = row["FileExtension"].ToString();
                    var filePath = row["Path"].ToString();

                    if (!string.Equals(siteName, _mainJobSiteUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        s_logger.Warn($"Found stub file but in different site. siteName:{siteName}, current FileUrl: {filePath}, original FileUrl:{fileIndex.Url}");
                    }

                    if (!string.Equals(fileExtension, LinkFileCommon.GetStubFileNameSuffix(fileIndex.LeaveStubType), StringComparison.OrdinalIgnoreCase))
                    {
                        s_logger.Warn($"Found stub file but not the same archived stub type. fileExtension:{fileExtension}, StubStype:{stubType}, FileUrl:{fileIndex.Url}");
                        continue;
                    }

                    if (!_siteNodeCache.TryGetValue(siteName, out var stubSiteNode))
                    {
                        var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteName);
                        if (remoteSiteCollection == null)
                        {
                            s_logger.Warn($"The site [{siteName}] not found in opus db");
                            _siteNodeCache[siteName] = null;
                        }
                        else
                        {
                            var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteName, _configuration.user, AveContextKind.ClientObjectModel);
                            stubSiteNode = new StubSiteNode()
                            {
                                AveSPSite = new AveSPSite(siteName, AveContextKind.ClientObjectModel, _configuration.user, null),
                                SiteUrl = siteName,
                                AveObjectModelFactory = aveObjectModelFactory,
                                CurrentFoundStubFileCount = 0,
                            };
                            _siteNodeCache[siteName] = stubSiteNode;
                        }
                    }

                    if (stubSiteNode == null)
                    {
                        var newPath = Path.ChangeExtension(filePath, LinkFileCommon.GetStubFileNameSuffixWithDot(_configuration));
                        _configuration.JobReportDto.AddRecordReport(newPath, ConvertStubAction.Create, JobDetailsStatus.Skipped, I18NEntity.GetString("RM_TS_SCNotRegister"));
                        stubFound = true;
                        s_logger.Warn($"File [{fileIndex.Url}] found in site not synced to Opus. Skip it");
                        continue;
                    }

                    if (stubSiteNode.StubListNodeCache.TryGetValue(listId, out var listNode))
                    {

                        listNode.StubFileNodeCache.Add(uniqueId, new StubFileNode()
                        {
                            UniqueId = uniqueId,
                            FileIndex = fileIndex,
                            IsSkipUpdateIndex = isNotFirstResult,
                        });
                    }
                    else
                    {
                        stubSiteNode.StubListNodeCache[listId] = new StubListNode()
                        {
                            ListId = listId,
                            WebId = webId,
                            StubFileNodeCache = new()
                            {
                                { uniqueId,
                                    new StubFileNode()
                                    {
                                        UniqueId = uniqueId,
                                        FileIndex = fileIndex,
                                        IsSkipUpdateIndex = isNotFirstResult,
                                    }
                                }
                            }
                        };
                    }
                    stubFound = true;
                    stubSiteNode.CurrentFoundStubFileCount++;
                    isNotFirstResult = true;
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while SearchStubsByStubIdAsync. StubId:{stubId}, FileId:{fileIndex.NodeGuid} E: {e}");
                stubFound = false;
            }

            return stubFound;
        }

        private bool TryGetStubsByStubType(string stubType, string webUrl, string scUrl, ArchiverBasicIndex fileIndex)
        {
            s_logger.Info($"Not found stubs for file {fileIndex.NodeGuid} by stubId. TryGetStubsByStubType stubType:{stubType}");
            var stubFound = false;
            try
            {
                using var _ = new PerformanceScope("ConvertStubJobHandler:TryGetStubsByStubType", $"Search stub by type: {stubType} for file: {fileIndex.NodeGuid}", true);
                if (!_siteNodeCache.TryGetValue(_mainJobSiteUrl, out var stubSiteNode))
                {
                    s_logger.Warn($"Site not found in Cache. SiteUrl: [{_mainJobSiteUrl}]");
                    return stubFound;
                }

                var webRelativeUrl = AveUrlUtility.GetServerRelativeUrl(webUrl);
                var web = webUrl.Equals(scUrl, StringComparison.OrdinalIgnoreCase) ? stubSiteNode.AveSPSite.SPSite.RootWeb : stubSiteNode.AveSPSite.SPSite.OpenWeb(webRelativeUrl);
                if (!web.Exists)
                {
                    s_logger.Warn($"Web not found. WebUrl: [{webUrl}], relativeUrl: [{webRelativeUrl}]");
                    return stubFound;
                }

                var possiblyStubTypes = new List<LeaveStubType>();
                if (!string.IsNullOrEmpty(stubType) && Enum.TryParse(stubType, true, out LeaveStubType recordStubType))
                {
                    ConvertStubUtility.TryAddStubTypeToList(possiblyStubTypes, recordStubType);
                }
                else
                {
                    if (_possiblyStubType != LeaveStubType.None)
                    {
                        possiblyStubTypes.Add(_possiblyStubType);
                    }

                    foreach (var item in _stubTypeSuffixMappings)
                    {
                        ConvertStubUtility.TryAddStubTypeToList(possiblyStubTypes, item.Key);
                    }
                }

                foreach (var possiblyType in possiblyStubTypes)
                {
                    try
                    {
                        string fileUrl = fileIndex.Url.Replace(scUrl, _mainJobSiteUrl);
                        var aveFile = web.GetFile(fileUrl + LinkFileCommon.GetStubFileNameSuffixWithDot(possiblyType));

                        if (aveFile.Exists)
                        {
                            s_logger.Info($"The file exists and found.");
                            if (_possiblyStubType != possiblyType)
                            {
                                s_logger.Info($"Switch the most possibly stub type from {_possiblyStubType} to {possiblyType}");
                                _possiblyStubType = possiblyType;   // try get stub at first by this type next time
                            }

                            if (stubSiteNode.StubListNodeCache.TryGetValue(aveFile.Item.ParentList.ID, out var listNode))
                            {
                                listNode.StubFileNodeCache[aveFile.Item.UniqueId] = new StubFileNode
                                {
                                    UniqueId = aveFile.Item.UniqueId,
                                    FileIndex = fileIndex,
                                };
                            }
                            else
                            {
                                stubSiteNode.StubListNodeCache[aveFile.Item.ParentList.ID] = new StubListNode()
                                {
                                    ListId = aveFile.Item.ParentList.ID,
                                    WebId = aveFile.Web.ID,
                                    StubFileNodeCache = new()
                                    {
                                        { aveFile.Item.UniqueId,
                                            new StubFileNode()
                                            {
                                                UniqueId = aveFile.Item.UniqueId,
                                                FileIndex = fileIndex,
                                            }
                                        }
                                    }
                                };
                            }
                            stubFound = true;
                            stubSiteNode.CurrentFoundStubFileCount++;
                            break;
                        }
                        else
                        {
                            //如果文件没有找到，打印出信息查看是不是方法实际转换的路径不对
                            s_logger.Info($"The file path is {fileUrl}. PossiblyType: {possiblyType}. scUrl: {scUrl}. mainJobSiteUrl {_mainJobSiteUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"An error occurred while getting stub. StubType:{possiblyType}, FileId:{fileIndex.NodeGuid} E: {ex}");
                    }
                }
                

            }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while SearchStubsByStubType. StubType:{stubType}, FileId:{fileIndex.NodeGuid} E: {e}");
                stubFound = false;
            }

            return stubFound;
        }

        #region support methods

        private void InitTenantConnection(RemoteSiteCollection remoteSiteCollection)
        {
            if (remoteSiteCollection == null)
            {
                s_logger.Error($"The site not found in opus db.");
                throw new Exception("RM_RDM_SCNotFound");
            }

            var o365TenantId = remoteSiteCollection.TenantId;
            _profileInfo = PoolUserUtil.GetBPOSInfoAsync(o365TenantId).GetAwaiter().GetResult();
            if (_profileInfo == null)
            {
                s_logger.Error($"The site [{remoteSiteCollection.url}] no app profile found.");
                throw new Exception("RM_JM_AppProfile_NotFoundError");
            }

            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            _tenantConnectionInfo = client.TenantManagementService.GetByTenantIdAsync(o365TenantId).GetAwaiter().GetResult();
            if (_tenantConnectionInfo == null)
            {
                s_logger.Error($"The site [{remoteSiteCollection.url}] no tenant info found in AOS.");
                throw new Exception("RM_JM_Archive_TenantRemoveFromAOS_ErrorMessage");
            }
        }

        public void InitAndOpenCacheManager()
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", "convertStub"),
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };

            var cacheSetting = new CacheSettingDto
            {
                Extension = new CacheSettingExtension { Path = new List<PathMap>() { new PathMap() { DiskInfo = disk } } },
                LimitFreeSpace = 1024 * 1024 * 1024,//1 GB
            };

            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            CacheManager.Open(cacheSetting, _indexLogicalDevice.IsDirectSystem);
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

        private void Close()
        {
            try
            {
                foreach (var group in _scAndIndexServiceDic)
                {
                    group.Value?.Close();
                    if (group.Key == _mainJobSiteUrl)
                    {
                        var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                        group.Value?.IndexSynchronizer?.Upload(dbInfo);
                        s_logger.Info($@"Success up index db, sc:{group.Key}");
                    }
                    else
                    {
                        s_logger.Info($@"Skip up index db, sc:{group.Key}");
                    }
                }
                _indexLogicalDevice?.Close();
                CacheManager?.Close();
            }
            catch (Exception ex)
            {
                s_logger.Warn(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceCloseException, ex.ToString());
            }
        }

        private void CloseSiteIndex(string siteUrl)
        {
            if (string.IsNullOrEmpty(siteUrl)) return;
            try
            {
                if (_scAndIndexServiceDic.TryGetValue(siteUrl, out var indexService) && indexService != null)
                {
                    indexService.Close();
                    if (siteUrl == _mainJobSiteUrl)
                    {
                        var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                        indexService.IndexSynchronizer?.Upload(dbInfo);
                        s_logger.Info($@"Success up index db, sc:{siteUrl}");
                    }
                    else
                    {
                        s_logger.Info($@"Skip up index db, sc:{siteUrl}");
                    }
                }

                _scAndIndexServiceDic.Remove(siteUrl);
            }
            catch (Exception ex)
            {
                s_logger.Warn(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceCloseException, ex.ToString());
            }
        }

        #endregion
    }
}
