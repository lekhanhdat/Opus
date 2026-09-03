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
using CommonModel.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest
{
    public class RedisCacheService
    {
        private ConnectionMultiplexer muxer;
        private string connectionString;

        public RedisCacheService(string connectionString)
        {
            this.connectionString = connectionString;
        }


        private IDatabase GetConnection()
        {
            EnsureMuxer();

            return muxer.GetDatabase();

        }

        private IServer GetServer()
        {
            EnsureMuxer();
            return muxer.GetServer(muxer.GetEndPoints()[0]);
        }

        #region async 

        public async Task AddAsync(string key, string value)
        {
            await GetConnection().StringSetAsync(key, value);
        }

        public async Task AddAsync(string key, string value, DateTimeOffset expiresAt)
        {
            await GetConnection().StringSetAsync(key, value, expiresAt.Subtract(DateTimeOffset.Now));
        }

        public async Task AddAsync(string key, string value, TimeSpan expiresIn)
        {
            await GetConnection().StringSetAsync(key, value, expiresIn);
        }

        public async Task<string> GetAsync(string key)
        {

            var value = await GetConnection().StringGetAsync(key);
            if (value.HasValue)
            {
                return await Task.FromResult<string>((string)value);
            }
            else
            {
                return await Task.FromResult<string>(null);
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await GetConnection().KeyDeleteAsync(key);
        }

        public async Task<bool> AddHashTableAsync(string hashKey, string key, string value)
        {
            return await GetConnection().HashSetAsync(hashKey, key, value);
        }

        public async Task<string> GetHashTableItemAsync(string hashKey, string key)
        {
            var value = await GetConnection().HashGetAsync(hashKey, key);
            return (string)value;
        }

        public async Task<Dictionary<string, string>> GetHashTableAsync(string hashKey)
        {
            var entries = await GetConnection().HashGetAllAsync(hashKey);
            return entries.ToDictionary(x => x.Name.ToString(),
                             x => (string)x.Value,
                             StringComparer.Ordinal);
        }

        public async Task<bool> HashTableItemExistsAsync(string hashKey, string key)
        {
            return await GetConnection().HashExistsAsync(hashKey, key);
        }

        public async Task<bool> DeleteHashTableItemAsync(string hashKey, string key)
        {
            return await GetConnection().HashDeleteAsync(hashKey, key);
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan expiresIn)
        {
            return await GetConnection().KeyExpireAsync(key, expiresIn);
        }

        #endregion

        #region old

        private void EnsureMuxer()
        {
            if (muxer == null || !muxer.IsConnected || muxer.GetDatabase() == null)
            {
                muxer = ConnectionMultiplexer.Connect(connectionString);
            }
        }

        public bool Add(string key, string value)
        {
            return GetConnection().StringSet(key, value);
        }

        public bool Add(string key, string value, DateTimeOffset expiresAt)
        {
            return GetConnection().StringSet(key, value, expiresAt.Subtract(DateTimeOffset.Now));
        }

        public bool AddAll(IList<Tuple<string, string>> items)
        {
            var values = items
                .Select(i => new KeyValuePair<RedisKey, RedisValue>(i.Item1, i.Item2))
                .ToArray();
            return GetConnection().StringSet(values);
        }

        public bool Add(string key, string value, TimeSpan expiresIn)
        {
            return GetConnection().StringSet(key, value, expiresIn);
        }

        public bool Add(string key, string value, string tag, TimeSpan expiresIn)
        {
            return Add(key, value, expiresIn);
        }

        public string Get(string key)
        {
            var value = GetConnection().StringGet(key);
            if (value.HasValue)
            {
                return (string)value;
            }
            return null;
        }

        public IDictionary<string, string> GetAll(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(i => (RedisKey)i).ToArray();
            var result = GetConnection().StringGet(redisKeys);

            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < redisKeys.Length; index++)
            {
                var value = result[index];
                dict.Add((string)redisKeys[index], (string)value);
            }

            return dict;
        }

        public bool KeyExists(string key)
        {
            return GetConnection().KeyExists(key);
        }

        public bool Remove(string key)
        {
            return GetConnection().KeyDelete(key);
        }

        public void ClearCache()
        {
            GetServer().FlushDatabase();
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            keys.ToList().ForEach(k => Remove(k));
        }

        public bool Replace(string key, string value)
        {
            return Add(key, value);
        }

        public bool Replace(string key, string value, DateTimeOffset expiresAt)
        {
            return Add(key, value, expiresAt);
        }

        public bool Replace(string key, string value, TimeSpan expiresIn)
        {
            return Add(key, value, expiresIn);
        }

        public bool AddHashSet(string key, string item)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key cannot be empty.", nameof(key));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "item cannot be null.");
            }

            return GetConnection().SetAdd(key, item);
        }

        public IEnumerable<string> GetHashSet(string memberName)
        {
            return GetConnection().SetMembers(memberName).Select(i => (string)i);
        }


        public void AddHashTable(string hashKey, Dictionary<string, string> values)
        {
            var entries = values.Select(kv => new HashEntry(kv.Key, kv.Value));
            GetConnection().HashSet(hashKey, entries.ToArray());
        }

        public bool AddHashTable(string hashKey, string key, string value)
        {
            return GetConnection().HashSet(hashKey, key, value);
        }

        public bool HashTableItemExists(string hashKey, string key)
        {
            return GetConnection().HashExists(hashKey, key);
        }

        public string GetHashTableItem(string hashKey, string key)
        {
            var value = GetConnection().HashGet(hashKey, key);
            return (string)value;
        }

        public T GetHashTableObject<T>(string hashKey)
        {
            var result = GetConnection().HashGetAll(hashKey).ToDictionary(x => x.Name.ToString(),
                             x => (string)x.Value,
                             StringComparer.Ordinal).ConvertToObject<T>();
            return result;
        }

        public Dictionary<string, string> GetHashTableItems(string hashKey, IEnumerable<string> keys)
        {
            return keys.Select(x => new { key = x, value = GetHashTableItem(hashKey, x) })
                       .ToDictionary(kv => (string)kv.key, kv => (string)kv.value, StringComparer.Ordinal);
        }

        public Dictionary<string, string> GetHashTable(string hashKey)
        {
            return GetConnection().HashGetAll(hashKey).ToDictionary(x => x.Name.ToString(),
                             x => (string)x.Value,
                             StringComparer.Ordinal);
        }

        public bool DeleteHashTableItem(string hashKey, string key)
        {
            return GetConnection().HashDelete(hashKey, key);
        }

        public long DeleteHashTableItems(string hashKey, IEnumerable<string> keys)
        {
            return GetConnection().HashDelete(hashKey, keys.Select(x => (RedisValue)x).ToArray());
        }

        public bool AddBlob(string key, byte[] value)
        {
            var tas = GetConnection().StringSet(key, value);
            return tas;
        }

        public bool AddBlob(string key, byte[] value, TimeSpan expiresIn)
        {
            var tas = GetConnection().StringSet(key, value, expiresIn);
            return tas;
        }

        public bool AddBlob(string key, byte[] value, DateTimeOffset expiresAt)
        {
            var tas = GetConnection().StringSet(key, value, expiresAt - DateTimeOffset.Now);
            return tas;
        }

        public byte[] GetBlob(string key)
        {
            var res = GetConnection().StringGet(key);
            return (byte[])res;
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            List<string> keys = new List<string>();
            var dbkeys = GetServer().Keys(GetConnection().Database, pattern);
            foreach (var key in dbkeys)
            {
                if (!keys.Contains(key.ToString()))
                {
                    keys.Add((string)key.ToString());
                }
            }
            return keys;
        }

        public bool Add<T>(string key, T value)
        {
            return Add(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value));
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return Add(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value), expiresIn);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt)
        {
            return Add(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value), expiresAt);
        }

        public T Get<T>(string key)
        {
            var value = Get(key);
            if (value != null)
            {
                return SerializerHelper.DeserializeByJsonConvert<T>(value);
            }
            return default(T);
        }

        public bool Replace<T>(string key, T value)
        {
            return Replace(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value));
        }

        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt)
        {
            return Replace(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value), expiresAt);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return Replace(key, SerializerHelper.SerializeByJsonConvertIgnoreNull(value), expiresIn);
        }

        #endregion 
    }

    public static class HashCacheHelper
    {
        private static readonly Type[] simpleDataTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(int),
            typeof(uint),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(char),
            typeof(bool),
            typeof(string),
        };

        public static T ConvertToObject<T>(this Dictionary<string, string> hash)
        {
            var props = typeof(T).GetProperties();
            var result = Activator.CreateInstance<T>();
            foreach (var propertyInfo in props)
            {
                if (hash.ContainsKey(propertyInfo.Name))
                {
                    var stringVal = hash[propertyInfo.Name];
                    var propVal = JsonConvert.DeserializeObject(stringVal, propertyInfo.PropertyType);
                    if (propVal != null)
                    {
                        propertyInfo.SetValue(result, propVal);
                    }
                }
            }
            return result;
        }


        public static string GetPropertyName<TO, TP>(this Expression<Func<TO, TP>> expression)
        {
            var body = expression.Body as MemberExpression;
            var member = body?.Member.Name;
            return member;
        }
    }
}
