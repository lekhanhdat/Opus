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
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage
{
    public class AdaptiveSpoStorage : IDisposable
    {
        protected static AveLogger _logger = AveLogger.GetInstance(typeof(AdaptiveSpoStorage));
        public AdaptiveSpoFolderStorage Folders { get; set; }

        public AdaptiveSpoItemStorage Items { get; set; }

        public AdaptiveSpoStorage(CacheDBOperator<SPOItem> itemCacheDBOperator, CacheDBOperator<SPOFolder> folderCacheDBOperator, SPOFolder currentFolder, bool forceUseDB = false)
        {
            Folders = new AdaptiveSpoFolderStorage(folderCacheDBOperator, currentFolder);
            Items = new AdaptiveSpoItemStorage(itemCacheDBOperator, currentFolder);
            if(forceUseDB)
            {
                _logger.Info($"Force to use DB storage for folder: {currentFolder?.FullPath}");
                ConvertFolderSystemToDBStorage();
            }
        }

        public void ConvertFolderSystemToDBStorage()
        {
            Folders.ConvertFolderSystemToDBStorage();
        }

        public void Dispose()
        {
            Folders.Dispose();
            Items.Dispose();
        }
    }
}
