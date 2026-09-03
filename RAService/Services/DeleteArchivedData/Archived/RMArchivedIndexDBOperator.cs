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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Settings;
using Media.Common.ClassicStorageApi;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.Archived
{
    public class RMArchivedIndexDBOperator
    {
        private static readonly string s_encryptionKey;

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedIndexDBOperator));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IMediaDataDao _mediaDao = PlatformWindsorManager.GetService<IMediaDataDao>();

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly IndexDatabaseHelper _indexDBHelper;

        private IXSystem _remoteStorageDevice;

        private IXSystem _localStorageDevice;

        private StorageInfo _remoteStorageInfo;

        private StorageInfo _localStorageInfo;

        static RMArchivedIndexDBOperator()
        {
            s_encryptionKey = new SettingProfileService().GetDBSEEMasterKey().Replace("\"", "#").Replace("\\", "*");
        }

        public RMArchivedIndexDBOperator(RestoredSitesInfo restoredSiteInfo)
        {
            _restoredSiteInfo = restoredSiteInfo;
            _indexDBHelper = new();
            Open();
        }

        public void Reload()
        {
            _indexDBHelper.Close();
            _remoteStorageDevice.Close();
            _localStorageDevice.Dispose();
            _localStorageDevice.DeleteFile(_localStorageInfo);
            Open();
        }

        public void Commit()
        {
            _indexDBHelper.Close();
            var commitResult = _remoteStorageDevice.CommitStream(_localStorageDevice.OpenStream(_localStorageInfo, FileMode.Open), _remoteStorageInfo);
            _logger.Info($"The site [{_restoredSiteInfo.SiteUrl}] index db has been commit [{commitResult.IsCommited}].");

            var siteStoragePath = SecurityUtils.SafeCombinePath(_localStorageInfo.HighPlusLowName.Replace("/", "\\").Split("\\").ToArray());
            var key = ServiceConstants.ModifyTimeHeader + siteStoragePath.TrimStart(['/', '\\']).TrimEnd(['/', '\\']);

            _mediaDao.UpdateOrInsertMediaDataAsync(key, DateTime.UtcNow.Ticks.ToString());

            _logger.Info($"The site [{_restoredSiteInfo.SiteUrl}] index db cache has been refershed.");

            _remoteStorageDevice.Close();
            _localStorageDevice.Dispose();
            _localStorageDevice.DeleteFile(_localStorageInfo);
        }

        public bool TryGetItemById(string id, out ArchiverBasicIndex item)
        {
            item = null;
            var sql = "SELECT * FROM TB_BODY_INDEX WHERE COL_ID = @Id";
            var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new Dictionary<string, object> { { "@Id", id } });
            if (items.Count == 0)
            {
                return false;
            }

            item = items[0];
            return true;
        }

        public List<ArchiverBasicIndex> GetRelateItems(ArchiverBasicIndex item)
        {
            var sql = "SELECT * FROM TB_BODY_INDEX WHERE COL_ITEMID = @ItemId AND COL_JOBID = @JobId AND COL_ID != @Id";
            var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new Dictionary<string, object> { { "@ItemId", item.NodeGuid }, { "@JobId", item.JobId }, { "@Id", item.Id } });
            return items;
        }

        public bool DeleteItem(ArchiverBasicIndex item)
        {
            try
            {
                var sql = "DELETE FROM TB_BODY_INDEX WHERE COL_ID = @Id";
                _indexDBHelper.ExecuteNonQuery(sql, new Dictionary<string, object> { { "@Id", item.Id } });
                _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] has been delete in archiver master index.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete site [{item.SitePath}] item [{item.PathMD5}] in archiver master index. Error: {e}");
                return false;
            }
        }

        public ArchiverBasicIndex GetContainerItem(string pathMd5)
        {
            var sql = "SELECT * FROM TB_HEAD_INDEX WHERE COL_PATH_MD5 = @pathMd5 ORDER BY COL_ID LIMIT 1";
            return _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new Dictionary<string, object> { { "@pathMd5", pathMd5 } }).First();
        }

        public bool IsDuplicateFile(ArchiverBasicIndex item)
        {
            return item.DuplicateStatus > 0;
        }

        public bool IsLastDuplicatedFileWithSameCRC(ArchiverBasicIndex item, HashSet<string> deletingFileIDs)
        {
            var sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_8 = @CRC;";
            var dupFiles = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new Dictionary<string, object>() { { "@CRC", item.StorageCrc64 } });
            var refsCount = dupFiles.Count(f => !deletingFileIDs.Contains(f.Id) && f.DuplicateStatus > 0);
            return refsCount == 0;
        }

        public long GetFileCount()
        {
            var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
            return Convert.ToInt64(_indexDBHelper.ExecuteScalar(sql, null));
        }

        public long GetFileVersionCount()
        {
            var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
            return Convert.ToInt64(_indexDBHelper.ExecuteScalar(sql, null));
        }

        private void Open()
        {
            var volumeGenerator = new ArchiverVolumeGenerator();
            var siteIndexVolume = volumeGenerator.GenerateIndexVolume(new()
            {
                FarmName = "",
                SiteCollectionUrl = _restoredSiteInfo.SiteUrl
            });

            OpenRemoteStorageDevice(siteIndexVolume);
            OpenLocalStorageDevice(siteIndexVolume);

            var buffer = new byte[1024];
            using (var remoteStream = _remoteStorageDevice.OpenStream(_remoteStorageInfo, FileMode.Open))
            {
                using var localStream = _localStorageDevice.OpenStream(_localStorageInfo, FileMode.CreateNew);
                var readLen = 0;
                while ((readLen = remoteStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    localStream.Write(buffer, 0, readLen);
                }
            }

            _indexDBHelper.Open(SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, _localStorageInfo.HighPlusLowName), s_encryptionKey);
            _logger.Info($"The index db of site [{_restoredSiteInfo.SiteUrl}] has been download and open.");
        }

        private void OpenRemoteStorageDevice(string siteIndexVolume)
        {
            var storageDeviceDto = _storageDeviceService.GetIndexDevice();
            var logicDeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives =
                [
                    new()
                    {
                        Id = storageDeviceDto.Id,
                        ConnectionString = storageDeviceDto.ConnectionString,
                        ModifyTime = storageDeviceDto.ModifyTime,
                        Type = storageDeviceDto.Type,
                    }
                ]
            };
            _remoteStorageDevice = XFactoryCommon.InstanceSystem(logicDeviceDto.ToXRIS().First());
            _remoteStorageDevice.Open();
            _logger.Info($"The remote storage device of site [{siteIndexVolume}] has been open.");

            _remoteStorageInfo = XConvert.FromNames(siteIndexVolume, "index.db", "");
        }

        private void OpenLocalStorageDevice(string siteIndexVolume)
        {
            var localdeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives =
                [
                    PhysicalDeviceDto.GenterateFS(Environment.CurrentDirectory, string.Empty, string.Empty)
                ]
            };
            _localStorageDevice = XFactoryCommon.InstanceSystem(localdeviceDto.ToXRIS().First());
            _localStorageDevice.Open();
            _logger.Info($"The local storage device of site [{siteIndexVolume}] has been open.");
            _localStorageInfo = new StorageInfo(siteIndexVolume, "index.db");
        }
    }
}
