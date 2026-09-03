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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.UniqueId;
using Microsoft.Azure.Cosmos;
using RABox.Converters;
using RABox.Util;
using System.Net;
using ActionStatus = AvePoint.RA.Contract.Schedule.ActionStatus;

namespace RABox
{
    public class RecordManager
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(RecordManager));

        private readonly IExplorerDao _explorerDao;

        private readonly ICosmosBulkOperator _cosmosOperator = CosmosBulkOperator.Instance;

        private ReportCenter _reportCenter;

        private SourceFlag _sourceFlag;

        private int _sourceFlagValue => (int)_sourceFlag;

        private bool _isEnableBulkOperation;

        private int _bulkSize;

        private bool _useCache = false;

        private BoxTreeNode _currentTopNode;

        private Dictionary<Guid, Record> RecordsCache = new Dictionary<Guid, Record>();

        private List<Record> NewRecordsCache = new List<Record>();

        private const int LIMIT = 1000;

        private long recordCount = 0;

        private static readonly object _lockObject = new object();

        private readonly long _jobStartTime = DateTime.UtcNow.Ticks;

        private string _reportOwnerId; // for report jobs

        public RecordManager()
        {
            _explorerDao = new ExplorerDao(true);
        }

        public void ClearCache()
        {
            if (_useCache)
            {
                RecordsCache.Clear();
            }
        }

        public RecordManager Build(ReportCenter reportCenter, SourceFlag sourceFlag)
        {
            _reportCenter = reportCenter;
            _sourceFlag = sourceFlag;
            var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _isEnableBulkOperation = keyValueDao.IsCosmosBulkOperationEnabled();
            _logger.Info($"Start checking current tenant [{_isEnableBulkOperation}] is enable bulk operation?");
            if (!_isEnableBulkOperation)
            {
                return this;
            }
            _bulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (_bulkSize <= 0)
            {
                _bulkSize = CosmosBulkOperator.DefualtBufferSize;
            }

            return this;
        }

        public RecordManager Config()
        {
            _cosmosOperator.Start(_bulkSize, SucceedProcessRecord, FailedProcessRecord);
            _logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{_bulkSize}].");
            _logger.Info($"Succeed build data synchronize record manager.");
            return this;
        }

        public RecordManager Config(string ownerId)
        {
            _reportOwnerId = ownerId;
            return this;
        }

        public bool IsLoadedCache(BoxTreeNode currentTopNode, bool useCache = false)
        {
            if (!useCache)
            {
                _currentTopNode = currentTopNode;
            }

            return _useCache = useCache;
        }

        public void LoadCache()
        {

            var continuation = string.Empty;
            var limit = 1000;
            do
            {
                var result = _explorerDao.QueryByPage(item => item.SourceFlag == _sourceFlagValue && item.ContainerId == _currentTopNode.ConnectionId &&
                        item.AveSiteId == _currentTopNode.OwnerId && item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active, limit, continuation, false);
                continuation = result.Item2;
                foreach (var item in result.Item1)
                {
                    RecordsCache.Add(item.Id, item);
                }
            } while (!string.IsNullOrEmpty(continuation));
        }

        public void LoadRuleActionCache()
        {

            var continuation = string.Empty;
            var limit = 1000;
            do
            {
                var result = _explorerDao.QueryByPage(item => item.SourceFlag == _sourceFlagValue && item.ContainerId == _currentTopNode.ConnectionId &&
                        item.AveSiteId == _currentTopNode.OwnerId && item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active &&
                        (item.NodeType == (int)RMNodeLevel.BoxFolder || (item.NodeType == (int)RMNodeLevel.BoxFile && item.RuleId != Guid.Empty)), limit, continuation, false);
                continuation = result.Item2;
                foreach (var item in result.Item1)
                {
                    RecordsCache.Add(item.Id, item);
                }
            } while (!string.IsNullOrEmpty(continuation));
        }

        public bool TryGetRecordValue(Guid id, int createDate, out Record record)
        {
            if (_useCache)
            {
                RecordsCache.TryGetValue(id, out record);
                return record != null;
            }

            record = Retry(() => _explorerDao.GetFirstOrDefault(item =>
                    item.Id == id && item.SourceFlag == _sourceFlagValue && item.ContainerId == _currentTopNode.ConnectionId && item.CreateDate == createDate &&
                    item.AveSiteId == _currentTopNode.OwnerId && item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active));
            return record != null;
        }

        public bool TryGetRecordByAncestor(Guid id, int createDate, out Record record)
        {
            if (_useCache)
            {
                RecordsCache.TryGetValue(id, out record);
                return record != null;
            }

            record = Retry(() => _explorerDao.GetFirstOrDefault(item =>
                    item.Id == id && item.SourceFlag == _sourceFlagValue && item.ContainerId == _currentTopNode.ConnectionId && item.CreateDate == createDate &&
                    (_currentTopNode.Level == RMNodeLevel.BoxUser || item.Ancestors.Contains(new Guid(_currentTopNode.Id))) &&
                    item.AveSiteId == _currentTopNode.OwnerId && item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active));
            return record != null;
        }

        public bool TryGetRuleActionRecordValue(Guid id, int createDate, out Record record)
        {
            if (_useCache)
            {
                RecordsCache.TryGetValue(id, out record);
                return record != null;
            }

            record = Retry(() => _explorerDao.GetFirstOrDefault(item =>
                    item.Id == id && item.SourceFlag == _sourceFlagValue && item.ContainerId == _currentTopNode.ConnectionId && item.CreateDate == createDate &&
                    item.AveSiteId == _currentTopNode.OwnerId && item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active &&
                    item.RecordStatus == (int)RMRecordStatus.Active && (item.NodeType == (int)RMNodeLevel.BoxFolder || (item.NodeType == (int)RMNodeLevel.BoxFile && item.RuleId != Guid.Empty))));
            return record != null;
        }

        public List<Record> GetRuleActionRecordsByParent(Guid parentId)
        {
            if (_useCache)
            {
                return RecordsCache.Values.Where(record => record.ParentId == parentId).ToList();
            }

            return Retry(() => _explorerDao.QueryAll(item => item.ParentId == parentId && item.SourceFlag == _sourceFlagValue &&
                    item.ContainerId == _currentTopNode.ConnectionId && item.AveSiteId == _currentTopNode.OwnerId &&
                    item.CollectTime < _jobStartTime && item.RecordStatus == (int)RMRecordStatus.Active &&
                    (item.NodeType == (int)RMNodeLevel.BoxFolder || (item.NodeType == (int)RMNodeLevel.BoxFile && item.RuleId != Guid.Empty))))
                .ToList();
        }

        public List<Record> GetRecordsByParentId(Guid parentId)
        {
            if (_useCache)
            {
                return RecordsCache.Values.Where(record => record.ParentId == parentId).ToList();
            }

            return Retry(() => _explorerDao.QueryAll(record =>
                    record.ParentId == parentId && record.SourceFlag == _sourceFlagValue && record.ContainerId == _currentTopNode.ConnectionId &&
                    record.AveSiteId == _currentTopNode.OwnerId && record.CollectTime < _jobStartTime && record.RecordStatus == (int)RMRecordStatus.Active))
                .ToList();
        }

        public IEnumerable<List<Record>> GetRecordsByTermIds(Dictionary<Guid, long> changedTerms, long jobStartTime)
        {
            var limit = 1000;
            if (_useCache)
            {
                var records = RecordsCache.Values
                                .Where(record => changedTerms.Keys.Contains(record.TermId) && record.ManualCollectionTime < changedTerms[record.TermId])
                                .ToList();
                for (int i = 0; i < records.Count; i += limit)
                {
                    yield return records.Skip(i).Take(limit).ToList();
                }
            }
            else
            {
                var continuation = string.Empty;

                do
                {
                    var result = Retry(() => _explorerDao.QueryByPage(item =>
                        item.SourceFlag == _sourceFlagValue &&
                        item.CollectTime < jobStartTime &&
                        item.RecordStatus == (int)RMRecordStatus.Active &&
                        changedTerms.Keys.Contains(item.TermId)
                        , limit, continuation, false));
                    continuation = result.Item2;
                    yield return result.Item1.Where(item => item.ManualCollectionTime < changedTerms[item.TermId] && item.CollectTime < changedTerms[item.TermId]).ToList();
                } while (!string.IsNullOrEmpty(continuation));
            }
        }

        public List<Record> GetRecordsByIds(List<Guid> ids)
        {
            return Retry(() => _explorerDao.GetRecordByIds(ids));
        }

        public bool IsRecordsHold(List<Guid> ids, long ticks)
        {
            return Retry(() => _explorerDao.IsRecordsHold(ids, ticks));
        }

        public Tuple<IEnumerable<Record>, string> QueryFileRecordsByParent(Guid folderId, string pageIndex, RMRecordStatus recordStatus)
        {
            return Retry(() => _explorerDao.QueryByPage(o =>
                o.SourceFlag == _sourceFlagValue
                && o.RecordStatus != (int)recordStatus
                && o.ParentId == folderId
                && o.AveSiteId == _reportOwnerId
                && o.NodeType == (int)RMNodeLevel.BoxFile, LIMIT, pageIndex));
        }

        public Tuple<IEnumerable<Record>, string> QueryFolderRecordsByAncestor(Guid ancestorId, string pageIndex)
        {
            return Retry(() => _explorerDao.QueryByPage(o =>
                o.SourceFlag == _sourceFlagValue
                && o.Ancestors.Contains(ancestorId)
                && o.AveSiteId == _reportOwnerId
                && o.NodeType == (int)RMNodeLevel.BoxFolder, LIMIT, pageIndex));
        }

        public Record GetBoxRecordById(Guid id)
        {
            return Retry(() => _explorerDao.GetBoxRecordById(id));
        }

        public void Add(Record item)
        {
            if (!_isEnableBulkOperation)
            {
                if (item.RecordsId == null)
                {
                    item.RecordsId = new UniqueIdUtil(TenantLocalValue.LogonGroupId, 1).GenerateUniqueId();
                }
                _explorerDao.Upsert(item);
                _reportCenter.RecordSuccessfulCommon(item.GenerateSyncActionDetail(), item.NodeType);
                return;
            }

            if (item.RecordsId != null)
            {
                _cosmosOperator.Add(item);
            }

            lock (_lockObject)
            {
                recordCount++;
                if (NewRecordsCache.Count >= 1000 || recordCount >= 1000)
                {
                    _logger.Info($"NewRecordsCache Count [{NewRecordsCache.Count}], recordCount [{recordCount}] when Add. Start Commit");
                    Commit();
                }

                if (item.RecordsId == null)
                {
                    NewRecordsCache.Add(item);
                }
            }
        }

        public void Delete(Record item)
        {
            _explorerDao.Delete(item.CreateDate, item.Id);
        }

        public void UpdateRecordStatusAndDestroyedTime(Record item, Rule rule, int updateStatus)
        {
            if (rule.BoxRule.IsManualApproval)
            {
                Retry(() => _explorerDao.UpdateAll(s => s.ContainerId == item.ContainerId && s.CreateDate == item.CreateDate && s.Id == item.Id, r => { r.RecordStatus = updateStatus; r.DestroyedTime = DateTime.UtcNow.Ticks; r.ManualArchiveStatus = (int)ActionStatus.Archiverd; }));
            }
            else
            {
                Retry(() => _explorerDao.UpdateAll(s => s.ContainerId == item.ContainerId && s.CreateDate == item.CreateDate && s.Id == item.Id, r => { r.RecordStatus = updateStatus; r.DestroyedTime = DateTime.UtcNow.Ticks; }));
            }
            _reportCenter.RecordSuccessfulCommon(item.GenerateDisposalActionJobDetail(I18NEntity.GetString(I18NResource.RemoveAndDestroyAction), rule.Name, string.Empty), item.NodeType);
        }

        // update all the properties can affect the rule (dynamic criteria): name, size, path, modifiedAt. (createdAt and type cannot changed)
        public void UpdateToNewRuleInfo(Record record)
        {
            Retry(() => _explorerDao.UpdateAll(s => s.ContainerId == record.ContainerId && s.CreateDate == record.CreateDate && s.Id == record.Id, r =>
            {
                r.RuleId = record.RuleId;
                r.LeafName = record.LeafName;
                r.TimeModified = record.TimeModified;
                r.ModifiedBy = record.ModifiedBy;
                r.DirPath = record.DirPath;
                r.Ancestors = record.Ancestors;
                r.MetaInfo = record.MetaInfo;
            }));
        }

        public void UpdateManualProperties(Record record, bool isChangedRule = false)
        {
            if (isChangedRule)
            {
                Retry(() => _explorerDao.UpdateAll(s => s.ContainerId == record.ContainerId && s.CreateDate == record.CreateDate && s.Id == record.Id, r =>
                {
                    r.ManualModifiedTime = record.TimeModified;
                    r.IsManualSynced = record.IsManualSynced;
                    r.ManualActionTime = record.ManualActionTime;
                    r.ManualApprovedBy = record.ManualApprovedBy;
                    r.ManualApprovedStatus = record.ManualApprovedStatus;
                    r.ManualArchivedTime = record.ManualArchivedTime;
                    r.ManualArchiveStatus = record.ManualArchiveStatus;
                    r.ManualCollectionTime = record.ManualCollectionTime;
                    r.ManualEmailNotificationCount = record.ManualEmailNotificationCount;
                    r.ManualEmailNotificationLastTime = record.ManualEmailNotificationLastTime;
                    r.ManualEscalatedComment = record.ManualEscalatedComment;
                    r.ManualEscalateFrom = record.ManualEscalateFrom;
                    r.ManualExtendComment = record.ManualExtendComment;
                    r.ManualExtendCount = record.ManualExtendCount;
                    r.ManualExtendTime = record.ManualExtendTime;
                    r.ManualLastApproveRejectComment = record.ManualLastApproveRejectComment;
                    r.ManualLastReviewedBy = record.ManualLastReviewedBy;
                    r.ManualLastlReviewTime = record.ManualLastlReviewTime;
                    r.ManualFullPath = record.ManualFullPath;
                    r.ManualFolderPath = record.ManualFolderPath;
                    r.ManualLastReasonForRejection = record.ManualLastReasonForRejection;
                    r.ManualInternalApprovedStatus = record.ManualInternalApprovedStatus;
                    r.ManualIsAutoReassigned = record.ManualIsAutoReassigned;
                    r.ManualNeedEmailNotification = record.ManualNeedEmailNotification;
                    r.ManualReviewer = record.ManualReviewer;
                    r.ManualRowKey = record.ManualRowKey;
                    r.ManualRuleCriteria = record.ManualRuleCriteria;
                    r.ManualRuleDisposalClass = record.ManualRuleDisposalClass;
                    r.ManualRuleName = record.ManualRuleName;
                    r.ManualWorkflowDefinitionId = record.ManualWorkflowDefinitionId;
                    r.ManualWorkflowInstanceId = record.ManualWorkflowInstanceId;
                    r.ManualWorkflowStepId = record.ManualWorkflowStepId;
                    r.RuleId = record.RuleId;
                    r.LeafName = record.LeafName;
                    r.TimeModified = record.TimeModified;
                    r.ModifiedBy = record.ModifiedBy;
                    r.DirPath = record.DirPath;
                    r.Ancestors = record.Ancestors;
                    r.MetaInfo = record.MetaInfo;
                    r.ManualLastReviewedBy = record.ManualLastReviewedBy;
                    r.ManualLastlReviewTime = record.ManualLastlReviewTime;
                    r.ManualLastApproveRejectComment = r.ManualLastApproveRejectComment;
                    r.ManualDisposalDueDate = record.ManualDisposalDueDate;
                }));
                return;
            }
            Retry(() => _explorerDao.UpdateAll(s => s.ContainerId == record.ContainerId && s.CreateDate == record.CreateDate && s.Id == record.Id, r =>
            {
                r.ManualModifiedTime = record.TimeModified;
                r.IsManualSynced = record.IsManualSynced;
                r.ManualActionTime = record.ManualActionTime;
                r.ManualApprovedBy = record.ManualApprovedBy;
                r.ManualApprovedStatus = record.ManualApprovedStatus;
                r.ManualArchivedTime = record.ManualArchivedTime;
                r.ManualArchiveStatus = record.ManualArchiveStatus;
                r.ManualCollectionTime = record.ManualCollectionTime;
                r.ManualEmailNotificationCount = record.ManualEmailNotificationCount;
                r.ManualEmailNotificationLastTime = record.ManualEmailNotificationLastTime;
                r.ManualEscalatedComment = record.ManualEscalatedComment;
                r.ManualEscalateFrom = record.ManualEscalateFrom;
                r.ManualExtendComment = record.ManualExtendComment;
                r.ManualExtendCount = record.ManualExtendCount;
                r.ManualExtendTime = record.ManualExtendTime;
                r.ManualLastApproveRejectComment = record.ManualLastApproveRejectComment;
                r.ManualLastReviewedBy = record.ManualLastReviewedBy;
                r.ManualLastlReviewTime = record.ManualLastlReviewTime;
                r.ManualFullPath = record.ManualFullPath;
                r.ManualFolderPath = record.ManualFolderPath;
                r.ManualLastReasonForRejection = record.ManualLastReasonForRejection;
                r.ManualInternalApprovedStatus = record.ManualInternalApprovedStatus;
                r.ManualIsAutoReassigned = record.ManualIsAutoReassigned;
                r.ManualNeedEmailNotification = record.ManualNeedEmailNotification;
                r.ManualReviewer = record.ManualReviewer;
                r.ManualRowKey = record.ManualRowKey;
                r.ManualRuleCriteria = record.ManualRuleCriteria;
                r.ManualRuleDisposalClass = record.ManualRuleDisposalClass;
                r.ManualRuleName = record.ManualRuleName;
                r.ManualWorkflowDefinitionId = record.ManualWorkflowDefinitionId;
                r.ManualWorkflowInstanceId = record.ManualWorkflowInstanceId;
                r.ManualWorkflowStepId = record.ManualWorkflowStepId;
                r.RuleId = record.RuleId;
                r.ManualLastReviewedBy = record.ManualLastReviewedBy;
                r.ManualLastlReviewTime = record.ManualLastlReviewTime;
                r.ManualLastApproveRejectComment = r.ManualLastApproveRejectComment;
                r.ManualDisposalDueDate = record.ManualDisposalDueDate;
            }));
        }

        public void Commit()
        {
            if (!_isEnableBulkOperation)
            {
                return;
            }

            _logger.Info($"Start cosmos db bulk operator commit.");
            lock (_lockObject)
            {
                var newCount = NewRecordsCache.Count;
                _logger.Info($"new added records count is {newCount} when Commit.{(newCount == 0 ? "" : " Start generating UniqueId and clear cache")}");
                if (newCount != 0)
                {
                    var recordUniqueIds = new UniqueIdUtil(TenantLocalValue.LogonGroupId, newCount);
                    foreach (var newRecord in NewRecordsCache)
                    {
                        newRecord.RecordsId = recordUniqueIds.GenerateUniqueId();
                        _cosmosOperator.Add(newRecord);
                    }
                    NewRecordsCache.Clear();
                }
                recordCount = 0;
            }
            _cosmosOperator.Complete();
            _cosmosOperator.Reset();
            _cosmosOperator.Start(_bulkSize, SucceedProcessRecord, FailedProcessRecord);

            _logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{_bulkSize}].");
            _logger.Info($"End cosmos db bulk operator commit.");
        }

        public void WaitComplete()
        {
            if (!_isEnableBulkOperation)
            {
                return;
            }

            try
            {
                lock (_lockObject)
                {
                    var newCount = NewRecordsCache.Count;
                    _logger.Info($"new added records count is {newCount} when WaitComplete.{(newCount == 0 ? "" : " Start generating UniqueId and clear cache")}");
                    if (newCount != 0)
                    {
                        var recordUniqueIds = new UniqueIdUtil(TenantLocalValue.LogonGroupId, newCount);
                        foreach (var newRecord in NewRecordsCache)
                        {
                            newRecord.RecordsId = recordUniqueIds.GenerateUniqueId();
                            _cosmosOperator.Add(newRecord);
                        }
                        NewRecordsCache.Clear();
                    }
                    recordCount = 0;
                }
                _logger.Info($"Waiting cosmos db bulk operator job complete.");
                _cosmosOperator.Complete();
                _logger.Info($"The cosmos db bulk operator job complete.");
            }

            catch (Exception ex)
            {
                _logger.Error($"An error occurred while completing the bulk operation. error: {ex.ToString()}");
                throw;
            }
        }

        private async Task SucceedProcessRecord(Record item)
        {
            if (_reportCenter.JobType == JobType.BoxRecordsDisposal)
            {
                if (item.RecordStatus == (int)RMRecordStatus.Destroyed)
                {
                    _reportCenter.RecordSuccessfulCommon(item.GenerateSyncActionDetail(), item.NodeType);
                }
            }
            else
            {
                _reportCenter.RecordSuccessfulCommon(item.GenerateSyncActionDetail(), item.NodeType);
            }
        }

        private void FailedProcessRecord(Record item, Exception e)
        {
            _logger.Error($"The item [{item.Id}] sync to cosmos db failed, Error: {e}");
            _reportCenter.RecordFailedCommon(item.GenerateSyncActionDetail(e.Message), item.GenerateFailureItemEntity(_reportCenter.JobId));
        }

        private T Retry<T>(Func<T> func)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter += 1;
                    return func();
                }
                catch (Exception ex)
                {
                    if (counter > 5)
                    {
                        _logger.Error($"Retry failed many times. Retry count:{counter}, ex:{ex}");
                        throw;
                    }

                    if (ex is CosmosException cosmosEx)
                    {
                        if (cosmosEx.StatusCode == HttpStatusCode.TooManyRequests
                            || cosmosEx.StatusCode == HttpStatusCode.ServiceUnavailable
                            || cosmosEx.Message.Contains("Request rate is large"))
                        {
                            _logger.Info($"Because of 429 error. Retry after {1000 * counter} ms. Retry count: {counter}");
                            Thread.Sleep(1000 * counter);
                            continue;
                        }
                    }
                }
            }
        }
    }
}
