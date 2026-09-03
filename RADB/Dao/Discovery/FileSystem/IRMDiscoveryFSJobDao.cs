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
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.FileSystem
{
    public interface IRMDiscoveryFSJobDao
    {
        Task<(bool has, RMDiscoveryFSMainJob mainJobInfo)> TryGetProcessingMainJobAsync();

        Task AddOrUpdateMainJobAsync(RMDiscoveryFSMainJob mainJobInfo);

        Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryFSDiscoveryJob[] discoveryJobs);

        Task<(bool has, RMDiscoveryFSMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status);

        Task<(bool has, RMDiscoveryFSMainJob mainJob)> TryGetMainJobAsync(Guid jobId);

        Task<(bool has, RMDiscoveryFSMainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types);

        Task<(bool has, RMDiscoveryFSAnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discoveryJobId, Guid connectionId, params RMDiscoveryJobStatus[] status);

        Task<List<RMDiscoveryFSAnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId);

        Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryFSAnalysisJob[] analysisJobs);

        Task<List<RMDiscoveryFSDiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status);

        Task<List<RMDiscoveryFSAnalysisJob>> GetAnalysisJobsAsync(Guid mainJobId, int count, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryFSAnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryFSAnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryFSAnalysisJob> GetAnalysisJobReportWithPaginationAsync(Guid mainJobId, int pageSize);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId);

        Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus);

        Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, params Guid[] discoveryJobIds);

        Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId);

        Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId);

        Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus);

        Task BatchInsertAnalysisJobAsync(List<RMDiscoveryFSAnalysisJob> analysisJobs);
    }
}
