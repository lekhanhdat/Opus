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
    class NintexWFMetadataProcessor
    {
        public NintexWFMetadataProcessor()
        { }

        public byte[] GetWorkflowMetadataContent(string ninTexWFType, Guid scopeId, string webId, string customerId)
        {
            XmlDocument document = new XmlDocument();
            XmlElement root = XMLUtility.GenerateChildElement(document, "WorkflowMetaData", string.Empty
                , new List<string> { "xmlns:xsd", "xmlns:xsi" }
                , new List<string> { "http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance" });
            root.AppendChild(XMLUtility.GenerateChildElement(document, "WorkflowId", string.Empty, null, null));
            root.AppendChild(XMLUtility.GenerateChildElement(document, "SubscriptionId", string.Empty, null, null));
            root.AppendChild(XMLUtility.GenerateChildElement(document, "WorkflowType", ninTexWFType.ToString(), null, null));
            root.AppendChild(XMLUtility.GenerateChildElement(document, "AppId", string.Format("{0}@{1}", webId, customerId), null, null));  //在开发Set item permission中发现如果不设置AppId和CustomerId,password无论是否有值，到目的端都会显示空
            root.AppendChild(XMLUtility.GenerateChildElement(document, "CustomerId", customerId, null, null));
            if (ninTexWFType == "List")
            {
                root.AppendChild(XMLUtility.GenerateChildElement(document, "ScopeId", scopeId.ToString(), null, null));
            }
            else
            {
                root.AppendChild(XMLUtility.GenerateChildElement(document, "ScopeId", string.Empty, new List<string>() { "xsi:nil" }, new List<string>() { "true" }));
            }
            document.AppendChild(root);
            return Encoding.Default.GetBytes(document.InnerXml);
        }
    }


}
