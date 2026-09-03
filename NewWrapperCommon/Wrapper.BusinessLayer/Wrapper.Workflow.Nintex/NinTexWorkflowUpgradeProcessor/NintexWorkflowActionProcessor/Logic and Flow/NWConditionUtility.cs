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

namespace LS.SPWorkflowProcessor
{
    class NWConditionUtility
    {
        private NintexWFActionProcessor workflowActionProcessor;
        private const string OPERATOR = "operator";
        private const string CONDITION = "condition";
        public NWConditionUtility(NintexWFActionProcessor workflowActionProcessor)
        {
            this.workflowActionProcessor = workflowActionProcessor;
        }

        public DictionaryValue ConvertToDictionaryValue(NWConditionConfigParam param)
        {
            var dicValue = new DictionaryValue();
            if (string.Equals(OPERATOR, param.Name, StringComparison.OrdinalIgnoreCase))
            {
                dicValue.Key = CONDITION;
            }
            else
            {
                dicValue.Key = param.Name;
            }
            return dicValue;
        }

        public Parameters GenerateParameters()
        {
            return new Parameters
            {
                Name = Guid.NewGuid().ToString(),
                Required = true,
                DataType = "Dictionary",
                DesignerType = "Hidden",
                Direction = "Input",
                Value = new ParametersValue()
            };
        }


        /// <summary>
        /// 通过name获取ConditionConfig parameter 的一个clone
        /// </summary>
        /// <param name="nwConditionConfigParams"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public NWConditionConfigParam FindParamCloneByName(NWConditionConfigParam[] nwConditionConfigParams, string paramName)
        {
            var param = nwConditionConfigParams.FirstOrDefault(item => item.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));
            if (param == null)
            {
                throw new AveWrapperBaseException(string.Format("Can not found param by name, param name is {0}", paramName));
            }
            return param.Clone();
        }

