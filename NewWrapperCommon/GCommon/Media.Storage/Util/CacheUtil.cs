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
using System.Collections;
using System.Threading;
using AvePoint.Media.Storage.Inner;

namespace AvePoint.Media.Storage
{
    

    public delegate SpaceInfo CheckFreeSpace();

    public class CacheUtil
    {
        static readonly object lockObj = new object();

        //static int CheckSpaceInterval = 1000 * 10;

        static SafeDictionary<string, SafeDictionary<string, SpaceInfo>> allSpaceInfos = new SafeDictionary<string, SafeDictionary<string, SpaceInfo>>();
        
        static SafeDictionary<string, SpaceInfo> GetSpaceInfos(string vimName)
        {
            SafeDictionary<string, SpaceInfo> spaceInfos = null;
            if (allSpaceInfos.ContainsKey(vimName))
            {
                spaceInfos = allSpaceInfos[vimName];
            }
            else
            {
                spaceInfos = new SafeDictionary<string, SpaceInfo>();
                allSpaceInfos[vimName] = spaceInfos;
            }
            return spaceInfos;
        }


        static bool IsTimeOut(SpaceInfo spaceInfo, int checkFreeSpaceIntervelTime)
        {
            if (checkFreeSpaceIntervelTime > ((DateTime.Now.Ticks - spaceInfo.DataObtainTime) / 10000))
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }

        /// <summary>
        /// Gets the space info, 按时间规则+配置文件
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="host">The host.</param>
        /// <param name="checkFreeSpace">The check free space.</param>
        /// <param name="isCache">if set to <c>true</c> [is cache].</param>
        /// <returns></returns>
        public static SpaceInfo GetSpaceInfo(string vimName, string host, CheckFreeSpace checkFreeSpace)
        {
            SpaceInfo result = null;
            VIMInfo vimInfo = XFactory.cfg.GetVIMInfo(vimName);
            if (vimInfo.IsCheckFreeSpace)
            {
                if (vimInfo.IsCacheSpaceInfo)
                {
                    SafeDictionary<string, SpaceInfo> spaceInfos = GetSpaceInfos(vimName);
                    if (!spaceInfos.ContainsKey(host) || IsTimeOut(spaceInfos[host], vimInfo.CheckFreeSpaceIntervalTime))
                    {
                        spaceInfos[host] = checkFreeSpace();
                        spaceInfos[host].DataObtainTime = DateTime.Now.Ticks;
                    }
                    result = spaceInfos[host];
                }
                else
                {
                    result = checkFreeSpace();
                }
            }
            else
            {
                result = new SpaceInfo()
                {
                    TotalSpace = long.MaxValue - 1,
                    TotalFreeSpace = long.MaxValue - 1,
                    TotalUsedSpace = 0
                };
            }
            return result;
        }

        //public static SpaceInfo GetSpaceInfo(string vimName, string host, CheckFreeSpace checkFreeSpace, bool isCache)
        //{
        //    if (!isCache)
        //    {
        //        return checkFreeSpace();
        //    }
        //    SafeDictionary<string, SpaceInfo> spaceInfos = GetSpaceInfos(vimName);
        //    if (!spaceInfos.ContainsKey(host) || IsTimeOut(spaceInfos[host]))
        //    {
        //        spaceInfos[host] = checkFreeSpace();
        //        spaceInfos[host].DataObtainTime = DateTime.Now.Ticks;
        //    }
        //    return spaceInfos[host];
        //}

        #region 按线程规则
        //static SafeDictionary<int, SafeDictionary<string, SpaceInfo>> allSpaceInfos = new SafeDictionary<int, SafeDictionary<string, SpaceInfo>>();

        //static SafeDictionary<string, SpaceInfo> GetSpaceInfos(int type)
        //{
        //    SafeDictionary<string, SpaceInfo> spaceInfos = null;
        //    if (allSpaceInfos.ContainsKey(type))
        //    {
        //        spaceInfos = allSpaceInfos[type];
        //    }
        //    else
        //    {
        //        spaceInfos = new SafeDictionary<string, SpaceInfo>();
        //        allSpaceInfos[type] = spaceInfos;
        //    }
        //    return spaceInfos;
        //}
        /// <summary>
        /// Gets the space info， 按线程规则
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="host">The host.</param>
        /// <param name="checkFreeSpace">The check free space.</param>
        /// <returns></returns>
        /// 
        //public static SpaceInfo GetSpaceInfo(int type, string host, CheckFreeSpace checkFreeSpace)
        //{
        //    lock (lockObj)
        //    {
        //        SafeDictionary<string, SpaceInfo> spaceInfos = GetSpaceInfos(type);
        //        if (!spaceInfos.ContainsKey(host))
        //        {
        //            spaceInfos[host] = checkFreeSpace();
        //            Thread t = new Thread(new ThreadStart(delegate()
        //            {
        //                try
        //                {
        //                    while (true)
        //                    {
        //                        Thread.Sleep(CheckSpaceInterval);
        //                        spaceInfos[host] = checkFreeSpace();
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    spaceInfos.Remove(host);
        //                }
        //            }));
        //            t.Name = type.ToString() + "_" + host;
        //            t.IsBackground = true;
        //            t.Start();
        //        }
        //        return spaceInfos[host];

        //    }

        //}
        #endregion

    }

    public class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private static readonly object syncRoot = new object();
        private readonly Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public void Add(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                d.Add(key, value);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            //lock (syncRoot)
            //{
            return d.ContainsKey(key);
            //}
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Keys;
                }
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            lock (syncRoot)
            {
                return d.Remove(key);
            }
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return d.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Values;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value></value>
        public TValue this[TKey key]
        {
            get { return d[key]; }
            set
            {
                lock (syncRoot)
                {

                    d[key] = value;
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).Add(item);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            lock (syncRoot)
            {
                d.Clear();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)d).Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return d.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)d).Remove(item);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)d).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)d).GetEnumerator();
        }

        #endregion
    }

}
