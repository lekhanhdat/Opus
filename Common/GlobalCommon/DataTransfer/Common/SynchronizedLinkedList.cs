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



using System.Collections.Generic;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Common
{
    internal class SynchronizedLinkedList<T>
    {
        LinkedList<T> mObjectList = new LinkedList<T>();
        int mCapability = 0;

        public int Count { get { return mObjectList.Count; } }

        public SynchronizedLinkedList()
        {
            this.mCapability = int.MaxValue;
        }

        public SynchronizedLinkedList(int capability)
        {
            this.mCapability = capability;
        }

        public void AddLast(T obj)
        {
            lock (mObjectList)
            {
                while (mObjectList.Count > mCapability)
                {
                    Monitor.Wait(mObjectList);
                }
                mObjectList.AddLast(obj);
                Monitor.Pulse(mObjectList);
            }
        }

        public bool TryAddLast(T obj)
        {
            lock (mObjectList)
            {
                if (mObjectList.Count > mCapability)
                {
                    return false;
                }
                mObjectList.AddLast(obj);
                Monitor.Pulse(mObjectList);
            }
            return true;
        }

        public T GetFirst()
        {
            T obj;
            lock (mObjectList)
            {
                while (mObjectList.Count == 0)
                {
                    Monitor.Wait(mObjectList);
                }
                obj = mObjectList.First.Value;
                mObjectList.RemoveFirst();
                Monitor.Pulse(mObjectList);
            }
            return obj;
        }

        public bool TryGetFirst(out T value)
        {
            value = default(T);
            lock (mObjectList)
            {
                if (mObjectList.Count == 0)
                {
                    return false;
                }
                else
                {
                    value = mObjectList.First.Value;
                    mObjectList.RemoveFirst();
                    Monitor.Pulse(mObjectList);
                }
            }
            return true;
        }
    }
}
