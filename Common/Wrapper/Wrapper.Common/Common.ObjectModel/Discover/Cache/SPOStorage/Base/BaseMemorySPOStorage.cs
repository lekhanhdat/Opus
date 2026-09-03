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
using AvePoint.GCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base
{
    public class BaseMemorySPOStorage<T> : ISpoStorage<T> where T : SPOItem
    {
        protected static AveLogger _log = AveLogger.GetInstance(typeof(BaseMemorySPOStorage<T>));
        
        private readonly Dictionary<string, T> _storage = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        public virtual int Count => _storage.Count;
        public virtual bool IsReadOnly => false;

        public virtual bool Contains(T item)
        {
            if(item == null)
            {
                return false;
            }
            return _storage.ContainsKey(item.Name);
        }

        public virtual T GetByName(string name)
        {
            if (_storage.ContainsKey(name))
            {
                return _storage[name];
            }
            else
            {
                return null;
            }
        }

        public virtual void Clear()
        {
            Interlocked.Add(ref MemoryDataCount.DataCount, -Count);
            _storage.Clear();
        }

        public virtual void Add(T item)
        {
            if (item == null || Contains(item))
            {
                return;
            }
            _storage[item.Name] = item;
            Interlocked.Add(ref MemoryDataCount.DataCount, 1);
            if (MemoryDataCount.DataCount % 1000 == 0)
            {
                _log.Info($"already cache,:{MemoryDataCount.DataCount}");
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _storage.Values.OrderBy(item => item.Id).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Dispose()
        {
            Interlocked.Add(ref MemoryDataCount.DataCount, -Count);
            _storage.Clear();
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _storage.Values.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            if(item == null)
            {
                return false;
            }
            bool res = _storage.Remove(item.Name);
            if (res)
            {
                Interlocked.Add(ref MemoryDataCount.DataCount, -1);
            }            
            return res;
        }

        ~BaseMemorySPOStorage()
        {
            try
            {
                Interlocked.Add(ref MemoryDataCount.DataCount, -Count);
            }
            catch (Exception e)
            {
                _log.Error($"Fail release sumItemCountOfMemory, ex:{e}");
            }
        }
    }
}
