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





using System.Security;

namespace AvePoint.GCommon.Utility.Cryptography.Encryption
{
    #region using directives
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    /// <summary>
    /// Represents a abstract implements of the IEncryption interface
    /// </summary>
    /// <example> Usage of the DocAve GCommon Utility IEncryption module
    /// <code>
    ///    public static void Test()
    ///    {
    ///        IEncryption e1 = EncryptionFactory.GetEncryption("blowfish");
    ///        string hello = "Hello,World";
    ///        byte[] bts = e1.EncodeString(hello);
    ///        string hello1 = e1.DecodeString(bts);
    ///        Console.WriteLine(string.Compare(hello, hello1, StringComparison.Ordinal) == 0);
    ///
    ///        IEncryption e2 = EncryptionFactory.GetEncryption("aes");
    ///        hello = "Hello,World";
    ///        bts = e2.EncodeString(hello);
    ///        hello1 = e2.DecodeString(bts);
    ///        Console.WriteLine(string.Compare(hello, hello1, StringComparison.Ordinal) == 0);
    ///    }
    /// </code>
    /// </example>
    public abstract class AbstractEncryption : IEncryption
    {
        protected SymmetricAlgorithm Crypto
        {
            get;
            set;
        }


        #region util method


        internal void SetKeyAndIV(byte[] key)
        {
            if (key == null)
            {
                Crypto.GenerateKey();
            }
            else
            {
                byte[] keyBytes = key;
                Crypto.Key = keyBytes;

            }

            Crypto.GenerateIV();
        }


        public byte[] EncryptBinary(byte[] data)
        {
            return EncryptBinary(data, 0, data.Length);
        }

        public byte[] EncryptBinary(Byte[] data, Int32 start, Int32 length)
        {
            using (var result = new MemoryStream())
            {

                var stream = CreateEncryptWithIVStream(result, CryptoStreamMode.Write);
                stream.Write(data, start, length);
                stream.Close();
                return result.ToArray();
            }
        }

        public Byte[] DecryptBinary(Byte[] data)
        {
            return DecryptBinary(data, 0, data.Length);
        }

        public Byte[] DecryptBinary(Byte[] data, Int32 start, Int32 count)
        {
            using (var result = new MemoryStream())
            {
 
                var stream = CreateDecryptWithIVStream(result, CryptoStreamMode.Write);
                stream.Write(data, start, data.Length);
                
                stream.Close();
                return result.ToArray();
            }
        }



        public string EncryptStringWithBase64(SecureString plainString)
        {
            if (plainString == null || plainString.Length == 0)
            {
                return "";
            }

            return Convert.ToBase64String(EncryptString(plainString));
        }


        public byte[] EncryptString(SecureString plainString)
        {
            if (plainString == null || plainString.Length == 0)
            {
                return new byte[0];
            }

            byte[] buf = CryptoUtil.ConvertSecureStringToBytes(plainString);
            byte[] blowfishEncrypted = EncryptBinary(buf);
            Array.Clear(buf, 0, buf.Length);
            return blowfishEncrypted;
        }

        public virtual SecureString DecryptString(String encryptedString)
        {

            if (string.IsNullOrEmpty(encryptedString))
            {
                SecureString sString = new SecureString();
                sString.MakeReadOnly();
                return sString;
            }

            byte[] base64Decrypted = Convert.FromBase64String(encryptedString);
            return DecryptString(base64Decrypted);
        }

        public SecureString DecryptString(byte[] encryptedByte)
        {
            SecureString sString = new SecureString();

            if (encryptedByte == null || encryptedByte.Length <= 0)
            {
                sString.MakeReadOnly();
                return sString;
            }
            byte[] decrytedBytes = DecryptBinary(encryptedByte);
            char[] decrytedChars = Encoding.UTF8.GetChars(decrytedBytes);

            foreach (char decrytedChar in decrytedChars)
            {
                sString.AppendChar(decrytedChar);
            }

            sString.MakeReadOnly();
            return sString;
        }


        public CryptoStream CreateEncryptStream(Stream stream, CryptoStreamMode mode)
        {
            return CreateEncryptStream(stream, mode, null);
        }

        public CryptoStream CreateEncryptStream(Stream stream, CryptoStreamMode mode, string key = null)
        {
            if (key == null)
            {

                return new CryptoStream(stream, Crypto.CreateEncryptor(), mode);
            }
            else
            {
                return new CryptoStream(stream, Crypto.CreateEncryptor(CspCommunicationWrapper.UnWrapKey(key), Crypto.IV), mode);
            }
        }


        public CryptoStream CreateDecryptStream(Stream stream, CryptoStreamMode mode, string key = null)
        {
            if (key == null)
            {
                return new CryptoStream(stream, Crypto.CreateDecryptor(), mode);
            }
            else
            {
                return new CryptoStream(stream, Crypto.CreateDecryptor(CspCommunicationWrapper.UnWrapKey(key), Crypto.IV), mode);

            }
        }


        public CryptoWithIVStream CreateEncryptWithIVStream(Stream stream, CryptoStreamMode mode, string key = null)
        {
            return new CryptoWithIVStream(stream, this, EncryptionMode.ENCRYPTION, mode, key);
        }



        public CryptoWithIVStream CreateDecryptWithIVStream(Stream stream, CryptoStreamMode mode, string key = null)
        {
            return new CryptoWithIVStream(stream, this, EncryptionMode.DECRYPTION, mode, key);
        }

        #endregion


        #region ICryptography Members

        public abstract CryptoMode FipsMode
        {
            get;
        }

        #endregion

        #region IEncryption Members

        public Byte[] Key
        {
            get
            {
                return Crypto.Key;
            }

        }


  

        public int CurrentKeySize
        {
            get { return Crypto.KeySize; }
        }

        public int CurrentBlockSize
        {
            get { return Crypto.BlockSize; }
        }

        public virtual KeySizes[] SupportedKeySizes
        {
            get { return Crypto.LegalKeySizes; }
        }

        public virtual KeySizes[] SupportedBlockSizes
        {
            get { return Crypto.LegalBlockSizes; }
        }

        #endregion

        #region IEncryption Members


        public Byte[] IV
        {
            get { return Crypto.IV; }
            set { Crypto.IV = value; }
        }

        #endregion

        #region IEncryption Members


        public string EncryptBytesWithBase64(byte[] data)
        {
            byte[] resultBytes = this.EncryptBinary(data);
            return Convert.ToBase64String(resultBytes);
        }

        public byte[] DecryptBytesWithBase64(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            return this.DecryptBinary(bytes); ;
        }

        #endregion

        #region IEncryption Members


        public byte[] GetTestData()
        {
            return Encoding.UTF8.GetBytes("DocAve Encryption Test Data");
        }

      
        #endregion


        public void GenerateIV()
        {
            Crypto.GenerateIV();
        }
    }
}
