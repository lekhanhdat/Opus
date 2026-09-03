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
using System.Reflection;
using AvePoint.GCommon;

namespace AvePoint.Common
{
    [AveVersion("$Revision: 253196 $")]
    public class AveCrypto
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const int DATA_BLOCK_SIZE = 64 * 1024;
        const int BLOCK_SIZE = 8;
        public DESCryptoServiceProvider mCryptoProvider;
        ICryptoTransform mEncryptor;
        ICryptoTransform mDecryptor;
        byte[] mBuffer;
        int mBufferSize;
        int mBufferPosit;
        int bugFix;
        byte[] mLastData;
        int mLastLen;

        public AveCrypto()
        {
            mCryptoProvider = new DESCryptoServiceProvider();
            //mCryptoProvider.Mode = CipherMode.ECB;
            //mCryptoProvider.Padding = PaddingMode.PKCS7;
            mBuffer = new byte[DATA_BLOCK_SIZE + BLOCK_SIZE];
            mLastData = new byte[BLOCK_SIZE];
            mLastLen = 0;
            //			byte[] Key = System.Text.Encoding.ASCII.GetBytes(s1);
            //			byte[] IV = System.Text.Encoding.ASCII.GetBytes(s2);
            byte[] Key = { 15, 218, 43, 167, 98, 156, 234, 134 };
            byte[] IV = { 145, 138, 67, 7, 198, 56, 224, 113 };
            mEncryptor = mCryptoProvider.CreateEncryptor(Key, IV);
            mDecryptor = mCryptoProvider.CreateDecryptor(Key, IV);
            bugFix = 0;
        }

        public int GetDataBlockSize()
        {
            return DATA_BLOCK_SIZE;
        }

        void Initialize(byte[] Key, byte[] IV)
        {
            mEncryptor = mCryptoProvider.CreateEncryptor(Key, IV);
            mDecryptor = mCryptoProvider.CreateDecryptor(Key, IV);
            mLastLen = 0;
            mBufferSize = 0;
            mBufferPosit = 0;
        }

        // The implement class always has a default key
        //Set key from a byte array
        public void SetKey(byte[] key)
        {
            byte[] Key = new byte[8];
            byte[] IV = new byte[8];
            int i;
            for (i = 0; i < 8 && i < key.Length; i++)
            {
                Key[i] = key[i];
            }
            for (; i < 8; i++)
                Key[i] = 0;
            for (i = 8; i < 16 && i < key.Length; i++)
            {
                IV[i - 8] = key[i];
            }
            for (; i < 16; i++)
                IV[i - 8] = 0;
            Initialize(Key, IV);
        }

        //Set key from a uuencoded string
        public void SetEncodedKey(string key)
        {
            byte[] t = System.Convert.FromBase64String(key);
            SetKey(t);
        }

        //Generate the key from a password string
        //Use SHA-1 to generate the key from the pwd
        public void GenerateKey(string pwd)
        {
            //TODO:Get SHA-1 for pwd and make a byte array, then call SetKey()
            byte[] t = System.Text.Encoding.UTF8.GetBytes(pwd);
            SetKey(t);
        }

        //Encrypt a string and return a uuencoded string
        //We can use this for password encryption
        //and control communication message encryption
        public string Encrypt(string message)
        {
            mLastLen = 0;
            try
            {
                int len = System.Text.UTF8Encoding.UTF8.GetBytes(message, 0, message.Length, mBuffer, 0);
                int rlen = len / BLOCK_SIZE * BLOCK_SIZE;
                mBufferPosit = 0;
                mBufferSize = 0;
                int i;
                for (i = 0; i < rlen; i += BLOCK_SIZE)
                {
                    mEncryptor.TransformBlock(mBuffer, i, BLOCK_SIZE, mBuffer, i);
                }
                byte[] buf = mEncryptor.TransformFinalBlock(mBuffer, i, len - rlen);
                Array.Copy(buf, 0, mBuffer, i, buf.Length);
                i += buf.Length;
                return System.Convert.ToBase64String(mBuffer, 0, i);
            }
            catch (System.Exception e)
            {
                mLog.Error(string.Format("Decrypt Error: {0}", e.ToString()));
                return null;
            }
        }

        //The reverse of the Encrypt
        public string Decrypt(string message)
        {
            if (message == null)
                return null;
            if (message == "")
                return "";

            mLastLen = 0;
            try
            {
                byte[] temp = System.Convert.FromBase64String(message);
                int len = temp.Length;
                int rlen = len / BLOCK_SIZE * BLOCK_SIZE;
                mBufferPosit = 0;
                mBufferSize = 0;
                int i;
                for (i = 0; i < rlen; i += BLOCK_SIZE)
                {
                    int bug = 0;
                    if (i != 0)
                        bug = i - BLOCK_SIZE;
                    mDecryptor.TransformBlock(temp, i, BLOCK_SIZE, mBuffer, bug);
                }
                //string str = System.Text.Encoding.UTF8.GetString(mBuffer,0,i);
                byte[] buf = mDecryptor.TransformFinalBlock(temp, i, len - rlen);
                Array.Copy(buf, 0, mBuffer, i - BLOCK_SIZE, buf.Length);
                i += buf.Length;
                return System.Text.Encoding.UTF8.GetString(mBuffer, 0, i - BLOCK_SIZE);
            }
            catch (System.Exception e)
            {
                mLog.Error(string.Format("Decrypt Error:{0}", e.ToString()));
                return null;
            }
        }

