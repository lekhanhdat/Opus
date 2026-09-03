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
using System.IO;
using System.Globalization;
using System.Web.UI;
using System.Diagnostics.CodeAnalysis;
namespace AvePoint.Wrapper.Common
{
    public class AveHttpUtility
    {
        //those three are thread safe
        private static readonly ushort[] HTMLCharMap1;
        private static readonly string[] HTMLData;
        private static readonly string[] s_crgstrUrlHexValue;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Http data")]
        static AveHttpUtility()
        {
            HTMLCharMap1 = new ushort[] { 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 1, 0, 0, 0, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 5, 0
            };

            HTMLData = new string[] { "", "&quot;", "&amp;", "&#39;", "&lt;", "&gt;", " ", "<br />", "&#160;", "<b>", "<i>", "<u>", "</b>", "</i>", "</u>", "<wbr />" };

            s_crgstrUrlHexValue = new string[] { 
                "%00", "%01", "%02", "%03", "%04", "%05", "%06", "%07", "%08", "%09", "%0A", "%0B", "%0C", "%0D", "%0E", "%0F", 
                "%10", "%11", "%12", "%13", "%14", "%15", "%16", "%17", "%18", "%19", "%1A", "%1B", "%1C", "%1D", "%1E", "%1F", 
                "%20", "%21", "%22", "%23", "%24", "%25", "%26", "%27", "%28", "%29", "%2A", "%2B", "%2C", "%2D", "%2E", "%2F", 
                "%30", "%31", "%32", "%33", "%34", "%35", "%36", "%37", "%38", "%39", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F", 
                "%40", "%41", "%42", "%43", "%44", "%45", "%46", "%47", "%48", "%49", "%4A", "%4B", "%4C", "%4D", "%4E", "%4F", 
                "%50", "%51", "%52", "%53", "%54", "%55", "%56", "%57", "%58", "%59", "%5A", "%5B", "%5C", "%5D", "%5E", "%5F", 
                "%60", "%61", "%62", "%63", "%64", "%65", "%66", "%67", "%68", "%69", "%6A", "%6B", "%6C", "%6D", "%6E", "%6F", 
                "%70", "%71", "%72", "%73", "%74", "%75", "%76", "%77", "%78", "%79", "%7A", "%7B", "%7C", "%7D", "%7E", "%7F", 
                "%80", "%81", "%82", "%83", "%84", "%85", "%86", "%87", "%88", "%89", "%8A", "%8B", "%8C", "%8D", "%8E", "%8F", 
                "%90", "%91", "%92", "%93", "%94", "%95", "%96", "%97", "%98", "%99", "%9A", "%9B", "%9C", "%9D", "%9E", "%9F", 
                "%A0", "%A1", "%A2", "%A3", "%A4", "%A5", "%A6", "%A7", "%A8", "%A9", "%AA", "%AB", "%AC", "%AD", "%AE", "%AF", 
                "%B0", "%B1", "%B2", "%B3", "%B4", "%B5", "%B6", "%B7", "%B8", "%B9", "%BA", "%BB", "%BC", "%BD", "%BE", "%BF", 
                "%C0", "%C1", "%C2", "%C3", "%C4", "%C5", "%C6", "%C7", "%C8", "%C9", "%CA", "%CB", "%CC", "%CD", "%CE", "%CF", 
                "%D0", "%D1", "%D2", "%D3", "%D4", "%D5", "%D6", "%D7", "%D8", "%D9", "%DA", "%DB", "%DC", "%DD", "%DE", "%DF", 
                "%E0", "%E1", "%E2", "%E3", "%E4", "%E5", "%E6", "%E7", "%E8", "%E9", "%EA", "%EB", "%EC", "%ED", "%EE", "%EF", 
                "%F0", "%F1", "%F2", "%F3", "%F4", "%F5", "%F6", "%F7", "%F8", "%F9", "%FA", "%FB", "%FC", "%FD", "%FE", "%FF"
             };
        }

        public static string ConvertSimpleHtmlToText(string html, int maxLength)
        {
            return HtmlDecodeCore(html, maxLength, null);
        }

