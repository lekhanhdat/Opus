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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    enum AssignFlexiTaskApprovalType
    {
        AllMustAgree,
        FirstToRespond,
        AllMustAgreeSpecific,
        Majority,
        MajoritySpecific,
    }

    class NWAssignFlexiTaskActionProcessor : NWStartATaskProcessActionBase
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(NWAssignFlexiTaskActionProcessor));
        private AssignFlexiTaskApprovalType approvalType = AssignFlexiTaskApprovalType.AllMustAgree;
        private string approvalTypeData = string.Empty;
        //On-premise为Reject与Approve而online为Rejected与Approved
        private Dictionary<string, string> approvalTypeDataMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "Reject", "Rejected" }, { "Approve", "Approved" } };

        public NWAssignFlexiTaskActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
        }
        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            if(IsSupportActionData(nwActionConfig))
            {
                throw new NotSupportedException("Can not support assign flexi task action with no branch");
            }

            return base.UpgradeWorkflowAction(nwActionConfig);
        }
        /// <summary>
        /// ADO-196313 Nintex online 不支持没有branch的action
        /// </summary>
        /// <param name="nwActionConfig"></param>
        /// <returns></returns>
        private bool IsSupportActionData(NWActionConfig nwActionConfig)
        {
            var usingBranches = NWCommonUtility.GetActivityParameterByName(nwActionConfig.Parameters, "UsingBranches", false);
            return usingBranches != null && string.Equals(bool.FalseString, NWCommonUtility.TryGetTheValueOfPrimitiveValue(usingBranches, bool.TrueString), StringComparison.OrdinalIgnoreCase);
        }
        private string GetMappingedName(string sourceName)
        {
            if (approvalTypeDataMapping.ContainsKey(sourceName))
            {
                return approvalTypeDataMapping[sourceName];
            }
            return sourceName;
        }

        private void InitializeApprovalTypeData(NWActionConfig nwActionConfig)
        {
            var result = nwActionConfig.Parameters.First(para => string.Equals(para.Name, "ApprovalTypeData", StringComparison.OrdinalIgnoreCase));
            approvalTypeData = GetMappingedName(result.PrimitiveValue.Value);
        }

        protected override void InitializeData(NWActionConfig nwActionConfig)
        {
            ConvertStateConfiguration(nwActionConfig);
            approvalType = requestActionUtility.GetApprovalType<AssignFlexiTaskApprovalType>(nwActionConfig, "ApprovalType");
            InitializeApprovalTypeData(nwActionConfig);
        }

        private void ConvertStateConfiguration(NWActionConfig nwActionConfig)
        {
            ConfiguredOutcomeCollection outcomCollection = nwActionConfig.Outcomes;
            var states = new List<State>();
            foreach (var outcome in outcomCollection)
            {
                var outcomeName = GetMappingedName(outcome.Name);
                if (!string.Equals(outcomeName, Approved, StringComparison.OrdinalIgnoreCase)
                 && !string.Equals(outcomeName, Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    useDefaultOutcome = false;
                }
                states.Add(new State { Id = Guid.NewGuid().ToString(), DisplayName = outcomeName });
            }
            if (UseOtherBranch(nwActionConfig))
            {
                states.Add(new State { Id = Guid.NewGuid().ToString(), DisplayName = "Other" });
            }

            stateConfiguration = new StateConfiguration
            {
                States = states.ToArray(),
            };
        }

        private bool UseOtherBranch(NWActionConfig nwActionConfig)
        {
            var parameter = nwActionConfig.Parameters.First(p => string.Equals(p.Name, "UsingOtherBranch", StringComparison.OrdinalIgnoreCase));
            return string.Equals(parameter.PrimitiveValue.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        private int FindOutcomeBranchIndex(string outcomeName)
        {
            if (string.IsNullOrEmpty(outcomeName))
            {
                return -1;
            }
            for (int branchIndex = 0; branchIndex < stateConfiguration.States.Length; branchIndex++)
            {
                if (string.Equals(stateConfiguration.States[branchIndex].DisplayName, outcomeName, StringComparison.OrdinalIgnoreCase))
                {
                    return branchIndex;
                }
            }
            logger.Warn("Can not find outcome, outcome name is {0}", outcomeName);
            return -1;
        }

        private string GetConfigurationName(NWActionConfig child)
        {
            var result = child.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, "Value", StringComparison.OrdinalIgnoreCase));
            if (result != null)
            {
                return GetMappingedName(result.PrimitiveValue.Value);
            }
            return string.Empty;

        }

        protected override List<WorkflowAction> GenerateChildrenWorkflowAction(NWActionConfig[] childActivities)
        {
            List<WorkflowAction> workflowActions = new List<WorkflowAction>();
            foreach (var child in childActivities)
            {
                var childWorkflowAction = this.workflowActionProcessor.WorkflowActionAdapter.UpgradeWorkflowAction(child);
                var configurationName = GetConfigurationName(child);
                State stateResult = GetStatebyName(configurationName);

                childWorkflowAction.Configuration.Name = stateResult.DisplayName;
                childWorkflowAction.Id = stateResult.Id;
                workflowActions.Add(childWorkflowAction);

            }
            return workflowActions;
        }

        private State GetStatebyName(string configurationName)
        {
            if (string.Equals("__NWDEFAULTBRANCH__", configurationName))
            {
                return stateConfiguration.States.First(state => string.Equals(state.DisplayName, "Other", StringComparison.OrdinalIgnoreCase));
            }
            return stateConfiguration.States.First(state => string.Equals(state.DisplayName, configurationName, StringComparison.OrdinalIgnoreCase));
        }

        protected override int GetCompletionCriteriaValue()
        {
            switch (approvalType)
            {
                case AssignFlexiTaskApprovalType.AllMustAgree:
                case AssignFlexiTaskApprovalType.MajoritySpecific:
                case AssignFlexiTaskApprovalType.AllMustAgreeSpecific:
                    return (int)StartTaskProcessApproveType.WaitForPercentageOfAResponse;
                case AssignFlexiTaskApprovalType.FirstToRespond:
                    return (int)StartTaskProcessApproveType.WaitForFirstResponse;
                case AssignFlexiTaskApprovalType.Majority:
                default:
                    return (int)StartTaskProcessApproveType.WaitForAllResponses;
            }
        }

        protected override DictionaryValue[] GetCompletionCriteriaDictionaryValue()
        {
            switch (approvalType)
            {
                case AssignFlexiTaskApprovalType.AllMustAgree:
                case AssignFlexiTaskApprovalType.AllMustAgreeSpecific:
                    return GetCompletionCriteriaDictionaryValue(FindOutcomeBranchIndex(approvalTypeData).ToString(), 100);
                case AssignFlexiTaskApprovalType.MajoritySpecific:
                    return GetCompletionCriteriaDictionaryValue(FindOutcomeBranchIndex(approvalTypeData).ToString(), 50);
                case AssignFlexiTaskApprovalType.FirstToRespond:
                case AssignFlexiTaskApprovalType.Majority:
                default:
                    return new DictionaryValue[] { };
            }
        }
    }
}
