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
using AvePoint.GCommon.Contract.Server.Login;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Email;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.Wrapper.Common;
using RABox.Converters;
using RABox.Util;
using RAManualApprovalCommon;
using RAManualApprovalCommon.Model;

namespace RABox.Disposal
{
    public class BoxManualManagement
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(BoxManualManagement));
        private RecordManager _recordManager;
        private ReportCenter _reportCenter;
        private RMEmailSender _mailSender;
        private RMEmailRedisStorage _mailStorage;
        private RMWorkflowProcessor _workflowProcessor;
        private readonly HistoryAddAction _historyAction = new();

        public BoxManualManagement Build(RecordManager recordManager, ReportCenter reportCenter, string jobId)
        {
            jobId = jobId.Substring(0, jobId.LastIndexOf('_')); //get parent's jobId to ensure all subjobs using the same storage configuration.
            _recordManager = recordManager;
            _mailStorage = new RMEmailRedisStorage(jobId, new RMEMailStorageManualMiddleware());
            _mailSender = new RMEmailSender(_mailStorage);
            _workflowProcessor = new RMWorkflowProcessor();
            _reportCenter = reportCenter;
            return this;
        }

        public async Task<bool> IsNeedProcessManualDisposalAsync(Rule rule, BoxSettingDto settingInfo, Record existRecord)
        {
            if (rule != null && rule.BoxRule != null && existRecord.RuleId.ToString().Equals(rule.Id))
            {
                ProcessManualResult processManualResult = new();

                if (!rule.BoxRule.IsManualApproval)
                {
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        _logger.Info($"Item:{existRecord.ItemId} match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
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

                if (settingInfo.ApprovalType == (int)AvePoint.RA.DB.Model.ApprovalType.AutoApproval)
                {
                    _logger.Info($"Item [{existRecord.Id}] has set auto approval");
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        if (existRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                        {
                            _logger.Info($"Item: {existRecord.Id} match manual rule, and approve status is approved,IsProcessApprovalDatasOnly is true.");
                            await AddManualActionHistory(existRecord);
                            existRecord.ManualModifiedTime = existRecord.TimeModified;
                            return false;
                        }
                        else
                        {
                            _logger.Info($"Item:{existRecord.ItemId} match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
                            return true;
                        }
                    }
                    if(existRecord.ManualInternalApprovedStatus == (int)SOApproveDBStatus.None ||
                       existRecord.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                    {
                        return false;
                    }
                    else if(existRecord.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        var workflowStepNodes = ManualApprovalWorkflowManager.Load(existRecord.ManualWorkflowDefinitionId.ToString()).Content.WorkflowNodes;
                        var hasOnlyOneReviewLayer = workflowStepNodes.Where(i => i.Reviewers.Any()).ToList().Count == 1;
                        var targetStep = workflowStepNodes.FirstOrDefault(n => n.Id == existRecord.ManualWorkflowStepId);

                        if (hasOnlyOneReviewLayer)
                        {
                            if(targetStep?.NodeType != WorkflowNodeType.BeginDisposalReview)
                            {
                                existRecord.ManualApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                                existRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                                existRecord.ManualActionTime = DateTime.UtcNow.Ticks;
                                existRecord.ManualModifiedTime = existRecord.TimeModified;
                                await AddManualActionHistory(existRecord);
                                _recordManager.UpdateManualProperties(existRecord);
                            }
                            return false;
                        }

                        //Check if the manual data is not approved/rejected at first user layer in approval process with multi layers
                        if (targetStep?.NodeType == WorkflowNodeType.BeginDisposalReview)
                        {
                           return false;
                        }
                    }

                    existRecord.ManualApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                    existRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                    existRecord.ManualActionTime = DateTime.UtcNow.Ticks;
                    existRecord.ManualModifiedTime = existRecord.TimeModified;
                    await AddManualActionHistory(existRecord);
                    _recordManager.UpdateManualProperties(existRecord);
                    return false;
                }
                 
                if (existRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                {
                    _logger.Warn($"Item {existRecord.Id} is in disposal extended time.");
                    return true;
                }

                if (settingInfo.ApprovalType != (int)AvePoint.RA.DB.Model.ApprovalType.None && !string.IsNullOrEmpty(settingInfo.WorkflowReferenceId))
                {
                    var workflowDefinition = ManualApprovalWorkflowManager.Get(settingInfo.WorkflowReferenceId);
                    if (workflowDefinition == null)
                    {
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoWorkflow), existRecord.NodeType);
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(rule.BoxRule.WorkflowId))
                {
                    var workflowDefinition = ManualApprovalWorkflowManager.Get(rule.BoxRule.WorkflowId);
                    if (workflowDefinition == null)
                    {
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoWorkflow), existRecord.NodeType);
                        return true;
                    }
                }

                if (existRecord.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                {
                    _logger.Warn($"Item {existRecord.Id} is waiting for approval.");
                    existRecord.ManualModifiedTime = existRecord.TimeModified;
                    processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                }
                else if (existRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                {
                    _logger.Info($"Item: {existRecord.Id} match manual rule, and approve status is approved.");
                    await AddManualActionHistory(existRecord);
                    existRecord.ManualModifiedTime = existRecord.TimeModified;
                    return false;
                }
                else if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    _logger.Info($"Item:{existRecord.ItemId} match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
                    return true;
                }
                else if (existRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                {
                    _logger.Info($"Item:{existRecord.Id} match manual rule, and approve status is rejected.");
                    await AddManualActionHistory(existRecord);
                    existRecord.ManualModifiedTime = existRecord.TimeModified;
                    processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                }
                else if (existRecord.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                {
                    _logger.Info($"Item: {existRecord.Id} match manual rule, and approve status is none.");
                    existRecord.ManualModifiedTime = existRecord.TimeModified;
                    processManualResult = await UpdateWaitingStatusAsync(existRecord, settingInfo, rule);
                }
                else
                {
                    _logger.Info($"Item: {existRecord.Id} match manual rule, but approve status is unknow. Skip manual action.");
                    return false;
                }

                if (processManualResult?.IsSuccess ?? false)
                {
                    _reportCenter.RecordSkipCommon(existRecord.GenerateDisposalActionJobDetail(I18NEntity.GetString(I18NResource.RemoveAndDestroyAction), rule.Name, I18NResource.WaitingForDisposal), existRecord.NodeType);
                    return true;
                }
                else
                {
                    if (processManualResult?.ErrorType == ProcessManualErrorType.NoOwnerError)
                    {
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NoRecordOwner), existRecord.NodeType);
                    }
                    else if (processManualResult?.ErrorType == ProcessManualErrorType.WorkflowNoSiteOwner)
                    {
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NotFoundSiteOwner), existRecord.NodeType);
                    }
                    else
                    {
                        _reportCenter.RecordFailedCommon(existRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.UnexpectedException), existRecord.NodeType);
                    }
                }
            }
            return true;
        }

        private async Task<ProcessManualResult> UpdateWaitingStatusAsync(Record record, BoxSettingDto settingInfo, Rule rule)
        {
            ProcessManualResult result = new ProcessManualResult();
            record.ManualModifiedTime = record.TimeModified;
            _logger.Info($"Process update waiting status for item [{record.Id}] with approval status [{(SOApproveDBStatus)record.ManualApprovedStatus}]");
            try
            {
                var isProcessByOwners = string.IsNullOrEmpty(rule.BoxRule.WorkflowId);
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
                else
                {
                    _logger.Error($"Error occured while updating waiting status for manual approval. error: {e}");
                    throw;
                }
            }   
            return result;
        }

        private async Task<Record> AddManualActionHistory(Record record)
        {
            record.ManualArchivedTime = DateTime.UtcNow.Ticks;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd;

            var historyData = _historyAction.Convert(
                record,
                (SOApproveDBStatus)record.ManualApprovedStatus,
                record.ManualApprovedBy,
                record.ManualActionTime
            );
            await _historyAction.Add(historyData);
            return record;
        }

        private async Task<Record> ProcessWaitingForApprovalRecordAsync(Record record, BoxSettingDto settingInfo, Rule rule)
        {
            var sourceFlag = SourceFlag.Box;
            if (!ManualApprovalRuleInfoManager.TryGet(sourceFlag, rule.Id, out var ruleInfo))
            {
                throw new Exception(I18NResource.RuleIsDeleted);
            }

            if (settingInfo.ApprovalType != (int)AvePoint.RA.DB.Model.ApprovalType.None)
            {
                _logger.Info($"Item [{record.Id}] is under the node that enable manual approval setting");
                ruleInfo.WorkflowId = settingInfo.WorkflowReferenceId;
                ruleInfo.IsSendEmailToOwner = settingInfo.EMailToRecordOwner;
                ruleInfo.Owners = settingInfo.RecordOwner.ConvertAll(o => new UserInfo()
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

            if (record.ManualApprovedStatus != (int)SOApproveDBStatus.WaitingApprove)
            {
                AssignManualProperties(record, ruleInfo);
            }
            else if (record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
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
            var workflowDefinition = ManualApprovalWorkflowManager.Get(ruleInfo.WorkflowId);
            var workflowInstance = await _workflowProcessor.LoadAsync(workflowDefinition.Id);

            var siteOwnersStepNode = workflowDefinition.Content.WorkflowNodes.FirstOrDefault(item => item.ReviewerType == WorkflowReviewerType.SiteOwners);
            if (siteOwnersStepNode != null)
            {
                throw new Exception(I18NResource.NoRecordOwner);
            }

            RMWorkflowStep step;
            if (record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress && record.ManualWorkflowDefinitionId == workflowDefinition.Id)
            {
                step = workflowInstance.LoadStep(record.ManualWorkflowStepId);
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

            if (ruleInfo.IsSendEmailToOwner)
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

            record.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            record.ManualWorkflowDefinitionId = workflowDefinition.Id;
            record.ManualWorkflowStepId = step.Id;
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
            record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
            record.ManualArchivedTime = 0;
        }
    }
}
