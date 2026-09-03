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

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveThreadSafeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private object threadLocker;

        protected AveThreadSafeDictionary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            threadLocker = new object();
        }

        public AveThreadSafeDictionary() : base()
        {
            threadLocker = new object();
        }

        public AveThreadSafeDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            threadLocker = new object();
        }

        public new void Add(TKey key, TValue value)
        {
            lock (threadLocker)
            {
                base.Add(key, value);
            }
        }

        public new void Clear()
        {
            lock (threadLocker)
            {
                base.Clear();
            }
        }

        public new void Remove(TKey key)
        {
            lock (threadLocker)
            {
                base.Remove(key);
            }
        }

        public new TValue this[TKey key]
        {
            get
            {
                lock (threadLocker)
                {
                    return base[key];
                }
            }
            set
            {
                lock (threadLocker)
                {
                    base[key] = value;
                }
            }
        }

        public new bool ContainsKey(TKey key)
        {
            lock (threadLocker)
            {
                return base.ContainsKey(key);
            }
        }

        public new bool ContainsValue(TValue value)
        {
            lock (threadLocker)
            {
                return base.ContainsValue(value);
            }
        }

        public new bool TryGetValue(TKey key, out TValue value)
        {
            lock (threadLocker)
            {
                return base.TryGetValue(key, out value);
            }
        }
    }
}
