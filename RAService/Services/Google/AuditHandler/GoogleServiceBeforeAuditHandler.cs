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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Google.AuditHandler;
public class GoogleServiceBeforeAuditHandler : IBeforeAuditHandler
{
    private RALogger logger = RALogger.GetInstance(typeof(GoogleServiceBeforeAuditHandler));

    public IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
    public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
    {
        var info = new RMAuditInfo();
        try
        {
            info = new RMAuditInfo
            {
                ModifyContent = new List<AuditItem>(),
                Action = (AuditAction)action,
                Category = (AuditCategory)category,
                Module = (AuditModule)model
            };

            switch ((AuditAction)action)
            {
                case AuditAction.SaveGeneralSetting:
                    await HandleAuditGoogleGeneralSetting(info, args);
                    break;
                case AuditAction.SaveLabelSetting:
                    await HandleAuditLabelSetting(info, args);
                    break;
                case AuditAction.EditInheritSettingGoogle:
                    HandletInheritSettingGoogle(info, args);
                    break;
            }
        }
        catch (Exception e)
        {
            logger.Warn("Google setting before Audit handler,message detail {0}", e.ToString());
        }

        return info;
    }

    private async Task HandleAuditGoogleGeneralSetting(RMAuditInfo info, object[] args)
    {
        RMGoogleTreeNode node = (RMGoogleTreeNode)args[0];
        info.Object = node.DisplayName;
        var containerId = string.IsNullOrEmpty(node.ContainerId) ? Guid.Empty : new Guid(node.ContainerId);
        var driveId = string.IsNullOrEmpty(node.DriveId) ? Guid.Empty : new Guid(node.DriveId);
        var dbSetting = await GoogleSettingDao.GetSettingInfo(containerId, driveId);
        if (dbSetting != null)
        {
            var oldEnableRecordManagement = dbSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
            var newEnableRecordManagement = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
            info.ModifyContent.Add(new AuditItem()
            {
                TargetSetting = "RM_SPS_GS_ManagedScope",
                OldValue = YesOrNoString(oldEnableRecordManagement),
                NewValue = YesOrNoString(newEnableRecordManagement),
            });
            if (oldEnableRecordManagement && newEnableRecordManagement)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                    OldValue = YesOrNoString(dbSetting.IsSyncData),
                    NewValue = YesOrNoString(node.IsSyncData)
                });
            }
            else if (oldEnableRecordManagement)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                    OldValue = YesOrNoString(dbSetting.IsSyncData)
                });
            }
            else if (newEnableRecordManagement)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                    NewValue = YesOrNoString(node.IsSyncData)
                });
            }
        }
        else
        {
            info.ModifyContent.Add(new AuditItem()
            {
                TargetSetting = "RM_SPS_GS_ManagedScope",
                NewValue = YesOrNoString(node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable),
            });
            if (node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_RC_Audit_Option_EnableIsSync",
                    NewValue = YesOrNoString(node.IsSyncData)
                });
            }
        }
    }
    private string YesOrNoString(bool boolValue)
    {
        return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
    }
    private async Task HandleAuditLabelSetting(RMAuditInfo info, object[] args)
    {
        RMGoogleTreeNode node = (RMGoogleTreeNode)args[0];
        logger.Info($"AuditLabelSetting. ContainerId: {node.ContainerId}, DriveId: {node.DriveId}");
        info.Object = node.DisplayName;
        var newEnableLabelSettings = !node.IsNullClassificationSetting;
        var containerId = string.IsNullOrEmpty(node.ContainerId) ? Guid.Empty : new Guid(node.ContainerId);
        var driveId = string.IsNullOrEmpty(node.DriveId) ? Guid.Empty : new Guid(node.DriveId);
        var dbSetting = await GoogleSettingDao.GetSettingInfo(containerId, driveId);

        if (dbSetting != null)
        {
            var oldEnableTermSettings = !dbSetting.IsNullClassificationSetting;
            AuditHelper.SaveAuditItem(info, "RM_JS_SPS_EnableApplyTermSettingsTitle", YesOrNoString(oldEnableTermSettings), YesOrNoString(newEnableLabelSettings));
            if (node.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer or (int) NodeLevel.GoogleMyDrive or (int) NodeLevel.GoogleSharedDrive )
            {
                if (!oldEnableTermSettings)
                {
                    var rules = await GoogleSettingDao.GetGoogleDriveMappingRules(dbSetting.ScopeId);
                    var oldRuleNames = String.Join("; ", rules?.Select(o => o.RuleName) ?? []);
                    AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_RuleNames_Title", oldRuleNames);
                }
                if (!newEnableLabelSettings)
                {
                    var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName) ?? []);
                    AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);
                }

            }
            
            if (oldEnableTermSettings)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                    OldValue = ContentRepositoryAuditUtil.GetApplyLabelMethodString((DeployLabelMethod) dbSetting.DeployLabelMethod),
                });
                if ((DeployLabelMethod)dbSetting.DeployLabelMethod == DeployLabelMethod.UseAutoClassification)
                {
                    var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                    AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesLabelCretiaString(oldAutoRules));
                    AuditHelper.SaveOldAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption));
                    AuditHelper.SaveOldAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(dbSetting.RunAutoFullJob));
                }
                if ((DeployLabelMethod)dbSetting.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification)
                {
                    AuditHelper.SaveOldAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption));
                    AuditHelper.SaveOldAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(dbSetting.RunAutoFullJob));
                }
            }

            if (newEnableLabelSettings)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                    NewValue = ContentRepositoryAuditUtil.GetApplyLabelMethodString(node.DeployLabelMethod)
                });
                if (node.DeployLabelMethod == DeployLabelMethod.UseAutoClassification)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.Empty,
                        TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                        NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                    });
                    AuditHelper.SaveNewAuditItem(info, "RM_SPS_AutoClassification_SkipOverrideOption", ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption));
                    AuditHelper.SaveNewAuditItem(info, "RM_SPS_Auto_RunFullJob", YesOrNoString(node.RunAutoFullJob));
                }
                if ((DeployLabelMethod)node.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification)
                {
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                }
            }
            if ((DeployLabelMethod)node.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
            {
                var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                if (node.AIApprovalType != (int)ApprovalType.None)
                {
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                }
            }


        }
        else
        {
            AuditHelper.SaveAuditItem(info, "RM_JS_SPS_EnableApplyTermSettingsTitle", YesOrNoString(node.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer ? false : true), YesOrNoString(newEnableLabelSettings));
            if (node.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer or (int)NodeLevel.GoogleMyDrive or (int)NodeLevel.GoogleSharedDrive)
            {
                if (!newEnableLabelSettings)
                {
                    var newRuleNames = String.Join("; ", node.Rules?.Select(o => o.RuleName));
                    AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);
                }
            }

            if (!newEnableLabelSettings)
            {
                return;
            }

            info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });

            info.ModifyContent.Add(new AuditItem()
            {
                Id = Guid.NewGuid(),
                TargetSetting = "RM_JS_SPS_AutoClassification_DeployLabelMethod",
                NewValue = ContentRepositoryAuditUtil.GetApplyLabelMethodString(node.DeployLabelMethod)
            });
            if ((DeployLabelMethod)node.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification || node.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
            {
                var aiReviewersNewValue = node.AIReviewers.Count > 0 ? string.Join(";", node.AIReviewers.Select(a => a.DisplayName)) : string.Empty;
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_MachineLearning_IntelligenceReviewers", NewValue = aiReviewersNewValue });
                if (node.AIApprovalType != (int)ApprovalType.None)
                {
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_MA_IsSendEmail", NewValue = YesOrNoString(node.AISendEMail) });
                }
            }
            if (node.DeployLabelMethod == DeployLabelMethod.UseAutoClassification)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    Id = Guid.Empty,
                    TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                    NewValue = ContentRepositoryAuditUtil.GetRulesLabelCretiaString(node.AutoClassificationRules)
                });
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
            }
            if ((DeployLabelMethod)node.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification)
            {
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
            }
        }
    }
    private void HandletInheritSettingGoogle(RMAuditInfo info, object[] args)
    {
        RMGoogleTreeNode node = (RMGoogleTreeNode)args[0];
        info.Object = node.DisplayName;
    }
}

