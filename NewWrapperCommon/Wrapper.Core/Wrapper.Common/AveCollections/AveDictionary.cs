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
namespace System.Collections.Generic
{
    using System.Linq;
    using System.Runtime.Serialization;
    public static class AveDictionaryFactory
    {
        public static IAveDictionary<TKey, TValue> CreateDefaultInstance<TKey, TValue>(IEqualityComparer<TKey> equalityComparer = null)
        {
            return new AveDictionary<TKey, TValue>(equalityComparer);
        }
    }
    [Serializable]
    public class AveDictionary<TKey, TValue> : ThreadLocker, IAveDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable
    {
        private Dictionary<TKey, TValue> InnerDictionary { get; set; }

        public ICollection<TKey> Keys
        {
            get
            {
                return GetKeys();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return GetValues();
            }
        }

        public int Count
        {
            get
            {
                return AcquireReadLock(() =>
                {
                    return InnerDictionary.Count;
                });
            }
        }

        public bool IsReadOnly { get { return false; } }

        ICollection IDictionary.Keys
        {
            get
            {
                return GetKeys();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return GetValues();
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return AcquireReadLock(() =>
                {
                    return (InnerDictionary as IDictionary).IsFixedSize;
                });
            }
        }

        public object SyncRoot
        {
            get
            {
                return Locker;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get { return GetKeys(); }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get { return GetValues(); }
        }

        public object this[object key]
        {
            get
            {
                if (key is TKey)
                {
                    return GetValue((TKey)key);
                }
                return null;
            }
            set
            {
                SetValue((TKey)key, (TValue)value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                SetValue(key, value);
            }
        }

        protected AveDictionary(SerializationInfo info, StreamingContext context)
        {
            IEqualityComparer<TKey> comparer = info.GetValue("Comparer", typeof(IEqualityComparer<TKey>)) as IEqualityComparer<TKey>;
            var data = info.GetValue("KeyValuePairs", typeof(KeyValuePair<TKey, TValue>[])) as KeyValuePair<TKey, TValue>[];
            InnerDictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (var item in data)
            {
                InnerDictionary.Add(item.Key, item.Value);
            }
        }

        public AveDictionary()
        {
            InnerDictionary = new Dictionary<TKey, TValue>();
        }

        public AveDictionary(IEqualityComparer<TKey> comparer)
        {
            InnerDictionary = new Dictionary<TKey, TValue>(comparer);
        }

        private Dictionary<TKey, TValue>.KeyCollection GetKeys()
        {
            return AcquireReadLock(() =>
            {
                return InnerDictionary.Keys;
            });
        }

        #region private methods
        private Dictionary<TKey, TValue>.ValueCollection GetValues()
        {
            return LockExecution(() =>
            {
                return InnerDictionary.Values;
            });
        }


        private void SetValue(TKey key, TValue value)
        {
            LockExecution(() =>
            {
                InnerDictionary[key] = value;
            });
        }

        private TValue GetValue(TKey key)
        {
            return LockExecution(() =>
            {
                return InnerDictionary[key];
            });
        }

        #endregion

        public bool ContainsKey(TKey key)
        {
            return AcquireReadLock(() =>
            {
                return InnerDictionary.ContainsKey(key);
            });
        }

        public void Add(TKey key, TValue value)
        {
            AcquireWriteLock(() =>
            {
                InnerDictionary.Add(key, value);
            });
        }

        public bool Remove(TKey key)
        {
            return AcquireWriteLock(() =>
            {
                return InnerDictionary.Remove(key);
            });
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            TValue tempValue = default(TValue);
            var result = AcquireReadLock(() =>
            {
                return (InnerDictionary.TryGetValue(key, out tempValue));
            });
            value = tempValue;
            return result;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            AcquireWriteLock(() =>
            {
                InnerDictionary.Clear();
            });
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return AcquireReadLock(() =>
            {
                return InnerDictionary.Contains(item);
            });
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            AcquireReadLock(() =>
            {
                (InnerDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
            });
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return AcquireWriteLock(() =>
            {
                TValue value;
                if (InnerDictionary.TryGetValue(item.Key, out value)
                && EqualityComparer<TValue>.Default.Equals(value, item.Value))
                {
                    return InnerDictionary.Remove(item.Key);
                }
                return false;
            });
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return AcquireReadLock(() =>
            {
                return InnerDictionary.GetEnumerator();
            });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return AcquireReadLock(() =>
            {
                return InnerDictionary.GetEnumerator();
            });
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Comparer", InnerDictionary.Comparer);
            KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
            this.CopyTo(array, 0);
            info.AddValue("KeyValuePairs", array, typeof(KeyValuePair<TKey, TValue>[]));
        }

        public bool Contains(object key)
        {
            return AcquireReadLock(() =>
            {
                return ((IDictionary)InnerDictionary).Contains(key);
            });
        }

        public void Add(object key, object value)
        {
            AcquireWriteLock(() =>
            {
                ((IDictionary)InnerDictionary).Add(key, value);
            });
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return AcquireReadLock(() =>
            {
                return ((IDictionary)InnerDictionary).GetEnumerator();
            });
        }

        public void Remove(object key)
        {
            AcquireWriteLock(() =>
            {
                ((IDictionary)InnerDictionary).Remove(key);
            });
        }

        public void CopyTo(Array array, int index)
        {
            AcquireReadLock(() =>
            {
                ((IDictionary)InnerDictionary).CopyTo(array, index);
            });
        }

        public IAveDictionary<TKey, TValue> Clone()
        {
            return AcquireReadLock(() =>
            {
                var clone = new AveDictionary<TKey, TValue>(InnerDictionary.Comparer);
                foreach (var key in InnerDictionary.Keys)
                {
                    clone[key] = InnerDictionary[key];
                }
                return clone;
            });
        }
    }
}
