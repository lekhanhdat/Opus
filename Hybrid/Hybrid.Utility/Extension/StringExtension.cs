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

// ReSharper disable CheckNamespace
namespace System
// ReSharper restore CheckNamespace
{
    using Diagnostics.Contracts;

    #region using directives

    using Text.RegularExpressions;
    using AvePoint.Hybrid.Utility.Hash;
    #endregion using directives

#pragma warning disable 1587
    ///<Summary>
    /// extension of the System.String class
    ///</Summary>
#pragma warning restore 1587
    public static class StringExtension
    {
        /**
         * Returns a hash code for this string. The hash code for a
         * String object is computed as
         *
         * s[0]*31^(n-1) + s[1]*31^(n-2) + ... + s[n-1]
         *
         * using int arithmetic, where s[i] is the
         * i th character of the string, n is the length of
         * the string, and ^ indicates exponentiation.
         * (The hash value of the empty string is zero.)
         *
         * @return a hash code value for this object.
         *
         *  This extension method use the JAVA 5 String class hash code
         *  algorithm to compute the JAVA hash
         */

        public static Int32 GetJavaHashCode(this String value)
        {
            return HashCodeHelper.ToJavaHashCode(value);
        }

        /// <summary>
        /// Compute a MD5 hash value of the input string value
        /// </summary>
        /// <param name="value">input value</param>
        /// <returns>the result md5 of the input string value</returns>
        // ReSharper disable InconsistentNaming
        public static String ToMD5HashCode(this String value)
        // ReSharper restore InconsistentNaming
        {
            return HashCodeHelper.ToMD5HashCode(value);
        }

        /// <summary>
        /// Compute a hash value of the input string value using special hash algorithm
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="hashAlgorithmName">the name of hash algorithm, please visit to <remarks>Url:</remarks>
        /// <see cref="http://msdn.microsoft.com/zh-cn/library/wet69s13(v=vs.85).aspx"/> for the valid names
        /// </param>
        /// <returns>the result hash code of the input string value</returns>
        public static String ToHashCode(this String value, String hashAlgorithmName)
        {
            return HashCodeHelper.ToHashCode(value, hashAlgorithmName);
        }

        /// <summary>
        /// To test if the string is null or empty at instance level
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the test result</returns>
        public static Boolean IsNullOrEmpty(this String value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// To test if the string is not null or empty at instance level
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the test result</returns>
        [Pure]
        public static Boolean IsNotNullOrEmpty(this String value)
        {
            return !String.IsNullOrEmpty(value);
        }
        /// <summary>
        /// format string with instance
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static String FormatWith(this String format, params Object[] args)
        {
            return String.Format(format, args);
        }

        /// <summary>
        /// wrap the is match method of the regex
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Boolean IsMatch(this String s, String pattern)
        {
            return s != null && Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// wrap the regex class match method
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static String Match(this String s, String pattern)
        {
            return s == null ? String.Empty : Regex.Match(s, pattern).Value;
        }

        /// <summary>
        /// the string class if extension
        /// </summary>
        /// <param name="value"></param>
        /// <param name="predicate"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static String If(this String value, Predicate<String> predicate, Func<String, String> function)
        {
            return predicate(value) ? function(value) : value;
        }

        /// <summary>
        /// convert string to int32
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>the converted int value</returns>
        public static Int32 ToInt32(this String value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Convert String to Enum
        /// </summary>
        /// <typeparam name="T">the enum type</typeparam>
        /// <param name="value">the enum constant string</param>
        /// <returns>converted enum value</returns>
        public static T ToEnum<T>(this String value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// equals method with ignore case
        /// </summary>
        /// <param name="currentValue">current string value</param>
        /// <param name="compareValue">compare string value</param>
        /// <returns>the equals result</returns>
        [Pure]
        public static Boolean EqualsIgnoreCase(this String currentValue, String compareValue)
        {
            return currentValue.Equals(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// index of method with ignore case
        /// </summary>
        /// <param name="currentValue">current string value</param>
        /// <param name="compareValue">compare string value</param>
        /// <returns>the index of result</returns>
        public static Int32 IndexOfIgnoreCase(this String currentValue, String compareValue)
        {
            return currentValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// last index of method with ignore case
        /// </summary>
        /// <param name="currentValue">current string value</param>
        /// <param name="compareValue">compare string value</param>
        /// <returns>the last index of result</returns>
        public static Int32 LastIndexOfIgnoreCase(this String currentValue, String compareValue)
        {
            return currentValue.LastIndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// equals method with ignore case
        /// </summary>
        /// <param name="currentValue">current string value</param>
        /// <param name="endValue">end string value</param>
        /// <returns>the end with string result</returns>
        public static Boolean EndWithIgnoreCase(this String currentValue, String endValue)
        {
            return currentValue.EndsWith(endValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compare two string
        /// </summary>
        /// <param name="currentValue">first string</param>
        /// <param name="compareValue">compare string</param>
        /// <returns>compare result</returns>
        public static Int32 CompareToIngnoreCase(this String currentValue, String compareValue)
        {
            return String.Compare(currentValue, compareValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces the first occurrence of a specified System.String in this instance, with another specified System.String.
        /// </summary>
        /// <param name="currentValue">current string value</param>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        /// <returns>replace result</returns>
        public static String ReplaceFirst(this String currentValue, String oldValue, String newValue)
        {
            var offset = currentValue.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            var temp = currentValue.Remove(offset, oldValue.Length);
            return temp.Insert(offset, newValue);
        }
        
    }
}