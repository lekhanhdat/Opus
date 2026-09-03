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
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Calculator.Duplicate
{
    public class RMDiscoveryAOSPDuplicateCalculator
    {
        private const int QUERY_PAGE_SIZE = 1000;

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPDuplicateCalculator));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryAOSPJobDao _jobDao;

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly IRMDiscoveryAOSPRuleInfoDao _ruleInfoDao;

        private readonly IRMDiscoveryAOSPConfigurationDao _configInfoDao;

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao;

        private readonly RMDiscoveryAOSPMainJob _jobInfo;

        public RMDiscoveryAOSPDuplicateCalculator(RMDiscoveryAOSPMainJob jobInfo)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryAOSPJobDao();
            _dataDao = new RMDiscoveryAOSPDataDao();
            _ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();
            _configInfoDao = new RMDiscoveryAOSPConfigurationDao();
            _nodeDao = new RMDiscoveryAOSPNodeDao();
            _jobInfo = jobInfo;
        }
        public async Task CalculateAsync()
        {
            try
            {
                _logger.Info($"Start calculate duplciate data(AOSP)");

                var duplicateRules = await _ruleInfoDao.GetRuleInfoesAsync(true, _jobInfo.O365TenantId, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument);
                if (!duplicateRules.Any())
                {
                    _logger.Warn("Current tenant no duplicate rule found.");
                    return;
                }

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_jobInfo.Id);
                var o365TenantIds = discoveryJobs.Select(item => item.O365TenantId).ToHashSet();

                foreach (var o365TenantId in o365TenantIds)
                {
                    var rotConfig = await _configInfoDao.GetByO365TenantIdAsync<RMDiscoveryAOSPRotDefinition>(RMDiscoveryConfigurationType.AOSPROTDefinition, o365TenantId.ToString());
                    if (!rotConfig.Enable)
                    {
                        _logger.Warn($"Current tenant not enable rot.");
                        return;
                    }

                    foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                    {
                        var duplicateFileTotalSize = await CalculateAsync(o365TenantId, contentSource);
                        var aggregateInfo = await _dataDao.GetAggregateTotalDataAsync(o365TenantId, contentSource);
                        aggregateInfo.DuplicateFileTotalSize = duplicateFileTotalSize;
                        await _dataDao.AddOrUpdateAggregateTotalDataAsync(o365TenantId, aggregateInfo);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate duplicate data. Error: {e}");
            }
        }

        private async Task<long> CalculateAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            try
            {
                _logger.Info($"Start calculate office 365 [{o365TenantId}] content source [{contentSource}] duplicate data.");

                var siteUniqueIds = (await _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId, contentSource).ToListAsync()).Select(item => item.SiteId.ToString().ToLower()).ToHashSet();

                var duplicateTotalSize = 0L;
                var needGTSize = 0L;
                while (true)
                {
                    var items = new List<RMDiscoveryOffice365DuplicateItemInfo>();
                    var hasNextAndNeedExpandQueryScope = true;
                    for (var i = 0; hasNextAndNeedExpandQueryScope; i++)
                    {
                        var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[contentSource]}?" +
                            $"$skip={i * QUERY_PAGE_SIZE}" +
                            $"&$top={QUERY_PAGE_SIZE}" +
                            $"&$filter=FileSize gt {needGTSize} " +
                            $"and not IsPHL " +
                            $"&$orderby=FileSize" +
                            $"&select=Name,FileSize,SiteId";
                        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, o365TenantId.ToString());
                        var innerItems = JsonConvert.DeserializeObject<List<RMDiscoveryOffice365DuplicateItemInfo>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                        items.AddRange(innerItems);

                        hasNextAndNeedExpandQueryScope = innerItems.Count == QUERY_PAGE_SIZE && items.GroupBy(item => item.FileSize).Count() == 1;
                        if (hasNextAndNeedExpandQueryScope)
                        {
                            _logger.Warn($"Office 365 [{o365TenantId}] content source [{contentSource}] ge [{needGTSize}] need expand query data scope.");
                        }
                    }

                    if (items.Count == 0)
                    {
                        break;
                    }

                    var needCalculateItems = items;
                    var lastGTIndex = items.FindIndex(item => item.FileSize == items.Last().FileSize) - 1;

                    if (items.Count % QUERY_PAGE_SIZE == 0 && lastGTIndex >= 0)
                    {
                        needCalculateItems = needCalculateItems.Take(lastGTIndex + 1).ToList();
                        needGTSize = needCalculateItems.Last().FileSize;
                    }

                    needCalculateItems
                        .Where(item => siteUniqueIds.Contains(item.SiteId.ToLower()))
                        .GroupBy(item => item.Name + "_" + item.FileSize).ToList().ForEach(group =>
                        {
                            if (group.Count() > 1)
                            {
                                var duplicateSize = group.Sum(item => item.FileSize);
                                duplicateTotalSize += duplicateSize;
                            }
                        });

                    if (lastGTIndex < 0 || items.Count % QUERY_PAGE_SIZE > 0)
                    {
                        break;
                    }
                }

                _logger.Info($"End calculate office 365 [{o365TenantId}] content source [{contentSource}] duplicate data. Duplicate data size [{duplicateTotalSize}].");

                return duplicateTotalSize;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate office 365 [{o365TenantId}] content source [{contentSource}] duplicate data. Error: {e}");
                return 0L;
            }
        }
    }

    public class RMDiscoveryOffice365DuplicateItemInfo
    {
        public string Name { get; set; }

        public long FileSize { get; set; }

        public string SiteId { get; set; }
    }
}
