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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.Google.GControlPlatform;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.BulkAction.ManualApprovalBulkActions
{
    public class ManualApprovalBulkApproveAction : ManualApprovalBulkAction
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ManualApprovalBulkAction));

        private static readonly ConcurrentDictionary<Guid, RMManualApproveHistoryTableEntity> _historyCache = new();

        private static readonly IRMWorkflowDefinitionDao _workflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static readonly IWorkflowInstanceDao _workflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IRMEmailItemDao _emailItemDao = PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        private static IFSConnectionDao _FSConnectionDao;
        public static IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction;

        private readonly HistoryAddAction _historyAddAction;

        private readonly RMWorkflowProcessor _workflowProcessor;

        public override ManualApprovalBulkActionType ActionType
        {
            get
            {
                return ManualApprovalBulkActionType.Approve;
            }
        }

        protected override List<ManualApprovalFilterDefinition> FilterDefinitions
        {
            get
            {
                return new() 
                {
                    new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                        Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                    },
                    new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.ExtendTime,
                        Value = "false"
                    }
                };
            }
        }
        protected override List<ManualApprovalFilterDefinition> FilterGControlDefinitions
        {
            get
            {
                return new()
                {
                    new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.GControlApprovalStatus,
                        Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                    },
                    new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.ExtendTime,
                        Value = "false"
                    }
                };
            }
        }

        public ManualApprovalBulkApproveAction()
        {
            _syncArchiverStatusAction = new();
            _historyAddAction = new();
            _workflowProcessor = new();
        }

        protected override async Task SucceedAction(Record item, string[] reviewers)
        {
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

                ManualApprovalBulkActionManager.AddSucceedJobDetail(item, (int)ActionType, _manualAppovalActionI18N[ActionType], reviewers);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred process auto approval succeed item [{item.Id}]. Error: {e}");
                ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], e.Message);
            }
            
        }

        protected override async Task ProcessAction(Record item)
        {
            try
            {
                item.ManualApprovalComment = ManualApprovalInfos.ApprovalComment;
                item.QuickReason = ManualApprovalInfos.QuickReason;
                if ((int)ActionType == (int)SOApproveDBStatus.Approved)
                {
                    item.QuickReason = string.Empty;
                    item.ManualLastReasonForRejection = string.Empty;
                }
                if (NeedApprovalOrRejectForWorkflow(item))
                {
                    await ApprovalOrRejectForWorkflowNewAsync(item);
                }
                else
                {
                    var historyData = _historyAddAction.Convert(item, (SOApproveDBStatus)ActionType, ApprovalAccount.Id);
                    item.ManualInternalApprovedStatus = (int)ActionType;
                    item.ManualApprovedStatus = (int)ActionType;
                    item.ManualApprovedBy = ApprovalAccount.Id;
                    item.ManualActionTime = DateTime.UtcNow.Ticks;
                    item.ManualLastApproveRejectComment = ManualApprovalInfos.ApprovalComment;
                    item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
                    item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
                    if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                    {
                        item.DisposalStatus = (int)ActionType;
                    }

                    ManualApprovalAzureTableManager.RebuildAudits(item, (SOApproveDBStatus)ActionType, ApprovalAccount);
                    if (item.SourceFlag >= 1000)
                    {
                        _historyCache.TryAdd(item.Id, historyData);
                    }
                }

                List<ManualApprovalFSAuditRecordDto> list = new List<ManualApprovalFSAuditRecordDto>();
                ManualApprovalFSAuditRecordDto record = BuildAuditRecords(item);
                list.Add(record);
                FSAuditSinkService.ApproveOrRejectFlushAsync(list);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while process item [{item.Id}]. Error: {e}");
                throw;
            }
        }

        private ManualApprovalFSAuditRecordDto BuildAuditRecords(Record item)
        {
            ManualApprovalFSAuditRecordDto record = new ManualApprovalFSAuditRecordDto();
            record.NodeId = item.Id;
            record.NodeName = item.LeafName;
            record.AuditLevel = (int)FSAuditLevel.File;
            record.ConnectionId = item.L2PartitionKey;
            if (!string.IsNullOrEmpty(record.ConnectionId))
            {
                FSConnection conn = FSConnectionDao.GetConnectionById(Guid.Parse(record.ConnectionId));
                if (conn != null && conn.GroupId != null)
                {
                    record.ConnectionGroupId = conn.GroupId.ToString();
                }
            }
            record.FullPath = item.ManualFullPath;
            record.Content = item.ManualLastApproveRejectComment;
            record.Status = (int)AuditStatus.Successful;
            record.ActionType = SOApproveDBStatus.Approved;
            return record;
        }

        private async Task ApprovalOrRejectForWorkflowNewAsync(Record item)
        {
            var workflowDefinitionId = FromGControl ? new Guid(item.GControlApprovalProcessId) : item.ManualWorkflowDefinitionId;
            var workflowStepId = FromGControl ? new Guid(item.GControlCurrentStageId) : item.ManualWorkflowStepId;
            if (item.ManualWorkflowInstanceId != Guid.Empty && item.ManualWorkflowDefinitionId == Guid.Empty && item.ManualWorkflowStepId == Guid.Empty)
            {
                var instance = await _workflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;

                await _workflowInstanceDao.UpdateStatusAsync(instance.Id, RMWorkflowStatus.Completed);
            }

            var workflowInstance = FromGControl switch
            {
                true => await _workflowProcessor.LoadFromGControlAsync(workflowDefinitionId),
                _ => await _workflowProcessor.LoadAsync(workflowDefinitionId)
            };

            var currentStep = workflowInstance.LoadStep(workflowStepId);

            var nextStep = currentStep;
            if ((int)ActionType == (int)SOApproveDBStatus.Approved)
            {
                nextStep = currentStep.Approve();
            }
            else
            {
                nextStep = currentStep.Reject();
            }

            var historyData = _historyAddAction.Convert(item, (SOApproveDBStatus)ActionType, ApprovalAccount.Id);
            if (FromGControl)
            {
                item.GControlCurrentStageId = nextStep.Id.ToString();
            }
            else
            {
                item.ManualWorkflowStepId = nextStep.Id;
            }
            item.ManualApprovedBy = ApprovalAccount.Id;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            item.ManualEscalateFrom = 0;
            item.ManualEscalatedComment = string.Empty;
            item.ManualLastApproveRejectComment = ManualApprovalInfos.ApprovalComment;
            item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
            ManualApprovalAzureTableManager.RebuildAudits(item, (SOApproveDBStatus)ActionType, ApprovalAccount);

            if (!nextStep.IsEnd)
            {
                if (FromGControl)
                {
                    item.GControlCurrentApproverId = (await nextStep.GetReviewersAsync(item.ScopeId))[0].UserId;
                    item.GControlManualReviewers = [];
                    if (!_accountCache.Contains(item.GControlCurrentApproverId))
                    {
                        var ggUserExistInDb = await CheckGGUserExistenceInDB(item.GControlCurrentApproverId);
                        if (!ggUserExistInDb)
                        {
                            await UpdateGoogleUserAsync(item.GControlCurrentApproverId);
                        }
                        _accountCache.Add(item.GControlCurrentApproverId);
                    }
                }
                if(item.IsFsControlRecordJPMC)
                {
                    item.ManualReviewer = (await nextStep.GetReviewersAsync(new Guid(item.AveSiteId))).Select(item => item.RMUserId).ToArray();
                }
                else
                {
                    item.ManualReviewer = (await nextStep.GetReviewersAsync(item.ScopeId)).Select(item => item.RMUserId).ToArray();
                }
            }

            if (nextStep.IsEnd)
            {
                if (FromGControl)
                {
                    item.GControlManualApprovedStatus = (int)ActionType;
                    item.GControlManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                }
                else
                {
                    item.ManualApprovedStatus = (int)ActionType;
                    item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                }

                if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                {
                    item.DisposalStatus = (int)ActionType;
                }
            }

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
            
            AddNewWorkflowDto(workflowDefinitionId, workflowStepId);
        }

        protected override List<ManualApprovalRecord> GenerateItems(List<ManualApprovalRecord> Items)
        {
            return Items;
        }
    }
}
