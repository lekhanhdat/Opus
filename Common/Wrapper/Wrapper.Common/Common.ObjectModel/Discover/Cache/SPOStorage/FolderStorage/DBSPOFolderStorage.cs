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
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using Microsoft.Graph.Models;
using Microsoft.SharePoint.News.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage
{
    public class DBSPOFolderStorage : BaseDBSPOStorage<SPOFolder>
    {
        internal DBSPOFolderStorage(CacheDBOperator<SPOFolder> cacheDbOperator, SPOFolder currentFolder) : base(cacheDbOperator, currentFolder)
        {
        }

        public override SPOFolder GetByName(string name)
        {
            SPOFolder res = base.GetByName(name);
            if(res != null)
            {
                return SPOFolder.BuildUnRootFolder(_currentFolder, res.Name, res.Id);
            }
            else
            {
                return null;
            }
        }

        public void UpdateCurrentFolderId(int newId)
        {
            _cacheDbOperator.UpdateItemId(newId, _currentFolder.Name, _currentFolder.ParentFolderPath);
        }

        protected override IEnumerator<SPOFolder> GetEnumerator()
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
                    yield return SPOFolder.BuildUnRootFolder(_currentFolder, item.Name, item.Id);
                }

                if (items.Count < pageSize)
                {
                    yield break;
                }

                offset += pageSize;
            }
        }
    }
}
