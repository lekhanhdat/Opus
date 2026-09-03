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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Converter;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive.V3;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot.V3;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    public class RMDiscoveryOffice365DataQueryService : IRMDiscoveryOffice365DataQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DataQueryService));

        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleDao = new RMDiscoveryOffice365RuleInfoDao();

        private readonly IRMDiscoveryOffice365TenantDao _tenantDao = new RMDiscoveryOffice365TenantDao();

        private readonly IRMDiscoveryOffice365JobDao _jobDao = new RMDiscoveryOffice365JobDao();

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        #region Inactive
        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveFileExtensionQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveFileExtension", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query file extensions of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveSizeRangeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveSizeRanges", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query size ranges of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveSummaryNodes", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var nodeInfo = await cacheManager.TryGetAsync("InactiveOptimizationNodes", queryParameter, querier.QueryAsync);

                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    using var scope = new PerformanceScope("Inactive.GetInScopeSiteCount");
                    foreach (var container in nodeInfo.Items)
                    {
                        var inscopeSite = await _siteOptimizationMappingTableDao.GetInScopeSiteCount(queryParameter.O365TenantId, Convert.ToInt32(container["id"]));
                        container["inScope"] = inscopeSite + "/" + container["siteCount"];
                        container.Remove("siteCount");
                    }
                }
                else
                {
                    var allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(queryParameter.O365TenantId, nodeInfo.Items.Select(item => Convert.ToInt64(item["id"])));
                    foreach (var site in nodeInfo.Items)
                    {
                        if (allInScopeSiteIds.Contains(Convert.ToInt64(site["id"])))
                        {
                            site["inScope"] = I18NEntity.GetString("RM_JS_Common_Yes");
                        }
                        else
                        {
                            site["inScope"] = I18NEntity.GetString("RM_JS_Common_No");
                        }
                    }
                }

                return nodeInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query optimization nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveSummaryNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var res = await cacheManager.TryGetAsync("InactiveOptimizationNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var inScopeCount = await _siteOptimizationMappingTableDao.CountAsync(queryParameter.O365TenantId);
                    res["inScope"] = inScopeCount;
                }
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query optimization nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryInactiveAggregateInfo(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion

        #region ROT
        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionsAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotFileExtensionQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotFileExtensions", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotSummaryNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotSummaryNodes", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot summary nodes ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryRotOptmizationNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotOptmizationNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var nodeInfo = await cacheManager.TryGetAsync("RotOptimizationNodes", queryParameter, querier.QueryAsync);
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    using var scope = new PerformanceScope("Rot.GetInScopeSiteCount");
                    foreach (var container in nodeInfo.Items)
                    {
                        var inscopeSite = await _siteOptimizationMappingTableDao.GetInScopeSiteCount(queryParameter.O365TenantId, Convert.ToInt32(container["id"]));
                        container["inScope"] = inscopeSite + "/" + container["siteCount"];
                    }
                }
                else
                {
                    var allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(queryParameter.O365TenantId, nodeInfo.Items.Select(item => Convert.ToInt64(item["id"])));
                    foreach (var site in nodeInfo.Items)
                    {
                        if (allInScopeSiteIds.Contains(Convert.ToInt64(site["id"])))
                        {
                            site["inScope"] = I18NEntity.GetString("RM_JS_Common_Yes");
                        }
                        else
                        {
                            site["inScope"] = I18NEntity.GetString("RM_JS_Common_No");
                        }
                    }
                }

                return nodeInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot optmization nodes ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotSummaryNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotSummaryNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotOptimizationNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var res = await cacheManager.TryGetAsync("RotOptimizationNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var inScopeCount = await _siteOptimizationMappingTableDao.CountAsync(queryParameter.O365TenantId);
                    res["inScope"] = inScopeCount;
                }

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query optimization nodes total aggregate info of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> QueryTreeRotRuleInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotRuleQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var res = await cacheManager.TryGetAsync("TreeRotRuleInfo", queryParameter, querier.QueryAsync);
                var allRule = await _ruleDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                foreach (var data in allRule)
                {
                    if (!res.Any(item => item.Id == data.Id))
                    {
                        var item = new RMDiscoveryRotRuleDataInfo()
                        {
                            Id = data.Id,
                            Label = data.Name,
                            FileTotalSize = 0,
                            Category = data.Category
                        };
                        res.Add(item);
                    }
                }
                return RMDiscoveryRuleTreeConverter.ConvertToTreeItem(res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot rules ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryRotAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot aggregate info ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion

        public async Task<RMDiscoveryNodeDataInfo> QueryRotV3SummaryNodeDataAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotV3NodeDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotV3SummaryNodeData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot v3 file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotV3FileExtensionDataAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotV3FileExtensionsQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotV3FileExtensionData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot v3 file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotV3SummaryNodeTotalAggregateInfoDataAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotV3NodeTotalAggregateDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotV3SummaryNodeTotalAggregateInfoData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of rot v3 ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> QueryRotV3RuleInfoOfTreeAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotV3RuleDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotV3RuleInfoOfTree", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary rule info of tree of rot v3 ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_RootNode"),
                    FileTotalSize = 0L,
                    Expand = true
                };
            }
        }

        public async Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryRotV3AggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotV3AggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("RotV3AggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot aggregate info ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotCategoryDataInfo> QueryRotV3CategoryDataAsync(Guid o365TenantId)
        {
            var res = new RMDiscoveryRotCategoryDataInfo();
            try
            {
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll);
                if (!isAdmin)
                {
                    return res;
                }

                res.HasLicense = _tenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, Contract.RoleAssignments.PaidForProduct.OpusDiscovery);
                if (!res.HasLicense)
                {
                    _logger.Info($"Current o365 tenant [{o365TenantId}] do not have discovery license.");
                    return res;
                }
            }
            catch(Exception le)
            {
                _logger.Error($"An error occurred while query o365 tenant [{o365TenantId}] rot V3 category license. Error: {le}");
                return res;
            }

            try
            {
                res.HasRunDiscovery = await _tenantDao.HasAsync(o365TenantId);
                if (!res.HasRunDiscovery)
                {
                    _logger.Info($"Current o365 tenant [{o365TenantId}] have not run discovery job.");
                    return res;
                }

                var (has, mainJobInfo) = await _jobDao.TryGetLatestMainJobAsync();
                var isNewVersion = mainJobInfo.Version.IsOffice365NewVersion();
                if (!isNewVersion)
                {
                    var ruleInfoes = await _ruleDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                    var ruleCategoryIdMapping = ruleInfoes.GroupBy(rule => rule.Category).ToDictionary(item => item.Key, item => item.Select(a => a.Id).ToList());
                    var dataDao = new RMDiscoveryOffice365DataDao();
                    foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                    {
                        var dataList = await dataDao.GetBasicRotDataListAsync(o365TenantId, contentSource);
                        foreach (var data in dataList)
                        {
                            if (ruleCategoryIdMapping.TryGetValue(RMDiscoveryRuleCategory.Redundant, out var redundantRules) && redundantRules.Contains(data.Rule))
                            {
                                res.Redundant += data.FileTotalSize;
                            }
                            else if (ruleCategoryIdMapping.TryGetValue(RMDiscoveryRuleCategory.Obsolete, out var obsoleteRules) && obsoleteRules.Contains(data.Rule))
                            {
                                res.Obsolete += data.FileTotalSize;
                            }
                            else
                            {
                                res.Trivial += data.FileTotalSize;
                            }
                        }
                    }
                }
                else
                {
                    var dataV3Dao = new RMDiscoveryOffice365DataV3Dao();
                    foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                    {
                        var dataList = await dataV3Dao.GetBasicCategoryLevelRotDataListAsync(o365TenantId, contentSource);
                        foreach (var data in dataList)
                        {
                            if (data.Category == RMDiscoveryRuleCategory.Redundant)
                            {
                                res.Redundant += data.FileTotalSize;
                            }
                            else if (data.Category == RMDiscoveryRuleCategory.Obsolete)
                            {
                                res.Obsolete += data.FileTotalSize;
                            }
                            else
                            {
                                res.Trivial += data.FileTotalSize;
                            }
                        }
                    }
                }
                
                res.Redundant = (long)Math.Ceiling((res.Redundant + 0.0) / 1024 / 1024 / 1024);
                res.Obsolete = (long)Math.Ceiling((res.Obsolete + 0.0) / 1024 / 1024 / 1024);
                res.Trivial = (long)Math.Ceiling((res.Trivial + 0.0) / 1024 / 1024 / 1024);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query o365 tenant [{o365TenantId}] rot V3 category data. Error: {e}");
            }
            return res;
        }

        #region V3
        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveV3SummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveV3NodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                return await cacheManager.TryGetAsync("InactiveSummaryNodes", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query v4 summary nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveV3SummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                var res = await cacheManager.TryGetAsync("InactiveSummaryNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);

                //res[DiscoveryConstants.PHL_TOTAL_SIZE_NAME] = await _siteOptimizationMappingTableDao.CountPHLDataTotalSizeV3(queryParameter.O365TenantId);
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer
                    && queryParameter.NodeQueryParameter.JoinedContainerId > 0)
                {
                    res[DiscoveryConstants.PHL_TOTAL_SIZE_NAME] = await _siteOptimizationMappingTableDao.GetPHLDataTotalSizeV3ByContainerId(queryParameter.O365TenantId, queryParameter.NodeQueryParameter.JoinedContainerId);
                }
                else
                {
                    res[DiscoveryConstants.PHL_TOTAL_SIZE_NAME] = await _siteOptimizationMappingTableDao.CountPHLDataTotalSizeV3(queryParameter.O365TenantId);
                }
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion
    }
}
