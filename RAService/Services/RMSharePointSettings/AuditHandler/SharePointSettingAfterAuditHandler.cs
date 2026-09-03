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
using AngleSharp.Dom;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Archiver;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler
{
    public class SharePointSettingAfterAuditHandler : IAfterAuditHandler
    {
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);

        public IRMSharePointSettingsService mRMSPSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();

        public IRMTeamsSettingsService TeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();

        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            if (info.Object == null && returnValue != null)
            {
                info.Object = returnValue.ToString();
            }
            //if (action == (int)AuditAction.RunSharePointSettingsScheduleJob)
            //{
            //    if ((int)args[0] == (int)JobRunBy.Schedule)
            //    {
            //        info.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
            //    }
            //}
            //else if (action == (int)AuditAction.RunSharePointSettingsScheduleJob)
            //{
            //    RAReturnMessage msg = (RAReturnMessage)returnValue;

            //    info.Status = msg.MessageType == RAMessageType.Successful ? 0 : 1;


            //}
            //else 
            if (action == (int)AuditAction.RunCollectionJob4SPOnPrem || action == (int)AuditAction.RunCollectionJob
                 || action == (int)AuditAction.RunCollectionJob4OneDrive || action == (int)AuditAction.RunCollectionJob4EXO
                 || action == (int)AuditAction.RunCollectionJob4Teams)
            {
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
                info.Object = returnValue?.ToString();
                var fromTimerJobPage = false;
                if (args.Length == 3)
                {
                    if (args[2] != null)
                    {
                        fromTimerJobPage = false;
                    }
                    else
                    {
                        fromTimerJobPage = true;
                    }
                }
                else
                {
                    fromTimerJobPage = true;
                }


                if (fromTimerJobPage)
                {
                    info.Module = AuditModule.ControlPanel;
                    info.Category = AuditCategory.TimerJobSettings;
                }
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
            }
            if (action == (int)AuditAction.ImportSPSetting)
            {
                info.Object = returnValue?.ToString();
            }
            if (action == (int)AuditAction.ExportSPSetting || action == (int)AuditAction.ExportSPSOSetting)
            {
                info.Object = returnValue?.ToString();
                info.ModifyContent = new List<AuditItem>();
                if (args[2].ToString().Equals(ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode.ToString()))
                {
                    info.ModifyContent.Add(new AuditItem
                    {
                        NewValue = "RM_JS_SP_ExportSetting_OptionAll"
                    });
                }
                else
                {
                    info.ModifyContent.Add(new AuditItem
                    {
                        NewValue = "RM_JS_SP_ExportSetting_OptionCustom"
                    });
                }
            }
            if (action == (int)AuditAction.ImportTeamsSetting)
            {
                info.Object = returnValue?.ToString();
            }
            if (action == (int)AuditAction.ExportTeamsSetting || action == (int)AuditAction.ExportTeamsSOSetting)
            {
                info.Object = returnValue?.ToString();
                info.ModifyContent = new List<AuditItem>();
                if (args[1].ToString().Equals(ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode.ToString()))
                {
                    info.ModifyContent.Add(new AuditItem
                    {
                        NewValue = "RM_JS_Teams_ExportSetting_OptionAll"
                    });
                }
                else
                {
                    info.ModifyContent.Add(new AuditItem
                    {
                        NewValue = "RM_JS_SP_ExportSetting_OptionCustom"
                    });
                }
            }
            if (action == (int)AuditAction.RunFSDashboardJob)
            {
                //var result = returnValue as RAReturnMessage;
                //info.Status = (int)result.MessageType;
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
                info.Object = returnValue?.ToString();
            }
            if (action == (int)AuditAction.FSMyHubDashboard)
            {
                //var result = returnValue as RAReturnMessage;
                //info.Status = (int)result.MessageType;
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
                info.Object = returnValue?.ToString();
            }


            //else if (action == (int)AuditAction.ApplySharePointSetting || action == (int)AuditAction.ApplyEXOSetting)
            //{
            //    //var msg = returnValue as RAReturnMessage;
            //    //info.Status = (int)msg.MessageType;
            //    info.Object = returnValue.ToString();
            //}
            //else 
            if (action == (int)AuditAction.EditEXOTermSetting)
            {
                RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                var enableTermSettings = !node.IsNullClassificationSetting;
                if (enableTermSettings)
                {
                    if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                    }
                }
            }
            else if (action == (int)AuditAction.EditDocLevelSetting || action == (int)AuditAction.EditTeamsDocLevelSetting)
            {
                List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == Guid.Empty).ToList();
                if (cretiaAudit.Count > 0)
                {
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    cretiaAudit[0].NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules);
                }
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if (action == (int)AuditAction.EditInheritSetting)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                RMSPTreeNode node = (RMSPTreeNode)args[0];
                //int parentCount = 0;
                RMSPSampleTreeNode newNode = new RMSPSampleTreeNode
                {
                    SPObjectId = node.SPObjectId,
                    FarmId = node.FarmId,
                    FarmName = node.FarmName,
                    SPType = node.SPType,
                    SPVersion = node.SPVersion,
                    TemplateId = node.TemplateId,
                    BposInfo = node.BposInfo,
                    Level = node.Level,
                    Parent = GetParentNode(node.Parent)
                };

                RMSPTreeNode parentNode = await mRMSPSettingsService.LoadSampleNodeSettingsAsync(newNode);
                if (parentNode != null)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                        OldValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                        NewValue = YesOrNoString(parentNode.EnableLifecycleManagementForSharePointLists ?? true)
                    });
                    string ownerNewValue = parentNode.RecordOwner.Count > 0 ? string.Join(";", parentNode.RecordOwner.Select(a => a.DisplayName)) : string.Empty;
                    string ownerOldValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;
                    if (ownerOldValue != null)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", OldValue = YesOrNoString(node.EMailToRecordOwner), NewValue = YesOrNoString(parentNode.EMailToRecordOwner) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", OldValue = ownerOldValue, NewValue = ownerNewValue });
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", NewValue = YesOrNoString(node.EMailToRecordOwner) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", NewValue = ownerNewValue });
                    }
                    string newSubsetPath = string.Empty;
                    string oldSubsetPath = string.Empty;
                    if (parentNode.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(parentNode.TermId);
                    }
                    else if (parentNode.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(parentNode.TermSetId);
                    }
                    if (node.TermId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                    }
                    else if (node.TermSetId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    }
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                        OldValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                        NewValue = parentNode.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                        OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)node.DeployTermMethod),
                        NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(parentNode.DeployTermMethod)
                    });

                    string newPath = string.Empty;
                    string oldPath = string.Empty;
                    if (parentNode.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(parentNode.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }
                    if (node.DefaultTermId != Guid.Empty)
                    {
                        oldPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                    }
                    else
                    {
                        oldPath = "RM_SS_NoDefaultValue";
                    }
                    bool oldApplyExistDocument = false;
                    oldApplyExistDocument = node.NeedCheckDefaultValue;
                    if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, node.ApplyExistType, node.IncludeDeclaredRecords) });
                    }
                    if (parentNode.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(parentNode.NeedCheckDefaultValue, parentNode.ApplyExistType, parentNode.IncludeDeclaredRecords) });
                    }
                    if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        var oldAutoRules = node.AutoClassificationRules;
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)node.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(node.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(node.IncludeDeclaredRecords) });
                    }
                    if (parentNode.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(parentNode.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(parentNode.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(parentNode.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(parentNode.IncludeDeclaredRecords) });
                    }

                    if (parentNode.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(parentNode.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(parentNode.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(parentNode.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(parentNode.IncludeDeclaredRecords) });
                    }

                    if (parentNode.Level == (int)NodeLevel.SiteCollection || parentNode.Level == (int)NodeLevel.Site)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SP_SettingRelatedRecords",
                            OldValue = YesOrNoString(node.EnableRelatedRecords),
                            NewValue = YesOrNoString(parentNode.EnableRelatedRecords)
                        });
                    }


                    string oldContainerPath = string.Empty, newContainerPath = string.Empty;
                    string oldContainerDes = string.Empty;
                    bool oldMarkPhysical = false;
                    if (node.TermIdOfContainer != Guid.Empty)
                    {
                        oldContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                    }
                    if (parentNode.TermIdOfContainer != Guid.Empty)
                    {
                        newContainerPath = TermDao.GetTermNamesPathByTermId(parentNode.TermIdOfContainer);
                    }
                    else
                    {
                    }
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", NewValue = newContainerPath, OldValue = oldContainerPath });
                    //i18n TODO
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", NewValue = parentNode.DescriptionOfContainer, OldValue = oldContainerDes });
                }
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if (action == (int)AuditAction.GeneralSetting4SPO)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if (action == (int)AuditAction.EditEXOInheritSetting)
            {

                RAReturnMessage msg = (RAReturnMessage)returnValue;
                RMEXOTreeNode node = (RMEXOTreeNode)args[0];
                RMSampleEXOTreeNode newNode = new RMSampleEXOTreeNode
                {
                    Id = node.Id,
                    Level = node.Level,
                    Name = node.Name,
                    DisplayName = node.DisplayName,
                    Title = node.Title,
                    FullPath = node.FullPath,
                    NodeType = node.NodeType,
                    Hidden = node.Hidden,
                    ChildrenCount = node.ChildrenCount,
                    Loaded = node.Loaded,
                    IncludeNew = node.IncludeNew,
                    Expanded = node.Expanded,
                    ParentId = node.ParentId,
                    CheckNumber = node.CheckNumber,
                    ChildrenIds = node.ChildrenIds,
                    IconStatus = node.IconStatus,
                    PageIndex = node.PageIndex,
                    GroupName = node.GroupName,
                    MailboxType = node.MailboxType,
                    InternalFolderPath = node.InternalFolderPath,
                    SiteCollectionUrl = node.SiteCollectionUrl,
                    Sender = node.Sender,
                    SendDate = node.SendDate,
                    DisplayTo = node.DisplayTo,
                    Email = node.Email,
                    Category = node.Category,
                    HasAttachment = node.HasAttachment,
                    OffSet = node.OffSet,
                    SubFolderCount = node.SubFolderCount,
                    Parent = node.Parent == null ? null : new RMSampleEXOTreeNode
                    {
                        Id = node.Parent.Id,
                        Level = node.Parent.Level,
                        Name = node.Parent.Name,
                        DisplayName = node.Parent.DisplayName,
                        Title = node.Parent.Title,
                        FullPath = node.Parent.FullPath,
                        NodeType = node.Parent.NodeType,
                        Hidden = node.Parent.Hidden,
                        ChildrenCount = node.Parent.ChildrenCount,
                        Loaded = node.Parent.Loaded,
                        IncludeNew = node.Parent.IncludeNew,
                        Expanded = node.Parent.Expanded,
                        ParentId = node.Parent.ParentId,
                        CheckNumber = node.Parent.CheckNumber,
                        ChildrenIds = node.Parent.ChildrenIds,
                        IconStatus = node.Parent.IconStatus,
                        PageIndex = node.Parent.PageIndex,
                        GroupName = node.Parent.GroupName,
                        MailboxType = node.Parent.MailboxType,
                        InternalFolderPath = node.Parent.InternalFolderPath,
                        SiteCollectionUrl = node.Parent.SiteCollectionUrl,
                        Sender = node.Parent.Sender,
                        SendDate = node.Parent.SendDate,
                        DisplayTo = node.Parent.DisplayTo,
                        Email = node.Parent.Email,
                        Category = node.Parent.Category,
                        HasAttachment = node.Parent.HasAttachment,
                        OffSet = node.Parent.OffSet,
                        SubFolderCount = node.Parent.SubFolderCount
                    }
                };

                RMEXOTreeNode parentNode = await mRMSPSettingsService.LoadExchangeNodeSettingAsync(newNode);

                string ownerOldValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;
                string ownerNewValue = parentNode.RecordOwner.Count > 0 ? string.Join(";", parentNode.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                if (ownerOldValue != null)
                {
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", OldValue = YesOrNoString(node.EMailToRecordOwner), NewValue = YesOrNoString(parentNode.EMailToRecordOwner) });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", OldValue = ownerOldValue, NewValue = ownerNewValue });
                }
                else
                {
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", NewValue = YesOrNoString(parentNode.EMailToRecordOwner) });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", NewValue = ownerNewValue });
                }


                bool oldApplyExistDocument = false;
                string newSubsetPath = string.Empty;
                string oldSubsetPath = string.Empty;
                //oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                if (parentNode.TermId != Guid.Empty)
                {
                    newSubsetPath = TermDao.GetTermNamesPathByTermId(parentNode.TermId);
                }
                else if (parentNode.TermSetId != Guid.Empty)
                {
                    newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(parentNode.TermSetId);
                }

                if (node.TermId != Guid.Empty)
                {
                    oldSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                }
                else if (node.TermSetId != Guid.Empty)
                {
                    oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                }

                string newPath = string.Empty;
                string oldPath = string.Empty;
                if (parentNode.DefaultTermId != Guid.Empty)
                {
                    newPath = TermDao.GetTermNamesPathByTermId(parentNode.DefaultTermId);
                }
                else
                {
                    newPath = "RM_SS_NoDefaultValue";
                }
                if (node.DefaultTermId != Guid.Empty)
                {
                    oldPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                }
                else
                {
                    oldPath = "RM_SS_NoDefaultValue";
                }
                //if (node.Level != (int)NodeLevel.WebApplication)
                //{
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                //}

                info.ModifyContent.Add(new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                    OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)node.DeployTermMethod, true),
                    NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(parentNode.DeployTermMethod, true)
                });
                if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                {
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, node.ApplyExistType) });
                }
                if (parentNode.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                {
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(parentNode.NeedCheckDefaultValue, parentNode.ApplyExistType) });
                }
                if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                {
                    var oldAutoRules = node.AutoClassificationRules;
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)node.AutoJobOption) });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(node.RunAutoFullJob) });
                }
                if (parentNode.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                        TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                        NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(parentNode.AutoClassificationRules)
                    });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(parentNode.AutoJobOption) });
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(parentNode.RunAutoFullJob) });
                }
            }

            else if (action == (int)AuditAction.EditSPOnPremDocLevelSetting)
            {
                List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == Guid.Empty).ToList();
                if (cretiaAudit.Count > 0)
                {
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    cretiaAudit[0].NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules);
                }
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }

            else if (action == (int)AuditAction.EditOneDriveTermSetting)
            {
                List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == Guid.Empty).ToList();
                if (cretiaAudit.Count > 0)
                {
                    RMSPTreeNode node = (RMSPTreeNode)args[0];
                    var enableTermSettings = !node.IsNullClassificationSetting;
                    if (enableTermSettings)
                    {
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                        }
                    }
                }
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if (action == (int)AuditAction.EditArchiverSetting || action == (int)AuditAction.EditArchiverSetting4OneDrive || action == (int)AuditAction.EditArchiverSetting4Teams)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if ( action == (int)AuditAction.InheritSubNodeToCurrent || action == (int)AuditAction.ArchiverGeneralSetting || action == (int)AuditAction.ArchiverGeneralSetting4OneDrive 
                    || action == (int)AuditAction.ArchiverGeneralSetting4Teams)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
                RMSPTreeNode node = (RMSPTreeNode)args[0];
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableArchiveManagement", NewValue = YesOrNoString(node.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable ? true : false) });
                if (node != null && (node.Level <= (int)NodeLevel.SiteCollection 
                    || (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.Office365GroupEntire)))
                {
                    //bool isFileLevelBackup = false;
                    //if (int.TryParse(_keyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                    //{
                    //    if (outputStreamLevel == (int)OutputStreamLevel.FileLevel)
                    //    {
                    //        isFileLevelBackup = true;
                    //    }
                    //}
                    if ((_keyValueDao.TryGetBoolValue(RMKeyValuesConstants.EnableDeleteRestoredDataFeature, out var enabled) && enabled))
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableDelDataCheckbox", NewValue = YesOrNoString(node.EnableDelArchivedData) });
                        if (node.EnableDelArchivedData)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_JS_JMD_Grid_Type",
                                NewValue = node.CleanupAndDelRestoredType switch
                                {
                                    CleanRestoreOption.None => "RM_RC_Audit_None",
                                    CleanRestoreOption.FileOrVersionOnly => "RM_AR_SPS_General_DelFileAndVersion",
                                    CleanRestoreOption.FileAndReletedVersions => "RM_AR_SPS_General_DelRelatedFileOrVersion",
                                    _ => "RM_RC_Audit_None",
                                }
                            });
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_CleanupRestoreDataDays", NewValue = node.DayNum.ToString() });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableCleanupAllStubsCheckbox", NewValue = YesOrNoString(node.EnableCleanStubs) });
                    }
                }
            }
            else if(action == (int)AuditAction.EditArchiverInheritSetting || action == (int)AuditAction.ArchiverInheritSetting4OneDrive || action == (int)AuditAction.ArchiverInheritSetting4Teams)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
                RMSPTreeNode node = (RMSPTreeNode)args[0];
                RMSPSampleTreeNode newNode = new RMSPSampleTreeNode
                {
                    SPObjectId = node.SPObjectId,
                    DisplayName = node.DisplayName,

                    FarmId = node.FarmId,
                    FarmName = node.FarmName,
                    SPType = node.SPType,
                    SPVersion = node.SPVersion,
                    TemplateId = node.TemplateId,
                    BposInfo = node.BposInfo,
                    Level = node.Level,
                    TeamsId = node.TeamsId,
                    Id = node.Id,
                    Parent = GetParentNode(node.Parent)
                };
                AvePoint.RA.Contract.Schedule.ScheduleType type = default;
                if(action == (int)AuditAction.EditArchiverInheritSetting)
                {
                    newNode.SourceType = (int)SourceFlag.SharePoint;
                    type = Contract.Schedule.ScheduleType.SPArchiveJobSchedule;
                }
                else if (action == (int)AuditAction.ArchiverInheritSetting4OneDrive)
                {
                    newNode.SourceType = (int)SourceFlag.OneDrive;
                    type = Contract.Schedule.ScheduleType.OneDriveArchiveJobSchedule;
                }
                else if (action == (int)AuditAction.ArchiverInheritSetting4Teams)
                {
                    newNode.SourceType = (int)SourceFlag.Teams;
                    type = Contract.Schedule.ScheduleType.TeamsArchiveJobSchedule;
                }
                RMSPTreeNode parentNode = RMArchiverSettingsService.LoadSampleNodeSettings(newNode, type);

                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableArchiveManagement", OldValue = YesOrNoString(node.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable ? true : false) });
                info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableArchiveManagement", NewValue = YesOrNoString(parentNode.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable ? true : false) });

                var oldRuleNames = String.Join("; ", node?.Rules?.Select(o => o.RuleName));
                AuditHelper.SaveOldAuditItem(info, "RM_JS_SPS_RuleNames_Title", oldRuleNames);

                var newRuleNames = String.Join("; ", parentNode.Rules?.Select(o => o.RuleName));
                AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_RuleNames_Title", newRuleNames);

                AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_Managed", YesOrNoString(node.IsManagedMetadataService));
                AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_Managed", YesOrNoString(parentNode.IsManagedMetadataService));

                AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_SuperUser", YesOrNoString(node.IsEnableSuperUserDecrypt));
                AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_SuperUser", YesOrNoString(parentNode.IsEnableSuperUserDecrypt));

                AuditHelper.SaveOldAuditItem(info, "RM_AR_SPS_Options_Remove_RetentionLabel", YesOrNoString(node.IsEnableRemoveRetentionLabel));
                AuditHelper.SaveNewAuditItem(info, "RM_AR_SPS_Options_Remove_RetentionLabel", YesOrNoString(parentNode.IsEnableRemoveRetentionLabel));


                if (node != null && node.Level<= (int)NodeLevel.SiteCollection 
                    || (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.Office365GroupEntire))
                {
                    //bool isFileLevelBackup = false;
                    //if (int.TryParse(_keyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                    //{
                    //    if (outputStreamLevel == (int)OutputStreamLevel.FileLevel)
                    //    {
                    //        isFileLevelBackup = true;
                    //    }
                    //}
                    if ((_keyValueDao.TryGetBoolValue(RMKeyValuesConstants.EnableDeleteRestoredDataFeature, out var enabled) && enabled))
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableDelDataCheckbox", OldValue = YesOrNoString(node.EnableDelArchivedData) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableDelDataCheckbox", NewValue = YesOrNoString(parentNode.EnableDelArchivedData) });
                        if (node.EnableDelArchivedData)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_JS_JMD_Grid_Type",
                                OldValue = node.CleanupAndDelRestoredType switch
                                {
                                    CleanRestoreOption.None => "RM_RC_Audit_None",
                                    CleanRestoreOption.FileOrVersionOnly => "RM_AR_SPS_General_DelFileAndVersion",
                                    CleanRestoreOption.FileAndReletedVersions => "RM_AR_SPS_General_DelRelatedFileOrVersion",
                                    _ => "RM_RC_Audit_None",
                                }
                            });
                        }
                        if (parentNode.EnableDelArchivedData)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = Guid.NewGuid(),
                                TargetSetting = "RM_JS_JMD_Grid_Type",
                                NewValue = parentNode.CleanupAndDelRestoredType switch
                                {
                                    CleanRestoreOption.None => "RM_RC_Audit_None",
                                    CleanRestoreOption.FileOrVersionOnly => "RM_AR_SPS_General_DelFileAndVersion",
                                    CleanRestoreOption.FileAndReletedVersions => "RM_AR_SPS_General_DelRelatedFileOrVersion",
                                    _ => "RM_RC_Audit_None",
                                }
                            });
                        }
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_CleanupRestoreDataDays", OldValue = node.DayNum.ToString() });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_CleanupRestoreDataDays", NewValue = parentNode.DayNum.ToString() });

                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableCleanupAllStubsCheckbox", OldValue = YesOrNoString(node.EnableCleanStubs) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_AR_SPS_General_EnableCleanupAllStubsCheckbox", NewValue = YesOrNoString(parentNode.EnableCleanStubs) });

                    }
                }
            }
            else if (action == (int)AuditAction.EditTeamsInheritSetting)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                RMSPTreeNode node = (RMSPTreeNode)args[0];
                //int parentCount = 0;
                RMSPSampleTreeNode newNode = new RMSPSampleTreeNode
                {
                    SPObjectId = node.SPObjectId,
                    FarmId = node.FarmId,
                    FarmName = node.FarmName,
                    SPType = node.SPType,
                    SPVersion = node.SPVersion,
                    TemplateId = node.TemplateId,
                    BposInfo = node.BposInfo,
                    Level = node.Level,
                    TeamsId = node.TeamsId,
                    Id = node.Id,
                    SourceType = (int)SourceFlag.Teams,
                    Parent = GetParentNode(node.Parent)
                };

                RMSPTreeNode parentNode = await TeamsSettingsService.LoadSampleNodeSettingsAsync(newNode);
                if (parentNode != null)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_SPS_EnableLifecycleManagementForSharePointLists",
                        OldValue = YesOrNoString(node.EnableLifecycleManagementForSharePointLists ?? true),
                        NewValue = YesOrNoString(parentNode.EnableLifecycleManagementForSharePointLists ?? true)
                    });
                    string ownerNewValue = parentNode.RecordOwner.Count > 0 ? string.Join(";", parentNode.RecordOwner.Select(a => a.DisplayName)) : string.Empty;
                    string ownerOldValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;
                    if (ownerOldValue != null)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", OldValue = YesOrNoString(node.EMailToRecordOwner), NewValue = YesOrNoString(parentNode.EMailToRecordOwner) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", OldValue = ownerOldValue, NewValue = ownerNewValue });
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation", NewValue = YesOrNoString(node.EMailToRecordOwner) });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_RecordOwners", NewValue = ownerNewValue });
                    }
                    string newSubsetPath = string.Empty;
                    string oldSubsetPath = string.Empty;
                    if (parentNode.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(parentNode.TermId);
                    }
                    else if (parentNode.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(parentNode.TermSetId);
                    }
                    if (node.TermId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                    }
                    else if (node.TermSetId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    }
                    info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_SubsetTerm", OldValue = oldSubsetPath, NewValue = newSubsetPath });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_SPS_Auditor_DisplayTerm_DisplayValue",
                        OldValue = node.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel",
                        NewValue = parentNode.IsDisplyaTermPath ? "RM_SPS_Auditor_DisplayTerm_EntirePath" : "RM_SPS_Auditor_DisplayTerm_TermLabel"
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                        OldValue = ContentRepositoryAuditUtil.GetApplyTermMethodString((DeployTermMethod)node.DeployTermMethod),
                        NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(parentNode.DeployTermMethod)
                    });

                    string newPath = string.Empty;
                    string oldPath = string.Empty;
                    if (parentNode.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(parentNode.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }
                    if (node.DefaultTermId != Guid.Empty)
                    {
                        oldPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                    }
                    else
                    {
                        oldPath = "RM_SS_NoDefaultValue";
                    }
                    bool oldApplyExistDocument = false;
                    oldApplyExistDocument = node.NeedCheckDefaultValue;
                    if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, node.ApplyExistType, node.IncludeDeclaredRecords) });
                    }
                    if (parentNode.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                    {
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(parentNode.NeedCheckDefaultValue, parentNode.ApplyExistType, parentNode.IncludeDeclaredRecords) });
                    }
                    if ((DeployTermMethod)node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        var oldAutoRules = node.AutoClassificationRules;
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)node.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(node.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", OldValue = YesOrNoString(node.IncludeDeclaredRecords) });
                    }
                    if (parentNode.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(parentNode.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(parentNode.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(parentNode.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(parentNode.IncludeDeclaredRecords) });
                    }

                    if (parentNode.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.Empty,//empty代表这条audit不完善，需要在AfterHandler里继续完善
                            TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                            NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(parentNode.AutoClassificationRules)
                        });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(parentNode.AutoJobOption) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(parentNode.RunAutoFullJob) });
                        info.ModifyContent.Add(new AuditItem() { Id = Guid.NewGuid(), TargetSetting = "RM_JS_SPS_IncludeDeclaredRecords", NewValue = YesOrNoString(parentNode.IncludeDeclaredRecords) });
                    }

                    if (parentNode.Level == (int)NodeLevel.Office365GroupEntire || parentNode.Level == (int)NodeLevel.SiteCollection || parentNode.Level == (int)NodeLevel.Site)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RM_SP_SettingRelatedRecords",
                            OldValue = YesOrNoString(node.EnableRelatedRecords),
                            NewValue = YesOrNoString(parentNode.EnableRelatedRecords)
                        });
                    }


                    string oldContainerPath = string.Empty, newContainerPath = string.Empty;
                    string oldContainerDes = string.Empty;
                    bool oldMarkPhysical = false;
                    if (node.TermIdOfContainer != Guid.Empty)
                    {
                        oldContainerPath = TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer);
                    }
                    if (parentNode.TermIdOfContainer != Guid.Empty)
                    {
                        newContainerPath = TermDao.GetTermNamesPathByTermId(parentNode.TermIdOfContainer);
                    }
                    else
                    {
                    }
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel", NewValue = newContainerPath, OldValue = oldContainerPath });
                    //i18n TODO
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_ContainerLevel_Des", NewValue = parentNode.DescriptionOfContainer, OldValue = oldContainerDes });
                }
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            return info;
        }

        private RMSPSampleTreeNode GetParentNode(RMSPTreeNode node)
        {
            if(node!=null)
            {
                return new RMSPSampleTreeNode
                {
                    SPObjectId = node.SPObjectId,
                    FarmId = node.FarmId,
                    FarmName = node.FarmName,
                    SPType = node.SPType,
                    SPVersion = node.SPVersion,
                    TemplateId = node.TemplateId,
                    BposInfo = node.BposInfo,
                    Level = node.Level,
                    TeamsId = node.TeamsId,
                    Id = node.Id,
                    Parent = GetParentNode(node.Parent)
                };
            }
            else
            {
                return null;
            }
        }
        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
        private string GetApplyExistString(bool applyExistDocument, int applyExistType, bool includeIncludeDeclaredRecords)
        {
            if (applyExistDocument)
            {
                var includeString = "; " + "RM_JS_SPS_IncludeDeclaredRecords ";
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm " + (includeIncludeDeclaredRecords ? includeString : "");
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplySkipTerm " + (includeIncludeDeclaredRecords ? includeString : "");
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
        private string GetApplyExistString(bool applyExistDocument, int applyExistType)
        {
            if (applyExistDocument)
            {
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm ";
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + "; " + "RM_JS_SPS_AutoClassification_ApplySkipTerm ";
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
