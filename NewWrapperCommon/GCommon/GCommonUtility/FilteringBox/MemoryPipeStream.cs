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
using System.Threading;
using System.IO;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class MemoryPipeStream : Stream
    {
        private int mReadPos;
        private int mWritePos;
        private int available;
        private int mCapacity;
        private byte[] mMemoryBuffer;
        private bool mFinishedWrite;

        public MemoryPipeStream(int capacity)
        {
            mCapacity = capacity;
            mMemoryBuffer = new byte[mCapacity];
            Reset();
        }

        public void Reset()
        {
            mReadPos = 0;
            mWritePos = 0;
            available = 0;
            mFinishedWrite = false;
        }

        //上层逻辑要控制是否有足够的空间可以写入，如果空间不足，写入时会抛出异常
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > mCapacity)
            {
                throw new Exception("length > mCapacity");
            }
            if (available + count > mCapacity)
            {
                throw new Exception("Available + length > mCapacity");
            }
            if (mWritePos + count > mCapacity)
            {
                Array.Copy(buffer, offset, mMemoryBuffer, mWritePos, mCapacity - mWritePos);
                Array.Copy(buffer, offset + (mCapacity - mWritePos), mMemoryBuffer, 0, count - (mCapacity - mWritePos));
            }
            else
            {
                Array.Copy(buffer, offset, mMemoryBuffer, mWritePos, count);
            }
            mWritePos = (mWritePos + count) % mCapacity;
            Interlocked.Add(ref available, count);
        }

        //返回-1代表结束;返回0只是目前没有可读数据，并不代表结束,此函数不等待
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (available == 0)
            {
                if (mFinishedWrite)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            int readLen = 0;
            if (available >= count)
            {
                readLen = count;
            }
            else
            {
                readLen = available;
            }
            if (mReadPos + readLen > mCapacity)
            {
                Array.Copy(mMemoryBuffer, mReadPos, buffer, offset, mCapacity - mReadPos);
                Array.Copy(mMemoryBuffer, 0, buffer, offset + (mCapacity - mReadPos), readLen - (mCapacity - mReadPos));
            }
            else
            {
                Array.Copy(mMemoryBuffer, mReadPos, buffer, offset, readLen);
            }
            mReadPos = (mReadPos + readLen) % mCapacity;
            Interlocked.Add(ref available, 0 - readLen);
            return readLen;
        }

        public void FinishWrite()
        {
            mFinishedWrite = true;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
