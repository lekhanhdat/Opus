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
    public class V2WebPartPropertyExtractor : WebPartPropertyExtractorBase
    {
        public const string WebPartV2NameSpace = "http://schemas.microsoft.com/WebPart/v2";
        public const string WebPartV2MembersNameSpace = "http://schemas.microsoft.com/WebPart/v2/Members";
        private XmlNamespaceManager v2NameSpaceManager;
        private string typeFullName;
        private XmlNode webPartNode;

        public V2WebPartPropertyExtractor(XmlNode webpartDefinition) : base(webpartDefinition)
        {
            v2NameSpaceManager = new XmlNamespaceManager(base.webpartDefinition.OwnerDocument.NameTable);
            v2NameSpaceManager.AddNamespace("d", WebPartV2NameSpace);
            v2NameSpaceManager.AddNamespace("m", WebPartV2MembersNameSpace);
        }

        protected override XmlNode GetPropertyValue(string propertyName)
        {
            if (propertyName.Equals("MembershipGroupId", StringComparison.OrdinalIgnoreCase))
            {
                return base.webpartDefinition.SelectSingleNode(string.Format("/d:WebPart/m:{0}", propertyName), v2NameSpaceManager);
            }
            else
            {
                return base.webpartDefinition.SelectSingleNode(string.Format("/d:WebPart/d:{0}", propertyName), v2NameSpaceManager);
            }
        }

        public override bool AddProperty(bool properties, string propertyName, object value)
        {
            if (webPartNode == null)
            {
                webPartNode = base.webpartDefinition.SelectSingleNode("/d:WebPart", v2NameSpaceManager);
            }

            var element = webPartNode.OwnerDocument.CreateElement(propertyName, WebPartV2NameSpace);
            element.InnerText = value.ToString();

            webPartNode.AppendChild(element);

            return true;
        }

        public override bool RemoveAndAddNewProperty(bool properties, string propertyName, object value)
        {
            if (webPartNode == null)
            {
                webPartNode = base.webpartDefinition.SelectSingleNode("/d:WebPart", v2NameSpaceManager);
            }

            XmlNode propertyNode = GetPropertyValue(propertyName);
            if (propertyNode != null)
            {
                webPartNode.RemoveChild(propertyNode);
            }
            return AddProperty(properties, propertyName, value);
        }

        public override string TypeFullName
        {
            get 
            {
                if (typeFullName == null)
                {
                    XmlNode assemblyNode = base.webpartDefinition.SelectSingleNode("d:Assembly", v2NameSpaceManager);
                    XmlNode typeNode = base.webpartDefinition.SelectSingleNode("d:TypeName", v2NameSpaceManager);
                    if (assemblyNode != null && typeNode != null)
                    {
                        typeFullName = typeNode.InnerText + ", " + assemblyNode.InnerText;
                    }
                }
                return typeFullName;
            }
        }

        public override Guid SolutionId
        {
            get 
            {
                return Guid.Empty;
            }
        }
    }
}
