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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao
{
    public interface IJobMonitorDao : IBaseDao<RMJobMonitor>
    {
        List<RMJobMonitor> GetJobs(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMJobMonitor, bool>> whereLambda = null);

        RMJobMonitor GetJob(string id, bool userNameNeedI18N = true);
        List<string> GetRCCJobIds(List<string> scopeIds);
        RMJobMonitor GetJobById(string id);
        RMJobMonitor GetSpecialJob(string id, bool userNameNeedI18N = true);

        List<RMJobMonitor> GetJobs(List<string> idArray);
        Task<List<RMJobMonitor>> GetJobsAsync(List<string> idArray);

        List<RMJobMonitor> GetJobsByProfileId(int profileId);

        List<RMJobMonitor> GetJobsByProfileIds(List<int> profileId);

        List<RMJobMonitor> GetJobsByJobType(JobType jobType);

        RMJobMonitor GetLastestJobByJobType(JobType jobType);

        string GetProfileNameById(int id);

        List<int> GetFilterList(Expression<Func<RMJobMonitor, int>> selectLambda);

        int DeleteJobs(List<string> idArray);

        Task<int> DeleteJobByJobTypes(List<JobType> jobTypes);

        int StopJobs(List<string> idArray);

        string CreateJob(string id, JobType jobType, string jobRunBy, string containerId = null, string scopedId = null, string fullPath = null);
        
        string CreateJobWithGControlJobId(string id, string gControlJobId, JobType jobType, string jobRunBy, string containerId = null, string scopedId = null, string fullPath = null);
        Task<string> CreateDiscoveryJobWithGControlJobId(string id, string gControlJobId, string jobRunBy, Guid mainJobId, Guid discoveryJobId, JobType jobType);

        Task CreateDiscoveryJobAsync(string id, string jobRunBy, Guid mainJobId, Guid discoveryJobId, JobType discoveryJobType);

        string CreateJobWithScopeId(string id, JobType jobType, string jobRunBy, string scopeId, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null,string fullPath = null,string jobConflictExtension = null);
        string CreateJobWithScopeIdAndWithGControlJobId(string id, string gControlJobId, JobType jobType, string jobRunBy, string scopeId, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null,string fullPath = null,string jobConflictExtension = null);
        string CreateJobWithScopeIdForTeams(string id, JobType jobType, string jobRunBy, string scopeId, string additionalInformation, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null, string fullPath = null, string jobConflictExtension = null);
        string CreateJobWithScopeIdForRecenter(string id, JobType jobType, string jobRunBy, string scopeId, int nodeType, string realRunJobUser, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null);
        bool HasRunningArchiverJobOnScope(List<JobType> types, string scope);
        List<string> GetRunningArchiverJobOnScope(List<JobType> types, string scope);
        List<RMJobMonitor> HasRunningArchiverJob(List<JobType> types);
        bool HasStoppingArchiverJobOnScope(List<JobType> types, string scope);

        string CreateJobWithProfileId(string id, JobType jobType, string jobRunBy, int profileId, string userId = null, int subJobCount = 0);

        bool UpdateJob(string id, int progress);

        /// <summary>
        /// 此方法用于避免因数据量较大无法及时更新job进度，导致的job 超时失败。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> UpdateJobWithoutProgressChangeAsync(string id);
        bool UpdateJobExtension(string id, string extension);
        bool UpdateJob(string id, JobStatus status);

        bool UpdateJob(string id, JobStatus status, string comment, bool cascadeSubJob = false);

        bool UpdateMigrationJob(string id, JobStatus status, string comment, string additionalInformation);

        bool UpdateJobAdditionalInformation(string id, string additionalInformation);

        bool UpdateJobExtensionById(string id, string extension);

        bool UpdateJob(string id, int progress, int status, long endTime, string comment = null);

        bool AtomicityUpdateJobExtension(string jobId, string oldJobExtension, string newJobExtension);

        int GetJobProgress(string id);

        RMJobMonitor GetLastFinishedJob(JobType jobType);
        List<string> GetRunningJobs(JobType jobType);
        List<RMJobMonitor> GetRunningJobs(List<JobType> jobTypes, string scopeId);

        List<RMJobMonitor> GetRunningJobsBatch(List<JobType> jobTypes, List<string> scopeIds);

        List<RMJobMonitor> GetRunningJobs(List<JobType> jobTypes);
        List<RMJobMonitor> GetRunningExpectStoppingJobs(List<string> jobIds);
        List<string> GetRunningJobsScopeId(JobType jobType);
        Task<bool> IsHavingRunningJob();
        

        //List<string> GetRunningJobs(JobType jobType, string scopeId);

        List<RMJobMonitor> GetRunningJobsByProfileIds(List<int> profileIds);

        List<RMJobMonitor> GetRunningAndWaitingJobs();
        List<RMJobMonitor> GetPermittedJobByScopeId(int jobType, string scopeId, int[] securityGroupId, int[] status);
        List<RMJobMonitor> GetPermittedJobByScopeId(int jobType, string scopeId, string userId, int[] status);
        List<RMJobMonitor> GetPermittedFinalJobByScopeId(int jobType, string scopeId, string userId);

        List<string> GetTimeOutJobIds(int timeoutMinutesForRecordsJobInProgress,int timeoutMinutesForRecordsJobWaiting);
        List<string> GetSharePointSettingJobs();

        List<string> GetTeamsSettingJobs();

        List<RMJobMonitor> GetUnstatisticFinishRestoreJobsByTime(long startTimeTicks, long finishTimeTicks);
        List<RMJobMonitor> GetUnstatisticFinishRestoreGoogleJobsByTime(long startTimeTicks, long finishTimeTicks);

        List<RMJobMonitor> GetUnstatisticFinishMigrationRestoreJobsByTime(long startTimeTicks, long finishTimeTicks);

        List<string> GetSharePointOnPremiseSettingJobs();

        List<string> GetRunningEXOApplySettingJob();


        string GetJobFakeidByKey(string key);
        List<string> GetUniqueIDSettingJobs();
        List<string> GetSPOnPremUniqueIDSettingJobs();
        List<string> GetCollectionDataSettingJobs();
        List<string> GetRunningSyncSecurityContainerJob();

        List<RMJobMonitor> GetJobInfoByTimeRangeAndStatus(long startTime, long endTime, List<JobType> excludeJobTypes, List<JobStatus> excludeJobStatuses);

        List<RMJobMonitor> GetFailedJobInfoByTimeRange(TimeSpan timeRange, List<JobType> excludeJobTypes = null);
        List<RMJobMonitor> GetLongRunningJobInfoByTimeRange(TimeSpan timeRange, TimeSpan longRunningTimeRange, List<JobType> excludeJobTypes = null);
        List<RMJobMonitor> GetSpecificJobExeptionInfoByTimeRange(TimeSpan timeRange, List<JobType> excludeJobTypes = null);
        Task UpdateJobWithMonitorExceptionAsync(string jobId, MonitorExceptionType exceptionType);
        
        List<string> GetJobIdsByScopeId(List<string> scopeIds);

        string GetJobIdByAdditional(string additional);

        bool CheckHasRunningManualJob();

        bool CheckCurrentUserHasRunningJob(string containerId,string jobId);

        Task<int> ClearOldArchiverJobsAsync();

        Task BulkMigrateJobsAsync(IEnumerable<ArchiverMigrationJobDto> jobs);

        Task<bool> CheckStoppedJobByDiscoveryJobId(Guid mainJobId);
        List<string> GetTeamsUniqueIDSettingJobs();

        int ArchiveDataBatch(int maxRowsPerRun, int olderThanDays, IReadOnlyCollection<int> archiveJobTypes);

        Task<bool> UpdateJobPriorityAsync(List<string> jobIds, JobPriority jobPriority);

        List<RMJobMonitor> GetWatingAndRunningJobsWithPriorityAndSubJob();

        List<RMJobMonitor> GetJobsByJobIds(List<string> jobIds);

        RMJobMonitor GetLastestJobByLocation(string location);

        Task<(List<RMJobMonitor> Items, int TotalCount)> GetJobReportsAsync(Expression<Func<RMJobMonitor, bool>> predicate, int pageIndex, int pageSize);

        Task<bool> UpdateJobVersion(string id, JobVersion version);

        Task<bool> AnyJobAsync(Expression<Func<RMJobMonitor, bool>> predicate);
    }
}
