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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Multi_Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class WorkflowManagementAfterAuditHandler : IAfterAuditHandler
    {

        public IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

        public IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo();
            WorkflowDefinitionDto dto = null;
            RAReturnMessage returnMessage = null;
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            if(info == null)
            {
                auditInfo.Action = (AuditAction)action;
            }
            var customTemplateList = EmailTemplateService.GetAllCustomEmailTemplates();
            switch (action)
            {
                case (int)AuditAction.CreateWorkflow:
                    dto = args[0] as WorkflowDefinitionDto;
                    auditInfo.Object = dto != null ? dto.Name : string.Empty;
                    ArgumentCheck.NotNull(dto, nameof(dto));
                    if (dto.Id == Guid.Empty) {
                        if (auditInfo.ModifyContent == null) { auditInfo.ModifyContent = new List<AuditItem>(); }
                  
                        AuditItem nameItem = new AuditItem();
                        nameItem.TargetSetting = "RM_RDM_WorkFlow_Word_ProfileName";
                        nameItem.NewValue = dto.Name;
                        auditInfo.ModifyContent.Add(nameItem);

                        AuditItem descItem = new AuditItem();
                        descItem.TargetSetting = "RM_RDM_WorkFlow_Word_Description";
                        descItem.NewValue = dto.Description;
                        auditInfo.ModifyContent.Add(descItem);

                        AuditItem reviewersItem = new AuditItem();
                        reviewersItem.TargetSetting = "RM_RDM_WorkFlow_ReviewerText";
                        var reviewers = "";
                        var reviewerNames = ManualProcessManagementService.GetReviewerNames(dto.Content);
                        if (reviewerNames.Count > 0)
                        {
                            reviewers = string.Join(",", reviewerNames);
                        }
                        reviewersItem.NewValue = reviewers;
                        auditInfo.ModifyContent.Add(reviewersItem);

                        AuditItem levelItem = new AuditItem();
                        levelItem.TargetSetting = "RM_RDM_MAProcess_Word_ApprovalLevel";
                        levelItem.NewValue = dto.LevelCount.ToString();
                        auditInfo.ModifyContent.Add(levelItem);

                        var emailItem = new AuditItem();
                        emailItem.TargetSetting = "RM_RDM_WorkFlow_Notification";
                        emailItem.NewValue = RenderNotificationInfo(customTemplateList, dto);
                        auditInfo.ModifyContent.Add(emailItem);


                    }
                    else {
                        if (auditInfo.ModifyContent != null && auditInfo.ModifyContent.Count != 0)
                        {
                            dto = args[0] as WorkflowDefinitionDto;
                            auditInfo.Object = dto != null ? dto.Name : string.Empty;
                            ArgumentCheck.NotNull(dto, nameof(dto));

                            AuditItem nameEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_WorkFlow_Word_ProfileName")).FirstOrDefault();
                            if (nameEditItem != null) { nameEditItem.NewValue = dto.Name; }

                            AuditItem descEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_WorkFlow_Word_Description")).FirstOrDefault();
                            if (descEditItem != null) { descEditItem.NewValue = dto.Description; }

                            AuditItem reviewersEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_WorkFlow_ReviewerText")).FirstOrDefault();
                            if (reviewersEditItem != null)
                            {
                                var editReviewers = "";
                                var reviewerNames = ManualProcessManagementService.GetReviewerNames(dto.Content);
                                if (reviewerNames.Count > 0)
                                {
                                    editReviewers = string.Join(",", reviewerNames);
                                }
                                reviewersEditItem.NewValue = editReviewers;
                            }

                            AuditItem levelEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_MAProcess_Word_ApprovalLevel")).FirstOrDefault();
                            if (levelEditItem != null) { levelEditItem.NewValue = dto.LevelCount.ToString(); }

                            var emailEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RDM_WorkFlow_Notification")).FirstOrDefault();
                            if (emailEditItem != null) { emailEditItem.NewValue = RenderNotificationInfo(customTemplateList, dto); }
                        }                     
                    }
                    bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                    if (isEnableMultiGeoFeature && (auditInfo?.ModifyContent == null || auditInfo.ModifyContent.Count == 0))
                    {
                        auditInfo.ModifyContent = new List<AuditItem>();
                        AuditItem nameItem = new AuditItem();
                        nameItem.TargetSetting = "RM_RDM_WorkFlow_Word_ProfileName";
                        nameItem.NewValue = dto.Name;
                        auditInfo.ModifyContent.Add(nameItem);

                        AuditItem descItem = new AuditItem();
                        descItem.TargetSetting = "RM_RDM_WorkFlow_Word_Description";
                        descItem.NewValue = dto.Description;
                        auditInfo.ModifyContent.Add(descItem);

                        AuditItem reviewersItem = new AuditItem();
                        reviewersItem.TargetSetting = "RM_RDM_WorkFlow_ReviewerText";
                        var reviewers = "";
                        var reviewerNames = ManualProcessManagementService.GetReviewerNames(dto.Content);
                        if (reviewerNames.Count > 0)
                        {
                            reviewers = string.Join(",", reviewerNames);
                        }
                        reviewersItem.NewValue = reviewers;
                        auditInfo.ModifyContent.Add(reviewersItem);

                        AuditItem levelItem = new AuditItem();
                        levelItem.TargetSetting = "RM_RDM_MAProcess_Word_ApprovalLevel";
                        levelItem.NewValue = dto.LevelCount.ToString();
                        auditInfo.ModifyContent.Add(levelItem);

                        var emailItem = new AuditItem();
                        emailItem.TargetSetting = "RM_RDM_WorkFlow_Notification";
                        emailItem.NewValue = RenderNotificationInfo(customTemplateList, dto);
                        auditInfo.ModifyContent.Add(emailItem);
                    }
                    break;
                case (int)AuditAction.DeleteWorkflow:
                    if (info != null)
                    {
                        auditInfo.Object = info.Object;
                    }
                    break;
            }

            returnMessage = (RAReturnMessage)returnValue;
            auditInfo.Status = returnMessage.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private static string RenderNotificationInfo(List<EmailTemplateDto> customTemplateList, WorkflowDefinitionDto dto)
        {
            var steps = dto.Content.WorkflowNodes.Where(step => step.NodeType == WorkflowNodeType.BeginDisposalReview || step.NodeType == WorkflowNodeType.DisposalReview);
            var stepInfo = string.Empty;
            foreach (var step in steps)
            {
                if (step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Default)
                {
                    stepInfo += $"{step.DisplayName}({RMWorkflowStepUsedEmailTemplateMode.Default})" + '\n';
                }
                else if (step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Specify)
                {
                    var customTemplate = customTemplateList.Where(template => template.UniqueId == step.UsedEmailTemplateId).FirstOrDefault();
                    stepInfo += $"{step.DisplayName}({RMWorkflowStepUsedEmailTemplateMode.Specify}) : {customTemplate?.Name}" + '\n';
                }
                else
                {
                    stepInfo += $"{step.DisplayName}({RMWorkflowStepUsedEmailTemplateMode.Custom}) : " + '\n';
                    var index = 1;
                    foreach (var customSetting in step.CustomIntervalSetting)
                    {
                        var templateName = I18NEntity.GetString("RM_CP_Email_ManualApprovalForRecordsReviewer") + " " + I18NEntity.GetString("RM_RDM_WorkFlow_DefaultTemplate");
                        var customTemplateName = customTemplateList.Where(template => template.UniqueId == new Guid(customSetting.UsedEmailTemplateId)).FirstOrDefault();
                        var intervalUnit = customSetting.Interval > 1 ? I18NEntity.GetString("RM_RDM_WorkFlow_ViewDays") : I18NEntity.GetString("RM_RDM_WorkFlow_ViewDay");
                        if (customTemplateName != null)
                        {
                            templateName = customTemplateName.Name;
                        }
                        if (index == 1)
                        {
                            stepInfo += index + "." + I18NEntity.GetString("RM_RDM_WorkFlow_IntoStage") + "; " + templateName + '\n';
                        }
                        else
                        {
                            stepInfo += index + "." + I18NEntity.GetString("RM_MA_Setting_Advanced_After") + customSetting.Interval + " " + intervalUnit + "; " + templateName + '\n';
                        }
                        index++;
                    }
                }
            }
            return stepInfo;
        }
    }
}
