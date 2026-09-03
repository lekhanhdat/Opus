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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.Wrapper.Mapping
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveCustomContentTypeMappingForXmlFatory : IAveCustomContentTypeMappingFactory
    {
        private Dictionary<AveMappingCondition, List<AveCustomContentTypeForXmlInfo>> customContentTypeMappings;
        public AveCustomContentTypeMappingForXmlFatory(XmlDocument xDoc, bool hasItemCondition)
        {
            this.Load(xDoc, hasItemCondition);
        }

        public IAveCustomContentTypeMapping GetMappingForListOrWeb(object listOrWeb)
        {
            Dictionary<string, AveCustomContentTypeForXmlInfo> mappings = GetCustomContentTypeMappings(listOrWeb);
            return new AveCustomContentTypeMappingForXml(mappings);
        }

        private void Load(XmlDocument xDoc,bool hasItemCondition)
        {
            customContentTypeMappings = new Dictionary<AveMappingCondition, List<AveCustomContentTypeForXmlInfo>>();
            XmlNodeList nodes = xDoc.GetElementsByTagName("ContentTypeMapping");
            foreach (XmlNode n in nodes)
            {
                XmlNode conditionNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Condition", StringComparison.OrdinalIgnoreCase)).First();
                AveMappingCondition mappingCondition = new AveMappingCondition();
                mappingCondition.Load(conditionNode as XmlElement,hasItemCondition);
                XmlNode mappingsNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Mappings", StringComparison.OrdinalIgnoreCase)).First();
                List<AveCustomContentTypeForXmlInfo> customContentTypeInfos = new List<AveCustomContentTypeForXmlInfo>();
                foreach (XmlNode mappingNode in mappingsNode)
                {
                    AveCustomContentTypeForXmlInfo info = AveCustomContentTypeForXmlInfo.CreateCustomContentTypeInfo(mappingNode as XmlElement);
                    info.Load(mappingNode as XmlElement);
                    info.ItemConditions = mappingCondition.GetItemCondition();
                    customContentTypeInfos.Add(info);
                }
                customContentTypeMappings[mappingCondition] = customContentTypeInfos;
            }
        }

        private Dictionary<string, AveCustomContentTypeForXmlInfo> GetCustomContentTypeMappings(object listOrWeb)
        {
            Dictionary<string, AveCustomContentTypeForXmlInfo> listCustomContentTypeMappings = new Dictionary<string, AveCustomContentTypeForXmlInfo>();
            if (customContentTypeMappings != null)
            {
                foreach (AveMappingCondition condition in customContentTypeMappings.Keys)
                {
                    if (condition.CheckCondition(listOrWeb, Guid.Empty))
                    {
                        List<AveCustomContentTypeForXmlInfo> ContentTypeInfos = customContentTypeMappings[condition];
                        foreach (AveCustomContentTypeForXmlInfo info in ContentTypeInfos)
                        {
                            listCustomContentTypeMappings[info.SourceName] = info;
                        }
                        break;
                    }
                }
            }
            return listCustomContentTypeMappings;
        }
    }
}
