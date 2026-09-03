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
using System.Text;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    enum ReviewType
    {
        AllMustApprove,
        AnyApprove,
    }

    class NWAssignToDoTaskActionProcessor : NWStartATaskProcessActionBase
    {
        private ReviewType approvalType = ReviewType.AllMustApprove;

        public NWAssignToDoTaskActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            useDefaultOutcome = false;
        }

        private WorkflowAction CreateChildWorkflowAction(string id, string name)
        {
            var childWorkflowAction = base.workflowActionProcessor.WorkflowActionAdapter.CreateSequenceActivityWorkflowAction();
            childWorkflowAction.Id = id;
            childWorkflowAction.Configuration.Name = name;
            return childWorkflowAction;
        }

        protected override List<WorkflowAction> GenerateChildrenWorkflowAction(NWActionConfig[] childActivities)
        {
            return new List<WorkflowAction>()
            {
                CreateChildWorkflowAction(stateConfiguration.States[0].Id,stateConfiguration.States[0].DisplayName),
                CreateChildWorkflowAction(stateConfiguration.States[1].Id,stateConfiguration.States[1].DisplayName),
            };
        }

        protected override DictionaryValue[] GetCompletionCriteriaDictionaryValue()
        {
            return new DictionaryValue[] { };
        }

        protected override int GetCompletionCriteriaValue()
        {
            switch (approvalType)
            {
                case ReviewType.AnyApprove:
                    return (int)StartTaskProcessApproveType.WaitForFirstResponse;
                case ReviewType.AllMustApprove:
                default:
                    return (int)StartTaskProcessApproveType.WaitForAllResponses;
            }
        }

        private void InitializeStateConfiguration()
        {
            stateConfiguration = new StateConfiguration()
            {
                States = new[]
                {
                    new State() {Id=Guid.NewGuid().ToString(),DisplayName = "NoUsebranch1"},
                    new State() { Id=Guid.NewGuid().ToString(),DisplayName = "NoUsebranch2"}
                }
            };
        }

        protected override void InitializeData(NWActionConfig nwActionConfig)
        {
            InitializeStateConfiguration();
            approvalType = requestActionUtility.GetApprovalType<ReviewType>(nwActionConfig, "ReviewType");
        }

    }
}
