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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public static class CryptoUtil
    {


        public static string ConvertBytesToString(byte[] key)
        {
            if (key == null)
            {
                return null;

            }
            string result = Encoding.UTF8.GetString(key);
            ZeroBytes(key);
            return result;
        }


        public static byte[] ConvertStringToBytes(string key)
        {
            if (key == null)
            {
                return null;

            }
            byte[] result = Encoding.UTF8.GetBytes(key);
            return result;
        }

        public static bool KeyHashVerify(byte[] key, byte[] hashValue)
        {
            byte[] result = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(key);
            return CryptographyManagement.ArraysEqual<byte>(result, hashValue);
        }

        public static char[] ConvertSecureStringToChars(SecureString sString)
        {
            var result = new char[sString.Length];
            IntPtr buf = Marshal.SecureStringToBSTR(sString);
            Marshal.Copy(buf, result, 0, result.Length);
            Marshal.ZeroFreeBSTR(buf);
            return result;

        }


        public static byte[] ConvertSecureStringToBytes(SecureString sString)
        {
            char[] resultChars = ConvertSecureStringToChars(sString);
            byte[] result = Encoding.UTF8.GetBytes(resultChars);
            ZeroChars(resultChars);
            return result;

        }


        public static SecureString ConvertBytesToSecureString(byte[] bytes)
        {
            //char[] resultChars = ConvertSecureStringToChars(sString);
            
            char[] resultChars = Encoding.UTF8.GetChars(bytes);

            return ConvertCharsToSecureString(resultChars);

        }

        public static SecureString ConvertCharsToSecureString(char[] chars)
        {
            SecureString result = new SecureString();
            foreach (char c in chars)
            {
                result.AppendChar(c);
            }

            ZeroChars(chars);
            return result;

        }
       
        public static void ZeroBytes(byte[] bytes) {
            Array.Clear(bytes, 0, bytes.Length);
        }

        public static void ZeroChars(char[] chars)
        {
            Array.Clear(chars, 0, chars.Length);
        }

        /// <summary>
        /// ClearMemory使用的内部函数
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] GetBrokenDownBytes(long length)
        {
            byte[] buffer = new byte[length];
            Random random = new Random();
            for (int i = 0; i < length; i = i + 2)
            {
                buffer[i] = (byte)random.Next(256);
            }

            return buffer;
        }

        public static string ByteToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }


    }
}
