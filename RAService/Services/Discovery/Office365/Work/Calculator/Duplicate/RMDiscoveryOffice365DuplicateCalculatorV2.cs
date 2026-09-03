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
using Aspose.Pdf;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate
{
    public class RMDiscoveryOffice365DuplicateCalculatorV2
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicateCalculatorV2));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        private readonly RMDiscoveryOffice365MainJob _jobInfo;

        public RMDiscoveryOffice365DuplicateCalculatorV2(RMDiscoveryOffice365MainJob jobInfo)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _dataDao = new RMDiscoveryOffice365DataDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _jobInfo = jobInfo;
        }

        public async Task CalculateAsync()
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
                    return;
                }

                foreach(var o365TenantId in o365TenantIds)
                {
                    await ClearDataAsync(o365TenantId, duplicateRules);
                    var fileExtensionAnalysisManager = new RMDiscoveryOffice365FileExtensionAnalysisManager(o365TenantId);
                    await fileExtensionAnalysisManager.InitAsync();

                    var contentSources = new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive };
                    _logger.Info($"The o365 tenant [{o365TenantId}] need to calculate duplicate data in content sources [{string.Join(", ", contentSources)}].");

                    foreach(var contentSource in contentSources)
                    {
                        var siteInfoes = await _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId, contentSource).ToListAsync();
                        var duplicateItemUniqueIds = await GetDuplicateItemUniqueIdsAsync(o365TenantId, contentSource, siteInfoes);
                        var siteRotDataList = await AnalysisSitesDuplicateDataAsync(o365TenantId, contentSource, duplicateItemUniqueIds, siteInfoes, duplicateRules, fileExtensionAnalysisManager);
                        await AnalysisContainerDuplicateDataAsync(o365TenantId, siteRotDataList);
                        await AnalysisBasicDuplicateDataAsync(o365TenantId, contentSource, siteRotDataList);
                    }
                }

                _logger.Info($"End calculate duplicate data.");
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while calculate duplicate data. Error: {e}");
            }
        }

        private async Task<List<string>> GetDuplicateItemUniqueIdsAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryOffice365SiteInfo> siteInfoes)
        {
            try
            {
                _logger.Info($"Start get o365 tenant [{o365TenantId}] [{contentSource}] duplicate data.");
                const int pageSize = 1000;
                var duplicateDic = new Dictionary<int, List<string>>();
                foreach(var  siteInfo in siteInfoes)
                {
                    var maxItemId = 0L;
                    while(true)
                    {
                        var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[contentSource]}?" +
                            $"$top={pageSize}" +
                            $"&$filter=SiteId eq '{siteInfo.SiteId}' " +
                            $"and ItemId gt {maxItemId} " +
                            $"and not IsPHL " +
                            $"&$orderby=ItemId" +
                            $"&select=ItemId,ObjectId,Name,FileSize";
                        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, o365TenantId.ToString());
                        var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                        foreach (var item in items)
                        {
                            var itemUniqueId = item.GetValue("ObjectId");
                            var fileName = item.GetValue("Name");
                            var fileSize = item.GetValue<long>("FileSize");
                            var hashCode = $"{fileName}_{fileSize}".GetHashCode();
                            if (!duplicateDic.TryGetValue(hashCode, out var duplicateItemList))
                            {
                                duplicateItemList = [];
                                duplicateDic[hashCode] = duplicateItemList;
                            }
                            duplicateItemList.Add(itemUniqueId);
                        }

                        if (items.Count == 0 || items.Count < pageSize)
                        {
                            break;
                        }

                        maxItemId = items.Last().GetValue<long>("ItemId");
                    }
                }

                var res = duplicateDic.Values.Where(item => item.Count > 1).SelectMany(item => item).ToList();
                _logger.Info($"End get o365 tenant [{o365TenantId}] [{contentSource}] duplicate data. Item count [{res.Count}].");

                return res;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get o365 tenant [{o365TenantId}] [{contentSource}] duplicate data. Error: {e}");
                return [];
            }
        }

        private async Task<List<RMDiscoveryOffice365SiteRotData>> AnalysisSitesDuplicateDataAsync(
            Guid o365TenantId, 
            SourceFlag contentSource, 
            List<string> itemUniqueIds, 
            List<RMDiscoveryOffice365SiteInfo> siteInfoes, 
            List<RMDiscoveryOffice365RuleInfo> duplicateRules,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionManager)
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{o365TenantId}] [{contentSource}] sites duplicate data.");
                var unRelateRuleDataList = new List<RMDiscoveryOffice365SiteRotData>();
                const int pageSize = 100;
                for(var i = 0; i < itemUniqueIds.Count; i+=pageSize) 
                {
                    var batchItemUniqueIds = itemUniqueIds.Skip(i).Take(pageSize).ConvertAll(item => $"'{item}'").ToList();
                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[contentSource]}?" +
                        $"$filter=ObjectId in ({string.Join(",", batchItemUniqueIds)})" +
                        $"&select=FileSize, FileExtension, HistoryVersionsSize, SiteId, {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME}, {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME}";
                    var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, o365TenantId.ToString());
                    var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                    foreach(var item in items)
                    {
                        var fileSize = item.GetValue<long>("FileSize");

                        var fileExtension = item.GetValue("FileExtension");
                        await fileExtensionManager.AddOrUpdateAsync(fileExtension);
                        var fileExtensionIntId = fileExtensionManager.GetId(fileExtension);

                        var historyVersionSize = item.GetValue<long>("HistoryVersionsSize");
                        var siteUniqueId = new Guid(item.GetValue("SiteId"));
                        var siteInfo = siteInfoes.First(item => item.SiteId == siteUniqueId);
                        var sizeRange = (int)item.GetValue<long>(RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME);
                        var withoutDate = (int)item.GetValue<long>(RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME);
                        var matchedRotData = unRelateRuleDataList.FirstOrDefault(item =>
                            item.SiteId == siteInfo.Id &&
                            item.SizeRange == sizeRange &&
                            item.WithoutInDate == withoutDate &&
                            item.FileExtension == fileExtensionIntId);
                        if(matchedRotData == null)
                        {
                            matchedRotData = new RMDiscoveryOffice365SiteRotData
                            {
                                SiteId = siteInfo.Id,
                                ContainerId = siteInfo.ContainerId,
                                SizeRange = sizeRange,
                                WithoutInDate = withoutDate,
                                FileExtension = fileExtensionIntId
                            };
                            unRelateRuleDataList.Add(matchedRotData);
                        }
                        matchedRotData.FileSumCount++;
                        matchedRotData.FileTotalSize += (historyVersionSize + fileSize);
                    }
                }

                var res = new List<RMDiscoveryOffice365SiteRotData>();
                foreach(var duplicateRule in duplicateRules)
                {
                    var relateRuleDataList = unRelateRuleDataList.ConvertAll(item => new RMDiscoveryOffice365SiteRotData 
                    {
                        SiteId = item.SiteId,
                        ContainerId = item.ContainerId,
                        SizeRange = item.SizeRange,
                        WithoutInDate = item.WithoutInDate,
                        FileExtension = item.FileExtension,
                        FileSumCount = item.FileSumCount,
                        FileTotalSize = item.FileTotalSize,
                        Rule = duplicateRule.Id
                    });

                    res.AddRange(relateRuleDataList);
                }

                await _dataDao.AddSiteRotDataAsync(o365TenantId, res.ToArray());

                _logger.Info($"End analysis o365 tenant [{o365TenantId}] [{contentSource}] sites duplicate data. Item count [{res.Count}].");

                return res;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{o365TenantId}] [{contentSource}] sites duplicate data. Error: {e}");
                return [];
            }
        }

        private async Task AnalysisContainerDuplicateDataAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRotData> siteRotDataList)
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{o365TenantId}] container duplicate data");
                var res = new List<RMDiscoveryOffice365ContainerRotData>();
                foreach (var siteRotData in siteRotDataList)
                {
                    var matchedRotData = res.FirstOrDefault(item =>
                        item.ContainerId == siteRotData.ContainerId &&
                        item.SizeRange == siteRotData.SizeRange &&
                        item.WithoutInDate == siteRotData.WithoutInDate &&
                        item.FileExtension == siteRotData.FileExtension &&
                        item.Rule == siteRotData.Rule
                        );
                    if (matchedRotData == null)
                    {
                        matchedRotData = new RMDiscoveryOffice365ContainerRotData
                        {
                            ContainerId = siteRotData.ContainerId,
                            SizeRange = siteRotData.SizeRange,
                            WithoutInDate = siteRotData.WithoutInDate,
                            FileExtension = siteRotData.FileExtension,
                            Rule = siteRotData.Rule,
                        };
                        res.Add(matchedRotData);
                    }

                    matchedRotData.FileSumCount += siteRotData.FileSumCount;
                    matchedRotData.FileTotalSize += siteRotData.FileTotalSize;
                }

                _logger.Info($"End analysis o365 tenant [{o365TenantId}] container duplicate data. Item count [{res.Count}].");

                await _dataDao.AddOrUpdateContainerRotDataAsync(o365TenantId, res.ToArray());
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{o365TenantId}] container duplicate data. Error: {e}");
            }
        }

        private async Task AnalysisBasicDuplicateDataAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryOffice365SiteRotData> siteRotDataList)
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{o365TenantId}] basic duplicate data");
                var res = new List<RMDiscoveryOffice365BasicRotData>();
                foreach (var siteRotData in siteRotDataList)
                {
                    var matchedRotData = res.FirstOrDefault(item =>
                        item.SizeRange == siteRotData.SizeRange &&
                        item.WithoutInDate == siteRotData.WithoutInDate &&
                        item.FileExtension == siteRotData.FileExtension &&
                        item.Rule == siteRotData.Rule
                        );
                    if (matchedRotData == null)
                    {
                        matchedRotData = new RMDiscoveryOffice365BasicRotData
                        {
                            ContentSource = contentSource,
                            SizeRange = siteRotData.SizeRange,
                            WithoutInDate = siteRotData.WithoutInDate,
                            FileExtension = siteRotData.FileExtension,
                            Rule = siteRotData.Rule,
                        };
                        res.Add(matchedRotData);
                    }

                    matchedRotData.FileSumCount += siteRotData.FileSumCount;
                    matchedRotData.FileTotalSize += siteRotData.FileTotalSize;
                }

                _logger.Info($"End analysis o365 tenant [{o365TenantId}] basic duplicate data. Item count [{res.Count}].");

                await _dataDao.AddBasicRotDataAsync(o365TenantId, res.ToArray());
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{o365TenantId}] basic duplicate data. Error: {e}");
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
    }
}