        internal static string HtmlDecodeCore(string html, int maxLength, IList<string> tagsToRetain)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            if (maxLength == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            int currentPosition = 0;
            int startIndex = 0;
            while ((currentPosition < html.Length) && ((maxLength < 0) || (builder.Length < maxLength)))
            {
                char ch = html[currentPosition];
                switch (ch)
                {
                    case '&':
                    case '<':
                        {
                            int length = currentPosition - startIndex;
                            bool flag = false;
                            if ((maxLength > -1) && ((builder.Length + length) >= maxLength))
                            {
                                flag = true;
                                length = maxLength - builder.Length;
                            }
                            if (length > 0)
                            {
                                builder.Append(html.Substring(startIndex, length));
                            }
                            if (flag)
                            {
                                goto Label_010B;
                            }
                            break;
                        }
                }
                switch (ch)
                {
                    case '&':
                        {
                            builder.Append(ProceedToEndOfHtmlString(html, ref currentPosition));
                            startIndex = currentPosition;
                            continue;
                        }
                    case '<':
                        {
                            builder.Append(ProceedToEndOfTag(html, tagsToRetain, ref currentPosition));
                            startIndex = currentPosition;
                            continue;
                        }
                }
                currentPosition++;
            }
            if ((maxLength < 0) || ((maxLength - builder.Length) >= (html.Length - startIndex)))
            {
                builder.Append(html.Substring(startIndex));
            }
            else
            {
                int num4 = maxLength - builder.Length;
                if (num4 > 0)
                {
                    builder.Append(html.Substring(startIndex, num4));
                }
            }
        Label_010B:
            return builder.ToString();
        }

        internal static string ProceedToEndOfHtmlString(string html, ref int currentPosition)
        {
            char ch = html[currentPosition];
            int num = currentPosition;
            while ((ch != ';') && (num < (html.Length - 1)))
            {
                ch = html[++num];
            }
            string str = string.Empty;
            switch (html.Substring(currentPosition, (num - currentPosition) + 1))
            {
                case "&quot;":
                    str = "\"";
                    break;

                case "&amp;":
                    str = "&";
                    break;

                case "&#39;":
                    str = "'";
                    break;

                case "&lt;":
                    str = "<";
                    break;

                case "&gt;":
                    str = ">";
                    break;

                case "&#160;":
                    str = " ";
                    break;
            }
            currentPosition = num + 1;
            return str;
        }

        internal static string ProceedToEndOfTag(string html, IList<string> tagsToRetain, ref int currentPosition)
        {
            char ch = html[currentPosition];
            int num = currentPosition;
            while ((ch != '>') && (num < (html.Length - 1)))
            {
                ch = html[++num];
            }
            string str = html.Substring(currentPosition, (num - currentPosition) + 1);
            bool flag = str.EndsWith("/>", StringComparison.Ordinal);
            int index = str.IndexOf(' ');
            if (index == -1)
            {
                index = str.IndexOf('>');
            }
            string item = str.Substring(1, index - 1);
            string targetCloseTag = "</" + item + ">";
            string str4 = string.Empty;
            if (str == HTMLData[7])
            {
                str4 = "\n";
            }
            if ((string.IsNullOrEmpty(str4) && (tagsToRetain != null)) && tagsToRetain.Contains(item))
            {
                if (flag)
                {
                    str4 = str;
                    currentPosition = num + 1;
                    return str4;
                }
                int startIndex = num + 1;
                ProceedToEndOfCloseTag(targetCloseTag, html, ref currentPosition);
                return (str + html.Substring(startIndex, currentPosition - startIndex));
            }
            if (!flag && ((str == "<style>") || str.Contains("display:none")))
            {
                ProceedToEndOfCloseTag(targetCloseTag, html, ref currentPosition);
                return str4;
            }
            currentPosition = num + 1;
            return str4;
        }

        private static void ProceedToEndOfCloseTag(string targetCloseTag, string html, ref int currentPosition)
        {
            int length = targetCloseTag.Length;
            while (currentPosition < (html.Length - 1))
            {
                int num2;
                currentPosition = num2 = currentPosition + 1;
                if (((html[num2] == '<') && ((currentPosition + length) < html.Length)) && targetCloseTag.Equals(html.Substring(currentPosition, length)))
                {
                    currentPosition += targetCloseTag.Length;
                    return;
                }
            }
        }

