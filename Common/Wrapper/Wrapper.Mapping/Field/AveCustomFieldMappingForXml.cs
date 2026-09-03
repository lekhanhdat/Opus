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
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.Wrapper.Mapping;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    internal class AveCustomFieldMappingForXml : IAveCustomFieldMapping
    {
        Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> internalMapping;
        /// <summary>
        /// 已经在还原field的时候找到了对应AveSourceFieldInfo AveCustomFieldForXmlInfo对 保存起来 fieldvalue的mapping时使用
        /// </summary>
        Dictionary<AveSourceFieldInfo, AveCustomFieldForXmlInfo> realInternalMapping = new Dictionary<AveSourceFieldInfo, AveCustomFieldForXmlInfo>(new AveCustomFieldInfoEqualityComparer());

        private Dictionary<string, object> nullToDefaultValueMapping = new Dictionary<string, object>();

        public Dictionary<string, object> NullToDefaultValueMapping
        {
            get { return nullToDefaultValueMapping; }
        }

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

         /// <summary>
         /// 用于value mapping，如果是源端type是managed metadata column 要支持name的get还有name|id的get
         /// </summary>
         /// <param name="sourceFieldValueInfo"></param>
         /// <returns></returns>
        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            string sourceValue = sourceFieldValueInfo.SourceValue;
            string mappedValue = sourceValue;
            mappedValue = InternalGetMappingValue(sourceFieldValueInfo);
            //这里区分大小写,name和name|id源端都要支持
            if (mappedValue.Equals(sourceValue) && sourceFieldValueInfo.SourceFieldInfo.SourceType.Equals(AveFieldType.Invalid) && sourceValue.Contains("|")) 
            {
                sourceFieldValueInfo.SourceValue = sourceValue.Substring(0, sourceValue.IndexOf("|"));
                mappedValue = InternalGetMappingValue(sourceFieldValueInfo);
            }
            return mappedValue;
        }

        private string InternalGetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (realInternalMapping != null && realInternalMapping.ContainsKey(sourceFieldValueInfo.SourceFieldInfo) && realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].MappingCondition.CheckItemCondition(sourceFieldValueInfo.SourceItemName) && realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping.ContainsKey(sourceFieldValueInfo.SourceValue))
            {
                return realInternalMapping[sourceFieldValueInfo.SourceFieldInfo].ValueMapping[sourceFieldValueInfo.SourceValue];
            }
            else
            {
                return sourceFieldValueInfo.SourceValue;
            }
        }

        public void InitNullToDefaultValueMapping()
        {
            if (this.realInternalMapping != null)
            {
                foreach (var mapping in this.realInternalMapping)
                {
                    if (mapping.Value.ValueMapping != null)
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
            }
        }

        public void Dispose()
        {
        }
    }

    public class AveMappingCondition
    {
        private List<AveMappingConditionInfo> siteCondition = new List<AveMappingConditionInfo>();
        private List<AveMappingConditionInfo> listCondition = new List<AveMappingConditionInfo>();
        private List<AveMappingConditionInfo> itemCondition = new List<AveMappingConditionInfo>();
        public object AveMappingSourceSPListOrWebInfo;

        public void Load(XmlElement node,bool hasItemCondition)
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
        public bool CheckCondition(object listOrWeb, Guid fieldId)
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
        private bool CheckConditionResult(object listOrWeb, List<AveMappingConditionInfo> conditions, Guid fieldId)
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

    public class AveMappingConditionInfo
    {
        private AveConditionType conditionType;
        private MappingFilterCondition operation;
        private string value;
        private AveConditionRelation relation;
        public void Load(XmlNode node)
        {
            conditionType = (AveConditionType)Enum.Parse(typeof(AveConditionType), node.Attributes["type"].Value, true);
            operation = (MappingFilterCondition)Enum.Parse(typeof(MappingFilterCondition), node.Attributes["condition"].Value, true);
            value = node.Attributes["value"].Value;
            relation = (AveConditionRelation)Enum.Parse(typeof(AveConditionRelation), node.Attributes["relation"].Value, true);
        }

        internal AveConditionRelation GetRelation()
        {
            return relation;
        }

        public bool CheckCondition(object listOrWeb,Guid fieldId)
        {
            switch (conditionType)
            {
                case AveConditionType.URL:
                    return CheckURLCondition(listOrWeb);
                case AveConditionType.SiteContentType:
                    return CheckSiteContentTypeCondition(listOrWeb, fieldId);
                case AveConditionType.TemplateID:
                    return CheckListTemplateIdCondition(listOrWeb as AveMappingSourceSPListInfo );
                case AveConditionType.ListTitle:
                    return CheckListTitleCondition(listOrWeb as AveMappingSourceSPListInfo );
                case AveConditionType.ListContentType:
                    return CheckListContentTypeCondition(listOrWeb as AveMappingSourceSPListInfo, fieldId);
            }

            return false;
        }

        public bool CheckCondition(AveFieldMappingConditionInfo condition)
        {
            switch (conditionType)
            {
                case AveConditionType.URL:
                    return CheckURLCondition(condition);
                case AveConditionType.SiteContentType:
                    return CheckSiteContentTypeCondition(condition);
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
            return CheckStringCondtion(value, itemName, operation);
        }

        private bool CheckURLCondition(object listOrWeb)
        {
            string url = string.Empty;
            if (listOrWeb is AveMappingSourceSPListInfo ) 
            {
                url=(listOrWeb as AveMappingSourceSPListInfo ).ParentWeb.Url;   
            }
            else if (listOrWeb is AveMappingSourceSPWebInfo) 
            {
                url = (listOrWeb as AveMappingSourceSPWebInfo).Url;
            }
            return CheckStringCondtion(value, url, operation);
        }

        private bool CheckURLCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(value, condition.SiteUrl, operation);
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
            return CheckContentTypeCondtion(value, contentTypesNames, operation, fieldId);
        }

        private bool CheckSiteContentTypeCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckContentTypeCondition(value, condition.SiteContentTypeCollection, operation);
        }

        private bool CheckListTemplateIdCondition(AveMappingSourceSPListInfo  list)
        {
            string listTemplateID = list.TemplateId;
            return CheckStringCondtion(value, listTemplateID, operation);
        }

        private bool CheckListTemplateIdCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(value, condition.ListTemplateID, operation);
        }

        private bool CheckListTitleCondition(AveMappingSourceSPListInfo  list)
        {
            string listTitle = list.Title;
            return CheckStringCondtion(value, listTitle, operation);
        }

        private bool CheckListTitleCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckStringCondtion(value, condition.ListTitle, operation);
        }

        private bool CheckListContentTypeCondition(AveMappingSourceSPListInfo list, Guid fieldId)
        {
            return CheckContentTypeCondtion(value, list.SourceListContentTypes, operation, fieldId);
        }

        private bool CheckListContentTypeCondition(AveFieldMappingConditionInfo condition)
        {
            return CheckContentTypeCondition(value, condition.ListContentTypeCollection, operation);
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
                    return comparedValue.Contains(value);
                case MappingFilterCondition.DoesNotContain:
                    return !comparedValue.Contains(value);
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
                    {
                        result = contenttypeNames.ContainsKey(name);
                        if (result && !fieldId.Equals(Guid.Empty))
                        {
                            bool isContentTypeField = contenttypeNames[name].Contains(fieldId);
                            result = result & isContentTypeField;
                        }
                    }
                    result = !result;
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
                                if (fieldId.Equals(Guid.Empty) || (!contenttypeNames[ctname].Contains(fieldId)))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
            return result;
        }

        private bool CheckContentTypeCondition(string name, List<string> contentTypes, MappingFilterCondition operation)
        {
            if (operation == MappingFilterCondition.Contains)
            {
                if (contentTypes.Contains(value))
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

    internal enum AveConditionType
    {
        URL,
        SiteContentType,
        TemplateID,
        ListTitle,
        ListContentType,
        Name,
    }

    internal enum AveConditionRelation
    {
        And,
        Or,
    }

    internal enum AveConditionOperation
    {
        Equal,
        NotEqual,
        Contains,
        NotContains,
    }

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
        public Dictionary<string, string> ValueMapping = new Dictionary<string,string>();
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
                    CustomFieldTypeAsString = CustomFieldType.ToString()
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

    public enum AveCustomFieldType
    {
        SameType,
        ChangeToMetadata,
        ChangeToDes,
        ChangeToLookUp,
    }

    public class AveSameTypeFieldInfo : AveCustomFieldForXmlInfo
    {
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.SameType;
            base.Load(node);
        }
    }

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
                    termGroup = TermSetPath.Split(new char[] { ';' })[0];
                    termSet = TermSetPath.Split(new char[] { ';' })[1];
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
                    CustomFieldTypeAsString = CustomFieldType.ToString(),
                    SeparateChar = SeparateChar
                };
                int index = -1;
                string[] path = TermSetPath.Split(new char[] { ';' });
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

    public class AveChangeToDestinationFieldInfo : AveCustomFieldForXmlInfo
    {
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.ChangeToDes;
            base.Load(node);
        }
    }

    public class AveChangeToLookupFieldInfo : AveCustomFieldForXmlInfo
    {
        public string ListTitle;
        public string ColumnName;
        public string SeparateCharString;
        public bool AllowMultiValue;
        internal override void Load(XmlElement node)
        {
            CustomFieldType = AveCustomFieldType.ChangeToLookUp;
            base.Load(node);
            XmlNode settingNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).First();
            ListTitle = settingNode.Attributes["listTitle"].Value;
            ColumnName = settingNode.Attributes["columnName"].Value;
            AllowMultiValue = Boolean.Parse(settingNode.Attributes["allowMultiValue"].Value);
            SeparateCharString=settingNode.Attributes["separateChar"].Value;
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
                    CustomFieldTypeAsString = CustomFieldType.ToString(),
                    SeparateChar = SeparateCharString
                };
            }
            return customFieldInfo;
        }
    }

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

        public AveMappingSourceSPListInfo(AveListInfo sourceListInfo, AveMappingSourceSPWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo, string fieldNodeName)
        {
            Initial(sourceListInfo, sourceWebInfo, webCTCollectionInfo);
            AveMappingSourceInfoProcessor.ConvertContentTypeInfo(SourceListContentTypes, webCTCollectionInfo, fieldNodeName);
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
                if (string.IsNullOrEmpty(ctInfo.FieldsSchemaXml))
                {
                    continue;
                }
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

    public class AveMappingSourceSPWebInfo
    {
        public string Url;
        public string TemplateId;
        public Dictionary<string, List<Guid>> SourceWebContentTypes = new Dictionary<string, List<Guid>>();
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

        private void Initial(AveWebInfo sourceWebInfo, AveContentTypeCollectionInfo webCTCollectionInfo)
        {
            Url = sourceWebInfo.Url;
            TemplateId = sourceWebInfo.WebTemplate;
        }
    }
}
