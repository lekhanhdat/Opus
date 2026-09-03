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
using AvePoint.GCommon.Contract.StorageOptimization.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using RAManualApprovalCommon;
using Microsoft.Graph;
using AvePoint.RA.Contract.Common;
using Box.V2.Models;
using RATeams;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.RuleManagement.AuditHandler
{
    public class RuleManagerAfterAuditHandler : IAfterAuditHandler
    {
        private IRuleManagerService mRuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly Dictionary<string, bool> _storageIsSystemDic = [];

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            //IMStorageOptimizationService soService = DocAveServiceHelper.CreateServiceClient<IMStorageOptimizationService>();
            RMAuditInfo auditInfo = new RMAuditInfo();
            RMRuleInfos rule = null;
            RAReturnMessage returnMessage = null;
            var isNewLogicAccount = TenantService.IsNewOpusTenant();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            switch ((AuditAction)action)
            {
                case AuditAction.CreateRule:
                    rule = args[0] as RMRuleInfos;
                    var ruleContainer = RMRuleDao.GetRuleContainersById(rule.ContainerId);
                    rule.ContainerName = ruleContainer.Name;
                    info = new RMAuditInfo();
                    auditInfo.Object = rule != null ? rule.RuleName : string.Empty;
                    returnMessage = (RAReturnMessage)returnValue;
                    auditInfo.Status = returnMessage.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;


                    RMRuleInfos c_ruleInfo = null;
                    try
                    {
                        c_ruleInfo = await mRuleManagerService.LoadRuleAsync(((RMRuleInfos)args[0]).RuleId);

                        if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }

                        AuditItem c_descItem = new AuditItem();
                        c_descItem.TargetSetting = "RM_JS_RDM_Rule_Description";
                        c_descItem.NewValue = rule.Description;
                        info.ModifyContent.Add(c_descItem);

                        AuditItem c_ruleContainerItem = new AuditItem();
                        c_ruleContainerItem.TargetSetting = "RM_JS_Rule_Detail_RuleContainer";
                        c_ruleContainerItem.NewValue = rule.ContainerName;
                        info.ModifyContent.Add(c_ruleContainerItem);

                        AuditItem c_disposalClassItem = new AuditItem();
                        c_disposalClassItem.TargetSetting = "RM_RDM_CreateRule_DisposalClass_Title";
                        c_disposalClassItem.NewValue = rule.DisposalClass;
                        info.ModifyContent.Add(c_disposalClassItem);
                        //SPSource
                        AuditItem spSource = new AuditItem();
                        spSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedSPSource";
                        spSource.NewValue = rule.IsSpSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        info.ModifyContent.Add(spSource);

                        if (rule.IsSpSource)
                        {
                            AuditItem c_conditionItem = new AuditItem();
                            c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition";
                            c_ruleInfo.RuleCretias.Add(c_ruleInfo.FilterCombineMode);
                            c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.RuleCretias);
                            info.ModifyContent.Add(c_conditionItem);

                            AuditItem c_actionItem = new AuditItem();
                            c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction";
                            c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule, isNewLogicAccount: isNewLogicAccount, sourceFlag: RA.SharePoint.ArchiverCommon.SOSourceFlag.SharePoint);
                            info.ModifyContent.Add(c_actionItem);

                            //AuditItem c_declaredFileItem = new AuditItem();
                            //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                            //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                            //info.ModifyContent.Add(c_declaredFileItem);
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval";
                                c_manualItem.NewValue = rule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);
                            }
                            if (rule.EnableManualApproval)
                            {
                                if (!string.IsNullOrEmpty(rule.WorkflowId))
                                {
                                    //workflow 
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName";
                                    processItem.NewValue = c_ruleInfo.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                else {
                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner";
                                    emailUsers.NewValue = rule.Users != null ? string.Join("; ", rule.Users.Select(u => u.DisplayName)) : "";
                                    info.ModifyContent.Add(emailUsers);
                                }
                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner";
                                sendEmailSetting.NewValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(sendEmailSetting);
                            }

                            AuditItem c_exportItem = new AuditItem();
                            c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction";
                            var soExportInfo = rule.EnableExport && rule.ExportInfo != null ? rule.ExportInfo : null;
                            c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                            info.ModifyContent.Add(c_exportItem);

                            if (NeedShowStoragePolicy(rule))
                            {
                                AuditItem storageName = new AuditItem();
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                                storageName.NewValue = rule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);
                            }
                            AuditItem moveArchiveTierType = new AuditItem();
                            moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                            moveArchiveTierType.NewValue = rule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.MoveToAnotherTierType switch
                            {
                                0 => "RM_RDM_CreateRule_DefaultTier",
                                3 => "RM_RDM_CreateRule_ArchivedTier",
                                4 => "RM_RDM_CreateRule_ColdTier",
                                _ => "RM_RDM_CreateRule_DefaultTier"
                            };//0 default,3 archive,4 cold
                            if (!string.IsNullOrWhiteSpace(rule.StoragePolicyId) && !IsSystemStorage(rule.StoragePolicyId) && (rule.MoveToArchiverTierWhenArchiving || rule.MoveToAnotherTierType != null))
                            {
                                info.ModifyContent.Add(moveArchiveTierType);
                            }
                            bool isEnableRetention = rule.RetentionInfoList == null ? rule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.RetentionInfoList != null)
                                {
                                    foreach (var infoList in rule.RetentionInfoList)
                                    {
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = new AuditItem()
                                            {
                                                TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                                NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                },
                                            };
                                            info.ModifyContent.Add(retentionTime);
                                            AuditItem retentionAction = new AuditItem()
                                            {
                                                TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                                                NewValue = infoList.OperateDataType switch
                                                {
                                                    (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                    (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                    {
                                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                    },
                                                    _ => ""
                                                }
                                            };
                                            info.ModifyContent.Add(retentionAction);
                                            if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                            {
                                                AuditItem removeStub = new AuditItem() { TargetSetting = "RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub", NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                                                info.ModifyContent.Add(removeStub);
                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x=>x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete").FirstOrDefault();
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No"
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                        TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                        NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.RetentionInfo.Condition switch
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
                                                TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                NewValue = workFlow.Name
                                            };
                                            info.ModifyContent.Add(workFlowAudit);
                                        }
                                        else if (rule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = new AuditItem()
                                            {
                                                TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                NewValue = string.Join(",", rule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                            };
                                            info.ModifyContent.Add(recordOwnerAudit);
                                        };
                                    }
                                    AuditItem sendEmail = new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_SendEMail",
                                        NewValue = rule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(sendEmail);
                                }
                            }
                        }
                        

                        //OneDriveSource
                        AuditItem oneDriveSource = new AuditItem
                        {
                            TargetSetting = "RM_JS_RDM_Rule_IsCheckedOneDriveSource",
                            NewValue = rule.IsOneDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                        };
                        info.ModifyContent.Add(oneDriveSource);

                        if (rule.IsOneDriveSource)
                        {
                            c_ruleInfo.OneDriveRule.RuleCretias.Add(c_ruleInfo.OneDriveRule.FilterCombineMode);
                            AuditItem c_conditionItem = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalCondition_ONE",
                                NewValue = string.Join("<br>", c_ruleInfo.OneDriveRule.RuleCretias)
                            };
                            info.ModifyContent.Add(c_conditionItem);

                            AuditItem c_actionItem = new AuditItem
                            {
                                TargetSetting = "RM_JS_RDM_DisposalAction_ONE",
                                NewValue = RuleAuditUtil.GetAuditorRuleActionString(c_ruleInfo.OneDriveRule, c_ruleInfo.ModelType, RA.SharePoint.ArchiverCommon.SOSourceFlag.OneDrive, isNewLogicAccount)
                            };
                            info.ModifyContent.Add(c_actionItem);
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem c_manualItem = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_ManualApproval_ONE",
                                    NewValue = rule.OneDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                };
                                info.ModifyContent.Add(c_manualItem);
                            }

                            if (rule.OneDriveRule.EnableManualApproval)
                            {
                                if (!string.IsNullOrEmpty(rule.OneDriveRule.WorkflowId))
                                {
                                    //workflow 
                                    AuditItem processItem = new AuditItem
                                    {
                                        TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_ONE",
                                        NewValue = c_ruleInfo.OneDriveRule.WorkflowName
                                    };
                                    info.ModifyContent.Add(processItem);
                                }
                                else
                                {
                                    //Email Users
                                    AuditItem emailUsers = new AuditItem
                                    {
                                        TargetSetting = "RM_JS_MA_Grid_RecordOwner_ONE",
                                        NewValue = rule.OneDriveRule.Users != null ? string.Join("; ", rule.OneDriveRule.Users.Select(u => u.DisplayName)) : ""
                                    };
                                    info.ModifyContent.Add(emailUsers);
                                }
                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem
                                {
                                    TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_ONE",
                                    NewValue = rule.OneDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                };
                                info.ModifyContent.Add(sendEmailSetting);
                            }

                            if(rule.OneDriveRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection)
                            {
                                AuditItem c_exportItem = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_ExportAction_ONE"
                                };
                                var soExportInfo = rule.OneDriveRule.EnableExport && rule.OneDriveRule.ExportInfo != null ? rule.OneDriveRule.ExportInfo : null;
                                c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                info.ModifyContent.Add(c_exportItem);
                            }

                            if (NeedShowStoragePolicy(rule.OneDriveRule))
                            {
                                AuditItem storageName = new AuditItem();
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_ONE";
                                storageName.NewValue = rule.OneDriveRule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);
                            }
                            AuditItem moveArchiveTierType = new AuditItem();
                            moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                            moveArchiveTierType.NewValue = rule.OneDriveRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.OneDriveRule.MoveToAnotherTierType switch
                            {
                                0 => "RM_RDM_CreateRule_DefaultTier",
                                3 => "RM_RDM_CreateRule_ArchivedTier",
                                4 => "RM_RDM_CreateRule_ColdTier",
                                _ => "RM_RDM_CreateRule_DefaultTier"
                            };//0 default,3 archive,4 cold
                            if (!string.IsNullOrWhiteSpace(rule.OneDriveRule.StoragePolicyId) && !IsSystemStorage(rule.OneDriveRule.StoragePolicyId) && (rule.OneDriveRule.MoveToArchiverTierWhenArchiving || rule.OneDriveRule.MoveToAnotherTierType != null))
                            {
                                info.ModifyContent.Add(moveArchiveTierType);
                            }

                            bool isEnableRetention = rule.OneDriveRule.RetentionInfoList == null ? rule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.OneDriveRule.RetentionInfoList != null)
                                {
                                    foreach (var infoList in rule.OneDriveRule.RetentionInfoList)
                                    {
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = new AuditItem()
                                            {
                                                TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                                NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                },
                                            };
                                            info.ModifyContent.Add(retentionTime);
                                            AuditItem retentionAction = new AuditItem()
                                            {
                                                TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                                                NewValue = infoList.OperateDataType switch
                                                {
                                                    (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                    (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                    {
                                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                    },
                                                    _ => ""
                                                }
                                            };
                                            info.ModifyContent.Add(retentionAction);
                                            if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                            {
                                                AuditItem removeStub = new AuditItem() { TargetSetting = "RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub", NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                                                info.ModifyContent.Add(removeStub);
                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete").FirstOrDefault();
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No"
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                else if (rule.OneDriveRule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = new AuditItem()
                                    {
                                        TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                        NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.OneDriveRule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + rule.OneDriveRule.RetentionInfo.KeepDateNumber + " " + rule.OneDriveRule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + rule.OneDriveRule.RetentionInfo.Date,
                                            _ => ""
                                        }
                                    };
                                    info.ModifyContent.Add(retentionTime);
                                    if (rule.OneDriveRule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.OneDriveRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.OneDriveRule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = new AuditItem()
                                            {
                                                TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                NewValue = workFlow.Name
                                            };
                                            info.ModifyContent.Add(workFlowAudit);
                                        }
                                        else if (rule.OneDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = new AuditItem()
                                            {
                                                TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                NewValue = string.Join(",", rule.OneDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                            };
                                            info.ModifyContent.Add(recordOwnerAudit);
                                        };
                                    }
                                    AuditItem sendEmail = new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_SendEMail",
                                        NewValue = rule.OneDriveRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(sendEmail);
                                }
                            }
                        }

                        //Teams
                        if(rule.ModelType == RuleModel.SOArchiver && rule.RuleLevel == GCommon.Contract.CommonFilter.PolicyLevel.Teams)
                        {
                            if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
                            {
                                AuditItem teamsSource = new AuditItem();
                                teamsSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedTeamsSource";
                                teamsSource.NewValue = rule.IsTeamsSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(teamsSource);
                            }
                        if (rule.IsTeamsSource)
                        {
                            AuditItem c_conditionItem = new AuditItem();
                            c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_TMS";
                            c_ruleInfo.TeamsRule.RuleCretias.Add(c_ruleInfo.TeamsRule.FilterCombineMode);
                            c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.TeamsRule.RuleCretias);
                            info.ModifyContent.Add(c_conditionItem);

                            AuditItem c_actionItem = new AuditItem();
                            c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_TMS";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.TeamsRule, rule.ModelType);
                            info.ModifyContent.Add(c_actionItem);

                            //AuditItem c_declaredFileItem = new AuditItem();
                            //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                            //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                            //info.ModifyContent.Add(c_declaredFileItem);
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_TMS";
                                c_manualItem.NewValue = rule.TeamsRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);
                            }
                            if (rule.TeamsRule.EnableManualApproval)
                            {
                                if (!string.IsNullOrEmpty(rule.TeamsRule.WorkflowId))
                                {
                                    //workflow 
                                    AuditItem processItem = new AuditItem();
                                    processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_TMS";
                                    processItem.NewValue = c_ruleInfo.TeamsRule.WorkflowName;
                                    info.ModifyContent.Add(processItem);
                                }
                                else
                                {
                                    //Email Users
                                    AuditItem emailUsers = new AuditItem();
                                    emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_TMS";
                                    emailUsers.NewValue = rule.TeamsRule.Users != null ? string.Join("; ", rule.TeamsRule.Users.Select(u => u.DisplayName)) : "";
                                    info.ModifyContent.Add(emailUsers);
                                }
                                //Send Email Setting
                                AuditItem sendEmailSetting = new AuditItem();
                                sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_TMS";
                                sendEmailSetting.NewValue = rule.TeamsRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(sendEmailSetting);
                            }

                            if(rule.TeamsRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.Teams)
                            {
                                AuditItem c_exportItem = new AuditItem();
                                c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_TMS";
                                var soExportInfo = rule.TeamsRule.EnableExport && rule.TeamsRule.ExportInfo != null ? rule.TeamsRule.ExportInfo : null;
                                c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                info.ModifyContent.Add(c_exportItem);
                            }

                            if (NeedShowStoragePolicy(rule.TeamsRule, rule.ModelType))
                            {
                                AuditItem storageName = new AuditItem();
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_TMS";
                                storageName.NewValue = rule.TeamsRule.StoragePolicyName;
                                info.ModifyContent.Add(storageName);
                            }
                            AuditItem moveArchiveTierType = new AuditItem();
                            moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                            moveArchiveTierType.NewValue = rule.TeamsRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.TeamsRule.MoveToAnotherTierType switch
                            {
                                0 => "RM_RDM_CreateRule_DefaultTier",
                                3 => "RM_RDM_CreateRule_ArchivedTier",
                                4 => "RM_RDM_CreateRule_ColdTier",
                                _ => "RM_RDM_CreateRule_DefaultTier"
                            };//0 default,3 archive,4 cold
                            if (!IsSystemStorage(rule.TeamsRule.StoragePolicyId) && (rule.TeamsRule.MoveToArchiverTierWhenArchiving || rule.TeamsRule.MoveToAnotherTierType != null))
                            {
                                info.ModifyContent.Add(moveArchiveTierType);
                            }
                            bool isEnableRetention = rule.TeamsRule.RetentionInfoList == null ? rule.TeamsRule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention_TMS", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.TeamsRule.RetentionInfoList != null)
                                {
                                    foreach (var infoList in rule.TeamsRule.RetentionInfoList)
                                    {
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = new AuditItem()
                                            {
                                                TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                                NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
                                                {
                                                    TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                    TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                },
                                            };
                                            info.ModifyContent.Add(retentionTime);
                                            AuditItem retentionAction = new AuditItem()
                                            {
                                                TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                                                NewValue = infoList.OperateDataType switch
                                                {
                                                    (int)OperateDateTypeEnum.Delete => "Gui.Common_Delete the data",
                                                    (int)OperateDateTypeEnum.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + infoList.TierType switch
                                                    {
                                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                                    },
                                                    _ => ""
                                                }
                                            };
                                            info.ModifyContent.Add(retentionAction);
                                            if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                            {
                                                AuditItem removeStub = new AuditItem() { TargetSetting = "RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub", NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                                                info.ModifyContent.Add(removeStub);
                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete").FirstOrDefault();
                                                    if (tempAuditItem == null)
                                                    {
                                                        AuditItem softDelete = new AuditItem()
                                                        {
                                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                            NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
                                                            {
                                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                                _ => ""
                                                            }) : "RM_JS_Common_No"
                                                        };
                                                        info.ModifyContent.Add(softDelete);
                                                    }
                                                    else
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                else if (rule.TeamsRule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = new AuditItem()
                                    {
                                        TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                        NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.TeamsRule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + rule.TeamsRule.RetentionInfo.KeepDateNumber + " " + rule.TeamsRule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + rule.TeamsRule.RetentionInfo.Date,
                                            _ => ""
                                        }
                                    };
                                    info.ModifyContent.Add(retentionTime);
                                    if (rule.TeamsRule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.TeamsRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.TeamsRule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = new AuditItem()
                                            {
                                                TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                NewValue = workFlow.Name
                                            };
                                            info.ModifyContent.Add(workFlowAudit);
                                        }
                                        else if (rule.TeamsRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = new AuditItem()
                                            {
                                                TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                NewValue = string.Join(",", rule.TeamsRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList())
                                            };
                                            info.ModifyContent.Add(recordOwnerAudit);
                                        };
                                    }
                                    AuditItem sendEmail = new AuditItem()
                                    {
                                        TargetSetting = "RM_SPS_SendEMail",
                                        NewValue = rule.TeamsRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(sendEmail);
                                }
                            }
                        }
                        }

                        if (rule.ModelType != RuleModel.SOArchiver)
                        {
                            AuditItem exoSource = new AuditItem();
                            exoSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedEXOSource";
                            exoSource.NewValue = rule.IsExoSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(exoSource);

                            if (rule.IsExoSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_EXO";
                                c_ruleInfo.EXORule.RuleCretias.Add(c_ruleInfo.EXORule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.EXORule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_EXO";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.EXORule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_EXO";
                                c_manualItem.NewValue = rule.EXORule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.EXORule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.EXORule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_EXO";
                                        processItem.NewValue = c_ruleInfo.EXORule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_EXO";
                                        emailUsers.NewValue = rule.EXORule.Users != null ? string.Join("; ", rule.EXORule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_EXO";
                                    sendEmailSetting.NewValue = rule.EXORule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                AuditItem c_exportItem = new AuditItem();
                                c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_EXO";
                                var soExportInfo = rule.EXORule.EnableExport && rule.EXORule.ExportInfo != null ? rule.EXORule.ExportInfo : null;
                                c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                info.ModifyContent.Add(c_exportItem);

                            }

                            //PhySource
                            AuditItem phySource = new AuditItem();
                            phySource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedPhysicalSource";
                            phySource.NewValue = rule.IsPhySource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(phySource);

                            if (rule.IsPhySource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_PHY";
                                c_ruleInfo.PhysicalRule.RuleCretias.Add(c_ruleInfo.PhysicalRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.PhysicalRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_PHY";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.PhysicalRule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_PHY";
                                c_manualItem.NewValue = rule.PhysicalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.PhysicalRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.PhysicalRule.WorkflowId))
                                    {
                                        //workflow
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_PHY";
                                        processItem.NewValue = c_ruleInfo.PhysicalRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_PHY";
                                        emailUsers.NewValue = rule.PhysicalRule.Users != null ? string.Join("; ", rule.PhysicalRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_PHY";
                                    sendEmailSetting.NewValue = rule.PhysicalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                //AuditItem c_exportItem = new AuditItem();
                                //c_exportItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_ExportAction");
                                //var soExportInfo = rule.EnableExport && rule.ExportInfo != null ? rule.ExportInfo : null;
                                //c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //info.ModifyContent.Add(c_exportItem);
                            }

                            //File System
                            //SPSource //to do I18N Gary
                            AuditItem fsSource = new AuditItem();
                            fsSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedFSSource";
                            fsSource.NewValue = rule.IsFSSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(fsSource);

                            if (rule.IsFSSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_FSO";
                                c_ruleInfo.FSRule.RuleCretias.Add(c_ruleInfo.FSRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.FSRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_FSO";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.FSRule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_FSO";
                                c_manualItem.NewValue = rule.FSRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.FSRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.FSRule.WorkflowId))
                                    {
                                        //workflow 
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_FSO";
                                        processItem.NewValue = c_ruleInfo.FSRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_FSO";
                                        emailUsers.NewValue = rule.FSRule.Users != null ? string.Join("; ", rule.FSRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_FSO";
                                    sendEmailSetting.NewValue = rule.FSRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                if (NeedShowStoragePolicy(rule))
                                {
                                    AuditItem storageName = new AuditItem();
                                    storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_FSO";
                                    storageName.NewValue = rule.FSRule.StoragePolicyName;
                                    info.ModifyContent.Add(storageName);
                                }

                                //AuditItem c_exportItem = new AuditItem();
                                //c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //info.ModifyContent.Add(c_exportItem);
                            }

                            AuditItem azureFileSource = new AuditItem();
                            azureFileSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedAzureFileSource";
                            azureFileSource.NewValue = rule.IsAzureFileSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(azureFileSource);

                            if (rule.IsAzureFileSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_AZF";
                                c_ruleInfo.AzureFileRule.RuleCretias.Add(c_ruleInfo.AzureFileRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.AzureFileRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_AZF";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.AzureFileRule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_AZF";
                                c_manualItem.NewValue = rule.AzureFileRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.AzureFileRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.AzureFileRule.WorkflowId))
                                    {
                                        //workflow 
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_AZF";
                                        processItem.NewValue = c_ruleInfo.AzureFileRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_AZF";
                                        emailUsers.NewValue = rule.AzureFileRule.Users != null ? string.Join("; ", rule.AzureFileRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_AZF";
                                    sendEmailSetting.NewValue = rule.AzureFileRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                //AuditItem c_exportItem = new AuditItem();
                                //c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //info.ModifyContent.Add(c_exportItem);
                            }
                            //RM_JS_RDM_Rule_IsCheckedConnectorSource

                            AuditItem connectorSource = new AuditItem();
                            connectorSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedConnectorSource";
                            connectorSource.NewValue = rule.IsConnectorSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(connectorSource);
                            if (rule.IsConnectorSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_CNT";
                                c_ruleInfo.ConnectorRule.RuleCretias.Add(c_ruleInfo.ConnectorRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.ConnectorRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_CNT";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.ConnectorRule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_CNT";
                                c_manualItem.NewValue = rule.ConnectorRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.ConnectorRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.ConnectorRule.WorkflowId))
                                    {
                                        //workflow 
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_CNT";
                                        processItem.NewValue = c_ruleInfo.ConnectorRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_CNT";
                                        emailUsers.NewValue = rule.ConnectorRule.Users != null ? string.Join("; ", rule.ConnectorRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_CNT";
                                    sendEmailSetting.NewValue = rule.ConnectorRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                //AuditItem c_exportItem = new AuditItem();
                                //c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_FSO";
                                //var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //info.ModifyContent.Add(c_exportItem);
                            }

                            //SPLocalSource
                            AuditItem spLocalSource = new AuditItem();
                            spLocalSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedSPLocalSource";
                            spLocalSource.NewValue = rule.IsSPLocalSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(spLocalSource);

                            if (rule.IsSPLocalSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_SPL";
                                c_ruleInfo.SPLocalRule.RuleCretias.Add(c_ruleInfo.SPLocalRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.SPLocalRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.SPLocalRule);
                                info.ModifyContent.Add(c_actionItem);

                                //AuditItem c_declaredFileItem = new AuditItem();
                                //c_declaredFileItem.TargetSetting = I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile");
                                //c_declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                //info.ModifyContent.Add(c_declaredFileItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_SPL";
                                c_manualItem.NewValue = rule.SPLocalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.SPLocalRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.SPLocalRule.WorkflowId))
                                    {
                                        //workflow 
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_SPL";
                                        processItem.NewValue = c_ruleInfo.SPLocalRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_SPL";
                                        emailUsers.NewValue = rule.SPLocalRule.Users != null ? string.Join("; ", rule.SPLocalRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    //Send Email Setting
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_SPL";
                                    sendEmailSetting.NewValue = rule.SPLocalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                AuditItem c_exportItem = new AuditItem();
                                c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_SPL";
                                var soExportInfo = rule.SPLocalRule.EnableExport && rule.SPLocalRule.ExportInfo != null ? rule.SPLocalRule.ExportInfo : null;
                                c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                info.ModifyContent.Add(c_exportItem);

                            }

                            //Box
                            AuditItem boxSource = new AuditItem();
                            boxSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedBoxSource";
                            boxSource.NewValue = rule.IsBoxSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(boxSource);

                            if (rule.IsBoxSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_BOX";
                                c_ruleInfo.BoxRule.RuleCretias.Add(c_ruleInfo.BoxRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.BoxRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_BOX";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.BoxRule);
                                info.ModifyContent.Add(c_actionItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_BOX";
                                c_manualItem.NewValue = rule.BoxRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.BoxRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.BoxRule.WorkflowId))
                                    {
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_BOX";
                                        processItem.NewValue = c_ruleInfo.BoxRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_BOX";
                                        emailUsers.NewValue = rule.BoxRule.Users != null ? string.Join("; ", rule.BoxRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_BOX";
                                    sendEmailSetting.NewValue = rule.BoxRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }
                            }

                            //Google Drive
                            AuditItem googleDriveSource = new AuditItem();
                            googleDriveSource.TargetSetting = "RM_JS_RDM_Rule_IsCheckedGoogleDriveSource";
                            googleDriveSource.NewValue = rule.IsGoogleDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(googleDriveSource);

                            if (rule.IsGoogleDriveSource)
                            {
                                AuditItem c_conditionItem = new AuditItem();
                                c_conditionItem.TargetSetting = "RM_JS_RDM_DisposalCondition_GGD";
                                c_ruleInfo.GoogleDriveRule.RuleCretias.Add(c_ruleInfo.GoogleDriveRule.FilterCombineMode);
                                c_conditionItem.NewValue = string.Join("<br>", c_ruleInfo.GoogleDriveRule.RuleCretias);
                                info.ModifyContent.Add(c_conditionItem);

                                AuditItem c_actionItem = new AuditItem();
                                c_actionItem.TargetSetting = "RM_JS_RDM_DisposalAction_GGD";
                                c_actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.GoogleDriveRule);
                                info.ModifyContent.Add(c_actionItem);

                                AuditItem c_manualItem = new AuditItem();
                                c_manualItem.TargetSetting = "RM_JS_RDM_ManualApproval_GGD";
                                c_manualItem.NewValue = rule.GoogleDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                info.ModifyContent.Add(c_manualItem);

                                if (rule.GoogleDriveRule.EnableManualApproval)
                                {
                                    if (!string.IsNullOrEmpty(rule.GoogleDriveRule.WorkflowId))
                                    {
                                        AuditItem processItem = new AuditItem();
                                        processItem.TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName_GGD";
                                        processItem.NewValue = c_ruleInfo.GoogleDriveRule.WorkflowName;
                                        info.ModifyContent.Add(processItem);
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = new AuditItem();
                                        emailUsers.TargetSetting = "RM_JS_MA_Grid_RecordOwner_GGD";
                                        emailUsers.NewValue = rule.GoogleDriveRule.Users != null ? string.Join("; ", rule.GoogleDriveRule.Users.Select(u => u.DisplayName)) : "";
                                        info.ModifyContent.Add(emailUsers);
                                    }
                                    AuditItem sendEmailSetting = new AuditItem();
                                    sendEmailSetting.TargetSetting = "RM_JS_MA_Grid_SendEmailRecordOwner_GGD";
                                    sendEmailSetting.NewValue = rule.GoogleDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    info.ModifyContent.Add(sendEmailSetting);
                                }

                                AuditItem c_exportItem = new AuditItem();
                                c_exportItem.TargetSetting = "RM_JS_RDM_ExportAction_GGD";
                                var googleExportInfo = rule.GoogleDriveRule.EnableExport && rule.GoogleDriveRule.ExportInfo != null ? rule.GoogleDriveRule.ExportInfo : null;
                                c_exportItem.NewValue = RuleAuditUtil.GetExportInfo(googleExportInfo);
                                info.ModifyContent.Add(c_exportItem);
                                if (NeedShowStoragePolicy(rule.GoogleDriveRule))
                                {
                                    AuditItem storageName = new AuditItem();
                                    storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                                    storageName.NewValue = rule.GoogleDriveRule.StoragePolicyName;
                                    info.ModifyContent.Add(storageName);
                                }

                                AuditItem moveArchiveTierType = new AuditItem
                                {
                                    TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle",
                                    NewValue = rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving
                                        ? "RM_RDM_CreateRule_ArchivedTier"
                                        : rule.GoogleDriveRule.MoveToAnotherTierType switch
                                        {
                                            0 => "RM_RDM_CreateRule_DefaultTier",
                                            3 => "RM_RDM_CreateRule_ArchivedTier",
                                            4 => "RM_RDM_CreateRule_ColdTier",
                                            _ => "RM_RDM_CreateRule_DefaultTier"
                                        } //0 default,3 archive,4 cold
                                };
                                if (!string.IsNullOrWhiteSpace(rule.GoogleDriveRule.StoragePolicyId) &&
                                    !IsSystemStorage(rule.GoogleDriveRule.StoragePolicyId) && (rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving ||
                                        rule.GoogleDriveRule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }

                                bool isEnableRetention = rule.GoogleDriveRule.RetentionInfoList == null
                                    ? rule.GoogleDriveRule.RetentionInfo == null ? false : true
                                    : true;
                                AuditItem enableRetention = new AuditItem()
                                {
                                    TargetSetting = "RM_JS_Rule_Detail_Retention",
                                    NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                };
                                if (isEnableRetention)
                                {
                                    if (rule.GoogleDriveRule.RetentionInfoList != null)
                                    {
                                        foreach (var infoList in rule.GoogleDriveRule.RetentionInfoList)
                                        {
                                            if (infoList.IsEnableRetention)
                                            {
                                                string auditString =
                                                    infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity
                                                        .KeepDateType.ModifiedTime
                                                        ? "RM_RDM_CreateRule_RemoveModified_Time"
                                                        : "RM_RDM_CreateRule_RemoveArchive_Time";
                                                AuditItem retentionTime = new AuditItem()
                                                {
                                                    TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                                    NewValue = auditString + " " +
                                                               "RM_JS_RDM_CreateRule_DateOption_Older" + " " +
                                                               infoList.KeepDateNumber + " " +
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
                                                               },
                                                };
                                                info.ModifyContent.Add(retentionTime);
                                                AuditItem retentionAction = new AuditItem()
                                                {
                                                    TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                                                    NewValue = infoList.OperateDataType switch
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
                                                    }
                                                };
                                                info.ModifyContent.Add(retentionAction);
                                                if (infoList.OperateDataType == (int)OperateDateTypeEnum.Delete)
                                                {
                                                    AuditItem removeStub = new AuditItem()
                                                    {
                                                        TargetSetting =
                                                            "RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub",
                                                        NewValue = infoList.RemoveOrphanedStub
                                                            ? "RM_JS_Common_Yes"
                                                            : "RM_JS_Common_No"
                                                    };
                                                    info.ModifyContent.Add(removeStub);
                                                    if (KeyValueService.IsEnableSoftDeleteSetting())
                                                    {
                                                        var tempAuditItem = info.ModifyContent.Where(x =>
                                                                x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete")
                                                            .FirstOrDefault();
                                                        if (tempAuditItem == null)
                                                        {
                                                            AuditItem softDelete = new AuditItem()
                                                            {
                                                                TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                                NewValue = infoList.IsSoftDelete
                                                                    ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                                        I18NEntity.GetString(
                                                                            "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                                        infoList.SoftKeepDateNumber + " " +
                                                                        infoList.SoftKeepDateUnite switch
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
                                                                    : "RM_JS_Common_No"
                                                            };
                                                            info.ModifyContent.Add(softDelete);
                                                        }
                                                        else
                                                        {
                                                            tempAuditItem.NewValue = infoList.IsSoftDelete
                                                                ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                                    I18NEntity.GetString(
                                                                        "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                                    infoList.SoftKeepDateNumber + " " +
                                                                    infoList.SoftKeepDateUnite switch
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
                                    }
                                    else if (rule.GoogleDriveRule.RetentionInfo != null)
                                    {
                                        AuditItem retentionTime = new AuditItem()
                                        {
                                            TargetSetting = "RM_RDM_CreateRule_RemoveArchive_Prefix",
                                            NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " +
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
                                                    ManualApprovalWorkflowManager.Get(rule.RetentionInfo.WorkflowId);
                                                AuditItem workFlowAudit = new AuditItem()
                                                {
                                                    TargetSetting = "RM_RDM_CreateRule_Title_SelectProcess",
                                                    NewValue = workFlow.Name
                                                };
                                                info.ModifyContent.Add(workFlowAudit);
                                            }
                                            else if (rule.GoogleDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                            {
                                                AuditItem recordOwnerAudit = new AuditItem()
                                                {
                                                    TargetSetting = "RM_SPS_MAChooseUsersTip",
                                                    NewValue = string.Join(",",
                                                        rule.GoogleDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName)
                                                            .ToList())
                                                };
                                                info.ModifyContent.Add(recordOwnerAudit);
                                            }

                                            ;
                                        }

                                        if (KeyValueService.IsEnableSoftDeleteSetting())
                                        {
                                            var tempAuditItem = info.ModifyContent.FirstOrDefault(x =>
                                                    x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete");
                                            if (tempAuditItem == null)
                                            {
                                                AuditItem softDelete = new AuditItem()
                                                {
                                                    TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                                    NewValue = rule.GoogleDriveRule.RetentionInfo.IsSoftDelete
                                                        ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                            I18NEntity.GetString(
                                                                "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                            rule.GoogleDriveRule.RetentionInfo.SoftKeepDateNumber + " " +
                                                            rule.GoogleDriveRule.RetentionInfo.SoftKeepDateUnite switch
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
                                                        : "RM_JS_Common_No"
                                                };
                                                info.ModifyContent.Add(softDelete);
                                            }
                                            else
                                            {
                                                tempAuditItem.NewValue = rule.GoogleDriveRule.RetentionInfo.IsSoftDelete
                                                    ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                        I18NEntity.GetString(
                                                            "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                        rule.GoogleDriveRule.RetentionInfo.SoftKeepDateNumber + " " +
                                                        rule.GoogleDriveRule.RetentionInfo.SoftKeepDateUnite switch
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

                                        AuditItem sendEmail = new AuditItem()
                                        {
                                            TargetSetting = "RM_SPS_SendEMail",
                                            NewValue = rule.GoogleDriveRule.RetentionInfo.IsSendEamilToOwner
                                                ? "RM_JS_Common_Yes"
                                                : "RM_JS_Common_No"
                                        };
                                        info.ModifyContent.Add(sendEmail);
                                    }
                                }
                            }
                        }


                        ResetTargetSettings(info);
                    }
                    catch (Exception)
                    {
                        //not need to add log
                    }
                    break;
                case AuditAction.EditRule:
                    rule = args[0] as RMRuleInfos;
                    var editRule = RMRuleDao.GetRuleContainersById(rule.ContainerId);
                    rule.ContainerName = editRule.Name;
                    RMRuleInfos ruleInfo = await mRuleManagerService.LoadRuleAsync(((RMRuleInfos)args[0]).RuleId);
                    auditInfo.Object = rule != null ? rule.RuleName : string.Empty;
                    returnMessage = (RAReturnMessage)returnValue;
                    auditInfo.Status = returnMessage.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;

                    if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                    {
                        AuditItem descItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_Description")).FirstOrDefault();
                        if (descItem != null) { descItem.NewValue = rule.Description; }

                        AuditItem ruleContainerItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_Rule_Detail_RuleContainer")).FirstOrDefault();
                        if (ruleContainerItem != null) { ruleContainerItem.NewValue = rule.ContainerName; }

                        AuditItem disposalClassItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_DisposalClass_Title")).FirstOrDefault();
                        if (disposalClassItem != null) { disposalClassItem.NewValue = rule.DisposalClass; }

                        //SPSource
                        AuditItem spSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedSPSource")).FirstOrDefault();
                        if (spSource != null)
                        {
                            spSource.NewValue = rule.IsSpSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        }
                        if (rule.IsSpSource)
                        {
                            AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition")).FirstOrDefault();
                            ruleInfo.RuleCretias.Add(ruleInfo.FilterCombineMode);
                            if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.RuleCretias); }

                            AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction")).FirstOrDefault();
                            if (actionItem != null)
                            {
                                actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule, isNewLogicAccount: isNewLogicAccount, sourceFlag: RA.SharePoint.ArchiverCommon.SOSourceFlag.SharePoint);
                            }

                            //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                            //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }
                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                            }
                            if (rule.EnableManualApproval)
                            {
                                if (ruleInfo.ManualReviewType == ReviewType.Workflow)
                                {
                                    //workflow
                                    AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName")).FirstOrDefault();
                                    if (processItem != null)
                                    {
                                        processItem.NewValue = ruleInfo.WorkflowName;
                                    }
                                }
                                else {
                                    //Email Users
                                    AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner")).FirstOrDefault();
                                    if (emailUsers != null)
                                    {
                                        emailUsers.NewValue = rule.Users != null ? string.Join("; ", rule.Users.Select(u => u.DisplayName)) : "";
                                    }
                                }

                                //Send Email Setting
                                AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner")).FirstOrDefault();
                                if (sendEmailSetting != null)
                                {
                                    sendEmailSetting.NewValue = rule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                }
                            }

                            AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction")).FirstOrDefault();
                            if (exportItem != null)
                            {
                                var soExportInfo = rule.EnableExport && rule.ExportInfo != null ? rule.ExportInfo : null;
                                exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                            }

                            AuditItem storageName = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_SelectedStorageName") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                            if (storageName != null)
                            {
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                                storageName.NewValue = rule.StoragePolicyName;
                            }
                            AuditItem moveArchiveTierType = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_CreateRule_StoreDataTitle") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                            if (moveArchiveTierType != null)
                            {
                                moveArchiveTierType.NewValue = rule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                            }
                            else
                            {
                                moveArchiveTierType = new AuditItem();
                                moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                moveArchiveTierType.NewValue = rule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                                if (!string.IsNullOrWhiteSpace(rule.StoragePolicyId) && !IsSystemStorage(rule.StoragePolicyId) && (rule.MoveToArchiverTierWhenArchiving || rule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }
                            }
                            bool isEnableRetention = rule.RetentionInfoList == null ? rule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.RetentionInfoList != null)
                                {
                                    for (int processedCount = 0; processedCount < rule.RetentionInfoList.Count(); processedCount++)
                                    {
                                        RetentionSettings infoList = rule.RetentionInfoList[processedCount];
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                            if (retentionTime != null)
                                            {
                                                retentionTime.NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
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
                                                retentionAction.NewValue = infoList.OperateDataType switch
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
                                                AuditItem removeStub=info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                                if (removeStub != null)
                                                {
                                                    removeStub.NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                }

                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete" && x.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                                    if (tempAuditItem != null)
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes "+"\n"+ string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                    AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                    if (retentionTime != null)
                                    {
                                        retentionTime.NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.RetentionInfo.Condition switch
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
                                        };
                                    }
                                    if (rule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_Title_SelectProcess") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                            if (workFlowAudit != null)
                                            {
                                                workFlowAudit.NewValue = workFlow.Name;
                                            }

                                        }
                                        else if (rule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_MAChooseUsersTip") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                            if (recordOwnerAudit != null)
                                            {
                                                recordOwnerAudit.NewValue = string.Join(",", rule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList());
                                            }
                                        };
                                    }
                                    AuditItem sendEmail = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_SendEMail") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                    if (sendEmail != null)
                                    {
                                        sendEmail.NewValue = rule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }
                            }
                        }

                        //OneDriveSource
                        AuditItem oneDriveSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedOneDriveSource")).FirstOrDefault();
                        if (oneDriveSource != null)
                        {
                            oneDriveSource.NewValue = rule.IsOneDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        }
                        if (rule.IsOneDriveSource)
                        {
                            AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_ONE")).FirstOrDefault();
                            ruleInfo.OneDriveRule.RuleCretias.Add(ruleInfo.OneDriveRule.FilterCombineMode);
                            if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.OneDriveRule.RuleCretias); }

                            AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_ONE")).FirstOrDefault();
                            if (actionItem != null)
                            {
                                if(rule.OneDriveRule.ModelType == RuleModel.None)
                                {
                                    rule.OneDriveRule.ModelType = rule.ModelType;
                                }
                                actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.OneDriveRule, rule.ModelType, RA.SharePoint.ArchiverCommon.SOSourceFlag.OneDrive, isNewLogicAccount);
                            }

                            AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ONE")).FirstOrDefault();
                            if (manualItem != null) { manualItem.NewValue = rule.OneDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                            if (rule.OneDriveRule.EnableManualApproval)
                            {
                                if (ruleInfo.OneDriveRule.ManualReviewType == ReviewType.Workflow)
                                {
                                    //workflow
                                    AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_ONE")).FirstOrDefault();
                                    if (processItem != null)
                                    {
                                        processItem.NewValue = ruleInfo.OneDriveRule.WorkflowName;
                                    }
                                }
                                else
                                {
                                    //Email Users
                                    AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_ONE")).FirstOrDefault();
                                    if (emailUsers != null)
                                    {
                                        emailUsers.NewValue = rule.OneDriveRule.Users != null ? string.Join("; ", rule.OneDriveRule.Users.Select(u => u.DisplayName)) : "";
                                    }
                                }

                                //Send Email Setting
                                AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_ONE")).FirstOrDefault();
                                if (sendEmailSetting != null)
                                {
                                    sendEmailSetting.NewValue = rule.OneDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                }
                            }

                            if(rule.OneDriveRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection)
                            {
                                AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_ONE")).FirstOrDefault();
                                if (exportItem != null)
                                {
                                    var soExportInfo = rule.OneDriveRule.EnableExport && rule.OneDriveRule.ExportInfo != null ? rule.OneDriveRule.ExportInfo : null;
                                    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                }
                            }
                            AuditItem storageName = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_SelectedStorageName_ONE") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                            if (storageName != null)
                            {
                                storageName.NewValue = rule.OneDriveRule.StoragePolicyName;
                            }
                            info.ModifyContent.Where(item => item.TargetSetting != null && item.TargetSetting.EndsWith("_ONE")).ToList().ForEach(n => n.TargetSetting = RuleAuditUtil.getEXORuleAuditString(n.TargetSetting));

                            
                            AuditItem moveArchiveTierType = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_CreateRule_StoreDataTitle") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                            if (moveArchiveTierType != null)
                            {
                                moveArchiveTierType.NewValue = rule.OneDriveRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.OneDriveRule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                            }
                            else
                            {
                                moveArchiveTierType = new AuditItem();
                                moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                moveArchiveTierType.NewValue = rule.OneDriveRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.OneDriveRule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                                if (!string.IsNullOrWhiteSpace(rule.OneDriveRule.StoragePolicyId) && !IsSystemStorage(rule.OneDriveRule.StoragePolicyId) && (rule.OneDriveRule.MoveToArchiverTierWhenArchiving || rule.OneDriveRule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }
                            }
                            bool isEnableRetention = rule.OneDriveRule.RetentionInfoList == null ? rule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.OneDriveRule.RetentionInfoList != null)
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
                                                retentionTime.NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
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
                                                retentionAction.NewValue = infoList.OperateDataType switch
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
                                                    removeStub.NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                }
                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete").LastOrDefault();
                                                    if (tempAuditItem != null)
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                else if (rule.OneDriveRule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                    if (retentionTime != null)
                                    {
                                        retentionTime.NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.OneDriveRule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + rule.OneDriveRule.RetentionInfo.KeepDateNumber + " " + rule.OneDriveRule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + rule.OneDriveRule.RetentionInfo.Date,
                                            _ => ""
                                        };
                                    }
                                    if (rule.OneDriveRule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.OneDriveRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.OneDriveRule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit =info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_Title_SelectProcess") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                            if (workFlowAudit != null)
                                            {
                                                workFlowAudit.NewValue = workFlow.Name;
                                            }
                                        }
                                        else if (rule.OneDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_MAChooseUsersTip") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                            if (recordOwnerAudit != null)
                                            {
                                                recordOwnerAudit.NewValue = string.Join(",", rule.OneDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList());
                                            }
                                        };
                                    }
                                    AuditItem sendEmail = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_SendEMail") && a.Id.Equals(new Guid(SOConstants.ODAuditId))).FirstOrDefault();
                                    if (sendEmail != null)
                                    {
                                        sendEmail.NewValue = rule.OneDriveRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }
                            }
                        }

                        //Teams
                        if(rule.ModelType == RuleModel.SOArchiver && rule.RuleLevel == GCommon.Contract.CommonFilter.PolicyLevel.Teams)
                        {
                        AuditItem teamsSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedTeamsSource")).FirstOrDefault();
                        if (teamsSource != null)
                        {
                            teamsSource.NewValue = rule.IsTeamsSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        }
                        if (rule.IsTeamsSource)
                        {
                            AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_TMS")).FirstOrDefault();
                            ruleInfo.TeamsRule.RuleCretias.Add(ruleInfo.TeamsRule.FilterCombineMode);
                            if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.TeamsRule.RuleCretias); }

                            AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_TMS")).FirstOrDefault();
                            if (actionItem != null)
                            {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.TeamsRule, rule.ModelType);
                            }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                            if (rule.ModelType == RuleModel.Records)
                            {
                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_TMS")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.TeamsRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                            }
                            if (rule.TeamsRule.EnableManualApproval)
                            {
                                if (ruleInfo.TeamsRule.ManualReviewType == ReviewType.Workflow)
                                {
                                    //workflow
                                    AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_TMS")).FirstOrDefault();
                                    if (processItem != null)
                                    {
                                        processItem.NewValue = ruleInfo.TeamsRule.WorkflowName;
                                    }
                                }
                                else
                                {
                                    //Email Users
                                    AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_TMS")).FirstOrDefault();
                                    if (emailUsers != null)
                                    {
                                        emailUsers.NewValue = rule.TeamsRule.Users != null ? string.Join("; ", rule.TeamsRule.Users.Select(u => u.DisplayName)) : "";
                                    }
                                }

                                //Send Email Setting
                                AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_TMS")).FirstOrDefault();
                                if (sendEmailSetting != null)
                                {
                                    sendEmailSetting.NewValue = rule.TeamsRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                }
                            }

                            if(rule.TeamsRule.RuleLevel != GCommon.Contract.CommonFilter.PolicyLevel.Teams)
                            {
                                AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_TMS")).FirstOrDefault();
                                if (exportItem != null)
                                {
                                    var soExportInfo = rule.TeamsRule.EnableExport && rule.TeamsRule.ExportInfo != null ? rule.TeamsRule.ExportInfo : null;
                                    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                }
                            }

                            AuditItem storageName = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_SelectedStorageName_TMS") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                            if (storageName != null)
                            {
                                storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_TMS";
                                storageName.NewValue = rule.TeamsRule.StoragePolicyName;
                            }
                            AuditItem moveArchiveTierType = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_CreateRule_StoreDataTitle") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                            if (moveArchiveTierType != null)
                            {
                                moveArchiveTierType.NewValue = rule.TeamsRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.TeamsRule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                            }
                            else
                            {
                                moveArchiveTierType = new AuditItem();
                                moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                moveArchiveTierType.NewValue = rule.TeamsRule.MoveToArchiverTierWhenArchiving ? "RM_RDM_CreateRule_ArchivedTier" : rule.TeamsRule.MoveToAnotherTierType switch
                                {
                                    0 => "RM_RDM_CreateRule_DefaultTier",
                                    3 => "RM_RDM_CreateRule_ArchivedTier",
                                    4 => "RM_RDM_CreateRule_ColdTier",
                                    _ => ""
                                };//0 default,3 archive,4 cold
                                if (!IsSystemStorage(rule.TeamsRule.StoragePolicyId) && (rule.TeamsRule.MoveToArchiverTierWhenArchiving || rule.TeamsRule.MoveToAnotherTierType != null))
                                {
                                    info.ModifyContent.Add(moveArchiveTierType);
                                }
                            }
                            bool isEnableRetention = rule.TeamsRule.RetentionInfoList == null ? rule.TeamsRule.RetentionInfo == null ? false : true : true;
                            AuditItem enableRetention = new AuditItem() { TargetSetting = "RM_JS_Rule_Detail_Retention", NewValue = isEnableRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No" };
                            if (isEnableRetention)
                            {
                                if (rule.TeamsRule.RetentionInfoList != null)
                                {
                                    for (int processedCount = 0; processedCount < rule.TeamsRule.RetentionInfoList.Count(); processedCount++)
                                    {
                                        RetentionSettings infoList = rule.TeamsRule.RetentionInfoList[processedCount];
                                        if (infoList.IsEnableRetention)
                                        {
                                            string auditString = infoList.RetentionDataTimeType == GCommon.Contract.Storage.Entity.KeepDateType.ModifiedTime ? "RM_RDM_CreateRule_RemoveModified_Time" : "RM_RDM_CreateRule_RemoveArchive_Time";
                                            AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.Deep == processedCount && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                            if (retentionTime != null)
                                            {
                                                retentionTime.NewValue = auditString + " " + "RM_JS_RDM_CreateRule_DateOption_Older" + " " + infoList.KeepDateNumber + " " + infoList.KeepDateUnite switch
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
                                                retentionAction.NewValue = infoList.OperateDataType switch
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
                                                    removeStub.NewValue = infoList.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                                }

                                                if (KeyValueService.IsEnableSoftDeleteSetting())
                                                {
                                                    var tempAuditItem = info.ModifyContent.Where(x => x.TargetSetting == "RM_AR_CP_GSS_Retention_SoftDelete" && x.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                                    if (tempAuditItem != null)
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete ? "RM_JS_Common_Yes " + "\n" + string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), infoList.SoftKeepDateNumber + " " + infoList.SoftKeepDateUnite switch
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
                                else if (rule.TeamsRule.RetentionInfo != null)
                                {
                                    AuditItem retentionTime = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                    if (retentionTime != null)
                                    {
                                        retentionTime.NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " + rule.TeamsRule.RetentionInfo.Condition switch
                                        {
                                            TimeFilterCondition.OlderThan => "RM_JS_RDM_CreateRule_DateOption_Older" + " " + rule.TeamsRule.RetentionInfo.KeepDateNumber + " " + rule.TeamsRule.RetentionInfo.KeepDateUnite switch
                                            {
                                                TimeUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                TimeUnit.Week => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                TimeUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                TimeUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                _ => ""
                                            },
                                            TimeFilterCondition.Is => "RM_JS_RDM_CreateRule_DateOption_Before" + " " + rule.TeamsRule.RetentionInfo.Date,
                                            _ => ""
                                        };
                                    }
                                    if (rule.TeamsRule.RetentionInfo.IsManualApproval)
                                    {
                                        if (rule.TeamsRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                        {
                                            var workFlow = ManualApprovalWorkflowManager.Get(rule.TeamsRule.RetentionInfo.WorkflowId);
                                            AuditItem workFlowAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_CreateRule_Title_SelectProcess") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                            if (workFlowAudit != null)
                                            {
                                                workFlowAudit.NewValue = workFlow.Name;
                                            }

                                        }
                                        else if (rule.TeamsRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                        {
                                            AuditItem recordOwnerAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_MAChooseUsersTip") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                            if (recordOwnerAudit != null)
                                            {
                                                recordOwnerAudit.NewValue = string.Join(",", rule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName).ToList());
                                            }
                                        };
                                    }
                                    AuditItem sendEmail = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_SendEMail") && a.Id.Equals(new Guid(SOConstants.TEAMSAuditId))).FirstOrDefault();
                                    if (sendEmail != null)
                                    {
                                        sendEmail.NewValue = rule.TeamsRule.RetentionInfo.IsSendEamilToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }
                            }
                        }
                        }

                        if (rule.ModelType != RuleModel.SOArchiver) 
                        {
                            AuditItem exoSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedEXOSource")).FirstOrDefault();
                            if (exoSource != null)
                            {
                                exoSource.NewValue = rule.IsExoSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }

                            if (rule.IsExoSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_EXO")).FirstOrDefault();
                                ruleInfo.EXORule.RuleCretias.Add(ruleInfo.EXORule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.EXORule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_EXO")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.EXORule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_EXO")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.EXORule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.EXORule.EnableManualApproval)
                                {
                                    if (ruleInfo.EXORule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_EXO")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.EXORule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_EXO")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.EXORule.Users != null ? string.Join("; ", rule.EXORule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_EXO")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.EXORule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_EXO")).FirstOrDefault();
                                if (exportItem != null)
                                {
                                    var soExportInfo = rule.EXORule.EnableExport && rule.EXORule.ExportInfo != null ? rule.EXORule.ExportInfo : null;
                                    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                }

                            }

                            //PhySource
                            AuditItem phySource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedPhysicalSource")).FirstOrDefault();
                            if (phySource != null)
                            {
                                phySource.NewValue = rule.IsPhySource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsPhySource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_PHY")).FirstOrDefault();
                                ruleInfo.PhysicalRule.RuleCretias.Add(ruleInfo.PhysicalRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.PhysicalRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_PHY")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.PhysicalRule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_PHY")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.PhysicalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.PhysicalRule.EnableManualApproval)
                                {
                                    if (ruleInfo.PhysicalRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_PHY")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.PhysicalRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_PHY")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.PhysicalRule.Users != null ? string.Join("; ", rule.PhysicalRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_PHY")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.PhysicalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }
                                //AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_ExportAction"))).FirstOrDefault();
                                //if (exportItem != null)
                                //{
                                //    var soExportInfo = rule.EnableExport && rule.ExportInfo != null ? rule.ExportInfo : null;
                                //    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //}
                            }

                            //FS Source
                            AuditItem fsSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedFSSource")).FirstOrDefault();
                            if (fsSource != null)
                            {
                                fsSource.NewValue = rule.IsFSSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsFSSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_FSO")).FirstOrDefault();
                                ruleInfo.FSRule.RuleCretias.Add(ruleInfo.FSRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.FSRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_FSO")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.FSRule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_FSO")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.FSRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.FSRule.EnableManualApproval)
                                {
                                    if (ruleInfo.FSRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_FSO")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.FSRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_FSO")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.FSRule.Users != null ? string.Join("; ", rule.FSRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_FSO")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.FSRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                AuditItem storageName = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_SelectedStorageName_FSO") && a.Id.Equals(new Guid(SOConstants.SPAuditId))).FirstOrDefault();
                                if (storageName != null)
                                {
                                    storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName_FSO";
                                    storageName.NewValue = rule.FSRule.StoragePolicyName;
                                }

                                //AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_FSO")).FirstOrDefault();
                                //if (exportItem != null)
                                //{
                                //    var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //}
                            }

                            AuditItem azureFileSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedAzureFileSource")).FirstOrDefault();
                            if (azureFileSource != null)
                            {
                                azureFileSource.NewValue = rule.IsAzureFileSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsAzureFileSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_AZF")).FirstOrDefault();
                                ruleInfo.AzureFileRule.RuleCretias.Add(ruleInfo.AzureFileRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.AzureFileRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_AZF")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.AzureFileRule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_AZF")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.AzureFileRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.AzureFileRule.EnableManualApproval)
                                {
                                    if (ruleInfo.AzureFileRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_AZF")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.AzureFileRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_FSO")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.AzureFileRule.Users != null ? string.Join("; ", rule.AzureFileRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_FSO")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.AzureFileRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                //AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_FSO")).FirstOrDefault();
                                //if (exportItem != null)
                                //{
                                //    var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //}
                            }

                            AuditItem connectorSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedConnectorSource")).FirstOrDefault();
                            if (connectorSource != null)
                            {
                                connectorSource.NewValue = rule.IsConnectorSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsConnectorSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_CNT")).FirstOrDefault();
                                ruleInfo.ConnectorRule.RuleCretias.Add(ruleInfo.ConnectorRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.ConnectorRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_CNT")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.ConnectorRule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_CNT")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.ConnectorRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.ConnectorRule.EnableManualApproval)
                                {
                                    if (ruleInfo.ConnectorRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_CNT")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.ConnectorRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_FSO")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.ConnectorRule.Users != null ? string.Join("; ", rule.ConnectorRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_FSO")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.ConnectorRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                //AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_FSO")).FirstOrDefault();
                                //if (exportItem != null)
                                //{
                                //    var soExportInfo = rule.FSRule.EnableExport && rule.FSRule.ExportInfo != null ? rule.FSRule.ExportInfo : null;
                                //    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                //}
                            }

                            //SPLocalSource
                            AuditItem spLocalSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedSPLocalSource")).FirstOrDefault();
                            if (spLocalSource != null)
                            {
                                spLocalSource.NewValue = rule.IsSPLocalSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsSPLocalSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_SPL")).FirstOrDefault();
                                ruleInfo.SPLocalRule.RuleCretias.Add(ruleInfo.SPLocalRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.SPLocalRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_SPL")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.SPLocalRule);
                                }

                                //AuditItem declaredFileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_RDM_IncludeDeclaredFile"))).FirstOrDefault();
                                //if (declaredFileItem != null) { declaredFileItem.NewValue = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"); }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_SPL")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.SPLocalRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.SPLocalRule.EnableManualApproval)
                                {
                                    if (ruleInfo.SPLocalRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        //workflow
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_SPL")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.SPLocalRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        //Email Users
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_SPL")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.SPLocalRule.Users != null ? string.Join("; ", rule.SPLocalRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }

                                    //Send Email Setting
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_SPL")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.SPLocalRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_SPL")).FirstOrDefault();
                                if (exportItem != null)
                                {
                                    var soExportInfo = rule.SPLocalRule.EnableExport && rule.SPLocalRule.ExportInfo != null ? rule.SPLocalRule.ExportInfo : null;
                                    exportItem.NewValue = RuleAuditUtil.GetExportInfo(soExportInfo);
                                }
                            }

                            //Box
                            AuditItem boxSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedBoxSource")).FirstOrDefault();
                            if (boxSource != null)
                            {
                                boxSource.NewValue = rule.IsBoxSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsBoxSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_BOX")).FirstOrDefault();
                                ruleInfo.BoxRule.RuleCretias.Add(ruleInfo.BoxRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.BoxRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_BOX")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.BoxRule);
                                }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_BOX")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.BoxRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.BoxRule.EnableManualApproval)
                                {
                                    if (ruleInfo.BoxRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_BOX")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.BoxRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_BOX")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.BoxRule.Users != null ? string.Join("; ", rule.BoxRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_BOX")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.BoxRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }
                            }

                            // Google Drive
                            AuditItem googleDriveSource = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_IsCheckedGoogleDriveSource")).FirstOrDefault();
                            if (googleDriveSource != null)
                            {
                                googleDriveSource.NewValue = rule.IsGoogleDriveSource ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            }
                            if (rule.IsGoogleDriveSource)
                            {
                                AuditItem conditionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalCondition_GGD")).FirstOrDefault();
                                ruleInfo.GoogleDriveRule.RuleCretias.Add(ruleInfo.GoogleDriveRule.FilterCombineMode);
                                if (conditionItem != null) { conditionItem.NewValue = string.Join("<br>", ruleInfo.GoogleDriveRule.RuleCretias); }

                                AuditItem actionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_DisposalAction_GGD")).FirstOrDefault();
                                if (actionItem != null)
                                {
                                    actionItem.NewValue = RuleAuditUtil.GetAuditorRuleActionString(rule.GoogleDriveRule);
                                }

                                AuditItem manualItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_GGD")).FirstOrDefault();
                                if (manualItem != null) { manualItem.NewValue = rule.GoogleDriveRule.EnableManualApproval ? "RM_JS_Common_Yes" : "RM_JS_Common_No"; }
                                if (rule.GoogleDriveRule.EnableManualApproval)
                                {
                                    if (ruleInfo.GoogleDriveRule.ManualReviewType == ReviewType.Workflow)
                                    {
                                        AuditItem processItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ManualApproval_ProcessName_GGD")).FirstOrDefault();
                                        if (processItem != null)
                                        {
                                            processItem.NewValue = ruleInfo.GoogleDriveRule.WorkflowName;
                                        }
                                    }
                                    else
                                    {
                                        AuditItem emailUsers = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_RecordOwner_GGD")).FirstOrDefault();
                                        if (emailUsers != null)
                                        {
                                            emailUsers.NewValue = rule.GoogleDriveRule.Users != null ? string.Join("; ", rule.GoogleDriveRule.Users.Select(u => u.DisplayName)) : "";
                                        }
                                    }
                                    AuditItem sendEmailSetting = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_MA_Grid_SendEmailRecordOwner_GGD")).FirstOrDefault();
                                    if (sendEmailSetting != null)
                                    {
                                        sendEmailSetting.NewValue = rule.GoogleDriveRule.IsSendEmailToOwner ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                                    }
                                }

                                AuditItem exportItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_ExportAction_GGD")).FirstOrDefault();
                                if (exportItem != null)
                                {
                                    var googleExportInfo = rule.GoogleDriveRule.EnableExport && rule.GoogleDriveRule.ExportInfo != null ? rule.GoogleDriveRule.ExportInfo : null;
                                    exportItem.NewValue = RuleAuditUtil.GetExportInfo(googleExportInfo);
                                }
                                
                                AuditItem storageName = info.ModifyContent.FirstOrDefault(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_Rule_SelectedStorageName") && a.Id.Equals(new Guid(SOConstants.GGAuditId)));
                                if (storageName != null)
                                {
                                    storageName.TargetSetting = "RM_JS_RDM_Rule_SelectedStorageName";
                                    storageName.NewValue = rule.GoogleDriveRule.StoragePolicyName;
                                }
                                AuditItem moveArchiveTierType = info.ModifyContent.FirstOrDefault(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_RDM_CreateRule_StoreDataTitle") && a.Id.Equals(new Guid(SOConstants.GGAuditId)));
                                if (moveArchiveTierType != null)
                                {
                                    moveArchiveTierType.NewValue = rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving
                                        ? "RM_RDM_CreateRule_ArchivedTier"
                                        : rule.GoogleDriveRule.MoveToAnotherTierType switch
                                        {
                                            0 => "RM_RDM_CreateRule_DefaultTier",
                                            3 => "RM_RDM_CreateRule_ArchivedTier",
                                            4 => "RM_RDM_CreateRule_ColdTier",
                                            _ => ""
                                        }; //0 default,3 archive,4 cold
                                }
                                else
                                {
                                    moveArchiveTierType = new AuditItem();
                                    moveArchiveTierType.TargetSetting = "RM_JS_RDM_CreateRule_StoreDataTitle";
                                    moveArchiveTierType.NewValue = rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving
                                        ? "RM_RDM_CreateRule_ArchivedTier"
                                        : rule.GoogleDriveRule.MoveToAnotherTierType switch
                                        {
                                            0 => "RM_RDM_CreateRule_DefaultTier",
                                            3 => "RM_RDM_CreateRule_ArchivedTier",
                                            4 => "RM_RDM_CreateRule_ColdTier",
                                            _ => ""
                                        }; //0 default,3 archive,4 cold
                                    if (!string.IsNullOrWhiteSpace(rule.GoogleDriveRule.StoragePolicyId) &&
                                        !IsSystemStorage(rule.GoogleDriveRule.StoragePolicyId) &&
                                        (rule.GoogleDriveRule.MoveToArchiverTierWhenArchiving || rule.GoogleDriveRule.MoveToAnotherTierType != null))
                                    {
                                        info.ModifyContent.Add(moveArchiveTierType);
                                    }
                                }

                                bool isEnableRetention = rule.GoogleDriveRule.RetentionInfoList == null
                                    ? rule.GoogleDriveRule.RetentionInfo == null ? false : true
                                    : true;
                                if (isEnableRetention)
                                {
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
                                                    retentionTime.NewValue = auditString + " " +
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
                                                    retentionAction.NewValue = infoList.OperateDataType switch
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
                                                    if (tempAuditItem != null)
                                                    {
                                                        tempAuditItem.NewValue = infoList.IsSoftDelete
                                                            ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                                I18NEntity.GetString(
                                                                    "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                                infoList.SoftKeepDateNumber + " " +
                                                                infoList.SoftKeepDateUnite switch
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
                                        AuditItem retentionTime = info.ModifyContent.Where(a =>
                                            a.TargetSetting != null &&
                                            a.TargetSetting.Equals("RM_RDM_CreateRule_RemoveArchive_Prefix") &&
                                            a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                        if (retentionTime != null)
                                        {
                                            retentionTime.NewValue = "RM_RDM_CreateRule_RemoveArchive_Time" + " " +
                                                                     rule.GoogleDriveRule.RetentionInfo.Condition switch
                                                                     {
                                                                         TimeFilterCondition.OlderThan =>
                                                                             "RM_JS_RDM_CreateRule_DateOption_Older" +
                                                                             " " + rule.GoogleDriveRule.RetentionInfo.KeepDateNumber +
                                                                             " " + rule.GoogleDriveRule.RetentionInfo
                                                                                     .KeepDateUnite switch
                                                                                 {
                                                                                     TimeUnit.Day => I18NEntity
                                                                                         .GetString(
                                                                                             "RM_JS_RDM_CreateRule_Unit_Days"),
                                                                                     TimeUnit.Week => I18NEntity
                                                                                         .GetString(
                                                                                             "RM_JS_RDM_CreateRule_Unit_Weeks"),
                                                                                     TimeUnit.Month => I18NEntity
                                                                                         .GetString(
                                                                                             "RM_JS_RDM_CreateRule_Unit_Months"),
                                                                                     TimeUnit.Year => I18NEntity
                                                                                         .GetString(
                                                                                             "RM_JS_RDM_CreateRule_Unit_Years"),
                                                                                     _ => ""
                                                                                 },
                                                                         TimeFilterCondition.Is =>
                                                                             "RM_JS_RDM_CreateRule_DateOption_Before" +
                                                                             " " + rule.GoogleDriveRule.RetentionInfo.Date,
                                                                         _ => ""
                                                                     };
                                        }

                                        if (rule.GoogleDriveRule.RetentionInfo.IsManualApproval)
                                        {
                                            if (rule.GoogleDriveRule.RetentionInfo.ReviewType == ReviewType.Workflow)
                                            {
                                                var workFlow =
                                                    ManualApprovalWorkflowManager.Get(rule.GoogleDriveRule.RetentionInfo.WorkflowId);
                                                AuditItem workFlowAudit = info.ModifyContent.Where(a =>
                                                    a.TargetSetting != null &&
                                                    a.TargetSetting.Equals("RM_RDM_CreateRule_Title_SelectProcess") &&
                                                    a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                                if (workFlowAudit != null)
                                                {
                                                    workFlowAudit.NewValue = workFlow.Name;
                                                }
                                            }
                                            else if (rule.GoogleDriveRule.RetentionInfo.ReviewType == ReviewType.RecordOwner)
                                            {
                                                AuditItem recordOwnerAudit = info.ModifyContent.Where(a =>
                                                    a.TargetSetting != null &&
                                                    a.TargetSetting.Equals("RM_SPS_MAChooseUsersTip") &&
                                                    a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                                if (recordOwnerAudit != null)
                                                {
                                                    recordOwnerAudit.NewValue = string.Join(",",
                                                        rule.GoogleDriveRule.RetentionInfo.UserInfos.Select(u => u.UserPrincipalName)
                                                            .ToList());
                                                }
                                            }
                                        }

                                        if (KeyValueService.IsEnableSoftDeleteSetting())
                                        {
                                            var tempAuditItem = info.ModifyContent.FirstOrDefault(x =>
                                                x.TargetSetting ==
                                                "RM_AR_CP_GSS_Retention_SoftDelete" &&
                                                x.Id.Equals(new Guid(SOConstants.GGAuditId)));
                                            if (tempAuditItem != null)
                                            {
                                                tempAuditItem.NewValue = rule.GoogleDriveRule.RetentionInfo.IsSoftDelete
                                                    ? "RM_JS_Common_Yes " + "\n" + string.Format(
                                                        I18NEntity.GetString(
                                                            "RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"),
                                                        rule.GoogleDriveRule.RetentionInfo.SoftKeepDateNumber + " " +
                                                        rule.GoogleDriveRule.RetentionInfo.SoftKeepDateUnite switch
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


                                        AuditItem sendEmail = info.ModifyContent.Where(a =>
                                            a.TargetSetting != null && a.TargetSetting.Equals("RM_SPS_SendEMail") &&
                                            a.Id.Equals(new Guid(SOConstants.GGAuditId))).FirstOrDefault();
                                        if (sendEmail != null)
                                        {
                                            sendEmail.NewValue = rule.GoogleDriveRule.RetentionInfo.IsSendEamilToOwner
                                                ? "RM_JS_Common_Yes"
                                                : "RM_JS_Common_No";
                                        }
                                    }
                                }
                            }
                        }

                        ResetTargetSettings(info);
                    }
                    
                    break;
                case AuditAction.DeleteRule:
                    if (info != null)
                    {
                        auditInfo.Object = info.Object;
                    }
                    auditInfo.Status = ((RAReturnMessage)returnValue)?.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    break;
                case AuditAction.ExportRuleUsageReport:
                    var client = new DAOAPIClientV1();
                    //rule = DocAveOnlineUtility.LoadRule((string)args[3]);
                    var soRule = client.LoadRule((string)args[3]);
                    auditInfo.Object = rule == null ? string.Empty : soRule.Name;
                    break;
                case AuditAction.CreateRuleContainer:
                case AuditAction.EditRuleContainer:
                    auditInfo.Object = info.Object;
                    if (returnValue == null)
                    {
                        auditInfo.NotNeedRecordAudit = true;
                    }
                    break;
                case AuditAction.DeleteRuleContainer:
                    auditInfo.Object = info.Object;
                    auditInfo.Status = ((RAReturnMessage)returnValue)?.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    break;
                default:
                    break;
            }
            auditInfo.Action = (AuditAction)action;
            auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
            return auditInfo;
        }


        private bool NeedShowStoragePolicy(RMRuleInfos rule, RuleModel ruleModel = RuleModel.None)
        {
            //export
            if (rule.EnableExport && rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return false;
            }
            //move
            if (rule.MoveDto != null)
            {
                return false;
            }
            //keep data
            if ((rule.RuleKeepDataOption & 16) == 16)
            {
                return false;
            }

            List<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel> ruleLevels = new List<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel>()
            {
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Site,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.List,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Folder,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Item,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.ItemVersion,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Attachment,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.GoogleDriveDocument,
            };
            if (ruleLevels.Contains(rule.RuleLevel) && rule.RuleKeepDataOption == (int)KeepDataStatus.Delete)
            {
                return true;
            }

            if ((rule.RuleLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document || rule.RuleLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.GoogleDriveDocument) && (rule.RuleKeepDataOption == (int)KeepDataStatus.Archive || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub))
            {
                return true;
            }
            if (rule.IsSpSource && rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
            {
                return true;
            }

            if((ruleModel == RuleModel.SOArchiver || rule.ModelType == RuleModel.SOArchiver) && (rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemove || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub))
            {
                return true;
            }

            return false;
        }

        private bool IsSystemStorage(string storagePolicyId)
        {
            if (!_storageIsSystemDic.ContainsKey(storagePolicyId))
            {
                var result = RuleAuditUtil.IsSystemStorage(storagePolicyId);
                _storageIsSystemDic[storagePolicyId] = result;
            }
            return _storageIsSystemDic[storagePolicyId];
        }

        private void ResetTargetSettings(RMAuditInfo info)
        {
            info.ModifyContent.Where(item => item.TargetSetting != null && IsNeedReplaceTargetSetting(item.TargetSetting)).ToList().ForEach(n => n.TargetSetting = RuleAuditUtil.getEXORuleAuditString(n.TargetSetting));
        }

        private bool IsNeedReplaceTargetSetting(string targetSetting)
        {
            var endStrs = new List<string> { "_EXO", "_PHY", "_FSO", "_SPL", "_ONE" , "_AZF" , "_CNT", "_BOX" , "_GGD", "_TMS"};
            var regexStr = $"({string.Join("|", endStrs)})$";
            return Regex.IsMatch(targetSetting, regexStr, RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
        }

    }

}
