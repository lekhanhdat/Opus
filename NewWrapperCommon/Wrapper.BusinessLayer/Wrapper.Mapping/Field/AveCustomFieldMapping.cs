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
    using System.Xml;
    using AvePoint.Common;
    using AvePoint.Wrapper.Common;
    using System.Globalization;

    internal class AveCustomFieldMapping : IAveCustomFieldMapping
    {
        readonly Dictionary<AveSourceFieldInfo, AveInternalCustomFieldMappingInfo> internalMapping;

        public AveCustomFieldMapping(XmlElement config)
        {
            this.internalMapping = config.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "column", StringComparison.OrdinalIgnoreCase))
                .Cast<XmlElement>().ToDictionary(child => new AveSourceFieldInfo() { SourceDisplayName = child.GetAttribute("name") }, child => new AveInternalCustomFieldMappingInfo(child),
new AveCustomFieldInfoEqualityComparer());
        }

        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            if (internalMapping != null && internalMapping.ContainsKey(sourceFieldInfo))
            {
                return internalMapping[sourceFieldInfo].Destination;
            }
            return null;
        }

        public List<AveCustomFieldInfo> GetNewFieldsBeforeAdd()
        {
            return null;
        }

        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (internalMapping != null && internalMapping.ContainsKey(sourceFieldValueInfo.SourceFieldInfo) && sourceFieldValueInfo.SourceValue != null)
            {
                return internalMapping[sourceFieldValueInfo.SourceFieldInfo].GetMappingValue(sourceFieldValueInfo.SourceValue);
            }
            return null;
        }

        public List<string> GetMultiMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (internalMapping != null && sourceFieldValueInfo.SourceValue != null)
            {
                AveInternalCustomFieldMappingInfo fieldInfo;
                if (internalMapping.TryGetValue(sourceFieldValueInfo.SourceFieldInfo, out fieldInfo))
                {
                    List<string> mappingValue = new List<string>();
                    foreach (var pValue in PrepareMappingValues(sourceFieldValueInfo))
                    {
                        mappingValue.Add(fieldInfo.GetMappingValue(sourceFieldValueInfo.SourceValue));
                    }
                    return mappingValue;
                }
            }
            return null;
        }

        private List<string> PrepareMappingValues(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            string sourceValue = sourceFieldValueInfo.SourceValue;
            string splitString = string.Empty;
            List<string> prepareValues = new List<string>();
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
                prepareValues = sourceValue.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                prepareValues.Add(sourceValue);
            }
            return prepareValues;
        }

        public object GetMappingNullValue(string fieldInternalName)
        {
            return null;
        }

        #region Internal mapping by xml
        private class AveInternalCustomFieldMappingInfo
        {
            public AveInternalCustomFieldMappingInfo(XmlElement config)
            {
                Destination = Singleton<AveCustomFieldInfoInternalFactory>.SingletonInstance.CreateFieldInfo(config.SelectSingleNode("destinationColumnInfo") as XmlElement);
                var valuesMappingConfig = config.SelectSingleNode("values") as XmlElement;
                if (valuesMappingConfig != null)
                {
                    this.valueMapping = valuesMappingConfig.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "value", StringComparison.OrdinalIgnoreCase))
                        .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("source"), child => child.GetAttribute("destination"), StringComparer.OrdinalIgnoreCase);
                }
            }

            public AveCustomFieldInfo Destination { get; set; }

            Dictionary<string, string> valueMapping;

            public string GetMappingValue(string srcVaule)
            {
                if (this.valueMapping != null && this.valueMapping.ContainsKey(srcVaule))
                {
                    return this.valueMapping[srcVaule];
                }
                return srcVaule;
            }
        }

        private class AveCustomFieldInfoInternalFactory : ISingleton
        {
            const string MMSField = "managed metadata";
            const string LookupField = "lookup";
            const string ChoiceField = "choice";
            const string YesOrNoField = "boolean";

            private AveCustomFieldInfoInternalFactory() { }

            public AveCustomFieldInfo CreateFieldInfo(XmlElement config)
            {
                if (config == null)
                {
                    return null;
                }
                var info = new AveCustomFieldInfo();

                var typeElement = config.SelectSingleNode("type") as XmlElement;
                if (typeElement != null)
                {
                    switch (typeElement.InnerText.ToLower(CultureInfo.InvariantCulture))
                    {
                        case MMSField:
                            info = new AveCustomMetadataFieldInfo()
                            {
                                TypeAsString = "TaxonomyFieldType",
                                Type = AveFieldType.Invalid,
                                TermGroup = GetNodeInnerText(config, "termGroup"),
                                TermSet = GetNodeInnerText(config, "termSet"),
                                IsMulti = "true".Equals(GetNodeInnerText(config, "IsMulti"), StringComparison.OrdinalIgnoreCase) ? true : false
                            };
                            break;
                        case LookupField:
                            info = new AveCustomLookupFieldInfo()
                            {
                                Type = AveFieldType.Lookup,
                                TypeAsString = LookupField,
                                WebRelativeUrl = GetNodeInnerText(config, "webRelativeUrl"),
                                ListTitle = GetNodeInnerText(config, "listTitle"),
                                FieldName = GetNodeInnerText(config, "fieldName"),
                                IsMulti = "true".Equals(GetNodeInnerText(config, "IsMulti"), StringComparison.OrdinalIgnoreCase) ? true : false
                            };
                            break;
                        case ChoiceField:
                            info = new AveCustomChoiceFieldInfo()
                            {
                                Type = AveFieldType.Choice,
                                TypeAsString = ChoiceField,
                                IsMulti = "true".Equals(GetNodeInnerText(config, "IsMulti"), StringComparison.OrdinalIgnoreCase) ? true : false
                            };
                            break;
                        case YesOrNoField:
                            info = new AveCustomYesOrNoFieldInfo()
                            {
                                Type = AveFieldType.Boolean,
                                TypeAsString = YesOrNoField
                            };
                            break;
                        default:
                            info.TypeAsString = typeElement.InnerText.ToLower(CultureInfo.InvariantCulture);
                            break;
                    }
                }
                info.Name = GetNodeInnerText(config, "name");
                return info;
            }

            private string GetNodeInnerText(XmlElement config, string subName)
            {
                var subNode = config.SelectSingleNode(subName);
                if (subNode != null)
                {
                    return subNode.InnerText;
                }
                return null;
            }
        }
        #endregion

        public void Dispose()
        {
        }



        public void GetValuesFromExcel(string excelPath)
        {
            throw new NotImplementedException();
        }

        public string GetValueFromGuiMapping(AveSourceFieldValueInfo source)
        {
            return null;
        }
    }
}
