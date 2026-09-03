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
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    public static class DictionaryExtension
    {
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            return TryGetValue(self, key, default(TValue));
        }
        public static void AddRange(this IDictionary<string, object> current, IDictionary<string, object> props)
        {
            foreach (var kv in props)
            {
                current[kv.Key] = kv.Value;
            }
        }
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue)
        {
            TValue value;
            if (self.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue;
        }

        public static TValue TryGetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key) where TValue : new()
        {
            if (!self.ContainsKey(key))
            {
                self.Add(key, new TValue());
            }
            return self[key];
        }
    }
}
