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
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RedisCache
{
    public interface IRedisCacheProvider
    {
        string RedisName { get; }

        #region Keys
        bool KeyDel(string cacheKey);
        bool KeyExists(string cacheKey);

        Task<bool> KeyDelAsync(string cacheKey);

        Task<long> KeyDelAsync(string[] cacheKey);
        bool KeyExpire(string cacheKey, int second);
        bool KeyExpire(string cacheKey, TimeSpan time);
        TimeSpan? KeyTimeToLive(string key);
        #endregion
        Task<bool> IsRedisAvailable();
        #region String
        bool StringSet(string cacheKey, string cacheValue, System.TimeSpan? expiration = null);
        Task<bool> StringSetAsync(string cacheKey, string cacheValue, System.TimeSpan? expiration = null);
        string StringGet(string cacheKey);
        Task<string> StringGetAsync(string cacheKey);
        #endregion

        #region HashSet
        bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        bool HExists(string cacheKey, string field);
        long HDel(string cacheKey, IList<string> fields = null);
        void HDelWithIgnoreCase(string cacheKey, IList<string> fields = null, bool ignoreCase = true);
        string HGet(string cacheKey, string field);
        Dictionary<string, T> HBatchGet<T>(string key, List<string> fieldKeys);
        Dictionary<string, string> HGetAll(string cacheKey);
        Dictionary<string, T> HGetAll<T>(string key);
        List<string> HKeys(string cacheKey);
        Dictionary<string, string> HMGet(string cacheKey, IList<string> fields);
        Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        Task<bool> HSetAsync(string cacheKey, string field, string cacheValue);
        bool HSet(string cacheKey, string field, string cacheValue);
        void HSet<T>(string key, Dictionary<string, T> fields, bool ignoreCase = true);
        Task<bool> HExistsAsync(string cacheKey, string field);
        Task<long> HDelAsync(string cacheKey, IList<string> fields = null);
        Task<string> HGetAsync(string cacheKey, string field);
        Task<Dictionary<string, string>> HGetAllAsync(string cacheKey);
        #endregion

        #region List
        IEnumerable<RedisValue> ListRange(string cacheKey);
        void ListRightPush(string cacheKey, RedisValue cacheValue);
        void ListRightPush(string cacheKey, IEnumerable<RedisValue> cacheValues);
        Task<bool> KeyExistsAsync(string cacheKey);
        Task<IEnumerable<RedisValue>> ListRangeAsync(string cacheKey);
        Task ListRightPushAsync(string cacheKey, RedisValue cacheValue);
        Task ListRightPushAsync(string cacheKey, IEnumerable<RedisValue> cacheValues);
        Task<bool> KeyExpireAsync(string cacheKey, int second);
        Task<bool> KeyExpireAsync(string cacheKey, TimeSpan time);
        #endregion
    }
}
