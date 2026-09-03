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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Cloud.Sdk.Data.AosModern;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCStubManager
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMDeleteArchivedSCStubManager));
        private string _jobId;
        private JobType _jobType;
        private string _siteUrl;
        private bool _needDeleteStub;
        private IAveSite _aveSite = null;
        private IAveORecords _aveRecord = null;
        private AppProfileInfo _profileInfo;
        private TenantConnectionInfo _tenantConnectionInfo;
        private TokenResult _tokenResult;

        private IDeleteArchivedSCIndexService _deleteArchivedSCIndexService;

        private readonly Dictionary<string, string> _webMapping = [];
        private readonly List<ArchiverBasicIndex> _searchStubByIdCache = [];
        private readonly Dictionary<string, bool> _foundSiteCollectionCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly RMDeleteArchivedSCJobReportManager _reportManager;

        public RMDeleteArchivedSCStubManager(RMDeleteArchivedSCJobReportManager reportManager)
        {
            _reportManager = reportManager;
            _jobId = _reportManager.JobId;
            _jobType = _reportManager.JobType;
        }

        public async Task InitAsync(string siteUrl, IDeleteArchivedSCIndexService deleteArchivedSCIndexService)
        {
            try
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    _logger.Error($"The site url is null or empty. Skip init stub manager");
                    return;
                }
                _siteUrl = siteUrl;

                if (deleteArchivedSCIndexService == null)
                {
                    _logger.Error($"The deleteArchivedSCIndexService is null. Skip init stub manager");
                    return;
                }
                _deleteArchivedSCIndexService = deleteArchivedSCIndexService;

                var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(_siteUrl);
                if (remoteSiteCollection == null)
                {
                    _logger.Error($"The site [{_siteUrl}] not found in opus db.");
                    // if not found, just skip process delete stub
                    _needDeleteStub = false;
                    _foundSiteCollectionCache[_siteUrl] = false;
                    return;
                }

                _foundSiteCollectionCache[_siteUrl] = true;

                InitTenantConnection(remoteSiteCollection);

                var bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(_siteUrl, bposInfo, AveContextKind.ClientObjectModel);
                _aveSite = aveObjectModelFactory.CreateSite(_siteUrl);
                _aveRecord = aveObjectModelFactory.CreateRecords();
                _needDeleteStub = true;
            }
            catch (Exception e)
            {
                _needDeleteStub = false;
                _logger.Error($"Error occurred while initializing stub manager site collection: {_siteUrl}. {e}");
            }

        }

        public async Task ProcessDeleteStub(ArchiverBasicIndex basicIndex)
        {
            if (!_needDeleteStub || !basicIndex.HasStub)
            {
                return;
            }

            if (basicIndex.HasStub && !string.IsNullOrEmpty(basicIndex.StubId))
            {
                _searchStubByIdCache.Add(basicIndex);
                if (_searchStubByIdCache.Count >= 25)
                {
                    _logger.Info($"Search stub by id cache count: {_searchStubByIdCache.Count}");
                    var notFoundStubs = await SearchStubsBatchAsync(_searchStubByIdCache);

                    foreach (var stubBasicIndex in notFoundStubs)
                    {
                        var webUrl = GetWebRelativeUrl(stubBasicIndex.ParentPathMD5);
                        var filePath = stubBasicIndex.Url + LinkFileCommon.GetStubFileNameSuffixWithDot(stubBasicIndex.LeaveStubType);
                        if (DeleteStubs(_siteUrl, webUrl, filePath))
                        {
                            // process delete record in table
                            //var rowKey = $"{file.RefDateTime:yyyyMMddHHmmss}_{file.ArchivedItemId:N}";
                            //await BufferRecordForDeletionAsync(rowKey);
                            //_processedTemplateIds.Add(file.StubTemplateId);
                        }
                    }
                    _searchStubByIdCache.Clear();
                }
            }
            else
            {
                var webUrl = GetWebRelativeUrl(basicIndex.ParentPathMD5);
                var filePath = basicIndex.Url + LinkFileCommon.GetStubFileNameSuffixWithDot(basicIndex.LeaveStubType);
                if (DeleteStubs(_siteUrl, webUrl, filePath))
                {
                    // process delete record in table
                    //var rowKey = $"{file.RefDateTime:yyyyMMddHHmmss}_{file.ArchivedItemId:N}";
                    //await BufferRecordForDeletionAsync(rowKey);
                    //_processedTemplateIds.Add(file.StubTemplateId);
                }
            }
        }

        public async Task FlushAsync()
        {
            if (!_needDeleteStub)
            {
                return;
            }

            _logger.Info($"Flushing stub cache for jobId: {_jobId}, siteUrl: {_siteUrl}. Cache count: {_searchStubByIdCache.Count}");
            if (_searchStubByIdCache.Count == 0)
            {
                return;
            }
            _logger.Info($"Search stub by id cache count: {_searchStubByIdCache.Count}");
            try
            {
                var notFoundStubs = await SearchStubsBatchAsync(_searchStubByIdCache);

                foreach (var stubBasicIndex in notFoundStubs)
                {
                    var webUrl = GetWebRelativeUrl(stubBasicIndex.ParentPathMD5);
                    var filePath = stubBasicIndex.Url + LinkFileCommon.GetStubFileNameSuffixWithDot(stubBasicIndex.LeaveStubType);
                    if (DeleteStubs(_siteUrl, webUrl, filePath))
                    {
                        // process delete record in table
                        //var rowKey = $"{file.RefDateTime:yyyyMMddHHmmss}_{file.ArchivedItemId:N}";
                        //await BufferRecordForDeletionAsync(rowKey);
                        //_processedTemplateIds.Add(file.StubTemplateId);
                    }
                }
                _searchStubByIdCache.Clear();
                _foundSiteCollectionCache.Clear();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while flushing stub cache for jobId: {_jobId}, siteUrl: {_siteUrl}. Error: {e}");
            }
            finally
            {
                _reportManager.IncreaseProgress();
            }
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
                    _logger.Warn("Empty query for SearchStubsBatchAsync.");
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
                    _logger.Info($"Batch search found {resultRows.Count} results.");

                    foreach (var row in resultRows)
                    {
                        string summary = row["HitHighlightedSummary"] != null ? row["HitHighlightedSummary"].ToString() : string.Empty;
                        var filePath = row["Path"] != null ? row["Path"].ToString() : string.Empty;
                        var matchedStub = stubFiles.FirstOrDefault(i => i.HasStub && summary.Contains(i.StubId));

                        if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                        {
                            await ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedStub);
                            continue;
                        }

                        // fallback to Path matching
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            var originalPath = filePath.Substring(0, filePath.LastIndexOf('.'));
                            var matchedPathIndex = stubFiles.FirstOrDefault(f => string.Equals(f.Url, originalPath, StringComparison.OrdinalIgnoreCase));
                            if (matchedStub != null && !string.IsNullOrEmpty(matchedStub.StubId))
                            {
                                await ValidateAndProcessMatchStub(foundStubIds, row, filePath, matchedPathIndex);
                                continue;
                            }
                        }

                        _logger.Warn($"Found a search result at [{filePath.LogBase64()}] but could not map back to any StubID in the batch via Summary. Summary content: {summary.Substring(0, Math.Min(summary.Length, 100))}...");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured while SearchStubsBatchAsync. E: {e}");
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

        private async Task ValidateAndProcessMatchStub(Dictionary<string, string> foundStubIds, IDictionary<string, object> row, string filePath, ArchiverBasicIndex matchedStub)
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

        private async Task<bool> ProcessFoundStubRowAsync(IDictionary<string, object> row, ArchiverBasicIndex fileIndex, bool isFirstResult)
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
                if (!string.Equals(siteName, _siteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warn($"Found stub file but in different site. siteName:{siteName}, current FileUrl: {filePath}, original FileUrl:{fileIndex.Url}");
                    if (!_foundSiteCollectionCache.TryGetValue(siteName, out var isExist))
                    {
                        var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteName);
                        isExist = remoteSiteCollection != null;
                        _foundSiteCollectionCache[siteName] = isExist;
                    }

                    if (!isExist)
                    {
                        _logger.Warn($"The site [{siteName}] not found in opus db");
                        return false;
                    }
                }

                // Validate Type
                if (!string.Equals(fileExtension, LinkFileCommon.GetStubFileNameSuffix(fileIndex.LeaveStubType), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warn($"Found stub file but not the same archived stub type. fileExtension:{fileExtension}, StubStype:{fileIndex.LeaveStubType}, FileUrl:{fileIndex.Url}");
                    return false;
                }

                _logger.Info($"Found stub for recordId:{fileIndex.Id}, FileId: {fileIndex.NodeGuid} at {filePath}");

                if (DeleteStubs(siteName, webId, filePath) && isFirstResult)
                {
                    // process delete record in table
                    //var rowKey = $"{fileDto.RefDateTime:yyyyMMddHHmmss}_{fileDto.ArchivedItemId:N}";
                    //await BufferRecordForDeletionAsync(rowKey);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing found stub row for StubID: {fileIndex.StubId}, File: {fileIndex.Url}. E: {ex}");
                return false;
            }
        }

        private bool DeleteStubs(string siteUrl, Guid webId, string stubUrl)
        {
            try
            {
                var fileInfo = _aveSite.OpenWeb(webId).GetFile(stubUrl);
                if (!fileInfo.Exists)
                {
                    _logger.Warn($"The stub file at {stubUrl.LogBase64()} not found. It might be already deleted. Skip it and delete tracking record");
                    return true;
                }

                try
                {
                    if (fileInfo.Item.IsRecord())
                    {
                        _logger.Info($"Undeclare record for item {stubUrl.LogBase64()} before deletion.");
                        _aveRecord.UndeclareItemAsRecord(fileInfo.Item);
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
                return false;
            }
        }

        private bool DeleteStubs(string siteUrl, string webUrl, string stubUrl)
        {
            try
            {
                var fileInfo = _aveSite.OpenWeb(webUrl).GetFile(stubUrl);
                if (!fileInfo.Exists)
                {
                    _logger.Warn($"The stub file at {stubUrl.LogBase64()} not found. It might be already deleted. Skip it and delete tracking record");
                    return true;
                }

                try
                {
                    if (fileInfo.Item.IsRecord())
                    {
                        _logger.Info($"Undeclare record for item {stubUrl.LogBase64()} before deletion.");
                        _aveRecord.UndeclareItemAsRecord(fileInfo.Item);
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
                return false;
            }
        }

        private void InitTenantConnection(RemoteSiteCollection remoteSiteCollection)
        {
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

        public string GetWebRelativeUrl(string containerPathMd5)
        {
            if (_webMapping.ContainsKey(containerPathMd5))
            {
                return _webMapping[containerPathMd5];
            }

            var containerPathMd5s = new List<string>() { containerPathMd5 };
            string webRelativeUrl;
            while (true)
            {
                var containerItem = _deleteArchivedSCIndexService.GetContainerItem(containerPathMd5);
                if (containerItem == null)
                {
                    //webRelativeUrl = string.Empty;
                    //break;
                    throw new Exception($"Container item not found for path md5: {containerPathMd5}");
                }
                if (containerItem.Type == "W")
                {
                    webRelativeUrl = containerItem.Name == "." ? "" : containerItem.Name;
                    break;
                }
                if (_webMapping.ContainsKey(containerPathMd5))
                {
                    webRelativeUrl = _webMapping[containerPathMd5];
                    break;
                }

                containerPathMd5s.Add(containerItem.ParentPathMD5);
                containerPathMd5 = containerItem.ParentPathMD5;
            }

            foreach (var md5 in containerPathMd5s)
            {
                _webMapping[md5] = webRelativeUrl;
            }

            return _webMapping[containerPathMd5];
        }
    }
}
