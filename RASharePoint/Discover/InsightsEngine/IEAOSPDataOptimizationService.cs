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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.SharePoint.Common;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AngleSharp.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;

namespace AvePoint.RA.SharePoint.Discover.InsightsEngine
{
    public class IEAOSPDataOptimizationService
    {
        private RALogger _logger = RALogger.GetInstance(typeof(IEDataOptimizationService));
        private IEApiClient _ieApiClient;
        private RMDiscoveryAOSPOptimizeDataSettingDto _dataOptimizeSettingDto;
        private bool _checkVersionRule = false;
        private string _office365TenantId;
        private List<RMDiscoveryAOSPFileExtension> _fileExtensions;
        private bool _needDoQuery = false;  // ArchivedDataType=All or has query rules
        private string _dataQueryFilters = "";
        private SourceFlag _sourceFlag;
        private IRMDiscoveryAOSPFileExtensionDao _FileExtensionDao = new RMDiscoveryAOSPFileExtensionDao();

        public IEAOSPDataOptimizationService(RMDiscoveryAOSPOptimizeDataSettingDto setting, bool checkVersionRule, SourceFlag sourceFlag)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _dataOptimizeSettingDto = setting;
            _office365TenantId = setting.O365TenantId;
            _checkVersionRule = checkVersionRule;
            _sourceFlag = sourceFlag;
            InitSettingsAsync().GetAwaiter().GetResult();
            _logger.Info($"Optimization setting is :{JsonConvert.SerializeObject(setting)}");
        }

        // itemUniqueId => ObjectId in DB
        public async Task<bool> TagAsArchivedAsync(string itemUniqueId)
        {
            var modifyTagModel = new ModifyTagModel
            {
                TagRuleId = RMDiscoveryBuildInRule.ARCHVIED_UNIQUE_ID,
                TagValue = 1
            };
            if (this._sourceFlag == SourceFlag.SharePoint)
            {
                return await _ieApiClient.SPDocumentTagService.ModifyAsync(_office365TenantId, itemUniqueId, modifyTagModel);
            }
            else if (this._sourceFlag == SourceFlag.OneDrive)
            {
                return await _ieApiClient.SPOneDriveDocumentTagService.ModifyAsync(_office365TenantId, itemUniqueId, modifyTagModel);
            }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllWebsAsync(string siteId)
        {
            if (!_needDoQuery) { return Enumerable.Empty<string>(); }

            var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[_sourceFlag]}?$apply=filter(SiteId eq '{siteId}' and NOT IsPHL{_dataQueryFilters})"
                + $"/groupby((WebId),aggregate($count as Count))";
            string dataJson = null;
            try
            {
                dataJson = await GetByODataUrlWithRetryAsync(odataUrl, _office365TenantId);
            }
            catch
            {
                _logger.Error($"Get AllWebs failed: {odataUrl}");
                throw;
            }

            var dataList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
            HashSet<string> webIDs = new HashSet<string>();
            if (dataList != null)
            {
                foreach (var data in dataList)
                {
                    var webId = (data.TryGet("_id") as ExpandoObject).TryGet("WebId")?.ToString();
                    webIDs.Add(webId);
                }
            }
            return webIDs;
        }

