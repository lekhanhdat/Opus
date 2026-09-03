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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RABox.Util;
using System.Collections.Concurrent;

namespace RABox
{
    public class ReportCenter
    {
        private readonly int _maxStorageFailedItemLimit = 2000;
        public bool IsLimitExceeded => _currentlyFailedItems.Count > _maxStorageFailedItemLimit;
        public JobType JobType => _jobType;
        public string JobId => _jobId;
        public bool JobHasStopped { get; set; }

        private readonly RALogger _logger = RALogger.GetInstance(typeof(ReportCenter));
        private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;
        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly ISyncFailureItemDao _failedObjectDao = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
        private readonly IRMBoxSyncJobProcessInfoDao _jobInfoDao = PlatformWindsorManager.GetService<IRMBoxSyncJobProcessInfoDao>();
        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private readonly IRMReportService _reportService = PlatformWindsorManager.GetService<IRMReportService>();
        private readonly IJobInfoUpdater _jobInfoUpdater = PlatformWindsorManager.GetService<IJobInfoUpdater>();

        private readonly List<SyncFailureItemEntity> _previouslyFailedItems;
        private readonly ConcurrentQueue<SyncFailureItemEntity> _currentlyFailedItems;
        private readonly ObjectStatistic _successfulObj;
        private readonly ObjectStatistic _failedObj;
        private readonly ObjectStatistic _skippedObj;
        private readonly object _lockSuccessfulObj = new object();
        private readonly object _lockFailedObj = new object();
        private readonly object _lockSkippedObj = new object();

        private long _itemSize;
        private string _scopeId;
        private Guid _containerId;
        private JobType _jobType;
        private string _jobId;
        private NodeFlagType _nodeFlag;


        public ReportCenter()
        {
            _previouslyFailedItems = new List<SyncFailureItemEntity>();
            _currentlyFailedItems = new ConcurrentQueue<SyncFailureItemEntity>();
            _successfulObj = new ObjectStatistic();
            _failedObj = new ObjectStatistic();
            _skippedObj = new ObjectStatistic();
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

        public ReportCenter Build(JobType jobType, string jobId)
        {
            _jobType = jobType;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            _reportManager.StartUpdateJobProgress(60);
            return this;
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

        public void RecordFailedCommon(SyncFailureItemEntity item, string comment)
        {
            var detail = new JMBoxDataSyncDetail
            {
                ObjectName = item.FullPath.Substring(item.FullPath.LastIndexOf('\\') + 1),
                FullPath = item.FullPath,
                ItemType = item.IsDirectory ? I18NResource.DataTypeBoxFolder : I18NResource.ObjectLevelDocument,
                Comment = comment,
            };

            var nodeType = item.IsDirectory ? (int)RMNodeLevel.BoxFolder : (int)RMNodeLevel.BoxFile;
            RecordFailed(detail, nodeType);
            _itemSize += detail.FileSize;

            if (IsLimitExceeded)
            {
                _logger.Warn($"Failed storage capacity has been exceeded. Skip storing.");
                return;
            }

            _currentlyFailedItems.Enqueue(item);
        }

        public void RecordFailedCommon(JMJobDetailsCommon detail, SyncFailureItemEntity? entity = null)
        {
            var nodeType = entity?.IsDirectory == true ? (int)RMNodeLevel.BoxFolder : (int)RMNodeLevel.BoxFile;

            RecordFailed(detail, nodeType);
            _itemSize += detail.FileSize;

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

        public void RecordFailedCommon(JMJobDetailsCommon detail, int nodeType)
        {
            RecordFailed(detail, nodeType);
            _itemSize += detail.FileSize;
        }

        public void RecordSuccessfulCommon(JMJobDetailsCommon detail, int nodeType)
        {
            RecordSuccessful(detail, nodeType);
            _itemSize += detail.FileSize;
        }

        public void RecordSkipCommon(JMJobDetailsCommon detail, int nodeType)
        {
            RecordSkip(detail, nodeType);
            _itemSize += detail.FileSize;
        }

        public void RecordFailed(JMJobDetails detail, int nodeType)
        {
            detail.Status = JobDetailsStatus.Failed;
            IncreaseObjCount(nodeType, _failedObj, _lockFailedObj);
            _reportManager.SendJobDetail(detail);
        }

        public void RecordSuccessful(JMJobDetails detail, int nodeType)
        {
            detail.Status = JobDetailsStatus.Successful;
            IncreaseObjCount(nodeType, _successfulObj, _lockSuccessfulObj);
            _reportManager.SendJobDetail(detail);
        }

        public void RecordSkip(JMJobDetails detail, int nodeType)
        {
            detail.Status = JobDetailsStatus.Skipped;
            IncreaseObjCount(nodeType, _skippedObj, _lockSkippedObj);
            _reportManager.SendJobDetail(detail);
        }

        private void IncreaseObjCount(int nodeType, ObjectStatistic obj, object lockObj)
        {
            if (nodeType == (int)RMNodeLevel.BoxFile)
            {
                lock (lockObj)
                {
                    obj.FileCount++;
                }
                return;
            }

            if (nodeType == (int)RMNodeLevel.BoxFolder)
            {
                lock (lockObj)
                {
                    obj.FolderCount++;
                }
                return;
            }

            if (nodeType == (int)RMNodeLevel.BoxUser)
            {
                lock (lockObj)
                {
                    obj.UserCount++;
                }
                return;
            }
        }

        public void SendReport(BaseReport report, JMJobDetails jobDetail)
        {
            RecordSuccessful(jobDetail, report.ObjectLevel);
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
            ActionStatistics statistic = new ActionStatistics();
            statistic.Size = _itemSize;

            statistic.SuccessfulObj = _successfulObj;
            statistic.FailedObj = _failedObj;
            statistic.SkippedObj = _skippedObj;

            JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (statistic != null)
            {
                summaryDetails.ActionStatistics.Add(statistic);
            }
            _reportManager.SendJobDetail(summaryDetails);
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

        public void Completed()
        {
            var jobFinishStatus = (_successfulObj.BoxTotalCount > 0 || _skippedObj.BoxTotalCount > 0) && _failedObj.BoxTotalCount > 0 ?
                JobStatus.FinishWithException :
                (
                    _failedObj.BoxTotalCount > 0 ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
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
            _reportManager.SetJobFinished(jobFinishStatus);
        }

        public void SetJobFinish(JobStatus jobStatus, string comment = "")
        {
            _reportManager.SetJobFinished(jobStatus, comment);
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
    }
}