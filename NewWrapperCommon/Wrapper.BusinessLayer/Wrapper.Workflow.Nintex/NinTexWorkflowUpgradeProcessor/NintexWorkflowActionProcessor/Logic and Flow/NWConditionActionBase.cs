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

namespace LS.SPWorkflowProcessor
{
    internal abstract class NWConditionActionBase : NWActionProcessorBase
    {
        private List<string> SupportedCondition = new List<string>() {
            "If current item field equals value",
            "If any value equals value",
            "Title field contains keywords",
            "Modified in a specific date span",
            "Modified by a specific person",
            "Created in a specific date span",
            "Created by a specific person",
            "The file type is a specific type"
        };

        private NWConditionUtility conditionUtility;
        private NWConditionRuleChecker conditionRuleChecker;

        protected NWConditionActionBase(NintexWFActionProcessor workflowActionProcessor)
        : base(workflowActionProcessor)
        {
            conditionUtility = new NWConditionUtility(workflowActionProcessor);
            conditionRuleChecker = new NWConditionRuleChecker(workflowActionProcessor.DataMappingManager);
        }

        /// <summary>
        /// Filter: Single One Rule 
        /// </summary>
        /// <param name="conditionConfig"></param>
        /// <returns></returns>
        private Parameters[] BuildParameters(NWConditionConfig conditionConfig)
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.AddRange(BuildParameter(conditionConfig, ConditionOperator.And));
            return parameters.ToArray();
        }

        /// <summary>
        /// Local 结构为嵌套结构
        /// Online 为平行结构
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private Parameters[] BuildParameters(ConditionPair condition)
        {
            List<Parameters> parameters = new List<Parameters>();
            if (condition.Left != null)
            {
                if (condition.Left is NWConditionConfig)
                {
                    parameters.AddRange(BuildParameter(condition.Left as NWConditionConfig, condition.Operator));
                }
                else
                {
                    var left = condition.Left as ConditionPair;
                    parameters.AddRange(BuildParameters(left));
                }
            }
            if (condition.Right != null)
            {
                if (condition.Right is NWConditionConfig)
                {
                    parameters.AddRange(BuildParameter(condition.Right as NWConditionConfig, condition.Operator));
                }
            }
            return parameters.ToArray();
        }

        private Parameters[] BuildParameter(NWConditionConfig conditionConfig, ConditionOperator conditionOperator)
        {
            if (SupportedCondition.Contains(conditionConfig.Name))
            {
                List<Parameters> parameters = new List<Parameters>();
                var parameter = conditionUtility.GenerateParameters();
                if (conditionConfig.Name.Equals("Title field contains keywords", StringComparison.OrdinalIgnoreCase))
                {
                    var field = new Field { Name = "Title", Type = "Text" };
                    parameter.Value.Dictionary = BuildDictionaryValueArrayWithNoCondition(conditionConfig.Params, conditionOperator, field, "Contains", "_1_");
                    parameters.Add(parameter);
                }
                else if (conditionConfig.Name.Equals("Modified in a specific date span", StringComparison.OrdinalIgnoreCase)
                    || conditionConfig.Name.Equals("Created in a specific date span", StringComparison.OrdinalIgnoreCase))
                {
                    #region split condition:"Modified in a specific date span" to two conditions: A and B
                    var field = new Field { Type = "DateTime", Name = conditionConfig.Name.Equals("Modified in a specific date span", StringComparison.OrdinalIgnoreCase) ? "Modified" : "Created" };

                    parameter.Value.Dictionary = BuildDictionaryValueArrayWithCondition(conditionConfig.Params, conditionOperator, field, "GreaterThanOrEqual", "_1_");
                    var parameterTwo = conditionUtility.GenerateParameters();
                    parameterTwo.Value.Dictionary = BuildDictionaryValueArrayWithCondition(conditionConfig.Params, ConditionOperator.And, field, "LessThanOrEqual", "_2_");
                    #endregion

                    parameters.Add(parameter);
                    parameters.Add(parameterTwo);
                }
                else if (conditionConfig.Name.Equals("Modified by a specific person", StringComparison.OrdinalIgnoreCase)
                    || conditionConfig.Name.Equals("Created by a specific person", StringComparison.OrdinalIgnoreCase))
                {
                    var field = new Field { Type = "User", Name = conditionConfig.Name.Equals("Modified by a specific person", StringComparison.OrdinalIgnoreCase) ? "Editor" : "Author" };

                    parameter.Value.Dictionary = BuildDictionaryValueArrayWithCondition(conditionConfig.Params, conditionOperator, field, "Equal", "_1_");
                    parameters.Add(parameter);
                }
                else if (conditionConfig.Name.Equals("The file type is a specific type", StringComparison.OrdinalIgnoreCase))
                {
                    var field = new Field { Name = "File_x0020_Type", Type = "String" };
                    parameter.Value.Dictionary = BuildDictionaryValueArrayWithNoCondition(conditionConfig.Params, conditionOperator, field, "Equal", "_1_");
                    parameters.Add(parameter);
                }
                else
                {
                    parameter.Value.Dictionary = BuildDictionaryValueArray(conditionConfig.Params, conditionOperator);
                    parameters.Add(parameter);
                }
                return parameters.ToArray();
            }
            else
            {
                throw new UnSupportedSettingException("Unsupported condition:{0}", conditionConfig.Name);
            }
        }

