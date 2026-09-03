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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
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

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator
{
    public class RMDiscoveryOffice365DuplicateCalculator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicateCalculator));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        private readonly RMDiscoveryOffice365MainJob _jobInfo;

        public RMDiscoveryOffice365DuplicateCalculator(RMDiscoveryOffice365MainJob jobInfo)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _dataDao = new RMDiscoveryOffice365DataDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _jobInfo = jobInfo;

        }

        public async Task<bool> CalculateAsync()
        {
            try
            {
                _logger.Info($"Start calculate duplicate data.");

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_jobInfo.Id);
                var o365TenantIds = discoveryJobs.Select(item => item.O365TenantId).ToHashSet();
                var duplicateRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument);

                if (!duplicateRules.Any())
                {
                    _logger.Warn("Current tenant no duplicate rule found.");
                    return true;
                }

                foreach (var o365TenantId in o365TenantIds)
                {
                    await ClearDataAsync(o365TenantId, duplicateRules);

                    var fileExtensionAnalysisManager = new RMDiscoveryOffice365FileExtensionAnalysisManager(o365TenantId);
                    await fileExtensionAnalysisManager.InitAsync();

                    var contentSources = discoveryJobs.Where(item => item.O365TenantId == o365TenantId).Select(item => item.ContentSource).ToHashSet();
                    _logger.Info($"The o365 tenant [{o365TenantId}] need to calculate duplicate data in content sources [{string.Join(", ", contentSources)}].");

                    foreach (var contentSource in contentSources)
                    {
                        _logger.Info($"Start analysis [{o365TenantId}] [{contentSource}] duplicate data.");

                        await ApplyDuplicateTagAsync(o365TenantId, contentSource, duplicateRules);
                        _logger.Info($"Apply [{o365TenantId}] [{contentSource}] duplicate tag successful.");

                        var siteDataList = await AnalysisSiteDataAsync(o365TenantId, contentSource, fileExtensionAnalysisManager, duplicateRules);
                        var groupedSiteDataList = siteDataList.GroupBy(item => item.SiteId).ToDictionary(item => item.Key, item => item.ToList());
                        foreach (var groupedSiteData in groupedSiteDataList)
                        {
                            var siteId = groupedSiteData.Key;

                            await _dataDao.AddSiteRotDataAsync(o365TenantId, groupedSiteData.Value.ToArray());
                            _logger.Info($"Analysis [{o365TenantId}] [{contentSource}] site [{siteId}] site data successful.");

                            if(_jobInfo.Version == Contract.Discovery.Job.RMDiscoveryJobVersion.V2)
                            {
                                await AnalysisContainerDataAsync(o365TenantId, groupedSiteData.Value);
                                _logger.Info($"Analysis [{o365TenantId}] [{contentSource}] site [{siteId}] container data successful.");

                                await AnalysisBasicDataAsync(o365TenantId, contentSource, groupedSiteData.Value);
                                _logger.Info($"Analysis [{o365TenantId}] [{contentSource}] site [{siteId}] basic data successful.");
                            }
                        }

                        _logger.Info($"End analysis [{o365TenantId}] [{contentSource}] duplicate data.");
                    }
                }

                _logger.Info($"End calculate duplicate data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate duplicate data. Error: {e}");
                return false;
            }
        }

        private async Task ClearDataAsync(Guid o365TenantId, List<RMDiscoveryOffice365RuleInfo> duplicateRules)
        {
            if (_jobInfo.Type == Contract.Discovery.Job.RMDiscoveryJobType.Newly)
            {
                return;
            }

            _logger.Info($"Start clear [{o365TenantId}] discovery duplicate rot data.");

            await _dataDao.DeleteSitesDuplicateDataListAsync(o365TenantId, [.. duplicateRules]);
            _logger.Info($"Successful delete [{o365TenantId}] sites duplicate data.");

            await _dataDao.DeleteContainersDuplicateDataListAsync(o365TenantId, [.. duplicateRules]);
            _logger.Info($"Successful delete [{o365TenantId}] containers duplicate data.");

            await _dataDao.DeleteBasicDuplicateDataListAsync(o365TenantId, [.. duplicateRules]);
            _logger.Info($"Successful delete [{o365TenantId}] basic duplicate data.");

            _logger.Info($"End clear [{o365TenantId}] discovery duplicate rot data.");
        }

        private async Task ApplyDuplicateTagAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryOffice365RuleInfo> duplicateRules)
        {

            const int pageSize = 1000;

            var siteExistDic = new Dictionary<Guid, bool>();

            KeyValuePair<int, List<string>>? lastDuplicateDataList = null;

            for (var i = 0; ; i++)
            {
                var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[contentSource]}?" +
                    $"$orderby=FileSize,Name&$top={pageSize}&$skip={i * pageSize}" +
                    $"&filter=(not IsPHL)" +
                    $"&select=SiteId,ObjectId,Name,FileSize";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(odataUrl, o365TenantId.ToString());
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"];
                var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values));

                if (dataObjList.Count == 0)
                {
                    break;
                }

                var duplicateDataList = dataObjList.ConvertAll(item => new
                {
                    SiteId = new Guid(item.GetValue("SiteId")),
                    ObjectId = item.GetValue("ObjectId"),
                    DuplicateValueHashCode = $"{item.GetValue("Name")}_{item.GetValue("FileSize")}".GetHashCode(),
                }).Where(item =>
                {
                    if (!siteExistDic.TryGetValue(item.SiteId, out var exists))
                    {
                        exists = (_nodeDao.GetDiscoverySiteInfoAsync(o365TenantId, item.SiteId).GetAwaiter().GetResult() != null);
                        siteExistDic.Add(item.SiteId, exists);
                    }
                    return exists;
                }).ToList();

                if (!duplicateDataList.Any())
                {
                    continue;
                }

                var groupedDuplicateDataList = duplicateDataList
                    .GroupBy(item => item.DuplicateValueHashCode, item => item)
                    .ToDictionary(item => item.Key, item => item.Select(i => i.ObjectId).ToList());

                if (lastDuplicateDataList.HasValue)
                {
                    var lastDuplicateValueHashCode = lastDuplicateDataList.Value.Key;
                    if (!groupedDuplicateDataList.TryGetValue(lastDuplicateValueHashCode, out var dataList))
                    {
                        dataList = [];
                        groupedDuplicateDataList[lastDuplicateValueHashCode] = dataList;
                    }

                    dataList.AddRange(lastDuplicateDataList.Value.Value);
                }

                var newlyLastDuplicateValueHashCode = duplicateDataList.Last().DuplicateValueHashCode;
                var newlyLastDuplicateDataList = groupedDuplicateDataList[newlyLastDuplicateValueHashCode];
                lastDuplicateDataList = KeyValuePair.Create(newlyLastDuplicateValueHashCode, newlyLastDuplicateDataList);
                groupedDuplicateDataList.Remove(newlyLastDuplicateValueHashCode);

                groupedDuplicateDataList = groupedDuplicateDataList.Where(item => item.Value.Count > 1).ToDictionary(item => item.Key, item => item.Value);
                await ApplyDuplicateTagAsync(o365TenantId, contentSource, groupedDuplicateDataList);

                if (dataObjList.Count < pageSize)
                {
                    break;
                }
            }

            if (lastDuplicateDataList.HasValue && lastDuplicateDataList.Value.Value.Count > 1)
            {
                await ApplyDuplicateTagAsync(o365TenantId, contentSource, new Dictionary<int, List<string>>() { { lastDuplicateDataList.Value.Key, lastDuplicateDataList.Value.Value } });
            }
        }

        private async Task ApplyDuplicateTagAsync(Guid o365TenantId, SourceFlag contentSource, Dictionary<int, List<string>> groupedDuplicateDataList)
        {
            var duplicateDataList = groupedDuplicateDataList.SelectMany(item => item.Value).ToList();
            if (!duplicateDataList.Any())
            {
                return;
            }

            var tagModel = new ModifyTagModel
            {
                TagRuleId = RMDiscoveryBuildInRule.DUPLICATE_UNIQUE_ID,
                TagValue = 1
            };

            var o365TenantIdStr = o365TenantId.ToString();

            foreach (var duplicateData in duplicateDataList)
            {
                await _ieApiClient.ModifyTagWithRetryAsync(contentSource, o365TenantIdStr, duplicateData, tagModel);
            }
        }

        private async Task<List<RMDiscoveryOffice365SiteRotData>> AnalysisSiteDataAsync(Guid o365TenantId, SourceFlag contentSource, RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalysisManager, List<RMDiscoveryOffice365RuleInfo> duplicateRules)
        {

            var res = new List<RMDiscoveryOffice365SiteRotData>();

            var siteInfoesCache = new Dictionary<string, RMDiscoveryOffice365SiteInfo>();

            var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[contentSource]}?$apply=filter({RMDiscoveryBuildInRule.DUPLICATE_COLUMN_NAME} eq 1)" +
                $"/groupby((SiteId, FileExtension, {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME}, {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME})," +
                $"aggregate($count as file_sum_count, FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size))";
            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(odataUrl, o365TenantId.ToString());
            var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

            foreach (var dataObj in dataObjList)
            {
                var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                var sizeRange = groupedDataObj.GetValue<long>(RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME);
                var withoutInDate = groupedDataObj.GetValue<long>(RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME);
                var fileExtension = groupedDataObj.GetValue("FileExtension");
                var fileSumCount = dataObj.GetValue<long>("file_sum_count");
                var fileTotalSize = dataObj.GetValue<long>("file_total_size") + dataObj.GetValue<long>("file_history_version_total_size");
                var siteId = groupedDataObj.GetValue("SiteId");

                if (!siteInfoesCache.TryGetValue(siteId, out var siteInfo))
                {
                    siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(o365TenantId, new Guid(siteId));
                    siteInfoesCache.Add(siteId, siteInfo);
                }

                if (siteInfo == null)
                {
                    continue;
                }

                await fileExtensionAnalysisManager.AddOrUpdateAsync(fileExtension);

                foreach (var duplicateRule in duplicateRules)
                {
                    res.Add(new()
                    {
                        ContainerId = siteInfo.ContainerId,
                        SiteId = siteInfo.Id,
                        WithoutInDate = (int)withoutInDate,
                        SizeRange = (int)sizeRange,
                        FileExtension = fileExtensionAnalysisManager.GetId(fileExtension),
                        Rule = duplicateRule.Id,
                        FileSumCount = fileSumCount,
                        FileTotalSize = fileTotalSize
                    });
                }
            }

            return res;
        }

        private async Task AnalysisContainerDataAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRotData> siteDataList)
        {
            var containerId = siteDataList.First().ContainerId;
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisContainerRotData, $"{o365TenantId}_{containerId}", TimeSpan.FromMinutes(30)))
            {
                _logger.Info("Start analysis duplicate rot container data.");

                var existsContainerDataList = await _dataDao.GetContainerRotDataListAsync(o365TenantId, containerId);

                foreach (var siteData in siteDataList)
                {
                    var existsContainerData = existsContainerDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                    if (existsContainerData == null)
                    {
                        existsContainerData = new RMDiscoveryOffice365ContainerRotData
                        {
                            ContainerId = containerId,
                            WithoutInDate = siteData.WithoutInDate,
                            FileExtension = siteData.FileExtension,
                            SizeRange = siteData.SizeRange,
                            Rule = siteData.Rule
                        };
                        existsContainerDataList.Add(existsContainerData);
                    }
                    existsContainerData.FileTotalSize += siteData.FileTotalSize;
                    existsContainerData.FileSumCount += siteData.FileSumCount;
                }

                _logger.Info($"The amount of duplicate rot container data that needs to be added to the DB is [{existsContainerDataList.Count}].");
                await _dataDao.AddOrUpdateContainerRotDataAsync(o365TenantId, [.. existsContainerDataList]);

                _logger.Info($"Successful add container duplicate rot data to db.");
            }
        }

        private async Task AnalysisBasicDataAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryOffice365SiteRotData> siteDataList)
        {
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisBasicRotData, o365TenantId.ToString(), TimeSpan.FromMinutes(30)))
            {
                var existsBasicDataList = await _dataDao.GetBasicRotDataListAsync(o365TenantId, contentSource);
                foreach (var siteData in siteDataList)
                {
                    var existsBaicData = existsBasicDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicRotData
                        {
                            WithoutInDate = siteData.WithoutInDate,
                            FileExtension = siteData.FileExtension,
                            SizeRange = siteData.SizeRange,
                            Rule = siteData.Rule,
                            ContentSource = contentSource
                        };
                        existsBasicDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += siteData.FileTotalSize;
                    existsBaicData.FileSumCount += siteData.FileSumCount;
                }

                _logger.Info($"The amount of duplicate rot basic data that needs to be added to the DB is [{existsBasicDataList.Count}].");
                await _dataDao.AddOrUpdateBasicRotDataAsync(o365TenantId, contentSource, [.. existsBasicDataList]);

                _logger.Info($"Successful add duplicate basic rot data to db.");
            }
        }
    }
}
