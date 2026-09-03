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
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Rot;

namespace AvePoint.RA.Service.Services.Discovery.Google
{
    public class RMDiscoveryGoogleDataQueryService : IRMDiscoveryGoogleDataQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDataQueryService));

        //private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        #region Inactive
        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveFileExtensionQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("InactiveFileExtension", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query file extensions of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveSizeRangeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("InactiveSizeRanges", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query size ranges of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodesAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("InactiveSummaryNodes", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveNodeTotalAggregateQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("InactiveSummaryNodeTotalAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryInactiveAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
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

        public async Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodeDataAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotNodeDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("RotV3SummaryNodeData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot v3 file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionDataAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotFileExtensionsQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("RotV3FileExtensionData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot v3 file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoDataAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotNodeTotalAggregateDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("RotV3SummaryNodeTotalAggregateInfoData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query summary nodes total aggregate info of rot v3 ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTreeAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotRuleDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
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

        public async Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryRotAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.OrganizationId, RMDiscoveryCacheDataSource.Google);
                return await cacheManager.TryGetAsync("RotV3AggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot aggregate info ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        #endregion
    }
}
