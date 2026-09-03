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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static AvePoint.GCommon.Utility.I18N.EventIds.SharePoint;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base
{
    public class BaseDBSPOStorage<T> : ISpoStorage<T> where T : SPOItem, new()
    {
        protected static AveLogger _log = AveLogger.GetInstance(typeof(DBSPOFolderStorage));

        protected readonly CacheDBOperator<T> _cacheDbOperator;
        public bool IsReadOnly => false;
        public int Count => _cacheDbOperator.CountItems(_currentFolder.FullPath);

        protected SPOFolder _currentFolder;

        internal BaseDBSPOStorage(CacheDBOperator<T> cacheDbOperator, SPOFolder currentFolder)
        {
            _cacheDbOperator = cacheDbOperator ?? throw new ArgumentNullException(nameof(cacheDbOperator));
            _currentFolder = currentFolder;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                return false;
            }
            return _cacheDbOperator.ContainsItem(item.Name, _currentFolder.FullPath);
        }

        public void Clear()
        {
            _cacheDbOperator.Clear(_currentFolder.FullPath);
        }

        public void Add(T item)
        {
            if (Contains(item))
            {
                return;
            }
            AddRange(item);
        }

        public virtual T GetByName(string name)
        {
            return _cacheDbOperator.QueryItemByName(name, _currentFolder.FullPath);
        }

        protected virtual IEnumerator<T> GetEnumerator()
        {
            const int pageSize = 500;
            int offset = 0;

            while (true)
            {
                var items = _cacheDbOperator.QueryItems(offset, _currentFolder.FullPath, pageSize);
                if (items == null || items.Count == 0)
                {
                    yield break;
                }

                foreach (var item in items)
                {
                    yield return item;
                }

                if (items.Count < pageSize)
                {
                    yield break;
                }

                offset += pageSize;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _cacheDbOperator?.Dispose();
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new NullReferenceException("array is null");
            }

            if ((uint)index > array.Length)
            {
                throw new IndexOutOfRangeException($"index out of range, index:{index}, arr len:{array.Length}");
            }

            if (array.Length - index < this.Count)
            {
                throw new IndexOutOfRangeException("array size is samll, index:{index}, arr len:{array.Length}");
            }

            foreach (T item in this)
            {
                array[index++] = item;
            }
        }

        public bool Remove(T item)
        {
            _cacheDbOperator.RemoveItem(item.Name, _currentFolder.FullPath);

            return true;
        }

        public void AddRange(params T[] items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            const int batchSize = 500;
            var batch = new List<T>(batchSize);

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    InsertBatch(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                InsertBatch(batch);
            }
        }

        private void InsertBatch(List<T> batch)
        {
            _cacheDbOperator.InsertItems(batch, _currentFolder);
        }

    }
}
