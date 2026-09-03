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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.RateLimitsPolicyManager.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace AvePoint.RA.Common.RateLimitsPolicyManager
{
    public class RateLimitsPolicyManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RateLimitsPolicyManager));
        private RMRateLimitsPolicy mGlobalRateLimitsPolicy;
        private readonly ConcurrentDictionary<string, RMRateLimitsPolicy> tenantPolicyDic = null;

        #region default values
        private const int GlobalPolicyExpiredMinutes = 120;
        private const int DefaultGlobalNumberOfExecutions = 3000;
        private const int DefaultGlobalMaxBurst = 100;
        private const int DefaultGlobalTimeSpanSecond = 60;

        private const int TenantPolicyExpiredMinutes = 30;
        private const int DefaultNumberOfExecutions = 600;
        private const int DefaultMaxBurst = 20;
        private const int DefaultTimeSpanSecond = 60;
        private readonly string RateLimitsKey = $"{KeyNameCollection.API_Rate_Limits}{RMNameValueDto.Seprator}{RMNameValueType.RateLimitsPolicy}";
        private readonly string GlobalRateLimitsKey = $"{KeyNameCollection.API_Rate_Limits}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.GlobalRateLimitsPolicy}";
        private readonly object policyLock = new object();
        #endregion

        #region interface      
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IGlobalKeyValueService GlobalKeyValueService => PlatformWindsorManager.GetService<IGlobalKeyValueService>();
        #endregion

        public RateLimitsPolicyManager()
        {
            tenantPolicyDic = new ConcurrentDictionary<string, RMRateLimitsPolicy>();
        }
        public RMRateLimitsPolicy GlobalRateLimitsPolicy
        {
            get
            {
                if (mGlobalRateLimitsPolicy == null || mGlobalRateLimitsPolicy.InitialTime.AddMinutes(GlobalPolicyExpiredMinutes) < DateTime.UtcNow)
                {
                    lock (policyLock)
                    {
                        if (mGlobalRateLimitsPolicy == null || mGlobalRateLimitsPolicy.InitialTime.AddMinutes(GlobalPolicyExpiredMinutes) < DateTime.UtcNow)
                        {
                            using (var performance = new PerformanceScope("RateLimitsPolicyManager.GlobalRateLimitsPolicy"))
                            {
                                var dbLimit = GetGlobalRateLimitFromDb();
                                if (mGlobalRateLimitsPolicy == null)
                                {
                                    mGlobalRateLimitsPolicy = new RMRateLimitsPolicy()
                                    {
                                        CachedRateLimit = dbLimit,
                                        CurrentPolicy = Policy.RateLimit(dbLimit.NumberOfExecutions, TimeSpan.FromSeconds(dbLimit.TimeSpanSecond), dbLimit.MaxBurst)
                                    };
                                }
                                else if (RateLimitChanged(mGlobalRateLimitsPolicy.CachedRateLimit, dbLimit))
                                {
                                    mGlobalRateLimitsPolicy.CachedRateLimit = dbLimit;
                                    mGlobalRateLimitsPolicy.CurrentPolicy = Policy.RateLimit(dbLimit.NumberOfExecutions, TimeSpan.FromSeconds(dbLimit.TimeSpanSecond), dbLimit.MaxBurst);
                                }
                                mGlobalRateLimitsPolicy.InitialTime = DateTime.UtcNow;
                            }
                        }
                    }
                }
                return mGlobalRateLimitsPolicy;
            }
        }
        public RMRateLimitsPolicy GetTenantRateLimitPolicy(string customerId)
        {
            RMRateLimitsPolicy rateLimitsPolicy = null;
            if (tenantPolicyDic.ContainsKey(customerId))
            {
                rateLimitsPolicy = tenantPolicyDic[customerId];
                if (rateLimitsPolicy.InitialTime.AddMinutes(TenantPolicyExpiredMinutes) < DateTime.UtcNow)
                {
                    using (var performance = new PerformanceScope("RateLimitsPolicyManager.GetTenantRateLimitPolicy"))
                    {
                        var dbLimit = GetTenantRateLimitFromDb();
                        if (RateLimitChanged(rateLimitsPolicy.CachedRateLimit, dbLimit))
                        {
                            rateLimitsPolicy.CurrentPolicy = Policy.RateLimit(dbLimit.NumberOfExecutions, TimeSpan.FromSeconds(dbLimit.TimeSpanSecond), dbLimit.MaxBurst);
                        }
                        rateLimitsPolicy.InitialTime = DateTime.UtcNow;
                    }
                }
            }
            else
            {
                lock (policyLock)
                {
                    if (tenantPolicyDic.ContainsKey(customerId))
                    {
                        rateLimitsPolicy = tenantPolicyDic[customerId];
                        if (rateLimitsPolicy.InitialTime.AddMinutes(TenantPolicyExpiredMinutes) < DateTime.UtcNow)
                        {
                            using (var performance = new PerformanceScope("RateLimitsPolicyManager.GetTenantRateLimitPolicy"))
                            {
                                var dbLimit = GetTenantRateLimitFromDb();
                                if (RateLimitChanged(rateLimitsPolicy.CachedRateLimit, dbLimit))
                                {
                                    rateLimitsPolicy.CurrentPolicy = Policy.RateLimit(dbLimit.NumberOfExecutions, TimeSpan.FromSeconds(dbLimit.TimeSpanSecond), dbLimit.MaxBurst);
                                }
                                rateLimitsPolicy.InitialTime = DateTime.UtcNow;
                            }
                        }
                    }
                    else
                    {
                        using (var performance = new PerformanceScope("RateLimitsPolicyManager.InitTenantRateLimitPolicy"))
                        {
                            var dbLimit = GetTenantRateLimitFromDb();
                            rateLimitsPolicy = new RMRateLimitsPolicy() { InitialTime = DateTime.UtcNow, CachedRateLimit = dbLimit };
                            rateLimitsPolicy.CurrentPolicy = Policy.RateLimit(dbLimit.NumberOfExecutions, TimeSpan.FromSeconds(dbLimit.TimeSpanSecond), dbLimit.MaxBurst);
                            tenantPolicyDic.AddOrReplaceInternal(customerId, rateLimitsPolicy);
                        }
                    }
                }               
            }
            return rateLimitsPolicy;
        }

        private bool RateLimitChanged(RateLimitDto cachedLimit, RateLimitDto dbLimit)
        {
            if (cachedLimit.NumberOfExecutions != dbLimit.NumberOfExecutions
                || cachedLimit.TimeSpanSecond != dbLimit.TimeSpanSecond
                || cachedLimit.MaxBurst != dbLimit.MaxBurst)

            {
                logger.Info($"API Rate Limit Changed. Current NumberOfExecutions:{dbLimit.NumberOfExecutions} TimeSpanSecond:{dbLimit.TimeSpanSecond} MaxBurst:{dbLimit.MaxBurst}");
                return true;
            }
            return false;
        }

        private RateLimitDto GetTenantRateLimitFromDb()
        {
            var policy = KeyValueService.Get(RateLimitsKey);
            if (policy != null && !string.IsNullOrWhiteSpace(policy.Value))
            {
                var dto = SerializerHelper.DeserializeByJsonConvert<RateLimitDto>(policy.Value);
                if (dto.MaxBurst < 1)
                {
                    dto.MaxBurst = DefaultMaxBurst;
                }
                if (dto.NumberOfExecutions < 1)
                {
                    dto.NumberOfExecutions = DefaultNumberOfExecutions;
                }
                if (dto.TimeSpanSecond < 1)
                {
                    dto.TimeSpanSecond = DefaultTimeSpanSecond;
                }
                return dto;
            }
            else
            {
                RateLimitDto defaultPolicy = new RateLimitDto()
                {
                    NumberOfExecutions = DefaultNumberOfExecutions,
                    TimeSpanSecond = DefaultTimeSpanSecond,
                    MaxBurst = DefaultMaxBurst
                };
                //KeyValueService.Save(new RMNameValueDto() { Name = KeyNameCollection.API_Rate_Limits, Value = SerializerHelper.SerializeByJsonConvert(defaultPolicy), Type = RMNameValueType.RateLimitsPolicy });
                return defaultPolicy;
            }
        }

        private RateLimitDto GetGlobalRateLimitFromDb()
        {
            var globalPolicy = GlobalKeyValueService.Get(GlobalRateLimitsKey);
            if (globalPolicy != null && !string.IsNullOrWhiteSpace(globalPolicy.Value))
            {
                var dto = SerializerHelper.DeserializeByJsonConvert<RateLimitDto>(globalPolicy.Value);
                if (dto.MaxBurst < 1)
                {
                    dto.MaxBurst = DefaultGlobalMaxBurst;
                }
                if (dto.NumberOfExecutions < 1)
                {
                    dto.NumberOfExecutions = DefaultGlobalNumberOfExecutions;
                }
                if (dto.TimeSpanSecond < 1)
                {
                    dto.TimeSpanSecond = DefaultGlobalTimeSpanSecond;
                }
                return dto;
            }
            else
            {
                RateLimitDto defaultPolicy = new RateLimitDto()
                {
                    NumberOfExecutions = DefaultGlobalNumberOfExecutions,
                    TimeSpanSecond = DefaultGlobalTimeSpanSecond,
                    MaxBurst = DefaultGlobalMaxBurst
                };
               // GlobalKeyValueService.Save(new RMGlobalNameValueDto() { Name = KeyNameCollection.API_Rate_Limits, Value = SerializerHelper.SerializeByJsonConvert(defaultPolicy), Type = RMGlobalNameValueType.GlobalRateLimitsPolicy });
                return defaultPolicy;
            }
        }
    }
}
