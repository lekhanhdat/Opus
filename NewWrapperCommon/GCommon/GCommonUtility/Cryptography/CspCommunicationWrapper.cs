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
using System.Text;
using System.Security;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public static class CspCommunicationWrapper
    {
        public static byte[] staticCommunicationEncryptionKey = 
        {
            (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204,
            (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204
        };

        private static byte[] communicationEncryptionKey;
        private static byte[] communicationEncryptionKeyHash;

        static CspCommunicationWrapper()
        {
            CryptographyManagement.CryptoInit();
        }
        

        public  static byte[] CommunicationEncryptionKey
        {
            get
            {
                return communicationEncryptionKey;
            }
            set
            {
                communicationEncryptionKey = value;
                communicationEncryptionKeyHash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1, new byte[0]).ComputeHash(communicationEncryptionKey);
            }


        }


        public static string AuthToken
        {
            get
            {
                if (communicationEncryptionKeyHash == null)
                {
                    return null;
                }
                else
                {
                    return Convert.ToBase64String(communicationEncryptionKeyHash);
                }
            }


        }

        private static void CheckAvailable()
        {
            if (communicationEncryptionKey == null || communicationEncryptionKeyHash == null)
            {

                throw new Exception("Communication key unavailable");
            }

        }
        //为了使敏感数据在传输过程中更加安全所以在加密之前和解密之后的明文都应该改为采用byte数组
        //或SecureString进行传输

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <param name="password">敏感数据</param>
        /// <returns>字符串密文</returns>
        public static string WrapKeyToBase64String(byte[] password)
        {
            if (password != null)
            {

                byte[] result = WrapKey(password);
                return Convert.ToBase64String(result);
            }
            return null;
        }


        //为了使敏感数据在传输过程中更加安全所以在加密之前和解密之后的明文都应该改为采用byte数组
        //或SecureString进行传输

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <param name="password">敏感数据</param>
        /// <returns>字符串密文</returns>
        public static byte[] WrapKey(byte[] password)
        {
            CheckAvailable();
            if (password != null)
            {

                IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
                byte[] key = en.EncryptBinary(password);
                return key;
            }
            return null;
        }

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <param name="password">敏感数据</param>
        /// <returns>字符串密文</returns>
        public static string WrapKeyToBase64String(SecureString password)
        {
            CheckAvailable();
            if (password != null)
            {
                byte[] result = WrapKey(password);
                return Convert.ToBase64String(result);
            }
            return null;
        }

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <param name="password">敏感数据</param>
        /// <returns>字符串密文</returns>
        public static byte[] WrapKey(SecureString password)
        {
            CheckAvailable();
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
                byte[] key = en.EncryptString(password);
                return key;
            }
            return null;
        }



        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="password">字符串密文</param>
        /// <returns></returns>
        public static byte[] UnWrapKey(string password)
        {
            CheckAvailable();
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
                return en.DecryptBytesWithBase64(password);
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="password">字符串密文</param>
        /// <returns></returns>
        public static byte[] UnWrapKey(byte[] password)
        {
            CheckAvailable();
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
                return en.DecryptBinary(password);
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="password">字符串密文</param>
        /// <returns></returns>
        public static SecureString UnWrapKeyToSecureString(string password)
        {
            CheckAvailable();
            if (password != null)
            {
                byte[] key = UnWrapKey(password);
                return CryptoUtil.ConvertBytesToSecureString(key);
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="password">字符串密文</param>
        /// <returns></returns>
        public static SecureString UnWrapKeyToSecureString(byte[] password)
        {
            CheckAvailable();
            if (password != null)
            {
                byte[] key = UnWrapKey(password);
                return CryptoUtil.ConvertBytesToSecureString(key);
            }
            return null;
        }

    }




}

