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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Rot;
using AvePoint.RA.Service.Services.Discovery.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPDataQueryService : IRMDiscoveryAOSPDataQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPDataQueryService));

        private IRMArchiveSiteInfoDao RMArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        #region Inactive
        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPInactiveFileExtensionQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                return await cacheManager.TryGetAsync("InactiveFileExtension", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query file extensions of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPInactiveSizeRangeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                return await cacheManager.TryGetAsync("InactiveSizeRanges", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query size ranges of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryAOSPAggregateStatisticDataInfo> QueryInactiveAggregateInfo(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPInactiveAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
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
        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionDataAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPRotFileExtensionsQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                return await cacheManager.TryGetAsync("RotFileExtensionData", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot v3 file extensions ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTreeAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPRotRuleDataQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                return await cacheManager.TryGetAsync("RotRuleInfoOfTree", queryParameter, querier.QueryAsync);
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

        public async Task<RMDiscoveryAOSPAggregateStatisticDataInfo> QueryRotAggregateInfoAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPRotAggregateStatisticQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                return await cacheManager.TryGetAsync("RotAggregateInfo", queryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query rot aggregate info ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        #endregion

        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveAndRotSiteNodesAsync(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryAOSPInactiveNodeQuerier(queryParameter);
                var cacheManager = new RMDiscoveryCacheManager(queryParameter.O365TenantId, RMDiscoveryCacheDataSource.AOSPOffice365);
                var nodeInfo = await cacheManager.TryGetAsync("InactiveOptimizationNodes", queryParameter, querier.QueryAsync);
                return nodeInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query optimization nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
        public async Task<List<RMDiscoveryNodeDataSizeInfo>> QuerySiteArchiveSizeInfo(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            var siteUniqueIds = queryParameter?.NodeQueryParameter?.SiteUniqueIds == null
                ? "null"
                : string.Join(",", queryParameter.NodeQueryParameter.SiteUniqueIds);
            _logger.Info($"Start GenerateSiteArchiveSizeInfo. TenantId:{queryParameter.O365TenantId}, SiteUniqueIds:{siteUniqueIds}");
            List<RMDiscoveryNodeDataSizeInfo> result = new List<RMDiscoveryNodeDataSizeInfo>();
            try
            {
                var siteInfoes = await RMArchiveSiteInfoDao.GetAllArchiverSiteInfoByTenant(queryParameter.O365TenantId.ToString(), queryParameter.NodeQueryParameter.PageIndex, queryParameter.NodeQueryParameter.PageSize, queryParameter.NodeQueryParameter.SiteUniqueIds);
                _logger.Info($"GenerateSiteArchiveSizeInfo loaded site infos. TenantId:{queryParameter.O365TenantId}, SiteCount:{siteInfoes?.Count ?? 0}");
                foreach (var temoSiteInfo in siteInfoes)
                {
                    var archiveSizeInBytes = temoSiteInfo.ArchivedSize <= 0
                        ? 0L
                        : Convert.ToInt64(temoSiteInfo.ArchivedSize * ContractConstants.GBSizeInterval);
                    var destroySizeInBytes = temoSiteInfo.DeletedSize <= 0
                        ? 0L
                        : Convert.ToInt64(temoSiteInfo.DeletedSize * ContractConstants.GBSizeInterval);

                    result.Add(new RMDiscoveryNodeDataSizeInfo
                    {
                        SiteUrl = temoSiteInfo.SiteUrl,
                        SiteId = temoSiteInfo.SiteId,
                        ArchiveSize = archiveSizeInBytes,
                        DestroySize = destroySizeInBytes,
                    });

                    _logger.Info($"GenerateSiteArchiveSizeInfo converted size. SiteId:{temoSiteInfo.SiteId}, SiteUrl:{temoSiteInfo.SiteUrl}, ArchivedSizeGB:{temoSiteInfo.ArchivedSize}, ArchiveSizeBytes:{archiveSizeInBytes}, DeletedSizeGB:{temoSiteInfo.DeletedSize}, DestroySizeBytes:{destroySizeInBytes}");
                }
                _logger.Info($"Finish GenerateSiteArchiveSizeInfo. TenantId:{queryParameter.O365TenantId}, ResultCount:{result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"GenerateSiteArchiveSizeInfo failed. TenantId:{queryParameter.O365TenantId}, Error:{ex}");
                throw;
            }
        }
    }
}
