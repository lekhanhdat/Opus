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
using System.IO;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.Encryption;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class EncryptionFilteringBox : IDataFilteringBox
    {
        public const int KB = 1024;
        public const int ENCRYPT_BUFFER_SIZE = 128 * KB;

        private IEncryption mEncryption;
        private bool isEncryption;
        private EncryptionAlgorithm mAlg;
        private byte[] mInnerBuffer = new byte[ENCRYPT_BUFFER_SIZE];
        private bool mIsBegining;
        private int mCurPos;
        private int mAvailSize;
        CryptoWithIVStream mEncryptionStream;
        private bool mIsEnd;
        private MemoryPipeStream mMemoryPipeStream = new MemoryPipeStream(512 * KB);
        private string mKey;

        public EncryptionFilteringBox(IEncryption encryption, bool isEncryption = true)
        {
            
            //this.mEncryption = encryption;
            this.isEncryption = isEncryption;
        }

        public EncryptionFilteringBox(bool isEncryption = true, string _key = null, EncryptionAlgorithm alg = EncryptionAlgorithm.AES_ENCRYPTION)
        {
            mKey = _key;
            mAlg = alg;
            this.isEncryption = isEncryption;
        }

        #region IOutputFilteringBox Members


        private void InitEncryptionStream(Stream stream) 
        {
            mEncryption.GenerateIV();
            if (isEncryption)
            {
                this.mEncryptionStream = mEncryption.CreateEncryptWithIVStream(stream, System.Security.Cryptography.CryptoStreamMode.Write, mKey);
            }
            else
            {
                this.mEncryptionStream = mEncryption.CreateDecryptWithIVStream(stream, System.Security.Cryptography.CryptoStreamMode.Write, mKey);
            }
        }


        public void InputBegin(string key, EncryptionAlgorithm alg)
        {
            mKey = key;
            if (mEncryption == null || mAlg != alg)
            {
                mEncryption = EncryptionFactory.GetEncryption(alg, mKey);
                mAlg = alg;
            }
            this.mIsBegining = true;
            this.mIsEnd = false;
            this.mCurPos = 0;
            this.mAvailSize = ENCRYPT_BUFFER_SIZE;
            this.mMemoryPipeStream.Reset();
        }


        public void InputBegin()
        {

            if (mEncryption == null)
            {
                mEncryption = EncryptionFactory.GetEncryption(mAlg, mKey);
            }
            this.mIsBegining = true;
            this.mIsEnd = false;
            this.mCurPos = 0;
            this.mAvailSize = ENCRYPT_BUFFER_SIZE;
            this.mMemoryPipeStream.Reset();
        }

        public void Input(byte[] data, int offset, int count)
        {
            if (count > 64 * KB)
            {
                throw new Exception("max input unit: 64KB");
            }
            if (mIsBegining)
            {
                if (mAvailSize > count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, count);
                    mCurPos += count;
                    mAvailSize -= count;
                    return;
                }
                else if (mAvailSize == count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, mAvailSize);
                    InitEncryptionStream(mMemoryPipeStream);
                    mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mEncryptionStream.Flush();
                    mCurPos = 0;
                    count = 0;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }
                else if (mAvailSize < count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, mAvailSize);
                    InitEncryptionStream(mMemoryPipeStream);
                    mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mEncryptionStream.Flush();
                    mCurPos = 0;
                    count -= mAvailSize;
                    offset += mAvailSize;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;

                    while (mAvailSize <= count)
                    {
                        Array.Copy(data, offset, mInnerBuffer, mCurPos, mAvailSize);
                        mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                        mEncryptionStream.Flush();
                        mCurPos = 0;
                        count -= mAvailSize;
                        offset += mAvailSize;
                        mAvailSize = ENCRYPT_BUFFER_SIZE;
                    }
                }

                mIsBegining = false;
            }
            else
            {
                if (mAvailSize > count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, count);
                    mCurPos += count;
                    mAvailSize -= count;
                    return;
                }
                else if (mAvailSize == count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, mAvailSize);
                    mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mEncryptionStream.Flush();
                    mCurPos = 0;
                    count = 0;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }

                while (mAvailSize < count)
                {
                    Array.Copy(data, offset, mInnerBuffer, mCurPos, mAvailSize);
                    mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mEncryptionStream.Flush();
                    mCurPos = 0;
                    offset += mAvailSize;
                    count -= mAvailSize;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }
            }

            if (count != 0)
            {
                Array.Copy(data, offset, mInnerBuffer, mCurPos, count);
                mCurPos += count;
                mAvailSize -= count;
                count = 0;
            }

           
        }

        public void InputEnd()
        {

            if (this.mIsEnd == false)
            {
                if (mIsBegining)
                {
                    InitEncryptionStream(mMemoryPipeStream);
                }

                mEncryptionStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE - mAvailSize);
                mEncryptionStream.FlushFinalBlock();
                mEncryptionStream.Close();

                this.mMemoryPipeStream.FinishWrite();
                this.mIsEnd = true;
            }

        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {

            return mMemoryPipeStream.Read(data, offset, count);
        }

        public void Dispose()
        {
            if(mMemoryPipeStream != null)
            {
                mMemoryPipeStream.Dispose();
            }
        }

        #endregion
    }
}
