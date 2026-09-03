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

    internal class AveCustomFieldMapping : IAveCustomFieldMapping
    {
        readonly Dictionary<AveSourceFieldInfo, AveInternalCustomFieldMappingInfo> internalMapping;

        public AveCustomFieldMapping(XmlElement config)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.AveCustomFieldMapping.Constructor"))
            {
#endif
                this.internalMapping = config.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "column", StringComparison.OrdinalIgnoreCase))
                    .Cast<XmlElement>().ToDictionary(child => new AveSourceFieldInfo(){ SourceDisplayName = child.GetAttribute("name")}, child => new AveInternalCustomFieldMappingInfo(child),
    new AveCustomFieldInfoEqualityComparer());
#if PerformanceLog
            }
#endif
        }

        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            if (internalMapping != null && internalMapping.ContainsKey(sourceFieldInfo))
            {
                return internalMapping[sourceFieldInfo].Destination;
            }
            else
            {
                return null;
            }
        }

        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (internalMapping != null && internalMapping.ContainsKey(sourceFieldValueInfo.SourceFieldInfo))
            {
                return internalMapping[sourceFieldValueInfo.SourceFieldInfo].GetMappingValue(sourceFieldValueInfo.SourceValue);
            }
            else
            {
                return sourceFieldValueInfo.SourceValue;
            }
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
                    switch (typeElement.InnerText.ToLower())
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
                            info.TypeAsString = typeElement.InnerText.ToLower();
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

        public Dictionary<string, object> NullToDefaultValueMapping
        {
            get { return new Dictionary<string, object>(); }
        }
    }
}
