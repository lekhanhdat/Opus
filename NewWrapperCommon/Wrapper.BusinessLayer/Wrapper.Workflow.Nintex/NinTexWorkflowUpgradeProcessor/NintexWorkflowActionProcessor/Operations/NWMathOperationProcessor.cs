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
    class NWMathOperationProcessor : NWActionProcessorBase
    {
        private static Dictionary<string, string> operatorMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "add","Add" },
                { "sub","Subtract" },
                { "times","Multiply" },
                { "divide","Divide" },
                { "mod","Mod" },
            };

        public NWMathOperationProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.Calc";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374404",
                ClassName = CLASSNAME,
                x49x49 = 294,
                y49x49 = 158,
                x30x30 = 294,
                y30x30 = 207,
                x16x16 = 327,
                y16x16 = 207,
            };
        }


        protected override List<Property> CreateProperties()
        {
            var properties = new List<Property>();
            properties.Add(CreateFirstOperand());
            properties.Add(CreateOperator());
            properties.Add(CreateSecondOperand());
            properties.Add(CreateOutput());
            return properties;
        }

        private void ReSetPrimitiveValueType(PrimitiveValue primitiveValue)
        {
            if (string.Equals("Int32", primitiveValue.Type, StringComparison.OrdinalIgnoreCase))
            {
                primitiveValue.Type = "Double";
            }
        }


        private bool IsDateTime(string type)
        {
            return string.Equals(type, "DateTime", StringComparison.OrdinalIgnoreCase);
        }

        private void CheckUnSupportData(Parameters parameter)
        {
            if (parameter.Value.PrimitiveValue != null && IsDateTime(parameter.Value.PrimitiveValue.Type))
            {
                throw new UnSupportedDataException("Parameters PrimitiveValue is DateTime.");
            }
            if (parameter.Value.ListLookup != null && IsDateTime(parameter.Value.ListLookup.SelectFieldType))
            {
                throw new UnSupportedDataException("Parameters ListLookup is DateTime.");
            }
            if (parameter.Value.Variable != null && IsDateTime(parameter.Value.Variable.DataType))
            {
                throw new UnSupportedDataException("Parameters Variable is DateTime.");
            }
        }

        private Property CreateOperator()
        {
            var sourceParameter = NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "Operator", true);
            Property operatorProperty = new Property();
            operatorProperty.ID = "p1";
            operatorProperty.DesignerType = "ChoiceList";
            operatorProperty.DisplayName = "Operator";
            Parameters parameter = new Parameters
            {
                Name = "Operator",
                Description = "Operator to use in calculation.",
                Required = true,
                DataType = "String",
                DesignerType = "ChoiceList",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceParameter),
                Options = CreateOptions(),
            };
            if (parameter.Value.PrimitiveValue != null && !string.IsNullOrEmpty(parameter.Value.PrimitiveValue.Value.StringValue))
            {
                parameter.Value.PrimitiveValue.Value = new Value(operatorMapping[parameter.Value.PrimitiveValue.Value.StringValue]);
            }
            operatorProperty.Parameters = new Parameters[] { parameter };
            return operatorProperty;
        }

        private Options[] CreateOptions()
        {
            List<Options> options = new List<Options>();
            options.Add(CreateOptions("plus", "Add", new TypeFilter { @string = "Double" }));
            options.Add(CreateOptions("minus", "Subtract", new TypeFilter { @string = "Double" }));
            options.Add(CreateOptions("multiply by", "Multiply", new TypeFilter { @string = "Double" }));
            options.Add(CreateOptions("divided by", "Divide", new TypeFilter { @string = "Double" }));
            options.Add(CreateOptions("mod", "Mod", new TypeFilter { @string = "Double" }));
            return options.ToArray();
        }

        private Options CreateOptions(string text, string value, TypeFilter typeFilter)
        {
            return new Options { Text = text, Value = value, Options1 = new Options[] { new Options { TypeFilter = typeFilter } } };
        }

        private Property CreateFirstOperand()
        {
            var sourceParameter = NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "Operand1", true);
            var firstOperand = CreateOperandProperty(sourceParameter);
            firstOperand.ID = "p0";
            firstOperand.DisplayName = "First operand";
            firstOperand.Parameters[0].Name = "LValue";
            firstOperand.Parameters[0].Description = "First operand to use in the calculation.";

            return firstOperand;
        }
        private Property CreateOperandProperty(ActivityParameter sourceParameter)
        {
            Property operand = new Property
            {
                ID = "",
                DesignerType = "Number",
                DisplayName = "",
                Parameters =
                new Parameters[]
                {
                    new Parameters
                    {
                        Name = "",
                        Description = "",
                        Required = true,
                        DataType = "Double",
                        DesignerType = "Number",
                        Direction = "Input",
                        Value = base.ConvertParameterValue(sourceParameter),
                    }
                }

            };
            var parameter = operand.Parameters[0];
            if (parameter.Value.PrimitiveValue != null)
            {
                ReSetPrimitiveValueType(parameter.Value.PrimitiveValue);
            }
            parameter.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameter);
            CheckUnSupportData(parameter);
            return operand;
        }
        private Property CreateSecondOperand()
        {
            var sourceParameter = NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "Operand2", true);

            var secondOperand = CreateOperandProperty(sourceParameter);
            secondOperand.ID = "p2";
            secondOperand.DisplayName = "Second operand";
            secondOperand.Parameters[0].Name = "RValue";
            secondOperand.Parameters[0].Description = "Second operand to use in the calculation.";
            return secondOperand;
        }

        private Property CreateOutput()
        {
            var sourceParameter = NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "VariableName", true);
            Property output = new Property();
            output.ID = "p3";
            output.DesignerType = "Variable";
            output.DisplayName = "Output";
            Parameters parameter = new Parameters
            {
                Name = "To",
                Required = true,
                DataType = "Numeric",
                DesignerType = "Variable",
                Direction = "Output",
                Value = base.ConvertParameterValue(sourceParameter),
            };
            output.Parameters = new Parameters[] { parameter };
            return output;
        }

    }
}
