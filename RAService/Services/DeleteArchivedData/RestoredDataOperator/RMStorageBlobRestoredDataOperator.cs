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
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.Media.Core.Index;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Tenant;
using System.IO;
using Media.Service.DomainModel.Index.ArchiverIndexes;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.RestoredDataOperator
{
    public class RMStorageBlobRestoredDataOperator : IRMRestoredDataOperator
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMStorageBlobRestoredDataOperator));

        private readonly BlobContainerClient _containerClient;

        private readonly BlobClient _blobClient;

        private readonly BlobItem _blobItem;

        private readonly IndexDatabaseHelper _indexDBHelper;

        private readonly RMDeleteArchivedDataSettingManager _settingManager;

        private string _indexDBPath;

        public string Sign => _blobItem.Name;

        public RMStorageBlobRestoredDataOperator(
            BlobContainerClient containerClient, 
            BlobItem blobItem,
            RMDeleteArchivedDataSettingManager settingManager)
        {
            _containerClient = containerClient;
            _blobClient = _containerClient.GetBlobClient(blobItem.Name);
            _blobItem = blobItem;
            _indexDBHelper = new();
            _settingManager = settingManager;
            Open();
        }

        private void Open()
        {
            var folderPath = Environment.CurrentDirectory;
            var blobPaths = new string[3] { TenantLocalValue.LogonGroupId, "delete_archived_data", _blobItem.Name.Split("/").Last() };
            for (var i = 0; i < blobPaths.Length - 1; i++)
            {
                var blobPath = blobPaths[i].Replace("#", "-").Replace(".", "-");
                folderPath = SecurityUtils.SafeCombinePath(folderPath, blobPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }

            _indexDBPath = SecurityUtils.SafeCombinePath(folderPath, blobPaths[blobPaths.Length - 1]);
            File.Create(_indexDBPath).Close();
            _blobClient.DownloadTo(_indexDBPath);
            _logger.Info($"The restored index db [{_blobItem.Name}] has been download to local [{_indexDBPath}].");
            _indexDBHelper.Open($"Data Source={_indexDBPath}");
            _logger.Info($"The restored index db [{_indexDBPath}] has been opend.");
        }

        public IEnumerable<RMRestoredItem> ReadItems()
        {
            var latestItemId = 0;
            while(true)
            {
                var sql = $"SELECT id, SiteId, JobId, StoragePath, COL_ID, ItemPathMd5, RestoreSetting, CleanRestoredOption, RestoredSiteUrl, RestoredUrl, RestoredTimeTicks FROM RestoredItems WHERE id > @id ORDER BY id LIMIT 100 OFFSET 0";
                var items = _indexDBHelper.ExecuteReader<ArchiverRestoredDataIndex>(sql, new Dictionary<string, object> { { "@id", latestItemId } });
                
                foreach(var item in items)
                {
                    if(_settingManager.HasTheDeletionTimeBeenReached(item.RestoredTimeTicks))
                    {
                        yield return RMRestoredItem.FromContract(item);
                    }
                }

                if (items.Count < 100)
                {
                    break;
                }
                latestItemId = items.Last().Id;
            }
        }

        public void DeleteItem(RMRestoredItem item)
        {
            try
            {
                var sql = "DELETE FROM RestoredItems WHERE id = @id";
                _indexDBHelper.ExecuteNonQuery(sql, new Dictionary<string, object> { { "@id", item.StorageBlobItemId } });
                _logger.Info($"The restored blob [{_blobItem.Name}] site [{item.SiteId}] item [{item.StorageBlobItemId}] has been deleted.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete restored blob [{_blobItem.Name}] site [{item.SiteId}] item [{item.StorageBlobItemId}]. Error: {e}");
            }
        }

        public bool HasRemaingItems()
        {
            var sql = $"SELECT COUNT(1) FROM RestoredItems";
            var obj = _indexDBHelper.ExecuteScalar(sql);
            var count = Convert.ToInt64(obj);
            return count > 0;
        }

        public void Close()
        {
            var sql = $"SELECT COUNT(1) FROM RestoredItems";
            var obj = _indexDBHelper.ExecuteScalar(sql);
            var count = Convert.ToInt64(obj);

            _logger.Info($"The restored index db [{_blobItem.Name}] remaining item count [{count}].");

            _indexDBHelper.Close();
            _logger.Info($"The local index db [{_indexDBPath}] has been closed.");

            if (count > 0)
            {
                using var fs = File.OpenRead(_indexDBPath);
                _blobClient.Upload(fs, true);
                _logger.Info($"Due to local index db [{_indexDBPath}] remaining items, upload and override to storage [{_blobItem.Name}].");
            }
            else
            {
                _blobClient.DeleteIfExists();
                _logger.Info($"No remaining items, delete restored index db [{_blobItem.Name}].");
            }

            File.Delete(_indexDBPath);
            _logger.Info($"The local index db [{_indexDBPath}] has been deleted.");
        }
    }
}
