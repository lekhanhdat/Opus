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
using Newtonsoft.Json;

namespace AvePoint.RA.RedisCache
{
    public partial class RedisCacheProvider : IRedisCacheProvider
    {

        public bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (expiration.HasValue)
            {
                var list = new List<HashEntry>();

                foreach (var item in vals)
                {
                    list.Add(new HashEntry(item.Key, item.Value));
                }

                _cache.HashSet(cacheKey, list.ToArray());

                var flag = _cache.KeyExpire(cacheKey, expiration);

                return flag;
            }
            else
            {
                var list = new List<HashEntry>();

                foreach (var item in vals)
                {
                    list.Add(new HashEntry(item.Key, item.Value));
                }

                _cache.HashSet(cacheKey, list.ToArray());

                return true;
            }
        }

        public bool HSet(string cacheKey, string field, string cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return _cache.HashSet(cacheKey, field, cacheValue);
        }

        public bool HExists(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return _cache.HashExists(cacheKey, field);
        }

        public long HDel(string cacheKey, IList<string> fields = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (fields != null && fields.Any())
            {
                return _cache.HashDelete(cacheKey, fields.Select(x => (RedisValue)x).ToArray());
            }
            else
            {
                var flag = _cache.KeyDelete(cacheKey);
                return flag ? 1 : 0;
            }
        }

        public string HGet(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var res = _cache.HashGet(cacheKey, field);
            return res;
        }

