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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Text;
    using AvePoint.GCommon.Utility.Cryptography.Hash;
    using System.IO;
    using AvePoint.GCommon.Utility.Cryptography;
    #endregion

    /// <summary>
    /// Provide the Hash algorithm of the string
    /// </summary>
    /// <remarks>the name of hash algorithm, please visit to</remarks>
    /// <remarks>Url:</remarks>
    /// <see cref=("http://msdn.microsoft.com/en-us/library/wet69s13(v=vs.85).aspx")/> for the valid names
    public static class HashCodeHelper
    {
        delegate String HashCodeDelegate(String value);
        static HashCodeDelegate md5HashCodeDelegate;

        static HashCodeHelper()
        {
            if (FipsModeUtil.IsFIPSMode())
            {
                md5HashCodeDelegate = value =>
                {
                    var md5HashAlgorithm = new AveMD5Provider();
                    var md5ByteArray = md5HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                    return BitConverter.ToString(md5ByteArray).Replace("-", "").ToLower();
                };
            }
            else md5HashCodeDelegate = value => ToHashCode(value, "MD5");
        }

        /// <summary>
        /// For FIPS support, you can find the registry location in the following URI
        /// <see cref="http://support.microsoft.com/kb/811833"/>, The kb article is
        /// describe the detail of how fips compliant cryptographic algorithms influence
        /// the dot net crypto apis
        /// </summary>
        /// <returns>the fips enabled status</returns>
        //static Boolean IsUseFIPS140CompliantCryptographicAlgorithms()
        //{
        //    var resultFipsStatus = default(String);
        //    var fipsKey = default(String);
        //    if (OSInformation.OSVersionNumber >= 60)
        //    {
        //        fipsKey = @"System\CurrentControlSet\Control\Lsa\FIPSAlgorithmPolicy";
        //        resultFipsStatus = RegistryManager.ReadLocalMachine(fipsKey, "Enabled");
        //    }
        //    else
        //    {
        //        fipsKey = @"System\CurrentControlSet\Control\Lsa";
        //        resultFipsStatus = RegistryManager.ReadLocalMachine(fipsKey, "FIPSAlgorithmPolicy");
        //    }

        //    return resultFipsStatus.Equals("1", StringComparison.OrdinalIgnoreCase);
        //}

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

        public static Int32 ToJavaHashCode(String value)
        {
            var hashResult = default(Int32);
            if (!String.IsNullOrEmpty(value))
                Array.ForEach(value.ToCharArray(), item => hashResult = 31 * hashResult + item);
            return hashResult;
        }

        /// <summary>
        /// Get a string value's md5 hash and convert to a guid object
        /// </summary>
        /// <param name="value">value which will be compute hash</param>
        /// <returns>The result guid get from </returns>
        public static Guid StringHash(String value)
        {
            return new Guid(ToMD5HashCode(value));
        }

        /// <summary>
        /// Compute a MD5 hash value of the input string value
        /// </summary>
        /// <param name="value">input value</param>
        /// <returns>the result md5 of the input string value</returns>
        public static String ToMD5HashCode(String value)
        {
            return md5HashCodeDelegate(value);
        }

        /// <summary>
        /// Compute a hash value of the input string value using special hash algorithm
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="hashAlgorithmName">the name of hash algorithm, please visit to <remarks>Url:</remarks>
        /// </param>
        /// <see cref=("http://msdn.microsoft.com/en-us/library/wet69s13(v=vs.85).aspx")>Url for the valid names</see>
        /// <returns>the result hash code of the input string value</returns>
        public static String ToHashCode(String value, String hashAlgorithmName)
        {
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithmName))
            {
                hashAlgorithm.Initialize();
                var md5ByteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(md5ByteArray).Replace("-", "").ToLower();
            }
        }
        public static String ToHashCode(Stream stream, String hashAlgorithmName)
        {
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithmName))
            {
                hashAlgorithm.Initialize();
                var hashByteArray = hashAlgorithm.ComputeHash(stream);
                return BitConverter.ToString(hashByteArray).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}