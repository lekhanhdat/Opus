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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RAGoogle.Archive;
using RAGoogle.Util;
using System.Collections.Concurrent;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;

namespace RAGoogle.Report;

public class ReportCenter
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(ReportCenter));
    private BaseJobDto _baseJobDto;
    private readonly int _maxStorageFailedItemLimit = 2000;
    public JobType JobType => _jobType;
    private JobStatus JobStatus => _jobStatus;
    public string JobId => _jobId;
    public bool IsLimitExceeded => _currentlyFailedItems.Count > _maxStorageFailedItemLimit;
    private IJobInfoUpdater _jobInfoUpdater;
    protected IJobInfoUpdater JobInfoUpdater
    {
        get
        {
            if (_jobInfoUpdater == null)
            {
                _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            }
            return _jobInfoUpdater;
        }
    }
    private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;
    private readonly IRMReportService _reportService = PlatformWindsorManager.GetService<IRMReportService>();
    protected readonly IRMGoogleProcessInfoDao _jobInfoDao = PlatformWindsorManager.GetService<IRMGoogleProcessInfoDao>();
    private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
    private readonly ISyncFailureItemDao _failedObjectDao = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
    private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
    private readonly List<SyncFailureItemEntity> _previouslyFailedItems;
    public List<JMJobDetails> JobDetailsCache;
    private readonly ConcurrentQueue<SyncFailureItemEntity> _currentlyFailedItems;
    private int _currentlyFailedItemCount;
    private int _currentlySuccessfulItemCount;
    private int _currentlySkipItemCount;
    private ActionStatistics BackupActionStatistics;
    private ActionStatistics ScanActionStatistics;
    private ActionStatistics OtherActionStatistics;
    private ActionStatistics ExportActionStatistics;
    private ActionStatistics RestoreActionStatistics;
    private readonly ObjectStatistic _successfulObj;
    private readonly ObjectStatistic _failedObj;
    private readonly ObjectStatistic _skippedObj;
    private readonly object _lockSuccessfulObj = new object();
    private readonly object _lockFailedObj = new object();
    private readonly object _lockSkippedObj = new object();
    private Guid _containerId;
    private Guid _scopeId;
    private string _jobId;
    private long _itemSize;
    private List<int> _nodeLevels = null;
    private bool _isShowItemsDetail = false;
    private ConcurrentDictionary<string, (bool, string)> permissionDestinationDrive = new();

    public bool HasCompleteNode { get; set; }
    public bool HasScanCompleteNode { get; set; }
    public bool HasErrorNode { get; set; }
    public bool JobHasStopped { get; set; }
    public string SummaryComments { get; set; }
    public IRMReportManager ReportManager { get => _reportManager; }

    private JobType _jobType;
    private JobStatus _jobStatus;

    public ReportCenter()
    {
        _previouslyFailedItems = new List<SyncFailureItemEntity>();
        _currentlyFailedItems = new ConcurrentQueue<SyncFailureItemEntity>();
        _successfulObj = new ObjectStatistic();
        _failedObj = new ObjectStatistic();
        _skippedObj = new ObjectStatistic();
    }

    public ReportCenter Init(string scopeId, string containerId, bool isShowItemsDetail)
    {
        _scopeId = new Guid(scopeId);
        _containerId = new Guid(containerId);
        _previouslyFailedItems.Clear();
        _currentlyFailedItems.Clear();
        _currentlyFailedItemCount = 0;
        _currentlySuccessfulItemCount = 0;
        _currentlySkipItemCount = 0;
        _isShowItemsDetail = isShowItemsDetail;

        var failedItems = _failedObjectDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, scopeId, (int)SourceFlag.Google);
        _previouslyFailedItems.AddRange(failedItems);
        _logger.Info($"The scope [{scopeId}] has [{failedItems.Count}] failed items");

        return this;
    }

    public ReportCenter AssignAccessLevel(List<int> levels)
    {
        _nodeLevels = levels;
        return this;
    }

    public bool CheckPermissionForDestinationDrive(string destinationDriveId)
    {
        return permissionDestinationDrive.TryGetValue(destinationDriveId, out _);
    }

    public void AssignPermissionForDestinationDrive(bool isAssignedPermission, string memberEmail, string destinationDriveId)
    {
        permissionDestinationDrive.TryAdd(destinationDriveId, (isAssignedPermission, memberEmail));
    }

    public IDictionary<string, (bool, string)> GetPermissionInfoInDestinationDrive()
    {
        return permissionDestinationDrive;
    }


    #region Job detail cache
    public void InitJobDetailsCache()
    {
        if (JobDetailsCache == null)
        {
            JobDetailsCache = new();
        }
        JobDetailsCache.Clear();
    }

    public void AddJobDetail(JMJobDetails detail, int nodeType = (int)RMNodeLevel.GoogleFile, SyncFailureItemEntity? entity = null)
    {
        if (JobDetailsCache == null)
        {
            InitJobDetailsCache();
        }

        switch (detail.Status)
        {
            case JobDetailsStatus.Successful:
                IncreaseObjCount(nodeType, _successfulObj, _lockSuccessfulObj);
                break;
            case JobDetailsStatus.Skipped:
                IncreaseObjCount(nodeType, _skippedObj, _lockSkippedObj);
                break;
            case JobDetailsStatus.Failed:
                IncreaseObjCount(nodeType, _failedObj, _lockFailedObj);
                _currentlyFailedItemCount++;
                if (entity != null)
                {
                    if (IsLimitExceeded)
                    {
                        _logger.Warn($"Failed storage capacity has been exceeded. Skip storing.");
                        return;
                    }

                    _currentlyFailedItems.Enqueue(entity);
                }
                break;
        }

        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        JobDetailsCache.Add(detail);
    }

    public void AddJobDetails(IEnumerable<JMJobDetails> details, int nodeType, SyncFailureItemEntity? entity = null)
    {
        if (JobDetailsCache == null)
        {
            InitJobDetailsCache();
        }

        foreach (var detail in details)
        {
            AddJobDetail(detail, nodeType, entity);
        }
    }

    public void CommitJobDetails(bool isSort = false)
    {
        if (JobDetailsCache.IsNotNullOrEmpty())
        {
            var firstEl = JobDetailsCache.FirstOrDefault();
            if (isSort && firstEl is JMTermSyncJobDetails)
            {
                var jobDetails = JobDetailsCache.Select(x => x as JMTermSyncJobDetails)?.OrderBy(l => l?.Term)?.ToList();
                _reportManager.BatchSendJobDetail(jobDetails);
                return;
            }
            else if (isSort && firstEl is JMImportTermDetail)
            {
                var jobDetails = JobDetailsCache.Select(x => x as JMImportTermDetail)?.OrderBy(l => l?.Term)?.ToList();
                _reportManager.BatchSendJobDetail(jobDetails);
                return;
            }

            _reportManager.BatchSendJobDetail(JobDetailsCache);
        }
    }
    #endregion

    #region Job info operator
    public void InitCurrentJobInfo(string jobId, JobType jobType)
    {
        _jobId = jobId;
        _jobType = jobType;
        _baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)jobType };
        JobInfoUpdater.UpdateJobState(jobId, (int)JobStatus.InProgress);
        JobInfoUpdater.UpdateJobProgress(jobId, 1);
        ReportMangerFactory.Instance.Init(jobId, jobType, true);
        _reportManager.StartUpdateJobProgress();
    }

    public RMSubJob GetSubJobInfo(string jobId, bool withContext = false)
    {
        if (!string.IsNullOrEmpty(jobId))
        {
            return _subJobDao.GetSubJob(jobId, withContext);
        }

        return new RMSubJob();
    }
    #endregion

    #region Last run time operator
    public long GetLastRunTime()
    {
        int type = (int)NodeFlagType.GoogleSync;
        switch (_jobType)
        {
            case JobType.GoogleDataSynchronization:
                type = (int)NodeFlagType.GoogleSync;
                break;
            case JobType.GoogleApplySettings:
                type = (int)NodeFlagType.GoogleApplySetting;
                break;
        }
        return _jobInfoDao.GetCollectionTime(type, _containerId, _scopeId);
    }

    public void UpsertLastRunTime(long lastRunTime)
    {
        int type = (int)NodeFlagType.GoogleSync;
        switch (_jobType)
        {
            case JobType.GoogleDataSynchronization:
                type = (int)NodeFlagType.GoogleSync;
                break;
            case JobType.GoogleApplySettings:
                type = (int)NodeFlagType.GoogleApplySetting;
                break;
            default:
                _logger.Error($"Invalid job type. Auto matched to sync job");
                break;
        }
        _jobInfoDao.UpsertLastJobProcessTime(lastRunTime, _containerId, _scopeId, type);
        _logger.Info($"Upsert selected node: [{_scopeId}] , last process time :{lastRunTime} of: {(NodeFlagType)type}.");
    }
    #endregion

    #region Job monitor progress
    public double GetProgress(string jobId)
    {
        return _subJobDao.GetSubJob(jobId)?.Progress ?? 100;
    }

    public void SetProgress(string jobId, int x)
    {
        _reportManager.SetProgress(x);
        _jobInfoUpdater.UpdateJobProgress(jobId, x);
    }

    public void IncreaseBaseProgress(long value)
    {
        _reportManager.IncreaseBase(value);
    }

    public JobStatus GetMainJobState()
    {
        var parentId = _jobId.Split('_')?[0];
        var parentStatus = _jobMonitorService.GetJobStatus(parentId);
        return parentStatus;
    }

    public void SetJobFinish(JobStatus jobStatus, string comment = "")
    {
        _logger.Info($"Job finished with status: [{jobStatus}], comment: [{comment}]");
        if (jobStatus == JobStatus.Stopped)
        {
            _reportManager.SetJobFinished(jobStatus);
            return;
        }

        if (jobStatus != JobStatus.Finished && string.IsNullOrEmpty(comment))
        {
            comment = "RM_TS_SS_Summary";
        }
        _reportManager.SetJobFinished(jobStatus, comment);
    }

    public JobStatus Completed(string comment = "", bool setJobFinish = true)
    {
        var jobFinishStatus = (_successfulObj.DriveTotalCount > 0 || _skippedObj.DriveTotalCount > 0) && _failedObj.DriveTotalCount > 0 ?
            JobStatus.FinishWithException :

                _failedObj.DriveTotalCount > 0 ?
                JobStatus.Failed :
                JobStatus.Finished
            ;
        if (JobHasStopped)
        {
            var parentId = _jobId.Split('_')?[0];
            var runningSubjobIds = _subJobDao.GetAllSubJobIds(parentId, [(int)JobStatus.InProgress]);
            _logger.Info($"Those [{string.Join(',', runningSubjobIds)}] subjobs are running");
            foreach (var id in runningSubjobIds)
            {
                if (id.Equals(_jobId))
                {
                    _subJobDao.UpdateStatus(_jobId, (int)JobStatus.Stopping, DateTime.UtcNow.Ticks);
                    continue;
                }
                _subJobDao.UpdateStatus(id, (int)JobStatus.Stopped, DateTime.UtcNow.Ticks);
                _logger.Info($"Stopping subjob [{id}] at {DateTime.UtcNow.Ticks}");
            }
            jobFinishStatus = JobStatus.Stopped;
        }
        if (setJobFinish) SetJobFinish(jobFinishStatus, comment);
        return jobFinishStatus;
    }

    public JobDetailsStatus CalculateJobDetails()
    {
        return
        (_currentlySuccessfulItemCount > 0 || _currentlySkipItemCount > 0) && _currentlyFailedItemCount > 0 ?
        JobDetailsStatus.Exception : _currentlyFailedItemCount > 0 ?
        JobDetailsStatus.Failed : JobDetailsStatus.Successful;
    }
    #endregion

    #region Report operator
    public RMProfileDto GetReportProfile(string profileId)
    {
        return _reportService.GetProfileByIdForReportJob(profileId);
    }

    public DateTime GetTimePoint(string extension1)
    {
        var timePoint = _reportService.GetUtcTimePoint(extension1);
        return timePoint;
    }

    public void SendReport(BaseReport report, JMJobDetails jobDetail)
    {
        RecordSuccessful(jobDetail, report.ObjectLevel);
        _reportManager.SendJobReport(report);
    }
    #endregion

    #region Record job detail
    public void RecordSuccessful(JMJobDetails detail, int nodeType)
    {
        detail.Status = JobDetailsStatus.Successful;
        IncreaseObjCount(nodeType, _successfulObj, _lockSuccessfulObj);
        _currentlySuccessfulItemCount++;
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        _reportManager.SendJobDetail(detail);
    }
    public void RecordSuccessfulBulk(IEnumerable<JMJobDetails> details, int nodeType)
    {
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        details.ForEach(detail => detail.Status = JobDetailsStatus.Successful);
        _reportManager.BatchSendJobDetail(details);
    }

    public void RecordFailedBulk(IEnumerable<JMJobDetails> details, int nodeType)
    {
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        details.ForEach(detail => detail.Status = JobDetailsStatus.Failed);
        _reportManager.BatchSendJobDetail(details);
    }

    public void RecordSkipBulk(IEnumerable<JMJobDetails> details, int nodeType)
    {
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        details.ForEach(detail => detail.Status = JobDetailsStatus.Skipped);
        _reportManager.BatchSendJobDetail(details);
    }

    public void RecordFailed(JMJobDetails detail, int nodeType, SyncFailureItemEntity? entity = null)
    {
        detail.Status = JobDetailsStatus.Failed;
        IncreaseObjCount(nodeType, _failedObj, _lockFailedObj);
        _currentlyFailedItemCount++;
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType))
        {
            return;
        }
        _reportManager.SendJobDetail(detail);

        if (entity != null)
        {
            if (IsLimitExceeded)
            {
                _logger.Warn($"Failed storage capacity has been exceeded. Skip storing.");
                return;
            }

            _currentlyFailedItems.Enqueue(entity);
        }

    }

    public void RecordSuccessfulCommon(JMJobDetailsCommon detail, int nodeType)
    {
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        RecordSuccessful(detail, nodeType);
        _itemSize += detail.FileSize;
    }

    public void RecordFailedCommon(JMJobDetailsCommon detail, int nodeType)
    {
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        RecordFailed(detail, nodeType);
        _itemSize += detail.FileSize;
    }

    public void RecordFailed(JMJobDetails detail, SyncFailureItemEntity? entity = null)
    {
        var nodeType = entity?.IsDirectory == true ? (int)RMNodeLevel.GoogleFolder : (int)RMNodeLevel.GoogleFile;
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }

        RecordFailed(detail, nodeType);

        if (entity != null)
        {
            if (IsLimitExceeded)
            {
                _logger.Warn($"Failed storage capacity has been exceeded. Skip storing.");
                return;
            }

            _currentlyFailedItems.Enqueue(entity);
        }
    }

    public void RecordSkipCommon(JMJobDetailsCommon detail, int nodeType)
    {
        RecordSkip(detail, nodeType);
        _itemSize += detail.FileSize;
    }

    public void RecordSkip(JMJobDetails detail, int nodeType)
    {
        detail.Status = JobDetailsStatus.Skipped;
        IncreaseObjCount(nodeType, _skippedObj, _lockSkippedObj);
        _currentlySkipItemCount++;
        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType))
        {
            return;
        }
        _reportManager.SendJobDetail(detail);
    }

    public void SendJobReport(JMJobDetails detail)
    {
        _reportManager.SendJobDetail(detail);
    }

    public void RecordCommon(JMJobDetails detail, int nodeType, SyncFailureItemEntity? entity = null)
    {
        if (detail.Status == JobDetailsStatus.None)
        {
            detail.Status = JobDetailsStatus.Successful;
        }

        switch (detail.Status)
        {
            case JobDetailsStatus.Successful:
                IncreaseObjCount(nodeType, _successfulObj, _lockSuccessfulObj);
                break;
            case JobDetailsStatus.Skipped:
                IncreaseObjCount(nodeType, _skippedObj, _lockSkippedObj);
                break;
            case JobDetailsStatus.Exception:
                IncreaseObjCount(nodeType, _failedObj, _lockFailedObj);
                _currentlyFailedItemCount++;
                break;
            case JobDetailsStatus.Failed:
                IncreaseObjCount(nodeType, _failedObj, _lockFailedObj);
                _currentlyFailedItemCount++;
                if (entity != null)
                {
                    if (IsLimitExceeded)
                    {
                        _logger.Warn($"Failed storage capacity has been exceeded. Skip storing.");
                        return;
                    }

                    _currentlyFailedItems.Enqueue(entity);
                }
                break;
        }

        if (_nodeLevels.IsNotNullOrEmpty() && !_nodeLevels.Contains(nodeType) && !_isShowItemsDetail)
        {
            return;
        }
        _reportManager.SendJobDetail(detail);
    }

    public void CommitDisposalAnalysis()
    {
        if (ScanActionStatistics != null || BackupActionStatistics != null || OtherActionStatistics != null || ExportActionStatistics != null )
        {
            FinishReport(SummaryComments);
            return;
        }
        ActionStatistics statistic = new ActionStatistics();
        statistic.Size = _itemSize;

        statistic.SuccessfulObj = _successfulObj;
        statistic.FailedObj = _failedObj;
        statistic.SkippedObj = _skippedObj;

        JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
        summaryDetails.ActionStatistics = new List<ActionStatistics>();
        if (statistic.SuccessfulObj.DriveTotalCount > 0 || statistic.SkippedObj.DriveTotalCount > 0 || statistic.FailedObj.DriveTotalCount > 0)
        {
            summaryDetails.ActionStatistics.Add(statistic);
        }
        _reportManager.SendJobDetail(summaryDetails);
    }

    private void IncreaseObjCount(int nodeType, ObjectStatistic obj, object lockObj)
    {
        if (nodeType == (int)RMNodeLevel.GoogleDrive || nodeType == (int)NodeLevel.GoogleSharedDrive || nodeType == (int)NodeLevel.GoogleMyDrive)
        {
            lock (lockObj)
            {
                obj.DriveCount++;
            }
            return;
        }
        else if (nodeType == (int)RMNodeLevel.GoogleFolder)
        {
            lock (lockObj)
            {
                obj.FolderCount++;
            }
            return;
        }
        else if (nodeType == (int)RMNodeLevel.GoogleFile)
        {
            lock (lockObj)
            {
                obj.ItemCount++;
            }
            return;
        }
    }
    #endregion

    #region generate job detail

    public JMJobDetailsCommon GenerateCommonJobDetail(JobType jobType, GoogleDriveTreeNodeDto node, JobDetailsStatus status, string comment = "")
    {
        switch (jobType)
        {
            case JobType.GoogleApplySettings:
                return new JMGoogleJobDetails()
                {
                    ObjectName = node.Name,
                    FullPath = node.FullPath,
                    Action = I18NResource.GoogleApplySettings,
                    Status = status,
                    ItemType = I18NResource.ObjectLevelDrive,
                    Comment = comment
                };
            case JobType.GoogleRecordsDisposal:
                return new JMArchiverActionJobDetails()
                {
                    SourceLocation = node.FullPath,
                    ActionTab = (int)ActionTab.Action,
                    Status = status,
                    Comment = comment
                };
            default:
                return new JMJobDetailsCommon();
        }
    }

    public static JMJobDetailsCommon GenerateDeletedDriveJobDetail(JobType jobType, GoogleDriveTreeNodeDto node, string comment = "")
    {
        switch (jobType)
        {
            case JobType.GoogleApplySettings:
                return new JMGoogleJobDetails()
                {
                    ObjectName = node.Name,
                    FullPath = node.FullPath,
                    Action = I18NResource.GoogleApplySettings,
                    Status = JobDetailsStatus.Failed,
                    ItemType = I18NResource.ObjectLevelDrive,
                    Comment = comment
                };
            case JobType.GoogleRecordsDisposal:
                return new JMArchiverActionJobDetails()
                {
                    ActionTab = (int)ActionTab.Scan,
                    Level = I18NResource.ObjectLevelGoogleDrive,
                    SourceLocation = node.FullPath,
                    Status = JobDetailsStatus.Failed,
                    FinishTime = DateTime.UtcNow.Ticks,
                    Comment = comment
                };
            default:
                return new JMJobDetailsCommon();
        }
    }

    public JMJobDetails GenerateJobDetailForGoogleSyncContent(JobType jobType, GoogleDriveTreeNodeDto node, JobDetailsStatus status, string comment = "")
    {

        return new JMGoogleDataSyncJobDetails()
        {
            ObjectName = node.Name,
            FullPath = node.FullPath,
            Status = status,
            ItemType = I18NEntity.GetString("RM_JS_SPS_TabLabel_Google"),
            Comment = comment
        };

    }
    public void AddBackupReport(JMArchiverActionJobDetails mArchiverActionJobDetails, string level)
    {
        try
        {
            AnalyzeStatus(mArchiverActionJobDetails.Status, (int)ActionTab.Backup);
            _reportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeBackUpDetailsForSummary(long.Parse(mArchiverActionJobDetails.Size), level, mArchiverActionJobDetails.Status);
        }
        catch (Exception e)
        {
            _logger.Warn($"An error occurred when add backup report {e.ToString()}");
        }
    }

    public void AddDeletionReport(JMArchiverActionJobDetails mArchiverActionJobDetails, string level)
    {
        try
        {
            AnalyzeStatus(mArchiverActionJobDetails.Status, (int)ActionTab.Action);
            _reportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeOtherDetailsForSummary(long.Parse(mArchiverActionJobDetails.Size), level, mArchiverActionJobDetails.Status);
        }
        catch (Exception e)
        {
            _logger.Warn($"An error occurred when add backup report {e.ToString()}");
        }
    }
    
    public void AddExportReport(JMArchiverActionJobDetails mArchiverActionJobDetails, string level)
    {
        try
        {
            AnalyzeStatus(mArchiverActionJobDetails.Status, (int)ActionTab.Export);
            _reportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeExportDetailsForSummary(long.Parse(mArchiverActionJobDetails.Size), level, mArchiverActionJobDetails.Status);
        }
        catch (Exception e)
        {
            _logger.Warn($"An error occurred when add backup report {e.ToString()}");
        }
    }

    public void AddScanReport(JMArchiverActionJobDetails mArchiverActionJobDetails, string level)
    {
        try
        {
            AnalyzeStatus(mArchiverActionJobDetails.Status, (int)ActionTab.Scan);
            _reportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeScanDetailsForSummary(long.Parse(mArchiverActionJobDetails.Size), level, mArchiverActionJobDetails.Status);
        }
        catch (Exception e)
        {
            _logger.Warn($"An error occurred when add scan report {e.ToString()}");
        }
    }

    public void AddGoogleDriveRestoreReport(string driveId, string sourcePath, long nodeSize, string path, int status, string level, string message = "")
    {
        AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);
        JMGDriveRestoreActionJobDetail mArchiverActionJobDetails = new JMGDriveRestoreActionJobDetail();
        mArchiverActionJobDetails.DriveId = driveId;
        mArchiverActionJobDetails.SourceLocation = sourcePath;
        mArchiverActionJobDetails.Size = nodeSize.ToString();
        mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
        mArchiverActionJobDetails.Status = (JobDetailsStatus)status;
        mArchiverActionJobDetails.Level = level;
        mArchiverActionJobDetails.Comment = message;
        mArchiverActionJobDetails.Path = path;
        _reportManager.SendJobDetail(mArchiverActionJobDetails);
        AnalyzeRestoreDetailsForSummary(nodeSize, level, (JobDetailsStatus)status);
    }

    public JMReportJobDetails GenerateCommonReportJobDetail(GoogleDriveTreeNodeDto treeNode, string comment = "")
    {
        return new JMReportJobDetails()
        {
            TitleOrName = treeNode.Name,
            Type = I18NResource.ObjectLevelDrive,
            Url = treeNode.FullPath,
            Comment = comment
        };
    }

    #endregion

    #region Failed items operator
    public List<SyncFailureItemEntity> GetFailedItems(string containerId, string scopeId)
    {
        return _previouslyFailedItems
            .Where(t => t.DataSource == (int)SourceFlag.Google && t.PartitionKey == scopeId)
            .ToList();
    }
    public bool StorageFailedItems()
    {
        try
        {
            _failedObjectDao.RemoveAll(TenantLocalValue.LogonGroupId, _scopeId.ToString());
            return _failedObjectDao.Add(TenantLocalValue.LogonGroupId, _currentlyFailedItems.ToList());
        }
        catch (Exception e)
        {
            _logger.Error($"An error occured while storing failed items. Error: {e}");
            return false;
        }
    }
    #endregion

    public string ConvertGoogleNodeLevelToI18n(int googleCacheNodeType)
    {
        string I18nStr = "";
        if (googleCacheNodeType == (int)GoogleCacheNodeType.Drive)
        {
            I18nStr = "RM_JS_Common_ReportType_GoogleDrive";
        }
        else if (googleCacheNodeType == (int)GoogleCacheNodeType.Folder)
        {
            I18nStr = "RM_JS_Rule_ObjectLevel_GoogleFolder";
        }
        else if (googleCacheNodeType == (int)GoogleCacheNodeType.Item)
        {
            I18nStr = "RM_JS_Rule_ObjectLevel_GoogleFile";
        }
        else if (googleCacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
        {
            I18nStr = "RM_JS_Rule_ObjectLevel_ItemVersion";
        }

        return I18nStr;
    }

    private void AnalyzeBackUpDetailsForSummary(long nodeSize, string level, JobDetailsStatus status)
    {
        if (BackupActionStatistics == null)
        {
            lock (_lockSuccessfulObj)
            {
                if (BackupActionStatistics == null)
                {
                    BackupActionStatistics = new ActionStatistics();
                    BackupActionStatistics.ActionTab = (int)ActionTab.Backup;
                }
            }
        }
        lock (_lockSuccessfulObj)
        {
            if (status == JobDetailsStatus.Successful)
            {
                BackupActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(BackupActionStatistics, level, status);
        }
    }

    public void AnalyzeOtherDetailsForSummary(long size, string level, JobDetailsStatus status)
    {
        AnalyzeStatus(status, (int)ActionTab.Action);
        if (OtherActionStatistics == null)
        {
            lock (_lockSuccessfulObj)
            {
                if (OtherActionStatistics == null)
                {
                    OtherActionStatistics = new ActionStatistics();
                    OtherActionStatistics.ActionTab = (int)ActionTab.Action;
                }
            }
        }
        lock (_lockSuccessfulObj)
        {
            if (status == JobDetailsStatus.Successful)
            {
                OtherActionStatistics.Size += size;
            }
            AnalyzeStatusForSummary(OtherActionStatistics, level, status);
        }
    }
    
    public void AnalyzeExportDetailsForSummary(long size, string level, JobDetailsStatus status)
    {
        AnalyzeStatus(status, (int)ActionTab.Export);
        if (ExportActionStatistics == null)
        {
            lock (_lockSuccessfulObj)
            {
                if (ExportActionStatistics == null)
                {
                    ExportActionStatistics = new ActionStatistics();
                    ExportActionStatistics.ActionTab = (int)ActionTab.Export;
                }
            }
        }
        lock (_lockSuccessfulObj)
        {
            if (status == JobDetailsStatus.Successful)
            {
                ExportActionStatistics.Size += size;
            }
            AnalyzeStatusForSummary(ExportActionStatistics, level, status);
        }
    }

    private void AnalyzeScanDetailsForSummary(long nodeSize, string level, JobDetailsStatus status)
    {
        if (ScanActionStatistics == null)
        {
            lock (_lockSuccessfulObj)
            {
                if (ScanActionStatistics == null)
                {
                    ScanActionStatistics = new ActionStatistics();
                    ScanActionStatistics.ActionTab = (int)ActionTab.Scan;
                }
            }
        }
        lock (_lockSuccessfulObj)
        {
            if (status == JobDetailsStatus.Successful)
            {
                ScanActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(ScanActionStatistics, level, status);
        }
    }

    private void AnalyzeRestoreDetailsForSummary(long size, string level, JobDetailsStatus status)
    {
        if (RestoreActionStatistics == null)
        {
            lock (_lockSuccessfulObj)
            {
                if (RestoreActionStatistics == null)
                {
                    RestoreActionStatistics = new ActionStatistics();
                    RestoreActionStatistics.ActionTab = (int)ActionTab.Restore;
                }
            }
        }
        lock (_lockSuccessfulObj)
        {
            if (status == JobDetailsStatus.Successful)
            {
                RestoreActionStatistics.Size += size;
            }
            AnalyzeStatusForSummary(RestoreActionStatistics, level, status);
        }
    }

    private void AnalyzeStatusForSummary(ActionStatistics sta, string level, JobDetailsStatus status)
    {
        switch (status)
        {
            case JobDetailsStatus.Successful:
                AnalyzeObjCount(sta.SuccessfulObj, level);
                break;
            case JobDetailsStatus.Skipped:
                AnalyzeObjCount(sta.SkippedObj, level);
                break;
            case JobDetailsStatus.Failed:
                AnalyzeObjCount(sta.FailedObj, level);
                break;
            default:
                break;
        }
    }

    private void AnalyzeObjCount(ObjectStatistic objSta, string level)
    {
        if (level == I18NResource.ObjectLevelFile)
        {
            objSta.ItemCount++;
        }
        else if (level == I18NResource.ObjectLevelFolder)
        {
            objSta.FolderCount++;
        }
        else if (level == I18NResource.ObjectLevelGoogleDrive)
        {
            objSta.DriveCount++;
        }
    }

    private void AnalyzeStatus(JobDetailsStatus status, int actionTab)
    {
        if ((status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped))
        {
            if (actionTab == (int)ActionTab.Scan)
            {
                HasScanCompleteNode = true;
            }
            else
            {
                HasCompleteNode = true;
            }
        }
        else if (status == JobDetailsStatus.Failed)
        {
            HasErrorNode = true;
        }
    }

    public JobStatus GetJobStatus()
    {
        if (JobHasStopped)
        {
            _jobStatus = JobStatus.Stopped;
        }
        else if (HasCompleteNode && !HasErrorNode)
        {
            _jobStatus = JobStatus.Finished;
        }
        else if (HasCompleteNode && HasErrorNode)
        {
            _jobStatus = JobStatus.FinishWithException;
        }
        else if (!HasCompleteNode && !HasErrorNode)
        {
            _jobStatus = JobStatus.Finished;
        }
        else if (!HasCompleteNode && HasErrorNode)
        {
            _jobStatus = JobStatus.Failed;
        }
        return _jobStatus;
    }

    public void FinishGoogleDriveRestoreReport()
    {
        AddRestoreJobSummaryDetails();
        _reportManager.SetJobFinished(GetJobStatus(), string.IsNullOrEmpty(SummaryComments) ? string.Empty : SummaryComments );
    }

    public void FinishReport(string message)
    {
        AddJobSummaryDetails();
        _reportManager.SetJobFinished(GetJobStatus(), message);
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

    public void AddJobSummaryDetails()
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
        if (OtherActionStatistics != null)
        {
            OtherActionStatistics.DeleteSize = OtherActionStatistics.Size;
            summaryDetails.ActionStatistics.Add(OtherActionStatistics);
        }
        if (ExportActionStatistics != null)
        {
            summaryDetails.ActionStatistics.Add(ExportActionStatistics);
        }
        if (summaryDetails.ActionStatistics.Count > 0)
        {
            _reportManager.SendJobDetail(summaryDetails);
        }
    }
    public void AddGenerateRestoreReport(string driveName, JobDetailsStatus status, string message = "")
    {
        AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);
        JMRestoreReportJobDetailes mRestoreReportJobDetails = new JMRestoreReportJobDetailes();
        mRestoreReportJobDetails.Status = status;
        mRestoreReportJobDetails.Comment = message;
        mRestoreReportJobDetails.Url = driveName;
        mRestoreReportJobDetails.Title = driveName;
        mRestoreReportJobDetails.Level = I18NResource.ObjectLevelGoogleDrive;
        _reportManager.SendJobDetail(mRestoreReportJobDetails);
    }
}
