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
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AvePoint.RA.RedisCache.Configurations
{
    internal class RedisOptionsExtension : ICacheOptionsExtension
    {
        private readonly string _name;
        private readonly Action<RedisOptions> configure;
        public RedisOptionsExtension(string name, Action<RedisOptions> configure)
        {
            this._name = name;
            this.configure = configure;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<IRedisCacheProviderFactory, RedisCacheProviderFactory>();
            services.Configure(_name, configure);
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(d =>
            {
                var optionsMonitor = d.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMonitor.Get(_name);
                return new RedisDatabaseProvider(_name, options);
            });
            services.AddSingleton<IRedisCacheProvider, RedisCacheProvider>(r => 
            {
                var dbProvider = r.GetServices<IRedisDatabaseProvider>();
                var optionsMon = r.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(_name);
                
                var log = r.GetService<ILoggerFactory>();
                return new RedisCacheProvider(_name, dbProvider, options, log);
            });
        }
    }
}
