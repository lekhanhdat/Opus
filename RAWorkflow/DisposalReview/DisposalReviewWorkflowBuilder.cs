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
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.Workflow.Builder.Interface;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Workflow.DisposalReview
{
    internal sealed class DisposalReviewWorkflowBuilder : IWorkflowBuilder
    {

        public const string ArgRequestInfoName = "argRequestInfo";
        public const string VarRequestInfoName = "varRequestInfo";
        public const string VarApproveActionName = "approveAction";
        private const string activityBuilderName = "AvePoint.RA.Workflow.DisposalReview.Activities.DisposalReviewActivitybuilder";
        
        private bool isFirstDisposalReviewActivityNode = true;

        private WorkflowDefinitionDto definitionDto;

        public DisposalReviewWorkflowBuilder(WorkflowDefinitionDto dto)
        {
            if (dto.Type != Contract.RMWeb.RMWorkflowType.DisposalReview) 
                throw new Exception("Workflow type is wrong, it should be type of DisposalReview");

            this.definitionDto = dto;
        }

        public ActivityBuilder BuildActivityBuilder()
        {
            var activity = BuildActivity();

            ActivityBuilder activityBuilder = new ActivityBuilder();
            activityBuilder.Properties.Add(new DynamicActivityProperty() { Name = ArgRequestInfoName, Type = typeof(InArgument<DisposalReviewRequestInfo>) });
            
            activityBuilder.Name = activityBuilderName;
            activityBuilder.Implementation = activity;

            return activityBuilder;
        }

        public Activity BuildActivity()
        {
            Flowchart chart = new Flowchart();
            //variables
            var approveAction = new Variable<DisposalReviewActionEnum>(VarApproveActionName);
            var varRequestInfo = new Variable<DisposalReviewRequestInfo>(VarRequestInfoName);

            chart.Variables.Add(approveAction);
            chart.Variables.Add(varRequestInfo);

            //get the start node

            var startNode = definitionDto.Content.WorkflowNodes.FirstOrDefault(o => o.NodeType == WorkflowNodeType.BeginDisposalReview);

            if (startNode == null) throw new Exception("No start node found in the workflow definition");

            FlowStep startFlowStep = Configure(startNode, definitionDto.Content.WorkflowNodes);
            chart.Nodes.Add(startFlowStep);
            chart.StartNode = startFlowStep;

            return chart;

        }

        private FlowStep Configure(WorkflowNode curNode, List<WorkflowNode> allNodes)
        {
            FlowStep flowStep = new FlowStep();
            flowStep.Action = CreateActivity(curNode);

            if (curNode.ChildrenIds != null)
            {
                var children = allNodes.Where(o => o.NodeType != WorkflowNodeType.End && curNode.ChildrenIds.Contains(o.Id));
                if (children != null)
                {
                    if (children.Count() == 1)
                    {
                        FlowStep subStep = Configure(children.First(), allNodes);
                        flowStep.Next = subStep;
                    }
                    else if (children.Count() > 1)
                    {
                        var flowSwitch = new FlowSwitch<DisposalReviewActionEnum>();
                        flowSwitch.Expression = new VisualBasicValue<DisposalReviewActionEnum>(VarApproveActionName);
                        foreach (var child in children)
                        {
                            var actionEnum = child.Status == WorkflowNodeStatus.Reject ? DisposalReviewActionEnum.Reject : DisposalReviewActionEnum.Approve;

                            FlowStep step = Configure(child, allNodes);
                            flowSwitch.Cases.Add(actionEnum, step);
                        }

                        flowStep.Next = flowSwitch;
                    }
                }
            }

            return flowStep;
        }

        private Activity CreateActivity(WorkflowNode curNode)
        {
            Activity activity = null;
            if (curNode.NodeType == WorkflowNodeType.BeginDisposalReview || curNode.NodeType == WorkflowNodeType.DisposalReview)
            {
                activity = DisposalReviewActivityConverter.Convert(curNode, isFirstDisposalReviewActivityNode == false);
                isFirstDisposalReviewActivityNode = false;
            }
            else if (curNode.NodeType == WorkflowNodeType.Destroy)
            {
                activity = DestoryActivityConverter.Convert(curNode);
            }
            else if (curNode.NodeType == WorkflowNodeType.NotDestroy)
            {
                activity = DoNotDestoryActivityConverter.Convert(curNode);

            }

            return activity;
        }

    }
}
