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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.O365Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Azure;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1
{
    public class RMDiscoveryOffice365Analyzer
    {
        private const string S_ENABLE_EXPAND_QUERY_TEST = "ENABLE_DISCOVERY_EXPAND_QUERY_TEST";

        protected readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365Analyzer));

        protected readonly IRMKeyValueDao _keyValueDao;

        protected readonly IRMDiscoveryConfigurationDao _configurationDao;

        protected readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        protected readonly IRMDiscoveryOffice365DataDao _dataDao;

        protected readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao;

        protected readonly IRMDiscoveryOffice365WithoutInDateDao _withoutDateDao;

        protected readonly IEApiClient _ieApiClient;

        protected readonly SourceFlag _contentSource;

        protected readonly RMDiscoveryExclusionInfo _exclusionInfo;

        protected readonly RMDiscoveryOffice365AnalysisJob _jobInfo;

        public RMDiscoveryOffice365Analyzer(SourceFlag contentSource, RMDiscoveryExclusionInfo exclusionInfo, RMDiscoveryOffice365AnalysisJob jobInfo)
        {
            _contentSource = contentSource;
            _exclusionInfo = exclusionInfo;
            _jobInfo = jobInfo;
            _configurationDao = new RMDiscoveryConfigurationDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _dataDao = new RMDiscoveryOffice365DataDao();
            _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();
            _withoutDateDao = new RMDiscoveryOffice365WithoutInDateDao();
            _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
        }

        public async Task<bool> AnalysisAsync()
        {
            try
            {
                var enableExpandQueryTest = false;
                var setting = _keyValueDao.GetValueByKey(S_ENABLE_EXPAND_QUERY_TEST);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    _ = bool.TryParse(setting.Value, out enableExpandQueryTest);
                }

                _logger.Info($"Expand query test is enable? [{enableExpandQueryTest}]");

                var sizeRangeIds = (await _sizeRangeDao.GetAllAsync()).Select(item => item.Id).ToList();
                var dateRangeIds = (await _withoutDateDao.GetAllAsync()).Select(item => item.Id).Concat(new List<int> { -1, 999 }).ToList();

                var (has, totalData) = await TryGetAggregateTotalDataAsync(sizeRangeIds, dateRangeIds, enableExpandQueryTest);

                _logger.Info($"Current site total size [{totalData?.FileTotalSize}]. Exclusion info: sharepoint size limit [{_exclusionInfo.SharePointOnlineSiteSizeLimit}], onedrive size limit [{_exclusionInfo.OneDriveSiteSizeLimit}].");

                if (!has)
                {
                    return false;
                }

                if (totalData == null || totalData.FileSumCount == 0)
                {
                    return true;
                }

                var hidden = totalData.FileTotalSize <= (_contentSource == SourceFlag.SharePoint ? _exclusionInfo.SharePointOnlineSiteSizeLimit : _exclusionInfo.OneDriveSiteSizeLimit);

                var containerInfo = await GetOrAddContainerInfoAsync();
                var siteInfo = await AddSiteInfoAsync(containerInfo, totalData, hidden);

                if (!hidden)
                {
                    var fileExtensionAnalysisManager = new RMDiscoveryOffice365FileExtensionAnalysisManager(_jobInfo.O365TenantId);
                    await fileExtensionAnalysisManager.InitAsync();

                    var inactiveAnalyzer = new RMDiscoveryOffice365InactiveDataAnalyzer(_contentSource, _jobInfo, fileExtensionAnalysisManager, containerInfo, siteInfo, sizeRangeIds, dateRangeIds, enableExpandQueryTest);
                    if (!await inactiveAnalyzer.AnalysisAsync())
                    {
                        return false;
                    }

                    var rotAnalyzer = new RMDiscoveryOffice365RotDataAnalyzer(_contentSource, _jobInfo, fileExtensionAnalysisManager, containerInfo, siteInfo, sizeRangeIds, dateRangeIds, enableExpandQueryTest);
                    if (!await rotAnalyzer.AnalysisAsync())
                    {
                        return false;
                    }
                }

                await UpdateAggregateTotalDataAsync(totalData, containerInfo);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while process analysis. Error: {e}");
                return false;
            }
        }

        private async Task<(bool has, RMDiscoveryOffice365AggregateTotalData totalData)> TryGetAggregateTotalDataAsync(List<int> sizeRangeIds, List<int> dateRangeIds, bool enableExpandQueryTest)
        {
            try
            {
                var odataUri = RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource];

                var totalData = new RMDiscoveryOffice365AggregateTotalData();

                var listManager = new RMDiscoveryOffice365ListManager(_jobInfo.O365TenantId, _jobInfo.SiteId);
                var listIds = await listManager.GetListsAsync();

                _logger.Info($"Current site discovered list count: [{listIds.Count}].");

                foreach (var listId in listIds)
                {
                    foreach (var sizeRangeId in sizeRangeIds)
                    {
                        foreach (var dateRangeId in dateRangeIds)
                        {
                            _logger.Info($"Query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] total data.");

                            var totalDataObj = new ExpandoObject();

                            try
                            {
                                if (enableExpandQueryTest)
                                {
                                    throw new Exception("Enable expand query test.");
                                }
                                var totalSql = $"{odataUri}?$apply=filter(SiteId eq '{_jobInfo.SiteId}' " +
                               $"and ListId eq '{listId}' " +
                               $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                               $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                               $"and not IsPHL)/" +
                               $"aggregate(" +
                               $"$count as file_sum_count, " +
                               $"FileSize with sum as file_total_size, " +
                               $"HistoryVersionsSize with sum as file_history_version_total_size)";
                                var totalDataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(totalSql, _jobInfo.O365TenantId.ToString());
                                totalDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(totalDataJson).FirstOrDefault();
                            }
                            catch (Exception e)
                            {
                                _logger.Warn($"The query list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] total data occur error. Error: {e}");
                                totalDataObj = await ExpandQueryAggregateTotalDataAsync(listId, sizeRangeId, dateRangeId);
                            }

                            if (totalDataObj != null)
                            {
                                totalData.FileSumCount += totalDataObj.GetValue<long>("file_sum_count");
                                totalData.FileTotalSize += totalDataObj.GetValue<long>("file_total_size") + totalDataObj.GetValue<long>("file_history_version_total_size");
                                totalData.TotalVersionSize += totalDataObj.GetValue<long>("file_history_version_total_size");
                            }
                        }
                    }
                }

                _logger.Info($"The current site file sum count [{totalData.FileSumCount}], total size [{totalData.FileTotalSize}], version size [{totalData.TotalVersionSize}].");

                var maxAgeSql = $"{odataUri}?$filter=SiteId eq '{_jobInfo.SiteId}' and not IsPHL&$orderby=CreatedMonth asc&$select=CreatedMonth&$top=1";
                var maxAgeDataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(maxAgeSql, _jobInfo.O365TenantId.ToString());
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(maxAgeDataJson)["value"];
                var maxAgeDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values)).FirstOrDefault();
                if (maxAgeDataObj != null)
                {
                    var nowYear = long.Parse(DateTime.UtcNow.Year.ToString());
                    var nowMonth = long.Parse(DateTime.UtcNow.Month.ToString());
                    var createTime = maxAgeDataObj.GetValue<long>("CreatedMonth");
                    var createYear = createTime / 100;
                    var createMonth = createTime % 100;
                    totalData.MaxFileAge = (int)((nowYear - createYear) * 12 + (nowMonth - createMonth));
                }
                _logger.Info($"The current site file max age [{totalData.MaxFileAge}].");

                var phlSql = $"{odataUri}?$filter=SiteId eq '{_jobInfo.SiteId}' and IsPHL &$apply=groupby((SiteId, IsPHL)," +
                    $"aggregate(" +
                    $"FileSize with sum as file_total_size, " +
                    $"HistoryVersionsSize with sum as file_history_version_total_size))";
                var phlDataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(phlSql, _jobInfo.O365TenantId.ToString());
                var phlDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(phlDataJson).FirstOrDefault();
                if (phlDataObj != null)
                {
                    totalData.PHLVolume = phlDataObj.GetValue<long>("file_total_size") + phlDataObj.GetValue<long>("file_history_version_total_size");
                }
                _logger.Info($"The current site file PHL volume [{totalData.PHLVolume}].");

                return (true, totalData);
            }
            catch (RequestFailedException ae) when (ae.Status == 404)
            {
                _logger.Warn($"There is no list under the site that supports discovery. message: {ae}");
                return (true, null);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis count. Error: {e}");
                return (false, null);
            }
        }

        private async Task<ExpandoObject> ExpandQueryAggregateTotalDataAsync(Guid listId, int sizeRangeId, int dateRangeId)
        {
            _logger.Info($"Start expand query aggregate total data.");

            IDictionary<string, object> dataObj = new Dictionary<string, object>
                        {
                            { "file_sum_count", 0 },
                            { "file_total_size", 0 },
                            { "file_history_version_total_size", 0 }
                        };

            const int pageSize = 1000;
            var maxItemId = 0L;

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
                    $"&select=ItemId, FileSize, FileExtension, HistoryVersionsSize";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _jobInfo.O365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                    dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>("FileSize");
                    dataObj["file_history_version_total_size"] = Convert.ToInt64(dataObj["file_history_version_total_size"]) + item.GetValue<long>("HistoryVersionsSize");
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query aggregate total data.");
            return dataObj.ConvertToExpandoObject();
        }

        private async Task<RMDiscoveryOffice365ContainerInfo> GetOrAddContainerInfoAsync()
        {
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisContainer, $"{_jobInfo.O365TenantId}_{_jobInfo.ContainerId}", TimeSpan.FromMinutes(10)))
            {
                var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(_jobInfo.O365TenantId, _jobInfo.ContainerId);
                if (has)
                {
                    return containerInfo;
                }

                var opusContainerInfo = await _nodeDao.GetOpusContainerById(_jobInfo.ContainerId);
                switch (opusContainerInfo.Url.Trim())
                {
                    case "Default_ SharePoint Sites_ Group":
                        opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                        break;
                    case "Default Office 365 Group Sites Group":
                        opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                        break;
                    case "Default Private Channel Sites Container":
                        opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                        break;
                }
                containerInfo = new RMDiscoveryOffice365ContainerInfo
                {
                    Name = opusContainerInfo.Url,
                    AosId = new Guid(opusContainerInfo.AosId),
                    OpusId = new Guid(opusContainerInfo.Id),
                    ContentSource = _contentSource,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedTime = DateTime.UtcNow.Ticks,
                    FileTotalSize = 0,
                    FileSumCount = 0,
                    SiteCount = 0
                };
                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.O365TenantId, containerInfo);
                _logger.Info($"Successful create container [{containerInfo.Id} - {containerInfo.Name}].");
                return containerInfo;
            }
        }

        private async Task<RMDiscoveryOffice365SiteInfo> AddSiteInfoAsync(RMDiscoveryOffice365ContainerInfo containerInfo, RMDiscoveryOffice365AggregateTotalData totalData, bool hidden)
        {
            var siteInfo = new RMDiscoveryOffice365SiteInfo
            {
                Url = _jobInfo.Url,
                SiteId = _jobInfo.SiteId,
                ContainerId = containerInfo.Id,
                ContentSource = _contentSource,
                FileTotalSize = totalData.FileTotalSize,
                FileSumCount = totalData.FileSumCount,
                CreateTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                Hidden = hidden
            };
            await _nodeDao.AddOrUpdateDiscoverySiteAsync(_jobInfo.O365TenantId, siteInfo);
            return siteInfo;
        }

        private async Task UpdateAggregateTotalDataAsync(RMDiscoveryOffice365AggregateTotalData totalData, RMDiscoveryOffice365ContainerInfo containerInfo)
        {
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisAggregateTotalData, $"{_jobInfo.O365TenantId}", TimeSpan.FromMinutes(10)))
            {
                var data = await _dataDao.GetAggregateTotalDataAsync(_jobInfo.O365TenantId, _contentSource);

                using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(_jobInfo.O365TenantId);
                using var transaction = efContext.Database.BeginTransaction();

                try
                {
                    data.FileTotalSize += totalData.FileTotalSize;
                    data.FileSumCount += totalData.FileSumCount;
                    data.MaxFileAge = Math.Max(data.MaxFileAge, totalData.MaxFileAge);
                    data.TotalVersionSize += totalData.TotalVersionSize;
                    data.PHLVolume += totalData.PHLVolume;
                    await _dataDao.AddOrUpdateAggregateTotalDataAsync(efContext, data);

                    containerInfo.FileTotalSize += totalData.FileTotalSize;
                    containerInfo.FileSumCount += totalData.FileSumCount;
                    containerInfo.SiteCount++;
                    await _nodeDao.AddOrUpdateDiscoveryContainerAsync(efContext, containerInfo);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
