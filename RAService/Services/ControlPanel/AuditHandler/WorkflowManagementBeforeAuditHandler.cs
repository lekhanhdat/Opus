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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class WorkflowManagementBeforeAuditHandler : IBeforeAuditHandler
    {

        public IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        public IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            var customTemplateList = EmailTemplateService.GetAllCustomEmailTemplates();
            info.ModifyContent = new List<AuditItem>();
            info.Action = (AuditAction)action;
            if (action == (int)AuditAction.CreateWorkflow)
            {
                WorkflowDefinitionDto dto = (WorkflowDefinitionDto)args[0];
                if (dto.Id == Guid.Empty)
                {
                    info.Action = AuditAction.CreateWorkflow;
                }
                else
                {
                    info.Action = AuditAction.EditWorkflow;
                    var oldWorkflow = await ManualProcessManagementService.LoadWorkflowViewDtoAsync(dto.Id);

                    AuditItem nameItem = new AuditItem();
                    nameItem.TargetSetting = "RM_RDM_WorkFlow_Word_ProfileName";
                    nameItem.OldValue = oldWorkflow.Name;
                    info.ModifyContent.Add(nameItem);

                    AuditItem descItem = new AuditItem();
                    descItem.TargetSetting = "RM_RDM_WorkFlow_Word_Description";
                    descItem.OldValue = oldWorkflow.Description;
                    info.ModifyContent.Add(descItem);

                    AuditItem reviewersItem = new AuditItem();
                    reviewersItem.TargetSetting = "RM_RDM_WorkFlow_ReviewerText";
                    var reviewers = "";
                    if (oldWorkflow.UserDisplayNames != null && oldWorkflow.UserDisplayNames.Count > 0)
                    {
                        reviewers = string.Join(",", oldWorkflow.UserDisplayNames);
                    }
                    reviewersItem.OldValue = reviewers;
                    info.ModifyContent.Add(reviewersItem);

                    AuditItem levelItem = new AuditItem();
                    levelItem.TargetSetting = "RM_RDM_MAProcess_Word_ApprovalLevel" ;
                    levelItem.OldValue = oldWorkflow.LevelCount.ToString();
                    info.ModifyContent.Add(levelItem);
                    var emailItem = new AuditItem()
                    {
                        TargetSetting = "RM_RDM_WorkFlow_Notification",
                    };
                    var steps = oldWorkflow.StepInfo.WorkflowNodes.Where(step => step.NodeType == WorkflowNodeType.BeginDisposalReview || step.NodeType == WorkflowNodeType.DisposalReview);
                    var stepInfo = string.Empty;
                    foreach(var step in steps)
                    {
                        if(step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Default)
                        {
                            stepInfo += $"{step.DisplayName}({RMWorkflowStepUsedEmailTemplateMode.Default})" + '\n';
                        }
                        else if(step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Specify)
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
                    emailItem.OldValue = stepInfo;
                    info.ModifyContent.Add(emailItem);
                }
            }
            else if (action == (int)AuditAction.DeleteWorkflow)
            {
                info.Action = AuditAction.DeleteWorkflow;
                Guid workFlowId = (Guid)args[0];
                var workFlow = ManualProcessManagementService.LoadProcess(workFlowId);
                info.Object = workFlow.Name;
            }
            return info;
        }
    }
}