        public static string HtmlEncode(Guid valueToEncode)
        {
            return valueToEncode.ToString("B").ToUpper(CultureInfo.InvariantCulture);
        }

        public static string HtmlEncode(int valueToEncode)
        {
            return valueToEncode.ToString(CultureInfo.InvariantCulture);
        }

        public static string HtmlEncode(string valueToEncode)
        {
            if ((valueToEncode == null) || (valueToEncode.Length == 0))
            {
                return valueToEncode;
            }
            StringBuilder sb = new StringBuilder(0xff);
            HtmlTextWriter output = new HtmlTextWriter(new StringWriter(sb, CultureInfo.InvariantCulture));
            HtmlEncode(valueToEncode, output);
            return sb.ToString();
        }

        public static void HtmlEncode(string valueToEncode, TextWriter output)
        {
            if (((valueToEncode != null) && (valueToEncode.Length != 0)) && (output != null))
            {
                int startIndex = 0;
                int length = 0;
                int num3 = valueToEncode.Length;
                for (int i = 0; i < num3; i++)
                {
                    int num5;
                    int index = valueToEncode[i];
                    if (index < 0x3f)
                    {
                        num5 = HTMLCharMap1[index];
                    }
                    else
                    {
                        num5 = 0;
                    }
                    if (num5 != 0)
                    {
                        if (length > 0)
                        {
                            output.Write(valueToEncode.Substring(startIndex, length));
                            length = 0;
                        }
                        startIndex = i + 1;
                        output.Write(HTMLData[num5]);
                    }
                    else
                    {
                        length++;
                    }
                }
                if (startIndex < num3)
                {
                    output.Write(valueToEncode.Substring(startIndex));
                }
            }
        }

        public static string HtmlUrlAttributeEncode(string urlAttributeToEncode)
        {
            if ((urlAttributeToEncode == null) || (urlAttributeToEncode.Length == 0))
            {
                return urlAttributeToEncode;
            }
            if (!AveUrlUtility.IsProtocolAllowed(urlAttributeToEncode))
            {
                return string.Empty;
            }
            return HtmlEncode(urlAttributeToEncode);
        }

        public static string NoEncode(object valueToEncode)
        {
            if (valueToEncode != null)
            {
                return valueToEncode.ToString();
            }
            return null;
        }

        public static string NoEncode(string valueToEncode)
        {
            return valueToEncode;
        }

        public static void NoEncode(object valueToEncode, TextWriter output)
        {
            if (valueToEncode != null)
            {
                while (output != null)
                {
                    output.Write(valueToEncode.ToString());
                    break;
                }
            }
        }

        private static void UrlEncodeUnicodeChar(TextWriter output, char ch, char chNext, out bool fUsedNextChar)
        {
            bool fInvalidUnicode = false;
            UrlEncodeUnicodeChar(output, ch, chNext, ref fInvalidUnicode, out fUsedNextChar);
        }

        private static void UrlEncodeUnicodeChar(TextWriter output, char ch, char chNext, ref bool fInvalidUnicode, out bool fUsedNextChar)
        {
            int num = 0xc0;
            int num2 = 0xe0;
            int num3 = 240;
            int num4 = 0x80;
            int num5 = 0xd800;
            int num6 = 0xfc00;
            int num7 = 0x10000;
            fUsedNextChar = false;
            int index = ch;
            if (index <= 0x7f)
            {
                output.Write(s_crgstrUrlHexValue[index]);
            }
            else
            {
                int num8;
                if (index <= 0x7ff)
                {
                    num8 = num | (index >> 6);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | (index & 0x3f);
                    output.Write(s_crgstrUrlHexValue[num8]);
                }
                else if ((index & num6) != num5)
                {
                    num8 = num2 | (index >> 12);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | ((index & 0xfc0) >> 6);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | (index & 0x3f);
                    output.Write(s_crgstrUrlHexValue[num8]);
                }
                else if (chNext != '\0')
                {
                    index = (index & 0x3ff) << 10;
                    fUsedNextChar = true;
                    index |= chNext & (char)0x3ff;
                    index += num7;
                    num8 = num3 | (index >> 0x12);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | ((index & 0x3f000) >> 12);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | ((index & 0xfc0) >> 6);
                    output.Write(s_crgstrUrlHexValue[num8]);
                    num8 = num4 | (index & 0x3f);
                    output.Write(s_crgstrUrlHexValue[num8]);
                }
                else
                {
                    fInvalidUnicode = true;
                }
            }
        }

