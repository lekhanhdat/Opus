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
    #region using directives

    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.CodeReview;
    //using AvePoint.GCommon.Utility;
    using Microsoft.CSharp;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/1/16",
    "yhzhang@avepoint.com",
    "yhzhang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    ///<Summary>
    /// extension of the System.String class
    ///</Summary>
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

        //public static Int32 GetJavaHashCode(this String value)
        //{
        //    return HashCodeHelper.ToJavaHashCode(value);
        //}

        /// <summary>
        /// Compute a MD5 hash value of the input string value
        /// </summary>
        /// <param name="value">input value</param>
        /// <returns>the result md5 of the input string value</returns>
        //public static String ToMD5HashCode(this String value)
        //{
        //    return HashCodeHelper.ToMD5HashCode(value);
        //}

        /// <summary>
        /// Compute a hash value of the input string value using special hash algorithm
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="hashAlgorithmName">the name of hash algorithm, please visit to <remarks>Url:</remarks>
        /// <see cref="http://msdn.microsoft.com/zh-cn/library/wet69s13(v=vs.85).aspx"/> for the valid names
        /// </param>
        /// <returns>the result hash code of the input string value</returns>
        //public static String ToHashCode(this String value, String hashAlgorithmName)
        //{
        //    return HashCodeHelper.ToHashCode(value, hashAlgorithmName);
        //}

        /// <summary>
        /// This method currently only supports the type in System.dll,System.Core.dll and System.Data.dll,
        /// also, default namespaces only support type in  System,System.Text,System.Linq,System.Collections.Generic
        /// </summary>
        /// <typeparam name="T">the return type value</typeparam>
        /// <param name="expression">the expression to execute</param>
        /// <param name="isNeedReturnValue">is need a return value</param>
        /// <returns>the expression result</returns>
        public static T Eval<T>(this String expression, Boolean isNeedReturnValue = true)
        {
            var result = default(T);

            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("using System.Text;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("public class ExecuteCSharpCode");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    public object ExecuteCode()");
            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine("       var result = default(object);");
            codeBuilder.AppendLine(isNeedReturnValue ? "result =" + expression + ";" : expression + ";");
            codeBuilder.AppendLine("       return result;");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");

            var complierParameters = new CompilerParameters(new String[] { "System.dll", "System.Core.dll", "System.Data.dll" });
            complierParameters.GenerateExecutable = false;
            complierParameters.GenerateInMemory = true;

            var providerOptions = new Dictionary<String, String>();
            providerOptions.Add("CompilerVersion", "v3.5");
            var csharpCodeDomProvider = new CSharpCodeProvider(providerOptions);

            var complierResult = csharpCodeDomProvider.CompileAssemblyFromSource(complierParameters, codeBuilder.ToString());
            if (!complierResult.Errors.HasErrors)
            {
                var assembly = complierResult.CompiledAssembly;
                var dynamicType = assembly.GetType("ExecuteCSharpCode");
                var executeCodeMethod = dynamicType.GetMethod("ExecuteCode");
                var obj = Activator.CreateInstance(dynamicType);
                result = (T)executeCodeMethod.Invoke(obj, null);
            }
            else throw new ArgumentException(String.Format("expression:{0},error details:[1]", expression, complierResult.Errors[0].ErrorText));
            return result;
        }

        /// <summary>
        /// No return value eval method
        /// </summary>
        /// <param name="value">input string value</param>
        public static void Eval(this String value)
        {
            Eval<String>(value, false);
        }

        /// <summary>
        /// To test if the string is null or empty at instance level
        /// </summary>
        /// <param name="value">the string value</param>
        /// <returns>the test result</returns>
        public static Boolean IsNullOrEmpty0(this String value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// java long string time to dot net time in ticks
        /// </summary>
        /// <param name="javaTimeInLongString"></param>
        /// <returns></returns>
        public static Int64 JavaToDotNetTimeInLong(this String javaTimeInLongString)
        {
            var time = Int64.Parse(javaTimeInLongString);
            return time.JavaToDotNetTimeInLong();
        }

        /// <summary>
        /// convert dot net date time ticks to java long time
        /// </summary>
        /// <param name="timeInLong"></param>
        /// <returns></returns>
        public static Int64 DotNetToJavaTime(this String timeInLong)
        {
            var time = Int64.Parse(timeInLong);
            return time.DotNetToJavaTime();
        }

        /// <summary>
        /// format string with instance
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static String FormatWith0(this String format, params Object[] args)
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
            if (s == null) return false;
            else return Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// wrap the regex class match method
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static String Match(this String s, String pattern)
        {
            if (s == null) return String.Empty;
            return Regex.Match(s, pattern).Value;
        }

        /// <summary>
        /// the string class if extension
        /// </summary>
        /// <param name="s"></param>
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
        public static Boolean EqualsIgnoreCase0(this String currentValue, String compareValue)
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

        /// <summary>
        /// To get the mime type of the special file name
        /// </summary>
        /// <param name="fileNameOrExtension">the file name or file name extension with dot </param>
        /// <returns>the mime type string</returns>
        //public static String GetMimeType(this String fileNameOrExtension)
        //{
        //    var mimeHelper = Singleton<MIMETypeHelper>.SingletonInstance;
        //    return mimeHelper.GetMimeTypeForFile(fileNameOrExtension);
        //}

        /// <summary>
        /// To get the mime type from the special file name data
        /// </summary>
        /// <param name="fileNameOrExtension">the file name or file name extension with dot </param>
        /// <returns>the mime type string</returns>
        //public static String GetMimeTypeFromFileData(this String filePath)
        //{
        //    var mimeHelper = Singleton<MIMETypeHelper>.SingletonInstance;
        //    return mimeHelper.GetMimeFromFile(filePath);
        //}

        /// <summary>
        /// Get hashcode in 64-bit
        /// </summary>
        /// <param name="value">the string need get hashcode </param>
        /// <returns>hashcode in 64-bit</returns>
        public static Int32 GetHashCodeIn64BitProcess(this String value)
        {
            unsafe
            {
                fixed (char* src = value)
                {
                    int hash1 = 5381;
                    int hash2 = hash1;
                    int c;
                    char* s = src;
                    while ((c = s[0]) != 0)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ c;
                        c = s[1];
                        if (c == 0)
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ c;
                        s += 2;
                    }
                    //return hash1 + (hash2 * 1566083941);
                    return 0;
                }
            }
        }

        /// <summary>
        /// Get hashcode in 32-bit
        /// </summary>
        /// <param name="value">the string need get hashcode </param>
        /// <returns>hashcode in 32-bit</returns>
        public static Int32 GetHashCodeIn32BitProcess(this String value)
        {
            unsafe
            {
                fixed (char* src = value)
                {
                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;
                    int* pint = (int*)src;
                    int len = value.Length;
                    while (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        if (len <= 2)
                        {
                            break;
                        }
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }
                    //return hash1 + (hash2 * 1566083941);
                    return 0;
                }
            }
        }
        //public static string ToPlainString(this SecureString secureString)
        //{
        //    if (secureString == null)
        //    {
        //        return null;
        //    }
        //    IntPtr intPtr = Marshal.SecureStringToBSTR(secureString);
        //    try
        //    {
        //        if (intPtr == IntPtr.Zero)
        //        {
        //            return string.Empty;
        //        }
        //        return Marshal.PtrToStringBSTR(intPtr);
        //    }
        //    finally
        //    {
        //        //Marshal.FreeBSTR(intPtr);
        //        Marshal.ZeroFreeBSTR(intPtr);
        //    }
        //}

        public static SecureString ToSecureStringWithEmptyCheck(this string password)
        {
            if (password.IsNullOrEmpty0())
            {
                return null;
            }
            else
            {
                SecureString secureString = new SecureString();
                foreach (char c in password)
                {
                    secureString.AppendChar(c);
                }

                return secureString;
            }
        }

        public static bool IsNullOrEmpty(this SecureString secureString)
        {
            if (secureString == null)
            {
                return true;
            }
            else
            {
                return secureString.Length == 0;
            }
        }
    }
}