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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWSetVariableProcessor : NWActionProcessorBase
    {
        private string variableDataType;

        public NWSetVariableProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#SetMultiVariablesActivity";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374393",
                ClassName = CLASSNAME,
                x49x49 = 196,
                y49x49 = 79,
                x30x30 = 196,
                y30x30 = 128,
                x16x16 = 229,
                y16x16 = 128
            };
        }

        protected override List<Property> CreateProperties()
        {
            return new List<Property> { CreateVariableProperty() };
        }

        private Property CreateVariableProperty()
        {
            var sourceVariableProperty = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "VariableName", true); 
            var variableProperty = new Property
            {
                ID = "multivariables",
                DesignerType = "MultiVariables",
                DisplayName = "Value",
            };
            var parameterTo = new Parameters
            {
                Name = "To",
                Description = "Variable to assign a value to.",
                Required = true,
                DataType = "Any",
                DesignerType = "Variable",
                Direction = "Output",
                Value = base.ConvertParameterValue(sourceVariableProperty),
            };
            variableDataType = parameterTo.Value.Variable.DataType;
            variableProperty.Parameters = new Parameters[] { parameterTo, CreateValueParameters() };
            return variableProperty;
        }

        private Parameters CreateValueParameters()
        {
            var sourceValueProperty = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Value", true);
            var parameterValue = new Parameters
            {
                Name = "Value",
                Description = "Value used to set the workflow variable.",
                Required = true,
                DataType = "Any",
                Direction = "Input",
                DependentOn = "To",
                Value = (string.Equals("CurrentDate", sourceValueProperty.SpecialReference, StringComparison.OrdinalIgnoreCase) || string.Equals("CurrentDateTime", sourceValueProperty.SpecialReference, StringComparison.OrdinalIgnoreCase))
                        ? new ParametersValue { PrimitiveValue = new PrimitiveValue { Type = "DateTime", Value = new Value { DateTimeInfo = new DateTimeInfo { UseCurrentDate = true } } } } :
                          ConvertParameterValue(sourceValueProperty),
            };

            return parameterValue;
        }

        private ParametersValue CreateTextVariableParametersValue(ParametersValue text)
        {
            var parameterValue = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue
                {
                    Type = "String",
                    Value = new Value("{0}"),
                    FormatValues = new List<FormatValues>(),
                },
            };
            SelectedValue selectedValue = new SelectedValue
            {
                Coercion = "AsDNString",
            };
            selectedValue.Variable = text.Variable;
            selectedValue.WorkflowContext = text.WorkflowContext;
            selectedValue.ListLookup = text.ListLookup;

            parameterValue.PrimitiveValue.FormatValues.Add(new FormatValues { SelectedValue = selectedValue });
            return parameterValue;
        }

        public string GetParameterDataType(ParametersValue parameters)
        {
            if (parameters.PrimitiveValue != null)
            {
                return parameters.PrimitiveValue.Type;
            }
            else if (parameters.ListLookup != null)
            {
                return parameters.ListLookup.SelectFieldType;
            }
            else if (parameters.Variable != null)
            {
                return parameters.Variable.DataType;
            }
            else if (parameters.WorkflowContext != null)
            {
                return parameters.WorkflowContext.Type;
            }
            return string.Empty;
        }

        protected override ParametersValue ConvertParameterValue(ActivityParameter activityParameter)
        {
            var parametersValue = base.ConvertParameterValue(activityParameter);
            parametersValue.Coercion = NWCoercionStringProcessor.GenerateCoercionString(variableDataType, GetParameterDataType(parametersValue), activityParameter.Coercion == null ? null : activityParameter.Coercion.Value);

            if (parametersValue.PrimitiveValue != null && parametersValue.PrimitiveValue.Value != null)
            {
                parametersValue.PrimitiveValue.Value.StringValue = AveHtmlUtility.HtmlDecode(parametersValue.PrimitiveValue.Value.StringValue);
            }

            var sourceVariableProperty = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "VariableName", true);

            //variale 为text类型时 数据结构比较特殊
            if (parametersValue.PrimitiveValue == null
             && sourceVariableProperty.Variable.Type.Equals("Text", StringComparison.OrdinalIgnoreCase))
            {
                parametersValue = CreateTextVariableParametersValue(parametersValue);
            }

            return parametersValue;
        }

    }
}