        public static string UrlKeyValueEncode(Guid guidKeyOrValueToEncode)
        {
            return guidKeyOrValueToEncode.ToString("B").ToUpper(CultureInfo.InvariantCulture);
        }

        public static string UrlKeyValueEncode(int keyOrValueToEncode)
        {
            return keyOrValueToEncode.ToString(CultureInfo.InvariantCulture);
        }

        public static string UrlKeyValueEncode(string keyOrValueToEncode)
        {
            if ((keyOrValueToEncode == null) || (keyOrValueToEncode.Length == 0))
            {
                return keyOrValueToEncode;
            }
            StringBuilder sb = new StringBuilder(0xff);
            HtmlTextWriter output = new HtmlTextWriter(new StringWriter(sb, CultureInfo.InvariantCulture));
            UrlKeyValueEncode(keyOrValueToEncode, output);
            return sb.ToString();
        }

        public static void UrlKeyValueEncode(string keyOrValueToEncode, TextWriter output)
        {
            if (((keyOrValueToEncode != null) && (keyOrValueToEncode.Length != 0)) && (output != null))
            {
                bool fUsedNextChar = false;
                int startIndex = 0;
                int length = 0;
                int num3 = keyOrValueToEncode.Length;
                for (int i = 0; i < num3; i++)
                {
                    char ch = keyOrValueToEncode[i];
                    if (((('0' <= ch) && (ch <= '9')) || (('a' <= ch) && (ch <= 'z'))) || (('A' <= ch) && (ch <= 'Z')))
                    {
                        length++;
                    }
                    else
                    {
                        if (length > 0)
                        {
                            output.Write(keyOrValueToEncode.Substring(startIndex, length));
                            length = 0;
                        }
                        UrlEncodeUnicodeChar(output, keyOrValueToEncode[i], (i < (num3 - 1)) ? keyOrValueToEncode[i + 1] : '\0', out fUsedNextChar);
                        if (fUsedNextChar)
                        {
                            i++;
                        }
                        startIndex = i + 1;
                    }
                }
                if ((startIndex < num3) && (output != null))
                {
                    output.Write(keyOrValueToEncode.Substring(startIndex));
                }
            }
        }

        public static string UrlKeyValueEncode(string keyToEncode, string valueToEncode)
        {
            return (UrlKeyValueEncode(keyToEncode) + "=" + UrlKeyValueEncode(valueToEncode));
        }

        public static void UrlKeyValueEncode(string keyToEncode, string valueToEncode, TextWriter output)
        {
            UrlKeyValueEncode(keyToEncode, output);
            output.Write("=");
            UrlKeyValueEncode(valueToEncode, output);
        }

        public static string UrlPathEncode(string urlToEncode, bool allowHashParameter)
        {
            return UrlPathEncode(urlToEncode, allowHashParameter, false);
        }

