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
using AngleSharp.Common;
using AngleSharp.Dom;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.IE;
using Cloud.Sdk.Telemetry.Data.Alita;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.General.Rot
{
    public class RMDiscoveryOffice365SiteRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteRotDataAnalyzer));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly int _containerId;

        private readonly int _siteId;

        private readonly Guid _siteUniqueId;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<Guid> _listIds;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly RMDiscoveryOffice365FileExtensionAnalysisManager _fileExtensionManager;

        private readonly bool _enableExpandQueryTest;

        public RMDiscoveryOffice365SiteRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource,
            int containerId,
            int siteId,
            Guid siteUniqueId,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            List<Guid> listIds,
            List<RMDiscoveryOffice365RuleInfo> rules,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionManager,
            bool enableExpandQueryTest
        )
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _containerId = containerId;
            _siteId = siteId;
            _siteUniqueId = siteUniqueId;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _listIds = listIds;
            _rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            _fileExtensionManager = fileExtensionManager;
            _enableExpandQueryTest = enableExpandQueryTest;
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteRuleLevelRotData> dataList)> AnalysisRuleLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteRuleLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] rule Level rot data.");
                }

                var dataList = await AnalysisRuleLevelDataAsync();

                await _dataDao.AddSiteRuleLevelRotDataListAsync(_o365TenantId, dataList);
               
                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteCategoryLevelRotData> dataList)> AnalysisCategoryLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteCategoryLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] category Level rot data.");
                }

                var dataList = await AnalysisCategoryLevelDataAsync();

                await _dataDao.AddSiteCategoryLevelRotDataListAsync(_o365TenantId, dataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteRootLevelRotData> dataList)> AnalysisRootLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteRootLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] root Level rot data.");
                }

                var dataList = await AnalysisRootLevelDataAsync();

                await _dataDao.AddSiteRootLevelRotDataListAsync(_o365TenantId, dataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data. Error: {e}");
                return (false, []);
            }
        }

        #region Rule Level
        private async Task<List<RMDiscoveryOffice365SiteRuleLevelRotData>> AnalysisRuleLevelDataAsync()
        {
            var rotRulesWithoutDuplicateColumns = _rules.ConvertAll(item => (
                item.Id,
                IsVersionRule: item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version,
                TagNames: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                ColumnName: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", "") + (item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version ? "/total_size" : ""),
                SumName: item.ToCustomColumn().Name
            )).ToList();

            var siteDataList = new List<RMDiscoveryOffice365SiteRuleLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId, ruleId) in QuerySiteRuleLevelDataObjects(rotRulesWithoutDuplicateColumns))
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = siteDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension &&
                        item.Rule == ruleId);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryOffice365SiteRuleLevelRotData
                        {
                            ContainerId = _containerId,
                            SiteId = _siteId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                            Rule = ruleId
                        };
                        siteDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return siteDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId, int ruleId)> QuerySiteRuleLevelDataObjects(List<(int Id, bool IsVersionRule, string TagName, string ColumnName, string SumName)> rotRulesWithoutDuplicateColumns)
        {

            foreach (var listId in _listIds)
            {
                foreach (var sizeRangeId in _sizeRangeIds)
                {
                    foreach (var dateRangeId in _dateRangeIds)
                    {
                        foreach (var (Id, IsVersionRule, TagName, ColumnName, SumName) in rotRulesWithoutDuplicateColumns)
                        {
                            List<ExpandoObject> dataObjList;
                            try
                            {
                                if (_enableExpandQueryTest)
                                {
                                    throw new Exception("Enable expand query test.");
                                }

                                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{_siteUniqueId}' " +
                                    $"and ListId eq '{listId}' " +
                                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                    $"and {ColumnName} gt 0 " +
                                    $"and not IsPHL)" +
                                    $"/groupby((FileExtension)," +
                                    $"aggregate($count as file_sum_count, {ColumnName} with sum as file_total_size))";
                                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                                dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                                _logger.Info($"End query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{Id}] rule level rot data.");
                            }
                            catch (Exception e)
                            {
                                _logger.Error($"An error occurred while query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{Id}] rule level rot data. Error: {e}");
                                dataObjList = await ExpandQuerySiteRuleLevelData(listId, sizeRangeId, dateRangeId, TagName, IsVersionRule);
                            }
                            yield return (dataObjList, sizeRangeId, dateRangeId, Id);
                        }
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> ExpandQuerySiteRuleLevelData(Guid listId, int sizeRangeId, int dateRangeId, string rotColumnName, bool isVersionRule)
        {
            _logger.Info($"Start expand query rule level rot data.");

            var dataDic = new Dictionary<string, IDictionary<string, object>>();

            var maxItemId = 0L;
            const int pageSize = 1000;

            for (var i = 0; ; i++)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{_siteUniqueId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileExtension, {rotColumnName}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    var fileExtension = item.GetValue("FileExtension");
                    if (!dataDic.TryGetValue(fileExtension, out var dataObj))
                    {
                        dataObj = new Dictionary<string, object>
                        {
                            { "_id", new Dictionary<string, object>{ { "FileExtension", fileExtension} }.ConvertToExpandoObject() },
                            { "file_sum_count", 0 },
                            { "file_total_size", 0 },
                        };
                        dataDic.Add(fileExtension, dataObj);
                    }

                    if (isVersionRule)
                    {
                        var dic = item as IDictionary<string, object>;
                        if (dic.ContainsKey(rotColumnName))
                        {
                            var columnExpandoObj = dic[rotColumnName];
                            var columnDataObj = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(columnExpandoObj));
                            dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + columnDataObj.GetValue<long>("total_size");
                            if (columnDataObj.GetValue<long>("total_size") <= 0)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>(rotColumnName);
                        if (item.GetValue<long>(rotColumnName) <= 0)
                        {
                            continue;
                        }
                    }

                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query rule level rot data.");
            return dataDic.Values.ConvertAll(item => item.ConvertToExpandoObject()).ToList();
        }
        #endregion

        #region Category Level

        private async Task<List<RMDiscoveryOffice365SiteCategoryLevelRotData>> AnalysisCategoryLevelDataAsync()
        {

            var siteDataList = new List<RMDiscoveryOffice365SiteCategoryLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId, category) in QuerySiteCategoryLevelDataObjects())
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = siteDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension &&
                        item.Category == category);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryOffice365SiteCategoryLevelRotData
                        {
                            ContainerId = _containerId,
                            SiteId = _siteId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                            Category = category
                        };
                        siteDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return siteDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId, RMDiscoveryRuleCategory category)> QuerySiteCategoryLevelDataObjects()
        {

            foreach (var listId in _listIds)
            {
                foreach (var sizeRangeId in _sizeRangeIds)
                {
                    foreach (var dateRangeId in _dateRangeIds)
                    {
                        foreach (var category in RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_COLUMN_MAPPING.Keys.ToList())
                        {
                            List<ExpandoObject> dataObjList;
                            var tagName = RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_COLUMN_MAPPING[category];
                            try
                            {

                                if (_enableExpandQueryTest)
                                {
                                    throw new Exception("Enable expand query test.");
                                }

                                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{_siteUniqueId}' " +
                                    $"and ListId eq '{listId}' " +
                                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                    $"and {tagName} gt 0 " +
                                    $"and not IsPHL)" +
                                    $"/groupby((FileExtension)," +
                                    $"aggregate($count as file_sum_count, {tagName} with sum as file_total_size))";
                                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                                dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                                _logger.Info($"End query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], category [{category}] category level rot data.");
                            }
                            catch (Exception e)
                            {
                                _logger.Error($"An error occurred while query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], category [{category}] category level rot data. Error: {e}");
                                dataObjList = await ExpandQuerySiteCategoryLevelData(listId, sizeRangeId, dateRangeId, tagName);
                            }
                            yield return (dataObjList, sizeRangeId, dateRangeId, category);
                        }
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> ExpandQuerySiteCategoryLevelData(Guid listId, int sizeRangeId, int dateRangeId, string categoryTagName)
        {
            _logger.Info($"Start expand query category level rot data.");

            var dataDic = new Dictionary<string, IDictionary<string, object>>();

            var maxItemId = 0L;
            const int pageSize = 1000;

            for (var i = 0; ; i++)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{_siteUniqueId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileExtension, {categoryTagName}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    var fileExtension = item.GetValue("FileExtension");
                    if (!dataDic.TryGetValue(fileExtension, out var dataObj))
                    {
                        dataObj = new Dictionary<string, object>
                        {
                            { "_id", new Dictionary<string, object>{ { "FileExtension", fileExtension} }.ConvertToExpandoObject() },
                            { "file_sum_count", 0 },
                            { "file_total_size", 0 },
                        };
                        dataDic.Add(fileExtension, dataObj);
                    }

                    dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>(categoryTagName);
                    if (item.GetValue<long>(categoryTagName) <= 0)
                    {
                        continue;
                    }

                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query category level rot data.");
            return dataDic.Values.ConvertAll(item => item.ConvertToExpandoObject()).ToList();
        }

        #endregion

        #region Root Level

        private async Task<List<RMDiscoveryOffice365SiteRootLevelRotData>> AnalysisRootLevelDataAsync()
        {

            var siteDataList = new List<RMDiscoveryOffice365SiteRootLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId) in QuerySiteRootLevelDataObjects())
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = siteDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryOffice365SiteRootLevelRotData
                        {
                            ContainerId = _containerId,
                            SiteId = _siteId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                        };
                        siteDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return siteDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId)> QuerySiteRootLevelDataObjects()
        {

            foreach (var listId in _listIds)
            {
                foreach (var sizeRangeId in _sizeRangeIds)
                {
                    foreach (var dateRangeId in _dateRangeIds)
                    {
                        List<ExpandoObject> dataObjList;
                        try
                        {

                            if (_enableExpandQueryTest)
                            {
                                throw new Exception("Enable expand query test.");
                            }

                            var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{_siteUniqueId}' " +
                                $"and ListId eq '{listId}' " +
                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                                $"and not IsPHL)" +
                                $"/groupby((FileExtension)," +
                                $"aggregate($count as file_sum_count, {RMDiscoveryBuildInRule.ROT_RULE_NAME} with sum as file_total_size))";
                            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                            dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                            _logger.Info($"End query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], root level rot data.");
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"An error occurred while query site [{_siteUniqueId}], list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], root level rot data. Error: {e}");
                            dataObjList = await ExpandQuerySiteRootLevelData(listId, sizeRangeId, dateRangeId);
                        }
                        yield return (dataObjList, sizeRangeId, dateRangeId);
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> ExpandQuerySiteRootLevelData(Guid listId, int sizeRangeId, int dateRangeId)
        {
            _logger.Info($"Start expand query root level rot data.");

            var dataDic = new Dictionary<string, IDictionary<string, object>>();

            var maxItemId = 0L;
            const int pageSize = 1000;

            for (var i = 0; ; i++)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{_siteUniqueId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileExtension, {RMDiscoveryBuildInRule.ROT_RULE_NAME}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    var fileExtension = item.GetValue("FileExtension");
                    if (!dataDic.TryGetValue(fileExtension, out var dataObj))
                    {
                        dataObj = new Dictionary<string, object>
                        {
                            { "_id", new Dictionary<string, object>{ { "FileExtension", fileExtension} }.ConvertToExpandoObject() },
                            { "file_sum_count", 0 },
                            { "file_total_size", 0 },
                        };
                        dataDic.Add(fileExtension, dataObj);
                    }

                    dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>(RMDiscoveryBuildInRule.ROT_RULE_NAME);
                    if (item.GetValue<long>(RMDiscoveryBuildInRule.ROT_RULE_NAME) <= 0)
                    {
                        continue;
                    }

                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query root level rot data.");
            return dataDic.Values.ConvertAll(item => item.ConvertToExpandoObject()).ToList();
        }

        #endregion
    }
}
