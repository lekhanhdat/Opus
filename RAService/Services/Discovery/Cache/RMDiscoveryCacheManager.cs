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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.RedisCache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Cache
{
    public class RMDiscoveryCacheManager
    {
        private readonly RALogger _logger;

        private readonly string _cacheKey;

        private IRedisCacheProvider CacheProvider => RedisCacheService.CacheProvider;

        public RMDiscoveryCacheManager(string dataSourceId, RMDiscoveryCacheDataSource dataSource)
        {
            _logger = RALogger.GetInstance(typeof(RMDiscoveryCacheManager));
            _cacheKey = $"{TenantLocalValue.LogonGroupId}_{dataSource}_{dataSourceId}_Discovery_Query";
        }

        public RMDiscoveryCacheManager(Guid dataSourceId, RMDiscoveryCacheDataSource dataSource)
        {
            _logger = RALogger.GetInstance(typeof(RMDiscoveryCacheManager));
            _cacheKey = $"{TenantLocalValue.LogonGroupId}_{dataSource}_{dataSourceId}_Discovery_Query";
        }

        public async Task<T> TryGetAsync<T>(string funcName, RMDiscoveryOffice365QueryParameter parameter, Func<Task<T>> funcAsync)
        {
            var key = $"{funcName}_Office365_{JsonConvert.SerializeObject(parameter).GetHashCode()}";
            try
            {
                if(!await CacheProvider.IsRedisAvailable())
                {
                    throw new Exception("The redis is unavailable.");
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(1)))
                {
                    var hasField = await CacheProvider.HExistsAsync(_cacheKey, key);
                    if (!hasField)
                    {
                        var res = await funcAsync();
                        await CacheProvider.HSetAsync(_cacheKey, key.ToString(), JsonConvert.SerializeObject(res));
                        await CacheProvider.KeyExpireAsync(_cacheKey, TimeSpan.FromDays(1));
                    }

                    var valueJson = await CacheProvider.HGetAsync(_cacheKey, key.ToString());
                    return JsonConvert.DeserializeObject<T>(valueJson);
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get discovery data from redis by key [{key}]. Error: {e}");
                return await funcAsync();
            }
        }

        public async Task<T> TryGetAsync<T>(string funcName, RMDiscoveryGoogleQueryParameter parameter, Func<Task<T>> funcAsync)
        {
            var key = $"{funcName}_Google_{JsonConvert.SerializeObject(parameter).GetHashCode()}";
            try
            {
                if (!await CacheProvider.IsRedisAvailable())
                {
                    throw new Exception("The redis is unavailable.");
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(10)))
                {
                    var hasField = await CacheProvider.HExistsAsync(_cacheKey, key);
                    if (!hasField)
                    {
                        var res = await funcAsync();
                        await CacheProvider.HSetAsync(_cacheKey, key.ToString(), JsonConvert.SerializeObject(res));
                        await CacheProvider.KeyExpireAsync(_cacheKey, TimeSpan.FromDays(1));
                    }

                    var valueJson = await CacheProvider.HGetAsync(_cacheKey, key.ToString());
                    return JsonConvert.DeserializeObject<T>(valueJson);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get discovery data from redis by key [{key}]. Error: {e}");
                return await funcAsync();
            }
        }
        
        public async Task<T> TryGetAsync<T>(string funcName, RMDiscoverySalesforceQueryParameter parameter, Func<Task<T>> funcAsync)
        {
            var key = $"{funcName}_Salesforce_{JsonConvert.SerializeObject(parameter).GetHashCode()}";
            try
            {
                if (!await CacheProvider.IsRedisAvailable())
                {
                    throw new Exception("The redis is unavailable.");
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(10)))
                {
                    var hasField = await CacheProvider.HExistsAsync(_cacheKey, key);
                    if (!hasField)
                    {
                        var res = await funcAsync();
                        await CacheProvider.HSetAsync(_cacheKey, key.ToString(), JsonConvert.SerializeObject(res));
                        await CacheProvider.KeyExpireAsync(_cacheKey, TimeSpan.FromDays(1));
                    }

                    var valueJson = await CacheProvider.HGetAsync(_cacheKey, key.ToString());
                    return JsonConvert.DeserializeObject<T>(valueJson);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get discovery data from redis by key [{key}]. Error: {e}");
                return await funcAsync();
            }
        }

        public async Task<T> TryGetAsync<T>(string funcName, RMDiscoveryAOSPQueryParameter parameter, Func<Task<T>> funcAsync)
        {
            var key = $"{funcName}_AOSP_{JsonConvert.SerializeObject(parameter).GetHashCode()}";
            try
            {
                if (!await CacheProvider.IsRedisAvailable())
                {
                    throw new Exception("The redis is unavailable.");
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(10)))
                {
                    var hasField = await CacheProvider.HExistsAsync(_cacheKey, key);
                    if (!hasField)
                    {
                        var res = await funcAsync();
                        await CacheProvider.HSetAsync(_cacheKey, key.ToString(), JsonConvert.SerializeObject(res));
                        await CacheProvider.KeyExpireAsync(_cacheKey, TimeSpan.FromDays(1));
                    }

                    var valueJson = await CacheProvider.HGetAsync(_cacheKey, key.ToString());
                    return JsonConvert.DeserializeObject<T>(valueJson);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get discovery data from redis by key [{key}]. Error: {e}");
                return await funcAsync();
            }
        }

        public async Task<T> TryGetAsync<T>(string funcName, RMDiscoveryFSQueryParameter parameter, Func<Task<T>> funcAsync)
        {
            var key = $"{funcName}_FileSystem_{JsonConvert.SerializeObject(parameter).GetHashCode()}";
            try
            {
                if (!await CacheProvider.IsRedisAvailable())
                {
                    throw new Exception("The redis is unavailable.");
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(1)))
                {
                    var hasField = await CacheProvider.HExistsAsync(_cacheKey, key);
                    if (!hasField)
                    {
                        var res = await funcAsync();
                        await CacheProvider.HSetAsync(_cacheKey, key.ToString(), JsonConvert.SerializeObject(res));
                        await CacheProvider.KeyExpireAsync(_cacheKey, TimeSpan.FromDays(1));
                    }

                    var valueJson = await CacheProvider.HGetAsync(_cacheKey, key.ToString());
                    return JsonConvert.DeserializeObject<T>(valueJson);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get discovery data from redis by key [{key}]. Error: {e}");
                return await funcAsync();
            }
        }


        public async Task ClearAsync()
        {
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryQuery, _cacheKey, TimeSpan.FromMinutes(10)))
                {
                    if (await CacheProvider.KeyExistsAsync(_cacheKey))
                    {
                        await CacheProvider.KeyDelAsync(_cacheKey);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while clear redis cache for discovery. Error: {e}");
            }
        }
    }

    public enum RMDiscoveryCacheDataSource
    {
        None = 0,
        Office365 = 1,
        Google = 2,
        Salesforce = 3,
        AOSPOffice365 = 4,
        FileSystem = 5,
    }
}
