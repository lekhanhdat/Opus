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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class AccessTimeCache<TKey, TValue> : IKeyValueCache<TKey, TValue>
    {
        private class CounterItem<TKey1, TValue1>
        {
            private TValue1 val;
            private DateTime lastAccessTime;
            private DateTime lastWriteTime;

            public CounterItem(TKey1 key, TValue1 value)
            {
                Key = key;
                val = value;
                lastAccessTime = DateTime.UtcNow;
                lastWriteTime = DateTime.UtcNow;
            }

            public TKey1 Key { get; set; }

            public TValue1 Value
            {
                get { lastAccessTime = DateTime.UtcNow; return val; }
                set { lastWriteTime = DateTime.UtcNow; lastAccessTime = DateTime.UtcNow; val = value; }
            }

            public DateTime LastAccessTime { get { return lastAccessTime; } }

            public DateTime LastWriteTime { get { return lastWriteTime; } }

            public bool IsNotExpired(int lifeCycleSecondTime)
            {
                return lastWriteTime.AddSeconds(lifeCycleSecondTime) < DateTime.UtcNow;
            }
        }

        private int capacity;
        private readonly int lifeCycleSecondTime;

        private Dictionary<TKey, CounterItem<TKey, TValue>> cache = new Dictionary<TKey, CounterItem<TKey, TValue>>();

        public int Capacity
        {
            get { return capacity; }
            set { capacity = value; }
        }

        public AccessTimeCache(int capacity)
            : this(capacity, -1)
        {
        }

        public AccessTimeCache(int capacity, int lifeCycleSecondTime)
        {
            this.capacity = capacity;
            this.lifeCycleSecondTime = lifeCycleSecondTime;
        }

        public TValue Get(TKey key)
        {
            CounterItem<TKey, TValue> counterValue;

            lock (cache)
            {
                if (cache.TryGetValue(key, out counterValue) && counterValue.IsNotExpired(lifeCycleSecondTime))
                {
                    return counterValue.Value;
                }
            }

            return default(TValue);
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            CounterItem<TKey, TValue> counterValue;

            var requireUpdate = true;

            lock (cache)
            {
                if (!cache.TryGetValue(key, out counterValue))
                {
                    counterValue = new CounterItem<TKey, TValue>(key, value);
                    cache[key] = counterValue;
                    requireUpdate = false;

                    if (cache.Count > capacity)
                    {
                        var first = cache.Values.OrderBy(item => item.LastAccessTime).First();

                        cache.Remove(first.Key);
                    }
                }
            }

            if (requireUpdate)
            {
                counterValue.Value = value;
            }
        }

        public void Clear()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }
    }
}
