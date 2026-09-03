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
    class NWSwitchActionProcessor : NWActionProcessorBase
    {
        public NWSwitchActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#Switch";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            var workflowAction = base.UpgradeWorkflowAction(nwActionConfig);
            workflowAction.Children = GenerateChildren(sourceConfig.ChildActivities);
            return workflowAction;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374433",
                ClassName = CLASSNAME,
                x49x49 = 343,
                y49x49 = 79,
                x30x30 = 343,
                y30x30 = 128,
                x16x16 = 376,
                y16x16 = 128
            };
        }

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.StateConfiguration = CreateStateConfiguration();
            return configuration;
        }

        private List<WorkflowAction> GenerateChildren(NWActionConfig[] childActivities)
        {
            List<WorkflowAction> workflowActions = new List<WorkflowAction>();
            foreach (var child in childActivities)
            {
                var workflowAction = base.workflowActionProcessor.WorkflowActionAdapter.UpgradeWorkflowAction(child);
                var name = child.Parameters.First(item => item.Name.Equals("Value")).PrimitiveValue.Value;
                workflowAction.Configuration.Name = IsOtherNode(name) ? "Other" : name;

                workflowActions.Add(workflowAction);
            }
            return workflowActions;
        }

        private StateConfiguration CreateStateConfiguration()
        {
            var stateConfiguration = new StateConfiguration();
            List<State> states = new List<State>();
            foreach (var fieldReference in this.sourceConfig.FieldReferences)
            {
                var state = new State();
                if (IsOtherNode(fieldReference.Name))
                {
                    state.Id = string.Format("other_{0}", Guid.NewGuid());
                    state.DisplayName = "Other";
                }
                else
                {
                    state.Id = Guid.NewGuid().ToString();
                    state.DisplayName = fieldReference.Name;
                }
                states.Add(state);
            }

            stateConfiguration.States = states.ToArray();
            return stateConfiguration;
        }

        protected override List<Property> CreateProperties()
        {
            var properties = new List<Property>();
            properties.Add(CreateSwitchVariableInput(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "SwitchVariable", true)));
            properties.Add(CreateSwitchValueProperty());
            return properties;
        }

        private Property CreateSwitchVariableInput(ActivityParameter switchVariableParameter)
        {
            var property = new Property();
            property.ID = "switchVariableInput";
            property.DesignerType = "Text";
            property.DisplayName = "Select the variable to evaluate";
            var parameters = new Parameters();
            parameters.Name = "switchVariableInput";
            parameters.Description = "The workflow variable that determines which child branch to run.";
            parameters.Required = true;
            parameters.DataType = "String";
            parameters.DesignerType = "Text";
            parameters.Direction = "Input";
            parameters.Value = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue
                {
                    Type = "String",
                    Value = new Value("{0}"),
                    FormatValues = CreateFormatValues(switchVariableParameter),
                },
            };
            property.Parameters = new Parameters[] { parameters };
            return property;
        }

        private List<FormatValues> CreateFormatValues(ActivityParameter switchVariableParameter)
        {
            var selectedValue = new SelectedValue
            {
                Coercion = "AsDNString",
            };

            if (switchVariableParameter.ListLookup != null)
            {
                selectedValue.ListLookup = NWListLookupConverter.ConvertListLookup(switchVariableParameter.ListLookup, base.workflowActionProcessor);
            }
            else
            {
                selectedValue.Variable = base.workflowActionProcessor.VariablesCacheManager.GetVariable(switchVariableParameter.Variable.Name, true);
            }
            return new List<FormatValues> { new FormatValues { SelectedValue = selectedValue } };
        }
        private Property CreateSwitchValueProperty()
        {
            var property = new Property();
            property.ID = "switchValue";
            property.DesignerType = "Switch";
            property.DisplayName = "Switch Value";
            property.Parameters = CreateSwitchValueParameters();
            return property;
        }

        private Parameters[] CreateSwitchValueParameters()
        {
            var switchValue = GenerateSwitchValueParametersBaicProperty("switchValue");
            switchValue.Value.PrimitiveValue.Value = new Value(IncludeOther().ToString());
            var logOtherValue = GenerateSwitchValueParametersBaicProperty("logOtherValue");
            logOtherValue.Value.PrimitiveValue.Value = new Value(false.ToString());
            return new Parameters[]
            {
                switchValue,
                logOtherValue,
            };
        }

        private Parameters GenerateSwitchValueParametersBaicProperty(string name)
        {
            return new Parameters
            {
                Name = name,
                Required = false,
                DataType = "Boolean",
                DesignerType = "Switch",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Boolean", }
                }
            };
        }

        private bool IncludeOther()
        {
            return this.sourceConfig.FieldReferences.FirstOrDefault(item => IsOtherNode(item.Name)) != null;
        }

        private bool IsOtherNode(string name)
        {
            return string.Compare("__NWDEFAULTBRANCH__", name, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
