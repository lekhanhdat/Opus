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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow.Nintex;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    class NWQueryListActionProcessor : NWLibariesAndListsActionProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(NWQueryListActionProcessor));

        public NWQueryListActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#QuerySPList";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;

            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };

            return action;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1470360014592",
                ClassName = CLASSNAME,
                x49x49 = 98,
                y49x49 = 395,
                x30x30 = 98,
                y30x30 = 444,
                x16x16 = 131,
                y16x16 = 444
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter query = null;
            ActivityParameter output = null;
            ActivityParameter isSetToBuilderMode = null;
            ActivityParameter storeRawValue = null;
            ActivityParameter baseUrl = null;
            ActivityParameter xmlEncodeCaml = null;
            ActivityParameter itemLimitLookup = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "Query", StringComparison.OrdinalIgnoreCase))
                {
                    query = para;
                }
                else if (string.Equals(para.Name, "Output", StringComparison.OrdinalIgnoreCase))
                {
                    output = para;
                }
                else if (string.Equals(para.Name, "IsSetToBuilderMode", StringComparison.OrdinalIgnoreCase))
                {
                    isSetToBuilderMode = para;
                }
                else if (string.Equals(para.Name, "StoreRawValue", StringComparison.OrdinalIgnoreCase))
                {
                    storeRawValue = para;
                }
                else if (string.Equals(para.Name, "BaseUrl", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = para;
                    if (baseUrl.PrimitiveValue != null && !string.IsNullOrEmpty(baseUrl.PrimitiveValue.Value))
                    {
                        throw new UnSupportedSettingException(WrapperNintexWorkflowResource.UnSupportedSetting, CLASSNAME, "Alternative site");
                    }
                }
                else if (string.Equals(para.Name, "XmlEncodeCaml", StringComparison.OrdinalIgnoreCase))
                {
                    xmlEncodeCaml = para;
                }
                else if (string.Equals(para.Name, "ItemLimitLookup", StringComparison.OrdinalIgnoreCase))
                {
                    itemLimitLookup = para;
                }
            }

            var querySPList = new Property
            {
                ID = "QuerySPList",
                DesignerType = "ListQueryBuilder",
                DisplayName = "Output",
                Parameters = GenerateQuerySPListParameters(query, output, itemLimitLookup)
            };

            var outResultCount = new Property
            {
                ID = "OutResultCount",
                DesignerType = "Variable",
                DisplayName = "Result count",
                Parameters = new Parameters[] {
                    GenerateOutResultCountParameters()
                }
            };

            return new List<Property> { querySPList, outResultCount };
        }

        private Parameters GenerateOutResultCountParameters()
        {
            var outResultCountParameters = new Parameters
            {
                Name = "OutResultCount",
                Value = new ParametersValue
                {
                    Variable = new Variable()
                },
                Required = false,
                DataType = "Int32",
                DesignerType = "Variable",
                Direction = "Output"
            };

            return outResultCountParameters;
        }

        private void CheckUnSupportData(ValueStorageCollection storageItems)
        {
            if (storageItems == null)
            {
                return;
            }

            foreach (var storageItem in storageItems)
            {
                if (string.IsNullOrEmpty(storageItem.ValueIdentifier))
                {
                    continue;
                }

                if (storageItem.ValueIdentifier.IndexOf("{Common:", StringComparison.OrdinalIgnoreCase) >= 0
                 || storageItem.ValueIdentifier.IndexOf("{WFConstant:", StringComparison.OrdinalIgnoreCase) >= 0
                 || storageItem.ValueIdentifier.IndexOf("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) >= 0
                 || storageItem.ValueIdentifier.IndexOf("{ItemProperty:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new UnSupportedDataException(string.Format("query list action not support the ValueIdentifier: {0}", storageItem.ValueIdentifier));
                }
            }
        }

        private Parameters[] GenerateQuerySPListParameters(ActivityParameter queryPara, ActivityParameter outputPara, ActivityParameter itemLimitLookupPara)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(System.Web.HttpUtility.HtmlDecode(queryPara.PrimitiveValue.Value)); //queryPara.PrimitiveValue.Value存储的是caml

            string srcListIdOrTitle = string.Empty;
            if (xd.SelectSingleNode("/Query/Lists/List").Attributes["ID"] != null)
            {
                srcListIdOrTitle = xd.SelectSingleNode("/Query/Lists/List").Attributes["ID"].Value;
            }
            else
            {
                srcListIdOrTitle = xd.SelectSingleNode("/Query/Lists/List").Attributes["Title"].Value;
            }
            var targetListId = new Parameters
            {
                Name = "TargetListId",
                Value = new ParametersValue
                {
                    ListLookup = new ListLookup
                    {
                        SelectList = srcListIdOrTitle
                    }
                },
                Required = true,
                DataType = "Guid",
                DesignerType = "Hidden",
                Direction = "Input"
            };
            var editorMode = new Parameters
            {
                Name = "EditorMode",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("Int32", "0")
                },
                Required = true,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
            };

            CheckUnSupportData(sourceConfig.ValueStorageCollection.ValueStorageItems);
            CheckUnsupportedActionType(itemLimitLookupPara);
            var oDataTop = CreateODataTopParameter(itemLimitLookupPara);

            var oDataFilter = new Parameters()
            {
                Name = "ODataFilter",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "String"
                    }
                },
                Description = "OData query filter",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
            };

            var oDataSort = new Parameters
            {
                Name = "ODataSort",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", GetOrderbyString(xd.SelectSingleNode("/Query/OrderBy")))
                },
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
            };

            var oDataSelect = new Parameters
            {
                Name = "ODataSelect",
                Value = GenerateODataSelectParametersValue(sourceConfig.ValueStorageCollection.ValueStorageItems, xd.SelectSingleNode("/Query/ViewFields").FirstChild.Attributes["Name"].Value),
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
            };

            var oDataExpand = new Parameters
            {
                Name = "ODataExpand",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", "")
                },
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
            };

            var caml = new Parameters
            {
                Name = "Caml",
                Value = new ParametersValue
                {
                    PrimitiveValue = GenerateCamlPrimitiveValue(xd.SelectSingleNode("/Query/Where"))
                },
                Required = false,
                DataType = "String",
                DesignerType = "Hidden",
                Direction = "Input"
            };

            var parameterVariableMap = new Parameters
            {
                Name = "ParameterVariableMap",
                Value = GenerateParameterVariableMappPrimitiveValue(sourceConfig.ValueStorageCollection.ValueStorageItems, outputPara, xd.SelectSingleNode("/Query/ViewFields").FirstChild.Attributes["Name"].Value),
                Required = true,
                DataType = "Dictionary",
                DesignerType = "Hidden",
                Direction = "Input"
            };

            var isArrayOutput = new Parameters
            {
                Name = "IsArrayOutput",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("Boolean", "true")
                },
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input"
            };

            var outResponse = new Parameters
            {
                Name = "OutResponse",
                Required = false,
                DataType = "DynamicValue",
                Type = "Array",
                DesignerType = "Variable",
                Direction = "Output"
            };

            List<Parameters> parametersArray = new List<Parameters> { targetListId, editorMode, oDataTop, oDataFilter, oDataSort, oDataSelect, oDataExpand, caml, parameterVariableMap, isArrayOutput, outResponse };

            if (sourceConfig.ValueStorageCollection.ValueStorageItems.Count > 0)
            {
                foreach (var storageItem in sourceConfig.ValueStorageCollection.ValueStorageItems)
                {
                    var tmpParameters = new Parameters
                    {
                        Name = Guid.NewGuid().ToString(),
                        Value = new ParametersValue
                        {
                            Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(storageItem.VariableName),
                        },
                        Description = storageItem.ValueIdentifier,
                        Required = true,
                        DataType = "Any",
                        DefaultType = "Array",
                        DesignerType = "Variable",
                        Direction = "Output"
                    };

                    parametersArray.Add(tmpParameters);
                }
            }
            else
            {
                var tmpParameters = new Parameters
                {
                    Name = Guid.NewGuid().ToString(),
                    Value = outputPara.Variable == null ? new ParametersValue { } : new ParametersValue { Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outputPara.Variable.Name) },
                    Description = xd.SelectSingleNode("/Query/ViewFields").FirstChild.Attributes["Name"].Value,
                    Required = true,
                    DataType = "Any",
                    DefaultType = "Array",
                    DesignerType = "Variable",
                    Direction = "Output"
                };

                parametersArray.Add(tmpParameters);
            }


            return parametersArray.ToArray();
        }

        private string GetOrderbyString(XmlNode orderByNode)
        {
            if (orderByNode == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (XmlNode child in orderByNode.ChildNodes)
            {
                sb.AppendFormat("{0} {1},", child.Attributes["Name"].Value, (Convert.ToBoolean(orderByNode.FirstChild.Attributes["Ascending"].Value) ? "asc" : "desc"));
            }
            if (sb.Length > 0)
            {
                sb.Length--;
            }
            return sb.ToString();

        }
        private ParametersValue GetDatatopParameterDefaultValue()
        {
            return new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue("Int32", "100")
            };
        }

        private void ProcessPrimitiveValueFormatForODatatopParameterValue(Parameters oDataTop)
        {
            int temp;
            if (!int.TryParse(oDataTop.Value.PrimitiveValue.Value.StringValue, out temp))
            {
                logger.Warn("ODataTop only can format int type data, wrong data is {0}", oDataTop.Value.PrimitiveValue.Value.StringValue);
                oDataTop.Value.PrimitiveValue.Value.StringValue = "100";
            }
        }

        private void ProcessVariableFormatForODatatopParameterValue(Parameters oDataTop)
        {
            var tempVariable = oDataTop.Value.Variable;
            if (string.Equals("User", tempVariable.DataType, StringComparison.OrdinalIgnoreCase))
            {
                oDataTop.Value.Coercion = "UserIDAsInteger";
            }
            else if (string.Equals("Boolean", tempVariable.DataType, StringComparison.OrdinalIgnoreCase)
               || string.Equals("Guid", tempVariable.DataType, StringComparison.OrdinalIgnoreCase)
               || string.Equals("DateTime", tempVariable.DataType, StringComparison.OrdinalIgnoreCase)
               || string.Equals("DynamicValue", tempVariable.DataType, StringComparison.OrdinalIgnoreCase))
            {
                logger.Warn("ODataTop only can not format this type, use default value to format.Wrong data typeis {0}", tempVariable.DataType);
                oDataTop.Value = GetDatatopParameterDefaultValue();
            }
        }

        private void ProcessWorkflowContextFormatForODatatopParameterValue(Parameters oDataTop)
        {
            var tempWorkflowContext = oDataTop.Value.WorkflowContext;
            //ADO-187067 Coercion对User类型有特殊处理
            if (string.Equals("User", tempWorkflowContext.Type, StringComparison.OrdinalIgnoreCase))
            {
                oDataTop.Value.Coercion = "UserIDAsInteger";
            }
            else
            {
                oDataTop.Value.Coercion = "AsDNInt32FromString";
            }
        }

        private void ProcessListLookupFormatForODatatopParameterValue(Parameters oDataTop)
        {
            var tempListLookup = oDataTop.Value.ListLookup;
            if (string.Equals("Lookup", tempListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase))
            {
                oDataTop.Value.Coercion = "LookupColumnDataOnlyAsInteger";
            }
            else if (string.Equals("User", tempListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase))
            {
                oDataTop.Value.Coercion = "UserIDAsInteger";
            }
            else if (string.Equals("Boolean", tempListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase)
               || string.Equals("DateTime", tempListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase)
               || string.Equals("URL", tempListLookup.SelectFieldType, StringComparison.OrdinalIgnoreCase))
            {
                logger.Warn("ODataTop only can not format this type, use default value to format.Wrong data typeis {0}", tempListLookup.SelectFieldType);
                oDataTop.Value = GetDatatopParameterDefaultValue();
            }
        }

        private void ProcessDataFormatODatatopParameterValue(Parameters oDataTop)
        {
            if (oDataTop.Value.PrimitiveValue != null && !string.IsNullOrEmpty(oDataTop.Value.PrimitiveValue.Value.StringValue))
            {
                ProcessPrimitiveValueFormatForODatatopParameterValue(oDataTop);
            }
            else if (oDataTop.Value.Variable != null)
            {
                ProcessVariableFormatForODatatopParameterValue(oDataTop);
            }
            else if (oDataTop.Value.WorkflowContext != null)
            {
                ProcessWorkflowContextFormatForODatatopParameterValue(oDataTop);
            }
            else if (oDataTop.Value.ListLookup != null)
            {
                ProcessListLookupFormatForODatatopParameterValue(oDataTop);
            }
        }

        private Parameters CreateODataTopParameter(ActivityParameter itemLimitLookupPara)
        {
            var oDataTop = new Parameters
            {
                Name = "ODataTop",
                Value = itemLimitLookupPara.PrimitiveValue.Value.Equals(string.Empty) ?
                           GetDatatopParameterDefaultValue()
                            : NWPrimitiveValueConverter.ConvertPrimitiveValueToParametersValue(itemLimitLookupPara.PrimitiveValue, "Int32", workflowActionProcessor, true),
                Required = true,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input"
            };

            ProcessDataFormatODatatopParameterValue(oDataTop);
            return oDataTop;
        }

        private PrimitiveValue GenerateCamlPrimitiveValue(XmlNode camlWhere)
        {
            PrimitiveValue result = new PrimitiveValue();
            result.Type = "String";
            if (camlWhere != null)
            {
                List<string> fieldValues = new List<string>();
                var filedValueNodes = camlWhere.SelectNodes("//Value");
                int count = 0;
                foreach (XmlNode node in filedValueNodes)
                {
                    if (node.InnerText.StartsWith("{", StringComparison.OrdinalIgnoreCase) && node.InnerText.EndsWith("}", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldValues.Add(node.InnerText);
                        node.InnerText = "{" + count.ToString() + "}";
                        count++;
                    }
                }
                string tmpWhereStr = camlWhere.InnerXml;
                #region replace operator from caml to odata
                tmpWhereStr = tmpWhereStr.Replace("<Eq>", "<eq>");
                tmpWhereStr = tmpWhereStr.Replace("</Eq>", "</eq>");

                tmpWhereStr = tmpWhereStr.Replace("<Neq>", "<ne>");
                tmpWhereStr = tmpWhereStr.Replace("</Neq>", "</ne>");

                tmpWhereStr = tmpWhereStr.Replace("<Gt>", "<gt>");
                tmpWhereStr = tmpWhereStr.Replace("</Gt>", "</gt>");

                tmpWhereStr = tmpWhereStr.Replace("<Lt>", "<lt>");
                tmpWhereStr = tmpWhereStr.Replace("</Lt>", "</lt>");

                tmpWhereStr = tmpWhereStr.Replace("<Geq>", "<ge>");
                tmpWhereStr = tmpWhereStr.Replace("</Geq>", "</ge>");

                tmpWhereStr = tmpWhereStr.Replace("<Leq>", "<le>");
                tmpWhereStr = tmpWhereStr.Replace("</Leq>", "</le>");

                tmpWhereStr = tmpWhereStr.Replace("<IsNull>", "<eqnull>");
                tmpWhereStr = tmpWhereStr.Replace("</IsNull>", "</eqnull>");

                tmpWhereStr = tmpWhereStr.Replace("<IsNotNull>", "<nenull>");
                tmpWhereStr = tmpWhereStr.Replace("</IsNotNull>", "</nenull>");

                tmpWhereStr = tmpWhereStr.Replace("<BeginsWith>", "<startswith>");
                tmpWhereStr = tmpWhereStr.Replace("</BeginsWith>", "</startswith>");

                tmpWhereStr = tmpWhereStr.Replace("<Contains>", "<substringof>");
                tmpWhereStr = tmpWhereStr.Replace("</Contains>", "</substringof>");

                tmpWhereStr = tmpWhereStr.Replace("<And>", "<and>");
                tmpWhereStr = tmpWhereStr.Replace("</And>", "</and>");

                tmpWhereStr = tmpWhereStr.Replace("<Or>", "<or>");
                tmpWhereStr = tmpWhereStr.Replace("</Or>", "</or>");
                #endregion
                string valueStr = string.Format("<View><Query><where xmlns=\"http://www.w3.org/1999/xhtml\">{0}</where></Query></View>", tmpWhereStr);
                result.Value = new Value(valueStr);
                CheckUnsupportedValueType(fieldValues);
                if (fieldValues.Count > 0)
                {
                    List<KeyValuePair<string, bool>> tempList = new List<KeyValuePair<string, bool>>();
                    foreach (string s in fieldValues)
                    {
                        tempList.Add(new KeyValuePair<string, bool>(s, false));
                    }
                    result.FormatValues = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(tempList, workflowActionProcessor, true);
                }
            }
            else
            {
                result.Value = new Value("<View><Query/></View>");
            }

            return result;
        }

        private void CheckUnsupportedValueType(List<string> valueType)
        {
            foreach (var type in valueType)
            {
                if (type.StartsWith("{WFConstant:", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnSupportedActionTypeException("Unsupported value type WorkflowConstant");
                }
            }
        }

        private ParametersValue GenerateODataSelectParametersValue(ValueStorageCollection storageItems, string fieldName)
        {
            var value = new ParametersValue();
            if (storageItems.Count > 0)
            {
                string valueString = string.Empty;
                foreach (var storageItem in storageItems)
                {
                    var tempFieldName = storageItem.ValueIdentifier;
                    valueString = valueString.Equals(string.Empty) ? tempFieldName : valueString + "," + tempFieldName;
                }
                value.PrimitiveValue = new PrimitiveValue("String", valueString);
            }
            else
            {
                value.PrimitiveValue = new PrimitiveValue("String", fieldName);
            }
            return value;
        }

        private DictionaryValue[] ConvertToDictionaryValueArray(ValueStorageCollection valueStorages)
        {
            var dictionaryValues = new List<DictionaryValue>();
            foreach (var valueStorage in valueStorages)
            {
                dictionaryValues.Add(new DictionaryValue
                {
                    Key = valueStorage.ValueIdentifier,
                    Value = new Value
                    {
                        Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(valueStorage.VariableName)
                    }
                });
            }

            return dictionaryValues.ToArray();
        }
        private ParametersValue GenerateParameterVariableMappPrimitiveValue(ValueStorageCollection storageItems, ActivityParameter output, string fieldName)
        {
            var value = new ParametersValue();
            var dictionaryValues = new List<DictionaryValue>();
            if (storageItems.Count > 0)
            {
                dictionaryValues.AddRange(ConvertToDictionaryValueArray(storageItems).ToList());
            }
            else
            {
                dictionaryValues.Add(new DictionaryValue
                {
                    Key = fieldName,
                    Value = new Value(workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(output.Variable.Name))
                });
            }
            value.Dictionary = dictionaryValues.ToArray();

            return value;
        }
    }
}
