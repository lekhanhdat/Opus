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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
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
using DocumentFormat.OpenXml.Wordprocessing;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate
{
    public class RMDiscoveryOffice365DuplicateCalculatorV3
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicateCalculatorV3));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        private readonly IRMDiscoveryConfigurationDao _configInfoDao;

        private readonly RMDiscoveryOffice365MainJob _jobInfo;

        public RMDiscoveryOffice365DuplicateCalculatorV3(RMDiscoveryOffice365MainJob jobInfo)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _dataDao = new RMDiscoveryOffice365DataDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _configInfoDao = new RMDiscoveryConfigurationDao();
            _jobInfo = jobInfo;
        }

        public async Task CalculateAsync()
        {
            try
            {
                _logger.Info($"Start calculate duplicate data(SQLite).");

                var rotConfig = await _configInfoDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
                if(!rotConfig.Enable)
                {
                    _logger.Warn($"Current tenant not enable rot.");
                    return;
                }

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
                    var fileExtensionAnalysisManager = new RMDiscoveryOffice365FileExtensionAnalysisManager(o365TenantId);
                    await fileExtensionAnalysisManager.InitAsync();

                    var contentSources = new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive };
                    _logger.Info($"The o365 tenant [{o365TenantId}] need to calculate duplicate data in content sources [{string.Join(", ", contentSources)}].");

                    foreach(var contentSource in contentSources)
                    {
                        using var cacheManger = new RMDiscoveryOffice365DuplicateCalculateCacheManager();
                        var siteInfoes = await _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId, contentSource).ToListAsync();
                        await AnalysisDuplicateItemUniqueIdsAsync(o365TenantId, contentSource, siteInfoes, cacheManger);
                        var duplicateFileTotalSize = await AnalysisSitesDuplicateDataAsync(o365TenantId, contentSource, cacheManger);
                        var aggregateInfo = await _dataDao.GetAggregateTotalDataAsync(o365TenantId, contentSource);
                        aggregateInfo.DuplicateFileTotalSize = duplicateFileTotalSize;
                        await _dataDao.AddOrUpdateAggregateTotalDataAsync(o365TenantId, aggregateInfo);
                    }
                }

                _logger.Info($"End calculate duplicate data.");
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while calculate duplicate data. Error: {e}");
            }
        }

        private async Task AnalysisDuplicateItemUniqueIdsAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryOffice365SiteInfo> siteInfoes, RMDiscoveryOffice365DuplicateCalculateCacheManager cacheManager)
        {
            try
            {
                _logger.Info($"Start get o365 tenant [{o365TenantId}] [{contentSource}] duplicate data.");
                const int pageSize = 1000;
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
                            cacheManager.InsertOrUpdateDuplicateData(hashCode, itemUniqueId);
                        }

                        if (items.Count == 0 || items.Count < pageSize)
                        {
                            break;
                        }

                        maxItemId = items.Last().GetValue<long>("ItemId");
                    }
                }

                _logger.Info($"End analysis o365 tenant [{o365TenantId}] [{contentSource}] duplicate data.");
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get o365 tenant [{o365TenantId}] [{contentSource}] duplicate data. Error: {e}");
            }
        }

        private async Task<long> AnalysisSitesDuplicateDataAsync(
            Guid o365TenantId, 
            SourceFlag contentSource,
            RMDiscoveryOffice365DuplicateCalculateCacheManager cacheManager)
        {
            try
            {

                var duplicateFileTotalSize = 0L;

                _logger.Info($"Start analysis o365 tenant [{o365TenantId}] [{contentSource}] sites duplicate data.");
                var unRelateRuleDataList = new List<RMDiscoveryOffice365SiteRotData>();
                foreach(var itemUniqueIds in cacheManager.GetAllDuplicateItemUniqueIds())
                {
                    var batchItemUniqueIds = itemUniqueIds.ConvertAll(item => $"'{item}'");
                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[contentSource]}?" +
                        $"$filter=ObjectId in ({string.Join(",", batchItemUniqueIds)})" +
                        $"&select=FileSize, HistoryVersionsSize";
                    var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, o365TenantId.ToString());
                    var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                    foreach (var item in items)
                    {
                        var fileSize = item.GetValue<long>("FileSize");
                        var historyVersionSize = item.GetValue<long>("HistoryVersionsSize");
                        duplicateFileTotalSize += (fileSize + historyVersionSize);
                    }
                }

                _logger.Info($"End analysis o365 tenant [{o365TenantId}] [{contentSource}] duplicate data. Total size [{duplicateFileTotalSize}].");

                return duplicateFileTotalSize;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{o365TenantId}] [{contentSource}] sites duplicate data. Error: {e}");
                return 0L;
            }
        }
    }
}
