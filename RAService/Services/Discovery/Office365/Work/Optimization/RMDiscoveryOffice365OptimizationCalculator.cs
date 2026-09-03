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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Optimization
{
    public class RMDiscoveryOffice365OptimizationCalculator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365OptimizationCalculator));

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        private readonly IRMDiscoveryOffice365ProgressDao _optimizationDao = new RMDiscoveryOffice365ProgeressDao();

        private readonly IEApiClient _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();

        private readonly Guid _o365TenantId;

        private readonly Guid _settingId;

        private readonly RMDiscoveryOffice365SiteInfo _siteInfo;

        private readonly RMDiscoveryOffice365OptimizationSettingsInfo _settingInfo;

        public RMDiscoveryOffice365OptimizationCalculator(Guid o365TenantId, RMDiscoveryOffice365SiteInfo siteInfo, RMDiscoveryOffice365OptimizationSettingsInfo settingInfo)
        {
            _o365TenantId = o365TenantId;
            _settingId = settingInfo.SettingId;
            _siteInfo = siteInfo;
            _settingInfo = settingInfo;
        }

        public async Task CalculateAsync()
        {
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationCalculate, _siteInfo.SiteId.ToString(), TimeSpan.FromMinutes(20)))
                {
                    var fileTotalSize = 0L;
                    var versionTotalSize = 0L;
                    var nextRunTime = 0L;
                    var optimizationSettingInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(_settingInfo.Setting));
                    if (optimizationSettingInfo != null)
                    {
                        nextRunTime = _settingInfo.NextTime == 0 ? DateTime.UtcNow.Ticks : _settingInfo.NextTime;
                        (fileTotalSize, versionTotalSize) = await CalculateOptimizableAsync();
                    }

                    var optimizationData = await _optimizationDao.GetSiteOptimizedInfoAsync(_o365TenantId, _siteInfo.Id);
                    optimizationData ??= new RMDiscoveryOffice365SiteOptimizedInfo
                    {
                        SiteId = _siteInfo.Id,
                        SettingId = _settingId,
                        NextOptimizationTime = 0,
                        NextOptimizableFileTotalSize = 0,
                        NextOptimizableVersionTotalSize = 0,
                        Archived = 0,
                        Deleted = 0,
                        LastOptimizedTime = 0L
                    };

                    optimizationData.NextOptimizationTime = nextRunTime;
                    optimizationData.NextOptimizableFileTotalSize = fileTotalSize;
                    optimizationData.NextOptimizableVersionTotalSize = versionTotalSize;

                    await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(_o365TenantId, optimizationData);
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while calculate site [{_siteInfo.Url}] setting [{_settingId}] optmization info. Error: {e}");
            }
        }

        private async Task<(long FileTotalSize, long VersionTotalSize)> CalculateOptimizableAsync()
        {
            var fileTotalSize = 0L;
            var versionTotalSize = 0L;
            var filters = await GetBasicFiltersAsync();
            var optimizationSettingInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(_settingInfo.Setting));
            if (optimizationSettingInfo.ArchiveDataType == (int)ArchiverDataType.All)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", filters)})" +
                    $"/aggregate(FileSize with sum as file_total_size, HistoryVersionsSize with sum as version_total_size)";
                fileTotalSize = await CalculateFileTotalSizeAsync(sql);
            }
            else
            {
                var fileRules = new List<RMDiscoveryOffice365RuleInfo>();
                var versionRules = new List<RMDiscoveryOffice365RuleInfo>();
                if (optimizationSettingInfo.InactiveRuleQueryParameter.Enable)
                {
                    var rules = await DiscoverUtil.GetInactiveRuleAsync(optimizationSettingInfo.InactiveRuleQueryParameter, optimizationSettingInfo.ArchiveDataType);
                    versionRules.AddRange(rules);
                }

                if (optimizationSettingInfo.ROTRuleQueryParameter.Enable)
                {
                    var rules = await DiscoverUtil.GetROTRuleAsync(optimizationSettingInfo.ROTRuleQueryParameter, optimizationSettingInfo.ArchiveDataType);
                    fileRules.AddRange(rules.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document));
                    versionRules.AddRange(rules.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version));
                }

                if (fileRules.Any())
                {
                    var fileRulesColumns = fileRules.ConvertAll(item => new
                    {
                        ColumnName = "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                        SumName = item.ToCustomColumn().Name
                    });

                    var fileRuleFilter = $"({string.Join(" or ", fileRulesColumns.ConvertAll(item => $"{item.ColumnName} gt 0"))})";
                    var fileFilters = new List<string>(filters)
                    {
                        fileRuleFilter
                    };
                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", fileFilters)})" +
                    $"/aggregate(FileSize with sum as file_total_size, HistoryVersionsSize with sum as version_total_size)";
                    fileTotalSize = await CalculateFileTotalSizeAsync(sql);
                }

                if (versionRules.Any())
                {
                    var versionRulesColumns = versionRules.ConvertAll(item => new
                    {
                        ColumnName = "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                        SumName = item.ToCustomColumn().Name
                    });
                    var sumSqls = versionRulesColumns.ConvertAll(item => $"{item.ColumnName}/total_size with sum as {item.SumName}");
                    var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", filters)})" +
                    $"/aggregate(FileSize with sum as file_total_size, HistoryVersionsSize with sum as version_total_size, {string.Join(", ", sumSqls)})";
                    versionTotalSize = await CalculateVersionTotalSizeAsync(sql, versionRulesColumns.Select(item => item.SumName).ToList());
                }
            }

            return (fileTotalSize, versionTotalSize);
        }

        private async Task<long> CalculateFileTotalSizeAsync(string sql)
        {
            var dataJson = await _ieApiClient.GetByODataUrlAsync(sql, _o365TenantId.ToString());
            var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
            if (!dataObjList.Any())
            {
                return 0;
            }

            var dataObj = dataObjList.First();
            return dataObj.GetValue<long>("file_total_size") + dataObj.GetValue<long>("version_total_size");
        }

        private async Task<long> CalculateVersionTotalSizeAsync(string sql, List<string> versionSumNames)
        {
            var dataJson = await _ieApiClient.GetByODataUrlAsync(sql, _o365TenantId.ToString());
            var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
            if (!dataObjList.Any())
            {
                return 0;
            }

            var dataObj = dataObjList.First();
            return versionSumNames.Sum(item => dataObj.GetValue<long>(item));
        }

        private async Task<List<string>> GetBasicFiltersAsync()
        {
            var optimizationSettingInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(_settingInfo.Setting));
            var filters = new List<string>
            {
                $"SiteId eq '{_siteInfo.SiteId}'",
                $"{RMDiscoveryBuildInRule.ARCHVIED_COLUMN_NAME} ne 1"
            };

            if (optimizationSettingInfo.MS365DataType == (int)MS365DataType.Phl)
            {
                filters.Add("IsPHL eq true");
            }
            else
            {
                filters.Add("IsPHL ne true");
            }

            if (optimizationSettingInfo.WithoutDateQueryParameter != null)
            {
                if (optimizationSettingInfo.WithoutDateQueryParameter.From > 0)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} gt {optimizationSettingInfo.WithoutDateQueryParameter.From}");
                }
                if (optimizationSettingInfo.WithoutDateQueryParameter.To < 999)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} le {optimizationSettingInfo.WithoutDateQueryParameter.To}");
                }
            }
            if (optimizationSettingInfo.FileExtensionQueryParameter?.FileExtensions != null
        && optimizationSettingInfo.FileExtensionQueryParameter?.FileExtensions.Count > 0)
            {
                var fileExtensions = await _fileExtensionDao.GetAsync(_o365TenantId, optimizationSettingInfo.FileExtensionQueryParameter.FileExtensions);
                if (fileExtensions.Any())
                {
                    filters.Add($"FileExtension in ('{string.Join("','", fileExtensions.Select(e => e.Name))}')");
                }
            }
            if (optimizationSettingInfo.SizeRangeQueryParameter != null
        && optimizationSettingInfo.SizeRangeQueryParameter.QueryMode != RMDiscoverySizeRangeQueryMode.None
        && optimizationSettingInfo.SizeRangeQueryParameter.SizeRange > 0)
            {
                var rangeId = optimizationSettingInfo.SizeRangeQueryParameter.SizeRange;
                var condition = optimizationSettingInfo.SizeRangeQueryParameter.QueryMode switch
                {
                    RMDiscoverySizeRangeQueryMode.LessThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} le {rangeId}",
                    RMDiscoverySizeRangeQueryMode.GenerateThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} ge {rangeId}",
                    _ => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {rangeId}"
                };
                filters.Add(condition);
            }

            return filters;
        }
    }
}
