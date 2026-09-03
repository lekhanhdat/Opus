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
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.RACommonUtility
{
    public class WorkflowAnalyzer
    {
        private WorkflowDefinitionDto workflowDefinition;

        private List<RMWorkflowStepNode> Steps;
        private RMWorkflowStepNode parentStep;
        private RMWorkflowStepNode currentStep;
        private List<RMWorkflowStepNode> nextSteps;
        
        public WorkflowAnalyzer(WorkflowDefinitionDto definition)
        {
            this.workflowDefinition = definition;
            this.Steps = definition.Content.WorkflowNodes;
            currentStep = definition.Content.WorkflowNodes.First(a => a.NodeType == WorkflowNodeType.Start);
            nextSteps = definition.Content.WorkflowNodes.Where(a => currentStep.ChildrenIds.Contains(a.Id)).ToList();
            parentStep = definition.Content.WorkflowNodes.FirstOrDefault(a => a.Id == currentStep.ParentId);

        }
        public WorkflowAnalyzer(WorkflowDefinitionDto definition, Guid stepId)
        {
            this.workflowDefinition = definition;
            this.Steps = definition.Content.WorkflowNodes;
            currentStep = definition.Content.WorkflowNodes.FirstOrDefault(a => a.Id == stepId);
            if(currentStep == null)
            {
                throw new ArgumentNullException("invilid stepId");
            }
            nextSteps = definition.Content.WorkflowNodes.Where(a => currentStep.ChildrenIds.Contains(a.Id)).ToList();
            parentStep = definition.Content.WorkflowNodes.FirstOrDefault(a => a.Id == currentStep.ParentId);
        }

        public RMWorkflowStepNode WaitingForApprove()
        {
            if(currentStep.NodeType == WorkflowNodeType.Start)
            {
                RMWorkflowStepNode node = Steps.Where(a => currentStep.ChildrenIds.Contains(a.Id)).First(c => c.NodeType == WorkflowNodeType.BeginDisposalReview || c.NodeType == WorkflowNodeType.DisposalReview);
                return node;
            }
            else
            {

                var start = Steps.First(a => a.NodeType == WorkflowNodeType.Start); 
                RMWorkflowStepNode node = Steps.Where(a => start.ChildrenIds.Contains(a.Id)).First(c => c.NodeType == WorkflowNodeType.BeginDisposalReview || c.NodeType == WorkflowNodeType.DisposalReview);
                return node;
            }
        }

        public RMWorkflowStepNode Approve()
        {
            return nextSteps.Where(a => a.NodeType == WorkflowNodeType.DisposalReview || a.NodeType == WorkflowNodeType.Destroy).First();
        }
        public RMWorkflowStepNode Reject()
        {
            return nextSteps.Where(a => a.NodeType == WorkflowNodeType.DisposalReview || a.NodeType == WorkflowNodeType.NotDestroy).First();
        }

        public RMWorkflowStepNode GetCurrentStep()
        {
            return currentStep;
        }

        public RMWorkflowStepNode FinalStep()
        {
            return Steps.First(a=>a.NodeType == WorkflowNodeType.End);
        }

        public bool CheckWorkflowHasStepUseSiteOwnerReviewer()
        { 
            return Steps.Exists(item => item.ReviewerType == WorkflowReviewerType.SiteOwners); 
        }
    }
}
