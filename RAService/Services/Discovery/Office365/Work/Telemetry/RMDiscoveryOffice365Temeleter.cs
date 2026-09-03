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
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry
{
    public class RMDiscoveryOffice365Temeleter
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365Temeleter));

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly Guid _mainJobId;

        private long _fileSumCount = 0;

        private long _fileTotalSize { get; set; }

        public RMDiscoveryOffice365Temeleter(Guid mainJobId)
        {
            _jobDao = new RMDiscoveryOffice365JobDao();
            _mainJobId = mainJobId;
        }

        public void Increse(long fileSumCount, long fileTotalSize)
        {
            _fileSumCount += fileSumCount;
            _fileTotalSize += fileTotalSize;
        }

        public async Task RecordAsync()
        {
            try
            {
                var (_, mainJobInfo) = await _jobDao.TryGetMainJobAsync(_mainJobId);
                var telemetryData = new List<object>
                {
                    mainJobInfo.Id,
                    "Office365",
                    mainJobInfo.Type,
                    mainJobInfo.Version,
                    (new DateTime(mainJobInfo.EndTime) - new DateTime(mainJobInfo.StartTime)).TotalSeconds,
                    mainJobInfo.Status,
                    mainJobInfo.SitesCount,
                    _fileSumCount,
                    _fileTotalSize,
                };
                TelemetryContext.SendToQueue(TelemetryModule.DiscoveryAndAnalysis, TelemetryEventType.DiscoveryAndAnalysisEachJobInfo, telemetryData);
                await TelemetryContext.FlushAsync();
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while record telemetry. Error: {e}");
            }
        }
    }
}
