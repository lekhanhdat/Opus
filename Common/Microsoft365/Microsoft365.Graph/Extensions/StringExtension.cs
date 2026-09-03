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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtension
    {
        //private static readonly ILogger logger = LoggerFactory.Get();

        private static readonly Regex mailRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*?)", RegexOptions.IgnoreCase);

        private static readonly Regex uriRegex = new Regex("^[a-z][a-z0-9+\\-.]*://([a-z0-9\\-._~%!$&'()*+,;=]+@)?(?<host>[a-z0-9\\-._~%]+|\\[[a-z0-9\\-._~%!$&'()*+,;=:]+\\])", RegexOptions.IgnoreCase);

        private static readonly string[] stringSeparators = new string[2] { "/", "\\" };

        //public static string ThrowIfNullOrEmpty([NotNull] this string value)
        //{
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        throw new ArgumentNullException(value);
        //    }

        //    return value;
        //}

        //public static bool IsNullOrEmpty([NotNullWhen(true)] this string value)
        //{
        //    return string.IsNullOrEmpty(value);
        //}

        //public static bool IsNotNullOrEmpty(this string value)
        //{
        //    return !string.IsNullOrEmpty(value);
        //}

        //public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string value)
        //{
        //    return string.IsNullOrWhiteSpace(value);
        //}

        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        //public static bool StartWithIgnoreCase(this string value, string arg)
        //{
        //    return value.StartsWith(arg, StringComparison.OrdinalIgnoreCase);
        //}

        public static bool EndWithIgnoreCase(this string value, string arg)
        {
            return value.EndsWith(arg, StringComparison.OrdinalIgnoreCase);
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool IsMatch(this string s, string pattern)
        {
            if (s != null)
            {
                return Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase);
            }

            return false;
        }

        public static string Match(this string s, string pattern)
        {
            if (s != null)
            {
                return Regex.Match(s, pattern).Value;
            }

            return string.Empty;
        }

        public static string If(this string value, Predicate<string> predicate, Func<string, string> function)
        {
            if (!predicate(value))
            {
                return value;
            }

            return function(value);
        }

        public static int ToInt32(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }

        //public static bool EqualsIgnoreCase(this string currentValue, string compareValue)
        //{
        //    return currentValue?.Equals(compareValue, StringComparison.OrdinalIgnoreCase) ?? (compareValue == null);
        //}

        public static int IndexOfIgnoreCase(this string currentValue, string compareValue)
        {
            return currentValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public static int LastIndexOfIgnoreCase(this string currentValue, string compareValue)
        {
            return currentValue.LastIndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public static int CompareToIngnoreCase(this string currentValue, string compareValue)
        {
            return string.Compare(currentValue, compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceFirst(this string currentValue, string oldValue, string newValue)
        {
            int num = currentValue.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (num < 0)
            {
                return currentValue;
            }

            return currentValue.Remove(num, oldValue.Length).Insert(num, newValue);
        }

        //public static SecureString ToSecureString(this string password)
        //{
        //    SecureString secureString = new SecureString();
        //    foreach (char c in password)
        //    {
        //        secureString.AppendChar(c);
        //    }

        //    return secureString;
        //}

        public static string Decompress(this string compressedText)
        {
            byte[] array = Convert.FromBase64String(compressedText);
            using MemoryStream memoryStream = new MemoryStream();
            int num = BitConverter.ToInt32(array, 0);
            memoryStream.Write(array, 4, array.Length - 4);
            byte[] array2 = new byte[num];
            memoryStream.Position = 0L;
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                int bytesRead = gZipStream.Read(array2, 0, array2.Length);
            }

            return Encoding.UTF8.GetString(array2);
        }

        public static string Compress(this string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gZipStream.Write(bytes, 0, bytes.Length);
            }

            memoryStream.Position = 0L;
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Read(array, 0, array.Length);
            byte[] array2 = new byte[array.Length + 4];
            Buffer.BlockCopy(array, 0, array2, 4, array.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
            return Convert.ToBase64String(array2);
        }

        public static string UpperFirstCharacter(this string text)
        {
            return Regex.Replace(text, "^[a-z]", (Match m) => m.Value.ToUpper());
        }

        public static string LowerFirstCharacter(this string text)
        {
            return Regex.Replace(text, "^[A-Z]", (Match m) => m.Value.ToLower());
        }

        //[Obsolete("MD5 is not allowed")]
        //public static string ToMD5(this string value)
        //{
        //    return value.ToHashCode("MD5");
        //}

        //public static string ToSHA256(this string value)
        //{
        //    return value.ToHashCode("SHA256");
        //}

        //public static string ToHashCode(this string value, string hashAlgorithmName)
        //{
        //    using HashAlgorithm hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
        //    hashAlgorithm.Initialize();
        //    return BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        //}

        //public static string EncryptLog(this string source, int prefixLength = 4, int suffixLength = 4)
        //{
        //    if (source.IsNullOrEmpty())
        //    {
        //        return source;
        //    }

        //    try
        //    {
        //        string input = mailRegex.Replace(source, (Match match) => HideEmail(match.Value));
        //        string text = uriRegex.Replace(input, (Match match) => HideUrl(match.Value));
        //        if (text.EndsWith("***") || text.EndsWith("***/"))
        //        {
        //            return text;
        //        }

        //        List<string> list = text.Split(stringSeparators, StringSplitOptions.None).ToList();
        //        if (list.Count > 1)
        //        {
        //            string text2 = list.LastOrDefault();
        //            string text3 = text.Substring(0, text.Length - text2.Length);
        //            string text4 = HideInfo(text2, prefixLength, suffixLength);
        //            text = text3 + text4;
        //        }
        //        else
        //        {
        //            if (!text.EqualsIgnoreCase(source))
        //            {
        //                return text;
        //            }

        //            text = HideInfo(text, prefixLength, suffixLength);
        //        }

        //        return text;
        //    }
        //    catch (System.Exception)
        //    {
        //        //logger.Error($"Failed to encrypt log '{source}': {value}", "EncryptLog", "/src/dotnet/util/Extension/StringExtension.cs", 326);
        //        return source;
        //    }
        //}

        //public static bool IsEmailAddress(this string source)
        //{
        //    if (source.IsNotNullOrEmpty())
        //    {
        //        return mailRegex.IsMatch(source);
        //    }

        //    return false;
        //}

        public static (bool isExactMatch, HashSet<string> allMatchedMails) IsExactMatchingEmail(this string source)
        {
            HashSet<string> hashSet = new HashSet<string>();
            if (source.IsNotNullOrEmpty())
            {
                MatchCollection matchCollection = mailRegex.Matches(source);
                if (matchCollection != null && matchCollection.Count > 0)
                {
                    if (matchCollection[0].Value.Equals(source))
                    {
                        return (isExactMatch: true, allMatchedMails: hashSet);
                    }

                    foreach (Match item in matchCollection)
                    {
                        hashSet.Add(item.Value);
                    }

                    return (isExactMatch: false, allMatchedMails: hashSet);
                }
            }

            return (isExactMatch: false, allMatchedMails: hashSet);
        }

        public static string Encode(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        private static string HideInfo(string source, int prefixLength, int suffixLength)
        {
            prefixLength = ((prefixLength < 0) ? 4 : prefixLength);
            suffixLength = ((suffixLength < 0) ? 4 : suffixLength);
            if (prefixLength == 0 && suffixLength == 0)
            {
                return "*";
            }

            if (source.Length <= prefixLength)
            {
                return source;
            }

            string text = "";
            if (source.Length > prefixLength + suffixLength)
            {
                text = source.Substring(source.Length - suffixLength, suffixLength);
            }

            source = source.Substring(0, prefixLength) + "*" + text;
            return source;
        }

        //private static string HideEmail(string source)
        //{
        //    string[] source2 = source.Split('@');
        //    string text = source2.LastOrDefault();
        //    return source2.FirstOrDefault() + "@" + text.Substring(0, text.IndexOf('.') + 1) + "***";
        //}

        private static string HideUrl(string source)
        {
            int num = source.IndexOf(".");
            if (num > 0)
            {
                return source.Substring(0, num + 1) + "***";
            }

            return source + "***";
        }
    }
}
