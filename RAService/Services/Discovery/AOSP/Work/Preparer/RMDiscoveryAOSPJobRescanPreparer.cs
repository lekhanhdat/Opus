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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Preparer;

public class RMDiscoveryAOSPJobRescanPreparer : RMDiscoveryAOSPWorker, IRMDiscoveryAOSPJobPreparable
{
    private readonly RMDiscoveryAOSPRescanJobParameter _jobParameter;
    
    public RMDiscoveryAOSPJobRescanPreparer(RMDiscoveryAOSPRescanJobParameter jobParameter) : base()
    {
        _jobParameter = jobParameter;
    }

    public async Task<(bool success, string errorMessage, Guid jobId)> PrepareAsync()
    {
        try
        {
            var mainJobId = Guid.Empty;
            foreach (var entry in _jobParameter.SiteUniqueIds)
            {
                var o365TenantId = entry.Key;
                var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync(o365TenantId);
                if (has)
                {
                    _logger.Error($"[AOSP] [{RMDiscoveryJobType.Rescan}] This tenant [{o365TenantId}] is already job [{mainJob.Id}] begin executed.");
                    continue;
                }

                var siteInfoes = await _nodeDao.GetSiteInfosBySiteIds(new Guid(o365TenantId), entry.Value.ToList().ConvertAll(i => new Guid(i)));
                var containerIds = siteInfoes.Select(i => i.ContainerId).ToHashSet();

                mainJob = new RMDiscoveryAOSPMainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containerIds.Count,
                    SitesCount = siteInfoes.Count,
                    NeedToReRegisterTags = false,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = RMDiscoveryJobStatus.Waiting,
                    Type = RMDiscoveryJobType.Rescan,
                    O365TenantId = o365TenantId,
                    AppProfileId = _jobParameter.AppProfileId,
                    Comment = string.Empty
                };

                await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                await _configurationDao.DeleteByO365TenantIdAndTypeAsync(o365TenantId, Contract.Discovery.Model.RMDiscoveryConfigurationType.AOSPRescanScope);
                await _configurationDao.AddOrUpdateAsync(new RMDiscoveryAOSPConfiguration
                {
                    ConfigurationType = Contract.Discovery.Model.RMDiscoveryConfigurationType.AOSPRescanScope,
                    O365TenantId = o365TenantId,
                    ValueJson = JsonConvert.SerializeObject(entry.Value),
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedTime = DateTime.UtcNow.Ticks
                });
                mainJobId = mainJob.Id;
                _logger.Info($"Tenant [{o365TenantId}] discovery [{RMDiscoveryJobType.Rescan}] job [{mainJob.Id}] is prepared.");
            }

            return (true, string.Empty, mainJobId);
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Rescan}] job. Error: {e}");
            return (false, string.Empty, Guid.Empty);
        }
    }

}
