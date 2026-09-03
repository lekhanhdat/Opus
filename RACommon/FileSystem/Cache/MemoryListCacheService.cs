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

using AvePoint.RA.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AvePoint.RA.Common.Cache
{
    public class MemoryStackCacheService<T> : ICacheService<T>
    {
        private readonly object _locker = new object();
        private Stack<T> _items = new Stack<T>();
        private int MaxCount = -1;

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _items.Count;
                }
            }
        }

        public void Add(T item)
        {
            CodeContract.NullThrowing(item, "item");
            while (MaxCount > 0 && _items.Count > MaxCount)
            {
                Thread.Sleep(1000);
            }
            lock (_locker)
            {
                _items.Push(item);
            }
        }

        public void AddBatch(IEnumerable<T> items)
        {
            CodeContract.NullThrowing(items, "items");
            while (MaxCount > 0 && _items.Count > MaxCount)
            {
                Thread.Sleep(1000);
            }
            lock (_locker)
            {
                foreach (T item in items)
                {
                    _items.Push(item);
                }
            }
        }
        /// <summary>
        /// if max=-1/0  means no throttling.
        /// </summary>
        /// <param name="max"></param>
        public void SetThrottling(int max)
        {
            MaxCount = max;
        }
        public IEnumerable<T> Take(int count = 1)
        {
            CodeContract.Require(count > 0, "count >0");

            lock (_locker)
            {
                if (count >= _items.Count)
                {
                    List<T> temp = _items.ToList();
                    _items.Clear();
                    return temp;
                }
                else
                {
                    List<T> temp = new List<T>();
                    for (int i = 0; i < count; i++)
                    {
                        T item = _items.Pop();
                        temp.Add(item);
                    }
                    return temp;
                }
            }

        }

        public T Take()
        {
            T item;
            lock (_locker)
            {
                item = _items.Count > 0 ? _items.Pop() : default(T);
            }
            return item;
        }

        public IEnumerable<T> TakeAll()
        {
            IEnumerable<T> temp;
            lock (_locker)
            {
                temp = _items.ToList();
                _items.Clear();
            }
            return temp;
        }
    }
}
