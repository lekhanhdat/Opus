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
using System.Text.RegularExpressions;

namespace LS.SPWorkflowProcessor
{
    static class NWValueConverter
    {
        public static ParametersValue ConvertValueToParametersValue(NintexWFActionProcessor workflowActionProcessor, ActivityParameter activityParameter)
        {
            var value = ConvertValue(workflowActionProcessor, activityParameter);
            if (value == null)
            {
                return null;
            }

            return new ParametersValue
            {
                WorkflowContext = value.WorkflowContext,
                PrimitiveValue = value.PrimitiveValue,
                Variable = value.Variable,
                ListLookup = value.ListLookup,
            };
        }

        public static Value ConvertValue(NintexWFActionProcessor workflowActionProcessor, ActivityParameter activityParameter)
        {
            return ConvertValue(workflowActionProcessor, activityParameter.ListLookup, activityParameter.PrimitiveValue, activityParameter.WorkflowContextData, activityParameter.Variable, activityParameter.WorkflowConstant, activityParameter.ProfileLookup);
        }

        public static Value ConvertValue(NintexWFActionProcessor workflowActionProcessor, ValueLookup listLookup)
        {
            return ConvertValue(workflowActionProcessor, listLookup.Lookup, listLookup.PrimitiveValue, listLookup.WorkflowContextData, listLookup.Variable, listLookup.WorkflowConstant, listLookup.ProfileLookup);
        }

        public static Value ConvertValue(NintexWFActionProcessor workflowActionProcessor, NWConditionConfigParam conditionConfigParam)
        {
            return ConvertValue(workflowActionProcessor, conditionConfigParam.ListLookup, conditionConfigParam.PrimitiveValue, conditionConfigParam.WorkflowContextData, conditionConfigParam.Variable, conditionConfigParam.WorkflowConstant, conditionConfigParam.ProfileLookup);
        }

        public static Value ConvertValue(NintexWFActionProcessor workflowActionProcessor, ValueLookup listLookup, Native13NinTexWorkflowEntity.PrimitiveValue primitiveValue, WorkflowContextData workflowContextData, NWWorkflowVariable variable, WorkflowConstantLookup workflowConstant, ProfileLookup profileLookup)
        {
            Value value = new Value();
            //List Lookup
            if (listLookup != null)
            {
                value.ListLookup = NWListLookupConverter.ConvertListLookup(listLookup, workflowActionProcessor);
                return value;
            }

            //Value
            if (primitiveValue != null && !string.IsNullOrEmpty(primitiveValue.Value))
            {
                value.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(primitiveValue, workflowActionProcessor, true);
                return value;
            }

            //Workflow Context
            if (workflowContextData != null)
            {
                value.WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(workflowContextData);
                return value;
            }

            //Workflow Variables
            if (variable != null && !string.IsNullOrEmpty(variable.Name))
            {
                value.Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(variable);
                return value;
            }

            if (workflowConstant != null)
            {
                throw new UnSupportedDataException("Can not support workflow contsant data.");
            }

            if (profileLookup != null)
            {
                throw new UnSupportedDataException("Can not support user profile data.");
            }
            return null;
        }

