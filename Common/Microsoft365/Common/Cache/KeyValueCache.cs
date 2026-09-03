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
using Microsoft365.Common.Logger;

namespace Microsoft365.Common.Cache
{
    public sealed class KeyValueCache<TKey, TValue> : IKeyValueCache<TKey, TValue>
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(KeyValueCache<TKey, TValue>));
        private Dictionary<TKey, KeyVauleCacheEntry<TValue>> caches = new Dictionary<TKey, KeyVauleCacheEntry<TValue>>();
        private const int DefaultCapacity = 1000;
        private const int DefaultKeyExpiredTimeSecondsValue = 60 * 60;
        private const int DefaultKeyExpiredEdgeSecondsValue = 5 * 60;
        public TimeSpan KeyExpiredEdge { get; set; }
        public TimeSpan DefaultKeyExpiredTime { get; set; }
        public int Capacity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="keyExpiredEdgeSeconds"></param>
        /// <param name="defaultKeyExpiredTimeMinutes"></param>
        public KeyValueCache(int? capacity = DefaultCapacity, int? keyExpiredEdgeSeconds = DefaultKeyExpiredEdgeSecondsValue, int? defaultKeyExpiredTimeSeconds = DefaultKeyExpiredTimeSecondsValue)
        {
            Capacity = capacity?? DefaultCapacity;
            KeyExpiredEdge = GetTimeSpanFromSeconds(keyExpiredEdgeSeconds, DefaultKeyExpiredEdgeSecondsValue);
            DefaultKeyExpiredTime = GetTimeSpanFromSeconds(defaultKeyExpiredTimeSeconds, DefaultKeyExpiredTimeSecondsValue);
        }

        private TimeSpan GetTimeSpanFromSeconds(int? value, int defaultValue)
        {
            return value.HasValue&&value.Value >= 0 ? TimeSpan.FromSeconds(value.Value) : TimeSpan.FromSeconds(defaultValue);
        }

        public TValue Get(TKey key)
        {
            lock (caches)
            {
                if (caches.TryGetValue(key, out KeyVauleCacheEntry<TValue> entry) && entry != null && entry.IsValid(KeyExpiredEdge))
                {
                    return entry.Value;
                }
            }
            return default;
        }

        [Obsolete("All should use AccessTokenCacheEntry in the future")]
        public void AddOrUpdate(TKey key, TValue value, DateTimeOffset expiresOn)
        {
            AddOrUpdate(key, new KeyVauleCacheEntry<TValue>(value, expiresOn));
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            AddOrUpdate(key, new KeyVauleCacheEntry<TValue>(value, DateTimeOffset.UtcNow.Add(DefaultKeyExpiredTime)));
        }

        private void AddOrUpdate(TKey key, KeyVauleCacheEntry<TValue> entry)
        {
            lock (caches)
            {
                if (caches.ContainsKey(key))
                {
                    caches[key] = entry;
                }
                else
                {
                    var capacity = Capacity;

                    if (caches.Count > capacity)
                    {
                        var items = caches.OrderBy(k => k.Value.ExpiresOn).Take(caches.Count - capacity);
                        foreach (var item in items)
                        {
                            logger.Info("Clean the cache:{0} with expire:{1}", item.Key, item.Value.ExpiresOn);
                            caches.Remove(item.Key);
                        }
                    }

                    caches[key] = entry;
                }
            }
        }

        public void Clear()
        {
            lock (caches)
            {
                caches.Clear();
            }
        }
    }
}