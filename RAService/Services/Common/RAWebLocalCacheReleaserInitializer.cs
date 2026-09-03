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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using System;

namespace AvePoint.RA.Service.Services.Common
{
    public class RAWebLocalCacheReleaserInitializer
    {
        public static void Init(string cacheConfigKey)
        {
            try
            {
                var config = GetConfiguration(cacheConfigKey);
                long oneGB = 1024L * 1024L * 1024L;

                RAWebLocalCacheReleaser.Configure(
                    config.MaxCacheLimitInGB * oneGB,
                    TimeSpan.FromSeconds(config.CacheProtectedPeriodInSec),
                    TimeSpan.FromSeconds(config.ReleaseIntervalInSec));
            }
            catch (Exception ex)
            {
                AveLogger.GetInstance(typeof(RAWebLocalCacheReleaser)).Error($"Init web cache releaser failed, {ex}");
            }
        }

        private static RAWebLocalCacheReleaserConfig GetConfiguration(string cacheConfigKey)
        {
            RAWebLocalCacheReleaserConfig defaultConfig = new RAWebLocalCacheReleaserConfig();
            try
            {
                IRMGlobalKeyValueDao keyValueDao = PlatformWindsorManager.GetService<IRMGlobalKeyValueDao>();
                var item = keyValueDao.GetValueByKey(cacheConfigKey);
                if (!string.IsNullOrEmpty(item?.Value))
                {
                    var config = SerializerHelper.DeserializeByJsonConvert<RAWebLocalCacheReleaserConfig>(item.Value);
                    if (config.MaxCacheLimitInGB <= 0)
                    {
                        config.MaxCacheLimitInGB = defaultConfig.MaxCacheLimitInGB;
                    }
                    if (config.CacheProtectedPeriodInSec <= 0)
                    {
                        config.CacheProtectedPeriodInSec = defaultConfig.CacheProtectedPeriodInSec;
                    }
                    if (config.ReleaseIntervalInSec <= 0)
                    {
                        config.ReleaseIntervalInSec = defaultConfig.ReleaseIntervalInSec;
                    }
                    return config;
                }
            }
            catch (Exception ex)
            {
                AveLogger.GetInstance(typeof(RAWebLocalCacheReleaser)).Error($"Failed to get web cache releaser config, {ex}");
            }
            return defaultConfig;
        }


        private class RAWebLocalCacheReleaserConfig
        {
            /// <summary>
            /// 最大的 cache 总 size，单位 Byte，cache file总 size 超过后需要开始 release cache
            /// Default 是 50GB
            /// </summary>
            public long MaxCacheLimitInGB { get; set; } = 50;
            /// <summary>
            /// 需要传入正值，只有保护期时间以内，未被访问的Cache File，才允许被Release; 比如1小时没被访问过的，才允许删除
            /// Default 是 1小时
            /// </summary>
            public double CacheProtectedPeriodInSec { get; set; } = 60 * 60;
            /// <summary>
            /// 内部 timer执行 release cache 操作的周期
            /// Default 是 50GB 上限，每30分钟 release 一次
            /// </summary>
            public double ReleaseIntervalInSec { get; set; } = 60 * 30;
        }
    }
}
