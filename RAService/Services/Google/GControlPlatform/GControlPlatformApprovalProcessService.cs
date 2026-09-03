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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Services;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Foundation;

namespace AvePoint.RA.Service.Services.Google.GControlPlatform;

public class GControlPlatformApprovalProcessService : GControlPlatformBaseService, IGControlPlatformApprovalProcessService
{
    public async Task<WorkflowDefinitionDto> GetPlatformApprovalProcess(Guid id)
    {
        using var performance = new PerformanceScope($"GControlPlatformApprovalProcessService.GetPlatformApprovalProcess-{id}");
        try
        {
            Logger.Info($"Getting approval process with id {id}");
            var result = await GControlPlatformApiClient.ApprovalProcessService.GetApprovalProcess(id);
            return ConvertToDefinitionDto(result);
        }
        catch (Exception ex)
        {
            Logger.Error($"Have a problem when getting approval process by id {id}, Exception: {ex}");
        }

        return new();
    }

    public async Task<bool> CreatePlatformRequest(WorkflowDefinitionDto platformApprovalProcess)
    {
        try
        {
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Have a problem when creating approval process, Exception: {ex}");
            return false;
        }
    }

    public async Task<bool> UpdatePlatformApprovalProcess(Guid id, WorkflowDefinitionDto platformApprovalProcess)
    {
        try
        {
            return await GControlPlatformApiClient.ApprovalProcessService.UpdateApprovalProcess(id, new());
        }
        catch (Exception ex)
        {
            Logger.Error($"Have a problem when updating approval process, Exception: {ex}");
            return false;
        }
    }

    public async Task<List<WorkflowDefinitionDto>> SearchPlatformApprovalProcesses(CommonRequest request)
    {
        try
        {
            var result =  await GControlPlatformApiClient.ApprovalProcessService
                .SearchApprovalProcesses(request);
            return result.Select(ConvertToDefinitionDto).ToList();
        }
        catch (Exception ex)
        {
            Logger.Error($"Have a problem when searching approval process, Exception: {ex}");
            return [];
        }
    }
    
    private WorkflowDefinitionDto ConvertToDefinitionDto(ApprovalProcess platformApprovalProcess)
    {
        var result = new WorkflowDefinitionDto()
        {
            Id = platformApprovalProcess.Id,
            Name = platformApprovalProcess.Name,
            Description = platformApprovalProcess.Description,
            ReferenceId = platformApprovalProcess.Id,
            Content = ConvertToDefinitionContent(platformApprovalProcess.ApprovalStages)
        };
        result.Content.WorkflowNodes.ForEach(workflowNode =>
        {
            workflowNode.UsedEmailTemplateId = platformApprovalProcess.AssignedEmailTemplateId;
            workflowNode.UsedEmailTemplateMode = RMWorkflowStepUsedEmailTemplateMode.Specify;
        } );
        return result;
    }

    private RMWorkflowContentDto ConvertToDefinitionContent(List<ApprovalStage> approvalStages)
    {
        return new()
        {
            WorkflowNodes = ConvertToWorkflowNode(approvalStages)
        };
    }

    private List<RMWorkflowStepNode> ConvertToWorkflowNode(List<ApprovalStage> approvalStages)
    {
        approvalStages = approvalStages.OrderBy(stage => stage.Order).ToList();
        var order = 1;
        var result = CreateBasicWorkflowStepsNode(ref order, approvalStages.First());
        for (int i = 0; i < approvalStages.Count; i++)
        {
            order++;
            var currentStage = approvalStages[i];
            if (i == approvalStages.Count - 1)
            {
                AddLastStepNodes(result, currentStage, i, order);
                break;
            }
            result.Add(new RMWorkflowStepNode
            {
                Id = currentStage.Id,
                ChildrenIds = [approvalStages[i+1].Id],
                NodeType = i == 0 ? WorkflowNodeType.BeginDisposalReview : WorkflowNodeType.DisposalReview,
                Status = WorkflowNodeStatus.None,
                DisplayName = currentStage.Approver.ApproverId,
                Name = order.ToString(),
                ParentId = result[i].Id,
                ReviewerType = WorkflowReviewerType.RecordUsers,
                Reviewers = [new ReviewerUser()
                {
                    InviteType = currentStage.Approver.IsUser ? RMActiveDirectoryObjectType.User : RMActiveDirectoryObjectType.Group,
                    UserId = currentStage.Approver.ApproverId
                }]
            });
        }

        return result;
    }

    private void AddLastStepNodes(List<RMWorkflowStepNode> result, ApprovalStage currentStage, int i ,int order )
    {
        var destroyStepId = Guid.NewGuid();
        var notDestroyStepId = Guid.NewGuid();
        result.Add(new RMWorkflowStepNode
        {
            Id = currentStage.Id,
            NodeType = i == 0 ? WorkflowNodeType.BeginDisposalReview : WorkflowNodeType.DisposalReview,
            Status = WorkflowNodeStatus.ApproveOrReject,
            DisplayName = currentStage.Approver.ApproverId,
            Name = order.ToString(),
            ParentId = result[i].Id,
            ChildrenIds = [destroyStepId, notDestroyStepId],
            ReviewerType = WorkflowReviewerType.RecordUsers,
            Reviewers =
            [
                new ReviewerUser
                {
                    InviteType = currentStage.Approver.IsUser ? RMActiveDirectoryObjectType.User : RMActiveDirectoryObjectType.Group,
                    UserId = currentStage.Approver.ApproverId
                }
            ]
        });
        result.AddRange([
            new RMWorkflowStepNode
            {
                Id = destroyStepId,
                NodeType = WorkflowNodeType.Destroy,
                Name = order.ToString(),
                DisplayName = destroyStepId.ToString(),
                ParentId = currentStage.Id,
                Status = WorkflowNodeStatus.Approve
            },
            new RMWorkflowStepNode
            {
                Id = notDestroyStepId,
                NodeType = WorkflowNodeType.NotDestroy,
                Name = order.ToString(),
                DisplayName = notDestroyStepId.ToString(),
                ParentId = currentStage.Id,
                Status = WorkflowNodeStatus.Reject
            }
        ]);
    }

    private List<RMWorkflowStepNode> CreateBasicWorkflowStepsNode(ref int order, ApprovalStage firstApprovalStage)
    {
        var firstStepGuidId = Guid.NewGuid();
        List<RMWorkflowStepNode> result =
        [
            new RMWorkflowStepNode
            {
                Id = firstStepGuidId,
                NodeType = 0,
                Name = order.ToString(),
                DisplayName = firstStepGuidId.ToString(),
                ChildrenIds = [firstApprovalStage.Id],
                Status = WorkflowNodeStatus.None
            },
        ];
        return result;
    }
}