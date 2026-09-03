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
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;

//using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
//using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Discovery.AOSP
{
    public class RMDiscoveryAOSPOptimizationCalculator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPOptimizationCalculator));

        private readonly IRMDiscoveryAOSPFileExtensionDao _fileExtensionDao = new RMDiscoveryAOSPFileExtensionDao();

        private readonly IRMDiscoveryAOSPRuleInfoDao _ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();

        private readonly IRMDiscoveryAOSPProgressDao _optimizationDao = new RMDiscoveryAOSPProgressDao();

        public static readonly ImmutableDictionary<SourceFlag, string> ODATA_URI =
            ImmutableDictionary.CreateRange([
                KeyValuePair.Create(SourceFlag.SharePoint, "odata/spdocument"),
                KeyValuePair.Create(SourceFlag.OneDrive, "odata/sponedrivedocuments"),
            ]);

        private readonly IEApiClient _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();

        private readonly Guid _o365TenantId;

        private Guid _settingId;

        private readonly long _beforeScheduleTicks;

        private readonly RMDiscoveryAOSPSiteInfo _siteInfo;

        private RMDiscoveryAOSPOptimizationSetting _optimizationSettingInfo;

        private RMDiscoveryAOSPOptimizationSettingsInfo _optimizationAllSettingInfo;

        public RMDiscoveryAOSPOptimizationCalculator(Guid o365TenantId, RMDiscoveryAOSPSiteInfo siteInfo, long beforeScheduleTicks)
        {
            _beforeScheduleTicks = beforeScheduleTicks;
            _o365TenantId = o365TenantId;
            _siteInfo = siteInfo;
        }

        public async Task CalculateAsync()
        {
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationCalculate, _siteInfo.SiteId.ToString(), TimeSpan.FromMinutes(10)))
                {
                    await InitNextSettingInfo();
                    var fileTotalSize = 0L;
                    var versionTotalSize = 0L;
                    var nextRunTime = _beforeScheduleTicks;
                    if (_optimizationSettingInfo != null)
                    {
                        nextRunTime = _optimizationAllSettingInfo.NextTime == 0 ? DateTime.UtcNow.Ticks : _optimizationAllSettingInfo.NextTime;
                        (fileTotalSize, versionTotalSize) = await CalculateOptimizableAsync();
                    }

                    var optimizationData = await _optimizationDao.GetSiteOptimizedInfoAsync(_o365TenantId, _siteInfo.Id);
                    optimizationData ??= new RMDiscoveryAOSPSiteOptimizedInfo
                    {
                        SiteId = _siteInfo.Id,
                        SettingId = _settingId,
                        NextOptimizationTime = 0,
                        NextOptimizableFileTotalSize = 0,
                        NextOptimizableVersionTotalSize = 0,
                        Archived = 0,
                        Deleted = 0,
                        LastOptimizedTime = 0L,
                        ContentSource = _siteInfo.ContentSource,
                    };

                    optimizationData.NextOptimizationTime = nextRunTime;
                    optimizationData.NextOptimizableFileTotalSize = fileTotalSize;
                    optimizationData.NextOptimizableVersionTotalSize = versionTotalSize;

                    await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(_o365TenantId, optimizationData);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate site next schedule job optimization info. Error: {e}");
            }
        }

        private async Task InitNextSettingInfo()
        {
            var settingInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();
            var settingInfo = await settingInfoDao.GetLatestSettingAsync(_o365TenantId, _siteInfo.SiteId, _beforeScheduleTicks);
            if (settingInfo != null)
            {
                _optimizationAllSettingInfo = settingInfo;
                _settingId = settingInfo.SettingId;
                _optimizationSettingInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryAOSPOptimizationSetting>(RMDiscoveryAOSPOptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            }
        }

        private async Task<(long FileTotalSize, long VersionTotalSize)> CalculateOptimizableAsync()
        {
            var fileTotalSize = 0L;
            var versionTotalSize = 0L;
            var filters = await GetBasicFiltersAsync();
            if (_optimizationSettingInfo.ArchiveDataType == (int)ArchiverDataType.All)
            {
                var sql = $"{ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", filters)})" +
                    $"/aggregate(FileSize with sum as file_total_size, HistoryVersionsSize with sum as version_total_size)";
                fileTotalSize = await CalculateFileTotalSizeAsync(sql);
            }
            else
            {
                var fileRules = new List<RMDiscoveryAOSPRuleInfo>();
                var versionRules = new List<RMDiscoveryAOSPRuleInfo>();
                if (_optimizationSettingInfo.InactiveRuleQueryParameter.Enable)
                {
                    var rules = await DiscoverUtil.GetInactiveRuleAsync(_o365TenantId.ToString(), _optimizationSettingInfo.InactiveRuleQueryParameter, _optimizationSettingInfo.ArchiveDataType);
                    versionRules.AddRange(rules);
                }

                if (_optimizationSettingInfo.ROTRuleQueryParameter.Enable)
                {
                    var rules = await DiscoverUtil.GetROTRuleAsync(_o365TenantId.ToString(), _optimizationSettingInfo.ROTRuleQueryParameter, _optimizationSettingInfo.ArchiveDataType);
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
                    var sql = $"{ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", fileFilters)})" +
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
                    var sql = $"{ODATA_URI[_siteInfo.ContentSource]}?$apply=filter({string.Join(" and ", filters)})" +
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
            return GetExpandObjectValue<long>(dataObj, "file_total_size") + GetExpandObjectValue<long>(dataObj, "version_total_size");
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
            return versionSumNames.Sum(item => GetExpandObjectValue<long>(dataObj, item));
        }

        private async Task<List<string>> GetBasicFiltersAsync()
        {
            var filters = new List<string>
            {
                $"SiteId eq '{_siteInfo.SiteId}'",
                "not IsPHL",
                $"{RMDiscoveryBuildInRule.ARCHVIED_COLUMN_NAME} ne 1"
            };
            if (_optimizationSettingInfo.WithoutDateQueryParameter != null)
            {
                if (_optimizationSettingInfo.WithoutDateQueryParameter.From > 0)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} gt {_optimizationSettingInfo.WithoutDateQueryParameter.From}");
                }
                filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} le {_optimizationSettingInfo.WithoutDateQueryParameter.To}");
            }
            if (_optimizationSettingInfo.FileExtensionQueryParameter?.FileExtensions != null
        && _optimizationSettingInfo.FileExtensionQueryParameter?.FileExtensions.Count > 0)
            {
                var fileExtensions = await _fileExtensionDao.GetAsync(_o365TenantId, _optimizationSettingInfo.FileExtensionQueryParameter.FileExtensions);
                if (fileExtensions.Any())
                {
                    filters.Add($"FileExtension in ('{string.Join("','", fileExtensions.Select(e => e.Name))}')");
                }
            }
            if (_optimizationSettingInfo.SizeRangeQueryParameter != null
        && _optimizationSettingInfo.SizeRangeQueryParameter.QueryMode != RMDiscoveryAOSPSizeRangeQueryMode.None
        && _optimizationSettingInfo.SizeRangeQueryParameter.SizeRange > 0)
            {
                var rangeId = _optimizationSettingInfo.SizeRangeQueryParameter.SizeRange;
                var condition = _optimizationSettingInfo.SizeRangeQueryParameter.QueryMode switch
                {
                    RMDiscoveryAOSPSizeRangeQueryMode.LessThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} le {rangeId}",
                    RMDiscoveryAOSPSizeRangeQueryMode.GenerateThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} ge {rangeId}",
                    _ => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {rangeId}"
                };
                filters.Add(condition);
            }

            return filters;
        }

        private static T GetExpandObjectValue<T>(ExpandoObject data, string key, T defaultValue = default) where T : struct
        {
            var res = data.TryGet<T>(key);
            if (res == null)
            {
                return defaultValue;
            }

            return res.Value;
        }
    }
}
