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

namespace OpenNLP.Tools.Util
{
     
    public static class StringUtils
    {
        public static readonly string[] EMPTY_STRING_ARRAY = new string[0];

         
        public static bool Find(string str, string regex)
        {
            return Regex.IsMatch(str, regex);
        }
         
        public static bool ContainsIgnoreCase(List<string> c, string s)
        {
            foreach (string squote in c)
            {
                if (squote.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
         
        public static bool LookingAt(string str, string regex)
        {
            return Regex.IsMatch(str, "^" + regex); 
        }
         
        public static string[] MapStringToArray(string map)
        {
            string[] m = map.Split(new[] {'[', ',', ';', ']'});
            int maxIndex = 0;
            var keys = new string[m.Length];
            var indices = new int[m.Length];
            for (int i = 0; i < m.Length; i++)
            {
                int index = m[i].LastIndexOf('=');
                keys[i] = m[i].Substring(0, index);
                indices[i] = int.Parse(m[i].Substring(index + 1));
                if (indices[i] > maxIndex)
                {
                    maxIndex = indices[i];
                }
            }
            var mapArr = new string[maxIndex + 1];
            //Arrays.fill(mapArr, null);
            for (int i = 0; i < m.Length; i++)
            {
                mapArr[indices[i]] = keys[i];
            }
            return mapArr;
        }
         
        public static Dictionary<string, string> MapStringToMap(string map)
        {
            string[] m = map.Split(new[] {'[', ',', ';', ']'});
            var res = new Dictionary<string, string>();
            foreach (string str in m)
            {
                int index = str.LastIndexOf('=');
                string key = str.Substring(0, index);
                string val = str.Substring(index + 1);
                res.Add(key.Trim(), val.Trim());
            }
            return res;
        }
         
        public static string Pad(string str, int totalChars)
        {
            if (str == null)
            {
                str = "null";
            }
            int slen = str.Length;
            var sb = new StringBuilder(str);
            for (int i = 0; i < totalChars - slen; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }
         
        public static string Pad(Object obj, int totalChars)
        {
            return Pad(obj.ToString(), totalChars);
        }
         
        public static string PadOrTrim(string str, int num)
        {
            if (str == null)
            {
                str = "null";
            }
            int leng = str.Length;
            if (leng < num)
            {
                var sb = new StringBuilder(str);
                for (int i = 0; i < num - leng; i++)
                {
                    sb.Append(' ');
                }
                return sb.ToString();
            }
            else if (leng > num)
            {
                return str.Substring(0, num);
            }
            else
            {
                return str;
            }
        }
         
        public static string PadLeftOrTrim(string str, int num)
        {
            if (str == null)
            {
                str = "null";
            }
            int leng = str.Length;
            if (leng < num)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < num - leng; i++)
                {
                    sb.Append(' ');
                }
                sb.Append(str);
                return sb.ToString();
            }
            else if (leng > num)
            {
                return str.Substring(str.Length - num);
            }
            else
            {
                return str;
            }
        }
         
        public static string PadOrTrim(Object obj, int totalChars)
        {
            return PadOrTrim(obj.ToString(), totalChars);
        }
         
        public static string PadLeft(string str, int totalChars, char ch)
        {
            if (str == null)
            {
                str = "null";
            }
            var sb = new StringBuilder();
            for (int i = 0, num = totalChars - str.Length; i < num; i++)
            {
                sb.Append(ch);
            }
            sb.Append(str);
            return sb.ToString();
        }
         
        public static string PadLeft(string str, int totalChars)
        {
            return PadLeft(str, totalChars, ' ');
        }


        public static string PadLeft(Object obj, int totalChars)
        {
            return PadLeft(obj.ToString(), totalChars);
        }

        public static string PadLeft(int i, int totalChars)
        {
            return PadLeft(i.ToString(), totalChars);
        }

        public static string PadLeft(double d, int totalChars)
        {
            return PadLeft(d.ToString(), totalChars);
        }
         
        public static string Trim(string s, int maxWidth)
        {
            if (s.Length <= maxWidth)
            {
                return (s);
            }
            return (s.Substring(0, maxWidth));
        }

        public static string Trim(Object obj, int maxWidth)
        {
            return Trim(obj.ToString(), maxWidth);
        }

        public static string Repeat(string s, int times)
        {
            if (times == 0)
            {
                return "";
            }
            var sb = new StringBuilder(times*s.Length);
            for (int i = 0; i < times; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static string Repeat(char ch, int times)
        {
            if (times == 0)
            {
                return "";
            }
            var sb = new StringBuilder(times);
            for (int i = 0; i < times; i++)
            {
                sb.Append(ch);
            }
            return sb.ToString();
        }
         
        public static string FileNameClean(string s)
        {
            char[] chars = s.ToCharArray();
            var sb = new StringBuilder();
            foreach (char c in chars)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c == '_'))
                {
                    sb.Append(c);
                }
                else
                {
                    if (c == ' ' || c == '-')
                    {
                        sb.Append('_');
                    }
                    else
                    {
                        sb.Append('x').Append((int) c).Append('x');
                    }
                }
            }
            return sb.ToString();
        }
         
        public static int NthIndex(string s, char ch, int n)
        {
            int index = 0;
            for (int i = 0; i < n; i++)
            {
                // if we're already at the end of the string,
                // and we need to find another ch, return -1
                if (index == s.Length - 1)
                {
                    return -1;
                }
                index = s.IndexOf(ch, index + 1);
                if (index == -1)
                {
                    return (-1);
                }
            }
            return index;
        }
         
        public static string Truncate(int n, int smallestDigit, int biggestDigit)
        {
            int numDigits = biggestDigit - smallestDigit + 1;
            var result = new char[numDigits];
            for (int j = 1; j < smallestDigit; j++)
            {
                n = n/10;
            }
            for (int j = numDigits - 1; j >= 0; j--)
            {
                result[j] = (char) (n%10);
                n = n/10;
            }
            return new string(result);
        }

        public static Dictionary<string, string> StringToProperties(string str)
        {
            var result = new Dictionary<string, string>();
            return StringToProperties(str, result);
        }
         
        public static Dictionary<string, string> StringToProperties(string str, Dictionary<string, string> props)
        {
            string[] propsStr = Regex.Split(str.Trim(), ",\\s*");
            foreach (string term in propsStr)
            {
                int divLoc = term.IndexOf('=');
                string key;
                string value;
                if (divLoc >= 0)
                {
                    key = term.Substring(0, divLoc).Trim();
                    value = term.Substring(divLoc + 1).Trim();
                }
                else
                {
                    key = term.Trim();
                    value = "true";
                }
                props[key] = value;
            }
            return props;
        }

        public static string StripNonAlphaNumerics(string orig)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < orig.Length; i++)
            {
                char c = orig[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        
        public static string EscapeString(string s, char[] charsToEscape, char escapeChar)
        {
            var result = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == escapeChar)
                {
                    result.Append(escapeChar);
                }
                else
                {
                    foreach (char charToEscape in charsToEscape)
                    {
                        if (c == charToEscape)
                        {
                            result.Append(escapeChar);
                            break;
                        }
                    }
                }
                result.Append(c);
            }
            return result.ToString();
        }
         
