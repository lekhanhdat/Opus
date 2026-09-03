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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export;

public class RMDiscoveryOffice365ExportRowDataProcessor
{
    private static readonly Dictionary<string, string> BUILD_IN_COLUMN_NAME_MAPPINGS = new()
    {
        {"ObjectId", I18NEntity.GetString("RM_JS_JM_DiscoveryObjectId")},
        {"FolderRelativeUrl", I18NEntity.GetString("RM_JS_JM_DiscoveryFolderRelativeUrl")},
        {"SiteUrl", I18NEntity.GetString("RM_JS_JM_DiscoverySiteUrl")},
        {"FullUrl", I18NEntity.GetString("RM_JS_JM_DiscoveryFullUrl")},
        {"Name", I18NEntity.GetString("RM_JS_JM_DiscoveryName")},
        {"SPObjectType", I18NEntity.GetString("RM_JS_JM_DiscoverySPObjectType")},
        {"FileExtension", I18NEntity.GetString("RM_JS_JM_DiscoveryFileExtension")},
        {"FileSize", I18NEntity.GetString("RM_JS_JM_DiscoveryFileSize")},
        {"ModifiedTime", I18NEntity.GetString("RM_JS_JM_DiscoveryModifiedTime")},
        {"ModifiedMonth", I18NEntity.GetString("RM_JS_JM_DiscoveryModifiedMonth")},
        {"CreatedMonth", I18NEntity.GetString("RM_JS_JM_DiscoveryCreatedMonth")},
        {"CreatedTime", I18NEntity.GetString("RM_JS_JM_DiscoveryCreatedTime")},
        {"CurrentVersion", I18NEntity.GetString("RM_JS_JM_DiscoveryCurrentVersion")},
        {"HistoryVersionsSize", I18NEntity.GetString("RM_JS_JM_DiscoveryHistoryVersionsSize")},
        {"HistoryVersionsCount", I18NEntity.GetString("RM_JS_JM_DiscoveryHistoryVersionsCount")},
        {"IsPHL", I18NEntity.GetString("RM_JS_JM_DiscoveryIsPHL")},
    };

    private static readonly Dictionary<string, string> OPTIONAL_COLUMNS_NAME_MAPPINGS = new()
    {
        {"SiteId", I18NEntity.GetString("RM_JS_JM_DiscoverySiteId")},
        {"WebId", I18NEntity.GetString("RM_JS_JM_DiscoveryWebId")},
        {"ListId", I18NEntity.GetString("RM_JS_JM_DiscoveryListId")},
        {"ItemId", "Item ID"},
    };

    private readonly IRALogger _logger = new RALogger(typeof(RMDiscoveryOffice365ExportRowDataProcessor));
    
    private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();
    
    private readonly IRMDiscoveryOffice365TenantDao _o365TenantDao = new RMDiscoveryOffice365TenantDao();
    
    private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
    
    private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

    private readonly string _folderPath;

    private readonly IRMReportManager _reportManager;

    public int FileCount { get; private set; }
    
    public RMDiscoveryOffice365ExportRowDataProcessor(string folderPath, IRMReportManager reportManager)
    {
        _folderPath = folderPath;
        _reportManager = reportManager;
    }
    
    public async Task ProcessAsync()
    {
        try
        {
            using PerformanceScope scope = new("Export Row Data ");
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            
            string recordsInOneSheet =  _keyValueDao.GetValueByKey("ExportRowDataRecordsInOneSheet")?.Value;
            int.TryParse(recordsInOneSheet, out var countInOneSheet);
            
            var rules = await _ruleInfoDao.GetRuleInfoesAsyncOrderByCategory(true, RMDiscoveryRuleDefinitionKind.Inactive,
                RMDiscoveryRuleDefinitionKind.ROT);

            var columnNameMapping = BuildColumnNameMapping(rules, ShouldIncludeOptionalColumns());

            var m365Tenants = await _o365TenantDao.GetAllAsync();
            var totalSiteCount = await _nodeDao.CountDiscoverySiteAsync(m365Tenants.Select(t => t.UniqueId).ToList());
            _logger.Info($"Start to generate row data report, total site count: {totalSiteCount}");
            _reportManager.IncreaseBase(totalSiteCount);
            var currentProgress = 1;
            foreach (var m365Tenant in m365Tenants)
            {
                var siteInfos = _nodeDao.GetDiscoverySiteInfoesAsync(m365Tenant.UniqueId);
                await foreach (var siteInfo in siteInfos)
                { 
                    _logger.Info($"Querying data with site {siteInfo.Url}, id {siteInfo.SiteId}"); 
                        
                    await using var exportor = new RMDiscoveryOffice365SiteDataExportor(_folderPath, siteInfo, columnNameMapping, countInOneSheet);
                    
                    var office365OdataQuery = new RMDiscoveryOffice365OdataQuery(siteInfo, m365Tenant.UniqueId, columnNameMapping);
                    
                    await exportor.WriteHeaderAsync();
                    await foreach (var item in office365OdataQuery.QueryAsync())
                    {
                        await exportor.WriteAsync(item);
                        FileCount++;
                    }
                    exportor.CheckIfMoreThanOneSheet();
                    currentProgress += (int)((double)1 / totalSiteCount * 100);
                    _reportManager.SetProgress(currentProgress);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Error($"generate row data report error Info:{exception}");
            throw;
        }
    }

    private bool ShouldIncludeOptionalColumns()
    {
        var config = _keyValueDao.GetValueByKey("ExportRowDataIncludeOptionalColumns")?.Value;
        return bool.TryParse(config, out var include) && include;
    }

    private static Dictionary<string, string> BuildColumnNameMapping(
        IEnumerable<RMDiscoveryOffice365RuleInfo> rules,
        bool includeOptionalColumns)
    {
        var customRuleNameMapping = rules.ToDictionary(
            item => $"{item.ToTagColumn()}",
            item => $"\"{item.Name.Replace("\"", "\"\"")} (KB)\"");

        var columnNameMapping = new Dictionary<string, string>();

        if (includeOptionalColumns)
        {
            columnNameMapping.AddRange(OPTIONAL_COLUMNS_NAME_MAPPINGS, true);
        }

        columnNameMapping.AddRange(BUILD_IN_COLUMN_NAME_MAPPINGS, true);

        columnNameMapping.AddRange(customRuleNameMapping, true);

        return columnNameMapping;
    }
}