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
using System.Collections.Concurrent;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using ExchangeCommonWrapper;
using RAArchiverCommon.DisposalProgress.Impl;
using RAArchiverCommon.TeamsController;

namespace M365GroupTeam
{
    public class ReportCenter : IReportCenter
    {
        private readonly int _maxStorageFailedItemLimit = 2000;
        public bool IsLimitExceeded => _currentlyFailedItems.Count > _maxStorageFailedItemLimit;
        public JobType JobType => _jobType;
        public string JobId => _jobId;
        public bool HasStop { get; set; }
        public bool HasCompleteNode { get; set; }
        public bool HasErrorNode { get; set; }
        public string ErrorComment { get; set; }

        public IRMReportManager _reportManager { get; set; }

        private readonly RALogger _logger = RALogger.GetInstance(typeof(ReportCenter));
        //private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;
        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly ISyncFailureItemDao _failedObjectDao = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
        private readonly IRMBoxSyncJobProcessInfoDao _jobInfoDao = PlatformWindsorManager.GetService<IRMBoxSyncJobProcessInfoDao>();
        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private readonly IRMReportService _reportService = PlatformWindsorManager.GetService<IRMReportService>();
        private readonly IJobInfoUpdater _jobInfoUpdater = PlatformWindsorManager.GetService<IJobInfoUpdater>();
        private readonly IRMKeyValueDao _rmKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IRMGlobalKeyValueDao _globalKeyValueDao = PlatformWindsorManager.GetService<IRMGlobalKeyValueDao>();

        private readonly List<SyncFailureItemEntity> _previouslyFailedItems;
        private readonly ConcurrentQueue<SyncFailureItemEntity> _currentlyFailedItems;
        private readonly List<JMJobDetails> _holdJobReports;

        private ActionStatistics ScanActionStatistics;
        private ActionStatistics BackupActionStatistics;
        private ActionStatistics ExportActionStatistics;
        private ActionStatistics OtherActionStatistics;
        private ActionStatistics RestoreActionStatistics;
        private CompoundDisposalStatistics CompoundStatistics = null;
        private static readonly object lockObj = new object();

        private long _itemSize;
        private string _scopeId;
        private Guid _containerId;
        private JobType _jobType;
        private string _jobId;
        private NodeFlagType _nodeFlag;
        private JobStatus mJobStatus;
        private bool _isEnableMigrationImportJob = false;
        private bool _isUpdateProgressByPhase = false;

        private List<JobType> needReportJob = new List<JobType>()
        {
            // Todo: add teams report job type
            //JobType.CreateAndDestroyedFileReport,
            //JobType.ItemsFilesDueDisposal,
            //JobType.BCSTermUsageReport,
            //JobType.RetiredTermReport,
            //JobType.OrphanedTermReport,
            //JobType.SPOActionAuditReport,
            //JobType.OneDriveActionAuditReport,
            JobType.TeamsItemsFilesDueDisposalReport,
            JobType.TeamsBCSTermUsageReport,
            JobType.TeamsOrphanedTermUsageReport,
            JobType.TeamsRetiredTermUsageReport,
            JobType.TeamsCreateAndDestroyedFileReport,
        };

        private List<JobType> needProgressByPhaseJobs = 
        [
            JobType.TeamsRecordsDisposal,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
        ];

        public ReportCenter()
        {
            _previouslyFailedItems = new List<SyncFailureItemEntity>();
            _currentlyFailedItems = new ConcurrentQueue<SyncFailureItemEntity>();
            _holdJobReports = [];
            LoadMigrationPreferences();
        }

        public void LoadMigrationPreferences()
        {
            // If migration import job tenant-level setting is not set, fallback to DC-level setting for backward compatibility.
            try
            {
                var migrationJobSetting = _rmKeyValueDao.GetMigrationImportJobSetting();
                if (migrationJobSetting is not null)
                {
                    _isEnableMigrationImportJob = bool.TryParse(migrationJobSetting.Value, out var result) && result;
                }
                else if (_globalKeyValueDao is not null)
                {
                    _isEnableMigrationImportJob = _globalKeyValueDao.IsMigrationImportJobEnabled();
                }

                _logger.Info($"Migration import job enabled: {_isEnableMigrationImportJob}");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while loading migration import job setting, fallback to default value false. Error: {e}");
            }

            TeamsRestoreState.IsEnableMigrationImportJob = _isEnableMigrationImportJob;
        }

