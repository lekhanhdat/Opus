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
using System.Security;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public static class CspCrossPlatformExchangeWrapper
    {
        //为了使敏感数据在传输过程中更加安全所以在加密之前和解密之后的明文都应该改为采用byte数组
        //或SecureString进行传输

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <returns>字符串密文</returns>
        public static string WrapKeyToBase64String(byte[] password)
        {
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
                byte[] key = en.EncryptBinary(password);
                return Convert.ToBase64String(key);
            }
            return null;
        }

        /// <summary>
        /// 加密(AES)
        /// </summary>
        /// <returns>字符串密文</returns>
        public static string WrapKeyToBase64String(SecureString password)
        {
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
                byte[] key = en.EncryptString(password);
                return Convert.ToBase64String(key);
            }
            return null;
        }

        public static byte[] WrapKeyToByte(byte[] password)
        {
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
                return en.EncryptBinary(password);
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <returns></returns>
        public static byte[] UnWrapKey(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                IEncryption en = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
                return en.DecryptBinary(Convert.FromBase64String(password));
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <returns></returns>
        public static SecureString UnWrapKeyToSecureString(string password)
        {
            if (password != null)
            {
                byte[] key = UnWrapKey(password);
                return CryptoUtil.ConvertBytesToSecureString(key);
            }
            return null;
        }

        public static byte[] UnWrapKeyFromByte(byte[] password)
        {
            if (password != null)
            {
                IEncryption en = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
                return en.DecryptBinary(password);
            }
            return null;
        }


    }
}
