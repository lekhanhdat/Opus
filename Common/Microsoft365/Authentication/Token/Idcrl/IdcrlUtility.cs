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
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft365.Authentication.Token.Idclr
{
    internal static class IdcrlUtility
    {


        public static string XmlValueEncode(string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                xmlWriter.WriteElementString("DummyElement", value);
            }
            string text = stringBuilder.ToString();
            int num = text.IndexOf("<DummyElement>", StringComparison.Ordinal) + "<DummyElement>".Length;
            int num2 = text.IndexOf('<', num);
            return text.Substring(num, num2 - num);
        }

        internal static XElement GetElementAtPath(XElement elem, params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                string expandedName = paths[i];
                if (elem == null)
                {
                    return null;
                }
                elem = elem.Element(XName.Get(expandedName));
            }
            return elem;
        }

        internal static string GetWebResponseHeader(WebResponse response)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (response != null && response.SupportsHeaders && response.Headers != null)
            {
                string[] allKeys = response.Headers.AllKeys;
                for (int i = 0; i < allKeys.Length; i++)
                {
                    string text = allKeys[i];
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", new object[]
                    {
                        text,
                        response.Headers[text]
                    });
                }
            }
            return stringBuilder.ToString();
        }
    }
}