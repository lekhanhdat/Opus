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
namespace AvePoint.Wrapper.Common
{
    using System.Collections.Concurrent;
    using System.Threading;

    class APPOnlyAPSTokenCache
    {
        private class ItemCell
        {
            public string Value;
            public long Times;
        }

        private ConcurrentDictionary<string, ItemCell> cache;
        private int capacity;

        public APPOnlyAPSTokenCache(int capacity)
        {
            this.capacity = capacity;
            this.cache = new ConcurrentDictionary<string, ItemCell>();
        }

        public bool TryGet(string accountName, out string tenantId)
        {
            var domainName = GetDomainName(accountName);
            ItemCell item;

            if (cache.TryGetValue(domainName, out item))
            {
                tenantId = item.Value;
                Interlocked.Add(ref item.Times, 1);

                return true;
            }

            tenantId = null;
            return false;
        }

        public void Add(string accountName, string tenantId)
        {
            if (cache.Count >= capacity)
            {
                string key = null;
                ItemCell lessUsedItem = null;

                foreach (var item in cache)
                {
                    if (item.Value.Times == 0)
                    {
                        key = item.Key;
                        lessUsedItem = item.Value;
                        break;
                    }
                    else if (lessUsedItem == null || lessUsedItem.Times > item.Value.Times)
                    {
                        key = item.Key;
                        lessUsedItem = item.Value;
                    }
                }

                cache.TryRemove(key, out lessUsedItem);
            }

            cache.TryAdd(accountName, new ItemCell() { Value = tenantId });
        }

        private string GetDomainName(string accountName)
        {
            var index = accountName.IndexOf('@');

            if (index > 0)
            {
                return accountName.Substring(index + 1);
            }

            return accountName;
        }
    }
}
