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

namespace AvePoint.Wrapper.Mapping
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    internal class AveCustomContentTypeMappingForXml : IAveCustomContentTypeMapping
    {
        Dictionary<string, AveCustomContentTypeForXmlInfo> internalMapping;

        public AveCustomContentTypeMappingForXml(Dictionary<string, AveCustomContentTypeForXmlInfo> mappings)
        {
            this.internalMapping = mappings;
        }

        public AveCustomContentTypeInfo GetMappingContentTypeBeforeAdd(string srcCTName)
        {
            if (internalMapping != null && internalMapping.ContainsKey(srcCTName))
            {
                return internalMapping[srcCTName].GetCustomContentTypeInfo();
            }
            else
            {
                return null;
            }
        }

        public string GetContentTypeNameMappingFromGui(string srcCTName) 
        {
            string name = srcCTName;
            AveCustomContentTypeInfo customContentTypeInfo = GetMappingContentTypeBeforeAdd(srcCTName);
            if (customContentTypeInfo != null) 
            {
                name = customContentTypeInfo.Name;
            }
            return name;
        }

        public void Dispose()
        {

        }
    }

    public class AveCustomContentTypeForXmlInfo
    {
        protected AveCustomContentTypeInfo customContentTypeInfo;
        public string SourceName;
        public string DestinationName;
        public List<AveMappingConditionInfo> ItemConditions;
        public Dictionary<string, string> ValueMapping = new Dictionary<string, string>();

        internal virtual void Load(XmlElement node)
        {
            SourceName = node.GetAttribute("sourceName");
            DestinationName = node.GetAttribute("destinationName");
        }

        public virtual AveCustomContentTypeInfo GetCustomContentTypeInfo()
        {
            if (customContentTypeInfo == null)
            {
                customContentTypeInfo = new AveCustomContentTypeInfo()
                {
                    Name = DestinationName,
                };
            }
            return customContentTypeInfo;
        }
        internal static AveCustomContentTypeForXmlInfo CreateCustomContentTypeInfo(XmlElement node)
        {
            AveCustomContentTypeForXmlInfo customContentTypeInfo = new AveCustomContentTypeForXmlInfo();
            customContentTypeInfo.Load(node);
            return customContentTypeInfo;
        }
       
    }
}