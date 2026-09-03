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
using System.IO;
using System.Globalization;
using System.Text;

using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
namespace LS
{
    public class LSUtilityOfBytes
    {
        //String Operations

        public static string LSReplaceStringIgnoreCase(string source, string oldValue, string newValue, int replacementCount, out int replacedCount)
        {
            string str = source;
            replacedCount = 0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue) || replacementCount <= 0)
            {
                return source;
            }
            str = LSReplaceInternal(source, oldValue, newValue, replacementCount, out replacedCount);
            return str;
        }

        private static string LSReplaceInternal(string source, string oldValue, string newValue, int replacementCount, out int replacedCount)
        {
            int length = source.Length;
            int oldValueLen = oldValue.Length;
            int offset = 0;
            StringBuilder builder = new StringBuilder(length);

            replacedCount = 0;
            while (offset < length)
            {

                if (replacedCount == replacementCount)
                {
                    builder.Append(source.Substring(offset));
                    break;
                }

                int curIndex = source.IndexOf(oldValue, offset, StringComparison.OrdinalIgnoreCase);
                if (curIndex < 0)
                {
                    builder.Append(source.Substring(offset));
                    break;
                }
                builder.Append(source.Substring(offset, curIndex - offset));
                builder.Append(newValue);
                replacedCount++;
                offset = curIndex + oldValueLen;
            }
            return builder.ToString();
        }



        /*
        public static string LSReplaceStringIgnoreCase(string original, string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern, position0)) != -1)
            {
                for (int i = position0; i < position1; ++i) chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i) chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i) chars[count++] = original[i];
            return new string(chars, 0, count);
        }
        */

        public static string LSReplaceString(string source, string find, string replacement, StringComparison comparisonTyep, int replacementCount, ref int replacedCount)
        {
            StringBuilder result = new StringBuilder();
            string curString = source;
            replacedCount = 0;
            while (true)
            {
                int index = curString.IndexOf(find, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    if (replacedCount >= replacementCount)
                    {
                        result.Append(curString);
                        break;
                    }
                    else
                    {
                        result.Append(curString.Substring(0, index));
                        result.Append(replacement);
                        replacedCount++;
                        if (index + find.Length >= curString.Length)
                            break;
                        else
                            curString = curString.Substring(index + find.Length);
                    }
                }
                else
                {
                    result.Append(curString);
                    break;
                }
            }
            return result.ToString();
        }

        public static bool LSHasNumber(string source)
        {
            byte[] temp = Encoding.UTF8.GetBytes(source);
            return LSHasNumber(temp);
        }


        //Byte[] Operations
        private static int GetNextIndex(byte[] content, int startIndex)
        {
            int i = -1;
            int j = -1;
            byte[] endFlag1 = Encoding.UTF8.GetBytes("\n");
            byte[] endFlag2 = Encoding.UTF8.GetBytes("\r");
            i = LSIndexOfBytes(content, endFlag1, StringComparison.OrdinalIgnoreCase, startIndex);
            j = LSIndexOfBytes(content, endFlag2, StringComparison.OrdinalIgnoreCase, startIndex);
            if (i == -1)
            {
                return j;
            }
            else if (j == -1)
            {
                return i;
            }
            else
            {
                return (i < j ? i : j);
            }
        }

        private static Dictionary<int, int> MakeSkipDictionary(byte[] pattern, StringComparison comparison)
        {

            Dictionary<int, int> dic = new Dictionary<int, int>(256);
            if (pattern != null && pattern.Length > 0)
            {
                int len = pattern.Length;
                for (int i = 0; i < 256; i++)
                {
                    dic.Add(i, len);
                }

                for (int j = 0; j < len; j++)
                {
                    int realLen = 0;
                    byte a = pattern[j];
                    if (j == len - 1)
                        realLen = len;
                    else
                        realLen = len - j - 1;

                    dic[a] = realLen;
                    if (comparison == StringComparison.CurrentCultureIgnoreCase || comparison == StringComparison.InvariantCultureIgnoreCase || comparison == StringComparison.OrdinalIgnoreCase)
                    {
                        if (a >= 65 && a <= 90)
                            dic[a + 32] = realLen;
                        if (a >= 97 && a <= 122)
                            dic[a - 32] = realLen;
                    }
                }
            }
            return dic;
        }

        private static Dictionary<int, int> MakeSkipDictionary2(byte[] pattern, StringComparison comparison)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>(256);
            if (pattern != null && pattern.Length > 0)
            {
                int len = pattern.Length;
                for (int i = 0; i < 256; i++)
                {
                    dic.Add(i, len + 1);
                }

                for (int j = 0; j < len; j++)
                {
                    byte a = pattern[j];
                    dic[a] = len - j;
                    if (comparison == StringComparison.CurrentCultureIgnoreCase || comparison == StringComparison.InvariantCultureIgnoreCase || comparison == StringComparison.OrdinalIgnoreCase)
                    {
                        if (a >= 65 && a <= 90)
                            dic[a + 32] = len - j;
                        if (a >= 97 && a <= 122)
                            dic[a - 32] = len - j;
                    }
                }
            }
            return dic;
        }

        private static bool CheckIfEqual(byte a, byte b, StringComparison comparison)
        {
            bool result = false;
            result = (a == b);

            if (comparison == StringComparison.CurrentCultureIgnoreCase || comparison == StringComparison.InvariantCultureIgnoreCase || comparison == StringComparison.OrdinalIgnoreCase)
            {
                if (!result)
                {
                    if (a >= 65 && a <= 90)
                        result = (a + 32 == b);
                }
                if (!result)
                {
                    if (a >= 97 && a <= 122)
                        result = (a - 32 == b);
                }
            }
            return result;

        }

        public static int LSIndexOfBytes(byte[] source, byte[] pattern, StringComparison comparison)
        {
            return LSIndexOfBytesNative(source, pattern, comparison, 0);
        }

        public static int LSIndexOfBytes(byte[] source, byte[] pattern, StringComparison comparison, int startIndex)
        {
            return LSIndexOfBytesNative(source, pattern, comparison, startIndex);
        }

        private static int LSIndexOfBytesNative(byte[] source, byte[] pattern, StringComparison comparison, int startIndex)
        {
            #region Algorithm I
            //int index = startIndex;
            //int offset = pattern.Length - 1;
            //int offset2 = 0;

            //int sLen = source.Length;
            //int pLen = pattern.Length;
            //Dictionary<int, int> skipDic = MakeSkipDictionary(pattern, comparison);

            //while (index + pLen <= sLen)
            //{
            //    if (CheckIfEqual(source[index + offset], pattern[offset], comparison))
            //    {
            //        if (offset == 0)
            //        {
            //            return index;
            //        }
            //        offset--;
            //        offset2++;
            //    }
            //    else
            //    {
            //        int temp = skipDic[source[index + offset]] - offset2;
            //        index += temp;
            //        offset = pLen - 1;
            //        offset2 = 0;
            //    }
            //}

            //return -1;
            #endregion

            #region Algorithm II
            int index = startIndex;
            int patternLen = pattern.Length;
            int sourceLen = source.Length;

            Dictionary<int, int> skipDic = MakeSkipDictionary2(pattern, comparison);
            while (true)
            {
                int i = 0;
                for (i = 0; i < patternLen; i++)
                {
                    if (!CheckIfEqual(source[index + i], pattern[i], StringComparison.OrdinalIgnoreCase))
                        break;
                }
                if (i == patternLen)
                    return index;
                if (index + patternLen >= sourceLen - 1)
                    return -1;
                index += skipDic[source[index + patternLen]];
                if (index + patternLen > sourceLen)
                    return -1;

            }
            #endregion
        }

        public static void LSAppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }

        public static byte[] LSSubBytes(byte[] source, int startIndex)
        {
            int length = source.Length - startIndex;
            return LSSubBytes(source, startIndex, length);
        }

        public static byte[] LSSubBytes(byte[] source, int startIndex, int length)
        {
            byte[] temp = new byte[length];
            Array.Copy(source, startIndex, temp, 0, length);
            //Array.Clear(source,0,source.Length);
            //Array.Resize<byte>(ref source,length);
            //Array.Copy(temp,0,source,0,length);
            return temp;
        }

        public static byte[] LSSubBytes(byte[] source, byte[] startBytes, byte[] endBytes)
        {
            return LSSubBytes(source, startBytes, endBytes, endBytes.Length);
        }

        public static byte[] LSSubBytes(byte[] source, byte[] startBytes, byte[] endBytes, int startIndex)
        {
            return LSSubBytes(source, startBytes, endBytes, endBytes.Length, startIndex);
        }

        public static byte[] LSSubBytes(byte[] source, byte[] startBytes, byte[] endBytes, int includeEndBytesLen, int startIndex)
        {
            int index1 = -1;
            int index2 = -1;
            return LSSubBytes(source, startBytes, endBytes, includeEndBytesLen, startIndex, ref index1, ref index2);
        }

        public static byte[] LSSubBytes(byte[] source, byte[] startBytes, byte[] endBytes, int includeEndBytesLen, int startIndex, ref int index1, ref int index2)
        {
            index1 = -1;
            index2 = -1;
            if (startIndex == -1)
                return null;
            index1 = LSIndexOfBytes(source, startBytes, StringComparison.OrdinalIgnoreCase, startIndex);
            if (index1 >= 0)
                index2 = LSIndexOfBytes(source, endBytes, StringComparison.OrdinalIgnoreCase, index1);
            if (index2 > 0)
                return LSSubBytes(source, index1, index2 - index1 + includeEndBytesLen);
            else
                return null;
        }

        public static List<byte[]> LSSubBytesEx(byte[] source, byte[] startBytes, byte[] endBytes)
        {
            List<byte[]> list = new List<byte[]>();
            int index1 = 0;
            int index2 = 0;
            int srcLen = source.Length;
            int endLen = endBytes.Length;
            while (index2 + endLen < srcLen)
            {
                byte[] temp = LSSubBytes(source, startBytes, endBytes, endBytes.Length, index2, ref index1, ref index2);
                if (temp == null)
                    break;
                else
                    list.Add(temp);
            }
            return list;
        }

        public static byte[] LSSubBytesForServer(byte[] source, byte[] startBytes, int includeEndBytesLen, int startIndex, ref int index1, ref int index2)
        {
            byte[] endBytes = { 10 };
            index1 = -1;
            index2 = -1;
            if (startIndex == -1)
                return null;
            index1 = LSIndexOfBytes(source, startBytes, StringComparison.OrdinalIgnoreCase, startIndex);
            if (index1 >= 0)
            {
                index2 = GetNextIndex(source, index1);
            }
            if (index2 > 0)
                return LSSubBytes(source, index1, index2 - index1 + includeEndBytesLen);
            else
                return null;
        }

        public static List<byte[]> LSSubBytesExForServer(byte[] source, byte[] startBytes)
        {
            List<byte[]> list = new List<byte[]>();
            int index1 = 0;
            int index2 = 0;
            int srcLen = source.Length;
            while (index2 + 1 < srcLen)
            {
                byte[] temp = LSSubBytesForServer(source, startBytes, 1, index2 + 1, ref index1, ref index2);
                if (temp == null)
                    break;
                else
                    list.Add(temp);
            }
            return list;
        }

        public static List<byte[]> LSSubBytesCollection(byte[] source, byte[] startBytes, byte[] endBytes)
        {
            return LSSubBytesEx(source, startBytes, endBytes);
        }

        public static List<byte[]> LSSubBytesCollectionEx(byte[] source, byte[] startBytes, byte[] endBytes, List<byte[]> mustContains)
        {
            List<byte[]> list = new List<byte[]>();
            int index1 = 0;
            int index2 = 0;
            int srcLen = source.Length;
            int endLen = endBytes.Length;
            while (index2 + endLen < srcLen)
            {
                byte[] temp = LSSubBytes(source, startBytes, endBytes, endBytes.Length, index2, ref index1, ref index2);
                if (temp == null)
                    break;
                else
                {
                    bool needAdd = true;
                    foreach (byte[] b in mustContains)
                    {
                        if (LSIndexOfBytes(temp, b, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            needAdd = false;
                            break;
                        }
                    }
                    if (needAdd)
                        list.Add(temp);
                }
            }
            return list;
        }

        public static byte[] LSReplaceBytes(byte[] source, byte[] find, byte[] replacement, StringComparison comparison, int replacementCount, ref int replacedCount)
        {
            byte[] result = new byte[0];
            byte[] curBytes = new byte[source.Length];
            Array.Copy(source, 0, curBytes, 0, source.Length);
            replacedCount = 0;
            while (true)
            {
                if (replacedCount >= replacementCount)
                {
                    LSAppendBytes(ref result, curBytes, 0, curBytes.Length);
                    break;
                }

                int index = LSIndexOfBytes(curBytes, find, comparison);
                if (index >= 0)
                {
                    LSAppendBytes(ref result, curBytes, 0, index);
                    LSAppendBytes(ref result, replacement, 0, replacement.Length);
                    replacedCount++;

                    if (index + find.Length >= curBytes.Length)
                        break;
                    else
                    {
                        curBytes = LSSubBytes(curBytes, index + find.Length);
                    }
                }
                else
                {
                    LSAppendBytes(ref result, curBytes, 0, curBytes.Length);
                    break;
                }
            }
            return result;
        }

        public static byte[] LSReplaceBytes(byte[] source, int startIndex, int needReplaceLen, byte[] replacement)
        {
            byte[] result = new byte[source.Length + replacement.Length - needReplaceLen];
            Array.Copy(source, 0, result, 0, startIndex);
            Array.Copy(replacement, 0, result, startIndex, replacement.Length);
            Array.Copy(source, startIndex + needReplaceLen, result, startIndex + replacement.Length, source.Length - startIndex - needReplaceLen);
            return result;
        }

        public static int LSFindBytes(byte[] source, byte[] find, StringComparison comparison, int findCount)
        {
            int found = 0;
            byte[] curBytes = new byte[source.Length];
            Array.Copy(source, 0, curBytes, 0, source.Length);
            while (true)
            {
                if (found >= findCount)
                {
                    break;
                }

                int index = LSIndexOfBytes(curBytes, find, comparison);
                if (index >= 0)
                {
                    found++;
                    if (index + find.Length >= curBytes.Length)
                        break;
                    else
                    {
                        curBytes = LSSubBytes(curBytes, index + find.Length);
                    }
                }
                else
                {
                    break;
                }
            }
            return found;
        }

        public static bool LSHasNumber(byte[] source)
        {
            foreach (byte b in source)
            {
                if (b >= 48 && b <= 57)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// string to byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] LSStringToHexBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// byte[] to string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string LSBytesToHexString(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        /// <summary>
        /// Stream to byte[]
        /// </summary>
        public static byte[] LSStreamToBytes(Stream stream)
        {
            return LSStreamToBytes(stream, 0, stream.Length);
        }

        public static byte[] LSStreamToBytes(Stream stream, int startIndex, long length)
        {
            byte[] bytes = new byte[length];
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, startIndex, (int)length);
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// byte[] to Stream
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream LSBytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
    }
}