        public Dictionary<string, string> HGetAll(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dict = new Dictionary<string, string>();

            var vals = _cache.HashGetAll(cacheKey);

            foreach (var item in vals)
            {
                if (!dict.ContainsKey(item.Name)) dict.Add(item.Name, item.Value);
            }

            return dict;
        }
        public Dictionary<string, T> HBatchGet<T>(string key, List<string> fieldKeys)
        {
            RedisValue[] redisFieldKeys = ConvertStrListToRedisValues(fieldKeys);
            if (redisFieldKeys == null || redisFieldKeys.Count() == 0)
            {
                return new Dictionary<string, T>();
            }
            RedisValue[] results = _cache.HashGet(key, redisFieldKeys);
            if (results == null || results.Count() == 0)
            {
                return new Dictionary<string, T>();
            }
            var dict = new Dictionary<string, T>();
            for (int index = 0, length = redisFieldKeys.Length; index < length; index++)
            {
                string fieldKey = redisFieldKeys[index];
                var fieldValue = results[index].IsNullOrEmpty ? default(T) : DeserializeRedisValue<T>(results[index]);
                if (!dict.ContainsKey(fieldKey))
                {
                    dict.Add(fieldKey, fieldValue);
                }
            }
            return dict;
        }
        public Dictionary<string, T> HGetAll<T>(string key)
        {
            
            var dict = new Dictionary<string, T>();
            HashEntry[] results = _cache.HashGetAll(key);
            if (results == null || results.Length == 0)
            {
                return dict;
            }
            for (int index = 0; index < results.Length; index++)
            {
                var hash = results[index];
                var fieldValue = hash.Value.IsNullOrEmpty ? default(T) : DeserializeRedisValue<T>(hash.Value);
                if (!dict.ContainsKey(hash.Name))
                {
                    dict.Add(hash.Name, fieldValue);
                }
            }
            return dict;
            
        }
        public async Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            if (expiration.HasValue)
            {
                var list = new List<HashEntry>();

                foreach (var item in vals)
                {
                    list.Add(new HashEntry(item.Key, item.Value));
                }

                await _cache.HashSetAsync(cacheKey, list.ToArray());

                var flag = await _cache.KeyExpireAsync(cacheKey, expiration.Value);

                return flag;
            }
            else
            {
                var list = new List<HashEntry>();

                foreach (var item in vals)
                {
                    list.Add(new HashEntry(item.Key, item.Value));
                }

                await _cache.HashSetAsync(cacheKey, list.ToArray());
                return true;
            }
        }

        public async Task<bool> HSetAsync(string cacheKey, string field, string cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return await _cache.HashSetAsync(cacheKey, field, cacheValue);
        }

        public async Task<bool> HExistsAsync(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return await _cache.HashExistsAsync(cacheKey, field);
        }

        public async Task<long> HDelAsync(string cacheKey, IList<string> fields)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (fields != null && fields.Any())
            {
                return await _cache.HashDeleteAsync(cacheKey, fields.Select(x => (RedisValue)x).ToArray());
            }
            else
            {
                var flag = await _cache.KeyDeleteAsync(cacheKey);
                return flag ? 1 : 0;
            }
        }

        public async Task<string> HGetAsync(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var res = await _cache.HashGetAsync(cacheKey, field);
            return res;
        }

        public async Task<Dictionary<string, string>> HGetAllAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dict = new Dictionary<string, string>();

            var vals = await _cache.HashGetAllAsync(cacheKey);

            foreach (var item in vals)
            {
                if (!dict.ContainsKey(item.Name)) dict.Add(item.Name, item.Value);
            }

            return dict;
        }
        public List<string> HKeys(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var keys = _cache.HashKeys(cacheKey);
            return keys.Select(x => x.ToString()).ToList();
        }
        public Dictionary<string, string> HMGet(string cacheKey, IList<string> fields)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(fields, nameof(fields));

            var dict = new Dictionary<string, string>();

            var list = _cache.HashGet(cacheKey, fields.Select(x => (RedisValue)x).ToArray());

            for (int i = 0; i < fields.Count(); i++)
            {
                if (!dict.ContainsKey(fields[i]))
                {
                    dict.Add(fields[i], list[i]);
                }
            }

            return dict;
        }
        public void HSet<T>(string key, Dictionary<string, T> fields, bool ignoreCase = true)
        {
            ArgumentCheck.NotNullAndCountGTZero(fields, nameof(fields));
            HashEntry[] entries = ConvertDictToHashEntryArray(fields, ignoreCase);
            BatchExecute(key, entries, (source) =>
            {
                _cache.HashSet(key, source);
            });
        }

        public void HDelWithIgnoreCase(string cacheKey, IList<string> fields = null, bool ignoreCase = true)
        {
            RedisValue[] keysForDeleting = fields.Select(k =>
            {
                return ignoreCase ? (RedisValue)k.ToLower() : (RedisValue)k;
            }).ToArray();
            BatchExecute(cacheKey, keysForDeleting, (deleteKeys) =>
            {
                _cache.HashDeleteAsync(cacheKey, deleteKeys);
            });
        }

        private void BatchExecute<T>(string key, T[] collection, Action<T[]> redisAction, int batch = 100)
        {
            if (collection == null || collection.Count() == 0)
            {
                return;
            }
            var total = collection.Count();
            var iteration = (total - 1) / batch + 1;
            for (int i = 0; i < iteration; i++)
            {
                var source = collection.Skip(i * batch).Take(batch).ToArray();
                redisAction(source);
            }
        }
        private HashEntry[] ConvertDictToHashEntryArray<T>(Dictionary<string, T> dict, bool ignoreCase = true)
        {
            var list = new List<HashEntry>();
            foreach (var pair in dict)
            {
                list.Add(new HashEntry(ignoreCase ? pair.Key.ToLower() : pair.Key, SerializeItemToRedisValue(pair.Value)));
            }
            return list.ToArray();
        }
        private RedisValue SerializeItemToRedisValue<T>(T item)
        {
            RedisValue value = RedisValue.Null;
            if (object.Equals(item, default(T)))
            {
                return value;
            }
            return JsonConvert.SerializeObject(item, JsonSettings);

        }

        private JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                };
            }
        }

        private RedisValue[] ConvertStrListToRedisValues(List<string> items)
        {
            var values = new RedisValue[] { };
            if (items == null || items.Count == 0)
            {
                return new RedisValue[] { };
            }
            return items.Select(item => (RedisValue)item).ToArray();
        }
        private T DeserializeRedisValue<T>(RedisValue value)
        {
            var result = default(T);

            result = JsonConvert.DeserializeObject<T>(value, JsonSettings);
           
            return result;
        }
    }
}
