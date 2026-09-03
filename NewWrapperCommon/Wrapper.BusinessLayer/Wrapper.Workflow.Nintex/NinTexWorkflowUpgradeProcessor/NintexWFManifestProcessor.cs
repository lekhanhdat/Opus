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

namespace LS.SPWorkflowProcessor
{
    class NintexWFManifestProcessor
    {
        public byte[] GetWorkflowManifestContent(DateTime createdTime, bool needVariables, bool needLists)
        {
            XmlDocument document = new XmlDocument();
            XmlElement root = XMLUtility.GenerateChildElement(document, "Manifest", string.Empty
                , new List<string> { "version", }
                , new List<string> { "2.0.3", });
            root.AppendChild(GenerateContentsElement(document, needVariables, needLists));
            root.AppendChild(GeneratePropertiesElement(document, createdTime));
            document.AppendChild(root);
            return Encoding.Default.GetBytes(document.InnerXml);
        }

        private XmlElement GeneratePropertiesElement(XmlDocument document, DateTime createdTime)
        {
            var propertiesElement = XMLUtility.GenerateChildElement(document, "Properties", string.Empty, null, null);
            propertiesElement.AppendChild(XMLUtility.GenerateChildElement(document, "Property", createdTime.ToString("O")
                   , new List<string>() { "name" }
                   , new List<string>() { "Created" }));
            return propertiesElement;
        }

        private XmlElement GenerateContentsElement(XmlDocument document, bool needVariables, bool needLists)
        {
            var contentsElement = XMLUtility.GenerateChildElement(document, "Contents", string.Empty, null, null);
            contentsElement.AppendChild(XMLUtility.GenerateChildElement(document, "File", null
                   , new List<string>() { "hash", "path", "name" }
                   , new List<string>() { "", @"Workflow\Actions.xml", "Actions" }));
            contentsElement.AppendChild(XMLUtility.GenerateChildElement(document, "File", null
                  , new List<string>() { "hash", "path", "name" }
                  , new List<string>() { "", @"Workflow\Settings.xml", "Settings" }));
            contentsElement.AppendChild(XMLUtility.GenerateChildElement(document, "File", null
                  , new List<string>() { "hash", "path", "name" }
                  , new List<string>() { "", @"Workflow\Metadata.xml", "Metadata" }));
            if (needVariables)
            {
                contentsElement.AppendChild(XMLUtility.GenerateChildElement(document, "File", null
                 , new List<string>() { "hash", "path", "name" }
                 , new List<string>() { "", @"Workflow\Variables.xml", "Variables" }));
            }

            if (needLists)
            {
                contentsElement.AppendChild(XMLUtility.GenerateChildElement(document, "File", null
                 , new List<string>() { "hash", "path", "name" }
                 , new List<string>() { "", @"Lists.xml", "Lists" }));
            }

            return contentsElement;

        }
    }
}
