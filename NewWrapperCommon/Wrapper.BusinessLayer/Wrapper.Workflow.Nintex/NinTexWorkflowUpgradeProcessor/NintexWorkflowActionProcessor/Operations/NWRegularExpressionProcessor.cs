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
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    enum StringOperation
    {
        Replace = 1,
        Match = 2,
        Split = 3,
        Extract = 4,
    }
    class NWRegularExpressionProcessor : NWActionProcessorBase
    {
        private StringOperation operation;
        public NWRegularExpressionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#RegEx";
        }

        protected override Configuration CreateConfiguration()
        {
            var operationParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Mode", true);
            operation = (StringOperation)Enum.Parse(typeof(StringOperation), operationParameter.PrimitiveValue.Value);
            return base.CreateConfiguration();
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374182",
                ClassName = CLASSNAME,
                x49x49 = 49,
                y49x49 = 395,
                x30x30 = 49,
                y30x30 = 444,
                x16x16 = 82,
                y16x16 = 444,
            };
        }

        protected override List<Property> CreateProperties()
        {
            return new List<Property>
            {
                new Property
                {
                    ID="RegEx",
                    DesignerType="RegularExpression",
                    Parameters = new Parameters[]
                    {
                        CreateInputTextParameter(),
                        CreateOperationParameter(),
                        CreatePatternParameter(),
                        CreateIgnoreCaseParameter(),
                        CreateReplacementTextParameter(),
                        CreateOutTextParameter(),
                        CreateOutTextListParameter(),
                        CreateOutMatchFoundParameter(),
                    }
                }
            };
        }

        private Parameters CreateInputTextParameter()
        {
            var inputTextParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Text", true);
            return new Parameters
            {
                Name = "InputText",
                Required = true,
                DataType = "String",
                DesignerType = "Multiline",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(inputTextParameter.PrimitiveValue, base.workflowActionProcessor, true),
                }
            };
        }

        private Parameters CreateOperationParameter()
        {

            return new Parameters
            {
                Name = "Operation",
                Required = true,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Int32", Value = new Value(((int)operation).ToString()) },
                }
            };
        }

        private Parameters CreatePatternParameter()
        {
            var patternParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Pattern", true);
            return new Parameters
            {
                Name = "Pattern",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(patternParameter.PrimitiveValue, base.workflowActionProcessor, true),
                }
            };
        }

        private Parameters CreateIgnoreCaseParameter()
        {
            var ignoreCaseParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "IgnoreCase", true);
            return new Parameters
            {
                Name = "IgnoreCase",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value(bool.TrueString.Equals(ignoreCaseParameter.PrimitiveValue.Value, StringComparison.OrdinalIgnoreCase).ToString())
                    },
                }
            };
        }

        private Parameters CreateReplacementTextParameter()
        {
            var replacementTextParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Replace", true);
            return new Parameters
            {
                Name = "ReplacementText",
                Required = false,
                DataType = "String",
                DesignerType = "Multiline",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(replacementTextParameter.PrimitiveValue, base.workflowActionProcessor, true),
                }
            };
        }

        private Parameters CreateOutTextParameter()
        {
            return new Parameters
            {
                Name = "OutText",
                Required = GetOutputRequiredProperty().Item1,
                DataType = "String",
                DesignerType = "Variable",
                Direction = "Output",
                Value = new ParametersValue
                {
                    Variable = GetVariableByStringOperation(StringOperation.Replace),
                }
            };
        }

        private Parameters CreateOutTextListParameter()
        {
            //SPlit&Extract 的Output都放在这个parameter上
            var variable = GetVariableByStringOperation(StringOperation.Split);
            if (string.IsNullOrEmpty(variable.Name))
            {
                variable = GetVariableByStringOperation(StringOperation.Extract);
            }
            return new Parameters
            {
                Name = "OutTextList",
                Required = GetOutputRequiredProperty().Item2,
                DataType = "DynamicValue",
                Type = "Array",
                DesignerType = "Variable",
                Direction = "Output",
                Value = new ParametersValue
                {
                    Variable = variable,
                }
            };
        }

        private Parameters CreateOutMatchFoundParameter()
        {
            return new Parameters
            {
                Name = "OutMatchFound",
                Required = GetOutputRequiredProperty().Item3,
                DataType = "Boolean",
                DesignerType = "Variable",
                Direction = "Output",
                Value = new ParametersValue
                {
                    Variable = GetVariableByStringOperation(StringOperation.Match),
                }
            };
        }

        private Variable GetVariableByStringOperation(StringOperation stringOperation)
        {
            if (operation != stringOperation)
            {
                return new Variable { DataType = string.Empty, Name = string.Empty, };
            }
            var outTextParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Output", true);
            return base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outTextParameter.Variable.Name);
        }

        /// <summary>
        /// Get OutText,OutTextList Required value
        /// Split and Extract use OutTextList as the output value(collection type)
        /// </summary>
        /// <returns>
        /// OutText: Item1
        /// OutTextList: Item2
        /// OutMatchFound: Item3
        /// </returns>
        private Tuple<bool, bool, bool> GetOutputRequiredProperty()
        {
            switch (operation)
            {
                case StringOperation.Split:
                case StringOperation.Extract:
                    return new Tuple<bool, bool, bool>(false, true, false);
                case StringOperation.Replace:
                    return new Tuple<bool, bool, bool>(true, false, false);
                case StringOperation.Match:
                    return new Tuple<bool, bool, bool>(false, false, true);
                default:
                    throw new ArgumentException("Invalid operation:" + operation);
            }
        }

    }
}
