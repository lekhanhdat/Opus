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

namespace AvePoint.Wrapper.Common
{
    public class V3WebPartPropertyExtractor : WebPartPropertyExtractorBase
    {
        public const string WebPartV3NameSpace = "http://schemas.microsoft.com/WebPart/v3";
        private XmlNamespaceManager v3NameSpaceManager;
        private string typeFullName;
        private Guid? solutionId;
        private XmlNode propertiesNode;
        private XmlNode webPartsNode;

        public V3WebPartPropertyExtractor(XmlNode webpartDefinition) : base(webpartDefinition)
        {
            v3NameSpaceManager = new XmlNamespaceManager(base.webpartDefinition.OwnerDocument.NameTable);
            v3NameSpaceManager.AddNamespace("d", WebPartV3NameSpace);
            v3NameSpaceManager.AddNamespace("sp", "http://schemas.microsoft.com/sharepoint/");
        }

        protected override XmlNode GetPropertyValue(string propertyName)
        {
            return base.webpartDefinition.SelectSingleNode(string.Format("d:data/d:properties/d:property[@name='{0}']", propertyName), v3NameSpaceManager);            
        }
        public override bool AddProperty(bool properties, string propertyName, object value)
        {
            if (properties)
            {
                if (propertiesNode == null)
                {
                    propertiesNode = base.webpartDefinition.SelectSingleNode("d:data/d:properties", v3NameSpaceManager);
                }

                var propertyElement = propertiesNode.OwnerDocument.CreateElement("property", WebPartV3NameSpace);
                propertyElement.SetAttribute("name", propertyName);
                propertyElement.SetAttribute("type", value.GetType().Name.ToLower());
                propertyElement.InnerText = value.ToString();

                propertiesNode.AppendChild(propertyElement);
            }
            else
            {
                if (webPartsNode == null)
                {
                    webPartsNode = base.webpartDefinition.OwnerDocument.SelectSingleNode("webParts", v3NameSpaceManager);
                }

                var propertyElement = propertiesNode.OwnerDocument.CreateElement(propertyName);
                propertyElement.InnerText = value.ToString();

                webPartsNode.AppendChild(propertyElement);
            }

            return true;
        }
        public override string TypeFullName
        {
            get 
            {
                if (typeFullName == null)
                {
                    XmlNode typeNode = base.webpartDefinition.SelectSingleNode("d:metaData/d:type/@name", v3NameSpaceManager);
                    typeFullName = typeNode != null ? typeNode.InnerText : null;                    
                }
                return typeFullName;
            }
        }

        public override Guid SolutionId
        {
            get 
            {
                if (solutionId == null)
                {
                    XmlNode typeNode = base.webpartDefinition.SelectSingleNode("d:metaData/sp:Solution/@SolutionId", v3NameSpaceManager);
                    solutionId = typeNode != null ? new Guid(typeNode.InnerText) : Guid.Empty;                    
                }
                return solutionId.Value;
            }
        }
    }
}
