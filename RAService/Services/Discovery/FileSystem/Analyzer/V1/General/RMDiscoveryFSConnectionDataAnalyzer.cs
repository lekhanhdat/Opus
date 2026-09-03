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
    public class RMDiscoveryFSConnectionDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSConnectionDataAnalyzer));

        private readonly IRMDiscoveryFSNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly int _containerId;

        private readonly RMDiscoveryFSAnalysisJob _jobInfo;

        public RMDiscoveryFSConnectionDataAnalyzer(
            RMDiscoveryJobType jobType,
            int containerId,
            RMDiscoveryFSAnalysisJob jobInfo
        )
        {
            _nodeDao = new RMDiscoveryFSNodeDao();
            _jobType = jobType;
            _containerId = containerId;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryFSConnectionInfo connectionInfo)> InitAsync(RMDiscoveryFSAggregateTotalData totalData)
        {
            try
            {
                var connectionInfo = await _nodeDao.GetDiscoveryConnectionInfoAsync(_jobInfo.ConnectionId);
                if (_jobType == RMDiscoveryJobType.Retry && connectionInfo != null)
                {
                    connectionInfo.Name = _jobInfo.ConnectionName;
                    connectionInfo.UNCPath = _jobInfo.UNCPath;
                    connectionInfo.ContainerId = _containerId;
                    connectionInfo.FileTotalSize = totalData.FileTotalSize;
                    connectionInfo.FileSumCount = totalData.FileSumCount;
                    connectionInfo.MaxFileAge = totalData.MaxFileAge;
                    connectionInfo.VersionTotalSize = totalData.TotalVersionSize;
                    connectionInfo.ModifiedTime = DateTime.UtcNow.Ticks;
                    await _nodeDao.AddOrUpdateDiscoveryConnectionAsync(connectionInfo);
                    _logger.Info($"Succeed reset agent [{_jobInfo.AgentId}] connection [{_jobInfo.ConnectionId}] [{_jobInfo.UNCPath}] data.");
                }

                if (connectionInfo == null)
                {
                    connectionInfo = new RMDiscoveryFSConnectionInfo
                    {
                        Name = _jobInfo.ConnectionName,
                        UNCPath = _jobInfo.UNCPath,
                        ConnectionId = _jobInfo.ConnectionId,
                        ContainerId = _containerId,
                        FileTotalSize = totalData.FileTotalSize,
                        FileSumCount = totalData.FileSumCount,
                        CreateTime = DateTime.UtcNow.Ticks,
                        MaxFileAge = totalData.MaxFileAge,
                        VersionTotalSize = totalData.TotalVersionSize,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    };
                    await _nodeDao.AddOrUpdateDiscoveryConnectionAsync(connectionInfo);
                    _logger.Info($"Succeed create agent [{_jobInfo.AgentId}] connection [{_jobInfo.ConnectionId}] [{_jobInfo.UNCPath}] data.");
                }

                return (true, connectionInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init agent [{_jobInfo.AgentId}] connection [{_jobInfo.ConnectionId}] [{_jobInfo.UNCPath}] data. Error: {e}");
                return (false, null);
            }
        }
    }
}
