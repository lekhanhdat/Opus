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

namespace AvePoint.RA.Cache.Services
{
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using AvePoint.RA.RedisCache;
    using AvePoint.RA.RedisCache.Configurations;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AvePoint.RA.Common.Security;
    using Cloud.Sdk.Data;
    using AvePoint.RA.Contract.Common;
    using AvePoint.RA.Contract.Tenant;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 需要在程序启动时调用Init方法初始化
    /// </summary>
    public class RedisCacheService
    {
        private readonly static object locker = new object();
        private RedisCacheService() { }
        private static IRedisCacheProvider redisCacheProvider;
        public static IRedisCacheProvider GetRedisProvider(string name, Action<RedisOptions> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddRedisCache(name, additionalSetup);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IRedisCacheProvider>();
        }

        public static IRedisCacheProvider CacheProvider
        {

            get
            {
                if (redisCacheProvider == null)
                {
                    lock (locker)
                    {
                        if (redisCacheProvider == null)
                        {
                            var redisConnection = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.RECO_REDIS_CONNECTION_STRING];
                            var isGCPEnv = RMGlobalConfiguration.EnvSetting.IsGCPEnvironment;
                            var isDevEnv = RMGlobalConfiguration.EnvSetting.IsDevEnvironment;
                            
                            redisCacheProvider = CacheService.GetRedisProvider(
                                "Records", x =>
                                {
                                    if (isGCPEnv)
                                    {
                                        x.DBConfig.IgnoreCertificateValidation = true;
                                    }
                                    x.DBConfig.Connection = redisConnection;
                                    x.DBConfig.IsDevelopmentEnvironment = isDevEnv;
                                }, 
                                new RedisLoggerFactory());
                        }
                    }
                }
                return redisCacheProvider;
            }
        }


        private class RedisLoggerFactory : ILoggerFactory
        {
            public ILogger CreateLogger(string typeName)
            {
                return new RedisLogger(typeName);
            }
            public void AddProvider(ILoggerProvider provider)
            {
            }

            public void Dispose()
            {
            }
        }
        private class RedisLogger : ILogger, IDisposable
        {
            private RALogger logger;

            public RedisLogger(string typeName)
            {
                Type type = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName, false, true);
                    if (type != null)
                    {
                        break;
                    }
                }

                if (type != null)
                {
                    this.logger = RALogger.GetInstance(type);
                }
                else
                {
                    this.logger = RALogger.GetInstance(typeof(RedisLogger));
                }
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull
            {
                return this;
            }

            public void Dispose()
            {
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var ex = exception == null ? "" : exception.ToString();
                var message = formatter != null ? $"{formatter(state, exception)}" : $"{state.ToString()}, {ex}";
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        break;
                    case LogLevel.Debug:
                        logger.Debug(message);
                        break;
                    case LogLevel.Information:
                        logger.Info(message);
                        break;
                    case LogLevel.Warning:
                        logger.Warn(message);
                        break;
                    case LogLevel.Error:
                        logger.Error(message);
                        break;
                    case LogLevel.Critical:
                        logger.Error(message);
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        break;
                }
            }
        }
    }

}
