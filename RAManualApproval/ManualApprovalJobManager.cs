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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RAManualApproval.BulkAction;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval
{
    public class ManualApprovalJobManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalJobManager));

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static bool HasSucceedDetail { get; set; }

        private static bool HasFailedDetail { get; set; }

        private static readonly Dictionary<SOApproveDBStatus, string> ApprovalStatusI18ns =
            new Dictionary<SOApproveDBStatus, string>
            {
                { SOApproveDBStatus.Approved, "RM_DAM_ManualApproval_ApprovedStatus" },
                { SOApproveDBStatus.Rejected, "RM_DAM_ManualApproval_RejectedStatus" },
                { SOApproveDBStatus.WaitingApprove, "RM_DAM_ManualApproval_WaitingApproveStatus" }
            };

        private static readonly Dictionary<RMReportObjectLevel, string> ReportObjectLevelI18ns =
            new Dictionary<RMReportObjectLevel, string>
            {
                { RMReportObjectLevel.Document, "RM_JS_Rule_ObjectLevel_Document" },
                { RMReportObjectLevel.SiteCollection, "RM_JS_Rule_ObjectLevel_SiteCollection" },
                { RMReportObjectLevel.Site, "RM_JS_Rule_ObjectLevel_Site" },
                { RMReportObjectLevel.List, "RM_JS_Rule_ObjectLevel_List" },
                { RMReportObjectLevel.Item, "RM_JS_Rule_ObjectLevel_Item" },
                { RMReportObjectLevel.Folder, "RM_JS_Rule_ObjectLevel_Folder" },
                { RMReportObjectLevel.ExchangeOnlineItem, "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem" },
                { RMReportObjectLevel.PhysicalBox, "RM_Common_ObjectLevel_PhysicalBox" },
                { RMReportObjectLevel.PhysicalFile, "RM_JS_Rule_ObjectLevel_PhysicalFile" },
                { RMReportObjectLevel.PhysicalRecord, "RM_JS_Rule_ObjectLevel_PhysicalRecord" },
                { RMReportObjectLevel.FSFile, "RM_JS_Rule_ObjectLevel_Document" },
                { RMReportObjectLevel.BoxFile, "RM_JS_Rule_ObjectLevel_BoxFile"},
                { RMReportObjectLevel.CustomizeConnectorItem, "RM_Connector_ItemLevel_Item" }
            };

        private static readonly Dictionary<RMNodeLevel, string> ReportNodeLevelI18ns =
            new Dictionary<RMNodeLevel, string>
            {
                        { RMNodeLevel.SiteCollection, "RM_JS_Rule_ObjectLevel_SiteCollection" },
                        { RMNodeLevel.Site, "RM_JS_Rule_ObjectLevel_Site" },
                        { RMNodeLevel.List, "RM_JS_Rule_ObjectLevel_List" },
                        { RMNodeLevel.Item, "RM_JS_Rule_ObjectLevel_Item" },
                        { RMNodeLevel.Folder, "RM_JS_Rule_ObjectLevel_Folder" },
                        { RMNodeLevel.ExchangeOnlineItem, "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem" },
                        { RMNodeLevel.PhysicalBox, "RM_Common_ObjectLevel_PhysicalBox" },
                        { RMNodeLevel.PhysicalFile, "RM_JS_Rule_ObjectLevel_PhysicalFile" },
                        { RMNodeLevel.PhysicalRecord, "RM_JS_Rule_ObjectLevel_PhysicalRecord" },
                        { RMNodeLevel.FSFile, "RM_JS_Rule_ObjectLevel_Document" },
                        { RMNodeLevel.BoxFile, "RM_JS_Rule_ObjectLevel_BoxFile" },
                        { RMNodeLevel.CustomizeConnectorItem, "RM_Connector_ItemLevel_Item" }
            };

        public static Dictionary<SOApproveDBStatus, string> ManualAppovalActionI18N = new()
        {
            {SOApproveDBStatus.Approved, "RM_MA_Approve" },
            {SOApproveDBStatus.Rejected, "RM_MA_Reject" },
            {SOApproveDBStatus.RestartProcess, "RM_MA_ResetManualWorkflow" },
        };

        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.ManualApproval);
            ReportManager.StartUpdateJobProgress();
        }

        public static void Init(string jobId, JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress();
        }

        public static void IncreaseBase(long count)
        {
            ReportManager.IncreaseBase(count);
        }

        public static void Increase()
        {
            ReportManager.Increase();
        }

        public static void AddSucceedJobDetail(Record item, ManualApprovalAction action)
        {
            var ownerDisplayNames = "";
            try
            {
                var ownerDisplayNameArray = ManualApprovalOwnerManager.GetOwnerDisplayNames(item.ManualReviewer);
                ownerDisplayNames = string.Join("; ", ownerDisplayNameArray);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get manual reviewer: [{string.Join(", ", item.ManualReviewer)}] display names. Error: {e}");
            }

            var detail = new JMManualApprovalJobDetails
            {
                TitleOrName = item.LeafName,
                Url = item.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)item.NodeType, out var nodeLevel) ? nodeLevel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)item.ManualApprovedStatus, out var approvalStatus) ? approvalStatus : "",
                Action = action.ToString(),
                RuleCriteria = item.ManualRuleCriteria,
                RecordOwner = ownerDisplayNames,
                Status = JobDetailsStatus.Successful
            };
            ReportManager.SendJobDetail(detail);
            HasSucceedDetail = true;
        }

        public static void AddSucceedJobDetail(Record record, int status, string action, string[] reviewers)
        {
            HasSucceedDetail = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                Action = action,
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = GetReviewers(reviewers),
                Status = JobDetailsStatus.Successful,
            });
        }

        public static void AddFailedJobDetail(Record item, ManualApprovalAction action, string comment)
        {
            var ownerDisplayNames = "";
            try
            {
                var ownerDisplayNameArray = ManualApprovalOwnerManager.GetOwnerDisplayNames(item.ManualReviewer);
                ownerDisplayNames = string.Join("; ", ownerDisplayNameArray);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get manual reviewer: [{string.Join(", ", item.ManualReviewer)}] display names. Error: {e}");
            }
            var detail = new JMManualApprovalJobDetails
            {
                TitleOrName = item.LeafName,
                Url = item.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)item.NodeType, out var nodeLevel) ? nodeLevel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)item.ManualApprovedStatus, out var approvalStatus) ? approvalStatus : "",
                Action = action.ToString(),
                RuleCriteria = item.ManualRuleCriteria,
                RecordOwner = ownerDisplayNames,
                Status = JobDetailsStatus.Failed,
                Comment = comment,
            };
            ReportManager.SendJobDetail(detail);
            HasFailedDetail = true;
        }

        public static void AddFailedJobDetail(Record record, int status, string[] reviewers, string action, string comment)
        {
            HasFailedDetail = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                Action = action,
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = GetReviewers(reviewers),
                Status = JobDetailsStatus.Failed,
                Comment = comment,
            });
        }

        public static void AddFailedJobDetail(ManualExportReportInfo report, ManualApprovalRuleModel ruleInfo, string comment)
        {
            var detail = new JMManualApprovalJobDetails
            {
                TitleOrName = report.LeafName,
                Url = report.Path,
                ObjectLevel = ReportObjectLevelI18ns.TryGetValue(report.ObjectLevel, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue(report.Status, out var approvalStatus) ? approvalStatus : "",
                Action = ManualApprovalAction.Export.ToString(),
                RuleCriteria = ruleInfo?.RuleCriterias,
                Status = JobDetailsStatus.Failed,
                Comment = comment,
            };
            ReportManager.SendJobDetail(detail);

            HasFailedDetail = true;
        }

        public static void SetJobFinished()
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            ReportManager.SetJobFinished(jobFinishStatus);
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }

        private static string GetReviewers(string[] reviewers)
        {
            string RecordReviewer = string.Empty;
            if (reviewers.Length > 0)
            {
                RecordReviewer = String.Join(";", reviewers);
            }
            return RecordReviewer;
        }
    }
}
