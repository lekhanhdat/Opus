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
using AvePoint.GCommon.BlowFish;
using AvePoint.GCommon.Utility.Encryption;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class DecryptionFilteringBox : IDataFilteringBox
    {
        public const int ENCRYPT_BUFFER_SIZE = 128 * 1024;

        private IEncryption mEncryption;

        private byte[] mInnerBuffer = new byte[ENCRYPT_BUFFER_SIZE];
        private bool mIsBegining;
        private int mCurPos;
        private int mAvailSize;

        private MemoryPipeStream mMemoryPipeStream = new MemoryPipeStream(512 * FilteringBoxConstants.KB);

        public DecryptionFilteringBox(IEncryption encryption)
        {
            this.mEncryption = encryption;
        }

        #region IOutputFilteringBox Members

        public void InputBegin()
        {
            this.mIsBegining = true;
            this.mCurPos = 0;
            this.mAvailSize = ENCRYPT_BUFFER_SIZE;
            this.mMemoryPipeStream.Reset();
        }

        public void Input(byte[] buffer, int offset, int count)
        {
            if (count > 64 * FilteringBoxConstants.KB)
            {
                throw new Exception("max input unit: 64KB");
            }
            if (mIsBegining)
            {
                if (mAvailSize > count)
                {
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, count);
                    mCurPos += count;
                    mAvailSize -= count;
                    return;
                }
                else if (mAvailSize == count)
                {
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, mAvailSize);
                    byte[] encryptedBuffer = mEncryption.DecryptBinaryBeginning(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    this.mMemoryPipeStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
                    mCurPos = 0;
                    count = 0;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }
                else if (mAvailSize < count)
                {
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, mAvailSize);
                    byte[] encryptedBuffer = mEncryption.DecryptBinaryBeginning(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    this.mMemoryPipeStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
                    mCurPos = 0;
                    count -= mAvailSize;
                    offset += mAvailSize;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;

                    while (mAvailSize <= count)
                    {
                        Array.Copy(buffer, offset, mInnerBuffer, mCurPos, mAvailSize);
                        mEncryption.DecryptBinaryBody(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                        this.mMemoryPipeStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
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
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, count);
                    mCurPos += count;
                    mAvailSize -= count;
                    return;
                }
                else if (mAvailSize == count)
                {
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, mAvailSize);
                    mEncryption.DecryptBinaryBody(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    this.mMemoryPipeStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mCurPos = 0;
                    count = 0;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }

                while (mAvailSize < count)
                {
                    Array.Copy(buffer, offset, mInnerBuffer, mCurPos, mAvailSize);
                    mEncryption.DecryptBinaryBody(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    this.mMemoryPipeStream.Write(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE);
                    mCurPos = 0;
                    offset += mAvailSize;
                    count -= mAvailSize;
                    mAvailSize = ENCRYPT_BUFFER_SIZE;
                }
            }

            if (count != 0)
            {
                Array.Copy(buffer, offset, mInnerBuffer, mCurPos, count);
                mCurPos += count;
                mAvailSize -= count;
                count = 0;
            }
        }

        public void InputEnd()
        {
            byte[] encryptedBuffer;
            if (mIsBegining)
            {
                encryptedBuffer = mEncryption.DecryptBinary(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE - mAvailSize);
            }
            else
            {
                encryptedBuffer = mEncryption.DecryptBinaryTail(mInnerBuffer, 0, ENCRYPT_BUFFER_SIZE - mAvailSize);
            }

            this.mMemoryPipeStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
            this.mMemoryPipeStream.FinishWrite();
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            return mMemoryPipeStream.Read(data, offset, count);
        }

        #endregion
    }
}
