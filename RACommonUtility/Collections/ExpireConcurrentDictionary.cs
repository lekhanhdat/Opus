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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Collections
{
    public class ExpireConcurrentDictionary<K, V> : IDisposable
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(ExpireConcurrentDictionary<K, V>));

        private static readonly TimeSpan DefaultExpiredTime = TimeSpan.FromSeconds(30);

        private readonly TimeSpan ExpiredTime;

        private readonly Timer timer;

        private readonly ConcurrentDictionary<long, List<K>> ExpireKeys = new ConcurrentDictionary<long, List<K>>();

        private readonly ConcurrentDictionary<K, V> Cache = new ConcurrentDictionary<K, V>();

        public ExpireConcurrentDictionary() : this(DefaultExpiredTime) { }

        public ExpireConcurrentDictionary(TimeSpan expiredTime)
        {
            CodeContract.NullThrowing(expiredTime, nameof(expiredTime));
            ExpiredTime = expiredTime;
            timer = new Timer(ListenExpiredKeys, null, TimeSpan.FromSeconds(0), ExpiredTime);
        }

        public bool TryGet(K key, out V value)
        {
            return Cache.TryGetValue(key, out value);
        }

        public bool TryAdd(K key, V value)
        {
            var expiredTicks = DateTime.UtcNow.Add(ExpiredTime).Ticks;
            if(!ExpireKeys.TryGetValue(expiredTicks, out var expireKeys))
            {
                expireKeys = new List<K>();
                if(!ExpireKeys.TryAdd(expiredTicks, expireKeys))
                {
                    Logger.Warn($"The add expired ticks: [{expiredTicks}] to cache failed.");
                    return false;
                }
            }

            expireKeys.Add(key);

            if(!Cache.TryAdd(key, value))
            {
                Logger.Warn($"The add cache failed. Key: [{key}].");
                return false;
            }

            return true;
        }

        private void ListenExpiredKeys(object state)
        {
            var currentTicks = DateTime.UtcNow.Ticks;
            var expiredTicksList = ExpireKeys.Keys.Where(item => item <= currentTicks);
            foreach(var expiredTicks in expiredTicksList)
            {
                if(!ExpireKeys.TryGetValue(expiredTicks, out var expiredKeys))
                {
                    continue;
                }

                foreach(var expiredKey in expiredKeys)
                {
                    if(Cache.TryRemove(expiredKey, out var value))
                    {
                        Logger.Info($"Successful remove expired cache key: [{expiredKey}].");
                    }
                }

                if(ExpireKeys.TryRemove(expiredTicks, out var expiredTicksValue))
                {
                    Logger.Info($"Successful remove expired ticks: [{expiredTicks}].");
                }
            }
        }

        public void Dispose()
        {
            if(timer != null)
            {
                timer.Dispose();
            }
        }
    }
}
