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
    public class WebPartExtractorFactory
    {
        public static IWebPartPropertyExtractor Create(string webpartDefinitionXml, string defaultNameSpace = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webpartDefinitionXml);

            return Create(doc, defaultNameSpace);
        }
        public static IWebPartPropertyExtractor Create(XmlDocument document, string defaultNameSpace = "")
        {
            XmlElement webpartNode = TrySelectV2WebPartNode(document);
            if (webpartNode != null)
            {
                return new V2WebPartPropertyExtractor(webpartNode);
            }

            webpartNode = TrySelectV3WebPartNode(document, defaultNameSpace);
            if (webpartNode != null)
            {
                return new V3WebPartPropertyExtractor(webpartNode);
            }

            return null;
        }
        private static XmlElement TrySelectV2WebPartNode(XmlDocument document)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("default", V2WebPartPropertyExtractor.WebPartV2NameSpace);
            return document.SelectSingleNode("default:WebPart", nsmgr) as XmlElement;
        }

        private static XmlElement TrySelectV3WebPartNode(XmlDocument document, string defaultNamespace = "")
        {
            string namespacePrefix = string.Empty;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("default", V3WebPartPropertyExtractor.WebPartV3NameSpace);
            if (!string.IsNullOrEmpty(defaultNamespace))
            {
                nsmgr.AddNamespace("sp", defaultNamespace);
                namespacePrefix = "sp:";
            }
            return document.SelectSingleNode(namespacePrefix + "webParts/default:webPart", nsmgr) as XmlElement;
        }
    }
}
