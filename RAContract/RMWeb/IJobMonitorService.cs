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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using DocAveOnline.WebApi.Contracts;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IJobMonitorService
    {
        string GenerateJobId(JobType jobType);
        Task<JMPageResult> GetJobsListAsync(JMPager pager);
        Task<string> GetJobsDataAsync(JMPager pager);
        Task<string> GetJobsDataForDisposalAsync(string recoJobid);
        Task<bool> HasRunningFSSyncDataJobAsync(string connectionId);

        Task<JMItemInfo> GetJobAsync(string id);
        Task<AOSPJMItemInfo> GetAOSPJobAsync(string id);
        Task<AOSPJMItemInfo> GetAOSPJobAsync(string id, Guid o365TenantId);
        Task<JMItemInfo> GetJobForRecenterAsync(string id);
        JobStatus GetJobStatus(string id);
        Task<List<JMItemInfo>> GetJobsAsync(List<string> idArray);
        Task<List<JMItemInfo>> GetJobsForRecenterAsync(List<string> idArray);
        Task<List<JMItemInfo>> GetEndedJobByScopeIdAsync(string scopeId, int[] status, int[] securityGroupId);
        Task<List<JMItemInfo>> GetEndedJobByScopeIdAsync(string scopeId, int[] status, string userId);
        Task<JMJobSummary> GetJobSummaryAsync(string id);
        Task<JMJobSetting> GetJobSettingAsync(string id,int type);
        Task<JMJobDetails> GetSOJobSummaryDetailsAsync(string id);
        Task<JMJobDetails> GetRestoreJobSummaryDetailsAsync(string id);
        Task<JMJobSummary> GetDAOJobSummaryDetailsAsync(string id, int type);
        Task<string> GetFilterListAsync(string filterName);

        List<BaseJobDto> GetJobsByJobType(JobType jobType);
        BaseJobDto GetLastestJobByJobType(JobType jobType);
        Task<int> DeleteJobsAsync(List<string> idArray);

        Task<int> DeleteJobByTypes(List<JobType> jobTypes);

        Task<int> DeleteJobsForAgentAsync(List<string> idArray);
        System.Threading.Tasks.Task DeleteOldOfflineSearchJobAsync(string scopeId, string exceptId);
        Task<int> DeleteJobsByProfileIdsAsync(List<int> proflieIds);

        bool UpdateJobProgress(string id, int progress);

        int GetJobProgress(string id);

        /// <summary>
        /// 此方法用于避免因数据量较大无法及时更新job进度，导致的job 超时失败。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> UpdateJobWithoutProgressChangeAsync(string id);

        bool UpdateJobStatus(string id, JobStatus status);

        bool UpdateJobStatus(string id, JobStatus status, string message);
        bool UpdateJobExtension(string id, ArchiveJobMonitorExtension extension);
        bool UpdateJobExtensionById(string id, string extension);
        bool AtomicityUpdateJobExtension(string jobId, string oldJobExtension, string newJobExtension);

        [Obsolete("使用新的CreateJob方法，需要jobRunBy的参数")]
        string CreateJob(JobType jobType);

        string CreateJob(JobType jobType, string jobRunBy, string containerId = null, string scopedId = null, string fullPath = null);

        string CreateJobWithJobId(string jobId, JobType jobType, string jobRunBy);

        Task<string> CreateDiscoveryJobAsync(string jobRunBy, Guid mainJobId, Guid discoveryJobId);

        Task<string> CreateDiscoveryJobNextVersionAsync(string jobRunBy, Guid mainJobId, JobType type);

        Task<string> CreateDiscoveryRetryJobAsync(string jobRunBy, Guid mainJobId, Guid discoveryJobId);

        string CreateJobWithScopeIdForTeams(JobType jobType, string jobRunBy, string scopeId, string additionalInformation, string containerId = null, string fullPath = null, string jobConflictExtension = null);
        string CreateJobWithScopeId(JobType jobType, string jobRunBy, string scopeId, string containerId = null,string fullPath = null, string jobConflictExtension = null);
        string CreateJobWithScopeId(string jobId ,JobType jobType, string jobRunBy, string scopeId, string containerId = null,string fullPath = null, string jobConflictExtension = null);
        string CreateJobWithScopeId(JobType jobType, JobStatus jobStatus, string jobRunBy, string scopeId, string containerId = null, string failedReason = null);

        string CreateJobWithProfileId(JobType jobType, string jobRunBy, int profileId, string userId = null, int subJobCount = 0);

        string GetJobIdByJobTypeExceptCurrent(JobType jobType, string currentId);

        string GetJobIdByJobTypeExceptCurrent(JobType jobType, string currentId, string scopeId);
        string CreateJobWithScopeIdForRecenter(JobType jobType, string jobRunBy, string scopeId, string jobid, int nodeType,string realRunJobUser,string containerId = null);

        (string, long) GetLastFinishedJob(JobType jobType);
        List<string> GetRunningJobs(JobType jobType);
        List<BaseJobDto> GetRunningJobs(List<JobType> jobTypes, string scopeId);

        List<BaseJobDto> GetRunningJobsBatch(List<JobType> jobTypes, List<string> scopeIds);

        List<BaseJobDto> GetRunningJobs(List<JobType> jobTypes);
        List<string> GetRunningJobsScopeId(JobType jobType);

        //List<string> GetRunningJobs(JobType jobType, string scopeId);

        List<int?> GetRunningJobsByProfileIds(List<int> profileIds);

        Task<string> GetJobDetailsAsync(JMDetailsQuery queryModel, bool isGettingMainJobDetails = false);
        Task<JMAOSPDetailsResult> GetAOSPJobDetailsAsync(JMDetailsQuery queryModel);
        HSMArchvierJobDetailsResult GetHSMJobFailedDetails(JMDetailsQuery queryModel);

        Task<JMDetailsResult> GetJobProgress(JMProgressDetailsQuery queryModel);

        string GetTermSelection(string jobId);

        Task<(List<KeyValuePair<string, string>>,bool)> GetJobByProfileIdAsync(int profileId, bool onlyFinishedJob = false);

        string GetProfileNameById(int id);
        List<BaseJobDto> GetJobDtoByProfileIds(List<int> ids);
        int DelJobReportFiles(List<BaseJobDto> jobInfos);

        void CheckAndDisposeTimeoutJob();

        List<string> GetRunningSharePointSettingJob();
        List<string> GetRunningTeamsSettingJob();
        List<string> GetRunningSharePointOnPremiseSettingJob();
        List<string> GetRunningEXOApplySettingJob();
        int GetRunningJobsCount(JobType jobType);
        int GetRunningJobsCount(List<JobType> jobTypes);
        int StopJobs(List<string> idArray);
        Task<JMJobSummary> GetDisposalJobSummaryAsync(string jobid);
        Task<JMJobSummary> GetDisposalJobSummaryAsync(SOJob soJob);

        string GetJobValidateKey(string id);

        string GetJobExtension(string id);

        void UpdateJob(string id, int progress, int status, long endTime, string comment = null);

        string GetJobFakeidByKey(string key);

        List<SOJob> ValidateJobs(List<string> keys);

        List<SOJob> GetJobByRECOID(string recoJobId);

        List<SOJob> GetSOJobsByIds(List<string> jobIds);

        void UpdateArchiverJob(SOJob job, string recoJobId);
        List<string> GetRunningUniqueIDSettingJob();
        List<string> GetRunningSPOnPremUniqueIDSettingJob();
        Task<string> RunExportDisposalJobAsync(string exportJobId, string jobRunByUser);
        Task<RAReturnMessage> SaveExportSettingsAsync(JobExportSettingDto setting);
        Task<string> GetExportSettingsAsync(bool loadExportLocation);
        void StartExportJob(string exportJobId);

        bool SetSumSCCountOfJobExtension(int sumCount, string jobId);
        List<string> GetCollectionDataSettingJobs();
        ///add for sub job update progress and detail.

        bool UpdateSubJobStatus(string id, JobStatus status, string message);
        bool UpdateSubJobProgress(string id, int progress);

        bool UpdateMigrationJobStatus(string id, JobStatus status, string message, ArchiverMigrationJobStatus migrationJobStatus);

        bool UpdateMigrationJobAdditionalInformation(string id, ArchiverMigrationJobStatus migrationJobStatus);

        List<string> GetRunningSetPermissionJob(string exceptJobId);

        List<string> GetRunningSyncSecurityContainerJob();

        bool CheckHasRunningManualJob();

        bool CheckCurrentUserHasRunningJob(string containerId,string jobId);

        bool CheckStoppedJobByDiscoveryJobId(Guid mainJobId);

        JobType GetJobType(string id);

        #region Sub Job Operation
        List<RMSubJobDto> GetRunnableSubJob();

        string GetJobContextSettingByJobId(string jobId);

        List<string> GetRunningMoveJobByDestUrl(string destUrl);
        bool UpdateRunable(string id, int runnable, bool updateState = false);
        /// <summary>
        /// get the running and runnable sub job count.
        /// </summary>
        /// <returns></returns>
        Dictionary<JobType, int> GetRunningAndRunnableSubJobCount();
        /// <summary>
        /// change a number of sub jobs' status from SubJob_Runnable_Waiting to SubJob_Runnable_CanRun
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="count"></param>
        void ChangeRunnableWiating2CanRun(JobType jobType, int count);

        Task<List<SubJobsResult>> GetSubJobsAsync(COPSubJobRequest request);

        #endregion

        #region archvier
        List<string> GetRunningArchiverJobsScopes(List<JobType> types);
        public bool HasRunningArchiverJobOnScope(List<JobType> types, string scope);
        List<string> FilterRunnableSOJobSitesInContainerForImportedSites(string containerId, List<string> siteUrls);
        List<string> GetRunningArchiverJobOnScope(List<JobType> types, string scope);
        List<string> GetRunningArchiverJobSiteUrl(IEnumerable<JobType> types, IEnumerable<string> siteCollectionUrls, bool includeTeamsExtra = false);
        List<string> GetRunningTeamsArchiverJobSiteUrl(IEnumerable<JobType> types, IEnumerable<string> siteCollectionUrls);

        Task<List<string>> GetRunningDriveNodeIds(List<JobType> types);

        HashSet<string> GetRunningArchiverJobs();

        Dictionary<string, List<string>> GetRunningTeamsArchiverJobSiteUrl(List<JobType> types, bool needLoadSiteUrl, Dictionary<string, List<string>> filterTeamAncSCDic, string skipCurrentJobId = "");

        public bool HasStoppingArchiverJobOnScope(List<JobType> types, string scope);
        #endregion
        #region
        Task<ArchiverExportJobDetailInfo> RecenterJobDetailsAsync(string jobId);
        #endregion


        System.Threading.Tasks.Task ClearOldArchiverJobsAsync();

        System.Threading.Tasks.Task BulkMigrateJobsAsync(IEnumerable<ArchiverMigrationJobDto> jobs);

        System.Threading.Tasks.Task BulkMigrateArchiverJobs(IEnumerable<ArchiverMigrationJobDto> jobs);

        Task<int> UpdateMigratedJobsInfoAsync();

        Task<int> DeleteMigratedArchiverJobsAsync();

        Task<int> DeleteMigratedMainJobsAsync();

        string GetMigrationJobReportExcelBlobName(string jobId);
        System.Threading.Tasks.Task UploadMigrationJobReportToStorageBlob(string jobId);

        Task<string> RealRunDownloadJobReportJob(string param);
        Task<string> RealRunDownloadJobReportJobForCOP(string param);
        string CreateJobWithScopeIdAndJobId(string jobId, JobType jobType, string jobRunBy, string scopeId, string containerId = null, string fullPath = null, string jobConflictExtension = null);
        List<string> GetTeamsRunningUniqueIDSettingJob();

        // progress updates, and job stop checks; returns total moved rows.
        Task<int> ArchiveJobRecordsAsync(string jobId);
        Task<bool> UpdateJobPriorityAsync(List<string> jobIds, JobPriority jobPriority);

        Task<bool> UpdateJobVersionAsync(string jobId, JobVersion jobVersion);

        HSMArchiverJobInfo GetHSMArchiverJobInfo(string location);
        JobMonitorStatisticsDto GetJobMonitorStatisDto(string mainJobId);
    }
}
