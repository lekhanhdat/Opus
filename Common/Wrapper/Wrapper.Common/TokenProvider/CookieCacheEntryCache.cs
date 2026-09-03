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
using System.Threading.Tasks;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public class CookieCacheEntryCache
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CookieCacheEntry> caches = new Dictionary<string, CookieCacheEntry>(StringComparer.OrdinalIgnoreCase);

        public int Capacity { get; set; }

        public CookieCacheEntryCache(int capacity)
        {
            Capacity = capacity;
        }

        public CookieCacheEntry Get(string key)
        {
            CookieCacheEntry entry = null;
            lock (caches)
            {
                caches.TryGetValue(key, out entry);
            }

            return entry;
        }

        public void AddOrUpdate(string key, CookieCacheEntry entry)
        {
            lock (caches)
            {
                if (caches.ContainsKey(key))
                {
                    caches[key] = entry;
                }
                else
                {
                    var capacity = Capacity;

                    if (caches.Count > capacity)
                    {
                        var items = caches.OrderBy(k => k.Value.Expires).Take(caches.Count - capacity);
                        foreach (var item in items)
                        {
                            log.Info("Clean the cache:{0} with expire:{1}", item.Key, item.Value.Expires);
                            caches.Remove(item.Key);
                        }
                    }
                    caches[key] = entry;
                }
            }
        }
    }
}
