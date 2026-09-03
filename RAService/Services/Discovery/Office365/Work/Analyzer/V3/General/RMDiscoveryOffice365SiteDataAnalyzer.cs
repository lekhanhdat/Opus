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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.General
{
    public class RMDiscoveryOffice365SiteDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteDataAnalyzer));

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly SourceFlag _contentSource;

        private readonly int _containerId;

        private readonly RMDiscoveryOffice365AnalysisJob _jobInfo;

        public RMDiscoveryOffice365SiteDataAnalyzer(
            RMDiscoveryJobType jobType,
            SourceFlag contentSource,
            int containerId,
            RMDiscoveryOffice365AnalysisJob jobInfo
        )
        {
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobType = jobType;
            _contentSource = contentSource;
            _containerId = containerId;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryOffice365SiteInfo siteInfo)> InitAsync(RMDiscoveryOffice365AggregateTotalData totalData)
        {
            try
            {
                var siteInfo =  await _nodeDao.GetDiscoverySiteInfoAsync(_jobInfo.O365TenantId, _jobInfo.SiteId);
                if(_jobType == RMDiscoveryJobType.Retry && siteInfo != null)
                {
                    siteInfo.Url = _jobInfo.Url;
                    siteInfo.ContainerId = _containerId;
                    siteInfo.ContentSource = _contentSource;
                    siteInfo.FileTotalSize = totalData.FileTotalSize;
                    siteInfo.FileSumCount = totalData.FileSumCount;
                    siteInfo.MaxFileAge = totalData.MaxFileAge;
                    siteInfo.PHLTotalSize = totalData.PHLVolume;
                    siteInfo.VersionTotalSize = totalData.TotalVersionSize;
                    siteInfo.ModifiedTime = DateTime.UtcNow.Ticks;
                    await _nodeDao.AddOrUpdateDiscoverySiteAsync(_jobInfo.O365TenantId, siteInfo);
                    _logger.Info($"Succeed reset tenant [{_jobInfo.O365TenantId}] site [{_jobInfo.SiteId}] [{_jobInfo.Url}] data.");
                }

                if(siteInfo == null)
                {
                    siteInfo = new RMDiscoveryOffice365SiteInfo
                    {
                        Url = _jobInfo.Url,
                        SiteId = _jobInfo.SiteId,
                        ContainerId = _containerId,
                        ContentSource = _contentSource,
                        FileTotalSize = totalData.FileTotalSize,
                        FileSumCount = totalData.FileSumCount,
                        CreateTime = DateTime.UtcNow.Ticks,
                        MaxFileAge = totalData.MaxFileAge,
                        PHLTotalSize = totalData.PHLVolume,
                        VersionTotalSize = totalData.TotalVersionSize,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    };
                    await _nodeDao.AddOrUpdateDiscoverySiteAsync(_jobInfo.O365TenantId, siteInfo);
                    _logger.Info($"Succeed create tenant [{_jobInfo.O365TenantId}] site [{_jobInfo.SiteId}] [{_jobInfo.Url}] data.");
                }

                return (true, siteInfo);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while init tenant [{_jobInfo.O365TenantId}] site [{_jobInfo.SiteId}] [{_jobInfo.Url}] data. Error: {e}");
                return (false, null);
            }
        }
    }
}
