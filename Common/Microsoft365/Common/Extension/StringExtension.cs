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
namespace System
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.CSharp;

    ///<Summary>
    /// extension of the System.String class
    ///</Summary>
    public static class StringExtension
    {

        //public static bool EqualsIgnoreCase(this string currentValue, string compareValue)
        //{
        //    return currentValue?.Equals(compareValue, StringComparison.OrdinalIgnoreCase) ?? (compareValue == null);
        //}
        //public static T StringToEnum<T>(this string value)
        //{
        //    return value.ToEnum<T>();
        //}

        /// <summary>
        /// Get hashcode in 64-bit
        /// </summary>
        /// <param name="value">the string need get hashcode </param>
        /// <returns>hashcode in 64-bit</returns>
        //public static Int32 GetHashCodeIn64BitProcess(this string value)
        //{
        //    unsafe
        //    {
        //        fixed (char* src = value)
        //        {
        //            int hash1 = 5381;
        //            int hash2 = hash1;
        //            int c;
        //            char* s = src;
        //            while ((c = s[0]) != 0)
        //            {
        //                hash1 = ((hash1 << 5) + hash1) ^ c;
        //                c = s[1];
        //                if (c == 0)
        //                    break;
        //                hash2 = ((hash2 << 5) + hash2) ^ c;
        //                s += 2;
        //            }
        //            return hash1 + (hash2 * 1566083941);
        //        }
        //    }
        //}

        /// <summary>
        /// Get hashcode in 32-bit
        /// </summary>
        /// <param name="value">the string need get hashcode </param>
        /// <returns>hashcode in 32-bit</returns>
        //public static Int32 GetHashCodeIn32BitProcess(this string value)
        //{
        //    unsafe
        //    {
        //        fixed (char* src = value)
        //        {
        //            int hash1 = (5381 << 16) + 5381;
        //            int hash2 = hash1;
        //            int* pint = (int*)src;
        //            int len = value.Length;
        //            while (len > 0)
        //            {
        //                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
        //                if (len <= 2)
        //                {
        //                    break;
        //                }
        //                hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
        //                pint += 2;
        //                len -= 4;
        //            }
        //            return hash1 + (hash2 * 1566083941);
        //        }
        //    }
        //}

        public static int Deepth(this string path, params char[] separator)
        {
            return path.Split(separator, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string Remove(this string str, params string[] keys)
        {
            if (string.IsNullOrEmpty(str)) return str;
            foreach (var key in keys)
            {
                if (str.IndexOf(key) >= 0)
                {
                    str = str.Replace(key, string.Empty);
                }
            }
            return str;
        }

        /// <summary>
        /// Do not display the file name in the log, the file name is distinguished from parent path by "/"
        /// </summary>
        public static string FormatURLInLog(this string url, int itemId = -1)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            if (!url.Contains("/"))
            {
                return itemId.ToString();
            }
            string parentUrl = url.Substring(0, url.LastIndexOf('/'));
            if (itemId == -1)
            {
                return parentUrl + "/";
            }
            else
            {
                return parentUrl + "/" + itemId;
            }
        }

        /// <summary>
        /// Do not display the file name in the log, the file name is distinguished from parent path by "\"
        /// </summary>
        public static string FormatFilePathInLog(this string filePath, int itemId = -1)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }
            if (!filePath.Contains("\\"))
            {
                return itemId.ToString();
            }
            string parentPath = filePath.Substring(0, filePath.LastIndexOf('\\'));
            if (itemId == -1)
            {
                return parentPath + "\\";
            }
            else
            {
                return parentPath + "\\" + itemId;
            }
        }

        [return: NotNullIfNotNull(nameof(argument))]
        public static String EnsureIfNotNullOrEmpty([NotNull] this string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(argument, paramName);
            return argument;
        }

        [return: NotNullIfNotNull(nameof(argument))]
        public static T EnsureIfNotNull<T>([NotNull] this T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            return argument;
        }

        public static string Truncate(this string value, int maxLength, string ellipsis = "...")
        {
            var valueSpan = value.AsSpan();
            var valueByteCount = Encoding.UTF8.GetByteCount(valueSpan);
            if (valueByteCount <= maxLength)
            {
                return value;
            }

            var ellipsisByteCount = Encoding.UTF8.GetByteCount(ellipsis.AsSpan());

            var availableByteSpace = maxLength - ellipsisByteCount;
            var halfAvailableByteSpace = availableByteSpace / 2;

            var bytes = new Span<byte>(new byte[valueByteCount]);
            Encoding.UTF8.GetBytes(valueSpan, bytes);

            return $"{LeftPart(bytes)}{ellipsis}{RightPart(bytes)}";

            string LeftPart(Span<byte> bytes)
            {
                var current = halfAvailableByteSpace;
                while (InMiddleOfChar(bytes, current))
                {
                    current--;
                }
                return Encoding.UTF8.GetString(bytes[..current]);
            }

            string RightPart(Span<byte> bytes)
            {
                var current = bytes.Length - halfAvailableByteSpace;
                while (InMiddleOfChar(bytes, current))
                {
                    current++;
                }
                return Encoding.UTF8.GetString(bytes[current..]);
            }

            bool InMiddleOfChar(Span<byte> bytes, int index) => index < bytes.Length && (bytes[index] & 0xC0) == 0x80;
        }

    }
}