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
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce
{
    public class RMDiscoverySalesforceJobDao : IRMDiscoverySalesforceJobDao
    {

        private static readonly HashSet<RMDiscoveryJobStatus> S_PROCESSING_JOB_STATUS = new()
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };

        public async Task<(bool has, RMDiscoverySalesforceMainJob mainJobInfo)> TryGetProcessingMainJobAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.SalesforceMainJob.FirstOrDefaultAsync(item => S_PROCESSING_JOB_STATUS.Contains(item.Status));
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoverySalesforceMainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types)
        {
            var hasTypes = types.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var latestJob = await efContext.SalesforceMainJob
                .Where(item => (!hasTypes || Enumerable.Contains(types, item.Type)))
                .OrderByDescending(item => item.StartTime).FirstOrDefaultAsync();
            return (latestJob != null, latestJob);
        }

        public async Task<(bool has, RMDiscoverySalesforceMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.SalesforceMainJob.FirstOrDefaultAsync(item => item.Status == status);
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoverySalesforceMainJob mainJob)> TryGetMainJobAsync(Guid id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var job = await efContext.SalesforceMainJob.FirstOrDefaultAsync(item => item.Id == id);
            if (job != null && job.Type == RMDiscoveryJobType.None)
            {
                job.Type = RMDiscoveryJobType.Newly;
            }
            return (job != null, job);
        }

        public async Task AddOrUpdateMainJobAsync(RMDiscoverySalesforceMainJob mainJobInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            mainJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
            efContext.SalesforceMainJob.AddOrUpdate(mainJobInfo);
            await efContext.SaveChangesAsync();
        }
    }
}