        /// <summary>
        ///Encrypt a data buffer
        ///The encrypted data will write into mBuffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns>The length of the encrypted data</returns>
        public void EncryptData(byte[] data, int startindex, int length)
        {
            if (length + mLastLen < BLOCK_SIZE)
            {
                //Array.Copy(data,startindex,mLastData,mLastLen,mEncryptor.InputBlockSize-mLastLen);
                Array.Copy(data, startindex, mLastData, mLastLen, length);
                mLastLen += length;
                return;
            }
            if (mLastLen > 0)
            {
                Array.Copy(data, startindex, mLastData, mLastLen, BLOCK_SIZE - mLastLen);
                mEncryptor.TransformBlock(mLastData, 0, BLOCK_SIZE, mBuffer, mBufferSize);
                // DES always return 8==BLOCK_SIZE
                mBufferSize += BLOCK_SIZE;
                startindex += BLOCK_SIZE - mLastLen;
                length -= BLOCK_SIZE - mLastLen;
                mLastLen = 0;
            }
            int n;
            int rlen = length / BLOCK_SIZE * BLOCK_SIZE;
            n = rlen + startindex;
            //		mEncryptor.TransformBlock(data,startindex,n - startindex ,mBuffer,mBufferSize);
            //		mBufferSize += n - startindex;
            for (; startindex < n; startindex += BLOCK_SIZE)
            {
                mEncryptor.TransformBlock(data, startindex, 8, mBuffer, mBufferSize);
                mBufferSize += BLOCK_SIZE;
            }
            if (rlen < length)
            {
                //save the data that is not big enough for a block
                mLastLen = length - rlen;
                Array.Copy(data, startindex, mLastData, 0, mLastLen);
            }
        }

        //End the encryption of the data
        public void EndEncryptData()
        {
            byte[] buf = mEncryptor.TransformFinalBlock(mLastData, 0, mLastLen);
            Array.Copy(buf, 0, mBuffer, mBufferSize, buf.Length);
            mBufferSize += buf.Length;
            buf = null;
            mLastLen = 0;
        }

        //Read the encrypted data
        //If GetEncryptData return 0, after call the EndEncryptData
        //It means it reads all encrypted data.
        //Each time, we encrypt almost DATA_BLOCK_SIZE data buffer
        //Then we read all the encrypted data out until no data can read
        public int GetEncryptData(byte[] buf, int startindex, int maxsize)
        {
            if (mBufferSize - mBufferPosit > maxsize)
            {
                Array.Copy(mBuffer, mBufferPosit, buf, startindex, maxsize);
                mBufferPosit += maxsize;
                return maxsize;
            }
            if (mBufferSize == mBufferPosit)
                return 0;
            maxsize = mBufferSize - mBufferPosit;
            Array.Copy(mBuffer, mBufferPosit, buf, startindex, maxsize);
            mBufferPosit = 0;
            mBufferSize = 0;
            return maxsize;
        }

        //Deencrypt a data buffer
        public void DecryptData(byte[] data, int startindex, int length)
        {
            if (length + mLastLen < BLOCK_SIZE)
            {
                //Array.Copy(data,startindex,mLastData,mLastLen,mDecryptor.InputBlockSize-mLastLen);
                Array.Copy(data, startindex, mLastData, mLastLen, length);
                mLastLen += length;
                return;
            }
            if (mLastLen > 0)
            {
                Array.Copy(data, startindex, mLastData, mLastLen, BLOCK_SIZE - mLastLen);
                mDecryptor.TransformBlock(mLastData, 0, BLOCK_SIZE, mBuffer, mBufferSize);
                // DES always return 8==BLOCK_SIZE

                if (bugFix == 1)
                    mBufferSize += BLOCK_SIZE;
                bugFix = 1;
                //mBufferSize+=BLOCK_SIZE;
                startindex += BLOCK_SIZE - mLastLen;
                length -= BLOCK_SIZE - mLastLen;
                mLastLen = 0;
            }
            int n;
            int rlen = length / BLOCK_SIZE * BLOCK_SIZE;
            n = rlen + startindex;
            //			mDecryptor.TransformBlock(data,startindex,n - startindex ,mBuffer,mBufferSize);
            //			mBufferSize += n - startindex;
            for (; startindex < n; startindex += BLOCK_SIZE)
            {
                mDecryptor.TransformBlock(data, startindex, 8, mBuffer, mBufferSize);
                if (bugFix == 1)
                    mBufferSize += BLOCK_SIZE;
                bugFix = 1;
            }
            if (rlen < length)
            {
                //save the data that is not big enough for a block
                mLastLen = length - rlen;
                Array.Copy(data, startindex, mLastData, 0, mLastLen);
            }
        }

        //End the Decryption of the data
        public void EndDecryptData()
        {
            byte[] buf = mDecryptor.TransformFinalBlock(mLastData, 0, mLastLen);
            Array.Copy(buf, 0, mBuffer, mBufferSize, buf.Length);
            mBufferSize += buf.Length;
            buf = null;
            mLastLen = 0;
        }

        //Read the Decrypted data
        //If GetDecryptData return 0, after call the EndDecryptData
        //It means it reads all encrypted data.
        //Each time, we encrypt almost DATA_BLOCK_SIZE data buffer
        //Then we read all the decrypted data out until no data can read
        public int GetDecryptData(byte[] buf, int startindex, int maxsize)
        {
            if (mBufferSize - mBufferPosit > maxsize)
            {
                Array.Copy(mBuffer, mBufferPosit, buf, startindex, maxsize);
                mBufferPosit += maxsize;
                return maxsize;
            }
            if (mBufferSize == mBufferPosit)
                return 0;
            maxsize = mBufferSize - mBufferPosit;
            Array.Copy(mBuffer, mBufferPosit, buf, startindex, maxsize);
            mBufferPosit = 0;
            mBufferSize = 0;
            return maxsize;
        }
    }
}
