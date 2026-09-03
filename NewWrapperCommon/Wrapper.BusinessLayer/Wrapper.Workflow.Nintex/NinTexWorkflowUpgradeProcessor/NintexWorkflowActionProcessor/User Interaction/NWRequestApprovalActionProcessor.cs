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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    enum RequestApprovalApprovalType
    {
        AllMustApprove = 0,
        FirstToRespond = 1,
        AnyApprove = 2,
        Vote = 3,
    }

    class NWRequestApprovalActionProcessor : NWStartATaskProcessActionBase
    {
        AveLogger logger = AveLogger.GetInstance(typeof(NWSendEmailActionProcessor));
        private double totalApproverCount = 0;
        private double needApproverCount = -1;
        private RequestApprovalApprovalType approvalType = RequestApprovalApprovalType.AllMustApprove;

        public NWRequestApprovalActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
        }

        protected override Parameters[] CreateParameters()
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.Add(CreateAssignToParameter());
            parameters.Add(CreateDueDateParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskDueDate", true)));
            parameters.Add(CreateTitleParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskName", true)));
            parameters.Add(CreateDescriptionParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskDescription", true)));
            parameters.Add(CreateCompletionCriteriaParameter());
            parameters.Add(CreateCompletionCriteriaPropertiesParameter());
            parameters.Add(CreateExpandGroupParameter());

            parameters.AddRange(CreateTaskNotificationRelevantParameters());
            parameters.AddRange(CreateNotRequiredNotificationRelevantParameters());

            parameters.AddRange(GetAllNoNeedConvertParameters());

            parameters = SortParametersList(parameters);
            return parameters.ToArray();
        }

        private List<Parameters> GetAllNoNeedConvertParameters()
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.AddRange(requestActionUtility.GetCommonNoNeedConvertParameters(base.workflowActionProcessor.List == null));

            parameters.Add(requestActionUtility.CreateRelatedContentTypeIdParameter(true));
            parameters.Add(requestActionUtility.CreateOutcomeFieldNameParameter(true));
            parameters.Add(requestActionUtility.CreateSendReminderEmailParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateDefaultTaskOutcomeParameter("0"));
            parameters.Add(requestActionUtility.CreateOverdueReminderRepeatParameter());
            parameters.Add(requestActionUtility.CreateOverdueRepeatTimesParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateOverdueEmailSubjectParameter(workflowActionProcessor, sourceConfig, true));
            parameters.Add(requestActionUtility.CreateOverdueEmailBodyParameter(workflowActionProcessor, sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationTypeParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationDateParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationDateCalculationUnitParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationDateCalculationValueParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationOutcomeParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationToParameter(workflowActionProcessor, sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationCCParameter(sourceConfig, true));
            parameters.Add(requestActionUtility.CreateEscalationEmailSubjectParameter());
            parameters.Add(requestActionUtility.CreateEscalationEmailBodyParameter());

            return parameters;
        }

        private void InitializeStateConfiguration()
        {
            stateConfiguration = new StateConfiguration()
            {
                States = new[]
                {
                    new State() {Id=Guid.NewGuid().ToString(),DisplayName = Rejected},
                    new State() { Id=Guid.NewGuid().ToString(),DisplayName = Approved}
                }
            };
        }

        private void InitializeApproverCount(NWActionConfig nwActionConfig)
        {
            //first approver is default approver xmlelement, is not a true approver.
            totalApproverCount = nwActionConfig.Approvers.Length - 1;
            var data = nwActionConfig.Parameters.First(para => string.Equals(para.Name, "ApprovalTypeData", StringComparison.OrdinalIgnoreCase));
            if (!Double.TryParse(data.PrimitiveValue.Value, out needApproverCount))
            {
                needApproverCount = -1;
            }
        }

        protected override void InitializeData(NWActionConfig nwActionConfig)
        {
            approvalType = requestActionUtility.GetApprovalType<RequestApprovalApprovalType>(nwActionConfig, "ApprovalType");
            InitializeApproverCount(nwActionConfig);
            InitializeStateConfiguration();
            defaultValue = 1;
        }

        protected override List<WorkflowAction> GenerateChildrenWorkflowAction(NWActionConfig[] childActivities)
        {
            List<WorkflowAction> workflowActions = new List<WorkflowAction>();
            var leftChildWorkflwAction = this.workflowActionProcessor.WorkflowActionAdapter.UpgradeWorkflowAction(childActivities[0]);
            leftChildWorkflwAction.Id = stateConfiguration.States[0].Id;
            leftChildWorkflwAction.Configuration.Name = Rejected;

            var rightChildWorkflwAction = this.workflowActionProcessor.WorkflowActionAdapter.UpgradeWorkflowAction(childActivities[1]);
            rightChildWorkflwAction.Id = stateConfiguration.States[1].Id;
            rightChildWorkflwAction.Configuration.Name = Approved;

            workflowActions.Add(leftChildWorkflwAction);
            workflowActions.Add(rightChildWorkflwAction);

            return workflowActions;
        }

        protected override DictionaryValue[] GetCompletionCriteriaDictionaryValue()
        {
            switch (approvalType)
            {
                case RequestApprovalApprovalType.AnyApprove:
                    return GetCompletionCriteriaDictionaryValue("0", 0);
                case RequestApprovalApprovalType.Vote:
                    return GetCompletionCriteriaDictionaryValue("0", needApproverCount == -1 ? 50 : (int)(needApproverCount / totalApproverCount * 100));
                case RequestApprovalApprovalType.AllMustApprove:
                    return GetCompletionCriteriaDictionaryValue("0", 100);
                case RequestApprovalApprovalType.FirstToRespond:
                default:
                    return new DictionaryValue[] { };
            }
        }

        protected override int GetCompletionCriteriaValue()
        {
            switch (approvalType)
            {
                case RequestApprovalApprovalType.Vote:
                case RequestApprovalApprovalType.AllMustApprove:
                    //AllMustApprove不同于Online的 wait for all responses,
                    //AllMustApprove是需要所有user approve，而wait for all responses
                    //只是需要一半以上的人有相同的选择即可
                    return (int)StartTaskProcessApproveType.WaitForPercentageOfAResponse;
                case RequestApprovalApprovalType.AnyApprove:
                    return (int)StartTaskProcessApproveType.WaitForSpecificResponse;
                case RequestApprovalApprovalType.FirstToRespond:
                    return (int)StartTaskProcessApproveType.WaitForFirstResponse;
                default:
                    return (int)StartTaskProcessApproveType.WaitForAllResponses;
            }
             
        }        
    }
}