        public DictionaryValue GenerateLogicDictionaryValue(ConditionOperator conditionOperator)
        {
            return new DictionaryValue
            {
                Key = "logic",
                Value = new Value
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value
                        {
                            StringValue = conditionOperator.ToString()
                        },
                    }
                },
            };
        }

        /// <summary>
        /// Convert the format of param: value like
        /// &lt;Value&gt;
        ///    &lt;Variable&gt;
        ///      &lt;Name&gt;XXX&lt;/Name&gt;
        ///      &lt;DataType&gt;XXX&lt;/DataType&gt;
        ///    &lt;/Variable&gt;
        ///    &lt;Coercion&gt;AsDNXXX&lt;/Coercion&gt;
        /// &lt;/Value&gt;
        /// to the format of result like
        /// &lt;Value&gt;
        ///    &lt;PrimitiveValue&gt;
        ///      &lt;Type&gt;String&lt;/Type&gt;
        ///      &lt;Value&gt;
        ///        &lt;string&gt;{0}&lt;/string&gt;
        ///      &lt;/Value&gt;
        ///      &lt;FormatValues&gt;
        ///        &lt;SelectedValue&gt;
        ///          &lt;ListLookup&gt;
        ///            &lt;SelectList&gt;[Current Item]&lt;/SelectList&gt;
        ///            &lt;SelectField&gt;ID&lt;/SelectField&gt;
        ///            &lt;SelectFieldType&gt;Int32&lt;/SelectFieldType&gt;
        ///            &lt;WhereField /&gt;
        ///            &lt;WhereFieldType /&gt;
        ///            &lt;WhereValue /&gt;
        ///            &lt;DisplayName&gt;Current Item&lt;/DisplayName&gt;
        ///            &lt;DisplayValue&gt;ID&lt;/DisplayValue&gt;
        ///          &lt;/ListLookup&gt;
        ///          &lt;Coercion&gt;AsDNString&lt;/Coercion&gt;
        ///        &lt;/SelectedValue&gt;
        ///      &lt;/FormatValues&gt;
        ///    &lt;/PrimitiveValue&gt;
        /// &lt;/Value&gt;
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Value ConvertToTextBuilderModeValue(DictionaryValue leftDictionaryValue, Value value)
        {
            if (value == null)
            {
                return value;
            }
            #region init
            var tempValue = new Value();
            tempValue.PrimitiveValue = new PrimitiveValue("String", "{0}")
            {
                FormatValues = new List<FormatValues>() {
                            new FormatValues() {
                                SelectedValue = new SelectedValue()
                            }
                        }
            };
            #endregion

            bool beConverted = false;
            string leftDataType = string.Empty;

            if (leftDictionaryValue.Value.WorkflowContext != null)
            {
                leftDataType = leftDictionaryValue.Value.WorkflowContext.Type;
                if (NWWorkflowContextDataConverter.TextBuilderModeWorkflowContextType.Contains(leftDictionaryValue.Value.WorkflowContext.Value))
                {
                    FillValue(leftDataType, value, tempValue);
                    beConverted = true;
                }
            }
            else if (leftDictionaryValue.Value.Variable != null)
            {
                leftDataType = leftDictionaryValue.Value.Variable.DataType;
                if (this.workflowActionProcessor.VariablesCacheManager.TextBuilderModeDataType.Contains(leftDictionaryValue.Value.Variable.DataType))
                {
                    FillValue(leftDataType, value, tempValue);
                    beConverted = true;
                }
            }
            else if (leftDictionaryValue.Value.ListLookup != null)
            {
                leftDataType = leftDictionaryValue.Value.ListLookup.SelectFieldType;
                if (NWListLookupConverter.TextBuilderModeFieldType.Contains(leftDictionaryValue.Value.ListLookup.SelectFieldType))
                {
                    FillValue(leftDataType, value, tempValue);
                    beConverted = true;
                }
            }
            else if (leftDictionaryValue.Value.PrimitiveValue != null
                && string.Equals("String", leftDictionaryValue.Value.PrimitiveValue.Type, StringComparison.OrdinalIgnoreCase))
            //如果Left type是String，那么right也需要FillValue
            {
                leftDataType = leftDictionaryValue.Value.PrimitiveValue.Type;
                FillValue(leftDictionaryValue.Value.PrimitiveValue.Type, value, tempValue);
                beConverted = true;
            }

            if (!beConverted)
            {
                tempValue = value;
            }

            if (tempValue.PrimitiveValue != null && tempValue.PrimitiveValue.Value != null)
            {
                tempValue.PrimitiveValue.Type = leftDataType;
            }

            tempValue = FillCoercionInValue(leftDataType, tempValue);

            return tempValue;
        }

        private Value FillCoercionInValue(string leftDataType, Value oldRightValue)
        {
            if (string.Equals(leftDataType, "Double", StringComparison.OrdinalIgnoreCase)
                && oldRightValue.PrimitiveValue != null
                && string.Equals(oldRightValue.PrimitiveValue.Type, "Int32", StringComparison.OrdinalIgnoreCase))
            {
                oldRightValue.PrimitiveValue.Type = "Double";
            }
            else if (oldRightValue.PrimitiveValue == null)
            {
                var coercion = NWCoercionStringProcessor.GetCoercionString(leftDataType, oldRightValue);
                if (!string.IsNullOrEmpty(coercion))
                {
                    oldRightValue.Coercion = coercion;
                }
            }

            return oldRightValue;
        }

        private void FillValue(string leftDataType, Value value, Value finalValue)
        {
            if (value.WorkflowContext != null)
            {
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.WorkflowContext = value.WorkflowContext;
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = NWCoercionStringProcessor.GetCoercionString(leftDataType, value.WorkflowContext.Type);
            }
            if (value.Variable != null)
            {
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.Variable = value.Variable;
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = NWCoercionStringProcessor.GetCoercionString(leftDataType, value.Variable.DataType);
            }
            if (value.ListLookup != null)
            {
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.ListLookup = value.ListLookup;
                finalValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = NWCoercionStringProcessor.GetCoercionString(leftDataType, value.ListLookup.SelectFieldType);
            }
            if (value.PrimitiveValue != null)
            {
                finalValue.PrimitiveValue = value.PrimitiveValue;
            }
        }

        public PrimitiveValue ConvertPrimitiveValue(Native13NinTexWorkflowEntity.PrimitiveValue sourcePrimitiveValue, string conditionType, bool isOperator)
        {
            var primitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(sourcePrimitiveValue, this.workflowActionProcessor, true);
            if (isOperator)
            {
                primitiveValue.Value.StringValue = ConvertOperatorPrimitiveValueStringValue(sourcePrimitiveValue.Value, conditionType);
            }
            return primitiveValue;
        }

        private string ConvertOperatorPrimitiveValueStringValue(string sourceValue, string conditionType)
        {
            if (NoNeedChangeValue(sourceValue, conditionType))
            {
                return sourceValue;
            }

            return string.Format("{0}{1}", sourceValue, conditionType);
        }

        private bool NoNeedChangeValue(string sourceValue, string conditionType)
        {
            if (string.Equals(conditionType, "Datetime", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(sourceValue, "EqualNoTime", StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(conditionType, "String", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals("IsEmpty", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("NotIsEmpty", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("StartsWith", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("NotStartsWith", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("EndsWith", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("NotEndsWith", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("Contains", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("NotContains", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("Matches", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("EqualNoCase", sourceValue, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("ContainsNoCase", sourceValue, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public string GetConditionType(Value value)
        {
            if (value.ListLookup != null)
            {
                return string.Equals("Lookup", value.ListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase) ? "DynamicValue" : value.ListLookup.SelectFieldType;
            }

            if (value.PrimitiveValue != null)
            {
                return "String";
            }

            if (value.WorkflowContext != null)
            {
                return value.WorkflowContext.Type;
            }

            if (value.Variable != null)
            {
                return value.Variable.DataType;
            }
            return string.Empty;
        }
    }
}
