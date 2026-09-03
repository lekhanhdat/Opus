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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.Archived
{
    public class RMArchivedJobManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedJobManager));

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private readonly IArchiverIndexSubInfoDao _archiveIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly Dictionary<string, RMBackupedJobInfo> _jobInfoDic = [];

        private readonly Dictionary<string, bool> _storageIsSystemDic = [];

        private readonly HashSet<string> _notFoundJobIds = [];

        public async Task<bool> HasJobInfoAsync(string subJobId)
        {
            var masterJobId = GetMasterJobId(subJobId);
            if (_notFoundJobIds.Contains(subJobId) || _notFoundJobIds.Contains(masterJobId))
            {
                return false;
            }

            if (!_jobInfoDic.TryGetValue(masterJobId, out var jobInfo))
            {
                var (has, masterJobInfo) = await _archiverSiteMasterIndexDao.TryGetSiteMasterIndexAsync(masterJobId);
                if (!has)
                {
                    _logger.Info($"The master job [{masterJobId}] not found in ArchiverSiteMasterIndex table.");
                    _notFoundJobIds.Add(masterJobId);
                    return false;
                }

                jobInfo = new RMBackupedJobInfo
                {
                    SiteMasterIndex = masterJobInfo
                };
                _jobInfoDic[masterJobId] = jobInfo;
            }

            if (!jobInfo.SubInfoDic.ContainsKey(subJobId))
            {
                var (has, subJobInfo) = await _archiveIndexSubInfoDao.TryGetByJobIdAsync(subJobId);
                if (!has)
                {
                    _logger.Info($"The sub job [{subJobId}] not found in ArchiverIndexSubInfoes.");
                    _notFoundJobIds.Add(subJobId);
                    return false;
                }

                _logger.Info($"The sub job [{subJobId}] media data size [{subJobInfo.MediaDataSize}].");
                jobInfo.SubInfoDic[subJobId] = subJobInfo;
            }

            return true;
        }

        public bool IsFileLevelBackup(string subJobId)
        {
            var masterJobId = GetMasterJobId(subJobId);
            return _jobInfoDic[masterJobId].SiteMasterIndex.BackupFileType == 1 || _jobInfoDic[masterJobId].SiteMasterIndex.BackupFileType == 2;
        }

        public Guid GetSiteId(string subJobId)
        {
            var masterJobId = GetMasterJobId(subJobId);
            return new Guid(_jobInfoDic[masterJobId].SiteMasterIndex.SiteId);
        }

        public async Task<string> GetStorageIdAsync(string subJobId)
        {
            await HasJobInfoAsync(subJobId);
            var masterJobId = GetMasterJobId(subJobId);
            var subJobInfo = _jobInfoDic[masterJobId].SubInfoDic[subJobId];
            return string.IsNullOrEmpty(subJobInfo.CurrentStorageId) ? subJobInfo.StorageInfo : subJobInfo.CurrentStorageId;
        }

        public bool IsSystemStorage(string storageId)
        {
            if(storageId == RecordsConstants.AVEPOINT_DEFAULT_STORAGEID)
            {
                return true;
            }
            if(!_storageIsSystemDic.ContainsKey(storageId))
            {
                var storageDeviceDto = _storageDeviceService.GetStorageDeviceById(storageId, needDecryptSecert: true);
                _storageIsSystemDic[storageId] = storageDeviceDto != null && storageDeviceDto.IsSystemStorage;
            }
            return _storageIsSystemDic[storageId];
        }

        public void DecreaseSize(string subJobId, long size)
        {
            var masterJobId = GetMasterJobId(subJobId);
            var subJobInfo = _jobInfoDic[masterJobId].SubInfoDic[subJobId];
            subJobInfo.MediaDataSize -= size;
            _logger.Info($"The sub job [{subJobId}] media data size [{subJobInfo.MediaDataSize}] after decrease.");
        }

        public void SyncSubJobDataSize()
        {
            try
            {
                var subJobInfoes = _jobInfoDic.Values.SelectMany(item => item.SubInfoDic.Values).ToList();
                subJobInfoes.ForEach(item =>
                {
                    if (item.MediaDataSize < 0)
                    {
                        item.MediaDataSize = 0;
                    }
                });

                _archiveIndexSubInfoDao.UpdateSubInfoes(subJobInfoes.ToArray());
                _logger.Info($"The sub jobs [{string.Join(", ", subJobInfoes.Select(item => item.Id))}] media data size has been synced.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while sync sub jobs [{string.Join(", ", _jobInfoDic.Values.SelectMany(item => item.SubInfoDic.Values).ToList().Select(item => item.Id))}] media data size. Error: {e}");
            }
        }

        private static string GetMasterJobId(string subSubJobId)
        {
            var splitedJobId = subSubJobId.Split("_");
            if (splitedJobId.Length >= 3)
            {
                return splitedJobId[0] + "_" + splitedJobId[1];
            }
            else
            {
                // Opus end user backup job id start with 'EA', and the the job don't have sub job. so sub job id is also the main job id.
                return splitedJobId[0];
            }
        }
    }

    internal class RMBackupedJobInfo
    {
        internal ArchiverSiteMasterIndexContract SiteMasterIndex { get; set; }

        internal Dictionary<string, ArchiverIndexSubInfo> SubInfoDic { get; set; } = [];
    }
}
