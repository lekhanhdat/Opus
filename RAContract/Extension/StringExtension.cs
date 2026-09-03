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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.SharePoint.Client.Extension
{
    public static class StringExtension
    {
        public static string EncodeAmpersandInHref(this string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                return xmlContent;
            }

            // This regex finds href attributes in anchor tags and captures their values.
            // It looks for href="...", handling potential whitespace around the equals sign.
            string pattern = @"(<a\s+[^>]*?href\s*=\s*"")(.*?)(?=""[^>]*?>)";

            return Regex.Replace(xmlContent, pattern, m =>
            {
                // The first group captures everything before the href value (e.g., <a href=").
                // The second group captures the href value itself.
                string hrefValue = m.Groups[2].Value;

                // Replace '&' with '%26' only in the href value.
                // We also need to handle existing '&amp;' which is the XML-encoded version of '&'.
                // This prevents double-encoding if the input is already partially escaped.
                string encodedHrefValue = hrefValue.Replace("&amp;", "&").Replace("&", "%26");

                // Reconstruct the attribute with the encoded value.
                return m.Groups[1].Value + encodedHrefValue;
            });
        }
    }
}
