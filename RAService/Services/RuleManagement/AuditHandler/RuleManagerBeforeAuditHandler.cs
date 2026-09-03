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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using RAManualApprovalCommon;
using AvePoint.RA.Contract.Common;
using RATeams;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.RuleManagement.AuditHandler
{
    public class RuleManagerBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(RuleManagerBeforeAuditHandler));
        private IRuleManagerService mRuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly Dictionary<string, bool> _storageIsSystemDic = [];

        public async Task<RMAuditInfo> CollectAsync(int mode, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {

                List<Rule> rules = new List<Rule>();
                var isNewLogicAccount = TenantService.IsNewOpusTenant();
                switch ((AuditAction)action)
                {
                    case AuditAction.CreateRule:
                        break;
                    case AuditAction.EditRule:

                        RMRuleInfos rule = await mRuleManagerService.LoadRuleAsync(((RMRuleInfos)args[0]).RuleId);
                        RMRuleInfos newRule = (RMRuleInfos)args[0];
                        if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }

                        AuditItem descItem = new AuditItem();
                        descItem.TargetSetting = "RM_JS_RDM_Rule_Description";
                        descItem.OldValue = rule.Description;
                        info.ModifyContent.Add(descItem);

                        AuditItem ruleContainerItem = new AuditItem();
                        ruleContainerItem.TargetSetting = "RM_JS_Rule_Detail_RuleContainer";
                        ruleContainerItem.OldValue = rule.ContainerName;
                        info.ModifyContent.Add(ruleContainerItem);

                        AuditItem disposalClassItem = new AuditItem();
                        disposalClassItem.TargetSetting = "RM_RDM_CreateRule_DisposalClass_Title";
                        disposalClassItem.OldValue = rule.DisposalClass;
                        info.ModifyContent.Add(disposalClassItem);

                        //SPSource
                        AuditItem spSource = new AuditItem();
                        spSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedSPSource";
                        spSource.OldValue = rule.IsSpSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        info.ModifyContent.Add(spSource);

                        if (rule.IsSpSource)
                        {
                            AuditItem conditionItem = new AuditItem();
                            conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition";
                            rule.RuleCretias.Add(rule.FilterCombineMode);
                            conditionItem.OldValue = string.Join("<br>", rule.RuleCretias);
                            info.ModifyContent.Add(conditionItem);

                            AuditItem actionItem = new AuditItem();
                            actionItem.TargetSetting = "RM_JS_RDM_DisposalAction";
                            actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule, isNewLogicAccount: isNewLogicAccount, sourceFlag: RA.SharePoint.ArchiverCommon.SOSourceFlag.SharePoint);

                            info.ModifyContent.Add(actionItem);

                            //AuditItem declaredFileItem = new AuditItem();
                            //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                            //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                            //info.ModifyContent.Add(declaredFileItem);
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval";
                                manualItem.OldValue = rule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                            }
                            if (rule.EnableManualApproval)
                            {
                                if (!string.IsNullOrEmpty(rule.WorkflowId))
                                {
                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName";
                                    processItem.OldValue = rule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                else {
                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner";
                                    emailUsers.OldValue = rule.Users != null ? string.Join("; ", rule.Users.Select(u => u.DisplayName)) : "";
                                    info.ModifyContent.Add(emailUsers);
                                }

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner";
                                sendEmailSetting.OldValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(sendEmailSetting);
                            }
                            else
                            {
                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName";
                                processItem.OldValue = rule.WorkflowName;
                                info.ModifyContent.Add(processItem);
                            }
                            AuditItem exportItem = new AuditItem();
                            exportItem.TargetSetting = "RM_JS_RDM_ExportAction";
                            exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.ExportInfo);
                            info.ModifyContent.Add(exportItem);

                            AuditItem storageName = new AuditItem();
                            storageName.Id = new Guid(SOConstants.SPAuditId);
                            storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                            storageName.OldValue = rule.StoragePolicyName;
                            info.ModifyContent.Add(storageName);

                            AuditItem moveArchiveTierType = new AuditItem();
                            moveArchiveTierType.Id = new Guid(SOConstants.SPAuditId);
                            moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                            moveArchiveTierType.OldValue = rule.MoveToArchiverTierWhenArchiving? "RM_RDM_CreateRule_ArchivedTier" : rule.MoveToAnotherTierType switch
                            {
                                0 => "RM_RDM_CreateRule_DefaultTier",
                                3 => "RM_RDM_CreateRule_ArchivedTier",
                                4 => "RM_RDM_CreateRule_ColdTier",
                                _ => "RM_RDM_CreateRule_DefaultTier"
                            };//0 default,3 archive,4 cold
                            if (!string.IsNullOrEmpty(rule.StoragePolicyId) && !IsSystemStorage(rule.StoragePolicyId) && (rule.MoveToArchiverTierWhenArchiving || rule.MoveToAnotherTierType != null))
                            {
                                info.ModifyContent.Add(moveArchiveTierType);
                            }
                            bool isEnableRetention = rule.RetentionInfoList == null ? rule.RetentionInfo == null ? false : true : true;
                            bool newRuleIsEnableRetention = newRule?.RetentionInfoList == null ? newRule?.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() {TargetSetting = "RM_JS_Rule_Detail_Retention",OldValue = isEnableRetention ? "RM_JS_Common_Yes": "RM_JS_Common_No" };
                            if (isEnableRetention || newRuleIsEnableRetention)
                            {
                                BuildTemplateForRetentionInfoList(rule, newRule, info, new Guid(SOConstants.SPAuditId));
                                if (rule.RetentionInfoList != null)
                                {
                                    for (int processedCount = 0; processedCount < rule.RetentionInfoList.Count(); processedCount++)
                                    {
                                        RetentionSettings infoList = rule.RetentionInfoList[processedCount];
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                            if(retentionTime != null)
                                            {
                                                retentionTime.OldValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                };
                                            }

                                            AuditItem retentionAction = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_AR_CP_GSS_OperateDataTitle") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                            if (retentionAction != null)
                                            {
                                                retentionAction.OldValue = infoList.OperateDataType switch
                                                {
                                                    (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                    (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                    {
                                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                    },
                                                    _ => ""
                                                };
                                            }

                                            if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                            {
                                                AuditItem removeStub = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                                if (removeStub != null)
                                                {
                                                    removeStub.OldValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                }

                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete" && x.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n"+ string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No",
                                                            Deep = processedCount
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes "+"\n"+ string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                        {
                                                            TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                            TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                            TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                            TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                            _ => ""
                                                        }) : "RM_JS_Common_No";
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                                else if (rule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = new AuditItem()
                                    {
                                        Id = new Guid(SOConstants.SPAuditId),
                                        TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                        OldValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + rule.RetentionInfo.KeepDateNumber + " " + rule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + rule.RetentionInfo.Date,
                                            _ => ""
                                        }
                                    };
                                    info.ModifyContent.Add(retentionTime);
                                    if (rule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = new AuditItem()
                                            {
                                                Id = new Guid(SOConstants.SPAuditId),
                                                TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                OldValue = workFlow.Name
                                            };
                                            info.ModifyContent.Add(workFlowAudit);
                                        }
                                        else if (rule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = new AuditItem()
                                            {
                                                Id = new Guid(SOConstants.SPAuditId),
                                                TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                OldValue = string.Join(",", rule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                            };
                                            info.ModifyContent.Add(recordOwnerAudit);
                                        };
                                    }
                                    AuditItem sendEmail = new AuditItem()
                                    {
                                        Id = new Guid(SOConstants.SPAuditId),
                                        TargetSetting = "RM_SPS_SendEMail",
                                        OldValue = rule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(sendEmail);
                                }
                            } 
                        }
                        else
                        {
                            AuditItem criteria = new AuditItem();
                            criteria.TargetSetting = "RM_JS_RDM_DisposalCondition";
                            criteria.OldValue = "";
                            info.ModifyContent.Add(criteria);

                            AuditItem ruleAction = new AuditItem();
                            ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction";
                            ruleAction.OldValue = "";
                            info.ModifyContent.Add(ruleAction);

                            AuditItem manualApprove = new AuditItem();
                            manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval";
                            manualApprove.OldValue = "";
                            info.ModifyContent.Add(manualApprove);

                            //Send Email Setting
                            AuditItem sendEmailSetting = new AuditItem();
                            sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner";
                            sendEmailSetting.OldValue = "";
                            info.ModifyContent.Add(sendEmailSetting);

                            //Email Users
                            AuditItem emailUsers = new AuditItem();
                            emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner";
                            emailUsers.OldValue = "";
                            info.ModifyContent.Add(emailUsers);

                            //workflow
                            AuditItem processItem = new AuditItem();
                            processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName";
                            processItem.OldValue = rule.WorkflowName;
                            info.ModifyContent.Add(processItem);

                            AuditItem export = new AuditItem();
                            export.TargetSetting = "RM_JS_RDM_ExportAction";
                            export.OldValue = "";
                            info.ModifyContent.Add(export);                                                 
                        }

                        //oneDriveSource
                        info.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = "RM_JS_RDM_Rule_IsCheckedOneDriveSource",
                            OldValue = rule.IsOneDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                        });

                        if (rule.IsOneDriveSource)
                        {
                            var oneDriveRule = rule.OneDriveRule;
                            var newOneDriveRule = newRule?.OneDriveRule;
                            oneDriveRule.RuleCretias.Add(oneDriveRule.FilterCombineMode);
                            AuditItem conditionItem = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalCondition_ONE",
                                OldValue = string.Join("<br>", oneDriveRule.RuleCretias)
                            };
                            info.ModifyContent.Add(conditionItem);

                            AuditItem actionItem = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalAction_ONE",
                                OldValue = RuleAuditUtil.GetAuditorRuleActionString(oneDriveRule, RuleModel.None, RA.SharePoint.ArchiverCommon.SOSourceFlag.OneDrive, isNewLogicAccount)
                            };
                            info.ModifyContent.Add(actionItem);
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem manualItem = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_ManualApproval_ONE",
                                    OldValue = oneDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                };
                                info.ModifyContent.Add(manualItem);
                            }
                            if (oneDriveRule.EnableManualApproval)
                            {
                                if (!string.IsNullOrEmpty(oneDriveRule.WorkflowId))
                                {
                                    //workflow
                                    AuditItem processItem = new AuditItem
                                    {
                                        TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_ONE",
                                        OldValue = oneDriveRule.WorkflowName
                                    };
                                    info.ModifyContent.Add(processItem);
                                }
                                else
                                {
                                    //Email Users
                                    AuditItem emailUsers = new AuditItem
                                    {
                                        TargetSetting = "RM_JS_MA_Grid_RecordOwner_ONE",
                                        OldValue = oneDriveRule.Users != null ? string.Join("; ", oneDriveRule.Users.Select(u => u.DisplayName)) : ""
                                    };
                                    info.ModifyContent.Add(emailUsers);
                                }

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem
                                {
                                    TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_ONE",
                                    OldValue = oneDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                };
                                info.ModifyContent.Add(sendEmailSetting);
                            }
                            else
                            {
                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem
                                {
                                    TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_ONE",
                                    OldValue = ""
                                };
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem
                                {
                                    TargetSetting = "RM_JS_MA_Grid_RecordOwner_ONE",
                                    OldValue = ""
                                };
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_ONE",
                                    OldValue = oneDriveRule.WorkflowName
                                };
                                info.ModifyContent.Add(processItem);
                            }

                            if(oneDriveRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection)
                            {
                                AuditItem exportItem = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_ExportAction_ONE",
                                    OldValue = RuleAuditUtil.GetExportInfo(oneDriveRule.ExportInfo)
                                };
                                info.ModifyContent.Add(exportItem);
                            }
                            

                            AuditItem storageName = new AuditItem();
                            storageName.Id = new Guid(SOConstants.ODAuditId);
                            storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_ONE";
                            storageName.OldValue = rule.OneDriveRule.StoragePolicyName;
                            info.ModifyContent.Add(storageName);

                            AuditItem moveArchiveTierType = new AuditItem();
                            moveArchiveTierType.Id = new Guid(SOConstants.ODAuditId);
                            moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                            moveArchiveTierType.OldValue = oneDriveRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : oneDriveRule.MoveToAnotherTierType switch
                            {
                                0 => "RM_RDM_CreateRule_DefaultTier",
                                3 => "RM_RDM_CreateRule_ArchivedTier",
                                4 => "RM_RDM_CreateRule_ColdTier",
                                _ => "RM_RDM_CreateRule_DefaultTier"
                            };//0 default,3 archive,4 cold
                            if (!string.IsNullOrWhiteSpace(oneDriveRule.StoragePolicyId) && !IsSystemStorage(rule.OneDriveRule.StoragePolicyId) && (oneDriveRule.MoveToArchiverTierWhenArchiving || oneDriveRule.MoveToAnotherTierType != null))
                            {
                                info.ModifyContent.Add(moveArchiveTierType);
                            }
                            bool isEnableRetention = oneDriveRule.RetentionInfoList == null ? oneDriveRule.RetentionInfo == null ? false : true : true;
                            bool newRuleIsEnableRetention = newOneDriveRule?.RetentionInfoList == null ? newOneDriveRule?.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { Id = new Guid(SOConstants.ODAuditId), TargetSetting = "RM_JS_Rule_Detail_Retention", OldValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention || newRuleIsEnableRetention)
                            {
                                BuildTemplateForRetentionInfoList(oneDriveRule, newOneDriveRule, info, new Guid(SOConstants.ODAuditId));
                                if (oneDriveRule.RetentionInfoList != null)
                                {
                                    for (int processedCount = 0; processedCount < rule.OneDriveRule.RetentionInfoList.Count(); processedCount++)
                                    {
                                        RetentionSettings infoList = rule.OneDriveRule.RetentionInfoList[processedCount];
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                            if (retentionTime != null)
                                            {
                                                retentionTime.OldValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                };
                                            }

                                            AuditItem retentionAction = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_AR_CP_GSS_OperateDataTitle") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                            if (retentionAction != null)
                                            {
                                                retentionAction.OldValue = infoList.OperateDataType switch
                                                {
                                                    (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                    (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                    {
                                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                    },
                                                    _ => ""
                                                };
                                            }

                                            if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                            {
                                                AuditItem removeStub = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                                if (removeStub != null)
                                                {
                                                    removeStub.OldValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                }

                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete").LastOrDefault();
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No",
                                                            Deep = processedCount
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                        {
                                                            TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                            TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                            TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                            TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                            _ => ""
                                                        }) : "RM_JS_Common_No";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (oneDriveRule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = new AuditItem()
                                    {
                                        Id = new Guid(SOConstants.ODAuditId),
                                        TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                        OldValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + oneDriveRule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + oneDriveRule.RetentionInfo.KeepDateNumber + " " + oneDriveRule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + oneDriveRule.RetentionInfo.Date,
                                            _ => ""
                                        }
                                    };
                                    info.ModifyContent.Add(retentionTime);
                                    if (oneDriveRule.RetentionInfo.IsManualApproval)
                                    {
                                        if (oneDriveRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(oneDriveRule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = new AuditItem()
                                            {
                                                Id = new Guid(SOConstants.ODAuditId),
                                                TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                OldValue = workFlow.Name
                                            };
                                            info.ModifyContent.Add(workFlowAudit);
                                        }
                                        else if (oneDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = new AuditItem()
                                            {
                                                Id = new Guid(SOConstants.ODAuditId),
                                                TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                OldValue = string.Join(",", oneDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                            };
                                            info.ModifyContent.Add(recordOwnerAudit);
                                        };
                                    }
                                    AuditItem sendEmail = new AuditItem()
                                    {
                                        Id = new Guid(SOConstants.ODAuditId),
                                        TargetSetting = "RM_SPS_SendEMail",
                                        OldValue = oneDriveRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(sendEmail);
                                }
                            }
                        }
                        else
                        {
                            AuditItem criteria = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalCondition_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(criteria);

                            AuditItem ruleAction = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalAction_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(ruleAction);

                            AuditItem manualApprove = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_ManualApproval_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(manualApprove);

                            //Send Email Setting
                            AuditItem sendEmailSetting = new AuditItem
                            {
                                TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(sendEmailSetting);

                            //Email Users
                            AuditItem emailUsers = new AuditItem
                            {
                                TargetSetting = "RM_JS_MA_Grid_RecordOwner_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(emailUsers);

                            //workflow
                            AuditItem processItem = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(processItem);

                            AuditItem export = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_ExportAction_ONE",
                                OldValue = ""
                            };
                            info.ModifyContent.Add(export);
                        }

                        //Teams
                        if(rule.ModelType == RuleModel.SOArchiver && (newRule.RuleLevel == GCommon.Contract.CommonFilter.PolicyLevel.Teams || rule.RuleLevel == GCommon.Contract.CommonFilter.PolicyLevel.Teams))
                        {
                            if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
                            {
                                AuditItem teamsSource = new AuditItem();
                                teamsSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedTeamsSource";
                                teamsSource.OldValue = rule.IsTeamsSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(teamsSource);
                            }

                            if (rule.IsTeamsSource)
                            {
                                var teamsRule = rule.TeamsRule;
                                var newTeamsRule = newRule?.TeamsRule;
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_TMS";
                                teamsRule.RuleCretias.Add(teamsRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", teamsRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_TMS";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(teamsRule, rule.ModelType);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = teamsRule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);
                                if (rule.ModelType == RuleModel.Records)
                                {
                                    AuditItem manualItem = new AuditItem();
                                    manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_TMS";
                                    manualItem.OldValue = teamsRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(manualItem);
                                }
                                if (teamsRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(teamsRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_TMS";
                                        processItem.OldValue = teamsRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_TMS";
                                        emailUsers.OldValue = teamsRule.Users != null ? string.Join("; ", teamsRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_TMS";
                                    sendEmailSetting.OldValue = teamsRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_TMS";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_TMS";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_TMS";
                                    processItem.OldValue = teamsRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                            if(teamsRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.Teams)
                                {
                                    AuditItem exportItem = new AuditItem();
                                    exportItem.TargetSetting = "RM_JS_RDM_ExportAction_TMS";
                                    exportItem.OldValue = RuleAuditUtil.GetExportInfo(teamsRule.ExportInfo);
                                    info.ModifyContent.Add(exportItem);
                                }

                                AuditItem storageName = new AuditItem();
                                storageName.Id = new Guid(SOConstants.TEAMSAuditId);
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_TMS";
                                storageName.OldValue = teamsRule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);

                                AuditItem moveArchiveTierType = new AuditItem();
                                moveArchiveTierType.Id = new Guid(SOConstants.TEAMSAuditId);
                                moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                moveArchiveTierType.OldValue = teamsRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : teamsRule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => "RM_RDM_CreateRule_DefaultTier"
                                };//0 default,3 archive,4 cold
                                if (!IsSystemStorage(teamsRule.StoragePolicyId) && (teamsRule.MoveToArchiverTierWhenArchiving || teamsRule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }
                                bool isEnableRetention = teamsRule.RetentionInfoList == null ? teamsRule.RetentionInfo == null ? false : true : true;
                                bool newRuleIsEnableRetention = newTeamsRule?.RetentionInfoList == null ? newTeamsRule?.RetentionInfo == null ? false : true : true;
                                AuditItem enableRetention = new AuditItem() { Id = new Guid(SOConstants.TEAMSAuditId), TargetSetting = "RM_JS_Rule_Detail_Retention", OldValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                                if (isEnableRetention || newRuleIsEnableRetention)
                                {
                                    BuildTemplateForRetentionInfoList(teamsRule, newTeamsRule, info, new Guid(SOConstants.TEAMSAuditId));
                                    if (teamsRule.RetentionInfoList != null)
                                    {
                                        for (int processedCount = 0; processedCount < teamsRule.RetentionInfoList.Count(); processedCount++)
                                        {
                                            RetentionSettings infoList = teamsRule.RetentionInfoList[processedCount];
                                            if (infoList.IsEnableRetention)
                                            {
                                                string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                                AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                                if (retentionTime != null)
                                                {
                                                    retentionTime.OldValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                    {
                                                        TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                        TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                        TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                        TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                        _ => ""
                                                    };
                                                }

                                                AuditItem retentionAction = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_AR_CP_GSS_OperateDataTitle") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                                if (retentionAction != null)
                                                {
                                                    retentionAction.OldValue = infoList.OperateDataType switch
                                                    {
                                                        (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                        (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                        {
                                                            (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                            (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                        },
                                                        _ => ""
                                                    };
                                                }

                                                if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                                {
                                                    AuditItem removeStub = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                                    if (removeStub != null)
                                                    {
                                                        removeStub.OldValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                    }

                                                    if (KeyValueService.IsEnableSoftDeleteSetting())
                                                    {
                                                        var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete" && x.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                                        if (tempAuditItem == null)
                                                        {
                                                            AuditItem softDelete = new AuditItem()
                                                            {
                                                                TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                                OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                                {
                                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                    _ => ""
                                                                }) : "RM_JS_Common_No",
                                                                Deep = processedCount
                                                            };
                                                            info.ModifyContent.Add(softDelete);
                                                        }
                                                        else
                                                        {
                                                            tempAuditItem.OldValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No";
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (teamsRule.RetentionInfo != null)
                                    {
                                        AuditItem retentionTime = new AuditItem()
                                        {
                                            Id = new Guid(SOConstants.SPAuditId),
                                            TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                            OldValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + teamsRule.RetentionInfo.Condition switch
                                            {
                                                TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + teamsRule.RetentionInfo.KeepDateNumber + " " + teamsRule.RetentionInfo.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                },
                                                TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + teamsRule.RetentionInfo.Date,
                                                _ => ""
                                            }
                                        };
                                        info.ModifyContent.Add(retentionTime);
                                        if (teamsRule.RetentionInfo.IsManualApproval)
                                        {
                                            if (teamsRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                            {
                                                var workFlow = ManualApprovalWorkflowManager.Get(teamsRule.RetentionInfo.WorkflowId);
                                                AuditItem workFlowAudit = new AuditItem()
                                                {
                                                    Id = new Guid(SOConstants.SPAuditId),
                                                    TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                    OldValue = workFlow.Name
                                                };
                                                info.ModifyContent.Add(workFlowAudit);
                                            }
                                            else if (teamsRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                            {
                                                AuditItem recordOwnerAudit = new AuditItem()
                                                {
                                                    Id = new Guid(SOConstants.SPAuditId),
                                                    TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                    OldValue = string.Join(",", teamsRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                                };
                                                info.ModifyContent.Add(recordOwnerAudit);
                                            };
                                        }
                                        AuditItem sendEmail = new AuditItem()
                                        {
                                            Id = new Guid(SOConstants.SPAuditId),
                                            TargetSetting = "RM_SPS_SendEMail",
                                            OldValue = teamsRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                        };
                                        info.ModifyContent.Add(sendEmail);
                                    }
                                }
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_TMS";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_TMS";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_TMS";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_TMS";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_TMS";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_TMS";
                                processItem.OldValue = rule.WorkflowName;
                                info.ModifyContent.Add(processItem);

                                AuditItem export = new AuditItem();
                                export.TargetSetting = "RM_JS_RDM_ExportAction_TMS";
                                export.OldValue = "";
                                info.ModifyContent.Add(export);
                            }
                        }

                        if (rule.ModelType != RuleModel.SOArchiver)
                        {
                            //EXOSource
                            AuditItem exoSource = new AuditItem();
                            exoSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedEXOSource";
                            exoSource.OldValue = rule.IsExoSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(exoSource);

                            if (rule.IsExoSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_EXO";
                                rule.EXORule.RuleCretias.Add(rule.EXORule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.EXORule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_EXO";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.EXORule);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_EXO";
                                manualItem.OldValue = rule.EXORule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.EXORule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.EXORule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_EXO";
                                        processItem.OldValue = rule.EXORule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_EXO";
                                        emailUsers.OldValue = rule.EXORule.Users != null ? string.Join("; ", rule.EXORule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_EXO";
                                    sendEmailSetting.OldValue = rule.EXORule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem exoSndEmailSetting = new AuditItem();
                                    exoSndEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_EXO";
                                    exoSndEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(exoSndEmailSetting);

                                    //Email Users
                                    AuditItem exoEmailUsers = new AuditItem();
                                    exoEmailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_EXO";
                                    exoEmailUsers.OldValue = "";
                                    info.ModifyContent.Add(exoEmailUsers);

                                    //workflow
                                    AuditItem exoProcessItem = new AuditItem();
                                    exoProcessItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_EXO";
                                    exoProcessItem.OldValue = "";
                                    info.ModifyContent.Add(exoProcessItem);
                                }
                                AuditItem exportItem = new AuditItem();
                                exportItem.TargetSetting = "RM_JS_RDM_ExportAction_EXO";
                                exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.EXORule.ExportInfo);
                                info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                //Rule Level and Criteria
                                AuditItem exoCriteria = new AuditItem();
                                exoCriteria.TargetSetting = "RM_JS_RDM_DisposalCondition_EXO";
                                exoCriteria.OldValue = "";
                                info.ModifyContent.Add(exoCriteria);

                                //Rule Action
                                AuditItem exoRuleAction = new AuditItem();
                                exoRuleAction.TargetSetting = "RM_JS_RDM_DisposalAction_EXO";
                                exoRuleAction.OldValue = "";
                                info.ModifyContent.Add(exoRuleAction);

                                //Manual Approve
                                AuditItem exoManualApprove = new AuditItem();
                                exoManualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_EXO";
                                exoManualApprove.OldValue = "";
                                info.ModifyContent.Add(exoManualApprove);
                                //Send Email Setting
                                AuditItem exoSndEmailSetting = new AuditItem();
                                exoSndEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_EXO";
                                exoSndEmailSetting.OldValue = "";
                                info.ModifyContent.Add(exoSndEmailSetting);

                                //Email Users
                                AuditItem exoEmailUsers = new AuditItem();
                                exoEmailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_EXO";
                                exoEmailUsers.OldValue = "";
                                info.ModifyContent.Add(exoEmailUsers);

                                //workflow
                                AuditItem exoProcessItem = new AuditItem();
                                exoProcessItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_EXO";
                                exoProcessItem.OldValue = "";
                                info.ModifyContent.Add(exoProcessItem);

                                //Export
                                AuditItem exoExport = new AuditItem();
                                exoExport.TargetSetting = "RM_JS_RDM_ExportAction_EXO";
                                exoExport.OldValue = "";
                                info.ModifyContent.Add(exoExport);
                            }

                            //PhySource
                            AuditItem phySource = new AuditItem();
                            phySource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedPhysicalSource";
                            phySource.OldValue = rule.IsPhySource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(phySource);

                            if (rule.IsPhySource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_PHY";
                                rule.PhysicalRule.RuleCretias.Add(rule.PhysicalRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.PhysicalRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_PHY";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.PhysicalRule);

                                info.ModifyContent.Add(actionItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_PHY";
                                manualItem.OldValue = rule.PhysicalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.PhysicalRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.PhysicalRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_PHY";
                                        processItem.OldValue = rule.PhysicalRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_PHY";
                                        emailUsers.OldValue = rule.PhysicalRule.Users != null ? string.Join("; ", rule.PhysicalRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_PHY";
                                    sendEmailSetting.OldValue = rule.PhysicalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_PHY";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_PHY";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_PHY";
                                    processItem.OldValue = "";
                                    info.ModifyContent.Add(processItem);
                                }
                                //AuditItem exportItem = new AuditItem();
                                //exportItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_ExportAction");
                                //exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.ExportInfo);
                                //info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_PHY";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_PHY";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_PHY";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_PHY";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_PHY";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_PHY";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);

                                //    AuditItem export = new AuditItem();
                                //    export.TargetSetting = I18NEntity.GetString("RM_JS_RDM_ExportAction");
                                //    export.OldValue = "";
                                //    info.ModifyContent.Add(export);
                            }

                            //FSSource
                            AuditItem fsSource = new AuditItem();
                            fsSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedFSSource";
                            fsSource.OldValue = rule.IsFSSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(fsSource);

                            if (rule.IsFSSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_FSO";
                                rule.FSRule.RuleCretias.Add(rule.FSRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.FSRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_FSO";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.FSRule);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_FSO";
                                manualItem.OldValue = rule.FSRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.FSRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.FSRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_FSO";
                                        processItem.OldValue = rule.FSRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_FSO";
                                        emailUsers.OldValue = rule.FSRule.Users != null ? string.Join("; ", rule.FSRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_FSO";
                                    sendEmailSetting.OldValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_FSO";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_FSO";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_FSO";
                                    processItem.OldValue = rule.FSRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }

                                AuditItem storageName = new AuditItem();
                                storageName.Id = new Guid(SOConstants.SPAuditId);
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_FSO";
                                storageName.OldValue = rule.FSRule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);
                                //AuditItem exportItem = new AuditItem();
                                //exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.FSRule.ExportInfo);
                                //info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_FSO";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_FSO";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_FSO";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_FSO";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_FSO";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_FSO";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);

                                //AuditItem export = new AuditItem();
                                //export.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //export.OldValue = "";
                                //info.ModifyContent.Add(export);
                            }

                            //AzureFileSource
                            AuditItem azureFileSource = new AuditItem();
                            azureFileSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedAzureFileSource";
                            azureFileSource.OldValue = rule.IsAzureFileSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(azureFileSource);

                            if (rule.IsAzureFileSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_AZF";
                                rule.AzureFileRule.RuleCretias.Add(rule.AzureFileRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.AzureFileRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_AZF";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.AzureFileRule);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_AZF";
                                manualItem.OldValue = rule.AzureFileRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.AzureFileRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.AzureFileRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_AZF";
                                        processItem.OldValue = rule.AzureFileRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_AZF";
                                        emailUsers.OldValue = rule.AzureFileRule.Users != null ? string.Join("; ", rule.AzureFileRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_AZF";
                                    sendEmailSetting.OldValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_AZF";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_AZF";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_AZF";
                                    processItem.OldValue = rule.AzureFileRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                //AuditItem exportItem = new AuditItem();
                                //exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.FSRule.ExportInfo);
                                //info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_AZF";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_AZF";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_AZF";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_AZF";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_AZF";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_AZF";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);

                                //AuditItem export = new AuditItem();
                                //export.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //export.OldValue = "";
                                //info.ModifyContent.Add(export);
                            }

                            //ConnectorSource
                            AuditItem connectorSource = new AuditItem();
                            connectorSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedConnectorSource";
                            connectorSource.OldValue = rule.IsConnectorSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(connectorSource);

                            if (rule.IsConnectorSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_CNT";
                                rule.ConnectorRule.RuleCretias.Add(rule.ConnectorRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.ConnectorRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_CNT";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.ConnectorRule);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_CNT";
                                manualItem.OldValue = rule.ConnectorRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.ConnectorRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.ConnectorRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_CNT";
                                        processItem.OldValue = rule.ConnectorRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_CNT";
                                        emailUsers.OldValue = rule.ConnectorRule.Users != null ? string.Join("; ", rule.ConnectorRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_CNT";
                                    sendEmailSetting.OldValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_CNT";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_CNT";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_CNT";
                                    processItem.OldValue = rule.ConnectorRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                //AuditItem exportItem = new AuditItem();
                                //exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.FSRule.ExportInfo);
                                //info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_CNT";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_CNT";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_CNT";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_CNT";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_CNT";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_CNT";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);

                                //AuditItem export = new AuditItem();
                                //export.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //export.OldValue = "";
                                //info.ModifyContent.Add(export);
                            }

                            //SPLocalSource
                            AuditItem spLocalSource = new AuditItem();
                            spLocalSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedSPLocalSource";
                            spLocalSource.OldValue = rule.IsSPLocalSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(spLocalSource);

                            if (rule.IsSPLocalSource)
                            {
                                var spLocalRule = rule.SPLocalRule;
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_SPL";
                                spLocalRule.RuleCretias.Add(spLocalRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", spLocalRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_SPL";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(spLocalRule);

                                info.ModifyContent.Add(actionItem);

                                //AuditItem declaredFileItem = new AuditItem();
                                //declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //declaredFileItem.OldValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(declaredFileItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_SPL";
                                manualItem.OldValue = spLocalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(spLocalRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_SPL";
                                        processItem.OldValue = spLocalRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_SPL";
                                        emailUsers.OldValue = spLocalRule.Users != null ? string.Join("; ", spLocalRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_SPL";
                                    sendEmailSetting.OldValue = spLocalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_SPL";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_SPL";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    //workflow
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_SPL";
                                    processItem.OldValue = spLocalRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                AuditItem exportItem = new AuditItem();
                                exportItem.TargetSetting = "RM_JS_RDM_ExportAction_SPL";
                                exportItem.OldValue = RuleAuditUtil.GetExportInfo(spLocalRule.ExportInfo);
                                info.ModifyContent.Add(exportItem);
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_SPL";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_SPL";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_SPL";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_SPL";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                //Email Users
                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_SPL";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                //workflow
                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_SPL";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);

                                AuditItem export = new AuditItem();
                                export.TargetSetting = "RM_JS_RDM_ExportAction_SPL";
                                export.OldValue = "";
                                info.ModifyContent.Add(export);
                            }

                            //BoxSource
                            AuditItem boxSource = new AuditItem();
                            boxSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedBoxSource";
                            boxSource.OldValue = rule.IsBoxSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(boxSource);

                            if (rule.IsBoxSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_BOX";
                                rule.BoxRule.RuleCretias.Add(rule.BoxRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.BoxRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_BOX";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.BoxRule);

                                info.ModifyContent.Add(actionItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_BOX";
                                manualItem.OldValue = rule.BoxRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.BoxRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.BoxRule.WorkflowId))
                                    {
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_BOX";
                                        processItem.OldValue = rule.BoxRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_BOX";
                                        emailUsers.OldValue = rule.BoxRule.Users != null ? string.Join("; ", rule.BoxRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_BOX";
                                    sendEmailSetting.OldValue = rule.BoxRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_BOX";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_BOX";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_BOX";
                                    processItem.OldValue = rule.BoxRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_BOX";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_BOX";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_BOX";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_BOX";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_BOX";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_BOX";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);
                            }

                            //GoogleDriveSource
                            AuditItem googleDriveSource = new AuditItem();
                            googleDriveSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedGoogleDriveSource";
                            googleDriveSource.OldValue = rule.IsGoogleDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(googleDriveSource);

                            if (rule.IsGoogleDriveSource)
                            {
                                AuditItem conditionItem = new AuditItem();
                                conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_GGD";
                                rule.GoogleDriveRule.RuleCretias.Add(rule.GoogleDriveRule.FilterCombineMode);
                                conditionItem.OldValue = string.Join("<br>", rule.GoogleDriveRule.RuleCretias);
                                info.ModifyContent.Add(conditionItem);

                                AuditItem actionItem = new AuditItem();
                                actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_GGD";
                                actionItem.OldValue = RuleAuditUtil.GetAuditorRuleActionString(rule.GoogleDriveRule);

                                info.ModifyContent.Add(actionItem);

                                AuditItem manualItem = new AuditItem();
                                manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_GGD";
                                manualItem.OldValue = rule.GoogleDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(manualItem);
                                if (rule.GoogleDriveRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.GoogleDriveRule.WorkflowId))
                                    {
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_GGD";
                                        processItem.OldValue = rule.GoogleDriveRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_GGD";
                                        emailUsers.OldValue = rule.GoogleDriveRule.Users != null ? string.Join("; ", rule.GoogleDriveRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_GGD";
                                    sendEmailSetting.OldValue = rule.GoogleDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                                else
                                {
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_GGD";
                                    sendEmailSetting.OldValue = "";
                                    info.ModifyContent.Add(sendEmailSetting);

                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_GGD";
                                    emailUsers.OldValue = "";
                                    info.ModifyContent.Add(emailUsers);

                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_GGD";
                                    processItem.OldValue = rule.GoogleDriveRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                AuditItem exportItem = new AuditItem();
                                exportItem.TargetSetting = "RM_JS_RDM_ExportAction_GGD";
                                exportItem.OldValue = RuleAuditUtil.GetExportInfo(rule.GoogleDriveRule.ExportInfo);
                                info.ModifyContent.Add(exportItem);

                                AuditItem storageName = new AuditItem();
                                storageName.Id = new Guid(SOConstants.GGAuditId);
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                                storageName.OldValue = rule.GoogleDriveRule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);

                                AuditItem moveArchiveTierType = new AuditItem();
                                moveArchiveTierType.Id = new Guid(SOConstants.GGAuditId);
                                moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                moveArchiveTierType.OldValue = rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving
                                    ? "RM_RDM_CreateRule_ArchivedTier"
                                    : rule.MoveToAnotherTierType switch
                                    {
                                        0 => "RM_RDM_CreateRule_DefaultTier",
                                        3 => "RM_RDM_CreateRule_ArchivedTier",
                                        4 => "RM_RDM_CreateRule_ColdTier",
                                        _ => "RM_RDM_CreateRule_DefaultTier"
                                    }; //0 default,3 archive,4 cold
                                if (!string.IsNullOrEmpty(rule.GoogleDriveRule.StoragePolicyId) &&
                                    !IsSystemStorage(rule.GoogleDriveRule.StoragePolicyId) && (rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving ||
                                        rule.GoogleDriveRule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }

                                bool isEnableRetention = rule.GoogleDriveRule.RetentionInfoList == null
                                    ? rule.GoogleDriveRule.RetentionInfo == null ? false : true
                                    : true;
                                bool newRuleIsEnableRetention = newRule?.GoogleDriveRule?.RetentionInfoList == null
                                    ? newRule?.GoogleDriveRule?.RetentionInfo == null ? false : true
                                    : true;
                                if (isEnableRetention || newRuleIsEnableRetention)
                                {
                                    BuildTemplateForRetentionInfoList(rule.GoogleDriveRule, newRule.GoogleDriveRule, info,
                                        new Guid(SOConstants.GGAuditId));
                                    if (rule.GoogleDriveRule.RetentionInfoList != null)
                                    {
                                        for (int processedCount = 0;
                                             processedCount < rule.GoogleDriveRule.RetentionInfoList.Count();
                                             processedCount++)
                                        {
                                            RetentionSettings infoList = rule.GoogleDriveRule.RetentionInfoList[processedCount];
                                            if (infoList.IsEnableRetention)
                                            {
                                                string auditString =
                                                    infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity
                                                        .KeepDateType.ModifiedTime
                                                        ? "RM_RDM_CreateRule_RemoveModified_Time"
                                                        : "RM_RDM_CreateRule_RemoveArchive_Time";
                                                AuditItem retentionTime = info.ModifyContent.Where(a =>
                                                    a.TargetSetting != null && a.Deep == processedCount &&
                                                    a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") &&
                                                    a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                                if (retentionTime != null)
                                                {
                                                    retentionTime.OldValue = auditString + " " +
                                                                             "RM_JS_RDM_CreateRule_DateOption_Older" +
                                                                             " " + infoList.KeepDateNumber + " " +
                                                                             infoList.KeepDateUnite switch
                                                                             {
                                                                                 TimeUnit.Day => I18NEntity.GetString(
                                                                                     "RM_JS_RDM_CreateRule_Unit_Days"),
                                                                                 TimeUnit.Week => I18NEntity.GetString(
                                                                                     "RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                                 TimeUnit.Month => I18NEntity.GetString(
                                                                                     "RM_JS_RDM_CreateRule_Unit_Months"),
                                                                                 TimeUnit.Year => I18NEntity.GetString(
                                                                                     "RM_JS_RDM_CreateRule_Unit_Years"),
                                                                                 _ => ""
                                                                             };
                                                }

                                                AuditItem retentionAction = info.ModifyContent.Where(a =>
                                                    a.TargetSetting != null && a.Deep == processedCount &&
                                                    a.TargetSetting.Equals("RM_AR_CP_GSS_OperateDataTitle") &&
                                                    a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                                if (retentionAction != null)
                                                {
                                                    retentionAction.OldValue = infoList.OperateDataType switch
                                                    {
                                                        (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                        (int)OperateDateTypeEnum.MarkTier =>
                                                            "RM_AR_CP_GSS_Retention_MarkDataTier" + " " +
                                                            infoList.TierType switch
                                                            {
                                                                (int)Storage.AccessTierType.Cold =>
                                                                    I18NEntity.GetString(
                                                                        "RM_JS_Rule_DetailValue_ColdTier"),
                                                                (int)Storage.AccessTierType.Archive => I18NEntity
                                                                    .GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                            },
                                                        _ => ""
                                                    };
                                                }

                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.FirstOrDefault(x =>
                                                        x.TargetSetting ==
                                                        "RM_AR_CP_GSS_Retention_SoftDelete" &&
                                                        x.Id.Equals(new Guid(SOConstants.GGAuditId)));
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            OldValue = rule.GoogleDriveRule.RetentionInfo.IsSoftDelete
                                                                ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                                    I18NEntity.GetString(
                                                                        "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                                    rule.GoogleDriveRule.RetentionInfo
                                                                        .SoftKeepDateNumber + " " +
                                                                    rule.GoogleDriveRule.RetentionInfo
                                                                            .SoftKeepDateUnite switch
                                                                        {
                                                                            TimeUnit.Day => I18NEntity.GetString(
                                                                                "RM_JS_RDM_CreateRule_Unit_Days"),
                                                                            TimeUnit.Week => I18NEntity.GetString(
                                                                                "RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                            TimeUnit.Month => I18NEntity.GetString(
                                                                                "RM_JS_RDM_CreateRule_Unit_Months"),
                                                                            TimeUnit.Year => I18NEntity.GetString(
                                                                                "RM_JS_RDM_CreateRule_Unit_Years"),
                                                                            _ => ""
                                                                        })
                                                                : "RM_JS_Common_No",
                                                            Id = new Guid(SOConstants.GGAuditId)
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.OldValue = rule.GoogleDriveRule.RetentionInfo
                                                            .IsSoftDelete
                                                            ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                                I18NEntity.GetString(
                                                                    "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                                rule.GoogleDriveRule.RetentionInfo.SoftKeepDateNumber +
                                                                " " +
                                                                rule.GoogleDriveRule.RetentionInfo
                                                                        .SoftKeepDateUnite switch
                                                                    {
                                                                        TimeUnit.Day => I18NEntity.GetString(
                                                                            "RM_JS_RDM_CreateRule_Unit_Days"),
                                                                        TimeUnit.Week => I18NEntity.GetString(
                                                                            "RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                        TimeUnit.Month => I18NEntity.GetString(
                                                                            "RM_JS_RDM_CreateRule_Unit_Months"),
                                                                        TimeUnit.Year => I18NEntity.GetString(
                                                                            "RM_JS_RDM_CreateRule_Unit_Years"),
                                                                        _ => ""
                                                                    })
                                                            : "RM_JS_Common_No";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (rule.GoogleDriveRule.RetentionInfo != null)
                                    {
                                        AuditItem retentionTime = new AuditItem()
                                        {
                                            Id = new Guid(SOConstants.GGAuditId),
                                            TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                            OldValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " +
                                                       rule.GoogleDriveRule.RetentionInfo.Condition switch
                                                       {
                                                           TimeFilterCondition.OlderThan =>
                                                               "RM_JS_RDM_CreateRule_DateOption_Older" + " " +
                                                               rule.GoogleDriveRule.RetentionInfo.KeepDateNumber + " " +
                                                               rule.GoogleDriveRule.RetentionInfo.KeepDateUnite switch
                                                               {
                                                                   TimeUnit.Day => I18NEntity.GetString(
                                                                       "RM_JS_RDM_CreateRule_Unit_Days"),
                                                                   TimeUnit.Week => I18NEntity.GetString(
                                                                       "RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                   TimeUnit.Month => I18NEntity.GetString(
                                                                       "RM_JS_RDM_CreateRule_Unit_Months"),
                                                                   TimeUnit.Year => I18NEntity.GetString(
                                                                       "RM_JS_RDM_CreateRule_Unit_Years"),
                                                                   _ => ""
                                                               },
                                                           TimeFilterCondition.Is =>
                                                               "RM_JS_RDM_CreateRule_DateOption_Before" + " " +
                                                               rule.GoogleDriveRule.RetentionInfo.Date,
                                                           _ => ""
                                                       }
                                        };
                                        info.ModifyContent.Add(retentionTime);
                                        if (rule.GoogleDriveRule.RetentionInfo.IsManualApproval)
                                        {
                                            if (rule.GoogleDriveRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                            {
                                                var workFlow =
                                                    ManualApprovalWorkflowManager.Get(rule.GoogleDriveRule.RetentionInfo.WorkflowId);
                                                AuditItem workFlowAudit = new AuditItem()
                                                {
                                                    Id = new Guid(SOConstants.GGAuditId),
                                                    TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                    OldValue = workFlow.Name
                                                };
                                                info.ModifyContent.Add(workFlowAudit);
                                            }
                                            else if (rule.GoogleDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                            {
                                                AuditItem recordOwnerAudit = new AuditItem()
                                                {
                                                    Id = new Guid(SOConstants.GGAuditId),
                                                    TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                    OldValue = string.Join(",",
                                                        rule.GoogleDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName)
                                                            .ToList())
                                                };
                                                info.ModifyContent.Add(recordOwnerAudit);
                                            }
                                        }

                                        AuditItem sendEmail = new AuditItem()
                                        {
                                            Id = new Guid(SOConstants.GGAuditId),
                                            TargetSetting = "RM_SPS_SendEMail",
                                            OldValue = rule.GoogleDriveRule.RetentionInfo.IsSendEamilToOwner
                                                ? "RM_JS_Common_Yes"
                                                : "RM_JS_Common_No"
                                        };
                                        info.ModifyContent.Add(sendEmail);
                                    }
                                } 
                            }
                            else
                            {
                                AuditItem criteria = new AuditItem();
                                criteria.TargetSetting = "RM_JS_RDM_DisposalCondition_GGD";
                                criteria.OldValue = "";
                                info.ModifyContent.Add(criteria);

                                AuditItem ruleAction = new AuditItem();
                                ruleAction.TargetSetting = "RM_JS_RDM_DisposalAction_GGD";
                                ruleAction.OldValue = "";
                                info.ModifyContent.Add(ruleAction);

                                AuditItem manualApprove = new AuditItem();
                                manualApprove.TargetSetting = "RM_JS_RDM_ManualApproval_GGD";
                                manualApprove.OldValue = "";
                                info.ModifyContent.Add(manualApprove);

                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_GGD";
                                sendEmailSetting.OldValue = "";
                                info.ModifyContent.Add(sendEmailSetting);

                                AuditItem emailUsers = new AuditItem();
                                emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_GGD";
                                emailUsers.OldValue = "";
                                info.ModifyContent.Add(emailUsers);

                                AuditItem processItem = new AuditItem();
                                processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_GGD";
                                processItem.OldValue = "";
                                info.ModifyContent.Add(processItem);
                            }
                        }


                        break;
                    case AuditAction.DeleteRule:
                        //IMStorageOptimizationService soService = DocAveServiceHelper.CreateServiceClient<IMStorageOptimizationService>();
                        using (new RA.Common.PerformanceScope(string.Format("manage.rule.delete.BeforAudit")))
                        {
                            List<string> ids = (List<string>)args[0];
                            var ruleNames = await mRuleManagerService.GetBaseRulesNameFromDBAsync(ids);
                            info.Object = String.Join(";", ruleNames);
                        }
                        break;
                    case AuditAction.CreateRuleContainer:
                    case AuditAction.EditRuleContainer:
                        RuleContainerDto ruleContainerDto = (RuleContainerDto)args[0];
                        info.Object = ruleContainerDto.Name;
                        info.ModifyContent = new List<AuditItem>();
                        if (action == (int)AuditAction.CreateRuleContainer)
                        {
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_RDM_RuleContainerName", NewValue = ruleContainerDto.Name });
                        }
                        else
                        {
                            var dbRuleContainer = RMRuleDao.GetRuleContainersById(ruleContainerDto.ContainerId);
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_RDM_RuleContainerName", NewValue = ruleContainerDto.Name, OldValue = dbRuleContainer.Name });
                        }
                        break;
                    case AuditAction.DeleteRuleContainer:
                        Guid ruleContainerId = (Guid)args[0];
                        var dbDeleteRuleContainer = RMRuleDao.GetRuleContainersById(ruleContainerId);
                        info.Object = dbDeleteRuleContainer.Name;
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return info;
        }

        private bool IsSystemStorage(string storagePolicyId)
        {
            if(!_storageIsSystemDic.ContainsKey(storagePolicyId))
            {
                var result = RuleAuditUtil.IsSystemStorage(storagePolicyId);
                _storageIsSystemDic[storagePolicyId] = result;
            }
            return _storageIsSystemDic[storagePolicyId];
        }

        private void BuildTemplateForRetentionInfoList(RMRuleInfos oldRule, RMRuleInfos newRule, RMAuditInfo info, Guid dataSource)
        {
            int retentionInfoListLength = oldRule?.RetentionInfoList == null ? 0 : oldRule.RetentionInfoList.Count();
            int newRetentionInfoListLength = newRule?.RetentionInfoList == null ? 0 : newRule.RetentionInfoList.Count();
            int maxRetentionInfoListLength = Math.Max(retentionInfoListLength, newRetentionInfoListLength);
            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", OldValue = retentionInfoListLength > 0 ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };

            for (int processedCount = 0; processedCount < maxRetentionInfoListLength; processedCount++)
            {
                RetentionSettings infoList = processedCount < retentionInfoListLength ? oldRule?.RetentionInfoList[processedCount] : null;
                RetentionSettings newInfoList = processedCount < newRetentionInfoListLength ? newRule?.RetentionInfoList[processedCount] : null;
                AuditItem retentionTime = new AuditItem()
                {
                    Id = dataSource,
                    TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                    Deep = processedCount
                };
                info.ModifyContent.Add(retentionTime);
                AuditItem retentionAction = new AuditItem()
                {
                    Id = dataSource,
                    TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                    Deep = processedCount
                };
                info.ModifyContent.Add(retentionAction);
                if (infoList?.OperateDataType == (int)OperateDateTypeEnum.Delete || newInfoList?.OperateDataType == (int)OperateDateTypeEnum.Delete)
                {
                    AuditItem removeStub = new AuditItem() 
                    { 
                        Id = dataSource, 
                        TargetSetting = "RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub",
                        Deep = processedCount
                    };
                    info.ModifyContent.Add(removeStub);
                    if (KeyValueService.IsEnableSoftDeleteSetting())
                    {
                        AuditItem softDelete = new AuditItem()
                        {
                            Id = dataSource,
                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                            Deep = processedCount
                        };
                        info.ModifyContent.Add(softDelete);
                    }
                }

            }
        }

    }
}
