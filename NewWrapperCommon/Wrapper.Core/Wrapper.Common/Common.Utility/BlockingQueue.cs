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


namespace AvePoint.Wrapper.Common.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Runtime.CompilerServices;

    public class BlockingQueue<T>:IDisposable where T : class
    {
        private readonly Queue<T> mQueue;
        private readonly int mMaxCount;
        private readonly Semaphore mEmptyQueueSemaphore;
        private readonly Semaphore mFullQueueSemaphore;
        private readonly ManualResetEvent mResetEvent;
        private const int mWaitTimeout = 60 * 60 * 1000;

        public BlockingQueue(int maxCount)
        {
            mQueue = new Queue<T>();
            mMaxCount = maxCount;
            mFullQueueSemaphore = new Semaphore(0, maxCount);
            mEmptyQueueSemaphore = new Semaphore(maxCount, maxCount);
            mResetEvent = new ManualResetEvent(false);
        }

        public T Dequeue()
        {
            if (WaitHandle.WaitAny(new WaitHandle[] { mFullQueueSemaphore, mResetEvent }, mWaitTimeout) != WaitHandle.WaitTimeout)
            {
                lock (mQueue)
                {
                    if (mQueue.Count == 0)
                    {
                        mFullQueueSemaphore.Close();
						mEmptyQueueSemaphore.Close();
						mResetEvent.Close();
                        return null;
                    }
                    else
                    {
                        T queueMember = mQueue.Dequeue();
                        mEmptyQueueSemaphore.Release();
                        return queueMember;
                    }
                }
            }
            else
            {
                throw new TimeoutException("Time out when getting queue member from queue.");
            }
        }

        public void Enqueue(T queueMember)
        {
            if (mEmptyQueueSemaphore.WaitOne(mWaitTimeout))
            {
                lock (mQueue)
                {
                    if (Interlocked.Equals(mQueue.Count, mMaxCount))
                    {
                        throw new Exception(string.Format("Exceed queue threshold, maximum count: {0}, current count: {1}", mMaxCount, mQueue.Count));
                    }
                    mQueue.Enqueue(queueMember);
                    mFullQueueSemaphore.Release();
                }
            }
            else
            {
                throw new TimeoutException("Time out when putting queue member into queue.");
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

        public void Close()
        {
            mResetEvent.Set();
        }

        public void Dispose()
        {
            
        }
    }
}
