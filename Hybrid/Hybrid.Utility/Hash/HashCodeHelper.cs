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
namespace  AvePoint.Hybrid.Utility.Hash
{
    #region using directives
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    /// <summary>
    /// Provide the Hash algorithm of the string
    /// </summary>
    /// <remarks>the name of hash algorithm, please visit to</remarks>
    /// <remarks>Url:</remarks>
    /// <see cref="http://msdn.microsoft.com/en-us/library/wet69s13(v=vs.85).aspx"/> for the valid names
    public static class HashCodeHelper
    {
        /// <summary>
        /// Returns a hash code for this string. The hash code for a
        /// String object is computed as
        /// 
        /// s[0]*31^(n-1) + s[1]*31^(n-2) + ... + s[n-1]
        /// 
        /// using int arithmetic, where s[i] is the
        /// i th character of the string, n is the length of
        /// the string, and ^ indicates exponentiation.
        /// (The hash value of the empty string is zero.)
        /// 
        ///  This extension method use the JAVA 5 String class hash code
        ///  algorithm to compute the JAVA hash
        /// </summary>
        /// <param name="value"></param>
        /// <returns>a hash code value for this object</returns>
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
            return ToHashCode(value, "MD5");
        }

        /// <summary>
        /// Compute a hash value of the input string value using special hash algorithm
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="hashAlgorithmName">the name of hash algorithm, please visit to <remarks>Url:</remarks>
        /// </param>
        /// <see cref="http://msdn.microsoft.com/en-us/library/wet69s13(v=vs.85).aspx">Url for the valid names</see>
        /// <returns>the result hash code of the input string value</returns>
        public static String ToHashCode(String value, String hashAlgorithmName)
        {
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithmName))
            {
                Debug.Assert(hashAlgorithm != null, "hashAlgorithm != null");
                hashAlgorithm.Initialize();
                var hashByteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hashByteArray).Replace("-", "").ToLowerInvariant();
            }
        }

        //public static Byte[] ToHashSecretKey(String secretKey)
        //{
        //    //Create a unsalted sha1 hash code
        //    var unsaltedSecretKey = CreateSha512Hash(secretKey);  //CreateSha512Hash(secretKey)

        //    //Generate a random salt key
        //    var rngProvider = new RNGCryptoServiceProvider();
        //    var saltValue = new Byte[SaltLength];
        //    rngProvider.GetBytes(saltValue);

        //    //Create salted secret key
        //    return CreateSaltedSecretKey(saltValue, unsaltedSecretKey, HashAlgorithm.SHA2); //CreateSha512SaltedSecretKey(saltValue, unsaltedSecretKey)
        //}

        static Byte[] CreateSaltedSecretKey(Byte[] saltValue, Byte[] unsaltedSecretKey, HashAlgorithm algorithm)
        {
            //The following is the main salted algorithm
            var rawSalted = new byte[unsaltedSecretKey.Length + saltValue.Length];
            unsaltedSecretKey.CopyTo(rawSalted, 0);
            saltValue.CopyTo(rawSalted, unsaltedSecretKey.Length);
            var saltedSecretedKey = CreateHash(algorithm, rawSalted);
            var saltedSecretedKeyWithSaltArray = new byte[saltedSecretedKey.Length + saltValue.Length];
            saltedSecretedKey.CopyTo(saltedSecretedKeyWithSaltArray, 0);
            saltValue.CopyTo(saltedSecretedKeyWithSaltArray, saltedSecretedKey.Length);
            return saltedSecretedKeyWithSaltArray;
        }

        static byte[] CreateHash(HashAlgorithm algorithm, byte[] types)
        {
            if (algorithm == HashAlgorithm.SHA2)
            {
                using (var sha512 = SHA512.Create())
                {
                    return sha512.ComputeHash(types);
                }
            }
            else
            {
                using (var sha1 = SHA1.Create())
                {
                    return sha1.ComputeHash(types);
                }
                    
            }
        }

        const Int32 SaltLength = 4;
        //public static Byte[] CreateSha1Hash(String secretKey)
        //{
        //    return SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(secretKey));
        //}

        //public static Byte[] CreateSha512Hash(String secretKey)
        //{
        //    return SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(secretKey));
        //}

        public static Boolean IsTheSameSecretedKey(Byte[] saltedSecretKey, Byte[] unsaltedSecretKey, HashAlgorithm algorithm)
        {
            if (saltedSecretKey == null || unsaltedSecretKey == null
               || unsaltedSecretKey.Length != saltedSecretKey.Length - SaltLength)
            {
                return false;
            }
            var saltValue = new Byte[SaltLength];
            Array.Copy(saltedSecretKey, saltedSecretKey.Length - SaltLength, saltValue, 0, SaltLength);
            var computedSaltedSecretKey = CreateSaltedSecretKey(saltValue, unsaltedSecretKey, algorithm);
            return ComareByteArray(saltedSecretKey, computedSaltedSecretKey);
        }

        public static string GetEncryptionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            key = key.ToLowerInvariant();
            var schemaName = new StringBuilder();
            var accountNameChars = key.ToCharArray();
            foreach (var c in accountNameChars)
            {
                if (Char.IsLetter(c) || Char.IsNumber(c))
                {
                    schemaName.Append(c);
                }
                else
                {
                    schemaName.Append('#');
                }
            }
            var firstHash = ToMD5HashCode(schemaName.ToString());
            var secondHash = ToMD5HashCode(firstHash);
            //return Encoding.UTF8.GetBytes(secondHash);
            return secondHash;
        }

        public static string GetOldEncryptionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            var schemaName = new StringBuilder();
            var accountNameChars = key.ToCharArray();
            foreach (var c in accountNameChars)
            {
                if (Char.IsLetter(c) || Char.IsNumber(c))
                {
                    schemaName.Append(c);
                }
                else
                {
                    schemaName.Append('#');
                }
            }
            var firstHash = ToMD5HashCode(schemaName.ToString());
            var secondHash = ToMD5HashCode(firstHash);
            //return Encoding.UTF8.GetBytes(secondHash);
            return secondHash;
        }
        static Boolean ComareByteArray(Byte[] saltedSecretKey, Byte[] computedSaltedSecretKey)
        {
            if (saltedSecretKey.Length != computedSaltedSecretKey.Length)
            {
                return false;
            }
            return !saltedSecretKey.Where((t, i) => t != computedSaltedSecretKey[i]).Any();
        }

        /*static Boolean IsUseFIPS140CompliantCryptographicAlgorithms()
        {
            var resultFipsStatus = default(String);
            var fipsKey = default(String);

            fipsKey = @"System\CurrentControlSet\Control\Lsa\FIPSAlgorithmPolicy";
            resultFipsStatus = ReadLocalMachine(fipsKey, "Enabled");
            if (resultFipsStatus.IsNotNullOrEmpty())
            {
                fipsKey = @"System\CurrentControlSet\Control\Lsa";
                resultFipsStatus = ReadLocalMachine(fipsKey, "FIPSAlgorithmPolicy");
            }

            return resultFipsStatus.Equals("1", StringComparison.OrdinalIgnoreCase);
        }*/

        public static String ReadLocalMachine(String subKey, String valueName)
        {
            var result = String.Empty;
            using (var key = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    result = value == null ? String.Empty : value.ToString();
                }
            }
            return result;
        }
    }

    public enum HashAlgorithm
    {
         SHA1,
         SHA2,
    }
}