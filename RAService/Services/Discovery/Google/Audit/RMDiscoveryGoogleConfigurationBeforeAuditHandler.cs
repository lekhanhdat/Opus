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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Audit
{
    public class RMDiscoveryGoogleConfigurationBeforeAuditHandler : IAsyncAuditBeforeHandler
    {
        private readonly IRMDiscoveryConfigurationDao _configInfoDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

        private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        private static readonly IRMTenantDiscoveryDBInfoDao s_tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        private readonly IRMDiscoveryGoogleProfileDao _profileDao = new RMDiscoveryGoogleProfileDao();

        private bool IsInitTenantDiscoveryDB => s_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult();

        private bool IsInitDiscoveryGoogleDB => RMDiscoveryDBManager.CheckGoogleTablesExistsAsync().GetAwaiter().GetResult();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            if (action == AuditAction.SaveDiscoveryConfiguration)
            {
                var newConfiguration = args[0] as RMDiscoveryGoogleConfigurationInfo;
                if(!IsInitTenantDiscoveryDB || !IsInitDiscoveryGoogleDB)
                {
                    await CollectScopeConfig(auditInfo, new() { ScopeType = RMDiscoveryGoogleScopeType.None}, newConfiguration.ScopeInfo);
                    await CollectROTConfig(auditInfo, new(), newConfiguration.RotDefinition);
                }
                else
                {
                    var oldScopeInfo = await _configInfoDao.GetAsync<RMDiscoveryGoogleScopeInfo>(RMDiscoveryConfigurationType.GoogleNewlyScope);
                    var oldRotInfo = await _configInfoDao.GetAsync<RMDiscoveryGoogleRotDefinition>(RMDiscoveryConfigurationType.GoogleROTDefinition);
                    await CollectScopeConfig(auditInfo, oldScopeInfo, newConfiguration.ScopeInfo);
                    await CollectROTConfig(auditInfo, oldRotInfo, newConfiguration.RotDefinition);
                }
            }

            if (category == AuditCategory.InactiveData || category == AuditCategory.ROTData)
            {
                var profileInfo = args[0] as RMDiscoveryGoogleProfileDataInfo;

                await BuildInactiveOrROTAudit(category, action, profileInfo, auditInfo);

            }

            return auditInfo;
        }
        public async Task BuildInactiveOrROTAudit(AuditCategory category, AuditAction action, RMDiscoveryGoogleProfileDataInfo profileInfo, RMAuditInfo auditInfo)
        {
            var commonUtils = new RMDiscoveryGoogleConfigurationAuditCommentUtils();

            var (newSizeRagesOrrotRuleName, newFileTypeName, newDateRangesName) = await commonUtils.GetProfileDetailsAsync(category, profileInfo);

            if (action == AuditAction.DeleteInactiveProfileInfo || action == AuditAction.DeleteRotProfileInfo ||
                action == AuditAction.UpdateInactiveProfileInfo || action == AuditAction.UpdateRotProfileInfo)
            {
                var profileName = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileName", OldValue = profileInfo.Name };
                var modifiedTimeRange = new AuditItem { TargetSetting = "RM_FA_Inactive_ModifiedTitle", OldValue = newDateRangesName };
                var fileSizeOrRotRule = new AuditItem { TargetSetting = "RM_JS_JMD_Summary_DataSize", OldValue = newSizeRagesOrrotRuleName };
                var fileType = new AuditItem { TargetSetting = "RM_FA_Inactive_OptimizationTab_FileCategoryTitle", OldValue = newFileTypeName };
                var sortBy = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileSortBy", OldValue = commonUtils.ProfileRenameSortBy(profileInfo.SortBy) };

                if (category == AuditCategory.ROTData)
                {
                    fileSizeOrRotRule.TargetSetting = "RM_FA_ROTRule_Optimization_ROTrule";
                }

                if (action == AuditAction.UpdateInactiveProfileInfo || action == AuditAction.UpdateRotProfileInfo)
                {
                    var oldProfileDataInfo = await BuildOldProfile(action, profileInfo);
                    var (oldSizeRangesOrRotRuleName, oldFileTypeName, oldDateRangesName) = await commonUtils.GetProfileDetailsAsync(category, oldProfileDataInfo);

                    profileName.OldValue = oldProfileDataInfo.Name;
                    modifiedTimeRange.OldValue = oldDateRangesName;
                    fileSizeOrRotRule.OldValue = oldSizeRangesOrRotRuleName;
                    fileType.OldValue = oldFileTypeName;
                    sortBy.OldValue = commonUtils.ProfileRenameSortBy(oldProfileDataInfo.SortBy);
                }

                auditInfo.ModifyContent.Add(profileName);
                auditInfo.ModifyContent.Add(modifiedTimeRange);
                auditInfo.ModifyContent.Add(fileSizeOrRotRule);
                auditInfo.ModifyContent.Add(fileType);
                auditInfo.ModifyContent.Add(sortBy);
            }
        }
        public async Task<RMDiscoveryGoogleProfileDataInfo> BuildOldProfile(AuditAction action, RMDiscoveryGoogleProfileDataInfo profileInfo)
        {
            List<RMDiscoveryGoogleProfileInfo> profileInfoes;
            if (action == AuditAction.UpdateInactiveProfileInfo)
            {
                profileInfoes = await _profileDao.GetProfileInfoesAsync(profileInfo.OrganizationId, RMDiscoveryProfileType.Inactive);
            }
            else
            {
                profileInfoes = await _profileDao.GetProfileInfoesAsync(profileInfo.OrganizationId, RMDiscoveryProfileType.ROT);
            }
            var oldProFileInfo = profileInfoes.Find(item => item.Id == profileInfo.Id);

            return new RMDiscoveryGoogleProfileDataInfo
            {
                Name = I18NEntity.GetString(oldProFileInfo.Name),
                SizeRange = oldProFileInfo.SizeRange,
                GreaterThanEqualWithoutInDate = oldProFileInfo.GreaterThanEqualWithoutInDate,
                LessThanEqualWithoutInDate = oldProFileInfo.LessThanEqualWithoutInDate,
                FileExtensionIds = JsonConvert.DeserializeObject<List<int>>(oldProFileInfo.FileExtensionIdsJson),
                RuleIds = JsonConvert.DeserializeObject<List<int>>(oldProFileInfo.RuleIdsJson),
                SortBy = oldProFileInfo.SortBy,
                OrganizationId = profileInfo.OrganizationId,
            };
        }
        public async Task CollectScopeConfig(RMAuditInfo auditInfo, RMDiscoveryGoogleScopeInfo oldScopeInfo, RMDiscoveryGoogleScopeInfo scopeInfo)
        {
            var scopeAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ScopeType",
                OldValue = oldScopeInfo.ScopeType.ToString(),
                NewValue = scopeInfo.ScopeType.ToString()
            };
            auditInfo.ModifyContent.Add(scopeAudit);

            if (oldScopeInfo.ScopeType == RMDiscoveryGoogleScopeType.All ||
                scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.All)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeAll",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryGoogleScopeType.All)
                {
                    var oldContainerInfo = await _nodeDao.GetOpusGoogleContainersAsync();
                    IdAudit.OldValue = string.Join(";\n ", GetContainerUrls(oldContainerInfo));
                }

                if (scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.All)
                {
                    var containerInfo = await _nodeDao.GetOpusGoogleContainersAsync();
                    IdAudit.NewValue = string.Join(";\n ", GetContainerUrls(containerInfo));
                }
                auditInfo.ModifyContent.Add(IdAudit);
            }

            if (oldScopeInfo.ScopeType == RMDiscoveryGoogleScopeType.Specify ||
                scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.Specify)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeSpecify",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryGoogleScopeType.Specify)
                {
                    var oldContainerInfo = await _nodeDao.GetOpusGoogleContainersAsync(oldScopeInfo.SpecifyContainerIds);
                    IdAudit.OldValue = string.Join(";\n ", GetContainerUrls(oldContainerInfo));
                }

                if (scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.Specify)
                {
                    var containerInfo = await _nodeDao.GetOpusGoogleContainersAsync(scopeInfo.SpecifyContainerIds);
                    IdAudit.NewValue = string.Join(";\n ", GetContainerUrls(containerInfo));
                }
                auditInfo.ModifyContent.Add(IdAudit);
            }
            
        }

        public async Task CollectROTConfig(RMAuditInfo auditInfo, RMDiscoveryGoogleRotDefinition oldRotDefinitionInfo, RMDiscoveryGoogleRotDefinition rotDefinitionInfo)
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

            rotConfigAudit.NewValue = rotDefinitionInfo.Enable.ToString();
           
            if (IsInitTenantDiscoveryDB && IsInitDiscoveryGoogleDB)
            {
                var oldRotRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);
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

        private List<string> GetContainerUrls(List<RMRemoteNode> container)
        {
            return container.ConvertAll(c =>
            {
                if (c.Url.Equals("Default_ Google_ SharedDrive_ Group"))
                {
                    c.Url = I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
                }
                else if (c.Url.Equals("Default_ GoogleUser_ Group"))
                {
                    c.Url = I18NEntity.GetString("RM_GoogleUser_Default_Container");
                }
                return c;
            }).Select(c => c.Url).ToList();
        }
    }
}
