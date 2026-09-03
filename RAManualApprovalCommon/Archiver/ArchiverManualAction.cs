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
using AvePoint.RA.Common.Email;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Workflow;
using DocumentFormat.OpenXml.Spreadsheet;
using RAManualApproval.Comparers;
using RAManualApprovalCommon.Model;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using RAManualApprovalCommon.RelatedUtil;
using System;
using System.Linq;
using System.Reflection;
using AvePoint.RA.Contract.ManualApproval.Model;

namespace RAManualApprovalCommon.Archiver
{
    public abstract class ArchiverManualAction
    {
        protected static readonly RALogger s_logger = RALogger.GetInstance(typeof(ArchiverManualAction));

        protected abstract SourceFlag ContentSource { get; }

        protected abstract ManualApprovalSettingModel GetSettingInfo(Record record);

        protected readonly Guid _containerId;

        private readonly HistoryAddAction _historyAction = new();

        private readonly RMEmailSender _emailSender;

        private readonly RMWorkflowProcessor _workflowProcessor = new();

        public ArchiverManualAction(string jobId, Guid containerId)
        {
            _containerId = containerId;
            _emailSender = new(new RMEmailRedisStorage(jobId, new RMEMailStorageManualMiddleware()));
        }

        public Task<Record> ProcessWaitingForApprovalRecordAsync(Record record)
        {
            return ProcessWaitingForApprovalRecordAsync(record, new List<AADAccount>());
        }

        public async Task<Record> ProcessWaitingForApprovalRecordAsync(Record record, List<AADAccount> accounts)
        {
            var sourceFlag = ContentSource;
            if (sourceFlag == SourceFlag.LifecycleRetention)
            {
                sourceFlag = (SourceFlag)record.SourceFlag;
            }
            if (!ManualApprovalRuleInfoManager.TryGet(sourceFlag, record.RuleId.ToString(), out var ruleInfo))
            {
                throw new Exception("RM_RDM_Rule_RuleIsDeleted");
            }

            
            var settingInfo = GetSettingInfo(record);
            if (settingInfo.IsEnableSettingManualApproval)
            {
                ruleInfo.WorkflowId = settingInfo.WorkflowId;
                ruleInfo.IsSendEmailToOwner = settingInfo.IsSendEmialToOwner;
                ruleInfo.Owners = settingInfo.Owners;
            }
            record.IsAutoApproval = settingInfo.ManualApprovalType == ApprovalType.AutoApproval ? true : false;
            InitialRecord(record, ruleInfo);
            
            if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
            {
                await ProcessByWorkflowAsync(record, ruleInfo, accounts);
            }
            else
            {
                ProcessByOwners(record, ruleInfo);
            }

            return record;
        }
        public Record SetRecordIsAutoApproval(Record record)
        {
            try
            {
                var settingInfo = GetSettingInfo(record);
                record.IsAutoApproval = settingInfo.ManualApprovalType == ApprovalType.AutoApproval ? true : false;
                return record;
            }
            catch (Exception ex)
            {
                s_logger.Error($"SetRecordIsAutoApproval failed,error:{ex}");
                return record;
            }
        }
        public Record ProcessApprovedOrRejectedRecord(Record record)
        {
            record.ManualArchivedTime = DateTime.UtcNow.Ticks;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd;

            var historyData = _historyAction.Convert(
                record,
                (SOApproveDBStatus)record.ManualApprovedStatus,
                record.ManualApprovedBy,
                record.ManualActionTime
            );

            _historyAction.Add(historyData).GetAwaiter().GetResult();

            return record;
        }

        protected async Task ProcessByWorkflowAsync(Record record, ManualApprovalRuleModel ruleInfo, List<AADAccount> accounts)
        {
            var workflowDefinition = ManualApprovalWorkflowManager.Get(ruleInfo.WorkflowId);
            var workflowInstance = _workflowProcessor.LoadAsync(workflowDefinition.Id).GetAwaiter().GetResult();

            if (workflowInstance.HasStepUsedSiteOwnerApprovalMode())
            {
                await ProcessWorkflowOwnerAsync(ruleInfo, record);
            }

            if (workflowInstance.HasStepUsedSharePointGroupApprovalMode())
            {
                await ProcessWorkflowSharePointGroupAsync(ruleInfo, record, workflowInstance);
            }

            var step = workflowInstance.Start();
            var reviewers = await step.GetReviewersAsync(record.ScopeId);
            var templateId = step.UsedEmailTemplateId;
            if(step.UsedEmailTemplateMode == AvePoint.RA.Contract.RMWeb.CP.RMWorkflowStepUsedEmailTemplateMode.Custom)
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
                var parameterList = reviewers.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.UserId,
                    ToUser = item.UserPrincipalName,
                    TemplateType = RMEmailTemplateType.Manual,
                    RequestComment = ""
                });

