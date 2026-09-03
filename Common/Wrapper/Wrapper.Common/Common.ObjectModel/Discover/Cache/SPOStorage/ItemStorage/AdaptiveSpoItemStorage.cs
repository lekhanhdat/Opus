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
    public class AdaptiveSpoItemStorage : BaseAdaptiveSpoStorage<SPOItem>
    {

        public AdaptiveSpoItemStorage(CacheDBOperator<SPOItem> cacheDBOperator, SPOFolder currentFolder, bool forceUseDB = false) : base(currentFolder)
        {
            CacheDBOperator = cacheDBOperator;
            if (MemoryDataCount.DataCount > MemoryDataCount.MemoryLimitCount ||
                (!currentFolder.IsRoot && currentFolder.ParentFolder.SubFolders.IsDBMode()))
            {
                _currentStorage = new DBSPOItemStorage(cacheDBOperator, _currentFolder);
            }
            else
            {
                _currentStorage = new MemorySPOItemStorage();
            }
        }

        public override bool IsDBMode()
        {
            return _currentStorage is DBSPOItemStorage;
        }

        public override void Add(SPOItem item)
        {
            if (!IsDBMode() && MemoryDataCount.DataCount > MemoryDataCount.MemoryLimitCount)
            {
                _currentFolder.AdaptiveSpoStorage.ConvertFolderSystemToDBStorage();
            }
            base.Add(item);
        }

        protected override void ConvertCurrentToDBStorage()
        {
            if (IsDBMode())
            {
                return;
            }
            DBSPOItemStorage dbStorage = new DBSPOItemStorage(CacheDBOperator, _currentFolder);
            int page = 0;
            int size = 500;
            while (_currentStorage.Count > page * size)
            {
                var items = _currentStorage.Skip(page++ * size).Take(size);
                dbStorage.AddRange(items.ToArray());
            }
            _currentStorage.Dispose();
            _currentStorage = dbStorage;
            return;
        }
    }
}
