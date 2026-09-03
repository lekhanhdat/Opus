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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RedisCache;
using Medallion.Threading.Redis;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Lcoker
{
    public class RMRedisLockHandler
    {

        private RMRedisLockHandler() { }

        public static Task<RMRedisLocker> LockAsync(RMRedisLockKey lockKey)
        {
            return LockAsync(TenantLocalValue.LogonGroupId, lockKey, "default_prefix", TimeSpan.FromHours(2));
        }

        public static Task<RMRedisLocker> LockAsync(RMRedisLockKey lockKey, TimeSpan timeout)
        {
            return LockAsync(TenantLocalValue.LogonGroupId, lockKey, "default_prefix", timeout);
        }

        public static Task<RMRedisLocker> LockAsync(RMRedisLockKey lockKey, string prefix)
        {
            return LockAsync(TenantLocalValue.LogonGroupId, lockKey, prefix, TimeSpan.FromHours(2));
        }

        public static Task<RMRedisLocker> LockAsync(RMRedisLockKey lockKey, string prefix, TimeSpan timeout)
        {
            return LockAsync(TenantLocalValue.LogonGroupId, lockKey, prefix, timeout);
        }

        public static async Task<RMRedisLocker> LockAsync(string tenantId, RMRedisLockKey lockKey, string prefix, TimeSpan timeout)
        {
            var redisKey = $"{tenantId.ToLower()}_{prefix}_{lockKey}";
            var connectionSring = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.RECO_REDIS_CONNECTION_STRING];
            var isGCPEnv = RMGlobalConfiguration.EnvSetting.IsGCPEnvironment;
            var isDevEnv = RMGlobalConfiguration.EnvSetting.IsDevEnvironment;
            var connection = await RedisConnectionFactory.ConnectAsync(connectionSring, isGCPEnv, isDevEnv);
            var distributedLock = new RedisDistributedLock(redisKey, connection.GetDatabase());
            var handle = await distributedLock.AcquireAsync(timeout);
            return new RMRedisLocker(handle);
        }
    }

    public class RMRedisLocker : IAsyncDisposable, IDisposable
    {

        private readonly RedisDistributedLockHandle _handle;

        internal RMRedisLocker(RedisDistributedLockHandle handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            _handle.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _handle.DisposeAsync();
        }
    }

    public enum RMRedisLockKey
    {
        None = 0,
        DiscoveryConfiguration = 1,
        DiscoveryJob = 2,
        DiscoveryAnalysisFileType = 3,
        DiscoveryAnalysisContainer = 4,
        DiscoveryAnalysisContainerInactiveData = 5,
        DiscoveryAnalysisBasicInactiveData = 6,
        DiscoveryAnalysisContainerRotData = 7,
        DiscoveryAnalysisBasicRotData = 8,
        DiscoveryAnalysisAggregateTotalData = 9,
        DiscoveryQuery = 10,
        InitNodesFromAOS = 11,
        DiscoveryOptimizationCalculate = 12,
        DiscoveryOptimizationJobCancel = 13,
        SOCheckMergeIndexLock = 14,
        DiscoveryGoogleConfiguration = 15
    }
}
