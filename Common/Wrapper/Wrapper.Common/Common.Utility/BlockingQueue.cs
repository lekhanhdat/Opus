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



namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Runtime.CompilerServices;

    public class BlockingQueue<T>: IDisposable
    {
        private readonly Queue<T> mQueue;
        private readonly int mMaxCount;
        private Semaphore mEmptyQueueSemaphore;
        private Semaphore mFullQueueSemaphore;
        private ManualResetEvent mResetEvent;
        private const int mWaitTimeout = int.MaxValue;   //large file sometimes will cause time out exceptions, make time out limit larger
        private bool mIsDisposed = false;

        public BlockingQueue(int maxCount)
        {
            mQueue = new Queue<T>();
            mMaxCount = maxCount;
            mFullQueueSemaphore = new Semaphore(0, maxCount);
            mEmptyQueueSemaphore = new Semaphore(maxCount, maxCount);
            mResetEvent = new ManualResetEvent(false);
        }

        public void Reset()
        {
            if(mIsDisposed)
            {
                mFullQueueSemaphore = new Semaphore(0, mMaxCount);
                mEmptyQueueSemaphore = new Semaphore(mMaxCount, mMaxCount);
                mResetEvent = new ManualResetEvent(false);
            }
            else
            {
                mFullQueueSemaphore.Close();
                mFullQueueSemaphore = new Semaphore(0, mMaxCount);
                mEmptyQueueSemaphore.Close();
                mEmptyQueueSemaphore = new Semaphore(mMaxCount, mMaxCount);
                mResetEvent.Reset();
            }
        }

        public T Dequeue()
        {
            int signal = WaitHandle.WaitAny(new WaitHandle[] { mFullQueueSemaphore, mResetEvent }, mWaitTimeout);
            if (signal == 0)
            {
                lock (mQueue)
                {
                    T queueMember = mQueue.Dequeue();
                    mEmptyQueueSemaphore.Release();
                    return queueMember;
                }
            }
            else if (signal == 1)     //reset event
            {
                this.Clear();   //release all cached resources
                //mFullQueueSemaphore.Close();
                //mEmptyQueueSemaphore.Close();
                //mResetEvent.Close();
                Reset();
                return default(T);  //default T is null
            }
            else if(signal == WaitHandle.WaitTimeout)
            {
                throw new TimeoutException("time out when getting queue member from queue.");
            }
            throw new Exception("Unexcepted signal caught when getting member from queue.");    //this line should never be rearched
        }

        public void Enqueue(T queueMember)
        {
            if (mEmptyQueueSemaphore.WaitOne(mWaitTimeout))
            {
                lock (mQueue)
                {
                    if (Interlocked.Equals(mQueue.Count, mMaxCount))
                    {
                        throw new Exception(string.Format("exceed queue threshold, maximum count: {0}, current count: {1}", mMaxCount, mQueue.Count));
                    }
                    mQueue.Enqueue(queueMember);
                    mFullQueueSemaphore.Release();
                }
            }
            else
            {
                this.Clear();   //if time out, clear wasted cache
                throw new TimeoutException("time out when putting queue member into queue.");
            }
        }

        public int Count
        {
            get
            {
                lock (mQueue)
                {
                    return mQueue.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (mQueue)
                {
                    return (mQueue.Count == 0);
                }
            }
        }

        public bool IsClosed => mIsDisposed;

        public bool Clear()
        {
            bool hasException = false;
            lock (mQueue)
            {
                foreach (T queueMember in mQueue)
                {
                    try
                    {
                        IDisposable disposableObj = queueMember as IDisposable;
                        if (disposableObj != null)
                        {
                            disposableObj.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        string errMsg = e.Message;
                        hasException = true;
                    }
                }
                mQueue.Clear();
            }
            return hasException;
        }

        public void Close()
        {
            mResetEvent.Set();

            if (mEmptyQueueSemaphore != null)
            {
                mEmptyQueueSemaphore.Dispose();
            }
            if (mFullQueueSemaphore != null)
            {
                mFullQueueSemaphore.Dispose();
            }
            if (mResetEvent != null)
            {
                mResetEvent.Dispose();
            }
            mIsDisposed = true;
        }

        public void Dispose()
        {
            if(mEmptyQueueSemaphore != null)
            {
                mEmptyQueueSemaphore.Dispose();
            }
            if(mFullQueueSemaphore != null)
            {
                mFullQueueSemaphore.Dispose();
            }
            if(mResetEvent != null)
            {
                mResetEvent.Dispose();
            }
            mIsDisposed = true;
        }
    }
}
