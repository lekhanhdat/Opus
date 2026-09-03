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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.RACommonUtility.Lcoker;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using Storage;
using Newtonsoft.Json;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Optimization;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    [AsyncAudit]
    public class RMDiscoveryOffice365ProgressService : IRMDiscoveryOffice365ProgressService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ProgressService));

        private readonly IRMDiscoveryOffice365ProgressDao _progressDao = new RMDiscoveryOffice365ProgeressDao();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMDiscoveryOffice365OptimizationSettingsInfoDao _optimizationSettingInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();

        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();

        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();

        private readonly IRMDiscoveryOffice365TenantConfigurationDao _o365TenantConfigurationDao = new RMDiscoveryOffice365TenantConfigurationDao();

        private readonly IRMDiscoveryOffice365ProgressDao _optimizationDao = new RMDiscoveryOffice365ProgeressDao();

        private const int RelatedSiteQueryBatchSize = 1000;

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

        public async Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressContainerOptimizedInfo>> GetContainerOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            try
            {
                var res = new RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressContainerOptimizedInfo>
                {
                    Items = await _progressDao.GetContainerOptimizedInfoesAsync(paginateInfo)
                };
                if (paginateInfo.NeedCalculateCount)
                {
                    res.Count = await _progressDao.CountContainerOptimizedAsync(paginateInfo.O365TenantId);
                }
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get container optimized infoes. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressSiteOptimizedInfo>> GetSiteOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            try
            {
                var res = new RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressSiteOptimizedInfo>
                {
                    Items = await _progressDao.GetSiteOptimizedInfoesAsync(paginateInfo)
                };

                var gls = await _generalSettingService.GetGeneralSettingAsync();
                res.Items.ForEach(item =>
                {
                    item.NextOptimizationTimeString = item.NextOptimizationTime == 0 ? "" : _generalSettingService.ConvertTiksToDateTime(gls, item.NextOptimizationTime, true).SimplifyFormatTime;
                });

                if (paginateInfo.NeedCalculateCount)
                {
                    res.Count = await _progressDao.CountSiteOptimizedAsync(paginateInfo.O365TenantId);
                }
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get container optimized infoes. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressOptimizationPlanDataInfo>> GetOptimizationPlanInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            try
            {
                var res = new RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressOptimizationPlanDataInfo>();
                var settingInfoes = await _optimizationSettingInfoDao.GetPlanSettingInfoAsync(paginateInfo);
                if (!settingInfoes.Any())
                {
                    return res;
                }

                if (paginateInfo.NeedCalculateCount)
                {
                    res.Count = await _optimizationSettingInfoDao.CountPlanSettingInfoAsync(paginateInfo.O365TenantId);
                }

                var fileExtensions = await _fileExtensionDao.GetAllAsync(paginateInfo.O365TenantId);
                var withoutInDateLists = await _withoutInDateDao.GetAllAsync();
                var sizeRanges = await _sizeRangeDao.GetAllAsync();
                var gls = await _generalSettingService.GetGeneralSettingAsync();

                foreach (var settingInfo in settingInfoes)
                {
                    var definition = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
                    var dataInfo = new RMDiscoveryProgressOptimizationPlanDataInfo
                    {
                        UniqueId = settingInfo.SettingId,
                        OptimizingTime = _generalSettingService.ConvertTiksToDateTime(gls, settingInfo.NextTime, true).SimplifyFormatTime,
                        TimeRange = GetModifiedRangeI18NStr(withoutInDateLists.FirstOrDefault(item => item.Id == definition.WithoutDateQueryParameter.From), withoutInDateLists.FirstOrDefault(item => item.Id == definition.WithoutDateQueryParameter.To)),
                        SizeRange = definition.SizeRangeQueryParameter.SizeRange == 0 || definition.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None ?
                        I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") : I18NEntity.GetString(sizeRanges.First(item => item.Id == definition.SizeRangeQueryParameter.SizeRange).DisplayName),
                    };
                    var relateSiteCount = await _optimizationSettingInfoDao.CountSettingRelateSiteAsync(paginateInfo.O365TenantId, settingInfo.SettingId);
                    dataInfo.Scope = relateSiteCount == 1 ? $"{relateSiteCount} {I18NEntity.GetString("RM_FA_Plan_Site_Collection")}" : $"{relateSiteCount} {I18NEntity.GetString("RM_FA_Plan_Site_Collections")}";
                    if (!definition.FileExtensionQueryParameter.FileExtensions.Any())
                    {
                        dataInfo.FileType = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                    }
                    else
                    {
                        var fileExtensionNames = definition.FileExtensionQueryParameter.FileExtensions.ConvertAll(item => fileExtensions.First(i => i.Id == item).Name);
                        fileExtensionNames = fileExtensionNames.Select(item => item.Equals("RM_FA_FileType_Empty") ? I18NEntity.GetString("RM_FA_FileType_Empty") : item).ToList();
                        dataInfo.FileType = string.Join("; ", fileExtensionNames);
                    }
                    res.Items.Add(dataInfo);
                }

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get optimization plan infoes. Error: {e}");
                return new();
            }
        }

        private static string GetModifiedRangeI18NStr(RMDiscoveryOffice365WithoutInDate from, RMDiscoveryOffice365WithoutInDate to)
        {
            string modifiedTimeFrom = string.Empty;
            string modifiedTimeTo = string.Empty;
            if (from == null)
            {
                modifiedTimeFrom = $"0 {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
            }
            else
            {
                if (from.UnitType == RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (from.UnitType == RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }

            if (to == null)
            {
                modifiedTimeTo = I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max");
            }
            else
            {
                if (to.UnitType == RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (to.UnitType == RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }
            return string.Format(I18NEntity.GetString("ExchangeOnline.Service_642972b7-1c4c-48e0-b94e-d968795edd09"), modifiedTimeFrom, modifiedTimeTo);
        }

        public async Task<RMDiscoveryProgressOptimizationPlanDetail> GetOptimizationSettingDetailAsync(Guid o365TenantId, Guid uniqueId)
        {
            try
            {
                var res = new RMDiscoveryProgressOptimizationPlanDetail();

                var fileExtensions = await _fileExtensionDao.GetAllAsync(o365TenantId);
                var withoutInDateLists = await _withoutInDateDao.GetAllAsync();
                var sizeRanges = await _sizeRangeDao.GetAllAsync();
                var gls = await _generalSettingService.GetGeneralSettingAsync();

                var settingInfo = await _optimizationSettingInfoDao.GetSettingInfoByIdAsync(uniqueId, o365TenantId);

                var definition = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
                var dataInfo = new RMDiscoveryProgressOptimizationPlanDataInfo
                {
                    UniqueId = settingInfo.SettingId,
                    OptimizingTime = _generalSettingService.ConvertTiksToDateTime(gls, settingInfo.NextTime, true).SimplifyFormatTime,
                    TimeRange = GetModifiedRangeI18NStr(withoutInDateLists.FirstOrDefault(item => item.Id == definition.WithoutDateQueryParameter.From), withoutInDateLists.FirstOrDefault(item => item.Id == definition.WithoutDateQueryParameter.To)),
                    SizeRange = definition.SizeRangeQueryParameter.SizeRange == 0 || definition.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None ?
                    I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") : I18NEntity.GetString(sizeRanges.First(item => item.Id == definition.SizeRangeQueryParameter.SizeRange).DisplayName),
                };
                var relateSiteCount = await _optimizationSettingInfoDao.CountSettingRelateSiteAsync(o365TenantId, settingInfo.SettingId);
                dataInfo.Scope = relateSiteCount == 1 ? $"{relateSiteCount} {I18NEntity.GetString("RM_FA_Plan_Site_Collection")}" : $"{relateSiteCount} {I18NEntity.GetString("RM_FA_Plan_Site_Collections")}";
                if (!definition.FileExtensionQueryParameter.FileExtensions.Any())
                {
                    dataInfo.FileType = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                }
                else
                {
                    var fileExtensionNames = definition.FileExtensionQueryParameter.FileExtensions.ConvertAll(item => fileExtensions.First(i => i.Id == item).Name);
                    fileExtensionNames = fileExtensionNames.Select(item => item.Equals("RM_FA_FileType_Empty") ? I18NEntity.GetString("RM_FA_FileType_Empty") : item).ToList();
                    dataInfo.FileType = string.Join("; ", fileExtensionNames);
                }
                var relatedSites = new List<string>();
                var skip = 0;
                var batchIndex = 0;
                while (true)
                {
                    var batch = await _optimizationSettingInfoDao.GetSettingRelateSitesAsync(o365TenantId, uniqueId, skip, RelatedSiteQueryBatchSize);
                    var batchCount = batch?.Count ?? 0;
                    _logger.Info($"GetOptimizationSettingDetailAsync fetched related site batch {batchIndex} with {batchCount} records (skip {skip}).");
                    if (batchCount == 0)
                    {
                        break;
                    }

                    relatedSites.AddRange(batch);
                    skip += batchCount;
                    batchIndex++;

                    if (batchCount < RelatedSiteQueryBatchSize)
                    {
                        _logger.Info($"GetOptimizationSettingDetailAsync reached final related site batch {batchIndex - 1} with {batchCount} records.");
                        break;
                    }
                }
                dataInfo.Sites = relatedSites;
                res.DataScopeInfo = dataInfo;

                res.ObjectScopeInfo = new OptimizationObjectScopeInfo
                {
                    DataType = (ArchiverDataType)definition.ArchiveDataType,
                    InactiveEnable = definition.InactiveRuleQueryParameter.Enable,
                    InactiveRules = (await DiscoverUtil.GetInactiveRuleAsync(definition.InactiveRuleQueryParameter, definition.ArchiveDataType))?.Select(item => item.Name).ToList(),
                    RotEnable = definition.ROTRuleQueryParameter.Enable,
                    ROTRules = (await DiscoverUtil.GetROTRuleAsync(definition.ROTRuleQueryParameter, definition.ArchiveDataType))?.Select(item => item.Name).ToList(),
                };

                res.ActionInfo = new OptimizationActionInfo
                {
                    FileAction = definition.ProcessActionParameter.FileAction,
                    VersionAction = (ArchiverDataType)definition.ArchiveDataType == ArchiverDataType.All ? VersionAction.None : definition.ProcessActionParameter.VersionAction,
                    IsEnableLeaveStub = definition.ProcessActionParameter.IsEnableLeaveStub,
                    DeleteRecords = definition.ProcessActionParameter.DeleteRecords,
                    DeleteRecordToRecycleBin = definition.ProcessActionParameter.DeleteRecordToRecycleBin,
                    DeleteVersionToRecycleBin = definition.ProcessActionParameter.DeleteVersionToRecycleBin
                };

                if (definition.ProcessActionParameter.EnableArchivedLatestVersion)
                {
                    res.ActionInfo.ArchivedLatestVersion = definition.ProcessActionParameter.ArchivedLatestVersion.ToString();
                }
                else if (definition.ProcessActionParameter.EnableArchivedOnlyLatestVersion)
                {
                    res.ActionInfo.ArchivedLatestVersion = definition.ProcessActionParameter.ArchivedOnlyLatestVersion.ToString();
                }
                else
                {
                    res.ActionInfo.ArchivedLatestVersion = string.Empty;
                }

                    res.ScheduleTime = dataInfo.OptimizingTime;
                res.StorageName = res.ActionInfo.FileAction == FileAction.ArchiveAndRemove || res.ActionInfo.VersionAction == VersionAction.ArchiveAndRemoveVerison ?
                    definition.SelectedStorage.Name : I18NEntity.GetString("RM_JS_Common_None");
                res.StorageDeviceUIDto = res.ActionInfo.FileAction == FileAction.ArchiveAndRemove || res.ActionInfo.VersionAction == VersionAction.ArchiveAndRemoveVerison ?
                    definition.SelectedStorage : null;
                res.MoveToAnotherTierType = definition.MoveToAnotherTierType;

                res.DataScopeInfo.MS365DataType = definition.MS365DataType == (int)MS365DataType.Phl ? MS365DataType.Phl : MS365DataType.Default;
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get optimization setting details by [{o365TenantId} - {uniqueId}]. Error: {e}");
                return new();
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
                    using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
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

        public async Task UpdateSiteOptimizedInfo(Guid o365TenantId, RMDiscoveryOffice365OptimizationSettingsInfo settingInfo)
        {
            var setting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            List<RMDiscoveryOffice365SiteInfo> siteInfos;
            if (setting.NodeQueryParameter.ContainerIds.Count != 0)
            {
                siteInfos = await _nodeDao.GetSiteInfosByContainerIds(o365TenantId, setting.NodeQueryParameter.ContainerIds);
            }
            else
            {
                var siteIds = setting.NodeQueryParameter.SiteIds;
                siteInfos = await _nodeDao.GetSiteInfosBySiteIds(o365TenantId, siteIds.ConvertAll(item => (long)item));
            }
            foreach (var siteInfo in siteInfos)
            {
                var siteOptimizedInfo = await _optimizationDao.GetSiteOptimizedInfoAsync(o365TenantId, siteInfo.Id);
                if (siteOptimizedInfo.NextOptimizationTime != settingInfo.NextTime) continue;
                var nextSettingInfo = await _optimizationSettingInfoDao.GetLatestSettingAsync(o365TenantId, siteInfo.SiteId, settingInfo.NextTime);
                if (nextSettingInfo != null)
                {
                    var calculator = new RMDiscoveryOffice365OptimizationCalculator(o365TenantId, siteInfo, nextSettingInfo);
                    await calculator.CalculateAsync();
                    continue;
                }
                var initSiteOptimizedInfo = new RMDiscoveryOffice365SiteOptimizedInfo()
                {
                    Id = siteOptimizedInfo.Id,
                    SiteId = siteOptimizedInfo.SiteId,
                    SettingId = Guid.Empty,
                    NextOptimizationTime = siteOptimizedInfo.LastOptimizedTime,
                    NextOptimizableFileTotalSize = 0L,
                    NextOptimizableVersionTotalSize = 0L,
                    Archived = siteOptimizedInfo.Archived,
                    Deleted = siteOptimizedInfo.Deleted,
                    LastOptimizedTime = siteOptimizedInfo.LastOptimizedTime
                };
                await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(o365TenantId, initSiteOptimizedInfo);
            }
        }

        public async Task<RMDiscoveryProjectionConfigurationInfo> GetProjectionConfigurationInfoAsync(Guid o365TenantId)
        {
            try
            {
                var res = await _o365TenantConfigurationDao.GetValueAsync<RMDiscoveryProjectionConfigurationInfo>(o365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration);
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
                var res = await _o365TenantConfigurationDao.GetValueAsync<RMDiscoveryProjectionConfigurationInfo>(configurationInfo.O365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration);
                res.MonthlyGrowthRate = configurationInfo.MonthlyGrowthRate;
                res.OdMonthlyGrowthRate = configurationInfo.OdMonthlyGrowthRate;
                res.DailyOptimizationSpeed = configurationInfo.DailyOptimizationSpeed;
                res.DataSizeUnitType = configurationInfo.DataSizeUnitType;
                await _o365TenantConfigurationDao.AddOrUpdateAsync(configurationInfo.O365TenantId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration, JsonConvert.SerializeObject(res));
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while update projection configuration info by tenant [{configurationInfo.O365TenantId}]. Error: {e}");
                return false;
            }
        }
    }
}
