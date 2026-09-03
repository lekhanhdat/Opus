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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWStateMachineActionProcessor : NWActionProcessorBase
    {
        private Dictionary<string, State> stateNameIdMapping = new Dictionary<string, State>(StringComparer.OrdinalIgnoreCase);

        public NWStateMachineActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#StateMachine";
        }

        public override WorkflowAction UpgradeWorkflowAction(Native13NinTexWorkflowEntity.NWActionConfig nwActionConfig)
        {
            InitlizeStateNameIdMapping(nwActionConfig.FieldReferences);
            var workflowAction = base.UpgradeWorkflowAction(nwActionConfig);
            workflowAction.Children = GetChildWorkflowAction(sourceConfig.ChildActivities);
            return workflowAction;
        }



        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.StateConfiguration = new StateConfiguration
            {
                States = stateNameIdMapping.Values.ToArray(),
            };
            return configuration;
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.DesignerType = "StateMachine";
            property.DisplayName = "Initial State";
            property.ID = "initialState";
            if (sourceConfig.Parameters != null)
            {
                var result = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "InitialState", false);
                if (result != null)
                {
                    property.Parameters = CreateParameters(result);
                }
            }
            return new List<Property>() { property };
        }


        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374429",
                ClassName = CLASSNAME,
                x49x49 = 294,
                y49x49 = 79,
                x30x30 = 294,
                y30x30 = 128,
                x16x16 = 327,
                y16x16 = 128,
            };
        }

        private List<WorkflowAction> GetChildWorkflowAction(NWActionConfig[] childActivities)
        {
            List<WorkflowAction> workflowActions = new List<WorkflowAction>();
            foreach (var child in childActivities)
            {
                var workflowAction = CreateChildSequenceActivityWorkflowAction(child.ChildActivities[0]);
                workflowActions.Add(workflowAction);
            }
            return workflowActions;
        }

        private WorkflowAction CreateChildSequenceActivityWorkflowAction(NWActionConfig activityAction)
        {
            var workflowAction = this.workflowActionProcessor.WorkflowActionAdapter.CreateSequenceActivityWorkflowAction();
            if (activityAction.Type.Equals("Nintex.Workflow.Activities.Adapters.NWState2Adapter", StringComparison.OrdinalIgnoreCase))
            {
                var result = activityAction.Parameters.Single(parameter => parameter.Name.Equals("State", StringComparison.OrdinalIgnoreCase));
                workflowAction.Id = stateNameIdMapping[result.PrimitiveValue.Value].ToString();
                workflowAction.Configuration.Name = result.PrimitiveValue.Value;
                workflowAction.Configuration.Id = Guid.NewGuid().ToString();
                if (activityAction.ChildActivities != null && activityAction.ChildActivities.Length > 0)
                {
                    AddChild(workflowAction, activityAction.ChildActivities);
                }
                return workflowAction;
            }
            throw new Exception(string.Format("Not expected data. action type: {0}", activityAction.Type));
        }

        private void AddChild(WorkflowAction parent, NWActionConfig[] activityActions)
        {
            foreach (var action in activityActions)
            {
                if (action.Type.Equals("Nintex.Workflow.Activities.Adapters.WFSequenceAdapter", StringComparison.OrdinalIgnoreCase))
                {
                    this.workflowActionProcessor.AddChildrenWorkflowAction(parent, action.ChildActivities);
                }
            }
        }

        private void InitlizeStateNameIdMapping(NWFieldReference[] fieldReferences)
        {
            foreach (var fieldReference in fieldReferences)
            {
                var state = new State() { Id = Guid.NewGuid().ToString(), DisplayName = fieldReference.Name };
                stateNameIdMapping[fieldReference.Name] = state;
            }
        }

        private Parameters[] CreateParameters(ActivityParameter activityParameter)
        {
            List<Parameters> parameters = new List<Parameters>();
            Parameters parameter = new Parameters();
            parameter.Name = "initialState";
            parameter.Required = true;
            parameter.DataType = "DynamicValue";
            parameter.DesignerType = "StateMachine";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue() { PrimitiveValue = GetPrimitiveValue(activityParameter.PrimitiveValue) };
            parameters.Add(parameter);
            return parameters.ToArray();
        }

        private PrimitiveValue GetPrimitiveValue(Native13NinTexWorkflowEntity.PrimitiveValue sourcePrimitiveValue)
        {
            PrimitiveValue primitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(sourcePrimitiveValue, base.workflowActionProcessor, true);

            //由于locla备份出来的initialState value是小写的，与State的name不一定相同，因此需要使用State的displayName
            if (stateNameIdMapping.ContainsKey(primitiveValue.Value.StringValue))
            {
                primitiveValue.Value.StringValue = stateNameIdMapping[primitiveValue.Value.StringValue].DisplayName;
            }
            return primitiveValue;
        }
    }
}
