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
using AvePoint.GCommon.Contract.Server.ControlPanel.ContentTypeMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    public class AveContentTypeMappingContractConverter
    {
        public static XmlDocument Convert(ContentTypeMappingDataContract contract)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("ContentTypeMappings"));
            if (contract.contentMappings != null)
            {
                foreach (ContentTypeMappingDto mappingDto in contract.contentMappings)
                {
                    XmlElement contentTypeMapping = doc.CreateElement("ContentTypeMapping");
                    XmlElement condition = doc.CreateElement("Condition");
                    if (mappingDto.SiteConditions != null)
                    {
                        XmlElement siteCondition = doc.CreateElement("SiteCondition");
                        foreach (ColumnFilter filter in mappingDto.SiteConditions)
                        {
                            InitConditionRule(siteCondition, filter);
                            condition.AppendChild(siteCondition);
                        }
                    }
                    if (mappingDto.ListConditions != null)
                    {
                        XmlElement listCondition = doc.CreateElement("ListCondition");
                        foreach (ColumnFilter filter in mappingDto.ListConditions)
                        {
                            InitConditionRule(listCondition, filter);
                            condition.AppendChild(listCondition);
                        }
                    }
                    contentTypeMapping.AppendChild(condition);

                    XmlElement mappings = doc.CreateElement("Mappings");
                    if (mappingDto.MappingValues != null)
                    {
                        foreach (MappingValue value in mappingDto.MappingValues)
                        {
                            XmlElement mapping = doc.CreateElement("Mapping");
                            mapping.SetAttribute("sourceName", value.SourceGroupName);
                            mapping.SetAttribute("destinationName", value.DestinationGroupName);
                            mappings.AppendChild(mapping);
                        }
                    }
                    contentTypeMapping.AppendChild(mappings);
                    doc.DocumentElement.AppendChild(contentTypeMapping);
                }
            }
            return doc;
        }

        private static void InitConditionRule(XmlElement element, ColumnFilter filter)
        {
            if (filter.Conditions != null)
            {
                foreach (ConditionItem condition in filter.Conditions)
                {
                    XmlElement conditionRule = element.OwnerDocument.CreateElement("ConditionRule");
                    conditionRule.SetAttribute("type", condition.MetaDataType.ToString());
                    conditionRule.SetAttribute("condition", condition.ConditionType.ToString());
                    conditionRule.SetAttribute("value", condition.Value);
                    conditionRule.SetAttribute("relation", condition.AndOr.ToString());
                    element.AppendChild(conditionRule);
                }
            }
        }
    }
}
