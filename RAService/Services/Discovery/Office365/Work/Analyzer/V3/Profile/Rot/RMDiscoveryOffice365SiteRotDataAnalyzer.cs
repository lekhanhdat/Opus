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
using Aspose.Email.Clients.Exchange.WebService;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile.Rot
{
    public class RMDiscoveryOffice365SiteRotDataAnalyzer
    {
        private const string DISCOVERY_MULTIPLE_RULES_ANALYSIS_METHOD = "DISCOVERY_MULTIPLE_RULES_ANALYSIS_METHOD";

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteRotDataAnalyzer));

        private readonly IRMKeyValueDao _keyValueDao;

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly IRMDiscoveryOffice365ProfileDataDao _profileDataDao;

        private readonly IEApiClient _ieApiClient;

        private readonly Guid _o365TenantId;

        private readonly RMDiscoveryOffice365SiteInfo _siteInfo;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<string> _fileExtensions;

        private readonly RMDiscoveryOffice365SiteRotDataAnalysisTestMethod _testMethod;

        public RMDiscoveryOffice365SiteRotDataAnalyzer(
            Guid o365TenantId,
            RMDiscoveryOffice365SiteInfo siteInfo,
            RMDiscoveryOffice365ProfileInfo profileInfo,
            List<RMDiscoveryOffice365RuleInfo> rules,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            List<RMDiscoveryOffice365FileExtension> fileExtensions
            ) 
        {
            _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _profileDataDao = new RMDiscoveryOffice365ProfileDataDao();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _o365TenantId = o365TenantId;
            _siteInfo = siteInfo;
            _profileInfo = profileInfo;
            _rules = rules;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;

            var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(profileInfo.FileExtensionIdsJson);
            _fileExtensions = fileExtensions.Where(item => fileExtensionIds.Count == 0 || fileExtensionIds.Contains(item.Id)).Select(item => item.Name == "RM_FA_FileType_Empty" ? "" : item.Name).ToList();

            var setting = _keyValueDao.GetValueByKey(DISCOVERY_MULTIPLE_RULES_ANALYSIS_METHOD);
            if (string.IsNullOrWhiteSpace(setting?.Value) || !Enum.TryParse(setting.Value, out RMDiscoveryOffice365SiteRotDataAnalysisTestMethod testMethod))
            {
                _testMethod = RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.None;
            }
            else
            {
                _testMethod = testMethod;
            }
        }

        public async Task<bool> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                await _profileDataDao.DeleteSiteRotDataBySiteIdAsync(_o365TenantId, _profileInfo.Id, _siteInfo.Id);

                RMDiscoveryProfileSiteRotData dataInfo;

                if (_profileInfo.IsBuildIn)
                {
                    dataInfo = await AnalysisBuildInProfileDataAsync();
                }
                else
                {
                    var ruleIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.RuleIdsJson);
                    var ruleInfoes = _rules.Where(item => ruleIds.Contains(item.Id)).ToList();
                    if (ruleInfoes.Count == 1)
                    {
                        dataInfo = await AnalysisSingleRuleProfileDataAsync(ruleInfoes);
                    }
                    else
                    {
                        dataInfo = await AnalysisMultipleRuleProfileDataAsync(ruleInfoes);
                    }
                }

                dataInfo.FileTotalSize = _siteInfo.FileTotalSize;
                dataInfo.ContainerId = _siteInfo.ContainerId;

                await _profileDataDao.AddSiteRotDataListAsync(_o365TenantId, _profileInfo.Id, dataInfo);

                _logger.Info($"End analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data. Error: {e}");
                return false;
            }
        }

        private async Task<RMDiscoveryProfileSiteRotData> AnalysisBuildInProfileDataAsync()
        {
            _logger.Info($"Analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] build in rot data.");
            var res = new RMDiscoveryProfileSiteRotData
            {
                SiteId = _siteInfo.Id
            };

            var dataEnumerable = _dataDao.GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, "", []);
            await foreach(var data in dataEnumerable)
            {
                res.RotFileTotalSize += data.FileTotalSize;
            }

            var rCategoryDataEnumerable = _dataDao.GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Redundant) });
            await foreach(var data in rCategoryDataEnumerable)
            {
                res.RCategoryFileTotalSize += data.FileTotalSize;
            }

            var oCategoryDataEnumerable = _dataDao.GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Obsolete) });
            await foreach (var data in oCategoryDataEnumerable)
            {
                res.OCategoryFileTotalSize += data.FileTotalSize;
            }

            var tCategoryDataEnumerable = _dataDao.GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Trivial) });
            await foreach (var data in tCategoryDataEnumerable)
            {
                res.TCategoryFileTotalSize += data.FileTotalSize;
            }

            return res;
        }

        private async Task<RMDiscoveryProfileSiteRotData> AnalysisSingleRuleProfileDataAsync(List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            _logger.Info($"Analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] single rule rot data.");

            var res = new RMDiscoveryProfileSiteRotData
            {
                SiteId = _siteInfo.Id
            };

            var ruleInfo = ruleInfoes.First();
            var totalSize = await AnalysisSiteRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfo);

            res.RotFileTotalSize = totalSize;
            if(ruleInfo.Category == RMDiscoveryRuleCategory.Redundant)
            {
                res.RCategoryFileTotalSize = totalSize;
            }
            else if(ruleInfo.Category == RMDiscoveryRuleCategory.Obsolete)
            {
                res.OCategoryFileTotalSize = totalSize;
            }
            else
            {
                res.TCategoryFileTotalSize = totalSize;
            }

            res.CustomColumns.Add(new(ruleInfo.ToCustomColumn().Name, totalSize, typeof(long)));

            return res;
        }

        private async Task<RMDiscoveryProfileSiteRotData> AnalysisMultipleRuleProfileDataAsync(List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            _logger.Info($"Analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] multiple rule rot data.");

            var res = new RMDiscoveryProfileSiteRotData
            {
                SiteId = _siteInfo.Id
            };

            res.RotFileTotalSize = await AnalysisFileTotalSizeFromMongoDBAsync(ruleInfoes);

            var rCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Redundant).ToList();
            res.RCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(rCategoryRules);

            var oCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Obsolete).ToList();
            res.OCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(oCategoryRules);

            var tCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Trivial).ToList();
            res.TCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(tCategoryRules);

            foreach(var ruleInfo in ruleInfoes)
            {
                var totalSize = await AnalysisSiteRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfo);
                res.CustomColumns.Add(new(ruleInfo.ToCustomColumn().Name, totalSize, typeof(long)));
            }

            return res;
        }

        private async Task<long> AnalysisMutipleRuleFileTotalSizeAsync(List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            if(ruleInfoes.Count == 0)
            {
                return 0;
            }

            if(ruleInfoes.Count == 1)
            {
                return await AnalysisSiteRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfoes.First());
            }

            return await AnalysisFileTotalSizeFromMongoDBAsync(ruleInfoes);
        }

        private async Task<long> AnalysisFileTotalSizeFromMongoDBAsync(List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            var versionRule = ruleInfoes.FirstOrDefault(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version);

            var documentRules = ruleInfoes.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document).ToList();
            var sizeRangeIds = _sizeRangeIds;
            var dateRangeIds = _dateRangeIds.Where(item => 
            item > _profileInfo.GreaterThanEqualWithoutInDate && 
            item <= _profileInfo.LessThanEqualWithoutInDate).ToList();
            if(_profileInfo.GreaterThanEqualWithoutInDate == -1)
            {
                dateRangeIds.Add(-1);
            }

            var listManager = new RMDiscoveryOffice365ListManager(_o365TenantId, _siteInfo.SiteId);
            var listIds = await listManager.GetListsAsync();

            var conditionalSql = " (" + string.Join(" or ", documentRules.ConvertAll(item => $" tags_{item.UniqueId.ToString().ToLower().Replace("-", "")} gt 0 ")) + ") ";
            var aggregateSql = $"FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size";
            var selecteSql = $"ItemId, FileExtension, FileSize, HistoryVersionsSize";
            var versionTagName = "";
            if (versionRule != null)
            {
                versionTagName = $"tags_{versionRule.UniqueId.ToString().ToLower().Replace("-", "")}";
                aggregateSql += $",{versionTagName}/total_size with sum as version_rule_total_size";
                selecteSql += $",{versionTagName}";
            }

            var fileTotalSize = 0L;

            var sumQueryCount = listIds.Count * sizeRangeIds.Count * dateRangeIds.Count;
            var itemsQueryCount = _siteInfo.FileSumCount / 1000;

            _logger.Info($"sum query way query count: [{sumQueryCount}], items query way query count [{itemsQueryCount}]");

            if ((itemsQueryCount < sumQueryCount && _testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.None) 
                || _testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.AllFile)
            {
                _logger.Info($"Analysis by all file.");
                fileTotalSize = await AnalysisExpandFileTotalSizeFromMongoDBAsync(ruleInfoes);
            }
            else
            {
                _logger.Info($"Analysis by condition.");
                foreach (var listId in listIds)
                {
                    foreach (var sizeRangeId in sizeRangeIds)
                    {
                        foreach (var dateRangeId in dateRangeIds)
                        {
                            try
                            {
                                if(_testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.ConditionWithFileType || _testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.ConditionWithAllFile)
                                {
                                    throw new Exception($"[{_testMethod}]");
                                }

                                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter(SiteId eq '{_siteInfo.SiteId}' " +
                                $"and ListId eq '{listId}' " +
                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                                $"and not IsPHL " +
                                $"and {conditionalSql} )" +
                                $"/groupby((FileExtension), aggregate({aggregateSql}))";
                                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString(), $"Site [{_siteInfo.SiteId}] analysis profile rot data");
                                var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
                                if (dataObjList != null)
                                {
                                    foreach (var dataObj in dataObjList)
                                    {
                                        var groupedDataObject = dataObj.TryGet("_id") as ExpandoObject;
                                        var fileExtension = groupedDataObject.GetValue("FileExtension");
                                        if (!_fileExtensions.Contains(fileExtension))
                                        {
                                            continue;
                                        }
                                        fileTotalSize += dataObj.GetValue<long>("file_total_size");
                                        fileTotalSize += dataObj.GetValue<long>("file_history_version_total_size");
                                        if (versionRule != null)
                                        {
                                            fileTotalSize -= dataObj.GetValue<long>("version_rule_total_size");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Error($"An error occurred while execute list: [{listId}] size range: [{sizeRangeId}] date range: [{dateRangeId}] group by extension. Error: {e}");
                                try
                                {
                                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter(SiteId eq '{_siteInfo.SiteId}' " +
                                            $"and ListId eq '{listId}' " +
                                            $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                            $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                            $"and not IsPHL)/" +
                                            $"aggregate(" +
                                            $"$count as file_sum_count)";
                                    var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString(), "AnalysisTotalSizeData");
                                    var countDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson).FirstOrDefault();
                                    if (countDataObj != null)
                                    {
                                        _logger.Info($"list: [{listId}] size range: [{sizeRangeId}] date range: [{dateRangeId}], query item way query count: [{countDataObj.GetValue<long>("file_sum_count") / 1000}], file extension way query count [{_fileExtensions.Count}]");
                                        if ((countDataObj.GetValue<long>("file_sum_count") / 1000 < _fileExtensions.Count && _testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.None) 
                                            || _testMethod == RMDiscoveryOffice365SiteRotDataAnalysisTestMethod.ConditionWithAllFile)
                                        {
                                            fileTotalSize += await AnalysisExpandWithConditionFileTotalSizeFromMongoDBAsync(versionRule, versionTagName, conditionalSql, selecteSql, listId, sizeRangeId, dateRangeId);
                                        }
                                        else
                                        {
                                            fileTotalSize += await AnalysisTraverseExtensionFileTotalSizeFromMongoDBAsync(versionRule, conditionalSql, aggregateSql, listId, sizeRangeId, dateRangeId);
                                        }
                                    }
                                }
                                catch(Exception countE)
                                {
                                    _logger.Error($"An error occurred while execute list: [{listId}] size range: [{sizeRangeId}] date range: [{dateRangeId}] get file count. Error: {countE}");
                                    fileTotalSize += await AnalysisExpandWithConditionFileTotalSizeFromMongoDBAsync(versionRule, versionTagName, conditionalSql, selecteSql, listId, sizeRangeId, dateRangeId);
                                }
                            }
                        }
                    }
                }
            }

            if(versionRule != null)
            {
                var singleVersionRuleTotalSize = await AnalysisSiteRuleLevelFileTotalSizeFromSqliteDBAsync(versionRule);
                fileTotalSize += singleVersionRuleTotalSize;
            }

            return fileTotalSize;
        }

        private async Task<long> AnalysisExpandFileTotalSizeFromMongoDBAsync(List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            var versionRule = ruleInfoes.FirstOrDefault(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version);

            var documentRules = ruleInfoes.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document).ToList();
            var dateRangeIds = _dateRangeIds.Where(item =>
            item > _profileInfo.GreaterThanEqualWithoutInDate &&
            item <= _profileInfo.LessThanEqualWithoutInDate).ToList();
            if (_profileInfo.GreaterThanEqualWithoutInDate == -1)
            {
                dateRangeIds.Add(-1);
            }

            var minDateRangeId = dateRangeIds.Min();
            var maxDateRangeId = dateRangeIds.Max();

            var listManager = new RMDiscoveryOffice365ListManager(_o365TenantId, _siteInfo.SiteId);
            var listIds = await listManager.GetListsAsync();

            var conditionalSql = " (" + string.Join(" or ", documentRules.ConvertAll(item => $" tags_{item.UniqueId.ToString().ToLower().Replace("-", "")} gt 0 ")) + ") ";
            var selecteSql = $"ItemId, FileExtension, FileSize, HistoryVersionsSize";
            var versionTagName = "";
            if (versionRule != null)
            {
                versionTagName = $"tags_{versionRule.UniqueId.ToString().ToLower().Replace("-", "")}";
                selecteSql += $",{versionTagName}";
            }

            var fileTotalSize = 0L;

            foreach(var listId in listIds)
            {
                var maxItemId = 0L;
                while (true)
                {
                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?" +
                                  $"$top=1000" +
                                  $"&$filter=SiteId eq '{_siteInfo.SiteId}' " +
                                  $"and ListId eq '{listId}' " +
                                  $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} ge {minDateRangeId} " +
                                  $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} le {maxDateRangeId} " +
                                  $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                                  $"and ItemId gt {maxItemId} " +
                                  $"and not IsPHL " +
                                  $"and {conditionalSql}" +
                                  $"&$orderby=ItemId" +
                                  $"&select={selecteSql}";
                    var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                    var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                    foreach(var item in items)
                    {
                        var fileExtension = item.GetValue("FileExtension");
                        if(!_fileExtensions.Contains(fileExtension))
                        {
                            continue;
                        }

                        fileTotalSize += item.GetValue<long>("FileSize");
                        fileTotalSize += item.GetValue<long>("HistoryVersionsSize");
                        if (versionRule != null)
                        {
                            var dic = item as IDictionary<string, object>;
                            if (dic.ContainsKey(versionTagName))
                            {
                                var columnExpandoObj = dic[versionTagName];
                                var columnDataObj = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(columnExpandoObj));
                                fileTotalSize -= columnDataObj.GetValue<long>("total_size");
                            }
                        }
                    }

                    if (items.Count == 0 || items.Count < 1000)
                    {
                        break;
                    }

                    maxItemId = items.Last().GetValue<long>("ItemId");
                }
            }

            return fileTotalSize;
        }

        private async Task<long> AnalysisTraverseExtensionFileTotalSizeFromMongoDBAsync(RMDiscoveryOffice365RuleInfo versionRule, string conditionalSql, string aggregateSql, Guid listId, int sizeRangeId, int dateRangeId)
        {
            _logger.Info($"Analysis by condition with file type.");
            var fileTotalSize = 0L;
            foreach (var fileExtension in _fileExtensions)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter(SiteId eq '{_siteInfo.SiteId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                    $"and FileExtension eq '{fileExtension.EscapeSpecialCharacters()}' " +
                    $"and not IsPHL " +
                    $"and {conditionalSql} )" +
                    $"/aggregate({aggregateSql})";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString(), $"Site [{_siteInfo.SiteId}] analysis profile rot data");
                var dataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson).FirstOrDefault();
                if (dataObj != null)
                {
                    fileTotalSize += dataObj.GetValue<long>("file_total_size");
                    fileTotalSize += dataObj.GetValue<long>("file_history_version_total_size");
                    if (versionRule != null)
                    {
                        fileTotalSize -= dataObj.GetValue<long>("version_rule_total_size");
                    }
                }
            }

            return fileTotalSize;
        }

        private async Task<long> AnalysisExpandWithConditionFileTotalSizeFromMongoDBAsync(RMDiscoveryOffice365RuleInfo versionRule, string versionTagName, string conditionalSql, string selectSql, Guid listId, int sizeRangeId, int dateRangeId)
        {
            _logger.Info($"Analysis by condition with all file.");
            var fileTotalSize = 0L;
            var maxItemId = 0L;
            while (true)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?" +
                              $"$top=1000" +
                              $"&$filter=SiteId eq '{_siteInfo.SiteId}' " +
                              $"and ListId eq '{listId}' " +
                              $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                              $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                              $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                              $"and ItemId gt {maxItemId} " +
                              $"and not IsPHL " +
                              $"and {conditionalSql}" +
                              $"&$orderby=ItemId" +
                              $"&select={selectSql}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    var fileExtension = item.GetValue("FileExtension");
                    if (!_fileExtensions.Contains(fileExtension))
                    {
                        continue;
                    }

                    fileTotalSize += item.GetValue<long>("FileSize");
                    fileTotalSize += item.GetValue<long>("HistoryVersionsSize");
                    if (versionRule != null)
                    {
                        var dic = item as IDictionary<string, object>;
                        if (dic.ContainsKey(versionTagName))
                        {
                            var columnExpandoObj = dic[versionTagName];
                            var columnDataObj = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(columnExpandoObj));
                            fileTotalSize -= columnDataObj.GetValue<long>("total_size");
                        }
                    }
                }

                if (items.Count == 0 || items.Count < 1000)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            return fileTotalSize;
        }

        private async Task<long> AnalysisSiteRuleLevelFileTotalSizeFromSqliteDBAsync(RMDiscoveryOffice365RuleInfo ruleInfo)
        {
            var fileTotalSize = 0L;

            var (sqlConditionList, parameters) = GetSqlConditionalExpression();
            sqlConditionList.Add(" [Rule] = @Rule");
            parameters.Add(new SQLiteParameter("@Rule", ruleInfo.Id));
            var sqlConditionExpression = string.Join(" AND ", sqlConditionList);

            var siteDataEnumerable = _dataDao.GetSiteRuleLevelRotDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, sqlConditionExpression, parameters);
            await foreach (var siteData in siteDataEnumerable)
            {
                fileTotalSize += siteData.FileTotalSize;
            }

            return fileTotalSize;
        }

        private (List<string> sqlConditionList, List<SQLiteParameter> parameters) GetSqlConditionalExpression()
        {
            var sqlConditionList = new List<string>();
            var sqlParameters = new List<SQLiteParameter>();

            if (_profileInfo.IsBuildIn)
            {
                return ([], []);
            }

            if (!string.IsNullOrWhiteSpace(_profileInfo.FileExtensionIdsJson))
            {
                var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.FileExtensionIdsJson);
                if (fileExtensionIds.Count > 0)
                {
                    var fileExtensionSqlConditionList = new List<string>();
                    foreach (var fileExtensionId in fileExtensionIds)
                    {
                        fileExtensionSqlConditionList.Add($"FileExtension = @FileExtension{fileExtensionId}");
                        sqlParameters.Add(new($"@FileExtension{fileExtensionId}", fileExtensionId));
                    }

                    var fileExtensionSqlCondition = $"({string.Join(" OR ", fileExtensionSqlConditionList)})";
                    sqlConditionList.Add(fileExtensionSqlCondition);
                }
            }

            if(_profileInfo.GreaterThanEqualWithoutInDate > -1)
            {
                sqlConditionList.Add("WithoutInDate > @GreaterThanEqualWithoutInDate");
                sqlParameters.Add(new("@GreaterThanEqualWithoutInDate", _profileInfo.GreaterThanEqualWithoutInDate));
            }

            sqlConditionList.Add("WithoutInDate <= @LessThanEqualWithoutInDate");
            sqlParameters.Add(new("@LessThanEqualWithoutInDate", _profileInfo.LessThanEqualWithoutInDate));

            return (sqlConditionList, sqlParameters);
        }
    }

    internal enum RMDiscoveryOffice365SiteRotDataAnalysisTestMethod
    {
        None = 0,
        AllFile = 1,
        Condition = 2,
        ConditionWithAllFile = 3,
        ConditionWithFileType = 4,
    }
}
