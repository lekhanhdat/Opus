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
namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    #endregion

    /// <summary>
    /// Represents a simple thread-safe collection of key/value pairs
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    internal class ConcurrentDic<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        private ReaderWriterLock rwLock = new ReaderWriterLock();
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(10);

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public void Add(TKey key, TValue value)
        {
            rwLock.AcquireWriterLock(timeout);
            try
            {
                this.dictionary.Add(key, value);
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }
            //set
            //{
            //    this.dictionary[key] = value;
            //}
        }

        public bool Remove(TKey key)
        {
            rwLock.AcquireWriterLock(timeout);
            try
            {
                return this.dictionary.Remove(key);
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

        public TKey FindKey(Func<TKey, bool> predicate)
        {
            rwLock.AcquireReaderLock(timeout);
            try
            {
                return this.dictionary.Keys.FirstOrDefault(predicate);
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        public List<TKey> KeyList()
        {
            rwLock.AcquireReaderLock(timeout);
            try
            {
                return this.dictionary.Keys.ToList();
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        public List<KeyValuePair<TKey, TValue>> TakeOut(Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            rwLock.AcquireWriterLock(timeout);
            try
            {
                var pairs = this.dictionary.Where(predicate).ToList();
                foreach (var kv in pairs)
                {
                    this.dictionary.Remove(kv.Key);
                }
                return pairs;
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

    }
}
