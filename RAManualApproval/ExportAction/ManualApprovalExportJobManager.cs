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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.ExportAction
{
    public class ManualApprovalExportJobManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalJobManager));

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();
        public static bool HasSucceedDetail { get; set; }

        public static bool HasFailedDetail { get; set; }

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
                        { RMNodeLevel.CustomizeConnectorItem, "RM_Connector_ItemLevel_Item" }
            };

        public static string JobComment { get; set; }

        public static void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType); 
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void IncreaseBase(long count)
        {
            ReportManager.IncreaseBase(count);
        }

        public static void Increase()
        {
            ReportManager.Increase();
        }

        public static void AddSucceedJobDetail(ManualApprovalRecord item, ManualApprovalAction action)
        {
            var ownerDisplayNames = "";
            try
            {
                var ownerDisplayNameArray = ManualApprovalOwnerManager.GetOwnerDisplayNames(item.ManualReviewer);
                ownerDisplayNames = string.Join(";", ownerDisplayNameArray);
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
                Status = JobDetailsStatus.Successful
            };
            ReportManager.SendJobDetail(detail);
            HasSucceedDetail = true;
        }

        public static void AddFailedJobDetail(ManualApprovalRecord item, ManualApprovalAction action, string comment)
        {
            var ownerDisplayNames = "";
            try
            {
                var ownerDisplayNameArray = ManualApprovalOwnerManager.GetOwnerDisplayNames(item.ManualReviewer);
                ownerDisplayNames = string.Join(";", ownerDisplayNameArray);
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
            ReportManager.SetJobFinished(jobFinishStatus, JobComment);
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }


        public static string GetI18NOfSourceFlag(SourceFlag sourceFlag, Dictionary<int, string> ContentSourceInfoes)
        {
            if (ContentSourceInfoes.ContainsKey((int)sourceFlag))
            {
                return ContentSourceInfoes[(int)sourceFlag];
            }
            return I18NEntity.GetString("RM_CP_Connector");
        }
        public static  async Task<string> GetUserDisplayNameAsync(int userIntId, Dictionary<int, string> UserDisplayNameCache)
        {
            if (userIntId <= 0)
            {
                return "";
            }

            if (!UserDisplayNameCache.ContainsKey(userIntId))
            {
                var user = await AccountDao.GetUserByIdAsync(userIntId);
                UserDisplayNameCache[userIntId] = user.DisplayName;
            }

            return UserDisplayNameCache[userIntId];
        }
        public static  string[] GetReviewers(int[] reviewerIds)
        {
            var reviewerNames = Array.Empty<string>();
            try
            {
                reviewerNames = ManualApprovalOwnerManager.GetOwnerDisplayNames(reviewerIds).ToArray();
                return reviewerNames;
            }
            catch (Exception e)
            {
                Logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
        }
        public static  async Task<string> GetApprovedByUser(int approvedById)
        {
            if (approvedById <= 0)
            {
                return "";
            }
            var user = await AccountDao.GetUserByIdAsync(approvedById);
            return user.DisplayName;
        }


    }
}