        public static string[] SplitOnCharWithQuoting(string s, char splitChar, char quoteChar, char escapeChar)
        {
            var result = new List<string>();
            int i = 0;
            int length = s.Length;
            var b = new StringBuilder();
            while (i < length)
            {
                char curr = s[i];
                if (curr == splitChar)
                {
                    // add last buffer
                    // cdm 2014: Do this even if the field is empty!
                    // if (b.Length() > 0) {
                    result.Add(b.ToString());
                    b = new StringBuilder();
                    // }
                    i++;
                }
                else if (curr == quoteChar)
                {
                    // find next instance of quoteChar
                    i++;
                    while (i < length)
                    {
                        curr = s[i];
                        // mrsmith: changed this condition from
                        // if (curr == escapeChar) {
                        if ((curr == escapeChar) && (i + 1 < length) && (s[i + 1] == quoteChar))
                        {
                            b.Append(s[i + 1]);
                            i += 2;
                        }
                        else if (curr == quoteChar)
                        {
                            i++;
                            break; // break this loop
                        }
                        else
                        {
                            b.Append(s[i]);
                            i++;
                        }
                    }
                }
                else
                {
                    b.Append(curr);
                    i++;
                }
            }
            // RFC 4180 disallows readonly comma. At any rate, don't produce a field after it unless non-empty
            if (b.Length > 0)
            {
                result.Add(b.ToString());
            }
            return result.ToArray();
        }
          
        public static int LongestCommonContiguousSubstring(string s, string t)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(t))
            {
                return 0;
            }
            int M = s.Length;
            int N = t.Length;
            var d = new int[M + 1, N + 1];
            for (int j = 0; j <= N; j++)
            {
                d[0, j] = 0;
            }
            for (int i = 0; i <= M; i++)
            {
                d[i, 0] = 0;
            }

