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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMSubJobDao : IBaseDao<RMSubJob>
    {
        Task<List<RMSubJob>> GetDiscoveryAnalysisSubJobs(IEnumerable<Guid> discoveryAnalysisJobIds);
        Task AddOrUpdateSubJobsAsync(params RMSubJob[] subJobs);
        RMSubJob GetSubJob(string id, bool withContext = false);
        void UpdateContentOfSubJobContext(string subJobId, string context);
        Task<JobStatus> GetSubJobStatusAsync(string id);
        Task<(JobStatus Status, string Comment)> GetSubJobStatusWithCommentAsync(string id);
        void DeleteJobContext(string jobId);
        void BatchUpdateBackupFailedStatusByIds(IEnumerable<string> ids, HasCheckedBackupStatus status);
        void AddJobContext(string jobId, string setting);
        double GetSubJobWeight(string jobId);
        bool UpdateSubJobWeight(string jobId, double weight);
        bool UpdateSubJobWeightByParentId(string parentId, double weight);
        void DeleteSubJob(string mainJobId, int jobstate);

        Task<string> Get365TenantIdByMainJobId(string mainJobId);

        bool HasInProgressSubJobByParent(string parentId);
        List<int> GetAllStatesByParent(string parentId);
        List<string> GetAllSubJobIds(string parentId, int[] states);
        List<RMSubJob> GetAllSubJobByMainJobId(string parentId);
        List<RMSubJob> QueryAllSubJobs(COPSubJobRequest request);
        Task<List<RMSubJob>> GetAllSubJobByMainJobIdAsync(string parentId, int[] states);
        List<string> GetAllExcludeSubJobIds(string parentId, int[] states);
        List<string> GetAllSubJobString1sByParentId(string parentId);
        Dictionary<string, string> GetAllSubJobSiteIdsByParentId(string parentId);
        List<RMSubJob> GetOneSubJobByParentIds(List<string> parentIds);
        List<RMSubJob> GetRunableSubJobList();
        Task<List<RMJobContext>> PageQueryJobContextByMainJobId(string mainJobId, int page, int size);
        string GetJobContextSettingByJobId(string jobId);
        string GetParentJobBySubJobId(string subJobId);

        List<RMJobContext> GetJobContextSettingByJobIds(List<string> jobIds);
        string GetJobContextSettingByMainJobId(string jobId);

        RMSubJob GetOneWaitingSubJob(string mainJobId);

        string GetOneWaitingSubJobId(string mainJobId);
        List<string> GetRunningMoveSubJobByDest(string destUrl, bool includeWaiting);

        bool UpdateProgress(string id, double progress, long dateTime);
        bool UpdateStatus(string id, int status, long dateTime, string comments = null);
        bool CascatMainJobProgress(string parentId, int progress, double doubleProgress);
        bool CascatMainJoStatus(string parentId, int status);
        bool UpdateSubJobCount(string parentId, int subJobCount);
        bool UpdateRunable(string id, int runnable, bool updateState = false);
        bool UpdateRunable(string id);

        bool UpdateJobTime(string id, bool isStartTime);

        string CreateJob(RMSubJob sub);
        void BulkCreateJobs(IEnumerable<RMSubJob> subJobs, int batchSize = 5000);
        bool UpdateJob(string id, JobStatus status, string comment);
        bool UpdateJob(string id, int progress);
        Dictionary<string, int> GetAgentJobCount(List<JobType> jobTypes);
        List<RMSubJob> GetRunningAgentJob(List<JobType> jobTypes);
        List<RMSubJob> GetRunningAgentJob(List<JobType> jobTypes, List<string> agentIds);
        List<RMSubJob> GetInProgressAgentJob(List<JobType> jobTypes);
        Task<bool> UpdateAgentIdAsync(string jobId, string agentId);
        string GetOtherOneWaitingPermissionSubJobId(string mainJobId);
        List<string> GetRunningSetPermissionJobIds(string exceptJobId = "");
        /// <summary>
        /// get the running and runnable sub job count.
        /// </summary>
        /// <returns></returns>
        Dictionary<JobType, int> GetRunningAndRunnableSubJobCount();

        Dictionary<string, int> GetRunningAndRunnableMainJobIdAndSubJobCountByJobType(params JobType[] jobTypes);

        /// <summary>
        /// change a number of sub jobs' status from SubJob_Runnable_Waiting to SubJob_Runnable_CanRun
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="count"></param>
        void ChangeRunnableWiating2CanRun(JobType jobType, int count);
        List<string> GetErrorJobSummary(string mainJobId, int limitCount);

        Task<int> UpdateWaitingSubJobToRunnableAsync(string o365TenantId, int maxRunSubJobCount, int runnableSubJobCount, Dictionary<string, int> runningMainJobsAndSubJobCountDict, params JobType[] jobTypes);

        Task<bool> HasWaitingSubJobCountAsync(params JobType[] jobTypes);

        Task<bool> HasWaitingSubJobCountExpectJobTypesAsync(params JobType[] jobTypes);
        Task<Dictionary<JobType, List<RMSubJob>>> GetWaitingSubJobsGroups(List<int> expectJobTypes);

        Task<List<string>> GetSubJobsParentIdsAsync(int jobType);

        Task<bool> UpdateSubJobToRunnableByIdsAsync(List<string> subJobIds);

        Task<int> GetRunningAndRunnableSubJobCountAsync(string o365TenantId, params JobType[] jobTypes);
        Task<Dictionary<string, int>> GetRunningAndRunnableMainJobIdAndSubJobCountAsync(string o365TenantId, params JobType[] jobTypes);
        Task<List<RMSubJob>> GetRunningAndRunnableSubJobListAsync(string o365TenantId, params JobType[] jobTypes);
        Task<List<RMSubJob>> GetRunningAndRunnableSubJobListAsync(params JobType[] jobTypes);
        List<RMSubJob> GetDirtyWaitingArchiverSubJob(List<JobType> jobTypes, List<string> existOffice365tenantIds);
        
        Task<List<RMSubJob>> GetOtherSubJobFinishedAsync(string jobId, string parentId);

        Task<List<RMSubJob>> GetSubJobsBySubJobIdsAsync(List<string> subJobIds);

        Task<bool> IsFinalSubJob(string jobId, string parentId);

        #region archiver
        List<string> GetSubJobScopesByMainJobId(string mainJobId, params string[] searchScope);
        List<string> GetRunningArchiverJobsScopes(List<JobType> types, params string[] excludeJobIds);
        List<RMSubJob> GetFailedSubJobs(Expression<Func<RMSubJob, bool>> whereLambda = null);
        List<RMSubJob> GetNotCheckedFailedSubJobs();
        List<RMSubJob> GetSubJobsByIds(List<string> subJobids);
        void DeleteSubJobById(string subsubJobid);
        List<HSMArchiverSubJobInfo> GetArchiverSubJobsByParentId(string parentId);
        #endregion

        bool TryReserveAgentSlot(string subJobId, string agentId, List<JobType> jobTypes, int maxConcurrent);
    }
}
