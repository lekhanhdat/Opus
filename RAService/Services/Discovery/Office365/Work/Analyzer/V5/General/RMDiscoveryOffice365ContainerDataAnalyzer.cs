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
using AvePoint.RA.I18N.Core;
using Microsoft.SharePoint.Client.CompliancePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General
{
    public class RMDiscoveryOffice365ContainerDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ContainerDataAnalyzer));

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly RMDiscoveryOffice365DiscoveryJob _jobInfo;

        private RMDiscoveryOffice365ContainerInfo _containerInfo;

        public RMDiscoveryOffice365ContainerInfo ContainerInfo {  get { return _containerInfo; } }

        public RMDiscoveryOffice365ContainerDataAnalyzer(
            RMDiscoveryJobType jobType,
            RMDiscoveryOffice365DiscoveryJob jobInfo
        ) 
        {
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobType = jobType;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryOffice365ContainerInfo containerInfo)> InitAsync()
        {
            try
            {
                var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(_jobInfo.O365TenantId, _jobInfo.ContainerId);
                if(!has)
                {
                    var opusContainerInfo = await _nodeDao.GetOpusContainerById(_jobInfo.ContainerId);
                    switch (opusContainerInfo.Url.Trim())
                    {
                        case "Default_ SharePoint Sites_ Group":
                            opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                            break;
                        case "Default Office 365 Group Sites Group":
                            opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                            break;
                        case "Default Private Channel Sites Container":
                            opusContainerInfo.Url = I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                            break;
                    }
                    containerInfo = new RMDiscoveryOffice365ContainerInfo
                    {
                        Name = opusContainerInfo.Url,
                        AosId = new Guid(opusContainerInfo.AosId),
                        OpusId = new Guid(opusContainerInfo.Id),
                        ContentSource = _jobInfo.ContentSource,
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        FileTotalSize = 0,
                        FileSumCount = 0,
                        SiteCount = 0,
                        PHLTotalSize = 0,
                        VersionTotalSize = 0,
                        MaxFileAge = 0
                    };
                    await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.O365TenantId, containerInfo);
                    _logger.Info($"Succeed create tenant [{_jobInfo.O365TenantId}] container [{containerInfo.Id} - {containerInfo.Name}] data.");
                }

                _containerInfo = containerInfo;
                return (true, _containerInfo);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while init tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return (false, null);
            }
        }

        public Task<bool> RefreshAndSaveAsync()
        {
            return RecalculateAndSaveAsync();
        }

        public void Increse(RMDiscoveryOffice365AggregateTotalData totalData)
        {
            if(_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            _containerInfo.FileTotalSize += totalData.FileTotalSize;
            _containerInfo.FileSumCount += totalData.FileSumCount;
            _containerInfo.VersionTotalSize += totalData.TotalVersionSize;
            _containerInfo.PHLTotalSize += totalData.PHLVolume;
            _containerInfo.MaxFileAge = Math.Max(_containerInfo.MaxFileAge, totalData.MaxFileAge);
            _containerInfo.SiteCount++;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if(_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.O365TenantId, _containerInfo);
                _logger.Info($"Succeed save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                _containerInfo.FileTotalSize = 0;
                _containerInfo.FileSumCount = 0;
                _containerInfo.SiteCount = 0;
                _containerInfo.PHLTotalSize = 0;
                _containerInfo.VersionTotalSize = 0;
                _containerInfo.MaxFileAge = 0;

                var siteInfoes = _nodeDao.GetDiscoverySiteInfoesAsync(_jobInfo.O365TenantId, _containerInfo.Id);
                await foreach (var siteInfo in siteInfoes)
                {
                    _containerInfo.FileTotalSize += siteInfo.FileTotalSize;
                    _containerInfo.FileSumCount += siteInfo.FileSumCount;
                    _containerInfo.VersionTotalSize += siteInfo.VersionTotalSize;
                    _containerInfo.PHLTotalSize += siteInfo.PHLTotalSize;
                    _containerInfo.MaxFileAge = Math.Max(_containerInfo.MaxFileAge, siteInfo.MaxFileAge);
                    _containerInfo.SiteCount++;
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.O365TenantId, _containerInfo);

                _logger.Info($"Succeed recalculate and save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data.");
                return true;
            }
            catch(Exception e)
            {
                _logger.Info($"An error occurred while recalculate and save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data.");
                return false;
            }
        }
    }
}
