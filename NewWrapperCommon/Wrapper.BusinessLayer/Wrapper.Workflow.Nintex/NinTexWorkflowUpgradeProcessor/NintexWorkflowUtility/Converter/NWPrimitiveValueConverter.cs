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
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    static class NWPrimitiveValueConverter
    {

        private static AveLogger logger = AveLogger.GetInstance(typeof(NWPrimitiveValueConverter));

        /// <summary>
        /// workflowVariable格式：{WorkflowVariable:VariableName}
        /// </summary>
        /// <param name="workflowVariable"></param>
        /// <returns></returns>
        private static SelectedValue ConvertWorkflowVariableToSelectedValue(NintexWFActionProcessor workflowActionProcessor, string workflowVariable)
        {
            var variable = GetWorkflowVariable(workflowVariable, workflowActionProcessor);
            return new SelectedValue
            {
                Coercion = "AsDNString",
                Variable = variable,
            };
        }

        private static Variable GetWorkflowVariable(string workflowVariable, NintexWFActionProcessor workflowActionProcessor)
        {
            int startIndex = workflowVariable.IndexOf("{WorkflowVariable:") + "{WorkflowVariable:".Length;
            string variableName = workflowVariable.Substring(startIndex, workflowVariable.IndexOf('}') - startIndex);
            return workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(variableName);
        }

        /// <summary>
        /// workflowVariable格式：{WorkflowVariable:VariableName}
        /// </summary>
        /// <param name="workflowVariable"></param>
        /// <param name="parametersDataType"></param>
        /// <returns></returns>
        private static ParametersValue ConvertWorkflowVariableToValue(string workflowVariable, string parametersDataType, NintexWFActionProcessor workflowActionProcessor)
        {
            var variable = GetWorkflowVariable(workflowVariable, workflowActionProcessor);
            return new ParametersValue
            {
                Variable = variable,
                Coercion = NWCoercionStringProcessor.GetCoercionString(parametersDataType, variable.DataType)
            };
        }

        /// <summary>
        /// WorkflowContext格式：{Common:WorkflowContextName}
        /// </summary>
        /// <param name="workflowContext"></param>
        /// <param name="parametersDataType"></param>
        /// <returns></returns>
        private static ParametersValue ConvertWorkflowContextToValue(string workflowContext, string parametersDataType, bool throwException)
        {
            try
            {
                var workflowContextData = GetWorkflowContext(workflowContext);
                return new ParametersValue
                {
                    WorkflowContext = workflowContextData,
                    Coercion = NWCoercionStringProcessor.GetCoercionString(parametersDataType, workflowContextData.Type)
                };
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while convert workflow context to ParametersValue. Error: {0}", e);
                if (throwException)
                {
                    throw e;
                }
                return null;
            }
        }

        private static WorkflowContext GetWorkflowContext(string workflowContext)
        {
            int startIndex = "{Common:".Length;
            string workflowContextName = workflowContext.Substring(startIndex, workflowContext.Length - startIndex - 1);
            return NWWorkflowContextDataConverter.ConvertWorkflowContextData(new WorkflowContextData { Name = workflowContextName });
        }

        /// <summary>
        /// WorkflowContext格式：{Common:WorkflowContextName}
        /// </summary>
        /// <param name="workflowVariable"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static SelectedValue ConvertWorkflowContextToSelectedValue(string workflowContext, bool throwException)
        {
            try
            {
                var workflowContextData = GetWorkflowContext(workflowContext);
                return new SelectedValue
                {
                    Coercion = "AsDNString",
                    WorkflowContext = workflowContextData,
                };
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while convert workflow context to SelectedValue. Error: {0}", e);
                if (throwException)
                {
                    throw e;
                }
                return null;
            }
        }

        private static Field GetFieldByInternalName(ListReferenceCollection listReferences, Guid listId, string fieldName)
        {
            var list = listReferences.GetList(listId);
            if (list != null)
            {
                var fielReference = list.Fields.GetByInternalName(fieldName);
                if (fielReference != null)
                {
                    return new Field { Name = fieldName, Type = fielReference.FieldType.ToString() };
                }
            }
            return new Field { Name = fieldName };
        }
        /// <summary>
        /// ItemProperty：{ItemProperty:FieldName}
        /// </summary>
        /// <param name="itemProperty"></param>
        /// <param name="parametersDataType">the data type of destination parameters</param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        private static ParametersValue ConvertItemPropertyToValue(string itemProperty, string parametersDataType, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            try
            {
                ValueLookup tempLookup = ConvertItemPropertyToValueLookup(workflowActionProcessor, itemProperty);
                var parametersValue = new ParametersValue();
                parametersValue.ListLookup = NWListLookupConverter.ConvertListLookup(tempLookup, workflowActionProcessor);
                parametersValue.Coercion = NWCoercionStringProcessor.GetCoercionString(parametersDataType, parametersValue.ListLookup.SelectFieldType);
                return parametersValue;
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while convert item property to Value. Error: {0}", e);
                if (throwException)
                {
                    throw e;
                }
                return null;
            }
        }

        private static ValueLookup ConvertItemPropertyToValueLookup(NintexWFActionProcessor workflowActionProcessor, string itemProperty)
        {
            int startIndex = "{ItemProperty:".Length;
            var fieldName = itemProperty.Substring(startIndex, itemProperty.Length - startIndex - 1);
            string fieldType = null;
            try
            {
                var field = workflowActionProcessor.List.Fields.GetFieldByInternalName(fieldName, false);
                if (field is IAveFieldUser)
                {
                    fieldType = (field as IAveFieldUser).BaseTypeString;
                }
                else
                {
                    fieldType = field is IAveFieldCalculated ? ((IAveFieldCalculated)field).OutputType.ToString() : field.Type.ToString();
                }
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while convert item property to value lookup. Error: {0}", e);
            }
            return new ValueLookup
            {
                LookupType = SLLookupType.ThisItemLookup,
                Field = new Field { Name = fieldName, Type = fieldType },
            };
        }

        public static ListLookup ConvertItemPropertyToListLookup(NintexWFActionProcessor workflowActionProcessor, string itemProperty)
        {
            ValueLookup tempLookup = ConvertItemPropertyToValueLookup(workflowActionProcessor, itemProperty);
            return NWListLookupConverter.ConvertListLookup(tempLookup, workflowActionProcessor);
        }

        /// <summary>
        /// ItemProperty：{ItemProperty:FieldName}
        /// </summary>
        /// <param name="workflowVariable"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static SelectedValue ConvertItemPropertyToSelectedValue(string itemProperty, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            try
            {
                ValueLookup tempLookup = ConvertItemPropertyToValueLookup(workflowActionProcessor, itemProperty);
                var tempSelectedValue = new SelectedValue
                {
                    ListLookup = NWListLookupConverter.ConvertListLookup(tempLookup, workflowActionProcessor),
                };
                tempSelectedValue.Coercion = NWCoercionStringProcessor.GetCoercionString("String", tempSelectedValue.ListLookup.SelectFieldType);
                return tempSelectedValue;
            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while convert item property to SelectedValue. Error: {0}", e);
                if (throwException)
                {
                    throw e;
                }
                return null;
            }
        }


        /// <summary>
        /// 根据{} 来split 具体例子如下：
        /// "123{465}789" -> List<string>{{"123"},{"{456}"},{"789"}}
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        private static List<parametersData> SplitString(string sourceValue)
        {
            int processIndex = 0;
            List<parametersData> parameters = new List<parametersData>();
            MatchCollection mc = Regex.Matches(sourceValue, @"{[^{}]+}");
            {
                foreach (Match m in mc)
                {
                    var startIndex = m.Index;
                    var endIndex = m.Index + m.Length - 1;

                    var noNeedReplace = sourceValue.Substring(processIndex, startIndex - processIndex);
                    parameters.Add(new parametersData() { parameterValue = noNeedReplace, isLink = false });
                    parameters.Add(new parametersData() { parameterValue = m.Value, isLink = LinkValueUtil.IsLinkValue(sourceValue, startIndex) });
                    processIndex = endIndex + 1;
                }
            }
            parameters.Add(new parametersData() { parameterValue = sourceValue.Substring(processIndex), isLink = false });
            return parameters;
        }

        private class LinkValueUtil
        {
            private Dictionary<int, int> linkLabelIndex;
            private static LinkValueUtil instance;
            private static string cacheSourceValue;

            private LinkValueUtil(string sourceValue)
            {
                cacheSourceValue = sourceValue;
                linkLabelIndex = LinkLabelIndex(sourceValue);
            }

            public static bool IsLinkValue(string sourceValue, int startIndex)
            {
                if (instance == null || !string.Equals(cacheSourceValue, sourceValue))
                {
                    instance = new LinkValueUtil(sourceValue);
                }
                return instance.CheckIndexInLinkLabelIndex(startIndex);
            }

            private bool CheckIndexInLinkLabelIndex(int index)
            {
                foreach (var linkLabelIndexPair in linkLabelIndex)
                {
                    var startIndex = linkLabelIndexPair.Key;
                    var endIndex = linkLabelIndexPair.Value;
                    if (index > startIndex && index < endIndex)
                    {
                        return true;
                    }
                }
                return false;
            }

            private Dictionary<int, int> LinkLabelIndex(string sourceValue)
            {
                // key: start index, value: end index
                Dictionary<int, int> linkLableIndex = new Dictionary<int, int>();
                var startIndex = sourceValue.IndexOf("href=", StringComparison.OrdinalIgnoreCase);
                var endIndex = sourceValue.IndexOf("</a>", StringComparison.OrdinalIgnoreCase);
                while (startIndex >= 0 && endIndex >= 0)
                {
                    linkLableIndex.Add(startIndex, endIndex);
                    startIndex = sourceValue.IndexOf("href=", endIndex + 1, StringComparison.OrdinalIgnoreCase);
                    endIndex = sourceValue.IndexOf("</a>", endIndex + 1, StringComparison.OrdinalIgnoreCase);
                }
                return linkLableIndex;
            }
        }

        private class parametersData
        {
            public string parameterValue { get; set; }
            public bool isLink { get; set; }

        }

        private static string ConvertPrimitiveValue(List<parametersData> parameters, List<FormatValues> formatValues, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            if (parameters.Count == 0)
            {
                return null;
            }
            StringBuilder result = new StringBuilder();
            int index = 0;
            foreach (var parameter in parameters)
            {
                SelectedValue tempSelectedValue = null;
                if (parameter.parameterValue.IndexOf("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tempSelectedValue = ConvertWorkflowVariableToSelectedValue(workflowActionProcessor, parameter.parameterValue);
                }
                else if (parameter.parameterValue.IndexOf("{Common:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tempSelectedValue = ConvertWorkflowContextToSelectedValue(parameter.parameterValue, throwException);
                }
                else if (parameter.parameterValue.IndexOf("{ItemProperty:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tempSelectedValue = ConvertItemPropertyToSelectedValue(parameter.parameterValue, workflowActionProcessor, throwException);
                }

                if (tempSelectedValue != null)
                {
                    result.Append("{" + index++ + "}");
                    if (parameter.isLink)
                    {
                        formatValues.Add(
                            new FormatValues { SelectedValue = new SelectedValue() { PrimitiveValue = new PrimitiveValue() { Type = "String", Value = new Value("{0}"), FormatValues = new List<FormatValues>() { new FormatValues() { SelectedValue = tempSelectedValue } } } } });
                    }
                    else
                    {
                        formatValues.Add(new FormatValues { SelectedValue = tempSelectedValue });
                    }
                }
                else
                {
                    result.Append(parameter.parameterValue);
                }
            }
            return result.ToString();
        }

        private static string ConvertPrimitiveValue(string sourceValue, List<FormatValues> selectedValues, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return sourceValue;
            }
            var parameters = SplitString(sourceValue);
            var result = ConvertPrimitiveValue(parameters, selectedValues, workflowActionProcessor, throwException);
            return AveHtmlUtility.HtmlDecode(result);
        }

        public static PrimitiveValue ConvertPrimitiveValue(Native13NinTexWorkflowEntity.PrimitiveValue sourcePrimitiveValue, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            var primitiveValue = new PrimitiveValue();
            primitiveValue.Value = new Value();
            primitiveValue.Type = NWFieldTypeMapping.ConvertFieldType(sourcePrimitiveValue.ValueType);

            if (sourcePrimitiveValue.ValueType.Equals("Datetime", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(sourcePrimitiveValue.Value))
            {
                var srcWebTimeZone = workflowActionProcessor.DataMappingManager.GetSourceWebTimeZone();
                var destWebTimeZone = workflowActionProcessor.Web.RegionalSettings.TimeZone;
                primitiveValue.Value.DateTimeInfo = NWDateTimeInfoConverter.ConvertDateTimeInfo(srcWebTimeZone,
                    destWebTimeZone, sourcePrimitiveValue.Value);
            }
            else
            {
                var formatValues = new List<FormatValues>();
                primitiveValue.Value.StringValue = ConvertPrimitiveValue(sourcePrimitiveValue.Value, formatValues, workflowActionProcessor, throwException);
                if (formatValues.Count > 0)
                {
                    primitiveValue.FormatValues = formatValues;
                }
            }
            return primitiveValue;
        }


        public static PrimitiveValue ConvertPrimitiveValue(string sourceValue, string valueType, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            var mockPrimitiveValue = new Native13NinTexWorkflowEntity.PrimitiveValue
            {
                ValueType = valueType,
                Value = sourceValue,
            };

            return ConvertPrimitiveValue(mockPrimitiveValue, workflowActionProcessor, throwException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters">The key of KeyValuePair is parameter name, the value of KeyValuePair means if the parameter is in the link:&lt;a href...&gt;...&lt;/a&gt;</param>
        /// <param name="workflowActionProcessor"></param>
        /// <param name="throwException"></param>
        /// <param name="keepConvertFailedParameters">If convert operation failed, use parameters name to fill</param>
        /// <returns></returns>
        public static List<FormatValues> ConvertPrimitiveValueToFormatValuesList(List<KeyValuePair<string, bool>> parameters, NintexWFActionProcessor workflowActionProcessor, bool throwException, bool keepConvertFailedParameters = false)
        {
            if (parameters.Count == 0)
            {
                return null;
            }
            List<FormatValues> formatValues = new List<FormatValues>();
            foreach (var parameter in parameters)
            {
                if (parameter.Key.StartsWith("{Common:", StringComparison.OrdinalIgnoreCase)
                    || parameter.Key.StartsWith("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase)
                    || parameter.Key.StartsWith("{ItemProperty:", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedValue tempSelectedValue = ConvertStringToSelectedValue(parameter.Key, workflowActionProcessor, throwException);

                    if (tempSelectedValue != null)
                    {
                        if (parameter.Value)
                        {
                            formatValues.Add(new FormatValues
                            {
                                SelectedValue = new SelectedValue
                                {
                                    PrimitiveValue = new PrimitiveValue("String", "{0}")
                                    {
                                        FormatValues = new List<FormatValues>() { new FormatValues { SelectedValue = tempSelectedValue } }
                                    }
                                }
                            });
                        }
                        else
                        {
                            formatValues.Add(new FormatValues { SelectedValue = tempSelectedValue });
                        }
                    }
                    else if (keepConvertFailedParameters) // 如果为空,说明convert失败,使用parameter name填充
                    {
                        formatValues.Add(new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                PrimitiveValue = new PrimitiveValue("String", parameter.Key)
                            }
                        });
                    }
                }
                else
                {
                    formatValues.Add(new FormatValues
                    {
                        SelectedValue = new SelectedValue
                        {
                            PrimitiveValue = new PrimitiveValue("String", parameter.Key)
                        }
                    });
                }
            }
            return formatValues;
        }

        public static SelectedValue ConvertStringToSelectedValue(string valueStr, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            SelectedValue selectedValue = null;
            if (valueStr.IndexOf("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                selectedValue = ConvertWorkflowVariableToSelectedValue(workflowActionProcessor, valueStr);
            }
            else if (valueStr.IndexOf("{Common:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                selectedValue = ConvertWorkflowContextToSelectedValue(valueStr, throwException);
            }
            else if (valueStr.IndexOf("{ItemProperty:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                selectedValue = ConvertItemPropertyToSelectedValue(valueStr, workflowActionProcessor, throwException);
            }
            return selectedValue;
        }


        /// <summary>
        /// Convert &lt;PrimitiveValue Value="{ItemProperty:ID}" ValueType="Text" /&gt; or &lt;PrimitiveValue Value="9" ValueType="Text" /&gt; to
        /// &lt;Value&gt;
        ///    &lt;ListLookup&gt;
        ///        &lt;SelectList&gt;[Current Item]&lt;/SelectList&gt;
        ///        &lt;SelectField&gt;ID&lt;/SelectField&gt;
        ///        &lt;SelectFieldType&gt;Int32&lt;/SelectFieldType&gt;
        ///        &lt;WhereField /&gt;
        ///        &lt;WhereFieldType /&gt;
        ///        &lt;WhereValue /&gt;
        ///        &lt;DisplayName&gt;Current Item&lt;/DisplayName&gt;
        ///        &lt;DisplayValue&gt;ID&lt;/DisplayValue&gt;
        ///    &lt;/ListLookup&gt;
        ///    &lt;Coercion&gt;AsDNDoubleFromInt32&lt;/Coercion&gt;
        /// &lt;/Value&gt;
        /// </summary>
        /// <param name="sourcePrimitiveValue">like &lt;PrimitiveValue Value="{ItemProperty:ID}" ValueType="Text" /&gt;</param>
        /// <param name="parametersDataType">the data type of destination parameters</param>
        /// <param name="workflowActionProcessor"></param>
        /// <returns></returns>
        public static ParametersValue ConvertPrimitiveValueToParametersValue(Native13NinTexWorkflowEntity.PrimitiveValue sourcePrimitiveValue, string parametersDataType, NintexWFActionProcessor workflowActionProcessor, bool throwException)
        {
            var parametersValue = new ParametersValue();

            if (sourcePrimitiveValue.Value.IndexOf("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                parametersValue = ConvertWorkflowVariableToValue(sourcePrimitiveValue.Value, parametersDataType, workflowActionProcessor);
            }
            else if (sourcePrimitiveValue.Value.IndexOf("{Common:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                parametersValue = ConvertWorkflowContextToValue(sourcePrimitiveValue.Value, parametersDataType, throwException);
            }
            else if (sourcePrimitiveValue.Value.IndexOf("{ItemProperty:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                parametersValue = ConvertItemPropertyToValue(sourcePrimitiveValue.Value, parametersDataType, workflowActionProcessor, throwException);
            }
            else
            {
                parametersValue = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue(parametersDataType, sourcePrimitiveValue.Value)
                };
            }

            return parametersValue;
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
        public static PrimitiveValue ConvertToPrimitiveValue(NintexWFActionProcessor workflowActionProcessor, ValueLookup listLookup, Native13NinTexWorkflowEntity.PrimitiveValue primitiveValue, WorkflowContextData workflowContextData, Coercion coercion, NWWorkflowVariable variable)
        {
            //List Lookup
            if (listLookup != null)
            {
                return new PrimitiveValue("String", "{0}")
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
            }

            //Value
            if (primitiveValue != null && !string.IsNullOrEmpty(primitiveValue.Value))
            {
                return ConvertPrimitiveValue(primitiveValue, workflowActionProcessor, true);
            }

            //Workflow Context
            if (workflowContextData != null)
            {
                return new PrimitiveValue("String", "{0}")
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
            }

            //Workflow Variables
            if (variable != null)
            {
                return new PrimitiveValue("String", "{0}")
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
            }
            return null;
        }
    }
}
