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
    class XMLUtility
    {
        /// <summary>
        /// Create Child Element
        /// </summary>
        /// <param name="document"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="attributeNames"></param>
        /// <param name="attributeValues"></param>
        /// <returns></returns>
        public static  XmlElement GenerateChildElement(XmlDocument document, string name, string value, List<string> attributeNames, List<string> attributeValues)
        {
            XmlElement element = document.CreateElement(name);
            element.InnerText = value;
            if (attributeNames != null && attributeValues != null)
            {
                for (int index = 0; index < attributeNames.Count; index++)
                {
                    XmlAttribute attribute;
                    if (string.Equals(attributeNames[index], "xsi:nil", StringComparison.OrdinalIgnoreCase))
                    {
                        attribute = document.CreateAttribute("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance");
                    }
                    else
                    {
                        attribute = document.CreateAttribute(attributeNames[index]);
                    }
                    attribute.Value = attributeValues[index];
                    element.Attributes.Append(attribute);
                }
            }
            return element;
        }
    }
}
