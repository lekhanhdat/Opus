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
    using System.Xml;

    public class AveCustomTemplateMapping : IAveCustomTemplateMapping
    {
        private Dictionary<string, AveInternalCustomTemplateMappingInfo> webCustomTemplateMapping = new Dictionary<string, AveInternalCustomTemplateMappingInfo>();
        private Dictionary<string, AveInternalCustomTemplateMappingInfo> listCustomTemplateMapping = new Dictionary<string, AveInternalCustomTemplateMappingInfo>();

        public AveCustomTemplateMapping(XmlElement config)
        {
            this.webCustomTemplateMapping = config.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "SiteCollection", StringComparison.OrdinalIgnoreCase))
                .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("name"), child => new AveInternalCustomTemplateMappingInfo(child), StringComparer.OrdinalIgnoreCase);
            this.listCustomTemplateMapping = config.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Web", StringComparison.OrdinalIgnoreCase))
                .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("name"), child => new AveInternalCustomTemplateMappingInfo(child), StringComparer.OrdinalIgnoreCase);
        }

        //public Dictionary<string, AveInternalCustomTemplateMappingInfo> WebCustomTemplateMapping
        //{
        //    get { return webCustomTemplateMapping; }
        //}

        //public Dictionary<string, AveInternalCustomTemplateMappingInfo> ListCustomTemplateMapping
        //{
        //    get { return listCustomTemplateMapping; }
        //}

        public string GetMappingTemplateBeforeAdd(TemplateKeyInfo srcTemplateInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.AveCustomTemplateMapping.GetMappingTemplateBeforeAdd"))
            {
#endif
                string templateName = srcTemplateInfo.templateSrcValue;
                srcTemplateInfo.keyValue = string.IsNullOrEmpty(srcTemplateInfo.keyValue) ? "*" : srcTemplateInfo.keyValue;
                switch (srcTemplateInfo.keyLevel)
                {
                    case TemplateMappingLevel.Global:
                        {
                            foreach (AveInternalCustomTemplateMappingInfo templateMappingInfo in webCustomTemplateMapping.Values)
                            {
                                if (templateMappingInfo.CheckTemplateValue(templateName))
                                {
                                    templateName = templateMappingInfo.GetMappingValue(templateName);
                                    break;
                                }
                            }
                            foreach (AveInternalCustomTemplateMappingInfo templateMappingInfo in listCustomTemplateMapping.Values)
                            {
                                if (templateMappingInfo.CheckTemplateValue(templateName))
                                {
                                    templateName = templateMappingInfo.GetMappingValue(templateName);
                                    break;
                                }
                            }
                        }
                        break;
                    case TemplateMappingLevel.Web:
                        templateName = webCustomTemplateMapping.ContainsKey(srcTemplateInfo.keyValue) ? webCustomTemplateMapping[srcTemplateInfo.keyValue].GetMappingValue(templateName) : templateName;
                        break;
                    case TemplateMappingLevel.List:
                        templateName = listCustomTemplateMapping.ContainsKey(srcTemplateInfo.keyValue) ? listCustomTemplateMapping[srcTemplateInfo.keyValue].GetMappingValue(templateName) : templateName;
                        break;
                }
                return templateName;
#if PerformanceLog
            }
#endif
        }

        public void Dispose()
        {
            this.webCustomTemplateMapping.Clear();
            this.listCustomTemplateMapping.Clear();
        }


    }

    public class AveInternalCustomTemplateMappingInfo
    {
        public AveInternalCustomTemplateMappingInfo(XmlElement config)
        {
            foreach (XmlElement mappingEle in config.GetElementsByTagName("TemplateMapping"))
            {
                valueMapping.Add(mappingEle.GetAttribute("key"), mappingEle.GetAttribute("value"));
            }
        }

        private Dictionary<string, string> valueMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> ValueMapping
        {
            get { return valueMapping; }
        }

        public string GetMappingValue(string srcVaule)
        {
            if (this.valueMapping != null && this.valueMapping.ContainsKey(srcVaule))
            {
                return this.valueMapping[srcVaule];
            }
            return srcVaule;
        }

        public bool CheckTemplateValue(string srcValue)
        {
            return valueMapping.ContainsKey(srcValue);
        }
    }
}
