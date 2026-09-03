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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RedisCache;
using Renci.SshNet;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Cache
{
    public class RMRedisCache : IRMCache
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMRedisCache));
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromHours(1);
        private static bool _isRedisAvailable = true;
        private static DateTime _nextCheckConnectionTime = DateTime.MinValue;
        private static readonly SemaphoreSlim _availabilityCheckLock = new SemaphoreSlim(1, 1);
        private IRedisCacheProvider CacheProvider
        {
            get
            {
                return RedisCacheService.CacheProvider;
            }
        }

        private static ConcurrentDictionary<string, SemaphoreSlim> SemahoreSlimDic = new System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim>();

        public async Task<T> GetAsync<T>(string key, bool BuildTenantKey = true)
        {
            if (BuildTenantKey)
            {
                key = GetTenantKey(key);
            }
            var value = await RedisUtils.ExecuteAsync<string>(() => { return CacheProvider.StringGetAsync(key); });
            if (!string.IsNullOrEmpty(value))
            {
                var data = value.ToString();
                return Decode<T>(data);
            }
            return default(T);
        }

        public async Task<T> TryGetAsync<T>(string key, Func<Task<T>> dataProvider, TimeSpan duration = default(TimeSpan), bool BuildTenantKey = true)
        {
            try
            {
                if (!await CheckRedisAvailable())
                {
                    return await dataProvider();
                }
                if (BuildTenantKey)
                {
                    key = GetTenantKey(key);
                }
                var value = await RedisUtils.ExecuteAsync<string>(() => { return CacheProvider.StringGetAsync(key); });
                if (!string.IsNullOrEmpty(value))
                {
                    var data = value.ToString();
                    return Decode<T>(data);
                }
                else
                {
                    var exsit = SemahoreSlimDic.TryGetValue(key, out var semaphoreSlim);
                    if (!exsit)
                    {
                        SemahoreSlimDic.TryAdd(key, new SemaphoreSlim(1));
                        semaphoreSlim = SemahoreSlimDic[key];
                    }

                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        value = await RedisUtils.ExecuteAsync<string>(() => { return CacheProvider.StringGetAsync(key); });
                        if (!string.IsNullOrEmpty(value))
                        {
                            var data = value.ToString();
                            return Decode<T>(data);
                        }
                        else
                        {
                            var result = await dataProvider();
                            if (duration == default(TimeSpan)) { duration = DefaultTimeout; }
                            await this.SetAsync(key, result, duration, false);
                            return result;
                        }
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"TryGetAsync error,will return reuslt from db, key:{key}, error:{e}");
                return await dataProvider();
            }
        }
        public async Task<bool> CheckRedisAvailable()
        {
            if (!_isRedisAvailable && _nextCheckConnectionTime > DateTime.UtcNow)
            {
                logger.Debug($"Redis is unavailable, {_nextCheckConnectionTime}");
                return _isRedisAvailable;
            }
            if (_isRedisAvailable && _nextCheckConnectionTime > DateTime.UtcNow)
            {
                return _isRedisAvailable;
            }
            if (!await _availabilityCheckLock.WaitAsync(0))
            {
                return _isRedisAvailable;
            }
            try
            {
                _isRedisAvailable = await CacheProvider.IsRedisAvailable();
                _nextCheckConnectionTime = DateTime.UtcNow.AddMinutes(5);
                return _isRedisAvailable;
            }
            finally
            {
                _availabilityCheckLock.Release();
            }
        }

        public bool GetCachedRedisAvailability()
        {
            return _isRedisAvailable;
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan duration = default(TimeSpan), bool BuildTenantKey = true)
        {
            if (BuildTenantKey)
            {
                key = GetTenantKey(key);
            }
            if (duration == default(TimeSpan)) { duration = DefaultTimeout; }
            return RedisUtils.ExecuteAsync(() => CacheProvider.StringSetAsync(key, Encode(value), duration));
        }

        public Task<bool> RenewAsync(string key, TimeSpan duration, bool BuildTenantKey = true)
        {
            if (BuildTenantKey)
            {
                key = GetTenantKey(key);
            }
            return RedisUtils.ExecuteAsync(() => CacheProvider.KeyExpireAsync(key, duration));
        }

        public async Task SetListAsync<T>(string key, IEnumerable<T> value)
        {
            key = GetTenantKey(key);
            await CacheProvider.KeyDelAsync(key);
            var vals = value.Select(v => (RedisValue)Encode(v)).ToArray();
            await CacheProvider.ListRightPushAsync(key, vals);
        }

        public Task ListAddAsync<T>(string key, T value)
        {
            key = GetTenantKey(key);
            return RedisUtils.ExecuteAsync(() => CacheProvider.ListRightPushAsync(key, Encode(value)));
        }

        public async Task<List<T>> GetListAsync<T>(string key)
        {
            key = GetTenantKey(key);
            var values = await RedisUtils.ExecuteAsync(() => CacheProvider.ListRangeAsync(key));
            if (values.Any())
            {
                var list = new List<T>();
                foreach (var item in values)
                {
                    list.Add(Decode<T>(item));
                }
                return list;
            }
            return default(List<T>);
        }

        public async Task<bool> RemoveAsync(string key, bool BuildTenantKey = true)
        {
            if (!await CheckRedisAvailable())
            {
                return true;
            }
            if (BuildTenantKey)
            {
                key = GetTenantKey(key);
            }
            return await RedisUtils.ExecuteAsync(() => CacheProvider.KeyDelAsync(key));
        }

        public async Task<long> RemoveAsync(string[] keys)
        {
            if (!await CheckRedisAvailable())
            {
                return 0;
            }
            keys = GetTenantKey(keys);
            return await RedisUtils.ExecuteAsync(() => CacheProvider.KeyDelAsync(keys));
        }

        public Task<bool> KeyExpiredAsync(string key, int second)
        {
            key = GetTenantKey(key);
            return RedisUtils.ExecuteAsync(() => CacheProvider.KeyExpireAsync(key, second));
        }

        public Task<bool> ExistAsync(string key)
        {
            key = GetTenantKey(key);
            return RedisUtils.ExecuteAsync(() => CacheProvider.KeyExistsAsync(key));
        }


        private static string Encode(object value)
        {
            return SerializerHelper.SerializeToBase64StringByDataContractSerializer(value);
        }

        private static T Decode<T>(string message)
        {
            return SerializerHelper.DeserializeFromBase64StringByDataContractSerializer<T>(message);
        }

        private static string GetTenantKey(string key)
        {
            return TenantLocalValue.LogonGroupId + key;
        }

        private static string[] GetTenantKey(string[] keys)
        {
            List<string> keysWithTenantId = new List<string>(keys.Length);
            foreach (var key in keys)
            {
                keysWithTenantId.Add(TenantLocalValue.LogonGroupId + key);
            }
            return keysWithTenantId.ToArray();
        }


        #region keys


        #endregion
    }


    /// <summary>
    /// duplicate remove.
    /// </summary>
    public class RMRedisCacheManager : IRMCacheManager
    {
        public readonly IRMCache cache = PlatformWindsorManager.GetService<IRMCache>();

        private event CacheInvalidateHandler SimpleInfoAdded;
        private event CacheInvalidateHandler SimpleInfoUpdated;
        private event CacheInvalidateHandler SimpleInfoDeleted;
        private event CacheInvalidateHandler GeneralSetingAdded;
        private event CacheInvalidateHandler GeneralSettingUpdated;
        private event CacheInvalidateHandler UserUpdated;
        private event CacheInvalidateHandler UserDeleted;
        private event CacheInvalidateHandler UserRemovedStatusChanged;
        private event CacheInvalidateHandler LnkUserGroupDeleted;
        private event CacheInvalidateHandler LnkUserGroupUpdated;
        private event CacheInvalidateHandler LnkUserGroupAdded;
        private event CacheInvalidateHandler ArchiverDatabaseConfigUpdated;

        public IRMCache Cache => cache;

        CacheInvalidateHandler IRMCacheManager.SimpleInfoAdded => SimpleInfoAdded;

        CacheInvalidateHandler IRMCacheManager.SimpleInfoUpdated => SimpleInfoUpdated;

        CacheInvalidateHandler IRMCacheManager.SimpleInfoDeleted => SimpleInfoDeleted;

        CacheInvalidateHandler IRMCacheManager.GeneralSetingAdded => GeneralSetingAdded;

        CacheInvalidateHandler IRMCacheManager.GeneralSettingUpdated => GeneralSettingUpdated;

        CacheInvalidateHandler IRMCacheManager.UserUpdated => UserUpdated;

        CacheInvalidateHandler IRMCacheManager.UserDeleted => UserDeleted;

        CacheInvalidateHandler IRMCacheManager.UserRemovedStatusChanged => UserRemovedStatusChanged;

        CacheInvalidateHandler IRMCacheManager.LnkUserGroupDeleted => LnkUserGroupDeleted;

        CacheInvalidateHandler IRMCacheManager.LnkUserGroupUpdated => LnkUserGroupUpdated;

        CacheInvalidateHandler IRMCacheManager.LnkUserGroupAdded => LnkUserGroupAdded;

        CacheInvalidateHandler IRMCacheManager.ArchiverDatabaseConfigUpdated => ArchiverDatabaseConfigUpdated;

        public RMRedisCacheManager()
        {
            Init();
        }

        private Task ClearCacheAsync(string key, string[] keyss)
        {
            if (keyss == null)
            {
                return Cache.RemoveAsync(key);
            }
            else
            {
                var keys = keyss.Select(fix => key + fix).ToArray();
                return Cache.RemoveAsync(keys);
            }
        }


        public void Init()
        {
            SimpleInfoAdded += RMRedisCacheManager_SimpleInfoAdded;
            SimpleInfoUpdated += RMRedisCacheManager_SimpleInfoUpdated;
            SimpleInfoDeleted += RMRedisCacheManager_SimpleInfoDeleted;
            GeneralSetingAdded += RMRedisCacheManager_GeneralSetingAdded;
            GeneralSettingUpdated += RMRedisCacheManager_GeneralSettingUpdated;
            UserUpdated += RMRedisCacheManager_UserUpdated;
            UserDeleted += RMRedisCacheManager_UserDeleted;
            UserRemovedStatusChanged += RMRedisCacheManager_UserRemovedStatusChanged;
            UserRemovedStatusChanged += RMRedisCacheManager_UserUpdated;
            LnkUserGroupDeleted += RMRedisCacheManager_LnkUserGroupDeleted;
            LnkUserGroupUpdated += RMRedisCacheManager_LnkUserGroupUpdated;
            LnkUserGroupAdded += RMRedisCacheManager_LnkUserGroupAdded;
            ArchiverDatabaseConfigUpdated += RMRedisCacheManager_ArchiverDatabaseConfigUpdated;
        }

        private Task RMRedisCacheManager_ArchiverDatabaseConfigUpdated(KeyType type = KeyType._Default, params string[] keys)
        {
            throw new NotImplementedException();
        }

        private Task RMRedisCacheManager_LnkUserGroupAdded(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.LnkUserGroupDao_GetAllGroupIdsAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_LnkUserGroupUpdated(KeyType type = KeyType._Default, params string[] keys)
        {
            throw new NotImplementedException();
        }

        private Task RMRedisCacheManager_LnkUserGroupDeleted(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.LnkUserGroupDao_GetAllGroupIdsAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_UserRemovedStatusChanged(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();

            switch (type)
            {
                case KeyType._Default:
                case KeyType.User_Id:
                    tasks.Add(ClearCacheAsync(IRMCache.Keys.AccountDao_GetUserById, keys));
                    break;
                case KeyType.User_UserId:
                    break;
            }

            //tasks.Add(ClearCacheAsync(IRMCache.Keys.AccountDao_GetIdsOfUserByUserIdsAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_UserDeleted(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.AccountDao_GetUserById, keys));
            //tasks.Add(ClearCacheAsync(IRMCache.Keys.AccountDao_GetIdsOfUserByUserIdsAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_UserUpdated(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();

            switch (type)
            {
                case KeyType._Default:
                case KeyType.User_Id:
                    tasks.Add(ClearCacheAsync(IRMCache.Keys.AccountDao_GetUserById, keys));
                    break;
                case KeyType.User_UserId:
                    break;
            }

            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_GeneralSettingUpdated(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.GeneralSettingService_GetGeneralSettingAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_GeneralSetingAdded(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.GeneralSettingService_GetGeneralSettingAsync, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_SimpleInfoDeleted(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_SimpleInfoUpdated(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, keys));
            return Task.WhenAll(tasks);
        }

        private Task RMRedisCacheManager_SimpleInfoAdded(KeyType type = KeyType._Default, params string[] keys)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ClearCacheAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, keys));
            return Task.WhenAll(tasks);
        }
    }
}
