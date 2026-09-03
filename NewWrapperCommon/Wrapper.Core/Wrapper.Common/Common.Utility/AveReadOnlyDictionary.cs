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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    internal sealed class AveReadOnlyDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {                
        private IDictionary<K, V> dictionary;
        private bool isFixedSize;
        
        internal AveReadOnlyDictionary(IDictionary<K, V> dictionary)
            : this(dictionary, true)
        {
        }

        internal AveReadOnlyDictionary(IDictionary<K, V> dictionary, bool makeCopy)
        {
            if (makeCopy)
            {
                this.dictionary = new Dictionary<K, V>(dictionary);
            }
            else
            {
                this.dictionary = dictionary;
            }
            this.isFixedSize = makeCopy;
        }

        public void Add(K key, V value)
        {
            throw new InvalidOperationException("Dictionary is readonly");
        }

        public void Clear()
        {
            throw new InvalidOperationException("Dictionary is readonly");
        }

        public bool ContainsKey(K key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public bool Remove(K key)
        {
            throw new InvalidOperationException("Dictionary is readonly");
        }

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> keyValuePair)
        {
            throw new InvalidOperationException("Dictionary is readonly");
        }

        bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> keyValuePair)
        {
            if (this.ContainsKey(keyValuePair.Key))
            {
                V local = this[keyValuePair.Key];
                return local.Equals(keyValuePair.Value);
            }
            return false;
        }

        void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> keyValuePair)
        {
            throw new InvalidOperationException("Dictionary is readonly");
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<K, V>>)this).GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        // Properties
        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this.isFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public V this[K key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                throw new InvalidOperationException("Dictionary is readonly");
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return this.dictionary.Keys;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return this.dictionary.Values;
            }
        }
    }
}