        public static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters)
        {
            bool invalidUnicode = false;
            return UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, ref invalidUnicode);
        }

        internal static string UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, ref bool invalidUnicode)
        {
            if ((urlToEncode == null) || (urlToEncode.Length == 0))
            {
                return urlToEncode;
            }
            StringBuilder sb = new StringBuilder(0xff);
            HtmlTextWriter output = new HtmlTextWriter(new StringWriter(sb, CultureInfo.InvariantCulture));
            UrlPathEncode(urlToEncode, allowHashParameter, encodeUnicodeCharacters, output, ref invalidUnicode);
            return sb.ToString();
        }

        private static void UrlPathEncode(string urlToEncode, bool allowHashParameter, bool encodeUnicodeCharacters, TextWriter output, ref bool invalidUnicode)
        {
            if (((urlToEncode != null) && (urlToEncode.Length != 0)) && (output != null))
            {
                bool fUsedNextChar = false;
                int num = 0;
                int length = urlToEncode.Length;
                while ((num < length) && (urlToEncode[num] == ' '))
                {
                    num++;
                }
                int startIndex = num;
                int num4 = 0;
                while (num < length)
                {
                    char ch = urlToEncode[num];
                    if ((ch == '?') || (allowHashParameter && (ch == '#')))
                    {
                        break;
                    }
                    if (((((ch & 0xffe0) == 0) || (ch == ' ')) || ((ch == '"') || (ch == '#'))) || (((ch == '%') || (ch == '<')) || (((ch == '>') || (ch == '\'')) || (ch == '&'))))
                    {
                        if (num4 > 0)
                        {
                            output.Write(urlToEncode.Substring(startIndex, num4));
                            num4 = 0;
                        }
                        startIndex = num + 1;
                        int num5 = ch & '\x00ff';
                        if (num5 < 0x10)
                        {
                            output.Write("%0");
                            output.Write(num5.ToString("X", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            output.Write('%');
                            output.Write(num5.ToString("X", CultureInfo.InvariantCulture));
                        }
                    }
                    else if (encodeUnicodeCharacters && (ch > '\x007f'))
                    {
                        if (num4 > 0)
                        {
                            output.Write(urlToEncode.Substring(startIndex, num4));
                            num4 = 0;
                        }
                        UrlEncodeUnicodeChar(output, urlToEncode[num], (num < (length - 1)) ? urlToEncode[num + 1] : '\0', ref invalidUnicode, out fUsedNextChar);
                        if (fUsedNextChar)
                        {
                            num++;
                        }
                        startIndex = num + 1;
                    }
                    else
                    {
                        num4++;
                    }
                    num++;
                }
                if (startIndex < length)
                {
                    output.Write(urlToEncode.Substring(startIndex));
                }
            }
        }

        public static string UrlPathDecode(string urlToDecode, bool allowHashParameter)
        {
            if (string.IsNullOrEmpty(urlToDecode)) return urlToDecode;
            int length = urlToDecode.Length;
            int index = urlToDecode.IndexOf('?');
            if (index == -1) index = length;
            if (allowHashParameter)
            {
                int num3 = urlToDecode.IndexOf('#');
                if (num3 != -1 && num3 < index) index = num3;
            }
            return UrlDecodeHelper(urlToDecode, index, false);
        }

        private static string UrlDecodeHelper(string stringToDecode, int length, bool decodePlus)
        {
            if (stringToDecode == null || stringToDecode.Length == 0) return stringToDecode;
            StringBuilder builder = new StringBuilder(length);
            byte[] bytes = null;
            int nIndex = 0;
            while (nIndex < length)
            {
                char ch = stringToDecode[nIndex];
                if (ch < ' ')
                    nIndex++;
                else
                {
                    if (decodePlus && ch == '+')
                    {
                        builder.Append(" ");
                        nIndex++;
                        continue;
                    }
                    if (IsHexEscapedChar(stringToDecode, nIndex, length))
                    {
                        if (bytes == null) bytes = new byte[(length - nIndex) / 3];
                        int count = 0;
                        do
                        {
                            int num3 = FromHexNoCheck(stringToDecode[nIndex + 1]) * 0x10 + FromHexNoCheck(stringToDecode[nIndex + 2]);
                            bytes[count++] = (byte)num3;
                            nIndex += 3;
                        }
                        while (IsHexEscapedChar(stringToDecode, nIndex, length));
                        builder.Append(Encoding.UTF8.GetChars(bytes, 0, count));
                        continue;
                    }
                    builder.Append(ch);
                    nIndex++;
                }
            }
            if (length < stringToDecode.Length) builder.Append(stringToDecode.Substring(length));
            return builder.ToString();
        }

        private static bool IsHexEscapedChar(string str, int nIndex, int nPathLength)
        {
            if (nIndex + 2 >= nPathLength || str[nIndex] != '%' || !IsHexDigit(str[nIndex + 1]) || !IsHexDigit(str[nIndex + 2])) return false;
            if (str[nIndex + 1] == '0') return (str[nIndex + 2] != '0');
            return true;
        }

        private static int FromHexNoCheck(char digit)
        {
            if (digit <= '9') return (digit - '0');
            if (digit <= 'F') return (digit - 'A' + 10);
            return (digit - 'a' + 10);
        }

        private static bool IsHexDigit(char digit)
        {
            if (('0' > digit || digit > '9') && ('a' > digit || digit > 'f')) return ('A' <= digit && digit <= 'F');
            return true;
        }

    }
}