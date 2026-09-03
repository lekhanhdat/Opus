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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using RAManualApproval.Model;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RAManualApproval.Archiver
{
    public abstract class ArchiverManualAction
    {
        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected abstract SourceFlag ContentSource { get; }

        protected abstract ManualApprovalSettingModel GetSettingInfo(Record record);

        private readonly HistoryAddAction HistoryAction = new();

        public Record ProcessWaitingForApprovalRecord(Record record)
        {
            var result = ManualApprovalRuleInfoManager.TryGetAsync(ContentSource, record.RuleId.ToString()).GetAwaiter().GetResult();
            if (!result.Item1)
            {
                throw new Exception("RM_RDM_Rule_RuleIsDeleted");
            }
            var ruleInfo = result.Item2;
            var settingInfo = GetSettingInfo(record);
            if(settingInfo.IsEnableSettingManualApproval)
            {
                ruleInfo.WorkflowId = settingInfo.WorkflowId;
                ruleInfo.IsSendEmailToOwner = settingInfo.IsSendEmialToOwner;
                ruleInfo.Owners = settingInfo.Owners;
            }

            InitialRecord(record, ruleInfo);
            if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
            {
                ProcessByWorkflow(record, ruleInfo);
            }
            else
            {
                ProcessByOwners(record, ruleInfo);
            }

            // todo send email logic
            if(ruleInfo.IsSendEmailToOwner)
            {

            }

            return record;
        }

        public async Task<Record> ProcessApprovedOrRejectedRecordAsync(Record record)
        {
            record.ManualArchivedTime = DateTime.UtcNow.Ticks;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd;

            var historyData = HistoryAction.Convert(
                record,
                (SOApproveDBStatus)record.ManualApprovedStatus,
                record.ManualApprovedBy,
                record.ManualActionTime
            );

            await HistoryAction.AddAsync(historyData);

            return record;
        }

        private static void ProcessByWorkflow(Record record, ManualApprovalRuleModel ruleInfo)
        {
            var workflowDefinition = ManualApprovalWorkflowManager.Get(ruleInfo.WorkflowId);
            var workflowAnalyzer = new WorkflowAnalyzer(workflowDefinition);

            if(workflowAnalyzer.CheckWorkflowHasStepUseSiteOwnerReviewer())
            {
                Logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has step used site owner.");
                //todo site owner
            }

            if(workflowAnalyzer.WaitingForApprove().ReviewerType == AvePoint.RA.Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners)
            {
                //todo get site owner
            }
            else
            {
                record.ManualReviewer = workflowAnalyzer.WaitingForApprove().Reviewers.Select(item => item.RMUserId).ToArray();
            }

            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            record.ManualWorkflowDefinitionId = workflowDefinition.Id;
            record.ManualWorkflowStepId = workflowAnalyzer.WaitingForApprove().Id;
        }

        private static void ProcessByOwners(Record record, ManualApprovalRuleModel ruleInfo)
        {
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            record.ManualReviewer = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners).ToArray();
        }

        private static void InitialRecord(Record record, ManualApprovalRuleModel ruleInfo)
        {
            record.ManualEmailNotificationCount = 0;
            record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            record.ManualNeedEmailNotification = ruleInfo.IsSendEmailToOwner;
            record.ManualExtendTime = 0;
            record.ManualExtendCount = 0;
            record.ManualExtendComment = string.Empty;
            record.ManualEscalateFrom = 0;
            record.ManualEscalatedComment = string.Empty;
            record.ManualIsAutoReassigned = false;
            record.IsManualSynced = true;
            record.ManualRuleName = ruleInfo.RuleName;
            record.ManualRuleCriteria = ruleInfo.RuleCriterias;
            record.ManualRuleDisposalClass = ruleInfo.RuleDisposalClass;
            record.ManualCollectionTime = DateTime.UtcNow.Ticks;
            record.ManualArchivedTime = 0;
            record.ManualWorkflowInstanceId = Guid.Empty;
            record.ManualWorkflowDefinitionId = Guid.Empty;
            record.ManualWorkflowStepId = Guid.Empty;
            record.ManualApprovedBy = 0;
            record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
            record.ManualArchivedTime = 0;
        }
    }
}
