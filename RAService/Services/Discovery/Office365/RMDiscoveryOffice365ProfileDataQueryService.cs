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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Inactive;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Rot;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Inactive.V3;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    public class RMDiscoveryOffice365ProfileDataQueryService : IRMDiscoveryOffice365ProfileDataQueryService
    {
        private readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ProfileDataQueryService));

        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();

        #region Inactive

        #region V3
        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveV3OptimizationNodesAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveV3ProfileNodeDataQuerier(queryParameter);
                var dataInfo = await querier.QueryAsync();

                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    using var scope = new PerformanceScope("Profile.Inactive.GetInScopeSiteCount");
                    foreach (var container in dataInfo.Items)
                    {
                        var inscopeSite = await _siteOptimizationMappingTableDao.GetInScopeSiteCount(queryParameter.O365TenantId, Convert.ToInt32(container["id"]));
                        container["inScope"] = inscopeSite + "/" + container["siteCount"];
                        container.Remove("siteCount");
                    }
                }
                else
                {
                    var allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(queryParameter.O365TenantId, dataInfo.Items.Select(item => Convert.ToInt64(item["id"])));
                    foreach (var site in dataInfo.Items)
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

                return dataInfo;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile optimization nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveV3OptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveV3ProfileNodeTotalAggregateDataQuerier(queryParameter);
                var res = await querier.QueryAsync();
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var inScopeCount = await _siteOptimizationMappingTableDao.CountAsync(queryParameter.O365TenantId);
                    res["inScope"] = inScopeCount;
                }
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
                s_logger.Error($"An error occurred while query profile optimization nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion

        #region v1 - v3
        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveProfileNodeDataQuerier(queryParameter);
                var dataInfo = await querier.QueryAsync();

                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    using var scope = new PerformanceScope("Profile.Inactive.GetInScopeSiteCount");
                    foreach (var container in dataInfo.Items)
                    {
                        var inscopeSite = await _siteOptimizationMappingTableDao.GetInScopeSiteCount(queryParameter.O365TenantId, Convert.ToInt32(container["id"]));
                        container["inScope"] = inscopeSite + "/" + container["siteCount"];
                        container.Remove("siteCount");
                    }
                }
                else
                {
                    var allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(queryParameter.O365TenantId, dataInfo.Items.Select(item => Convert.ToInt64(item["id"])));
                    foreach (var site in dataInfo.Items)
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

                return dataInfo;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile optimization nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveProfileNodeTotalAggregateDataQuerier(queryParameter);
                var res = await querier.QueryAsync();
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var inScopeCount = await _siteOptimizationMappingTableDao.CountAsync(queryParameter.O365TenantId);
                    res["inScope"] = inScopeCount;
                }
                return res;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile optimization nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveAggregateInfo(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryInactiveProfileAggregateStatisticDataQuerier(queryParameter);
                return await querier.QueryAsync();
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion
        #endregion

        #region Rot

        public async Task<RMDiscoveryNodeDataInfo> QueryRotOptimizationNodesAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotProfileNodeDataQuerier(queryParameter);
                var dataInfo = await querier.QueryAsync();

                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    using var scope = new PerformanceScope("Profile.Rot.GetInScopeSiteCount");
                    foreach (var container in dataInfo.Items)
                    {
                        var inscopeSite = await _siteOptimizationMappingTableDao.GetInScopeSiteCount(queryParameter.O365TenantId, Convert.ToInt32(container["id"]));
                        container["inScope"] = inscopeSite + "/" + container["siteCount"];
                        container.Remove("siteCount");
                    }
                }
                else
                {
                    var allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(queryParameter.O365TenantId, dataInfo.Items.Select(item => Convert.ToInt64(item["id"])));
                    foreach (var site in dataInfo.Items)
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

                return dataInfo;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile optimization nodes of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryRotProfileNodeTotalAggregateDataQuerier(queryParameter);
                var res = await querier.QueryAsync();
                if (queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var inScopeCount = await _siteOptimizationMappingTableDao.CountAsync(queryParameter.O365TenantId);
                    res["inScope"] = inScopeCount;
                }
                return res;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query profile optimization nodes total aggregate info of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        #endregion
    }
}
