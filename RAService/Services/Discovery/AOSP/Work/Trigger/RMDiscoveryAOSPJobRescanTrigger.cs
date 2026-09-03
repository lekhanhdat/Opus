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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using Cloud.Sdk.AosModern;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Trigger;

public class RMDiscoveryAOSPJobRescanTrigger : RMDiscoveryAOSPWorker, IRMDiscoveryAOSPJobTriggerible
{

    private readonly RMDiscoveryAOSPMainJob _jobInfo;

    internal RMDiscoveryAOSPJobRescanTrigger(RMDiscoveryAOSPMainJob jobInfo) : base()
    {
        _jobInfo = jobInfo;
    }

    public async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items, string errorMessage)> GetWillTriggerJobsAsync()
    {
        try
        {
            var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();
            var siteIds = await _configurationDao.GetByO365TenantIdAsync<List<string>>(Contract.Discovery.Model.RMDiscoveryConfigurationType.AOSPRescanScope, _jobInfo.O365TenantId);
            var siteInfoes = await _nodeDao.GetSiteInfosBySiteIds(new Guid(_jobInfo.O365TenantId), siteIds.ConvertAll(i => new Guid(i)));
            var containerIntIds = siteInfoes.Select(i => i.ContainerId).ToHashSet();
            var containerInfos = await _nodeDao.GetDiscoveryContainersAsync(new Guid(_jobInfo.O365TenantId), containerIntIds);
            foreach (var containerInfo in containerInfos)
            {
                var sites = siteInfoes.Where(i => i.ContainerId == containerInfo.Id).ToList();
                res.Add((new Guid(_jobInfo.O365TenantId), new RMRemoteNode
                {
                    Id = containerInfo.OpusId.ToString(),
                    Url = containerInfo.Name,
                    NodeLevel = containerInfo.ContentSource == Contract.Explorer.SourceFlag.OneDrive ? (int)NodeLevel.SkyDriveProGroup : (int)NodeLevel.SiteCollection
                }, sites.ConvertAll(item => new RMRemoteNode
                {
                    ObjectId = item.SiteId.ToString(),
                    Url = item.Url,
                })));
            }

            _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
            return (true, res, string.Empty);
        }
        catch(Exception e)
        {
            _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
            return (false, [], e.Message);
        }
    }

    public Task<(bool succeed, string errorMessage)> InitTablesAsync(List<Guid> o365TenantIds)
    {
        return Task.FromResult((true, ""));
    }
}
