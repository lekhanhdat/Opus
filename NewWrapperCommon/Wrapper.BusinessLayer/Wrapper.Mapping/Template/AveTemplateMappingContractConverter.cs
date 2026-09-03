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
using System.Xml;
using AvePoint.GCommon.Contract.Server.ControlPanel.TemplateMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    public class AveTemplateMappingContractConverter
    {
        public static XmlDocument Convert(TemplateMappingContract contract)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("CustomTemplateMapping"));
            XmlElement siteTemplates = doc.CreateElement("SiteCollection");
            siteTemplates.SetAttribute("name", "*");
            if (contract.SiteTemplateMappings != null)
            {
                InitTemplateMappings(siteTemplates, contract.SiteTemplateMappings);
            }
            doc.DocumentElement.AppendChild(siteTemplates);
            XmlElement listTemplates = doc.CreateElement("Web");
            listTemplates.SetAttribute("name", "*");
            if (contract.ListTemplateMappings != null)
            {
                InitTemplateMappings(listTemplates, contract.ListTemplateMappings);
            }
            doc.DocumentElement.AppendChild(listTemplates);
            return doc;
        }

        private static void InitTemplateMappings(XmlElement element,List<MappingContent> mappings)
        {
            XmlElement templateMappings = element.OwnerDocument.CreateElement("TemplateMappings");
            foreach (MappingContent mapping in mappings)
            {
                XmlElement templateMapping = element.OwnerDocument.CreateElement("TemplateMapping");
                templateMapping.SetAttribute("key", mapping.SourceDomain);
                templateMapping.SetAttribute("value", mapping.TargetDomain);
                templateMappings.AppendChild(templateMapping);
            }
            element.AppendChild(templateMappings);
        }
    }
}
