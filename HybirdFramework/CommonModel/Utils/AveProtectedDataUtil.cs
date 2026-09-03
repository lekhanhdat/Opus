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
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HybridCommonModel.Utils
{
    public sealed class AveProtectedDataUtil
    {

        private static byte[] optionalEntropy = { 0, 9, 5, 6, 4, 7, 8 };

        //public static byte[] Protect(byte[] userData)
        //{
        //    return ProtectedData.Protect(userData, optionalEntropy, DataProtectionScope.LocalMachine);

        //}

        public static byte[] Protect(byte[] userData)
        {
            byte[] result = ProtectedData.Protect(userData, optionalEntropy, DataProtectionScope.LocalMachine);
            return result;

        }


        public static byte[] UnProtect(byte[] userData)
        {

            byte[] resultBytes = ProtectedData.Unprotect(userData, optionalEntropy, DataProtectionScope.LocalMachine);
            return resultBytes;

        }


        public static byte[] ProtectWithString(SecureString userData)
        {
            byte[] userDataBytes = ConvertSecureStringToBytes(userData);
            byte[] result = Protect(userDataBytes);
            ZeroBytes(userDataBytes);
            return result;

        }


        public static SecureString UnProtectWithString(byte[] userData)
        {

            byte[] resultBytes = UnProtect(userData);
            SecureString resultString = ConvertBytesToSecureString(resultBytes);
            ZeroBytes(resultBytes);
            resultString.MakeReadOnly();
            return resultString;

        }


        public static string ProtectWithBase64(byte[] userData)
        {
            byte[] result = ProtectedData.Protect(userData, optionalEntropy, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(result);

        }



        public static byte[] UnProtectWithBase64(string userData)
        {

            byte[] resultBytes = ProtectedData.Unprotect(Convert.FromBase64String(userData), optionalEntropy, DataProtectionScope.LocalMachine);
            return resultBytes;

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

        public static void ZeroBytes(byte[] bytes)
        {
            Array.Clear(bytes, 0, bytes.Length);
        }

        public static void ZeroChars(char[] chars)
        {
            Array.Clear(chars, 0, chars.Length);
        }

    }
}
