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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General
{
    public class RMDiscoveryGoogleDriveDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDriveDataAnalyzer));

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly int _containerId;

        private readonly RMDiscoveryGoogleAnalysisJob _jobInfo;

        public RMDiscoveryGoogleDriveDataAnalyzer(
            RMDiscoveryJobType jobType,
            int containerId,
            RMDiscoveryGoogleAnalysisJob jobInfo
            )
        {
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _jobType = jobType;
            _containerId = containerId;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryGoogleDriveInfo driveInfo)> InitAsync(RMDiscoveryGoogleAggregateTotalData totalData)
        {
            try
            {
                var driveInfo = await _nodeDao.GetDiscoveryGoogleDriveInfoAsync(_jobInfo.OrganizationId, _jobInfo.DriveId);
                if (_jobType == RMDiscoveryJobType.Retry && driveInfo != null)
                {
                    driveInfo.DriveId = _jobInfo.DriveId;
                    driveInfo.DriveName = _jobInfo.DriveName;
                    driveInfo.DriveType = _jobInfo.DriveType;
                    driveInfo.ContainerId = _containerId;
                    driveInfo.FileTotalSize = totalData.FileTotalSize;
                    driveInfo.FileSumCount = totalData.FileSumCount;
                    driveInfo.MaxFileAge = totalData.MaxFileAge;
                    driveInfo.VersionTotalSize = totalData.TotalVersionSize;
                    driveInfo.ModifiedTime = DateTime.UtcNow.Ticks;
                    await _nodeDao.AddOrUpdateDiscoveryGoogleDriveAsync(_jobInfo.OrganizationId, driveInfo);
                    _logger.Info($"Succeed reset organization [{_jobInfo.OrganizationId}] drive [{_jobInfo.DriveId}] [{_jobInfo.DriveName}] data.");
                }

                if (driveInfo == null)
                {
                    driveInfo = new RMDiscoveryGoogleDriveInfo
                    {
                        DriveId = _jobInfo.DriveId,
                        DriveName = _jobInfo.DriveName,
                        DriveType = _jobInfo.DriveType,
                        ContainerId = _containerId,
                        FileTotalSize = totalData.FileTotalSize,
                        FileSumCount = totalData.FileSumCount,
                        CreateTime = DateTime.UtcNow.Ticks,
                        MaxFileAge = totalData.MaxFileAge,
                        VersionTotalSize = totalData.TotalVersionSize,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    };
                    await _nodeDao.AddOrUpdateDiscoveryGoogleDriveAsync(_jobInfo.OrganizationId, driveInfo);
                    _logger.Info($"Succeed create organization [{_jobInfo.OrganizationId}] drive [{_jobInfo.DriveId}] [{_jobInfo.DriveName}] data.");
                }

                return (true, driveInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init organization [{_jobInfo.OrganizationId}] drive [{_jobInfo.DriveId}] [{_jobInfo.DriveName}] data. Error: {e}");
                return (false, null);
            }
        }
    }
}
