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
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.AOSP
{
    public interface IRMDiscoveryAOSPJobDao
    {
        Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetLatestMainJobAsync(string o365TenantId, params RMDiscoveryJobType[] types);

        Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetProcessingMainJobAsync();

        Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetProcessingMainJobAsync(string o365TenantId);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId);

        Task AddOrUpdateMainJobAsync(RMDiscoveryAOSPMainJob mainJobInfo);

        Task<(bool has, RMDiscoveryAOSPMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status);

        Task<(bool has, List<RMDiscoveryAOSPMainJob> mainJobs)> TryGetMainJobsAsync(RMDiscoveryJobStatus status);

        Task<(bool has, RMDiscoveryAOSPMainJob mainJob)> TryGetMainJobAsync(Guid id);

        Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryAOSPDiscoveryJob[] discoveryJobs);

        Task BatchInsertAnalysisJobAsync(List<RMDiscoveryAOSPAnalysisJob> analysisJobs);

        Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus);

        Task<List<RMDiscoveryAOSPAnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout);

        Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryAOSPAnalysisJob[] analysisJobs);

        Task<List<RMDiscoveryAOSPDiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status);

        Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId);

        Task<string> GetAnalysisErrorCommentLatestAsync(Guid discoveryJobId);

        Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId);

        Task<(bool has, RMDiscoveryAOSPAnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discvoeryJobId, Guid siteId, params RMDiscoveryJobStatus[] status);

        Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus);

        Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, string comment, params Guid[] discoveryJobIds);

        Task<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobByIdAsync(Guid analysisJobId);

        IAsyncEnumerable<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        IAsyncEnumerable<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status);

        Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId);

        Task<string> GetDiscoveryErrorCommentLatest(Guid mainJobId);
    }
}
