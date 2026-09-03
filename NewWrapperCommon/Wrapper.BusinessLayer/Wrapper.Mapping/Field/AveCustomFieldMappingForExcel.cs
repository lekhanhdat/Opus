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




namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using System.IO;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using System.Collections;
    using AvePoint.Wrapper.Resource.Mapping;

    public class AveCustomFieldMappingForExcel : IAveCustomFieldMapping
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, string> TypeMappings = new Dictionary<string, string>()
                                       {
                                           {"Single line of text", "Text"},
                                           {"Multiple lines of text", "Note"},
                                           {"Choice (menu to choose from)", "Choice"},
                                           {"Choice (menu to choose from)_AllowMultiple", "MultiChoice"},
                                           {"Number (1, 1.0, 100)", "Number"},
                                           {"Currency ($, ¥, €)", "Currency"},
                                           {"Date and Time", "DateTime"},
                                           {"Lookup (information already on this site)", "Lookup"},
                                           {"Lookup (information already on this site)_AllowMultiple", "LookupMulti"},
                                           {"Yes/No (check box)", "Boolean"},
                                           {"Person or Group", "User"},
                                           {"Person or Group_AllowMultiple", "UserMulti"},
                                           {"Hyperlink or Picture", "URL"},
                                           {"Calculated (calculation based on other columns)", "Calculated"},
                                           {"Managed Metadata", "TaxonomyFieldType"},
                                           {"Managed Metadata_AllowMultiple", "TaxonomyFieldTypeMulti"},
                                       };

        private Dictionary<AveSourceFieldInfo, AveCustomFieldInfo> internalExcelFieldMapping;
        private Dictionary<int, Dictionary<string, string>> itemsValues = new Dictionary<int, Dictionary<string, string>>();
        private readonly Dictionary<AveSourceFieldInfo, AveCustomFieldInfo> mappedFileds = new Dictionary<AveSourceFieldInfo, AveCustomFieldInfo>(new AveCustomFieldInfoDisplayNameEqualityComparer());

        public AveCustomFieldMappingForExcel(string excelFolderPath)
        {
            if (!Directory.Exists(excelFolderPath))
            {
                throw new DirectoryNotFoundException(string.Format("The directory is not found,the excel folder path is{0}", excelFolderPath));
            }
        }

        public void GetValuesFromExcel(string excelPath)
        {
            using (new AvePerformanceScope("Wrapper.Mapping.AveCustomFieldMappingForExcel.GetValuesFromExcel"))
            {
                if (string.IsNullOrEmpty(excelPath))
                {
                    return;
                }
                var excelReader = new FMExcelOpenXml();
                try
                {
                    excelReader.Open(excelPath);
                    Dictionary<string, string> tempDic;
                    while ((tempDic = excelReader.ReadLine()) != null)
                    {
                        //初始化internalExcelFieldMapping, Excel的第一行
                        if (internalExcelFieldMapping == null)
                        {
                            internalExcelFieldMapping = new Dictionary<AveSourceFieldInfo, AveCustomFieldInfo>(new AveCustomFieldInfoDisplayNameEqualityComparer());
                            foreach (string header in tempDic.Keys)
                            {
                                if (header.Equals("Path"))
                                {
                                    continue;
                                }
                                var field = GetFieldInfo(header);
                                if (field != null)
                                {
                                    var key = new AveSourceFieldInfo { SourceDisplayName = field.Name };
                                    this.internalExcelFieldMapping[key] = field;
                                }
                            }
                        }
                        //初始化itemsValues, 每一行对于一个Item
                        itemsValues.Add(Convert.ToInt32(tempDic["ID:=Counter"]),
                                    tempDic.ToDictionary(
                                            pair => pair.Key.Split(new[] { ":=" }, StringSplitOptions.RemoveEmptyEntries)[0],
                                            pair => pair.Value
                                        ));

                    }
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.ERROR, "When load Excel " + excelPath + " for custom field: " + e.ToString());
                }
                finally
                {
                    excelReader.Dispose();
                }
            }
        }

        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            if (sourceFieldInfo.IsHidenOrReadOnly)
            {
                return null;
            }
            if (!mappedFileds.ContainsKey(sourceFieldInfo))
            {
                AveCustomFieldInfo result = null;
                if (internalExcelFieldMapping != null)
                {
                    if (internalExcelFieldMapping.ContainsKey(sourceFieldInfo))
                    {
                        result = internalExcelFieldMapping[sourceFieldInfo];
                        if (sourceFieldInfo.SourceTypeAsString.Equals(result.TypeAsString, StringComparison.Ordinal))
                        {
                            if (!(result is AveCustomLookupFieldInfo || result is AveCustomMetadataFieldInfo))
                            {
                                result.Type = AveFieldType.Invalid;
                                result.TypeAsString = string.Empty;
                            }
                        }
                        internalExcelFieldMapping.Remove(sourceFieldInfo);
                    }
                    else
                    {
                        result = new AveCustomFieldInfo() { NeedSkipRestore = true };
                    }
                }
                mappedFileds.Add(sourceFieldInfo, result);
            }
            return mappedFileds[sourceFieldInfo];
        }

        public List<AveCustomFieldInfo> GetNewFieldsBeforeAdd()
        {
            return internalExcelFieldMapping.Values.ToList();
        }

        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (this.itemsValues.ContainsKey(sourceFieldValueInfo.SourceItemRowId))
            {
                var values = this.itemsValues[sourceFieldValueInfo.SourceItemRowId];
                if (values.ContainsKey(sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName))
                {
                    return values[sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName];
                }
            }
            return null;
        }

        public List<string> GetMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> mappingValues = new List<string>();
            string tempValue = GetMappingValue(sourceFieldValueInfo);
            string splitString = string.Empty;
            if (!string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
            {
                splitString = sourceFieldValueInfo.SplitString;
            }
            else
            {
                if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.MultiChoice)
                {
                    splitString = ";#";
                }
                else if (string.Equals(sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString, "UserMulti", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString, "TaxonomyMulti", StringComparison.OrdinalIgnoreCase))
                {
                    splitString = ";";
                }
            }
            if (!string.IsNullOrEmpty(splitString))
            {
                mappingValues = tempValue.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else if (string.Equals(sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString, "Note", StringComparison.OrdinalIgnoreCase)
                || string.Equals(sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString, "HTML", StringComparison.OrdinalIgnoreCase))
            {
                mappingValues = GetNoteMappingValueList(sourceFieldValueInfo, tempValue);
            }
            else
            {
                mappingValues.Add(tempValue);
            }

            sourceFieldValueInfo.SourceValue = tempValue;
            return mappingValues;
        }

        private List<string> GetNoteMappingValueList(AveSourceFieldValueInfo sourceFieldValueInfo, string value)
        {
            List<string> mappingValues = new List<string>();
            if (sourceFieldValueInfo.SourceFieldInfo.RichText || sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase))
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                string htmlString = "<HtmlRoot>" + value + "</HtmlRoot>";
                htmlDoc.LoadHtml(htmlString);
                HtmlNode rootNode = htmlDoc.DocumentNode.FirstChild;
                GetHtmlInnerText(rootNode, mappingValues);
            }
            else
            {
                string[] splitValues = value.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitValues.Length; i++)
                {
                    mappingValues.Add(splitValues[i].Trim('\r'));
                }
            }
            return mappingValues;

        }

        private void GetHtmlInnerText(HtmlNode node, List<string> innerTexts)
        {
            foreach (var n in node.ChildNodes)
            {
                if (n.ChildNodes.Count == 0 )
                {
                    if (n.InnerText != "\r\n" && n.InnerText != "\n") // 这种特殊字符不能当作value 还原，否则会显示在界面上
                    {
                        innerTexts.Add(RemoveSpecialChar(n.InnerText));
                    }
                }
                else
                {
                    GetHtmlInnerText(n, innerTexts);
                }
            }
        }

        private string RemoveSpecialChar(string value)
        {
            //部分value前后面有asc码值为8203的字符，导致value不能被mapping,note,html等类型的column存在该问题
            if (!string.IsNullOrEmpty(value))
            {
                return value.Trim((char)8203);
            }
            return value;
        }

        public object GetMappingNullValue(string fieldInternalName)
        {
            return null;
        }

        #region 根据rowId判断item是否在excel表中
        public bool CheckItemExistInExcel(int rowId)
        {
            if (this.itemsValues.ContainsKey(rowId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void Dispose()
        {
        }

        private AveCustomFieldInfo GetFieldInfo(string header)
        {
            string[] tmpArray = header.Split(new[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
            var field = new AveCustomFieldInfo { Name = tmpArray[0] };
            if (tmpArray.Length > 1)
            {
                InitFieldType(ref field, tmpArray[1]);
            }
            return field;
        }

        private void InitFieldType(ref AveCustomFieldInfo fieldInfo, string type)
        {
            if (TypeMappings.ContainsKey(type))
            {
                type = TypeMappings[type];
            }
            fieldInfo.TypeAsString = type;
            try
            {
                #region 在Export导出的Excel表中，添加允许多值的column(如person and group类型)未转移到目的端，这里特殊处理一下
                if (fieldInfo.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                {
                    fieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), "User");
                    fieldInfo.IsMulti = true;
                }
                else if (fieldInfo.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                {
                    fieldInfo = new AveCustomLookupFieldInfo()
                    {
                        Name = fieldInfo.Name,
                        SeparateChar = ";",
                    };
                }
                else if (CheckColumnConfigFormat(fieldInfo.TypeAsString, "Taxonomy"))
                {
                    Dictionary<string, string> option = GetMetaDataOption(fieldInfo.TypeAsString);
                    var newFieldInfo = new AveCustomMetadataFieldInfo()
                    {
                        Name = fieldInfo.Name,
                        Type = AveFieldType.Invalid,

                        TermGroup = option["TermGroup"],
                        TermSet = option["TermSet"],
                        Terms = option["Terms"],
                        IsMulti = Convert.ToBoolean(option["IsMulti"]),
                        SeparateChar = option["SeparateChar"],
                    };
                    newFieldInfo.TypeAsString = newFieldInfo.IsMulti ? "TaxonomyFieldTypeMulti" : "TaxonomyFieldType";
                    fieldInfo = newFieldInfo;
                }
                else if (fieldInfo.TypeAsString.StartsWith("Taxonomy", StringComparison.OrdinalIgnoreCase))
                {// Taxonomy Field 没有对应的Type，需要过滤掉，避免下面枚举解析时异常
                    if (fieldInfo.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldInfo.IsMulti = true;
                    }
                }
                else if (CheckColumnConfigFormat(fieldInfo.TypeAsString, "Lookup"))
                {
                    Dictionary<string, string> option = GetLookupOption(fieldInfo.TypeAsString);
                    var newFieldInfo = new AveCustomLookupFieldInfo()
                    {
                        Name = fieldInfo.Name,
                        Type = AveFieldType.Lookup,

                        ListTitle = option["ListTitle"],
                        FieldName = option["FieldName"],
                        IsMulti = Convert.ToBoolean(option["IsMulti"]),
                        SeparateChar = option["SeparateChar"],
                    };
                    newFieldInfo.TypeAsString = newFieldInfo.IsMulti ? "LookupMulti" : "Lookup";
                    fieldInfo = newFieldInfo;
                }
                #endregion
                else
                {
                    fieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), fieldInfo.TypeAsString);
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, WrapperMappingResource.AWMConvertValueToFieldTypeError, e.ToString(), fieldInfo.Name, fieldInfo.TypeAsString);
            }
        }

        private bool CheckColumnConfigFormat(string configStr, string type)
        {
            if (configStr.StartsWith(type, StringComparison.Ordinal) && (type.Length < configStr.Length))
            {
                string option = configStr.Substring(type.Length).Trim();
                if (option.StartsWith("(", StringComparison.Ordinal) && option.EndsWith(")", StringComparison.Ordinal))
                {
                    string[] setting = option.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (setting.Length >= 4)
                    {
                        return true;
                    }
                }
                mLog.Log(AveLogLevel.WARN, WrapperMappingResource.AWMCheckColumnConfigFormatError, configStr);
            }
            return false;
        }

        private Dictionary<string, string> GetMetaDataOption(string optionString)
        {
            Dictionary<string, string> option = new Dictionary<string, string>();
            int index = optionString.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            ArrayList settings = ProcessOptionString(optionString.Substring(index + 1));

            option.Add("TermGroup", settings[0].ToString());
            option.Add("TermSet", settings[1].ToString());
            option.Add("IsMulti", settings[settings.Count - 2].ToString());

            string separateChar = settings[settings.Count - 1].ToString();

            if (separateChar.Equals(")", StringComparison.OrdinalIgnoreCase))
            {
                option.Add("SeparateChar", ";");
            }
            else
            {
                option.Add("SeparateChar", separateChar.TrimEnd(')').Trim());
            }

            string terms = string.Empty;

            for (int i = 2; i < settings.Count - 2; i++)
            {
                terms = terms + settings[i].ToString() + ";";
            }
            if (!string.IsNullOrEmpty(terms))
            {
                terms = terms.TrimEnd(';');
            }
            option.Add("Terms", terms);
            return option;
        }

        private Dictionary<string, string> GetLookupOption(string optionString)
        {
            Dictionary<string, string> option = new Dictionary<string, string>();
            int index = optionString.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            ArrayList settings = ProcessOptionString(optionString.Substring(index + 1));

            option.Add("ListTitle", settings[0].ToString());
            option.Add("FieldName", settings[1].ToString());
            option.Add("IsMulti", settings[settings.Count - 2].ToString());

            string separateChar = settings[settings.Count - 1].ToString();

            if (separateChar.Equals(")", StringComparison.OrdinalIgnoreCase))
            {
                option.Add("SeparateChar", ";");
            }
            else
            {
                option.Add("SeparateChar", separateChar.TrimEnd(')').Trim());
            }
            return option;
        }

        private ArrayList ProcessOptionString(string oldStr)
        {
            string[] temp = oldStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            ArrayList settings = new ArrayList();
            foreach (string unit in temp)
            {
                if (!string.IsNullOrEmpty(unit.Trim()))
                {
                    settings.Add(unit.Trim());
                }
            }
            return settings;
        }

        public string GetValueFromGuiMapping(AveSourceFieldValueInfo source)
        {
            return null;
        }
    }
}
