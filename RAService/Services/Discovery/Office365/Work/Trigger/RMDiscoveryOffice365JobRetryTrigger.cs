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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Trigger
{
    internal class RMDiscoveryOffice365JobRetryTrigger : RMDiscoveryOffice365Worker, IRMDiscoveryOffice365JobTriggerible
    {

        private readonly RMDiscoveryOffice365MainJob _jobInfo;
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();


        public RMDiscoveryOffice365JobRetryTrigger(RMDiscoveryOffice365MainJob jobInfo) : base()
        {
            _jobInfo = jobInfo;
        }

        public async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items)> GetWillTriggerJobsAsync()
        {
            try
            {
                var (has, needRetryJobInfo) = await _jobDao.TryGetMainJobAsync(_jobInfo.ParentId);
                if(!has)
                {
                    _logger.Error($"No [{RMDiscoveryJobType.Retry}] job [{_jobInfo.Id}] found.");
                    return (false, []);
                }

                var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();

                var remoteNodes = new List<RMRemoteNode>();

                var needRetryDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(needRetryJobInfo.Id, RMDiscoveryJobStatus.Failed, RMDiscoveryJobStatus.Exception);

                await foreach (var job in _jobDao.GetAnalysisJobsWithPaginationAsync(needRetryJobInfo.Id, 1000, RMDiscoveryJobStatus.Failed, RMDiscoveryJobStatus.Timeout))
                {
                    remoteNodes.Add(new RMRemoteNode
                    {
                        ObjectId = job.SiteId.ToString(),
                        ParentId = job.ContainerId.ToString(),
                        Url = job.Url,
                        TenantId = job.O365TenantId.ToString(),
                        FailedCause = (RMRemoteNodeDao.GetRemoteSiteCollectionByObjectId(job.SiteId.ToString()) == null) ? RMDiscoveryJobFailedCause.SiteNotFound : job.FailedCause,
                    });
                }

                var groupedSites = remoteNodes.GroupBy(item => item.TenantId)
                    .ToDictionary(item => item.Key, item => item.GroupBy(i => i.ParentId)
                    .ToDictionary(i => i.Key, i => i.ToList()));
                foreach(var groupedSite in groupedSites)
                {
                    foreach(var groupedByContainerSite in groupedSite.Value)
                    {
                        var discoveryJob = needRetryDiscoveryJobs.First(item => item.ContainerId == new Guid(groupedByContainerSite.Key));
                        res.Add((new Guid(groupedSite.Key), new RMRemoteNode
                        {
                            Id = discoveryJob.ContainerId.ToString(),
                            Url = discoveryJob.ContainerName,
                            NodeLevel = discoveryJob.ContentSource == Contract.Explorer.SourceFlag.OneDrive ? (int)NodeLevel.SkyDriveProGroup : (int)NodeLevel.WebApplication
                        }, groupedByContainerSite.Value));
                    }
                }

                return (true, res);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs by [{RMDiscoveryJobType.Retry}] job [{_jobInfo.Id}]. Error: {e}");
                return (false, []);
            }
        }

        public Task<bool> InitTablesAsync(List<Guid> o365TenantIds)
        {
            return Task.FromResult(true);
        }
    }
}
