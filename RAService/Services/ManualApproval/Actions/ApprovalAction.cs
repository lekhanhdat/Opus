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
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.Records.Core.Utilities.Extensions;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using GOneGlobal.GlobalDomain;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RAGoogle.Common;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class ApprovalAction
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ApprovalAction));

        private static IWorkflowInstanceDao WorkflowInstanceDao => PlatformWindsorManager.GetService<IWorkflowInstanceDao>();
        private static IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMEmailItemDao EmailItemDao => PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly HashSet<GControlWorkflowDto> _gControlWorkflowDtos = new();
        
        private static IGControlTaskAssigneeService GControlTaskAssigneeService => PlatformWindsorManager.GetService<IGControlTaskAssigneeService>();
        
        private static ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        private IUserService _userService => PlatformWindsorManager.GetService<IUserService>();

        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        private static IFSConnectionDao _FSConnectionDao;

        public static IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        private readonly RMWorkflowProcessor _workflowProcessor ;

        private readonly HistoryAddAction _historyAddAction;

        private readonly ManualApprovalRecordRepository _repository;

        private readonly SOApproveDBStatus _approvalStatus;

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction;

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;
        
        private readonly bool _hasGControlLicense;

        private RMAccount _approvalAccount;
        
        private IPeoplePickerService _peoplePickerService;
        
        private ConcurrentBag<string> _accountCache = new();


        public ApprovalAction(ManualApprovalRecordRepository repository, SOApproveDBStatus approvalStatus)
        {
            _repository = repository;
            _approvalStatus = approvalStatus;
            
            _workflowProcessor = new();
            _historyAddAction = new ();
            _syncArchiverStatusAction = new();

            _hasFSLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            _hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;
            _peoplePickerService = new PeoplePickerService();
        }

        public async System.Threading.Tasks.Task InitAsync()
        {
            var accountId = TenantLocalValue.LogonUserId;

            if (!string.IsNullOrEmpty(accountId))
            {
                _approvalAccount = await AccountDao.GetActiveUserByUserIdAsync(accountId);
            }
        }

        public async Task<ManualApprovalActionResult> ApproveOrReject(ManualApprovalActionParams approveParameters)
        {
            var ids = approveParameters.NeedActionIds;
            
            Logger.Info($"Start process [{_approvalStatus}] for items: [{string.Join(",", ids)}]");
            var result = new ManualApprovalActionResult();

            var perCheckResult = await PreCheckApporvalItems(approveParameters.NeedActionIds, approveParameters.FromGControl);
            if (!perCheckResult)
            {
                result.CompletedStatus = ActionCompletedStatus.Failed;
                result.Message = "Cant not find items in database";
                return result;
            }

            var items = await _repository.QueryItemsAsync(record => ids.Contains(record.Id));

            if(!_hasFSLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.FileSystem))
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

            Dictionary<Guid, ManualApprovalRecord> nodeDict = items.ToDictionary(i => i.Id, i => i);

            foreach (var item in items)
            {
                var itemActionResult = await ApprovalOrRejectAsync(item, approveParameters);
                result.EffectItems.Add(itemActionResult);
            }

            if (result.EffectItems != null && result.EffectItems.Count > 0) {
                List<ManualApprovalFSAuditRecordDto> list = new List<ManualApprovalFSAuditRecordDto>();
                foreach (var item in result.EffectItems) {
                    ManualApprovalRecord node = nodeDict[item.Id];
                    ManualApprovalFSAuditRecordDto record = BuildAuditRecords(item, node, approveParameters.ApprovalComment);
                    list.Add(record);
                }
                FSAuditSinkService.ApproveOrRejectFlushAsync(list);
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

            if (_gControlWorkflowDtos.Count > 0)
            {
                await GControlTaskAssigneeService.BatchAddAsync(_gControlWorkflowDtos.ToList());
            }
            
            return result;
        }

        private ManualApprovalFSAuditRecordDto BuildAuditRecords(ManualApprovalItemActionResult resNode, ManualApprovalRecord node, 
            string approvalComment) {
            ManualApprovalFSAuditRecordDto record = new ManualApprovalFSAuditRecordDto();
            record.NodeId = node.Id;
            record.NodeName = node.LeafName;
            record.AuditLevel =  (int)FSAuditLevel.File;
            record.ConnectionId = node.L2PartitionKey;
            if (!string.IsNullOrEmpty(record.ConnectionId)) {
                FSConnection conn = FSConnectionDao.GetConnectionById(Guid.Parse(record.ConnectionId));
                if (conn != null && conn.GroupId != null) {
                    record.ConnectionGroupId = conn.GroupId.ToString();
                }
            }
            record.FullPath = resNode.EffectItemFullPath;
            record.Content = approvalComment;
            if (resNode.IsSucceed) {
                record.Status = (int)AuditStatus.Successful;
            }
            else {
                record.Status = (int)AuditStatus.Failed;
            }
            if (_approvalStatus == SOApproveDBStatus.Approved)
            {
                record.ActionType = SOApproveDBStatus.Approved;
            }
            else {
                record.ActionType = SOApproveDBStatus.Rejected;
            }
            return record;
        }

        private async Task<ManualApprovalItemActionResult> ApprovalOrRejectAsync(ManualApprovalRecord item , ManualApprovalActionParams approveParameters )
        {
            try
            {
                var extendTime = await CalculationExtendTimeAsync(approveParameters);

                var result = new ManualApprovalItemActionResult
                {
                    IsSucceed = await CheckApproveParameters(extendTime, item, approveParameters),
                    OldValue = approveParameters.FromGControl ? (SOApproveDBStatus)item.GControlManualApprovedStatus :(SOApproveDBStatus)item.ManualApprovedStatus,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath,
                    ExtendType = approveParameters.ExtendType,
                    ExtendTime = approveParameters.CustomeExtendDate,
                    Id = item.Id
                };

                if (result.IsSucceed)
                {
                    var isWorkflow = approveParameters.FromGControl ?  item.GControlCurrentStageId != Guid.Empty.ToString() && item.GControlApprovalProcessId != Guid.Empty.ToString() 
                        : item.ManualWorkflowDefinitionId != Guid.Empty && item.ManualWorkflowStepId != Guid.Empty;
                    if (isWorkflow)
                    {
                        await ApprovalOrRejectForWorkflowAsync(item, approveParameters, extendTime);
                    }
                    else
                    {
                        await ApprovalOrRejectForOwnerAsync(item, approveParameters, extendTime);
                    }
                }

                return result;

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while [{_approvalStatus}] item: [{item.Id}]. Error: {e}");
                return new ManualApprovalItemActionResult
                {
                    IsSucceed = false,
                    Message = e.Message,
                    OldValue = (SOApproveDBStatus)item.ManualApprovedStatus,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath,
                };
            }
        }

        private async System.Threading.Tasks.Task ApprovalOrRejectForWorkflowAsync(ManualApprovalRecord item, ManualApprovalActionParams approveParameters,long extendTime)
        {
            bool isGControlAction = approveParameters.FromGControl;
                
            var workflowDefinitionId = isGControlAction ? new Guid(item.GControlApprovalProcessId) : item.ManualWorkflowDefinitionId;
            var workflowStepId =  isGControlAction ? new Guid(item.GControlCurrentStageId) : item.ManualWorkflowStepId;
            if(NeedUpgradeWorkflow(item))   
            {
                Logger.Info($"Start upgrade workflow for item: [{item.Id}]");
                var instance = await RMWorkflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);  
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;

                Logger.Info($"Upgrade workflow instance for item: [{item.Id}] to workflowDefinitionId: [{workflowDefinitionId}] and workflowStepId: [{workflowStepId}]");
                await WorkflowInstanceDao.UpdateStatusAsync(instance.Id, RMWorkflowStatus.Completed);
            }

            Logger.Info($"Load workflow instance for item: [{item.Id}] with workflowDefinitionId: [{workflowDefinitionId}] and workflowStepId: [{workflowStepId}]");
            var workflowInstance = isGControlAction switch
            {
                true => await _workflowProcessor.LoadFromGControlAsync(workflowDefinitionId),
                _ => await _workflowProcessor.LoadAsync(workflowDefinitionId)
            };
            var currentStep = workflowInstance.LoadStep(workflowStepId);
            
            var nextStep = currentStep;
            Logger.Info($"{(_approvalStatus == SOApproveDBStatus.Approved ? "Approving" : "Rejecting")} item: [{item.Id}] in workflow, current step: [{currentStep.Id}]");
            if (_approvalStatus == SOApproveDBStatus.Approved)
            {
                nextStep = currentStep.Approve();
                approveParameters.QuickReason = string.Empty;
            }
            else
            {
                nextStep = currentStep.Reject();
            }
            item.ManualApprovalComment = approveParameters.ApprovalComment;
            item.QuickReason = approveParameters.QuickReason;
            item.ManualLastReasonForRejection = string.Empty;

            Logger.Info($"Start converting history data for item: [{item.Id}]");
            var historyData = _historyAddAction.Convert(item, _approvalStatus, _approvalAccount.Id);
            if (isGControlAction)
            {
                Logger.Info($"Updating workflow stage for GControl item: [{item.Id}] to next step: [{nextStep.Id}]");
                item.GControlCurrentStageId = _approvalStatus == SOApproveDBStatus.Approved
                    ? nextStep.Id.ToString()
                    : item.GControlCurrentStageId;
            }
            else
            {
                Logger.Info($"Updating workflow step for item: [{item.Id}] to next step: [{nextStep.Id}]");  
                item.ManualWorkflowStepId = nextStep.Id;
            }
            item.ManualApprovedBy = _approvalAccount.Id;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            item.ManualEscalatedComment = string.Empty;

            item.ManualLastApproveRejectComment = approveParameters.ApprovalComment;
            item.ManualLastReviewedBy = _approvalAccount.DisplayName;
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;

            if (!nextStep.IsEnd)
            {
                Logger.Info("Setting reviewers for next step.");
                if (isGControlAction)
                {
                    var reviewers = _approvalStatus == SOApproveDBStatus.Approved
                        ? await nextStep.GetReviewersAsync(item.ScopeId)
                        : await currentStep.GetReviewersAsync(item.ScopeId);
                    item.GControlCurrentApproverId = reviewers[0].UserId;
                    item.GControlManualReviewers = [];
                }
                else if (item.IsFsControlRecordJPMC)
                {
                    item.ManualReviewer = (await nextStep.GetReviewersAsync(new Guid(item.AveSiteId))).Select(item => item.RMUserId).ToArray();
                }
                else
                {
                    item.ManualReviewer = (await nextStep.GetReviewersAsync(item.ScopeId)).Select(item => item.RMUserId).ToArray();
                }
                item.ManualEscalateFrom = 0;

                item.ManualLastExtendType = approveParameters.ExtendType;
                item.ManualLastCustomeExtendDate = approveParameters.CustomeExtendDate;

                if (_approvalStatus == SOApproveDBStatus.Approved)
                {
                    item.ManualLastExtendType = ManualApprovalExtendType.After1Month;
                }
            }

            bool isRejectedInControlPlus = isGControlAction && _approvalStatus == SOApproveDBStatus.Rejected;
            if (nextStep.IsEnd || isRejectedInControlPlus)
            {
                if (isGControlAction)
                {
                    item.GControlManualApprovedStatus = (int)_approvalStatus;
                    item.GControlManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                }
                else
                {
                    item.ManualApprovedStatus = (int)_approvalStatus;
                    item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                }
                item.ManualLastReasonForRejection = item.QuickReason;
                await _syncArchiverStatusAction.UpdateItemArchiverStatusAsync(item);
                if (_approvalStatus == SOApproveDBStatus.Rejected  && approveParameters.ExtendType != ManualApprovalExtendType.None) 
                {
                    item.ManualExtendTime = extendTime;
                    item.ManualExtendComment = string.Empty;
                    item.ManualExtendCount += 1;
                }
            }
          
            await RebuildAuditsAsync(item, item.ManualApprovalComment, item.QuickReason, approveParameters.ExtendType, approveParameters.CustomeExtendDate, approveParameters.ManualFromTab);

            if(!nextStep.IsEnd && item.ManualNeedEmailNotification)
            {
                Logger.Info($"Adding email notification for item: [{item.Id}] as next step is not end and email notification is needed.");
                var emailItem = new RMEmailItem
                {
                    Id = item.Id,
                    Status = RMSendEmailStatus.WaittingSendEmail,
                    ModifyTime = DateTime.UtcNow
                };
                await EmailItemDao.AddWorkflowManualItemAsync(emailItem);
            }

            if ((!nextStep.IsEnd || item.SourceFlag > (int)SourceFlag.Connector) && !isRejectedInControlPlus)
            {
                Logger.Info($"Adding history record for item: [{item.Id}] as next step is not end or source flag is greater than Connector, and it's not rejected in GControl.");
                await _historyAddAction.AddAsync(historyData);
            }

            Logger.Info($"Updating item: [{item.Id}].");
            await _repository.UpsertItemAsync(item);
            
            AddNewWorkflowDto(isGControlAction, workflowDefinitionId, workflowStepId);
        }

        private void AddNewWorkflowDto(bool isGControlAction, Guid workflowDefinitionId, Guid workflowStepId)
        {
            if (isGControlAction)
            {
                _gControlWorkflowDtos.Add(new GControlWorkflowDto
                {
                    WorkflowId = workflowDefinitionId,
                    StageId = workflowStepId,
                    Status = _approvalStatus switch
                    {
                        SOApproveDBStatus.Approved => ApprovalProcessStatus.Approved,
                        _ => ApprovalProcessStatus.Rejected
                    }
                });
            }
        }

        private static bool NeedUpgradeWorkflow(ManualApprovalRecord item)
        {
            return item.ManualWorkflowInstanceId != Guid.Empty && item.ManualWorkflowDefinitionId == Guid.Empty && item.ManualWorkflowStepId == Guid.Empty;
        }

        private async Task ApprovalOrRejectForOwnerAsync(ManualApprovalRecord item, ManualApprovalActionParams approveParameters,long extendTime)
        {

            Logger.Info($"{(_approvalStatus == SOApproveDBStatus.Approved ? "waiting for disposal" : "disposal extensions")}");

            item.ManualApprovalComment = approveParameters.ApprovalComment;
            item.QuickReason = string.Empty;
            item.ManualLastReasonForRejection = string.Empty;
            item.ManualLastExtendType = ManualApprovalExtendType.After1Month;

            if (item.SourceFlag >= 1000)
            {
                var historyData = _historyAddAction.Convert(item, _approvalStatus, _approvalAccount.Id);
                await _historyAddAction.AddAsync(historyData);
            }
            item.ManualInternalApprovedStatus = (int)_approvalStatus;
            item.ManualApprovedStatus = (int)_approvalStatus;
            item.ManualApprovedBy = _approvalAccount.Id;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            if (approveParameters.ManualFromTab == ManualApprovalTab.UnderReview)
            {
                item.ManualLastApproveRejectComment = approveParameters.ApprovalComment;
            }
            item.ManualLastReviewedBy = _approvalAccount.DisplayName;
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
           

            await RebuildAuditsAsync(item, item.ManualApprovalComment, approveParameters.QuickReason, approveParameters.ExtendType, approveParameters.CustomeExtendDate , approveParameters.ManualFromTab);
            if (_approvalStatus == SOApproveDBStatus.Rejected)
            {
                item.QuickReason = approveParameters.QuickReason;
                item.ManualLastReasonForRejection = item.QuickReason;
                if (approveParameters.ExtendType != ManualApprovalExtendType.None) 
                {
                    item.ManualExtendTime = extendTime;
                    item.ManualExtendComment = string.Empty;
                    item.ManualExtendCount += 1;
                }
            }
            await _syncArchiverStatusAction.UpdateItemArchiverStatusAsync(item);
            await _repository.UpsertItemAsync(item);

            Logger.Info($"Succeed [{_approvalStatus}] item: [{item.Id}].");
        }

        private async Task RebuildAuditsAsync(ManualApprovalRecord item, string approvalComment, string quickReason, ManualApprovalExtendType extendType,DateTime customeExtendDate, ManualApprovalTab ManualFromTab)
        {

            var extendTime = extendType switch
            {
                ManualApprovalExtendType.After1Month => DateTime.UtcNow.AddMonths(1),
                ManualApprovalExtendType.After3Month => DateTime.UtcNow.AddMonths(3),
                ManualApprovalExtendType.After6Month => DateTime.UtcNow.AddMonths(6),
                ManualApprovalExtendType.After1Year => DateTime.UtcNow.AddYears(1),
                _ => customeExtendDate,
            };

            var extendSimplifyFormatTime = await  GeneralSettingService.ConvertTiksToDateTimeAsync(extendTime.Ticks , true);
            
            var audits = new List<ReviewAudits>();

            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }

            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = _approvalAccount.DisplayName,
                Action = _approvalStatus == SOApproveDBStatus.Approved ? "RM_MA_Approve" : "RM_MA_ApproveStatus_RejectAndExtend", //"RM_MA_Reject"
                Comment = approvalComment,
                QuickReason = quickReason,
                ExtendTime = ManualFromTab == ManualApprovalTab.WaitDisposal ?  string.Empty : _approvalStatus == SOApproveDBStatus.Approved ? string.Empty : extendSimplifyFormatTime.SimplifyFormatTime.ToString(),
            }) ;

            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }
   
        private static async Task<long> CalculationExtendTimeAsync(ManualApprovalActionParams approveParameters)
        {
            var now = DateTime.UtcNow;
            if (approveParameters.ExtendType == ManualApprovalExtendType.Custom)
            {
                return approveParameters.CustomeExtendDate.Ticks;
            }
            else if (approveParameters.ExtendType == ManualApprovalExtendType.After1Month)
            {
                return now.AddMonths(1).Ticks;
            }
            else if (approveParameters.ExtendType == ManualApprovalExtendType.After3Month)
            {
                return now.AddMonths(3).Ticks;
            }
            else if (approveParameters.ExtendType == ManualApprovalExtendType.After6Month)
            {
                return now.AddMonths(6).Ticks;
            }
            else if (approveParameters.ExtendType == ManualApprovalExtendType.After1Year)
            {
                return now.AddYears(1).Ticks;
            }

            return 0;
        }

        private static async Task<bool> CheckApproveParameters(long extendTime, ManualApprovalRecord item, ManualApprovalActionParams approveParameters)
        {  
            // check extendTime
            if (extendTime <= DateTime.UtcNow.Ticks && approveParameters.ExtendType != ManualApprovalExtendType.None)
            {
                Logger.Error($"Cuttent{item} extendTime <= DateTime.UtcNow.Ticks");
                return false;
            }
            return true;
        }

        private static async Task<bool> PreCheckApporvalItems(List<Guid> itemIds, bool fromGControl)
        {
            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            queryDefinition.FromGControl = fromGControl;
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            if (count != itemIds.Count)
            {
                return false;
            }

            return true;
        }
    }
}
