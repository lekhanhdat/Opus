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
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Utils
{
    internal class LocalCache
    {
        private readonly MemoryCache cache;

        private readonly TimeSpan defaultExpTime;

        public LocalCache()
            : this(TimeSpan.FromDays(36500.0))
        {
        }

        public LocalCache(TimeSpan defaultExpTime)
        {
            cache = new MemoryCache(new MemoryCacheOptions());
            this.defaultExpTime = defaultExpTime;
        }

        public T Get<T>(string key)
        {
            return cache.Get<T>(key);
        }

        public T Get<T>(string key, Func<T> valueFactory)
        {
            return Get(key, valueFactory, defaultExpTime);
        }

        public T Get<T>(string key, Func<T> valueFactory, TimeSpan expTime)
        {
            return cache.GetOrCreate(key, delegate (ICacheEntry entry)
            {
                T result = valueFactory();
                entry.AbsoluteExpirationRelativeToNow = expTime;
                return result;
            });
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> valueFactory)
        {
            return await GetAsync(key, valueFactory, defaultExpTime);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expTime)
        {
            return await cache.GetOrCreateAsync(key, async delegate (ICacheEntry entry)
            {
                T result = await valueFactory();
                entry.AbsoluteExpirationRelativeToNow = expTime;
                return result;
            });
        }

        public void Set(string key, object value)
        {
            Set(key, value, defaultExpTime);
        }

        public void Set(string key, object value, TimeSpan expTime)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expTime
            };
            cache.Set(key, value, options);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void Dispose()
        {
            cache.Dispose();
        }
    }
}
