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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Audit
{
    public class RMDiscoveryOffice365ConfigurationAfterAuditHandler : IAsyncAuditAfterHandler
    {
        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        private readonly IRMDiscoveryConfigurationDao _configInfoDao = new RMDiscoveryConfigurationDao();

        private static readonly IRMDiscoveryOffice365NodeDao s_nodeDao = new RMDiscoveryOffice365NodeDao();

        private static readonly IRMDiscoveryOffice365ProfileDao _profileDao = new RMDiscoveryOffice365ProfileDao();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args, object returnValue)
        {
            if (action == AuditAction.SaveDiscoveryConfiguration)
            {
                var oldScopeInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope);
                var oldInactiveInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition);
                var oldRotInfo = await _configInfoDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
                var oldInactiveRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.Inactive);
                var oldRotRuleInfo = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);
                await CollectScopeConfig(auditInfo, oldScopeInfo);
                CollectInactiveConfig(auditInfo, oldInactiveInfo, oldInactiveRuleInfo);
                CollectROTConfig(auditInfo, oldRotInfo, oldRotRuleInfo);

                var actionResult = returnValue as RAReturnMessage;
                auditInfo.Status = (int)(actionResult.MessageType == RAMessageType.Successful ? AuditStatus.Successful : AuditStatus.Failed);
            }

            if (action == AuditAction.CancelPlanOptimizableJob)
            {
                var isCancel = (bool)returnValue;
                auditInfo.Status = isCancel ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }

            if (category == AuditCategory.InactiveData || category == AuditCategory.ROTData)
            {
                var actionResult = returnValue as RAReturnMessage;

                var profileInfo = args[0] as RMDiscoveryProfileDataInfo;

                auditInfo.Status = (int)(actionResult.MessageType == RAMessageType.Successful ? AuditStatus.Successful : AuditStatus.Failed);

                auditInfo.Object = profileInfo.Name;

                await BuildInactiveOrROTAudit(category, action, profileInfo, auditInfo);
            }
            if (action == AuditAction.ExportO365Profile)
            {
                var actionResult = returnValue as RAReturnMessage;
                var dataAnalysis = args[0] as DiscoveryO365DataAnalysis;
                var profileInfo = await _profileDao.GetProfileInfoByIdAsync(new Guid(dataAnalysis.TenantId), new Guid(dataAnalysis.ProfileId)); 
                auditInfo.Object = profileInfo.Name;
                auditInfo.Status = (int)(actionResult.MessageType == RAMessageType.Successful ? AuditStatus.Successful : AuditStatus.Failed);
            }

            return auditInfo;
        }


        public async Task BuildInactiveOrROTAudit(AuditCategory category, AuditAction action, RMDiscoveryProfileDataInfo profileInfo, RMAuditInfo auditInfo)
        {
            var commonUtils = new RMDiscoveryOffice365ConfigurationAuditCommentUtils();

            var (newSizeRagesOrrotRuleName, newFileTypeName, newDateRangesName) = await commonUtils.GetProfileDetailsAsync(category, profileInfo);

            switch (action)
            {
                case AuditAction.AddInactiveProfileInfo:
                case AuditAction.AddRotProfileInfo:
                    var profileNmae = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileName", NewValue = profileInfo.Name };
                    var modifiedTimeRange = new AuditItem { TargetSetting = "RM_FA_Inactive_ModifiedTitle", NewValue = newDateRangesName };
                    var fileSizeorRotRule = new AuditItem { TargetSetting = "RM_JS_JMD_Summary_DataSize", NewValue = newSizeRagesOrrotRuleName };
                    var fileType = new AuditItem { TargetSetting = "RM_FA_Inactive_OptimizationTab_FileCategoryTitle", NewValue = newFileTypeName };
                    var sortBy = new AuditItem { TargetSetting = "RM_DA_Profile_ProfileSortBy", NewValue = commonUtils.ProfileRenameSortBy(profileInfo.SortBy) };

                    if (category == AuditCategory.ROTData)
                    {
                        fileSizeorRotRule.TargetSetting = "RM_FA_ROTRule_Optimization_ROTrule";
                    }
                    auditInfo.ModifyContent.Add(profileNmae);
                    auditInfo.ModifyContent.Add(modifiedTimeRange);
                    auditInfo.ModifyContent.Add(fileSizeorRotRule);
                    auditInfo.ModifyContent.Add(fileType);
                    auditInfo.ModifyContent.Add(sortBy);
                    break;
                case AuditAction.UpdateInactiveProfileInfo:
                case AuditAction.UpdateRotProfileInfo:
                    auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_DA_Profile_ProfileName")).NewValue = profileInfo.Name;
                    auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_FA_Inactive_ModifiedTitle")).NewValue = newDateRangesName;
                    auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_JS_JMD_Summary_DataSize") || content.TargetSetting.Equals("RM_FA_ROTRule_Optimization_ROTrule")).NewValue = newSizeRagesOrrotRuleName;
                    auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_FA_Inactive_OptimizationTab_FileCategoryTitle")).NewValue = newFileTypeName;
                    auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_DA_Profile_ProfileSortBy")).NewValue = commonUtils.ProfileRenameSortBy(profileInfo.SortBy);
                    break;
            }
        }
        public static async Task CollectScopeConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365ScopeInfo beforeScopeInfo)
        {
            var containerInfo = await s_nodeDao.GetOpusContainersAsync(beforeScopeInfo.SpecifyContainerIds);
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_ScopeType")).OldValue = beforeScopeInfo.ScopeType.ToString();
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_ScopeSpecify")).OldValue = string.Join(";\n ", containerInfo.Select(c => c.Url));
        }

        public static void CollectInactiveConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365InactiveDefinition beforeinactiveDefinition, List<RMDiscoveryOffice365RuleInfo> beforeInactiveRules)
        {
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_InactiveEnable")).OldValue = beforeinactiveDefinition.Enable.ToString();
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_InactiveRule")).OldValue = string.Join(";\n", beforeInactiveRules.Where(rule => rule.IsEnable).Select(rule => rule.Name));
        }

        public static void CollectROTConfig(RMAuditInfo auditInfo, RMDiscoveryOffice365RotDefinition beforeRotDefinition, List<RMDiscoveryOffice365RuleInfo> beforeRotRules)
        {
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_RotEnable")).OldValue = beforeRotDefinition.Enable.ToString();
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_RedundantRule")).OldValue = string.Join(";\n ", beforeRotRules.Where(rule => rule.Category == RMDiscoveryRuleCategory.Redundant && rule.IsEnable).Select(rule => rule.Name));
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_ObsoleteRule")).OldValue = string.Join(";\n ", beforeRotRules.Where(rule => rule.Category == RMDiscoveryRuleCategory.Obsolete && rule.IsEnable).Select(rule => rule.Name));
            auditInfo.ModifyContent.First(content => content.TargetSetting.Equals("RM_RC_Audit_Discovery_TrivialRule")).OldValue = string.Join(";\n ", beforeRotRules.Where(rule => rule.Category == RMDiscoveryRuleCategory.Trivial && rule.IsEnable).Select(rule => rule.Name));
        }
    }
}
