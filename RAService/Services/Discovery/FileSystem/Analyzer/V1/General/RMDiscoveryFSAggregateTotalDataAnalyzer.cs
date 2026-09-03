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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General
{
    public class RMDiscoveryFSAggregateTotalDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSAggregateTotalDataAnalyzer));

        private readonly IRMDiscoveryFSDataDao _dataDao;

        private readonly IRMDiscoveryFSNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private RMDiscoveryFSAggregateTotalData _data;

        private RMDiscoveryFSAggregateTotalData _memoryData;

        public RMDiscoveryFSAggregateTotalDataAnalyzer(
                RMDiscoveryJobType jobType
            )
        {
            _dataDao = new RMDiscoveryFSDataDao();
            _nodeDao = new RMDiscoveryFSNodeDao();
            _jobType = jobType;
            _data = new();
            _memoryData = new();
        }

        public (bool analysisSucceed, RMDiscoveryFSAggregateTotalData data) Analysis(RMDiscoveryFSAnalyzedDataManager analyzedDataManger)
        {
            try
            {
                var (hasError, hasData, dataInfo) = analyzedDataManger.TryGetAnalyzedConnectionDataInfo();
                if (hasError)
                {
                    return (false, new());
                }

                if (!hasData)
                {
                    return (false, new());
                }

                var nowYear = long.Parse(DateTime.UtcNow.Year.ToString());
                var nowMonth = long.Parse(DateTime.UtcNow.Month.ToString());
                var createTime = dataInfo.MinCreatedMonth;
                var createYear = createTime / 100;
                var createMonth = createTime % 100;
                var maxFileAge = (int)((nowYear - createYear) * 12 + (nowMonth - createMonth));

                _logger.Info($"The current connection [{analyzedDataManger.ConnectionId}] file sum count [{dataInfo.FileSumCount}], file total size [{dataInfo.FileTotalSize}], total version size [{dataInfo.VersionTotalSize}], max file age [{maxFileAge}].");

                return (true, new()
                {
                    FileSumCount = dataInfo.FileSumCount,
                    FileTotalSize = dataInfo.FileTotalSize,
                    TotalVersionSize = dataInfo.VersionTotalSize,
                    MaxFileAge = maxFileAge,
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis connection [{analyzedDataManger.ConnectionId}] aggregate total data. Error: {e}");
                return (false, null);
            }
        }

        public void Memeory()
        {
            _memoryData = JsonConvert.DeserializeObject<RMDiscoveryFSAggregateTotalData>(JsonConvert.SerializeObject(_data));
        }

        public void Increse(RMDiscoveryFSAggregateTotalData data)
        {
            _data.FileSumCount += data.FileSumCount;
            _data.FileTotalSize += data.FileTotalSize;
            _data.TotalVersionSize += data.TotalVersionSize;
            _data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
        }

        public void Fallback()
        {
            _data = _memoryData;
        }

        public long TotalFileSizeBytes => _data.FileTotalSize;

        public async Task<bool> SaveAsync()
        {
            try
            {
                var data = await _dataDao.GetAggregateTotalDataAsync();

                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync(data);
                }

                data.FileSumCount += _data.FileSumCount;
                data.FileTotalSize += _data.FileTotalSize;
                data.TotalVersionSize += _data.TotalVersionSize;
                data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
                data.DuplicateFileTotalSize = -1;

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(data);
                _logger.Info($"Succeed save aggregate total data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save aggregate total data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync(RMDiscoveryFSAggregateTotalData data)
        {
            try
            {
                data.FileSumCount = 0;
                data.FileTotalSize = 0;
                data.TotalVersionSize = 0;
                //data.PHLVolume = 0;
                data.MaxFileAge = 0;
                data.DuplicateFileTotalSize = -1;
                var containerInfoes = await _nodeDao.GetAllDiscoveryContainersAsync();
                foreach (var containerInfo in containerInfoes)
                {
                    data.FileSumCount += containerInfo.FileSumCount;
                    data.FileTotalSize += containerInfo.FileTotalSize;
                    data.TotalVersionSize += containerInfo.VersionTotalSize;
                    data.MaxFileAge = Math.Max(data.MaxFileAge, containerInfo.MaxFileAge);
                }

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(data);
                _logger.Info($"Succeed recalculate and save aggregate total data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save aggregate total data. Error: {e}");
                return false;
            }

        }
    }
}
