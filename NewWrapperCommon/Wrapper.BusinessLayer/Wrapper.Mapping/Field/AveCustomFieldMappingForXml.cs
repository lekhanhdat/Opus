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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Mapping
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [Serializable]
    internal class AveCustomFieldMappingForXml : IAveCustomFieldMapping
    {
        Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> internalMapping;
        /// <summary>
        /// 已经在还原field的时候找到了对应AveSourceFieldInfo AveCustomFieldForXmlInfo对 保存起来 fieldvalue的mapping时使用
        /// </summary>
        Dictionary<AveSourceFieldInfo, AveCustomFieldForXmlInfo> realInternalMapping = new Dictionary<AveSourceFieldInfo, AveCustomFieldForXmlInfo>(new AveCustomFieldInfoEqualityComparer());

        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, object> nullToDefaultValueMapping = new Dictionary<string, object>();

        public AveCustomFieldMappingForXml(Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> mappings)
        {
            this.internalMapping = mappings;
        }

        /// <summary>
        /// 还原field时是使用，获取field的AveCustomFieldForXmlInfo，先用internal name再用display name，找到一个对应就break
        /// 由于contenttype condition，只能在这里check
        /// </summary>
        /// <param name="sourceFieldInfo"></param>
        /// <returns></returns>
        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            AveCustomFieldInfo tempFieldInfo = null;
            if (internalMapping != null)
            {
                AveSourceFieldInfo internalNameSourceFieldInfo = new AveSourceFieldInfo() { SourceInternalName = sourceFieldInfo.SourceInternalName, SourceDisplayName = string.Empty };
                AveSourceFieldInfo displayNameSourceFieldInfo = new AveSourceFieldInfo() { SourceInternalName = string.Empty, SourceDisplayName = sourceFieldInfo.SourceDisplayName };
                if (internalMapping.ContainsKey(internalNameSourceFieldInfo))
                {
                    foreach (AveCustomFieldForXmlInfo info in internalMapping[internalNameSourceFieldInfo])
                    {
                        if (info.MappingCondition.CheckCondition(null, sourceFieldInfo.SourceFieldId))
                        {
                            tempFieldInfo = info.GetCustomFieldInfo(true);
                            if (!realInternalMapping.ContainsKey(sourceFieldInfo))
                            {
                                realInternalMapping.Add(sourceFieldInfo, info);
                            }
                            break;
                        }
                    }
                }
                else if (internalMapping.ContainsKey(displayNameSourceFieldInfo))
                {
                    foreach (AveCustomFieldForXmlInfo info in internalMapping[displayNameSourceFieldInfo])
                    {
                        if (info.MappingCondition.CheckCondition(null, sourceFieldInfo.SourceFieldId))
                        {
                            tempFieldInfo = info.GetCustomFieldInfo(false);
                            if (!realInternalMapping.ContainsKey(sourceFieldInfo))
                            {
                                realInternalMapping.Add(sourceFieldInfo, info);
                            }
                            break;
                        }
                    }
                }
                InitNullToDefaultValueMapping();
            }
            return tempFieldInfo;
        }

        public List<AveCustomFieldInfo> GetNewFieldsBeforeAdd()
        {
            return null;
        }

        /// <summary>
        /// 用于value mapping，如果是源端type是managed metadata column 要支持name的get还有name|id的get
        /// 对于value mapping，不区分大小写
        /// </summary>
        /// <param name="sourceFieldValueInfo"></param>
        /// <returns></returns>
        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (sourceFieldValueInfo.SourceValue == null)
            {
                return null;
            }
            //string sourceValue = prepareValue;
            sourceFieldValueInfo.SourceValue = PrepareMappingValue(sourceFieldValueInfo);
            string mappedValue = InternalGetMappingValue(sourceFieldValueInfo, sourceFieldValueInfo.SourceValue);
            //if (mappedValue == null && sourceFieldValueInfo.SourceFieldInfo.SourceType.Equals(AveFieldType.Invalid) && sourceValue.Contains("|"))
            //{
            //    if (sourceValue.Contains(";"))
            //    {
            //        string[] values = sourceValue.Split(new char[] { ';' });
            //        for (int i = 0; i < values.Length; i++)
            //        {
            //            sourceFieldValueInfo.SourceValue = values[i].Substring(0, values[i].IndexOf("|", StringComparison.Ordinal));
            //            string newvalue = InternalGetMappingValue(sourceFieldValueInfo);
            //            newvalue = newvalue == null ? sourceFieldValueInfo.SourceValue : newvalue;
            //            mappedValue = mappedValue + newvalue + ";";
            //        }
            //    }
            //    else
            //    {
            //        sourceFieldValueInfo.SourceValue = sourceValue.Substring(0, sourceValue.IndexOf("|", StringComparison.Ordinal));
            //        string newvalue = InternalGetMappingValue(sourceFieldValueInfo);
            //        mappedValue = newvalue == null ? sourceFieldValueInfo.SourceValue : newvalue;
            //        //mappedValue = InternalGetMappingValue(sourceFieldValueInfo);
            //    }

            //}
            //sourceFieldValueInfo.SourceValue = mappedValue;
            if (string.IsNullOrEmpty(mappedValue))
            {
                mappedValue = sourceFieldValueInfo.SourceValue;
            }
            return mappedValue;
        }

        public List<string> GetMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> mappingValues = new List<string>();
            List<string> prepareValues = PrepareMultiMappingValue(sourceFieldValueInfo);
            foreach (var sourceValue in prepareValues)
            {
                string mappingValue = InternalGetMappingValue(sourceFieldValueInfo, sourceValue);
                if (!string.IsNullOrEmpty(mappingValue))
                {
                    mappingValues.Add(mappingValue);
                }
                else
                {
                    mappingValues.Add(sourceValue);
                }
            }
            return mappingValues;
        }

        private string PrepareMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            string prepareValue = string.Empty;
            string sourceValue = sourceFieldValueInfo.SourceValue;
            switch (sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString)
            {
                case "URL":
                    prepareValue = PrepareURLMappingValue(sourceFieldValueInfo);
                    break;
                case "Lookup":
                    prepareValue = PrepareLookupMappingValue(sourceFieldValueInfo);
                    break;
                //case "TaxonomyFieldType":
                //    prepareValue = sourceValue.Contains("|") ? sourceValue.Substring(0, sourceValue.IndexOf("|", StringComparison.Ordinal)) : sourceValue;
                //    break;
                default:
                    prepareValue = sourceValue;
                    break;
            }
            return prepareValue;
        }

        private List<string> PrepareMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> prepareValues = new List<string>();
            switch (sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString)
            {
                case "UserMulti":
                    prepareValues = PrepareMultiUserMappingValue(sourceFieldValueInfo);
                    break;
                case "LookupMulti":
                    prepareValues = PrepareMultiLookupMappingValue(sourceFieldValueInfo);
                    break;
                case "MultiChoice":
                    prepareValues = sourceFieldValueInfo.SourceValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "HTML":
                case "Note":
                    prepareValues = PrepareNoteMappingValue(sourceFieldValueInfo);
                    break;
                case "TaxonomyFieldTypeMulti":
                    prepareValues = PrepareMultiTaxonomyMappingValue(sourceFieldValueInfo);
                    break;
                //Default 说明源端column 是单值，需要使用单值mapping 处理。
                default:
                    if (!string.IsNullOrEmpty(sourceFieldValueInfo.SourceValue))
                    {
                        var singleValue = PrepareMappingValue(sourceFieldValueInfo);
                        if (!string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
                        {
                            prepareValues = singleValue.Split(new string[] { sourceFieldValueInfo.SplitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                        else
                        {
                            prepareValues.Add(singleValue);
                        }
                    }
                    else if (sourceFieldValueInfo.SourceDataJunction != null)
                    {
                        foreach (var pair in sourceFieldValueInfo.SourceDataJunction)
                        {
                            if (!string.IsNullOrEmpty(pair.Value))
                            {
                                prepareValues.Add(pair.Value);
                            }
                        }
                    }
                    break;
            }
            return prepareValues;
        }

        #region Prepare User Column Value
        private List<string> PrepareMultiUserMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> prepareValues = new List<string>();
            if (!string.IsNullOrEmpty(sourceFieldValueInfo.SourceValue))
            {
                if (!string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
                {
                    prepareValues = sourceFieldValueInfo.SourceValue.Split(new string[] { sourceFieldValueInfo.SplitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    prepareValues.Add(sourceFieldValueInfo.SourceValue);
                }
            }
            else
            {
                foreach (var pair in sourceFieldValueInfo.SourceDataJunction)
                {
                    prepareValues.Add(pair.Value);
                }
            }
            return prepareValues;
        }
        #endregion

        #region Prepare Note Column Value
        private List<string> PrepareNoteMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> prepareValues = new List<string>();
            string tempValue = sourceFieldValueInfo.SourceValue;
            if (sourceFieldValueInfo.SourceFieldInfo.RichText || sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase))
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                string htmlString = "<HtmlRoot>" + tempValue + "</HtmlRoot>";
                htmlDoc.LoadHtml(htmlString);
                HtmlNode rootNode = htmlDoc.DocumentNode.FirstChild;
                GetHtmlInnerText(rootNode, prepareValues);
            }
            else
            {
                if (!string.IsNullOrEmpty(tempValue))
                {
                    string[] splitValues = tempValue.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < splitValues.Length; i++)
                    {
                        prepareValues.Add(splitValues[i].Trim('\r'));
                    }
                }
            }
            return prepareValues;

        }

        private void GetHtmlInnerText(HtmlNode node, List<string> innerTexts)
        {
            foreach (var n in node.ChildNodes)
            {
                if (n.ChildNodes.Count == 0 && n.InnerText != "\r\n" && n.InnerText != "\n")
                {
                    innerTexts.Add(RemoveSpecialChar(n.InnerText));
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
        #endregion

        #region Prepare Lookup Column Value
        private string PrepareLookupMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            string prepareValue = sourceFieldValueInfo.SourceValue;
            //单值lookup column value的格式为RowId;DisplayValue#TPGuid&LeafName,如果value里面包含了&说明备份了itemLeafName，此时走column mapping逻辑，LeafName失效去掉即可
            //var index = prepareValue.IndexOf('&');
            var index = prepareValue.IndexOf("&leafName&");
            if (index > -1)
            {
                prepareValue = prepareValue.Substring(0, index);
            }
            //如果value里面包含了#说明备份了TPGuid，此时走column mapping逻辑，Guid失效去掉即可
            //index = prepareValue.IndexOf('#');
            index = prepareValue.IndexOf("#GUID#");
            if (index > -1)
            {
                prepareValue = prepareValue.Substring(0, index);
            }
            //如果value里面不包含；说明没有备份DisplayValue，由于此处是走mapping逻辑，所以默认value包含DisplayValue，格式为RowId;DisplayValue，如果没有分号，将其加入value中，方便后面处理
            index = prepareValue.IndexOf(';');
            if (index > -1)
            {
                prepareValue = prepareValue.Substring(index + 1, prepareValue.Length - index - 1);
            }
            return prepareValue;
        }

        private List<string> PrepareMultiLookupMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> prepareValues = new List<string>();
            if (!string.IsNullOrEmpty(sourceFieldValueInfo.SourceValue))
            {
                if (!string.IsNullOrEmpty(sourceFieldValueInfo.SplitString))
                {
                    prepareValues = sourceFieldValueInfo.SourceValue.Split(new string[] { sourceFieldValueInfo.SplitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    prepareValues.Add(sourceFieldValueInfo.SourceValue);
                }
            }
            else if (sourceFieldValueInfo.SourceDataJunction != null)
            {
                foreach (var pair in sourceFieldValueInfo.SourceDataJunction)
                {
                    string tempValue = pair.Value;
                    //if (tempValue.Contains("&"))
                    if (tempValue.Contains("&leafName&"))
                    {
                        //tempValue = tempValue.Substring(0, tempValue.IndexOf("&", StringComparison.OrdinalIgnoreCase));
                        tempValue = tempValue.Substring(0, tempValue.IndexOf("&leafName&", StringComparison.OrdinalIgnoreCase));
                    }
                    //if (tempValue.Contains("#"))
                    if (tempValue.Contains("#GUID#"))
                    {
                        //tempValue = tempValue.Substring(0, tempValue.IndexOf("#", StringComparison.OrdinalIgnoreCase));
                        tempValue = tempValue.Substring(0, tempValue.IndexOf("#GUID#", StringComparison.OrdinalIgnoreCase));
                    }
                    prepareValues.Add(tempValue);
                }
            }
            return prepareValues;
        }
        #endregion

        #region Prepare URL Column Value
        private string PrepareURLMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            var sourceFieldInfo = sourceFieldValueInfo.SourceFieldInfo;
            string prepareValue = sourceFieldValueInfo.SourceValue;
            //如果Site collection是hostheader，不能用web app url拼absolute url,直接用Site url拼。
            string sourceWebAppUrl = (string.IsNullOrEmpty(sourceFieldInfo.SourceSiteUrl) || (!string.IsNullOrEmpty(sourceFieldInfo.SourceWebAppUrl) && sourceFieldInfo.SourceSiteUrl.StartsWith(sourceFieldInfo.SourceWebAppUrl, StringComparison.OrdinalIgnoreCase))) ? sourceFieldInfo.SourceWebAppUrl
                : sourceFieldInfo.SourceSiteUrl;

            if (!prepareValue.StartsWith("Http", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(sourceWebAppUrl) && !prepareValue.StartsWith(sourceWebAppUrl, StringComparison.OrdinalIgnoreCase))
            {
                prepareValue = sourceWebAppUrl.TrimEnd('/') + '/' + prepareValue.TrimStart('/');
            }
            return prepareValue;
        }
        #endregion

        #region Prepare Taxonomy Column Value
        private List<string> PrepareMultiTaxonomyMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            List<string> prepareValues = new List<string>();
            string splitString = string.IsNullOrEmpty(sourceFieldValueInfo.SplitString) ? ";" : sourceFieldValueInfo.SplitString;
            string[] splitValues = sourceFieldValueInfo.SourceValue.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitValues.Length; i++)
            {
                //if (splitValues[i].Contains('|'))
                //{
                //    prepareValues.Add(splitValues[i].Substring(0, splitValues[i].IndexOf('|')));
                //}
                //else
                //{
                //    prepareValues.Add(splitValues[i]);
                //}
                prepareValues.Add(splitValues[i]);
            }
            return prepareValues;
        }
        #endregion

        public object GetMappingNullValue(string fieldInternalName)
        {
            return nullToDefaultValueMapping.GetValueWithLock(fieldInternalName);
        }

        private string InternalGetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo, string sourceFieldValue)
        {
            if (realInternalMapping.ContainsKey(sourceFieldValueInfo.SourceFieldInfo) && realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].MappingCondition.CheckItemCondition(sourceFieldValueInfo.SourceItemName) && sourceFieldValue != null)
            {
                if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.DateTime)
                {
                    foreach (KeyValuePair<string, string> pair in realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping)
                    {
                        DateTime mappingTime;
                        DateTime sourceTime;
                        try
                        {
                            mappingTime = DateTime.SpecifyKind(DateTime.Parse(pair.Key, CultureInfo.InvariantCulture), DateTimeKind.Utc);
                            sourceTime = DateTime.SpecifyKind(DateTime.Parse(sourceFieldValue, CultureInfo.InvariantCulture), DateTimeKind.Utc);
                            if (mappingTime.Equals(sourceTime))
                            {
                                return realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping[pair.Key];
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug("The source value cannot be converted to date time, source time: {0}, mapping time: {1}, error: {2}", sourceFieldValue, pair.Key, ex);
                        }
                    }
                }
                #region Deal with metadata column value.
                if (sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString.StartsWith("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) && sourceFieldValue.Contains('|'))
                {
                    var displayValue = sourceFieldValue.Substring(0, sourceFieldValue.IndexOf('|'));
                    if (realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping.ContainsKey(sourceFieldValue))
                    {
                        return realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping[sourceFieldValue];
                    }
                    else if (realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping.ContainsKey(displayValue))
                    {
                        return realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping[displayValue];
                    }
                }
                #endregion
                else if (realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping.ContainsKey(sourceFieldValue))
                {
                    return realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping[sourceFieldValue];
                }
            }
            return null;
        }

        public void InitNullToDefaultValueMapping()
        {
            foreach (var mapping in this.realInternalMapping)
            {
                foreach (var item in mapping.Value.ValueMapping)
                {
                    if (string.IsNullOrEmpty(item.Key) && !this.nullToDefaultValueMapping.ContainsKey(mapping.Key.SourceInternalName))
                    {
                        this.nullToDefaultValueMapping.Add(mapping.Key.SourceInternalName, item.Value);
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        public void GetValuesFromExcel(string excelPath)
        {
            throw new NotImplementedException();
        }

        public string GetValueFromGuiMapping(AveSourceFieldValueInfo source)
        {
            foreach (var kv in internalMapping)
            {
                if (string.Equals(kv.Key.SourceDisplayName, source.SourceFieldInfo.SourceDisplayName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kv.Key.SourceInternalName, source.SourceFieldInfo.SourceInternalName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (AveCustomFieldForXmlInfo info in kv.Value)
                    {
                        if (info.ValueMapping.ContainsKey(source.SourceValue))
                        {
                            return info.ValueMapping[source.SourceValue];
                        }
                    }
                }
            }
            return source.SourceValue;
        }
    }

    [Serializable]
    public class AveMappingCondition
    {
        private List<AveMappingConditionInfo> siteCondition = new List<AveMappingConditionInfo>();
        private List<AveMappingConditionInfo> listCondition = new List<AveMappingConditionInfo>();
        private List<AveMappingConditionInfo> itemCondition = new List<AveMappingConditionInfo>();
        public object AveMappingSourceSPListOrWebInfo;

        public void Load(XmlElement node, bool hasItemCondition)
        {
            XmlNode siteConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "SiteCondition", StringComparison.OrdinalIgnoreCase)).First();
            LoadConditions(siteCondition, siteConditionNode);
            XmlNode listConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ListCondition", StringComparison.OrdinalIgnoreCase)).First();
            LoadConditions(listCondition, listConditionNode);
            if (hasItemCondition)
            {
                XmlNode itemConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ItemCondition", StringComparison.OrdinalIgnoreCase)).First();
                LoadConditions(itemCondition, itemConditionNode);
            }
        }

        private void LoadConditions(List<AveMappingConditionInfo> conditions, XmlNode conditionsNode)
        {
            foreach (XmlNode n in conditionsNode.ChildNodes)
            {
                AveMappingConditionInfo conditionInfo = new AveMappingConditionInfo();
                conditionInfo.Load(n);
                conditions.Add(conditionInfo);
            }
        }

        public List<AveMappingConditionInfo> GetItemCondition()
        {
            return itemCondition;
        }

        /// <summary>
        /// check item name condition
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public bool CheckItemCondition(string itemName)
        {
            ///暂时加上 调试需要
            if (itemName == string.Empty)
            {
                return true;
            }
            bool result = true;
            bool perInfoResult = true;
            AveConditionRelation preRelation = AveConditionRelation.And;
            ///为了最后一个条件不做or加true 或者and加false的处理
            int condionCount = 0;
            foreach (AveMappingConditionInfo info in itemCondition)
            {
                condionCount++;
                perInfoResult = info.CheckItemNameCondition(itemName);
                if (preRelation == AveConditionRelation.And)
                {
                    result = result & perInfoResult;
                }
                else if (preRelation == AveConditionRelation.Or)
                {
                    result = result | perInfoResult;
                }
                preRelation = info.GetRelation();
                if (condionCount == itemCondition.Count)
                {
                    break;
                }
                else if (preRelation == AveConditionRelation.And && !result || preRelation == AveConditionRelation.Or && result)
                {
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 可以现在list或者web级别checkcondition，但是如果有contenttype condition必须还原field时check，这个可以考虑拿掉
        /// </summary>
        /// <param name="listOrWeb"></param>
        /// <returns></returns>
        public virtual bool CheckCondition(object listOrWeb, Guid fieldId)
        {
            bool result = false;
            if (listOrWeb == null)
            {
                listOrWeb = AveMappingSourceSPListOrWebInfo;
            }
            if (listOrWeb != null)
            {
                if (listOrWeb is AveMappingSourceSPListInfo)
                {
                    result = CheckConditionResult(listOrWeb, siteCondition, fieldId) && CheckConditionResult(listOrWeb, listCondition, fieldId);
                }
                else if (listOrWeb is AveMappingSourceSPWebInfo)
                {
                    result = listCondition.Count == 0 && itemCondition.Count == 0 && CheckConditionResult(listOrWeb, siteCondition, fieldId);
                }
            }
            return result;
        }

        public bool CheckCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckConditionResult(condition, siteCondition) && CheckConditionResult(condition, listCondition);
        }
        protected bool CheckConditionResult(object listOrWeb, List<AveMappingConditionInfo> conditions, Guid fieldId)
        {
            bool result = true;
            if (conditions.Count > 0)
            {
                AveConditionRelation relation = AveConditionRelation.And;
                foreach (AveMappingConditionInfo info in conditions)
                {
                    if (relation == AveConditionRelation.And)
                    {
                        result = result && info.CheckCondition(listOrWeb, fieldId);
                    }
                    else
                    {
                        result = result || info.CheckCondition(listOrWeb, fieldId);
                    }
                    relation = info.GetRelation();
                }
            }
            return result;
        }

        private bool CheckConditionResult(AveFieldMappingConditionInfo sourceCondition, List<AveMappingConditionInfo> conditions)
        {
            bool result = false;
            if (conditions.Count > 0)
            {
                AveConditionRelation preRelation = AveConditionRelation.Or;
                bool preResult = true;
                foreach (AveMappingConditionInfo info in conditions)
                {
                    if (preRelation == AveConditionRelation.And && !preResult)
                    {
                        preRelation = info.GetRelation();
                        preResult = false;
                        continue;
                    }
                    if (info.CheckCondition(sourceCondition))
                    {
                        if (info.GetRelation() == AveConditionRelation.Or)
                        {
                            result = true;
                            break;
                        }
                        else
                        {
                            preRelation = AveConditionRelation.And;
                            preResult = true;
                        }
                    }
                    else
                    {
                        preRelation = info.GetRelation();
                        preResult = false;
                        continue;
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
    }

    [Serializable]
    public class AveMappingConditionInfo
    {
        public AveConditionType ConditionType { get; set; }

        public MappingFilterCondition Operation { get; set; }

        public string ConditionValue { get; set; }

        public AveConditionRelation Relation { get; set; }

        public void Load(XmlNode node)
        {
            ConditionType = (AveConditionType)Enum.Parse(typeof(AveConditionType), node.Attributes["type"].Value, true);
            Operation = (MappingFilterCondition)Enum.Parse(typeof(MappingFilterCondition), node.Attributes["condition"].Value, true);
            ConditionValue = node.Attributes["value"].Value;
            Relation = (AveConditionRelation)Enum.Parse(typeof(AveConditionRelation), node.Attributes["relation"].Value, true);
        }

        internal AveConditionRelation GetRelation()
        {
            return Relation;
        }

        public bool CheckCondition(object listOrWeb, Guid fieldId)
        {
            switch (ConditionType)
            {
                case AveConditionType.URL:
                    return CheckURLCondition(listOrWeb);
                case AveConditionType.SiteContentType:
                    return CheckSiteContentTypeCondition(listOrWeb, fieldId);
                case AveConditionType.WebTemplate:
                    return CheckWebTemplateCondition(listOrWeb, fieldId);
                case AveConditionType.TemplateID:
                    return CheckListTemplateIdCondition(listOrWeb as AveMappingSourceSPListInfo);
                case AveConditionType.ListTitle:
                    return CheckListTitleCondition(listOrWeb as AveMappingSourceSPListInfo);
                case AveConditionType.ListContentType:
                    return CheckListContentTypeCondition(listOrWeb as AveMappingSourceSPListInfo, fieldId);
            }

            return false;
        }

        public bool CheckCondition(AveFieldMappingConditionInfo condition)
        {
            switch (ConditionType)
            {
                case AveConditionType.URL:
                    return CheckURLCondition(condition);
                case AveConditionType.SiteContentType:
                    return CheckSiteContentTypeCondition(condition);
                case AveConditionType.WebTemplate:
                    return CheckWebTemplateCondition(condition);
                case AveConditionType.TemplateID:
                    return CheckListTemplateIdCondition(condition);
                case AveConditionType.ListTitle:
                    return CheckListTitleCondition(condition);
                case AveConditionType.ListContentType:
                    return CheckListContentTypeCondition(condition);
            }

            return false;
        }

        public bool CheckItemNameCondition(string itemName)
        {
            return CheckStringCondtion(ConditionValue, itemName, Operation);
        }

        private bool CheckURLCondition(object listOrWeb)
        {
            string url = string.Empty;
            if (listOrWeb is AveMappingSourceSPListInfo)
            {
                url = (listOrWeb as AveMappingSourceSPListInfo).ParentWeb.Url;
            }
            else if (listOrWeb is AveMappingSourceSPWebInfo)
            {
                url = (listOrWeb as AveMappingSourceSPWebInfo).Url;
            }
            return CheckStringCondtion(ConditionValue, url, Operation);
        }

        private bool CheckURLCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(ConditionValue, condition.SiteUrl, Operation);
        }

        private bool CheckSiteContentTypeCondition(object listOrWeb, Guid fieldId)
        {
            //IAveContentTypeCollection contentTypeCollection = null;
            //if (listOrWeb is AveMappingSourceSPListInfo )
            //{
            //    contentTypeCollection = (listOrWeb as AveMappingSourceSPListInfo ).ParentWeb.ContentTypes;
            //}
            //else if (listOrWeb is AveMappingSourceSPWebInfo)
            //{
            //    contentTypeCollection = (listOrWeb as AveMappingSourceSPWebInfo).ContentTypes;
            //}
            Dictionary<string, List<Guid>> contentTypesNames = new Dictionary<string, List<Guid>>();
            if (listOrWeb is AveMappingSourceSPListInfo)
            {
                contentTypesNames = (listOrWeb as AveMappingSourceSPListInfo).SourceListContentTypes;
            }
            else if (listOrWeb is AveMappingSourceSPWebInfo)
            {
                contentTypesNames = (listOrWeb as AveMappingSourceSPWebInfo).SourceWebContentTypes;
            }
            return CheckContentTypeCondtion(ConditionValue, contentTypesNames, Operation, fieldId);
        }

        private bool CheckSiteContentTypeCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckContentTypeCondition(ConditionValue, condition.SiteContentTypeCollection, Operation);
        }

        private bool CheckListTemplateIdCondition(AveMappingSourceSPListInfo list)
        {
            string listTemplateID = list.TemplateId;
            return CheckStringCondtion(ConditionValue, listTemplateID, Operation);
        }

        private bool CheckListTemplateIdCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(ConditionValue, condition.ListTemplateID, Operation);
        }

        private bool CheckWebTemplateCondition(object listOrWeb, Guid fieldId)
        {
            string webTemplate = string.Empty;
            if (listOrWeb is AveMappingSourceSPListInfo)
            {
                webTemplate = (listOrWeb as AveMappingSourceSPListInfo).ParentWeb.TemplateId;
            }
            else if (listOrWeb is AveMappingSourceSPWebInfo)
            {
                webTemplate = (listOrWeb as AveMappingSourceSPWebInfo).TemplateId;
            }

            return CheckStringCondtion(ConditionValue, webTemplate, Operation);
        }

        private bool CheckWebTemplateCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(ConditionValue, condition.WebTemplate, Operation);
        }

        private bool CheckListTitleCondition(AveMappingSourceSPListInfo list)
        {
            string listTitle = list.Title;
            return CheckStringCondtion(ConditionValue, listTitle, Operation);
        }

        private bool CheckListTitleCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(ConditionValue, condition.ListTitle, Operation);
        }

        private bool CheckListContentTypeCondition(AveMappingSourceSPListInfo list, Guid fieldId)
        {
            return CheckContentTypeCondtion(ConditionValue, list.SourceListContentTypes, Operation, fieldId);
        }

        private bool CheckListContentTypeCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckContentTypeCondition(ConditionValue, condition.ListContentTypeCollection, Operation);
        }

        private bool CheckStringCondtion(string value, string comparedValue, MappingFilterCondition operation)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(comparedValue))
            {
                return false;
            }
            switch (operation)
            {
                case MappingFilterCondition.Equal:
                    return string.Equals(value, comparedValue, StringComparison.OrdinalIgnoreCase);
                case MappingFilterCondition.NotEqual:
                    return !string.Equals(value, comparedValue, StringComparison.OrdinalIgnoreCase);
                case MappingFilterCondition.Contains:
                    return comparedValue.ToUpper(CultureInfo.InvariantCulture).Contains(value.ToUpper(CultureInfo.InvariantCulture));
                case MappingFilterCondition.DoesNotContain:
                    return !comparedValue.ToUpper(CultureInfo.InvariantCulture).Contains(value.ToUpper(CultureInfo.InvariantCulture));
            }
            return false;
        }

        private bool CheckContentTypeCondtion(string name, Dictionary<string, List<Guid>> contenttypeNames, MappingFilterCondition operation, Guid fieldId)
        {
            bool result = false;
            switch (operation)
            {
                case MappingFilterCondition.Equal:
                    result = contenttypeNames.ContainsKey(name);
                    if (result && !fieldId.Equals(Guid.Empty))
                    {
                        bool isContentTypeField = contenttypeNames[name].Contains(fieldId);
                        result = result & isContentTypeField;
                    }
                    break;
                case MappingFilterCondition.NotEqual:
                    if (!fieldId.Equals(Guid.Empty))
                    { //不包含时的逻辑判断，用的是包含，判断是否包含，之后再取反
                        result = contenttypeNames.ContainsKey(name);
                        if (result && !fieldId.Equals(Guid.Empty))
                        {
                            bool isContentTypeField = contenttypeNames[name].Contains(fieldId);
                            result = result & isContentTypeField;
                        }
                        result = !result;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case MappingFilterCondition.Contains:
                    foreach (string ctname in contenttypeNames.Keys)
                    {
                        if (ctname.Contains(name))
                        {
                            //构造customfiledmapping时filedid为空 这时返回true即可
                            if (fieldId.Equals(Guid.Empty) || contenttypeNames[ctname].Contains(fieldId))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                    break;
                case MappingFilterCondition.DoesNotContain:
                    if (!fieldId.Equals(Guid.Empty))
                    {
                        foreach (string ctname in contenttypeNames.Keys)
                        {
                            if (ctname.Contains(name))
                            {
                                if (contenttypeNames[ctname].Contains(fieldId))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                        result = !result;
                    }
                    else //因为id是空的时候，无法判断content type是否包含了这个column,因为此时这个column还没有进行还原
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

        private bool CheckContentTypeCondition(string name, List<string> contentTypes, MappingFilterCondition operation)
        {
            if (operation == MappingFilterCondition.Contains)
            {
                if (contentTypes.Contains(ConditionValue))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    public enum AveConditionType
    {
        URL,
        SiteContentType,
        TemplateID,
        ListTitle,
        ListContentType,
        Name,
        WebTemplate
    }

    public enum AveConditionRelation
    {
        And,
        Or,
    }

    public enum AveConditionOperation
    {
        Equal,
        NotEqual,
        Contains,
        NotContains,
    }

    [Serializable]
    public class AveCustomFieldForXmlInfo
    {
        protected AveCustomFieldInfo customFieldInfo;
        public string SourceName;
        public string DestinationName;
        public string SourceDisplayName;
        public string DestinationDisplayName;
        public AveCustomFieldType CustomFieldType;
        public List<AveMappingConditionInfo> ItemConditions
        {
            get { return MappingCondition.GetItemCondition(); }
        }
        public Dictionary<string, string> ValueMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public AveMappingCondition MappingCondition;

        internal virtual void Load(XmlElement node)
        {
            SourceName = node.GetAttribute("sourceName");
            DestinationName = node.GetAttribute("destinationName");
            SourceDisplayName = node.GetAttribute("sourceDisplayName");
            DestinationDisplayName = node.GetAttribute("destinationDisplayName");
            XmlNode valueMappingsNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ValueMappings", StringComparison.OrdinalIgnoreCase)).First();
            foreach (XmlNode n in valueMappingsNode.ChildNodes)
            {
                if (n.Name.Equals("ValueMapping", StringComparison.OrdinalIgnoreCase))
                {
                    string sourceValue = n.Attributes["sourceValue"].Value;
                    string destinationValue = n.Attributes["destinationValue"].Value;
                    ValueMapping[sourceValue] = destinationValue;
                }
            }
        }

        public virtual AveCustomFieldInfo GetCustomFieldInfo(bool useInternalOrDisplay)
        {
            if (customFieldInfo == null)
            {
                customFieldInfo = new AveCustomFieldInfo()
                {
                    InternalName = DestinationName,
                    Name = DestinationDisplayName,
                    CustomFieldType = CustomFieldType,
                };
            }
            customFieldInfo.UseInternalOrDisplay = useInternalOrDisplay;
            return customFieldInfo;
        }

        internal static AveCustomFieldForXmlInfo CreateCustomFieldInfo(XmlElement node)
        {
            string type = node.GetAttribute("type");
            AveCustomFieldForXmlInfo customFieldInfo = null;
            if (string.Equals("SameType", type, StringComparison.OrdinalIgnoreCase))
            {
                customFieldInfo = new AveSameTypeFieldInfo();
            }
            else if (string.Equals("ChangeToMetadata", type, StringComparison.OrdinalIgnoreCase))
            {
                customFieldInfo = new AveChangeToMetadataFieldInfo();
            }
            else if (string.Equals("ChangeToDes", type, StringComparison.OrdinalIgnoreCase))
            {
                customFieldInfo = new AveChangeToDestinationFieldInfo();
            }
            else if (string.Equals("ChangeToLookUp", type, StringComparison.OrdinalIgnoreCase))
            {
                customFieldInfo = new AveChangeToLookupFieldInfo();
            }

            if (customFieldInfo != null)
            {
                customFieldInfo.Load(node);
            }
            return customFieldInfo;
        }
        /// <summary>
        /// 由于contenttype condition 必须在还原column时check 所以要保存condition
        /// </summary>
        public void GetCondition(AveMappingCondition mappingCondition)
        {
            MappingCondition = mappingCondition;
        }
        public void SetConditonsMappingSourceSPListOrWeb(object sourceSPListOrWebInfo)
        {
            MappingCondition.AveMappingSourceSPListOrWebInfo = sourceSPListOrWebInfo;
        }
    }


    [Serializable]
    public class AveSameTypeFieldInfo : AveCustomFieldForXmlInfo
    {
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.SameType;
            base.Load(node);
        }
    }

    [Serializable]
    public class AveChangeToMetadataFieldInfo : AveCustomFieldForXmlInfo
    {
        public string TermSetPath;
        public bool AllowMultiValue;
        public string SeparateChar;
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.ChangeToMetadata;
            base.Load(node);
            XmlNode settingNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).First();
            TermSetPath = settingNode.Attributes["termSetPath"].Value;
            AllowMultiValue = Boolean.Parse(settingNode.Attributes["allowMultiValue"].Value);
            SeparateChar = settingNode.Attributes["separateChar"].Value;
        }

        public override AveCustomFieldInfo GetCustomFieldInfo(bool useInternalOrDisplay)
        {
            if (customFieldInfo == null)
            {
                String termGroup = String.Empty;
                String termSet = String.Empty;
                if (!(TermSetPath == "" && TermSetPath == string.Empty))
                {
                    termGroup = TermSetPath.Split(new char[] { ':', ';' })[0];
                    termSet = TermSetPath.Split(new char[] { ':', ';' })[1];
                }
                customFieldInfo = new AveCustomMetadataFieldInfo()
                {
                    InternalName = DestinationName,
                    Name = DestinationDisplayName,
                    TypeAsString = "TaxonomyFieldType",
                    Type = AveFieldType.Invalid,
                    TermGroup = termGroup,
                    TermSet = termSet,
                    IsMulti = AllowMultiValue,
                    UseInternalOrDisplay = useInternalOrDisplay,
                    CustomFieldType = CustomFieldType,
                    SeparateChar = SeparateChar
                };
                int index = -1;
                string[] path = TermSetPath.Split(new char[] { ':', ';' });
                if (path.Length > 2)
                {
                    index = path[0].Length + path[1].Length + 1;
                }
                if (index > 0)
                {
                    (customFieldInfo as AveCustomMetadataFieldInfo).Terms = TermSetPath.Substring(index + 1);
                }
                else
                {
                    (customFieldInfo as AveCustomMetadataFieldInfo).Terms = string.Empty;
                }
                (customFieldInfo as AveCustomMetadataFieldInfo).Terms = (customFieldInfo as AveCustomMetadataFieldInfo).Terms.Trim('|');
            }
            return customFieldInfo;
        }
    }

    [Serializable]
    public class AveChangeToDestinationFieldInfo : AveCustomFieldForXmlInfo
    {
        public string SeparateCharString;

        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.ChangeToDestination;
            base.Load(node);
            XmlNode settingNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).First();
            SeparateCharString = settingNode.Attributes["separateChar"] != null ? settingNode.Attributes["separateChar"].Value : null;
        }

        public override AveCustomFieldInfo GetCustomFieldInfo(bool useInternalOrDisplay)
        {
            if (customFieldInfo == null)
            {
                customFieldInfo = new AveCustomChangeToDesInfo()
                {
                    InternalName = DestinationName,
                    Name = DestinationDisplayName,
                    CustomFieldType = CustomFieldType,
                    SeparateChar = this.SeparateCharString,
                };
            }
            customFieldInfo.UseInternalOrDisplay = useInternalOrDisplay;
            return customFieldInfo;
        }
    }

    [Serializable]
    public class AveChangeToLookupFieldInfo : AveCustomFieldForXmlInfo
    {
        public string ListTitle;
        public string ColumnName;
        public string SeparateCharString;
        public bool AllowMultiValue;
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.ChangeToLookup;
            base.Load(node);
            XmlNode settingNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).First();
            ListTitle = settingNode.Attributes["listTitle"].Value;
            ColumnName = settingNode.Attributes["columnName"].Value;
            AllowMultiValue = Boolean.Parse(settingNode.Attributes["allowMultiValue"].Value);
            SeparateCharString = settingNode.Attributes["separateChar"] != null ? settingNode.Attributes["separateChar"].Value : null;
        }

        public override AveCustomFieldInfo GetCustomFieldInfo(bool useInternalOrDisplay)
        {
            if (customFieldInfo == null)
            {
                customFieldInfo = new AveCustomLookupFieldInfo()
                {
                    InternalName = DestinationName,
                    Name = DestinationDisplayName,
                    Type = AveFieldType.Lookup,
                    TypeAsString = "lookup",
                    ListTitle = ListTitle,
                    FieldName = ColumnName,
                    IsMulti = AllowMultiValue,
                    UseInternalOrDisplay = useInternalOrDisplay,
                    CustomFieldType = CustomFieldType,
                    SeparateChar = SeparateCharString
                };
            }
            return customFieldInfo;
        }
    }

    [Serializable]
    public class AveMappingSourceSPListInfo
    {
        public string Title;
        public string TemplateId;
        public Dictionary<string, List<Guid>> SourceListContentTypes = new Dictionary<string, List<Guid>>();
        public AveMappingSourceSPWebInfo ParentWeb;

        [Obsolete("use AveMappingSourceSPListInfo(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo, string fieldNodeName) instead.")]
        public AveMappingSourceSPListInfo(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo)
        {
            Initial(sourceListInfo, sourceWebInfo, webCTCollectionInfo);
            AveMappingSourceInfoProcessor.ConvertContentTypeInfo(SourceListContentTypes, webCTCollectionInfo, AveMappingSourceInfoProcessor.FIELD_ERF);
        }
        //对于List Title Mapping这个功能，初始化时不需要传入Content Type Info和Field Info，所以抽离出这个方法。
        public AveMappingSourceSPListInfo(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo)
        {
            Initial(sourceListInfo, sourceWebInfo);
        }

        public AveMappingSourceSPListInfo(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo, string fieldNodeName)
        {
            Initial(sourceListInfo, sourceWebInfo, webCTCollectionInfo);
            AveMappingSourceInfoProcessor.ConvertContentTypeInfo(SourceListContentTypes, webCTCollectionInfo, fieldNodeName);
        }

        public void Initial(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo)
        {
            Title = sourceListInfo.Title;
            TemplateId = Convert.ToString(sourceListInfo.BaseTemplate);
            ParentWeb = sourceWebInfo;
        }

        public void Initial(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo)
        {
            Title = sourceListInfo.Title;
            TemplateId = Convert.ToString(sourceListInfo.BaseTemplate);
            ParentWeb = sourceWebInfo;
        }
    }

    public class AveMappingSourceInfoProcessor
    {
        public const string FIELD_ERF = "FieldRef";
        public const string FIELD = "Field";

        public static void ConvertContentTypeInfo(Dictionary<string, List<Guid>> contentTypes, AveContentTypeCollectionInfo CTCollectionInfo, string fieldNodeName)
        {
            foreach (AveContentTypeInfo ctInfo in CTCollectionInfo.ContentTypes)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(ctInfo.FieldsSchemaXml);
                //XmlNodeList xnl = xDoc.DocumentElement.SelectNodes(fieldNodeName);
                if (!contentTypes.ContainsKey(ctInfo.Name))
                {
                    List<Guid> fieldIds = new List<Guid>();
                    foreach (XmlNode xn in xDoc.DocumentElement.ChildNodes)
                    {
                        Guid id = Guid.Empty;
                        if ((xn as XmlElement).HasAttribute("ID"))
                        {
                            id = new Guid(xn.Attributes["ID"].Value);
                        }
                        if (!id.Equals(Guid.Empty) && !fieldIds.Contains(id))
                        {
                            fieldIds.Add(id);
                        }
                    }
                    contentTypes.Add(ctInfo.Name, fieldIds);
                }
                else
                {
                    List<Guid> fieldIds = contentTypes[ctInfo.Name];
                    foreach (XmlNode xn in xDoc.DocumentElement.ChildNodes)
                    {
                        Guid id = Guid.Empty;
                        if ((xn as XmlElement).HasAttribute("ID"))
                        {
                            id = new Guid(xn.Attributes["ID"].Value);
                        }
                        if (!id.Equals(Guid.Empty) && !fieldIds.Contains(id))
                        {
                            fieldIds.Add(id);
                        }
                    }
                }

            }
        }
    }

    [Serializable]
    public class AveMappingSourceSPWebInfo
    {
        public string Url;
        public string TemplateId;
        public Dictionary<string, List<Guid>> SourceWebContentTypes = new Dictionary<string, List<Guid>>();
        //对于List Title Mapping这个功能，初始化时不需要传入Content Type Info，所以抽离出这个方法。
        public AveMappingSourceSPWebInfo(AveWebInfo sourceWebInfo)
        {
            Initial(sourceWebInfo);
        }
        public AveMappingSourceSPWebInfo(AveWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo)
        {
            Initial(sourceWebInfo, webCTCollectionInfo);
            AveMappingSourceInfoProcessor.ConvertContentTypeInfo(SourceWebContentTypes, webCTCollectionInfo, AveMappingSourceInfoProcessor.FIELD_ERF);
        }

        public AveMappingSourceSPWebInfo(AveWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo, string srcXmlFieldNodeName)
        {
            Initial(sourceWebInfo, webCTCollectionInfo);
            AveMappingSourceInfoProcessor.ConvertContentTypeInfo(SourceWebContentTypes, webCTCollectionInfo, srcXmlFieldNodeName);
        }

        //对于List Title Mapping这个功能，初始化时不需要传入Content Type Info，所以抽离出这个方法。
        private void Initial(AveWebInfo sourceWebInfo)
        {
            Url = sourceWebInfo.Url;
            TemplateId = sourceWebInfo.WebTemplate;
        }
        private void Initial(AveWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo)
        {
            Url = sourceWebInfo.Url;
            TemplateId = sourceWebInfo.WebTemplate;
        }
    }
}
