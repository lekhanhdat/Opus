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
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage
{
    public class AdaptiveSpoFolderStorage : BaseAdaptiveSpoStorage<SPOFolder>
    {

        public AdaptiveSpoFolderStorage(CacheDBOperator<SPOFolder> cacheDBOperator, SPOFolder currentFolder) : base(currentFolder)
        {
            bool ParentFolderIsDBMode() => currentFolder.ParentFolder.SubFolders.IsDBMode();
            CacheDBOperator = cacheDBOperator;
            if (MemoryDataCount.DataCount > MemoryDataCount.MemoryLimitCount ||
                (!currentFolder.IsRoot && ParentFolderIsDBMode()))
            {
                _currentStorage = new DBSPOFolderStorage(cacheDBOperator, _currentFolder);
            }
            else
            {
                _currentStorage = new MemorySPOFolderStorage();
            }
        }

        public void InternalUpdateCurrentFolderId(int newId)
        {
            if(newId == _currentFolder.Id)
            {
                return;
            }
            bool CurrentFolderStorageInDB() => _currentFolder.ParentFolder.SubFolders.IsDBMode();
            if (!_currentFolder.IsRoot && CurrentFolderStorageInDB())
            {
                ((DBSPOFolderStorage)_currentStorage).UpdateCurrentFolderId(newId);
            }
        }

        public override bool IsDBMode()
        {
            return _currentStorage is DBSPOFolderStorage;
        }

        public override void Add(SPOFolder item)
        {
            if(!string.Equals(item.ParentFolder?.FullPath, _currentFolder.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"folder was add to error parent folder,item.ParentFolderPath:{item.ParentFolderPath}," +
                    $"item.ParentFolder.FullPath:{item.ParentFolder?.FullPath}, _currentFolder.FullPath:{_currentFolder.FullPath} ");
            }
            if (!IsDBMode() && MemoryDataCount.DataCount > MemoryDataCount.MemoryLimitCount)
            {
                _currentFolder.AdaptiveSpoStorage.ConvertFolderSystemToDBStorage();
            }
            base.Add(item);
        }

        protected override void ConvertCurrentToDBStorage()
        {
            if (!IsDBMode())
            {
                var dbStorage = new DBSPOFolderStorage(CacheDBOperator, _currentFolder);
                int page = 0;
                int size = 500;
                while (_currentStorage.Count > page * size)
                {
                    var items = _currentStorage.Skip(page++ * size).Take(size);
                    dbStorage.AddRange(items.ToArray());
                }
                _currentStorage.Dispose();
                _currentStorage = dbStorage;
            }
        }
    }
}