            int max = 0;
            for (int i = 1; i <= M; i++)
            {
                for (int j = 1; j <= N; j++)
                {
                    if (s[i - 1] == t[j - 1])
                    {
                        d[i, j] = d[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        d[i, j] = 0;
                    }

                    if (d[i, j] > max)
                    {
                        max = d[i, j];
                    }
                }
            }
            return max;
        }
         
        /// <summary>
        /// Computes the WordNet 2.0 POS tag corresponding to the PTB POS tag s
        /// </summary>
        /// <param name="s">a Penn TreeBank POS tag.</param>
        /// <returns></returns>
        public static string PennPosToWordnetPos(string s)
        {
            if (Regex.IsMatch(s, "NN|NNP|NNS|NNPS"))
            {
                return "noun";
            }
            if (Regex.IsMatch(s, "VB|VBD|VBG|VBN|VBZ|VBP|MD"))
            {
                return "verb";
            }
            if (Regex.IsMatch(s, "JJ|JJR|JJS|CD"))
            {
                return "adjective";
            }
            if (Regex.IsMatch(s, "RB|RBR|RBS|RP|WRB"))
            {
                return "adverb";
            }
            return null;
        }
         
         
        /// <summary>
        /// Uppercases the first character of a string.
        /// </summary>
        /// <param name="s">a string to capitalize</param>
        /// <returns>a capitalized version of the string</returns>
        public static string Capitalize(string s)
        {
            if (char.IsLower(s[0]))
            {
                return char.ToUpper(s[0]) + s.Substring(1);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Check if a string begins with an uppercase.
        /// </summary>
        /// <returns>true if the string is capitalized, false otherwise</returns>
        public static bool IsCapitalized(string s)
        {
            return (char.IsUpper(s[0]));
        }

        public static string SearchAndReplace(string text, string from, string to)
        {
            from = EscapeString(from, new char[] {'.', '[', ']', '\\'}, '\\'); // special chars in regex
            var res = Regex.Replace(text, from, to);
            return res;
        }
        
        /// <summary>
        /// Returns an HTML table containing the matrix of strings passed in.
        /// The first dimension of the matrix should represent the rows, and the second dimension the columns.
        /// </summary>
        public static string MakeHtmlTable(string[][] table, string[] rowLabels, string[] colLabels)
        {
            var buff = new StringBuilder();
            buff.Append("<table class=\"auto\" border=\"1\" cellspacing=\"0\">\n");
            // top row
            buff.Append("<tr>\n");
            buff.Append("<td></td>\n"); // the top left cell
            for (int j = 0; j < table[0].Length; j++)
            {
                // assume table is a rectangular matrix
                buff.Append("<td class=\"label\">").Append(colLabels[j]).Append("</td>\n");
            }
            buff.Append("</tr>\n");
            // all other rows
            for (int i = 0; i < table.Length; i++)
            {
                // one row
                buff.Append("<tr>\n");
                buff.Append("<td class=\"label\">").Append(rowLabels[i]).Append("</td>\n");
                for (int j = 0; j < table[i].Length; j++)
                {
                    buff.Append("<td class=\"data\">");
                    buff.Append(((table[i][j] != null) ? table[i][j] : ""));
                    buff.Append("</td>\n");
                }
                buff.Append("</tr>\n");
            }
            buff.Append("</table>");
            return buff.ToString();
        }
         
        public static string MakeTextTable(Object[][] table, Object[] rowLabels, Object[] colLabels, int padLeft,
            int padRight, bool tsv)
        {
            var buff = new StringBuilder();
            // top row
            buff.Append(MakeAsciiTableCell("", padLeft, padRight, tsv)); // the top left cell
            for (int j = 0; j < table[0].Length; j++)
            {
                // assume table is a rectangular matrix
                buff.Append(MakeAsciiTableCell(colLabels[j], padLeft, padRight, (j != table[0].Length - 1) && tsv));
            }
            buff.Append('\n');
            // all other rows
            for (int i = 0; i < table.Length; i++)
            {
                // one row
                buff.Append(MakeAsciiTableCell(rowLabels[i], padLeft, padRight, tsv));
                for (int j = 0; j < table[i].Length; j++)
                {
                    buff.Append(MakeAsciiTableCell(table[i][j], padLeft, padRight, (j != table[0].Length - 1) && tsv));
                }
                buff.Append('\n');
            }
            return buff.ToString();
        }

        /// <summary>
        /// The cell string is the string representation of the object.
        ///  If padLeft is greater than 0, it is padded. Ditto right
        /// </summary>
        private static string MakeAsciiTableCell(Object obj, int padLeft, int padRight, bool tsv)
        {
            string result = obj.ToString();
            if (padLeft > 0)
            {
                result = StringUtils.PadLeft(result, padLeft);
            }
            if (padRight > 0)
            {
                result = Pad(result, padRight);
            }
            if (tsv)
            {
                result = result + '\t';
            }
            return result;
        }
        
        public static string ToAscii(string s)
        {
            var b = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c > 127)
                {
                    string result = "?";
                    if (c >= 0x00c0 && c <= 0x00c5)
                    {
                        result = "A";
                    }
                    else if (c == 0x00c6)
                    {
                        result = "AE";
                    }
                    else if (c == 0x00c7)
                    {
                        result = "C";
                    }
                    else if (c >= 0x00c8 && c <= 0x00cb)
                    {
                        result = "E";
                    }
                    else if (c >= 0x00cc && c <= 0x00cf)
                    {
                        result = "F";
                    }
                    else if (c == 0x00d0)
                    {
                        result = "D";
                    }
                    else if (c == 0x00d1)
                    {
                        result = "N";
                    }
                    else if (c >= 0x00d2 && c <= 0x00d6)
                    {
                        result = "O";
                    }
                    else if (c == 0x00d7)
                    {
                        result = "x";
                    }
                    else if (c == 0x00d8)
                    {
                        result = "O";
                    }
                    else if (c >= 0x00d9 && c <= 0x00dc)
                    {
                        result = "U";
                    }
                    else if (c == 0x00dd)
                    {
                        result = "Y";
                    }
                    else if (c >= 0x00e0 && c <= 0x00e5)
                    {
                        result = "a";
                    }
                    else if (c == 0x00e6)
                    {
                        result = "ae";
                    }
                    else if (c == 0x00e7)
                    {
                        result = "c";
                    }
                    else if (c >= 0x00e8 && c <= 0x00eb)
                    {
                        result = "e";
                    }
                    else if (c >= 0x00ec && c <= 0x00ef)
                    {
                        result = "i";
                    }
                    else if (c == 0x00f1)
                    {
                        result = "n";
                    }
                    else if (c >= 0x00f2 && c <= 0x00f8)
                    {
                        result = "o";
                    }
                    else if (c >= 0x00f9 && c <= 0x00fc)
                    {
                        result = "u";
                    }
                    else if (c >= 0x00fd && c <= 0x00ff)
                    {
                        result = "y";
                    }
                    else if (c >= 0x2018 && c <= 0x2019)
                    {
                        result = "\'";
                    }
                    else if (c >= 0x201c && c <= 0x201e)
                    {
                        result = "\"";
                    }
                    else if (c >= 0x0213 && c <= 0x2014)
                    {
                        result = "-";
                    }
                    else if (c >= 0x00A2 && c <= 0x00A5)
                    {
                        result = "$";
                    }
                    else if (c == 0x2026)
                    {
                        result = ".";
                    }
                    b.Append(result);
                }
                else
                {
                    b.Append(c);
                }
            }
            return b.ToString();
        }


