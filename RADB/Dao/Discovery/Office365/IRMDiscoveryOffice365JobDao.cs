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
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365JobDao
    {
        Task<(bool has, RMDiscoveryOffice365MainJob mainJobInfo)> TryGetProcessingMainJobAsync();

        Task<(bool has, RMDiscoveryOffice365MainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types);

        Task AddOrUpdateMainJobAsync(RMDiscoveryOffice365MainJob mainJobInfo);

        Task<(bool has, RMDiscoveryOffice365MainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status);

        Task<(bool has, RMDiscoveryOffice365MainJob mainJob)> TryGetMainJobAsync(Guid id);

        Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryOffice365DiscoveryJob[] discoveryJobs);

        Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryOffice365AnalysisJob[] analysisJobs);

        Task BatchInsertAnalysisJobAsync(List<RMDiscoveryOffice365AnalysisJob> analysisJobs);

        Task<List<RMDiscoveryOffice365AnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout);

        Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId);

        Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId);

        Task<List<RMDiscoveryOffice365DiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status);
        Task<List<RMDiscoveryOffice365MainJob>> GetDiscoveryJobsHangingAsync(long threshold);

        Task<RMDiscoveryOffice365DiscoveryJob> GetDiscoveryJobAsync(Guid id);

        Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId);

        Task<List<RMDiscoveryOffice365AnalysisJob>> GetAnalysisJobsAsync(Guid mainJobId, int count, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobReportWithPaginationAsync(Guid mainJobId, int pageSize);

        Task<int> CountAnalysisJobsByMainJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status);

        Task<(bool has, RMDiscoveryOffice365AnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discvoeryJobId, Guid siteId, params RMDiscoveryJobStatus[] status);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId);

        Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus);

        Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, params Guid[] discoveryJobIds);

        Task<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobByIdAsync(Guid analysisJobId);
    }
}
