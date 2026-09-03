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
using System.Linq;
using System.Text;

namespace AvePoint.RA.RedisCache
{
    public class RedisCacheProviderFactory : IRedisCacheProviderFactory
    {
        private readonly IEnumerable<IRedisCacheProvider> _redisProviders;
        public RedisCacheProviderFactory(
            IEnumerable<IRedisCacheProvider> redisProviders)
        {
            this._redisProviders = redisProviders;
        }

        public IRedisCacheProvider GetRedisProvider(string name)
        {
            ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            var provider = _redisProviders.FirstOrDefault(x => x.RedisName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (provider == null) throw new ArgumentException("can not find a match redis provider!");

            return provider;
        }
    }
}