        public ReportCenter ConfigFor(string scopeId, string containerId)
        {
            _scopeId = scopeId;
            _containerId = new Guid(containerId);
            _previouslyFailedItems.Clear();
            _currentlyFailedItems.Clear();

            var failedItems = _failedObjectDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, scopeId, (int)SourceFlag.Box);
            _previouslyFailedItems.AddRange(failedItems);
            _logger.Info($"The scope [{scopeId}] has [{failedItems.Count}] failed items");

            return this;
        }

        public ReportCenter Build(JobType jobType, string jobId, NodeFlagType nodeFlag)
        {
            _jobType = jobType;
            _jobId = jobId;
            _nodeFlag = nodeFlag;
            ReportMangerFactory.Instance.Init(jobId, jobType);
            _reportManager.StartUpdateJobProgress(60);
            return this;
        }

        public JobStatus GetMainJobState()
        {
            var parentId = _jobId.Split('_')?[0];
            var parentStatus = _jobMonitorService.GetJobStatus(parentId);
            return parentStatus;
        }

        public double GetProgress(string jobId)
        {
            return _subJobDao.GetSubJob(jobId)?.Progress ?? 100;
        }

        public void SetProgress(string jobId, int x)
        {
            _reportManager.SetProgress(x);
            _jobInfoUpdater.UpdateJobProgress(jobId, x);
        }

        public ReportCenter Build(JobType jobType, string jobId, int totalPhases = 0)
        {
            _jobType = jobType;
            _jobId = jobId;
            var needReport = needReportJob.Contains(jobType);
            ReportMangerFactory.Instance.Init(jobId, jobType, needReport);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            if (needProgressByPhaseJobs.Contains(_jobType) && totalPhases > 0)
            {
                _reportManager.StartUpdateJobProgressByPhase(totalPhases);
                _isUpdateProgressByPhase = true;
                _logger.Info($"Job [{jobId}] of type [{jobType}] will update progress by phase with total phases: {totalPhases}");
            }
            else
            {
                _reportManager.StartUpdateJobProgress();
            }

            var parentId = _jobId.Split('_')?[0] ?? _jobId;
            CompoundStatistics = CompoundDisposalStatistics.Instance;
            CompoundStatistics.Init(new RAArchiverCommon.DisposalProgress.DisposalStaticInitObject()
            {
                MainJobId = parentId,
                SubJobId = jobId,
                JobType = jobType,
            });
            CompoundStatistics.StartStatistic(); // need count items not in SP ?
            return this;
        }

        public bool AdvanceToNextPhase()
        {
            if (_isUpdateProgressByPhase)
            {
                _reportManager.AdvanceToNextPhase();
                return true;
            }

            return false;
        }

        public bool DecreaseTotalPhases(int count)
        {
            if (_isUpdateProgressByPhase)
            {
                _reportManager.DecreaseTotalPhases(count);
                return true;
            }

            return false;
        }

        public void EndDisposalStatistic(string mainJobId)
        {
            CompoundStatistics?.PrepareEndStatistic();
            CompoundStatistics?.WaitEndStatistic(); // currently only count for teams
        }

        public void ResetReportManager(string jobId, bool needAddHoldReport = false)
        {
            ReportMangerFactory.Instance.Init(_reportManager);
            if (needAddHoldReport) // add hold records to report manager if site has any matching rule item.
            {
                foreach (var item in _holdJobReports)
                {
                    AddReportRecord(item);
                }
                _holdJobReports.Clear();
            }
        }

        public RMProfileDto GetReportProfile(string profileId)
        {
            return _reportService.GetProfileByIdForReportJob(profileId);
        }

        public string GetJobContent(string jobId)
        {
            var jobInfo = _subJobDao.GetSubJob(jobId, true);
            if (string.IsNullOrEmpty(jobInfo?.JobContext.Content))
            {
                throw new Exception("Can't find job context info.");
            }
            return jobInfo.JobContext.Content;
        }


        public void RecordFailed(JMJobDetails detail)
        {
            detail.Status = JobDetailsStatus.Failed;
            AnalyzeStatus(detail.Status);
            _reportManager.SendJobDetail(detail);
            if (detail is JMArchiverActionJobDetails archiveDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(archiveDetail.Level);
                AnalyzeDetailsForSummary(archiveDetail.Size, nodeType, detail.Status, (ActionTab)archiveDetail.ActionTab);
            }
        }

