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
using System.Collections.Generic;
using System.Linq;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.CustomUtility;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Examples
{
    #region Custom Filter Example
    public class CustomFilterExample : IAveCustomFilter
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Example code, never run in produce.")]
        public AveCustomFilterPolicy GetCustomFilters(FilterLevel level)
        {
            List<AveBaseCondition> conditions = new List<AveBaseCondition>();
            string expression = string.Empty;

            if (level == FilterLevel.Document)
            {
                //Content Type filter
                AveContentTypeCondition ct01 = new AveContentTypeCondition()
                {
                    SequenceNo = 1,
                    CmpAction = CompareAction.Equals,
                    ContentTypeName = "Document"
                };

                //Field filter based on field type and display name; String type column
                AveFieldCondition fd01 = new AveFieldCondition()
                {
                    SequenceNo = 2,
                    CmpAction = CompareAction.Equals,
                    FieldDisplayName = "Title",
                    ColumnType = AveFieldType.Text,
                    FdValue = new FilterValue("Hello")
                };

                //Field filter based on field type and internal name; DateTime type column
                AveFieldCondition fd02 = new AveFieldCondition()
                {
                    SequenceNo = 3,
                    CmpAction = CompareAction.FromTo,
                    FieldInternalName = "Created_0X20",
                    ColumnType = AveFieldType.DateTime,
                    FdValue = new FilterValue(DateTime.Now.AddDays(-60).ToString("yyyy-mm-dd HH:MM:SS"), DateTime.Now.ToString("yyyy-mm-dd HH:MM:SS"))
                };

                //Field filter based on display name; Number type column
                AveFieldCondition fd03 = new AveFieldCondition()
                {
                    SequenceNo = 4,
                    CmpAction = CompareAction.LessOrEqualThan,
                    FieldDisplayName = "DelayDays",
                    FdValue = new FilterValue("50", ValueUnit.Days),
                };

                //Field filter based on content type and display name; DateTime type column
                AveFieldCondition fd04 = new AveFieldCondition()
                {
                    SequenceNo = 5,
                    FieldDisplayName = "Modified",
                    CmpAction = CompareAction.Before,
                    FdValue = new FilterValue(DateTime.Now.AddDays(-30).ToString("yyyy-mm-dd HH:MM:SS")),
                };

                conditions.Add(ct01);
                conditions.Add(fd01);
                conditions.Add(fd02);
                conditions.Add(fd03);

                //Use the SerialNo to generate expression to identify how to joint the conditions
                expression = "((1 and 2) or (3 and 4))";
            }
            else
            {
                //Content Type filter
                AveContentTypeCondition ct01 = new AveContentTypeCondition()
                {
                    SequenceNo = 1,
                    CmpAction = CompareAction.Equals,
                    ContentTypeName = "Test Custom Item"
                };

                //Field filter based on field type and display name; String type column
                AveFieldCondition fd01 = new AveFieldCondition()
                {
                    SequenceNo = 2,
                    CmpAction = CompareAction.Equals,
                    FieldDisplayName = "Title",
                    ColumnType = AveFieldType.Text,
                    FdValue = new FilterValue("Hello")
                };

                //Field filter based on field type and internal name; DateTime type column
                AveFieldCondition fd02 = new AveFieldCondition()
                {
                    SequenceNo = 3,
                    CmpAction = CompareAction.FromTo,
                    FieldInternalName = "Created_0X20",
                    ColumnType = AveFieldType.DateTime,
                    FdValue = new FilterValue(DateTime.Now.AddDays(-60).ToString("yyyy-mm-dd HH:MM:SS"), DateTime.Now.ToString("yyyy-mm-dd HH:MM:SS"))
                };

                conditions.Add(ct01);
                conditions.Add(fd01);
                conditions.Add(fd02);

                //Use the SerialNo to generate expression to identify how to joint the conditions
                expression = "(1 and (2 or 3))";
            }

            return new AveCustomFilterPolicy() { Conditions = conditions, ExpressionString = expression, Level = level };
        }
    }
    #endregion

    #region Custom Field Mapping

    public class CustomFieldMappingExample : IAveCustomFieldMapping
    {
        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            AveCustomFieldInfo customField = null;

            //Map choice column "Export Control" to "Export Control Policy" based on display name
            if (sourceFieldInfo.SourceDisplayName.Equals("Export Control", StringComparison.OrdinalIgnoreCase))
            {
                customField = new AveCustomFieldInfo()
                {
                    Name = "Export Control Policy",
                    InternalName = "Export_0x20_Policy",
                    Type = AveFieldType.Choice,
                    //AveCustomFieldInfo中添加CustomFieldType并废弃CustomFieldTypeAsString。
                    //CustomFieldTypeAsString = AveFieldType.Choice.ToString(),
                    UseInternalOrDisplay = false
                };
            }
            //Skip a specified column during the migration job.
            else if (sourceFieldInfo.SourceDisplayName.Equals("CustomM", StringComparison.OrdinalIgnoreCase))
            {
                customField = new AveCustomFieldInfo()
                {
                    Name = "CustomM",
                    //AveCustomFieldInfo中添加CustomFieldType并废弃CustomFieldTypeAsString。
                    //CustomFieldTypeAsString = sourceFieldInfo.SourceType.ToString(),
                    UseInternalOrDisplay = false,
                    NeedSkipRestore = true
                };
            }

            return customField;
        }

        public List<AveCustomFieldInfo> GetNewFieldsBeforeAdd()
        {
            List<AveCustomFieldInfo> customFields = null;

            customFields.Add(new AveCustomFieldInfo()
                {
                    Name = "NewCol1",
                    Type = AveFieldType.Text,
                });
            customFields.Add(new AveCustomFieldInfo()
                {
                    Name = "NewCol2",
                    Type = AveFieldType.DateTime
                });

            return customFields;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Example code, never run in produce.")]
        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            string newValue = null;

            #region Implement normal value mapping
            //Map the value of "Export Control" column. "NOFORN" -> "No Foreign Eyes", "SECRET" -> "Secret", "ORCON" -> "Originator Controlled"
            if (sourceFieldValueInfo.SourceFieldInfo != null && sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName.Equals("Export Control", StringComparison.OrdinalIgnoreCase) && sourceFieldValueInfo.SourceValue != null)
            {
                switch (sourceFieldValueInfo.SourceValue.ToLower(CultureInfo.InvariantCulture))
                {
                    //case "noforn":
                    //    newValue = "No Foreign Eyes";
                    //    break;
                    //case "secret":
                    //    newValue = "Secret";
                    //    break;
                    //case "orcon;#B;#C":
                    //    newValue = "a;#ddd;#fdsfs";
                    //    break;
                    default:
                        newValue = sourceFieldValueInfo.SourceValue;
                        break;
                }
            }
            #endregion
            #region Implement value mapping based on item/doc name
            else if (sourceFieldValueInfo.SourceFieldInfo != null && sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName.Equals("TestCol", StringComparison.OrdinalIgnoreCase))
            {
                if (sourceFieldValueInfo.SourceItemName.Contains("Gemini"))
                {
                    newValue = "No Foreign Eyes";
                }
                else
                {
                    newValue = "Originator Controlled";
                }
            }
            #endregion
            #region Implement Collapse folder function
            else if (sourceFieldValueInfo.SourceFieldInfo != null && sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName.Equals("Country", StringComparison.OrdinalIgnoreCase))
            {
                object path;
                if (AveFetchObject.TryGetMetadata("FolderPath", out path))
                {
                    string[] ps = path.ToString().Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    newValue = ps.Length > 0 ? ps[0] : null;
                }
            }
            else if (sourceFieldValueInfo.SourceFieldInfo != null && sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName.Equals("City", StringComparison.OrdinalIgnoreCase))
            {
                object path;
                if (AveFetchObject.TryGetMetadata("FolderPath", out path))
                {
                    string[] ps = path.ToString().Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    newValue = ps.Length > 1 ? ps[1] : null;
                }
            }
            else if (sourceFieldValueInfo.SourceFieldInfo != null && sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName.Equals("District", StringComparison.OrdinalIgnoreCase))
            {
                object path;
                if (AveFetchObject.TryGetMetadata("FolderPath", out path))
                {
                    string[] ps = path.ToString().Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    newValue = ps.Length > 2 ? ps[2] : null;
                }
            }
            #endregion

            return newValue;
        }

        public List<string> GetMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> mappingValues = new List<string>();
            List<string> prepareValues = new List<string>();
            if (!string.IsNullOrEmpty(sourceFieldValueInfo.SourceValue))
            {
                if (!string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
                {
                    //如果多值的value是一个string通过特殊字符分隔，需要将多值先进行分割，然后逐一mapping
                    prepareValues = sourceFieldValueInfo.SourceValue.Split(new string[] { sourceFieldValueInfo.SplitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    //如果没有分隔符，说明得到的value就是一个string，可用这个value直接进行mapping
                    prepareValues.Add(sourceFieldValueInfo.SourceValue);
                }
            }
            else if (sourceFieldValueInfo.SourceDataJunction != null)
            {
                //如果多值的value是DataJunction，那么直接获取可以mapping的DisplayValue进行mapping
                foreach (var pair in sourceFieldValueInfo.SourceDataJunction)
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        prepareValues.Add(pair.Value);
                    }
                }
            }
            foreach (var sourceValue in prepareValues)
            {
                //将处理后的value逐一进行mapping，并将mapping后的value添加到mappingValues中
            }
            return mappingValues;
        }

        public object GetMappingNullValue(string fieldInternalName)
        {
            throw new NotImplementedException();
        }

        public void GetValuesFromExcel(string excelPath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetValueFromGuiMapping(AveSourceFieldValueInfo source)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
