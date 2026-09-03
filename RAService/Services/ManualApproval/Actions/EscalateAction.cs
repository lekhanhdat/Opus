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
using AvePoint.RA.Common.Email;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.AccountManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.Model;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class EscalateAction
    {

        private static readonly IRALogger s_logger = RALogger.GetInstance(typeof(EscalateAction));

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        
        private readonly IGControlTaskAssigneeService _taskAssigneeService = PlatformWindsorManager.GetService<IGControlTaskAssigneeService>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly ManualApprovalRecordRepository _repository;

        private readonly RMAccount _actionAccount;

        private readonly RMWorkflowProcessor _workflowProcessor = new();
        
        private readonly HashSet<GControlWorkflowDto> _gControlReviewerQueueList = [];

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;

        public EscalateAction(ManualApprovalRecordRepository repository)
        {
            _repository = repository;
            var accountId = TenantLocalValue.LogonUserId;
            _actionAccount = AccountDao.Find(item => item.UserId == accountId && item.IsRemoved == 0);

            _hasFSLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
        }

        public async System.Threading.Tasks.Task<ManualApprovalActionResult> Escalate(ManualAprovalEscalateDefinition definition)
        {
            try
            {
                (var synced, var accounts) = await TrySyncUsersAsync(definition.ToUsers);
                if (!synced)
                {
                    return new ManualApprovalActionResult
                    {
                        CompletedStatus = ActionCompletedStatus.Failed,
                        Message = I18NEntity.GetString("RM_RegisterUser_Error_Message")
                    };
                }

                var result = new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed,
                };

                var items = await _repository.QueryItemsAsync(item => definition.ItemIds.Contains(item.Id));

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

                var accountIds = accounts.Select(item => item.Id).ToArray();
                var nowTicks = DateTime.UtcNow.Ticks;
                items.ForEach(item =>
                {
                    var reviewers = definition.FromGControl ? item.GControlManualReviewers?.ToHashSet() ?? [] : item.ManualReviewer.ToHashSet();
                    reviewers.UnionWith(accountIds);

                    var itemActionResult = definition.FromGControl ? EscalateOrReassignNexusItem(item, reviewers.ToArray(), nowTicks, definition.Comment, true) : EscalateOrReassignItem(item, reviewers.ToArray(), nowTicks, definition.Comment, true);
                    result.EffectItems.Add(itemActionResult);
                });
                
                if (definition.FromGControl)
                {
                    await _taskAssigneeService.BatchAddAsync(_gControlReviewerQueueList);
                }

                await _repository.UpsertItemsAsync(items);
                if (definition.NeedSendEmail)
                {
                    if(definition.FromGControl)
                    {
                        await GControlSendEmailAsync(items, accounts, definition.Comment);
                    }
                    else
                    {
                        await SendEmailAsync(items, accounts, definition.Comment);
                    }
                }

                return result;
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while execute escalate action for items: [{string.Join(", ", definition.ItemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = e.Message
                };
            }
        }

        public async System.Threading.Tasks.Task<ManualApprovalActionResult> Reassign(ManualAprovalEscalateDefinition definition)
        {
            try
            {
                (var synced, var accounts) = await TrySyncUsersAsync(definition.ToUsers);
                if (!synced)
                {
                    return new ManualApprovalActionResult
                    {
                        CompletedStatus = ActionCompletedStatus.Failed,
                        Message = I18NEntity.GetString("RM_RegisterUser_Error_Message")
                    };
                }

                var result = new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed,
                };

                var items = await _repository.QueryItemsAsync(item => definition.ItemIds.Contains(item.Id));

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

                var accountIds = accounts.Select(item => item.Id).ToArray();
                var nowTicks = DateTime.UtcNow.Ticks;
                items.ForEach(item =>
                {
                    item.ManualEmailNotificationCount = 0;
                    item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                    item.ManualIsAutoReassigned = false;
                    if (definition.FromGControl)
                    {
                        AddNotUsedCurrentUser(item.GControlManualReviewers, item.GControlCurrentApproverId);
                    }
                    var itemActionResult = definition.FromGControl ? EscalateOrReassignNexusItem(item, accountIds, nowTicks, definition.Comment, false) : EscalateOrReassignItem(item, accountIds, nowTicks, definition.Comment, false);
                    result.EffectItems.Add(itemActionResult);
                });
                
                if (definition.FromGControl)
                {
                    await _taskAssigneeService.BatchAddAsync(_gControlReviewerQueueList);
                }
                await _repository.UpsertItemsAsync(items);

                if(definition.NeedSendEmail)
                {
                    if(definition.FromGControl)
                    {
                        await GControlSendEmailAsync(items, accounts, definition.Comment);
                    }
                    else
                    {
                        await SendEmailAsync(items, accounts, definition.Comment);
                    }
                }

                return result;
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while execute reassign action for items: [{string.Join(", ", definition.ItemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = e.Message
                };
            }
        }

        private void AddNotUsedCurrentUser(int[] manualReviewers, string currentApproverId)
        {
            if (manualReviewers.IsNotNullOrEmpty())
            {
                manualReviewers.ForEach(reviewerId =>
                {
                    _gControlReviewerQueueList.Add(new GControlWorkflowDto
                    {
                        ManualReviewerId = reviewerId,
                        Status = ApprovalProcessStatus.RemoveMapping,
                        WorkflowId = Guid.Empty,
                        StageId = Guid.Empty
                    });
                });
            }
            var approverId = AccountDao.GetUserByAADIdAsync(currentApproverId).Result;
            if (approverId != null)
            {
                _gControlReviewerQueueList.Add(new GControlWorkflowDto
                {
                    ManualReviewerId = approverId.Id,
                    Status = ApprovalProcessStatus.RemoveMapping,
                    WorkflowId = Guid.Empty,
                    StageId = Guid.Empty
                });
            }
        }

        private ManualApprovalItemActionResult EscalateOrReassignItem(ManualApprovalRecord item, int[] reviewIds, long nowTicks, string comment, bool isEsclate)
        {
            item.ManualEscalateFrom = _actionAccount.Id;
            item.ManualReviewer = reviewIds;
            item.ManualActionTime = nowTicks;
            item.ManualEscalatedComment = comment;
            item.ManualAudits = ReBuildAudits(item, isEsclate);

            return new ManualApprovalItemActionResult
            {
                IsSucceed = true,
                EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
            };
        }
        
        private ManualApprovalItemActionResult EscalateOrReassignNexusItem(ManualApprovalRecord item, int[] reviewIds, long nowTicks, string comment, bool isEsclate)
        {
            item.ManualEscalateFrom = _actionAccount.Id;
            item.GControlManualReviewers = reviewIds;
            item.ManualActionTime = nowTicks;
            item.ManualEscalatedComment = comment;
            item.ManualAudits = ReBuildAudits(item, isEsclate);
            if(!isEsclate)
            {
                item.GControlCurrentApproverId = Guid.Empty.ToString();
            }
            
            reviewIds.ForEach(reviewId =>
            {
                _gControlReviewerQueueList.Add(new GControlWorkflowDto()
                {
                    ManualReviewerId = reviewId,
                    Status = ApprovalProcessStatus.AddMapping,
                    WorkflowId = Guid.Empty,
                    StageId = Guid.Empty
                });
            });

            return new ManualApprovalItemActionResult
            {
                IsSucceed = true,
                EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
            };
        }

        private string ReBuildAudits(ManualApprovalRecord item, bool isEscalated)
        {
            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = _actionAccount.DisplayName,
                Action = isEscalated ? "RM_MA_Escalate" : "RM_MA_Reassign",
                Comment = item.ManualEscalatedComment
            });

            return SerializerHelper.SerializeToXmlString(audits);
        }

        private async Task<(bool, List<RMAccount>)> TrySyncUsersAsync(List<ToUserInfo> toUsers)
        {
            List<RMAccount> accounts = new List<RMAccount>();
            try
            {
                await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, toUsers);
                var userIds = toUsers.Select(item => item.UserId).ToList();
                accounts = await AccountDao.FindListAsync(item => userIds.Contains(item.UserId) && item.IsRemoved == 0);
                return (true, accounts);
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while try sync users. Error: {e}");
                return (false, accounts);
            }
        }

        private async System.Threading.Tasks.Task SendEmailAsync(List<ManualApprovalRecord> items, List<RMAccount> accounts, string comment)
        {
            try
            {
                var emailSender = new RMEmailSender(new RMEmailMemoryStorage(new RMEMailStorageManualMiddleware()));
                var parameters = accounts.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.UserId,
                    ToUser = item.UserPrincipalName,
                    RequestComment = comment,
                    TemplateType = RMEmailTemplateType.Manual
                });
                foreach (var item in items)
                {
                    var templateId = await GetEmailTemplateIdAsync(item);
                    emailSender.AddRange(templateId, parameters);
                }

                await emailSender.SendAsync();

                s_logger.Info($"Succeed send email to escalate/reassign users.");
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }
        
        private async Task GControlSendEmailAsync(List<ManualApprovalRecord> items, List<RMAccount> accounts, string comment)
        {
            try
            {
                var emailSender = new RMEmailSender(new RMEmailMemoryStorage(new RMEMailStorageManualMiddleware()));
                var parameters = accounts.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.AADId,
                    ToUser = item.UserPrincipalName,
                    RequestComment = comment,
                    TemplateType = RMEmailTemplateType.Manual
                });
                foreach (var item in items)
                {
                    var templateId = await GetGoogleEmailTemplateIdAsync(item);
                    emailSender.AddGControlRange(templateId, parameters);
                }

                await emailSender.SendAsync();

                s_logger.Info($"Succeed send email to escalate/reassign users.");
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }

        private async System.Threading.Tasks.Task<Guid> GetEmailTemplateIdAsync(Record item)
        {
            if(item.ManualWorkflowDefinitionId == Guid.Empty && 
                item.ManualWorkflowInstanceId == Guid.Empty &&
                item.ManualWorkflowStepId == Guid.Empty)
            {
                return RMEmailTemplateId.MANUAL_APPROVAL;
            }

            var workflowDefinitionId = item.ManualWorkflowDefinitionId;
            var workflowStepId = item.ManualWorkflowStepId;
            if(item.ManualWorkflowInstanceId != Guid.Empty)
            {
                var instance = await RMWorkflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);
            }

            var workflowInstance = await _workflowProcessor.LoadAsync(workflowDefinitionId);
            var step = workflowInstance.LoadStep(workflowStepId);
            var templateId = step.UsedEmailTemplateId;
            if (step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
            {
                if (item.ManualEmailNotificationCount < step.CustomIntervalSettings.Count - 1)
                {
                    templateId = new Guid((step.CustomIntervalSettings[item.ManualEmailNotificationCount]).UsedEmailTemplateId);
                }
                else
                {
                    templateId = new Guid((step.CustomIntervalSettings.Last()).UsedEmailTemplateId);
                }
            }
            if(templateId == Guid.Empty)
            {
                templateId = RMEmailTemplateId.MANUAL_APPROVAL;
            }

            return templateId;
        }
        
        private async System.Threading.Tasks.Task<Guid> GetGoogleEmailTemplateIdAsync(Record item)
        {
            if(item.GControlApprovalProcessId == Guid.Empty.ToString() && 
               item.GControlCurrentStageId == Guid.Empty.ToString())
            {
                return RMEmailTemplateId.MANUAL_APPROVAL;
            }

            var workflowDefinitionId = new Guid(item.GControlApprovalProcessId);
            var workflowStepId = new Guid(item.GControlCurrentStageId);

            var workflowInstance = await _workflowProcessor.LoadFromGControlAsync(workflowDefinitionId);
            var step = workflowInstance.LoadStep(workflowStepId);
            var templateId = step.UsedEmailTemplateId;
            if(templateId == Guid.Empty)
            {
                templateId = RMEmailTemplateId.MANUAL_APPROVAL;
            }

            return templateId;
        }
    }
}
