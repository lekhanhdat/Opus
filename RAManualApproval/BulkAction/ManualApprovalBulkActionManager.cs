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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using RACloudFS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.BulkAction
{
    class ManualApprovalBulkActionManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ManualApprovalBulkActionManager));

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

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
                { RMNodeLevel.CustomizeConnectorItem, "RM_Connector_ItemLevel_Item" },
                { RMNodeLevel.BoxFile, "RM_JS_Rule_ObjectLevel_BoxFile" },
                { RMNodeLevel.GoogleFile, "RM_JS_Rule_ObjectLevel_GoogleFile" }
    };

        private static readonly Dictionary<SOApproveDBStatus, string> ApprovalStatusI18ns =
            new Dictionary<SOApproveDBStatus, string>
            {
                { SOApproveDBStatus.Approved, "RM_DAM_ManualApproval_ApprovedStatus" },
                { SOApproveDBStatus.Rejected, "RM_DAM_ManualApproval_RejectedStatus" },
                { SOApproveDBStatus.WaitingApprove, "RM_DAM_ManualApproval_WaitingApproveStatus" }
            };


        private static bool HasSucceed { get; set; }

        private static bool HasFailed { get; set; }
        public static void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress(60);
        }
        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualApprovalOrRejectJob);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void IncreaseBase(int process)
        {
            ReportManager.IncreaseBase(process);
        }

        public static void Increase()
        {
            ReportManager.Increase();
        }


        public static void AddSucceedJobDetail(Record record, int status, string action, string[] reviewers)
        {
            HasSucceed = true;
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

        public static void AddSkippedJobDetail(Record record, int status, string action, string[] reviewers,string comment)
        {
            HasSucceed = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                Action = action,
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = GetReviewers(reviewers),
                Status = JobDetailsStatus.Skipped,
                Comment = comment,
            });
        }

        public static void BetchAddSkippedJobDetail(List<ManualApprovalRecord> records, int status, string action, string comment)
        {
            HasSucceed = true;
            foreach (var record in records)
            {
                ReportManager.SendJobDetail(new JMManualApprovalJobDetails
                {
                    TitleOrName = record.LeafName,
                    Url = record.ManualFullPath,
                    ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                    ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                    Action = action,
                    RuleCriteria = record?.ManualRuleCriteria,
                    RecordOwner = GetReviewers(GetReviewers(record.ManualReviewer)),
                    Status = JobDetailsStatus.Skipped,
                    Comment = comment,
                });
            }
        }

        public static void AddFailedJobDetail(Record record, int status, string[] reviewers, string action, string comment)
        {
            HasFailed = true;
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

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }

        public static void SetJobStopped(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Stopped,comment);
        }


        public static void SetJobFinished()
        {
            var status = JobStatus.Finished;
            if (HasFailed && HasSucceed)
            {
                status = JobStatus.FinishWithException;
            }
            else if (HasFailed)
            {
                status = JobStatus.Failed;
            }

            ReportManager.SetJobFinished(status);

        }

        public static void SetJobFinished(string comment)
        {
            var status = JobStatus.Finished;
            if (HasFailed && HasSucceed)
            {
                status = JobStatus.FinishWithException;
            }
            else if (HasFailed)
            {
                status = JobStatus.Failed;
            }

            ReportManager.SetJobFinished(status, comment);
        }

        private static string[] GetReviewers(int[] reviewerIds)
        {
            var reviewerNames = Array.Empty<string>();
            try
            {
                reviewerNames = ManualApprovalOwnerManager.GetOwnerDisplayNames(reviewerIds).ToArray();
                return reviewerNames;
            }
            catch (Exception e)
            {
                _logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
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
