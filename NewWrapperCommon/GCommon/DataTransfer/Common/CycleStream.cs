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
using AvePoint.GCommon.Transfer.Data.Interface;

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 一个可重用的内存池定义
    /// 池是一个循环内存队列，可以反复使用，提高内存利用率，并且可以在写入和读取的时候进行数据流量的控制。
    /// </summary>
    public class CycleStream : IDisposable
    {
        private int mReadPos; //当前读取数据的位置
        private int mWritePos;//当前可以写入的位置
        private int mCanReadBuffer;//可以被读取的内存字节数
        private int mCapacity;//默认内存池的容量
        private byte[] mMemoryBuffer; //实际的内存池
        private bool mFinishedWrite;  //是否已经完成写入
        private int mWriteBufferSize = 64 * 1024;//每一次写入的数据大小，用于提供内部拆分，减少调用这拆分的困难，因为内存池容量有限
        private long readLength = 0L;
        private long writeLength = 0L;
        //private object lockObj = new object();
        private AutoResetEvent autoResetEventForWrite = null;
        private AutoResetEvent autoResetEventForRead = null;
        //private ManualResetEvent resetEvent = null;
        private DataTransferCommonDelegate readTimeoutDelegate;
        private DataTransferCommonDelegate writeTimeoutDelegate;
        private CommonPerformanceTimerPool performanceTimerPool;
        /// <summary>
        /// is stopped
        /// </summary>
        private bool isStopped;
        /// <summary>
        /// stop message
        /// </summary>
        private string stopMessage;

        public long ReadLength
        {
            get { return readLength; }
        }

        public long WriteLength
        {
            get { return writeLength; }
        }

        public bool IsWriteFinish
        {
            get { return mFinishedWrite; }
        }

        public bool IsStopped
        {
            get { return isStopped; }
        }

        /// <summary>
        /// 存储数据的最大大小
        /// </summary>
        public int Capacity
        {
            get { return mCapacity; }
        }

        public int Length
        {
            get { return mCanReadBuffer; }
        }

        public DataTransferCommonDelegate ReadTimeoutDelegate
        {
            get { return readTimeoutDelegate; }
            set { readTimeoutDelegate = value; }
        }

        public DataTransferCommonDelegate WriteTimeoutDelegate
        {
            get { return writeTimeoutDelegate; }
            set { writeTimeoutDelegate = value; }
        }

        public CommonPerformanceTimerPool PerformanceTimerPool
        {
            get { return performanceTimerPool; }
            set { performanceTimerPool = value; }
        }

        #region inner function
        /// <summary>
        /// 重置所有内存池的相应参数
        /// </summary>
        internal void Reset()
        {
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("Reset cycle stream", true);
            }

            isStopped = false;
            stopMessage = null;
            mReadPos = 0;
            mWritePos = 0;
            mCanReadBuffer = 0;
            mMemoryBuffer = new byte[mCapacity];
            double tempSize = mCapacity / 2;
            mWriteBufferSize = mWriteBufferSize < mCapacity ? mWriteBufferSize : Convert.ToInt32(Math.Round(tempSize));
            mFinishedWrite = false;
            if (autoResetEventForWrite == null)
            {
                autoResetEventForWrite = new AutoResetEvent(false);
            }
            else
            {
                autoResetEventForWrite.Reset();
            }

            if (autoResetEventForRead == null)
            {
                autoResetEventForRead = new AutoResetEvent(false);
            }
            else
            {
                autoResetEventForRead.Reset();
            }

            readTimeoutDelegate = null;
            writeTimeoutDelegate = null;

            if (performanceTimerPool != null)
            {
                performanceTimerPool.Action("Reset cycle stream", false);
            }
        }
        /// <summary>
        /// 内部方法，将数据实际写入buffer池中
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private void Write(byte[] buf, int offset, int length)
        {
            if (length > mCapacity)
            {
                throw new Exception("length > mCapacity");
            }
            if(mFinishedWrite)
            {
                throw new Exception("Finished Write.");
            }
            CheckStopStatus();
            //判断在内存池中存在可用的内存空间是否能够装下当前的buffer
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("WriteFull", true);
            }
            while (mCanReadBuffer + length > mCapacity)
            {
                if (mFinishedWrite)
                {
                    throw new Exception("Finished Write.");
                }
                else
                {
                    if (!autoResetEventForWrite.WaitOne(200))
                    {
                        if (writeTimeoutDelegate != null)
                        {
                            writeTimeoutDelegate();
                        }
                    }
                }
                CheckStopStatus();
            }

            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("WriteFull", false);
                performanceTimerPool.Action("SafeWrite", true);
            }

            if (mWritePos + length > mCapacity)
            {
                //在循环池中的位置加上待写buffer的长度，需要进行分段写入循环池中，
                //险些一部分数据到循环池的尾部
                Array.Copy(buf, offset, mMemoryBuffer, mWritePos, mCapacity - mWritePos);
                //之后将剩余部分从循环池的头部开始写入
                Array.Copy(buf, offset + (mCapacity - mWritePos), mMemoryBuffer, 0, length - (mCapacity - mWritePos));
            }
            else
            {
                //存在整段内存可以使用直接写入
                Array.Copy(buf, offset, mMemoryBuffer, mWritePos, length);
            }
            Interlocked.Add(ref mCanReadBuffer, length);
            autoResetEventForRead.Set();
            mWritePos = (mWritePos + length) % mCapacity;//更新下一次可以在内存池中写入数据的的位置
            writeLength += length;
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("SafeWrite", false);
            }
        }
        #endregion

        public CycleStream(int capacity)
        {
            mCapacity = capacity;
            Reset();
        }

        #region 对外提供的操作接口
        /// <summary>
        /// 将数据写入buffer池中，如果池中满了，该函数会被阻塞，直到读取完全部需要的内存
        /// </summary>
        /// <param name="buf">准备写入的buffer</param>
        /// <param name="offset">写入的起始位置</param>
        /// <param name="length">写入的数据大小</param>
        public void SafeWrite(byte[] buf, int offset, int length)
        {
            while (length > 0)
            {
                if (mWriteBufferSize > length)
                {
                    Write(buf, offset, length);
                    length = 0;
                }
                else
                {
                    Write(buf, offset, mWriteBufferSize);
                    offset += mWriteBufferSize;
                    length -= mWriteBufferSize;
                }
            }
        }
        /// <summary>
        /// 从Buffer池中读取指定大小的buffer
        /// </summary>
        /// <param name="buf">需要读取的数据块</param>
        /// <param name="offset">写入读取buffer的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <returns>读取的数据长度</returns>
        public int Read(byte[] buf, int offset, int length)
        {
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("ReadEmptyInCycleStream", true);
            }
            while (mCanReadBuffer == 0)
            {
                if (mFinishedWrite)
                {
                    if (mCanReadBuffer != 0)
                    {
                        break;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (!autoResetEventForRead.WaitOne(200))
                    {
                        if (readTimeoutDelegate != null)
                        {
                            readTimeoutDelegate();
                        }
                    }
                }
                CheckStopStatus();
            }

            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("ReadEmptyInCycleStream", false);
                performanceTimerPool.Action("ReadDataInCycleStream", true);
            }

            int readLen = 0;
            var canReadLength = mCanReadBuffer;
            if (canReadLength >= length)
            {
                readLen = length;
            }
            else
            {
                readLen = canReadLength;
            }
            if (mReadPos + readLen > mCapacity)
            {
                //如果已经读取到了尾部，那么需要分段读取
                //先读取尾部的内容
                Array.Copy(mMemoryBuffer, mReadPos, buf, offset, mCapacity - mReadPos);
                //在从头部读取剩余的内容
                Array.Copy(mMemoryBuffer, 0, buf, offset + (mCapacity - mReadPos), readLen - (mCapacity - mReadPos));
            }
            else
            {
                //可读数据足够，所以直接读取
                Array.Copy(mMemoryBuffer, mReadPos, buf, offset, readLen);
            }
            Interlocked.Add(ref mCanReadBuffer, 0 - readLen);//重新设置可读数据
            autoResetEventForWrite.Set();
            mReadPos = (mReadPos + readLen) % mCapacity;     //更新读取位置
            readLength += readLen;
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("ReadDataInCycleStream", false);
            }
            return readLen;
        }

        public int SafeRead(byte[] buf, int offset, int length, bool throwExceptionIfNoBuffer = true)
        {
            int readLen = 0;
            while (readLen < length)
            {
                var currentReadLen = Read(buf, offset + readLen, length - readLen);
                if (currentReadLen != 0)
                {
                    readLen += currentReadLen;
                }
                else
                {
                    if (throwExceptionIfNoBuffer)
                    {
                        throw new Exception("There is no enough buffer for you to read.");
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return readLen;
        }

        public void FinishWrite()
        {
            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("Finish Write", true);
            }

            autoResetEventForRead.Set();
            autoResetEventForWrite.Set();
            mFinishedWrite = true;

            if (performanceTimerPool != null && performanceTimerPool.IsEnable)
            {
                performanceTimerPool.Action("Finish Write", false);
            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (autoResetEventForWrite != null)
            {
                autoResetEventForWrite.Close();
                autoResetEventForWrite = null;
            }
            if (autoResetEventForRead != null)
            {
                autoResetEventForRead.Close();
                autoResetEventForRead = null;
            }
        }

        #endregion

        internal void Stop(string message)
        {
            if (!isStopped)
            {
                isStopped = true;
                stopMessage = message;
                autoResetEventForRead.Set();
                autoResetEventForWrite.Set();
            }
        }

        private void CheckStopStatus()
        {
            if(isStopped)
            {
                throw new Exception(stopMessage);
            }
        }

        public string StopMessage { get { return stopMessage; } }
    }
}
