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
using Aspose.Pdf.Operators;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.SharePoint.ArchiverCommon;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using RAManualApproval.BulkAction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.FolderView
{
    public class ManualFolderViewActionProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ManualFolderViewActionProcessor));

        private static readonly ManualApprovalRecordRepository _repository = new();

        private static readonly ConcurrentDictionary<Guid, RMManualApproveHistoryTableEntity> _historyCache = new();

        private static readonly ConcurrentDictionary<Guid, string[]> _reviewerNamesCache = new();

        private static readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private static readonly IRMWorkflowDefinitionDao _workflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static readonly IWorkflowInstanceDao _workflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IRMEmailItemDao _emailItemDao = PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static readonly IRMFunctionSettingDao _functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();


        private static readonly int _pageSize = 500;
        private ManualApprovalActionParams ManualApprovalActionInfos { get; set; }
        private RMAccount ApprovalAccount { get; set; }

        protected int CurrentTotalCount { get; set; }

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction;

        private readonly HistoryAddAction _historyAddAction;

        private readonly RMWorkflowProcessor _workflowProcessor;

        private readonly int _maxExtentedCount;
        private string Continuation { get; set; }

        public ManualFolderViewActionProcessor(string jobId, string userId)
        {
            ManualApprovalJobManager.Init(jobId, JobType.ManualFolderViewActions);
            var subJob = _subJobDao.GetSubJob(jobId, true);
            ManualApprovalActionInfos = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalActionParams>(subJob.JobContext.Content);
            var settingInfo = _functionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
            var manualSettingInfo = JsonConvert.DeserializeObject<ManualApprovalSettings>(settingInfo);
            _maxExtentedCount = manualSettingInfo.DisposalExtentionSetting.MaxDelayTimes;

            if (!string.IsNullOrEmpty(userId))
            {
                ApprovalAccount = _accountDao.Find(item => item.UserId == userId && item.IsRemoved == 0);
            }

            _historyAddAction = new();
            _syncArchiverStatusAction = new();
            _workflowProcessor = new();
        }

        public async Task RunAsync()
        {
            try
            {
                using (var jScope = new CheckJobStopScope())
                {
                    _logger.Info($"Begin to executor [{ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType]}] action");
                    ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessRecordSucceedAsync, ProcessRecordFailed);

                    var needActionIds = ManualApprovalActionInfos.NeedActionIds;
                    _logger.Info($"Need action count is {needActionIds.Count}");

                    var items = await _repository.QueryItemsAsync(record => needActionIds.Contains(record.Id));
                    _logger.Info($"Need action folder count is {items?.Count(item => item.NodeType == (int)NodeType.Folder || item.NodeType == (int)NodeType.List)}");

                    using (new PerformanceScope($"Excute Action", $"Action Type :[{ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType]}], item count: {items.Count}", true))
                    {
                        var parentQueue = new Queue<Record>();
                        await ProcessItems(items, parentQueue);
                        CurrentTotalCount += items.Count;

                        while (parentQueue.Count > 0)
                        {
                            using var queueScope = new CheckJobStopScope();
                            var folder = parentQueue.Dequeue();
                            do
                            {
                                using var folderScope = new CheckJobStopScope();
                                _logger.Info($"Current process folder is [{folder.ManualFullPath}]");
                                var childs = await QueryItemsUnderParent(folder);
                                CurrentTotalCount += childs.Count;
                                _logger.Info($"Current process items count is [{childs.Count}]");
                                await ProcessItems(childs, parentQueue);

                                if (CurrentTotalCount >= 10000)
                                {
                                    ManualApprovalDataSyncManager.Commit();
                                    ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessRecordSucceedAsync, ProcessRecordFailed);
                                    CurrentTotalCount = 0;
                                }
                            } while (!string.IsNullOrEmpty(Continuation));
                        }
                        ManualApprovalDataSyncManager.WaitComplete();
                        ManualApprovalJobManager.SetJobFinished();
                        PerformanceMonitor.WritePerformanceResult();
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Info($"Process folder view  [{ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType]}] action failed, error : {e}");
                ManualApprovalJobManager.SetJobFailed(e.Message);
            }
        }

        private async Task ProcessItems(List<ManualApprovalRecord> items, Queue<Record> queue)
        {
            foreach (var item in items)
            {
                var reviewers = GetReviewers(item.ManualReviewer);
                try
                {
                    if (item.NodeType == (int)NodeType.Folder || item.NodeType == (int)NodeType.List)
                    {
                        queue.Enqueue(item);
                        continue;
                    }

                    if (item.ManualExtendTime >= DateTime.UtcNow.Ticks)
                    {
                        _logger.Error($"Extended item can't approve/reject");
                        continue;
                    }

                    if (ManualApprovalActionInfos.ActionType == SOApproveDBStatus.Rejected && item.ManualExtendCount >= _maxExtentedCount)
                    {
                        throw new Exception("RM_MA_Extended_ExtendLimitForOne");
                    }

                    await ProcessFolderViewAction(item);
                    ManualApprovalDataSyncManager.Add(item);
                    _reviewerNamesCache.TryAdd(item.Id, reviewers);
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred process item, item id : [{item.Id}], action : [{ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType]}] Error: {e}");
                    ManualApprovalJobManager.AddFailedJobDetail(item, (int)ManualApprovalActionInfos.ActionType, reviewers, ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType], e.Message);
                }
            }
        }

        private async Task<List<ManualApprovalRecord>> QueryItemsUnderParent(Record parentRecord)
        {
            using (new PerformanceScope("Query Datas under parent folder", $"Parent folder id is {parentRecord.NodeId}", true))
            {
                var queryDefinition = new ManualApprovalQueryDefinition();
                queryDefinition.PageSize = _pageSize;
                queryDefinition.ManualSiteUrl = parentRecord.ManualSiteUrl;
                queryDefinition.IsEnableFolderView = true;
                queryDefinition.Continuation = Continuation;
                queryDefinition.FolderInfos = new()
                {
                    new()
                    {
                        Id = parentRecord.Id.ToString(),
                        NodeId = parentRecord.NodeId.ToString(),
                        LeafName = parentRecord.LeafName,
                        NodeType = parentRecord.NodeType,
                        ParentId = parentRecord.ParentId.ToString(),
                        ManualSiteUrl = parentRecord.ManualSiteUrl,
                        ManualFullPath = parentRecord.ManualFullPath,
                    }
                };
                queryDefinition.NeedCalculationCount = false;
                var result = await ManualApprovalQuerier.CosmosDBFolderViewQueryAsync(queryDefinition, _repository);
                Continuation = result.Continuation;
                return result.Items;
            }
        }

        private async Task ProcessRecordSucceedAsync(Record item)
        {
            using (new PerformanceScope("Update Archiver and Add history"))
            {
                var reviewers = _reviewerNamesCache[item.Id];
                try
                {
                    if (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved ||
                        item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                    {
                        await _syncArchiverStatusAction.UpdateItemArchiverStatusAsync(item);
                    }

                    if (_historyCache.TryGetValue(item.Id, out var historyData))
                    {
                        await _historyAddAction.AddAsync(historyData);
                        _logger.Info($"Successful add item: [{item.Id}] to history.");
                        _historyCache.Remove(item.Id, out var history);
                    }

                    ManualApprovalJobManager.AddSucceedJobDetail(item, (int)ManualApprovalActionInfos.ActionType, ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType], reviewers);
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred process auto approval succeed item [{item.Id}]. Error: {e}");
                    ManualApprovalJobManager.AddFailedJobDetail(item, (int)ManualApprovalActionInfos.ActionType, reviewers, ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType], e.Message);
                }
                _reviewerNamesCache.Remove(item.Id, out var reviewer);
            }
        }

        private void ProcessRecordFailed(Record item, string message)
        {
            var reviewerNames = _reviewerNamesCache[item.Id];
            ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ManualApprovalActionInfos.ActionType, reviewerNames, ManualApprovalJobManager.ManualAppovalActionI18N[ManualApprovalActionInfos.ActionType], message);
            _reviewerNamesCache.Remove(item.Id, out var reviewer);
        }

        private async Task ProcessFolderViewAction(Record item)
        {
            try
            {
                var actionType = ManualApprovalActionInfos.ActionType;
                item.ManualApprovalComment = ManualApprovalActionInfos.ApprovalComment;
                item.QuickReason = ManualApprovalActionInfos.QuickReason;
                if (actionType == SOApproveDBStatus.Approved)
                {
                    item.QuickReason = string.Empty;
                }
                if (item.ManualWorkflowInstanceId != Guid.Empty || (item.ManualWorkflowDefinitionId != Guid.Empty && item.ManualWorkflowStepId != Guid.Empty))
                {
                    await ApprovalOrRejectForWorkflowNewAsync(item);
                }
                else
                {
                    var historyData = _historyAddAction.Convert(item, actionType, ApprovalAccount.Id);
                    item.ManualInternalApprovedStatus = (int)actionType;
                    item.ManualApprovedStatus = (int)actionType;
                    if ((int)actionType == (int)SOApproveDBStatus.Rejected)
                    {
                        item.ManualLastReasonForRejection = item.QuickReason;
                    }
                    item.ManualApprovedBy = ApprovalAccount.Id;
                    item.ManualActionTime = DateTime.UtcNow.Ticks;
                    item.ManualLastApproveRejectComment = ManualApprovalActionInfos.ApprovalComment;
                    item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
                    item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
                    if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                    {
                        item.DisposalStatus = (int)actionType;
                    }

                    if (actionType == SOApproveDBStatus.Rejected)
                    {
                        await ManualApprovalAzureTableManager.RebuildAuditsAsync(item, actionType, ApprovalAccount, ManualApprovalActionInfos.ExtendType, 0, ManualApprovalActionInfos.CustomeExtendDate);
                    }
                    else
                    {
                        ManualApprovalAzureTableManager.RebuildAudits(item, actionType, ApprovalAccount);
                    }
                    if (item.SourceFlag >= 1000)
                    {
                        _historyCache.TryAdd(item.Id, historyData);
                    }
                    if (actionType == SOApproveDBStatus.Rejected)
                    {
                        item.ManualExtendTime = await CalculationExtendTimeAsync(ManualApprovalActionInfos.ExtendType, ManualApprovalActionInfos.CustomeExtendDate);
                        item.ManualExtendComment = string.Empty;
                        item.ManualExtendCount += 1;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while process item [{item.Id}]. Error: {e}");
                throw;
            }
        }

        private async Task ApprovalOrRejectForWorkflowNewAsync(Record item)
        {
            var actionType = ManualApprovalActionInfos.ActionType;
            var workflowDefinitionId = item.ManualWorkflowDefinitionId;
            var workflowStepId = item.ManualWorkflowStepId;
            if (item.ManualWorkflowInstanceId != Guid.Empty && item.ManualWorkflowDefinitionId == Guid.Empty && item.ManualWorkflowStepId == Guid.Empty)
            {
                var instance = await _workflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;

                await _workflowInstanceDao.UpdateStatusAsync(instance.Id, RMWorkflowStatus.Completed);
            }

            var workflowInstance = _workflowProcessor.LoadAsync(workflowDefinitionId).GetAwaiter().GetResult();
            var currentStep = workflowInstance.LoadStep(workflowStepId);

            var nextStep = currentStep;
            if (actionType == SOApproveDBStatus.Approved)
            {
                nextStep = currentStep.Approve();
            }
            else
            {
                nextStep = currentStep.Reject();
            }
            var historyData = _historyAddAction.Convert(item, actionType, ApprovalAccount.Id);
            item.ManualWorkflowStepId = nextStep.Id;
            item.ManualApprovedBy = ApprovalAccount.Id;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            item.ManualEscalateFrom = 0;
            item.ManualEscalatedComment = string.Empty;
            item.ManualLastApproveRejectComment = ManualApprovalActionInfos.ApprovalComment;
            item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
            ManualApprovalAzureTableManager.RebuildAudits(item, actionType, ApprovalAccount);

            if (!nextStep.IsEnd)
            {
                item.ManualReviewer = (await nextStep.GetReviewersAsync(item.ScopeId)).Select(item => item.RMUserId).ToArray();
            }

            if (nextStep.IsEnd)
            {
                item.ManualApprovedStatus = (int)actionType;
                item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;

                if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                {
                    item.DisposalStatus = (int)actionType;
                }
                if (actionType == SOApproveDBStatus.Rejected)
                {
                    item.ManualLastReasonForRejection = item.QuickReason;
                    item.ManualExtendTime = await CalculationExtendTimeAsync(ManualApprovalActionInfos.ExtendType, ManualApprovalActionInfos.CustomeExtendDate);
                    item.ManualExtendComment = string.Empty;
                    item.ManualExtendCount += 1;
                }
            }
            await ManualApprovalAzureTableManager.RebuildAuditsAsync(item, actionType, ApprovalAccount, ManualApprovalActionInfos.ExtendType, 0, ManualApprovalActionInfos.CustomeExtendDate);
            if (!nextStep.IsEnd && item.ManualNeedEmailNotification)
            {
                var emailItem = new RMEmailItem
                {
                    Id = item.Id,
                    Status = RMSendEmailStatus.WaittingSendEmail,
                    ModifyTime = DateTime.UtcNow
                };
                await _emailItemDao.AddWorkflowManualItemAsync(emailItem);
            }

            if (!nextStep.IsEnd || item.SourceFlag > (int)SourceFlag.Connector)
            {
                _historyCache.TryAdd(item.Id, historyData);
            }
        }

        private static async Task<long> CalculationExtendTimeAsync(ManualApprovalExtendType extendType, DateTime customeExtendDate)
        {
            var now = DateTime.UtcNow;
            if (extendType == ManualApprovalExtendType.Custom)
            {
                return customeExtendDate.Ticks;
            }
            else if (extendType == ManualApprovalExtendType.After1Month)
            {
                return now.AddMonths(1).Ticks;
            }
            else if (extendType == ManualApprovalExtendType.After3Month)
            {
                return now.AddMonths(3).Ticks;
            }
            else if (extendType == ManualApprovalExtendType.After6Month)
            {
                return now.AddMonths(6).Ticks;
            }
            else if (extendType == ManualApprovalExtendType.After1Year)
            {
                return now.AddYears(1).Ticks;
            }

            return 0;
        }

        private static string[] GetReviewers(int[] reviewerIds)
        {
            var reviewerNames = Array.Empty<string>();
            try
            {
                reviewerNames = ManualApprovalOwnerManager.GetOwnerDisplayNames(reviewerIds).ToArray();
                return reviewerNames;
            }
            catch (Exception e)
            {
                _logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
        }
    }
}