        public static string ToCSVString(string[] fields)
        {
            var b = new StringBuilder();
            foreach (string fld in fields)
            {
                if (b.Length > 0)
                {
                    b.Append(',');
                }
                var tempFld = (fld.IsNullOrEmpty() ? "" : fld);
                string field = EscapeString(tempFld, new char[] {'\"'}, '\"'); // escape quotes with double quotes
                b.Append('\"').Append(field).Append('\"');
            }
            return b.ToString();
        }
         
        /// <summary>
        /// Returns the supplied string with any trailing '\n' removed.
        /// </summary>
        public static string Chomp(string s)
        {
            if (s.Length == 0)
                return s;
            int l_1 = s.Length - 1;
            if (s[l_1] == '\n')
            {
                return s.Substring(0, l_1);
            }
            return s;
        }
        
        /// <summary>
        /// Returns the result of calling ToString() on the supplied Object, but with any trailing '\n' removed.
        /// </summary>
        public static string Chomp(Object o)
        {
            return Chomp(o.ToString());
        }
         
        /// <summary>
        /// Strip directory from filename.  Like Unix 'basename'. <p/>
        /// Example: <code>getBaseName("/u/wcmac/foo.txt") ==> "foo.txt"</code>
        /// </summary>
        public static string GetBaseName(string fileName)
        {
            return GetBaseName(fileName, "");
        }
        
