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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.Telemetry.Data.Alita;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1
{
    public class RMDiscoveryOffice365RotDataAnalyzer : RMDiscoveryOffice365DataAnalyzer
    {
        public RMDiscoveryOffice365RotDataAnalyzer(
            SourceFlag contentSource,
            RMDiscoveryOffice365AnalysisJob jobInfo,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalysisManager,
            RMDiscoveryOffice365ContainerInfo containerInfo,
            RMDiscoveryOffice365SiteInfo siteInfo,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            bool enableExpandQueryTest)
            : base(contentSource, jobInfo, fileExtensionAnalysisManager, containerInfo, siteInfo, sizeRangeIds, dateRangeIds, enableExpandQueryTest)
        {
        }

        public override async Task<bool> AnalysisAsync()
        {
            try
            {
                var rotDefinition = await _configurationDao.GetAsync<RMDiscoveryOffice365RotDefinition>(Contract.Discovery.Model.RMDiscoveryConfigurationType.Office365ROTDefinition);
                if (!rotDefinition.Enable)
                {
                    _logger.Info("Doesn't enable rot setting.");
                    return true;
                }

                _logger.Info($"Start analysis rot data.");

                var siteDataList = await QuerySiteDataList();
                await _dataDao.AddSiteRotDataAsync(_jobInfo.O365TenantId, siteDataList.ToArray());
                _logger.Info($"Successful add site rot data to db.");

                _logger.Info($"Finished analysis rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis rot data. Error: {e}");
                return false;
            }
        }

        private async Task<List<RMDiscoveryOffice365SiteRotData>> QuerySiteDataList()
        {
            var rotRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
            var rotRulesWithoutDuplicateColumns = rotRules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ConvertAll(item => (
                item.Id,
                IsVersionRule: item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version,
                TagNames: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                ColumnName: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", "") + (item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version ? "/total_size" : ""),
                SumName: item.ToCustomColumn().Name
            )).ToList();

            var siteDataList = new List<RMDiscoveryOffice365SiteRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId, ruleId) in QuerySiteDataObjects(rotRulesWithoutDuplicateColumns))
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionAnalysisManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionAnalysisManager.GetId(groupedDataObj.GetValue("FileExtension"));

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
                        exstisDataItem = new RMDiscoveryOffice365SiteRotData
                        {
                            ContainerId = _containerInfo.Id,
                            SiteId = _siteInfo.Id,
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

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId, int ruleId)> QuerySiteDataObjects(List<(int Id, bool IsVersionRule, string TagName, string ColumnName, string SumName)> rotRulesWithoutDuplicateColumns)
        {

            var listManager = new RMDiscoveryOffice365ListManager(_jobInfo.O365TenantId, _jobInfo.SiteId);
            var listIds = await listManager.GetListsAsync();

            _logger.Info($"Current site discovered list count: [{listIds.Count}].");

            foreach (var listId in listIds)
            {
                foreach (var sizeRangeId in _sizeRangeIds)
                {
                    foreach (var dateRangeId in _dateRangeIds)
                    {
                        foreach (var rotRulesWithoutDuplicateColumn in rotRulesWithoutDuplicateColumns)
                        {
                            _logger.Info($"Start query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{rotRulesWithoutDuplicateColumn.Id}] rot data.");

                            var dataObjList = new List<ExpandoObject>();

                            try
                            {
                                if (_enableExpandQueryTest)
                                {
                                    throw new Exception("Enable expand query test.");
                                }
                                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{_jobInfo.SiteId}' " +
                                                                $"and ListId eq '{listId}' " +
                                                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                                                $"and {rotRulesWithoutDuplicateColumn.ColumnName} gt 0 " +
                                                                $"and not IsPHL)" +
                                                                $"/groupby((FileExtension)," +
                                                                $"aggregate($count as file_sum_count, {rotRulesWithoutDuplicateColumn.ColumnName} with sum as file_total_size))";
                                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _jobInfo.O365TenantId.ToString());
                                dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
                            }
                            catch (Exception e)
                            {
                                _logger.Error($"The query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{rotRulesWithoutDuplicateColumn.Id}] rot data occur error. Error: {e}");
                                dataObjList = await ExpandQuerySiteData(listId, sizeRangeId, dateRangeId, rotRulesWithoutDuplicateColumn.TagName, rotRulesWithoutDuplicateColumn.IsVersionRule);
                            }

                            yield return (dataObjList, sizeRangeId, dateRangeId, rotRulesWithoutDuplicateColumn.Id);

                            _logger.Info($"End query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{rotRulesWithoutDuplicateColumn.Id}] rot data.");
                        }
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> ExpandQuerySiteData(Guid listId, int sizeRangeId, int dateRangeId, string rotColumnName, bool isVersionRule)
        {
            _logger.Info($"Start expand query rot data.");

            var dataDic = new Dictionary<string, IDictionary<string, object>>();

            var maxItemId = 0L;
            const int pageSize = 1000;

            for (var i = 0; ; i++)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{_jobInfo.SiteId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and {rotColumnName}{(isVersionRule ? "/total_size" : "")} gt 0 " +
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileExtension, {rotColumnName}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _jobInfo.O365TenantId.ToString());
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

                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;

                    if (isVersionRule)
                    {
                        var dic = item as IDictionary<string, object>;
                        if (dic.ContainsKey(rotColumnName))
                        {
                            var columnExpandoObj = dic[rotColumnName];
                            var columnDataObj = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(columnExpandoObj));
                            dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + columnDataObj.GetValue<long>("total_size");
                        }
                    }
                    else
                    {
                        dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>(rotColumnName);
                    }

                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query rot data.");
            return dataDic.Values.ConvertAll(item => item.ConvertToExpandoObject()).ToList();
        }
    }
}
