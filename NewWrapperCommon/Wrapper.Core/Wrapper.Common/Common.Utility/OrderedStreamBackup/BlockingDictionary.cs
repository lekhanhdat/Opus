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

    public class BlockingDictionary<T> : IDisposable where T : class
    {
        private readonly Dictionary<int, T> mOrderDic;
        private readonly int mMaxCount;
        private readonly Semaphore mEmptyQueueSemaphore;
        private const int mWaitTimeout = 60 * 60 * 1000;
        private AutoResetEvent mGetStreamEvent = new AutoResetEvent(false);
        private int mCurrentItem = -1;

        public BlockingDictionary(int maxCount)
        {
            mOrderDic = new Dictionary<int, T>();
            mMaxCount = maxCount;
            mEmptyQueueSemaphore = new Semaphore(maxCount, maxCount);
        }

        public T this[int order]
        {
            get
            {
                //don't put all winthin lock, to avoid dead lock.
                var flag = false;
                lock (mOrderDic)
                {
                    mCurrentItem = order;
                    if (!mOrderDic.ContainsKey(order))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    mGetStreamEvent.WaitOne();
                }
                lock (mOrderDic)
                {
                    T queueMember = mOrderDic[order];
                    return queueMember;
                }
            }
        }

        public bool Remove(int order)
        {
            lock (mOrderDic)
            {
                bool removeSuccess = mOrderDic.Remove(order);
                if (removeSuccess)
                {
                    mEmptyQueueSemaphore.Release();
                }
                return removeSuccess;
            }
        }

        public void Add(int order, T queueMember)
        {
            if (mEmptyQueueSemaphore.WaitOne(mWaitTimeout))
            {
                lock (mOrderDic)
                {
                    if (Interlocked.Equals(mOrderDic.Count, mMaxCount))
                    {
                        throw new Exception(string.Format("Exceed queue threshold, maximum count: {0}, current count: {1}", mMaxCount, mOrderDic.Count));
                    }
                    mOrderDic.Add(order, queueMember);
                    if (order == mCurrentItem)
                    {
                        mGetStreamEvent.Set();
                    }
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
                lock (mOrderDic)
                {
                    return mOrderDic.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (mOrderDic)
                {
                    return (mOrderDic.Count == 0);
                }
            }
        }

        public void Close()
        {

        }

        public void Dispose()
        {

        }
    }
}
