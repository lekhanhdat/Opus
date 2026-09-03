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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Core.Index;
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMNeedDeleteArchivedDataTemporaryStorageManager
    {
        private IndexDatabaseHelper _indexDBHelper;

        public RMNeedDeleteArchivedDataTemporaryStorageManager()
        {
            Open();
        }

        public void Open()
        {
            var dbPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "need_delete_items.db");
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            SQLiteConnection.CreateFile(dbPath);
            _indexDBHelper = new IndexDatabaseHelper();
            _indexDBHelper.Open($"Data Source={dbPath}");
            _indexDBHelper.ExecuteNonQuery("create table NeedDeleteItems(Id INTEGER PRIMARY KEY AUTOINCREMENT, ItemId nvarchar(500), RelatedDelete INTEGER, RestoredUrl nvarchar(500))", []);
        }

        public void Add(string itemId, bool isRelatedDelete, string restoredUrl)
        {
            _indexDBHelper.ExecuteNonQuery("INSERT INTO NeedDeleteItems (ItemId, RelatedDelete, RestoredUrl) VALUES (@ItemId, @RelatedDelete, @RestoredUrl)", new Dictionary<string, object>
            {
                {"@ItemId", itemId },
                {"@RelatedDelete", isRelatedDelete ? 1 : 0 },
                {"@RestoredUrl", restoredUrl },
            });
        }

        public IEnumerable<RMNeedDeleteItem> GetItems()
        {
            var latestItemId = 0;
            while (true)
            {
                var sql = "SELECT * FROM NeedDeleteItems WHERE Id > @Id ORDER BY Id LIMIT 100 OFFSET 0";
                var items = _indexDBHelper.ExecuteReader<RMNeedDeleteItem>(sql, new Dictionary<string, object> { { "@Id", latestItemId } });

                foreach (var item in items)
                {
                    yield return item;
                }

                if (items.Count < 100)
                {
                    break;
                }

                latestItemId = items.Last().Id;
            }
        }

        public void Close()
        {
            _indexDBHelper.Close();
        }
    }
}
