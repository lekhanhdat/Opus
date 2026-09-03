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



namespace  AvePoint.Hybrid.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Xml;
    #endregion

    public sealed class XmlAnalyserHelper
    {
        public static XmlNode GetFirstNonDeclarationNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("node");
            }
            foreach (XmlNode child in doc.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    return child;
                }
            }
            return null;
        }

        public static XmlNode GetSingleChildNodeByLocalName(XmlNode parentNode, string localName)
        {
            if (parentNode == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(localName))
            {
                throw new ArgumentNullException("localName");
            }
            XmlNode retValue = null;
            foreach (XmlNode child in parentNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) { continue; }
                if (string.Compare(child.LocalName, localName, StringComparison.Ordinal) == 0)
                {
                    retValue = child;
                    break;
                }
            }
            return retValue;
        }

        public static IEnumerable<XmlNode> GetChildNodesByLocalName(XmlNode parentNode, string localName)
        {
            List<XmlNode> nodes = new List<XmlNode>();
            if (parentNode == null)
            {
                return nodes;
            }
            if (string.IsNullOrEmpty(localName))
            {
                throw new ArgumentNullException("localName");
            }
            foreach (XmlNode child in parentNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) { continue; }
                if (string.Compare(child.LocalName, localName, StringComparison.Ordinal) == 0)
                {
                    nodes.Add(child);
                }
            }

            return nodes;
        }

        public static XmlNode GetComponentsNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            foreach (XmlNode child in doc.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) { continue; }
                if (string.Compare(child.LocalName, "castle", StringComparison.Ordinal) == 0)
                {
                    foreach (XmlNode childOfCastle in child.ChildNodes)
                    {
                        if (childOfCastle.NodeType != XmlNodeType.Element) { continue; }
                        if (string.Compare(childOfCastle.LocalName, "components", StringComparison.Ordinal) == 0)
                        {
                            return childOfCastle;
                        }
                    }
                    break;
                }
            }
            return null;
        }
    }
}
