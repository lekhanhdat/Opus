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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export
{
    public class RMDiscoveryOffice365ExportDuplicationReportProcessor
    {
        private const int QUERY_PAGE_SIZE = 1000;

        private const int LARGE_CACHE_THRESHOLD = 100000;

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicateCalculatorV4));

        private readonly IGeneralSettingService _generalSettingService = RA.Common.PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        private readonly IRMDiscoveryConfigurationDao _configInfoDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryOffice365MainJob _jobInfo;

        private readonly IRMReportManager _reportManager;

        private Func<RMDiscoveryOffice365DuplicationReportInfo, IDictionary<string, object>> _rowMapper;

        private RMDiscoveryOffice365DuplicationDataExportor<RMDiscoveryOffice365DuplicationReportInfo> _exportor;

        private GeneralSettingModel _gls;

        private readonly string _baseFolderPath;
        
        private readonly Guid _o365TenantId;

        private Dictionary<(string Name, long FileSize), int> _groupIndexMap = new();
        
        private int _duplicatedGroupIndex = 1;
        
        private long _exportedCount = 0;

        private long? _activeFileSize;
        
        private  HashSet<(string, long)> _exportedKeys = new HashSet<(string, long)>();

        public RMDiscoveryOffice365ExportDuplicationReportProcessor(RMDiscoveryOffice365MainJob jobInfo, IRMReportManager reportManager, string baseFolderPath, Guid o365TenantId)
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _configInfoDao = new RMDiscoveryConfigurationDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobInfo = jobInfo;
            _reportManager = reportManager;
            _baseFolderPath = baseFolderPath;
            _o365TenantId = o365TenantId;
        }

        public RMDiscoveryOffice365ExportDuplicationReportProcessor Initialize()
        {
            _logger.Info("Initialize export duplication report processor.");
            var header = RMDiscoveryOffice365ReportHeaderDefinition.FromList(new[]
            {
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_DuplicatedGroup"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileName"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_ItemId"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileUrl"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_SiteCollection"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_LastModifedTime"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileType"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_VersionSize"),
                I18NEntity.GetString("RM_JS_JM_Discovery_Report_Action"),
            });
            _rowMapper = r => new Dictionary<string, object>
            {
                [header.OrderedColumns[0]] = r.DuplicatedGroup,
                [header.OrderedColumns[1]] = r.Name,
                [header.OrderedColumns[2]] = r.ObjectId,
                [header.OrderedColumns[3]] = r.FullUrl,
                [header.OrderedColumns[4]] = r.SiteUrl,
                [header.OrderedColumns[5]] = r.ModifiedTime4Display,
                [header.OrderedColumns[6]] = r.FileExtension,
                [header.OrderedColumns[7]] = r.VersionSize,
                [header.OrderedColumns[8]] = string.Empty,
            };
            _exportor = new RMDiscoveryOffice365DuplicationDataExportor<RMDiscoveryOffice365DuplicationReportInfo>(
                _baseFolderPath, "DiscoveryDuplicationReport", header, _rowMapper);
            _logger.Info("Finished initializing export duplication report processor.");
            return this;
        }

        public async Task ExecuteAsync()
        {
            _logger.Info("Start to export duplication report.");
            try
            {
                var rotConfig = await _configInfoDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
                if (!rotConfig.Enable)
                {
                    _logger.Warn($"Current tenant not enable rot.");
                    return;
                }

                var duplicateRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument);
                if (!duplicateRules.Any())
                {
                    _logger.Warn("Current tenant no duplicate rule found.");
                    return;
                }

                _reportManager.StartUpdateJobProgress();
                _gls = await _generalSettingService.GetGeneralSettingAsync();
                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_jobInfo.Id);
                var targetContentSources = discoveryJobs.Where(job => job.O365TenantId == _o365TenantId).Select(job => job.ContentSource).Distinct().ToList();

                if (targetContentSources.Count() == 0)
                {
                    _logger.Warn($"No discovery job with valid content source found for tenant {_o365TenantId}.");
                }

                foreach (var contentSource in targetContentSources)
                {
                    await HandleDuplicationDataAsync(_o365TenantId, contentSource);
                }

                if (_exportedCount == 0) _exportor.ForceExportWithHeaderOnly();
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred during exporting duplication report.", ex);
                throw;
            }
            finally
            {
                _exportor.Dispose();
                _logger.Info($"Finished exporting duplication report. Total exported: {_exportedCount}");
            }
        }

        private async Task HandleDuplicationDataAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            _logger.Info($"Start exporting duplication data. Tenant={o365TenantId}, Source={contentSource}");
            try
            {
                ResetSourceScopedCache(contentSource);

                var siteUrlMapping = await _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId, contentSource)
                    .ToDictionaryAsync(x => x.SiteId.ToString("D"), x => x.Url);
                long needGTSize = 0;

                while (true)
                {
                    var items = new List<RMDiscoveryOffice365DuplicationReportInfo>();
                    var hasNextAndNeedExpandQueryScope = true;
                    for (int page = 0; hasNextAndNeedExpandQueryScope; page++)
                    {
                        var sql =
                            $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[contentSource]}?" +
                            $"$skip={page * QUERY_PAGE_SIZE}" +
                            $"&$top={QUERY_PAGE_SIZE}" +
                            $"&$filter=FileSize gt {needGTSize} " +
                            $"and not IsPHL " +
                            $"and {RMDiscoveryBuildInRule.ARCHVIED_COLUMN_NAME} ne 1"+
                            $"&$orderby=FileSize" +
                            $"&$select=Name,FullUrl,SiteId,ObjectId,FileSize,HistoryVersionsSize,ModifiedTime";

                        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, o365TenantId.ToString());

                        var innerItems = JsonConvert.DeserializeObject<List<RMDiscoveryOffice365DuplicationReportInfo>>(
                            JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));

                        items.AddRange(innerItems);
                        _logger.Info($"IE query completed. Tenant={o365TenantId}, Source={contentSource}, GTFileSize={needGTSize}, BatchItemsCount={items.Count} .memory used: {ProcessUtil.GetProcessMemoryMB()}");

                        hasNextAndNeedExpandQueryScope = innerItems.Count == QUERY_PAGE_SIZE
                            && items.Count > 0
                            && items[0].FileSize == items[^1].FileSize;

                        if (hasNextAndNeedExpandQueryScope)
                        {
                            _logger.Warn($"Office 365 [{o365TenantId}] content source [{contentSource}] gt [{needGTSize}] need expand query data scope.");
                        }
                    }

                    if (items.Count == 0)
                    {
                        break;
                    }

                    var needProcessItems = items;
                    var lastGTIndex = items.FindIndex(item => item.FileSize == items.Last().FileSize) - 1;

                    if (items.Count % QUERY_PAGE_SIZE == 0 && lastGTIndex >= 0)
                    {
                        needProcessItems = needProcessItems.Take(lastGTIndex + 1).ToList();
                        needGTSize = needProcessItems.Last().FileSize;
                    }

                    var validItems = needProcessItems.Where(i => siteUrlMapping.ContainsKey(i.SiteId)).ToList();
                    _reportManager.IncreaseBase(validItems.Count);
                    ProcessDuplicateGroups(validItems, siteUrlMapping);

                    if (lastGTIndex < 0 || items.Count % QUERY_PAGE_SIZE > 0)
                    {
                        break;
                    }
                }

                _logger.Info($"Finished exporting duplication data. Tenant={o365TenantId}, Source={contentSource}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error exporting duplication data. Tenant={o365TenantId}, Source={contentSource}", ex);
                throw;
            }
        }

        private void ResetSourceScopedCache(SourceFlag contentSource)
        {
            _logger.Info($"Reset source-scoped duplication cache. Source={contentSource}, ActiveFileSize={_activeFileSize?.ToString() ?? "null"}, ExportedKeys={_exportedKeys.Count}, GroupIndexMap={_groupIndexMap.Count}");
            _activeFileSize = null;
            _exportedKeys = new HashSet<(string, long)>();
            _groupIndexMap = new Dictionary<(string Name, long FileSize), int>();
        }

        private void ProcessDuplicateGroups(IEnumerable<RMDiscoveryOffice365DuplicationReportInfo> items, Dictionary<string, string> siteUrlMapping)
        {
            int successCount = 0;
            var lastGroupIndex = _duplicatedGroupIndex;
            foreach (var group in items.Where(i => siteUrlMapping.ContainsKey(i.SiteId)).GroupBy(i => (i.Name, i.FileSize)))
            {
                EnsureActiveFileSize(group.Key.FileSize);

                int groupItemCount = group.Count();
                int groupSuccessCount = 0;
                if (groupItemCount <= 1 && !_groupIndexMap.ContainsKey(group.Key)) continue;

                if (!_groupIndexMap.TryGetValue(group.Key, out var groupIndex))
                {
                    groupIndex = lastGroupIndex++;
                    _groupIndexMap[group.Key] = groupIndex;
                }

                foreach (var item in AssembleReportsAsync(group, siteUrlMapping, groupIndex))
                {
                    var exportKey = (item.FullUrl, item.FileSize);
                    if (_exportedKeys.Add(exportKey))
                    {
                        _exportor.WriteData(item);
                        _reportManager.Increase();
                        successCount++;
                        groupSuccessCount++;
                    }
                }
                if (groupIndex == 1 || groupIndex % 10000 == 0)
                {
                    _logger.Info($"Processed duplicated group progress: GroupIndex={groupIndex}, GroupCount={groupItemCount}, SuccessCount={groupSuccessCount}.");
                }
            }
            _exportedCount += successCount;
            _duplicatedGroupIndex = lastGroupIndex;
        }

        private void EnsureActiveFileSize(long fileSize)
        {
            if (!_activeFileSize.HasValue)
            {
                _activeFileSize = fileSize;
                return;
            }

            if (_activeFileSize.Value == fileSize)
            {
                return;
            }

            if (fileSize < _activeFileSize.Value)
            {
                _logger.Warn($"Unexpected file size order detected. CurrentFileSize={_activeFileSize.Value}, IncomingFileSize={fileSize}. Skip clearing caches.");
                return;
            }

            _activeFileSize = fileSize;

            if (_exportedKeys.Count >= LARGE_CACHE_THRESHOLD)
            {
                _exportedKeys = new HashSet<(string, long)>();
            }
            else
            {
                _exportedKeys.Clear();
            }

            if (_groupIndexMap.Count >= LARGE_CACHE_THRESHOLD)
            {
                _groupIndexMap = new Dictionary<(string Name, long FileSize), int>();
            }
            else
            {
                _groupIndexMap.Clear();
            }
        }

        private IEnumerable<RMDiscoveryOffice365DuplicationReportInfo> AssembleReportsAsync(IGrouping<(string Name, long FileSize), RMDiscoveryOffice365DuplicationReportInfo> groupReport, Dictionary<string, string> siteUrlMapping, int groupIndex)
        {
            foreach (var item in groupReport)
            {
                try
                {
                    if (siteUrlMapping.TryGetValue(item.SiteId, out var siteUrl)) item.SiteUrl = siteUrl;
                    item.DuplicatedGroup = groupIndex;
                    item.ModifiedTime4Display = _generalSettingService.ConvertTiksToDateTime(_gls, item.ModifiedTime.Ticks, true).SimplifyFormatTime;
                    item.VersionSize = ((item.FileSize + item.HistoryVersionsSize) / 1024d).ToString("F2", CultureInfo.InvariantCulture); //KB
                    item.FileExtension = Path.GetExtension(item.Name)?.TrimStart('.') ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to assemble report. ItemId={item.ObjectId}", ex);
                    continue;
                }
                yield return item;
            }
        }

    }

    public class RMDiscoveryOffice365DuplicationReportInfo : RMDiscoveryOffice365DuplicateItemInfo
    {
        public int DuplicatedGroup { get; set; }

        public string ObjectId { get; set; }

        public string FullUrl { get; set; } //File URL

        public string SiteUrl { get; set; }

        public string FileExtension { get; set; }

        public DateTime ModifiedTime { get; set; }

        public string ModifiedTime4Display { get; set; }

        public string VersionSize { get; set; }

        public RMDiscoveryOffice365DuplicationDataAction Action { get; set; }
    }

    public enum RMDiscoveryOffice365DuplicationDataAction
    {
        Keep = 0,
        Archive = 1,
        Destroy = 2,
        Other = 3,
    }
}
