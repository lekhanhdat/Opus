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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    internal static class IdcrlUtility
    {
        // Fields
        private const string DummyElementName = "DummyElement";
        private const string DummyElementTag = "<DummyElement>";

        // Methods
        internal static XElement GetElementAtPath(XElement elem, params string[] paths)
        {
            foreach (string str in paths)
            {
                if (elem == null) return null;
                elem = elem.Element(XName.Get(str));
            }
            return elem;
        }

        internal static string GetWebResponseHeader(WebResponse response)
        {
            StringBuilder builder = new StringBuilder();
            if (response != null  && response.Headers != null)
            {
                foreach (string str in response.Headers.AllKeys)
                {
                    if (builder.Length > 0) builder.Append(", ");
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", new object[] { str, response.Headers[str] });
                }
            }
            return builder.ToString();
        }

        public static string XmlValueEncode(string value)
        {
            StringBuilder output = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(output))
            {
                writer.WriteElementString("DummyElement", value);
            }
            string str = output.ToString();
            int startIndex = str.IndexOf("<DummyElement>", StringComparison.Ordinal) + "<DummyElement>".Length;
            int index = str.IndexOf('<', startIndex);
            return str.Substring(startIndex, index - startIndex);
        }
    }
}

