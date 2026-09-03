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
using Microsoft.Extensions.DependencyInjection;
using AvePoint.RA.RedisCache.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AvePoint.RA.RedisCache
{
    public class CacheService
    {
        public static IRedisCacheProvider GetRedisProvider(string name, Action<RedisOptions> additionalSetup, ILoggerFactory loggerFactory = null)
        {
            IServiceCollection services = new ServiceCollection();

            if(loggerFactory != null)
            {
                services.AddSingleton(loggerFactory);
            }

            services.AddRedisCache(name, additionalSetup);
            
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IRedisCacheProvider>();
        }
    }
}
