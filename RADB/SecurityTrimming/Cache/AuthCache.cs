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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SecurityTrimming.Cache
{
    public class AuthCache
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AuthCache));
        private bool refreshCache = false;
        public IRMCache RMCache { get { return (IRMCache)PlatformWindsorManager.GetService(typeof(IRMCache)); } }
        public void RefreshCache(bool value)
        {
            refreshCache = value;
        }

        public async Task<string> GetAsync(string permissionKey, string key, Func<Task<string>> action)
        {
            try
            {
                if (!await RMCache.CheckRedisAvailable()) 
                {
                    return await action();
                }
                if (!refreshCache && await RedisCacheService.CacheProvider.HExistsAsync(permissionKey, key))
                {
                    return await RedisCacheService.CacheProvider.HGetAsync(permissionKey, key);
                }
                else
                {
                    string result = await action();
                    await RedisCacheService.CacheProvider.HSetAsync(permissionKey, key, result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"get auth failed, will return reuslt from source. error:{ex.ToString()}");
                return await action();
            }
           
             
        }
        public async Task<long> RemoveAsync(string permissionKey, List<string> fields = null)
        {
            if (!await RMCache.CheckRedisAvailable())
            {
                return 0;
            }
            return await RedisCacheService.CacheProvider.HDelAsync(permissionKey, fields);
        }
    }
}
