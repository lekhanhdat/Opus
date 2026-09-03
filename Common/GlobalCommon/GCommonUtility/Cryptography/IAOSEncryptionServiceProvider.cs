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
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.Cryptography
{
    /// <summary>
    /// 加解密时成对使用同名方法
    /// </summary>
    public interface IAOSEncryptionServiceProvider
    {
        byte[] EncryptBinary(byte[] binary);

        byte[] DecryptBinary(byte[] binary);

        string EncryptStringWithBase64(string content);

        string DecryptStringWithBase64(string wrapKey);

        /// <summary>
        /// return a base64 string
        /// </summary>
        /// <param name="plainString"></param>
        /// <returns></returns>
        string EncryptSecureStringWithBase64(SecureString plainString);

        /// <summary>
        /// parameter is a base 64 string
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        SecureString DecryptSecureStringWithBase64(string encryptedString);

        byte[] EncryptSecureString(SecureString plainString);

        SecureString DecryptSecureString(byte[] encryptedByte);

        string GetProfileId();

        string GetSecret(string secretName);

        string SetSecret(string secretName, string value);
    }

    /// <summary>
    /// fail to get security profile from AOS, return parameter or null
    /// </summary>
    public class NonSecurityProfileServiceProvider : IAOSEncryptionServiceProvider
    {
        public byte[] EncryptBinary(byte[] binary)
        {
            return binary;
        }

        public byte[] DecryptBinary(byte[] binary)
        {
            return binary;
        }

        public string EncryptStringWithBase64(string content)
        {
            return content;
        }

        public string DecryptStringWithBase64(string wrapKey)
        {
            return wrapKey;
        }

        /// <summary>
        /// return a base64 string
        /// </summary>
        /// <param name="plainString"></param>
        /// <returns></returns>
        public string EncryptSecureStringWithBase64(SecureString plainString)
        {
            return string.Empty;
        }

        /// <summary>
        /// parameter is a base 64 string
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public SecureString DecryptSecureStringWithBase64(string encryptedString)
        {
            return null;
        }

        public byte[] EncryptSecureString(SecureString plainString)
        {
            return new byte[0];
        }

        public SecureString DecryptSecureString(byte[] encryptedByte)
        {
            return null;
        }

        public string GetProfileId()
        {
            return string.Empty;
        }

        public string GetSecret(string secretName)
        {
            return string.Empty;
        }

        public string SetSecret(string secretName, string value)
        {
            return string.Empty;
        }
    }


    public class DatabaseEncrytionServiceProvider : IAOSEncryptionServiceProvider
    {
        public byte[] EncryptBinary(byte[] binary)
        {
            return binary;
        }

        public byte[] DecryptBinary(byte[] binary)
        {
            return binary;
        }

        public string EncryptStringWithBase64(string content)
        {
            return content;
        }

        public string DecryptStringWithBase64(string wrapKey)
        {
            return wrapKey;
        }


        public string EncryptSecureStringWithBase64(SecureString plainString)
        {
            return string.Empty;
        }

        public SecureString DecryptSecureStringWithBase64(string encryptedString)
        {
            return null;
        }

        public byte[] EncryptSecureString(SecureString plainString)
        {
            return new byte[0];
        }

        public SecureString DecryptSecureString(byte[] encryptedByte)
        {
            return null;
        }

        public string GetProfileId()
        {
            return string.Empty;
        }

        public string GetSecret(string secretName)
        {
            return string.Empty;
        }

        public string SetSecret(string secretName, string value)
        {
            return string.Empty;
        }
    }
}
