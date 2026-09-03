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
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage
{
    public class BaseAdaptiveSpoStorage<T> : ISpoStorage<T> where T : SPOItem, new()
    {
        protected static AveLogger _logger = AveLogger.GetInstance(typeof(BaseAdaptiveSpoStorage<T>));

        protected ISpoStorage<T> _currentStorage;

        protected SPOFolder _currentFolder;

        public int Count => _currentStorage.Count();
        public bool IsReadOnly => false;

        public CacheDBOperator<T> CacheDBOperator { get; set; }

        public BaseAdaptiveSpoStorage(SPOFolder currentFolder)
        {
            _currentFolder = currentFolder;
        }

        public virtual bool IsDBMode()
        {
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _currentStorage.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _currentStorage.GetEnumerator();
        }

        public virtual T GetByName(string name)
        {
            return _currentStorage.GetByName(name);
        }

        public virtual void Dispose()
        {
            _currentStorage.Dispose();
        }

        public virtual void Add(T item)
        {
            _currentStorage.Add(item);
        }

        public virtual void Clear()
        {
            _currentStorage.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _currentStorage.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _currentStorage.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            return _currentStorage.Remove(item);
        }

        protected virtual void ConvertCurrentToDBStorage()
        {

        }

        public void ConvertFolderSystemToDBStorage()
        {
            SPOFolder startPoint = _currentFolder;
            if (!_currentFolder.IsRoot && _currentFolder.ParentFolder != null)
            {
                startPoint = _currentFolder.ParentFolder;
            }
            _logger.Info($"start convert foder and item to db cache,max limit:{MemoryDataCount.MemoryLimitCount}, current count:{MemoryDataCount.DataCount},current folder:{startPoint?.FullPath}, is root:{startPoint?.IsRoot}");
            ConvertSubFolderSystemToDBStorage(startPoint);
        }

        private void ConvertSubFolderSystemToDBStorage(SPOFolder startPoint)
        {
            if(startPoint == null || startPoint.AdaptiveSpoStorage.Folders.IsDBMode())
            {
                return;
            }
            IEnumerable<SPOFolder> subFolders = startPoint.SubFolders.ToList();
            foreach (SPOFolder subFolder in subFolders)
            {
                ConvertSubFolderSystemToDBStorage(subFolder);
                subFolder.SubFolders.ConvertCurrentToDBStorage();
                subFolder.Items.ConvertCurrentToDBStorage();
            }
            startPoint.SubFolders.ConvertCurrentToDBStorage();
            startPoint.Items.ConvertCurrentToDBStorage();
        }
    }
}
