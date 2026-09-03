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
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public static class AveProtocolAnalyzer
    {      
        public static AveProtocolHeader AnalyzeHeader(string headerStr)
        {
            AveProtocolHeader protocolHeader = null;

            if (headerStr != null)            
            {
                protocolHeader = new AveProtocolHeader();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(headerStr);
                XmlNode headerNode = doc.FirstChild;
                protocolHeader.Type = Convert.ToChar(GetAttributeValue(headerNode, AveProtocolHeaderConstants.HEADER_ELEMENT_ATTR_TYPE));
                //protocolHeader.Path = headerNode.Attributes[PROTOCOL_HEADER_PATH].Value;
                protocolHeader.WebRelativeUrl = GetAttributeValue(headerNode, AveProtocolHeaderConstants.HEADER_ELEMENT_ATTR_WEB_RELATIVE_URL);
                //protocolHeader.ListTitle = GetAttributeValue(headerNode, PROTOCOL_HEADER_LIST_TITLE);
                protocolHeader.FolderRelativeUrl = GetAttributeValue(headerNode, AveProtocolHeaderConstants.HEADER_ELEMENT_ATTR_FOLDER_RELATIVE_URL);
            }

            return protocolHeader;
        }

        private static string GetAttributeValue(XmlNode node, string attr)
        {
            XmlAttribute attribute = node.Attributes[attr];

            return attribute == null ? null : attribute.Value;
        }

        public static bool IsRestoreEnd(AveProtocolHeader header)
        {
            return header.Type == AveProtocolHeaderConstants.END;
        }

        public static bool IsRestoreReset(AveProtocolHeader header)
        {
            return header.Type == AveProtocolHeaderConstants.RESET;
        }
    }
}
