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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RADataBroker;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class ReManualReviewAction
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ReManualReviewAction));

        private static IWorkflowInstanceDao WorkflowInstanceDao => PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMWorkflowSiteOwnersDao WorkflowSiteOwnersDao => PlatformWindsorManager.GetService<IRMWorkflowSiteOwnersDao>();

        private static IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private static IRMEmailItemDao EmailItemDao => PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly ManualApprovalRecordRepository Repository;

        private readonly RMAccount ApprovalAccount;

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction;

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;

        private readonly bool _hasGControlLicense;

        private readonly bool _isFromGControl;

        public ReManualReviewAction(ManualApprovalRecordRepository repository, bool isFromGControl = false)
        {
            Repository = repository;
            var accountId = TenantLocalValue.LogonUserId;

            if (!string.IsNullOrEmpty(accountId))
            {
                ApprovalAccount = AccountDao.Find(item => item.UserId == accountId && item.IsRemoved == 0);
            }

            _syncArchiverStatusAction = new();

            _hasFSLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            _hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;
            _isFromGControl = isFromGControl;
        }

        public async  Task<ManualApprovalActionResult> ResetWorkflow(List<Guid> ids)
        {
            Logger.Info($"Start reset workflow for items: [{string.Join(",", ids)}]");
            var result = new ManualApprovalActionResult();

            var items = _isFromGControl ? await Repository.QueryItemsAsync(record => ids.Contains(record.Id) &&
                    record.GControlManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete &&
                    record.ManualArchiveStatus != (int)Contract.Schedule.ActionStatus.Archiverd)
                : await Repository.QueryItemsAsync(record => ids.Contains(record.Id) &&
                    record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete &&
                    record.ManualArchiveStatus != (int)Contract.Schedule.ActionStatus.Archiverd);


            if (!_hasFSLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.FileSystem))
            {
                result.CompletedStatus = ActionCompletedStatus.Failed;
                return result;
            }

            if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
            {
                result.CompletedStatus = ActionCompletedStatus.Failed;
                return result;
            }
            if (!_hasGControlLicense && items.Any(item => item.IsGControlRecord))
            {
                result.CompletedStatus = ActionCompletedStatus.Failed;
                return result;
            }

            foreach (var item in items)
            {
                var itemActionResult = await ResetWorkflowAsync(item);
                result.EffectItems.Add(itemActionResult);
            }

            if (result.EffectItems.All(item => item.IsSucceed))
            {
                result.CompletedStatus = ActionCompletedStatus.Succeed;
            }
            else if (result.EffectItems.All(item => !item.IsSucceed))
            {
                result.CompletedStatus = ActionCompletedStatus.Failed;
            }
            else
            {
                result.CompletedStatus = ActionCompletedStatus.HasException;
            }

            return result;
        }

        private async Task<ManualApprovalItemActionResult> ResetWorkflowAsync(ManualApprovalRecord item)
        {
            try
            {
                var result = new ManualApprovalItemActionResult
                {
                    IsSucceed = true,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
                };

                var instanceId = Guid.Empty;

                var workflowDefinitionId = _isFromGControl ? new Guid(item.GControlApprovalProcessId) : item.ManualWorkflowDefinitionId;
                if (workflowDefinitionId == Guid.Empty)
                {
                    var instance = await RMWorkflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                    workflowDefinitionId = instance.DefinitionId;
                    instanceId = instance.Id;
                }

                var definition = _isFromGControl ? await ManualProcessManagementService.LoadProcessFromGControl(workflowDefinitionId) : ManualProcessManagementService.LoadProcess(workflowDefinitionId);
                var analyzer = new WorkflowAnalyzer(definition);
                var waitingStepNode = analyzer.WaitingForApprove();

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;
                if (_isFromGControl)
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
                item.ManualLastReasonForRejection = item.QuickReason;
                item.ManualActionTime = DateTime.UtcNow.Ticks;
                item.ManualIsAutoReassigned = false;
                item.ManualEmailNotificationCount = 0;
                item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                item.ManualEscalateFrom = 0;
                item.ManualLastExtendType = ManualApprovalExtendType.After1Month;
                if (waitingStepNode.ReviewerType == WorkflowReviewerType.SiteOwners)
                {
                    var users = await WorkflowSiteOwnersDao.FindListAsync(i => i.DefinitionId == workflowDefinitionId.ToString() && i.SiteId == item.ScopeId && !i.IsSPGroup);
                    item.ManualReviewer = AccountDao.GetUserWithRemovedByUserIds(users.Select(i => i.OwnerId).ToList())
                        .OrderByDescending(item => item.CreateTime)
                        .DistinctBy(item => item.UserPrincipalName).Select(i => i.Id).ToArray();
                }
                else if(waitingStepNode.ReviewerType == WorkflowReviewerType.SharePointGroup)
                {
                    var groupName = waitingStepNode.GroupName.Trim();
                    var users = await WorkflowSiteOwnersDao.FindListAsync(i => i.DefinitionId == workflowDefinitionId.ToString() && i.SiteId == item.ScopeId && i.IsSPGroup && i.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                    item.ManualReviewer = AccountDao.GetUserWithRemovedByUserIds(users.Select(i => i.OwnerId).ToList())
                        .OrderByDescending(item => item.CreateTime)
                        .DistinctBy(item => item.UserPrincipalName).Select(i => i.Id).ToArray();
                }
                else
                {
                    if (_isFromGControl)
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
                    await WorkflowInstanceDao.UpdateStatusAsync(instanceId, RMWorkflowStatus.Completed);
                }

                if (item.ManualNeedEmailNotification)
                {
                    var emailItem = new RMEmailItem
                    {
                        Id = item.Id,
                        Status = RMSendEmailStatus.WaittingSendEmail,
                        ModifyTime = DateTime.UtcNow
                    };
                    await EmailItemDao.AddWorkflowManualItemAsync(emailItem);
                }

                await Repository.UpsertItemAsync(item);
                
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while reset workflow for item: [{item.Id}]. Error: {e}");
                return new ManualApprovalItemActionResult
                {
                    IsSucceed = false,
                    Message = e.Message,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
                };
            }
        }

        private void RebuildAudits(ManualApprovalRecord item)
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
