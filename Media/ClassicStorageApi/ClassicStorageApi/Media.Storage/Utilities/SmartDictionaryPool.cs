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

namespace AvePoint.Media.ClassicStorage.Utilities
{
    public class SmartDictionaryPool<TKey, TValue> where TValue : IDisposable
    {
        private Dictionary<TKey, SmartItem<TValue>> resources = new Dictionary<TKey, SmartItem<TValue>>();
        private object locker = new object();
        private int maxSize = 100;
        public SmartDictionaryPool() { }

        public TValue this[TKey index]
        {
            get
            {
                lock (locker)
                {
                    resources[index].UpdateTime = DateTime.Now.Ticks;
                    return resources[index].Value;
                }
            }
            set
            {
                lock (locker)
                {
                    if (resources.Count >= maxSize)
                    {
                        var ids = (from entry in resources
                                   orderby entry.Value.UpdateTime ascending
                                   select entry.Key).Take(maxSize / 2);
                        foreach (TKey key in ids)
                        {
                            resources[key].Value.Dispose();
                            resources.Remove(key);
                        }
                    }
                    resources[index] = new SmartItem<TValue>() { Value = value , UpdateTime = System.DateTime.Now.Ticks};
                }
            }
        }
    }

    class SmartItem<TValue>
    {
        public TValue Value { get; set; }
        public long UpdateTime { get; set; }
    }
}
