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
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveWebPartDefinitionXmlUtility
    {
        private static AveLogger logger = new AveLogger(typeof (AveWebPartDefinitionXmlUtility));

        /// <summary>
        /// 从WebPartDefinitionXml中解析出来View的viewFields等setting信息
        /// </summary>
        /// <param name="definitionXml"></param>
        /// <returns></returns>
        public static byte[] GetViewSetting(string definitionXml)
        {
            var definitionXmlDocument = LoadDefinitionXml(definitionXml);
            var xmlDefinition = GetPropertyValue(definitionXmlDocument, "XmlDefinition");
            if (!string.IsNullOrEmpty(xmlDefinition))
            {
                var xmlDefinitionNode = LoadDefinitionXml(System.Web.HttpUtility.HtmlDecode(xmlDefinition));
                if (xmlDefinitionNode.DocumentElement != null)
                {
                    var viewSettings = xmlDefinitionNode.DocumentElement.OuterXml;
                    return AveCompressedUtility.GetTCompressedBytes(viewSettings);
                }
            }
            return null;
        }

        private static XmlDocument LoadDefinitionXml(string definitionXml)
        {
            var definitionXmlDocument = new XmlDocument();
            definitionXmlDocument.LoadXml(definitionXml);
            return definitionXmlDocument;
        }

        private static string GetPropertyValue(XmlNode definitionDocument, string propertyName)
        {
            if (definitionDocument == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            var definitionNode = definitionDocument.SelectSingleNode("//*[@name = 'XmlDefinition']")
                                    ?? definitionDocument.SelectSingleNode("//*[name() = 'ListViewXml']");
            return definitionNode != null ? definitionNode.InnerXml : string.Empty;
        }
        public static void RetrieveWebPartAssemblyInfo(string definition, ref string assemblyString, ref string typeNameString)
        {
            try
            {
                if(string.IsNullOrEmpty(definition))
                {
                    return;
                }
                var acquirer = AveWebPartTypeAcquirer.CreateAcquirerInstance(definition);
                acquirer.GetWebpartTypeName(ref assemblyString, ref typeNameString);
            }
            catch(Exception e)
            {
                logger.Warn("Get Webpart assembly info failed. Definition: {0}, Error: {1}", definition, e);
            }
        }
        abstract class AveWebPartTypeAcquirer
        {
            static string v2NameSpace = "http://schemas.microsoft.com/WebPart/v2";
            protected XmlNode WebPartNode { private set; get; }
            public AveWebPartTypeAcquirer(XmlNode webPartNode)
            {
                this.WebPartNode = webPartNode;
            }
            public static AveWebPartTypeAcquirer CreateAcquirerInstance(string webPartDefinition)
            {
                var xd = new XmlDocument();
                xd.LoadXml(webPartDefinition);
                var webPartNode = xd.FirstChild;
                if (string.IsNullOrEmpty(webPartNode.NamespaceURI))
                {
                    webPartNode = webPartNode.FirstChild;
                }
                if (webPartNode.NamespaceURI.Equals(v2NameSpace, StringComparison.OrdinalIgnoreCase))
                {
                    return new AveV2WebPartTypeAcquirer(webPartNode);
                }
                return new AveV3WebPartTypeAcquirer(webPartNode);
            }
            public abstract void GetWebpartTypeName(ref string assemblyString, ref string typeNameString);
        }
        class AveV2WebPartTypeAcquirer : AveWebPartTypeAcquirer
        {
            public AveV2WebPartTypeAcquirer(XmlNode webPartNode) 
                : base(webPartNode)
            {
            }

            public override void GetWebpartTypeName(ref string assemblyString, ref string typeNameString)
            {
                XmlNode assemblyNode = WebPartNode.SelectSingleNode("//*[name() = 'Assembly']");
                XmlNode typeNameNode = WebPartNode.SelectSingleNode("//*[name() = 'TypeName']");
                if (assemblyNode != null && !string.IsNullOrEmpty(assemblyNode.InnerText)
                    && typeNameNode != null && !string.IsNullOrEmpty(typeNameNode.InnerText))
                {
                    assemblyString = assemblyNode.InnerText;
                    typeNameString = typeNameNode.InnerText;
                }
            }
        }
        class AveV3WebPartTypeAcquirer : AveWebPartTypeAcquirer
        {
            public AveV3WebPartTypeAcquirer(XmlNode webPartNode) 
                : base(webPartNode)
            {
            }

            public override void GetWebpartTypeName(ref string assemblyString, ref string typeNameString)
            {
                XmlNode assemblyNode = WebPartNode.SelectSingleNode("//*[name() = 'type']");
                if (assemblyNode != null && assemblyNode.Attributes["name"] != null && !string.IsNullOrEmpty(assemblyNode.Attributes["name"].Value))
                {
                    var webPartTypeString = assemblyNode.Attributes["name"].Value;
                    var index = webPartTypeString.IndexOf(',');
                    if (index > 0)
                    {
                        typeNameString = webPartTypeString.Substring(0, index);
                        assemblyString = webPartTypeString.Substring(index + 1, webPartTypeString.Length - typeNameString.Length - 1);
                    }
                }
            }
        }
    }
}
