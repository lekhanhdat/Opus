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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Preparer
{
    public class RMDiscoveryOffice365JobRetryPreparer(Guid needRetryJobId) : RMDiscoveryOffice365Worker(), IRMDiscoveryOffice365JobPreparable
    {

        public async Task<(bool success, string errorMessage)> PrepareAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync();
                if (has)
                {
                    _logger.Error($"There is already job [{mainJob.Id}] begin executed.");
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_HasRunningJob"));
                }

                (has, mainJob) = await _jobDao.TryGetMainJobAsync(needRetryJobId);
                if(!has)
                {
                    _logger.Error($"No job [{needRetryJobId}] found that need to retry.");
                    return (false, null);
                }

                (has, mainJob) = await _jobDao.TryGetLatestMainJobAsync(RMDiscoveryJobType.Newly);
                if(!has)
                {
                    _logger.Error($"No newly job found that need to retry.");
                    return (false, null);
                }

                var o365Tenants = new HashSet<Guid>();
                var containers = new HashSet<Guid>();
                var sites = new HashSet<Guid>();

                await foreach(var job in _jobDao.GetAnalysisJobsWithPaginationAsync(needRetryJobId, 1000, RMDiscoveryJobStatus.Failed, RMDiscoveryJobStatus.Timeout))
                {
                    o365Tenants.Add(job.O365TenantId);
                    containers.Add(job.ContainerId);
                    sites.Add(job.SiteId);
                }

                if(!o365Tenants.Any())
                {
                    _logger.Error($"Job [{needRetryJobId}] no analysis job found that need to retry.");
                    return (false, null);
                }

                _logger.Info($"The o365 tenants [{string.Join(", ", o365Tenants)}] that need to be retry by job [{needRetryJobId}].");
                _logger.Info($"The containers [{string.Join(", ", containers)}] that need to be retry by job [{needRetryJobId}].");
                _logger.Info($"The sites [{string.Join(", ", sites)}] that need to be retry by job [{needRetryJobId}].");

                var retryJobInfo = new RMDiscoveryOffice365MainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containers.Count,
                    SitesCount = sites.Count,
                    NeedToReRegisterTags = false,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = mainJob.Version.ToOffice365ProfileJobInitStatus(),
                    Type = RMDiscoveryJobType.Retry,
                    ParentId = needRetryJobId,
                    MainJobId = mainJob.Id,
                    Version = mainJob.Version.ToOffice365RetryVersion(),
                };

                await _jobDao.AddOrUpdateMainJobAsync(retryJobInfo);

                _logger.Info($"Discovery [{RMDiscoveryJobType.Retry}] job [{retryJobInfo.Id}] is prepared.");

                return (true, null);
            } 
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Retry}] job. Error: {e}");
                return (false, null);
            }
        }
    }
}
