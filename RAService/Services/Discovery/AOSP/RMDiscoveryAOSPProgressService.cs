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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Optimization;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPProgressService : IRMDiscoveryAOSPProgressService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPProgressService));

        private readonly IRMDiscoveryAOSPProgressDao _progressDao = new RMDiscoveryAOSPProgressDao();

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao = new RMDiscoveryAOSPNodeDao();

        private readonly IRMDiscoveryAOSPTenantConfigurationDao _aospTenantConfigurationDao = new RMDiscoveryAOSPTenantConfigurationDao();

        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _optimizationSettingInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();

        private readonly IRMDiscoveryAOSPSiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryAOSPSiteOptimizationMappingTableDao();

        public async Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfoAsync(Guid o365TenantId)
        {
            try
            {
                return await _progressDao.GetSummaryOptimizedInfoAsync(o365TenantId);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get summary optimized info. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryProjectionConfigurationInfo> GetProjectionConfigurationInfoAsync(Guid o365TenantId)
        {
            try
            {
                var res = await _aospTenantConfigurationDao.GetValueAsync<RMDiscoveryProjectionConfigurationInfo>(o365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration);
                res.O365TenantId = o365TenantId;
                res.OldestYear = new DateTime(res.OldestDateTimeTicks).Year;
                res.OldestMonth = new DateTime(res.OldestDateTimeTicks).Month;
                res.LatestYear = new DateTime(res.LatestDateTimeTicks).Year;
                res.LatestMonth = new DateTime(res.LatestDateTimeTicks).Month;
                res.OdOldestYear = new DateTime(res.OdOldestDateTimeTicks).Year;
                res.OdOldestMonth = new DateTime(res.OdOldestDateTimeTicks).Month;
                res.OdLatestYear = new DateTime(res.OdLatestDateTimeTicks).Year;
                res.OdLatestMonth = new DateTime(res.OdLatestDateTimeTicks).Month;
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get projection configuration info by o365 tenant [{o365TenantId}]. Error: {e}");
                return new();
            }
        }

        public async Task<bool> UpdateProjectionConfigurationInfoAsync(RMDiscoveryProjectionConfigurationInfo configurationInfo)
        {
            try
            {
                if (configurationInfo.DataSizeUnitType != RMDiscoveryProjectionDataSizeUnitType.GB &&
                    configurationInfo.DataSizeUnitType != RMDiscoveryProjectionDataSizeUnitType.TB)
                {
                    return false;
                }
                var res = await _aospTenantConfigurationDao.GetValueAsync<RMDiscoveryProjectionConfigurationInfo>(configurationInfo.O365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration);
                res.MonthlyGrowthRate = configurationInfo.MonthlyGrowthRate;
                res.OdMonthlyGrowthRate = configurationInfo.OdMonthlyGrowthRate;
                res.DailyOptimizationSpeed = configurationInfo.DailyOptimizationSpeed;
                res.DataSizeUnitType = configurationInfo.DataSizeUnitType;
                await _aospTenantConfigurationDao.AddOrUpdateAsync(configurationInfo.O365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration, JsonConvert.SerializeObject(res));
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while update projection configuration info by tenant [{configurationInfo.O365TenantId}]. Error: {e}");
                return false;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.CancelPlanOptimizableJob, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<bool> GetCancelJobAsync(Guid o365TenantId, Guid settingId)
        {
            var settingInfo = await _optimizationSettingInfoDao.GetSettingInfoByIdAsync(settingId, o365TenantId);
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationJobCancel, o365TenantId.ToString(), TimeSpan.FromMinutes(10)))
                {
                    using var context = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
                    using var transaction = context.Database.BeginTransaction();
                    try
                    {
                        var count = await _optimizationSettingInfoDao.removePlanSettingInfoAsync(context, settingInfo.SettingId);
                        if (count == 0)
                        {
                            transaction.Commit();
                            return false;
                        }
                        await _siteOptimizationMappingTableDao.removeMappingInfoAsync(context, settingInfo.SettingId);
                        transaction.Commit();
                        try
                        {
                            await UpdateSiteOptimizedInfo(o365TenantId, settingInfo);
                            return true;
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"An error occurred while cancel job to update site optimized info. Error: {e}");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"An error occurred while cancel job to remove data. Error: {ex}");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while cancel job. Error: {e}");
                return false;
            }
        }

        public async Task UpdateSiteOptimizedInfo(Guid o365TenantId, RMDiscoveryAOSPOptimizationSettingsInfo settingInfo)
        {
            var setting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryAOSPOptimizationSetting>(RMDiscoveryAOSPOptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            List<RMDiscoveryAOSPSiteInfo> siteInfos;
            var siteIds = setting.NodeQueryParameter.SiteIds;
            siteInfos = await _nodeDao.GetSiteInfosBySiteIds(o365TenantId, siteIds.ConvertAll(item => (long)item));
            foreach (var siteInfo in siteInfos)
            {
                var siteOptimizedInfo = await _progressDao.GetSiteOptimizedInfoAsync(o365TenantId, siteInfo.Id);
                if (siteOptimizedInfo.NextOptimizationTime != settingInfo.NextTime) continue;
                var nextSettingInfo = await _optimizationSettingInfoDao.GetLatestSettingAsync(o365TenantId, siteInfo.SiteId, settingInfo.NextTime);
                if (nextSettingInfo != null)
                {
                    var calculator = new RMDiscoveryAOSPOptimizationCalculator(o365TenantId, siteInfo, nextSettingInfo);
                    await calculator.CalculateAsync();
                    continue;
                }
                var initSiteOptimizedInfo = new RMDiscoveryAOSPSiteOptimizedInfo()
                {
                    Id = siteOptimizedInfo.Id,
                    SiteId = siteOptimizedInfo.SiteId,
                    SettingId = Guid.Empty,
                    NextOptimizationTime = siteOptimizedInfo.LastOptimizedTime,
                    NextOptimizableFileTotalSize = 0L,
                    NextOptimizableVersionTotalSize = 0L,
                    Archived = siteOptimizedInfo.Archived,
                    Deleted = siteOptimizedInfo.Deleted,
                    LastOptimizedTime = siteOptimizedInfo.LastOptimizedTime,
                    ContentSource = siteInfo.ContentSource,
                };
                await _progressDao.AddOrUpdateSiteOptimizedInfoAsync(o365TenantId, initSiteOptimizedInfo);
            }
        }
    }
}
