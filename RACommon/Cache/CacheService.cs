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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Cache
{
    public static class CacheService
    {
        private static readonly CacheItemPolicy DefaultCachePolicy = new CacheItemPolicy();

        public static string Get(CacheNamespace module, string key, Func<string> getVal)
        {
            return Get(module.ToString(), key, getVal, DefaultCachePolicy);
        }

        public static string Remove(CacheNamespace module, string key)
        {
            var realKey = GetKey(module.ToString(), key);
            return Remove(realKey);
        }

        public static RemoteSiteCollection Get(CacheNamespace module, string key, Func<RemoteSiteCollection> getVal, TimeSpan timeoutValue)
        {
            return Get(module.ToString(), key, getVal, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.Add(timeoutValue)
            });
        }

        public static string Get(CacheNamespace module, string key, Func<string> getVal, TimeSpan timeoutValue)
        {
            return Get(module.ToString(), key, getVal, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.Add(timeoutValue)
            });
        }

        public static string Get(CacheNamespace module, string key)
        {
            key = GetRealKey(module.ToString(), key);
            var cache = GetCache();
            var result = cache.Get(key);
            return result != null ? Convert.ToString(result) : null;
        }

        public static void Set(CacheNamespace module, string key, string value, DateTime absoluteExpiration)
        {
            key = GetRealKey(module.ToString(), key);
            Set(key, value, new CacheItemPolicy()
            {
                AbsoluteExpiration = absoluteExpiration
            });
        }

        private static string GetRealKey(string prefix, string key)
        {
            return string.Format("{0}_{1}", prefix, key);
        }

        private static void Set(string key, string value, CacheItemPolicy cachePolicy)
        {
            var cache = GetCache();
            cache.Set(key, value, cachePolicy);
        }

        public static string Get(string prefix, string key, Func<string> getVal)
        {
            return Get(prefix, key, getVal, DefaultCachePolicy);
        }

        public static string Get(string prefix, string key, Func<string> getVal, TimeSpan timeoutValue)
        {
            return Get(prefix, key, getVal, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow.Add(timeoutValue)
            });
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module">不同功能不能使用一个cache，否则可能出现不同功能使用同一个Id，但是cache的内容不同的情况</param>
        /// <param name="key">不允许为null or empty</param>
        /// <param name="getObj">
        /// 当cache中不存在key对应的item并且getObj不为null时，使用getObj获取item并加入到cache中
        /// 但是如果getObj返回的是null，则并不会被加入到cache
        /// </param>
        /// <returns>
        /// 如果cache中存在则返回cache中的item
        /// 否则返回getObj的结果
        /// </returns>
        private static string Get(string prefix, string key, Func<string> getObj, CacheItemPolicy cachePolicy)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key can't be null or empty");
            }
            key = GetRealKey(prefix, key);
            var cache = GetCache();
            var result = cache.Get(key);
            if (result == null)
            {
                result = getObj();
                if (result != null)
                {
                    cache.Set(key, result, cachePolicy);
                }
            }
            if (result == null)
            {
                return null;
            }
            else
            {
                return Convert.ToString(result);
            }
        }

        private static MemoryCache GetCache()
        {
            return MemoryCache.Default;
        }
        private static RemoteSiteCollection Get(string prefix, string key, Func<RemoteSiteCollection> getObj, CacheItemPolicy cachePolicy)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key can't be null or empty");
            }
            key = string.Format("{0}_{1}", prefix, key);
            var cache = MemoryCache.Default;
            var result = cache.Get(key);
            if (result == null)
            {
                result = getObj();
                if (result != null)
                {
                    cache.Set(key, result, cachePolicy);
                }
            }
            if (result == null)
            {
                return null;
            }
            else
            {
                return result as RemoteSiteCollection;
            }
        }
        private static string Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key can't be null or empty");
            }
            var cache = MemoryCache.Default;
            return cache.Remove(key)?.ToString();
        }

        private static string GetKey(string prefix, string key)
        {
            return string.Format("{0}_{1}", prefix, key);
        }
    }

    /// <summary>
    /// 不同逻辑Cache对象时需要有自己的NameSpace，防止相同Id覆盖问题
    /// </summary>
    public enum CacheNamespace
    {
        O365Domain,
        DaoServiceUrl,
        AosEmailSetting,
        O365TenantIds,
        TenantStatus,
        DAOToken,
        DAOClient,
        #region
        //cache time 12H  to do next how to change...
        O365Site,
        RecordRuleInfos,
        #endregion
        O365AccessToken,
        AuthenticationProfiles,
        SalesforceToken,
        SalesforceOrganizations
    }
}
