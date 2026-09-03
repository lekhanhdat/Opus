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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.BulkAction.ManualApprovalBulkActions
{
    public class ManualApprovalBulkRestartProcessAction : ManualApprovalBulkAction
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ManualApprovalBulkAction));

        private static readonly ConcurrentDictionary<Guid, AvePoint.RA.DB.Model.RMWorkflowInstance> _workflowInstanceCache = new();

        private static readonly ConcurrentDictionary<Guid, WorkflowDefinitionDto> _workflowDefinitionceCache = new();

        private static readonly ConcurrentDictionary<string, List<RMWorkflowSiteOwner>> _siteOwnersCache = new();

        private static readonly ConcurrentDictionary<string, List<RMWorkflowSiteOwner>> _spGroupCache = new();

        private static readonly ConcurrentDictionary<string, int[]> _reviewersCache = new();

        private static readonly IManualProcessManagementService _manualProcessManagementService = PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private static readonly IRMWorkflowDefinitionDao _workflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static readonly IWorkflowInstanceDao _workflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IRMEmailItemDao _emailItemDao = PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static readonly IRMWorkflowSiteOwnersDao _workflowSiteOwnersDao = PlatformWindsorManager.GetService<IRMWorkflowSiteOwnersDao>();

        private static readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction;

        public override ManualApprovalBulkActionType ActionType
        {
            get
            {
                return ManualApprovalBulkActionType.RestartProcess;
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
                        Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected })
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
                        Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected })
                    }
                };
            }
        }

        public ManualApprovalBulkRestartProcessAction()
        {
            _syncArchiverStatusAction = new();
        }

        protected override async Task SucceedAction(Record item, string[] reviewers)
        {
            try
            {
                ManualApprovalBulkActionManager.AddSucceedJobDetail(item, 0, _manualAppovalActionI18N[ActionType], reviewers);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred process restart item workflow: [{item.Id}]. Error: {e}");
                ManualApprovalBulkActionManager.AddFailedJobDetail(item, 0, reviewers, _manualAppovalActionI18N[ActionType], e.Message);
            }
        }

        protected override async Task ProcessAction(Record item)
        {
            var instanceId = Guid.Empty;
            var workflowDefinitionId = FromGControl ? new Guid(item.GControlApprovalProcessId) : item.ManualWorkflowDefinitionId;
            if (workflowDefinitionId == Guid.Empty)
            {
                var instance = new AvePoint.RA.DB.Model.RMWorkflowInstance();
                if (!_workflowInstanceCache.ContainsKey(workflowDefinitionId))
                {
                    _workflowInstanceCache[workflowDefinitionId] = await _workflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                }
                instance = _workflowInstanceCache[workflowDefinitionId];
                workflowDefinitionId = instance.DefinitionId;
                instanceId = instance.Id;
            }

            var definition = new WorkflowDefinitionDto();
            if (!_workflowDefinitionceCache.ContainsKey(workflowDefinitionId))
            {
                _workflowDefinitionceCache[workflowDefinitionId] = FromGControl ? await _manualProcessManagementService.LoadProcessFromGControl(workflowDefinitionId) : _manualProcessManagementService.LoadProcess(workflowDefinitionId);

            }

            definition = _workflowDefinitionceCache[workflowDefinitionId];
            var analyzer = new WorkflowAnalyzer(definition);
            var waitingStepNode = analyzer.WaitingForApprove();

            item.ManualWorkflowInstanceId = Guid.Empty;
            item.ManualWorkflowDefinitionId = workflowDefinitionId;
            if (FromGControl)
            {
                item.GControlCurrentStageId = waitingStepNode.Id.ToString();
                item.GControlManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                item.GControlManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            }
            else
            {
                item.ManualWorkflowStepId = waitingStepNode.Id;
                item.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            }
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            if (waitingStepNode.ReviewerType == WorkflowReviewerType.SiteOwners)
            {
                var users = new List<RMWorkflowSiteOwner>();
                var usersKey = workflowDefinitionId.ToString() + ";" + item.ScopeId.ToString();
                if (!_siteOwnersCache.ContainsKey(usersKey))
                {
                    _siteOwnersCache[usersKey] = _workflowSiteOwnersDao.FindListAsync(i => i.DefinitionId == workflowDefinitionId.ToString() && i.SiteId == item.ScopeId && !i.IsSPGroup).GetAwaiter().GetResult();
                }
                users = _siteOwnersCache[usersKey];

                var usersIdKey = string.Join(';', users.Select(i => i.OwnerId));
                if (!_reviewersCache.ContainsKey(usersIdKey))
                {
                    _reviewersCache[usersIdKey] = _accountDao.GetUserWithRemovedByUserIds(users.Select(i => i.OwnerId).ToList())
                        .OrderByDescending(item => item.CreateTime)
                        .DistinctBy(item => item.UserPrincipalName).Select(i => i.Id).ToArray();
                }
                item.ManualReviewer = _reviewersCache[usersIdKey];
            }
            else if(waitingStepNode.ReviewerType == WorkflowReviewerType.SharePointGroup)
            {
                var users = new List<RMWorkflowSiteOwner>();
                var groupName = waitingStepNode.GroupName.Trim();
                var usersKey = $"{workflowDefinitionId}=AVE={item.ScopeId}=AVE={groupName}";
                if (!_spGroupCache.ContainsKey(usersKey))
                {
                    _spGroupCache[usersKey] = await _workflowSiteOwnersDao.FindListAsync(i => i.DefinitionId == workflowDefinitionId.ToString() && i.SiteId == item.ScopeId && i.IsSPGroup && i.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                }
                users = _spGroupCache[usersKey];

                var usersIdKey = string.Join(';', users.Select(i => i.OwnerId));
                if (!_reviewersCache.ContainsKey(usersIdKey))
                {
                    _reviewersCache[usersIdKey] = _accountDao.GetUserWithRemovedByUserIds(users.Select(i => i.OwnerId).ToList())
                        .OrderByDescending(item => item.CreateTime)
                        .DistinctBy(item => item.UserPrincipalName).Select(i => i.Id).ToArray();
                }
                item.ManualReviewer = _reviewersCache[usersIdKey];
            }
            else
            {
                if (FromGControl)
                {
                    item.GControlCurrentApproverId = waitingStepNode.Reviewers[0].UserId;
                    item.GControlManualReviewers = [];
                }
                else
                {
                    item.ManualReviewer = waitingStepNode.Reviewers.Select(i => i.RMUserId).ToArray();
                }
            }
            RebuildAudits(item);

            await _syncArchiverStatusAction.ResetItemArchiverStatusAsync(item);

            if (instanceId != Guid.Empty)
            {
                await _workflowInstanceDao.UpdateStatusAsync(instanceId, RMWorkflowStatus.Completed);
            }

            if (item.ManualNeedEmailNotification)
            {
                var emailItem = new RMEmailItem
                {
                    Id = item.Id,
                    Status = RMSendEmailStatus.WaittingSendEmail,
                    ModifyTime = DateTime.UtcNow
                };
                await _emailItemDao.AddWorkflowManualItemAsync(emailItem);
            }
        }

        protected override List<ManualApprovalRecord> GenerateItems(List<ManualApprovalRecord> Items)
        {
            var needRestartProcessItems = Items.Where(item => (FromGControl 
                ? item.GControlManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete 
                : item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete)
             && item.ManualArchiveStatus != (int)ActionStatus.Archiverd).ToList();

            var noNeedRestartProcessItems = Items.Except(needRestartProcessItems).ToList();

            if (noNeedRestartProcessItems.Any())
            {
                _logger.Info($"No need to restart items count is : {noNeedRestartProcessItems.Count}");
                ManualApprovalBulkActionManager.BetchAddSkippedJobDetail(noNeedRestartProcessItems, 0, _manualAppovalActionI18N[ActionType], "RM_MA_ItemCannotRestartProcess");
            }

            return needRestartProcessItems;
        }

        private void RebuildAudits(Record item)
        {
            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = ApprovalAccount.DisplayName,
                Action = "RM_JS_MA_ResetManualWorkflow"
            });
            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }
    }
}