        public void RecordSuccessful(JMJobDetails detail)
        {
            detail.Status = JobDetailsStatus.Successful;
            AnalyzeStatus(detail.Status);
            _reportManager.SendJobDetail(detail);
            if (detail is JMArchiverActionJobDetails archiveDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(archiveDetail.Level);
                AnalyzeDetailsForSummary(archiveDetail.Size, nodeType, detail.Status, (ActionTab)archiveDetail.ActionTab);
            }
        }

        public void RecordSkip(JMJobDetails detail)
        {
            detail.Status = JobDetailsStatus.Skipped;
            AnalyzeStatus(detail.Status);
            _reportManager.SendJobDetail(detail);
            if (detail is JMArchiverActionJobDetails archiveDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(archiveDetail.Level);
                AnalyzeDetailsForSummary(archiveDetail.Size, nodeType, detail.Status, (ActionTab)archiveDetail.ActionTab);
            }
        }

        public void AddReportRecord(JMJobDetails detail, JobDetailsStatus status = JobDetailsStatus.None, bool isHold = false)
        {
            detail.Status = status != JobDetailsStatus.None ? status : detail.Status;
            if (isHold)
            {
                _holdJobReports.Add(detail);
                return;
            }
            AnalyzeStatus(detail.Status);
            detail = AdjustJobDetailStrsForSorting(detail);
            _reportManager.SendJobDetail(detail);
            if (detail is JMArchiverActionJobDetails archiveDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(archiveDetail.Level);
                AnalyzeDetailsForSummary(archiveDetail.Size, nodeType, detail.Status, (ActionTab)archiveDetail.ActionTab);
            }
        }

        // Adjusts the job detail strings for sorting purposes when get job detail.
        public JMJobDetails AdjustJobDetailStrsForSorting(JMJobDetails detail)
        {
            if (detail is JMArchiverActionJobDetails archiveDetail) // for archive job
            {
                if (archiveDetail.RuleName == "")
                {
                    archiveDetail.RuleName = null;
                }
                if (archiveDetail.Action == "")
                {
                    archiveDetail.Action = null;
                }
                if (archiveDetail.Comment == null)
                {
                    archiveDetail.Comment = "";
                }
                return archiveDetail;
            }

            return detail;
        }

        public void SendReport(BaseReport report, JMJobDetails jobDetail)
        {
            AddReportRecord(jobDetail, JobDetailsStatus.Successful);
            _reportManager.SendJobReport(report);
        }

        public DateTime GetTimePoint(string ext1)
        {
            var timePoint = _reportService.GetUtcTimePoint(ext1);
            return timePoint;
        }

