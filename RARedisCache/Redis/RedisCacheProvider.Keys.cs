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
using Microsoft.Extensions.Logging;
using AvePoint.RA.RedisCache.Configurations;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AvePoint.RA.RedisCache
{
    public partial class RedisCacheProvider : IRedisCacheProvider
    {
       
        public bool KeyDel(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyDelete(cacheKey);
            return flag;
        }

        public async Task<bool> KeyDelAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyDeleteAsync(cacheKey);
            return flag;
        }

        public async Task<long> KeyDelAsync(string[] cacheKey)
        {
            List<RedisKey> keys = new List<RedisKey>(cacheKey.Length);
            foreach(var key in cacheKey)
            {
                keys.Add(key);
            }

            var flag = await _cache.KeyDeleteAsync(keys.ToArray());
            return flag;
        }

        public bool KeyExpire(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyExpire(cacheKey, TimeSpan.FromSeconds(second));
            return flag;
        }
        public bool KeyExpire(string cacheKey, TimeSpan time)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyExpire(cacheKey, time);
            return flag;
        }

        public async Task<bool> KeyExpireAsync(string cacheKey, TimeSpan time)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyExpireAsync(cacheKey, time);
            return flag;
        }

        public async Task<bool> KeyExpireAsync(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(second));
            return flag;
        }

        public bool KeyExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyExists(cacheKey);
            return flag;
        }

        public async Task<bool> KeyExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyExistsAsync(cacheKey);
            return flag;
        }
        public TimeSpan? KeyTimeToLive(string key)
        {
            return _cache.KeyTimeToLive(key);
        }
    }
}
