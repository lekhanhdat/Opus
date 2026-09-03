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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General
{
    public class RMDiscoveryGoogleContainerDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleContainerDataAnalyzer));

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly RMDiscoveryGoogleDiscoveryJob _jobInfo;

        private RMDiscoveryGoogleContainerInfo _containerInfo;

        public RMDiscoveryGoogleContainerInfo ContainerInfo { get { return _containerInfo; } }

        public RMDiscoveryGoogleContainerDataAnalyzer(
            RMDiscoveryJobType jobType,
            RMDiscoveryGoogleDiscoveryJob jobInfo
        )
        {
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _jobType = jobType;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryGoogleContainerInfo containerInfo)> InitAsync()
        {
            try
            {
                var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(_jobInfo.OrganizationId, _jobInfo.ContainerId);
                if (!has)
                {
                    var opusContainerInfo = await _nodeDao.GetOpusGoogleContainerById(_jobInfo.ContainerId);
                    containerInfo = new RMDiscoveryGoogleContainerInfo
                    {
                        Name = opusContainerInfo.Url,
                        AosId = new Guid(opusContainerInfo.AosId),
                        OpusId = new Guid(opusContainerInfo.Id),
                        DriveType = _jobInfo.DriveType,
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        FileTotalSize = 0,
                        FileSumCount = 0,
                        DriveCount = 0,
                        VersionTotalSize = 0,
                        MaxFileAge = 0
                    };
                    await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.OrganizationId, containerInfo);
                    _logger.Info($"Succeed create organization [{_jobInfo.OrganizationId}] container [{containerInfo.Id} - {containerInfo.Name}] data.");
                }

                _containerInfo = containerInfo;
                return (true, _containerInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init organization [{_jobInfo.OrganizationId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return (false, null);
            }
        }

        public void Increse(RMDiscoveryGoogleAggregateTotalData totalData)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            _containerInfo.FileTotalSize += totalData.FileTotalSize;
            _containerInfo.FileSumCount += totalData.FileSumCount;
            _containerInfo.VersionTotalSize += totalData.TotalVersionSize;
            _containerInfo.MaxFileAge = Math.Max(_containerInfo.MaxFileAge, totalData.MaxFileAge);
            _containerInfo.DriveCount++;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.OrganizationId, _containerInfo);
                _logger.Info($"Succeed save organization [{_jobInfo.OrganizationId}] container [{_jobInfo.ContainerId}] data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save organization [{_jobInfo.OrganizationId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                _containerInfo.FileTotalSize = 0;
                _containerInfo.FileSumCount = 0;
                _containerInfo.DriveCount = 0;
                _containerInfo.VersionTotalSize = 0;
                _containerInfo.MaxFileAge = 0;

                var siteInfoes = _nodeDao.GetDiscoveryGoogleDriveInfoesAsync(_jobInfo.OrganizationId, _containerInfo.Id);
                await foreach (var siteInfo in siteInfoes)
                {
                    _containerInfo.FileTotalSize += siteInfo.FileTotalSize;
                    _containerInfo.FileSumCount += siteInfo.FileSumCount;
                    _containerInfo.VersionTotalSize += siteInfo.VersionTotalSize;
                    _containerInfo.MaxFileAge = Math.Max(_containerInfo.MaxFileAge, siteInfo.MaxFileAge);
                    _containerInfo.DriveCount++;
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.OrganizationId, _containerInfo);

                _logger.Info($"Succeed recalculate and save organization [{_jobInfo.OrganizationId}] container [{_jobInfo.ContainerId}] data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while recalculate and save organization [{_jobInfo.OrganizationId}] container [{_jobInfo.ContainerId}] data.");
                return false;
            }
        }
    }
}
