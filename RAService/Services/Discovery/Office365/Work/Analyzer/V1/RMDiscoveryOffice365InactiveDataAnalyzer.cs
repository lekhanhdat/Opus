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
using Aspose.Pdf;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1
{
    public class RMDiscoveryOffice365InactiveDataAnalyzer : RMDiscoveryOffice365DataAnalyzer
    {
        public RMDiscoveryOffice365InactiveDataAnalyzer(
            SourceFlag contentSource,
            RMDiscoveryOffice365AnalysisJob jobInfo,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileTypeAnalysisManager,
            RMDiscoveryOffice365ContainerInfo containerInfo,
            RMDiscoveryOffice365SiteInfo siteInfo,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            bool enableExpandQueryTest)
            : base(contentSource, jobInfo, fileTypeAnalysisManager, containerInfo, siteInfo, sizeRangeIds, dateRangeIds, enableExpandQueryTest)
        {
        }

        public override async Task<bool> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis inactive data.");

                var inactiveEnable = (await _configurationDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition)).Enable;
                var rules = inactiveEnable ? await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive) : [];

                _logger.Info($"Iactive rule is enable [{inactiveEnable}]. Available rules [{string.Join(", ", rules.Select(item => item.Id))}].");

                var siteDataList = await QuerySiteDataList(rules);
                await _dataDao.AddSiteInactiveDataAsync(_jobInfo.O365TenantId, siteDataList.ToArray());
                _logger.Info($"Successful add site inactive data to db.");

                _logger.Info($"Finished analysis inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<List<RMDiscoveryOffice365SiteInactiveData>> QuerySiteDataList(List<RMDiscoveryOffice365RuleInfo> rules)
        {
            _logger.Info("Start analysis site data.");

            List<(string ColumnName, string SumName)> inactiveVersionRulesColumns = rules.ConvertAll(item => (
                "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                item.ToCustomColumn().Name
            ));

            var inactiveVersionRulesSumSql = inactiveVersionRulesColumns.ConvertAll(item => $"{item.ColumnName}/total_size with sum as {item.SumName}");

            var siteDataList = new List<RMDiscoveryOffice365SiteInactiveData>();


            await foreach (var (dataObjList, sizeRangeId, dateRangeId) in QuerySiteDataObjects(inactiveVersionRulesSumSql, inactiveVersionRulesColumns))
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionAnalysisManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionAnalysisManager.GetId(groupedDataObj.GetValue("FileExtension"));
                    var exstisDataItem = siteDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryOffice365SiteInactiveData
                        {
                            ContainerId = _containerInfo.Id,
                            SiteId = _siteInfo.Id,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                        };
                        foreach (var inactiveVersionRuleColumn in inactiveVersionRulesColumns)
                        {
                            exstisDataItem.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                                inactiveVersionRuleColumn.SumName,
                                0L,
                                typeof(long)
                                )
                            );
                        }
                        siteDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += dataObj.GetValue<long>("file_total_size") + dataObj.GetValue<long>("file_history_version_total_size");
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                    foreach (var inactiveVersionRuleColumn in inactiveVersionRulesColumns)
                    {
                        var customColumn = exstisDataItem.CustomColumns.First(item => item.Name == inactiveVersionRuleColumn.SumName);
                        customColumn.Value = Convert.ToInt64(customColumn.Value) + dataObj.GetValue<long>(inactiveVersionRuleColumn.SumName);
                    }
                }
                _logger.Info($"The amount of inactive site data that needs to be added to the DB is [{siteDataList.Count}].");
            }
            return siteDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId)> QuerySiteDataObjects(List<string> inactiveVersionRulesSumSql, List<(string ColumnName, string SumName)> inactiveVersionRulesColumns)
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
                        _logger.Info($"Start query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data.");

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
                           $"and not IsPHL)" +
                           $"/groupby((FileExtension)," +
                           $"aggregate($count as file_sum_count, FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size {(inactiveVersionRulesSumSql.Any() ? "," + string.Join(", ", inactiveVersionRulesSumSql) : "")}))";
                            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _jobInfo.O365TenantId.ToString());
                            dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"The query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data occur error. Error: {e}");
                            dataObjList = await ExpandQuerySiteData(listId, sizeRangeId, dateRangeId, inactiveVersionRulesColumns);
                        }

                        yield return (dataObjList, sizeRangeId, dateRangeId);

                        _logger.Info($"End query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data.");
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> ExpandQuerySiteData(Guid listId, int sizeRangeId, int dateRangeId, List<(string ColumnName, string SumName)> inactiveVersionRulesColumns)
        {
            _logger.Info($"Start expand query inactive data.");

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
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileSize, FileExtension, HistoryVersionsSize {(inactiveVersionRulesColumns.Any() ? "," + string.Join(", ", inactiveVersionRulesColumns.Select(item => item.ColumnName)) : "")}";
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
                            { "file_history_version_total_size", 0 }
                        };
                        foreach (var (ColumnName, SumName) in inactiveVersionRulesColumns)
                        {
                            dataObj.Add(SumName, 0);
                        }
                        dataDic.Add(fileExtension, dataObj);
                    }

                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                    dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>("FileSize");
                    dataObj["file_history_version_total_size"] = Convert.ToInt64(dataObj["file_history_version_total_size"]) + item.GetValue<long>("HistoryVersionsSize");
                    foreach (var (ColumnName, SumName) in inactiveVersionRulesColumns)
                    {
                        var dic = item as IDictionary<string, object>;
                        if (dic.ContainsKey(ColumnName))
                        {
                            var columnExpandoObj = dic[ColumnName];
                            var columnDataObj = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(columnExpandoObj));
                            dataObj[SumName] = Convert.ToInt64(dataObj[SumName]) + columnDataObj.GetValue<long>("total_size");
                        }
                    }
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query inactive data.");
            return dataDic.Values.ConvertAll(item => item.ConvertToExpandoObject()).ToList();
        }
    }
}
