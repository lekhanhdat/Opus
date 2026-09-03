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
using AvePoint.GCommon.Contract.Server.Login;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Email;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using GOneGlobal.GlobalDomain;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Report;
using RAGoogle.Util;
using RAManualApprovalCommon;
using RAManualApprovalCommon.Model;
using RMWorkflowStep = AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;

namespace RAGoogle.ManualManagement;

public class GoogleManualManagement
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleManualManagement));
    private RecordManager _recordManager;
    private ReportCenter _reportCenter;
    private RMEmailSender _mailSender;
    private RMEmailRedisStorage _mailStorage;
    private RMWorkflowProcessor _workflowProcessor;
    private readonly HistoryAddAction _historyAction = new();
    private GoogleSettingDto _settingInfo;
    private ITenantService _tenantService;
    private bool hasGControlLicense;
    private IGControlPlatformTaskService _gControlPlatformTaskService;
    private IUserService _userService;
    private bool hasInitializedPlatformTask;
    private ILnkUserGroupDao _lnkUserGroupDao;
    private IPeoplePickerService _peoplePickerService;
    private ConcurrentBag<string> _accountCache = new();
    private ConcurrentDictionary<string, List<string>> _groupUserMapping = [];
    private IAccountDao _accountDao;

    public GoogleManualManagement Build(RecordManager recordManager, ReportCenter reportCenter, string jobId)
    {
        jobId = jobId.Substring(0, jobId.LastIndexOf('_')); //get parent's jobId to ensure all subjobs using the same storage configuration.
        _recordManager = recordManager;
        _mailStorage = new RMEmailRedisStorage(jobId, new RMEMailStorageManualMiddleware());
        _mailSender = new RMEmailSender(_mailStorage);
        _workflowProcessor = new RMWorkflowProcessor();
        _reportCenter = reportCenter;
        _tenantService = PlatformWindsorManager.GetService<ITenantService>();
        _gControlPlatformTaskService = PlatformWindsorManager.GetService<IGControlPlatformTaskService>();
        _userService = PlatformWindsorManager.GetService<IUserService>();
        _lnkUserGroupDao = PlatformWindsorManager.GetService<ILnkUserGroupDao>();
        _peoplePickerService = new PeoplePickerService();
        hasGControlLicense = _tenantService.HasInitGControlPlatForm().Result;
        _accountDao = PlatformWindsorManager.GetService<IAccountDao>();
        return this;
    }
    public GoogleManualManagement Build(RecordManager recordManager, ReportCenter reportCenter, string jobId, GoogleSettingDto settingInfo)
    {
        _settingInfo = settingInfo;
        return Build(recordManager, reportCenter, jobId);
    }
    public async Task<bool> IsNeedProcessManualDisposalAsync(Rule rule, Record existRecord)
    {
        if (rule is null || existRecord is null)
        {
            return false;
        }
        return await IsNeedProcessManualDisposalAsync(rule, _settingInfo, existRecord);
    }
    public async Task<bool> IsNeedProcessManualDisposalAsync(Rule? rule, GoogleSettingDto settingInfo, Record existRecord)
    {
        if (rule is { GoogleDriveRule: not null } && existRecord.RuleId.ToString().Equals(rule.Id))
        {
            ProcessManualResult processManualResult;

            if (!rule.GoogleDriveRule.IsManualApproval)
            {
                if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    _logger.Info($"Item:{existRecord.ItemId} match manual rule, New rule id:{rule.Id},and it is process Approval Data Only");
                    return true;
                }
                if (existRecord.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                {
                    existRecord.RemoveManualProperties();
                    _logger.Info($"The rule [{rule.Id}] of item [{existRecord.Id}] has disabled manual approval.");
                    _recordManager.UpdateManualProperties(existRecord);
                }
                return false;
            }

            existRecord.ManualFullPath = existRecord.DirPath;
            existRecord.ManualFolderPath = string.Empty;

            if (existRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
            {
                _logger.Warn($"Item {existRecord.Id} is in disposal extended time.");
                return true;
            }

            if (!string.IsNullOrEmpty(rule.GoogleDriveRule.WorkflowId))
            {
                if (!rule.GoogleDriveRule.IsGControlManualApproval)
                {
                    var workflowDefinition = ManualApprovalWorkflowManager.Get(rule.GoogleDriveRule.WorkflowId);
                    if (workflowDefinition == null)
                    {
                        _logger.Warn("The approval process this item is subject to does not exist. Try to get workflowid from record. WorkflowId: {0}", rule.GoogleDriveRule.WorkflowId);
                        var oldWorkflowDefinitionId = existRecord.ManualWorkflowDefinitionId;

                        if (!oldWorkflowDefinitionId.Equals(Guid.Empty) && oldWorkflowDefinitionId.ToString() != rule.GoogleDriveRule.WorkflowId)
                        {
                            workflowDefinition = ManualApprovalWorkflowManager.Load(oldWorkflowDefinitionId.ToString());
                        }

                        if (workflowDefinition == null)
                        {
                            _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoWorkflow), existRecord.NodeType);
                            return true;
                        }
                    }
                }
                else
                {
                    var workflow =  hasGControlLicense ? ManualApprovalWorkflowManager.GetGControlWorkflow(rule.GoogleDriveRule.WorkflowId) : null;

                    if (workflow == null)
                    {
                        _logger.Warn("The google control approval process this item is subject to does not exist. Try to get workflow id from record. WorkflowId: {0}", rule.GoogleDriveRule.WorkflowId);
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoWorkflow), existRecord.NodeType);
                        return true;
                    }
                }
               
            }

            var isGControlAction = rule.GoogleDriveRule.IsGControlManualApproval;
            
            var approvedStatus = isGControlAction
                ? existRecord.GControlManualApprovedStatus
                : existRecord.ManualApprovedStatus;

            switch (approvedStatus)
            {
                case (int)SOApproveDBStatus.WaitingApprove:
                    _logger.Warn($"Item {existRecord.Id} is waiting for approval.");
                    processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                    break;
                case (int)SOApproveDBStatus.Approved:
                    _logger.Info($"Item: {existRecord.Id} match manual rule, and approve status is approved.");
                    await AddManualActionHistory(existRecord);
                    return false;
                default:
                    {
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            _logger.Info($"Item:{existRecord.ItemId} match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
                            return true;
                        }
                        switch (approvedStatus)
                        {
                            case (int)SOApproveDBStatus.Rejected:
                                _logger.Info($"Item:{existRecord.Id} match manual rule, and approve status is rejected.");
                                await AddManualActionHistory(existRecord);
                                processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                                break;
                            case (int)SOApproveDBStatus.None:
                                _logger.Info($"Item: {existRecord.Id} match manual rule, and approve status is none.");
                                processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                                break;
                            default:
                                _logger.Info($"Item: {existRecord.Id} match manual rule, but approve status is unknow. Skip manual action.");
                                return false;
                        }

                        break;
                    }
            }

            if (processManualResult?.IsSuccess ?? false)
            {
                _reportCenter.RecordSkipCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.WaitingForDisposal), existRecord.NodeType);
                return true;
            }

            switch (processManualResult?.ErrorType)
            {
                case ProcessManualErrorType.NoOwnerError:
                    _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoRecordOwner), existRecord.NodeType);
                    break;
                case ProcessManualErrorType.WorkflowNoSiteOwner:
                    _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoSupportSiteOwner), existRecord.NodeType);
                    break;
                default:
                    _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.UnexpectedException), existRecord.NodeType);
                    break;
            }
        }
        return true;
    }

    private async Task<ProcessManualResult> UpdateWaitingStatusAsync(Record record, GoogleSettingDto settingInfo, Rule? rule)
    {
        ProcessManualResult result = new();
        record.ManualModifiedTime = record.TimeModified;
        _logger.Info($"Process update waiting status for item [{record.Id}] with approval status [{(SOApproveDBStatus)record.ManualApprovedStatus}]");
        try
        {
            var isProcessByOwners = string.IsNullOrEmpty(rule?.GoogleDriveRule.WorkflowId);
            var newRec = await ProcessWaitingForApprovalRecordAsync(record, settingInfo, rule);

            if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
            {
                result.IsSuccess = false;
                result.ErrorType = ProcessManualErrorType.NoOwnerError;
            }
            else
            {
                _recordManager.UpdateManualProperties(newRec);
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains(I18NResource.NoRecordOwner))
            {
                result.IsSuccess = false;
                result.ErrorType = ProcessManualErrorType.NoOwnerError;
            }
            else if (e.Message.Contains(I18NResource.NoSupportSiteOwner))
            {
                result.IsSuccess = false;
                result.ErrorType = ProcessManualErrorType.WorkflowNoSiteOwner;
            }
            else
            {
                _logger.Error($"Error occured while updating waiting status for manual approval. error: {e}");
                throw;
            }
        }
        return result;
    }

    private async Task AddManualActionHistory(Record record)
    {
        record.ManualArchivedTime = DateTime.UtcNow.Ticks;
        record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd;

        var approvedStatus =
            record.IsGControlRecord ? record.GControlManualApprovedStatus : record.ManualApprovedStatus;
        
        var historyData = _historyAction.Convert(
            record,
            (SOApproveDBStatus) approvedStatus, 
            record.ManualApprovedBy,
            record.ManualActionTime
        );

        await _historyAction.Add(historyData);
    }
    private async Task<Record> ProcessWaitingForApprovalRecordAsync(Record record, GoogleSettingDto settingInfo, Rule? rule)
    {
        var sourceFlag = SourceFlag.Google;
        if (!ManualApprovalRuleInfoManager.TryGet(sourceFlag, rule.Id, out var ruleInfo))
        {
            throw new Exception(I18NResource.RuleIsDeleted);
        }

        if (settingInfo.ApprovalType != (int)AvePoint.RA.DB.Model.ApprovalType.None)
        {
            _logger.Info($"Item [{record.Id}] is under the node that enable manual approval setting");
            ruleInfo.WorkflowId = settingInfo.WorkflowReferenceId;
            ruleInfo.IsSendEmailToOwner = settingInfo.EmailToRecordOwner;
            ruleInfo.Owners = settingInfo.RecordOwner.ConvertAll(o => new UserInfo
            {
                DisplayName = o.DisplayName,
                Email = o.Email,
                Id = o.Id,
                InviteType = (InviteType)o.InviteType,
                TenantId = o.TenantId,
                UserId = o.UserId,
                UserPrincipalName = o.UserPrincipalName
            });
        }

        var approvedStatus = ruleInfo.IsGControlWorkflow
            ? record.GControlManualApprovedStatus
            : record.ManualApprovedStatus;

        var manualInternalApprovedStatus = ruleInfo.IsGControlWorkflow ? record.GControlManualApprovedStatus : record.ManualInternalApprovedStatus;

        if (approvedStatus != (int)SOApproveDBStatus.WaitingApprove)
        {
            AssignManualProperties(record, ruleInfo);
        }
        else if (manualInternalApprovedStatus is (int)SOApproveDBStatus.WaitingApprove or (int)SOApproveDBStatus.WorkflowInProgress)
        {
            return record;
        }

        if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
        {
            _logger.Info($"Item [{record.Id}] has approval type: {AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess.ToString()}");
            await ProcessByWorkflowAsync(record, ruleInfo);
        }
        else
        {
            ProcessByOwners(record, ruleInfo);
        }

        return record;
    }

    private async Task ProcessByWorkflowAsync(Record record, ManualApprovalRuleModel ruleInfo)
    {
        var workflowDefinition = ruleInfo.IsGControlWorkflow switch
        {
            true =>  ManualApprovalWorkflowManager.GetGControlWorkflow(ruleInfo.WorkflowId),
            _ => ManualApprovalWorkflowManager.Get(ruleInfo.WorkflowId)
        };
        var workflowInstance = ruleInfo.IsGControlWorkflow switch
        {
            true => await _workflowProcessor.LoadFromGControlAsync(workflowDefinition.Id),
            _ => await _workflowProcessor.LoadAsync(workflowDefinition.Id)
        };

        var driveOwnersStepNode = workflowDefinition.Content.WorkflowNodes.FirstOrDefault(item => item.ReviewerType == WorkflowReviewerType.SiteOwners);
        if (driveOwnersStepNode != null)
        {
            throw new Exception(I18NResource.NoSupportSiteOwner);
        }

        RMWorkflowStep step;
        var manualInternalApprovedStatus = ruleInfo.IsGControlWorkflow ? record.GControlManualApprovedStatus : record.ManualInternalApprovedStatus;
        var workflowDefinitionId = ruleInfo.IsGControlWorkflow ? new Guid(record.GControlApprovalProcessId) : record.ManualWorkflowDefinitionId;
        if (manualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress && workflowDefinitionId == workflowDefinition.Id)
        {
            var stepId = ruleInfo.IsGControlWorkflow ? new Guid(record.GControlCurrentStageId) : record.ManualWorkflowStepId;
            step = workflowInstance.LoadStep(stepId);
        }
        else
        {
            _logger.Info($"Item [{record.Id}] has changed approval process: {workflowDefinition.Id}. Restart process");
            step = workflowInstance.Start();
        }
        var reviewers = await step.GetReviewersAsync(new Guid(record.ContainerId));

        var templateId = step.UsedEmailTemplateId;
        if (step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
        {
            var customIntervalSetting = step.CustomIntervalSettings[0];
            if (customIntervalSetting == null)
            {
                templateId = RMEmailTemplateId.MANUAL_APPROVAL;
            }
            else
            {
                templateId = new Guid(customIntervalSetting.UsedEmailTemplateId);
                if (templateId == Guid.Empty)
                {
                    templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                }
            }
        }

        if (ruleInfo.IsSendEmailToOwner && !ruleInfo.IsGControlWorkflow)
        {
            var emailTemplateParameters = new List<RMEmailTemplateParameters>();

            var existingParameters = _mailStorage.GetParameters(RMEmailTemplateId.MANUAL_APPROVAL);
            
            reviewers.ForEach(item =>
            {
                if (!existingParameters.Any(p => p.ToUser.Equals(item.UserPrincipalName, StringComparison.OrdinalIgnoreCase)))
                {
                    emailTemplateParameters.Add(new RMManualEmailTemplateParameters
                    {
                        UserId = item.UserId,
                        ToUser = item.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual,
                        RequestComment = ""
                    });
                };
            });

            _mailSender.AddRange(templateId, emailTemplateParameters);
        }
        
        if (ruleInfo.IsGControlWorkflow)
        {
            record.IsGControlRecord = true;
            record.GControlApprovalProcessId = workflowDefinition.Id.ToString();
            record.GControlCurrentStageId = step.Id.ToString();
            record.GControlCurrentApproverId = reviewers[0].UserId;
            record.GControlManualReviewers = [];
            record.GControlManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            record.GControlPlatformTaskId = _gControlPlatformTaskService.GetTaskId().ToString();
            await HandleGControlUser(record, templateId, ruleInfo);
        }
        else
        {
            record.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            record.ManualWorkflowDefinitionId = workflowDefinition.Id;
            record.ManualWorkflowStepId = step.Id;
        }
    }

    private async Task HandleGControlUser(Record record, Guid templateId, ManualApprovalRuleModel ruleInfo)
    {
        if (!hasInitializedPlatformTask)
        {
            await _gControlPlatformTaskService.CreateOpusTask();
            hasInitializedPlatformTask = true;
        }
        if(!_accountCache.Contains(record.GControlCurrentApproverId))
        {
            var dbAccount = (await _accountDao.GetGoogleUserByUserIdsAsync([record.GControlCurrentApproverId])).FirstOrDefault();
            if (dbAccount != null)
            {
                if (dbAccount.ObjectType == RMActiveDirectoryObjectType.Group)
                {
                    var memberIds = await _peoplePickerService.GetGroupUserIdsAsync(dbAccount.AADId);
                    if(!_groupUserMapping.ContainsKey(dbAccount.AADId))
                    {
                        var dbGroupUsers= await _accountDao.GetExistGoogleUserIdsAsync(memberIds);
                        await _lnkUserGroupDao.AddUsersInGroupAsync(dbGroupUsers.Select(user => user.Item2), dbAccount.AADId);
                        _groupUserMapping.TryAdd(dbAccount.AADId, memberIds);
                    }
                }
                CacheEmail(ruleInfo.IsSendEmailToOwner, templateId, dbAccount.AADId, dbAccount.UserPrincipalName);
            }
            else
            { 
                await QueryGoogleAccount(record.GControlCurrentApproverId, ruleInfo, templateId);
            }

            _accountCache.Add(record.GControlCurrentApproverId);
        }

        List<string> approverIds = _groupUserMapping.TryGetValue(record.GControlCurrentApproverId, out var assignees)
            ? [..assignees, record.GControlCurrentApproverId]
            : [record.GControlCurrentApproverId];
        await AddTaskAssigneesMapping(new Guid(record.GControlApprovalProcessId), approverIds,
            new Guid(record.GControlCurrentStageId));
    }

    private async Task QueryGoogleAccount(string userId, ManualApprovalRuleModel ruleInfo, Guid templateId)
    {
        var (account, members) = await _peoplePickerService.GetDirectoryAndUsersInGroupTypeDirectoryAsync(userId);
        if(account != null)
        {
            var neededAddAccounts = new List<AccountDto>() { account };
            if (account.ObjectType == RMActiveDirectoryObjectType.Group && members.IsNotNullOrEmpty())
            {
                neededAddAccounts.AddRange(members);
                await _lnkUserGroupDao.AddUsersInGroupAsync(members.Select(item => item.UserId), account.UserId);
                _groupUserMapping.TryAdd(account.UserId, members.Select(item => item.UserId).ToList());
            }
            await _userService.BatchAddAccountsAsync(neededAddAccounts);
            _userService.SaveUsersToBuiltInGroup(new List<string>() { userId });
            CacheEmail(ruleInfo.IsSendEmailToOwner, templateId, account.UserId, account.UserPrincipalName);
        }
    }

    private void CacheEmail(bool isSendEmailToOwner, Guid templateId, string userId, string upn)
    {
        if(isSendEmailToOwner && templateId != Guid.Empty)
        {
            var userParameter = new RMManualEmailTemplateParameters
            {
                UserId = userId,
                ToUser = upn,
                TemplateType = RMEmailTemplateType.Manual,
                RequestComment = ""
            };
            _mailSender.AddGControlTemplate(templateId, userParameter);
        }
    }

    private async Task AddTaskAssigneesMapping(Guid approvalProcessId, List<string> approverIds, Guid stageId)
    {
        foreach (var approverId in approverIds)
        {
            var needToSaveUserTaskMapping = new GControlWorkflowDto()
            {
                WorkflowId = approvalProcessId,
                ApproverId = approverId,
                StageId = stageId,
                Status = ApprovalProcessStatus.Pending,
            };
            await ManualApprovalWorkflowManager.CacheGControlWorkflowIdAndStageIdMapping(needToSaveUserTaskMapping);
        }
    }

    private void ProcessByOwners(Record record, ManualApprovalRuleModel ruleInfo)
    {
        _logger.Info($"Item [{record.Id}] has approval type: {AvePoint.RA.DB.Model.ApprovalType.RecordOwners.ToString()}");
        record.ManualWorkflowInstanceId = Guid.Empty;
        record.ManualWorkflowDefinitionId = Guid.Empty;
        record.ManualWorkflowStepId = Guid.Empty;
        record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
        record.ManualReviewer = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners).ToArray();
        if (ruleInfo.IsSendEmailToOwner)
        {
            var emailTemplateParameters = new List<RMEmailTemplateParameters>();

            var existingParameters = _mailStorage.GetParameters(RMEmailTemplateId.MANUAL_APPROVAL);

            ruleInfo.Owners.ForEach(item =>
            {
                if (!existingParameters.Any(p => p.ToUser.Equals(item.UserPrincipalName, StringComparison.OrdinalIgnoreCase)))
                {
                    emailTemplateParameters.Add(
                    new RMManualEmailTemplateParameters
                    {
                        UserId = item.UserId,
                        ToUser = item.UserPrincipalName,
                        RequestComment = record.Comment,
                        TemplateType = RMEmailTemplateType.Manual
                    });
                }
            });

            _mailStorage.AddRange(RMEmailTemplateId.MANUAL_APPROVAL, emailTemplateParameters);
        }
    }

    private void AssignManualProperties(Record record, ManualApprovalRuleModel ruleInfo)
    {
        record.ManualEmailNotificationCount = 0;
        record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
        record.ManualNeedEmailNotification = ruleInfo.IsSendEmailToOwner;
        if (record.ManualApprovedStatus == (int)SOApproveDBStatus.None)
        {
            record.ManualExtendTime = 0;
            record.ManualExtendCount = 0;
            record.ManualExtendComment = string.Empty;
            record.RecordStatus = string.IsNullOrEmpty(record.RecordsId) ? (int)RMRecordStatus.ManualPreSync : (int)RMRecordStatus.Active;
        }
        record.ManualEscalateFrom = 0;
        record.ManualEscalatedComment = string.Empty;
        record.ManualIsAutoReassigned = false;
        record.IsManualSynced = true;
        record.ManualRuleName = ruleInfo.RuleName;
        record.ManualRuleCriteria = ruleInfo.RuleCriterias;
        record.ManualRuleDisposalClass = ruleInfo.RuleDisposalClass;
        record.ManualCollectionTime = DateTime.UtcNow.Ticks;
        record.ManualWorkflowInstanceId = Guid.Empty;
        record.ManualWorkflowDefinitionId = Guid.Empty;
        record.ManualWorkflowStepId = Guid.Empty;
        if (ruleInfo.IsGControlWorkflow)
        {
            record.GControlManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
        }
        else
        {
            record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
        }
        record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
        record.ManualArchivedTime = 0;
        record.GControlApprovalProcessId = Guid.Empty.ToString();
        record.GControlPlatformTaskId = Guid.Empty.ToString();
        record.GControlCurrentStageId = Guid.Empty.ToString();
        record.GControlCurrentApproverId = string.Empty;
        record.GControlManualReviewers = [];
        record.IsGControlRecord = ruleInfo.IsGControlWorkflow;
    }
}