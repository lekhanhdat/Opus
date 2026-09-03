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
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General
{
    public class RMDiscoveryAOSPAggregateTotalDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPAggregateTotalDataAnalyzer));

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly IEApiClient _ieApiClient;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private RMDiscoveryAOSPAggregateTotalData _data;

        private RMDiscoveryAOSPAggregateTotalData _memoryData;

        public RMDiscoveryAOSPAggregateTotalDataAnalyzer(
                Guid o365TenantId,
                RMDiscoveryJobType jobType,
                SourceFlag contentSource
            )
        {
            _dataDao = new RMDiscoveryAOSPDataDao();
            _nodeDao = new RMDiscoveryAOSPNodeDao();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();

            _o365TenantId = o365TenantId;
            _jobType = jobType;
            _contentSource = contentSource;
            _data = new()
            {
                ContentSource = contentSource
            };
            _memoryData = new()
            {
                ContentSource = contentSource
            };
        }

        public (bool analysisSucceed, RMDiscoveryAOSPAggregateTotalData data, string comment) Analysis(RMDiscoveryAOSPAnalyzedDataManager analyzedDataManger)
        {
            try
            {
                var (hasError, hasData, dataInfo, errorMessage) = analyzedDataManger.TryGetAnalyzedSiteDataInfo();
                if (hasError)
                {
                    return (false, new()
                    {
                        ContentSource = _contentSource
                    }, errorMessage);
                }

                if (!hasData)
                {
                    return (true, new()
                    {
                        ContentSource = _contentSource
                    }, string.Empty);
                }

                var nowYear = long.Parse(DateTime.UtcNow.Year.ToString());
                var nowMonth = long.Parse(DateTime.UtcNow.Month.ToString());
                var createTime = dataInfo.MinCreatedMonth;
                var createYear = createTime / 100;
                var createMonth = createTime % 100;
                var maxFileAge = (int)((nowYear - createYear) * 12 + (nowMonth - createMonth));

                _logger.Info($"The current site [{analyzedDataManger.SiteId}] file sum count [{dataInfo.FileSumCount}], file total size [{dataInfo.FileTotalSize}], total version size [{dataInfo.VersionTotalSize}], max file age [{maxFileAge}], phl volume [{dataInfo.PHLVolume}].");

                return (true, new()
                {
                    ContentSource = _contentSource,
                    FileSumCount = dataInfo.FileSumCount,
                    FileTotalSize = dataInfo.FileTotalSize,
                    TotalVersionSize = dataInfo.VersionTotalSize,
                    MaxFileAge = maxFileAge,
                    PHLVolume = dataInfo.PHLVolume,
                }, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis site [{analyzedDataManger.SiteId}] aggregate total data. Error: {e}");
                return (false, null, e.Message);
            }
        }

        public void Memeory()
        {
            _memoryData = JsonConvert.DeserializeObject<RMDiscoveryAOSPAggregateTotalData>(JsonConvert.SerializeObject(_data));
        }

        public void Fallback()
        {
            _data = _memoryData;
        }

        public void Increse(RMDiscoveryAOSPAggregateTotalData data)
        {
            _data.FileSumCount += data.FileSumCount;
            _data.FileTotalSize += data.FileTotalSize;
            _data.TotalVersionSize += data.TotalVersionSize;
            _data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
            _data.PHLVolume += data.PHLVolume;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                var data = await _dataDao.GetAggregateTotalDataAsync(_o365TenantId, _contentSource);

                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    return await RecalculateAndSaveAsync(data);
                }

                data.FileSumCount += _data.FileSumCount;
                data.FileTotalSize += _data.FileTotalSize;
                data.TotalVersionSize += _data.TotalVersionSize;
                data.PHLVolume += _data.PHLVolume;
                data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
                data.DuplicateFileTotalSize = -1;

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(_o365TenantId, data);
                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync(RMDiscoveryAOSPAggregateTotalData data)
        {
            try
            {
                data.FileSumCount = 0;
                data.FileTotalSize = 0;
                data.TotalVersionSize = 0;
                data.PHLVolume = 0;
                data.MaxFileAge = 0;
                data.DuplicateFileTotalSize = -1;
                var containerInfoes = await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource);
                foreach (var containerInfo in containerInfoes)
                {
                    data.FileSumCount += containerInfo.FileSumCount;
                    data.FileTotalSize += containerInfo.FileTotalSize;
                    data.TotalVersionSize += containerInfo.VersionTotalSize;
                    data.PHLVolume += containerInfo.PHLTotalSize;
                    data.MaxFileAge = Math.Max(data.MaxFileAge, containerInfo.MaxFileAge);
                }

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(_o365TenantId, data);
                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data. Error: {e}");
                return false;
            }

        }
    }
}
