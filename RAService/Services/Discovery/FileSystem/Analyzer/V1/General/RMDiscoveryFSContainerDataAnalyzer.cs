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
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General
{
    public class RMDiscoveryFSContainerDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSContainerDataAnalyzer));

        private readonly IRMDiscoveryFSNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly RMDiscoveryFSDiscoveryJob _jobInfo;

        private RMDiscoveryFSContainerInfo _containerInfo;

        public RMDiscoveryFSContainerInfo ContainerInfo { get { return _containerInfo; } }

        public RMDiscoveryFSContainerDataAnalyzer(
            RMDiscoveryJobType jobType,
            RMDiscoveryFSDiscoveryJob jobInfo
        )
        {
            _nodeDao = new RMDiscoveryFSNodeDao();
            _jobType = jobType;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryFSContainerInfo containerInfo)> InitAsync()
        {
            try
            {
                var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(_jobInfo.ContainerId);
                if (!has)
                {
                    var opusContainerInfo = await _nodeDao.GetConnectionGroupsById(_jobInfo.ContainerId);
                    containerInfo = new RMDiscoveryFSContainerInfo
                    {
                        Name = opusContainerInfo.Name,
                        AosId = new Guid(),
                        OpusId = opusContainerInfo.Id,
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        FileTotalSize = 0,
                        FileSumCount = 0,
                        ConnectionCount = 0,
                        VersionTotalSize = 0,
                        MaxFileAge = 0
                    };
                    await _nodeDao.AddOrUpdateDiscoveryContainerAsync(containerInfo);
                    _logger.Info($"Succeed create container [{containerInfo.Id} - {containerInfo.Name}] data.");
                }

                _containerInfo = containerInfo;
                return (true, _containerInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init container [{_jobInfo.ContainerId}] data. Error: {e}");
                return (false, null);
            }
        }

        public void Increse(RMDiscoveryFSAggregateTotalData totalData)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            _containerInfo.FileTotalSize += totalData.FileTotalSize;
            _containerInfo.FileSumCount += totalData.FileSumCount;
            _containerInfo.VersionTotalSize += totalData.TotalVersionSize;
            _containerInfo.MaxFileAge = Math.Max(_containerInfo.MaxFileAge, totalData.MaxFileAge);
            _containerInfo.ConnectionCount++;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_containerInfo);
                _logger.Info($"Succeed save container [{_jobInfo.ContainerId}] data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save container [{_jobInfo.ContainerId}] data. Error: {e}");
                return false;
            }
        }
    }
}