        protected FormatValues CreateFormatValuesWithPrimitiveValue(string taskPropertyValue, string taskPropertyType)
        {
            return new FormatValues
            {
                SelectedValue = new SelectedValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("{0}"),
                        FormatValues = new List<FormatValues>
                    {
                        new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                Coercion="AsDNString",
                                TaskProperty=new TaskProperty
                                {
                                    Value =taskPropertyValue,
                                    Type =taskPropertyType
                                }
                            },
                        }
                    }
                    }
                }
            };
        }

        protected FormatValues CreateFormatValuesWithoutPrimitiveValue(string taskPropertyValue, string taskPropertyType)
        {
            return new FormatValues
            {
                SelectedValue = new SelectedValue
                {
                    Coercion = "AsDNString",
                    TaskProperty = new TaskProperty
                    {
                        Value = taskPropertyValue,
                        Type = taskPropertyType,
                    },
                }
            };
        }
        private bool IsNotSupportLeft(NWConditionConfigParam leftConfigParam)
        {
            //text 类型需要特殊处理
            if (leftConfigParam.PrimitiveValue != null && !string.IsNullOrEmpty(leftConfigParam.PrimitiveValue.Value))
            {
                return NWCommonUtility.SplitString(leftConfigParam.PrimitiveValue.Value).Count > 1;
                {
                    throw new UnSupportedSettingException("The destination left condition does not support multiple references");
                }
            }
            return false;
        }
        /// <summary>
        /// Convert input parameters to Value type like
        /// &lt;Value&gt;
        ///   &lt;WorkflowContext&gt;
        ///     &lt;Value&gt;AssociationTitle&lt;/Value&gt;
        ///     &lt;Type&gt;String&lt;/Type&gt;
        ///   &lt;/WorkflowContext&gt;
        /// &lt;/Value&gt;
        /// </summary>
        /// <param name="workflowActionProcessor"></param>
        /// <param name="listLookup"></param>
        /// <param name="primitiveValue"></param>
        /// <param name="workflowContextData"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private  Value ConvertLeftParamValue(NintexWFActionProcessor workflowActionProcessor, NWConditionConfigParam conditionConfigParam)
        {
            //text 类型需要特殊处理
            if (conditionConfigParam.PrimitiveValue != null && !string.IsNullOrEmpty(conditionConfigParam.PrimitiveValue.Value))
            {
                return NWValueConverter.ConvertPrimitiveValueToValue(workflowActionProcessor, conditionConfigParam.PrimitiveValue);
            }


            return NWValueConverter.ConvertValue(workflowActionProcessor, conditionConfigParam);
        }

        private DictionaryValue BuildDictionaryValue(NWConditionConfigParam param, DictionaryValue leftDictionaryValue, out string conditionType)
        {
            conditionType = string.Empty;
            var dicValue = conditionUtility.ConvertToDictionaryValue(param);

            if (dicValue.Key.Equals("left", StringComparison.OrdinalIgnoreCase))
            {
                if (IsNotSupportLeft(param))
                {
                    throw new UnSupportedSettingException("The destination left condition does not support multiple references");
                }
                dicValue.Value = ConvertLeftParamValue(base.workflowActionProcessor, param);
            }
            else if (dicValue.Key.Equals("right", StringComparison.OrdinalIgnoreCase) && leftDictionaryValue != null)
            {
                if (string.Equals("CurrentDate", param.SpecialReference, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("CurrentDateTime", param.SpecialReference, StringComparison.OrdinalIgnoreCase))
                {
                    dicValue.Value = new Value()
                    {
                        PrimitiveValue =
                        new PrimitiveValue
                        {
                            Type = "DateTime",
                            Value = new Value
                            {
                                DateTimeInfo = new DateTimeInfo
                                {
                                    UseCurrentDate = true
                                }
                            },
                        }
                    };
                }
                else
                {
                    dicValue.Value = ConvertLeftParamValue(base.workflowActionProcessor, param);
                    //dicValue.Value = NWValueConverter.ConvertValue(base.workflowActionProcessor, param);
                }

                dicValue.Value = conditionUtility.ConvertToTextBuilderModeValue(leftDictionaryValue, dicValue.Value);
            }

            if (dicValue.Value != null)
            {
                conditionType = conditionUtility.GetConditionType(dicValue.Value);
            }
            return dicValue;
        }

        private DictionaryValue BuildDictionaryValueV2(DictionaryValue leftDictionaryValue, NWConditionConfigParam param)
        {
            var dicValue = conditionUtility.ConvertToDictionaryValue(param);
            dicValue.Value = NWValueConverter.ConvertValueWithTextBuilderMode(base.workflowActionProcessor, param.ListLookup, param.PrimitiveValue, param.WorkflowContextData, param.Coercion, param.Variable, param.ProfileLookup, param.WorkflowConstant);
            if (param.Name.Equals("right", StringComparison.OrdinalIgnoreCase))
            {
                dicValue.Value = conditionUtility.ConvertToTextBuilderModeValue(leftDictionaryValue, dicValue.Value);
            }
            return dicValue;
        }

        private DictionaryValue BuildOperatorDictionaryValue(NWConditionConfigParam param, string conditionType)
        {
            var dicValue = conditionUtility.ConvertToDictionaryValue(param);
            dicValue.Value = new Value();
            dicValue.Value.PrimitiveValue = conditionUtility.ConvertPrimitiveValue(param.PrimitiveValue, conditionType, true);
            return dicValue;
        }

        private DictionaryValue[] BuildDictionaryValueArray(NWConditionConfigParam[] nwConditionConfigParams, ConditionOperator conditionOperator)
        {
            List<DictionaryValue> dictionaryValues = new List<DictionaryValue>();
            dictionaryValues.Add(conditionUtility.GenerateLogicDictionaryValue(conditionOperator));
            string conditionType = string.Empty;
            var paramLeft = conditionUtility.FindParamCloneByName(nwConditionConfigParams, "left");
            dictionaryValues.Add(BuildDictionaryValue(paramLeft, null, out conditionType));
            var paramOperator = conditionUtility.FindParamCloneByName(nwConditionConfigParams, "operator");
            dictionaryValues.Add(BuildOperatorDictionaryValue(paramOperator, conditionType));
            if (paramOperator.PrimitiveValue != null)
            {
                if (paramOperator.PrimitiveValue.Value.ToLower().Contains("IsEmpty".ToLower()))
                {
                    throw new NotSupportedException("Not surpport 'Empty' condition");
                }
            }
            if ((conditionType.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase)
                || conditionType.Equals("UserMulti", StringComparison.OrdinalIgnoreCase)
                || conditionType.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase)
                || conditionType.Equals("Lookup", StringComparison.OrdinalIgnoreCase)
                || conditionType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)
                || conditionType.Equals("DynamicValue", StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnSupportedSettingException(string.Format("Office 365 nintex workflow doesn't support where condition:{0} for multiple-valued column ,Lookup column or collection type variable.", paramOperator.PrimitiveValue.Value));
            }
            var paramRight = conditionUtility.FindParamCloneByName(nwConditionConfigParams, "right");
            var rightDictionaryValue = BuildDictionaryValue(paramRight, dictionaryValues[1], out conditionType);
            if (rightDictionaryValue.Value != null)
            {
                dictionaryValues.Add(rightDictionaryValue);
            }
            conditionRuleChecker.CheckCondition(dictionaryValues[1], dictionaryValues[2]);
            return dictionaryValues.ToArray();
        }

        private List<DictionaryValue> BuildCommonDictionaryValueArray(NWConditionConfigParam[] nwConditionConfigParams, ConditionOperator conditionOperator, Field field, Native13NinTexWorkflowEntity.PrimitiveValue operatorPrimitiveValue)
        {
            List<DictionaryValue> dictionaryValues = new List<DictionaryValue>();
            dictionaryValues.Add(conditionUtility.GenerateLogicDictionaryValue(conditionOperator));
            string conditionType = string.Empty;
            var paramLeft = new NWConditionConfigParam()
            {
                Name = "left",
                ListLookup = new ValueLookup()
                {
                    LookupType = SLLookupType.ThisItemLookupTopLevel,
                    Field = field
                }
            };
            dictionaryValues.Add(BuildDictionaryValue(paramLeft, null, out conditionType));

            var paramOperator = new NWConditionConfigParam()
            {
                Name = "operator",
                PrimitiveValue = operatorPrimitiveValue
            };
            dictionaryValues.Add(BuildOperatorDictionaryValue(paramOperator, conditionType));

            return dictionaryValues;
        }

        /// <summary>
        /// For condition "Title field contains keywords" or "The file type is a specific type"
        /// </summary>
        /// <param name="nwConditionConfigParams"></param>
        /// <param name="conditionOperator"></param>
        /// <param name="rightParamName"></param>
        /// <returns></returns>
        private DictionaryValue[] BuildDictionaryValueArrayWithNoCondition(NWConditionConfigParam[] nwConditionConfigParams, ConditionOperator conditionOperator, Field field, string whereCondition, string rightParamName)
        {
            List<DictionaryValue> dictionaryValues = BuildCommonDictionaryValueArray(nwConditionConfigParams, conditionOperator, field, new Native13NinTexWorkflowEntity.PrimitiveValue() { Value = whereCondition, ValueType = "Choice" });

            var paramRight = conditionUtility.FindParamCloneByName(nwConditionConfigParams, rightParamName);
            paramRight.Name = "right";
            dictionaryValues.Add(BuildDictionaryValueV2(dictionaryValues[1], paramRight));
            return dictionaryValues.ToArray();
        }

        /// <summary>
        /// For condition "Modified or created in a specific data span"
        /// </summary>
        /// <param name="nwConditionConfigParams"></param>
        /// <param name="conditionOperator"></param>
        /// <param name="field"></param>
        /// <param name="rightParamName"></param>
        /// <returns></returns>
        private DictionaryValue[] BuildDictionaryValueArrayWithCondition(NWConditionConfigParam[] nwConditionConfigParams, ConditionOperator conditionOperator, Field field, string whereCondition, string rightParamName)
        {
            List<DictionaryValue> dictionaryValues = BuildCommonDictionaryValueArray(nwConditionConfigParams, conditionOperator, field, new Native13NinTexWorkflowEntity.PrimitiveValue() { Value = whereCondition, ValueType = "Choice" });

            string conditionType = string.Empty;
            var paramRight = conditionUtility.FindParamCloneByName(nwConditionConfigParams, rightParamName);
            paramRight.Name = "right";
            dictionaryValues.Add(BuildDictionaryValue(paramRight, dictionaryValues[1], out conditionType));
            return dictionaryValues.ToArray();
        }

        protected Parameters[] BuildParameters(ConditionConfig condition)
        {
            if (condition is NWConditionConfig)
            {
                return BuildParameters(condition as NWConditionConfig);
            }
            if (condition is ConditionPair)
            {
                return BuildParameters(condition as ConditionPair);
            }
            throw new Exception(string.Format("Not expected type. Type: {0}", condition.GetType().Name));
        }

    }
}
