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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.Common.Email;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using DocumentFormat.OpenXml.Spreadsheet;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using RAManualApproval.ImportAction;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using RAManualApproval.BulkAction;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Google;

namespace RAManualApproval.EmailSchedule
{
    public class ManualApprovalEmailScheduleProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(ManualApprovalEmailScheduleProcessor));

        private static readonly IRMFunctionSettingDao s_functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static readonly IAccountDao s_accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly ITenantInfoDao s_tenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();

        private static readonly IRMWorkflowDefinitionDao s_workflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static readonly IWorkflowInstanceDao s_workflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IRMEmailItemDao s_emailItemDao = PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static readonly ITenantService s_tenantService = PlatformWindsorManager.GetService<ITenantService>();
        
        private static readonly IGControlUpdateTaskAssignee s_gControlUpdateTaskAssignee = PlatformWindsorManager.GetService<IGControlUpdateTaskAssignee>();

        private static readonly DateTime s_now = DateTime.UtcNow;

        private static readonly ManualApprovalSettings s_settings;

        private static readonly RMAccount s_tenantOwnerAccount;

        private static readonly HistoryAddAction s_historyAddAction;

        private static readonly SyncItemArchiverStatusAction s_syncArchiverStatusAction;

        private static readonly RMWorkflowProcessor s_workflowProcessor = new();

        private static readonly RMEmailSender s_emailSender = new(new RMEmailMemoryStorage(new RMEMailStorageManualMiddleware()));

        private static readonly IExplorerDao s_explorerDao = new ExplorerDao();

        private static readonly Dictionary<int, RMAccount> s_userCache = new();
        
        private static readonly Dictionary<string, RMAccount> s_googleUserCache = new();

        private static readonly List<Guid> CustomNotificationPendingItemIds = new();

        private static readonly List<Record> CustomNotificationFinishedItems = new();

        private static readonly ManualApprovalSettingType s_manualApprovalSettingType;

        private static readonly bool _hasFSLiscense;

        private static readonly bool _hasLSPLiscense;
        
        private static readonly bool _hasGControlLicense;

        private static readonly List<Guid> PendWorkflowNotificationItems = new();

        private static readonly int maxDisposalExtendCount;
        
        private static readonly bool isSuccessfullyAddedApprovalTaskAssignee;

        private static int CurrentTotalCount { get; set; }

        static ManualApprovalEmailScheduleProcessor()
        {
            s_functionSettingDao.NotExistCreateIt(FunctionSettingType.ManualSetting, JsonConvert.SerializeObject(new ManualApprovalSettings())).GetAwaiter().GetResult();
            var settingInfo = s_functionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
            s_settings = JsonConvert.DeserializeObject<ManualApprovalSettings>(settingInfo);
            s_manualApprovalSettingType = s_settings.EmailNotificationSetting.ManualApprovalSettingType;
            var tenantInfo = s_tenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId);
            s_tenantOwnerAccount = s_accountDao.Find(item => item.UserPrincipalName == tenantInfo.RegisterEmail);
            TenantLocalValue.LogonUserId = s_tenantOwnerAccount.UserId;
            s_historyAddAction = new ();
            s_syncArchiverStatusAction = new();
            if(s_manualApprovalSettingType == ManualApprovalSettingType.Advance)
            {
                s_settings.EmailNotificationSetting.OccurrencesTimes = s_settings.EmailNotificationSetting.AdvanceEmailSetting.Count;
            }

            _hasFSLiscense = s_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = s_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            _hasGControlLicense = s_tenantService.HasInitGControlPlatForm().Result;
            PendWorkflowNotificationItems = s_emailItemDao.GetRecordsIdForNewWorkflow();
            maxDisposalExtendCount = SerializerHelper.DeserializeByJsonConvert<ManualApprovalSettings>(s_functionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult()).DisposalExtentionSetting.MaxDelayTimes;
            if (_hasGControlLicense && s_gControlUpdateTaskAssignee.IsSucceedAddedApprovalTaskAssignee().Result && s_gControlUpdateTaskAssignee.IsSucceedAddedTaskReviewer().Result)
            {
                isSuccessfullyAddedApprovalTaskAssignee = true;
            }
        }

        public static async Task ProcessAsync(string jobId)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    s_logger.Info($"Start process email schedule.");
                    ManualApprovalEmailScheduleJobManager.Init(jobId);

                    await ProcessCustomNotificationSetting();

                    await ProcessItemsEscalationSettingAsync();

                    await ProcessItemsSendEmailAsync();

                    await SendWorkflowPendingEmailAsync();

                    await s_emailSender.SendAsync();

                    ManualApprovalDataSyncManager.WaitComplete();
                    ManualApprovalEmailScheduleJobManager.SetJobFinished();
                    s_logger.Info($"Succeed process email schedule.");
                }  
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while proccess email schedule. Error: {e}");
                ManualApprovalEmailScheduleJobManager.SetJobFailed(e.Message);
            }
        }

        private static async Task ProcessCustomNotificationSetting()
        {
            try
            {
                var customNotificationWorkflows = await s_workflowDefinitionDao.GetCustomNotificationWorkflowAsync();
                var customNotificationWorkflowsIds = customNotificationWorkflows.Select(w => w.Id).ToList();
                var waitingApproveStatus = (int)SOApproveDBStatus.WaitingApprove;
                var itemsList = ManualApprovalDataSyncManager.QueryItems(record =>
                    record.IsManualSynced &&
                    record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                    record.ManualNeedEmailNotification &&
                    record.ManualApprovedStatus == waitingApproveStatus &&
                    customNotificationWorkflowsIds.Contains(record.ManualWorkflowDefinitionId) &&
                    record.ManualExtendTime < DateTime.UtcNow.Ticks

                );
                ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessAutoApprovalSucceedAsync, ProcessAutoApprovalFailed);
                foreach (var items in itemsList)
                {
                    foreach (var item in items)
                    {
                        if (s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Rejected && item.ManualExtendCount >= maxDisposalExtendCount)
                        {
                            s_logger.Info($"Reject and {item.ManualExtendCount}. more than {maxDisposalExtendCount}");
                            continue;
                        }
                        var notificationCount = item.ManualEmailNotificationCount;
                        var workflowDefinitionId = item.ManualWorkflowDefinitionId;
                        var workflowStepId = item.ManualWorkflowStepId;
                        var workflowInstance = s_workflowProcessor.LoadAsync(workflowDefinitionId).GetAwaiter().GetResult();
                        var currentStep = workflowInstance.LoadStep(workflowStepId);
                        if (currentStep.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom &&
                            !PendWorkflowNotificationItems.Contains(item.Id))
                        {
                            var customNotificationSetting = currentStep.CustomIntervalSettings;
                            if (notificationCount < customNotificationSetting.Count - 1)
                            {
                                var currentNotificationSetting = customNotificationSetting[notificationCount + 1];
                                var ticksPredicate = GetQueryNeedSendEmailItemsTicksPredicate(currentNotificationSetting.Interval, ManualApprovalIntervalType.Days);
                                if (item.ManualEmailNotificationLastTime <= ticksPredicate)
                                {
                                    s_logger.Info($"Process items send email,current step is {currentStep.Id}, current send count is {notificationCount} ,current step interval is {currentNotificationSetting.Interval}");
                                    var templateId = new Guid(currentNotificationSetting.UsedEmailTemplateId);
                                    if (templateId == Guid.Empty)
                                    {
                                        templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                                    }
                                    await SendEmailAsync(item, templateId);
                                    item.ManualEmailNotificationLastTime = s_now.Ticks;
                                    item.ManualEmailNotificationCount += 1;
                                    ManualApprovalDataSyncManager.Add(item);
                                }
                            }
                            else
                            {
                                s_logger.Info($"Process items send email arrived {customNotificationSetting.Count} counts, no need to send email.");
                                CustomNotificationFinishedItems.Add(item);
                            }
                            CustomNotificationPendingItemIds.Add(item.Id);
                        }
                    }
                }
                ManualApprovalDataSyncManager.Commit();
                ManualApprovalDataSyncManager.RegisteProcessItemCallback(null, null);

            }
            catch(Exception e) 
            {
                s_logger.Info($"ProcessCustomNotificationSetting Error.");
            }           

            async Task ProcessAutoApprovalSucceedAsync(Record item)
            {
                try
                {
                    ManualApprovalEmailScheduleJobManager.AddSucceedJobDetail(item.LeafName, SettingAction.Notification);
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while process item: [{item.Id}]. Error: {e}");
                    ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, e.Message);
                }
            }

            void ProcessAutoApprovalFailed(Record item, string message)
            {
                ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, message);
            }
        }
        
        #region Escalate setting

        private static async Task ProcessItemsEscalationSettingAsync()
        {
            if (s_settings.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.NoAction)
            {
                s_logger.Info($"The escalate setting type is no action.");
                return;
            }
            if (s_settings.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.WorkflowNextStep)
            {
                await ProcessItemsWorkflowNextStepAsync();
            }
            else
            {
                await ProcessItemsReassignAsync();
            }
        }

        private static async Task ProcessItemsWorkflowNextStepAsync()
        {
            if (s_settings.EmailNotificationSetting.EndType == ManualApprovalEndType.NoEnd)
            {
                s_logger.Info($"current manual setting no end email notification.");
                return;
            }

            var historyCache = new Dictionary<Guid, RMManualApproveHistoryTableEntity>();

            var waitingApproveStatus = (int)SOApproveDBStatus.WaitingApprove;
            var settingAction = s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Approved ? SettingAction.Approved : SettingAction.Rejected;

            ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessAutoApprovalSucceedAsync, ProcessAutoApprovalFailed);
            var itemsList = ManualApprovalDataSyncManager.QueryItems(record =>
                record.IsManualSynced &&
                record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                record.ManualNeedEmailNotification &&
                record.ManualEmailNotificationCount >= s_settings.EmailNotificationSetting.OccurrencesTimes &&
                record.ManualApprovedStatus == waitingApproveStatus &&
                (record.ManualWorkflowInstanceId != Guid.Empty || (record.ManualWorkflowDefinitionId != Guid.Empty && record.ManualWorkflowStepId != Guid.Empty) &&
                !CustomNotificationPendingItemIds.Contains(record.Id)) &&
                record.ManualExtendTime < DateTime.UtcNow.Ticks
            );

            

            foreach (var items in itemsList)
            {
                var processItems = new List<Record>();
                processItems.AddRange(items);
                if (CustomNotificationFinishedItems.Count != 0)
                {
                    processItems.AddRange(CustomNotificationFinishedItems);
                    CustomNotificationFinishedItems.Clear();
                }
                CurrentTotalCount += processItems.Count;            
                foreach (var item in processItems)
                {
                    try
                    {

                        if (!_hasFSLiscense && item.SourceFlag == (int)SourceFlag.FileSystem)
                        {
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, settingAction, "RM_MA_NoLicense");
                            continue;
                        }

                        if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
                        {
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, settingAction, "RM_MA_NoLicense");
                            continue;
                        }
                        if (s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Rejected && item.ManualExtendCount >= maxDisposalExtendCount)
                        {
                            s_logger.Info($"Item max disposal date , can not reject. ");
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Rejected, "RM_MA_MaxRejectExtendDisposalDate");
                            continue;
                        }
                        item.ManualApprovalComment = I18NEntity.GetString("RM_MA_AutomaticApproval");
                        item.QuickReason = string.Empty;
                        item.ManualLastReasonForRejection = string.Empty;
                        var historyData = s_historyAddAction.Convert(item, s_settings.EscalationSetting.ApprovalStatus, s_tenantOwnerAccount.Id);
                        var instance = await ProcessItemWorkflowAsync(item);  
                        if (instance != RMWorkflowStatus.Completed || item.SourceFlag > (int)SourceFlag.Connector)
                        {
                            historyCache.Add(item.Id, historyData);
                        }                        
                        s_logger.Info($"Succeed resume item: [{item.Id}] workflow: [{item.ManualWorkflowInstanceId}] to next step.");
                    }
                    catch (Exception e)
                    {
                        s_logger.Error($"An error occurred while process item: [{item.Id}] execute reassign action. Error: {e}");
                    }
                }
                if (CurrentTotalCount >= 10000)
                {
                    ManualApprovalDataSyncManager.Commit();
                    ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessAutoApprovalSucceedAsync, ProcessAutoApprovalFailed);
                    CurrentTotalCount = 0;
                }
            }

            ManualApprovalDataSyncManager.Commit();
            ManualApprovalDataSyncManager.RegisteProcessItemCallback(null, null);

            async Task ProcessAutoApprovalSucceedAsync(Record item)
            {
                try
                {
                    if (item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete)
                    {

                        await s_syncArchiverStatusAction.UpdateItemArchiverStatusAsync(item);
                    }

                    if (historyCache.TryGetValue(item.Id, out var historyData))
                    {
                        await s_historyAddAction.AddAsync(historyData);
                        s_logger.Info($"Successful add item: [{item.Id}] to history.");
                        historyCache.Remove(item.Id);
                    }

                    ManualApprovalEmailScheduleJobManager.AddSucceedJobDetail(item.LeafName, settingAction);
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred process auto approval succeed item [{item.Id}]. Error: {e}");
                    ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, settingAction, e.Message);
                }
            }

            void ProcessAutoApprovalFailed(Record item, string message)
            {
                ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, settingAction, message);
            }
        }

        private static async Task<RMWorkflowStatus> ProcessItemWorkflowAsync(Record item)
        {
            var workflowDefinitionId = item.ManualWorkflowDefinitionId;
            var workflowStepId = item.ManualWorkflowStepId;
            if (item.ManualWorkflowInstanceId != Guid.Empty && item.ManualWorkflowDefinitionId == Guid.Empty && item.ManualWorkflowStepId == Guid.Empty)
            {
                var instance = await s_workflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;

                await s_workflowInstanceDao.UpdateStatusAsync(instance.Id, RMWorkflowStatus.Completed);
            }

            var workflowInstance = s_workflowProcessor.LoadAsync(workflowDefinitionId).GetAwaiter().GetResult();
            var currentStep = workflowInstance.LoadStep(workflowStepId);

            var nextStep = currentStep;
            if (s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Approved)
            {
                nextStep = currentStep.Approve();
            }
            else
            {
                nextStep = currentStep.Reject();
            }
            item.ManualWorkflowStepId = nextStep.Id;
            item.ManualApprovedBy = int.MinValue;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            item.ManualEscalateFrom = 0;
            item.ManualEscalatedComment = string.Empty;
            var extendType = ManualApprovalExtendType.After1Month;
            var customDateTime = DateTime.UtcNow;
            item.ManualLastExtendType = ManualApprovalExtendType.After1Month;
            item.ManualLastApproveRejectComment = string.Empty;
            item.ManualLastReviewedBy = I18NEntity.GetString("RM_MA_AutomaticApproval");
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
            if (!nextStep.IsEnd)
            {
                var reviewers = new List<ReviewerUser>();
                if (item.IsFsControlRecordJPMC)
                {
                    reviewers = (await nextStep.GetReviewersAsync(new Guid(item.AveSiteId)));
                }
                else
                {
                    reviewers = await nextStep.GetReviewersAsync(item.ScopeId);
                }
                var templateId = nextStep.UsedEmailTemplateId;
                if (nextStep.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
                {
                    var customIntervalSettings = nextStep.CustomIntervalSettings[0];
                    if (customIntervalSettings == null)
                    {
                        templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                    }
                    else
                    {
                        templateId = new Guid(customIntervalSettings.UsedEmailTemplateId);
                        if (templateId == Guid.Empty)
                        {
                            templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                        }
                    }
                }
                item.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
                if (item.ManualNeedEmailNotification)
                {
                    foreach (var reviewer in reviewers)
                    {
                        s_emailSender.Add(templateId, new RMManualEmailTemplateParameters
                        {
                            UserId = reviewer.UserId,
                            ToUser = reviewer.UserPrincipalName,
                            TemplateType = RMEmailTemplateType.Manual,
                            RequestComment = "",
                        });
                    }
                }
            }

            if (nextStep.IsEnd)
            {
                item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                item.ManualApprovedStatus = (int)s_settings.EscalationSetting.ApprovalStatus;
                if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                {
                    item.DisposalStatus = item.ManualApprovedStatus;
                }
                if (s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Rejected)
                {
                    item.ManualExtendTime = DateTime.UtcNow.AddMonths(1).Ticks;
                    item.ManualExtendComment = string.Empty;
                    item.ManualExtendCount += 1;

                }
            }
            await ManualApprovalAzureTableManager.RebuildAuditsAsync(item, s_settings.EscalationSetting.ApprovalStatus, s_tenantOwnerAccount, extendType, 0 ,customDateTime);
            ManualApprovalDataSyncManager.Add(item);

            return nextStep.IsEnd ? RMWorkflowStatus.Completed : RMWorkflowStatus.Running;
        }

        private static async Task ProcessItemsReassignAsync()
        {
            if (s_settings.EmailNotificationSetting.EndType == ManualApprovalEndType.NoEnd)
            {
                s_logger.Info($"current manual setting no end email notfication.");
                return;
            }
            var waitingApproveStatus = (int)SOApproveDBStatus.WaitingApprove;
            var users = s_settings.EscalationSetting.ReassignUsers;
            var userIntIds = users.Select(item => item.RMUserId).ToArray();

            var itemsList = ManualApprovalDataSyncManager.QueryItems(record =>
                record.IsManualSynced && record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                record.ManualNeedEmailNotification &&
                !record.ManualIsAutoReassigned &&
                record.ManualEmailNotificationCount >= s_settings.EmailNotificationSetting.OccurrencesTimes &&
                record.ManualApprovedStatus == waitingApproveStatus &&
                !CustomNotificationPendingItemIds.Contains(record.Id)  &&
                record.ManualExtendTime < DateTime.UtcNow.Ticks
            );

            var parametersList = users.ConvertAll(item => new RMManualEmailTemplateParameters
            {
                UserId = item.UserId,
                ToUser = item.UserPrincipalName,
                TemplateType = RMEmailTemplateType.Manual,
                RequestComment = ""
            });
            ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessAutoApprovalSucceedAsync, ProcessAutoApprovalFailed);
            foreach (var items in itemsList)
            {
                var processItems = new List<Record>();
                processItems.AddRange(items);
                if (CustomNotificationFinishedItems.Count != 0)
                {
                    var needReassignItems = CustomNotificationFinishedItems.Where(record => !record.ManualIsAutoReassigned);
                    processItems.AddRange(needReassignItems);
                    CustomNotificationFinishedItems.Clear();
                }
                CurrentTotalCount += processItems.Count;
                foreach (var item in processItems)
                {
                    try
                    {
                        if (userIntIds.Length == 0)
                        {
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, "RM_MA_NoOwner");
                            continue;
                        }

                        if (!_hasFSLiscense && item.SourceFlag == (int)SourceFlag.FileSystem)
                        {
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, "RM_MA_NoLicense");
                            continue;
                        }

                        if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
                        {
                            ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, "RM_MA_NoLicense");
                            continue;
                        }
                        var templateId = await GetEmailTemplateIdAsync(item);
                        s_emailSender.AddRange(templateId, parametersList);

                        item.ManualEscalateFrom = s_tenantOwnerAccount.Id;
                        item.ManualReviewer = userIntIds;
                        item.ManualActionTime = s_now.Ticks;
                        item.ManualEscalatedComment = "";
                        item.ManualIsAutoReassigned = true;
                        ManualApprovalAzureTableManager.ReBuildReassignAudits(item, s_tenantOwnerAccount);
                        ManualApprovalDataSyncManager.Add(item);
                    }
                    catch (Exception e)
                    {
                        s_logger.Error($"An error occurred while process item: [{item.Id}] execute reassign action. Error: {e}");
                        ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, e.Message);
                    }
                }
                if (CurrentTotalCount >= 10000)
                {
                    ManualApprovalDataSyncManager.Commit();
                    CurrentTotalCount = 0;
                }
            }

            ManualApprovalDataSyncManager.Commit();
            ManualApprovalDataSyncManager.RegisteProcessItemCallback(null, null);

            async Task ProcessAutoApprovalSucceedAsync(Record item)
            {
                try
                {
                    ManualApprovalEmailScheduleJobManager.AddSucceedJobDetail(item.LeafName, SettingAction.Reassign);
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while process item: [{item.Id}]. Error: {e}");
                    ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, e.Message);
                }
            }

            void ProcessAutoApprovalFailed(Record item, string message)
            {
                ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Reassign, message);
            }
        }

        #endregion

        #region Send Email

        private static async Task ProcessItemsSendEmailAsync()
        {
            if(s_manualApprovalSettingType == ManualApprovalSettingType.Advance)
            {
                foreach(var advanceSetting in s_settings.EmailNotificationSetting.AdvanceEmailSetting.OrderBy(setting => setting.CurrentStep))
                {
                    s_logger.Info($"Process items send email,current step is {advanceSetting.CurrentStep}, current step interval is {advanceSetting.Interval}, current step interval type is {advanceSetting.IntervalType}");
                    var advncaeExpression = GetAdvanceNeedSendEmailItemsExpression(advanceSetting.CurrentStep, advanceSetting.Interval, advanceSetting.IntervalType);
                    await ProcessItemsListAsync(advncaeExpression);
                }
                return;
            }

            var intervalExpression = GetIntervalNeedSendEmailItemsExpression();
            await ProcessItemsListAsync(intervalExpression);
        }

        private static async Task ProcessItemsListAsync(Expression<Func<Record, bool>> expression)
        {
            var itemsList = ManualApprovalDataSyncManager.QueryItems(expression);
            foreach (var items in itemsList)
            {
                await ProcessItemsAsync(items);
            }
        }

        private static async Task ProcessItemsAsync(IEnumerable<Record> items)
        {
            s_logger.Info($"Current batch process items count: [{items.Count()}]");
            var ManualSettingInfoJson = s_functionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
            var ManualSettingInfoes = SerializerHelper.DeserializeByJsonConvert<ManualApprovalSettings>(ManualSettingInfoJson);
            var maxDisposalExtendCount = ManualSettingInfoes.DisposalExtentionSetting.MaxDelayTimes;
            ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessAutoApprovalSucceedAsync, ProcessAutoApprovalFailed);
            foreach (var item in items)
            {
                try
                {
                    if (!_hasFSLiscense && item.SourceFlag == (int)SourceFlag.FileSystem)
                    {
                        ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, "RM_MA_NoLicense");
                        continue;
                    }

                    if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
                    {
                        ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, "RM_MA_NoLicense");
                        continue;
                    }
                    await SendEmailAsync(item);
                    item.ManualEmailNotificationLastTime = s_now.Ticks;
                    item.ManualEmailNotificationCount += 1;
                    ManualApprovalDataSyncManager.Add(item);
                    s_logger.Info($"Succeed add item: [{item.Id}] reviewr to need send email user collection.");  
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while process item: [{item.Id}]. Error: {e}");
                }
            }

            ManualApprovalDataSyncManager.Commit();
            ManualApprovalDataSyncManager.RegisteProcessItemCallback(null, null);

            async Task ProcessAutoApprovalSucceedAsync(Record item)
            {
                try
                {
                    ManualApprovalEmailScheduleJobManager.AddSucceedJobDetail(item.LeafName, SettingAction.Notification);
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while process item: [{item.Id}]. Error: {e}");
                    ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, e.Message);
                }
            }

            void ProcessAutoApprovalFailed(Record item, string message)
            {
                ManualApprovalEmailScheduleJobManager.AddFailedJobDetail(item.LeafName, SettingAction.Notification, message);
            }
        }

        private static async Task SendEmailAsync(Record item)
        {
            try
            {
                var accounts = await GetAccountsAsync(item.ManualReviewer.ToList());
                var parameters = accounts.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.UserId,
                    ToUser = item.UserPrincipalName,
                    TemplateType = RMEmailTemplateType.Manual,
                    RequestComment = ""
                });
                var templateId = await GetEmailTemplateIdAsync(item);
                s_emailSender.AddRange(templateId, parameters);

                s_logger.Info($"Succeed send email to escalate/reassign users.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }

        private static async Task SendEmailAsync(Record item, Guid templateId)
        {
            try
            {
                var accounts = await GetAccountsAsync(item.ManualReviewer.ToList());
                var parameters = accounts.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.UserId,
                    ToUser = item.UserPrincipalName,
                    TemplateType = RMEmailTemplateType.Manual,
                    RequestComment = ""
                });
                s_emailSender.AddRange(templateId, parameters);

                s_logger.Info($"Succeed send email to users.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }

        private static async Task<Guid> GetEmailTemplateIdAsync(Record item)
        {
            if (item.ManualWorkflowDefinitionId == Guid.Empty &&
                item.ManualWorkflowInstanceId == Guid.Empty &&
                item.ManualWorkflowStepId == Guid.Empty)
            {
                return RMEmailTemplateId.MANUAL_APPROVAL;
            }

            var workflowDefinitionId = item.ManualWorkflowDefinitionId;
            var workflowStepId = item.ManualWorkflowStepId;
            if (item.ManualWorkflowInstanceId != Guid.Empty)
            {
                var instance = await s_workflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);
            }

            var workflowInstance = await s_workflowProcessor.LoadAsync(workflowDefinitionId);
            var step = workflowInstance.LoadStep(workflowStepId);

            if(step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
            {
                var custonIntervalStting = step.CustomIntervalSettings.FirstOrDefault();
                if(custonIntervalStting == null)
                {
                    return RMEmailTemplateId.MANUAL_APPROVAL;
                }
                var templateId = new Guid(custonIntervalStting.UsedEmailTemplateId);
                if (templateId == Guid.Empty)
                {
                    return RMEmailTemplateId.MANUAL_APPROVAL;
                }
                return templateId;
            }

            return step.UsedEmailTemplateId;
        }
        
        private static async Task<Guid> GetGoogleEmailTemplateIdAsync(Record item)
        {
            var workflowDefinitionId = item.GControlApprovalProcessId;
            var workflowStepId = item.GControlCurrentStageId;

            var workflowInstance = await s_workflowProcessor.LoadFromGControlAsync(new Guid(workflowDefinitionId));
            var step = workflowInstance.LoadStep(new Guid(workflowStepId));

            return step.UsedEmailTemplateId;
        }

        private static Expression<Func<Record, bool>> GetIntervalNeedSendEmailItemsExpression()
        {
            var emailNotificationSetting = s_settings.EmailNotificationSetting;
            var waitingApproveStatus = (int)SOApproveDBStatus.WaitingApprove;
            var ticksPredicate = GetQueryNeedSendEmailItemsTicksPredicate(emailNotificationSetting.Interval, emailNotificationSetting.IntervalType);
            var occurrences = emailNotificationSetting.OccurrencesTimes;

            if (emailNotificationSetting.EndType == ManualApprovalEndType.NoEnd)
            {
                return (record) => record.IsManualSynced 
                    && record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted
                    && record.ManualNeedEmailNotification
                    && record.ManualApprovedStatus == waitingApproveStatus
                    && record.ManualEmailNotificationLastTime <= ticksPredicate
                    && !CustomNotificationPendingItemIds.Contains(record.Id) &&
                    record.ManualExtendTime < DateTime.UtcNow.Ticks;
            }

            return (record) => record.IsManualSynced
                && record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted
                && record.ManualNeedEmailNotification
                && record.ManualApprovedStatus == waitingApproveStatus
                && record.ManualEmailNotificationLastTime <= ticksPredicate
                && record.ManualEmailNotificationCount < occurrences &&
                !CustomNotificationPendingItemIds.Contains(record.Id) &&
                 record.ManualExtendTime < DateTime.UtcNow.Ticks;
        }

        private static Expression<Func<Record, bool>> GetAdvanceNeedSendEmailItemsExpression(int currentStep, int Interval, ManualApprovalIntervalType intervalType)
        {
            var waitingApproveStatus = (int)SOApproveDBStatus.WaitingApprove;
            var occurrences = s_settings.EmailNotificationSetting.OccurrencesTimes;
            var ticksPredicate = GetQueryNeedSendEmailItemsTicksPredicate(Interval, intervalType);
            return (record) => record.IsManualSynced 
                    && record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted
                    && record.ManualNeedEmailNotification
                    && record.ManualApprovedStatus == waitingApproveStatus
                    && record.ManualEmailNotificationCount == currentStep - 1
                    && record.ManualEmailNotificationLastTime <= ticksPredicate &&
                    !CustomNotificationPendingItemIds.Contains(record.Id) &&
                    record.ManualExtendTime < DateTime.UtcNow.Ticks;
        }

        private static long GetQueryNeedSendEmailItemsTicksPredicate(int interval, ManualApprovalIntervalType intervalType)
        {
            var days = interval;
            if (intervalType == ManualApprovalIntervalType.Weeks)
            {
                days = interval * 7;
            }
            return s_now.AddDays(0 - days).Ticks;
        }

        #endregion

        #region Workflow Send Email

        private static async Task SendWorkflowPendingEmailAsync()
        {
            try
            {
                var itemIds = PendWorkflowNotificationItems;
                for (var i = 0; i < itemIds.Count; i += 500)
                {
                    var batchItemIds = itemIds.Skip(i).Take(500).ToList();
                    var items = s_explorerDao.GetRecordByIds(batchItemIds);
                    foreach (var item in items)
                    {
                        if (!item.IsGControlRecord && (item.ManualWorkflowDefinitionId == Guid.Empty ||
                            item.ManualWorkflowStepId == Guid.Empty ||
                            item.ManualApprovedStatus != (int)SOApproveDBStatus.WaitingApprove))
                        {
                            continue;
                        }
                        if (item.IsGControlRecord && (item.GControlApprovalProcessId == Guid.Empty.ToString() ||
                                                           item.GControlCurrentApproverId == Guid.Empty.ToString() ||
                                                           item.GControlManualApprovedStatus != (int)SOApproveDBStatus.WaitingApprove)
                                                    && isSuccessfullyAddedApprovalTaskAssignee)

                        {
                            continue;
                        }
                        if (item.ManualExtendTime >= DateTime.UtcNow.Ticks)
                        {
                            s_logger.Info($"SendWorkflowPendingEmailAsync - extemd {item.LeafName} -{item.ManualExtendTime} cant autoApproval");
                            continue;
                        }
                        if (s_settings.EscalationSetting.ApprovalStatus == SOApproveDBStatus.Rejected && item.ManualExtendCount >= maxDisposalExtendCount)
                        {
                            s_logger.Info($"Item max disposal date , can not reject. ");
                            continue;
                        }

                        if (item.IsGControlRecord)
                        {
                            var googleUsers = await GetGoogleAccountsAsync([item.GControlCurrentApproverId]);
                            var googleUser = googleUsers.FirstOrDefault();
                            if (googleUser is null)
                            {
                                s_logger.Warn($"Cannot find google user by user id: {item.GControlCurrentApproverId}");
                                continue;
                            }

                            var googleUserParameter = new RMManualEmailTemplateParameters
                            {
                                UserId = googleUser.UserId,
                                ToUser = googleUser.UserPrincipalName,
                                TemplateType = RMEmailTemplateType.Manual,
                                RequestComment = ""
                            };
                            var gControlTemplateId = await GetGoogleEmailTemplateIdAsync(item);
                            s_emailSender.AddGControlTemplate(gControlTemplateId, googleUserParameter);
                            continue;
                        }
                        var account = await GetAccountsAsync(item.ManualReviewer.ToList());
                        var parametersList = account.ConvertAll(item => new RMManualEmailTemplateParameters
                        {
                            UserId = item.UserId,
                            ToUser = item.UserPrincipalName,
                            TemplateType = RMEmailTemplateType.Manual,
                            RequestComment = ""
                        });
                      
                        var templateId = await GetEmailTemplateIdAsync(item);
                        s_emailSender.AddRange(templateId, parametersList);
                    }
                }

                var effectCount = await s_emailItemDao.Empty();
                s_logger.Info($"Succeed clear cached email items [{effectCount}] from db.");
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while send workflow pending emails. Error: {e}");
            }
        }

        private static async Task<List<RMAccount>> GetAccountsAsync(List<int> userIds)
        {
            var notInCacheUserIds = userIds.Where(item => !s_userCache.ContainsKey(item));
            if(notInCacheUserIds.Any())
            {
                var accounts = await s_accountDao.GetUserWithRemovedByIds(notInCacheUserIds.ToList());
                accounts.ForEach(item => s_userCache.Add(item.Id, item));
            }

            var res = new List<RMAccount>();

            foreach(var userId in userIds)
            {
                if(s_userCache.TryGetValue(userId, out var account))
                {
                    res.Add(account);
                }
            }

            return res;
        }
        
        private static async Task<List<RMAccount>> GetGoogleAccountsAsync(List<string> userIds)
        {
            var notInCacheUserIds = userIds.Where(item => !s_googleUserCache.ContainsKey(item));
            if(notInCacheUserIds.Any())
            {
                var accounts = await s_accountDao.GetGoogleUserByUserIdsAsync(notInCacheUserIds.ToList());
                accounts.ForEach(item => s_googleUserCache.TryAdd(item.AADId, item));
            }

            var res = new List<RMAccount>();

            foreach(var userId in userIds)
            {
                if(s_googleUserCache.TryGetValue(userId, out var account))
                {
                    res.Add(account);
                }
            }

            return res;
        }

        #endregion
    }
}