        public async Task<Dictionary<Guid, RMTermIdentity>> GetTermsOfRMAsync(RMProfileDto profile, bool isOrphanedTermReport, bool isRetiredTermReport)
        {
            if (isOrphanedTermReport)
            {
                return await _reportService.GetOrphanedTermsOfRMAsync();
            }
            else if (isRetiredTermReport)
            {
                return await _reportService.GetRetiredTermsOfRMAsync();
            }
            else
            {
                return await _reportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1);
            }
        }

        public void BatchSendJobDetail(List<JMTermSelection> details)
        {
            _reportManager.BatchSendJobDetail(details);
        }

        public void CommitDisposalAnalysis()
        {
            JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (ScanActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ScanActionStatistics);
            }
            if (BackupActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(BackupActionStatistics);
            }
            if (ExportActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ExportActionStatistics);
            }
            if (OtherActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(OtherActionStatistics);
            }
            if (summaryDetails.ActionStatistics.Count > 0)
            {
                _reportManager.SendJobDetail(summaryDetails);
            }
        }

        public List<string> GetRunningJobsScopeId()
        {
            return _jobMonitorService.GetRunningJobsScopeId(_jobType);
        }

        public Tuple<long, string> GetLastRunTime()
        {
            return _jobInfoDao.GetCollectionTimeAndStreamPosition((int)_nodeFlag, _containerId, new Guid(_scopeId));
        }

        public void UpsertLastRunTime(string streamPosition)
        {
            if (_currentlyFailedItems.Count > 0)
            {
                _logger.Warn($"the current node {_scopeId} has {_currentlyFailedItems.Count} failed items. Not upsert job process time");
            }
            else
            {
                _jobInfoDao.UpsertLastJobProcessTime(streamPosition, _containerId, new Guid((_scopeId)));
                _logger.Info($"Upsert selected node: [{_scopeId}] last sync job process item.");
            }
        }

        public bool IsDataFailedInLastJob(string id)
        {
            return _previouslyFailedItems.Any(item => item.RowKey == id);
        }

        public List<SyncFailureItemEntity> GetFailedItems(string containerId,
            string ownerId, string? parentId = null)
        {
            return _previouslyFailedItems
                .Where(t => t.DataSource == (int)SourceFlag.Box && t.ContainerId == containerId
                 && t.OwnerId == ownerId && t.ParentId == parentId)
                .ToList();
        }

        public bool TryGetFailedItem(string rowKey, out SyncFailureItemEntity? failedItem)
        {
            failedItem = _previouslyFailedItems.FirstOrDefault(item => item.RowKey == rowKey);

            return failedItem != null;
        }

        public bool StorageFailedItems()
        {
            try
            {
                _failedObjectDao.RemoveAll(TenantLocalValue.LogonGroupId, _scopeId);
                return _failedObjectDao.Add(TenantLocalValue.LogonGroupId, _currentlyFailedItems.ToList());
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured while storing failed items. Error: {e}");
                return false;
            }
        }

        public void Finish(JobStatus status, string message = "")
        {
            _reportManager.SetJobFinished(status, message);
        }

        private void AnalyzeDetailsForSummary(string nodeSizeStr, StatisticsLevel cacheNodeType, JobDetailsStatus status, ActionTab actionTab)
        {
            if (!long.TryParse(nodeSizeStr, out long nodeSize))
            {
                nodeSize = 0;
            }
            switch (actionTab)
            {
                case ActionTab.Scan:
                    AnalyzeScanDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Export:
                    AnalyzeExportDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Backup:
                    AnalyzeBackUpDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Action:
                    AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeScanDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (ScanActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ScanActionStatistics == null)
                    {
                        ScanActionStatistics = new ActionStatistics();
                        ScanActionStatistics.ActionTab = (int)ActionTab.Scan;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    ScanActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(ScanActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeBackUpDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (BackupActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (BackupActionStatistics == null)
                    {
                        BackupActionStatistics = new ActionStatistics();
                        BackupActionStatistics.ActionTab = (int)ActionTab.Backup;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    BackupActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(BackupActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeExportDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (ExportActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ExportActionStatistics == null)
                    {
                        ExportActionStatistics = new ActionStatistics();
                        ExportActionStatistics.ActionTab = (int)ActionTab.Export;
                    }
                }
            }
            if (status == JobDetailsStatus.Successful)
            {
                ExportActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(ExportActionStatistics, cacheNodeType, status);
        }

        private void AnalyzeOtherDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (OtherActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (OtherActionStatistics == null)
                    {
                        OtherActionStatistics = new ActionStatistics();
                        OtherActionStatistics.ActionTab = (int)ActionTab.Action;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    OtherActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(OtherActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeStatusForSummary(ActionStatistics sta, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    AnalyzeObjCount(sta.SuccessfulObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Skipped:
                    AnalyzeObjCount(sta.SkippedObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Failed:
                    AnalyzeObjCount(sta.FailedObj, cacheNodeType);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeObjCount(ObjectStatistic objSta, StatisticsLevel cacheNodeType)
        {
            switch (cacheNodeType)
            {
                case StatisticsLevel.TeamsGroup:
                    objSta.TeamsGroupCount++;
                    break;
                case StatisticsLevel.Channel:
                    objSta.ChannelCount++;
                    break;
                case StatisticsLevel.ChannelConversation:
                    objSta.ChannelConversationCount++;
                    break;
                case StatisticsLevel.GroupMailbox:
                    objSta.GroupMailboxCount++;
                    break;
                case StatisticsLevel.GroupMailboxItem:
                    objSta.GroupMailboxItemCount++;
                    break;
                case StatisticsLevel.SiteCollection:
                    objSta.SiteCollectionCount++;
                    break;
                case StatisticsLevel.Site:
                    objSta.SiteCount++;
                    break;
                case StatisticsLevel.List:
                    objSta.ListCount++;
                    break;
                case StatisticsLevel.Folder:
                    objSta.FolderCount++;
                    break;
                case StatisticsLevel.Item:
                    objSta.ItemCount++;
                    break;
                case StatisticsLevel.Plan:
                    objSta.PlanCount++;
                    break;
                case StatisticsLevel.Task:
                    objSta.TaskCount++;
                    break;
                case StatisticsLevel.Attachment:
                    objSta.AttachmentCount++;
                    break;
                case StatisticsLevel.Exception:
                    objSta.ExceptionCount++;
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void AddRestoreReport(ReportDto detail)
        {
            try
            {
                _reportManager.Increase();
                AnalyzeStatus((JobDetailsStatus)detail.Status);

                var mArchiverActionJobDetails = _isEnableMigrationImportJob 
                    ? new JMMigrationRestoreActionJobDetailes() { StartTime = detail.StartTime } 
                    : new JMRestoreActionJobDetailes();

                mArchiverActionJobDetails.SourceLocation = detail.SourcePath;
                mArchiverActionJobDetails.Path = detail.Path;
                mArchiverActionJobDetails.Size = detail.Size.ToString();
                mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
                mArchiverActionJobDetails.Status = (JobDetailsStatus)detail.Status;
                mArchiverActionJobDetails.Level = ReportUtil.ConvertTeamsObjectLevelToI18N(detail.Type);
                mArchiverActionJobDetails.Comment = detail.ErrorMessage;
                _reportManager.SendJobDetail(mArchiverActionJobDetails);
                AnalyzeRestoreDetailsForSummary(detail.Size, ConvertI18nToStatisticsLevel(ReportUtil.ConvertTeamsObjectLevelToI18N(detail.Type)), (JobDetailsStatus)detail.Status);
            }
            catch (Exception e)
            {
                _logger.Warn(@"Looks up a localized string similar to An error occurred while adding restore report. Path: {0}, type: {1} , EX: {2}", detail.Path, detail.Type, e);
            }
        }

        private void AnalyzeRestoreDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (RestoreActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (RestoreActionStatistics == null)
                    {
                        RestoreActionStatistics = new ActionStatistics();
                        RestoreActionStatistics.ActionTab = (int)ActionTab.Restore;
                    }
                }
            }
            if (status == JobDetailsStatus.Successful)
            {
                RestoreActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(RestoreActionStatistics, cacheNodeType, status);
        }

        public void AddRestoreJobSummaryDetails()
        {
            JMRestoreSummaryDetails summaryDetails = new JMRestoreSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (RestoreActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(RestoreActionStatistics);
            }
            if (summaryDetails.ActionStatistics.Count > 0)
            {
                _reportManager.SendJobDetail(summaryDetails);
            }
        }

        private static StatisticsLevel ConvertI18nToStatisticsLevel(string i18nStr)
        {
            switch (i18nStr)
            {
                case "RM_Archiver_JobDetailTeamsGroupLevel":
                    return StatisticsLevel.TeamsGroup;
                case "RM_Archiver_JobDetailChannelLevel":
                    return StatisticsLevel.Channel;
                case "RM_Archiver_JobDetailChannelConversationLevel":
                    return StatisticsLevel.ChannelConversation;
                case "RM_Archiver_JobDetailGroupMailboxLevel":
                    return StatisticsLevel.GroupMailbox;
                case "RM_Archiver_JobDetailGroupMailboxItemLevel":
                    return StatisticsLevel.GroupMailboxItem;
                case "RM_JS_Rule_ObjectLevel_SiteCollection":
                    return StatisticsLevel.SiteCollection;
                case "RM_JS_Rule_ObjectLevel_Site":
                    return StatisticsLevel.Site;
                case "RM_JS_Rule_ObjectLevel_List":
                    return StatisticsLevel.List;
                case "RM_JS_Rule_ObjectLevel_Folder":
                    return StatisticsLevel.Folder;
                case "RM_JS_Rule_ObjectLevel_Item":
                case "RM_JS_Rule_ObjectLevel_Document":
                case "RM_JS_Rule_ObjectLevel_DocumentVersion":
                case "StorageOptimization.Gui_Attachment":
                    return StatisticsLevel.Item;
                case "RM_Archiver_JobDetailPlanLevel":
                    return StatisticsLevel.Plan;
                case "RM_Archiver_JobDetailTaskLevel":
                    return StatisticsLevel.Task;
                case "RM_JS_Rule_ObjectLevel_Attachment":
                case "RM_Archiver_JobDetailConversationLevel":
                case "RM_Archiver_JobDetailEventLevel":
                    return StatisticsLevel.GroupMailboxItem;
                case "RM_Archiver_JobDetailExceptionLevel":
                    return StatisticsLevel.Exception;
                default:
                    return StatisticsLevel.None;
            }
        }

        public void SetErrorMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                HasErrorNode = true;
            }
            ErrorComment = message;
        }

        public void Finish()
        {
            AddRestoreJobSummaryDetails();
            CommitDisposalAnalysis();
            if (string.IsNullOrEmpty(ErrorComment))
                _reportManager.SetJobFinished(GetJobStatus());
            else
                _reportManager.SetJobFinished(GetJobStatus(), ErrorComment);
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped)
            {
                HasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                HasErrorNode = true;
            }
        }

        public JobStatus GetJobStatus()
        {
            if (HasStop || CheckJobStatusUtility.isStopping)
            {
                mJobStatus = JobStatus.Stopped;
            }
            else if (HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = JobStatus.Finished;
            }
            else if (HasCompleteNode && HasErrorNode)
            {
                mJobStatus = JobStatus.FinishWithException;
            }
            else if (!HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = JobStatus.Finished;
            }
            else if (!HasCompleteNode && HasErrorNode)
            {
                mJobStatus = JobStatus.Failed;
            }
            return mJobStatus;
        }

        public void UpdateStatistics(ActionStatistics actionStatistics, ActionTab actionTab)
        {
            switch (actionTab)
            {
                case ActionTab.Scan:
                    if (ScanActionStatistics == null)
                    {
                        ScanActionStatistics = actionStatistics;
                    }
                    break;
                case ActionTab.Backup:
                    if (BackupActionStatistics == null)
                    {
                        BackupActionStatistics = actionStatistics;
                    }
                    else
                    {
                        BackupActionStatistics.Size+= actionStatistics.Size;
                        BackupActionStatistics.SuccessfulObj.GroupMailboxCount += actionStatistics.SuccessfulObj.GroupMailboxCount;
                        BackupActionStatistics.SuccessfulObj.GroupMailboxItemCount += actionStatistics.SuccessfulObj.GroupMailboxItemCount;
                        BackupActionStatistics.SuccessfulObj.GroupMailboxFolderCount += actionStatistics.SuccessfulObj.GroupMailboxFolderCount;
                        BackupActionStatistics.FailedObj.GroupMailboxCount += actionStatistics.FailedObj.GroupMailboxCount;
                        BackupActionStatistics.FailedObj.GroupMailboxItemCount += actionStatistics.FailedObj.GroupMailboxItemCount;
                        BackupActionStatistics.FailedObj.GroupMailboxFolderCount += actionStatistics.FailedObj.GroupMailboxFolderCount;
                        BackupActionStatistics.SkippedObj.GroupMailboxCount += actionStatistics.SkippedObj.GroupMailboxCount;
                        BackupActionStatistics.SkippedObj.GroupMailboxItemCount += actionStatistics.SkippedObj.GroupMailboxItemCount;
                        BackupActionStatistics.SkippedObj.GroupMailboxFolderCount += actionStatistics.SkippedObj.GroupMailboxFolderCount;
                    }
                    break;
                case ActionTab.Action:
                    if (OtherActionStatistics == null)
                    {
                        OtherActionStatistics = actionStatistics;
                    }
                    else
                    {
                        OtherActionStatistics.Size += actionStatistics.Size;
                        OtherActionStatistics.SuccessfulObj.GroupMailboxCount += actionStatistics.SuccessfulObj.GroupMailboxCount;
                        OtherActionStatistics.SuccessfulObj.GroupMailboxItemCount += actionStatistics.SuccessfulObj.GroupMailboxItemCount;
                        OtherActionStatistics.SuccessfulObj.GroupMailboxFolderCount += actionStatistics.SuccessfulObj.GroupMailboxFolderCount;
                        OtherActionStatistics.FailedObj.GroupMailboxCount += actionStatistics.FailedObj.GroupMailboxCount;
                        OtherActionStatistics.FailedObj.GroupMailboxItemCount += actionStatistics.FailedObj.GroupMailboxItemCount;
                        OtherActionStatistics.FailedObj.GroupMailboxFolderCount += actionStatistics.FailedObj.GroupMailboxFolderCount;
                        OtherActionStatistics.SkippedObj.GroupMailboxCount += actionStatistics.SkippedObj.GroupMailboxCount;
                        OtherActionStatistics.SkippedObj.GroupMailboxItemCount += actionStatistics.SkippedObj.GroupMailboxItemCount;
                        OtherActionStatistics.SkippedObj.GroupMailboxFolderCount += actionStatistics.SkippedObj.GroupMailboxFolderCount;
                    }
                    break;
            }
        }

        public void StopJob()
        {
            _logger.Error("Job stopped!!!");
            HasStop = true;
        }
    }
}