        /// <summary>
        /// Strip directory and suffix from filename.  Like Unix 'basename'.
        /// Example: <code>getBaseName("/u/wcmac/foo.txt", "") ==> "foo.txt"</code>
        /// Example: <code>getBaseName("/u/wcmac/foo.txt", ".txt") ==> "foo"</code>
        /// Example: <code>getBaseName("/u/wcmac/foo.txt", ".pdf") ==> "foo.txt"</code>
        /// </summary>
        public static string GetBaseName(string fileName, string suffix)
        {
            string[] elts = fileName.Split(new[] {"/"}, StringSplitOptions.None);
            string lastElt = elts[elts.Length - 1];
            if (lastElt.EndsWith(suffix))
            {
                lastElt = lastElt.Substring(0, lastElt.Length - suffix.Length);
            }
            return lastElt;
        }
        
        ///// <summary>
        ///// Given a string the method uses Regex to check if the string only contains alphabet characters
        ///// </summary>
        ///// <param name="s">a string to check using regex</param>
        ///// <returns>true if the string is valid</returns>
        //public static bool IsAlpha(string s)
        //{
        //    /*Pattern p = Pattern.compile("^[\\p{Alpha}\\s]+$");
        //    Matcher m = p.matcher(s);
        //    return m.matches();*/
        //    return Regex.IsMatch(s, "^[\\p{Alpha}\\s]+$");
        //}

        ///// <summary>
        ///// Given a string the method uses Regex to check if the string only contains numeric characters
        ///// </summary>
        ///// <param name="s">a string to check using regex</param>
        ///// <returns>true if the string is valid</returns>
        //public static bool IsNumeric(string s)
        //{
        //    /*Pattern p = Pattern.compile("^[\\p{Digit}\\s\\.]+$");
        //    Matcher m = p.matcher(s);
        //    return m.matches();*/
        //    return Regex.IsMatch(s, "^[\\p{Digit}\\s\\.]+$");
        //}
        
        ///// <summary>
        ///// Given a string the method uses Regex to check 
        ///// if the string only contains alphanumeric characters
        ///// </summary>
        ///// <param name="s">a string to check using regex</param>
        ///// <returns>true if the string is valid</returns>
        //public static bool IsAlphanumeric(string s)
        //{
        //    /*Pattern p = Pattern.compile("^[\\p{Alnum}\\s\\.]+$");
        //    Matcher m = p.matcher(s);
        //    return m.matches();*/
        //    return Regex.IsMatch(s, "^[\\p{Alnum}\\s\\.]+$");
        //}

        ///// <summary>
        ///// Given a string the method uses Regex to check 
        ///// if the string only contains punctuation characters
        ///// </summary>
        ///// <param name="s">a string to check using regex</param>
        ///// <returns>true if the string is valid</returns>
        //public static bool IsPunct(string s)
        //{
        //    /*Pattern p = Pattern.compile("^[\\p{Punct}]+$");
        //    Matcher m = p.matcher(s);
        //    return m.matches();*/
        //    return Regex.IsMatch(s, "^[\\p{Punct}]+$");
        //}

        ///// <summary>
        ///// Given a string the method uses Regex to check if the string looks like an acronym
        ///// </summary>
        ///// <param name="s">a string to check using regex</param>
        ///// <returns>true if the string is valid</returns>
        //public static bool IsAcronym(string s)
        //{
        //    /*Pattern p = Pattern.compile("^[\\p{Upper}]+$");
        //    Matcher m = p.matcher(s);
        //    return m.matches();*/
        //    return Regex.IsMatch(s, "^[\\p{Upper}]+$");
        //}

        public static string GetNotNullString(string s)
        {
            if (s == null)
                return "";
            else
                return s;
        }
         
        /// <summary>
        /// Build a list of character-based ngrams from the given string.
        /// </summary>
        public static List<string> GetCharacterNgrams(string s, int minSize, int maxSize)
        {
            var ngrams = new List<string>();
            int len = s.Length;

            for (int i = 0; i < len; i++)
            {
                for (int ngramSize = minSize;
                    ngramSize > 0 && ngramSize <= maxSize && i + ngramSize <= len;
                    ngramSize++)
                {
                    ngrams.Add(s.Substring(i, i + ngramSize));
                }
            }

            return ngrams;
        }
         
    }
}