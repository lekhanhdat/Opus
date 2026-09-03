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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using RAManualApproval.BulkAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ImportAction
{
    public class ImportUnderReviewDatasManager
    {
       // private static readonly RALogger logger = RALogger.GetInstance(typeof(ImportUnderReviewDatasManager));

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
                { RMNodeLevel.BoxFile, "RM_JS_Rule_ObjectLevel_BoxFile" },
                { RMNodeLevel.GoogleFile, "RM_JS_Rule_ObjectLevel_GoogleFile" },
                { RMNodeLevel.CustomizeConnectorItem, "RM_Connector_ItemLevel_Item" }
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
        public static string JobComment { get; set; }

        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualImportUnderReviewDatasJob);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void AddSucceedJobDetail(Record record, int status, string[] reviewers)
        {
            HasSucceed = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = string.Join(";", reviewers),
                Status = JobDetailsStatus.Successful,
            });
        }

        public static void AddFailedJobDetail(Record record, int status, string[] reviewers, string comment)
        {
            HasFailed = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = string.Join(";", reviewers),
                Status = JobDetailsStatus.Failed,
                Comment = comment,
            });
        }

        public static void AddSkippedJobDetail(ManualApprovalRecord record, int status, string[] reviewers, string comment)
        {
            HasSucceed = true;
            ReportManager.SendJobDetail(new JMManualApprovalJobDetails
            {
                TitleOrName = record.LeafName,
                Url = record.ManualFullPath,
                ObjectLevel = ReportNodeLevelI18ns.TryGetValue((RMNodeLevel)record.NodeType, out var objectLvel) ? objectLvel : "",
                ApprovalStatus = ApprovalStatusI18ns.TryGetValue((SOApproveDBStatus)status, out var approvalStatus) ? approvalStatus : "",
                RuleCriteria = record?.ManualRuleCriteria,
                RecordOwner = string.Join(";", reviewers),
                Status = JobDetailsStatus.Skipped,
                Comment = comment,
            });
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
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

            ReportManager.SetJobFinished(status, JobComment);

        }
    }
}
