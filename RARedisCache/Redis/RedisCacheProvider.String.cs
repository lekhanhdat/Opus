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

        public bool StringSet(string cacheKey, string cacheValue, System.TimeSpan? expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            bool flag = _cache.StringSet(cacheKey, cacheValue, expiration, false);
            return flag;
        }

        public async Task<bool> StringSetAsync(string cacheKey, string cacheValue, System.TimeSpan? expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            bool flag = await _cache.StringSetAsync(cacheKey, cacheValue, expiration, false);
            return flag;
        }

        public string StringGet(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var val = _cache.StringGet(cacheKey);
            return val;
        }

        public async Task<string> StringGetAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var val = await _cache.StringGetAsync(cacheKey);
            return val;
        }

    }
}
