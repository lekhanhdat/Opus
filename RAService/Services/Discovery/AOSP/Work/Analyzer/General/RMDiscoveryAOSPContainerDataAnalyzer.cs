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
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General
{
    public class RMDiscoveryAOSPContainerDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPContainerDataAnalyzer));

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly RMDiscoveryAOSPDiscoveryJob _jobInfo;

        private RMDiscoveryAOSPContainerInfo _containerInfo;

        public RMDiscoveryAOSPContainerInfo ContainerInfo { get { return _containerInfo; } }

        public RMDiscoveryAOSPContainerDataAnalyzer(
            RMDiscoveryJobType jobType,
            RMDiscoveryAOSPDiscoveryJob jobInfo
        )
        {
            _nodeDao = new RMDiscoveryAOSPNodeDao();
            _jobType = jobType;
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, RMDiscoveryAOSPContainerInfo containerInfo)> InitAsync()
        {
            try
            {
                var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(_jobInfo.O365TenantId, _jobInfo.ContainerId);
                var aosContainers = await _nodeDao.GetAOSContainersForAOSPAsync(_jobInfo.O365TenantId.ToString(), Contract.Explorer.SourceFlag.SharePoint, Contract.Explorer.SourceFlag.OneDrive);
                _logger.Info($"Current container ids is[{string.Join(", ", aosContainers.Select(c => c.Id))}]");
                if (!has)
                {
                    var opusContainerInfo = aosContainers.Where(container => new Guid(container.Id) == _jobInfo.ContainerId).FirstOrDefault();
                    if (opusContainerInfo != null)
                    {
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
                        containerInfo = new RMDiscoveryAOSPContainerInfo
                        {
                            Name = opusContainerInfo.Url,
                            AosId = !string.IsNullOrEmpty(opusContainerInfo.AosId) ? new Guid(opusContainerInfo.AosId) : Guid.Empty,
                            OpusId = !string.IsNullOrEmpty(opusContainerInfo.Id) ? new Guid(opusContainerInfo.Id) : Guid.Empty,
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
                    else
                    {
                        containerInfo = new RMDiscoveryAOSPContainerInfo
                        {
                            Name = "",
                            AosId = Guid.Empty,
                            OpusId = Guid.Empty,
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
                    }
                }

                _containerInfo = containerInfo;
                return (true, _containerInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return (false, null);
            }
        }

        public void Increse(RMDiscoveryAOSPAggregateTotalData totalData)
        {
            if (_jobType == RMDiscoveryJobType.Rescan)
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
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    return await RecalculateAndSaveAsync();
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_jobInfo.O365TenantId, _containerInfo);
                _logger.Info($"Succeed save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data.");

                return true;
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                _logger.Info($"An error occurred while recalculate and save tenant [{_jobInfo.O365TenantId}] container [{_jobInfo.ContainerId}] data. Error: {e}");
                return false;
            }
        }
    }
}
