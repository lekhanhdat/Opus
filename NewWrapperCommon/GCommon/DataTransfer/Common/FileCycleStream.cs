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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 一个可重用的文件池定义
    /// 池是一个循环文件队列，可以反复使用，减少对文件系统的使用，并且可以在写入和读取的时候进行数据流量的控制。
    /// </summary>
    public class FileCycleStream : IDisposable
    {
        private int mReadPos; //当前读取数据的Cache文件Location
        private int mWritePos;//当前可以写入的Cache文件Location
        private int mCanReadBuffer;//可以被读取的数据字节数
        private int mCapacity;//文件缓存池的最大容量
        private string mCacheFilePath = string.Empty;//用于Cache文件的location
        private bool mFinishedWrite;  //是否已经完成写入
        private int mWriteBufferSize = 64 * 1024;//每一次写入的数据大小，用于提供内部拆分，减少调用这拆分的困难，因为内存池容量有限
        private AutoResetEvent autoResetEventForWrite = null;
        private AutoResetEvent autoResetEventForRead = null;
        private Dictionary<int, FileCycleUnit> mFileCycleCache = new Dictionary<int, FileCycleUnit>();// 文件缓存池的缓存队列，缓存队列中的文件大小并没有固定要求，因为每个文件是独立的，有自己独立的长度
        private bool resetTimeout = true;
        //private ManualResetEvent resetEvent = null;
        public delegate void CheckLogicDelegate(bool isInitial);
        private event CheckLogicDelegate mCheckLogicDelegate;//用于注册外围回调的逻辑，由于读写文件缓存池的情况下会出现等待的情况，如果写入或者读出没有数据可用的情况下，可以调用外围逻辑
        private AveLogger mLogger = AveLogger.GetInstance(typeof(FileCycleStream));
        private object mFinishWriteLock = new object();//finish write赋值之后，会Reset event，如果外围读线程操作很快，上传结束调用dispose会造成空引用，导致出现问题
        public CheckLogicDelegate CheckLogicDelegateEvent
        {
            set { this.mCheckLogicDelegate = value; }
        }

        public string CacheFilePath
        {
            get { return mCacheFilePath; }
        }

        /// <summary>
        /// 文件池中还可以写入的长度
        /// </summary>
        public int CanWriteBuffer
        {
            get { return this.mCapacity - this.mCanReadBuffer; }
        }

        /// <summary>
        /// 文件池中可以读出的数据长度
        /// </summary>
        public int CanReadBuffer
        {
            get { return mCanReadBuffer; }
        }

        /// <summary>
        /// 判断文件池是否被标记已经写入完毕
        /// </summary>
        public bool IsWriteFinish
        {
            get { return mFinishedWrite; }
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


        #region inner function
        /// <summary>
        /// 重置所有文件缓存池的相应参数
        /// </summary>
        internal void Reset()
        {
            mReadPos = 0;
            mWritePos = 0;
            mCanReadBuffer = 0;
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

        }

        private void Write(byte[] buf, int offset, int length)
        {
            if (length > mCapacity)
            {
                throw new Exception("length > mCapacity");
            }
            if (mFinishedWrite)
            {
                throw new Exception("Finished Write.");
            }
            //判断文件缓存队列是否可以写入当前的数据
            while (mCanReadBuffer + length > mCapacity)
            {
                if (mFinishedWrite)
                {
                    resetTimeout = true;
                    throw new Exception("Finished Write.");
                }
                else
                {
                    if (!autoResetEventForWrite.WaitOne(200))
                    {
                        if (mCheckLogicDelegate != null)
                        {
                            mCheckLogicDelegate(resetTimeout);
                        }
                        resetTimeout = false;
                    }
                }
            }
            resetTimeout = true;

            #region 获取当前文件缓存池需要写入的文件单元，判断文件单元可写入的长度来决定是否需要往下个文件块中继续写入数据
            int write = 0;
            while (write < length)
            {
                FileCycleUnit unit = mFileCycleCache[mWritePos];
                int unitWriteLength = unit.CanWriteLength;
                if (unitWriteLength > (length - write))//如果当前文件块可以写入所有数据，直接将数据写入即可，不需要移到下个文件块
                {
                    unit.WriteByte(buf, offset, length - write);
                    write += (length - write);
                }
                else
                {
                    unit.WriteByte(buf, offset, unitWriteLength);//在当前文件块写入能够写入的数据
                    offset += unitWriteLength;
                    write += unitWriteLength;
                    if (unit.WriteFinishInThisCycle)//如果当前文件确实已经写入完毕了
                    {
                        mWritePos = (mWritePos + 1) % mFileCycleCache.Count;//更新写入文件池位置
                    }
                }
            }
            #endregion

            Interlocked.Add(ref mCanReadBuffer, length);
            autoResetEventForRead.Set();
        }
        #endregion

       

        public FileCycleStream(int capacity, int cacheUnitLimit)
        {
            mCapacity = capacity;
            Reset();
            EnsureCacheFileLocation();
            ResetFileCycleStream(capacity, cacheUnitLimit);
        }

        #region 对外提供的操作接口
        /// <summary>
        /// 将数据写入文件系统中，如果文件系统数据满了，该函数会被阻塞，直到写入全部需要的数据
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
        /// 从文件cache中获取指定大小的数据
        /// </summary>
        /// <param name="buf">需要读取的数据块</param>
        /// <param name="offset">写入读取buffer的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <returns>读取的数据长度</returns>
        public int Read(byte[] buf, int offset, int length)
        {
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
                        resetTimeout = true;
                        return 0;
                    }
                }
                else
                {
                    if (!autoResetEventForRead.WaitOne(200))
                    {
                        if (mCheckLogicDelegate != null)
                        {
                            mCheckLogicDelegate(resetTimeout);
                        }
                        resetTimeout = false;
                    }
                }
            }
            resetTimeout = true;
            int currentCanReadBufferLength = mCanReadBuffer;//用临时变量，避免 异步操作数据变化导致行为不一致
            int readLen = (currentCanReadBufferLength >= length) ? length : currentCanReadBufferLength;
            int read = 0;
            while (read < readLen)
            {
                FileCycleUnit unit = mFileCycleCache[mReadPos];
                int unitReadLength = unit.CanReadLength;
                if (unitReadLength > (readLen - read))
                {
                    unit.ReadByte(buf, offset, readLen - read);
                    read += (readLen - read);
                }
                else
                {
                    unit.ReadByte(buf, offset, unitReadLength);
                    offset += unitReadLength;
                    read += unitReadLength;
                    if (unit.ReadFinishInThisCycle)
                    {
                        mReadPos = (mReadPos + 1) % mFileCycleCache.Count;//更新写入文件池位置
                    }
                }
            }
            Interlocked.Add(ref mCanReadBuffer, 0 - readLen);//重新设置可读数据
            autoResetEventForWrite.Set();
            return readLen;
        }

        public int SafeRead(byte[] buf, int offset, int length, bool throwExceptionIfNoBuffer = false)
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
            lock (mFinishWriteLock)
            {
                mFinishedWrite = true;
                autoResetEventForRead.Set();
                autoResetEventForWrite.Set();
            }
        }
        #endregion

        public void Dispose()
        {
            lock (mFinishWriteLock)
            {
                try
                {
                    if (mFileCycleCache != null)
                    {
                        foreach (FileCycleUnit unit in mFileCycleCache.Values)
                        {
                            unit.Dispose();
                        }
                        if (Directory.Exists(mCacheFilePath))
                        {
                            Directory.Delete(mCacheFilePath, true);
                        }
                    }
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
                catch (Exception e)
                {
                    mLogger.Warn("Dispose file cycle stream:{0} failed, exception:{1}", mCacheFilePath, e.ToString());
                }
            }
        }

        private void ResetFileCycleStream(int capacity, int cacheUnitLimit)
        {
            foreach (FileCycleUnit unit in mFileCycleCache.Values)
            {
                unit.Dispose();
            }
            mFileCycleCache.Clear();
            for (int index = 0; ; index++)
            {
                if (capacity > cacheUnitLimit)
                {
                    FileCycleUnit unit = new FileCycleUnit(Path.Combine(mCacheFilePath, "Cache" + index + ".dat"), cacheUnitLimit);
                    capacity -= cacheUnitLimit;
                    mFileCycleCache.Add(index, unit);
                }
                else
                {
                    FileCycleUnit unit = new FileCycleUnit(Path.Combine(mCacheFilePath, "Cache" + index + ".dat"), capacity);
                    mFileCycleCache.Add(index, unit);
                    break;
                }
            }
        }

        private void EnsureCacheFileLocation()
        {
            string binPath = "\\bin\\";
            string currentExecuteLocation = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\";
            if (currentExecuteLocation.EndsWith(binPath, StringComparison.OrdinalIgnoreCase))
            {
                currentExecuteLocation = currentExecuteLocation.Substring(0, currentExecuteLocation.Length - binPath.Length);
            }
            string fileCycleStreamRootPath = Path.Combine(Path.Combine(currentExecuteLocation, "temp"), "FileCycleStream");
            if (!Directory.Exists(fileCycleStreamRootPath))
            {
                Directory.CreateDirectory(fileCycleStreamRootPath);
            }
            mCacheFilePath = Path.Combine(fileCycleStreamRootPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(mCacheFilePath))
            {
                Directory.CreateDirectory(mCacheFilePath);
            }
        }
    }

    /// <summary>
    /// 文件缓存池中最小的文件缓存块，文件缓存块初始化的时候需要给予文件块大小
    /// </summary>
    public class FileCycleUnit : IDisposable
    {
        string mFilePath;
        int mCapacity;
        FileStream mStream;
        object mReadWriteLock = new object();
        int mReadPos;
        int mWritePos;
        int mCanWriteLength;
        int mCanReadLength;
        AveLogger mLogger = AveLogger.GetInstance(typeof(FileCycleUnit));
        /// <summary>
        /// 外围从当前文件可以读取的数据长度
        /// </summary>
        public int CanWriteLength
        {
            get
            {
                lock (mReadWriteLock)
                {
                    if (mReadPos < mWritePos)//不存在cycle的情况，读入数据写入数据在一次cycle中
                    {
                        return mCapacity - mWritePos;
                    }
                    else if (mReadPos > mWritePos)//出现cycle，读出写入不在一次cycle
                    {
                        return mReadPos - mWritePos;
                    }
                    else
                    {
                        if (mCanWriteLength > 0)//节点在一起根据实际情况判断可读取数据
                        {
                            return mCapacity - mWritePos;
                        }
                        return 0;
                    }
                }
            }
        }

        public int CanReadLength
        {
            get
            {
                lock (mReadWriteLock)
                {
                    if (mReadPos < mWritePos)
                    {
                        return mWritePos - mReadPos;
                    }
                    else if (mReadPos < mWritePos)
                    {
                        return mCapacity - mReadPos;
                    }
                    else
                    {
                        if (mCanReadLength > 0)
                        {
                            return mCapacity - mReadPos;
                        }
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// 判断当前文件在本次循环写入范围内是否写完，外围通过这个状态位决定是否取下一个cache文件进行写入数据
        /// </summary>
        
        public bool WriteFinishInThisCycle
        {
            get { lock (mReadWriteLock) { return mWritePos == 0 && mCanReadLength != 0; } }
        }

        /// <summary>
        /// 判断当前文件cycle范围内是否读完，外围通过这个状态位决定是否取下一个cache文件读取数据
        /// </summary>
        public bool ReadFinishInThisCycle
        {
            get { lock (mReadWriteLock) { return mReadPos == 0 && mCanWriteLength != 0; } }
        }

        public int WritePos
        {
            get { lock (mReadWriteLock) { return mWritePos; } }
        }

        public FileCycleUnit(string filePath, int capacity)
        {
            this.mFilePath = filePath;
            this.mCapacity = capacity;
            this.mCanWriteLength = capacity;
            mStream = new FileStream(mFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public void WriteByte(byte[] buffer, int offset, int length)
        {
            lock (mReadWriteLock)
            {
                mStream.Position = mWritePos;
                mStream.Write(buffer, offset, length);
                mWritePos = (mWritePos + length) % mCapacity;//底层只负责移动光标位置，数据控制需要上层控制，如果上层在底层文件没有读取完毕就开始写入出现问题由上层负责
                mCanWriteLength -= length;
                mCanReadLength += length;
            }
        }

        public void ReadByte(byte[] buffer, int offset, int length)
        {
            lock (mReadWriteLock)
            {
                mStream.Position = mReadPos;
                mStream.Read(buffer, offset, length);
                mReadPos = (mReadPos + length) % mCapacity;
                mCanWriteLength += length;
                mCanReadLength -= length;
            }
        }

        /// <summary>
        /// 为cache send数据使用，首先这个文件不存在循环使用的情况，读取不需要移动读指针位置，与正常使用不冲突
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="streamStartOffset"></param>
        /// <param name="length"></param>
        public void ReadByteOneCycle(byte[] buffer, int offset, int streamStartOffset, int length)
        {
            lock (mReadWriteLock)
            {
                mStream.Position = streamStartOffset;
                mStream.Read(buffer, offset, length);
            }
        }

        public void Dispose()
        {
            try
            {
                mStream.Close();
                mStream.Dispose();
                mStream = null;
                if (File.Exists(mFilePath))
                {
                    File.Delete(mFilePath);
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Dispose file Unit:{0} failed, exception:{1}", mFilePath, e.ToString());
            }
        }
    }
}
