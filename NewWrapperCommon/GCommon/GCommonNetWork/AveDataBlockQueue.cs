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
using System.Reflection;
using System.Text;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.GCommon.Network;

namespace AvePoint.GCommon.Network
{
    /// <summary>
    /// 当DataBlockQueue被实例化后，队列中将保存指定数量的Block，不会再创建Block，这样做是为了控制内存的增长。
    /// 需要注意：将DataBlock取出后，需要手动将Block释放回队列中。
    /// </summary>
    public class AveDataBlockQueue : IDisposable
    {
        private List<AveDataBlock> blockQueue;
        private readonly int maxCount;

        private Semaphore workingBlockSemaphore;
        private int workingCount;
        private Semaphore freeBlockSemaphore;
        private int freeCount;
        private ManualResetEvent exceptionEvent = new ManualResetEvent(false);
        private string exceptionMessage = string.Empty;

        private AveDataBlockType lastInputWorkingBlockType = AveDataBlockType.UNKNOW_TYPE;
        private AveDataBlockType lastOutputWorkingBlockType = AveDataBlockType.UNKNOW_TYPE;

        private int timeOut = 1000 * 60 * 60 * 24;
        private string name = string.Empty;

        public int TimeOut { set { timeOut = value; } }
        public string Name { set { name = value; } }

        /// <summary>
        /// 初始化DataBlock队列，默认队列大小为100
        /// </summary>
        /// <param name="initSize"></param>
        /// <param name="needInit"></param>
        public AveDataBlockQueue(int initSize = 100)
        {
            maxCount = initSize > 0 ? initSize : 100;
            this.blockQueue = new List<AveDataBlock>(maxCount);
            for (int i = 0; i < maxCount; i++)
            {
                blockQueue.Add(new AveDataBlock());
            }
            workingBlockSemaphore = new Semaphore(0, maxCount);
            workingCount = 0;
            freeBlockSemaphore = new Semaphore(maxCount, maxCount);
            freeCount = maxCount;
        }

        public AveDataBlockQueue(int initSize, int blockSize = AveDataBlock.DATA_BLOCK_SIZE)
        {
            maxCount = initSize > 0 ? initSize : 100;
            blockSize = blockSize > AveDataBlock.DATA_BLOCK_HEADER_LEN ? blockSize : AveDataBlock.DATA_BLOCK_SIZE;
            this.blockQueue = new List<AveDataBlock>(maxCount);
            for (int i = 0; i < maxCount; i++)
            {
                blockQueue.Add(new AveDataBlock(blockSize));
            }
            workingBlockSemaphore = new Semaphore(0, maxCount);
            workingCount = 0;
            freeBlockSemaphore = new Semaphore(maxCount, maxCount);
            freeCount = maxCount;
        }

        /// <summary>
        /// 将填充有数据的DataBlock放回到队列尾
        /// </summary>
        /// <param name="block"></param>
        public void PutWorkingBlock(AveDataBlock block)
        {
            if (lastInputWorkingBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE && block.Type != AveDataBlockType.ALIVE_TYPE)
            {
                throw new BlockQueueSyncException("Can't put data block after closed.");
            }
            lock (blockQueue)
            {
                blockQueue.Add(block);
                workingCount++;
            }
            workingBlockSemaphore.Release();
            lastInputWorkingBlockType = block.Type;
        }

        /// <summary>
        /// 从队列中取回第一个填充有数据的DataBlock。
        /// 注意：这个方法会减少队列中可用的Block，需要调用PutFreeBlock方法，将使用后的block释放回队列中
        /// </summary>
        /// <returns></returns>
        public AveDataBlock TakeWorkingBlock()
        {
            if (lastOutputWorkingBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                throw new BlockQueueSyncException("Can't take data block after closed.");
            }
            while (true)
            {
                int waitResult = WaitHandle.WaitAny(new WaitHandle[] { exceptionEvent, workingBlockSemaphore }, timeOut);
                if (waitResult == 0) throw new BlockQueueSyncException(exceptionMessage);
                if (waitResult == 1) break;
                if (waitResult == WaitHandle.WaitTimeout) SetException(string.Format("{0} take working block timeout: {1}", name, timeOut));
            }

            AveDataBlock block;
            lock (blockQueue)
            {
                block = blockQueue[freeCount];
                blockQueue.RemoveAt(freeCount);
                workingCount--;
            }
            lastOutputWorkingBlockType = block.Type;
            return block;
        }

