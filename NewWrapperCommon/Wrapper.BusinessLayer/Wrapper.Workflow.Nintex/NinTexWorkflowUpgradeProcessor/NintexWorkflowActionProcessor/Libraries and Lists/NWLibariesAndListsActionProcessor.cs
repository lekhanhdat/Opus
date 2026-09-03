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
using System;
using Native13NinTexWorkflowEntity;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    abstract class NWLibariesAndListsActionProcessor : NintexOffice365ActionBase
    {
        public NWLibariesAndListsActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {

        }

        /// <summary>
        /// 用于创建Properteis/Property/Parameters/:Name=ListId 节点
        /// 这个节点只有两个属性需要赋值，DisplayName， SelectList
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        protected Parameters CreateListIdParameter(ActivityParameter listId, ActivityParameter lookupField, ActivityParameter lookupFieldType, ActivityParameter thisItemValue = null)
        {
            //用来处理 WorkflowAction/Configuration/Properties/Property/Parameters(Name=="ListId")
            var para = new Parameters();
            para.Name = "ListId";
            para.Required = true;
            para.DataType = "Guid";
            //para.DesignerType = "None";
            para.Direction = "Input";
            para.DependentOn = string.Empty;
            para.OriginalSelectedValue = string.Empty;
            para.Properties = new ParametersProperties();

            //用来处理 WorkflowAction/Configuration/Properties/Property/Parameters(Name=="ListId")/Value/ListLookup
            para.Value = new ParametersValue();
            var isCurrentItem = IsCurrentItem(thisItemValue);
            ValueLookup lookup = new ValueLookup
            {
                LookupType = isCurrentItem ? SLLookupType.ThisItemLookup : SLLookupType.CrossItemLookup,
                ListId = isCurrentItem ? string.Empty : listId.PrimitiveValue.Value,
            };
            para.Value.ListLookup = NWListLookupConverter.ConvertListLookup(lookup, base.workflowActionProcessor);
            return para;
        }

        protected Parameters CreateItemGuidParameter(ActivityParameter listId, ActivityParameter lookupField, ActivityParameter lookupFieldType, ActivityParameter lookupFieldValue, ActivityParameter thisItemValue = null)
        {

            //用来处理 WorkflowAction/Configuration/Properties/Property/Parameters(Name=="ItemGuid")
            var para2 = new Parameters();
            para2.Name = "ItemGuid";
            para2.Required = true;
            para2.DataType = "Guid";
            para2.Direction = "Input";
            para2.DependentOn = string.Empty;
            para2.OriginalSelectedValue = string.Empty;
            para2.Properties = new ParametersProperties();

            //用来处理 WorkflowAction/Configuration/Properties/Property/Parameters(Name=="ItemGuid")/Value
            para2.Value = new ParametersValue();

            if (IsCurrentItem(thisItemValue))
            {
                para2.Value.PrimitiveValue = new PrimitiveValue { Type = "Guid", Value = new Value("[Unique Id]") };
            }
            else
            {
                ValueLookup valueLookup = new ValueLookup
                {
                    LookupType = SLLookupType.CrossItemLookup,
                    ListId = listId.PrimitiveValue.Value,
                    CompareField = new Field { Name = lookupField.PrimitiveValue.Value, Type = lookupFieldType.PrimitiveValue.Value },
                    Coercion = lookupField.Coercion,
                    Lookup = lookupFieldValue.ListLookup,
                    PrimitiveValue = lookupFieldValue.PrimitiveValue,
                    Variable = lookupFieldValue.Variable,
                    WorkflowContextData = lookupFieldValue.WorkflowContextData,
                };
                para2.Value.ListLookup = NWListLookupConverter.ConvertListLookup(valueLookup, base.workflowActionProcessor);
                para2.Value.ListLookup.WhereValue = ConvertToTextBuilderModeValue(para2.Value.ListLookup);
                para2.Value.ListLookup.DisplayName = string.Empty;

                #region 原来的逻辑 暂时先留着 
                //用来处理 WorkflowAction/Configuration/Properties/Property/Parameters(Name=="ItemGuid")/Value/ListLookup
                //para2.Value.ListLookup = new ListLookup();
                //var selectList = GetListIdFromMapping(listId.PrimitiveValue.Value);
                //para2.Value.ListLookup.SelectList = selectList;
                //para2.Value.ListLookup.WhereField = lookupField.PrimitiveValue.Value;
                //para2.Value.ListLookup.WhereFieldType = GetMappingTypeFromWorkflowContext(lookupFieldType.PrimitiveValue.Value);
                //para2.Value.ListLookup.SelectField = string.Empty;
                //para2.Value.ListLookup.SelectFieldType = string.Empty;
                //para2.Value.ListLookup.DisplayName = string.Empty;
                //para2.Value.ListLookup.DisplayValue = string.Empty;

                //para2.Value.ListLookup.WhereValue = new Value();



                //if (lookupField.Coercion != null)
                //{
                //    para2.Value.ListLookup.Coercion = lookupField.Coercion.Value;
                //}

                //if (lookupFieldValue.ListLookup != null)
                //{
                //    para2.Value.ListLookup.WhereValue.ListLookup = NWListLookupConverter.ConvertListLookup(lookupFieldValue.ListLookup, base.workflowActionProcessor);// ConvertListLookup(lookupFieldValue.ListLookup);
                //}
                //else if (lookupFieldValue.PrimitiveValue != null)
                //{
                //    //para2.Value.ListLookup.WhereValue.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(lookupFieldValue.PrimitiveValue, base.workflowActionProcessor.Web.RegionalSettings.TimeZone);
                //    if (lookupFieldValue.PrimitiveValue.Value.StartsWith("{Common:"))
                //    {//Insert common插入的某些变量应该转成WorkflowContext

                //        para2.Value.ListLookup.WhereValue.PrimitiveValue = CreatePrimitiveValue(GetMappingTypeFromWorkflowContext(lookupFieldValue.PrimitiveValue.ValueType), "{0}");
                //        para2.Value.ListLookup.WhereValue.PrimitiveValue.FormatValues = new List<SelectedValue>
                //        {
                //            new SelectedValue
                //            {
                //                WorkflowContext = new WorkflowContext
                //                {
                //                    Value = GetMappedWorkflowInsertCommonValue(lookupFieldValue.PrimitiveValue.Value),
                //                    Type = GetMappingTypeFromWorkflowContext(lookupFieldValue.PrimitiveValue.ValueType)
                //                },
                //                //Coercion = lookupFieldValue.Coercion.Value//??
                //            }
                //        };
                //        //需要加到Lists文件中的Field
                //        this.workflowActionProcessor.AddField(new Guid(listId.PrimitiveValue.Value), lookupField.PrimitiveValue.Value);
                //    }
                //    else
                //    {
                //        para2.Value.ListLookup.WhereValue.PrimitiveValue = CreatePrimitiveValue(lookupFieldValue.PrimitiveValue.ValueType, lookupFieldValue.PrimitiveValue.Value);
                //    }
                //}

                //else if (lookupFieldValue.Variable != null)
                //{
                //    para2.Value.ListLookup.WhereValue.Variable = NWVariableGetter.GetSimpleVariable(lookupFieldValue.Variable);
                //}
                //else if (lookupFieldValue.WorkflowContextData != null)
                //{
                //    para2.Value.ListLookup.WhereValue.PrimitiveValue = new PrimitiveValue { Type = GetMappingTypeFromWorkflowContext(lookupFieldValue.WorkflowContextData.Type), Value = new Value("{0}") };
                //    para2.Value.ListLookup.WhereValue.PrimitiveValue.FormatValues = new List<SelectedValue>
                //        {
                //            new SelectedValue
                //            {
                //                WorkflowContext = new WorkflowContext
                //                {
                //                    Value = GetMappedWorkflowContextValue(lookupFieldValue.WorkflowContextData.Name),
                //                    Type = GetMappingTypeFromWorkflowContext(lookupFieldValue.WorkflowContextData.Type)
                //                },
                //                Coercion = lookupFieldValue.Coercion.Value//??
                //            }};
                //}

                //this.workflowActionProcessor.AddField(new Guid(selectList), lookupField.PrimitiveValue.Value);
                #endregion
            }


            return para2;
        }

        private Value ConvertToTextBuilderModeValue(ListLookup listLookup)
        {
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

            if (NWListLookupConverter.TextBuilderModeFieldType.Contains(listLookup.WhereFieldType))
            {
                FillValue(listLookup.WhereFieldType, listLookup.WhereValue, ref tempValue);
                beConverted = true;
            }
            else
            {
                string fromDataType = string.Empty;
                if (listLookup.WhereValue.ListLookup != null)
                {
                    fromDataType = listLookup.WhereValue.ListLookup.SelectFieldType;
                }
                else if (listLookup.WhereValue.Variable != null)
                {
                    fromDataType = listLookup.WhereValue.Variable.DataType;
                }
                else if (listLookup.WhereValue.WorkflowContext != null)
                {
                    fromDataType = listLookup.WhereValue.WorkflowContext.Type;
                }

                listLookup.WhereValue.Coercion = NWCoercionStringProcessor.GetCoercionString(listLookup.WhereFieldType, fromDataType);
            }

            if (!beConverted)
            {
                tempValue = listLookup.WhereValue;
            }

            return tempValue;
        }

        private void FillValue(string leftDataType, Value value, ref Value finalValue)
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

        protected static bool IsCurrentItem(ActivityParameter thisItemValue)
        {
            return thisItemValue != null && thisItemValue.PrimitiveValue != null && string.Equals(thisItemValue.PrimitiveValue.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        protected Value ConvertToValue(NWFieldReference nwFieldReference)
        {
            var value = new Value();
            if (nwFieldReference.PrimitiveValue != null)
            {
                value.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(nwFieldReference.PrimitiveValue, base.workflowActionProcessor, true);
            }
            else
            {
                string destDataType = NWFieldTypeMapping.ConvertFieldType(nwFieldReference.Type);

                if (nwFieldReference.WorkflowContextData != null)
                {
                    value.WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(nwFieldReference.WorkflowContextData);
                    value.Coercion = NWCoercionStringProcessor.GetCoercionString(destDataType, value.WorkflowContext.Type);
                }
                else if (nwFieldReference.Variable != null)
                {
                    value.Variable = this.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(nwFieldReference.Variable);
                    value.Coercion = NWCoercionStringProcessor.GetCoercionString(destDataType, value.Variable.DataType);
                }
                else if (nwFieldReference.ListLookup != null)
                {
                    value.ListLookup = NWListLookupConverter.ConvertListLookup(nwFieldReference.ListLookup, base.workflowActionProcessor);
                    value.Coercion = NWCoercionStringProcessor.GetCoercionString(destDataType, value.ListLookup.SelectFieldType);
                }
                else
                {
                    throw new NotSupportedException(string.Format("can not support data, ProfileLookup: {0}, WorkflowConstant: {1}", nwFieldReference.ProfileLookup == null, nwFieldReference.WorkflowConstant == null));
                }

                if (NWListLookupConverter.TextBuilderModeFieldType.Contains(nwFieldReference.Type))
                {
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

                    FillValue(destDataType, value, ref tempValue);
                    value = tempValue;
                }
            }
            return value;
        }

        protected PrimitiveValue ConvertToPrimitiveValue(ActivityParameter activityParameter)
        {
            var primitiveValue = new PrimitiveValue();
            if (activityParameter.PrimitiveValue != null)
            {
                primitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(activityParameter.PrimitiveValue, base.workflowActionProcessor, true);
            }
            else
            {
                primitiveValue = new PrimitiveValue { Type = "String", Value = new Value("{0}"), FormatValues = new List<FormatValues>() };
                if (activityParameter.ListLookup != null)
                {
                    primitiveValue.FormatValues.Add(
                        new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                ListLookup = NWListLookupConverter.ConvertListLookup(activityParameter.ListLookup, base.workflowActionProcessor)
                            }
                        });
                }
                else if (activityParameter.WorkflowContextData != null)
                {
                    primitiveValue.FormatValues.Add(
                        new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(activityParameter.WorkflowContextData),
                            }
                        });
                }
                else if (activityParameter.Variable != null)
                {
                    primitiveValue.FormatValues.Add(
                        new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                Variable = this.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(activityParameter.Variable),
                            }
                        });
                }
                else
                {
                    throw new NotSupportedException("Can not support data.");
                }
                if (activityParameter.Coercion != null)
                {
                    primitiveValue.FormatValues[0].SelectedValue.Coercion = activityParameter.Coercion.Value;
                }
            }
            return primitiveValue;
        }

        protected virtual List<DictionaryValue> GetListItemsParameters(ActivityParameter listId)
        {
            List<DictionaryValue> values = new List<DictionaryValue>();

            foreach (var nwFieldReference in sourceConfig.FieldReferences)
            {
                values.Add(new DictionaryValue
                {
                    //Value中存的应该是Field的internal name
                    Key = nwFieldReference.Value,
                    Value = ConvertToValue(nwFieldReference),
                });
            }
            return values;
        }
    }
}
