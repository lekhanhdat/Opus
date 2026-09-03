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




namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class ThreadSafeMappingExtension
    {
        public static void AddWithLock<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic != null)
            {
                lock (dic)
                {
                    dic[key] = value;
                }
            }
        }

        public static TValue GetValueWithLock<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            if (dic != null)
            {
                if (dic.ContainsKey(key))
                {
                    lock (dic)
                    {
                        if (dic.ContainsKey(key))
                        {
                            return dic[key];
                        }
                    }
                }
            }
            return default(TValue);
        }
        public static void ReomveValueWithLock<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            if (dic != null)
            {
                if (dic.ContainsKey(key))
                {
                    lock (dic)
                    {
                        if (dic.ContainsKey(key))
                        {
                            dic.Remove(key);
                        }
                    }
                }
            }
        }


        public static void ForeachElementWithLock<TKey, TValue>(this Dictionary<TKey, TValue> dic, Action<TKey, TValue> action)
        {
            if (dic != null)
            {
                lock (dic)
                {
                    foreach (var key in dic.Keys)
                    {
                        action(key, dic[key]);
                    }
                }
            }
        }

    }
}