        /// <summary>
        /// 将DataBlock放回到队列头，并增加一个FreeCount
        /// </summary>
        /// <param name="freeBlock"></param>
        public void PutFreeBlock(AveDataBlock freeBlock)
        {
            lock (blockQueue)
            {
                blockQueue.Insert(0, freeBlock);
                freeCount++;
            }
            freeBlockSemaphore.Release();
        }

        /// <summary>
        /// 从队列头取出一个空DataBlock
        /// </summary>
        /// <returns></returns>
        public AveDataBlock TakeFreeBlock()
        {
            while (true)
            {
                int waitResult = WaitHandle.WaitAny(new WaitHandle[] { exceptionEvent, freeBlockSemaphore }, timeOut);
                if (waitResult == 0) throw new BlockQueueSyncException(exceptionMessage);
                if (waitResult == 1) break;
                if (waitResult == WaitHandle.WaitTimeout) SetException(string.Format("{0} take free block timeout: {1}", name, timeOut));
            }

            AveDataBlock block;
            lock (blockQueue)
            {
                block = blockQueue[0];
                blockQueue.RemoveAt(0);
                freeCount--;
            }
            return block;
        }

        /// <summary>
        /// 通知所有等待Queue的线程出现异常
        /// </summary>
        /// <param name="error"></param>
        public void SetException(string error)
        {
            exceptionMessage = error;
            exceptionEvent.Set();
        }

        public void CheckException()
        {
            if (!String.IsNullOrEmpty(this.exceptionMessage))
            {
                throw new CommonNetworkException(this.exceptionMessage);
            }
        }

        public void Dispose()
        {
            if (workingBlockSemaphore != null)
            {
                workingBlockSemaphore.Close();
                workingBlockSemaphore = null;
            }
            if (freeBlockSemaphore != null)
            {
                freeBlockSemaphore.Close();
                freeBlockSemaphore = null;
            }
            if (exceptionEvent != null)
            {
                exceptionEvent.Close();
                exceptionEvent = null;
            }
        }
    }

    [Serializable]
    public class BlockQueueSyncException : CommonNetworkException
    {
        public BlockQueueSyncException() { }
        public BlockQueueSyncException(string message) : base(message) { }
        public BlockQueueSyncException(string message, Exception inner) : base(message, inner) { }
        protected BlockQueueSyncException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ClosedWithErrorException : CommonNetworkException
    {
        public ClosedWithErrorException() { }
        public ClosedWithErrorException(string message) : base(message) { }
        public ClosedWithErrorException(string message, Exception inner) : base(message, inner) { }
        protected ClosedWithErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    //public class SyncEvent : IDisposable
    //{
    //    private ManualResetEvent mNormalEvent;
    //    private ManualResetEvent mExceptionEvent;
    //    private ManualResetEvent[] mEvents;
    //    private string mMsg;

    //    public SyncEvent()
    //    {
    //        mMsg = "Exception event set";
    //        mNormalEvent = new ManualResetEvent(false);
    //        mExceptionEvent = new ManualResetEvent(false);
    //        mEvents = new ManualResetEvent[] { mExceptionEvent, mNormalEvent };
    //    }

    //    public bool Wait(object o)
    //    {
    //        Monitor.Exit(o);
    //        int res = WaitHandle.WaitAny(mEvents);
    //        Monitor.Enter(o);
    //        mNormalEvent.Reset();
    //        if (res == 1)
    //            return true;
    //        else if (res == 0)
    //            throw new Exception(mMsg);
    //        return false;
    //    }

    //    public void Set()
    //    {
    //        mNormalEvent.Set();
    //    }

    //    public void SetException(string msg)
    //    {
    //        mMsg = msg;
    //        mExceptionEvent.Set();
    //    }

    //    public void Dispose()
    //    {
    //        if (mEvents != null)
    //        {
    //            mNormalEvent.Close();
    //            mNormalEvent = null;
    //            mExceptionEvent.Close();
    //            mNormalEvent = null;
    //            mEvents = null;
    //        }
    //    }
    //}
}