        public async Task<IEnumerable<string>> GetAllListsAsync(string siteId, string webId)
        {
            if (!_needDoQuery) { return Enumerable.Empty<string>(); }

            var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[_sourceFlag]}?$apply=filter(SiteId eq '{siteId}' and NOT IsPHL and WebId eq '{webId}'{_dataQueryFilters})"
                + $"/groupby((ListId),aggregate($count as Count))";
            string dataJson = null;
            try
            {
                dataJson = await GetByODataUrlWithRetryAsync(odataUrl, _office365TenantId);
            }
            catch
            {
                _logger.Error($"Get AllLists failed: {odataUrl}");
                throw;
            }
            var dataList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
            HashSet<string> listIDs = new HashSet<string>();
            if (dataList != null)
            {
                foreach (var data in dataList)
                {
                    var listId = GetValueFromIdData(data, "ListId")?.ToString();
                    listIDs.Add(listId);
                }
            }
            return listIDs;
        }

        public async IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> GetAllFilesAsync(string siteId, string webId, string listId, int pageSize = 1000)
        {
            if (_needDoQuery)
            {
                int offset = 0;
                long maxItemId = 0L;
                do
                {
                    //var (files, total) = await GetFilesAsync(siteId, webId, listId, offset, pageSize);
                    var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[_sourceFlag]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{siteId}' " +
                    $"and WebId eq '{webId}' " +
                    $"and ListId eq '{listId}' " +
                    $"{_dataQueryFilters} " +
                    $"and ItemId gt {maxItemId} " +
                    $"&$orderby=ItemId ";
                    string dataJson = null;
                    try
                    {
                        dataJson = await GetByODataUrlWithRetryAsync(odataUrl, _office365TenantId);
                    }
                    catch
                    {
                        _logger.Error($"Get AllFiles failed: {odataUrl}");
                        throw;
                    }
                    var results = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                    var dataList = results.TryGet("value") as List<object>;
                    List<RMDiscoveryFileDataInfo> files = new List<RMDiscoveryFileDataInfo>();
                    if (dataList != null && dataList.Count > 0)
                    {
                        var tempData = dataList.Last();
                        maxItemId = (int)GetValue<long>(tempData as ExpandoObject, "ItemId");
                        foreach (var item in dataList)
                        {
                            var dataObj = item as ExpandoObject;
                            var fileData = new RMDiscoveryFileDataInfo
                            {
                                Id = GetValue(dataObj, "Id"),
                                Name = GetValue(dataObj, "Name"),
                                SiteUrl = GetValue(dataObj, "SiteUrl"),
                                FullUrl = GetValue(dataObj, "FullUrl"),
                                FolderRelativeUrl = GetValue(dataObj, "FolderRelativeUrl"),
                                SiteId = GetValue(dataObj, "SiteId"),
                                WebId = GetValue(dataObj, "WebId"),
                                ListId = GetValue(dataObj, "ListId"),
                                FolderId = GetValue(dataObj, "FolderId"),
                                ItemId = (int)GetValue<long>(dataObj, "ItemId"),
                                ItemUniqueId = GetValue(dataObj, "ObjectId"),
                                FileExtension = GetValue(dataObj, "FileExtension"),
                                FileSize = GetValue<long>(dataObj, "FileSize"),
                                CurrentVersion = GetValue(dataObj, "CurrentVersion"),
                                HistoryVersionsCount = GetValue<int>(dataObj, "HistoryVersionsCount"),
                                HistoryVersionsSize = GetValue<int>(dataObj, "HistoryVersionsSize"),
                                AuthorId = GetValue<long>(dataObj, "AuthorId"),
                                EditorId = GetValue<long>(dataObj, "EditorId"),
                                CreatedTime = GetValue<DateTime>(dataObj, "CreatedTime"),
                                ModifiedTime = GetValue<DateTime>(dataObj, "ModifiedTime"),
                            };


                            if (_checkVersionRule)
                            {
                                var versionObjs = dataObj.TryGet("Versions") as List<object>;
                                if (versionObjs != null && versionObjs.Count > 0)
                                {
                                    fileData.Versions = new List<RMDiscoveryFileVersionDataInfo>();
                                    foreach (var versionObj in versionObjs)
                                    {
                                        var versionData = versionObj as ExpandoObject;
                                        fileData.Versions.Add(new()
                                        {
                                            Version = GetValue(versionData, "Version"),
                                            VersionSize = GetValue<long>(versionData, "VersionSize"),
                                            CreatedTime = GetValue<DateTime>(versionData, "CreatedTime"),
                                            ModifiedTime = GetValue<DateTime>(versionData, "ModifiedTime"),
                                        });
                                    }
                                }
                            }

                            files.Add(fileData);
                        }
                    }
                    yield return files;

                    offset += files.Count;

                    if (files.Count < pageSize || files.Count == 0)
                    {
                        break;
                    }
                } while (true);
            }
            else
            {
                yield return new List<RMDiscoveryFileDataInfo>();
            }
        }



        private async Task InitSettingsAsync()
        {
            List<string> filters = new List<string>();

            filters.Add($"{RMDiscoveryBuildInRule.ARCHVIED_COLUMN_NAME} ne 1");

            if (_dataOptimizeSettingDto.WithoutDateQueryParameter != null)
            {
                if (_dataOptimizeSettingDto.WithoutDateQueryParameter.From > 0)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} gt {_dataOptimizeSettingDto.WithoutDateQueryParameter.From}");
                }
                filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} le {_dataOptimizeSettingDto.WithoutDateQueryParameter.To}");
            }

            if (_dataOptimizeSettingDto.FileExtensionQueryParameter?.FileExtensions != null
                && _dataOptimizeSettingDto.FileExtensionQueryParameter?.FileExtensions.Count > 0)
            {
                _fileExtensions = await _FileExtensionDao.GetAsync(new Guid(_office365TenantId), _dataOptimizeSettingDto.FileExtensionQueryParameter.FileExtensions);
                if (_fileExtensions.Count > 0)
                {
                    List<string> tempNameList = new List<string>();
                    bool hasEmptyExtenstion = false;
                    foreach (var fileExten in _fileExtensions)
                    {
                        if (fileExten.Name == "RM_FA_FileType_Empty")
                        {
                            hasEmptyExtenstion = true;
                        }
                        else
                        {
                            tempNameList.Add(fileExten.Name.EscapeSpecialCharacters());
                        }
                    }
                    string extensionString = $"FileExtension in ('{string.Join("','", tempNameList)}')";
                    if (hasEmptyExtenstion)
                    {
                        extensionString = '(' + extensionString + " or FileExtension eq '')";
                    }
                    filters.Add(extensionString);
                }
            }

            if (_dataOptimizeSettingDto.SizeRangeQueryParameter != null
                && _dataOptimizeSettingDto.SizeRangeQueryParameter.QueryMode != RMDiscoveryAOSPSizeRangeQueryMode.None
                && _dataOptimizeSettingDto.SizeRangeQueryParameter.SizeRange > 0)
            {
                var rangeId = _dataOptimizeSettingDto.SizeRangeQueryParameter.SizeRange;
                var condition = _dataOptimizeSettingDto.SizeRangeQueryParameter.QueryMode switch
                {
                    RMDiscoveryAOSPSizeRangeQueryMode.LessThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} le {rangeId}",
                    RMDiscoveryAOSPSizeRangeQueryMode.GenerateThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} ge {rangeId}",
                    _ => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {rangeId}"
                };

                filters.Add(condition);
            }

            if (_dataOptimizeSettingDto.ArchiveDataType == (int)ArchiverDataType.Special)
            {
                bool hasDocumentRule = false;
                bool hasVersionRule = false;
                List<RMDiscoveryRuleDefinition> rules = new List<RMDiscoveryRuleDefinition>();
                if (_checkVersionRule && _dataOptimizeSettingDto.InactiveRule != null && _dataOptimizeSettingDto.InactiveRule.Count > 0)
                {
                    hasVersionRule = true;
                    rules.AddRange(_dataOptimizeSettingDto.InactiveRule);
                }
                if (_dataOptimizeSettingDto.ROTRule != null && _dataOptimizeSettingDto.ROTRule.Count > 0)
                {
                    if (_checkVersionRule)
                    {
                        var versionRules = _dataOptimizeSettingDto.ROTRule.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version);
                        if (versionRules.Any())
                        {
                            hasVersionRule = true;
                            rules.AddRange(versionRules);
                        }
                    }
                    else
                    {
                        var docRules = _dataOptimizeSettingDto.ROTRule.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document);
                        if (docRules.Any())
                        {
                            hasDocumentRule = true;
                            rules.AddRange(docRules);
                        }
                    }
                }
                if ((_checkVersionRule ? hasVersionRule : hasDocumentRule) && rules.Count > 0)
                {
                    _needDoQuery = true;
                    var ruleTagColumns = rules.ConvertAll(r => ($"tags_{r.UniqueId.ToString().ToLower().Replace("-", "")}", r));
                    var ruleFilters = ruleTagColumns.Select(r => $"{(r.Item2.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version ? $"{r.Item1}/count" : r.Item1)} gt 0");

                    filters.Add($"({string.Join(" or ", ruleFilters)})");
                }
            }
            else
            {
                _needDoQuery = true;
            }

            if (filters.Count > 0)
            {
                _dataQueryFilters = $" and {string.Join(" and ", filters)}";
            }
            _logger.Info($"optimization filter info:{_dataQueryFilters}");
        }

        private object GetValueFromIdData(ExpandoObject data, string columnName)
        {
            return (data.TryGet("_id") as ExpandoObject).TryGet(columnName);
        }
        private string GetValue(ExpandoObject data, string key, string defaultValue = null)
        {
            var res = data.TryGet(key);
            if (res == null)
            {
                return defaultValue;
            }
            return res.ToString();
        }
        private T GetValue<T>(ExpandoObject data, string key, T defaultValue = default) where T : struct
        {
            var res = data.TryGet<T>(key);
            if (res == null)
            {
                return defaultValue;
            }
            return res.Value;
        }
        private async Task<string> GetByODataUrlWithRetryAsync(string odataUrl, string office365TenantId)
        {
            string dataJson = string.Empty;
            try
            {
                dataJson = await _ieApiClient.GetByODataUrlAsync(odataUrl, office365TenantId);
                return dataJson;
            }
            catch (Exception ex)
            {
                _logger.Error($"query data by OData Url error ,will retry,error:{ex.ToString()}");
                int i = 1;
                int retryCount = 5;
                while (i <= retryCount)
                {
                    try
                    {
                        dataJson = await _ieApiClient.GetByODataUrlAsync(odataUrl, office365TenantId);
                        _logger.Info("retry success,will return dataJson");
                        return dataJson;
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"query data by OData Url error , retry,retry {i},error:{e.ToString()}");
                        await Task.Delay(5000);
                        i++;
                    }
                }
                _logger.Error("query data by OData Url error ,retry over,throw");
                throw;
            }
        }
    }
}