                _emailSender.AddRange(templateId, parameterList);
            }

            record.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            record.ManualWorkflowDefinitionId = workflowDefinition.Id;
            record.ManualWorkflowStepId = step.Id;
        }

        protected virtual Task ProcessWorkflowSharePointGroupAsync(ManualApprovalRuleModel ruleInfo, Record record, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowInstance workflowInstance)
        {
            s_logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has step used Share Point group.");
            //todo site owner
            return ManualApprovalWorkflowManager.SyncSharePointGroupAsync(ruleInfo.WorkflowId, record, record.ScopeId, workflowInstance, ContentSource == SourceFlag.LifecycleRetention);
        }

        protected virtual Task ProcessWorkflowOwnerAsync(ManualApprovalRuleModel ruleInfo, Record record)
        {
            s_logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has step used site owner.");
            //todo site owner
            return ManualApprovalWorkflowManager.SyncSiteOwnerAsync(ruleInfo.WorkflowId, record, record.ScopeId);
        }

        private void ProcessByOwners(Record record, ManualApprovalRuleModel ruleInfo)
        {
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            record.ManualReviewer = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners).ToArray();
            if (ruleInfo.IsSendEmailToOwner)
            {
                var parameterList = ruleInfo.Owners.ConvertAll(item => new RMManualEmailTemplateParameters
                {
                    UserId = item.UserId,
                    ToUser = item.UserPrincipalName,
                    RequestComment = record.Comment,
                    TemplateType = RMEmailTemplateType.Manual
                });

                _emailSender.AddRange(RMEmailTemplateId.MANUAL_APPROVAL, parameterList);
            }
        }

        private static void InitialRecord(Record record, ManualApprovalRuleModel ruleInfo)
        {
            record.ManualEmailNotificationCount = 0;
            record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            record.ManualNeedEmailNotification = ruleInfo.IsSendEmailToOwner;
            record.ManualExtendTime = 0;
            record.ManualExtendComment = string.Empty;
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
            record.ManualApprovedBy = 0;
            record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
            record.ManualArchivedTime = 0;
            record.ManualLastExtendType = ManualApprovalExtendType.After1Month;
            record.ManualLastCustomeExtendDate = DateTime.UtcNow;

            try
            {
                var relatedInfos = RelatedItemUtil.GetRelatedProperties(record.RelatedRecords);
                if (relatedInfos != null && relatedInfos.Count > 0)
                {
                    var reportRelatedRecords = new List<ReportRelatedRecords>();
                    relatedInfos.ForEach(item =>
                    {
                        if (item.SourceFlag == (int)SourceFlag.SharePoint || item.SourceFlag == (int)SourceFlag.All)
                        {
                            var relatedItemUrl = WebUtil.MakeFullUrl(item.SiteUrl, item.url);
                            reportRelatedRecords.Add(
                                new ReportRelatedRecords
                                {
                                    Name = item.name,
                                    Url = relatedItemUrl
                                }
                            );
                        }
                        else if (item.SourceFlag == (int)SourceFlag.Physical)
                        {
                            var url = $"/Root/PRM/RecordsExplorer/?uniqueId={item.recId}";
                            reportRelatedRecords.Add(new ReportRelatedRecords() { Name = item.recId, Url = url });
                        }
                        else if (item.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                        {
                            string itemFullUrl = string.Empty;
                            if (!item.url.StartsWith(item.SiteUrl))
                            {
                                itemFullUrl = WebUtil.MakeFullUrl(item.SiteUrl, item.url);
                            }
                            else
                            {
                                itemFullUrl = item.url;
                            }
                            reportRelatedRecords.Add(
                                new ReportRelatedRecords
                                {
                                    Name = item.name,
                                    Url = itemFullUrl
                                }
                            );
                        }
                    });
                    record.ManualRelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
                    record.ManualIsRelatedRecords = reportRelatedRecords.Count > 0;
                }
                record.ManualRelatedRecordsAction = ruleInfo.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both ? 1 : 0;
            }
            catch (System.Exception e)
            {
                s_logger.Warn($"Parse related records error: {e}");
            }
        }
    }
}
