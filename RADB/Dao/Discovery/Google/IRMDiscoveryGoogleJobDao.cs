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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.DB.Dao.Discovery.Google
{
    public interface IRMDiscoveryGoogleJobDao
    {
        Task<(bool has, RMDiscoveryGoogleMainJob mainJobInfo)> TryGetProcessingMainJobAsync();

        Task AddOrUpdateMainJobAsync(RMDiscoveryGoogleMainJob mainJobInfo);

        Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryGoogleDiscoveryJob[] discoveryJobs);

        Task<(bool has, RMDiscoveryGoogleMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status);

        Task<(bool has, RMDiscoveryGoogleMainJob mainJob)> TryGetMainJobAsync(Guid jobId);

        Task<(bool has, RMDiscoveryGoogleMainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types);

        Task<(bool has, RMDiscoveryGoogleAnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discoveryJobId, string driveId, params RMDiscoveryJobStatus[] status);

        Task<List<RMDiscoveryGoogleAnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId);

        Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryGoogleAnalysisJob[] analysisJobs);

        Task<List<RMDiscoveryGoogleDiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status);

        Task<List<RMDiscoveryGoogleAnalysisJob>> GetAnalysisJobsAsync(Guid mainJobId, int count, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobReportWithPaginationAsync(Guid mainJobId, int pageSize);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId);

        Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus);

        Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, params Guid[] discoveryJobIds);

        Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId);

        Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId);

        Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus);

        Task BatchInsertAnalysisJobAsync(List<RMDiscoveryGoogleAnalysisJob> analysisJobs);
    }
}
