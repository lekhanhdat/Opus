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
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Common
{
    public class AveHtmlUtility
    {
        private const string AttributeSuffix = "=";

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string StripHTML(string strHtml)
        {
            string[] aryReg = { @"<script[^>]*?>.*?</script>", @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>", @"([\r\n])[\s]+", @"&(quot|#34);", @"&(amp|#38);", @"&(lt|#60);", @"&(gt|#62);", @"&(nbsp|#160);", @"&(iexcl|#161);", @"&(cent|#162);", @"&(pound|#163);", @"&(copy|#169);", @"&#(\d+);", @"-->", @"<!--.*\n" };
            string[] aryRep = { "", "", "", "\"", "&", "<", ">", " ", "\xa1", "\xa2", "\xa3", "\xa9", "", "\r\n", "" };
            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            strOutput = strOutput.Replace("<", "");
            strOutput = strOutput.Replace(">", "");
            strOutput = strOutput.Replace("\r\n", "");
            return strOutput;
        }

        public static List<Dictionary<string, string>> CollectAttributeValues(string html, string startElementName, string endElementName, string[] attributeNames)
        {
            List<Dictionary<string, string>> elements = new List<Dictionary<string, string>>();
            int startIndex = html.IndexOf(startElementName, StringComparison.OrdinalIgnoreCase);
            int endIndex = html.IndexOf(endElementName, StringComparison.OrdinalIgnoreCase);

            while (startIndex > 0 && startIndex < endIndex && endIndex <= html.Length)
            {
                int nextElementIndex = html.IndexOf(startElementName, ++startIndex, StringComparison.OrdinalIgnoreCase);
                nextElementIndex = nextElementIndex == -1 ? html.Length : nextElementIndex;
                Dictionary<string, string> element = new Dictionary<string, string>();
                foreach (string attributeName in attributeNames)
                {
                    int attributeIndex = html.IndexOf(attributeName + AttributeSuffix, startIndex, StringComparison.Ordinal);
                    if (attributeIndex >= 0 && attributeIndex <= endIndex)
                    {
                        string attributeValue = null;
                        attributeIndex = ReadAttributeValue(html, attributeIndex, out attributeValue);
                        if (attributeIndex < nextElementIndex)
                        {
                            element.Add(attributeName.Trim(), attributeValue);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                elements.Add(element);
                startIndex = nextElementIndex;
                endIndex = html.IndexOf(endElementName, startIndex, StringComparison.OrdinalIgnoreCase);
            }

            return elements;
        }

        public static Dictionary<string, string> CollectZoneIDAttribute(string html)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string startContent = "<WebPartPages:WebPartZone";
            string endContent = "</WebPartPages:WebPartZone";
            int startIndex = html.IndexOf(startContent, StringComparison.OrdinalIgnoreCase);
            while (startIndex > 0 && startIndex < html.Length)
            {
                int nextIndex = html.IndexOf(endContent, ++startIndex, StringComparison.OrdinalIgnoreCase);
                if (nextIndex < 0)
                {
                    break;
                }
                int attributeIndex = html.IndexOf(" ID" + AttributeSuffix, startIndex, StringComparison.OrdinalIgnoreCase);
                if (attributeIndex < 0)
                {
                    startIndex = html.IndexOf(startContent, ++nextIndex, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                string zoneId = null;
                attributeIndex = ReadAttributeValue(html, attributeIndex, out zoneId);
                attributeIndex = html.IndexOf("__WebPartId" + AttributeSuffix, ++startIndex, StringComparison.OrdinalIgnoreCase);
                while (attributeIndex > startIndex && attributeIndex < nextIndex)
                {
                    string webpartId;
                    int idValueIndex = ReadAttributeValue(html, attributeIndex, out webpartId);
                    if (webpartId.StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(webpartId.Replace('_', '-').Substring(2), zoneId);
                    }
                    else
                    {
                        result.Add(webpartId.Trim('{', '}'), zoneId);
                    }
                    attributeIndex = html.IndexOf("__WebPartId" + AttributeSuffix, ++attributeIndex, StringComparison.OrdinalIgnoreCase);
                }
                startIndex = html.IndexOf(startContent, ++nextIndex, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }

        private static int ReadAttributeValue(string html, int index, out string attributeValue)
        {
            while (html[index] != '"') { index++; };
            int attributeStartIndex = index + 1;
            while (html[++index] != '"') ;
            attributeValue = html.Substring(attributeStartIndex, index - attributeStartIndex);
            return index;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string SimpleEncode(string data)
        {
            char[] ArraySeparator = new char[] { '"' };
            char[] NodeSeparator = new char[] { '<' };
            char[] PropertySeparator = new char[] { '>' };
            char[] SingleQuote = new char[] { '\'' };
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Replace(ArraySeparator[0].ToString(), "&quot;");
                data = data.Replace(NodeSeparator[0].ToString(), "&lt;");
                data = data.Replace(PropertySeparator[0].ToString(), "&gt;");
                data = data.Replace(SingleQuote[0].ToString(), "&squot;");
            }
            return data;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string HtmlDecode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }
            data = data.Replace("&gt;", ">");
            data = data.Replace("&lt;", "<");
            data = data.Replace("&nbsp;", " ");
            data = data.Replace("&quot;", "\"");
            data = data.Replace("&#39;", "\'");
            data = data.Replace("<br/>", "\n");
            return data;
        }

        /// <summary>
        /// Remove html eletments convert to text.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string ConvertHtmlToText(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            try
            {
                string result;
                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = html.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // That's it.
                return result;
            }
            catch
            {
                return html;
            }
        }
    }
}