        private static void CheckCanSupportData(string value)
        {
            const string variablePattern = @"{WorkflowVariable:.*}";
            const string commonPattern = @"{Common:.*}";
            const string itemPropertyPattern = @"{ItemProperty:.*}";
            bool includeSpecialValue = Regex.IsMatch(value, variablePattern) || Regex.IsMatch(value, commonPattern) || Regex.IsMatch(value, itemPropertyPattern);
            bool isOnlyIncludeSpecialVlaue = Regex.IsMatch(value, string.Format("^{0}$", variablePattern)) || Regex.IsMatch(value, string.Format("^{0}$", commonPattern)) || Regex.IsMatch(value, string.Format("^{0}$", itemPropertyPattern));
            if (includeSpecialValue && !isOnlyIncludeSpecialVlaue)
            {
                //ADO-193545 无法支持****{WorkflowVariable:aaa}*** 这样的数据，online创建不出来
                throw new NotSupportedException(string.Format("Only support one value for primitive value, now value is {0}", value));
            }
        }
        public static Value ConvertPrimitiveValueToValue(NintexWFActionProcessor workflowActionProcessor, Native13NinTexWorkflowEntity.PrimitiveValue primitiveValue)
        {
            Value value = new Value();
            if (primitiveValue != null && !string.IsNullOrEmpty(primitiveValue.Value))
            {
                CheckCanSupportData(primitiveValue.Value);

                if (primitiveValue.Value.IndexOf("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    value.Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(new NWWorkflowVariable() { Name = primitiveValue.Value.Substring("{WorkflowVariable:".Length, primitiveValue.Value.Length - "{WorkflowVariable:".Length - 1) });
                }
                else if (primitiveValue.Value.IndexOf("{Common:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    value.WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(new WorkflowContextData() { Name = primitiveValue.Value.Substring("{Common:".Length, primitiveValue.Value.Length - "{Common:".Length - 1) });
                }
                else if (primitiveValue.Value.IndexOf("{ItemProperty:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    value.ListLookup = NWPrimitiveValueConverter.ConvertItemPropertyToListLookup(workflowActionProcessor, primitiveValue.Value);
                }
                else
                {
                    value.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(primitiveValue, workflowActionProcessor, false);
                }
            }
            else
            {
                value = null;
            }

            return value;
        }

        /// <summary>
        /// if input param: listLookup, workflowContextData or variable is not null, result will include {0}
        /// </summary>
        /// <param name="workflowActionProcessor"></param>
        /// <param name="listLookup"></param>
        /// <param name="primitiveValue"></param>
        /// <param name="workflowContextData"></param>
        /// <param name="coercion"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static Value ConvertValueWithTextBuilderMode(NintexWFActionProcessor workflowActionProcessor, ValueLookup listLookup, Native13NinTexWorkflowEntity.PrimitiveValue primitiveValue, WorkflowContextData workflowContextData, Coercion coercion, NWWorkflowVariable variable,ProfileLookup profileLookup, WorkflowConstantLookup workflowConstant)
        {
            Value value = new Value();
            //List Lookup
            if (listLookup != null)
            {
                value.PrimitiveValue = new PrimitiveValue("String", "{0}")
                {
                    FormatValues = new List<FormatValues>
                    {
                        new FormatValues()
                        {
                            SelectedValue = new SelectedValue()
                            {
                                ListLookup = NWListLookupConverter.ConvertListLookup(listLookup, workflowActionProcessor)
                            }
                        }
                    }
                };
                return value;
            }

            //Value
            if (primitiveValue != null && !string.IsNullOrEmpty(primitiveValue.Value))
            {
                value.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(primitiveValue, workflowActionProcessor, true);
                return value;
            }

            //Workflow Context
            if (workflowContextData != null)
            {
                value.PrimitiveValue = new PrimitiveValue("String", "{0}")
                {
                    FormatValues = new List<FormatValues>
                    {
                        new FormatValues()
                        {
                            SelectedValue = new SelectedValue()
                            {
                                WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(workflowContextData),
                                Coercion = coercion.Value
                            }
                        }
                    }
                };
                return value;
            }

            //Workflow Variables
            if (variable != null)
            {
                value.PrimitiveValue = new PrimitiveValue("String", "{0}")
                {
                    FormatValues = new List<FormatValues>
                    {
                        new FormatValues()
                        {
                            SelectedValue = new SelectedValue()
                            {
                                Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(variable)
                            }
                        }
                    }
                };
                return value;
            }

            if (workflowConstant != null)
            {
                throw new UnSupportedDataException("Can not support workflow contsant data.");
            }

            if (profileLookup != null)
            {
                throw new UnSupportedDataException("Can not support user profile data.");
            }
            return null;
        }
    }
}
