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
        private readonly string _name;
        private readonly IDatabase _cache;
        private readonly ILogger _logger;
        private readonly RedisOptions _options;
        private readonly IRedisDatabaseProvider _dbProvider;
        public string RedisName => this._name;
        public RedisCacheProvider(
            string name,
            IEnumerable<IRedisDatabaseProvider> dbProviders, 
            RedisOptions redisOptions, 
            ILoggerFactory loggerFactory)
        {
            this._name = name;
            this._dbProvider = dbProviders.Single(x => x.DBProviderName.Equals(name));
            this._options = redisOptions;
            this._logger = loggerFactory?.CreateLogger<RedisCacheProvider>();
            this._cache = _dbProvider.GetDatabase();
        }
        public async Task<bool> IsRedisAvailable() 
        {
            try
            {
                var pingTask = this._cache.PingAsync();
                var completed = await Task.WhenAny(pingTask, Task.Delay(2000));
                if (completed != pingTask)
                {
                    this._logger?.LogWarning("Redis ping timed out after 2 seconds.");
                    return false;
                }
                var result = await pingTask;
                return result.TotalSeconds > 1E-06;
            }
            catch (Exception ex)
            {
                this._logger?.LogError($"Error occurred while checking Redis availability. {ex}");
                return false;
            }
        }
    }
}
