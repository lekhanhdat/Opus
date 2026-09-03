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
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using LOGRESOURCEARCHIVEINTER = Merged18NResources.Archive.ArchiveForInternationalization;

namespace RAArchiverCommon
{
    /// <summary>
    /// Producer-Consumer container. for synchronic access.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PCContainer<T> : IDisposable where T : class
    {
        private readonly int mMaxCount = 30;
        private LinkedList<T> mLists;

        private readonly object mLocker = new object();
        private bool mProducerEnd = false;
        private bool mConsumerEnd = false;

        public PCContainer()
        {
            mLists = new LinkedList<T>();
        }

        public PCContainer(int maxCount)
            : this()
        {
            mMaxCount = maxCount;
        }

        public void Produce(T obj)
        {
            lock (mLocker)
            {
                do
                {
                    if (mConsumerEnd)
                    {
                        return;
                    }
                    if (mProducerEnd)
                    {
                        throw new ObjectDisposedException(LOGRESOURCE.StorageOptimization13_SOARCOMPCContainerProduceObjectDisposedException);
                    }
                    if (mLists.Count == mMaxCount)
                    {
                        Monitor.Wait(mLocker);
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                if (mLists.Count > mMaxCount)
                {
                    throw new SynchronizationLockException(LOGRESOURCE.StorageOptimization13_SOARCOMPCContainerProduceSynchronizationLockException);
                }

                mLists.AddLast(obj);
                Monitor.Pulse(mLocker);
            }
        }

        public T Consume()
        {
            lock (mLocker)
            {
                do
                {
                    if (mConsumerEnd)
                    {
                        throw new ObjectDisposedException(LOGRESOURCE.StorageOptimization13_SOARCOMPCContainerConsumeObjectDisposedException);
                    }
                    if (0 == mLists.Count)
                    {
                        if (mProducerEnd)
                        {
                            return null;
                        }
                        Monitor.Wait(mLocker);
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                T tmp = mLists.First.Value;
                mLists.RemoveFirst();
                Monitor.Pulse(mLocker);
                return tmp;
            }
        }

        public int Count
        {
            get
            {
                lock (mLocker)
                {
                    return mLists.Count;
                }
            }
        }
        /// <summary>
        /// this method should be invoked by producer.
        /// </summary>
        public void EndProduce()
        {
            lock (mLocker)
            {
                mProducerEnd = true;
                Monitor.PulseAll(mLocker);
            }
        }
        public void StartProduce()
        {
            lock (mLocker)
            {
                mProducerEnd = false;
            }
        }
        /// <summary>
        /// this method should be invoked by consumer
        /// </summary>
        public void EndConsume()
        {
            lock (mLocker)
            {
                mConsumerEnd = true;

                Monitor.PulseAll(mLocker);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

            EndConsume();
            EndProduce();
            if (typeof(T).GetInterface("IDisposable") != null)
            {
                foreach (IDisposable tmp in mLists)
                {
                    tmp.Dispose();
                }
            }

        }

    }
}
