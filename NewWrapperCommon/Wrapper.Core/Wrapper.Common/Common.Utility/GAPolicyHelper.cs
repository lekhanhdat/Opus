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
using System.Security.Cryptography;
using System.Collections;
using System.IO;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Wrapper.Common
{
    public class GAPolicyHelper
    {
        public static readonly List<string> keysNeedToDecryption = new List<string>() { "Gov Auto Policy" };

        public static string GetPolicyValue(string _str, Guid siteId, Guid webId)
        {
            return Decrypt(_str, siteId, webId);
        }

        public static string Encrypt(string _str, Guid siteId, Guid webId)
        {
            byte[] key = new byte[16];
            Array.Copy(siteId.ToByteArray(), 0, key, 0, 8);
            Array.Copy(webId.ToByteArray(), 0, key, 8, 8);
            return Encrypt(_str, new Guid(key));
        }

        public static string Decrypt(string _str, Guid siteId, Guid webId)
        {
            byte[] key = new byte[16];
            Array.Copy(siteId.ToByteArray(), 0, key, 0, 8);
            Array.Copy(webId.ToByteArray(), 0, key, 8, 8);
            return Decrypt(_str, new Guid(key));
        }

        private static string Encrypt(string _str, Guid _key)
        {
            if (String.IsNullOrEmpty(_str))
            {
                throw new ArgumentNullException
                       ("The string which needs to be encrypted cannot be null.");
            }
            if (_key.Equals(Guid.Empty))
            {
                throw new ArgumentException
                       ("The key cannot be an empty GUID.");
            }

            //Old Method
            //RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider(1024);
            //string publicPrivateKeyXML = RSAProvider.ToXmlString(true);
            //string publicOnlyKeyXML = RSAProvider.ToXmlString(false);
            //byte[] value = RSAEncrypt(_str, 1024, publicOnlyKeyXML);

            byte[] value = Convert.FromBase64String(GAEncryptInternal.Encrypt(_str));

            byte[] key = new byte[8];
            byte[] iv = new byte[8];
            Array.Copy(_key.ToByteArray(), 0, key, 0, 8);
            Array.Copy(_key.ToByteArray(), 8, key, 0, 8);
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(Convert.ToBase64String(GAEncryptInternal.CommunicationEncryptionKey));
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            long len = memoryStream.Length;
            byte[] buffer = new byte[4];
            buffer[0] = (byte)len;
            len >>= 8;
            buffer[1] = (byte)len;
            len >>= 8;
            buffer[2] = (byte)len;
            len >>= 8;
            buffer[3] = (byte)len;

            MemoryStream ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            ms.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            ms.Write(value, 0, value.Length);
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        private static string Decrypt(string _str, Guid _key)
        {
            if (String.IsNullOrEmpty(_str))
            {
                throw new ArgumentNullException
                   ("The string which needs to be decrypted cannot be null.");
            }
            byte[] key = new byte[8];
            byte[] iv = new byte[8];
            Array.Copy(_key.ToByteArray(), 0, key, 0, 8);
            Array.Copy(_key.ToByteArray(), 8, key, 0, 8);
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(_str));
            byte[] buffer = new byte[4];
            ms.Read(buffer, 0, 4);
            int len = 0;
            int offset = 3;
            for (int i = 0; i < 4; ++i)
            {
                len <<= 8;
                len += buffer[offset--];
            }

            byte[] temp = new byte[len];
            ms.Read(temp, 0, len);
            byte[] value = new byte[ms.Length - 4 - len];
            ms.Read(value, 0, value.Length);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream
                    (temp);
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(key, iv), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            string internalKey = reader.ReadToEnd();

            GAEncryptInternal.CommunicationEncryptionKey = Convert.FromBase64String(internalKey);
            return GAEncryptInternal.Decrypt(Convert.ToBase64String(value));

            //string publicOnlyKeyXML = reader.ReadToEnd();

            //return RSADecrypt(Convert.ToBase64String(value), 1024, publicOnlyKeyXML);
        }

        #region Old Method
        //private static string Decrypt(string _str, Guid _key)
        //{
        //    if (String.IsNullOrEmpty(_str))
        //    {
        //        throw new ArgumentNullException
        //           ("The string which needs to be decrypted cannot be null.");
        //    }
        //    byte[] key = new byte[8];
        //    byte[] iv = new byte[8];
        //    Array.Copy(_key.ToByteArray(), 0, key, 0, 8);
        //    Array.Copy(_key.ToByteArray(), 8, key, 0, 8);
        //    MemoryStream ms = new MemoryStream(Convert.FromBase64String(_str));
        //    byte[] buffer = new byte[4];
        //    ms.Read(buffer, 0, 4);
        //    int len = 0;
        //    int offset = 3;
        //    for (int i = 0; i < 4; ++i)
        //    {
        //        len <<= 8;
        //        len += buffer[offset--];
        //    }

        //    byte[] temp = new byte[len];
        //    ms.Read(temp, 0, len);
        //    byte[] value = new byte[ms.Length - 4 - len];
        //    ms.Read(value, 0, value.Length);
        //    DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        //    MemoryStream memoryStream = new MemoryStream
        //            (temp);
        //    CryptoStream cryptoStream = new CryptoStream(memoryStream,
        //        cryptoProvider.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        //    StreamReader reader = new StreamReader(cryptoStream);
        //    string publicOnlyKeyXML = reader.ReadToEnd();

        //    return RSADecrypt(Convert.ToBase64String(value), 1024, publicOnlyKeyXML);
        //}

        //private static string RSADecrypt(string inputString, int dwKeySize,
        //                             string xmlString)
        //{
        //    RSACryptoServiceProvider rsaCryptoServiceProvider
        //                             = new RSACryptoServiceProvider(dwKeySize);
        //    rsaCryptoServiceProvider.FromXmlString(xmlString);
        //    int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ?
        //      (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
        //    int iterations = inputString.Length / base64BlockSize;
        //    ArrayList arrayList = new ArrayList();
        //    for (int i = 0; i < iterations; i++)
        //    {
        //        byte[] encryptedBytes = Convert.FromBase64String(
        //             inputString.Substring(base64BlockSize * i, base64BlockSize));
        //        Array.Reverse(encryptedBytes);
        //        arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(
        //                            encryptedBytes, true));
        //    }
        //    return Encoding.UTF32.GetString(arrayList.ToArray(
        //                              Type.GetType("System.Byte")) as byte[]);
        //}
        #endregion
    }

    internal class GAEncryptInternal
    {
        private static byte[] defaultEncryptionKey;

        private static byte[] communicationEncryptionKey;
        private static byte[] communicationEncryptionKeyHash;

        static GAEncryptInternal()
        {
            defaultEncryptionKey = new byte[] 
            {
                (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204,
                (Byte) 201, (Byte) 219, (Byte) 55, (Byte) 183, (Byte) 156, (Byte) 64, (Byte) 85, (Byte) 204 
            };

            CommunicationEncryptionKey = defaultEncryptionKey;
        }

        public static byte[] CommunicationEncryptionKey
        {
            get
            {
                return communicationEncryptionKey;
            }
            set
            {
                communicationEncryptionKey = value;
                communicationEncryptionKeyHash = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.SHA1, new byte[0]).ComputeHash(communicationEncryptionKey);
            }
        }

        public static string Encrypt(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
            return en.EncryptBytesWithBase64(buffer);
        }

        public static string Decrypt(string secureValue)
        {
            IEncryption en = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, communicationEncryptionKey, communicationEncryptionKeyHash);
            byte[] result = en.DecryptBytesWithBase64(secureValue);
            return Encoding.UTF8.GetString(result);
        }
    }
}
