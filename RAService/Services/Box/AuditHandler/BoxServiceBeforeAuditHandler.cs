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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box.AuditHandler
{
    public class BoxServiceBeforeAuditHandler : IBeforeAuditHandler
    {

        public IBoxSettingDao BoxSettingDao => PlatformWindsorManager.GetService<IBoxSettingDao>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            info.Module = (AuditModule)model;

            if (action == (int)AuditAction.BoxSaveTermSetting)
            {
                #region AuditAction.BoxSaveTermSetting

                BoxSettingDto dto = (BoxSettingDto)args[0];
                var node = dto.SelectedNode;
                info.Object = node.FullPath;
                var dbSetting = BoxSettingDao.GetSettingByScopeIdAndGroupId(node.Id, node.ContainerId);
                if (dbSetting != null)
                {
                    bool oldApplyExistDocument = false;
                    string newSubsetPath = string.Empty;
                    string oldSubsetPath = string.Empty;
                    oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                    if (dto.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(dto.TermId);
                    }
                    else if (dto.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dto.TermSetId);
                    }

                    if (dbSetting.TermId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                    }
                    else if (dbSetting.TermSetId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                    }
                    string newPath = string.Empty;
                    string oldPath = string.Empty;
                    if (dto.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(dto.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }
                    if (dbSetting.DefaultTermId != Guid.Empty)
                    {
                        oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                    }
                    else
                    {
                        oldPath = "RM_SS_NoDefaultValue";
                    }
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                        OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)dbSetting.DeployTermMethod),
                        NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(dto.DeployTermMethod)
                    });
                    if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType) });
                    }
                    if (dto.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(dto.NeedCheckDefaultValue, dto.ApplyExistType) });
                    }
                    if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                    }
                    if (dto.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = ContentRepositoryAuditUtil.NeedReAuditorInAfter,
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(dto.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(dto.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(dto.RunAutoFullJob) });
                    }
                }
                else
                {
                    string newSubsetPath = string.Empty;
                    if (dto.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(dto.TermId);
                    }
                    else if (dto.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dto.TermSetId);
                    }

                    string newPath = string.Empty;
                    if (dto.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(dto.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }

                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                        NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(dto.DeployTermMethod)
                    });
                    if (dto.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(dto.NeedCheckDefaultValue, dto.ApplyExistType) });
                    }
                    if (dto.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = ContentRepositoryAuditUtil.NeedReAuditorInAfter,
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(dto.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(dto.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(dto.RunAutoFullJob) });
                    }
                }
                #endregion
            }
            else if (action == (int)AuditAction.BoxInheritSetting)
            {
                #region AuditAction.BoxInheritSetting
                BoxTreeNode node = (BoxTreeNode)args[0];
                info.Object = node.FullPath;
                #endregion
            }
            else if (action == (int)AuditAction.BoxDeactiveSetting)
            {
                #region AuditAction.BoxDeactiveSetting
                BoxSettingDto dto = (BoxSettingDto)args[0];
                var node = dto.SelectedNode;
                if (dto.IsActive)
                {
                    info.Action = AuditAction.BoxActiveSetting;
                }
                info.Object = node.FullPath;
                #endregion
            }
            return info;
        }

        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
        private string GetApplyExistString(bool oldApplyExistDocument, int applyExistType)
        {
            if (oldApplyExistDocument)
            {
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + " " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm ";
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + " " + "RM_JS_SPS_AutoClassification_ApplySkipTerm ";
                }
                else
                {
                    return "RM_JS_Common_Yes";
                }
            }
            else
            {
                return "RM_JS_Common_No";
            }
        }
    }
}
