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
using System.Linq;
using System.Text;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.GCommon.Network;

namespace AvePoint.Wrapper.FileTransfer
{
    class AveBlockQueue : IAveSyncCallback
    {
        public const int DEFAULT_QUEUE_SIZE = 100;

        private List<AveDataBlock> mQueue;
        private const int WAIT_TIME = 5 * 60000;

        private int mReservedSize;
        private AveSyncEvent mSyncEvent;

        public AveBlockQueue(int reserved, int initSize)
        {
            if (initSize > 0)
            {
                mQueue = new List<AveDataBlock>(initSize);
            }
            else
            {
                mQueue = new List<AveDataBlock>();
            }
            mReservedSize = reserved;
        }

        public void Fail(AveSyncEvent syncEvent)
        {
            lock (mQueue)
            {
                mSyncEvent = syncEvent;
                Monitor.Pulse(mQueue);
            }
        }

        public void PutBlock(AveDataBlock block)
        {
            lock (mQueue)
            {
                mQueue.Add(block);
                Monitor.Pulse(mQueue);
            }
        }

        public AveDataBlock TakeBlock()
        {
            return TakeBlock(false);
        }

        public AveDataBlock TakeBlock(bool timeout)
        {
            lock (mQueue)
            {
                if (timeout)
                {
                    if (mQueue.Count <= mReservedSize)
                    {
                        Monitor.Wait(mQueue, WAIT_TIME);
                    }
                    if (mSyncEvent != null)
                    {
                        mSyncEvent.CheckIsRunning();
                    }
                    if (mQueue.Count == 0)
                    {
                        return null;
                    }
                }
                else
                {
                    if (mSyncEvent != null)
                    {
                        mSyncEvent.CheckIsRunning();
                    }
                    while (mQueue.Count <= mReservedSize)
                    {
                        Monitor.Wait(mQueue);
                        if (mSyncEvent != null)
                        {
                            mSyncEvent.CheckIsRunning();
                        }
                    }
                }
                AveDataBlock block = mQueue[0];
                mQueue.RemoveAt(0);
                return block;
            }
        }

        public int Count
        {
            get
            {
                // to make the queue thread safe
                lock (mQueue)
                {
                    return mQueue.Count;
                }
            }
        }

        public int ReservedSize
        {
            get { return mReservedSize; }
        }
    }
}
