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
using System.Threading;

namespace AutoInstallationCommon.ActiveDirectory
{
    public class MemeryCache
    {
        private const long AvailableRange = 5L * 60 * 1000 * 10000;
        private static readonly object mlock = new object();
        private static readonly Dictionary<string, MCacheItem> _cache = new Dictionary<string, MCacheItem>();
        private static readonly Dictionary<string, long> _record = new Dictionary<string, long>();
        public static bool CacheEnabled = true;

        public static T GetValue<T>(string key)
        {
            if (!CacheEnabled) return default(T);
            ClearExpiredItemWorker(null);
            var result = default(T);
            lock (mlock)
            {
                var lastUpdateTime = DateTime.UtcNow.Ticks;
                if (_cache.ContainsKey(key))
                {
                    _cache[key].LastUpdateTime = lastUpdateTime;
                    _record[key] = lastUpdateTime + AvailableRange;
                    result = (T) _cache[key].Value;
                }
                else
                {
                    result = default(T);
                }
            }

            return result;
        }

        public static T CreateItem<T>(string key, T value)
        {
            lock (mlock)
            {
                var lastUpdateTime = DateTime.UtcNow.Ticks;
                if (_cache.ContainsKey(key))
                {
                    _cache[key].Value = value;
                    _cache[key].LastUpdateTime = lastUpdateTime;
                    _record[key] = lastUpdateTime + AvailableRange;
                }
                else
                {
                    _cache.Add(key, new MCacheItem
                    {
                        LastUpdateTime = lastUpdateTime,
                        AvailableRange = AvailableRange,
                        Key = key,
                        Value = value
                    });

                    _record.Add(key, lastUpdateTime + AvailableRange);
                }
            }

            //Free lock and ask checker to change the items
            ClearExpiredItem();
            return value;
        }

        private static void ClearExpiredItem()
        {
            ThreadPool.QueueUserWorkItem(ClearExpiredItemWorker);
        }

        private static void ClearExpiredItemWorker(object state)
        {
            lock (mlock)
            {
                var nowTime = DateTime.UtcNow.Ticks;
                var removableKeys = new List<string>();
                foreach (var key in _record.Keys)
                    if (nowTime > _record[key])
                    {
                        _cache.Remove(key);
                        removableKeys.Add(key);
                    }

                foreach (var key in removableKeys) _record.Remove(key);
            }
        }
    }

    public class MCacheItem
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public long LastUpdateTime { get; set; }
        public long AvailableRange { get; set; }
    }
}