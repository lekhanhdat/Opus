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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Audit
{
    public class RMDiscoveryOffice365ConfigurationBeforeAuditHandler : IAsyncAuditBeforeHandler
    {
        private readonly IRMDiscoveryConfigurationDao _configInfoDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly IRMDiscoveryOffice365OptimizationSettingsInfoDao _optimizationSettingInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IRMDiscoveryOffice365ProfileDao _profileDao = new RMDiscoveryOffice365ProfileDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            if (action == AuditAction.SaveOptimizationDataPreScanSetting)
            {
                var current = args[0] as RMDiscoveryOffice365OptimizationSetting;
                await CollectOptimizationDataPreScanSavingConfig(auditInfo, current);
            }

            if (action == AuditAction.SaveDiscoveryConfiguration)
            {
                var newConfiguration = args[0] as RMDiscoveryOffice365ConfigurationInfo;
                var oldScopeInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope);
                var oldInactiveInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition);
               var oldRotInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
                await CollectScopeConfig(auditInfo, oldScopeInfo, newConfiguration.ScopeInfo);
                await CollectInactiveConfig(auditInfo, oldInactiveInfo, newConfiguration.InactiveDefinition);
                await CollectROTConfig(auditInfo, oldRotInfo, newConfiguration.RotDefinition);
            }

            if (action == AuditAction.SaveCostSavingInfo)
            {
                var exist = await _configInfoDao.ExistAsync(RMDiscoveryConfigurationType.Office365CostSaving);
                var current = args[0] as RMDiscoveryOffice365CostSavingInfo;
                await CollectCostSavingConfig(auditInfo, current, exist);
            }

            if (action == AuditAction.SaveOptimizationDataSetting)
            {
                var current = args[0] as RMDiscoveryOffice365OptimizationSetting;
                await CollectOptimizationDataSavingConfig(auditInfo, current);
            }

            if (action == AuditAction.CancelPlanOptimizableJob)
            {
                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var settingInfo = await _optimizationSettingInfoDao.GetSettingInfoByIdAsync((Guid)args[1], (Guid)args[0]);
                var optimizedTime = new AuditItem
                {
                    OldValue = _generalSettingService.ConvertTiksToDateTime(gls, settingInfo.NextTime, true).SimplifyFormatTime
                };
                auditInfo.ModifyContent.Add(optimizedTime);
            }

            if (action == AuditAction.DiscoveryAppend)
            {
                var specifyContainerIds = args[0] as List<Guid>;
                var containerInfo = await _nodeDao.GetOpusContainersAsync(specifyContainerIds);
                auditInfo.ModifyContent.Add(new()
                {
                    NewValue = string.Join(";\n ", GetContainerUrls(containerInfo))
                });
            }

            if (category == AuditCategory.InactiveData || category == AuditCategory.ROTData)
            {
                var profileInfo = args[0] as RMDiscoveryProfileDataInfo;

                await BuildInactiveOrROTAudit(category, action, profileInfo, auditInfo);

            }

            if (action == AuditAction.ExportO365Profile)
            {
                auditInfo.Module = module;
                auditInfo.Action = action;
            }

            return auditInfo;
        }

        public async Task BuildInactiveOrROTAudit(AuditCategory category, AuditAction action, RMDiscoveryProfileDataInfo profileInfo, RMAuditInfo auditInfo)
        {
            var commonUtils = new RMDiscoveryOffice365ConfigurationAuditCommentUtils();

            var (newSizeRagesOrrotRuleName, newFileTypeName, newDateRangesName) = await commonUtils.GetProfileDetailsAsync(category, profileInfo);

            if (action == AuditAction.DeleteInactiveProfileInfo || action == AuditAction.DeleteRotProfileInfo ||
                action == AuditAction.UpdateInactiveProfileInfo || action == AuditAction.UpdateRotProfileInfo)
            {
                var profileNmae = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileName", OldValue = profileInfo.Name };
                var modifiedTimeRange = new AuditItem { TargetSetting = "RM_FA_Inactive_ModifiedTitle", OldValue = newDateRangesName };
                var fileSizeorRotRule = new AuditItem { TargetSetting = "RM_JS_JMD_Summary_DataSize", OldValue = newSizeRagesOrrotRuleName };
                var fileType = new AuditItem { TargetSetting = "RM_FA_Inactive_OptimizationTab_FileCategoryTitle", OldValue = newFileTypeName };
                var sortBy = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileSortBy", OldValue = commonUtils.ProfileRenameSortBy(profileInfo.SortBy) };

                if (category == AuditCategory.ROTData)
                {
                    fileSizeorRotRule.TargetSetting = "RM_FA_ROTRule_Optimization_ROTrule";
                }

                if (action == AuditAction.UpdateInactiveProfileInfo || action == AuditAction.UpdateRotProfileInfo)
                {
                    var oldProFileDataInfo = await BuildOldProfile(action, profileInfo);
                    var (OldsizeRagesOrrotRuleNamee, OldFileTypeName, OldDateRangesName) = await commonUtils.GetProfileDetailsAsync(category, oldProFileDataInfo);

                    profileNmae.OldValue = oldProFileDataInfo.Name;
                    modifiedTimeRange.OldValue = OldDateRangesName;
                    fileSizeorRotRule.OldValue = OldsizeRagesOrrotRuleNamee;
                    fileType.OldValue = OldFileTypeName;
                    sortBy.OldValue = commonUtils.ProfileRenameSortBy(oldProFileDataInfo.SortBy);
                }

                auditInfo.ModifyContent.Add(profileNmae);
                auditInfo.ModifyContent.Add(modifiedTimeRange);
                auditInfo.ModifyContent.Add(fileSizeorRotRule);
                auditInfo.ModifyContent.Add(fileType);
                auditInfo.ModifyContent.Add(sortBy);
            }
        }

        public async Task<RMDiscoveryProfileDataInfo> BuildOldProfile(AuditAction action, RMDiscoveryProfileDataInfo profileInfo)
        {
            List<RMDiscoveryOffice365ProfileInfo> profileInfoes;
            if (action == AuditAction.UpdateInactiveProfileInfo)
            {
                profileInfoes = await _profileDao.GetProfileInfoesAsync(profileInfo.O365TenantId, RMDiscoveryProfileType.Inactive);
            }
            else
            {
                profileInfoes = await _profileDao.GetProfileInfoesAsync(profileInfo.O365TenantId, RMDiscoveryProfileType.ROT);
            }
            var oldProFileInfo = profileInfoes.Find(item => item.Id == profileInfo.Id);

            return new RMDiscoveryProfileDataInfo
            {
                Name = I18NEntity.GetString(oldProFileInfo.Name),
                SizeRange = oldProFileInfo.SizeRange,
                GreaterThanEqualWithoutInDate = oldProFileInfo.GreaterThanEqualWithoutInDate,
                LessThanEqualWithoutInDate = oldProFileInfo.LessThanEqualWithoutInDate,
                FileExtensionIds = JsonConvert.DeserializeObject<List<int>>(oldProFileInfo.FileExtensionIdsJson),
                RuleIds = JsonConvert.DeserializeObject<List<int>>(oldProFileInfo.RuleIdsJson),
                SortBy = oldProFileInfo.SortBy,
                O365TenantId = profileInfo.O365TenantId,
            };
        }

        public async Task CollectScopeConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365ScopeInfo oldScopeInfo, RMDiscoveryOffice365ScopeInfo scopeInfo)
        {
            var scopeAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ScopeType",
                OldValue = oldScopeInfo.ScopeType.ToString(),
                NewValue = scopeInfo.ScopeType.ToString()
            };

            auditInfo.ModifyContent.Add(scopeAudit);

            if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource ||
                scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                oldScopeInfo = oldScopeInfo.CompatibleConvert();
                var oldDataSource = oldScopeInfo.ContentSources.ConvertAll(item =>
                    I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[item]));
                var newDataSource = scopeInfo.ContentSources.ConvertAll(item =>
                    I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[item]));
                var dataSourceAudit = new AuditItem
                {
                    TargetSetting = "RM_FA_Discovery_JobPage_Scope_DataSource",
                    OldValue = string.Join(";\n ", oldDataSource),
                    NewValue = string.Join(";\n ", newDataSource),
                };
                auditInfo.ModifyContent.Add(dataSourceAudit);
            }

            if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify ||
                scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeSpecify",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
                {
                    var oldContainerInfo = await _nodeDao.GetOpusContainersAsync(oldScopeInfo.SpecifyContainerIds);
                    IdAudit.OldValue = string.Join(";\n ", GetContainerUrls(oldContainerInfo));
                }

                if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
                {
                    var containerInfo = await _nodeDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
                    IdAudit.NewValue = string.Join(";\n ", GetContainerUrls(containerInfo));
                }

                auditInfo.ModifyContent.Add(IdAudit);
            }
        }

        public async Task CollectInactiveConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365InactiveDefinition oldInactiveDefinitionInfo, RMDiscoveryOffice365InactiveDefinition inactiveDefinitionInfo)
        {
            var inactiveConfigAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_InactiveEnable",
                NewValue = inactiveDefinitionInfo.Enable.ToString(),
                OldValue = oldInactiveDefinitionInfo.Enable.ToString()
            };
            var inactiveRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_InactiveRule",
            };

            if (oldInactiveDefinitionInfo.Enable)
            {
                var oldInactiveRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.Inactive);
                inactiveRuleAudit.OldValue = string.Join(";\n ", oldInactiveRuleInfo.Where(rule => rule.IsEnable).Select(r => r.Name));
            }

            if (inactiveDefinitionInfo.Enable)
            {
                inactiveRuleAudit.NewValue = string.Join(";\n ", inactiveDefinitionInfo.Rules.Where(rule => rule.IsEnable).Select(r => r.Name));
            }

            auditInfo.ModifyContent.Add(inactiveConfigAudit);
            auditInfo.ModifyContent.Add(inactiveRuleAudit);
        }

        public async Task CollectROTConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365RotDefinition oldRotDefinitionInfo, RMDiscoveryOffice365RotDefinition rotDefinitionInfo)
        {
            var rotConfigAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_RotEnable"
            };
            var rotRedundantRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_RedundantRule",
            };
            var rotObsoleteRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ObsoleteRule",
            };
            var rotTrivialRuleAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_TrivialRule",
            };
            var oldRotRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);
            rotConfigAudit.NewValue = rotDefinitionInfo.Enable.ToString();
            rotConfigAudit.OldValue = oldRotDefinitionInfo.Enable.ToString();

            if (oldRotDefinitionInfo.Enable)
            {
                rotRedundantRuleAudit.OldValue = string.Join(";\n ",
                    oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Redundant && rule.IsEnable)
                        .Select(rule => rule.Name));
                rotObsoleteRuleAudit.OldValue = string.Join(";\n ",
                    oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Obsolete && rule.IsEnable)
                        .Select(rule => rule.Name));
                rotTrivialRuleAudit.OldValue = string.Join(";\n ",
                    oldRotRuleInfo.Where(rule => rule.Category == RMDiscoveryRuleCategory.Trivial && rule.IsEnable)
                        .Select(rule => rule.Name));
            }

            if (rotDefinitionInfo.Enable)
            {
                rotRedundantRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.RedundantRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
                rotObsoleteRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.ObsoleteRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
                rotTrivialRuleAudit.NewValue = string.Join(";\n ",
                    rotDefinitionInfo.TrivialRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
            }

            auditInfo.ModifyContent.Add(rotConfigAudit);
            auditInfo.ModifyContent.Add(rotRedundantRuleAudit);
            auditInfo.ModifyContent.Add(rotObsoleteRuleAudit);
            auditInfo.ModifyContent.Add(rotTrivialRuleAudit);
        }

        private async Task CollectCostSavingConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365CostSavingInfo current, bool exist)
        {
            var spFreeStorageAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_SPFreeStorage",
            };
            var spFreeStoragePrice = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_SPFreeStoragePrice",
            };
            var archivedDataStoragePrice = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ArchivedDataStoragePrice",
            };
            var odFreeStorageAudit = new AuditItem
            {
                TargetSetting = "RM_FA_CostSaving_TotalStorageTitle_Od"
            };
            var odFreeStoragePriceAudit = new AuditItem
            {
                TargetSetting = "RM_FA_CostSaving_StoragePriceTitle_Od",
            };

            if (exist)
            {
                var costSavingConfig = await _configInfoDao.GetAsync<RMDiscoveryOffice365CostSavingInfo>(RMDiscoveryConfigurationType.Office365CostSaving);
                spFreeStorageAudit.OldValue = costSavingConfig.SPFreeStorage.ToString();
                spFreeStoragePrice.OldValue = costSavingConfig.SPStoragePrice.ToString();
                odFreeStorageAudit.OldValue = costSavingConfig.ODFreeStorage.ToString();
                odFreeStoragePriceAudit.OldValue = costSavingConfig.ODStoragePrice.ToString();
                archivedDataStoragePrice.OldValue = costSavingConfig.ArchivedDataStoragePrice.ToString();
            }

            spFreeStorageAudit.NewValue = current.SPFreeStorage.ToString();
            spFreeStoragePrice.NewValue = current.SPStoragePrice.ToString();
            odFreeStorageAudit.NewValue = current.ODFreeStorage.ToString();
            odFreeStoragePriceAudit.NewValue = current.ODStoragePrice.ToString();
            archivedDataStoragePrice.NewValue = current.ArchivedDataStoragePrice.ToString();

            auditInfo.ModifyContent.Add(spFreeStorageAudit);
            auditInfo.ModifyContent.Add(spFreeStoragePrice);
            auditInfo.ModifyContent.Add(odFreeStorageAudit);
            auditInfo.ModifyContent.Add(odFreeStoragePriceAudit);
            auditInfo.ModifyContent.Add(archivedDataStoragePrice);

        }

        private async Task CollectOptimizationDataSavingConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365OptimizationSetting current)
        {
            var ms365DataType = new AuditItem
            {
                TargetSetting = "RM_FA_DataOptimize_DSOMS365DataFilterTypeTitle",
            };
            var modifiedTimeRange = new AuditItem
            {
                TargetSetting = "RM_FA_Inactive_ModifiedTitle",
            };
            var dataSize = new AuditItem
            {
                TargetSetting = "RM_JS_JMD_Summary_DataSize",
            };
            var fileType = new AuditItem
            {
                TargetSetting = "RM_FA_Inactive_OptimizationTab_FileCategoryTitle",
            };
            var rule = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_Rules",
            };
            var actionOnFiles = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_DocumentAction",
            };
            var actionOnVersions = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_DocumentVersionAction",
            };

            var schedule = new AuditItem
            {
                TargetSetting = "RM_CP_TimerJob",
            };
            var storageLocation = new AuditItem
            {
                TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName"
            };
            var targetTier = new AuditItem
            {
                TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle",
            };
            var container = new AuditItem
            {
                TargetSetting = "RM_CP_AM_Permission_ContentSource",
            };
            var siteCollection = new AuditItem
            {
                TargetSetting = "RM_JS_RC_ActionAudit_ObjType_SiteCollection",
            };

            DataOptimizationSettingsForJobHistory discoveryJobSettings = await ConvertSettingToJobHistorySettingsAsync(current);

            ms365DataType.NewValue = discoveryJobSettings?.ScopeSettings?.MS365DataType == MS365DataType.Phl ? "RM_FA_DataOptimize_PreservationHoldLibraryTitle" : "RM_FA_DataOptimize_SharepointOrOneDriveTitle";
            modifiedTimeRange.NewValue = GetModifiedRangeI18NStr(discoveryJobSettings);
            dataSize.NewValue = GetSizeRangeI18NStr(discoveryJobSettings);
            fileType.NewValue = discoveryJobSettings?.ScopeSettings?.MS365DataType == MS365DataType.Phl ? "RM_RC_ActionAudit_ViewDetail_All" : discoveryJobSettings?.ScopeSettings?.FileCatagorysStr;
            targetTier.NewValue = current.MoveToAnotherTierType switch
            {
                0 => "RM_RDM_CreateRule_DefaultTier",
                3 => "RM_RDM_CreateRule_ArchivedTier",
                4 => "RM_RDM_CreateRule_ColdTier",
                _ => "RM_RDM_CreateRule_DefaultTier"
            };//0 default,3 archive,4 cold
            rule.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DefinitionsStr;
            actionOnFiles.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DocumentActionStr;
            actionOnVersions.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DocumentVersionActionStr;
            storageLocation.NewValue = current.SelectedStorage.Name;
            if (current.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container && current.NodeQueryParameter.ContainerIds != null)
            {
                var containerNames = await _nodeDao.GetContainerNamesByIds(new Guid(current.O365TenantId), current.NodeQueryParameter.ContainerIds);
                container.NewValue = string.Join("; ", containerNames);
            }
            else if (current.NodeQueryParameter.SiteIds != null)
            {
                var siteUrls = await _nodeDao.GetSiteUrlBySiteIds(new Guid(current.O365TenantId), current.NodeQueryParameter.SiteIds);
                siteCollection.NewValue = string.Join("; ", siteUrls);
            }

            var timeSetting = await _generalSettingService.GetTimeSettingModelAsync(TenantLocalValue.LogonGroupId);
            schedule.NewValue = DateTime.MinValue == current.ScheduleParameter.StartTime ? "RM_DAM_RunNow" : await _generalSettingService.ConvertFromUTCDateTimeAsync(current.ScheduleParameter.StartTime.ToString()) + " " + DateTimeUtil.GetSimplifyZoneInfo(timeSetting.TimeZoneId);
            auditInfo.ModifyContent.Add(ms365DataType);
            auditInfo.ModifyContent.Add(modifiedTimeRange);
            auditInfo.ModifyContent.Add(dataSize);
            auditInfo.ModifyContent.Add(fileType);
            auditInfo.ModifyContent.Add(rule);
            auditInfo.ModifyContent.Add(actionOnFiles);
            auditInfo.ModifyContent.Add(actionOnVersions);
            auditInfo.ModifyContent.Add(schedule);
            auditInfo.ModifyContent.Add(container);
            auditInfo.ModifyContent.Add(siteCollection);
            auditInfo.ModifyContent.Add(storageLocation);
            auditInfo.ModifyContent.Add(targetTier);
            var indexDevice = _storageDeviceService.GetIndexDevice();
            if (indexDevice == null)
            {
                auditInfo.Status = (int)AuditStatus.Failed;
            }
        }

        private async Task CollectOptimizationDataPreScanSavingConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365OptimizationSetting current)
        {
            var ms365DataType = new AuditItem
            {
                TargetSetting = "RM_FA_DataOptimize_DSOMS365DataFilterTypeTitle",
            };
            var modifiedTimeRange = new AuditItem
            {
                TargetSetting = "RM_FA_Inactive_ModifiedTitle",
            };
            var dataSize = new AuditItem
            {
                TargetSetting = "RM_JS_JMD_Summary_DataSize",
            };
            var fileType = new AuditItem
            {
                TargetSetting = "RM_FA_Inactive_OptimizationTab_FileCategoryTitle",
            };
            var rule = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_Rules",
            };
            var actionOnFiles = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_DocumentAction",
            };
            var actionOnVersions = new AuditItem
            {
                TargetSetting = "RM_JM_DOSummary_Column_DocumentVersionAction",
            };

            DataOptimizationSettingsForJobHistory discoveryJobSettings = await ConvertSettingToJobHistorySettingsAsync(current);

            modifiedTimeRange.NewValue = GetModifiedRangeI18NStr(discoveryJobSettings);
            dataSize.NewValue = GetSizeRangeI18NStr(discoveryJobSettings);
            fileType.NewValue = discoveryJobSettings?.ScopeSettings?.MS365DataType == MS365DataType.Phl ? "RM_RC_ActionAudit_ViewDetail_All" : discoveryJobSettings?.ScopeSettings?.FileCatagorysStr;
            ms365DataType.NewValue = discoveryJobSettings?.ScopeSettings?.MS365DataType == MS365DataType.Phl ? "RM_FA_DataOptimize_PreservationHoldLibraryTitle" : "RM_FA_DataOptimize_SharepointOrOneDriveTitle";

            rule.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DefinitionsStr;
            actionOnFiles.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DocumentActionStr;
            actionOnVersions.NewValue = discoveryJobSettings?.DefinitionAndActionSettings?.DocumentVersionActionStr;
            var timeSetting = await _generalSettingService.GetTimeSettingModelAsync(TenantLocalValue.LogonGroupId);
            auditInfo.ModifyContent.Add(ms365DataType);
            auditInfo.ModifyContent.Add(modifiedTimeRange);
            auditInfo.ModifyContent.Add(dataSize);
            auditInfo.ModifyContent.Add(fileType);
            auditInfo.ModifyContent.Add(rule);
            auditInfo.ModifyContent.Add(actionOnFiles);
            auditInfo.ModifyContent.Add(actionOnVersions);
            
            var indexDevice = _storageDeviceService.GetIndexDevice();
            if (indexDevice == null)
            {
                auditInfo.Status = (int)AuditStatus.Failed;
            }
        }

        private string GetModifiedRangeI18NStr(DataOptimizationSettingsForJobHistory settingsHistory)
        {
            string modifiedTimeFrom = string.Empty;
            string modifiedTimeTo = string.Empty;
            if (settingsHistory.ScopeSettings.WithoutDateQueryParameter.From <= -1)
            {
                modifiedTimeFrom = $"0 {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
            }
            else
            {
                var from = settingsHistory.ScopeSettings.WithoutInDateDataInfos.FirstOrDefault(i => i.Id == settingsHistory.ScopeSettings.WithoutDateQueryParameter.From);
                if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }

            if (settingsHistory.ScopeSettings.WithoutDateQueryParameter.To >= 999)
            {
                modifiedTimeTo = I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max");
            }
            else
            {
                var to = settingsHistory.ScopeSettings.WithoutInDateDataInfos.FirstOrDefault(i => i.Id == settingsHistory.ScopeSettings.WithoutDateQueryParameter.To);
                if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }
            return string.Format(I18NEntity.GetString("ExchangeOnline.Service_642972b7-1c4c-48e0-b94e-d968795edd09"), modifiedTimeFrom, modifiedTimeTo);
        }

        private string GetSizeRangeI18NStr(DataOptimizationSettingsForJobHistory settingsHistory)
        {
            if (settingsHistory.ScopeSettings.SizeRangeQueryParameter.SizeRange == 0 || settingsHistory.ScopeSettings.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None)
            {
                return settingsHistory.ScopeSettings.SizeRangeStr = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
            }
            else
            {
                return settingsHistory.ScopeSettings.SizeRangeStr;
            }
        }

        private async Task<DataOptimizationSettingsForJobHistory> ConvertSettingToJobHistorySettingsAsync(RMDiscoveryOffice365OptimizationSetting setting)
        {

            IRMDiscoveryOffice365OptimizationSettingsInfoDao optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
            IRMDiscoveryOffice365BasicInfoQueryService _basicInfoQueryService = new RMDiscoveryOffice365BaiscInfoQueryService();
            RMDiscoveryOffice365OptimizationSetting currentNodeSetting = setting;

            var fileExtensionsTask = _basicInfoQueryService.GetFileExtensionsAsync(new Guid(setting.O365TenantId));
            var withoutInDateListTask = _basicInfoQueryService.GetWithoutInDateListAsync();
            var sizeRangeListTask = _basicInfoQueryService.GetSizeRangeListAsync();

            DataOptimizationSettingsForJobHistory settingsHistory = new DataOptimizationSettingsForJobHistory();

            settingsHistory.ScopeSettings.MS365DataType = (MS365DataType)setting.MS365DataType;

            #region fileExtensions
            var fileExtensions = await fileExtensionsTask;
            PackageFileExtensionsToSettingsHistory(settingsHistory, currentNodeSetting, fileExtensions);
            #endregion

            #region withoutInDateList
            var withoutInDateList = await withoutInDateListTask;
            PackageWithoutInDateListToSettingsHistory(settingsHistory, currentNodeSetting, withoutInDateList);
            #endregion

            #region sizeRangeList
            var sizeRangeList = await sizeRangeListTask;
            PackageSizeRangeListToSettingsHistory(settingsHistory, currentNodeSetting, sizeRangeList);
            #endregion

            #region RuleList
            var rules = new List<RMDiscoveryOffice365RuleInfo>();

            if (currentNodeSetting.ArchiveDataType == (int)ArchiverDataType.Special)
            {
                var inactiveRuleTask = DiscoverUtil.GetInactiveRuleAsync(currentNodeSetting.InactiveRuleQueryParameter, currentNodeSetting.ArchiveDataType);
                var rotRuleTask = DiscoverUtil.GetROTRuleAsync(currentNodeSetting.ROTRuleQueryParameter, currentNodeSetting.ArchiveDataType);
                var inactiveRule = await inactiveRuleTask;
                var rotRule = await rotRuleTask;
                if (inactiveRule != null && inactiveRule.Count > 0)
                {
                    rules.AddRange(inactiveRule);
                }
                if (rotRule != null && rotRule.Count > 0)
                {
                    rules.AddRange(rotRule);
                }
            }
            PackageRuleListToSettingsHistory(settingsHistory, currentNodeSetting, rules);
            PackageActionToSettingsHistory(settingsHistory, currentNodeSetting);
            #endregion

            return settingsHistory;
        }

        private void PackageFileExtensionsToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryFileExtensionDataInfo> fileExtensions)
        {
            if (currentNodeSetting.FileExtensionQueryParameter.FileExtensions != null && currentNodeSetting.FileExtensionQueryParameter.FileExtensions.Count == 0)
            {
                //all
                settingsHistory.ScopeSettings.FileExtensionDataInfos = fileExtensions;
            }
            else
            {
                settingsHistory.ScopeSettings.FileExtensionDataInfos = fileExtensions.Where(i => currentNodeSetting.FileExtensionQueryParameter.FileExtensions.Contains(i.Id)).ToList();
            }
            settingsHistory.ScopeSettings.FileCatagorysStr = ParseListToFormatString(settingsHistory.ScopeSettings.FileExtensionDataInfos.ConvertAll(f => f.Name));
        }

        private void PackageWithoutInDateListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryWithoutInDateDataInfo> withoutInDateList)
        {
            string modifiedTimeFrom = string.Empty;
            string modifiedTimeTo = string.Empty;
            if (currentNodeSetting.WithoutDateQueryParameter.From <= -1)
            {
                modifiedTimeFrom = $"0 {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
            }
            else
            {
                var from = withoutInDateList.FirstOrDefault(i => i.Id == currentNodeSetting.WithoutDateQueryParameter.From);
                if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }

            if (currentNodeSetting.WithoutDateQueryParameter.To >= 999)
            {
                modifiedTimeTo = I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max");
            }
            else
            {
                var to = withoutInDateList.FirstOrDefault(i => i.Id == currentNodeSetting.WithoutDateQueryParameter.To);
                if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }
            settingsHistory.ScopeSettings.ModifiedTimeRangeStr = string.Format(I18NEntity.GetString("ExchangeOnline.Service_642972b7-1c4c-48e0-b94e-d968795edd09"), modifiedTimeFrom, modifiedTimeTo);
            settingsHistory.ScopeSettings.WithoutDateQueryParameter = currentNodeSetting.WithoutDateQueryParameter;
            settingsHistory.ScopeSettings.WithoutInDateDataInfos = withoutInDateList;
        }

        private void PackageSizeRangeListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoverySizeRangeDataInfo> sizeRangeList)
        {
            if (currentNodeSetting.SizeRangeQueryParameter.SizeRange == 0 || currentNodeSetting.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None)
            {
                settingsHistory.ScopeSettings.SizeRangeStr = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
            }
            else
            {
                settingsHistory.ScopeSettings.SizeRangeDataInfos = sizeRangeList.FirstOrDefault(i => i.Id == currentNodeSetting.SizeRangeQueryParameter.SizeRange);
                settingsHistory.ScopeSettings.SizeRangeStr = settingsHistory.ScopeSettings.SizeRangeDataInfos.Name;
            }
            settingsHistory.ScopeSettings.SizeRangeQueryParameter = currentNodeSetting.SizeRangeQueryParameter;
        }

        private void PackageRuleListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryOffice365RuleInfo> ruleList)
        {
            if (ruleList.Count == 0)
            {
                settingsHistory.DefinitionAndActionSettings.DefinitionsStr = I18NEntity.GetString("RM_FA_DataOptimize_Archive_All");
            }
            else
            {
                settingsHistory.DefinitionAndActionSettings.DefinitionsStr = ParseListToFormatString(ruleList.ConvertAll(r => r.Name));
            }

        }

        private void PackageActionToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, RMDiscoveryOffice365OptimizationSetting currentNodeSetting)
        {
            settingsHistory.DefinitionAndActionSettings.ProcessActionParameter = currentNodeSetting.ProcessActionParameter;
            settingsHistory.DefinitionAndActionSettings.ArchiveDataType = currentNodeSetting.ArchiveDataType;
            settingsHistory.DefinitionAndActionSettings.ROTRuleQueryParameter = currentNodeSetting.ROTRuleQueryParameter;
            settingsHistory.DefinitionAndActionSettings.InactiveRuleQueryParameter = currentNodeSetting.InactiveRuleQueryParameter;
            bool addFileAction = false;
            bool addFileVersionAction = false;
            if (currentNodeSetting.ArchiveDataType == (int)ArchiverDataType.Special)
            {
                if (currentNodeSetting.ROTRuleQueryParameter.Enable)
                {
                    addFileAction = true;
                    addFileVersionAction = true;
                }
                else if (currentNodeSetting.InactiveRuleQueryParameter.Enable)
                {
                    addFileVersionAction = true;
                }
            }
            else
            {
                addFileAction = true;
            }

            if (addFileAction)
            {
                if (currentNodeSetting.ProcessActionParameter.FileAction == FileAction.ArchiveAndRemove)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = I18NEntity.GetString("RM_FA_DataOptimize_File_ArchiveAndRemove");
                    if (currentNodeSetting.ProcessActionParameter.IsEnableLeaveStub)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "\r\n";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += I18NEntity.GetString("RM_FA_DataOptimize_File_LeaveStub");
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += " ";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += currentNodeSetting.ProcessActionParameter.StubSettingDto.Name;
                    }
                }
                else if (currentNodeSetting.ProcessActionParameter.FileAction == Contract.Discovery.Model.Configuration.Office365.FileAction.Archive)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Backup");
                    if (settingsHistory.DefinitionAndActionSettings.ProcessActionParameter!=null && settingsHistory.DefinitionAndActionSettings.ProcessActionParameter.EnableArchivedOnlyLatestVersion)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; " + I18NEntity.GetString("RM_JS_Rule_ArchiveVersionAndDestroyFile");
                    }
                }
                else
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = I18NEntity.GetString("RM_FA_DataOptimize_File_RemoveFile");
                }
                if (currentNodeSetting.ProcessActionParameter.DeleteRecords)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; " + I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile");
                }
                if (currentNodeSetting.ProcessActionParameter.DeleteRecordToRecycleBin)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; " + I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteToRecycleBin");
                }
            }
            if (addFileVersionAction)
            {
                if (currentNodeSetting.ProcessActionParameter.VersionAction == VersionAction.ArchiveAndRemoveVerison)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentVersionActionStr = I18NEntity.GetString("RM_FA_DataOptimize_Version_ArchiveAndRemove");
                }
                else
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentVersionActionStr = I18NEntity.GetString("RM_FA_DataOptimize_Version_RemoveVersion");
                    if (currentNodeSetting.ProcessActionParameter.DeleteVersionToRecycleBin)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentVersionActionStr += "; " + I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteToRecycleBin");
                    }
                }
            }
        }

        private string ParseListToFormatString(List<string> list)
        {
            if (list.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    str.Append($"{list[i]}");
                }
                else
                {
                    str.Append($"{list[i]}, ");
                }
            }
            return str.ToString();
        }

        private List<string> GetContainerUrls(List<RMRemoteNode> contianerInfo)
        {
            return contianerInfo.ConvertAll(c =>
            {
                if (c.Url.Equals("Default_ SharePoint Sites_ Group"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                }
                else if (c.Url.Equals("Default Office 365 Group Sites Group"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                }
                else if (c.Url.Equals("Default Private Channel Sites Container"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                }

                return c;
            }).Select(c => c.Url).ToList();
        }
    }
}
